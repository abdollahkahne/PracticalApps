using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Src.Data;

namespace Src.Pages
{
    // 0- define class for input model here inside the class if they aren't defined as models
    // 1- Define ctor and inject dependencies
    // 2- Define Bind Properties, Temp Data and View/Bag Data
    // 3- Define Properties which used for data saving but not binded
    // 4- Define Private/public helper methods
    public class IndexModel : PageModel
    {
        // private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Message Message { get; set; }

        [TempData]
        public string MessageAnalysisResult { get; set; }
        public IList<Message> Messages { get; private set; }

        public async Task OnGetAsync()
        {
            Messages = await _db.GetMessagesAsync();
        }

        public async Task<IActionResult> OnPostAddMessageAsync()
        {
            // check Model Validity in case of POST except in APIs
            if (!ModelState.IsValid)
            {
                // Messages = await _db.GetMessagesAsync(); // This is reuired since the Messages Property does not fill in constructor itself but in OnGetAsync. Otherwise it would be null and in case of clien-side verification failure it gave an error
                return Page();
            }
            await _db.AddMessageAsync(Message);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllMessagesAsync()
        {
            await _db.DeleteAllMessagesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMessageAsync(int id)
        {
            await _db.DeleteMessageAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAnalyzeMessagesAsync()
        {
            Messages = await _db.GetMessagesAsync();
            if (Messages.Count == 0)
            {
                MessageAnalysisResult = "There are no messages to analyze.";
            }
            else
            {
                var wordCount = 0;
                foreach (var item in Messages)
                {
                    wordCount += item.Text.Split(' ').Length;
                }
                var avgWordCount = Decimal.Divide(wordCount, (Messages.Count));
                MessageAnalysisResult = $"The average message length is {avgWordCount:0.##} words";
            }
            return RedirectToPage();
        }
    }
}
