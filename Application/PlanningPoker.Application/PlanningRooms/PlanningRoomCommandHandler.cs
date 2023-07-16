using AutoMapper;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application.ProductBacklogItems;

namespace PlanningPoker.Application.PlanningRooms
{
    public class PlanningRoomCommandHandler : IHandler
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;
        readonly CreatePlanningRoomCommandValidator _createPlanningRoomCommandValidator;
        readonly RegisterPlanningRoomUserCommandValidator _registerPlanningRoomUserCommandValidator;

        public PlanningRoomCommandHandler(PlanningPokerContext planningPokerContext, IMapper mapper, CreatePlanningRoomCommandValidator createPlanningRoomCommandValidator, RegisterPlanningRoomUserCommandValidator registerPlanningRoomUserCommandValidator)
        {
            _mapper = mapper;
            _planningPokerContext = planningPokerContext;
            _createPlanningRoomCommandValidator = createPlanningRoomCommandValidator;
            _registerPlanningRoomUserCommandValidator = registerPlanningRoomUserCommandValidator;
        }

        public async Task<Result<PlanningRoom>> CreatePlanningRoomAsync(CreatePlanningRoomCommand command, CancellationToken cancellationToken)
        {
            var validationResult = _createPlanningRoomCommandValidator.Validate(command);
            if (!validationResult.IsValid)
            {
                return Result.Fail(validationResult.Errors.Select(v => v.CreateErrorFromValidationResult()));
            }

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
                await _planningPokerContext.AddAsync(newCategory, cancellationToken);
                await _planningPokerContext.SaveChangesAsync(cancellationToken);
                command.EstimateValueCategoryId = newCategory.Id;
            }

            var result = await _planningPokerContext.PlanningRoom.AddAsync(_mapper.Map<PlanningRoom>(command), cancellationToken);
            await _planningPokerContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }

        public async Task<Result<RegisterPlanningRoomUserResponse>> RegisterPlanningRoomUserAsync(RegisterPlanningRoomUserCommand command, CancellationToken cancellationToken)
        {
            var validationResult = _registerPlanningRoomUserCommandValidator.Validate(command);
            if (!validationResult.IsValid)
            {
                return Result.Fail(validationResult.Errors.Select(v => v.CreateErrorFromValidationResult()));
            }

            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == command.PlanningRoomId, cancellationToken);
            if (!existing)
            {
                return Result.Fail(new Error($"Planning room {command.PlanningRoomId} not found").WithNotFoundErrorMetadata());
            }

            var result = await _planningPokerContext.PlanningRoomUsers.Include(p => p.User).Where(u => u.PlanningRoomId == command.PlanningRoomId && u.UserId == command.UserId).SingleOrDefaultAsync(cancellationToken);
            if (result == null)
            {
                var newUser = new PlanningRoomUsers() { PlanningRoomId = command.PlanningRoomId, UserId = command.UserId };
                await _planningPokerContext.PlanningRoomUsers.AddAsync(newUser, cancellationToken);
                await _planningPokerContext.SaveChangesAsync(cancellationToken);

                return Result.Ok(new RegisterPlanningRoomUserResponse(await _planningPokerContext.PlanningRoomUsers.Include(p => p.User).SingleAsync(p => p.UserId == command.UserId && p.PlanningRoomId == command.PlanningRoomId, cancellationToken), true));
            }

            return Result.Ok(new RegisterPlanningRoomUserResponse(result, false));
        }
    }
}
