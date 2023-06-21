using Microsoft.EntityFrameworkCore;
using PlanningPoker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Infrastructure
{
    public partial class PlanningPokerContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlanningRoom>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ProductBacklogItem>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .ValueGeneratedOnAdd();
            });
        }
    }
}
