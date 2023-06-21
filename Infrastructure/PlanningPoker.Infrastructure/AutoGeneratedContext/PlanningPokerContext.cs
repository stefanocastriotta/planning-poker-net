﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Domain;

namespace PlanningPoker.Infrastructure;

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
        modelBuilder.Entity<AspNetUsers>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<EstimateValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC07E7E2A322");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.EstimateValue)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EstimateValue_ToTable");
        });

        modelBuilder.Entity<EstimateValueCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0752DE1A23");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<PlanningRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC07AC9288F0");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreationUserId)
                .IsRequired()
                .HasMaxLength(450);
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.CreationUser).WithMany(p => p.PlanningRoom)
                .HasForeignKey(d => d.CreationUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanningRoom_ToTable");

            entity.HasOne(d => d.EstimateValueCategory).WithMany(p => p.PlanningRoom)
                .HasForeignKey(d => d.EstimateValueCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanningRoom_ToTable_1");
        });

        modelBuilder.Entity<PlanningRoomUsers>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PlanningRoomId }).HasName("PK__Planning__79102391E89C42C3");

            entity.HasOne(d => d.PlanningRoom).WithMany(p => p.PlanningRoomUsers)
                .HasForeignKey(d => d.PlanningRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanningRoomUsers_ToTable");

            entity.HasOne(d => d.User).WithMany(p => p.PlanningRoomUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanningRoomUsers_ToTable_1");
        });

        modelBuilder.Entity<ProductBacklogItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductB__3214EC075FA54AE4");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(250);

            entity.HasOne(d => d.PlanningRoom).WithMany(p => p.ProductBacklogItem)
                .HasForeignKey(d => d.PlanningRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductBacklogItem_ToTable_1");

            entity.HasOne(d => d.Status).WithMany(p => p.ProductBacklogItem)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductBacklogItem_ToTable");
        });

        modelBuilder.Entity<ProductBacklogItemEstimate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductB__3214EC07F2367097");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.HasOne(d => d.EstimateValue).WithMany(p => p.ProductBacklogItemEstimate)
                .HasForeignKey(d => d.EstimateValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Table_ToTable_2");

            entity.HasOne(d => d.ProductBacklogItem).WithMany(p => p.ProductBacklogItemEstimate)
                .HasForeignKey(d => d.ProductBacklogItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Table_ToTable");

            entity.HasOne(d => d.User).WithMany(p => p.ProductBacklogItemEstimate)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Table_ToTable_1");
        });

        modelBuilder.Entity<ProductBacklogItemStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductB__3214EC07354F0823");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))")
                .HasColumnName("Is_Active");
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}