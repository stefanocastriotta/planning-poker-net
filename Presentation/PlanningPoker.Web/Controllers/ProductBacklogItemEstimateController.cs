using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Domain;
using PlanningPoker.Infrastructure;
using PlanningPoker.Web.Models;
using PlanningPoker.Web.SignalrHub;
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
        readonly IHubContext<PlanningRoomHub, IPlanningRoomHub> _hubContext;
        readonly ConnectionManager _connectionManager;

        public ProductBacklogItemEstimateController(PlanningPokerContext planningPokerContext, IMapper mapper, IHubContext<PlanningRoomHub, IPlanningRoomHub> hubContext, ConnectionManager connectionManager)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
        }

        [HttpPost]
        public async Task<ActionResult<ProductBacklogItemEstimateModel>> Post([FromBody] ProductBacklogItemEstimateModel productBacklogItemEstimateModel)
        {
            var existing = await _planningPokerContext.ProductBacklogItem.SingleOrDefaultAsync(p => p.Id == productBacklogItemEstimateModel.ProductBacklogItemId);
            if (existing == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            productBacklogItemEstimateModel.UserId = currentUserId;

            var result = await _planningPokerContext.ProductBacklogItemEstimate.Persist(_mapper).InsertOrUpdateAsync(productBacklogItemEstimateModel);
            await _planningPokerContext.SaveChangesAsync();

            var estimateDto = _mapper.Map<ProductBacklogItemEstimateDto>(result);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + existing.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemEstimated(estimateDto);

            return Ok(estimateDto);
        }
    }
}
