using System;

namespace cacheBenchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running benchmark!");
            SqlServerBenchmarker blah = new SqlServerBenchmarker();
            //blah.GetConn().Wait();
            //blah.DoInsert(blah.GetConn()).Wait();
            //RedisBenchmarker blah = new RedisBenchmarker();
            blah.memOrDisk = "Disk";
            Console.WriteLine(blah.memOrDisk);
            blah.DoInserts().Wait();
            blah.RebuildIndex();
            blah.DoGets().Wait();
        }
    }
}
