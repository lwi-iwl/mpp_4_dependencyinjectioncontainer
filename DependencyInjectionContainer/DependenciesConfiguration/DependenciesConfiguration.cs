using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionContainer.DependenciesConfiguration
{
    public class DependenciesConfiguration
    {
        public Dictionary<Type, List<Implementation>> Dependencies { get; private set; }

        public DependenciesConfiguration()
        {
            Dependencies = new Dictionary<Type, List<Implementation>>();
        }

        public void Register<TDependency, TImplementation>(LifeTime lifetime,
            ServiceImplementation serviceImplementation = ServiceImplementation.None) where TDependency : class where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), lifetime, serviceImplementation);
        }

        public void Register(Type dependencyType, Type implementationType, LifeTime lifetime,
            ServiceImplementation serviceImplementation = ServiceImplementation.None)
        {
            List<Implementation> implementations;
            if (!Dependencies.ContainsKey(dependencyType))
            {
                Dependencies.Add(dependencyType, new List<Implementation>());
            }
            int index = Dependencies[dependencyType]
                .FindIndex(x => x.ImplementationType.FullName == implementationType.FullName);
            if (index != -1)
            {
                Dependencies[dependencyType].RemoveAt(index);
            }
            Dependencies[dependencyType].Add(new Implementation(implementationType, lifetime, serviceImplementation));

        }
    }
}