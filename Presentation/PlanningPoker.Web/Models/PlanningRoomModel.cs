using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Web.Models
{
    public class PlanningRoomModel
    {
        public int Id { get; set; }

        [Required]
        public string? Description { get; set; }
        public string? CreationUserId { get; set; }
        [Required]
        public int EstimateValueCategoryId { get; set; }
        public string? NewEstimateValueCategory { get; set; }
        public string? NewEstimateValueCategoryValues { get; set; }
        public EstimateValueCategoryModel? EstimateValueCategory { get; set; }
        public List<PlanningRoomUserModel>? PlanningRoomUsers { get; set; }
        public List<ProductBacklogItemModel>? ProductBacklogItem { get; set; }
    }
}
