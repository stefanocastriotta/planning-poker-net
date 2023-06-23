using PlanningPoker.Domain;

namespace PlanningPoker.Web.Models
{
    public class EstimateValueCategoryDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public virtual ICollection<EstimateValueDto> EstimateValue { get; set; }
    }
}
