using System.Linq;
using System.Windows;

namespace AltitudeLogViewer
{
	public partial class MainWindow : Window
	{
		MainWindowViewModel model;

		public MainWindow()
		{
			InitializeComponent();
			model = new MainWindowViewModel();
			DataContext = model;

			
		}

		private void SelectLogFileButton_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Link;
			else
				e.Effects = DragDropEffects.None;
		}

		private void SelectLogFileButton_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (files.Count() > 0)
					model.LogFileName = files[0];
			}
		}
	}
}
