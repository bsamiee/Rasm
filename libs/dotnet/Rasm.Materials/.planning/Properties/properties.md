# [MATERIALS_PROPERTIES]

THE TYPED-ENGINEERING-PROPERTY SOURCE. One `MaterialPropertyCatalogue` keys published mechanical, thermal, acoustic, fire, damping, hygrothermal, optical, and electrical data per `MaterialId`, and one `Admit` lowers a published row into the contract's typed cases. Engineering properties are never a per-discipline material type — one `Seq<MaterialPropertySet>` over one `MaterialId` carries conductivity, sound spectrum, fire rating, structural grade, damping ratio, sorption anchors, glazing optics, and conductor resistivity together. This page is the SOURCE and the contract the CARRIER: it re-mints no contract type, authors no aggregation, and crosses to `Rasm.Compute`/`Rasm.Bim` only through the contract graph. Multi-ply assembly aggregation is `Rasm.Compute`'s; a substance-level transmittance and a product-level resilient-layer stiffness are neither the catalogue's nor representable in it.

The typed family is contract-owned — the `Rasm.Element` `MaterialPropertySet` class-root `[Union]` keyed to `Discipline`, `MeasureValue` the SI-coerced dimensional column, the acoustic folds on the contract `Acoustic` case. This page DECLARES `Published<T>`, the shared uncertainty ingress carrier over `VividOrange.Uncertainties` that `Properties/assessment#ASSESSMENT_RECORD` COMPOSES. Every dimensional mint passes through the `Component/component#COMPONENT_OWNER` `QuantityRow` rows, and the EN-vendored mechanical columns DELEGATE per `SEED_ROW_LAW`: the `steel.s235`–`steel.s460` (+`steel.s450`, `metal.steel`) triples resolve through `EnSteelFactory.CreateBiLinear` over `EnSteelMaterial` × `EnSteelDeliveryCondition`, the six-member `EnRebarGrade` roster through `EnRebarFactory.CreateBiLinear`, each documented factory refusal trapped ONCE as cause-bearing `ComponentFault.GradeDerivation` while unknown throws remain exact. Hand rows keep the non-vendor columns alone and the no-EN-producer grades stay AUTHORED verbatim. The lifecycle `Environmental`/`Cost` cases lower from `Properties/sustainability#SUSTAINABILITY_PROPERTY`, the directional `Orthotropic` from `Component/timber#TIMBER_FAMILY`, and `Lookup` is the resolution `Projection/component#COMPONENT_PROJECTOR` calls. Returned invalid values fail `ElementFault.ValueRejected` (2500); provider refusals retain their own component identity, never becoming the appearance `MaterialFault` 2450 of another concern.

## [01]-[INDEX]

- [02]-[MATERIAL_PROPERTY_CATALOGUE]: the shared `Published<T>` ingress carrier, the `VapourResistance` permeability class, the `MechanicalSource` vendor-delegation axis with its `NailWireClass` bending-yield band table, the `SubstancePhysics` shared family anchor, the `MaterialPropertyRow` ingress record, the registered-row database, the `Admit` row→contract-case lowering, and the memoized `Lookup` the projector calls.
- [03]-[DURABILITY_MIX]: the mix-keyed fib chloride-migration and ageing table, keyed on cement type × w/c because no strength class determines either, and the `Durability` resolution a project composes with its own exposure-class carbonation rate.
- [04]-[MIX_PROPORTION]: the EN 206 Annex F exposure-floor axis, the ACI 211.1-91 SI proportioning tables, and the one `MixDesign.Proportion` absolute-volume fold over a caller-declared `MixSpec`.
- [05]-[ASSESSMENT_INPUT]: why Materials authors NO assessment-input node — the material's `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the input `Rasm.Compute` reads off the graph directly.

## [02]-[MATERIAL_PROPERTY_CATALOGUE]

- Owner: `Published<T>` the shared evidence-bearing uncertainty carrier (private-minted — the `Of`/`Exact`/`Interval` triad is the only way in); `VapourResistance` the closed permeability class; `MechanicalSource` the closed mechanical-column source axis (`Authored` / `NailWire` / `EnSteel` / `EnRebar`); `NailWireClass` the ASTM F1667-S1 bending-yield band table; `SubstancePhysics` the shared family anchor a roster row references; `MaterialPropertyRow` the published-data ingress record; `MaterialPropertyCatalogue` the registered-row database; `Admit` the row→contract-case lowering; `Lookup` the projector-facing resolution.
- Cases: one `MaterialPropertyRow` shape across all materials — a `SubstancePhysics` anchor (density, Poisson, expansion, conductivity, specific heat, vapour class, fire declaration, design damping) with the row's own `MechanicalSource`, and the optional acoustic, hygrothermal, optical, and electrical declarations only a characterized substance carries; `Admit` produces a `Seq<MaterialPropertySet>` of the contract `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Damping`/`Hygrothermal`/`Optical`/`Electrical` cases — each over a `MaterialId`, never a property subtype.
- Law: AUTHORED transcriptions carry the catalogue's relative band and code-REGISTERED values cross EXACT (folder `RULINGS [02]`) — `Published.Of` admits through the `VividOrange` relative factory, `Published.Exact` mints the zero-width datum, and the contract's EXACTNESS-IS-BAND-ABSENCE rule then answers `None` at the `Measure` mint rather than offering `MeasureBand.Admit` a zero-width band that refuses its own re-admission. A DELEGATED vendor value never wears a fabricated spread. The carrier's constructor is CLOSED so no consumer can pair an `Exact` kind with a spread carrier, or a `Normal` kind with an absent distribution the `Band` lowering then cannot read.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row)` — resolves the `MechanicalSource` (the stored MPa triple, the F1667-S1 band read, or the vendor build whose documented grade refusal is classified with its cause by the kernel `Try.lift` funnel), mints every dimensional column through the `QuantityRow` typed-mint rows with the `Published<T>.Band` provider-model→`MeasureBand` lowering, passes the scalar columns central-only (the contract guards Poisson `[0,0.5]`, μ `>= 1`, ζ `[0,1)`, εr `>= 1`, the isotherm `wf >= w80`, the optical conservation refinements), folds the acoustic declaration through the contract `Acoustic.Of` gate, the electrical declaration through the contract `OfElectrical` raw arity, and the fire declaration through `FactoryBridge.Row<string, FireRating>(reaction)` + the ONE contract `EuroclassSuffix` `[ObjectFactory<string>]` grammar (`FactoryBridge.Accept<EuroclassSuffix>(text)`) + the three-criterion `FireResistance.Of`. Only the `Strength` resolution BINDS; the eight discipline groups and the three fire columns are INDEPENDENT and ACCUMULATE applicatively, so a row with several rejected columns faults them ALL in one `Fin.Fail` `ManyErrors`. `Lookup(MaterialId id)` reads the memoized admitted catalogue and faults for an unregistered material — one polymorphic resolution, never a `GetMechanical`/`GetThermal` family.
- Packages: Rasm.Element (project — `MaterialPropertySet` + its `Of*` admissions, `MeasureValue.OfSi`/`WithUncertainty`, `MeasureBand.Admit`, `UncertaintyKind`, `PropertyEvidence.Of`, `EvidenceGrade`, `FireRating` through kernel `FactoryBridge.Row`, `EuroclassSuffix` (the `[ObjectFactory<string>]` grammar via the kernel `FactoryBridge.Accept` arm), `FireResistance.Of`, `Acoustic.Of`, `Discipline`, `MaterialId`, `ElementFault.ValueRejected`), Rasm.Materials.Component (project — `QuantityRow.OfNative`, `ComponentFault.GradeDerivation`, and the shared `EnGrade.GradeRefusal` classifier), VividOrange.Uncertainties + VividOrange.Uncertainties.Quantities (the four uncertainty models over the `double` and `IQuantity` carriers, the fluent `WithRelativeUncertainty`/`WithAbsoluteUncertainty`/`WithIntervalUncertainty` admissions, the `IntervalUncertaintyQuantity<TQuantity>` carrier the dimensional interval arm mints directly, the `IUncertainty<T>` kind interfaces), VividOrange.Materials (`EnSteelFactory`/`EnRebarFactory`/`EnSteelMaterial`/`EnSteelGrade`/`EnRebarGrade`/`EnSteelDeliveryCondition`/`IBiLinearMaterial`), VividOrange.Standards (`NationalAnnex`), UnitsNet (`Density`/`Pressure`/`ThermalConductivity`/`SpecificEntropy`/`Length` — raw-to-SI coercion at this boundary only), NodaTime (`LocalDate` evidence expiry), Rasm (project — the `Try.lift` trap funnel), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option` + `Match`/`Map`), BCL inbox (`FrozenDictionary`, `Lazy<T>`, `ReadOnlyMemory<double>`, `ImmutableArray<T>`).
- Growth: a new engineering property shared across materials is one column on the matching contract case the row gains a published column for and `Admit` lowers; a new known material is one `Rows` entry naming its `SubstancePhysics` anchor (the roster grows by row to thousands with no contract touch, and a corrected family figure is one anchor edit rather than a hundred-row sweep); a new vendor grade table or published-yield convention is one `MechanicalSource` case and one `Strength` arm, compiler-forced at the generated `Switch`; a new nail-wire class or diameter band is one `NailWireClass` row or one band entry; a new property discipline is one contract case — the `Damping`/`Hygrothermal`/`Optical`/`Electrical` cases landed exactly this way and this catalogue sources four of them.
- Boundary: `MaterialPropertyRow` is the published-DATA ingress, NOT a parallel domain union — `Admit` is the one `BOUNDARY_ADMISSION`, so the row and every declaration beside it stay `internal` and the public surface is `Admit`/`Lookup` alone. The dimensional columns coerce to SI through `UnitsNet` reads inside the `QuantityRow`-typed mint, the provider uncertainty models lower to neutral `MeasureBand` bounds at exactly that mint, and provider types never cross into `Rasm.Element`. A SUBSTANCE HAS NO TRANSMITTANCE: the contract `Thermal` U-value column takes this mint's conductance at UNIT thickness — numerically λ, carrying no thickness at all — and the EN ISO 6946 assembly fold in `Rasm.Compute` owns every real U-value over a declared buildup. A SUBSTANCE HAS NO DYNAMIC STIFFNESS: EN 29052-1 `s'` is an installed-assembly quantity measured to differ across thicknesses of one declared product, so the contract's optional slot stays absent from every roster row. A SUBSTANCE CARRIES NO AMPACITY: the NEC 310.16 / IEC 60364-5-52 tables key on insulation temperature rating, installation method, and conductor grouping — component and assembly facts the electrical detail rows own. The vendor factories' documented `ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException` failures become cause-bearing `GradeDerivation`; unknown throws remain exact and returned invalid values use the contract-owned refusal. No uncertainty value routes a VividOrange serializer, the canonical Rasm codec owning every wire; the `Optical`/`Hygrothermal` column sets align at the CONTRACT with the IFC material property sets `Rasm.Bim` emits, so neither side transcribes the other's member names. SUBSTANCE-ID CLOSURE is a hard invariant: every `Component.SubstanceId` a seed page mints resolves a row here — a seed-keyed id with no row is a projection-time `Lookup` fault — so a new seed substance lands with its row in the same campaign; a ply-cavity, stud-appearance, or adhesive-appearance id is NOT a substance key and never routes this catalogue.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Materials.Component;
using Thinktecture;
using UnitsNet;
using VividOrange.Materials;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Standards.Eurocode;
using VividOrange.Uncertainties;
using VividOrange.Uncertainties.Quantities;
using VividOrange.Uncertainties.Quantities.Utility;
using VividOrange.Uncertainties.Utility;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record VapourResistance {
    public sealed record Impermeable : VapourResistance;
    public sealed record Factor(double Mu) : VapourResistance;
}

