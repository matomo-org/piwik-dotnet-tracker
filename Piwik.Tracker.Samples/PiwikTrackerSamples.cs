#region license
// http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later
#endregion

using System;
using System.Text;
using System.Net;
using System.IO;

namespace Piwik.Tracker.Samples
{
    using System.Collections.Generic;

    class PiwikTrackerSamples
    {
        static void Main(string[] args)
        {
            PiwikTracker.URL = "http://piwik.local";

            // ** PAGE VIEW **
//            RecordSimplePageView();
//            RecordSimplePageViewWithCustomProperties();

            // ** CUSTOM VARIABLES **
//            RecordCustomVariables();

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

            Console.ReadKey(true);
        }


        /// <summary>
        /// Triggers a Goal conversion
        /// </summary>
        static private void GoalConversion()
        {
            var piwikTracker = new PiwikTracker(1);

            var response = piwikTracker.doTrackGoal(1, 42.69F);

            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Records 2 page scoped custom variables and 2 visit scoped custom variables
        /// </summary>
        static private void RecordCustomVariables()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.setCustomVariable(1, "var1", "value1");
            piwikTracker.setCustomVariable(2, "var2", "value2");

            piwikTracker.setCustomVariable(1, "pagevar1", "pagevalue1", CustomVar.Scopes.page);
            piwikTracker.setCustomVariable(2, "pagevar2", "pagevalue2", CustomVar.Scopes.page);

            var response = piwikTracker.doTrackPageView("Document title of current page view");

            displayHttpWebReponse(response);
        }


        /// <summary>
        ///  Records a simple page view with a specified document title
        /// </summary>
        static private void RecordSimplePageView()
        {
            var piwikTracker = new PiwikTracker(1);

            var response = piwikTracker.doTrackPageView("Document title of current page view");

            displayHttpWebReponse(response); 
        }


        /// <summary>
        /// Records a simple page view with advanced user, browser and server properties
        /// </summary>
        static private void RecordSimplePageViewWithCustomProperties()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.setResolution(1600, 1400);

            piwikTracker.setIp("192.168.52.64");
            piwikTracker.setVisitorId("33c31B01394bdc65");

            piwikTracker.setForceVisitDateTime(new DateTime(2011, 10, 23, 10, 20, 50));

            piwikTracker.setResolution(1600, 1400);

            piwikTracker.setTokenAuth("XYZ");

            var browserPluginsToSet = new BrowserPlugins();
            browserPluginsToSet.silverlight = true;
            browserPluginsToSet.flash = true;
            piwikTracker.setPlugins(browserPluginsToSet);
            piwikTracker.setBrowserHasCookies(true);

            piwikTracker.setLocalTime(new DateTime(2000, 1, 1, 9, 10, 25));

            piwikTracker.setUrl("http://piwik-1.5/supernova");
            piwikTracker.setUrlReferrer("http://supernovadirectory.org");

            var response = piwikTracker.doTrackPageView("Document title of current page view");

            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Triggers a Goal conversion with advanced attribution properties
        /// </summary>
        static private void GoalConversionWithAttributionInfo()
        {
            var piwikTracker = new PiwikTracker(1);

            var attributionInfo = new AttributionInfo();

            attributionInfo.campaignName = "CAMPAIGN NAME";
            attributionInfo.campaignKeyword = "CAMPAIGN KEYWORD";
            attributionInfo.referrerTimestamp = new DateTime(2011, 04, 08, 23, 48, 24);
            attributionInfo.referrerUrl = "http://www.example.org/test/really?q=yes";

            piwikTracker.setAttributionInfo(attributionInfo);

            var response = piwikTracker.doTrackGoal(1, 42.69F);

            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Records a clicked link
        /// </summary>
        static private void TrackLink()
        {
            var piwikTracker = new PiwikTracker(1);

            var response = piwikTracker.doTrackAction("http://dev.piwik.org/svn", PiwikTracker.ActionType.link);

            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Records a file download
        /// </summary>
        static private void trackDownload()
        {
            var piwikTracker = new PiwikTracker(1);

            var response = piwikTracker.doTrackAction("http://piwik.org/path/again/latest.zip", PiwikTracker.ActionType.download);

            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Records a category page view
        /// </summary>
        static private void ECommerceCategoryView()
        {
            var piwikTracker = new PiwikTracker(1);      

            piwikTracker.setEcommerceView("", "", new List<string> { "Electronics & Cameras" });
            var response = piwikTracker.doTrackPageView("Looking at Electronics & Cameras page with a page level custom variable");
            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Records a product view
        /// </summary>
        static private void ECommerceView()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.setEcommerceView("SKU2", "PRODUCT name", new List<string> { "Electronics & Cameras", "Clothes" });

            var response = piwikTracker.doTrackPageView("incredible title!");
            displayHttpWebReponse(response);
        }


        /// <summary>
        /// Records a product view which doesn't belong to a category
        /// </summary>
        static private void ECommerceViewWithoutCategory()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.setEcommerceView("SKU VERY nice indeed", "PRODUCT name");

            var response = piwikTracker.doTrackPageView("another incredible title!");
            displayHttpWebReponse(response);
        }

        /// <summary>
        /// Update an eCommerce Cart with one product
        /// </summary>
        static private void UpdateECommerceCartWithOneProduct()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.addEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name", new List<string> { "Clothes", "Electronics & Cameras" }, 
                500.2,
                2
            );

            var response = piwikTracker.doTrackEcommerceCartUpdate(1000.4);
            
            displayHttpWebReponse(response);
        }

