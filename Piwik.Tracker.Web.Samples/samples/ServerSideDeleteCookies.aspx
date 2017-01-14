<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>

<%@ Import Namespace="Piwik.Tracker" %>

This page deletes all first party Piwik cookies using the Server Side Tracking API.
<br />
<br />

<%
    var PiwikBaseUrl = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(1, PiwikBaseUrl);
    piwikTracker.DeleteCookies();
%>

<br />

<a href="../Default.aspx">Back</a>