using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using Jasper.Marten;
using Jasper.Marten.Codegen;
using Jasper.Messaging.Runtime.Subscriptions;
using Marten;
using Microsoft.Extensions.DependencyInjection;

// SAMPLE: MartenExtension
[assembly:JasperModule(typeof(MartenExtension))]

namespace Jasper.Marten
{
    public class MartenExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Alter<StoreOptions>(x =>
            {
                x.Schema.For<ServiceCapabilities>().Identity(_ => _.ServiceName);
            });

            registry.Services.AddSingleton<IDocumentStore>(x =>
            {
                var storeOptions = x.GetService<StoreOptions>();
                var documentStore = new DocumentStore(storeOptions);
                return documentStore;
            });

            registry.Handlers.GlobalPolicy<FineGrainedSessionCreationPolicy>();


            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().OpenSession());
            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().QuerySession());

            registry.CodeGeneration.Sources.Add(new SessionVariableSource());

        }
    }


}
// ENDSAMPLE
