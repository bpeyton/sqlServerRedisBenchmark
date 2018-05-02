using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace cacheBenchmarker
{
    class RedisBenchmarker
    {
        const string connectionStr = "localhost:32768";
        int numInserts, numReads;


        ConnectionMultiplexer redis;
        ConcurrentBag<string> ids = new ConcurrentBag<string>();

        public RedisBenchmarker(int numInserts, int numReads)
        {
            this.numInserts = numInserts;
            this.numReads = numReads;
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionStr);
            options.AllowAdmin = true;
            redis = ConnectionMultiplexer.Connect(options);
        }

        public void DeleteData()
        {
            IServer server = redis.GetServer(connectionStr);
            
            server.FlushDatabaseAsync().Wait();
            Console.WriteLine("All redis dbs flushed!");
        }

        public async Task DoInserts()
        {

            IDatabase db = redis.GetDatabase();

            int count = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int getModulo = numInserts / numReads;
            
            List<Task<bool>> sets = new List<Task<bool>>(numInserts);
            for (int i = 0; i < numInserts; i++)
            {
                

                Random random = new Random();
                string guidStr = Guid.NewGuid().ToString();
                sets.Add(db.StringSetAsync(guidStr, random.Next().ToString()));
                Interlocked.Increment(ref count);
                if (count % getModulo == 0)
                {
                    ids.Add(guidStr);
                }
                
            }
            
            await Task.WhenAll(sets);
            stopwatch.Stop();
            double seconds = stopwatch.Elapsed.TotalSeconds;
            double rowsPerSecond = ids.Count / seconds;
            Console.WriteLine($"{numInserts} sets completed.. {seconds} seconds... {rowsPerSecond} gets per second");
            

        }

        public async Task DoGets()
        {
            IDatabase db = redis.GetDatabase();
            List<Task<RedisValue>> tasks = new List<Task<RedisValue>>(ids.Count);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (string id in ids)
            {
                //RedisKey key = RedisKey;

                tasks.Add( db.StringGetAsync(id));
            }
            await Task.WhenAll(tasks);

            stopwatch.Stop();
            double seconds = stopwatch.Elapsed.TotalSeconds;
            double rowsPerSecond = ids.Count / seconds;
            Console.WriteLine($"{ids.Count} gets read.. {seconds} seconds... {rowsPerSecond} gets per second");
            Console.WriteLine($"random value {tasks[482].Result}");
        }

    }
}
