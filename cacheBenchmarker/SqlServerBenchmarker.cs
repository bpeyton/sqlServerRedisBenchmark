using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using cfg = System.Configuration.ConfigurationManager;
using System.Threading.Tasks;
using System.Collections.Concurrent;
//using System.Data.sqlc
namespace cacheBenchmarker
{
    class SqlServerBenchmarker
    {
        ConcurrentBag<string> guids = new ConcurrentBag<string>();


        public SqlConnection GetConn()
        {
            SqlConnection conn = new SqlConnection(cfg.ConnectionStrings["sqlConn"].ConnectionString);
            conn.Open();
            return conn;
        }

        public void DoInsert(SqlConnection conn)
        {
            Random random = new Random();
            SqlCommand cmd = new SqlCommand("insert into cacheDisk (cacheKey, cacheValue) values (@cacheKey, @cacheValue);", conn);
            string guidStr = Guid.NewGuid().ToString();
            guids.Add(guidStr);
            cmd.Parameters.AddWithValue("cacheKey", guidStr);
            cmd.Parameters.AddWithValue("cacheValue", random.Next().ToString());
            cmd.ExecuteNonQuery();
            
        }


        public async Task<string> DoGet(Task<SqlConnection> conn)
        {
            
            string cacheKey;
            guids.TryPeek(out cacheKey);
            SqlCommand cmd = new SqlCommand("select cacheValue from cache where cacheKey=@cacheKey;", await conn);
            cmd.Parameters.AddWithValue("cacheKey", cacheKey);
            string val = (string)(await cmd.ExecuteScalarAsync());
            return val;
        }


        public void DoInserts()
        {
            DateTime startTime = DateTime.Now;
            const int inserts = 100000;
            //const int numParallel = 10000;
            Parallel.For(0, inserts, x =>
            {
                //for (int i = 0; i < inserts / numParallel; i++)
                //{
                    using (SqlConnection conn = GetConn())
                    {
                        DoInsert(conn);
                        //insertTask.Wait();
                    }
                    //tasks.Add(insertTask);
                //}
               // Task.WhenAll(tasks).Wait();
            });
            DateTime endTime = DateTime.Now;
            double seconds = (endTime - startTime).TotalSeconds;
            double rowsPerSecond = inserts / seconds;
            Console.WriteLine($"{inserts} rows written.. {seconds} seconds... {rowsPerSecond} rows per second");
        }

    }
}
