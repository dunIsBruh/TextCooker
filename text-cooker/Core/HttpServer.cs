using System.Net;

namespace text_cooker.Core;

public class HttpServer(string url, MiddlewarePipeline pipeline, IServiceProvider serviceProvider)
{
    private readonly HttpListener _listener = new();
    
    public async Task StartAsync(CancellationTokenSource tokenSource)
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            tokenSource.Cancel();
        };
        
        try
        {
            _listener.Prefixes.Add(url);
            _listener.Start();

            var token = tokenSource.Token;
            // Обработка запросов
            while (_listener.IsListening)
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(async () =>
                {
                    var contextEx = new ContextEx(context, token);
                    try
                    {
                        await pipeline.ExecuteAsync(contextEx, serviceProvider);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"work finished with error {ex.Message}");
        }
        finally
        {
            Console.WriteLine("listener stopped");
            _listener.Stop();
            _listener.Close();
        }
    }

}