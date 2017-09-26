using System;
using System.Linq;
using System.Reflection;
using BlueMilk.Util;

namespace BlueMilk.Scanning.Conventions
{
    public class DefaultConventionScanner : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry registry)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes).Where(type => type.HasConstructors()))
            {
                var pluginType = FindPluginType(type);
                if (pluginType != null)
                {
                    registry.AddType(pluginType, type);
                }
            }
        }

        public virtual Type FindPluginType(Type concreteType)
        {
            var interfaceName = "I" + concreteType.Name;
            return concreteType.GetTypeInfo().GetInterfaces().FirstOrDefault(t => t.Name == interfaceName);
        }

        public override string ToString()
        {
            return "Default I[Name]/[Name] registration convention";
        }
    }
}
