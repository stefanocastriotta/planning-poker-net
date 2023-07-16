using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Application.ProductBacklogItems
{
    public class RegisterProductBacklogItemEstimateCommandValidator : AbstractValidator<RegisterProductBacklogItemEstimateCommand>
    {
        public RegisterProductBacklogItemEstimateCommandValidator()
        {
            RuleFor(command => command.EstimateValueId).NotEmpty();
            RuleFor(command => command.ProductBacklogItemId).NotEmpty();
            RuleFor(command => command.UserId).NotEmpty();
        }
    }
}
