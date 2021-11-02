using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Src.Core.Interfaces;
using Src.Core.Models;

namespace Src.Infrastructure
{
    public class EfStormSessionRepository : IBrainstormSessionRepository
    {
        private readonly AppDbContext _db;

        public EfStormSessionRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(BrainstormSession session)
        {
            _db.BrainstormSessions.Add(session);
            return _db.SaveChangesAsync();
        }

        public Task<BrainstormSession> GetByIdAsync(int id)
        {
            return _db.BrainstormSessions
            .Include(session => session.Ideas)
            .AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == id);
        }

        public Task<List<BrainstormSession>> ListAsync()
        {
            return _db.BrainstormSessions
            .Include(session => session.Ideas)
            .OrderByDescending(session => session.DateCreated)
            .AsNoTracking()
            .ToListAsync();
        }

        public Task UpdateAsync(BrainstormSession session)
        {
            _db.BrainstormSessions.Update(session);
            // _db.Entry(session).State = EntityState.Modified; // This only update the session and not ideas if already they are not tracking as a result of Getting
            return _db.SaveChangesAsync();
        }
    }
}