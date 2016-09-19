using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lukix29
{
    public class LRE
    {
        public static void Main(string[] args)
        {
            Bitmap b = new Bitmap("test.png");
            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);
            byte[] input = new byte[b.Width * b.Height * (Image.GetPixelFormatSize(b.PixelFormat) / 8)];
            Marshal.Copy(bd.Scan0, input, 0, input.Length);
            b.UnlockBits(bd);
            List<int> output = new List<int>();
            int cnt = 1;
            int temp = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                if (temp == input[i])
                {
                    cnt++;
                    if (cnt == Int16.MaxValue || i >= input.Length - 1)
                    {
                        output.Add(cnt);
                        output.Add(temp);
                        cnt = 1;
                    }
                }
                if (temp != input[i])
                {
                    output.Add(cnt);
                    output.Add(temp);
                    cnt = 1;
                }
                temp = input[i];
            }
            List<byte> outs = new List<byte>();
            for (int i = 0; i < output.Count; i += 2)
            {
                byte[] ba = new byte[output[i]];
                ba.Populate((byte)output[i + 1]);
                outs.AddRange(ba);
            }

            b = new Bitmap(b.Width, b.Height, PixelFormat.Format24bppRgb);
            bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);
            input = new byte[b.Width * b.Height * (Image.GetPixelFormatSize(b.PixelFormat) / 8)];
            Marshal.Copy(input, 0, bd.Scan0, input.Length);
            b.UnlockBits(bd);
            b.Save("test0.png", ImageFormat.Png);

            Console.WriteLine(input.Length);
            Console.WriteLine(output.Count);
            Console.WriteLine(outs.Count);
            Console.ReadKey();
        }
    }
    public static class EXT
    {
        public static void Populate(this byte[] arr, byte value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }
    }
}
