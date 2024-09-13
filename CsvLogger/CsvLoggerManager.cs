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
        private static IDictionary<Type, object> _loggers = new Dictionary<Type, object>();
        private static IDictionary<string, DynamicCsvLogger> _dynamicLoggers = new Dictionary<string, DynamicCsvLogger>(StringComparer.OrdinalIgnoreCase);

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
        /// <typeparam name="CsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<CsvSchemaType>(DirectoryInfo outputDirectory) where CsvSchemaType : ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<CsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(CsvSchemaType), new CsvLogger<CsvSchemaType>(outputDirectory));
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type
        /// </summary>
        /// <typeparam name="CsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<CsvSchemaType>(DirectoryInfo outputDirectory, char delimiter) where CsvSchemaType : ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<CsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(CsvSchemaType), new CsvLogger<CsvSchemaType>(outputDirectory, delimiter));
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type.
        /// </summary>
        /// <typeparam name="CsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<CsvSchemaType>(DirectoryInfo outputDirectory, FileSize maxFileSize) where CsvSchemaType : ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<CsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(CsvSchemaType), new CsvLogger<CsvSchemaType>(outputDirectory, maxFileSize));
        }

        /// <summary>
        /// Registers a <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type.
        /// </summary>
        /// <typeparam name="CsvSchemaType"></typeparam>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="SchemaTypeAlreadyRegisteredException">Thrown when a logger of the given CsvSchema has already been registered.</exception>
        public static void Register<CsvSchemaType>(DirectoryInfo outputDirectory, char delimiter, FileSize maxFileSize) where CsvSchemaType : ICsvSchema, new()
        {
            ThrowIfNotValidRegistration<CsvSchemaType>(outputDirectory);

            _loggers.Add(typeof(CsvSchemaType), new CsvLogger<CsvSchemaType>(outputDirectory, maxFileSize, delimiter));
        }

        private static void ThrowIfNotValidRegistration<CsvSchemaType>(DirectoryInfo outputDirectory) where CsvSchemaType : ICsvSchema, new()
        {
            if (outputDirectory is null) throw new ArgumentNullException(nameof(outputDirectory));

            if (IsDirectoryInUse(outputDirectory)) throw new DirectoryAlreadyInUseException($"The directory with path ({outputDirectory.FullName}) is already being used by a different logger.");

            if (_loggers.TryGetValue(typeof(CsvSchemaType), out _)) throw new SchemaTypeAlreadyRegisteredException($"A logger for schema ({typeof(CsvSchemaType).FullName}) has already been registered.");
        }

        /// <summary>
        /// Retrieves the registered <see cref="CsvLogger{CsvSchemaType}"/> for the specified schema type.
        /// </summary>
        /// <typeparam name="CsvSchemaType">The type representing the CSV schema, which must implement <see cref="ICsvSchema"/> and have a parameterless constructor.</typeparam>
        /// <returns>The <see cref="CsvLogger{CsvSchemaType}"/> instance associated with the specified schema type.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the logger for the specified schema type is not found.</exception>
        public static CsvLogger<CsvSchemaType> GetLogger<CsvSchemaType>() where CsvSchemaType : ICsvSchema, new()
        {
            if (_loggers.TryGetValue(typeof(CsvSchemaType), out var logger))
            {
                return logger as CsvLogger<CsvSchemaType>;
            }
            throw new KeyNotFoundException($"No logger was found that uses {typeof(CsvSchemaType).FullName} as schema.");
        }

        /// <summary>
        /// Registers a <see cref="DynamicCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new DynamicCsvLogger(outputDirectory, schemaFile));
        }

        /// <summary>
        /// Registers a <see cref="DynamicCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile, char delimiter)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new DynamicCsvLogger(outputDirectory, schemaFile, delimiter));
        }

        /// <summary>
        /// Registers a <see cref="DynamicCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile, FileSize maxFileSize)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new DynamicCsvLogger(outputDirectory, schemaFile, maxFileSize));
        }

        /// <summary>
        /// Registers a <see cref="DynamicCsvLogger"/> for the specified identifier.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <param name="outputDirectory">The directory where log files will be stored.</param>
        /// <param name="delimiter">The delimiter to use in the CSV file.</param>
        /// <param name="maxFileSize">The maximum file size per log file.</param>
        /// <exception cref="DirectoryAlreadyInUseException">Thrown when a logger with the specified directory is already being used by a logger.</exception>
        /// <exception cref="IdentifierNotUniqueException">Thrown when a logger with the specified identifier has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static void RegisterDynamic(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile, char delimiter, FileSize maxFileSize)
        {
            ThrowIfNotValidRegistration(identifier, outputDirectory, schemaFile);

            _dynamicLoggers.Add(identifier, new DynamicCsvLogger(outputDirectory, schemaFile, maxFileSize, delimiter));
        }

        private static void ThrowIfNotValidRegistration(string identifier, DirectoryInfo outputDirectory, FileInfo schemaFile)
        {
            ValidateIdentifier(identifier);

            if (outputDirectory is null) throw new ArgumentNullException(nameof(outputDirectory));

            if (schemaFile is null) throw new ArgumentNullException(nameof(schemaFile));

            if (IsDirectoryInUse(outputDirectory)) throw new DirectoryAlreadyInUseException($"The directory with path ({outputDirectory.FullName}) is already being used by a different logger.");

            if (_dynamicLoggers.TryGetValue(identifier, out var logger)) throw new IdentifierNotUniqueException($"A logger with identifier ({identifier}) has already been registered.");
        }

        /// <summary>
        /// Retrieves the registered <see cref="DynamicCsvLogger"/> for the specified schema type.
        /// </summary>
        /// <param name="identifier">Unique identifier for specific logger, case insensitive.</param>
        /// <returns>The <see cref="DynamicCsvLogger"/> instance associated with the specified identifier.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the logger for the specified identifier is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is null or empty.</exception>
        public static DynamicCsvLogger GetDynamicLogger(string identifier)
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