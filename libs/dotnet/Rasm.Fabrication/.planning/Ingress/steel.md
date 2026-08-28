# [RASM_FABRICATION_STEEL_IMPORT]

`SteelImport` owns DSTV/NC1 admission from path, text, or bytes into one fabrication steel owner. Every source preserves its received bytes for `ContentKey`, every fault carries a positive DSTV line the `SourceKind.Steel` locus gate admits, and every admitted feature leaves provider types at the boundary.

`SteelPart` carries the complete `ST` descriptor, recognized feature family, arc-aware contour measures, and `AK` minus `IK` region hierarchy. DSTV face-local coordinates resolve into part space through the `SteelFace` frame rows, so a downstream plane consumes placed geometry rather than a face tag it must interpret. `DstvMap` is the folder's Mapperly benchmark and the ONE provider transcription: it carries the header row and every feature record, so the twenty-four-position admission call site collapses to one row argument and `Posting/dialect` `Nc1Canonical` composes the SAME table as its inverse rather than restating twenty-six positions. `SteelView` parameterizes downstream projection without opening a writer; NC1 emission remains `PostDialect` work.

## [01]-[INDEX]

- [02]-[STEEL_EXCHANGE]: `SteelSource` path, text, and byte ingress preserving received bytes, the `SteelProfileCode`/`SteelFace`/`SteelBlockKind`/`SteelParseKind` DSTV vocabularies carrying face admissibility, contour correspondence, topology sign, and exception-type classification, the admitted `SteelHeader`/`SteelPart` owners, and `DstvMap` the one provider transcription.
- [03]-[STEEL_LIFECYCLE]: `SteelImport.Read` admitting one `ImportedSteel` over `SteelHeader` and `SteelFeature` — header-before-feature admission over stable bytes, and deferred `Eff` parse effects accumulating independent feature faults on `Validation`.
- [04]-[PROJECTION_EGRESS]: `SteelView` selecting part, boundary, preparation, feature, placement, topology, or identity egress through one generated behavior row, and `SteelProjection` carrying each result shape.

## [02]-[STEEL_EXCHANGE]

- Owner: `SteelPart` owns the normalized header, operations, contours, placement, and identity; `SteelFace` owns the DSTV placement convention per row; `SteelBlockKind` owns statement identity, contour correspondence, and topology sign; `DstvMap` owns every provider-to-owned transcription.
- Cases: `SteelSource` closes path, text, and byte ingress; `SteelFeature` closes every readable DSTV feature payload; `SteelParseKind` closes the parser exception hierarchy by most-derived row.
- Law: the header admits from ONE `SteelHeaderRow` argument. Twenty-four positional arguments at a call site make a transposed pair — a flange width where a flange thickness belongs — invisible to the compiler and to every reader; the row names each column once at the mapper that lifts it.
- Law: `DstvMap` is the ONE table. `Posting/dialect` `Nc1Canonical.Header` is its exact inverse and composes it through `[IncludeMappingConfiguration]`, so the header correspondence is stated once and a round trip becomes a build fact rather than two rosters that drift.
- Auto: generated owners validate policy, header, and aggregate values; `SteelBlockKind` supplies statement identity, contour correspondence, and topology sign; `SteelParseKind` classifies a `ParseException` by inheritance depth so declaration order is free; `SteelProfileCode.Admits` gates each located element's face before any geometry is built.
- Result: `SteelPart.Topology` preserves outer, hole, parent, depth, area, and bounds evidence; `SteelPart.Placed` resolves each face-local feature into part coordinates with contour bulges beside transformed vertices; `SteelPart.Preparations` publishes the per-edge groove demand the skewed contour points state, keyed on the boundary ordinal a run's profile column shares.
- Packages: `DSTV.Net` owns asynchronous parsing; `Riok.Mapperly` owns field transcription; `Thinktecture.Runtime.Extensions` owns cases and policy rows; `LanguageExt.Core` owns effects, accumulation, and immutable carriers; `UnitsNet` owns physical values; `Loop` composes `CavalierContours` for arc measures; `PolygonAlgebra` composes `Clipper2` for hierarchy and fill.
- Growth: a readable block lands as one `SteelFeature` case, one `SteelBlockKind` row, and one Mapperly declaration; a parser fault lands as one `SteelParseKind` row; a profile or face convention lands as one `SteelProfileCode` or `SteelFace` row; a new source or view lands as one generated case or row.
- Boundary: `DstvBend` remains a typed `KA` rejection until its complete payload is publicly readable; face frames derive wholly from the admitted header so a convention correction is one row; an unlisted DSTV code refuses through the vocabulary's own generated `TryGet` lifted to `Option`, on the error channel at the line that read it. The documented `ParseException` hierarchy and BCL file availability lower to caused fabrication cases; every other throw retains the exact exceptional `Error`. `ToSvg()` remains outside fabrication projection.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
using Rasm.Drawing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Ingress;

