using AutoMapper;

namespace PlanningPoker.Application.ProductBacklogItems
{
    public class CreateProductBacklogItemCommand
    {
        public int PlanningRoomId { get; set; }

        public string Description { get; set; }
    }
}
