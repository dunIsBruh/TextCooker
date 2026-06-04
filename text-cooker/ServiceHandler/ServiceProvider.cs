namespace text_cooker.ServiceHandler;

public class ServiceProvider : IServiceProvider, IDisposable
{
    private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptors;
    private readonly List<IDisposable> _disposables = [];
    private bool _disposed;

    public ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        _serviceDescriptors = serviceDescriptors.ToDictionary(sd => sd.ServiceType, sd => sd);
        
        _serviceDescriptors.Add(typeof(IServiceProvider), new ServiceDescriptor
            (typeof(IServiceProvider), this.GetType(), ServiceLifetime.Singleton, this));
    }

    public object? GetService(Type serviceType)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceProvider));
        }

        if (!_serviceDescriptors.TryGetValue(serviceType, out var descriptor))
        {
            return null;
        }

        if (descriptor is { Lifetime: ServiceLifetime.Singleton, Instance: not null })
        {
            return descriptor.Instance;
        }

        var implementation = CreateInstance(descriptor.ImplementationType);

        if (descriptor.Lifetime == ServiceLifetime.Singleton)
        {
            descriptor.Instance = implementation;
        }

        if (implementation is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return implementation;
    }

    private object CreateInstance(Type implementationType)
    {
        var ctor = implementationType.GetConstructors().First();
        var parameters = ctor.GetParameters()
            .Select(p => GetService(p.ParameterType))
            .ToArray();

        return Activator.CreateInstance(implementationType, parameters)!;
    }

    public object GetRequiredService(Type serviceType)
    {
        return GetService(serviceType)
            ?? throw new InvalidCastException($"Сервис {serviceType.Name} не зарегистрирован");
    }
    
    public TService GetRequiredService<TService>() 
        => (TService)GetRequiredService(typeof(TService)); 
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();
    }
}