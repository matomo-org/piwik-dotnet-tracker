using NUnit.Framework;

namespace Piwik.Tracker.Tests
{
    [TestFixture]
    internal class CryptoExtensionsTests
    {
        [Test]
        [TestCase("", "da39a3ee5e6b4b0d3255bfef95601890afd80709")]
        [TestCase(" ", "b858cb282617fb0956d960215c8e84d1ccf909c6")]
        [TestCase("1234dsfa", "644977634278d36d5f451961fe19622ab13cec87")]
        [TestCase("1-2-3-45-6", "5c13bf8b7ff1d43869a7b4246bef897f9499833b")]
        [TestCase("öüüä%&&", "c24eb4685cd57f32098b33066b5b08b31e378981")]
        [TestCase("+- fdgsdgafdgffdsfddgdgdfdfgdfhdghdfghdgfhgfdgar^^°gfra7685&%§$\"$§&(=)(&=,// \\", "bde6cf181dd5bc0ef11342d5c6a4e81a934d9cb8")]
        public void ToSha1_RegressionTests(string valueToEncrypt, string expectedHash)
        {
            //Act
            var actualHash = valueToEncrypt.ToSha1();
            //Assert
            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }
    }
}