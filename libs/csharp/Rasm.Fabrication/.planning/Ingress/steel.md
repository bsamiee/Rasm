# [RASM_FABRICATION_STEEL_IMPORT]

`SteelImport` owns DSTV/NC1 admission from path, text, or bytes into one fabrication steel owner. Every source preserves its received bytes for `ContentKey`, every fault carries a positive DSTV line the `SourceKind.Steel` locus gate admits, and every admitted feature leaves provider types at the boundary.

`SteelPart` carries the complete `ST` descriptor, recognized feature family, arc-aware contour measures, and `AK` minus `IK` region hierarchy. DSTV face-local coordinates resolve into part space through the `SteelFace` frame rows, so a downstream plane consumes placed geometry rather than a face tag it must interpret. `SteelView` parameterizes downstream projection without opening a writer; NC1 emission remains `PostDialect` work.

## [01]-[INDEX]

| [INDEX] | [OWNER]              | [OWNS]                                                    |
| :-----: | :------------------- | :-------------------------------------------------------- |
|  [01]   | `SteelSource`        | path, text, and byte ingress preserving received bytes    |
|  [02]   | `SteelProfileCode`   | DSTV profile vocabulary and its admitted face set         |
|  [03]   | `SteelFace`          | DSTV face vocabulary and the face-to-part placement frame |
|  [04]   | `SteelBlockKind`     | block identity, contour correspondence, and topology sign |
|  [05]   | `SteelParseKind`     | parser-failure classification over exception-type rows    |
|  [06]   | `SteelHeader`        | the complete admitted `ST` descriptor                     |
|  [07]   | `SteelFeature`       | every readable DSTV feature payload                       |
|  [08]   | `SteelPart`          | admitted header, features, topology, and placement        |
|  [09]   | `SteelImport`        | source, parse, and admission rails                        |
|  [10]   | `SteelView`          | parameterized projection over the admitted receipt        |

## [02]-[STEEL_EXCHANGE]

- Owner: `SteelImport` admits one `SteelSource`, and `SteelPart` owns the normalized header, operations, contours, placement, and identity.
- Cases: `SteelSource` closes path, text, and byte ingress; `SteelFeature` closes every readable DSTV feature payload; `SteelProjection` closes part, boundary, feature, topology, placement, and identity views.
- Entry: `Read` returns one deferred `Eff<SteelImportReceipt>` and accumulates independent feature faults before admission.
- Auto: `DstvMap` transcribes feature records, generated owners validate policy, header, and aggregate values, `SteelBlockKind` supplies statement identity, contour correspondence, and topology sign, and `SteelParseKind` classifies a `ParseException` by most-derived exception-type row.
- Receipt: `SteelImportReceipt` carries the admitted part, content key, and source-byte count; `SteelPart.Topology` preserves outer, hole, parent, depth, area, and bounds evidence; `SteelPart.Placed` resolves each face-local feature into part coordinates.
- Packages: `DSTV.Net` owns asynchronous parsing; `Riok.Mapperly` owns field transcription; `Thinktecture.Runtime.Extensions` owns cases and policy rows; `LanguageExt.Core` owns effects, accumulation, and immutable carriers; `UnitsNet` owns physical values; `Loop` composes `CavalierContours` for arc measures; `PolygonAlgebra` composes `Clipper2` for hierarchy and fill.
- Growth: a readable block lands as one `SteelFeature` case, one `SteelBlockKind` row, and one Mapperly declaration; a parser fault lands as one `SteelParseKind` row; a profile or face convention lands as one `SteelProfileCode` or `SteelFace` row; a new source or view lands as one generated case or row.
- Boundary: `DstvBend` remains a typed `KA` rejection until its complete payload is publicly readable, face frames derive wholly from the admitted header so a convention correction is one row, and `ToSvg()` remains outside fabrication projection.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using DSTV.Net.Contracts;
using DSTV.Net.Data;
using DSTV.Net.Enums;
using DSTV.Net.Exceptions;
using DSTV.Net.Implementations;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Ingress;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SteelSource {
    private SteelSource() { }

    public sealed record Path(string Value, CancellationToken Cancellation) : SteelSource;
    public sealed record Text(string Value) : SteelSource;
    public sealed record Bytes(ReadOnlyMemory<byte> Value) : SteelSource;
}

[SmartEnum<string>]
public sealed partial class SteelBlockKind {
    public static readonly SteelBlockKind St = new("ST", None, topologySign: 0);
    public static readonly SteelBlockKind Bo = new("BO", None, topologySign: 0);
    public static readonly SteelBlockKind Si = new("SI", None, topologySign: 0);
    public static readonly SteelBlockKind Sc = new("SC", None, topologySign: 0);
    public static readonly SteelBlockKind Ak = new("AK", Some(ContourType.AK), topologySign: 1);
    public static readonly SteelBlockKind Ik = new("IK", Some(ContourType.IK), topologySign: -1);
    public static readonly SteelBlockKind Ko = new("KO", Some(ContourType.KO), topologySign: 0);
    public static readonly SteelBlockKind Pu = new("PU", Some(ContourType.PU), topologySign: 0);
    public static readonly SteelBlockKind Ka = new("KA", None, topologySign: 0);
    public static readonly SteelBlockKind Source = new("SOURCE", None, topologySign: 0);
    public static readonly SteelBlockKind Unknown = new("UNKNOWN", None, topologySign: 0);

