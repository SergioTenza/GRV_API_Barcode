using Barcoded;
using GRV_API_Barcode.Domain.Modelos;
using GRV_API_Barcode.Events.Consumers;
using GRV_API_Barcode.Events.Producers;
using GRV_API_Barcode.Infraestructura.Contracts.Repositorios;
using GRV_API_Barcode.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text;

const string API_KEY = "0dbfcb316105a903cfa4fc88a7ff62b028e7f7f2c60665bff76973a71c509872";

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
builder.Services.AddSingleton<IRepository<Peticion>, InMemoryRepository>();
var app = builder.Build();

IRepository<Peticion> repository = ActivatorUtilities.GetServiceOrCreateInstance<InMemoryRepository>(app.Services);
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

// RABBITMQ DECLARATION 
var rabbitmq = new ConnectionFactory() { HostName = "intranet.garvalin.local" };
var connection = rabbitmq.CreateConnection();
var channel = connection.CreateModel();
QueueConsumer.Consume(channel, app.Services, repository);
QueueProducer.Publish(channel);

//channel.ExchangeDeclare(exchange: "crystal-reports", type: ExchangeType.Topic);

//channel.QueueBind(
//    queue: "consumer-api",
//    exchange: "crystal-reports",
//    routingKey: "producer-api");
//consumer = new EventingBasicConsumer(channel);
//consumer.Received += (model, ea) =>
//{
//    try
//    {
//        var body = ea.Body.ToArray();
//        var message = Encoding.UTF8.GetString(body);
//        var petition = JsonConvert.DeserializeObject<Peticion>(message);
//        if (petition != null)
//        {
//            if (petition.ApiKey == API_KEY)
//            {
//                Console.WriteLine(" [^] Recibido {0}", body);
//                var repository = ActivatorUtilities.GetServiceOrCreateInstance<InMemoryRepository>(app.Services);
//                var response = repository.GetById(petition.Guid);
//                if (response is null)
//                {
//                    repository.Insert(petition);
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Console.WriteLine($"[ERROR] Ha ocurrido un problema al recibir la peticion {ex.Message}");
//    }

//};


app.MapGet("/", () =>
{
    return Results.Ok("GRV_API_BARCODES.");
})
.WithName("Home")
.WithOpenApi();

app.MapPost("/getpdf", async (HttpRequest request) =>
{
    string body = "";
    Peticion? peticion = null;
    using (StreamReader stream = new StreamReader(request.Body))
    {
        body = await stream.ReadToEndAsync();
        peticion = JsonConvert.DeserializeObject<Peticion>(body);
    }
    
    SendMessage(peticion ?? throw new ArgumentNullException("No hemos podido deserializar la peticion"));
    var canContinue = 0;
    while (canContinue < 20)
    {
        var response = repository.GetById(peticion.Guid);
        if (response is null)
        {
            Thread.Sleep(50);
            canContinue++;
        }
        else
        {
            byte[] bytes = Convert.FromBase64String(response.Base64);
            MemoryStream memoryStream = new MemoryStream(bytes);
            return Results.File(memoryStream, "application/pdf",$"{peticion.Guid}.pdf");
            
        }
    }
    return Results.UnprocessableEntity(peticion);

})
.Accepts<Peticion>("application/json")
.WithName("getpdf")
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

void SendMessage(Peticion peticion)
{

    //var message = JsonConvert.SerializeObject(peticion);
    //var body = Encoding.UTF8.GetBytes(message);

    //channel.BasicPublish(exchange: "crystal-reports",
    //                     routingKey: "consumer-console",
    //                     basicProperties: null,
    //                     body: body);
    QueueProducer.SendMessage(peticion);

}

public static class Extensions
{
    public static Stream ConvertToBase64(this Stream stream)
    {
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            bytes = memoryStream.ToArray();
        }

        string base64 = Convert.ToBase64String(bytes);
        return new MemoryStream(Encoding.UTF8.GetBytes(base64));
    }
    public static string ConvertToBase64String(this Stream stream)
    {
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            bytes = memoryStream.ToArray();
        }

        /*string base64 = */
        return Convert.ToBase64String(bytes);
        //return new MemoryStream(Encoding.UTF8.GetBytes(base64));
    }
}
/*
 var rabbitmq = new ConnectionFactory() { HostName = "intranet.garvalin.local" };
using (var connection = rabbitmq.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "crystal-reports", type: ExchangeType.Fanout);
    var queueName = channel.QueueDeclare().QueueName;
    channel.QueueBind(
        queue: queueName,
        exchange: "crystal-reports",
        routingKey: "producer-api");
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var petition = petition(message);
            if (petition != null)
            {
                if (petition.ApiKey == API_KEY)
                {
                    var stream = JsonConvert.DeserializeObject<Peticion>(petition);
                    if (stream != null)
                    {
                        var base64String = stream.ConvertToBase64String();
                        Console.WriteLine(" [x] {0}", base64String);
                        SendMessage(base64String);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ERROR] Ha ocurrido un problema {ex.Message}");
        }
    };
 */