using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application;

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
        public async Task<ActionResult<List<EstimateValue>>> GetEstimateValues(int id, CancellationToken cancellationToken)
        {
            return Ok(await _planningPokerContext.EstimateValue.Where(v => v.CategoryId == id).ToListAsync(cancellationToken));
        }


        // GET api/<EstimateValueController>/5
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<EstimateValueCategory>>> GetCategories(CancellationToken cancellationToken)
        {
            return Ok(await _planningPokerContext.EstimateValueCategory.ToListAsync(cancellationToken));
        }

        // POST api/<EstimateValueController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] EstimateValueCategory value, CancellationToken cancellationToken)
        {
            await _planningPokerContext.EstimateValueCategory.AddAsync(value, cancellationToken);
            await _planningPokerContext.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
    }
}
