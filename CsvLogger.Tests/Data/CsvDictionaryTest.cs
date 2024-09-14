using System.Xml.Schema;
using CsvLogger.Data;
using CsvLogger.Exceptions;

namespace CsvLogger.Tests.Data;

[TestClass]
public class CsvDictionaryTest
{
    private const string VALIDXMLFILEPATH = @"./Data/ValidTestCsvSchema.xml";
    private const string INVALIDXMLFILEPATH = @"./Data/InvalidTestCsvSchema.xml";

    private readonly Dictionary<string, Type> _expectedKeys = new Dictionary<string, Type>()
    {
        { "StartDateTime", typeof(DateTime) },
        { "EndDateTime", typeof(DateTime) },
        { "Identifier", typeof(Guid) },
        { "Field1", typeof(string) },
        { "Field2", typeof(int) },
        { "Field3", typeof(bool) },
        { "Field4", typeof(double) },
        { "Field5", typeof(decimal) },
        { "Field6", typeof(float) },
        { "Field7", typeof(char) },
        { "Field8", typeof(long) }
    };

    [TestMethod]
    public void LoadSchema_WithValidXmlFile_ShouldCreateExpectedDictionary()
    {
        var testSchema = new FileInfo(VALIDXMLFILEPATH);

        var csvDict = new CsvDictionary();

        csvDict.LoadSchema(testSchema);

        var headings = csvDict.GetHeadings();

        var internalTypes = csvDict.GetTypes().Select(t => t.GetGenericArguments().FirstOrDefault()).ToList();

        Assert.IsNotNull(headings);
        Assert.IsNotNull(internalTypes);
        Assert.IsTrue(_expectedKeys.Keys.ToList().SequenceEqual(headings));
        Assert.IsTrue(_expectedKeys.Values.ToList().SequenceEqual(internalTypes));
    }

    [TestMethod]
    public void LoadScheme_WithNonXmlFileType_ShouldThrowIncorrectFileTypeException()
    {
        var wrongFile = new FileInfo("test.txt");

        Assert.ThrowsException<IncorrectFileTypeException>(() =>
        {
            var csvDict = new CsvDictionary();
            csvDict.LoadSchema(wrongFile);
        });
    }

    [TestMethod]
    public void LoadSchema_WithInvalidXmlFile_ShouldThrowXmlSchemaValidationException()
    {
        var invalidTestSchema = new FileInfo(INVALIDXMLFILEPATH);

        Assert.ThrowsException<XmlSchemaValidationException>(() =>
        {
            var csvDict = new CsvDictionary();
            csvDict.LoadSchema(invalidTestSchema);
        });
    }

    [TestMethod]
    public void GetValue_AfterAValueIsSet_ShouldRepresentTheCorrectValue()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        var expectedValue = "TestValue";
        var key = "Field1";

        csvDictionary.SetValue<string>(key, expectedValue);

        Assert.AreEqual<string>(expectedValue, csvDictionary.GetValue<string>(key));
    }

    [TestMethod]
    public void GetValue_WithUpperAndLowerCaseKey_ShouldReturnTheSameField()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        var expectedValue = "TestValue";
        var key = "Field1";

        csvDictionary.SetValue<string>(key, expectedValue);

        Assert.AreEqual<string>(expectedValue, csvDictionary.GetValue<string>(key.ToUpper()));
        Assert.AreEqual<string>(expectedValue, csvDictionary.GetValue<string>(key.ToLower()));
    }

    [TestMethod]
    public void GetValue_WhenPassedNullAsKey_ShouldThrowArgumentNullException()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            csvDictionary.GetValue(null);
        });
    }

    [TestMethod]
    public void GetValue_WhenPassedUnknownKey_ShouldThrowArgumentException()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        Assert.ThrowsException<ArgumentException>(() =>
        {
            csvDictionary.GetValue("NotAKey");
        });
    }

    [TestMethod]
    public void GetValue_WhenGettingValueWithIncorrectType_ShouldThrowInvalidCastException()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        Assert.ThrowsException<InvalidCastException>(() =>
        {
            csvDictionary.GetValue<string>("StartDateTime");
        });
    }

    [TestMethod]
    public void SetValue_WhenGivenTheCorrectTypeForAKey_ShouldProperlyUpdateTheValue()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        var key = "Field1";
        string currentValue = csvDictionary.GetValue<string>(key);
        string expectedNewValue = "NewValue";

        csvDictionary.SetValue(key, expectedNewValue);

        Assert.AreEqual<string>(expectedNewValue, csvDictionary.GetValue<string>(key));
        Assert.AreNotEqual<string>(currentValue, csvDictionary.GetValue<string>(key));
    }

    [TestMethod]
    public void SetValue_WhenGivenUpperOrLowercaseKey_ShouldInBothCasesSetTheValue()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        var key = "Field1";
        string currentValue = csvDictionary.GetValue<string>(key);
        string expectedNewValue1 = "NewValue1";
        string expectedNewValue2 = "NewValue2";

        csvDictionary.SetValue(key, expectedNewValue1);
        Assert.AreEqual<string>(expectedNewValue1, csvDictionary.GetValue<string>(key.ToUpper()));

        csvDictionary.SetValue(key, expectedNewValue2);
        Assert.AreEqual<string>(expectedNewValue2, csvDictionary.GetValue<string>(key.ToLower()));
    }

    [TestMethod]
    public void SetValue_WhenPassedNullAsKey_ShouldThrowArgumentNullException()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            csvDictionary.SetValue(null, "");
        });
    }

    [TestMethod]
    public void SetValue_WhenPassedUnknownKey_ShouldThrowArgumentException()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        Assert.ThrowsException<ArgumentException>(() =>
        {
            csvDictionary.SetValue("NotAKey", "");
        });
    }

    private static object? GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

    [TestMethod]
    public void GetValues_WhenJustInitialized_ValuesAreTheDefaultOfTheirType()
    {
        var csvDictionary = new CsvDictionary();
        csvDictionary.LoadSchema(new FileInfo(VALIDXMLFILEPATH));

        var values = csvDictionary.GetValues();
        var expectedValue = _expectedKeys.Select(v => GetDefaultValue(v.Value)?.ToString()).ToList();

        Assert.IsTrue(values.SequenceEqual(expectedValue));
    }
}