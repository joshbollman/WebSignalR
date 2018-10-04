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

        #endregion

        #region Properties
        public static Dictionary<string, List<UserModel>> Connections { get; set; } = new Dictionary<string, List<UserModel>>();

        public static Dictionary<string, GroupModel> ChatGroups { get; set; } = new Dictionary<string, GroupModel>();
        #endregion

        #region Constructors
        #endregion

        #region Methods


        public async Task JoinGroup(string groupName, string nick)
        {
            //await Clients.Client(Context.ConnectionId).SendAsync("ConnectionID", Context.ConnectionId);

            if (groupName == "general")
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, new List<UserModel>() {
                    new UserModel() {
                        ClientID = Context.ConnectionId,
                        UserName = nick,
                        Enabled = true
                    }
                });
            else if (Connections[groupName].Count() == 10 || (ChatGroups.ContainsKey(groupName) && ChatGroups[groupName].InProgress))
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

            await Clients.Group(groupName).SendAsync("GroupMessage", "Server", nick + " has joined the group.");
        }

        public async Task LeaveGroup(string groupName, string nick)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (!Connections.ContainsKey(groupName))
                Connections.Add(groupName, new List<UserModel>());
            else if (Connections[groupName].Any(c => c.ClientID == Context.ConnectionId))
                Connections[groupName].RemoveAll(c => c.ClientID == Context.ConnectionId);

            if (ChatGroups.ContainsKey(groupName) && ChatGroups[groupName].ClientOrder.Any(c => c.ClientID == Context.ConnectionId))
                ChatGroups[groupName].ClientOrder.First(c => c.ClientID == Context.ConnectionId).Enabled = false;

            await Clients.Group(groupName).SendAsync("GroupMessage", nick, nick + " has left the group.");
        }

        public async Task GroupMessage(string groupName, string nick, string message)
        {
            if (groupName == "general")
                await Clients.All.SendAsync("GroupMessage", nick + "(general)", message);

            await Clients.Group(groupName).SendAsync("GroupMessage", nick, message);
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
