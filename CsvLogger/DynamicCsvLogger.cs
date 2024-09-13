using CsvLogger.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsvLogger
{
    /// <summary>
    /// A logger for writing CSV data with dynamic schema.
    /// </summary>
    public class DynamicCsvLogger : BaseCsvLogger
    {
        public CsvDictionary Data { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DynamicCsvLogger"/> class. Whith default delimiter of ';' and maxfilesize of 10gb
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">The FileInfo of the schema file.</param>
        public DynamicCsvLogger(DirectoryInfo outputDirectory, FileInfo schemaFile) : this(outputDirectory, schemaFile, FileSize.FromGB(10), ';') { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCsvLogger"/> class. With default delimiter of ';'.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">The FileInfo of the schema file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        public DynamicCsvLogger(DirectoryInfo outputDirectory, FileInfo schemaFile, FileSize maxFileSize) : this(outputDirectory, schemaFile, maxFileSize, ';') { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCsvLogger"/> class. With default maxfilesize of 10gb.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">The FileInfo of the schema file.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        public DynamicCsvLogger(DirectoryInfo outputDirectory, FileInfo schemaFile, char delimiter) : this(outputDirectory, schemaFile, FileSize.FromGB(10), delimiter) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCsvLogger"/> class.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">The FileInfo of the schema file.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        public DynamicCsvLogger(DirectoryInfo outputDirectory, FileInfo schemaFile, FileSize maxFileSize, char delimiter) : base(outputDirectory, maxFileSize, delimiter)
        {
            Data = new CsvDictionary(schemaFile);

            if (!_outputDirectory.Exists)
            {
                _outputDirectory.Create();
            }
            _latestFile = GetLatestValidFile();

            Data.SetValue(CsvDictionary.STARTTIMEKEY, DateTime.Now);
        }

        /// <summary>
        /// Writes a new log line to the CSV file.
        /// Updates the start and end time of the log entry.
        /// </summary>
        public override void WriteLogLine()
        {
            Data.SetValue(CsvDictionary.ENDTIMEKEY, DateTime.Now);
            base.WriteLogLine();
        }

        /// <summary>
        /// Gets the headings for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of headings.</returns>
        public override List<string> GetHeadings() => Data.GetHeadings();

        /// <summary>
        /// Gets the values for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of values.</returns>
        public override List<string> GetValues() => Data.GetValues();
    }
}