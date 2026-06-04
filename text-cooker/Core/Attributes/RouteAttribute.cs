namespace text_cooker.Core.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class RouteAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}