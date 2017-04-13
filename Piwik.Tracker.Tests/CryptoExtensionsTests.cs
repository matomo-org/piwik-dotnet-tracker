using System.Text;
using NUnit.Framework;

namespace Piwik.Tracker.Tests
{
    [TestFixture]
    internal class CryptoExtensionsTests
    {
        [Test]
        [TestCase("", "D41D8CD98F00B204E9800998ECF8427E")]
        [TestCase(" ", "7215EE9C7D9DC229D2921A40E899EC5F")]
        [TestCase("1234dsfa", "0FFDCBA8AFFF5812719B3BF2D0E80558")]
        [TestCase("1-2-3-45-6", "5B727BB9E9C69B60E233C9DCFE52D2E8")]
        [TestCase("öüüä%&&", "13EB37F4B5505F21E9E875FBF489E22B")]
        [TestCase("+- fdgsdgafdgffdsfddgdgdfdfgdfhdghdfghdgfhgfdgar^^°gfra7685&%§$\"$§&(=)(&=,// \\", "F77A4A9DFFCB33CB8D7CE078A6DE1BAA")]
        public void CreateMd5_RegressionTests(string valueToEncrypt, string expectedHash)
        {
            //Act
            var actualHash = valueToEncrypt.CreateMd5();
            //Assert
            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }

        [Test]
        // Utf8 encoding
        [TestCase(true, "", "DA-39-A3-EE-5E-6B-4B-0D-32-55-BF-EF-95-60-18-90-AF-D8-07-09")]
        [TestCase(true, " ", "B8-58-CB-28-26-17-FB-09-56-D9-60-21-5C-8E-84-D1-CC-F9-09-C6")]
        [TestCase(true, "1234dsfa", "64-49-77-63-42-78-D3-6D-5F-45-19-61-FE-19-62-2A-B1-3C-EC-87")]
        [TestCase(true, "1-2-3-45-6", "5C-13-BF-8B-7F-F1-D4-38-69-A7-B4-24-6B-EF-89-7F-94-99-83-3B")]
        [TestCase(true, "öüüä%&&", "C2-4E-B4-68-5C-D5-7F-32-09-8B-33-06-6B-5B-08-B3-1E-37-89-81")]
        [TestCase(true, "+- fdgsdgafdgffdsfddgdgdfdfgdfhdghdfghdgfhgfdgar^^°gfra7685&%§$\"$§&(=)(&=,// \\", "BD-E6-CF-18-1D-D5-BC-0E-F1-13-42-D5-C6-A4-E8-1A-93-4D-9C-B8")]
        // Default encoding
        [TestCase(false, "", "DA-39-A3-EE-5E-6B-4B-0D-32-55-BF-EF-95-60-18-90-AF-D8-07-09")]
        [TestCase(false, " ", "B8-58-CB-28-26-17-FB-09-56-D9-60-21-5C-8E-84-D1-CC-F9-09-C6")]
        [TestCase(false, "1234dsfa", "64-49-77-63-42-78-D3-6D-5F-45-19-61-FE-19-62-2A-B1-3C-EC-87")]
        [TestCase(false, "1-2-3-45-6", "5C-13-BF-8B-7F-F1-D4-38-69-A7-B4-24-6B-EF-89-7F-94-99-83-3B")]
        [TestCase(false, "öüüä%&&", "90-82-E4-37-59-8F-22-B2-6F-5F-8B-74-FF-E0-2F-B5-4A-FA-45-BB")]
        [TestCase(false, "+- fdgsdgafdgffdsfddgdgdfdfgdfhdghdfghdgfhgfdgar^^°gfra7685&%§$\"$§&(=)(&=,// \\", "FC-66-DA-AD-A5-65-E4-E9-F3-6A-BF-FE-C6-EC-53-BD-19-71-01-8D")]
        public void CreateSha1_RegressionTests(bool utf8, string valueToEncrypt, string expectedHash)
        {
            //Act
            var actualHash = valueToEncrypt.CreateSha1(utf8 ? Encoding.UTF8 : Encoding.Default, hashAsHexadecimal: false);
            //Assert
            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }

        [Test]
        // Utf8 encoding
        [TestCase(true, "", "da39a3ee5e6b4b0d3255bfef95601890afd80709")]
        [TestCase(true, " ", "b858cb282617fb0956d960215c8e84d1ccf909c6")]
        [TestCase(true, "1234dsfa", "644977634278d36d5f451961fe19622ab13cec87")]
        [TestCase(true, "1-2-3-45-6", "5c13bf8b7ff1d43869a7b4246bef897f9499833b")]
        [TestCase(true, "öüüä%&&", "c24eb4685cd57f32098b33066b5b08b31e378981")]
        [TestCase(true, "+- fdgsdgafdgffdsfddgdgdfdfgdfhdghdfghdgfhgfdgar^^°gfra7685&%§$\"$§&(=)(&=,// \\", "F77A4A9DFFCB33CB8D7CE078A6DE1BAA")]
        // Default encoding
        [TestCase(false, "", "da39a3ee5e6b4b0d3255bfef95601890afd80709")]
        [TestCase(false, " ", "b858cb282617fb0956d960215c8e84d1ccf909c6")]
        [TestCase(false, "1234dsfa", "644977634278d36d5f451961fe19622ab13cec87")]
        [TestCase(false, "1-2-3-45-6", "5c13bf8b7ff1d43869a7b4246bef897f9499833b")]
        [TestCase(false, "öüüä%&&", "9082e437598f22b26f5f8b74ffe02fb54afa45bb")]
        [TestCase(false, "+- fdgsdgafdgffdsfddgdgdfdfgdfhdghdfghdgfhgfdgar^^°gfra7685&%§$\"$§&(=)(&=,// \\", "F77A4A9DFFCB33CB8D7CE078A6DE1BAA")]
        public void CreateSha1_WhenHashMustBeHexadecimal_RegressionTests(bool utf8, string valueToEncrypt, string expectedHash)
        {
            //Act
            var actualHash = valueToEncrypt.CreateSha1(utf8 ? Encoding.UTF8 : Encoding.Default, hashAsHexadecimal: true);
            //Assert
            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }
    }
}