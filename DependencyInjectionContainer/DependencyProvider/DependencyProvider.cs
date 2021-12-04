using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencyInjectionContainer.Configuration;

namespace DependencyInjectionContainer.DependencyProvider
{
    public class DependencyProvider
    {
        private DependenciesConfiguration _dependenciesConfiguration;
        private Dictionary<Implementation, object> _singletones;
        private Validator _validator;

        public DependencyProvider(DependenciesConfiguration dependencies)
        {
            _validator = new Validator();
            _validator.DependenciesConfiguration = dependencies;
            if (!_validator.Validate())
            {
                throw new ArgumentException("Wrong configuration");
            }
            _dependenciesConfiguration = dependencies;
            _singletones = new Dictionary<Implementation, object>();
        }

        public IEnumerable<TDependency> Resolve<TDependency>(ServiceImplementation serviceImplementation = ServiceImplementation.Any)
            where TDependency : class
        {
            object[] resultWithoutType = Resolve(typeof(TDependency), serviceImplementation);
            TDependency[] result = new TDependency[resultWithoutType.Length];
            for (int i = 0; i < resultWithoutType.Length; i++)
            {
                result[i] = (TDependency) resultWithoutType[i];
            }
            return result;
        }
        
        public object[] Resolve(Type dependencyType, ServiceImplementation serviceImplementation)
        {
            List<object> result = new List<object>();
            List<object> tempResult = new List<object>();
            if (dependencyType.GetGenericArguments().ToList().Count == 0)
            {
                tempResult = NonGenericResolve(dependencyType);
            }
            else
            {
                tempResult = GenericResolve(dependencyType);
            }
            if (serviceImplementation == ServiceImplementation.Any)
            {
                result = tempResult;
            }
            else if (serviceImplementation == ServiceImplementation.First && tempResult.Count>0)
            {
                result.Add(tempResult[0]);
            }
            else if (serviceImplementation == ServiceImplementation.Second && tempResult.Count>1)
            {
                result.Add(tempResult[1]);
            }
            return result.ToArray();
        }

        public List<object> GenericResolve(Type dependencyType)
        {
            List<object> tempResult = new List<object>() ;
            string dependencyName = dependencyType.FullName.Split('[')[0];
            Type depend = _dependenciesConfiguration.Dependencies.Keys.ToList().Find(x => x.FullName.Split('[')[0] == dependencyName);
            if (depend != null)
            {
                foreach (Implementation implementation in _dependenciesConfiguration.Dependencies[depend])
                {
                    if (implementation.LifeTime == LifeTime.Singleton)
                    {
                        if (_singletones.ContainsKey(implementation))
                        {
                            tempResult.Add(_singletones[implementation]);
                        }
                        else
                        {
                            object newObject = GenerateGeneric(implementation, dependencyType);
                            if (newObject != null)
                            {
                                _singletones.Add(implementation, newObject);
                                tempResult.Add(_singletones[implementation]);
                            }
                        }
                    }
                    else
                    {
                        object newObject = GenerateGeneric(implementation, dependencyType);
                        if (newObject != null)
                            tempResult.Add(newObject);
                    }
                }
            }
            return tempResult;
        }

        public List<object> NonGenericResolve(Type dependencyType)
        {
            List<object> tempResult = new List<object>();
            if (_dependenciesConfiguration.Dependencies.Keys.ToList().Exists(x => x == dependencyType))
            {
                foreach (Implementation implementation in _dependenciesConfiguration.Dependencies[dependencyType])
                {
                    if (implementation.LifeTime == LifeTime.Singleton)
                    {
                        if (_singletones.ContainsKey(implementation))
                        {
                            tempResult.Add(_singletones[implementation]);
                        }
                        else
                        {
                            object newObject = GenerateNonGeneric(implementation);
                            if (newObject != null)
                            {
                                _singletones.Add(implementation, newObject);
                                tempResult.Add(_singletones[implementation]);
                            }
                        }
                    }
                    else
                    {
                        object newObject = GenerateNonGeneric(implementation);
                        if (newObject != null)
                            tempResult.Add(newObject);
                    }
                }
            }
            return tempResult;
        }

        private object GenerateGeneric(Implementation implementation, Type dependencyType)
        {
            Type implementationType;
            if (implementation.ImplementationType.GetGenericArguments()[0] !=
                dependencyType.GetGenericArguments()[0])
            {
                implementationType = implementation.ImplementationType.MakeGenericType(new Type[] { dependencyType.GetGenericArguments()[0] });
            }
            else
            {
                implementationType = implementation.ImplementationType;
            }
            ConstructorInfo implementationConstructor =
                implementationType.GetConstructors().ToList().Find(x =>
                    x.GetParameters().ToList().FindIndex(y =>
                        y.ParameterType == implementationType.GetGenericArguments()[0]) != -1);
            if (implementationConstructor == null)
            {
                List<ConstructorInfo> implementationConstructors =
                    implementationType.GetConstructors().ToList();
                List<ConstructorInfo> availableСonstructors = new List<ConstructorInfo>();
                foreach (ConstructorInfo constructor in implementationConstructors)
                {
                    bool isAvailable = true;
                    foreach (ParameterInfo parameter in constructor.GetParameters())
                    {
                        isAvailable = isAvailable && _dependenciesConfiguration.Dependencies.Keys.ToList()
                            .Contains(parameter.ParameterType);
                    }

                    if (isAvailable)
                        availableСonstructors.Add(constructor);
                }

                if (availableСonstructors.Count > 0)
                {
                    implementationConstructor = availableСonstructors[0];
                }
            }

            List<ParameterInfo> parameters = implementationConstructor.GetParameters().ToList();
            List<object> newParameters = new List<object>();
            foreach (ParameterInfo parameter in parameters)
            {
                object[] newParameter = Resolve(parameter.ParameterType, ServiceImplementation.First);
                if (newParameter.Length == 0)
                    return null;
                newParameters.Add(newParameter[0]);
            }

            //object newObject = Activator.CreateInstance(dependencyType, newParameters.ToArray());
            object newObject = implementationConstructor.Invoke(newParameters.ToArray());
            return newObject;
        }
        
        private object GenerateNonGeneric(Implementation implementation)
        {
            List<ConstructorInfo> implementationConstructors =
                implementation.ImplementationType.GetConstructors().ToList();
            List<ConstructorInfo> availableСonstructors = new List<ConstructorInfo>();
            foreach (ConstructorInfo implementationConstructor in implementationConstructors)
            {
                bool isAvailable = true;
                foreach (ParameterInfo parameter in implementationConstructor.GetParameters())
                {
                    isAvailable = isAvailable && _dependenciesConfiguration.Dependencies.Keys.ToList()
                        .Contains(parameter.ParameterType);
                }

                if (isAvailable)
                    availableСonstructors.Add(implementationConstructor);
            }
            if (availableСonstructors.Count > 0)
            {
                List<ParameterInfo> parameters = availableСonstructors[0].GetParameters().ToList();
                List<object> newParameters = new List<object>();
                foreach (ParameterInfo parameter in parameters)
                {
                    if (_dependenciesConfiguration.Dependencies.Keys.ToList()
                        .Exists(x => x == parameter.ParameterType))
                    {
                        object[] newParameter = Resolve(parameter.ParameterType, ServiceImplementation.First);
                        if (newParameter.Length == 0)
                            return null;
                        newParameters.Add(newParameter[0]);
                    }
                }
                object newObject = availableСonstructors[0].Invoke(newParameters.ToArray());
                return newObject;
            }

            return null;
        }

    }
}