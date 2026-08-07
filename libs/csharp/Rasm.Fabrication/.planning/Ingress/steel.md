# [RASM_FABRICATION_STEEL_IMPORT]

`SteelImport` owns DSTV/NC1 admission from path, text, or bytes into one fabrication steel owner. Every source preserves its received bytes for `ContentKey`, every fault carries a positive DSTV line the `SourceKind.Steel` locus gate admits, and every admitted feature leaves provider types at the boundary.

`SteelPart` carries the complete `ST` descriptor, recognized feature family, arc-aware contour measures, and `AK` minus `IK` region hierarchy. DSTV face-local coordinates resolve into part space through the `SteelFace` frame rows, so a downstream plane consumes placed geometry rather than a face tag it must interpret. `DstvMap` is the folder's Mapperly benchmark and the ONE provider transcription: it carries the header row and every feature record, so the twenty-four-position admission call site collapses to one row argument and `Posting/dialect` `Nc1Canonical` composes the SAME table as its inverse rather than restating twenty-six positions. `SteelView` parameterizes downstream projection without opening a writer; NC1 emission remains `PostDialect` work.

## [01]-[INDEX]

- [02]-[STEEL_EXCHANGE]: `SteelSource` path, text, and byte ingress preserving received bytes, the `SteelProfileCode`/`SteelFace`/`SteelBlockKind`/`SteelParseKind` DSTV vocabularies carrying face admissibility, contour correspondence, topology sign, and exception-type classification, the admitted `SteelHeader`/`SteelPart` owners, and `DstvMap` the one provider transcription.
- [03]-[STEEL_LIFECYCLE]: `SteelImport.Read` admitting one `SteelPart` over `SteelHeader` and `SteelFeature` into a `SteelImportReceipt`, header-before-feature admission over stable bytes, and deferred `Eff` parse effects accumulating independent feature faults on `Validation`.
- [04]-[PROJECTION_EGRESS]: `SteelView` selecting part, boundary, preparation, feature, placement, topology, or identity egress through one generated behavior row, and `SteelProjection` carrying each result shape.

## [02]-[STEEL_EXCHANGE]

- Owner: `SteelPart` owns the normalized header, operations, contours, placement, and identity; `SteelFace` owns the DSTV placement convention per row; `SteelBlockKind` owns statement identity, contour correspondence, and topology sign; `DstvMap` owns every provider-to-owned transcription.
- Cases: `SteelSource` closes path, text, and byte ingress; `SteelFeature` closes every readable DSTV feature payload; `SteelParseKind` closes the parser exception hierarchy by most-derived row.
- Law: the header admits from ONE `SteelHeaderRow` argument. Twenty-four positional arguments at a call site make a transposed pair — a flange width where a flange thickness belongs — invisible to the compiler and to every reader; the row names each column once at the mapper that lifts it.
- Law: `DstvMap` is the ONE table. `Posting/dialect` `Nc1Canonical.Header` is its exact inverse and composes it through `[IncludeMappingConfiguration]`, so the header correspondence is stated once and a round trip becomes a build fact rather than two rosters that drift.
- Auto: generated owners validate policy, header, and aggregate values; `SteelBlockKind` supplies statement identity, contour correspondence, and topology sign; `SteelParseKind` classifies a `ParseException` by inheritance depth so declaration order is free; `SteelProfileCode.Admits` gates each located element's face before any geometry is built.
- Receipt: `SteelPart.Topology` preserves outer, hole, parent, depth, area, and bounds evidence; `SteelPart.Placed` resolves each face-local feature into part coordinates with contour bulges beside transformed vertices; `SteelPart.Preparations` publishes the per-edge groove demand the skewed contour points state, keyed on the boundary ordinal a run's profile column shares.
- Packages: `DSTV.Net` owns asynchronous parsing; `Riok.Mapperly` owns field transcription; `Thinktecture.Runtime.Extensions` owns cases and policy rows; `LanguageExt.Core` owns effects, accumulation, and immutable carriers; `UnitsNet` owns physical values; `Loop` composes `CavalierContours` for arc measures; `PolygonAlgebra` composes `Clipper2` for hierarchy and fill.
- Growth: a readable block lands as one `SteelFeature` case, one `SteelBlockKind` row, and one Mapperly declaration; a parser fault lands as one `SteelParseKind` row; a profile or face convention lands as one `SteelProfileCode` or `SteelFace` row; a new source or view lands as one generated case or row.
- Boundary: `DstvBend` remains a typed `KA` rejection until its complete payload is publicly readable; face frames derive wholly from the admitted header so a convention correction is one row; a `Get` that throws on an unlisted DSTV code rides the transcription boundary's own `Try` capture, so the throw lands as the block-and-line fault rather than escaping the rail; `ToSvg()` remains outside fabrication projection.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
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
            .Fold(Option<SteelParseKind>.None, static (best, kind) =>
                best.Filter(held => Depth(held.ExceptionType) >= Depth(kind.ExceptionType)).IsSome ? best : Some(kind))
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

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SteelContourPolicy {
    public Context Tolerance { get; }
    public Length MinimumLeg { get; }
    public Angle AngularTolerance { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Context tolerance,
        ref Length minimumLeg,
        ref Angle angularTolerance) {
        double leg = minimumLeg.As(LengthUnit.Millimeter);
        double angle = angularTolerance.As(AngleUnit.Radian);
        if (!Witness.Positive(leg) || !Witness.Positive(angle) || angle >= Math.PI / 2.0)
            validationError = IngressFault.Policy("steel-contour-policy:domain");
    }

    public static Fin<SteelContourPolicy> Admit(Context tolerance, Length minimumLeg, Angle angularTolerance) =>
        Validate(tolerance, minimumLeg, angularTolerance, out SteelContourPolicy policy).Admitted(policy);

    public static Fin<SteelContourPolicy> Canonical(Context tolerance) => Admit(
        tolerance,
        Length.FromMillimeters(tolerance.Absolute.Value),
        Angle.FromRadians(tolerance.Angle.Value));
}