// --- [TYPES] ---------------------------------------------------------------------------
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

    public static SteelParseKind Classify(ParseException error) =>
        toSeq(Items)
            .Filter(kind => kind.ExceptionType.IsInstanceOfType(error))
            .Fold(Option<SteelParseKind>.None, static (best, kind) =>
                best.Filter(held => Depth(held.ExceptionType) >= Depth(kind.ExceptionType)).IsSome ? best : Some(kind))
            .IfNone(Unknown);

    private static int Depth(Type type) =>
        type == typeof(ParseException) ? 0 : 1 + Depth(type.BaseType ?? typeof(ParseException));
}

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
    public static readonly SteelFace Unknown = new("?", static (_, local) =>
        new Point3d(local.X, 0.0, local.Y), false);

    public Func<SteelHeader, Point3d, Point3d> Place { get; }
    public bool Reverses { get; }

    public Arr<double> PlaceBulges(Arr<double> bulges) =>
        Reverses ? bulges.Map(static bulge => -bulge) : bulges;

    public static Option<SteelFace> Of(string code) =>
        TryGet(code.Trim().ToUpperInvariant(), out SteelFace? row) ? Some(row) : None;
}

[SmartEnum<string>]
public sealed partial class SteelProfileCode {
    public static readonly SteelProfileCode Unknown = new("?", Seq<SteelFace>());
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

    public static Option<SteelProfileCode> Of(char code) =>
        TryGet(code.ToString().Trim().ToUpperInvariant(), out SteelProfileCode? row) ? Some(row) : None;
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct OrderMark {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = Witness.Keyed(value) ? null : new ValidationError(string.Join(" | ", new object?[] { "steel-header:order" }));
    }

    public static Fin<OrderMark> Admit(string value) => Admission.OfValue<OrderMark, string>(value);
}

[ValueObject<string>]
public readonly partial struct PhaseMark {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = Witness.Keyed(value) ? null : new ValidationError(string.Join(" | ", new object?[] { "steel-header:phase" }));
    }

    public static Fin<PhaseMark> Admit(string value) => Admission.OfValue<PhaseMark, string>(value);
}

[ValueObject<string>]
public readonly partial struct PieceMark {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = Witness.Keyed(value) ? null : new ValidationError(string.Join(" | ", new object?[] { "steel-header:piece" }));
    }

    public static Fin<PieceMark> Admit(string value) => Admission.OfValue<PieceMark, string>(value);
}

[ComplexValueObject]
public sealed partial class SteelContourPolicy {
    public Context Tolerance { get; }
    public Length MinimumLeg { get; }
    public Angle AngularTolerance { get; }

    public NamingStandard Drawings { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Context tolerance,
        ref Length minimumLeg,
        ref Angle angularTolerance,
        ref NamingStandard drawings) {
        double leg = minimumLeg.As(LengthUnit.Millimeter);
        double angle = angularTolerance.As(AngleUnit.Radian);
        if (!ValidityClaim.Positive(leg).Holds || !ValidityClaim.Positive(angle).Holds || angle >= Math.PI / 2.0)
            validationError = new ValidationError(string.Join(" | ", new object?[] { "steel-contour-policy:domain" }));
    }

    public static Fin<SteelContourPolicy> Admit(
        Context tolerance, Length minimumLeg, Angle angularTolerance, NamingStandard drawings) =>
        Validate(tolerance, minimumLeg, angularTolerance, drawings, out SteelContourPolicy policy).Admitted(policy);

