# [MATERIALS_PIPEWORK]

THE PIPEWORK SEED PAGE owns the `ComponentFamily.Pipework` fold (`ComponentClass.Minor`, `DetailLane.Product`): the buildable pressure-pipe PRODUCT rows — dimensions, materials, ratings — across seven material systems. Copper water tube rides ASTM B88 Types K/L/M over the copper-tube-size rule, steel A53/A106 and PVC D1785 and CPVC F441 share ONE ASME B36.10 iron-pipe-size ladder under Sch 40/80 wall selection, PEX F876 derives its SDR9 wall from the standard's own formula, cast-iron soil A74 carries the SV and XH hub-and-spigot services, and ductile AWWA C151 stands as an OD-and-class roster whose wall matrix is typed-absent. Every ladder GENERATES from one published roster per system — a size is a roster row and a system a policy row, never sixty hand-minted components. Hydraulic SIZING — flow, pressure drop, demand — is `Rasm.Compute`'s; this page owns the buildable product rows alone.

The page composes settled law without re-derivation: `SectionProfile.CircleHollow.Of` is the railed geometry admission and `Component.Of` the one construction rail (`component#COMPONENT_OWNER`); the product bag builds through `ComponentDetail.ProductRows` over the `Rasm.Element/Properties/property#DETAIL_SCHEMA` rows `PipeSchedule`/`PressureClass`/`NominalBore`, with the joint modality riding `JointType` over the widened `Realization` allowed-set (`Threaded`/`Grooved`/`Fused`/`Compression`/`Brazed`/`Bonded` — the solvent-cement modality the PVC/CPVC systems stamp — beside the structural `Cast`/`Welded` tokens); `ThreadRow.InchToMm` is the one inch basis (`fastener#FASTENER_FAMILY`); `Provenance` states each row's producer and the page-minted `Attestation` axis states its source count; the IFC stamp is `IfcPipeSegment` with `RIGIDSEGMENT` for rigid systems and `FLEXIBLESEGMENT` for PEX; substances bind `copper.c12200`/`steel.a53`/`pipe.pvc`/`pipe.cpvc`/`pipe.pex`/`iron.cast` at `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`.

## [01]-[INDEX]

- [02]-[PIPE_SYSTEMS]: the `Attestation` source-count axis, the `PipeSystem` policy row, and the four published rosters — `CtsRow` copper-tube sizes, `IpsRow` iron-pipe sizes, `SoilRow` hub-and-spigot services, `DuctileRow` OD-and-class — each carrying its derivation algebra.
- [03]-[PIPEWORK_SEED]: `SegmentRows` shared trade mints, the `PipeworkDetail` product bag, the `PipeworkSeed.Rows` generation fold, and the typed sizing refusal.

## [02]-[PIPE_SYSTEMS]

- Owner: `PipeSystem` the one policy axis — schedule token, default joint modality, IFC flexibility, provenance, per-column attestation, and both `MaterialId` slots per system; `Cts`/`Ips`/`Soil`/`Ductile` the published rosters; `Attestation` the SEED_ROW_LAW source-count axis every fluid-trade page composes.
- Cases: twelve minting systems {copper-k · copper-l · copper-m · steel-sch40 · steel-sch80 · pvc-sch40 · pvc-sch80 · cpvc-sch40 · cpvc-sch80 · pex-sdr9 · soil-sv · soil-xh} over four rosters; ductile is a ROSTER without a system row — its wall matrix conflicts between secondary sources, so no `CircleHollow` exists to admit and no component mints until the wall column lands `Corroborated`.
- Law: stored columns are only what the standards print independently — copper OD derives from the CTS rule (nominal + 1/8 in), PEX wall from the SDR9 formula under its 0.070 in floor, soil wall from the published barrel OD/ID pair — so a transcription slip in a derivable cell is unrepresentable and the derivation is the executable spec.
- Law: the copper working-pressure matrix lands typed-absent — its basis (P = 2·S·t_min/(D_max − 0.8·t_min), S = 41.4/71.0 MPa annealed/drawn at 100 °F) is PRIMARY-SINGLE and keys on tolerance minima the nominal roster does not carry, so a nominal-wall derivation overstates every rating; no derived pressure publishes.
- Exemption: an HDPE system (and the `Fused` joint token's first pipework consumer) joins as one `PipeSystem` row and one roster the moment its dimension pack proves; `pipe.hdpe` already resolves at the property catalogue.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (the seam bag currencies), Rasm.Materials.Component parent owner (`Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile`/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`Provenance`, the `ThreadRow.InchToMm` inch basis), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`), BCL (`ImmutableArray`). NO pipe-dimension producer exists among admitted packages, so the rosters are PUBLISHED here under SEED_ROW_LAW with per-column provenance.
- Growth: a new size is one roster row; a new schedule or material system one `PipeSystem` row wired into the `Rows` fold; a new service class one `SoilRow` column pair; the ductile wall matrix is one `Option` column flip from typed-absent to minting.
- Boundary: `Attestation` qualifies the TRANSCRIPTION, `Provenance` the PRODUCER — a value lands standards-published yet single-posted, and the two axes cross the seam as independent bag rows so a downstream reader never mistakes a primary-single cell for a corroborated one.

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
// The SEED_ROW_LAW source-count axis BESIDE Provenance: Provenance names WHO produced a value, Attestation states
// whether a second INDEPENDENT posting corroborates the transcription. A primary-single column crosses the seam
// flagged rather than dressed as two-sourced — the ductwork gauge schedules and the soil XH service are the standing
// consumers. And combines cell attestations onto one bag row: any primary-single contributor rules the row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Attestation {
    public static readonly Attestation Corroborated  = new("corroborated",   independent: true);
    public static readonly Attestation PrimarySingle = new("primary-single", independent: false);
    public bool Independent { get; }
    public Attestation And(Attestation other) => Independent && other.Independent ? Corroborated : PrimarySingle;
}

