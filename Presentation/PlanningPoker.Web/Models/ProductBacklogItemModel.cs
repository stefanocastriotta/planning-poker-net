﻿using PlanningPoker.Domain;
using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Web.Models
{
    public class ProductBacklogItemModel
    {
        public int Id { get; set; }

        public int PlanningRoomId { get; set; }

        public string Description { get; set; }

        public int StatusId { get; set; }
    }
}
