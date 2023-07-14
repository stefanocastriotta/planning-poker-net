using AutoMapper.Internal;
using CommandQuery;
using Microsoft.Extensions.DependencyInjection;
using PlanningPoker.Application.PlanningRooms;

namespace PlanningPoker.Application
{
    public static class ApplicationServices
    {
        public static void AddPlanningPokerApplicationServices(this IServiceCollection services)
        {
            typeof(PlanningRoomRequestHandler).Assembly
            .GetTypes()
            .Where(a => (a.GetGenericInterface(typeof(ICommandHandler<>)) != null || a.GetGenericInterface(typeof(ICommandHandler<,>)) != null) && !a.IsAbstract && !a.IsInterface)
            .ToList()
            .ForEach(typeToRegister =>
            {
                services.AddScoped(typeToRegister);
            });
            services.AddAutoMapper((serviceProvider, automapper) =>
            {
                automapper.AddMaps(typeof(ApplicationServices).Assembly);
            }, typeof(PlanningPokerContext).Assembly);
        }
    }
}
