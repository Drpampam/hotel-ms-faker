using Dapper;
using hotelier_core_app.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace hotelier_core_app.Domain.Executers
{
    public class Executers : IExecuters, IAutoDependencyRepository
    {
        private readonly IConfiguration _configuration;

        public Executers(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ExecuteCommand(string connStr, Action<NpgsqlConnection, NpgsqlTransaction> task)
        {
            using NpgsqlConnection sqlConnection = new NpgsqlConnection(connStr);
            NpgsqlTransaction sqlTransaction = null;
            try
            {
                sqlConnection.Open();
                sqlTransaction = sqlConnection.BeginTransaction();
                task(sqlConnection, sqlTransaction);
                sqlTransaction.Commit();
            }
            catch (PostgresException)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                throw;
            }
            catch (Exception)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                throw;
            }
        }

        public T ExecuteCommand<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, T> task)
        {
            using NpgsqlConnection sqlConnection = new NpgsqlConnection(connStr);
            NpgsqlTransaction sqlTransaction = null;
            try
            {
                sqlConnection.Open();
                sqlTransaction = sqlConnection.BeginTransaction();
                T result = task(sqlConnection, sqlTransaction);
                sqlTransaction.Commit();
                return result;
            }
            catch (PostgresException)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                throw;
            }
            catch (Exception)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                throw;
            }
        }

        public async Task<T> ExecuteCommandAsync<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> task)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            NpgsqlTransaction _sqlTransaction = null;
            try
            {
                conn.Open();
                _sqlTransaction = conn.BeginTransaction();
                T result = await task(conn, _sqlTransaction);
                _sqlTransaction.Commit();
                return result;
            }
            catch (PostgresException)
            {
                if (_sqlTransaction?.Connection != null)
                {
                    _sqlTransaction.Rollback();
                }

                throw;
            }
            catch (Exception)
            {
                if (_sqlTransaction?.Connection != null)
                {
                    _sqlTransaction.Rollback();
                }

                throw;
            }
        }

        public dynamic ExecuteCommand<T>(string connStr, string query, object param)
        {
            using NpgsqlConnection sqlConnection = new NpgsqlConnection(connStr);
            NpgsqlTransaction sqlTransaction = null;
            try
            {
                sqlConnection.Open();
                sqlTransaction = sqlConnection.BeginTransaction();
                dynamic result = sqlConnection.QueryFirstOrDefault(query, param, sqlTransaction);
                sqlTransaction.Commit();
                return result;
            }
            catch (PostgresException)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                throw;
            }
            catch (Exception)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                throw;
            }
        }

        public async Task<dynamic> ExecuteCommandAsync<T>(string connStr, string query, object param)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            NpgsqlTransaction _sqlTransaction = null;
            try
            {
                conn.Open();
                _sqlTransaction = conn.BeginTransaction();
                object result = await conn.QueryFirstOrDefaultAsync(query, param, _sqlTransaction);
                _sqlTransaction.Commit();
                return result;
            }
            catch (PostgresException)
            {
                if (_sqlTransaction?.Connection != null)
                {
                    _sqlTransaction.Rollback();
                }

                throw;
            }
            catch (Exception)
            {
                if (_sqlTransaction?.Connection != null)
                {
                    _sqlTransaction.Rollback();
                }

                throw;
            }
        }

        public IEnumerable<T> ExecuteReader<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, IEnumerable<T>> task)
        {
            using NpgsqlConnection sqlConnection = new NpgsqlConnection(connStr);
            NpgsqlTransaction arg = null;
            try
            {
                sqlConnection.Open();
                return task(sqlConnection, arg);
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(string connStr, Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> task)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            NpgsqlTransaction arg = null;
            try
            {
                conn.Open();
                return await task(conn, arg);
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<T> ExecuteReader<T>(string connStr, string query, object param)
        {
            using NpgsqlConnection sqlConnection = new NpgsqlConnection(connStr);
            try
            {
                sqlConnection.Open();
                return sqlConnection.Query<T>(query, param, null, buffered: true, _configuration.GetValue<int>("AppSettings:DatabaseReadTimeout"));
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(string connStr, string query, object param)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            try
            {
                conn.Open();
                return await conn.QueryAsync<T>(query, param, null, _configuration.GetValue<int>("AppSettings:DatabaseReadTimeout"));
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> ExecuteReaderWithIncludeAsync<T, T1>(string connStr, string query, Func<T, T1, T> map, object param)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            try
            {
                conn.Open();
                return await conn.QueryAsync(query, map, param, null, buffered: true, "Id", _configuration.GetValue<int>("AppSettings:DatabaseReadTimeout"));
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T2>(string connStr, string query, object param)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            try
            {
                conn.Open();
                return await conn.QueryAsync<T>(query, param, null, _configuration.GetValue<int>("AppSettings:DatabaseReadTimeout"));
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<T> ExecuteSingleReaderAsync<T>(string connStr, string query, object param)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(connStr);
            try
            {
                conn.Open();
                return await conn.QueryFirstOrDefaultAsync<T>(query, param, null, _configuration.GetValue<int>("AppSettings:DatabaseReadTimeout"));
            }
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public dynamic ExecuteCommand<T>(string query, object param, NpgsqlTransaction sqlTransaction)
        {
            if (sqlTransaction == null)
            {
                throw new SqlTransactionNotInitializedException("Sql Transaction is not initialized");
            }

            try
            {
                return sqlTransaction.Connection.Execute(query, param, sqlTransaction);
            }
            catch (PostgresException)
            {
                sqlTransaction?.Rollback();
                throw;
            }
            catch (Exception)
            {
                sqlTransaction?.Rollback();
                throw;
            }
        }

        public async Task<dynamic> ExecuteCommandAsync<T>(string query, object param, NpgsqlTransaction sqlTransaction)
        {
            if (sqlTransaction == null)
            {
                throw new SqlTransactionNotInitializedException("Sql Transaction is not initialized");
            }

            try
            {
                return await sqlTransaction.Connection.ExecuteAsync(query, param, sqlTransaction);
            }
            catch (PostgresException)
            {
                sqlTransaction?.Rollback();
                throw;
            }
            catch (Exception)
            {
                sqlTransaction?.Rollback();
                throw;
            }
        }
    }
}
