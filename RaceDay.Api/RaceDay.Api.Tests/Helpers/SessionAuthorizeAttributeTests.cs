using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.Api.Filters;
using RaceDay.Api.Tests.Helpers;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class SessionAuthorizeAttributeTests
    {
        private static AuthorizationFilterContext CreateContext(ISession session)
        {
            var httpContext = new DefaultHttpContext { Session = session };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void NoSession_ReturnsUnauthorized()
        {
            ISession session = new FakeSession();
            var context = CreateContext(session);
            var attribute = new SessionAuthorizeAttribute();

            attribute.OnAuthorization(context);

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void WrongRole_ReturnsForbidden()
        {
            ISession session = new FakeSession();
            session.SetInt32("UserId", 1);
            session.SetString("Role", "Participant");
            var context = CreateContext(session);
            var attribute = new SessionAuthorizeAttribute("Organiser");

            attribute.OnAuthorization(context);

            Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, ((ObjectResult)context.Result!).StatusCode);
        }

        [Fact]
        public void CorrectRole_AllowsAccess()
        {
            ISession session = new FakeSession();
            session.SetInt32("UserId", 1);
            session.SetString("Role", "Organiser");
            var context = CreateContext(session);
            var attribute = new SessionAuthorizeAttribute("Organiser");

            attribute.OnAuthorization(context);

            Assert.Null(context.Result);
        }
    }
}