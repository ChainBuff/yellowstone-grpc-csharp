// See https://aka.ms/new-console-template for more information


// Load configuration

using System.Drawing;
using Colorful;
using Grpc.Net.Client;
using GrpcGeyser;
using Microsoft.Extensions.Configuration;
using Console = Colorful.Console;
Figlet figlet = new Figlet();
Console.WriteLine(figlet.ToAscii("Solana"), ColorTranslator.FromHtml("#FAD6FF"));
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

request.Transactions.Add("pumpfun", new SubscribeRequestFilterTransactions
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
//订阅消息
await stream.RequestStream.WriteAsync(request);
Task responseTask = Task.Run(async () =>
{
    //消息回复
    while (await stream.ResponseStream.MoveNext(cancellationToken))
    {
        var data = stream.ResponseStream.Current;
        if (data.Transaction != null)
        {
            var transaction = data.Transaction.Transaction;
            var signature = Base58.Encode(transaction.Transaction.Signatures[0].ToByteArray());
            var mint = transaction.Meta.PostTokenBalances[0].Mint;
            if(string.IsNullOrWhiteSpace(mint))return;
            var creator = transaction.Meta.PostTokenBalances[1].Owner ?? "未知";
            // 交易涉及的账户
            var accountKeys = transaction.Transaction.Message.AccountKeys.Select(k => Base58.Encode(k.ToByteArray())).ToList();
            var bondingCurveAddress = accountKeys.Count >= 3 ? accountKeys[2] : "未知";
            var associatedBondingCurveAddress =accountKeys.Count>=4?accountKeys[3]:"未知";
            Console.WriteLine($"===== 新代币创建 =================================================");
            Console.WriteLine($"代币地址:\x1b[92m{mint}\x1b[0m");
            Console.WriteLine($"创建者:\x1b[93m{creator}\x1b[0m");
            Console.WriteLine($"Bonding Curve:\x1b[95m{bondingCurveAddress}\x1b[0m");
            Console.WriteLine($"Associated Bonding Curve:\x1b[96m{associatedBondingCurveAddress}\x1b[0m");
            Console.WriteLine($"链接:https://solscan.io/tx/{signature}");
            Console.WriteLine($"==== {DateTime.Now:HH:mm:ss,fff} =================================================");
        }
    }
});

Timer timer = new(async (state) =>
{
    //定时ping
    await stream.RequestStream.WriteAsync(pingRequest);
}, null, pingIntervalMs, pingIntervalMs);

await Task.WhenAny(responseTask, Task.Run(() => Console.ReadKey()));