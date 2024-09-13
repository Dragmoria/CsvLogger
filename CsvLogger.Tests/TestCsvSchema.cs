using CsvLogger;

namespace CsvLogger.Tests
{
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
}