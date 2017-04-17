namespace Piwik.Tracker.Web.Samples
{
    using System.Net.Http;

    public class Singleton
    {
        public static readonly HttpClient HttpClient = new HttpClient();
    }
}