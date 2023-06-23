using PlanningPoker.Domain;
using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Web.Models
{
    public class ProductBacklogItemEstimateDto
    {
        public int Id { get; set; }

        public int ProductBacklogItemId { get; set; }

        public string UserId { get; set; }

        public int EstimateValueId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
