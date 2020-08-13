using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using KrakenX63Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace NzxtKrakenX63Driver
{
    class Program
    {

        Dictionary<string, int> krakenColorChannels = new Dictionary<string, int> {
            {"external", 0b001 },
            {"ring", 0b010 },
            {"logo", 0b100 },
            {"sync", 0b111 },
        };

        /// <summary>
        /// Between 0 and 1, the two colors of the tai chi effect
        /// </summary>
        public static byte GetTaiChiColorIndexAndSpeed(int colorIndex, int speed)
        {
            colorIndex *= 2;
            string concatenated = colorIndex.ToString("X") + speed.ToString();
            return (byte) int.Parse(concatenated, System.Globalization.NumberStyles.HexNumber);
        }

        public static void AppendColorBytes(Color Color, List<byte> commandBytes)
        {
            // Not sure what this does.
            commandBytes.Add(Color.G);
            commandBytes.Add(Color.R);
            commandBytes.Add(Color.B);

            // 8 leds in NZXT Kraken X63 ring
            for (int i = 0; i < 8; i++)
            {
                commandBytes.Add(Convert.ToByte(Color.R));
                commandBytes.Add(Convert.ToByte(Color.G));
                commandBytes.Add(Convert.ToByte(Color.B));
            }
        }

        static Dictionary<string, byte> colorChannels = new Dictionary<string, byte> {
            { "external", 0b001 },
            { "ring", 0b010 },
            { "logo", 0b100 },
            { "sync", 0b111 },
        };

        static Dictionary<byte, byte> staticValue = new Dictionary<byte, byte>
        {
            { 0b001, 40 },
            { 0b010, 8 },
            { 0b100, 1 },
            { 0b111, 40 },
        };

        static Dictionary<string, byte> animationSpeeds = new Dictionary<string, byte> {
            { "slowest", 0x32},
            { "slower", 0x28},
            { "normal", 0x1e},
            { "faster", 0x14},
            { "fastest", 0x0a}
        };

        public static List<byte[]> BuildBytes(Color[] colorPair)
        {
            int colorCount = colorPair.Length;
            List<byte[]> byteCommandQueue = new List<byte[]>();
            for (int colorIndex = 0; colorIndex < colorCount; colorIndex++)
            {
                byte cid = colorChannels["ring"];

                // cmd, cmd, channel (ring only), cmd, color index and speed byte
                // First 5 settings bytes
                List<byte> bytes = new List<byte>(0x41) // 65 bytes
                {
                    //0x02, 0x4c, 0x02, 0x08, GetTaiChiColorIndexAndSpeed(colorIndex, 2)
                    0x2a, 0x04, // op code
                    cid, cid, // address , color channel = ring, cid
                    0x0e, // mval - 'Tai chi' byte code
                    animationSpeeds["slowest"], 0x0, // Speed value
                };

                //
                //self.device.set_color(channel='ring', mode='fixed', colors=iter([[3, 2, 1]]), speed = 'fastest')

                AppendColorBytes(colorPair[colorIndex], bytes);

                // Pad remaining types with 
                //int numToPad = 0x41 - 5 - (9 * 3) - bytes.Count; // 65 - 5 - 9*3
                int numToPad = 3 * (16 - colorCount);
                for (int i = 0; i < numToPad; i++)
                {
                    bytes.Add(0x00);
                }

                // Footer
                bytes.Add(0x00); // backwards byte
                bytes.Add((byte)colorCount); // color count
                bytes.Add(0x05); // mode-related 'tai chi'
                bytes.Add(staticValue[cid]); // mode-related 'tai chi'
                bytes.Add(0x03); // led size

                byteCommandQueue.Add(bytes.ToArray());
            }

            return byteCommandQueue;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            KrakenX63 x63 = new KrakenX63();
            x63.SetColor(
                KrakenX63.KrakenXColorChannel.Ring,
                KrakenX63.ColorEffect.SuperBreathing,
                new Color[] { 
                    Color.FromArgb(0, 255, 255), 
                    Color.FromArgb(255, 0, 180),
                    Color.FromArgb(0, 255, 255),
                    Color.FromArgb(255, 0, 180),
                    Color.FromArgb(0, 255, 255),
                    Color.FromArgb(255, 0, 180),
                    Color.FromArgb(0, 255, 255),
                    Color.FromArgb(255, 0, 180),
                    Color.FromArgb(0, 255, 255),
                },
                KrakenX63.AnimationSpeed.Slowest);


            if (!DeviceList.Local.TryGetHidDevice(out HidDevice krakenX63, vendorID: 0x1e71, productID: 0x2007))
            {
                throw new ArgumentException("Could not find kraken x63");
            }

            Console.WriteLine(krakenX63.DevicePath);
            Console.WriteLine(krakenX63);

            try
            {
                Console.WriteLine(string.Format("Max Lengths: Input {0}, Output {1}, Feature {2}",
                    krakenX63.GetMaxInputReportLength(),
                    krakenX63.GetMaxOutputReportLength(),
                    krakenX63.GetMaxFeatureReportLength()));
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine("Opening device for 20 seconds...");

            HidStream hidStream = null;
            if (krakenX63.TryOpen(out hidStream))
            //if (krakenX63.TryOpen(out DeviceStream deviceStream))
            {
                Console.WriteLine("Opened device.");
                hidStream.ReadTimeout = Timeout.Infinite;

                ReportDescriptor reportDescriptor = krakenX63.GetReportDescriptor();

                Console.WriteLine($"DeviceItems: {reportDescriptor.DeviceItems.Count}");

                List<byte[]> bytesToWrite = BuildBytes(new Color[] { Color.FromArgb(255, 0, 0), Color.FromArgb(0, 0, 255) });

                foreach (byte[] cmds in bytesToWrite)
                {
                    Console.WriteLine(string.Join(" ", cmds.Select(a => a.ToString("X"))));
                    hidStream.Write(cmds);
                }

                //using (hidStream)
                //{
                //    DeviceItem deviceItem = reportDescriptor.DeviceItems.First();

                //    byte[] inputReportBuffer = new byte[krakenX63.GetMaxInputReportLength()];
                //    HidDeviceInputReceiver inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                //    DeviceItemInputParser inputParser = deviceItem.CreateDeviceItemInputParser();

                //    inputReceiver.Start(hidStream);

                //    int startTime = Environment.TickCount;
                //    while (true)
                //    {
                //        if (inputReceiver.WaitHandle.WaitOne(1000))
                //        {
                //            if (!inputReceiver.IsRunning) { break; } // Disconnected?

                //            Report report;
                //            while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                //            {
                //                Console.WriteLine(report.DataItems.First().);
                //                // Parse the report if possible.
                //                // This will return false if (for example) the report applies to a different DeviceItem.
                //                if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                //                {
                //                    WriteDeviceItemInputParserResult(inputParser);
                //                } else
                //                {
                //                    Console.WriteLine("Could not parse report");
                //                }
                //            }
                //        }

                //        uint elapsedTime = (uint)(Environment.TickCount - startTime);
                //        if (elapsedTime >= 20000) { break; } // Stay open for 20 seconds.
                //    }

                    //inputReceiver.Received += (sender, e) =>
                    //{
                    //    Report report;
                    //    while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                    //    {
                    //        // Parse the report if possible.
                    //        // This will return false if (for example) the report applies to a different DeviceItem.
                    //        if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                    //        {
                    //            WriteDeviceItemInputParserResult(inputParser);
                    //        }
                    //    }
                    //};
                    //inputReceiver.Start(hidStream);

                    //Thread.Sleep(20000);
                
                //}

                //Console.WriteLine("Closed device.");
            }
            else
            {
                Console.WriteLine("Failed to open device.");
            }

            Console.WriteLine("Press a key to exit...");
            Console.ReadKey();
        }

        public static void WriteDeviceItemInputParserResult(DeviceItemInputParser parser)
        {
            //while (parser.HasChanged)
            //{
            //    int changedIndex = parser.GetNextChangedIndex();
            //    var previousDataValue = parser.GetPreviousValue(changedIndex);
            //    var dataValue = parser.GetValue(changedIndex);

            //    Console.WriteLine(string.Format("  {0}: {1} -> {2}",
            //                      (Usage)dataValue.Usages.FirstOrDefault(), previousDataValue.GetPhysicalValue(), dataValue.GetPhysicalValue()));
            //}

            if (parser.HasChanged)
            {
                int valueCount = parser.ValueCount;

                for (int valueIndex = 0; valueIndex < valueCount; valueIndex++)
                {
                    var dataValue = parser.GetValue(valueIndex);
                    Console.Write(string.Format("  {0}: {1}",
                                      (Usage)dataValue.Usages.FirstOrDefault(), dataValue.GetPhysicalValue()));

                }

                Console.WriteLine();
            }
        }
    }
}

