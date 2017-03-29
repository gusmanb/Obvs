using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obvs.Extensions;
using Microsoft.Extensions.DependencyModel;

namespace Obvs.Configuration
{
    public static class MessageTypes
    {
        public static IEnumerable<Type> Get<TMessage, TServiceMessage>(Func<Assembly, bool> assemblyFilter = null, Func<Type, bool> typeFilter = null)
            where TMessage : class
            where TServiceMessage : class
        {
            assemblyFilter = assemblyFilter ?? (assembly => true);
            typeFilter = typeFilter ?? (type => true);

            var types = GetAssemblies()
                .Where(assemblyFilter)
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(typeFilter)
                .Where(t => t.IsValidMessageType<TMessage, TServiceMessage>())
                .ToArray();

            EnsureTypesAreVisible(types);

            return types.ToArray();
        }

        private static Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (IsCandidateCompilationLibrary(library))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }

            return assemblies.ToArray();
        }

        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            var name = typeof(MessageTypes).GetTypeInfo().Assembly.GetName().Name;

            return compilationLibrary.Name == name
                || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith(name));
        }

        private static void EnsureTypesAreVisible(IEnumerable<Type> types)
        {
            var notVisible = types.Where(t => !t.GetTypeInfo().IsVisible).ToArray();

            if (notVisible.Any())
            {
                throw new Exception(
                    "The following message types are not visible so Obvs will not be able to deserialize them. Please mark as public: " + Environment.NewLine +
                    string.Join(Environment.NewLine, notVisible.Select(t => string.Format("- {0}", t.FullName))));
            }
        }
        
    }
}