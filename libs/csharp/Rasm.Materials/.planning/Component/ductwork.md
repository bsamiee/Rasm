# [MATERIALS_DUCTWORK]

THE DUCTWORK SEED PAGE owns the `ComponentFamily.Ductwork` fold (`ComponentClass.Minor`, `DetailLane.Product`): the buildable galvanized sheet-metal duct PRODUCT rows — pressure class, gauge, seal and liner classes, geometry — under the SMACNA duct-construction schedules. The pressure-class ladder (½″–10″ w.g. with its 125–2500 Pa metric twins) is corroborated law; the rectangular unreinforced-gauge schedule and the round Table 3-2A schedule are PRIMARY-SINGLE — transcribed from the CFR-incorporated primary text with no independent second posting — so every gauge-derived cell crosses the seam under the `Attestation.PrimarySingle` flag, never as falsely two-sourced. A component's gauge is RESOLVED from the schedule by its class, size, and seam, never asserted per stocked row. Airside SIZING — flow, friction, static-pressure balance — is `Rasm.Compute`'s; this page owns the buildable product rows alone.

The page composes settled law without re-derivation: `SectionProfile.CircleHollow.Of` and `SectionProfile.RectangleHollow.Of` are the railed geometry admissions and `Component.Of` the one construction rail (`component#COMPONENT_OWNER`); the product bag builds through `ComponentDetail.ProductRows` over the `Rasm.Element/Properties/property#DETAIL_SCHEMA` rows `DuctGauge`/`SealClass`/`LinerClass` with the duct pressure classification riding the shared `PressureClass` row; `Attestation` and the `SegmentRows` trade mints compose from `pipework#PIPE_SYSTEMS` and `pipework#PIPEWORK_SEED`; `ThreadRow.InchToMm` is the one inch basis (`fastener#FASTENER_FAMILY`); the IFC stamp is `IfcPipeSegment`'s duct sibling `IfcDuctSegment`/`RIGIDSEGMENT`; the substance binds `steel.galvanized` (the ASTM A653 lock-forming sheet row) and the appearance `metal.steel` at `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`.

## [01]-[INDEX]

- [02]-[DUCT_SCHEDULE]: the `DuctGauge` sheet ladder, the `DuctSeam` axis, the `DuctClass` pressure-class policy rows carrying the rectangular breakpoint schedule, the `RoundRow` Table 3-2A roster, and the `DuctSchedule` resolution folds.
- [03]-[DUCTWORK_SEED]: the `DuctSeal`/`DuctLiner` vocabularies, the `DuctworkDetail` product bag, the `DuctworkSeed.Rows` generation fold, and the typed sizing refusal.

## [02]-[DUCT_SCHEDULE]

