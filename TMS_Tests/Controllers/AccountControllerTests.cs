using Moq;
using TMS_.Controllers;
using TMS_.Data;
using TMS_.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMS_Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<TMS_DbContext> _mockContext;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var options = new DbContextOptions<TMS_DbContext>();
            _mockContext = new Mock<TMS_DbContext>(options);
            _controller = new AccountController(_mockContext.Object);
        }

        private Mock<DbSet<T>> CreateMockSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var user = new User { Username = "test", Password = "password", Role = "User" };

            var users = new List<User> { user }.AsQueryable();
            var mockSet = CreateMockSet(users);

            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var result = await _controller.Register(user);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var login = new User { Username = "test", Password = "wrongpassword" };

            var users = new List<User>().AsQueryable();
            var mockSet = CreateMockSet(users);

            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            // Ensure the login object is not null
            Assert.NotNull(login);

            var result = await _controller.Login(login);

            // Ensure the result object is not null
            Assert.NotNull(result);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
