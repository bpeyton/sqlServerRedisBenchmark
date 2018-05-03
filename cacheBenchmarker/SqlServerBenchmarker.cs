using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using cfg = System.Configuration.ConfigurationManager;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
//using System.Data.sqlc
namespace cacheBenchmarker
{
    class SqlServerBenchmarker
    {
        ConcurrentBag<string> guids = new ConcurrentBag<string>();
        public string memOrDisk = "Mem";
        int numInserts, numReads;
        const int numParallel = 100;

        public SqlServerBenchmarker(int numInserts, int numReads)
        {
            this.numInserts = numInserts;
            this.numReads = numReads;
        }

        public async Task<SqlConnection> GetConn()
        {
            SqlConnection conn = new SqlConnection(cfg.ConnectionStrings["sqlConn"].ConnectionString);
            await conn.OpenAsync();
            return conn;
        }

        public void DeleteData()
        {
            using (SqlConnection conn = GetConn().Result)
            {
                SqlCommand cmd = new SqlCommand("delete from cache" + memOrDisk, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public async Task DoInsert(SqlConnection conn, bool addToBag)
        {
            Random random = new Random();
            //SqlCommand cmd = new SqlCommand("insert into cacheDisk (cacheKey, cacheValue) values (@cacheKey, @cacheValue);", conn);
            SqlCommand cmd = new SqlCommand("insertCache" + memOrDisk, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            string guidStr = Guid.NewGuid().ToString();
            if (addToBag)
            {
                guids.Add(guidStr);
            }
            cmd.Parameters.AddWithValue("cacheKey", guidStr);
            cmd.Parameters.AddWithValue("cacheValue", random.Next().ToString());
            await cmd.ExecuteNonQueryAsync();
            return;
        }


        public async Task<string> DoGet(SqlConnection conn, string cacheKey)
        {
            SqlCommand cmd = new SqlCommand("getCache" + memOrDisk, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("cacheKey", cacheKey);
            string val = (string)(await cmd.ExecuteScalarAsync());
            return val;
        }




        public List<string> GetGuids()
        {
            using (SqlConnection conn = GetConn().Result)
            {
                SqlCommand cmd = new SqlCommand($"select top ${numReads} cacheKey from cacheMem;", conn);
                
                //cmd.Parameters.AddWithValue("cacheKey", cacheKey);
                //string val = (string)(await cmd.ExecuteScalarAsync());
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<string> retval = new List<string>(numReads);
                    while (reader.Read())
                    {
                        retval.Add((string)reader[0]);
                    }
                    return retval;
                }

            }
        }

        public async Task DoInserts()
        {
            DateTime startTime = DateTime.Now;
            //const int numChunck 
            SemaphoreSlim slim = new SemaphoreSlim(numParallel);
            using (SqlConnection conn = await GetConn())
            {
                List<Task> tasks = new List<Task>(numInserts);
                for (int i = 0; i < numInserts; i++)
                {

                    //int nextChunk = i + numChunk;
                    //for (;i < nextChunk; i++)
                    //{
                    await slim.WaitAsync();
                    tasks.Add(DoInsert(conn, i % 10 == 0).ContinueWith(x => slim.Release()));
                    //}

                    

                }
                await Task.WhenAll(tasks);

            }
            //tasks.ForEach(conn => { conn.Dispose(); });

            DateTime endTime = DateTime.Now;
            double seconds = (endTime - startTime).TotalSeconds;
            double rowsPerSecond = numInserts / seconds;
            Console.WriteLine($"{numInserts} rows written.. {seconds} seconds... {rowsPerSecond} rows per second");
        }

        public void RebuildIndex()
        {
            
            const string sql = "ALTER INDEX [cacheDisk_primaryKey] ON [dbo].[cacheDisk] REBUILD PARTITION = ALL WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, RESUMABLE = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)";
            using (SqlConnection conn =  GetConn().Result)
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
        }
        

        public async Task DoGets()
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (SqlConnection conn = await GetConn())
            {
                List<Task<string>> tasks = new List<Task<string>>(guids.Count);
                foreach (string x in guids)
                {

                    tasks.Add(DoGet(conn, x));
                }
                await Task.WhenAll(tasks);
            }

            stopwatch.Stop();
            double seconds = stopwatch.Elapsed.TotalSeconds;
            double rowsPerSecond = guids.Count / seconds;
            Console.WriteLine($"{guids.Count} rows read.. {seconds} seconds... {rowsPerSecond} rows per second");
        }

    }
}
