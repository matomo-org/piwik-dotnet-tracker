#region license

// http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later

#endregion license

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
        public bool Flash { get; set; }
        public bool Java { get; set; }
        public bool Director { get; set; }
        public bool QuickTime { get; set; }
        public bool RealPlayer { get; set; }
        public bool Pdf { get; set; }
        public bool WindowsMedia { get; set; }
        public bool Gears { get; set; }
        public bool Silverlight { get; set; }
    }
}