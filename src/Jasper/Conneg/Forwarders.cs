using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging;

namespace Jasper.Conneg
{
    public class Forwarders
    {
        private readonly LightweightCache<Type, List<Type>> _forwarders =
            new LightweightCache<Type, List<Type>>(t => new List<Type>());

        public void Add(Type type)
        {
            if (!type.HasAttribute<VersionAttribute>())
                throw new ArgumentOutOfRangeException(nameof(type),
                    $"'Forwarding' type {type.FullName} must be decorated with a {typeof(VersionAttribute).FullName} attribute to denote the message version");

            var forwardedType = type
                .FindInterfaceThatCloses(typeof(IForwardsTo<>))
                .GetGenericArguments()
                .Single();

            _forwarders[forwardedType].Add(type);
        }

        public IReadOnlyList<Type> ForwardingTypesTo(Type handledType)
        {
            return _forwarders.Has(handledType) ? _forwarders[handledType] : new List<Type>();
        }
    }
}
