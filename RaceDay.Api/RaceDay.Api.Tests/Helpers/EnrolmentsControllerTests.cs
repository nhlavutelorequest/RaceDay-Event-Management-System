using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Tests.Helpers;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentsControllerTests
    {
        private static (EnrolmentsController controller, ISession session, Data.RaceDayDbContext context) Setup()
        {
            var context = TestDbContextFactory.Create();

            var ev = new Event { EventId = 1, OrganiserId = 1, Name = "Test Event", EventDate = DateTime.UtcNow.AddDays(10), Location = "Test", DistanceKm = 10, EventType = "Run" };
            context.Events.Add(ev);
            context.Categories.Add(new Category { CategoryId = 1, EventId = 1, CategoryName = "Senior" });
            context.SaveChanges();

            var controller = new EnrolmentsController(context);
            ISession session = new FakeSession();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { Session = session } };

            return (controller, session, context);
        }

        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreatedAndRecordsEnrolment()
        {
            var (controller, session, context) = Setup();
            session.SetInt32("UserId", 5);
            session.SetString("Role", "Participant");

            var result = await controller.Enrol(1, new CreateEnrolmentDto { CategoryId = 1 });

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Single(context.Enrolments);
            Assert.Equal(5, context.Enrolments.First().ParticipantId);
        }

        [Fact]
        public async Task Enrol_Twice_ReturnsConflict()
        {
            var (controller, session, context) = Setup();
            session.SetInt32("UserId", 5);
            session.SetString("Role", "Participant");

            await controller.Enrol(1, new CreateEnrolmentDto { CategoryId = 1 });
            var secondAttempt = await controller.Enrol(1, new CreateEnrolmentDto { CategoryId = 1 });

            Assert.IsType<ConflictObjectResult>(secondAttempt);
        }

        [Fact]
        public async Task Enrol_WithInvalidCategoryForEvent_ReturnsBadRequest()
        {
            var (controller, session, _) = Setup();
            session.SetInt32("UserId", 5);
            session.SetString("Role", "Participant");

            var result = await controller.Enrol(1, new CreateEnrolmentDto { CategoryId = 999 });

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}