[Union]
public abstract partial record MechanicalSource {
    public sealed record Authored(double YoungsMpa, double YieldMpa, double UltimateMpa) : MechanicalSource;
    public sealed record NailWire(NailWireClass Class, double ShankDiameterIn, double UltimateMpa) : MechanicalSource;
    public sealed record EnSteel(EnSteelGrade Grade, EnSteelDeliveryCondition Delivery) : MechanicalSource;
    public sealed record EnRebar(EnRebarGrade Grade) : MechanicalSource;

    public static MechanicalSource Mpa(double youngs, double yieldStrength, double ultimate) =>
        new Authored(youngs, yieldStrength, ultimate);
}

// --- [NAIL_WIRE_YIELD]
public sealed record NailWireBand(double MinDiameterIn, double MaxDiameterIn, double FybKsi, double FybMpa);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NailWireClass {
    public static readonly NailWireClass LowCarbon = new("low-carbon", [
        new(0.099, 0.142, 100.0, 689.0),
        new(0.142, 0.177,  90.0, 620.0),
        new(0.177, 0.236,  80.0, 552.0),
        new(0.236, 0.273,  70.0, 483.0),
        new(0.273, 0.344,  60.0, 414.0),
        new(0.344, 0.375,  45.0, 310.0),
    ]);

    public static readonly NailWireClass Hardened = new("hardened", [
        new(0.120, 0.142, 130.0, 896.0),
        new(0.142, 0.192, 115.0, 793.0),
        new(0.192, 0.207, 100.0, 689.0),
    ]);

    public ImmutableArray<NailWireBand> Bands { get; }

    public Option<NailWireBand> At(double shankDiameterIn) =>
        Optional(Bands.FirstOrDefault(band => shankDiameterIn >= band.MinDiameterIn
            && (shankDiameterIn < band.MaxDiameterIn || shankDiameterIn == Bands[^1].MaxDiameterIn)));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Published<T> {
    internal Published(IUncertainty<T> value, UncertaintyKind kind, Option<INormalDistributionUncertainty<T>> normal, PropertyEvidence evidence) =>
        (Value, Kind, Normal, Evidence) = (value, kind, normal, evidence);

    public IUncertainty<T> Value { get; }
    public UncertaintyKind Kind { get; }
    public Option<INormalDistributionUncertainty<T>> Normal { get; }
    public PropertyEvidence Evidence { get; }

    public Fin<MeasureBand> Band(Func<T, double> si, double scale) =>
        MeasureBand.Admit(
            Kind,
            si(Value.LowerBound) * scale,
            si(Value.UpperBound) * scale,
            Normal.Map(normal => si(normal.StandardDeviation) * scale),
            Normal.Map(static normal => normal.CoverageFactor));
}

public static class Published {
    public static Published<TQuantity> Of<TQuantity>(TQuantity value, double relative, PropertyEvidence evidence) where TQuantity : IQuantity =>
        new(value.WithRelativeUncertainty(relative), UncertaintyKind.Relative, None, evidence.Normalized());

    public static Published<double> Of(double value, double relative, PropertyEvidence evidence) =>
        new(value.WithRelativeUncertainty(relative), UncertaintyKind.Relative, None, evidence.Normalized());

    public static Published<TQuantity> Exact<TQuantity>(TQuantity value, PropertyEvidence evidence) where TQuantity : IQuantity =>
        new(value.WithAbsoluteUncertainty(0.0), UncertaintyKind.Exact, None, evidence.Normalized());

    public static Published<double> Exact(double value, PropertyEvidence evidence) =>
        new(value.WithAbsoluteUncertainty(0.0), UncertaintyKind.Exact, None, evidence.Normalized());

    public static Published<TQuantity> Interval<TQuantity>(TQuantity central, TQuantity lower, TQuantity upper, PropertyEvidence evidence) where TQuantity : IQuantity =>
        new(new IntervalUncertaintyQuantity<TQuantity>(central, lower, upper), UncertaintyKind.Interval, None, evidence.Normalized());

    public static Published<double> Interval(double central, double lower, double upper, PropertyEvidence evidence) =>
        new(central.WithIntervalUncertainty(lower, upper), UncertaintyKind.Interval, None, evidence.Normalized());

    extension(Published<double> datum) {
        public double Central => datum.Value.CentralValue;
    }

    extension<TQuantity>(Published<TQuantity> datum) where TQuantity : IQuantity {
        public TQuantity Central => datum.Value.CentralValue;
    }
}

internal readonly record struct SubstancePhysics(
    double DensityKgM3,
    double PoissonsRatio,
    double ExpansionPerK,
    double ConductivityWMK,
    double SpecificHeatJKgK,
    VapourResistance Vapour,
    Option<FireDeclaration> Fire,
    Option<double> DampingRatio);

internal readonly record struct FireDeclaration(
    string Reaction,
    string Suffix,
    Option<int> LoadBearingMinutes = default,
    Option<int> IntegrityMinutes = default,
    Option<int> InsulationMinutes = default);

internal readonly record struct AcousticDeclaration(
    ReadOnlyMemory<double> Absorption,
    ReadOnlyMemory<double> Sri,
    Option<double> FlowResistivityPaSPerM2 = default,
    Option<double> LossFactor = default);

internal readonly record struct HygrothermalDeclaration(
    double Porosity,
    double W80KgM3,
    double WfKgM3,
    Option<double> AValueKgM2SqrtS = default);

internal readonly record struct OpticalDeclaration(
    double VisibleT, double VisibleRf, double VisibleRb,
    double SolarT, double SolarRf, double SolarRb,
    double IrT, double IrEf, double IrEb);

internal readonly record struct ElectricalDeclaration(
    double ResistivityOhmM,
    double RelativePermittivity,
    Option<double> DielectricStrengthVPerM = default,
    Option<double> MagneticPermeabilityRelative = default);

internal sealed record MaterialPropertyRow(
    SubstancePhysics Physics,
    MechanicalSource Mechanical,
    Option<AcousticDeclaration> Acoustic = default,
    Option<HygrothermalDeclaration> Hygrothermal = default,
    Option<OpticalDeclaration> Optical = default,
    Option<ElectricalDeclaration> Electrical = default,
    PropertyEvidence Evidence = default);

internal readonly record struct StrengthTriple(
    Published<Pressure> Youngs,
    Published<Pressure> Yield,
    Published<Pressure> Ultimate,
    PropertyEvidence Evidence);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialPropertyCatalogue {
    const double GradeThicknessMm = 16.0;

    const double AuthoredBand = 0.05;

    static readonly PropertyEvidence SteelTable =
        PropertyEvidence.Of("vendor", EvidenceGrade.Import, Some("en 1993-1-1 table 3.1 / vividorange.materials"));
    static readonly PropertyEvidence RebarTable =
        PropertyEvidence.Of("vendor", EvidenceGrade.Import, Some("en 1992-1-1 §3.2 + en 10080 / vividorange.materials"));
    static readonly PropertyEvidence NailWireTable =
        PropertyEvidence.Of("vendor", EvidenceGrade.Import, Some("astm f1667 s1 / astm f1575 via nds 12.3.1b"));

    internal static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row) =>
        Strength(row.Mechanical).Bind(strength =>
            (Mechanical(row, strength).ToValidation(),
             Thermal(row).ToValidation(),
             row.Acoustic.Traverse(a =>
                 Acoustic.Of(a.Absorption, a.Sri, flowResistivity: a.FlowResistivityPaSPerM2, lossFactor: a.LossFactor)
                     .Map(spectrum => Seq(MaterialPropertySet.OfAcoustic(spectrum, row.Evidence))).ToValidation()).As()
                 .Map(static groups => groups.IfNone(Seq<MaterialPropertySet>())),
             row.Physics.Fire.Traverse(f =>
                 (FactoryBridge.Row<string, FireRating>(f.Reaction).ToValidation(),
                  FactoryBridge.Accept<EuroclassSuffix>(f.Suffix).ToValidation(),
                  (f.LoadBearingMinutes.IsNone && f.IntegrityMinutes.IsNone && f.InsulationMinutes.IsNone
                      ? Fin.Succ(FireResistance.None)
                      : FireResistance.Of(f.LoadBearingMinutes, f.IntegrityMinutes, f.InsulationMinutes)).ToValidation())
                     .Apply((reaction, suffix, resistance) => Seq(MaterialPropertySet.OfFire(reaction, suffix, resistance, row.Evidence))).As()).As()
                 .Map(static groups => groups.IfNone(Seq<MaterialPropertySet>())),
             row.Physics.DampingRatio.Traverse(zeta =>
                 MaterialPropertySet.OfDamping(zeta, Option<(double AlphaPerS, double BetaS)>.None, row.Evidence).Map(set => Seq(set)).ToValidation()).As()
                 .Map(static groups => groups.IfNone(Seq<MaterialPropertySet>())),
             row.Hygrothermal.Traverse(h =>
                 MaterialPropertySet.OfHygrothermal(h.Porosity, h.W80KgM3, h.WfKgM3, h.AValueKgM2SqrtS, row.Evidence).Map(set => Seq(set)).ToValidation()).As()
                 .Map(static groups => groups.IfNone(Seq<MaterialPropertySet>())),
             row.Optical.Traverse(o =>
                 MaterialPropertySet.OfOptical(o.VisibleT, o.VisibleRf, o.VisibleRb, o.SolarT, o.SolarRf, o.SolarRb, o.IrT, o.IrEf, o.IrEb, row.Evidence).Map(set => Seq(set)).ToValidation()).As()
                 .Map(static groups => groups.IfNone(Seq<MaterialPropertySet>())),
             row.Electrical.Traverse(e =>
                 MaterialPropertySet.OfElectrical(e.ResistivityOhmM, e.RelativePermittivity, e.DielectricStrengthVPerM, e.MagneticPermeabilityRelative, row.Evidence).Map(set => Seq(set)).ToValidation()).As()
                 .Map(static groups => groups.IfNone(Seq<MaterialPropertySet>())))
            .Apply(static (mechanical, thermal, acoustic, fire, damping, hygrothermal, optical, electrical) =>
                Seq(mechanical, thermal) + acoustic + fire + damping + hygrothermal + optical + electrical).As()
            .ToFin());

