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
        readonly CreateProductBacklogItemCommandValidator _createProductBacklogItemCommandValidator;
        readonly RegisterProductBacklogItemEstimateCommandValidator _registerProductBacklogItemEstimateCommandValidator;
        readonly UpdateProductBacklogItemCommandValidator _updateProductBacklogItemCommandValidator;

        public ProductBacklogItemCommandHandler(PlanningPokerContext planningPokerContext, IMapper mapper, 
            RegisterProductBacklogItemEstimateCommandValidator registerProductBacklogItemEstimateCommandValidator, UpdateProductBacklogItemCommandValidator updateProductBacklogItemCommandValidator, 
            CreateProductBacklogItemCommandValidator createProductBacklogItemCommandValidator)
        {
            _mapper = mapper;
            _planningPokerContext = planningPokerContext;
            _registerProductBacklogItemEstimateCommandValidator = registerProductBacklogItemEstimateCommandValidator;
            _updateProductBacklogItemCommandValidator = updateProductBacklogItemCommandValidator;
            _createProductBacklogItemCommandValidator = createProductBacklogItemCommandValidator;
        }

        public async Task<Result<ProductBacklogItem>> CreateProductBacklogItemAsync(CreateProductBacklogItemCommand createProductBacklogItemCommand, CancellationToken cancellationToken)
        {
            var validationResult = _createProductBacklogItemCommandValidator.Validate(createProductBacklogItemCommand);
            if (!validationResult.IsValid)
            {
                return Result.Fail(validationResult.Errors.Select(v => v.CreateErrorFromValidationResult()));
            }

            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == createProductBacklogItemCommand.PlanningRoomId, cancellationToken);
            if (!existing)
            {
                return Result.Fail(new Error($"Planning room {createProductBacklogItemCommand.PlanningRoomId} not found").WithMetadata(ErrorExtensions.ErrorCodeMetadata, ErrorExtensions.NotFoundErrorCode));
            }

            var productBacklogItem = _mapper.Map<ProductBacklogItem>(createProductBacklogItemCommand);
            productBacklogItem.Status = await _planningPokerContext.ProductBacklogItemStatus.SingleAsync(s => s.Id == (int)ProductBaclogItemStatusEnum.Inserted, cancellationToken);
            var result = await _planningPokerContext.ProductBacklogItem.AddAsync(productBacklogItem, cancellationToken);
            await _planningPokerContext.SaveChangesAsync(cancellationToken);
            return productBacklogItem;
        }

        public async Task<Result<ProductBacklogItem>> UpdateProductBacklogItemAsync(int productBacklogItemId, UpdateProductBacklogItemCommand productBacklogItem, CancellationToken cancellationToken)
        {
            var validationResult = _updateProductBacklogItemCommandValidator.Validate(productBacklogItem);
            if (!validationResult.IsValid)
            {
                return Result.Fail(validationResult.Errors.Select(v => v.CreateErrorFromValidationResult()));
            }

            var existing = await _planningPokerContext.ProductBacklogItem.SingleAsync(p => p.Id == productBacklogItemId, cancellationToken);
            if (existing == null)
            {
                return Result.Fail(new Error($"Product Backlog Item {productBacklogItemId} not found").WithMetadata(ErrorExtensions.ErrorCodeMetadata, ErrorExtensions.NotFoundErrorCode));
            }

            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync(cancellationToken);
            if (productBacklogItem.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
            {
                _planningPokerContext.ProductBacklogItem
                    .Where(p => p.PlanningRoomId == existing.PlanningRoomId && p.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
                    .ExecuteUpdate(u => u.SetProperty(p => p.StatusId, p => (int)ProductBaclogItemStatusEnum.Inserted));
            }

            _mapper.Map(productBacklogItem, existing);

            _planningPokerContext.ProductBacklogItem.Update(existing);
            await _planningPokerContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return existing;
        }

        public async Task<Result<RegisterProductBacklogItemEstimateResponse?>> RegisterProductBacklogItemEstimateAsync(RegisterProductBacklogItemEstimateCommand command, CancellationToken cancellationToken)
        {
            var validationResult = _registerProductBacklogItemEstimateCommandValidator.Validate(command);
            if (!validationResult.IsValid)
            {
                return Result.Fail(validationResult.Errors.Select(v => v.CreateErrorFromValidationResult()));
            }

            var existingProductBacklogItem = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.ProductBacklogItemEstimate)
                .Include(p => p.PlanningRoom.PlanningRoomUsers)
                .SingleOrDefaultAsync(p => p.Id == command.ProductBacklogItemId, cancellationToken);
            if (existingProductBacklogItem == null)
            {
                return Result.Fail(new Error($"Product Backlog Item {command.ProductBacklogItemId} not found").WithMetadata(ErrorExtensions.ErrorCodeMetadata, ErrorExtensions.NotFoundErrorCode));
            }

            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync(cancellationToken);

            var newEstimate = _mapper.Map<ProductBacklogItemEstimate>(command);

            existingProductBacklogItem.ProductBacklogItemEstimate.Add(newEstimate);
            if (existingProductBacklogItem.PlanningRoom.PlanningRoomUsers.All(u => existingProductBacklogItem.ProductBacklogItemEstimate.Any(p => p.UserId == u.UserId)))
            {
                existingProductBacklogItem.StatusId = (int)ProductBaclogItemStatusEnum.Completed;
            }

            _planningPokerContext.Update(existingProductBacklogItem);

            int res = await _planningPokerContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            existingProductBacklogItem.Status = await _planningPokerContext.ProductBacklogItemStatus.SingleAsync(s => s.Id == existingProductBacklogItem.StatusId, cancellationToken);

            return new RegisterProductBacklogItemEstimateResponse(newEstimate);
        }
    }
}
