using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Interfaces;

/// <summary>
/// Interface for customize DayEvent colors
/// </summary>
public interface IPersonalizableDayEvent
{
    #region PersonalizableProperties
  
	EventIndicator EventIndicator { get; set; }

	#endregion
}