// The pipework policy axis: one row per minting material system. Schedule is the PipeSchedule bag token; Joint the
// system's default modality over the widened Realization allowed-set (a per-connection override is realization
// detail, never a type edit); Flexible selects the IfcPipeSegment predefined token; Dims/Rated the attestation of the
// dimension and rating columns (steel carries Rated None — the pack scope is dimensions only, so no rating stamps).
// PEX appearance rides plastic.pvc — the one smooth-polymer render row the library publishes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PipeSystem {
    public static readonly PipeSystem CopperK    = new("copper-k",    schedule: "type-k", joint: "Brazed",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: None,                               substanceId: "copper.c12200", appearanceId: "metal.copper");
    public static readonly PipeSystem CopperL    = new("copper-l",    schedule: "type-l", joint: "Brazed",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: None,                               substanceId: "copper.c12200", appearanceId: "metal.copper");
    public static readonly PipeSystem CopperM    = new("copper-m",    schedule: "type-m", joint: "Brazed",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: None,                               substanceId: "copper.c12200", appearanceId: "metal.copper");
    public static readonly PipeSystem SteelSch40 = new("steel-sch40", schedule: "sch40",  joint: "Threaded",    flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: None,                               substanceId: "steel.a53",     appearanceId: "metal.steel");
    public static readonly PipeSystem SteelSch80 = new("steel-sch80", schedule: "sch80",  joint: "Welded",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: None,                               substanceId: "steel.a53",     appearanceId: "metal.steel");
    public static readonly PipeSystem PvcSch40   = new("pvc-sch40",   schedule: "sch40",  joint: "Bonded",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: Some(Attestation.PrimarySingle),    substanceId: "pipe.pvc",      appearanceId: "plastic.pvc");
    public static readonly PipeSystem PvcSch80   = new("pvc-sch80",   schedule: "sch80",  joint: "Bonded",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: Some(Attestation.Corroborated),     substanceId: "pipe.pvc",      appearanceId: "plastic.pvc");
    public static readonly PipeSystem CpvcSch40  = new("cpvc-sch40",  schedule: "sch40",  joint: "Bonded",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: Some(Attestation.PrimarySingle),    substanceId: "pipe.cpvc",     appearanceId: "plastic.pvc");
    public static readonly PipeSystem CpvcSch80  = new("cpvc-sch80",  schedule: "sch80",  joint: "Bonded",      flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: Some(Attestation.Corroborated),     substanceId: "pipe.cpvc",     appearanceId: "plastic.pvc");
    public static readonly PipeSystem PexSdr9    = new("pex-sdr9",    schedule: "sdr9",   joint: "Compression", flexible: true,  source: Provenance.Defined,   dims: Attestation.Corroborated,  rated: Some(Attestation.Corroborated),     substanceId: "pipe.pex",      appearanceId: "plastic.pvc");
    public static readonly PipeSystem SoilSv     = new("soil-sv",     schedule: "sv",     joint: "Compression", flexible: false, source: Provenance.Published, dims: Attestation.Corroborated,  rated: None,                               substanceId: "iron.cast",     appearanceId: "metal.iron");
    public static readonly PipeSystem SoilXh     = new("soil-xh",     schedule: "xh",     joint: "Compression", flexible: false, source: Provenance.Published, dims: Attestation.PrimarySingle, rated: None,                               substanceId: "iron.cast",     appearanceId: "metal.iron");

    public string Schedule { get; }
    public string Joint { get; }
    public bool Flexible { get; }
    public Provenance Source { get; }
    public Attestation Dims { get; }
    public Option<Attestation> Rated { get; }
    public string SubstanceId { get; }
    public string AppearanceId { get; }
    public IfcBinding Ifc => IfcBinding.Of("IfcPipeSegment", Flexible ? "FLEXIBLESEGMENT" : "RIGIDSEGMENT");
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
}

