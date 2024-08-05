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
    public class TasksControllerTests
    {
        private readonly Mock<TMS_DbContext> _mockContext;
        private readonly TasksController _controller;

        // setting up the mock context and controller for tests
        public TasksControllerTests()
        {
            var options = new DbContextOptions<TMS_DbContext>();
            _mockContext = new Mock<TMS_DbContext>(options);
            _controller = new TasksController(_mockContext.Object);
        }

        // method to create a mock DbSet
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

        // test for unauthorized access to get user tasks with invalid user
        [Fact]
        public async Task GetUserTasks_ReturnsUnauthorizedForInvalidUser()
        {
            var users = new List<User>().AsQueryable();
            var tasks = new List<UserTask>().AsQueryable();

            var mockUserSet = CreateMockSet(users);
            var mockTaskSet = CreateMockSet(tasks);

            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.UserTasks).Returns(mockTaskSet.Object);

            var result = await _controller.GetUserTasks("invaliduser") as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        // test for bad request when assigning a task to an invalid user
        [Fact]
        public async Task AssignTask_ReturnsBadRequestForInvalidAssignment()
        {
            var task = new TaskAssignmentDto
            {
                Title = "New Task",
                Description = "Description",
                DueDate = DateTime.Now,
                Priority = "High",
                AssignedTo = "invaliduser",
                CreatedBy = "adminUsername"
            };

            var users = new List<User>
            {
                new User { Id = 1, Username = "adminUsername", Role = "Admin" }
            }.AsQueryable();

            var tasks = new List<UserTask>().AsQueryable();
            var mockUserSet = CreateMockSet(users);
            var mockTaskSet = CreateMockSet(tasks);

            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.UserTasks).Returns(mockTaskSet.Object);

            var result = await _controller.AssignTask(task) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        // test for not found when updating the status of an invalid task
        [Fact]
        public async Task UpdateTaskStatus_ReturnsNotFoundForInvalidTask()
        {
            var updateStatus = new TaskStatusUpdateDto { TaskId = 999, Status = "InProgress" };

            var tasks = new List<UserTask>().AsQueryable();
            var mockTaskSet = CreateMockSet(tasks);
            _mockContext.Setup(c => c.UserTasks).Returns(mockTaskSet.Object);

            var result = await _controller.UpdateTaskStatus(updateStatus) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // test for not found when deleting an invalid task
        [Fact]
        public async Task DeleteTask_ReturnsNotFoundForInvalidTask()
        {
            var tasks = new List<UserTask>().AsQueryable();

            var mockTaskSet = CreateMockSet(tasks);
            _mockContext.Setup(c => c.UserTasks).Returns(mockTaskSet.Object);

            var result = await _controller.DeleteTask(999, "adminUsername") as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }
    }
}