- Owner: `DuctClass` the one pressure-class axis — inches w.g., the SMACNA-rounded Pa class label, the rectangular unreinforced breakpoint ladder with its reinforcement-required limit, and the round-schedule read for the classes the round tables publish; `DuctGauge` the galvanized sheet ladder with its SMACNA metric-twin thickness; `DuctSeam` the spiral/longitudinal axis; `DuctSchedule` the two resolution folds.
- Cases: seven classes {½ · 1 · 2 · 3 · 4 · 6 · 10 in w.g.} × two geometries; the round schedule exists only at +2/+4/+10 in w.g., so the other classes answer `None` at the round read and a round duct outside those classes is unmintable rather than silently re-classed.
- Law: the exact conversion is 1 in w.g. = 248.84 Pa and the class labels are SMACNA's own roundings (125/250/500/750/1000/1500/2500 Pa) — the label is a published token, never a recomputed value; the unassigned-design default is the 1 in w.g. class and VAV duct upstream of boxes the 2 in w.g. class.
- Law: a `None` from either resolution fold names the REINFORCEMENT-REQUIRED band — the reinforcement matrices (codes A–L, tie rods, joint spacing) are out of the corroborated set, so larger sizes refuse rather than carry a gauge the standard conditions on reinforcement this page does not model.
- Law: the gauge↔mm correspondence is the SMACNA metric twin, PRIMARY-SINGLE like the schedules; the connector `Gauges` table overlaps it at 22–16 ga with agreeing values, and both stand — one prints the AISI cold-formed design basis, the other the SMACNA duct-sheet basis, two authorities for two regimes.
- Exemption: flat-oval and aluminum duct carry no corroborated schedule — each joins as one stocked selection with its schedule roster the moment its table proves; no vocabulary row is minted for either now.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (the seam bag currencies), Rasm.Materials.Component parent owner (`Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile`/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`Provenance`, the sibling `Attestation`/`SegmentRows`, the `ThreadRow.InchToMm` inch basis), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]`), LanguageExt.Core, BCL (`ImmutableArray`). NO duct-schedule producer exists among admitted packages, so the schedules are PUBLISHED here under SEED_ROW_LAW with per-column provenance.
- Growth: a new pressure class is one `DuctClass` row; a heavier or lighter sheet one `DuctGauge` row; a negative-pressure round schedule one roster beside `RoundRow` read by the same class delegates.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;
using System.Globalization;             // the invariant designation-tag format
using LanguageExt;
using Rasm.Domain;                      // Op, Context
using Rasm.Element.Composition;         // MaterialId, DetailSchema, PropertyBag, PropertyName, PropertyValue
using Rasm.Element.Properties;          // MeasureValue, PropertyCategory
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis the detail-bag mints ride
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The galvanized duct-sheet ladder: gauge token + the SMACNA metric-twin thickness (PRIMARY-SINGLE with the
// schedules that consume it). The duct wall the section admits IS this thickness — no second wall column exists.
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

// The round-duct seam axis Table 3-2A splits its gauge columns on — the pick delegate keeps the column choice a row
// read rather than a consumer ternary.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctSeam {
    public static readonly DuctSeam Spiral       = new("spiral",       static (spiral, longitudinal) => spiral);
    public static readonly DuctSeam Longitudinal = new("longitudinal", static (spiral, longitudinal) => longitudinal);
    [UseDelegateFromConstructor] public partial DuctGauge Pick(DuctGauge spiral, DuctGauge longitudinal);
}

// --- [MODELS] ------------------------------------------------------------------------------
// One Table 3-2A band: the positive-pressure unreinforced round-duct gauge at +2/+4/+10 in w.g., spiral and
// longitudinal seam. Bands are ascending max-diameter; the first band covering a diameter rules it.
public readonly record struct RoundRow(
    double UpToIn,
    DuctGauge Spiral2, DuctGauge Long2, DuctGauge Spiral4, DuctGauge Long4, DuctGauge Spiral10, DuctGauge Long10);

// --- [TABLES] ------------------------------------------------------------------------------
// The pressure-class policy axis: inches w.g., the SMACNA-rounded Pa label, the rectangular unreinforced breakpoint
// ladder (first band covering the LONGEST side rules; beyond RectLimitIn reinforcement is required and the fold
// answers None), and the round-schedule read — Some only at the +2/+4/+10 classes the round tables publish. The
// breakpoints transcribe SMACNA Tables 1-3…1-9 column 2 cell-for-cell; the 4/6/10 in w.g. ladders index from 8 in
// and down, which is why their first band is (8, …).
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

// The two schedule resolution folds. Rect answers the first breakpoint covering the longest side, None past the
// unreinforced limit; RoundOf answers the first diameter band's class-and-seam cell, None where the class publishes
// no round table. Round transcribes Table 3-2A cell-for-cell across its twelve diameter bands.
public static class DuctSchedule {
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
            ? @class.Steps.Filter(step => longestIn <= step.UpToIn).Map(static step => step.Gauge).HeadOrNone()
            : None;

    public static Option<DuctGauge> RoundOf(DuctClass @class, double diameterIn, DuctSeam seam) =>
        toSeq(Round).Filter(row => diameterIn <= row.UpToIn).HeadOrNone().Bind(row => @class.Round(row, seam));
}
```

## [03]-[DUCTWORK_SEED]

