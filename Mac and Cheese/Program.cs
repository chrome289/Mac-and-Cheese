using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Mac_and_Cheese
{
    class Program
    {
        public static bool adapter_found = false;
        public static string adapter_name = "",reg_string="",mac="";
        public static int adapter_id = 0;
       
        static void Main(string[] args)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                if ((nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && nic.OperationalStatus == OperationalStatus.Up)
                {
                    adapter_name = nic.Description;
                }
            }
            while (adapter_found == false)
            {
                if ((string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\000" + adapter_id, "DriverDesc", "none") == adapter_name)
                {
                    adapter_found = true;
                    break;
                }
                adapter_id++;
            }
            Console.WriteLine(adapter_id);
            Directory.SetCurrentDirectory(@"C:\Program Files (x86)\Overlook Fing 2.2\bin\");
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", @" /c fing -r 1 -o table,csv,d:\fing-log.csv");
            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
            StreamReader reader = new StreamReader(@"d:\fing-log.csv");
            string address;
            while (reader.Read() != 0)
            {
                string line = reader.ReadLine();
                string[] temp = new string[2];
                string[] split = line.Split(":".ToCharArray(), 8);
                split[0] = split[0].Substring(split[0].Length - 2, 2);
                temp = split[5].Split(";".ToCharArray(), 2);
                split[5] = temp[0];
                temp[1] = temp[1] + "";
                Console.WriteLine(temp[1]);
                if (temp[1] != "Cisco")
                {
                    address = split[0].ToString() + "-" + split[1].ToString() + "-" + split[2].ToString() + "-" + split[3].ToString() + "-" + split[4].ToString() + "-" + split[5].ToString();
                    reg_string = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\000" + adapter_id;
                    mac = (string)Registry.GetValue(reg_string, "NetworkAddress", "none");
                    Registry.SetValue(reg_string, "NetworkAddress", address);
                    mac = (string)Registry.GetValue(reg_string, "NetworkAddress", "none");
                    processStartInfo = new ProcessStartInfo("cmd.exe", @" /c wmic path win32_networkadapter where index=" + adapter_id + " call disable");
                    process = Process.Start(processStartInfo);
                    process.WaitForExit();
                    processStartInfo = new ProcessStartInfo("cmd.exe", @" /c wmic path win32_networkadapter where index=" + adapter_id + " call enable");
                    process = Process.Start(processStartInfo);
                    process.WaitForExit();
                    Console.WriteLine("Trying mac -- " + mac);
                    System.Threading.Thread.Sleep(45000);
                    Ping ping = new Ping();
                    bool p;
                    PingReply pingStatus = ping.Send(@"www.google.com");
                    if (pingStatus.Status == IPStatus.Success)
                    {
                        p = true;
                    }
                    else
                    {
                        p = false;
                    }
                    if (p == true)
                        break;
                }
            }
            Console.WriteLine("Done");
            Console.WriteLine(mac);
            Console.ReadKey();
        }
    }
        
}
