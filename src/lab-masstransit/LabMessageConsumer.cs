using System.Threading.Tasks;
using MassTransit;

namespace lab_masstransit
{
    internal class LabMessageConsumer : IConsumer<LabMessage>
    {
        public Task Consume(ConsumeContext<LabMessage> context)
        {
            var msg = context.Message;

            System.Console.WriteLine($"Received Message: {msg.Information} (authored: {msg.DateTime})");

            return Task.FromResult(context.Message);
        }
    }
}