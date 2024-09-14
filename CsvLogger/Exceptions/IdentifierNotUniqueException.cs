using System;

namespace CsvLogger.Exceptions
{
    public class IdentifierNotUniqueException : Exception
    {
        public IdentifierNotUniqueException(string message) : base(message)
        {
        }
    }
}