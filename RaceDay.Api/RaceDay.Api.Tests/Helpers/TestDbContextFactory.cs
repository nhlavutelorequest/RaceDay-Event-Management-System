using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static RaceDayDbContext Create()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);

            context.Roles.Add(new Role { RoleId = 1, RoleName = "Organiser" });
            context.Roles.Add(new Role { RoleId = 2, RoleName = "Participant" });
            context.SaveChanges();

            return context;
        }
    }
}