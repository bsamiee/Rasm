using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LookupLayout {
    public static readonly LookupLayout Hald = new(
        static (width, height) => (int)float.Round(float.Cbrt(width), MidpointRounding.ToEven) switch {
            var level when width == height && level * level * level == width => Some(level * level),
            _ => None,
        },
        static edge => (int)float.Round(float.Sqrt(edge), MidpointRounding.ToEven) switch {
            var level when level * level == edge => Extended(level * level * level, level * level * level),
            _ => None,
        },
        static (row, run, runs, edge) => ((row * runs) + run) * edge);
    public static readonly LookupLayout KeyShot = new(
        static (width, height) => width == height * height ? Some(height) : None,
        static edge => Extended(edge * edge, edge),
        static (row, run, _, edge) => (edge * (edge - 1 - row)) + (edge * edge * run));

    private static readonly DecoderOptions Options = new() { Configuration = new Configuration(new PngConfigurationModule()), SkipMetadata = true };

    [UseDelegateFromConstructor]
    public partial Option<int> Edge(int width, int height);

    [UseDelegateFromConstructor]
    public partial Option<PixelExtent> Extent(int edge);

    [UseDelegateFromConstructor]
    public partial int Start(int row, int run, int runs, int edge);

    public static Fin<LutTable> Decode(ReadOnlyMemory<byte> bytes) =>
        Try.lift(() => Image.Load<Rgb48>(Options, bytes.Span)).Run()
            .MapFail(static Error (cause) => new ImagingFault("lookup image", ImagingReason.Refused, cause))
            .Bind(Lattice);

    public Fin<Func<Stream, IO<Unit>>> Encode(LutTable table, FileMetadata metadata) =>
        (from cube in LutText.Only<LutTable.Cube>(LutText.Flat(table))
         where LutText.Coded(cube.Lattice)
         from extent in Extent(cube.Lattice.Size)
         select Frame(cube, extent))
            .ToFin(new ImagingFault("lookup image table beside a single 3D unit lattice with samples inside [0, 1]", ImagingReason.Unsupported))
            .Bind(static frame => StillFormat.PngRaster.From(frame, OutputDepth.UInt16, None, alpha: false))
            .Map<Func<Stream, IO<Unit>>>(still => stream => StillWriter.Encode(still, metadata, stream));

    private static Option<PixelExtent> Extended(int width, int height) => PixelExtent.TryCreate(width, height, out PixelExtent? extent) ? Optional(extent) : None;

    private static Fin<LutTable> Lattice(Image<Rgb48> image) {
        using Image<Rgb48> owned = image;
        return toSeq(Items).Choose(layout => layout.Edge(owned.Width, owned.Height).Map(edge => (Layout: layout, Edge: edge))).Head
            .ToFin(new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"lookup image of {owned.Width}x{owned.Height}"), ImagingReason.Unnamed))
            .Bind(found => {
                (LookupLayout layout, int edge) = found;
                using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * edge * edge * edge);
                owned.ProcessPixelRows(rows => {
                    Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.Span);
                    int runs = rows.Width / edge;
                    for (int row = 0; row < rows.Height; row++) {
                        Span2D<Rgb48> tiles = rows.GetRowSpan(row).AsSpan2D(runs, edge);
                        for (int run = 0; run < runs; run++) {
                            Span<Rgb48> pixels = tiles.GetRowSpan(run);
                            Span<Vector3> entries = nodes.Slice(layout.Start(row, run, runs, edge), edge);
                            for (int red = 0; red < edge; red++)
                                entries[red] = pixels[red].ToScaledVector4().AsVector3();
                        }
                    }
                });
                return LutText.UnitLattice(edge, samples.Memory);
            });
    }

    private PixelFrame Frame(LutTable.Cube cube, PixelExtent extent) {
        (PixelFrame frame, int edge, int runs) = (new(default, extent, extent), cube.Lattice.Size, extent.Width / cube.Lattice.Size);
        Span2D<Vector4> image = frame.View.Span;
        ReadOnlySpan<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(cube.Lattice.Data);
        for (int row = 0; row < image.Height; row++) {
            Span2D<Vector4> tiles = image.GetRowSpan(row).AsSpan2D(runs, edge);
            for (int run = 0; run < runs; run++) {
                Span<Vector4> pixels = tiles.GetRowSpan(run);
                ReadOnlySpan<Vector3> entries = nodes.Slice(Start(row, run, runs, edge), edge);
                for (int red = 0; red < edge; red++)
                    pixels[red] = new Vector4(entries[red], 1f);
            }
        }
        return frame;
    }
}

