using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;
using System.Media;

namespace Lukix29
{
    public static class alarmclock
    {
        private static char ReplaceForCom(string input, out string command)
        {
            if (input.StartsWith("-"))
            {
                input = input.Remove(0, 1);
            }
            command = input.Substring(input.IndexOf('=') + 1).Trim().ToLower();
            return input[0];
        }
        private static void ExecuteCommand(string com)
        {
            DateTime dt = new DateTime(0);
            if (DateTime.TryParse(com, out dt))
            {
                alarmTime = dt;
                return;
            }
            string[] args = com.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < args.Length; i++)
            {
                string command = "";
                char type = ReplaceForCom(args[i], out command);
                switch (type)
                {
                    case 's':
                        int.TryParse(command, out  snoozeTime);
                        break;
                    case 'r':
                        int.TryParse(command, out ringTime);
                        break;
                    case 'a':
                        if (command == "true" || command == "on")
                        {
                            isAlarmOn = true;
                        }
                        else
                        {
                            isAlarmOn = false;
                        }
                        break;
                    case 't':
                        DateTime.TryParse(command, out alarmTime);
                        break;
                    case 'w':
                        int.TryParse(command, out wfac);
                        break;
                    case 'h':
                        int.TryParse(command, out hfac);
                        break;
                    case 'x':
                        int.TryParse(command, out xOff);
                        break;
                    case 'y':
                        int.TryParse(command, out yOff);
                        break;
                    case 'c':
                        File.Delete("clock.txt");
                        hfac = 1;
                        wfac = 1;
                        xOff = 0;
                        yOff = 0;
                        isAlarmOn = false;
                        break;
                }
            }
            SetSize();
        }
        private static void RingAlarm()
        {
            if (DateTime.Now.Ticks >= alarmTime.Ticks && DateTime.Now.Subtract(alarmTime).TotalMinutes <= ringTime)
            {
                if (isAlarmOn && !stopRing)
                {
                    SoundPlayer sp = null;
                    if (IsLinux)
                    {
                        sp = new SoundPlayer("alarm.wav");
                        sp.PlayLooping();
                    }
                    //FileStream fs = File.OpenRead("/dev/input/mouse0");
                    string ts = "!!ALARM!!";
                    for (int t = 0; t < ts.Length; t++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int xi = ((t * (width + 1)) + x) + xOff;
                                int yi = (y + yOff);
                                if (xi < Console.BufferWidth && yi < Console.BufferHeight)
                                {
                                    char c = chars[ts[t]][(int)(y / hfac)][(int)(x / wfac)];
                                    Console.SetCursorPosition(xi, yi);
                                    Console.Write(c);
                                }
                            }
                        }
                    }
                    while (!Console.KeyAvailable)
                    {
                        //if (fs.ReadByte() >= 0)
                        //{
                        //    break;
                        //}
                        if ((int)DateTime.Now.Subtract(alarmTime).TotalMinutes >= ringTime)
                        {
                            break;
                        }
                    }
                    alarmTime = new DateTime(alarmTime.Ticks + new TimeSpan(1, 0, 0, 0).Ticks);
                    SaveSettings();
                    stopRing = true;
                    if (IsLinux)
                    {
                        sp.Stop();
                    }
                }
            }
        }
        private static void LoadSettings()
        {
            if (File.Exists("clock.txt"))
            {
                string[] sa = File.ReadAllLines("clock.txt");
                if (sa.Length >= 5)
                {
                    wfac = int.Parse(sa[0]);
                    hfac = int.Parse(sa[1]);
                    xOff = int.Parse(sa[2]);
                    yOff = int.Parse(sa[3]);
                    alarmTime = new DateTime(long.Parse(sa[4]));
                    isAlarmOn = bool.Parse(sa[5]);
                }
            }
            SetSize();
        }
        private static void SaveSettings()
        {
            StringBuilder sw = new StringBuilder();
            sw.AppendLine(wfac.ToString());
            sw.AppendLine(hfac.ToString());
            sw.AppendLine(xOff.ToString());
            sw.AppendLine(yOff.ToString());
            sw.AppendLine(alarmTime.Ticks.ToString());
            sw.AppendLine(isAlarmOn.ToString());
            File.WriteAllText("clock.txt", sw.ToString());
        }
        private static void SetSize()
        {
            width = (int)(chars.Width * wfac);
            height = (int)(chars.Height * hfac);
            clear = new string(' ', 8 * width);
            SaveSettings();
            Console.Clear();
        }
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static BigChars chars = new BigChars();
        private static DateTime alarmTime;
        private static bool stopRing = false;
        private static int ringTime = 1;
        private static int snoozeTime = 1;
        private static int hfac = 1;
        private static int wfac = 1;
        private static int xOff = 0;
        private static int yOff = 0;
        private static int width = 0;
        private static int height = 0;
        private static string clear = "";
        private static string sinput = "";
        private static bool isAlarmOn = false;

        public static void Main(string[] args)
        {
            if(args.Length>0)
            {
                foreach(string s in args)
                {
                    if(s.Replace("-","").StartsWith("r"))
                    {
                        File.Delete("clock.txt");
                    }
                }
            }
            alarmTime = new DateTime(0);
            LoadSettings();
            DateTime dt = new DateTime(0);
            Console.CursorVisible = false;
            bool run = true;
            bool update = true;
            int blink = 0;
            bool blool = false;
            int timeRefresh = 0;
            string time = DateTime.Now.ToLongTimeString();
            Console.Clear();
            while (run)
            {
                if (Console.KeyAvailable)
                {
                    update = true;
                    ConsoleKeyInfo ck = Console.ReadKey(true);
                    switch (ck.Key)
                    {
                        case ConsoleKey.Escape:
                            run = false;
                            break;
                        case ConsoleKey.Enter:
                            ExecuteCommand(sinput);
                            sinput = "";
                            break;
                        case ConsoleKey.Backspace:
                            if (sinput.Length > 0)
                            {
                                sinput = sinput.Remove(sinput.Length - 1);
                            }
                            break;
                        default:
                            if (sinput.Length < clear.Length)
                            {
                                sinput += ck.KeyChar;
                            }
                            break;
                    }
                }
                if (DateTime.Now.Subtract(dt).TotalMilliseconds >= 1000) update = true;
                if (DateTime.Now.Subtract(dt).TotalMilliseconds >= 100 || update)
                {
                    if (timeRefresh++ > 10 || update)
                    {
                        time = DateTime.Now.ToLongTimeString();
                        for (int t = 0; t < time.Length; t++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    int xi = ((t * (width + 1)) + x) + xOff;
                                    int yi = (y + yOff);
                                    if (xi < Console.BufferWidth && yi < Console.BufferHeight)
                                    {
                                        char c = chars[time[t]][(int)(y / hfac)][(int)(x / wfac)];
                                        Console.SetCursorPosition(xi, yi);
                                        Console.Write(c);
                                    }
                                }
                            }
                        }
                        timeRefresh = 0;
                    }
                    if (blink++ > 5 || update)
                    {
                        if (isAlarmOn)
                        {
                            Console.SetCursorPosition(xOff, height + yOff);
                            Console.WriteLine("Next Alarm: " + alarmTime);
                        }
                        Console.SetCursorPosition(xOff, height + yOff + 1);
                        Console.Write(clear);
                        Console.SetCursorPosition(xOff, height + yOff + 1);
                        Console.Write(sinput);
                        if (blool)
                        {
                            Console.Write("_");
                            blool = false;
                        }
                        else
                        {
                            Console.Write(" ");
                            blool = true;
                        }
                        blink = 0;
                    }

                    RingAlarm();

                    dt = DateTime.Now;
                    update = false;
                }
            }
            Console.CursorVisible = true;
            Console.Clear();
        }
    }

    public class BigChars
    {
        private static string CharBlock
        {
            get
            {
                return @"
  ##  
  ##  
  ##  
  ##  
      
  ##  
  ##  
-!
      
  ##  
  ##  
      
  ##  
  ##  
      
-:
 #### 
#    #
#    #
#    #
#    #
#    #
 #### 
-0
   ## 
  # # 
 #  # 
#   # 
    # 
    # 
    # 
-1
 #### 
#    #
    # 
   #  
  #   
 #    
###### 
-2
 #### 
#    #
     #
 #### 
     #
#    #
 #### 
-3
    ##
   # #
  #  #
 #   #
######
     #
     #
-4
######
#     
#     
##### 
     #
     #
##### 
-5
 #####
#     
#     
##### 
#    #
#    #
 #### 
-6
######
     #
    # 
   #  
  #   
 #    
#     
-7
 #### 
#    #
#    #
 #### 
#    #
#    #
 #### 
-8
 #### 
#    #
#    #
 #####
     #
     #
##### 
-9
  ##  
 #  # 
#    #
#    #
######
#    #
#    #
-A
####  
#   # 
#   # 
####  
#   # 
#   # 
####  
-B
 #### 
#     
#     
#     
#     
#     
 #### 
-C
####  
#   # 
#   # 
#   # 
#   # 
#   # 
####  
-D
##### 
#     
#     
##### 
#     
#     
##### 
-E
##### 
#     
#     
##### 
#     
#     
#     
-F
 ###  
#   # 
#     
#     
#  ## 
#   # 
 ###  
-G
#   # 
#   # 
#   # 
##### 
#   # 
#   # 
#   # 
-H
 #### 
  ##  
  ##  
  ##  
  ##  
  ##  
 #### 
-I
   ## 
   ## 
   ## 
   ## 
   ## 
#  ## 
 ###  
-J
#   # 
#  #  
# #   
##    
# #   
#  #  
#   # 
-K
#     
#     
#     
#     
#     
#     
##### 
-L
##  ##
# ## #
# ## #
#    #
#    #
#    #
#    #
-M
##   #
##   #
# #  #
# ## #
#  # #
#   ##
#   ##
-N
 #### 
#    #
#    #
#    #
#    #
#    #
 #### 
-O
####  
#   # 
#   # 
####  
#     
#     
#     
-P
 #### 
#    #
#    #
#    #
#  # #
#   # 
 ### #
-Q
####  
#   # 
#   # 
####  
# #   
#  #  
#   # 
-R
 ###  
#   # 
 #    
  #   
   #  
#   # 
 ###  
-S
######
  ##  
  ##  
  ##  
  ##  
  ##  
  ##  
-T
#    #
#    #
#    #
#    #
#    #
#    #
 #### 
-U
#    #
#    #
#    #
 #  # 
 #  # 
  ##  
  ##  
-V
#    #
#    #
#    #
#    #
# ## #
# ## #
##  ##
-W
#    #
#    #
 #  # 
  ##  
 #  # 
#    #
#    #
-X
#   # 
#   # 
#   # 
 # #  
  #   
  #   
  #   
-Y
##### 
    # 
   #  
  #   
 #    
#     
##### 
-Z
      
      
  ##  
 #### 
  ##  
      
      
-+
      
      
      
 #### 
      
      
      
--
      
      
 #### 
      
 #### 
      
      
-=
      
      
      
      
      
##    
##    
-.
      
      
      
      
  ##  
  ##  
 ##   
-,
      
      
  ##  
      
  ##  
  ##  
 ##   
-;
 ## ##
 ## ##
      
      
      
      
      
-""
   ## 
   ## 
  ##  
      
      
      
      
-'
  #   
 #    
#     
#     
#     
 #    
  #   
-(
   #  
    # 
     #
     #
     #
    # 
   #  
-)
    # 
   #  
   #  
  #   
  #   
 #    
 #    
-/
 #    
  #   
  #   
   #  
   #  
    # 
    # 
-\
";
            }
        }
        private Dictionary<char, string[]> chars = new Dictionary<char, string[]>();
        public string[] this[char c]
        {
            get { return chars[char.ToUpper(c)]; }
        }

        public int Count
        {
            get
            {
                return chars.Count;
            }
        }
        public int Length
        {
            get
            {
                return Width * Height;
            }
        }
        public int Width
        {
            get
            {
                return chars['0'][0].Length;
            }
        }
        public int Height
        {
            get
            {
                return chars['0'].Length;
            }
        }

        public BigChars()
        {
            try
            {
                StringReader sr = new StringReader(CharBlock);
                int cnt = 0;
                List<string> list = new List<string>();
                while (true)
                {
                    string s = sr.ReadLine();
                    if (s != null && s.Length > 0)
                    {
                        if (s.Contains("-"))
                        {
                            char c = s[1];// ctemp[cnt];
                            chars.Add(c, list.ToArray());
                            list.Clear();
                        }
                        else
                        {
                            list.Add(s.Replace("#", "█"));
                        }
                    }
                    else
                    {
                        cnt++;
                        if (cnt >= 100) break;
                    }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
        }
    }
}

//namespace LX29_Drawing
//{
//    public class DrawingX
//    {
//        private ImageX buffer;
//        public DrawingX(ImageX Buffer)
//        {
//            buffer = Buffer;
//        }
//        public void DrawRectangle(int x, int y, int w, int h, char c)
//        {
//            if (x > buffer.Size.Width || y > buffer.Size.Height) return;
//            int r = MathX.Clamp(x + w, 0, buffer.Size.Width);
//            int b = MathX.Clamp(y + h, 0, buffer.Size.Height);
//            for (int xi = x; xi < r; xi++)
//            {
//                buffer.SetPixel(xi, y, c);
//                buffer.SetPixel(xi, y + h, c);
//            }
//            for (int yi = y; yi < b; yi++)
//            {
//                buffer.SetPixel(x + w, yi, c);
//                buffer.SetPixel(x, yi, c);
//            }
//        }
//        public void DrawRectangle(float x, float y, float w, float h, char c)
//        {
//            DrawRectangle((int)x, (int)y, (int)w, (int)h, c);
//        }
//        public void DrawRectangle(Rectangle rec, char c)
//        {
//            DrawRectangle(rec.X, rec.Y, rec.Width, rec.Height, c);
//        }
//        public void DrawRectangle(RectangleF rec, char c)
//        {
//            DrawRectangle(rec.X, rec.Y, rec.Width, rec.Height, c);
//        }
//        public void DrawRectangleLTRB(int x, int y, int r, int b, char c)
//        {
//            DrawRectangle(Rectangle.FromLTRB(x, y, r, b), c);
//        }
//        public void FillRectangle(int x, int y, int w, int h, char c)
//        {
//            if (x > buffer.Size.Width || y > buffer.Size.Height) return;
//            int r = MathX.Clamp(x + w, 0, buffer.Size.Width);
//            int b = MathX.Clamp(y + h, 0, buffer.Size.Height);
//            for (int xi = x; xi < r; xi++)
//            {
//                for (int yi = y; yi < b; yi++)
//                {
//                    buffer.SetPixel(xi, yi, c);
//                }
//            }
//        }
//        public void FillRectangle(float x, float y, float w, float h, char c)
//        {
//            FillRectangle((int)x, (int)y, (int)w, (int)h, c);
//        }
//        public void DrawXLine(int x0, int y, int x1, char c)
//        {
//            if (y < 0 || y > buffer.Size.Height) return;
//            x0 = MathX.Clamp(x0, 0, buffer.Size.Width);
//            x1 = MathX.Clamp(x1, 0, buffer.Size.Width);
//            for (int xi = x0; xi <= x1; xi++)
//            {
//                buffer.SetPixel(xi, y, c);
//            }
//        }
//        public void DrawYLine(int x, int y0, int y1, char c)
//        {
//            if (x < 0 || x > buffer.Size.Width) return;
//            y0 = MathX.Clamp(y0, 0, buffer.Size.Height);
//            y1 = MathX.Clamp(y1, 0, buffer.Size.Height);
//            for (int yi = y0; yi <= y1; yi++)
//            {
//                buffer.SetPixel(x, yi, c);
//            }
//        }
//        public void DrawLine(int x0, int y0, int x1, int y1, char color)
//        {
//            bool steep = (Math.Abs(y1 - y0) > Math.Abs(x1 - x0));
//            if (steep)
//            {
//                MathX.Swap(ref x0, ref y0);
//                MathX.Swap(ref x1, ref y1);
//            }

//            if (x0 > x1)
//            {
//                MathX.Swap(ref x0, ref x1);
//                MathX.Swap(ref y0, ref y1);
//            }

//            int dx, dy;
//            dx = x1 - x0;
//            dy = Math.Abs(y1 - y0);

//            int err = dx / 2;
//            int ystep;

//            if (y0 < y1)
//            {
//                ystep = 1;
//            }
//            else
//            {
//                ystep = -1;
//            }

//            for (; x0 <= x1; x0++)
//            {
//                if (steep)
//                {
//                    buffer.SetPixel(y0, x0, color);
//                }
//                else
//                {
//                    buffer.SetPixel(x0, y0, color);
//                }
//                err -= dy;
//                if (err < 0)
//                {
//                    y0 += ystep;
//                    err += dx;
//                }
//            }
//        }
//        public void DrawCross(Point center, int radius, int width, char c)
//        {
//            int r2 = radius / 2;
//            for (int i = 0; i < width; i++)
//            {
//                DrawXLine(center.X - r2, center.Y + i, center.X + r2 + 1, c);
//                DrawYLine(center.X + i, center.Y - r2, center.Y + r2 + 1, c);
//            }
//        }
//        public void FillRectangle(Rectangle rec, char c)
//        {
//            FillRectangle(rec.X, rec.Y, rec.Width, rec.Height, c);
//        }
//        public void FillRectangle(RectangleF rec, char c)
//        {
//            FillRectangle((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height, c);
//        }
//        public void Clear(char c)
//        {
//            FillRectangle(0, 0, buffer.Width, buffer.Height, c);
//        }

//        //public static double CompareBitmaps(Bitmap Bitmap1, Bitmap Bitmap2, int chunkSize)
//        //{
//        //    byte[] ba1 = RayMath.GetBGRA(Bitmap1);
//        //    Bitmap bi = Bitmap2;
//        //    if (Bitmap1.WorldWidth != Bitmap2.WorldWidth || Bitmap1.WorldHeight != Bitmap2.WorldHeight)
//        //    {
//        //        bi = new Bitmap(bi, Bitmap1.Size);
//        //    }
//        //    byte[] ba2 = RayMath.GetBGRA(bi);
//        //    double rd1 = 0.0;
//        //    double gd1 = 0.0;
//        //    double bd1 = 0.0;
//        //    double rd2 = 0.0;
//        //    double gd2 = 0.0;
//        //    double bd2 = 0.0;

//        //    int dw = chunkSize;// 0;
//        //    int cntall = 0;
//        //    for (int x = 0; x < Bitmap1.WorldWidth - dw; x += dw)
//        //    {
//        //        for (int adv = 0; adv < Bitmap1.WorldHeight - dw; adv += dw)
//        //        {
//        //            for (int x = x; x < x + 10; x++)
//        //            {
//        //                for (int y = adv; y < adv + 10; y++)
//        //                {
//        //                    ColorX c1 = GetPixel(x, y, ba1, Bitmap1.WorldWidth, Bitmap1.PixelFormat);
//        //                    ColorX c2 = GetPixel(x, y, ba2, bi.WorldWidth, bi.PixelFormat);
//        //                    rd1 += (c1.R / 255.0);
//        //                    gd1 += (c1.G / 255.0);
//        //                    bd1 += (c1.B / 255.0);
//        //                    rd2 += (c2.R / 255.0);
//        //                    gd2 += (c2.G / 255.0);
//        //                    bd2 += (c2.B / 255.0);
//        //                    cntall++;
//        //                }
//        //            }
//        //        }
//        //    }
//        //    rd1 /= (double)cntall;
//        //    gd1 /= (double)cntall;
//        //    bd1 /= (double)cntall;
//        //    rd2 /= (double)cntall;
//        //    gd2 /= (double)cntall;
//        //    bd2 /= (double)cntall;

//        //    double d = (DivH(rd1, rd2) + DivH(gd1, gd2) + DivH(bd1, bd2)) / 3.0;
//        //    return d;
//        //}
//        public static double DivH(double d, double d1)
//        {
//            return Math.Max(d, d1) / Math.Min(d, d1);
//        }


//        public void DrawImage(ImageX image, int x, int y)
//        {
//            DrawImage(image, x, y, image.Width, image.Height);
//        }
//        public void DrawImage(ImageX image, int x, int y, int w, int h)
//        {
//            try
//            {
//                if (x > buffer.Size.Width || y > buffer.Size.Height) return;

//                float wf = (float)image.Width / (float)w;
//                float hf = (float)image.Height / (float)h;

//                for (int xi = 0; xi < image.Width; xi += (int)wf)
//                {
//                    for (int yi = 0; yi < image.Height; yi += (int)hf)
//                    {
//                        char c = image.GetPixel(xi, yi);
//                        buffer.SetPixel((int)(x + (xi / wf)), (int)(y + (yi / hf)), c);
//                    }
//                }
//            }
//            catch (Exception xe)
//            {
//                string s = xe.ToString();
//            }
//        }

//        public void DrawImage(ImageX image, int x, int y, int w, int h, char color)
//        {
//            if (x > buffer.Size.Width || y > buffer.Size.Height) return;

//            float wf = image.Width / w;
//            float hf = image.Height / h;

//            for (int xi = 0; xi < image.Width; xi += (int)wf)
//            {
//                for (int yi = 0; yi < image.Height; yi += (int)hf)
//                {
//                    buffer.SetPixel((int)(x + (xi / wf)), (int)(y + (yi / hf)), color);
//                }
//            }
//        }

//        public void DrawString(string s, FontX font, char c, int x, int y, float fontSize)
//        {
//            //s = s.Replace("ö", "o");
//            //s = s.Replace("ä", "a");
//            //s = s.Replace("ü", "u");
//            string si = "";
//            int lines = 1;
//            for (int i = 0; s.Length > i; i++)
//            {
//                if (!char.IsControl(s[i]))
//                {
//                    si += s[i];
//                }
//                else if (s[i] == '\n')
//                {
//                    si += s[i];
//                    lines++;
//                }
//            }
//            int fontWidth = (int)(fontSize * font.Width);
//            int fontHeight = (int)(fontSize * font.Height);
//            int xw = x;
//            for (int i = 0; si.Length > i; i++)
//            {
//                if (si[i] == '\n')
//                {
//                    y += fontHeight + 10;
//                    xw = x;
//                }
//                else
//                {
//                    font.DrawChar(s[i], buffer, c, xw, y, fontWidth, fontHeight);
//                    xw += (int)(font.Width * fontSize);
//                }
//            }
//        }
//        public Rectangle DrawString(string s, FontX font, float fontSize, char c, Rectangle bounds, StringAlignment Alignment)
//        {
//            Size sf = MeasureString(s, font, fontSize);
//            Rectangle rec = getLoc(Alignment, bounds, sf);
//            DrawString(s, font, c, rec.X, rec.Y, fontSize);
//            return rec;
//        }
//        public static Rectangle MeasureString(string s, FontX font, float fontSize, Rectangle bounds, StringAlignment Alignment)
//        {
//            Size sf = MeasureString(s, font, fontSize);
//            Rectangle rec = getLoc(Alignment, bounds, sf);
//            return rec;
//        }
//        public static Rectangle MeasureString(int charCount, int lineCnt, FontX font, float fontSize, Rectangle bounds, StringAlignment Alignment)
//        {
//            Size sf = MeasureString(charCount, lineCnt, font, fontSize);
//            Rectangle rec = getLoc(Alignment, bounds, sf);
//            return rec;
//        }
//        public enum StringAlignment
//        {
//            TopLeft,
//            TopRight,
//            BotLeft,
//            BotRight,
//            Centered,
//            CentLeft,
//            CentRight,
//            CentTop,
//            CentBot
//        }
//        public static Rectangle getLoc(StringAlignment pos, Rectangle bounds, Size textSize)
//        {
//            switch (pos)
//            {
//                case StringAlignment.TopLeft:
//                    return new Rectangle(bounds.X, bounds.Y, textSize.Width, textSize.Height);
//                case StringAlignment.TopRight:
//                    return new Rectangle(bounds.Right - textSize.Width, bounds.Y, textSize.Width, textSize.Height);

//                case StringAlignment.BotLeft:
//                    return new Rectangle(bounds.X, bounds.Bottom - textSize.Height, textSize.Width, textSize.Height);
//                case StringAlignment.BotRight:
//                    return new Rectangle(bounds.Right - textSize.Width, bounds.Bottom - textSize.Height,
//                        textSize.Width, textSize.Height);

//                case StringAlignment.Centered:
//                    return new Rectangle(bounds.X + ((bounds.Width / 2)) - (textSize.Width / 2),
//                        bounds.Y + ((bounds.Height / 2)) - (textSize.Height / 2),
//                        textSize.Width, textSize.Height);

//                case StringAlignment.CentLeft:
//                    return new Rectangle(bounds.X,
//                        bounds.Y + ((bounds.Height / 2)) - (textSize.Height / 2),
//                        textSize.Width, textSize.Height);
//                case StringAlignment.CentRight:
//                    return new Rectangle(bounds.Right - textSize.Width,
//                        bounds.Y + ((bounds.Height / 2)) - (textSize.Height / 2),
//                        textSize.Width, textSize.Height);

//                case StringAlignment.CentTop:
//                    return new Rectangle(bounds.X + ((bounds.Width / 2)) - (textSize.Width / 2),
//                        bounds.Y,
//                        textSize.Width, textSize.Height);
//                case StringAlignment.CentBot:
//                    return new Rectangle(bounds.X + ((bounds.Width / 2)) - (textSize.Width / 2),
//                        bounds.Bottom - textSize.Height,
//                        textSize.Width, textSize.Height);
//            }
//            return Rectangle.Empty;
//        }
//        public static Size MeasureString(string s, FontX font, float fontSize)
//        {
//            //string[] sa = s.Split('\n');
//            int lines = 1;
//            int max = 1;
//            int cnt = 0;
//            for (int i = 0; s.Length > i; i++)
//            {
//                if (!char.IsControl(s[i]))
//                {
//                    cnt++;
//                }
//                else
//                {
//                    if (cnt > max) max = cnt;
//                    cnt = 0;

//                    if (s[i] == '\n')
//                    {
//                        lines++;
//                    }
//                }
//            }
//            if (cnt > max) max = cnt;
//            return new Size((int)(font.Width * fontSize * max), (int)((font.Height * lines) * fontSize) + 10);
//        }
//        public static Size MeasureString(int charCount, int lineCnt, FontX font, float fontSize)
//        {
//            return new Size((int)(font.Width * fontSize * charCount), (int)((font.Height * lineCnt) * fontSize) + 10);
//        }
//    }

//    public class ImageX
//    {
//        private char[] buffer;
//        private Size size;
//        public int Length
//        {
//            get { return buffer.Length; }
//        }
//        public ImageX(int w, int h)
//        {
//            size = new Size(w, h);
//            buffer = new char[w * h];
//        }
//        public ImageX()
//        {
//            buffer = new char[0];
//            size = new Size();
//        }

//        public Size Size
//        {
//            get { return size; }
//        }
//        public int Width
//        {
//            get { return size.Width; }
//        }
//        public int Height
//        {
//            get { return size.Height; }
//        }

//        public char GetPixel(int x, int y)
//        {
//            if (x >= size.Width) x = size.Width - 1;
//            else if (x < 0) x = 0;
//            if (y >= size.Height) y = size.Height - 1;
//            else if (y < 0) y = 0;

//            long i = ((y * size.Width) + x);

//            if (i >= 0 && i < buffer.Length)
//            {
//                return buffer[i];
//            }
//            return ' ';
//        }
//        public void SetPixel(int x, int y, char c)
//        {
//            if (x >= size.Width) return;
//            else if (x < 0) return;
//            if (y >= size.Height) return;
//            else if (y < 0) return;

//            //BGRA
//            int i = ((y * size.Width) + x);

//            if (i >= 0 && i < buffer.Length)
//            {
//                buffer[i] = c;
//            }
//        }
//        public char[] Raw
//        {
//            get { return buffer; }
//            set { buffer = value; }
//        }
//        public void CopyTo(IntPtr ptr)
//        {
//            Marshal.Copy(buffer, 0, ptr, buffer.Length);
//        }
//    }

//    public class FontX
//    {
//        private Size size;
//        public int Width
//        {
//            get { return size.Width; }
//        }
//        public int Height
//        {
//            get { return size.Height; }
//        }
//        private ImageX image = new ImageX();
//        private static string[] fontNames = new string[] { "kongtext", "november" };
//        public static void CreateFontX(float fontSize)
//        {
//            byte minChar = 32;
//            byte maxChar = 255;
//            foreach (string s in fontNames)
//            {
//                try
//                {
//                    Font f = new Font(s, fontSize);
//                    Bitmap bitmap = new Bitmap(200, 200);

//                    Graphics g = Graphics.FromImage(bitmap);

//                    SizeF sft = g.MeasureString("A", f);
//                    Size charSize = new Size((int)Math.Ceiling(sft.Width), (int)Math.Ceiling(sft.Height));

//                    g.Dispose();

//                    bitmap = new Bitmap((int)(charSize.Width * (maxChar - minChar)), (int)(charSize.Height));

//                    g = Graphics.FromImage(bitmap);
//                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
//                    g.Clear(Color.Black);
//                    g.FillRectangle(Brushes.Red, charSize.Width, 0, 1, 1);
//                    string sc = Encoding.GetEncoding(1252).GetString(new byte[] { 127 });
//                    for (byte i = minChar; i < maxChar; i++)
//                    {
//                        string si = Encoding.GetEncoding(1252).GetString(new byte[] { i });
//                        if (si != sc)
//                        {
//                            g.DrawString(si, f, Brushes.White, (i - minChar) * charSize.Width, 3);
//                        }
//                        //else
//                        //{
//                        //    g0.FillRectangle(Brushes.White, (x - minChar) * charSize.WorldWidth, 0, charSize.WorldWidth, charSize.WorldHeight);
//                        //}
//                    }
//                    g.Dispose();
//                    bitmap.Save(s + ".png", ImageFormat.Png);
//                }
//                catch (Exception x)
//                {
//                    string sx = x.ToString();
//                }
//            }
//        }
//        public FontX(ImageX b)
//        {
//            image = b;
//            size = GetFontSize(image);
//        }
//        public static Size GetFontSize(ImageX font)
//        {
//            int w = 26;
//            int h = font.Height;

//            while (w < font.Width)
//            {
//                if (font.GetPixel(w, 0) > ' ')
//                {
//                    break;
//                }
//                w++;
//            }
//            return new Size(w, h);
//        }

//        //public Size MeasureString(string s, float fontSize)
//        //{
//        //    //string[] sa = tempText.Split(new char[] { '\obj', '\n' });
//        //    return new Size((int)(WorldWidth * fontSize * s.Length), (int)(WorldHeight * fontSize));
//        //}
//        //public Size MeasureString(int charCount, float fontSize)
//        //{
//        //    //string[] sa = tempText.Split(new char[] { '\obj', '\n' });

//        //    return new Size((int)(WorldWidth * fontSize * charCount), (int)(WorldHeight * fontSize));
//        //}
//        //public void DrawChar(char heightSorter, ImageX toDrawOn, ColorX ColorX, int x, int y)
//        //{
//        //    int x = (int)heightSorter - 32;
//        //    if (heightSorter == ' ') x = 1;
//        //    int left = x * size.WorldWidth;
//        //    int right = left + size.WorldWidth;

//        //    for (int x = left; x < right; x++)
//        //    {
//        //        for (int y = 0; size.WorldHeight > y; y++)
//        //        {
//        //            ColorX cOK = texture.GetPixel(x, y);
//        //            if (cOK.A > 0 && cOK.R > 0 && cOK.G > 0 && cOK.B > 0)
//        //            {
//        //                toDrawOn.SetPixel(x + (x - left), y + y, ColorX);
//        //            }
//        //        }
//        //    }
//        //}
//        public void DrawChar(char c, ImageX toDrawOn, char ColorX, int x, int y, int w, int h)
//        {
//            if (Char.IsControl(c)) return;
//            if ((int)c > 126) return;
//            int i = (int)c - 32;
//            int left = i * size.Width;
//            int right = left + size.Width;

//            float wf = size.Width / w;
//            float hf = size.Height / h;

//            for (int xi = left; xi < right; xi += (int)wf)
//            {
//                for (int yi = 0; size.Height > yi; yi += (int)hf)
//                {
//                    char co = image.GetPixel(xi, yi);
//                    if (co > ' ')
//                    {
//                        toDrawOn.SetPixel((int)(x + (xi - left) / wf), (int)(y + yi / hf), ColorX);
//                    }
//                }
//            }
//        }
//    }

//    public static class MathX
//    {
//        public static int Clamp(int x, int max, int min)
//        {
//            return Math.Max(min, Math.Min(max, x));
//        }
//        public static float Clamp(float x, float max, float min)
//        {
//            return Math.Max(min, Math.Min(max, x));
//        }
//        public static bool InRange(int x, int min, int max)
//        {
//            return (x >= min && x <= max);
//        }
//        public static bool InRange(float x, float min, float max)
//        {
//            return (x >= min && x <= max);
//        }

//        public static int nextPowerOf2(int n)
//        {
//            int count = 0;

//            if (Math.Log(n, 2) == (int)Math.Log(n, 2))
//            {
//                return n;
//            }
//            while (n != 0)
//            {
//                n >>= 1;
//                count += 1;
//            }

//            return 1 << count;
//        }

//        public static void Swap(ref int a, ref int b)
//        {
//            int t = a;
//            a = b;
//            b = t;
//        }

//        public static int map(int x, int in_min, int in_max, int out_min, int out_max)
//        {
//            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
//        }
//        public static float map(float x, float in_min, float in_max, float out_min, float out_max)
//        {
//            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
//        }

//        public static int GetBytesPerPixel(PixelFormat format)
//        {
//            return Image.GetPixelFormatSize(format) / 8;
//        }
//        public static PixelFormat GetPixelsPerByte(int bytesPerPixel)
//        {
//            if (bytesPerPixel == 4) return PixelFormat.Format32bppPArgb;
//            else if (bytesPerPixel == 1) return PixelFormat.Format1bppIndexed;
//            return PixelFormat.Format24bppRgb;
//        }

//        public static byte[] GetBGRA(Bitmap b)
//        {
//            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
//                        ImageLockMode.WriteOnly, b.PixelFormat);
//            byte[] ba = new byte[b.Width * b.Height * GetBytesPerPixel(b.PixelFormat)];
//            Marshal.Copy(bd.Scan0, ba, 0, ba.Length);
//            b.UnlockBits(bd);
//            return ba;
//        }
//        public static Bitmap GetBitmap(byte[] buff, Size size, PixelFormat pixFormat)
//        {
//            Bitmap b = new Bitmap(size.Width, size.Height, pixFormat);
//            BitmapData bd = b.LockBits(new Rectangle(0, 0, size.Width, size.Height),
//                ImageLockMode.WriteOnly, b.PixelFormat);
//            Marshal.Copy(buff, 0, bd.Scan0, buff.Length);
//            b.UnlockBits(bd);
//            return b;
//        }

//        public static int GetIndex(int x, int y, int width, int BitsPerPixel)
//        {
//            return ((y * width) + x) * BitsPerPixel;
//        }
//        public static Color GetPixel(int x, int y, byte[] buff, int width, int format)
//        {
//            int i = ((y * width) + x) * format;
//            if (i >= 0 && i + 2 < buff.Length)
//            {
//                int a = 255;
//                if (format > 3)
//                {
//                    a = buff[i + 3];
//                }
//                return Color.FromArgb(a, buff[i + 2], buff[i + 1], buff[i]); ;
//            }
//            return Color.Transparent;
//        }
//        public static void SetPixel(int x, int y, byte[] buff, int width, int format, int r, int g, int b, int a)
//        {
//            //BGRA
//            int i = ((y * width) + x) * format;
//            if (i >= 0 && i + 2 < buff.Length)
//            {
//                if (a < 255)
//                {
//                    float af = (float)a / 255.0f;
//                    b = (int)(af * b + (1.0f - af) * buff[i + 0]);
//                    g = (int)(af * g + (1.0f - af) * buff[i + 1]);
//                    r = (int)(af * r + (1.0f - af) * buff[i + 2]);
//                }
//                buff[i + 0] = (byte)(b);
//                buff[i + 1] = (byte)(g);
//                buff[i + 2] = (byte)(r);
//            }
//        }
//        public static void SetPixel(int x, int y, byte[] buff, int width, int format, Color c)
//        {
//            SetPixel(x, y, buff, width, format, c.R, c.G, c.B, c.A);
//        }
//    }
//}
