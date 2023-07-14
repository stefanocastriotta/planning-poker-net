using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace PlanningPoker.Application.PlanningRooms
{
    public class PlanningRoomCommandHandler : IHandler
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;

        public PlanningRoomCommandHandler(PlanningPokerContext planningPokerContext, IMapper mapper)
        {
            _mapper = mapper;
            _planningPokerContext = planningPokerContext;
        }

        public async Task<PlanningRoom> CreatePlanningRoomAsync(CreatePlanningRoomCommand command, CancellationToken cancellationToken)
        {
            if (command.NewEstimateValueCategoryValues != null)
            {
                var categoryValues = command.NewEstimateValueCategoryValues.Split(",").ToList();
                EstimateValueCategory newCategory = new EstimateValueCategory();
                newCategory.Description = command.NewEstimateValueCategory;
                foreach (var category in categoryValues)
                {
                    newCategory.EstimateValue.Add(new EstimateValue
                    {
                        Label = category,
                        Value = int.TryParse(category, out int estimateValue) ? estimateValue : 0,
                        Order = categoryValues.IndexOf(category) + 1
                    });
                }
                await _planningPokerContext.AddAsync(newCategory);
                await _planningPokerContext.SaveChangesAsync();
                command.EstimateValueCategoryId = newCategory.Id;
            }

            var result = await _planningPokerContext.PlanningRoom.AddAsync(_mapper.Map<PlanningRoom>(command));
            await _planningPokerContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<Result<RegisterPlanningRoomUserResponse>> RegisterPlanningRoomUserAsync(RegisterPlanningRoomUserCommand command, CancellationToken cancellationToken)
        {
            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == command.PlanningRoomId);
            if (!existing)
            {
                return Result.Fail(new Error($"Planning room {command.PlanningRoomId} not found").WithMetadata("ErrorCode", 404));
            }

            var result = await _planningPokerContext.PlanningRoomUsers.Include(p => p.User).Where(u => u.PlanningRoomId == command.PlanningRoomId && u.UserId == command.UserId).SingleOrDefaultAsync();
            if (result == null)
            {
                var newUser = new PlanningRoomUsers() { PlanningRoomId = command.PlanningRoomId, UserId = command.UserId };
                await _planningPokerContext.PlanningRoomUsers.AddAsync(newUser);
                await _planningPokerContext.SaveChangesAsync();

                return Result.Ok(new RegisterPlanningRoomUserResponse(_planningPokerContext.PlanningRoomUsers.Include(p => p.User).Single(p => p.UserId == command.UserId && p.PlanningRoomId == command.PlanningRoomId), true));
            }

            return Result.Ok(new RegisterPlanningRoomUserResponse(result, false));
        }
    }
}
