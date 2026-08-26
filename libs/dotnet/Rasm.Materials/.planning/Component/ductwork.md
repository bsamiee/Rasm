# [MATERIALS_DUCTWORK]

THE DUCTWORK SEED PAGE owns the `ComponentFamily.Ductwork` row facts (`ComponentClass.Minor`, `DetailLane.Product`): the buildable galvanized sheet-metal duct PRODUCT rows — pressure class, gauge, seal and liner classes, geometry — under the SMACNA duct-construction schedules. The pressure-class ladder (½″–10″ w.g. with its 125–2500 Pa metric twins) is corroborated law; the rectangular unreinforced-gauge schedule and the round Table 3-2A schedule are PRIMARY-SINGLE — transcribed from the CFR-incorporated primary text with no independent second posting — so every gauge-derived cell crosses the boundary under the `Attestation.PrimarySingle` flag, never as falsely two-sourced. A component's gauge is RESOLVED from the schedule by its class, size, and seam, never asserted per stocked row, and the resolution is the seed law's own coherence conjunct, so a whole roster of out-of-schedule selections reports together. Airside SIZING — flow, friction, static-pressure balance — is `Rasm.Compute`'s; this page owns the buildable product rows alone.

The page composes settled law without re-derivation: `SectionProfile.CircleHollow.Of` and `SectionProfile.RectangleHollow.Of` are the fallible geometry admissions and `component#COMPONENT_SEED` the ONE generator fold; the product bag builds through `ComponentDetail.ProductRows` over the `Rasm.Element/Properties/property#DETAIL_SCHEMA` rows `DuctGauge`/`SealClass`/`LinerClass` with the duct pressure classification riding the shared `PressureClass` row; `Attestation` and the `SegmentRows` trade mints compose from `pipework#PIPE_SYSTEMS` and `pipework#PIPEWORK_SEED`; `ThreadRow.InchToMm` is the one inch basis (`fastener#FASTENER_FAMILY`); the IFC stamp is `IfcPipeSegment`'s duct sibling `IfcDuctSegment`/`RIGIDSEGMENT`; the substance binds `steel.galvanized` (the ASTM A653 lock-forming sheet row) and the appearance `metal.steel` at `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`.

## [01]-[INDEX]

- [02]-[DUCT_SCHEDULE]: the `DuctGauge` sheet ladder, the `DuctSeam` axis, the `DuctClass` pressure-class policy rows carrying the rectangular breakpoint schedule, the `RoundRow` Table 3-2A roster, and the `DuctSchedule` resolution folds with their transcription attestation.
- [03]-[DUCTWORK_SEED]: the `DuctSeal`/`DuctLiner` vocabularies, the closed `DuctShape` geometry payload, the `DuctRow` roster row, and `DuctworkSeed` — the roster, the seed law with its accumulating coherence and product bag, and the typed sizing refusal.

## [02]-[DUCT_SCHEDULE]

