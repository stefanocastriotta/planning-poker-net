﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanningPoker.Application;
using System;
using System.Collections.Generic;

namespace PlanningPoker.Application.Configurations
{
    public partial class EstimateValueCategoryConfiguration : IEntityTypeConfiguration<EstimateValueCategory>
    {
        public void Configure(EntityTypeBuilder<EstimateValueCategory> entity)
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0752DE1A23");

            entity.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(50);

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<EstimateValueCategory> entity);
    }
}
