namespace Gat
{
    using System;

    public class AnalyticsSession : IAnalyticsSession
    {
        private readonly AnalyticsClient analyticsClient;

        public AnalyticsSession(string domain, string trackingCode)
        {
            analyticsClient = new AnalyticsClient(domain, trackingCode);
        }

        public AnalyticsSession(string domain, string trackingCode, int userRandomNumber, int visitCount, DateTime? firstVisitTimeStamp)
        {
            analyticsClient = new AnalyticsClient(domain, trackingCode, userRandomNumber, visitCount, firstVisitTimeStamp);
        }

        public IAnalyticsPageViewRequest CreatePageViewRequest(string page, string title) => new AnalyticsPageViewRequest(analyticsClient, page, title);

        public void SetCustomVariable(int position, string key, string value) => analyticsClient.SetCustomVariable(position, key, value);
    }
}
