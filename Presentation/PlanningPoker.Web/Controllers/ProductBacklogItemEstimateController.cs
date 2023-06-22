using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Infrastructure;
using PlanningPoker.Web.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlanningPoker.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductBacklogItemEstimateController : ControllerBase
    {
        readonly PlanningPokerContext _planningPokerContext;
        readonly IMapper _mapper;

        public ProductBacklogItemEstimateController(PlanningPokerContext planningPokerContext, IMapper mapper)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ProductBacklogItemEstimateModel>> Post([FromBody] ProductBacklogItemEstimateModel productBacklogItemEstimateModel)
        {
            var existing = await _planningPokerContext.ProductBacklogItem.AnyAsync(p => p.Id == productBacklogItemEstimateModel.ProductBacklogItemId);
            if (!existing)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            productBacklogItemEstimateModel.UserId = currentUserId;

            var result = await _planningPokerContext.ProductBacklogItemEstimate.Persist(_mapper).InsertOrUpdateAsync(productBacklogItemEstimateModel);
            await _planningPokerContext.SaveChangesAsync();

            return Ok(_mapper.Map<ProductBacklogItemModel>(result));
        }
    }
}
