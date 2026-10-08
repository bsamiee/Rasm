using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.ColorManagement;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidColor>]
internal sealed partial class ClfBitDepth {
    public static readonly ClfBitDepth Integer8 = new("8i", OutputDepth.UInt8);
    public static readonly ClfBitDepth Integer10 = new("10i", OutputDepth.UInt10);
    public static readonly ClfBitDepth Integer12 = new("12i", OutputDepth.UInt12);
    public static readonly ClfBitDepth Integer16 = new("16i", OutputDepth.UInt16);
    public static readonly ClfBitDepth Float16 = new("16f", OutputDepth.Float16);
    public static readonly ClfBitDepth Float32 = new("32f", OutputDepth.Float32);

    public OutputDepth Depth { get; }

    public float Scale => Depth.Levels.Map(static levels => levels.MaxCode).IfNone(1f);
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class ClfInterpolation {
    public static readonly ClfInterpolation Trilinear = new("trilinear", LutInterpolation.Trilinear);
    public static readonly ClfInterpolation Tetrahedral = new("tetrahedral", LutInterpolation.Tetrahedral);

    public LutInterpolation Interpolation { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class ClfLogStyle {
    public static readonly ClfLogStyle Log10 = new("log10", Some(10f), camera: false, static curve => new LutTable.Log(curve));
    public static readonly ClfLogStyle AntiLog10 = new("antiLog10", Some(10f), camera: false, static curve => new LutTable.Antilog(curve));
    public static readonly ClfLogStyle Log2 = new("log2", Some(2f), camera: false, static curve => new LutTable.Log(curve));
    public static readonly ClfLogStyle AntiLog2 = new("antiLog2", Some(2f), camera: false, static curve => new LutTable.Antilog(curve));
    public static readonly ClfLogStyle LinToLog = new("linToLog", None, camera: false, static curve => new LutTable.Log(curve));
    public static readonly ClfLogStyle LogToLin = new("logToLin", None, camera: false, static curve => new LutTable.Antilog(curve));
    public static readonly ClfLogStyle CameraLinToLog = new("cameraLinToLog", None, camera: true, static curve => new LutTable.Log(curve));
    public static readonly ClfLogStyle CameraLogToLin = new("cameraLogToLin", None, camera: true, static curve => new LutTable.Antilog(curve));

    public Option<float> Base { get; }
    public bool Camera { get; }

    [UseDelegateFromConstructor]
    public partial LutTable Table(LogCurve curve);
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class ProcessNode {
    public static readonly ProcessNode Matrix = new("Matrix", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadMatrix));
    public static readonly ProcessNode Lut1D = new("LUT1D", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadLut1D));
    public static readonly ProcessNode Lut3D = new("LUT3D", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadLut3D));
    public static readonly ProcessNode Range = new("Range", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadRange));
    public static readonly ProcessNode Log = new("Log", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadLog));
    public static readonly ProcessNode Exponent = new("Exponent", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadExponent));
    public static readonly ProcessNode Gamma = new("Gamma", [LutFormat.Ctf], Some(LutFiles.ReadGamma));
    public static readonly ProcessNode AscCdl = new("ASC_CDL", [LutFormat.Clf, LutFormat.Ctf], Some(LutFiles.ReadCdl));
    public static readonly ProcessNode InverseLut1D = new("InverseLUT1D", [LutFormat.Ctf], Some(LutFiles.ReadInverseLut1D));
    public static readonly ProcessNode Description = new("Description", [LutFormat.Clf, LutFormat.Ctf], None);
    public static readonly ProcessNode InputDescriptor = new("InputDescriptor", [LutFormat.Clf, LutFormat.Ctf], None);
    public static readonly ProcessNode OutputDescriptor = new("OutputDescriptor", [LutFormat.Clf, LutFormat.Ctf], None);
    public static readonly ProcessNode Info = new("Info", [LutFormat.Clf, LutFormat.Ctf], None);

    public Seq<LutFormat> Formats { get; }
    public Option<Func<XElement, LutFormat, Fin<LutTable>>> Reader { get; }
}

[SmartEnum]
public sealed partial class LookupLayout {
    public static readonly LookupLayout Hald = new(
        LutFormat.Hald,
        static (width, height) => (int)float.Round(float.Cbrt(width), MidpointRounding.ToEven) switch {
            var level when width == height && level * level * level == width && LatticeSize.Validate(level * level, provider: null, out LatticeSize edge) is null => Some(edge),
            _ => None,
        },
        static edge => (int)float.Round(float.Sqrt(edge), MidpointRounding.ToEven) switch {
            var level when level * level == edge && PixelExtent.Validate(level * level * level, level * level * level, out PixelExtent image) is null => Some(image),
            _ => None,
        },
        static (x, y, image, _) => (y * image.Width) + x);
    public static readonly LookupLayout KeyShot = new(
        LutFormat.KeyShotLookup,
        static (width, height) => width == height * height && LatticeSize.Validate(height, provider: null, out LatticeSize edge) is null ? Some(edge) : None,
        static edge => PixelExtent.Validate(edge * edge, edge, out PixelExtent image) is null ? Some(image) : None,
        static (x, y, _, edge) => (x % edge) + (edge * (edge - 1 - y)) + (edge * edge * (x / edge)));

    public LutFormat Format { get; }

    [UseDelegateFromConstructor]
    public partial Option<LatticeSize> Edge(int width, int height);

    [UseDelegateFromConstructor]
    public partial Option<PixelExtent> Image(int edge);

    [UseDelegateFromConstructor]
    public partial int Node(int x, int y, PixelExtent image, int edge);
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LutFormat {
    public static readonly LutFormat Cube = new("cube", ".cube");
    public static readonly LutFormat Spi1d = new("spi1d", ".spi1d");
    public static readonly LutFormat Spi3d = new("spi3d", ".spi3d");
    public static readonly LutFormat SpiMatrix = new("spimtx", ".spimtx");
    public static readonly LutFormat Autodesk = new("3dl", ".3dl");
    public static readonly LutFormat Clf = new("clf", ".clf");
    public static readonly LutFormat Ctf = new("ctf", ".ctf");
    public static readonly LutFormat Hald = new("hald", ".png");
    public static readonly LutFormat KeyShotLookup = new("keyshot-lookup", ".png");

    public string Extension { get; }
}

[ValueObject<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LutPath {
    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref string value) =>
        validationError = Path.IsPathFullyQualified(value) && LutFiles.Readers.ContainsKey(Path.GetExtension(value)) ? null : new InvalidColor();
}

public sealed record LutFile {
    private LutFile(LutPath path, LutTable table) => (Path, Table) = (path, table);

    public LutPath Path { get; }
    public LutTable Table { get; }

