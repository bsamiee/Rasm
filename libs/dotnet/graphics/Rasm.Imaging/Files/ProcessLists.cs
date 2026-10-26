using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Quantization;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
internal sealed partial class ClfBitDepth {
    public static readonly ClfBitDepth Integer8 = new("8i", OutputDepth.UInt8);
    public static readonly ClfBitDepth Integer10 = new("10i", OutputDepth.UInt10);
    public static readonly ClfBitDepth Integer12 = new("12i", OutputDepth.UInt12);
    public static readonly ClfBitDepth Integer16 = new("16i", OutputDepth.UInt16);
    public static readonly ClfBitDepth Float16 = new("16f", OutputDepth.Float16);
    public static readonly ClfBitDepth Float32 = new("32f", OutputDepth.Float32);

    public OutputDepth Depth { get; }

    public float Scale => Depth.Levels.Match(Some: static levels => levels.MaxCode, None: 1f);
}

[SmartEnum<string>]
internal sealed partial class ClfInterpolation {
    public static readonly ClfInterpolation Trilinear = new("trilinear", LutInterpolation.Trilinear);
    public static readonly ClfInterpolation Tetrahedral = new("tetrahedral", LutInterpolation.Tetrahedral);

    public LutInterpolation Interpolation { get; }
}

[SmartEnum<string>]
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
internal sealed partial class ProcessNode {
    public static readonly ProcessNode Matrix = new("Matrix", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadMatrix);
    public static readonly ProcessNode Lut1D = new("LUT1D", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadLut1D);
    public static readonly ProcessNode Lut3D = new("LUT3D", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadLut3D);
    public static readonly ProcessNode Range = new("Range", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadRange);
    public static readonly ProcessNode Log = new("Log", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadLog);
    public static readonly ProcessNode Exponent = new("Exponent", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadExponent);
    public static readonly ProcessNode Gamma = new("Gamma", [LutFormat.Ctf], ProcessLists.ReadGamma);
    public static readonly ProcessNode AscCdl = new("ASC_CDL", [LutFormat.Clf, LutFormat.Ctf], ProcessLists.ReadCdl);
    public static readonly ProcessNode InverseLut1D = new("InverseLUT1D", [LutFormat.Ctf], ProcessLists.ReadInverseLut1D);

