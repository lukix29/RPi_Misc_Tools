using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WiringPi;

namespace NMEA_Parser
{
    class LX29_gpio
    {
        static void Main(string[] args)
        {
            int result = GPIO_Init.WiringPiSetup();

            if (result == -1)
            {
                Console.WriteLine("WiringPi init failed!");
            }
            else
            {
                Console.WriteLine("WiringPi init success!");

                //int R_pin = 27;
                //int G_pin = 28;
                //int B_pin = 29;
                int W_pin = 21;
                //int Fan_pin = 24;
                //soft pins 21 22 24 25 26 27 28 29
                //hardware pins 23

                PWM pwm = new PWM();

                //pwm.Attach(R_pin);
                //pwm.Attach(G_pin);
                //pwm.Attach(B_pin);
                pwm.Attach(W_pin);
                //pwm.Write(W_pin, 1024);
                //pwm.Attach(Fan_pin);
                //string s = "";
                Random rd = new Random();
                while (!Console.KeyAvailable)
                {
                    //if (Console.KeyAvailable)
                    //{
                    //    s += Console.ReadKey().KeyChar;
                    //    if (s.EndsWith("\r") || s.EndsWith("\n"))
                    //    {
                    //        int p = 0;
                    //        if (int.TryParse(s, out p))
                    //        {
                    //            p = Math.Max(0, Math.Min(pwm.PWM_MAX, p));
                    //            pwm.Write(pin, p);
                    //            Console.WriteLine("PWM set to: " + p + "/" + pwm.PWM_MAX);
                    //        }
                    //        s = "";
                    //    }
                    //}
                    Console.WriteLine("Fading In");
                    for (int i = 0; i <= pwm.PWM_MAX; i += 8)
                    {
                        pwm.Write(W_pin, i);
                        if (i % 128 == 0) Console.Write(i + ", ");
                        GPIO.Timing.delay(40);
                    }

                    Console.WriteLine();
                    Console.WriteLine("Fading Out");
                    if (Console.KeyAvailable) break;

                    for (int i = pwm.PWM_MAX; i >= 0; i -= 8)
                    {
                        pwm.Write(W_pin, i);
                        if (i % 128 == 0) Console.Write(i + ", ");
                        GPIO.Timing.delay(40);
                    }

                    Console.WriteLine();
                    Console.Write("10 Random Values: ");
                    if (Console.KeyAvailable) break;

                    for (int i = 10; i >= 0; i--)
                    {
                        int r = rd.Next(0, pwm.PWM_MAX);
                        pwm.Write(W_pin, r);
                        Console.Write(r + ", ");
                        GPIO.Timing.delay(200);
                    }
                    Console.WriteLine();
                }
                pwm.Stop();
            }
        }
        public class PWM
        {
            public const int PWM_CHANNELS = 8;
            public readonly int PWM_MAX = 1024;
            private readonly int PWM_MIN = 4;
            public readonly uint Interval = 1;

            private bool running = false;
            private bool stopped = false;
            private int used_PWM_channels = 0;
            private int[] pins = new int[PWM_CHANNELS];
            private int[] values = new int[PWM_CHANNELS];
            private int[] curVal = new int[PWM_CHANNELS];

            public bool Stopped
            {
                get { return stopped; }
            }
            public int[] Pins
            {
                get { return pins; }
            }
            private int[] Values
            {
                get { return values; }
            }

            public bool Debug
            {
                get;
                set;
            }

            public PWM() : this(1024) { }
            public PWM(int pwm_max)
            {
                PWM_MAX = pwm_max;
                PWM_MIN = PWM_MAX % 10;
                Debug = false;
                for (int i = 0; i < pins.Length; i++)
                {
                    pins[i] = -1;
                    values[i] = 0;
                }
                Interval = (uint)((1000f / (float)PWM_MAX) + 1f);

                new System.Threading.Thread(pwmThread).Start();
                running = true;
            }

            public void Attach(int pin)
            {
                if (used_PWM_channels >= PWM_CHANNELS) return;
                for (int i = 0; i < pins.Length; i++)
                {
                    if (pins[i] < 0)
                    {
                        pins[i] = pin;
                        values[i] = 0;
                        used_PWM_channels++;

                        GPIO.pinMode(i, (int)GPIO.GPIOpinmode.Output);
                        GPIO.Timing.delay(100);
                        GPIO.digitalWrite(i, 0);

                        if (Debug) Console.WriteLine("Pin " + pin + " is now PWM!");
                        return;
                    }
                }
            }
            public void Detach(int pin)
            {
                if (used_PWM_channels == 0) return;
                for (int i = 0; i < pins.Length; i++)
                {
                    if (pins[i] == pin)
                    {
                        pins[i] = -1;
                        values[i] = 0;
                        used_PWM_channels--;

                        GPIO.digitalWrite(i, 0);

                        if (Debug) Console.WriteLine("Pin " + pin + " detached from PWM!");
                        return;
                    }
                }
            }

