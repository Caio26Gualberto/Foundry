using Microsoft.AspNetCore.SignalR;

namespace Boilerplate.Application.Services.SignalR
{
    public class SystemNotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"🔗 SignalR connected: UserIdentifier={Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }
    }
}