    public Seq<LutFormat> Formats { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<LutTable> Read(XElement node, LutFormat format);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ProcessLists {
    // --- [TOKENS]
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
    private const string ChannelLetters = "RGB";
    private const string ExponentParams = "ExponentParams";
    private const string ExponentValue = "exponent";
    private const string OffsetValue = "offset";
    private const string SopNode = "SOPNode";
    private const string SatNode = "SatNode";
    private const string Slope = "Slope";
    private const string Offset = "Offset";
    private const string Power = "Power";
    private const string Saturation = "Saturation";
    private static readonly Seq<string> Descriptive = ["Description", "InputDescriptor", "OutputDescriptor", "Info"];

    // --- [READS]
    internal static Fin<LutTable> ParseClf(string text) => Parse(LutFormat.Clf, text);

    internal static Fin<LutTable> ParseCtf(string text) => Parse(LutFormat.Ctf, text);

    private static Fin<LutTable> Parse(LutFormat format, string text) =>
        Try.lift(() => XDocument.Parse(text, LoadOptions.SetLineInfo)).Run()
            .MapFail(error => error.Exception.Case is XmlException xml ? LutText.Misstated(format, xml.LineNumber) : error)
            .Bind(document => document.Root is { Name.LocalName: ProcessList } root
                ? toSeq(root.Elements())
                    .Filter(static node => !Marked(node, "bypass", "true") && !Descriptive.Exists(name => string.Equals(name, node.Name.LocalName, StringComparison.Ordinal)))
                    .TraverseM(node => ProcessNode.TryGet(node.Name.LocalName, out ProcessNode? kind) && kind.Formats.Exists(held => held == format)
                        ? kind.Read(node, format)
                        : Unheld(node, format))
                    .As()
                    .Map(LutText.Chained)
                : LutText.Misstated(format, document.Root is { } other ? Line(other) : 1));

    internal static Fin<LutTable> ReadMatrix(XElement node, LutFormat format) =>
        from read in (Scales(node, format), Values<double>(node, format)).Apply(static (scales, array) => (Scales: scales, Array: array)).As()
        from table in (read.Array.Dimensions, read.Array.Values.Length) switch {
            ([4, ..], _) => Unheld(node, format),
            ([3, 3] or [3, 3, 3], 9) => Fin.Succ<LutTable>(LutText.Affine(read.Array.Values.AsSpan(0, 9), 3, read.Scales)),
            ([3, 4] or [3, 4, 3], 12) => LutText.Affine(read.Array.Values.AsSpan(0, 12), 4, read.Scales),
            _ => Malformed(node, format),
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
                ClfInterpolation.TryGet(Attribute(node, Interpolation).IfNone(ClfInterpolation.Trilinear.Key), out ClfInterpolation? interpolation) ? Fin.Succ(interpolation.Interpolation) : Malformed(node, format))
            .Apply(static (scales, array, interpolation) => (Scales: scales, Array: array, Interpolation: interpolation))
            .As()
        from size in (read.Array.Dimensions is [var red, var green, var blue, 3] && red == green && green == blue && read.Array.Values.Length == 3 * red * red * red
                ? Some(red) : None).ToFin(Malformed(node, format))
        from cube in LutText.LatticeTable(size, (Vector3.Zero, Vector3.One), read.Interpolation).Build(LutText.Transposed(read.Array.Values, size, 1f / read.Scales.Out))
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
            (RemapBound low, null) when clamps => new RemapBounds.Floor(low),
            (null, RemapBound high) when clamps => new RemapBounds.Ceiling(high),
            _ => Malformed(node, format),
        }
        select (LutTable)new LutTable.Remap(bounds);

    internal static Fin<LutTable> ReadLog(XElement node, LutFormat format) =>
        from style in Attribute(node, Style).Bind(static token => ClfLogStyle.TryGet(token, out ClfLogStyle? style) ? Some(style) : None).ToFin(Malformed(node, format))
        from curve in style.Base.Match(
            Some: static logBase => new LogCurve(logBase, Vector3.One, Vector3.Zero, Vector3.One, Vector3.Zero),
            None: () => Logarithm(node, format, style.Camera))
        select style.Table(curve);

    internal static Fin<LutTable> ReadExponent(XElement node, LutFormat format) => Exponential(node, format, ExponentParams, ExponentValue);

    internal static Fin<LutTable> ReadGamma(XElement node, LutFormat format) => Exponential(node, format, "GammaParams", "gamma");

    internal static Fin<LutTable> ReadCdl(XElement node, LutFormat format) =>
        from read in (
                Attribute(node, Style).Match(
                    Some: static token => (CdlStyle.TryGet(token, out CdlStyle? style) ? Some(style) : None) | toSeq(CdlStyle.Items).Find(item => string.Equals(item.Legacy, token, StringComparison.OrdinalIgnoreCase)),
                    None: static () => Some(CdlStyle.Fwd)).ToFin(Malformed(node, format)),
                CorrectionValues(node, _ => Malformed(node, format)))
            .Apply(static (style, values) => (Style: style, Values: values))
            .As()
        select (LutTable)new LutTable.Cdl(read.Style, read.Values.Slope, read.Values.Offset, read.Values.Power, read.Values.Saturation);

    internal static Fin<(Vector3 Slope, Vector3 Offset, Vector3 Power, float Saturation)> CorrectionValues(XElement correction, Func<string, Error> malformed) {
        Fin<T> Field<T>(string parent, string name, Func<string, Option<T>> parse, T absent) =>
            Child(correction, parent).Match(
                Some: held => Child(held, name).Bind(element => parse(element.Value)).ToFin(malformed(name)),
                None: () => absent);
        return (Field(SopNode, Slope, LutText.Triple, Vector3.One), Field(SopNode, Offset, LutText.Triple, Vector3.Zero), Field(SopNode, Power, LutText.Triple, Vector3.One), Field(SatNode, Saturation, LutText.Number, 1f))
            .Apply(static (slope, offset, power, saturation) => (Slope: slope, Offset: offset, Power: power, Saturation: saturation))
            .As()
            .Bind(values => Vector3.AllWhereAllBitsSet(Vector3.GreaterThanOrEqual(values.Slope, Vector3.Zero) & Vector3.GreaterThan(values.Power, Vector3.Zero)) && values.Saturation >= 0f
                ? Fin.Succ(values)
                : malformed(SopNode));
    }

    private static Fin<Lut1D> Curve(XElement node, LutFormat format, float scale) =>
        from array in (
                Marked(node, "rawHalfs", "true")
                    ? Values<ushort>(node, format).Map(static array => (array.Dimensions, Values: array.Values.Select(static bits => (float)BitConverter.UInt16BitsToHalf(bits)).ToArray()))
                    : Values<float>(node, format),
                guard<Error>(!Marked(node, "halfDomain", "true") && !Marked(node, "hueAdjust", "dw3"), Unheld(node, format)).ToFin())
            .Apply(static (array, _) => array)
            .As()
        from length in (array.Dimensions is [var entries, 1 or 3] && array.Values.Length == entries * array.Dimensions[1] ? LutText.Entries(entries) : None).ToFin(Malformed(node, format))
        from curve in Lut1D.From(length, array.Values.SelectMany(value => Enumerable.Repeat(value / scale, 3 / array.Dimensions[1])).ToArray(), Vector3.Zero, Vector3.One)
        select curve;

    private static Fin<LutTable> Indexed(XElement node, LutFormat format, float scale, int length, LutTable table) =>
        Child(node, "IndexMap").Match(
            Some: map =>
                from entries in map.Value.Split(LutText.Blanks.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) is [var first, var second]
                    ? (from low in IndexEntry(first) from high in IndexEntry(second) select (Low: low, High: high)).ToFin(Malformed(map, format))
                    : Unheld(map, format)
                select (LutTable)new LutTable.Sequence([
                    new LutTable.Remap(new RemapBounds.Between(
                        new RemapBound(entries.Low.Input / scale, entries.Low.Index / (length - 1)),
                        new RemapBound(entries.High.Input / scale, entries.High.Index / (length - 1)),
                        Clamps: true)),
                    table]),
            None: () => table);

    private static Option<(float Input, float Index)> IndexEntry(string entry) =>
        entry.Split('@') is [var input, var index] ? from value in LutText.Number(input) from at in LutText.Number(index) select (value, at) : None;

    private static Fin<Option<RemapBound>> Bound(XElement node, LutFormat format, string input, string output, (float In, float Out) scales) =>
        (Child(node, input).Map(static element => element.Value), Child(node, output).Map(static element => element.Value)) switch {
            var (inside, outside) when inside.IsNone && outside.IsNone => Option<RemapBound>.None,
            var (inside, outside) => (from value in inside.Bind(LutText.Number) from mapped in outside.Bind(LutText.Number) select Some(new RemapBound(value / scales.In, mapped / scales.Out)))
                .ToFin(Malformed(node, format)),
        };

    private static Fin<LogCurve> Logarithm(XElement node, LutFormat format, bool camera) =>
        from bases in Children(node, LogParams).TraverseM(element => Decimal(element, format, LogBase)).As()
        from logBase in bases.Somes().Distinct() switch {
            [] => Fin.Succ(2f),
            [> 0f and not 1f and var stated] => stated,
            _ => Malformed(node, format),
        }
        from channels in PerChannel(
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
                    read.Sides.X is not 0f && read.Sides.Z is not 0f && read.Knee.IsSome == camera && (camera || read.Slope.IsNone),
                    Malformed(element, format)).ToFin()
                select (read.Sides, Camera: read.Knee.Map(at => (Knee: at, Slope: read.Slope.IfNone(() => read.Sides.X * read.Sides.Z / (((read.Sides.Z * at) + read.Sides.W) * float.Log(logBase)))))),
            camera ? None : Some((Sides: new Vector4(1f, 0f, 1f, 0f), Camera: Option<(float Knee, float Slope)>.None)))
        select new LogCurve(
            logBase,
            Across(channels, static channel => channel.Sides.X),
            Across(channels, static channel => channel.Sides.Y),
            Across(channels, static channel => channel.Sides.Z),
            Across(channels, static channel => channel.Sides.W)) {
            Camera = channels.Traverse(static channel => channel.Camera).As().Map(static joints => new CameraSegment(Across(joints, static joint => joint.Knee), Across(joints, static joint => joint.Slope))),
        };

