# 创建订阅

本部分将介绍创建grpc订阅的通用流程。若要更改订阅的数据，只需要修改订阅请求即可。

VS Code 环境及插件部分请参考： https://learn.microsoft.com/zh-cn/aspnet/core/grpc/basics?view=aspnetcore-9.0

## 创建订阅客户端和数据流

首先，需要先指定grpc的endpoint，来创建订阅的客户端。

```c#
using var channel = GrpcChannel.ForAddress("https://solana-yellowstone-grpc.publicnode.com:443");
```

## 创建订阅请求

以下是一个订阅slot更新的例子，获取数据的级别为`processed`。

```c#
// 创建订阅请求
var request = new SubscribeRequest
{
    Commitment = CommitmentLevel.Processed // 指定级别为processed
};

request.Slots.Add("slot", new SubscribeRequestFilterSlots { FilterByCommitment = true }); // 指定只获取processed的slot
```

## 发送订阅请求并获取数据流

之后，便可将订阅请求发送给服务端，并获取数据流。

```ts
// 发送订阅请求
await stream.RequestStream.WriteAsync(request);


// 获取订阅数据
var cancellationToken = new CancellationToken();

while (await stream.ResponseStream.MoveNext(cancellationToken))
{
    var message = stream.ResponseStream.Current;
    Console.WriteLine(message.ToString());
}
```

输出应如下：

```bash
{ "filters": [ "slot" ], "slot": { "slot": "311149563", "parent": "311149562" } }
```


# 总结

以上就是创建grpc订阅的基本流程。之后，我们需要了解订阅的基本格式以订阅各种不同的数据。