using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSignalR
{
    public class Chat : Hub
    {
        public static Dictionary<string, int> Connections { get; set; } = new Dictionary<string, int>();

        public async Task JoinGroup(string groupName, string nick)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, 1);
            else
                Connections[groupName] += 1;

            await Clients.Group(groupName).SendAsync("GroupMessage", nick, nick  + " has joined the chat.");
        }

        public async Task LeaveGroup(string groupName, string nick)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, 0);
            else if(Connections[groupName] > 0)
                Connections[groupName] -= 1;

            await Clients.Group(groupName).SendAsync("GroupMessage", nick, nick + " has left the chat.");
        }

        public async Task GroupMessage(string groupName, string nick, string message)
        {
            await Clients.Group(groupName).SendAsync("GroupMessage", nick, message);
        }

        public async Task SendAll(string nick, string message)
        {
            await Clients.All.SendAsync("SendAll", nick, message);
        }
    }
}
