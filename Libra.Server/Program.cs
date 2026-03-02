using Libra.Server.Middleware;
using Libra.Server.Extensions;
using Libra.Virgo;
using Libra.Virgo.Models;
using Newtonsoft.Json;
using Libra.Server.Service.Auth;

namespace Libra.Server
{
    public static class Program
    {
        public static DateTime StartTime { get; } = DateTime.Now;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddServiceDefaults();

            builder.Services.AddProblemDetails();
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<Libra.Server.Filters.GlobalExceptionFilter>();
                options.Filters.Add<Libra.Server.Filters.ModelBindingExceptionFilter>();
            });

            builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

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

            builder.Services.AddOpenApi();

            TotpConfigManager.Initialize();

            Runtimes.Initialize(8888);

            var app = builder.Build();

            app.UseErrorHandling();

            app.UseCors("AllowAll");

            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    if (!context.Response.ContentType?.Contains("application/json") ?? true)
                    {
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
            app.UseEndpoints(static endpoints =>
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
        }
    }
}
