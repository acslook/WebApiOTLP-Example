using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace WebApiOTLP_Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PedidoController : ControllerBase
    {        
        private readonly ILogger<PedidoController> _logger;        
        private readonly GeradorPedido _geradorPedido;

        private ActivitySource activitySource;

        public PedidoController(ILogger<PedidoController> logger, GeradorPedido geradorPedido)
        {
            this._logger = logger;
            this.activitySource = new ActivitySource("PedidoApi");
            this._geradorPedido = geradorPedido;
        }     

        [HttpPost]
        public async Task<Pedido> Post()
        {
            var traceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            
            using var activity = activitySource.StartActivity("Get data");
            activity?.AddTag("sample", "value");

            var tags = new List<KeyValuePair<string, object?>>
            { 
                KeyValuePair.Create<string, object>("api_name", "pedido"),
                KeyValuePair.Create<string, object>("request_id", traceId)
            }.ToArray();
            var meter = new Meter("PedidoApiMetrics");            
            
            var counter = meter.CreateCounter<int>("compute_requests");            
            var histogram = meter.CreateHistogram<double>("request_duration", unit: "ms");
            meter.CreateObservableGauge("ThreadCount", () => new[] { new Measurement<int>(ThreadPool.ThreadCount) });

            // Measure the number of requests
            counter.Add(1, tags);
            var stopwatch = Stopwatch.StartNew();

            var client = new GenericHttpClient();

            var delay = Convert.ToInt32(Environment.GetEnvironmentVariable("DELAY"));

            await Task.Delay(delay * 1000);

            var baseAddress = Environment.GetEnvironmentVariable("BASE_ADDRESS");
            var url = Environment.GetEnvironmentVariable("URL_POST");                

            var pedido = _geradorPedido.GerarNovoPedido();    
            var pedidoJson = JsonSerializer.Serialize(pedido);
            if (baseAddress != null)
                await client.PostAsync(baseAddress, url, pedidoJson);

            _logger.LogInformation("Novo pedido criado: {pedido}", pedidoJson);

            // Measure the duration in ms of requests and includes the host in the tags
            histogram.Record(stopwatch.ElapsedMilliseconds, tags);

            return pedido;
        }

        [HttpGet]
        public async Task<List<Pedido>> Get()
        {
            return _geradorPedido.GetPedidos();
        }
    }
}
