using System;

namespace cacheBenchmarker
{
    class Program
    {
        const int numInserts = 1000000;
        const int numReads = 100000;
        static void Main(string[] args)
        {
            Console.WriteLine("Running benchmark!");
            BenchmarkSqlServer();
            BenchmarkRedis();
        }

        static void BenchmarkSqlServer()
        {
            SqlServerBenchmarker blah = new SqlServerBenchmarker(numInserts, numReads);
            blah.memOrDisk = "Disk";
            Console.WriteLine(blah.memOrDisk);
            blah.DeleteData();
            blah.DoInserts().Wait();
            blah.RebuildIndex();
            blah.DoGets().Wait();

            blah = new SqlServerBenchmarker(numInserts, numReads);
            blah.DeleteData();
            blah.memOrDisk = "Mem";
            Console.WriteLine(blah.memOrDisk);
            blah.DoInserts().Wait();
            blah.DoGets().Wait();

        }

        static void BenchmarkRedis()
        {
            Console.WriteLine("Redis!");
            RedisBenchmarker blah = new RedisBenchmarker(numInserts, numReads);
            blah.DeleteData();
            blah.DoInserts().Wait();
            blah.DoGets().Wait();
        }
    }
}
