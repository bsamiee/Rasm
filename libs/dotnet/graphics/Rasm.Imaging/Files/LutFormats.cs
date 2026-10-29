using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LutFormat {
    public static readonly LutFormat Cube = Text("cube", ParseCube, RenderCube);
    public static readonly LutFormat Spi1d = Text("spi1d", ParseSpi1d, RenderSpi1d);
    public static readonly LutFormat Spi3d = Text("spi3d", ParseSpi3d, RenderSpi3d);
    public static readonly LutFormat SpiMatrix = Text("spimtx", ParseSpiMatrix, RenderSpiMatrix);
    public static readonly LutFormat Autodesk = Text("3dl", Parse3dl, Render3dl);
    public static readonly LutFormat CineSpace = Text("csp", ParseCsp, RenderCsp);
    public static readonly LutFormat Clf = ProcessList("clf", ctf: false);
    public static readonly LutFormat Ctf = ProcessList("ctf", ctf: true);
    public static readonly LutFormat ColorCorrection = Correction("cc", []);
    public static readonly LutFormat ColorCorrectionCollection = Correction("ccc", ["ColorCorrectionCollection"]);
    public static readonly LutFormat ColorDecisionList = Correction("cdl", ["ColorDecisionList", "ColorDecision"]);
    public static readonly LutFormat Hald = Lookup("hald",
        static edge => (int)float.Round(float.Sqrt(edge), MidpointRounding.ToEven) is var level && level * level == edge ? Some(level * level * level) : None,
        static (row, run, runs, edge) => ((row * runs) + run) * edge);
    public static readonly LutFormat KeyShotLookup = Lookup("keyshot-lookup", static edge => Some(edge * edge), static (row, run, _, edge) => (edge * (edge - 1 - row)) + (edge * edge * run));
    public static readonly LutFormat DirectDrawSurface = new(Dds, $".{Dds}", Volume, static (_, _, _) => ImagingFault.Of(ImagingReason.Unsupported, Dds));

    private const string Blanks = " \t\r\n";
    private const string Dds = "dds";
    private const int Codes = ushort.MaxValue + 1;
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
    private const string CspLut = "CSPLUTV100";
    private const string Csp1D = "1D";
    private const string Csp3D = "3D";
    private const string MetadataBegin = "BEGIN METADATA";
    private const string MetadataEnd = "END METADATA";
    private static readonly (Vector3 Low, Vector3 High) UnitDomain = (Vector3.Zero, Vector3.One);

    public string Extension { get; }

    public Func<ReadOnlyMemory<byte>, Fin<LutTable>> Decode { get; }

    public Func<LutTable, string, FileMetadata, Fin<Func<Stream, IO<Unit>>>> Encode { get; }

    public static IO<LutTable> Read(string path) =>
        (from bytes in IO.liftAsync(env => File.ReadAllBytesAsync(path, env.Token))
         from table in IO.lift(toSeq(Items).Filter(format => string.Equals(format.Extension, Path.GetExtension(path), StringComparison.OrdinalIgnoreCase)).Fold(
             Fin.Fail<LutTable>(ImagingFault.Of(ImagingReason.UnknownFormat, path)),
             (held, format) => held.BindFail(_ => format.Decode(bytes))))
         select table)
            .Catch(
                static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(),
                error => IO.fail<LutTable>(ImagingFault.Of(ImagingReason.Unreadable, path, inner: error)));

    private static LutFormat Text(string key, Func<string, Fin<LutTable>> parse, Func<LutTable, string, Option<string>> render) =>
        new(key, $".{key}",
            bytes => {
                using StreamReader reader = new(bytes.AsStream());
                return parse(reader.ReadToEnd());
            },
            (table, name, _) => Written(render(table, name), key, static (text, stream) => IO.liftAsync(async env => {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(text), env.Token).ConfigureAwait(false);
                return unit;
            })));

    private static LutFormat Lookup(string key, Func<int, Option<int>> width, Func<int, int, int, int, int> start) =>
        new(key, ".png",
            bytes => Try.lift(() => SixLabors.ImageSharp.Image.Load<RgbaVector>(bytes.Span)).Run().MapFail(error => ImagingFault.Of(ImagingReason.Refused, key, inner: error)).Bind(image => {
                using Image<RgbaVector> owned = image;
                int edge = (int)double.Round(double.Cbrt((double)owned.Width * owned.Height), MidpointRounding.ToEven);
                return ImagingFault.Unless(Cubed(edge) == (long)owned.Width * owned.Height && width(edge) == owned.Width, ImagingReason.UnknownFormat, (owned.Width, owned.Height)).Bind(_ => {
                    Vector4[] pixels = GC.AllocateUninitializedArray<Vector4>(owned.Width * owned.Height);
                    float[] samples = GC.AllocateUninitializedArray<float>(3 * pixels.Length);
                    owned.CopyPixelDataTo(MemoryMarshal.Cast<Vector4, RgbaVector>(pixels.AsSpan()));
                    Walk(new Span2D<Vector4>(pixels, owned.Height, owned.Width), samples, edge, start, read: true);
                    return Lattice(edge, samples, UnitDomain);
                });
            }),
            (table, name, metadata) => Written(
                from cube in table.Steps is [LutTable.Cube held] && Coded(held.Lattice) ? Some(held.Lattice) : None
                from extent in width(cube.Size).Bind(columns => PixelExtent.Validated(columns, (int)(Cubed(cube.Size) / columns)).ToOption())
                select Filled(new PixelFrame(default, extent, extent), cube, start),
                key,
                (frame, stream) => new ImageFile.Raster(frame, None, Alpha: false, new PngEncoder { BitDepth = PngBitDepth.Bit16, ColorType = PngColorType.Rgb })
                    .Write(metadata with { Fields = metadata.Fields.AddOrUpdate("Title", name) }, stream)));

    private static Fin<Func<Stream, IO<Unit>>> Written<T>(Option<T> content, string key, Func<T, Stream, IO<Unit>> write) =>
        content.ToFin(ImagingFault.Of(ImagingReason.Unsupported, key)).Map<Func<Stream, IO<Unit>>>(held => stream => write(held, stream));

    private static Fin<LutTable> ParseCube(string text) =>
        from scan in Scan.Of(text, ["TITLE", Lut1DSize, Lut3DSize, DomainMin, DomainMax, Lut1DInputRange, Lut3DInputRange])
        from stated in (scan.Stated(Lut1DSize, Single<int>), scan.Stated(Lut3DSize, Single<int>), scan.Stated(DomainMin, Triple), scan.Stated(DomainMax, Triple), scan.Stated(Lut1DInputRange, Interval), scan.Stated(Lut3DInputRange, Interval))
            .Apply(static (curve, lattice, low, high, curveRange, latticeRange) => (Curve: curve, Lattice: lattice, Domain: (low.IfNone(Vector3.Zero), high.IfNone(Vector3.One)), CurveRange: curveRange, LatticeRange: latticeRange))
            .As()
        from _ in ImagingFault.Unless(stated.Curve.IsSome || stated.Lattice.IsSome, ImagingReason.Missing, (Lut1DSize, Lut3DSize))
        let length = stated.Curve.IfNone(0)
        from rows in scan.Table(0, 3, length + Cubed(stated.Lattice.IfNone(0)))
        from steps in (stated.Curve.ToSeq().Map(_ => Curve(rows[..(3 * length)], stated.CurveRange.IfNone(stated.Domain)))
                + stated.Lattice.ToSeq().Map(size => Lattice(size, rows[(3 * length)..], stated.LatticeRange.IfNone(stated.Domain))))
            .Traverse(static step => step).As()
        select LutTable.Of(steps);

    private static Option<string> RenderCube(LutTable table, string name) =>
        Parts(table).Bind(parts => {
            bool shaped = parts.Curve.IsSome && parts.Lattice.IsSome;
            if (shaped && !(parts.Curve.ForAll(static curve => Uniform(curve.DomainMinimum, curve.DomainMaximum)) && parts.Lattice.ForAll(static lattice => Uniform(lattice.DomainMinimum, lattice.DomainMaximum))))
                return Option<string>.None;
            StringBuilder text = new StringBuilder().Append(CultureInfo.InvariantCulture, $"TITLE \"{name}\"\n");
            foreach (Lut1D curve in parts.Curve.ToSpan())
                text = Bounded(text.Append(CultureInfo.InvariantCulture, $"{Lut1DSize} {curve.Length}\n"), shaped ? Lut1DInputRange : None, curve.DomainMinimum, curve.DomainMaximum);
            foreach (Lut3D lattice in parts.Lattice.ToSpan())
                text = Bounded(text.Append(CultureInfo.InvariantCulture, $"{Lut3DSize} {lattice.Size}\n"), shaped ? Lut3DInputRange : None, lattice.DomainMinimum, lattice.DomainMaximum);
            foreach (Lut1D curve in parts.Curve.ToSpan())
                text = Rows(text, Interleaved(curve.Channel(0), curve.Channel(1), curve.Channel(2)), 3);
            foreach (Lut3D lattice in parts.Lattice.ToSpan())
                text = Rows(text, lattice.Data, 3);
            return text.ToString();
        });

    private static Fin<LutTable> ParseSpi1d(string text) =>
        from scan in Scan.Of(text, [Version, From, Length, Components, "{", "}"])
        from stated in (scan.Stated(Version, static value => value is "1" ? Some(unit) : None), scan.Stated(From, Interval), scan.Stated(Length, Single<int>), scan.Stated(Components, static value => Single<int>(value).Filter(static width => width is 1 or 3)))
            .Apply(static (_, domain, length, width) => (Domain: domain.IfNone(UnitDomain), Length: length, Width: width))
            .As()
        from shape in (stated.Length.ToFin(ImagingFault.Of(ImagingReason.Missing, Length)), stated.Width.ToFin(ImagingFault.Of(ImagingReason.Missing, Components)))
            .Apply(static (length, width) => (Length: length, Width: width)).As()
        from rows in scan.Table(0, shape.Width, shape.Length)
        from table in Curve(shape.Width is 3 ? rows : Interleaved(rows.Span, rows.Span, rows.Span), stated.Domain)
        select table;

    private static Option<string> RenderSpi1d(LutTable table, string name) =>
        from parts in Parts(table)
        where parts.Lattice.IsNone
        from curve in parts.Curve
        where Uniform(curve.DomainMinimum, curve.DomainMaximum)
        let width = curve.Channel(0).SequenceEqual(curve.Channel(1)) && curve.Channel(1).SequenceEqual(curve.Channel(2)) ? 1 : 3
        select Rows(
            new StringBuilder().Append(CultureInfo.InvariantCulture, $"{Version} 1\n{From} {curve.DomainMinimum.X} {curve.DomainMaximum.X}\n{Length} {curve.Length}\n{Components} {width}\n{{\n"),
            width is 1 ? curve.Channel(0) : Interleaved(curve.Channel(0), curve.Channel(1), curve.Channel(2)),
            width).Append("}\n").ToString();

    private static Fin<LutTable> ParseSpi3d(string text) =>
        from scan in Scan.Of(text, [SpiLut])
        from size in (scan.Row(0) is [3f, 3f] && scan.Row(1) is [var red, var green, var blue] && red == green && green == blue && float.IsInteger(red) ? Some((int)red) : None)
            .ToFin(ImagingFault.Of(ImagingReason.Malformed, SpiLut, scan.Line(1)))
        from _ in scan.Table(2, 6, Cubed(size))
        from samples in Placed(scan, size)
        from table in Lattice(size, samples, UnitDomain)
        select table;

    private static Option<string> RenderSpi3d(LutTable table, string name) =>
        from lattice in table.Steps is [LutTable.Cube { Lattice: var held }] && (held.DomainMinimum, held.DomainMaximum) == UnitDomain ? Some(held) : None
        select Indexed(new StringBuilder().Append(CultureInfo.InvariantCulture, $"{SpiLut} 1.0\n3 3\n{lattice.Size} {lattice.Size} {lattice.Size}\n"), lattice).ToString();

    private static Fin<LutTable> ParseSpiMatrix(string text) =>
        from scan in Scan.Of(text, [])
        from _ in ImagingFault.Unless(scan.Keywords.IsEmpty && scan.Values.Length is 12, ImagingReason.Malformed, scan.Values.Length)
        let matrix = Homogeneous(scan.Values.AsSpan(), 4)
        select (LutTable)new LutTable.Affine(matrix with { Translation = matrix.Translation / ushort.MaxValue });

    private static Option<string> RenderSpiMatrix(LutTable table, string name) =>
        from matrix in table.Steps is [LutTable.Affine held] ? Some(held.Matrix) : None
        select Rows(new StringBuilder(), Cells(matrix with { Translation = matrix.Translation * ushort.MaxValue }, 4), 4).ToString();

    private static Fin<LutTable> Parse3dl(string text) =>
        from scan in Scan.Of(text, [])
        from size in (Ramped(scan.Row(0)) ? Some(scan.Row(0).Length) : None).ToFin(ImagingFault.Of(ImagingReason.Malformed, scan.Row(0).Length, scan.Line(0)))
        from rows in scan.Table(1, 3, Cubed(size))
        from levels in toSeq(OutputDepth.Items).Map(static depth => depth.Levels).Somes().Find(held => held.MaxCode >= TensorPrimitives.Max(rows.Span))
            .ToFin(ImagingFault.Of(ImagingReason.Range, TensorPrimitives.Max(rows.Span)))
        from _ in ImagingFault.Unless(TensorPrimitives.Min(rows.Span) >= 0f, ImagingReason.Range, TensorPrimitives.Min(rows.Span))
        from table in Lattice(size, Transposed(rows.Span, size, 1f / levels.MaxCode), UnitDomain)
        select table;

    private static Option<string> Render3dl(LutTable table, string name) =>
        from lattice in table.Steps is [LutTable.Cube { Lattice: var held }] && Coded(held) && held.Size > 3 ? Some(held) : None
        from mesh in OutputDepth.UInt10.Levels
        from codes in OutputDepth.UInt12.Levels
        select Rows(
            Rows(new StringBuilder(), Quantized(Ramp(lattice.Size, 0f, 1f), mesh), lattice.Size),
            Quantized(Transposed(lattice.Data, lattice.Size, 1f), codes),
            3).ToString();

    private static Fin<LutTable> ParseCsp(string text) =>
        from scan in Scan.Of(Unmarked(text), [CspLut, Csp1D, Csp3D])
        let cube = scan.Keywords.ContainsKey(Csp3D)
        from _ in ImagingFault.Unless(scan.Keywords.ContainsKey(CspLut) && scan.Keywords.ContainsKey(Csp1D) != cube, ImagingReason.Malformed, CspLut)
        from shape in (cube
                ? scan.Row(9) is [var red, var green, var blue] && red == green && green == blue && float.IsInteger(red) ? Some((Edge: (int)red, Count: Cubed((int)red))) : None
                : scan.Row(9) is [var count] && float.IsInteger(count) ? Some((Edge: 0, Count: (long)count)) : None)
            .ToFin(ImagingFault.Of(ImagingReason.Malformed, scan.Row(9).Length, scan.Line(9)))
        from parts in (scan.Table(10, 3, shape.Count), Prelut(scan)).Apply(static (rows, prelut) => (Rows: rows, Prelut: prelut)).As()
        from body in cube ? Lattice(shape.Edge, parts.Rows, UnitDomain) : Curve(parts.Rows, UnitDomain)
        select LutTable.Of([.. parts.Prelut.ToSeq(), body]);

    private static Option<string> RenderCsp(LutTable table, string name) =>
        Parts(table).Map(parts => {
            StringBuilder text = new StringBuilder().Append(CultureInfo.InvariantCulture, $"{CspLut}\n{(parts.Lattice.IsSome ? Csp3D : Csp1D)}\n\n{MetadataBegin}\n{name}\n{MetadataEnd}\n\n");
            (Vector3 low, Vector3 high) = parts.Lattice.Match(static lattice => (lattice.DomainMinimum, lattice.DomainMaximum), static () => UnitDomain);
            for (int channel = 0; channel < 3; channel++) {
                (float[] inputs, float[] outputs) = (parts.Curve.Case, parts.Lattice.IsSome) switch {
                    (Lut1D shaper, true) => (Ramp(shaper.Length, shaper.DomainMinimum[channel], shaper.DomainMaximum[channel]), Normalized(shaper.Channel(channel), low[channel], high[channel])),
                    (Lut1D curve, false) => ([curve.DomainMinimum[channel], curve.DomainMaximum[channel]], [0f, 1f]),
                    _ => ([low[channel], high[channel]], [0f, 1f]),
                };
                text = Rows(Rows(text.Append(CultureInfo.InvariantCulture, $"{inputs.Length}\n"), inputs, inputs.Length), outputs, outputs.Length);
            }
            foreach (Lut3D lattice in parts.Lattice.ToSpan())
                text = Rows(text.Append(CultureInfo.InvariantCulture, $"\n{lattice.Size} {lattice.Size} {lattice.Size}\n"), lattice.Data, 3);
            foreach (Lut1D curve in parts.Lattice.IsNone ? parts.Curve.ToSpan() : [])
                text = Rows(text.Append(CultureInfo.InvariantCulture, $"\n{curve.Length}\n"), Interleaved(curve.Channel(0), curve.Channel(1), curve.Channel(2)), 3);
            return text.ToString();
        });

    private static Fin<Option<LutTable>> Prelut(Scan scan) =>
        Seq(0, 1, 2).Traverse(channel => (scan.Row(3 * channel) is [var count] && count >= 2f && scan.Row((3 * channel) + 1).Length == (int)count && scan.Row((3 * channel) + 2).Length == (int)count
                ? Some((Inputs: scan.Row((3 * channel) + 1).ToArray(), Outputs: scan.Row((3 * channel) + 2).ToArray()))
                : None).ToFin(ImagingFault.Of(ImagingReason.Malformed, channel, scan.Line(3 * channel)))).As()
            .Bind(static channels => Some(channels).Filter(static held => !held.ForAll(static channel => channel.Inputs.AsSpan().SequenceEqual(channel.Outputs))).Traverse(Splined).As());

    private static Fin<LutTable> Splined(Seq<(float[] Inputs, float[] Outputs)> channels) {
        LinearSpline[] splines = [.. channels.Map(static held => LinearSpline.InterpolateSorted([.. held.Inputs.Select(static value => (double)value)], [.. held.Outputs.Select(static value => (double)value)]))];
        return Sampled(new Vector3(channels[0].Inputs[0], channels[1].Inputs[0], channels[2].Inputs[0]), new Vector3(channels[0].Inputs[^1], channels[1].Inputs[^1], channels[2].Inputs[^1]), row => {
            foreach (ref Vector4 pixel in row)
                pixel = new Vector4((float)splines[0].Interpolate(pixel.X), (float)splines[1].Interpolate(pixel.Y), (float)splines[2].Interpolate(pixel.Z), pixel.W);
        }).Map(static curve => (LutTable)new LutTable.ChannelCurve(curve));
    }

    private static Option<(Option<Lut1D> Curve, Option<Lut3D> Lattice)> Parts(LutTable table) =>
        table.Steps switch {
            [LutTable.ChannelCurve { Inverted: false, HueAdjust: false, Table.HalfDomain: false } curve] => Some((Some(curve.Table), Option<Lut3D>.None)),
            [LutTable.Cube cube] => Some((Option<Lut1D>.None, Some(cube.Lattice))),
            [LutTable.ChannelCurve { Inverted: false, HueAdjust: false, Table.HalfDomain: false } curve, LutTable.Cube cube] => Some((Some(curve.Table), Some(cube.Lattice))),
            [var shaper and (LutTable.Log or LutTable.Exponent or LutTable.Range or LutTable.ChannelCurve { HueAdjust: false }), LutTable.Cube cube] => Shaper(shaper).Map(curve => (Some(curve), Some(cube.Lattice))),
            _ => None,
        };

    private static Option<Lut1D> Shaper(LutTable step) {
        Vector4[] bounds = [Vector4.Zero, Vector4.One];
        if (step.Inverse().ToOption().Case is not LutTable inverse)
            return None;
        inverse.Apply(bounds);
        return Uniform(bounds[0].AsVector3(), bounds[1].AsVector3()) && bounds[0].X < bounds[1].X ? Sampled(bounds[0].AsVector3(), bounds[1].AsVector3(), step.Apply).ToOption() : None;
    }

    private static Fin<Lut1D> Sampled(Vector3 low, Vector3 high, Action<Span<Vector4>> map) {
        Vector4[] entries = GC.AllocateUninitializedArray<Vector4>(Codes);
        for (int entry = 0; entry < entries.Length; entry++)
            entries[entry] = new Vector4(Vector3.Lerp(low, high, (float)entry / (Codes - 1)), 1f);
        map(entries);
        float[] rows = GC.AllocateUninitializedArray<float>(3 * Codes);
        MemoryMarshal.Cast<Vector4, float>(entries.AsSpan()).AsSpan2D(Codes, 4)[.., ..3].CopyTo(new Span2D<float>(rows, Codes, 3));
        return Lut1D.From(rows, low, high);
    }

    private static Fin<float[]> Placed(Scan scan, int size) {
        float[] samples = GC.AllocateUninitializedArray<float>(3 * (int)Cubed(size));
        bool[] filled = new bool[samples.Length / 3];
        Span2D<float> nodes = new(samples, filled.Length, 3);
        for (int row = 2; row < scan.Rows.Count; row++) {
            Vector3 index = new(scan.Row(row)[..3]);
            int node = (int)((((index.Z * size) + index.Y) * size) + index.X);
            if (Vector3.Truncate(index) != index || Vector3.AnyWhereAllBitsSet(Vector3.LessThan(index, Vector3.Zero) | Vector3.GreaterThan(index, new Vector3(size - 1))) || filled[node])
                return ImagingFault.Of(ImagingReason.Malformed, index, scan.Line(row));
            filled[node] = true;
            scan.Row(row)[3..].CopyTo(nodes.GetRowSpan(node));
        }
        return samples;
    }

    private static StringBuilder Indexed(StringBuilder text, Lut3D lattice) {
        (int edge, int node) = (lattice.Size, 0);
        ReadOnlySpan2D<float> samples = lattice.Data.AsSpan2D(edge * edge * edge, 3);
        for (int blue = 0; blue < edge; blue++)
            for (int green = 0; green < edge; green++)
                for (int red = 0; red < edge; red++, node++)
                    text = text.Append(CultureInfo.InvariantCulture, $"{red} {green} {blue} {Spaced(new Vector3(samples.GetRowSpan(node)))}\n");
        return text;
    }

    private static PixelFrame Filled(PixelFrame frame, Lut3D cube, Func<int, int, int, int, int> start) {
        Walk(frame.View.Span, cube.Data.ToArray(), cube.Size, start, read: false);
        return frame;
    }

    private static void Walk(Span2D<Vector4> image, Span<float> nodes, int edge, Func<int, int, int, int, int> start, bool read) {
        int runs = image.Width / edge;
        for (int row = 0; row < image.Height; row++)
            for (int run = 0; run < runs; run++) {
                Span2D<float> tile = MemoryMarshal.Cast<Vector4, float>(image.GetRowSpan(row).Slice(run * edge, edge)).AsSpan2D(edge, 4)[.., ..3];
                Span2D<float> entries = nodes.Slice(3 * start(row, run, runs, edge), 3 * edge).AsSpan2D(edge, 3);
                if (read)
                    tile.CopyTo(entries);
                else
                    entries.CopyTo(tile);
            }
    }

    private static Fin<LutTable> Volume(ReadOnlyMemory<byte> bytes) {
        const int header = 148;
        const uint sharedExponent = 67;
        ReadOnlySpan<byte> file = bytes.Span;
        (int height, int width, int depth, uint format) = file.Length < header || !file.StartsWith("DDS "u8) || !file[84..].StartsWith("DX10"u8)
            ? (0, 0, 0, 0u)
            : (BinaryPrimitives.ReadInt32LittleEndian(file[12..]), BinaryPrimitives.ReadInt32LittleEndian(file[16..]), BinaryPrimitives.ReadInt32LittleEndian(file[24..]), BinaryPrimitives.ReadUInt32LittleEndian(file[128..]));
        return ImagingFault.Unless(width > 0 && width == height && width == depth && format is sharedExponent && file.Length >= header + (sizeof(uint) * Cubed(width)), ImagingReason.Unsupported, (file.Length, width, height, depth, format))
            .Bind(_ => {
                float[] samples = GC.AllocateUninitializedArray<float>(3 * (int)Cubed(width));
                Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.AsSpan());
                ReadOnlySpan<byte> texels = bytes.Span[header..];
                for (int node = 0; node < nodes.Length; node++)
                    nodes[node] = BinaryPrimitives.ReadUInt32LittleEndian(texels[(sizeof(uint) * node)..]) switch {
                        var texel => new Vector3(BitHelper.ExtractRange(texel, 0, 9), BitHelper.ExtractRange(texel, 9, 9), BitHelper.ExtractRange(texel, 18, 9)) * float.ScaleB(1f, (int)BitHelper.ExtractRange(texel, 27, 5) - 24),
                    };
                return Lattice(width, samples, UnitDomain);
            });
    }

    private static bool Ramped(ReadOnlySpan<float> mesh) {
        bool ramped = mesh.Length > 3 && mesh[^1] >= mesh.Length - 1;
        for (int at = 0; ramped && at < mesh.Length; at++)
            ramped = float.Abs(mesh[at] - (at * mesh[^1] / (mesh.Length - 1))) <= 1f;
        return ramped;
    }

    private static string Unmarked(string text) =>
        text.IndexOf(MetadataBegin, StringComparison.OrdinalIgnoreCase) is var begin and >= 0 && text.IndexOf(MetadataEnd, begin, StringComparison.OrdinalIgnoreCase) is var end and >= 0
            ? string.Concat(text.AsSpan(0, begin), new string('\n', System.MemoryExtensions.Count(text.AsSpan(begin, end - begin), '\n')), text.AsSpan(end + MetadataEnd.Length))
            : text;

    private static float[] Ramp(int length, float low, float high) {
        float[] positions = GC.AllocateUninitializedArray<float>(length);
        for (int entry = 0; entry < length; entry++)
            positions[entry] = float.Lerp(low, high, (float)entry / (length - 1));
        return positions;
    }

    private static float[] Normalized(ReadOnlySpan<float> values, float low, float high) {
        float[] normalized = GC.AllocateUninitializedArray<float>(values.Length);
        TensorPrimitives.Subtract(values, low, normalized);
        TensorPrimitives.Divide(normalized, high - low, normalized);
        return normalized;
    }

    private static float[] Quantized(float[] values, Quantizer levels) {
        levels.Coded(values, values);
        return values;
    }

    private static long Cubed(int size) => (long)size * size * size;

    private static bool Uniform(Vector3 minimum, Vector3 maximum) => minimum == new Vector3(minimum.X) && maximum == new Vector3(maximum.X);

    private static StringBuilder Bounded(StringBuilder text, Option<string> range, Vector3 minimum, Vector3 maximum) =>
        (minimum, maximum) == UnitDomain ? text : range.Match(
            Some: keyword => text.Append(CultureInfo.InvariantCulture, $"{keyword} {minimum.X} {maximum.X}\n"),
            None: () => text.Append(CultureInfo.InvariantCulture, $"{DomainMin} {Spaced(minimum)}\n{DomainMax} {Spaced(maximum)}\n"));

    private static Fin<LutTable> Curve(ReadOnlyMemory<float> rows, (Vector3 Low, Vector3 High) domain, bool halfDomain = false, bool hueAdjust = false) =>
        Lut1D.From(rows, domain.Low, domain.High, halfDomain).Map(curve => (LutTable)new LutTable.ChannelCurve(curve, hueAdjust));

    private static Fin<LutTable> Lattice(int size, ReadOnlyMemory<float> rows, (Vector3 Low, Vector3 High) domain, LutInterpolation interpolation = LutInterpolation.Tetrahedral) =>
        Lut3D.From(size, rows, domain.Low, domain.High).Map(lattice => (LutTable)new LutTable.Cube(lattice, interpolation));

    private static bool Coded(Lut3D lattice) => (lattice.DomainMinimum, lattice.DomainMaximum) == UnitDomain && !lattice.Data.ContainsAnyExceptInRange(0f, 1f);

    private static Matrix4x4 Homogeneous(ReadOnlySpan<float> values, int columns) {
        Matrix4x4 rows = Matrix4x4.Identity;
        values.AsSpan2D(3, columns).CopyTo(MemoryMarshal.Cast<Matrix4x4, float>(new Span<Matrix4x4>(ref rows)).AsSpan2D(4, 4)[..3, ..columns]);
        return Matrix4x4.Transpose(rows);
    }

    private static float[] Cells(Matrix4x4 matrix, int columns) {
        Matrix4x4 rows = Matrix4x4.Transpose(matrix);
        float[] cells = new float[3 * columns];
        MemoryMarshal.Cast<Matrix4x4, float>(new Span<Matrix4x4>(ref rows)).AsSpan2D(4, 4)[..3, ..columns].CopyTo(new Span2D<float>(cells, 3, columns));
        return cells;
    }

    private static float[] Transposed(ReadOnlySpan<float> values, int edge, float scale) {
        float[] reordered = GC.AllocateUninitializedArray<float>(values.Length);
        ReadOnlySpan2D<float> source = values.AsSpan2D(values.Length / 3, 3);
        Span2D<float> target = new(reordered, values.Length / 3, 3);
        for (int blue = 0; blue < edge; blue++)
            for (int green = 0; green < edge; green++)
                for (int red = 0; red < edge; red++)
                    source.GetRowSpan((((red * edge) + green) * edge) + blue).CopyTo(target.GetRowSpan((((blue * edge) + green) * edge) + red));
        TensorPrimitives.Multiply(reordered, scale, reordered);
        return reordered;
    }

    private static float[] Interleaved(ReadOnlySpan<float> red, ReadOnlySpan<float> green, ReadOnlySpan<float> blue) {
        float[] rows = GC.AllocateUninitializedArray<float>(3 * red.Length);
        Span2D<float> grid = new(rows, red.Length, 3);
        red.CopyTo(grid.GetColumn(0));
        green.CopyTo(grid.GetColumn(1));
        blue.CopyTo(grid.GetColumn(2));
        return rows;
    }

    private static bool Appended<T>(ReadOnlySpan<char> text, List<T> values) where T : struct, INumberBase<T> {
        foreach (System.Range token in text.SplitAny(Blanks)) {
            if (text[token].IsEmpty)
                continue;
            if (!T.TryParse(text[token], NumberStyles.Float, CultureInfo.InvariantCulture, out T value))
                return false;
            values.Add(value);
        }
        return true;
    }

    private static Option<T[]> Numbers<T>(string text) where T : struct, INumberBase<T> => new List<T>() is var values && Appended(text, values) ? values.ToArray() : None;

    private static Option<T> Single<T>(string text) where T : struct, INumberBase<T> => Numbers<T>(text).Bind(static values => values is [var held] ? Some(held) : None);

    private static Option<Vector3> Triple(string text) => Numbers<float>(text).Bind(static values => values is [var r, var g, var b] ? Some(new Vector3(r, g, b)) : None);

    private static Option<(Vector3 Low, Vector3 High)> Interval(string text) => Numbers<float>(text).Bind(static values => values is [var low, var high] ? Some((new Vector3(low), new Vector3(high))) : None);

    private static string Spaced(Vector3 value) => string.Create(CultureInfo.InvariantCulture, $"{value.X} {value.Y} {value.Z}");

    private static StringBuilder Rows<T>(StringBuilder text, ReadOnlySpan<T> values, int width) where T : INumberBase<T> {
        for (int at = 0; at < values.Length; at++)
            text = text.Append(CultureInfo.InvariantCulture, $"{values[at]}{(at % width == width - 1 ? '\n' : ' ')}");
        return text;
    }

    private sealed record Scan(HashMap<string, (string Value, int Line)> Keywords, ImmutableArray<float> Values, Seq<(int Start, int End, int Line)> Rows) {
        public static Fin<Scan> Of(string text, Seq<string> keywords) {
            (HashMap<string, (string Value, int Line)> stated, List<float> values, List<(int Start, int End, int Line)> rows, int number) = ([], [], [], 0);
            foreach (ReadOnlySpan<char> raw in text.AsSpan().EnumerateLines()) {
                ReadOnlySpan<char> line = raw.Trim();
                (int start, number) = (values.Count, number + 1);
                if (line.IsEmpty || line[0] == '#')
                    continue;
                if (Appended(line, values)) {
                    rows.Add((start, values.Count, number));
                    continue;
                }
                values.RemoveRange(start, values.Count - start);
                int split = line.IndexOfAny(Blanks) is var at and >= 0 ? at : line.Length;
                string word = line[..split].ToString();
                if ((keywords.IsEmpty ? Some(word) : keywords.Find(known => string.Equals(known, word, StringComparison.OrdinalIgnoreCase))).Filter(key => !stated.ContainsKey(key)).Case is not string key)
                    return ImagingFault.Of(ImagingReason.Malformed, word, number);
                stated = stated.Add(key, (line[split..].Trim().ToString(), number));
            }
            return new Scan(stated, [.. values], toSeq(rows));
        }

        public ReadOnlySpan<float> Row(int row) => row < Rows.Count ? Values.AsSpan()[Rows[row].Start..Rows[row].End] : [];

        public Option<int> Line(int row) => Rows.At(row).Map(static held => held.Line);

        public Fin<Option<T>> Stated<T>(string keyword, Func<string, Option<T>> parse) =>
            Keywords.Find(keyword).Traverse(held => parse(held.Value).ToFin(ImagingFault.Of(ImagingReason.Malformed, keyword, held.Line))).As();

        public Fin<ReadOnlyMemory<float>> Table(int first, int width, long count) =>
            Rows.Count - first != count
                ? ImagingFault.Of(ImagingReason.Miscounted, (count, Rows.Count - first))
                : Rows.Skip(first).Find(row => row.End - row.Start != width).Match(
                    Some: row => Fin.Fail<ReadOnlyMemory<float>>(ImagingFault.Of(ImagingReason.Malformed, width, row.Line)),
                    None: () => Values.AsMemory()[(Values.Length - (int)(width * count))..]);
    }
}