    public static Fin<SteelContourPolicy> Canonical(Context tolerance) => Admit(
        tolerance,
        Length.FromMillimeters(tolerance.Absolute.Value),
        Angle.FromRadians(tolerance.Angle.Value),
        NamingStandard.Simple);
}

public sealed record SteelHeaderRow(
    string OrderIdentification,
    string DrawingIdentification,
    string PhaseIdentification,
    string PieceIdentification,
    int QuantityOfPieces,
    string Profile,
    SteelProfileCode ProfileCode,
    string SteelQuality,
    Length Length,
    Length SawLength,
    Length ProfileHeight,
    Length FlangeWidth,
    Length FlangeThickness,
    Length WebThickness,
    Length Radius,
    Angle WebStartCut,
    Angle WebEndCut,
    Angle FlangeStartCut,
    Angle FlangeEndCut,
    double WeightByMeter,
    double PaintingSurfaceByMeter,
    string Text1InfoOnPiece,
    string Text2InfoOnPiece,
    string Text3InfoOnPiece,
    string Text4InfoOnPiece);

[ComplexValueObject]
public sealed partial class SteelHeader {
    public Option<OrderMark> Order { get; }
    public Option<SheetNumber> Drawing { get; }
    public Option<PhaseMark> Phase { get; }
    public PieceMark Piece { get; }
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
        ref Option<OrderMark> order,
        ref Option<SheetNumber> drawing,
        ref Option<PhaseMark> phase,
        ref PieceMark piece,
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
        profile = profile.Trim();
        steelQuality = steelQuality.Trim();
        text1InfoOnPiece = text1InfoOnPiece.Trim();
        text2InfoOnPiece = text2InfoOnPiece.Trim();
        text3InfoOnPiece = text3InfoOnPiece.Trim();
        text4InfoOnPiece = text4InfoOnPiece.Trim();
        Seq<double> extent = [length.As(LengthUnit.Millimeter), sawLength.As(LengthUnit.Millimeter)];
        Seq<double> section = [profileHeight.As(LengthUnit.Millimeter), flangeWidth.As(LengthUnit.Millimeter),
            flangeThickness.As(LengthUnit.Millimeter), webThickness.As(LengthUnit.Millimeter),
            radius.As(LengthUnit.Millimeter), weightByMeter, paintingSurfaceByMeter];
        Seq<double> angles = [webStartCut.As(AngleUnit.Radian), webEndCut.As(AngleUnit.Radian),
            flangeStartCut.As(AngleUnit.Radian), flangeEndCut.As(AngleUnit.Radian)];
        Seq<(string Slot, bool Admits)> slots = [
            ("identity", Witness.Keyed(profile) && Witness.Keyed(steelQuality)),
            ("quantity", quantityOfPieces > 0),
            ("extent", extent.ForAll(static value => ValidityClaim.Positive(value).Holds)),
            ("section", section.ForAll(static value => double.IsFinite(value) && value >= 0.0)),
            ("end-cut", angles.ForAll(double.IsFinite))];
        validationError = slots
            .Find(static slot => !slot.Admits)
            .Match<ValidationError?>(
                Some: static slot => new ValidationError(string.Join(" | ", new object?[] { $"steel-header:{slot.Slot}" })),
                None: static () => null);
    }

    private static Fin<Option<T>> Stated<T>(string text, Func<string, Fin<T>> admit) =>
        Witness.Keyed(text) ? admit(text.Trim()).Map(Some) : Fin.Succ(Option<T>.None);

    public static Fin<SteelHeader> Admit(SteelHeaderRow row, NamingStandard drawings) =>
        (from order in Stated(row.OrderIdentification, OrderMark.Admit)
         from drawing in Stated(row.DrawingIdentification, text => SheetNumber.Parse(drawings, text))
         from phase in Stated(row.PhaseIdentification, PhaseMark.Admit)
         from piece in PieceMark.Admit(row.PieceIdentification)
         select (Order: order, Drawing: drawing, Phase: phase, Piece: piece))
        .Bind(id => Validate(
            id.Order, id.Drawing, id.Phase, id.Piece,
            row.QuantityOfPieces, row.Profile, row.ProfileCode, row.SteelQuality,
            row.Length, row.SawLength, row.ProfileHeight, row.FlangeWidth, row.FlangeThickness, row.WebThickness,
            row.Radius, row.WebStartCut, row.WebEndCut, row.FlangeStartCut, row.FlangeEndCut,
            row.WeightByMeter, row.PaintingSurfaceByMeter,
            row.Text1InfoOnPiece, row.Text2InfoOnPiece, row.Text3InfoOnPiece, row.Text4InfoOnPiece,
            out SteelHeader header).Admitted(header));
}

