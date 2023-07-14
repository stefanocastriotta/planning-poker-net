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
        public async Task<ActionResult<ProductBacklogItemDto>> AddNewProductBacklogItem([FromBody] ProductBacklogItemModel productBacklogItemModel)
        {
            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == productBacklogItemModel.PlanningRoomId);
            if (!existing)
            {
                return NotFound();
            }

            var productBacklogItem = _mapper.Map<ProductBacklogItem>(productBacklogItemModel);
            var result = await _planningPokerContext.ProductBacklogItem.AddAsync(productBacklogItem);
            await _planningPokerContext.SaveChangesAsync();
            productBacklogItem.Status = _planningPokerContext.ProductBacklogItemStatus.Single(s => s.Id == productBacklogItem.StatusId);

            var dto = _mapper.Map<ProductBacklogItemDto>(result.Entity);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + productBacklogItem.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemInserted(dto);

            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<List<ProductBacklogItemDto>>> UpdateProductBacklogItem(int id, [FromBody] ProductBacklogItemModel productBacklogItem)
        {
            var result = await _productBacklogItemCommandHandler.UpdateProductBacklogItemAsync(id, productBacklogItem);

            if (result.IsFailed)
            {
                if (result.HasError(e => e.HasMetadata("ErrorCode", m => m.Equals(404))))
                    return NotFound(result);
                return BadRequest(result);
            }

            var productNacklogItemList = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.Status)
                .Include(p => p.ProductBacklogItemEstimate)
                .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId)
                .ToListAsync();

            var dtoList = _mapper.Map<List<ProductBacklogItemDto>>(productNacklogItemList);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + productBacklogItem.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemUpdated(id, dtoList);

            return dtoList;
        }

        // DELETE api/<ProductBacklogItemController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = new ProductBacklogItem { Id = id };
            _planningPokerContext.ProductBacklogItem.Remove(entity);
            await _planningPokerContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
