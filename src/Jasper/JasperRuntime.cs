using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.EnvironmentChecks;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Lamar;
using Lamar.Codegen.Variables;
using Lamar.Scanning.Conventions;
using Lamar.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper
{
    public partial class JasperRuntime : IDisposable
    {
        private readonly Lazy<IMessageContext> _bus;
        private bool isDisposing;
        private static ApplicationLifetime _lifetime;


        private JasperRuntime(JasperRegistry registry, PerfTimer timer)
        {
            Bootstrapping = timer;



            registry.CodeGeneration.Sources.Add(new NowTimeVariableSource());

            registry.CodeGeneration.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            registry.CodeGeneration.Assemblies.Add(registry.ApplicationAssembly);

            Registry = registry;

            _bus = new Lazy<IMessageContext>(Get<IMessageContext>);
        }

        public PerfTimer Bootstrapping { get; }

        internal JasperRegistry Registry { get; }

        /// <summary>
        ///     The main application assembly for the running application
        /// </summary>
        public Assembly ApplicationAssembly => Registry.ApplicationAssembly;

        /// <summary>
        /// The underlying Lamar container
        /// </summary>
        public IContainer Container { get; private set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Summary of all the message handling, subscription, and message publishing
        ///     capabilities of the running Jasper application
        /// </summary>
        public ServiceCapabilities Capabilities { get; internal set; }

        public string[] HttpAddresses { get; private set; } = new string[0];

        /// <summary>
        ///     Shortcut to retrieve an instance of the IServiceBus interface for the application
        /// </summary>
        public IMessageContext Messaging => _bus.Value;

        /// <summary>
        ///     The logical name of the application from JasperRegistry.ServiceName
        /// </summary>
        public string ServiceName => Registry.ServiceName;

        /// <summary>
        ///     Information about the running service node as published to service discovery
        /// </summary>
        public IServiceNode Node { get; internal set; }



        /// <summary>
        ///     Creates a Jasper application for the current executing assembly
        ///     using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static JasperRuntime Basic()
        {
            return BasicAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Creates a Jasper application for the current executing assembly
        ///     using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static Task<JasperRuntime> BasicAsync()
        {
            return bootstrap(new JasperRegistry());
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static JasperRuntime For(JasperRegistry registry)
        {
            return ForAsync(registry).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static Task<JasperRuntime> ForAsync(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the JasperRegistry of
        ///     type T
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The type of your JasperRegistry</typeparam>
        /// <returns></returns>
        public static JasperRuntime For<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            return ForAsync<T>(configure).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the JasperRegistry of
        ///     type T
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The type of your JasperRegistry</typeparam>
        /// <returns></returns>
        public static Task<JasperRuntime> ForAsync<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return bootstrap(registry);
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static JasperRuntime For(Action<JasperRegistry> configure)
        {
            return ForAsync(configure).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Task<JasperRuntime> ForAsync(Action<JasperRegistry> configure)
        {
            var registry = new JasperRegistry();
            configure?.Invoke(registry);

            return bootstrap(registry);
        }

        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return Container.GetInstance<T>();
        }

        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get(Type type)
        {
            return Container.GetInstance(type);
        }

        /// <summary>
        ///     Writes a textual report about the configured transports and servers
        ///     for this application
        /// </summary>
        /// <param name="writer"></param>
        public void Describe(TextWriter writer)
        {
            writer.WriteLine($"Running service '{ServiceName}'");
            if (ApplicationAssembly != null) writer.WriteLine("Application Assembly: " + ApplicationAssembly.FullName);

            var hosting = Container.TryGetInstance<IHostingEnvironment>();

            if (hosting != null)
            {
                writer.WriteLine($"Hosting environment: {hosting.EnvironmentName}");
                writer.WriteLine($"Content root path: {hosting.ContentRootPath}");
            }

            var hosted = Container.GetAllInstances<IHostedService>();
            foreach (var hostedService in hosted)
            {
                writer.WriteLine("Hosted Service: " + hostedService);
            }

            Registry.Describe(this, writer);
        }
    }
}
