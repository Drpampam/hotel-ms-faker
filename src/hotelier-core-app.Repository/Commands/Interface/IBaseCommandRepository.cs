namespace hotelier_core_app.Domain.Commands.Interface
{
    public interface IBaseCommandRepository<TEntity> where TEntity : class
    {
        object AddSoft(TEntity entity);

        Task<object?> AddSoftAsync(TEntity entity);

        object Add(TEntity entity);

        Task<object> AddAsync(TEntity entity);

        void AddRange(List<TEntity> entity);

        Task AddRangeAsync(List<TEntity> entity);

        void Update(TEntity entity);

        Task UpdateAsync(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        Task UpdateRangeAsync(IEnumerable<TEntity> entities);

        void Delete(object id);

        Task DeleteAsync(object id);

        void Delete(TEntity entity);

        void DeleteRange(List<TEntity> entity);

        void AttachEntity(TEntity entity);

        int Save();

        Task<int> SaveAsync();
    }
}
