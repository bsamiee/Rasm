# [RASM_FABRICATION_STEEL_IMPORT]

The DSTV NC1 steel-ingress boundary: `SteelImport` admits an `.nc1` steel-profile program through `DSTV.Net` into the fabrication atoms vocabulary, projecting the `ST` header plus `BO`/`SI`/`SC`/`AK`/`IK`/`KA` block record tree into `Loop`, hole, slot, cut, numeration, and bend-seed rows. The reader is ingress only: `DstvReader.ParseAsync` parses the inbound program, `ParseException.LineNumber` lowers to `FabricationFault.IngressTranslation(SourceKind.Steel, SourceLocus.DstvBlock(...))`, and no DSTV.Net type crosses beyond the boundary. `Riok.Mapperly` owns the field transcription from provider records to local rows; topology assembly stays on `SteelImport`, because `Loop.AsCcw`, `ContentKey.Of(EgressKind.Nc1, ...)`, and the profile admission rail are fabrication-owned surfaces.

The imported profile is the same `Loop` part library consumed by `Nesting/nfp` and the posting cut-program owner. `KA` rows leave this page as typed `SteelBendSeed` rows: the bend locus, flange, source line, and source thickness reach `Forming/sheet`, while K factor, bend allowance, radius policy, relief, and the kernel development overlay remain there. NC1 emission shares the local header and feature model with the posting dialect, but the emit fold is the dialect owner; this page never writes `.nc1`.

Wire posture: HOST-LOCAL, HOST-NEUTRAL. `DSTV.Net` is pure managed input material, `Mapperly` is compile-time transcription, and the public ingress entry remains `Ingress.Admit(IngressSource)` through its `Steel` arm — the fold itself is declared ONCE on `Ingress/profile#PROFILE_IMPORT` and this page supplies only the arm body. A second steel reader, a second `Ingress` fold declaration, a hand-written DSTV record copyist, a direct `DstvElement` dependency in nesting/toolpath/forming, or a local NC1 writer is the rejected shape.

## [01]-[INDEX]

- [01]-[STEEL_IMPORT]: owns `SteelImport.Read`, the `IngressSource.Steel` arm, the local `SteelHeader`/`SteelPart`/`SteelFeature`/`SteelContour`/`SteelBendSeed` model, the Mapperly `DstvMap` seam, the DSTV parse-fault lowering through 2711, and the `Loop` projection over `AK`/`IK` contour blocks.

## [02]-[STEEL_IMPORT]

