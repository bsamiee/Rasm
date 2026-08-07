# [MATERIALS_PROPERTIES]

THE TYPED-ENGINEERING-PROPERTY SOURCE. This owner holds the estate's known-material physics: one `MaterialPropertyCatalogue` keying published mechanical, thermal, acoustic, fire, damping, hygrothermal, and optical data per `MaterialId`, and one `Admit` lowering that turns a published row into the seam's own typed cases. A material's engineering properties are never a per-discipline material type — one `Seq<MaterialPropertySet>` over one `MaterialId` carries conductivity, sound spectrum, fire rating, structural grade, damping ratio, sorption anchors, and glazing optics together. The boundary is exact: this page is the SOURCE and the seam is the CARRIER, so it re-mints no seam type, authors no aggregation, and crosses to `Rasm.Compute`/`Rasm.Bim` only through the seam graph. Multi-ply assembly aggregation is `Rasm.Compute`'s; a substance-level transmittance and a product-level resilient-layer stiffness are neither the catalogue's nor representable in it.

The typed property family is seam-owned: the `Rasm.Element` `MaterialPropertySet` class-root `[Union]` keyed to the one `Discipline` is the canonical carrier the `Material` node holds, `MeasureValue` the SI-coerced measure each dimensional column carries, and the intrinsic acoustic folds (`Nrc`/`Saa`/`StcWeighted`/`Rw` over the shared `RatingContour.Fit` kernel) live on the seam `Acoustic` case. This page DECLARES `Published<T>`, the ONE shared ingress carrier over `VividOrange.Uncertainties` that `Properties/sustainability#SUSTAINABILITY_PROPERTY` and `Properties/assessment#ASSESSMENT_RECORD` both COMPOSE. Every typed dimensional mint passes through the `Component/component#COMPONENT_OWNER` `QuantityRow` rows, and the EN-vendored mechanical columns DELEGATE per `SEED_ROW_LAW`: the `steel.s235`–`steel.s460` (+`steel.s450`, `metal.steel`) triples resolve through `EnSteelFactory.CreateBiLinear` over `EnSteelMaterial` × `EnSteelDeliveryCondition`, the six-member `EnRebarGrade` roster through `EnRebarFactory.CreateBiLinear`, each factory throw trapped ONCE at this boundary onto `Fin`. Hand rows keep only the non-vendor columns, and the no-EN-producer grades stay AUTHORED verbatim. The lifecycle `Environmental`/`Cost` cases lower from `Properties/sustainability#SUSTAINABILITY_PROPERTY`, the directional `Orthotropic` from `Component/timber#TIMBER_FAMILY`, and `Lookup` is the projector-facing resolution `Projection/component#COMPONENT_PROJECTOR` calls. Every seam admission and every trapped vendor throw rails ONE band — `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (2500), never the appearance `MaterialFault` 2450 of another concern.

## [01]-[INDEX]

- [02]-[MATERIAL_PROPERTY_CATALOGUE]: the shared `Published<T>` ingress carrier, the `VapourResistance` permeability class, the `MechanicalSource` vendor-delegation axis with its `NailWireClass` bending-yield band table, the `SubstancePhysics` shared family anchor, the `MaterialPropertyRow` ingress record, the registered-row database, the `Admit` row→seam-case lowering, and the memoized `Lookup` the projector calls.
- [03]-[DURABILITY_MIX]: the mix-keyed fib chloride-migration and ageing table, keyed on cement type × w/c because no strength class determines either, and the `Durability` resolution a project composes with its own exposure-class carbonation rate.
- [04]-[MIX_PROPORTION]: the EN 206 Annex F exposure-floor axis, the ACI 211.1-91 SI proportioning tables, and the one `MixDesign.Proportion` absolute-volume fold over a caller-declared `MixSpec`.
- [05]-[ASSESSMENT_INPUT]: why Materials authors NO assessment-input node — the material's `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the input `Rasm.Compute` reads off the graph directly.

## [02]-[MATERIAL_PROPERTY_CATALOGUE]

- Owner: `Published<T>` the ONE shared evidence-bearing uncertainty carrier all three Properties owners ride; `VapourResistance` the closed permeability class; `MechanicalSource` the closed mechanical-column source axis (`Authored` / `NailWire` / `EnSteel` / `EnRebar`); `NailWireClass` the ASTM F1667-S1 bending-yield band table; `SubstancePhysics` the shared family anchor a roster row references; `MaterialPropertyRow` the published-data ingress record; `MaterialPropertyCatalogue` the registered-row database; `Admit` the row→seam-case lowering; `Lookup` the projector-facing resolution.
- Cases: one `MaterialPropertyRow` shape across all materials — a `SubstancePhysics` anchor (density, Poisson, expansion, conductivity, specific heat, vapour class, fire declaration, design damping) plus the row's own `MechanicalSource`, and the optional acoustic, hygrothermal, and optical declarations only a characterized substance carries; `Admit` produces a `Seq<MaterialPropertySet>` of the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Damping`/`Hygrothermal`/`Optical` cases — each a `MaterialPropertySet` over a `MaterialId`, never a property subtype.
- Law: AUTHORED transcriptions carry the catalogue's relative band and code-REGISTERED values cross EXACT — `Published.Of` admits through the `VividOrange` relative factory, `Published.Exact` mints the zero-width datum, and the seam's own EXACTNESS-IS-BAND-ABSENCE rule then answers `None` at the `Measure` mint rather than offering `MeasureBand.Admit` a zero-width band that refuses its own re-admission. A DELEGATED vendor value never wears a fabricated spread.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key)` — resolves the `MechanicalSource` (the stored MPa triple, the F1667-S1 band read, or the vendor build with its throw trapped onto `ElementFault.ValueRejected` via the kernel `Op.Catch` funnel), mints every dimensional column through the `QuantityRow` typed-mint rows with the `Published<T>.Band` provider-model→`MeasureBand` lowering, passes the scalar columns central-only (the seam guards Poisson `[0,0.5]`, μ `>= 1`, ζ `[0,1)`, the isotherm `wf >= w80`, the optical conservation refinements), folds the acoustic declaration through the six-arg seam `Acoustic.Of` gate, and the fire declaration through `FireRating.Parse` + the generated `SmokeClass.TryGet`/`DropletClass.TryGet` + the three-criterion `FireResistance` ctor. Only the `Strength` resolution BINDS; the seven discipline groups and the three fire tokens are INDEPENDENT and ACCUMULATE applicatively, so a row with several rejected columns faults them ALL in one `Fin.Fail` `ManyErrors`. `MaterialPropertyCatalogue.Lookup(MaterialId id, Op key)` reads the memoized admitted catalogue, faulting `ElementFault.ValueRejected` for an unregistered material — one polymorphic resolution, never a `GetMechanical`/`GetThermal` family.
- Packages: Rasm.Element (project — `MaterialPropertySet` + its `Of*` admissions, `MeasureValue.OfSi`/`WithUncertainty`, `MeasureBand.Admit`, `UncertaintyKind`, `PropertyEvidence`, `FireRating.Parse`, `SmokeClass`/`DropletClass`, `FireResistance`, `Acoustic.Of`, `Discipline`, `MaterialId`, `ElementFault.ValueRejected`), Rasm.Materials.Component (project — the `QuantityRow` typed-mint owner and its `OfNative` mint), VividOrange.Uncertainties + VividOrange.Uncertainties.Quantities (the four uncertainty models over the `double` and `IQuantity` carriers, the fluent `WithRelativeUncertainty`/`WithAbsoluteUncertainty`/`WithIntervalUncertainty` admissions, the `IntervalUncertaintyQuantity<TQuantity>` carrier the dimensional interval arm mints directly, the `IUncertainty<T>` kind interfaces), VividOrange.Materials (`EnSteelFactory`/`EnRebarFactory`/`EnSteelMaterial`/`EnSteelGrade`/`EnRebarGrade`/`EnSteelDeliveryCondition`/`IBiLinearMaterial`), VividOrange.Standards (`NationalAnnex`), UnitsNet (`Density`/`Pressure`/`ThermalConductivity`/`SpecificEntropy`/`Length` — raw-to-SI coercion at this boundary only), NodaTime (`LocalDate` evidence expiry), Rasm (project — `Op` + the `Op.Catch` trap funnel), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option` + `Match`/`Map`), BCL inbox (`FrozenDictionary`, `Lazy<T>`, `ReadOnlyMemory<double>`, `ImmutableArray<T>`).
- Growth: a new engineering property shared across materials is one column on the matching seam case the row gains a published column for and `Admit` lowers; a new known material is one `Rows` entry naming its `SubstancePhysics` anchor (the roster grows by row to thousands with no seam touch, and a corrected family figure is one anchor edit rather than a hundred-row sweep); a new vendor grade table or published-yield convention is one `MechanicalSource` case plus one `Strength` arm, compiler-forced at the generated `Switch`; a new nail-wire class or diameter band is one `NailWireClass` row or one band entry; a new property discipline is one seam case — the `Damping`/`Hygrothermal`/`Optical` cases landed exactly this way and this catalogue sources three of them.
- Boundary: `MaterialPropertyRow` is the published-DATA ingress, NOT a parallel domain union — the seam `MaterialPropertySet` is the one typed carrier and `Admit` the one `BOUNDARY_ADMISSION`, so the row and every declaration beside it stay `internal` and the public surface is `Admit`/`Lookup` alone, both answering the ADMITTED set the analytics projection also folds. The dimensional columns coerce to SI through `UnitsNet` reads inside the `QuantityRow`-typed mint, the provider uncertainty models lower to neutral `MeasureBand` bounds at exactly that mint, and provider types never cross into `Rasm.Element`. A SUBSTANCE HAS NO TRANSMITTANCE: the seam `Thermal` case carries a U-value column, this mint answers the substance's conductance at UNIT thickness — numerically λ, carrying no thickness at all — and the EN ISO 6946 assembly fold in `Rasm.Compute` owns every real U-value over a declared buildup. A SUBSTANCE HAS NO DYNAMIC STIFFNESS either: EN 29052-1 `s'` is an installed-assembly quantity whose airflow term is pure geometry, measured to differ across thicknesses of one declared product, so the seam `Acoustic.Of` optional slot stays absent from every roster row and a product-keyed source is the only honest home. The vendor factories are exception-throwing at their derivation boundary (`ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException`) so `Strength` traps them ONCE via `Op.Catch` onto the SAME band every seam admission rails — a mixed-band `Admit` chain is the rejected cross-concern leak. The lowered `Seq<MaterialPropertySet>` is what `Projection/component#COMPONENT_PROJECTOR` writes onto the seam `Material` node, and no uncertainty value routes a VividOrange serializer, the canonical Rasm codec owning every wire; the `Optical`/`Hygrothermal` column sets align one-to-one with the standard IFC material property sets `Rasm.Bim` emits FROM that graph, so the alignment lives at the seam and neither side transcribes the other's member names. SUBSTANCE-ID CLOSURE is a hard invariant: every `Component.SubstanceId` a seed page mints resolves a row here, a seed-keyed id with no row being a projection-time `Lookup` fault, so a new seed substance lands with its row in the same campaign; a ply-cavity, stud-appearance, or adhesive-appearance id is NOT a substance key and never routes this catalogue.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Threading;                               // LazyThreadSafetyMode — the admitted-catalogue publication mode
using LanguageExt;
using LanguageExt.Common;                             // Error — the Validation slot the applicative discipline-group join accumulates
using NodaTime;                                       // LocalDate — the vendor evidence expiry PropertyEvidence carries
using Rasm.Domain;                                    // Op + the Op.Catch boundary trap funnel
using Rasm.Element.Composition;                       // MaterialId, MaterialPropertySet, MeasureValue, MeasureBand, UncertaintyKind,
using Rasm.Element.Projection;                        // PropertyEvidence, FireRating, SmokeClass, DropletClass, FireResistance, Acoustic (Composition);
using Rasm.Element.Properties;                        // ElementFault — band 2500, the ONE band this SOURCE rails (Projection)
using Rasm.Materials.Component;                       // QuantityRow — the one typed-mint owner and its OfNative railed mint
using Thinktecture;                                   // [Union], [SmartEnum<string>], ComparerAccessors
using UnitsNet;
using VividOrange.Materials;                          // IBiLinearMaterial — the E/f_y/f_u law the EN factories return
using VividOrange.Materials.StandardMaterials.En;     // EnSteelGrade, EnSteelMaterial, EnSteelFactory, EnSteelDeliveryCondition, EnRebarGrade, EnRebarFactory
using VividOrange.Standards.Eurocode;                 // NationalAnnex (the EN factory annex axis; Table 3.1 strengths are annex-independent)
using VividOrange.Uncertainties;                      // IUncertainty<T> + INormalDistributionUncertainty<T>
using VividOrange.Uncertainties.Quantities;           // IntervalUncertaintyQuantity<TQuantity> — the quantity carrier with no fluent admission
using VividOrange.Uncertainties.Quantities.Utility;   // (TQuantity).WithRelativeUncertainty / .WithAbsoluteUncertainty
using VividOrange.Uncertainties.Utility;              // (double).WithRelativeUncertainty / .WithAbsoluteUncertainty / .WithIntervalUncertainty
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // the property-catalogue folder owner — the projector imports Rasm.Materials.Properties

