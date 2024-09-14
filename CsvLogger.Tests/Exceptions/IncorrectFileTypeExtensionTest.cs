using CsvLogger.Exceptions;

namespace CsvLogger.Tests.Exceptions;

[TestClass]
public class IncorrectFileTypeExtensionTest
{
    [TestMethod]
    public void ThrowIfIncorrectFileType_ThrowsArgumentNullException_ForNullFile()
    {
        FileInfo nullFile = null;
        var expectedFileType = ".csv";

        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            IncorrectFileTypeException.ThrowIfIncorrectFileType(nullFile, expectedFileType)
        );

        Assert.AreEqual("file", ex.ParamName);
    }

    [TestMethod]
    public void ThrowIfIncorrectFileType_ThrowsArgumentNullException_ForNullFileType()
    {
        var file = new FileInfo("test.csv");
        string nullFileType = null;

        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            IncorrectFileTypeException.ThrowIfIncorrectFileType(file, nullFileType)
        );

        Assert.AreEqual("expectedFileType", ex.ParamName);
    }

    [TestMethod]
    public void ThrowIfIncorrectFileType_ThrowsIncorrectFileTypeException_ForMismatchedFileType()
    {
        var file = new FileInfo("test.txt");
        var expectedFileType = ".csv";
        var expectedFileTypeWithoutDot = "csv";

        Assert.ThrowsException<IncorrectFileTypeException>(() =>
            IncorrectFileTypeException.ThrowIfIncorrectFileType(file, expectedFileType)
        );

        Assert.ThrowsException<IncorrectFileTypeException>(() =>
            IncorrectFileTypeException.ThrowIfIncorrectFileType(file, expectedFileTypeWithoutDot)
        );
    }

    [TestMethod]
    public void ThrowIfIncorrectFileType_DoesNotThrowException_ForCorrectFileType()
    {
        var file = new FileInfo("test.csv");
        var expectedFileType = ".csv";
        var expectedFileTypeWithoutDot = "csv";

        IncorrectFileTypeException.ThrowIfIncorrectFileType(file, expectedFileType);
        IncorrectFileTypeException.ThrowIfIncorrectFileType(file, expectedFileTypeWithoutDot);
    }
}