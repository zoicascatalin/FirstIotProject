using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubMonitoring.Models
{
    public class DeviceLastValuesDTO
    {
        public string DeviceId { get; set; }
        public decimal Average { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
