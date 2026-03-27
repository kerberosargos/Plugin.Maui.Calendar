using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

sealed partial class DayModel : ObservableObject
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	[NotifyPropertyChangedFor(nameof(OutlineColor))]
	DateTime date;

	[ObservableProperty]
	string day;

	[ObservableProperty]
	Thickness dayViewBorderMargin = new(0, 0, 0, 0);

	[ObservableProperty]
	double dayViewSize;

	[ObservableProperty]
	double dayIndicatorViewSize;

	[ObservableProperty]
	float dayViewCornerRadius;

	[ObservableProperty]
	Style daysLabelStyle = DefaultStyles.DefaultLabelStyle;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundFullEventColor)
	)]
	bool isEventDayBackgroundColorActive = false;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundFullEventColor)
	)]
	Color eventDayBackgroundColor;

	[ObservableProperty]
	Style eventIndicatorDotStyle = DefaultStyles.DefaultEventIndicatorDotStyle;

	[ObservableProperty]
	Style eventIndicatorTextContainerStyle = DefaultStyles.DefaultEventIndicatorTextContainerStyle;

	[ObservableProperty]
	Style eventIndicatorTextStyle = DefaultStyles.DefaultEventIndicatorTextStyle;

	[ObservableProperty]
	Style eventIndicatorImageStyle = DefaultStyles.DefaultEventIndicatorImageStyle;

	[ObservableProperty]
	ICommand dayTappedCommand;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundColor), 
		nameof(OutlineColor), 
		nameof(BackgroundFullEventColor)
	)]
	[NotifyPropertyChangedFor(
		nameof(DayRowIndex), 
		nameof(EventIndicatorRowIndex),
		nameof(BackgroundFullEventColor)
	)] 
	bool hasEvents;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(TextColor), 
		nameof(IsVisible), 
		nameof(IsControlVisible)
	)]
	bool isThisMonth;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(TextColor),
		nameof(BackgroundColor),
		nameof(OutlineColor),
		nameof(BackgroundFullEventColor)
	)]
	bool isSelected;

	[ObservableProperty]
	bool allowDeselect;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsVisible))]
	bool otherMonthIsVisible;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsControlVisible))]
	bool otherMonthWeekIsVisible;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	bool isDisabled;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color selectedTextColor = Colors.White;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color selectedTodayTextColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color otherMonthColor = Colors.Silver;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color otherMonthSelectedColor = Colors.Gray;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color weekendDayColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color deselectedTextColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	Color selectedBackgroundColor = Color.FromArgb("#2196F3");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	Color deselectedBackgroundColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundColor),
		nameof(DayRowIndex), 
		nameof(EventIndicatorRowIndex)
	)]
	EventIndicatorType eventIndicatorType = EventIndicatorType.Bottom;

	[ObservableProperty]
	List<EventIndicator> eventIndicators;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(OutlineColor))]
	Color todayOutlineColor = Color.FromArgb("#FF4081");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color todayTextColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	Color todayFillColor = Colors.Transparent;

	[ObservableProperty]
	Color disabledColor = Color.FromArgb("#ECECEC");

	public int DayRowIndex => (HasEvents && EventIndicatorType == EventIndicatorType.Top) ? 1 : 0;

	public int EventIndicatorRowIndex => (EventIndicatorType == EventIndicatorType.Top) ? 0 : 1;

	public Color BackgroundFullEventColor => HasEvents && IsEventDayBackgroundColorActive ? EventDayBackgroundColor : Colors.Transparent;

	public Color OutlineColor => IsToday && !IsSelected ? TodayOutlineColor : Colors.Transparent;

	public Color BackgroundColor
	{
		get
		{
			if (!IsVisible || IsDisabled)
			{
				return DeselectedBackgroundColor;
			}

			return (IsSelected, IsToday) switch
			{
				(true, _) => SelectedBackgroundColor,
				(false, true) => TodayFillColor,
				(_, _) => DeselectedBackgroundColor
			};
		}
	}

	public Color TextColor
	{
		get
		{
			if (!IsVisible)
			{
				return OtherMonthColor;
			}

			return (IsDisabled, IsSelected, HasEvents, IsThisMonth, IsToday, IsWeekend) switch
			{
				(true, _, _, _, _, _) => DisabledColor,
				(false, true, false, true, true, _)
					=> SelectedTodayTextColor == Colors.Transparent
						? SelectedTextColor
						: SelectedTodayTextColor,
				(false, true, false, true, false, _) => SelectedTextColor,
				(false, false, _, false, _, _) => OtherMonthColor,
				(false, true, _, false, _, _) => OtherMonthSelectedColor,
				(false, false, false, true, true, _)
					=> TodayTextColor == Colors.Transparent ? DeselectedTextColor : TodayTextColor,
				(false, _, _, _, _, true) => WeekendDayColor,
				(false, false, false, true, false, _) => DeselectedTextColor,
			};
		}
	}

	public bool IsVisible => IsThisMonth || OtherMonthIsVisible;

	public bool IsControlVisible => IsThisMonth || OtherMonthWeekIsVisible;

	bool IsToday => Date.Date == DateTime.Today;

	public bool IsWeekend => (Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday) && WeekendDayColor != Colors.Transparent;
}
