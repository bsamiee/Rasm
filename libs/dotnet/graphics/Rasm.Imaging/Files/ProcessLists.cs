using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Color;
using Rasm.Imaging.Quantization;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
internal sealed partial class ClfLogStyle {
    public static readonly ClfLogStyle Log10 = new("log10", Some(10f), camera: false, inverted: false);
    public static readonly ClfLogStyle AntiLog10 = new("antiLog10", Some(10f), camera: false, inverted: true);
    public static readonly ClfLogStyle Log2 = new("log2", Some(2f), camera: false, inverted: false);
    public static readonly ClfLogStyle AntiLog2 = new("antiLog2", Some(2f), camera: false, inverted: true);
    public static readonly ClfLogStyle LinToLog = new("linToLog", None, camera: false, inverted: false);
    public static readonly ClfLogStyle LogToLin = new("logToLin", None, camera: false, inverted: true);
    public static readonly ClfLogStyle CameraLinToLog = new("cameraLinToLog", None, camera: true, inverted: false);
    public static readonly ClfLogStyle CameraLogToLin = new("cameraLogToLin", None, camera: true, inverted: true);

    public Option<float> Base { get; }
    public bool Camera { get; }
    public bool Inverted { get; }
}

public sealed partial class LutFormat {
    private const string ProcessListNode = "ProcessList";
    private const string MatrixNode = "Matrix";
    private const string Lut1DNode = "LUT1D";
    private const string Lut3DNode = "LUT3D";
    private const string RangeNode = "Range";
    private const string LogNode = "Log";
    private const string ExponentNode = "Exponent";
    private const string GammaNode = "Gamma";
    private const string CdlNode = "ASC_CDL";
    private const string InverseLut1DNode = "InverseLUT1D";
    private const string ArrayNode = "Array";
    private const string Dimensions = "dim";
    private const string Identifier = "id";
    private const string InBitDepth = "inBitDepth";
    private const string OutBitDepth = "outBitDepth";
    private const string Style = "style";
    private const string Interpolation = "interpolation";
    private const string HalfDomain = "halfDomain";
    private const string HueAdjust = "hueAdjust";
    private const string HueRestore = "dw3";
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
    private const string ExponentParams = "ExponentParams";
    private const string ExponentValue = "exponent";
    private const string OffsetValue = "offset";
    private const string MonCurve = "monCurve";
    private const string Channel = "channel";
    private const string ChannelLetters = "RGB";
    private const string CorrectionNode = "ColorCorrection";
    private const string SopNode = "SOPNode";
    private const string SatNode = "SatNode";
    private static readonly XNamespace Asc = "urn:ASC:CDL:v1.01";
    private static readonly FrozenSet<string> Descriptive = FrozenSet.Create(StringComparer.Ordinal, ["Description", "InputDescriptor", "OutputDescriptor", "Info"]);
    private static readonly FrozenSet<string> CtfNodes = FrozenSet.Create(StringComparer.Ordinal, [GammaNode, InverseLut1DNode]);
    private static readonly FrozenDictionary<string, Func<XElement, Fin<LutTable>>> Steps = new Dictionary<string, Func<XElement, Fin<LutTable>>>(StringComparer.Ordinal) {
        [MatrixNode] = ReadMatrix, [Lut1DNode] = static node => ReadCurve(node, inverted: false), [InverseLut1DNode] = static node => ReadCurve(node, inverted: true),
        [Lut3DNode] = ReadLattice, [RangeNode] = ReadRange, [LogNode] = ReadLog, [ExponentNode] = static node => ReadExponent(node, ExponentParams, ExponentValue),
        [GammaNode] = static node => ReadExponent(node, "GammaParams", "gamma"), [CdlNode] = ReadCdl,
    }.ToFrozenDictionary(StringComparer.Ordinal);
    private static readonly string Float = Token(OutputDepth.Float32);
    private static readonly FrozenDictionary<string, float> Scales =
        OutputDepth.Items.ToFrozenDictionary(Token, static depth => depth.Levels.Match(Some: static levels => levels.MaxCode, None: 1f), StringComparer.Ordinal);

