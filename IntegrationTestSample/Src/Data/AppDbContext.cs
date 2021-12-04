using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Src.Data.IdentityModels;

namespace Src.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<Message> Messages { get; set; }

        #region snippet1
        public virtual async Task<List<Message>> GetMessagesAsync()
        {
            return await Messages
            .OrderBy(msg => msg.Text)
            .AsNoTracking()
            .ToListAsync();
        }
        #endregion

        #region snippet2
        public virtual async Task AddMessageAsync(Message message)
        {
            Messages.Add(message);
            await SaveChangesAsync();
        }
        #endregion

        #region snippet3
        public virtual async Task DeleteAllMessagesAsync()
        {
            Messages.RemoveRange(Messages);
            await SaveChangesAsync();
        }
        #endregion

        #region snippet4
        public virtual async Task DeleteMessageAsync(int id)
        {
            var message = await Messages.FindAsync(id);
            if (message != null)
            {
                Messages.Remove(message);
                await SaveChangesAsync();
            }

        }
        #endregion

        public static List<Message> GetSeedingMessages()
        {
            return new List<Message> {
                new Message(){Id=1, Text = "You're standing on my scarf." },
                new Message(){Id=2, Text = "Would you like a jelly baby?" },
                new Message(){Id=3, Text = "To the rational mind, nothing is inexplicable; only unexplained." }
            };
        }

        public void Initialize()
        {
            Messages.AddRange(GetSeedingMessages());
            SaveChanges();
        }

    }
}