    public Option<ContourType> Contour { get; }

    public int TopologySign { get; }

    public bool Boundary => TopologySign != 0;

    public static Option<SteelBlockKind> Of(ContourType type) =>
        toSeq(Items).Find(row => row.Contour == Some(type));
}

[SmartEnum<string>]
public sealed partial class SteelParseKind {
    public static readonly SteelParseKind Start = new("ST:START", typeof(MissingStartOfFileException));
    public static readonly SteelParseKind Character = new("ST:CHARACTER", typeof(UnexpectedCharacterException));
    public static readonly SteelParseKind End = new("ST:END", typeof(UnexpectedEndException));
    public static readonly SteelParseKind Integer = new("ST:INTEGER", typeof(IntegerParseException));
    public static readonly SteelParseKind Double = new("ST:DOUBLE", typeof(DoubleParseException));
    public static readonly SteelParseKind Enum = new("ST:ENUM", typeof(EnumParseException));
    public static readonly SteelParseKind Tuple = new("ST:TUPLE", typeof(TupleParseException));
    public static readonly SteelParseKind FreeText = new("ST:FREE-TEXT", typeof(FreeTextTooLargeException));
    public static readonly SteelParseKind Structure = new("ST:STRUCTURE", typeof(DstvParseException));
    public static readonly SteelParseKind Unknown = new("ST:UNKNOWN", typeof(ParseException));

    public Type ExceptionType { get; }

    // Specificity ranks by inheritance depth, so a base row never shadows a derived one and declaration order is free.
    public static SteelParseKind Classify(ParseException error) =>
        toSeq(Items)
            .Filter(kind => kind.ExceptionType.IsInstanceOfType(error))
            .OrderByDescending(static kind => Depth(kind.ExceptionType))
            .Head
            .IfNone(Unknown);

    private static int Depth(Type type) =>
        type == typeof(ParseException) ? 0 : 1 + Depth(type.BaseType ?? typeof(ParseException));
}

// SteelFace row owns its DSTV placement convention, so corrections stay on one row.
// Part x runs the member length; the section occupies part y and z.
[SmartEnum<string>]
public sealed partial class SteelFace {
    public static readonly SteelFace Web = new("V", static (_, local) =>
        new Point3d(local.X, 0.0, local.Y), false);
    public static readonly SteelFace Top = new("O", static (header, local) =>
        new Point3d(local.X, local.Y, header.ProfileHeight.As(LengthUnit.Millimeter)), false);
    public static readonly SteelFace Bottom = new("U", static (_, local) =>
        new Point3d(local.X, -local.Y, 0.0), true);
    public static readonly SteelFace Rear = new("H", static (header, local) =>
        new Point3d(-local.X, header.WebThickness.As(LengthUnit.Millimeter), local.Y), true);

    public Func<SteelHeader, Point3d, Point3d> Place { get; }
    public bool Reverses { get; }

    public Arr<double> PlaceBulges(Arr<double> bulges) =>
        Reverses ? bulges.Map(static bulge => -bulge) : bulges;
}

[SmartEnum<string>]
public sealed partial class SteelProfileCode {
    public static readonly SteelProfileCode I = new("I", Seq(SteelFace.Web, SteelFace.Top, SteelFace.Bottom, SteelFace.Rear));
    public static readonly SteelProfileCode U = new("U", Seq(SteelFace.Web, SteelFace.Top, SteelFace.Bottom, SteelFace.Rear));
    public static readonly SteelProfileCode C = new("C", Seq(SteelFace.Web, SteelFace.Top, SteelFace.Bottom, SteelFace.Rear));
    public static readonly SteelProfileCode M = new("M", Seq(SteelFace.Web, SteelFace.Top, SteelFace.Bottom, SteelFace.Rear));
    public static readonly SteelProfileCode So = new("SO", Seq(SteelFace.Web, SteelFace.Top, SteelFace.Bottom, SteelFace.Rear));
    public static readonly SteelProfileCode T = new("T", Seq(SteelFace.Web, SteelFace.Top, SteelFace.Rear));
    public static readonly SteelProfileCode L = new("L", Seq(SteelFace.Web, SteelFace.Bottom, SteelFace.Rear));
    public static readonly SteelProfileCode Ro = new("RO", Seq(SteelFace.Web));
    public static readonly SteelProfileCode Ru = new("RU", Seq(SteelFace.Web));
    public static readonly SteelProfileCode B = new("B", Seq(SteelFace.Web));