// --- [TYPES] -------------------------------------------------------------------------------
// PERMEABILITY IS A CLASS BEFORE IT IS A MAGNITUDE. EN ISO 10456 tabulates metals and glass as vapour-tight, and a
// tight substance has no μ a band could wrap — the finite sentinel this replaces published a fabricated magnitude
// wearing the catalogue's ±5% transcription band, so every consumer read an invented number with invented spread.
// Factor carries the EN ISO 13788 μ the porous rows genuinely publish.
[Union]
public abstract partial record VapourResistance {
    public sealed record Impermeable : VapourResistance;
    public sealed record Factor(double Mu) : VapourResistance;
}

// The SEED_ROW_LAW mechanical-column source axis: an EN grade with an admitted vendor producer DELEGATES its
// E/f_y/f_u to the factory table (hand re-transcriptions of vendor-owned values DELETE); a grade with no producer
// stays Authored; nail wire takes its own case because its published yield is a BENDING datum banded on diameter
// rather than a tensile column any triple can hold. Growth: one case plus one Strength arm — compiler-forced.
[Union]
public abstract partial record MechanicalSource {
    public sealed record Authored(double YoungsMpa, double YieldMpa, double UltimateMpa) : MechanicalSource;
    public sealed record NailWire(NailWireClass Class, double ShankDiameterIn, double UltimateMpa) : MechanicalSource;
    public sealed record EnSteel(EnSteelGrade Grade, EnSteelDeliveryCondition Delivery) : MechanicalSource;   // EN 1993-1-1 Table 3.1 (delivery selects the AR/N/M/Q sub-table)
    public sealed record EnRebar(EnRebarGrade Grade) : MechanicalSource;                                      // EN 1992-1-1 §3.2 f_yk × ductility-k + E_s 200 GPa

    // The MPa print order every standards table uses — E, then f_y, then f_u — as ONE named mint, so a roster row
    // never spells a bare run of magnitudes whose order a reader must recover from the values themselves.
    public static MechanicalSource Mpa(double youngs, double yieldStrength, double ultimate) =>
        new Authored(youngs, yieldStrength, ultimate);
}

// --- [NAIL_WIRE_YIELD]
// ASTM F1667 Supplementary Requirement S1 publishes the nail-wire yield as a BENDING datum: a three-point flexure
// value at the 5%-of-diameter offset per ASTM F1575, back-calculated on the shank diameter. It is NOT a tensile
// yield and legitimately EXCEEDS the wire's tensile strength, and it is BANDED on diameter rather than published as
// a substance constant — ASTM A853 publishes tensile only and ASTM A510 publishes no grade table at all, so no
// "read the yield off the wire grade" route exists to take. The band bounds follow NDS Table 12.3.1B where the two
// sources disagree at a boundary.
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

    // The band is HALF-OPEN on its upper bound so two adjacent published bands never both claim one diameter, and
    // the topmost band closes so the table's own published ceiling resolves. A diameter outside every band answers
    // absence rather than the nearest band: extrapolating a flexure value past the diameters the standard tested
    // publishes a strength nobody measured on a nail nobody bent.
    public Option<NailWireBand> At(double shankDiameterIn) =>
        Optional(Bands.FirstOrDefault(band => shankDiameterIn >= band.MinDiameterIn
            && (shankDiameterIn < band.MaxDiameterIn || shankDiameterIn == Bands[^1].MaxDiameterIn)));
}

// --- [MODELS] ------------------------------------------------------------------------------
// THE shared Published ingress carrier ALL THREE Properties owners ride (declared HERE, composed by
// Properties/sustainability#SUSTAINABILITY_PROPERTY and Properties/assessment#ASSESSMENT_RECORD): one
// evidence-bearing uncertainty datum over both VividOrange carriers — IUncertainty<TQuantity> dimensional rows and
// IUncertainty<double> scalar rows. Kind is DECLARED by the minting factory rather than re-derived by probing the
// carrier's interfaces: the four model interfaces do not partition the static type, so an interface probe needs an
// arm no admission can reach, and the kind is a fact the admission already decided. Normal carries the one model
// whose band needs more than two bounds, so Band is total with no cast and no fallthrough. The Published family
// below is the SOLE mint of this struct.
public readonly record struct Published<T>(
    IUncertainty<T> Value,
    UncertaintyKind Kind,
    Option<INormalDistributionUncertainty<T>> Normal,
    PropertyEvidence Evidence) {
    // MeasureBand.Interval and MeasureBand.Normal are seam-INTERNAL, so Admit is the one public band entry a
    // sibling assembly reaches and the lowering is RAILED. Scale is the QuantityRow's own factor, applied to the
    // bounds and the deviation alike so a band and the magnitude it wraps never sit on two scales.
    public Fin<MeasureBand> Band(Func<T, double> si, double scale, Op key) =>
        MeasureBand.Admit(
            Kind,
            si(Value.LowerBound) * scale,
            si(Value.UpperBound) * scale,
            Normal.Map(normal => si(normal.StandardDeviation) * scale),
            Normal.Map(static normal => normal.CoverageFactor),
            key);
}

// The Of family discriminates on CARRIER (MODAL_ARITY): a UnitsNet quantity admits through the .Quantities.Utility
// factories, a raw double through the double .Utility. Exact is the EVIDENCE-first arm — a code-registered
// characteristic value (an EN 1993-1-1 Table 3.1 f_y, a printed EN 1992-1-1 Ecm) IS the normative constant, and
// inventing a ±5% distribution around it fabricates a spread the standard does not publish, so the arm takes no
// confidence argument. Interval is the arm a genuinely min/max-published column takes; the quantity carrier has no
// fluent interval admission, so it mints its concrete IntervalUncertaintyQuantity directly rather than routing a
// published pair through a relative factory that would recompute the bounds it was given.
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

    // Central is ONE member name over both carriers — the scalar block reads the raw double and the dimensioned
    // block returns the QUANTITY, so a consumer selects its SI unit off the quantity's own accessor instead of
    // re-deriving a scale. `double` satisfies no IQuantity, so the two never overlap.
    extension(Published<double> datum) {
        public double Central => datum.Value.CentralValue;
    }

    extension<TQuantity>(Published<TQuantity> datum) where TQuantity : IQuantity {
        public TQuantity Central => datum.Value.CentralValue;
    }
}

// The physics a whole SUBSTANCE FAMILY shares, hoisted exactly as the sibling lifecycle roster hoists EcoProfile: a
// carbon-steel row and an EN 338 softwood row each carry eight columns that vary by family and not by grade, so the
// anchor travels once and a row spells the anchor plus the columns that genuinely move. A corrected family figure
// is then a one-line edit rather than a hundred-row sweep whose one missed row is a silent divergence, and the
// positional run of bare magnitudes a roster row previously opened with — where a transposed pair type-checks and
// publishes a material with another material's expansion — is unspellable.
internal readonly record struct SubstancePhysics(
    double DensityKgM3,
    double PoissonsRatio,
    double ExpansionPerK,
    double ConductivityWMK,
    double SpecificHeatJKgK,
    VapourResistance Vapour,
    Option<FireDeclaration> Fire,
    Option<double> DampingRatio);

// EN 13501-1 reaction plus the three EN 13501-2 criteria. The criteria are OPTIONAL because an untested criterion
// and a zero-minute one are different facts: a reaction-class-only datasheet declares R/E/I on NOTHING, and
// zero-filling it publishes a material rated to fail instantly on every criterion — evidence nobody measured,
// reaching the design report as a number.
internal readonly record struct FireDeclaration(
    string Reaction,
    string Smoke,
    string Droplets,
    Option<int> LoadBearingMinutes = default,
    Option<int> IntegrityMinutes = default,
    Option<int> InsulationMinutes = default);

// The acoustic ingress group: the two eighteen-band vectors RAW (per-band uncertainty wrapping was unread ceremony
// — only centrals cross the seam) plus the seam Acoustic.Of intrinsics a SUBSTANCE can carry. Dynamic stiffness is
// absent by construction and named on the card, not omitted by oversight.
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

// The published engineering data for one material — pure DATA, lifted only at Admit. The prior shape banded every
// column in its own constructor and Admit then read the centrals straight back out, so each row allocated ten
// uncertainty carriers to answer ten doubles; banding now happens once, at the mint that consumes it.
internal sealed record MaterialPropertyRow(
    SubstancePhysics Physics,
    MechanicalSource Mechanical,
    Option<AcousticDeclaration> Acoustic = default,
    Option<HygrothermalDeclaration> Hygrothermal = default,
    Option<OpticalDeclaration> Optical = default,
    PropertyEvidence Evidence = default);

