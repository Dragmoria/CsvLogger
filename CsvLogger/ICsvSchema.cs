using System;

namespace CsvLogger
{
    /// <summary>
    /// Defines the contract for a CSV schema, which includes properties for tracking
    /// the start and end times of a log entry.
    /// </summary>
    public interface ICsvSchema
    {
        /// <summary>
        /// Gets or sets the start date and time of the log entry.
        /// </summary>
        DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the log entry.
        /// </summary>
        DateTime EndDateTime { get; set; }
    }

    public struct Test : ICsvSchema
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public string Field1 { get; set; }
    }
}