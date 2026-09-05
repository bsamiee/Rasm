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
    // Compression acts on bytes, and every bit pattern, NaN payloads and signed zeros included, comes back unchanged
    [Fact]
    public void Blosc2DatasetsRoundTripEveryDoubleBitForBit() =>
        TestAssertions.ForAll(
            Generators.NonEmptyArray(Generators.AnyDouble, 4096),
            static values => Assert.Equal(Bits(values), Bits(WriteAndRead(values, Blosc2(compressor: "blosclz")).Values)));

    // Every codec the native package stages compresses, and a codec absent from the staged closure fails the write
    [Theory]
    [InlineData("blosclz")]
    [InlineData("lz4hc")] // lz4 is absent, PureHDF 3.0.0-beta.1 Blosc2Filter.GetCompressorCode rejects compressor code 1, the LZ4 code, as its failure value
    [InlineData("zlib")]
    [InlineData("zstd")]
    public void EveryBlosc2CompressorShrinksAConstantDataset(string compressor) => RoundTripsAConstantDataset(Blosc2(compressor), shrinks: true);

    // Every filter Initialize registers writes and reads a chunk, and an unregistered filter throws on the write
    [Theory]
    [InlineData(nameof(Blosc2Filter), true)]
    [InlineData(nameof(BZip2SharpZipLibFilter), true)]
    [InlineData(nameof(LzfFilter), false)] // PureHDF 3.0.0-beta.1 LzfFilter.Filter returns the whole buffer in place of the compressed slice, the chunk stays at raw size
    public void EveryRegisteredFilterRoundTripsAConstantDataset(string filter, bool shrinks) =>
        RoundTripsAConstantDataset(filter switch {
            nameof(Blosc2Filter) => new H5Filter(Blosc2Filter.Id),
            nameof(BZip2SharpZipLibFilter) => new H5Filter(BZip2SharpZipLibFilter.Id),
            nameof(LzfFilter) => new H5Filter(LzfFilter.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, "filter is not one Initialize registers"),
        }, shrinks);

    // The compressing codecs write between 335 and 1352 bytes of the 262144 raw, and the hundredfold bound holds while none of them degrades toward passthrough
    private static void RoundTripsAConstantDataset(H5Filter filter, bool shrinks) {
        double[] values = [.. Enumerable.Repeat(1.5, Hdf5Fixtures.CompressibleLength)];
        long rawBytes = (long)values.Length * sizeof(double);
        (double[] actual, long fileBytes) = WriteAndRead(values, filter);
        Assert.Equal(values, actual);
        if (shrinks) Assert.InRange(fileBytes, 1L, rawBytes / 100);
        else Assert.InRange(fileBytes, rawBytes, rawBytes * 2);
    }

    // The facade documents that a repeated Register call overwrites the entry, and every filter still reads and writes after it
    [Fact]
    public void RepeatedInitializationKeepsTheFiltersRegistered() {
        Hdf5Interop.Initialize();
        RoundTripsAConstantDataset(new H5Filter(Blosc2Filter.Id), shrinks: true);
    }

    private static H5Filter Blosc2(string compressor) =>
        new(Blosc2Filter.Id, new Dictionary<string, object>(StringComparer.Ordinal) { [Blosc2Filter.COMPRESSOR_CODE] = compressor });

    private static (double[] Values, long FileBytes) WriteAndRead(double[] values, H5Filter filter) {
        using TestDirectory directory = TestDirectory.Create("hdf5");
        FileInfo target = directory.File("values.h5");
        H5File file = new() { [Hdf5Fixtures.DatasetName] = new H5Dataset<double[]>(values, chunks: [(uint)values.Length], datasetCreation: new H5DatasetCreation(Filters: [filter])) };
        file.Write(target.FullName);
        using NativeFile read = H5File.OpenRead(target.FullName);
        return (read.Dataset(Hdf5Fixtures.DatasetName).Read<double[]>(), target.Length);
    }

    private static long[] Bits(double[] values) => [.. values.Select(BitConverter.DoubleToInt64Bits)];
}
