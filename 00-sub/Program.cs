using Grpc.Net.Client;
using GrpcGeyser;

using var channel = GrpcChannel.ForAddress("https://solana-yellowstone-grpc.publicnode.com:443");
var client = new Geyser.GeyserClient(channel);
var request = new SubscribeRequest
{
    Commitment = CommitmentLevel.Processed
};

request.Slots.Add("slot", new SubscribeRequestFilterSlots { FilterByCommitment = true });

using var stream = client.Subscribe();

await stream.RequestStream.WriteAsync(request);
var cancellationToken = new CancellationToken();


while (await stream.ResponseStream.MoveNext(cancellationToken))
{
    var message = stream.ResponseStream.Current;
    Console.WriteLine(message.ToString());
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();