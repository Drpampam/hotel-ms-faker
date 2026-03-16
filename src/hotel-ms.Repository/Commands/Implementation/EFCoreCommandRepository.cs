using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Migrations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace hotelier_core_app.Domain.Commands.Implementation
{
    public class EFCoreCommandRepository<TEntity> : IDBCommandRepository<TEntity>, IBaseCommandRepository<TEntity> where TEntity : class
    {
        private AppDbContext _context;
        private DbSet<TEntity> _dbSet;

        public EFCoreCommandRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();

        }

        /// <summary>
        /// Adds a new entity to the context.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public object Add(TEntity entity)
        {
            return _dbSet.Add(entity).Entity;
        }

        /// <summary>
        /// Asynchronously adds a new entity to the context.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public async Task<object> AddAsync(TEntity entity)
        {
            var valueTask = await _dbSet.AddAsync(entity);
            return valueTask.Entity;
        }

        /// <summary>
        /// Adds a range of entities to the context.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        public void AddRange(List<TEntity> entity)
        {
            _dbSet.AddRange(entity);
        }

        /// <summary>
        /// Asynchronously adds a range of entities to the context.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRangeAsync(List<TEntity> entity)
        {
            await _dbSet.AddRangeAsync(entity);
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
        /// Adds a new entity with soft logic (implementation required).
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public object AddSoft(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously adds a new entity with soft logic (implementation required).
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        public Task<object?> AddSoftAsync(TEntity entity)
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
        public Task AddWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <returns>The started transaction.</returns>
        public NpgsqlTransaction BeginTransaction()
        {
            throw new NotImplementedException();
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
        /// Asynchronously begins a new database transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the started transaction.</returns>
        public Task<NpgsqlTransaction> BeginTransactionAsync()
        {
            throw new NotImplementedException();
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
        /// Commits the specified transaction.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to commit.</param>
        public void CommitTransaction(NpgsqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously commits the specified transaction.
        /// </summary>
        /// <param name="sqlTransaction">The transaction to commit.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task CommitTransactionAsync(NpgsqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
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
        /// Deletes the specified entity from the context.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes a range of entities from the context.
        /// </summary>
        /// <param name="entity">The entities to delete.</param>
        public void DeleteRange(List<TEntity> entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.RemoveRange(entity);
        }

        /// <summary>
        /// Attaches the specified entity to the context.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        public void AttachEntity(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Attach(entity);
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
        public Task RollBackTransactionAsync(NpgsqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves changes made in the context to the database.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public int Save()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Asynchronously saves changes made in the context to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the number of affected rows.</returns>
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the specified entity in the context.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Asynchronously updates the specified entity in the context.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates a range of entities in the context.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously updates a range of entities in the context.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
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
        public Task UpdateWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction)
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
