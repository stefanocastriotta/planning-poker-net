﻿using PlanningPoker.Web.Models;

namespace PlanningPoker.Web.SignalrHub
{
    public interface IPlanningRoomHub
    {
        Task UserJoined(PlanningRoomUserDto planningRoomUserDto);
        Task ProductBacklogItemInserted(ProductBacklogItemDto productBacklogItem);
        Task ProductBacklogItemUpdated(int updatedId, List<ProductBacklogItemDto> productBacklogItems);
        Task ProductBacklogItemEstimated(ProductBacklogItemEstimateDto productBacklogItemEstimateDto);
    }
}