            public void Stop()
            {
                while (!stopped)
                {
                    running = false;
                }
                for (int i = 0; i < pins.Length; i++)
                {
                    GPIO.digitalWrite(pins[i], 0);
                    pins[i] = -1;
                    values[i] = 0;
                }
            }
            public void Write(int pin, int value)
            {
                for (int i = 0; i < pins.Length; i++)
                {
                    if (pins[i] == pin)
                    {
                        values[i] = Math.Max(0, Math.Min(PWM_MAX, value));
                        curVal[i] = 0;
                        if (Debug) Console.WriteLine("Pin = " + pin + ", Value = " + values[i]);
                        return;
                    }
                }
                Attach(pin);
            }
            private void pwmThread()
            {
                while (running)
                {
                    for (int i = 0; i < pins.Length; i++)
                    {
                        if (pins[i] >= 0)
                        {
                            //locked = true;
                            if (values[i] <= PWM_MIN)
                            {
                                GPIO.digitalWrite(pins[i], 0);
                            }
                            else if (values[i] >= PWM_MAX)
                            {
                                GPIO.digitalWrite(pins[i], 1);
                            }
                            else
                            {
                                curVal[i]++;
                                if (curVal[i] == values[i])
                                {
                                    GPIO.digitalWrite(pins[i], 0);
                                }
                                else if (curVal[i] == PWM_MAX)
                                {
                                    GPIO.digitalWrite(pins[i], 1);
                                    curVal[i] = 0;
                                }
                            }
                        }
                    }
                    // locked = false;
                    GPIO.Timing.delayMicroseconds(Interval);
                }
                stopped = true;
            }
        }
    }
}


namespace WiringPi
{
    /// <summary>
    /// Used to initialise Gordon's library, there's 4 different ways to initialise and we're going to support all 4
    /// </summary>
    public class GPIO_Init
    {
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetup")]     //This is an example of how to call a method / function in a c library from c#
        public static extern int WiringPiSetup();

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupGpio")]
        public static extern int WiringPiSetupGpio();

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupSys")]
        public static extern int WiringPiSetupSys();

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupPhys")]
        public static extern int WiringPiSetupPhys();
    }

    /// <summary>
    /// Used to configure a GPIO pin's direction and provide read & write functions to a GPIO pin
    /// </summary>
    public class GPIO
    {
        [DllImport("libwiringPi.so", EntryPoint = "pinMode")]           //Uses Gpio pin numbers
        public static extern void pinMode(int pin, int mode);

        [DllImport("libwiringPi.so", EntryPoint = "digitalWrite")]      //Uses Gpio pin numbers
        public static extern void digitalWrite(int pin, int value);

        [DllImport("libwiringPi.so", EntryPoint = "digitalWriteByte")]      //Uses Gpio pin numbers
        public static extern void digitalWriteByte(int value);

        [DllImport("libwiringPi.so", EntryPoint = "digitalRead")]           //Uses Gpio pin numbers
        public static extern int digitalRead(int pin);

        [DllImport("libwiringPi.so", EntryPoint = "pullUpDnControl")]         //Uses Gpio pin numbers  
        public static extern void pullUpDnControl(int pin, int pud);

        //This pwm mode cannot be used when using GpioSys mode!!
        [DllImport("libwiringPi.so", EntryPoint = "pwmWrite")]              //Uses Gpio pin numbers
        public static extern void pwmWrite(int pin, int value);

        [DllImport("libwiringPi.so", EntryPoint = "pwmSetMode")]             //Uses Gpio pin numbers
        public static extern void pwmSetMode(int mode);

        [DllImport("libwiringPi.so", EntryPoint = "pwmSetRange")]             //Uses Gpio pin numbers
        public static extern void pwmSetRange(uint range);

        [DllImport("libwiringPi.so", EntryPoint = "pwmSetClock")]             //Uses Gpio pin numbers
        public static extern void pwmSetClock(int divisor);

        [DllImport("libwiringPi.so", EntryPoint = "gpioClockSet")]              //Uses Gpio pin numbers
        public static extern void ClockSetGpio(int pin, int freq);

        public enum GPIOpinmode : int
        {
            Input = 0,
            Output = 1,
            PWMOutput = 2,
            GPIOClock = 3
        }

        public enum GPIOpinvalue
        {
            High = 1,
            Low = 0
        }

