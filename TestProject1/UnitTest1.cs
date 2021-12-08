using System;
using System.Collections.Generic;
using System.Linq;
using DependencyInjectionContainer.Configuration;
using DependencyInjectionContainer.DependencyProvider;
using NUnit.Framework;

namespace TestProject1
{
    public class Tests
    {
        private Validator _validator;

        private DependencyProvider _dependencyProvider;

        interface IService
        {
            IRepository Repository { get; set; }
        }
        class ServiceImpl : IService
        {
            public IRepository Repository { get; set; }

            public ServiceImpl(IRepository repository)
            {
                Repository = repository;
            }
        }
        
        class ServiceImpl2 : IService
        {
            public IRepository Repository { get; set; }
            public ServiceImpl2(IService service)
            {
                
            }
        }
        interface IRepository{}
        class RepositoryImpl : IRepository
        {
            
        }
        
        class AnotherRepositoryImpl : IRepository
        {
            
        }
        
        interface IServiceEnum<TMySqlRepository> where TMySqlRepository : IRepository
        {
        }
        class ServiceEnumImpl<TRepository> : IServiceEnum<TRepository> where TRepository : IRepository
        {
            public ServiceEnumImpl(TRepository repository, IService service)
            {
            }
        }
        
        class ServiceEnumImpl2<TRepository> : IServiceEnum<TRepository> where TRepository : IRepository
        {
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
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
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

        public void ThrowWithNotValid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            //dependency.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            var ex = Assert.Throws<ArgumentException>(() => _dependencyProvider = new DependencyProvider(dependencies));
            Assert.AreEqual("Wrong configuration", ex.Message);
        }

        [Test]
        public void Resolve()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(RepositoryImpl));
        }

        
        [Test]
        public void ValidateWithCycle()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IService, ServiceImpl2>(LifeTime.Singleton);
            var ex = Assert.Throws<ArgumentException>(() => _dependencyProvider = new DependencyProvider(dependencies));
            Assert.AreEqual("Wrong configuration", ex.Message);
        }
        
        
        [Test]
        public void ResolveNested()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IService> newEnumerator = _dependencyProvider.Resolve<IService>().GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(ServiceImpl));
            Assert.AreEqual(newEnumerator.Current.Repository.GetType(), typeof(RepositoryImpl));
        }
        
        [Test]
        public void ResolveSurface()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IRepository, AnotherRepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(RepositoryImpl));
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(AnotherRepositoryImpl));
        }
        
        [Test]
        public void ResolveGeneric()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            dependencies.Register<IServiceEnum<IRepository>, ServiceEnumImpl<IRepository>>(LifeTime.Singleton);
            //dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl<>), LifeTime.Singleton);
            dependencies.Register<IRepository, MySqlRepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            _dependencyProvider.Resolve<IServiceEnum<IRepository>>();
            IEnumerator<IServiceEnum<IRepository>> newEnumerator = _dependencyProvider.Resolve<IServiceEnum<IRepository>>().GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(ServiceEnumImpl<IRepository>));
        }
        
        
        [Test]
        public void ResolveOpenGeneric()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            //dependencies.Register<IServiceEnum<IRepository>, ServiceEnumImpl<IRepository>>(LifeTime.Singleton);
            dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl<>), LifeTime.Singleton);
            dependencies.Register<IRepository, MySqlRepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IServiceEnum<IRepository>> newEnumerator = _dependencyProvider.Resolve<IServiceEnum<IRepository>>().GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(ServiceEnumImpl<IRepository>));
        }
        
        [Test]
        public void ResolveOpenGenericWithoutConstructor()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            //dependencies.Register<IServiceEnum<IRepository>, ServiceEnumImpl<IRepository>>(LifeTime.Singleton);
            dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl2<>), LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IServiceEnum<IRepository>> newEnumerator = _dependencyProvider.Resolve<IServiceEnum<IRepository>>().GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(ServiceEnumImpl2<IRepository>));
        }

        [Test]
        public void ResolveSingleton()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            newEnumerator.MoveNext();
            IEnumerator<IRepository> newEnumerator2 = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            newEnumerator2.MoveNext();
            Assert.AreEqual(newEnumerator.Current, newEnumerator2.Current);
        }

        [Test]
        public void ResolveInstancePerDependency()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.InstancePerDependency);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            newEnumerator.MoveNext();
            IEnumerator<IRepository> newEnumerator2 = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            newEnumerator2.MoveNext();
            Assert.AreNotEqual(newEnumerator.Current, newEnumerator2.Current);
        }
        
        [Test]
        public void ResolveFirstImplementation()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IRepository, AnotherRepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>(ServiceImplementation.First).GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(RepositoryImpl));
        }

        
        [Test]
        public void ResolveSecondImplementation()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IRepository, AnotherRepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>(ServiceImplementation.Second).GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(AnotherRepositoryImpl));
        }
        
        [Test]

        public void AsSelfResolve()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<RepositoryImpl, RepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<RepositoryImpl> newEnumerator = _dependencyProvider.Resolve<RepositoryImpl>(ServiceImplementation.First).GetEnumerator();
            newEnumerator.MoveNext();
            Assert.AreEqual(newEnumerator.Current.GetType(), typeof(RepositoryImpl));
        }
        
        [Test]
        public void ResolveNonExistent()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<RepositoryImpl, RepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            Assert.AreEqual(_dependencyProvider.Resolve<IRepository>(ServiceImplementation.First).Any(), false);
        }

        private IEnumerator<IRepository> FirstThread()
        {
            IEnumerator<IRepository> newEnumerator = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            return newEnumerator;
        }
        
        private IEnumerator<IRepository> SecondThread()
        {
            IEnumerator<IRepository> newEnumerator2 = _dependencyProvider.Resolve<IRepository>().GetEnumerator();
            return newEnumerator2;
        }
        
        [Test]
        public void ThreadSingleton()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            _validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            _dependencyProvider = new DependencyProvider(dependencies);
            IEnumerator<IRepository> newEnumerator = FirstThread();
            IEnumerator<IRepository> newEnumerator2 = SecondThread();
            newEnumerator.MoveNext();
            newEnumerator2.MoveNext();
            Assert.AreEqual(newEnumerator.Current, newEnumerator2.Current);
        }
    }
}