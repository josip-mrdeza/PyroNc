using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Updates;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine.Device;

namespace Pyro.Nc.Configuration.Statistics
{
    public static class Collector
    {
        private static Stopwatch _time;
        public static readonly HttpClient HttpClient = new HttpClient();
        public static string BaseAddress;
        public static string ID;

        public static void Init()
        {
            _time = Stopwatch.StartNew();
            Globals.Variables.AddVariable("baseAddress.txt");
            Globals.Variables.AddVariable("machineId.txt", Environment.MachineName);
            PyroConsoleView.PushTextStatic("Registered:\n" +
                                           "    --baseAddress.txt\n" +
                                           "    --machineId.txt");
            BaseAddress = Globals.Variables.GetVariableDataAsString("baseAddress.txt");
            ID = Globals.Variables.GetVariableDataAsString("machineId.txt");
            
            PyroConsoleView.PushTextStatic("Read Registered Data:\n" +
                                           $"    --baseAddress.txt -> {BaseAddress},\n" +
                                           $"    --machineId.txt -> {ID}");
        }

        public static async Task PushToLog(this Task<HttpResponseMessage> message)
        {
            var msg = await message;
            PyroConsoleView.PushTextStatic($"HttpResult: {msg.ReasonPhrase} - {msg.StatusCode.ToString()}!");
        }

        public static async Task SendTimeStatisticAsync()
        { 
            await HttpClient.PostAsync(BaseAddress + $"/time/{ID}", new StringContent(_time.Elapsed.TotalMinutes + "min", Encoding.Default, "text/plain")).PushToLog();
        }

        public static async Task SendUsageStatisticAsync()
        {
            await HttpClient.PostAsync(BaseAddress + $"/usage/{ID}", new StringContent((GC.GetTotalMemory(false)/1_000_000).ToString()+"MB", Encoding.Default, "text/plain")).PushToLog();
        }

        public static async Task SendVersionStatisticAsync()
        {
            await HttpClient.PostAsync(BaseAddress + $"/version/{ID}", 
                                       new StringContent(JsonSerializer.Serialize(SoftwareVersion.Info), Encoding.Default, "text/json"));
        }

        public static async Task SendLogStatisticAsync()
        {
            await HttpClient.PostAsync(BaseAddress + $"/logs/{ID}", 
                                       new StringContent(Globals.Variables.GetVariableDataAsString("pyroLog.txt"), Encoding.Default, "text/plain"));
        }
        public static void SendStatisticsPeriodic(int ms = 30_000)
        {
            Task.Run(async () =>
            {
                while (!Process.GetCurrentProcess().HasExited)
                {
                    try
                    {
                        if (!Globals.IsNetworkPresent)
                        {
                            continue;
                        }
                        await SendTimeStatisticAsync();
                        await SendVersionStatisticAsync();
                        await SendUsageStatisticAsync();
                        await SendLogStatisticAsync();
                    }
                    catch (Exception e)
                    {
                        PyroConsoleView.PushTextStatic("An error has occured in Collector.SendStatisticsPeriodic:", e.Message);
                    }
                    finally
                    {
                        await Task.Delay(ms);
                    }
                }
            });
        }
    }
}