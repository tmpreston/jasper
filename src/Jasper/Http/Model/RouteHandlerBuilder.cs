﻿using Jasper.Http.ContentHandling;
using Lamar;
using Lamar.Codegen;
using Lamar.Compilation;

namespace Jasper.Http.Model
{
    public class RouteHandlerBuilder
    {
        private readonly IContainer _container;
        private readonly ConnegRules _rules;
        private readonly GenerationRules _generation;

        public RouteHandlerBuilder(IContainer container, ConnegRules rules, GenerationRules generation)
        {
            _container = container;
            _rules = rules;
            _generation = generation;
        }

        public RouteHandler Build(RouteChain chain)
        {


            var generatedAssembly = new GeneratedAssembly(_generation);
            chain.AssemblyType(generatedAssembly, _rules);

            _container.CompileWithInlineServices(generatedAssembly);

            var handler = chain.CreateHandler(_container);
            handler.Chain = chain;


            return handler;
        }
    }
}
