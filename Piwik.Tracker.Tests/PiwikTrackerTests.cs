using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Piwik.Tracker.Tests
{
    [TestFixture]
    internal class PiwikTrackerTests
    {
        private const string UA = "Firefox";
        private const string PiwikBaseUrl = "http://piwik.local";
        private const int SiteId = 1;
        private PiwikTracker _sut;

        [SetUp]
        public void SetUpTest()
        {
            _sut = new PiwikTracker(SiteId, PiwikBaseUrl);
        }

        [Test]
        [TestCase(Scopes.Page, 2)]
        [TestCase(Scopes.Event, 3)]
        [TestCase(Scopes.Visit, 4)]
        public void GetCustomVariable_WhenVariableNotSet_ReturnsNull(Scopes variableScope, int variableId)
        {
            //Arrange, Act
            var actual = _sut.GetCustomVariable(variableId, variableScope);
            //Assert
            Assert.That(actual, Is.Null);
        }

        [Test]
        [TestCase(Scopes.Page, 2, null, null)]
        [TestCase(Scopes.Page, 2, "myPageVar", "myPageVarValue")]
        [TestCase(Scopes.Event, 3, null, null)]
        [TestCase(Scopes.Event, 3, "myEventVar", "myEventVarValue")]
        [TestCase(Scopes.Visit, 4, null, null)]
        [TestCase(Scopes.Visit, 4, "myVisitVar", "myVisitVarValue")]
        public void GetCustomVariable_WhenVariableIsSet_ReturnsCorrectVariable(Scopes variableScope, int variableId, string variableName, string variableValue)
        {
            //Arrange
            _sut.SetCustomVariable(variableId, variableName, variableValue, variableScope);

            //Act
            var actual = _sut.GetCustomVariable(variableId, variableScope);
            //Assert
            Assert.That(actual.Name, Is.EqualTo(variableName));
            Assert.That(actual.Value, Is.EqualTo(variableValue));
        }

        [Test]
        public void GetCustomVariable_WhenInvalidScopeArgument_Throws()
        {
            //Arrange
            //Act & Assert
            Assert.Throws<ArgumentException>(() => _sut.GetCustomVariable(1, (Scopes)1234));
        }

        [Test]
        public void Ctor_WhenNoUrlProvided_Throws()
        {
            Assert.Throws<ArgumentException>(() => new PiwikTracker(0, null));
        }

        [Test]
        [TestCase("testCharset", "&cs=testCharset")]
        [TestCase("utf16", "&cs=utf16")]
        [TestCase("", null)]
        public void SetPageCharset_WhenSpecified_IsAddedToRequest(string charset, string expected)
        {
            // Arrange, Act
            _sut.SetPageCharset(charset);
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(expected))
            {
                Assert.That(actual, Does.Not.Contain("&cs="));
            }
            else
            {
                Assert.That(actual, Does.Contain(expected));
            }
        }

        [Test]
        [TestCase("http://myurl.com/index.html")]
        [TestCase("")]
        public void SetUrl_WhenSpecified_IsAddedToRequest(string pageUrl)
        {
            // Arrange, Act
            _sut.SetUrl(pageUrl);
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(pageUrl))
            {
                Assert.That(actual, Does.Not.Contain("&url="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&url=" + HttpUtility.UrlEncode(pageUrl)));
            }
        }

        [Test]
        [TestCase("http://myurlreferer.com/index.html")]
        [TestCase("")]
        public void SetUrlReferrer_WhenSpecified_IsAddedToRequest(string referer)
        {
            // Arrange, Act
            _sut.SetUrlReferrer(referer);
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(referer))
            {
                Assert.That(actual, Does.Not.Contain("&urlref="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&urlref=" + HttpUtility.UrlEncode(referer)));
            }
        }

        [Test]
        [TestCase(55)]
        [TestCase(null)]
        public void SetGenerationTime_WhenSpecified_IsAddedToRequest(int? timeMs)
        {
            // Arrange, Act
            if (timeMs.HasValue)
            {
                _sut.SetGenerationTime(timeMs.Value);
            }

            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (!timeMs.HasValue)
            {
                Assert.That(actual, Does.Not.Contain("&urlref="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&gt_ms=" + timeMs));
            }
        }

        [Test]
        [TestCase("myCampaignName")]
        [TestCase("")]
        public void SetAttributionInfo_WhenCampaignNameSpecified_IsAddedToRequest(string campaignName)
        {
            // Arrange, Act
            _sut.SetAttributionInfo(new AttributionInfo() { CampaignName = campaignName });
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(campaignName))
            {
                Assert.That(actual, Does.Not.Contain("&_rcn="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&_rcn=" + HttpUtility.UrlEncode(campaignName)));
            }
        }

        [Test]
        [TestCase("02/04/2017 16:51:46 +01:00")]
        [TestCase("")]
        public void SetAttributionInfo_WhenReferrerTimestampSpecified_IsAddedToRequest(string referrerDateTime)
        {
            var expectedTs = "1486223506"; // cf http://xmillis.com/l1c9bu4i.9e
            var timestampProvided = !string.IsNullOrEmpty(referrerDateTime);
            if (timestampProvided)
            {
                var referrerTimestamp = DateTimeOffset.Parse(referrerDateTime, CultureInfo.InvariantCulture);
                var attrInfo = new AttributionInfo {ReferrerTimestamp = referrerTimestamp};
                Assert.That(attrInfo.ToArray()[2], Is.EqualTo(expectedTs));
                _sut.SetAttributionInfo(attrInfo);

            }
            Assert.That(_sut.GetRequest(SiteId), timestampProvided ? Does.Contain("&_refts=" + expectedTs) : Does.Not.Contain("&_refts="));
        }

        [Test]
        [TestCase("Http://myReferrerUrl.com/index.php?key=value")]
        [TestCase("")]
        public void SetAttributionInfo_WhenReferrerUrlSpecified_IsAddedToRequest(string referrerUrl)
        {
            // Arrange, Act
            _sut.SetAttributionInfo(new AttributionInfo() { ReferrerUrl = referrerUrl });
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(referrerUrl))
            {
                Assert.That(actual, Does.Not.Contain("&_ref="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&_ref=" + HttpUtility.UrlEncode(referrerUrl)));
            }
        }

        [Test]
        [TestCase("dimension1", "")]
        [TestCase("dimension2", null)]
        [TestCase("dimension3", "123")]
        public void SetCustomTrackingParameter_WhenSpecified_IsAddedToRequest(string trackingApiParameter, string value)
        {
            // Arrange, Act
            _sut.SetCustomTrackingParameter(trackingApiParameter, value);

            // Assert
            var actual = _sut.GetRequest(SiteId);
            Assert.That(actual, Does.Contain($"&{trackingApiParameter}={value}"));
        }

        [Test]
        public void SetNewVisitorId_WhenSpecified_IsAddedToRequest()
        {
            //Arrange
            var initalVisitorId = _sut.GetVisitorId();
            Assert.That(initalVisitorId, Is.Not.Null.Or.Empty);
            //Act
            _sut.SetNewVisitorId();
            //Assert
            var actual = _sut.GetVisitorId();
            Assert.That(actual, Is.Not.Null.Or.Empty);
            Assert.That(actual, Is.Not.EqualTo(initalVisitorId));
            var request = _sut.GetRequest(SiteId);
            Assert.That(request, Does.Contain("&_id=" + actual));
        }

        [Test]
        [TestCase("")]
        [TestCase("fghfügh")]
        public void SetCountry_WhenSpecified_IsAddedToRequest(string country)
        {
            // Arrange, Act
            _sut.SetCountry(country);
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(country))
            {
                Assert.That(actual, Does.Not.Contain("&country="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&country=" + HttpUtility.UrlEncode(country)));
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("fghfügh")]
        public void SetRegion_WhenSpecified_IsAddedToRequest(string region)
        {
            // Arrange, Act
            _sut.SetRegion(region);
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(region))
            {
                Assert.That(actual, Does.Not.Contain("&region="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&region=" + HttpUtility.UrlEncode(region)));
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("fghfügh")]
        public void SetCity_WhenSpecified_IsAddedToRequest(string city)
        {
            // Arrange, Act
            _sut.SetCity(city);
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (string.IsNullOrEmpty(city))
            {
                Assert.That(actual, Does.Not.Contain("&city="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&city=" + HttpUtility.UrlEncode(city)));
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase(0L)]
        [TestCase(42L)]
        public void SetLatitude_WhenSpecified_IsAddedToRequest(long? latitude)
        {
            // Arrange, Act
            if (latitude.HasValue)
            {
                _sut.SetLatitude(latitude.Value);
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (!latitude.HasValue)
            {
                Assert.That(actual, Does.Not.Contain("&lat="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&lat=" + latitude.Value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase(0L)]
        [TestCase(42L)]
        public void SetLongitude_WhenSpecified_IsAddedToRequest(long? @long)
        {
            // Arrange, Act
            if (@long.HasValue)
            {
                _sut.SetLongitude(@long.Value);
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (!@long.HasValue)
            {
                Assert.That(actual, Does.Not.Contain("&long="));
            }
            else
            {
                Assert.That(actual, Does.Contain("&long=" + @long.Value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DisableSendImageResponse_WhenSpecified_IsAddedToRequest(bool doDisable)
        {
            // Arrange, Act
            if (doDisable)
            {
                _sut.DisableSendImageResponse();
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (doDisable)
            {
                Assert.That(actual, Does.Contain("&send_image=0"));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain("&send_image=0"));
            }
        }

        [Test]
        [TestCase("gh-.:/65%", "myName", "1,2,3", 0.70, 9223372036854775808UL)]
        [TestCase("gh-.:/65%", "", "1,2,3", 0.570, 0UL)]
        [TestCase("gh-.:/65%", "myName", null, 0.70, 9223372036854775808UL)]
        [TestCase("gh-.:/65%", "myName", "1", 45763756d, 9223372036854775808UL)]
        public void AddEcommerceItem_Test(string sku, string name, string categories, double price, ulong quantity)
        {
            List<string> categoryList = null;
            if (!string.IsNullOrEmpty(categories))
            {
                categoryList = categories.Split(',').ToList();
            }
            var actual = _sut.GetUrlTrackEcommerce(0);
            Assert.That(actual, Does.Not.Contain("ec_items"));
            _sut.AddEcommerceItem(sku, name, categoryList, price, quantity);
            actual = _sut.GetUrlTrackEcommerce(0);
            var expected = new Dictionary<string, object[]>
            {
                { "",new object[] {sku, name, categoryList, price.ToString("0.##", CultureInfo.InvariantCulture), quantity}}
            };
            var expectedAsJson = new JavaScriptSerializer().Serialize(expected.Values);
            Console.WriteLine(expectedAsJson);
            Assert.That(actual, Does.Contain("&ec_items=" + HttpUtility.UrlEncode(expectedAsJson)));
        }

        [Test]
        [TestCase("mySku", "myName", "1,2,3", 0.70)]
        [TestCase("mySku", "myName", null, 0.70)]
        [TestCase("mySku", "myName", "1,2,3", 22265460.70)]
        public void SetEcommerceView_Test(string sku, string name, string categories, double price)
        {
            // Arrange
            List<string> categoryList = null;
            if (!string.IsNullOrEmpty(categories))
            {
                categoryList = categories.Split(',').ToList();
            }
            // Act
            _sut.SetEcommerceView(sku, name, categoryList, price);
            var actualRequest = _sut.GetRequest(SiteId);
            // Assert
            var actualRequestArguments = new Uri(actualRequest).ParseQueryString().ToKeyValuePairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var actualVariablesById = JsonConvert.DeserializeObject<Dictionary<int, object>>(actualRequestArguments["cvar"]);
            Assert.That(actualVariablesById.Count, Is.EqualTo(4));
            foreach (var variableById in actualVariablesById)
            {
                Console.WriteLine(variableById.Key + "=" + variableById.Value);
                var variableValue = JsonConvert.DeserializeObject<string[]>(variableById.Value.ToString());
                switch (variableById.Key)
                {
                    case PiwikTracker.CvarIndexEcommerceItemPrice:
                        Assert.That(variableValue, Is.EquivalentTo(new[] { "_pkp", price.ToString("0.##", CultureInfo.InvariantCulture) }));
                        break;

                    case PiwikTracker.CvarIndexEcommerceItemName:
                        Assert.That(variableValue, Is.EquivalentTo(new[] { "_pkn", name }));
                        break;

                    case PiwikTracker.CvarIndexEcommerceItemCategory:
                        if (string.IsNullOrEmpty(categories))
                        {
                            continue;
                        }
                        Assert.That(variableValue, Is.EquivalentTo(new[] { "_pkc", new JavaScriptSerializer().Serialize(categoryList) }));
                        break;

                    case PiwikTracker.CvarIndexEcommerceItemSku:
                        Assert.That(variableValue, Is.EquivalentTo(new[] { "_pks", sku }));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetForceVisitDateTime_WhenSpecified_IsAddedToRequest(bool setValue)
        {
            if (setValue)
                _sut.SetForceVisitDateTime(new DateTimeOffset(2017, 4, 20, 18, 11, 10, new TimeSpan(2, 0, 0)));
            Assert.That(_sut.GetRequest(SiteId), setValue ? Does.Contain("&cdt=2017-04-20 16:11:10") : Does.Not.Contain("&cdt"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetForceNewVisit_WhenSpecified_IsAddedToRequest(bool setValue)
        {
            // Arrange
            //Act
            if (setValue)
            {
                _sut.SetForceNewVisit();
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (setValue)
            {
                Assert.That(actual, Does.Contain("&new_visit=1"));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain("&new_visit"));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetIp_WhenSpecified_IsAddedToRequest(bool setValue)
        {
            // Arrange
            var expectedIp = "l30.54.2.1";
            //Act
            if (setValue)
            {
                _sut.SetIp(expectedIp);
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (setValue)
            {
                Assert.That(actual, Does.Contain("&cip=" + HttpUtility.UrlEncode(expectedIp)));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain("&cip"));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetUserId_WhenSpecified_IsAddedToRequest(bool setValue)
        {
            // Arrange
            var expectedId = "l30%&Ö";
            //Act
            if (setValue)
            {
                _sut.SetUserId(expectedId);
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (setValue)
            {
                Assert.That(actual, Does.Contain("&uid=" + HttpUtility.UrlEncode(expectedId)));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain("&uid"));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetLocalTime_WhenSpecified_IsAddedToRequest(bool setValue)
        {
            // Arrange
            var expected = DateTime.Now.AddHours(-3);
            //Act
            if (setValue)
            {
                _sut.SetLocalTime(expected);
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (setValue)
            {
                Assert.That(actual, Does.Contain("&h=" + expected.Hour));
                Assert.That(actual, Does.Contain("&m=" + expected.Minute));
                Assert.That(actual, Does.Contain("&s=" + expected.Second));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain("&h="));
                Assert.That(actual, Does.Not.Contain("&m="));
                Assert.That(actual, Does.Not.Contain("&s="));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetResolution_WhenSpecified_IsAddedToRequest(bool setValue)
        {
            // Arrange
            //Act
            if (setValue)
            {
                _sut.SetResolution(800, 600);
            }
            // Assert
            var actual = _sut.GetRequest(SiteId);
            if (setValue)
            {
                Assert.That(actual, Does.Contain("&res=800x600"));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain("&res="));
            }
        }
    }
}