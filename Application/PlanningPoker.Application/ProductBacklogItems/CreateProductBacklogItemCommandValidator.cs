using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Application.ProductBacklogItems
{
    public class CreateProductBacklogItemCommandValidator : AbstractValidator<CreateProductBacklogItemCommand>
    {
        public CreateProductBacklogItemCommandValidator()
        {
            RuleFor(command => command.PlanningRoomId).NotEmpty();
            RuleFor(command => command.Description).NotEmpty();
        }
    }
}
