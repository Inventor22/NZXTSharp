﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NZXTSharp.Devices {
    enum SerialDeviceID {

        Unknown = -1,
        
        // Kraken Devices
        KrakenM22 = 0x1715,
        KrakenX   = 0x170e,

        // Hue Devices
        Hue2       = 0x2001,
        HueAmbient = 0x2002,

        // Grid Devices
        GridV2 = -1,
        GridV3 = 0x1711,

        // Motherboards
        N7      = 0x1713,
        N7_Z390 = 0x2005,

        // Misc
        H7Lumi      = 0x1712,
        SmartDevice = 0x1714,
    }
}
