﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application.Configurations;
using System;
using System.Collections.Generic;
#nullable disable

namespace PlanningPoker.Application;

public partial class PlanningPokerContext : DbContext
{
    public PlanningPokerContext(DbContextOptions<PlanningPokerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }

    public virtual DbSet<EstimateValue> EstimateValue { get; set; }

    public virtual DbSet<EstimateValueCategory> EstimateValueCategory { get; set; }

    public virtual DbSet<PlanningRoom> PlanningRoom { get; set; }

    public virtual DbSet<PlanningRoomUsers> PlanningRoomUsers { get; set; }

    public virtual DbSet<ProductBacklogItem> ProductBacklogItem { get; set; }

    public virtual DbSet<ProductBacklogItemEstimate> ProductBacklogItemEstimate { get; set; }

    public virtual DbSet<ProductBacklogItemStatus> ProductBacklogItemStatus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            modelBuilder.ApplyConfiguration(new Configurations.AspNetUsersConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.EstimateValueConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.EstimateValueCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.PlanningRoomConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.PlanningRoomUsersConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.ProductBacklogItemConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.ProductBacklogItemEstimateConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.ProductBacklogItemStatusConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}