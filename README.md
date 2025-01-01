# Yellowstone gRPC 教程

Yellowstone gRPC 是获取 Solana 链上数据最快的方式。数据以流的方式推送，客户端需要配置订阅来获取和解析数据。

本教程旨在提供一些简单的订阅配置例子，帮助你快速熟悉此工具。

---

在阅读之前，需要添加所需的 NuGet 包。

```bash
Install-Package Grpc.Net.Client

Install-Package Google.Protobuf

Install-Package Grpc.Tools
```

## 目录

### 基础

0. [创建订阅](./00-sub/)
