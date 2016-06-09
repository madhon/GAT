namespace Gat
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class AnalyticsClient : IAnalyticsClient
    {
        private readonly VariableBucket sessionVariables;

        private const string ReferralSource = "(direct)";
        private const string Medium = "(none)";
        private const string Campaign = "(direct)";
        private string domain;

        /// <summary>
        /// Use for non persisted user session mode (Does not guarantee uniqueness of users)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="trackingCode"></param>
        public AnalyticsClient(string domain, string trackingCode)
            : this(domain, trackingCode, new Random(DateTime.Now.Millisecond).Next(1000000000), 1, null)
        {
        }

        /// <summary>
        /// Use for persisted user session mode can guarantee uniqueness by machine+user
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="trackingCode"></param>
        /// <param name="randomNumber">Random number that was generated on first launch</param>
        /// <param name="recentVisitCount">Recent visit count, must be incremented before passing in</param>
        /// <param name="firstVisitTimestamp">Timestamp for first session</param>
        public AnalyticsClient(string domain, string trackingCode, int randomNumber, int recentVisitCount, DateTime? firstVisitTimestamp)
        {
            sessionVariables = new VariableBucket();
            Timestamp = ConvertToUnixTimestamp(DateTime.Now).ToString();
            Domain = domain;
            RandomNumber = randomNumber.ToString();
            TrackingCode = trackingCode;
            FirstSessionTimestamp = firstVisitTimestamp.HasValue ?
                ConvertToUnixTimestamp(firstVisitTimestamp.Value).ToString() : Timestamp;
            VisitCount = recentVisitCount;
        }

        public string Domain
        {
            get { return domain; }
            set
            {
                domain = value;
                DomainHash = CalculateDomainHash(value);
            }
        }
        public string TrackingCode { get; set; }
        public string Timestamp { get; set; }
        public string FirstSessionTimestamp { get; set; }
        public string RandomNumber { get; set; }
        public int VisitCount { get; set; }

        public int DomainHash { get; private set; }

        public string CookieString => GetCookieString();

        public void SubmitPageView(string page, string title, VariableBucket pageVariables)
        {
            var client = CreateBrowser(page, title);

            var variables = sessionVariables.MergeWith(pageVariables);

            if (variables.Any())
                client.QueryString["utme"] = variables.ToUtme();

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    client.DownloadData(new Uri("__utm.gif", UriKind.Relative));
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {

                }
            });
        }

        public void SubmitEvent(string page, string title, string category, string action, string label, string value, VariableBucket pageVariables)
        {
            var client = CreateBrowser(page, title);

            client.QueryString["utmt"] = "event";
            client.QueryString["utme"] = FormatUtme(category, action, label, value);

            var variables = sessionVariables.MergeWith(pageVariables);

            if (variables.Any())
            {
                //    client.QueryString["utme"] += variables.ToUtme();    
            }

            Task.Run(() => client.DownloadDataAsync(new Uri("__utm.gif", UriKind.Relative)));

            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    client.DownloadDataAsync(new Uri("__utm.gif", UriKind.Relative));
            //});
        }

        public void SetCustomVariable(int position, string key, string value)
        {
            sessionVariables.Set(position, key, value);
        }

        public void ClearCustomVariable(int position)
        {
            sessionVariables.Clear(position);
        }

        private static string GetDefaultUserAgent()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"GAT v{version.Major.ToString()}.{version.Minor.ToString()}";
        }

        private WebClient CreateBrowser(string page, string title)
        {
            Random randomNumber = new Random();

            var proxy = WebRequest.GetSystemWebProxy();
            WebClient client = new WebClient { Proxy = proxy };

            client.Headers.Add(HttpRequestHeader.UserAgent, GetDefaultUserAgent());
            client.BaseAddress = "http://www.google-analytics.com/";

            client.QueryString["utmhn"] = Domain;
            client.QueryString["utmcs"] = "UTF-8";
            client.QueryString["utmsr"] = "1280x800";
            client.QueryString["utmvp"] = "1280x800";
            client.QueryString["utmsc"] = "24-bit";
            client.QueryString["utmul"] = "en-us";
            client.QueryString["utmdt"] = title;
            client.QueryString["utmhid"] = randomNumber.Next(1000000000).ToString();
            client.QueryString["utmac"] = TrackingCode;
            client.QueryString["utmn"] = randomNumber.Next(1000000000).ToString();
            client.QueryString["utmr"] = "-";
            client.QueryString["utmp"] = page;
            client.QueryString["utmwv"] = "5.3.5";
            client.QueryString["utmcc"] = CookieString;

            return client;
        }

        private static string FormatUtme(string category, string action, string label, string value)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("5({0}*{1}", EncodeUtmePart(category), EncodeUtmePart(action));

            if (!string.IsNullOrEmpty(label))
                builder.AppendFormat("*{0}", EncodeUtmePart(label));

            builder.Append(")");

            if (!string.IsNullOrEmpty(value))
                builder.AppendFormat("({0})", EncodeUtmePart(value));

            return builder.ToString();
        }

        internal static string EncodeUtmePart(string part)
        {
            return part.Replace("'", "'0").Replace(")", "'1").Replace("*", "'2");
        }

        private static int ConvertToUnixTimestamp(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (int)span.TotalSeconds;
        }

        private int CalculateDomainHash(string domainName)
        {
            int h;

            var a = 0;
            for (h = domainName.Length - 1; h >= 0; h--)
            {
                var chrCharacter = char.Parse(domainName.Substring(h, 1));
                var intCharacter = (int)chrCharacter;
                a = (a << 6 & 268435455) + intCharacter + (intCharacter << 14);
                var c = a & 266338304;
                a = c != 0 ? a ^ c >> 21 : a;
            }

            return a;
        }

        private string GetCookieString()
        {
            string utma = $"{DomainHash.ToString()}.{RandomNumber}.{FirstSessionTimestamp}.{Timestamp}.{Timestamp}.{VisitCount.ToString()}"; // total visit count

            //referral information
            string utmz =
                $"{DomainHash.ToString()}.{Timestamp}.{"1"}.{"1"}.utmcsr={ReferralSource}|utmccn={Campaign}|utmcmd={Medium}";

            string utmcc = Uri.EscapeDataString($"__utma={utma};+__utmz={utmz};");

            return (utmcc);
        }
    }
}
