using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using FluentResults;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application;
using PlanningPoker.Application.ProductBacklogItems;
using PlanningPoker.Web.SignalrHub;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlanningPoker.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductBacklogItemController : ControllerBase
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;
        readonly IHubContext<PlanningRoomHub, IPlanningRoomHub> _hubContext;
        readonly ConnectionManager _connectionManager;
        readonly ProductBacklogItemCommandHandler _productBacklogItemCommandHandler;

        public ProductBacklogItemController(PlanningPokerContext planningPokerContext, IMapper mapper, IHubContext<PlanningRoomHub, IPlanningRoomHub> hubContext, ConnectionManager connectionManager, ProductBacklogItemCommandHandler productBacklogItemCommandHandler)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _productBacklogItemCommandHandler = productBacklogItemCommandHandler;
        }

        [HttpPost]
        public async Task<ActionResult<ProductBacklogItemDto>> AddNewProductBacklogItem([FromBody] CreateProductBacklogItemCommand createProductBacklogItemCommand, CancellationToken cancellationToken)
        {
            var result = await _productBacklogItemCommandHandler.CreateProductBacklogItemAsync(createProductBacklogItemCommand, cancellationToken);
            if (result.IsFailed)
            {
                if (result.HasNotFoundErrorMetadata())
                    return NotFound(result.Errors.Select(e => e.ToString()));
                return BadRequest(result.Errors.Select(e => e.ToString()));
            }

            var dto = _mapper.Map<ProductBacklogItemDto>(result.Value);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + createProductBacklogItemCommand.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemInserted(dto);

            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<List<ProductBacklogItemDto>>> UpdateProductBacklogItem(int id, [FromBody] UpdateProductBacklogItemCommand updateProductBacklogItemCommand, CancellationToken cancellationToken)
        {
            var result = await _productBacklogItemCommandHandler.UpdateProductBacklogItemAsync(id, updateProductBacklogItemCommand, cancellationToken);

            if (result.IsFailed)
            {
                if (result.HasNotFoundErrorMetadata())
                    return NotFound(result.Errors.Select(e => e.ToString()));
                return BadRequest(result.Errors.Select(e => e.ToString()));
            }

            var productBacklogItemList = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.Status)
                .Include(p => p.ProductBacklogItemEstimate)
                .Where(p => p.PlanningRoomId == result.Value.PlanningRoomId)
                .ToListAsync(cancellationToken);

            var dtoList = _mapper.Map<List<ProductBacklogItemDto>>(productBacklogItemList);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + result.Value.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemUpdated(id, dtoList);

            return dtoList;
        }

        // DELETE api/<ProductBacklogItemController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var entity = new ProductBacklogItem { Id = id };
            _planningPokerContext.ProductBacklogItem.Remove(entity);
            await _planningPokerContext.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
    }
}
