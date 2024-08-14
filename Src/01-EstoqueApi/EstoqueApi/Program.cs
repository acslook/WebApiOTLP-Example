using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using WebApiOTLP_Example;
using OpenTelemetry.Logs;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OTLP
var tracingOtlpEndpoint = "http://otel-collector:4317";
var otel = builder.Services.AddOpenTelemetry();

// Custom ActivitySource for the application
var greeterActivitySource = new ActivitySource("EstoqueApi");

// Configure OpenTelemetry Resources with the application name
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

// Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
otel.WithMetrics(metrics => metrics
    // Metrics provider from OpenTelemetry
    .AddAspNetCoreInstrumentation()    
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

otel.WithLogging(logging => 
{                    
    logging.AddOtlpExporter((otlpOptions, processorOptions) =>
    {        
        otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);                
        processorOptions.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 2000;
        processorOptions.BatchExportProcessorOptions.MaxExportBatchSize = 512;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
