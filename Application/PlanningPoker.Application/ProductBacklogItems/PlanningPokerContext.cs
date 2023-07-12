using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application.ProductBacklogItems;

namespace PlanningPoker.Application
{
    public partial class PlanningPokerContext
    {
        public async Task<int> UpdateProductBacklogItemAsync(ProductBacklogItem productBacklogItem)
        {
            var existing = await ProductBacklogItem.AnyAsync(p => p.Id == productBacklogItem.Id);
            if (!existing)
            {
                return 0;
            }
            using var transaction = await Database.BeginTransactionAsync();
            if (productBacklogItem.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
            {
                ProductBacklogItem
                    .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId && p.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
                    .ExecuteUpdate(u => u.SetProperty(p => p.StatusId, p => (int)ProductBaclogItemStatusEnum.Inserted));
            }

            ProductBacklogItem.Update(productBacklogItem);
            int res = await SaveChangesAsync();

            await transaction.CommitAsync();

            return res;
        }

        public async Task<ProductBacklogItem?> RegisterProductBacklogItemEstimateAsync(ProductBacklogItemEstimate productBacklogItemEstimate)
        {
            var existingProductBacklogItem = await ProductBacklogItem
                .Include(p => p.ProductBacklogItemEstimate)
                .Include(p => p.PlanningRoom.PlanningRoomUsers)
                .SingleOrDefaultAsync(p => p.Id == productBacklogItemEstimate.ProductBacklogItemId);
            if (existingProductBacklogItem == null)
            {
                return null;
            }

            using var transaction = await Database.BeginTransactionAsync();

            existingProductBacklogItem.ProductBacklogItemEstimate.Add(productBacklogItemEstimate);
            if (existingProductBacklogItem.PlanningRoom.PlanningRoomUsers.All(u => existingProductBacklogItem.ProductBacklogItemEstimate.Any(p => p.UserId == u.UserId)))
            {
                existingProductBacklogItem.StatusId = (int)ProductBaclogItemStatusEnum.Completed;
            }

            Update(existingProductBacklogItem);

            int res = await SaveChangesAsync();
            await transaction.CommitAsync();

            existingProductBacklogItem.Status = ProductBacklogItemStatus.Single(s => s.Id == existingProductBacklogItem.StatusId);

            return existingProductBacklogItem;
        }
    }
}
