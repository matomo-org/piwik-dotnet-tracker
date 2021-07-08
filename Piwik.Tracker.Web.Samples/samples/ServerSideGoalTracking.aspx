﻿<%@ Page Language="C#" Async="true" %>

<%@ Import Namespace="System.Threading.Tasks" %>
<%@ Import Namespace="Piwik.Tracker" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        RegisterAsyncTask(new PageAsyncTask(GoalTrackingAsync));
    }

    private async Task GoalTrackingAsync()
    {
        var PiwikBaseUrl = ConfigurationSettings.AppSettings["PiwikURL"];

        var piwikTracker = new PiwikTracker(Piwik.Tracker.Web.Samples.Singleton.HttpClient, 1, PiwikBaseUrl);
        piwikTracker.EnableCookies();

        var attributionInfo = new AttributionInfo();

        attributionInfo.CampaignName = "CAMPAIGN NAME";
        attributionInfo.CampaignKeyword = "CAMPAIGN KEYWORD";
        attributionInfo.ReferrerTimestamp = new DateTime(2011, 04, 08, 23, 48, 24, DateTimeKind.Utc);
        attributionInfo.ReferrerUrl = "http://www.example.org/test/really?q=yes";

        piwikTracker.SetAttributionInfo(attributionInfo);

        piwikTracker.SetCustomVariable(1, "custom-variable1", "custom-variable1-value");

        statusCode.Text = (await piwikTracker.TrackGoalAsync(1, 42.69F)).HttpStatusCode.ToString();
    }
</script>


<form id="form1" runat="server">
    This page tracks a goal conversion with the Server Side tracking API and displays the response.
    <br />
    <br />
    <asp:literal id="statusCode" runat="server" enableviewstate="False" />
    <br />

    <a href="../Default.aspx">Back</a>
</form>
