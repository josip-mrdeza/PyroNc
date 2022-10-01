using System;
using System.Management;
using Pyro.Math;

namespace Pyro.Nc.Configuration
{
    public class SystemInformation
    {
        public SystemInformation()
        {
            
        }

        public SystemInformation(bool init)
        {
            if (init)
            {
                var win32Processor = new ManagementClass("Win32_Processor");
                var objectCollection = win32Processor.GetInstances();
                Name = Environment.UserName;
                Manufacturer = GetCPUManufacturer(objectCollection);
                ClockSpeed = GetCPUCurrentClockSpeed(objectCollection);
                ClockSpeedGHz = (ClockSpeed / 1000f).Round();
                Memory = GetPhysicalMemory();
                ProcessorName = GetComputerName(objectCollection);
            }
        }

        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public int ClockSpeed { get; set; }
        public double ClockSpeedGHz { get; set; }
        public long Memory { get; set; }
        public string CpuInfo { get; set; }
        public string ProcessorName { get; set; }

        public string GetCPUManufacturer(ManagementObjectCollection collection)
        {
            string robotCpu = string.Empty;
            foreach (var o in collection)
            {
                var obj = (ManagementObject) o;
                if (robotCpu == string.Empty)
                {
                    robotCpu = obj.Properties["Manufacturer"].Value.ToString();
                }
            }

            Manufacturer = robotCpu;
            return robotCpu;
        }
        
        public int GetCPUCurrentClockSpeed(ManagementObjectCollection collection)
        {
            int cpuSpeed = 0;
            foreach (var o in collection)
            {
                var obj = (ManagementObject) o;
                if (cpuSpeed == 0)
                {
                    cpuSpeed = Convert.ToInt32(obj.Properties["CurrentClockSpeed"].Value.ToString());
                }
            }

            ClockSpeed = cpuSpeed;
            return cpuSpeed;
        }
        
        public double GetCpuSpeedInGHz(ManagementObjectCollection collection)
        {
            double hz = 0;
            foreach (var o in collection)
            {
                hz = 0.001 * (UInt32) o.Properties["CurrentClockSpeed"].Value;

                break;
            }

            ClockSpeedGHz = hz;
            return hz;
        }

        public long GetPhysicalMemory()
        {
            ManagementScope managementScope = new ManagementScope();
            ObjectQuery query = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher(managementScope, query);
            ManagementObjectCollection objectCollection = objectSearcher.Get();

            long liveMSize = 0;

            foreach (var o in objectCollection)
            {
                var obj = (ManagementObject) o;
                var linecap = Convert.ToInt64(obj["Capacity"]);
                liveMSize += linecap;
            }

            liveMSize = (liveMSize / 1024) / 1024;
            Memory = liveMSize;
            return liveMSize;
        }

        public string GetProcessorInformation(ManagementObjectCollection collection)
        {
            var info = string.Empty;
            foreach (var o in collection)
            {
                var name = (string)o["Name"];
                name = name.Replace("(TM)", "")
                           .Replace("(tm)", "")
                           .Replace("(R)", "")
                           .Replace("(r)", "")
                           .Replace("(C)", "")
                           .Replace("(c)", "")
                           .Replace("    ", " ")
                           .Replace("  ", " ");

                info = name + ", " + (string)o["Caption"] + ", " + (string)o["SocketDesignation"];
            }

            CpuInfo = info;
            return info;
        }
        
        public string GetComputerName(ManagementObjectCollection collection)
        {
            var info = string.Empty;
            foreach (var o in collection)
            {
                info = (string)o["Name"];
                //myObj.Properties["Name"].Value.ToString();
                //break;
            }

            ProcessorName = info;
            return info;
        }
        
        
    }
}