public sealed record SteelBevel(Angle FirstAngle, Length FirstBlunting, Angle SecondAngle, Length SecondBlunting);

public sealed record SteelVertex(Point3d At, bool IsNotch, Length Radius, Option<SteelBevel> Bevel);

public sealed record EdgePreparation(int Profile, Point3d At, SteelFace Face, SteelBevel Bevel);

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
    public RegionTopology Topology { get; }

    [IgnoreMember]
    public Seq<SteelContour> Boundaries => Features
        .Choose(static feature => feature is SteelFeature.Boundary row ? Some(row.Contour) : None);

    [IgnoreMember]
    public Seq<SteelContour> Markings => Features
        .Choose(static feature => feature is SteelFeature.Marking row ? Some(row.Contour) : None);

    [IgnoreMember]
    public Arr<Loop> Loops => Boundaries.Map(static contour => contour.Loop).ToArr();

    [IgnoreMember]
    public Arr<EdgePreparation> Preparations => Boundaries
        .Map(static (contour, profile) => (Contour: contour, Profile: profile))
        .Bind(static row => row.Contour.Vertices.ToSeq().Choose(vertex => vertex.Bevel
            .Map(bevel => new EdgePreparation(row.Profile, vertex.At, row.Contour.Face, bevel))))
        .ToArr();

    [IgnoreMember]
    public Seq<SteelPlacement> Placed => Features.Map(feature => feature.Switch(
        state: Header,
        hole: static (header, hole) => new SteelPlacement(hole, hole.Face, Seq(hole.Face.Place(header, hole.Center)), Arr<double>()),
        slot: static (header, slot) => new SteelPlacement(slot, slot.Face, Seq(slot.Face.Place(header, slot.Center)), Arr<double>()),
        cut: static (header, cut) => new SteelPlacement(cut, cut.Face, Seq(cut.Face.Place(header, cut.At)), Arr<double>()),
        numeration: static (header, numeration) => new SteelPlacement(
            numeration, numeration.Face, Seq(numeration.Face.Place(header, numeration.At)), Arr<double>()),
        boundary: static (header, boundary) => Contoured(header, boundary, boundary.Contour),
        marking: static (header, marking) => Contoured(header, marking, marking.Contour)));

    private static SteelPlacement Contoured(SteelHeader header, SteelFeature feature, SteelContour contour) =>
        new(feature, contour.Face,
            contour.Loop.Vertices.Map(point => contour.Face.Place(header, point)),
            contour.Face.PlaceBulges(contour.Loop.Bulges));

    public static Fin<SteelPart> Admit(SteelHeader header, Seq<SteelFeature> features, RegionTopology topology) =>
        Validate(header, features, topology, out SteelPart part).Admitted(part);
}

