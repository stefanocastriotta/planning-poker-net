using System.Collections.Concurrent;

namespace PlanningPoker.Web.SignalrHub
{
    public class ConnectionManager
    {
        private ConcurrentDictionary<string, string> UserConnectionIds { get; set; } = new ConcurrentDictionary<string, string>();
        public void AddUserConnectionId(string user, string connectionId)
        {
            UserConnectionIds.AddOrUpdate(user, u => connectionId, (key, value) => connectionId);
        }

        public void RemoveUserConnectionId(string user)
        {
            UserConnectionIds.Remove(user, out string _);
        }

        public string? GetUserConnectionId(string user)
        {
            return UserConnectionIds.GetValueOrDefault(user);
        }
    }
}
