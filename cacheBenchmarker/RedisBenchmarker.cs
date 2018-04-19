using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace cacheBenchmarker
{
    class RedisBenchmarker
    {
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:32768");
        ConcurrentBag<string> ids = new ConcurrentBag<string>();

        public async Task DoInserts()
        {

            IDatabase db = redis.GetDatabase();

            int count = 0;

            //Parallel.For(0, 1000000, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, i =>
            const int numInserts = 1000000;
            List<Task<bool>> sets = new List<Task<bool>>(numInserts);
            for (int i = 0; i < numInserts; i++)
            {
                

                Random random = new Random();
                string guidStr = Guid.NewGuid().ToString();
                //db.StringSet(guidStr, random.Next().ToString());
                sets.Add(db.StringSetAsync(guidStr, random.Next().ToString()));
                Interlocked.Increment(ref count);
                if (count % 10 == 0)
                {
                    ids.Add(guidStr);
                }
                if (count % 10000 == 0)
                {
                    Console.WriteLine("count = " + count.ToString());
                }
            }
            
            await Task.WhenAll(sets);
            
        }

        public async Task DoGets()
        {
            IDatabase db = redis.GetDatabase();
            List<Task<RedisValue>> tasks = new List<Task<RedisValue>>(ids.Count);
            DateTime startTime = DateTime.Now;

            foreach (string id in ids)
            {
                //RedisKey key = RedisKey;

                tasks.Add( db.StringGetAsync(id));
            }
            await Task.WhenAll(tasks);

            DateTime endTime = DateTime.Now;
            double seconds = (endTime - startTime).TotalSeconds;
            double rowsPerSecond = ids.Count / seconds;
            Console.WriteLine($"{ids.Count} gets read.. {seconds} seconds... {rowsPerSecond} gets per second");
            Console.WriteLine($"random value {tasks[482].Result}");
        }

    }
}
