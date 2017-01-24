using System;
using NUnit.Framework;

namespace Piwik.Tracker.Tests
{
    [TestFixture]
    public class PiwikTrackerTests
    {
        private const string UA = "Firefox";
        private static readonly string PiwikBaseUrl = "http://piwik.local";
        private static readonly int SiteId = 1;

        [Test]
        [TestCase(Scopes.Page, null, null, null)]
        [TestCase(Scopes.Page, 2, null, null)]
        [TestCase(Scopes.Page, 2, "myPageVar", "myPageVarValue")]
        [TestCase(Scopes.Event, null, null, null)]
        [TestCase(Scopes.Event, 3, null, null)]
        [TestCase(Scopes.Event, 3, "myEventVar", "myEventVarValue")]
        [TestCase(Scopes.Visit, null, null, null)]
        [TestCase(Scopes.Visit, 4, null, null)]
        [TestCase(Scopes.Visit, 4, "myVisitVar", "myVisitVarValue")]
        public void GetCustomVariable_Test(Scopes variableScope, int? variableId, string variableName, string variableValue)
        {
            //Arrange
            var sut = new PiwikTracker(SiteId, PiwikBaseUrl);
            if (variableId != null)
            {
                sut.SetCustomVariable(variableId.Value, variableName, variableValue, variableScope);
            }
            //Act

            var actual = sut.GetCustomVariable(variableId ?? 99, variableScope);
            //Assert
            if (variableId != null)
            {
                Assert.That(actual.Name, Is.EqualTo(variableName));
                Assert.That(actual.Value, Is.EqualTo(variableValue));
            }
            else
            {
                Assert.That(actual, Is.Null);
            }
        }

        [Test, Ignore("Todo: Provide ability to mock cookie!")]
        public void GetCustomVariable_WhenScopeIsVisit_ReturnsVariableFromCookie()
        {
            Assert.That(false, Is.True, "Todo: Provide ability to mock cookie!");
        }

        [Test]
        public void GetCustomVariable_WhenInvalidScopeArgument_Throws()
        {
            //Arrange
            var sut = new PiwikTracker(SiteId, PiwikBaseUrl);

            //Act & Assert
            Assert.Throws<ArgumentException>(() => sut.GetCustomVariable(1, (Scopes)1234));
        }
    }
}