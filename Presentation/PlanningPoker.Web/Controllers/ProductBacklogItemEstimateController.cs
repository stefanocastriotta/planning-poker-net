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
        public async Task<ActionResult<RegisterEstimateResult>> Post([FromBody] ProductBacklogItemEstimateModel productBacklogItemEstimateModel)
        {
            var existing = await _planningPokerContext.ProductBacklogItem
                .Include(p => p.ProductBacklogItemEstimate)
                .Include(p => p.PlanningRoom.PlanningRoomUsers)
                .SingleOrDefaultAsync(p => p.Id == productBacklogItemEstimateModel.ProductBacklogItemId);
            if (existing == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            productBacklogItemEstimateModel.UserId = currentUserId;

            using var transaction = await _planningPokerContext.Database.BeginTransactionAsync();

            var estimate = _mapper.Map<ProductBacklogItemEstimate>(productBacklogItemEstimateModel);
            existing.ProductBacklogItemEstimate.Add(estimate);
            if (existing.PlanningRoom.PlanningRoomUsers.All(u => existing.ProductBacklogItemEstimate.Any(p => p.UserId == u.UserId)))
            {
                existing.StatusId = (int)ProductBaclogItemStatusEnum.Completed;
            }

            _planningPokerContext.Update(existing);

            await _planningPokerContext.SaveChangesAsync();
            await transaction.CommitAsync();

            existing.Status = _planningPokerContext.ProductBacklogItemStatus.Single(s => s.Id == existing.StatusId);

            var estimateDto = _mapper.Map<ProductBacklogItemEstimateDto>(estimate);
            var productBacklogItemDto = _mapper.Map<ProductBacklogItemDto>(existing);
            await _hubContext.Clients.GroupExcept("PlanningRoom" + existing.PlanningRoomId, _connectionManager.GetUserConnectionId(currentUserId)).ProductBacklogItemEstimated(estimateDto, productBacklogItemDto);

            return Ok(new RegisterEstimateResult { Estimate = estimateDto, ProductBacklogItem = productBacklogItemDto });
        }
    }
}
