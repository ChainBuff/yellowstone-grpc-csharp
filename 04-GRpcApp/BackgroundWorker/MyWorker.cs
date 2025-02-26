namespace _04_GRpcApp.BackgroundWorker;

public class MyWorker:AsyncPeriodicBackgroundWorkerBase
{
    public ILogger<MyWorker> Logger { get; set; }

    public MyWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory):base(timer, serviceScopeFactory)
    {
        Timer.Period = 1000;//1s 执行一下次
        Logger=NullLogger<MyWorker>.Instance;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogDebug($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff}");
    }
}