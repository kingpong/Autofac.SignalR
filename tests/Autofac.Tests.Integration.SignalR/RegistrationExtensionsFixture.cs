using System;
using System.Reflection;
using Autofac.Core;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using NUnit.Framework;

namespace Autofac.Tests.Integration.SignalR
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void RegisterHubsFindHubInterfaces()
        {
            var builder = new ContainerBuilder();

            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            Assert.That(container.IsRegistered<TestHub>(), Is.True);
        }

        [Test]
        public void HubRegistrationsAreExternallyOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            var container = builder.Build();

            var service = new TypedService(typeof(TestHub));
            IComponentRegistration registration;
            container.ComponentRegistry.TryGetRegistration(service, out registration);

            Assert.That(registration.Ownership, Is.EqualTo(InstanceOwnership.ExternallyOwned));
        }

        [Test]
        public void LifetimeScopedHubIsExternallyOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterHubWithLifetimeScope<TestHub>();
            var container = builder.Build();

            var service = new TypedService(typeof(TestHub));
            IComponentRegistration registration;
            container.ComponentRegistry.TryGetRegistration(service, out registration);

            Assert.That(registration.Ownership, Is.EqualTo(InstanceOwnership.ExternallyOwned));
        }

        [Test]
        public void LifetimeScopedHubCreatesImplicitScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterHubWithLifetimeScope<LifetimeScopedHub>();
            builder.RegisterType<DisposableComponent>().InstancePerLifetimeScope();
            using (var container = builder.Build())
            {
                var outerDisposable = container.Resolve<DisposableComponent>();
                DisposableComponent innerComponent;
                using (var hub = container.Resolve<LifetimeScopedHub>())
                {
                    innerComponent = hub.InnerDisposable;
                    Assert.That(innerComponent, Is.Not.SameAs(outerDisposable),
                        "implicit lifetime scope should require a new DisposableComponent");
                    Assert.That(innerComponent.IsDisposed, Is.False,
                        "the implicit scope has not been disposed yet, so its children should not be either");
                }
                Assert.That(innerComponent.IsDisposed, Is.True,
                    "the implicit scope has been disposed, which should cascade to the children");
                Assert.That(outerDisposable.IsDisposed, Is.False,
                    "the original scope has not been disposed, so its children should not be either");
            }
        }

        [Test]
        public void LifetimeScopedHubCachesClassDefinition()
        {
            Type t1, t2;

            var builder = new ContainerBuilder();
            builder.RegisterHubWithLifetimeScope<TestHub>();
            using (var container = builder.Build())
            {
                t1 = container.Resolve<TestHub>().GetType();
            }

            builder = new ContainerBuilder();
            builder.RegisterHubWithLifetimeScope<TestHub>();
            using (var container = builder.Build())
            {
                t2 = container.Resolve<TestHub>().GetType();
            }

            Assert.That(t1, Is.SameAs(t2),
                "multiple registrations for the same hub class should reuse the same proxy class");
        }
    }

    public class TestHub : Hub
    {
    }

    public sealed class DisposableComponent : IDisposable
    {
        public bool IsDisposed { get; set; }
        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class LifetimeScopedHub : Hub
    {
        public DisposableComponent InnerDisposable { get; private set; }

        public LifetimeScopedHub(DisposableComponent innerDisposable)
        {
            InnerDisposable = innerDisposable;
        }
    }
}