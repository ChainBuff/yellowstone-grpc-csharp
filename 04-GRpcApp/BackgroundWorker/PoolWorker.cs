namespace _04_GRpcApp.BackgroundWorker;

public class PoolWorker : BackgroundWorkerBase
{
    public ILogger<PoolWorker> Logger { get; set; }
    private Geyser.GeyserClient client;
    private AsyncDuplexStreamingCall<SubscribeRequest, SubscribeUpdate> stream;

    public PoolWorker(Geyser.GeyserClient client)
    {
        this.client = client;
        Logger = NullLogger<PoolWorker>.Instance;
    }

    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task OnSubscribe(CancellationToken cancellationToken)
    {
        while (await stream.ResponseStream.MoveNext(cancellationToken))
        {
            var data = stream.ResponseStream.Current;
            if (data.Account != null)
            {
                //解析消息
                var accountData = data.Account.Account;
                var pubkey = Base58.Encode(accountData.Pubkey.ToByteArray());
                // 读取 BigInt（64位无符号整数）
                ulong baseDecimal = BitConverter.ToUInt64(accountData.Data.Span.Slice(0, 8).ToArray(), 0);
                ulong quoteDecimal = BitConverter.ToUInt64(accountData.Data.Span.Slice(8, 8).ToArray(), 0);
                // Base58 编码其他字段
                string baseVault = Base58.Encode(accountData.Data.Span.Slice(16, 32).ToArray());
                string quoteVault = Base58.Encode(accountData.Data.Span.Slice(48, 32).ToArray());
                string baseMint = Base58.Encode(accountData.Data.Span.Slice(80, 32).ToArray());
                string quoteMint = Base58.Encode(accountData.Data.Span.Slice(112, 32).ToArray());
                Logger.LogDebug($"{pubkey} {baseVault} {quoteVault} {baseMint} {quoteMint}");
                //后续逻辑 可以 使用消息总线 发送到其他服务处理 或者存储
            }
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        stream = client.Subscribe();
        Task.Run(() => OnSubscribe(cancellationToken), cancellationToken);
        var request = new SubscribeRequest
        {
            Commitment = CommitmentLevel.Confirmed
        };
        request.Accounts.Add("amm", new SubscribeRequestFilterAccounts()
        {
            Owner = { "675kPX9MHTjS2zt1qfr1NYHuzeLXfQM9H24wFSUt1Mp8" },
            Filters =
            {
                new SubscribeRequestFilterAccountsFilter()
                {
                    Datasize = 752
                }
            },
            NonemptyTxnSignature = true
        });
        //过滤数据 此处为 精度 baseDecimal quoteDecimal
        request.AccountsDataSlice.Add(new SubscribeRequestAccountsDataSlice()
        {
            Offset = 32, Length = 16
        });
        //过滤数据 此处为 baseVault quoteVault baseMint quoteMint
        request.AccountsDataSlice.Add(new SubscribeRequestAccountsDataSlice()
        {
            Offset = 336, Length = 128
        });
        await stream.RequestStream.WriteAsync(request, cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        stream.Dispose();
        await base.StopAsync(cancellationToken);
    }
}