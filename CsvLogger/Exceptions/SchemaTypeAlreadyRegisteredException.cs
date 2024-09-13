using System;

namespace CsvLogger.Exceptions
{
    internal class SchemaTypeAlreadyRegisteredException : Exception
    {
        public SchemaTypeAlreadyRegisteredException(string message) : base(message)
        {
        }
    }
}