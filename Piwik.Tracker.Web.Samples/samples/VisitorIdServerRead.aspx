<%@ Page Language="C#" %>

<%@ Import Namespace="Piwik.Tracker" %>

This page displays the result of the getVisitorId() method.
<br />
<br />

If a tracking request has been sent via javascript or using the Server Side Tracking API, this method will output the content of the piwik Id cookie.
<br />
<br />

Otherwise, a random Id is generated<br />
<br />

It is also possible to override the id using "piwikTracker.setVisitorId()"<br />
<br />

<%
    var PiwikBaseUrl = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(Piwik.Tracker.Web.Samples.Singleton.HttpClient, 1, PiwikBaseUrl);
    piwikTracker.EnableCookies();

    this.Response.Write(piwikTracker.GetVisitorId());
%>

<br />

<a href="../Default.aspx">Back</a>