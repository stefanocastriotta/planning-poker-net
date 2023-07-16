using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PlanningPoker.Application.PlanningRooms;

namespace PlanningPoker.Application
{
    public static class ApplicationServices
    {
        public static void AddPlanningPokerApplicationServices(this IServiceCollection services)
        {
            typeof(PlanningRoomCommandHandler).Assembly
            .GetTypes()
            .Where(type => typeof(IHandler).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            .ToList()
            .ForEach(typeToRegister =>
            {
                services.AddScoped(typeToRegister);
            });
            services.AddValidatorsFromAssemblyContaining<CreatePlanningRoomCommandValidator>();
            services.AddAutoMapper((serviceProvider, automapper) =>
            {
                automapper.AddMaps(typeof(ApplicationServices).Assembly);
            }, typeof(PlanningPokerContext).Assembly);
        }
    }
}
