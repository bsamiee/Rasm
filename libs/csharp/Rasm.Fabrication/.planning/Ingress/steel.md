# [RASM_FABRICATION_STEEL_IMPORT]

The DSTV NC1 steel-ingress boundary admits an `.nc1` profile program through `DSTV.Net` and projects the complete 25-field `ST` header plus verified `BO`/`SI`/`SC`/`AK`/`IK`/`KO`/`PU` payloads into fabrication-owned rows. `DstvReader.ParseAsync` owns parse admission, `ParseException.LineNumber` lowers to `FabricationFault.IngressTranslation(SourceKind.Steel, SourceLocus.DstvBlock(...))`, and no provider type crosses the boundary. `Riok.Mapperly` owns provider-record transcription while `SteelImport` owns feature dispatch, contour topology, `Loop.AsCcw`, and `ContentKey.Of(EgressKind.Nc1, ...)`.

A radiused `AK`/`IK` contour point lowers arc-exactly: the vertex becomes a validated tangent pair and one signed bulge span. `SteelContourPolicy` carries the canonical `Context` and rejects zero-length legs, collinear corners, oversized radii, and non-finite tangent geometry before `Loop.Admit`. A bevelled `DstvSkewedPoint` carries both angle/blunting pairs on `SteelBevel`. `KO`/`PU` contours remain marking features and never enter the boundary-loop library.

The imported profile is the same `Loop` library consumed by nesting and toolpath. `DSTV.Net` recognizes `KA` as `DstvBend` but every payload field (`_originX`/`_originY`/`_finishX`/`_finishY`/`_bendingAngle`/`_bendingRadius`) is private with no public accessor or `Deconstruct`; a `KA` element therefore routes the typed `KA` block fault instead of fabricating geometry from unreachable members. A future `KA` admission requires a package surface exposing the complete bend record, and its analytic lowering remains owned by `Forming/sheet`. NC1 emission remains the posting dialect owner.

Wire posture: HOST-LOCAL, HOST-NEUTRAL. `DSTV.Net` is pure managed input material, `Mapperly` is compile-time transcription, and the public ingress entry remains `Ingress.Admit(IngressSource)` through its `Steel` arm — the fold itself is declared ONCE on `Ingress/profile#PROFILE_IMPORT` and this page supplies only the arm body. A second steel reader, a second `Ingress` fold declaration, a hand-written DSTV record copyist, a direct `DstvElement` dependency in nesting/toolpath/forming, or a local NC1 writer is the rejected shape.

## [01]-[INDEX]

- [01]-[STEEL_IMPORT]: owns `SteelImport.Read`, the `IngressSource.Steel` arm, `SteelContourPolicy`, the local `SteelHeader`/`SteelPart`/`SteelFeature`/`SteelContour`/`SteelBevel` model, the Mapperly `DstvMap` seam, parse and recognized-unsupported block fault lowering, validated corner-fillet bulges, and `AK`/`IK` loop projection.

## [02]-[STEEL_IMPORT]

