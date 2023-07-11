using AutoMapper;

namespace PlanningPoker.Application.Estimates
{
    internal class EstimateValueAutomapperProfile : Profile
    {
        public EstimateValueAutomapperProfile() 
        {
            CreateMap<EstimateValue, EstimateValueDto>();
            CreateMap<EstimateValueCategory, EstimateValueCategoryDto>();
        }
    }
}
