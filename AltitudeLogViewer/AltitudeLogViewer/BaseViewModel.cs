using System.ComponentModel;

namespace AltitudeLogViewer
{
	class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (PropertyChanged != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
