using System.Text.RegularExpressions;
using text_cooker.Entities;

namespace text_cooker.Core;

public class RouteRegistry
{
    private readonly List<Route> _routes = new();

    public RouteRegistry Map(string method, string template, Delegate handler, Role accessLevel)
    {
        _routes.Add(new Route(method.ToUpperInvariant(), Normalize(template), handler, accessLevel));
        return this;
    }
    
    public RouteRegistry Map(string method, string template, Delegate handler)
        => Map(method, template, handler, Role.Anonymous);

    public bool TryMatch(
        string method,
        string path,
        out RouteMatch match,
        out Route route)
    {
        method = method.ToUpperInvariant();
        path = Normalize(path);

        foreach (var route1 in _routes)
        {
            if (route1.Method != method) continue;

            var regexMatch = route1.Regex.Match(path);
            if (!regexMatch.Success) continue;

            var parameters = route1.ParameterNames
                .ToDictionary(p => p, p => regexMatch.Groups[p].Value);

            match = new RouteMatch(route1.Handler, parameters);
            route = route1;
            return true;
        }

        match = default!;
        route = default!;
        return false;
    }

    private static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "/";
        path = path.Trim();
        if (!path.StartsWith("/")) path = "/" + path;
        return path.TrimEnd('/');
    }
}

public record Route
{
    private static readonly Regex ParamRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);
    
    public string Method { get; }
    public Delegate Handler { get; }
    public string[] ParameterNames { get; }
    public Regex Regex { get; }
    public Role AccessLevel { get; }
    
    public Route(string method, string template, Delegate handler, Role accessLevel)
    {
        Method = method;
        Handler = handler;
        AccessLevel = accessLevel;
        
        ParameterNames = ParamRegex.Matches(template)
            .Select(m => m.Groups[1].Value)
            .ToArray();
        
        Regex = new(
            "^" + ParamRegex.Replace(template, @"(?<$1>[^/]+)") + "$", RegexOptions.Compiled
            );
    }
}

public record RouteMatch(Delegate Handler, Dictionary<string, string> Parameters);