using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine.Device;

namespace Pyro.Nc.Configuration.Statistics
{
    public static class Collector
    {
        private static Stopwatch _time;
        public static SystemInformation Info;
        public static readonly HttpClient HttpClient = new HttpClient();
        public static string BaseAddress;

        public static void Init()
        {
            _time = Stopwatch.StartNew();
            var roaming = LocalRoaming.OpenOrCreate("PyroNc/PyroSoftwareUpdater");
            var fn = "SystemInformation.json";
            if (!roaming.Exists(fn))
            {
                Process.Start("PyroSoftwareUpdater.exe", "generate").WaitForExit();
            }

            Info = roaming.ReadFileAs<SystemInformation>(fn);
            PyroConsoleView.PushTextStatic("Read system information:",
                                           $"Name: {Info.Name}",
                                           $"Manufacturer: {Info.Manufacturer}",
                                           $"Cpu Name:{Info.ProcessorName}",
                                           $"Cpu Info: {Info.CpuInfo}",
                                           $"Clock Speed: {Info.ClockSpeed}",
                                           $"Clock Speed (GHz): {Info.ClockSpeedGHz}",
                                           $"Memory (MB): {Info.Memory}");
            Globals.Roaming.AddFile("baseAddress.txt");
            Globals.Roaming.AddFile("machineId.txt", Environment.MachineName);
            PyroConsoleView.PushTextStatic("Registered:\n" +
                                           "    --baseAddress.txt\n" +
                                           "    --machineId.txt");
            BaseAddress = Globals.Roaming.ReadFileAsText("baseAddress.txt");
            
            PyroConsoleView.PushTextStatic("Read Registered Data:\n" +
                                           $"    --baseAddress.txt -> {BaseAddress},\n" +
                                           $"    --machineId.txt -> {Info.Name}");
        }

        public static async Task PushToLog(this Task<HttpResponseMessage> message)
        {
            var msg = await message;
            PyroConsoleView.PushTextStatic($"HttpResult: {await msg.Content.ReadAsStringAsync()} - {msg.StatusCode.ToString()}!");
        }

        public static async Task SendTimeStatisticAsync()
        { 
            await HttpClient.PostAsync(BaseAddress + $"/time/{Info.Name}", new StringContent(_time.Elapsed.TotalMinutes.Round() + "min", Encoding.Default, "text/plain")).PushToLog();
        }

        public static async Task SendMachineInfo()
        {
            await HttpClient.PostAsync(BaseAddress + $"/info/{Info.Name}", new StringContent(JsonSerializer.Serialize(Info), Encoding.Default, "text/json")).PushToLog();
        }

        public static async Task SendVersionStatisticAsync()
        {
            // await HttpClient.PostAsync(BaseAddress + $"/version/{ID}", 
            //                            new StringContent(JsonSerializer.Serialize(), Encoding.Default, "text/json"));
        }

        public static async Task SendLogStatisticAsync()
        {
            await HttpClient.PostAsync(BaseAddress + $"/logs/{Info.Name}", 
                                       new StringContent(Globals.Roaming.ReadFileAsText("pyroLog.txt"), Encoding.Default, "text/plain"));
        }
        public static void SendStatisticsPeriodic(int ms = 30_000)
        {
            Task.Run(async () =>
            {
                if (!Globals.IsNetworkPresent)
                {
                    await SendMachineInfo();
                }

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
                        //await SendLogStatisticAsync();
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