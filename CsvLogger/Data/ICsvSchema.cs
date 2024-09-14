using System;

namespace CsvLogger.Data
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
}