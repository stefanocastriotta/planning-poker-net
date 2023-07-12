using Microsoft.Extensions.DependencyInjection;

namespace PlanningPoker.Application
{
    public static class ApplicationServices
    {
        public static void AddPlanningPokerApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper((serviceProvider, automapper) =>
            {
                automapper.AddMaps(typeof(ApplicationServices).Assembly);
            }, typeof(PlanningPokerContext).Assembly);
        }
    }
}
