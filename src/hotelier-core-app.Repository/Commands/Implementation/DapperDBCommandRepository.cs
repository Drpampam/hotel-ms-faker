using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Executers;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Domain.SqlGenerator;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Serilog;
using System.Reflection;

namespace hotelier_core_app.Domain.Commands.Implementation
{

    public class DapperDBCommandRepository<TEntity> : IDBCommandRepository<TEntity>, IBaseCommandRepository<TEntity> where TEntity : class
    {
        private readonly IConfiguration _configuration;
        private readonly IExecuters _executers;
        private readonly string? _connectionString;
        private readonly ISqlGenerator<TEntity> _sqlGenerator;

        private const int UniqueIndexExceptionNumber = 2601;
        private NpgsqlConnection? _connection = null;
        private NpgsqlTransaction? _transaction = null;

        public DapperDBCommandRepository(IConfiguration configuration, IExecuters executers, ISqlGenerator<TEntity> sqlGenerator)
        {
            _configuration = configuration;
            _executers = executers;
            _sqlGenerator = sqlGenerator;
            _connectionString = _configuration.GetConnectionString("DbConnectionString");
        }

        public object AddSoft(TEntity entity)
        {
            if (_connectionString == null)
                throw new InvalidOperationException("Database connection string is not configured.");
            IDictionary<string, object> obj = (IDictionary<string, object>)(object)_executers.ExecuteCommand<TEntity>(_connectionString, _sqlGenerator.GetInsertQuery(entity).GetSql(), _sqlGenerator.GetInsertQueryParams(entity));
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> item in obj)
            {
                dictionary[item.Key] = item.Value;
            }

            return dictionary["Id"];
        }

