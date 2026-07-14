# [RASM_FABRICATION_STEEL_IMPORT]

The DSTV NC1 steel-ingress boundary: `SteelImport` admits an `.nc1` steel-profile program through `DSTV.Net` into the fabrication atoms vocabulary, projecting the FULL 25-field `ST` header plus the `BO`/`SI`/`SC`/`AK`/`IK`/`KO`/`PU`/`KA` block record tree into `Loop`, hole, slot, cut, numeration, marking, and bend-seed rows. The reader is ingress only: `DstvReader.ParseAsync` parses the inbound program, `ParseException.LineNumber` lowers to `FabricationFault.IngressTranslation(SourceKind.Steel, SourceLocus.DstvBlock(...))`, and no DSTV.Net type crosses beyond the boundary. `Riok.Mapperly` owns the field transcription from provider records to local rows; topology assembly stays on `SteelImport`, because the corner-fillet lowering, `Loop.AsCcw`, `ContentKey.Of(EgressKind.Nc1, ...)`, and the profile admission rail are fabrication-owned surfaces. A radiused `AK`/`IK` contour point lowers ARC-EXACT: the vertex replaces with its tangent pair and one bulged span in the owner#atoms `Loop.Bulges` column, so a rounded or coped NC1 boundary reaches nesting and toolpath as the arc it encodes, never a straight-chord substitute; a bevelled `DstvSkewedPoint` carries its weld-prep angles on the typed `SteelBevel` payload.

The imported profile is the same `Loop` part library consumed by `Nesting/nfp` and the posting cut-program owner. `KA` rows leave this page as typed `SteelBendSeed` rows — the bend locus, flange, source line, and source thickness — and lower at this boundary onto the `Forming/sheet` `FormSource` `BendSeed` wire (`Edge3` line, SIGNED angle, optional radius) the analytic unfold lane consumes; the signed bend angle rides the `DstvBend` record — RESEARCH: verify the `DstvBend` bend-angle member spelling by `assay api` decompile over `DSTV.Net` before the lowering bakes it. K factor, bend allowance, radius policy, relief, and the kernel development overlay remain on `Forming/sheet`. `KO`/`PU` contour blocks are MARKING geometry — typed `SteelFeature.Marking` rows the layout/marking consumers read, never boundary loops and never silent drops. NC1 emission shares the local header and feature model with the posting dialect, but the emit fold is the dialect owner; this page never writes `.nc1`.

Wire posture: HOST-LOCAL, HOST-NEUTRAL. `DSTV.Net` is pure managed input material, `Mapperly` is compile-time transcription, and the public ingress entry remains `Ingress.Admit(IngressSource)` through its `Steel` arm — the fold itself is declared ONCE on `Ingress/profile#PROFILE_IMPORT` and this page supplies only the arm body. A second steel reader, a second `Ingress` fold declaration, a hand-written DSTV record copyist, a direct `DstvElement` dependency in nesting/toolpath/forming, or a local NC1 writer is the rejected shape.

## [01]-[INDEX]

- [01]-[STEEL_IMPORT]: owns `SteelImport.Read`, the `IngressSource.Steel` arm, the local `SteelHeader`/`SteelPart`/`SteelFeature`/`SteelContour`/`SteelBevel`/`SteelBendSeed` model, the Mapperly `DstvMap` seam, the DSTV parse-fault lowering through 2711, the corner-fillet bulge lowering, and the `Loop` projection over `AK`/`IK` contour blocks.

## [02]-[STEEL_IMPORT]