        public class SoftPwm
        {
            [DllImport("libwiringPi.so", EntryPoint = "softPwmCreate")]
            public static extern int Create(int pin, int initialValue, int pwmRange);

            [DllImport("libwiringPi.so", EntryPoint = "softPwmWrite")]
            public static extern void Write(int pin, int value);

            [DllImport("libwiringPi.so", EntryPoint = "softPwmStop")]
            public static extern void Stop(int pin);
        }

        /// <summary>
        /// Provides use of the Timing functions such as delays
        /// </summary>
        public class Timing
        {
            [DllImport("libwiringPi.so", EntryPoint = "millis")]
            public static extern uint millis();

            [DllImport("libwiringPi.so", EntryPoint = "micros")]
            public static extern uint micros();

            [DllImport("libwiringPi.so", EntryPoint = "delay")]
            public static extern void delay(uint howLong);

            [DllImport("libwiringPi.so", EntryPoint = "delayMicroseconds")]
            public static extern void delayMicroseconds(uint howLong);
        }

        /// <summary>
        /// Provides access to the Thread priority and interrupts for IO
        /// </summary>
        public class PiThreadInterrupts
        {
            [DllImport("libwiringPi.so", EntryPoint = "piHiPri")]
            public static extern int piHiPri(int priority);

            [DllImport("libwiringPi.so", EntryPoint = "waitForInterrupt")]
            public static extern int waitForInterrupt(int pin, int timeout);

            //This is the C# equivelant to "void (*function)(void))" required by wiringPi to define a callback method
            public delegate void ISRCallback();

            [DllImport("libwiringPi.so", EntryPoint = "wiringPiISR")]
            public static extern int wiringPiISR(int pin, int mode, ISRCallback method);

            public enum InterruptLevels
            {
                INT_EDGE_SETUP = 0,
                INT_EDGE_FALLING = 1,
                INT_EDGE_RISING = 2,
                INT_EDGE_BOTH = 3
            }

            //static extern int piThreadCreate(string name);
        }

        public class MiscFunctions
        {
            [DllImport("libwiringPi.so", EntryPoint = "piBoardRev")]
            public static extern int piBoardRev();

            [DllImport("libwiringPi.so", EntryPoint = "wpiPinToGpio")]
            public static extern int wpiPinToGpio(int wPiPin);

            [DllImport("libwiringPi.so", EntryPoint = "physPinToGpio")]
            public static extern int physPinToGpio(int physPin);

            [DllImport("libwiringPi.so", EntryPoint = "setPadDrive")]
            public static extern int setPadDrive(int group, int value);
        }

        /// <summary>
        /// Provides SPI port functionality
        /// </summary>
        public class SPI
        {
            /// <summary>
            /// Configures the SPI channel specified on the Raspberry Pi
            /// </summary>
            /// <param name="channel">Selects either Channel 0 or 1 for use</param>
            /// <param name="speed">Selects speed, 500,000 to 32,000,000</param>
            /// <returns>-1 for an error, or the linux file descriptor the channel uses</returns>
            [DllImport("libwiringPiSPI.so", EntryPoint = "wiringPiSPISetup")]
            public static extern int wiringPiSPISetup(int channel, int speed);

            /// <summary>
            /// Read and Write data over the SPI bus, don't forget to configure it first
            /// </summary>
            /// <param name="channel">Selects Channel 0 or Channel 1 for this operation</param>
            /// <param name="data">signed byte array pointer which holds the data to send and will then hold the received data</param>
            /// <param name="len">How many bytes to write and read</param>
            /// <returns>-1 for an error, or the linux file descriptor the channel uses</returns>
            [DllImport("libwiringPiSPI.so", EntryPoint = "wiringPiSPIDataRW")]
            public static unsafe extern int wiringPiSPIDataRW(int channel, byte* data, int len);  //char is a signed byte
        }

        /// <summary>
        /// Provides access to the I2C port
        /// </summary>
        public class I2C
        {
            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CSetup")]
            public static extern int wiringPiI2CSetup(int devId);

            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CRead")]
            public static extern int wiringPiI2CRead(int fd);

            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CWrite")]
            public static extern int wiringPiI2CWrite(int fd, int data);

            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CWriteReg8")]
            public static extern int wiringPiI2CWriteReg8(int fd, int reg, int data);

            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CWriteReg16")]
            public static extern int wiringPiI2CWriteReg16(int fd, int reg, int data);

            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CReadReg8")]
            public static extern int wiringPiI2CReadReg8(int fd, int reg);

            [DllImport("libwiringPiI2C.so", EntryPoint = "wiringPiI2CReadReg16")]
            public static extern int wiringPiI2CReadReg16(int fd, int reg);
        }
    }
}