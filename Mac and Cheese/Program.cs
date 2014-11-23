using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Collections.Generic;
namespace Mac_and_Cheese
{
    class Program
    {
        public static bool adapter_found = false, is_connected = false,black=false;
        public static string adapter_name = "",reg_string="",mac="";
        public static int adapter_id = 0;
        public static ProcessStartInfo processStartInfo;
        public static Process process;
        public static List<string> list = new List<string>();

        static void randomize (int n )
        {
            Random r=new Random();
            int i,ran=r.Next(100);
            for (i = n-1; i > 0; i--)
            {
                int j = r.Next(100) % (i+1);
                string temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        static void restart_adapter()
        {
            processStartInfo = new ProcessStartInfo("cmd.exe", @" /c wmic path win32_networkadapter where index=" + adapter_id + " call disable");
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            process = Process.Start(processStartInfo);
            process.WaitForExit();
            processStartInfo = new ProcessStartInfo("cmd.exe", @" /c wmic path win32_networkadapter where index=" + adapter_id + " call enable");
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true; 
            process = Process.Start(processStartInfo);
            process.WaitForExit();   
        }
        static void ping()
        {
            Ping ping = new Ping();
            int temp=0;
            try
            {
                while (temp < 2)
                {
                    PingReply pingStatus = ping.Send(@"www.google.com");
                    if (pingStatus.Status == IPStatus.Success)
                    {
                        is_connected = true;
                        break;
                    }
                    else
                    {
                        is_connected = false;
                        temp++;
                    }
                }
            }
            catch (PingException e)
            {
                Console.WriteLine(DateTime.Now + "  Trying another MAC");
                is_connected = false;
            }
        }
        static void Main(string[] args)
        {
            try
            {
                //find the active adapter
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nic in nics)
                {
                    if ((nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && nic.OperationalStatus == OperationalStatus.Up)
                    {
                        adapter_name = nic.Description;
                    }
                }

                //find adapter id
                while (adapter_found == false)
                {
                    if ((string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\000" + adapter_id, "DriverDesc", "none") == adapter_name)
                    {
                        adapter_found = true;
                        break;
                    }
                    adapter_id++;
                }
                string[] blacklist = { "A0-B3-CC-75-18-B0", "64-27-37-AA-BD-95", "A0-B3-CC-7C-A1-A0", "48-D2-24-30-C3-90", "D4-BE-D9-46-C8-46" };
                while (true)
                {
                    StreamWriter sw = new StreamWriter("d:\\c#\\log.txt", true);
                    sw.AutoFlush = true;
                    Console.SetOut(sw);
                    Console.WriteLine("---------------");
                    //check if already connected
                    if (Process.GetProcessesByName("proxifier").Length < 1)
                    {
                        Process p = Process.Start(@"C:\Program Files (x86)\Proxifier\proxifier.exe");
                    }
                    ping();

                    if (is_connected == true)
                    {
                        Console.WriteLine(DateTime.Now + "  You are already connected");
                        Console.WriteLine(DateTime.Now + "  Going to sleep ...");
                        System.Threading.Thread.Sleep(300000);
                    }
                    else
                    {
                        //kill proxifier
                        if (Process.GetProcessesByName("proxifier").Length > 0)
                        {
                            Process p = Process.GetProcessesByName("proxifier")[0];
                            p.Kill();
                        }

                        //generate mac addresses of connected devices

                        list.Clear();
                        File.Delete(@"d:\c#\fing-log.txt");
                        Directory.SetCurrentDirectory(@"C:\Program Files (x86)\Overlook Fing 2.2\bin\");
                        processStartInfo = new ProcessStartInfo("cmd.exe", @" /c fing --silent -r 1 -o table,csv,d:\c#\fing-log.txt");
                        processStartInfo.UseShellExecute = false;
                        processStartInfo.CreateNoWindow = true; 
                        process = Process.Start(processStartInfo);
                        process.WaitForExit();
                        StreamReader reader = new StreamReader(@"d:\c#\fing-log.txt");
                        string address;
                        while (reader.EndOfStream != true)
                        {
                            //parse mac address;
                            string line = reader.ReadLine();
                            string[] temp = new string[2];
                            string[] split = line.Split(":".ToCharArray(), 8);
                            split[0] = split[0].Substring(split[0].Length - 2, 2);
                            temp = split[5].Split(";".ToCharArray(), 2);
                            split[5] = temp[0];
                            temp[1] = temp[1] + "";
                            address = split[0].ToString() + "-" + split[1].ToString() + "-" + split[2].ToString() + "-" + split[3].ToString() + "-" + split[4].ToString() + "-" + split[5].ToString();

                            //Blacklist
                            black = false;
                            for (int x = 0; x < blacklist.Length; x++)
                            {
                                if (address == blacklist[x])
                                {
                                    black = true;
                                    break;
                                }
                            }

                            //check if device is a client
                            if (temp[1] != "Cisco" && black == false)
                            {
                                list.Add(address);
                            }
                        }
                        reader.Close();

                        //randomize the mac list
                        randomize(list.Count);

                        //check each mac in order
                        while (list.Count != 0)
                        {
                            address = list[0];
                            list.RemoveAt(0);

                            //change mac address
                            reg_string = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\000" + adapter_id;
                            mac = (string)Registry.GetValue(reg_string, "NetworkAddress", "none");
                            Registry.SetValue(reg_string, "NetworkAddress", address);
                            mac = (string)Registry.GetValue(reg_string, "NetworkAddress", "none");

                            restart_adapter();

                            Console.WriteLine(DateTime.Now + "  Trying MAC -- " + mac);

                            System.Threading.Thread.Sleep(45000);

                            ping();

                            if (is_connected == true)
                                break;
                        }
                        reader.Close();
                        Console.WriteLine(DateTime.Now + "  Connected");
                        Console.WriteLine(DateTime.Now + "  Current MAC -- " + mac);
                        if (Process.GetProcessesByName("proxifier").Length < 1)
                        {
                            Process p = Process.Start(@"C:\Program Files (x86)\Proxifier\proxifier.exe");
                        }
                        if (is_connected == false)
                        {
                            Environment.Exit(0);
                            //reset mac address
                            reg_string = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\000" + adapter_id;
                            mac = (string)Registry.GetValue(reg_string, "NetworkAddress", "none");
                            Registry.SetValue(reg_string, "NetworkAddress", "08-2E-5F-74-CA-73");
                        }
                    }
                    sw.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(DateTime.Now + "  Unhandled Exception " + e.InnerException + "    " + e.Message);
            }
        }
    }
        
}
