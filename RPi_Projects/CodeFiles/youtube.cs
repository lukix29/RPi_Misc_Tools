using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;

using Newtonsoft.Json.Converters;

namespace Lukix29.Youtube
{
    class youtube
    {
        private static string OMX_KEY_INFO
        {
            get
            {
                return @"Omxplayer Keyboard:
1   decrease speed
2   increase speed
<   rewind
>   fast forward
z   show info
j   previous audio stream
k   next audio stream
i   previous chapter
o   next chapter
q   exit omxplayer
p   pause/resume(space)
-   decrease volume
+   increase volume
<-  seek -30 seconds(left)
->  seek +30 seconds(right)
i  	seek -600 seconds
o	seek +600 seconds";
            }
        }
        private static string[] validArgs = new string[] { "--info", "--keys", "--window", "--output", "--pos" };
        private static string[] argInfos = new string[] {
            "\t\t\t\t= show Video Url.", "\t\t\t\t= show OMX-Player Keys.",
            " \"0 0 1920 1080\"\t= (set OMX-Player Window size)",
            " both/hdmi/local\t= (set Output)", 
            " 10\t\t\t= (seek Video in Minutes)" };
        private static string argWindow = "";
        private static string argOutType = "-o both";
        private static string argStartTime = "";
        private static bool showURL = false;

        public class VideoInfo
        {
            private string videoId;
            private string channelId;
            public string Title
            {
                get;
                private set;
            }
            public string Description
            {
                get;
                private set;
            }
            public string ChannelTitle
            {
                get;
                private set;
            }
            public string Video_Url
            {
                get { return "https://www.youtube.com/watch?v=" + videoId; }
            }
            public string Channel_Url
            {
                get { return "https://www.youtube.com/channel/" + channelId; }
            }
            public VideoInfo(Dictionary<string, string> values)
            {
                foreach (KeyValuePair<string, string> kvp in values)
                {
                    switch (kvp.Key)
                    {
                        case "videoId":
                            videoId = kvp.Value;
                            break;
                        case "channelId":
                            channelId = kvp.Value;
                            break;
                        case "title":
                            Title = kvp.Value;
                            break;
                        case "description":
                            Description = kvp.Value;
                            break;
                        case "channelTitle":
                            ChannelTitle = kvp.Value;
                            break;
                    }
                }
            }

