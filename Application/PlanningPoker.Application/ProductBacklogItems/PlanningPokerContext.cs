using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application.ProductBacklogItems;

namespace PlanningPoker.Application
{
    public partial class PlanningPokerContext
    {
        public async Task<ProductBacklogItem> RegisterProductBacklogItemAsync(IMapper mapper, ProductBacklogItemModel productBacklogItem)
        {
            var result = await ProductBacklogItem.Persist(mapper).InsertOrUpdateAsync(productBacklogItem);
            await SaveChangesAsync();
            result.Status = ProductBacklogItemStatus.Single(s => s.Id == result.StatusId);
            return result;
        }

        public async Task<ProductBacklogItem?> UpdateProductBacklogItemAsync(IMapper mapper, int id, ProductBacklogItemModel productBacklogItem)
        {
            var existing = await ProductBacklogItem.AnyAsync(p => p.Id == id);
            if (!existing)
            {
                return null;
            }
            using var transaction = await Database.BeginTransactionAsync();
            if (productBacklogItem.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
            {
                ProductBacklogItem
                    .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId && p.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
                    .ExecuteUpdate(u => u.SetProperty(p => p.StatusId, p => (int)ProductBaclogItemStatusEnum.Inserted));
            }

            var result = await ProductBacklogItem.Persist(mapper).InsertOrUpdateAsync(productBacklogItem);
            await SaveChangesAsync();

            await transaction.CommitAsync();

            return result;
        }

        public async Task<(ProductBacklogItemEstimate? newEstimate, ProductBacklogItem? updatedProductBacklogItem)> RegisterProductBacklogItemEstimateAsync(IMapper mapper, ProductBacklogItemEstimateModel productBacklogItemEstimateModel)
        {
            var existingProductBacklogItem = await ProductBacklogItem
                .Include(p => p.ProductBacklogItemEstimate)
                .Include(p => p.PlanningRoom.PlanningRoomUsers)
                .SingleOrDefaultAsync(p => p.Id == productBacklogItemEstimateModel.ProductBacklogItemId);
            if (existingProductBacklogItem == null)
            {
                return (null, null);
            }

            using var transaction = await Database.BeginTransactionAsync();

            var newEstimate = mapper.Map<ProductBacklogItemEstimate>(productBacklogItemEstimateModel);
            existingProductBacklogItem.ProductBacklogItemEstimate.Add(newEstimate);
            if (existingProductBacklogItem.PlanningRoom.PlanningRoomUsers.All(u => existingProductBacklogItem.ProductBacklogItemEstimate.Any(p => p.UserId == u.UserId)))
            {
                existingProductBacklogItem.StatusId = (int)ProductBaclogItemStatusEnum.Completed;
            }

            Update(existingProductBacklogItem);

            await SaveChangesAsync();
            await transaction.CommitAsync();

            existingProductBacklogItem.Status = ProductBacklogItemStatus.Single(s => s.Id == existingProductBacklogItem.StatusId);

            return (newEstimate, existingProductBacklogItem);
        }
    }
}
