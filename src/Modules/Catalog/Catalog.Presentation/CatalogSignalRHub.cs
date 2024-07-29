namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Presentation;

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class CatalogSignalRHub : Hub
{
    public async Task OnCheckHealth()
    {
        await this.Clients.All.SendAsync("CheckHealth");
    }
}