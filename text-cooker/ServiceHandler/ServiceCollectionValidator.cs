using System.Reflection;

namespace text_cooker.ServiceHandler;

public static class ServiceCollectionValidator
{
    public static void ValidateDependencies(this ServiceCollection services)
    {
        var dependencyGraph = new Dictionary<Type, List<Type>>();

        foreach (var descriptor in services)
        {
            // ключ — интерфейс или абстракция
            dependencyGraph[descriptor.ServiceType] =
                GetConstructorDependencies(descriptor.ImplementationType);
            
            if (descriptor.Instance != null)
            {
                dependencyGraph[descriptor.ServiceType] = new List<Type>();
            }
        }

        // Проверяем циклические зависимости
        var visited = new HashSet<Type>();
        var stack = new HashSet<Type>();

        foreach (var node in dependencyGraph.Keys)
        {
            if (HasCycle(node, dependencyGraph, visited, stack, out var cycle))
            {
                var cycleStr = string.Join(" -> ", cycle.Select(t => t.Name));
                throw new InvalidOperationException($"Cyclic dependency detected: {cycleStr}");
            }
        }
        
        
        // Проверяем, что все зависимости зарегистрированы
        var unresolved = dependencyGraph
            .SelectMany(kv => kv.Value)
            .Where(dep => !dependencyGraph.ContainsKey(dep) && !IsFrameworkType(dep))
            .Distinct()
            .ToList();

        if (unresolved.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Unresolved dependencies detected (not registered in container):");
            foreach (var dep in unresolved)
                Console.WriteLine($"   - {dep.FullName}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Dependency validation passed. No unresolved or cyclic dependencies.");
            Console.ResetColor();
        }
    }

    private static List<Type> GetConstructorDependencies(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (constructors.Length == 0) return [];

        // Берем конструктор с наибольшим количеством параметров
        var ctor = constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        return ctor.GetParameters().Select(p => p.ParameterType).ToList();
    }

    private static bool HasCycle(
        Type node,
        Dictionary<Type, List<Type>> graph,
        HashSet<Type> visited,
        HashSet<Type> stack,
        out List<Type> cycle)
    {
        cycle = [];
        if (!visited.Add(node))
            return false;

        stack.Add(node);

        if (graph.TryGetValue(node, out var deps))
        {
            foreach (var dep in deps)
            {
                if (IsFrameworkType(dep))
                    continue;
                if (!graph.ContainsKey(dep))
                    continue;

                if (!visited.Contains(dep) && HasCycle(dep, graph, visited, stack, out cycle))
                {
                    cycle.Insert(0, node);
                    return true;
                }
                else if (stack.Contains(dep))
                {
                    cycle = [.. stack.SkipWhile(t => t != dep), dep];
                    return true;
                }
            }
        }

        stack.Remove(node);
        return false;
    }
    
    private static bool IsFrameworkType(Type type) =>
        type.Namespace != null &&
        (type.Namespace.StartsWith("System") ||
         type.Namespace.StartsWith("Npgsql") ||
         type.Namespace.StartsWith("Microsoft") ||
         type.Namespace.StartsWith("FluentValidation") ||
         type.Namespace.StartsWith("Serilog"));
}