    public static Fin<Seq<(Option<string> Id, LutTable.Cdl Correction)>> Corrections(ReadOnlyMemory<byte> bytes) => Loaded(bytes).Bind(static document => Corrections(document));

    private static Fin<Seq<(Option<string> Id, LutTable.Cdl Correction)>> Corrections(XDocument document) =>
        from corrections in toSeq(document.Descendants()).Filter(static element => element.Name.LocalName is CorrectionNode)
            .Traverse(static correction => CorrectionValues(correction).Map(values =>
                (Optional(correction.Attribute(Identifier)?.Value), new LutTable.Cdl(CdlStyle.FwdNoClamp, values.Slope, values.Offset, values.Power, values.Saturation))))
            .As()
        from _ in ImagingFault.Unless(!corrections.IsEmpty, ImagingReason.Missing, CorrectionNode)
        select corrections;

    private static LutFormat ProcessList(string key, bool ctf) =>
        Xml(key,
            document =>
                from root in document.Root is { Name.LocalName: ProcessListNode } held ? Fin.Succ(held) : ImagingFault.Of(ImagingReason.Malformed, key, document.Root is { } other ? Line(other) : 1)
                from steps in toSeq(root.Elements())
                    .Filter(static node => !Flag(node, "bypass") && !Descriptive.Contains(node.Name.LocalName))
                    .Traverse(node => Steps.TryGetValue(node.Name.LocalName, out Func<XElement, Fin<LutTable>>? read) && (ctf || !CtfNodes.Contains(node.Name.LocalName))
                        ? read(node)
                        : ImagingFault.Of(ImagingReason.Unsupported, node.Name.LocalName, Line(node)))
                    .As()
                select LutTable.Of(steps),
            (table, name) => table.Steps.TraverseM(step => Nodes(step, ctf)).As().Map(nodes =>
                new XElement(ProcessListNode, new XAttribute(Identifier, name), ctf ? new XAttribute("version", "2") : new XAttribute("compCLFversion", "3"), nodes.Flatten())));

    private static LutFormat Correction(string key, Seq<string> ancestors) =>
        Xml(key,
            static document => Corrections(document).Map(static corrections => (LutTable)corrections[0].Correction),
            (table, name) => table.Steps is [LutTable.Cdl cdl] && (cdl.Style == CdlStyle.Fwd || cdl.Style == CdlStyle.FwdNoClamp)
                ? Some(ancestors.FoldBack(new XElement(Asc + CorrectionNode, new XAttribute(Identifier, name), CorrectionNodes(Asc, cdl)), static (inner, ancestor) => new XElement(Asc + ancestor, inner)))
                : None);