    public Seq<SteelFace> Faces { get; }

    public bool Admits(SteelFace face) => Faces.Contains(face);
}

[ComplexValueObject]
public sealed partial class SteelContourPolicy {
    public Context Tolerance { get; }
    public Length MinimumLeg { get; }
    public Angle AngularTolerance { get; }

    public static SteelContourPolicy Canonical(Context tolerance) =>
        Create(
            tolerance,
            Length.FromMillimeters(tolerance.Absolute.Value),
            Angle.FromRadians(tolerance.Angle.Value));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Context tolerance,
        ref Length minimumLeg,
        ref Angle angularTolerance) {
        double leg = minimumLeg.As(LengthUnit.Millimeter);
        double angle = angularTolerance.As(AngleUnit.Radian);
        if (!double.IsFinite(leg) || leg <= 0.0 || !double.IsFinite(angle) || angle <= 0.0 || angle >= (Math.PI / 2.0))
            validationError = new ValidationError("steel contour policy is outside its positive finite domain");
    }
}

[ComplexValueObject]
public sealed partial class SteelHeader {
    public string OrderIdentification { get; }
    public string DrawingIdentification { get; }
    public string PhaseIdentification { get; }
    public string PieceIdentification { get; }
    public int QuantityOfPieces { get; }
    public string Profile { get; }
    public SteelProfileCode ProfileCode { get; }
    public string SteelQuality { get; }
    public Length Length { get; }
    public Length SawLength { get; }
    public Length ProfileHeight { get; }
    public Length FlangeWidth { get; }
    public Length FlangeThickness { get; }
    public Length WebThickness { get; }
    public Length Radius { get; }
    public Angle WebStartCut { get; }
    public Angle WebEndCut { get; }
    public Angle FlangeStartCut { get; }
    public Angle FlangeEndCut { get; }
    public double WeightByMeter { get; }
    public double PaintingSurfaceByMeter { get; }
    public string Text1InfoOnPiece { get; }
    public string Text2InfoOnPiece { get; }
    public string Text3InfoOnPiece { get; }
    public string Text4InfoOnPiece { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string orderIdentification,
        ref string drawingIdentification,
        ref string phaseIdentification,
        ref string pieceIdentification,
        ref int quantityOfPieces,
        ref string profile,
        ref SteelProfileCode profileCode,
        ref string steelQuality,
        ref Length length,
        ref Length sawLength,
        ref Length profileHeight,
        ref Length flangeWidth,
        ref Length flangeThickness,
        ref Length webThickness,
        ref Length radius,
        ref Angle webStartCut,
        ref Angle webEndCut,
        ref Angle flangeStartCut,
        ref Angle flangeEndCut,
        ref double weightByMeter,
        ref double paintingSurfaceByMeter,
        ref string text1InfoOnPiece,
        ref string text2InfoOnPiece,
        ref string text3InfoOnPiece,
        ref string text4InfoOnPiece) {
        orderIdentification = orderIdentification?.Trim() ?? string.Empty;
        drawingIdentification = drawingIdentification?.Trim() ?? string.Empty;
        phaseIdentification = phaseIdentification?.Trim() ?? string.Empty;
        pieceIdentification = pieceIdentification?.Trim() ?? string.Empty;
        profile = profile?.Trim() ?? string.Empty;
        steelQuality = steelQuality?.Trim() ?? string.Empty;
        text1InfoOnPiece = text1InfoOnPiece?.Trim() ?? string.Empty;
        text2InfoOnPiece = text2InfoOnPiece?.Trim() ?? string.Empty;
        text3InfoOnPiece = text3InfoOnPiece?.Trim() ?? string.Empty;
        text4InfoOnPiece = text4InfoOnPiece?.Trim() ?? string.Empty;
        Seq<double> positive = [length.As(LengthUnit.Millimeter), sawLength.As(LengthUnit.Millimeter)];
        Seq<double> nonnegative = [profileHeight.As(LengthUnit.Millimeter), flangeWidth.As(LengthUnit.Millimeter),
            flangeThickness.As(LengthUnit.Millimeter), webThickness.As(LengthUnit.Millimeter), radius.As(LengthUnit.Millimeter),
            weightByMeter, paintingSurfaceByMeter];
        Seq<double> angles = [webStartCut.As(AngleUnit.Radian), webEndCut.As(AngleUnit.Radian),
            flangeStartCut.As(AngleUnit.Radian), flangeEndCut.As(AngleUnit.Radian)];
        Seq<(string Slot, bool Admits)> slots = [
            ("identity", pieceIdentification.Length > 0 && profile.Length > 0 && steelQuality.Length > 0),
            ("quantity", quantityOfPieces > 0),
            ("extent", positive.ForAll(static value => double.IsFinite(value) && value > 0.0)),
            ("section", nonnegative.ForAll(static value => double.IsFinite(value) && value >= 0.0)),
            ("end-cut", angles.ForAll(static value => double.IsFinite(value)))];
        validationError = slots
            .Find(static slot => !slot.Admits)
            .Match(
                Some: static slot => new ValidationError($"steel header rejected at {slot.Slot}"),
                None: static () => (ValidationError?)null);
    }
}

