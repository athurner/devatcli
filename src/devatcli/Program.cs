using System;
using System.Threading.Tasks;
using MassTransit;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using devatcli.DataModel;

namespace devatcli
{
    class Program
    {
        private static int Main(string[] args)
        {
            var appName = "DevAT Swiss-Army-Knife CLI ";
            var versionInfo = $"1.0.0";

            var app = new CommandLineApplication(false)
            {
                Name = appName,
                Description = "The swiss army knife for development and testing"
            };

            app.HelpOption("-?|-h|--help");

            // This is a helper/shortcut method to display version info - it is creating a regular Option, with some defaults.
            // The default help text is "Show version Information"
            app.VersionOption(
                "-v|--version",
                () =>
                {
                    return $"Version {versionInfo}";
                });

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddSerilog(
                    new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger(),
                    true);
            });

            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation($"{appName} - {versionInfo}");

            app.Command(
                "efsql",
                (cmd) =>
                {                    
                    cmd.Description = "using Entity Framework to access SQL Server";
                    cmd.HelpOption("-?|-h|--help");

                    var hostOpt = cmd.Option("-s|--server", "The server name.", CommandOptionType.SingleValue);
                    var dbOpt = cmd.Option("-d|--database", "The database.", CommandOptionType.SingleValue);
                    var userOpt = cmd.Option("-u|--user", "The user.", CommandOptionType.SingleValue);
                    var passwordOpt = cmd.Option("-pw|--password", "The password.", CommandOptionType.SingleValue);
                    //var querOpt = cmd.Option("-q|--query", "The tsql quey", CommandOptionType.SingleValue);
                    
                    cmd.OnExecute(async () =>
                        {
                            var connectionString = $"SERVER={hostOpt.Value()};Database={dbOpt.Value()};User={userOpt.Value()};Password={passwordOpt.Value()}";

                            var options = new DbContextOptionsBuilder<NorthwindContext>()
                                    .UseSqlServer(connectionString)
                                    .UseLoggerFactory(loggerFactory) 
                                    .Options;
                            using (var db = new NorthwindContext(options))
                            {
                                var products = await db.Products
                                                        .ToListAsync();

                                foreach(var p in products)
                                {
                                    logger.LogInformation(p.ProductName);
                                }
                            }
                        });
                });

            app.Command(
                "rabbitmq",
                (cmd) =>
                {
                    cmd.Description = "using RabbitMQ as messaging service";
                    cmd.HelpOption("-?|-h|--help");

                    var hostOpt = cmd.Option("-s|--server", "The server name.", CommandOptionType.SingleValue);
                    var portOpt = cmd.Option("-p|--port", "The port number.", CommandOptionType.SingleValue);
                    portOpt.Validators.Add(new UshortValidator());
                    var vhostOpt = cmd.Option("-v|--virtualhost", "The virtual host.", CommandOptionType.SingleValue);

                    var userOpt = cmd.Option("-u|--user", "The user.", CommandOptionType.SingleValue);
                    var passwordOpt = cmd.Option("-pw|--password", "The password.", CommandOptionType.SingleValue);

                    cmd.Command(
                        "publish",
                        (pubcmd) =>
                        { 
                            pubcmd.Description = "publish to a message bus.";
                            pubcmd.HelpOption("-?|-h|--help");

                            var msgOpt = pubcmd.Option("-m|--message", "The message to publish.", CommandOptionType.SingleValue);
                            var queueOpt = pubcmd.Option("-q|--queue", "The queue name", CommandOptionType.SingleValue);

                            pubcmd.OnExecute(async () =>
                            {
                                var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                                {
                                    cfg.SetLoggerFactory(loggerFactory);
                                    var host = cfg.Host(
                                        hostOpt.Value(),
                                        ushort.Parse(portOpt.Value()),
                                        vhostOpt.Value(),
                                        h =>
                                        {
                                            h.Username(userOpt.Value());
                                            h.Password(passwordOpt.Value());
                                        });

                                });

                                var uri = new Uri($"rabbitmq://{hostOpt.Value()}:{portOpt.Value()}/{vhostOpt.Value()}/{queueOpt.Value()}");

                                try
                                {
                                    await busControl.StartAsync();

                                    var sendEndpoint = await busControl.GetSendEndpoint(uri);

                                    await sendEndpoint.Send(new LabMessage
                                    {
                                        Information = msgOpt.Value()
                                    });

                                }
                                finally
                                {
                                    await busControl.StopAsync();
                                }

                                return 0;
                            });
                        });

                    cmd.Command(
                        "subscribe",
                        (subcmd) =>
                        {
                            subcmd.Description = "subscribe to a message bus.";
                            subcmd.HelpOption("-?|-h|--help");

                            var queueOpt = subcmd.Option("-q|--queue", "The queue name", CommandOptionType.SingleValue);

                            subcmd.OnExecute(async () =>
                            {
                                var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                                {
                                    cfg.SetLoggerFactory(loggerFactory);
                                    var host = cfg.Host(
                                        hostOpt.Value(),
                                        ushort.Parse(portOpt.Value()),
                                        vhostOpt.Value(),
                                        h =>
                                        {
                                            h.Username(userOpt.Value());
                                            h.Password(passwordOpt.Value());
                                        });

                                    cfg.ReceiveEndpoint(host, queueOpt.Value(), conf =>
                                    {
                                        conf.Consumer<LabMessageConsumer>();
                                    });
                                });
                                try
                                {
                                    await busControl.StartAsync();

                                    logger.LogInformation("Hit any key to stop listening");
                                    Console.ReadKey();
                                }
                                finally
                                {
                                     await busControl.StopAsync();
                                }

                                return 0;
                            });
                        });
                });

            return app.Execute(args);
        }
    }
}
