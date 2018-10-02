using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSignalR.Models;

namespace WebSignalR
{
    public class Chat : Hub
    {
        public static Dictionary<string, List<UserModel>> Connections { get; set; } = new Dictionary<string, List<UserModel>>();

        public static Dictionary<string, GroupModel> ChatGroups { get; set; } = new Dictionary<string, GroupModel>();

        public async Task JoinGroup(string groupName, string nick)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, new List<UserModel>() {
                    new UserModel() {
                        ClientID = Context.ConnectionId,
                        UserName = nick,
                        Enabled = true
                    }
                });
            else if (Connections[groupName].Count() == 4)
            {
                await Clients.Caller.SendAsync("GroupMessage", "", $"Group '{groupName}' is full.");
                return;
            }
            else
                Connections[groupName].Add(
                    new UserModel()
                    {
                        ClientID = Context.ConnectionId,
                        UserName = nick,
                        Enabled = true
                    });

            await Clients.Group(groupName).SendAsync("GroupMessage", "Server", nick  + " has joined the chat.");
        }

        public async Task LeaveGroup(string groupName, string nick)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, new List<UserModel>());
            else if(Connections[groupName].Any(c => c.ClientID == Context.ConnectionId))
                Connections[groupName].RemoveAll(c => c.ClientID == Context.ConnectionId);

            await Clients.Group(groupName).SendAsync("GroupMessage", nick, nick + " has left the chat.");
        }

        public async Task GroupMessage(string groupName, string nick, string message)
        {
            await Clients.Group(groupName).SendAsync("GroupMessage", nick, message);
        }

        public async Task StartGroupSession(string groupName)
        {
            var group = ChatGroups[groupName] = new GroupModel();
            group.ClientOrder.AddRange(Connections[groupName]);

            group.FullReset();

            await TurnEligible(groupName, group.ClientOrder, group.CurrentClient.ClientID);
            await GroupMessage(groupName, "Server", $"it is now {group.CurrentClient.UserName}'s turn.");
        }

        public async Task StartTurn(string groupName)
        {
            if (ChatGroups.ContainsKey(groupName)) {
                var group = ChatGroups[groupName];
                var client = group.NextClient();
                var nextOrder = group.Order.GetNext();

                if (nextOrder > 0)
                {
                    await SendAllBut(groupName, "StartTurn", group.ClientOrder, client, "XXX");
                    await Clients.Client(client).SendAsync("StartTurn", nextOrder);
                }
                else
                    await Clients.Group(groupName).SendAsync("StartTurn", "end");
            }
        }

        public async Task DoTurnAction(string groupName, string action)
        {
            if (ChatGroups.ContainsKey(groupName))
            {
                var group = ChatGroups[groupName];
                var client = group.ClientOrder.First(c => c.ClientID == Context.ConnectionId);
                client.Plays.Add(action);

                await Clients.Group(groupName).SendAsync("Action", client.UserName + " chose " + action + "<br/>Plays: " + string.Join(' ', client.Plays.ToArray()));
            }
        }

        public async Task AskEligible(string group)
        {
            if (!string.IsNullOrWhiteSpace(group) && Connections.ContainsKey(group))
                await TurnEligible(group, Connections[group], Context.ConnectionId);
        }

        public async Task TurnEligible(string groupName, List<UserModel> clients, string clientName)
        {
            await Clients.Group(groupName).SendAsync("TurnEligible", "false");
            await Clients.Client(clientName).SendAsync("TurnEligible", "true");
        }

        public async Task SendAll(string nick, string message)
        {
            await Clients.All.SendAsync("SendAll", nick, message);
        }

        public async Task SendAllBut(string groupName, string function, List<UserModel> clients, string except, string message)
        {
            foreach(var client in clients.Where(c => c.ClientID != except))
            {
                await Clients.Client(client.ClientID).SendAsync(function, message);
            }
        }
    }
}