public sealed record SteelBevel(Angle FirstAngle, Length FirstBlunting, Angle SecondAngle, Length SecondBlunting);

public sealed record SteelVertex(Point3d At, bool IsNotch, Length Radius, Option<SteelBevel> Bevel);

public sealed record SteelContour(SteelBlockKind Block, SteelFace Face, Loop Loop, Arr<SteelVertex> Vertices) {
    public double SignedAreaMm2 => Loop.Area();
    public Length Perimeter => Length.FromMillimeters(Loop.Length());
    public Sign Winding => Loop.Winding();
    public BoundingBox Bounds => Loop.Bound();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SteelFeature {
    private SteelFeature() { }

    public sealed record Hole(Point3d Center, SteelFace Face, Length Diameter, Length Depth) : SteelFeature;
    public sealed record Slot(Point3d Center, SteelFace Face, Length Diameter, Length Depth, Length Span, Length Width, Angle Rotation) : SteelFeature;
    public sealed record Cut(Point3d At, SteelFace Face) : SteelFeature;
    public sealed record Numeration(Point3d At, SteelFace Face) : SteelFeature;
    public sealed record Boundary(SteelContour Contour) : SteelFeature;
    public sealed record Marking(SteelContour Contour) : SteelFeature;
}

public sealed record SteelPlacement(
    SteelFeature Feature,
    SteelFace Face,
    Seq<Point3d> Geometry,
    Arr<double> Bulges);

[ComplexValueObject]
public sealed partial class SteelPart {
    public SteelHeader Header { get; }
    public Seq<SteelFeature> Features { get; }
    public TopologyReceipt Topology { get; }

    [IgnoreMember]
    public Seq<SteelContour> Boundaries => ContoursOf(boundary: true);

    [IgnoreMember]
    public Seq<SteelContour> Markings => ContoursOf(boundary: false);

    [IgnoreMember]
    public Arr<Loop> Loops => Boundaries.Map(static contour => contour.Loop).ToArr();

    // Face-local DSTV coordinates only become part geometry through the header, so placement lives with the aggregate
    // that owns both and never with the feature case that carries the bare face tag.
    [IgnoreMember]
    public Seq<SteelPlacement> Placed => Features.Map(feature => feature.Switch(
        hole: hole => new SteelPlacement(feature, hole.Face, Seq(hole.Face.Place(Header, hole.Center)), Arr<double>()),
        slot: slot => new SteelPlacement(feature, slot.Face, Seq(slot.Face.Place(Header, slot.Center)), Arr<double>()),
        cut: cut => new SteelPlacement(feature, cut.Face, Seq(cut.Face.Place(Header, cut.At)), Arr<double>()),
        numeration: numeration => new SteelPlacement(
            feature, numeration.Face, Seq(numeration.Face.Place(Header, numeration.At)), Arr<double>()),
        boundary: boundary => new SteelPlacement(feature, boundary.Contour.Face,
            boundary.Contour.Loop.Vertices.Map(point => boundary.Contour.Face.Place(Header, point)),
            boundary.Contour.Face.PlaceBulges(boundary.Contour.Loop.Bulges)),
        marking: marking => new SteelPlacement(feature, marking.Contour.Face,
            marking.Contour.Loop.Vertices.Map(point => marking.Contour.Face.Place(Header, point)),
            marking.Contour.Face.PlaceBulges(marking.Contour.Loop.Bulges))));

    private Seq<SteelContour> ContoursOf(bool boundary) => Features.Choose(feature => (feature, boundary) switch {
        (SteelFeature.Boundary row, true) => Some(row.Contour),
        (SteelFeature.Marking row, false) => Some(row.Contour),
        _ => None,
    });
}

public sealed record SteelImportReceipt(SteelPart Part, ContentKey Key, int SourceBytes);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SteelProjection {
    private SteelProjection() { }

    public sealed record Part(SteelPart Value) : SteelProjection;
    public sealed record Boundaries(Arr<Loop> Value) : SteelProjection;
    public sealed record Features(Seq<SteelFeature> Value) : SteelProjection;
    public sealed record Placements(Seq<SteelPlacement> Value) : SteelProjection;
    public sealed record Topology(TopologyReceipt Value) : SteelProjection;
    public sealed record Identity(ContentKey Value) : SteelProjection;
}

[SmartEnum]
public sealed partial class SteelView {
    public static readonly SteelView Part = new(static receipt => new SteelProjection.Part(receipt.Part));
    public static readonly SteelView Boundaries = new(static receipt => new SteelProjection.Boundaries(receipt.Part.Loops));
    public static readonly SteelView Features = new(static receipt => new SteelProjection.Features(receipt.Part.Features));
    public static readonly SteelView Placements = new(static receipt => new SteelProjection.Placements(receipt.Part.Placed));
    public static readonly SteelView Topology = new(static receipt => new SteelProjection.Topology(receipt.Part.Topology));
    public static readonly SteelView Identity = new(static receipt => new SteelProjection.Identity(receipt.Key));

