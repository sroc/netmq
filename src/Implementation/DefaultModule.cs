using Autofac;
using Implementation.Actors;
using Implementation.Interfaces;

namespace Implementation
{
    public sealed class DefaultModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterDependencies(builder);
        }

        private static void RegisterDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<RouterActor>().As<IRouterActor>().InstancePerDependency();
            builder.RegisterType<DealerActor>().As<IDealerActor>().InstancePerDependency();
            builder.RegisterType<BroadcastMessage>().As<IBroadcastMessage>().InstancePerDependency();
        }
    }
}
