# [MATERIALS_PROPERTIES]

THE TYPED-ENGINEERING-PROPERTY SOURCE. The typed engineering-property family is SEAM-owned: the `Rasm.Element` `MaterialPropertySet` class-root `[Union]` (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`/`Damping`/`Hygrothermal`/`Durability`/`Optical`, keyed to the one `Discipline`) is the canonical material-physics carrier the `Material` node holds, the seam `MeasureValue` is the SI-coerced measure each dimensional column carries, and the intrinsic single-material acoustic folds (`Nrc`/`Saa`/`StcWeighted`/`Rw` over the shared `RatingContour.Fit` contour kernel) live on the seam `Acoustic` case — so the migration source's Materials-owned `MaterialProperty` `[Union]`, its `MaterialPropertyKind` coercion enum, and its acoustic projection folds are RETIRED, ROUTED into the seam. This owner is the Materials SOURCE: it DECLARES `Published<T>` — the ONE shared ingress carrier over `VividOrange.Uncertainties` (the `IUncertainty<TQuantity>` dimensional family admitted through the `.Quantities.Utility` `WithRelativeUncertainty` factory and the `IUncertainty<double>` scalar family through the double `.Utility`, each datum riding its `PropertyEvidence`) that `Properties/sustainability#SUSTAINABILITY_PROPERTY` COMPOSES, replacing the duplicated `Datum<TQuantity>`/`ScalarDatum` scaffolds both pages carried — and one `MaterialPropertyCatalogue`, the registered-row database of known-material engineering data per `MaterialId`, whose `Admit` lowering coerces a published row into the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Damping`/`Hygrothermal`/`Optical` cases. Every typed dimensional mint passes through the `Component/component#COMPONENT_OWNER` `QuantityRow` rows (`Density`/`Pressure`/`ThermalConductivity`/`SpecificEntropy`/`HeatTransferCoefficient` — byte-identical `QuantityType`/`Dimension`/scale to the retired inline triples, so `MeasureValue` content keys are unchanged), and the EN-vendored mechanical columns DELEGATE per `SEED_ROW_LAW`: the `steel.s235`–`steel.s460` (+`steel.s450`, `metal.steel`) `E`/`f_y`/`f_u` resolve through `EnSteelFactory.CreateBiLinear` over `EnSteelMaterial` × `EnSteelDeliveryCondition` (the EN 1993-1-1 Table 3.1 source, the `steel.md` split-brain resolved to the ONE factory) and the full six-member `EnRebarGrade` roster `steel.b450a`/`b450c`/`b500a/b/c`/`b550b` through `EnRebarFactory.CreateBiLinear` (EN 1992-1-1 §3.2 `f_yk`×ductility-`k` + `E_s` 200 GPa), the factory throws trapped ONCE at this boundary onto `Fin`; hand rows keep only the non-vendor density/Poisson/expansion/thermal/acoustic/fire columns, and the no-EN-producer rows (ASTM/CSA rebar, AISC `steel.a36`/`a992`/`a572`, EN 10025-6 `steel.s690`, stainless, every non-steel family) stay AUTHORED verbatim. A material's engineering properties are NEVER a per-discipline material type: one `Seq<MaterialPropertySet>` over one `MaterialId` carries conductivity, sound spectrum, fire rating, structural grade, damping ratio, sorption anchors, and glazing optics together — never a `StructuralMaterial`/`ThermalMaterial` surface; the multi-ply `AssemblyAggregator` is RELOCATED to `Rasm.Compute` (the seam carries per-material INPUT, never assembly aggregation). The page composes the seam smart-constructors (`MaterialPropertySet.OfMechanical`/`OfThermal`/`OfAcoustic`/`OfFire`/`OfDamping`/`OfHygrothermal`/`OfOptical`, `FireRating.Parse`, the generated `SmokeClass.TryGet`/`DropletClass.TryGet`, the `FireResistance` ctor, the six-arg `Acoustic.Of` band gate) and rails ONE band — the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (2500) every seam admission and every trapped vendor throw lifts on, never the appearance `MaterialFault` 2450 of another concern; it re-mints NO seam type and reads the seam acoustic folds it never re-authors. The lifecycle `Environmental`/`Cost` cases lower from `Properties/sustainability#SUSTAINABILITY_PROPERTY` (its `Classification` value-object is that sibling's EGRESS riding `MaterialBinding` to the bound element's Object node, never a `MaterialPropertySet` case); `Lookup` is the projector-facing resolution `Projection/component#COMPONENT_PROJECTOR` calls.

## [01]-[INDEX]

- [02]-[MATERIAL_PROPERTY_CATALOGUE]: the shared `Published<T>` ingress carrier (declared here, composed by `Properties/sustainability`), the `MechanicalSource` vendor-delegation axis (`Authored`/`EnSteel`/`EnRebar`), the `MaterialPropertyRow` published-data ingress record, the `MaterialPropertyCatalogue` registered-row database (the full structural-materials roster across steel/stainless/rebar/concrete/CMU/timber/glulam/aluminium/masonry/stone/glass/insulation/gypsum/boards/membranes), the `Admit` row→seam-case lowering minting through `QuantityRow`, and the `Lookup` resolution the projector calls.
- [03]-[ASSESSMENT_INPUT]: why Materials authors NO assessment-input node — the material's `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the input `Rasm.Compute` reads off the graph directly.
- [04]-[RESEARCH]: the seam contract, roster-coverage, delegation, coercion, and relocation registers.

## [02]-[MATERIAL_PROPERTY_CATALOGUE]

- Owner: `Published<T>` the ONE shared evidence-bearing uncertainty carrier both Properties owners ride; `MechanicalSource` the closed `[Union]` mechanical-column source axis (`Authored` stored triple / `EnSteel` / `EnRebar` vendor delegation); `MaterialPropertyRow` the published-data ingress record; `MaterialPropertyUncertainty` the named relative-confidence profile; `MaterialPropertyCatalogue` the registered-row database; `Admit` the row→seam-case lowering; `Lookup` the projector-facing resolution.
- Cases: one `MaterialPropertyRow` shape across all materials — mechanical (`Density` + the `MechanicalSource`-resolved `E`/`f_y`/`f_u` + `Poisson`/`Expansion`), thermal (`Conductivity`/`SpecificHeat`/`UValue`/`VapourMu`), optional acoustic (the eighteen-band absorption + SRI vectors, now with the seam's optional `DynamicStiffnessMNPerM3`/`FlowResistivityPaSPerM2`/`LossFactor` intrinsics), optional fire (EN 13501-1 `(Reaction, Smoke, Droplets)` + EN 13501-2 R/E/I minutes), optional damping (the EN 1998-1 fraction-of-critical ζ), optional hygrothermal (the EN 15026 porosity + `w80`/`wf` sorption anchors + capillary A-value), and optional optical (the EN 410 nine-column glazing record); `Admit` produces a `Seq<MaterialPropertySet>` of the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Damping`/`Hygrothermal`/`Optical` cases — the lifecycle `Environmental`/`Cost` lower from `Properties/sustainability#SUSTAINABILITY_PROPERTY`, the directional `Orthotropic` from `Component/timber#TIMBER_FAMILY`, the `Durability` case awaits an admitted fib data source (`[04]-[RESEARCH]`) — each case a `MaterialPropertySet` over a `MaterialId`, never a property subtype.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key)` — resolves the `MechanicalSource` (stored `Published<Pressure>` triple, or the `EnSteelFactory.CreateBiLinear`/`EnRebarFactory.CreateBiLinear` vendor build with the throw trapped onto `ElementFault.ValueRejected` via the kernel `Op.Catch` funnel), mints every dimensional column through the `QuantityRow` typed-mint rows with the `Published<T>.Band` provider-model→`MeasureBand` lowering riding `Some`, passes the scalar columns central-only (the seam guards Poisson `[0,0.5]`, μ `>= 1`, ζ `[0,1)`, the isotherm `wf >= w80`, the optical conservation refinements), folds the acoustic vectors + intrinsics through the six-arg seam `Acoustic.Of` gate, and the fire triple through `FireRating.Parse` + the generated `SmokeClass.TryGet`/`DropletClass.TryGet` + the three-criterion `FireResistance` ctor — only the `Strength` resolution BINDS (mechanical's dependency); the seven discipline groups and the three fire tokens are INDEPENDENT and ACCUMULATE applicatively (`ToValidation` slots, tuple `Apply`, one `ToFin`), so a row with several rejected columns faults them ALL in one `Fin.Fail` `ManyErrors`; `MaterialPropertyCatalogue.Lookup(MaterialId id, Op key)` resolves a registered material to its lowered seam-case set, faulting `ElementFault.ValueRejected` for an unregistered material (engineering properties are REQUIRED for a known structural material — the asymmetric dual of the OPTIONAL `SustainabilityCatalogue.Lookup` that returns empty) — one polymorphic resolution, never a `GetMechanical`/`GetThermal` family.
- Packages: Rasm.Element (project — `MaterialPropertySet` + its `Of*` admissions, `MeasureValue`/`MeasureBand`/`UncertaintyKind`, `PropertyEvidence`, `FireRating.Parse`, `SmokeClass`/`DropletClass`, `FireResistance`, `Acoustic.Of`, `Discipline`, `MaterialId`, `ElementFault.ValueRejected`), Rasm.Materials.Component (project — the `QuantityRow` typed-mint owner), VividOrange.Uncertainties + VividOrange.Uncertainties.Quantities (the four uncertainty models over the `double` and `IQuantity` carriers, the fluent `WithRelativeUncertainty` admissions, the `IUncertainty<T>` kind interfaces `Published<T>.Band` discriminates), VividOrange.Materials (`EnSteelFactory`/`EnRebarFactory`/`EnSteelMaterial`/`EnSteelGrade`/`EnRebarGrade`/`EnSteelDeliveryCondition`/`IBiLinearMaterial` — the EN 1993-1-1 Table 3.1 + EN 1992-1-1 §3.2 vendor tables), VividOrange.Standards (`NationalAnnex`), UnitsNet (`Density`/`Pressure`/`ThermalConductivity`/`SpecificEntropy`/`HeatTransferCoefficient`/`Length` — raw-to-SI coercion at this boundary only), NodaTime (`LocalDate` evidence expiry), Rasm (project — `Op` + the `Op.Catch` trap funnel), Thinktecture.Runtime.Extensions (`[Union]` the `MechanicalSource` axis), LanguageExt.Core (`Fin`/`Seq`/`Option` + `Match`/`Map`), BCL inbox (`FrozenDictionary`, `ReadOnlyMemory<double>`, `ImmutableArray<T>`).
- Growth: a new engineering property shared across materials is one column on the matching seam case the `MaterialPropertyRow` gains a published column for and `Admit` lowers; a new known material is one `Rows` entry (the roster grows by row to thousands with no seam touch); a new vendor grade table is one `MechanicalSource` case + one `Strength` dispatch arm (compiler-forced at the generated `Switch`); a new property discipline is one seam case — the `Damping`/`Hygrothermal`/`Durability`/`Optical` cases landed exactly this way and this catalogue sources three of them today, the fourth gated on published data — never a parallel Materials union, never a per-discipline material type.
- Boundary: `MaterialPropertyRow` is the published-DATA ingress, NOT a parallel domain union — the seam `MaterialPropertySet` is the one typed carrier and `Admit` the one `BOUNDARY_ADMISSION`; the dimensional columns coerce to SI through `UnitsNet` reads inside the `QuantityRow`-typed `Measure` mint (never a Materials coercion enum), the provider uncertainty models lower to neutral `MeasureBand` bounds at exactly that mint, and provider types never cross into `Rasm.Element`; the vendor factories are exception-throwing at their derivation boundary (`ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException`) so `Strength` traps them ONCE via `Op.Catch` and lowers onto the SAME `ElementFault.ValueRejected` band every seam admission rails — a mixed-band `Admit` chain is the rejected cross-concern leak; the per-band `ScalarDatum` wrapping of the acoustic vectors is DELETED as unread ceremony (the seam takes raw `ReadOnlyMemory<double>` centrals; a group-level `PropertyEvidence` rides `AcousticDatum`); the seam `Acoustic` case carries the intrinsic rating folds this owner reads and never re-authors; the lowered `Seq<MaterialPropertySet>` is what `Projection/component#COMPONENT_PROJECTOR` writes onto the seam `Material` node, and the catalogue crosses to `Rasm.Compute`/`Rasm.Bim` only through the seam graph, never a Materials wire carrier.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using LanguageExt.Common;                             // Error — the Validation slot the applicative discipline-group join accumulates
using NodaTime;                                       // LocalDate — the vendor/EPD evidence expiry PropertyEvidence carries
using Rasm.Domain;                                    // Op + the Op.Catch boundary trap funnel
using Rasm.Element.Composition;                                   // MaterialId, MaterialPropertySet, MeasureValue, MeasureBand, UncertaintyKind,
using Rasm.Element.Projection;
using Rasm.Element.Properties;
                                                      // PropertyEvidence, FireRating, SmokeClass, DropletClass, FireResistance, Acoustic,
                                                      // ElementFault (band 2500 — the ONE band this SOURCE rails, never MaterialFault 2450)
using Rasm.Materials.Component;                       // QuantityRow — the one typed-mint owner (byte-identical QuantityType/Dimension/scale)
using UnitsNet;
using VividOrange.Materials;                          // IBiLinearMaterial — the E/f_y/f_u law the EN factories return
using VividOrange.Materials.StandardMaterials.En;     // EnSteelGrade, EnSteelMaterial, EnSteelFactory, EnSteelDeliveryCondition, EnRebarGrade, EnRebarFactory
using VividOrange.Standards.Eurocode;                 // NationalAnnex (the EN factory annex axis; Table 3.1 strengths are annex-independent)
using VividOrange.Uncertainties;                      // IUncertainty<T> + the four kind interfaces
using VividOrange.Uncertainties.Quantities.Utility;   // (TQuantity).WithRelativeUncertainty — the dimensional admission
using VividOrange.Uncertainties.Utility;              // (double).WithRelativeUncertainty — the scalar admission
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // the property-catalogue folder owner — the projector imports Rasm.Materials.Properties

// --- [MODELS] ------------------------------------------------------------------------------
// THE shared Published ingress carrier BOTH Properties owners ride (declared HERE, composed by
// Properties/sustainability#SUSTAINABILITY_PROPERTY): one evidence-bearing uncertainty datum over both
// VividOrange carriers — IUncertainty<TQuantity> dimensional rows and IUncertainty<double> scalar rows —
// replacing the Datum<TQuantity>/ScalarDatum parallel pair each page duplicated. Band lowers the provider
// model to the neutral seam MeasureBand at the ONE seam crossing; provider types never leave this folder.
public readonly record struct Published<T>(IUncertainty<T> Value, PropertyEvidence Evidence) {
    public UncertaintyKind Kind => Value switch {
        IAbsoluteUncertainty<T> => UncertaintyKind.Absolute,
        IRelativeUncertainty<T> => UncertaintyKind.Relative,
        IIntervalUncertainty<T> => UncertaintyKind.Interval,
        INormalDistributionUncertainty<T> => UncertaintyKind.Normal,
        _ => UncertaintyKind.Exact,
    };

    public MeasureBand Band(Func<T, double> si) =>
        Value is INormalDistributionUncertainty<T> normal
            ? MeasureBand.Normal(si(Value.LowerBound), si(Value.UpperBound), si(normal.StandardDeviation), normal.CoverageFactor)
            : MeasureBand.Interval(Kind, si(Value.LowerBound), si(Value.UpperBound));
}

// The Of family discriminates on carrier (MODAL_ARITY): a UnitsNet quantity admits through the
// .Quantities.Utility relative factory, a raw double through the double .Utility; Vector/Centrals are the
// banded-series duals the sibling's StageGwp vector rides. Central is the scalar read every raw-crossing
// column lowers through (the seam guards centrals; only the dimensional Measure mints carry a band).
// An interval/normal-published column is ONE more Of arm here — Kind/Band already lower all four models.
public static class Published {
    public static Published<TQuantity> Of<TQuantity>(TQuantity value, double relative, PropertyEvidence evidence) where TQuantity : IQuantity =>
        new(value.WithRelativeUncertainty(relative), evidence.Normalized());

    public static Published<double> Of(double value, double relative, PropertyEvidence evidence) =>
        new(value.WithRelativeUncertainty(relative), evidence.Normalized());

    public static ImmutableArray<Published<double>> Vector(ReadOnlyMemory<double> values, double relative, PropertyEvidence evidence) =>
        [.. values.ToArray().Select(value => Of(value, relative, evidence))];

    public static ReadOnlyMemory<double> Centrals(ImmutableArray<Published<double>> values) =>
        values.Select(static datum => datum.Central).ToArray();

    extension(Published<double> datum) {
        public double Central => datum.Value.CentralValue;
    }
}

// The SEED_ROW_LAW mechanical-column source axis: an EN grade with an admitted vendor producer DELEGATES its
// E/f_y/f_u to the factory table (hand re-transcriptions of vendor-owned values DELETE); a no-producer grade
// stays Authored. Growth: a new vendor table is one case here plus one Strength arm — compiler-forced.
[Union]
public abstract partial record MechanicalSource {
    public sealed record Authored(Published<Pressure> Youngs, Published<Pressure> Yield, Published<Pressure> Ultimate) : MechanicalSource;
    public sealed record EnSteel(EnSteelGrade Grade, EnSteelDeliveryCondition Delivery) : MechanicalSource;   // EN 1993-1-1 Table 3.1 via EnSteelFactory (delivery selects the AR/N/M/Q sub-table — the decompiled EnSteelDeliveryCondition axis)
    public sealed record EnRebar(EnRebarGrade Grade) : MechanicalSource;                                      // EN 1992-1-1 §3.2 f_yk × ductility-k + E_s 200 GPa via EnRebarFactory
}

// The acoustic ingress group: the two eighteen-band vectors RAW (per-band uncertainty wrapping was unread
// ceremony — only centrals cross the seam) plus the seam Acoustic.Of optional intrinsics — dynamic stiffness
// s' (EN 29052-1, the floating-floor input), flow resistivity r (EN 29053, the Delany-Bazley porous-absorber
// input), and the small-strain loss factor η — and ONE group evidence.
public sealed record AcousticDatum(
    ReadOnlyMemory<double> Absorption,
    ReadOnlyMemory<double> Reduction,
    Option<double> DynamicStiffnessMNPerM3,
    Option<double> FlowResistivityPaSPerM2,
    Option<double> LossFactor,
    PropertyEvidence Evidence);

public sealed record FireDatum(
    string Reaction,
    string Smoke,
    string Droplets,
    int LoadBearingMinutes,
    int IntegrityMinutes,
    int InsulationMinutes,
    PropertyEvidence Evidence);

// The relative-confidence profile the two banded discipline groups read (the acoustic vectors cross raw —
// per-band wrapping was unread ceremony, so no acoustic column exists here).
public readonly record struct MaterialPropertyUncertainty(
    double Mechanical,
    double Thermal) {
    public static readonly MaterialPropertyUncertainty Catalogue = new(0.05, 0.05);
}

// The published engineering data for one material — the ingress row Admit lowers into the seam cases. A flat
// DATA record, not a parallel domain union; NO primary ctor — the AUTHORED/DELEGATED admission ctors below are
// the only construction (a positional head would force a `this(...)` chain the delegated body cannot spell).
// The hygrothermal/optical groups are named tuples (ingress data, not domain shapes); the seam cases are the
// typed owners. Evidence derives from the density column (every column shares the construction evidence —
// DERIVED_LOGIC, no parallel field).
public sealed record MaterialPropertyRow {
    public Published<Density> Density { get; }
    public MechanicalSource Mechanical { get; }
    public Published<double> Poisson { get; }
    public Published<double> Expansion { get; }
    public Published<ThermalConductivity> Conductivity { get; }
    public Published<SpecificEntropy> SpecificHeat { get; }
    public Published<HeatTransferCoefficient> UValue { get; }
    public Published<double> VapourMu { get; }
    public Option<AcousticDatum> Acoustic { get; }
    public Option<FireDatum> Fire { get; }
    public Option<double> DampingRatio { get; }
    public Option<(double Porosity, double W80KgM3, double WfKgM3, Option<double> AValueKgM2SqrtS)> Hygrothermal { get; }
    public Option<(double VisibleT, double VisibleRf, double VisibleRb, double SolarT, double SolarRf, double SolarRb, double IrT, double IrEf, double IrEb)> Optical { get; }
    public MaterialPropertyUncertainty Confidence { get; }
    public PropertyEvidence Evidence => Density.Evidence;

    // The AUTHORED admission ctor — the roster's twelve positional columns are unchanged from the prior shape;
    // the discipline extensions ride trailing optionals so an existing row literal never re-spells. A null
    // uncertainty resolves the Catalogue profile; a default evidence normalizes to Catalogue provenance.
    public MaterialPropertyRow(
        double densityKgM3, double youngsModulusMpa, double yieldStrengthMpa, double ultimateStrengthMpa,
        double poissonsRatio, double thermalExpansionPerK,
        double conductivityWMK, double specificHeatJKgK, double uValueWM2K, double vapourResistanceFactorMu,
        Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> acoustic,
        Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> fire,
        Option<double> damping = default,
        Option<double> flowResistivity = default,
        Option<double> dynamicStiffness = default,
        Option<double> lossFactor = default,
        Option<(double Porosity, double W80KgM3, double WfKgM3, Option<double> AValueKgM2SqrtS)> hygrothermal = default,
        Option<(double VisibleT, double VisibleRf, double VisibleRb, double SolarT, double SolarRf, double SolarRb, double IrT, double IrEf, double IrEb)> optical = default,
        PropertyEvidence evidence = default,
        MaterialPropertyUncertainty? uncertainty = null)
        : this(
            Source(youngsModulusMpa, yieldStrengthMpa, ultimateStrengthMpa, evidence, uncertainty ?? MaterialPropertyUncertainty.Catalogue),
            densityKgM3, poissonsRatio, thermalExpansionPerK, conductivityWMK, specificHeatJKgK, uValueWM2K, vapourResistanceFactorMu,
            acoustic, fire, damping, flowResistivity, dynamicStiffness, lossFactor, hygrothermal, optical, evidence, uncertainty) { }

    // The DELEGATED admission ctor — a vendor-sourced grade passes its MechanicalSource arm and only the
    // non-vendor columns (SEED_ROW_LAW: the factory owns E/f_y/f_u; density/Poisson/expansion/thermal/
    // acoustic/fire have no EN producer and stay hand-published).
    public MaterialPropertyRow(
        MechanicalSource mechanical,
        double densityKgM3, double poissonsRatio, double thermalExpansionPerK,
        double conductivityWMK, double specificHeatJKgK, double uValueWM2K, double vapourResistanceFactorMu,
        Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> acoustic,
        Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> fire,
        Option<double> damping = default,
        Option<double> flowResistivity = default,
        Option<double> dynamicStiffness = default,
        Option<double> lossFactor = default,
        Option<(double Porosity, double W80KgM3, double WfKgM3, Option<double> AValueKgM2SqrtS)> hygrothermal = default,
        Option<(double VisibleT, double VisibleRf, double VisibleRb, double SolarT, double SolarRf, double SolarRb, double IrT, double IrEf, double IrEb)> optical = default,
        PropertyEvidence evidence = default,
        MaterialPropertyUncertainty? uncertainty = null) {
        MaterialPropertyUncertainty confidence = uncertainty ?? MaterialPropertyUncertainty.Catalogue;
        PropertyEvidence normalized = evidence.Normalized();
        Density = Published.Of(UnitsNet.Density.FromKilogramsPerCubicMeter(densityKgM3), confidence.Mechanical, normalized);
        Mechanical = mechanical;
        Poisson = Published.Of(poissonsRatio, confidence.Mechanical, normalized);
        Expansion = Published.Of(thermalExpansionPerK, confidence.Mechanical, normalized);
        Conductivity = Published.Of(ThermalConductivity.FromWattsPerMeterKelvin(conductivityWMK), confidence.Thermal, normalized);
        SpecificHeat = Published.Of(SpecificEntropy.FromJoulesPerKilogramKelvin(specificHeatJKgK), confidence.Thermal, normalized);
        UValue = Published.Of(HeatTransferCoefficient.FromWattsPerSquareMeterKelvin(uValueWM2K), confidence.Thermal, normalized);
        VapourMu = Published.Of(vapourResistanceFactorMu, confidence.Thermal, normalized);
        Acoustic = acoustic.Map(a => new AcousticDatum(a.Absorption, a.Sri, dynamicStiffness, flowResistivity, lossFactor, normalized));
        Fire = fire.Map(f => new FireDatum(f.Reaction, f.Smoke, f.Droplets, f.LoadBearingMin, f.IntegrityMin, f.InsulationMin, normalized));
        DampingRatio = damping;
        Hygrothermal = hygrothermal;
        Optical = optical;
        Confidence = confidence;
    }

    static MechanicalSource Source(double youngsMpa, double yieldMpa, double ultimateMpa, PropertyEvidence evidence, MaterialPropertyUncertainty confidence) =>
        new MechanicalSource.Authored(
            Published.Of(Pressure.FromMegapascals(youngsMpa), confidence.Mechanical, evidence),
            Published.Of(Pressure.FromMegapascals(yieldMpa), confidence.Mechanical, evidence),
            Published.Of(Pressure.FromMegapascals(ultimateMpa), confidence.Mechanical, evidence));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialPropertyCatalogue {
    // The <=40 mm Table 3.1 band the substance datum reads (the roster's documented t <= 16 mm nominal);
    // per-section thickness banding stays Component/steel#STEEL_FAMILY YieldMpa — never re-solved here.
    const double GradeThicknessMm = 16.0;

    static readonly PropertyEvidence SteelTable = new("vendor", "en 1993-1-1 table 3.1 / vividorange.materials", Option<LocalDate>.None);
    static readonly PropertyEvidence RebarTable = new("vendor", "en 1992-1-1 §3.2 + en 10080 / vividorange.materials", Option<LocalDate>.None);

    // The empty discipline-group slot an absent optional group contributes to the applicative join.
    static readonly Validation<Error, Seq<MaterialPropertySet>> NoGroup = Success<Error, Seq<MaterialPropertySet>>(Seq<MaterialPropertySet>());

    // Lowers a published row into the seam cases. EVERY value flows through a seam admission (the Of*
    // constructors, FireRating.Parse, the TryGet gate) or the trapped vendor build, so the WHOLE chain rails
    // ONE band — ElementFault.ValueRejected (2500). Strength is mechanical's ONE dependency and binds first;
    // the seven discipline groups are INDEPENDENT and accumulate APPLICATIVELY — the seam's own
    // VALIDATION_MONOID admission shape one level up, so a curated row with a bad fire token AND a bad
    // optical fraction reports BOTH in one Fin.Fail (ManyErrors), never first-fault-only. Mechanical/Thermal
    // mint banded MeasureValues through the QuantityRow rows; Poisson/μ/ζ/sorption/optics cross central-only
    // (the seam guards range and refinement: Poisson [0,0.5], μ >= 1, ζ [0,1), wf >= w80, per-band τ+ρ <= 1).
    // Damping passes no Rayleigh pair — the (α, β) calibration is a per-model FE input, never a catalogue datum.
    public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key) =>
        Strength(row.Mechanical, row.Confidence, key).Bind(strength =>
            (MaterialPropertySet.OfMechanical(
                 Measure(row.Density, static q => q.KilogramsPerCubicMeter, QuantityRow.Density),
                 Measure(strength.Youngs, static q => q.Pascals, QuantityRow.Pressure),
                 Measure(strength.Yield, static q => q.Pascals, QuantityRow.Pressure),
                 Measure(strength.Ultimate, static q => q.Pascals, QuantityRow.Pressure),
                 row.Poisson.Central, row.Expansion.Central, key, row.Evidence).ToValidation(),
             MaterialPropertySet.OfThermal(
                 Measure(row.Conductivity, static q => q.WattsPerMeterKelvin, QuantityRow.ThermalConductivity),
                 Measure(row.SpecificHeat, static q => q.JoulesPerKilogramKelvin, QuantityRow.SpecificEntropy),
                 Measure(row.UValue, static q => q.WattsPerSquareMeterKelvin, QuantityRow.HeatTransferCoefficient),
                 row.VapourMu.Central, key, row.Evidence).ToValidation(),
             row.Acoustic.Match(
                 None: static () => NoGroup,
                 Some: a => Acoustic.Of(a.Absorption, a.Reduction, key, a.DynamicStiffnessMNPerM3, a.FlowResistivityPaSPerM2, a.LossFactor)
                     .Map(spectrum => Seq(MaterialPropertySet.OfAcoustic(spectrum, a.Evidence))).ToValidation()),
             row.Fire.Match(
                 None: static () => NoGroup,
                 // FireRating.Parse is the seam's ONE fire-reaction admission; SmokeClass/DropletClass expose NO
                 // Parse, so the generated TryGet resolves. The three tokens and the independent R/E/I minutes
                 // accumulate before the admitted FireResistance reaches the total OfFire constructor.
                 Some: f => (FireRating.Parse(f.Reaction, key).ToValidation(),
                             Sub(SmokeClass.TryGet, f.Smoke, key, "smoke").ToValidation(),
                             Sub(DropletClass.TryGet, f.Droplets, key, "droplet").ToValidation(),
                             FireResistance.Of(f.LoadBearingMinutes, f.IntegrityMinutes, f.InsulationMinutes, key).ToValidation())
                     .Apply((reaction, smoke, droplets, resistance) => Seq(MaterialPropertySet.OfFire(reaction, smoke, droplets, resistance, f.Evidence))).As()),
             row.DampingRatio.Match(
                 None: static () => NoGroup,
                 Some: zeta => MaterialPropertySet.OfDamping(zeta, Option<(double AlphaPerS, double BetaS)>.None, key, row.Evidence).Map(set => Seq(set)).ToValidation()),
             row.Hygrothermal.Match(
                 None: static () => NoGroup,
                 Some: h => MaterialPropertySet.OfHygrothermal(h.Porosity, h.W80KgM3, h.WfKgM3, h.AValueKgM2SqrtS, key, row.Evidence).Map(set => Seq(set)).ToValidation()),
             row.Optical.Match(
                 None: static () => NoGroup,
                 Some: o => MaterialPropertySet.OfOptical(o.VisibleT, o.VisibleRf, o.VisibleRb, o.SolarT, o.SolarRf, o.SolarRb, o.IrT, o.IrEf, o.IrEb, key, row.Evidence).Map(set => Seq(set)).ToValidation()))
            .Apply(static (mechanical, thermal, acoustic, fire, damping, hygrothermal, optical) =>
                Seq(mechanical, thermal) + acoustic + fire + damping + hygrothermal + optical).As()
            .ToFin());

    // The QuantityRow-typed banded mint: the row carries the byte-identical QuantityType/Dimension/scale/unit
    // the retired inline triples spelled, the Published band lowers through the SAME scaled SI read — one mint
    // site, content keys unchanged (the engineering rows all carry Scale 1.0).
    static MeasureValue Measure<TQuantity>(Published<TQuantity> datum, Func<TQuantity, double> si, QuantityRow row) where TQuantity : IQuantity {
        double Scaled(TQuantity value) => si(value) * row.Scale;
        return new(row.Type, row.Dim, Scaled(datum.Value.CentralValue), row.Unit, Some(datum.Band(Scaled)));
    }

    // The SEED_ROW_LAW dispatch (one exhaustive generated Switch): Authored reads the stored triple; the EN
    // arms build the grade record and read the vendor law, the factory's construction/derivation throws
    // (ArgumentException, MissingNationalAnnexException, InvalidSteelSpecificationException) trapped ONCE via
    // the kernel Op.Catch funnel onto the page's one band. Table 3.1 strengths are annex-independent, so the
    // build pins NationalAnnex.RecommendedValues; the delivery condition routes the AR/N/M/Q sub-table that
    // holds the grade (AR/EN 10025-2 holds S235/S275/S355/S450; N/EN 10025-3 holds S420/S460; the Q/EN 10025-6
    // sub-table holds only S460 — EnSteelGrade tops out at S460, so S690 has no producer and stays AUTHORED).
    // The spec default HollowSection=false pins the non-hollow tables — the factory's "hollow section not set"
    // throw is unreachable from this build.
    static Fin<(Published<Pressure> Youngs, Published<Pressure> Yield, Published<Pressure> Ultimate)> Strength(MechanicalSource source, MaterialPropertyUncertainty confidence, Op key) =>
        source.Switch(
            authored: a => Fin.Succ((a.Youngs, a.Yield, a.Ultimate)),
            enSteel: s => key.Catch(() => {
                    EnSteelMaterial material = new(s.Grade, NationalAnnex.RecommendedValues);
                    material.Specification.DeliveryCondition = s.Delivery;
                    return Fin.Succ(EnSteelFactory.CreateBiLinear(material, Length.FromMillimeters(GradeThicknessMm)));
                })
                .MapFail(error => ElementFault.ValueRejected(key, $"<en-steel-grade:{s.Grade}:{s.Delivery}:{error.Message}>"))
                .Map(law => Wrap(law, confidence, SteelTable)),
            enRebar: r => key.Catch(() => Fin.Succ(EnRebarFactory.CreateBiLinear(r.Grade)))
                .MapFail(error => ElementFault.ValueRejected(key, $"<en-rebar-grade:{r.Grade}:{error.Message}>"))
                .Map(law => Wrap(law, confidence, RebarTable)));

    static (Published<Pressure> Youngs, Published<Pressure> Yield, Published<Pressure> Ultimate) Wrap(IBiLinearMaterial law, MaterialPropertyUncertainty confidence, PropertyEvidence evidence) =>
        (Published.Of(law.ElasticModulus, confidence.Mechanical, evidence),
         Published.Of(law.YieldStrength, confidence.Mechanical, evidence),
         Published.Of(law.UltimateStrength, confidence.Mechanical, evidence));

    // The EN 13501-1 sub-class admission: SmokeClass/DropletClass are seam [SmartEnum<string>] with NO Parse
    // wrapper, so an empty token resolves the seam's NotSpecified row and a present token resolves through the
    // Thinktecture-generated TryGet — railing ElementFault.ValueRejected on an out-of-domain class, the SAME
    // band (and "Value" telemetry Category) FireRating.Parse rails, so the whole fire admission carries one band.
    delegate bool TryGetter<T>(string? token, out T? value);
    static Fin<T> Sub<T>(TryGetter<T> tryGet, string token, Op key, string label) where T : class =>
        tryGet(token, out T? value) && value is { } row
            ? Fin.Succ(row)
            : ElementFault.ValueRejected(key, $"<fire-{label}-class-unknown:{token}>");

    // --- [TABLES]
    // Row-literal anchors: the shared fire classifications, the EN 1998-1 §3 / ISO 10137 design damping ζ per
    // structural family (welded steel + aluminium 0.02, RC/masonry/stone 0.05, timber 0.08), and the empty
    // acoustic Option — spelling anchors only, every row VALUE verbatim.
    static readonly Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> NoAcoustic = default;
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireA1 = Some(("A1", "", "", 0, 0, 0));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireA1Ei120 = Some(("A1", "", "", 0, 120, 120));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireA2 = Some(("A2", "s1", "d0", 0, 0, 0));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireB = Some(("B", "s1", "d0", 0, 0, 0));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireC = Some(("C", "s1", "d0", 0, 0, 0));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireD = Some(("D", "s2", "d0", 0, 0, 0));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireD30 = Some(("D", "s2", "d0", 30, 30, 30));
    static readonly Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> FireE = Some(("E", "s2", "d0", 0, 0, 0));
    static readonly Option<double> ZSteel = Some(0.02);
    static readonly Option<double> ZConcrete = Some(0.05);
    static readonly Option<double> ZTimber = Some(0.08);

    // The structural-materials roster — every row a published EN/ASTM/CSA datasheet keyed by the canonical
    // MaterialId (seam-generated ordinal-ignore-case equality keys the table), in EXACT MaterialId parity with
    // the Properties/sustainability EPD roster (FULL_ROSTER — every engineering id resolves a lifecycle id).
    // Mechanical columns: DELEGATED rows read EN 1993-1-1 Table 3.1 (EnSteelFactory) or EN 1992-1-1 §3.2
    // (EnRebarFactory) at Admit; AUTHORED rows store the published CHARACTERISTIC values (EN 1993-1-4 Table 2.1
    // stainless, EN 1992-1-1 Table 3.1 fck/fcm/Ecm printed values, EN 338/14080 fm,k, EN 1999-1-1 f0/fu, ASTM
    // A615/A706 + CSA G30.18 fy/fu_min with the ACI 318 §20.2.2.2 E_s 200 GPa, AISC A36/A992/A572 fy/fu_min
    // with E 200 GPa, ASTM C90/TMS 402 f'm + E_m = 900·f'm for the CMU substance). Thermal: EN ISO 10456 design
    // λ + the EN ISO 13788 vapour factor μ; the U-value column is a single-layer reference the EN ISO 6946
    // assembly fold in Rasm.Compute supersedes. Acoustic: the eighteen-band absorption + field-incidence SRI
    // vectors only the acoustically-characterized rows carry, the porous rows carrying the EN 29053 flow
    // resistivity the Delany-Bazley route reads. Fire: EN 13501-1 reaction + EN 13501-2 R/E/I minutes (most
    // carry 0 — resistance is an assembly property Rasm.Compute computes over the buildup). Damping: the EN
    // 1998-1 design ζ on the structural families. Hygrothermal: the WUFI/Fraunhofer-published sorption anchors
    // on the characterized porous rows. Optical: the EN 410 published clear-glass record on the glass rows.
    public static readonly FrozenDictionary<MaterialId, MaterialPropertyRow> Rows = new (MaterialId Id, MaterialPropertyRow Row)[] {
        // --- structural carbon steel (EN 10025-2/-3/-4; DELEGATED to EnSteelFactory — Table 3.1 <=40 mm band:
        //     S235 235/360, S275 275/430, S355 355/490 on AR; S420 420/520, S460 460/540 on N; S450 440/550 on AR;
        //     E 210 GPa the factory law. Hand columns: rho 7850, nu 0.30, alpha 12e-6, lambda ~50, c ~460, A1.)
        (MaterialId.Of("steel.s235"), new(new MechanicalSource.EnSteel(EnSteelGrade.S235, EnSteelDeliveryCondition.AR), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.s275"), new(new MechanicalSource.EnSteel(EnSteelGrade.S275, EnSteelDeliveryCondition.AR), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.s355"), new(new MechanicalSource.EnSteel(EnSteelGrade.S355, EnSteelDeliveryCondition.AR), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.s420"), new(new MechanicalSource.EnSteel(EnSteelGrade.S420, EnSteelDeliveryCondition.N), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // steel.s450 — the EN 10025-2 S450 grade the Component/steel#STEEL_FAMILY SteelGrade.S450 SubstanceId keys (Table 3.1 AR 440/550)
        (MaterialId.Of("steel.s450"), new(new MechanicalSource.EnSteel(EnSteelGrade.S450, EnSteelDeliveryCondition.AR), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.s460"), new(new MechanicalSource.EnSteel(EnSteelGrade.S460, EnSteelDeliveryCondition.N), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // EN 10025-6 quenched-and-tempered S690QL — outside Table 3.1 (no factory producer), AUTHORED (fy 690 / fu 770)
        (MaterialId.Of("steel.s690"), new(7850.0, 210_000.0, 690.0, 770.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // metal.steel — the generic-structural-steel alias Component.SubstanceId resolves on an unspecified grade:
        // the conservative S235 baseline DELEGATED through the same factory row, so the connection-design seam
        // reads a real Mechanical row rather than faulting; a graded connector keys steel.s355 directly.
        (MaterialId.Of("metal.steel"), new(new MechanicalSource.EnSteel(EnSteelGrade.S235, EnSteelDeliveryCondition.AR), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // metal.iron — the cast/wrought-iron generic the Component/joint weld family keys; ductile EN-GJS-400-15
        // baseline (E ~170 GPa, fy ~250 / fu ~400) — no EN factory producer, AUTHORED; keyed in BOTH catalogues.
        (MaterialId.Of("metal.iron"), new(7200.0, 170_000.0, 250.0, 400.0, 0.28, 11.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // --- AISC structural steel (ASTM A36 250/400, A992 345/450, A572 Gr50 345/450; E 200 GPa AISC — no EN
        //     producer, FLOOR_SCOPE_GATE, AUTHORED) — the Component/steel#STEEL_FAMILY A36/A992/A572 SubstanceId rows
        (MaterialId.Of("steel.a36"),  new(7850.0, 200_000.0, 250.0, 400.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.a992"), new(7850.0, 200_000.0, 345.0, 450.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.a572"), new(7850.0, 200_000.0, 345.0, 450.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // --- reinforcing steel (the FULL six-member EnRebarGrade roster DELEGATED to EnRebarFactory: fyk parsed
        //     from the grade digits, fu = k·fyk by ductility class A 1.05 / B 1.08 / C 1.15, E_s 200 GPa — the
        //     EN 1992-1-1 §3.2.7 law; B450A/C the Italian NAD grades, B550B the Scandinavian;
        //     ASTM A615/A706 + CSA G30.18 have no EN producer, AUTHORED per the spec tables at E_s 200 GPa)
        (MaterialId.Of("steel.b450a"), new(new MechanicalSource.EnRebar(EnRebarGrade.B450A), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.b450c"), new(new MechanicalSource.EnRebar(EnRebarGrade.B450C), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.b500a"), new(new MechanicalSource.EnRebar(EnRebarGrade.B500A), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.b500b"), new(new MechanicalSource.EnRebar(EnRebarGrade.B500B), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.b500c"), new(new MechanicalSource.EnRebar(EnRebarGrade.B500C), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.b550b"), new(new MechanicalSource.EnRebar(EnRebarGrade.B550B), 7850.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.gr40"),  new(7850.0, 200_000.0, 280.0, 420.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.gr60"),  new(7850.0, 200_000.0, 420.0, 620.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.gr75"),  new(7850.0, 200_000.0, 520.0, 690.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.gr80"),  new(7850.0, 200_000.0, 550.0, 725.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.gr60w"), new(7850.0, 200_000.0, 420.0, 550.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.gr80w"), new(7850.0, 200_000.0, 550.0, 690.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.400w"),  new(7850.0, 200_000.0, 400.0, 540.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.500w"),  new(7850.0, 200_000.0, 500.0, 620.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // --- cold-formed sheet + fasteners (AISI SS Grade 33/50 sheet g33/g50; ISO 898-1/SAE J429/ASTM F3125 bolt
        //     classes fastener-*) — the Component connector Gauge / fastener Grade SubstanceId rows; no EN producer,
        //     AUTHORED. Carbon-steel physics identical to A36 (rho 7850, E 200 GPa, nu 0.30, alpha 12e-6, lambda 50,
        //     c 460, A1); only fy/fu vary — bolt class X.Y is fu=100·X, fy=10·X·Y; SAE/ASTM ksi->MPa (Gr2 57/74,
        //     Gr5 & A325 92/120, Gr8 & A490 130/150); AISI SS 33 ksi/45 ksi and 50 ksi/65 ksi.
        (MaterialId.Of("steel.g33"),           new(7850.0, 200_000.0, 230.0,  310.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.g50"),           new(7850.0, 200_000.0, 340.0,  450.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-4_6"),  new(7850.0, 200_000.0, 240.0,  400.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-4_8"),  new(7850.0, 200_000.0, 320.0,  400.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-5_6"),  new(7850.0, 200_000.0, 300.0,  500.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-5_8"),  new(7850.0, 200_000.0, 400.0,  500.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-6_8"),  new(7850.0, 200_000.0, 480.0,  600.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-8_8"),  new(7850.0, 200_000.0, 640.0,  800.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-10_9"), new(7850.0, 200_000.0, 900.0,  1000.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-12_9"), new(7850.0, 200_000.0, 1080.0, 1200.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-gr2"),  new(7850.0, 200_000.0, 393.0,  510.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-gr5"),  new(7850.0, 200_000.0, 634.0,  827.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-gr8"),  new(7850.0, 200_000.0, 896.0,  1034.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-a325"), new(7850.0, 200_000.0, 634.0,  827.0,  0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.fastener-a490"), new(7850.0, 200_000.0, 896.0,  1034.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9, NoAcoustic, FireA1, ZSteel)),
        // --- stainless steel (EN 10088; EN 1993-1-4 Table 2.1 0.2%-proof/tensile — outside the carbon Table 3.1
        //     factory, AUTHORED; E 200 GPa; austenitic lambda ~15 / c ~500 / alpha ~16e-6, duplex alpha ~13e-6)
        (MaterialId.Of("steel.1.4301"), new(8000.0, 200_000.0, 210.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.1.4307"), new(8000.0, 200_000.0, 200.0, 500.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.1.4401"), new(8000.0, 200_000.0, 220.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.1.4404"), new(8000.0, 200_000.0, 220.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.1.4571"), new(8000.0, 200_000.0, 220.0, 520.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9, NoAcoustic, FireA1, ZSteel)),
        (MaterialId.Of("steel.1.4462"), new(7800.0, 200_000.0, 460.0, 640.0, 0.30, 13.0e-6, 15.0, 500.0, 1.76, 1e9, NoAcoustic, FireA1, ZSteel)),
        // --- concrete (EN 206; EN 1992-1-1 Table 3.1 dual-class: fck the yield surrogate, fcm = fck+8 the ultimate,
        //     Ecm the PRINTED Table 3.1 value — printed values stay PUBLISHED, never re-derived (the package
        //     EnConcreteFactory secant is the design σ(ε) stiffness, NOT the Table 3.1 mean modulus, so the concrete
        //     mechanical columns stay AUTHORED); rho 2400-2500, nu 0.20, alpha 10e-6, lambda ~1.6-2.3, c 1000; A1.
        //     The id is the EN dual-class token (C12/15 -> concrete.c12_15) the sustainability EPD roster keys.)
        (MaterialId.Of("concrete.c12_15"),  new(2400.0, 27_000.0, 12.0, 20.0, 0.20, 10.0e-6, 1.65, 1000.0, 8.25, 50.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c16_20"),  new(2400.0, 29_000.0, 16.0, 24.0, 0.20, 10.0e-6, 1.80, 1000.0, 9.00, 50.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c20_25"),  new(2400.0, 30_000.0, 20.0, 28.0, 0.20, 10.0e-6, 2.00, 1000.0, 10.0, 50.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c25_30"),  new(2400.0, 31_000.0, 25.0, 33.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 50.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c30_37"),  new(2400.0, 33_000.0, 30.0, 38.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 50.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c35_45"),  new(2450.0, 34_000.0, 35.0, 43.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 80.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c40_50"),  new(2450.0, 35_000.0, 40.0, 48.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 80.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c45_55"),  new(2450.0, 36_000.0, 45.0, 53.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 80.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c50_60"),  new(2450.0, 37_000.0, 50.0, 58.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 90.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c55_67"),  new(2500.0, 38_000.0, 55.0, 63.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 100.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c60_75"),  new(2500.0, 39_000.0, 60.0, 68.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 100.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c70_85"),  new(2500.0, 41_000.0, 70.0, 78.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 120.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c80_95"),  new(2500.0, 42_000.0, 80.0, 88.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 120.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("concrete.c90_105"), new(2500.0, 44_000.0, 90.0, 98.0, 0.20, 10.0e-6, 2.30, 1000.0, 11.5, 130.0, NoAcoustic, FireA1, ZConcrete)),
        // EN 1992-1-1 §11 lightweight aggregate concrete (LC, generic class) — reduced density 1800, eta_E knockdown E ~18 GPa
        (MaterialId.Of("concrete.lc"), new(1800.0, 18_000.0, 30.0, 38.0, 0.20, 8.0e-6, 0.80, 1000.0, 4.00, 50.0, NoAcoustic, FireA1, ZConcrete)),
        // concrete.cmu — the CMU block-concrete substance the Component/cmu#CMU_FAMILY SubstanceId keys (ASTM C90
        // f'm 13.8 MPa / unit 17.2, E_m = 900·f'm TMS 402 ~12.4 GPa, rho 2000 normal weight, lambda ~1.15, mu ~6)
        (MaterialId.Of("concrete.cmu"), new(2000.0, 12_400.0, 13.8, 17.2, 0.20, 8.0e-6, 1.15, 1000.0, 5.75, 6.0, NoAcoustic, FireA1, ZConcrete)),
        // --- structural softwood timber (EN 338:2016 Table 1: fm,k the bending surrogate, E0,mean; nu ~0.4,
        //     lambda EN ISO 10456 0.13, c 1600, mu 50; D-s2,d0)
        (MaterialId.Of("timber.c14"), new(290.0, 7_000.0, 14.0, 23.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c16"), new(310.0, 8_000.0, 16.0, 26.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c18"), new(320.0, 9_000.0, 18.0, 30.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c20"), new(330.0, 9_500.0, 20.0, 33.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c22"), new(340.0, 10_000.0, 22.0, 37.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c24"), new(350.0, 11_000.0, 24.0, 40.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Some((Absorb(0.10, 0.11, 0.10, 0.08, 0.06, 0.06, 0.07, 0.07, 0.08, 0.08, 0.09, 0.09, 0.09, 0.10, 0.10, 0.10, 0.11, 0.11),
                  Sri(14, 16, 18, 20, 22, 24, 26, 27, 29, 31, 33, 34, 36, 38, 40, 41, 42, 43))),
            FireD30, ZTimber)),
        (MaterialId.Of("timber.c27"), new(370.0, 11_500.0, 27.0, 45.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c30"), new(380.0, 12_000.0, 30.0, 50.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c35"), new(400.0, 13_000.0, 35.0, 58.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c40"), new(420.0, 14_000.0, 40.0, 66.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c45"), new(440.0, 15_000.0, 45.0, 75.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.c50"), new(460.0, 16_000.0, 50.0, 83.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0, NoAcoustic, FireD, ZTimber)),
        // --- structural hardwood timber (EN 338:2016 Table 3 — the FULL fourteen-class D-series; denser, lambda 0.17, c 2400)
        (MaterialId.Of("timber.d18"), new(475.0, 9_500.0, 18.0, 30.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d24"), new(485.0, 10_000.0, 24.0, 40.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d27"), new(510.0, 10_500.0, 27.0, 45.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d30"), new(530.0, 11_000.0, 30.0, 50.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d35"), new(540.0, 12_000.0, 35.0, 58.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d40"), new(550.0, 13_000.0, 40.0, 66.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d45"), new(580.0, 13_500.0, 45.0, 75.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d50"), new(620.0, 14_000.0, 50.0, 83.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d55"), new(660.0, 15_500.0, 55.0, 92.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d60"), new(700.0, 17_000.0, 60.0, 100.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d65"), new(750.0, 18_500.0, 65.0, 109.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d70"), new(800.0, 20_000.0, 70.0, 117.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d75"), new(850.0, 22_000.0, 75.0, 125.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("timber.d80"), new(900.0, 24_000.0, 80.0, 134.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0, NoAcoustic, FireD, ZTimber)),
        // wood.oak — the named-hardwood generic alias (a D30-class European white oak the Component/timber family
        // keys when a species rather than a strength class is supplied), absorptive interior-finish vector carried
        (MaterialId.Of("wood.oak"), new(700.0, 11_000.0, 40.0, 90.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Some((Absorb(0.05, 0.06, 0.07, 0.08, 0.10, 0.10, 0.11, 0.10, 0.10, 0.10, 0.10, 0.10, 0.09, 0.09, 0.09, 0.09, 0.09, 0.09),
                  Sri(18, 20, 22, 24, 26, 29, 31, 33, 35, 37, 38, 39, 40, 40, 39, 35, 33, 31))),
            FireD, ZTimber)),
        // --- glued-laminated timber (EN 14080:2013 Table 5 homogeneous + Table 4 combined, the FULL seven-per-layup set)
        (MaterialId.Of("timber.gl20h"), new(340.0, 8_400.0, 20.0, 33.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl22h"), new(370.0, 10_500.0, 22.0, 37.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl24h"), new(385.0, 11_500.0, 24.0, 40.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl26h"), new(405.0, 12_100.0, 26.0, 43.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl28h"), new(425.0, 12_600.0, 28.0, 47.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl30h"), new(440.0, 13_600.0, 30.0, 50.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl32h"), new(440.0, 14_200.0, 32.0, 53.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl20c"), new(355.0, 10_400.0, 20.0, 33.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl22c"), new(355.0, 10_400.0, 22.0, 37.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl24c"), new(365.0, 11_000.0, 24.0, 40.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl26c"), new(385.0, 12_000.0, 26.0, 43.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl28c"), new(390.0, 12_500.0, 28.0, 47.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl30c"), new(390.0, 13_000.0, 30.0, 50.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        (MaterialId.Of("timber.gl32c"), new(400.0, 13_500.0, 32.0, 53.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0, NoAcoustic, FireD30, ZTimber)),
        // --- aluminium (EN 1999-1-1 Table 3.2 wrought alloys; f0 0.2%-proof / fu; E 70 GPa, nu 0.33, c ~900; B-s1,d0;
        //     6082 named first by EN 1999-1-1 as the most common European structural extrusion; EN 755-2 extruded f0/fu)
        (MaterialId.Of("aluminium.6082t6"), new(2710.0, 70_000.0, 260.0, 310.0, 0.33, 23.1e-6, 170.0, 897.0, 18.9, 1e9, NoAcoustic, FireB, ZSteel)),
        (MaterialId.Of("aluminium.6061t6"), new(2700.0, 70_000.0, 240.0, 290.0, 0.33, 23.0e-6, 167.0, 900.0, 18.8, 1e9, NoAcoustic, FireB, ZSteel)),
        (MaterialId.Of("aluminium.6063t5"), new(2700.0, 70_000.0, 130.0, 175.0, 0.33, 23.0e-6, 200.0, 900.0, 18.8, 1e9, NoAcoustic, FireB, ZSteel)),
        (MaterialId.Of("aluminium.5083"),   new(2660.0, 70_000.0, 125.0, 275.0, 0.33, 24.0e-6, 117.0, 900.0, 18.8, 1e9, NoAcoustic, FireB, ZSteel)),
        // --- masonry units (EN 771; fb the normalized compressive strength surrogate; lambda EN ISO 10456; A1;
        //     the WUFI/Fraunhofer sorption anchors carried on the hygrothermally-characterized clay/AAC rows)
        (MaterialId.Of("masonry.clay"), new(1800.0, 7_000.0, 10.0, 20.0, 0.25, 6.0e-6, 0.77, 1000.0, 3.50, 16.0,
            Some((Absorb(0.02, 0.02, 0.03, 0.03, 0.03, 0.04, 0.04, 0.05, 0.05, 0.05, 0.05, 0.06, 0.06, 0.06, 0.07, 0.07, 0.07, 0.07),
                  Sri(30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 53, 54, 55, 56, 57, 58))),
            FireA1, ZConcrete, hygrothermal: Some((0.38, 9.2, 190.0, Some(0.110))))),
        (MaterialId.Of("masonry.calciumsilicate"), new(1800.0, 8_000.0, 12.0, 24.0, 0.25, 8.0e-6, 1.00, 1000.0, 4.55, 15.0, NoAcoustic, FireA1, ZConcrete)),
        (MaterialId.Of("masonry.aac"), new(500.0, 2_000.0, 4.0, 5.0, 0.20, 8.0e-6, 0.13, 1000.0, 0.43, 6.0, NoAcoustic, FireA1, ZConcrete, hygrothermal: Some((0.81, 7.7, 380.0, Some(0.050))))),
        (MaterialId.Of("masonry.aggregate"), new(1400.0, 9_000.0, 7.0, 14.0, 0.20, 8.0e-6, 0.51, 1000.0, 2.55, 6.0, NoAcoustic, FireA1, ZConcrete)),
        // --- dimension stone (EN 771-6; fk the characteristic compressive surrogate; A1, EI 120/120 slab rating)
        (MaterialId.Of("stone.marble"),  new(2700.0, 70_000.0, 15.0, 100.0, 0.25, 7.0e-6, 2.8, 880.0, 3.50, 10_000.0, NoAcoustic, FireA1Ei120, ZConcrete)),
        (MaterialId.Of("stone.granite"), new(2650.0, 60_000.0, 20.0, 130.0, 0.23, 8.0e-6, 3.0, 790.0, 3.40, 10_000.0, NoAcoustic, FireA1Ei120, ZConcrete)),
        // --- glazing (EN 572 soda-lime float + EN 1748-1 borosilicate; fk the characteristic bending strength;
        //     the EN 410 nine-column optical record carried per glass substance — the seam Optical/Energy input;
        //     glass.crown/glass.flint are the Component/glazing#GLAZING_FAMILY pane SubstanceIds)
        (MaterialId.Of("glass.float"), new(2500.0, 70_000.0, 45.0, 50.0, 0.22, 9.0e-6, 1.00, 720.0, 5.88, 1e9,
            Some((Absorb(0.18, 0.10, 0.07, 0.05, 0.04, 0.03, 0.03, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02),
                  Sri(25, 27, 29, 30, 31, 32, 33, 34, 33, 32, 30, 29, 31, 34, 37, 39, 40, 41))),
            FireA1, optical: Some((0.90, 0.08, 0.08, 0.85, 0.075, 0.075, 0.0, 0.837, 0.837)))),
        (MaterialId.Of("glass.crown"), new(2500.0, 70_000.0, 45.0, 50.0, 0.22, 9.0e-6, 1.00, 720.0, 5.88, 1e9, NoAcoustic, FireA1, optical: Some((0.90, 0.08, 0.08, 0.85, 0.075, 0.075, 0.0, 0.837, 0.837)))),
        (MaterialId.Of("glass.flint"), new(2230.0, 63_000.0, 45.0, 50.0, 0.20, 3.3e-6, 1.20, 830.0, 5.88, 1e9, NoAcoustic, FireA1, optical: Some((0.92, 0.07, 0.07, 0.84, 0.07, 0.07, 0.0, 0.837, 0.837)))),
        // --- insulation (EN 13162-13166 + EN ISO 10456 design lambda + EN ISO 13788 mu; mineral wool A1, foams E;
        //     the porous absorbers carry the EN 29053 flow resistivity the Delany-Bazley route reads)
        (MaterialId.Of("insulation.glasswool"), new(40.0, 1.0, 0.001, 0.002, 0.0, 0.0, 0.035, 1030.0, 0.13, 1.0,
            Some((Absorb(0.15, 0.25, 0.40, 0.55, 0.70, 0.80, 0.90, 0.95, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                  Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 16))),
            FireA1, flowResistivity: Some(15_000.0))),
        (MaterialId.Of("insulation.stonewool"), new(45.0, 1.0, 0.001, 0.002, 0.0, 0.0, 0.035, 1030.0, 0.13, 1.0,
            Some((Absorb(0.16, 0.26, 0.42, 0.58, 0.72, 0.82, 0.92, 0.97, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                  Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 16))),
            FireA1, flowResistivity: Some(38_000.0))),
        (MaterialId.Of("insulation.eps"), new(20.0, 5.0, 0.05, 0.10, 0.10, 60.0e-6, 0.036, 1450.0, 0.13, 60.0, NoAcoustic, FireE)),
        (MaterialId.Of("insulation.xps"), new(33.0, 15.0, 0.20, 0.45, 0.10, 70.0e-6, 0.034, 1450.0, 0.12, 150.0, NoAcoustic, FireE)),
        (MaterialId.Of("insulation.pir"), new(32.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.022, 1400.0, 0.08, 60.0, NoAcoustic, FireE)),
        (MaterialId.Of("insulation.pur"), new(35.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.025, 1400.0, 0.08, 60.0, NoAcoustic, FireE)),
        (MaterialId.Of("insulation.phenolic"), new(35.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.020, 1400.0, 0.07, 50.0, NoAcoustic, FireC)),
        (MaterialId.Of("insulation.woodfibre"), new(160.0, 50.0, 0.10, 0.20, 0.10, 8.0e-6, 0.038, 2100.0, 0.13, 5.0,
            Some((Absorb(0.12, 0.20, 0.35, 0.50, 0.65, 0.75, 0.85, 0.90, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95),
                  Sri(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 13, 14, 14, 15, 15, 16, 17))),
            FireE, flowResistivity: Some(100_000.0))),
        // --- gypsum board (EN 520; A2; absorptive interior lining; the WUFI plasterboard sorption anchors carried)
        (MaterialId.Of("gypsum.board"), new(700.0, 2_500.0, 2.0, 4.0, 0.25, 18.0e-6, 0.25, 1000.0, 20.0, 10.0,
            Some((Absorb(0.29, 0.20, 0.12, 0.10, 0.08, 0.06, 0.06, 0.05, 0.04, 0.04, 0.04, 0.04, 0.05, 0.05, 0.06, 0.06, 0.07, 0.07),
                  Sri(15, 16, 17, 18, 20, 22, 24, 26, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38))),
            FireA2, hygrothermal: Some((0.65, 6.3, 400.0, Some(0.287))))),
        // --- sheet-goods board SUBSTANCES (the Component/panel#PANEL_FAMILY PanelKind.SubstanceId keys): ASTM C1325
        //     fibre-cement backer; EN 13986/EN 636 plywood + EN 300 OSB/3 isotropic-surrogate substance physics
        (MaterialId.Of("cement.board"), new(1400.0, 7_000.0, 7.0, 10.0, 0.20, 8.0e-6, 0.19, 900.0, 15.0, 15.0, NoAcoustic, FireA2)),
        (MaterialId.Of("wood.plywood"), new(600.0, 8_000.0, 30.0, 40.0, 0.30, 5.0e-6, 0.13, 1600.0, 10.8, 90.0, NoAcoustic, FireD, ZTimber)),
        (MaterialId.Of("wood.osb"),     new(650.0, 3_500.0, 20.0, 26.0, 0.30, 5.0e-6, 0.13, 1700.0, 11.8, 200.0, NoAcoustic, FireD, ZTimber)),
        // --- roofing/waterproofing membrane (EN 13956; per-area products; high vapour resistance — the membrane IS the vapour control layer)
        (MaterialId.Of("membrane.epdm"), new(1150.0, 5.0, 5.0, 9.0, 0.45, 160.0e-6, 0.25, 1000.0, 12.5, 50_000.0, NoAcoustic, FireE)),
        (MaterialId.Of("membrane.pvc"),  new(1300.0, 15.0, 10.0, 15.0, 0.40, 70.0e-6, 0.16, 1000.0, 8.00, 20_000.0, NoAcoustic, FireE)),
        (MaterialId.Of("membrane.tpo"),  new(920.0, 10.0, 9.0, 14.0, 0.40, 150.0e-6, 0.20, 1000.0, 10.0, 30_000.0, NoAcoustic, FireE)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    // The projector-facing resolution: a registered material lowers to its full engineering set; an UNREGISTERED
    // material rails the seam ElementFault.ValueRejected (band 2500) — the SAME band Admit's columns rail, so a
    // Lookup and the admission it delegates to never split bands. Engineering properties are REQUIRED for a known
    // structural material the Component/capacity#SECTION_CAPACITY / Rasm.Compute design-code routes read, the
    // asymmetric dual of the OPTIONAL Properties/sustainability#SUSTAINABILITY_PROPERTY Lookup (lifecycle data is
    // declared-or-absent, so it returns Fin.Succ(empty)). An app authoring a material with bespoke properties
    // supplies them at the wire and does not route this catalogue.
    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialPropertyRow? row)
            ? Admit(row!, key)
            : ElementFault.ValueRejected(key, $"<unregistered-material-properties:{id.Value}>");

    // The eighteen-band literal-vector helpers — the AcousticBand resolution (100..5000 Hz) the seam Acoustic.Of
    // gates; params ReadOnlySpan<double> collapses the eighteen positional bands to one boundary.
    static ReadOnlyMemory<double> Absorb(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
    static ReadOnlyMemory<double> Sri(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
}
// The case→Discipline map is the seam's own MaterialPropertySet.Discipline accessor (one owner); a consumer reads
// set.Discipline directly — Rasm.Compute selects its analysis route by it, this folder mints no parallel map.
```

## [03]-[ASSESSMENT_INPUT]

- Owner: NONE — the Materials folder authors NO assessment-input marshaller and NO `Assessment` node; the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected seam `Material` node IS the analysis input.
- Cases: zero — there is no input shape to model; `Rasm.Compute` reads the typed `MaterialPropertySet` cases off the graph and dispatches on `set.Discipline`, so a per-discipline input bag is the deleted form.
- Entry: `Rasm.Compute` reads the `Material` node plies DIRECTLY above the seam (`id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the relocated multi-ply `AssemblyAggregator` + the ISO/EN closed-form routes + the VividOrange/FE structural solvers), and writes the seam `Assessment` `Result` node back content-keyed on `(input key, route)`; the case→`Discipline` map is the seam's own `MaterialPropertySet.Discipline` accessor (`Mechanical`/`Orthotropic`→`Structural`, `Thermal`→`Thermal`, `Acoustic`→`Acoustic`, `Fire`→`Fire`, `Environmental`→`Environmental`, `Cost`→`Cost`, `Damping`→`Dynamic`, `Hygrothermal`→`Hygrothermal`, `Durability`→`Durability`, `Optical`→`Energy`), so Compute selects its route by `set.Discipline` with no parallel Materials marshaller.
- Boundary: the migration `MaterialAssessmentInput` marshaller is RETIRED — a Materials-authored typed-input bag is redundant with Compute reading the typed `MaterialPropertySet` cases off the graph, so the seam carries ONE property surface (the `Material` node), not a parallel input node; the seam `Acoustic` case's intrinsic `Nrc`/`StcWeighted`/`SoundReductionIndexDb` folds (`Composition/acoustic`) are the single-material ratings Compute's ISO 12354 layered fold reads through the SAME `RatingContour.Fit` contour kernel, so the assembly STC and the material STC share one contour owner; the multi-ply aggregation is `Rasm.Compute`'s, this folder retaining only the single-material property SOURCE and crossing to Compute solely through the seam graph.

## [04]-[RESEARCH]

- [SEAM_OWNS_PROPERTY_UNION]: the typed engineering-property family is the seam `MaterialPropertySet` class-root `[Union]` + `[Equatable]` — ELEVEN cases (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`/`Damping`/`Hygrothermal`/`Durability`/`Optical`), keyed to the seam `Discipline` — so the migration Materials `MaterialProperty` `[Union]`, its `MaterialPropertyKind` coercion enum, and the inline acoustic folds are RETIRED, ROUTED into the seam. SEAM CONTRACT (Rasm.Element side; this folder consumes): the `MeasureValue`-overloaded `OfMechanical(density, youngs, yield, ultimate, poissons, thermalExpansion, key, evidence = default)` / `OfThermal(conductivity, specificHeat, uValue, vapourResistanceFactor, key, evidence = default)` are VALIDATION_MONOID admissions (each independent column one `Validation<Error,_>` slot, every miss reported in one `Fin.Fail`); `OfAcoustic(Acoustic, evidence)` / `OfFire(FireRating, SmokeClass, DropletClass, FireResistance, evidence)` are TOTAL over admitted carriers; `OfDamping(dampingRatio, rayleigh, key, evidence)` guards ζ `[0,1)`; `OfHygrothermal(porosity, waterContent80Rh, freeWaterSaturation, waterAbsorption, key, evidence, sorptionIsotherm, liquidTransport, moistureConductivity)` binds its `wf >= w80` isotherm refinement AFTER the accumulated leaves, mints the anchors as `MoistureStorage`-typed kg/m³ measures internally, and carries three optional pre-admitted `SampledCurve` measured functions (defaulting `None` — this catalogue's two-point rows call the anchor form unchanged; a WUFI-grade row supplies the curves and the seam enforces curve↔anchor agreement); `OfOptical(nine [0,1] fractions, key, evidence)` accumulates six per-band-per-side `τ+ρ <= 1` conservation refinements as a second stage; `Acoustic.Of(absorption, sri, key, dynamicStiffness = default, flowResistivity = default, lossFactor = default)` gates the eighteen-band vectors AND the three optional intrinsics; `FireRating.Parse` rails the reaction token, `SmokeClass`/`DropletClass` expose NO `Parse` (the generated `TryGet` resolves, `NotSpecified` the empty-token row), and `FireResistance.Of(loadBearing, integrity, insulation, key)` accumulates the independent non-negative R/E/I minutes before the total `OfFire`. Provider uncertainty models stay HERE; only neutral `MeasureValue`/`MeasureBand` + `PropertyEvidence` cross. The timber rows lower the isotropic `Mechanical` (substance SOURCE); the directional `Orthotropic` lowering (`OfOrthotropic` over the EN-grade `E0`/`E90`/`G`/`fc0k`/`fc90k`) is `Component/timber#TIMBER_FAMILY`'s concern.
- [SHARED_PUBLISHED_INGRESS]: `Published<T>` is the ONE ingress carrier BOTH Properties owners ride — declared HERE, composed by `Properties/sustainability#SUSTAINABILITY_PROPERTY` (its `ScalarDatum`/`SustainabilityUncertainty`/`CostDatum` scaffolds collapsed onto it). One generic record over `VividOrange.Uncertainties` `IUncertainty<T>` with the two `Of` carrier arms (`IQuantity` via `Quantities.Utility.WithRelativeUncertainty`, `double` via the double `.Utility`), the `Vector`/`Centrals` banded-series duals, the `Kind` discriminant over the four kind interfaces, and the `Band` provider-model→seam-`MeasureBand` lowering (`Normal` bands carry σ and coverage factor; `Absolute`/`Relative`/`Interval` lower to corner intervals). The per-band `ScalarDatum` wrapping of the acoustic vectors is DELETED as unread ceremony — only centrals cross the seam vector gates, so the vectors ride raw `ReadOnlyMemory<double>` with ONE group evidence. The `0.2.0` `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` on the uncertainty types is a DISTINCT contract from the `0.1.0` `VividOrange.Serialization` floor — neither serializes the other's types, and no uncertainty value routes a VividOrange serializer (the canonical Rasm codec owns the wire).
- [DELEGATED_MECHANICAL_SOURCE]: `SEED_ROW_LAW` executed — `MechanicalSource` is the closed delegation axis: `EnSteel(EnSteelGrade, EnSteelDeliveryCondition)` builds `new EnSteelMaterial(grade, NationalAnnex.RecommendedValues)`, routes `material.Specification.DeliveryCondition = delivery` (the `Component/steel#STEEL_FAMILY` `YieldMpa` precedent), and reads `EnSteelFactory.CreateBiLinear(material, Length.FromMillimeters(16))` — the decompile-verified EN 1993-1-1 Table 3.1 tables (AR holds S235 235/360, S275 275/430, S355 355/490, S450 440/550; N holds S420 420/520, S460 460/540 — byte-identical to the retired hand values), `IBiLinearMaterial.ElasticModulus` the factory's 210 GPa law, the `<=40 mm` band selected; `EnRebar(EnRebarGrade)` reads `EnRebarFactory.CreateBiLinear` (`f_yk` parsed from the grade digits, `f_u = k·f_yk` by ductility class A 1.05 / B 1.08 / C 1.15, `E_s` 200 GPa — B500A 525 / B500B 540 / B500C 575, byte-identical; the decompiled `EnRebarGrade` roster is SIX members — B450A/B450C/B500A/B500B/B500C/B550B — and the catalogue delegates it whole). The decompiled `EnSteelDeliveryCondition` axis is `{AR, N, M, Q}` (EN 10025-2/-3/-4/-6) — the weathering `W` table inside the factory is UNROUTED dead data (`GetTable3_1Properties` switches only on `DeliveryCondition` × `HollowSection` and no branch returns it), never a delivery member — the Q sub-table holds only S460 (460/570), `EnSteelGrade` topping out at S460, so EN 10025-6 S690 has no producer and stays AUTHORED; the spec defaults (`HollowSection = false`, hot-formed) pin the non-hollow tables so the factory's hollow/cold-formed throws are unreachable from this build. The factories are exception-throwing at their boundary (`ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException`) — `Strength` traps them ONCE via the kernel `Op.Catch` funnel onto `ElementFault.ValueRejected` (BOUNDARY_EXCEPTION_LAW; the throw never reaches an interior signature). Table 3.1 strengths are annex-independent, so the pinned `RecommendedValues` annex only satisfies construction. NO producer exists for ASTM/CSA rebar, AISC A36/A992/A572, EN 10025-6 S690, EN 10088 stainless (`EnSteelGrade` is the carbon EN 10025 set), or any non-steel family — those stay AUTHORED ([FLOOR_SCOPE_GATE]: the `AS3600`/`ACI318`/`AASHTO` floor enums have no producer in the EN-only assembly). CONCRETE stays AUTHORED deliberately: `EnConcreteFactory.CreateLinearElastic` derives the design secant `fck/ε` — a DIFFERENT physical quantity from the printed EN 1992-1-1 Table 3.1 mean `Ecm` the `YoungsModulus` column carries — so delegating it would silently swap moduli; the printed `Ecm`/`fck`/`fcm` values stay PUBLISHED per the column-provenance law.
- [CATALOGUE_DOMAIN_COVERAGE]: the roster is the authoritative EN/ASTM/CSA grade set — EN 10025 carbon steel (`s235`–`s460` + `s450` DELEGATED, `s690` authored, the `metal.steel` S235-baseline alias, `metal.iron` ductile casting), AISC `steel.a36`/`a992`/`a572` (the `Component/steel#STEEL_FAMILY` `SteelGrade.SubstanceId` rows), reinforcing steel (the full six-member EN 10080 `EnRebarGrade` set `b450a`/`b450c`/`b500a/b/c`/`b550b` DELEGATED; ASTM A615 `gr40`–`gr80`, A706 `gr60w`/`gr80w`, CSA G30.18 `400w`/`500w` authored at the ACI 318 §20.2.2.2 `E_s` 200 GPa), EN 10088 stainless (five austenitic + `1.4462` duplex per EN 1993-1-4 Table 2.1), EN 1992/EN 206 concrete (`c12_15`–`c90_105` + `lc` + the `concrete.cmu` ASTM C90/TMS 402 block substance the `Component/cmu#CMU_FAMILY` keys), EN 338:2016 timber (twelve C-classes + the FULL fourteen D-classes + `wood.oak`), EN 14080:2013 glulam (the full seven-per-layup h/c sets), EN 1999-1-1 aluminium (four alloys), EN 771 masonry + EN 771-6 stone, glazing (`glass.float` + the `Component/glazing#GLAZING_FAMILY` pane substances `glass.crown` EN 572 / `glass.flint` EN 1748-1 borosilicate — the low-expansion `3.3e-6` fire-glass signature), EN 13162-13166 insulation, EN 520 gypsum, the sheet-goods board substances (`cement.board`/`wood.plywood`/`wood.osb`), and EN 13956 membranes. DUAL-KEYING: every row keys the SAME canonical `MaterialId` the `Properties/sustainability#SUSTAINABILITY_PROPERTY` EPD roster keys in EXACT parity (FULL_ROSTER — no engineering-only orphan; the `Projection/component#COMPONENT_SUBGRAPH` `Capture` join composes both). The REQUIRED-vs-OPTIONAL asymmetry is TYPE-LEVEL: `MaterialPropertyCatalogue.Lookup` faults on an unregistered id, `SustainabilityCatalogue.Lookup` returns empty. Substance-id closure is a HARD invariant: every `Component.SubstanceId` a seed page mints (`steel.md` A36/A992/A572/S450, `cmu.md` `concrete.cmu`, `glazing.md` `glass.crown`/`glass.flint`) resolves a row here — a seed-keyed id with no row is a projection-time fault, so a NEW seed substance lands with its row in the same campaign; the glazing `gas.cavity` ply id and the `polymer.adhesive` appearance id are NOT substance keys (plies and render rows never route this catalogue).
- [DISCIPLINE_SOURCE_COVERAGE]: the seam's four grown cases and their sources — `Damping` SOURCED HERE (the EN 1998-1 §3 / ISO 10137 design ζ on every structural family: welded steel + aluminium 0.02, RC/masonry/stone 0.05, timber 0.08 — the response-spectrum and footfall route input; the Rayleigh `(α, β)` pair stays a per-model FE calibration, never a catalogue datum), `Hygrothermal` SOURCED HERE for the WUFI/Fraunhofer-characterized porous rows (clay brick 0.38/9.2/190/A 0.110, AAC 0.81/7.7/380/A 0.050, plasterboard 0.65/6.3/400/A 0.287 — the EN 15026 transient inputs; a row without published sorption data carries None, never an invented isotherm), `Optical` SOURCED HERE for the glass substances (the EN 410 clear-float record τv 0.90 / ρv 0.08 / τe 0.85 / ρe 0.075 / τIR 0 / ε 0.837; borosilicate τv 0.92), `Durability` NOT YET SOURCED — the fib Model Code `D_RCM`/carbonation-K reference tables are mix-parameterized (w/c, cement type), not strength-class-printed, so a per-class row value would violate the PUBLISHED transcription-fidelity law; the landing is FIXED (one `Option<(double K, double Drcm, double Alpha)>` row column + one `OfDurability` Admit arm, the R1-pattern data-source watch), and until a mix-keyed source is admitted the catalogue lowers no `Durability` case.
- [SEAM_MEASURE_COERCION]: the engineering-property SI carrier is the seam `MeasureValue(QuantityType, Dimension, double Si, string CanonicalUnit, Option<MeasureBand>)`; the typed mints route through the `Component/component#COMPONENT_OWNER` `QuantityRow` `[SmartEnum<string>]` rows — `Density`/`Pressure`/`ThermalConductivity`/`SpecificEntropy`/`HeatTransferCoefficient`, each carrying the byte-identical `QuantityType`/`Dimension`/scale/unit the retired inline `(QuantityType.Create(…), Dimension.Create(…), unit)` triples spelled — so a Properties-minted and a `SeamSection`-minted measure share ONE mint vocabulary and `MeasureValue` content keys are unchanged. The `Published<T>.Band` lowering is the ONE provider-model→`MeasureBand` bridge (`UnitsNet` quantity values and VividOrange uncertainty models never cross the seam). UnitsNet v5 removed the `QuantityType` enum, so the seam `QuantityType` `[ValueObject<string>]` is the string discriminator and `Dimension` the 7-vector physical signature. The photometric/appearance luminous coercion stays on `Appearance/photometric#PHOTOMETRIC`.
- [SEAM_OWNS_ACOUSTIC_FOLDS]: the intrinsic single-material acoustic folds — `Nrc` (ASTM C423), `Saa`, `StcWeighted` (ASTM E413), `Rw` (ISO 717-1) over the shared `RatingContour.Fit` kernel — live on the seam `Acoustic` case (`Rasm.Element` `Composition/acoustic`), which now ALSO forwards the three optional intrinsic constants (`DynamicStiffnessMNPerM3` EN 29052-1, `FlowResistivityPaSPerM2` EN 29053, `LossFactor`) this catalogue's `AcousticDatum` supplies for the characterized porous rows (mineral wools, wood-fibre); this owner READS the folds and never re-authors them. The seam `Acoustic.Of` band-arity gate (eighteen one-third-octave `AcousticBand` centres, absorption `[0,1]`, SRI finite, intrinsics positive-when-Some) is the one admission. All THREE intrinsic slots are roster-suppliable (`dynamicStiffness`/`flowResistivity`/`lossFactor` ride the row ctor's trailing optionals — the ingress record and the admission ctor carry the SAME surface); today only the flow-resistivity column is published substance-level, the EN 29052-1 `s'` being thickness-specific — a resilient-layer row supplies it the day a thickness-keyed source is admitted, never an invented figure.
- [ASSEMBLY_AGGREGATOR_RELOCATED]: the multi-ply `AssemblyAggregator` (the ISO 6946 series-resistance U-value, ISO 12354 layered-STC, rule-of-mixtures, worst-ply fire envelope, and lifecycle GWP/cost folds) is RELOCATED to `Rasm.Compute` — the seam carries the per-material `MaterialPropertySet` cases on the `Material` node, Compute folds the element's `MaterialComposition` plies and writes the `Assessment` `Result` node back content-keyed on `(input key, route)`. The single-layer `UValueWM2K` column is a per-material reference datum the relocated ISO 6946 fold supersedes. Ripple counterpart: `Rasm.Compute` `Analysis/aggregator` + `Analysis/assessment`.
- [IFC_MATERIAL_PROPERTIES]: IFC 4.3 `IfcMaterialProperties` extends `IfcExtendedProperties`; the standard `Pset_MaterialMechanical`/`Pset_MaterialThermal`/`Pset_MaterialCommon`/`Pset_MaterialOptical`/`Pset_MaterialHygroscopic` carry the canonical member names (`MassDensity`/`YoungModulus`/`PoissonRatio`/`ThermalConductivity`/`SpecificHeatCapacity`/`VisibleTransmittance`/`SolarTransmittance`/`MoistureContent`), and the seam cases map one-to-one — the `Optical` case is exactly the `Pset_MaterialOptical` column set, the `Hygrothermal` anchors feed `Pset_MaterialHygroscopic`. `Rasm.Bim` reads the projected seam `Material` node and emits the Psets from the seam graph — no Materials→IFC carrier; the Pset member-name mapping is `Rasm.Bim`'s side, the property computation this side, the seam the alignment.