    [UseDelegateFromConstructor]
    public partial SteelProjection Project(SteelImportReceipt receipt);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class SteelImport {
    private const int HeaderLine = 1;
    private const int FirstFeatureLine = HeaderLine + 1;

    public static Eff<SteelImportReceipt> Read(SteelSource source, SteelContourPolicy policy) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(policy);
        return from bytes in Payload(source)
               from parsed in Parse(bytes)
               from receipt in Admit(parsed, bytes, policy).ToEff()
               select receipt;
    }

    private static Eff<byte[]> Payload(SteelSource source) =>
        source.Switch(
                path: static path => Eff.lift(async () =>
                    await File.ReadAllBytesAsync(path.Value, path.Cancellation).ConfigureAwait(false)),
                text: static text => Eff.lift(() => Encoding.UTF8.GetBytes(text.Value)),
                bytes: static bytes => Eff.lift(() => bytes.Value.ToArray()))
            .MapFail(static _ => Fault(SteelBlockKind.Source.Key, HeaderLine));

    private static Eff<IDstv> Parse(byte[] bytes) =>
        Eff.lift(async () => {
            using MemoryStream stream = new(bytes, writable: false);
            using TextReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
            return await new DstvReader().ParseAsync(reader).ConfigureAwait(false);
        }).MapFail(static error => error.Exception
            .Bind(static exception => Optional(exception as ParseException))
            .Match(
                Some: static parsed => Fault(SteelParseKind.Classify(parsed).Key, parsed.LineNumber ?? HeaderLine),
                None: static () => Fault(SteelParseKind.Unknown.Key, HeaderLine)));

    private static Fin<SteelImportReceipt> Admit(IDstv document, byte[] bytes, SteelContourPolicy policy) =>
        from source in Optional(document.Header).ToFin(Fault(SteelBlockKind.St.Key, HeaderLine))
        from header in Header(source)
        from features in Features(document.Elements, header, policy).ToFin()
        from topology in TopologyOf(features)
        from part in Try.lift(() => SteelPart.Create(header, features, topology))
            .Run()
            .MapFail(static _ => Fault(SteelBlockKind.St.Key, HeaderLine))
        select new SteelImportReceipt(part, ContentKey.Of(EgressKind.Nc1, bytes), bytes.Length);

    private static Fin<TopologyReceipt> TopologyOf(Seq<SteelFeature> features) {
        Seq<(SteelBlockKind Block, Loop Loop)> regions = features
            .Choose(static feature => feature is SteelFeature.Boundary { Contour: { Block: var block, Loop: var loop } }
                ? Some((Block: block, Loop: loop))
                : None);
        Seq<Loop> outers = regions.Filter(static row => row.Block.TopologySign > 0).Map(static row => row.Loop);
        Seq<Loop> holes = regions.Filter(static row => row.Block.TopologySign < 0).Map(static row => row.Loop);
        PolygonOp operation = holes.IsEmpty
            ? new PolygonOp.Inspect(outers, new PolygonQuery.Topology(PolygonFill.NonZero))
            : new PolygonOp.Boolean(outers, holes, PolygonBoolean.Difference, PolygonFill.NonZero);
        return outers.IsEmpty
            ? Fin.Fail<TopologyReceipt>(Fault(SteelBlockKind.Ak.Key, HeaderLine))
            : PolygonAlgebra.Apply(operation).Bind(static trace => trace is PolygonTrace.Regions result
                ? Fin.Succ(result.Result)
                : Fin.Fail<TopologyReceipt>(Fault(SteelBlockKind.Ak.Key, HeaderLine)));
    }

