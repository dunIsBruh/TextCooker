using System.Net;
using System.Security.Claims;
using System.Text.Json;
using text_cooker.Entities;
using text_cooker.Helper;
using text_cooker.Pipeline;

namespace text_cooker.Core;

public class ContextEx
{
    public HttpListenerContext HttpContext { get; }
    public HttpListenerRequest Request => HttpContext.Request;
    public HttpListenerResponse Response => HttpContext.Response;
    public CancellationToken RequestAborted { get; }
    public User? CurrentUser { get; set; }
    public ClaimsPrincipal? Claims { get; set; }
    
    public Type? BodyInputType { get; set; }
    public object? BodyInput { get; set; }
    
    public Delegate EndpointHandler { get; set; }
    public Dictionary<string, string> RouteParams { get; set; }
    public bool IsStaticFile { get; set; } = false;

    public ContextEx(HttpListenerContext context, CancellationToken token = default)
    {
        HttpContext = context;
        RequestAborted = token;
        BodyInputType = BodyInput?.GetType();
        RouteParams = new Dictionary<string, string>();
    }

    public async Task<object> ReadJsonAsync(Type type)
    {
        using var reader = new StreamReader(HttpContext.Request.InputStream, HttpContext.Request.ContentEncoding);
        var body = await reader.ReadToEndAsync(RequestAborted);

        if (string.IsNullOrWhiteSpace(body))
            throw new BadRequestException("Тело запроса отсутствует или пустое.");

        try
        {
            var model = JsonSerializer.Deserialize(body, type, JsonOptions.Default);
            if (model is null)
                throw new BadRequestException("Не удалось десериализовать тело запроса.");

            return model;
        }
        catch (JsonException ex)
        {
            throw new BadRequestException($"Ошибка десериализации тела запроса: {ex.Message}");
        }
    }
}