namespace text_cooker.ServiceHandler;

public class ServiceCollection : List<ServiceDescriptor>
{
    public void AddTransient<TService, TImplementation>() where TImplementation : class, TService
        => Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));
    
    public void AddTransient<TImplementation>() where TImplementation : class, new() 
        => Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), ServiceLifetime.Transient));
    
    public void AddSingleton<TService, TImplementation>() where TImplementation : class, TService
    => Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));

    public void AddSingleton<TService>(TService instance)
    => Add(new ServiceDescriptor
            (typeof(TService), instance?.GetType() ?? throw new NullReferenceException(), ServiceLifetime.Singleton)
            {
                Instance = instance
            });
}