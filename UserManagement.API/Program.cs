using Scalar.AspNetCore;
using Serilog;
using UserManagement.API.ExceptionHandling;
using UserManagement.API.Interfaces;
using UserManagement.API.Repositories;

public sealed class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        }, preserveStaticLogger: true, writeToProviders: true);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options =>
            {
                options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
            });
        }

        // By default request logging writes to the static Log.Logger, which is not configured
        // because of preserveStaticLogger. Use the application's own logger instead.
        app.UseSerilogRequestLogging(options =>
            options.Logger = app.Services.GetRequiredService<Serilog.ILogger>());
        app.UseExceptionHandler();

        app.MapControllers();

        await app.RunAsync();
    }
}
