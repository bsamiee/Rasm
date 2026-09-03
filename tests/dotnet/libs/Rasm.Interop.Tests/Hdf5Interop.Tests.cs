using System.Runtime.CompilerServices;
using PureHDF;
using PureHDF.Filters;
using PureHDF.VOL.Native;
using Rasm.Interop.Hdf5;
using Rasm.TestSupport;

namespace Rasm.Interop.Tests;

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class HostInitialization {
    [ModuleInitializer]
    internal static void Initialize() => Hdf5Interop.Initialize();
}

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class Hdf5Fixtures {
    public const string DatasetName = "values";
    public const int CompressibleLength = 1 << 15;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Hdf5InteropTests {
    [Fact]
    public void Blosc2DatasetsRoundTripEveryFiniteArray() =>
        TestAssertions.ForAll(
            Generators.NonEmptyArray(Generators.Finite, 4096),
            static values => Assert.Equal(values, WriteAndRead(values, "blosclz").Values));

    [Theory]
    [InlineData("blosclz")]
    [InlineData("zlib")]
    [InlineData("zstd")]
    public void EveryCompressorShrinksAConstantDataset(string compressor) {
        double[] values = [.. Enumerable.Repeat(1.5, Hdf5Fixtures.CompressibleLength)];
        long rawBytes = (long)values.Length * sizeof(double);
        (double[] actual, long fileBytes) = WriteAndRead(values, compressor);
        Assert.Equal(values, actual);
        Assert.InRange(fileBytes, 1, rawBytes / 8);
    }

    private static (double[] Values, long FileBytes) WriteAndRead(double[] values, string compressor) {
        using TestDirectory directory = TestDirectory.Create("hdf5");
        FileInfo target = directory.File("values.h5");
        H5DatasetCreation creation = new(Filters: [new H5Filter(Blosc2Filter.Id, new Dictionary<string, object>(StringComparer.Ordinal) { [Blosc2Filter.COMPRESSOR_CODE] = compressor })]);
        H5File file = new() { [Hdf5Fixtures.DatasetName] = new H5Dataset<double[]>(values, chunks: [checked((uint)values.Length)], datasetCreation: creation) };
        file.Write(target.FullName);
        using NativeFile read = H5File.OpenRead(target.FullName);
        return (read.Dataset(Hdf5Fixtures.DatasetName).Read<double[]>(), target.Length);
    }
}
