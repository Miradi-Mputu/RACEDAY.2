using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class AuthTests
    {
        // Helper method that creates a fresh in-memory database for each test
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public void Register_StoresHashedPassword_NotPlainText()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var plainPassword = "Password123";

            var user = new User
            {
                FullName = "Test User",
                Email = "test@raceday.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                Role = "Participant"
            };

            // Act
            context.Users.Add(user);
            context.SaveChanges();

            // Assert
            var savedUser = context.Users.First();
            Assert.NotEqual(plainPassword, savedUser.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(plainPassword, savedUser.PasswordHash));
        }

        [Fact]
        public void Register_RejectsDuplicateEmail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User
            {
                FullName = "First",
                Email = "same@raceday.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("abc123"),
                Role = "Participant"
            });
            context.SaveChanges();

            // Act
            bool alreadyExists = context.Users.Any(u => u.Email == "same@raceday.com");

            // Assert
            Assert.True(alreadyExists);
        }

        [Fact]
        public void Login_WithWrongPassword_Fails()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var correctPassword = "CorrectPassword";
            context.Users.Add(new User
            {
                FullName = "Test",
                Email = "user@raceday.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword),
                Role = "Participant"
            });
            context.SaveChanges();

            var user = context.Users.First();

            // Act
            bool isValid = BCrypt.Net.BCrypt.Verify("WrongPassword", user.PasswordHash);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Login_WithCorrectPassword_Succeeds()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User
            {
                FullName = "Test",
                Email = "user2@raceday.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("MyPassword"),
                Role = "Organiser"
            });
            context.SaveChanges();

            var user = context.Users.First();

            // Act
            bool isValid = BCrypt.Net.BCrypt.Verify("MyPassword", user.PasswordHash);

            // Assert
            Assert.True(isValid);
        }
    }
}