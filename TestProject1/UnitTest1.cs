using DependencyInjectionContainer.Configuration;
using DependencyInjectionContainer.DependencyProvider;
using NUnit.Framework;

namespace TestProject1
{
    public class Tests
    {
        public Validator Validator { get; private set; }
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
            public RepositoryImpl(){}
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
            Validator = new Validator();
        }

        [Test]
        public void Valid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            Validator.DependenciesConfiguration = dependencies;
            dependencies.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            Assert.AreEqual(false, Validator.Validate());
        }
        
        [Test]
        public void NotValid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            Validator.DependenciesConfiguration = dependencies;
            //dependency.Register<IRepository, RepositoryImpl>(LifeTime.Singleton);
            dependencies.Register<IService, ServiceImpl>(LifeTime.Singleton);
            Assert.AreEqual(false, Validator.Validate());
        }
        
        [Test]
        public void ValidGeneric()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            Validator.DependenciesConfiguration = dependencies;
            dependencies.Register(typeof(IServiceEnum<>), typeof(ServiceEnumImpl<>), LifeTime.Singleton);
            dependencies.Register<IRepository, MySqlRepositoryImpl>(LifeTime.Singleton);
            //dependencies.Register<IService<IRepository>, ServiceImpl<IRepository>>(LifeTime.Singleton);
            Assert.AreEqual(false, Validator.Validate());
        }


        [Test]

        public void AsSelfValid()
        {
            DependenciesConfiguration dependencies = new DependenciesConfiguration();
            Validator.DependenciesConfiguration = dependencies;
            dependencies.Register<RepositoryImpl, RepositoryImpl>(LifeTime.Singleton);
            Assert.AreEqual(true, Validator.Validate());
        }

    }
}