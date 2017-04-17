namespace Piwik.Tracker.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class PiwikTrackerSamples
    {
        private const string UA = "Firefox";
        private static readonly string PiwikBaseUrl = "http://piwik.local";
        private static readonly int SiteId = 1;
        private static readonly HttpClient HttpClient = new HttpClient();

        private static void Main(string[] args)
        {
            // ** PAGE VIEW **
            //            RecordSimplePageView();
            //            RecordSimplePageViewWithCustomProperties();
            //            RecordSimplePageViewWithCustomGeoLocation();
            //            RecordSimplePageViewWithGenerationTime();

            // ** CUSTOM VARIABLES **
            //            RecordCustomVariables();

            // ** CUSTOM DIMENSIONS **
            //            RecordCustomDimensions();

            // ** GOAL CONVERSION **
            //            GoalConversion();
            //            GoalConversionWithAttributionInfo();

            // ** ACTION TRACKING **
            //            trackDownload();
            //            TrackLink();

            // ** ECOMMERCE TRACKING **
            //            ECommerceView();
            //            ECommerceCategoryView();
            //            ECommerceViewWithoutCategory();
            //            UpdateECommerceCartWithOneProduct();
            //            UpdateECommerceCartWithOneProductSKUOnly();
            //            UpdateECommerceCartWithMultipleProducts();
            //            RecordECommerceOrder();
            //            RecordTwoECommerceOrders();

            // ** Bulk Tracking **
            //            BulkTrackTwoRequests();

            // ** Site Search **
            //            TrackSiteSearch();

            // ** Event Tracking **
            //            TrackSongPlayback();

            Console.ReadKey(true);
        }

        /// <summary>
        /// Triggers a Goal conversion
        /// </summary>
        private static async Task GoalConversionAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var response = await piwikTracker.TrackGoalAsync(1, 42.69F);

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records 2 page scoped custom variables and 2 visit scoped custom variables
        /// </summary>
        private static async Task RecordCustomVariablesAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetCustomVariable(1, "var1", "value1");
            piwikTracker.SetCustomVariable(2, "var2", "value2");

            piwikTracker.SetCustomVariable(1, "pagevar1", "pagevalue1", Scopes.Page);
            piwikTracker.SetCustomVariable(2, "pagevar2", "pagevalue2", Scopes.Page);

            var response = await piwikTracker.TrackPageViewAsync("Document title of current page view");

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records 2 custom dimensions
        /// </summary>
        private static async Task RecordCustomDimensionsAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetCustomTrackingParameter("dimension1", "value1");
            piwikTracker.SetCustomTrackingParameter("dimension2", "value2");

            var response = await piwikTracker.TrackPageViewAsync("Document title of current page view");

            DisplayDebugInfo(response);
        }

        /// <summary>
        ///  Records a simple page view with a specified document title
        /// </summary>
        private static async Task RecordSimplePageViewAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var response = await piwikTracker.TrackPageViewAsync("Document title of current page view");

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a simple page view with advanced user, browser and server properties
        /// </summary>
        private static async Task RecordSimplePageViewWithCustomPropertiesAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetResolution(1600, 1400);

            piwikTracker.SetIp("192.168.52.64");
            piwikTracker.SetVisitorId("33c31B01394bdc65");

            piwikTracker.SetForceVisitDateTime(new DateTime(2011, 10, 23, 10, 20, 50, DateTimeKind.Utc));

            piwikTracker.SetResolution(1600, 1400);

            piwikTracker.SetTokenAuth("XYZ");

            var browserPluginsToSet = new BrowserPlugins();
            browserPluginsToSet.Silverlight = true;
            browserPluginsToSet.Flash = true;
            piwikTracker.SetPlugins(browserPluginsToSet);
            piwikTracker.SetBrowserHasCookies(true);

            piwikTracker.SetLocalTime(new DateTime(2000, 1, 1, 9, 10, 25, DateTimeKind.Utc));

            piwikTracker.SetUrl("http://piwik-1.5/supernova");
            piwikTracker.SetUrlReferrer("http://supernovadirectory.org");

            var response = await piwikTracker.TrackPageViewAsync("Document title of current page view");

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a simple page view with custom geo location parameters
        /// </summary>
        private static async Task RecordSimplePageViewWithCustomGeoLocationAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetTokenAuth("XYZ");

            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetCountry("fr");
            piwikTracker.SetRegion("A8");
            piwikTracker.SetCity("Paris");
            piwikTracker.SetLatitude(48.2F);
            piwikTracker.SetLongitude(2.1F);

            var response = await piwikTracker.TrackPageViewAsync("Document title of current page view");

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a simple page view with generation time metric
        /// </summary>
        private static async Task RecordSimplePageViewWithGenerationTimeAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetGenerationTime(10000);

            var response = await piwikTracker.TrackPageViewAsync("Document title of current page view");

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Triggers a Goal conversion with advanced attribution properties
        /// </summary>
        private static async Task GoalConversionWithAttributionInfoAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var attributionInfo = new AttributionInfo();

            attributionInfo.CampaignName = "CAMPAIGN NAME";
            attributionInfo.CampaignKeyword = "CAMPAIGN KEYWORD";
            attributionInfo.ReferrerTimestamp = new DateTime(2011, 04, 08, 23, 48, 24, DateTimeKind.Utc);
            attributionInfo.ReferrerUrl = "http://www.example.org/test/really?q=yes";

            piwikTracker.SetAttributionInfo(attributionInfo);

            var response = await piwikTracker.TrackGoalAsync(1, 42.69F);

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a clicked link
        /// </summary>
        private static async Task TrackLinkAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var response = await piwikTracker.TrackActionAsync("http://dev.piwik.org/svn", ActionType.Link);

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a file download
        /// </summary>
        private static async Task TrackDownloadAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var response = await piwikTracker.TrackActionAsync("http://piwik.org/path/again/latest.zip", ActionType.Download);

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a category page view
        /// </summary>
        private static async Task ECommerceCategoryViewAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetEcommerceView("", "", new List<string> { "Electronics & Cameras" });
            var response = await piwikTracker.TrackPageViewAsync("Looking at Electronics & Cameras page with a page level custom variable");
            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a product view
        /// </summary>
        private static async Task ECommerceViewAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetEcommerceView("SKU2", "PRODUCT name", new List<string> { "Electronics & Cameras", "Clothes" });

            var response = await piwikTracker.TrackPageViewAsync("incredible title!");
            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Records a product view which doesn't belong to a category
        /// </summary>
        private static async Task ECommerceViewWithoutCategoryAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.SetEcommerceView("SKU VERY nice indeed", "PRODUCT name");

            var response = await piwikTracker.TrackPageViewAsync("another incredible title!");
            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Update an eCommerce Cart with one product
        /// </summary>
        private static async Task UpdateECommerceCartWithOneProductAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.AddEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name", new List<string> { "Clothes", "Electronics & Cameras" },
                500.2,
                2
            );

            var response = await piwikTracker.TrackEcommerceCartUpdateAsync(1000.4);

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Update an eCommerce Cart with one product
        /// </summary>
        private static async Task UpdateECommerceCartWithOneProductSKUOnlyAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.AddEcommerceItem(
                "SKU VERY nice indeed"
            );

            var response = await piwikTracker.TrackEcommerceCartUpdateAsync(1000.2);
            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Update an eCommerce Cart with multiple products
        /// </summary>
        private static async Task UpdateECommerceCartWithMultipleProductsAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.AddEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras" },
                500
            );

            // This one overrides the previous addition
            piwikTracker.AddEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras" },
                500,
                2
            );

            piwikTracker.AddEcommerceItem(
                "SKU NEW",
                "BLABLA",
                null,
                5000000,
                20
            );

            var response = await piwikTracker.TrackEcommerceCartUpdateAsync(1000);

            DisplayDebugInfo(response);
        }

        /// <summary>
        /// Registers 2 eCommerce orders
        /// </summary>
        private static async Task RecordTwoECommerceOrdersAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            // First order

            piwikTracker.AddEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras" },
                500,
                2
             );

            piwikTracker.AddEcommerceItem(
                "ANOTHER SKU HERE",
                "PRODUCT name BIS",
                null,
                100,
                6
            );

            var response =
                await piwikTracker.TrackEcommerceOrderAsync(
                    "137nsjusG 1094",
                    1111.11,
                    1000,
                    111,
                    0.11,
                    666
                );

            DisplayDebugInfo(response);

            // Second Order

            piwikTracker.AddEcommerceItem(
                "SKU2",
                "Canon SLR",
                new List<string> { "Electronics & Cameras" },
                1500,
                1
            );

            response = await piwikTracker.TrackEcommerceOrderAsync(
                "1037Bjusu4s3894",
                2000,
                1500,
                400,
                100,
                0
            );

            DisplayDebugInfo(response);
        }

        private static async Task RecordECommerceOrderAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            piwikTracker.AddEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras", "Clothes" },
                500,
                2
             );

            piwikTracker.AddEcommerceItem(
                "ANOTHER SKU HERE",
                "PRODUCT name BIS",
                null,
                100,
                6
            );

            var response =
                await piwikTracker.TrackEcommerceOrderAsync(
                    "133nsjusu 1094",
                    1111.11,
                    1000,
                    111,
                    0.11,
                    666
                );

            DisplayDebugInfo(response);
        }

        private static async Task BulkTrackTwoRequestsAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);

            piwikTracker.SetUserAgent(UA);
            piwikTracker.SetTokenAuth("YOUR TOKEN");

            piwikTracker.EnableBulkTracking();

            await piwikTracker.TrackPageViewAsync("Tracking Request 1");
            await piwikTracker.TrackPageViewAsync("Tracking Request 2");

            DisplayDebugInfo(await piwikTracker.BulkTrackAsync());
        }

        private static async Task TrackSiteSearchAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var response = await piwikTracker.TrackSiteSearchAsync("keyword1", "category2", 0);

            DisplayDebugInfo(response);
        }

        private static async Task TrackSongPlaybackAsync()
        {
            var piwikTracker = new PiwikTracker(HttpClient, SiteId, PiwikBaseUrl);
            piwikTracker.SetUserAgent(UA);

            var response = await piwikTracker.TrackEventAsync("music", "play", "Eye Of The Tiger");

            DisplayDebugInfo(response);
        }

        private static void DisplayDebugInfo(TrackingResponse response)
        {
            Console.WriteLine("DEBUG_LAST_REQUESTED_URL :");
            Console.WriteLine(response.RequestedUrl);
            Console.Write("\r\n");
            Console.WriteLine(response.HttpStatusCode);
        }
    }
}