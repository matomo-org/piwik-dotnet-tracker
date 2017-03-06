<%@ Page Language="C#" %>

This page registers 2 visit scoped custom variables with the Javascript API. <br/><br/>

Action taken : <br/><br/>

- The 2 custom variables are set in the cvar cookie<br />
- The 2 custom variables are sent to the Piwik server within the tracking request<br/><br/>

Custom variables can be read using the Server Side Tracking API <br/><br/>

Click <a href="VisitScopedCustomVariableServerRead.aspx">here</a> to do so<br/><br/>

<a href="../Default.aspx">Back</a>

<!-- Piwik -->
    <script type="text/javascript">
        var _paq = _paq || [];
        _paq.push(['setCustomVariable', 1, 'key1', 'value1', 'visit']);
        _paq.push(['setCustomVariable', 2, 'key2', 'value2', 'visit']);
        _paq.push(['trackPageView']);
        (function () {
            var u = "<%=ConfigurationSettings.AppSettings["PiwikURL"]%>/";
            _paq.push(['setTrackerUrl', u + 'piwik.php']);
            _paq.push(['setSiteId', <%=ConfigurationSettings.AppSettings["SiteId"]%>]);
            var d = document, g = d.createElement('script'), s = d.getElementsByTagName('script')[0];
            g.type = 'text/javascript'; g.async = true; g.defer = true; g.src = u + 'piwik.js'; s.parentNode.insertBefore(g, s);
        })();
</script>
<!-- End Piwik Code -->