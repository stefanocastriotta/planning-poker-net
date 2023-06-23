using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Web.Models
{
    public class ProductBacklogItemStatusDto
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public int Order { get; set; }

        public bool? IsActive { get; set; }
    }
}
