using CsvLogger.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvLogger
{
    /// <summary>
    /// Base class for CsvLogger. Implements functions any of the CsvLoggers will need. The way the data is managed is left to the child.
    /// </summary>
    public abstract class BaseCsvLogger
    {
        protected readonly char _delimiter;
        protected readonly FileSize _maxFileSize;
        protected readonly DirectoryInfo _outputDirectory;
        protected FileInfo _latestFile;

        protected BaseCsvLogger(DirectoryInfo outputDirectory, FileSize maxFileSize, char delimiter)
        {
            _outputDirectory = outputDirectory;
            _maxFileSize = maxFileSize;
            _delimiter = delimiter;
        }

        /// <summary>
        /// Gets the headings for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of headings.</returns>
        public abstract List<string> GetHeadings();

        /// <summary>
        /// Gets the values for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of values.</returns>
        public abstract List<string> GetValues();

        /// <summary>
        /// Checks if the headings in the specified file are consistent with the schema.
        /// </summary>
        /// <param name="fileInfo">The file to check.</param>
        /// <returns><c>true</c> if the headings are consistent; otherwise, <c>false</c>.</returns>
        protected bool AreHeadingsConsistent(FileInfo fileInfo)
        {
            if (fileInfo.Length == 0)
            {
                fileInfo.Delete();
                return false;
            }

            using (var reader = new StreamReader(fileInfo.FullName))
            {
                string line;
                string nonWhiteSpaceLine = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        nonWhiteSpaceLine = line;
                        break;
                    }
                }

                if (nonWhiteSpaceLine == null)
                {
                    fileInfo.Delete();
                    return false;
                }

                var itemsOnLine = nonWhiteSpaceLine.Split(_delimiter);
                var headings = GetHeadings();

                return headings.SequenceEqual(itemsOnLine);
            }
        }

        /// <summary>
        /// Appends a list of items to the specified file.
        /// </summary>
        /// <param name="items">The items to append.</param>
        /// <param name="fileInfo">The file to append to.</param>
        protected void AppendToFile(List<string> items, FileInfo fileInfo)
        {
            using (var writer = File.AppendText(fileInfo.FullName))
            {
                writer.WriteLine(string.Join(_delimiter.ToString(), items));
            }
        }

        /// <summary>
        /// Generates a new log file with a unique name.
        /// </summary>
        /// <returns>The newly created log file.</returns>
        protected FileInfo GenerateNewLogFile()
        {
            string fileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var newFile = new FileInfo(Path.Combine(_outputDirectory.FullName, fileName));

            using (var _ = newFile.Create()) { }
            AppendToFile(GetHeadings(), newFile);
            return newFile;
        }

        /// <summary>
        /// Gets the most recent valid CSV file, or creates a new one if necessary.
        /// </summary>
        /// <returns>The most recent valid CSV file, or a new file if none are valid.</returns>
        protected FileInfo GetLatestValidFile()
        {
            var files = _outputDirectory.GetFiles("*.csv");
            if (files.Length == 0)
            {
                return GenerateNewLogFile();
            }

            var mostRecentFile = files.OrderByDescending(f => f.LastWriteTime).First();

            if (AreHeadingsConsistent(mostRecentFile))
            {
                return mostRecentFile;
            }

            return GenerateNewLogFile();
        }

        /// <summary>
        /// Writes a new log line to the CSV file.
        /// Updates the start and end time of the log entry.
        /// </summary>
        public virtual void WriteLogLine()
        {
            if (new FileSize(_latestFile.Length) > _maxFileSize)
            {
                _latestFile = GenerateNewLogFile();
            }

            AppendToFile(GetValues(), _latestFile);
        }
    }
}