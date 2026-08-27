# [ELEMENT_MATERIAL]

`Material` nodes key on `MaterialId` `[ValueObject<string>]`, carry one `MaterialComposition` `[Union]` closing the type-level material-set structure (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), and carry one `MaterialPropertySet` `[Union]` closing the typed engineering-property family keyed to the one `Classification/classification#DISCIPLINE_AXIS` `Discipline`.

Each material is a FULL engineering object: one node carries U-value, sound spectrum, fire REI rating, structural grade, seismic damping, moisture-storage curve, service-life diffusion data, solar-optical constants, electrical resistivity, embodied carbon, and cost together — never a per-discipline material type.

Every multi-column admission ACCUMULATES: the `Of*` smart-constructors run each independent column as a concrete `Validation<Error,_>` slot joined by the tuple `.Apply` and collapse `.As().ToFin()` once, so a bad datasheet reports every offending column in one `Fin.Fail` (`ManyErrors`), never first-fault-wins.

`MaterialComposition` and `MaterialPropertySet` are the contract's two material owners: the `Rasm.Materials` projector lowers its material subgraph onto `Material` nodes carrying them, and the `Rasm.Bim` projector reads them. `Relations/relation#EDGE_ALGEBRA` `Associate` carries the occurrence usage binding (layer direction/offset, profile cardinal point), this owner keeping only the type-level SET structure.

`MaterialPropertySet` composes `Properties/quantity#MEASURE_VALUE` for every measured column (`MeasureValue.Of` SI coercion with the `Dimension` discriminator), `Composition/acoustic#ACOUSTIC_FOLDS` for the `Acoustic` case, and `Classification/classification#DISCIPLINE_AXIS` for the property-to-discipline key; universal non-finite or out-of-range admission stays on `KernelFault.OutOfRange`; Element-only composition and conservation invariants use `ElementFault.ValueRejected`.

`ProfileSet` carries its `Seq<MaterialProfile>` rows — each a neutral `ProfileRef` with its IFC junction priority, function category, and reference-axis offsets, beside the neutral `SectionProperties` the `Rasm.Materials` projector resolves ONE-HOP (M7) and BAKES on (`WithSection`) — so a `Rasm.Compute` structural consumer reads the section off the element graph (`ElementGraph.SectionOf`) without re-resolving or admitting VividOrange, and a built-up compound keeps every row rather than its primary alone.

`SectionProperties` carries the FULL structural-design and fire column set the `Rasm.Compute` design-code checks read off the contract (the AISC 360 / EN 1993 / AISI S100 / ACI 318 / NDS / TMS 402 flexure-shear-compression and the EN 1993-1-2 / EN 1992-1-2 fire routes) — a CONSUMER-CONTRACT-driven shape, never a per-element-type section carrier.

`Rasm.Materials` resolves its elastic columns from the VividOrange polygon solver and computes the plastic moduli, torsion constant, shear areas, shear-centre offsets, and mono-symmetry factor the solver does not expose — the asymmetric-section columns the EN 1993-1-1 §6.3.2 general LTB route needs for a channel, tee, or angle, zero on a doubly-symmetric section.

## [01]-[INDEX]

- [02]-[MATERIAL_COMPOSITION]: `MaterialId`, `ProfileRef` with its content key, `SectionProperties` the S-E1 cross-section algebra (`OfMillimetres` mm-basis admission, `Lower()` solver frame, `LtbRoute` route rows, `Centroid`/`SectionForm`), the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` rows, `PropertyEvidence` the S-E3 evidence carrier (`EvidenceGrade` rank, `Attestation`, the `EvidenceRun` audit link), `SampledCurve` the kernel-fitted state-dependent function, and the four-case `MaterialComposition` union with its accumulating set admissions.
- [03]-[MATERIAL_PROPERTY]: `MaterialPropertySet` the class-root `[Union]` + `[Equatable]` keyed to `Discipline`, the `FireRating`/`EuroclassSuffix` reaction vocabulary, `FireCoverage` + `FireResistance` the EN 13501-2 criteria, `ImpactCategory`/`LifecycleStage` the EN 15804+A2 matrix axes, the accumulating `Of` admissions over the kernel-`Band` slots, and the named per-discipline reads over one private polymorphic lookup.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialId` the `[ValueObject<string>]` material-identity key a `Material` node carries; `MaterialComposition` the `[Union]` type-level material-set structure; `MaterialLayer` the layer row (`MaterialId` + `Dimension`-length `Thickness` + name + `Priority`/`Category`/`Ventilated`); `MaterialConstituent` the constituent row (`MaterialId` + category + fraction + `PartName`); `MaterialProfile` the compound-profile row (`MaterialId` + its own `ProfileRef` + `Priority`/`Category` + the reference-axis `Offsets` vector); `ProfileRef` the neutral section-profile reference (`Standard` + `Designation` + content key) a `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalogue; `SectionProperties` the neutral baked section algebra over `Properties/quantity#MEASURE_VALUE` columns.
- Cases: `Single` (one homogeneous `MaterialId` — `IfcMaterial`) · `LayerSet` (a `Seq<MaterialLayer>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`) · `ProfileSet` (a `Seq<MaterialProfile>` of per-row material-plus-profile rows beside the set-level `Composite` outline and one baked section, members and built-up compounds — `IfcMaterialProfileSet`) · `ConstituentSet` (a `Seq<MaterialConstituent>` of fraction-weighted keyword-tagged components, composites — `IfcMaterialConstituentSet`); the closed IFC material-definition family (`IfcMaterialList` deprecated and never admitted), a composition selecting how the material resolves.
- Entry: `MaterialComposition.OfSingle(material)` is the TOTAL constructor (no admission invariant — the `MaterialPropertySet.OfAcoustic`/`OfFire` total shape, never a `Fin` wrapper over a total op); `OfProfileSet` owns both profile modalities on the INPUT SHAPE — `(material, profile)` the total single-row mint every authored member takes, `(profiles, key, composite)` the `Fin<T>` compound admission an IFC ingest folds; `OfLayerSet(layers, key)` and `OfConstituentSet(constituents, key)` are `Fin<T>` admissions using kernel scalar refusal for non-positive thickness and bounded priorities/fractions, while empty sets, offset arity, and fraction normalization remain `ElementFault.ValueRejected` semantics; every factory is `Of`-prefixed so the name never collides with the same-named nested case type (the `MaterialPropertySet.Of*` convention — a bare `Single(...)` static method and a nested `Single` case are one declaration space, a compile collision). `Materials` projects the assigned `MaterialId` set, `PrimaryMaterial` the appearance/structural-default key, `TotalThickness` (a `LayerSet` read) the layer buildup depth, and `ProfileSet.Primary`/`Material`/`Profile` the IFC-ordered head-row reads.
- Auto: the three invariant-bearing SET cases carry a PRIVATE constructor and an internal `Seed` re-hydration escape (the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage.ProfileSet` and `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` admission shape), so the only public admission is the `Of` factory and an empty/degenerate set is UNREPRESENTABLE — `PrimaryMaterial`'s `OrderByDescending(...).First()` and `ProfileSet.Primary`'s index-zero read are then total, never a latent throw on the empty set a public positional ctor admits; `Materials` dispatches the generated `Switch` projecting the `MaterialId` set each case carries, a compound `ProfileSet` reporting EVERY row's material; `OfLayerSet` guards each `MaterialLayer.Thickness` positive (the SI metre magnitude of the `Properties/quantity#MEASURE_VALUE` length) and each row priority in `[0,100]`; `OfProfileSet` guards the row set non-empty, each row priority in range, and each offset vector within the IFC `LIST[1:2]` arity; `OfConstituentSet` guards each fraction finite and the fraction sum to one within tolerance so a composite mixture normalizes once at construction (the `Rasm.Compute` `AssemblyAggregator` rule-of-mixtures reads the normalized fractions and never re-guards them) — every guard an independent accumulating slot, so one malformed datasheet reports every offending row and column at once.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<string>]`), Generator.Equals (`[Equatable]`/`[OrderedEquality]`/`[StringEquality]`), LanguageExt.Core (`Seq`/`Fin`/`Option`), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter`, `ContentAddress.Of`), `Rasm` (`Rasm/Domain/validation#ADMISSION_SLOTS`).
- Growth: a temperature-dependent property is one `Option<SampledCurve>` column beside its steady-state scalar; the IFC material-definition family is closed at four cases; a new layer/constituent/profile attribute is one row column; a new structural or fire section column is one `MeasureValue` field the Materials resolver fills and a Compute check reads — appended AFTER `Form` in the canonical order, never re-ordered; a new section catalogue is one `ProfileRef.Standard` token; a new evidence axis is one `PropertyEvidence` column + its `CaseBytes` write in the same edit; imported material `Pset` rows are NOT columns here — each lands as a neutral `PROPERTY_BAG` node under `EvidenceGrade.Import` bound by one `Assign.PropertyDefinition` edge, the typed family staying FULL-VECTOR and authored-only.
- Boundary: `MaterialComposition` is the ONE composition owner — a per-element-type composition class is the deleted form; the composition is the TYPE-LEVEL set structure only, the occurrence usage binding (`LayerSetUsage` direction/sense/offset, `ProfileSetUsage` cardinal-point/extent) riding the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, so a layer set's geometric usage never duplicates onto the composition; a `ProfileSet` stores its `Seq<MaterialProfile>` rows beside the set-level `Composite` and DERIVES `Material`/`Profile` — a primary scalar stored beside row zero is the named double-store defect, and the single-row member is the one-row case of the same store rather than a second shape, so a built-up compound (plate girder, steel-concrete composite) keeps every row's material, priority, category, own profile geometry, and reference-axis offsets where a primary-only store drops all but the first; `Profile` is the section identity a consumer resolves one-hop — the declared `Composite` when a compound set carries one, else the primary row's own profile — so the two-level store keeps row zero's plate geometry a composite-overwrites-primary read destroys; each row carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section-property type — the contract references no VividOrange, the `Rasm.Materials` projector resolving the `ProfileRef` one-hop and BAKING the neutral `SectionProperties` (`WithSection`) so a structural consumer reads the resolved section once; the `SectionProperties` is the consumer-contract column set the `Rasm.Compute` design-code routes read (`Area`/`Iyy`/`Izz`/`J`/`Iw`/`Wely`/`Welz`/`Wply`/`Wplz`/`AvY`/`AvZ`/radii/`Depth`/`Width`/`HeatedPerimeter`/`AxisDistance`/`ShearCentreY`/`ShearCentreZ`/`MonosymmetryFactor`) — the contract carries the baked scalars, never a VividOrange type, and the projector computes the plastic moduli/torsion/warping/shear-area/asymmetry columns the VividOrange polygon solver does not expose (the `Iw` warping constant the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling routes require, never derivable from `J` alone, AND the `ShearCentreY`/`ShearCentreZ` shear-centre offsets + the `MonosymmetryFactor` β_y the EN 1993-1-1 §6.3.2 GENERAL LTB route requires for a channel/tee/angle — all zero for a doubly-symmetric section, so a PFC/tee is no longer the unbuckle-checkable thin slice the symmetric-only column set left); a `MaterialLayer.Thickness` is a `Properties/quantity#MEASURE_VALUE` `Dimension`-length-checked measure read SI-native through `.Si`, never a bare double, and a `MaterialProfile.Offsets` entry is the same `Dimension`-length measure so a reference-axis offset never crosses as a native-unit scalar; a row `Priority` is `Option<int>` over the IFC `[0,100]` junction percentage because GeometryGym spells an unset priority as `int.MinValue` — projecting that sentinel at the `Rasm.Bim` read is the `[SENTINEL_PROJECTION]` obligation, and an `int` column carrying it into the contract or the content hash is the deleted form; `MaterialLayer.Ventilated` is `Option<bool>` over the three-state `IfcLogical` the `Properties/property#PROPERTY_VALUE` `Logical` case already ratifies (`None` = `UNKNOWN`), so a second three-state vocabulary minted for one `IfcLogical` domain is the deleted parallel shape and an `UNKNOWN` coerced to `false` is the named EN ISO 6946 falsification; a per-LAYER offset column is unrepresentable BY LAW because `IfcMaterialLayerWithOffsets` publishes no accessor and no public constructor — the asymmetry with the profile subtype's public `OffsetValues` is a GeometryGym surface fact, never a symmetry the contract fabricates; `MaterialComposition` is a CLASS-root `[Union]` + `[Equatable]` and the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile`/`SectionProperties` rows are `[Equatable]` record structs so the `Rasm.Persistence` `StructuralMerge` drills a changed layer thickness / constituent fraction / row priority / section column to `Composition.Layers[i].Thickness` / `.Constituents[i].Fraction` / `.Profiles[i].Priority` / `.Section.<column>` rather than replacing the whole composition (the record-root opaque-leaf form is deleted); the composition serializes to the IFC 4.3 material-definition family at the `Rasm.Bim` boundary, host-neutral here; the `CanonicalBytes` arms fold EVERY case field with each collection count-prefixed and each optional column presence-prefixed — the `Bool`-prefixed baked `Section` delegating to `SectionProperties.CanonicalBytes` — so the M7 bake (which runs at projection, before the `Material` node's content-keyed mint), a re-resolved section column, a changed row priority, and an `UNKNOWN`-versus-`FALSE` ventilation each fork the node identity, a section-omitting or column-omitting arm being the deleted collision that addressed two distinct compositions as one material.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using Generator.Equals;
using LanguageExt;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using System.Numerics.Tensors;
using Thinktecture;
using Band = Rasm.Numerics.Band;
using Interpolant = Rasm.Numerics.Interpolant;
using CalculusInterpolant = Rasm.Numerics.Interpolant<Rasm.Numerics.ICalculus>;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Composition;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class MaterialId {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();
}

