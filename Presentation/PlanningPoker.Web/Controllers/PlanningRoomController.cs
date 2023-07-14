using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PlanningPoker.Application;
using PlanningPoker.Application.PlanningRooms;
using PlanningPoker.Web.SignalrHub;
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
        readonly PlanningRoomRequestHandler _planningRoomRequestHandler;
        readonly IHubContext<PlanningRoomHub, IPlanningRoomHub> _hubContext;
        readonly ConnectionManager _connectionManager;

        public PlanningRoomController(PlanningPokerContext planningPokerContext, IMapper mapper, PlanningRoomRequestHandler planningRoomRequestHandler, IHubContext<PlanningRoomHub, IPlanningRoomHub> hubContext, ConnectionManager connectionManager)
        {
            _planningPokerContext = planningPokerContext;
            _mapper = mapper;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _planningRoomRequestHandler = planningRoomRequestHandler;
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

        [HttpPost("{id}/registeruser")]
        public async Task<ActionResult> RegisterUser(int id, CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _planningRoomRequestHandler.HandleAsync(new RegisterPlanningRoomUserRequest(id, currentUserId), cancellationToken);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }
            else if (result.Value.IsNew)
            {
                await _hubContext.Clients.GroupExcept("PlanningRoom" + id, _connectionManager.GetUserConnectionId(currentUserId)).UserJoined(_mapper.Map<PlanningRoomUserDto>(result.Value.PlanningRoomUsers));
            }

            return NoContent();
        }

        // POST api/<PlanningRoomController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PlanningRoomModel value, CancellationToken cancellationToken)
        {
            value.CreationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _planningRoomRequestHandler.HandleAsync(value, cancellationToken);
            
            return Created(Url.Action(nameof(Get), new { id = result.Id }), _mapper.Map<PlanningRoomDto>(result));
        }

        // PUT api/<PlanningRoomController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] PlanningRoomModel value)
        {
            var existing = await _planningPokerContext.PlanningRoom.SingleOrDefaultAsync(p => p.Id == id);
            if (existing == null)
            {
                return NotFound();
            }
            existing.Description = value.Description;
            _planningPokerContext.PlanningRoom.Update(existing);
            await _planningPokerContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
