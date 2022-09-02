namespace Database
{
    public interface ILocalDatabase
    {
        Task<int> Command(string query, Dictionary<string, object>? parameters = null);
        void CreateIfNotExists(string sql);
        Task<T?> Get<T>(string query, Dictionary<string, object>? parameters = null);
        Task<List<T>?> GetAll<T>(string query, Dictionary<string, object>? parameters = null);
    }
}