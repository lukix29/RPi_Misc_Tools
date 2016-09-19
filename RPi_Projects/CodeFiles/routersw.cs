using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace RPi_Projects
{

    class interfaces
    {
        private static string yesRouter
        {
            get
            {
                return @"# Localhost
auto lo
iface lo inet loopback

# WLAN-Interface 0
auto wlan0
allow-hotplug wlan0
iface wlan0 inet static
address 192.168.1.1
netmask 255.255.255.0

#ETHERNET
auto eth0
iface eth0 inet manual

#WLAN1 AUTO
#auto wlan1
#allow-hotplug wlan1
#iface wlan1 inet manual
#       wpa-conf /etc/wpa_supplicant/wpa_supplicant.conf

#BRIDGE
#auto br0
#iface br0 inet static
#bridge_ports eth0 wlan0 wlan1

#NAT und Masquerading aktivieren
up /sbin/iptables -A FORWARD -o eth0 -i wlan0 -m conntrack --ctstate NEW -j ACCEPT
up /sbin/iptables -A FORWARD -m conntrack --ctstate ESTABLISHED,RELATED -j ACCEPT
up /sbin/iptables -t nat -F POSTROUTING
up /sbin/iptables -t nat -A POSTROUTING -o eth0 -j MASQUERADE

#IP-Forwarding aktivieren
up sysctl -w net.ipv4.ip_forward=1
#up sysctl -w net.ipv6.conf.all.forwarding=1

# hostapd und dnsmasq neu starten
up service hostapd restart
up service dnsmasq restart";
            }
        }
        private static string noRouter
        {
            get
            {
                return @"# Localhost
auto lo
iface lo inet loopback

# WLAN-Interface 0
auto wlan0
allow-hotplug wlan0
iface wlan0 inet manual
wpa-conf /etc/wpa_supplicant/wpa_supplicant.conf

# ETHERNET
auto eth0
iface eth0 inet manual";
            }
        }

        const string path = "/etc/network/interfaces";

        private static void Backup()
        {
            File.Copy(path, path + ".old", true);
        }
        private static void Restore()
        {
            if (File.Exists(path + ".old"))
            {
                File.Copy(path + ".old", path, true);
            }
        }
        private static void RestartNetwork()
        {
            Process p = new Process();
            p.StartInfo.FileName = "/etc/init.d/networking";
            p.StartInfo.Arguments = "restart";
            p.StartInfo.UseShellExecute = true;
            //p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            //int line = Console.CursorTop;
            //while (!p.HasExited)
            //{
            //}
        }
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string si = args[0].ToLower().Trim();
                if (si == "on")
                {
                    Backup();
                    File.WriteAllText(path, yesRouter);
                    RestartNetwork();
                }
                else if (si == "off")
                {
                    Backup();
                    File.WriteAllText(path, noRouter);
                    RestartNetwork();
                }
                else if (si == "restore")
                {
                    Restore();
                }
            }
            else
            {
                Console.WriteLine("Usage: routersw on/off/restore");
            }
        }
    }
}
