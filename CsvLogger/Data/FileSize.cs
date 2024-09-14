namespace CsvLogger.Data
{
    public readonly struct FileSize
    {
        /// <summary>
        /// Gets the size in bytes.
        /// </summary>
        public long Bytes { get; }

        /// <summary>
        /// Gets the size in kilobytes.
        /// </summary>
        public long KiloBytes => Bytes / 1024;

        /// <summary>
        /// Gets the size in megabytes.
        /// </summary>
        public double Megabytes => Bytes / (1024.0 * 1024.0);

        /// <summary>
        /// Gets the size in gigabytes.
        /// </summary>
        public double Gigabytes => Bytes / (1024.0 * 1024.0 * 1024.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSize"/> struct with the specified size in bytes.
        /// </summary>
        /// <param name="bytes">The size of the file in bytes.</param>
        public FileSize(long bytes)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileSize"/> from the specified number of bytes.
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A <see cref="FileSize"/> object representing the specified size in bytes.</returns>
        public static FileSize FromBytes(long bytes)
        {
            return new FileSize(bytes);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileSize"/> from the specified number of kilobytes.
        /// </summary>
        /// <param name="kilobytes">The file size in kilobytes.</param>
        /// <returns>A <see cref="FileSize"/> object representing the specified size in kilobytes.</returns>
        public static FileSize FromKb(double kilobytes)
        {
            return new FileSize((long)(kilobytes * 1024));
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileSize"/> from the specified number of megabytes.
        /// </summary>
        /// <param name="megabytes">The file size in megabytes.</param>
        /// <returns>A <see cref="FileSize"/> object representing the specified size in megabytes.</returns>
        public static FileSize FromMb(double megabytes)
        {
            return new FileSize((long)(megabytes * 1024 * 1024));
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileSize"/> from the specified number of gigabytes.
        /// </summary>
        /// <param name="gigabytes">The file size in gigabytes.</param>
        /// <returns>A <see cref="FileSize"/> object representing the specified size in gigabytes.</returns>
        public static FileSize FromGb(double gigabytes)
        {
            return new FileSize((long)(gigabytes * 1024 * 1024 * 1024));
        }

        // Comparison operators
        public static bool operator >(FileSize left, FileSize right) => left.Bytes > right.Bytes;

        public static bool operator <(FileSize left, FileSize right) => left.Bytes < right.Bytes;

        public static bool operator >=(FileSize left, FileSize right) => left.Bytes >= right.Bytes;

        public static bool operator <=(FileSize left, FileSize right) => left.Bytes <= right.Bytes;

        public static bool operator ==(FileSize left, FileSize right) => left.Bytes == right.Bytes;

        public static bool operator !=(FileSize left, FileSize right) => left.Bytes != right.Bytes;

        /// <summary>
        /// Determines whether this instance and a specified object, which must also be a FileSize object, have the same value.
        /// </summary>
        /// <param name="obj">The FileSize to compare to this instance.</param>
        /// <returns>true if obj is a FileSize and its value is the same as this instance; otherwise, false. If obj is null, the method returns false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is FileSize other)
            {
                return Bytes == other.Bytes;
            }
            return false;
        }

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        /// <summary>
        /// Converts the file size to a string representation in bytes, megabytes, and gigabytes.
        /// </summary>
        /// <returns>A string representation of the file size.</returns>
        public override string ToString()
        {
            return $"{Bytes} bytes ({Megabytes:F2} MB, {Gigabytes:F2} GB)";
        }
    }
}