// The transcription target: every ST column lifted onto its canonical unit ONCE, in provider order. Admission takes
// this row, so the call site names one argument rather than twenty-four positions where a transposed pair — a
// flange width seated where a flange thickness belongs — type-checks silently.
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
[ValidationError<FabricationFault>]
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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
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
        orderIdentification = orderIdentification.Trim();
        drawingIdentification = drawingIdentification.Trim();
        phaseIdentification = phaseIdentification.Trim();
        pieceIdentification = pieceIdentification.Trim();
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
        // Each slot names the invariant it decides, so a rejected header is addressable at its own locus rather
        // than through one aggregate message a caller has to parse.
        Seq<(string Slot, bool Admits)> slots = [
            ("identity", Witness.Keyed(pieceIdentification) && Witness.Keyed(profile) && Witness.Keyed(steelQuality)),
            ("quantity", quantityOfPieces > 0),
            ("extent", extent.ForAll(Witness.Positive)),
            ("section", section.ForAll(static value => double.IsFinite(value) && value >= 0.0)),
            ("end-cut", angles.ForAll(double.IsFinite))];
        validationError = slots
            .Find(static slot => !slot.Admits)
            .Match<FabricationFault?>(
                Some: static slot => IngressFault.Policy($"steel-header:{slot.Slot}"),
                None: static () => null);
    }

    public static Fin<SteelHeader> Admit(SteelHeaderRow row) => Validate(
        row.OrderIdentification, row.DrawingIdentification, row.PhaseIdentification, row.PieceIdentification,
        row.QuantityOfPieces, row.Profile, row.ProfileCode, row.SteelQuality,
        row.Length, row.SawLength, row.ProfileHeight, row.FlangeWidth, row.FlangeThickness, row.WebThickness,
        row.Radius, row.WebStartCut, row.WebEndCut, row.FlangeStartCut, row.FlangeEndCut,
        row.WeightByMeter, row.PaintingSurfaceByMeter,
        // A `[ComplexValueObject]` `Validate` takes its members and the out slot alone — the `IFormatProvider`
        // parameter belongs to the keyed `[ValueObject<T>]` arity and does not exist on this generator.
        row.Text1InfoOnPiece, row.Text2InfoOnPiece, row.Text3InfoOnPiece, row.Text4InfoOnPiece,
        out SteelHeader header).Admitted(header);
}

public sealed record SteelBevel(Angle FirstAngle, Length FirstBlunting, Angle SecondAngle, Length SecondBlunting);

public sealed record SteelVertex(Point3d At, bool IsNotch, Length Radius, Option<SteelBevel> Bevel);

// The per-edge preparation demand DSTV states. A skewed contour point carries the groove its edge is cut to, and a
// receipt that kept it only inside the raw vertex left every downstream plane reading square-edged loops and the
// source it never receives. `Profile` indexes `SteelPart.Loops` — the same ordinal the run's profile column carries —
// and the locus is the SOURCE vertex, because corner rounding splits an apex into two loop vertices and an index
// correspondence across that split would be false.
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
[ValidationError<FabricationFault>]
public sealed partial class SteelPart {
    public SteelHeader Header { get; }
    public Seq<SteelFeature> Features { get; }
    public TopologyReceipt Topology { get; }

