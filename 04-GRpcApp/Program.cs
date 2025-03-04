// See https://aka.ms/new-console-template for more information

using _04_GRpcApp;

var template = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.File($"{Environment.CurrentDirectory}/logs/.txt", rollingInterval: RollingInterval.Day, outputTemplate: template))
    .WriteTo.Async(c => c.Console())
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddAppSettingsSecretsJson();
builder.Logging.ClearProviders().AddSerilog();
builder.ConfigureContainer(builder.Services.AddAutofacServiceProviderFactory());
await builder.Services.AddApplicationAsync<AppGRpcModule>();

var host = builder.Build();

await host.InitializeAsync();

await host.RunAsync();