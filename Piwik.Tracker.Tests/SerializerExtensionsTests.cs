using System.Linq;
using NUnit.Framework;

namespace Piwik.Tracker.Tests
{
    [TestFixture]
    internal class SerializerExtensionsTests
    {
        private static readonly string[] ArrayToSerialize = { "314", "ilughl", "üöä", "" };
        private static readonly string SerializedArray = "[\"314\",\"ilughl\",\"üöä\",\"\"]";
        private static readonly TestClass ClassToSerialize = new TestClass();
        private const string SerializedClass = "{\"StringProperty\":\"dshsdhfg34656(/%/$\\u0026/%§üÄÖ..-;\",\"IntProperty\":34534,\"DoubleProperty\":-0.54235234,\"Childreen\":[{\"IntProperty\":34534},{\"IntProperty\":34534},{\"IntProperty\":34534}]}";

        [Test]
        public void Serialize_Test()
        {
            //Act
            var actualSerialized = ClassToSerialize.Serialize();
            //Assert
            Assert.That(actualSerialized, Is.EqualTo(SerializedClass));

            //Act
            actualSerialized = ArrayToSerialize.Serialize();
            //Assert
            Assert.That(actualSerialized, Is.EqualTo(SerializedArray));
        }

        [Test]
        public void Deserialize_Test()
        {
            //Act
            var actualDeserialized = SerializedClass.Deserialize<TestClass>();
            //Assert
            Assert.That(actualDeserialized.StringProperty, Is.EqualTo(ClassToSerialize.StringProperty));
            Assert.That(actualDeserialized.IntProperty, Is.EqualTo(ClassToSerialize.IntProperty));
            Assert.That(actualDeserialized.DoubleProperty, Is.EqualTo(ClassToSerialize.DoubleProperty));
            Assert.That(actualDeserialized.Childreen.Select(c => c.IntProperty), Is.EqualTo(ClassToSerialize.Childreen.Select(c => c.IntProperty)));

            //Act
            var actualDeserializedArray = SerializedArray.Deserialize<string[]>();
            //Assert
            Assert.That(string.Join(",", actualDeserializedArray), Is.EqualTo(string.Join(",", ArrayToSerialize)));
        }

        private class TestClass
        {
            public string StringProperty { get; set; } = "dshsdhfg34656(/%/$&/%§üÄÖ..-;";
            public int IntProperty { get; set; } = 34534;
            public double DoubleProperty { get; set; } = -0.54235234;

            public TestClassChild[] Childreen { get; set; } =
            {
                new TestClassChild(),
                new TestClassChild(),
                new TestClassChild(),
            };
        }

        private class TestClassChild
        {
            public int IntProperty { get; set; } = 34534;
        }
    }
}