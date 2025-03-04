using _04_GRpcApp.BackgroundWorker;
using _04_GRpcApp.Options;
using Grpc.Net.Client;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace _04_GRpcApp;

[DependsOn(typeof(AbpAutofacModule), typeof(AbpBackgroundWorkersModule))]
public class AppGRpcModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        var configuration = context.Services.GetConfiguration();
        //solana 配置
        var solanaNet = configuration.GetSection("Solana").Get<SolanaOptions>();
        Configure<SolanaOptions>(options =>
        {
            options.MainUri = solanaNet.MainUri;
            options.MainWsrRpc = solanaNet.MainWsrRpc;
            options.MainWsUri = solanaNet.MainWsUri;
        });
        //grpc 配置
        var grpcConf = configuration.GetSection("Grpc").Get<GrpcOptions>();
        Configure<GrpcOptions>(options =>
        {
            options.Commitment = grpcConf.Commitment;
            options.Endpoint = grpcConf.Endpoint;
        });
        var channelOptions = new GrpcChannelOptions
        {
            MaxReceiveMessageSize = 128 * 1024 * 1024, // 64MB，匹配 Yellowstone 的需求
        };
        GrpcChannel channel = GrpcChannel.ForAddress(grpcConf.Endpoint,channelOptions);
        Geyser.GeyserClient client = new Geyser.GeyserClient(channel);
        context.Services.AddSingleton<Geyser.GeyserClient>(client);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        base.OnApplicationInitialization(context);

        var _logger = context.ServiceProvider.GetRequiredService<ILogger<AppGRpcModule>>();
        var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
        _logger.LogDebug($"Module 加载成功=>EnvironmentName => {hostEnvironment.EnvironmentName}");

        //以下加载各种服务
        //context.AddBackgroundWorkerAsync<MyWorker>();//定是任务
        context.AddBackgroundWorkerAsync<PingWorker>();//定时ping
        context.AddBackgroundWorkerAsync<PoolWorker>(); //订阅池子服务
        context.AddBackgroundWorkerAsync<PriceWorker>(); //订阅 内盘 外盘 价格服务 
    }
}