    private static Fin<SteelHeader> Header(IDstvHeader source) {
        if (!SteelProfileCode.TryGet(source.CodeProfile.ToString().Trim().ToUpperInvariant(), out SteelProfileCode? code))
            return Fin.Fail<SteelHeader>(Fault(SteelBlockKind.St.Key, HeaderLine));
        ValidationError? failure = SteelHeader.Validate(
            source.OrderIdentification,
            source.DrawingIdentification,
            source.PhaseIdentification,
            source.PieceIdentification,
            source.QuantityOfPieces,
            source.Profile,
            code,
            source.SteelQuality,
            DstvMap.Millimeters(source.Length),
            DstvMap.Millimeters(source.SawLength),
            DstvMap.Millimeters(source.ProfileHeight),
            DstvMap.Millimeters(source.FlangeWidth),
            DstvMap.Millimeters(source.FlangeThickness),
            DstvMap.Millimeters(source.WebThickness),
            DstvMap.Millimeters(source.Radius),
            DstvMap.Degrees(source.WebStartCut),
            DstvMap.Degrees(source.WebEndCut),
            DstvMap.Degrees(source.FlangeStartCut),
            DstvMap.Degrees(source.FlangeEndCut),
            source.WeightByMeter,
            source.PaintingSurfaceByMeter,
            source.Text1InfoOnPiece,
            source.Text2InfoOnPiece,
            source.Text3InfoOnPiece,
            source.Text4InfoOnPiece,
            CultureInfo.InvariantCulture,
            out SteelHeader? admitted);
        return failure is null && admitted is not null
            ? Fin.Succ(admitted)
            : Fin.Fail<SteelHeader>(Fault(SteelBlockKind.St.Key, HeaderLine));
    }

    // DSTV block positions are one-based; the ordinal converts once here so no fault site can mint the line-zero locus
    // SourceKind.Steel refuses.
    private static Validation<Error, Seq<SteelFeature>> Features(
        IEnumerable<DstvElement> elements,
        SteelHeader header,
        SteelContourPolicy policy) =>
        toSeq(elements).Map(static (element, ordinal) => (Element: element, Line: ordinal + FirstFeatureLine))
            .Traverse(row => Feature(row.Element, row.Line, header, policy).ToValidation()).As();

    // DstvSlot derives from DstvHole and DstvSkewedPoint from DstvContourPoint, so the derived arm precedes its base.
    private static Fin<SteelFeature> Feature(DstvElement element, int line, SteelHeader header, SteelContourPolicy policy) =>
        element switch {
            DstvSlot slot => Capture(() => DstvMap.Slot(slot), SteelBlockKind.Bo, line, header),
            DstvHole hole => Capture(() => DstvMap.Hole(hole), SteelBlockKind.Bo, line, header),
            DstvCut cut => Capture(() => DstvMap.Cut(cut), SteelBlockKind.Sc, line, header),
            DstvNumeration numeration => Capture(() => DstvMap.Numeration(numeration), SteelBlockKind.Si, line, header),
            DstvBend => Fin.Fail<SteelFeature>(Fault(SteelBlockKind.Ka.Key, line)),
            Contour contour => SteelBlockKind.Of(contour.ContourType)
                .ToFin(Fault(SteelBlockKind.Unknown.Key, line))
                .Bind(block => ContourOf(contour, block, line, header, policy)),
            _ => Fin.Fail<SteelFeature>(Fault(SteelBlockKind.Unknown.Key, line)),
        };

    private static Fin<SteelFeature> Capture(Func<SteelFeature> mapping, SteelBlockKind block, int line, SteelHeader header) =>
        Try.lift(mapping)
            .Run()
            .MapFail(_ => Fault(block.Key, line))
            .Bind(feature => Valid(feature, block, line, header));

    private static Fin<SteelFeature> Valid(SteelFeature feature, SteelBlockKind block, int line, SteelHeader header) =>
        feature.Switch(
            state: header,
            hole: static (hole, row) => Faced(row, hole.Face) && ValidPoint(hole.Center)
                && Positive(hole.Diameter) && Nonnegative(hole.Depth),
            slot: static (slot, row) => Faced(row, slot.Face) && ValidPoint(slot.Center)
                && Positive(slot.Diameter) && Nonnegative(slot.Depth) && Positive(slot.Span) && Positive(slot.Width)
                && slot.Span >= slot.Width && Finite(slot.Rotation),
            cut: static (cut, row) => Faced(row, cut.Face) && ValidPoint(cut.At),
            numeration: static (numeration, row) => Faced(row, numeration.Face) && ValidPoint(numeration.At),
            boundary: static (boundary, row) => Faced(row, boundary.Contour.Face),
            marking: static (marking, row) => Faced(row, marking.Contour.Face))
            ? Fin.Succ(feature)
            : Fin.Fail<SteelFeature>(Fault(block.Key, line));

    private static bool Faced(SteelHeader header, SteelFace face) => header.ProfileCode.Admits(face);

