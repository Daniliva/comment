using Comments.API.Middleware;
using Comments.API.Service;
using Comments.Application.Services;
using Comments.Core.Interfaces;
using Comments.Infrastructure.Data;
using Comments.Infrastructure.Mappings;
using Comments.Infrastructure.Repositories;
using Comments.Infrastructure.Services;
using Comments.Infrastructure.Validators;
using Hellang.Middleware.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


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
            // Add services to the container.

            builder.Host.UseSerilog();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            builder.Services.AddOpenApi();
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

            // Exception Handling
            builder.Services.AddCustomExceptionHandling(builder.Environment);
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseProblemDetails();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("ReactApp");
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
}
