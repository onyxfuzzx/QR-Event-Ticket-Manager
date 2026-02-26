using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QREventPlatform.Advanced.Extensions;


namespace QREventPlatform.Advanced.Hubs;
[Authorize(Roles = "Admin")]
public class AdminHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var adminId = Context.User.GetUserId();

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"ADMIN_{adminId}"
        );

        await base.OnConnectedAsync();
    }
}
