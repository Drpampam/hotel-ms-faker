using Autofac;

namespace hotelier_core_app.Service.AutofacModule
{
    public class AutofacServiceContainerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IAutoDependencyService).Assembly)
                .AssignableTo<IAutoDependencyService>()
                .As<IAutoDependencyService>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
