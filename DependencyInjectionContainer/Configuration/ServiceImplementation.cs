namespace DependencyInjectionContainer.Configuration
{
    public enum ServiceImplementation
    {
        None = 1,
        First = 2,
        Second = 4,
        Any = None | First | Second
    }
}