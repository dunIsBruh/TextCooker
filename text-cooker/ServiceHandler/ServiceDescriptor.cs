namespace text_cooker.ServiceHandler;

public class ServiceDescriptor(
    Type serviceType,
    Type implementationType,
    ServiceLifetime lifetime,
    object? instance = null)
{
    public Type ServiceType { get; set; } = serviceType;
    public Type ImplementationType { get; set; } = implementationType;
    public ServiceLifetime Lifetime { get; set; } = lifetime;
    public object? Instance { get; set; } = instance;
}