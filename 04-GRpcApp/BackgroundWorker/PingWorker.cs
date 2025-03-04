namespace _04_GRpcApp.BackgroundWorker;

public class PingWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ILogger<PingWorker> Logger { get; set; }
    private Geyser.GeyserClient client;
    private AsyncDuplexStreamingCall<SubscribeRequest, SubscribeUpdate> stream;
    private DateTime startTime;
    public PingWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory, Geyser.GeyserClient client) : base(
        timer, serviceScopeFactory)
    {
        this.client = client;
        Logger = NullLogger<PingWorker>.Instance;
        Timer.Period = 5000; //5s 执行一下次
    }

    private async Task OnSubscribe(CancellationToken cancellationToken)
    {
        while (await stream.ResponseStream.MoveNext(cancellationToken))
        {
            var data = stream.ResponseStream.Current;
            if (data.Pong != null)
            {
                var endTime = DateTime.Now;
                var timeSpan = endTime - startTime;
                Logger.LogDebug($"GRpc延时 => {timeSpan.TotalMilliseconds} ms");
            }
        }
    }
    public override async Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        stream = client.Subscribe();
        Task.Run(() => OnSubscribe(cancellationToken), cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var pingRequest = new SubscribeRequest
        {
            Ping = new SubscribeRequestPing
            {
                Id = 1
            }
        };
        startTime=DateTime.Now;
        await stream.RequestStream.WriteAsync(pingRequest);
    }

    public override async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        stream.Dispose();
        await base.StopAsync(cancellationToken);
    }
}