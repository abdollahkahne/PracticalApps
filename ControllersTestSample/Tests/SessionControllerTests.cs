using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Src.Controllers;
using Src.Core.Interfaces;
using Src.Core.Models;
using Src.Models;
using Xunit;

namespace Tests
{
    public class SessionControllerTests
    {
        [Fact]
        public async Task Index_NullId_ReturnRedirectToActionResult()
        {
            // Arrange (This does not do anything with Repository dependency so we can insert it null)
            var controller = new SessionController(repository: null);
            // Act
            var result = await controller.Index(id: null);
            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            // we can add related Assert as complement to above assertion
            Assert.Equal("Home", redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Index_IdHasValueButSessionNotFound_ReturnContentResult()
        {
            // Arrange
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            var id = 1;
            mockRepository.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((BrainstormSession)null); // An alternative to return Task is Returns(Task.From((BrainstormSession)null))
            var controller = new SessionController(mockRepository.Object);
            // Act
            var result = await controller.Index(id);
            // Assert
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("Session not found.", contentResult.Content);
        }

        [Fact]
        public async Task Index_IdHasValueAndSessionFound_ReturnViewResult()
        {
            // Arrange
            var id = 1;
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            mockRepository.Setup(repository => repository.GetByIdAsync(id))
                .Returns(Task.FromResult(GetTestSession()));
            var controller = new SessionController(mockRepository.Object);
            // Act
            var result = await controller.Index(id);
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_IdHasValueAndSessionFound_PopulateModel()
        {
            // Arrange
            var id = 1;
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            mockRepository.Setup(repository => repository.GetByIdAsync(id))
                .Returns(Task.FromResult(GetTestSession()));
            var controller = new SessionController(mockRepository.Object);
            // Act
            await controller.Index(id);
            // Assert
            var model = Assert.IsType<StormSessionViewModel>(controller.ViewData.Model);
            Assert.NotNull(model);
        }


        private BrainstormSession GetTestSession()
        {
            var session = new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 2),
                Id = 1,
                Name = "Test One",
            };

            // Add idea to it
            session.Ideas.Add(new Idea
            {
                Id = 1,
                Name = "Idea One",
                DateCreated = new DateTime(2016, 7, 3),
                Description = "This is a sample idea for session Test One",
            });
            session.Ideas.Add(new Idea
            {
                Id = 2,
                Name = "Idea Two",
                DateCreated = new DateTime(2016, 7, 4),
                Description = "This is a sample idea for session Test One",
            });
            return session;
        }
    }
}