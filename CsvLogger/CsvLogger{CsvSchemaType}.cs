using CsvLogger.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CsvLogger
{
    /// <summary>
    /// A logger for writing CSV data with a specified schema.
    /// </summary>
    /// <typeparam name="TCsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
    public class CsvLogger<TCsvSchemaType> : BaseCsvLogger where TCsvSchemaType : class, ICsvSchema, new()
    {
        /// <summary>
        /// Gets or sets the data to be logged.
        /// </summary>
        public TCsvSchemaType Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLogger{CsvSchemaType}"/> class. With default delimiter of ';' and maxfilesize of 10gb.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        public CsvLogger(DirectoryInfo outputDirectory) : this(outputDirectory, FileSize.FromGb(10), ';') { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLogger{CsvSchemaType}"/> class. With default delimiter of ';'.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        public CsvLogger(DirectoryInfo outputDirectory, FileSize maxFileSize) : this(outputDirectory, maxFileSize, ';') { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLogger{CsvSchemaType}"/> class. With default maxfilesize of 10gb.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        public CsvLogger(DirectoryInfo outputDirectory, char delimiter) : this(outputDirectory, FileSize.FromGb(10), delimiter) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLogger{CsvSchemaType}"/> class.
        /// </summary>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        public CsvLogger(DirectoryInfo outputDirectory, FileSize maxFileSize, char delimiter) : base(outputDirectory, maxFileSize, delimiter)
        {
            Data = new TCsvSchemaType();

            if (!_outputDirectory.Exists) _outputDirectory.Create();

            _latestFile = GetLatestValidFile();

            Data.StartDateTime = DateTime.Now;
        }

        public override void WriteLogLine()
        {
            Data.EndDateTime = DateTime.Now;
            base.WriteLogLine();
        }

        /// <summary>
        /// Gets the headings for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of headings.</returns>
        public override List<string> GetHeadings()
        {
            return typeof(TCsvSchemaType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Select(prop => prop.Name)
                            .ToList();
        }

        /// <summary>
        /// Gets the values for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of values.</returns>
        public override List<string> GetValues()
        {
            return typeof(TCsvSchemaType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Select(prop => prop.GetValue(Data)?.ToString() ?? "")
                            .ToList();
        }

        /// <summary>
        /// Gets the schema as a read-only dictionary.
        /// </summary>
        /// <returns>A read-only dictionary of schema property names and values.</returns>
        public ReadOnlyDictionary<string, string> GetSchemaAsDictionary()
        {
            var schemaProps = typeof(TCsvSchemaType).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var schemaDict = schemaProps.ToDictionary(
                prop => prop.Name,
                prop => prop.GetValue(Data)?.ToString() ?? ""
            );

            return new ReadOnlyDictionary<string, string>(schemaDict);
        }
    }
}