using System;
using System.Collections.Generic;
using DependencyInjectionContainer.Configuration;
using DependencyInjectionContainer.DependencyProvider;
using NUnit.Framework;

namespace TestProject1
{
    public class Tests
    {
        private Validator _validator;

        private DependencyProvider _dependencyProvider;

            interface IService {}
        class ServiceImpl : IService
        {
            public ServiceImpl(IRepository repository) 
            {
                
            }
        }
        interface IRepository{}
        class RepositoryImpl : IRepository
        {
            
        }
        
        interface IServiceEnum<TMySqlRepository> where TMySqlRepository : IRepository
        {
        }
        class ServiceEnumImpl<TRepository> : IServiceEnum<TRepository> where TRepository : IRepository
        {
            public ServiceEnumImpl(TRepository repository)
            {
            }
        }
        
        class MySqlRepositoryImpl : IRepository
        {
            public MySqlRepositoryImpl(){}
        }

        [SetUp]
        public void Setup()
        {
            _validator = new Validator();
        }

        [Test]
        public void Valid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            Assert.AreEqual(true, _validator.Validate());
        }
        
        [Test]
        public void NotValid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            //dependency.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            Assert.AreEqual(false, _validator.Validate());
        }
        
        [Test]
        public void ValidGeneric()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl<>), LifeTime.Singleton);
            dependencies.Register<IRepository, MySqlRepositoryImpl>(LifeTime.Singleton);
            _validator.DependenciesConfiguration = dependencies;
            //dependencies.Register<IService<IRepository>, ServiceImpl<IRepository>>(LifeTime.Singleton);
            Assert.AreEqual(true, _validator.Validate());
        }


        [Test]

        public void AsSelfValid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<RepositoryImpl, RepositoryImpl>(LifeTime.Singleton);
            Assert.AreEqual(true, _validator.Validate());
        }
        
        [Test]
        public void Resolve()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IService> newEnumerator = _dependencyProvider.Resolve<IService>().GetEnumerator();
            newEnumerator.MoveNext();
            Console.WriteLine(newEnumerator.Current);
        }
        
        [Test]
        public void ResolveGeneric()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            dependencies.Register<IServiceEnum<IRepository>, ServiceEnumImpl<IRepository>>(LifeTime.Singleton);
            //dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl<>), LifeTime.Singleton);
            dependencies.Register<IRepository, MySqlRepositoryImpl>(LifeTime.Singleton);
            
            _dependencyProvider = new DependencyProvider(dependencies);
            _dependencyProvider.Resolve<IServiceEnum<IRepository>>();
        }
        
        
        [Test]
        public void ResolveOpenGeneric()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            //dependencies.Register<IServiceEnum<IRepository>, ServiceEnumImpl<IRepository>>(LifeTime.Singleton);
            dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl<>), LifeTime.Singleton);
            dependencies.Register<IRepository, MySqlRepositoryImpl>(LifeTime.Singleton);
            
            _dependencyProvider = new DependencyProvider(dependencies);
            _dependencyProvider.Resolve<IServiceEnum<IRepository>>();
        }

    }
}