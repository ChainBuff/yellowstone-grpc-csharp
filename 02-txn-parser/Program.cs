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

request.Transactions.Add("txn", new SubscribeRequestFilterTransactions
{
    Vote = false,
    Failed = false,
    AccountInclude = { accountToTrack },
    AccountExclude = { },
    AccountRequired = { }
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
        var data = stream.ResponseStream.Current;
        if (data.Transaction != null)
        {
            // 解析交易
            var txnSignature = Base58.Encode(data.Transaction.Transaction.Transaction.Signatures[0].ToByteArray());
            Console.WriteLine("交易签名：{0}", txnSignature);

            // 交易涉及的账户
            var accountKeys = data.Transaction.Transaction.Transaction.Message.AccountKeys.Select(k => Base58.Encode(k.ToByteArray()));
            Console.WriteLine("交易涉及的账户：{0}", accountKeys.ToArray());

            // 交易指令
            var instructions = data.Transaction.Transaction.Transaction.Message.Instructions;
            Console.WriteLine("交易指令：{0}", instructions);

            // 日志
            Console.WriteLine("日志：{0}", data.Transaction.Transaction.Meta.LogMessages);
        }
    }
});

Timer timer = new(async (state) =>
{
    await stream.RequestStream.WriteAsync(pingRequest);
}, null, pingIntervalMs, pingIntervalMs);

await Task.WhenAny(responseTask, Task.Run(() => Console.ReadKey()));