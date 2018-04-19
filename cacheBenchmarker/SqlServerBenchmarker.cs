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
        public string memOrDisk = "Mem";

        public async Task<SqlConnection> GetConn()
        {
            SqlConnection conn = new SqlConnection(cfg.ConnectionStrings["sqlConn"].ConnectionString);
            await conn.OpenAsync();
            return conn;
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


            //SqlCommand cmd = new SqlCommand("select cacheValue from cacheMem where cacheKey=@cacheKey;", conn);
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
                SqlCommand cmd = new SqlCommand("select top 100000 cacheKey from cacheMem;", conn);
                
                //cmd.Parameters.AddWithValue("cacheKey", cacheKey);
                //string val = (string)(await cmd.ExecuteScalarAsync());
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<string> retval = new List<string>(100000);
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
            const int inserts = 1000000;
            //const int inserts = 100;
            //const int inserts = 2000000;
            const int numChunk = 1000;
            
            using (SqlConnection conn = await GetConn())
            {
                //List<SqlConnection> sqlConnections = new List<SqlConnection>(inserts);
                for (int i = 0; i < inserts; i++)
                {
                    List<Task> tasks = new List<Task>(inserts);
                    int nextChunk = i + numChunk;
                    for (;i < nextChunk; i++)
                    {
                        tasks.Add(DoInsert(conn, i % 10 == 0));
                    }

                    //sqlConnections.Add(conn);
                    await Task.WhenAll(tasks);

                }

                
            }
            //tasks.ForEach(conn => { conn.Dispose(); });

            DateTime endTime = DateTime.Now;
            double seconds = (endTime - startTime).TotalSeconds;
            double rowsPerSecond = inserts / seconds;
            Console.WriteLine($"{inserts} rows written.. {seconds} seconds... {rowsPerSecond} rows per second");
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
            //List<string> guids = GetGuids();
            Console.WriteLine("Got guids");
            DateTime startTime = DateTime.Now;

            //foreach (string x in guids)
            //Parallel.ForEach(guids, x =>
            //{

            const string strGuid = "4e57a384-1ab5-4e8c-afea-08598cf95127";

            using (SqlConnection conn = await GetConn())
            {
                List<Task<string>> tasks = new List<Task<string>>(guids.Count);
                foreach (string x in guids)
                {

                    tasks.Add(DoGet(conn, x));
                }
                await Task.WhenAll(tasks);
            }
            //});

            DateTime endTime = DateTime.Now;
            double seconds = (endTime - startTime).TotalSeconds;
            double rowsPerSecond = guids.Count / seconds;
            Console.WriteLine($"{guids.Count} rows read.. {seconds} seconds... {rowsPerSecond} rows per second");
        }

    }
}
