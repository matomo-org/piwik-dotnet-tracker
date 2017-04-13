// <summary>
// Piwik - free/libre analytics platform
//
// Client to record visits, page views, Goals, Ecommerce activity (product views, add to carts, Ecommerce orders) in a Piwik server.
// This is a C# Version of the piwik.js standard Tracking API.
// For more information, see http://piwik.org/docs/tracking-api/
//
// <see href="http://piwik.org/docs/tracking-api/"/>
// Piwik Server Api: <see href="http://developer.piwik.org/api-reference/tracking-api"/>
// </summary>

namespace Piwik.Tracker
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Globalization;
    using System.Text;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// PiwikTracker implements the Piwik Tracking Web API.
    ///
    /// The PHP Tracking Client provides all features of the Javascript Tracker, such as Ecommerce Tracking, Custom Variable, Event tracking and more.
    /// Functions are named the same as the Javascript functions.
    ///
    /// See introduction docs at: {@link http://piwik.org/docs/tracking-api/}
    ///
    /// ### Example: using the PHP PiwikTracker class
    ///
    /// The following code snippet is an advanced example of how to track a Page View using the Tracking API PHP client.
    ///
    ///      $t = new PiwikTracker( $idSite = 1, 'http://example.org/piwik/');
    ///
    ///      // Optional function calls
    ///      $t->setUserAgent( "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-GB) Firefox/3.6.6");
    ///      $t->setBrowserLanguage('fr');
    ///      $t->setLocalTime( '12:34:06' );
    ///      $t->setResolution( 1024, 768 );
    ///      $t->setBrowserHasCookies(true);
    ///      $t->setPlugins($flash = true, $java = true, $director = false);
    ///
    ///      // set a Custom Variable called 'Gender'
    ///      $t->setCustomVariable( 1, 'gender', 'male' );
    ///
    ///      // If you want to force the visitor IP, or force the server date time to a date in the past,
    ///      // it is required to authenticate the Tracking request by calling setTokenAuth
    ///      // You can pass the Super User token_auth or any user with 'admin' privilege on the website $idSite
    ///      $t->setTokenAuth( $token_auth );
    ///      $t->setIp( "134.10.22.1" );
    ///      $t->setForceVisitDateTime( '2011-04-05 23:55:02' );
    ///
    ///      // if you wanted to force to record the page view or conversion to a specific User ID
    ///      // $t->setUserId( "username@example.org" );
    ///      // Mandatory: set the URL being tracked
    ///      $t->setUrl( $url = 'http://example.org/store/list-category-toys/' );
    ///
    ///      // Finally, track the page view with a Custom Page Title
    ///      // In the standard JS API, the content of the <![CDATA[<title>]]> tag would be set as the page title
    ///      $t->doTrackPageView('This is the page title');
    ///
    /// ### Example: tracking Ecommerce interactions
    ///
    /// Here is an example showing how to track Ecommerce interactions on your website, using the PHP Tracking API.
    /// Usually, Ecommerce tracking is done using standard Javascript code,
    /// but it is very common to record Ecommerce interactions after the fact
    /// (for example, when payment is done with Paypal and user doesn't come back on the website after purchase).
    /// For more information about Ecommerce tracking in Piwik, check out the documentation: Tracking Ecommerce in Piwik.
    ///
    ///      $t = new PiwikTracker( $idSite = 1, 'http://example.org/piwik/');
    ///
    ///      // Force IP to the actual visitor IP
    ///      $t->setTokenAuth( $token_auth );
    ///      $t->setIp( "134.10.22.1" );
    ///
    ///      // Example 1: on a Product page, track an "Ecommerce Product view"
    ///      $t->setUrl( $url = 'http://www.mystore.com/Endurance-Shackletons-Legendary-Antarctic-Expedition' );
    ///      $t->setEcommerceView($sku = 'SKU0011', $name = 'Endurance - Shackleton', $category = 'Books');
    ///      $t->doTrackPageView( 'Endurance Shackletons Legendary Antarctic Expedition - Mystore.com');
    ///
    ///      // Example 2: Tracking Ecommerce Cart containing 2 products
    ///      $t->addEcommerceItem($sku = 'SKU0011', $name = 'Endurance - Shackleton' , $category = 'Books', $price = 17, $quantity = 1);
    ///      // Note that when setting a product category, you can specify an array of up to 5 categories to track for this product
    ///      $t->addEcommerceItem($sku = 'SKU0321', $name = 'Amélie' , $categories = array('DVD Foreign','Best sellers','Our pick'), $price = 25, $quantity = 1);
    ///      $t->doTrackEcommerceCartUpdate($grandTotal = 42);
    ///
    ///      // Example 3: Tracking Ecommerce Order
    ///      $t->addEcommerceItem($sku = 'SKU0011', $name = 'Endurance - Shackleton' , $category = 'Books', $price = 17, $quantity = 1);
    ///      $t->addEcommerceItem($sku = 'SKU0321', $name = 'Amélie' , $categories = array('DVD Foreign','Best sellers','Our pick'), $price = 25, $quantity = 1);
    ///      $t->doTrackEcommerceOrder($orderId = 'B000111387', $grandTotal = 55.5, $subTotal = 42, $tax = 8, $shipping = 5.5, $discount = 10);
    ///
    /// ### Note: authenticating with the token_auth
    ///
    /// To set the visitor IP, or the date and time of the visit, or to force to record the visit (or page, or goal conversion) to a specific Visitor ID,
    /// you must call setTokenAuth( $token_auth ). The token_auth must be either the Super User token_auth,
    /// or the token_auth of any user with 'admin' permission for the website you are recording data against.
    /// </summary>
    public class PiwikTracker
    {
        /// <summary>
        /// API Version
        /// </summary>
        private const int Version = 1;

        /// <summary>
        /// Visitor ID length
        /// </summary>
        private const int LengthVisitorId = 16;

        /// <summary>
        /// Charset
        /// <see cref="SetPageCharset"/>
        /// </summary>
	    private const string DefaultCharsetParameterValues = "utf-8";

        /// <summary>
        /// <see cref="http://developer.piwik.org/api-reference/tracking-javascript"/>
        /// </summary>
        private const string FirstPartyCookiesPrefix = "_pk_";

        /// <summary>
        /// Ecommerce item page view tracking stores item's metadata in these Custom Variables slots.
        /// </summary>
        internal const int CvarIndexEcommerceItemPrice = 2;

        internal const int CvarIndexEcommerceItemSku = 3;
        internal const int CvarIndexEcommerceItemName = 4;
        internal const int CvarIndexEcommerceItemCategory = 5;

        private const string DefaultCookiePath = "/";

        // Life of the visitor cookie (in sec)
        private const int ConfigVisitorCookieTimeout = 33955200;

        // Life of the session cookie (in sec)
        private const int ConfigSessionCookieTimeout = 1800; // 30 minutes

        // Life of the session cookie (in sec)
        private const int ConfigReferralCookieTimeout = 15768000; // 6 months

        private string _debugAppendUrl;
        private string _userAgent;
        private DateTimeOffset _localTime = DateTimeOffset.MinValue;
        private bool _hasCookies;
        private string _plugins;
        private Dictionary<string, string[]> _visitorCustomVar;
        private Dictionary<string, string[]> _pageCustomVar = new Dictionary<string, string[]>();
        private Dictionary<string, string[]> _eventCustomVar = new Dictionary<string, string[]>();
        private Dictionary<string, string> _customParameters = new Dictionary<string, string>();
        private readonly string _customData;
        private DateTimeOffset _forcedDatetime = DateTimeOffset.MinValue;
        private bool _forcedNewVisit;
        private string _tokenAuth;
        private AttributionInfo _attributionInfo;
        private DateTimeOffset _ecommerceLastOrderTimestamp = DateTimeOffset.MinValue;
        private Dictionary<string, object[]> _ecommerceItems = new Dictionary<string, object[]>();
        private int? _generationTime;
        private string _referrerUrl;
        private string _pageCharset = DefaultCharsetParameterValues;
        private string _pageUrl;
        private string _ip;
        private string _acceptLanguage;
        private string _userId;
        private string _forcedVisitorId;
        private string _cookieVisitorId;
        private string _randomVisitorId;
        private int _width;
        private int _height;
        private bool _doBulkRequests;
        private List<string> _storedTrackingActions = new List<string>();
        private string _country;
        private string _region;
        private string _city;
        private float? _latitude;
        private float? _longitude;

        private bool _configCookiesDisabled;
        private string _configCookiePath = DefaultCookiePath;
        private string _configCookieDomain = "";
        private readonly long _currentTs = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
        private long _createTs;
        private long? _visitCount = 0;
        private long? _currentVisitTs;
        private long? _lastVisitTs;
        private long? _lastEcommerceOrderTs;
        private bool _sendImageResponse = true;

        /// <summary>
        /// Builds a PiwikTracker object, used to track visits, pages and Goal conversions
        /// for a specific website, by using the Piwik Tracking API.
        /// If the tracker is used within a web page or web controller, the following information are pre-initialised :
        /// URL Referrer, current page URL, remote IP, Accept-Language HTTP header and User-Agent HTTP header.
        /// </summary>
        /// <param name="idSite">Id site to be tracked</param>
        /// <param name="apiUrl">"http://example.org/piwik/" or "http://piwik.example.org/". If set, will overwrite PiwikTracker.URL</param>
        /// <exception cref="ArgumentException">apiUrl must not be null or empty</exception>
        public PiwikTracker(int idSite, string apiUrl)
        {
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new ArgumentException("Piwik api url must not be emty or null.", nameof(apiUrl));
            }
            PiwikBaseUrl = FixPiwikBaseUrl(apiUrl);
            IdSite = idSite;

            _referrerUrl = HttpContext.Current?.Request?.UrlReferrer?.AbsoluteUri ?? string.Empty;
            _ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            _acceptLanguage = HttpContext.Current?.Request?.UserLanguages?.FirstOrDefault() ?? string.Empty;
            _userAgent = HttpContext.Current?.Request?.UserAgent ?? string.Empty;

            _pageUrl = GetCurrentUrl();
            SetNewVisitorId();
            _createTs = _currentTs;
            _visitorCustomVar = GetCustomVariablesFromCookie();
        }

        /// <summary>
        /// Gets the Piwik base URL, for example http://example.org/piwik/
        /// </summary>
        public string PiwikBaseUrl { get; }

        /// <summary>
        /// Gets the piwik site ID.
        /// see: http://developer.piwik.org/api-reference/tracking-api
        /// idsite (required) — The ID of the website we're tracking a visit/action for.
        /// </summary>
        public int IdSite { get; }

        /// <summary>
        /// Gets or sets the maximum number of seconds the tracker will spend waiting for a response
        /// from Piwik. Defaults to 600 seconds.
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(600);

        /// <summary>
        /// Gets or sets the proxy used for web-requests, or <c>null</c> if no proxy is used.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// By default, Piwik expects utf-8 encoded values, for example
        /// for the page URL parameter values, Page Title, etc.
        /// It is recommended to only send UTF-8 data to Piwik.
        /// If required though, you can also specify another charset using this function.
        /// </summary>
        /// <param name="charset">The charset.</param>
	    public void SetPageCharset(string charset = "")
        {
            _pageCharset = charset;
        }

        /// <summary>
        /// Sets the current URL being tracked
        /// </summary>
        /// <param name="url">Raw URL (not URL encoded)</param>
        public void SetUrl(string url)
        {
            _pageUrl = url;
        }

        /// <summary>
        /// Sets the URL referrer used to track Referrers details for new visits.
        /// </summary>
        /// <param name="url">Raw URL (not URL encoded)</param>
        public void SetUrlReferrer(string url)
        {
            _referrerUrl = url;
        }

        /// <summary>
        /// Sets the time that generating the document on the server side took.
        /// </summary>
        /// <param name="timeMs">Generation time in ms</param>
	    public void SetGenerationTime(int timeMs)
        {
            _generationTime = timeMs;
        }

        /// <summary>
        /// Sets the attribution information to the visit, so that subsequent Goal conversions are
        /// properly attributed to the right Referrer URL, timestamp, Campaign Name and Keyword.
        ///
        /// If you call enableCookies() then these referral attribution values will be set
        /// to the 'ref' first party cookie storing referral information.
        /// </summary>
        /// <param name="attributionInfo">Attribution info for the visit</param>
        /// <see>function getAttributionInfo() in "https://github.com/piwik/piwik/blob/master/js/piwik.js"</see>
        public void SetAttributionInfo(AttributionInfo attributionInfo)
        {
            _attributionInfo = attributionInfo;
        }

        /// <summary>
        /// Sets Visit Custom Variable.
        /// See http://piwik.org/docs/custom-variables/
        /// </summary>
        /// <param name="id">Custom variable slot ID from 1-5</param>
        /// <param name="name">Custom variable name</param>
        /// <param name="value">Custom variable value</param>
        /// <param name="scope">Custom variable scope. Possible values: visit, page, event</param>
        /// <exception cref="ArgumentException">Invalid 'scope' parameter value - scope</exception>
        public void SetCustomVariable(int id, string name, string value, Scopes scope = Scopes.Visit)
        {
            string stringId = Convert.ToString(id);
            string[] customVar = { name, value };

            switch (scope)
            {
                case Scopes.Page:
                    _pageCustomVar[stringId] = customVar;
                    break;

                case Scopes.Visit:
                    _visitorCustomVar[stringId] = customVar;
                    break;

                case Scopes.Event:
                    _eventCustomVar[stringId] = customVar;
                    break;

                default:
                    throw new ArgumentException("Invalid 'scope' parameter value", nameof(scope));
            }
        }

        /// <summary>
        /// Sets a custom tracking parameter
        /// <para></para>
        /// To track custom dimensions use 'dimension{#}' as the value for
        /// <paramref name="trackingApiParameter"/>, e.g. dimension1.
        /// </summary>
        /// <param name="trackingApiParameter">The name of the custom tracking parameter. Use dimension{#} for custom dimensions, e.g. dimension1 for dimension 1.</param>
        /// <param name="value">The value of the custom parameter</param>
        public void SetCustomTrackingParameter(string trackingApiParameter, string value)
        {
            _customParameters[trackingApiParameter] = value;
        }

        /// <summary>
        /// Returns the currently assigned Custom Variable.
        /// If scope is 'visit', it will attempt to read the value set in the first party cookie created by Piwik Tracker ($_COOKIE array).
        /// </summary>
        /// <param name="id">Custom Variable integer index to fetch from cookie. Should be a value from 1 to 5</param>
        /// <param name="scope">Custom variable scope. Possible values: visit, page, event</param>
        /// <returns>
        /// The requested custom variable
        /// </returns>
        /// <exception cref="ArgumentException">Invalid 'scope' parameter value - scope</exception>
        public CustomVar GetCustomVariable(int id, Scopes scope = Scopes.Visit)
        {
            var stringId = Convert.ToString(id);
            switch (scope)
            {
                case Scopes.Page:
                    return _pageCustomVar.ContainsKey(stringId)
                        ? new CustomVar(_pageCustomVar[stringId][0], _pageCustomVar[stringId][1])
                        : null;

                case Scopes.Event:
                    return _eventCustomVar.ContainsKey(stringId)
                        ? new CustomVar(_eventCustomVar[stringId][0], _eventCustomVar[stringId][1])
                        : null;

                case Scopes.Visit:
                    if (_visitorCustomVar.ContainsKey(stringId))
                    {
                        return new CustomVar(_visitorCustomVar[stringId][0], _visitorCustomVar[stringId][1]);
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid 'scope' parameter value", nameof(scope));
            }

            var cookieDecoded = GetCustomVariablesFromCookie();
            if (!cookieDecoded.ContainsKey(stringId)
                || cookieDecoded[stringId].Count() != 2)
            {
                return null;
            }
            return new CustomVar(cookieDecoded[stringId][0], cookieDecoded[stringId][1]);
        }

        /// <summary>
        /// Returns the currently assigned value for the given custom tracking parameter
        /// </summary>
        /// <param name="trackingApiParameter">The name of the custom tracking parameter</param>
        /// <returns>The value of the custom tracking parameter</returns>
        public string GetCustomTrackingParameter(string trackingApiParameter)
        {
            return _customParameters.ContainsKey(trackingApiParameter) ? _customParameters[trackingApiParameter] : "";
        }

        /// <summary>
        /// Gets the stored tracking actions, that have been stored for bulkRequest <see cref="EnableBulkTracking"/>, <see cref="DoBulkTrack"/>.
        /// </summary>
        /// <returns></returns>
        public string[] GetStoredTrackingActions()
        {
            return _storedTrackingActions.ToArray();
        }

        /// <summary>
        /// Clears any Custom Variable that may be have been set.
        ///
        /// This can be useful when you have enabled bulk requests,
        /// and you wish to clear Custom Variables of 'visit' scope.
        /// </summary>
        public void ClearCustomVariables()
        {
            _visitorCustomVar = new Dictionary<string, string[]>();
            _pageCustomVar = new Dictionary<string, string[]>();
            _eventCustomVar = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Clears any custom tracking parameters that have been set.
        /// </summary>
        public void ClearCustomTrackingParameters()
        {
            _customParameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets the current visitor ID to a random new one.
        /// </summary>
        public void SetNewVisitorId()
        {
            _randomVisitorId = Guid.NewGuid().ToString().CreateMd5().Substring(0, LengthVisitorId).ToLower();
            _userId = null;
            _forcedVisitorId = null;
            _cookieVisitorId = null;
        }

        /// <summary>
        /// Sets the Browser language. Used to guess visitor countries when GeoIP is not enabled
        /// </summary>
        /// <param name="acceptLanguage">For example "fr-fr"</param>
        public void SetBrowserLanguage(string acceptLanguage)
        {
            _acceptLanguage = acceptLanguage;
        }

        /// <summary>
        /// Sets the user agent, used to detect OS and browser.
        /// If this function is not called, the User Agent will default to the current user agent
        /// if there is an active HttpContext
        /// </summary>
        /// <param name="userAgent">HTTP User Agent</param>
        public void SetUserAgent(string userAgent)
        {
            _userAgent = userAgent;
        }

        /// <summary>
        /// Sets the country of the visitor. If not used, Piwik will try to find the country
        /// using either the visitor's IP address or language.
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>
        /// <param name="country">The country.</param>
        public void SetCountry(string country)
        {
            _country = country;
        }

        /// <summary>
        /// Sets the region of the visitor. If not used, Piwik may try to find the region
        /// using the visitor's IP address (if configured to do so).
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>
        /// <param name="region">The region.</param>
        public void SetRegion(string region)
        {
            _region = region;
        }

        /// <summary>
        /// Sets the city of the visitor. If not used, Piwik may try to find the city
        /// using the visitor's IP address (if configured to do so).
        ///
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>
        public void SetCity(string city)
        {
            _city = city;
        }

        /// <summary>
        /// Sets the latitude of the visitor. If not used, Piwik may try to find the visitor's
        /// latitude using the visitor's IP address (if configured to do so).
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>
        /// <param name="lat">The lat.</param>
        public void SetLatitude(float lat)
        {
            _latitude = lat;
        }

        /// <summary>
        /// Sets the longitude of the visitor. If not used, Piwik may try to find the visitor's
        /// longitude using the visitor's IP address (if configured to do so).
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>
        /// <param name="longitude">The longitude.</param>
        public void SetLongitude(float longitude)
        {
            _longitude = longitude;
        }

        /// <summary>
        /// Enables the bulk request feature. When used, each tracking action is stored until the
        /// doBulkTrack method is called. This method will send all tracking data at once.
        /// </summary>
        public void EnableBulkTracking()
        {
            _doBulkRequests = true;
        }

        /// <summary>
        /// Enable Cookie Creation - this will cause a first party VisitorId cookie to be set when the VisitorId is set or reset
        /// </summary>
        /// <param name="domain">(optional) Set first-party cookie domain. Accepted values: example.com, *.example.com (same as .example.com) or subdomain.example.com</param>
        /// <param name="path">(optional) Set first-party cookie path</param>
        public void EnableCookies(string domain = "", string path = "/")
        {
            _configCookiesDisabled = false;
            _configCookieDomain = DomainFixup(domain);
            _configCookiePath = path;
        }

        /// <summary>
        /// If image response is disabled Piwik will respond with a HTTP 204 header instead of responding with a gif.
        /// </summary>
        public void DisableSendImageResponse()
        {
            _sendImageResponse = false;
        }

        /// <summary>
        /// Fix-up domain
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns></returns>
        protected static string DomainFixup(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return domain;
            }
            var dl = domain.Length - 1;
            // remove trailing '.'
            if (domain[dl].Equals('.'))
            {
                domain = domain.Substring(0, dl);
            }
            // remove leading '*'
            if (domain.Substring(0, 2).Equals("*."))
            {
                domain = domain.Substring(1);
            }
            return domain;
        }

        /// <summary>
        /// Get cookie name with prefix and domain hash
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <returns></returns>
        protected string GetCookieName(string cookieName)
        {
            // NOTE: If the cookie name is changed, we must also update the method in piwik.js with the same name.
            var cookieDomain = (string.IsNullOrWhiteSpace(_configCookieDomain)
                ? GetCurrentHost()
                : _configCookieDomain)
                + _configCookiePath;
            var hash = cookieDomain.CreateSha1(hashAsHexadecimal: true).Substring(0, 4);
            return FirstPartyCookiesPrefix + cookieName + "." + IdSite + "." + hash;
        }

        /// <summary>
        /// Tracks a page view
        /// </summary>
        /// <param name="documentTitle">Page title as it will appear in the Actions > Page titles report</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackPageView(string documentTitle = null)
        {
            string url = GetUrlTrackPageView(documentTitle);
            return SendRequest(url);
        }

        /// <summary>
        /// Tracks an event
        /// </summary>
        /// <param name="category">The Event Category (Videos, Music, Games...)</param>
        /// <param name="action">The Event's Action (Play, Pause, Duration, Add Playlist, Downloaded, Clicked...)</param>
        /// <param name="name">(optional) The Event's object Name (a particular Movie name, or Song name, or File name...)</param>
        /// <param name="value">(optional) The Event's value</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackEvent(string category, string action, string name = "", string value = "")
        {
            var url = GetUrlTrackEvent(category, action, name, value);
            return SendRequest(url);
        }

        /// <summary>
        /// Tracks a content impression
        /// </summary>
        /// <param name="contentName">The name of the content. For instance 'Ad Foo Bar'</param>
        /// <param name="contentPiece">The actual content. For instance the path to an image, video, audio, any text</param>
        /// <param name="contentTarget">(optional) The target of the content. For instance the URL of a landing page.</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackContentImpression(string contentName, string contentPiece = "Unknown", string contentTarget = null)
        {
            var url = GetUrlTrackContentImpression(contentName, contentPiece, contentTarget);
            return SendRequest(url);
        }

        /// <summary>
        /// Tracks a content interaction. Make sure you have tracked a content impression using the same content name and
        /// content piece, otherwise it will not count. To do so you should call the method doTrackContentImpression();
        /// </summary>
        /// <param name="interaction">The name of the interaction with the content. For instance a 'click'</param>
        /// <param name="contentName">The name of the content. For instance 'Ad Foo Bar'</param>
        /// <param name="contentPiece">The actual content. For instance the path to an image, video, audio, any text</param>
        /// <param name="contentTarget">(optional) The target the content leading to when an interaction occurs. For instance the URL of a landing page.</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackContentInteraction(string interaction, string contentName, string contentPiece = "Unknown", string contentTarget = null)
        {
            var url = GetUrlTrackContentInteraction(interaction, contentName, contentPiece, contentTarget);
            return SendRequest(url);
        }

        /// <summary>
        /// Tracks an internal Site Search query, and optionally tracks the Search Category, and Search results Count.
        /// These are used to populate reports in Actions > Site Search.
        /// </summary>
        /// <param name="keyword">Searched query on the site</param>
        /// <param name="category">(optional) Search engine category if applicable</param>
        /// <param name="countResults">(optional) results displayed on the search result page. Used to track "zero result" keywords.</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackSiteSearch(string keyword, string category = "", int? countResults = null)
        {
            var url = GetUrlTrackSiteSearch(keyword, category, countResults);
            return SendRequest(url);
        }

        /// <summary>
        /// Records a Goal conversion
        /// </summary>
        /// <param name="idGoal">Id Goal to record a conversion</param>
        /// <param name="revenue">Revenue for this conversion</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackGoal(int idGoal, float revenue = 0)
        {
            string url = GetUrlTrackGoal(idGoal, revenue);
            return SendRequest(url);
        }

        /// <summary>
        /// Tracks a download or outlink
        /// </summary>
        /// <param name="actionUrl">URL of the download or outlink</param>
        /// <param name="actionType">Type of the action: 'download' or 'link'</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackAction(string actionUrl, ActionType actionType)
        {
            // Referrer could be udpated to be the current URL temporarily (to mimic JS behavior)
            string url = GetUrlTrackAction(actionUrl, actionType);
            return SendRequest(url);
        }

        /// <summary>
        /// Adds an item in the Ecommerce order.
        /// This should be called before doTrackEcommerceOrder(), or before doTrackEcommerceCartUpdate().
        /// This function can be called for all individual products in the cart (or order).
        /// SKU parameter is mandatory. Other parameters are optional (set to false if value not known).
        /// Ecommerce items added via this function are automatically cleared when doTrackEcommerceOrder() or getUrlTrackEcommerceOrder() is called.
        /// </summary>
        /// <param name="sku">SKU, Product identifier</param>
        /// <param name="name">Product name</param>
        /// <param name="categories">Array of product categories (up to 5 categories can be specified for a given product)</param>
        /// <param name="price">Individual product price (supports integer and decimal prices)</param>
        /// <param name="quantity">Product quantity. If not specified, will default to 1 in the Reports</param>
        /// <exception cref="System.ArgumentException">You must specify a SKU for the Ecommerce item - sku</exception>
        public void AddEcommerceItem(string sku, string name = "", List<string> categories = null, double price = 0, ulong quantity = 1)
        {
            if (string.IsNullOrEmpty(sku))
            {
                throw new ArgumentException("You must specify a SKU for the Ecommerce item", nameof(sku));
            }

            object[] eCommerceItem = { sku, name, categories, FormatMonetaryValue(price), quantity };

            _ecommerceItems.Remove(sku);
            _ecommerceItems.Add(sku, eCommerceItem);
        }

        /// <summary>
	    /// Tracks a Cart Update (add item, remove item, update item).
	    ///
	    /// On every Cart update, you must call addEcommerceItem() for each item (product) in the cart,
	    /// including the items that haven't been updated since the last cart update.
	    /// Items which were in the previous cart and are not sent in later Cart updates will be deleted from the cart (in the database).
        /// </summary>
        /// <param name="grandTotal">Cart grandTotal (typically the sum of all items' prices)</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackEcommerceCartUpdate(double grandTotal)
        {
            string url = GetUrlTrackEcommerceCartUpdate(grandTotal);
            return SendRequest(url);
        }

        /// <summary>
        /// Sends all stored tracking actions at once. Only has an effect if bulk tracking is enabled.
        /// To enable bulk tracking, call enableBulkTracking().
        /// </summary>
        /// <returns>
        /// Response
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Error: you must call the function DoTrackPageView or DoTrackGoal from this class, before calling this method DoBulkTrack()</exception>
        public TrackingResponse DoBulkTrack()
        {
            if (!_storedTrackingActions.Any())
            {
                throw new InvalidOperationException("Error: you must call the function DoTrackPageView or DoTrackGoal from this class, before calling this method DoBulkTrack()");
            }

            var data = new Dictionary<string, object>();
            data["requests"] = _storedTrackingActions;

            // token_auth is not required by default, except if bulk_requests_require_authentication=1
            if (!string.IsNullOrWhiteSpace(_tokenAuth))
            {
                data["token_auth"] = _tokenAuth;
            }

            var postData = new JavaScriptSerializer().Serialize(data);
            var response = SendRequest(PiwikBaseUrl, "POST", postData, true);

            _storedTrackingActions = new List<string>();

            return response;
        }

        /// <summary>
	    /// Tracks an Ecommerce order.
	    ///
	    /// If the Ecommerce order contains items (products), you must call first the addEcommerceItem() for each item in the order.
	    /// All revenues (grandTotal, subTotal, tax, shipping, discount) will be individually summed and reported in Piwik reports.
	    /// Only the parameters $orderId and $grandTotal are required.
        /// </summary>
        /// <param name="orderId">Unique Order ID. This will be used to count this order only once in the event the order page is reloaded several times. orderId must be unique for each transaction, even on different days, or the transaction will not be recorded by Piwik.</param>
        /// <param name="grandTotal">Grand Total revenue of the transaction (including tax, shipping, etc.)</param>
        /// <param name="subTotal">Sub total amount, typically the sum of items prices for all items in this order (before Tax and Shipping costs are applied)</param>
        /// <param name="tax">Tax amount for this order</param>
        /// <param name="shipping">Shipping amount for this order</param>
        /// <param name="discount">Discounted amount in this order</param>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoTrackEcommerceOrder(string orderId, double grandTotal, double subTotal = 0, double tax = 0, double shipping = 0, double discount = 0)
        {
            string url = GetUrlTrackEcommerceOrder(orderId, grandTotal, subTotal, tax, shipping, discount);
            return SendRequest(url);
        }

        /// <summary>
        /// Sends a ping request.
        ///
        /// Ping requests do not track new actions. If they are sent within the standard visit length (see global.ini.php),
        /// they will extend the existing visit and the current last action for the visit. If after the standard visit length,
        /// ping requests will create a new visit using the last action in the last known visit.
        /// </summary>
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public TrackingResponse DoPing()
        {
            var url = GetRequest(IdSite);
            url += "&ping=1";
            return SendRequest(url);
        }

        /// <summary>
        /// Sets the current page view as an item (product) page view, or an Ecommerce Category page view.
        ///
        /// This must be called before doTrackPageView() on this product/category page.
        /// It will set 3 custom variables of scope "page" with the SKU, Name and Category for this page view.
        /// Note: Custom Variables of scope "page" slots 3, 4 and 5 will be used.
        ///
        /// On a category page, you may set the parameter $category only and set the other parameters to false.
        ///
        /// Tracking Product/Category page views will allow Piwik to report on Product and Categories
        /// conversion rates (Conversion rate = Ecommerce orders containing this product or category / Visits to the product or category)
        ///
        /// </summary>
        /// <param name="sku">Product SKU being viewed</param>
        /// <param name="name">Product Name being viewed</param>
        /// <param name="categories">Category being viewed. On a Product page, this is the product's category. You can also specify an array of up to 5 categories for a given page view.</param>
        /// <param name="price">Specify the price at which the item was displayed</param>
        public void SetEcommerceView(string sku = "", string name = "", List<string> categories = null, double price = 0)
        {
            var serializedCategories = "";
            if (categories != null)
            {
                serializedCategories = new JavaScriptSerializer().Serialize(categories);
            }
            SetCustomVariable(CvarIndexEcommerceItemCategory, "_pkc", serializedCategories, Scopes.Page);

            if (!price.Equals(0))
            {
                SetCustomVariable(CvarIndexEcommerceItemPrice, "_pkp", FormatMonetaryValue(price), Scopes.Page);
            }

            // On a category page, do not record "Product name not defined"
            if (string.IsNullOrEmpty(sku) && string.IsNullOrEmpty(name))
            {
                return;
            }
            if (!string.IsNullOrEmpty(sku))
            {
                SetCustomVariable(CvarIndexEcommerceItemSku, "_pks", sku, Scopes.Page);
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "";
            }
            SetCustomVariable(CvarIndexEcommerceItemName, "_pkn", name, Scopes.Page);
        }

        /// <summary>
        /// Returns URL used to track Ecommerce Cart updates
        /// Calling this function will reinitializes the property ecommerceItems to empty array
        /// so items will have to be added again via addEcommerceItem()
        /// </summary>
        /// <param name="grandTotal">The grand total.</param>
        /// <returns></returns>
        private string GetUrlTrackEcommerceCartUpdate(double grandTotal)
        {
            return GetUrlTrackEcommerce(grandTotal);
        }

        /// <summary>
        /// Returns URL used to track Ecommerce Orders
        /// Calling this function will reinitializes the property ecommerceItems to empty array
        /// so items will have to be added again via addEcommerceItem()
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="grandTotal">The grand total.</param>
        /// <param name="subTotal">The sub total.</param>
        /// <param name="tax">The tax.</param>
        /// <param name="shipping">The shipping.</param>
        /// <param name="discount">The discount.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">You must specifiy an orderId for the Ecommerce order - orderId</exception>
        public string GetUrlTrackEcommerceOrder(string orderId, double grandTotal, double subTotal = 0, double tax = 0, double shipping = 0, double discount = 0)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                throw new ArgumentException("You must specifiy an orderId for the Ecommerce order", nameof(orderId));
            }

            string url = GetUrlTrackEcommerce(grandTotal, subTotal, tax, shipping, discount);
            url += "&ec_id=" + UrlEncode(orderId);

            _ecommerceLastOrderTimestamp = _forcedDatetime.Equals(DateTimeOffset.MinValue) ? DateTimeOffset.Now : _forcedDatetime;

            return url;
        }

        /// <summary>
        /// Returns URL used to track Ecommerce orders
        /// Calling this function will reinitializes the property ecommerceItems to empty array
        /// so items will have to be added again via addEcommerceItem()
        /// </summary>
        /// <param name="grandTotal">The grand total.</param>
        /// <param name="subTotal">The sub total.</param>
        /// <param name="tax">The tax.</param>
        /// <param name="shipping">The shipping.</param>
        /// <param name="discount">The discount.</param>
        /// <returns></returns>
        protected internal string GetUrlTrackEcommerce(double grandTotal, double subTotal = 0, double tax = 0, double shipping = 0, double discount = 0)
        {
            string url = GetRequest(IdSite) + "&idgoal=0&revenue=" + FormatMonetaryValue(grandTotal);

            if (!subTotal.Equals(0))
            {
                url += "&ec_st=" + FormatMonetaryValue(subTotal);
            }

            if (!tax.Equals(0))
            {
                url += "&ec_tx=" + FormatMonetaryValue(tax);
            }

            if (!shipping.Equals(0))
            {
                url += "&ec_sh=" + FormatMonetaryValue(shipping);
            }

            if (!discount.Equals(0))
            {
                url += "&ec_dt=" + FormatMonetaryValue(discount);
            }

            if (_ecommerceItems.Count > 0)
            {
                url += "&ec_items=" + UrlEncode(new JavaScriptSerializer().Serialize(_ecommerceItems.Values));
            }

            _ecommerceItems = new Dictionary<string, object[]>();

            return url;
        }

        /// <summary>
        /// Builds URL to track a page view.
        /// </summary>
        /// <see cref="DoTrackPageView"/>
        /// <param name="documentTitle">Page view name as it will appear in Piwik reports</param>
        /// <returns>URL to piwik.php with all parameters set to track the pageview</returns>
        public string GetUrlTrackPageView(string documentTitle = "")
        {
            var url = GetRequest(IdSite);

            if (!string.IsNullOrWhiteSpace(documentTitle))
            {
                url += "&action_name=" + UrlEncode(documentTitle);
            }

            return url;
        }

        /// <summary>
        /// Builds URL to track a custom event.
        /// </summary>
        /// <param name="category">The Event Category (Videos, Music, Games...)</param>
        /// <param name="action">The Event's Action (Play, Pause, Duration, Add Playlist, Downloaded, Clicked...)</param>
        /// <param name="name">(optional) The Event's object Name (a particular Movie name, or Song name, or File name...)</param>
        /// <param name="value">(optional) The Event's value</param>
        /// <returns>
        /// URL to piwik.php with all parameters set to track the pageview
        /// </returns>
        /// <exception cref="ArgumentException">
        /// You must specify an Event Category name (Music, Videos, Games...). - category
        /// or
        /// You must specify an Event action (click, view, add...). - action
        /// </exception>
        /// <see cref="DoTrackEvent" />
        public string GetUrlTrackEvent(string category, string action, string name = "", string value = "")
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("You must specify an Event Category name (Music, Videos, Games...).", nameof(category));
            }
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("You must specify an Event action (click, view, add...).", nameof(action));
            }

            var url = GetRequest(IdSite);
            url += "&e_c=" + UrlEncode(category);
            url += "&e_a=" + UrlEncode(action);

            if (!string.IsNullOrWhiteSpace(name))
            {
                url += "&e_n=" + UrlEncode(name);
            }
            if (!string.IsNullOrWhiteSpace(value))
            {
                url += "&e_v=" + value;
            }
            return url;
        }

        /// <summary>
        /// Builds URL to track a content impression.
        /// </summary>
        /// <param name="contentName">The name of the content. For instance 'Ad Foo Bar'</param>
        /// <param name="contentPiece">The actual content. For instance the path to an image, video, audio, any text</param>
        /// <param name="contentTarget">(optional) The target of the content. For instance the URL of a landing page.</param>
        /// <returns>
        /// URL to piwik.php with all parameters set to track the pageview
        /// </returns>
        /// <exception cref="ArgumentException">You must specify a content name - contentName</exception>
        /// <see cref="DoTrackContentImpression" />
        public string GetUrlTrackContentImpression(string contentName, string contentPiece, string contentTarget)
        {
            if (string.IsNullOrWhiteSpace(contentName))
            {
                throw new ArgumentException("You must specify a content name", nameof(contentName));
            }

            var url = GetRequest(IdSite);
            url += "&c_n=" + UrlEncode(contentName);

            if (!string.IsNullOrWhiteSpace(contentPiece))
            {
                url += "&c_p=" + UrlEncode(contentPiece);
            }
            if (!string.IsNullOrWhiteSpace(contentTarget))
            {
                url += "&c_t=" + UrlEncode(contentTarget);
            }

            return url;
        }

        /// <summary>
        /// Builds URL to track a content impression.
        /// </summary>
        /// <param name="interaction">The name of the interaction with the content. For instance a 'click'</param>
        /// <param name="contentName">The name of the content. For instance 'Ad Foo Bar'</param>
        /// <param name="contentPiece">The actual content. For instance the path to an image, video, audio, any text</param>
        /// <param name="contentTarget">(optional) The target the content leading to when an interaction occurs. For instance the URL of a landing page.</param>
        /// <returns>
        /// URL to piwik.php with all parameters set to track the pageview
        /// </returns>
        /// <exception cref="ArgumentException">
        /// You must specify a name for the interaction - interaction
        /// or
        /// You must specify a content name - contentName
        /// </exception>
        /// <see cref="DoTrackContentImpression" />
        public string GetUrlTrackContentInteraction(string interaction, string contentName, string contentPiece, string contentTarget)
        {
            if (string.IsNullOrWhiteSpace(interaction))
            {
                throw new ArgumentException("You must specify a name for the interaction", nameof(interaction));
            }

            if (string.IsNullOrWhiteSpace(contentName))
            {
                throw new ArgumentException("You must specify a content name", nameof(contentName));
            }

            var url = GetRequest(IdSite);
            url += "&c_i=" + UrlEncode(interaction);
            url += "&c_n=" + UrlEncode(contentName);

            if (!string.IsNullOrWhiteSpace(contentPiece))
            {
                url += "&c_p=" + UrlEncode(contentPiece);
            }
            if (!string.IsNullOrWhiteSpace(contentTarget))
            {
                url += "&c_t=" + UrlEncode(contentTarget);
            }

            return url;
        }

        /// <summary>
        /// Builds URL to track a site search.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="category">The category.</param>
        /// <param name="countResults">The count results.</param>
        /// <returns></returns>
        /// <see cref="DoTrackSiteSearch" />
        public string GetUrlTrackSiteSearch(string keyword, string category, int? countResults)
        {
            var url = GetRequest(IdSite);
            url += "&search=" + UrlEncode(keyword);
            if (!string.IsNullOrWhiteSpace(category))
            {
                url += "&search_cat=" + UrlEncode(category);
            }
            if (countResults != null)
            {
                url += "&search_count=" + countResults;
            }
            return url;
        }

        /// <summary>
        /// Builds URL to track a goal with idGoal and revenue.
        /// </summary>
        /// <see cref="DoTrackGoal"/>
        /// <param name="idGoal">Id Goal to record a conversion</param>
        /// <param name="revenue">Revenue for this conversion</param>
        /// <returns>URL to piwik.php with all parameters set to track the goal conversion</returns>
        public string GetUrlTrackGoal(int idGoal, float revenue = 0)
        {
            var url = GetRequest(IdSite);
            url += "&idgoal=" + idGoal;
            if (!revenue.Equals(0))
            {
                url += "&revenue=" + FormatMonetaryValue(revenue);
            }
            return url;
        }

        /// <summary>
        /// Builds URL to track a new action.
        /// </summary>
        /// <see cref="DoTrackAction"/>
        /// <param name="actionUrl">URL of the download or outlink</param>
        /// <param name="actionType">Type of the action: 'download' or 'link'</param>
        /// <returns>URL to piwik.php with all parameters set to track an action</returns>
        public string GetUrlTrackAction(string actionUrl, ActionType actionType)
        {
            var url = GetRequest(IdSite);
            url += "&" + actionType + "=" + UrlEncode(actionUrl);
            return url;
        }

        /// <summary>
        /// Overrides server date and time for the tracking requests.
        /// By default Piwik will track requests for the "current datetime" but this function allows you
        /// to track visits in the past. All times are in UTC.
        ///
        /// Allowed only for Super User, must be used along with setTokenAuth()
        /// </summary>
        /// <param name="dateTime">Date to set</param>
        public void SetForceVisitDateTime(DateTimeOffset dateTime)
        {
            _forcedDatetime = dateTime;
        }

        /// <summary>
        /// Forces Piwik to create a new visit for the tracking request.
        ///
        /// By default, Piwik will create a new visit if the last request by this user was more than 30 minutes ago.
        /// If you call setForceNewVisit() before calling doTrack*, then a new visit will be created for this request.
        /// </summary>
        public void SetForceNewVisit()
        {
            _forcedNewVisit = true;
        }

        /// <summary>
        /// Overrides IP address
        ///
        /// Allowed only for Super User, must be used along with setTokenAuth()
        /// </summary>
        /// <param name="ip">IP string, eg. 130.54.2.1</param>
        public void SetIp(string ip)
        {
            _ip = ip;
        }

        /// <summary>
        /// Force the action to be recorded for a specific User. The User ID is a string representing a given user in your system.
        /// A User ID can be a username, UUID or an email address, or any number or string that uniquely identifies a user or client.
        /// </summary>
        /// <param name="userId">Any user ID string (eg. email address, ID, username). Must not be empty or <c>null</c>.</param>
        /// <exception cref="System.ArgumentException">User ID cannot be empty or null. - userId</exception>
        public void SetUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID must not be empty or null.", nameof(userId));
            }
            _userId = userId;
        }

        /// <summary>
        /// Hash function used internally by Piwik to hash a User ID into the Visitor ID.
        /// Note: matches implementation of Tracker\Request-&gt;getUserIdHashed()
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetUserIdHashed(string id)
        {
            var hash = id.CreateSha1(hashAsHexadecimal: false);
            return hash.Substring(0, 16);
        }

        /// <summary>
        /// Forces the requests to be recorded for the specified Visitor ID.
        /// Note: it is recommended to use -&gt;setUserId($userId); instead.
        /// Rather than letting Piwik attribute the user with a heuristic based on IP and other user fingeprinting attributes,
        /// force the action to be recorded for a particular visitor.
        /// If you use both setVisitorId and setUserId, setUserId will take precedence.
        /// If not set, the visitor ID will be fetched from the 1st party cookie, or will be set to a random UUID.
        /// </summary>
        /// <param name="visitorId">16 hexadecimal characters visitor ID, eg. "33c31e01394bdc63"</param>
        /// <exception cref="System.ArgumentException">SetVisitorId() expects a n characters hexadecimal string
        /// (containing only the following: "01234567890abcdefABCDEF")</exception>
        [Obsolete("We recommend to use  ->setUserId($userId).")]
        public void SetVisitorId(string visitorId)
        {
            if (visitorId.Length != LengthVisitorId
                || !Regex.IsMatch(visitorId, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                throw new ArgumentException("setVisitorId() expects a " + LengthVisitorId
                                + " characters hexadecimal string (containing only the following: "
                                + "01234567890abcdefABCDEF"
                                + ")", nameof(visitorId));
            }

            _forcedVisitorId = visitorId;
        }

        /// <summary>
        /// If the user initiating the request has the Piwik first party cookie,
        /// this function will try and return the ID parsed from this first party cookie.
        ///
        /// If you call this function from a server, where the call is triggered by a cron or script
        /// not initiated by the actual visitor being tracked, then it will return
        /// the random Visitor ID that was assigned to this visit object.
        ///
        /// This can be used if you wish to record more visits, actions or goals for this visitor ID later on.
        /// </summary>
        /// <returns>16 hex chars visitor ID string</returns>
        public string GetVisitorId()
        {
            if (!string.IsNullOrEmpty(_userId))
            {
                return GetUserIdHashed(_userId);
            }
            if (!string.IsNullOrEmpty(_forcedVisitorId))
            {
                return _forcedVisitorId;
            }
            if (LoadVisitorIdCookie())
            {
                return _cookieVisitorId;
            }
            return _randomVisitorId;
        }

        /// <summary>
        /// Returns the User ID string, which may have been set via:
        ///     $v->setUserId('username@example.org');
        /// </summary>
        public string GetUserId()
        {
            return _userId;
        }

        /// <summary>
        /// Loads values from the VisitorId Cookie
        /// </summary>
        /// <returns>True if cookie exists and is valid, False otherwise</returns>
        protected bool LoadVisitorIdCookie()
        {
            var idCookie = GetCookieMatchingName("id");
            if (idCookie == null)
            {
                return false;
            }
            var parts = idCookie.Value.Split('.');
            if (parts[0].Length != LengthVisitorId)
            {
                return false;
            }
            _cookieVisitorId = parts[0]; // provides backward compatibility since getVisitorId() didn't change any existing VisitorId value
            _createTs = long.Parse(parts[1]);
            if (!string.IsNullOrWhiteSpace(parts[2]))
            {
                _visitCount = long.Parse(parts[2]);
            }
            //  _currentVisitTs is set for information / debugging purposes
            _currentVisitTs = long.Parse(parts[3]);
            if (!string.IsNullOrWhiteSpace(parts[4]))
            {
                _lastVisitTs = long.Parse(parts[4]);
            }
            if (!string.IsNullOrWhiteSpace(parts[5]))
            {
                _lastEcommerceOrderTs = long.Parse(parts[5]);
            }
            return true;
        }

        /// <summary>
        /// Deletes all first party cookies from the client
        /// </summary>
        public void DeleteCookies()
        {
            if (HttpContext.Current != null)
            {
                var expire = _currentTs - 86400;
                var cookies = new[] { "id", "ses", "cvar", "ref" };
                foreach (var cookie in cookies)
                {
                    SetCookie(cookie, "", expire);
                }
            }
        }

        /// <summary>
        /// Returns the currently assigned Attribution Information stored in a first party cookie.
        ///
        /// This function will only work if the user is initiating the current request, and his cookies
        /// can be read from an active HttpContext.
        /// </summary>
        /// <returns>Referrer information for Goal conversion attribution. Will return null if the cookie could not be found</returns>
        /// <see>Piwik.js getAttributionInfo()</see>
        public AttributionInfo GetAttributionInfo()
        {
            if (_attributionInfo != null)
            {
                return _attributionInfo;
            }
            var refCookie = GetCookieMatchingName("ref");

            if (refCookie == null)
            {
                return null;
            }

            var cookieDecoded = new JavaScriptSerializer().Deserialize<string[]>(HttpUtility.UrlDecode(refCookie.Value ?? string.Empty));

            if (cookieDecoded == null)
            {
                return null;
            }

            var arraySize = cookieDecoded.Length;

            if (arraySize == 0)
            {
                return null;
            }

            var attributionInfo = new AttributionInfo();

            if (!string.IsNullOrEmpty(cookieDecoded[0]))
            {
                attributionInfo.CampaignName = cookieDecoded[0];
            }

            if (arraySize > 1 && !string.IsNullOrEmpty(cookieDecoded[1]))
            {
                attributionInfo.CampaignKeyword = cookieDecoded[1];
            }

            if (arraySize > 2 && !string.IsNullOrEmpty(cookieDecoded[2]))
            {
                attributionInfo.ReferrerTimestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToInt32(cookieDecoded[2]));
            }

            if (arraySize > 3 && !string.IsNullOrEmpty(cookieDecoded[3]))
            {
                attributionInfo.ReferrerUrl = cookieDecoded[3];
            }

            return attributionInfo;
        }

        /// <summary>
        /// Some Tracking API functionnality requires express authentication, using either the
        /// Super User token_auth, or a user with 'admin' access to the website.
        ///
        /// The following features require access:
        /// - force the visitor IP
        /// - force the date and time of the tracking requests rather than track for the current datetime
        ///
        /// </summary>
        /// <param name="tokenAuth">32 chars token_auth string</param>
	    public void SetTokenAuth(string tokenAuth)
        {
            _tokenAuth = tokenAuth;
        }

        /// <summary>
        /// Sets local visitor time
        /// </summary>
        /// <param name="localTime">Time to set</param>
        public void SetLocalTime(DateTimeOffset localTime)
        {
            _localTime = localTime;
        }

        /// <summary>
        /// Sets user resolution width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void SetResolution(int width, int height)
        {
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Sets if the browser supports cookies
        /// This is reported in "List of plugins" report in Piwik.
        /// </summary>
        /// <param name="hasCookies">if set to <c>true</c> [has cookies].</param>
        public void SetBrowserHasCookies(bool hasCookies)
        {
            _hasCookies = hasCookies;
        }

        /// <summary>
        /// Will append a custom string at the end of the Tracking request.
        /// </summary>
        public void SetDebugStringAppend(string debugString)
        {
            _debugAppendUrl = "&" + debugString;
        }

        /// <summary>
        /// Sets visitor browser supported plugins
        /// </summary>
        /// <param name="browserPlugins">The browser plugins.</param>
        public void SetPlugins(BrowserPlugins browserPlugins)
        {
            _plugins =
                "&fla=" + (browserPlugins.Flash ? "1" : "0") +
                "&java=" + (browserPlugins.Java ? "1" : "0") +
                "&dir=" + (browserPlugins.Director ? "1" : "0") +
                "&qt=" + (browserPlugins.QuickTime ? "1" : "0") +
                "&realp=" + (browserPlugins.RealPlayer ? "1" : "0") +
                "&pdf=" + (browserPlugins.Pdf ? "1" : "0") +
                "&wma=" + (browserPlugins.WindowsMedia ? "1" : "0") +
                "&gears=" + (browserPlugins.Gears ? "1" : "0") +
                "&ag=" + (browserPlugins.Silverlight ? "1" : "0");
        }

        /// <summary>
        /// By default, PiwikTracker will read first party cookies
        /// from the request and write updated cookies in the response (using setrawcookie).
        /// This can be disabled by calling this function.
        /// </summary>
        public void DisableCookieSupport()
        {
            _configCookiesDisabled = true;
        }

        private TrackingResponse SendRequest(string url, string method = "GET", string data = null, bool force = false)
        {
            // if doing a bulk request, store the url
            if (_doBulkRequests && !force)
            {
                _storedTrackingActions.Add(
                    url
                    + (!string.IsNullOrEmpty(_userAgent) ? "&ua=" + UrlEncode(_userAgent) : "")
                    + (!string.IsNullOrEmpty(_acceptLanguage) ? "&lang=" + UrlEncode(_acceptLanguage) : "")
                );

                // Clear custom variables so they don't get copied over to other users in the bulk request
                ClearCustomVariables();
                ClearCustomTrackingParameters();
                _userAgent = null;
                _acceptLanguage = null;
                return null;
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.UserAgent = _userAgent;
            request.Headers.Add("Accept-Language", _acceptLanguage);
            request.Timeout = (int)RequestTimeout.TotalMilliseconds;
            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            if (!string.IsNullOrEmpty(data))
            {
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
            }
            using (var result = (HttpWebResponse)request.GetResponse())
            {
                return new TrackingResponse { HttpStatusCode = result.StatusCode, RequestedUrl = url };
            }
        }

        /// <summary>
        /// Returns the base URL for the piwik server.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static string FixPiwikBaseUrl(string url)
        {
            if (!url.Contains("/piwik.php") && !url.Contains("/proxy-piwik.php")
            )
            {
                url += "/piwik.php";
            }
            return url;
        }

        internal string GetRequest(int idSite)
        {
            SetFirstPartyCookies();

            var customFields = "";
            if (_customParameters.Any())
            {
                foreach (var kvp in _customParameters)
                {
                    customFields += string.Format("&{0}={1}", UrlEncode(kvp.Key), UrlEncode(kvp.Value));
                }
            }
            // http://developer.piwik.org/api-reference/tracking-api
            // Required parameters:
            //     idsite(required) — The ID of the website we're tracking a visit/action for.
            //     rec(required) — Required for tracking, must be set to one, eg, &rec = 1.
            //     url(required) — The full URL for the current action.
            // Recommended parameters
            //      action_name(recommended) — The title of the action being tracked.It is possible to use slashes / to set one or several categories for this action.For example, Help / Feedback will create the Action Feedback in the category Help.
            //      _id(recommended) — The unique visitor ID, must be a 16 characters hexadecimal string.Every unique visitor must be assigned a different ID and this ID must not change after it is assigned.If this value is not set Piwik will still track visits, but the unique visitors metric might be less accurate.
            //      rand(recommended) — Meant to hold a random value that is generated before each request.Using it helps avoid the tracking request being cached by the browser or a proxy.
            //      apiv(recommended) — The parameter & apiv = 1 defines the api version to use(currently always set to 1)

            var url = PiwikBaseUrl +
                    "?idsite=" + idSite +
                    "&rec=1" +
                    "&apiv=" + Version +
                    "&r=" + new Random().Next(0, 1000000).ToString("000000") +

                    // Only allowed for Super User, token_auth required,
                    (!string.IsNullOrEmpty(_ip) ? "&cip=" + _ip : "") +
                    (!string.IsNullOrEmpty(_userId) ? "&uid=" + UrlEncode(_userId) : "") +
                    (!_forcedDatetime.Equals(DateTimeOffset.MinValue) ? "&cdt=" + FormatDateValue(_forcedDatetime) : "") +
                    (_forcedNewVisit ? "&new_visit=1" : "") +
                    (!string.IsNullOrEmpty(_tokenAuth) && !_doBulkRequests ? "&token_auth=" + UrlEncode(_tokenAuth) : "") +

                    // Values collected from cookie
                    "&_idts=" + _createTs +
                    "&_idvc=" + _visitCount +
                    ((_lastVisitTs != null) ? "&_viewts=" + _lastVisitTs : "") +
                    ((_lastEcommerceOrderTs != null) ? "&_ects=" + _lastEcommerceOrderTs : "") +

                    // These parameters are set by the JS, but optional when using API
                    (!string.IsNullOrEmpty(_plugins) ? _plugins : "") +
                    (!_localTime.Equals(DateTimeOffset.MinValue) ? "&h=" + _localTime.Hour + "&m=" + _localTime.Minute + "&s=" + _localTime.Second : "") +
                    ((_width != 0 && _height != 0) ? "&res=" + _width + "x" + _height : "") +
                    (_hasCookies ? "&cookie=1" : "") +
                    (!_ecommerceLastOrderTimestamp.Equals(DateTimeOffset.MinValue) ? "&_ects=" + FormatTimestamp(_ecommerceLastOrderTimestamp) : "") +

                    // Various important attributes
                    // todo _customData is never assigned!
                    (!string.IsNullOrEmpty(_customData) ? "&data=" + _customData : "") +
                    (_visitorCustomVar.Any() ? "&_cvar=" + UrlEncode(new JavaScriptSerializer().Serialize(_visitorCustomVar)) : "") +
                    (_pageCustomVar.Any() ? "&cvar=" + UrlEncode(new JavaScriptSerializer().Serialize(_pageCustomVar)) : "") +
                    (_eventCustomVar.Any() ? "&e_cvar=" + UrlEncode(new JavaScriptSerializer().Serialize(_eventCustomVar)) : "") +
                    (_generationTime != null ? "&gt_ms=" + _generationTime : "") +
                    (!string.IsNullOrEmpty(_forcedVisitorId) ? "&cid=" + _forcedVisitorId : "&_id=" + GetVisitorId()) +

                    // URL parameters
                    (!string.IsNullOrEmpty(_pageUrl) ? "&url=" + UrlEncode(_pageUrl) : "") +
                    (!string.IsNullOrEmpty(_referrerUrl) ? "&urlref=" + UrlEncode(_referrerUrl) : "") +
                    (!string.IsNullOrEmpty(_pageCharset) && !_pageCharset.Equals(DefaultCharsetParameterValues) ? "&cs=" + _pageCharset : "") +

                    // Attribution information, so that Goal conversions are attributed to the right referrer or campaign
                    // Campaign name
                    ((_attributionInfo != null && !string.IsNullOrEmpty(_attributionInfo.CampaignName)) ? "&_rcn=" + UrlEncode(_attributionInfo.CampaignName) : "") +
                    // Campaign keyword
                    ((_attributionInfo != null && !string.IsNullOrEmpty(_attributionInfo.CampaignKeyword)) ? "&_rck=" + UrlEncode(_attributionInfo.CampaignKeyword) : "") +
                    // Timestamp at which the referrer was set
                    ((_attributionInfo != null && !_attributionInfo.ReferrerTimestamp.Equals(DateTimeOffset.MinValue)) ? "&_refts=" + FormatTimestamp(_attributionInfo.ReferrerTimestamp) : "") +
                    // Referrer URL
                    ((_attributionInfo != null && !string.IsNullOrEmpty(_attributionInfo.ReferrerUrl)) ? "&_ref=" + UrlEncode(_attributionInfo.ReferrerUrl) : "") +

                    // custom location info
                    (!string.IsNullOrEmpty(_country) ? "&country=" + UrlEncode(_country) : "") +
                    (!string.IsNullOrEmpty(_region) ? "&region=" + UrlEncode(_region) : "") +
                    (!string.IsNullOrEmpty(_city) ? "&city=" + UrlEncode(_city) : "") +
                    (_latitude != null ? "&lat=" + FormatGeoLocationValue((float)_latitude) : "") +
                    (_longitude != null ? "&long=" + FormatGeoLocationValue((float)_longitude) : "") +
                    customFields +
                    (!_sendImageResponse ? "&send_image=0" : "") +

                    // DEBUG
                    _debugAppendUrl;

            // Reset page level custom variables after this page view
            _pageCustomVar = new Dictionary<string, string[]>();
            _eventCustomVar = new Dictionary<string, string[]>();
            ClearCustomTrackingParameters();

            // force new visit only once, user must call again setForceNewVisit()
            _forcedNewVisit = false;

            return url;
        }

        private HttpCookie GetCookieMatchingName(string name)
        {
            if (_configCookiesDisabled)
            {
                return null;
            }
            name = GetCookieName(name);

            if (HttpContext.Current != null)
            {
                var cookies = HttpContext.Current.Request.Cookies;
                for (var i = 0; i < cookies.Count; i++)
                {
                    if (cookies[i].Name.Contains(name))
                    {
                        return cookies[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// If current URL is <![CDATA[http://example.org/dir1/dir2/index.php?param1=value1&param2=value2]]>
        /// will return "/dir1/dir2/index.php"
        /// </summary>
        protected static string GetCurrentScriptName()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.AbsolutePath;
            }
            return "";
        }

        /// <summary>
        /// If the current URL is <![CDATA[http://example.org/dir1/dir2/index.php?param1=value1&param2=value2]]>
        /// will return 'http'.
        /// </summary>
        /// <returns>string 'https' or 'http'</returns>
        protected static string GetCurrentScheme()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.Scheme;
            }
            return "http";
        }

        /// <summary>
        /// If current URL is <![CDATA[http://example.org/dir1/dir2/index.php?param1=value1&param2=value2]]>
        /// will return <![CDATA[http://example.org]]>.
        /// </summary>
        /// <returns>string 'https' or 'http'</returns>
        protected static string GetCurrentHost()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.Host;
            }
            return "unknown";
        }

        /// <summary>
        /// If current URL is <![CDATA[http://example.org/dir1/dir2/index.php?param1=value1&param2=value2]]>.
        /// will return <![CDATA[?param1=value1&param2=value2]]>.
        /// </summary>
        protected static string GetCurrentQueryString()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.Query;
            }
            return "";
        }

        /// <summary>
        /// Returns the current full URL (scheme, host, path and query string.
        /// </summary>
        protected static string GetCurrentUrl()
        {
            return GetCurrentScheme() + "://"
                + GetCurrentHost()
                + GetCurrentScriptName()
                + GetCurrentQueryString();
        }

        /// <summary>
        /// Sets the first party cookies as would the piwik.js
        /// All cookies are supported: 'id' and 'ses' and 'ref' and 'cvar' cookies.
        /// </summary>
        protected void SetFirstPartyCookies()
        {
            if (_configCookiesDisabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_cookieVisitorId))
            {
                LoadVisitorIdCookie();
            }

            // Set the 'ref' cookie
            var attributionInfo = GetAttributionInfo();
            if (attributionInfo != null)
            {
                SetCookie("ref", UrlEncode(new JavaScriptSerializer().Serialize(attributionInfo.ToArray())), ConfigReferralCookieTimeout);
            }

            // Set the 'ses' cookie
            SetCookie("ses", "*", ConfigSessionCookieTimeout);

            // Set the 'id' cookie
            var visitCount = _visitCount + 1;
            var cookieValue = GetVisitorId() + "." + _createTs + "." + visitCount + "." + _currentTs + "." + _lastVisitTs + "." + _lastEcommerceOrderTs;
            SetCookie("id", cookieValue, ConfigVisitorCookieTimeout);

            // Set the 'cvar' cookie
            SetCookie("cvar", UrlEncode(new JavaScriptSerializer().Serialize(_visitorCustomVar)), ConfigSessionCookieTimeout);
        }

        /// <summary>
        /// Sets a first party cookie to the client to improve dual JS-PHP tracking.
        /// This replicates the piwik.js tracker algorithms for consistency and better accuracy.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="cookieValue">The cookie value.</param>
        /// <param name="cookieTtl">The cookie TTL.</param>
        protected void SetCookie(string cookieName, string cookieValue, long cookieTtl)
        {
            if (HttpContext.Current != null)
            {
                var cookieExpire = _currentTs + cookieTtl;
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(GetCookieName(cookieName), cookieValue) { Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(cookieExpire), Path = _configCookiePath, Domain = _configCookieDomain });
            }
        }

        /// <summary>
        /// Gets the custom variables from cookie.
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, string[]> GetCustomVariablesFromCookie()
        {
            var cookie = GetCookieMatchingName("cvar");
            if (cookie == null)
            {
                return new Dictionary<string, string[]>();
            }
            return new JavaScriptSerializer().Deserialize<Dictionary<string, string[]>>(HttpUtility.UrlDecode(cookie.Value ?? string.Empty));
        }

        private string FormatDateValue(DateTimeOffset date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string FormatTimestamp(DateTimeOffset date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            double seconds = Convert.ToInt32(diff.TotalSeconds);
            return seconds.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatMonetaryValue(double value)
        {
            return value.ToString("0.##", new CultureInfo("en-US"));
        }

        private string FormatGeoLocationValue(float value)
        {
            return value.ToString(new CultureInfo("en-US"));
        }

        private string UrlEncode(string value)
        {
            return HttpUtility.UrlEncode(value);
        }
    }
}