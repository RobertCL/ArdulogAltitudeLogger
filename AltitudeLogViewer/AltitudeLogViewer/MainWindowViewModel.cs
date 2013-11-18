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

		private List<BarometerReading> BarometerData;
		public ObservableCollection<BarometerReading> BarometerChartData { get; set; }
		

		public MainWindowViewModel()
		{
			SelectLogFileCommand = new DelegateCommand(SelectLogFile);
			BarometerChartData = new ObservableCollection<BarometerReading>();
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

			BarometerData = parser.BarometerReadings;

			FillChartData();
		}

		private void FillChartData()
		{
			BarometerChartData.Clear();

			// Make sure we end up with about 500 points on the screen (otherwise charting component is slow)
			int skip = BarometerData.Count() / 500;

			foreach (var r in BarometerData.Where((r, i) => i % skip == 0))
				BarometerChartData.Add(r);
		}
	}
}
