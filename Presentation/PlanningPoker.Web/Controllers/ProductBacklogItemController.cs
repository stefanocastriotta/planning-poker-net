using AutoMapper;
using AutoMapper.EntityFrameworkCore;
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


        public ProductBacklogItemController(PlanningPokerContext planningPokerContext, IMapper mapper, IHubContext<PlanningRoomHub, IPlanningRoomHub> hubContext, ConnectionManager connectionManager)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
        }

        [HttpPost]
        public async Task<ActionResult<ProductBacklogItemDto>> Post([FromBody] ProductBacklogItemModel productBacklogItem)
        {
            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == productBacklogItem.PlanningRoomId);
            if (!existing)
            {
                return NotFound();
            }

            var result = await _planningPokerContext.ProductBacklogItem.Persist(_mapper).InsertOrUpdateAsync(productBacklogItem);
            await _planningPokerContext.SaveChangesAsync();
            result.Status = _planningPokerContext.ProductBacklogItemStatus.Single(s => s.Id == result.StatusId);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dto = _mapper.Map<ProductBacklogItemDto>(result);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + productBacklogItem.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemInserted(dto);

            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<List<ProductBacklogItemDto>>> Put(int id, [FromBody] ProductBacklogItemModel productBacklogItem)
        {
            var existing = await _planningPokerContext.ProductBacklogItem.SingleOrDefaultAsync(p => p.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync();
            if (productBacklogItem.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
            {
                _planningPokerContext.ProductBacklogItem
                    .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId && p.StatusId == (int)ProductBaclogItemStatusEnum.Processing)
                    .ExecuteUpdate(u => u.SetProperty(p => p.StatusId, p => (int)ProductBaclogItemStatusEnum.Inserted));
            }

            await _planningPokerContext.ProductBacklogItem.Persist(_mapper).InsertOrUpdateAsync(productBacklogItem);
            await _planningPokerContext.SaveChangesAsync();
            
            await transaction.CommitAsync();

            var result = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.Status)
                .Include(p => p.ProductBacklogItemEstimate)
                .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId)
                .ToListAsync();

            var dtoList = _mapper.Map<List<ProductBacklogItemDto>>(result);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + productBacklogItem.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemUpdated(productBacklogItem.Id, dtoList);

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