    private static Fin<SteelFeature> ContourOf(
        Contour contour,
        SteelBlockKind block,
        int line,
        SteelHeader header,
        SteelContourPolicy policy) => Try.lift(() => (
            Face: DstvMap.Face(contour.FlCode),
            Vertices: toSeq(contour.Points).Map(static point => point switch {
                DstvSkewedPoint skew => DstvMap.Vertex(skew) with {
                    Bevel = Some(new SteelBevel(
                        DstvMap.Degrees(skew.FirstAngle), DstvMap.Millimeters(skew.FirstBlunting),
                        DstvMap.Degrees(skew.SecondAngle), DstvMap.Millimeters(skew.SecondBlunting))),
                },
                _ => DstvMap.Vertex(point),
            }).ToArr()))
        .Run()
        .MapFail(_ => Fault(block.Key, line))
        .Bind(active => Faced(header, active.Face)
            ? Rounded(active.Vertices, policy, block, line)
            .Bind(loop => block.TopologySign > 0 ? loop.AsCcw() : Fin.Succ(loop))
            .Map(loop => block.Boundary
                ? (SteelFeature)new SteelFeature.Boundary(new SteelContour(block, active.Face, loop, active.Vertices))
                : new SteelFeature.Marking(new SteelContour(block, active.Face, loop, active.Vertices)))
            : Fin.Fail<SteelFeature>(Fault(block.Key, line)));

    private static Fin<Loop> Rounded(Arr<SteelVertex> vertices, SteelContourPolicy policy, SteelBlockKind block, int line) =>
        vertices.Count < 3
            ? Fin.Fail<Loop>(Fault(block.Key, line))
            : toSeq(Enumerable.Range(0, vertices.Count)).Traverse(index => Corner(vertices, index, policy, block, line)).As()
                .Bind(corners => toSeq(Enumerable.Range(0, vertices.Count)).Exists(index => {
                    int next = (index + 1) % vertices.Count;
                    Vector3d edge = vertices[next].At - vertices[index].At;
                    Vector3d straight = corners[next].Enter - corners[index].Exit;
                    return straight.Length <= policy.Tolerance.Absolute.Value || (edge * straight) <= 0.0;
                })
                    ? Fin.Fail<Loop>(Fault(block.Key, line))
                    : Fin.Succ(corners.Bind(corner => corner.Enter.DistanceTo(corner.Exit) <= policy.Tolerance.Absolute.Value
                        ? Seq((At: corner.Enter, Bulge: 0.0))
                        : Seq((At: corner.Enter, corner.Bulge), (At: corner.Exit, Bulge: 0.0)))))
                .Bind(spans => Loop.Admit(
                    spans.Map(static span => span.At).ToArr(),
                    closed: true,
                    spans.Map(static span => span.Bulge).ToArr(),
                    policy.Tolerance).MapFail(_ => Fault(block.Key, line)));

    private static Fin<(Point3d Enter, double Bulge, Point3d Exit)> Corner(
        Arr<SteelVertex> vertices,
        int index,
        SteelContourPolicy policy,
        SteelBlockKind block,
        int line) {
        SteelVertex vertex = vertices[index];
        double radius = vertex.Radius.As(LengthUnit.Millimeter);
        if (!ValidPoint(vertex.At) || !double.IsFinite(radius) || radius < 0.0 || !ValidBevel(vertex.Bevel))
            return Fin.Fail<(Point3d, double, Point3d)>(Fault(block.Key, line));
        if (radius == 0.0)
            return Fin.Succ((vertex.At, 0.0, vertex.At));
        Point3d previous = vertices[((index - 1) + vertices.Count) % vertices.Count].At;
        Point3d next = vertices[(index + 1) % vertices.Count].At;
        Vector3d incoming = previous - vertex.At;
        Vector3d outgoing = next - vertex.At;
        double incomingLength = incoming.Length;
        double outgoingLength = outgoing.Length;
        double minimum = policy.MinimumLeg.As(LengthUnit.Millimeter);
        if (incomingLength <= minimum || outgoingLength <= minimum)
            return Fin.Fail<(Point3d, double, Point3d)>(Fault(block.Key, line));
        Vector3d towardPrevious = incoming / incomingLength;
        Vector3d towardNext = outgoing / outgoingLength;
        double theta = Vector3d.VectorAngle(towardPrevious, towardNext);
        double tangent = radius / Math.Tan(theta / 2.0);
        double sign = Math.Sign(Vector3d.CrossProduct(-towardPrevious, towardNext).Z);
        double angular = policy.AngularTolerance.As(AngleUnit.Radian);
        return !double.IsFinite(theta) || theta <= angular || (Math.PI - theta) <= angular
            || !double.IsFinite(tangent) || tangent <= 0.0 || tangent >= incomingLength || tangent >= outgoingLength || sign == 0.0
                ? Fin.Fail<(Point3d, double, Point3d)>(Fault(block.Key, line))
                : Fin.Succ((
                    vertex.At + (towardPrevious * tangent),
                    (vertex.IsNotch ? -sign : sign) * Math.Tan((Math.PI - theta) / 4.0),
                    vertex.At + (towardNext * tangent)));
    }

    private static bool ValidBevel(Option<SteelBevel> bevel) =>
        bevel.ForAll(static row => Finite(row.FirstAngle) && Nonnegative(row.FirstBlunting)
            && Finite(row.SecondAngle) && Nonnegative(row.SecondBlunting));

    private static bool ValidPoint(Point3d point) =>
        double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z);

