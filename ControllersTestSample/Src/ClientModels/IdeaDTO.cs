using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Src.ClientModels
{
    // Why use DTO models instead of domain entities? (for example we use NewIdeaModel and IdeaDTO instead of Idea)
    // Avoid returning business domain entities directly via API calls. Domain entities:
    // Often include more data than the client requires.
    // Unnecessarily couple the app's internal domain model with the publicly exposed API.

    public class IdeaDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}