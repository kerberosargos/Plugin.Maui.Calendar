using CommunityToolkit.Mvvm.ComponentModel;

namespace Plugin.Maui.Calendar.Models;

public sealed partial class EventIndicator : ObservableObject
{
	[ObservableProperty]
	Color dotColor;

	[ObservableProperty]
	ImageSource imageSource;

	[ObservableProperty]
	string text;
}