    private static Fin<LutTable> Exponential(XElement node, LutFormat format, string parameters, string exponent) =>
        from style in Attribute(node, Style).Bind(static token => ExponentStyle.TryGet(token, out ExponentStyle? style) ? Some(style) : None).ToFin(Malformed(node, format))
        from channels in PerChannel(
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
        from table in (Across(channels, static channel => channel.Power), Across(channels, static channel => channel.Offset)) switch {
            var (power, offset) when offset == Vector3.Zero => Fin.Succ<LutTable>(new LutTable.Exponent(style.Basic.IfNone(style), power, offset)),
            var (power, offset) when Vector3.AllWhereAllBitsSet(Vector3.GreaterThan(offset, Vector3.Zero) & Vector3.GreaterThan(power, Vector3.One)) => new LutTable.Exponent(style, power, offset),
            _ => Malformed(node, format),
        }
        select table;

    private static Fin<Seq<T>> PerChannel<T>(XElement node, LutFormat format, string name, Func<XElement, Fin<T>> read, Option<T> unstated) =>
        Children(node, name)
            .TraverseM(element =>
                from channels in Attribute(element, Channel).Match(
                    Some: static letter => letter.Length is 1 && ChannelLetters.IndexOf(letter, StringComparison.OrdinalIgnoreCase) is var channel and >= 0 ? Some(Seq(channel)) : None,
                    None: static () => Some(Seq(0, 1, 2))).ToFin(Malformed(element, format))
                from value in read(element)
                select (Channels: channels, Value: value))
            .As()
            .Bind(entries => Seq(0, 1, 2)
                .Traverse(channel => (entries.Filter(entry => entry.Channels.Exists(held => held == channel)).Last.Map(static entry => entry.Value) | unstated).ToFin(Malformed(node, format)))
                .As());

    private static Vector3 Across<T>(Seq<T> channels, Func<T, float> channel) => new([.. channels.Map(channel)]);

    private static Fin<(float In, float Out)> Scales(XElement node, LutFormat format) =>
        (Scale(node, format, InBitDepth), Scale(node, format, OutBitDepth)).Apply(static (input, output) => (In: input, Out: output)).As();

    private static Fin<float> Scale(XElement node, LutFormat format, string name) =>
        ClfBitDepth.TryGet(Attribute(node, name).IfNone(ClfBitDepth.Float32.Key), out ClfBitDepth? depth) ? depth.Scale : Malformed(node, format);

    private static Fin<Option<float>> Decimal(XElement element, LutFormat format, string name) =>
        Attribute(element, name).Traverse(text => LutText.Number(text).ToFin(Malformed(element, format))).As();

    private static Fin<(int[] Dimensions, T[] Values)> Values<T>(XElement node, LutFormat format) where T : struct, INumberBase<T> =>
        (from array in Child(node, ArrayElement)
         from dimensions in Attribute(array, Dimensions).Bind(static text => LutText.Numbers<int>(text))
         from values in LutText.Numbers<T>(array.Value)
         select (dimensions, values)).ToFin(Malformed(node, format));

    // --- [WRITES]
    internal static Fin<string> RenderClf(LutTable table, string name) => Render(LutFormat.Clf, table, name);

    internal static Fin<string> RenderCtf(LutTable table, string name) => Render(LutFormat.Ctf, table, name);

    private static Fin<string> Render(LutFormat format, LutTable table, string name) =>
        Nodes(format, table).Map(nodes => {
            StringBuilder builder = new("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            using (XmlWriter writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true, NewLineChars = "\n", OmitXmlDeclaration = true }))
                new XElement(ProcessList, new XAttribute("id", name), format == LutFormat.Clf ? new XAttribute("compCLFversion", "3") : new XAttribute("version", "2"), nodes)
                    .WriteTo(writer);
            return builder.Append('\n').ToString();
        }).ToFin(LutText.Unsupported(format));

    private static Option<Seq<XElement>> Nodes(LutFormat format, LutTable table) =>
        table.Switch(
            format,
            cube: static (_, cube) => toSeq(ClfInterpolation.Items).Find(item => item.Interpolation == cube.Interpolation).Map(interpolation =>
                Normalized(cube.Lattice.DomainMinimum, cube.Lattice.DomainMaximum).Add(Node(
                    ProcessNode.Lut3D,
                    new XAttribute(Interpolation, interpolation.Key),
                    Grid([cube.Lattice.Size, cube.Lattice.Size, cube.Lattice.Size, 3], LutText.Transposed(cube.Lattice.Data, cube.Lattice.Size, 1f))))),
            channelCurve: static (_, curve) => Normalized(curve.Table.DomainMinimum, curve.Table.DomainMaximum)
                .Add(Node(ProcessNode.Lut1D, Grid([curve.Table.Length, 3], LutText.Interleaved(curve.Table)))),
            inverseChannelCurve: static (format, inverse) => ProcessNode.InverseLut1D.Formats.Exists(held => held == format)
                ? Seq(Node(ProcessNode.InverseLut1D, Grid([inverse.Table.Forward.Length, 3], LutText.Interleaved(inverse.Table.Forward))))
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
                Parameters(ExponentParams, channel => exponent.Style.Basic.IsSome
                    ? Seq((ExponentValue, exponent.Power[channel]), (OffsetValue, exponent.Offset[channel]))
                    : Seq((ExponentValue, exponent.Power[channel]))))),
            cdl: static (_, cdl) => Seq(Node(ProcessNode.AscCdl, new XAttribute(Style, cdl.Style.Key), CorrectionNodes(XNamespace.None, cdl))),
            sequence: static (format, sequence) => sequence.Steps.TraverseM(step => Nodes(format, step)).As().Map(static nodes => nodes.Flatten()));

