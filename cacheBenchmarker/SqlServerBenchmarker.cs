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


        public async Task<SqlConnection> GetConn()
        {
            SqlConnection conn = new SqlConnection(cfg.ConnectionStrings["sqlConn"].ConnectionString);
            await conn.OpenAsync();
            return conn;
        }

        public async Task DoInsert(Task<SqlConnection> conn)
        {
            Random random = new Random();
            SqlCommand cmd = new SqlCommand("insert into cache (cacheKey, cacheValue) values (@cacheKey, @cacheValue);", await conn);
            string guidStr = Guid.NewGuid().ToString();
            guids.Add(guidStr);
            cmd.Parameters.AddWithValue("cacheKey", guidStr);
            cmd.Parameters.AddWithValue("cacheValue", random.Next().ToString());
            await cmd.ExecuteNonQueryAsync();
            
        }


        public async Task DoGet(Task<SqlConnection> conn)
        {
            string cacheKey;
            guids.TryPeek(out cacheKey);
            SqlCommand cmd = new SqlCommand("select cacheValue from cache where cacheKey=@cacheKey;", await conn);
            cmd.Parameters.AddWithValue("cacheKey", cacheKey);
            string val = (string)(await cmd.ExecuteScalarAsync());



        }


        public async Task DoInserts()
        {
            Task<SqlConnection> conn = GetConn();
            for (int i = 0; i < 10000; i++)
            {
                await DoInsert(conn);
            }

            Console.WriteLine("10000 rows written");
        }

    }
}
