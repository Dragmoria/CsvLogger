using CsvLogger.Data;

namespace CsvLogger.Tests.Data;

[TestClass]
public class TypedValueTest
{
    [TestMethod]
    public void SetValue_WhenGivenValidValue_ShouldSaveValue()
    {
        var value = new TypedValue<string>();
        var newValue = "test";
        value.SetValue(newValue);

        Assert.AreEqual<string>(newValue, value);
    }

    private class SomeClass
    {
        public override string ToString()
        {
            return "Expected Value";
        }
    }

    [TestMethod]
    public void ToString_WhenInvoked_ShouldReturnTheToStringValueOfT()
    {
        var value = new TypedValue<SomeClass>();
        value.SetValue(new SomeClass());

        var expectedValue = new SomeClass().ToString();

        Assert.AreEqual(expectedValue, value.ToString());
    }
}