// The mechanical triple an admission resolved, carrying the evidence its SOURCE earned: a delegated row speaks for
// the vendor table it read, an authored row for the catalogue transcription.
internal readonly record struct StrengthTriple(
    Published<Pressure> Youngs,
    Published<Pressure> Yield,
    Published<Pressure> Ultimate,
    PropertyEvidence Evidence);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialPropertyCatalogue {
    // The <=40 mm Table 3.1 band the substance datum reads (the roster's documented t <= 16 mm nominal);
    // per-section thickness banding stays Component/steel#STEEL_FAMILY YieldMpa — never re-solved here.
    const double GradeThicknessMm = 16.0;

    // The relative-confidence profile the AUTHORED columns read — ONE band, because a datasheet's transcription
    // confidence is a property of the SOURCE and not of the discipline it lands in. DELEGATED columns carry no band
    // at all: a registered EN table value is exact.
    const double AuthoredBand = 0.05;

    // The memo fold's own op: the once-per-process admission is a CATALOGUE event with no caller behind it.
    static readonly Op AdmitKey = Op.Of(name: "material-property-catalogue-admit");

    static readonly PropertyEvidence SteelTable = new("vendor", "en 1993-1-1 table 3.1 / vividorange.materials", Option<LocalDate>.None);
    static readonly PropertyEvidence RebarTable = new("vendor", "en 1992-1-1 §3.2 + en 10080 / vividorange.materials", Option<LocalDate>.None);
    static readonly PropertyEvidence NailWireTable = new("vendor", "astm f1667 s1 / astm f1575 via nds 12.3.1b", Option<LocalDate>.None);

    // The empty discipline-group slot an absent optional group contributes to the applicative join.
    static readonly Validation<Error, Seq<MaterialPropertySet>> NoGroup = Success<Error, Seq<MaterialPropertySet>>(Seq<MaterialPropertySet>());

    // Lowers a published row into the seam cases. EVERY value flows through a seam admission or the trapped vendor
    // build, so the WHOLE chain rails ONE band — ElementFault.ValueRejected (2500). Strength is mechanical's ONE
    // dependency and binds first; the seven discipline groups are INDEPENDENT and accumulate APPLICATIVELY — the
    // seam's own VALIDATION_MONOID shape one level up, so a curated row with a bad fire token AND a bad optical
    // fraction reports BOTH in one Fin.Fail (ManyErrors), never first-fault-only. Poisson/μ/ζ/sorption/optics cross
    // central-only (the seam guards range and refinement: Poisson [0,0.5], μ >= 1, ζ [0,1), wf >= w80, per-band
    // τ+ρ <= 1). Damping passes no Rayleigh pair — the (α, β) calibration is a per-model FE input, never a
    // catalogue datum.
    internal static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key) =>
        Strength(row.Mechanical, key).Bind(strength =>
            (Mechanical(row, strength, key).ToValidation(),
             Thermal(row, key).ToValidation(),
             row.Acoustic.Match(
                 None: static () => NoGroup,
                 Some: a => Acoustic.Of(a.Absorption, a.Sri, key, flowResistivity: a.FlowResistivityPaSPerM2, lossFactor: a.LossFactor)
                     .Map(spectrum => Seq(MaterialPropertySet.OfAcoustic(spectrum, row.Evidence))).ToValidation()),
             row.Physics.Fire.Match(
                 None: static () => NoGroup,
                 // FireRating.Parse is the seam's ONE fire-reaction admission; SmokeClass/DropletClass expose NO
                 // Parse, so the generated TryGet resolves. The three tokens and the independent R/E/I minutes
                 // accumulate before the admitted FireResistance reaches the total OfFire constructor.
                 Some: f => (FireRating.Parse(f.Reaction, key).ToValidation(),
                             Sub(SmokeClass.TryGet, f.Smoke, key, "smoke").ToValidation(),
                             Sub(DropletClass.TryGet, f.Droplets, key, "droplet").ToValidation(),
                             // FireResistance.Of REFUSES an all-absent triple by design, so a reaction-class-only
                             // row lands the seam's own unclassified carrier rather than railing the whole material.
                             (f.LoadBearingMinutes.IsNone && f.IntegrityMinutes.IsNone && f.InsulationMinutes.IsNone
                                 ? Fin.Succ(FireResistance.None)
                                 : FireResistance.Of(f.LoadBearingMinutes, f.IntegrityMinutes, f.InsulationMinutes, key)).ToValidation())
                     .Apply((reaction, smoke, droplets, resistance) => Seq(MaterialPropertySet.OfFire(reaction, smoke, droplets, resistance, row.Evidence))).As()),
             row.Physics.DampingRatio.Match(
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

    // The density column carries the AUTHORED transcription band even on a DELEGATED row, because no EN factory
    // publishes a density — the vendor delegation covers the elastic triple alone.
    static Fin<MaterialPropertySet> Mechanical(MaterialPropertyRow row, StrengthTriple strength, Op key) =>
        from density in Measure(Published.Of(UnitsNet.Density.FromKilogramsPerCubicMeter(row.Physics.DensityKgM3), AuthoredBand, row.Evidence),
                                static q => q.KilogramsPerCubicMeter, QuantityRow.Density, key)
        from youngs in Measure(strength.Youngs, static q => q.Pascals, QuantityRow.Pressure, key)
        from proof in Measure(strength.Yield, static q => q.Pascals, QuantityRow.Pressure, key)
        from ultimate in Measure(strength.Ultimate, static q => q.Pascals, QuantityRow.Pressure, key)
        from set in MaterialPropertySet.OfMechanical(density, youngs, proof, ultimate, row.Physics.PoissonsRatio, row.Physics.ExpansionPerK, key, strength.Evidence)
        select set;

    // The U-value the seam column takes is the substance's conductance at UNIT THICKNESS — numerically λ, by
    // definition — because a substance has no thickness and therefore no transmittance. The prior column authored
    // λ/d for a d no row declared, so a hundred rows each published a transmittance for an imaginary buildup that
    // no consumer could use and every row had to invent. Vapour lowers through the permeability class: a tight
    // substance is an unbounded factor, which the seam's μ >= 1 gate admits and every diffusion fold reads as the
    // zero permeance it is.
    static Fin<MaterialPropertySet> Thermal(MaterialPropertyRow row, Op key) =>
        from conductivity in Measure(Published.Of(ThermalConductivity.FromWattsPerMeterKelvin(row.Physics.ConductivityWMK), AuthoredBand, row.Evidence),
                                     static q => q.WattsPerMeterKelvin, QuantityRow.ThermalConductivity, key)
        from specificHeat in Measure(Published.Of(SpecificEntropy.FromJoulesPerKilogramKelvin(row.Physics.SpecificHeatJKgK), AuthoredBand, row.Evidence),
                                     static q => q.JoulesPerKilogramKelvin, QuantityRow.SpecificEntropy, key)
        from unitThickness in QuantityRow.HeatTransferCoefficient.OfNative(row.Physics.ConductivityWMK)
        from set in MaterialPropertySet.OfThermal(conductivity, specificHeat, unitThickness, VapourFactor(row.Physics.Vapour), key, row.Evidence)
        select set;

    static double VapourFactor(VapourResistance vapour) => vapour.Switch(
        impermeable: static _ => double.PositiveInfinity,
        factor: static f => f.Mu);

    // The QuantityRow-typed banded mint. EXACTNESS IS BAND ABSENCE at the seam — a zero-width band refuses its own
    // re-admission through the WithUncertainty contains-the-nominal gate — so an Exact datum mints the bare
    // magnitude and only a genuinely spread datum reaches MeasureBand.Admit. The si projections at every call site
    // are static lambdas, so the mint allocates no closure per column per material.
    static Fin<MeasureValue> Measure<TQuantity>(Published<TQuantity> datum, Func<TQuantity, double> si, QuantityRow row, Op key) where TQuantity : IQuantity =>
        row.OfNative(si(datum.Value.CentralValue)).Bind(measure => datum.Kind == UncertaintyKind.Exact
            ? Fin.Succ(measure)
            : datum.Band(si, row.Scale, key).Bind(band => measure.WithUncertainty(band, key)));

    // The SEED_ROW_LAW dispatch (one exhaustive generated Switch). Authored bands the stored triple at the
    // catalogue transcription confidence — the ONE site where the authored-uncertainty law becomes a value. NailWire
    // reads the F1667-S1 band and crosses EXACT, its Youngs the ASTM A510 wire modulus and its Ultimate the row's
    // own published tensile. The EN arms build the grade record and read the vendor law, the factory's throws
    // (ArgumentException, MissingNationalAnnexException, InvalidSteelSpecificationException) trapped ONCE via the
    // kernel Op.Catch funnel onto the page's one band. Table 3.1 strengths are annex-independent, so the
    // RecommendedValues pin only satisfies construction; the delivery condition routes the AR/N/M/Q sub-table that
    // holds the grade (AR/EN 10025-2 holds S235/S275/S355/S450; N/EN 10025-3 holds S420/S460; the Q/EN 10025-6
    // sub-table holds only S460 — EnSteelGrade tops out at S460, so S690 has no producer and stays AUTHORED). The
    // spec default HollowSection=false pins the non-hollow tables, making the factory's "hollow section not set"
    // throw unreachable from this build.
    static Fin<StrengthTriple> Strength(MechanicalSource source, Op key) =>
        source.Switch(
            state: key,
            authored: static (k, a) => Fin.Succ(new StrengthTriple(
                Published.Of(Pressure.FromMegapascals(a.YoungsMpa), AuthoredBand, PropertyEvidence.Catalogue),
                Published.Of(Pressure.FromMegapascals(a.YieldMpa), AuthoredBand, PropertyEvidence.Catalogue),
                Published.Of(Pressure.FromMegapascals(a.UltimateMpa), AuthoredBand, PropertyEvidence.Catalogue),
                PropertyEvidence.Catalogue)),
            nailWire: static (k, n) => n.Class.At(n.ShankDiameterIn).Match(
                Some: band => Fin.Succ(new StrengthTriple(
                    Published.Exact(Pressure.FromMegapascals(NailWireModulusMpa), NailWireTable),
                    Published.Exact(Pressure.FromMegapascals(band.FybMpa), NailWireTable),
                    Published.Exact(Pressure.FromMegapascals(n.UltimateMpa), NailWireTable),
                    NailWireTable)),
                None: () => ElementFault.ValueRejected(k, $"<nail-wire-diameter-unbanded:{n.Class.Key}:{n.ShankDiameterIn:R}>")),
            enSteel: static (k, s) => k.Catch(() => {
                    EnSteelMaterial material = new(s.Grade, NationalAnnex.RecommendedValues);
                    material.Specification.DeliveryCondition = s.Delivery;
                    return Fin.Succ(EnSteelFactory.CreateBiLinear(material, Length.FromMillimeters(GradeThicknessMm)));
                })
                .MapFail(error => ElementFault.ValueRejected(k, $"<en-steel-grade:{s.Grade}:{s.Delivery}:{error.Message}>"))
                .Map(law => Delegated(law, SteelTable)),
            enRebar: static (k, r) => k.Catch(() => Fin.Succ(EnRebarFactory.CreateBiLinear(r.Grade)))
                .MapFail(error => ElementFault.ValueRejected(k, $"<en-rebar-grade:{r.Grade}:{error.Message}>"))
                .Map(law => Delegated(law, RebarTable)));

    // A DELEGATED column is a code-REGISTERED characteristic value the vendor factory returned, so it crosses EXACT:
    // banding an EN 1993-1-1 Table 3.1 f_y at a fabricated ±5% invents a distribution around a normative constant
    // the standard publishes without one.
    static StrengthTriple Delegated(IBiLinearMaterial law, PropertyEvidence evidence) =>
        new(Published.Exact(law.ElasticModulus, evidence),
            Published.Exact(law.YieldStrength, evidence),
            Published.Exact(law.UltimateStrength, evidence),
            evidence);

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
    // structural family (welded steel + aluminium 0.02, RC/masonry/stone 0.05, timber 0.08), the permeability
    // classes, and the ASTM A510 hard-drawn wire modulus — spelling anchors only, every row VALUE verbatim.
    const double NailWireModulusMpa = 200_000.0;

    static readonly VapourResistance Impermeable = new VapourResistance.Impermeable();
    static VapourResistance Mu(double factor) => new VapourResistance.Factor(factor);

    static readonly Option<FireDeclaration> FireA1 = Some(new FireDeclaration("A1", "", ""));
    static readonly Option<FireDeclaration> FireA1Ei120 = Some(new FireDeclaration("A1", "", "", IntegrityMinutes: Some(120), InsulationMinutes: Some(120)));
    static readonly Option<FireDeclaration> FireA2 = Some(new FireDeclaration("A2", "s1", "d0"));
    static readonly Option<FireDeclaration> FireB = Some(new FireDeclaration("B", "s1", "d0"));
    static readonly Option<FireDeclaration> FireC = Some(new FireDeclaration("C", "s1", "d0"));
    static readonly Option<FireDeclaration> FireD = Some(new FireDeclaration("D", "s2", "d0"));
    static readonly Option<FireDeclaration> FireD30 = Some(new FireDeclaration("D", "s2", "d0", Some(30), Some(30), Some(30)));
    static readonly Option<FireDeclaration> FireE = Some(new FireDeclaration("E", "s2", "d0"));

    static readonly Option<double> ZSteel = Some(0.02);
    static readonly Option<double> ZConcrete = Some(0.05);
    static readonly Option<double> ZTimber = Some(0.08);
    static readonly Option<double> NoDamping = Option<double>.None;

    // The shared SUBSTANCE-FAMILY physics anchors. Density/λ/μ move within several families, so a row re-anchors
    // exactly the columns its standard prints differently and every unnamed column is the family's by construction.
    static readonly SubstancePhysics CarbonSteel   = new(7850.0, 0.30, 12.0e-6, 50.0, 460.0, Impermeable, FireA1, ZSteel);
    static readonly SubstancePhysics CastIron      = CarbonSteel with { DensityKgM3 = 7200.0, PoissonsRatio = 0.28, ExpansionPerK = 11.0e-6 };
    static readonly SubstancePhysics Austenitic    = new(8000.0, 0.30, 16.0e-6, 15.0, 500.0, Impermeable, FireA1, ZSteel);      // EN 10088 austenitic block
    static readonly SubstancePhysics Duplex        = Austenitic with { DensityKgM3 = 7800.0, ExpansionPerK = 13.0e-6 };
    static readonly SubstancePhysics Aluminium     = new(2700.0, 0.33, 23.0e-6, 167.0, 900.0, Impermeable, FireB, ZSteel);
    static readonly SubstancePhysics Thermoset     = new(1200.0, 0.35, 60.0e-6, 0.20, 1000.0, Mu(10_000.0), FireE, NoDamping);  // a bond line is not an EN 1998-1 ζ family
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

    // The structural-materials roster — every row a published EN/ASTM/CSA datasheet keyed by the canonical
    // MaterialId (seam-generated ordinal-ignore-case equality keys the table), in EXACT MaterialId parity with the
    // Properties/sustainability EPD roster, a parity the sibling catalogue's type-init census PROVES rather than
    // asserts. Mechanical columns: DELEGATED rows read EN 1993-1-1 Table 3.1 or EN 1992-1-1 §3.2 at Admit; AUTHORED
    // rows store the published CHARACTERISTIC values (EN 1993-1-4 Table 2.1 stainless, EN 1992-1-1 Table 3.1
    // fck/fcm/Ecm printed values, EN 338/14080 fm,k, EN 1999-1-1 f0/fu, ASTM A615/A706 + CSA G30.18 fy/fu_min with
    // the ACI 318 §20.2.2.2 E_s 200 GPa, AISC A36/A992/A572 with E 200 GPa, ASTM C90/TMS 402 f'm + E_m = 900·f'm).
    // Thermal: EN ISO 10456 design λ + the EN ISO 13788 vapour factor μ. Acoustic: the eighteen-band absorption and
    // field-incidence SRI vectors only the acoustically-characterized rows carry, the porous rows carrying the
    // EN 29053 flow resistivity the Delany-Bazley route reads. Fire: EN 13501-1 reaction + EN 13501-2 R/E/I minutes
    // where a slab rating is published — resistance is otherwise an assembly property Rasm.Compute computes over
    // the buildup. Hygrothermal: the WUFI/Fraunhofer sorption anchors. Optical: the EN 410 published record.
    internal static readonly FrozenDictionary<MaterialId, MaterialPropertyRow> Rows = new (MaterialId Id, MaterialPropertyRow Row)[] {
        // --- structural carbon steel (EN 10025-2/-3; DELEGATED — Table 3.1 <=40 mm: S235 235/360, S275 275/430,
        //     S355 355/490, S450 440/550 on AR; S420 420/520, S460 460/540 on N; E 210 GPa the factory law)
        (MaterialId.Of("steel.s235"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S235, EnSteelDeliveryCondition.AR))),
        (MaterialId.Of("steel.s275"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S275, EnSteelDeliveryCondition.AR))),
        (MaterialId.Of("steel.s355"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S355, EnSteelDeliveryCondition.AR))),
        (MaterialId.Of("steel.s420"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S420, EnSteelDeliveryCondition.N))),
        // steel.s450 — the EN 10025-2 grade the Component/steel#STEEL_FAMILY SteelGrade.S450 SubstanceId keys
        (MaterialId.Of("steel.s450"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S450, EnSteelDeliveryCondition.AR))),
        (MaterialId.Of("steel.s460"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S460, EnSteelDeliveryCondition.N))),
        // EN 10025-6 quenched-and-tempered S690QL — outside Table 3.1, no factory producer, AUTHORED
        (MaterialId.Of("steel.s690"), new(CarbonSteel, MechanicalSource.Mpa(210_000.0, 690.0, 770.0))),
        // metal.steel — the generic-structural-steel alias Component.SubstanceId resolves on an unspecified grade:
        // the conservative S235 baseline DELEGATED through the same factory row, so the connection-design seam reads
        // a real Mechanical row rather than faulting; a graded connector keys steel.s355 directly.
        (MaterialId.Of("metal.steel"), new(CarbonSteel, new MechanicalSource.EnSteel(EnSteelGrade.S235, EnSteelDeliveryCondition.AR))),
        // metal.iron — the cast/wrought-iron generic the Component/joint weld family keys; ductile EN-GJS-400-15
        (MaterialId.Of("metal.iron"), new(CastIron, MechanicalSource.Mpa(170_000.0, 250.0, 400.0))),
        // --- AISC structural steel (ASTM A36 250/400, A992 345/450, A572 Gr50 345/450; E 200 GPa AISC — no EN
        //     producer, FLOOR_SCOPE_GATE, AUTHORED) — the Component/steel#STEEL_FAMILY SubstanceId rows
        (MaterialId.Of("steel.a36"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 250.0, 400.0))),
        (MaterialId.Of("steel.a992"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 345.0, 450.0))),
        (MaterialId.Of("steel.a572"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 345.0, 450.0))),
        // --- reinforcing steel (the FULL six-member EnRebarGrade roster DELEGATED: fyk parsed from the grade digits,
        //     fu = k·fyk by ductility class A 1.05 / B 1.08 / C 1.15, E_s 200 GPa — the EN 1992-1-1 §3.2.7 law;
        //     B450A/C the Italian NAD grades, B550B the Scandinavian. ASTM A615/A706 + CSA G30.18 have no EN
        //     producer and stay AUTHORED per the spec tables at E_s 200 GPa.)
        (MaterialId.Of("steel.b450a"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B450A))),
        (MaterialId.Of("steel.b450c"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B450C))),
        (MaterialId.Of("steel.b500a"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B500A))),
        (MaterialId.Of("steel.b500b"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B500B))),
        (MaterialId.Of("steel.b500c"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B500C))),
        (MaterialId.Of("steel.b550b"), new(CarbonSteel, new MechanicalSource.EnRebar(EnRebarGrade.B550B))),
        (MaterialId.Of("steel.gr40"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 280.0, 420.0))),
        (MaterialId.Of("steel.gr60"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 420.0, 620.0))),
        (MaterialId.Of("steel.gr75"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 520.0, 690.0))),
        (MaterialId.Of("steel.gr80"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 550.0, 725.0))),
        (MaterialId.Of("steel.gr60w"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 420.0, 550.0))),
        (MaterialId.Of("steel.gr80w"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 550.0, 690.0))),
        (MaterialId.Of("steel.400w"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 400.0, 540.0))),
        (MaterialId.Of("steel.500w"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 500.0, 620.0))),
        // --- cold-formed sheet + fasteners (AISI SS Grade 33/50 sheet; ISO 898-1/SAE J429/ASTM F3125 bolt classes)
        //     — the Component connector Gauge / fastener Grade SubstanceId rows. Bolt class X.Y is fu=100·X,
        //     fy=10·X·Y; SAE/ASTM ksi->MPa (Gr2 57/74, Gr5 & A325 92/120, Gr8 & A490 130/150).
        (MaterialId.Of("steel.g33"),           new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 230.0, 310.0))),
        (MaterialId.Of("steel.g50"),           new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 340.0, 450.0))),
        (MaterialId.Of("steel.fastener-4_6"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 240.0, 400.0))),
        (MaterialId.Of("steel.fastener-4_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 320.0, 400.0))),
        (MaterialId.Of("steel.fastener-5_6"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 300.0, 500.0))),
        (MaterialId.Of("steel.fastener-5_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 400.0, 500.0))),
        (MaterialId.Of("steel.fastener-6_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 480.0, 600.0))),
        (MaterialId.Of("steel.fastener-8_8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 640.0, 800.0))),
        (MaterialId.Of("steel.fastener-10_9"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 900.0, 1000.0))),
        (MaterialId.Of("steel.fastener-12_9"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 1080.0, 1200.0))),
        (MaterialId.Of("steel.fastener-gr2"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 393.0, 510.0))),
        (MaterialId.Of("steel.fastener-gr5"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 634.0, 827.0))),
        (MaterialId.Of("steel.fastener-gr8"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 896.0, 1034.0))),
        (MaterialId.Of("steel.fastener-a325"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 634.0, 827.0))),
        (MaterialId.Of("steel.fastener-a490"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 896.0, 1034.0))),
        // --- hollow-section, pipe, and sheet steels (the Component/steel#STEEL_FAMILY GradeOf policy rows): ASTM
        //     A500 Gr C shaped HSS 345/427 (the ROUND band 317/427 rides the grade row's own
        //     SteelGrade.A500Round.NominalYieldMpa — one substance, the shape-dependent yield a grade column),
        //     ASTM A53 Gr B pipe 240/415, ASTM A653 SS Grade 50 sheet 340/450.
        (MaterialId.Of("steel.a500"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 345.0, 427.0))),
        (MaterialId.Of("steel.a53"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 240.0, 415.0))),
        (MaterialId.Of("steel.a653"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 340.0, 450.0))),
        // --- weld filler metal (the Component/joint#JOINT_FAMILY ElectrodeClass SubstanceId rows): AWS A5.1 carbon
        //     (E60/E70) and A5.5 low-alloy (E80..E110) deposited-metal minima — the FEXX tensile is the electrode
        //     row's own TensileMpa column and the yield the matching AWS classification minimum.
        (MaterialId.Of("steel.e60"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 330.0, 415.0))),
        (MaterialId.Of("steel.e70"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 400.0, 485.0))),
        (MaterialId.Of("steel.e80"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 460.0, 550.0))),
        (MaterialId.Of("steel.e90"),  new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 530.0, 620.0))),
        (MaterialId.Of("steel.e100"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 600.0, 690.0))),
        (MaterialId.Of("steel.e110"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 670.0, 760.0))),
        // --- headed shear studs (the Component/joint#JOINT_FAMILY StudGrade SubstanceId rows): the ISO 13918 SD
        //     grades and AWS D1.1 types carry their OWN specified fy/fu on the grade row, transcribed verbatim so
        //     the grade and the substance can never diverge. SD3 is X5CrNi18-10 austenitic stainless.
        (MaterialId.Of("steel.sd1"),   new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 350.0, 450.0))),
        (MaterialId.Of("steel.sd2"),   new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 235.0, 400.0))),
        (MaterialId.Of("steel.sd3"),   new(Austenitic, MechanicalSource.Mpa(200_000.0, 350.0, 500.0))),
        (MaterialId.Of("steel.aws-a"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 340.0, 420.0))),
        (MaterialId.Of("steel.aws-b"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 350.0, 450.0))),
        // --- plain-shank fastener stock (the Component/fastener#FASTENER_FAMILY StockRow.Plain SubstanceId rows).
        //     The nail's yield is the ASTM F1667-S1 BENDING datum read off the band table at the substance's own
        //     DECLARED reference shank — 0.131 in, the 10d common nail F1667 names — so the substance column states
        //     a published value at a stated diameter instead of a convention derived from its tensile. A connection
        //     design at any other shank reads NailWireClass.At directly and never this column. Dowel and rivet
        //     yields are genuinely PUBLISHED (EN 10025 S235 round bar 235; ASTM A502 Gr 1 195).
        (MaterialId.Of("steel.fastener-nail"),  new(CarbonSteel, new MechanicalSource.NailWire(NailWireClass.LowCarbon, 0.131, 690.0))),
        (MaterialId.Of("steel.fastener-dowel"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 235.0, 400.0))),
        (MaterialId.Of("steel.fastener-rivet"), new(CarbonSteel, MechanicalSource.Mpa(200_000.0, 195.0, 415.0))),
        // --- prestressing strand (the Component/reinforcement#REINFORCEMENT_FAMILY StrandRow SubstanceId rows): the
        //     ultimate is the row's published fpu and the yield its printed proof ratio × fpu (ASTM A416
        //     low-relaxation fpy = 0.90·fpu; EN 10138-3 Fp0,1/Fm = 0.88), the SAME derivation TendonBasis.Yield
        //     projects, so the schedule force and the Mechanical row can never disagree. E_p 195 GPa is LOWER than
        //     the 200 GPa bar value — the helical lay is real stiffness loss.
        (MaterialId.Of("steel.strand-1725"), new(CarbonSteel, MechanicalSource.Mpa(195_000.0, 1552.0, 1725.0))),
        (MaterialId.Of("steel.strand-1860"), new(CarbonSteel, MechanicalSource.Mpa(195_000.0, 1674.0, 1860.0))),
        (MaterialId.Of("steel.y1860s7"),     new(CarbonSteel, MechanicalSource.Mpa(195_000.0, 1637.0, 1860.0))),
        // --- structural adhesives and sealant (the Component/joint#JOINT_FAMILY AdhesiveClass SubstanceId rows): a
        //     POLYMER block — the strength pair is the class's OWN published ASTM D1002 single-lap shear (an
        //     adhesive has no metallic yield plateau, so proof and ultimate are the one design allowable) and the
        //     modulus the published bulk tensile modulus.
        (MaterialId.Of("adhesive.epoxy"),              new(Thermoset, MechanicalSource.Mpa(3000.0, 30.0, 30.0))),
        (MaterialId.Of("adhesive.methacrylate"),       new(Thermoset with { DensityKgM3 = 1050.0, PoissonsRatio = 0.38, ExpansionPerK = 80.0e-6, SpecificHeatJKgK = 1400.0 }, MechanicalSource.Mpa(1500.0, 25.0, 25.0))),
        (MaterialId.Of("adhesive.polyurethane"),       new(Thermoset with { DensityKgM3 = 1150.0, PoissonsRatio = 0.40, ExpansionPerK = 100.0e-6, ConductivityWMK = 0.21, SpecificHeatJKgK = 1600.0 }, MechanicalSource.Mpa(800.0, 15.0, 15.0))),
        (MaterialId.Of("sealant.silicone-structural"), new(Thermoset with { DensityKgM3 = 1400.0, PoissonsRatio = 0.48, ExpansionPerK = 250.0e-6, ConductivityWMK = 0.35, SpecificHeatJKgK = 1200.0 }, MechanicalSource.Mpa(2.0, 1.0, 1.0))),
        // --- stainless steel (EN 10088; EN 1993-1-4 Table 2.1 0.2%-proof/tensile — outside the carbon Table 3.1
        //     factory, AUTHORED; E 200 GPa)
        (MaterialId.Of("steel.1.4301"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 210.0, 520.0))),
        (MaterialId.Of("steel.1.4307"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 200.0, 500.0))),
        (MaterialId.Of("steel.1.4401"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 220.0, 520.0))),
        (MaterialId.Of("steel.1.4404"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 220.0, 520.0))),
        (MaterialId.Of("steel.1.4571"), new(Austenitic, MechanicalSource.Mpa(200_000.0, 220.0, 520.0))),
        (MaterialId.Of("steel.1.4462"), new(Duplex,     MechanicalSource.Mpa(200_000.0, 460.0, 640.0))),
        // --- concrete (EN 206; EN 1992-1-1 Table 3.1 dual-class: fck the yield surrogate, fcm = fck+8 the ultimate,
        //     Ecm the PRINTED Table 3.1 value — printed values stay PUBLISHED, never re-derived, because the package
        //     EnConcreteFactory secant is the design σ(ε) stiffness and NOT the Table 3.1 mean modulus. The id is
        //     the EN dual-class token the sustainability EPD roster keys.)
        (MaterialId.Of("concrete.c12_15"),  new(Concrete with { ConductivityWMK = 1.65 }, MechanicalSource.Mpa(27_000.0, 12.0, 20.0))),
        (MaterialId.Of("concrete.c16_20"),  new(Concrete with { ConductivityWMK = 1.80 }, MechanicalSource.Mpa(29_000.0, 16.0, 24.0))),
        (MaterialId.Of("concrete.c20_25"),  new(Concrete with { ConductivityWMK = 2.00 }, MechanicalSource.Mpa(30_000.0, 20.0, 28.0))),
        (MaterialId.Of("concrete.c25_30"),  new(Concrete, MechanicalSource.Mpa(31_000.0, 25.0, 33.0))),
        (MaterialId.Of("concrete.c30_37"),  new(Concrete, MechanicalSource.Mpa(33_000.0, 30.0, 38.0))),
        (MaterialId.Of("concrete.c35_45"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(80.0) }, MechanicalSource.Mpa(34_000.0, 35.0, 43.0))),
        (MaterialId.Of("concrete.c40_50"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(80.0) }, MechanicalSource.Mpa(35_000.0, 40.0, 48.0))),
        (MaterialId.Of("concrete.c45_55"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(80.0) }, MechanicalSource.Mpa(36_000.0, 45.0, 53.0))),
        (MaterialId.Of("concrete.c50_60"),  new(Concrete with { DensityKgM3 = 2450.0, Vapour = Mu(90.0) }, MechanicalSource.Mpa(37_000.0, 50.0, 58.0))),
        (MaterialId.Of("concrete.c55_67"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(100.0) }, MechanicalSource.Mpa(38_000.0, 55.0, 63.0))),
        (MaterialId.Of("concrete.c60_75"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(100.0) }, MechanicalSource.Mpa(39_000.0, 60.0, 68.0))),
        (MaterialId.Of("concrete.c70_85"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(120.0) }, MechanicalSource.Mpa(41_000.0, 70.0, 78.0))),
        (MaterialId.Of("concrete.c80_95"),  new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(120.0) }, MechanicalSource.Mpa(42_000.0, 80.0, 88.0))),
        (MaterialId.Of("concrete.c90_105"), new(Concrete with { DensityKgM3 = 2500.0, Vapour = Mu(130.0) }, MechanicalSource.Mpa(44_000.0, 90.0, 98.0))),
        // EN 1992-1-1 §11 lightweight aggregate concrete (LC, generic class) — eta_E knockdown E ~18 GPa
        (MaterialId.Of("concrete.lc"), new(Concrete with { DensityKgM3 = 1800.0, ExpansionPerK = 8.0e-6, ConductivityWMK = 0.80 }, MechanicalSource.Mpa(18_000.0, 30.0, 38.0))),
        // concrete.cmu — the CMU block-concrete substance the Component/cmu#CMU_FAMILY SubstanceId keys (ASTM C90
        // f'm 13.8 MPa / unit 17.2, E_m = 900·f'm TMS 402 ~12.4 GPa)
        (MaterialId.Of("concrete.cmu"), new(Concrete with { DensityKgM3 = 2000.0, ExpansionPerK = 8.0e-6, ConductivityWMK = 1.15, Vapour = Mu(6.0) }, MechanicalSource.Mpa(12_400.0, 13.8, 17.2))),
        // --- structural softwood timber (EN 338:2016 Table 1: fm,k the bending surrogate, E0,mean)
        (MaterialId.Of("timber.c14"), new(Softwood with { DensityKgM3 = 290.0 }, MechanicalSource.Mpa(7_000.0, 14.0, 23.0))),
        (MaterialId.Of("timber.c16"), new(Softwood with { DensityKgM3 = 310.0 }, MechanicalSource.Mpa(8_000.0, 16.0, 26.0))),
        (MaterialId.Of("timber.c18"), new(Softwood with { DensityKgM3 = 320.0 }, MechanicalSource.Mpa(9_000.0, 18.0, 30.0))),
        (MaterialId.Of("timber.c20"), new(Softwood with { DensityKgM3 = 330.0 }, MechanicalSource.Mpa(9_500.0, 20.0, 33.0))),
        (MaterialId.Of("timber.c22"), new(Softwood with { DensityKgM3 = 340.0 }, MechanicalSource.Mpa(10_000.0, 22.0, 37.0))),
        (MaterialId.Of("timber.c24"), new(Softwood with { Fire = FireD30 }, MechanicalSource.Mpa(11_000.0, 24.0, 40.0),
            Some(new AcousticDeclaration(
                Absorb(0.10, 0.11, 0.10, 0.08, 0.06, 0.06, 0.07, 0.07, 0.08, 0.08, 0.09, 0.09, 0.09, 0.10, 0.10, 0.10, 0.11, 0.11),
                Sri(14, 16, 18, 20, 22, 24, 26, 27, 29, 31, 33, 34, 36, 38, 40, 41, 42, 43))))),
        (MaterialId.Of("timber.c27"), new(Softwood with { DensityKgM3 = 370.0 }, MechanicalSource.Mpa(11_500.0, 27.0, 45.0))),
        (MaterialId.Of("timber.c30"), new(Softwood with { DensityKgM3 = 380.0 }, MechanicalSource.Mpa(12_000.0, 30.0, 50.0))),
        (MaterialId.Of("timber.c35"), new(Softwood with { DensityKgM3 = 400.0 }, MechanicalSource.Mpa(13_000.0, 35.0, 58.0))),
        (MaterialId.Of("timber.c40"), new(Softwood with { DensityKgM3 = 420.0 }, MechanicalSource.Mpa(14_000.0, 40.0, 66.0))),
        (MaterialId.Of("timber.c45"), new(Softwood with { DensityKgM3 = 440.0 }, MechanicalSource.Mpa(15_000.0, 45.0, 75.0))),
        (MaterialId.Of("timber.c50"), new(Softwood with { DensityKgM3 = 460.0 }, MechanicalSource.Mpa(16_000.0, 50.0, 83.0))),
        // --- structural hardwood timber (EN 338:2016 Table 3 — the FULL fourteen-class D-series)
        (MaterialId.Of("timber.d18"), new(Hardwood with { DensityKgM3 = 475.0 }, MechanicalSource.Mpa(9_500.0, 18.0, 30.0))),
        (MaterialId.Of("timber.d24"), new(Hardwood with { DensityKgM3 = 485.0 }, MechanicalSource.Mpa(10_000.0, 24.0, 40.0))),
        (MaterialId.Of("timber.d27"), new(Hardwood with { DensityKgM3 = 510.0 }, MechanicalSource.Mpa(10_500.0, 27.0, 45.0))),
        (MaterialId.Of("timber.d30"), new(Hardwood with { DensityKgM3 = 530.0 }, MechanicalSource.Mpa(11_000.0, 30.0, 50.0))),
        (MaterialId.Of("timber.d35"), new(Hardwood with { DensityKgM3 = 540.0 }, MechanicalSource.Mpa(12_000.0, 35.0, 58.0))),
        (MaterialId.Of("timber.d40"), new(Hardwood, MechanicalSource.Mpa(13_000.0, 40.0, 66.0))),
        (MaterialId.Of("timber.d45"), new(Hardwood with { DensityKgM3 = 580.0 }, MechanicalSource.Mpa(13_500.0, 45.0, 75.0))),
        (MaterialId.Of("timber.d50"), new(Hardwood with { DensityKgM3 = 620.0 }, MechanicalSource.Mpa(14_000.0, 50.0, 83.0))),
        (MaterialId.Of("timber.d55"), new(Hardwood with { DensityKgM3 = 660.0 }, MechanicalSource.Mpa(15_500.0, 55.0, 92.0))),
        (MaterialId.Of("timber.d60"), new(Hardwood with { DensityKgM3 = 700.0 }, MechanicalSource.Mpa(17_000.0, 60.0, 100.0))),
        (MaterialId.Of("timber.d65"), new(Hardwood with { DensityKgM3 = 750.0 }, MechanicalSource.Mpa(18_500.0, 65.0, 109.0))),
        (MaterialId.Of("timber.d70"), new(Hardwood with { DensityKgM3 = 800.0 }, MechanicalSource.Mpa(20_000.0, 70.0, 117.0))),
        (MaterialId.Of("timber.d75"), new(Hardwood with { DensityKgM3 = 850.0 }, MechanicalSource.Mpa(22_000.0, 75.0, 125.0))),
        (MaterialId.Of("timber.d80"), new(Hardwood with { DensityKgM3 = 900.0 }, MechanicalSource.Mpa(24_000.0, 80.0, 134.0))),
        // wood.oak — the named-hardwood generic alias (a D30-class European white oak the Component/timber family
        // keys when a species rather than a strength class is supplied), absorptive interior-finish vector carried
        (MaterialId.Of("wood.oak"), new(Hardwood with { DensityKgM3 = 700.0 }, MechanicalSource.Mpa(11_000.0, 40.0, 90.0),
            Some(new AcousticDeclaration(
                Absorb(0.05, 0.06, 0.07, 0.08, 0.10, 0.10, 0.11, 0.10, 0.10, 0.10, 0.10, 0.10, 0.09, 0.09, 0.09, 0.09, 0.09, 0.09),
                Sri(18, 20, 22, 24, 26, 29, 31, 33, 35, 37, 38, 39, 40, 40, 39, 35, 33, 31))))),
        // --- glued-laminated timber (EN 14080:2013 Table 5 homogeneous + Table 4 combined, the FULL per-layup set)
        (MaterialId.Of("timber.gl20h"), new(Glulam with { DensityKgM3 = 340.0 }, MechanicalSource.Mpa(8_400.0, 20.0, 33.0))),
        (MaterialId.Of("timber.gl22h"), new(Glulam with { DensityKgM3 = 370.0 }, MechanicalSource.Mpa(10_500.0, 22.0, 37.0))),
        (MaterialId.Of("timber.gl24h"), new(Glulam with { DensityKgM3 = 385.0 }, MechanicalSource.Mpa(11_500.0, 24.0, 40.0))),
        (MaterialId.Of("timber.gl26h"), new(Glulam with { DensityKgM3 = 405.0 }, MechanicalSource.Mpa(12_100.0, 26.0, 43.0))),
        (MaterialId.Of("timber.gl28h"), new(Glulam with { DensityKgM3 = 425.0 }, MechanicalSource.Mpa(12_600.0, 28.0, 47.0))),
        (MaterialId.Of("timber.gl30h"), new(Glulam with { DensityKgM3 = 440.0 }, MechanicalSource.Mpa(13_600.0, 30.0, 50.0))),
        (MaterialId.Of("timber.gl32h"), new(Glulam with { DensityKgM3 = 440.0 }, MechanicalSource.Mpa(14_200.0, 32.0, 53.0))),
        (MaterialId.Of("timber.gl20c"), new(Glulam with { DensityKgM3 = 355.0 }, MechanicalSource.Mpa(10_400.0, 20.0, 33.0))),
        (MaterialId.Of("timber.gl22c"), new(Glulam with { DensityKgM3 = 355.0 }, MechanicalSource.Mpa(10_400.0, 22.0, 37.0))),
        (MaterialId.Of("timber.gl24c"), new(Glulam with { DensityKgM3 = 365.0 }, MechanicalSource.Mpa(11_000.0, 24.0, 40.0))),
        (MaterialId.Of("timber.gl26c"), new(Glulam with { DensityKgM3 = 385.0 }, MechanicalSource.Mpa(12_000.0, 26.0, 43.0))),
        (MaterialId.Of("timber.gl28c"), new(Glulam with { DensityKgM3 = 390.0 }, MechanicalSource.Mpa(12_500.0, 28.0, 47.0))),
        (MaterialId.Of("timber.gl30c"), new(Glulam with { DensityKgM3 = 390.0 }, MechanicalSource.Mpa(13_000.0, 30.0, 50.0))),
        (MaterialId.Of("timber.gl32c"), new(Glulam, MechanicalSource.Mpa(13_500.0, 32.0, 53.0))),
        // --- aluminium (EN 1999-1-1 Table 3.2 wrought alloys; f0 0.2%-proof / fu; E 70 GPa; 6082 named first by
        //     EN 1999-1-1 as the most common European structural extrusion; EN 755-2 extruded f0/fu)
        (MaterialId.Of("aluminium.6082t6"), new(Aluminium with { DensityKgM3 = 2710.0, ExpansionPerK = 23.1e-6, ConductivityWMK = 170.0 }, MechanicalSource.Mpa(70_000.0, 260.0, 310.0))),
        (MaterialId.Of("aluminium.6061t6"), new(Aluminium, MechanicalSource.Mpa(70_000.0, 240.0, 290.0))),
        (MaterialId.Of("aluminium.6063t5"), new(Aluminium with { ConductivityWMK = 200.0 }, MechanicalSource.Mpa(70_000.0, 130.0, 175.0))),
        (MaterialId.Of("aluminium.5083"),   new(Aluminium with { DensityKgM3 = 2660.0, ExpansionPerK = 24.0e-6, ConductivityWMK = 117.0 }, MechanicalSource.Mpa(70_000.0, 125.0, 275.0))),
        // --- masonry units (EN 771; fb the normalized compressive strength surrogate; the WUFI/Fraunhofer sorption
        //     anchors carried on the hygrothermally-characterized clay and AAC rows)
        (MaterialId.Of("masonry.clay"), new(ClayUnit, MechanicalSource.Mpa(7_000.0, 10.0, 20.0),
            Some(new AcousticDeclaration(
                Absorb(0.02, 0.02, 0.03, 0.03, 0.03, 0.04, 0.04, 0.05, 0.05, 0.05, 0.05, 0.06, 0.06, 0.06, 0.07, 0.07, 0.07, 0.07),
                Sri(30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 53, 54, 55, 56, 57, 58))),
            Hygrothermal: Some(new HygrothermalDeclaration(0.38, 9.2, 190.0, Some(0.110))))),
        (MaterialId.Of("masonry.calciumsilicate"), new(SilicateUnit, MechanicalSource.Mpa(8_000.0, 12.0, 24.0))),
        (MaterialId.Of("masonry.aac"), new(AacUnit, MechanicalSource.Mpa(2_000.0, 4.0, 5.0),
            Hygrothermal: Some(new HygrothermalDeclaration(0.81, 7.7, 380.0, Some(0.050))))),
        (MaterialId.Of("masonry.aggregate"), new(AggregateUnit, MechanicalSource.Mpa(9_000.0, 7.0, 14.0))),
        // --- dimension stone (EN 771-6; fk the characteristic compressive surrogate; EI 120/120 slab rating)
        (MaterialId.Of("stone.marble"),  new(Stone, MechanicalSource.Mpa(70_000.0, 15.0, 100.0))),
        (MaterialId.Of("stone.granite"), new(Stone with { DensityKgM3 = 2650.0, PoissonsRatio = 0.23, ExpansionPerK = 8.0e-6, ConductivityWMK = 3.00, SpecificHeatJKgK = 790.0 }, MechanicalSource.Mpa(60_000.0, 20.0, 130.0))),
        // --- glazing (EN 572 soda-lime float + EN 1748-1 borosilicate; fk the characteristic bending strength; the
        //     EN 410 nine-column optical record carried per glass substance — the seam Optical/Energy input;
        //     glass.crown/glass.flint are the Component/glazing#GLAZING_FAMILY pane SubstanceIds)
        (MaterialId.Of("glass.float"), new(SodaLime, MechanicalSource.Mpa(70_000.0, 45.0, 50.0),
            Some(new AcousticDeclaration(
                Absorb(0.18, 0.10, 0.07, 0.05, 0.04, 0.03, 0.03, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02),
                Sri(25, 27, 29, 30, 31, 32, 33, 34, 33, 32, 30, 29, 31, 34, 37, 39, 40, 41))),
            Optical: Some(new OpticalDeclaration(0.90, 0.08, 0.08, 0.85, 0.075, 0.075, 0.0, 0.837, 0.837)))),
        (MaterialId.Of("glass.crown"), new(SodaLime, MechanicalSource.Mpa(70_000.0, 45.0, 50.0),
            Optical: Some(new OpticalDeclaration(0.90, 0.08, 0.08, 0.85, 0.075, 0.075, 0.0, 0.837, 0.837)))),
        (MaterialId.Of("glass.flint"), new(Borosilicate, MechanicalSource.Mpa(63_000.0, 45.0, 50.0),
            Optical: Some(new OpticalDeclaration(0.92, 0.07, 0.07, 0.84, 0.07, 0.07, 0.0, 0.837, 0.837)))),
        // --- insulation (EN 13162-13166 + EN ISO 10456 design lambda + EN ISO 13788 mu; the porous absorbers carry
        //     the EN 29053 flow resistivity the Delany-Bazley route reads)
        (MaterialId.Of("insulation.glasswool"), new(MineralWool, MechanicalSource.Mpa(1.0, 0.001, 0.002),
            Some(new AcousticDeclaration(
                Absorb(0.15, 0.25, 0.40, 0.55, 0.70, 0.80, 0.90, 0.95, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 16),
                FlowResistivityPaSPerM2: Some(15_000.0))))),
        (MaterialId.Of("insulation.stonewool"), new(MineralWool with { DensityKgM3 = 45.0 }, MechanicalSource.Mpa(1.0, 0.001, 0.002),
            Some(new AcousticDeclaration(
                Absorb(0.16, 0.26, 0.42, 0.58, 0.72, 0.82, 0.92, 0.97, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 16),
                FlowResistivityPaSPerM2: Some(38_000.0))))),
        (MaterialId.Of("insulation.eps"),      new(RigidFoam with { DensityKgM3 = 20.0, ExpansionPerK = 60.0e-6, ConductivityWMK = 0.036 }, MechanicalSource.Mpa(5.0, 0.05, 0.10))),
        (MaterialId.Of("insulation.xps"),      new(RigidFoam with { Vapour = Mu(150.0) }, MechanicalSource.Mpa(15.0, 0.20, 0.45))),
        (MaterialId.Of("insulation.pir"),      new(RigidFoam with { DensityKgM3 = 32.0, ConductivityWMK = 0.022, SpecificHeatJKgK = 1400.0 }, MechanicalSource.Mpa(10.0, 0.10, 0.20))),
        (MaterialId.Of("insulation.pur"),      new(RigidFoam with { DensityKgM3 = 35.0, ConductivityWMK = 0.025, SpecificHeatJKgK = 1400.0 }, MechanicalSource.Mpa(10.0, 0.10, 0.20))),
        (MaterialId.Of("insulation.phenolic"), new(RigidFoam with { DensityKgM3 = 35.0, ConductivityWMK = 0.020, SpecificHeatJKgK = 1400.0, Vapour = Mu(50.0), Fire = FireC }, MechanicalSource.Mpa(10.0, 0.10, 0.20))),
        (MaterialId.Of("insulation.woodfibre"), new(WoodFibre, MechanicalSource.Mpa(50.0, 0.10, 0.20),
            Some(new AcousticDeclaration(
                Absorb(0.12, 0.20, 0.35, 0.50, 0.65, 0.75, 0.85, 0.90, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95, 0.95),
                Sri(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 13, 14, 14, 15, 15, 16, 17),
                FlowResistivityPaSPerM2: Some(100_000.0))))),
        // --- gypsum board (EN 520; absorptive interior lining; the WUFI plasterboard sorption anchors carried)
        (MaterialId.Of("gypsum.board"), new(Gypsum, MechanicalSource.Mpa(2_500.0, 2.0, 4.0),
            Some(new AcousticDeclaration(
                Absorb(0.29, 0.20, 0.12, 0.10, 0.08, 0.06, 0.06, 0.05, 0.04, 0.04, 0.04, 0.04, 0.05, 0.05, 0.06, 0.06, 0.07, 0.07),
                Sri(15, 16, 17, 18, 20, 22, 24, 26, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38))),
            Hygrothermal: Some(new HygrothermalDeclaration(0.65, 6.3, 400.0, Some(0.287))))),
        // --- sheet-goods board SUBSTANCES (the Component/panel#PANEL_FAMILY PanelKind.SubstanceId keys): ASTM C1325
        //     fibre-cement backer; EN 13986/EN 636 plywood + EN 300 OSB/3 isotropic-surrogate substance physics
        (MaterialId.Of("cement.board"), new(FibreCement, MechanicalSource.Mpa(7_000.0, 7.0, 10.0))),
        (MaterialId.Of("wood.plywood"), new(WoodPanel, MechanicalSource.Mpa(8_000.0, 30.0, 40.0))),
        (MaterialId.Of("wood.osb"),     new(WoodPanel with { DensityKgM3 = 650.0, SpecificHeatJKgK = 1700.0, Vapour = Mu(200.0) }, MechanicalSource.Mpa(3_500.0, 20.0, 26.0))),
        // --- roofing/waterproofing membrane (EN 13956; per-area products; the membrane IS the vapour control layer)
        (MaterialId.Of("membrane.epdm"), new(Membrane, MechanicalSource.Mpa(5.0, 5.0, 9.0))),
        (MaterialId.Of("membrane.pvc"),  new(Membrane with { DensityKgM3 = 1300.0, ExpansionPerK = 70.0e-6, ConductivityWMK = 0.16, Vapour = Mu(20_000.0) }, MechanicalSource.Mpa(15.0, 10.0, 15.0))),
        (MaterialId.Of("membrane.tpo"),  new(Membrane with { DensityKgM3 = 920.0, ExpansionPerK = 150.0e-6, ConductivityWMK = 0.20, Vapour = Mu(30_000.0) }, MechanicalSource.Mpa(10.0, 9.0, 14.0))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    // The ADMITTED catalogue, frozen at first access. Admission runs the whole applicative join AND constructs an
    // EnSteelMaterial per delegated row, so a projector resolving a thousand elements previously paid a thousand
    // vendor constructions over a table that cannot change. Only the rows that ADMIT memoize: a curation defect is
    // not a hot path, so a failing row re-derives at the CALLER's key and reaches the projection with its whole
    // ManyErrors set intact rather than a summary re-stamped from a frozen cell.
    static readonly Lazy<FrozenDictionary<MaterialId, Seq<MaterialPropertySet>>> Admitted =
        new(static () => Rows
                .Select(static entry => (entry.Key, Sets: Admit(entry.Value, AdmitKey)))
                .Where(static entry => entry.Sets.IsSucc)
                .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Sets.IfFail(Seq<MaterialPropertySet>())),
            LazyThreadSafetyMode.ExecutionAndPublication);

    // The projector-facing resolution: one frozen read on every healthy path. An UNREGISTERED material rails,
    // because engineering properties are REQUIRED for a known structural material the
    // Component/capacity#SECTION_CAPACITY and Rasm.Compute design-code routes read — the asymmetric dual of the
    // OPTIONAL Properties/sustainability#SUSTAINABILITY_PROPERTY Lookup, whose lifecycle data is
    // declared-or-absent. An app authoring a material with bespoke properties supplies them at the wire and does
    // not route this catalogue.
    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Admitted.Value.TryGetValue(id, out Seq<MaterialPropertySet> admitted)
            ? Fin.Succ(admitted)
            : Rows.TryGetValue(id, out MaterialPropertyRow? row)
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