- Owner: `SteelImport` the static boundary owning DSTV NC1 read admission; `SteelBlockKind` the bounded block-code vocabulary for diagnostic loci (`ST`/`BO`/`SI`/`SC`/`AK`/`IK`/`KO`/`PU`/`KA`); `SteelHeader` the FULL 25-field `ST` descriptor (identity, profile, section geometry, the four end-cut angles, weight/painting-surface, four free-text rows); `SteelFeature` the closed local feature family; `SteelPart` the ONE feature store whose per-kind views DERIVE (`Loops`/`Contours`/`Bends`/`Holes`/`Slots`/`Cuts`/`Numerations`/`Markings` are expression-bodied projections over `Features`, never parallel stored state); `DstvMap` the Mapperly-generated transcription capsule. The owner converts provider records once, then exports only fabrication rows.
- Cases: `ST` maps to `SteelHeader`; `BO` to `SteelHole`; slotted `BO` to `SteelSlot` (slot dispatch precedes hole dispatch — `DstvSlot` IS a `DstvHole`); `SI` to `SteelNumeration`; `SC` to `SteelCut`; `AK`/`IK` to boundary `Loop` contours (fillet-lowered, bulge-carrying); `KO`/`PU` to `SteelFeature.Marking` contours; `KA` to `SteelBendSeed`; a bevelled `DstvSkewedPoint` maps its `FirstAngle`/`FirstBlunting`/`SecondAngle`/`SecondBlunting` onto the point's `SteelBevel` payload (skewed dispatch precedes the base point — subtype order); provider elements outside the block roster drop from the feature rail because unsupported DSTV previews carry no fabrication geometry.
- Entry: `Fin<SteelImportReceipt> SteelImport.Read(string path)` reads raw bytes for stable NC1 content identity, parses the program through `DstvReader.ParseAsync(TextReader)` bridged synchronously at the one boundary, projects it through `DstvMap`, assembles loops and features, and returns the typed receipt. `Ingress.Admit` adds only the `Steel` arm: `SteelImport.Read(s.Path).Map(r => new AdmittedGeometry.Profiles(r.Part.Loops))`.
- Auto: the parse boundary catches `ParseException` once inside the sync bridge and lowers `LineNumber` to `SourceLocus.DstvBlock("ST", line)`; any non-parse throw rides the `Try.lift` capture; missing header lowers to `ST:0`; an empty boundary contour set lowers to `AK:0`. Mapperly generates header, located feature, slot, contour-point, and numeration copy code; `SteelImport` owns case dispatch, fillet-to-bulge contour assembly, bend-axis derivation from local header dimensions, content-key minting, and result projection.
- Receipt: `SteelImportReceipt` carries `SteelPart` plus `ContentKey.Of(EgressKind.Nc1, rawBytes)`. `SteelPart.Loops` is the bulge-carrying boundary library, `SteelPart.Features` the ONE stored feature family, every per-kind view a derived projection, and no `IDstv`, `DstvElement`, `Contour`, `DstvHole`, or `CodeProfile` leaks.
- Packages: `DSTV.Net` (`DstvReader.ParseAsync(TextReader)`, `IDstv.Header`/`Elements`, `IDstvHeader` 25 fields incl. `WebStartCut`/`WebEndCut`/`FlangeStartCut`/`FlangeEndCut`/`WeightByMeter`/`PaintingSurfaceByMeter`/`Text1InfoOnPiece`…`Text4InfoOnPiece`, `DstvElement` cases, `LocatedElement.FlCode`/`XCoord`/`YCoord`, `DstvHole`, `DstvSlot`, `DstvCut`, `DstvBend`, `DstvNumeration`, `DstvContourPoint`, `DstvSkewedPoint`, `Contour.Points`, `ContourType` incl. `KO`/`PU`, `ParseException.LineNumber`); `Riok.Mapperly` (`[Mapper]`, `[MapProperty]`, `[MapPropertyFromSource]`, `[UserMapping]`); `Process/owner#FABRICATION_OWNER` (`Loop`+`Bulges`, `Edge3`, `EgressKind.Nc1`, `ContentKey`, `AdmittedGeometry`, `IngressSource`); `Process/faults#FAULT_BAND` (`SourceKind.Steel`, `SourceLocus.DstvBlock`, `FabricationFault.IngressTranslation`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Arr`, `Try`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`); BCL inbox (`File`, `TextReader`, `Encoding`, `MemoryStream`).
- Growth: a new NC1 read block is one `SteelFeature` case, one `SteelBlockKind` row, and one Mapperly partial mapping. A new contour interpretation is one topology arm under `SteelImport`. A new NC1 emission detail lands on the posting dialect over the shared local row model. A new bend convention lands on `Forming/sheet` over `SteelBendSeed`.
- Boundary: DSTV.Net is read-only; `ToSvg()` is debug preview, not drafting output; NC1 write belongs to posting dialect; K factor and unroll belong to `Forming/sheet`; `KA` rows do not run a local second unfold engine; Mapperly maps records and owns no identity, rail, hashing, topology, or policy; `FabricationFault.IngressTranslation` is the sole steel-ingress fault arm; the feature family is stored ONCE and a second materialized per-kind collection beside the derived views is the parallel-state defect this rebuild collapsed; a recognized contour type silently erased by a default arm is the rejected form — `KO`/`PU` own their marking case.

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
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// The full ST descriptor: identity, profile, section geometry, end-cut prep angles, mass/surface, free text —
// end cuts are load-bearing miter/cope geometry, never dropped columns.
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

