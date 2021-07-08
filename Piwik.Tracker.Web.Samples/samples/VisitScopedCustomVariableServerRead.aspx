<%@ Page Language="C#" %>

<%@ Import Namespace="Piwik.Tracker" %>

This page displays visit scoped custom variables (index 1 & 2) using the Server Side Tracking API.
<br />
<br />

<%
    var PiwikBaseUrl = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(Piwik.Tracker.Web.Samples.Singleton.HttpClient, 1, PiwikBaseUrl);
    piwikTracker.EnableCookies();

    var customVar1 = piwikTracker.GetCustomVariable(1);
    var customVar2 = piwikTracker.GetCustomVariable(2);

    if (customVar1 != null)
    {
        Response.Write("Custom var index 1, name = " + customVar1.Name + "<br />");
        Response.Write("Custom var index 1, value = " + customVar1.Value + "<br />");
    }
    else
    {
        Response.Write("No custom var on index 1 <br />");
    }

    Response.Write("<br />");

    if (customVar2 != null)
    {
        Response.Write("Custom var index 2, name = " + customVar2.Name + "<br />");
        Response.Write("Custom var index 2, value = " + customVar2.Value + "<br />");
    }
    else
    {
        Response.Write("No custom var on index 2 <br />");
    }

%>

<br />

<a href="../Default.aspx">Back</a>