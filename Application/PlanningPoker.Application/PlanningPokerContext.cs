using Microsoft.EntityFrameworkCore;

namespace PlanningPoker.Application
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
