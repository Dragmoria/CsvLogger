using System;

namespace CsvLogger.Exceptions
{
    public class DirectoryAlreadyInUseException : Exception
    {
        public DirectoryAlreadyInUseException(string message) : base(message)
        {
        }
    }
}