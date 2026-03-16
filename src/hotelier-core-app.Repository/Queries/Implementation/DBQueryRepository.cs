using Autofac.Features.Indexed;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Domain.Queries.Interface;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace hotelier_core_app.Domain.Queries.Implementation
{

    public class DBQueryRepository<TEntity> : IDBQueryRepository<TEntity>, IBaseQueryRepository<TEntity> where TEntity : class
    {
        private readonly IIndex<DBProvider, IDBQueryRepository<TEntity>> _queryRepositories;
        private IDBQueryRepository<TEntity> _queryRepository;

        private readonly IConfiguration _configuration;

        public DBQueryRepository(IIndex<DBProvider, IDBQueryRepository<TEntity>> queryRepositories, IConfiguration configuration)
        {
            _configuration = configuration;
            _queryRepositories = queryRepositories;
            if (!Enum.TryParse<DBProvider>(_configuration.GetValue<string>("AppSettings:OrmType"), ignoreCase: true, out var result))
            {
                result = DBProvider.SQL_EFCore;
            }

            _queryRepository = queryRepositories[result];
        }

        /// <summary>
        /// Switches the database provider for the repository.
        /// </summary>
        /// <param name="provider">The database provider to switch to.</param>
        public void SwitchProvider(DBProvider provider)
        {
            if (!_queryRepositories.TryGetValue(provider, out var newRepository))
            {
                throw new InvalidOperationException($"Provider {provider} is not registered.");
            }

            _queryRepository = newRepository;
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
        /// Asynchronously finds an entity by its identifier using the current provider.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>A task representing the asynchronous operation, with the found entity.</returns>
        public async Task<TEntity?> FindAsync(object id)
        {
            return await _queryRepository.FindAsync(id);
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
        /// Gets all entities using the current provider.
        /// </summary>
        /// <returns>An enumerable of all entities.</returns>
        public IEnumerable<TEntity> GetAll()
        {
            return _queryRepository.GetAll();
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
        /// Asynchronously gets all entities using the current provider.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with an enumerable of all entities.</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _queryRepository.GetAllAsync();
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
        /// Gets entities matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>An enumerable of matching entities.</returns>
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryRepository.GetBy(predicate);
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
        /// Asynchronously gets entities matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities.</returns>
        public async Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _queryRepository.GetByAsync(predicate);
        }

        /// <summary>
        /// Asynchronously gets a limited number of entities matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="limit">The maximum number of entities to return.</param>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities.</returns>
        public async Task<IEnumerable<TEntity>> GetByWithLimitAsync(int limit, Expression<Func<TEntity, bool>> predicate)
        {
            return await _queryRepository.GetByWithLimitAsync(limit, predicate);
        }

        /// <summary>
        /// Asynchronously gets entities matching the specified predicate, including related entities, using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities with included properties.</returns>
        public async Task<IEnumerable<TEntity>> GetByIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            return await _queryRepository.GetByIncludesAsync(predicate, map, includes);
        }

        /// <summary>
        /// Asynchronously gets a limited number of entities matching the specified predicate, including related entities, using the current provider.
        /// </summary>
        /// <param name="limit">The maximum number of entities to return.</param>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities with included properties.</returns>
        public async Task<IEnumerable<TEntity>> GetByIncludesWithLimitAsync<T1>(int limit, Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            return await _queryRepository.GetByIncludesWithLimitAsync(limit, predicate, map, includes);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate, including related entities, using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity with included properties.</returns>
        public async Task<TEntity?> GetByDefaultIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            return await _queryRepository.GetByDefaultIncludesAsync(predicate, map, includes);
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
        /// Gets the first entity matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>The first matching entity.</returns>
        public TEntity GetByDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryRepository.GetByDefault(predicate);
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
        /// Asynchronously gets the first entity matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity.</returns>
        public async Task<TEntity?> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _queryRepository.GetByDefaultAsync(predicate);
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
        /// Checks if any entity exists matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>True if any entity exists, otherwise false.</returns>
        public bool IsExist(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryRepository.IsExist(predicate);
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
        /// Asynchronously checks if any entity exists matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with true if any entity exists, otherwise false.</returns>
        public async Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _queryRepository.IsExistAsync(predicate);
        }

        /// <summary>
        /// Asynchronously gets the count of entities matching the specified predicate using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the count of matching entities.</returns>
        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _queryRepository.GetCountAsync(predicate);
        }

        /// <summary>
        /// Asynchronously gets the next value in the specified database sequence using the current provider.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>A task representing the asynchronous operation, with the next value in the sequence.</returns>
        public async Task<int> GetNextValueInSequenceAsync(string sequenceName)
        {
            return await _queryRepository.GetNextValueInSequenceAsync(sequenceName);
        }

        /// <summary>
        /// Asynchronously gets the next value in the specified database sequence using the given connection string.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the next value in the sequence.</returns>
        public async Task<int> GetNextValueInSequenceAsync(string sequenceName, string connectionString)
        {
            return await _queryRepository.GetNextValueInSequenceAsync(sequenceName, connectionString);
        }

        /// <summary>
        /// Gets all entities as a queryable collection using the current provider.
        /// </summary>
        /// <returns>A queryable collection of all entities.</returns>
        public IQueryable<TEntity> GetAllQueryable()
        {
            return _queryRepository.GetAllQueryable();
        }

        /// <summary>
        /// Gets all entities as a queryable collection with tracking enabled using the current provider.
        /// </summary>
        /// <returns>A queryable collection of all entities with tracking.</returns>
        public IQueryable<TEntity> GetAllTrackEntity()
        {
            return _queryRepository.GetAllTrackEntity();
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate with no tracking using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>The first matching entity, or null if none found.</returns>
        public TEntity? GetByDefaultAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryRepository.GetByDefaultAsNoTracking(predicate);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate with no tracking using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity, or null if none found.</returns>
        public async Task<TEntity?> GetByDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _queryRepository.GetByDefaultAsNoTrackingAsync(predicate);
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate, including related entities, using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>The first matching entity with included properties, or null if none found.</returns>
        public TEntity? GetByDefaultIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return _queryRepository.GetByDefaultIncluding(predicate, includeProperties);
        }

        /// <summary>
        /// Gets all entities, including related entities, as a queryable collection using the current provider.
        /// </summary>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>A queryable collection of all entities with included properties.</returns>
        public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return _queryRepository.GetAllIncluding(includeProperties);
        }

        /// <summary>
        /// Gets all entities, including related entities, as an enumerable using the current provider.
        /// </summary>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>An enumerable of all entities with included properties.</returns>
        public IEnumerable<TEntity> AllInclude(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return _queryRepository.AllInclude(includeProperties);
        }

        /// <summary>
        /// Finds entities matching the specified predicate, including related entities, using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>An enumerable of matching entities with included properties.</returns>
        public IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return _queryRepository.FindByInclude(predicate, includeProperties);
        }

        /// <summary>
        /// Gets entities matching the specified predicate, including related entities, as a queryable collection using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>A queryable collection of matching entities with included properties.</returns>
        public IQueryable<TEntity> GetByAllIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return _queryRepository.GetByAllIncluding(predicate, includeProperties);
        }

        /// <summary>
        /// Finds an entity matching the specified predicate, including child entities, using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>The first matching entity with child entities, or null if none found.</returns>
        public TEntity? FindWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return _queryRepository.FindWithChildInclude(predicate, includeMembers);
        }

        /// <summary>
        /// Gets entities matching the specified predicate, including child entities, as a queryable collection using the current provider.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>A queryable collection of matching entities with child entities.</returns>
        public IQueryable<TEntity> GetAllByWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return _queryRepository.GetAllByWithChildInclude(predicate, includeMembers);
        }

        /// <summary>
        /// Gets all entities, including child entities, as a queryable collection using the current provider.
        /// </summary>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>A queryable collection of all entities with child entities.</returns>
        public IQueryable<TEntity> GetAllWithChildInclude(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return _queryRepository.GetAllWithChildInclude(includeMembers);
        }

        /// <summary>
        /// Gets entities using a stored procedure using the current provider.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to execute.</param>
        /// <param name="parameter">The parameters for the stored procedure.</param>
        /// <returns>A queryable collection of entities returned by the stored procedure.</returns>
        public IQueryable<TEntity> GetRecordUsingStoredProcedure(string storedProcedure, object[] parameter)
        {
            return _queryRepository.GetRecordUsingStoredProcedure(storedProcedure, parameter);
        }
    }
}
