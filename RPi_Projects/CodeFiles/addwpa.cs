using System;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Lukix29
{
    class AddWPA
    {
        public const string confFile = "/etc/wpa_supplicant/wpa_supplicant.conf";
        static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                string ssid = args[0];
                string pw = args[1];
                bool checkWIFI = false;
                foreach (string si in args)
                {
                    if (si.StartsWith("id=") || si.StartsWith("ssid="))
                    {
                        ssid = si.Replace("id=", "").Replace("ssid=", "");
                    }
                    else if (si.StartsWith("pw="))
                    {
                        pw = si.Replace("pw=", "");
                    }
                    else if (si.StartsWith("check"))
                    {
                        checkWIFI = true;
                    }
                }

                if (checkWIFI)
                {
                    Console.WriteLine("Checking available Wifi Networks...");
                    Process p = new Process();
                    p.StartInfo.FileName = "iwlist";
                    p.StartInfo.Arguments = "wlan0 scan";
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    while (!p.HasExited) ;
                    string wlans = p.StandardOutput.ReadToEnd();
                    if (!wlans.Contains(ssid))
                    {
                        Console.WriteLine("No matching WiFi found!");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Matching WiFi found!");
                    }
                }
                string s = File.ReadAllText(confFile);
                if (!(s.Contains(ssid) && s.Contains(pw)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("network={");
                    sb.AppendLine("\tssid=\"" + ssid + "\"");
                    sb.AppendLine("\tpsk=\"" + pw + "\"");
                    sb.AppendLine("\tkey_mgmt=WPA-PSK");
                    sb.AppendLine("}");
                    Console.WriteLine(sb.ToString());
                    Console.WriteLine("Click \"Enter\" to add this Network.");
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        File.AppendAllText(confFile, sb.ToString());
                        
                        Console.WriteLine("Added Network to List!");
                        Console.WriteLine("Restarting WLAN");
                        Process p = new Process();
                        p.StartInfo.FileName = "sudo";
                        p.StartInfo.Arguments = "ifdown wlan0";
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        p.WaitForExit();
                        p = new Process();
                        p.StartInfo.FileName = "sudo";
                        p.StartInfo.Arguments = "ifup wlan0";
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        p.WaitForExit();
                        Console.WriteLine("Finished!");
                    }
                }
                else
                {
                    Console.WriteLine("Network is already in the List!");
                }
            }
            else
            {
                Console.WriteLine("Usage: addwpa id=[SSID] pw=[WPA-Password]");
                Console.WriteLine("or   : addwpa check id=[SSID] pw=[WPA-Password]");
                Console.WriteLine();
            }
        }
    }
}