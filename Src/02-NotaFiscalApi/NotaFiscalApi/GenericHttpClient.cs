using System.Text;

namespace WebApiOTLP_Example
{
    public class GenericHttpClient
    {
        private readonly HttpClient _httpClient;

        public GenericHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAsync(string baseAddress, string url)
        {
            Console.WriteLine($"baseAddress: {baseAddress}");
            Console.WriteLine($"url: {url}");

            _httpClient.BaseAddress = new Uri(baseAddress);

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode()
                .WriteRequestToConsole();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");

            return jsonResponse;
        }

        public async Task PostAsync(string baseAddress, string url, string body)
        {
            Console.WriteLine($"baseAddress: {baseAddress}");
            Console.WriteLine($"url: {url}");

            _httpClient.BaseAddress = new Uri(baseAddress);

            using StringContent jsonContent = new(
                body,
                Encoding.UTF8,
                "application/json");

            using HttpResponseMessage response = await _httpClient.PostAsync(url, jsonContent);

            response.EnsureSuccessStatusCode()
                .WriteRequestToConsole();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");
        }
    }
}
