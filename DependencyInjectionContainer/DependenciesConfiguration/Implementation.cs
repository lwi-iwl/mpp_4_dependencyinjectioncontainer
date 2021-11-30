using System;

namespace DependencyInjectionContainer.DependenciesConfiguration
{
    public class Implementation
    {
        public Type ImplementationType { get; private set; }
        public LifeTime LifeTime { get; private set; }
        public ServiceImplementation ServiceImplementation { get; private set; }

        public Implementation(Type implementationType, LifeTime lifeTime, ServiceImplementation serviceImplementation)
        {
            ImplementationType = implementationType;
            LifeTime = lifeTime;
            ServiceImplementation = serviceImplementation;
        }
    }
}