// --- [MODELS] ------------------------------------------------------------------------------
// ASTM B88 / F876 copper-tube-size row: the CTS rule OD = nominal + 1/8 in is the standard's own correspondence, so
// only the nominal and the three published wall columns store; None = the type is not furnished at that size. The
// F876 SDR9 wall is OD/9 under the 0.070 in floor — the formula reproduces every printed F876 cell, so PEX carries
// Provenance.Defined and no wall column of its own.
public readonly record struct CtsRow(string Nominal, double NominalIn, Option<double> KWallIn, Option<double> LWallIn, Option<double> MWallIn, bool Pex) {
    public double OdIn => NominalIn + 0.125;
    public double OdMm => OdIn * ThreadRow.InchToMm;
    public double PexWallMm => Math.Max(OdIn / 9.0, 0.070) * ThreadRow.InchToMm;
}

// ASME B36.10 iron-pipe-size row shared by steel A53/A106, PVC D1785, and CPVC F441 — one OD ladder, two schedule
// walls, and the D1785/F441 73 °F water working-pressure pair (CPVC prints the same psi cells as PVC at 73 °F; the
// temperature derating that separates them is Rasm.Compute's). Sch 40 = STD through NPS 10, Sch 80 = XS through NPS 8.
public readonly record struct IpsRow(string Nps, double NpsIn, double OdIn, double Sch40In, double Sch80In, double Rated80Psi, double Rated40Psi) {
    public double OdMm => OdIn * ThreadRow.InchToMm;
}

// ASTM A74 hub-and-spigot soil-pipe row: both services on one row, wall DERIVED from the published barrel OD/ID pair
// so the two printed columns stay the authority. The hub ID is the joint envelope a fit check reads, kept per service.
public readonly record struct SoilRow(string Size, double SizeIn, double SvHubIn, double SvOdIn, double SvIdIn, double XhHubIn, double XhOdIn, double XhIdIn) {
    public double SvWallIn => (SvOdIn - SvIdIn) / 2.0;
    public double XhWallIn => (XhOdIn - XhIdIn) / 2.0;
}

// AWWA C151/A21.51 ductile row: the OD ladder and the five-class roster are corroborated; the wall matrix CONFLICTS
// between secondary sources cell-for-cell, so WallIn is typed-absent on every row and the system mints nothing.
public readonly record struct DuctileRow(string Size, double OdIn, Option<double> WallIn);

// --- [TABLES] ------------------------------------------------------------------------------
// ASTM B88 Table: 16 sizes, walls in the inches the standard prints. Type M is not furnished at 1/4 and 5/8.
public static class Cts {
    public static readonly ImmutableArray<CtsRow> Rows = [
        new("1/4",   0.25,  0.035, 0.030, None,  Pex: false),
        new("3/8",   0.375, 0.049, 0.035, 0.025, Pex: true),
        new("1/2",   0.5,   0.049, 0.040, 0.028, Pex: true),
        new("5/8",   0.625, 0.049, 0.042, None,  Pex: true),
        new("3/4",   0.75,  0.065, 0.045, 0.032, Pex: true),
        new("1",     1.0,   0.065, 0.050, 0.035, Pex: true),
        new("1-1/4", 1.25,  0.065, 0.055, 0.042, Pex: true),
        new("1-1/2", 1.5,   0.072, 0.060, 0.049, Pex: true),
        new("2",     2.0,   0.083, 0.070, 0.058, Pex: true),
        new("2-1/2", 2.5,   0.095, 0.080, 0.065, Pex: true),
        new("3",     3.0,   0.109, 0.090, 0.072, Pex: true),
        new("3-1/2", 3.5,   0.120, 0.100, 0.083, Pex: false),
        new("4",     4.0,   0.134, 0.114, 0.095, Pex: false),
        new("5",     5.0,   0.160, 0.125, 0.109, Pex: false),
        new("6",     6.0,   0.192, 0.140, 0.122, Pex: false),
        new("8",     8.0,   0.271, 0.200, 0.170, Pex: false)];
}

