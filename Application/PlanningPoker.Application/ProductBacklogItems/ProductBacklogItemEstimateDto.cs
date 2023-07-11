namespace PlanningPoker.Application.ProductBacklogItems
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
