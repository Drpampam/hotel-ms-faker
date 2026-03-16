using hotelier_core_app.Domain.Helpers;
using System.Linq.Expressions;

namespace hotelier_core_app.Domain.Queries.Interface
{
    public interface IDBQueryRepository<TEntity> : IBaseQueryRepository<TEntity> where TEntity : class
    {
        void SwitchProvider(DBProvider provider);
        TEntity Find(object id, string connectionString);

        Task<TEntity> FindAsync(object id, string connectionString);

        IEnumerable<TEntity> GetAll(string connectionString);

        Task<IEnumerable<TEntity>> GetAllAsync(string connectionString);

        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> predicate, string connectionString);

        Task<IEnumerable<TEntity>> GetByAsync(Expression<Func<TEntity, bool>> predicate, string connectionString);

        TEntity GetByDefault(Expression<Func<TEntity, bool>> predicate, string connectionString);

        Task<TEntity?> GetByDefaultAsync(Expression<Func<TEntity, bool>> predicate, string connectionString);

        bool IsExist(Expression<Func<TEntity, bool>> predicate, string connectionString);

        Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate, string connectionString);

        Task<int> GetNextValueInSequenceAsync(string sequenceName);

        Task<int> GetNextValueInSequenceAsync(string sequenceName, string connectionString);
    }
}
