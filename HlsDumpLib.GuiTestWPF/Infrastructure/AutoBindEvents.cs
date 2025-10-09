using System.Windows;
using System.Windows.Controls;

namespace HlsDumpLib.GuiTestWPF
{
	public static class AutoBindEvents
	{
		public static readonly DependencyProperty IsAutoBindEventProperty =
			DependencyProperty.RegisterAttached("IsAutoBindEvent", typeof(bool),
				typeof(AutoBindEvents), new PropertyMetadata(false, OnPropertyChanged));

		public static bool GetIsAutoBindEvent(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsAutoBindEventProperty);
		}

		public static void SetIsAutoBindEvent(DependencyObject obj, bool value)
		{
			obj.SetValue(IsAutoBindEventProperty, value);
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is Window window)
			{
				window.DataContextChanged += (s, args) =>
					(window.DataContext as ViewModelMainWindow).Initialize(sender);
			}
			if (sender is ListView)
			{
				((sender as ListView).DataContext as ViewModelMainWindow).Initialize(sender);
			}
		}
	}
}