    // Each projection reads its OWN case; a shared boolean-parameterized reader made the caller supply a flag the
    // case discriminant already carries and made both reads look like one operation with two modes.
    [IgnoreMember]
    public Seq<SteelContour> Boundaries => Features
        .Choose(static feature => feature is SteelFeature.Boundary row ? Some(row.Contour) : None);

    [IgnoreMember]
    public Seq<SteelContour> Markings => Features
        .Choose(static feature => feature is SteelFeature.Marking row ? Some(row.Contour) : None);

    [IgnoreMember]
    public Arr<Loop> Loops => Boundaries.Map(static contour => contour.Loop).ToArr();

    // Boundary ordinal is the loop ordinal, so a demand keys onto the profile column a run admits without a second
    // correspondence; a contour whose vertices state no groove contributes no row rather than an empty one.
    [IgnoreMember]
    public Arr<EdgePreparation> Preparations => Boundaries
        .Map(static (contour, profile) => (Contour: contour, Profile: profile))
        .Bind(static row => row.Contour.Vertices.ToSeq().Choose(vertex => vertex.Bevel
            .Map(bevel => new EdgePreparation(row.Profile, vertex.At, row.Contour.Face, bevel))))
        .ToArr();

    // Face-local DSTV coordinates only become part geometry through the header, so placement lives with the aggregate
    // that owns both and never with the feature case that carries the bare face tag.
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

    public static Fin<SteelPart> Admit(SteelHeader header, Seq<SteelFeature> features, TopologyReceipt topology) =>
        Validate(header, features, topology, out SteelPart part).Admitted(part);
}

public sealed record SteelImportReceipt(SteelPart Part, ContentKey Key, int SourceBytes);

