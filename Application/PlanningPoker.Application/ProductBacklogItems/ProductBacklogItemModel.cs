using AutoMapper;

namespace PlanningPoker.Application.ProductBacklogItems
{
    public class ProductBacklogItemModel
    {
        public int Id { get; set; }

        public int PlanningRoomId { get; set; }

        public string Description { get; set; }

        public int StatusId { get; set; }
    }
}
