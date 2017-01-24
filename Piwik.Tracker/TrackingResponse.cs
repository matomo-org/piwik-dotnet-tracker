using System.Net;

namespace Piwik.Tracker
{
    /// <summary>
    /// Represents the result of a tracking action.
    /// </summary>
    public class TrackingResponse
    {
        /// <summary>
        /// Gets the HTTP status code recived from the piwik server.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; internal set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Url used for last request. Used in tests to output useful error messages.
        /// </summary>
        public string RequestedUrl { get; internal set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(HttpStatusCode)}: {HttpStatusCode}, {nameof(RequestedUrl)}: {RequestedUrl}";
        }
    }
}