    public static IO<LutFile> Read(LutPath path) =>
        LutFiles.Readers[System.IO.Path.GetExtension(path)](path)
            .Map(table => new LutFile(path, table))
            .Catch(
                static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() || error.HasException<ImageFormatException>(),
                error => IO.fail<LutFile>(new LutUnreadable(path, error)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LutFiles {
    // --- [TOKENS]
    private const string Blanks = " \t\r\n";
    private const string Title = "TITLE";
    private const string Lut1DSize = "LUT_1D_SIZE";
    private const string Lut3DSize = "LUT_3D_SIZE";
    private const string DomainMin = "DOMAIN_MIN";
    private const string DomainMax = "DOMAIN_MAX";
    private const string Lut1DInputRange = "LUT_1D_INPUT_RANGE";
    private const string Lut3DInputRange = "LUT_3D_INPUT_RANGE";
    private const string Version = "Version";
    private const string From = "From";
    private const string Length = "Length";
    private const string Components = "Components";
    private const string SpiLut = "SPILUT";
    private const string ProcessList = "ProcessList";
    private const string ArrayElement = "Array";
    private const string Dimensions = "dim";
    private const string InBitDepth = "inBitDepth";
    private const string OutBitDepth = "outBitDepth";
    private const string Style = "style";
    private const string Interpolation = "interpolation";
    private const string MinIn = "minInValue";
    private const string MinOut = "minOutValue";
    private const string MaxIn = "maxInValue";
    private const string MaxOut = "maxOutValue";
    private const string Clamp = "Clamp";
    private const string NoClamp = "noClamp";
    private const string LogParams = "LogParams";
    private const string LogBase = "base";
    private const string LogSideSlope = "logSideSlope";
    private const string LogSideOffset = "logSideOffset";
    private const string LinSideSlope = "linSideSlope";
    private const string LinSideOffset = "linSideOffset";
    private const string LinSideBreak = "linSideBreak";
    private const string LinearSlope = "linearSlope";
    private const string Channel = "channel";
    private const string Lanes = "RGB";
    private const string ExponentParams = "ExponentParams";
    private const string ExponentValue = "exponent";
    private const string OffsetValue = "offset";
    private const string SopNode = "SOPNode";
    private const string SatNode = "SatNode";
    private const string Slope = "Slope";
    private const string Offset = "Offset";
    private const string Power = "Power";
    private const string Saturation = "Saturation";

    // --- [READS]
    private static readonly DecoderOptions LookupOptions = new() {
        Configuration = new Configuration(new PngConfigurationModule(), new TiffConfigurationModule()),
        SkipMetadata = true,
    };

    internal static FrozenDictionary<string, Func<LutPath, IO<LutTable>>> Readers { get; } =
        toSeq(LookupOptions.Configuration.ImageFormats).Bind(static format => toSeq(format.FileExtensions)).Map(static extension => $".{extension}") switch {
            var lookups => lookups.Map(static extension => (Extension: extension, Read: (Func<LutPath, IO<LutTable>>)ReadLookup))
                .Concat(toSeq(LutFormat.Items)
                    .Filter(format => !lookups.Exists(extension => string.Equals(extension, format.Extension, StringComparison.OrdinalIgnoreCase)))
                    .Map(static format => (format.Extension, Read: (Func<LutPath, IO<LutTable>>)(path =>
                        from token in cancelToken
                        from text in IO.liftAsync(() => File.ReadAllTextAsync(path, cancellationToken: token))
                        from table in IO.lift(Parse(text, format))
                        select table))))
                .ToFrozenDictionary(static reader => reader.Extension, static reader => reader.Read, StringComparer.OrdinalIgnoreCase),
        };

    private static IO<LutTable> ReadLookup(LutPath path) =>
        from token in cancelToken
        from table in use(IO.liftAsync(() => SixLabors.ImageSharp.Image.LoadAsync<Rgb48>(LookupOptions, path, token))).Bind(static image => IO.lift(Lattice(image))).Bracket()
        select table;

    private static Fin<LutTable> Lattice(Image<Rgb48> image) =>
        toSeq(LookupLayout.Items)
            .Choose(layout => from edge in layout.Edge(image.Width, image.Height) from extent in layout.Image(edge) select (Layout: layout, Edge: edge, Extent: extent))
            .Head.ToFin(new LookupShapeRefused(image.Width, image.Height))
            .Bind(found => {
                using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * found.Edge * found.Edge * found.Edge);
                image.ProcessPixelRows(rows => {
                    Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.Span);
                    for (int pixel = 0; pixel < rows.Width * rows.Height; pixel++)
                        nodes[found.Layout.Node(pixel % rows.Width, pixel / rows.Width, found.Extent, found.Edge)] = rows.GetRowSpan(pixel / rows.Width)[pixel % rows.Width].ToScaledVector4().AsVector3();
                });
                return LatticeTable(found.Edge, (Vector3.Zero, Vector3.One), LutInterpolation.Tetrahedral).Build(samples.Memory);
            });

    // --- [GRAMMARS]
    public static Fin<LutTable> Parse(string text, LutFormat format) =>
        format.Switch(
            text,
            cube: ParseCube,
            spi1d: ParseSpi1d,
            spi3d: ParseSpi3d,
            spiMatrix: ParseSpiMatrix,
            autodesk: Parse3dl,
            clf: static text => ParseProcessList(LutFormat.Clf, text),
            ctf: static text => ParseProcessList(LutFormat.Ctf, text),
            hald: static _ => new LutFormatRefused(LutFormat.Hald),
            keyShotLookup: static _ => new LutFormatRefused(LutFormat.KeyShotLookup));

    private static Fin<LutTable> ParseCube(string text) =>
        from header in Header.Read(text, LutFormat.Cube, [Title, Lut1DSize, Lut3DSize, DomainMin, DomainMax, Lut1DInputRange, Lut3DInputRange], static line => char.IsAsciiLetter(line[0]))
        from stated in (
                header.Stated(Lut1DSize, static value => Invariant.Number<int>(value).Bind(Sized<CurveLength>)),
                header.Stated(Lut3DSize, static value => Invariant.Number<int>(value).Bind(Sized<LatticeSize>)),
                header.Stated(DomainMin, Triple),
                header.Stated(DomainMax, Triple),
                header.Stated(Lut1DInputRange, Pair),
                header.Stated(Lut3DInputRange, Pair))
            .Apply(static (curve, lattice, low, high, curveRange, latticeRange) =>
                (Curve: curve, Lattice: lattice, Low: low, High: high, CurveRange: curveRange, LatticeRange: latticeRange))
            .As()
        from table in (
                guard<Error>(stated.Curve.IsSome || stated.Lattice.IsSome, new LutSizeAbsent(LutFormat.Cube)).ToFin(),
                ((stated.Low | stated.High).Filter(_ => (stated.Curve.IsSome && stated.Lattice.IsSome) || stated.CurveRange.IsSome || stated.LatticeRange.IsSome).Map(static domain => domain.Line)
                    | stated.CurveRange.Filter(_ => stated.Curve.IsNone).Map(static range => range.Line)
                    | stated.LatticeRange.Filter(_ => stated.Lattice.IsNone).Map(static range => range.Line))
                .Traverse(static line => Fin.Fail<Unit>(new LutLineMalformed(LutFormat.Cube, line))).As(),
                Tabled(
                    LutFormat.Cube, text, header.End, braced: false, width: 3,
                    stated.Curve.Map(length => CurveTable(length.Value, Domain(stated.CurveRange, stated.Low, stated.High))).ToSeq()
                    + stated.Lattice.Map(size => LatticeTable(size.Value, Domain(stated.LatticeRange, stated.Low, stated.High), LutInterpolation.Tetrahedral)).ToSeq()))
            .Apply(static (_, _, table) => table)
            .As()
        select table;

    private static Fin<LutTable> ParseSpi1d(string text) =>
        from header in Header.Read(text, LutFormat.Spi1d, [Version, From, Length, Components], static line => line[0] != '{')
        from stated in (
                header.Stated(Version, static value => Invariant.Number<int>(value).Filter(static version => version == 1)),
                header.Stated(From, Pair),
                header.Stated(Length, static value => Invariant.Number<int>(value).Bind(Sized<CurveLength>)),
                header.Stated(Components, static value => Invariant.Number<int>(value).Filter(static count => count is 1 or 3)))
            .Apply(static (version, domain, length, components) => (Version: version, Domain: domain, Length: length, Components: components))
            .As()
        from shape in (
                stated.Length.ToFin(new LutSizeAbsent(LutFormat.Spi1d)),
                stated.Components.Filter(_ => stated.Version.IsSome).ToFin(new LutLineMalformed(LutFormat.Spi1d, header.End)))
            .Apply(static (size, width) => (Size: size.Value, Width: width.Value))
            .As()
        from table in Tabled(LutFormat.Spi1d, text, header.End + 1, braced: true, shape.Width, [CurveTable(shape.Size, Domain(stated.Domain, None, None))])
        select table;

    private static Fin<LutTable> ParseSpi3d(string text) {
        Span<System.Range> tokens = stackalloc System.Range[7];
        Span<int> codes = stackalloc int[3];
        SpanLineEnumerator lines = text.AsSpan().EnumerateLines();
        if (!lines.MoveNext() || !lines.Current.TrimStart().StartsWith(SpiLut, StringComparison.OrdinalIgnoreCase))
            return new LutLineMalformed(LutFormat.Spi3d, 1);
        if (!lines.MoveNext() || !lines.MoveNext())
            return new LutSizeAbsent(LutFormat.Spi3d);
        if (lines.Current.SplitAny(tokens, Blanks, StringSplitOptions.RemoveEmptyEntries) != 3 || Parsed(lines.Current, tokens, codes) != 3
            || codes.ContainsAnyExcept(codes[0]) || Sized<LatticeSize>(codes[0]).Case is not LatticeSize size)
            return new LutLineMalformed(LutFormat.Spi3d, 3);
        int nodes = size * size * size;
        using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * nodes);
        using SpanOwner<bool> filled = SpanOwner<bool>.Allocate(nodes, AllocationMode.Clear);
        (int number, int read) = (3, 0);
        while (lines.MoveNext()) {
            number++;
            if (lines.Current.SplitAny(tokens, Blanks, StringSplitOptions.RemoveEmptyEntries) != 6 || Parsed(lines.Current, tokens, codes) != 3)
                continue;
            int node = (((codes[2] * size) + codes[1]) * size) + codes[0];
            if (codes.ContainsAnyExceptInRange(0, size - 1) || filled.Span[node] || Parsed(lines.Current, tokens[3..6], samples.Span.Slice(3 * node, 3)) != 3)
                return new LutLineMalformed(LutFormat.Spi3d, number);
            (filled.Span[node], read) = (true, read + 1);
        }
        return read != nodes
            ? new LutCountMismatched(LutFormat.Spi3d, nodes, read)
            : LatticeTable(size, (Vector3.Zero, Vector3.One), LutInterpolation.Tetrahedral).Build(samples.Memory);
    }