[Equatable]
public readonly partial record struct ProfileRef {
 [StringEquality(StringComparison.Ordinal)] public string Standard { get; }
 [StringEquality(StringComparison.Ordinal)] public string Designation { get; }
 public UInt128 ContentKey { get; }

 private ProfileRef(string standard, string designation, UInt128 contentKey) =>
  (Standard, Designation, ContentKey) = (standard, designation, contentKey);

 public static ProfileRef Of(string designation) => Of("", designation);
 public static ProfileRef Of(string standard, string designation) {
  string normalizedStandard = standard.Trim();
  string normalizedDesignation = designation.Trim();
  return new(normalizedStandard, normalizedDesignation,
   ContentAddress.Of((normalizedStandard, normalizedDesignation), 0.0,
    static (pair, w) => w.String(pair.normalizedStandard).String(pair.normalizedDesignation)).ToValue());
 }

 public static Fin<ProfileRef> Rehydrate(string standard, string designation, UInt128 contentKey) {
  ProfileRef admitted = Of(standard, designation);
  return admitted.ContentKey == contentKey
   ? Fin.Succ(admitted)
   : new ElementFault.ValueRejected($"<profile-content-key-mismatch:{contentKey}>");
 }
}

[Equatable]
public readonly partial record struct SectionProperties(
 MeasureValue Area, MeasureValue Iyy, MeasureValue Izz, MeasureValue J, MeasureValue Iw,
 MeasureValue Wely, MeasureValue Welz, MeasureValue Wply, MeasureValue Wplz,
 MeasureValue AvY, MeasureValue AvZ, MeasureValue RadiusOfGyrationMajor, MeasureValue RadiusOfGyrationMinor,
 MeasureValue Depth, MeasureValue Width, MeasureValue HeatedPerimeter, MeasureValue AxisDistance,
 MeasureValue ShearCentreY, MeasureValue ShearCentreZ, double MonosymmetryFactor,
 Vector3 Centroid = default, Option<SectionForm> Form = default) {
 public MeasureValue LeastDimension => Depth.Si <= Width.Si ? Depth : Width;

 public LtbRoute Ltb =>
  ShearCentreY.Si == 0.0 && ShearCentreZ.Si == 0.0 && MonosymmetryFactor == 0.0 ? LtbRoute.Simplified : LtbRoute.General;

 public void CanonicalBytes(CanonicalWriter w) {
  w.Measure(Area).Measure(Iyy).Measure(Izz).Measure(J).Measure(Iw)
   .Measure(Wely).Measure(Welz).Measure(Wply).Measure(Wplz)
   .Measure(AvY).Measure(AvZ).Measure(RadiusOfGyrationMajor).Measure(RadiusOfGyrationMinor)
   .Measure(Depth).Measure(Width).Measure(HeatedPerimeter).Measure(AxisDistance)
   .Measure(ShearCentreY).Measure(ShearCentreZ).Double(MonosymmetryFactor)
   .Double(Centroid.X).Double(Centroid.Y).Double(Centroid.Z)
   .Optional(Form, static (f, run) => run.Ordinal(f.VertexCount).Ordinal(f.CurvedEdges).Double(f.RadialRatio)
    .Measure(f.Perimeter).Measure(f.Major).Measure(f.Minor));
 }

 public static Fin<SectionProperties> OfMillimetres(
  double areaMm2, double iyyMm4, double izzMm4, double jMm4, double iwMm6,
  double welyMm3, double welzMm3, double wplyMm3, double wplzMm3,
  double avyMm2, double avzMm2, double radiusMajorMm, double radiusMinorMm,
  double depthMm, double widthMm, double heatedPerimeterMm, double axisDistanceMm,
  double shearCentreYMm, double shearCentreZMm, double monosymmetryFactor,
  Vector3 centroidMm, Option<SectionForm> form) =>
  (Column(areaMm2, Millimetre.Area, QuantityType.Area, Dimension.AreaDim, "section-area", strict: true),
   Column(iyyMm4, Millimetre.Quartic, QuantityType.AreaMomentOfInertia, InertiaDim, "section-iyy", strict: true),
   Column(izzMm4, Millimetre.Quartic, QuantityType.AreaMomentOfInertia, InertiaDim, "section-izz", strict: true),
   Column(jMm4, Millimetre.Quartic, Torsion, InertiaDim, "section-j", strict: true),
   Column(iwMm6, Millimetre.Sextic, Warping, WarpingDim, "section-iw", strict: false),
   Column(welyMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-wely", strict: true),
   Column(welzMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-welz", strict: true),
   Column(wplyMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-wply", strict: true),
   Column(wplzMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-wplz", strict: true),
   Column(avyMm2, Millimetre.Area, QuantityType.Area, Dimension.AreaDim, "section-avy", strict: true))
  .Apply(static (area, iyy, izz, j, iw, wely, welz, wply, wplz, avy) => (area, iyy, izz, j, iw, wely, welz, wply, wplz, avy))
  .As()
  .Bind(head =>
   (Column(avzMm2, Millimetre.Area, QuantityType.Area, Dimension.AreaDim, "section-avz", strict: true),
    Column(radiusMajorMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-radius-major", strict: true),
    Column(radiusMinorMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-radius-minor", strict: true),
    Column(depthMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-depth", strict: true),
    Column(widthMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-width", strict: true),
    Column(heatedPerimeterMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-heated-perimeter", strict: true),
    Column(axisDistanceMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-axis-distance", strict: false),
    Column(shearCentreYMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-shear-centre-y", strict: false),
    Column(shearCentreZMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-shear-centre-z", strict: false),
    Finite(("section-monosymmetry", monosymmetryFactor)))
   .Apply((avz, rmaj, rmin, depth, width, heated, axis, scy, scz, _) =>
    new SectionProperties(head.area, head.iyy, head.izz, head.j, head.iw,
     head.wely, head.welz, head.wply, head.wplz, head.avy,
     avz, rmaj, rmin, depth, width, heated, axis, scy, scz, monosymmetryFactor,
     new Vector3(centroidMm.X * Millimetre.Length, centroidMm.Y * Millimetre.Length, centroidMm.Z * Millimetre.Length), form))
   .As().ToFin());

 public FrameConstants Lower() =>
  new(Area.Si, AvY.Si, AvZ.Si, Iyy.Si, Izz.Si, J.Si, Iw.Si);

 static readonly QuantityType Modulus = QuantityType.Create("SectionModulus");
 static readonly QuantityType Torsion = QuantityType.Create("TorsionConstant");
 static readonly QuantityType Warping = QuantityType.Create("WarpingConstant");
 static readonly Dimension InertiaDim = Dimension.Create(4, 0, 0, 0, 0, 0, 0);
 static readonly Dimension WarpingDim = Dimension.Create(6, 0, 0, 0, 0, 0, 0);

 static Validation<Error, MeasureValue> Column(double valueMm, double factor, QuantityType type, Dimension dimension, string name, bool strict) =>
  (strict ? In(valueMm, Band.Positive, name) : Finite((name, valueMm)).Map(_ => valueMm))
   .Bind(value => MeasureValue.OfSi(type, dimension, value * factor).ToValidation());

 static class Millimetre {
  internal const double Length = 1e-3;
  internal const double Area = 1e-6;
  internal const double Cubic = 1e-9;
  internal const double Quartic = 1e-12;
  internal const double Sextic = 1e-18;
 }
}

public readonly record struct SectionForm(
 int VertexCount, int CurvedEdges, double RadialRatio, MeasureValue Perimeter, MeasureValue Major, MeasureValue Minor);

public readonly record struct FrameConstants(
 double Area, double ShearAreaY, double ShearAreaZ, double Iy, double Iz, double Torsion, double Warping);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LtbRoute {
 public static readonly LtbRoute Simplified = new("simplified");
 public static readonly LtbRoute General = new("general");
}

[Equatable]
public readonly partial record struct MaterialLayer(
 MaterialId Material, MeasureValue Thickness, string LayerName,
 Option<int> Priority = default, string Category = "", Option<bool> Ventilated = default);

[Equatable]
public readonly partial record struct MaterialConstituent(
 MaterialId Material, string Category, double Fraction, string PartName = "");

[Equatable]
public readonly partial record struct MaterialProfile(
 MaterialId Material, ProfileRef Profile,
 Option<int> Priority = default, string Category = "",
 [property: OrderedEquality] Seq<MeasureValue> Offsets = default);

// --- [MODELS]
public readonly record struct PropertyEvidence {
 private PropertyEvidence(string source, Option<string> reference, Option<LocalDate> validUntil, EvidenceGrade grade, Option<Attestation> attested, Option<EvidenceRun> run) =>
  (Source, Reference, ValidUntil, Grade, Attested, Run) = (source, reference, validUntil, grade, attested, run);

 public string Source { get; }
 public Option<string> Reference { get; }
 public Option<LocalDate> ValidUntil { get; }
 public EvidenceGrade Grade { get; }
 public Option<Attestation> Attested { get; }
 public Option<EvidenceRun> Run { get; }

 public static readonly PropertyEvidence Catalogue =
  new("catalogue", None, Option<LocalDate>.None, EvidenceGrade.Catalogue, None, None);

 public static PropertyEvidence Of(string source, EvidenceGrade grade,
  Option<string> reference = default, Option<LocalDate> validUntil = default,
  Option<Attestation> attested = default, Option<EvidenceRun> run = default) {
  string kind = (source ?? "").Trim();
  return kind.Length == 0
   ? Catalogue
   : new(kind.ToLowerInvariant(), reference.Map(static r => r.Trim()).Filter(static r => r.Length != 0), validUntil, grade, attested, run);
 }

 public static PropertyEvidence Declaration(string source, string reference, LocalDate validUntil) =>
  Of(source, EvidenceGrade.Import, Some(reference), Some(validUntil));

 public bool Citable(LocalDate asOf) => Grade.Attributable && ValidUntil.ForAll(until => asOf <= until);

 public PropertyEvidence Normalized() => Source is null ? Catalogue : this;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AttestationRole {
 public static readonly AttestationRole Manufacturer = new("manufacturer", independentAuthority: false);
 public static readonly AttestationRole ManufacturerAuthorized = new("manufacturer-authorized", independentAuthority: false);
 public static readonly AttestationRole Purchaser = new("purchaser", independentAuthority: true);
 public static readonly AttestationRole Independent = new("independent", independentAuthority: true);
 public static readonly AttestationRole Quality = new("quality", independentAuthority: false);
 public static readonly AttestationRole Regulator = new("regulator", independentAuthority: true);
 public static readonly AttestationRole WeldingInspector = new("welding-inspector", independentAuthority: true);
 public static readonly AttestationRole CalibrationLaboratory = new("calibration-laboratory", independentAuthority: true);
 public static readonly AttestationRole MaterialReviewBoard = new("material-review-board", independentAuthority: true);
 public static readonly AttestationRole SustainabilityVerifier = new("sustainability-verifier", independentAuthority: true);

 public bool IndependentAuthority { get; }
}

public readonly record struct Attestation(AttestationRole Role, string Credential, ContentAddress Payload, Instant At);

[Equatable]
public sealed partial record SampledCurve {
 [property: OrderedEquality] public ImmutableArray<double> Axis { get; }
 [property: OrderedEquality] public ImmutableArray<double> Values { get; }
 [IgnoreEquality] private readonly CalculusInterpolant fit;

 private SampledCurve(ImmutableArray<double> axis, ImmutableArray<double> values, CalculusInterpolant fit) =>
  (Axis, Values, this.fit) = (axis, values, fit);

 public static Fin<SampledCurve> Of(ReadOnlyMemory<double> axis, ReadOnlyMemory<double> values) =>
  axis.Length < 2 || axis.Length != values.Length
   ? new ElementFault.ValueRejected($"<curve-arity:axis={axis.Length}:values={values.Length}>")
   : (Indexed(axis.Span, double.IsFinite, "curve-axis"), Indexed(values.Span, double.IsFinite, "curve-value"))
    .Apply(static (_, _) => unit).As().ToFin()
    .Bind(_ => NotIncreasing(axis.Span)
     ? new ElementFault.ValueRejected("<curve-axis-not-increasing>")
     : Interpolant.LinearSpline(toArray(axis.ToArray()), toArray(values.ToArray()))
        .Map(fitted => new SampledCurve([.. axis.Span], [.. values.Span], fitted)));

 public Fin<double> At(double x) =>
  double.IsFinite(x)
   ? AtAdmitted(x)
   : new KernelFault.OutOfRange("curve-query", x, "be finite");

 internal Fin<double> AtAdmitted(double x) {
  ReadOnlySpan<double> a = Axis.AsSpan();
  return fit.Evaluate(Math.Clamp(x, a[0], a[^1]));
 }

 public void CanonicalBytes(CanonicalWriter w) =>
  w.Doubles(Axis.AsSpan()).Doubles(Values.AsSpan());

 private static bool NotIncreasing(ReadOnlySpan<double> axis) {
  for (int i = 1; i < axis.Length; i++) { if (axis[i] <= axis[i - 1]) { return true; } }
  return false;
 }
}

[Union]
public abstract partial class MaterialComposition {
 private MaterialComposition() { }

 [Equatable] public sealed partial class Single(MaterialId material) : MaterialComposition { public MaterialId Material { get; } = material; }

 [Equatable]
 public sealed partial class LayerSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialLayer> Layers { get; }
  private LayerSet(Seq<MaterialLayer> layers) => Layers = layers;
  internal static LayerSet Seed(Seq<MaterialLayer> layers) => new(layers);
  public double TotalThickness => Layers.Sum(static l => l.Thickness.Si);
 }

 [Equatable]
 public sealed partial class ProfileSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialProfile> Profiles { get; }
  public Option<ProfileRef> Composite { get; }
  public Option<SectionProperties> Section { get; }
  private ProfileSet(Seq<MaterialProfile> profiles, Option<ProfileRef> composite, Option<SectionProperties> section) =>
   (Profiles, Composite, Section) = (profiles, composite, section);
  internal static ProfileSet Seed(Seq<MaterialProfile> profiles, Option<ProfileRef> composite, Option<SectionProperties> section) =>
   new(profiles, composite, section);
  public MaterialProfile Primary => Profiles[0];
  public MaterialId Material => Primary.Material;
  public ProfileRef Profile => Composite.IfNone(Primary.Profile);
  public ProfileSet With(SectionProperties section) => new(Profiles, Composite, Some(section));
 }

 [Equatable]
 public sealed partial class ConstituentSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialConstituent> Constituents { get; }
  private ConstituentSet(Seq<MaterialConstituent> constituents) => Constituents = constituents;
  internal static ConstituentSet Seed(Seq<MaterialConstituent> constituents) => new(constituents);
 }

 public (Seq<MaterialId> All, MaterialId Primary) Census => Switch(
  single: static s => (Seq(s.Material), s.Material),
  layerSet: static s => (s.Layers.Map(static l => l.Material), s.Layers.OrderByDescending(static l => l.Thickness.Si).First().Material),
  profileSet: static s => (s.Profiles.Map(static p => p.Material), s.Material),
  constituentSet: static s => (s.Constituents.Map(static c => c.Material), s.Constituents.OrderByDescending(static c => c.Fraction).First().Material));

 public Seq<MaterialId> Materials => Census.All;
 public MaterialId PrimaryMaterial => Census.Primary;

 public MaterialComposition WithSection(SectionProperties section) =>
  this is ProfileSet ps ? ps.With(section) : this;

 public void CanonicalBytes(CanonicalWriter w) => Switch(
  single: s => w.Ordinal(0).String(s.Material.ToValue()),
  layerSet: s => w.Ordinal(1).Rows(s.Layers, static (l, run) => run
   .String(l.Material.ToValue()).Measure(l.Thickness).String(l.LayerName).String(l.Category)
   .Optional(l.Priority, static (p, deep) => deep.Ordinal(p))
   .Optional(l.Ventilated, static (v, deep) => deep.Bool(v))),
  profileSet: s => w.Ordinal(2)
   .Rows(s.Profiles, static (p, run) => run
    .String(p.Material.ToValue()).String(p.Profile.Standard).String(p.Profile.Designation).U128(p.Profile.ContentKey)
    .String(p.Category)
    .Optional(p.Priority, static (v, deep) => deep.Ordinal(v))
    .Rows(p.Offsets, static (o, deep) => deep.Measure(o)))
   .Optional(s.Composite, static (c, run) => run.String(c.Standard).String(c.Designation).U128(c.ContentKey))
   .Optional(s.Section, static (x, run) => x.CanonicalBytes(run)),
  constituentSet: s => w.Ordinal(3).Rows(s.Constituents, static (c, run) => run
   .String(c.Material.ToValue()).String(c.Category).String(c.PartName).Double(c.Fraction)));

 private const double FractionTolerance = 1e-3;
 private const int PriorityCeiling = 100;
 private const int OffsetArityCeiling = 2;

 public static MaterialComposition OfSingle(MaterialId material) => new Single(material);

 public static MaterialComposition OfProfileSet(MaterialId material, ProfileRef profile) =>
  ProfileSet.Seed(Seq(new MaterialProfile(material, profile)), Option<ProfileRef>.None, Option<SectionProperties>.None);

 public static Fin<MaterialComposition> OfProfileSet(Seq<MaterialProfile> profiles, Option<ProfileRef> composite = default) =>
  (Gate(!profiles.IsEmpty, "<profile-set-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Accumulate(profiles.Map((profile, index) => profile.Priority.Match(
    Some: priority => InRange(priority, 0, PriorityCeiling, $"profile-priority[{index}]").Map(static _ => unit),
    None: static () => Success<Error, Unit>(unit)))),
   Accumulate(profiles.Map((profile, index) => Gate(profile.Offsets.Count <= OffsetArityCeiling, $"<profile-offset-arity:index={index}:count={profile.Offsets.Count}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)))))
  .Apply((_, _, _) => (MaterialComposition)ProfileSet.Seed(profiles, composite, Option<SectionProperties>.None))
  .As().ToFin();

 public static Fin<MaterialComposition> OfLayerSet(Seq<MaterialLayer> layers) =>
  (Gate(!layers.IsEmpty, "<layer-set-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Accumulate(layers.Map((layer, index) => In(layer.Thickness.Si, Band.Positive, $"layer-thickness[{index}]").Map(static _ => unit))),
   Accumulate(layers.Map((layer, index) => layer.Priority.Match(
    Some: priority => InRange(priority, 0, PriorityCeiling, $"layer-priority[{index}]").Map(static _ => unit),
    None: static () => Success<Error, Unit>(unit)))))
  .Apply((_, _, _) => (MaterialComposition)LayerSet.Seed(layers))
  .As().ToFin();

 public static Fin<MaterialComposition> OfConstituentSet(Seq<MaterialConstituent> constituents) =>
  (Gate(!constituents.IsEmpty, "<constituent-set-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Accumulate(constituents.Map((constituent, index) => In(constituent.Fraction, Band.Unit, $"constituent-fraction[{index}]").Map(static _ => unit))))
  .Apply(static (_, _) => unit)
  .As().ToFin()
  .Bind(_ => Math.Abs(constituents.Sum(static constituent => constituent.Fraction) - 1.0) <= FractionTolerance
   ? Fin.Succ<MaterialComposition>(ConstituentSet.Seed(constituents))
   : new ElementFault.ValueRejected($"<constituent-fraction-not-normalized:{constituents.Sum(static constituent => constituent.Fraction):R}>"));
}
```

## [03]-[MATERIAL_PROPERTY]

- Owner: `MaterialPropertySet` the `[Union]` typed engineering-property family keyed to `Discipline`; `FireRating` the `[SmartEnum<string>]` reaction-to-fire class with `EuroclassSuffix` the EN 13501-1 sub-classification pair; `FireCoverage` the criterion presence rows and `FireResistance` the EN 13501-2 R/E/I criteria; the `Of` admissions coercing each measured column through `Properties/quantity#MEASURE_VALUE` and gating scalars on the kernel `Band` rows through `[ADMISSION_SLOTS]` `In`/`InRange`.
- Cases: `Mechanical` (density / Young's modulus / yield strength / ultimate strength as `MeasureValue`, Poisson's ratio + thermal-expansion as guarded dimensionless doubles, the isotropic shear modulus DERIVED `G = E/(2(1+ν))`, and the optional `YoungsReduction`/`YieldReduction` temperature-reduction `SampledCurve` pair — factor vs °C, the fire-route stiffness/strength decay evidence beside the 20 °C scalars — `Discipline.Structural`) · `Orthotropic` (density / the two principal moduli `E1∥`/`E2⊥` / the INDEPENDENT measured shear modulus `G` / the two principal strengths as `MeasureValue` + thermal-expansion, with the matching optional `ModulusReduction`/`StrengthReduction` pair — the directional-stiffness carrier the isotropic `Mechanical` structurally cannot model, the `Rasm.Materials` `timber#TIMBER_FAMILY` consumer's contract home, also `Discipline.Structural` so the case TYPE discriminates an isotropic from a directional material) · `Thermal` (conductivity / specific heat / U-value as `MeasureValue` + vapour-resistance factor μ as a guarded dimensionless double for EN 13788 Glaser condensation — `Discipline.Thermal`) · `Acoustic` (the `Composition/acoustic#ACOUSTIC_FOLDS` banded carrier with its `DynamicStiffnessMNPerM3`/`FlowResistivityPaSPerM2`/`LossFactor` intrinsic constants forwarded — `Discipline.Acoustic`) · `Fire` (an optional `FireRating` reaction class + its `EuroclassSuffix` pair + a `FireResistance` R/E/I rating — `Discipline.Fire`) · `Environmental` (a `MeasurementBasis` declared unit + the EN 15804+A2 `(ImpactCategory × LifecycleStage)` row-major flat `Impacts` matrix that is the ONE impact store + `RecycledContent`/`EndOfLifeRecovery` fractions + EPD provenance, with the general `IndicatorAt(category, stage)` cell read and the `WholeLife(category)` cross-stage fold, and the carbon-keyed convenience projections `Gwp` (the DERIVED cradle-to-gate `(GwpTotal, A1A3)` cell, never a parallel stored scalar), `StageAt`, `WholeLifeGwp`, and the DERIVED `StageGwp` per-module GwpTotal-row vector (the `[A1A3..D]` carbon row sliced from the matrix via `IndicatorAt(GwpTotal, stage)`, never a parallel stored 6-vector, the `Rasm.Compute` carbon fold reads one-hop) over them — `Discipline.Environmental`) · `Cost` (supply / install / lifecycle per-unit columns over a `Currency` + `MeasurementBasis` — `Discipline.Cost`) · `Damping` (the EN 1998-1 fraction-of-critical `DampingRatio` ζ + the optional per-material Rayleigh `(α, β)` proportional-damping pair a time-history FE model reads + the DERIVED hysteretic `StructuralLossFactor = 2ζ` — `Discipline.Dynamic`) · `Hygrothermal` (the EN 15026/WUFI transient-simulation inputs the steady-state `Thermal` case cannot model: `Porosity`, the `WaterContent80Rh`/`FreeWaterSaturation` sorption-isotherm anchors as MoistureStorage-typed kg/m³ measures, the optional capillary `WaterAbsorptionKgPerM2SqrtS` A-value, and the three optional `SampledCurve` measured functions — the full sorption isotherm `w(φ)`, the liquid-transport table `Dw(w)`, the moisture-dependent conductivity `λ(w)` — the WUFI-class solver runs on where the anchors under-model — `Discipline.Hygrothermal`) · `Durability` (the fib Model Code service-life inputs: `CarbonationRateMmPerSqrtYear` K, the `ChlorideDiffusion` D_RCM as a ChlorideDiffusivity-typed m²/s measure, the `AgeingExponent` decay fraction — `Discipline.Durability`) · `Optical` (the IFC `Pset_MaterialOptical` / EnergyPlus `WindowMaterial:Glazing` solar-optical record: per-band transmittance with side-asymmetric front/back reflectances for the visible and solar bands, thermal-IR transmittance with front/back hemispherical emissivities, the band absorptances DERIVED conservation remainders — `Discipline.Energy`) · `Electrical` (the IEC 60364 / NEC substance constants a conductor-, insulation-, or shielding-material declares: `Resistivity` the DC volume resistivity as an ElectricResistivity-typed Ω·m measure, `RelativePermittivity` the dielectric constant εr as a guarded ≥ 1 dimensionless double, the optional `DielectricStrength` breakdown field as a DielectricStrength-typed V/m measure, and the optional `MagneticPermeabilityRelative` μr ratio — `Discipline.Electrical`); a property is a `MaterialPropertySet` case over a `MaterialId`, never a property subtype, and a single-indicator GWP-only environmental model is the deleted 1-of-13 slice of the EN 15804+A2 indicator family.
- Law: FAULT ARITY selects the admission idiom, and the two idioms this branch carries are principled peers, never drift. `[ComplexValueObject]` with `ValidateFactoryArguments` owns SINGLE-fault shape-and-trim admission — one product, one refusal, the generated `Validate` the only authority (`Currency`'s alpha-3 shape gate, `Classification/classification#CLASSIFICATION_AXIS` `Classification`, `Geospatial/reference#GEO_REFERENCE` `ProjectedCrs`) — because the generated factory spine returns at most one error and re-minting the hook to accumulate is unmanufacturable. Hand-rolled private-ctor accumulating `Of` triads earn their place exactly where MULTI-SLOT accumulation IS the contract — every `MaterialPropertySet` case, `FireResistance`, `SampledCurve`, `MaterialComposition`'s three set cases — because a datasheet with three bad columns must report three named faults in one `Fin.Fail`. Owners cross from the first idiom to the second only when the independent-column count passes one; converting a single-fault owner to the triad buys nothing and forfeits the generated `Validate`, `Create`, and equality surface.
- Entry: `MaterialPropertySet.OfMechanical(density, youngsModulus, yieldStrength, ultimateStrength, poissons, thermalExpansion, key, evidence, youngsReduction, yieldReduction)` / `OfOrthotropic(density, e1Parallel, e005Parallel, e2Perpendicular, shearModulus, strength1Parallel, strength2Perpendicular, thermalExpansion, key, evidence, modulusReduction, strengthReduction)` (the trailing `Option<SampledCurve>` pairs the factor-vs-°C temperature-reduction evidence, arriving already admitted through `SampledCurve.Of` and riding as pass-through columns — the `Thermal` `conductivityCurve` shape; `e005Parallel` an `Option` on BOTH arities — the measured fifth-percentile parallel stiffness every timber grade prints and a fractile-less directional source omits, the EN 1995 stability kernels refusing on absence rather than reconstructing a ratio) / `OfThermal(conductivity, specificHeat, uValue, vapourResistanceFactor, key, evidence, conductivityCurve)` (`uValue` an `Option` on BOTH arities — a substance declares no transmittance and the EN ISO 6946 assembly fold owns U, so only a product-declared transmittance fills it) / `OfAcoustic(acoustic)` / `OfFire(rating, resistance)` (+ the full `OfFire(rating, suffix, resistance)`) / `OfEnvironmental(basis, impacts, recycledContent, endOfLifeRecovery, key)` (the `impacts` an `ImmutableArray<double>` of arity `ImpactCategory.Items.Count × LifecycleStage.Items.Count`; the two resource fractions `Option<double>` — scenario data many declarations omit, absence never a fabricated fraction; EPD identity + `LocalDate` expiry ride the `evidence` argument as `PropertyEvidence.Declaration("epd", id, validUntil)`, never per-case columns) / `OfCost(basis, currency, supply, install, lifecycle, key)` (the factory's leading pair mirrors the `Cost` case ctor, so no call site reorders one against the other) / `OfDamping(dampingRatio, rayleigh)` / `OfHygrothermal(porosity, waterContent80Rh, freeWaterSaturation, waterAbsorption)` / `OfDurability(carbonationRate, chlorideDiffusion, ageingExponent)` / `OfOptical(visibleTransmittance, visibleReflectanceFront, visibleReflectanceBack, solarTransmittance, solarReflectanceFront, solarReflectanceBack, thermalIrTransmittance, thermalIrEmissivityFront, thermalIrEmissivityBack)` — `OfMechanical`, `OfOrthotropic`, and `OfThermal` each DECLARE both a raw-double and a typed-`MeasureValue` arity discriminating on input shape, the typed form owning the ONE slot set and the raw form coercing its declared-unit doubles (those coercions accumulating among themselves) before delegating into it, so a producer holding `QuantityRow`-minted columns keeps its propagated `MeasureBand` instead of unwrapping to a declared-unit double and re-coercing, and the two arities cannot drift; the typed smart-constructors coerce each measured column to its SI base and guard the dimensionless ratios, every multi-column form an ACCUMULATING admission (each independent column one slot — the shared `Rasm/Domain/validation#ADMISSION_SLOTS` fold on its concrete carrier — the tuple `.Apply` unioning kernel scalar and Element semantic refusals through `Error.Combine`/`ManyErrors`, `.As().ToFin()` collapsing once at the shared return — the public result stays `Fin<T>`, so consumers are untouched while a bad datasheet reports ALL offending columns; the total `OfAcoustic`/`OfFire` carry no invariant and return the bare case; `OfHygrothermal` (whose trailing `Option<SampledCurve>` `sorptionIsotherm`/`liquidTransport`/`moistureConductivity` columns arrive already admitted through `SampledCurve.Of` and ride as pass-through evidence) binds its `wf >= w80` isotherm refinement AND the curve↔anchor agreement refinement (`Disagrees` at `φ=0.8`/`φ=1.0` within `IsothermAnchorTolerance`) AFTER the accumulated leaves, and `OfOptical` accumulates its six per-band-per-side `τ + ρ <= 1`/`τIR + ε <= 1` conservation refinements as a SECOND stage after the nine in-unit leaves, the COMPOSITE_ADMISSION order) / `OfElectrical(resistivityOhmM, relativePermittivity, dielectricStrengthVPerM, magneticPermeabilityRelative)`; `Discipline` reads the case-to-discipline map; the named per-discipline reads derive from ONE private polymorphic `Property<T>()` body (a future case lands its one-line forward — the generic read carried zero external consumers, so it no longer ships public), and `props.Density` is the cross-case substance read over both stiffness carriers.
- Auto: `MaterialPropertySet` is a CLASS-root `[Union]` + `[Equatable]` (the `[GRAPH_FAMILY]` form), so the generated `Switch`/`Map` survive while structural equality and the member diff ride `Generator.Equals` — the `Graph/element#NODE_MODEL` `Node.Material` `[Equatable]` drill descends into each case's columns (a record-root case is an opaque equality leaf that collapses the `Rasm.Persistence` `StructuralMerge` to whole-property replacement); `Discipline` dispatches the generated `Switch` mapping each case to its row (`Mechanical`/`Orthotropic`→`Structural`, `Damping`→`Dynamic`, `Hygrothermal`→`Hygrothermal`, `Durability`→`Durability`, `Optical`→`Energy`, …, `Cost`→`Cost`); the `Of` constructors route each dimensioned value through `MeasureValue.Of(value, UnitsNet.Units.X, key)` (or the TYPED `OfSi` for the registry-less MoistureStorage/ChlorideDiffusivity signatures) so the column carries its SI base and `Dimension`, the Poisson's ratio guarded to the physical isotropic `[0,0.5]` range (the `is >= 0.0 and <= 0.5` relational pattern rejecting an out-of-range ratio AND a `NaN`), every density/stiffness/strength/conductivity column guarded finite-AND-strictly-positive through the per-column `Positive` slot (a negative MPa is finite, so the `MeasureValue.Of` finiteness gate alone admits a physically-impossible negative-stiffness material the contract rejects BY NAME), the dimensionless ratios and the `MeasurementBasis`-relative fractions guarded finite-and-in-unit through the same NaN-rejecting relational patterns, and the raw-double cost columns guarded finite-and-non-negative (the `MeasureValue` finiteness gate never sees the raw-double `Cost`/`Environmental`-fraction carriers, so a bare `< 0.0` guard admits `NaN`/∞ into the content hash is rejected at admission) — every such miss ACCUMULATED across the constructor's independent slots, never first-fault-wins; the `Mechanical` shear modulus is a DERIVED read off `E` and `ν` (the isotropic relation `G = E/(2(1+ν))`), never a drift-prone stored column; the `Acoustic` case wraps the `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` carrier whose `Nrc`/`Saa`/`StcWeighted` are derived reads; the `Fire` case carries the EN 13501-1 reaction class with its smoke/droplet sub-class and the EN 13501-2 R/E/I `FireResistance`; the `Environmental` case stores the EN 15804+A2 impact matrix row-major flat and `OfEnvironmental` guards its `Environmental.MatrixArity` and finiteness once so the derived `IndicatorAt`/`Gwp`/`WholeLife` reads trust the admission.
- Output: a `Seq<MaterialPropertySet>` on a `Material` node is the full engineering profile a `Bake`-derived `Element` reads flat — `props.Thermal.Bind(t => t.UValue)`, `props.Mechanical.Map(m => m.YieldStrength)`, `props.Acoustic.Map(a => a.StcWeighted)`, `props.Damping.Map(d => d.DampingRatio)`, `props.Durability.Map(u => u.ChlorideDiffusion)`, or the generic `props.Property<T>()` for a future case before its named forward lands — one node carrying every discipline keyed by `Discipline`; the `Rasm.Compute` analysis route reads the `MeasureValue` columns by `Discipline`, and the assembly aggregation (series-resistance U-value, rule-of-mixtures density, layered STC) folds the `MaterialComposition` plies in Compute, never re-keyed per assembly.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`/`[ValueObject<string>]`), Generator.Equals (`[Equatable]` the class-root `MaterialPropertySet` union's structural equality + the member diff the `Rasm.Persistence` `StructuralMerge` drills, `[OrderedEquality]` the `Environmental.Impacts` matrix), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Validation<Error,_>` the accumulating admission slots joined by the tuple `.Apply` and collapsed `.ToFin()`/`Choose`/`Find`), `Rasm/Domain/validation#ADMISSION_SLOTS` (the shared `Guarded`-sibling slots — `Gate`, `Accumulate`, `Optional` — the material-domain combinators sit beside), NodaTime (`LocalDate` the `PropertyEvidence.ValidUntil` calendar expiry — the exact EPD/declaration date the procurement filter compares, over the deleted lossy int-year), UnitsNet (via `MeasureValue`), System.Collections.Immutable (`ImmutableArray<double>` the immutable impact-matrix store), `Rasm/Domain/identity#CONTENT_KEY` (`CanonicalWriter` the `MaterialPropertySet.CanonicalBytes` content projection writes through).
- Growth: a new engineering property shared across materials is one column on its `MaterialPropertySet` case; a new property discipline with no fit is one `MaterialPropertySet` case carrying its `Discipline` — never a parallel `Eco`/`Cost` owner (the `Damping`/`Hygrothermal`/`Durability`/`Optical`/`Electrical` cases are this law EXECUTED: each one case + one `Discipline` row + one next-free `CanonicalBytes` ordinal + one named forward, zero new surfaces beside the union); a new fire-reaction class is one `FireRating` row, a new acoustic rating one fold on the `Acoustic` carrier, a new EN 15804 environmental indicator one `ImpactCategory` row (the `Impacts` matrix widens by one indicator row and `IndicatorAt`/`WholeLife` read it with no new column or method); a new admission invariant is one `[ADMISSION_SLOTS]` combinator slot, never a per-constructor guard chain; a new state-dependent measured function (a temperature-dependent modulus, a moisture-dependent property) is one `Option<SampledCurve>` column on its owning case and one `Curve` canon write — the ONE sampled-function carrier, never a per-curve column spray and never a lossy point compression; the family grows by case, column, and vocabulary row, never by a per-discipline material type, and the typed lookup grows by ONE generic `Property<T>()` over the case type and an ergonomic named forward, never a per-case roster of independent `Choose` bodies.
- Boundary: `MaterialPropertySet` is the ONE typed property family — a per-discipline material type is the deleted form, a property being a case over a `MaterialId`. The family is FULL-VECTOR and AUTHORED-only: an imported foreign `Pset` lands as a neutral `PROPERTY_BAG` node under `EvidenceGrade.Import`, never Option-widened columns (widening fabricates every undeclared column); the CARVE is the column an authored producer structurally cannot declare (product-only `UValue`, scenario fractions, tested-system-only fire `Reaction`) — each an `Option` whose absence is the domain's own state. Every dimensioned column admits through `MeasureValue`; the isotropic `G` DERIVES from `E`/`ν` while a measured directional `G` rides `Orthotropic`; fire is a closed reaction vocabulary + the typed R/E/I criteria (a single scalar cannot tell `R 90` from `EI 60`); the `Acoustic` case is the banded carrier, never a scalar STC. The `Cost` case carries neutral per-unit doubles over an OPAQUE ISO 4217 `Currency` + `MeasurementBasis` (`Rasm.Bim`'s NodaMoney algebra owns the roster and the quantity×rate join). The `Environmental` case's flat `(ImpactCategory × LifecycleStage)` `ImmutableArray` matrix is the ONE impact store (13 indicators × 6 modules; a GWP-only vector is the deleted 1-of-13 slice; a `Map` of arrays reference-compares under Generator.Equals) — `IndicatorAt`/`Gwp`/`WholeLife`/`StageGwp` DERIVE from it, `OfEnvironmental` gates arity-then-finite once (`TensorPrimitives.IsFiniteAll`), an EPD declaring fewer indicators zeroes the rest so arity is invariant, and every cell rides the case's `MeasurementBasis` so the Compute folds scale by the basis-matching quantity. Provenance is SINGLE-stored on `PropertyEvidence` (a per-case EPD column pair double-stores it; an int year is lossy against a full expiry date). `Damping.DampingRatio` (large-strain design ζ) never derives from the acoustic small-strain `LossFactor` η — different standards at amplitude regimes apart — while `StructuralLossFactor = 2ζ` is the case's own derived input. `Optical` carries the EN 410 / EnergyPlus engineering constants with side-asymmetric fronts/backs, absorptances DERIVED as conservation remainders and the `Conserves` slot refusing an unphysical datasheet; render appearance stays the `Rasm.Materials` Appearance owner's. `Electrical` carries SUBSTANCE constants alone (ampacity is a component row). Fractional-exponent quantities (mm/√year, kg/(m²·√s)) stay raw doubles with the unit in the NAME — √t is inexpressible in the integer 7-vector — while every integer-dimension column is a typed `MeasureValue` under the same-dimension-distinct-type discipline.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FireRating {
 public static readonly FireRating A1 = new("A1", combustible: false);
 public static readonly FireRating A2 = new("A2", combustible: false);
 public static readonly FireRating B = new("B", combustible: true);
 public static readonly FireRating C = new("C", combustible: true);
 public static readonly FireRating D = new("D", combustible: true);
 public static readonly FireRating E = new("E", combustible: true);
 public static readonly FireRating F = new("F", combustible: true);
 public bool Combustible { get; }

}

[ComplexValueObject]
[ObjectFactory<string>]
[ValidationError]
public sealed partial class EuroclassSuffix {
 public string Smoke { get; }
 public string Droplets { get; }

 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string smoke, ref string droplets) {
  smoke = smoke.Trim().ToLowerInvariant();
  droplets = droplets.Trim().ToLowerInvariant();
  validationError =
   smoke is not ("" or "s1" or "s2" or "s3") ? new ValidationError($"unknown Euroclass smoke suffix {smoke}")
   : droplets is not ("" or "d0" or "d1" or "d2") ? new ValidationError($"unknown Euroclass droplets suffix {droplets}")
   : validationError;
 }

 public static readonly EuroclassSuffix NotSpecified = Create("", "");

 public static ValidationError? Validate(string? suffix, IFormatProvider? provider, out EuroclassSuffix? item) {
  string[] parts = (suffix ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
  if (parts.Length > 2) { item = null; return new ValidationError("Euroclass suffix accepts smoke and droplets columns"); }
  (string smoke, string droplets) = parts switch {
   [] => ("", ""),
   [var sole] => sole.StartsWith('d') ? ("", sole) : (sole, ""),
   _ => (parts[0], parts[1]),
  };
  return Validate(smoke, droplets, out item);
 }
}

public readonly record struct FireResistance {
 public Option<int> LoadBearingMinutes { get; }
 public Option<int> IntegrityMinutes { get; }
 public Option<int> InsulationMinutes { get; }

 private FireResistance(Option<int> loadBearingMinutes, Option<int> integrityMinutes, Option<int> insulationMinutes) =>
  (LoadBearingMinutes, IntegrityMinutes, InsulationMinutes) = (loadBearingMinutes, integrityMinutes, insulationMinutes);

 public static readonly FireResistance None = new(Option<int>.None, Option<int>.None, Option<int>.None);

 public static Fin<FireResistance> Of(Option<int> loadBearingMinutes, Option<int> integrityMinutes, Option<int> insulationMinutes) =>
  (Gate(loadBearingMinutes.IsSome || integrityMinutes.IsSome || insulationMinutes.IsSome, "<fire-resistance-unmeasured>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Minutes(loadBearingMinutes, "load-bearing"),
   Minutes(integrityMinutes, "integrity"),
   Minutes(insulationMinutes, "insulation"))
  .Apply(static (_, r, e, i) => new FireResistance(r, e, i))
  .As().ToFin();

 public static Fin<FireResistance> Of(FireCoverage coverage, int minutes) =>
  Of(coverage.LoadBearing ? Some(minutes) : None,
     coverage.Integrity ? Some(minutes) : None,
     coverage.Insulation ? Some(minutes) : None);

 private static Validation<Error, Option<int>> Minutes(Option<int> value, string criterion) =>
  value.Exists(static minutes => minutes < 0)
   ? new KernelFault.OutOfRange($"fire-resistance-{criterion}", value.IfNone(0), "be non-negative")
   : value;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FireCoverage {
 public static readonly FireCoverage Rei = new("REI", loadBearing: true, integrity: true, insulation: true);
 public static readonly FireCoverage R = new("R", loadBearing: true, integrity: false, insulation: false);
 public static readonly FireCoverage Ei = new("EI", loadBearing: false, integrity: true, insulation: true);
 public static readonly FireCoverage I = new("I", loadBearing: false, integrity: false, insulation: true);

 public bool LoadBearing { get; }
 public bool Integrity { get; }
 public bool Insulation { get; }
}

[SmartEnum<int>]
public sealed partial class LifecycleStage {
 public static readonly LifecycleStage A1A3 = new(0, "A1-A3");
 public static readonly LifecycleStage A4 = new(1, "A4");
 public static readonly LifecycleStage A5 = new(2, "A5");
 public static readonly LifecycleStage B = new(3, "B1-B7");
 public static readonly LifecycleStage C = new(4, "C1-C4");
 public static readonly LifecycleStage D = new(5, "D");
 public string Module { get; }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Currency {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim().ToUpperInvariant();
  if (value.Length != 3 || !value.All(char.IsAsciiLetterUpper)) { validationError = new ValidationError("currency requires an ISO 4217 alpha-3 code"); }
 }
}

[SmartEnum<string>]
public sealed partial class MeasurementBasis {
 public static readonly MeasurementBasis PerKg = new("per-kg");
 public static readonly MeasurementBasis PerM2 = new("per-m2");
 public static readonly MeasurementBasis PerM3 = new("per-m3");
 public static readonly MeasurementBasis PerItem = new("per-item");
}

[SmartEnum<int>]
public sealed partial class ImpactCategory {
 public static readonly ImpactCategory GwpTotal = new(0, "GWP-total", "kg CO2 eq");
 public static readonly ImpactCategory GwpFossil = new(1, "GWP-fossil", "kg CO2 eq");
 public static readonly ImpactCategory GwpBiogenic = new(2, "GWP-biogenic", "kg CO2 eq");
 public static readonly ImpactCategory GwpLuluc = new(3, "GWP-luluc", "kg CO2 eq");
 public static readonly ImpactCategory Odp = new(4, "ODP", "kg CFC11 eq");
 public static readonly ImpactCategory Ap = new(5, "AP", "mol H+ eq");
 public static readonly ImpactCategory EpFreshwater = new(6, "EP-freshwater", "kg P eq");
 public static readonly ImpactCategory EpMarine = new(7, "EP-marine", "kg N eq");
 public static readonly ImpactCategory EpTerrestrial = new(8, "EP-terrestrial", "mol N eq");
 public static readonly ImpactCategory Pocp = new(9, "POCP", "kg NMVOC eq");
 public static readonly ImpactCategory AdpMinerals = new(10, "ADP-minerals", "kg Sb eq");
 public static readonly ImpactCategory AdpFossil = new(11, "ADP-fossil", "MJ");
 public static readonly ImpactCategory Wdp = new(12, "WDP", "m3 world eq");

 public string Name { get; }
 public string Unit { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial class MaterialPropertySet {
 private MaterialPropertySet(PropertyEvidence evidence) {
  Evidence = evidence.Normalized();
 }

 public PropertyEvidence Evidence { get; }

 [Equatable]
 public sealed partial class Mechanical(MeasureValue density, MeasureValue youngsModulus, MeasureValue yieldStrength, MeasureValue ultimateStrength, double poissonsRatio, double thermalExpansionPerK, PropertyEvidence evidence, Option<SampledCurve> youngsReduction = default, Option<SampledCurve> yieldReduction = default) : MaterialPropertySet(evidence) {
  public MeasureValue Density { get; } = density;
  public MeasureValue YoungsModulus { get; } = youngsModulus;
  public MeasureValue YieldStrength { get; } = yieldStrength;
  public MeasureValue UltimateStrength { get; } = ultimateStrength;
  public double PoissonsRatio { get; } = poissonsRatio;
  public double ThermalExpansionPerK { get; } = thermalExpansionPerK;
  public Option<SampledCurve> YoungsReduction { get; } = youngsReduction;
  public Option<SampledCurve> YieldReduction { get; } = yieldReduction;
  public MeasureValue ShearModulus =>
   MeasureValue.Reproject(QuantitySignature.Of(YoungsModulus), YoungsModulus.Si / (2.0 * (1.0 + PoissonsRatio)));
 }
 [Equatable]
 public sealed partial class Orthotropic(MeasureValue density, MeasureValue e1Parallel, Option<MeasureValue> e005Parallel, MeasureValue e2Perpendicular, MeasureValue shearModulus, MeasureValue strength1Parallel, MeasureValue strength2Perpendicular, double thermalExpansionPerK, PropertyEvidence evidence, Option<SampledCurve> modulusReduction = default, Option<SampledCurve> strengthReduction = default) : MaterialPropertySet(evidence) {
  public MeasureValue Density { get; } = density;
  public MeasureValue E1Parallel { get; } = e1Parallel;
  public Option<MeasureValue> E005 { get; } = e005Parallel;
  public MeasureValue E2Perpendicular { get; } = e2Perpendicular;
  public MeasureValue ShearModulus { get; } = shearModulus;
  public MeasureValue Strength1Parallel { get; } = strength1Parallel;
  public MeasureValue Strength2Perpendicular { get; } = strength2Perpendicular;
  public double ThermalExpansionPerK { get; } = thermalExpansionPerK;
  public Option<SampledCurve> ModulusReduction { get; } = modulusReduction;
  public Option<SampledCurve> StrengthReduction { get; } = strengthReduction;
 }
 [Equatable]
 public sealed partial class Thermal(MeasureValue conductivity, MeasureValue specificHeat, Option<MeasureValue> uValue, double vapourResistanceFactor, PropertyEvidence evidence, Option<SampledCurve> conductivityCurve = default) : MaterialPropertySet(evidence) {
  public MeasureValue Conductivity { get; } = conductivity;
  public MeasureValue SpecificHeat { get; } = specificHeat;
  public Option<MeasureValue> UValue { get; } = uValue;
  public double VapourResistanceFactor { get; } = vapourResistanceFactor;
  public Option<SampledCurve> ConductivityCurve { get; } = conductivityCurve;
 }
 [Equatable]
 public sealed partial class Acoustic(global::Rasm.Element.Composition.Acoustic spectrum, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public global::Rasm.Element.Composition.Acoustic Spectrum { get; } = spectrum;
  public ImmutableArray<double> AbsorptionSpectrum => Spectrum.AbsorptionSpectrum;
  public ImmutableArray<double> SoundReductionIndexDb => Spectrum.SoundReductionIndexDb;
  public Option<double> DynamicStiffnessMNPerM3 => Spectrum.DynamicStiffnessMNPerM3;
  public Option<double> FlowResistivityPaSPerM2 => Spectrum.FlowResistivityPaSPerM2;
  public Option<double> LossFactor => Spectrum.LossFactor;
  public double Nrc => Spectrum.Nrc;
  public double Saa => Spectrum.Saa;
  public int StcWeighted => Spectrum.StcWeighted;
  public int Rw => Spectrum.Rw;
 }
 [Equatable]
 public sealed partial class Fire(Option<FireRating> reaction, EuroclassSuffix suffix, FireResistance resistance, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public Option<FireRating> Reaction { get; } = reaction;
  public EuroclassSuffix Suffix { get; } = suffix;
  public FireResistance Resistance { get; } = resistance;
 }
 [Equatable]
 public sealed partial class Environmental(MeasurementBasis basis, ImmutableArray<double> impacts, Option<double> recycledContent, Option<double> endOfLifeRecovery, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasurementBasis Basis { get; } = basis;
  [property: OrderedEquality] public ImmutableArray<double> Impacts { get; } = impacts;
  public Option<double> RecycledContent { get; } = recycledContent;
  public Option<double> EndOfLifeRecovery { get; } = endOfLifeRecovery;

  public double IndicatorAt(ImpactCategory category, LifecycleStage stage) {
   int i = category.Key * LifecycleStage.Items.Count + stage.Key;
   return i >= 0 && i < Impacts.Length ? Impacts[i] : 0.0;
  }
  public double WholeLife(ImpactCategory category) =>
   LifecycleStage.Items.Sum(stage => IndicatorAt(category, stage));
  public double Gwp => IndicatorAt(ImpactCategory.GwpTotal, LifecycleStage.A1A3);
  public double StageAt(LifecycleStage stage) => IndicatorAt(ImpactCategory.GwpTotal, stage);
  public double WholeLifeGwp => WholeLife(ImpactCategory.GwpTotal);
  public ImmutableArray<double> StageGwp =>
   [.. LifecycleStage.Items.OrderBy(static s => s.Key).Select(s => IndicatorAt(ImpactCategory.GwpTotal, s))];
  public static ImmutableArray<double> CarbonMatrix(ReadOnlyMemory<double> stageGwp) {
   double[] matrix = new double[MatrixArity];
   ReadOnlySpan<double> row = stageGwp.Span;
   int gwpRow = ImpactCategory.GwpTotal.Key * LifecycleStage.Items.Count;
   int stages = Math.Min(row.Length, LifecycleStage.Items.Count);
   for (int s = 0; s < stages; s++) { matrix[gwpRow + s] = row[s]; }
   return [.. matrix];
  }
  public static int MatrixArity => ImpactCategory.Items.Count * LifecycleStage.Items.Count;
  private static readonly Lazy<Environmental> Baseline =
   new(static () => new(MeasurementBasis.PerM3, [.. new double[MatrixArity]], 0.0, 0.0, PropertyEvidence.Catalogue),
    LazyThreadSafetyMode.ExecutionAndPublication);

  public static Environmental Empty => Baseline.Value;
 }
 [Equatable]
 public sealed partial class Cost(MeasurementBasis basis, Currency currency, double supplyPerUnit, double installPerUnit, double lifecyclePerUnit, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasurementBasis Basis { get; } = basis;
  public Currency Currency { get; } = currency;
  public double SupplyPerUnit { get; } = supplyPerUnit;
  public double InstallPerUnit { get; } = installPerUnit;
  public double LifecyclePerUnit { get; } = lifecyclePerUnit;
 }
 [Equatable]
 public sealed partial class Damping(double dampingRatio, Option<(double AlphaPerS, double BetaS)> rayleigh, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double DampingRatio { get; } = dampingRatio;
  public Option<(double AlphaPerS, double BetaS)> Rayleigh { get; } = rayleigh;
  public double StructuralLossFactor => 2.0 * DampingRatio;
 }
 [Equatable]
 public sealed partial class Hygrothermal(double porosity, MeasureValue waterContent80Rh, MeasureValue freeWaterSaturation, Option<double> waterAbsorptionKgPerM2SqrtS, Option<SampledCurve> sorptionIsotherm, Option<SampledCurve> liquidTransport, Option<SampledCurve> moistureConductivity, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double Porosity { get; } = porosity;
  public MeasureValue WaterContent80Rh { get; } = waterContent80Rh;
  public MeasureValue FreeWaterSaturation { get; } = freeWaterSaturation;
  public Option<double> WaterAbsorptionKgPerM2SqrtS { get; } = waterAbsorptionKgPerM2SqrtS;
  public Option<SampledCurve> SorptionIsotherm { get; } = sorptionIsotherm;
  public Option<SampledCurve> LiquidTransport { get; } = liquidTransport;
  public Option<SampledCurve> MoistureConductivity { get; } = moistureConductivity;
 }
 [Equatable]
 public sealed partial class Durability(double carbonationRateMmPerSqrtYear, MeasureValue chlorideDiffusion, double ageingExponent, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double CarbonationRateMmPerSqrtYear { get; } = carbonationRateMmPerSqrtYear;
  public MeasureValue ChlorideDiffusion { get; } = chlorideDiffusion;
  public double AgeingExponent { get; } = ageingExponent;
 }
 [Equatable]
 public sealed partial class Optical(double visibleTransmittance, double visibleReflectanceFront, double visibleReflectanceBack, double solarTransmittance, double solarReflectanceFront, double solarReflectanceBack, double thermalIrTransmittance, double thermalIrEmissivityFront, double thermalIrEmissivityBack, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double VisibleTransmittance { get; } = visibleTransmittance;
  public double VisibleReflectanceFront { get; } = visibleReflectanceFront;
  public double VisibleReflectanceBack { get; } = visibleReflectanceBack;
  public double SolarTransmittance { get; } = solarTransmittance;
  public double SolarReflectanceFront { get; } = solarReflectanceFront;
  public double SolarReflectanceBack { get; } = solarReflectanceBack;
  public double ThermalIrTransmittance { get; } = thermalIrTransmittance;
  public double ThermalIrEmissivityFront { get; } = thermalIrEmissivityFront;
  public double ThermalIrEmissivityBack { get; } = thermalIrEmissivityBack;
  public double SolarAbsorptanceFront => 1.0 - SolarTransmittance - SolarReflectanceFront;
  public double SolarAbsorptanceBack => 1.0 - SolarTransmittance - SolarReflectanceBack;
 }
 [Equatable]
 public sealed partial class Electrical(MeasureValue resistivity, double relativePermittivity, Option<MeasureValue> dielectricStrength, Option<double> magneticPermeabilityRelative, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasureValue Resistivity { get; } = resistivity;
  public double RelativePermittivity { get; } = relativePermittivity;
  public Option<MeasureValue> DielectricStrength { get; } = dielectricStrength;
  public Option<double> MagneticPermeabilityRelative { get; } = magneticPermeabilityRelative;
 }

 public Discipline Discipline => Switch(
  mechanical: static _ => Discipline.Structural,
  orthotropic: static _ => Discipline.Structural,
  thermal: static _ => Discipline.Thermal,
  acoustic: static _ => Discipline.Acoustic,
  fire: static _ => Discipline.Fire,
  environmental: static _ => Discipline.Environmental,
  cost: static _ => Discipline.Cost,
  damping: static _ => Discipline.Dynamic,
  hygrothermal: static _ => Discipline.Hygrothermal,
  durability: static _ => Discipline.Durability,
  optical: static _ => Discipline.Energy,
  electrical: static _ => Discipline.Electrical);

 public void CanonicalBytes(CanonicalWriter w) => Switch(
  mechanical:    m => CaseBytes(w, 0).Measure(m.Density).Measure(m.YoungsModulus).Measure(m.YieldStrength).Measure(m.UltimateStrength).Double(m.PoissonsRatio).Double(m.ThermalExpansionPerK)
   .Optional(m.YoungsReduction, static (c, run) => c.CanonicalBytes(run)).Optional(m.YieldReduction, static (c, run) => c.CanonicalBytes(run)),
  thermal:       t => CaseBytes(w, 1).Measure(t.Conductivity).Measure(t.SpecificHeat)
   .Optional(t.UValue, static (u, run) => run.Measure(u)).Double(t.VapourResistanceFactor)
   .Optional(t.ConductivityCurve, static (c, run) => c.CanonicalBytes(run)),
  acoustic:      a => { CaseBytes(w, 2); a.Spectrum.CanonicalBytes(w); return w; },
  fire:          f => CaseBytes(w, 3)
   .Optional(f.Reaction, static (r, run) => run.String(r.Key))
   .String(f.Suffix.Smoke).String(f.Suffix.Droplets)
   .Optional(f.Resistance.LoadBearingMinutes, static (m, run) => run.Ordinal(m))
   .Optional(f.Resistance.IntegrityMinutes, static (m, run) => run.Ordinal(m))
   .Optional(f.Resistance.InsulationMinutes, static (m, run) => run.Ordinal(m)),
  environmental: e => CaseBytes(w, 4).String(e.Basis.Key).Doubles(e.Impacts.AsSpan())
   .Optional(e.RecycledContent, static (r, run) => run.Double(r))
   .Optional(e.EndOfLifeRecovery, static (r, run) => run.Double(r)),
  cost:          c => CaseBytes(w, 5).String(c.Basis.Key).String(c.Currency.ToValue()).Double(c.SupplyPerUnit).Double(c.InstallPerUnit).Double(c.LifecyclePerUnit),
  orthotropic:   o => CaseBytes(w, 6).Measure(o.Density).Measure(o.E1Parallel).Measure(o.E2Perpendicular).Measure(o.ShearModulus).Measure(o.Strength1Parallel).Measure(o.Strength2Perpendicular).Double(o.ThermalExpansionPerK)
   .Optional(o.ModulusReduction, static (c, run) => c.CanonicalBytes(run)).Optional(o.StrengthReduction, static (c, run) => c.CanonicalBytes(run)),
  damping:       d => CaseBytes(w, 7).Double(d.DampingRatio)
   .Optional(d.Rayleigh, static (r, run) => run.Double(r.AlphaPerS).Double(r.BetaS)),
  hygrothermal:  h => CaseBytes(w, 8).Double(h.Porosity).Measure(h.WaterContent80Rh).Measure(h.FreeWaterSaturation)
   .Optional(h.WaterAbsorptionKgPerM2SqrtS, static (v, run) => run.Double(v))
   .Optional(h.SorptionIsotherm, static (c, run) => c.CanonicalBytes(run))
   .Optional(h.LiquidTransport, static (c, run) => c.CanonicalBytes(run))
   .Optional(h.MoistureConductivity, static (c, run) => c.CanonicalBytes(run)),
  durability:    u => CaseBytes(w, 9).Double(u.CarbonationRateMmPerSqrtYear).Measure(u.ChlorideDiffusion).Double(u.AgeingExponent),
  optical:       o => CaseBytes(w, 10).Double(o.VisibleTransmittance).Double(o.VisibleReflectanceFront).Double(o.VisibleReflectanceBack).Double(o.SolarTransmittance).Double(o.SolarReflectanceFront).Double(o.SolarReflectanceBack).Double(o.ThermalIrTransmittance).Double(o.ThermalIrEmissivityFront).Double(o.ThermalIrEmissivityBack),
  electrical:    e => CaseBytes(w, 11).Measure(e.Resistivity).Double(e.RelativePermittivity)
   .Optional(e.DielectricStrength, static (v, run) => run.Measure(v))
   .Optional(e.MagneticPermeabilityRelative, static (v, run) => run.Double(v)));

 CanonicalWriter CaseBytes(CanonicalWriter w, int ordinal) =>
  w.Ordinal(ordinal).String(Evidence.Source)
   .Optional(Evidence.Reference, static (r, run) => run.String(r))
   .Optional(Evidence.ValidUntil, static (d, run) => run.Ordinal(d.Year).Ordinal(d.Month).Ordinal(d.Day))
   .Ordinal(Evidence.Grade.Key)
   .Optional(Evidence.Attested, static (a, run) => run.String(a.Role.Key).String(a.Credential).U128(a.Payload.ToValue()).I64(a.At.ToUnixTimeTicks()))
   .Optional(Evidence.Run, static (r, run) => r.CanonicalBytes(run));

 public static Fin<MaterialPropertySet> OfMechanical(double density, double youngsModulus, double yieldStrength, double ultimateStrength, double poissons, double thermalExpansion, PropertyEvidence evidence = default, Option<SampledCurve> youngsReduction = default, Option<SampledCurve> yieldReduction = default) =>
  (Coerce(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter),
   Coerce(youngsModulus, UnitsNet.Units.PressureUnit.Megapascal),
   Coerce(yieldStrength, UnitsNet.Units.PressureUnit.Megapascal),
   Coerce(ultimateStrength, UnitsNet.Units.PressureUnit.Megapascal))
  .Apply(static (rho, e, fy, fu) => (Density: rho, Youngs: e, Yield: fy, Ultimate: fu))
  .As().ToFin()
  .Bind(column => OfMechanical(column.Density, column.Youngs, column.Yield, column.Ultimate, poissons, thermalExpansion, evidence, youngsReduction, yieldReduction));

 public static Fin<MaterialPropertySet> OfMechanical(MeasureValue density, MeasureValue youngsModulus, MeasureValue yieldStrength, MeasureValue ultimateStrength, double poissons, double thermalExpansion, PropertyEvidence evidence = default, Option<SampledCurve> youngsReduction = default, Option<SampledCurve> yieldReduction = default) =>
  (Positive(density, "mechanical-density"),
   Positive(youngsModulus, "mechanical-youngs-modulus"),
   Positive(yieldStrength, "mechanical-yield-strength"),
   Positive(ultimateStrength, "mechanical-ultimate-strength"),
   InRange(poissons, 0.0, 0.5, "poisson-isotropic"),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite"))
  .Apply((d, e, y, u, nu, a) => (MaterialPropertySet)new Mechanical(d, e, y, u, nu, a, evidence, youngsReduction, yieldReduction))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfOrthotropic(double density, double e1Parallel, Option<double> e005Parallel, double e2Perpendicular, double shearModulus, double strength1Parallel, double strength2Perpendicular, double thermalExpansion, PropertyEvidence evidence = default, Option<SampledCurve> modulusReduction = default, Option<SampledCurve> strengthReduction = default) =>
  (Coerce(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter),
   Coerce(e1Parallel, UnitsNet.Units.PressureUnit.Megapascal),
   e005Parallel.Traverse(e => Coerce(e, UnitsNet.Units.PressureUnit.Megapascal)).As(),
   Coerce(e2Perpendicular, UnitsNet.Units.PressureUnit.Megapascal),
   Coerce(shearModulus, UnitsNet.Units.PressureUnit.Megapascal),
   Coerce(strength1Parallel, UnitsNet.Units.PressureUnit.Megapascal),
   Coerce(strength2Perpendicular, UnitsNet.Units.PressureUnit.Megapascal))
  .Apply(static (rho, e1, e05, e2, g, s1, s2) => (Density: rho, E1: e1, E005: e05, E2: e2, Shear: g, S1: s1, S2: s2))
  .As().ToFin()
  .Bind(column => OfOrthotropic(column.Density, column.E1, column.E005, column.E2, column.Shear, column.S1, column.S2, thermalExpansion, evidence, modulusReduction, strengthReduction));

 public static Fin<MaterialPropertySet> OfOrthotropic(MeasureValue density, MeasureValue e1Parallel, Option<MeasureValue> e005Parallel, MeasureValue e2Perpendicular, MeasureValue shearModulus, MeasureValue strength1Parallel, MeasureValue strength2Perpendicular, double thermalExpansion, PropertyEvidence evidence = default, Option<SampledCurve> modulusReduction = default, Option<SampledCurve> strengthReduction = default) =>
  (Positive(density, "orthotropic-density"),
   Positive(e1Parallel, "orthotropic-e1-parallel"),
   e005Parallel.Traverse(e => Positive(e, "orthotropic-e005-parallel")).As(),
   Positive(e2Perpendicular, "orthotropic-e2-perpendicular"),
   Positive(shearModulus, "orthotropic-shear-modulus"),
   Positive(strength1Parallel, "orthotropic-strength1-parallel"),
   Positive(strength2Perpendicular, "orthotropic-strength2-perpendicular"),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite"))
  .Apply((rho, e1, e05, e2, g, s1, s2, a) => (MaterialPropertySet)new Orthotropic(rho, e1, e05, e2, g, s1, s2, a, evidence, modulusReduction, strengthReduction))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfThermal(double conductivity, double specificHeat, Option<double> uValue, double vapourResistanceFactor, PropertyEvidence evidence = default, Option<SampledCurve> conductivityCurve = default) =>
  (Coerce(conductivity, UnitsNet.Units.ThermalConductivityUnit.WattPerMeterKelvin),
   Coerce(specificHeat, UnitsNet.Units.SpecificEntropyUnit.JoulePerKilogramKelvin),
   uValue.Traverse(u => MeasureValue.Of(u, UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin).ToValidation()).As())
  .Apply(static (lambda, cp, u) => (Conductivity: lambda, SpecificHeat: cp, UValue: u))
  .As().ToFin()
  .Bind(column => OfThermal(column.Conductivity, column.SpecificHeat, column.UValue, vapourResistanceFactor, evidence, conductivityCurve));

 public static Fin<MaterialPropertySet> OfThermal(MeasureValue conductivity, MeasureValue specificHeat, Option<MeasureValue> uValue, double vapourResistanceFactor, PropertyEvidence evidence = default, Option<SampledCurve> conductivityCurve = default) =>
  (Positive(conductivity, "thermal-conductivity"),
   Positive(specificHeat, "thermal-specific-heat"),
   uValue.Traverse(u => Positive(u, "thermal-u-value")).As(),
   Guarded(vapourResistanceFactor is >= 1.0, vapourResistanceFactor, "vapour-resistance-factor-below-unity"))
  .Apply((c, s, u, mu) => (MaterialPropertySet)new Thermal(c, s, u, mu, evidence, conductivityCurve))
  .As().ToFin();

 public static MaterialPropertySet OfAcoustic(global::Rasm.Element.Composition.Acoustic spectrum, PropertyEvidence evidence = default) =>
  new Acoustic(spectrum, evidence);

 public static MaterialPropertySet OfFire(Option<FireRating> reaction, FireResistance resistance, PropertyEvidence evidence = default) =>
  new Fire(reaction, EuroclassSuffix.NotSpecified, resistance, evidence);

 public static MaterialPropertySet OfFire(FireRating reaction, EuroclassSuffix suffix, FireResistance resistance, PropertyEvidence evidence = default) =>
  new Fire(Option<FireRating>.Some(reaction), suffix, resistance, evidence);

 public static Fin<MaterialPropertySet> OfEnvironmental(MeasurementBasis basis, ImmutableArray<double> impacts, Option<double> recycledContent, Option<double> endOfLifeRecovery, PropertyEvidence evidence = default) =>
  (Matrix(impacts),
   recycledContent.Traverse(r => In(r, Band.Unit, "environmental-recycled-content")).As(),
   endOfLifeRecovery.Traverse(r => In(r, Band.Unit, "environmental-recovery")).As())
  .Apply((m, recycled, recovery) => (MaterialPropertySet)new Environmental(basis, m, recycled, recovery, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfCost(MeasurementBasis basis, Currency currency, double supply, double install, double lifecycle, PropertyEvidence evidence = default) =>
  (In(supply, Band.Nonnegative, "cost-supply"),
   In(install, Band.Nonnegative, "cost-install"),
   In(lifecycle, Band.Nonnegative, "cost-lifecycle"))
  .Apply((s, i, l) => (MaterialPropertySet)new Cost(basis, currency, s, i, l, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfDamping(double dampingRatio, Option<(double AlphaPerS, double BetaS)> rayleigh, PropertyEvidence evidence = default) =>
  (In(dampingRatio, Band.Fractional, "damping-ratio"),
   Rayleigh(rayleigh))
  .Apply((zeta, pair) => (MaterialPropertySet)new Damping(zeta, pair, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfHygrothermal(double porosity, double waterContent80Rh, double freeWaterSaturation, Option<double> waterAbsorption, PropertyEvidence evidence = default,
  Option<SampledCurve> sorptionIsotherm = default, Option<SampledCurve> liquidTransport = default, Option<SampledCurve> moistureConductivity = default) =>
  (In(porosity, Band.Unit, "hygrothermal-porosity"),
   PositiveSi(waterContent80Rh, QuantityType.Create("MoistureStorage"), Dimension.DensityDim, "hygrothermal-w80"),
   PositiveSi(freeWaterSaturation, QuantityType.Create("MoistureStorage"), Dimension.DensityDim, "hygrothermal-free-saturation"),
   Optional(waterAbsorption, Band.Positive, "hygrothermal-water-absorption"))
  .Apply((phi, w80, wf, a) => (Phi: phi, W80: w80, Wf: wf, A: a))
  .As().ToFin()
  .Bind(t => t.Wf.Si < t.W80.Si
   ? Fin.Fail<MaterialPropertySet>(new ElementFault.ValueRejected($"<hygrothermal-isotherm-inverted:w80={t.W80.Si:R}:wf={t.Wf.Si:R}>"))
   : Anchors(sorptionIsotherm, t.W80.Si, t.Wf.Si)
      .Map(_ => (MaterialPropertySet)new Hygrothermal(t.Phi, t.W80, t.Wf, t.A, sorptionIsotherm, liquidTransport, moistureConductivity, evidence)));

 private const double IsothermAnchorTolerance = 0.02;
 private static Fin<Unit> Anchors(Option<SampledCurve> curve, double w80, double wf) =>
  curve.TraverseM(sample => (sample.AtAdmitted(0.8), sample.AtAdmitted(1.0))
    .Apply((at80, at100) => (At80: at80, At100: at100)).As()
    .Bind(at => Disagrees(at.At80, w80) || Disagrees(at.At100, wf)
     ? Fin.Fail<Unit>(new ElementFault.ValueRejected("<hygrothermal-isotherm-anchor-mismatch>"))
     : Fin.Succ(unit))).As().Map(static _ => unit);
 private static bool Disagrees(double curve, double anchor) => Math.Abs(curve - anchor) > IsothermAnchorTolerance * Math.Max(Math.Abs(anchor), 1.0);

 public static Fin<MaterialPropertySet> OfDurability(double carbonationRate, double chlorideDiffusion, double ageingExponent, PropertyEvidence evidence = default) =>
  (In(carbonationRate, Band.Nonnegative, "durability-carbonation-rate"),
   PositiveSi(chlorideDiffusion, QuantityType.Create("ChlorideDiffusivity"), Dimension.Create(2, 0, -1, 0, 0, 0, 0), "durability-chloride-diffusion"),
   In(ageingExponent, Band.Unit, "durability-ageing-exponent"))
  .Apply((k, d, alpha) => (MaterialPropertySet)new Durability(k, d, alpha, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfOptical(double visibleTransmittance, double visibleReflectanceFront, double visibleReflectanceBack, double solarTransmittance, double solarReflectanceFront, double solarReflectanceBack, double thermalIrTransmittance, double thermalIrEmissivityFront, double thermalIrEmissivityBack, PropertyEvidence evidence = default) =>
  (In(visibleTransmittance, Band.Unit, "optical-visible-transmittance"),
   In(visibleReflectanceFront, Band.Unit, "optical-visible-reflectance-front"),
   In(visibleReflectanceBack, Band.Unit, "optical-visible-reflectance-back"),
   In(solarTransmittance, Band.Unit, "optical-solar-transmittance"),
   In(solarReflectanceFront, Band.Unit, "optical-solar-reflectance-front"),
   In(solarReflectanceBack, Band.Unit, "optical-solar-reflectance-back"),
   In(thermalIrTransmittance, Band.Unit, "optical-ir-transmittance"),
   In(thermalIrEmissivityFront, Band.Unit, "optical-ir-emissivity-front"),
   In(thermalIrEmissivityBack, Band.Unit, "optical-ir-emissivity-back"))
  .Apply((tv, rvf, rvb, te, rsf, rsb, tir, ef, eb) => new Optical(tv, rvf, rvb, te, rsf, rsb, tir, ef, eb, evidence))
  .As().ToFin()
  .Bind(o =>
   (Conserves(o.VisibleTransmittance, o.VisibleReflectanceFront, "visible", "front"),
    Conserves(o.VisibleTransmittance, o.VisibleReflectanceBack, "visible", "back"),
    Conserves(o.SolarTransmittance, o.SolarReflectanceFront, "solar", "front"),
    Conserves(o.SolarTransmittance, o.SolarReflectanceBack, "solar", "back"),
    Conserves(o.ThermalIrTransmittance, o.ThermalIrEmissivityFront, "ir", "front"),
    Conserves(o.ThermalIrTransmittance, o.ThermalIrEmissivityBack, "ir", "back"))
   .Apply((_, _, _, _, _, _) => (MaterialPropertySet)o)
   .As().ToFin());

 public static Fin<MaterialPropertySet> OfElectrical(double resistivityOhmM, double relativePermittivity, Option<double> dielectricStrengthVPerM, Option<double> magneticPermeabilityRelative, PropertyEvidence evidence = default) =>
  (Positive(MeasureValue.Of(resistivityOhmM, UnitsNet.Units.ElectricResistivityUnit.OhmMeter), "electrical-resistivity"),
   Guarded(relativePermittivity is >= 1.0, relativePermittivity, "electrical-relative-permittivity-below-unity"),
   dielectricStrengthVPerM.Traverse(v => PositiveSi(v, QuantityType.Create("DielectricStrength"), Dimension.Create(1, 1, -3, -1, 0, 0, 0), "electrical-dielectric-strength")).As(),
   Optional(magneticPermeabilityRelative, Band.Positive, "electrical-permeability"))
  .Apply((rho, er, ds, mu) => (MaterialPropertySet)new Electrical(rho, er, ds, mu, evidence))
  .As().ToFin();

 // --- [ADMISSION_SLOTS]
 private static Validation<Error, Unit> Conserves(double transmittance, double counterpart, string band, string side) =>
  transmittance + counterpart <= 1.0
   ? unit
   : new ElementFault.ValueRejected(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"<optical-{band}-{side}-conservation:{transmittance + counterpart:R}>"));

 private static Validation<Error, MeasureValue> Coerce(double value, Enum unit) =>
  MeasureValue.Of(value, unit).ToValidation();

 private static Validation<Error, double> Guarded(bool valid, double value, string name) =>
  valid ? value : new KernelFault.OutOfRange(name, value, "satisfy the declared scalar predicate");

 private static Validation<Error, MeasureValue> Positive(Fin<MeasureValue> column, string name) =>
  column.Bind(m => m.Si > 0.0 ? Fin.Succ(m) : new KernelFault.OutOfRange(name, m.Si, "be positive")).ToValidation();

 private static Validation<Error, MeasureValue> Positive(MeasureValue column, string name) =>
  Positive(Fin.Succ(column), name);

 private static Validation<Error, MeasureValue> PositiveSi(double value, QuantityType type, Dimension dimension, string name) =>
  double.IsFinite(value) && value > 0.0
   ? MeasureValue.OfSi(type, dimension, value).ToValidation()
   : new KernelFault.OutOfRange(name, value, "be finite and positive");

 private static Validation<Error, Option<(double AlphaPerS, double BetaS)>> Rayleigh(Option<(double AlphaPerS, double BetaS)> pair) =>
  pair.Traverse(r => (In(r.AlphaPerS, Band.Nonnegative, "damping-rayleigh-alpha"),
    In(r.BetaS, Band.Nonnegative, "damping-rayleigh-beta")).Apply(static (alpha, beta) => (alpha, beta)).As()).As();

 private static Validation<Error, ImmutableArray<double>> Matrix(ImmutableArray<double> impacts) =>
  impacts.IsDefaultOrEmpty || impacts.Length != Environmental.MatrixArity
   ? new ElementFault.ValueRejected($"<environmental-impact-arity:{(impacts.IsDefault ? -1 : impacts.Length)}:expected={Environmental.MatrixArity}>")
   : Indexed(impacts.AsSpan(), double.IsFinite, "environmental-impact").Map(_ => impacts);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialPropertyAccess {
 extension(Seq<MaterialPropertySet> properties) {
  private Option<T> Property<T>() where T : MaterialPropertySet =>
   properties.Choose(static p => p is T t ? Some(t) : None).Head;

  public Option<MaterialPropertySet.Mechanical> Mechanical => properties.Property<MaterialPropertySet.Mechanical>();
  public Option<MaterialPropertySet.Orthotropic> Orthotropic => properties.Property<MaterialPropertySet.Orthotropic>();
  public Option<MaterialPropertySet.Damping> Damping => properties.Property<MaterialPropertySet.Damping>();
  public Option<MaterialPropertySet.Thermal> Thermal => properties.Property<MaterialPropertySet.Thermal>();
  public Option<MaterialPropertySet.Hygrothermal> Hygrothermal => properties.Property<MaterialPropertySet.Hygrothermal>();
  public Option<MaterialPropertySet.Acoustic> Acoustic => properties.Property<MaterialPropertySet.Acoustic>();
  public Option<MaterialPropertySet.Fire> Fire => properties.Property<MaterialPropertySet.Fire>();
  public Option<MaterialPropertySet.Optical> Optical => properties.Property<MaterialPropertySet.Optical>();
  public Option<MaterialPropertySet.Environmental> Environmental => properties.Property<MaterialPropertySet.Environmental>();
  public Option<MaterialPropertySet.Cost> Cost => properties.Property<MaterialPropertySet.Cost>();

  public Option<MeasureValue> Density =>
   Mechanical.Map(static m => m.Density) | Orthotropic.Map(static o => o.Density);

 }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
