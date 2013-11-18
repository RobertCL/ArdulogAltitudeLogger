using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AltitudeLogViewer
{
	class LogParser
	{
		public List<BarometerReading> BarometerReadings { get; private set; }

		public LogParser(string fileName)
		{
			Parse(fileName);
		}

		void Parse(string fileName)
		{
			var lines = File.ReadAllLines(fileName);

			var bmpLines = lines.Where(l => l.StartsWith("BMP"));
			var gpsLines = lines.Where(l => l.StartsWith("GPS"));

			BarometerReadings = bmpLines.ToList().ConvertAll(new Converter<string, BarometerReading>(s => BarometerReading.FromLogFileLine(s)));
			
		}
	}
}
