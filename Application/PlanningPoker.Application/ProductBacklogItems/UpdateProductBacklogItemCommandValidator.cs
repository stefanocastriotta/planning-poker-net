using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Application.ProductBacklogItems
{
    public class UpdateProductBacklogItemCommandValidator : AbstractValidator<UpdateProductBacklogItemCommand>
    {
        public UpdateProductBacklogItemCommandValidator()
        {
            RuleFor(command => command.StatusId).NotEmpty();
        }
    }
}
