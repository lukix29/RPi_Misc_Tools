using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lukix29
{
    class screensaver
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public static int Width
        {
            get { return Console.BufferWidth; }
        }
        public static int Height
        {
            get { return Console.BufferHeight; }
        }
        public static void Main(string[] args)
        {
            char[] chars = new char[] { '.', ',', ':', '+', '!' };
            Console.Clear();
            string s = new string('_', Width);
            Random rd = new Random();
            while (true)
            {
                for (int y = 0; y < Height; y++)
                {
                    //Console.SetCursorPosition(0, y - 1);
                    //Console.Write(se);
                    if (y < Height - 1)
                    {
                        Console.SetCursorPosition(0, y + 1);
                        Console.Write(s);
                    }
                    for (int x = 0; x < Width; x++)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(chars[rd.Next(0, chars.Length)]);
                    }
                    if (Console.KeyAvailable)
                    {
                        Console.Clear();
                        return;
                    }
                    System.Threading.Thread.Sleep(30);
                }
                //Console.SetCursorPosition(0, Height - 1);
                //Console.Write(se);
                System.Threading.Thread.Sleep(1000);
            }
        }
        public static void Empty() { }
    }
}
