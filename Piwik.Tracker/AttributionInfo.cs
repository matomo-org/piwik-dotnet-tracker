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
    }
}
