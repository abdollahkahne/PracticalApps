using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Src.Core.Interfaces;
using Moq;
using Xunit;
using Src;
using Src.Core.Models;
using Src.Controllers;
using Microsoft.AspNetCore.Mvc;
using Src.Models;

namespace Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index__ReturnViewResult()
        {
            // Arrange
            // Setup MockRepository (we do not need db context options here!)
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            mockRepository.Setup(repo => repo.ListAsync()).Returns(Task.FromResult(GetTestSessions()));
            var controller = new HomeController(mockRepository.Object);
            // Act
            var result = await controller.Index();
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index__PopulateModel()
        {
            // Arrange
            // Setup MockRepository (we do not need db context options here!)
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            mockRepository.Setup(repo => repo.ListAsync()).Returns(Task.FromResult(GetTestSessions()));
            var controller = new HomeController(mockRepository.Object);
            // Act
            await controller.Index();
            var model = controller.ViewData.Model;
            // Assert
            var sessions = Assert.IsAssignableFrom<IEnumerable<StormSessionViewModel>>(model);
            Assert.NotEmpty(sessions);
        }

        [Fact]
        public async Task IndexPost_InvalidModelState_ReturnBadRequest()
        {
            // Arrange
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            var controller = new HomeController(mockRepository.Object);
            controller.ModelState.AddModelError("SessionName", "Session Name is required");
            var session = new NewSession();

            // Act
            var result = await controller.Index(session);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);

        }

        [Fact]
        public async Task IndexPost_ValidModelState_ReturnRedirectToActionResult()
        {
            // Arrange
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            // Since AddAsync Return void/Task, mock does not require further setup and by default return Task.CompletedTask
            var controller = new HomeController(mockRepository.Object);
            var session = new NewSession { SessionName = "Added Session" };

            // Act
            var result = await controller.Index(session);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }


        private List<BrainstormSession> GetTestSessions()
        {
            var sessions = new List<BrainstormSession>();

            sessions.Add(new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 2),
                Id = 1,
                Name = "Test One",
            });
            sessions.Add(new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 1),
                Id = 2,
                Name = "Test Two"
            });

            // Add idea to it
            var sessionOne = sessions.Single(s => s.Id == 1);
            sessionOne.Ideas.Add(new Idea
            {
                Id = 1,
                Name = "Idea One",
                DateCreated = new DateTime(2016, 7, 3),
                Description = "This is a sample idea for session Test One",
            });
            sessionOne.Ideas.Add(new Idea
            {
                Id = 2,
                Name = "Idea Two",
                DateCreated = new DateTime(2016, 7, 4),
                Description = "This is a sample idea for session Test One",
            });
            return sessions;
        }
    }
}