- Owner: `DuctClass` the one pressure-class axis — inches w.g., the SMACNA-rounded Pa class label, the rectangular unreinforced breakpoint ladder with its reinforcement-required limit, and the round-schedule read for the classes the round tables publish; `DuctGauge` the galvanized sheet ladder with its SMACNA metric-twin thickness; `DuctSeam` the spiral/longitudinal axis; `DuctSchedule` the two resolution folds and the attestation of the cells they read.
- Cases: seven classes {½ · 1 · 2 · 3 · 4 · 6 · 10 in w.g.} × two geometries; the round schedule exists only at +2/+4/+10 in w.g., so the other classes answer `None` at the round read and a round duct outside those classes is unmintable rather than silently re-classed.
- Law: the exact conversion is 1 in w.g. = 248.84 Pa and the class labels are SMACNA's own roundings (125/250/500/750/1000/1500/2500 Pa) — the label is a published token, never a recomputed value; the unassigned-design default is the 1 in w.g. class and VAV duct upstream of boxes the 2 in w.g. class.
- Law: a `None` from either resolution fold names the REINFORCEMENT-REQUIRED band — the reinforcement matrices (codes A–L, tie rods, joint spacing) are out of the corroborated set, so larger sizes refuse rather than carry a gauge the standard conditions on reinforcement this page does not model.
- Law: the gauge↔mm correspondence is the SMACNA metric twin, PRIMARY-SINGLE like the schedules; the connector `Gauges` table overlaps it at 22–16 ga with agreeing values, and both stand — one prints the AISI cold-formed design basis, the other the SMACNA duct-sheet basis, two authorities for two regimes.
- Exemption: flat-oval and aluminum duct carry no corroborated schedule — each joins as one stocked row with its schedule roster the moment its table proves; no vocabulary row is minted for either now.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, the contract bag currencies), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED` owners, the sibling `Attestation`/`SegmentRows`, the `ThreadRow.InchToMm` inch basis, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[Union]` + `[UseDelegateFromConstructor]`), LanguageExt.Core, BCL (`ImmutableArray`). NO duct-schedule producer exists among admitted packages, so the schedules are PUBLISHED here under SEED_ROW_LAW with per-column provenance.
- Growth: a new pressure class is one `DuctClass` row; a heavier or lighter sheet one `DuctGauge` row; a negative-pressure round schedule one roster beside `RoundRow` read by the same class delegates.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctGauge {
    public static readonly DuctGauge Ga28 = new("28ga", thicknessMm: 0.48);
    public static readonly DuctGauge Ga26 = new("26ga", thicknessMm: 0.55);
    public static readonly DuctGauge Ga24 = new("24ga", thicknessMm: 0.70);
    public static readonly DuctGauge Ga22 = new("22ga", thicknessMm: 0.85);
    public static readonly DuctGauge Ga20 = new("20ga", thicknessMm: 1.00);
    public static readonly DuctGauge Ga18 = new("18ga", thicknessMm: 1.31);
    public static readonly DuctGauge Ga16 = new("16ga", thicknessMm: 1.61);
    public double ThicknessMm { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctSeam {
    public static readonly DuctSeam Spiral       = new("spiral",       static (spiral, longitudinal) => spiral);
    public static readonly DuctSeam Longitudinal = new("longitudinal", static (spiral, longitudinal) => longitudinal);
    [UseDelegateFromConstructor] public partial DuctGauge Pick(DuctGauge spiral, DuctGauge longitudinal);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RoundRow(
    double UpToIn,
    DuctGauge Spiral2, DuctGauge Long2, DuctGauge Spiral4, DuctGauge Long4, DuctGauge Spiral10, DuctGauge Long10);

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctClass {
    public static readonly DuctClass Half  = new("wg-half", inchesWg: 0.5,  paClass: 125,  rectLimitIn: 36.0, steps: Seq((18.0, DuctGauge.Ga26), (20.0, DuctGauge.Ga24), (24.0, DuctGauge.Ga22), (26.0, DuctGauge.Ga20), (30.0, DuctGauge.Ga18), (36.0, DuctGauge.Ga16)), round: static (row, seam) => None);
    public static readonly DuctClass One   = new("wg-1",    inchesWg: 1.0,  paClass: 250,  rectLimitIn: 30.0, steps: Seq((12.0, DuctGauge.Ga26), (14.0, DuctGauge.Ga24), (18.0, DuctGauge.Ga22), (20.0, DuctGauge.Ga20), (26.0, DuctGauge.Ga18), (30.0, DuctGauge.Ga16)), round: static (row, seam) => None);
    public static readonly DuctClass Two   = new("wg-2",    inchesWg: 2.0,  paClass: 500,  rectLimitIn: 24.0, steps: Seq((10.0, DuctGauge.Ga26), (12.0, DuctGauge.Ga24), (14.0, DuctGauge.Ga22), (18.0, DuctGauge.Ga20), (20.0, DuctGauge.Ga18), (24.0, DuctGauge.Ga16)), round: static (row, seam) => Some(seam.Pick(row.Spiral2, row.Long2)));
    public static readonly DuctClass Three = new("wg-3",    inchesWg: 3.0,  paClass: 750,  rectLimitIn: 24.0, steps: Seq((10.0, DuctGauge.Ga24), (12.0, DuctGauge.Ga22), (14.0, DuctGauge.Ga20), (18.0, DuctGauge.Ga18), (24.0, DuctGauge.Ga16)),                        round: static (row, seam) => None);
    public static readonly DuctClass Four  = new("wg-4",    inchesWg: 4.0,  paClass: 1000, rectLimitIn: 18.0, steps: Seq((8.0, DuctGauge.Ga24), (10.0, DuctGauge.Ga22), (12.0, DuctGauge.Ga20), (16.0, DuctGauge.Ga18), (18.0, DuctGauge.Ga16)),                         round: static (row, seam) => Some(seam.Pick(row.Spiral4, row.Long4)));
    public static readonly DuctClass Six   = new("wg-6",    inchesWg: 6.0,  paClass: 1500, rectLimitIn: 16.0, steps: Seq((8.0, DuctGauge.Ga24), (10.0, DuctGauge.Ga20), (14.0, DuctGauge.Ga18), (16.0, DuctGauge.Ga16)),                                                 round: static (row, seam) => None);
    public static readonly DuctClass Ten   = new("wg-10",   inchesWg: 10.0, paClass: 2500, rectLimitIn: 12.0, steps: Seq((8.0, DuctGauge.Ga22), (10.0, DuctGauge.Ga18), (12.0, DuctGauge.Ga16)),                                                                          round: static (row, seam) => Some(seam.Pick(row.Spiral10, row.Long10)));

    [UseDelegateFromConstructor] public partial Option<DuctGauge> Round(RoundRow row, DuctSeam seam);
    public double InchesWg { get; }
    public int PaClass { get; }
    public double RectLimitIn { get; }
    public Seq<(double UpToIn, DuctGauge Gauge)> Steps { get; }
}

public static class DuctSchedule {
    public static readonly Attestation Attested = Attestation.PrimarySingle;

    public static readonly ImmutableArray<RoundRow> Round = [
        new(6.0,  DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28),
        new(8.0,  DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga28, DuctGauge.Ga26),
        new(10.0, DuctGauge.Ga28, DuctGauge.Ga26, DuctGauge.Ga28, DuctGauge.Ga26, DuctGauge.Ga28, DuctGauge.Ga26),
        new(12.0, DuctGauge.Ga28, DuctGauge.Ga26, DuctGauge.Ga28, DuctGauge.Ga26, DuctGauge.Ga26, DuctGauge.Ga24),
        new(14.0, DuctGauge.Ga28, DuctGauge.Ga26, DuctGauge.Ga26, DuctGauge.Ga24, DuctGauge.Ga26, DuctGauge.Ga24),
        new(16.0, DuctGauge.Ga26, DuctGauge.Ga24, DuctGauge.Ga26, DuctGauge.Ga24, DuctGauge.Ga24, DuctGauge.Ga22),
        new(18.0, DuctGauge.Ga26, DuctGauge.Ga24, DuctGauge.Ga24, DuctGauge.Ga24, DuctGauge.Ga24, DuctGauge.Ga22),
        new(26.0, DuctGauge.Ga26, DuctGauge.Ga24, DuctGauge.Ga24, DuctGauge.Ga22, DuctGauge.Ga24, DuctGauge.Ga22),
        new(36.0, DuctGauge.Ga24, DuctGauge.Ga22, DuctGauge.Ga22, DuctGauge.Ga20, DuctGauge.Ga22, DuctGauge.Ga20),
        new(50.0, DuctGauge.Ga22, DuctGauge.Ga20, DuctGauge.Ga20, DuctGauge.Ga20, DuctGauge.Ga20, DuctGauge.Ga20),
        new(60.0, DuctGauge.Ga20, DuctGauge.Ga18, DuctGauge.Ga18, DuctGauge.Ga18, DuctGauge.Ga18, DuctGauge.Ga18),
        new(84.0, DuctGauge.Ga18, DuctGauge.Ga16, DuctGauge.Ga18, DuctGauge.Ga16, DuctGauge.Ga18, DuctGauge.Ga16)];

    public static Option<DuctGauge> Rect(DuctClass @class, double longestIn) =>
        longestIn <= @class.RectLimitIn
            ? @class.Steps.Filter(step => longestIn <= step.UpToIn).Map(static step => step.Gauge).Head
            : None;

    public static Option<DuctGauge> RoundOf(DuctClass @class, double diameterIn, DuctSeam seam) =>
        toSeq(Round).Filter(row => diameterIn <= row.UpToIn).Head.Bind(row => @class.Round(row, seam));
}
```

## [03]-[DUCTWORK_SEED]

- Owner: `DuctSeal` and `DuctLiner` the product-class vocabularies; `DuctShape` the closed geometry payload; `DuctRow` the roster row; `DuctworkSeed` the roster, the seed law, and the capacity refusal.
- Cases: shape {round — diameter × seam, resolving `(class, diameter, seam)` through `DuctSchedule.RoundOf` onto `CircleHollow`; rectangular — width × depth, resolving `(class, longest side)` through `DuctSchedule.Rect` onto `RectangleHollow`}. The gauge is DEFINED by the schedule, so a stocked row cannot assert a sheet the class refuses, and a selection outside the unreinforced band faults typed at coherence time BESIDE every other offending row.
- Entry: `ComponentSeed.Rows(context, DuctworkSeed.Roster, DuctworkSeed.Law)` — one roster over one closed shape payload, so the two hand folds that differed only in geometry are one law.
- Law: the seal and liner selections on a stocked row are AUTHORED product spec — the SMACNA class→seal assignment rule and the liner thickness schedule are outside the corroborated set, so the tokens stamp under the row's own evidence grade and the assignment rule lands as data the moment it proves.
- Law: no `JointType` stamps — the transverse-joint and reinforcement vocabulary (codes A–L, slip-drive, flanged systems) is reinforcement-grade data out of the corroborated set, and the widened Realization allowed-set carries pipe modalities only.
- Output: the projector derives the takeoff rows from the solved hollow section through `QuantityRow.VolumePerLength`/`SurfaceAreaPerLength`/`LinearDensity` (`component#QUANTITY_ROW`) — no takeoff cell is stamped here.
- Boundary: every stamped bag rides `DuctSchedule.Attested` because the defining gauge cell is primary-single even where the pressure-class ladder itself is corroborated — the weakest contributor rules the row, per the `pipework#PIPEWORK_SEED` law, and the flag reads off the schedule owner rather than being asserted at the bag. `DuctworkSeed.Capacity` is the typed refusal — a duct run's governing verdict is airside, owned by `Rasm.Compute`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctSeal {
    public static readonly DuctSeal A        = new("seal-a");
    public static readonly DuctSeal B        = new("seal-b");
    public static readonly DuctSeal C        = new("seal-c");
    public static readonly DuctSeal Unsealed = new("unsealed");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctLiner {
    public static readonly DuctLiner Bare     = new("none");
    public static readonly DuctLiner Acoustic = new("acoustic");
}

[Union]
public abstract partial record DuctShape {
    private DuctShape() { }
    public sealed record Round(double DiameterIn, DuctSeam Seam) : DuctShape;
    public sealed record Rectangular(double WidthIn, double DepthIn) : DuctShape;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DuctRow(DuctShape Shape, DuctClass Class, DuctSeal Seal) {
    public DuctLiner Liner { get; init; } = DuctLiner.Bare;
    public EvidenceGrade Source { get; init; } = EvidenceGrade.Catalogue;

    public string Designation => Shape.Switch(
        round: r => $"ductwork.round-{Class.Key}-{Tag(r.DiameterIn)}",
        rectangular: r => $"ductwork.rect-{Class.Key}-{Tag(r.WidthIn)}x{Tag(r.DepthIn)}");

    static string Tag(double inches) => inches.ToString("00", CultureInfo.InvariantCulture);
}

// --- [TABLES] --------------------------------------------------------------------------
public static class DuctworkSeed {
    static readonly ComponentStandard UsSmacna =
        new(ComponentAuthority.Smacna.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.Smacna);
    static readonly IfcBinding Rigid = IfcBinding.Of("IfcDuctSegment", "RIGIDSEGMENT");
    static readonly MaterialId Galvanized = MaterialId.Create("steel.galvanized");
    static readonly MaterialId Sheet = MaterialId.Create("metal.steel");

    public static readonly Seq<DuctRow> Roster = Seq(
        new DuctRow(new DuctShape.Round(8.0,  DuctSeam.Spiral), DuctClass.Two,  DuctSeal.C),
        new DuctRow(new DuctShape.Round(12.0, DuctSeam.Spiral), DuctClass.Two,  DuctSeal.C),
        new DuctRow(new DuctShape.Round(16.0, DuctSeam.Spiral), DuctClass.Two,  DuctSeal.C),
        new DuctRow(new DuctShape.Round(20.0, DuctSeam.Spiral), DuctClass.Two,  DuctSeal.C),
        new DuctRow(new DuctShape.Round(24.0, DuctSeam.Spiral), DuctClass.Two,  DuctSeal.C),
        new DuctRow(new DuctShape.Round(12.0, DuctSeam.Spiral), DuctClass.Four, DuctSeal.A),
        new DuctRow(new DuctShape.Round(24.0, DuctSeam.Spiral), DuctClass.Four, DuctSeal.A),
        new DuctRow(new DuctShape.Round(12.0, DuctSeam.Spiral), DuctClass.Ten,  DuctSeal.A),
        new DuctRow(new DuctShape.Rectangular(12.0, 8.0),  DuctClass.Two,  DuctSeal.C),
        new DuctRow(new DuctShape.Rectangular(24.0, 12.0), DuctClass.One,  DuctSeal.C),
        new DuctRow(new DuctShape.Rectangular(30.0, 16.0), DuctClass.Half, DuctSeal.C));

    public static readonly SeedLaw<DuctRow> Law = SeedLaw<DuctRow>.Of(
        family: ComponentFamily.Ductwork,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: Profile,
        substance: static _ => Galvanized,
        source: static r => r.Source,
        standard: static _ => UsSmacna,
        detail: Some<Func<DuctRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        appearance: static _ => Sheet,
        ifc: static _ => Rigid);

    static Validation<Error, Unit> Coherence(DuctRow r, Op key) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(
                GaugeOf(r).IsSome,
                new KernelFault.InvalidValue(nameof(DuctGauge), "a gauge inside the unreinforced schedule", Some(key))),
            AdmissionSlots.Gate(
                r.Shape.Switch(
                    round: static x => double.IsFinite(x.DiameterIn) && x.DiameterIn > 0.0,
                    rectangular: static x => double.IsFinite(x.WidthIn) && x.WidthIn > 0.0 && double.IsFinite(x.DepthIn) && x.DepthIn > 0.0),
                new KernelFault.InvalidValue(nameof(r.Shape), "positive finite duct dimensions", Some(key)))));

    static Option<DuctGauge> GaugeOf(DuctRow r) => r.Shape.Switch(
        state: r.Class,
        round: static (@class, x) => DuctSchedule.RoundOf(@class, x.DiameterIn, x.Seam),
        rectangular: static (@class, x) => DuctSchedule.Rect(@class, Math.Max(x.WidthIn, x.DepthIn)));

    static Fin<SectionProfile> Profile(DuctRow r, Op key) =>
        from gauge in Gauge(r, key)
        from profile in r.Shape.Switch(
            state: (Gauge: gauge, Key: key),
            round: static (x, s) => SectionProfile.CircleHollow.Of(s.DiameterIn * ThreadRow.InchToMm, x.Gauge.ThicknessMm, x.Key),
            rectangular: static (x, s) => SectionProfile.RectangleHollow.Of(
                s.WidthIn * ThreadRow.InchToMm, s.DepthIn * ThreadRow.InchToMm, x.Gauge.ThicknessMm,
                innerFilletMm: 0.0, outerFilletMm: 0.0, x.Key))
        select profile;

    static Fin<PropertyBag> Detail(DuctRow r, SectionProfile profile, Op key) =>
        from gauge in Gauge(r, key)
        from wall in ComponentDetail.Measured(SegmentRows.WallThickness, Dimension.LengthDim, gauge.ThicknessMm * 1e-3)
        from diameter in r.Shape is DuctShape.Round round
            ? ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, round.DiameterIn * ThreadRow.InchToMm * 1e-3).Map(Some)
            : Fin.Succ(Option<(PropertyName, PropertyValue)>.None)
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.DuctGauge, gauge.Key),
            ComponentDetail.Token(DetailSchema.PressureClass, r.Class.Key),
            ComponentDetail.Token(DetailSchema.SealClass, r.Seal.Key),
            ComponentDetail.Token(DetailSchema.LinerClass, r.Liner.Key),
            ComponentDetail.Sourced(r.Source),
            SegmentRows.Attested(DuctSchedule.Attested),
            wall,
            .. diameter.ToSeq(),
        ]);

    static Fin<DuctGauge> Gauge(DuctRow r, Op key) =>
        GaugeOf(r).ToFin(new KernelFault.InvalidValue(nameof(DuctGauge), "a gauge inside the unreinforced schedule", Some(key)));

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [04]-[RESEARCH]

(none)