        public async Task<object?> AddSoftAsync(TEntity entity)
        {
            object? id = null;
            try
            {
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                dynamic val = await _executers.ExecuteCommandAsync<TEntity>(_connectionString, _sqlGenerator.GetInsertQuery(entity).GetSql(), _sqlGenerator.GetInsertQueryParams(entity));
                if ((object)val != null)
                {
                    IDictionary<string, object> obj = (IDictionary<string, object>)val;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, object> item in obj)
                    {
                        dictionary[item.Key] = item.Value;
                    }

                    id = dictionary["Id"];
                }

                return id;
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                {
                    SqlQuery uniqueSelectQuery = _sqlGenerator.GetUniqueSelectQuery(entity);
                    if (_connectionString == null)
                        throw new InvalidOperationException("Database connection string is not configured.");
                    TEntity obj2 = await _executers.ExecuteSingleReaderAsync<TEntity>(_connectionString, uniqueSelectQuery.GetSql(), uniqueSelectQuery.Param);
                    PropertyInfo[] properties = typeof(TEntity).GetProperties();
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        if (propertyInfo.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                        {
                            var value = propertyInfo.GetValue(obj2);
                            id = value != null ? value.ToString() : null;
                            break;
                        }
                    }

                    return id;
                }

                throw;
            }
        }

        public object Add(TEntity entity)
        {
            try
            {
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                object obj = _executers.ExecuteCommand<TEntity>(_connectionString, _sqlGenerator.GetInsertQuery(entity).GetSql(), _sqlGenerator.GetInsertQueryParams(entity));
                if (obj != null)
                {
                    IDictionary<string, object> obj2 = (IDictionary<string, object>)obj;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, object> item in obj2)
                    {
                        dictionary[item.Key] = item.Value;
                    }

                    return dictionary["Id"];
                }

                return null!;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public async Task<object> AddAsync(TEntity entity)
        {
            try
            {
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                dynamic val = await _executers.ExecuteCommandAsync<TEntity>(_connectionString, _sqlGenerator.GetInsertQuery(entity).GetSql(), _sqlGenerator.GetInsertQueryParams(entity));
                if ((object)val != null)
                {
                    IDictionary<string, object> obj = (IDictionary<string, object>)val;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, object> item in obj)
                    {
                        dictionary[item.Key] = item.Value;
                    }

                    return dictionary["Id"];
                }

                return null!;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public void AddRange(List<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public async Task AddRangeAsync(List<TEntity> entities)
        {
            try
            {
                SqlQuery bulkInsertQuery = _sqlGenerator.GetBulkInsertQuery(entities);
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                await _executers.ExecuteCommandAsync<TEntity>(_connectionString, bulkInsertQuery.GetSql(), bulkInsertQuery.Param);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public void AddRangeWithTransaction(List<TEntity> entity, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public async Task AddRangeWithTransactionAsync(List<TEntity> entity, NpgsqlTransaction transaction)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public void AddWithTransaction(TEntity entity, NpgsqlTransaction transaction)
        {
            try
            {
                _executers.ExecuteCommand<TEntity>(_sqlGenerator.GetInsertQuery(entity).GetSql(), _sqlGenerator.GetInsertQueryParams(entity), transaction);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public async Task AddWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction)
        {
            try
            {
                await _executers.ExecuteCommandAsync<TEntity>(_sqlGenerator.GetInsertQuery(entity).GetSql(), _sqlGenerator.GetInsertQueryParams(entity), transaction);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public NpgsqlTransaction BeginTransaction()
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
            return _connection.BeginTransaction();
        }

        public NpgsqlTransaction BeginTransaction(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
            return _connection.BeginTransaction();
        }

        public async Task<NpgsqlTransaction> BeginTransactionAsync()
        {
            _connection = new NpgsqlConnection(_connectionString);
            await _connection.OpenAsync();
            return await _connection.BeginTransactionAsync();
        }

        public async Task<NpgsqlTransaction> BeginTransactionAsync(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            await _connection.OpenAsync();
            return await _connection.BeginTransactionAsync();
        }

        public void CommitTransaction(NpgsqlTransaction sqlTransaction)
        {
            try
            {
                sqlTransaction.Commit();
            }
            catch
            {
                sqlTransaction.Rollback();
                throw;
            }
            finally
            {
                sqlTransaction.Dispose();
            }
        }

        public async Task CommitTransactionAsync(NpgsqlTransaction sqlTransaction)
        {
            try
            {
                await sqlTransaction.CommitAsync();
            }
            catch
            {
                await sqlTransaction.RollbackAsync();
                throw;
            }
            finally
            {
                await sqlTransaction.DisposeAsync();
            }
        }

        /// <summary>
        /// Deletes an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously deletes an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(object id)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an entity by its identifier within a transaction.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        public void DeleteWithTransaction(object id, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously deletes an entity by its identifier within a transaction.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteWithTransactionAsync(object id, NpgsqlTransaction transaction)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rolls back the specified transaction.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to roll back.</param>
        public void RollBackTransaction(NpgsqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously rolls back the specified transaction.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to roll back.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RollBackTransactionAsync(NpgsqlTransaction sqlTransaction)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(TEntity entity)
        {
            try
            {
                SqlQuery updateQuery = _sqlGenerator.GetUpdateQuery(entity);
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                await _executers.ExecuteCommandAsync<TEntity>(_connectionString, updateQuery.GetSql(), updateQuery.Param);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the specified entity within a transaction.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        public void UpdateWithTransaction(TEntity entity, NpgsqlTransaction transaction)
        {
            try
            {
                SqlQuery updateQuery = _sqlGenerator.GetUpdateQuery(entity);
                _executers.ExecuteCommand<TEntity>(_sqlGenerator.GetUpdateQuery(entity).GetSql(), updateQuery.Param, transaction);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates the specified entity within a transaction.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction)
        {
            try
            {
                SqlQuery updateQuery = _sqlGenerator.GetUpdateQuery(entity);
                await _executers.ExecuteCommandAsync<TEntity>(_sqlGenerator.GetUpdateQuery(entity).GetSql(), updateQuery.Param, transaction);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Updates a range of entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            try
            {
                SqlQuery bulkUpdateQuery = _sqlGenerator.GetBulkUpdateQuery(entities);
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                _executers.ExecuteCommand<TEntity>(_connectionString, bulkUpdateQuery.GetSql(), bulkUpdateQuery.Param);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates a range of entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                SqlQuery bulkUpdateQuery = _sqlGenerator.GetBulkUpdateQuery(entities);
                if (_connectionString == null)
                    throw new InvalidOperationException("Database connection string is not configured.");
                await _executers.ExecuteCommandAsync<TEntity>(_connectionString, bulkUpdateQuery.GetSql(), bulkUpdateQuery.Param);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public int Save()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously saves changes to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the number of affected rows.</returns>
        public async Task<int> SaveAsync()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a range of entities within a transaction.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        public void UpdateRangeWithTransaction(IEnumerable<TEntity> entities, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously updates a range of entities within a transaction.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateRangeWithTransactionAsync(IEnumerable<TEntity> entities, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a range of entities.
        /// </summary>
        /// <param name="entity">The entities to delete.</param>
        public void DeleteRange(List<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attaches the specified entity to the context.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        public void AttachEntity(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Switches the database provider.
        /// </summary>
        /// <param name="provider">The database provider to switch to.</param>
        public void SwitchProvider(DBProvider provider)
        {
        }
    }
}
