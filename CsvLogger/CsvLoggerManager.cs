using CsvLogger.Data;
using CsvLogger.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsvLogger
{
    /// <summary>
    /// Manages instances of <see cref="CsvLogger{CsvSchemaType}"/> for different schema types.
    /// </summary>
    public static class CsvLoggerManager
    {
        private static readonly IDictionary<Type, object> _loggers = new Dictionary<Type, object>();
        private static readonly IDictionary<string, XmlSchemaCsvLogger> _dynamicLoggers = new Dictionary<string, XmlSchemaCsvLogger>(StringComparer.OrdinalIgnoreCase);

        private static readonly List<string> _inUseDirectories = new List<string>();

        private static bool IsDirectoryInUse(DirectoryInfo directory)
        {
            if (_inUseDirectories.Contains(directory.FullName))
            {
                return true;
            }
            _inUseDirectories.Add(directory.FullName);
            return false;
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type
        /// </summary>
        /// <typeparam name="TCsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<TCsvSchemaType>(DirectoryInfo outputDirectory) where TCsvSchemaType : class, ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<TCsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(TCsvSchemaType), new CsvLogger<TCsvSchemaType>(outputDirectory));
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type
        /// </summary>
        /// <typeparam name="TCsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<TCsvSchemaType>(DirectoryInfo outputDirectory, char delimiter) where TCsvSchemaType : class, ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<TCsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(TCsvSchemaType), new CsvLogger<TCsvSchemaType>(outputDirectory, delimiter));
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type.
        /// </summary>
        /// <typeparam name="TCsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<TCsvSchemaType>(DirectoryInfo outputDirectory, FileSize maxFileSize) where TCsvSchemaType : class, ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<TCsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(TCsvSchemaType), new CsvLogger<TCsvSchemaType>(outputDirectory, maxFileSize));
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type.
        /// </summary>
        /// <typeparam name="TCsvSchemaType"></typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<TCsvSchemaType>(DirectoryInfo outputDirectory, char delimiter, FileSize maxFileSize) where TCsvSchemaType : class, ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<TCsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(TCsvSchemaType), new CsvLogger<TCsvSchemaType>(outputDirectory, maxFileSize, delimiter));
        }

        private static void ThrowIfNotValidRegistration<TCsvSchemaType>(DirectoryInfo outputDirectory) where TCsvSchemaType : ICsvSchema, new()
        {
            if (outputDirectory is null) throw new ArgumentNullException(nameof(outputDirectory));

            if (IsDirectoryInUse(outputDirectory)) throw new DirectoryAlreadyInUseException($"The directory with path ({outputDirectory.FullName}) is already being used by a different logger.");

            if (_loggers.TryGetValue(typeof(TCsvSchemaType), out _)) throw new SchemaTypeAlreadyRegisteredException($"A logger for schema ({typeof(TCsvSchemaType).FullName}) has already been registered.");
        }

        /// <summary>
        /// Retrieves the registered <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type.
        /// </summary>
        /// <typeparam name="TCsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <returns>The <see cref="CsvLogger{CsvSchemaType}"/> instance associated with the specified schema type.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the logger for the specified schema type is not found.</exception>
        public static CsvLogger<TCsvSchemaType> GetLogger<TCsvSchemaType>() where TCsvSchemaType : class, ICsvSchema, new()
        {
            if (_loggers.TryGetValue(typeof(TCsvSchemaType), out var logger))
            {
                return logger as CsvLogger<TCsvSchemaType>;
            }
            throw new KeyNotFoundException($"No logger was found that uses {typeof(TCsvSchemaType).FullName} as schema.");
        }

        /// <summary>
        /// Registers a <see cref="XmlSchemaCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">FileInfo of the schema file to use.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new XmlSchemaCsvLogger(outputDirectory, schemaFile));
        }

        /// <summary>
        /// Registers a <see cref="XmlSchemaCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">FileInfo of the schema file to use.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile, char delimiter)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new XmlSchemaCsvLogger(outputDirectory, schemaFile, delimiter));
        }

        /// <summary>
        /// Registers a <see cref="XmlSchemaCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">FileInfo of the schema file to use.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile, FileSize maxFileSize)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new XmlSchemaCsvLogger(outputDirectory, schemaFile, maxFileSize));
        }

        /// <summary>
        /// Registers a <see cref="XmlSchemaCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="schemaFile">FileInfo of the schema file to use.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile, char delimiter, FileSize maxFileSize)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new XmlSchemaCsvLogger(outputDirectory, schemaFile, maxFileSize, delimiter));
        }

        private static void ThrowIfNotValidRegistration(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile)
        {
            ValidateIdentifier(identifier);

            if (outputDirectory is null) throw new ArgumentNullException(nameof(outputDirectory));

            if (schemaFile is null) throw new ArgumentNullException(nameof(schemaFile));

            if (IsDirectoryInUse(outputDirectory)) throw new DirectoryAlreadyInUseException($"The directory with path ({outputDirectory.FullName}) is already being used by a different logger.");

            if (_dynamicLoggers.TryGetValue(identifier, out _)) throw new IdentifierNotUniqueException($"A logger with identifier ({identifier}) has already been registered.");
        }

        /// <summary>
        /// Retrieves the registered <see cref="XmlSchemaCsvLogger"/> for the specified schema type.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <returns>The <see cref="XmlSchemaCsvLogger"/> instance associated with the specified identifier.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the logger for the specified identifier is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static XmlSchemaCsvLogger GetDynamicLogger(string identifier)
        {
            ValidateIdentifier(identifier);

            if (_dynamicLoggers.TryGetValue(identifier, out var logger))
            {
                return logger;
            }
            throw new KeyNotFoundException($"No logger was found that uses ({identifier}) as identifier.");
        }

        private static void ValidateIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentException($"Argument ({nameof(identifier)})  null of empty.");
        }
    }
}