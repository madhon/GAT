namespace Gat
{
    public class AnalyticsPageViewRequest : IAnalyticsPageViewRequest
    {
        private readonly IAnalyticsClient analyticsClient;
        private readonly string page;
        private readonly string title;
        private readonly VariableBucket customVariables;

        internal AnalyticsPageViewRequest(IAnalyticsClient analyticsClient, string page, string title)
        {
            this.analyticsClient = analyticsClient;
            this.page = page;
            this.title = title;
            customVariables = new VariableBucket();
        }

        public void Send() => analyticsClient.SubmitPageView(page, title, customVariables);

        public void SendEvent(string category, string action, string label, string value) => analyticsClient.SubmitEvent(page, title, category, action, label, value, customVariables);

        public void SetCustomVariable(int position, string key, string value) => customVariables.Set(position, key, value);

        public void ClearCustomVariable(int position) => customVariables.Clear(position);
    }
}
