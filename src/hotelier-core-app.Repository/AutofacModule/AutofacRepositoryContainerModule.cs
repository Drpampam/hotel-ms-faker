using Autofac;
using hotelier_core_app.Domain.Commands.Implementation;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Domain.Queries.Implementation;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Domain.SqlGenerator;

namespace hotelier_core_app.Domain.AutofacModule
{
    public class AutofacRepositoryContainerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(DBCommandRepository<>))
               .As(typeof(IDBCommandRepository<>))
               .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(DBQueryRepository<>))
               .As(typeof(IDBQueryRepository<>))
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(IAutoDependencyRepository).Assembly)
                .AssignableTo<IAutoDependencyRepository>()
                .As<IAutoDependencyRepository>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(SqlGenerator<>))
                .As(typeof(ISqlGenerator<>))
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(DapperDBCommandRepository<>))
                .Keyed(DBProvider.SQL_Dapper, typeof(IDBCommandRepository<>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(DapperDBQueryRepository<>))
                .Keyed(DBProvider.SQL_Dapper, typeof(IDBQueryRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(EFCoreCommandRepository<>))
                .Keyed(DBProvider.SQL_EFCore, typeof(IDBCommandRepository<>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(EFCoreQueryRepository<>))
                .Keyed(DBProvider.SQL_EFCore, typeof(IDBQueryRepository<>))
                .InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
