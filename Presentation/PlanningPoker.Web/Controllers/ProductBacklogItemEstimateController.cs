using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using FluentResults;
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
        readonly ProductBacklogItemCommandHandler _productBacklogItemCommandHandler;

        public ProductBacklogItemEstimateController(PlanningPokerContext planningPokerContext, IMapper mapper, IHubContext<PlanningRoomHub, IPlanningRoomHub> hubContext, ConnectionManager connectionManager, ProductBacklogItemCommandHandler productBacklogItemCommandHandler)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _productBacklogItemCommandHandler = productBacklogItemCommandHandler;
        }

        [HttpPost]
        public async Task<ActionResult<RegisterEstimateResultDto>> RegisterProductBacklogItem([FromBody] RegisterProductBacklogItemEstimateCommand registerProductBacklogItemEstimateCommand, CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            registerProductBacklogItemEstimateCommand.UserId = currentUserId;

            var result = await _productBacklogItemCommandHandler.RegisterProductBacklogItemEstimateAsync(registerProductBacklogItemEstimateCommand, cancellationToken);

            if (result.IsFailed)
            {
                if (result.HasNotFoundErrorMetadata())
                    return NotFound(result.Errors.Select(e => e.ToString()));
                return BadRequest(result.Errors.Select(e => e.ToString()));
            }

            var estimateDto = _mapper.Map<ProductBacklogItemEstimateDto>(result.Value.ProductBacklogItemEstimate);
            var productBacklogItemDto = _mapper.Map<ProductBacklogItemDto>(result.Value.ProductBacklogItemEstimate.ProductBacklogItem);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + productBacklogItemDto.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemEstimated(estimateDto, productBacklogItemDto);

            return Ok(new RegisterEstimateResultDto { Estimate = estimateDto, ProductBacklogItem = productBacklogItemDto });
        }
    }
}
