# [MATERIALS_PIPEWORK]

THE PIPEWORK SEED PAGE owns the `ComponentFamily.Pipework` row facts (`ComponentClass.Minor`, `DetailLane.Product`): the buildable pressure-pipe PRODUCT rows — dimensions, materials, ratings — across seven material systems. Copper water tube rides ASTM B88 Types K/L/M over the copper-tube-size rule, steel A53/A106 and PVC D1785 and CPVC F441 share ONE ASME B36.10 iron-pipe-size ladder under Sch 40/80 wall selection, PEX F876 derives its SDR9 wall from the standard's own formula, cast-iron soil A74 carries the SV and XH hub-and-spigot services, and ductile AWWA C151 stands as an OD-and-class roster whose wall matrix is typed-absent. Every ladder GENERATES from one published roster per system: the SIZING is a `PipeSystem` row's own delegate column, so `PipeworkSeed.Roster` is the flattened cross product and a new system is ONE row with no second edit anywhere. Hydraulic SIZING — flow, pressure drop, demand — is `Rasm.Compute`'s; this page owns the buildable product rows alone.

The page composes settled law without re-derivation: `SectionProfile.CircleHollow.Of` is the fallible geometry admission and `component#COMPONENT_SEED` the ONE generator fold; the product bag builds through `ComponentDetail.ProductRows` over the `Rasm.Element/Properties/property#DETAIL_SCHEMA` rows `PipeSchedule`/`PressureClass`/`NominalBore`, with the joint modality riding `JointType` over the widened `Realization` allowed-set (`Threaded`/`Grooved`/`Fused`/`Compression`/`Brazed`/`Bonded` — the solvent-cement modality the PVC/CPVC systems stamp — beside the structural `Cast`/`Welded` tokens); `ThreadRow.InchToMm` is the one inch basis (`fastener#FASTENER_FAMILY`); the contract `EvidenceGrade` states each row's producer and the page-minted `Attestation` axis its source count; the IFC stamp is `IfcPipeSegment` with `RIGIDSEGMENT` for rigid systems and `FLEXIBLESEGMENT` for PEX; substances bind `copper.c12200`/`steel.a53`/`pipe.pvc`/`pipe.cpvc`/`pipe.pex`/`iron.cast` at `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`.

## [01]-[INDEX]

- [02]-[PIPE_SYSTEMS]: the `Attestation` source-count axis, the `PipeSystem` policy row with its sizing delegate, the `PipeSize`/`PipeRow` seed currencies, and the four published rosters — `CtsRow` copper-tube sizes, `IpsRow` iron-pipe sizes, `SoilRow` hub-and-spigot services, `DuctileRow` OD-and-class — each carrying its derivation algebra.
- [03]-[PIPEWORK_SEED]: `SegmentRows` the shared trade mints, and `PipeworkSeed` — the flattened roster, the seed law with its accumulating coherence and product bag, and the typed sizing refusal.

## [02]-[PIPE_SYSTEMS]

