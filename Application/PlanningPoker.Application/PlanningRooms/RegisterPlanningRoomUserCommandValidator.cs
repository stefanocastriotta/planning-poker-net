using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace PlanningPoker.Application.PlanningRooms
{
    public class RegisterPlanningRoomUserCommandValidator : AbstractValidator<RegisterPlanningRoomUserCommand>
    {
        public RegisterPlanningRoomUserCommandValidator()
        {
            RuleFor(command => command.UserId).NotEmpty();
            RuleFor(command => command.PlanningRoomId).NotEmpty();
        }
    }
}