## [03]-[DURABILITY_MIX]

- Owner: `CementType` the binder axis carrying its own published ageing exponent; `DurabilityMix` the published `(cement × w/c)` chloride-migration row; `DurabilityCatalogue` the fib Annex B transcription and its `Resolve` lowering onto the seam `Durability` case.
- Law: DURABILITY IS MIX-KEYED, NEVER SUBSTANCE-KEYED. The reference publishes the migration coefficient `D_RCM,0` and the ageing exponent `alpha` against binder type and equivalent water/cement ratio and against nothing else, and a strength class determines neither: one C30/37 is reachable at w/c 0.45 on CEM I and at w/c 0.55 on CEM III/B, whose migration coefficients differ by roughly sevenfold in OPPOSITE directions. A `Durability` column on a concrete substance row is therefore unfillable in principle rather than merely unfilled, and the roster carries none by construction.
- Cases: three published binder rows — CEM I 42.5R, CEM I with fly ash at `k = 0.5`, and CEM III/B — each over the five published w/c steps. A binder the reference adds is one `CementType` row plus its five `DurabilityMix` entries.
- Entry: `public static Fin<MaterialPropertySet> Resolve(CementType cement, double waterCementRatio, double carbonationRateMmPerSqrtYear, Op key)` — reads the published pair and lowers through the seam `OfDurability`. The carbonation rate is a CALLER input because the reference keys it on exposure class rather than on mix, so the mix table answers exactly the two columns it publishes and the exposure class supplies the third.
- Packages: Rasm.Element (project — `MaterialPropertySet.OfDurability`, `PropertyEvidence`, `ElementFault.ValueRejected`), Rasm (project — `Op`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` the binder axis), LanguageExt.Core (`Fin`/`Option`), BCL inbox (`FrozenDictionary`).
- Boundary: the table admits only the ratios the reference PRINTS. It publishes at 0.05 steps and an interpolated cell is a derivation rather than a transcription, so a ratio between two rows rails instead of blending them, and a ratio outside `[0.40, 0.60]` is outside the reference's stated validity domain and rails for that reason. The migration coefficient carries the reference's own coefficient of variation as a relative band and the ageing exponent its published mean and standard deviation, both stated on the row rather than assumed by a consumer; a project supplying a measured mix design substitutes a `Properties/assessment#ASSESSMENT_RECORD` `Measured` record and never edits this table.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CementType {
    // The ageing exponent is a BINDER property in the reference and does not move with w/c, so it rides the binder
    // row and the migration table carries the ratio-dependent column alone.
    public static readonly CementType PortlandCem1  = new("cem-i-42.5r",   alphaMean: 0.30, alphaSd: 0.12);
    public static readonly CementType PortlandFlyAsh = new("cem-i-fa-k05", alphaMean: 0.60, alphaSd: 0.15);
    public static readonly CementType BlastFurnace  = new("cem-iii-b",     alphaMean: 0.45, alphaSd: 0.20);
    public double AlphaMean { get; }
    public double AlphaSd { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The published chloride-migration coefficient at the reference's own 28-day age, in 1e-12 m2/s as printed. CoV is
// the reference's stated dispersion for the column and lowers as the datum's relative band — the ONE spread the
// source publishes, never a catalogue transcription band layered on top of it.
public readonly record struct DurabilityMix(CementType Cement, double WaterCementRatio, double DrcmE12, double CoefficientOfVariation);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DurabilityCatalogue {
    const double DrcmCoefficientOfVariation = 0.20;
    const double DrcmScaleToSi = 1.0e-12;
    const double MinWaterCementRatio = 0.40;
    const double MaxWaterCementRatio = 0.60;

    static readonly PropertyEvidence MixTable = new("vendor", "fib bulletin 34 annex b", Option<LocalDate>.None);

    // --- [TABLES]
    // The transcription, keyed on the pair the reference itself keys on. The ratio is a table KEY rather than a
    // continuous argument, which is why it is compared exactly: every value here is printed, and a value between
    // two printed rows has no published cell to read.
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

    // The two published columns plus the caller's exposure-class carbonation rate, lowered through the seam's own
    // admission. A ratio the reference does not print rails with its own domain named, so a caller reading the
    // fault learns whether it asked outside the validity range or merely between two printed steps.
    public static Fin<MaterialPropertySet> Resolve(CementType cement, double waterCementRatio, double carbonationRateMmPerSqrtYear, Op key) =>
        At(cement, waterCementRatio).Match(
            Some: mix => MaterialPropertySet.OfDurability(
                carbonationRateMmPerSqrtYear, mix.DrcmE12 * DrcmScaleToSi, cement.AlphaMean, key, MixTable),
            None: () => ElementFault.ValueRejected(key, waterCementRatio is >= MinWaterCementRatio and <= MaxWaterCementRatio
                ? $"<durability-mix-unprinted:{cement.Key}:{waterCementRatio:R}>"
                : $"<durability-mix-out-of-domain:{cement.Key}:{waterCementRatio:R}:{MinWaterCementRatio:R}..{MaxWaterCementRatio:R}>"));
}
```

## [04]-[MIX_PROPORTION]

- Owner: `ExposureClass` the EN 206:2013 Annex F Table F.1 durability-floor axis (max w/c, min cement, min strength class, min air, per exposure class); `SlumpBand`/`AggregateSize`/`AirBand` the closed ACI table keys; the `Water`/`WcStrength`/`CoarseFraction` published tables (ACI 211.1-91 R2002, SI appendix); `MixSpec` the caller's mix declaration; `MixProportion` the derived per-m³ mass receipt; `MixDesign.Proportion` the one absolute-volume fold.
- Law: A MIX IS DECLARED, NEVER INFERRED FROM A STRENGTH CLASS — the same law the durability sibling carries: a C30/37 substance row determines no proportions, so the proportion fold takes a caller `MixSpec` and answers the published method's own derivation, and no substance roster column carries a mix. The tables publish the METHOD'S inputs and the job supplies what the method requires by test (§A3.3.1): the two aggregate specific gravities and the oven-dry-rodded coarse unit weight are REQUIRED spec columns, never defaults — the one published assumption is the cement specific gravity 3.15 (§A3.2.1, ASTM C 150/C 175 portlands; a blended cement supplies its tested value).
- Cases: 18 `ExposureClass` rows (X0 · XC1-4 · XS1-3 · XD1-3 · XF1-4 · XA1-3, every cell as EN 206:2013 prints it, two-source verified); 3 `SlumpBand` × 8 `AggregateSize` water cells per lane with the printed dashes typed-absent; 6 SI strength anchors per lane with the air-entrained 40 MPa dash typed-absent; 8 × 4 coarse-fraction cells over the fineness-modulus band `[2.40, 3.00]`.
- Entry: `public static Fin<MixProportion> MixDesign.Proportion(MixSpec spec, Op key)` — the ACI absolute-volume chain under the EN exposure floor: water and air from the `Water` row (`Air = None` reads the entrapped-air row, `Some(band)` the entrained target), w/c interpolated between the printed strength anchors on the spec's lane (the method's own interpolation practice; outside the printed band refuses) then CAPPED by the exposure's max w/c, the strength floor enforced (a target below the exposure's minimum class refuses typed), cement `max(water / wc, exposure minimum)`, coarse mass the fineness-interpolated volume fraction × the spec's dry-rodded unit weight, fine aggregate the absolute-volume remainder (a negative remainder refuses — the declared mix is over-constrained), and the receipt records the APPLIED w/c and whether an exposure floor governed.
- Packages: Rasm.Element (project — `ElementFault.ValueRejected`, `MaterialId`), Rasm (project — `Op`), Thinktecture.Runtime.Extensions (`[SmartEnum]` the four key vocabularies), LanguageExt.Core (`Fin`/`Option`), BCL inbox (`FrozenDictionary`/`ImmutableArray`).
- Growth: a national-annex variant is a SIBLING exposure table beside this one, never edits to these cells — the CEN survey records most states substituting their own values, so the EN base table stays the transcription and a jurisdiction lands as its own keyed set; a new slump or aggregate row is one key row with its printed cells; a richer method edition (ACI 211.1-22 widened rosters) is a sibling anchor set, never cells blended into the -91 table.
- Boundary: every cell transcribes the print — the SI appendix tables (mixing-water table, w/c-strength table, coarse-fraction table; the appendix table designators vary between printings, so no designator is hard-coded), EN 206:2013 Table F.1 with its footnotes carried as row comments — and an interpolated FM or strength value stays INSIDE the printed lattice with the out-of-band read refusing; the XF rows' 4,0 % air and the XA2/XA3 sulfate-resisting-cement obligation are row facts a specifier reads, not derivations; `MixSpec.Materials` binds the constituent `MaterialId`s as caller declarations (this page names no substance ids); the constituent-row projection is `Projection/component#COMPOSITION_AUTHOR` `Constituents`' — this owner answers masses and stops.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// EN 206:2013 Annex F Table F.1 — the recommended limiting values per exposure class, every cell as printed
// (two-source verified against the standard page and an independent reprint; the 2000→2013 delta is wording only).
// Absent cells are absent columns, never zeros. XF rows: aggregate per EN 12620 with adequate freeze/thaw
// resistance; XA2/XA3: sulfate-resisting cement per EN 197-1. MinFckMpa transcribes the class token's cylinder
// number so the strength floor compares without a parser.
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

// The ACI SI table keys — closed vocabularies over the printed rosters; Ordinal is the column index every
// 8-wide row array aligns to, so a cell read is a lattice read, never a search.
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

// The entrained-air exposure band (the US print names the third row Severe, the SI print Extreme — one row).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AirBand {
    public static readonly AirBand Mild     = new("mild",     targets: [4.5, 4.0, 3.5, 3.0, 2.5, 2.0, 1.5, 1.0]);
    public static readonly AirBand Moderate = new("moderate", targets: [6.0, 5.5, 5.0, 4.5, 4.5, 4.0, 3.5, 3.0]);
    public static readonly AirBand Severe   = new("severe",   targets: [7.5, 7.0, 6.0, 6.0, 5.5, 5.0, 4.5, 4.0]);
    public ImmutableArray<double> Targets { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The constituent MaterialId bindings — caller declarations, so this page names no substance id.
public sealed record MixMaterials(MaterialId Cement, MaterialId Water, MaterialId FineAggregate, MaterialId CoarseAggregate);

// The caller's mix declaration. The three by-test columns are REQUIRED (§A3.3.1); the cement specific gravity
// defaults to the method's own stated 3.15 assumption (§A3.2.1) and a blended cement supplies its tested value.
public sealed record MixSpec(
    ExposureClass Exposure, double TargetMpa, SlumpBand Slump, AggregateSize Aggregate, double FinenessModulus,
    Option<AirBand> Air, double FineSpecificGravity, double CoarseSpecificGravity, double CoarseDryRoddedKgM3,
    MixMaterials Materials, double CementSpecificGravity = 3.15);

// The derived per-m³ receipt: masses, the air fraction, the APPLIED w/c (after the exposure cap and cement
// floor), and whether an exposure floor governed — the typed algorithm evidence a consumer audits.
public sealed record MixProportion(
    double CementKgM3, double WaterKgM3, double FineKgM3, double CoarseKgM3,
    double AirFraction, double WaterCement, bool ExposureGoverned);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MixDesign {
    const double WaterDensityKgM3 = 1000.0;   // the SI worked examples' value; the US body spells 62.4 lb/ft³

    // ACI A1.5.3.3 mixing water kg/m³ — rows per (lane, slump band) over the size roster order; null IS the
    // printed dash, so a 150 mm aggregate at 150-175 mm slump refuses rather than fabricating a cell.
    static readonly FrozenDictionary<(bool Air, SlumpBand Band), ImmutableArray<double?>> Water =
        new Dictionary<(bool, SlumpBand), ImmutableArray<double?>> {
            [(false, SlumpBand.S25To50)]   = [207, 199, 190, 179, 166, 154, 130, 113],
            [(false, SlumpBand.S75To100)]  = [228, 216, 205, 193, 181, 169, 145, 124],
            [(false, SlumpBand.S150To175)] = [243, 228, 216, 202, 190, 178, 160, null],
            [(true,  SlumpBand.S25To50)]   = [181, 175, 168, 160, 150, 142, 122, 107],
            [(true,  SlumpBand.S75To100)]  = [202, 193, 184, 175, 165, 157, 133, 119],
            [(true,  SlumpBand.S150To175)] = [216, 205, 197, 184, 174, 166, 154, null],
        }.ToFrozenDictionary();

    // Entrapped air, non-air-entrained lane, percent over the size roster order.
    static readonly ImmutableArray<double> EntrappedAir = [3.0, 2.5, 2.0, 1.5, 1.0, 0.5, 0.3, 0.2];

    // ACI A1.5.3.4(a) — w/c versus 28-day MPa, (anchor, non-air, air) with the air 40 MPa cell printed as a dash.
    static readonly ImmutableArray<(double Mpa, double NonAir, double? Air)> WcStrength = [
        (40.0, 0.42, null), (35.0, 0.47, 0.39), (30.0, 0.54, 0.45), (25.0, 0.61, 0.52), (20.0, 0.69, 0.60), (15.0, 0.79, 0.70)];

    // ACI A1.5.3.6 — oven-dry-rodded coarse-aggregate volume per unit volume, FM columns [2.40, 2.60, 2.80, 3.00].
    static readonly FrozenDictionary<AggregateSize, ImmutableArray<double>> CoarseFraction =
        new Dictionary<AggregateSize, ImmutableArray<double>> {
            [AggregateSize.A9p5]  = [0.50, 0.48, 0.46, 0.44], [AggregateSize.A12p5] = [0.59, 0.57, 0.55, 0.53],
            [AggregateSize.A19]   = [0.66, 0.64, 0.62, 0.60], [AggregateSize.A25]   = [0.71, 0.69, 0.67, 0.65],
            [AggregateSize.A37p5] = [0.75, 0.73, 0.71, 0.69], [AggregateSize.A50]   = [0.78, 0.76, 0.74, 0.72],
            [AggregateSize.A75]   = [0.82, 0.80, 0.78, 0.76], [AggregateSize.A150]  = [0.87, 0.85, 0.83, 0.81],
        }.ToFrozenDictionary();

    // The one absolute-volume fold under the EN exposure floor. Interpolation stays INSIDE the printed lattice
    // (the method's own practice); every out-of-band or dashed read refuses typed with the lattice named.
    public static Fin<MixProportion> Proportion(MixSpec spec, Op key) =>
        from water in Cell(spec, key)
        from _floor in spec.TargetMpa >= spec.Exposure.MinFckMpa
            ? Fin.Succ(unit)
            : ElementFault.ValueRejected(key, $"<mix-strength-below-exposure:{spec.Exposure.Key}:{spec.TargetMpa:R}:{spec.Exposure.StrengthClass}>")
        from wcMethod in InterpolatedWc(spec, key)
        let wcApplied = spec.Exposure.MaxWc.Match(Some: cap => Math.Min(wcMethod, cap), None: () => wcMethod)
        let cementMethod = water.KgM3 / wcApplied
        let cement = spec.Exposure.MinCementKgM3.Match(Some: floor => Math.Max(cementMethod, floor), None: () => cementMethod)
        from coarseShare in InterpolatedCoarse(spec, key)
        let coarse = coarseShare * spec.CoarseDryRoddedKgM3
        let fineVolume = 1.0 - (water.KgM3 / WaterDensityKgM3) - (cement / (spec.CementSpecificGravity * WaterDensityKgM3))
            - (coarse / (spec.CoarseSpecificGravity * WaterDensityKgM3)) - water.AirFraction
        from fine in fineVolume > 0.0
            ? Fin.Succ(fineVolume * spec.FineSpecificGravity * WaterDensityKgM3)
            : ElementFault.ValueRejected(key, $"<mix-overconstrained:{spec.Exposure.Key}:{fineVolume:R}>")
        select new MixProportion(
            CementKgM3: cement, WaterKgM3: water.KgM3, FineKgM3: fine, CoarseKgM3: coarse,
            AirFraction: water.AirFraction, WaterCement: water.KgM3 / cement,
            ExposureGoverned: cement > cementMethod || wcApplied < wcMethod);

    static Fin<(double KgM3, double AirFraction)> Cell(MixSpec spec, Op key);
    // The Water row read at the spec's (lane, band, size) with the null dash refusing; air from the entrapped row
    // (None) or the band's target column (Some), returned as a fraction. The XF exposure air floor re-checks here:
    // an exposure demanding 4,0 % air refuses a non-entrained spec rather than silently under-airing.

    static Fin<double> InterpolatedWc(MixSpec spec, Op key);
    // Linear between the two bracketing printed anchors on the spec's lane; a target outside [15, 40] MPa, or an
    // air-entrained target above the lane's highest printed anchor (the 40 MPa dash), refuses with the band named.

    static Fin<double> InterpolatedCoarse(MixSpec spec, Op key);
    // Linear in fineness modulus between the row's printed FM columns; FM outside [2.40, 3.00] refuses — the
    // printed lattice is linear in FM, so the interpolation reproduces it exactly and invents nothing.
}
```

## [05]-[ASSESSMENT_INPUT]

- Owner: NONE — the Materials folder authors NO assessment-input marshaller and NO `Assessment` node; the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected seam `Material` node IS the analysis input. `Properties/assessment#ASSESSMENT_RECORD` is the disjoint concern: it owns the DATED DECLARATION source — an in-situ result, a survey grade, a product EPD — that overrides a catalogue row before projection, never an input bag a consumer reads after it.
- Cases: zero — there is no input shape to model; `Rasm.Compute` reads the typed `MaterialPropertySet` cases off the graph and dispatches on `set.Discipline`, so a per-discipline input bag is the deleted form.
- Entry: `Rasm.Compute` reads the `Material` node plies DIRECTLY above the seam (`id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the relocated multi-ply `AssemblyAggregator` + the ISO/EN closed-form routes + the VividOrange/FE structural solvers), and writes the seam `Assessment` `Result` node back content-keyed on `(input key, route)`; the case→`Discipline` map is the seam's own `MaterialPropertySet.Discipline` accessor, so Compute selects its route with no parallel Materials marshaller.
- Boundary: a Materials-authored typed-input bag is redundant with Compute reading the typed cases off the graph, so the seam carries ONE property surface (the `Material` node) and never a parallel input node; the seam `Acoustic` case's intrinsic `Nrc`/`StcWeighted`/`SoundReductionIndexDb` folds are the single-material ratings Compute's ISO 12354 layered fold reads through the SAME `RatingContour.Fit` contour kernel, so the assembly STC and the material STC share one contour owner; the multi-ply aggregation is `Rasm.Compute`'s, this folder retaining only the single-material property SOURCE and crossing to Compute solely through the seam graph.

## [06]-[RESEARCH]

- (none)
