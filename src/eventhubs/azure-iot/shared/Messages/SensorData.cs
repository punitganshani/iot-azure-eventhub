using System;

namespace shared.Messages
{
    public class SensorData
    {
        public decimal MeasuredValue { get; set; }
        public string DeviceName { get; set; }
        public DateTime RecordedAt { get; set; }
        public string SensorType { get; set; }
        public string MessageId { get; set; }
    }
}
