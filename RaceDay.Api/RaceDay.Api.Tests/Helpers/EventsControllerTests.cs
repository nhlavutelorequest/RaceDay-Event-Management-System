using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.DTOs;
using RaceDay.Api.Tests.Helpers;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventsControllerTests
    {
        private static (EventsController controller, ISession session) CreateController(Data.RaceDayDbContext context)
        {
            var controller = new EventsController(context);
            ISession session = new FakeSession();
            var httpContext = new DefaultHttpContext { Session = session };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return (controller, session);
        }

        private static UpdateEventDto SampleEvent() => new()
        {
            Name = "Test Run",
            Description = "A test event",
            EventDate = DateTime.UtcNow.AddDays(30),
            Location = "Pretoria",
            DistanceKm = 10,
            EventType = "Run"
        };

        [Fact]
        public async Task Create_AsOrganiser_ReturnsCreated()
        {
            var context = TestDbContextFactory.Create();
            var (controller, session) = CreateController(context);

            session.SetInt32("UserId", 1);
            session.SetString("Role", "Organiser");

            var result = await controller.Create(SampleEvent());

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Update_ByNonOwningOrganiser_ReturnsForbid()
        {
            var context = TestDbContextFactory.Create();
            var (controller, session) = CreateController(context);
            session.SetInt32("UserId", 1);
            session.SetString("Role", "Organiser");
            var createResult = await controller.Create(SampleEvent()) as CreatedAtActionResult;
            var eventId = (int)createResult!.Value!.GetType().GetProperty("EventId")!.GetValue(createResult.Value)!;

            session.SetInt32("UserId", 2);

            var updateResult = await controller.Update(eventId, SampleEvent());

            Assert.IsType<ForbidResult>(updateResult);
        }

        [Fact]
        public async Task Delete_NonExistentEvent_ReturnsNotFound()
        {
            var context = TestDbContextFactory.Create();
            var (controller, session) = CreateController(context);
            session.SetInt32("UserId", 1);
            session.SetString("Role", "Organiser");

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}