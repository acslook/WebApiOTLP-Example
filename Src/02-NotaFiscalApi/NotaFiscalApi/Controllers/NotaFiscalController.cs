using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace WebApiOTLP_Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotaFiscalController : ControllerBase
    {
        private readonly ILogger<NotaFiscalController> _logger;

        private ActivitySource activitySource;

        public NotaFiscalController(ILogger<NotaFiscalController> logger)
        {
            this._logger = logger;            
        }     

        [HttpPost]
        public async Task<IActionResult> Post(Pedido pedido)
        {            
            var client = new GenericHttpClient();

            var delay = Convert.ToInt32(Environment.GetEnvironmentVariable("DELAY"));

            await Task.Delay(delay * 1000);

            var baseAddress = Environment.GetEnvironmentVariable("BASE_ADDRESS");
            var url = Environment.GetEnvironmentVariable("URL_POST");

            _logger.LogInformation($"Nota fiscal emitida para o pedido {pedido.Id}");

            if (baseAddress != null)
                await client.PostAsync(baseAddress, url, JsonSerializer.Serialize(pedido));

            return Ok();
        }
    }
}
