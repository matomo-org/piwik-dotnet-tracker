<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>
<%@ Import Namespace="Piwik.Tracker" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.IO" %>

This page tracks a goal conversion with the Server Side tracking API and displays the response. <br/><br/>

<% 
    PiwikTracker.URL = ConfigurationSettings.AppSettings["PiwikURL"];

    var piwikTracker = new PiwikTracker(1);
    piwikTracker.enableCookies();

    var attributionInfo = new AttributionInfo();

    attributionInfo.campaignName = "CAMPAIGN NAME";
    attributionInfo.campaignKeyword = "CAMPAIGN KEYWORD";
    attributionInfo.referrerTimestamp = new DateTime(2011, 04, 08, 23, 48, 24);
    attributionInfo.referrerUrl = "http://www.example.org/test/really?q=yes";

    piwikTracker.setAttributionInfo(attributionInfo);
    
    piwikTracker.setCustomVariable(1, "custom-variable1", "custom-variable1-value");

    var response = piwikTracker.doTrackGoal(1, 42.69F);

    this.Response.Write(response.StatusCode);
    var receiveStream = response.GetResponseStream();
    var encode = Encoding.GetEncoding("utf-8");
    // Pipes the stream to a higher level stream reader with the required encoding format. 
    var readStream = new StreamReader(receiveStream, encode);
    this.Response.Write("\r\nResponse stream received.");
    var read = new Char[256];
    // Reads 256 characters at a time.    
    var count = readStream.Read(read, 0, 256);
    this.Response.Write("HTML...\r\n");
    while (count > 0)
    {
        // Dumps the 256 characters on a string and displays the string to the console.
        String str = new String(read, 0, count);
        Response.Write(str);
        count = readStream.Read(read, 0, 256);
    }
    // Releases the resources of the response.
    response.Close();
    // Releases the resources of the Stream.
    readStream.Close();    
%>

<br />

<a href="../Default.aspx">Back</a>