public sealed record ImportedSteel(SteelPart Part, ContentKey Key);

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class DstvMap {
    [MapProperty(nameof(IDstvHeader.CodeProfile), nameof(SteelHeaderRow.ProfileCode), Use = nameof(Profile))]
    [MapProperty(nameof(IDstvHeader.Length), nameof(SteelHeaderRow.Length), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.SawLength), nameof(SteelHeaderRow.SawLength), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.ProfileHeight), nameof(SteelHeaderRow.ProfileHeight), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.FlangeWidth), nameof(SteelHeaderRow.FlangeWidth), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.FlangeThickness), nameof(SteelHeaderRow.FlangeThickness), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.WebThickness), nameof(SteelHeaderRow.WebThickness), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.Radius), nameof(SteelHeaderRow.Radius), Use = nameof(Millimeters))]
    [MapProperty(nameof(IDstvHeader.WebStartCut), nameof(SteelHeaderRow.WebStartCut), Use = nameof(Degrees))]
    [MapProperty(nameof(IDstvHeader.WebEndCut), nameof(SteelHeaderRow.WebEndCut), Use = nameof(Degrees))]
    [MapProperty(nameof(IDstvHeader.FlangeStartCut), nameof(SteelHeaderRow.FlangeStartCut), Use = nameof(Degrees))]
    [MapProperty(nameof(IDstvHeader.FlangeEndCut), nameof(SteelHeaderRow.FlangeEndCut), Use = nameof(Degrees))]
    [MapperConstructor]
    public static partial SteelHeaderRow Header(IDstvHeader source);

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

    [UserMapping]
    internal static SteelFace Face(string code) => SteelFace.Of(code).IfNone(SteelFace.Unknown);

    [UserMapping]
    internal static SteelProfileCode Profile(char code) => SteelProfileCode.Of(code).IfNone(SteelProfileCode.Unknown);

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

## [03]-[STEEL_LIFECYCLE]

