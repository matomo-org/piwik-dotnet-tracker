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
    /// Visit Custom Variable
    /// See http://piwik.org/docs/custom-variables/
    /// </summary>
    public class CustomVar
    {
        public enum Scopes {Visit, Page, Event};

        public string Name { get; set; }
        public string Value { get; set; }

        public CustomVar(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
