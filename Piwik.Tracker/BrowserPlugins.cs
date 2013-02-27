#region license
// http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later
#endregion

/// <summary>
/// Piwik - Open source web analytics
/// For more information, see http://piwik.org
/// </summary>
namespace Piwik.Tracker
{
    /// <summary>
    /// Supported browser plugins
    /// </summary>
    public class BrowserPlugins
    {
        public bool flash { get; set; }
        public bool java { get; set; }
        public bool director { get; set; }
        public bool quickTime { get; set; }
        public bool realPlayer { get; set; }
        public bool pdf { get; set; }
        public bool windowsMedia { get; set; }
        public bool gears { get; set; }
        public bool silverlight { get; set; }
    }
}