    internal static XElement[] CorrectionNodes(XNamespace space, LutTable.Cdl cdl) => [
        new(space + SopNode, new XElement(space + Slope, LutText.Spaced(cdl.Slope)), new XElement(space + Offset, LutText.Spaced(cdl.Offset)), new XElement(space + Power, LutText.Spaced(cdl.Power))),
        new(space + SatNode, new XElement(space + Saturation, cdl.Saturation.ToString(CultureInfo.InvariantCulture))),
    ];

    private static XElement Node(ProcessNode kind, params ReadOnlySpan<object> content) =>
        new(kind.Key, new XAttribute(InBitDepth, ClfBitDepth.Float32.Key), new XAttribute(OutBitDepth, ClfBitDepth.Float32.Key), content.ToArray());

    private static XElement Matrix(Matrix<double> matrix, (double X, double Y, double Z) offset) =>
        offset == default
            ? Node(ProcessNode.Matrix, Grid([3, 3], matrix.ToRowMajorArray()))
            : Node(ProcessNode.Matrix, Grid([3, 4], [matrix[0, 0], matrix[0, 1], matrix[0, 2], offset.X, matrix[1, 0], matrix[1, 1], matrix[1, 2], offset.Y, matrix[2, 0], matrix[2, 1], matrix[2, 2], offset.Z]));

