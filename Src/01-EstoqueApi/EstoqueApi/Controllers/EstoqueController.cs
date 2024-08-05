using Bogus;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace WebApiOTLP_Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EstoqueController : ControllerBase
    {
        private readonly ILogger<EstoqueController> _logger;        

        private ActivitySource activitySource;

        public EstoqueController(ILogger<EstoqueController> logger, Instrumentation instrumentation)
        {
            this._logger = logger;
            this.activitySource = instrumentation.ActivitySource;
        }     

        [HttpPost]
        public async Task<IActionResult> Post(Pedido pedido)
        {            
            var traceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            var tags = new List<KeyValuePair<string, object?>>
            { 
                KeyValuePair.Create<string, object>("api_name", "estoque"),
                KeyValuePair.Create<string, object>("request_id", traceId)
            }.ToArray();

            var meter = new Meter("EstoqueApiMetrics");

            var counter = meter.CreateCounter<int>("compute_requests");            
            var histogram = meter.CreateHistogram<double>("request_duration", unit: "ms");

            // Measure the number of requests
            counter.Add(1, tags);
            var stopwatch = Stopwatch.StartNew();

            var client = new GenericHttpClient();

            var delay = Convert.ToInt32(Environment.GetEnvironmentVariable("DELAY"));

            await Task.Delay(delay * 1000);

            var baseAddress = Environment.GetEnvironmentVariable("BASE_ADDRESS");
            var url = Environment.GetEnvironmentVariable("URL_POST");

            _logger.LogInformation("Estoque separado para o pedido {pedidoId}", pedido.Id);

            if (baseAddress != null)
                await client.PostAsync(baseAddress, url, JsonSerializer.Serialize(pedido));

            // Measure the duration in ms of requests and includes the host in the tags
            histogram.Record(stopwatch.ElapsedMilliseconds, tags);

            return Ok();
        }
    }
}
