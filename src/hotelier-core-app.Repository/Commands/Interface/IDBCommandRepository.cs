using hotelier_core_app.Domain.Helpers;
using Npgsql;

namespace hotelier_core_app.Domain.Commands.Interface
{
    public interface IDBCommandRepository<TEntity> : IBaseCommandRepository<TEntity> where TEntity : class
    {
        void SwitchProvider(DBProvider provider);
        void AddWithTransaction(TEntity entity, NpgsqlTransaction transaction);

        Task AddWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction);

        void AddRangeWithTransaction(List<TEntity> entity, NpgsqlTransaction transaction);

        Task AddRangeWithTransactionAsync(List<TEntity> entity, NpgsqlTransaction transaction);

        void UpdateWithTransaction(TEntity entity, NpgsqlTransaction transaction);

        Task UpdateWithTransactionAsync(TEntity entity, NpgsqlTransaction transaction);

        void UpdateRangeWithTransaction(IEnumerable<TEntity> entities, NpgsqlTransaction transaction);

        Task UpdateRangeWithTransactionAsync(IEnumerable<TEntity> entities, NpgsqlTransaction transaction);

        void DeleteWithTransaction(object id, NpgsqlTransaction transaction);

        Task DeleteWithTransactionAsync(object id, NpgsqlTransaction transaction);

        NpgsqlTransaction BeginTransaction();

        Task<NpgsqlTransaction> BeginTransactionAsync();

        NpgsqlTransaction BeginTransaction(string connectionString);

        Task<NpgsqlTransaction> BeginTransactionAsync(string connectionString);

        void CommitTransaction(NpgsqlTransaction sqlTransaction);

        Task CommitTransactionAsync(NpgsqlTransaction sqlTransaction);

        void RollBackTransaction(NpgsqlTransaction sqlTransaction);

        Task RollBackTransactionAsync(NpgsqlTransaction sqlTransaction);
    }
}
