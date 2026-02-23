using Libra.Server.Middleware;
using Libra.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers(options =>
{
    // 注册全局异常过滤器
    options.Filters.Add<Libra.Server.Filters.GlobalExceptionFilter>();
    // 注册模型绑定异常过滤器
    options.Filters.Add<Libra.Server.Filters.ModelBindingExceptionFilter>();
});

// 配置API行为选项，自定义模型绑定错误处理
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var response = new Libra.Server.Models.API.ApiResponse<string>
        {
            Code = Libra.Server.Enum.LibraStatusCode.BadRequest,
            Message = "请求参数格式错误，请检查数据类型是否正确",
            Data = string.Empty,
            Timestamp = DateTime.Now.ToUnixTimestamp()
        };

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
    };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Initialize TOTP config (check and generate secret key if not exists)
Libra.Server.Service.TotpConfigManager.Initialize();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseErrorHandling();

// 自定义BadRequest处理程序，处理模型绑定错误
app.Use(async (context, next) =>
{
    await next();

    // 检查是否是400错误
    if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
    {
        // 检查是否已经是我们自定义的响应格式
        if (!context.Response.ContentType?.Contains("application/json") ?? true)
        {
            // 重置响应
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var response = new Libra.Server.Models.API.ApiResponse<string>
            {
                Code = Libra.Server.Enum.LibraStatusCode.BadRequest,
                Message = "请求参数格式错误，请检查数据类型是否正确",
                Data = string.Empty,
                Timestamp = DateTime.Now.ToUnixTimestamp()
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
});

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();
