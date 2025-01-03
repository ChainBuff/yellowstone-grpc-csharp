
# Solana Slot 数据订阅程序

## 项目概述

这是一个使用 Yellowstone-grpc 服务订阅 Solana 区块链 Slot 数据的 .NET 应用程序。本项目可以实时监控 Solana 区块链的 Slot 更新信息，支持可配置参数。

## 环境要求

- .NET 8.0 SDK 或更高版本
- Visual Studio 2022 或 VS Code
- 基本的 Solana 区块链知识
- Yellowstone-grpc 端点访问权限

## 安装步骤

1. 克隆代码仓库：
```bash
git clone https://github.com/ChainBuff/yellowstone-grpc-csharp.git
cd yellowstone-grpc-csharp/00-sub
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
    "Commitment": "Processed"
  },
  "Subscription": {
    "PingIntervalMs": 5000,
    "PingId": 1
  }
}
```

### 配置参数说明

- `Endpoint`: Yellowstone-grpc 服务端点
- `Commitment`: Solana 确认级别 (Processed/Confirmed/Finalized)
- `PingIntervalMs`: 心跳间隔时间（毫秒）
- `PingId`: ping 消息的唯一标识

## 使用方法

1. 根据需要修改 `appsettings.json` 配置

2. 运行程序：
```bash
dotnet run
```

3. 程序将会：
   - 连接到指定的 gRPC 端点
   - 订阅 Slot 更新信息
   - 实时显示 Slot 数据
   - 通过定期发送 ping 消息保持连接

## 输出示例

程序会输出 Slot 更新信息，例如：
```json
{
  "filters": ["slot"],
  "slot": {
    "slot": "311537640",            // Slot 编号
    "parentSlot": "311537639",      // 父 Slot 编号
    "status": "Processed",          // Slot 状态
    "timestamp": "1735876744"       // 时间戳
  }
}
```

## 常见问题

1. 连接错误：
   - 检查端点 URL 是否正确
   - 检查网络连接
   - 确保端点可以从您的网络访问

2. 未收到数据：
   - 检查确认级别设置是否正确
   - 确保网络连接稳定

3. 程序崩溃：
   - 查看日志中的错误信息
   - 验证配置值是否有效
   - 确保安装了8.0的 .NET SDK

