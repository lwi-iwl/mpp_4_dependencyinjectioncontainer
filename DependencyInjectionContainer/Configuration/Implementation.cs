﻿using System;

namespace DependencyInjectionContainer.Configuration
{
    public class Implementation
    {
        public Type ImplementationType { get; private set; }
        public LifeTime LifeTime { get; private set; }

        public Implementation(Type implementationType, LifeTime lifeTime)
        {
            ImplementationType = implementationType;
            LifeTime = lifeTime;
        }
    }
}