using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace hotelier_core_app.Migrations
{
    /// <summary>
    /// Produces a model cache key that includes tenant schema to prevent cross-tenant model reuse.
    /// </summary>
    public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            if (context is AppDbContext appDbContext)
            {
                return (context.GetType(), appDbContext.CurrentSchema, designTime);
            }

            return (context.GetType(), designTime);
        }
    }
}
