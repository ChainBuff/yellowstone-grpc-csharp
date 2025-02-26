namespace _04_GRpcApp.BackgroundWorker;

public class PriceWorker:BackgroundWorkerBase
{
    public ILogger<PoolWorker> Logger { get; set; }
    private Geyser.GeyserClient client;
    private AsyncDuplexStreamingCall<SubscribeRequest, SubscribeUpdate> stream;
    public PriceWorker(Geyser.GeyserClient client)
    {
        this.client = client;
        Logger=NullLogger<PoolWorker>.Instance;
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
                var type = data.Filters[0];
                if (type == "pump")
                {
                    var offset = 8;
                    var virtualTokenReserves = BitConverter.ToUInt64(accountData.Data.Span.Slice(offset, 8).ToArray(), 0);
                    offset += 8;
                    var virtualSolReserves = BitConverter.ToUInt64(accountData.Data.Span.Slice(offset, 8).ToArray(), 0);
                    offset += 8;
                    var realTokenReserves = BitConverter.ToUInt64(accountData.Data.Span.Slice(offset, 8).ToArray(), 0);
                    offset += 8;
                    var realSolReserves = BitConverter.ToUInt64(accountData.Data.Span.Slice(offset, 8).ToArray(), 0);
                    offset += 8;
                    var tokenTotalSupply = BitConverter.ToUInt64(accountData.Data.Span.Slice(offset, 8).ToArray(), 0);
                    offset += 8;
                    var complete = accountData.Data.Span[offset]==1;
                    var tokenPrice = (decimal)(virtualSolReserves / 1e9) / (decimal)(virtualTokenReserves / 1e6);
                    Logger.LogDebug($"[{type}]{pubkey} {tokenPrice}");
                }
                else if (type == "ray")
                {
                    var offset = 0;
                    var mint =Base58.Encode(accountData.Data.Span.Slice(offset, 32).ToArray()); //0 32
                    offset += 32;
                    var owner = Base58.Encode(accountData.Data.Span.Slice(offset, 32).ToArray()); //32 32
                    offset += 32;
                    ulong amount =BitConverter.ToUInt64( accountData.Data.Span.Slice(offset, 8).ToArray(),0); // 小端序64 8
                    offset += 8;
                    var delegateOption =BitConverter.ToUInt32( accountData.Data.Span.Slice(offset, 4).ToArray(),0); //72 4
                    offset += 4;
                    var delegatePubkey = delegateOption == 1 ?  Base58.Encode(accountData.Data.Span.Slice(offset, 32).ToArray()) : null; //76 32
                    offset += 32;
                    var state =(int)accountData.Data.Span[offset]; //108 1
                    offset += 1;
                    var isNativeOption = BitConverter.ToUInt32( accountData.Data.Span.Slice(offset, 4).ToArray(),0); //109 4
                    offset += 4;
                    var isNative = BitConverter.ToUInt64( accountData.Data.Span.Slice(offset, 8).ToArray(),0); //113 8
                    offset += 8;
                    var delegatedAmount = BitConverter.ToUInt64( accountData.Data.Span.Slice(offset, 8).ToArray(),0); //121 8
                    offset += 8;
                    var closeAuthorityOption =BitConverter.ToUInt32( accountData.Data.Span.Slice(offset, 4).ToArray(),0); //129 4
                    offset += 4;
                    var closeAuthority = closeAuthorityOption == 1 ? Base58.Encode(accountData.Data.Span.Slice(offset, 32).ToArray()) : null;
                    Logger.LogDebug($"[{type}]{pubkey} {mint} {amount}");
                }
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
        //订阅 ray
        request.Accounts.Add("ray", new SubscribeRequestFilterAccounts()
        {
            Owner = { "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA" },
            Filters =
            {
                new SubscribeRequestFilterAccountsFilter()
                {
                    Datasize = 165,
                    Memcmp = new SubscribeRequestFilterAccountsFilterMemcmp()
                    {
                        Offset = 32, Base58 =  "5Q544fKrFoe6tsEbD7S8EmxGTJYAKtTVhAW5Q5pge4j1"
                    }
                }
            },
            NonemptyTxnSignature = true
        });
        //订阅pump
        request.Accounts.Add("pump", new SubscribeRequestFilterAccounts()
        {
            Owner = { "6EF8rrecthR5Dkzon8Nwu78hRvfCKubJ14M5uBEwF6P" },
            Filters =
            {
                new SubscribeRequestFilterAccountsFilter() { Datasize = 49 }
            },
            NonemptyTxnSignature = true
        });
        await stream.RequestStream.WriteAsync(request,cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        stream.Dispose();
        await base.StopAsync(cancellationToken);
    }
}