// ASME B36.10 ladder NPS 1/2–12 with the D1785/F441 73 °F psi pair. The Sch 40 rating column is PRIMARY-SINGLE
// (one direct D1785 posting; the second support is the same-geometry F441 schedule) — the PvcSch40/CpvcSch40 system
// rows carry that flag, so the cells stamp flagged rather than falsely two-sourced.
public static class Ips {
    public static readonly ImmutableArray<IpsRow> Rows = [
        new("1/2",   0.5,   0.840,  0.109, 0.147, 850.0, 600.0),
        new("3/4",   0.75,  1.050,  0.113, 0.154, 690.0, 480.0),
        new("1",     1.0,   1.315,  0.133, 0.179, 630.0, 450.0),
        new("1-1/4", 1.25,  1.660,  0.140, 0.191, 520.0, 370.0),
        new("1-1/2", 1.5,   1.900,  0.145, 0.200, 470.0, 330.0),
        new("2",     2.0,   2.375,  0.154, 0.218, 400.0, 280.0),
        new("2-1/2", 2.5,   2.875,  0.203, 0.276, 420.0, 300.0),
        new("3",     3.0,   3.500,  0.216, 0.300, 370.0, 260.0),
        new("4",     4.0,   4.500,  0.237, 0.337, 320.0, 220.0),
        new("6",     6.0,   6.625,  0.280, 0.432, 280.0, 180.0),
        new("8",     8.0,   8.625,  0.322, 0.500, 250.0, 160.0),
        new("10",    10.0,  10.750, 0.365, 0.593, 230.0, 140.0),
        new("12",    12.0,  12.750, 0.406, 0.687, 230.0, 130.0)];
}

// ASTM A74 hub-and-spigot: SV columns corroborated; the XH service is PRIMARY-SINGLE (the SoilXh system row carries
// the flag). Hubless soil pipe is CISPI 301 — a different standard, never a row here.
public static class Soil {
    public static readonly ImmutableArray<SoilRow> Rows = [
        new("2",  2.0,  2.94,  2.30,  1.96,  3.06,  2.38,  2.00),
        new("3",  3.0,  3.94,  3.30,  2.96,  4.19,  3.50,  3.00),
        new("4",  4.0,  4.94,  4.30,  3.94,  5.19,  4.50,  4.00),
        new("5",  5.0,  5.94,  5.30,  4.94,  6.19,  5.50,  5.00),
        new("6",  6.0,  6.94,  6.30,  5.94,  7.19,  6.50,  6.00),
        new("8",  8.0,  9.25,  8.38,  7.94,  9.50,  8.62,  8.00),
        new("10", 10.0, 11.38, 10.50, 9.94,  11.62, 10.75, 10.00),
        new("12", 12.0, 13.50, 12.50, 11.94, 13.75, 12.75, 12.00)];
}

