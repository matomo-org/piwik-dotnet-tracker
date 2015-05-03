#region license
// http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later
#endregion

using System;

/// <summary>
/// Piwik - Open source web analytics
/// For more information, see http://piwik.org
/// </summary>
namespace Piwik.Tracker
{
    /// <summary>
    /// Represents visit attribution information: referrer URL, referrer timestamp, campaign name & keyword.
    /// Used in the context of goal converions to attribute the right information.
    /// </summary>
    public class AttributionInfo
    {

        public string campaignName { get; set; }
        public string campaignKeyword { get; set; }

        /// <summary>
        /// Timestamp at which the referrer was set
        /// </summary>  
        public DateTimeOffset referrerTimestamp { get; set; }

        public string referrerUrl { get; set; }

        public string[] toArray()
        {
            var infos = new string[4];
            infos[0] = campaignName;
            infos[1] = campaignKeyword;
            infos[2] = (referrerTimestamp - new DateTime(1970, 1, 1)).TotalSeconds.ToString();
            infos[3] = referrerUrl;
            return infos;
        }
    }
}
