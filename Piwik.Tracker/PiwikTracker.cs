// <summary>
// Piwik - free/libre analytics platform
// 
// Client to record visits, page views, Goals, Ecommerce activity (product views, add to carts, Ecommerce orders) in a Piwik server.
// This is a C# Version of the piwik.js standard Tracking API.
// For more information, see http://piwik.org/docs/tracking-api/
// 
// <see href="http://www.opensource.org/licenses/bsd-license.php">released under BSD License</see>
// <see href="http://piwik.org/docs/tracking-api/"/>
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
    ///      // In the standard JS API, the content of the <title> tag would be set as the page title
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
        private const int VERSION = 1;

        /// <summary>
        /// Visitor ID length
        /// </summary>
        private const int LENGTH_VISITOR_ID = 16;

        /// <summary>
        /// Charset
        /// <see cref="setPageCharset"/>
        /// </summary>
	    private const string DEFAULT_CHARSET_PARAMETER_VALUES = "utf-8";

        /// <summary>
        /// <see cref="piwik.js"/>
        /// </summary>
        private const string FIRST_PARTY_COOKIES_PREFIX = "_pk_";

        /// <summary>
        /// Ecommerce item page view tracking stores item's metadata in these Custom Variables slots.
        /// </summary>
        private const int CVAR_INDEX_ECOMMERCE_ITEM_PRICE = 2;
        private const int CVAR_INDEX_ECOMMERCE_ITEM_SKU = 3;
        private const int CVAR_INDEX_ECOMMERCE_ITEM_NAME = 4;
        private const int CVAR_INDEX_ECOMMERCE_ITEM_CATEGORY = 5;

        private const string DEFAULT_COOKIE_PATH = "/";

        /// <summary>
        /// Piwik base URL, for example http://example.org/piwik/
        /// Must be set before using the class by calling PiwikTracker.URL = 'http://yourwebsite.org/piwik/'
        /// </summary>
        public static string URL;

        private string DEBUG_APPEND_URL;
        private string userAgent;
        private DateTimeOffset localTime;
        private bool hasCookies;
        private string plugins;
        private Dictionary<string, string[]> visitorCustomVar;
        private Dictionary<string, string[]> pageCustomVar;
        private Dictionary<string, string[]> eventCustomVar;
        private string customData;
        private DateTimeOffset forcedDatetime;
        private bool forcedNewVisit;
        private string token_auth;
        private AttributionInfo attributionInfo;
        private DateTimeOffset ecommerceLastOrderTimestamp;
        private Dictionary<string, object[]> ecommerceItems;
        private int? generationTime;
        private int idSite;
        private string urlReferrer;
        private string pageCharset;
        private string pageUrl;
        private string ip;
        private string acceptLanguage;
        private string userId;
        private string forcedVisitorId;
        private string cookieVisitorId;
        private string randomVisitorId;
        private int width;
        private int height;
        private int requestTimeout;
        private bool doBulkRequests;
        private List<string> storedTrackingActions;
        private string country;
        private string region;
        private string city;
        private float? lat;
        private float? longitude;
        private int configVisitorCookieTimeout;
        private int configSessionCookieTimeout;
        private int configReferralCookieTimeout;
        private bool configCookiesDisabled;
        private string configCookiePath;
        private string configCookieDomain;
        private long currentTs;
        private long createTs;
        private long? visitCount;
        private long? currentVisitTs;
        private long? lastVisitTs;
        private long? lastEcommerceOrderTs;

        public enum ActionType {download, link};

        /// <summary>
        /// Builds a PiwikTracker object, used to track visits, pages and Goal conversions
        /// for a specific website, by using the Piwik Tracking API.
        /// 
        /// If the tracker is used within a web page or web controller, the following information are pre-initialised : 
        /// URL Referrer, current page URL, remote IP, Accept-Language HTTP header and User-Agent HTTP header.
        /// </summary>       
        /// <param name="idSite">Id site to be tracked</param>
        /// <param name="apiUrl">"http://example.org/piwik/" or "http://piwik.example.org/". If set, will overwrite PiwikTracker.URL</param>
        public PiwikTracker(int idSite, string apiUrl = "")
        {
            this.userAgent = null;
            this.localTime = DateTimeOffset.MinValue;
            this.hasCookies = false;
            this.plugins = null;            
            this.pageCustomVar = new Dictionary<string, string[]>();
            this.eventCustomVar = new Dictionary<string, string[]>();
            this.customData = null;
            this.forcedDatetime = DateTimeOffset.MinValue;
            this.forcedNewVisit = false;
            this.token_auth = null;
            this.attributionInfo = null;
            this.ecommerceLastOrderTimestamp = DateTimeOffset.MinValue;
            this.ecommerceItems =  new Dictionary<string, object[]>();
            this.generationTime = null;

            this.idSite = idSite;
            var currentContext = HttpContext.Current;
            if (currentContext != null) {
                if (currentContext.Request.UrlReferrer != null) {
                    this.urlReferrer = currentContext.Request.UrlReferrer.AbsoluteUri;
                }
                
                this.ip = currentContext.Request.UserHostAddress;

                if (currentContext.Request.UserLanguages != null && currentContext.Request.UserLanguages.Any())
                    this.acceptLanguage = currentContext.Request.UserLanguages.First();

                this.userAgent = currentContext.Request.UserAgent;
            }
            this.pageCharset = DEFAULT_CHARSET_PARAMETER_VALUES;
            this.pageUrl = getCurrentUrl();
            if (!String.IsNullOrEmpty(apiUrl)) {
                URL = apiUrl;
            }

            // Life of the visitor cookie (in sec)
            this.configVisitorCookieTimeout = 63072000; // 2 years
            // Life of the session cookie (in sec)
            this.configSessionCookieTimeout = 1800; // 30 minutes
            // Life of the session cookie (in sec)
            this.configReferralCookieTimeout = 15768000; // 6 months

            // Visitor Ids in order
            this.userId = null;
            this.forcedVisitorId = null;
            this.cookieVisitorId = null;
            this.randomVisitorId = null;

            this.setNewVisitorId();

            this.configCookiesDisabled = false;
            this.configCookiePath = DEFAULT_COOKIE_PATH;
            this.configCookieDomain = "";

            this.currentTs = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            this.createTs = this.currentTs;
            this.visitCount = 0;
            this.currentVisitTs = null;
            this.lastVisitTs = null;
            this.lastEcommerceOrderTs = null;

		    // Allow debug while blocking the request
    	    this.requestTimeout = 600;
    	    this.doBulkRequests = false;
    	    this.storedTrackingActions = new List<string>();

            this.visitorCustomVar = this.getCustomVariablesFromCookie();
        }


        /// <summary>
        /// By default, Piwik expects utf-8 encoded values, for example
        /// for the page URL parameter values, Page Title, etc.
        /// It is recommended to only send UTF-8 data to Piwik.
        /// If required though, you can also specify another charset using this function.
        /// </summary>       
	    public void setPageCharset(string charset = "")
	    {
		    this.pageCharset = charset;
	    }


        /// <summary>
        /// Sets the current URL being tracked
        /// </summary>       
        /// <param name="url">Raw URL (not URL encoded)</param>
        public void setUrl(string url)
        {
            pageUrl = url;
        }


        /// <summary>
        /// Sets the URL referrer used to track Referrers details for new visits.
        /// </summary>       
        /// <param name="url">Raw URL (not URL encoded)</param>
        public void setUrlReferrer(string url)
        {
            urlReferrer = url;
        }


        /// <summary>
        /// Sets the time that generating the document on the server side took.
        /// </summary>       
        /// <param name="timeMs">Generation time in ms</param>
	    public void setGenerationTime(int timeMs)
	    {
		    this.generationTime = timeMs;
	    }


        /// <summary>
        /// Sets the attribution information to the visit, so that subsequent Goal conversions are 
        /// properly attributed to the right Referrer URL, timestamp, Campaign Name & Keyword.
        /// 
        /// If you call enableCookies() then these referral attribution values will be set
        /// to the 'ref' first party cookie storing referral information.
        /// </summary>       
        /// <param name="attributionInfo">Attribution info for the visit</param>        
        /// <see>function getAttributionInfo() in "https://github.com/piwik/piwik/blob/master/js/piwik.js"</see>
        public void setAttributionInfo(AttributionInfo attributionInfo)
        {
            this.attributionInfo = attributionInfo;
        }


        /// <summary>
        /// Sets Visit Custom Variable.
        /// See http://piwik.org/docs/custom-variables/
        /// </summary>       
        /// <param name="id">Custom variable slot ID from 1-5</param>
        /// <param name="name">Custom variable name</param>
        /// <param name="value">Custom variable value</param>
        /// <param name="scope">Custom variable scope. Possible values: visit, page, event</param>
        /// <exception cref="Exception"/>
        public void setCustomVariable(int id, string name, string value, CustomVar.Scopes scope = CustomVar.Scopes.visit)
        {
            string stringId = Convert.ToString(id);
            string[] customVar = {name, value};

            switch (scope)
            {
                case CustomVar.Scopes.page:
                    pageCustomVar[stringId] = customVar;
                    break;

                case CustomVar.Scopes.visit:
                    visitorCustomVar[stringId] = customVar;
                    break;

                case CustomVar.Scopes._event:
                    eventCustomVar[stringId] = customVar;
                    break;

                default:
                    throw new Exception("Invalid 'scope' parameter value");
            }
        }


        /// <summary>
        /// Returns the currently assigned Custom Variable.
        /// 
        /// If scope is 'visit', it will attempt to read the value set in the first party cookie created by Piwik Tracker ($_COOKIE array).
        /// </summary>       
        /// <param name="id">Custom Variable integer index to fetch from cookie. Should be a value from 1 to 5</param>
        /// <param name="scope">Custom variable scope. Possible values: visit, page, event</param> 
        /// <exception cref="Exception"/>
        /// <returns>The requested custom variable</returns>
        public CustomVar getCustomVariable(int id, CustomVar.Scopes scope = CustomVar.Scopes.visit)
        {
            var stringId = Convert.ToString(id);

            if (scope.Equals(CustomVar.Scopes.page)) {
                return pageCustomVar.ContainsKey(stringId) ? new CustomVar(pageCustomVar[stringId][0], pageCustomVar[stringId][1]) : null;
            }
            else if (!scope.Equals(CustomVar.Scopes._event)) {
                return eventCustomVar.ContainsKey(stringId) ? new CustomVar(eventCustomVar[stringId][0], eventCustomVar[stringId][1]) : null;
            }
            else if (!scope.Equals(CustomVar.Scopes.visit)) {
                throw new Exception("Invalid 'scope' parameter value");
            }
            if (this.visitorCustomVar.ContainsKey(stringId)) {
                return new CustomVar(visitorCustomVar[stringId][0], visitorCustomVar[stringId][1]);
            }
            var cookieDecoded = this.getCustomVariablesFromCookie();
            if (!cookieDecoded.ContainsKey(stringId)    		
    		    || cookieDecoded[stringId].Count() != 2) {
    		    return null;
    	    }
            return new CustomVar(cookieDecoded[stringId][0], cookieDecoded[stringId][1]);
        }


        /// <summary>
        /// Clears any Custom Variable that may be have been set.
        /// 
        /// This can be useful when you have enabled bulk requests,
        /// and you wish to clear Custom Variables of 'visit' scope.
        /// </summary> 
        public void clearCustomVariables()
        {
            this.visitorCustomVar = new Dictionary<string, string[]>();
            this.pageCustomVar = new Dictionary<string, string[]>();
            this.eventCustomVar = new Dictionary<string, string[]>();
        }

        
         /// <summary>
        /// Sets the current visitor ID to a random new one.
        /// </summary>       
        public void setNewVisitorId()
        {
            var encodedGuidBytes = new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(Guid.NewGuid().ToString()));
            this.randomVisitorId = BitConverter.ToString(encodedGuidBytes).Replace("-", "").Substring(0, LENGTH_VISITOR_ID).ToLower();
            this.userId = null;
            this.forcedVisitorId = null;
            this.cookieVisitorId = null;
        }

    
        /// <summary>
        /// Sets the current site ID.
        /// </summary>       
        /// <param name="idSite"/>
        public void setIdSite(int idSite)
        {
    	    this.idSite = idSite;
        }
    

        /// <summary>
        /// Sets the Browser language. Used to guess visitor countries when GeoIP is not enabled
        /// </summary>       
        /// <param name="acceptLanguage">For example "fr-fr"</param>    
        public void setBrowserLanguage(string acceptLanguage)
        {
            this.acceptLanguage = acceptLanguage;
        }


        /// <summary>
        /// Sets the user agent, used to detect OS and browser.
        /// If this function is not called, the User Agent will default to the current user agent
        /// if there is an active HttpContext
        /// </summary>       
        /// <param name="userAgent">HTTP User Agent</param>  
        public void setUserAgent(string userAgent)
        {
            this.userAgent = userAgent;
        }


        /// <summary>
        /// Sets the country of the visitor. If not used, Piwik will try to find the country
        /// using either the visitor's IP address or language.
        /// 
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>       
        public void setCountry(string country)
        {
    	    this.country = country;
        }
    

        /// <summary>
        /// Sets the region of the visitor. If not used, Piwik may try to find the region
        /// using the visitor's IP address (if configured to do so).
        /// 
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>       
        public void setRegion(string region)
        {
    	    this.region = region;
        }

    
        /// <summary>
        /// Sets the city of the visitor. If not used, Piwik may try to find the city
        /// using the visitor's IP address (if configured to do so).
        /// 
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>       
        public void setCity(string city)
        {
    	    this.city = city;
        }

    
        /// <summary>
        /// Sets the latitude of the visitor. If not used, Piwik may try to find the visitor's
        /// latitude using the visitor's IP address (if configured to do so).
        /// 
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>  
        public void setLatitude(float lat)
        {
    	    this.lat = lat;
        }
    

        /// <summary>
        /// Sets the longitude of the visitor. If not used, Piwik may try to find the visitor's
        /// longitude using the visitor's IP address (if configured to do so).
        /// 
        /// Allowed only for Admin/Super User, must be used along with setTokenAuth().
        /// </summary>  
        public void setLongitude(float longitude)
        {
    	    this.longitude = longitude;
        }
    
        
        /// <summary>
        /// Enables the bulk request feature. When used, each tracking action is stored until the
        /// doBulkTrack method is called. This method will send all tracking data at once.
        /// </summary>     
	    public void enableBulkTracking()
	    {
		    this.doBulkRequests = true;
	    }


        /// <summary>
        /// Enable Cookie Creation - this will cause a first party VisitorId cookie to be set when the VisitorId is set or reset
        /// </summary>       
        /// <param name="domain">(optional) Set first-party cookie domain. Accepted values: example.com, *.example.com (same as .example.com) or subdomain.example.com</param>    
        /// <param name="path">(optional) Set first-party cookie path</param>    
        public void enableCookies(string domain = "", string path = "/" )
        {
            this.configCookiesDisabled = false;
            this.configCookieDomain = domainFixup(domain);
            this.configCookiePath = path;
        }


        /// <summary>
        /// Fix-up domain
        /// </summary>  
        static protected string domainFixup(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)){
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
        /// <summary>
        protected string getCookieName(string cookieName) {
            // NOTE: If the cookie name is changed, we must also update the method in piwik.js with the same name.
            var hash = getHexStringFromBytes(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes((string.IsNullOrWhiteSpace(this.configCookieDomain) ? getCurrentHost() : this.configCookieDomain) + this.configCookiePath))).Substring(0, 4);
            return FIRST_PARTY_COOKIES_PREFIX + cookieName + "." + this.idSite + "." + hash;
        }

        protected string getHexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        } 

        /// <summary>
        /// Tracks a page view
        /// </summary>       
        /// <param name="documentTitle">Page title as it will appear in the Actions > Page titles report</param> 
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public HttpWebResponse doTrackPageView(string documentTitle = null)
        {
            string url = getUrlTrackPageView(documentTitle);
            return sendRequest(url);
        }


        /// <summary>
        /// Tracks an event
        /// </summary>       
        /// <param name="category">The Event Category (Videos, Music, Games...)</param> 
        /// <param name="action">The Event's Action (Play, Pause, Duration, Add Playlist, Downloaded, Clicked...)</param> 
        /// <param name="name">(optional) The Event's object Name (a particular Movie name, or Song name, or File name...)</param> 
        /// <param name="value">(optional) The Event's value</param> 
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public HttpWebResponse doTrackEvent(string category, string action, string name = "", string value = "")
        {
            var url = this.getUrlTrackEvent(category, action, name, value);
            return this.sendRequest(url);
        }


        /// <summary>
        /// Tracks an internal Site Search query, and optionally tracks the Search Category, and Search results Count.
        /// These are used to populate reports in Actions > Site Search.
        /// </summary>       
        /// <param name="keyword">Searched query on the site</param> 
        /// <param name="category">(optional) Search engine category if applicable</param> 
        /// <param name="countResults">(optional) results displayed on the search result page. Used to track "zero result" keywords.</param> 
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
	    public HttpWebResponse doTrackSiteSearch(string keyword, string category = "", int? countResults = null)
	    {
		    var url = this.getUrlTrackSiteSearch(keyword, category, countResults);
		    return this.sendRequest(url);
	    }


        /// <summary>
        /// Records a Goal conversion
        /// </summary>       
        /// <param name="idGoal">Id Goal to record a conversion</param> 
        /// <param name="revenue">Revenue for this conversion</param> 
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public HttpWebResponse doTrackGoal(int idGoal, float revenue = 0)
        {
    	    string url = getUrlTrackGoal(idGoal, revenue);
    	    return sendRequest(url);
        }


        /// <summary>
        /// Tracks a download or outlink
        /// </summary>       
        /// <param name="actionUrl">URL of the download or outlink</param> 
        /// <param name="actionType">Type of the action: 'download' or 'link'</param> 
        /// <returns>HTTP Response from the server or null if using bulk requests.</returns>
        public HttpWebResponse doTrackAction(string actionUrl, ActionType actionType)
        {
            // Referrer could be udpated to be the current URL temporarily (to mimic JS behavior)
    	    string url = getUrlTrackAction(actionUrl, actionType);
    	    return sendRequest(url); 
        }


        /// <summary>
        /// Adds an item in the Ecommerce order.
        /// 
        /// This should be called before doTrackEcommerceOrder(), or before doTrackEcommerceCartUpdate().
        /// This function can be called for all individual products in the cart (or order).
        /// SKU parameter is mandatory. Other parameters are optional (set to false if value not known).
        /// Ecommerce items added via this function are automatically cleared when doTrackEcommerceOrder() or getUrlTrackEcommerceOrder() is called.
        /// </summary>       
        /// <param name="sku">SKU, Product identifier </param> 
        /// <param name="name">Product name</param> 
        /// <param name="categories">Array of product categories (up to 5 categories can be specified for a given product)</param> 
        /// <param name="price"> Individual product price (supports integer and decimal prices)</param> 
        /// <param name="quantity">Product quantity. If not specified, will default to 1 in the Reports</param> 
        /// <exception cref="Exception"/>
        public void addEcommerceItem(string sku, string name = "", List<string> categories = null, double price = 0, ulong quantity = 1)
        {
    	    if (string.IsNullOrEmpty(sku)) {
    		    throw new Exception("You must specify a SKU for the Ecommerce item");
    	    }

            object[] eCommerceItem = { sku, name, categories, formatMonetaryValue(price), quantity };

            ecommerceItems.Remove(sku);
            ecommerceItems.Add(sku, eCommerceItem);
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
        public HttpWebResponse doTrackEcommerceCartUpdate(double grandTotal)
        {
    	    string url = getUrlTrackEcommerceCartUpdate(grandTotal);
    	    return sendRequest(url); 
        }

        /// <summary>
        /// Sends all stored tracking actions at once. Only has an effect if bulk tracking is enabled.
        /// 
        /// To enable bulk tracking, call enableBulkTracking().
        /// </summary>   
        /// <exception cref="Exception"/>    
        /// <returns>Response</returns>
        public HttpWebResponse doBulkTrack()
        {
    	    if (!this.storedTrackingActions.Any()) {
                throw new Exception("Error:  you must call the function doTrackPageView or doTrackGoal from this class, before calling this method doBulkTrack()");
    	    }
    	
    	    var data = new Dictionary<string, Object>();
            data["requests"] = this.storedTrackingActions;

            // token_auth is not required by default, except if bulk_requests_require_authentication=1
            if(!string.IsNullOrWhiteSpace(this.token_auth)) {
                data["token_auth"] = this.token_auth;
            }            
    	
    	    var postData = new JavaScriptSerializer().Serialize(data);
    	    var response = this.sendRequest(this.getBaseUrl(), "POST", postData, true);
    	
    	    this.storedTrackingActions = new List<string>();
    	
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
        public HttpWebResponse doTrackEcommerceOrder(string orderId, double grandTotal, double subTotal = 0, double tax = 0, double shipping = 0, double discount = 0)
        {
    	    string url = getUrlTrackEcommerceOrder(orderId, grandTotal, subTotal, tax, shipping, discount);
    	    return sendRequest(url); 
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
        /// Tracking Product/Category page views will allow Piwik to report on Product & Categories 
        /// conversion rates (Conversion rate = Ecommerce orders containing this product or category / Visits to the product or category)
        ///
        /// </summary>       
        /// <param name="sku">Product SKU being viewed</param> 
        /// <param name="name">Product Name being viewed</param> 
        /// <param name="categories">Category being viewed. On a Product page, this is the product's category. You can also specify an array of up to 5 categories for a given page view.</param> 
        /// <param name="price">Specify the price at which the item was displayed</param> 
        public void setEcommerceView(string sku = "", string name = "", List<string> categories = null, double price = 0)
        {
            var serializedCategories = "";
            if (categories != null) {
                serializedCategories = new JavaScriptSerializer().Serialize(categories);
            }
            this.setCustomVariable(CVAR_INDEX_ECOMMERCE_ITEM_CATEGORY, "_pkc", serializedCategories, CustomVar.Scopes.page);

            if (!price.Equals(0)) {
                this.setCustomVariable(CVAR_INDEX_ECOMMERCE_ITEM_PRICE, "_pkp", this.formatMonetaryValue(price), CustomVar.Scopes.page);
            }

            // On a category page, do not record "Product name not defined" 
            if (string.IsNullOrEmpty(sku) && string.IsNullOrEmpty(name)) {
                return;
            }
            if (!string.IsNullOrEmpty(sku)) {
                this.setCustomVariable(CVAR_INDEX_ECOMMERCE_ITEM_SKU, "_pks", sku, CustomVar.Scopes.page);
            }
            if (string.IsNullOrEmpty(name)) {
                name = "";
            }
            this.setCustomVariable(CVAR_INDEX_ECOMMERCE_ITEM_NAME, "_pkn", name, CustomVar.Scopes.page);
        }


        /// <summary>
        /// Returns URL used to track Ecommerce Cart updates
        /// Calling this function will reinitializes the property ecommerceItems to empty array 
        /// so items will have to be added again via addEcommerceItem()  
        /// </summary>        
        private string getUrlTrackEcommerceCartUpdate(double grandTotal)
        {
        	return getUrlTrackEcommerce(grandTotal);
        }


        /// <summary>
        /// Returns URL used to track Ecommerce Orders
        /// Calling this function will reinitializes the property ecommerceItems to empty array 
        /// so items will have to be added again via addEcommerceItem()  
        /// </summary>  
        public string getUrlTrackEcommerceOrder(string orderId, double grandTotal, double subTotal = 0, double tax = 0, double shipping = 0, double discount = 0)
        {
    	    if (String.IsNullOrEmpty(orderId)) {
    		    throw new Exception("You must specifiy an orderId for the Ecommerce order");
    	    }

    	    string url = getUrlTrackEcommerce(grandTotal, subTotal, tax, shipping, discount);
    	    url += "&ec_id=" + urlEncode(orderId);

    	    ecommerceLastOrderTimestamp = forcedDatetime.Equals(DateTimeOffset.MinValue) ? DateTimeOffset.Now : forcedDatetime;

    	    return url;
        }


        /// <summary>
        /// Returns URL used to track Ecommerce orders
        /// Calling this function will reinitializes the property ecommerceItems to empty array 
        /// so items will have to be added again via addEcommerceItem()  
        /// </summary>  
        protected string getUrlTrackEcommerce(double grandTotal, double subTotal = 0, double tax = 0, double shipping = 0, double discount = 0)
        {
    	
    	    string url = getRequest(idSite) + "&idgoal=0&revenue="  + formatMonetaryValue(grandTotal);

            if (!subTotal.Equals(0)) {
    		    url += "&ec_st=" + formatMonetaryValue(subTotal);
    	    }

            if (!tax.Equals(0)) {
    		    url += "&ec_tx=" + formatMonetaryValue(tax);
    	    }

            if (!shipping.Equals(0)) {
    		    url += "&ec_sh="  + formatMonetaryValue(shipping);
    	    }

            if (!discount.Equals(0)) {
    		    url += "&ec_dt=" + formatMonetaryValue(discount);
    	    }

    	    if (ecommerceItems.Count > 0) {
    		    url += "&ec_items=" + urlEncode(new JavaScriptSerializer().Serialize(ecommerceItems.Values));                
    	    }

            ecommerceItems = new Dictionary<string, object[]>();

    	    return url;
        }

        /// <summary>
        /// Builds URL to track a page view.
        /// </summary>
        /// <see cref="doTrackPageView"/>
        /// <param name="documentTitle">Page view name as it will appear in Piwik reports</param> 
        /// <returns>URL to piwik.php with all parameters set to track the pageview</returns>
        public string getUrlTrackPageView(string documentTitle = "")
        {
            var url = getRequest(idSite);

            if (!string.IsNullOrWhiteSpace(documentTitle)) {
                url += "&action_name=" + urlEncode(documentTitle);
            }

            return url;
        }


        /// <summary>
        /// Builds URL to track a custom event.
        /// </summary>
        /// <see cref="doTrackEvent"/>
        /// <param name="category">The Event Category (Videos, Music, Games...)</param> 
        /// <param name="action">The Event's Action (Play, Pause, Duration, Add Playlist, Downloaded, Clicked...)</param> 
        /// <param name="name">(optional) The Event's object Name (a particular Movie name, or Song name, or File name...)</param> 
        /// <param name="value">(optional) The Event's value</param> 
        /// <returns>URL to piwik.php with all parameters set to track the pageview</returns>
        public string getUrlTrackEvent(string category, string action, string name = "", string value = "")
        {
            var url = this.getRequest(this.idSite);
            if(string.IsNullOrWhiteSpace(category)) {
                throw new Exception("You must specify an Event Category name (Music, Videos, Games...).");
            }
            if(string.IsNullOrWhiteSpace(action)) {
                throw new Exception("You must specify an Event action (click, view, add...).");
            }

            url += "&e_c=" + urlEncode(category);
            url += "&e_a=" + urlEncode(action);

            if(!string.IsNullOrWhiteSpace(name)) {
                url += "&e_n=" + urlEncode(name);
            }
            if(!string.IsNullOrWhiteSpace(value)) {
                url += "&e_v=" + value;
            }
            return url;
        }


        /// <summary>
        /// Builds URL to track a site search.
        /// </summary>
        /// <see cref="doTrackSiteSearch"/>
        /// <param name="keyword"/>
        /// <param name="category"/>
        /// <param name="countResults"/>
        public string getUrlTrackSiteSearch(string keyword, string category, int? countResults)
	    {
		    var url = this.getRequest(this.idSite);
		    url += "&search=" + urlEncode(keyword);
		    if (!string.IsNullOrWhiteSpace(category)) {
			    url += "&search_cat=" + urlEncode(category);
		    }
            if (countResults != null) {
                url += "&search_count=" + countResults;
            }
            return url;
	    }


        /// <summary>
        /// Builds URL to track a goal with idGoal and revenue.
        /// </summary>
        /// <see cref="doTrackGoal"/>
        /// <param name="idGoal">Id Goal to record a conversion</param> 
        /// <param name="revenue">Revenue for this conversion</param> 
        /// <returns>URL to piwik.php with all parameters set to track the goal conversion</returns>
        public string getUrlTrackGoal(int idGoal, float revenue = 0)
        {
    	    var url = getRequest(idSite);
            url += "&idgoal=" + idGoal;
    	    if (!revenue.Equals(0)) {
                url += "&revenue=" + formatMonetaryValue(revenue);
    	    }
    	    return url;
        }

        
        /// <summary>
        /// Builds URL to track a new action.
        /// </summary>
        /// <see cref="doTrackAction"/>
        /// <param name="actionUrl">URL of the download or outlink</param> 
        /// <param name="actionType">Type of the action: 'download' or 'link'</param> 
        /// <returns>URL to piwik.php with all parameters set to track an action</returns>
        public string getUrlTrackAction(string actionUrl, ActionType actionType)
        {
    	    var url = getRequest(idSite);
		    url += "&" + actionType + "=" + urlEncode(actionUrl);		
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
        public void setForceVisitDateTime(DateTimeOffset dateTime)
        {
            this.forcedDatetime = dateTime;
        }


        /// <summary>
        /// Forces Piwik to create a new visit for the tracking request.
        /// 
        /// By default, Piwik will create a new visit if the last request by this user was more than 30 minutes ago.
        /// If you call setForceNewVisit() before calling doTrack*, then a new visit will be created for this request.
        /// 
        /// Allowed only for Super User, must be used along with setTokenAuth()
        /// </summary>
        /// <see cref="setTokenAuth"/>
        public void setForceNewVisit()
        {
            this.forcedNewVisit = true;
        }


        /// <summary>
        /// Overrides IP address
        /// 
        /// Allowed only for Super User, must be used along with setTokenAuth()
        /// </summary>
        /// <param name="ip">IP string, eg. 130.54.2.1</param>  
        public void setIp(string ip)
        {
            this.ip = ip;
        }

        /// <summary>
        /// Force the action to be recorded for a specific User. The User ID is a string representing a given user in your system.
        /// 
        /// A User ID can be a username, UUID or an email address, or any number or string that uniquely identifies a user or client.
        /// </summary>
        /// <param name="userId">Any user ID string (eg. email address, ID, username). Must be non empty. Set to false to de-assign a user id previously set.</param>
        /// <exception cref="Exception"/>
        public void setUserId(string userId)
        {
            if (userId == null) {
                this.setNewVisitorId();
                return;
            }
            if (string.IsNullOrEmpty(this.userId)) {
                throw new Exception("User ID cannot be empty.");
            }
            this.userId = userId;
        }


        /// <summary>
        /// Hash function used internally by Piwik to hash a User ID into the Visitor ID.
        /// </summary>
        static public string getUserIdHashed(string id)
        {
            var encodedIdBytes = new SHA1CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(id));
            return BitConverter.ToString(encodedIdBytes).Substring(0, 16);
        }


        /// <summary>
        /// Forces the requests to be recorded for the specified Visitor ID.
        /// Note: it is recommended to use ->setUserId($userId); instead.
        /// 
        /// Rather than letting Piwik attribute the user with a heuristic based on IP and other user fingeprinting attributes,
        /// force the action to be recorded for a particular visitor.
        /// 
        /// If you use both setVisitorId and setUserId, setUserId will take precedence.
        /// If not set, the visitor ID will be fetched from the 1st party cookie, or will be set to a random UUID.
        ///
        /// </summary>       
        /// <param name="visitorId">16 hexadecimal characters visitor ID, eg. "33c31e01394bdc63"</param>          
        /// <exception cref="Exception"/>
        [Obsolete("We recommend to use  ->setUserId($userId).")]
        public void setVisitorId(string visitorId)
        {
            if (visitorId.Length != LENGTH_VISITOR_ID
                || !System.Text.RegularExpressions.Regex.IsMatch(visitorId, @"\A\b[0-9a-fA-F]+\b\Z")
            ) {
        		throw new Exception("setVisitorId() expects a "
                                + LENGTH_VISITOR_ID
                                + " characters hexadecimal string (containing only the following: "
                                + "01234567890abcdefABCDEF"
                                + ")");
        	}

    	    this.forcedVisitorId = visitorId;
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
        public string getVisitorId()
        {
            if (!string.IsNullOrEmpty(this.userId)) {
                return getUserIdHashed(this.userId);
            }
    	    if (!string.IsNullOrEmpty(this.forcedVisitorId)) {
    		    return this.forcedVisitorId;
    	    }
            if (this.loadVisitorIdCookie()) {
                return this.cookieVisitorId;
            }
            return this.randomVisitorId;
        }


        /// <summary>
        /// Returns the User ID string, which may have been set via:
        ///     $v->setUserId('username@example.org');
        /// </summary>
        public string getUserId()
        {
            return this.userId;
        }


        /// <summary>
        /// Loads values from the VisitorId Cookie
        /// </summary>       
        /// <returns>True if cookie exists and is valid, False otherwise</returns>
        protected bool loadVisitorIdCookie() 
        {
            var idCookie = this.getCookieMatchingName("id");
            if (idCookie == null) {
                return false;
            }
            var parts = idCookie.Value.Split('.');
            if (parts[0].Length != LENGTH_VISITOR_ID) {
                return false;
            }
            this.cookieVisitorId = parts[0]; // provides backward compatibility since getVisitorId() didn't change any existing VisitorId value
            this.createTs = long.Parse(parts[1]);
            if (!string.IsNullOrWhiteSpace(parts[2])) {
                this.visitCount = long.Parse(parts[2]);
            }            
            this.currentVisitTs = long.Parse(parts[3]);
            if (!string.IsNullOrWhiteSpace(parts[4])) {
                this.lastVisitTs = long.Parse(parts[4]);
            }
            if (!string.IsNullOrWhiteSpace(parts[5])) {
                this.lastEcommerceOrderTs = long.Parse(parts[5]);
            }
            return true;
        }


        /// <summary>
        /// Deletes all first party cookies from the client
        /// </summary> 
        public void deleteCookies() 
        {
            if (HttpContext.Current != null) {
                var expire = this.currentTs - 86400;
                var cookies = new[] {"id", "ses", "cvar", "ref"};
                foreach(var cookie in cookies) {
                    this.setCookie(cookie, "", expire);
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
        public AttributionInfo getAttributionInfo()
        {
            if(this.attributionInfo != null) {
                return this.attributionInfo;
            }
            var refCookie = getCookieMatchingName("ref");

            if (refCookie == null) {
                return null;
            }

            var cookieDecoded = new JavaScriptSerializer().Deserialize<string[]>(HttpUtility.UrlDecode(refCookie.Value));

            if (cookieDecoded == null) {
                return null;
            }

            var arraySize = cookieDecoded.Length;

            if (arraySize == 0) {
                return null;
            }

            var attributionInfo = new AttributionInfo();

            if (!string.IsNullOrEmpty(cookieDecoded[0])) {
                attributionInfo.campaignName = cookieDecoded[0];
            }

            if (arraySize > 1 && !String.IsNullOrEmpty(cookieDecoded[1])) {
                attributionInfo.campaignKeyword = cookieDecoded[1];
            }

            if (arraySize > 2 && !String.IsNullOrEmpty(cookieDecoded[2])) {
                attributionInfo.referrerTimestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToInt32(cookieDecoded[2]));
            }

            if (arraySize > 3 && !String.IsNullOrEmpty(cookieDecoded[3])) {
                attributionInfo.referrerUrl = cookieDecoded[3];
            }

            return attributionInfo;            
        }


        /// <summary>
        /// Some Tracking API functionnality requires express authentication, using either the 
        /// Super User token_auth, or a user with 'admin' access to the website.
        /// 
        /// The following features require access:
        /// - force the visitor IP
        /// - force the date & time of the tracking requests rather than track for the current datetime
        /// 
        /// </summary>
        /// <param name="token_auth">32 chars token_auth string</param>
	    public void setTokenAuth(string token_auth)
	    {
    		this.token_auth = token_auth;
    	}


        /// <summary>
        /// Sets local visitor time
        /// </summary>
        /// <param name="localTime">Time to set</param>
        public void setLocalTime(DateTimeOffset localTime)
        {
            this.localTime = localTime;
        }


        /// <summary>
        /// Sets user resolution width and height.
        /// </summary>       
        public void setResolution(int width, int height)
        {
    	    this.width = width;
    	    this.height = height;
        }


        /// <summary>
        /// Sets if the browser supports cookies 
        /// This is reported in "List of plugins" report in Piwik.
        /// </summary>  
        public void setBrowserHasCookies(bool hasCookies)
        {
            this.hasCookies = hasCookies;
        }


        /// <summary>
        /// Will append a custom string at the end of the Tracking request. 
        /// </summary> 
        public void setDebugStringAppend(string debugString)
        {
            this.DEBUG_APPEND_URL = "&" + debugString;
        }


        /// <summary>
        /// Sets visitor browser supported plugins
        /// </summary>       
        public void setPlugins(BrowserPlugins browserPlugins)
        {
    	    plugins =
                "&fla=" + (browserPlugins.flash ? "1" : "0") +
                "&java=" + (browserPlugins.java ? "1" : "0") +
                "&dir=" + (browserPlugins.director ? "1" : "0") + 
    		    "&qt=" + (browserPlugins.quickTime ? "1" : "0") + 
    		    "&realp=" + (browserPlugins.realPlayer ? "1" : "0") + 
    		    "&pdf=" + (browserPlugins.pdf ? "1" : "0") + 
    		    "&wma=" + (browserPlugins.windowsMedia ? "1" : "0") + 
    		    "&gears=" + (browserPlugins.gears ? "1" : "0") + 
    		    "&ag=" + (browserPlugins.silverlight ? "1" : "0"); 
        }


        /// <summary>
        /// By default, PiwikTracker will read first party cookies
        /// from the request and write updated cookies in the response (using setrawcookie).
        /// This can be disabled by calling this function.
        /// </summary>      
        public void disableCookieSupport()
        {
        	this.configCookiesDisabled = true;
        }


        /// <summary>
        /// Returns the maximum number of seconds the tracker will spend waiting for a response
        /// from Piwik. Defaults to 600 seconds.
        /// </summary>   
        public int getRequestTimeout()
        {
    	    return this.requestTimeout;
        }
	
        /// <summary>
        /// Sets the maximum number of seconds that the tracker will spend waiting for a response
        /// from Piwik.
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="Exception"/>
        public void setRequestTimeout(int timeout)
        {
    	    if (timeout < 0) {
    		    throw new Exception("Invalid value supplied for request timeout: $timeout");
    	    }
    	
    	    this.requestTimeout = timeout;
        }

        /// <summary>
        /// Used in tests to output useful error messages.
        /// </summary>
        static public string DEBUG_LAST_REQUESTED_URL;

        private HttpWebResponse sendRequest(string url, string method = "GET", string data = null, bool force = false)
        {
            DEBUG_LAST_REQUESTED_URL = url;

    	    // if doing a bulk request, store the url
    	    if (this.doBulkRequests && !force) {
    		    this.storedTrackingActions.Add(
                    url
                    + (!String.IsNullOrEmpty(userAgent) ? "&ua=" + urlEncode(userAgent) : "")
                    + (!String.IsNullOrEmpty(acceptLanguage) ? "&lang=" + urlEncode(acceptLanguage) : "")
                );

                // Clear custom variables so they don't get copied over to other users in the bulk request
                this.clearCustomVariables();
                this.userAgent = null;
                this.acceptLanguage = null;
    		    return null;
    	    }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.UserAgent = this.userAgent;            
            request.Headers.Add("Accept-Language", acceptLanguage);
            request.Timeout = this.requestTimeout;

            if (!string.IsNullOrEmpty(data)) {
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                    streamWriter.Write(data);
                }
            }

            return (HttpWebResponse) request.GetResponse();
        }

        /// <summary>
        /// Returns the base URL for the piwik server.
        /// </summary>
        protected string getBaseUrl()
        {
            if (string.IsNullOrEmpty(URL)) {
                throw new Exception("You must first set the Piwik Tracker URL by calling PiwikTracker.URL = \"http://your-website.org/piwik/\";");
            }
            if (!URL.Contains("/piwik.php")
                && !URL.Contains("/proxy-piwik.php")
            ) {
                URL += "/piwik.php";
            }
            return URL;
        }

        private string getRequest(int idSite)
        {   	
            this.setFirstPartyCookies();

            var url = this.getBaseUrl() +
                "?idsite=" + idSite +
		        "&rec=1" +
		        "&apiv=" + VERSION + 
	            "&r=" + new Random().Next(0, 1000000).ToString("000000") +

                // Only allowed for Super User, token_auth required,
		        (!string.IsNullOrEmpty(ip) ? "&cip=" + ip : "") +
                (!string.IsNullOrEmpty(this.userId) ? "&uid=" + this.urlEncode(this.userId) : "") +    	        
                (!forcedDatetime.Equals(DateTimeOffset.MinValue) ? "&cdt=" + formatDateValue(forcedDatetime) : "") +
                (this.forcedNewVisit ? "&new_visit=1" : "") +
                (!string.IsNullOrEmpty(token_auth) && !this.doBulkRequests ? "&token_auth=" + this.urlEncode(token_auth) : "") +

                // Values collected from cookie
                "&_idts=" + this.createTs +
                "&_idvc=" + this.visitCount +
                ((this.lastVisitTs != null) ? "&_viewts=" + this.lastVisitTs : "" ) +
                ((this.lastEcommerceOrderTs != null) ? "&_ects=" + this.lastEcommerceOrderTs : "" ) +
	        
		        // These parameters are set by the JS, but optional when using API
	            (!string.IsNullOrEmpty(plugins) ? plugins : "") +
                (!localTime.Equals(DateTimeOffset.MinValue) ? "&h=" + localTime.Hour + "&m=" + localTime.Minute + "&s=" + localTime.Second : "") +
	            ((width != 0 && height != 0) ? "&res=" + width + "x" + height : "") +
	            (hasCookies ? "&cookie=1" : "") +
                (!ecommerceLastOrderTimestamp.Equals(DateTimeOffset.MinValue) ? "&_ects=" + formatTimestamp(ecommerceLastOrderTimestamp) : "") +
	        
	            // Various important attributes
	            (!string.IsNullOrEmpty(customData) ? "&data=" + customData : "") +
                (this.visitorCustomVar.Any() ? "&_cvar=" + this.urlEncode(new JavaScriptSerializer().Serialize(this.visitorCustomVar)) : "") +
                (this.pageCustomVar.Any() ? "&cvar=" + this.urlEncode(new JavaScriptSerializer().Serialize(this.pageCustomVar)) : "") +
                (this.eventCustomVar.Any() ? "&e_cvar=" + this.urlEncode(new JavaScriptSerializer().Serialize(this.eventCustomVar)) : "") +
                (this.generationTime != null ? "&gt_ms=" + this.generationTime : "") +
                (!string.IsNullOrEmpty(forcedVisitorId) ? "&cid=" + forcedVisitorId : "&_id=" + this.getVisitorId()) +
	        
	            // URL parameters
                (!string.IsNullOrEmpty(pageUrl) ? "&url=" + urlEncode(pageUrl) : "") +
                (!string.IsNullOrEmpty(urlReferrer) ? "&urlref=" + urlEncode(urlReferrer) : "") +
                (!string.IsNullOrEmpty(this.pageCharset) && !this.pageCharset.Equals(DEFAULT_CHARSET_PARAMETER_VALUES) ? "&cs=" + this.pageCharset : "") +

	            // Attribution information, so that Goal conversions are attributed to the right referrer or campaign
	            // Campaign name
                ((attributionInfo != null && !string.IsNullOrEmpty(attributionInfo.campaignName)) ? "&_rcn=" + urlEncode(attributionInfo.campaignName) : "") +
    	        // Campaign keyword
                ((attributionInfo != null && !string.IsNullOrEmpty(attributionInfo.campaignKeyword)) ? "&_rck=" + urlEncode(attributionInfo.campaignKeyword) : "") +
    	        // Timestamp at which the referrer was set
                ((attributionInfo != null && !attributionInfo.referrerTimestamp.Equals(DateTimeOffset.MinValue)) ? "&_refts=" + formatTimestamp(attributionInfo.referrerTimestamp) : "") +
    	        // Referrer URL
                ((attributionInfo != null && !string.IsNullOrEmpty(attributionInfo.referrerUrl)) ? "&_ref=" + urlEncode(attributionInfo.referrerUrl) : "") +
    		
    		    // custom location info
    		    (!string.IsNullOrEmpty(this.country) ? "&country=" + urlEncode(this.country) : "") +
    		    (!string.IsNullOrEmpty(this.region) ? "&region=" + urlEncode(this.region) : "") +
    		    (!string.IsNullOrEmpty(this.city) ? "&city=" + urlEncode(this.city) : "") +
                (this.lat != null ? "&lat=" + formatGeoLocationValue((float) this.lat) : "") +
                (this.longitude != null ? "&long=" + formatGeoLocationValue((float) this.longitude) : "") +

    	        // DEBUG 
	            DEBUG_APPEND_URL;


            // Reset page level custom variables after this page view
            pageCustomVar = new Dictionary<string ,string[]>();
            eventCustomVar = new Dictionary<string ,string[]>();

            // force new visit only once, user must call again setForceNewVisit()
            this.forcedNewVisit = false;
    	
            return url;
        }

        private HttpCookie getCookieMatchingName(string name)
        {
            if(this.configCookiesDisabled) {
                return null;
            }
            name = this.getCookieName(name);

            if (HttpContext.Current != null) {
                var cookies = HttpContext.Current.Request.Cookies;
                for (var i = 0; i < cookies.Count; i++) {
                    if (cookies[i].Name.Contains(name)) {
                        return cookies[i];
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// If current URL is "http://example.org/dir1/dir2/index.php?param1=value1&param2=value2"
        /// will return "/dir1/dir2/index.php"
        /// </summary>   
        static protected string getCurrentScriptName()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.AbsolutePath;
            }
            return "";
        }


        /// <summary>
        /// If the current URL is 'http://example.org/dir1/dir2/index.php?param1=value1&param2=value2"
        /// will return 'http'
        /// </summary>   
        /// <returns>string 'https' or 'http'</returns>        
        static protected string getCurrentScheme()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.Scheme;
            }
            return "http";
        }


        /// <summary>
        /// If current URL is "http://example.org/dir1/dir2/index.php?param1=value1&param2=value2"
        /// will return "http://example.org"
        /// </summary>   
        /// <returns>string 'https' or 'http'</returns>  
        static protected string getCurrentHost()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.Url.Host;
            }
            return "unknown";
        }


        /// <summary>
        /// If current URL is "http://example.org/dir1/dir2/index.php?param1=value1&param2=value2"
        /// will return "?param1=value1&param2=value2"
        /// </summary>   
        static protected string getCurrentQueryString()
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
        static protected string getCurrentUrl()
        {
            return getCurrentScheme() + "://"
                + getCurrentHost()
                + getCurrentScriptName()
                + getCurrentQueryString();
	    }


        /// <summary>
        /// Sets the first party cookies as would the piwik.js
        /// All cookies are supported: 'id' and 'ses' and 'ref' and 'cvar' cookies.
        /// </summary>  
        protected void setFirstPartyCookies()
        {
            if (this.configCookiesDisabled) {
                return;
            }

            if (string.IsNullOrWhiteSpace(this.cookieVisitorId)) {
                this.loadVisitorIdCookie();
            }

            // Set the 'ref' cookie
            var attributionInfo = this.getAttributionInfo();
            if(attributionInfo != null) {
                this.setCookie("ref", this.urlEncode(new JavaScriptSerializer().Serialize(attributionInfo.toArray())), this.configReferralCookieTimeout);
            }

            // Set the 'ses' cookie
            this.setCookie("ses", "*", this.configSessionCookieTimeout);

            // Set the 'id' cookie
            var visitCount = this.visitCount + 1;
            var cookieValue = this.getVisitorId() + "." + this.createTs + "." + visitCount + "." + this.currentTs + "." + this.lastVisitTs + "." + this.lastEcommerceOrderTs;
            this.setCookie("id", cookieValue, this.configVisitorCookieTimeout);

            // Set the 'cvar' cookie
            this.setCookie("cvar", this.urlEncode(new JavaScriptSerializer().Serialize(this.visitorCustomVar)), this.configSessionCookieTimeout);
        }


        /// <summary>
        /// Sets a first party cookie to the client to improve dual JS-PHP tracking.
        /// 
        /// This replicates the piwik.js tracker algorithms for consistency and better accuracy.
        /// </summary>       
        /// <param name="cookieName"/>
        /// <param name="cookieValue"/>
        /// <param name="cookieTTL"/>
        protected void setCookie( string cookieName, string cookieValue, long cookieTTL)
        {
            if (HttpContext.Current != null) {
                var cookieExpire = this.currentTs + cookieTTL;
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(this.getCookieName(cookieName), cookieValue) { Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(cookieExpire), Path = this.configCookiePath, Domain = this.configCookieDomain });
            }
        }


        protected Dictionary<string, string[]> getCustomVariablesFromCookie()
        {
            var cookie = getCookieMatchingName("cvar");
            if (cookie == null) {
                return new Dictionary<string, string[]>();
            }
            return new JavaScriptSerializer().Deserialize<Dictionary<string, string[]>>(HttpUtility.UrlDecode(cookie.Value));
        }


        private string formatDateValue(DateTimeOffset date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string formatTimestamp(DateTimeOffset date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            double seconds = Convert.ToInt32(diff.TotalSeconds);
            return seconds.ToString();
        }

        private string formatMonetaryValue(double value)
        {
            return value.ToString("0.##", new CultureInfo("en-US")); 
        }        

        private string formatGeoLocationValue(float value)
        {
            return value.ToString(new CultureInfo("en-US")); 
        }        

        private string urlEncode(string value)
        {
            return HttpUtility.UrlEncode(value);
        }

    } 
}