// AWWA C151 OD ladder (18 sizes) + the five pressure classes, both corroborated; every WallIn is typed-absent under
// the cross-source wall-matrix conflict. The roster is the admission domain a future wall landing mints from —
// substance iron.ductile and the PressureClass tokens are already bound the moment a row's wall proves.
public static class Ductile {
    public static readonly ImmutableArray<int> Classes = [150, 200, 250, 300, 350];   // psi rated working pressure incl. surge allowance
    public static readonly ImmutableArray<DuctileRow> Rows = [
        new("3", 3.96, None),   new("4", 4.80, None),   new("6", 6.90, None),   new("8", 9.05, None),
        new("10", 11.10, None), new("12", 13.20, None), new("14", 15.30, None), new("16", 17.40, None),
        new("18", 19.50, None), new("20", 21.60, None), new("24", 25.80, None), new("30", 32.00, None),
        new("36", 38.30, None), new("42", 44.50, None), new("48", 50.80, None), new("54", 57.56, None),
        new("60", 61.61, None), new("64", 65.67, None)];
}
```

## [03]-[PIPEWORK_SEED]

- Owner: `SegmentRows` the Materials-scoped bag rows every fluid-segment trade shares; `PipeworkDetail` the product-bag constructor; `PipeworkSeed` the generation fold and the capacity refusal.
- Cases: one `Mint` rail for every system — `Copper` folds the CTS roster per wall selector, `Schedule` the IPS ladder per schedule selector, `Pex` the CTS PEX subset through the SDR9 derivation, `Cast` the soil roster per service selector — ~150 components from 55 roster rows, zero hand-minted.
- Law: `PressureClass` stamps only a class-designated system (the ductile PC tokens, once walls prove); a psi rating is a `WorkingPressure` Measured row, never a class token — the two facts have different shapes and one row cannot carry both honestly.
- Law: the bag's one `Attestation` row is the AND of its stamped cells' attestations — a corroborated dimension set under a primary-single rating reads primary-single, because the weakest contributor rules what a reader may rely on.
- Entry: `PipeworkSeed.Rows(context)` the `ComponentFamily.Pipework` row fold; `PipeworkSeed.Capacity` the typed refusal — a pipe run's hydraulic verdict rides `Rasm.Compute`, and the refusal names that route.
- Output: the projector derives the takeoff rows from the solved `CircleHollow` section through `QuantityRow.VolumePerLength`/`SurfaceAreaPerLength`/`LinearDensity` (`component#QUANTITY_ROW`) — no takeoff cell is stamped here.
- Boundary: `SegmentRows` mints through the owner-blessed `PropertyCategory.Materials` scope, so the wall, working-pressure, hub, and attestation rows are one vocabulary across pipework, ductwork, and electrical — a per-page `PropertyName.Create` respelling is the fork this owner closes.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The shared Materials-scoped trade rows: WallThickness the admitted wall SI value, WorkingPressure the rated
// pressure, Attested the source-count token beside every fluid-trade bag's Sourced row. Ductwork and electrical
// compose these rows; the seam DetailSchema statics stay the canonical names where one exists (NominalDiameter,
// PipeSchedule, NominalBore) and this class mints only the rows the seam does not name.
public static class SegmentRows {
    public static readonly PropertyName WallThickness   = PropertyCategory.Materials.Row("WallThickness");
    public static readonly PropertyName WorkingPressure = PropertyCategory.Materials.Row("WorkingPressure");
    public static (PropertyName, PropertyValue) Attested(Attestation attestation) =>
        (PropertyCategory.Materials.Row(nameof(Attestation)), new PropertyValue.Text(attestation.Key));
}

// The DetailLane.Product bag: schedule and bore tokens, the joint modality through the schema's closed allowed-set,
// provenance and attestation, and the measured OD/wall pair; the rating and hub rows ride only where the system
// publishes them, absent otherwise — never a zero.
public static class PipeworkDetail {
    public const double PsiPa = 6_894.757;   // the one psi→Pa basis every stamped rating converts on
    static readonly PropertyName Hub = PropertyCategory.Materials.Row("HubDiameter");

    public static Fin<PropertyBag> Of(PipeSystem system, string nominal, double odMm, double wallMm, Option<double> ratedPsi, Option<double> hubMm, Op key) =>
        from joint in ComponentDetail.Joint(system.Joint, key)
        from od in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, odMm * 1e-3)
        from wall in ComponentDetail.Measured(SegmentRows.WallThickness, Dimension.LengthDim, wallMm * 1e-3)
        from rated in Opt(ratedPsi, psi => ComponentDetail.Measured(SegmentRows.WorkingPressure, Dimension.PressureDim, psi * PsiPa))
        from hub in Opt(hubMm, mm => ComponentDetail.Measured(Hub, Dimension.LengthDim, mm * 1e-3))
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.PipeSchedule, system.Schedule),
            ComponentDetail.Token(DetailSchema.NominalBore, nominal),
            ComponentDetail.Sourced(system.Source),
            SegmentRows.Attested(system.Rated.Filter(_ => ratedPsi.IsSome).Map(system.Dims.And).IfNone(system.Dims)),
            joint, od, wall,
            .. rated.ToSeq(),
            .. hub.ToSeq(),
        ]);

    static Fin<Option<(PropertyName, PropertyValue)>> Opt(Option<double> value, Func<double, Fin<(PropertyName, PropertyValue)>> mint) =>
        value.Match(Some: v => mint(v).Map(Some), None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None));
}