- Owner: `PipeSystem` the one policy axis — schedule token, default joint modality, IFC flexibility, evidence grade, per-column attestation, both `MaterialId` slots, and the SIZING delegate that produces the system's own rungs; `Cts`/`Ips`/`Soil`/`Ductile` the published rosters; `PipeSize` the sized rung and `PipeRow` the system × rung seed row; `Attestation` the SEED_ROW_LAW source-count axis every fluid-trade page composes.
- Cases: twelve minting systems {copper-k · copper-l · copper-m · steel-sch40 · steel-sch80 · pvc-sch40 · pvc-sch80 · cpvc-sch40 · cpvc-sch80 · pex-sdr9 · soil-sv · soil-xh} over four rosters; ductile is a ROSTER without a system row — its wall matrix conflicts between secondary sources, so no `CircleHollow` exists to admit and no component mints until the wall column lands `Corroborated`.
- Law: stored columns are only what the standards print independently — copper OD derives from the CTS rule (nominal + 1/8 in), PEX wall from the SDR9 formula under its 0.070 in floor, soil wall from the published barrel OD/ID pair — so a transcription slip in a derivable cell is unrepresentable and the derivation is the executable spec.
- Law: the copper working-pressure matrix lands typed-absent — its basis (P = 2·S·t_min/(D_max − 0.8·t_min), S = 41.4/71.0 MPa annealed/drawn at 100 °F) is PRIMARY-SINGLE and keys on tolerance minima the nominal roster does not carry, so a nominal-wall derivation overstates every rating; no derived pressure publishes.
- Law: a system's RATING attestation and its sized rows' rating cells are ONE fact read at two owners — the sizing delegate stamps a psi cell exactly where the `Rated` column declares one, and the seed coherence proves the correspondence per row, so the bag's attestation fold reads the declared column with no second presence test.
- Exemption: an HDPE system (and the `Fused` joint token's first pipework consumer) joins as one `PipeSystem` row the moment its dimension pack proves; `pipe.hdpe` already resolves at the property catalogue.
- Packages: Rasm.Domain (`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, the contract bag currencies), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED` owners (and the `ThreadRow.InchToMm` inch basis), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]`), LanguageExt.Core (`Validation`/`Fin`/`Seq`/`Option`), BCL (`ImmutableArray`, `FrozenDictionary`). NO pipe-dimension producer exists among admitted packages, so the rosters are PUBLISHED here under SEED_ROW_LAW with per-column provenance.
- Growth: a new size is one roster row; a new schedule or material system one `PipeSystem` row carrying its own sizing delegate — the roster flattens `Items`, so nothing else edits; a new service class one `SoilRow` column pair; the ductile wall matrix is one `Option` column flip from typed-absent to minting.
- Boundary: `Attestation` qualifies the TRANSCRIPTION, `EvidenceGrade` the PRODUCER — a value lands standards-published yet single-posted, and the two axes cross the boundary as independent bag rows so a downstream reader never mistakes a primary-single cell for a corroborated one.

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
public sealed partial class Attestation {
    public static readonly Attestation Corroborated  = new("corroborated",   independent: true);
    public static readonly Attestation PrimarySingle = new("primary-single", independent: false);
    public bool Independent { get; }
    public Attestation And(Attestation other) => Independent && other.Independent ? Corroborated : PrimarySingle;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PipeSize(
    string Nominal, double NominalIn, double OdMm, double WallMm, Option<double> RatedPsi, Option<double> HubMm);

public readonly record struct PipeRow(PipeSystem System, PipeSize Size) {
    public string Designation =>
        $"pipework.{System.Key}-{(Size.NominalIn * 1000.0).ToString("00000", CultureInfo.InvariantCulture)}";
}

public readonly record struct CtsRow(string Nominal, double NominalIn, Option<double> KWallIn, Option<double> LWallIn, Option<double> MWallIn, bool Pex) {
    public double OdIn => NominalIn + 0.125;
    public double OdMm => OdIn * ThreadRow.InchToMm;
    public double PexWallMm => Math.Max(OdIn / 9.0, 0.070) * ThreadRow.InchToMm;
}

public readonly record struct IpsRow(string Nps, double NpsIn, double OdIn, double Sch40In, double Sch80In, double Rated80Psi, double Rated40Psi) {
    public double OdMm => OdIn * ThreadRow.InchToMm;
}

public readonly record struct SoilRow(string Size, double SizeIn, double SvHubIn, double SvOdIn, double SvIdIn, double XhHubIn, double XhOdIn, double XhIdIn) {
    public double SvWallIn => (SvOdIn - SvIdIn) / 2.0;
    public double XhWallIn => (XhOdIn - XhIdIn) / 2.0;
}

public readonly record struct DuctileRow(string Size, double OdIn, Option<double> WallIn);

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PipeSystem {
    public const double PexRatedPsi = 160.0;

    public static readonly PipeSystem CopperK    = new("copper-k",    schedule: "type-k", joint: "Brazed",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: None,                            substanceId: "copper.c12200", appearanceId: "metal.copper",  sizes: static () => CopperSizes(static r => r.KWallIn));
    public static readonly PipeSystem CopperL    = new("copper-l",    schedule: "type-l", joint: "Brazed",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: None,                            substanceId: "copper.c12200", appearanceId: "metal.copper",  sizes: static () => CopperSizes(static r => r.LWallIn));
    public static readonly PipeSystem CopperM    = new("copper-m",    schedule: "type-m", joint: "Brazed",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: None,                            substanceId: "copper.c12200", appearanceId: "metal.copper",  sizes: static () => CopperSizes(static r => r.MWallIn));
    public static readonly PipeSystem SteelSch40 = new("steel-sch40", schedule: "sch40",  joint: "Threaded",    flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: None,                            substanceId: "steel.a53",     appearanceId: "metal.steel",   sizes: static () => IpsSizes(static r => r.Sch40In, None));
    public static readonly PipeSystem SteelSch80 = new("steel-sch80", schedule: "sch80",  joint: "Welded",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: None,                            substanceId: "steel.a53",     appearanceId: "metal.steel",   sizes: static () => IpsSizes(static r => r.Sch80In, None));
    public static readonly PipeSystem PvcSch40   = new("pvc-sch40",   schedule: "sch40",  joint: "Bonded",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: Some(Attestation.PrimarySingle), substanceId: "pipe.pvc",      appearanceId: "plastic.pvc",   sizes: static () => IpsSizes(static r => r.Sch40In, Rated40));
    public static readonly PipeSystem PvcSch80   = new("pvc-sch80",   schedule: "sch80",  joint: "Bonded",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: Some(Attestation.Corroborated),  substanceId: "pipe.pvc",      appearanceId: "plastic.pvc",   sizes: static () => IpsSizes(static r => r.Sch80In, Rated80));
    public static readonly PipeSystem CpvcSch40  = new("cpvc-sch40",  schedule: "sch40",  joint: "Bonded",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: Some(Attestation.PrimarySingle), substanceId: "pipe.cpvc",     appearanceId: "plastic.pvc",   sizes: static () => IpsSizes(static r => r.Sch40In, Rated40));
    public static readonly PipeSystem CpvcSch80  = new("cpvc-sch80",  schedule: "sch80",  joint: "Bonded",      flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: Some(Attestation.Corroborated),  substanceId: "pipe.cpvc",     appearanceId: "plastic.pvc",   sizes: static () => IpsSizes(static r => r.Sch80In, Rated80));
    public static readonly PipeSystem PexSdr9    = new("pex-sdr9",    schedule: "sdr9",   joint: "Compression", flexible: true,  source: EvidenceGrade.Defined,   dims: Attestation.Corroborated,  rated: Some(Attestation.Corroborated),  substanceId: "pipe.pex",      appearanceId: "plastic.pvc",   sizes: static () => PexSizes());
    public static readonly PipeSystem SoilSv     = new("soil-sv",     schedule: "sv",     joint: "Compression", flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.Corroborated,  rated: None,                            substanceId: "iron.cast",     appearanceId: "metal.iron",    sizes: static () => SoilSizes(static r => (r.SvOdIn, r.SvWallIn, r.SvHubIn)));
    public static readonly PipeSystem SoilXh     = new("soil-xh",     schedule: "xh",     joint: "Compression", flexible: false, source: EvidenceGrade.Catalogue, dims: Attestation.PrimarySingle, rated: None,                            substanceId: "iron.cast",     appearanceId: "metal.iron",    sizes: static () => SoilSizes(static r => (r.XhOdIn, r.XhWallIn, r.XhHubIn)));

    [UseDelegateFromConstructor] public partial Seq<PipeSize> Sizes();
    public string Schedule { get; }
    public string Joint { get; }
    public bool Flexible { get; }
    public EvidenceGrade Source { get; }
    public Attestation Dims { get; }
    public Option<Attestation> Rated { get; }
    public string SubstanceId { get; }
    public string AppearanceId { get; }
    public IfcBinding Ifc => IfcBinding.Of("IfcPipeSegment", Flexible ? "FLEXIBLESEGMENT" : "RIGIDSEGMENT");
    public MaterialId Substance => MaterialId.Create(SubstanceId);
    public MaterialId Appearance => MaterialId.Create(AppearanceId);

    // --- [LADDERS]
    static readonly Option<Func<IpsRow, double>> Rated40 = Some<Func<IpsRow, double>>(static r => r.Rated40Psi);
    static readonly Option<Func<IpsRow, double>> Rated80 = Some<Func<IpsRow, double>>(static r => r.Rated80Psi);

    static Seq<PipeSize> CopperSizes(Func<CtsRow, Option<double>> wall) =>
        toSeq(Cts.Rows).Bind(row => wall(row)
            .Map(inches => new PipeSize(row.Nominal, row.NominalIn, row.OdMm, inches * ThreadRow.InchToMm, None, None))
            .ToSeq());

    static Seq<PipeSize> IpsSizes(Func<IpsRow, double> wall, Option<Func<IpsRow, double>> rated) =>
        toSeq(Ips.Rows).Map(row => new PipeSize(
            row.Nps, row.NpsIn, row.OdMm, wall(row) * ThreadRow.InchToMm, rated.Map(pick => pick(row)), None));

    static Seq<PipeSize> PexSizes() =>
        toSeq(Cts.Rows).Filter(static r => r.Pex).Map(static row => new PipeSize(
            row.Nominal, row.NominalIn, row.OdMm, row.PexWallMm, Some(PexRatedPsi), None));

    static Seq<PipeSize> SoilSizes(Func<SoilRow, (double OdIn, double WallIn, double HubIn)> service) =>
        toSeq(Soil.Rows).Map(row => {
            (double odIn, double wallIn, double hubIn) = service(row);
            return new PipeSize(row.Size, row.SizeIn, odIn * ThreadRow.InchToMm, wallIn * ThreadRow.InchToMm,
                None, Some(hubIn * ThreadRow.InchToMm));
        });
}

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

public static class Ductile {
    public static readonly ImmutableArray<int> Classes = [150, 200, 250, 300, 350];
    public static readonly ImmutableArray<DuctileRow> Rows = [
        new("3", 3.96, None),   new("4", 4.80, None),   new("6", 6.90, None),   new("8", 9.05, None),
        new("10", 11.10, None), new("12", 13.20, None), new("14", 15.30, None), new("16", 17.40, None),
        new("18", 19.50, None), new("20", 21.60, None), new("24", 25.80, None), new("30", 32.00, None),
        new("36", 38.30, None), new("42", 44.50, None), new("48", 50.80, None), new("54", 57.56, None),
        new("60", 61.61, None), new("64", 65.67, None)];
}
```

## [03]-[PIPEWORK_SEED]

- Owner: `SegmentRows` the Materials-scoped bag rows every fluid-segment trade shares; `PipeworkSeed` the flattened roster, the seed law (coherence, profile, product bag), and the capacity refusal.
- Entry: `ComponentSeed.Rows(context, PipeworkSeed.Roster, PipeworkSeed.Law)` — the roster is `PipeSystem.Items` flattened through each row's own sizing delegate, so ~150 components come from 55 published roster rows with zero hand mints and the `Rows` fold that hand-wired twelve systems is gone.
- Law: `PressureClass` stamps only a class-designated system (the ductile PC tokens, once walls prove); a psi rating is a `WorkingPressure` Measured row, never a class token — the two facts have different shapes and one row cannot carry both honestly.
- Law: the bag's one `Attestation` row is the AND of its stamped cells' attestations — a corroborated dimension set under a primary-single rating reads primary-single, because the weakest contributor rules what a reader may rely on. The rating presence is the seed coherence's own proof, so the fold reads the declared column rather than re-testing the cell.
- Output: the projector derives the takeoff rows from the solved `CircleHollow` section through `QuantityRow.VolumePerLength`/`SurfaceAreaPerLength`/`LinearDensity` (`component#QUANTITY_ROW`) — no takeoff cell is stamped here.
- Boundary: `SegmentRows` mints through the owner-blessed `PropertyCategory.Materials` scope, so the wall, working-pressure, and attestation rows are one vocabulary across pipework, ductwork, and electrical — a per-page `PropertyName.Create` respelling is the fork this owner closes. `PipeworkSeed.Capacity` is the typed refusal — a pipe run's hydraulic verdict rides `Rasm.Compute`, and the refusal names that route.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SegmentRows {
    public static readonly PropertyName WallThickness   = PropertyCategory.Materials.Row("WallThickness");
    public static readonly PropertyName WorkingPressure = PropertyCategory.Materials.Row("WorkingPressure");
    public static (PropertyName, PropertyValue) Attested(Attestation attestation) =>
        (PropertyCategory.Materials.Row(nameof(Attestation)), new PropertyValue.Text(attestation.Key));
}

