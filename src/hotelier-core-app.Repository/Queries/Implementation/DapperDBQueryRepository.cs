using hotelier_core_app.Domain.Executers;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Domain.SqlGenerator;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace hotelier_core_app.Domain.Queries.Implementation
{
    public class DapperDBQueryRepository<TEntity> : IDBQueryRepository<TEntity>, IBaseQueryRepository<TEntity> where TEntity : class
    {
        private readonly string? _connStr;

        private readonly IExecuters _executers;

        private readonly IConfiguration _configuration;

        private readonly ISqlGenerator<TEntity> _sqlGenerator;

        public DapperDBQueryRepository(IConfiguration configuration, IExecuters executers, ISqlGenerator<TEntity> sqlGenerator)
        {
            _configuration = configuration;
            _executers = executers;
            _connStr = _configuration.GetConnectionString("DbConnectionString");
            _sqlGenerator = sqlGenerator;
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
            SqlQuery selectById = _sqlGenerator.GetSelectById(id);
            if (_connStr == null)
                throw new InvalidOperationException("Database connection string is not configured.");
            return await _executers.ExecuteSingleReaderAsync<TEntity>(_connStr, selectById.GetSql(), selectById.Param);
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
            throw new NotImplementedException();
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
            SqlQuery selectAllQuery = _sqlGenerator.GetSelectAllQuery(0);
            return await _executers.ExecuteReaderAsync<TEntity>(_connStr, selectAllQuery.GetSql(), selectAllQuery.Param);
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
            throw new NotImplementedException();
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
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, 0);
            return await _executers.ExecuteReaderAsync<TEntity>(_connStr, selectQuery.GetSql(), selectQuery.Param);
        }

        /// <summary>
        /// Asynchronously gets a limited number of entities matching the specified predicate.
        /// </summary>
        /// <param name="limit">The maximum number of entities to return.</param>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities.</returns>
        public async Task<IEnumerable<TEntity>> GetByWithLimitAsync(int limit, Expression<Func<TEntity, bool>> predicate)
        {
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, limit);
            return await _executers.ExecuteReaderAsync<TEntity>(_connStr, selectQuery.GetSql(), selectQuery.Param);
        }

        /// <summary>
        /// Asynchronously gets entities matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities with included properties.</returns>
        public async Task<IEnumerable<TEntity>> GetByIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, 0, includes);
            return await _executers.ExecuteReaderWithIncludeAsync(_connStr, selectQuery.GetSql(), map, selectQuery.Param);
        }

        /// <summary>
        /// Asynchronously gets a limited number of entities matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="limit">The maximum number of entities to return.</param>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with an enumerable of matching entities with included properties.</returns>
        public async Task<IEnumerable<TEntity>> GetByIncludesWithLimitAsync<T1>(int limit, Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, limit, includes);
            return await _executers.ExecuteReaderWithIncludeAsync(_connStr, selectQuery.GetSql(), map, selectQuery.Param);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="map">The mapping function for related entities.</param>
        /// <param name="includes">The related entities to include.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity with included properties.</returns>
        public async Task<TEntity?> GetByDefaultIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes)
        {
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, 1, includes);
            if (_connStr == null)
                throw new InvalidOperationException("Database connection string is not configured.");
            return (await _executers.ExecuteReaderWithIncludeAsync(_connStr, selectQuery.GetSql(), map, selectQuery.Param)).FirstOrDefault();
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
        /// <returns>The first matching entity.</returns>
        public TEntity GetByDefault(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate using the given connection string.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity.</returns>
        public async Task<TEntity?> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate, string connectionString)
        {
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, 1);
            return await _executers.ExecuteSingleReaderAsync<TEntity>(connectionString, selectQuery.GetSql(), selectQuery.Param);
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity.</returns>
        public async Task<TEntity?> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, 1);
            if (_connStr == null)
                throw new InvalidOperationException("Database connection string is not configured.");
            return await _executers.ExecuteSingleReaderAsync<TEntity>(_connStr, selectQuery.GetSql(), selectQuery.Param);
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
            throw new NotImplementedException();
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
            SqlQuery selectQuery = _sqlGenerator.GetSelectQuery(predicate, 1);
            return await _executers.ExecuteSingleReaderAsync<bool>(_connStr, selectQuery.GetSql(), selectQuery.Param);
        }

        /// <summary>
        /// Asynchronously gets the count of entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the count of matching entities.</returns>
        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            SqlQuery count = _sqlGenerator.GetCount(predicate);
            return await _executers.ExecuteSingleReaderAsync<int>(_connStr, count.GetSql(), count.Param);
        }

        /// <summary>
        /// Asynchronously gets the next value in the specified database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>A task representing the asynchronous operation, with the next value in the sequence.</returns>
        public async Task<int> GetNextValueInSequenceAsync(string sequenceName)
        {
            if (_connStr == null)
                throw new InvalidOperationException("Database connection string is not configured.");
            return (await _executers.ExecuteReaderAsync<int>(_connStr, "SELECT NEXT VALUE FOR " + sequenceName, null!)).FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously gets the next value in the specified database sequence using the given connection string.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A task representing the asynchronous operation, with the next value in the sequence.</returns>
        public async Task<int> GetNextValueInSequenceAsync(string sequenceName, string connectionString)
        {
            return (await _executers.ExecuteReaderAsync<int>(connectionString, "SELECT NEXT VALUE FOR " + sequenceName, null!)).FirstOrDefault();
        }

        /// <summary>
        /// Gets all entities as a queryable collection.
        /// </summary>
        /// <returns>A queryable collection of all entities.</returns>
        public IQueryable<TEntity> GetAllQueryable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all entities as a queryable collection with tracking enabled.
        /// </summary>
        /// <returns>A queryable collection of all entities with tracking.</returns>
        public IQueryable<TEntity> GetAllTrackEntity()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate with no tracking.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>The first matching entity, or null if none found.</returns>
        public TEntity? GetByDefaultAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets the first entity matching the specified predicate with no tracking.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching entity, or null if none found.</returns>
        public Task<TEntity?> GetByDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the first entity matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>The first matching entity with included properties, or null if none found.</returns>
        public TEntity? GetByDefaultIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all entities, including related entities, as a queryable collection.
        /// </summary>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>A queryable collection of all entities with included properties.</returns>
        public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all entities, including related entities, as an enumerable.
        /// </summary>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>An enumerable of all entities with included properties.</returns>
        public IEnumerable<TEntity> AllInclude(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds entities matching the specified predicate, including related entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>An enumerable of matching entities with included properties.</returns>
        public IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets entities matching the specified predicate, including related entities, as a queryable collection.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeProperties">The related entities to include.</param>
        /// <returns>A queryable collection of matching entities with included properties.</returns>
        public IQueryable<TEntity> GetByAllIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds an entity matching the specified predicate, including child entities.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>The first matching entity with child entities, or null if none found.</returns>
        public TEntity? FindWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets entities matching the specified predicate, including child entities, as a queryable collection.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>A queryable collection of matching entities with child entities.</returns>
        public IQueryable<TEntity> GetAllByWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all entities, including child entities, as a queryable collection.
        /// </summary>
        /// <param name="includeMembers">The function to include child entities.</param>
        /// <returns>A queryable collection of all entities with child entities.</returns>
        public IQueryable<TEntity> GetAllWithChildInclude(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets entities using a stored procedure.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to execute.</param>
        /// <param name="parameter">The parameters for the stored procedure.</param>
        /// <returns>A queryable collection of entities returned by the stored procedure.</returns>
        public IQueryable<TEntity> GetRecordUsingStoredProcedure(string storedProcedure, object[] parameter)
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
