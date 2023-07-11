using AutoMapper;

namespace PlanningPoker.Application.ProductBacklogItems
{
    internal class ProductBacklogItemAutomapperProfile : Profile
    {
        public ProductBacklogItemAutomapperProfile()
        {
            CreateMap<ProductBacklogItemModel, ProductBacklogItem>();
            CreateMap<ProductBacklogItem, ProductBacklogItemDto>();
            CreateMap<ProductBacklogItemEstimate, ProductBacklogItemEstimateDto>();
            CreateMap<ProductBacklogItemEstimateModel, ProductBacklogItemEstimate>();

            CreateMap<ProductBacklogItemStatus, ProductBacklogItemStatusDto>();
        }
    }
}
