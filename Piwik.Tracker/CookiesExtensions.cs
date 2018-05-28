using System;
#if NETSTANDARD1_4
using Microsoft.AspNetCore.Http;
#else
using System.Web;
#endif

namespace Piwik.Tracker 
{
    internal static class CookiesExtensions 
    {
#if NETSTANDARD1_4
        public static void Add(this IResponseCookies cookies, string cookieName, string cookieValue,
                DateTime expirationUtc, string domain, string path) 
        {
            cookies.Append(cookieName, cookieValue, new CookieOptions() { Expires = expirationUtc, Domain = domain, Path = path });
        }
#else
        public static void Add(this HttpCookieCollection cookies, string cookieName, string cookieValue,
                DateTime expirationUtc, string domain, string path) 
        {
            cookies.Add(new HttpCookie(cookieName, cookieValue) { Expires = expirationUtc, Domain = domain, Path = path });
        }
#endif
    }
}