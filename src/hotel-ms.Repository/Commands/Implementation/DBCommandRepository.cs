using Autofac.Features.Indexed;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace hotelier_core_app.Domain.Commands.Implementation
{

    public class DBCommandRepository<TEntity> : IDBCommandRepository<TEntity>, IBaseCommandRepository<TEntity> where TEntity : class
    {
        private readonly IIndex<DBProvider, IDBCommandRepository<TEntity>> _commandRepositories;
        private IDBCommandRepository<TEntity> _commandRepository;

        private readonly IConfiguration _configuration;

        public DBCommandRepository(IIndex<DBProvider, IDBCommandRepository<TEntity>> commandRepositories, IConfiguration configuration)
        {
            _commandRepositories = commandRepositories;
            _configuration = configuration;
            if (!Enum.TryParse<DBProvider>(_configuration.GetValue<string>("AppSettings:OrmType"), ignoreCase: true, out var result))
            {
                result = DBProvider.SQL_EFCore;
            }

            _commandRepository = commandRepositories[result];
        }

        /// <summary>
        /// Switches the database provider for the repository.
        /// </summary>
        /// <param name="provider">The database provider to switch to.</param>
        public void SwitchProvider(DBProvider provider)
        {
            if (!_commandRepositories.TryGetValue(provider, out var newRepository))
            {
                throw new InvalidOperationException($"Provider {provider} is not registered.");
            }

            _commandRepository = newRepository;
        }

        /// <summary>
        /// Adds a new entity with soft logic using the current provider.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public object AddSoft(TEntity entity)
        {
            return _commandRepository.AddSoft(entity);
        }

        /// <summary>
        /// Asynchronously adds a new entity with soft logic using the current provider.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public async Task<object?> AddSoftAsync(TEntity entity)
        {
            return await _commandRepository.AddSoftAsync(entity);
        }

        /// <summary>
        /// Adds a new entity using the current provider.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public object Add(TEntity entity)
        {
            return _commandRepository.Add(entity);
        }

        /// <summary>
        /// Asynchronously adds a new entity using the current provider.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public async Task<object> AddAsync(TEntity entity)
        {
            return await _commandRepository.AddAsync(entity);
        }

        /// <summary>
        /// Adds a range of entities using the current provider.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        public void AddRange(List<TEntity> entity)
        {
            _commandRepository.AddRange(entity);
        }

        /// <summary>
        /// Asynchronously adds a range of entities using the current provider.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRangeAsync(List<TEntity> entity)
        {
            await _commandRepository.AddRangeAsync(entity);
        }

        /// <summary>
        /// Adds a range of entities within a transaction.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        public void AddRangeWithTransaction(List<TEntity> entity, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously adds a range of entities within a transaction.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task AddRangeWithTransactionAsync(List<TEntity> entity, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a new entity within a transaction.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        public void AddWithTransaction(TEntity entity, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously adds a new entity within a transaction.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction)
        {
            await _commandRepository.AddWithTransactionAsync(entity, transaction);
        }

        /// <summary>
        /// Begins a new database transaction using the current provider.
        /// </summary>
        /// <returns>The started transaction.</returns>
        public NpgsqlTransaction BeginTransaction()
        {
            return _commandRepository.BeginTransaction();
        }

        /// <summary>
        /// Begins a new database transaction using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The started transaction.</returns>
        public NpgsqlTransaction BeginTransaction(string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously begins a new database transaction using the current provider.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the started transaction.</returns>
        public Task<NpgsqlTransaction> BeginTransactionAsync()
        {
            return _commandRepository.BeginTransactionAsync();
        }

        /// <summary>
        /// Asynchronously begins a new database transaction using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the started transaction.</returns>
        public Task<NpgsqlTransaction> BeginTransactionAsync(string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Commits the specified transaction using the current provider.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to commit.</param>
        public void CommitTransaction(NpgsqlTransaction sqlTransaction)
        {
            _commandRepository.CommitTransaction(sqlTransaction);
        }

        /// <summary>
        /// Asynchronously commits the specified transaction using the current provider.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to commit.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CommitTransactionAsync(NpgsqlTransaction sqlTransaction)
        {
            await _commandRepository.CommitTransactionAsync(sqlTransaction);
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
        public Task DeleteAsync(object id)
        {
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
        public Task DeleteWithTransactionAsync(object id, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rolls back the specified transaction using the current provider.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to roll back.</param>
        public void RollBackTransaction(NpgsqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously rolls back the specified transaction using the current provider.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to roll back.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RollBackTransactionAsync(NpgsqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves changes made in the context to the database using the current provider.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public int Save()
        {
            return _commandRepository.Save();
        }

        /// <summary>
        /// Asynchronously saves changes made in the context to the database using the current provider.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the number of affected rows.</returns>
        public async Task<int> SaveAsync()
        {
            return await _commandRepository.SaveAsync();
        }

        /// <summary>
        /// Updates the specified entity using the current provider.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public void Update(TEntity entity)
        {
            _commandRepository.Update(entity);
        }

        /// <summary>
        /// Asynchronously updates the specified entity using the current provider.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(TEntity entity)
        {
            await _commandRepository.UpdateAsync(entity);
        }

        /// <summary>
        /// Updates the specified entity within a transaction.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        public void UpdateWithTransaction(TEntity entity, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously updates the specified entity within a transaction.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="transaction">The transaction to use for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction)
        {
            await _commandRepository.UpdateWithTransactionAsync(entity, transaction);
        }

        /// <summary>
        /// Updates a range of entities using the current provider.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _commandRepository.UpdateRange(entities);
        }

        /// <summary>
        /// Asynchronously updates a range of entities using the current provider.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            await _commandRepository.UpdateRangeAsync(entities);
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
        /// Deletes the specified entity using the current provider.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(TEntity entity)
        {
            _commandRepository.Delete(entity);
        }

        /// <summary>
        /// Deletes a range of entities using the current provider.
        /// </summary>
        /// <param name="entity">The entities to delete.</param>
        public void DeleteRange(List<TEntity> entity)
        {
            _commandRepository.DeleteRange(entity);
        }

        /// <summary>
        /// Attaches the specified entity using the current provider.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        public void AttachEntity(TEntity entity)
        {
            _commandRepository.AttachEntity(entity);
        }
    }
}
