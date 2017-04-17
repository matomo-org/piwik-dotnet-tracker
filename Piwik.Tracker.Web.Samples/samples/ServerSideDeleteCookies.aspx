<%@ Page Language="C#" %>

<%@ Import Namespace="Piwik.Tracker" %>

This page deletes all first party Piwik cookies using the Server Side Tracking API.
<br />
<br />

<%
    var PiwikBaseUrl = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(Piwik.Tracker.Web.Samples.Singleton.HttpClient, 1, PiwikBaseUrl);
    piwikTracker.DeleteCookies();
%>

<br />

<a href="../Default.aspx">Back</a>