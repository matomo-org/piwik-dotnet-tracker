<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>
<%@ Import Namespace="Piwik" %>

This page displays the result of the getVisitorId() method. <br/><br/>

If a tracking request has been sent via javascript, this method will output the content of the piwik Id cookie. <br/><br/>

<% 
    PiwikTracker.URL = "http://piwik-1.5";
    var piwikTracker = new PiwikTracker(1);

    Response.Write(piwikTracker.getVisitorId());                    
%>

<br />

<a href="Default.aspx">Back</a>