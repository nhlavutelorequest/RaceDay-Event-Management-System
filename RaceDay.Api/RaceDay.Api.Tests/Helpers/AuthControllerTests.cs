using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.DTOs;
using RaceDay.Api.Tests.Helpers;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthControllerTests
    {
        private static AuthController CreateController(Data.RaceDayDbContext context)
        {
            var controller = new AuthController(context);
            ISession session = new FakeSession();
            var httpContext = new DefaultHttpContext { Session = session };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            var context = TestDbContextFactory.Create();
            var controller = CreateController(context);

            var dto = new RegisterDto
            {
                FullName = "Jane Organiser",
                Email = "jane@raceday.co.za",
                Password = "Password1!",
                Role = "Organiser"
            };

            var result = await controller.Register(dto);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var context = TestDbContextFactory.Create();
            var controller = CreateController(context);
            var dto = new RegisterDto { FullName = "A", Email = "dup@raceday.co.za", Password = "Password1!", Role = "Participant" };

            await controller.Register(dto);
            var secondResult = await controller.Register(dto);

            Assert.IsType<ConflictObjectResult>(secondResult);
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOk()
        {
            var context = TestDbContextFactory.Create();
            var controller = CreateController(context);
            var registerDto = new RegisterDto { FullName = "Login Test", Email = "login@raceday.co.za", Password = "Password1!", Role = "Participant" };
            await controller.Register(registerDto);

            var loginResult = await controller.Login(new LoginDto { Email = "login@raceday.co.za", Password = "Password1!" });

            Assert.IsType<OkObjectResult>(loginResult);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var context = TestDbContextFactory.Create();
            var controller = CreateController(context);
            var registerDto = new RegisterDto { FullName = "Wrong Pw", Email = "wrongpw@raceday.co.za", Password = "Password1!", Role = "Participant" };
            await controller.Register(registerDto);

            var loginResult = await controller.Login(new LoginDto { Email = "wrongpw@raceday.co.za", Password = "WrongPassword!" });

            Assert.IsType<UnauthorizedObjectResult>(loginResult);
        }
    }
}