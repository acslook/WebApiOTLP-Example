using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using WebApiOTLP_Example;
using OpenTelemetry.Logs;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register the Instrumentation class as a singleton in the DI container.
builder.Services.AddSingleton<Instrumentation>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OTLP
var tracingOtlpEndpoint = "http://otel-collector:4317";
var otel = builder.Services.AddOpenTelemetry();

// Custom ActivitySource for the application
var greeterActivitySource = new ActivitySource("NotaFiscalApi");

// Configure OpenTelemetry Resources with the application name
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

// Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
otel.WithMetrics(metrics => metrics
    // Metrics provider from OpenTelemetry
    .AddAspNetCoreInstrumentation()
    //.AddMeter("NotaFiscalApi.NovosPedidos")
    // Metrics provides by ASP.NET Core in .NET 8
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddOtlpExporter(otlpOptions => 
    {
        otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
    })
    );

// Add Tracing for ASP.NET Core and our custom ActivitySource and export to Jaeger
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource(greeterActivitySource.Name);
    if (tracingOtlpEndpoint != null)
    {
        tracing.AddOtlpExporter(otlpOptions =>
         {
             otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
         });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});

// builder.Services.AddOpenTelemetry()
//     // .UseOtlpExporter(OtlpExportProtocol.HttpProtobuf, new Uri("http://otel-collector:4318/"))
//     .ConfigureResource(resource => resource.AddService(
//         serviceName: serviceName,
//         serviceVersion: serviceVersion))
//     .WithTracing(tracing => tracing
//         .AddOtlpExporter(options =>
//             {
//                 options.Endpoint = new Uri("http://otel-collector:4318");
//                 options.Protocol = OtlpExportProtocol.HttpProtobuf;
//             })
//         .AddSource(serviceName)
//         .AddAspNetCoreInstrumentation()
//         .AddConsoleExporter())
//     .WithMetrics(metrics => metrics
//         .AddOtlpExporter(options =>
//             {
//                 options.Endpoint = new Uri("http://otel-collector:4318");
//                 options.Protocol = OtlpExportProtocol.HttpProtobuf;
//             })
//         .AddMeter(serviceName)
//         .AddAspNetCoreInstrumentation()
//         .AddConsoleExporter());

// builder.Logging.AddOpenTelemetry(options => options
//     .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
//         serviceName: serviceName,
//         serviceVersion: serviceVersion))
//     .AddConsoleExporter());

var app = builder.Build();

// app.MapPrometheusScrapingEndpoint();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
