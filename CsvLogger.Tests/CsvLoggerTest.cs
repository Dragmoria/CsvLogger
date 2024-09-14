using System.Reflection;
using CsvLogger.Data;

namespace CsvLogger.Tests;

internal class TestCsvSchema : ICsvSchema
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string TestResult1 { get; set; }
    public string TestResult2 { get; set; }
    public string TestResult3 { get; set; }
    public string TestResult4 { get; set; }
    public string TestResult5 { get; set; }
}


internal class SecondTestCsvSchema : TestCsvSchema
{
    public string NewHeading { get; set; }
}

[TestClass]
public class CsvLoggerTest
{
    private static List<string> GetExpectedHeadings()
    {
        return typeof(TestCsvSchema).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(prop => prop.Name)
            .ToList();
    }

    private static readonly DirectoryInfo _tempDirectoryInfo = new DirectoryInfo("./output/");

    [TestMethod]
    public void GetHeadings_WhenClassConstructedWithProperParameters_ShouldInstantiateDataWithCorrectClass()
    {
        // Check to see if the normal use case works
        var maxFileSize = FileSize.FromGb(1);
        var delimiter = ';';

        var expectedHeadings = GetExpectedHeadings();

        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, maxFileSize, delimiter);

        Assert.IsTrue(expectedHeadings.SequenceEqual(logger.GetHeadings()));

