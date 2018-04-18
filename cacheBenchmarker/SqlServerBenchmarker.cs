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


        public string DoGet(SqlConnection conn, string cacheKey)
        {


            //SqlCommand cmd = new SqlCommand("select cacheValue from cacheMem where cacheKey=@cacheKey;", conn);
            SqlCommand cmd = new SqlCommand("getCacheDisk", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("cacheKey", cacheKey);
            string val = (string)(cmd.ExecuteScalar());
            return val;
        }




        public List<string> GetGuids()
        {
            using (SqlConnection conn = GetConn())
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

        public void DoInserts()
        {
            DateTime startTime = DateTime.Now;
            const int inserts = 1000000;
            //const int inserts = 2000000;
            //const int numParallel = 10000;
            Parallel.For(0, inserts, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, x =>
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

        public void DoGets()
        {
            List<string> guids = GetGuids();
            Console.WriteLine("Got guids");
            DateTime startTime = DateTime.Now;

            //foreach (string x in guids)
            Parallel.ForEach(guids, x =>
            {

                using (SqlConnection conn = GetConn())
                {


                    //DoInsert(conn);
                    DoGet(conn, x);
                }
            });

            DateTime endTime = DateTime.Now;
            double seconds = (endTime - startTime).TotalSeconds;
            double rowsPerSecond = guids.Count / seconds;
            Console.WriteLine($"{guids.Count} rows read.. {seconds} seconds... {rowsPerSecond} rows per second");
        }

    }
}
