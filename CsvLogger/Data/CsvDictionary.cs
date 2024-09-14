using CsvLogger.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace CsvLogger.Data
{
    /// <summary>
    /// Represents a dictionary that maps column names to typed values and supports schema validation. Disables the adding of new keys.
    /// </summary>
    public class CsvDictionary
    {
        private const string XSDFILEPATH = @"./Data/Schema.xsd";
        public const string STARTTIMEKEY = "StartDateTime";
        public const string ENDTIMEKEY = "EndDateTime";

        /// <summary>
        /// The dictionary that holds the column names and their associated typed values.
        /// </summary>
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDictionary"/> class.
        /// </summary>
        /// <param name="schemaFile">The FileInfo of the schema file.</param>
        public CsvDictionary(FileInfo schemaFile)
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            LoadSchema(schemaFile);
        }

        public CsvDictionary()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the value for a specified key in the dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the dictionary entry.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the dictionary does not contain the specified <paramref name="key"/>.</exception>
        public void SetValue<T>(string key, T value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!_dictionary.TryGetValue(key, out var untypedValue)) throw new ArgumentException($"Dictionary does not contain a key of {key}");

            var typedValue = (TypedValue<T>)untypedValue;
            typedValue.SetValue(value);
        }

        public TypedValue<T> GetValue<T>(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (!_dictionary.TryGetValue(key, out var untypedValue)) throw new ArgumentException($"Dictionary does not contain a key of {key}");

            return (TypedValue<T>)untypedValue;
        }

        public object GetValue(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (!_dictionary.TryGetValue(key, out var untypedValue)) throw new ArgumentException($"Dictionary does not contain a key of {key}");

            return untypedValue;
        }

        /// <summary>
        /// Loads the schema from the specified XML file and populates the dictionary.
        /// </summary>
        /// <param name="schemaFile">The XML file that contains the schema definition.</param>
        /// <exception cref="IncorrectFileTypeException">Thrown when the file type is incorrect.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file can't be found.</exception>
        public void LoadSchema(FileInfo schemaFile)
        {
            IncorrectFileTypeException.ThrowIfIncorrectFileType(schemaFile, "xml");

            var xsdSchema = new XmlSchemaSet();
            xsdSchema.Add("", XSDFILEPATH);

            var settings = new XmlReaderSettings
            {
                Schemas = xsdSchema,
                ValidationType = ValidationType.Schema
            };
            settings.ValidationEventHandler += Settings_ValidationEventHandler;

            using (var reader = XmlReader.Create(schemaFile.FullName, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "column") continue;
                    
                    var columnName = reader.GetAttribute("name");
                    var typeAbbreviation = reader.GetAttribute("type");

                    if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(typeAbbreviation))
                    {
                        AddTypedValueToDictionary(columnName, typeAbbreviation);
                    }
                }
            }

            AddStartAndEndTime();
        }

        /// <summary>
        /// Adds the start and end time columns to the dictionary if they do not already exist.
        /// </summary>
        private void AddStartAndEndTime()
        {
            if (!_dictionary.ContainsKey(STARTTIMEKEY)) _dictionary.Add(STARTTIMEKEY, new TypedValue<DateTime>());

            if (!_dictionary.ContainsKey(ENDTIMEKEY)) _dictionary.Add(ENDTIMEKEY, new TypedValue<DateTime>());
        }

        /// <summary>
        /// Adds a typed value to the dictionary based on the column name and type abbreviation.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="typeAbbreviation">The type abbreviation for the column.</param>
        /// <exception cref="InvalidOperationException">Thrown when the type abbreviation is unsupported.</exception>
        private void AddTypedValueToDictionary(string columnName, string typeAbbreviation)
        {
            switch (typeAbbreviation)
            {
                case "s":
                    _dictionary.Add(columnName, new TypedValue<string>());
                    break;

                case "i":
                    _dictionary.Add(columnName, new TypedValue<int>());
                    break;

                case "b":
                    _dictionary.Add(columnName, new TypedValue<bool>());
                    break;

                case "d":
                    _dictionary.Add(columnName, new TypedValue<double>());
                    break;

                case "dt":
                    _dictionary.Add(columnName, new TypedValue<DateTime>());
                    break;

                case "dec":
                    _dictionary.Add(columnName, new TypedValue<decimal>());
                    break;

                case "g":
                    _dictionary.Add(columnName, new TypedValue<Guid>());
                    break;

                case "f":
                    _dictionary.Add(columnName, new TypedValue<float>());
                    break;

                case "c":
                    _dictionary.Add(columnName, new TypedValue<char>());
                    break;

                case "l":
                    _dictionary.Add(columnName, new TypedValue<long>());
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported type abbreviation: {typeAbbreviation}");
            }
        }

        /// <summary>
        /// Handles validation events when XML schema validation fails.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The validation event arguments.</param>
        /// <exception cref="XmlSchemaValidationException">Thrown when XML schema validation fails.</exception>
        private static void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            throw new XmlSchemaValidationException("Xsd -> xml validation failed.");
        }

        /// <summary>
        /// Gets the headings for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of headings.</returns>
        public List<string> GetHeadings() => _dictionary.Keys.ToList();

        /// <summary>
        /// Gets the values for the CSV file from the properties of the schema type.
        /// </summary>
        /// <returns>A list of values.</returns>
        public List<string> GetValues()
        {
            return _dictionary.Values.Select(value => value.ToString()).ToList();
        }

        public List<Type> GetTypes()
        {
            return _dictionary.Values.Select(value => value.GetType()).ToList();
        }
    }
}