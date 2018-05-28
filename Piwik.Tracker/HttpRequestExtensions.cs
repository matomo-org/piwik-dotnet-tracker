using System;
using System.Linq;
using System.Text;
#if NETSTANDARD1_4
using Microsoft.AspNetCore.Http;
#else
using System.Web;
#endif

namespace Piwik.Tracker 
{
    internal static class HttpRequestExtensions
    {
        public static Uri GetUrl(this HttpRequest request)
        {
#if NETSTANDARD1_4
            if (request == null)
                throw new ArgumentNullException("request");
            if (string.IsNullOrWhiteSpace(request.Scheme))
                throw new ArgumentException("HttpRequest Scheme not specified");
            if (!request.Host.HasValue)
                throw new ArgumentException("HttpRequest Host not specified");
            var sb = new StringBuilder();
            sb.Append(request.Scheme).Append("://").Append(request.Host);
            if (request.Path.HasValue)
                sb.Append(request.Path.Value);
            if (request.QueryString.HasValue)
                sb.Append(request.QueryString.ToString());
            return new Uri(sb.ToString());
#else
            return request.Url;
#endif
        }

#if NETSTANDARD1_4
        public static Uri GetUrlReferrer(this HttpRequest request)
        {
            var s = request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(s)) {
                return null;
            }
            try {
                return s.IndexOf("://", StringComparison.Ordinal) < 0 ? new Uri(request.GetUrl(), s) : new Uri(s);
            } catch {
                return null;
            }
        }
#else
        public static Uri GetUrlReferrer(this HttpRequest request) 
        {
            return request.UrlReferrer;
        }
#endif


#if NETSTANDARD1_4
        public static string GetUserAgent(this HttpRequest request) 
        {
            return request?.Headers["User-Agent"].ToString();
        }
#else
        public static string GetUserAgent(this HttpRequest request) 
        {
            return request?.UserAgent;
        }
#endif

#if NETSTANDARD1_4
        public static string GetFirstUserLanguage(this HttpRequest request) 
        {
            return request?.Headers["Accept-Language"].FirstOrDefault();
        }
#else
        public static string GetFirstUserLanguage(this HttpRequest request) 
        {
            return request?.UserLanguages?.FirstOrDefault();
        }
#endif

#if NETSTANDARD1_4
        public static string GetUserHostAddress(this HttpRequest request) 
        {
            return request.HttpContext.Connection.RemoteIpAddress.ToString();
        }
#else
        public static string GetUserHostAddress(this HttpRequest request) 
        {
            return request?.UserHostAddress;
        }
#endif
    }
}