using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.AWS.SQS;
using OpenMessage.Samples.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Samples.AWS
{
    internal class Program
    {
        private static int _counter;

        private static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "XXX", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "XXX", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "XXX", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", "eu-west-2", EnvironmentVariableTarget.Process);

            var verbose = false;

            await Host.CreateDefaultBuilder()
                      .ConfigureServices(services => services.AddOptions()
                                                             .AddLogging()
                                                             .AddSampleCore()
                                                             .AddMassProducerService<SimpleModel>() // Adds a producer that calls configured dispatcher
                      )
                      .ConfigureMessaging(host =>
                      {
                          // Adds a handler that writes to console every 100 messages
                          host.ConfigureHandler<CoreModel>(msg =>
                          {
                              var counter = Interlocked.Increment(ref _counter);

                              if (verbose)
                              {
                                  var properties = msg is ISupportProperties sp
                                      ? sp.Properties
                                      : Enumerable.Empty<KeyValuePair<string, string>>();

                                  Console.WriteLine($"Received: #{counter} Received: {DateTime.UtcNow} Properties: {string.Join(",", properties)}");
                              }
                              else if(counter % 100 == 1)
                              {
                                  Console.WriteLine($"Received: #{counter}");
                              }
                          });

                          // Allow us to write to SNS
                          // host.ConfigureSnsDispatcher<SimpleModel>()
                          //     .FromConfiguration(config =>
                          //     {
                          //         config.TopicArn = "arn:aws:sns:eu-west-2:000000000000:openmessage_samples_core_models_simplemodel";
                          //         config.ServiceURL = "http://localhost:4575";
                          //     })
                          //     .Build();

                          // For testing the dispatchers
                          host.ConfigureSqsDispatcher<SimpleModel>()
                              .FromConfiguration(config =>
                              {
                                  config.QueueUrl = "http://localhost:4576/000000000000/openmessage_samples_core_models_simplemodel.queue";
                                  config.ServiceURL = "http://localhost:4576";
                              })
                              .WithBatchedDispatcher(true)
                              .Build();

                          // Consume from the same topic as we are writing to
                          host.ConfigureSqsConsumer<CoreModel>()
                              .FromConfiguration(config =>
                              {
                                  config.QueueUrl = "http://localhost:4576/000000000000/openmessage_samples_core_models_simplemodel.queue";
                                  config.ServiceURL = "http://localhost:4576";
                              })
                              .Build();
                      })
                      .Build()
                      .RunAsync();
        }
    }
}