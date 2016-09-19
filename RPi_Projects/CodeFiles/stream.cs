using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using LX29_TwitchApi;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Lukix29
{
    class StreamManager
    {
        public static string[] QUALITIES = new string[] { "audio", "worst", "low", "medium", "high", "source" };
        private static string[] validArgs = new string[] { "--both", "--hdmi", "--analog" };
        private static string[] argInfos = new string[] {
            " \"0 0 1920 1080\"\t= (set OMX-Player Window size)", 
            " \"0 0 1920 1080\"\t= (set OMX-Player Window size)", 
            " \"0 0 1920 1080\"\t= (set OMX-Player Window size)" };

        static void Main(string[] args)
        {
            if (!Settings.ParseArgs(args, validArgs, argInfos))
            {
                return;
            }
            StreamMenue menue = new StreamMenue("Stream", File.ReadAllLines("streams.txt"));

            Console.CursorVisible = false;
            Console.Clear();

            if (menue.Update() >= 0)
            {
                startStream(menue.CurEntry, menue.StartIfOnline, menue.Quality);
            }
            Console.Clear();
        }

        private static void startStream(string name, bool startIfOnline, string qualy)
        {
            if (!startIfOnline)
            {
                Menue menue = new Menue("Qualities:", QUALITIES);
                if (menue.Update() >= 0)
                {
                    qualy = menue.CurEntry;
                    startIfOnline = true;
                }
            }
            if (startIfOnline)
            {
                string prog = "livestreamer";
                string arg = "";

                arg += " twitch.tv/" + name.ToLower() + " " + qualy;

                if (Settings.IsLinux)
                {
                    arg += " --player-passthrough hls --hls-segment-threads 2 -p 'mpv --cache 2048 --video-aspect=1.706666'";
                }
                else
                {
                    prog = "C:\\Program Files (x86)\\Livestreamer\\livestreamer.exe";
                }
                Console.Clear();
                Console.WriteLine("Starting " + name + "'s Stream");
                Console.WriteLine("Press \"Escape\" to stop the Stream (or \"e\")\r\n");

                Process p = new Process();
                p.StartInfo.FileName = prog;
                p.StartInfo.Arguments = arg;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                //WaitForConsole(p);
                while (!p.HasExited)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey key = Console.ReadKey().Key;
                        if (key == ConsoleKey.Escape || key == ConsoleKey.Q)
                        {
                            p.Kill();
                            if (!p.HasExited)
                            {
                                Process.Start("pkill", "mpv");
                            }
                            break;
                        }
                    }
                    else System.Threading.Thread.Sleep(200);
                }
            }
            Main(null);
        }
    }

    public static class ArgsParser
    {
        public static void PrintUsedArgs(string prog, string[] validArgs, string[] argInfos)
        {
            Console.WriteLine("Usage: " + prog + " [OPTIONS]");
            Console.WriteLine(" Options:");
            for (int i = 0; i < validArgs.Length; i++)
            {
                string s = "-" + validArgs[i].Replace("--", "")[0];
                Console.WriteLine("  " + s + " or " + validArgs[i] + argInfos[i]);
            }
        }
        private static string[] GetArgs(string[] validArgs, string[] args)
        {
            string arg = "";
            for (int i = 0; i < args.Length; i++)
            {
                arg += args[i] + " ";
            }
            string[] sa = arg.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < sa.Length; i++)
            {
                sa[i] = "-" + sa[i];
            }
            return sa;
        }
        private static string parseKey(string[] validArgs, string s, out string data)
        {
            s = s.ToLower();
            foreach (string k in validArgs)
            {
                string keyS = ("-" + k.Replace("--", "")[0]).ToLower();
                string keyL = k.ToLower();
                string si = s.Replace(keyL, keyS);
                if (si.StartsWith(keyS))
                {
                    data = si.Replace(keyS, "").Trim(' ');
                    return keyS;
                }
            }
            data = "";
            return "";
        }
        /// <summary>
        /// Dictionary(string, string) args = ArgsParser.ParseArgs(arguments, 0, usedArgs);
        /// foreach (KeyValuePair(string, string) kvp in args)
        ///  string data = kvp.Value;
        ///  switch (kvp.Key)
        /// </summary>
        /// <param name="args">Passed Arguments from Main(string[] args).</param>
        /// <param name="startIndex">Index to start.</param>
        /// <param name="validArgs">Arguments to use e.g. "--clear"</param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseArgs(string[] args, int startIndex, params string[] validArgs)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (args.Length >= startIndex)
            {
                args = GetArgs(validArgs, args);
                for (int i = startIndex; i < args.Length; i++)
                {
                    string data = "";
                    string key = parseKey(validArgs, args[i], out data);
                    dict.Add(key, data);
                }
            }
            return dict;
        }
    }
    public static class Settings
    {
        public enum Types
        {
            Analog,
            Both,
            HDMI
        }
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        private const string linuxConfigPath = "/home/pi/sharp/stream.conf";
        private const string winConfigPath = ".\\stream.conf";
        private static string palconfig = IsLinux ? linuxConfigPath : winConfigPath;
        private static Dictionary<string, string> settings = new Dictionary<string, string>();

        public static string GetValue(Types type)
        {
            return settings[Enum.GetName(typeof(Types), type)];
        }

        public static bool ParseArgs(string[] args, string[] validArgs, string[] argInfos)
        {
            //Maps.me Android
            LoadSettings();
            if (args != null && args.Length > 0)
            {
                Dictionary<string, string> dict = ArgsParser.ParseArgs(args, 0, validArgs);
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    //if (s.StartsWith("-h"))
                    //{
                    //    ArgsParser.PrintUsedArgs("stream.exe", validArgs, argInfos);
                    //    return false;
                    //}
                    foreach (string s in Enum.GetNames(typeof(Types)))
                    {
                        if (s.ToLower().StartsWith(kvp.Key.Replace("-", "")))
                        {
                            settings[s] = kvp.Value;
                        }
                    }
                }
                SaveSettings();
            }
            return true;
        }

        private static void SaveSettings()
        {
            StreamWriter sw = new StreamWriter(File.Create(palconfig));
            foreach (KeyValuePair<string, string> kvp in settings)
            {
                sw.WriteLine(kvp.Key + ":" + kvp.Value);
            }
            sw.Close();
        }
        private static void LoadSettings()
        {
            if (File.Exists(palconfig))
            {
                settings = new Dictionary<string, string>();
                string[] sa = File.ReadAllLines(palconfig);
                foreach (string s in sa)
                {
                    string[] sarr = s.Split(':');
                    settings.Add(sarr[0], sarr[1]);
                }
            }
            else
            {
                settings.Add("HDMI", "0 0 1920 1080");
                settings.Add("Analog", "0 0 720 560");
                settings.Add("Both", "0 0 1920 1080");
                SaveSettings();
            }
        }
    }
    public class Menue
    {
        private string title = "";
        private string[] entries;
        private int index = 0;
        public int Index
        {
            get { return index; }
            set
            {
                if (value >= 0 && value < entries.Length)
                {
                    index = value;
                }
            }
        }
        public string CurEntry
        {
            get { return entries[index]; }
        }

        public Menue(string title, params string[] entry)
        {
            this.title = title;
            entries = entry;
        }
        DateTime dt = DateTime.Now;
        public int Update()
        {
            Console.Clear();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.W:
                            index = Math.Max(0, index - 1);
                            break;
                        case ConsoleKey.UpArrow:
                            index = Math.Max(0, index - 1);
                            break;
                        case ConsoleKey.S:
                            index = Math.Min(entries.Length - 1, index + 1);
                            break;
                        case ConsoleKey.DownArrow:
                            index = Math.Min(entries.Length - 1, index + 1);
                            break;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            return index;
                        case ConsoleKey.Escape:
                            Console.Clear();
                            return -1;
                        case ConsoleKey.E:
                            Console.Clear();
                            return -1;
                    }
                }
                if (DateTime.Now.Subtract(dt).TotalMilliseconds > 50)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Select " + title);
                    Console.WriteLine("----------------");

                    for (int i = 0; i < entries.Length; i++)
                    {
                        string s = (index == i) ? ">" : " ";
                        Console.WriteLine(s + entries[i]);
                    }

                    Console.WriteLine("----------------");
                    Console.WriteLine("Exit = \"Escape\"");

                    dt = DateTime.Now;
                }
            }
        }
    }
    public class StreamMenue
    {
        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        private bool loaded = false;
        private string title = "";
        private bool[] isOnline;
        private string[] entries;
        private int quality = StreamManager.QUALITIES.Length - 1;
        private bool abortThread = false;
        private int startIfOnline;
        private int index = 0;
        public int Index
        {
            get { return index; }
            set
            {
                if (value >= 0 && value < entries.Length)
                {
                    index = value;
                }
            }
        }
        public string CurEntry
        {
            get { return entries[index]; }
        }
        public bool StartIfOnline
        {
            get { return startIfOnline == index; }
        }
        public string Quality
        {
            get { return StreamManager.QUALITIES[quality]; }
        }


        public StreamMenue(string title, params string[] entry)
        {
            this.title = title;
            entries = entry;
        }
        private void updateOnline()
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (abortThread)
                {
                    return;
                }
                string name = entries[i];
                string s = "\"stream\":null";
                if (!IsLinux)
                {
                    //ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                    WebClient wc = new WebClient();
                    s = wc.UploadString("http://api.twitch.tv/kraken/streams/" +
                        TwitchApi.GetName(name), "");
                    wc.Dispose();
                }
                else
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "wget";
                    p.StartInfo.Arguments = "-q -O - https://api.twitch.tv/kraken/streams/" +
                        TwitchApi.GetName(name);
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    while (!p.HasExited)
                    {
                        if (abortThread)
                        {
                            p.Kill();
                            return;
                        }
                    }
                    //p.BeginOutputReadLine();
                    s = p.StandardOutput.ReadToEnd();
                }
                if (s.Contains("\"stream\":null"))
                {
                    isOnline[i] = false;
                }
                else
                {
                    isOnline[i] = true;
                    if (abortThread) return;
                    Console.Clear();
                    DrawMenue();
                }
            }

            loaded = true;
            Console.Clear();
            DrawMenue();
        }

        private void DrawMenue()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Select " + title + ((loaded) ? "" : " (Loading Online Status)"));
            Console.WriteLine("----------------");

            for (int i = 0; i < entries.Length; i++)
            {
                string s = (index == i) ? "->" : "  ";
                string sio = (startIfOnline == i) ? "[X]" : "[ ]";
                string isOn = isOnline[i] ? "  *ONLINE*" : "";
                Console.Write(s + sio + entries[i] + isOn);
                if (startIfOnline == i)
                {
                    Console.Write(" Quality(A-D):" + StreamManager.QUALITIES[quality] + "      ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("----------------");
            Console.WriteLine("Exit = \"Escape\"");
        }
        public int Update()
        {
            Console.Clear();

            isOnline = new bool[entries.Length];
            startIfOnline = -1;

            DrawMenue();
            loaded = true;
            int updtCnt = 10;
            DateTime dtUpdate = new DateTime(0);
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.A:
                            if (startIfOnline >= 0)
                            {
                                quality = (quality - 1 < 0) ? StreamManager.QUALITIES.Length - 1 : quality - 1;
                            }
                            break;
                        case ConsoleKey.D:
                            if (startIfOnline >= 0)
                            {
                                quality = (quality + 1 >= StreamManager.QUALITIES.Length) ? 0 : quality + 1;
                            }
                            break;
                        case ConsoleKey.W:
                            index = (index - 1 < 0) ? entries.Length - 1 : index - 1;
                            break;
                        case ConsoleKey.UpArrow:
                            index = (index - 1 < 0) ? entries.Length - 1 : index - 1;
                            break;
                        case ConsoleKey.S:
                            index = (index + 1 >= entries.Length) ? 0 : index + 1;
                            break;
                        case ConsoleKey.DownArrow:
                            index = (index + 1 >= entries.Length) ? 0 : index + 1;
                            break;
                        case ConsoleKey.Enter:
                            abortThread = true;
                            if (isOnline[index])
                            {
                                Console.Clear();
                                return index;
                            }
                            else
                            {
                                startIfOnline = index;
                                break;
                            }
                        case ConsoleKey.Escape:
                            abortThread = true;
                            Console.Clear();
                            return -1;
                        case ConsoleKey.Q:
                            abortThread = true;
                            Console.Clear();
                            return -1;
                        case ConsoleKey.Delete:
                            StringBuilder sw = new StringBuilder();
                            List<string> list = new List<string>();
                            for (int i = 0; i < entries.Length; i++)
                            {
                                if (index != i)
                                {
                                    list.Add(entries[i]);
                                    sw.AppendLine(entries[i]);
                                }
                            }
                            entries = list.ToArray();
                            File.WriteAllText("streams.txt", sw.ToString());
                            break;
                    }
                    DrawMenue();
                }
                if (DateTime.Now.Subtract(dtUpdate).Seconds >= 10)
                {
                    for (int i = 0; entries.Length > i; i++)
                    {
                        if (startIfOnline == index)
                        {
                            return index;
                        }
                    }
                    if (updtCnt++ >= 10 && loaded)
                    {
                        updtCnt = 0;
                        loaded = false;
                        new Thread(updateOnline).Start();
                    }
                    dtUpdate = DateTime.Now;
                }
            }
        }
    }

}
namespace LX29_TwitchApi
{
    public enum ApiRequests : int
    {
        users = 0,
        streams = 1,
        channels = 2,
    }
    public enum ApiErrors : int
    {
        Error = -2,
        None_Online = -1,
        NotFound = 0,
        Offline = 1,
    }

