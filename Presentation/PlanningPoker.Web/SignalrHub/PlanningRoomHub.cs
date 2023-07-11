using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PlanningPoker.Web.SignalrHub
{
    [Authorize]
    public class PlanningRoomHub : Hub<IPlanningRoomHub>
    {
        readonly ConnectionManager _connectionManager;
        public PlanningRoomHub(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override Task OnConnectedAsync()
        {
            _connectionManager.AddUserConnectionId(Context.UserIdentifier, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionManager.RemoveUserConnectionId(Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(int planningRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "PlanningRoom" + planningRoomId);
        }

        //public async Task UserJoined(int planningRoomId, PlanningRoomUserDto planningRoomUserDto)
        //{
        //    await Clients.OthersInGroup("PlanningRoom" + planningRoomId).SendAsync("UserJoined", planningRoomUserDto);
        //}

        //public async Task ProductBacklogItemInserted(int planningRoomId, ProductBacklogItemDto productBacklogItem)
        //{
        //    await Clients.OthersInGroup("PlanningRoom"+ planningRoomId).SendAsync("ProductBacklogItemInserted", productBacklogItem);
        //}
    }
}
