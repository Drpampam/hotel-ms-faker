using Npgsql;

namespace hotelier_core_app.Domain.Executers
{
    public interface IExecuters : IAutoDependencyRepository
    {
        void ExecuteCommand(string connStr, Action<NpgsqlConnection, NpgsqlTransaction> task);

        T ExecuteCommand<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, T> task);

        Task<T> ExecuteCommandAsync<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> task);

        dynamic ExecuteCommand<T>(string connStr, string query, object param);

        Task<dynamic> ExecuteCommandAsync<T>(string connStr, string query, object param);

        IEnumerable<T> ExecuteReader<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, IEnumerable<T>> task);

        IEnumerable<T> ExecuteReader<T>(string connStr, string query, object param);

        Task<IEnumerable<T>> ExecuteReaderAsync<T>(string connStr, string query, object param);

        Task<IEnumerable<T>> ExecuteReaderWithIncludeAsync<T, T1>(string connStr, string query, Func<T, T1, T> map, object param);

        Task<T> ExecuteSingleReaderAsync<T>(string connStr, string query, object param);

        Task<IEnumerable<T>> ExecuteReaderAsync<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> task);

        Task<dynamic> ExecuteCommandAsync<T>(string query, object param, NpgsqlTransaction sqlTransaction);

        dynamic ExecuteCommand<T>(string query, object param, NpgsqlTransaction sqlTransaction);
    }
}
