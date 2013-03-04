namespace Infrito.GoogleAnalytics
{
	public interface IGoogleAnalytics
	{
		void TrackEvent(string category, string action);

		void TrackScreen(string screenName);

		void TrackException(string description, bool isFatal = false);
	}
}
