namespace PlanningPoker.Application.ProductBacklogItems
{
    public class ProductBacklogItemDto
    {
        public int Id { get; set; }

        public int PlanningRoomId { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public int StatusId { get; set; }

        public List<ProductBacklogItemEstimateDto> ProductBacklogItemEstimate { get; set; }

        public ProductBacklogItemStatusDto Status { get; set; }
    }
}
