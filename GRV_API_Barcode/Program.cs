using Barcoded;
using Serilog;
using Serilog.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateBootstrapLogger();

Log.Information("Starting Web Host Barcodes...");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Host.UseSerilog((context, services, configuration) => configuration
   .ReadFrom.Configuration(context.Configuration)
   .ReadFrom.Services(services)
   .Enrich.FromLogContext());

var app = builder.Build();
app.UseSerilogRequestLogging(configure =>
{
    configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} ({UserId}) responded {StatusCode} in {Elapsed:0.0000}ms";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapGet("/", () =>
{
    return Results.Ok("GRV_API_BARCODES.");
})
.WithName("Home")
.WithOpenApi();

app.MapGet("api/v1/barcode/{code}", (string code, int rotation) =>
{

    var codeClean = code.Replace('Ê', ' ').TrimStart();

    LinearBarcode newBarcode = new LinearBarcode(codeClean, Symbology.GS1128)
    {
        Encoder =
        {
            Dpi = 300,
            BarcodeHeight = 200,
        }
    };
    ///
    /// Si es necesario mostrar la etiqueta en formato leible humano
    ///
    //var codeSub = code.Replace("Ê9906", "(9906)");
    //newBarcode.Encoder.HumanReadableValue = codeSub;
    //newBarcode.Encoder.SetHumanReadablePosition("Above");
    //newBarcode.Encoder.SetHumanReadableFont("Arial", 8);
    newBarcode.Encoder.ShowEncoding = false;
    var image = newBarcode.SaveImage("JPG");
    Image img = Image.Load(image);
    img.Mutate(x => x.Rotate(rotation));
    using MemoryStream memoryStream = new MemoryStream();
    img.Save(memoryStream, new JpegEncoder());
    return Results.Bytes(memoryStream.ToArray(), "image/jpeg");
})
.WithName("GetBarcode")
.WithOpenApi();

app.Run();