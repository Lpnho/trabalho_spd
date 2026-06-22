namespace Freeway.Core;

public abstract class Worker
{
    private Thread _thread;
    private CancellationTokenSource? _publicTokenSource;
    private CancellationTokenSource? _linkedTokenSource;
    private CancellationToken? _externalTokenSource;
    public bool IsRunning { get; private set; }
    public Worker(string? threadName = null)
    {
        _thread = new Thread(RunThread);
        if (threadName != null)
        {
            _thread.Name = threadName;
        }
    }
    private void RunThread()
    {
        try
        {
            _publicTokenSource = new CancellationTokenSource();
            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_publicTokenSource.Token,
                _externalTokenSource!.Value);
            Run(_linkedTokenSource!.Token);
        }
        finally
        {
            IsRunning = false;
        }
    }
    public void Start(CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            IsRunning = true;
            _externalTokenSource = cancellationToken;
            _thread.Start();
        }
    }
    public void Stop()
    {
        if (IsRunning)
        {
            _publicTokenSource?.Cancel();
            _linkedTokenSource?.Dispose();
            _thread.Join();
        }
    }

    protected abstract void Run(CancellationToken cancellationToken);

}
