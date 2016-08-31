using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;
using System.Diagnostics;

namespace Kinetix.Connectors
{
    public class RedisConnector
    {
        private ConnectionMultiplexer Redis;
        private IDatabase RedisDb;

        public RedisConnector(string RedisHost, int RedisPort, int RedisDatabase, string PasswordOption)
        {
            Debug.Assert(!String.IsNullOrEmpty(RedisHost));
            Debug.Assert(PasswordOption != null);
            Debug.Assert(RedisDatabase >= 0 && RedisDatabase < 16, String.Format("there 16 DBs(0 - 15); your index database '{0}' is not inside this range", RedisDatabase));
            // ---
            Redis = ConnectionMultiplexer.Connect(RedisHost + ":" + RedisPort);
            RedisDb = Redis.GetDatabase(RedisDatabase);
        }


        public IDatabase getResource()
        {
            return RedisDb;
        }


    }
}
