﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Marten.Persistence;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Testing.Messaging;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
{
    public class MartenEnvelopePersistorTests : IDisposable
    {
        public JasperRuntime theRuntime = JasperRuntime.For<ItemReceiver>();

        public MartenEnvelopePersistorTests()
        {
            var store = theRuntime.Get<IDocumentStore>();
            store.Advanced.Clean.CompletelyRemoveAll();
            store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact]
        public async Task get_counts()
        {
            var thePersistor = theRuntime.Get<MartenEnvelopePersistor>();

            var list = new List<Envelope>();

            // 10 incoming
            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());



            // 7 scheduled
            list.Clear();
            for (int i = 0; i < 7; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Scheduled;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());


            // 3 outgoing
            list.Clear();
            for (int i = 0; i < 3; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 0);

            var counts = await thePersistor.GetPersistedCounts();

            counts.Incoming.ShouldBe(10);
            counts.Scheduled.ShouldBe(7);
            counts.Outgoing.ShouldBe(3);
        }
    }
}
