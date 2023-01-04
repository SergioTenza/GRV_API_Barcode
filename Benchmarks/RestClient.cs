using System.Net.Http.Headers;

namespace Benchmarks
{
    public class RestClient
    {
        private static readonly HttpClient client = new HttpClient();
        public async Task GenerateBarcodeAsync()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var code = "Ê990600382426";
            var rotation = 90;
            await client.GetAsync($"http://localhost:5011/api/v1/barcode/{code}?rotation={rotation}");
        }
    }
}
