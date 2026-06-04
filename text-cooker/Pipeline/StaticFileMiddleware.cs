using System.Text;
using text_cooker.Core;
using text_cooker.Core.Interfaces;

namespace text_cooker.Pipeline;

public class StaticFileMiddleware(string rootPath = "wwwroot") : IMiddleware
{
    private readonly Dictionary<string, string> _mimeTypes = new()
    {
        { ".html", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".svg", "image/svg+xml" },
        { ".ico", "image/x-icon" },
        { ".txt", "text/plain" },
        { ".pdf", "application/pdf" },
        { ".zip", "application/zip" }
    };

    public async Task InvokeAsync(ContextEx contextEx, Func<Task> next)
    {
        var path = contextEx.Request.Url?.AbsolutePath ?? "/";
        
        // Convert URL path to file system path
        var filePath = path == "/" ? 
            Path.Combine(rootPath, "index.html") : 
            Path.Combine(rootPath, path.TrimStart('/'));

        // Check if file exists
        if (!File.Exists(filePath))
        {
            // If file not found, try index.html for SPA routing
            var indexPath = Path.Combine(rootPath, "index.html");
            if (File.Exists(indexPath))
            {
                await ServeFileAsync(contextEx, indexPath);
                return;
            }

            contextEx.Response.StatusCode = 404;
            return;
        }

        await ServeFileAsync(contextEx, filePath);
    }

    private async Task ServeFileAsync(ContextEx contextEx, string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = _mimeTypes.GetValueOrDefault(extension, "application/octet-stream");

        if (contentType.StartsWith("text/"))
            contentType += "; charset=utf-8";

        contextEx.Response.ContentType = contentType;
        contextEx.Response.Headers.Add("Cache-Control", "public, max-age=3600");

        byte[] buffer;
        if (contentType.StartsWith("text/") ||
            contentType == "application/javascript" || 
            contentType == "application/json")
        {
            // For text files, read as UTF-8 text
            var content = await File.ReadAllTextAsync(filePath, contextEx.RequestAborted);
            buffer = Encoding.UTF8.GetBytes(content);
        }
        else
        {
            // For binary files, read as bytes
            buffer = await File.ReadAllBytesAsync(filePath, contextEx.RequestAborted);
        }

        contextEx.Response.ContentLength64 = buffer.Length;
        await contextEx.Response.OutputStream.WriteAsync(buffer, contextEx.RequestAborted);
    }
}