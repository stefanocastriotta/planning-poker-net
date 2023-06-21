using System.ComponentModel.DataAnnotations;

namespace PlanningPoker.Web.Models
{
    public class PlanningRoomUserModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string Email { get; set; }

        public string NormalizedEmail { get; set; }
    }
}