- Owner: `SteelImport` the static boundary owning DSTV NC1 read admission; `SteelBlockKind` the bounded block-code vocabulary for diagnostic loci (`ST`/`BO`/`SI`/`SC`/`AK`/`IK`/`KA`); `SteelHeader` the local `ST` descriptor; `SteelFeature` the closed local feature family; `SteelPart` the one part receipt carrying loops, rows, bend seeds, and the `ContentKey`; `DstvMap` the Mapperly-generated transcription capsule. The owner converts provider records once, then exports only fabrication rows.
- Cases: `ST` header maps to `SteelHeader`; `BO` maps to `SteelHole`; slotted `BO` maps to `SteelSlot`; `SI` maps to `SteelNumeration`; `SC` maps to `SteelCut`; `AK`/`IK` map to `Loop` contours; `KA` maps to `SteelBendSeed`; unknown provider elements drop from the feature rail because unsupported DSTV previews and debug projections carry no fabrication geometry. Slot dispatch precedes hole dispatch because `DstvSlot` is a `DstvHole` subtype.
- Entry: `Fin<SteelImportReceipt> SteelImport.Read(string path)` reads raw bytes for stable NC1 content identity, parses the program through `DstvReader.ParseAsync(TextReader)`, projects it through `DstvMap`, assembles loops and features, and returns the typed receipt. `Ingress.Admit` adds only the `Steel` arm: `SteelImport.Read(s.Path).Map(r => new AdmittedGeometry.Profiles(r.Part.Loops))`.
- Auto: the parse boundary catches the abstract `ParseException` once and lowers `LineNumber` to `SourceLocus.DstvBlock("ST", line)`; missing header lowers to `ST:0`; an empty boundary contour set lowers to `AK:0`. Mapperly generates header, located feature, slot, contour-point, and numeration copy code; `SteelImport` owns case dispatch, contour assembly, bend-axis derivation from local header dimensions, content-key minting, and result projection.
- Receipt: `SteelImportReceipt` carries `SteelPart` plus `ContentKey.Of(EgressKind.Nc1, rawBytes)`. `SteelPart.Loops` is the profile library, `SteelPart.Contours` preserves `AK`/`IK` point and radius rows, `SteelPart.Features` is the local block row set, `SteelPart.Bends` is the sheet-metal seed stream, and no `IDstv`, `DstvElement`, `Contour`, `DstvHole`, or `CodeProfile` leaks.
- Packages: `DSTV.Net` (`DstvReader.ParseAsync(TextReader)`, `IDstv.Header`, `IDstv.Elements`, `IDstvHeader`, `DstvElement` cases, `LocatedElement.FlCode`/`XCoord`/`YCoord`, `DstvHole`, `DstvSlot`, `DstvCut`, `DstvBend`, `DstvNumeration`, `DstvContourPoint`, `Contour.Points`, `ContourType`, `ParseException.LineNumber`); `Riok.Mapperly` (`[Mapper]`, `[MapProperty]`, `[MapPropertyFromSource]`, `[MapperRequiredMapping]`, `[UserMapping]`); `Process/owner#FABRICATION_OWNER` (`Loop`, `Edge3`, `EgressKind.Nc1`, `ContentKey`, `AdmittedGeometry`, `IngressSource`); `Process/faults#FAULT_BAND` (`SourceKind.Steel`, `SourceLocus.DstvBlock`, `FabricationFault.IngressTranslation`); `Forming/sheet#FLAT_PATTERN` consumes `SteelBendSeed`; LanguageExt.Core (`Fin`, `Option`, `Seq`, `Arr`, `Eff`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`); BCL inbox (`File`, `IEnumerable`, `Task`, `TextReader`, `Encoding`, `MemoryStream`).
- Growth: a new NC1 read block is one `SteelFeature` case, one `SteelBlockKind` row, and one Mapperly partial mapping. A new contour interpretation is one topology arm under `SteelImport`. A new NC1 emission detail lands on the posting dialect over the shared local row model. A new bend convention lands on `Forming/sheet` over `SteelBendSeed`.
- Boundary: DSTV.Net is read-only; `ToSvg()` is debug preview, not drafting output; NC1 write belongs to posting dialect; K factor and unroll belong to `Forming/sheet`; `KA` rows do not run a local second unfold engine; Mapperly maps records and owns no identity, rail, hashing, topology, or policy; `FabricationFault.IngressTranslation` is the sole steel-ingress fault arm.

```csharp signature
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

[SmartEnum<string>]
public sealed partial class SteelBlockKind {
    public static readonly SteelBlockKind St = new("ST");
    public static readonly SteelBlockKind Bo = new("BO");
    public static readonly SteelBlockKind Si = new("SI");
    public static readonly SteelBlockKind Sc = new("SC");
    public static readonly SteelBlockKind Ak = new("AK");
    public static readonly SteelBlockKind Ik = new("IK");
    public static readonly SteelBlockKind Ka = new("KA");
}

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
    double Radius);

public sealed record SteelPoint(Point3d At, bool IsNotch, double RadiusMm);

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
    public sealed record Bend(SteelBendSeed Row) : SteelFeature;
}

public sealed record SteelPart(
    SteelHeader Header,
    Arr<Loop> Loops,
    Seq<SteelFeature> Features,
    Seq<SteelBendSeed> Bends,
    Seq<SteelContour> Contours,
    Seq<SteelHole> Holes,
    Seq<SteelSlot> Slots,
    Seq<SteelCut> Cuts,
    Seq<SteelNumeration> Numerations);

public sealed record SteelImportReceipt(SteelPart Part, ContentKey Key);

public static class SteelImport {
    static readonly IDstvReader Reader = new DstvReader();

    public static Fin<SteelImportReceipt> Read(string path) =>
        Try(() => File.ReadAllBytes(path))
            .ToFin()
            .MapFail(static _ => Fault(SteelBlockKind.St, 0))
            .Bind(static raw => Parse(raw).Bind(dstv => Project(dstv, raw)));

    static Fin<IDstv> Parse(byte[] raw) =>
        Eff.lift<IDstv>(() => ParseTask(raw)).Run();

    static async Task<Fin<IDstv>> ParseTask(byte[] raw) {
        try {
            using MemoryStream stream = new(raw);
            using TextReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            IDstv parsed = await Reader.ParseAsync(reader).ConfigureAwait(false);
            return Fin.Succ(parsed);
        }
        catch (ParseException ex) {
            return Fin.Fail<IDstv>(Fault(SteelBlockKind.St, ex.LineNumber ?? 0));
        }
    }

