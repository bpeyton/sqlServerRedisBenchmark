using System;

namespace cacheBenchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SqlServerBenchmarker blah = new SqlServerBenchmarker();
            blah.GetConn().Wait();
            blah.DoInsert(blah.GetConn()).Wait();


        }
    }
}
