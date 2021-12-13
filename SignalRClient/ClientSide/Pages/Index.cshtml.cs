using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClientSide.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HubClient _hubClient;

    public IndexModel(ILogger<IndexModel> logger, HubClient hubClient)
    {
        _logger = logger;
        _hubClient = hubClient;
    }

    public async Task OnGet()
    {
        await _hubClient.SendMessage(new ServerSide.Hubs.MessageInput { Messsage = "Called From .Net Client" });
    }
}