    private static bool Positive(Length value) =>
        double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) > 0.0;

    private static bool Nonnegative(Length value) =>
        double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) >= 0.0;

    private static bool Finite(Angle value) => double.IsFinite(value.As(AngleUnit.Radian));

    // SourceKind.Steel admits a DstvBlock only on a positive line, so the one mint floors every locus at the ST block.
    private static Error Fault(string block, int line) =>
        new FabricationFault.IngressTranslation(new SourceLocus.DstvBlock(block, Math.Max(line, HeaderLine)));
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class DstvMap {
    [MapPropertyFromSource(nameof(SteelVertex.At), Use = nameof(ContourPoint))]
    [MapProperty(nameof(DstvContourPoint.Radius), nameof(SteelVertex.Radius), Use = nameof(Millimeters))]
    [MapPropertyFromSource(nameof(SteelVertex.Bevel), Use = nameof(NoBevel))]
    public static partial SteelVertex Vertex(DstvContourPoint source);

    [MapPropertyFromSource(nameof(SteelFeature.Hole.Center), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelFeature.Hole.Face), Use = nameof(Face))]
    [MapProperty(nameof(DstvHole.Diameter), nameof(SteelFeature.Hole.Diameter), Use = nameof(Millimeters))]
    [MapProperty(nameof(DstvHole.Depth), nameof(SteelFeature.Hole.Depth), Use = nameof(Millimeters))]
    public static partial SteelFeature.Hole Hole(DstvHole source);

    [MapPropertyFromSource(nameof(SteelFeature.Slot.Center), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelFeature.Slot.Face), Use = nameof(Face))]
    [MapProperty(nameof(DstvHole.Diameter), nameof(SteelFeature.Slot.Diameter), Use = nameof(Millimeters))]
    [MapProperty(nameof(DstvHole.Depth), nameof(SteelFeature.Slot.Depth), Use = nameof(Millimeters))]
    [MapProperty(nameof(DstvSlot.SlotLength), nameof(SteelFeature.Slot.Span), Use = nameof(Millimeters))]
    [MapProperty(nameof(DstvSlot.SlotWidth), nameof(SteelFeature.Slot.Width), Use = nameof(Millimeters))]
    [MapProperty(nameof(DstvSlot.SlotAngle), nameof(SteelFeature.Slot.Rotation), Use = nameof(Degrees))]
    public static partial SteelFeature.Slot Slot(DstvSlot source);

    [MapPropertyFromSource(nameof(SteelFeature.Cut.At), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelFeature.Cut.Face), Use = nameof(Face))]
    public static partial SteelFeature.Cut Cut(DstvCut source);

    [MapPropertyFromSource(nameof(SteelFeature.Numeration.At), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelFeature.Numeration.Face), Use = nameof(Face))]
    public static partial SteelFeature.Numeration Numeration(DstvNumeration source);

    // Get throws on an unlisted DSTV face code; every call site rides SteelImport.Capture, so the throw lands as the
    // block-and-line fault rather than escaping the rail.
    [UserMapping]
    internal static SteelFace Face(string code) => SteelFace.Get(code.Trim().ToUpperInvariant());

    [UserMapping]
    internal static Point3d LocatedPoint(LocatedElement source) => new(source.XCoord, source.YCoord, 0.0);

    [UserMapping]
    internal static Point3d ContourPoint(DstvContourPoint source) => new(source.XCoord, source.YCoord, 0.0);

    [UserMapping]
    internal static Length Millimeters(double value) => new(value, LengthUnit.Millimeter);

    [UserMapping]
    internal static Angle Degrees(double value) => new(value, AngleUnit.Degree);

    [UserMapping]
    internal static Option<SteelBevel> NoBevel(DstvContourPoint source) => None;
}
```

## [03]-[LIFECYCLE]

- Admission: `SteelSource` normalizes every source to stable bytes before `DstvReader.ParseAsync` runs; the header admits before any feature so profile-code face admissibility gates each located element.
- Owner: `SteelHeader`, `SteelFeature`, and `SteelPart` retain the complete readable NC1 program without provider shapes; `SteelContour` exposes arc-aware measure evidence, `SteelPart.Topology` owns the normalized `AK` minus `IK` hierarchy, and `SteelPart.Placed` owns face-to-part resolution with contour bulges beside transformed vertices.
- Rail: parse and source effects remain deferred on `Eff`; independent feature faults accumulate on `Validation<Error, Seq<SteelFeature>>` and collapse once into the ingress rail.
- Projection: `SteelView` selects part, boundary, feature, placement, topology, or identity egress through one generated behavior row, and `SteelProjection` preserves each result shape including `TopologyReceipt`.
- Boundary: path cancellation remains source data, contour geometry is the named statement kernel, one `Fault` mint floors every locus at the `ST` line so `SourceKind.Steel` admits it, and every unreadable block fails with its block key and one-based line.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
