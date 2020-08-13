using NZXTSharp;
using NZXTSharp.KrakenX;
using System;

namespace NzxtTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            KrakenX aio = new KrakenX();
            Console.WriteLine(
                $"X63 device Id: {aio.DeviceID}" +
                $"Firmware version: {aio.FirmwareVersion}");

            //TaiChi taichi = new TaiChi(new Color[] { new Color(255, 0, 0 /*80*/), new Color(0, 255, 180) }, Speed: 2);
            Fixed green = new Fixed(new Color(0, 255, 0));

            aio.ApplyEffect(aio.Ring, green);

            Console.WriteLine("Made the ring green!");
        }
    }
}