    private static Seq<XElement> Normalized(Vector3 minimum, Vector3 maximum) =>
        minimum == Vector3.Zero && maximum == Vector3.One ? [] : [Diagonal(Vector3.One / (maximum - minimum), -minimum / (maximum - minimum))];

    private static Seq<XElement> Restored(Vector3 minimum, Vector3 maximum) =>
        minimum == Vector3.Zero && maximum == Vector3.One ? [] : [Diagonal(maximum - minimum, minimum)];

    private static XElement Diagonal(Vector3 scale, Vector3 offset) =>
        Matrix(Matrix<double>.Build.DenseOfDiagonalArray([scale.X, scale.Y, scale.Z]), (offset.X, offset.Y, offset.Z));

    private static XElement Grid<T>(ReadOnlySpan<int> dimensions, ReadOnlySpan<T> values) where T : struct, INumberBase<T> =>
        new(ArrayElement, new XAttribute(Dimensions, string.Join(' ', dimensions.ToArray())), LutText.Rows(new StringBuilder().Append('\n'), values, dimensions[^1]).ToString());

    private static XElement[] Bounded(string input, string output, RemapBound bound) =>
        [new(input, bound.In.ToString(CultureInfo.InvariantCulture)), new(output, bound.Out.ToString(CultureInfo.InvariantCulture))];

