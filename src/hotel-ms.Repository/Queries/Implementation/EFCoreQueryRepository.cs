using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Migrations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace hotelier_core_app.Domain.Queries.Implementation
{
    public class EFCoreQueryRepository<TEntity> : IDBQueryRepository<TEntity>, IBaseQueryRepository<TEntity> where TEntity : class
    {
        private AppDbContext _context;
        private DbSet<TEntity> _dbSet;

        public EFCoreQueryRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Finds an entity by its identifier using the specified connection string.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The found entity.</returns>
        public TEntity Find(object id, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The found entity.</returns>
        public TEntity Find(object id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously finds an entity by its identifier using the specified connection string.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the found entity.</returns>
        public Task<TEntity> FindAsync(object id, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously finds an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>A task representing the asynchronous operation, with the found entity.</returns>
        public async Task<TEntity?> FindAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Gets all entities using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>An enumerable of all entities.</returns>
        public IEnumerable<TEntity> GetAll(string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>An enumerable of all entities.</returns>
        public IEnumerable<TEntity> GetAll()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        /// <summary>
        /// Asynchronously gets all entities using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of all entities.</returns>
        public Task<IEnumerable<TEntity>> GetAllAsync(string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets all entities.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with an enumerable of all entities.</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Gets all entities as a queryable collection.
        /// </summary>
        /// <returns>A queryable collection of all entities.</returns>
        public IQueryable<TEntity> GetAllQueryable()
        {
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        /// Gets all entities as a queryable collection with tracking enabled.
        /// </summary>
        /// <returns>A queryable collection of all entities with tracking.</returns>
        public IQueryable<TEntity> GetAllTrackEntity()
        {
            return _dbSet;
        }

        /// <summary>
        /// Gets entities matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>An enumerable of matching entities.</returns>
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>An enumerable of matching entities.</returns>
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).AsQueryable();
        }

        /// <summary>
        /// Asynchronously gets entities matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities.</returns>
        public Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities.</returns>
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The first matching entity.</returns>
        public TEntity GetByDefault(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>The first matching entity, or null if none found.</returns>
        public TEntity? GetByDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity.</returns>
        public Task<TEntity> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity, or null if none found.</returns>
        public async Task<TEntity?> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate with no tracking.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>The first matching entity, or null if none found.</returns>
        public TEntity? GetByDefaultAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.AsNoTracking().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate with no tracking.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity, or null if none found.</returns>
        public async Task<TEntity?> GetByDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity, or null if none found.</returns>
        public Task<TEntity?> GetByDefaultIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Gets the first entity matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>The first matching entity, or null if none found.</returns>
        public TEntity? GetByDefaultIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var Query = GetAllIncluding(includeProperties);

            return Query.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets all entities, including related entities.
        /// </summary>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>A queryable collection of all entities with included properties.</returns>
        public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> Queryable = _dbSet.AsNoTracking();

            return includeProperties.Aggregate
              (Queryable, (current, includeProperty) => current.Include(includeProperty));
        }

        /// <summary>
        /// Gets all entities, including related entities, as an enumerable.
        /// </summary>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>An enumerable of all entities with included properties.</returns>
        public IEnumerable<TEntity> AllInclude(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return GetAllIncluding(includeProperties).ToList();
        }

        /// <summary>
        /// Finds entities matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>An enumerable of matching entities with included properties.</returns>
        public IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var Query = GetAllIncluding(includeProperties);
            IEnumerable<TEntity> results = Query.Where(predicate).ToList();
            return results;
        }

        /// <summary>
        /// Gets entities matching the specified predicate, including related entities, as a queryable collection.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>A queryable collection of matching entities with included properties.</returns>
        public IQueryable<TEntity> GetByAllIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> Queryable = _dbSet.AsNoTracking().Where(predicate);

            return includeProperties.Aggregate
              (Queryable, (current, includeProperty) => current.Include(includeProperty));
        }

        /// <summary>
        /// Finds an entity matching the specified predicate, including child entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>The first matching entity with child entities, or null if none found.</returns>
        public TEntity? FindWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            DbSet<TEntity> set = _context.Set<TEntity>();
            IQueryable<TEntity> result = includeMembers(set.AsNoTracking());

            return result.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets entities matching the specified predicate, including child entities, as a queryable collection.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>A queryable collection of matching entities with child entities.</returns>
        public IQueryable<TEntity> GetAllByWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            DbSet<TEntity> set = _context.Set<TEntity>();
            IQueryable<TEntity> result = includeMembers(set.AsNoTracking());

            return result.Where(predicate).AsQueryable<TEntity>();
        }

        /// <summary>
        /// Gets all entities, including child entities, as a queryable collection.
        /// </summary>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>A queryable collection of all entities with child entities.</returns>
        public IQueryable<TEntity> GetAllWithChildInclude(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            DbSet<TEntity> set = _context.Set<TEntity>();
            IQueryable<TEntity> result = includeMembers(set.AsNoTracking());

            return result;
        }

        /// <summary>
        /// Gets entities using a stored procedure.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to execute.</param>
        /// <param name="parameter">The parameters for the stored procedure.</param>
        /// <returns>A queryable collection of entities returned by the stored procedure.</returns>
        public IQueryable<TEntity> GetRecordUsingStoredProcedure(string storedProcedure, object[] parameter)
        {
            return _dbSet.FromSqlRaw(storedProcedure, parameter);
        }

        /// <summary>
        /// Asynchronously gets entities matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities with included properties.</returns>
        public Task<IEnumerable<TEntity>> GetByIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets a limited number of entities matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="limit">The maximum number of entities to return.</param>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities with included properties.</returns>
        public Task<IEnumerable<TEntity>> GetByIncludesWithLimitAsync<T1>(int limit, Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets a limited number of entities matching the specified predicate.
        /// </summary>
        /// <param name="limit">The maximum number of entities to return.</param>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities.</returns>
        public async Task<IEnumerable<TEntity>> GetByWithLimitAsync(int limit, Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().Where(predicate).Take(limit).ToListAsync();
        }

        /// <summary>
        /// Asynchronously gets the count of entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the count of matching entities.</returns>
        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().CountAsync(predicate);
        }

        /// <summary>
        /// Asynchronously gets the next value in the specified database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>A task representing the asynchronous operation, with the next value in the sequence.</returns>
        public Task<int> GetNextValueInSequenceAsync(string sequenceName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets the next value in the specified database sequence using the given connection string.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the next value in the sequence.</returns>
        public Task<int> GetNextValueInSequenceAsync(string sequenceName, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if any entity exists matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>True if any entity exists, otherwise false.</returns>
        public bool IsExist(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if any entity exists matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>True if any entity exists, otherwise false.</returns>
        public bool IsExist(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.AsNoTracking().Any(predicate);
        }

        /// <summary>
        /// Asynchronously checks if any entity exists matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with true if any entity exists, otherwise false.</returns>
        public Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously checks if any entity exists matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with true if any entity exists, otherwise false.</returns>
        public async Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().AnyAsync(predicate);
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
