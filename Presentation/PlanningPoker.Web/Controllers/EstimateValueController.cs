using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Domain;
using PlanningPoker.Infrastructure;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlanningPoker.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstimateValueController : ControllerBase
    {
        readonly PlanningPokerContext _planningPokerContext;

        public EstimateValueController(PlanningPokerContext planningPokerContext)
        {
            _planningPokerContext = planningPokerContext;
        }

        
        // GET api/<EstimateValueController>/5
        [HttpGet("category/{id}/getvalues")]
        public async Task<ActionResult<List<EstimateValue>>> GetEstimateValues(int id)
        {
            return Ok(await _planningPokerContext.EstimateValue.Where(v => v.CategoryId == id).ToListAsync());
        }


        // GET api/<EstimateValueController>/5
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<EstimateValueCategory>>> GetCategories()
        {
            return Ok(await _planningPokerContext.EstimateValueCategory.ToListAsync());
        }

        // POST api/<EstimateValueController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] EstimateValueCategory value)
        {
            await _planningPokerContext.EstimateValueCategory.AddAsync(value);
            await _planningPokerContext.SaveChangesAsync();
            return NoContent();
        }

        // PUT api/<EstimateValueController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] EstimateValueCategory value)
        {
            var existing = await _planningPokerContext.EstimateValueCategory.SingleOrDefaultAsync(e => e.Id == id);
            if (existing == null)
            {
                return NotFound();
            }
            existing.Description = value.Description;
            for (int i = existing.EstimateValue.Count -1; i >= 0; i--)
            {
                var estimateValue = existing.EstimateValue.ElementAt(i);
                var newValue = value.EstimateValue.FirstOrDefault(v => v.Id == estimateValue.Id);
                if (newValue != null)
                {
                    estimateValue.Value = newValue.Value;
                    estimateValue.Label = newValue.Label;
                    estimateValue.Order = newValue.Order;
                }
                else
                {
                    existing.EstimateValue.Remove(estimateValue);
                }
            }
            foreach (var estimateValue in value.EstimateValue.Where(v  => !existing.EstimateValue.Any(ev => ev.Id == v.Id)))
            {
                existing.EstimateValue.Add(estimateValue);
            }
            _planningPokerContext.Update(existing);
            await _planningPokerContext.SaveChangesAsync();
            return NoContent();
        }

    }
}
