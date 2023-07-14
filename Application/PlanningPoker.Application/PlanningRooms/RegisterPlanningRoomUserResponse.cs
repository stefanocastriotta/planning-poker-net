using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Application.PlanningRooms
{
    public record RegisterPlanningRoomUserResponse(PlanningRoomUsers PlanningRoomUsers, bool IsNew);
}
