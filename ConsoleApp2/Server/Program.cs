using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/Log.txt", rollingInterval: RollingInterval.Year)
    .CreateLogger();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(dispose: true));

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Остановки города Минска";
});

app.Run();
