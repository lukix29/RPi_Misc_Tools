using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Drawing;

namespace Lukix29
{
    public class ASCII_ART
    {
        private static Point[] nearFac_List = new Point[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) };

        private static string path = "";
        private static int nearest_factor = 0;
        private static int black_threshold = 40;
        private static string save_to_text = "";
        private static bool delete_file_after = true;
        private static bool is_from_Web = false;
        private static bool verbose = false;
        private static int max_dict_key = 255;
        private static Dictionary<int, char> chars = new Dictionary<int, char>();

        private static bool ParseArgs(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                path = args[0];
                if (path.StartsWith("http"))
                {
                    HelpersAscii.SaveBitmapFromUrl(path);
                    is_from_Web = true;
                    path = Path.GetFileName(path);
                }
                if (args.Length > 1)
                {
                    foreach (string s in args)
                    {
                        if (s.StartsWith("-v"))
                        {
                            verbose = true;
                        }
                        if (s.StartsWith("-s"))
                        {
                            if (s.Length > 2 && s.StartsWith("-s="))
                            {
                                save_to_text = s.Replace("-s=", "");
                            }
                            else
                            {
                                save_to_text = path.Replace(Path.GetExtension(path), ".txt");
                            }
                        }
                        else if (s.StartsWith("-a"))
                        {
                            nearest_factor = int.Parse(s.Replace("-a", ""));
                            nearest_factor = Math.Max(0, Math.Min(nearFac_List.Length - 1, nearest_factor));
                        }
                        else if (s.StartsWith("-t"))
                        {
                            black_threshold = int.Parse(s.Replace("-t", ""));
                        }
                        else if (s.StartsWith("-d"))
                        {
                            if (is_from_Web)
                            {
                                delete_file_after = true;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                Console.WriteLine("Syntax: scroller 'Image_Path' [Options]");
                Console.WriteLine("\t-a0 = (0-3)Nearest Neighbaor Factor");
                Console.WriteLine("\t-t50 = Black Threshold");
                Console.WriteLine("\t-d Delete file after finishing (only if File is from Web)");
                Console.WriteLine("\t-s Save to TextFile(Name is \"FileName\".txt");
                return false;
            }
        }
        private static void CreateHueDictionary()
        {
            if (!File.Exists("chars.txt"))
            {
                Bitmap b = new Bitmap(100, 100);
                Font font = new Font(FontFamily.GenericMonospace, 24f, FontStyle.Bold);
                Graphics g = Graphics.FromImage(b);
                StreamWriter sw = new StreamWriter(File.Create("chars.txt"));
                for (char c = ' '; c < 255; c++)
                {
                    if (char.IsControl(c)) continue;
                    g = Graphics.FromImage(b);
                    SizeF sf = g.MeasureString(c.ToString(), font);
                    b = new Bitmap((int)sf.Width, (int)sf.Height);
                    g.Dispose();
                    g = Graphics.FromImage(b);
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    g.Clear(Color.Black);
                    g.DrawString(c.ToString(), font, Brushes.White, 0, 0);
                    g.Dispose();

                    float f = HelpersAscii.CalculateAverageBrightness(b);
                    int val = (int)(f * 1000);
                    if (!chars.ContainsKey(val))
                    {
                        chars.Add(val, c);
                        sw.WriteLine(c.ToString() + val.ToString());
                    }
                }
                sw.Close();
            }
            else
            {
                string[] sa = File.ReadAllLines("chars.txt");
                foreach (string s in sa)
                {
                    chars.Add(int.Parse(s.Substring(1)), s[0]);
                }
            }
            max_dict_key = chars.Keys.Max();
        }

        private static char GetChar(int hue)
        {
            if (!chars.ContainsKey(hue))
            {
                int h1 = hue;
                if (h1 < max_dict_key)
                {
                    while (true)
                    {
                        h1++;
                        if (chars.ContainsKey(h1))
                        {
                            break;
                        }
                    }
                }
                else h1 = max_dict_key;
                hue = h1;
            }
            if (verbose) Console.Write(chars[hue]);
            return chars[hue];
        }
        private static int GetBrightness(Bitmap b, int xi, int yi)
        {
            int hue = 0;
            for (int i = 0; i <= nearest_factor; i++)
            {
                Color c = b.GetPixel(xi + nearFac_List[i].X, yi + nearFac_List[i].Y);
                if ((c.R + c.G + c.B) / 3 >= black_threshold)
                {
                    hue += (int)(c.GetBrightness() * 1000);
                }
            }
            return hue / (nearest_factor + 1);
        }
        private static string Create_ASCII_Art()
        {
            Bitmap b = new Bitmap(new Bitmap(path), HelpersAscii.Bounds.Width * 2, HelpersAscii.Bounds.Height * 2);

            string output = "";

            if (verbose) Console.SetCursorPosition(0, 0);
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    int hue = GetBrightness(b, x, y);

                    if (verbose)
                    {
                        Point p = HelpersAscii.ClampPosition(x / 2, y / 2);
                        Console.SetCursorPosition(p.X, p.Y);
                    }
                    char c = GetChar(hue);
                    output += c;
                }
                output += "\r\n";
            }
            return output;
        }

        public static void Main(string[] args)
        {
            if (!ParseArgs(args))
            {
                return;
            }

            Console.Clear();

            CreateHueDictionary();

            string art = Create_ASCII_Art();

            if (save_to_text.Length > 0)
            {
                File.WriteAllText(save_to_text, art);
                Console.WriteLine("Saved to \"" + Path.GetFullPath(save_to_text) + "\"");
            }

            if (delete_file_after)
            {
                string[] files = Directory.GetFiles("./",
                    Path.GetFileNameWithoutExtension(path) +
                    "*" + Path.GetExtension(path) + "*");
                foreach (string s in files)
                {
                    File.Delete(s);
                }
            }
        }
    }

