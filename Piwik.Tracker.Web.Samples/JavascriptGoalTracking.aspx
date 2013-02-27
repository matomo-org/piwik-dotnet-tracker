<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>

This page tracks a goal conversion with the Javascript API. <br/><br/>

<a href="Default.aspx">Back</a>

<!-- Piwik -->
<script type="text/javascript">
    var pkBaseURL = (("https:" == document.location.protocol) ? "https://piwik-1.5/" : "http://piwik-1.5");
    document.write(unescape("%3Cscript src='" + pkBaseURL + "piwik.js' type='text/javascript'%3E%3C/script%3E"));
</script>

<script type="text/javascript">

    var piwikTracker = Piwik.getTracker(pkBaseURL + "piwik.php", 1);

    piwikTracker.setReferrerUrl("http://supernovarep.org");
    piwikTracker.trackGoal(1);

</script>
<!-- End Piwik Tracking Code -->