using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using cfg = System.Configuration.ConfigurationManager;
using System.Threading.Tasks;
//using System.Data.sqlc
namespace cacheBenchmarker
{
    class SqlServerBenchmarker
    {

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
            cmd.Parameters.AddWithValue("cacheKey", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("cacheValue", random.Next().ToString());
            await cmd.ExecuteNonQueryAsync();

        }
    }
}