// Weld-prep bevel payload of a skewed contour vertex: two angle/blunting pairs, degrees/mm as parsed.
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

// SourceLine = the parsed element ordinal (DSTV.Net exposes no per-element file line) — the KA locus diagnostics trace.
public sealed record SteelBendSeed(Edge3 Line, string Flange, double ThicknessMm, int SourceLine);

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
    public sealed record Bend(SteelBendSeed Row) : SteelFeature;
}

// ONE stored feature family; every per-kind view derives — parallel materialized collections are the deleted form.
public sealed record SteelPart(SteelHeader Header, Seq<SteelFeature> Features) {
    public Seq<SteelContour> Contours => Features.Choose(static f => f is SteelFeature.Contour c ? Some(c.Row) : None);
    public Arr<Loop> Loops => Contours.Map(static c => c.Loop).ToArr();
    public Seq<SteelContour> Markings => Features.Choose(static f => f is SteelFeature.Marking m ? Some(m.Row) : None);
    public Seq<SteelBendSeed> Bends => Features.Choose(static f => f is SteelFeature.Bend b ? Some(b.Row) : None);
    public Seq<SteelHole> Holes => Features.Choose(static f => f is SteelFeature.Hole h ? Some(h.Row) : None);
    public Seq<SteelSlot> Slots => Features.Choose(static f => f is SteelFeature.Slot s ? Some(s.Row) : None);
    public Seq<SteelCut> Cuts => Features.Choose(static f => f is SteelFeature.Cut c ? Some(c.Row) : None);
    public Seq<SteelNumeration> Numerations => Features.Choose(static f => f is SteelFeature.Numeration n ? Some(n.Row) : None);
}

public sealed record SteelImportReceipt(SteelPart Part, ContentKey Key);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class SteelImport {
    static readonly IDstvReader Reader = new DstvReader();

    public static Fin<SteelImportReceipt> Read(string path) =>
        Try.lift(() => File.ReadAllBytes(path)).Run()
            .MapFail(static _ => Fault(SteelBlockKind.St, 0))
            .Bind(static raw => Parse(raw).Bind(dstv => Project(dstv, raw)));

    // EXCEPTION_CAPTURE form: Try.lift captures any non-parse throw, the self-flattening Bind collapses the
    // outer rail into the sync bridge's inner Fin.
    static Fin<IDstv> Parse(byte[] raw) =>
        Try.lift(() => ParseSync(raw)).Run()
            .MapFail(static _ => Fault(SteelBlockKind.St, 0))
            .Bind(identity);

    // Sync-over-async at the ONE parse boundary (ParseAsync is the sole package ingress); ParseException is
    // the typed failure source and its LineNumber the recoverable locus. Statement seam.
    static Fin<IDstv> ParseSync(byte[] raw) {
        using MemoryStream stream = new(raw);
        using TextReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        try { return Fin.Succ(Reader.ParseAsync(reader).GetAwaiter().GetResult()); }
        catch (ParseException ex) { return Fin.Fail<IDstv>(Fault(SteelBlockKind.St, ex.LineNumber ?? 0)); }
    }

    static Fin<SteelImportReceipt> Project(IDstv dstv, byte[] raw) =>
        Optional(dstv.Header)
            .ToFin(Fault(SteelBlockKind.St, 0))
            .Bind(header => Build(DstvMap.Header(header), dstv.Elements, raw));

    static Fin<SteelImportReceipt> Build(SteelHeader header, IEnumerable<DstvElement> elements, byte[] raw) {
        // The parsed element ordinal IS the recoverable NC1 locus — threaded once, carried by every seed
        // that keeps a source trace.
        Seq<SteelFeature> features = toSeq(elements).Map((element, ordinal) => Feature(element, header, ordinal)).Somes();
        SteelPart part = new(header, features);
        return part.Loops.IsEmpty
            ? Fin.Fail<SteelImportReceipt>(Fault(SteelBlockKind.Ak, 0))
            : Fin.Succ(new SteelImportReceipt(part, ContentKey.Of(EgressKind.Nc1, raw)));
    }

