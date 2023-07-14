namespace PlanningPoker.Application.ProductBacklogItems
{
    public class RegisterProductBacklogItemEstimateCommand
    {
        public int ProductBacklogItemId { get; set; }

        public int EstimateValueId { get; set; }

        public string? UserId { get; set; }
    }
}
