using PlanningPoker.Domain;

namespace PlanningPoker.Web.Models
{
    public class EstimateValueCategoryModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public virtual ICollection<EstimateValueModel>? EstimateValue { get; set; }
    }
}
