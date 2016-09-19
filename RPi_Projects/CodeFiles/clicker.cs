using System;
using System.IO;

namespace Lukix29
{
    class clicker
    {
        private static DateTime dt = DateTime.Now;

        private const string path = "/home/pi/lastclick.txt";
        private static void ShowTime()
        {
            DateTime rf = DateTime.Now;
            while (true)
            {
                if (DateTime.Now.Subtract(rf).TotalMilliseconds >= 1000)
                {
                    Console.Clear();
                    Console.WriteLine("Last Click:");
                    Console.WriteLine(DateTime.Now.Subtract(dt).ToString());
                    Console.WriteLine("Press \"ENTER\" for new Click.");
                    rf = DateTime.Now;
                }
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Enter)
                    {
                        AddClick();
                    }
                    else
                    {
                        Console.Clear();
                        return;
                    }
                }
            }
        }
        private static void AddClick()
        {
            dt = DateTime.Now;
            File.WriteAllText(path, dt.Ticks.ToString());
            Console.WriteLine("Clicked (" + dt.ToString() + ")");
        }
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    int cnt = 0;
                    if (args[cnt].StartsWith("-n"))
                    {
                        AddClick();
                        cnt++;
                    }
                    if (cnt >= args.Length) return;
                    if (args[cnt].StartsWith("-s"))
                    {
                        if (File.Exists(path))
                        {
                            long ticks = long.Parse(File.ReadAllText(path));
                            dt = new DateTime(ticks);

                            ShowTime();
                        }
                        cnt++;
                    }
                }
                else
                {
                    Console.WriteLine("Help");
                    Console.WriteLine("Syntax: clicker [Argument 1] [Argument 2]");
                    Console.WriteLine("-n\tNew \"Click\"");
                    Console.WriteLine("-s\tShow Time since \"Click\"");
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
        }
    }
}