        ClassCleanup();
    }

    [TestMethod]
    public void GetValues_AfterAlteringAValue_HasCorrectValues()
    {
        var expectedValue = "Expected value";

        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo);

        logger.Data.TestResult1 = expectedValue;

        var actualValues = logger.GetValues();

        Assert.IsTrue(actualValues.Contains(expectedValue));

        ClassCleanup();
    }

    [TestMethod]
    public void Constructor_WhenClassIsInstantiated_ShouldSetStartDateTime()
    {
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo);

        Assert.IsNotNull(logger.Data.StartDateTime);
        Assert.IsTrue(logger.Data.StartDateTime <= DateTime.Now);

        ClassCleanup();
    }

    [TestMethod]
    public void Constructor_WhenClassIsInstantiated_ShouldCreateLogDirectoryAndFile()
    {
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo);

        Assert.IsTrue(_tempDirectoryInfo.Exists);
        Assert.IsTrue(_tempDirectoryInfo.GetFiles().Length > 0);

        ClassCleanup();
    }

    [TestMethod]
    public void Constructor_AfterCreatingFile_HeadingsShouldBeConsistentWithType()
    {
        var delimiter = ';';
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter);

        var headings = new List<string>();

        using (var reader = new StreamReader(_tempDirectoryInfo.GetFiles().FirstOrDefault().FullName))
        {
            var firstLine = reader.ReadLine();

            headings = firstLine.Split(delimiter).ToList();
        }

        Assert.IsTrue(headings.SequenceEqual(GetExpectedHeadings()));

        ClassCleanup();
    }



    [TestMethod]
    public void WriteLogLine_WhenAllDataPointsHaveBeenSet_FileShouldHaveEqualData()
    {
        var delimiter = ';';
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter);

        logger.Data.TestResult1 = "a";
        logger.Data.TestResult2 = "b";
        logger.Data.TestResult3 = "c";
        logger.Data.TestResult4 = "d";
        logger.Data.TestResult5 = "e";

        logger.WriteLogLine();

        var expectedData = logger.GetValues();
        var dataInFile = new List<string>();

        using (var reader = new StreamReader(_tempDirectoryInfo.GetFiles().FirstOrDefault().FullName))
        {
            var firstLine = reader.ReadLine();
            var secondLine = reader.ReadLine();

            dataInFile = secondLine.Split(delimiter).ToList();
        }

        Assert.IsTrue(expectedData.SequenceEqual(dataInFile));

        ClassCleanup();
    }

    [TestMethod]
    public void WriteLogLine_WhenWritingTwice_TwoLinesOfDataArePresent()
    {
        var delimiter = ';';
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter);

        logger.Data.TestResult1 = "a";
        logger.Data.TestResult2 = "b";
        logger.Data.TestResult3 = "c";
        logger.Data.TestResult4 = "d";
        logger.Data.TestResult5 = "e";

        logger.WriteLogLine();
        logger.WriteLogLine();

        using (var reader = new StreamReader(_tempDirectoryInfo.GetFiles().FirstOrDefault().FullName))
        {
            var firstLine = reader.ReadLine();
            var secondLine = reader.ReadLine();
            var thirdLine = reader.ReadLine();

            Assert.IsNotNull(firstLine);
            Assert.IsNotNull(secondLine);
            Assert.IsNotNull(thirdLine);
        }

        ClassCleanup();
    }

    [TestMethod]
    public void WriteLogLine_WhenWritingASecondLine_BothLinesShouldHaveSameValues()
    {
        var delimiter = ';';
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter);

        logger.Data.TestResult1 = "a";
        logger.Data.TestResult2 = "b";
        logger.Data.TestResult3 = "c";
        logger.Data.TestResult4 = "d";
        logger.Data.TestResult5 = "e";

        logger.WriteLogLine();
        logger.WriteLogLine();

        using (var reader = new StreamReader(_tempDirectoryInfo.GetFiles().FirstOrDefault().FullName))
        {
            var firstLine = reader.ReadLine();
            var secondLine = reader.ReadLine();
            var thirdLine = reader.ReadLine();

            Assert.IsNotNull(firstLine);
            Assert.AreEqual(secondLine, thirdLine);
        }

        ClassCleanup();
    }

    [TestMethod]
    public void WriteLogLine_WhenSchemaChanges_NewFileIsCreated()
    {
        var delimiter = ';';
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter);

        var logger2 = new CsvLogger<SecondTestCsvSchema>(_tempDirectoryInfo, delimiter);

        Assert.IsTrue(_tempDirectoryInfo.GetFiles().Length > 1);

        ClassCleanup();
    }

    [TestMethod]
    public void WriteLogLine_WhenDelimiterChanges_NewFileIsCreatedAndOldFileRemains()
    {
        var delimiter1 = ';';
        var delimiter2 = ',';
        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter1);

        var logger2 = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, delimiter2);

        Assert.IsTrue(_tempDirectoryInfo.GetFiles().Length > 1);

        ClassCleanup();
    }

    /* This one here made me realize why testing needs to happen. I wanted to document the instance for myself. The idea was that a log file grows and grows and should
    // eventually stop growing if it reached a given maximum file size. 
    // Here is how I did that before this test:
    // */

    /* This one here made me realize why testing needs to happen. I wanted to document the instance for myself. The idea was that a log file grows and grows and should
     * eventually stop growing if it reached a given maximum file size. 
     * Here is how I did that before this test:
     * public virtual void WriteLogLine()
        {
            if (new FileSize(_latestFile.Length) > _maxFileSize)
            {
                _latestFile = GenerateNewLogFile();
            }
        
            AppendToFile(GetValues(), _latestFile);
        }
     *
     * Here _latestFile is a FileInfo object. When writing this it did not occur to me that the Length field would not be a live representation of the actual file.
     * Just a representation of the moment we initialized the object. This meant that as long as the program runs we would never reach a file size bigger than the max
     * since it always stays the same size in our check. Well whooops indeed. :)
     */
    [TestMethod]
    public void WriteLogLine_WhenFileGetsTooBig_NewFileIsCreated()
    {
        var delimiter = ';';
        var fileSize = new FileSize(100);

        var logger = new CsvLogger<TestCsvSchema>(_tempDirectoryInfo, fileSize, delimiter);

        logger.Data.TestResult1 = "a";
        logger.Data.TestResult2 = "b";
        logger.Data.TestResult3 = "c";
        logger.Data.TestResult4 = "d";
        logger.Data.TestResult5 = "e";

        logger.WriteLogLine();
        logger.WriteLogLine();
        logger.WriteLogLine();

        Assert.IsTrue(_tempDirectoryInfo.GetFiles().Length > 1);

        ClassCleanup();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        // remove log files
        if (!_tempDirectoryInfo.Exists) return;

        foreach (var fileInfo in _tempDirectoryInfo.GetFiles())
        {
            fileInfo.Delete();
        }

        _tempDirectoryInfo.Delete();
    }
}