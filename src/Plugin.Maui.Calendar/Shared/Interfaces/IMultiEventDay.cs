using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Interfaces;

public interface IMultiEventDay
{
    IReadOnlyList<EventIndicator> EventIndicators { get; }
}