<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>
<%@ Import Namespace="Piwik.Tracker" %>

This page displays the result of the getAttributionInfo() method. <br/><br/>

If a tracking request has been sent via javascript, this method will output the content of the piwik ref cookie. <br/><br/>

<% 
    PiwikTracker.URL = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(1);
    piwikTracker.enableCookies();

    var attributionInfo = piwikTracker.getAttributionInfo();
    
    if(attributionInfo == null)
    {
        Response.Write("No attribution information to read from cookies");                        
    }
    else
    {
        Response.Write("campaignName : " + attributionInfo.campaignName + "<br>");
        Response.Write("campaignKeywork : " + attributionInfo.campaignKeyword + "<br>");
        Response.Write("referrerTimestamp : " + attributionInfo.referrerTimestamp + "<br>");
        Response.Write("referrerUrl : " + attributionInfo.referrerUrl + "<br>");
    }
    
    
%>

<br />

<a href="../Default.aspx">Back</a>