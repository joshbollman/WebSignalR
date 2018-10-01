using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace WebSignalR.Models
{
    public class OrderModel
    {
        private int[] values = new int[] {
            1,1,1,1,
            2,2,
            3,3,
            4,4,
            5,5,
            6,
            7,
            8 };

        public int Excluded { get; private set; }

        public int Current { get; private set; }

        public int[] Order { get; private set; }

        public OrderModel()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

            Order = values.Shuffle().ToArray();

            var val = ThreadSafeRandom.ThisThreadsRandom.Next(Order.Length);
            Excluded = Order[val];
            Order[val] = 0;
        }

        public int GetNext()
        {
            Current++;

            if (Current < Order.Length - 1)
            {
                if (Order[Current] > 0)
                    return Order[Current];
                else
                {
                    return GetNext();
                }
            }
            else
                return 0;
        }
    }
}
