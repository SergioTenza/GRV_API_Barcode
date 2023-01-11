using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using GRV_API_Barcode.Domain.Modelos;
using GRV_API_Barcode.Repositorios;
using Newtonsoft.Json;
using GRV_API_Barcode.Infraestructura.Contracts.Repositorios;

namespace GRV_API_Barcode.Events.Consumers
{
    public static class QueueConsumer
    {
        public static void Consume(IModel channel,IServiceProvider services,IRepository<Peticion> repository)
        {
            channel.QueueDeclare("consumer-api",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) => {
                System.Console.WriteLine($"[RECIBIDO] consumer-api");
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var petition = JsonConvert.DeserializeObject<Peticion>(message);
                Console.WriteLine(" [^] Recibido {0}", petition);                
                if (petition != null) 
                {   
                    var response = repository.GetById(petition.Guid);
                    if (response is null)
                    {
                        repository.Insert(petition);
                    }
                    Console.WriteLine(" [^] Agregado a repositorio {0}", petition);
                }
            };

            channel.BasicConsume("consumer-api", true, consumer);
            Console.WriteLine("consumer-api started");
        }
    }
}
