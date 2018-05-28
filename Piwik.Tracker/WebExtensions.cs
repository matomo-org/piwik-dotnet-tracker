namespace Piwik.Tracker
{
    internal static class WebExtensions
    {
        public static string UrlDecode(this string value)
        {
#if NETSTANDARD1_4
            return System.Net.WebUtility.UrlDecode(value);
#else
            return System.Web.HttpUtility.UrlDecode(value);
#endif
        }

        public static string UrlEncode(this string value)
        {
#if NETSTANDARD1_4
            return System.Net.WebUtility.UrlEncode(value);
#else
            return System.Web.HttpUtility.UrlEncode(value);
#endif
        }
    }
}