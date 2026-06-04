using System.Net;
using System.Text.Json;
using text_cooker.Helper;
namespace text_cooker.Core;

public static class JsonResponseSender
{
    // ReSharper disable once MemberCanBePrivate.Global
    public static async Task Send(this ContextEx context, int statusCode, object content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions.Default);

        var buffer = System.Text.Encoding.UTF8.GetBytes(json);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.ContentLength64 = buffer.Length;

        await context.Response.OutputStream.WriteAsync(buffer);
    }

    public static async Task Ok(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.OK, content);
    }
    
    public static async Task Created(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.Created, content);
    }
    
    public static async Task InternalServerError(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.InternalServerError, content);
    }
    
    public static void NoContent(this ContextEx context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        context.Response.ContentLength64 = 0;
    }
    
    public static async Task BadRequest(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.BadRequest, content);
    }
    
    public static async Task NotFound(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.NotFound, content);
    }
    
    public static async Task Forbidden(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.Forbidden, content);
    }

    public static async Task Unauthorized(this ContextEx context, object content)
    {
        await Send(context, (int)HttpStatusCode.Unauthorized, content);
    }
}