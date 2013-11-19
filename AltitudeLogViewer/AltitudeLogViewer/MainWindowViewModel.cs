using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AltitudeLogViewer
{
	class MainWindowViewModel : BaseViewModel
	{
		public DelegateCommand SelectLogFileCommand { get; private set; }

		private string logFileName;
		public string LogFileName 
		{
			get { return logFileName; }
			set
			{
				if (value != logFileName)
				{
					logFileName = value;
					OnPropertyChanged("LogFileName");
					LoadLogFile();
				}
			}
		}

		private List<BarometerReading> barometerData;
		public ObservableCollection<BarometerReading> BarometerChartData { get; set; }
		private List<GpsGgaReading> gpsData;
		public ObservableCollection<GpsGgaReading> GpsChartData { get; set; }

		public MainWindowViewModel()
		{
			SelectLogFileCommand = new DelegateCommand(SelectLogFile);
			BarometerChartData = new ObservableCollection<BarometerReading>();
			GpsChartData = new ObservableCollection<GpsGgaReading>();
		}

		private void SelectLogFile(object obj)
		{
			var d = new Microsoft.Win32.OpenFileDialog();
			var result = d.ShowDialog(null);
			if (result.HasValue && result.Value)
				LogFileName = d.FileName;
		}

		private void LoadLogFile()
		{
			var parser = new LogParser(LogFileName);

			barometerData = parser.BarometerReadings;
			gpsData = parser.GpsGgaReadings;

			FillChartData();
		}

		private void FillChartData()
		{
			BarometerChartData.Clear();

			// Make sure we end up with about 500 points on the screen (otherwise charting component is slow)
			int skip = barometerData.Count() / 500;

			foreach (var r in barometerData.Where((r, i) => i % skip == 0))
				BarometerChartData.Add(r);

			foreach (var r in gpsData.Where((r, i) => i % skip == 0))
				GpsChartData.Add(r);
		}
	}
}
