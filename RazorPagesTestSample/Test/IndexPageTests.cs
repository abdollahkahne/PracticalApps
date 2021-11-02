using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Src.Data;
using Src.Pages;
using Test.Utilities;
using Xunit;

namespace Test
{
    // This group of tests often mock the methods of the DAL (Data Access Layer):
    // When a page model method executes a method from DAL, the mock returns the result.
    // The data doesn't come from the database. This creates predictable, reliable test conditions
    public class IndexPageTests
    {
        // Determining if the methods follow the correct behavior when the ModelState is invalid.
        // Confirming the methods produce the correct IActionResult.
        // Checking that property value assignments are made correctly (Like Messages Property in OnGetAsync)
        [Fact]
        public async Task OnGetAsync__PopulatePageModelMessagesProperty()
        {
            //Given (Arrange)
            var seedingMsgs = AppDbContext.GetSeedingMessages();
            // Setup Mock (Or better to say it is Stub Here since it only used as initial fake data and not used in Assert Phase altough it is used as Mock in some of tests )
            var options = (new DbContextOptionsBuilder<AppDbContext>()).Options;
            var mockAppDbContext = new Mock<AppDbContext>(options);
            mockAppDbContext.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(seedingMsgs));

            // inject Db Context to Index Page Model
            var pageModel = new IndexModel(db: mockAppDbContext.Object);

            var expected = seedingMsgs.OrderBy(msg => msg.Id).Select(msg => msg.Text);


            //When (Act)
            await pageModel.OnGetAsync();
            var messages = pageModel.Messages;
            var actual = messages.OrderBy(msg => msg.Id).Select(msg => msg.Text);

            //Then (Assert)
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task OnPostAddMessageAsync_InvalidModel_ReturnAPageResult()
        {
            //Given (Arrange)
            var seedingMsgs = AppDbContext.GetSeedingMessages();
            // Setup Mock (Or better to say it is Stub Here since it only used as initial fake data and not used in Assert Phase altough it is used as Mock in some of tests )
            var options = (new DbContextOptionsBuilder<AppDbContext>()).Options;
            var mockAppDbContext = new Mock<AppDbContext>(options);
            // we need this setup since we call it in case of invalid model
            mockAppDbContext.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(seedingMsgs));

            // Generate Index Page Model and also inject to it database
            var pageModel = Utility.TestIndexModel(db: mockAppDbContext.Object);

            // make page model state invalid
            pageModel.ModelState.AddModelError("Message.Text", "The Text field is required.");

            // Act (When)
            var result = await pageModel.OnPostAddMessageAsync();

            // Assert
            Assert.IsType<PageResult>(result);

        }

        [Fact]
        public async Task OnPostAddMessageAsync_InvalidModel_PopulateMessagesProperty()
        {
            // 1- Arrange
            // Generate a stub of database
            var seedingMsgs = AppDbContext.GetSeedingMessages();
            var expected = seedingMsgs.OrderBy(msg => msg.Id).Select(msg => msg.Text);
            var options = (new DbContextOptionsBuilder<AppDbContext>()).Options;
            var stubDbContext = new Mock<AppDbContext>(options);
            stubDbContext.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(seedingMsgs));

            // Generate Index Page Model
            var pageModel = Utility.TestIndexModel(stubDbContext.Object);
            // make it invalid
            pageModel.ModelState.AddModelError("Message.Text", "The field is required");

            // 2- Act
            await pageModel.OnPostAddMessageAsync();
            var actual = pageModel.Messages.OrderBy(msg => msg.Id).Select(msg => msg.Text);

