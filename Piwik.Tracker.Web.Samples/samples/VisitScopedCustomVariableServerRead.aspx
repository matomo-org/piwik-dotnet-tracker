<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>
<%@ Import Namespace="Piwik.Tracker" %>


This page displays visit scoped custom variables (index 1 & 2) using the Server Side Tracking API. <br/><br/>

<% 
    PiwikTracker.URL = ConfigurationSettings.AppSettings["PiwikURL"];
    var piwikTracker = new PiwikTracker(1);
    piwikTracker.enableCookies();
    
    var customVar1 = piwikTracker.getCustomVariable(1);
    var customVar2 = piwikTracker.getCustomVariable(2);   
    
    if (customVar1 != null)
    {
        Response.Write("Custom var index 1, name = " + customVar1.name + "<br/>");
        Response.Write("Custom var index 1, value = " + customVar1.value + "<br/>");    
    } else
    {
        Response.Write("No custom var on index 1 <br/>"); 
    }

    Response.Write("<br/>");

    if (customVar2 != null)
    {
        Response.Write("Custom var index 2, name = " + customVar2.name + "<br/>");
        Response.Write("Custom var index 2, value = " + customVar2.value + "<br/>");
    }
    else
    {
        Response.Write("No custom var on index 2 <br/>");
    }    
                 
%>

<br />

<a href="../Default.aspx">Back</a>