        /// <summary>
        /// Update an eCommerce Cart with one product
        /// </summary>
        static private void UpdateECommerceCartWithOneProductSKUOnly()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.addEcommerceItem(
                "SKU VERY nice indeed"
            );

            var response = piwikTracker.doTrackEcommerceCartUpdate(1000.2);
            displayHttpWebReponse(response);
        }

        /// <summary>
        /// Update an eCommerce Cart with multiple products
        /// </summary>
        static private void UpdateECommerceCartWithMultipleProducts()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.addEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras" },
                500
            );

            // This one overrides the previous addition
            piwikTracker.addEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name", 
                new List<string> { "Electronics & Cameras" }, 
                500, 
                2
            );

            piwikTracker.addEcommerceItem(
                "SKU NEW",
                "BLABLA", 
                null, 
                5000000, 
                20
            );


            var response = piwikTracker.doTrackEcommerceCartUpdate(1000);

            displayHttpWebReponse(response);
        }
        
        /// <summary>
        /// Registers 2 eCommerce orders
        /// </summary>
        static private void RecordTwoECommerceOrders()
        {
            var piwikTracker = new PiwikTracker(1);

            // First order

            piwikTracker.addEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras" }, 
                500,
                2
             );

            piwikTracker.addEcommerceItem(
                "ANOTHER SKU HERE", 
                "PRODUCT name BIS" , 
                null, 
                100, 
                6
            );        

            var response = 
                piwikTracker.doTrackEcommerceOrder(
                    "137nsjusG 1094", 
                    1111.11, 
                    1000, 
                    111, 
                    0.11,
                    666
                );

            displayHttpWebReponse(response);   

            // Second Order

            piwikTracker.addEcommerceItem(
                "SKU2", 
                "Canon SLR" , 
                new List<string> { "Electronics & Cameras" }, 
                1500,
                1
            );
            
            response = piwikTracker.doTrackEcommerceOrder(
                "1037Bjusu4s3894", 
                2000, 
                1500, 
                400, 
                100, 
                0
            );

            displayHttpWebReponse(response);  
        }

        static private void RecordECommerceOrder()
        {
            var piwikTracker = new PiwikTracker(1);

            piwikTracker.addEcommerceItem(
                "SKU VERY nice indeed",
                "PRODUCT name",
                new List<string> { "Electronics & Cameras" , "Clothes"},
                500,
                2
             );

            piwikTracker.addEcommerceItem(
                "ANOTHER SKU HERE",
                "PRODUCT name BIS",
                null,
                100,
                6
            );

            var response =
                piwikTracker.doTrackEcommerceOrder(
                    "133nsjusu 1094",
                    1111.11,
                    1000,
                    111,
                    0.11,
                    666
                );

            displayHttpWebReponse(response);
        }

        /// <summary>
        /// Outputs the HTTP reponse to the console
        /// 
        /// Code taken from http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.getresponsestream.aspx
        /// </summary>
        /// <param name="response"></param>       
        static private void displayHttpWebReponse(HttpWebResponse response)
        {
            Console.WriteLine(response.StatusCode);
            Stream receiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);
            Console.WriteLine("\r\nResponse stream received.");
            Char[] read = new Char[256];
            // Reads 256 characters at a time.    
            int count = readStream.Read(read, 0, 256);
            Console.WriteLine("HTML...\r\n");
            while (count > 0)
            {
                // Dumps the 256 characters on a string and displays the string to the console.
                String str = new String(read, 0, count);
                Console.Write(str);
                count = readStream.Read(read, 0, 256);
            }
            Console.WriteLine("");
            // Releases the resources of the response.
            response.Close();
            // Releases the resources of the Stream.
            readStream.Close();
        }
    }
}
