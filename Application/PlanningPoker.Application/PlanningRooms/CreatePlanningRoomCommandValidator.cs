using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace PlanningPoker.Application.PlanningRooms
{
    public class CreatePlanningRoomCommandValidator : AbstractValidator<CreatePlanningRoomCommand>
    {
        public CreatePlanningRoomCommandValidator()
        {
            RuleFor(command => command.Description).NotEmpty();
            RuleFor(command => command.CreationUserId).NotEmpty();
            When(command => command.EstimateValueCategoryId == -1, () =>
            {
                RuleFor(command => command.NewEstimateValueCategory).NotEmpty();
                RuleFor(command => command.NewEstimateValueCategoryValues).NotEmpty();
                RuleFor(command => command.NewEstimateValueCategoryValues).Must(value =>
                {
                    string[] values = value.Split(',');
                    return values.Length > 0 && values.All(v => int.TryParse(v, out int _));
                }).WithMessage("I valori della nuova categoria devono essere numeri validi separati da virgola (,)").When(command => command.NewEstimateValueCategoryValues != null);
            }).Otherwise(() => {
                RuleFor(command => command.EstimateValueCategoryId).NotEmpty();
            });
        }
    }
}
