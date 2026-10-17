
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212_PART2.Authorization;
using PROG6212_PART2.Controllers;
using PROG6212_PART2.Data;
using PROG6212_PART2.Models;
using Xunit;

namespace PROG6212_PART2.Tests
{
    public class RaceDayTests
    {
        private static RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private static DefaultHttpContext CreateHttpContext(
            int? userId = null,
            string? role = null)
        {
            var context = new DefaultHttpContext();

            context.Features.Set<ISessionFeature>(
                new TestSessionFeature
                {
                    Session = new TestSession()
                });

            if (userId.HasValue)
                context.Session.SetInt32("UserId", userId.Value);

            if (role != null)
                context.Session.SetString("UserRole", role);

            return context;
        }

        [Fact]
        public void IsAuthenticated_NoUser_ReturnsFalse()
        {
            var context = CreateHttpContext();

            Assert.False(
                SessionAuthorization.IsAuthenticated(context));
        }

        [Fact]
        public void IsAuthenticated_LoggedInUser_ReturnsTrue()
        {
            var context = CreateHttpContext(1, "Participant");

            Assert.True(
                SessionAuthorization.IsAuthenticated(context));
        }

        [Fact]
        public void HasRole_CorrectRole_ReturnsTrue()
        {
            var context = CreateHttpContext(1, "Organiser");

            Assert.True(
                SessionAuthorization.HasRole(context, "Organiser"));
        }

        [Fact]
        public void HasRole_WrongRole_ReturnsFalse()
        {
            var context = CreateHttpContext(1, "Participant");

            Assert.False(
                SessionAuthorization.HasRole(context, "Organiser"));
        }

        [Fact]
        public async Task GetEvents_NotLoggedIn_ReturnsUnauthorized()
        {
            using var db = CreateDatabase();

            var controller = new EventsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContext()
                }
            };

            var result = await controller.GetEvents();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateEvent_Participant_ReturnsForbidden()
        {
            using var db = CreateDatabase();

            var controller = new EventsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContext(1, "Participant")
                }
            };

            var eventItem = new Event
            {
                Name = "Test Run",
                Description = "Test event",
                EventDate = DateTime.Now.AddDays(10),
                Location = "Cape Town",
                Distance = 5,
                EventType = "Run"
            };

            var result = await controller.CreateEvent(eventItem);

            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, response.StatusCode);
        }

        [Fact]
        public async Task CreateEnrolment_ValidCategory_ReturnsCreated()
        {
            using var db = CreateDatabase();

            db.Events.Add(new Event
            {
                EventId = 1,
                OrganiserId = 10,
                Name = "Cape Town Fun Run",
                EventDate = DateTime.Now.AddDays(10),
                Location = "Cape Town",
                Distance = 5,
                EventType = "Run"
            });

            db.Categories.Add(new Category
            {
                CategoryId = 1,
                EventId = 1,
                Name = "Open",
                Distance = 5,
                MaxParticipants = 100
            });

            await db.SaveChangesAsync();

            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContext(20, "Participant")
                }
            };

            var result = await controller.CreateEnrolment(
                new Enrolment { CategoryId = 1 });

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Single(await db.Enrolments.ToListAsync());
        }

        [Fact]
        public async Task CreateEnrolment_Organiser_ReturnsForbidden()
        {
            using var db = CreateDatabase();

            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContext(10, "Organiser")
                }
            };

            var result = await controller.CreateEnrolment(
                new Enrolment { CategoryId = 1 });

            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, response.StatusCode);
        }

        [Fact]
        public async Task CreateEnrolment_UnknownCategory_ReturnsNotFound()
        {
            using var db = CreateDatabase();

            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContext(20, "Participant")
                }
            };

            var result = await controller.CreateEnrolment(
                new Enrolment { CategoryId = 999 });

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id => "test-session";
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public Task CommitAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value)
            => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
            => _store.TryGetValue(key, out value!);
    }

    public class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = null!;
    }
}

