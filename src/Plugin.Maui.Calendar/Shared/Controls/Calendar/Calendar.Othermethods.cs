using System.Globalization;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	int GetWeekNumber(DateTime date)
	{
		return Culture.Calendar.GetWeekOfYear(
			date,
			CalendarWeekRule.FirstFourDayWeek,
			Culture.DateTimeFormat.FirstDayOfWeek
		);
	}

	void PrevUnit()
	{
		var oldMonth = DateOnly.FromDateTime(ShownDate);
		ShownDate = CurrentViewLayoutEngine.GetPreviousUnit(ShownDate);
		var newMonth = DateOnly.FromDateTime(ShownDate);

		MonthChanged?.Invoke(this, new MonthChangedEventArgs(oldMonth, newMonth));

		if (MonthChangedCommand?.CanExecute(null) == true)
		{
			MonthChangedCommand.Execute(new MonthChangedEventArgs(oldMonth, newMonth));
		}
	}

	void NextUnit()
	{
		var oldMonth = DateOnly.FromDateTime(ShownDate);
		ShownDate = CurrentViewLayoutEngine.GetNextUnit(ShownDate);
		var newMonth = DateOnly.FromDateTime(ShownDate);

		MonthChanged?.Invoke(this, new MonthChangedEventArgs(oldMonth, newMonth));

		if (MonthChangedCommand?.CanExecute(null) == true)
		{
			MonthChangedCommand.Execute(new MonthChangedEventArgs(oldMonth, newMonth));
		}
	}

	void NextYear(object obj)
	{
		ShownDate = ShownDate.AddYears(1);
	}

	bool CanExecuteNextYear(object obj)
	{
		try
		{
			var maxDate = Culture.Calendar.MaxSupportedDateTime;
			return ShownDate.Year < maxDate.Year;
		}
		catch
		{
			return false;
		}
	}

	void PrevYear(object obj)
	{
		ShownDate = ShownDate.AddYears(-1);
	}

	bool CanExecutePrevYear(object obj)
	{
		try
		{
			var minDate = Culture.Calendar.MinSupportedDateTime;
			return ShownDate.Year > minDate.Year;
		}
		catch
		{
			return false;
		}
	}

	void ToggleCalendarSectionVisibility() => CalendarSectionShown = !CalendarSectionShown;

	void AnimateMonths(double currentValue)
	{
		calendarContainer.HeightRequest = calendarSectionHeight * currentValue;
		calendarContainer.TranslationY = calendarSectionHeight * (currentValue - 1);
		calendarContainer.Opacity = currentValue * currentValue * currentValue;
	}

	public void ClearSelection()
	{
		isSelectingDates = false;
		SelectedDates = null;
		SelectedDate = null;
	}

	void OnSwiped(object sender, SwipedEventArgs e)
	{
		switch (e.Direction)
		{
			case SwipeDirection.Left:
				OnSwipeLeft();
				break;
			case SwipeDirection.Right:
				OnSwipeRight();
				break;
			case SwipeDirection.Up:
				OnSwipeUp();
				break;
			case SwipeDirection.Down:
				OnSwipeDown();
				break;
		}
	}

	void OnSwipeLeft() => SwipedLeft?.Invoke(this, EventArgs.Empty);

	void OnSwipeRight() => SwipedRight?.Invoke(this, EventArgs.Empty);

	void OnSwipeUp() => SwipedUp?.Invoke(this, EventArgs.Empty);

	void OnSwipeDown() => SwipedDown?.Invoke(this, EventArgs.Empty);



	public void InitializeViewLayoutEngine()
	{
		CurrentViewLayoutEngine = new MonthViewEngine(FirstDayOfWeek);
	}


	void RenderLayout()
	{
		CurrentViewLayoutEngine = CalendarLayout switch
		{
			WeekLayout.Week => new WeekViewEngine(1, FirstDayOfWeek),
			WeekLayout.TwoWeek => new WeekViewEngine(2, FirstDayOfWeek),
			_ => new MonthViewEngine(FirstDayOfWeek),
		};

		daysControl.Children.Clear();
		daysControl.RowDefinitions.Clear();
		daysControl.ColumnDefinitions.Clear();

		var generatedLayout = CurrentViewLayoutEngine.GenerateLayout(
			dayViews,
			this,
			nameof(DaysTitleLabelStyle),
			DayTappedCommand
		);

		foreach (var colDef in generatedLayout.ColumnDefinitions)
		{
			daysControl.ColumnDefinitions.Add(colDef);
		}

		for (int i = 0; i < generatedLayout.RowDefinitions.Count; i++)
		{
			if (i > 0) // Day Titles
			{
				var separator = new BoxView { Style = SeparatorStyle };
				Grid.SetRow(separator, i);
				Grid.SetColumnSpan(separator, 7);
				daysControl.Children.Add(separator);
			}

			daysControl.RowDefinitions.Add(generatedLayout.RowDefinitions[i]);
		}

		foreach (var child in generatedLayout.Children)
		{
			daysControl.Children.Add(child);
		}

		UpdateDaysColors();
		UpdateDayTitles();
		UpdateDays();
	}

	internal void AssignIndicatorColors(ref DayModel dayModel)
	{
		dayModel.DayViewSize = DayViewSize;
		dayModel.DayIndicatorViewSize = DayIndicatorViewSize;
		dayModel.DayViewCornerRadius = DayViewCornerRadius;
		dayModel.EventIndicatorStyle = EventIndicatorStyle;
		dayModel.EventIndicatorTextStyle = EventIndicatorTextStyle;
		dayModel.EventIndicatorImageStyle = EventIndicatorImageStyle;

		if (Events.TryGetValue(dayModel.Date, out var dayEventCollection))
		{
			if (dayEventCollection is IPersonalizableDayEvent personalizableDay)
			{
				dayModel.EventIndicatorColor =
					personalizableDay?.EventIndicatorColor ?? EventIndicatorColor;
				dayModel.EventIndicatorSelectedColor =
					personalizableDay?.EventIndicatorSelectedColor
				 ?? personalizableDay?.EventIndicatorColor
				 ?? EventIndicatorSelectedColor;
				dayModel.EventIndicatorTextColor =
					personalizableDay?.EventIndicatorTextColor ?? EventIndicatorTextColor;
				dayModel.EventIndicatorSelectedTextColor =
					personalizableDay?.EventIndicatorSelectedTextColor
				 ?? personalizableDay?.EventIndicatorTextColor
				 ?? EventIndicatorSelectedTextColor;
			}
			if (dayEventCollection is IMultiEventDay multiEventDay)
			{
				dayModel.EventIndicators = multiEventDay.EventIndicators
				.Take(5)
				.Select(e => new EventIndicator
				{
					DotColor = e.DotColor,
					Text = e.Text,
					ImageSource = e.ImageSource,
				})
				.ToList();
			}
			else
			{
				dayModel.EventIndicators =
				[
					new EventIndicator()
					{
						DotColor = dayModel.IsSelected ? dayModel.EventIndicatorSelectedColor : dayModel.EventIndicatorColor,
					}
				];
			}
		}
		else
		{
			dayModel.EventIndicators = [];
		}
	}

	void InitializeSelectionType()
	{
		CurrentSelectionEngine = new SingleSelectionEngine();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Events is EventCollection events)
			{
				events.CollectionChanged -= OnEventsCollectionChanged;
			}
			calendarSectionAnimateHide.Value.Dispose();
			calendarSectionAnimateShow.Value.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