    public enum ApiInfo : int
    {
        display_name = 0,
        name = 1,
        bio = 2,
        game = 3,
        status = 4,
        viewers = 5,
        views = 6,
        followers = 7,
        video_height = 8,
        average_fps = 9,
        language = 10,
        delay = -1,
        is_playlist = -2,
        created_at = -3,
        updated_at = -4,
        partner = -6,
        mature = -7,
        large = -8
    }

    public static class ApiRequestKeyNames
    {
        public static string[] UserInfo
        {
            get
            {
                return new string[]
                {
                "Display Name: ",//0
                "Profile: ",//1
                "Bio: ",//2
                "Game: ",//3
                "Status: ",//4
                "Viewers: ",//5
                "Views: ",//6
                "Followers: ",//7
                "Resolution: ",//8
                "Framerate: ",//9
                "Language: "//10
                };
            }
        }
        public static string[] ApiError
        {
            get
            {
                return new string[]
                {
                "error:Not Found",
                "stream:null"
                };
            }
        }
    }

    public struct ApiResult
    {
        private string raw;
        private ApiErrors error;
        private ApiRequests request;

        public static readonly ApiResult Empty = new ApiResult();
        public bool IsEmpty
        {
            get { return raw.Length == 0; }
        }
        public string Result
        {
            get { return raw; }
        }
        public ApiErrors Error
        {
            get { return error; }
        }
        public ApiRequests RequestType
        {
            get { return request; }
        }
        public ApiResult(string Raw, ApiErrors error, ApiRequests request)
        {
            raw = Raw;
            this.error = error;
            this.request = request;
        }

