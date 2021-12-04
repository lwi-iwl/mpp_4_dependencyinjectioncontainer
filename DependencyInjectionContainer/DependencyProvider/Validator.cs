using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencyInjectionContainer.Configuration;

namespace DependencyInjectionContainer.DependencyProvider
{
    
    public class Validator
    {
        public DependenciesConfiguration DependenciesConfiguration { get; set; }


        public bool Validate()
        {
            bool isValid = true;
            foreach (Type dependency in DependenciesConfiguration.Dependencies.Keys.ToList())
            {
                isValid = isValid && IsValid(dependency);
            }

            return isValid;
        }

        public bool IsValid(Type dependency)
        {
            bool isValid = true;
            if (dependency.GetGenericArguments().ToList().Count == 0)
            {
                isValid = IsNonGenericValid(dependency);
            }
            else
            {
                isValid = IsGenericValid(dependency);
            }

            return isValid;
        }

        private bool IsConstructorValid(Implementation implementation)
        {
            List<ConstructorInfo> constructors = implementation.ImplementationType.GetConstructors().ToList();
            bool isValid = false;
            foreach (ConstructorInfo constructor in constructors)
            {
                List<ParameterInfo> parameters = constructor.GetParameters().ToList();
                bool isParametersValid = true;
                foreach (ParameterInfo parameter in parameters)
                {
                    if (!parameter.ParameterType.IsGenericParameter)
                    {
                        int index = DependenciesConfiguration.Dependencies.Keys.ToList()
                            .FindIndex(x => x == parameter.ParameterType);
                        if (index == -1)
                            isParametersValid = false;
                        else
                            isParametersValid = IsValid(DependenciesConfiguration.Dependencies.Keys.ToList()[index]);
                    }
                }
                isValid = isValid || isParametersValid;
            }
            return isValid;
        }

        public bool IsNonGenericValid(Type dependency)
        {
            bool isValid = true;
            if (DependenciesConfiguration.Dependencies.Keys.ToList().Exists(x => x.FullName == dependency.FullName))
                foreach (Implementation implementation in DependenciesConfiguration.Dependencies[dependency])
                {
                    isValid = isValid && implementation.ImplementationType.IsAssignableFrom(dependency) ||
                              implementation
                                  .ImplementationType.GetInterfaces()
                                  .Any(x => x.ToString() == dependency.ToString());
                    isValid = isValid && IsConstructorValid(implementation);
                }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        public bool IsGenericValid(Type dependency)
        {
            bool isValid = true;
            string dependencyName = dependency.FullName.Split('[')[0];
            if (DependenciesConfiguration.Dependencies.Keys.ToList().Exists(x => x.FullName.Split('[')[0] == dependencyName))
            {
                Type depend = DependenciesConfiguration.Dependencies.Keys.ToList().Find(x => x.FullName == dependencyName);
                if (depend != null)
                {           
                    foreach (Implementation implementation in DependenciesConfiguration.Dependencies[depend])
                    {
                        isValid = isValid && implementation.ImplementationType.IsAssignableFrom(dependency) ||
                                  implementation
                                      .ImplementationType.GetInterfaces()
                                      .Any(x => x.ToString().Split('[')[0] == dependency.ToString().Split('[')[0]);
                        isValid = isValid && IsConstructorValid(implementation);
                    }
                }
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }
    }
}