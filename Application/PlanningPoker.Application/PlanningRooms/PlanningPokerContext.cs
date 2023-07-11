using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Application.PlanningRooms;

namespace PlanningPoker.Application
{
    public partial class PlanningPokerContext
    {
        public async Task<PlanningRoom> CreatePlanningRoomAsync(IMapper mapper, PlanningRoomModel value, string userId)
        {
            if (value.NewEstimateValueCategoryValues != null)
            {
                var categoryValues = value.NewEstimateValueCategoryValues.Split(",").ToList();
                EstimateValueCategory newCategory = new EstimateValueCategory();
                newCategory.Description = value.NewEstimateValueCategory;
                foreach (var category in categoryValues)
                {
                    newCategory.EstimateValue.Add(new EstimateValue
                    {
                        Label = category,
                        Value = int.TryParse(category, out int estimateValue) ? estimateValue : 0,
                        Order = categoryValues.IndexOf(category) + 1
                    });
                }
                await AddAsync(newCategory);
                await SaveChangesAsync();
                value.EstimateValueCategoryId = newCategory.Id;
            }
            value.CreationUserId = userId;
            var result = await PlanningRoom.Persist(mapper).InsertOrUpdateAsync(value);
            await SaveChangesAsync();

            return result;
        }

        public async Task<(PlanningRoomUsers planningRoomUsers, bool isNew)> RegisterPlanningRoomUserAsync(int planningRoomId, string userId)
        {
            var result = await PlanningRoomUsers.Include(p => p.User).Where(u => u.PlanningRoomId == planningRoomId && u.UserId == userId).SingleOrDefaultAsync();
            if (result == null)
            {
                var newUser = new PlanningRoomUsers() { PlanningRoomId = planningRoomId, UserId = userId };
                await PlanningRoomUsers.AddAsync(newUser);
                await SaveChangesAsync();

                return (PlanningRoomUsers.Include(p => p.User).Single(p => p.UserId == userId && p.PlanningRoomId == planningRoomId), true);
            }

            return (result, false);
        }

    }
}