// The ComponentFamily.Pipework generator: one Mint rail, four roster folds, the system→roster wiring in ONE Rows
// expression. The designation tag is the nominal in decimal mils ("1/2" -> 00500), derived so the tag can never name
// a size the row does not carry.
public static class PipeworkSeed {
    public const double PexRatedPsi = 160.0;   // F876 standard rating at 73 °F; the 100 psi/180 °F and 80 psi/200 °F rungs are Compute-side temperature derating
    static readonly ComponentStandard UsAstm = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Seq(Copper(PipeSystem.CopperK, static r => r.KWallIn, context),
            Copper(PipeSystem.CopperL, static r => r.LWallIn, context),
            Copper(PipeSystem.CopperM, static r => r.MWallIn, context),
            Schedule(PipeSystem.SteelSch40, static r => r.Sch40In, static r => r.Rated40Psi, context),
            Schedule(PipeSystem.SteelSch80, static r => r.Sch80In, static r => r.Rated80Psi, context),
            Schedule(PipeSystem.PvcSch40,   static r => r.Sch40In, static r => r.Rated40Psi, context),
            Schedule(PipeSystem.PvcSch80,   static r => r.Sch80In, static r => r.Rated80Psi, context),
            Schedule(PipeSystem.CpvcSch40,  static r => r.Sch40In, static r => r.Rated40Psi, context),
            Schedule(PipeSystem.CpvcSch80,  static r => r.Sch80In, static r => r.Rated80Psi, context),
            Pex(context),
            Cast(PipeSystem.SoilSv, static r => (r.SvOdIn, r.SvWallIn, r.SvHubIn), context),
            Cast(PipeSystem.SoilXh, static r => (r.XhOdIn, r.XhWallIn, r.XhHubIn), context))
        .Traverse(static fold => fold).As()
        .Map(static folds => folds.Bind(static rows => rows));

    // The ComponentFamily.Pipework CAPACITY producer: an explicit typed refusal — a pipe segment's governing verdict
    // is hydraulic (flow, pressure drop, surge), owned by the Rasm.Compute fluid route, and pricing it off a section
    // integral here would certify a structural answer to a hydraulic question.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<pipework-sizing-rides-compute-hydraulic:{component.Designation.Value}>");

    static Fin<Seq<ComponentRow>> Copper(PipeSystem system, Func<CtsRow, Option<double>> wall, Context context) =>
        toSeq(Cts.Rows)
            .Bind(row => wall(row).Map(w => (Row: row, WallIn: w)).ToSeq())
            .Traverse(sized => Mint(system, sized.Row.Nominal, sized.Row.NominalIn, sized.Row.OdMm, sized.WallIn * ThreadRow.InchToMm, None, None, context)).As();

    static Fin<Seq<ComponentRow>> Schedule(PipeSystem system, Func<IpsRow, double> wall, Func<IpsRow, double> rated, Context context) =>
        toSeq(Ips.Rows)
            .Traverse(row => Mint(system, row.Nps, row.NpsIn, row.OdMm, wall(row) * ThreadRow.InchToMm, system.Rated.Map(_ => rated(row)), None, context)).As();

    static Fin<Seq<ComponentRow>> Pex(Context context) =>
        toSeq(Cts.Rows).Filter(static r => r.Pex)
            .Traverse(row => Mint(PipeSystem.PexSdr9, row.Nominal, row.NominalIn, row.OdMm, row.PexWallMm, Some(PexRatedPsi), None, context)).As();

    static Fin<Seq<ComponentRow>> Cast(PipeSystem system, Func<SoilRow, (double OdIn, double WallIn, double HubIn)> service, Context context) =>
        toSeq(Soil.Rows)
            .Traverse(row => Mint(system, row.Size, row.SizeIn,
                service(row).OdIn * ThreadRow.InchToMm, service(row).WallIn * ThreadRow.InchToMm,
                None, Some(service(row).HubIn * ThreadRow.InchToMm), context)).As();

    static Fin<ComponentRow> Mint(PipeSystem system, string nominal, double nominalIn, double odMm, double wallMm, Option<double> ratedPsi, Option<double> hubMm, Context context) =>
        from profile in SectionProfile.CircleHollow.Of(odMm, wallMm, context.Key)
        from detail in PipeworkDetail.Of(system, nominal, odMm, wallMm, ratedPsi, hubMm, context.Key)
        from item in Component.Of(
            ComponentFamily.Pipework, $"pipework.{system.Key}-{(nominalIn * 1000.0).ToString("00000", CultureInfo.InvariantCulture)}",
            profile, system.Ifc, Coring.None, UsAstm,
            substanceId: system.Substance, appearanceId: system.Appearance,
            detail: Some(detail), context.Key)
        select new ComponentRow(item, system.Source);
}
```

## [04]-[RESEARCH]

(none)