- Owner: `SteelImport` owns source normalization, parse, header-before-feature admission, contour rounding, and topology derivation.
- Law: parse and source effects remain deferred on `Eff`; independent feature faults accumulate on `Validation<Error, Seq<SteelFeature>>` and collapse once into the ingress pipeline.
- Entry: `SteelImport.Read(SteelSource, SteelContourPolicy)` normalizes every source to stable bytes before `DstvReader.ParseAsync` runs; both arguments arrive ADMITTED, so a null guard at the entry is refuted ceremony the type system already carries.
- Auto: the header admits before any feature, so profile-code face admissibility gates each located element; DSTV block positions are one-based and the ordinal converts once, so no fault site can mint the line-zero locus `SourceKind.Steel` refuses; an outer contour orients counter-clockwise on the same path that admits it.
- Exemption: `Corner` and `Rounded` are the named contour statement kernel — the tangent construction IS the fillet law, and each guard names the geometric condition it refuses.
- Boundary: path cancellation remains source data; one `Fault` mint floors every locus at the `ST` line so `SourceKind.Steel` admits it, and every unreadable block fails with its block key and one-based line.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SteelImport {
    private const int HeaderLine = 1;
    private const int FirstFeatureLine = HeaderLine + 1;

    public static Eff<ImportedSteel> Read(
        SteelSource source, SteelContourPolicy policy) =>
        from bytes in Payload(source)
        from parsed in Parse(bytes)
        from result in Admit(parsed, bytes, policy).ToEff()
        select result;

    private static Eff<byte[]> Payload(SteelSource source) =>
        source.Switch(
                path: static path => liftEff(() => HostEdge.Captured(
                    async execution => Fin.Succ(
                        await File.ReadAllBytesAsync(path.Value, execution).ConfigureAwait(false)),
                    token: path.Cancellation).AsTask())
                    .MapFail(error => Classify(Path.GetFileName(path.Value), error)),
                text: static text => Eff.lift(() => Encoding.UTF8.GetBytes(text.Value)),
                bytes: static bytes => Eff.lift(() => bytes.Value.ToArray()));

    private static Eff<IDstv> Parse(byte[] bytes) =>
        liftEff(() => HostEdge.Captured(async _ => {
            using MemoryStream stream = new(bytes, writable: false);
            using TextReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
            return Fin.Succ<IDstv>(await new DstvReader().ParseAsync(reader).ConfigureAwait(false));
        }).AsTask()).MapFail(static error => error.Exception
            .Bind(static exception => Optional(exception as ParseException))
            .Match(
                Some: parsed => Fault(SteelParseKind.Classify(parsed).Key, parsed.LineNumber ?? HeaderLine, error),
                None: () => error));

    private static Fin<ImportedSteel> Admit(
        IDstv document, byte[] bytes, SteelContourPolicy policy) =>
        from source in Optional(document.Header).ToFin(Fault(SteelBlockKind.St.Key, HeaderLine, "steel-header:missing"))
        from header in Header(source, policy.Drawings)
        from features in Features(document.Elements, header, policy).ToFin()
        from topology in TopologyOf(features)
        from part in SteelPart.Admit(header, features, topology)
        select new ImportedSteel(part, ContentKey.Of(EgressKind.Nc1, bytes));

    private static Fin<SteelHeader> Header(IDstvHeader source, NamingStandard drawings) =>
        Try.lift(() => Fin.Succ(DstvMap.Header(source))).Run().Bind(static inner => inner)
            .Bind(row => SteelHeader.Admit(row, drawings));

    private static Fin<RegionTopology> TopologyOf(Seq<SteelFeature> features) {
        Seq<(SteelBlockKind Block, Loop Loop)> regions = features
            .Choose(static feature => feature is SteelFeature.Boundary { Contour: { Block: var block, Loop: var loop } }
                ? Some((Block: block, Loop: loop))
                : None);
        Seq<Loop> outers = regions.Filter(static row => row.Block.TopologySign > 0).Map(static row => row.Loop);
        Seq<Loop> holes = regions.Filter(static row => row.Block.TopologySign < 0).Map(static row => row.Loop);
        PolygonOp operation = holes.IsEmpty
            ? new PolygonOp.Topology(outers, PolygonFill.NonZero)
            : new PolygonOp.Boolean(outers, holes, BooleanOp.Difference, PolygonFill.NonZero);
        return outers.IsEmpty
            ? Fin.Fail<RegionTopology>(Fault(SteelBlockKind.Ak.Key, HeaderLine, "steel-topology:outer-missing"))
            : PolygonAlgebra.Apply(operation).Bind(static trace => trace.Regioned(
                new KernelFault.InvalidValue("steel", "steel-topology:projection")));
    }

    private static Validation<Error, Seq<SteelFeature>> Features(
        IEnumerable<DstvElement> elements,
        SteelHeader header,
        SteelContourPolicy policy) =>
        toSeq(elements)
            .Map(static (element, ordinal) => (Element: element, Line: ordinal + FirstFeatureLine))
            .Traverse(row => Feature(row.Element, row.Line, header, policy).ToValidation()).As();

    private static Fin<SteelFeature> Feature(DstvElement element, int line, SteelHeader header, SteelContourPolicy policy) =>
        element switch {
            DstvSlot slot => Capture(() => DstvMap.Slot(slot), SteelBlockKind.Bo, line, header),
            DstvHole hole => Capture(() => DstvMap.Hole(hole), SteelBlockKind.Bo, line, header),
            DstvCut cut => Capture(() => DstvMap.Cut(cut), SteelBlockKind.Sc, line, header),
            DstvNumeration numeration => Capture(() => DstvMap.Numeration(numeration), SteelBlockKind.Si, line, header),
            DstvBend => Fin.Fail<SteelFeature>(Fault(SteelBlockKind.Ka.Key, line, "steel-feature:bend-unsupported")),
            Contour contour => SteelBlockKind.Of(contour.ContourType)
                .ToFin(Fault(SteelBlockKind.Unknown.Key, line, "steel-contour:block-unknown"))
                .Bind(block => ContourOf(contour, block, line, header, policy)),
            _ => Fin.Fail<SteelFeature>(Fault(SteelBlockKind.Unknown.Key, line, "steel-feature:unsupported")),
        };

    private static Fin<SteelFeature> Capture(Func<SteelFeature> mapping, SteelBlockKind block, int line, SteelHeader header) =>
        Try.lift(() => Fin.Succ(mapping())).Run().Bind(static inner => inner)
            .Bind(feature => Valid(feature, block, line, header));

    private static Fin<SteelFeature> Valid(SteelFeature feature, SteelBlockKind block, int line, SteelHeader header) =>
        feature.Switch(
            state: header,
            hole: static (row, hole) => Faced(row, hole.Face) && ValidPoint(hole.Center)
                && Positive(hole.Diameter) && Nonnegative(hole.Depth),
            slot: static (row, slot) => Faced(row, slot.Face) && ValidPoint(slot.Center)
                && Positive(slot.Diameter) && Nonnegative(slot.Depth) && Positive(slot.Span) && Positive(slot.Width)
                && slot.Span >= slot.Width && Finite(slot.Rotation),
            cut: static (row, cut) => Faced(row, cut.Face) && ValidPoint(cut.At),
            numeration: static (row, numeration) => Faced(row, numeration.Face) && ValidPoint(numeration.At),
            boundary: static (row, boundary) => Faced(row, boundary.Contour.Face),
            marking: static (row, marking) => Faced(row, marking.Contour.Face))
            ? Fin.Succ(feature)
            : Fin.Fail<SteelFeature>(Fault(block.Key, line, "steel-feature:invalid"));

    private static bool Faced(SteelHeader header, SteelFace face) => header.ProfileCode.Admits(face);

    private static Fin<SteelFeature> ContourOf(
        Contour contour,
        SteelBlockKind block,
        int line,
        SteelHeader header,
        SteelContourPolicy policy) =>
        SteelFace.Of(contour.FlCode).ToFin(Fault(block.Key, line, "steel-contour:face")).Bind(face => Try.lift(() => Fin.Succ((
            Face: face,
            Vertices: toSeq(contour.Points).Map(static point => point switch {
                DstvSkewedPoint skew => DstvMap.Vertex(skew) with {
                    Bevel = Some(new SteelBevel(
                        DstvMap.Degrees(skew.FirstAngle), DstvMap.Millimeters(skew.FirstBlunting),
                        DstvMap.Degrees(skew.SecondAngle), DstvMap.Millimeters(skew.SecondBlunting))),
                },
                _ => DstvMap.Vertex(point),
            }).ToArr()))).Run().Bind(static inner => inner)
        .Bind(active => Faced(header, active.Face)
            ? Rounded(active.Vertices, policy, block, line)
                .Map(loop => block.TopologySign > 0 ? loop.AsCcw() : loop)
                .Map(loop => block.Boundary
                    ? (SteelFeature)new SteelFeature.Boundary(new SteelContour(block, active.Face, loop, active.Vertices))
                    : new SteelFeature.Marking(new SteelContour(block, active.Face, loop, active.Vertices)))
            : Fin.Fail<SteelFeature>(Fault(block.Key, line, "steel-contour:face-profile"))));

    private static Fin<Loop> Rounded(Arr<SteelVertex> vertices, SteelContourPolicy policy, SteelBlockKind block, int line) =>
        vertices.Count < 3
            ? Fin.Fail<Loop>(Fault(block.Key, line, "steel-contour:vertices"))
            : toSeq(Range(0, vertices.Count)).Traverse(index => Corner(vertices, index, policy, block, line)).As()
                .Bind(corners => toSeq(Range(0, vertices.Count)).Exists(index => {
                    int next = (index + 1) % vertices.Count;
                    Vector3d edge = vertices[next].At - vertices[index].At;
                    Vector3d straight = corners[next].Enter - corners[index].Exit;
                    return straight.Length <= policy.Tolerance.Absolute.Value || (edge * straight) <= 0.0;
                })
                    ? Fin.Fail<Loop>(Fault(block.Key, line, "steel-contour:edge"))
                    : Fin.Succ(corners.Bind(corner => corner.Enter.DistanceTo(corner.Exit) <= policy.Tolerance.Absolute.Value
                        ? Seq((At: corner.Enter, Bulge: 0.0))
                        : Seq((At: corner.Enter, corner.Bulge), (At: corner.Exit, Bulge: 0.0)))))
                .Bind(spans => Loop.Admit(
                    spans.Map(static span => span.At).ToArr(),
                    closed: true,
                    spans.Map(static span => span.Bulge).ToArr(),
                    policy.Tolerance));

    private static Fin<(Point3d Enter, double Bulge, Point3d Exit)> Corner(
        Arr<SteelVertex> vertices,
        int index,
        SteelContourPolicy policy,
        SteelBlockKind block,
        int line) {
        SteelVertex vertex = vertices[index];
        double radius = vertex.Radius.As(LengthUnit.Millimeter);
        if (!ValidPoint(vertex.At) || !double.IsFinite(radius) || radius < 0.0 || !ValidBevel(vertex.Bevel))
            return Fin.Fail<(Point3d, double, Point3d)>(Fault(block.Key, line, "steel-corner:vertex"));
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
            return Fin.Fail<(Point3d, double, Point3d)>(Fault(block.Key, line, "steel-corner:leg"));
        Vector3d towardPrevious = incoming / incomingLength;
        Vector3d towardNext = outgoing / outgoingLength;
        double theta = Vector3d.VectorAngle(towardPrevious, towardNext);
        double tangent = radius / Math.Tan(theta / 2.0);
        double sign = Math.Sign(Vector3d.CrossProduct(-towardPrevious, towardNext).Z);
        double angular = policy.AngularTolerance.As(AngleUnit.Radian);
        return !double.IsFinite(theta) || theta <= angular || (Math.PI - theta) <= angular
            || !double.IsFinite(tangent) || tangent <= 0.0 || tangent >= incomingLength || tangent >= outgoingLength || sign == 0.0
                ? Fin.Fail<(Point3d, double, Point3d)>(Fault(block.Key, line, "steel-corner:tangent"))
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

    private static bool Positive(Length value) => ValidityClaim.Positive(value.As(LengthUnit.Millimeter));

    private static bool Nonnegative(Length value) =>
        double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) >= 0.0;

    private static bool Finite(Angle value) => double.IsFinite(value.As(AngleUnit.Radian));

    private static Error Fault(string block, int line, string detail) =>
        FabricationFault.Sourced(new SourceLocus.DstvBlock(block, Math.Max(line, HeaderLine)), detail);

    private static Error Fault(string block, int line, Error cause) =>
        FabricationFault.Unavailable(
            new SourceLocus.DstvBlock(block, Math.Max(line, HeaderLine)), cause.Message, cause);

    private static Error Classify(string block, Error error) => error.Exception
        .Filter(static raised => raised is IOException or UnauthorizedAccessException)
        .Map(_ => Fault(block, HeaderLine, error))
        .IfNone(error);
}
```

## [04]-[PROJECTION_EGRESS]

- Owner: `SteelView` is the closed egress row carrying its own projection delegate, and `SteelProjection` carries each row's result shape.
- Cases: part · boundaries · preparations · features · placements · topology · identity.
- Entry: `SteelView.<row>.Project(ImportedSteel)` — the row IS the dispatch, so no request family and no total `Switch` restate the egress roster.
- Growth: a new egress is one `SteelView` row carrying its delegate and one `SteelProjection` case.
- Boundary: projection returns settled evidence alone and opens no writer; NC1 emission is `Posting/dialect` work over the same `DstvMap` table this page owns.

```csharp
// --- [PROJECTION_EGRESS] ---------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SteelProjection {
    private SteelProjection() { }

    public sealed record Part(SteelPart Value) : SteelProjection;
    public sealed record Boundaries(Arr<Loop> Value) : SteelProjection;
    public sealed record Preparations(Arr<EdgePreparation> Value) : SteelProjection;
    public sealed record Features(Seq<SteelFeature> Value) : SteelProjection;
    public sealed record Placements(Seq<SteelPlacement> Value) : SteelProjection;
    public sealed record Topology(RegionTopology Value) : SteelProjection;
    public sealed record Identity(ContentKey Value) : SteelProjection;
}

[SmartEnum<string>]
public sealed partial class SteelView {
    public static readonly SteelView Part = new("part",
        static result => new SteelProjection.Part(result.Part));
    public static readonly SteelView Boundaries = new("boundaries",
        static result => new SteelProjection.Boundaries(result.Part.Loops));
    public static readonly SteelView Preparations = new("preparations",
        static result => new SteelProjection.Preparations(result.Part.Preparations));
    public static readonly SteelView Features = new("features",
        static result => new SteelProjection.Features(result.Part.Features));
    public static readonly SteelView Placements = new("placements",
        static result => new SteelProjection.Placements(result.Part.Placed));
    public static readonly SteelView Topology = new("topology",
        static result => new SteelProjection.Topology(result.Part.Topology));
    public static readonly SteelView Identity = new("identity",
        static result => new SteelProjection.Identity(result.Key));

    [UseDelegateFromConstructor]
    public partial SteelProjection Project(ImportedSteel result);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
