using GRV_API_Barcode.Domain.Modelos;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace GRV_API_Barcode.Events.Producers
{
    public static class QueueProducer
    {
        private static IModel? Channel;
        public static void Publish(IModel channel)
        {
            Channel = channel;
            channel.QueueDeclare("publisher-api",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            Console.WriteLine("publisher-api started");
        }
        
        public static void SendMessage(Peticion petition)
        {
            if (Channel is not null)
            {                
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(petition));
                Channel.BasicPublish("", "publisher-api", null, body);
                System.Console.WriteLine($"[ENVIADO] Enviada correctamente peticion {petition.Guid}");
            }
        }
    }
}
