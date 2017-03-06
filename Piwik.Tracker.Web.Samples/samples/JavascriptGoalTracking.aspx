<%@ Page Language="C#" %>

This page tracks a goal conversion with the Javascript API. <br/><br/>

<a href="../Default.aspx">Back</a>

<!-- Piwik -->
    <script type="text/javascript">
        var _paq = _paq || [];
        _paq.push(['setReferrerUrl', 'http://supernovarep.org']);
        _paq.push(['trackGoal', 1]);
        (function() {
            var u = "<%=ConfigurationSettings.AppSettings["PiwikURL"]%>/";
            _paq.push(['setTrackerUrl', u+'piwik.php']);
            _paq.push(['setSiteId', <%=ConfigurationSettings.AppSettings["SiteId"]%>]);
            var d=document, g=d.createElement('script'), s=d.getElementsByTagName('script')[0];
            g.type='text/javascript'; g.async=true; g.defer=true; g.src=u+'piwik.js'; s.parentNode.insertBefore(g,s);
        })();
</script>
<!-- End Piwik Code -->