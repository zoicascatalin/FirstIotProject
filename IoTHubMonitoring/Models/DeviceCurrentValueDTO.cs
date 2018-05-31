﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubMonitoring.Models
{
    public class DeviceCurrentValueDTO
    {
        public string DeviceId { get; set; }
        public decimal CurrentValue { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