    private static Fin<LutTable> ParseSpiMatrix(string text) {
        const int Cells = 12;
        (Seq<float> values, int number) = ([], 0);
        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines()) {
            number++;
            if (Numbers<float>(line).Case is not float[] row)
                return new LutLineMalformed(LutFormat.SpiMatrix, number);
            values += toSeq(row);
        }
        return values.Count != Cells
            ? new LutCountMismatched(LutFormat.SpiMatrix, Cells, values.Count)
            : Affine(values.AsSpan()[..Cells], 4, (ushort.MaxValue, ushort.MaxValue));
    }

    private static Fin<LutTable> Parse3dl(string text) {
        Span<System.Range> tokens = stackalloc System.Range[LatticeSize.MaxValue + 1];
        Span<int> codes = stackalloc int[LatticeSize.MaxValue + 1];
        SpanLineEnumerator lines = text.AsSpan().EnumerateLines();
        (int number, int count, int read) = (0, 0, 0);
        while (read == 0 && lines.MoveNext()) {
            number++;
            count = lines.Current.SplitAny(tokens, Blanks, StringSplitOptions.RemoveEmptyEntries);
            read = Parsed(lines.Current, tokens[..count], codes[..count]);
        }
        if (read == 0)
            return new LutSizeAbsent(LutFormat.Autodesk);
        if (read != count || count <= 3 || codes[..count] is not [.., var last] || last < 128 || !Ramped(codes[..count], last) || Sized<LatticeSize>(count).Case is not LatticeSize size)
            return new LutLineMalformed(LutFormat.Autodesk, number);
        int nodes = size * size * size;
        using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * nodes);
        Span<Vector3> lattice = MemoryMarshal.Cast<float, Vector3>(samples.Span);
        (int rows, int largest) = (0, 0);
        while (lines.MoveNext()) {
            number++;
            count = lines.Current.SplitAny(tokens[..4], Blanks, StringSplitOptions.RemoveEmptyEntries);
            read = Parsed(lines.Current, tokens[..count], codes[..count]);
            if (read == 0)
                continue;
            if (count != 3 || read != count || codes[..3].ContainsAnyInRange(int.MinValue, -1))
                return new LutLineMalformed(LutFormat.Autodesk, number);
            if (rows < nodes)
                lattice[Transposed(rows, size)] = new Vector3(codes[0], codes[1], codes[2]);
            (rows, largest) = (rows + 1, int.Max(largest, TensorPrimitives.Max(codes[..3])));
        }
        if (rows != nodes)
            return new LutCountMismatched(LutFormat.Autodesk, nodes, rows);
        TensorPrimitives.Divide(samples.Span, uint.Clamp(BitOperations.RoundUpToPowerOf2((uint)largest), 128u, 65_536u) - 1f, samples.Span);
        TensorPrimitives.Min(samples.Span, 1f, samples.Span);
        return LatticeTable(size, (Vector3.Zero, Vector3.One), LutInterpolation.Tetrahedral).Build(samples.Memory);

