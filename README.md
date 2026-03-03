# Libra

- [中文文档](doc/README.md)

A remote management and monitoring framework based on .NET 10, adopting C/S architecture, supporting real-time screen monitoring, camera streaming, file management, remote Shell and other functions.

## Project Structure

```
Libra.sln
├── Libra.Server        # ASP.NET Core Server
├── Libra.Agent         # Windows Client
├── Libra.Virgo         # Shared Transport Protocol Library
├── Libra.AppHost       # .NET Aspire Orchestration
└── frontend/           # React + TypeScript Frontend
```

## Tech Stack

- **Server**: ASP.NET Core 10, JWT Authentication, OpenTelemetry, SSE Streaming
- **Agent**: .NET 10 AOT
- **Transport**: TCP + 4-byte big-endian length prefix + JSON, custom Virgo protocol
- **Frontend**: React, TypeScript, HeroUI

## Features

- Real-time screen monitoring (differential frame compression, SSE streaming)
- Real-time camera streaming (multiple cameras, adjustable FPS)
- Remote Shell execution
- File browsing and downloading
- Process management
- TOTP two-factor authentication

## Quick Start

### Environment Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for frontend)
- Windows (Agent only supports Windows)

### 1. Clone the Project

```bash
git clone <repo-url>
cd Libra
```

### 2. Start the Server

```bash
dotnet run --project Libra.Server
```

The server listens for Agent TCP connections on port `8888` by default, and the HTTP API is served on the default port.

### 3. Start the Frontend

```bash
cd frontend
npm install
npm run dev
```

### 4. Start the Agent (Windows)

```bash
dotnet run --project Libra.Agent
```

The Agent will automatically connect to the server and register after startup.

### Using Aspire Orchestration (Optional)

```bash
dotnet run --project Libra.AppHost
```

One-click start of all services.

### AOT Publish Agent

```bash
dotnet publish Libra.Agent -c Release -r win-x64
```

### Known Bugs:
- 1. When publishing Libra.Agent with AOT, FlashCap will be code trimmed, causing camera monitoring to be unavailable.
- Solution: Do not trim code during publishing, but this will increase the generated file size

## Screenshots
![1](doc/image/1.png)
![2](doc/image/2.png)
![3](doc/image/3.png)
![4](doc/image/4.png)
![5](doc/image/5.png)
![6](doc/image/6.png)
![7](doc/image/7.png)

## Disclaimer

This project is for security research, teaching demonstrations, and authorized testing only. Users must comply with the laws and regulations of their respective regions.

**Before using this project, please confirm:**

1. You have obtained explicit written authorization from the owner of the target system
2. Your usage scenario complies with local laws and regulations
3. You will not use this project for any unauthorized access, monitoring, or attack behavior

**Author's Statement:**

- This project is provided "as is" without any express or implied warranties
- The author is not responsible for any direct or indirect losses caused by the use or abuse of this project
- Any consequences arising from the use of this project in violation of laws and regulations shall be borne by the user
- This project does not encourage or support any illegal activities

Unauthorized access, monitoring, or control of computer systems is illegal and may result in serious criminal and civil liability.

## License

This project is licensed under the [GPL-3.0](LICENSE) license.


## Donation
- If you like this project, you can choose to buy me a coffee to support my continued development.
![qrcode](doc/image/like.png)

- In the name of Jasmine, I profess my love to Libra, may you not forget me. Spring River Moon.