    private static LutFormat Xml(string key, Func<XDocument, Fin<LutTable>> read, Func<LutTable, string, Option<XElement>> write) =>
        new(key, $".{key}", bytes => Loaded(bytes).Bind(read), (table, name, _) => Written(write(table, name), key, static (root, stream) => IO.liftAsync(async env => {
            XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), Indent = true, NewLineChars = "\n", Async = true });
            await using (writer.ConfigureAwait(false))
                await root.SaveAsync(writer, env.Token).ConfigureAwait(false);
            return unit;
        })));

    private static Fin<XDocument> Loaded(ReadOnlyMemory<byte> bytes) =>
        Try.lift(() => XDocument.Load(bytes.AsStream(), LoadOptions.SetLineInfo)).Run()
            .MapFail(static error => error.Exception.Case is XmlException xml ? ImagingFault.Of(ImagingReason.Malformed, xml.Message, xml.LineNumber, error) : error);

    private static Fin<LutTable> ReadMatrix(XElement node) =>
        from read in (Depths(node), Values<float>(node)).Apply(static (scales, array) => (Scales: scales, Array: array)).As()
        from columns in (read.Array.Dimensions, read.Array.Values.Length) switch {
            ([3, 3] or [3, 3, 3], 9) => Fin.Succ(3),
            ([3, 4] or [3, 4, 3], 12) => 4,
            _ => Malformed(node),
        }
        select (LutTable)new LutTable.Affine(Matrix4x4.CreateScale(read.Scales.In) * Homogeneous(read.Array.Values, columns) * Matrix4x4.CreateScale(1f / read.Scales.Out));

    private static Fin<LutTable> ReadCurve(XElement node, bool inverted) =>
        from scales in Depths(node)
        from array in Flag(node, "rawHalfs") ? Values<ushort>(node).Map(static array => (array.Dimensions, Values: Halves(array.Values))) : Values<float>(node)
        from width in (array.Dimensions is [var entries, 1 or 3] && array.Values.Length == entries * array.Dimensions[1] ? Some(array.Dimensions[1]) : None).ToFin(Malformed(node))
        from _ in ImagingFault.Unless(Attribute(node, HueAdjust).ForAll(static held => held is HueRestore), ImagingReason.Unsupported, node.Name.LocalName, Line(node))
        from curve in Curve(Divided(width is 3 ? array.Values : Interleaved(array.Values, array.Values, array.Values), scales.Out), UnitDomain, Flag(node, HalfDomain), Attribute(node, HueAdjust).IsSome)
        from table in inverted ? curve.Inverse() : curve
        from indexed in Indexed(node, scales.In, array.Values.Length / width, table)
        select indexed;

    private static Fin<LutTable> ReadLattice(XElement node) =>
        from read in (
                Depths(node),
                Values<float>(node),
                Enum.TryParse(Attribute(node, Interpolation).IfNone(nameof(LutInterpolation.Trilinear)), ignoreCase: true, out LutInterpolation interpolation) ? Fin.Succ(interpolation) : Malformed(node))
            .Apply(static (scales, array, interpolation) => (Scales: scales, Array: array, Interpolation: interpolation))
            .As()
        from size in (read.Array.Dimensions is [var red, var green, var blue, 3] && red == green && green == blue && read.Array.Values.Length == 3 * red * red * red ? Some(red) : None).ToFin(Malformed(node))
        from cube in Lattice(size, Transposed(read.Array.Values, size, 1f / read.Scales.Out), UnitDomain, read.Interpolation)
        from table in Indexed(node, read.Scales.In, size, cube)
        select table;

    private static Fin<LutTable> ReadRange(XElement node) =>
        from scales in Depths(node)
        from stated in (Bound(node, MinIn, MinOut, scales), Bound(node, MaxIn, MaxOut, scales)).Apply(static (minimum, maximum) => (Minimum: minimum, Maximum: maximum)).As()
        let style = Attribute(node, Style)
        let clamps = !style.Exists(static held => held is NoClamp)
        let bounded = (from low in stated.Minimum from high in stated.Maximum select low.In != high.In).IfNone(clamps && stated.Minimum.IsSome != stated.Maximum.IsSome)
        from _ in ImagingFault.Unless(style.ForAll(static held => held is Clamp or NoClamp) && bounded, ImagingReason.Malformed, RangeNode, Line(node))
        select (LutTable)new LutTable.Range(stated.Minimum, stated.Maximum, clamps);

    private static Fin<LutTable> ReadLog(XElement node) =>
        from style in Attribute(node, Style).Bind(static token => ClfLogStyle.TryGet(token, out ClfLogStyle? style) ? Some(style) : None).ToFin(Malformed(node))
        from curve in style.Base.Match(Some: static logBase => new LogCurve(logBase, Vector3.One, Vector3.Zero, Vector3.One, Vector3.Zero), None: () => Logarithm(node, style.Camera))
        select (LutTable)new LutTable.Log(curve, style.Inverted);

    private static Fin<LutTable> ReadExponent(XElement node, string parameters, string exponent) =>
        from style in Attribute(node, Style).Bind(static token => ExponentStyle.TryGet(token, out ExponentStyle? style) ? Some(style) : None).ToFin(Malformed(node))
        let monCurve = style.Key.StartsWith(MonCurve, StringComparison.Ordinal)
        from channels in ChannelParameters(node, parameters)
        from _ in ImagingFault.Unless(
            channels.ForAll(held => held.IsEmpty || (held.Find(exponent).Case, held.Find(OffsetValue).Case) switch {
                (float power, float offset) => monCurve && power is >= 1f and <= 10f && offset is >= 0f and <= 0.9f && (offset is 0f || power > 1f),
                (float power, null) => !monCurve && power > 0f,
                _ => false,
            }),
            ImagingReason.Range, parameters, Line(node))
        select (LutTable)new LutTable.Exponent(style, Across(channels, held => held.Find(exponent).IfNone(1f)), Across(channels, static held => held.Find(OffsetValue).IfNone(0f)));

    private static Fin<LutTable> ReadCdl(XElement node) =>
        from read in (
                Attribute(node, Style).Match(
                    Some: static token => (CdlStyle.TryGet(token, out CdlStyle? style) ? Some(style) : None) | toSeq(CdlStyle.Items).Find(item => string.Equals(item.Legacy, token, StringComparison.OrdinalIgnoreCase)),
                    None: static () => Some(CdlStyle.Fwd)).ToFin(Malformed(node)),
                CorrectionValues(node))
            .Apply(static (style, values) => (Style: style, Values: values))
            .As()
        select (LutTable)new LutTable.Cdl(read.Style, read.Values.Slope, read.Values.Offset, read.Values.Power, read.Values.Saturation);

    private static Fin<(Vector3 Slope, Vector3 Offset, Vector3 Power, float Saturation)> CorrectionValues(XElement correction) =>
        from values in (
                Field(correction, SopNode, nameof(LutTable.Cdl.Slope), Triple, Vector3.One),
                Field(correction, SopNode, nameof(LutTable.Cdl.Offset), Triple, Vector3.Zero),
                Field(correction, SopNode, nameof(LutTable.Cdl.Power), Triple, Vector3.One),
                Field(correction, SatNode, nameof(LutTable.Cdl.Saturation), Single<float>, 1f))
            .Apply(static (slope, offset, power, saturation) => (Slope: slope, Offset: offset, Power: power, Saturation: saturation))
            .As()
        from _ in ImagingFault.Unless(
            Vector3.AllWhereAllBitsSet(Vector3.GreaterThanOrEqual(values.Slope, Vector3.Zero) & Vector3.GreaterThan(values.Power, Vector3.Zero)) && values.Saturation >= 0f,
            ImagingReason.Range, values, Line(correction))
        select values;

    private static Fin<T> Field<T>(XElement correction, string parent, string name, Func<string, Option<T>> parse, T absent) =>
        Child(correction, parent).Match(
            Some: held => Child(held, name).Match(Some: element => parse(element.Value).ToFin(Malformed(element)), None: () => Malformed(held)),
            None: () => absent);

    private static Fin<LutTable> Indexed(XElement node, float scale, int length, LutTable table) =>
        Child(node, "IndexMap").Match(
            Some: map =>
                from entries in (map.Value.Split(Blanks.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) is [var first, var second]
                    ? from low in IndexEntry(first) from high in IndexEntry(second) select (Low: low, High: high)
                    : None).ToFin(Malformed(map))
                select LutTable.Of([new LutTable.Range((entries.Low.Input / scale, entries.Low.Index / (length - 1)), (entries.High.Input / scale, entries.High.Index / (length - 1))), table]),
            None: () => table);

    private static Option<(float Input, float Index)> IndexEntry(string entry) =>
        entry.Split('@') is [var input, var index] ? from value in Single<float>(input) from at in Single<float>(index) select (value, at) : None;

    private static Fin<Option<(float In, float Out)>> Bound(XElement node, string input, string output, (float In, float Out) scales) =>
        (Child(node, input), Child(node, output)) switch {
            var (inside, outside) when inside.IsNone && outside.IsNone => Option<(float In, float Out)>.None,
            var (inside, outside) => (from value in inside.Bind(static element => Single<float>(element.Value)) from mapped in outside.Bind(static element => Single<float>(element.Value))
                                      select Some((value / scales.In, mapped / scales.Out))).ToFin(Malformed(node)),
        };

    private static Fin<LogCurve> Logarithm(XElement node, bool camera) =>
        from channels in ChannelParameters(node, LogParams)
        from logBase in channels.Map(static held => held.Find(LogBase)).Somes().Distinct() switch {
            [] => Fin.Succ(2f),
            [> 0f and not 1f and var stated] => stated,
            _ => Malformed(node),
        }
        let curve = new LogCurve(logBase, Across(channels, static held => held.Find(LogSideSlope).IfNone(1f)), Across(channels, static held => held.Find(LogSideOffset).IfNone(0f)),
            Across(channels, static held => held.Find(LinSideSlope).IfNone(1f)), Across(channels, static held => held.Find(LinSideOffset).IfNone(0f)))
        from _ in ImagingFault.Unless(
            !Vector3.AnyWhereAllBitsSet(Vector3.Equals(curve.LogSideSlope, Vector3.Zero) | Vector3.Equals(curve.LinSideSlope, Vector3.Zero)) && channels.ForAll(held => held.ContainsKey(LinSideBreak) == camera && (camera || !held.ContainsKey(LinearSlope))),
            ImagingReason.Malformed, LogParams, Line(node))
        select camera
            ? LogCurve.Of(curve.Base, curve.LogSideSlope, curve.LogSideOffset, curve.LinSideSlope, curve.LinSideOffset, Across(channels, static held => held.Find(LinSideBreak).IfNone(0f))) switch {
                var defaulted => defaulted with { Camera = defaulted.Camera.Map(joint => (joint.LinSideBreak, new Vector3([.. channels.Map((held, at) => held.Find(LinearSlope).IfNone(joint.LinearSlope[at]))]))) },
            }
            : curve;

    private static Fin<Seq<HashMap<string, float>>> ChannelParameters(XElement node, string name) =>
        Children(node, name)
            .Traverse(static element =>
                from channels in Attribute(element, Channel).Match(
                    Some: static letter => letter.Length is 1 && ChannelLetters.IndexOf(letter, StringComparison.OrdinalIgnoreCase) is var channel and >= 0 ? Some(Seq(channel)) : None,
                    None: static () => Some(Seq(0, 1, 2))).ToFin(Malformed(element))
                from values in toSeq(element.Attributes()).Filter(static attribute => attribute.Name.LocalName is not Channel)
                    .Traverse(attribute => Single<float>(attribute.Value).Map(value => (attribute.Name.LocalName, value)).ToFin(Malformed(element))).As()
                select (Channels: channels, Values: toHashMap(values)))
            .As()
            .Map(static entries => Seq(0, 1, 2).Map(channel => entries.Filter(entry => entry.Channels.Exists(held => held == channel)).Last.Map(static entry => entry.Values).IfNone(HashMap<string, float>())).Strict());

    private static Vector3 Across<T>(Seq<T> channels, Func<T, float> channel) => new([.. channels.Map(channel)]);

    private static Fin<(float In, float Out)> Depths(XElement node) =>
        (Depth(node, InBitDepth), Depth(node, OutBitDepth)).Apply(static (input, output) => (In: input, Out: output)).As();

    private static Fin<float> Depth(XElement node, string name) =>
        Scales.TryGetValue(Attribute(node, name).IfNone(Float), out float scale) ? scale : Malformed(node);

    private static Fin<(int[] Dimensions, T[] Values)> Values<T>(XElement node) where T : struct, INumberBase<T> =>
        (from array in Child(node, ArrayNode)
         from dimensions in Attribute(array, Dimensions).Bind(static text => Numbers<int>(text))
         from values in Numbers<T>(array.Value)
         select (dimensions, values)).ToFin(Malformed(node));

    private static string Token(OutputDepth depth) => string.Create(CultureInfo.InvariantCulture, $"{depth.Bits}{(depth.Levels.IsSome ? 'i' : 'f')}");

    private static float[] Divided(float[] values, float divisor) {
        TensorPrimitives.Divide(values, divisor, values);
        return values;
    }

    private static float[] Halves(ushort[] bits) {
        float[] values = GC.AllocateUninitializedArray<float>(bits.Length);
        TensorPrimitives.ConvertToSingle(MemoryMarshal.Cast<ushort, Half>(bits.AsSpan()), values);
        return values;
    }

    private static Option<Seq<XElement>> Nodes(LutTable step, bool ctf) =>
        step.Switch(
            ctf,
            cube: static (_, cube) => Some(Normalized(cube.Lattice.DomainMinimum, cube.Lattice.DomainMaximum).Add(Node(
                Lut3DNode,
                new XAttribute(Interpolation, JsonNamingPolicy.CamelCase.ConvertName(cube.Interpolation.ToString())),
                Grid<float>([cube.Lattice.Size, cube.Lattice.Size, cube.Lattice.Size, 3], Transposed(cube.Lattice.Data, cube.Lattice.Size, 1f))))),
            channelCurve: static (ctf, curve) => (curve.Inverted, ctf) switch {
                (false, _) => Some((curve.Table.HalfDomain ? [] : Normalized(curve.Table.DomainMinimum, curve.Table.DomainMaximum)).Add(Curved(Lut1DNode, curve))),
                (true, true) => Some(Seq(Curved(InverseLut1DNode, curve)) + (curve.Table.HalfDomain ? [] : Restored(curve.Table.DomainMinimum, curve.Table.DomainMaximum))),
                _ => None,
            },
            affine: static (_, affine) => Some(Seq(Matrix(affine.Matrix))),
            range: static (_, range) => Some(Seq(Node(
                RangeNode,
                new XAttribute(Style, range.Clamps ? Clamp : NoClamp),
                range.Minimum.ToSeq().Bind(static bound => Seq(new XElement(MinIn, Invariant(bound.In)), new XElement(MinOut, Invariant(bound.Out)))),
                range.Maximum.ToSeq().Bind(static bound => Seq(new XElement(MaxIn, Invariant(bound.In)), new XElement(MaxOut, Invariant(bound.Out))))))),
            log: static (_, log) => Some(Seq(Node(
                LogNode,
                new XAttribute(Style, toSeq(ClfLogStyle.Items).Find(style => style.Base.IsNone && style.Camera == log.Curve.Camera.IsSome && style.Inverted == log.Inverted).Map(static style => style.Key).IfNone(ClfLogStyle.LinToLog.Key)),
                Parameters(LogParams, channel =>
                    Seq((LogBase, log.Curve.Base), (LogSideSlope, log.Curve.LogSideSlope[channel]), (LogSideOffset, log.Curve.LogSideOffset[channel]), (LinSideSlope, log.Curve.LinSideSlope[channel]), (LinSideOffset, log.Curve.LinSideOffset[channel]))
                    + log.Curve.Camera.ToSeq().Bind(camera => Seq((LinSideBreak, camera.LinSideBreak[channel]), (LinearSlope, camera.LinearSlope[channel]))))))),
            exponent: static (_, exponent) => Some(Seq(Node(
                ExponentNode,
                new XAttribute(Style, exponent.Style.Key),
                Parameters(ExponentParams, channel => exponent.Style.Key.StartsWith(MonCurve, StringComparison.Ordinal)
                    ? Seq((ExponentValue, exponent.Power[channel]), (OffsetValue, exponent.Offset[channel]))
                    : Seq((ExponentValue, exponent.Power[channel])))))),
            cdl: static (_, cdl) => Some(Seq(Node(CdlNode, new XAttribute(Style, cdl.Style.Key), CorrectionNodes(XNamespace.None, cdl)))),
            fixedFunction: static (_, _) => Option<Seq<XElement>>.None,
            sequence: static (ctf, sequence) => sequence.Steps.TraverseM(step => Nodes(step, ctf)).As().Map(static nodes => nodes.Flatten()));

    private static XElement[] CorrectionNodes(XNamespace space, LutTable.Cdl cdl) => [
        new(space + SopNode, new XElement(space + nameof(cdl.Slope), Spaced(cdl.Slope)), new XElement(space + nameof(cdl.Offset), Spaced(cdl.Offset)), new XElement(space + nameof(cdl.Power), Spaced(cdl.Power))),
        new(space + SatNode, new XElement(space + nameof(cdl.Saturation), Invariant(cdl.Saturation))),
    ];

    private static XElement Node(string name, params ReadOnlySpan<object?> content) =>
        new(name, new XAttribute(InBitDepth, Float), new XAttribute(OutBitDepth, Float), content.ToArray());

    private static XElement Curved(string name, LutTable.ChannelCurve curve) =>
        Node(
            name,
            curve.Table.HalfDomain ? new XAttribute(HalfDomain, XmlConvert.ToString(value: true)) : null,
            curve.HueAdjust ? new XAttribute(HueAdjust, HueRestore) : null,
            Grid([curve.Table.Length, 3], Interleaved(curve.Table.Channel(0), curve.Table.Channel(1), curve.Table.Channel(2))));

    private static XElement Matrix(Matrix4x4 matrix) =>
        (matrix.Translation == Vector3.Zero ? 3 : 4) switch {
            var columns => Node(MatrixNode, Grid([3, columns], Cells(matrix, columns))),
        };

    private static Seq<XElement> Normalized(Vector3 minimum, Vector3 maximum) =>
        (minimum, maximum) == UnitDomain ? [] : [Matrix(Matrix4x4.CreateScale(Vector3.One / (maximum - minimum)) with { Translation = -minimum / (maximum - minimum) })];

    private static Seq<XElement> Restored(Vector3 minimum, Vector3 maximum) =>
        (minimum, maximum) == UnitDomain ? [] : [Matrix(Matrix4x4.CreateScale(maximum - minimum) with { Translation = minimum })];

    private static XElement Grid<T>(ReadOnlySpan<int> dimensions, ReadOnlySpan<T> values) where T : INumberBase<T> =>
        new(ArrayNode, new XAttribute(Dimensions, string.Join(' ', dimensions.ToArray())), Rows(new StringBuilder().Append('\n'), values, dimensions[^1]).ToString());

    private static Seq<XElement> Parameters(string element, Func<int, Seq<(string Name, float Value)>> channel) =>
        Seq(0, 1, 2).Map(channel).Strict() switch {
            var channels when channels.Distinct().Count is 1 => channels.Take(1).Map(held => new XElement(element, Attributes(held))),
            var channels => channels.Map((held, at) => new XElement(element, new XAttribute(Channel, ChannelLetters[at].ToString()), Attributes(held))),
        };

    private static Seq<XAttribute> Attributes(Seq<(string Name, float Value)> held) => held.Map(static attribute => new XAttribute(attribute.Name, Invariant(attribute.Value)));

    private static string Invariant(float value) => value.ToString(CultureInfo.InvariantCulture);

    private static Option<XElement> Child(XElement node, string name) => Children(node, name).Head;

    private static Seq<XElement> Children(XElement node, string name) => toSeq(node.Elements()).Filter(element => string.Equals(element.Name.LocalName, name, StringComparison.Ordinal));

    private static Option<string> Attribute(XElement node, string name) => Optional(node.Attribute(name)?.Value);

    private static bool Flag(XElement node, string name) => Attribute(node, name).Exists(static held => bool.TryParse(held, out bool flag) && flag);

    private static int Line(XElement node) => ((IXmlLineInfo)node).LineNumber;

    private static ImagingFault Malformed(XElement node) => ImagingFault.Of(ImagingReason.Malformed, node.Name.LocalName, Line(node));
}
