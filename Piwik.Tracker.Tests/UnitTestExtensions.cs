using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Piwik.Tracker.Tests
{
    internal static class UnitTestExtensions
    {
        public static string GetAuthorityAndPath(this Uri uri)
        {
            return uri?.GetLeftPart(UriPartial.Path) ?? string.Empty;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this NameValueCollection nameValueCollection)
        {
            return nameValueCollection.AllKeys.Select(k => new KeyValuePair<string, string>(k, nameValueCollection[(string)k]));
        }
    }
}