    static Fin<SteelImportReceipt> Project(IDstv dstv, byte[] raw) =>
        Optional(dstv.Header)
            .ToFin(Fault(SteelBlockKind.St, 0))
            .Bind(header => Build(DstvMap.Header(header), dstv.Elements, raw));

    static Fin<SteelImportReceipt> Build(SteelHeader header, IEnumerable<DstvElement> elements, byte[] raw) {
        Seq<DstvElement> source = toSeq(elements);
        // DSTV.Net exposes no per-element line number, so the parsed element ordinal IS the recoverable
        // NC1 locus — threaded here once and carried by every seed that keeps a source trace.
        Seq<SteelFeature> features = source.Map((element, ordinal) => Feature(element, header, ordinal)).Somes();
        Seq<SteelContour> contours = features.Choose(static feature => feature is SteelFeature.Contour contour
            ? Some(contour.Row)
            : None);
        Arr<Loop> loops = contours.Map(static contour => contour.Loop).ToArr();
        Seq<SteelBendSeed> bends = features.Choose(static feature => feature is SteelFeature.Bend bend
            ? Some(bend.Row)
            : None);
        Seq<SteelHole> holes = features.Choose(static feature => feature is SteelFeature.Hole hole
            ? Some(hole.Row)
            : None);
        Seq<SteelSlot> slots = features.Choose(static feature => feature is SteelFeature.Slot slot
            ? Some(slot.Row)
            : None);
        Seq<SteelCut> cuts = features.Choose(static feature => feature is SteelFeature.Cut cut
            ? Some(cut.Row)
            : None);
        Seq<SteelNumeration> numerations = features.Choose(static feature => feature is SteelFeature.Numeration numeration
            ? Some(numeration.Row)
            : None);

        return loops.IsEmpty
            ? Fin.Fail<SteelImportReceipt>(Fault(SteelBlockKind.Ak, 0))
            : Fin.Succ(new SteelImportReceipt(
                new SteelPart(header, loops, features, bends, contours, holes, slots, cuts, numerations),
                ContentKey.Of(EgressKind.Nc1, raw)));
    }

    static Option<SteelFeature> Feature(DstvElement element, SteelHeader header, int ordinal) =>
        element switch {
            DstvSlot slot => Some<SteelFeature>(new SteelFeature.Slot(DstvMap.Slot(slot))),
            DstvHole hole => Some<SteelFeature>(new SteelFeature.Hole(DstvMap.Hole(hole))),
            DstvCut cut => Some<SteelFeature>(new SteelFeature.Cut(DstvMap.Cut(cut))),
            DstvNumeration numeration => Some<SteelFeature>(new SteelFeature.Numeration(DstvMap.Numeration(numeration))),
            DstvBend bend => Some<SteelFeature>(new SteelFeature.Bend(BendSeed(bend, header, ordinal))),
            Contour contour when Boundary(contour) => Some<SteelFeature>(new SteelFeature.Contour(ContourOf(contour))),
            _ => None,
        };

    static bool Boundary(Contour contour) =>
        contour.ContourType is ContourType.AK or ContourType.IK;

    static SteelContour ContourOf(Contour contour) {
        Arr<SteelPoint> points = toSeq(contour.Points).Map(static point => DstvMap.Point(point)).ToArr();
        Loop loop = new Loop(points.Map(static point => point.At).ToArr(), Closed: true).AsCcw();
        return new SteelContour(BlockOf(contour), loop, points);
    }

    static SteelBlockKind BlockOf(Contour contour) =>
        contour.ContourType switch {
            ContourType.AK => SteelBlockKind.Ak,
            ContourType.IK => SteelBlockKind.Ik,
            _ => SteelBlockKind.Ak,
        };

    static SteelBendSeed BendSeed(DstvBend bend, SteelHeader header, int ordinal) {
        double y1 = bend.YCoord;
        double y2 = bend.YCoord + AxisLength(header);
        double thickness = header.FlangeThickness > 0.0 ? header.FlangeThickness : header.WebThickness;
        return new SteelBendSeed(
            new Edge3(new Point3d(bend.XCoord, y1, 0.0), new Point3d(bend.XCoord, y2, 0.0)),
            bend.FlCode,
            thickness,
            SourceLine: ordinal);
    }

    static double AxisLength(SteelHeader header) =>
        header.FlangeWidth > 0.0 ? header.FlangeWidth : header.ProfileHeight;

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
}
```
