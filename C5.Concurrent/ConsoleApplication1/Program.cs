using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C5.Concurrent;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var q = new ConcurrentQueue<int>();

            Parallel.For(0, 1000, q.Enqueue);

            var l = new List<int>();

            while (true)
            {
                var item = q.Dequeue();
                if (item.IsSome)
                {
                    l.Add(item.Value);
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine(string.Join(", ", l));
        }
    }
}
