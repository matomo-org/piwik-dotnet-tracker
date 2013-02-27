<%--http://www.gnu.org/licenses/gpl-3.0.html GPL v3 or later--%>

<%@ Page Language="C#" %>

This page registers 2 visit scoped custom variables with the Javascript API. <br/><br/>

Action taken : <br/><br/>

- The 2 custom variables are set in the cvar cookie<br />
- The 2 custom variables are sent to the Piwik server within the tracking request<br/><br/>

Custom variables can be read from the Server Side Tracking API (ie. the C# Tracking API) <br/><br/>

Click <a href="VisitScopedCustomVariableServerRead.aspx">here</a> to do so<br/><br/>

<a href="Default.aspx">Back</a>

<!-- Piwik -->
<script type="text/javascript">
    var pkBaseURL = (("https:" == document.location.protocol) ? "https://piwik-1.5/" : "http://piwik-1.5");
    document.write(unescape("%3Cscript src='" + pkBaseURL + "piwik.js' type='text/javascript'%3E%3C/script%3E"));
</script>

<script type="text/javascript">

    try {

        var piwikTracker = Piwik.getTracker(pkBaseURL + "piwik.php", 1);

        piwikTracker.setCustomVariable(
          1, // Index, the number from 1 to 5 where this Custom Variable name is stored
            "key1", // Name, the name of the variable, for example: Gender, VisitorType
            "value1", // Value, for example: "Male", "Female" or "new", "engaged", "customer"
            "visit" // Scope of the Custom Variable, "visit" means the Custom Variable applies to the current visit
        );

        piwikTracker.setCustomVariable(
            2,
            "key2",
            "value2",
            "visit"
        );

        piwikTracker.trackPageView();

    } catch (err) { }
</script>
<!-- End Piwik Tracking Code -->