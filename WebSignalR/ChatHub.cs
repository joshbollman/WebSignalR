using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSignalR
{
    public class Chat : Hub
    {
        public async Task JoinGroup(string groupName, string nick)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName, string nick)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task GroupMessage(string groupName, string nick, string message)
        {
            await Clients.Group(groupName).SendAsync("SendAll", nick, message);
        }

        public async Task SendAll(string nick, string message)
        {
            await Clients.All.SendAsync("SendAll", nick, message);
        }
    }
}