    static Option<SteelFeature> Feature(DstvElement element, SteelHeader header, int ordinal) =>
        element switch {
            DstvSlot slot => Some<SteelFeature>(new SteelFeature.Slot(DstvMap.Slot(slot))),
            DstvHole hole => Some<SteelFeature>(new SteelFeature.Hole(DstvMap.Hole(hole))),
            DstvCut cut => Some<SteelFeature>(new SteelFeature.Cut(DstvMap.Cut(cut))),
            DstvNumeration numeration => Some<SteelFeature>(new SteelFeature.Numeration(DstvMap.Numeration(numeration))),
            DstvBend bend => Some<SteelFeature>(new SteelFeature.Bend(BendSeed(bend, header, ordinal))),
            Contour contour when contour.ContourType is ContourType.AK or ContourType.IK =>
                Some<SteelFeature>(new SteelFeature.Contour(ContourOf(contour))),
            Contour contour when contour.ContourType is ContourType.KO or ContourType.PU =>
                Some<SteelFeature>(new SteelFeature.Marking(ContourOf(contour))),
            _ => None,
        };

    static SteelContour ContourOf(Contour contour) {
        Arr<SteelPoint> points = toSeq(contour.Points).Map(static point => point switch {
            DstvSkewedPoint skew => DstvMap.Point(skew) with {
                Bevel = Some(new SteelBevel(skew.FirstAngle, skew.FirstBlunting, skew.SecondAngle, skew.SecondBlunting)) },
            _ => DstvMap.Point(point),
        }).ToArr();
        return new SteelContour(BlockOf(contour), Rounded(points).AsCcw(), points);
    }

    // Corner-fillet lowering onto the atoms arc rail: a radiused non-notch vertex replaces with its tangent
    // pair at t = r/tan(θ/2) along both incident segments and ONE bulged span b = ±tan((π−θ)/4), the sign the
    // turn direction's — arc-exact, never a straight-chord substitute; notch evidence stays on the point row.
    static Loop Rounded(Arr<SteelPoint> points) {
        var verts = new List<Point3d>();
        var bulges = new List<double>();
        for (int i = 0; i < points.Count; i++) {
            SteelPoint p = points[i];
            if (p.RadiusMm <= 0.0 || p.IsNotch) { verts.Add(p.At); bulges.Add(0.0); continue; }
            Point3d prev = points[((i - 1) + points.Count) % points.Count].At;
            Point3d next = points[(i + 1) % points.Count].At;
            Vector3d toPrev = prev - p.At, toNext = next - p.At;
            toPrev.Unitize(); toNext.Unitize();
            double theta = Vector3d.VectorAngle(toPrev, toNext);
            double t = p.RadiusMm / Math.Tan(theta / 2.0);
            double sweep = Math.PI - theta;
            double sign = Math.Sign(Vector3d.CrossProduct(-toPrev, toNext).Z);
            verts.Add(p.At + toPrev * t); bulges.Add(sign * Math.Tan(sweep / 4.0));
            verts.Add(p.At + toNext * t); bulges.Add(0.0);
        }
        return new Loop(verts.ToArr(), Closed: true, bulges.ToArr());
    }

    static SteelBlockKind BlockOf(Contour contour) =>
        contour.ContourType switch {
            ContourType.IK => SteelBlockKind.Ik,
            ContourType.KO => SteelBlockKind.Ko,
            ContourType.PU => SteelBlockKind.Pu,
            _ => SteelBlockKind.Ak,
        };

    static SteelBendSeed BendSeed(DstvBend bend, SteelHeader header, int ordinal) {
        double axis = header.FlangeWidth > 0.0 ? header.FlangeWidth : header.ProfileHeight;
        double thickness = header.FlangeThickness > 0.0 ? header.FlangeThickness : header.WebThickness;
        return new SteelBendSeed(
            new Edge3(new Point3d(bend.XCoord, bend.YCoord, 0.0), new Point3d(bend.XCoord, bend.YCoord + axis, 0.0)),
            bend.FlCode, thickness, SourceLine: ordinal);
    }

    static Error Fault(SteelBlockKind block, int line) =>
        FabricationFault.IngressTranslation(SourceKind.Steel, new SourceLocus.DstvBlock(block.Key, line)).ToError();
}

// Ingress.Admit lives on Ingress/profile#PROFILE_IMPORT; this page contributes ONLY the Steel arm body:
// steel: static steel => SteelImport.Read(steel.Path).Map(receipt => (AdmittedGeometry)new AdmittedGeometry.Profiles(receipt.Part.Loops))
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
