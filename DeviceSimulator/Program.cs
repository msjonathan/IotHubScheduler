using CommandLine;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    class Program
    {
        public class Options
        {
            [Option('c', Required = true, HelpText = "Connectionstring of IoTHub")]
            public string connectionstring { get; set; }

            [Option("id", Required = true, HelpText = "DeviceId")]
            public string DeviceId { get; set; }
        }

        static async Task Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    options = opts;
                })
                .WithNotParsed((errs) => throw new ArgumentException(string.Join(Environment.NewLine, errs)));

            var client = DeviceClient.CreateFromConnectionString(options.connectionstring, options.DeviceId);
            await client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null);
            await client.SetMethodHandlerAsync("HandleDirectMethod", HandleDirectMethod, null);

            await Task.Delay(TimeSpan.FromMinutes(10));

        }
        private static async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("\tDesired property changed:");
            Console.WriteLine($"\t{desiredProperties.ToJson()}");
            await Task.CompletedTask;
        }

        private static Task<MethodResponse> HandleDirectMethod(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            Console.WriteLine($"Received direct method: {data}");

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Executed DirectMethod successfully"), 200));
        }
    }
}
