using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Features;
using RaceDayAPI.Attributes;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class RoleEnforcementTests
    {
        // Helper that creates an AuthorizationFilterContext with a given role in session
        private AuthorizationFilterContext CreateContext(string? role)
        {
            var httpContext = new DefaultHttpContext();

            // Set up an in-memory session so the test can store/read values
            var sessionFeature = new TestSessionFeature();
            httpContext.Features.Set<ISessionFeature>(sessionFeature);

            if (role != null)
            {
                httpContext.Session.SetString("UserRole", role);
                httpContext.Session.SetInt32("UserId", 1);
            }

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void Organiser_OnOrganiserEndpoint_IsAllowed()
        {
            var context = CreateContext("Organiser");
            var attribute = new RoleAuthorizeAttribute("Organiser");

            attribute.OnAuthorization(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void Participant_OnOrganiserEndpoint_IsForbidden()
        {
            var context = CreateContext("Participant");
            var attribute = new RoleAuthorizeAttribute("Organiser");

            attribute.OnAuthorization(context);

            Assert.IsType<ObjectResult>(context.Result);
            var result = context.Result as ObjectResult;
            Assert.Equal(403, result!.StatusCode);
        }

        [Fact]
        public void Organiser_OnParticipantEndpoint_IsForbidden()
        {
            var context = CreateContext("Organiser");
            var attribute = new RoleAuthorizeAttribute("Participant");

            attribute.OnAuthorization(context);

            Assert.IsType<ObjectResult>(context.Result);
            var result = context.Result as ObjectResult;
            Assert.Equal(403, result!.StatusCode);
        }

        [Fact]
        public void NoSession_OnAnyEndpoint_IsUnauthorized()
        {
            var context = CreateContext(null);
            var attribute = new RoleAuthorizeAttribute("Organiser");

            attribute.OnAuthorization(context);

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }
    }

    // A minimal in-memory session implementation for tests
    public class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = new TestSession();
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id => "test-session";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}