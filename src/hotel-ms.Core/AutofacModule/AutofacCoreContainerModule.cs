using Autofac;

namespace hotelier_core_app.Core.AutofacModule
{
    /// <summary>
    /// Autofac module for registering Core project dependencies.
    /// </summary>
    public class AutofacCoreContainerModule : Module
    {
        /// <summary>
        /// Registers all types in the Core assembly that implement <see cref="IAutoDependencyCore"/> for dependency injection.
        /// </summary>
        /// <param name="builder">The Autofac container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IAutoDependencyCore).Assembly)
                .AssignableTo<IAutoDependencyCore>()
                .As<IAutoDependencyCore>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
