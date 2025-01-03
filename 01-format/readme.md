
# Solana 区块数据订阅程序

## 项目概述

这是一个使用 Yellowstone-grpc 服务订阅 Solana 区块链数据的 .NET 应用程序。本项目可以实时监控特定账户和交易活动，并支持可配置参数。

## 环境要求

- .NET 8.0 SDK 或更高版本
- Visual Studio 2022 或 VS Code
- 基本的 Solana 区块链知识
- Yellowstone-grpc 端点访问权限

## 安装步骤

1. 克隆代码仓库：
```bash
git clone https://github.com/ChainBuff/yellowstone-grpc-csharp.git
cd yellowstone-grpc-csharp/01-format
```

2. 安装依赖：
```bash
dotnet restore
```

## 配置说明

程序使用 `appsettings.json` 进行配置：

```json
{
  "GrpcService": {
    "Endpoint": "https://solana-yellowstone-grpc.publicnode.com:443",
    "Commitment": "Confirmed"
  },
  "Subscription": {
    "PingIntervalMs": 5000,
    "PingId": 1,
    "AccountToTrack": "GuiU6MpLahPHSHYcsfSRjwLUm1AtZ9zP2eiLAkJMBjg"
  }
}
```

### 配置参数说明

- `Endpoint`: Yellowstone-grpc 服务端点
- `Commitment`: Solana 确认级别 (Processed/Confirmed/Finalized)
- `PingIntervalMs`: 心跳间隔时间（毫秒）
- `PingId`: ping 消息的唯一标识
- `AccountToTrack`: 需要监控的 Solana 账户地址

## 使用方法

1. 根据需要修改 `appsettings.json` 配置

2. 运行程序：
```bash
dotnet run
```

3. 程序将会：
   - 连接到指定的 gRPC 端点
   - 订阅区块更新
   - 过滤指定账户的相关交易
   - 实时显示区块数据
   - 通过定期发送 ping 消息保持连接

## 输出示例

程序会输出详细的区块信息：
```json

{
    "filters": ["block"],                // 过滤器类型：区块数据
    "block": {
        "slot": "311537640",            // 区块槽位号
        "blockhash": "AbMe59...",       // 区块哈希值
        "rewards": {                     // 区块奖励信息
            "rewards": [{
                "pubkey": "5Cchr1...",  // 接收奖励的账户公钥
                "lamports": "59282351",  // 奖励金额(lamports)
                "postBalance": "106052738593",  // 奖励后余额
                "rewardType": "Fee"      // 奖励类型：交易费
            }]
        },
        "blockTime": {
            "timestamp": "1735876744"    // 区块时间戳(Unix时间)
        },
        "blockHeight": {
            "blockHeight": "289861157"   // 区块高度
        },
        "parentSlot": "311537639",      // 父区块槽位号
        "parentBlockhash": "7FxxPa...",  // 父区块哈希值
        "executedTransactionCount": "2386",  // 已执行的交易数量
        "updatedAccountCount": "5336",   // 更新的账户数量
        "entriesCount": "300"           // 区块条目数量
    }
}
```

## 常见问题

1. 连接错误：
   - 检查端点 URL 是否正确
   - 检查网络连接
   - 确保端点可以从您的网络访问

2. 未收到数据：
   - 验证账户地址是否正确
   - 检查确认级别设置
   - 确保账户在区块链上有活动

3. 程序崩溃：
   - 查看日志中的错误信息
   - 验证配置值是否有效
   - 确保安装了8.0的 .NET SDK
