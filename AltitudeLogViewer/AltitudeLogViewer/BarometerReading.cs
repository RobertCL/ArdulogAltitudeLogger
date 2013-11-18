
namespace AltitudeLogViewer
{
	class BarometerReading
	{
		public int TimeStamp { get; set; }
		public int SecondsFromStart { get { return TimeStamp / 1000; } set { } }
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
