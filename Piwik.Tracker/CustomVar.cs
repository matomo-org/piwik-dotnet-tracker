#region license

// http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later

#endregion license

namespace Piwik.Tracker
{
    /// <summary>
    /// Visit Custom Variable
    /// See http://piwik.org/docs/custom-variables/
    /// </summary>
    public class CustomVar
    {
        /// <summary>
        /// The possible scopes.
        /// </summary>
        public enum Scopes { Visit, Page, Event };

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVar"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public CustomVar(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}