    public static class HelpersAscii
    {
        public static string SaveBitmapFromUrl(string Url)
        {
            if (HelpersAscii.IsLinux)
            {
                Process p = new Process();
                p.StartInfo.FileName = "wget";
                p.StartInfo.Arguments = Url;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                Console.WriteLine("Downloading Image!");
                while (!p.HasExited) ;
                Console.WriteLine("Image complete!");
            }
            else
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(Url, Path.GetFileName(Url));
                wc.Dispose();
            }
            return Path.GetFullPath(Path.GetFileName(Url));
        }
        public static string GetStringFromUrl(string Url)
        {
            if (HelpersAscii.IsLinux)
            {
                Process p = new Process();
                p.StartInfo.FileName = "wget";
                p.StartInfo.Arguments = Url;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                while (!p.HasExited) ;
                return p.StandardOutput.ReadToEnd();
            }
            else
            {
                WebClient wc = new WebClient();
                string s = wc.DownloadString(Url);
                wc.Dispose();
                return s;
            }
        }
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public static Rectangle Bounds
        {
            get { return new Rectangle(0, 0, Console.BufferWidth, Console.BufferHeight); }
        }
        public static Point ClampPosition(int x, int y)
        {
            return new Point(
                Math.Max(0, Math.Min(Bounds.Width - 1, x)),
                Math.Max(0, Math.Min(Bounds.Height - 1, y)));
        }
        public static float CalculateAverageBrightness(Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int red = 0;
            int green = 0;
            int blue = 0;
            float totalWhite = 0;
            int cnt = 0;
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            System.Drawing.Imaging.BitmapData srcData = bm.LockBits(
                new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bm.PixelFormat);
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];
                        Color c = Color.FromArgb(red, green, blue);
                        totalWhite += c.GetBrightness();
                        cnt++;
                    }
                }
            }

            return totalWhite / cnt;
        }
        public static int Map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
