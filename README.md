# Libra 天秤座

一个基于 .NET 10 的远程管理与监控框架，采用 C/S 架构，支持实时屏幕监控、摄像头流、文件管理、远程Shell等功能。

## 项目结构

```
Libra.sln
├── Libra.Server        # ASP.NET Core 服务端（API + TCP 监听）
├── Libra.Agent         # Windows 客户端（AOT 编译）
├── Libra.Virgo         # 共享传输协议库
├── Libra.AppHost       # .NET Aspire 编排
└── frontend/           # React + TypeScript 前端
```

## 技术栈

- **Server**: ASP.NET Core 10, JWT 认证, OpenTelemetry, SSE 推流
- **Agent**: .NET 10 AOT, Media Foundation (摄像头), System.Drawing (屏幕捕获)
- **Transport**: TCP + 4字节大端长度前缀 + JSON, 自定义 Virgo 协议
- **Frontend**: React, TypeScript, HeroUI

## 功能

- 实时屏幕监控（差异帧压缩，SSE 推流）
- 摄像头实时流（多摄像头，可调 FPS）
- 远程 Shell 执行
- 文件浏览与下载
- 进程管理
- Agent 在线构建与二进制注入（IP/端口/Token）
- TOTP 双因素认证

## 快速启动

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (前端)
- Windows（Agent 仅支持 Windows）

### 1. 克隆项目

```bash
git clone <repo-url>
cd Libra
```

### 2. 启动服务端

```bash
dotnet run --project Libra.Server
```

服务端默认在 `8888` 端口监听 Agent TCP 连接，HTTP API 在默认端口提供服务。

### 3. 启动前端

```bash
cd frontend
npm install
npm run dev
```

### 4. 启动 Agent（Windows）

```bash
dotnet run --project Libra.Agent
```

Agent 启动后会自动连接服务端并注册。

### 使用 Aspire 编排（可选）

```bash
dotnet run --project Libra.AppHost
```

一键启动所有服务。

### AOT 发布 Agent

```bash
dotnet publish Libra.Agent -c Release -r win-x64
```

## API 概览

所有接口需 JWT 认证。

| 端点 | 方法 | 说明 |
|------|------|------|
| `/api/v1/agents/online` | GET | 获取在线 Agent 列表 |
| `/api/v1/agents/build` | POST | 构建自定义 Agent |
| `/api/v1/monitor/frame/{agentId}` | GET | 获取单帧屏幕截图 |
| `/api/v1/monitor/stream/{agentId}` | GET | 屏幕实时流 (SSE) |
| `/api/v1/monitor/camera/{agentId}` | GET | 获取单帧摄像头画面 |
| `/api/v1/monitor/camera/stream/{agentId}` | GET | 摄像头实时流 (SSE) |

## 免责声明

本项目仅供安全研究、教学演示和授权测试使用。使用者必须遵守所在地区的法律法规。

**在使用本项目前，请确认：**

1. 你已获得目标系统所有者的明确书面授权
2. 你的使用场景符合当地法律法规
3. 你不会将本项目用于任何未经授权的访问、监控或攻击行为

**作者声明：**

- 本项目按"原样"提供，不提供任何形式的明示或暗示担保
- 作者不对因使用或滥用本项目造成的任何直接或间接损失承担责任
- 任何因违反法律法规使用本项目所产生的后果，由使用者自行承担
- 本项目不鼓励、不支持任何非法活动

未经授权对计算机系统进行访问、监控或控制是违法行为，可能导致严重的刑事和民事责任。

## 协议

本项目采用 [GPL-3.0](LICENSE) 许可证。