    private static XElement Logged(LogCurve curve, ClfLogStyle style) =>
        Node(
            ProcessNode.Log,
            new XAttribute(Style, style.Key),
            Parameters(LogParams, channel =>
                Seq((LogBase, curve.Base), (LogSideSlope, curve.LogSlope[channel]), (LogSideOffset, curve.LogOffset[channel]), (LinSideSlope, curve.LinSlope[channel]), (LinSideOffset, curve.LinOffset[channel]))
                + curve.Camera.ToSeq().Bind(camera => Seq((LinSideBreak, camera.Break[channel]), (LinearSlope, camera.Slope[channel])))));

    private static Seq<XElement> Parameters(string element, Func<int, Seq<(string Name, float Value)>> channel) =>
        Seq(0, 1, 2).Map(channel).Strict() switch {
            var channels when channels.Distinct().Count is 1 => channels.Take(1).Map(held => new XElement(element, Attributes(held))),
            var channels => channels.Map((held, at) => new XElement(element, new XAttribute(Channel, ChannelLetters[at].ToString()), Attributes(held))),
        };

    private static Seq<XAttribute> Attributes(Seq<(string Name, float Value)> held) =>
        held.Map(static attribute => new XAttribute(attribute.Name, attribute.Value.ToString(CultureInfo.InvariantCulture)));

    // --- [NODES]
    private static Option<XElement> Child(XElement node, string name) => Children(node, name).Head;

    private static Seq<XElement> Children(XElement node, string name) =>
        toSeq(node.Elements()).Filter(element => string.Equals(element.Name.LocalName, name, StringComparison.Ordinal));

    private static Option<string> Attribute(XElement node, string name) => Optional(node.Attribute(name)?.Value);

    private static bool Marked(XElement node, string name, string token) => Attribute(node, name).Exists(held => string.Equals(held, token, StringComparison.OrdinalIgnoreCase));

    private static int Line(XElement node) => ((IXmlLineInfo)node).LineNumber;

    // --- [FAULTS]
    private static ImagingFault Malformed(XElement node, LutFormat format) => LutText.Misstated(format, Line(node));

    private static ImagingFault Unheld(XElement node, LutFormat format) =>
        new(string.Create(CultureInfo.InvariantCulture, $"{format} LUT line {Line(node)} node {node.Name.LocalName}"), ImagingReason.Unsupported);
}
