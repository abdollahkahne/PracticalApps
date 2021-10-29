using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Src.Data;
using Test.Utilities;
using Xunit;

namespace Test
{
    public class DataAccessLayerTest
    {
        [Fact]
        public async Task DeleteMessageAsync_WhenMessageExist_MessageDeleted()
        {
            var options = Utility.TestDbContextOptions();
            using (var db = new AppDbContext(options))
            {
                // Arrange (Given)
                var seedMsgs = AppDbContext.GetSeedingMessages();
                // await db.Initialize(); // This is wrong since it is dependent on other method!
                db.Messages.AddRange(seedMsgs); // This method already tested and should consider verified
                await db.SaveChangesAsync();
                var recId = 1;
                var expected = seedMsgs.Where(message => message.Id != recId).OrderBy(msg => msg.Id).Select(msg => msg.Text).ToList();

                // Act (When)
                await db.DeleteMessageAsync(recId);

                // Assert (Then)
                var actual = db.Messages.OrderBy(msg => msg.Id).Select(msg => msg.Text).ToList();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task DeleteMessageAsync_WhenMessageNotExist_NoMessageDeleted()
        {
            var options = Utility.TestDbContextOptions();
            using (var db = new AppDbContext(options))
            {
                // Arrange (Given)
                var seedMsgs = AppDbContext.GetSeedingMessages();
                // await db.Initialize(); // This is wrong since it is dependent on other method!
                db.Messages.AddRange(seedMsgs); // This method already tested and should consider verified
                await db.SaveChangesAsync();
                var recId = 0;
                var expected = seedMsgs.OrderBy(msg => msg.Id).Select(msg => msg.Text).ToList();

                // Act (When)
                try
                {
                    await db.DeleteMessageAsync(recId);
                }
                catch (System.Exception)
                {


                }

                // Assert (Then)
                var actual = db.Messages.OrderBy(msg => msg.Id).Select(msg => msg.Text).ToList();
                Assert.Equal(expected, actual);
                // In order to compare that the two List<Message> are the same:
                // The messages are ordered by Id.
                // Message pairs are compared on the Text property.

            }
        }

        [Fact]
        public async Task GetMessagesAsync__ReturnsAllMessage()
        {
            var options = Utility.TestDbContextOptions();
            using (var db = new AppDbContext(options))
            {
                // Given-Arrange
                var seedMsgs = AppDbContext.GetSeedingMessages();
                db.Messages.AddRange(seedMsgs);
                await db.SaveChangesAsync();
                var expected = seedMsgs.OrderBy(msg => msg.Id).Select(msg => msg.Text).ToList();

                // When-Act
                var result = await db.GetMessagesAsync();
                var actual = result.OrderBy(msg => msg.Id).Select(msg => msg.Text).ToList();

                // then-Assert
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task AddMessageAsync__MessageAdded()
        {
            // Here we decide to test alternative way to generate DbContextOptions
            // Here we get the error that:  An item with the same key has already been added
            // So always try to generate a serviceProvider first
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("InMemory").Options;
            using (var db = new AppDbContext(options))
            {
                //Arrange (Given)
                var recId = 1;
                var message = new Message { Id = recId, Text = "This message added in Test" };
                var expected = message;

                // Act (When)
                await db.AddMessageAsync(message);
                var actual = db.Messages.Where(msg => msg.Id == recId).SingleOrDefault();

                // Assert (Then)
                Assert.Equal(expected, actual);
            }
        }

        // [Fact]
        // public async Task AddMessageAsync__MessageAdded2()
        // {
        //     // Here we decide to test alternative way to generate DbContextOptions
        //     var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("InMemory").Options;
        //     using (var db = new AppDbContext(options))
        //     {
        //         //Arrange (Given)
        //         var recId = 1;
        //         var message = new Message { Id = recId, Text = "This message added in second Test" };
        //         var expected = message;

        //         // Act (When)
        //         await db.AddMessageAsync(message);
        //         var actual = db.Messages.Where(msg => msg.Id == recId).SingleOrDefault();

        //         // Assert (Then)
        //         Assert.Equal(expected, actual);
        //     }
        // }

        [Fact]
        public async Task DeleteAllMessagesAsync__AllMessagesDeleted()
        {
            var options = Utility.TestDbContextOptions();
            using (var db = new AppDbContext(options))
            {

                // Given (Arrange)
                var seedMsgs = AppDbContext.GetSeedingMessages();
                db.Messages.AddRange(seedMsgs);
                await db.SaveChangesAsync();

                // When (Act)
                await db.DeleteAllMessagesAsync();
                var messages = db.Messages.AsNoTracking().ToList();

                //Then (Assert)
                Assert.Empty(messages);

            }
        }

    }
}