    static Fin<MaterialPropertySet> Mechanical(MaterialPropertyRow row, StrengthTriple strength) =>
        from density in Measure(Published.Of(UnitsNet.Density.FromKilogramsPerCubicMeter(row.Physics.DensityKgM3), AuthoredBand, row.Evidence),
                                static q => q.KilogramsPerCubicMeter, QuantityRow.Density)
        from youngs in Measure(strength.Youngs, static q => q.Pascals, QuantityRow.Pressure)
        from proof in Measure(strength.Yield, static q => q.Pascals, QuantityRow.Pressure)
        from ultimate in Measure(strength.Ultimate, static q => q.Pascals, QuantityRow.Pressure)
        from set in MaterialPropertySet.OfMechanical(density, youngs, proof, ultimate, row.Physics.PoissonsRatio, row.Physics.ExpansionPerK, strength.Evidence)
        select set;

    static Fin<MaterialPropertySet> Thermal(MaterialPropertyRow row) =>
        from conductivity in Measure(Published.Of(ThermalConductivity.FromWattsPerMeterKelvin(row.Physics.ConductivityWMK), AuthoredBand, row.Evidence),
                                     static q => q.WattsPerMeterKelvin, QuantityRow.ThermalConductivity)
        from specificHeat in Measure(Published.Of(SpecificEntropy.FromJoulesPerKilogramKelvin(row.Physics.SpecificHeatJKgK), AuthoredBand, row.Evidence),
                                     static q => q.JoulesPerKilogramKelvin, QuantityRow.SpecificEntropy)
        from unitThickness in QuantityRow.HeatTransferCoefficient.OfNative(row.Physics.ConductivityWMK)
        from set in MaterialPropertySet.OfThermal(conductivity, specificHeat, unitThickness, VapourFactor(row.Physics.Vapour), row.Evidence)
        select set;

    static double VapourFactor(VapourResistance vapour) => vapour.Switch(
        impermeable: static _ => double.PositiveInfinity,
        factor: static f => f.Mu);

    static Fin<MeasureValue> Measure<TQuantity>(Published<TQuantity> datum, Func<TQuantity, double> si, QuantityRow row) where TQuantity : IQuantity =>
        row.OfNative(si(datum.Value.CentralValue)).Bind(measure => datum.Kind == UncertaintyKind.Exact
            ? Fin.Succ(measure)
            : datum.Band(si, row.Scale).Bind(band => measure.WithUncertainty(band)));

    static Fin<StrengthTriple> Strength(MechanicalSource source) =>
        source.Switch(
            authored: static a => Fin.Succ(new StrengthTriple(
                Published.Of(Pressure.FromMegapascals(a.YoungsMpa), AuthoredBand, PropertyEvidence.Catalogue),
                Published.Of(Pressure.FromMegapascals(a.YieldMpa), AuthoredBand, PropertyEvidence.Catalogue),
                Published.Of(Pressure.FromMegapascals(a.UltimateMpa), AuthoredBand, PropertyEvidence.Catalogue),
                PropertyEvidence.Catalogue)),
            nailWire: static n => n.Class.At(n.ShankDiameterIn).Match(
                Some: band => Fin.Succ(new StrengthTriple(
                    Published.Exact(Pressure.FromMegapascals(NailWireModulusMpa), NailWireTable),
                    Published.Exact(Pressure.FromMegapascals(band.FybMpa), NailWireTable),
                    Published.Exact(Pressure.FromMegapascals(n.UltimateMpa), NailWireTable),
                    NailWireTable)),
                None: () => new ElementFault.ValueRejected($"<nail-wire-diameter-unbanded:{n.Class.Key}:{n.ShankDiameterIn:R}>")),
            enSteel: static s => Try.lift(() => {
                    EnSteelMaterial material = new(s.Grade, NationalAnnex.RecommendedValues);
                    material.Specification.DeliveryCondition = s.Delivery;
                    return Fin.Succ(EnSteelFactory.CreateBiLinear(material, Length.FromMillimeters(GradeThicknessMm)));
                }).Run().Bind(static inner => inner)
                .Map(law => Delegated(law, SteelTable)),
            enRebar: static r => Try.lift(() => Fin.Succ(EnRebarFactory.CreateBiLinear(r.Grade))).Run().Bind(static inner => inner)
                .Map(law => Delegated(law, RebarTable)));

    static StrengthTriple Delegated(IBiLinearMaterial law, PropertyEvidence evidence) =>
        new(Published.Exact(law.ElasticModulus, evidence),
            Published.Exact(law.YieldStrength, evidence),
            Published.Exact(law.UltimateStrength, evidence),
            evidence);

    // --- [TABLES]
    const double NailWireModulusMpa = 200_000.0;

    static readonly VapourResistance Impermeable = new VapourResistance.Impermeable();
    static VapourResistance Mu(double factor) => new VapourResistance.Factor(factor);

    static readonly Option<FireDeclaration> FireA1 = Some(new FireDeclaration("A1", ""));
    static readonly Option<FireDeclaration> FireA1Ei120 = Some(new FireDeclaration("A1", "", IntegrityMinutes: Some(120), InsulationMinutes: Some(120)));
    static readonly Option<FireDeclaration> FireA2 = Some(new FireDeclaration("A2", "s1,d0"));
    static readonly Option<FireDeclaration> FireB = Some(new FireDeclaration("B", "s1,d0"));
    static readonly Option<FireDeclaration> FireC = Some(new FireDeclaration("C", "s1,d0"));
    static readonly Option<FireDeclaration> FireD = Some(new FireDeclaration("D", "s2,d0"));
    static readonly Option<FireDeclaration> FireD30 = Some(new FireDeclaration("D", "s2,d0", Some(30), Some(30), Some(30)));
    static readonly Option<FireDeclaration> FireE = Some(new FireDeclaration("E", "s2,d0"));
    static readonly Option<FireDeclaration> NoFire = Option<FireDeclaration>.None;

    static readonly Option<double> ZSteel = Some(0.02);
    static readonly Option<double> ZConcrete = Some(0.05);
    static readonly Option<double> ZTimber = Some(0.08);
    static readonly Option<double> NoDamping = Option<double>.None;

    static readonly SubstancePhysics CarbonSteel   = new(7850.0, 0.30, 12.0e-6, 50.0, 460.0, Impermeable, FireA1, ZSteel);
    static readonly SubstancePhysics CastIron      = CarbonSteel with { DensityKgM3 = 7200.0, PoissonsRatio = 0.28, ExpansionPerK = 11.0e-6 };
    static readonly SubstancePhysics Austenitic    = new(8000.0, 0.30, 16.0e-6, 15.0, 500.0, Impermeable, FireA1, ZSteel);
    static readonly SubstancePhysics Duplex        = Austenitic with { DensityKgM3 = 7800.0, ExpansionPerK = 13.0e-6 };
    static readonly SubstancePhysics Aluminium     = new(2700.0, 0.33, 23.0e-6, 167.0, 900.0, Impermeable, FireB, ZSteel);
    static readonly SubstancePhysics Copper        = new(8940.0, 0.34, 17.0e-6, 339.0, 385.0, Impermeable, FireA1, NoDamping);
    static readonly SubstancePhysics Thermoset     = new(1200.0, 0.35, 60.0e-6, 0.20, 1000.0, Mu(10_000.0), FireE, NoDamping);
    static readonly SubstancePhysics Concrete      = new(2400.0, 0.20, 10.0e-6, 2.30, 1000.0, Mu(50.0), FireA1, ZConcrete);
    static readonly SubstancePhysics Softwood      = new(350.0, 0.40, 5.0e-6, 0.13, 1600.0, Mu(50.0), FireD, ZTimber);
    static readonly SubstancePhysics Hardwood      = new(550.0, 0.35, 5.0e-6, 0.17, 2400.0, Mu(50.0), FireD, ZTimber);
    static readonly SubstancePhysics Glulam        = new(400.0, 0.40, 5.0e-6, 0.12, 1600.0, Mu(50.0), FireD30, ZTimber);
    static readonly SubstancePhysics ClayUnit      = new(1800.0, 0.25, 6.0e-6, 0.77, 1000.0, Mu(16.0), FireA1, ZConcrete);
    static readonly SubstancePhysics SilicateUnit  = new(1800.0, 0.25, 8.0e-6, 1.00, 1000.0, Mu(15.0), FireA1, ZConcrete);
    static readonly SubstancePhysics AacUnit       = new(500.0, 0.20, 8.0e-6, 0.13, 1000.0, Mu(6.0), FireA1, ZConcrete);
    static readonly SubstancePhysics AggregateUnit = new(1400.0, 0.20, 8.0e-6, 0.51, 1000.0, Mu(6.0), FireA1, ZConcrete);
    static readonly SubstancePhysics Stone         = new(2700.0, 0.25, 7.0e-6, 2.80, 880.0, Mu(10_000.0), FireA1Ei120, ZConcrete);
    static readonly SubstancePhysics SodaLime      = new(2500.0, 0.22, 9.0e-6, 1.00, 720.0, Impermeable, FireA1, NoDamping);
    static readonly SubstancePhysics Borosilicate  = new(2230.0, 0.20, 3.3e-6, 1.20, 830.0, Impermeable, FireA1, NoDamping);
    static readonly SubstancePhysics MineralWool   = new(40.0, 0.0, 0.0, 0.035, 1030.0, Mu(1.0), FireA1, NoDamping);
    static readonly SubstancePhysics RigidFoam     = new(33.0, 0.10, 70.0e-6, 0.034, 1450.0, Mu(60.0), FireE, NoDamping);
    static readonly SubstancePhysics WoodFibre     = new(160.0, 0.10, 8.0e-6, 0.038, 2100.0, Mu(5.0), FireE, NoDamping);
    static readonly SubstancePhysics Gypsum        = new(700.0, 0.25, 18.0e-6, 0.25, 1000.0, Mu(10.0), FireA2, NoDamping);
    static readonly SubstancePhysics FibreCement   = new(1400.0, 0.20, 8.0e-6, 0.19, 900.0, Mu(15.0), FireA2, NoDamping);
    static readonly SubstancePhysics WoodPanel     = new(600.0, 0.30, 5.0e-6, 0.13, 1600.0, Mu(90.0), FireD, ZTimber);
    static readonly SubstancePhysics Membrane      = new(1150.0, 0.45, 160.0e-6, 0.25, 1000.0, Mu(50_000.0), FireE, NoDamping);
    static readonly SubstancePhysics RigidVinyl    = new(1400.0, 0.38, 52.0e-6, 0.16, 900.0, Mu(50_000.0), NoFire, NoDamping);
    static readonly SubstancePhysics Polyolefin    = new(958.0, 0.42, 150.0e-6, 0.40, 1900.0, Mu(100_000.0), FireE, NoDamping);
    static readonly SubstancePhysics Ceramic       = new(2300.0, 0.22, 6.5e-6, 1.30, 840.0, Impermeable, FireA1, NoDamping);
    static readonly SubstancePhysics Textile       = new(700.0, 0.0, 0.0, 0.06, 1300.0, Mu(5.0), NoFire, NoDamping);
    static readonly SubstancePhysics MineralFelt   = new(250.0, 0.0, 0.0, 0.06, 1000.0, Mu(5.0), NoFire, NoDamping);

