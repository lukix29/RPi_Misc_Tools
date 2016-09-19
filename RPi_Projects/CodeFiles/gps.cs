using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace Lukix29
{
    class gps
    {
        static string[] arguments = new string[] { "-p (--parse)", "-s (--settime)", "-o (--once)", "-r (--raw)" };
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (!File.Exists("/dev/ttyUSB0")) return;
                bool parse = ParseArgs(args, "p", "parse");
                bool settime = ParseArgs(args, "s", "set", "settime");
                bool once = ParseArgs(args, "o", "once");
                bool raw = ParseArgs(args, "r", "raw");
                if (settime)
                {
                    parse = true;
                    raw = false;
                    once = true;
                }
                if (once)
                {
                    parse = true;
                    raw = false;
                }

                SerialPort sp = new SerialPort("/dev/ttyUSB0", 9600, Parity.None, 8, StopBits.One);
                sp.Open();

                Console.Clear();

                NMEA_Parser parser = new NMEA_Parser();
                bool hasFix = true;
                while (true)
                {
                    if (parse)
                    {
                        string s = sp.ReadLine();

                        GPS_Data data = parser.GPGGA(s);
                        DateTime dt = parser.GPZDA(s);
                        if (data != null)
                        {
                            Console.WriteLine(data.ToString());
                            Console.WriteLine();
                            hasFix = true;
                        }
                        else if (dt.Ticks > 0)
                        {
                            if (dt.Ticks > 0)
                            {
                                //"HH:MM:SS MM/DD/YYYY"
                                if (settime)
                                {
                                    Console.WriteLine("Set Time to:");
                                    Process p = Process.Start("date", "-s \"" + dt.ToString().Replace(".", "/") + "\"");
                                    while (!p.HasExited) ;
                                }
                                else
                                {
                                    Console.WriteLine(dt.ToString());
                                    Console.WriteLine();
                                }
                                if (once) break;
                                hasFix = true;
                                Console.SetCursorPosition(0, 0);
                            }
                        }
                        else
                        {
                            if (hasFix)
                            {
                                Console.WriteLine("No Fix available!");
                            }
                            hasFix = false;
                        }
                    }
                    if (raw)
                    {
                        Console.Write(sp.ReadExisting());
                    }
                }
                sp.Close();
            }
            else
            {
                Console.WriteLine("Usage:");
                foreach (string s in arguments)
                {
                    Console.Write("gps.exe "); Console.WriteLine(s);
                }
                Console.WriteLine("Set Time requires root permissions!");
                Console.WriteLine();
            }
        }

        public class GPS_Data
        {
            public static float rawToDeg(string s)
            {
                float raw = float.Parse(s, System.Globalization.NumberStyles.Any); //4807.038,N   Latitude 48 deg 07.038' N
                float degWhole = (float)((int)(raw / 100));
                float degDec = (raw - degWhole * 100) / 60;
                float deg = degWhole + degDec;
                //if (head == 'W' || head == 'S')
                //{
                //    deg = -deg;
                //}
                return deg;
            }
            public const int Length = 12;
            public GPS_Data(string[] sa)
            {
                Time = NMEA_Parser.parseUTC(sa[1]);

                Latitude = rawToDeg(sa[2]);
                LatHeading = sa[3];
                Longitude = rawToDeg(sa[4]);
                LongHeading = sa[5];
                Quality = int.Parse(sa[6]);
                Satelites = int.Parse(sa[7]);
                Dilution = float.Parse(sa[8], System.Globalization.NumberStyles.Any);
                Altitude = float.Parse(sa[9], System.Globalization.NumberStyles.Any);
                AltMeasure = sa[10];
                WGS84 = float.Parse(sa[11], System.Globalization.NumberStyles.Any);
                WGS84Measure = sa[12];
            }

            public DateTime Time
            {
                get;
                private set;
            }
            public string AltMeasure
            {
                get;
                private set;
            }
            public string WGS84Measure
            {
                get;
                private set;
            }
            public string LatHeading
            {
                get;
                private set;
            }
            public string LongHeading
            {
                get;
                private set;
            }
            public float Latitude
            {
                get;
                private set;
            }
            public float Longitude
            {
                get;
                private set;
            }
            /// <summary>
            ///Fix quality: 
            ///0 = invalid
            ///1 = GPS fix (SPS)
            ///2 = DGPS fix
            ///3 = PPS fix
            ///4 = Real Time Kinematic
            ///5 = Float RTK
            ///6 = estimated (dead reckoning) (2.3 feature)
            ///7 = Manual input mode
            ///8 = Simulation mode
            /// </summary>
            public int Quality
            {
                get;
                private set;
            }
            public int Satelites
            {
                get;
                private set;
            }
            public float Dilution
            {
                get;
                private set;
            }
            public float Altitude
            {
                get;
                private set;
            }
            /// <summary>
            /// Height of geoid (mean sea level) above WGS84 ellipsoid
            /// </summary>
            public float WGS84
            {
                get;
                private set;
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Global Positioning System Fix Data");
                sb.AppendLine("Fix taken at:\t" + Time.ToLongTimeString());
                sb.AppendLine("Latitude:\t" + Latitude + LatHeading);
                sb.AppendLine("Longitude:\t" + Longitude + LongHeading);
                sb.AppendLine("Fix quality:\t" + NMEA_Parser.QualityName(Quality));
                sb.AppendLine("Satellites:\t" + Satelites);
                sb.AppendLine("Dilution:\t" + Dilution);
                sb.AppendLine("Altitude:\t" + Altitude + AltMeasure);
                sb.AppendLine("Mean Sea Level:\t" + WGS84 + WGS84Measure);
                return sb.ToString();
            }
        }
        public class NMEA_Parser
        {
            private string[] Profiles = new string[] { "GPGGA", "GPZDA" };

            public static string QualityName(int quality)
            {
                string[] sa = new string[]{
                    "Invalid",
                    "GPS fix (SPS)",
                    "DGPS fix",
                    "PPS fix",
                    "Real Time Kinematic",
                    "Float RTK",
                    "Estimated (dead reckoning)",
                    "Manual input mode","Simulation mode"};
                return sa[quality];
            }
            public static DateTime parseUTC(string utc, int Year, int Month, int Day)
            {
                string s = utc.Substring(0, 2);
                int Hour = int.Parse(s);
                s = utc.Substring(2, 2);
                int Minute = int.Parse(s);
                s = utc.Substring(4, 2);
                int Second = int.Parse(s);

                return TimeZone.CurrentTimeZone.ToLocalTime(
                    new DateTime(Year, Month, Day, Hour, Minute, Second));
            }
            public static DateTime parseUTC(string utc)
            {
                return parseUTC(utc, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            private string[] getNMEAvalues(string input)
            {
                if (input.Contains("$") && input.Contains(",") &&
                    input.Contains("*") && Contains(input, Profiles))
                {
                    string s = Remove(input, "\r", "\n", " ");
                    int i0 = s.IndexOf("$");
                    int i1 = s.IndexOf("*");
                    if (i1 - i0 > 5)
                    {
                        s = s.Substring(i0, i1 - i0);
                        if (s.Count<char>(new Func<char, bool>(delegate(char c) { return (c == '$'); })) == 1)
                        {
                            int i3 = s.LastIndexOf(",");
                            s = s.Substring(0, i3);
                            string[] sa = Split(input, ",");
                            for (int i = 0; i < sa.Length; i++)
                            {
                                sa[i] = Remove(sa[i], ",", "$");
                            }
                            return sa;
                        }
                    }
                }
                return new string[0];
            }

            public GPS_Data GPGGA(string NMEA_Sentence)
            {
                string[] values = getNMEAvalues(NMEA_Sentence);
                if (values.Length >= GPS_Data.Length)
                {
                    if (values[0] == "GPGGA")
                    {
                        return new GPS_Data(values);
                    }
                }
                return null;
            }
            public DateTime GPZDA(string NMEA_Sentence)
            {
                string[] values = getNMEAvalues(NMEA_Sentence);
                if (values.Length >= 5)
                {
                    if (values[0] == "GPZDA")
                    {
                        int day = int.Parse(values[2]);
                        int month = int.Parse(values[3]);
                        int year = int.Parse(values[4]);
                        //Console.WriteLine(time);
                        //Console.WriteLine(day);
                        //Console.WriteLine(month);
                        //Console.WriteLine(year);
                        return parseUTC(values[1], year, month, day);
                    }
                }
                return new DateTime(0);
            }
        }


        public static string[] Split(string s, params string[] arr)
        {
            return s.Split(arr, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string Remove(string sin, params string[] arr)
        {
            foreach (string s in arr)
            {
                sin = sin.Replace(s, "");
            }
            return sin;
        }
        public static bool Contains(string sin, params string[] arr)
        {
            foreach (string s in arr)
            {
                if (sin.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ParseArgs(string[] arr, params string[] items)
        {
            if (arr.Length > 0)
            {
                foreach (string s in arr)
                {
                    foreach (string si in items)
                    {
                        if (Remove(s, "-", "/").Contains(si))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
