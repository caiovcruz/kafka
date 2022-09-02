using Dapper;
using System.Data;
using System.Data.SQLite;

namespace Database
{
    public class LocalDatabase : ILocalDatabase
    {
        private string _connectionString;

        public LocalDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateIfNotExists(string query)
        {
            try
            {
                using (var sqliteConnection = new SQLiteConnection(_connectionString))
                {
                    sqliteConnection.Open();

                    sqliteConnection.ExecuteAsync(query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<int> Command(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using (var sqliteConnection = new SQLiteConnection(_connectionString))
                {
                    sqliteConnection.Open();

                    return await sqliteConnection.ExecuteAsync(query, GetParameters(parameters));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        public async Task<T?> Get<T>(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using (var sqliteConnection = new SQLiteConnection(_connectionString))
                {
                    sqliteConnection.Open();

                    var result = await sqliteConnection.QueryFirstOrDefaultAsync<T>(query, GetParameters(parameters));

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default;
        }

        public async Task<List<T>?> GetAll<T>(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                var sqliteConnection = new SQLiteConnection(_connectionString);
                {
                    sqliteConnection.Open();

                    var result = await sqliteConnection.QueryAsync<T>(query, GetParameters(parameters));

                    return result?.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default;
        }

        private DynamicParameters GetParameters(Dictionary<string, object>? parameters = null)
        {
            var dynamicParameters = new DynamicParameters();

            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return dynamicParameters;
        }
    }
}