        static bool Ramped(ReadOnlySpan<int> mesh, int last) {
            for (int at = 0; at < mesh.Length; at++)
                if (float.Abs(mesh[at] - ((float)at * last / (mesh.Length - 1))) > 1f)
                    return false;
            return true;
        }
    }

    private static Fin<LutTable> ParseProcessList(LutFormat format, string text) =>
        Try.lift(() => XDocument.Parse(text, LoadOptions.SetLineInfo)).Run()
            .MapFail(error => error.Exception.Case is XmlException xml ? new LutLineMalformed(format, xml.LineNumber) : error)
            .Bind(document => document.Root is { } root && string.Equals(root.Name.LocalName, ProcessList, StringComparison.Ordinal)
                ? toSeq(root.Elements())
                    .Filter(static node => !Marked(node, "bypass", "true"))
                    .TraverseM(node => ProcessNode.TryGet(node.Name.LocalName, out ProcessNode? kind) && kind.Formats.Exists(held => held == format)
                        ? kind.Reader.Traverse(read => read(node, format)).As()
                        : Fin.Fail<Option<LutTable>>(Unheld(node, format)))
                    .As()
                    .Map(static steps => Chained(steps.Somes()))
                : new LutLineMalformed(format, document.Root is { } other ? Line(other) : 1));

    internal static Fin<LutTable> ReadMatrix(XElement node, LutFormat format) =>
        from read in (Scales(node, format), Values<float>(node, format)).Apply(static (scales, array) => (Scales: scales, Array: array)).As()
        from table in (read.Array.Dimensions, read.Array.Values.Length) switch {
            ([4, ..], _) => Fin.Fail<LutTable>(Unheld(node, format)),
            ([3, 3] or [3, 3, 3], 9) => Fin.Succ<LutTable>(Affine(read.Array.Values.AsSpan(0, 9), 3, read.Scales)),
            ([3, 4] or [3, 4, 3], 12) => Fin.Succ<LutTable>(Affine(read.Array.Values.AsSpan(0, 12), 4, read.Scales)),
            _ => Fin.Fail<LutTable>(Malformed(node, format)),
        }
        select table;

    internal static Fin<LutTable> ReadLut1D(XElement node, LutFormat format) =>
        from scales in Scales(node, format)
        from curve in Curve(node, format, scales.Out)
        from table in Indexed(node, format, scales.In, curve.Length, new LutTable.ChannelCurve(curve))
        select table;

    internal static Fin<LutTable> ReadInverseLut1D(XElement node, LutFormat format) =>
        from scales in Scales(node, format)
        from curve in Curve(node, format, scales.Out)
        from inverse in InverseLut1D.From(curve)
        select (LutTable)new LutTable.InverseChannelCurve(inverse);

    internal static Fin<LutTable> ReadLut3D(XElement node, LutFormat format) =>
        from read in (
                Scales(node, format),
                Values<float>(node, format),
                ClfInterpolation.Validate(Attribute(node, Interpolation).IfNone(ClfInterpolation.Trilinear.Key), provider: null, out ClfInterpolation? interpolation) is null
                    ? Fin.Succ(interpolation!.Interpolation)
                    : Fin.Fail<LutInterpolation>(Malformed(node, format)))
            .Apply(static (scales, array, interpolation) => (Scales: scales, Array: array, Interpolation: interpolation))
            .As()
        from size in (read.Array.Dimensions is [var red, var green, var blue, 3] && red == green && green == blue && read.Array.Values.Length == 3 * red * red * red
                ? Sized<LatticeSize>(red)
                : None).ToFin(Malformed(node, format))
        from cube in LatticeTable(size, (Vector3.Zero, Vector3.One), read.Interpolation).Build(Transposed(read.Array.Values, size, 1f / read.Scales.Out))
        from table in Indexed(node, format, read.Scales.In, size, cube)
        select table;

    internal static Fin<LutTable> ReadRange(XElement node, LutFormat format) =>
        from scales in Scales(node, format)
        from stated in (
                Bound(node, format, MinIn, MinOut, scales),
                Bound(node, format, MaxIn, MaxOut, scales),
                guard<Error>(Attribute(node, Style).IsNone || Marked(node, Style, Clamp) || Marked(node, Style, NoClamp), Malformed(node, format)).ToFin())
            .Apply(static (minimum, maximum, _) => (Minimum: minimum, Maximum: maximum))
            .As()
        let clamps = !Marked(node, Style, NoClamp)
        from bounds in (stated.Minimum.Case, stated.Maximum.Case) switch {
            (RemapBound low, RemapBound high) when low.In != high.In => Fin.Succ<RemapBounds>(new RemapBounds.Between(low, high, clamps)),
            (RemapBound low, null) when clamps => Fin.Succ<RemapBounds>(new RemapBounds.Floor(low)),
            (null, RemapBound high) when clamps => Fin.Succ<RemapBounds>(new RemapBounds.Ceiling(high)),
            _ => Fin.Fail<RemapBounds>(Malformed(node, format)),
        }
        select (LutTable)new LutTable.Remap(bounds);

    internal static Fin<LutTable> ReadLog(XElement node, LutFormat format) =>
        from style in Attribute(node, Style).Bind(static token => ClfLogStyle.Validate(token, provider: null, out ClfLogStyle? style) is null ? Some(style) : None).ToFin(Malformed(node, format))
        from curve in style.Base.Match(
            Some: static logBase => Fin.Succ(new LogCurve(logBase, Vector3.One, Vector3.Zero, Vector3.One, Vector3.Zero, None)),
            None: () => Logarithm(node, format, style.Camera))
        select style.Table(curve);

    internal static Fin<LutTable> ReadExponent(XElement node, LutFormat format) => Exponential(node, format, ExponentParams, ExponentValue);

    internal static Fin<LutTable> ReadGamma(XElement node, LutFormat format) => Exponential(node, format, "GammaParams", "gamma");

    internal static Fin<LutTable> ReadCdl(XElement node, LutFormat format) =>
        from read in (
                Attribute(node, Style).Match(
                    Some: static token => CdlStyle.Validate(token, provider: null, out CdlStyle? style) is null
                        ? Some(style!)
                        : toSeq(CdlStyle.Items).Find(item => string.Equals(item.Legacy, token, StringComparison.OrdinalIgnoreCase)),
                    None: static () => Some(CdlStyle.Fwd)).ToFin(Malformed(node, format)),
                CorrectionValues(node, _ => Malformed(node, format)).ToFin())
            .Apply(static (style, values) => (Style: style, Values: values))
            .As()
        from _ in guard<Error>(
            Vector3.GreaterThanOrEqualAll(read.Values.Slope, Vector3.Zero) && Vector3.GreaterThanAll(read.Values.Power, Vector3.Zero) && read.Values.Saturation >= 0f,
            Malformed(node, format)).ToFin()
        select (LutTable)new LutTable.Cdl(read.Style, read.Values.Slope, read.Values.Offset, read.Values.Power, read.Values.Saturation);

    internal static Validation<Error, (Vector3 Slope, Vector3 Offset, Vector3 Power, float Saturation)> CorrectionValues(XElement correction, Func<string, Error> malformed) {
        Validation<Error, T> Field<T>(string parent, string name, Func<string, Option<T>> parse, T absent) =>
            Child(correction, parent).Match(
                Some: held => Child(held, name).Bind(element => parse(element.Value)).ToValidation(malformed(name)),
                None: () => Validation.Success<Error, T>(absent));
        return (Field(SopNode, Slope, Triple, Vector3.One), Field(SopNode, Offset, Triple, Vector3.Zero), Field(SopNode, Power, Triple, Vector3.One), Field(SatNode, Saturation, Invariant.Number<float>, 1f))
            .Apply(static (slope, offset, power, saturation) => (slope, offset, power, saturation))
            .As();
    }

    private static Fin<LutTable> Tabled(LutFormat format, string text, int start, bool braced, int width, Seq<(int Floats, Func<ReadOnlyMemory<float>, Fin<LutTable>> Build)> tables) {
        int stated = tables.Fold(0, static (sum, table) => sum + table.Floats) / 3;
        Span<System.Range> tokens = stackalloc System.Range[4];
        Span<float> row = stackalloc float[3];
        using MemoryOwner<float> rows = MemoryOwner<float>.Allocate(3 * stated);
        Span<Vector3> entries = MemoryMarshal.Cast<float, Vector3>(rows.Span);
        (int number, int read) = (0, 0);
        foreach (ReadOnlySpan<char> raw in text.AsSpan().EnumerateLines()) {
            ReadOnlySpan<char> line = raw.Trim();
            if (++number < start || line.IsEmpty || line[0] == '#')
                continue;
            if (braced && line[0] == '}')
                break;
            if (line.SplitAny(tokens[..(width + 1)], Blanks, StringSplitOptions.RemoveEmptyEntries) != width || Parsed(line, tokens, row[..width]) != width)
                return new LutLineMalformed(format, number);
            row[width..].Fill(row[0]);
            if (read < stated)
                entries[read] = new Vector3(row);
            read++;
        }
        return read != stated
            ? new LutCountMismatched(format, stated, read)
            : tables.Zip(tables.Map(static table => table.Floats).Scan(0, static (offset, floats) => offset + floats))
                .Traverse(pair => pair.First.Build(rows.Memory.Slice(pair.Second, pair.First.Floats)).ToValidation())
                .As().ToFin()
                .Map(Chained);
    }

    private static (int Floats, Func<ReadOnlyMemory<float>, Fin<LutTable>> Build) CurveTable(CurveLength length, (Vector3 Minimum, Vector3 Maximum) domain) =>
        (3 * length, rows => Lut1D.From(length, rows, domain.Minimum, domain.Maximum).Map(static table => (LutTable)new LutTable.ChannelCurve(table)));

    private static (int Floats, Func<ReadOnlyMemory<float>, Fin<LutTable>> Build) LatticeTable(LatticeSize size, (Vector3 Minimum, Vector3 Maximum) domain, LutInterpolation interpolation) =>
        (3 * size * size * size, rows => Lut3D.From(size, rows, domain.Minimum, domain.Maximum).Map(lattice => (LutTable)new LutTable.Cube(lattice, interpolation)));

    private static LutTable Chained(Seq<LutTable> steps) => steps is [var step] ? step : new LutTable.Sequence(steps);

    private static (Vector3 Minimum, Vector3 Maximum) Domain(Option<(Vector2 Value, int Line)> range, Option<(Vector3 Value, int Line)> low, Option<(Vector3 Value, int Line)> high) =>
        range.Map(static held => (new Vector3(held.Value.X), new Vector3(held.Value.Y)))
            .IfNone((low.Map(static held => held.Value).IfNone(Vector3.Zero), high.Map(static held => held.Value).IfNone(Vector3.One)));

    private static Fin<Lut1D> Curve(XElement node, LutFormat format, float scale) =>
        from array in (
                Marked(node, "rawHalfs", "true")
                    ? Values<ushort>(node, format).Map(static array => (array.Dimensions, Values: array.Values.Select(static bits => (float)BitConverter.UInt16BitsToHalf(bits)).ToArray()))
                    : Values<float>(node, format),
                guard<Error>(!Marked(node, "halfDomain", "true") && !Marked(node, "hueAdjust", "dw3"), Unheld(node, format)).ToFin())
            .Apply(static (array, _) => array)
            .As()
        from length in (array.Dimensions is [var entries, 1 or 3] && array.Values.Length == entries * array.Dimensions[1] ? Sized<CurveLength>(entries) : None).ToFin(Malformed(node, format))
        from curve in Lut1D.From(length, array.Values.SelectMany(value => Enumerable.Repeat(value / scale, 3 / array.Dimensions[1])).ToArray(), Vector3.Zero, Vector3.One)
        select curve;

    private static Fin<LutTable> Indexed(XElement node, LutFormat format, float scale, int length, LutTable table) =>
        Child(node, "IndexMap").Match(
            Some: map =>
                from entries in map.Value.Split(Blanks.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) is [var first, var second]
                    ? (from low in IndexEntry(first) from high in IndexEntry(second) select (Low: low, High: high)).ToFin(Malformed(map, format))
                    : Fail<Error>(Unheld(map, format))
                select (LutTable)new LutTable.Sequence([
                    new LutTable.Remap(new RemapBounds.Between(
                        new RemapBound(entries.Low.Input / scale, entries.Low.Index / (length - 1)),
                        new RemapBound(entries.High.Input / scale, entries.High.Index / (length - 1)),
                        Clamps: true)),
                    table]),
            None: () => table);

    private static Option<(float Input, float Index)> IndexEntry(string entry) =>
        entry.Split('@') is [var input, var index] ? from value in Invariant.Number<float>(input) from at in Invariant.Number<float>(index) select (value, at) : None;

    private static Fin<Option<RemapBound>> Bound(XElement node, LutFormat format, string input, string output, (float In, float Out) scales) =>
        (Child(node, input).Map(static element => element.Value), Child(node, output).Map(static element => element.Value)) switch {
            var (inside, outside) when inside.IsNone && outside.IsNone => Option<RemapBound>.None,
            var (inside, outside) => (from value in inside.Bind(Invariant.Number<float>) from mapped in outside.Bind(Invariant.Number<float>) select Some(new RemapBound(value / scales.In, mapped / scales.Out)))
                .ToFin(Malformed(node, format)),
        };

    private static Fin<LogCurve> Logarithm(XElement node, LutFormat format, bool camera) =>
        from bases in Children(node, LogParams).TraverseM(element => Decimal(element, format, LogBase)).As()
        from logBase in bases.Somes().Distinct() switch {
            [] => Fin.Succ(2f),
            [> 0f and not 1f and var stated] => Fin.Succ(stated),
            _ => Fin.Fail<float>(Malformed(node, format)),
        }
        from lanes in Laned(
            node, format, LogParams,
            element =>
                from read in (
                        Decimal(element, format, LogSideSlope),
                        Decimal(element, format, LogSideOffset),
                        Decimal(element, format, LinSideSlope),
                        Decimal(element, format, LinSideOffset),
                        Decimal(element, format, LinSideBreak),
                        Decimal(element, format, LinearSlope))
                    .Apply(static (logSlope, logOffset, linSlope, linOffset, knee, slope) =>
                        (Sides: new Vector4(logSlope.IfNone(1f), logOffset.IfNone(0f), linSlope.IfNone(1f), linOffset.IfNone(0f)), Knee: knee, Slope: slope))
                    .As()
                from _ in guard<Error>(
                    read.Sides.X != 0f && read.Sides.Z != 0f && read.Knee.IsSome == camera && (camera || read.Slope.IsNone),
                    Malformed(element, format)).ToFin()
                select (read.Sides, Camera: read.Knee.Map(at => (Knee: at, Slope: read.Slope.IfNone(() => read.Sides.X * read.Sides.Z / (((read.Sides.Z * at) + read.Sides.W) * float.Log(logBase)))))),
            camera ? None : Some((Sides: new Vector4(1f, 0f, 1f, 0f), Camera: Option<(float Knee, float Slope)>.None)))
        select new LogCurve(
            logBase,
            Across(lanes, static lane => lane.Sides.X),
            Across(lanes, static lane => lane.Sides.Y),
            Across(lanes, static lane => lane.Sides.Z),
            Across(lanes, static lane => lane.Sides.W),
            lanes.Traverse(static lane => lane.Camera).As().Map(static joints => new CameraSegment(Across(joints, static joint => joint.Knee), Across(joints, static joint => joint.Slope))));

    private static Fin<LutTable> Exponential(XElement node, LutFormat format, string parameters, string exponent) =>
        from style in Attribute(node, Style).Bind(static token => ExponentStyle.Validate(token, provider: null, out ExponentStyle? style) is null ? Some(style) : None).ToFin(Malformed(node, format))
        from lanes in Laned(
            node, format, parameters,
            element =>
                from read in (Decimal(element, format, exponent).Bind(held => held.ToFin(Malformed(element, format))), Decimal(element, format, OffsetValue))
                    .Apply(static (power, offset) => (Power: power, Offset: offset))
                    .As()
                from _ in guard<Error>(
                    style.Basic.IsSome ? read.Power is >= 1f and <= 10f && read.Offset.Exists(static held => held is >= 0f and <= 0.9f) : read.Power > 0f && read.Offset.IsNone,
                    Malformed(element, format)).ToFin()
                select (read.Power, Offset: read.Offset.IfNone(0f)),
            Some((Power: 1f, Offset: 0f)))
        from table in (Across(lanes, static lane => lane.Power), Across(lanes, static lane => lane.Offset)) switch {
            var (power, offset) when offset == Vector3.Zero => Fin.Succ<LutTable>(new LutTable.Exponent(style.Basic.IfNone(style), power, offset)),
            var (power, offset) when Vector3.GreaterThanAll(offset, Vector3.Zero) && Vector3.GreaterThanAll(power, Vector3.One) => Fin.Succ<LutTable>(new LutTable.Exponent(style, power, offset)),
            _ => Fin.Fail<LutTable>(Malformed(node, format)),
        }
        select table;

    private static Fin<Seq<T>> Laned<T>(XElement node, LutFormat format, string name, Func<XElement, Fin<T>> read, Option<T> unstated) =>
        Children(node, name)
            .TraverseM(element =>
                from lanes in Attribute(element, Channel).Match(
                    Some: static letter => letter.Length == 1 && Lanes.IndexOf(letter, StringComparison.OrdinalIgnoreCase) is var lane and >= 0 ? Some(Seq(lane)) : None,
                    None: static () => Some(Seq(0, 1, 2))).ToFin(Malformed(element, format))
                from value in read(element)
                select (Lanes: lanes, Value: value))
            .As()
            .Bind(entries => Seq(0, 1, 2)
                .Traverse(lane => (entries.Filter(entry => entry.Lanes.Exists(held => held == lane)).Last.Map(static entry => entry.Value) | unstated).ToFin(Malformed(node, format)))
                .As());

    private static Vector3 Across<T>(Seq<T> lanes, Func<T, float> lane) => new([.. lanes.Map(lane)]);

    private static Fin<(float In, float Out)> Scales(XElement node, LutFormat format) =>
        (Scale(node, format, InBitDepth), Scale(node, format, OutBitDepth)).Apply(static (input, output) => (In: input, Out: output)).As();

    private static Fin<float> Scale(XElement node, LutFormat format, string name) =>
        ClfBitDepth.Validate(Attribute(node, name).IfNone(ClfBitDepth.Float32.Key), provider: null, out ClfBitDepth? depth) is null ? depth!.Scale : Malformed(node, format);

    private static Fin<Option<float>> Decimal(XElement element, LutFormat format, string name) =>
        Attribute(element, name).Traverse(text => Invariant.Number<float>(text).ToFin(Malformed(element, format))).As();

    private static Fin<(int[] Dimensions, T[] Values)> Values<T>(XElement node, LutFormat format) where T : struct, INumberBase<T> =>
        (from array in Child(node, ArrayElement)
         from dimensions in Attribute(array, Dimensions).Bind(static text => Numbers<int>(text))
         from values in Numbers<T>(array.Value)
         select (dimensions, values)).ToFin(Malformed(node, format));

    private static LutTable.Affine Affine(ReadOnlySpan<float> values, int columns, (float In, float Out) scales) =>
        (scales.In / scales.Out) switch {
            var gain => new LutTable.Affine(
                new ColorMatrix3x3(
                    values[0] * gain, values[1] * gain, values[2] * gain,
                    values[columns] * gain, values[columns + 1] * gain, values[columns + 2] * gain,
                    values[2 * columns] * gain, values[(2 * columns) + 1] * gain, values[(2 * columns) + 2] * gain),
                columns == 4 ? new Vector3(values[3], values[7], values[11]) / scales.Out : Vector3.Zero),
        };

    private static Option<XElement> Child(XElement node, string name) => Children(node, name).Head;

    private static Seq<XElement> Children(XElement node, string name) =>
        toSeq(node.Elements()).Filter(element => string.Equals(element.Name.LocalName, name, StringComparison.Ordinal));

    private static Option<string> Attribute(XElement node, string name) => Optional(node.Attribute(name)?.Value);

    private static bool Marked(XElement node, string name, string token) => Attribute(node, name).Exists(held => string.Equals(held, token, StringComparison.OrdinalIgnoreCase));

    private static int Line(XElement node) => ((IXmlLineInfo)node).LineNumber;

    private static LutLineMalformed Malformed(XElement node, LutFormat format) => new(format, Line(node));

    private static LutStepRefused Unheld(XElement node, LutFormat format) => new(format, Line(node), node.Name.LocalName);

    private static Option<T> Sized<T>(int value) where T : struct, IObjectFactory<T, int, InvalidColor> => T.Validate(value, provider: null, out T item) is null ? Some(item) : None;

    private static Option<Vector3> Triple(string text) => Numbers<float>(text).Bind(static values => values is [var r, var g, var b] ? Some(new Vector3(r, g, b)) : None);

    private static Option<Vector2> Pair(string text) => Numbers<float>(text).Bind(static values => values is [var low, var high] ? Some(new Vector2(low, high)) : None);

    private static Option<T[]> Numbers<T>(ReadOnlySpan<char> text) where T : struct, INumberBase<T> {
        int count = 0;
        foreach (System.Range token in text.SplitAny(Blanks))
            count += text[token].IsEmpty ? 0 : 1;
        using SpanOwner<System.Range> tokens = SpanOwner<System.Range>.Allocate(count + 1);
        T[] values = new T[count];
        return Parsed(text, tokens.Span[..text.SplitAny(tokens.Span, Blanks, StringSplitOptions.RemoveEmptyEntries)], values) == count ? values : None;
    }

    private static int Parsed<T>(ReadOnlySpan<char> text, ReadOnlySpan<System.Range> tokens, Span<T> into) where T : struct, INumberBase<T> {
        int at = 0;
        while (at < into.Length && T.TryParse(text[tokens[at]], NumberStyles.Float, CultureInfo.InvariantCulture, out into[at]))
            at++;
        return at;
    }

    private static int Transposed(int node, int edge) => (node % edge * edge * edge) + (node / edge % edge * edge) + (node / (edge * edge));

    private static float[] Transposed(ReadOnlySpan<float> values, int edge, float scale) {
        float[] reordered = GC.AllocateUninitializedArray<float>(values.Length);
        for (int at = 0; at < values.Length; at++)
            reordered[at] = values[(3 * Transposed(at / 3, edge)) + (at % 3)] * scale;
        return reordered;
    }

    private sealed record Header(LutFormat Format, HashMap<string, (string Value, int Line)> Lines, int End) {
        public static Fin<Header> Read(string text, LutFormat format, Seq<string> keywords, Func<ReadOnlySpan<char>, bool> heading) {
            Span<System.Range> parts = stackalloc System.Range[2];
            (HashMap<string, (string Value, int Line)> lines, int number) = (HashMap<string, (string Value, int Line)>(), 0);
            foreach (ReadOnlySpan<char> raw in text.AsSpan().EnumerateLines()) {
                ReadOnlySpan<char> line = raw.Trim();
                number++;
                if (line.IsEmpty || line[0] == '#')
                    continue;
                if (!heading(line))
                    return new Header(format, lines, number);
                int count = line.SplitAny(parts, Blanks, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string keyword = line[parts[0]].ToString();
                if (keywords.Find(known => string.Equals(known, keyword, StringComparison.OrdinalIgnoreCase)).Filter(known => !lines.ContainsKey(known)).Case is not string key)
                    return new LutLineMalformed(format, number);
                lines = lines.Add(key, (count == 2 ? line[parts[1]].ToString() : "", number));
            }
            return new Header(format, lines, number + 1);
        }

        public Fin<Option<(T Value, int Line)>> Stated<T>(string keyword, Func<string, Option<T>> parse) =>
            Lines.Find(keyword).Traverse(line => parse(line.Value).Map(value => (value, line.Line)).ToFin(new LutLineMalformed(Format, line.Line))).As();
    }

    // --- [WRITES]
    public static Fin<string> Render(LutTable table, LutFormat format, string name) =>
        format.Switch<(LutTable Table, string Name), Option<string>>(
            (table, name),
            cube: static render => RenderCube(Flat(render.Table)),
            spi1d: static render => RenderSpi1d(Flat(render.Table)),
            spi3d: static render => RenderSpi3d(Flat(render.Table)),
            spiMatrix: static render => RenderSpiMatrix(Flat(render.Table)),
            autodesk: static render => Render3dl(Flat(render.Table)),
            clf: static render => RenderProcessList(LutFormat.Clf, render.Table, render.Name),
            ctf: static render => RenderProcessList(LutFormat.Ctf, render.Table, render.Name),
            hald: static _ => None,
            keyShotLookup: static _ => None).ToFin(new LutFormatRefused(format));

    public static IO<Unit> Write(LutTable table, LutFormat format, OutputPath path) =>
        from text in IO.lift(Render(table, format, Path.GetFileNameWithoutExtension(path)))
        from _ in StillWriter.Commit(path, text)
        select unit;

    public static IO<Unit> WriteLookup(LutTable table, LookupLayout layout, FileMetadata metadata, OutputPath path) =>
        from still in IO.lift(Lookup(table, layout))
        from _ in StillWriter.Write(still, metadata, path)
        select unit;

    private static Fin<StillFormat> Lookup(LutTable table, LookupLayout layout) =>
        (from lattice in Only<LutTable.Cube>(Flat(table)).Map(static cube => cube.Lattice)
         where Coded(lattice)
         from image in layout.Image(lattice.Size)
         select new PixelFrame(System.Drawing.Point.Empty, image, image, block => {
             Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
             ReadOnlySpan<Vector3> entries = MemoryMarshal.Cast<float, Vector3>(lattice.Data);
             for (int pixel = 0; pixel < pixels.Length; pixel++)
                 pixels[pixel] = new Vector4(entries[layout.Node(pixel % image.Width, pixel / image.Width, image, lattice.Size)], 1f);
         }))
            .ToFin(new LutFormatRefused(layout.Format))
            .Bind(static frame => StillFormat.PngRaster.From(frame, OutputDepth.UInt16, None, alpha: false));

    private static Option<string> RenderCube(Seq<LutTable> steps) =>
        steps switch {
            [LutTable.Cube { Lattice: var lattice }] =>
                Rows(Domained(Sized(new StringBuilder(), Lut3DSize, lattice.Size), lattice.DomainMinimum, lattice.DomainMaximum), lattice.Data, 3).ToString(),
            [LutTable.ChannelCurve { Table: var curve }] =>
                Rows(Domained(Sized(new StringBuilder(), Lut1DSize, curve.Length), curve.DomainMinimum, curve.DomainMaximum), Interleaved(curve), 3).ToString(),
            [LutTable.ChannelCurve { Table: var shaper }, LutTable.Cube { Lattice: var lattice }] => Resolve(shaper, lattice),
            [LutTable.Log { Curve: var curve }, LutTable.Cube { Lattice: var lattice }] => Some(curve).Filter(Uniform).Bind(Shaper).Bind(shaper => Resolve(shaper, lattice)),
            _ => None,
        };

    private static Option<string> RenderSpi1d(Seq<LutTable> steps) =>
        from curve in Only<LutTable.ChannelCurve>(steps).Map(static step => step.Table)
        where Uniform(curve.DomainMinimum) && Uniform(curve.DomainMaximum)
        let width = curve.Channel(0).SequenceEqual(curve.Channel(1)) && curve.Channel(1).SequenceEqual(curve.Channel(2)) ? 1 : 3
        select Rows(
            new StringBuilder().Append(CultureInfo.InvariantCulture, $"{Version} 1\n{From} {curve.DomainMinimum.X} {curve.DomainMaximum.X}\n{Length} {curve.Length}\n{Components} {width}\n{{\n"),
            width == 1 ? curve.Channel(0) : Interleaved(curve),
            width).Append("}\n").ToString();

    private static Option<string> RenderSpi3d(Seq<LutTable> steps) =>
        from lattice in Only<LutTable.Cube>(steps).Map(static step => step.Lattice)
        where lattice.DomainMinimum == Vector3.Zero && lattice.DomainMaximum == Vector3.One
        select Rows(
            new StringBuilder().Append(CultureInfo.InvariantCulture, $"{SpiLut} 1.0\n3 3\n{lattice.Size} {lattice.Size} {lattice.Size}\n"),
            Enumerable.Range(0, lattice.Size * lattice.Size * lattice.Size)
                .SelectMany<int, float>(node => [
                    node % lattice.Size, node / lattice.Size % lattice.Size, node / (lattice.Size * lattice.Size),
                    lattice.Data[3 * node], lattice.Data[(3 * node) + 1], lattice.Data[(3 * node) + 2]])
                .ToArray(),
            6).ToString();

    private static Option<string> RenderSpiMatrix(Seq<LutTable> steps) =>
        from affine in Only<LutTable.Affine>(steps)
        select Rows(
            new StringBuilder(),
            [
                affine.Matrix.M11, affine.Matrix.M12, affine.Matrix.M13, affine.Offset.X * ushort.MaxValue,
                affine.Matrix.M21, affine.Matrix.M22, affine.Matrix.M23, affine.Offset.Y * ushort.MaxValue,
                affine.Matrix.M31, affine.Matrix.M32, affine.Matrix.M33, affine.Offset.Z * ushort.MaxValue,
            ],
            4).ToString();

    private static Option<string> Render3dl(Seq<LutTable> steps) =>
        from lattice in Only<LutTable.Cube>(steps).Map(static step => step.Lattice)
        where Coded(lattice) && lattice.Size > 3
        from mesh in OutputDepth.UInt10.Levels
        from codes in OutputDepth.UInt12.Levels
        select Rows(
            Rows(new StringBuilder(), Quantized(Enumerable.Range(0, lattice.Size).Select(at => (float)at / (lattice.Size - 1)).ToArray(), mesh), lattice.Size),
            Quantized(Transposed(lattice.Data, lattice.Size, 1f), codes),
            3).ToString();

    private static Option<string> Resolve(Lut1D shaper, Lut3D lattice) =>
        Uniform(shaper.DomainMinimum) && Uniform(shaper.DomainMaximum) && Uniform(lattice.DomainMinimum) && Uniform(lattice.DomainMaximum)
            ? Rows(
                Rows(
                    Ranged(
                        Sized(Ranged(Sized(new StringBuilder(), Lut1DSize, shaper.Length), Lut1DInputRange, shaper.DomainMinimum.X, shaper.DomainMaximum.X), Lut3DSize, lattice.Size),
                        Lut3DInputRange,
                        lattice.DomainMinimum.X,
                        lattice.DomainMaximum.X),
                    Interleaved(shaper),
                    3),
                lattice.Data,
                3).ToString()
            : None;

    private static Option<Lut1D> Shaper(LogCurve curve) {
        float top = curve.Inverse(Vector3.One).X;
        using MemoryOwner<float> rows = MemoryOwner<float>.Allocate(3 * CurveLength.ResolveShaper);
        Span<Vector3> entries = MemoryMarshal.Cast<float, Vector3>(rows.Span);
        for (int entry = 0; entry < entries.Length; entry++)
            entries[entry] = curve.Forward(new Vector3(entry * top / (entries.Length - 1)));
        return Lut1D.From(CurveLength.ResolveShaper, rows.Memory, Vector3.Zero, new Vector3(top)).ToOption();
    }

    private static Option<string> RenderProcessList(LutFormat format, LutTable table, string name) =>
        Nodes(format, table).Map(nodes => {
            StringBuilder builder = new("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            using (XmlWriter writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true, NewLineChars = "\n", OmitXmlDeclaration = true }))
                new XElement(ProcessList, new XAttribute("id", name), format == LutFormat.Clf ? new XAttribute("compCLFversion", "3") : new XAttribute("version", "2"), nodes)
                    .WriteTo(writer);
            return builder.Append('\n').ToString();
        });

    private static Option<Seq<XElement>> Nodes(LutFormat format, LutTable table) =>
        table.Switch(
            format,
            cube: static (_, cube) => toSeq(ClfInterpolation.Items).Find(item => item.Interpolation == cube.Interpolation).Map(interpolation =>
                Normalized(cube.Lattice.DomainMinimum, cube.Lattice.DomainMaximum).Add(Node(
                    ProcessNode.Lut3D,
                    new XAttribute(Interpolation, interpolation.Key),
                    Grid([cube.Lattice.Size, cube.Lattice.Size, cube.Lattice.Size, 3], Transposed(cube.Lattice.Data, cube.Lattice.Size, 1f))))),
            channelCurve: static (_, curve) => Normalized(curve.Table.DomainMinimum, curve.Table.DomainMaximum)
                .Add(Node(ProcessNode.Lut1D, Grid([curve.Table.Length, 3], Interleaved(curve.Table)))),
            inverseChannelCurve: static (format, inverse) => ProcessNode.InverseLut1D.Formats.Exists(held => held == format)
                ? Seq(Node(ProcessNode.InverseLut1D, Grid([inverse.Table.Forward.Length, 3], Interleaved(inverse.Table.Forward))))
                    + Restored(inverse.Table.Forward.DomainMinimum, inverse.Table.Forward.DomainMaximum)
                : None,
            affine: static (_, affine) => Seq(Matrix(affine.Matrix, affine.Offset)),
            remap: static (_, remap) => Seq(remap.Bounds.Switch(
                between: static between => Node(ProcessNode.Range, new XAttribute(Style, between.Clamps ? Clamp : NoClamp), Bounded(MinIn, MinOut, between.Minimum), Bounded(MaxIn, MaxOut, between.Maximum)),
                floor: static floor => Node(ProcessNode.Range, Bounded(MinIn, MinOut, floor.Minimum)),
                ceiling: static ceiling => Node(ProcessNode.Range, Bounded(MaxIn, MaxOut, ceiling.Maximum)))),
            log: static (_, log) => Seq(Logged(log.Curve, log.Curve.Camera.IsSome ? ClfLogStyle.CameraLinToLog : ClfLogStyle.LinToLog)),
            antilog: static (_, antilog) => Seq(Logged(antilog.Curve, antilog.Curve.Camera.IsSome ? ClfLogStyle.CameraLogToLin : ClfLogStyle.LogToLin)),
            exponent: static (_, exponent) => Seq(Node(
                ProcessNode.Exponent,
                new XAttribute(Style, exponent.Style.Key),
                Parameters(ExponentParams, lane => exponent.Style.Basic.IsSome
                    ? Seq((ExponentValue, exponent.Power[lane]), (OffsetValue, exponent.Offset[lane]))
                    : Seq((ExponentValue, exponent.Power[lane]))))),
            cdl: static (_, cdl) => Seq(Node(ProcessNode.AscCdl, new XAttribute(Style, cdl.Style.Key), CorrectionNodes(XNamespace.None, cdl))),
            sequence: static (format, sequence) => sequence.Steps.TraverseM(step => Nodes(format, step)).As().Map(static nodes => nodes.Flatten()));

    internal static XElement[] CorrectionNodes(XNamespace space, LutTable.Cdl cdl) => [
        new(space + SopNode, new XElement(space + Slope, Spaced(cdl.Slope)), new XElement(space + Offset, Spaced(cdl.Offset)), new XElement(space + Power, Spaced(cdl.Power))),
        new(space + SatNode, new XElement(space + Saturation, cdl.Saturation.ToString(CultureInfo.InvariantCulture))),
    ];

    private static XElement Node(ProcessNode kind, params ReadOnlySpan<object> content) =>
        new(kind.Key, new XAttribute(InBitDepth, ClfBitDepth.Float32.Key), new XAttribute(OutBitDepth, ClfBitDepth.Float32.Key), content.ToArray());

    private static XElement Matrix(ColorMatrix3x3 matrix, Vector3 offset) =>
        offset == Vector3.Zero
            ? Node(ProcessNode.Matrix, Grid([3, 3], [matrix.M11, matrix.M12, matrix.M13, matrix.M21, matrix.M22, matrix.M23, matrix.M31, matrix.M32, matrix.M33]))
            : Node(ProcessNode.Matrix, Grid([3, 4], [matrix.M11, matrix.M12, matrix.M13, offset.X, matrix.M21, matrix.M22, matrix.M23, offset.Y, matrix.M31, matrix.M32, matrix.M33, offset.Z]));

    private static Seq<XElement> Normalized(Vector3 minimum, Vector3 maximum) =>
        minimum == Vector3.Zero && maximum == Vector3.One ? [] : [Diagonal(Vector3.One / (maximum - minimum), -minimum / (maximum - minimum))];

    private static Seq<XElement> Restored(Vector3 minimum, Vector3 maximum) =>
        minimum == Vector3.Zero && maximum == Vector3.One ? [] : [Diagonal(maximum - minimum, minimum)];

    private static XElement Diagonal(Vector3 scale, Vector3 offset) => Matrix(new ColorMatrix3x3(scale.X, 0f, 0f, 0f, scale.Y, 0f, 0f, 0f, scale.Z), offset);

    private static XElement Grid(ReadOnlySpan<int> dimensions, ReadOnlySpan<float> values) =>
        new(ArrayElement, new XAttribute(Dimensions, string.Join(' ', dimensions.ToArray())), Rows(new StringBuilder().Append('\n'), values, dimensions[^1]).ToString());

    private static XElement[] Bounded(string input, string output, RemapBound bound) =>
        [new(input, bound.In.ToString(CultureInfo.InvariantCulture)), new(output, bound.Out.ToString(CultureInfo.InvariantCulture))];

    private static XElement Logged(LogCurve curve, ClfLogStyle style) =>
        Node(
            ProcessNode.Log,
            new XAttribute(Style, style.Key),
            Parameters(LogParams, lane =>
                Seq((LogBase, curve.Base), (LogSideSlope, curve.LogSlope[lane]), (LogSideOffset, curve.LogOffset[lane]), (LinSideSlope, curve.LinSlope[lane]), (LinSideOffset, curve.LinOffset[lane]))
                + curve.Camera.ToSeq().Bind(camera => Seq((LinSideBreak, camera.Break[lane]), (LinearSlope, camera.Slope[lane])))));

    private static Seq<XElement> Parameters(string element, Func<int, Seq<(string Name, float Value)>> lane) =>
        Seq(0, 1, 2).Map(lane).Strict() switch {
            var lanes when lanes.Distinct().Count == 1 => lanes.Take(1).Map(held => new XElement(element, Attributes(held))),
            var lanes => lanes.Map((held, at) => new XElement(element, new XAttribute(Channel, Lanes[at].ToString()), Attributes(held))),
        };

    private static Seq<XAttribute> Attributes(Seq<(string Name, float Value)> held) =>
        held.Map(static attribute => new XAttribute(attribute.Name, attribute.Value.ToString(CultureInfo.InvariantCulture)));

    private static string Spaced(Vector3 value) => string.Create(CultureInfo.InvariantCulture, $"{value.X} {value.Y} {value.Z}");

    private static Seq<LutTable> Flat(LutTable table) => table is LutTable.Sequence { Steps: var steps } ? steps.Bind(Flat) : [table];

    private static Option<T> Only<T>(Seq<LutTable> steps) where T : LutTable => steps is [T only] ? only : None;

    private static bool Coded(Lut3D lattice) =>
        lattice.DomainMinimum == Vector3.Zero && lattice.DomainMaximum == Vector3.One && !lattice.Data.ContainsAnyExceptInRange(0f, 1f);

    private static bool Uniform(Vector3 value) => value.X == value.Y && value.Y == value.Z;

    private static bool Uniform(LogCurve curve) =>
        Uniform(curve.LogSlope) && Uniform(curve.LogOffset) && Uniform(curve.LinSlope) && Uniform(curve.LinOffset)
            && curve.Camera.ForAll(static camera => Uniform(camera.Break) && Uniform(camera.Slope));

    private static float[] Interleaved(Lut1D curve) {
        float[] rows = GC.AllocateUninitializedArray<float>(3 * curve.Length);
        for (int at = 0; at < rows.Length; at++)
            rows[at] = curve.Channel(at % 3)[at / 3];
        return rows;
    }

    private static float[] Quantized(ReadOnlySpan<float> values, Quantizer levels) {
        float[] codes = GC.AllocateUninitializedArray<float>(values.Length);
        for (int at = 0; at < values.Length; at++)
            codes[at] = levels.Round(new Vector4(levels.MaxCode * values[at])).X;
        return codes;
    }

    private static StringBuilder Sized(StringBuilder builder, string keyword, int size) => builder.Append(CultureInfo.InvariantCulture, $"{keyword} {size}\n");

    private static StringBuilder Domained(StringBuilder builder, Vector3 minimum, Vector3 maximum) =>
        minimum == Vector3.Zero && maximum == Vector3.One ? builder : builder.Append(CultureInfo.InvariantCulture, $"{DomainMin} {Spaced(minimum)}\n{DomainMax} {Spaced(maximum)}\n");

    private static StringBuilder Ranged(StringBuilder builder, string keyword, float minimum, float maximum) =>
        minimum == 0f && maximum == 1f ? builder : builder.Append(CultureInfo.InvariantCulture, $"{keyword} {minimum} {maximum}\n");

    private static StringBuilder Rows(StringBuilder builder, ReadOnlySpan<float> values, int width) {
        for (int at = 0; at < values.Length; at++)
            builder.Append(CultureInfo.InvariantCulture, $"{values[at]}{(at % width == width - 1 ? '\n' : ' ')}");
        return builder;
    }
}

internal static class EmbeddedTables {
    public static readonly Func<string, Lut3D> Lattice = memoUnsafe(static (string file) =>
        LutFiles.Parse(Text(file), LutFormat.Cube)
            .Bind(static Fin<Lut3D> (table) => table is LutTable.Cube { Lattice: var lattice } ? lattice : new LutFormatRefused(LutFormat.Cube))
            .ThrowIfFail());

    public static readonly Func<string, Lut1D> Curve = memoUnsafe(static (string file) =>
        LutFiles.Parse(Text(file), LutFormat.Spi1d)
            .Bind(static Fin<Lut1D> (table) => table is LutTable.ChannelCurve { Table: var curve } ? curve : new LutFormatRefused(LutFormat.Spi1d))
            .ThrowIfFail());

    public static Stream Open(string file) => typeof(EmbeddedTables).Assembly.GetManifestResourceStream(typeof(EmbeddedTables), $"Tables.{file}")!;

    private static string Text(string file) {
        using StreamReader reader = new(Open(file));
        return reader.ReadToEnd();
    }
}