// --- [TABLES] --------------------------------------------------------------------------
public static class PipeworkSeed {
    public const double PsiPa = 6_894.757;
    static readonly ComponentStandard UsAstm =
        new(ComponentAuthority.Astm.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.Astm);
    static readonly PropertyName Hub = PropertyCategory.Materials.Row("HubDiameter");

    public static readonly Seq<PipeRow> Roster =
        toSeq(PipeSystem.Items).Bind(static system => system.Sizes().Map(size => new PipeRow(system, size)));

    public static readonly SeedLaw<PipeRow> Law = SeedLaw<PipeRow>.Of(
        family: ComponentFamily.Pipework,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: static (r, key) => SectionProfile.CircleHollow.Of(r.Size.OdMm, r.Size.WallMm),
        substance: static r => r.System.Substance,
        source: static r => r.System.Source,
        standard: static _ => UsAstm,
        detail: Some<Func<PipeRow, SectionProfile, Fin<PropertyBag>>>(Detail),
        appearance: static r => r.System.Appearance,
        ifc: static r => r.System.Ifc);

    static Validation<Error, Unit> Coherence(PipeRow r) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(
                r.Size.RatedPsi.IsSome == r.System.Rated.IsSome,
                new KernelFault.InvalidValue(nameof(r.Size.RatedPsi), "presence matching the pipe-system rating")),
            AdmissionSlots.Gate(
                double.IsFinite(r.Size.OdMm) && double.IsFinite(r.Size.WallMm)
                    && r.Size.WallMm > 0.0 && r.Size.OdMm > 2.0 * r.Size.WallMm,
                new KernelFault.InvalidValue(nameof(r.Size), "a positive finite annulus"))));

    static Fin<PropertyBag> Detail(PipeRow r, SectionProfile profile) =>
        from joint in ComponentDetail.Joint(r.System.Joint, key)
        from od in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, r.Size.OdMm * 1e-3)
        from wall in ComponentDetail.Measured(SegmentRows.WallThickness, Dimension.LengthDim, r.Size.WallMm * 1e-3)
        from rated in r.Size.RatedPsi.TraverseM(static psi => ComponentDetail.Measured(SegmentRows.WorkingPressure, Dimension.PressureDim, psi * PsiPa)).As()
        from hub in r.Size.HubMm.TraverseM(static mm => ComponentDetail.Measured(Hub, Dimension.LengthDim, mm * 1e-3)).As()
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.PipeSchedule, r.System.Schedule),
            ComponentDetail.Token(DetailSchema.NominalBore, r.Size.Nominal),
            ComponentDetail.Sourced(r.System.Source),
            SegmentRows.Attested(r.System.Rated.Map(r.System.Dims.And).IfNone(r.System.Dims)),
            joint, od, wall,
            .. rated.ToSeq(),
            .. hub.ToSeq(),
        ]);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        new ComponentFault.CapacityUnavailable(component.Designation);
}
```

## [04]-[RESEARCH]

(none)
