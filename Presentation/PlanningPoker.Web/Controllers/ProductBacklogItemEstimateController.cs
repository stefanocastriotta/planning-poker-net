using AutoMapper;
using AutoMapper.EntityFrameworkCore;
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
        public async Task<ActionResult<RegisterEstimateResult>> Post([FromBody] ProductBacklogItemEstimateModel productBacklogItemEstimateModel)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            productBacklogItemEstimateModel.UserId = currentUserId;

            var newEstimate = _mapper.Map<ProductBacklogItemEstimate>(productBacklogItemEstimateModel);
            var result = await _planningPokerContext.RegisterProductBacklogItemEstimateAsync(newEstimate);

            if (result == null)
            {
                return NotFound();
            }

            var estimateDto = _mapper.Map<ProductBacklogItemEstimateDto>(newEstimate);
            var productBacklogItemDto = _mapper.Map<ProductBacklogItemDto>(result);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + result.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemEstimated(estimateDto, productBacklogItemDto);

            return Ok(new RegisterEstimateResult { Estimate = estimateDto, ProductBacklogItem = productBacklogItemDto });
        }
    }
}
