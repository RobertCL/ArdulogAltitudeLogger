
using System;
namespace AltitudeLogViewer
{
	class BarometerReading
	{
		public BarometerReading()
		{
			LogStartTime = DateTime.MinValue;
		}

		public int TimeStamp { get; set; }
		public int SecondsFromStart { get { return TimeStamp / 1000; } set { } }
		public DateTime TimeFromStart { get { return LogStartTime.AddMilliseconds(TimeStamp); } }
		public DateTime LogStartTime { get; set; }
		public decimal Temperature { get; set; }
		public decimal Pressure { get; set; }
		public decimal Altitude { get; set; }

		public static BarometerReading FromLogFileLine(string line)
		{
			var parts = line.Split(',');

			var reading = new BarometerReading
			{
				TimeStamp = int.Parse(parts[1]),
				Temperature = decimal.Parse(parts[2]) / 10.0M,
				Pressure = decimal.Parse(parts[3]),
				Altitude = decimal.Parse(parts[4])
			};

			return reading;
		}
	}
}