        public string GetValue(ApiInfo type)
        {
            string[] input = raw.Replace("\"", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string nem = Enum.GetName(typeof(ApiInfo), type).ToLower() + ":";
            if (input.Length > 0)
            {
                foreach (string s in input)
                {
                    if (s.StartsWith(nem))
                    {
                        string si = s.Replace(nem, "");
                        return ConvertValue(si, type);
                    }
                }
            }
            return "";
        }
        private string ConvertValue(string s, ApiInfo type)
        {
            if (type == ApiInfo.name)
            {
                return "https://twitch.tv/" + s + "/profile";
            }
            else if (type == ApiInfo.language)
            {
                CultureInfo ci = CultureInfo.GetCultureInfo(s, s);
                if (CultureInfo.CurrentCulture.EnglishName.Contains(ci.EnglishName))
                {
                    s = ci.NativeName;
                }
                else
                {
                    s = ci.EnglishName;
                }
            }
            else if (char.IsDigit(s[0]))
            {
                s = s.Replace(".", ",");
                int i = 0;
                float f = 0;
                bool b = false;
                if (int.TryParse(s, out i))
                {
                    s = i.ToString("N0");
                    if (type == ApiInfo.video_height)
                    {
                        s += "p";
                    }
                }
                else if (float.TryParse(s, out f))
                {
                    s = f.ToString("F0");
                    if (type == ApiInfo.average_fps)
                    {
                        s += "fps";
                    }
                }
                else if (bool.TryParse(s, out b))
                {
                    s = b.ToString();
                }
            }
            return s;
        }

        public string GetInfos()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= 10; i++)
            {
                try
                {
                    sb.Append(ApiRequestKeyNames.UserInfo[i]);
                    string s = GetValue((ApiInfo)i);
                    if (s.Length > 0)
                    {
                        sb.AppendLine(s);
                        sb.AppendLine();
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.ToString());//x.Handle("Error.log", false);
                }
            }
            return sb.ToString();
        }
    }
    public static class TwitchApi
    {
        public static string GetName(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsLetterOrDigit(input[i]) || char.IsWhiteSpace(input[i]))
                {
                    string s = input.Substring(0, i).ToLower();
                    return s;
                }
            }
            return input.ToLower();
        }
        private static ApiErrors CheckError(string input)
        {
            input = input.Replace("\"", "");
            for (int i = 0; i < ApiRequestKeyNames.ApiError.Length; i++)
            {
                if (input.Contains(ApiRequestKeyNames.ApiError[i]))
                {
                    return (ApiErrors)i;
                }
            }
            return ApiErrors.None_Online;
        }
        private static string getApiUrl(ApiRequests type, string name)
        {
            return "https://api.twitch.tv/kraken/" +
                Enum.GetName(typeof(ApiRequests), type) + "/" + name;
        }

        public static ApiResult GetApiRequest(ApiRequests type, string name)
        {
            name = GetName(name);
            string url = getApiUrl(type, name);
            try
            {
                ApiErrors error = ApiErrors.None_Online;
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string outs = wc.DownloadString(url).Replace("{", "").Replace("}", "");
                wc.Dispose();

                error = CheckError(outs);
                //if (error == ApiErrors.None)
                //{
                //    outs = outs.Replace("{\"stream\":", "");
                //}
                return new ApiResult(outs, error, type);
            }
            catch (Exception x)
            {
                return new ApiResult(x.Message, ApiErrors.Error, type);
            }
        }
    }
}
