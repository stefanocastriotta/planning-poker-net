using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Application.PlanningRooms
{
    public class CreatePlanningRoomCommand
    {
        public string Description { get; set; }
        public string? CreationUserId { get; set; }
        public int EstimateValueCategoryId { get; set; }
        public string? NewEstimateValueCategory { get; set; }
        public string? NewEstimateValueCategoryValues { get; set; }
    }
}
