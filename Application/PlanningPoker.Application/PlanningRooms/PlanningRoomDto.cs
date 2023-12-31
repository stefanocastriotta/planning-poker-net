﻿using PlanningPoker.Application.Estimates;
using PlanningPoker.Application.ProductBacklogItems;

namespace PlanningPoker.Application.PlanningRooms
{
    public class PlanningRoomDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? CreationUserId { get; set; }
        public int EstimateValueCategoryId { get; set; }
        public string? NewEstimateValueCategory { get; set; }
        public string? NewEstimateValueCategoryValues { get; set; }
        public EstimateValueCategoryDto EstimateValueCategory { get; set; }
        public List<PlanningRoomUserDto> PlanningRoomUsers { get; set; }
        public List<ProductBacklogItemDto> ProductBacklogItem { get; set; }
    }
}
