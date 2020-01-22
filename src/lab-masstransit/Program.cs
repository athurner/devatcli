﻿using System;
using System.Threading.Tasks;
using MassTransit;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace lab_masstransit
{
    class Program
    {
        private static int Main(string[] args)
        {
            var appName = "LAB MassTransit CLI";
            var versionInfo = $"1.0.5";

            var app = new CommandLineApplication(false)
            {
                Name = appName,
                Description = "The command line interface for testing MasTransit"
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

            //var serviceCollection = new ServiceCollection();
            //ConfigureServices(serviceCollection);
            //var serviceProvider = serviceCollection.BuildServiceProvider();
            //var logger = serviceProvider.GetService<ILogger<Program>>();

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

                                var uri = new Uri(string.Concat(busControl.Address.AbsoluteUri, "/", queueOpt.Value()));
                                logger.LogInformation($"Publishing to Message Bus at {uri}");

                                logger.LogInformation("Sending ... to Message Bus");
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

                                logger.LogInformation("Finnished publishing to Message Bus");
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
                                logger.LogInformation("Subscribing to Message Bus");

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

        //private static void ConfigureServices(IServiceCollection services)
        //{
        //}
    }
}