// --- [BOUNDARIES] ---------------------------------------------------------------------------------------------------------------------------------
// The ONE provider transcription. `Posting/dialect` `Nc1Canonical.Header` composes this configuration as its exact
// inverse, so the twenty-six-column NC1 header correspondence is declared once and a write-then-read round trip is
// a build fact rather than two rosters that drift apart.
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

    // Get throws on an unlisted DSTV face or profile code; every call site rides a `Try` capture at the transcription
    // boundary, so the throw lands as the block-and-line fault rather than escaping the rail.
    [UserMapping]
    internal static SteelFace Face(string code) => SteelFace.Get(code.Trim().ToUpperInvariant());

    [UserMapping]
    internal static SteelProfileCode Profile(char code) =>
        SteelProfileCode.Get(code.ToString().Trim().ToUpperInvariant());

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
- Law: parse and source effects remain deferred on `Eff`; independent feature faults accumulate on `Validation<Error, Seq<SteelFeature>>` and collapse once into the ingress rail.
- Entry: `SteelImport.Read(SteelSource, SteelContourPolicy)` normalizes every source to stable bytes before `DstvReader.ParseAsync` runs; both arguments arrive ADMITTED, so a null guard at the entry is refuted ceremony the type system already carries.
- Auto: the header admits before any feature, so profile-code face admissibility gates each located element; DSTV block positions are one-based and the ordinal converts once, so no fault site can mint the line-zero locus `SourceKind.Steel` refuses; an outer contour orients counter-clockwise on the same rail that admits it.
- Exemption: `Corner` and `Rounded` are the named contour statement kernel — the tangent construction IS the fillet law, and each guard names the geometric condition it refuses.
- Boundary: path cancellation remains source data; one `Fault` mint floors every locus at the `ST` line so `SourceKind.Steel` admits it, and every unreadable block fails with its block key and one-based line.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class SteelImport {
    private const int HeaderLine = 1;
    private const int FirstFeatureLine = HeaderLine + 1;

    public static Eff<SteelImportReceipt> Read(SteelSource source, SteelContourPolicy policy) =>
        from bytes in Payload(source)
        from parsed in Parse(bytes)
        from receipt in Admit(parsed, bytes, policy).ToEff()
        select receipt;

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
        from part in SteelPart.Admit(header, features, topology)
        select new SteelImportReceipt(part, ContentKey.Of(EgressKind.Nc1, bytes), bytes.Length);

    // The transcription runs inside one `Try`: `Profile` and `Face` resolve through generated `Get`, which throws on
    // an unlisted DSTV code, and the capture lands that throw as the ST-block fault the locus gate admits.
    private static Fin<SteelHeader> Header(IDstvHeader source) =>
        Try.lift(() => DstvMap.Header(source))
            .Run()
            .MapFail(static _ => Fault(SteelBlockKind.St.Key, HeaderLine))
            .Bind(SteelHeader.Admit)
            .MapFail(static _ => Fault(SteelBlockKind.St.Key, HeaderLine));

    private static Fin<TopologyReceipt> TopologyOf(Seq<SteelFeature> features) {
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
            ? Fin.Fail<TopologyReceipt>(Fault(SteelBlockKind.Ak.Key, HeaderLine))
            : PolygonAlgebra.Apply(operation).Bind(static trace => trace is PolygonTrace.Regions result
                ? Fin.Succ(result.Result)
                : Fin.Fail<TopologyReceipt>(Fault(SteelBlockKind.Ak.Key, HeaderLine)));
    }

    // DSTV block positions are one-based; the ordinal converts once here so no fault site can mint the line-zero
    // locus `SourceKind.Steel` refuses.
    private static Validation<Error, Seq<SteelFeature>> Features(
        IEnumerable<DstvElement> elements,
        SteelHeader header,
        SteelContourPolicy policy) =>
        toSeq(elements)
            .Map(static (element, ordinal) => (Element: element, Line: ordinal + FirstFeatureLine))
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
                // `AsCcw` returns a Loop, never a rail: re-orientation preserves every admitted invariant, so an
                // outer contour orients in place rather than through a second admission that could refuse.
                .Map(loop => block.TopologySign > 0 ? loop.AsCcw() : loop)
                .Map(loop => block.Boundary
                    ? (SteelFeature)new SteelFeature.Boundary(new SteelContour(block, active.Face, loop, active.Vertices))
                    : new SteelFeature.Marking(new SteelContour(block, active.Face, loop, active.Vertices)))
            : Fin.Fail<SteelFeature>(Fault(block.Key, line)));

    private static Fin<Loop> Rounded(Arr<SteelVertex> vertices, SteelContourPolicy policy, SteelBlockKind block, int line) =>
        vertices.Count < 3
            ? Fin.Fail<Loop>(Fault(block.Key, line))
            : toSeq(Range(0, vertices.Count)).Traverse(index => Corner(vertices, index, policy, block, line)).As()
                .Bind(corners => toSeq(Range(0, vertices.Count)).Exists(index => {
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

    private static bool Positive(Length value) => Witness.Positive(value.As(LengthUnit.Millimeter));

    private static bool Nonnegative(Length value) =>
        double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) >= 0.0;

    private static bool Finite(Angle value) => double.IsFinite(value.As(AngleUnit.Radian));

    // SourceKind.Steel admits a DstvBlock only on a positive line, so the one mint floors every locus at the ST block.
    private static Error Fault(string block, int line) =>
        FabricationFault.Sourced(new SourceLocus.DstvBlock(block, Math.Max(line, HeaderLine)));
}
```

## [04]-[PROJECTION_EGRESS]

- Owner: `SteelView` is the closed egress row carrying its own projection delegate, and `SteelProjection` carries each row's result shape.
- Cases: part · boundaries · preparations · features · placements · topology · identity.
- Entry: `SteelView.<row>.Project(SteelImportReceipt)` — the row IS the dispatch, so no request family and no total `Switch` restate the egress roster.
- Growth: a new egress is one `SteelView` row carrying its delegate and one `SteelProjection` case.
- Boundary: projection returns settled evidence alone and opens no writer; NC1 emission is `Posting/dialect` work over the same `DstvMap` table this page owns.

```csharp signature
// --- [PROJECTION_EGRESS] --------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SteelProjection {
    private SteelProjection() { }

    public sealed record Part(SteelPart Value) : SteelProjection;
    public sealed record Boundaries(Arr<Loop> Value) : SteelProjection;
    public sealed record Preparations(Arr<EdgePreparation> Value) : SteelProjection;
    public sealed record Features(Seq<SteelFeature> Value) : SteelProjection;
    public sealed record Placements(Seq<SteelPlacement> Value) : SteelProjection;
    public sealed record Topology(TopologyReceipt Value) : SteelProjection;
    public sealed record Identity(ContentKey Value) : SteelProjection;
}

[SmartEnum<string>]
public sealed partial class SteelView {
    public static readonly SteelView Part = new("part",
        static receipt => new SteelProjection.Part(receipt.Part));
    public static readonly SteelView Boundaries = new("boundaries",
        static receipt => new SteelProjection.Boundaries(receipt.Part.Loops));
    public static readonly SteelView Preparations = new("preparations",
        static receipt => new SteelProjection.Preparations(receipt.Part.Preparations));
    public static readonly SteelView Features = new("features",
        static receipt => new SteelProjection.Features(receipt.Part.Features));
    public static readonly SteelView Placements = new("placements",
        static receipt => new SteelProjection.Placements(receipt.Part.Placed));
    public static readonly SteelView Topology = new("topology",
        static receipt => new SteelProjection.Topology(receipt.Part.Topology));
    public static readonly SteelView Identity = new("identity",
        static receipt => new SteelProjection.Identity(receipt.Key));

    [UseDelegateFromConstructor]
    public partial SteelProjection Project(SteelImportReceipt receipt);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
