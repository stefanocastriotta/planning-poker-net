using AutoMapper;

namespace PlanningPoker.Application.PlanningRooms
{
    internal class PlanningRoomAutomapperProfile : Profile
    {
        public PlanningRoomAutomapperProfile()
        {
            CreateMap<PlanningRoom, PlanningRoomDto>();
            CreateMap<CreatePlanningRoomCommand, PlanningRoom>();
            CreateMap<AspNetUsers, PlanningRoomUserDto>();
            CreateMap<PlanningRoomUsers, PlanningRoomUserDto>()
                .IncludeMembers(p => p.User);
        }
    }
}
