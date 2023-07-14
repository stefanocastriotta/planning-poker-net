using CommandQuery;
using FluentResults;

namespace PlanningPoker.Application.PlanningRooms
{
    public record RegisterPlanningRoomUserRequest(int PlanningRoomId, string UserId) : ICommand<Result<RegisterPlanningRoomUserResponse>>;
}
