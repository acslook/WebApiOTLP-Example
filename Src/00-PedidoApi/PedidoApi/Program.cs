using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using WebApiOTLP_Example;
using OpenTelemetry.Logs;

// This is required if the collector doesn't expose an https endpoint. By default, .NET
// only allows http2 (required for gRPC) to secure endpoints.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GeradorPedido>();

// Add services to the container.
// Register the Instrumentation class as a singleton in the DI container.
builder.Services.AddSingleton<Instrumentation>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OTLP
var tracingOtlpGrpcEndpoint = "http://otel-collector:4317";
var otel = builder.Services.AddOpenTelemetry();

// Configure OpenTelemetry Resources with the application name
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

// Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
otel.WithMetrics(metrics => metrics
    // Metrics provider from OpenTelemetry
    .AddRuntimeInstrumentation()
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("PedidoApiMetrics")
    // Metrics provides by ASP.NET Core in .NET 8
    // .AddMeter("Microsoft.AspNetCore.Hosting")
    // .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    
    .AddOtlpExporter(otlpOptions => 
    {
        otlpOptions.Endpoint = new Uri(tracingOtlpGrpcEndpoint);
    })
    );

// Add Tracing for ASP.NET Core and our custom ActivitySource and export to Jaeger
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource("PedidoApi");
    if (tracingOtlpGrpcEndpoint != null)
    {
        tracing.AddOtlpExporter(otlpOptions =>
         {
             otlpOptions.Endpoint = new Uri(tracingOtlpGrpcEndpoint);             
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
        otlpOptions.Endpoint = new Uri(tracingOtlpGrpcEndpoint);                
        processorOptions.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 2000;
        //processorOptions.BatchExportProcessorOptions.MaxExportBatchSize = 512;
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (true)//(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
