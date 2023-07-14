using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application.ProductBacklogItems;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PlanningPoker.Application
{
    public class ProductBacklogItemCommandHandler : IHandler
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;

        public ProductBacklogItemCommandHandler(PlanningPokerContext planningPokerContext, IMapper mapper)
        {
            _mapper = mapper;
            _planningPokerContext = planningPokerContext;
        }

        public async Task<Result> UpdateProductBacklogItemAsync(int productBacklogItemId, ProductBacklogItemModel productBacklogItem)
        {
            var existing = await _planningPokerContext.ProductBacklogItem.SingleAsync(p => p.Id == productBacklogItemId);
            if (existing == null)
            {
                return Result.Fail(new Error($"Product Backlog Item {productBacklogItemId} not found").WithMetadata("ErrorCode", 404));
            }
            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync();
            if (productBacklogItem.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
            {
                _planningPokerContext.ProductBacklogItem
                    .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId && p.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
                    .ExecuteUpdate(u => u.SetProperty(p => p.StatusId, p => (int)ProductBaclogItemStatusEnum.Inserted));
            }

            _mapper.Map(productBacklogItem, existing);

            _planningPokerContext.ProductBacklogItem.Update(existing);
            await _planningPokerContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return Result.Ok();
        }

        public async Task<Result<RegisterProductBacklogItemEstimateResponse?>> RegisterProductBacklogItemEstimateAsync(RegisterProductBacklogItemEstimateCommand command)
        {
            var existingProductBacklogItem = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.ProductBacklogItemEstimate)
                .Include(p => p.PlanningRoom.PlanningRoomUsers)
                .SingleOrDefaultAsync(p => p.Id == command.ProductBacklogItemId);
            if (existingProductBacklogItem == null)
            {
                return Result.Fail(new Error($"Product Backlog Item {command.ProductBacklogItemId} not found").WithMetadata("ErrorCode", 404));
            }

            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync();

            var newEstimate = _mapper.Map<ProductBacklogItemEstimate>(command);

            existingProductBacklogItem.ProductBacklogItemEstimate.Add(newEstimate);
            if (existingProductBacklogItem.PlanningRoom.PlanningRoomUsers.All(u => existingProductBacklogItem.ProductBacklogItemEstimate.Any(p => p.UserId == u.UserId)))
            {
                existingProductBacklogItem.StatusId = (int)ProductBaclogItemStatusEnum.Completed;
            }

            _planningPokerContext.Update(existingProductBacklogItem);

            int res = await _planningPokerContext.SaveChangesAsync();
            await transaction.CommitAsync();

            existingProductBacklogItem.Status = _planningPokerContext.ProductBacklogItemStatus.Single(s => s.Id == existingProductBacklogItem.StatusId);

            return new RegisterProductBacklogItemEstimateResponse(newEstimate);
        }
    }
}
