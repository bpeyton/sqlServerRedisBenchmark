using System;

namespace cacheBenchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running benchmark!");
            BenchmarkSqlServer();
            BenchmarkRedis();
        }

        static void BenchmarkSqlServer()
        {
            SqlServerBenchmarker blah = new SqlServerBenchmarker();
            blah.memOrDisk = "Disk";
            Console.WriteLine(blah.memOrDisk);
            blah.DeleteData();
            blah.DoInserts().Wait();
            blah.RebuildIndex();
            blah.DoGets().Wait();

            blah = new SqlServerBenchmarker();
            blah.DeleteData();
            blah.memOrDisk = "Mem";
            Console.WriteLine(blah.memOrDisk);
            blah.DoInserts().Wait();
            blah.DoGets().Wait();

        }

        static void BenchmarkRedis()
        {
            Console.WriteLine("Redis!");
            RedisBenchmarker blah = new RedisBenchmarker();
            blah.DeleteData();
            blah.DoInserts().Wait();
            blah.DoGets().Wait();
        }
    }
}