- Owner: `DuctSeal` and `DuctLiner` the product-class vocabularies; `DuctworkDetail` the product-bag constructor; `DuctworkSeed` the stocked selections, the generation fold, and the capacity refusal.
- Cases: round stocked rows resolve `(class, diameter, seam)` through `DuctSchedule.RoundOf` onto `CircleHollow`; rectangular rows resolve `(class, longest side)` through `DuctSchedule.Rect` onto `RectangleHollow` — the gauge is Defined by the schedule, so a stocked row cannot assert a sheet the class refuses, and a selection outside the unreinforced band faults typed at seed time.
- Law: the seal and liner selections on a stocked row are AUTHORED product spec — the SMACNA class→seal assignment rule and the liner thickness schedule are outside the corroborated set, so the tokens stamp as this estate's selection under the row's `Sourced`/`Attested` evidence, and the assignment rule lands as data the moment it proves.
- Law: no `JointType` stamps — the transverse-joint and reinforcement vocabulary (codes A–L, slip-drive, flanged systems) is reinforcement-grade data out of the corroborated set, and the widened Realization allowed-set carries pipe modalities only.
- Entry: `DuctworkSeed.Rows(context)` the `ComponentFamily.Ductwork` row fold; `DuctworkSeed.Capacity` the typed refusal — a duct run's governing verdict is airside, owned by `Rasm.Compute`.
- Output: the projector derives the takeoff rows from the solved hollow section through `QuantityRow.VolumePerLength`/`SurfaceAreaPerLength`/`LinearDensity` (`component#QUANTITY_ROW`) — no takeoff cell is stamped here.
- Boundary: every stamped bag rides `Attestation.PrimarySingle` because the defining gauge cell is primary-single even where the pressure-class ladder itself is corroborated — the weakest contributor rules the row, per the `pipework#PIPEWORK_SEED` law.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The SMACNA seal-class tokens. The class→leakage-number correspondence and the class→pressure-class assignment
// rule are out of the corroborated set, so the rows carry identity alone and both facts join as columns when proven.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctSeal {
    public static readonly DuctSeal A        = new("seal-a");
    public static readonly DuctSeal B        = new("seal-b");
    public static readonly DuctSeal C        = new("seal-c");
    public static readonly DuctSeal Unsealed = new("unsealed");
}

// The liner axis: lined or not. Acoustic-liner thickness schedules are out of the corroborated set — thickness joins
// as a column when proven; the token alone crosses now.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DuctLiner {
    public static readonly DuctLiner Bare     = new("none");
    public static readonly DuctLiner Acoustic = new("acoustic");
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The DetailLane.Product bag: gauge/class/seal/liner tokens, provenance and attestation, the measured sheet wall,
// and the diameter row only where the duct is round — a rectangular duct's envelope rides its profile.
public static class DuctworkDetail {
    public static Fin<PropertyBag> Of(DuctClass @class, DuctGauge gauge, DuctSeal seal, DuctLiner liner, Option<double> diameterMm, Op key) =>
        from wall in ComponentDetail.Measured(SegmentRows.WallThickness, Dimension.LengthDim, gauge.ThicknessMm * 1e-3)
        from diameter in diameterMm.Match(
            Some: mm => ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, mm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.DuctGauge, gauge.Key),
            ComponentDetail.Token(DetailSchema.PressureClass, @class.Key),
            ComponentDetail.Token(DetailSchema.SealClass, seal.Key),
            ComponentDetail.Token(DetailSchema.LinerClass, liner.Key),
            ComponentDetail.Sourced(Provenance.Published),
            SegmentRows.Attested(Attestation.PrimarySingle),
            wall,
            .. diameter.ToSeq(),
        ]);
}

