﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNPlugin {
    interface FNResourceSuppliable {
        void receiveFNResource(float power_supplied,String resourcename);
        float consumeFNResource(double power_to_consume, String resourcename);
        float consumeFNResource(float power_to_consume, String resourcename);
    }
}
