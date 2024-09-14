using System;
using System.IO;

namespace CsvLogger.Exceptions
{
    public class IncorrectFileTypeException : Exception
    {
        public IncorrectFileTypeException(string message) : base(message)
        {
        }

        public static void ThrowIfIncorrectFileType(FileInfo file, string expectedFileType)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            if (expectedFileType == null) throw new ArgumentNullException(nameof(expectedFileType));

            if (!file.Extension.Contains(expectedFileType)) throw new IncorrectFileTypeException($"The file type of {file.FullName} does not match the expected file type of {expectedFileType}");
        }
    }
}