// The ComponentFamily.Ductwork generator: two stocked selections, gauge resolved from the schedule — never carried
// on the selection — and one construction rail per geometry. Designation tags carry the class and the size in
// inches, so the id reads as the submittal line it names.
public static class DuctworkSeed {
    static readonly ComponentStandard UsSmacna = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Smacna);
    static readonly IfcBinding Rigid = IfcBinding.Of("IfcDuctSegment", "RIGIDSEGMENT");
    static readonly MaterialId Galvanized = MaterialId.Of("steel.galvanized");
    static readonly MaterialId Sheet = MaterialId.Of("metal.steel");

    // Positive-pressure single-wall spiral supply at the three round-rated classes; the seal selection is authored
    // spec (heavier class, heavier seal) pending the SMACNA assignment rule.
    static readonly ImmutableArray<(double DiaIn, DuctClass Class, DuctSeam Seam, DuctSeal Seal)> Rounds = [
        (8.0,  DuctClass.Two,  DuctSeam.Spiral, DuctSeal.C),
        (12.0, DuctClass.Two,  DuctSeam.Spiral, DuctSeal.C),
        (16.0, DuctClass.Two,  DuctSeam.Spiral, DuctSeal.C),
        (20.0, DuctClass.Two,  DuctSeam.Spiral, DuctSeal.C),
        (24.0, DuctClass.Two,  DuctSeam.Spiral, DuctSeal.C),
        (12.0, DuctClass.Four, DuctSeam.Spiral, DuctSeal.A),
        (24.0, DuctClass.Four, DuctSeam.Spiral, DuctSeal.A),
        (12.0, DuctClass.Ten,  DuctSeam.Spiral, DuctSeal.A)];

    static readonly ImmutableArray<(double WidthIn, double DepthIn, DuctClass Class, DuctSeal Seal)> Rects = [
        (12.0, 8.0,  DuctClass.Two,  DuctSeal.C),
        (24.0, 12.0, DuctClass.One,  DuctSeal.C),
        (30.0, 16.0, DuctClass.Half, DuctSeal.C)];

    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        from rounds in toSeq(Rounds).Traverse(stock =>
            from gauge in DuctSchedule.RoundOf(stock.Class, stock.DiaIn, stock.Seam)
                .ToFin(ComponentFault.Family(context.Key, $"<round-gauge-outside-unreinforced-schedule:{stock.DiaIn:R}:{stock.Class.Key}>"))
            from profile in SectionProfile.CircleHollow.Of(stock.DiaIn * ThreadRow.InchToMm, gauge.ThicknessMm, context.Key)
            from detail in DuctworkDetail.Of(stock.Class, gauge, stock.Seal, DuctLiner.Bare, Some(stock.DiaIn * ThreadRow.InchToMm), context.Key)
            from item in Component.Of(
                ComponentFamily.Ductwork, $"ductwork.round-{stock.Class.Key}-{Tag(stock.DiaIn)}",
                profile, Rigid, Coring.None, UsSmacna,
                substanceId: Galvanized, appearanceId: Sheet, detail: Some(detail), context.Key)
            select new ComponentRow(item, Provenance.Published)).As()
        from rects in toSeq(Rects).Traverse(stock =>
            from gauge in DuctSchedule.Rect(stock.Class, Math.Max(stock.WidthIn, stock.DepthIn))
                .ToFin(ComponentFault.Family(context.Key, $"<rect-gauge-outside-unreinforced-schedule:{stock.WidthIn:R}x{stock.DepthIn:R}:{stock.Class.Key}>"))
            from profile in SectionProfile.RectangleHollow.Of(
                stock.WidthIn * ThreadRow.InchToMm, stock.DepthIn * ThreadRow.InchToMm, gauge.ThicknessMm, innerFilletMm: 0.0, outerFilletMm: 0.0, context.Key)
            from detail in DuctworkDetail.Of(stock.Class, gauge, stock.Seal, DuctLiner.Bare, None, context.Key)
            from item in Component.Of(
                ComponentFamily.Ductwork, $"ductwork.rect-{stock.Class.Key}-{Tag(stock.WidthIn)}x{Tag(stock.DepthIn)}",
                profile, Rigid, Coring.None, UsSmacna,
                substanceId: Galvanized, appearanceId: Sheet, detail: Some(detail), context.Key)
            select new ComponentRow(item, Provenance.Published)).As()
        select rounds + rects;

    // The ComponentFamily.Ductwork CAPACITY producer: an explicit typed refusal — a duct segment's governing verdict
    // is airside (flow, friction, leakage against its pressure class), owned by the Rasm.Compute route.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<ductwork-sizing-rides-compute-airside:{component.Designation.Value}>");

    static string Tag(double inches) => inches.ToString("00", CultureInfo.InvariantCulture);
}
```

## [04]-[RESEARCH]

(none)
