using System.Threading;
using Microsoft.Maui.Dispatching;

namespace Vault.App.Services;

public sealed class InactivityMonitor : IAsyncDisposable
{
    private readonly IDispatcher _dispatcher;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    private DateTimeOffset _lastActivityUtc = DateTimeOffset.UtcNow;
    private TimeSpan _timeout = TimeSpan.FromMinutes(10);
    private Func<Task>? _onTimeoutAsync;

    public InactivityMonitor(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void Configure(TimeSpan timeout, Func<Task> onTimeoutAsync)
    {
        _timeout = timeout;
        _onTimeoutAsync = onTimeoutAsync;
    }

    public void NotifyActivity()
    {
        _lastActivityUtc = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        if (_timer is not null) return;
        if (_onTimeoutAsync is null) throw new InvalidOperationException("InactivityMonitor is not configured.");

        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        _ = LoopAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _timer?.Dispose();
        _timer = null;
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        try
        {
            while (_timer != null && await _timer.WaitForNextTickAsync(ct))
            {
                var idle = DateTimeOffset.UtcNow - _lastActivityUtc;
                if (idle >= _timeout)
                {
                    // Avoid re-entrance: reset activity to prevent trigger loop
                    _lastActivityUtc = DateTimeOffset.UtcNow;

                    await DispatchAsync(_onTimeoutAsync!);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private Task DispatchAsync(Func<Task> action)
    {
        var tcs = new TaskCompletionSource();

        _dispatcher.Dispatch(async () =>
        {
            try { await action(); tcs.SetResult(); }
            catch (Exception ex) { tcs.SetException(ex); }
        });

        return tcs.Task;
    }

    public ValueTask DisposeAsync()
    {
        Stop();
        return ValueTask.CompletedTask;
    }
}
