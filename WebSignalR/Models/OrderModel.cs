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

        public int[] Order { get; private set; }

        public OrderModel()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

            Order = values.Shuffle().ToArray();

            var val = ThreadSafeRandom.ThisThreadsRandom.Next(Order.Length);
            Excluded = Order[val];
            Order[val] = 0;
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    static class MyExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}
