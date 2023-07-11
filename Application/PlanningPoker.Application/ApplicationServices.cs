using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using AutoMapper.EquivalencyExpression;

namespace PlanningPoker.Application
{
    public static class ApplicationServices
    {
        public static void AddPlanningPokerApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper((serviceProvider, automapper) =>
            {
                automapper.AddCollectionMappers();
                automapper.UseEntityFrameworkCoreModel<PlanningPokerContext>(serviceProvider);
                automapper.AddMaps(typeof(ApplicationServices).Assembly);
            }, typeof(PlanningPokerContext).Assembly);
        }
    }
}