            private static string Get_JSON_Value(string input, string type)
            {
                Newtonsoft.Json.JsonTextReader jtr = new Newtonsoft.Json.JsonTextReader(new StringReader(input));
                while (jtr.Read())
                {
                    if (jtr.Value != null)
                    {
                        if (jtr.ValueType == typeof(string))
                        {
                            if ((string)jtr.Value == type)
                            {
                                // Console.WriteLine(jtr.Value);
                                if (jtr.Read() && jtr.Value != null)
                                {
                                    //Console.WriteLine(jtr.Value);
                                    return (string)jtr.Value;
                                }
                            }
                        }
                    }
                }
                return "";
            }
            public static VideoInfo[] Search_YT_Videos(string name, out string channelName)
            {
                int cnt = 11;
                if (name.Contains(":"))
                {
                    string[] sa = name.Split(':');
                    foreach (string s in sa)
                    {
                        int c = 0;
                        if (int.TryParse(s, out c))
                        {
                            cnt = c + 1;
                        }
                        else
                        {
                            name = s;
                        }
                    }
                }
                string Api_key = "AIzaSyA_CjAIcROaKajZgtQ1SEqp4PCSuX3WJkU";

                string channelReqUrl = "https://www.googleapis.com/youtube/v3/channels?key="
                    + Api_key + "&forUsername=\"" + name + "\"&part=id";

                string result = Helpers.DownloadString(channelReqUrl);

                string channelKey = Get_JSON_Value(result, "id");

                string videoReqUrl = "";
                if (channelKey.Length > 0)
                {
                    videoReqUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet&channelId="
                        + channelKey + "&maxResults=" + cnt + "&order=date&key=" + Api_key;
                }
                else
                {
                    videoReqUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&q=" +
                       "\"" + name + "\"&maxResults=" + cnt + "&key=" + Api_key;
                }
                result = Helpers.DownloadString(videoReqUrl);
                VideoInfo[] vi = VideoInfo.ParseVideos(result);
                if (channelKey.Length > 0)
                {
                    channelName = "Channel: " + vi[0].ChannelTitle;
                }
                else
                {
                    channelName = "Query: " + name;
                }
                return vi;
            }
            private static VideoInfo[] ParseVideos(string input)
            {
                Newtonsoft.Json.JsonTextReader jtr = new Newtonsoft.Json.JsonTextReader(new StringReader(input));
                Dictionary<string, string> list = new Dictionary<string, string>();
                List<VideoInfo> videos = new List<VideoInfo>();
                bool hasFound = false;
                while (jtr.Read())
                {
                    if (jtr.Value != null)
                    {
                        string nme = (string)jtr.Value;
                        if (jtr.Read() && jtr.Value != null)
                        {
                            if (jtr.ValueType == typeof(string))
                            {
                                string val = (string)jtr.Value;
                                if (val == "youtube#searchResult")
                                {
                                    if (hasFound)
                                    {
                                        videos.Add(new VideoInfo(list));
                                        // Console.WriteLine();
                                        list.Clear();
                                    }
                                    else
                                    {
                                        hasFound = true;
                                    }
                                }
                                else if (hasFound)
                                {
                                    //Console.WriteLine(nme + ": " + jtr.Value);
                                    if (!list.ContainsKey(nme))
                                    {
                                        list.Add(nme, (string)jtr.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                return videos.ToArray();
            }
        }
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (!ParseArgs(args))
                {
                    ArgsParser.PrintUsedArgs("youtube.exe", validArgs, argInfos);
                    return;
                }
            }
            //else
            //{
            //    ArgsParser.PrintUsedArgs("youtube.exe [URL]", validArgs, argInfos);
            //    return;
            //}
            string tosearch = "";
            if (File.Exists("youtube.conf"))
            {
                Console.WriteLine("Reading \"youtube.conf\"");
                string[] sarr = File.ReadAllLines("youtube.conf");
                foreach (string s in sarr)
                {
                    if (s.StartsWith("--win"))
                    {
                        argWindow = s;
                    }
                    else if (s.StartsWith("last:"))
                    {
                        tosearch = s.Split(':')[1];
                    }
                }
            }
            Console.Clear();
            Console.WriteLine("Enter search term:");
            tosearch = Console.ReadLine();

            string channelName = "";
            VideoInfo[] videos = VideoInfo.Search_YT_Videos(tosearch, out channelName);
            Menue vMenue = new Menue("Videos for " + channelName, videos.Select(x => x.Title).ToArray());

            Console.Clear();
            int idx = -1;
            idx = vMenue.Update();
            if (idx >= 0)
            {
                Console.Clear();
                if (Helpers.IsLinux || showURL)
                {
                    Console.WriteLine("Loading URL-Dictionary.");
                    URL_Escape.LoadDict();
                }
                string videoURL = "";
                if (Helpers.IsLinux || showURL)
                {
                    Console.WriteLine("Fetching Youtube Video-Url.");

                    videoURL = GetVideoFromUrl(videos[vMenue.Index].Video_Url);
                    videoURL = URL_Escape.ReplaceAll(videoURL);

                    if (showURL)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Video URL:");
                        Console.WriteLine(videoURL);
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("Starting Player.");
                Console.WriteLine();

                Process p = new Process();
                if (Helpers.IsLinux)
                {
                    p.StartInfo.FileName = "mpv";// "omxplayer";
                    p.StartInfo.Arguments = videoURL + " " + argStartTime;
                    //"--live " + argStartTime + " " + argWindow + " " + argOutType + " " + videoURL;
                }
                else
                {
                    p.StartInfo.FileName = "mpv.exe";
                    p.StartInfo.Arguments = videos[vMenue.Index].Video_Url + " " + argStartTime;
                }
                p.StartInfo.UseShellExecute = false;
                p.Start();

                while (!p.HasExited)
                {
                    if (Console.KeyAvailable)
                    {
                        Console.Clear();
                        if (Console.ReadKey().Key == ConsoleKey.Escape)
                        {
                            p.Close();
                            return;
                        }
                        Console.SetCursorPosition(0, Console.BufferHeight - 10);
                    }
                    else System.Threading.Thread.Sleep(100);
                }
            }
            else if (idx == -2)
            {
                Main(new string[0]);
            }
        }


        private static string GetVideoFromUrl(string Url)
        {
            Process p = new Process();
            if (Helpers.IsLinux)
            {
                p.StartInfo.FileName = "youtube-dl";
            }
            else
            {
                p.StartInfo.FileName = ".\\youtube-dl.exe";
                p.StartInfo.WorkingDirectory = ".\\";
            }
            p.StartInfo.Arguments = "--no-playlist -g " + Url;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            while (!p.HasExited) ;
            return p.StandardOutput.ReadToEnd();
        }

        private static bool ParseArgs(string[] arguments)
        {
            bool showKeys = false;
            if (arguments.Length > 0)
            {
                Dictionary<string, string> args = ArgsParser.ParseArgs(arguments, 0, validArgs);
                foreach (KeyValuePair<string, string> kvp in args)
                {
                    string data = kvp.Value;
                    switch (kvp.Key)
                    {
                        case "-h":
                            return false;
                        case "-i":
                            showURL = true;
                            break;

                        case "-k":
                            showKeys = true;
                            break;

                        case "-w":
                            if (data.Length > 0)
                            {
                                argWindow = "--win \"" + data.Replace("\"", "") + "\"";
                                Helpers.Insert("youtube.conf", "--win", argWindow);
                            }
                            else
                            {
                                Console.WriteLine("Deleting Screen Resolution Settings.");
                                File.Delete("youtube.conf");
                            }
                            break;

                        case "-o":
                            if (data.Length > 0)
                            {
                                argOutType = "-o " + data.Split(' ')[0];
                            }
                            else
                            {
                                argOutType = "-o local";
                            }
                            break;

                        case "-p":
                            TimeSpan ts = new TimeSpan(0, int.Parse(data), 0);
                            argStartTime = "--start=" + ts.ToString("hh\\:mm\\:ss");
                            break;
                    }
                }

                if (argWindow.Length > 0)
                {
                    Console.WriteLine("Saving Window Size. ( " + argWindow + ")");
                }
                if (showKeys)
                {
                    Console.WriteLine(OMX_KEY_INFO);
                    Console.WriteLine("\r\n\tUsage: youtube [URL] [OPTIONS]\r\n");
                }
            }
            return true;
        }
    }
    public static class URL_Escape
    {
        private static Dictionary<string, string> URL_Replace_Values = new Dictionary<string, string>();
        private static string Url_Replace_keys
        {
            get
            {
                return @"   %20	%20
!	%21	%21
""	%22	%22
#	%23	%23
$	%24	%24
%	%25	%25
&	%26	%26
'	%27	%27
(	%28	%28
)	%29	%29
*	%2A	%2A
+	%2B	%2B
,	%2C	%2C
-	%2D	%2D
.	%2E	%2E
/	%2F	%2F
0	%30	%30
1	%31	%31
2	%32	%32
3	%33	%33
4	%34	%34
5	%35	%35
6	%36	%36
7	%37	%37
8	%38	%38
9	%39	%39
:	%3A	%3A
;	%3B	%3B
<	%3C	%3C
=	%3D	%3D
>	%3E	%3E
?	%3F	%3F
@	%40	%40
A	%41	%41
B	%42	%42
C	%43	%43
D	%44	%44
E	%45	%45
F	%46	%46
G	%47	%47
H	%48	%48
I	%49	%49
J	%4A	%4A
K	%4B	%4B
L	%4C	%4C
M	%4D	%4D
N	%4E	%4E
O	%4F	%4F
P	%50	%50
Q	%51	%51
R	%52	%52
S	%53	%53
T	%54	%54
U	%55	%55
V	%56	%56
W	%57	%57
X	%58	%58
Y	%59	%59
Z	%5A	%5A
[	%5B	%5B
\	%5C	%5C
]	%5D	%5D
^	%5E	%5E
_	%5F	%5F
`	%60	%60
a	%61	%61
b	%62	%62
c	%63	%63
d	%64	%64
e	%65	%65
f	%66	%66
g	%67	%67
h	%68	%68
i	%69	%69
j	%6A	%6A
k	%6B	%6B
l	%6C	%6C
m	%6D	%6D
n	%6E	%6E
o	%6F	%6F
p	%70	%70
q	%71	%71
r	%72	%72
s	%73	%73
t	%74	%74
u	%75	%75
v	%76	%76
w	%77	%77
x	%78	%78
y	%79	%79
z	%7A	%7A
{	%7B	%7B
|	%7C	%7C
}	%7D	%7D
~	%7E	%7E
 	%7F	%7F
`	%80	%E2%82%AC
	%81	%81
‚	%82	%E2%80%9A
ƒ	%83	%C6%92
„	%84	%E2%80%9E
…	%85	%E2%80%A6
†	%86	%E2%80%A0
‡	%87	%E2%80%A1
ˆ	%88	%CB%86
‰	%89	%E2%80%B0
Š	%8A	%C5%A0
‹	%8B	%E2%80%B9
Œ	%8C	%C5%92
	%8D	%C5%8D
Ž	%8E	%C5%BD
	%8F	%8F
	%90	%C2%90
‘	%91	%E2%80%98
’	%92	%E2%80%99
“	%93	%E2%80%9C
”	%94	%E2%80%9D
•	%95	%E2%80%A2
–	%96	%E2%80%93
—	%97	%E2%80%94
˜	%98	%CB%9C
™	%99	%E2%84
š	%9A	%C5%A1
›	%9B	%E2%80
œ	%9C	%C5%93
	%9D	%9D
ž	%9E	%C5%BE
Ÿ	%9F	%C5%B8
 	%A0	%C2%A0
¡	%A1	%C2%A1
¢	%A2	%C2%A2
£	%A3	%C2%A3
¤	%A4	%C2%A4
¥	%A5	%C2%A5
¦	%A6	%C2%A6
§	%A7	%C2%A7
¨	%A8	%C2%A8
©	%A9	%C2%A9
ª	%AA	%C2%AA
«	%AB	%C2%AB
¬	%AC	%C2%AC
®	%AE	%C2%AE
¯	%AF	%C2%AF
°	%B0	%C2%B0
±	%B1	%C2%B1
²	%B2	%C2%B2
³	%B3	%C2%B3
´	%B4	%C2%B4
µ	%B5	%C2%B5
¶	%B6	%C2%B6
·	%B7	%C2%B7
¸	%B8	%C2%B8
¹	%B9	%C2%B9
º	%BA	%C2%BA
»	%BB	%C2%BB
¼	%BC	%C2%BC
½	%BD	%C2%BD
¾	%BE	%C2%BE
¿	%BF	%C2%BF
À	%C0	%C3%80
Á	%C1	%C3%81
Â	%C2	%C3%82
Ã	%C3	%C3%83
Ä	%C4	%C3%84
Å	%C5	%C3%85
Æ	%C6	%C3%86
Ç	%C7	%C3%87
È	%C8	%C3%88
É	%C9	%C3%89
Ê	%CA	%C3%8A
Ë	%CB	%C3%8B
Ì	%CC	%C3%8C
Í	%CD	%C3%8D
Î	%CE	%C3%8E
Ï	%CF	%C3%8F
Ð	%D0	%C3%90
Ñ	%D1	%C3%91
Ò	%D2	%C3%92
Ó	%D3	%C3%93
Ô	%D4	%C3%94
Õ	%D5	%C3%95
Ö	%D6	%C3%96
×	%D7	%C3%97
Ø	%D8	%C3%98
Ù	%D9	%C3%99
Ú	%DA	%C3%9A
Û	%DB	%C3%9B
Ü	%DC	%C3%9C
Ý	%DD	%C3%9D
Þ	%DE	%C3%9E
ß	%DF	%C3%9F
à	%E0	%C3%A0
á	%E1	%C3%A1
â	%E2	%C3%A2
ã	%E3	%C3%A3
ä	%E4	%C3%A4
å	%E5	%C3%A5
æ	%E6	%C3%A6
ç	%E7	%C3%A7
è	%E8	%C3%A8
é	%E9	%C3%A9
ê	%EA	%C3%AA
ë	%EB	%C3%AB
ì	%EC	%C3%AC
í	%ED	%C3%AD
î	%EE	%C3%AE
ï	%EF	%C3%AF
ð	%F0	%C3%B0
ñ	%F1	%C3%B1
ò	%F2	%C3%B2
ó	%F3	%C3%B3
ô	%F4	%C3%B4
õ	%F5	%C3%B5
ö	%F6	%C3%B6
÷	%F7	%C3%B7
ø	%F8	%C3%B8
ù	%F9	%C3%B9
ú	%FA	%C3%BA
û	%FB	%C3%BB
ü	%FC	%C3%BC
ý	%FD	%C3%BD
þ	%FE	%C3%BE
ÿ	%FF	%C3%BF";
            }
        }
        public static void LoadDict()
        {
            URL_Replace_Values = new Dictionary<string, string>();
            string[] sai = Url_Replace_keys.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in sai)
            {
                string[] si = s.Split('\t');
                if (si.Length > 2)
                {
                    string value = si[0];
                    string key = si[2];
                    if (!URL_Replace_Values.ContainsKey(key))
                    {
                        URL_Replace_Values.Add(key, value);
                    }
                }
            }
        }
        public static string ReplaceAll(string input)
        {
            foreach (string s in URL_Replace_Values.Keys.ToArray())
            {
                input = input.Replace(s, URL_Replace_Values[s]);
            }
            return input;
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
        private static string[] GetArgs(string[] validArgs, int startIndex, string[] args)
        {
            string arg = "";
            for (int i = startIndex; i < args.Length; i++)
            {
                arg += args[i] + " ";
            }
            string[] sa = arg.Split('-');
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
                args = GetArgs(validArgs, startIndex, args);
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
        private void Draw()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(title);
            Console.WriteLine("----------------");

            for (int i = 0; i < entries.Length; i++)
            {
                string s = (index == i) ? "->" : " ";
                string all = (i + 1).ToString("0#") + " " + s + entries[i] + "   ";
                if (all.Length >= Console.BufferWidth)
                {
                    all = all.Remove(Console.BufferWidth - 1);
                }
                Console.WriteLine(all);
            }

            Console.WriteLine("----------------");
            Console.WriteLine("New Search = \"R\"");
            Console.WriteLine("Exit = \"Escape\"");
        }
        public int Update()
        {
            Console.CursorVisible = false;
            Draw();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.R:
                            return -2;
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
                            Console.CursorVisible = true;
                            Console.Clear();
                            return index;
                        case ConsoleKey.Escape:
                            Console.CursorVisible = true;
                            Console.Clear();
                            return -1;
                        case ConsoleKey.E:
                            Console.CursorVisible = true;
                            Console.Clear();
                            return -1;
                    }
                    Draw();
                }
            }
        }
    }
    public static class Helpers
    {
        public static void Insert(string fileName, int index, string toinsert)
        {
            if (File.Exists(fileName))
            {
                string[] sa = File.ReadAllLines(fileName);
                if (index < sa.Length)
                {
                    sa[index] = toinsert;
                    File.WriteAllLines(fileName, sa);
                }
            }
            else
            {
                File.WriteAllText(fileName, toinsert);
            }
        }
        public static void Insert(string fileName, string id, string toinsert)
        {
            if (File.Exists(fileName))
            {
                string[] sa = File.ReadAllLines(fileName);
                for (int i = 0; i < sa.Length; i++)
                {
                    if (sa[i].Contains(id))
                    {
                        sa[i] = toinsert;
                    }
                }
                File.WriteAllLines(fileName, sa);
            }
            else
            {
                File.WriteAllText(fileName, toinsert);
            }
        }

        public static string DownloadString(string url)
        {
            if (!IsLinux)
            {
                WebClient wc = new WebClient();
                return wc.DownloadString(url);
            }
            else
            {
                Process p = new Process();
                p.StartInfo.FileName = "wget";
                p.StartInfo.Arguments = "-q -O - " + url;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                while (!p.HasExited) ;
                //p.BeginOutputReadLine();
                string s = p.StandardOutput.ReadToEnd();
                //Console.WriteLine(url);
                //Console.WriteLine(s);
                //Console.ReadKey();
                return s;
            }
        }
        public static string[] Split(this string s, StringSplitOptions sso, params string[] sa)
        {
            return s.Split(sa, sso);
        }
        public static string[] Split(this string s, params string[] sa)
        {
            return s.Split(sa, StringSplitOptions.None);
        }
        public static int[] Size
        {
            get { return new int[] { Console.BufferWidth, Console.BufferHeight }; }
        }
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public static string GetStdOut(string app, string args)
        {
            Process p = new Process();
            p.StartInfo.FileName = app;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            while (!p.HasExited) ;
            return p.StandardOutput.ReadToEnd();
        }
        public static string ReplaceAll(this string input, string replace_to, params string[] to_replace)
        {
            foreach (string s in to_replace)
            {
                input = input.Replace(s, replace_to);
            }
            return input;
        }
    }
}
