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
		public List<GpsGgaReading> GpsGgaReadings { get; private set; }
		public TimeSync TimeSync { get; private set; }

		public LogParser(string fileName)
		{
			Parse(fileName);
		}

		void Parse(string fileName)
		{
			var lines = File.ReadAllLines(fileName);

			var bmpLines = lines.Where(l => l.StartsWith("BMP"));
			var gpsLines = lines.Where(l => l.StartsWith("GPS") && l.Contains("$"));

			File.WriteAllLines(fileName + ".baro", bmpLines);
			File.WriteAllLines(fileName + ".gps", gpsLines.Select(l => l.Substring(l.IndexOf('$'))));

			// Convert barometer data into something we can work with
			BarometerReadings = bmpLines.ToList().ConvertAll(new Converter<string, BarometerReading>(s => BarometerReading.FromLogFileLine(s)));

			// Convert gps data into something we can work with
			GpsGgaReadings = new List<GpsGgaReading>();
			var haveFix = false;
			foreach (var line in gpsLines)
			{
				try
				{
					var parts = line.Split(',');
					if (haveFix && parts[2] == "$GPRMC")
						ParseGprmc(parts);
					else if (parts[2] == "$GPGGA")
					{
						var r = GpsGgaReading.FromLogLine(line);
						GpsGgaReadings.Add(r);
						if (r.Fix == GpsGgaReading.FixQuality.GPS)
							haveFix = true;
					}
				}
				catch (Exception)
				{
					// TODO: report issues when parsing lines
				}
			}

			// Fix up times (hopefully we have got something from the GPS
			if (null == TimeSync)
				TimeSync = new TimeSync { Date = DateTime.Today };
		
			BarometerReadings.ForEach(r => r.LogStartTime = TimeSync.ZeroDate);
			GpsGgaReadings.ForEach(r => r.LogStartTime = TimeSync.ZeroDate);
		}

		/// <summary>
		/// GPRMC sentences contain date/time information.  We can correlate this with
		/// the timestamps from the Arduino clock to work out the real time of the 
		/// barometer readings.
		/// </summary>
		private void ParseGprmc(string[] parts)
		{
			/*
				$GPRMC,174439.000,A,5140.4799,N,00446.6480,W,0.00,223.26,100712,,,A*7D
				
				Recommended minimum specific GPS/Transit data

				eg1. $GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62
				eg2. $GPRMC,225446,A,4916.45,N,12311.12,W,000.5,054.7,191194,020.3,E*68


						   225446       Time of fix 22:54:46 UTC
						   A            Navigation receiver warning A = OK, V = warning
						   4916.45,N    Latitude 49 deg. 16.45 min North
						   12311.12,W   Longitude 123 deg. 11.12 min West
						   000.5        Speed over ground, Knots
						   054.7        Course Made Good, True
						   191194       Date of fix  19 November 1994
						   020.3,E      Magnetic variation 20.3 deg East
						   *68          mandatory checksum
			*/
			var t = new TimeSync();

			t.TimeStamp = int.Parse(parts[1]);

			int year = 2000 + int.Parse(parts[11].Substring(4, 2));
			int month = int.Parse(parts[11].Substring(2, 2));
			int day = int.Parse(parts[11].Substring(0, 2));
			int hour = int.Parse(parts[3].Substring(0, 2));
			int minute = int.Parse(parts[3].Substring(2, 2));
			int second = int.Parse(parts[3].Substring(4, 2));
			t.Date = new DateTime(year, month, day, hour, minute, second);

			if (null == TimeSync)
				TimeSync = t;
			else
				if (!t.IsCompatibleWith(TimeSync))
					throw new ApplicationException("timesync issue");
		}
	}

	class TimeSync
	{
		public int TimeStamp { get; set; }
		public DateTime Date { get; set; }

		public DateTime ZeroDate { get { return Date.AddMilliseconds(-TimeStamp); } }

		public bool IsCompatibleWith(TimeSync t)
		{
			return Math.Abs((this.ZeroDate - t.ZeroDate).TotalSeconds) < 10;
		}
	}
}
