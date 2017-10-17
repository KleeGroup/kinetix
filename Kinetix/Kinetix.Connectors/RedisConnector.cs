using System;
using System.Diagnostics;
using StackExchange.Redis;

namespace Kinetix.Connectors {
    public class RedisConnector {

        private ConnectionMultiplexer Redis;
        private IDatabase RedisDb;

        public RedisConnector(string RedisHost, int RedisPort, int? RedisDatabase = null, bool allowAdmin = false, string PasswordOption = null) {
            Debug.Assert(!String.IsNullOrEmpty(RedisHost));
            //Debug.Assert(PasswordOption != null);
            Debug.Assert(!RedisDatabase.HasValue || (RedisDatabase >= 0 && RedisDatabase < 16), String.Format("there 16 DBs(0 - 15); your index database '{0}' is not inside this range", RedisDatabase));
            // ---

            string allowAdminString = allowAdmin ? ",allowAdmin=true" : string.Empty;

            Redis = ConnectionMultiplexer.Connect(RedisHost + ":" + RedisPort + allowAdminString);

            if (RedisDatabase.HasValue) {
                RedisDb = Redis.GetDatabase(RedisDatabase.Value);
            }
        }

        public ConnectionMultiplexer GetMultiplexer() {
            return Redis;
        }

        public IDatabase GetResource() {
            return RedisDb;
        }
    }
}
