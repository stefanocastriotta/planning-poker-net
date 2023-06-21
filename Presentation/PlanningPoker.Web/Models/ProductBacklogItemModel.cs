using PlanningPoker.Domain;
using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Web.Models
{
    public class ProductBacklogItemModel
    {
        public int Id { get; set; }

        public int PlanningRoomId { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public int StatusId { get; set; }

        //public virtual ICollection<ProductBacklogItemEstimate> ProductBacklogItemEstimate { get; set; } = new List<ProductBacklogItemEstimate>();

        public virtual ProductBacklogItemStatusModel? Status { get; set; }
    }
}
