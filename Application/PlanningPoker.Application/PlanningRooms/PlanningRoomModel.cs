using CommandQuery;
using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Application.PlanningRooms
{
    public class PlanningRoomModel : ICommand<PlanningRoom>
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }
        public string? CreationUserId { get; set; }
        [Required]
        public int EstimateValueCategoryId { get; set; }
        public string? NewEstimateValueCategory { get; set; }
        public string? NewEstimateValueCategoryValues { get; set; }
    }
}
