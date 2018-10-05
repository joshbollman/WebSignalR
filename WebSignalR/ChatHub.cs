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
        #region Fields
        const string _generalChat = "general";
        #endregion

        #region Properties
        public static Dictionary<string, List<UserModel>> Connections { get; set; } = new Dictionary<string, List<UserModel>>();

        public static Dictionary<string, GroupModel> ChatGroups { get; set; } = new Dictionary<string, GroupModel>();
        #endregion

        #region Constructors
        #endregion

        #region Methods
        public override Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var kvp in Connections.Where(m => m.Value.Any(u => u.ClientID == Context.ConnectionId)))
            {
                Connections[kvp.Key].RemoveAll(u => u.ClientID == Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName, string handle)
        {
            if (groupName == _generalChat)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
            {
                Connections.Add(groupName, new List<UserModel>() {
                    new UserModel() {
                        ClientID = Context.ConnectionId,
                        UserName = handle,
                        Enabled = true
                    }
                });
            }
            else if (Connections[groupName].Any(u => u.ClientID == Context.ConnectionId))
            {
                return;
            }
            else if (Connections[groupName].Count() == 10 || (ChatGroups.ContainsKey(groupName) && ChatGroups[groupName].InProgress))
            {
                await SelfMessage($"{groupName} is full.");
                return;
            }
            else
            {
                Connections[groupName].Add(
                    new UserModel()
                    {
                        ClientID = Context.ConnectionId,
                        UserName = handle,
                        Enabled = true
                    });
            }
            await GroupMessage(groupName, "", $"{handle} has joined {groupName}.");
        }

        public async Task LeaveGroup(string groupName, string handle)
        {
            if (groupName == _generalChat)
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, new List<UserModel>());
            else if (Connections[groupName].Any(c => c.ClientID == Context.ConnectionId))
                Connections[groupName].RemoveAll(c => c.ClientID == Context.ConnectionId);

            if (ChatGroups.ContainsKey(groupName) && ChatGroups[groupName].ClientOrder.Any(c => c.ClientID == Context.ConnectionId))
                ChatGroups[groupName].ClientOrder.First(c => c.ClientID == Context.ConnectionId).Enabled = false;

            await GroupMessage(groupName, "", $"{handle} has left {groupName}.");
        }

        public async Task SelfMessage(string message)
        {
            await Clients.Caller.SendAsync("SelfMessage", message);
        }

        public async Task GroupMessage(string groupName, string handle, string message)
        {
            if (groupName == _generalChat)
            {
                await Clients.Others.SendAsync("GroupMessage", handle + "[general]", message);
                await SelfMessage("[general] " + message);
                return;
            }

            await OthersInGroup(groupName, handle, message);
            await SelfMessage(message);
        }

        public async Task OthersInGroup(string groupName, string handle, string message)
        {
            await Clients.OthersInGroup(groupName).SendAsync("GroupMessage", handle, message);
        }
        #endregion

        #region letter
        public async Task StartGroupSession(string groupName)
        {
            var group = ChatGroups[groupName] = new GroupModel();
            group.InProgress = true;
            group.ClientOrder.AddRange(Connections[groupName]);

            group.FullReset();

            await StartGame(groupName);
            await StartTurn(groupName);
            await TurnEligible(groupName, group.ClientOrder, group.CurrentClient.ClientID);
            await Clients.Others.SendAsync("GroupMessage", groupName, "Dealer", $"It's {group.CurrentClient.UserName}'s turn.");
        }

        public async Task StartGame(string groupName)
        {
            if (ChatGroups.ContainsKey(groupName))
            {
                var group = ChatGroups[groupName];

                foreach (var c in group.ClientOrder)
                {
                    var nextOrder = group.Order.GetNext();
                    c.CurrentCard = nextOrder;

                    await Clients.Client(c.ClientID).SendAsync("StartGame", $"/images/chat/{nextOrder.ToNumberWord()}.jpg");
                }
            }
        }

        public async Task StartTurn(string groupName)
        {
            if (ChatGroups.ContainsKey(groupName))
            {
                var group = ChatGroups[groupName];
                var client = group.NextClient();
                var nextOrder = group.Order.GetNext();
                group.CurrentClient.Plays.Add($"/images/chat/{nextOrder.ToNumberWord()}.jpg");

                if (nextOrder > 0)
                {
                    await Clients.Client(client).SendAsync("NewTurn", $"/images/chat/{nextOrder.ToNumberWord()}.jpg");
                    await Clients.Client(client).SendAsync("GroupMessage", "Dealer", $"It's your turn, {group.CurrentClient.UserName}");
                }
                else
                    await Clients.Group(groupName).SendAsync("NewTurn", "end");
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
        #endregion
    }
}
