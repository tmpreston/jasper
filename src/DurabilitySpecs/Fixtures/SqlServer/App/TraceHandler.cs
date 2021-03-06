﻿using System.Data.SqlClient;
using Jasper;
using Jasper.SqlServer;
using Jasper.SqlServer.Util;


namespace DurabilitySpecs.Fixtures.SqlServer.App
{
    [JasperIgnore]
    public class TraceHandler
    {
        [SqlTransaction]
        public void Handle(TraceMessage message, SqlTransaction tx)
        {
            var traceDoc = new TraceDoc{Name = message.Name};

            tx.Connection.CreateCommand(tx, "insert into receiver.trace_doc (id, name) values (@id, @name)")
                .With("id", traceDoc.Id)
                .With("name", traceDoc.Name)
                .ExecuteNonQuery();
        }
    }
}
