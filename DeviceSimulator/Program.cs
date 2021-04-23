using CommandLine;
using Microsoft.Azure.Devices.Client;
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
            await Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                (Options opts) => Process(opts), err => Task.FromResult(0));
        }

        private static async Task<int> Process(Options options)
        {
            var client = DeviceClient.CreateFromConnectionString(options.connectionstring, options.DeviceId);

            await client.SetMethodHandlerAsync("HandleDirectMethod", HandleDirectMethod, null);
            await Task.Run(() => { WaitForKeyPress(); });
            return await Task.FromResult(1);
        }

        public static void WaitForKeyPress()
        {
            Console.WriteLine("Press Enter to exit.");

            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do
            {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                // Wait for an enter
            } while (cki.Key != ConsoleKey.Enter);

        }

        private static Task<MethodResponse> HandleDirectMethod(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            Console.WriteLine($"Received direct method: {data}");

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Executed DirectMethod successfully"), 200));
        }
    }
}
