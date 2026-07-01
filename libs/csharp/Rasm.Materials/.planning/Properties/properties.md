# [MATERIALS_PROPERTIES]

THE TYPED-ENGINEERING-PROPERTY SOURCE. The typed engineering-property family is SEAM-owned: the `Rasm.Element` `MaterialPropertySet` `[Union]` (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`, keyed to the one `Discipline`) is the canonical material-physics carrier the `Material` node holds, the seam `MeasureValue` is the SI-coerced measure each dimensional column carries ([H2]), and the intrinsic single-material acoustic folds (`Nrc`/`Saa`/`StcWeighted`/`Rw` over the shared `RatingContour.Fit` contour kernel) live on the seam `Acoustic` case (`Rasm.Element` `Composition/acoustic`) — so the migration source's Materials-owned `MaterialProperty` `[Union]`, its `MaterialPropertyKind` coercion enum, and its acoustic projection folds are RETIRED, ROUTED into the seam. This owner is now the Materials SOURCE: one `MaterialPropertyCatalogue` is the registered-row database of known-material engineering data (the published mechanical/thermal/acoustic/fire values per `MaterialId`, grounded in EN 1993-1-1 / EN 1993-1-4 / EN 1992-1-1 / EN 10080 / EN 338 / EN 14080 / EN 1999-1-1 / EN 771 / EN 572 / EN ISO 10456 and the published acoustic/fire datasheets) the `Admit` lowering coerces into the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire` cases, composing `UnitsNet` quantities and `VividOrange.Uncertainties` at the Materials boundary before lowering to seam `MeasureValue`/`MeasureBand` and `PropertyEvidence`, and `Lookup` is the projector-facing resolution the `Projection/component#COMPONENT_PROJECTOR` calls. A material's engineering properties are NEVER a per-discipline material type: a wall material carries its conductivity, sound-transmission spectrum, fire rating, and structural grade as one `Seq<MaterialPropertySet>` over one `MaterialId`, a full engineering object the projector lowers into the seam `Material` node, never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface; the multi-ply `AssemblyAggregator` (the series-resistance U-value, layered-STC, rule-of-mixtures, and lifecycle GWP/cost folds) is RELOCATED to `Rasm.Compute` (the seam carries the per-material INPUT, never the assembly aggregation). The page composes the seam smart-constructors (`MaterialPropertySet.OfMechanical`/`OfThermal`/`OfAcoustic`/`OfFire`, `FireRating.Parse`, `SmokeClass.TryGet`/`DropletClass.TryGet`, the `FireResistance` ctor, `Acoustic.Of`) and the seam types they carry (`Discipline`/`AcousticBand`/`MaterialId`) — dimensional values are built as `MeasureValue` with neutral uncertainty bounds, and provider uncertainty types do not cross into `Rasm.Element` — the `Properties/sustainability#SUSTAINABILITY_PROPERTY` `Environmental`/`Cost` lowering for the lifecycle disciplines (its `(system, code)` is the sibling's `Classification` value-object EGRESS riding the `MaterialBinding` to the bound element's Object node, NEVER a `MaterialPropertySet` case), and the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` band-2500 rail (the SAME band every seam smart-constructor and `FireRating.Parse` rail, so this SOURCE catalogue never mints the appearance `MaterialFault` 2450 of another concern across its admission); it re-mints NO seam type and reads the seam acoustic folds it never re-authors.

## [01]-[INDEX]

- [01]-[MATERIAL_PROPERTY_CATALOGUE]: the `MaterialPropertyRow` published-data ingress record, the `MaterialPropertyCatalogue` registered-row database (the full structural-materials roster across steel/stainless/concrete/rebar/timber/glulam/aluminium/masonry/stone/glass/insulation/gypsum/membrane), the `Admit` row→seam-case lowering composing the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire` smart-constructors with uncertainty-bearing measurements (the lifecycle `Environmental`/`Cost` cases lower from the sibling `Properties/sustainability`; `Classification` is that sibling's value-object EGRESS, not a case here), and the `Lookup` resolution the projector calls.
- [02]-[ASSESSMENT_INPUT]: why Materials authors NO assessment-input node — the material's `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the input `Rasm.Compute` reads off the graph directly, the migration `MaterialAssessmentInput` marshaller retired and the case→`Discipline` map the seam's own accessor.

## [02]-[MATERIAL_PROPERTY_CATALOGUE]

- Owner: `MaterialPropertyRow` the published-data ingress record; `MaterialPropertyUncertainty` the named relative-uncertainty profile scalar row overloads lift through; `MaterialPropertyCatalogue` the registered-row database; `Admit` the row→seam-case lowering; `Lookup` the projector-facing resolution.
- Cases: one `MaterialPropertyRow` shape across all materials — the mechanical (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`UltimateStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`), thermal (`ConductivityWMK`/`SpecificHeatJKgK`/`UValueWM2K`/`VapourResistanceFactorMu`), optional acoustic (the seventeen-band absorption + sound-reduction vectors), and optional fire (the EN 13501-1 reaction `(Reaction, Smoke, Droplets)` triple + the EN 13501-2 R/E/I minutes) published columns; the `Admit` lowering produces a `Seq<MaterialPropertySet>` of the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire` cases, the lifecycle `Environmental`/`Cost` cases lowering from `Properties/sustainability#SUSTAINABILITY_PROPERTY` (classification is NOT a property case — it is that sibling's `Classification` value-object egress that rides the binding to the bound element's Object node), each case a `MaterialPropertySet` over a `MaterialId`, never a property subtype.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key)` — the published-row lowering passes VividOrange/UnitsNet datums to seam `MeasureValue` with neutral `MeasureBand` bounds before calling `MaterialPropertySet.OfMechanical`/`OfThermal`, folds the seventeen-band acoustic vectors through the seam `Acoustic.Of` band gate (the intrinsic folds ride the seam case), and folds the fire `(Reaction, Smoke, Droplets, minutes)` columns through the seam `FireRating.Parse`, the generated `SmokeClass.TryGet`/`DropletClass.TryGet`, and the three-criterion `FireResistance(loadBearing, integrity, insulation)` ctor (the independent R/E/I minutes — a stone slab's `EI 120` is `(0, 120, 120)`, never the all-equal `Rei` shorthand), `Fin<T>` aborting on a non-finite or out-of-range column (the seam admission's fault lifts unchanged); the scalar-row overloads lift old catalogue rows through `MaterialPropertyUncertainty.Catalogue` unless a row supplies a denser uncertainty-bearing canonical `Datum<T>` shape; `MaterialPropertyCatalogue.Lookup(MaterialId id, Op key)` resolves a registered material to its lowered seam-case set the projector reads, faulting the seam `ElementFault.ValueRejected` for an unregistered material (engineering properties are REQUIRED for a known structural material — unlike the OPTIONAL sustainability data `SustainabilityCatalogue.Lookup` returns empty for) — one polymorphic resolution, never a `GetMechanical`/`GetThermal` family.
- Packages: Rasm.Element (project — the `MaterialPropertySet` smart-constructors `OfMechanical`/`OfThermal`/`OfAcoustic`/`OfFire`, `MeasureValue`, `MeasureBand`, `PropertyEvidence`, `FireRating.Parse`, the generated `SmokeClass.TryGet`/`DropletClass.TryGet`, the `FireResistance` ctor, `Acoustic.Of`, `AcousticBand`, `Discipline`, `MaterialId`, `ElementFault.ValueRejected`), UnitsNet, VividOrange.Uncertainties, VividOrange.Uncertainties.Quantities, Rasm (project — `Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Seq`/`Fin`/`Option` + `Map`/`Match` + the single-element `Seq(value)` ctor), BCL inbox (`FrozenDictionary`/`ReadOnlyMemory<double>`/`ImmutableArray<T>`).
- Growth: a new engineering property shared across materials is one column on the matching seam `MaterialPropertySet` case (a seam growth) the `MaterialPropertyRow` gains a published column for and `Admit` lowers; a new known material is one `MaterialPropertyCatalogue.Rows` entry — a `MaterialId` key and a `MaterialPropertyRow` value (the roster grows by row to thousands without a seam touch); a new property discipline with no fit is one seam `MaterialPropertySet` case carrying its mapping (the lifecycle `Environmental`/`Cost` disciplines land exactly this way on `Properties/sustainability#SUSTAINABILITY_PROPERTY`; classification is NOT a discipline-keyed property — it is that sibling's `Classification` value-object egress, not a case) — never a parallel Materials union, never a per-discipline material type. The catalogue grows by row, the property vocabulary by seam case.
- Boundary: `MaterialPropertyRow` is the published-DATA ingress (the raw EN-standard/datasheet values), NOT a parallel domain union — the seam `MaterialPropertySet` is the one typed carrier, `Admit` the `BOUNDARY_ADMISSION` that lowers the raw row into the seam cases once; the dimensional columns coerce to SI THROUGH the seam smart-constructors (the `UltimateStrengthMpa` column the ACI/EN concrete + EN 1993 net-section checks read and the `VapourResistanceFactorMu` column the EN ISO 13788 Glaser condensation route reads pass RAW to `OfMechanical`/`OfThermal`, which call `MeasureValue.Of(value, UnitsNet.Units.X, key)` internally — the seam's `Of(double value, Enum unit, Op key)` registry coercion, NOT a Materials `QuantityType`-leading overload that does not exist), the `PoissonsRatio` guarded by the seam `OfMechanical` to the isotropic `[0,0.5]` (a thermodynamically-impossible ratio unrepresentable) and the vapour factor guarded `>= 1`, the acoustic vectors validated once through the seam `Acoustic.Of` band-arity gate, the fire reaction parsed to the seam `FireRating` `[SmartEnum<string>]` (a non-standard class is a row never a free string) and the published minutes lifted to a typed EN 13501-2 R/E/I `FireResistance` never a bare scalar; the seam `Acoustic` case carries the `Nrc`/`Saa`/`StcWeighted` intrinsic folds over the shared `RatingContour.Fit` contour kernel — this owner READS them (never re-authors), so a Materials consumer reading a single-number STC reads the seam contour-fit, never a second algorithm; the catalogue NEVER stores a coercion enum or a unit type — the only unit boundary is the seam smart-constructor, so this folder admits `UnitsNet` solely THROUGH the seam ([H2]) and never reaches DOWN to the app-platform `Rasm.Compute` units owner; the lowered `Seq<MaterialPropertySet>` is what the `Projection/component#COMPONENT_PROJECTOR` writes onto the seam `Material` node, and the per-material set feeds the `[03]-[ASSESSMENT_INPUT]` discipline marshalling — the property catalogue crosses to `Rasm.Compute`/`Rasm.Bim` only through the seam graph, never a Materials wire carrier.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Domain;                   // Op
using Rasm.Element;                  // MaterialId, Discipline, MaterialPropertySet (Mechanical|Thermal|Acoustic|Fire|Environmental|Cost),
                                     // FireRating, SmokeClass, DropletClass, FireResistance, Acoustic,
                                     // ElementFault (the seam value-admission band 2500 — this SOURCE catalogue rails the SAME band the seam
                                     // smart-constructors and FireRating.Parse rail, never the appearance MaterialFault 2450 of another concern)
using UnitsNet;
using VividOrange.Uncertainties;
using VividOrange.Uncertainties.Quantities.Utility;
using VividOrange.Uncertainties.Utility;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // the property-catalogue folder owner — MaterialPropertyCatalogue lives here, the Projection/component projector imports it as Rasm.Materials.Properties

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct Datum<TQuantity>(
    IUncertainty<TQuantity> Value,
    PropertyEvidence Evidence) where TQuantity : IQuantity;

public readonly record struct ScalarDatum(
    IUncertainty<double> Value,
    PropertyEvidence Evidence);

public sealed record AcousticDatum(
    ImmutableArray<ScalarDatum> Absorption,
    ImmutableArray<ScalarDatum> Reduction);

public sealed record FireDatum(
    string Reaction,
    string Smoke,
    string Droplets,
    FireResistance Resistance,
    PropertyEvidence Evidence);

public readonly record struct MaterialPropertyUncertainty(
    double Mechanical,
    double Thermal,
    double Acoustic) {
    public static readonly MaterialPropertyUncertainty Catalogue = new(0.05, 0.05, 0.10);
}

// The published engineering data for one material — the ingress row Admit lowers into the seam MaterialPropertySet
// cases. A flat DATA record, not a parallel domain union: the seam owns the union and this owner carries VividOrange
// uncertainty at the Materials boundary before lowering it to neutral MeasureBand values.
public sealed record MaterialPropertyRow(
    Datum<Density> Density,
    Datum<Pressure> Youngs,
    Datum<Pressure> Yield,
    Datum<Pressure> Ultimate,
    ScalarDatum Poisson,
    ScalarDatum Expansion,
    Datum<ThermalConductivity> Conductivity,
    Datum<SpecificEntropy> SpecificHeat,
    Datum<HeatTransferCoefficient> UValue,
    ScalarDatum VapourMu,
    Option<AcousticDatum> Acoustic,
    Option<FireDatum> Fire) {
    public MaterialPropertyRow(
        PropertyEvidence evidence,
        MaterialPropertyUncertainty uncertainty,
        double densityKgM3,
        double youngsModulusMpa,
        double yieldStrengthMpa,
        double ultimateStrengthMpa,
        double poissonsRatio,
        double thermalExpansionPerK,
        double conductivityWMK,
        double specificHeatJKgK,
        double uValueWM2K,
        double vapourResistanceFactorMu,
        Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> acoustic,
        Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> fire)
        : this(
            DatumOf(Density.FromKilogramsPerCubicMeter(densityKgM3), evidence, uncertainty.Mechanical),
            DatumOf(Pressure.FromMegapascals(youngsModulusMpa), evidence, uncertainty.Mechanical),
            DatumOf(Pressure.FromMegapascals(yieldStrengthMpa), evidence, uncertainty.Mechanical),
            DatumOf(Pressure.FromMegapascals(ultimateStrengthMpa), evidence, uncertainty.Mechanical),
            ScalarOf(poissonsRatio, evidence, uncertainty.Mechanical),
            ScalarOf(thermalExpansionPerK, evidence, uncertainty.Mechanical),
            DatumOf(ThermalConductivity.FromWattsPerMeterKelvin(conductivityWMK), evidence, uncertainty.Thermal),
            DatumOf(SpecificEntropy.FromJoulesPerKilogramKelvin(specificHeatJKgK), evidence, uncertainty.Thermal),
            DatumOf(HeatTransferCoefficient.FromWattsPerSquareMeterKelvin(uValueWM2K), evidence, uncertainty.Thermal),
            ScalarOf(vapourResistanceFactorMu, evidence, uncertainty.Thermal),
            acoustic.Map(a => AcousticOf(a.Absorption, a.Sri, evidence, uncertainty.Acoustic)),
            fire.Map(f => new FireDatum(f.Reaction, f.Smoke, f.Droplets, new FireResistance(f.LoadBearingMin, f.IntegrityMin, f.InsulationMin), evidence))) { }

    public MaterialPropertyRow(
        PropertyEvidence evidence,
        double densityKgM3,
        double youngsModulusMpa,
        double yieldStrengthMpa,
        double ultimateStrengthMpa,
        double poissonsRatio,
        double thermalExpansionPerK,
        double conductivityWMK,
        double specificHeatJKgK,
        double uValueWM2K,
        double vapourResistanceFactorMu,
        Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> acoustic,
        Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> fire)
        : this(evidence, MaterialPropertyUncertainty.Catalogue, densityKgM3, youngsModulusMpa, yieldStrengthMpa, ultimateStrengthMpa,
            poissonsRatio, thermalExpansionPerK, conductivityWMK, specificHeatJKgK, uValueWM2K, vapourResistanceFactorMu, acoustic, fire) { }

    public MaterialPropertyRow(
        double densityKgM3,
        double youngsModulusMpa,
        double yieldStrengthMpa,
        double ultimateStrengthMpa,
        double poissonsRatio,
        double thermalExpansionPerK,
        double conductivityWMK,
        double specificHeatJKgK,
        double uValueWM2K,
        double vapourResistanceFactorMu,
        Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> acoustic,
        Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> fire)
        : this(PropertyEvidenceDefaults.Catalogue, MaterialPropertyUncertainty.Catalogue, densityKgM3, youngsModulusMpa, yieldStrengthMpa, ultimateStrengthMpa,
            poissonsRatio, thermalExpansionPerK, conductivityWMK, specificHeatJKgK, uValueWM2K, vapourResistanceFactorMu, acoustic, fire) { }

    static Datum<TQuantity> DatumOf<TQuantity>(TQuantity value, PropertyEvidence evidence, double relativeUncertainty)
        where TQuantity : IQuantity =>
        new(value.WithRelativeUncertainty(relativeUncertainty), PropertyEvidenceDefaults.Normalize(evidence));

    static ScalarDatum ScalarOf(double value, PropertyEvidence evidence, double relativeUncertainty) =>
        new(value.WithRelativeUncertainty(relativeUncertainty), PropertyEvidenceDefaults.Normalize(evidence));

    static AcousticDatum AcousticOf(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> reduction, PropertyEvidence evidence, double relativeUncertainty) =>
        new(Scalars(absorption, evidence, relativeUncertainty), Scalars(reduction, evidence, relativeUncertainty));

    static ImmutableArray<ScalarDatum> Scalars(ReadOnlyMemory<double> values, PropertyEvidence evidence, double relativeUncertainty) {
        ImmutableArray<ScalarDatum>.Builder builder = ImmutableArray.CreateBuilder<ScalarDatum>(values.Length);
        foreach (double value in values.Span) { builder.Add(ScalarOf(value, evidence, relativeUncertainty)); }
        return builder.MoveToImmutable();
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialPropertyCatalogue {
    // Lowers a published row into the seam MaterialPropertySet cases. Materials keeps UnitsNet quantities and VividOrange
    // uncertainty models at this boundary, then lowers them to seam MeasureValue/MeasureBand/PropertyEvidence before the
    // Mechanical/Thermal cases cross the seam. The acoustic vectors flow through the seam Acoustic.Of band gate (the intrinsic
    // Nrc/Saa/StcWeighted folds ride the seam Acoustic case); the fire triple parses to the seam FireRating/SmokeClass/
    // DropletClass and the published minutes lift to a typed EN 13501-2 R/E/I FireResistance. EVERY value flows through a
    // seam admission (the OfMechanical/OfThermal/OfAcoustic constructors, FireRating.Parse, the Sub TryGet gate) so the
    // WHOLE chain rails ONE band — the seam ElementFault.ValueRejected (2500) — never the appearance MaterialFault 2450 of
    // another concern: a mixed-band Admit chain forks the telemetry the fault federation bands by code (an out-of-domain
    // smoke token reading "Parameter"/appearance rather than "Value"/seam-admission is the rejected cross-concern leak).
    public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key) =>
        from mechanical in MaterialPropertySet.OfMechanical(
            MeasureOf(row.Density, static q => q.KilogramsPerCubicMeter, QuantityType.Create("Density"), Dimension.DensityDim, "kg/m3"),
            MeasureOf(row.Youngs, static q => q.Pascals, QuantityType.Create("Pressure"), Dimension.PressureDim, "Pa"),
            MeasureOf(row.Yield, static q => q.Pascals, QuantityType.Create("Pressure"), Dimension.PressureDim, "Pa"),
            MeasureOf(row.Ultimate, static q => q.Pascals, QuantityType.Create("Pressure"), Dimension.PressureDim, "Pa"),
            Central(row.Poisson),
            Central(row.Expansion),
            key,
            EvidenceOf(row.Density))
        from thermal in MaterialPropertySet.OfThermal(
            MeasureOf(row.Conductivity, static q => q.WattsPerMeterKelvin, QuantityType.Create("ThermalConductivity"), Dimension.Create(1, 1, -3, 0, -1, 0, 0), "W/(m.K)"),
            MeasureOf(row.SpecificHeat, static q => q.JoulesPerKilogramKelvin, QuantityType.Create("SpecificEntropy"), Dimension.Create(2, 0, -2, 0, -1, 0, 0), "J/(kg.K)"),
            MeasureOf(row.UValue, static q => q.WattsPerSquareMeterKelvin, QuantityType.Create("HeatTransferCoefficient"), Dimension.ThermalTransmittance, "W/(m2.K)"),
            Central(row.VapourMu),
            key,
            EvidenceOf(row.Conductivity))
        from acoustic in row.Acoustic.Match(
            None: () => Fin.Succ(Seq<MaterialPropertySet>()),
            Some: a => Acoustic.Of(CentralValues(a.Absorption), CentralValues(a.Reduction), key)
                .Map(spectrum => Seq(MaterialPropertySet.OfAcoustic(spectrum, EvidenceOf(a.Absorption)))))
        from fire in row.Fire.Match(
            None: () => Fin.Succ(Seq<MaterialPropertySet>()),
            Some: f => from reaction in FireRating.Parse(f.Reaction, key)          // the seam's Parse over the generated TryGet — the ONE fire-reaction admission
                       from smoke in Sub(SmokeClass.TryGet, f.Smoke, key, "smoke") // SmokeClass/DropletClass expose NO Parse; resolve through the generated TryGet
                       from droplets in Sub(DropletClass.TryGet, f.Droplets, key, "droplet")
                       select Seq(MaterialPropertySet.OfFire(reaction, smoke, droplets, f.Resistance, f.Evidence)))
        select Seq(mechanical, thermal) + acoustic + fire;

    static MeasureBand BandOf<TQuantity>(IUncertainty<TQuantity> value, Func<TQuantity, double> si)
        where TQuantity : IQuantity =>
        value is INormalDistributionUncertainty<TQuantity> normal
            ? MeasureBand.Normal(si(value.LowerBound), si(value.UpperBound), si(normal.StandardDeviation), normal.CoverageFactor)
            : MeasureBand.Interval(KindOf(value), si(value.LowerBound), si(value.UpperBound));

    static MeasureValue MeasureOf<TQuantity>(
        Datum<TQuantity> datum,
        Func<TQuantity, double> si,
        QuantityType type,
        Dimension dimension,
        string unit) where TQuantity : IQuantity =>
        new(type, dimension, si(datum.Value.CentralValue), unit, Some(BandOf(datum.Value, si)));

    static UncertaintyKind KindOf<T>(IUncertainty<T> value) =>
        value switch {
            IAbsoluteUncertainty<T> _ => UncertaintyKind.Absolute,
            IRelativeUncertainty<T> _ => UncertaintyKind.Relative,
            IIntervalUncertainty<T> _ => UncertaintyKind.Interval,
            INormalDistributionUncertainty<T> _ => UncertaintyKind.Normal,
            _ => UncertaintyKind.Exact
        };

    static double Central(ScalarDatum datum) => datum.Value.CentralValue;

    static PropertyEvidence EvidenceOf<TQuantity>(Datum<TQuantity> datum) where TQuantity : IQuantity =>
        PropertyEvidenceDefaults.Normalize(datum.Evidence);

    static PropertyEvidence EvidenceOf(ImmutableArray<ScalarDatum> values) =>
        values.IsDefaultOrEmpty ? PropertyEvidenceDefaults.Catalogue : PropertyEvidenceDefaults.Normalize(values[0].Evidence);

    static ReadOnlyMemory<double> CentralValues(ImmutableArray<ScalarDatum> values) {
        double[] result = new double[values.Length];
        for (int i = 0; i < values.Length; i++) { result[i] = Central(values[i]); }
        return result.AsMemory();
    }

    // The EN 13501-1 sub-class admission: SmokeClass/DropletClass are seam [SmartEnum<string>] with NO Parse wrapper
    // (only FireRating/Discipline/MeasurementBasis carry one), so an empty token defaults the seam's NotSpecified row and a
    // present token resolves through the Thinktecture-generated TryGet — railing the seam ElementFault.ValueRejected on an
    // out-of-domain class, the SAME band (and "Value" telemetry Category) FireRating.Parse rails for the sibling reaction
    // token, so the whole fire admission carries one band. The TryGetter delegate matches the generated smart-enum signature
    // `static bool TryGet([AllowNull] string, [MaybeNullWhen(false)] out T)`, so the one helper serves both sub-classes.
    delegate bool TryGetter<T>(string? token, out T? value);
    static Fin<T> Sub<T>(TryGetter<T> tryGet, string token, Op key, string label) where T : class =>
        tryGet(token, out T? value) && value is { } row
            ? Fin.Succ(row)
            : ElementFault.ValueRejected(key, $"<fire-{label}-class-unknown:{token}>");

    // The structural-materials roster — every row a published EN-standard datasheet keyed by the canonical MaterialId
    // (seam-generated ordinal-ignore-case equality keys the table). The roster grows by ROW to thousands of grades; the
    // seam vocabulary never changes. Mechanical columns are the published CHARACTERISTIC values (the EN 1993-1-1 Table 3.1
    // nominal fy/fu for carbon steel, the EN 1993-1-4 Table 2.1 0.2%-proof/tensile for stainless, EN 1992-1-1 Table 3.1
    // fck as the yield surrogate and fcm = fck+8 as ultimate for concrete, EN 338/EN 14080 fm,k as the bending-strength
    // surrogate for timber, EN 1999-1-1 f0/fu for aluminium); a YieldStrengthMpa naming the published characteristic
    // strength of the grade, never an invented number. Thermal: EN ISO 10456 design lambda + the EN ISO 13788
    // vapour-resistance factor mu. Acoustic: the seventeen-band absorption + field-incidence SRI vectors a datasheet
    // declares (only the hard-surfaced / acoustically-characterized rows carry them; a structural steel section carries
    // None). Fire: EN 13501-1 reaction (A1 noncombustible steel/concrete/stone/mineral-wool through F) + the EN 13501-2
    // R/E/I minutes where a bare material is classified (most carry 0 — resistance is an assembly property a Rasm.Compute
    // route computes over the buildup). U-value columns are a single-layer reference (lambda / a nominal thickness) the
    // EN ISO 6946 assembly fold in Rasm.Compute supersedes; the seam carries the per-material datum, the assembly the fold.
    public static readonly FrozenDictionary<MaterialId, MaterialPropertyRow> Rows = new (MaterialId Id, MaterialPropertyRow Row)[] {
        // --- structural carbon steel (EN 10025-2; EN 1993-1-1 Table 3.1 nominal fy/fu at t <= 16 mm; E 210 GPa, nu 0.30,
        //     alpha 12e-6; lambda EN ISO 10456 ~50, c ~460; A1 noncombustible)
        (MaterialId.Of("steel.s235"), new(7850.0, 210_000.0, 235.0, 360.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.s275"), new(7850.0, 210_000.0, 275.0, 430.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.s355"), new(7850.0, 210_000.0, 355.0, 490.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // EN 10025-3/-4 fine-grain S420N/M (Table 3.1 fy 420, fu lowest-of-range 520) and EN 10025-6 quenched-and-tempered S690QL
        (MaterialId.Of("steel.s420"), new(7850.0, 210_000.0, 420.0, 520.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.s460"), new(7850.0, 210_000.0, 460.0, 540.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.s690"), new(7850.0, 210_000.0, 690.0, 770.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // metal.steel — the generic-structural-steel grade alias the Component/component#COMPONENT_OWNER Component.SubstanceId
        // resolves (a galvanized fastener/connector whose grade is unspecified): the conservative EN 1993-1-1 S235 baseline (fy 235, fu 360),
        // so the connection-design seam reads a real Mechanical row by metal.steel rather than faulting; a graded connector keys steel.s355 directly.
        (MaterialId.Of("metal.steel"), new(7850.0, 210_000.0, 235.0, 360.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // metal.iron — the cast/wrought-iron generic the Component/joint weld family keys (the electrode/casting AppearanceId);
        // ductile-iron E ~170 GPa, fy ~250 / fu ~400 (EN-GJS-400-15 grade family), the conservative casting baseline the
        // Mechanical row carries; keyed in BOTH catalogues (its Furnes ductile-cast-iron EPD on the sustainability side).
        (MaterialId.Of("metal.iron"), new(7200.0, 170_000.0, 250.0, 400.0, 0.28, 11.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- reinforcing steel (EN 10080 / EN 1992-1-1 §3.2; B500A/B/C share fyk 500, fu rising with ductility class
        //     A k>=1.05 / B k>=1.08 / C 1.15<=k<1.35; ASTM A615/A706 + CSA G30.18 graded fy/fu_min per the spec tables;
        //     E_s 200 GPa ALL grades the ACI 318 §20.2.2.2 / EN 1992-1-1 §3.2.7 reinforcing modulus (NOT the 210 GPa
        //     structural-section value); rho 7850, nu 0.30, alpha 12e-6; A1 — the RebarGrade.SubstanceId rows the bar reads)
        (MaterialId.Of("steel.b500a"),  new(7850.0, 200_000.0, 500.0, 525.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.b500b"),  new(7850.0, 200_000.0, 500.0, 540.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.b500c"),  new(7850.0, 200_000.0, 500.0, 575.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // ASTM A615 carbon-steel (Table 2 fy/fu_min: Gr40 280/420, Gr60 420/620, Gr75 520/690, Gr80 550/725) — the reinforcement#REINFORCEMENT_FAMILY RebarGrade.SubstanceId rows
        (MaterialId.Of("steel.gr40"),   new(7850.0, 200_000.0, 280.0, 420.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.gr60"),   new(7850.0, 200_000.0, 420.0, 620.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.gr75"),   new(7850.0, 200_000.0, 520.0, 690.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.gr80"),   new(7850.0, 200_000.0, 550.0, 725.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // ASTM A706 low-alloy weldable (Gr60 420/550 with fu/fy>=1.25, Gr80 550/690) — the weldable-grade SubstanceId rows
        (MaterialId.Of("steel.gr60w"),  new(7850.0, 200_000.0, 420.0, 550.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.gr80w"),  new(7850.0, 200_000.0, 550.0, 690.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // CSA G30.18 weldable (400W fy 400 / fu_min 540, 500W fy 500 / fu_min 620) — the Canadian-metric SubstanceId rows
        (MaterialId.Of("steel.400w"),   new(7850.0, 200_000.0, 400.0, 540.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.500w"),   new(7850.0, 200_000.0, 500.0, 620.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- stainless steel (EN 10088; EN 1993-1-4 Table 2.1 hot-rolled-plate 0.2%-proof fy / tensile fu; E 200 GPa all
        //     grades; austenitic lambda ~15 / c ~500 / alpha ~16e-6, duplex lambda ~15 / alpha ~13e-6; A1)
        (MaterialId.Of("steel.1.4301"),   new(8000.0, 200_000.0, 210.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.1.4307"),   new(8000.0, 200_000.0, 200.0, 500.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.1.4401"),   new(8000.0, 200_000.0, 220.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.1.4404"),   new(8000.0, 200_000.0, 220.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.1.4571"),   new(8000.0, 200_000.0, 220.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.1.4462"),   new(7800.0, 200_000.0, 460.0, 640.0, 0.30, 13.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- concrete (EN 206; EN 1992-1-1 Table 3.1 dual-class C{fck}/{fck,cube}: fck the yield surrogate, fcm = fck+8 the
        //     ultimate; Ecm GPa via 22*(fcm/10)^0.3; rho 2400 (2500 reinforced), nu 0.20, alpha 10e-6, lambda ~1.6-2.3, c 1000;
        //     A1). The id is the EN dual-class token (C12/15 -> concrete.c12_15) the Properties/sustainability EPD roster keys
        //     on the SAME MaterialId, so a registered strength class resolves a mechanical/thermal/fire row AND a lifecycle row.
        (MaterialId.Of("concrete.c12_15"), new(2400.0, 27_000.0, 12.0, 20.0, 0.20, 10.0e-6, 1.65, 1000.0, 8.25, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c16_20"), new(2400.0, 29_000.0, 16.0, 24.0, 0.20, 10.0e-6, 1.80, 1000.0, 9.00, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c20_25"), new(2400.0, 30_000.0, 20.0, 28.0, 0.20, 10.0e-6, 2.00, 1000.0, 10.0, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c25_30"), new(2400.0, 31_000.0, 25.0, 33.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c30_37"), new(2400.0, 33_000.0, 30.0, 38.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c35_45"), new(2450.0, 34_000.0, 35.0, 43.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 80.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c40_50"), new(2450.0, 35_000.0, 40.0, 48.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 80.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c45_55"), new(2450.0, 36_000.0, 45.0, 53.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 80.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c50_60"), new(2450.0, 37_000.0, 50.0, 58.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 90.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c55_67"), new(2500.0, 38_000.0, 55.0, 63.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 100.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c60_75"), new(2500.0, 39_000.0, 60.0, 68.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 100.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c70_85"), new(2500.0, 41_000.0, 70.0, 78.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 120.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c80_95"), new(2500.0, 42_000.0, 80.0, 88.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 120.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c90_105"), new(2500.0, 44_000.0, 90.0, 98.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 130.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // EN 1992-1-1 §11 lightweight aggregate concrete (LC, generic class) — reduced density 1800, eta_E lambda-knockdown
        // E ~18 GPa, lambda ~0.8; concrete.lc the generic id the sustainability roster's concrete.lc lifecycle row matches
        (MaterialId.Of("concrete.lc"), new(1800.0, 18_000.0, 30.0, 38.0, 0.20, 8.0e-6, 0.80, 1000.0, 4.00, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- structural softwood timber (EN 338:2016 Table 1: fm,k the bending-strength surrogate, E0,mean GPa; nu ~0.4,
        //     alpha ~5e-6, lambda EN ISO 10456 ~0.13, c ~1600, mu ~50; D reaction with EN 13501-1 s2,d0)
        (MaterialId.Of("timber.c14"),    new(290.0, 7_000.0, 14.0, 23.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c16"),    new(310.0, 8_000.0, 16.0, 26.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c18"),    new(320.0, 9_000.0, 18.0, 30.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c20"),    new(330.0, 9_500.0, 20.0, 33.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c22"),    new(340.0, 10_000.0, 22.0, 37.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c24"),    new(350.0, 11_000.0, 24.0, 40.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Some((Absorb(0.10, 0.11, 0.10, 0.08, 0.06, 0.06, 0.07, 0.07, 0.08, 0.08, 0.09, 0.09, 0.09, 0.10, 0.10, 0.10, 0.11),
                  Sri(14, 16, 18, 20, 22, 24, 26, 27, 29, 31, 33, 34, 36, 38, 40, 41, 42)),
                 Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.c27"),    new(370.0, 11_500.0, 27.0, 45.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c30"),    new(380.0, 12_000.0, 30.0, 50.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c35"),    new(400.0, 13_000.0, 35.0, 58.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c40"),    new(420.0, 14_000.0, 40.0, 66.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c45"),    new(440.0, 15_000.0, 45.0, 75.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.c50"),    new(460.0, 16_000.0, 50.0, 83.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        // --- structural hardwood timber (EN 338:2016 Table 3 — the FULL fourteen-class D-series D18..D80: D-class fm,k /
        //     E0,mean GPa, rho_k; denser, lambda ~0.17, c ~2400)
        (MaterialId.Of("timber.d18"),    new(475.0, 9_500.0, 18.0, 30.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d24"),    new(485.0, 10_000.0, 24.0, 40.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d27"),    new(510.0, 10_500.0, 27.0, 45.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d30"),    new(530.0, 11_000.0, 30.0, 50.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d35"),    new(540.0, 12_000.0, 35.0, 58.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d40"),    new(550.0, 13_000.0, 40.0, 66.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d45"),    new(580.0, 13_500.0, 45.0, 75.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d50"),    new(620.0, 14_000.0, 50.0, 83.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d55"),    new(660.0, 15_500.0, 55.0, 92.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d60"),    new(700.0, 17_000.0, 60.0, 100.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d65"),    new(750.0, 18_500.0, 65.0, 109.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d70"),    new(900.0, 20_000.0, 70.0, 117.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d75"),    new(850.0, 22_000.0, 75.0, 125.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("timber.d80"),    new(900.0, 24_000.0, 80.0, 134.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        // wood.oak — the named-hardwood generic alias (a D30-class European white oak the Component/timber family keys when a
        // species rather than a strength class is supplied), absorptive interior-finish acoustic vector carried
        (MaterialId.Of("wood.oak"),      new(700.0, 11_000.0, 40.0, 90.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Some((Absorb(0.05, 0.06, 0.07, 0.08, 0.10, 0.10, 0.11, 0.10, 0.10, 0.10, 0.10, 0.10, 0.09, 0.09, 0.09, 0.09, 0.09),
                  Sri(18, 20, 22, 24, 26, 29, 31, 33, 35, 37, 38, 39, 40, 40, 39, 35, 33)),
                 Some(("D", "s2", "d0", 0, 0, 0)))),
        // --- glued-laminated timber (EN 14080:2013 Table 5 homogeneous h-class GL20h..GL32h + Table 4 combined c-class
        //     GL20c..GL32c, the FULL seven-per-layup set: fm,g,k the bending-strength surrogate, E0,g,mean GPa, rho,g,k;
        //     D reaction, charring an assembly route in Rasm.Compute)
        (MaterialId.Of("timber.gl20h"),  new(340.0, 8_400.0, 20.0, 33.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl22h"),  new(370.0, 10_500.0, 22.0, 37.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl24h"),  new(385.0, 11_500.0, 24.0, 40.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl26h"),  new(405.0, 12_100.0, 26.0, 43.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl28h"),  new(425.0, 12_600.0, 28.0, 47.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl30h"),  new(440.0, 13_600.0, 30.0, 50.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl32h"),  new(440.0, 14_200.0, 32.0, 53.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl20c"),  new(355.0, 10_400.0, 20.0, 33.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl22c"),  new(355.0, 10_400.0, 22.0, 37.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl24c"),  new(365.0, 11_000.0, 24.0, 40.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl26c"),  new(385.0, 12_000.0, 26.0, 43.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl28c"),  new(390.0, 12_500.0, 28.0, 47.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl30c"),  new(390.0, 13_000.0, 30.0, 50.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl32c"),  new(400.0, 13_500.0, 32.0, 53.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        // --- aluminium (EN 1999-1-1 Table 3.2 wrought alloys; f0 0.2%-proof the yield surrogate / fu ultimate; E 70 GPa,
        //     nu 0.33, alpha 23e-6, c ~900; lambda per temper; B reaction s1,d0). EN 1999-1-1 names 6082 FIRST as the most
        //     common European structural extrusion alloy, then 6061 and 7020; the rows carry the EN 755-2:2016 extruded f0/fu.
        (MaterialId.Of("aluminium.6082t6"), new(2710.0, 70_000.0, 260.0, 310.0, 0.33, 23.1e-6, 170.0, 897.0, 18.9, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("B", "s1", "d0", 0, 0, 0)))),
        (MaterialId.Of("aluminium.6061t6"), new(2700.0, 70_000.0, 240.0, 290.0, 0.33, 23.0e-6, 167.0, 900.0, 18.8, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("B", "s1", "d0", 0, 0, 0)))),
        (MaterialId.Of("aluminium.6063t5"), new(2700.0, 70_000.0, 130.0, 175.0, 0.33, 23.0e-6, 200.0, 900.0, 18.8, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("B", "s1", "d0", 0, 0, 0)))),
        (MaterialId.Of("aluminium.5083"),   new(2660.0, 70_000.0, 125.0, 275.0, 0.33, 24.0e-6, 117.0, 900.0, 18.8, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("B", "s1", "d0", 0, 0, 0)))),
        // --- masonry units (EN 771; fb the normalized compressive strength the yield surrogate; lambda EN ISO 10456;
        //     clay/calcium-silicate/AAC/aggregate; A1 except organic-bound)
        (MaterialId.Of("masonry.clay"),  new(1800.0, 7_000.0, 10.0, 20.0, 0.25, 6.0e-6, 0.77, 1000.0, 3.50, 16.0,
            Some((Absorb(0.02, 0.02, 0.03, 0.03, 0.03, 0.04, 0.04, 0.05, 0.05, 0.05, 0.05, 0.06, 0.06, 0.06, 0.07, 0.07, 0.07),
                  Sri(30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 53, 54, 55, 56, 57)),
                 Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("masonry.calciumsilicate"), new(1800.0, 8_000.0, 12.0, 24.0, 0.25, 8.0e-6, 1.00, 1000.0, 4.55, 15.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("masonry.aac"),   new(500.0, 2_000.0, 4.0, 5.0, 0.20, 8.0e-6, 0.13, 1000.0, 0.43, 6.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("masonry.aggregate"), new(1400.0, 9_000.0, 7.0, 14.0, 0.20, 8.0e-6, 0.51, 1000.0, 2.55, 6.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- dimension stone (EN 771-6 natural stone; fk the characteristic compressive strength surrogate; A1 noncombustible,
        //     EI 120/120 a typical separating-wall slab rating). marble: E 70 GPa, lambda 2.8; granite: denser/stiffer
        //     E 60 GPa with higher compressive strength, lambda 3.0 (the sustainability roster keys both on the SAME id)
        (MaterialId.Of("stone.marble"),  new(2700.0, 70_000.0, 15.0, 100.0, 0.25, 7.0e-6, 2.8, 880.0, 3.50, 10_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 120, 120)))),
        (MaterialId.Of("stone.granite"), new(2650.0, 60_000.0, 20.0, 130.0, 0.23, 8.0e-6, 3.0, 790.0, 3.40, 10_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 120, 120)))),
        // --- glazing (EN 572 soda-lime float glass; fk the characteristic bending strength; E 70 GPa, nu 0.23, alpha 9e-6,
        //     lambda 1.0, c 720; A1; spectral acoustic vector carried)
        (MaterialId.Of("glass.float"),   new(2500.0, 70_000.0, 45.0, 50.0, 0.22, 9.0e-6, 1.00, 720.0, 5.88, 1e9,
            Some((Absorb(0.18, 0.10, 0.07, 0.05, 0.04, 0.03, 0.03, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02),
                  Sri(25, 27, 29, 30, 31, 32, 33, 34, 33, 32, 30, 29, 31, 34, 37, 39, 40)),
                 Some(("A1", "", "", 0, 0, 0)))),
        // --- insulation (EN 13162 glass/stone mineral wool / EN 13163 EPS / EN 13164 XPS / EN 13165 PUR-PIR / EN 13166
        //     phenolic / EN ISO 10456 design lambda + EN ISO 13788 mu; mineral wool A1, foams E reaction; absorptive
        //     mineral/wood-fibre). insulation.glasswool/stonewool key the SAME ids the sustainability EPD roster carries
        //     (EN 13162 splits glass wool from stone wool); insulation.phenolic is the EN 13166 best-foam-lambda grade
        //     (lambda 0.020), keyed in BOTH catalogues (its Kingspan-Kooltherm EN 15804+A2 EPD on the sustainability side).
        (MaterialId.Of("insulation.glasswool"), new(40.0, 1.0, 0.001, 0.002, 0.0, 0.0, 0.035, 1030.0, 0.13, 1.0,
            Some((Absorb(0.15, 0.25, 0.40, 0.55, 0.70, 0.80, 0.90, 0.95, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                  Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15)),
                 Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("insulation.stonewool"), new(45.0, 1.0, 0.001, 0.002, 0.0, 0.0, 0.035, 1030.0, 0.13, 1.0,
            Some((Absorb(0.16, 0.26, 0.42, 0.58, 0.72, 0.82, 0.92, 0.97, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                  Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15)),
                 Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("insulation.eps"),  new(20.0, 5.0, 0.05, 0.10, 0.10, 60.0e-6, 0.036, 1450.0, 0.13, 60.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.xps"),  new(33.0, 15.0, 0.20, 0.45, 0.10, 70.0e-6, 0.034, 1450.0, 0.12, 150.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.pir"),  new(32.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.022, 1400.0, 0.08, 60.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.pur"),  new(35.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.025, 1400.0, 0.08, 60.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.phenolic"), new(35.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.020, 1400.0, 0.07, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("C", "s1", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.woodfibre"), new(160.0, 50.0, 0.10, 0.20, 0.10, 8.0e-6, 0.038, 2100.0, 0.13, 5.0,
            Some((Absorb(0.12, 0.20, 0.35, 0.50, 0.65, 0.75, 0.85, 0.90, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95),
                  Sri(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 13, 14, 14, 15, 15, 16)),
                 Some(("E", "s2", "d0", 0, 0, 0)))),
        // --- gypsum board (EN 520; A2; absorptive interior lining)
        (MaterialId.Of("gypsum.board"),  new(700.0, 2_500.0, 2.0, 4.0, 0.25, 18.0e-6, 0.25, 1000.0, 20.0, 10.0,
            Some((Absorb(0.29, 0.20, 0.12, 0.10, 0.08, 0.06, 0.06, 0.05, 0.04, 0.04, 0.04, 0.04, 0.05, 0.05, 0.06, 0.06, 0.07),
                  Sri(15, 16, 17, 18, 20, 22, 24, 26, 28, 30, 31, 32, 33, 34, 35, 36, 37)),
                 Some(("A2", "s1", "d0", 0, 0, 0)))),
        // --- sheet-goods board SUBSTANCES (the Component/panel#PANEL_FAMILY PanelKind.SubstanceId keys — the buildable BOARD
        //     is the Component, the board SUBSTANCE resolves HERE). fibre-cement backer (ASTM C1325; fibre-mat-reinforced
        //     portland-cement-aggregate: rho ~1400, Em,0,mean ~7 GPa the EN cement-bonded-board plate modulus, fm the ASTM
        //     C947 flexural surrogate, lambda EN ISO 10456 ~0.19, c ~900, mu ~15 >10-perm-permeable; A2-s1,d0 Class-A
        //     noncombustible) — a fibre-cement board's Mechanical/Thermal row the panel analysis seam reads by cement.board.
        (MaterialId.Of("cement.board"),  new(1400.0, 7_000.0, 7.0, 10.0, 0.20, 8.0e-6, 0.19, 900.0, 15.0, 15.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A2", "s1", "d0", 0, 0, 0)))),
        // --- wood structural panels (EN 13986 / EN 636 plywood + EN 300 OSB/3 — the isotropic-surrogate substance physics
        //     the panel seam reads by wood.plywood / wood.osb; the panel's span-rating/bond-class ORIENTATION lives on the
        //     Component/panel#PANEL_FAMILY PanelSection, this row the substance datum). plywood: rho ~600, Em,0,mean ~8 GPa
        //     (EN 310 major-axis MOE), fm,k ~30 the bending surrogate; OSB/3: rho ~650, Em ~3.5 GPa (EN 310 major-axis MOE),
        //     fm,k ~20 (EN 300 major-axis MOR surrogate); both lambda EN 13986 0.13, c ~1600/1700, D-s2,d0, mu 90/200.
        (MaterialId.Of("wood.plywood"),  new(600.0, 8_000.0, 30.0, 40.0, 0.30, 5.0e-6, 0.13, 1600.0, 10.8, 90.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("wood.osb"),      new(650.0, 3_500.0, 20.0, 26.0, 0.30, 5.0e-6, 0.13, 1700.0, 11.8, 200.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 0, 0, 0)))),
        // --- roofing/waterproofing membrane (EPDM EN 13956 / PVC EN 13956 / TPO EN 13956; per-area products; E/B reaction;
        //     high vapour resistance — the membrane IS the vapour control layer)
        (MaterialId.Of("membrane.epdm"), new(1150.0, 5.0, 5.0, 9.0, 0.45, 160.0e-6, 0.25, 1000.0, 12.5, 50_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("membrane.pvc"),  new(1300.0, 15.0, 10.0, 15.0, 0.40, 70.0e-6, 0.16, 1000.0, 8.00, 20_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("membrane.tpo"),  new(920.0, 10.0, 9.0, 14.0, 0.40, 150.0e-6, 0.20, 1000.0, 10.0, 30_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    // The projector-facing resolution: a registered material lowers to its full engineering set; an UNREGISTERED material
    // rails the seam ElementFault.ValueRejected (band 2500) — the SAME band Admit's columns rail, so a Lookup and the
    // admission it delegates to never split bands. Engineering properties are REQUIRED for a known structural material the
    // Component/capacity#SECTION_CAPACITY / Rasm.Compute design-code routes read, the asymmetric dual of the OPTIONAL Properties/
    // sustainability#SUSTAINABILITY_PROPERTY Lookup (lifecycle data is declared-or-absent, so it returns Fin.Succ(empty)).
    // An app authoring a material with bespoke properties supplies them at the wire and does not route this catalogue.
    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialPropertyRow? row)
            ? Admit(row!, key)
            : ElementFault.ValueRejected(key, $"<unregistered-material-properties:{id.Value}>");

    // The seventeen-band literal-vector helpers — the AcousticBand resolution (100..4000 Hz) the seam Acoustic.Of gates;
    // a row that omits the acoustic Option carries None. params ReadOnlySpan<double> collapses the seventeen positional
    // bands to one boundary; ToArray().AsMemory() materializes the ReadOnlyMemory<double> the row + seam carry.
    static ReadOnlyMemory<double> Absorb(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
    static ReadOnlyMemory<double> Sri(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
}
// The case→Discipline map is the seam's own MaterialPropertySet.Discipline accessor (one owner); a consumer reads
// set.Discipline directly — Rasm.Compute selects its analysis route by it, this folder mints no parallel map.
```

## [03]-[ASSESSMENT_INPUT]

- Owner: NONE — the Materials folder authors NO assessment-input marshaller and NO `Assessment` node; the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected seam `Material` node IS the analysis input.
- Cases: zero — there is no input shape to model; `Rasm.Compute` reads the typed `MaterialPropertySet` cases off the graph and dispatches on `set.Discipline`, so a per-discipline input bag is the deleted form.
- Entry: `Rasm.Compute` reads the `Material` node plies DIRECTLY above the seam (`id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the relocated multi-ply `AssemblyAggregator` + the ISO/EN closed-form routes + the VividOrange/FE structural solvers), and writes the seam `Assessment` `Result` node back content-keyed on `(input key, route)`; the case→`Discipline` map is the seam's own `MaterialPropertySet.Discipline` accessor (`Mechanical`/`Orthotropic`→`Structural`, `Thermal`→`Thermal`, `Acoustic`→`Acoustic`, `Fire`→`Fire`, `Environmental`→`Environmental`, `Cost`→`Cost`; `Energy` rides an `Assessment` node only), so Compute selects its route by `set.Discipline` with no parallel Materials marshaller.
- Boundary: the migration `MaterialAssessmentInput` marshaller is RETIRED — a Materials-authored typed-input bag is redundant with Compute reading the typed `MaterialPropertySet` cases off the graph, so the seam carries ONE property surface (the `Material` node), not a parallel input node; the seam `Acoustic` case's intrinsic `Nrc`/`StcWeighted`/`SoundReductionIndexDb` folds (`Composition/acoustic`) are the single-material ratings Compute's ISO 12354 layered fold reads through the SAME `RatingContour.Fit` contour kernel, so the assembly STC and the material STC share one contour owner; the multi-ply aggregation is `Rasm.Compute`'s, this folder retaining only the single-material property SOURCE and crossing to Compute solely through the seam graph.

## [04]-[RESEARCH]

- [SEAM_OWNS_PROPERTY_UNION]: the typed engineering-property family is the seam `MaterialPropertySet` `[Union]` (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`, keyed to the seam `Discipline`), so the migration Materials `MaterialProperty` `[Union]`, its `MaterialPropertyKind` coercion enum, and the inline contour-fit/`Nrc`/`Saa` projection folds are RETIRED and ROUTED into the seam (the contour fit now the seam's shared `RatingContour.Fit` kernel). This owner keeps only the Materials SOURCE: the `MaterialPropertyCatalogue` published-row database and the `Admit` lowering. SEAM CONTRACT (Rasm.Element side; this folder consumes): the `MaterialPropertySet` smart-constructors `OfMechanical(MeasureValue density, MeasureValue youngs, MeasureValue yield, MeasureValue ultimate, poisson, expansion, key, evidence)` / `OfThermal(MeasureValue conductivity, MeasureValue specificHeat, MeasureValue uValue, vapour, key, evidence)` / `OfAcoustic(Acoustic, evidence)` / `OfFire(FireRating, SmokeClass, DropletClass, FireResistance, evidence)` (the cases carry `MeasureValue` columns; the `Acoustic` case forwards `AbsorptionSpectrum`/`SoundReductionIndexDb` over `AcousticBand` + the intrinsic `Nrc`/`Saa`/`StcWeighted`/`Rw` folds over the shared `RatingContour.Fit` contour kernel), `Acoustic.Of(absorption, sri, key)`, `FireRating.Parse` (the seam's `Parse` over the generated `TryGet`), the generated `SmokeClass.TryGet`/`DropletClass.TryGet` (the seam gives these sub-classes NO `Parse` wrapper, so this folder resolves them through the Thinktecture-generated `TryGet` whose signature is `static bool TryGet([AllowNull] string, [MaybeNullWhen(false)] out T)`), the `FireResistance(loadBearing, integrity, insulation)` ctor, and `MaterialPropertySet.Discipline`. The provider uncertainty models stay here; only neutral `MeasureBand` bounds and `PropertyEvidence` cross the seam. The timber rows lower the isotropic `Mechanical` case here (the substance SOURCE carrying `fm,k` as the yield surrogate); the directional `Orthotropic` lowering (the EN-grade `E0,mean`/`E90,mean`/`G,mean`/`fc0k`/`fc90k` data) is the `Component/timber#TIMBER_FAMILY` concern, not the substance catalogue's.
- [CATALOGUE_DOMAIN_COVERAGE]: the `MaterialPropertyCatalogue` is the SOURCE of known-material engineering data — the FULL structural-materials domain, never a sample slice. The roster is the authoritative EN/ISO grade set: EN 10025-2 carbon steel (`S235`/`S275`/`S355`/`S420`/`S460`, `S690` to EN 10025-6, the `metal.steel` S235 generic the `Component/component#COMPONENT_OWNER` `Component.SubstanceId` resolves on an unspecified grade, and the `metal.iron` ductile-casting generic), reinforcing steel (EN 10080 `B500A`/`B500B`/`B500C` at `fyk` 500 with `fu` rising by ductility class, ASTM A615 `gr40`/`gr60`/`gr75`/`gr80`, ASTM A706 weldable `gr60w`/`gr80w`, CSA G30.18 `400w`/`500w` — each its spec `fy`/`fu_min` and the ACI 318 §20.2.2.2 / EN 1992-1-1 §3.2.7 reinforcing modulus `E_s` 200 GPa, the `Component/reinforcement#REINFORCEMENT_FAMILY` `RebarGrade.SubstanceId`-keyed rows so a projected bar's seam `Mechanical` reads the bar's REAL grade strength, never the `metal.steel` section baseline), EN 10088 stainless (`1.4301`/`1.4307`/`1.4401`/`1.4404`/`1.4571` austenitic + `1.4462` duplex, the EN 1993-1-4 Table 2.1 0.2%-proof/tensile values), EN 1992-1-1 / EN 206 concrete (the dual-class `concrete.c12_15` through `concrete.c90_105` plus `concrete.lc` lightweight, `fck` the yield surrogate and `fcm = fck+8` the ultimate, `Ecm = 22·(fcm/10)^0.3` GPa), EN 338:2016 timber (`C14` through `C50` softwood — the twelve-class Table 1 C-series — and the FULL fourteen-class Table 3 hardwood D-series `D18`/`D24`/`D27`/`D30`/`D35`/`D40`/`D45`/`D50`/`D55`/`D60`/`D65`/`D70`/`D75`/`D80`, the `wood.oak` D30-class generic, `fm,k` the bending surrogate), EN 14080:2013 glulam (the full seven-class homogeneous series `GL20h`/`GL22h`/`GL24h`/`GL26h`/`GL28h`/`GL30h`/`GL32h` and the seven-class combined series `GL20c`/`GL22c`/`GL24c`/`GL26c`/`GL28c`/`GL30c`/`GL32c`), EN 1999-1-1 aluminium (`6082-T6` the alloy EN 1999-1-1 names first as the most common European structural extrusion, `6061-T6`, `6063-T5`, `5083`), EN 771 masonry (`clay`/`calciumsilicate`/`aac`/`aggregate`, `fb` the normalized compressive strength) plus EN 771-6 dimension stone (`stone.marble`/`stone.granite`), EN 572 float glass, EN 13162-13166 insulation (`glasswool`/`stonewool`/`eps`/`xps`/`pir`/`pur`/`phenolic`/`woodfibre`, design λ + the EN ISO 13788 vapour factor μ), EN 520 gypsum board, the sheet-goods board substances (ASTM C1325 fibre-cement `cement.board`, EN 13986/EN 636 plywood `wood.plywood`, EN 300 OSB/3 `wood.osb` — the `Component/panel#PANEL_FAMILY` `PanelKind.SubstanceId` substance datums the panel seam reads by key), and EN 13956 membranes (`epdm`/`pvc`/`tpo`). Each row's mechanical columns are the published characteristic values, the thermal columns the design λ + μ, the acoustic Option the seventeen-band absorption + field-incidence SRI vectors a datasheet declares (only the acoustically-characterized rows carry them — a structural steel section carries `None`), and the fire Option the EN 13501-1 reaction triple + EN 13501-2 R/E/I minutes (`A1` for steel/concrete/stone/mineral-wool through `F`; most bare materials carry 0 minutes — resistance is an assembly property a `Rasm.Compute` route computes over the buildup). The roster grows by ROW to thousands of grades with NO seam touch — the catalogue is the open data surface, the seam the closed vocabulary. DUAL-KEYING CONTRACT: every row keys the SAME canonical `MaterialId` the `Properties/sustainability#SUSTAINABILITY_PROPERTY` EPD roster keys, in EXACT parity — the two curated rosters carry the IDENTICAL `MaterialId` set (the EN dual-class `concrete.c12_15`…`concrete.c90_105`, `steel.s235`…`steel.s690`, the generic `metal.steel`/`metal.iron` aliases, the EN 10088 `steel.1.4301`…`steel.1.4462`, `steel.b500a/b/c` plus the ASTM A615 `steel.gr40`…`gr80`/A706 `gr60w`/`gr80w`/CSA G30.18 `400w`/`500w` rebar grades, `timber.c14`…`c50`/`d18`…`d80`/`gl24h`…`gl32c`/`wood.oak`, `aluminium.6082t6/6061t6/6063t5/5083`, `masonry.*`, `stone.marble/granite`, `glass.float`, `insulation.glasswool/stonewool/eps/xps/pir/pur/phenolic/woodfibre`, `gypsum.board`, the sheet-goods board substances `cement.board`/`wood.plywood`/`wood.osb`, `membrane.epdm/pvc/tpo`), so a registered grade resolves a REQUIRED mechanical/thermal/fire row HERE AND a lifecycle row there via the `Projection/component#COMPONENT_SUBGRAPH` `Capture` join — no engineering-only orphan. The REQUIRED-vs-OPTIONAL distinction is a TYPE-LEVEL contract, not a curated-roster gap: `MaterialPropertyCatalogue.Lookup` rails `ElementFault.ValueRejected` for an unregistered id (engineering data is required for a known structural grade), while `SustainabilityCatalogue.Lookup` returns `Fin.Succ(empty)` for an id with no row (lifecycle data is declared-or-absent) — so an app authoring a bespoke grade with no EPD lowers a valid engineering material with an empty lifecycle set, the asymmetry living in the absence semantics rather than in a roster the curated tables happen to populate fully. The generic-grade aliases (`metal.steel` the `Component.SubstanceId` S235 fallback, `metal.iron` the ductile-casting generic, `insulation.phenolic` the EN 13166 best-λ foam) and the non-EN reinforcement grades share their lifecycle vector with the matching EN family there (the ASTM/CSA rebar grades fold onto the CRSI/ArcelorMittal EAF profile because GWP tracks rebar mass not spec grade), so the parity holds without inventing a per-grade EPD.
- [SEAM_MEASURE_COERCION]: the engineering-property SI carrier is the seam `MeasureValue` ([H2] — a `Dimension` 7-SI-exponent value-object discriminator plus a `QuantityType` `[ValueObject<string>]` name plus `MeasureValue(QuantityType, Dimension, double Si, string CanonicalUnit, Option<MeasureBand>)`), so the migration in-folder `MaterialPropertyKind.CanonicalUnit`/`MaterialUnits.Coerce` is RETIRED. `Admit` uses UnitsNet quantity values and VividOrange uncertainty models locally, lowers central SI values plus lower/upper bounds to neutral `MeasureValue`/`MeasureBand`, and passes those seam values into `OfMechanical`/`OfThermal`. UnitsNet v5 removed the `QuantityType` enum, so the `QuantityType` `[ValueObject<string>]` is a string discriminator and the `Dimension` 7-vector the physical signature. The photometric/appearance luminous coercion stays in-folder on `Appearance/photometric#PHOTOMETRIC` (an appearance concern not lowered to the seam); only the engineering-property coercion moved.
- [SEAM_OWNS_ACOUSTIC_FOLDS]: the intrinsic single-material acoustic pure folds — `Nrc` (the four octave-coincident one-third-octave bands 250/500/1k/2k arithmetic mean rounded to 0.05, ASTM C423), `Saa` (the twelve one-third-octave bands 200-2500 mean rounded to 0.01), `StcWeighted` (the ASTM E413 contour fit over the SRI bands) and `Rw` (the ISO 717-1 fit), and the shared `RatingContour.Fit` contour kernel both the rating and the `Rasm.Compute` layered fold invoke — live on the seam `Acoustic` case (`Rasm.Element` `Composition/acoustic`), so this owner READS them (a Materials consumer reads `a.Nrc`/`a.StcWeighted` off the case) and never re-authors them. The seam `Acoustic.Of` band-arity gate (the seventeen one-third-octave `AcousticBand` centres, absorption in `[0,1]`, SRI finite) is the one admission; the inline `OutOfUnit`/`NonFinite`/contour-fit kernels the migration page carried are the seam's.
- [ASSEMBLY_AGGREGATOR_RELOCATED]: the multi-ply `AssemblyAggregator` (the series-resistance U-value `1/U = Rsi + Σ(t_i/λ_i) + Rse` ISO 6946 fold, the mass-law layered-STC ISO 12354 fold over the accumulated areal mass, the rule-of-mixtures effective density/conductivity fold, the worst-ply fire envelope, and the lifecycle GWP/cost folds from `Properties/sustainability`) is RELOCATED to `Rasm.Compute` — the seam carries the per-material `MaterialPropertySet` cases on the `Material` node, Compute reads them directly, runs the assembly aggregation over the element's `MaterialComposition` plies, and writes the `Assessment` `Result` node back content-keyed on `(input key, route)`. The migration `AssemblyProperty`/`ConstituentWeight`/`AggregateLayers`/`AggregateConstituents` owners and the surface-film `Rsi`/`Rse` constants leave this folder entirely. The single-layer `UValueWM2K` column the roster carries is a per-material reference datum (λ over a nominal thickness), the assembly U-value the relocated ISO 6946 fold. Ripple counterpart: `Rasm.Compute` `Analysis/aggregator` (the multi-ply `AssemblyAggregator` folds reading the seam `MaterialComposition` + the per-ply `MaterialPropertySet`) + `Analysis/assessment` (the discipline solvers writing `Assessment` `Result` nodes).
- [IFC_MATERIAL_PROPERTIES]: the IFC 4.3 `IfcMaterialProperties` extends `IfcExtendedProperties` with a `Material` reference and a `Properties` set; the standard `Pset_MaterialMechanical`/`Pset_MaterialThermal`/`Pset_MaterialCommon` carry the canonical column names (`MassDensity`/`YoungModulus`/`PoissonRatio`/`ThermalConductivity`/`SpecificHeatCapacity`). The seam `MaterialPropertySet` cases map one-to-one onto the Psets and the `PropertyName` tokens use the canonical Pset member names, so `Rasm.Bim` reads the projected seam `Material` node's property set and emits `IfcMaterialProperties` from the seam graph — no Materials→IFC carrier, the Pset member-name mapping the `Rasm.Bim` side, the property computation this side, the seam the alignment.
