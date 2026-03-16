using System.Linq.Expressions;

namespace hotelier_core_app.Domain.SqlGenerator
{
    public interface ISqlGenerator<TEntity> : IAutoDependencyRepository where TEntity : class
    {
        SqlQuery GetInsertQuery(TEntity entity);

        object GetInsertQueryParams(TEntity entity);

        SqlQuery GetSelectQuery(Expression<Func<TEntity, bool>> predicate, int limit = 0, params Expression<Func<TEntity, object>>[] includes);

        SqlQuery GetSelectAllQuery(int limit = 0, params Expression<Func<TEntity, object>>[] includes);

        SqlQuery GetUniqueSelectQuery(TEntity entity);

        SqlQuery GetBulkInsertQuery(IEnumerable<TEntity> entities);

        SqlQuery GetBulkUpdateQuery(IEnumerable<TEntity> entities);

        SqlQuery GetUpdateQuery(TEntity entity);

        SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate);

        SqlQuery GetSelectById(object id);
    }
}
