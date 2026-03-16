using System.Linq.Expressions;

namespace hotelier_core_app.Domain.Queries.Interface
{
    public interface IBaseQueryRepository<TEntity> where TEntity : class
    {
        TEntity Find(object id);

        Task<TEntity?> FindAsync(object id);

        IEnumerable<TEntity> GetAll();

        Task<IEnumerable<TEntity>> GetAllAsync();

        IQueryable<TEntity> GetAllQueryable();

        IQueryable<TEntity> GetAllTrackEntity();

        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> GetByWithLimitAsync(int limit, Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> GetByIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes);

        Task<IEnumerable<TEntity>> GetByIncludesWithLimitAsync<T1>(int limit, Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes);

        TEntity? GetByDefault(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity?> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        TEntity? GetByDefaultAsNoTracking(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity?> GetByDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate);

        TEntity? GetByDefaultIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties);

        IEnumerable<TEntity> AllInclude(params Expression<Func<TEntity, object>>[] includeProperties);

        IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> GetByAllIncluding(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity? FindWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        IQueryable<TEntity> GetAllByWithChildInclude(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        IQueryable<TEntity> GetAllWithChildInclude(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        IQueryable<TEntity> GetRecordUsingStoredProcedure(string storedProcedure, object[] parameter);

        Task<TEntity?> GetByDefaultIncludesAsync<T1>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T1, TEntity> map, params Expression<Func<TEntity, object>>[] includes);

        bool IsExist(Expression<Func<TEntity, bool>> predicate);

        Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate);

        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
