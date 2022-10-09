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
    public class Collector : InitializerRoot
    {
        private static Stopwatch _time;
        public static SystemInformation Info;
        public static readonly HttpClient HttpClient = new HttpClient();
        public static string BaseAddress;

        public override void Initialize()
        {
            _time = Stopwatch.StartNew();
            AddRequiredFiles();
            Task.Run(() =>
            {
                GenerateSoftwareInfo();
                SendStatisticsPeriodic();
                ReadAndPushRegisteredFiles();
                PushSystemInfo();
                string appId = "PyNc";
                Globals.Info = SoftwareInfo.GetFromCache();
                Globals.Info.Refresh(appId);
            });
        }

        private static void GenerateSoftwareInfo()
        {
            var roaming = LocalRoaming.OpenOrCreate("PyroNc\\PyroSoftwareUpdater");
            var fn = "SystemInformation.json";
            if (!roaming.Exists(fn))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = $"{Environment.CurrentDirectory}\\Updater\\PyroSoftwareUpdater.exe";
                psi.WorkingDirectory = Environment.CurrentDirectory;
                psi.Arguments = "generate";
                Process.Start(psi).WaitForExit();
                roaming.Files.Add(fn, new FileInfo(roaming.Site + fn));
            }
            var fn2 = "dev.pyro";
            if (roaming.Exists(fn2))
            {
                Process.Start("{Environment.CurrentDirectory}\\Updater\\PyroSoftwareUpdater.exe", "pack PyNc update.json");
            }
            Info = roaming.ReadFileAs<SystemInformation>(fn);
        }

        private static void ReadAndPushRegisteredFiles()
        {
            PyroConsoleView.PushTextStatic("Registered:\n" +
                                           "    --baseAddress.txt\n" +
                                           "    --machineId.txt");
            BaseAddress = Globals.Roaming.ReadFileAsText("baseAddress.txt");
            PyroConsoleView.PushTextStatic("Read Registered Data:\n" +
                                           $"    --baseAddress.txt -> {BaseAddress},\n" +
                                           $"    --machineId.txt -> {Info.Name}");
        }

        private static void AddRequiredFiles()
        {
            Globals.Roaming.AddFile("baseAddress.txt");
            Globals.Roaming.AddFile("machineId.txt", Environment.MachineName);
        }

        private static void PushSystemInfo()
        {
            PyroConsoleView.PushTextStatic("Read system information:",
                                           $"Name: {Info.Name}",
                                           $"Manufacturer: {Info.Manufacturer}",
                                           $"Cpu Name:{Info.ProcessorName}",
                                           $"Cpu Info: {Info.CpuInfo}",
                                           $"Clock Speed: {Info.ClockSpeed}",
                                           $"Clock Speed (GHz): {Info.ClockSpeedGHz}",
                                           $"Memory (MB): {Info.Memory}");
        }

        public static async Task PushToLog(Task<HttpResponseMessage> message)
        {
            var msg = await message;
            PyroConsoleView.PushTextStatic($"HttpResult from {BaseAddress}: {await msg.Content.ReadAsStringAsync()} - {msg.StatusCode.ToString()}!");
        }

        public static async Task SendTimeStatisticAsync()
        { 
            await PushToLog(HttpClient.PostAsync(BaseAddress + $"/time/{Info.Name}", 
                                                 new StringContent(_time.Elapsed.TotalMinutes.Round() + "min", Encoding.Default, "text/plain")));
        }

        public static async Task SendMachineInfo()
        {
            var msg = HttpClient.PostAsync(BaseAddress + $"/info/{Info.Name}", 
                                                 new StringContent(JsonSerializer.Serialize(Info), Encoding.Default, "text/json"));
            await PushToLog(msg);
        }

        public static async Task SendVersionStatisticAsync()
        {
            // await HttpClient.PostAsync(BaseAddress + $"/version/{ID}", 
            //                            new StringContent(JsonSerializer.Serialize(), Encoding.Default, "text/json"));
        }

        public static async Task SendLogStatisticAsync()
        {
            var msg = HttpClient.PostAsync(BaseAddress + $"/logs/{Info.Name}", 
                                       new StringContent(Globals.Roaming.ReadFileAsText("pyroLog.txt"), Encoding.Default, "text/plain"));
            await msg;
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