using AngleSharp.Dom;
using Comments.API.Controllers;
using Comments.API.Middleware;
using Comments.API.Service;
using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;
using Comments.Core.Entities;
using Comments.Core.Interfaces;
using Comments.Infrastructure.Data;
using Comments.Infrastructure.Extensions;
using Comments.Infrastructure.Mappings;
using Comments.Infrastructure.Repositories;
using Comments.Infrastructure.Services;
using Comments.Infrastructure.Validators;
using Hellang.Middleware.ProblemDetails;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using Nest;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.WebSockets;

namespace Comments.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/comments-api-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Host.UseSerilog();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.OperationFilter<CaptchaSchemaFilter>();
            });
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Redis Cache
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["Redis__ConnectionString"];
                options.InstanceName = "CommentsCache:";
            });
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<CommentCreatedConsumer>(); 

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(builder.Configuration["RabbitMQ:Username"]!);
                        h.Password(builder.Configuration["RabbitMQ:Password"]!);
                    });

                    cfg.ReceiveEndpoint("comment-created-queue", e =>
                    {
                        e.ConfigureConsumer<CommentCreatedConsumer>(context);
                    });
                });
            });

            builder.Services.AddSignalR(); 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000",
                            "http://localhost:3001", "https://localhost:7002")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            builder.Services.AddOpenApi();
            builder.Services.AddSingleton<CustomWebSocketManager>();
            // Repositories
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICaptchaRepository, CaptchaRepository>();
            // Services
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ICaptchaService, CaptchaService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
            // Validators
            builder.Services.AddScoped<CreateCommentRequestValidator>();
            builder.Services.AddScoped<GetCommentsRequestValidator>();
            builder.Services.AddScoped<PagedListConverter<Comment, CommentResponse>>();
            // Exception Handling
            builder.Services.AddCustomExceptionHandling(builder.Environment);
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();
            builder.Services.AddElasticsearch(builder.Configuration);

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["Redis__ConnectionString"];
                options.InstanceName = "CommentsCache:";
            });

            var redisConfig = builder.Configuration["Redis__ConnectionString"];
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
            var app = builder.Build();
            app.MapHub<CommentHub>("/hubs/comments");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Comments API V1");
                    c.DocumentTitle = "Comments API - with WebSocket at /ws for real-time";
                    c.InjectJavascript("/swagger-ui/captcha-preview.js");
                    c.DisplayOperationId();
                    c.EnableTryItOutByDefault();
                });
            }
            app.UseCors("ReactApp");
            app.UseProblemDetails();
           // app.MapGraphQL();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/health");
            // Database migration
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }
            try
            {
                Log.Information("Starting Comments API");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }


    public class CaptchaSchemaFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId == "CaptchaController_GenerateCaptcha") 
            {
                operation.Responses["200"].Content["application/json"].Example = new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["data"] = new OpenApiObject
                    {
                        ["captchaId"] = new OpenApiString("123"),
                        ["imageData"] = new OpenApiString("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="),
                        ["expiresAt"] = new OpenApiString(DateTime.UtcNow.AddMinutes(5).ToString("o"))
                    }
                };

                operation.Extensions.Add("x-captcha-preview", new OpenApiObject
                {
                    ["html"] = new OpenApiString(
                        "<div style='margin-top:10px; text-align:center;'>" +
                        "<img id='swagger-captcha-img' src='' style='border:1px solid #ccc; border-radius:4px; max-width:200px;' />" +
                        "<p><small>¬ведите код с изображени€ выше</small></p>" +
                        "</div>" +
                        "<script>" +
                        "setTimeout(() => {" +
                        "  const img = document.getElementById('swagger-captcha-img');" +
                        "  const example = document.querySelector('.response-col_description pre code');" +
                        "  if (example && img) {" +
                        "    try {" +
                        "      const json = JSON.parse(example.textContent);" +
                        "      if (json.data?.imageData) {" +
                        "        img.src = 'data:image/png;base64,' + json.data.imageData;" +
                        "      }" +
                        "    } catch (e) { console.log('CAPTCHA parse error', e); }" +
                        "  }" +
                        "}, 500);" +
                        "</script>"
                    )
                });
            }
        }
    }
}