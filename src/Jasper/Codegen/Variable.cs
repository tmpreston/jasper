﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Util;

namespace Jasper.Codegen
{
    public abstract class Variable
    {
        public static Variable[] GatherAllDependencies(IEnumerable<Variable> variables)
        {
            var list = new List<Variable>(variables);

            foreach (var variable in variables)
            {
                variable.gather(list);
            }

            return list.TopologicalSort(v => v.Dependencies).ToArray();
        }

        private void gather(List<Variable> list)
        {
            foreach (var dependency in Dependencies)
            {
                if (list.Contains(dependency)) continue;

                list.Add(dependency);
                dependency.gather(list);
            }
        }

        // TODO -- change this. Too ugly. Strip out the initial "I" if an interface
        // Use camel casing instead
        public static string DefaultArgName(Type argType)
        {
            return argType.Name.SplitPascalCase().ToLower().Replace(" ", "_");
        }

        public Variable(Type argType, VariableCreation creation = VariableCreation.Injected)
            : this(argType, DefaultArgName(argType), creation)
        {
        }

        public Variable(Type argType, string name, VariableCreation creation)
        {
            Name = name;
            Creation = creation;
            VariableType = argType;
        }

        public string Name { get; }
        public VariableCreation Creation { get; }
        public Type VariableType { get; }

        public virtual IEnumerable<Variable> Dependencies
        {
            get
            {
                yield break;
            }
        }

        public virtual Frame CreateInstantiationFrame()
        {
            return null;
        }

    }
}