using AutoMapper;
using CommandQuery;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Application.PlanningRooms
{
    public class PlanningRoomRequestHandler : ICommandHandler<PlanningRoomModel, PlanningRoom>, ICommandHandler<RegisterPlanningRoomUserRequest, Result<RegisterPlanningRoomUserResponse>>
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;

        public PlanningRoomRequestHandler(PlanningPokerContext planningPokerContext, IMapper mapper)
        {
            _mapper = mapper;
            _planningPokerContext = planningPokerContext;
        }

        public async Task<PlanningRoom> HandleAsync(PlanningRoomModel request, CancellationToken cancellationToken)
        {
            if (request.NewEstimateValueCategoryValues != null)
            {
                var categoryValues = request.NewEstimateValueCategoryValues.Split(",").ToList();
                EstimateValueCategory newCategory = new EstimateValueCategory();
                newCategory.Description = request.NewEstimateValueCategory;
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
                request.EstimateValueCategoryId = newCategory.Id;
            }

            var result = await _planningPokerContext.PlanningRoom.AddAsync(_mapper.Map<PlanningRoom>(request));
            await _planningPokerContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<Result<RegisterPlanningRoomUserResponse>> HandleAsync(RegisterPlanningRoomUserRequest request, CancellationToken cancellationToken)
        {
            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == request.PlanningRoomId);
            if (!existing)
            {
                return Result.Fail(new Error($"Planning room {request.PlanningRoomId} not found").WithMetadata("ErrorCode", 404));
            }

            var result = await _planningPokerContext.PlanningRoomUsers.Include(p => p.User).Where(u => u.PlanningRoomId == request.PlanningRoomId && u.UserId == request.UserId).SingleOrDefaultAsync();
            if (result == null)
            {
                var newUser = new PlanningRoomUsers() { PlanningRoomId = request.PlanningRoomId, UserId = request.UserId };
                await _planningPokerContext.PlanningRoomUsers.AddAsync(newUser);
                await _planningPokerContext.SaveChangesAsync();

                return Result.Ok(new RegisterPlanningRoomUserResponse(_planningPokerContext.PlanningRoomUsers.Include(p => p.User).Single(p => p.UserId == request.UserId && p.PlanningRoomId == request.PlanningRoomId), true));
            }

            return Result.Ok(new RegisterPlanningRoomUserResponse(result, false));
        }
    }
}
