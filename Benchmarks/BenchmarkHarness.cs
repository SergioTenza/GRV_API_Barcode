using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [HtmlExporter]
    public class BenchmarkHarness
    {
        [Params(100,500,1000)]
        public int IterationCount;
        private readonly RestClient _restClient = new RestClient();

        [Benchmark]
        public async Task RestGetSmallPayloadAsync()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                await _restClient.GenerateBarcodeAsync();
            }
        }
    }
}
