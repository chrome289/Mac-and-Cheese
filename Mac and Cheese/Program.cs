using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mac_and_Cheese
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(@"C:\Program Files (x86)\Overlook Fing 2.2\bin\");
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", @" /c fing -r 1 -o table,csv,d:\fing-log.csv");
            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
            StreamReader reader = new StreamReader(@"d:\fing-log.csv");
            while(reader.Read()!=0)
            {
                string line = reader.ReadLine();
                string[] temp = new string[2];
                string[] split = line.Split(":".ToCharArray(),8);
                split[0] = split[0].Substring(split[0].Length - 2, 2);
                temp = split[5].Split(";".ToCharArray(),2);
                split[5] = temp[0];
                //Console.WriteLine(split[5]);
                Console.ReadKey();
            }
        }
    }
}
