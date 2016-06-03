﻿namespace Gat.WPFSample
{
    using System;

    public class AnalyticsHelper
    {
        private IAnalyticsSession _analyticsSession;

        private static AnalyticsHelper _Current;
        public static AnalyticsHelper Current
        {
            get
            {
                if (_Current == null)
                    Initialize();
                return _Current;
            }
        }

        public bool AnalyticsEnabled { get; set; }
        public bool IsTrial { get; set; }

        /// <summary>
        /// This method displays how to use non persistent mode of Gat, each time you run will make a new unique user on GA dashboard
        /// For persistent mode, store first visit date, visit count (increment each time), and random number somewhere like app settings
        /// Use Constructor overload for Analytics sessions 
        /// Replace UA code with your Google Analytics Tracking code
        /// </summary>
        private static void Initialize() => _Current = new AnalyticsHelper { _analyticsSession = new AnalyticsSession("", "UA-XXXXXXX-X") };

        public void LogEvent(string url, string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(url))
                throw new InvalidOperationException("Parameters cannot contain empty values");
            try
            {
                var request = _analyticsSession.CreatePageViewRequest(url, eventName);
                request.Send();
            }
            catch
            {
                // Keep silent exceptions for robust scenarios
            }
        }
    }
}
