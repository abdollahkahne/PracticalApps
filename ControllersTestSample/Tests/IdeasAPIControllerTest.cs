using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Src.Api;
using Src.ClientModels;
using Src.Core.Interfaces;
using Src.Core.Models;
using Xunit;

namespace Tests
{
    public class IdeasAPIControllerTest
    {
        [Fact]
        public async Task ForSession_SessionNotFound_ReturnNotFoundObjectResult()
        {
            // Arrange
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            var sessionId = 1;
            mockRepository.Setup(repository => repository.GetByIdAsync(sessionId)).ReturnsAsync((BrainstormSession)null); // An alternative to return Task is Returns(Task.From((BrainstormSession)null))
            var controller = new IdeasController(mockRepository.Object);
            // Act
            var result = await controller.ForSession(sessionId);
            // Assert
            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(sessionId, notFoundObjectResult.Value); // Check consistency in return type for APIs
        }

        [Fact]
        public async Task ForSession_SessionFound_ReturnOkObjectResult()
        {
            // Arrange
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            var sessionId = 1;
            mockRepository.Setup(repository => repository.GetByIdAsync(sessionId)).Returns(Task.FromResult(GetTestSession()));
            var controller = new IdeasController(mockRepository.Object);
            // Act
            var result = await controller.ForSession(sessionId);
            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ForSession_SessionFound_ReturnIdeaList()
        {
            // Arrange
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            var sessionId = 1;
            var session = GetTestSession();
            var expected = session.Ideas.OrderBy(idea => idea.Id).Select(idea => idea.Name);
            mockRepository.Setup(repository => repository.GetByIdAsync(sessionId)).Returns(Task.FromResult(session));
            var controller = new IdeasController(mockRepository.Object);
            // Act
            var result = await controller.ForSession(sessionId);
            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsAssignableFrom<IEnumerable<IdeaDTO>>(okObjectResult.Value).OrderBy(idea => idea.Id).Select(idea => idea.Name);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Create_InvalidModelState_ReturnBadRequestObjectResult()
        {
            //Arrange
            var controller = new IdeasController(repository: null);
            var newIdea = new NewIdeaModel();
            controller.ModelState.AddModelError("Name", "Name is required");
            //Act
            var result = await controller.Create(newIdea);
            // Assert
            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestObjectResult.Value);

        }

        [Fact]
        public async Task Create_SessionDoesNotExistToAddIdea_ReturnNotFoundObjectResult()
        {
            // Arrange
            var newIdea = new NewIdeaModel()
            {
                SessionId = 1,
                Name = "Sample Idea from Test",
                Description = "This idea created during test",
            };
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            mockRepository.Setup(repository => repository.GetByIdAsync(newIdea.SessionId))
            .Returns(Task.FromResult((BrainstormSession)null));
            var controller = new IdeasController(mockRepository.Object);

            // Act
            var result = await controller.Create(newIdea);

            // Assert
            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(newIdea.SessionId, notFoundObjectResult.Value);

        }

        [Fact]
        public async Task Create_ValidModelAndSessionExist_ReturnOkObjectResult()
        {
            // Arrange
            var newIdea = new NewIdeaModel()
            {
                SessionId = 1,
                Name = "Sample Idea from Test",
                Description = "This idea created during test",
            };
            var session = GetTestSession();
            var mockRepository = new Mock<IBrainstormSessionRepository>();
            mockRepository.Setup(repository => repository.GetByIdAsync(newIdea.SessionId))
            .Returns(Task.FromResult(session));
            // session.Ideas.Add(new Idea
            // {
            //     Name = newIdea.Name,
            //     Description = newIdea.Description,
            //     DateCreated = DateTimeOffset.UnixEpoch,
            // });
            // mockRepository.Setup(repository => repository.UpdateAsync(session))
            // .Returns(Task.CompletedTask);// This unnecessary since by default it complete methods that does return void/Task
            var controller = new IdeasController(mockRepository.Object);

            // Act
            var result = await controller.Create(newIdea);

            // Assert
            Assert.IsType<OkResult>(result); // In case that Ok method does not have parameters it return OkResult instead of OkObjectResult
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