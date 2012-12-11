using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;

namespace Infrito.GoogleAnalytics
{
	public class AnalyticsService
	{
		private const string BaseAnalyticsUrl = "http://www.google-analytics.com/collect";

		private readonly CookieContainer _cookie;

		private string _trackingId = string.Empty;
		private string _clientId = string.Empty;
		private string _appName = string.Empty;
		private string _appVersion = string.Empty;

		private bool _isStartSession;

		public AnalyticsService()
		{
			_cookie = new CookieContainer();
		}

		#region Properties

		public string TrackingId
		{
			get
			{
				return _trackingId;
			}
			set
			{
				_trackingId = value;
			}
		}

		public string ClientId
		{
			get
			{
				return _clientId;
			}
			set
			{
				_clientId = value;
			}
		}

		public string AppName
		{
			get
			{
				return _appName;
			}
			set
			{
				_appName = value;
			}
		}

		public string AppVersion
		{
			get
			{
				return _appVersion;
			}
			set
			{
				_appVersion = value;
			}
		}

		#endregion

		public void TrackEvent(string category, string action)
		{
			var parameters = new Dictionary<string, string>();
			AddParameters(parameters);

			parameters.Add("t", "event");
			parameters.Add("ec", category);
			parameters.Add("ea", action);

			CreateRequest(parameters);
		}

		public void TrackScreen(string screenName)
		{
			var parameters = new Dictionary<string, string>();
			AddParameters(parameters);

			parameters.Add("t", "appview");
			parameters.Add("cd", screenName);

			CreateRequest(parameters);
		}

		public void TrackException(string description, bool isFatal = false)
		{
			var parameters = new Dictionary<string, string>();
			AddParameters(parameters);

			parameters.Add("t", "exception");
			parameters.Add("exd", description);
			parameters.Add("exf", isFatal ? "1" : "0");

			CreateRequest(parameters);
		}

		public void TrackEndSession()
		{
			if (!_isStartSession)
			{
				return;
			}

			var parameters = new Dictionary<string, string>();
			AddParameters(parameters);
			EndSession(parameters);
			parameters.Add("t", "appview");
			CreateRequest(parameters);
		}

		protected void AddParameters(ICollection<KeyValuePair<string, string>> collection)
		{
			if (!_isStartSession)
			{
				StartSession(collection);
			}

			collection.Add(new KeyValuePair<string, string>("v", "1"));
			collection.Add(new KeyValuePair<string, string>("tid", TrackingId));
			collection.Add(new KeyValuePair<string, string>("cid", ClientId));
			collection.Add(new KeyValuePair<string, string>("an", AppName));
			collection.Add(new KeyValuePair<string, string>("av", AppVersion));
		}

		protected void StartSession(ICollection<KeyValuePair<string, string>> collection)
		{
			collection.Add(new KeyValuePair<string, string>("sc", "start"));
			_isStartSession = true;
		}

		protected void EndSession(ICollection<KeyValuePair<string, string>> collection)
		{
			collection.Add(new KeyValuePair<string, string>("sc", "end"));
		}

		protected async void CreateRequest(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}

			var sb = new StringBuilder();
			var isFirstParam = true;
			foreach (var pair in parameters)
			{
				var key = Uri.EscapeUriString(pair.Key);
				var value = Uri.EscapeUriString(pair.Value);

				if (isFirstParam)
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", key, value);

					isFirstParam = false;
				}
				else
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, "&{0}={1}", key, value);
				}
			}

			var request = WebRequest.CreateHttp(BaseAnalyticsUrl);
			request.Method = "POST";
			request.CookieContainer = _cookie;

			var requestStream = await request.GetRequestStreamAsync();
			using (var writer = new StreamWriter(requestStream))
			{
				writer.Write(sb.ToString());
			}
			try
			{
				var response = await request.GetResponseAsync();
			}
			catch (Exception)
			{
			}
		}
	}
}
