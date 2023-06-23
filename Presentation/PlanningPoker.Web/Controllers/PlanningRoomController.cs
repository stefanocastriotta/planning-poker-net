using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PlanningPoker.Domain;
using PlanningPoker.Infrastructure;
using PlanningPoker.Web.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlanningPoker.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanningRoomController : ControllerBase
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;

        public PlanningRoomController(PlanningPokerContext planningPokerContext, IMapper mapper)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
        }


        // GET api/<PlanningRoomController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlanningRoomDto?>> Get(int id)
        {
            var result = await _planningPokerContext.PlanningRoom
                .Include(p => p.EstimateValueCategory)
                    .ThenInclude(p => p.EstimateValue)
                .Include(p => p.ProductBacklogItem)
                    .ThenInclude(p => p.Status)
                .Include(p => p.ProductBacklogItem)
                    .ThenInclude(p => p.ProductBacklogItemEstimate)
                .Include(p => p.PlanningRoomUsers)
                    .ThenInclude(p => p.User)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return _mapper.Map<PlanningRoomDto>(result);
        }

        [HttpGet("{id}/productBacklogItems")]
        public async Task<ActionResult<List<ProductBacklogItemDto>>> GetProductBacklogItems(int id)
        {
            var result = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.Status)
                .Include(p => p.ProductBacklogItemEstimate)
                .Where(p => p.PlanningRoomId == id)
                .ToListAsync();

            if (result == null)
            {
                return NotFound();
            }

            return _mapper.Map<List<ProductBacklogItemDto>>(result);
        }


        [HttpPost("{id}/registeruser")]
        public async Task<ActionResult> RegisterUser(int id)
        {
            var existing = await _planningPokerContext.PlanningRoom.AnyAsync(p => p.Id == id);
            if (!existing)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _planningPokerContext.PlanningRoomUsers.Where(u => u.PlanningRoomId == id && u.UserId == currentUserId).SingleOrDefaultAsync();
            if (result == null)
            {
                await _planningPokerContext.PlanningRoomUsers.AddAsync(new PlanningRoomUsers() {  PlanningRoomId = id, UserId = currentUserId });
                await _planningPokerContext.SaveChangesAsync();
            }

            return NoContent();
        }

        // POST api/<PlanningRoomController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PlanningRoomModel value)
        {
            if (value.NewEstimateValueCategoryValues != null)
            {
                var categoryValues = value.NewEstimateValueCategoryValues.Split(",").ToList();
                EstimateValueCategory newCategory = new EstimateValueCategory();
                newCategory.Description = value.NewEstimateValueCategory;
                foreach (var category in categoryValues)
                {
                    newCategory.EstimateValue.Add(new EstimateValue
                    {
                        Label = category,
                        Value = int.TryParse(category, out int estimateValue) ? estimateValue : 0,
                        Order = categoryValues.IndexOf(category) + 1
                    });
                }
                await  _planningPokerContext.AddAsync(newCategory);
                await _planningPokerContext.SaveChangesAsync();
                value.EstimateValueCategoryId = newCategory.Id;
            }
            value.CreationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _planningPokerContext.PlanningRoom.Persist(_mapper).InsertOrUpdateAsync(value);
            await _planningPokerContext.SaveChangesAsync();
            
            return Created(Url.Action(nameof(Get), new { id = result.Id }), _mapper.Map<PlanningRoomDto>(result));
        }

        // PUT api/<PlanningRoomController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] PlanningRoom value)
        {
            var existing = await _planningPokerContext.PlanningRoom.SingleOrDefaultAsync(p => p.Id == id);
            if (existing == null)
            {
                return NotFound();
            }
            existing.Description = value.Description;
            _planningPokerContext.PlanningRoom.Update(value);
            await _planningPokerContext.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/<PlanningRoomController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = new PlanningRoom { Id = id };
            _planningPokerContext.PlanningRoom.Remove(entity);
            await _planningPokerContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
