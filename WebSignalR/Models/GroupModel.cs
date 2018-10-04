using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSignalR.Models
{
    public class GroupModel
    {
        public OrderModel Order { get; set; }

        public UserModel CurrentClient { get; set; }

        public List<UserModel> ClientOrder { get; set; }

        public bool InProgress { get; set; }

        public GroupModel()
        {
            Order = new OrderModel();
            ClientOrder = new List<UserModel>();
        }

        public void ShuffleClients()
        {
            ClientOrder = ClientOrder.Shuffle().ToList();
            CurrentClient = ClientOrder[0];
        }

        public void StartOrder()
        {
            Order = new OrderModel();
        }

        public void FullReset()
        {
            Order = new OrderModel();
            ClientOrder = ClientOrder.Shuffle().ToList();
            CurrentClient = ClientOrder[0];
        }

        public string NextClient()
        {
            int o = ClientOrder.IndexOf(CurrentClient);
            if (o == ClientOrder.Count)
                CurrentClient = ClientOrder[0];
            else
                CurrentClient = ClientOrder[o + 1];

            if (!CurrentClient.Enabled)
                NextClient();

            return CurrentClient.ClientID;
        }
    }
}
