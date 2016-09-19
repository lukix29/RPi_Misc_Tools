using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RNG
{
    public class HWRNG
    {
        public static void Main(string[] args)
        {

        }
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        private System.Security.Cryptography.RandomNumberGenerator rng = null;
        private int bufferSize = 32 * 8;
        private byte[] buffer;
        private int bufferIndex = 0;

        public HWRNG()
        {
            buffer = new byte[bufferSize];
            FillBuffer();
        }
        public HWRNG(int BufferSize)
        {
            bufferSize = bufferSize * 8;
            buffer = new byte[bufferSize];
            FillBuffer();
        }
        public void FillBuffer()
        {
            if (IsLinux)
            {
                FileStream fs = File.OpenRead("/dev/random");
                for (int i = 0; i < bufferSize; i++)
                {
                    buffer[i] = (byte)fs.ReadByte();
                }
                fs.Close();
            }
            else
            {
                if (rng == null)
                {
                    rng = System.Security.Cryptography.RandomNumberGenerator.Create();
                }
                rng.GetBytes(buffer);
                rng.Dispose();
            }
            bufferIndex = 0;
        }
        public double NextDouble()
        {
            long l = Math.Abs(BitConverter.ToInt64(buffer, bufferIndex));
            double f = l / Math.Pow(10, Math.Floor(Math.Log10(l) + 1));
            bufferIndex += 8;
            if (bufferIndex >= bufferSize)
            {
                FillBuffer();
            }
            return f;
        }

        public int Next(int Min, int Max)
        {
            return (int)NextDouble(Min, Max + 1);
        }
        public long Next(long Min, long Max)
        {
            return (long)NextDouble(Min, Max + 1);
        }
        public byte NextByte(byte Min, byte Max)
        {
            return (byte)((double)(Min + (Max - Min)) * NextDouble());
        }
        public byte NextByte()
        {
            return (byte)Next((int)byte.MinValue, (int)byte.MaxValue + 1);
        }
        public bool NextBool()
        {
            return (Next((int)byte.MinValue, (int)byte.MaxValue + 1) > 127) ? true : false;
        }
        public double NextDouble(double Min, double Max)
        {
            return Min + (Max - Min) * NextDouble();
        }
        public decimal NextDecimal(decimal Min, decimal Max)
        {
            return Min + (Max - Min) * (decimal)NextDouble();
        }
        /// <summary>
        /// Returns Minimum or Maximum.
        /// </summary>
        /// <param name="Min"></param>
        /// <param name="Max"></param>
        /// <returns></returns>
        public double NextNumber(double Min, double Max)
        {
            //double d = Math.Truncate(Min + (Max - Min) * random.NextDouble());
            //double rDistance = (Min + Max) * 0.5;
            //rDistance -= rDistance * 0.5;
            if ((int)(NextDouble() * 100) >= 50)
            {
                return Max;
            }
            else
            {
                return Min;
            }
        }
        /// <summary>
        /// Returns Minimum or Maximum.
        /// </summary>
        /// <param name="Min"></param>
        /// <param name="Max"></param>
        /// <returns></returns>
        public double NextNumber(int Min, int Max)
        {
            //double d = Math.Truncate(Min + (Max - Min) * random.NextDouble());
            //double rDistance = (Min + Max) * 0.5;
            //rDistance -= rDistance * 0.5;
            if ((int)(NextDouble() * 100) >= 50)
            {
                return Max;
            }
            else
            {
                return Min;
            }
        }
        public float NextSingle(float Min, float Max)
        {
            return (float)NextDouble(Min, Max);
        }
        public float NextSingle()
        {
            return (float)(NextDouble());
        }
        public short NextShort(short Min, short Max)
        {
            return (short)((Min + (Max - Min)) * NextDouble());
        }
    }
}
