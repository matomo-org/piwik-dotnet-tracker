#if NETSTANDARD1_4
//  .Net.Core dropped the JavaScriptSerializer in favor of the widely used Newtonsoft.Json package.
    using Newtonsoft.Json;
#else

using System.Web.Script.Serialization;

#endif

namespace Piwik.Tracker
{
    internal static class SerializerExtensions
    {
        public static TReturn Deserialize<TReturn>(this string value)
        {
#if NETSTANDARD1_4
            return JsonConvert.DeserializeObject<TReturn>(value);
#else
            return new JavaScriptSerializer().Deserialize<TReturn>(value);
#endif
        }

        public static string Serialize(this object value)
        {
#if NETSTANDARD1_4
            return JsonConvert.SerializeObject(value);
#else
            return new JavaScriptSerializer().Serialize(value);
#endif
        }
    }
}