using System;

namespace AltitudeLogViewer
{
	class ReadingBase
	{
		public ReadingBase()
		{
			LogStartTime = DateTime.MinValue;
		}

		public int TimeStamp { get; set; }
		public int SecondsFromStart { get { return TimeStamp / 1000; } set { } }
		public DateTime TimeFromStart { get { return LogStartTime.AddMilliseconds(TimeStamp); } }
		public DateTime LogStartTime { get; set; }
	}
}
