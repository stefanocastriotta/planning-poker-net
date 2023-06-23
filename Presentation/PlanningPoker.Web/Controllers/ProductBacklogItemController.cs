using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Infrastructure;
using PlanningPoker.Web.Models;

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

        public ProductBacklogItemController(PlanningPokerContext planningPokerContext, IMapper mapper)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
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

            return Ok(_mapper.Map<ProductBacklogItemDto>(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductBacklogItemDto>> Put(int id, [FromBody] ProductBacklogItemModel productBacklogItem)
        {
            var existing = await _planningPokerContext.ProductBacklogItem.SingleOrDefaultAsync(p => p.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync();
            if (productBacklogItem.StatusId == 2)
            {
                _planningPokerContext.ProductBacklogItem
                    .Where(p => p.PlanningRoomId == productBacklogItem.PlanningRoomId && p.StatusId == 2)
                    .ExecuteUpdate(u => u.SetProperty(p => p.StatusId, p => 1));
            }

            var result = await _planningPokerContext.ProductBacklogItem.Persist(_mapper).InsertOrUpdateAsync(productBacklogItem);
            await _planningPokerContext.SaveChangesAsync();
            result.Status = _planningPokerContext.ProductBacklogItemStatus.Single(s => s.Id == result.StatusId);
            await transaction.CommitAsync();

            return Ok(_mapper.Map<ProductBacklogItemDto>(result));
        }

        // DELETE api/<ProductBacklogItemController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
