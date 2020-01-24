using System.Threading.Tasks;
using MassTransit;

namespace devatcli
{
    internal class LabMessageConsumer : IConsumer<ILabMessage>
    {
        public Task Consume(ConsumeContext<ILabMessage> context)
        {
            ILabMessage msg = context.Message;

            System.Console.WriteLine($"Received Message: {msg.Information} (authored: {msg.DateTime})");

            return Task.FromResult(context.Message);
        }
    }
}