    internal static readonly FrozenDictionary<MaterialId, MaterialPropertyRow> Rows = new (MaterialId Id, MaterialPropertyRow Row)[] {
        (MaterialId.Create("steel.s235"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S235, EnSteelDeliveryCondition.AR))),
        (MaterialId.Create("steel.s275"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S275, EnSteelDeliveryCondition.AR))),
        (MaterialId.Create("steel.s355"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S355, EnSteelDeliveryCondition.AR))),
        (MaterialId.Create("steel.s420"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S420, EnSteelDeliveryCondition.N))),
        (MaterialId.Create("steel.s450"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S450, EnSteelDeliveryCondition.AR))),
        (MaterialId.Create("steel.s460"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S460, EnSteelDeliveryCondition.N))),
        (MaterialId.Create("steel.s690"), new(CarbonSteel, MechanicalSource.Mpa(210_000.0, 690.0, 770.0))),
        (MaterialId.Create("metal.steel"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S235, EnSteelDeliveryCondition.AR))),
        (MaterialId.Create("metal.iron"), new(CastIron, MechanicalSource.Mpa(170_000.0, 250.0, 400.0))),
        (MaterialId.Create("iron.cast"),    new(CastIron with { DensityKgM3 = 7150.0, PoissonsRatio = 0.26 }, MechanicalSource.Mpa(100_000.0, 207.0, 207.0))),
        (MaterialId.Create("iron.ductile"), new(CastIron with { DensityKgM3 = 7100.0 }, MechanicalSource.Mpa(165_000.0, 290.0, 414.0))),
        (MaterialId.Create("steel.a36"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 250.0, 400.0))),
        (MaterialId.Create("steel.a992"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 345.0, 450.0))),
        (MaterialId.Create("steel.a572"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 345.0, 450.0))),
        (MaterialId.Create("steel.b450a"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B450A))),
        (MaterialId.Create("steel.b450c"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B450C))),
        (MaterialId.Create("steel.b500a"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B500A))),
        (MaterialId.Create("steel.b500b"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B500B))),
        (MaterialId.Create("steel.b500c"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B500C))),
        (MaterialId.Create("steel.b550b"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B550B))),
        (MaterialId.Create("steel.gr40"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 280.0, 420.0))),
        (MaterialId.Create("steel.gr60"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 420.0, 620.0))),
        (MaterialId.Create("steel.gr75"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 520.0, 690.0))),
        (MaterialId.Create("steel.gr80"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 550.0, 725.0))),
        (MaterialId.Create("steel.gr60w"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 420.0, 550.0))),
        (MaterialId.Create("steel.gr80w"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 550.0, 690.0))),
        (MaterialId.Create("steel.400w"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 400.0, 540.0))),
        (MaterialId.Create("steel.500w"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 500.0, 620.0))),
        (MaterialId.Create("steel.g33"),           new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 230.0, 310.0))),
        (MaterialId.Create("steel.g50"),           new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 340.0, 450.0))),
        (MaterialId.Create("steel.fastener-4_6"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 240.0, 400.0))),
        (MaterialId.Create("steel.fastener-4_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 320.0, 400.0))),
        (MaterialId.Create("steel.fastener-5_6"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 300.0, 500.0))),
        (MaterialId.Create("steel.fastener-5_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 400.0, 500.0))),
        (MaterialId.Create("steel.fastener-6_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 480.0, 600.0))),
        (MaterialId.Create("steel.fastener-8_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 640.0, 800.0))),
        (MaterialId.Create("steel.fastener-10_9"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 900.0, 1000.0))),
        (MaterialId.Create("steel.fastener-12_9"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 1080.0, 1200.0))),
        (MaterialId.Create("steel.fastener-gr2"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 393.0, 510.0))),
        (MaterialId.Create("steel.fastener-gr5"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 634.0, 827.0))),
        (MaterialId.Create("steel.fastener-gr8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 896.0, 1034.0))),
        (MaterialId.Create("steel.fastener-a325"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 634.0, 827.0))),
        (MaterialId.Create("steel.fastener-a490"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 896.0, 1034.0))),
        (MaterialId.Create("steel.a500"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 345.0, 427.0))),
        (MaterialId.Create("steel.a53"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 240.0, 415.0))),
        (MaterialId.Create("steel.a653"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 340.0, 450.0))),
        (MaterialId.Create("steel.galvanized"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 230.0, 310.0))),
        (MaterialId.Create("steel.e60"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 330.0, 415.0))),
        (MaterialId.Create("steel.e70"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 400.0, 485.0))),
        (MaterialId.Create("steel.e80"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 460.0, 550.0))),
        (MaterialId.Create("steel.e90"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 530.0, 620.0))),
        (MaterialId.Create("steel.e100"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 600.0, 690.0))),
        (MaterialId.Create("steel.e110"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 670.0, 760.0))),
        (MaterialId.Create("steel.sd1"),   new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 350.0, 450.0))),
        (MaterialId.Create("steel.sd2"),   new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 235.0, 400.0))),
        (MaterialId.Create("steel.sd3"),   new(Austenitic, MechanicalSource.Mpa(200_000.0, 350.0, 500.0))),
        (MaterialId.Create("steel.aws-a"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 340.0, 420.0))),
        (MaterialId.Create("steel.aws-b"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 350.0, 450.0))),
        (MaterialId.Create("steel.fastener-nail"),  new(CarbonSteel, new MechanicalSource.NailWire(NailWireClass.LowCarbon, 0.131, 690.0))),
        (MaterialId.Create("steel.fastener-dowel"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 235.0, 400.0))),
        (MaterialId.Create("steel.fastener-rivet"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 195.0, 415.0))),
        (MaterialId.Create("steel.strand-1725"), new(CarbonSteel, MechanicalSource.Mpa(195_000.0, 1552.0, 1725.0))),
        (MaterialId.Create("steel.strand-1860"), new(CarbonSteel, MechanicalSource.Mpa(195_000.0, 1674.0, 1860.0))),
        (MaterialId.Create("steel.y1860s7"),     new(CarbonSteel, MechanicalSource.Mpa(195_000.0, 1637.0, 1860.0))),
        (MaterialId.Create("adhesive.epoxy"),              new(Thermoset, MechanicalSource.Mpa(3000.0, 30.0, 30.0))),
        (MaterialId.Create("adhesive.methacrylate"),       new(Thermoset with { DensityKgM3 = 1050.0, PoissonsRatio = 0.38, ExpansionPerK = 80.0e-6, SpecificHeatJKgK = 1400.0 }, MechanicalSource.Mpa(1500.0, 25.0, 25.0))),
        (MaterialId.Create("adhesive.polyurethane"),       new(Thermoset with { DensityKgM3 = 1150.0, PoissonsRatio = 0.40, ExpansionPerK = 100.0e-6, ConductivityWMK = 0.21, SpecificHeatJKgK = 1600.0 }, MechanicalSource.Mpa(800.0, 15.0, 15.0))),
        (MaterialId.Create("sealant.silicone-structural"), new(Thermoset with { DensityKgM3 = 1400.0, PoissonsRatio = 0.48, ExpansionPerK = 250.0e-6, ConductivityWMK = 0.35, SpecificHeatJKgK = 1200.0 }, MechanicalSource.Mpa(2.0, 1.0, 1.0))),
        (MaterialId.Create("steel.1.4301"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 210.0, 520.0))),
        (MaterialId.Create("steel.1.4307"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 200.0, 500.0))),
        (MaterialId.Create("steel.1.4401"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 220.0, 520.0))),
        (MaterialId.Create("steel.1.4404"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 220.0, 520.0))),
        (MaterialId.Create("steel.1.4571"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 220.0, 520.0))),
        (MaterialId.Create("steel.1.4462"), new(Duplex,     MechanicalSource.Mpa(200_000.0, 460.0, 640.0))),
        (MaterialId.Create("concrete.c12_15"),  new(Concrete with { ConductivityWMK = 1.65 }, MechanicalSource.Mpa(27_000.0, 12.0, 20.0))),
        (MaterialId.Create("concrete.c16_20"),  new(Concrete with { ConductivityWMK = 1.80 }, MechanicalSource.Mpa(29_000.0, 16.0, 24.0))),
        (MaterialId.Create("concrete.c20_25"),  new(Concrete with { ConductivityWMK = 2.00 }, MechanicalSource.Mpa(30_000.0, 20.0, 28.0))),
        (MaterialId.Create("concrete.c25_30"),  new(Concrete, MechanicalSource.Mpa(31_000.0, 25.0, 33.0))),
        (MaterialId.Create("concrete.c30_37"),  new(Concrete, MechanicalSource.Mpa(33_000.0, 30.0, 38.0))),
        (MaterialId.Create("concrete.c35_45"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(80.0) }, MechanicalSource.Mpa(34_000.0, 35.0, 43.0))),
        (MaterialId.Create("concrete.c40_50"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(80.0) }, MechanicalSource.Mpa(35_000.0, 40.0, 48.0))),
        (MaterialId.Create("concrete.c45_55"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(80.0) }, MechanicalSource.Mpa(36_000.0, 45.0, 53.0))),
        (MaterialId.Create("concrete.c50_60"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(90.0) }, MechanicalSource.Mpa(37_000.0, 50.0, 58.0))),
        (MaterialId.Create("concrete.c55_67"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(100.0) }, MechanicalSource.Mpa(38_000.0, 55.0, 63.0))),
        (MaterialId.Create("concrete.c60_75"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(100.0) }, MechanicalSource.Mpa(39_000.0, 60.0, 68.0))),
        (MaterialId.Create("concrete.c70_85"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(120.0) }, MechanicalSource.Mpa(41_000.0, 70.0, 78.0))),
        (MaterialId.Create("concrete.c80_95"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(120.0) }, MechanicalSource.Mpa(42_000.0, 80.0, 88.0))),
        (MaterialId.Create("concrete.c90_105"), new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(130.0) }, MechanicalSource.Mpa(44_000.0, 90.0, 98.0))),
        (MaterialId.Create("concrete.lc"), new(Concrete with { DensityKgM3 = 1800.0, ExpansionPerK = 8.0e-6, ConductivityWMK = 0.80 }, MechanicalSource.Mpa(18_000.0, 30.0, 38.0))),
        (MaterialId.Create("concrete.cmu"), new(Concrete with { DensityKgM3 = 2000.0, ExpansionPerK = 8.0e-6, ConductivityWMK = 1.15, Vapour = Mu(6.0) }, MechanicalSource.Mpa(12_400.0, 13.8, 17.2))),
        (MaterialId.Create("timber.c14"), new(Softwood with { DensityKgM3 = 290.0 }, MechanicalSource.Mpa(7_000.0, 14.0, 23.0))),
        (MaterialId.Create("timber.c16"), new(Softwood with { DensityKgM3 = 310.0 }, MechanicalSource.Mpa(8_000.0, 16.0, 26.0))),
        (MaterialId.Create("timber.c18"), new(Softwood with { DensityKgM3 = 320.0 }, MechanicalSource.Mpa(9_000.0, 18.0, 30.0))),
        (MaterialId.Create("timber.c20"), new(Softwood with { DensityKgM3 = 330.0 }, MechanicalSource.Mpa(9_500.0, 20.0, 33.0))),
        (MaterialId.Create("timber.c22"), new(Softwood with { DensityKgM3 = 340.0 }, MechanicalSource.Mpa(10_000.0, 22.0, 37.0))),
        (MaterialId.Create("timber.c24"), new(Softwood with { Fire = FireD30 }, MechanicalSource.Mpa(11_000.0, 24.0, 40.0),
            Some(new AcousticDeclaration(
                Absorb(0.10, 0.11, 0.10, 0.08, 0.06, 0.06, 0.07, 0.07, 0.08, 0.08, 0.09, 0.09, 0.09, 0.10, 0.10, 0.10, 0.11, 0.11),
                Sri(14, 16, 18, 20, 22, 24, 26, 27, 29, 31, 33, 34, 36, 38, 40, 41, 42, 43))))),
        (MaterialId.Create("timber.c27"), new(Softwood with { DensityKgM3 = 370.0 }, MechanicalSource.Mpa(11_500.0, 27.0, 45.0))),
        (MaterialId.Create("timber.c30"), new(Softwood with { DensityKgM3 = 380.0 }, MechanicalSource.Mpa(12_000.0, 30.0, 50.0))),
        (MaterialId.Create("timber.c35"), new(Softwood with { DensityKgM3 = 400.0 }, MechanicalSource.Mpa(13_000.0, 35.0, 58.0))),
        (MaterialId.Create("timber.c40"), new(Softwood with { DensityKgM3 = 420.0 }, MechanicalSource.Mpa(14_000.0, 40.0, 66.0))),
        (MaterialId.Create("timber.c45"), new(Softwood with { DensityKgM3 = 440.0 }, MechanicalSource.Mpa(15_000.0, 45.0, 75.0))),
        (MaterialId.Create("timber.c50"), new(Softwood with { DensityKgM3 = 460.0 }, MechanicalSource.Mpa(16_000.0, 50.0, 83.0))),
        (MaterialId.Create("timber.d18"), new(Hardwood with { DensityKgM3 = 475.0 }, MechanicalSource.Mpa(9_500.0, 18.0, 30.0))),
        (MaterialId.Create("timber.d24"), new(Hardwood with { DensityKgM3 = 485.0 }, MechanicalSource.Mpa(10_000.0, 24.0, 40.0))),
        (MaterialId.Create("timber.d27"), new(Hardwood with { DensityKgM3 = 510.0 }, MechanicalSource.Mpa(10_500.0, 27.0, 45.0))),
        (MaterialId.Create("timber.d30"), new(Hardwood with { DensityKgM3 = 530.0 }, MechanicalSource.Mpa(11_000.0, 30.0, 50.0))),
        (MaterialId.Create("timber.d35"), new(Hardwood with { DensityKgM3 = 540.0 }, MechanicalSource.Mpa(12_000.0, 35.0, 58.0))),
        (MaterialId.Create("timber.d40"), new(Hardwood, MechanicalSource.Mpa(13_000.0, 40.0, 66.0))),
        (MaterialId.Create("timber.d45"), new(Hardwood with { DensityKgM3 = 580.0 }, MechanicalSource.Mpa(13_500.0, 45.0, 75.0))),
        (MaterialId.Create("timber.d50"), new(Hardwood with { DensityKgM3 = 620.0 }, MechanicalSource.Mpa(14_000.0, 50.0, 83.0))),
        (MaterialId.Create("timber.d55"), new(Hardwood with { DensityKgM3 = 660.0 }, MechanicalSource.Mpa(15_500.0, 55.0, 92.0))),
        (MaterialId.Create("timber.d60"), new(Hardwood with { DensityKgM3 = 700.0 }, MechanicalSource.Mpa(17_000.0, 60.0, 100.0))),
        (MaterialId.Create("timber.d65"), new(Hardwood with { DensityKgM3 = 750.0 }, MechanicalSource.Mpa(18_500.0, 65.0, 109.0))),
        (MaterialId.Create("timber.d70"), new(Hardwood with { DensityKgM3 = 800.0 }, MechanicalSource.Mpa(20_000.0, 70.0, 117.0))),
        (MaterialId.Create("timber.d75"), new(Hardwood with { DensityKgM3 = 850.0 }, MechanicalSource.Mpa(22_000.0, 75.0, 125.0))),
        (MaterialId.Create("timber.d80"), new(Hardwood with { DensityKgM3 = 900.0 }, MechanicalSource.Mpa(24_000.0, 80.0, 134.0))),
        (MaterialId.Create("wood.oak"), new(Hardwood with { DensityKgM3 = 700.0 }, MechanicalSource.Mpa(11_000.0, 40.0, 90.0),
            Some(new AcousticDeclaration(
                Absorb(0.05, 0.06, 0.07, 0.08, 0.10, 0.10, 0.11, 0.10, 0.10, 0.10, 0.10, 0.10, 0.09, 0.09, 0.09, 0.09, 0.09, 0.09),
                Sri(18, 20, 22, 24, 26, 29, 31, 33, 35, 37, 38, 39, 40, 40, 39, 35, 33, 31))))),
        (MaterialId.Create("timber.gl20h"), new(Glulam with { DensityKgM3 = 340.0 }, MechanicalSource.Mpa(8_400.0, 20.0, 33.0))),
        (MaterialId.Create("timber.gl22h"), new(Glulam with { DensityKgM3 = 370.0 }, MechanicalSource.Mpa(10_500.0, 22.0, 37.0))),
        (MaterialId.Create("timber.gl24h"), new(Glulam with { DensityKgM3 = 385.0 }, MechanicalSource.Mpa(11_500.0, 24.0, 40.0))),
        (MaterialId.Create("timber.gl26h"), new(Glulam with { DensityKgM3 = 405.0 }, MechanicalSource.Mpa(12_100.0, 26.0, 43.0))),
        (MaterialId.Create("timber.gl28h"), new(Glulam with { DensityKgM3 = 425.0 }, MechanicalSource.Mpa(12_600.0, 28.0, 47.0))),
        (MaterialId.Create("timber.gl30h"), new(Glulam with { DensityKgM3 = 440.0 }, MechanicalSource.Mpa(13_600.0, 30.0, 50.0))),
        (MaterialId.Create("timber.gl32h"), new(Glulam with { DensityKgM3 = 440.0 }, MechanicalSource.Mpa(14_200.0, 32.0, 53.0))),
        (MaterialId.Create("timber.gl20c"), new(Glulam with { DensityKgM3 = 355.0 }, MechanicalSource.Mpa(10_400.0, 20.0, 33.0))),
        (MaterialId.Create("timber.gl22c"), new(Glulam with { DensityKgM3 = 355.0 }, MechanicalSource.Mpa(10_400.0, 22.0, 37.0))),
        (MaterialId.Create("timber.gl24c"), new(Glulam with { DensityKgM3 = 365.0 }, MechanicalSource.Mpa(11_000.0, 24.0, 40.0))),
        (MaterialId.Create("timber.gl26c"), new(Glulam with { DensityKgM3 = 385.0 }, MechanicalSource.Mpa(12_000.0, 26.0, 43.0))),
        (MaterialId.Create("timber.gl28c"), new(Glulam with { DensityKgM3 = 390.0 }, MechanicalSource.Mpa(12_500.0, 28.0, 47.0))),
        (MaterialId.Create("timber.gl30c"), new(Glulam with { DensityKgM3 = 390.0 }, MechanicalSource.Mpa(13_000.0, 30.0, 50.0))),
        (MaterialId.Create("timber.gl32c"), new(Glulam, MechanicalSource.Mpa(13_500.0, 32.0, 53.0))),
        (MaterialId.Create("aluminium.6082t6"), new(Aluminium with { DensityKgM3 = 2710.0, ExpansionPerK = 23.1e-6, ConductivityWMK = 170.0 }, MechanicalSource.Mpa(70_000.0, 260.0, 310.0))),
        (MaterialId.Create("aluminium.6061t6"), new(Aluminium, MechanicalSource.Mpa(70_000.0, 240.0, 290.0))),
        (MaterialId.Create("aluminium.6063t5"), new(Aluminium with { ConductivityWMK = 200.0 }, MechanicalSource.Mpa(70_000.0, 130.0, 175.0))),
        (MaterialId.Create("aluminium.6063t6"), new(Aluminium with { ConductivityWMK = 200.0 }, MechanicalSource.Mpa(70_000.0, 160.0, 195.0))),
        (MaterialId.Create("aluminium.5083"),   new(Aluminium with { DensityKgM3 = 2660.0, ExpansionPerK = 24.0e-6, ConductivityWMK = 117.0 }, MechanicalSource.Mpa(70_000.0, 125.0, 275.0))),
        (MaterialId.Create("aluminium.1350"),   new(Aluminium with { ConductivityWMK = 234.0 }, MechanicalSource.Mpa(70_000.0, 28.0, 83.0),
            Electrical: Some(new ElectricalDeclaration(2.83e-8, 1.0)))),
        (MaterialId.Create("copper.c12200"), new(Copper, MechanicalSource.Mpa(117_000.0, 207.0, 248.0),
            Electrical: Some(new ElectricalDeclaration(2.03e-8, 1.0)))),
        (MaterialId.Create("masonry.clay"), new(ClayUnit, MechanicalSource.Mpa(7_000.0, 10.0, 20.0),
            Some(new AcousticDeclaration(
                Absorb(0.02, 0.02, 0.03, 0.03, 0.03, 0.04, 0.04, 0.05, 0.05, 0.05, 0.05, 0.06, 0.06, 0.06, 0.07, 0.07, 0.07, 0.07),
                Sri(30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 53, 54, 55, 56, 57, 58))),
            Hygrothermal: Some(new HygrothermalDeclaration(0.38, 9.2, 190.0, Some(0.110))))),
        (MaterialId.Create("masonry.calciumsilicate"), new(SilicateUnit, MechanicalSource.Mpa(8_000.0, 12.0, 24.0))),
        (MaterialId.Create("masonry.aac"), new(AacUnit, MechanicalSource.Mpa(2_000.0, 4.0, 5.0),
            Hygrothermal: Some(new HygrothermalDeclaration(0.81, 7.7, 380.0, Some(0.050))))),
        (MaterialId.Create("masonry.aggregate"), new(AggregateUnit, MechanicalSource.Mpa(9_000.0, 7.0, 14.0))),
        (MaterialId.Create("stone.marble"),  new(Stone, MechanicalSource.Mpa(70_000.0, 15.0, 100.0))),
        (MaterialId.Create("stone.granite"), new(Stone with { DensityKgM3 = 2650.0, PoissonsRatio = 0.23, ExpansionPerK = 8.0e-6, ConductivityWMK = 3.00, SpecificHeatJKgK = 790.0 }, MechanicalSource.Mpa(60_000.0, 20.0, 130.0))),
        (MaterialId.Create("glass.float"), new(SodaLime, MechanicalSource.Mpa(70_000.0, 45.0, 50.0),
            Some(new AcousticDeclaration(
                Absorb(0.18, 0.10, 0.07, 0.05, 0.04, 0.03, 0.03, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02),
                Sri(25, 27, 29, 30, 31, 32, 33, 34, 33, 32, 30, 29, 31, 34, 37, 39, 40, 41))),
            Optical: Some(new OpticalDeclaration(0.90, 0.08, 0.08, 0.85, 0.075, 0.075, 0.0, 0.837, 0.837)))),
        (MaterialId.Create("glass.crown"), new(SodaLime, MechanicalSource.Mpa(70_000.0, 45.0, 50.0),
            Optical: Some(new OpticalDeclaration(0.90, 0.08, 0.08, 0.85, 0.075, 0.075, 0.0, 0.837, 0.837)))),
        (MaterialId.Create("glass.flint"), new(Borosilicate, MechanicalSource.Mpa(63_000.0, 45.0, 50.0),
            Optical: Some(new OpticalDeclaration(0.92, 0.07, 0.07, 0.84, 0.07, 0.07, 0.0, 0.837, 0.837)))),
        (MaterialId.Create("insulation.glasswool"), new(MineralWool, MechanicalSource.Mpa(1.0, 0.001, 0.002),
            Some(new AcousticDeclaration(
                Absorb(0.15, 0.25, 0.40, 0.55, 0.70, 0.80, 0.90, 0.95, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 16),
                FlowResistivityPaSPerM2: Some(15_000.0))))),
        (MaterialId.Create("insulation.stonewool"), new(MineralWool with { DensityKgM3 = 45.0 }, MechanicalSource.Mpa(1.0, 0.001, 0.002),
            Some(new AcousticDeclaration(
                Absorb(0.16, 0.26, 0.42, 0.58, 0.72, 0.82, 0.92, 0.97, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 16),
                FlowResistivityPaSPerM2: Some(38_000.0))))),
        (MaterialId.Create("insulation.eps"),      new(RigidFoam with { DensityKgM3 = 20.0, ExpansionPerK = 60.0e-6, ConductivityWMK = 0.036 }, MechanicalSource.Mpa(5.0, 0.05, 0.10))),
        (MaterialId.Create("insulation.xps"),      new(RigidFoam with { Vapour = Mu(150.0) }, MechanicalSource.Mpa(15.0, 0.20, 0.45))),
        (MaterialId.Create("insulation.pir"),      new(RigidFoam with { DensityKgM3 = 32.0, ConductivityWMK = 0.022, SpecificHeatJKgK = 1400.0 }, MechanicalSource.Mpa(10.0, 0.10, 0.20))),
        (MaterialId.Create("insulation.pur"),      new(RigidFoam with { DensityKgM3 = 35.0, ConductivityWMK = 0.025, SpecificHeatJKgK = 1400.0 }, MechanicalSource.Mpa(10.0, 0.10, 0.20))),
        (MaterialId.Create("insulation.phenolic"), new(RigidFoam with { DensityKgM3 = 35.0, ConductivityWMK = 0.020, SpecificHeatJKgK = 1400.0, Vapour = Mu(50.0), Fire = FireC }, MechanicalSource.Mpa(10.0, 0.10, 0.20))),
        (MaterialId.Create("insulation.woodfibre"), new(WoodFibre, MechanicalSource.Mpa(50.0, 0.10, 0.20),
            Some(new AcousticDeclaration(
                Absorb(0.12, 0.20, 0.35, 0.50, 0.65, 0.75, 0.85, 0.90, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95),
                Sri(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 13, 14, 14, 15, 15, 16, 17),
                FlowResistivityPaSPerM2: Some(100_000.0))))),
        (MaterialId.Create("gypsum.board"), new(Gypsum, MechanicalSource.Mpa(2_500.0, 2.0, 4.0),
            Some(new AcousticDeclaration(
                Absorb(0.29, 0.20, 0.12, 0.10, 0.08, 0.06, 0.06, 0.05, 0.04, 0.04, 0.04, 0.04, 0.05, 0.05, 0.06, 0.06, 0.07, 0.07),
                Sri(15, 16, 17, 18, 20, 22, 24, 26, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38))),
            Hygrothermal: Some(new HygrothermalDeclaration(0.65, 6.3, 400.0, Some(0.287))))),
        (MaterialId.Create("cement.board"), new(FibreCement, MechanicalSource.Mpa(7_000.0, 7.0, 10.0))),
        (MaterialId.Create("wood.plywood"), new(WoodPanel, MechanicalSource.Mpa(8_000.0, 30.0, 40.0))),
        (MaterialId.Create("wood.osb"),     new(WoodPanel with { DensityKgM3 = 650.0, SpecificHeatJKgK = 1700.0, Vapour = Mu(200.0) }, MechanicalSource.Mpa(3_500.0, 20.0, 26.0))),
        (MaterialId.Create("membrane.epdm"), new(Membrane, MechanicalSource.Mpa(5.0, 5.0, 9.0))),
        (MaterialId.Create("membrane.pvc"),  new(Membrane with { DensityKgM3 = 1300.0, ExpansionPerK = 70.0e-6, ConductivityWMK = 0.16, Vapour = Mu(20_000.0) }, MechanicalSource.Mpa(15.0, 10.0, 15.0))),
        (MaterialId.Create("membrane.tpo"),  new(Membrane with { DensityKgM3 = 920.0, ExpansionPerK = 150.0e-6, ConductivityWMK = 0.20, Vapour = Mu(30_000.0) }, MechanicalSource.Mpa(10.0, 9.0, 14.0))),
        (MaterialId.Create("membrane.wrap"), new(Polyolefin with { DensityKgM3 = 330.0, ConductivityWMK = 0.50, Vapour = Mu(54.0) }, MechanicalSource.Mpa(200.0, 34.0, 34.0))),
        (MaterialId.Create("membrane.pe"),   new(Polyolefin with { DensityKgM3 = 920.0, ConductivityWMK = 0.33, Fire = NoFire }, MechanicalSource.Mpa(200.0, 10.0, 17.0))),
        (MaterialId.Create("membrane.sbs"),  new(Membrane with { DensityKgM3 = 1100.0, ConductivityWMK = 0.23 }, MechanicalSource.Mpa(50.0, 3.5, 3.5))),
        (MaterialId.Create("pipe.pvc"),  new(RigidVinyl, MechanicalSource.Mpa(2_760.0, 48.0, 48.0),
            Electrical: Some(new ElectricalDeclaration(1.0e12, 3.4)))),
        (MaterialId.Create("pipe.cpvc"), new(RigidVinyl with { DensityKgM3 = 1550.0, ExpansionPerK = 62.0e-6, ConductivityWMK = 0.14 }, MechanicalSource.Mpa(2_900.0, 55.0, 55.0))),
        (MaterialId.Create("pipe.pex"),  new(Polyolefin with { DensityKgM3 = 940.0, ConductivityWMK = 0.38 }, MechanicalSource.Mpa(800.0, 19.0, 22.0),
            Electrical: Some(new ElectricalDeclaration(1.0e14, 2.3)))),
        (MaterialId.Create("pipe.hdpe"), new(Polyolefin, MechanicalSource.Mpa(1_000.0, 23.0, 30.0),
            Electrical: Some(new ElectricalDeclaration(1.0e14, 2.3)))),
        (MaterialId.Create("ceramic.tile"),       new(Ceramic, MechanicalSource.Mpa(70_000.0, 35.0, 35.0))),
        (MaterialId.Create("flooring.resilient"), new(RigidVinyl with { DensityKgM3 = 1700.0, ConductivityWMK = 0.17 }, MechanicalSource.Mpa(2_000.0, 15.0, 25.0))),
        (MaterialId.Create("flooring.carpet"),    new(Textile, MechanicalSource.Mpa(1.0, 0.001, 0.002))),
        (MaterialId.Create("ceiling.mineral"),    new(MineralFelt, MechanicalSource.Mpa(50.0, 0.20, 0.40))),
        (MaterialId.Create("coating.paint"),      new(Thermoset with { DensityKgM3 = 1300.0, Fire = NoFire }, MechanicalSource.Mpa(1_000.0, 3.0, 3.0))),
        (MaterialId.Create("fireproofing.sfrm"),        new(Gypsum with { DensityKgM3 = 300.0, ConductivityWMK = 0.12, SpecificHeatJKgK = 1200.0, Fire = FireA1 }, MechanicalSource.Mpa(10.0, 0.0072, 0.0072))),
        (MaterialId.Create("fireproofing.intumescent"), new(Thermoset with { DensityKgM3 = 1400.0, Fire = NoFire }, MechanicalSource.Mpa(2_000.0, 5.0, 5.0))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    static readonly Lazy<FrozenDictionary<MaterialId, Seq<MaterialPropertySet>>> Admitted =
        new(static () => Rows
                .Select(static entry => (entry.Key, Sets: Admit(entry.Value, AdmitKey)))
                .Where(static entry => entry.Sets.IsSucc)
                .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Sets.ThrowIfFail()),
            LazyThreadSafetyMode.ExecutionAndPublication);

    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id) =>
        Admitted.Value.TryGetValue(id, out Seq<MaterialPropertySet> admitted)
            ? Fin.Succ(admitted)
            : Rows.TryGetValue(id, out MaterialPropertyRow? row)
                ? Admit(row!)
                : new ElementFault.ValueRejected($"<unregistered-material-properties:{id.ToValue()}>");

    static ReadOnlyMemory<double> Absorb(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
    static ReadOnlyMemory<double> Sri(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
}
```

## [03]-[DURABILITY_MIX]

- Owner: `CementType` the binder axis carrying its own published ageing exponent; `DurabilityMix` the published `(cement × w/c)` chloride-migration row; `DurabilityCatalogue` the fib Annex B transcription and its `Resolve` lowering onto the contract `Durability` case.
- Law: DURABILITY IS MIX-KEYED, NEVER SUBSTANCE-KEYED. The reference publishes the migration coefficient `D_RCM,0` and the ageing exponent `alpha` against binder type and equivalent water/cement ratio and against nothing else, and a strength class determines neither: one C30/37 is reachable at w/c 0.45 on CEM I and at w/c 0.55 on CEM III/B, whose migration coefficients differ by roughly sevenfold in OPPOSITE directions. A `Durability` column on a concrete substance row is therefore unfillable in principle rather than merely unfilled, and the roster carries none by construction.
- Cases: three published binder rows — CEM I 42.5R, CEM I with fly ash at `k = 0.5`, and CEM III/B — each over the five published w/c steps. A binder the reference adds is one `CementType` row with its five `DurabilityMix` entries.
- Entry: `public static Fin<MaterialPropertySet> Resolve(CementType cement, double waterCementRatio, double carbonationRateMmPerSqrtYear)` — reads the published pair and lowers through the contract `OfDurability`. The carbonation rate is a CALLER input because the reference keys it on exposure class rather than on mix, so the mix table answers exactly the two columns it publishes and the exposure class supplies the third.
- Packages: Rasm.Element (project — `MaterialPropertySet.OfDurability`, `PropertyEvidence`, `ElementFault.ValueRejected`), Rasm, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` the binder axis), LanguageExt.Core (`Fin`/`Option`), BCL inbox (`FrozenDictionary`).
- Boundary: the table admits only the ratios the reference PRINTS. It publishes at 0.05 steps and an interpolated cell is a derivation rather than a transcription, so a ratio between two rows fails instead of blending them, and a ratio outside `[0.40, 0.60]` is outside the reference's stated validity domain and fails for that reason. The migration coefficient carries the reference's own coefficient of variation as a relative band and the ageing exponent its published mean and standard deviation, both stated on the row rather than assumed by a consumer; a project supplying a measured mix design substitutes a `Properties/assessment#ASSESSMENT_RECORD` `Measured` record and never edits this table.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CementType {
    public static readonly CementType PortlandCem1  = new("cem-i-42.5r",   alphaMean: 0.30, alphaSd: 0.12);
    public static readonly CementType PortlandFlyAsh = new("cem-i-fa-k05", alphaMean: 0.60, alphaSd: 0.15);
    public static readonly CementType BlastFurnace  = new("cem-iii-b",     alphaMean: 0.45, alphaSd: 0.20);
    public double AlphaMean { get; }
    public double AlphaSd { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DurabilityMix(CementType Cement, double WaterCementRatio, double DrcmE12, double CoefficientOfVariation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DurabilityCatalogue {
    const double DrcmCoefficientOfVariation = 0.20;
    const double DrcmScaleToSi = 1.0e-12;
    const double MinWaterCementRatio = 0.40;
    const double MaxWaterCementRatio = 0.60;

    static readonly PropertyEvidence MixTable =
        PropertyEvidence.Of("vendor", EvidenceGrade.Import, Some("fib bulletin 34 annex b"));

    // --- [TABLES]
    static readonly FrozenDictionary<(string Cement, double Ratio), DurabilityMix> Mixes = new DurabilityMix[] {
        new(CementType.PortlandCem1, 0.40, 8.9, DrcmCoefficientOfVariation),
        new(CementType.PortlandCem1, 0.45, 10.0, DrcmCoefficientOfVariation),
        new(CementType.PortlandCem1, 0.50, 15.8, DrcmCoefficientOfVariation),
        new(CementType.PortlandCem1, 0.55, 19.7, DrcmCoefficientOfVariation),
        new(CementType.PortlandCem1, 0.60, 25.0, DrcmCoefficientOfVariation),
        new(CementType.PortlandFlyAsh, 0.40, 5.6, DrcmCoefficientOfVariation),
        new(CementType.PortlandFlyAsh, 0.45, 6.9, DrcmCoefficientOfVariation),
        new(CementType.PortlandFlyAsh, 0.50, 9.0, DrcmCoefficientOfVariation),
        new(CementType.PortlandFlyAsh, 0.55, 10.9, DrcmCoefficientOfVariation),
        new(CementType.PortlandFlyAsh, 0.60, 14.9, DrcmCoefficientOfVariation),
        new(CementType.BlastFurnace, 0.40, 1.4, DrcmCoefficientOfVariation),
        new(CementType.BlastFurnace, 0.45, 1.9, DrcmCoefficientOfVariation),
        new(CementType.BlastFurnace, 0.50, 2.8, DrcmCoefficientOfVariation),
        new(CementType.BlastFurnace, 0.55, 3.0, DrcmCoefficientOfVariation),
        new(CementType.BlastFurnace, 0.60, 3.4, DrcmCoefficientOfVariation),
    }.ToFrozenDictionary(static mix => (mix.Cement.Key, mix.WaterCementRatio));

    public static Option<DurabilityMix> At(CementType cement, double waterCementRatio) =>
        Mixes.TryGetValue((cement.Key, waterCementRatio), out DurabilityMix mix) ? Some(mix) : Option<DurabilityMix>.None;

    public static Fin<MaterialPropertySet> Resolve(CementType cement, double waterCementRatio, double carbonationRateMmPerSqrtYear) =>
        At(cement, waterCementRatio)
            .ToFin(new ElementFault.ValueRejected(waterCementRatio is >= MinWaterCementRatio and <= MaxWaterCementRatio
                ? $"<durability-mix-unprinted:{cement.Key}:{waterCementRatio:R}>"
                : $"<durability-mix-out-of-domain:{cement.Key}:{waterCementRatio:R}:{MinWaterCementRatio:R}..{MaxWaterCementRatio:R}>"))
            .Bind(mix => MaterialPropertySet.OfDurability(
                carbonationRateMmPerSqrtYear, mix.DrcmE12 * DrcmScaleToSi, cement.AlphaMean, MixTable));
}
```

## [04]-[MIX_PROPORTION]

- Owner: `ExposureClass` the EN 206:2013 Annex F Table F.1 durability-floor axis (max w/c, min cement, min strength class, min air, per exposure class); `SlumpBand`/`AggregateSize`/`AirBand` the closed ACI table keys; the `Water`/`WcStrength`/`CoarseFraction` published tables (ACI 211.1-91 R2002, SI appendix); `MixSpec` the caller's mix declaration; `MixProportion` the derived per-m³ mass row; `MixDesign.Proportion` the one absolute-volume fold.
- Law: A MIX IS DECLARED, NEVER INFERRED FROM A STRENGTH CLASS — the same law the durability sibling carries: a C30/37 substance row determines no proportions, so the proportion fold takes a caller `MixSpec` and answers the published method's own derivation, and no substance roster column carries a mix. The tables publish the METHOD'S inputs and the job supplies what the method requires by test (§A3.3.1): the two aggregate specific gravities and the oven-dry-rodded coarse unit weight are REQUIRED spec columns, never defaults — the one published assumption is the cement specific gravity 3.15 (§A3.2.1, ASTM C 150/C 175 portlands; a blended cement supplies its tested value).
- Cases: 18 `ExposureClass` rows (X0 · XC1-4 · XS1-3 · XD1-3 · XF1-4 · XA1-3, every cell as EN 206:2013 prints it, two-source verified); 3 `SlumpBand` × 8 `AggregateSize` water cells per lane with the printed dashes typed-absent; 6 SI strength anchors per lane with the air-entrained 40 MPa dash typed-absent; 8 × 4 coarse-fraction cells over the fineness-modulus band `[2.40, 3.00]`.
- Entry: `public static Fin<MixProportion> MixDesign.Proportion(MixSpec spec)` — the ACI absolute-volume chain under the EN exposure floor: water and air from the `Water` row (`Air = None` reads the entrapped-air row, `Some(band)` the entrained target), w/c interpolated between the printed strength anchors on the spec's lane (the method's own interpolation practice; outside the printed band refuses) then CAPPED by the exposure's max w/c, the strength floor enforced (a target below the exposure's minimum class refuses typed), cement `max(water / wc, exposure minimum)`, coarse mass the fineness-interpolated volume fraction × the spec's dry-rodded unit weight, fine aggregate the absolute-volume remainder (a negative remainder refuses — the declared mix is over-constrained), and the proportion records the APPLIED w/c and whether an exposure floor governed.
- Packages: Rasm.Element (project — `ElementFault.ValueRejected`, `MaterialId`), Rasm, Thinktecture.Runtime.Extensions (`[SmartEnum]` the four key vocabularies), LanguageExt.Core (`Fin`/`Option`), BCL inbox (`FrozenDictionary`/`ImmutableArray`).
- Growth: a national-annex variant is a SIBLING exposure table beside this one, never edits to these cells — the CEN survey records most states substituting their own values, so the EN base table stays the transcription and a jurisdiction lands as its own keyed set; a new slump or aggregate row is one key row with its printed cells; a richer method edition (ACI 211.1-22 widened rosters) is a sibling anchor set, never cells blended into the -91 table.
- Boundary: every cell transcribes the print — the SI appendix tables (mixing-water table, w/c-strength table, coarse-fraction table; the appendix table designators vary between printings, so no designator is hard-coded), EN 206:2013 Table F.1 with its footnotes carried as row comments — and an interpolated FM or strength value stays INSIDE the printed table with the out-of-band read refusing; the XF rows' 4,0 % air and the XA2/XA3 sulfate-resisting-cement obligation are row facts a specifier reads, not derivations; `MixSpec.Materials` binds the constituent `MaterialId`s as caller declarations (this page names no substance ids); the constituent-row projection is `Projection/component#COMPOSITION_AUTHOR` `Constituents`' — this owner answers masses and stops.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExposureClass {
    public static readonly ExposureClass X0  = new("X0",  maxWc: Option<double>.None, minCementKgM3: Option<double>.None, strengthClass: "C12/15", minFckMpa: 12, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xc1 = new("XC1", maxWc: 0.65, minCementKgM3: 260, strengthClass: "C20/25", minFckMpa: 20, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xc2 = new("XC2", maxWc: 0.60, minCementKgM3: 280, strengthClass: "C25/30", minFckMpa: 25, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xc3 = new("XC3", maxWc: 0.55, minCementKgM3: 280, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xc4 = new("XC4", maxWc: 0.50, minCementKgM3: 300, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xs1 = new("XS1", maxWc: 0.50, minCementKgM3: 300, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xs2 = new("XS2", maxWc: 0.45, minCementKgM3: 320, strengthClass: "C35/45", minFckMpa: 35, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xs3 = new("XS3", maxWc: 0.45, minCementKgM3: 340, strengthClass: "C35/45", minFckMpa: 35, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xd1 = new("XD1", maxWc: 0.55, minCementKgM3: 300, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xd2 = new("XD2", maxWc: 0.55, minCementKgM3: 300, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xd3 = new("XD3", maxWc: 0.45, minCementKgM3: 320, strengthClass: "C35/45", minFckMpa: 35, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xf1 = new("XF1", maxWc: 0.55, minCementKgM3: 300, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xf2 = new("XF2", maxWc: 0.55, minCementKgM3: 300, strengthClass: "C25/30", minFckMpa: 25, minAirPercent: 4.0);
    public static readonly ExposureClass Xf3 = new("XF3", maxWc: 0.50, minCementKgM3: 320, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: 4.0);
    public static readonly ExposureClass Xf4 = new("XF4", maxWc: 0.45, minCementKgM3: 340, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: 4.0);
    public static readonly ExposureClass Xa1 = new("XA1", maxWc: 0.55, minCementKgM3: 300, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xa2 = new("XA2", maxWc: 0.50, minCementKgM3: 320, strengthClass: "C30/37", minFckMpa: 30, minAirPercent: Option<double>.None);
    public static readonly ExposureClass Xa3 = new("XA3", maxWc: 0.45, minCementKgM3: 360, strengthClass: "C35/45", minFckMpa: 35, minAirPercent: Option<double>.None);

    public Option<double> MaxWc { get; }
    public Option<double> MinCementKgM3 { get; }
    public string StrengthClass { get; }
    public double MinFckMpa { get; }
    public Option<double> MinAirPercent { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SlumpBand {
    public static readonly SlumpBand S25To50   = new("25-50");
    public static readonly SlumpBand S75To100  = new("75-100");
    public static readonly SlumpBand S150To175 = new("150-175");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AggregateSize {
    public static readonly AggregateSize A9p5  = new("9.5",  ordinal: 0);
    public static readonly AggregateSize A12p5 = new("12.5", ordinal: 1);
    public static readonly AggregateSize A19   = new("19",   ordinal: 2);
    public static readonly AggregateSize A25   = new("25",   ordinal: 3);
    public static readonly AggregateSize A37p5 = new("37.5", ordinal: 4);
    public static readonly AggregateSize A50   = new("50",   ordinal: 5);
    public static readonly AggregateSize A75   = new("75",   ordinal: 6);
    public static readonly AggregateSize A150  = new("150",  ordinal: 7);
    public int Ordinal { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AirBand {
    public static readonly AirBand Mild     = new("mild",     targets: [4.5, 4.0, 3.5, 3.0, 2.5, 2.0, 1.5, 1.0]);
    public static readonly AirBand Moderate = new("moderate", targets: [6.0, 5.5, 5.0, 4.5, 4.5, 4.0, 3.5, 3.0]);
    public static readonly AirBand Severe   = new("severe",   targets: [7.5, 7.0, 6.0, 6.0, 5.5, 5.0, 4.5, 4.0]);
    public ImmutableArray<double> Targets { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MixMaterials(MaterialId Cement, MaterialId Water, MaterialId FineAggregate, MaterialId CoarseAggregate);

public sealed record MixSpec(
    ExposureClass Exposure, double TargetMpa, SlumpBand Slump, AggregateSize Aggregate, double FinenessModulus,
    Option<AirBand> Air, double FineSpecificGravity, double CoarseSpecificGravity, double CoarseDryRoddedKgM3,
    MixMaterials Materials, double CementSpecificGravity = 3.15);

public sealed record MixProportion(
    double CementKgM3, double WaterKgM3, double FineKgM3, double CoarseKgM3,
    double AirFraction, double WaterCement, bool ExposureGoverned);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MixDesign {
    const double WaterDensityKgM3 = 1000.0;

    static readonly FrozenDictionary<(bool Air, SlumpBand Band), ImmutableArray<double?>> Water =
        new Dictionary<(bool, SlumpBand), ImmutableArray<double?>> {
            [(false, SlumpBand.S25To50)]   = [207, 199, 190, 179, 166, 154, 130, 113],
            [(false, SlumpBand.S75To100)]  = [228, 216, 205, 193, 181, 169, 145, 124],
            [(false, SlumpBand.S150To175)] = [243, 228, 216, 202, 190, 178, 160, null],
            [(true,  SlumpBand.S25To50)]   = [181, 175, 168, 160, 150, 142, 122, 107],
            [(true,  SlumpBand.S75To100)]  = [202, 193, 184, 175, 165, 157, 133, 119],
            [(true,  SlumpBand.S150To175)] = [216, 205, 197, 184, 174, 166, 154, null],
        }.ToFrozenDictionary();

    static readonly ImmutableArray<double> EntrappedAir = [3.0, 2.5, 2.0, 1.5, 1.0, 0.5, 0.3, 0.2];

    static readonly ImmutableArray<(double Mpa, double NonAir, double? Air)> WcStrength = [
        (40.0, 0.42, null), (35.0, 0.47, 0.39), (30.0, 0.54, 0.45), (25.0, 0.61, 0.52), (20.0, 0.69, 0.60), (15.0, 0.79, 0.70)];

    static readonly FrozenDictionary<AggregateSize, ImmutableArray<double>> CoarseFraction =
        new Dictionary<AggregateSize, ImmutableArray<double>> {
            [AggregateSize.A9p5]  = [0.50, 0.48, 0.46, 0.44], [AggregateSize.A12p5] = [0.59, 0.57, 0.55, 0.53],
            [AggregateSize.A19]   = [0.66, 0.64, 0.62, 0.60], [AggregateSize.A25]   = [0.71, 0.69, 0.67, 0.65],
            [AggregateSize.A37p5] = [0.75, 0.73, 0.71, 0.69], [AggregateSize.A50]   = [0.78, 0.76, 0.74, 0.72],
            [AggregateSize.A75]   = [0.82, 0.80, 0.78, 0.76], [AggregateSize.A150]  = [0.87, 0.85, 0.83, 0.81],
        }.ToFrozenDictionary();

    public static Fin<MixProportion> Proportion(MixSpec spec) =>
        from water in Cell(spec)
        from _floor in spec.TargetMpa >= spec.Exposure.MinFckMpa
            ? Fin.Succ(unit)
            : new ElementFault.ValueRejected($"<mix-strength-below-exposure:{spec.Exposure.Key}:{spec.TargetMpa:R}:{spec.Exposure.StrengthClass}>")
        from wcMethod in InterpolatedWc(spec)
        let wcApplied = spec.Exposure.MaxWc.Match(Some: cap => Math.Min(wcMethod, cap), None: () => wcMethod)
        let cementMethod = water.KgM3 / wcApplied
        let cement = spec.Exposure.MinCementKgM3.Match(Some: floor => Math.Max(cementMethod, floor), None: () => cementMethod)
        from coarseShare in InterpolatedCoarse(spec)
        let coarse = coarseShare * spec.CoarseDryRoddedKgM3
        let fineVolume = 1.0 - (water.KgM3 / WaterDensityKgM3) - (cement / (spec.CementSpecificGravity * WaterDensityKgM3))
            - (coarse / (spec.CoarseSpecificGravity * WaterDensityKgM3)) - water.AirFraction
        from fine in fineVolume > 0.0
            ? Fin.Succ(fineVolume * spec.FineSpecificGravity * WaterDensityKgM3)
            : new ElementFault.ValueRejected($"<mix-overconstrained:{spec.Exposure.Key}:{fineVolume:R}>")
        select new MixProportion(
            CementKgM3: cement, WaterKgM3: water.KgM3, FineKgM3: fine, CoarseKgM3: coarse,
            AirFraction: water.AirFraction, WaterCement: water.KgM3 / cement,
            ExposureGoverned: cement > cementMethod || wcApplied < wcMethod);

    static Fin<(double KgM3, double AirFraction)> Cell(MixSpec spec);

    static Fin<double> InterpolatedWc(MixSpec spec);

    static Fin<double> InterpolatedCoarse(MixSpec spec);
}
```

## [05]-[ASSESSMENT_INPUT]

- Owner: NONE — the Materials folder authors NO assessment-input marshaller and NO `Assessment` node; the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected contract `Material` node IS the analysis input. `Properties/assessment#ASSESSMENT_RECORD` is the disjoint concern: it owns the DATED DECLARATION source — an in-situ result, a survey grade, a product EPD — that overrides a catalogue row before projection, never an input bag a consumer reads after it.
- Cases: zero — there is no input shape to model; `Rasm.Compute` reads the typed `MaterialPropertySet` cases off the graph and dispatches on `set.Discipline`, so a per-discipline input bag is the deleted form.
- Entry: `Rasm.Compute` reads the `Material` node plies DIRECTLY above the contract (`id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the relocated multi-ply `AssemblyAggregator` + the ISO/EN closed-form routes + the VividOrange/FE structural solvers), and writes the contract `Assessment` `Result` node back content-keyed on `(input key, route)`; the case→`Discipline` map is the contract's own `MaterialPropertySet.Discipline` accessor, so Compute selects its route with no parallel Materials marshaller.
- Boundary: a Materials-authored typed-input bag is redundant with Compute reading the typed cases off the graph, so the contract carries ONE property surface (the `Material` node) and never a parallel input node; the contract `Acoustic` case's intrinsic `Nrc`/`StcWeighted`/`SoundReductionIndexDb` folds are the single-material ratings Compute's ISO 12354 layered fold reads through the SAME `RatingContour.Fit` contour kernel, so the assembly STC and the material STC share one contour owner; the multi-ply aggregation is `Rasm.Compute`'s, this folder retaining only the single-material property SOURCE and crossing to Compute solely through the contract graph.

## [06]-[RESEARCH]

(none)