            // 3- Assert
            Assert.Equal(expected, actual);

        }

        // The following test does not need Message Property and strict Setup in Mock since:
        // Database Method (AddMessageAsync) doesn't return anything
        // Model State by Default is Valid and Message=null used in method.
        // Even it is not necessary to use Utility. Use new IndexModel(db);

        [Fact]
        public async Task OnPostAddMessageAsync_ValidModelState_ReturnRedirectToPageResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockAppDbContext = new Mock<AppDbContext>(options);
            // var message = new Message { Id = 1, Text = "Add a new Message" };
            // mockAppDbContext.Setup(db => db.AddMessageAsync(message)).Returns(Task.CompletedTask);
            // var pageModel = Utility.TestIndexModel(db: mockAppDbContext.Object);
            var pageModel = new IndexModel(mockAppDbContext.Object);
            // pageModel.Message = message;
            // Console.WriteLine(pageModel.ModelState.ValidationState.ToString()); // A new ModelStateDictionary is valid by default.
            //Act
            var result = await pageModel.OnPostAddMessageAsync();
            //Assert
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task OnPostDeleteAllMessagesAsync__ReturnRedirectToPageResult()
        {
            // Arrange
            // Since the method return void/Task we do not need special setu for stub database
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var stubDbContext = new Mock<AppDbContext>(options);
            var pageModel = new IndexModel(stubDbContext.Object);

            // Act
            var result = await pageModel.OnPostDeleteAllMessagesAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task OnPostDeleteMessageAsync__ReturnRedirectToPageResult()
        {
            // Arrange
            // Since the method return void/Task we do not need special setu for stub database
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var stubDbContext = new Mock<AppDbContext>(options);
            var pageModel = new IndexModel(stubDbContext.Object);
            var recId = 1;

            // Act
            var result = await pageModel.OnPostDeleteMessageAsync(recId);

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task OnPostAnalyzeMessagesAsync_NoneEmptyMessagesList_ReturnRedirectToPageResult()
        {
            // Arrange
            var seedingMsgs = AppDbContext.GetSeedingMessages();
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var stubDb = new Mock<AppDbContext>(options);
            stubDb.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(seedingMsgs));
            var pageModel = new IndexModel(stubDb.Object);
            // Act
            var result = await pageModel.OnPostAnalyzeMessagesAsync();
            // Assert
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task OnPostAnalyzeMessagesAsync_NoneEmptyMessagesList_SameMassageAnalysisResult()
        {
            // Arrange
            var seedingMsgs = AppDbContext.GetSeedingMessages();
            // Generate MessageAnalysisResult for fake data
            var wordCount = 0;
            foreach (var item in seedingMsgs)
            {
                wordCount += item.Text.Split(' ').Length;
            }
            var avgWordCount = Decimal.Divide(wordCount, (seedingMsgs.Count));
            var expected = $"The average message length is {avgWordCount:0.##} words";
            // generate fake db
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var stubDb = new Mock<AppDbContext>(options);
            stubDb.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(seedingMsgs));
            var pageModel = new IndexModel(stubDb.Object);
            // Act
            await pageModel.OnPostAnalyzeMessagesAsync();
            var actual = pageModel.MessageAnalysisResult;
            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task OnPostAnalyzeMessagesAsync_EmptyMessagesList_ReturnRedirectToPageResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var stubDb = new Mock<AppDbContext>(options);
            stubDb.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(new List<Message>()));
            var pageModel = new IndexModel(stubDb.Object);
            // Act
            var result = await pageModel.OnPostAnalyzeMessagesAsync();
            // Assert
            Assert.IsType<RedirectToPageResult>(result);
        }
        [Fact]
        public async Task OnPostAnalyzeMessagesAsync_EmptyMessagesList_SameMassageAnalysisResult()
        {
            // Arrange
            var expected = "There are no messages to analyze.";
            // generate fake db
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var stubDb = new Mock<AppDbContext>(options);
            stubDb.Setup(db => db.GetMessagesAsync()).Returns(Task.FromResult(new List<Message>()));
            var pageModel = new IndexModel(stubDb.Object);
            // Act
            await pageModel.OnPostAnalyzeMessagesAsync();
            var actual = pageModel.MessageAnalysisResult;
            // Assert
            Assert.Equal(expected, actual);
        }
    }
}