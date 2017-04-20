using System.Globalization;

namespace Piwik.Tracker
{
    using System;

    internal static class DateTimeUtils
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ConvertToUnixTime(DateTimeOffset date)
        {
            return (date - UnixEpoch).TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }
    }
}
