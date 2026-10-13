using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class EventTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public void CreateEvent_SavesEventWithOrganiserId()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var organiser = new User
            {
                FullName = "Org",
                Email = "org@raceday.com",
                PasswordHash = "hash",
                Role = "Organiser"
            };
            context.Users.Add(organiser);
            context.SaveChanges();

            var newEvent = new Event
            {
                Name = "Test Marathon",
                Description = "A test event",
                Date = new DateTime(2026, 5, 1),
                Location = "Cape Town",
                Distance = 21.1,
                EventType = "Run",
                OrganiserId = organiser.Id
            };

            // Act
            context.Events.Add(newEvent);
            context.SaveChanges();

            // Assert
            var saved = context.Events.First();
            Assert.Equal("Test Marathon", saved.Name);
            Assert.Equal(organiser.Id, saved.OrganiserId);
        }

        [Fact]
        public void GetEvents_ReturnsAllEvents()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Events.Add(new Event { Name = "Event A", OrganiserId = 1 });
            context.Events.Add(new Event { Name = "Event B", OrganiserId = 1 });
            context.SaveChanges();

            // Act
            var events = context.Events.ToList();

            // Assert
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public void Enrolment_LinksParticipantEventAndCategory()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            var enrolment = new Enrolment
            {
                ParticipantId = 5,
                EventId = 3,
                CategoryId = 2,
                EnrolledAt = DateTime.UtcNow
            };

            // Act
            context.Enrolments.Add(enrolment);
            context.SaveChanges();

            // Assert
            var saved = context.Enrolments.First();
            Assert.Equal(5, saved.ParticipantId);
            Assert.Equal(3, saved.EventId);
            Assert.Equal(2, saved.CategoryId);
        }
    }
}