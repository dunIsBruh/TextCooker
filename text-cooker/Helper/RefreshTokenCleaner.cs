using text_cooker.Core.Interfaces;

namespace text_cooker.Helper;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RefreshTokenCleaner : IDisposable
{
    private readonly IRefreshTokenRepository _repo;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _cts = new();

    public RefreshTokenCleaner(IRefreshTokenRepository repo, TimeSpan? interval = null)
    {
        _repo = repo;
        _interval = interval ?? TimeSpan.FromHours(12);

        Task.Run(RunLoopAsync); // Запускаем фоновую задачу
    }

    private async Task RunLoopAsync()
    {
        using var timer = new PeriodicTimer(_interval);
        Console.WriteLine("🧹 Очистка токенов запущена...");

        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            try
            {
                await _repo.DeleteExpiredAsync();
                Console.WriteLine($"[{DateTime.UtcNow}] ✅ Очистка завершена.");
                // TODO: перевести вывод в консоль на logging
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.UtcNow}] ❌ Ошибка очистки токенов: {e.Message}");
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
