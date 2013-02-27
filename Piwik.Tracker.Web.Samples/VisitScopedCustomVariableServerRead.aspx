<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>
<%@ Import Namespace="Piwik" %>


This page displays visit scoped custom variables (index 1 & 2) using the Server Side Tracking API (ie. the C# Tracking API). <br/><br/>

<% 
    PiwikTracker.URL = "http://piwik-1.5";
    var piwikTracker = new PiwikTracker(1);
    CustomVar customVar1 = piwikTracker.getCustomVariable(1);
    CustomVar customVar2 = piwikTracker.getCustomVariable(2);   
    
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

<a href="Default.aspx">Back</a>