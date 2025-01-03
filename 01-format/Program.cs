using Grpc.Net.Client;
using GrpcGeyser;
using Microsoft.Extensions.Configuration;

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var endpoint = configuration["GrpcService:Endpoint"];
var commitment = configuration["GrpcService:Commitment"];
var pingIntervalMs = int.Parse(configuration["Subscription:PingIntervalMs"] ?? "5000");
var pingId = int.Parse(configuration["Subscription:PingId"] ?? "1");
var accountToTrack = configuration["Subscription:AccountToTrack"];

using var channel = GrpcChannel.ForAddress(endpoint!);
var client = new Geyser.GeyserClient(channel);
var request = new SubscribeRequest
{
    Commitment = Enum.Parse<CommitmentLevel>(commitment!)
};
request.Blocks.Add("block", new SubscribeRequestFilterBlocks
{
    AccountInclude = { accountToTrack },
    IncludeTransactions = true,
    IncludeAccounts = false,
    IncludeEntries = false
});

var pingRequest = new SubscribeRequest
{
    Ping = new SubscribeRequestPing
    {
        Id = pingId
    }
};

using var stream = client.Subscribe();

var cancellationToken = new CancellationToken();
await stream.RequestStream.WriteAsync(request);
Task responseTask = Task.Run(async () =>
{
    while (await stream.ResponseStream.MoveNext(cancellationToken))
    {
        var message = stream.ResponseStream.Current;
        Console.WriteLine(message.ToString());
    }
});

Timer timer = new(async (state) =>
{
    await stream.RequestStream.WriteAsync(pingRequest);
}, null, pingIntervalMs, pingIntervalMs);

await Task.WhenAny(responseTask, Task.Run(() => Console.ReadKey()));