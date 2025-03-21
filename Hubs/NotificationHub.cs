using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace user_management.Hubs
{
    [SignalRHub]
    public class NotificationHub : Hub
    {
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        public Task SendMessage(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task SendNotificationToAdmins(string message)
        {
            await Clients.Group("Admins").SendAsync("ReceiveNotification", message);
        }
    }
}