- Owner: `SteelImport` owns read admission; `SteelBlockKind` owns diagnostic codes; `SteelHeader` carries all 25 public `ST` fields; `SteelFeature` closes over hole, slot, cut, numeration, contour, and marking; `SteelPart` stores only `Features` and derives every per-kind view; `SteelContourPolicy` carries the canonical `Context`, minimum-leg, and angular-degeneracy bounds; `DstvMap` owns transcription.
- Cases: `ST` maps to `SteelHeader`; `DstvSlot` precedes `DstvHole`; `SI` maps to numeration; `SC` maps to cut; `AK`/`IK` map to boundary contours; `KO`/`PU` map to markings; `DstvSkewedPoint` precedes its base point and maps both bevel pairs. `DstvBend` is recognized but fails at the `KA` locus because its public record carries no usable bend payload, and an unrecognized `DstvElement` subtype fails at the `UNKNOWN` locus.
- Entry: `Fin<SteelImportReceipt> SteelImport.Read(string path, SteelContourPolicy policy)` reads stable identity bytes, parses through the one sync bridge, traverses every recognized feature on `Fin`, validates contour rounding, and returns the receipt. `Ingress.Admit` carries `SteelContourPolicy` on the source case and returns the complete receipt as `AdmittedGeometry.Steel` — header, features, bevels, and content key cross the fold intact.
- Auto: `ParseException.LineNumber` lowers to `ST`; `Try.lift` captures non-parse failures; missing header lowers to `ST:0`; empty boundary contours lower to `AK:0`; `KA` lowers at its parsed ordinal; an unknown provider subtype lowers to `UNKNOWN:<ordinal>`. The feature traversal admits every catalogued element or aborts, adjacent corner radii must leave a forward, tolerance-long straight span on their shared leg, and an `IsNotch` point inverts its bulge sign — the notch arc bows into the material where the fillet arc bows away.
- Receipt: `SteelImportReceipt` carries `SteelPart` plus `ContentKey.Of(EgressKind.Nc1, rawBytes)`. `SteelPart.Loops` is the bulge-carrying boundary library, `SteelPart.Features` the ONE stored feature family, every per-kind view a derived projection, and no `IDstv`, `DstvElement`, `Contour`, `DstvHole`, or `CodeProfile` leaks.
- Packages: `DSTV.Net` (`DstvReader.ParseAsync(TextReader)`, `IDstv.Header`/`Elements`, the 25 public `IDstvHeader` fields, `DstvElement`, `LocatedElement`, `DstvHole`, `DstvSlot`, `DstvCut`, `DstvBend` recognition only, `DstvNumeration`, `DstvContourPoint`, `DstvSkewedPoint`, `Contour.Points`, `ContourType`, `ParseException.LineNumber`); `Riok.Mapperly` (`[Mapper]`, `[MapProperty]`, `[MapPropertyFromSource]`, `[UserMapping]`); fabrication atoms and fault band; LanguageExt.Core; Thinktecture.Runtime.Extensions; BCL inbox.
- Growth: a new NC1 block is one feature case, block row, and Mapperly mapping. A package release exposing complete `KA` data admits one bend feature case and one analytic `Forming/sheet` seam; incomplete coordinate-only admission remains rejected. NC1 emission detail lands on the posting dialect.
- Boundary: `DSTV.Net` is read-only; `ToSvg()` is preview-only; Mapperly owns transcription but no topology, rail, identity, or policy. The feature family stores once. Recognized fabrication blocks never disappear through a default arm, and unsupported `KA` input never synthesizes a bend from header dimensions.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Text;
using DSTV.Net.Contracts;
using DSTV.Net.Data;
using DSTV.Net.Enums;
using DSTV.Net.Exceptions;
using DSTV.Net.Implementations;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Ingress;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SteelBlockKind {
    public static readonly SteelBlockKind St = new("ST");
    public static readonly SteelBlockKind Bo = new("BO");
    public static readonly SteelBlockKind Si = new("SI");
    public static readonly SteelBlockKind Sc = new("SC");
    public static readonly SteelBlockKind Ak = new("AK");
    public static readonly SteelBlockKind Ik = new("IK");
    public static readonly SteelBlockKind Ko = new("KO");
    public static readonly SteelBlockKind Pu = new("PU");
    public static readonly SteelBlockKind Ka = new("KA");
    public static readonly SteelBlockKind Unknown = new("UNKNOWN");
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record SteelHeader(
    string OrderIdentification,
    string DrawingIdentification,
    string PhaseIdentification,
    string PieceIdentification,
    int QuantityOfPieces,
    string Profile,
    string ProfileCode,
    string SteelQuality,
    double Length,
    double SawLength,
    double ProfileHeight,
    double FlangeWidth,
    double FlangeThickness,
    double WebThickness,
    double Radius,
    double WebStartCut,
    double WebEndCut,
    double FlangeStartCut,
    double FlangeEndCut,
    double WeightByMeter,
    double PaintingSurfaceByMeter,
    string Text1InfoOnPiece,
    string Text2InfoOnPiece,
    string Text3InfoOnPiece,
    string Text4InfoOnPiece);

public sealed record SteelBevel(double FirstAngleDeg, double FirstBlunting, double SecondAngleDeg, double SecondBlunting);

public sealed record SteelPoint(Point3d At, bool IsNotch, double RadiusMm, Option<SteelBevel> Bevel);

public sealed record SteelHole(Point3d Center, string Flange, double DiameterMm, double DepthMm);

public sealed record SteelSlot(
    Point3d Center,
    string Flange,
    double DiameterMm,
    double DepthMm,
    double SlotLengthMm,
    double SlotWidthMm,
    double SlotAngleDeg);

public sealed record SteelCut(Point3d At, string Flange);

public sealed record SteelNumeration(Point3d At, string Flange);

public sealed record SteelContour(SteelBlockKind Kind, Loop Loop, Arr<SteelPoint> Points);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SteelFeature {
    private SteelFeature() { }

    public sealed record Hole(SteelHole Row) : SteelFeature;
    public sealed record Slot(SteelSlot Row) : SteelFeature;
    public sealed record Cut(SteelCut Row) : SteelFeature;
    public sealed record Numeration(SteelNumeration Row) : SteelFeature;
    public sealed record Contour(SteelContour Row) : SteelFeature;
    public sealed record Marking(SteelContour Row) : SteelFeature;
}

public sealed record SteelPart(SteelHeader Header, Seq<SteelFeature> Features) {
    public Seq<SteelContour> Contours => Features.Choose(static f => f is SteelFeature.Contour c ? Some(c.Row) : None);
    public Arr<Loop> Loops => Contours.Map(static c => c.Loop).ToArr();
    public Seq<SteelContour> Markings => Features.Choose(static f => f is SteelFeature.Marking m ? Some(m.Row) : None);
    public Seq<SteelHole> Holes => Features.Choose(static f => f is SteelFeature.Hole h ? Some(h.Row) : None);
    public Seq<SteelSlot> Slots => Features.Choose(static f => f is SteelFeature.Slot s ? Some(s.Row) : None);
    public Seq<SteelCut> Cuts => Features.Choose(static f => f is SteelFeature.Cut c ? Some(c.Row) : None);
    public Seq<SteelNumeration> Numerations => Features.Choose(static f => f is SteelFeature.Numeration n ? Some(n.Row) : None);
}

public sealed record SteelImportReceipt(SteelPart Part, ContentKey Key);

public readonly record struct SteelContourPolicy(Context Tolerance, double MinimumLegMm, double AngularToleranceRad) {
    public static Fin<SteelContourPolicy> Canonical(Context tolerance) => Of(tolerance, minimumLegMm: 1e-6, angularToleranceRad: 1e-9);

    public static Fin<SteelContourPolicy> Of(Context tolerance, double minimumLegMm, double angularToleranceRad) =>
        double.IsFinite(minimumLegMm) && minimumLegMm > 0.0
        && double.IsFinite(angularToleranceRad) && angularToleranceRad > 0.0 && angularToleranceRad < Math.PI / 2.0
            ? Fin.Succ(new SteelContourPolicy(tolerance, minimumLegMm, angularToleranceRad))
            : Fin.Fail<SteelContourPolicy>(FabricationFault.IngressTranslation(
                SourceKind.Steel, new SourceLocus.DstvBlock(SteelBlockKind.Ak.Key, 0)).ToError());
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class SteelImport {
    public static Fin<SteelImportReceipt> Read(string path, SteelContourPolicy policy) =>
        Validate(policy).Bind(_ => Try.lift(() => File.ReadAllBytes(path)).Run()
            .MapFail(static _ => Fault(SteelBlockKind.St, 0))
            .Bind(raw => Parse(raw).Bind(dstv => Project(dstv, raw, policy))));

    static Fin<IDstv> Parse(byte[] raw) =>
        Try.lift(() => ParseSync(raw)).Run()
            .MapFail(static _ => Fault(SteelBlockKind.St, 0))
            .Bind(identity);

    static Fin<IDstv> ParseSync(byte[] raw) {
        using MemoryStream stream = new(raw);
        using TextReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        try { return Fin.Succ(new DstvReader().ParseAsync(reader).GetAwaiter().GetResult()); }
        catch (ParseException ex) { return Fin.Fail<IDstv>(Fault(SteelBlockKind.St, ex.LineNumber ?? 0)); }
    }

    static Fin<SteelImportReceipt> Project(IDstv dstv, byte[] raw, SteelContourPolicy policy) =>
        Optional(dstv.Header)
            .ToFin(Fault(SteelBlockKind.St, 0))
            .Bind(header => ValidateHeader(DstvMap.Header(header)).Bind(admitted => Build(admitted, dstv.Elements, raw, policy)));

    static Fin<SteelImportReceipt> Build(SteelHeader header, IEnumerable<DstvElement> elements, byte[] raw, SteelContourPolicy policy) =>
        toSeq(elements).Map((element, ordinal) => (element, ordinal))
            .TraverseM(row => Feature(row.element, row.ordinal, policy)).As()
            .Map(static admitted => admitted.Somes())
            .Bind(features => {
                SteelPart part = new(header, features);
                return part.Loops.IsEmpty
                    ? Fin.Fail<SteelImportReceipt>(Fault(SteelBlockKind.Ak, 0))
                    : Fin.Succ(new SteelImportReceipt(part, ContentKey.Of(EgressKind.Nc1, raw)));
            });

    static Fin<Option<SteelFeature>> Feature(DstvElement element, int ordinal, SteelContourPolicy policy) =>
        element switch {
            DstvSlot slot => Valid(new SteelFeature.Slot(DstvMap.Slot(slot)), SteelBlockKind.Bo, ordinal),
            DstvHole hole => Valid(new SteelFeature.Hole(DstvMap.Hole(hole)), SteelBlockKind.Bo, ordinal),
            DstvCut cut => Valid(new SteelFeature.Cut(DstvMap.Cut(cut)), SteelBlockKind.Sc, ordinal),
            DstvNumeration numeration => Valid(new SteelFeature.Numeration(DstvMap.Numeration(numeration)), SteelBlockKind.Si, ordinal),
            DstvBend => Fin.Fail<Option<SteelFeature>>(Fault(SteelBlockKind.Ka, ordinal)),
            Contour contour when contour.ContourType is ContourType.AK or ContourType.IK =>
                ContourOf(contour, ordinal, policy).Map(row => (Option<SteelFeature>)Some<SteelFeature>(new SteelFeature.Contour(row))),
            Contour contour when contour.ContourType is ContourType.KO or ContourType.PU =>
                ContourOf(contour, ordinal, policy).Map(row => (Option<SteelFeature>)Some<SteelFeature>(new SteelFeature.Marking(row))),
            _ => Fin.Fail<Option<SteelFeature>>(Fault(SteelBlockKind.Unknown, ordinal)),
        };

    static Fin<Option<SteelFeature>> Valid(SteelFeature feature, SteelBlockKind kind, int ordinal) =>
        feature.Switch(
            hole: static hole => hole.Row.Center.IsValid && !string.IsNullOrWhiteSpace(hole.Row.Flange)
                && double.IsFinite(hole.Row.DiameterMm) && hole.Row.DiameterMm > 0.0
                && double.IsFinite(hole.Row.DepthMm) && hole.Row.DepthMm >= 0.0,
            slot: static slot => slot.Row.Center.IsValid && !string.IsNullOrWhiteSpace(slot.Row.Flange)
                && double.IsFinite(slot.Row.DiameterMm) && slot.Row.DiameterMm > 0.0
                && double.IsFinite(slot.Row.DepthMm) && slot.Row.DepthMm >= 0.0
                && double.IsFinite(slot.Row.SlotLengthMm) && slot.Row.SlotLengthMm > 0.0
                && double.IsFinite(slot.Row.SlotWidthMm) && slot.Row.SlotWidthMm > 0.0
                && slot.Row.SlotLengthMm >= slot.Row.SlotWidthMm
                && double.IsFinite(slot.Row.SlotAngleDeg),
            cut: static cut => cut.Row.At.IsValid && !string.IsNullOrWhiteSpace(cut.Row.Flange),
            numeration: static numeration => numeration.Row.At.IsValid && !string.IsNullOrWhiteSpace(numeration.Row.Flange),
            contour: static _ => true,
            marking: static _ => true)
            ? Fin.Succ<Option<SteelFeature>>(Some(feature))
            : Fin.Fail<Option<SteelFeature>>(Fault(kind, ordinal));

    static Fin<SteelContour> ContourOf(Contour contour, int ordinal, SteelContourPolicy policy) {
        Arr<SteelPoint> points = toSeq(contour.Points).Map(static point => point switch {
            DstvSkewedPoint skew => DstvMap.Point(skew) with {
                Bevel = Some(new SteelBevel(skew.FirstAngle, skew.FirstBlunting, skew.SecondAngle, skew.SecondBlunting)) },
            _ => DstvMap.Point(point),
        }).ToArr();
        SteelBlockKind kind = BlockOf(contour);
        return Rounded(points, policy, kind, ordinal).Map(loop => new SteelContour(kind, loop.AsCcw(), points));
    }

    static Fin<Loop> Rounded(Arr<SteelPoint> points, SteelContourPolicy policy, SteelBlockKind kind, int ordinal) =>
        points.Count < 3
            ? Fin.Fail<Loop>(Fault(kind, ordinal))
            : toSeq(Enumerable.Range(0, points.Count)).TraverseM(index => Corner(points, index, policy, kind, ordinal)).As()
                .Bind(corners => toSeq(Enumerable.Range(0, points.Count)).Exists(index => {
                    int next = (index + 1) % points.Count;
                    Vector3d edge = points[next].At - points[index].At;
                    Vector3d straight = corners[next].Enter - corners[index].Exit;
                    return straight.Length <= policy.Tolerance.Absolute.Value || Vector3d.Multiply(edge, straight) <= 0.0;
                })
                    ? Fin.Fail<Loop>(Fault(kind, ordinal))
                    : Fin.Succ(corners.Bind(corner => corner.Enter.DistanceTo(corner.Exit) <= policy.Tolerance.Absolute.Value
                        ? Seq((At: corner.Enter, Bulge: 0.0))
                        : Seq((At: corner.Enter, corner.Bulge), (At: corner.Exit, Bulge: 0.0)))))
                .Bind(spans => Loop.Admit(spans.Map(static span => span.At).ToArr(), closed: true,
                    spans.Map(static span => span.Bulge).ToArr(), policy.Tolerance).MapFail(_ => Fault(kind, ordinal)));

    static Fin<(Point3d Enter, double Bulge, Point3d Exit)> Corner(Arr<SteelPoint> points, int index, SteelContourPolicy policy,
        SteelBlockKind kind, int ordinal) {
        SteelPoint point = points[index];
        if (!double.IsFinite(point.At.X) || !double.IsFinite(point.At.Y) || !double.IsFinite(point.At.Z)
            || !double.IsFinite(point.RadiusMm) || point.RadiusMm < 0.0
            || point.Bevel.Exists(static bevel => !double.IsFinite(bevel.FirstAngleDeg) || !double.IsFinite(bevel.FirstBlunting)
                || !double.IsFinite(bevel.SecondAngleDeg) || !double.IsFinite(bevel.SecondBlunting)))
            return Fin.Fail<(Point3d, double, Point3d)>(Fault(kind, ordinal));
        if (point.RadiusMm == 0.0)
            return Fin.Succ((point.At, 0.0, point.At));
        Point3d previous = points[((index - 1) + points.Count) % points.Count].At;
        Point3d next = points[(index + 1) % points.Count].At;
        Vector3d previousLeg = previous - point.At;
        Vector3d nextLeg = next - point.At;
        double previousLength = previousLeg.Length;
        double nextLength = nextLeg.Length;
        if (previousLength <= policy.MinimumLegMm || nextLength <= policy.MinimumLegMm)
            return Fin.Fail<(Point3d, double, Point3d)>(Fault(kind, ordinal));
        Vector3d toPrevious = previousLeg / previousLength;
        Vector3d toNext = nextLeg / nextLength;
        double theta = Vector3d.VectorAngle(toPrevious, toNext);
        double tangent = point.RadiusMm / Math.Tan(theta / 2.0);
        double sign = Math.Sign(Vector3d.CrossProduct(-toPrevious, toNext).Z);
        // DSTV notch semantics: a notch radius arcs INTO the material, so the span bows opposite the fillet side.
        double orientation = point.IsNotch ? -sign : sign;
        return !double.IsFinite(theta) || theta <= policy.AngularToleranceRad || Math.PI - theta <= policy.AngularToleranceRad
            || !double.IsFinite(tangent) || tangent <= 0.0 || tangent >= previousLength || tangent >= nextLength || sign == 0.0
                ? Fin.Fail<(Point3d, double, Point3d)>(Fault(kind, ordinal))
                : Fin.Succ((point.At + toPrevious * tangent, orientation * Math.Tan((Math.PI - theta) / 4.0),
                    point.At + toNext * tangent));
    }

    static SteelBlockKind BlockOf(Contour contour) =>
        contour.ContourType switch {
            ContourType.IK => SteelBlockKind.Ik,
            ContourType.KO => SteelBlockKind.Ko,
            ContourType.PU => SteelBlockKind.Pu,
            _ => SteelBlockKind.Ak,
        };

    static Fin<Unit> Validate(SteelContourPolicy policy) =>
        SteelContourPolicy.Of(policy.Tolerance, policy.MinimumLegMm, policy.AngularToleranceRad).Map(static _ => unit);

    static Fin<SteelHeader> ValidateHeader(SteelHeader header) =>
        string.IsNullOrWhiteSpace(header.PieceIdentification) || string.IsNullOrWhiteSpace(header.Profile)
        || string.IsNullOrWhiteSpace(header.ProfileCode) || string.IsNullOrWhiteSpace(header.SteelQuality)
        || header.QuantityOfPieces <= 0
        || !double.IsFinite(header.Length) || header.Length <= 0.0
        || !double.IsFinite(header.SawLength) || header.SawLength <= 0.0
        || !double.IsFinite(header.ProfileHeight) || header.ProfileHeight < 0.0
        || !double.IsFinite(header.FlangeWidth) || header.FlangeWidth < 0.0
        || !double.IsFinite(header.FlangeThickness) || header.FlangeThickness < 0.0
        || !double.IsFinite(header.WebThickness) || header.WebThickness < 0.0
        || !double.IsFinite(header.Radius) || header.Radius < 0.0
        || !double.IsFinite(header.WebStartCut) || !double.IsFinite(header.WebEndCut)
        || !double.IsFinite(header.FlangeStartCut) || !double.IsFinite(header.FlangeEndCut)
        || !double.IsFinite(header.WeightByMeter) || header.WeightByMeter < 0.0
        || !double.IsFinite(header.PaintingSurfaceByMeter) || header.PaintingSurfaceByMeter < 0.0
            ? Fin.Fail<SteelHeader>(Fault(SteelBlockKind.St, 0))
            : Fin.Succ(header);

    static Error Fault(SteelBlockKind block, int line) =>
        FabricationFault.IngressTranslation(SourceKind.Steel, new SourceLocus.DstvBlock(block.Key, line)).ToError();
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class DstvMap {
    [MapProperty(nameof(IDstvHeader.CodeProfile), nameof(SteelHeader.ProfileCode), Use = nameof(ProfileCode))]
    public static partial SteelHeader Header(IDstvHeader source);

    [MapPropertyFromSource(nameof(SteelPoint.At), Use = nameof(ContourPoint))]
    [MapProperty(nameof(DstvContourPoint.Radius), nameof(SteelPoint.RadiusMm))]
    [MapPropertyFromSource(nameof(SteelPoint.Bevel), Use = nameof(NoBevel))]
    public static partial SteelPoint Point(DstvContourPoint source);

    [MapPropertyFromSource(nameof(SteelHole.Center), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelHole.Flange))]
    [MapProperty(nameof(DstvHole.Diameter), nameof(SteelHole.DiameterMm))]
    [MapProperty(nameof(DstvHole.Depth), nameof(SteelHole.DepthMm))]
    public static partial SteelHole Hole(DstvHole source);

    [MapPropertyFromSource(nameof(SteelSlot.Center), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelSlot.Flange))]
    [MapProperty(nameof(DstvHole.Diameter), nameof(SteelSlot.DiameterMm))]
    [MapProperty(nameof(DstvHole.Depth), nameof(SteelSlot.DepthMm))]
    [MapProperty(nameof(DstvSlot.SlotLength), nameof(SteelSlot.SlotLengthMm))]
    [MapProperty(nameof(DstvSlot.SlotWidth), nameof(SteelSlot.SlotWidthMm))]
    [MapProperty(nameof(DstvSlot.SlotAngle), nameof(SteelSlot.SlotAngleDeg))]
    public static partial SteelSlot Slot(DstvSlot source);

    [MapPropertyFromSource(nameof(SteelCut.At), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelCut.Flange))]
    public static partial SteelCut Cut(DstvCut source);

    [MapPropertyFromSource(nameof(SteelNumeration.At), Use = nameof(LocatedPoint))]
    [MapProperty(nameof(LocatedElement.FlCode), nameof(SteelNumeration.Flange))]
    public static partial SteelNumeration Numeration(DstvNumeration source);

    [UserMapping]
    static string ProfileCode(CodeProfile code) => code.ToString();

    [UserMapping]
    static Point3d LocatedPoint(LocatedElement source) => new(source.XCoord, source.YCoord, 0.0);

    [UserMapping]
    static Point3d ContourPoint(DstvContourPoint source) => new(source.XCoord, source.YCoord, 0.0);

    [UserMapping]
    static Option<SteelBevel> NoBevel(DstvContourPoint source) => None;
}
```
