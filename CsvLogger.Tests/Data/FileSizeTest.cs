using CsvLogger.Data;

namespace CsvLogger.Tests.Data;

[TestClass]
public class FileSizeTest
{
    [TestMethod]
    public void Constructor_ShouldInitializeBytes()
    {
        long expectedBytes = 1024;

        var fileSize = new FileSize(expectedBytes);

        Assert.AreEqual(expectedBytes, fileSize.Bytes);
    }

    [TestMethod]
    public void KiloBytes_ShouldReturnCorrectValue()
    {
        var fileSize = new FileSize(2048);

        var kiloBytes = fileSize.KiloBytes;

        Assert.AreEqual(2, kiloBytes);
    }

    [TestMethod]
    public void Megabytes_ShouldReturnCorrectValue()
    {
        var fileSize = new FileSize(2 * 1024 * 1024);

        var megabytes = fileSize.Megabytes;

        Assert.AreEqual(2.0, megabytes);
    }

    [TestMethod]
    public void Gigabytes_ShouldReturnCorrectValue()
    {
        long t = 2;

        var fileSize = new FileSize(t * 1024 * 1024 * 1024);

        var gigabytes = fileSize.Gigabytes;

        Assert.AreEqual(2.0, gigabytes);
    }

    [TestMethod]
    public void FromBytes_ShouldCreateFileSize()
    {
        long bytes = 2048;

        var fileSize = FileSize.FromBytes(bytes);

        Assert.AreEqual(bytes, fileSize.Bytes);
    }

    [TestMethod]
    public void FromKB_ShouldCreateFileSize()
    {
        double kilobytes = 2.0;

        var fileSize = FileSize.FromKb(kilobytes);

        Assert.AreEqual((long)(kilobytes * 1024), fileSize.Bytes);
    }

    [TestMethod]
    public void FromMB_ShouldCreateFileSize()
    {
        double megabytes = 2.0;

        var fileSize = FileSize.FromMb(megabytes);

        Assert.AreEqual((long)(megabytes * 1024 * 1024), fileSize.Bytes);
    }

    [TestMethod]
    public void FromGB_ShouldCreateFileSize()
    {
        double gigabytes = 2.0;

        var fileSize = FileSize.FromGb(gigabytes);

        Assert.AreEqual((long)(gigabytes * 1024 * 1024 * 1024), fileSize.Bytes);
    }

    [TestMethod]
    public void EqualityOperators_ShouldWorkCorrectly()
    {
        var size1 = new FileSize(1024);
        var size2 = new FileSize(1024);
        var size3 = new FileSize(2048);

        Assert.IsTrue(size1 == size2);
        Assert.IsTrue(size1 != size3);
        Assert.IsTrue(size1 < size3);
        Assert.IsTrue(size1 <= size3);
        Assert.IsTrue(size3 > size1);
        Assert.IsTrue(size3 >= size1);
    }

    [TestMethod]
    public void Equals_ShouldReturnTrueForEqualFileSizes()
    {
        var size1 = new FileSize(1024);
        var size2 = new FileSize(1024);

        Assert.IsTrue(size1.Equals(size2));
    }

    [TestMethod]
    public void Equals_ShouldReturnFalseForDifferentFileSizes()
    {
        var size1 = new FileSize(1024);
        var size2 = new FileSize(2048);

        Assert.IsFalse(size1.Equals(size2));
    }

    [TestMethod]
    public void GetHashCode_ShouldReturnSameHashCodeForEqualFileSizes()
    {
        var size1 = new FileSize(1024);
        var size2 = new FileSize(1024);

        Assert.AreEqual(size1.GetHashCode(), size2.GetHashCode());
    }
}