[SmartEnum<string>]
public sealed partial class LutFormat {
    public static readonly LutFormat Cube = new("cube", ".cube",
        static bytes => LutText.ParseCube(LutText.Text(bytes)),
        static (table, _, _) => LutText.Written(LutText.RenderCube(table)));
    public static readonly LutFormat Spi1d = new("spi1d", ".spi1d",
        static bytes => LutText.ParseSpi1d(LutText.Text(bytes)),
        static (table, _, _) => LutText.Written(LutText.RenderSpi1d(table)));
    public static readonly LutFormat Spi3d = new("spi3d", ".spi3d",
        static bytes => LutText.ParseSpi3d(LutText.Text(bytes)),
        static (table, _, _) => LutText.Written(LutText.RenderSpi3d(table)));
    public static readonly LutFormat SpiMatrix = new("spimtx", ".spimtx",
        static bytes => LutText.ParseSpiMatrix(LutText.Text(bytes)),
        static (table, _, _) => LutText.Written(LutText.RenderSpiMatrix(table)));
    public static readonly LutFormat Autodesk = new("3dl", ".3dl",
        static bytes => LutText.Parse3dl(LutText.Text(bytes)),
        static (table, _, _) => LutText.Written(LutText.Render3dl(table)));
    public static readonly LutFormat Clf = new("clf", ".clf",
        static bytes => ProcessLists.ParseClf(LutText.Text(bytes)),
        static (table, name, _) => LutText.Written(ProcessLists.RenderClf(table, name)));
    public static readonly LutFormat Ctf = new("ctf", ".ctf",
        static bytes => ProcessLists.ParseCtf(LutText.Text(bytes)),
        static (table, name, _) => LutText.Written(ProcessLists.RenderCtf(table, name)));
    public static readonly LutFormat Hald = new("hald", ".png",
        LookupLayout.Decode,
        static (table, _, metadata) => LookupLayout.Hald.Encode(table, metadata));
    public static readonly LutFormat KeyShotLookup = new("keyshot-lookup", ".png",
        LookupLayout.Decode,
        static (table, _, metadata) => LookupLayout.KeyShot.Encode(table, metadata));

    public string Extension { get; }

    [UseDelegateFromConstructor]
    public partial Fin<LutTable> Decode(ReadOnlyMemory<byte> bytes);

    [UseDelegateFromConstructor]
    public partial Fin<Func<Stream, IO<Unit>>> Encode(LutTable table, string name, FileMetadata metadata);
}

public sealed record LutFile {
    private static readonly FrozenDictionary<string, LutFormat> Formats =
        LutFormat.Items.DistinctBy(static format => format.Extension, StringComparer.OrdinalIgnoreCase).ToFrozenDictionary(static format => format.Extension, StringComparer.OrdinalIgnoreCase);

    private LutFile(string path, LutTable table) => (Path, Table) = (path, table);

    public string Path { get; }
    public LutTable Table { get; }

    public static IO<LutFile> Read(string path) =>
        (from format in IO.lift(Named(path))
         from token in cancelToken
         from bytes in IO.liftAsync(() => File.ReadAllBytesAsync(path, token))
         from table in IO.lift(format.Decode(bytes))
         select new LutFile(path, table))
            .Catch(
                static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(),
                error => IO.fail<LutFile>(new ImagingFault($"LUT file {path}", ImagingReason.Unreadable, error)));

    private static Fin<LutFormat> Named(string path) =>
        Formats.TryGetValue(new FileInfo(path).Extension, out LutFormat? format)
            ? format
            : new ImagingFault($"LUT path {path} beside {string.Join(", ", Formats.Keys)}", ImagingReason.Unnamed);
}
