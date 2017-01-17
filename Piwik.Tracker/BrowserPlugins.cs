#region license

// http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later

#endregion license

namespace Piwik.Tracker
{
    /// <summary>
    /// Supported browser plugins
    /// </summary>
    public class BrowserPlugins
    {
        /// <summary>
        /// Gets or sets a value indicating whether browser supports Flash.
        /// </summary>
        public bool Flash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports Java.
        /// </summary>
        public bool Java { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports Director.
        /// </summary>
        public bool Director { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports QuickTime.
        /// </summary>
        public bool QuickTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports RealPlayer.
        /// </summary>
        public bool RealPlayer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports PDF.
        /// </summary>
        public bool Pdf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports WindowsMedia.
        /// </summary>
        public bool WindowsMedia { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports Gears.
        /// </summary>
        public bool Gears { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browser supports Silverlight.
        /// </summary>
        public bool Silverlight { get; set; }
    }
}