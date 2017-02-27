﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class MethodCall : Frame
    {
        public Type HandlerType { get; }
        public MethodInfo Method { get; }

        public static MethodCall For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new MethodCall(typeof(T), method);
        }

        private Variable[] _variables = new Variable[0];
        private Variable _target;


        // What's it got to know?
        // whether it returns a variable

        public MethodCall(Type handlerType, MethodInfo method) : base(method.IsAsync())
        {
            HandlerType = handlerType;
            Method = method;
        }

        public override void ResolveVariables(HandlerGeneration chain)
        {
            _variables = Method.GetParameters()
                .Select(param => chain.FindVariable(param.ParameterType))
                .ToArray();

            if (!Method.IsStatic)
            {
                _target = chain.FindVariable(HandlerType);
            }
        }

        public override void GenerateCode(HandlerGeneration generation, ISourceWriter writer)
        {
            var callingCode = $"{Method.Name}({_variables.Select(x => x.Name).Join(", ")})";
            var target = Method.IsStatic
                ? HandlerType.FullName
                : _target.Name;

            var returnValue = "";
            var suffix = "";

            if (IsAsync)
            {
                if (generation.AsyncMode == AsyncMode.ReturnFromLastNode)
                {
                    returnValue = "return ";
                }
                else
                {
                    // TODO -- going to need to see if it returns something
                    returnValue = "await ";
                }
            }

            // TODO -- will need to see if it's IDisposable too
            // TODO -- will have to deal with frames that declare a variable

            writer.Write($"{returnValue}{target}.{callingCode}{suffix};");

            Next?.GenerateAllCode(generation, writer);
        }


        public override bool CanReturnTask()
        {
            return IsAsync;
        }
    }
}