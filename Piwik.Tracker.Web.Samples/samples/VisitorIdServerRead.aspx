<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>
<%@ Import Namespace="Piwik.Tracker" %>

This page displays the result of the getVisitorId() method. <br/><br/>

If a tracking request has been sent via javascript or using the Server Side Tracking API, this method will output the content of the piwik Id cookie. <br/><br/>

Otherwise, a random Id is generated<br/><br/>

It is also possible to override the id using "piwikTracker.setVisitorId()"<br/><br/>

<% 
    PiwikTracker.Url = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(1);
    piwikTracker.EnableCookies();

    this.Response.Write(piwikTracker.GetVisitorId());                    
%>

<br />

<a href="../Default.aspx">Back</a>