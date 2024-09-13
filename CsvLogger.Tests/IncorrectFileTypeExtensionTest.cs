using CsvLogger.Exceptions;

namespace CsvLogger.Tests
{
    [TestClass]
    public class IncorrectFileTypeExtensionTest
    {
        [TestMethod]
        public void ThrowIfIncorrectFileType_ThrowsArgumentNullException_ForNullFile()
        {
            FileInfo nullFile = null;
            string expectedFileType = ".csv";

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
            string expectedFileType = ".csv";

            var ex = Assert.ThrowsException<IncorrectFileTypeException>(() =>
                IncorrectFileTypeException.ThrowIfIncorrectFileType(file, expectedFileType)
            );

            StringAssert.Contains(ex.Message, "The file type of test.txt does not match the expected file type of .csv");
        }

        [TestMethod]
        public void ThrowIfIncorrectFileType_DoesNotThrowException_ForCorrectFileType()
        {
            var file = new FileInfo("test.csv");
            string expectedFileType = ".csv";

            IncorrectFileTypeException.ThrowIfIncorrectFileType(file, expectedFileType);
        }
    }
}