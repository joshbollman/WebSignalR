using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSignalR.Models
{
    public class UserModel
    {
        public string ClientID { get; set; }

        public string UserName { get; set; }

        public int Successes { get; set; }

        public bool Enabled { get; set; }

        public int[] Plays { get; set; }
    }
}
