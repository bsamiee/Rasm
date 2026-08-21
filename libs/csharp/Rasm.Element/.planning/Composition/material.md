# [ELEMENT_MATERIAL]

`Material` nodes key on `MaterialId` `[ValueObject<string>]`, carry one `MaterialComposition` `[Union]` closing the type-level material-set structure (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), and carry one `MaterialPropertySet` `[Union]` closing the typed engineering-property family keyed to the one `Classification/classification#DISCIPLINE_AXIS` `Discipline`.

Each material is a FULL engineering object: one node carries U-value, sound spectrum, fire REI rating, structural grade, seismic damping, moisture-storage curve, service-life diffusion data, solar-optical constants, electrical resistivity, embodied carbon, and cost together — never a per-discipline material type.

Every multi-column admission ACCUMULATES: the `Of*` smart-constructors run each independent column as a concrete `Validation<Error,_>` slot joined by the tuple `.Apply` and collapse `.As().ToFin()` once, so a bad datasheet reports every offending column in one `Fin.Fail` (`ManyErrors`), never first-fault-wins.

`MaterialComposition` and `MaterialPropertySet` are the seam's two material owners: the `Rasm.Materials` projector lowers its material subgraph onto `Material` nodes carrying them, and the `Rasm.Bim` projector reads them. `Relations/relation#EDGE_ALGEBRA` `Associate` carries the occurrence usage binding (layer direction/offset, profile cardinal point), this owner keeping only the type-level SET structure.

`MaterialPropertySet` composes `Properties/quantity#MEASURE_VALUE` for every measured column (`MeasureValue.Of` SI coercion with the `Dimension` discriminator), `Composition/acoustic#ACOUSTIC_FOLDS` for the `Acoustic` case, and `Classification/classification#DISCIPLINE_AXIS` for the property-to-discipline key; universal non-finite or out-of-range admission stays on `KernelFault.OutOfRange`; Element-only composition and conservation invariants use `ElementFault.ValueRejected`.

`ProfileSet` carries its `Seq<MaterialProfile>` rows — each a neutral `ProfileRef` with its IFC junction priority, function category, and reference-axis offsets, beside the neutral `SectionProperties` the `Rasm.Materials` projector resolves ONE-HOP (M7) and BAKES on (`WithSection`) — so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf`) without re-resolving or admitting VividOrange, and a built-up compound keeps every row rather than its primary alone.

`SectionProperties` carries the FULL structural-design and fire column set the `Rasm.Compute` design-code checks read off the seam (the AISC 360 / EN 1993 / AISI S100 / ACI 318 / NDS / TMS 402 flexure-shear-compression and the EN 1993-1-2 / EN 1992-1-2 fire routes) — a CONSUMER-CONTRACT-driven shape, never a per-element-type section receipt.

`Rasm.Materials` resolves its elastic columns from the VividOrange polygon solver and computes the plastic moduli, torsion constant, shear areas, shear-centre offsets, and mono-symmetry factor the solver does not expose — the asymmetric-section columns the EN 1993-1-1 §6.3.2 general LTB route needs for a channel, tee, or angle, zero on a doubly-symmetric section.

## [01]-[INDEX]

- [02]-[MATERIAL_COMPOSITION]: `MaterialId`, `ProfileRef` with its content key, `SectionProperties` the S-E1 cross-section algebra (`OfMillimetres` mm-basis admission, `Lower()` solver frame, `LtbRoute` route rows, `Centroid`/`SectionForm`), the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` rows, `PropertyEvidence` the S-E3 evidence carrier (`EvidenceGrade` rank, `Attestation`, the `EvidenceRun` audit link), `SampledCurve` the kernel-fitted state-dependent function, and the four-case `MaterialComposition` union with its accumulating set admissions.
- [03]-[MATERIAL_PROPERTY]: `MaterialPropertySet` the class-root `[Union]` + `[Equatable]` keyed to `Discipline`, the `FireRating`/`EuroclassSuffix` reaction vocabulary, `FireCoverage` + `FireResistance` the EN 13501-2 criteria, `ImpactCategory`/`LifecycleStage` the EN 15804+A2 matrix axes, the accumulating `Of` admissions over the kernel-`Band` slots, and the named per-discipline reads over one private polymorphic lookup.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialId` the `[ValueObject<string>]` material-identity key a `Material` node carries; `MaterialComposition` the `[Union]` type-level material-set structure; `MaterialLayer` the layer row (`MaterialId` + `Dimension`-length `Thickness` + name + `Priority`/`Category`/`Ventilated`); `MaterialConstituent` the constituent row (`MaterialId` + category + fraction + `PartName`); `MaterialProfile` the compound-profile row (`MaterialId` + its own `ProfileRef` + `Priority`/`Category` + the reference-axis `Offsets` vector); `ProfileRef` the neutral section-profile reference (`Standard` + `Designation` + content key) a `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalogue; `SectionProperties` the neutral baked section receipt over `Properties/quantity#MEASURE_VALUE` columns.
- Cases: `Single` (one homogeneous `MaterialId` — `IfcMaterial`) · `LayerSet` (a `Seq<MaterialLayer>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`) · `ProfileSet` (a `Seq<MaterialProfile>` of per-row material-plus-profile rows beside the set-level `Composite` outline and one baked section, members and built-up compounds — `IfcMaterialProfileSet`) · `ConstituentSet` (a `Seq<MaterialConstituent>` of fraction-weighted keyword-tagged components, composites — `IfcMaterialConstituentSet`); the closed IFC material-definition family (`IfcMaterialList` deprecated and never admitted), a composition selecting how the material resolves.
- Entry: `MaterialComposition.OfSingle(material)` is the TOTAL constructor (no admission invariant — the `MaterialPropertySet.OfAcoustic`/`OfFire` total shape, never a `Fin` wrapper over a total op, never an `Op` key the body discards); `OfProfileSet` owns both profile modalities on the INPUT SHAPE — `(material, profile)` the total single-row mint every authored member takes, `(profiles, key, composite)` the `Fin<T>` compound admission an IFC ingest folds; `OfLayerSet(layers, key)` and `OfConstituentSet(constituents, key)` are `Fin<T>` admissions using kernel scalar refusal for non-positive thickness and bounded priorities/fractions, while empty sets, offset arity, and fraction normalization remain `ElementFault.ValueRejected` semantics; every factory is `Of`-prefixed so the name never collides with the same-named nested case type (the `MaterialPropertySet.Of*` convention — a bare `Single(...)` static method and a nested `Single` case are one declaration space, a compile collision). `Materials` projects the assigned `MaterialId` set, `PrimaryMaterial` the appearance/structural-default key, `TotalThickness` (a `LayerSet` read) the layer buildup depth, and `ProfileSet.Primary`/`Material`/`Profile` the IFC-ordered head-row reads.
- Auto: the three invariant-bearing SET cases carry a PRIVATE constructor and an internal `Seed` re-hydration escape (the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage.ProfileSet` and `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` admission shape), so the only public admission is the `Of` factory and an empty/degenerate set is UNREPRESENTABLE — `PrimaryMaterial`'s `OrderByDescending(...).First()` and `ProfileSet.Primary`'s index-zero read are then total, never a latent throw on the empty set a public positional ctor admits; `Materials` dispatches the generated `Switch` projecting the `MaterialId` set each case carries, a compound `ProfileSet` reporting EVERY row's material; `OfLayerSet` guards each `MaterialLayer.Thickness` positive (the SI metre magnitude of the `Properties/quantity#MEASURE_VALUE` length) and each row priority in `[0,100]`; `OfProfileSet` guards the row set non-empty, each row priority in range, and each offset vector within the IFC `LIST[1:2]` arity; `OfConstituentSet` guards each fraction finite and the fraction sum to one within tolerance so a composite mixture normalizes once at construction (the `Rasm.Compute` `AssemblyAggregator` rule-of-mixtures reads the normalized fractions and never re-guards them) — every guard an independent accumulating slot, so one malformed datasheet reports every offending row and column at once.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<string>]`), Generator.Equals (`[Equatable]`/`[OrderedEquality]`/`[StringEquality]`), LanguageExt.Core (`Seq`/`Fin`/`Option`), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter`, `ContentAddress.Of`), `Rasm` (`Op` and `Rasm/Domain/validation#ADMISSION_SLOTS`).
- Growth: a temperature-dependent property is one `Option<SampledCurve>` column beside its steady-state scalar; the IFC material-definition family is closed at four cases; a new layer/constituent/profile attribute is one row column; a new structural or fire section column is one `MeasureValue` field the Materials resolver fills and a Compute check reads — appended AFTER `Form` in the canonical order, never re-ordered; a new section catalogue is one `ProfileRef.Standard` token; a new evidence axis is one `PropertyEvidence` column + its `CaseBytes` write in the same edit; imported material `Pset` rows are NOT columns here — each lands as a neutral `PROPERTY_BAG` node under `EvidenceGrade.Import` bound by one `Assign.PropertyDefinition` edge, the typed family staying FULL-VECTOR and authored-only.
- Boundary: `MaterialComposition` is the ONE composition owner — a per-element-type composition class is the deleted form; the composition is the TYPE-LEVEL set structure only, the occurrence usage binding (`LayerSetUsage` direction/sense/offset, `ProfileSetUsage` cardinal-point/extent) riding the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, so a layer set's geometric usage never duplicates onto the composition; a `ProfileSet` stores its `Seq<MaterialProfile>` rows beside the set-level `Composite` and DERIVES `Material`/`Profile` — a primary scalar stored beside row zero is the named double-store defect, and the single-row member is the one-row case of the same store rather than a second shape, so a built-up compound (plate girder, steel-concrete composite) keeps every row's material, priority, category, own profile geometry, and reference-axis offsets where a primary-only store drops all but the first; `Profile` is the section identity a consumer resolves one-hop — the declared `Composite` when a compound set carries one, else the primary row's own profile — so the two-level store keeps row zero's plate geometry a composite-overwrites-primary read destroys; each row carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section-property type — the seam references no VividOrange, the `Rasm.Materials` projector resolving the `ProfileRef` one-hop and BAKING the neutral `SectionProperties` (`WithSection`) so a structural consumer reads the resolved section once; the `SectionProperties` is the consumer-contract column set the `Rasm.Compute` design-code routes read (`Area`/`Iyy`/`Izz`/`J`/`Iw`/`Wely`/`Welz`/`Wply`/`Wplz`/`AvY`/`AvZ`/radii/`Depth`/`Width`/`HeatedPerimeter`/`AxisDistance`/`ShearCentreY`/`ShearCentreZ`/`MonosymmetryFactor`) — the seam carries the baked scalars, never a VividOrange type, and the projector computes the plastic moduli/torsion/warping/shear-area/asymmetry columns the VividOrange polygon solver does not expose (the `Iw` warping constant the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling routes require, never derivable from `J` alone, AND the `ShearCentreY`/`ShearCentreZ` shear-centre offsets + the `MonosymmetryFactor` β_y the EN 1993-1-1 §6.3.2 GENERAL LTB route requires for a channel/tee/angle — all zero for a doubly-symmetric section, so a PFC/tee is no longer the unbuckle-checkable thin slice the symmetric-only column set left); a `MaterialLayer.Thickness` is a `Properties/quantity#MEASURE_VALUE` `Dimension`-length-checked measure read SI-native through `.Si`, never a bare double, and a `MaterialProfile.Offsets` entry is the same `Dimension`-length measure so a reference-axis offset never crosses as a native-unit scalar; a row `Priority` is `Option<int>` over the IFC `[0,100]` junction percentage because GeometryGym spells an unset priority as `int.MinValue` — projecting that sentinel at the `Rasm.Bim` read is the `[SENTINEL_PROJECTION]` obligation, and an `int` column carrying it into the seam or the content hash is the deleted form; `MaterialLayer.Ventilated` is `Option<bool>` over the three-state `IfcLogical` the `Properties/property#PROPERTY_VALUE` `Logical` case already ratifies (`None` = `UNKNOWN`), so a second three-state vocabulary minted for one `IfcLogical` domain is the deleted parallel shape and an `UNKNOWN` coerced to `false` is the named EN ISO 6946 falsification; a per-LAYER offset column is unrepresentable BY LAW because `IfcMaterialLayerWithOffsets` publishes no accessor and no public constructor — the asymmetry with the profile subtype's public `OffsetValues` is a GeometryGym surface fact, never a symmetry the seam fabricates; `MaterialComposition` is a CLASS-root `[Union]` + `[Equatable]` and the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile`/`SectionProperties` rows are `[Equatable]` record structs so the `Rasm.Persistence` `StructuralMerge` drills a changed layer thickness / constituent fraction / row priority / section column to `Composition.Layers[i].Thickness` / `.Constituents[i].Fraction` / `.Profiles[i].Priority` / `.Section.<column>` rather than replacing the whole composition (the record-root opaque-leaf form is deleted); the composition serializes to the IFC 4.3 material-definition family at the `Rasm.Bim` boundary, host-neutral here; the `CanonicalBytes` arms fold EVERY case field with each collection count-prefixed and each optional column presence-prefixed — the `Bool`-prefixed baked `Section` delegating to `SectionProperties.CanonicalBytes` — so the M7 bake (which runs at projection, before the `Material` node's content-keyed mint), a re-resolved section column, a changed row priority, and an `UNKNOWN`-versus-`FALSE` ventilation each fork the node identity, a section-omitting or column-omitting arm being the deleted collision that addressed two distinct receipts as one material.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
using InterpolantSmooth = Rasm.Numerics.Interpolant<Rasm.Numerics.Smooth>;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Composition;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class MaterialId {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();
 public static MaterialId Of(string value) => Create(value);   // the codebase .Of factory the cross-package catalogues key on
}

// A neutral section profile: catalogue designation (+ optional standard) the Materials projector resolves
// ONE-HOP (M7) to the baked SectionProperties. The content key derives from the normalized pair through the
// one canonical codec (private ctor closes it; Rehydrate rejects a disagreeing persisted key); Standard/
// Designation take ORDINAL equality because the writer hashes them verbatim — equality and identity never fork.
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
    static (pair, w) => w.String(pair.normalizedStandard).String(pair.normalizedDesignation)).Value);
 }

 public static Fin<ProfileRef> Rehydrate(string standard, string designation, UInt128 contentKey, Op key) {
  ProfileRef admitted = Of(standard, designation);
  return admitted.ContentKey == contentKey
   ? Fin.Succ(admitted)
   : new ElementFault.ValueRejected(key, $"<profile-content-key-mismatch:{contentKey}>");
 }
}

// SectionProperties IS the neutral cross-section algebra (S-E1) — the FULL structural-design + fire column set the
// Rasm.Materials resolver BAKES onto a ProfileSet (M7) and the Rasm.Compute design-code checks read off the seam:
// area, both-axis I, torsion J, warping Iw (the §6.3.2 LTB input), elastic+plastic moduli, shear areas, radii,
// bounding Depth/Width, HeatedPerimeter (Am/V), EN 1992-1-2 AxisDistance, the asymmetric-LTB shear-centre offsets
// and mono-symmetry factor, the section CENTROID (host-neutral Vector3, SI), and the optional SectionForm shape
// witness the Fabrication tube-forming lane reads. Every measured column is a TYPED OfSi mint (SectionModulus /
// TorsionConstant / WarpingConstant are the three consumer-domain Create names the registry lacks; the second
// moment IS the registry AreaMomentOfInertia). [Equatable] on the record struct so the Persistence StructuralMerge
// drills Composition.Section.<column>; the measure is the merge leaf.
[Equatable]
public readonly partial record struct SectionProperties(
 MeasureValue Area, MeasureValue Iyy, MeasureValue Izz, MeasureValue J, MeasureValue Iw,
 MeasureValue Wely, MeasureValue Welz, MeasureValue Wply, MeasureValue Wplz,
 MeasureValue AvY, MeasureValue AvZ, MeasureValue RadiusOfGyrationMajor, MeasureValue RadiusOfGyrationMinor,
 MeasureValue Depth, MeasureValue Width, MeasureValue HeatedPerimeter, MeasureValue AxisDistance,
 MeasureValue ShearCentreY, MeasureValue ShearCentreZ, double MonosymmetryFactor,
 Vector3 Centroid = default, Option<SectionForm> Form = default) {
 // EN 1992-1-2 concrete-fire minimum cross-section dimension — derived, never a stored column that could drift
 // (the Compute fire check reads it against the tabulated (min dimension, min axis distance) pair).
 public MeasureValue LeastDimension => Depth.Si <= Width.Si ? Depth : Width;

 // The §6.3.2 route discriminant, derived from the stamped asymmetry columns — a doubly-symmetric section
 // (shear centre AT the centroid, vanishing β) takes the simplified form; Materials' steel LTB arm reads the row.
 public LtbRoute Ltb =>
  ShearCentreY.Si == 0.0 && ShearCentreZ.Si == 0.0 && MonosymmetryFactor == 0.0 ? LtbRoute.Simplified : LtbRoute.General;

 // Centroid and Form APPEND after MonosymmetryFactor — any other position re-keys every stored composition.
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

 // The mm-basis admission every peer carrier lowers through (S-E1): PER-COLUMN positivity split — the areas,
 // moduli, second moments, torsion, radii, extents, and heated perimeter refuse non-positive; Iw, AxisDistance, the
 // shear-centre offsets, and β are signed-and-zero-valid. Centroid arrives in mm and stores SI.
 public static Fin<SectionProperties> OfMillimetres(
  double areaMm2, double iyyMm4, double izzMm4, double jMm4, double iwMm6,
  double welyMm3, double welzMm3, double wplyMm3, double wplzMm3,
  double avyMm2, double avzMm2, double radiusMajorMm, double radiusMinorMm,
  double depthMm, double widthMm, double heatedPerimeterMm, double axisDistanceMm,
  double shearCentreYMm, double shearCentreZMm, double monosymmetryFactor,
  Vector3 centroidMm, Option<SectionForm> form, Op key) =>
  (Column(areaMm2, Millimetre.Area, QuantityType.Area, Dimension.AreaDim, "section-area", key, strict: true),
   Column(iyyMm4, Millimetre.Quartic, QuantityType.AreaMomentOfInertia, InertiaDim, "section-iyy", key, strict: true),
   Column(izzMm4, Millimetre.Quartic, QuantityType.AreaMomentOfInertia, InertiaDim, "section-izz", key, strict: true),
   Column(jMm4, Millimetre.Quartic, Torsion, InertiaDim, "section-j", key, strict: true),
   Column(iwMm6, Millimetre.Sextic, Warping, WarpingDim, "section-iw", key, strict: false),
   Column(welyMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-wely", key, strict: true),
   Column(welzMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-welz", key, strict: true),
   Column(wplyMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-wply", key, strict: true),
   Column(wplzMm3, Millimetre.Cubic, Modulus, Dimension.VolumeDim, "section-wplz", key, strict: true),
   Column(avyMm2, Millimetre.Area, QuantityType.Area, Dimension.AreaDim, "section-avy", key, strict: true))
  .Apply(static (area, iyy, izz, j, iw, wely, welz, wply, wplz, avy) => (area, iyy, izz, j, iw, wely, welz, wply, wplz, avy))
  .As()
  .Bind(head =>
   (Column(avzMm2, Millimetre.Area, QuantityType.Area, Dimension.AreaDim, "section-avz", key, strict: true),
    Column(radiusMajorMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-radius-major", key, strict: true),
    Column(radiusMinorMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-radius-minor", key, strict: true),
    Column(depthMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-depth", key, strict: true),
    Column(widthMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-width", key, strict: true),
    Column(heatedPerimeterMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-heated-perimeter", key, strict: true),
    Column(axisDistanceMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-axis-distance", key, strict: false),
    Column(shearCentreYMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-shear-centre-y", key, strict: false),
    Column(shearCentreZMm, Millimetre.Length, QuantityType.Length, Dimension.LengthDim, "section-shear-centre-z", key, strict: false),
    Finite(key, ("section-monosymmetry", monosymmetryFactor)))
   .Apply((avz, rmaj, rmin, depth, width, heated, axis, scy, scz, _) =>
    new SectionProperties(head.area, head.iyy, head.izz, head.j, head.iw,
     head.wely, head.welz, head.wply, head.wplz, head.avy,
     avz, rmaj, rmin, depth, width, heated, axis, scy, scz, monosymmetryFactor,
     new Vector3(centroidMm.X * Millimetre.Length, centroidMm.Y * Millimetre.Length, centroidMm.Z * Millimetre.Length), form))
   .As().ToFin());

 // The bare-double frame the Compute solver lowering reads — one projection, never per-site .Si spelling.
 public FrameConstants Lower() =>
  new(Area.Si, AvY.Si, AvZ.Si, Iyy.Si, Izz.Si, J.Si, Iw.Si);

 // Consumer-domain quantity names the registry lacks + the two unrostered signatures (SectionProperties discipline).
 static readonly QuantityType Modulus = QuantityType.Create("SectionModulus");
 static readonly QuantityType Torsion = QuantityType.Create("TorsionConstant");
 static readonly QuantityType Warping = QuantityType.Create("WarpingConstant");
 static readonly Dimension InertiaDim = Dimension.Create(4, 0, 0, 0, 0, 0, 0);
 static readonly Dimension WarpingDim = Dimension.Create(6, 0, 0, 0, 0, 0, 0);

 // ONE mm^n -> SI column admission: strict refuses non-positive, lax gates finite alone (signed/zero-valid columns).
 static Validation<Error, MeasureValue> Column(double valueMm, double factor, QuantityType type, Dimension dimension, string name, Op key, bool strict) =>
  (strict ? In(valueMm, Band.Positive, name, key) : Finite(key, (name, valueMm)).Map(_ => valueMm))
   .Bind(value => MeasureValue.OfSi(type, dimension, value * factor, key: key).ToValidation());

 // mm-basis powers, named once.
 static class Millimetre {
  internal const double Length = 1e-3;
  internal const double Area = 1e-6;
  internal const double Cubic = 1e-9;
  internal const double Quartic = 1e-12;
  internal const double Sextic = 1e-18;
 }
}

// SectionForm is the shape witness the Fabrication tube-forming lane reads off the seam: vertex/curved-edge
// census, the radial compactness ratio, the outline perimeter, and the two BOUNDING EXTENTS. Major/Minor are the
// larger and smaller extent of the section's own bounding box — the discriminant a tube family admits on (a
// circular tube holds Major/Minor within one percent, an elliptic one does not) and the dimension a bend-radius
// factor, an ovality, and a mandrel support law normalize against. They are NOT the radii of gyration: those are
// `RadiusOfGyrationMajor`/`RadiusOfGyrationMinor` on the owner above, derived from a second moment and an area,
// and a reader that swapped the pair would size a die off a stiffness radius. Columns APPEND at the tail — any
// other position re-keys every stored composition.
public readonly record struct SectionForm(
 int VertexCount, int CurvedEdges, double RadialRatio, MeasureValue Perimeter, MeasureValue Major, MeasureValue Minor);

// The bare-SI frame the Compute discretization/structural lowerings read (five sites) — one owner for the
// section-to-solver projection.
public readonly record struct FrameConstants(
 double Area, double ShearAreaY, double ShearAreaZ, double Iy, double Iz, double Torsion, double Warping);

// EN 1993-1-1 §6.3.2 route rows — the discriminant a bool erased (WHICH route, not merely whether symmetric).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LtbRoute {
 public static readonly LtbRoute Simplified = new("simplified");
 public static readonly LtbRoute General = new("general");
}

// [Equatable] so the StructuralMerge drills to Layers[i].Thickness rather than whole-row replacement; each row
// mirrors ONE IFC material-row entity 1:1. Priority is the [0,100] junction percentage (GeometryGym's
// int.MinValue unset sentinel retires to None at the Bim fold); Ventilated is the three-state IfcLogical
// (None = UNKNOWN, a REFUSAL input to the EN ISO 6946 fold, never a silent unventilated layer).
[Equatable]
public readonly partial record struct MaterialLayer(
 MaterialId Material, MeasureValue Thickness, string LayerName,
 Option<int> Priority = default, string Category = "", Option<bool> Ventilated = default);

// PartName is the IfcMaterialConstituent.Name — the part the constituent FORMS ("framing"/"infill"), a different axis
// from Category (its function keyword), so a two-part composite whose rows share one category stays addressable.
[Equatable]
public readonly partial record struct MaterialConstituent(
 MaterialId Material, string Category, double Fraction, string PartName = "");

// One IfcMaterialProfile row of a compound set — own material, content-keyed ProfileRef, priority, category,
// and the IfcMaterialProfileWithOffsets vector (arity 1..2; EMPTY IS the base profile — IFC LIST[1:2] makes
// empty⟺base a bijection, never a sentinel). Layers carry no offset twin: the GeometryGym surface keeps the
// layer vector internal, so fabricating one is the deleted form. Row name IS Profile.Designation.
[Equatable]
public readonly partial record struct MaterialProfile(
 MaterialId Material, ProfileRef Profile,
 Option<int> Priority = default, string Category = "",
 [property: OrderedEquality] Seq<MeasureValue> Offsets = default);

// --- [MODELS] ----------------------------------------------------------------------------- The
// ONE evidence carrier (S-E3): Source the ingress kind, Reference the declaration identity (ABSENT where a
// curated catalogue row has none — a blank string re-authored an identity the source never declared), ValidUntil the
// exact expiry the procurement filter compares, Grade the attributable rank axis, Attested the third-party
// signature, Run the solver-run audit (Assessment/assessment#ASSESSMENT_NODE EvidenceRun). Private ctor + Of:
// the columns canonicalize once and no blank-string sentinel reaches the union base.
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

 // Citability is a QUESTION WITH A DATE, never an ambient-clock read: attributable grade, unexpired as of the
 // caller's own day.
 public bool Citable(LocalDate asOf) => Grade.Attributable && ValidUntil.ForAll(until => asOf <= until);

 // Normalized survives for the defaulted-struct path alone: a `default` evidence argument reads as Catalogue.
 public PropertyEvidence Normalized() => Source is null ? Catalogue : this;
}

// Third-party attestation of an evidence row — the WHOLE report-family role vocabulary (the five inspection /
// verification authorities included), so the Fabrication local AttestationRole DELETES onto this owner in its
// own W3 unit with zero row loss; Payload is the attested document's content key.
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

// STATE-DEPENDENT properties as ONE sampled function (sorption isotherm w(φ), Dw(w), λ(w), any datasheet
// curve) — measured samples stored content-equal, the kernel Interpolant.Linear fit built at admission,
// never a fitted parametric form that drifts from the measurement; axis/value units ride the OWNING column's
// name (the declared-unit discipline).
[Equatable]
public sealed partial record SampledCurve {
 [property: OrderedEquality] public ImmutableArray<double> Axis { get; }
 [property: OrderedEquality] public ImmutableArray<double> Values { get; }
 // The kernel fit, built ONCE at admission over the proven samples; derived state, excluded from equality and canon.
 [IgnoreEquality] private readonly InterpolantSmooth fit;

 private SampledCurve(ImmutableArray<double> axis, ImmutableArray<double> values, InterpolantSmooth fit) =>
  (Axis, Values, this.fit) = (axis, values, fit);

 // BOUNDARY_ADMISSION: at least two samples, matching arity, every ordinate finite (vectorized), the axis strictly
 // increasing — then the kernel Interpolant.Linear fit runs INSIDE the admission, so a refused fit rails here and
 // every read on an admitted curve is total.
 public static Fin<SampledCurve> Of(ReadOnlyMemory<double> axis, ReadOnlyMemory<double> values, Op key) =>
  axis.Length < 2 || axis.Length != values.Length
   ? new ElementFault.ValueRejected(key, $"<curve-arity:axis={axis.Length}:values={values.Length}>")
   : (Indexed(axis.Span, double.IsFinite, key, "curve-axis"), Indexed(values.Span, double.IsFinite, key, "curve-value"))
    .Apply(static (_, _) => unit).As().ToFin()
    .Bind(_ => NotIncreasing(axis.Span)
     ? new ElementFault.ValueRejected(key, "<curve-axis-not-increasing>")
     : Interpolant.Linear(toArr(axis.ToArray()), toArr(values.ToArray()), key)
        .Map(fitted => new SampledCurve([.. axis.Span], [.. values.Span], fitted)));

 public Fin<double> At(double x, Op key) =>
  double.IsFinite(x)
   ? AtAdmitted(x, key)
   : new KernelFault.OutOfRange("curve-query", x, "be finite", Some(key));

 // The WUFI table convention — CLAMP to the end samples — is Element's declared column over the kernel fit, which
 // would otherwise extrapolate the end segments. The kernel read retains its exact refusal on the same Fin rail.
 internal Fin<double> AtAdmitted(double x, Op key) {
  ReadOnlySpan<double> a = Axis.AsSpan();
  return fit.Value(Math.Clamp(x, a[0], a[^1]), key);
 }

 public void CanonicalBytes(CanonicalWriter w) =>
  w.Doubles(Axis.AsSpan()).Doubles(Values.AsSpan());

 private static bool NotIncreasing(ReadOnlySpan<double> axis) {
  for (int i = 1; i < axis.Length; i++) { if (axis[i] <= axis[i - 1]) { return true; } }
  return false;
 }
}

// CLASS-root [Union] ([GRAPH_FAMILY]): equality and the member diff ride Generator.Equals, seated PER NESTED
// CASE (a root seat leaves case members reference-compared — TTRESG106), so the Persistence StructuralMerge
// localizes a changed layer thickness to Composition.Layers[2].Thickness after discrimination. Generated
// Switch/Map survive; a class-root case has NO `with`, so ProfileSet.With/WithSection RECONSTRUCT.
[Union]
public abstract partial class MaterialComposition {
 private MaterialComposition() { }

 // Single alone carries no admission invariant — a public positional ctor is safe, and the Of-prefixed factory mirrors it
 // as a TOTAL constructor so the family is named uniformly without a fake Fin rail.
 [Equatable] public sealed partial class Single(MaterialId material) : MaterialComposition { public MaterialId Material { get; } = material; }

 // The three SET cases: PRIVATE ctor + internal Seed forces every admission through Of, so an empty set is
 // unrepresentable and every head/extremum read is total. Row Seqs are ORDERED ([OrderedEquality] matches the
 // physical buildup order AND the stored-order canonical iteration).
 [Equatable]
 public sealed partial class LayerSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialLayer> Layers { get; }
  private LayerSet(Seq<MaterialLayer> layers) => Layers = layers;
  internal static LayerSet Seed(Seq<MaterialLayer> layers) => new(layers);
  public double TotalThickness => Layers.Sum(static l => l.Thickness.Si);
 }

 // COMPOUND set: per-row profiles + the set-level Composite outline IfcMaterialProfileSet declares. Material/
 // Profile are DERIVED reads off row zero and the composite (a stored primary scalar is the double-store defect).
 [Equatable]
 public sealed partial class ProfileSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialProfile> Profiles { get; }
  public Option<ProfileRef> Composite { get; }
  public Option<SectionProperties> Section { get; }
  private ProfileSet(Seq<MaterialProfile> profiles, Option<ProfileRef> composite, Option<SectionProperties> section) =>
   (Profiles, Composite, Section) = (profiles, composite, section);
  internal static ProfileSet Seed(Seq<MaterialProfile> profiles, Option<ProfileRef> composite, Option<SectionProperties> section) =>
   new(profiles, composite, section);
  // Index zero is total under the non-empty admission the Of factory owns.
  public MaterialProfile Primary => Profiles[0];
  public MaterialId Material => Primary.Material;
  public ProfileRef Profile => Composite.IfNone(Primary.Profile);
  // With owns the M7 section bake: a class-root [Union] case has NO compiler-generated `with`, so With RECONSTRUCTS the
  // case (the base WithSection delegates here so the base never copies a case across the private-ctor type boundary).
  // Section is the ONLY member that changes; the resolved neutral SectionProperties stamps once at projection.
  public ProfileSet With(SectionProperties section) => new(Profiles, Composite, Some(section));
 }

 [Equatable]
 public sealed partial class ConstituentSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialConstituent> Constituents { get; }
  private ConstituentSet(Seq<MaterialConstituent> constituents) => Constituents = constituents;
  internal static ConstituentSet Seed(Seq<MaterialConstituent> constituents) => new(constituents);
 }

 // Every assigned material, per case — a compound ProfileSet reports EVERY row's material (a composite member's steel AND
 // its slab), so an assembly aggregation reaches both substances where the primary-only read stranded the second.
 // ONE dispatch answers both material reads — the roster and its dominant member per case law (thickest layer,
 // IFC-ordered primary profile row, largest fraction; total, the set cases' admission guarantees non-empty rows) —
 // and the two named reads are one-hop projections, never a parallel "primary" flag.
 public (Seq<MaterialId> All, MaterialId Primary) Census => Switch(
  single: static s => (Seq(s.Material), s.Material),
  layerSet: static s => (s.Layers.Map(static l => l.Material), s.Layers.OrderByDescending(static l => l.Thickness.Si).First().Material),
  profileSet: static s => (s.Profiles.Map(static p => p.Material), s.Material),
  constituentSet: static s => (s.Constituents.Map(static c => c.Material), s.Constituents.OrderByDescending(static c => c.Fraction).First().Material));

 public Seq<MaterialId> Materials => Census.All;
 public MaterialId PrimaryMaterial => Census.Primary;

  // WithSection lands the M7 bake: the Rasm.Materials projector resolves the ProfileRef one-hop and stamps the section onto
 // a ProfileSet composition, so a structural consumer reads it through graph.SectionOf without re-resolving.
 public MaterialComposition WithSection(SectionProperties section) =>
  this is ProfileSet ps ? ps.With(section) : this;

  // Case ordinal, then the payload through kernel Rows/Optional — count-framed and presence-prefixed, so the
  // encoding stays injective and two compositions differing only in a baked section column address apart.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  single: s => w.Ordinal(0).String(s.Material.Value),
  layerSet: s => w.Ordinal(1).Rows(s.Layers, static (l, run) => run
   .String(l.Material.Value).Measure(l.Thickness).String(l.LayerName).String(l.Category)
   .Optional(l.Priority, static (p, deep) => deep.Ordinal(p))
   .Optional(l.Ventilated, static (v, deep) => deep.Bool(v))),
  profileSet: s => w.Ordinal(2)
   .Rows(s.Profiles, static (p, run) => run
    .String(p.Material.Value).String(p.Profile.Standard).String(p.Profile.Designation).U128(p.Profile.ContentKey)
    .String(p.Category)
    .Optional(p.Priority, static (v, deep) => deep.Ordinal(v))
    .Rows(p.Offsets, static (o, deep) => deep.Measure(o)))
   .Optional(s.Composite, static (c, run) => run.String(c.Standard).String(c.Designation).U128(c.ContentKey))
   .Optional(s.Section, static (x, run) => x.CanonicalBytes(run)),
  constituentSet: s => w.Ordinal(3).Rows(s.Constituents, static (c, run) => run
   .String(c.Material.Value).String(c.Category).String(c.PartName).Double(c.Fraction)));

 private const double FractionTolerance = 1e-3;
 // IFC bounds IfcMaterialLayer/IfcMaterialProfile priority to a [0,100] percentage; GeometryGym's setter silently clamps
 // an out-of-range value to its unset sentinel, so the seam rejects it BY NAME rather than inheriting a silent coercion.
 private const int PriorityCeiling = 100;
 // IfcMaterialProfileWithOffsets.OffsetValues is a LIST[1:2] — a start offset and an optional end offset.
 private const int OffsetArityCeiling = 2;

 // Single is TOTAL — no admission invariant, so no Fin and no Op key (the OfAcoustic/OfFire shape).
 public static MaterialComposition OfSingle(MaterialId material) => new Single(material);

 // ONE OfProfileSet family discriminates on the INPUT SHAPE (MODAL_ARITY): a (material, profile) pair is the single-row
 // set every authored member takes — non-empty by construction, hence TOTAL with no Fin and no Op key — while a row Seq
 // carries the compound set an IFC ingest folds, admitting through the per-row slots. Never a batch flag beside a value:
 // row arity IS recoverable from the argument, and the rail difference is the invariant's consequence, never a knob.
 public static MaterialComposition OfProfileSet(MaterialId material, ProfileRef profile) =>
  ProfileSet.Seed(Seq(new MaterialProfile(material, profile)), Option<ProfileRef>.None, Option<SectionProperties>.None);

 public static Fin<MaterialComposition> OfProfileSet(Seq<MaterialProfile> profiles, Op key, Option<ProfileRef> composite = default) =>
  (Gate(!profiles.IsEmpty, key, "<profile-set-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Accumulate(profiles.Map((profile, index) => profile.Priority.Match(
    Some: priority => InRange(priority, 0, PriorityCeiling, $"profile-priority[{index}]", key).Map(static _ => unit),
    None: static () => Success<Error, Unit>(unit)))),
   Accumulate(profiles.Map((profile, index) => Gate(profile.Offsets.Count <= OffsetArityCeiling, key, $"<profile-offset-arity:index={index}:count={profile.Offsets.Count}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)))))
  .Apply((_, _, _) => (MaterialComposition)ProfileSet.Seed(profiles, composite, Option<SectionProperties>.None))
  .As().ToFin();

 // Each layer is an independent admission slot, so every bad row is retained with its index in one ManyErrors — the
 // thickness magnitude and the priority percentage accumulate as peers rather than short-circuiting on the first row.
 public static Fin<MaterialComposition> OfLayerSet(Seq<MaterialLayer> layers, Op key) =>
  (Gate(!layers.IsEmpty, key, "<layer-set-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Accumulate(layers.Map((layer, index) => In(layer.Thickness.Si, Band.Positive, $"layer-thickness[{index}]", key).Map(static _ => unit))),
   Accumulate(layers.Map((layer, index) => layer.Priority.Match(
    Some: priority => InRange(priority, 0, PriorityCeiling, $"layer-priority[{index}]", key).Map(static _ => unit),
    None: static () => Success<Error, Unit>(unit)))))
  .Apply((_, _, _) => (MaterialComposition)LayerSet.Seed(layers))
  .As().ToFin();

  // OfConstituentSet admits a composite MIXTURE: the fractions are proportions of the whole, so they normalize to one
 // within tolerance (the Rasm.Compute rule-of-mixtures fold reads them normalized and never re-guards) — a set that
 // does not sum to unity is a malformed mixture the admission rejects, the seam carrying only valid composites.
 public static Fin<MaterialComposition> OfConstituentSet(Seq<MaterialConstituent> constituents, Op key) =>
  (Gate(!constituents.IsEmpty, key, "<constituent-set-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Accumulate(constituents.Map((constituent, index) => In(constituent.Fraction, Band.Unit, $"constituent-fraction[{index}]", key).Map(static _ => unit))))
  .Apply(static (_, _) => unit)
  .As().ToFin()
  .Bind(_ => Math.Abs(constituents.Sum(static constituent => constituent.Fraction) - 1.0) <= FractionTolerance
   ? Fin.Succ<MaterialComposition>(ConstituentSet.Seed(constituents))
   : new ElementFault.ValueRejected(key, $"<constituent-fraction-not-normalized:{constituents.Sum(static constituent => constituent.Fraction):R}>"));
}
```

## [03]-[MATERIAL_PROPERTY]

- Owner: `MaterialPropertySet` the `[Union]` typed engineering-property family keyed to `Discipline`; `FireRating` the `[SmartEnum<string>]` reaction-to-fire class with `EuroclassSuffix` the EN 13501-1 sub-classification pair (one product, one `Parse`); `FireCoverage` the criterion presence rows and `FireResistance` the EN 13501-2 R/E/I criteria; the `Of` admissions coercing each measured column through `Properties/quantity#MEASURE_VALUE` and gating scalars on the kernel `Band` rows through `[ADMISSION_SLOTS]` `In`/`InRange`.
- Cases: `Mechanical` (density / Young's modulus / yield strength / ultimate strength as `MeasureValue`, Poisson's ratio + thermal-expansion as guarded dimensionless doubles, the isotropic shear modulus DERIVED `G = E/(2(1+ν))`, and the optional `YoungsReduction`/`YieldReduction` temperature-reduction `SampledCurve` pair — factor vs °C, the fire-route stiffness/strength decay evidence beside the 20 °C scalars — `Discipline.Structural`) · `Orthotropic` (density / the two principal moduli `E1∥`/`E2⊥` / the INDEPENDENT measured shear modulus `G` / the two principal strengths as `MeasureValue` + thermal-expansion, with the matching optional `ModulusReduction`/`StrengthReduction` pair — the directional-stiffness carrier the isotropic `Mechanical` structurally cannot model, the `Rasm.Materials` `timber#TIMBER_FAMILY` consumer's seam home, also `Discipline.Structural` so the case TYPE discriminates an isotropic from a directional material) · `Thermal` (conductivity / specific heat / U-value as `MeasureValue` + vapour-resistance factor μ as a guarded dimensionless double for EN 13788 Glaser condensation — `Discipline.Thermal`) · `Acoustic` (the `Composition/acoustic#ACOUSTIC_FOLDS` banded carrier with its `DynamicStiffnessMNPerM3`/`FlowResistivityPaSPerM2`/`LossFactor` intrinsic constants forwarded — `Discipline.Acoustic`) · `Fire` (an optional `FireRating` reaction class + its `EuroclassSuffix` pair + a `FireResistance` R/E/I rating — `Discipline.Fire`) · `Environmental` (a `MeasurementBasis` declared unit + the EN 15804+A2 `(ImpactCategory × LifecycleStage)` row-major flat `Impacts` matrix that is the ONE impact store + `RecycledContent`/`EndOfLifeRecovery` fractions + EPD provenance, with the general `IndicatorAt(category, stage)` cell read and the `WholeLife(category)` cross-stage fold, and the carbon-keyed convenience projections `Gwp` (the DERIVED cradle-to-gate `(GwpTotal, A1A3)` cell, never a parallel stored scalar), `StageAt`, `WholeLifeGwp`, and the DERIVED `StageGwp` per-module GwpTotal-row vector (the `[A1A3..D]` carbon row sliced from the matrix via `IndicatorAt(GwpTotal, stage)`, never a parallel stored 6-vector, the `Rasm.Compute` carbon fold reads one-hop) over them — `Discipline.Environmental`) · `Cost` (supply / install / lifecycle per-unit columns over a `Currency` + `MeasurementBasis` — `Discipline.Cost`) · `Damping` (the EN 1998-1 fraction-of-critical `DampingRatio` ζ + the optional per-material Rayleigh `(α, β)` proportional-damping pair a time-history FE model reads + the DERIVED hysteretic `StructuralLossFactor = 2ζ` — `Discipline.Dynamic`) · `Hygrothermal` (the EN 15026/WUFI transient-simulation inputs the steady-state `Thermal` case cannot model: `Porosity`, the `WaterContent80Rh`/`FreeWaterSaturation` sorption-isotherm anchors as MoistureStorage-typed kg/m³ measures, the optional capillary `WaterAbsorptionKgPerM2SqrtS` A-value, and the three optional `SampledCurve` measured functions — the full sorption isotherm `w(φ)`, the liquid-transport table `Dw(w)`, the moisture-dependent conductivity `λ(w)` — the WUFI-class solver runs on where the anchors under-model — `Discipline.Hygrothermal`) · `Durability` (the fib Model Code service-life inputs: `CarbonationRateMmPerSqrtYear` K, the `ChlorideDiffusion` D_RCM as a ChlorideDiffusivity-typed m²/s measure, the `AgeingExponent` decay fraction — `Discipline.Durability`) · `Optical` (the IFC `Pset_MaterialOptical` / EnergyPlus `WindowMaterial:Glazing` solar-optical record: per-band transmittance with side-asymmetric front/back reflectances for the visible and solar bands, thermal-IR transmittance with front/back hemispherical emissivities, the band absorptances DERIVED conservation remainders — `Discipline.Energy`) · `Electrical` (the IEC 60364 / NEC substance constants a conductor-, insulation-, or shielding-material declares: `Resistivity` the DC volume resistivity as an ElectricResistivity-typed Ω·m measure, `RelativePermittivity` the dielectric constant εr as a guarded ≥ 1 dimensionless double, the optional `DielectricStrength` breakdown field as a DielectricStrength-typed V/m measure, and the optional `MagneticPermeabilityRelative` μr ratio — `Discipline.Electrical`); a property is a `MaterialPropertySet` case over a `MaterialId`, never a property subtype, and a single-indicator GWP-only environmental model is the deleted 1-of-13 slice of the EN 15804+A2 indicator family.
- Law: FAULT ARITY selects the admission idiom, and the two idioms this branch carries are principled peers, never drift. `[ComplexValueObject]` with `ValidateFactoryArguments` owns SINGLE-fault shape-and-trim admission — one product, one refusal, the generated `Validate` the only authority (`Currency`'s alpha-3 shape gate, `Classification/classification#CLASSIFICATION_AXIS` `Classification`, `Geospatial/reference#GEO_REFERENCE` `ProjectedCrs`) — because the generated factory spine returns at most one error and re-minting the hook to accumulate is unmanufacturable. Hand-rolled private-ctor accumulating `Of` triads earn their place exactly where MULTI-SLOT accumulation IS the contract — every `MaterialPropertySet` case, `FireResistance`, `SampledCurve`, `MaterialComposition`'s three set cases — because a datasheet with three bad columns must report three named faults in one `Fin.Fail`. Owners cross from the first idiom to the second only when the independent-column count passes one; converting a single-fault owner to the triad buys nothing and forfeits the generated `Validate`, `Create`, and equality surface.
- Entry: `MaterialPropertySet.OfMechanical(density, youngsModulus, yieldStrength, ultimateStrength, poissons, thermalExpansion, key, evidence, youngsReduction, yieldReduction)` / `OfOrthotropic(density, e1Parallel, e005Parallel, e2Perpendicular, shearModulus, strength1Parallel, strength2Perpendicular, thermalExpansion, key, evidence, modulusReduction, strengthReduction)` (the trailing `Option<SampledCurve>` pairs the factor-vs-°C temperature-reduction evidence, arriving already admitted through `SampledCurve.Of` and riding as pass-through columns — the `Thermal` `conductivityCurve` shape; `e005Parallel` an `Option` on BOTH arities — the measured fifth-percentile parallel stiffness every timber grade prints and a fractile-less directional source omits, the EN 1995 stability kernels refusing on absence rather than reconstructing a ratio) / `OfThermal(conductivity, specificHeat, uValue, vapourResistanceFactor, key, evidence, conductivityCurve)` (`uValue` an `Option` on BOTH arities — a substance declares no transmittance and the EN ISO 6946 assembly fold owns U, so only a product-declared transmittance fills it) / `OfAcoustic(acoustic)` / `OfFire(rating, resistance)` (+ the full `OfFire(rating, suffix, resistance)`) / `OfEnvironmental(basis, impacts, recycledContent, endOfLifeRecovery, key)` (the `impacts` an `ImmutableArray<double>` of arity `ImpactCategory.Count × LifecycleStage.Count`; the two resource fractions `Option<double>` — scenario data many declarations omit, absence never a fabricated fraction; EPD identity + `LocalDate` expiry ride the `evidence` argument as `PropertyEvidence.Declaration("epd", id, validUntil)`, never per-case columns) / `OfCost(basis, currency, supply, install, lifecycle, key)` (the factory's leading pair mirrors the `Cost` case ctor, so no call site reorders one against the other) / `OfDamping(dampingRatio, rayleigh, key)` / `OfHygrothermal(porosity, waterContent80Rh, freeWaterSaturation, waterAbsorption, key)` / `OfDurability(carbonationRate, chlorideDiffusion, ageingExponent, key)` / `OfOptical(visibleTransmittance, visibleReflectanceFront, visibleReflectanceBack, solarTransmittance, solarReflectanceFront, solarReflectanceBack, thermalIrTransmittance, thermalIrEmissivityFront, thermalIrEmissivityBack, key)` — `OfMechanical`, `OfOrthotropic`, and `OfThermal` each DECLARE both a raw-double and a typed-`MeasureValue` arity discriminating on input shape, the typed form owning the ONE slot set and the raw form coercing its declared-unit doubles (those coercions accumulating among themselves) before delegating into it, so a producer holding `QuantityRow`-minted columns keeps its propagated `MeasureBand` instead of unwrapping to a declared-unit double and re-coercing, and the two arities cannot drift; the typed smart-constructors coerce each measured column to its SI base and guard the dimensionless ratios, every multi-column form an ACCUMULATING admission (each independent column one slot — the shared `Rasm/Domain/validation#ADMISSION_SLOTS` fold on its concrete carrier — the tuple `.Apply` unioning kernel scalar and Element semantic refusals through `Error.Combine`/`ManyErrors`, `.As().ToFin()` collapsing once at the seam return — the public rail stays `Fin<T>`, so consumers are untouched while a bad datasheet reports ALL offending columns; the total `OfAcoustic`/`OfFire` carry no invariant and return the bare case; `OfHygrothermal` (whose trailing `Option<SampledCurve>` `sorptionIsotherm`/`liquidTransport`/`moistureConductivity` columns arrive already admitted through `SampledCurve.Of` and ride as pass-through evidence) binds its `wf >= w80` isotherm refinement AND the curve↔anchor agreement refinement (`Disagrees` at `φ=0.8`/`φ=1.0` within `IsothermAnchorTolerance`) AFTER the accumulated leaves, and `OfOptical` accumulates its six per-band-per-side `τ + ρ <= 1`/`τIR + ε <= 1` conservation refinements as a SECOND stage after the nine in-unit leaves, the COMPOSITE_ADMISSION order) / `OfElectrical(resistivityOhmM, relativePermittivity, dielectricStrengthVPerM, magneticPermeabilityRelative, key)`; `Discipline` reads the case-to-discipline map; the named per-discipline reads derive from ONE private polymorphic `Property<T>()` body (a future case lands its one-line forward — the generic read carried zero external consumers, so it no longer ships public), and `props.Density` is the cross-case substance read over both stiffness carriers.
- Auto: `MaterialPropertySet` is a CLASS-root `[Union]` + `[Equatable]` (the `[GRAPH_FAMILY]` form), so the generated `Switch`/`Map` survive while structural equality and the member diff ride `Generator.Equals` — the `Graph/element#NODE_MODEL` `Node.Material` `[Equatable]` drill descends into each case's columns (a record-root case is an opaque equality leaf that collapses the `Rasm.Persistence` `StructuralMerge` to whole-property replacement); `Discipline` dispatches the generated `Switch` mapping each case to its row (`Mechanical`/`Orthotropic`→`Structural`, `Damping`→`Dynamic`, `Hygrothermal`→`Hygrothermal`, `Durability`→`Durability`, `Optical`→`Energy`, …, `Cost`→`Cost`); the `Of` constructors route each dimensioned value through `MeasureValue.Of(value, UnitsNet.Units.X, key)` (or the TYPED `OfSi` for the registry-less MoistureStorage/ChlorideDiffusivity signatures) so the column carries its SI base and `Dimension`, the Poisson's ratio guarded to the physical isotropic `[0,0.5]` range (the `is >= 0.0 and <= 0.5` relational pattern rejecting an out-of-range ratio AND a `NaN`), every density/stiffness/strength/conductivity column guarded finite-AND-strictly-positive through the per-column `Positive` slot (a negative MPa is finite, so the `MeasureValue.Of` finiteness gate alone admits a physically-impossible negative-stiffness material the seam rejects BY NAME), the dimensionless ratios and the `MeasurementBasis`-relative fractions guarded finite-and-in-unit through the same NaN-rejecting relational patterns, and the raw-double cost columns guarded finite-and-non-negative (the `MeasureValue` finiteness gate never sees the raw-double `Cost`/`Environmental`-fraction carriers, so a bare `< 0.0` guard admits `NaN`/∞ into the content hash is rejected at admission) — every such miss ACCUMULATED across the constructor's independent slots, never first-fault-wins; the `Mechanical` shear modulus is a DERIVED read off `E` and `ν` (the isotropic relation `G = E/(2(1+ν))`), never a drift-prone stored column; the `Acoustic` case wraps the `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` carrier whose `Nrc`/`Saa`/`StcWeighted` are derived reads; the `Fire` case carries the EN 13501-1 reaction class with its smoke/droplet sub-class and the EN 13501-2 R/E/I `FireResistance`; the `Environmental` case stores the EN 15804+A2 impact matrix row-major flat and `OfEnvironmental` guards its `Environmental.MatrixArity` and finiteness once so the derived `IndicatorAt`/`Gwp`/`WholeLife` reads trust the admission.
- Receipt: a `Seq<MaterialPropertySet>` on a `Material` node is the full engineering profile a `Bake`-derived `Element` reads flat — `props.Thermal.Bind(t => t.UValue)`, `props.Mechanical.Map(m => m.YieldStrength)`, `props.Acoustic.Map(a => a.StcWeighted)`, `props.Damping.Map(d => d.DampingRatio)`, `props.Durability.Map(u => u.ChlorideDiffusion)`, or the generic `props.Property<T>()` for a future case before its named forward lands — one node carrying every discipline keyed by `Discipline`; the `Rasm.Compute` analysis route reads the `MeasureValue` columns by `Discipline`, and the assembly aggregation (series-resistance U-value, rule-of-mixtures density, layered STC) folds the `MaterialComposition` plies in Compute, never re-keyed per assembly.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`/`[ValueObject<string>]`), Generator.Equals (`[Equatable]` the class-root `MaterialPropertySet` union's structural equality + the member diff the `Rasm.Persistence` `StructuralMerge` drills, `[OrderedEquality]` the `Environmental.Impacts` matrix), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Validation<Error,_>` the accumulating admission slots joined by the tuple `.Apply` and collapsed `.ToFin()`/`Choose`/`Find`), `Rasm/Domain/validation#ADMISSION_SLOTS` (the shared `Guarded`-sibling slots — `Gate`, `Accumulate`, `Optional` — the material-domain combinators sit beside), NodaTime (`LocalDate` the `PropertyEvidence.ValidUntil` calendar expiry — the exact EPD/declaration date the procurement filter compares, over the deleted lossy int-year), UnitsNet (via `MeasureValue`), System.Collections.Immutable (`ImmutableArray<double>` the immutable impact-matrix store), `Rasm/Domain/identity#CONTENT_KEY` (`CanonicalWriter` the `MaterialPropertySet.CanonicalBytes` content projection writes through), `Rasm` (the kernel `Op` op-key).
- Growth: a new engineering property shared across materials is one column on its `MaterialPropertySet` case; a new property discipline with no fit is one `MaterialPropertySet` case carrying its `Discipline` — never a parallel `Eco`/`Cost` owner (the `Damping`/`Hygrothermal`/`Durability`/`Optical`/`Electrical` cases are this law EXECUTED: each one case + one `Discipline` row + one next-free `CanonicalBytes` ordinal + one named forward, zero new surfaces beside the union); a new fire-reaction class is one `FireRating` row, a new acoustic rating one fold on the `Acoustic` carrier, a new EN 15804 environmental indicator one `ImpactCategory` row (the `Impacts` matrix widens by one indicator row and `IndicatorAt`/`WholeLife` read it with no new column or method); a new admission invariant is one `[ADMISSION_SLOTS]` combinator slot, never a per-constructor guard chain; a new state-dependent measured function (a temperature-dependent modulus, a moisture-dependent property) is one `Option<SampledCurve>` column on its owning case and one `Curve` canon write — the ONE sampled-function carrier, never a per-curve column spray and never a lossy point compression; the family grows by case, column, and vocabulary row, never by a per-discipline material type, and the typed lookup grows by ONE generic `Property<T>()` over the case type and an ergonomic named forward, never a per-case roster of independent `Choose` bodies.
- Boundary: `MaterialPropertySet` is the ONE typed property family — a per-discipline material type is the deleted form, a property being a case over a `MaterialId`. The family is FULL-VECTOR and AUTHORED-only: an imported foreign `Pset` lands as a neutral `PROPERTY_BAG` node under `EvidenceGrade.Import`, never Option-widened columns (widening fabricates every undeclared column); the CARVE is the column an authored producer structurally cannot declare (product-only `UValue`, scenario fractions, tested-system-only fire `Reaction`) — each an `Option` whose absence is the domain's own state. Every dimensioned column admits through `MeasureValue`; the isotropic `G` DERIVES from `E`/`ν` while a measured directional `G` rides `Orthotropic`; fire is a closed reaction vocabulary + the typed R/E/I criteria (a single scalar cannot tell `R 90` from `EI 60`); the `Acoustic` case is the banded carrier, never a scalar STC. The `Cost` case carries neutral per-unit doubles over an OPAQUE ISO 4217 `Currency` + `MeasurementBasis` (`Rasm.Bim`'s NodaMoney algebra owns the roster and the quantity×rate join). The `Environmental` case's flat `(ImpactCategory × LifecycleStage)` `ImmutableArray` matrix is the ONE impact store (13 indicators × 6 modules; a GWP-only vector is the deleted 1-of-13 slice; a `Map` of arrays reference-compares under Generator.Equals) — `IndicatorAt`/`Gwp`/`WholeLife`/`StageGwp` DERIVE from it, `OfEnvironmental` gates arity-then-finite once (`TensorPrimitives.IsFiniteAll`), an EPD declaring fewer indicators zeroes the rest so arity is invariant, and every cell rides the case's `MeasurementBasis` so the Compute folds scale by the basis-matching quantity. Provenance is SINGLE-stored on `PropertyEvidence` (a per-case EPD column pair double-stores it; an int year is lossy against a full expiry date). `Damping.DampingRatio` (large-strain design ζ) never derives from the acoustic small-strain `LossFactor` η — different standards at amplitude regimes apart — while `StructuralLossFactor = 2ζ` is the case's own derived input. `Optical` carries the EN 410 / EnergyPlus engineering constants with side-asymmetric fronts/backs, absorptances DERIVED as conservation remainders and the `Conserves` slot refusing an unphysical datasheet; render appearance stays the `Rasm.Materials` Appearance owner's. `Electrical` carries SUBSTANCE constants alone (ampacity is a component row). Fractional-exponent quantities (mm/√year, kg/(m²·√s)) stay raw doubles with the unit in the NAME — √t is inexpressible in the integer 7-vector — while every integer-dimension column is a typed `MeasureValue` under the same-dimension-distinct-type discipline.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
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

 public static Fin<FireRating> Parse(string reaction, Op key) =>
  key.AcceptValidated<FireRating>(reaction);
}

// EN 13501-1 sub-classification pair as ONE product (the former sibling rosters shared shape, one consuming
// file, and nothing on a value said which axis): tokens close at admission, one [ObjectFactory<string>] grammar
// serves the "s1,d0" | "s1" | "d0" | "" datasheet spellings, and NotSpecified is the absent pair — a "B"
// reports "B-s1,d0" complete.
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

 // The joined datasheet suffix delegates its admitted pair to the generated product validator.
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

// EN 13501-2 criteria in minutes, each an Option because ABSENCE and ZERO are different facts: an ACI 216.1
// equivalent-thickness derivation measures INSULATION alone — a zero R/E publishes a fabricated failing
// rating (FORGED_ZERO). Of admits any subset with at least one measured criterion; negatives accumulate.
public readonly record struct FireResistance {
 public Option<int> LoadBearingMinutes { get; }
 public Option<int> IntegrityMinutes { get; }
 public Option<int> InsulationMinutes { get; }

 private FireResistance(Option<int> loadBearingMinutes, Option<int> integrityMinutes, Option<int> insulationMinutes) =>
  (LoadBearingMinutes, IntegrityMinutes, InsulationMinutes) = (loadBearingMinutes, integrityMinutes, insulationMinutes);

  // None carries the unclassified rating: nothing measured on any criterion, distinct from "measured at zero minutes".
 public static readonly FireResistance None = new(Option<int>.None, Option<int>.None, Option<int>.None);

 public static Fin<FireResistance> Of(Option<int> loadBearingMinutes, Option<int> integrityMinutes, Option<int> insulationMinutes, Op key) =>
  (Gate(loadBearingMinutes.IsSome || integrityMinutes.IsSome || insulationMinutes.IsSome, key, "<fire-resistance-unmeasured>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Minutes(loadBearingMinutes, "load-bearing", key),
   Minutes(integrityMinutes, "integrity", key),
   Minutes(insulationMinutes, "insulation", key))
  .Apply(static (_, r, e, i) => new FireResistance(r, e, i))
  .As().ToFin();

  // The coverage-keyed mint: the (R,E,I) presence triple is ROW DATA, so a new EN 13501-2 combination (RE, EW…)
 // is one FireCoverage row; the named mints stay as one-hop reads over it for the live peer call sites, each
 // stating which criteria the test COVERED, the rest absent rather than zero-filled.
 public static Fin<FireResistance> Of(FireCoverage coverage, int minutes, Op key) =>
  Of(coverage.LoadBearing ? Some(minutes) : None,
     coverage.Integrity ? Some(minutes) : None,
     coverage.Insulation ? Some(minutes) : None, key);

 public static Fin<FireResistance> Rei(int minutes, Op key) => Of(FireCoverage.Rei, minutes, key);
 public static Fin<FireResistance> R(int minutes, Op key) => Of(FireCoverage.R, minutes, key);
 public static Fin<FireResistance> Ei(int minutes, Op key) => Of(FireCoverage.Ei, minutes, key);
  // I is the insulation-only mint an ACI 216.1 equivalent-thickness derivation takes — the arity a copied R/E figure forges.
 public static Fin<FireResistance> I(int minutes, Op key) => Of(FireCoverage.I, minutes, key);

 private static Validation<Error, Option<int>> Minutes(Option<int> value, string criterion, Op key) =>
  value.Exists(static minutes => minutes < 0)
   ? new KernelFault.OutOfRange($"fire-resistance-{criterion}", value.IfNone(0), "be non-negative", Some(key))
   : value;
}

// EN 13501-2 coverage rows carrying the criterion presence triple.
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

// LifecycleStage rows the EN 15978 modules banding the COLUMN axis of the Environmental case's (ImpactCategory × LifecycleStage)
// Impacts matrix over (ImpactCategory is the row axis). Index is the matrix column, Count the per-indicator stage arity.
[SmartEnum<int>]
public sealed partial class LifecycleStage {
 public static readonly LifecycleStage A1A3 = new(0, "A1-A3");  // product (the cradle-to-gate boundary the Gwp carries)
 public static readonly LifecycleStage A4 = new(1, "A4");       // transport to site
 public static readonly LifecycleStage A5 = new(2, "A5");       // construction-installation
 public static readonly LifecycleStage B = new(3, "B1-B7");     // use / maintenance / operational
 public static readonly LifecycleStage C = new(4, "C1-C4");     // end-of-life
 public static readonly LifecycleStage D = new(5, "D");         // benefits / loads beyond the system boundary
 public string Module { get; }
 public int Index => Key;
 // Items-derived arity through an ACCESSOR, never an eager static initializer — reading the generated lazy Items
 // inside this type's own static init races the cross-partial field-order the [LOOKUP_LIFECYCLE] law forbids.
 public static int Count => Items.Count;
}

// OPAQUE ISO 4217 alpha-3 token: shape is the seam invariant, MEMBERSHIP is the Rasm.Bim NodaMoney roster's
// (the same shape-vs-roster split Classification.System holds) — a closed currency enum forces a seam edit
// per currency.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Currency {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim().ToUpperInvariant();
  if (value.Length != 3 || !value.All(char.IsAsciiLetterUpper)) { validationError = new ValidationError("currency requires an ISO 4217 alpha-3 code"); }
 }
 // Parse rails a wire/catalog token through the kernel generated-owner bridge; the ISO 4217 roster check is NodaMoney's.
 public static Fin<Currency> Parse(string code, Op key) =>
  key.AcceptValidated<Currency>(code);
}

// MeasurementBasis declares the unit basis the Cost AND Environmental cases share — a genuinely CLOSED four-set (per mass/area/
// volume/item), so here a non-standard basis IS a row, never a free string (unlike the OPEN Currency above whose
// roster lives downstream): the four bases are the complete declared-unit closure, never a downstream-owned roster.
[SmartEnum<string>]
public sealed partial class MeasurementBasis {
 public static readonly MeasurementBasis PerKg = new("per-kg");
 public static readonly MeasurementBasis PerM2 = new("per-m2");
 public static readonly MeasurementBasis PerM3 = new("per-m3");
 public static readonly MeasurementBasis PerItem = new("per-item");
 public static Fin<MeasurementBasis> Parse(string token, Op key) =>
  key.AcceptValidated<MeasurementBasis>(token);
}

// EN 15804+A2 indicator rows banding the Impacts matrix ROW axis; Name is the EPD wire token, Unit the
// characterization base, Index the matrix row.
[SmartEnum<int>]
public sealed partial class ImpactCategory {
 public static readonly ImpactCategory GwpTotal = new(0, "GWP-total", "kg CO2 eq");        // EN 15804+A2 total global-warming potential
 public static readonly ImpactCategory GwpFossil = new(1, "GWP-fossil", "kg CO2 eq");      // fossil-fuel component
 public static readonly ImpactCategory GwpBiogenic = new(2, "GWP-biogenic", "kg CO2 eq");  // biogenic-carbon component
 public static readonly ImpactCategory GwpLuluc = new(3, "GWP-luluc", "kg CO2 eq");        // land-use / land-use-change component
 public static readonly ImpactCategory Odp = new(4, "ODP", "kg CFC11 eq");                 // ozone-depletion potential
 public static readonly ImpactCategory Ap = new(5, "AP", "mol H+ eq");                     // acidification potential
 public static readonly ImpactCategory EpFreshwater = new(6, "EP-freshwater", "kg P eq");  // eutrophication, freshwater
 public static readonly ImpactCategory EpMarine = new(7, "EP-marine", "kg N eq");          // eutrophication, marine
 public static readonly ImpactCategory EpTerrestrial = new(8, "EP-terrestrial", "mol N eq"); // eutrophication, terrestrial
 public static readonly ImpactCategory Pocp = new(9, "POCP", "kg NMVOC eq");               // photochemical-ozone-creation potential
 public static readonly ImpactCategory AdpMinerals = new(10, "ADP-minerals", "kg Sb eq");  // abiotic depletion, minerals/metals
 public static readonly ImpactCategory AdpFossil = new(11, "ADP-fossil", "MJ");            // abiotic depletion, fossil resources
 public static readonly ImpactCategory Wdp = new(12, "WDP", "m3 world eq");                // water (user) deprivation potential

 public string Name { get; }
 public string Unit { get; }
 public int Index => Key;
 // Items-derived arity through an accessor (the LifecycleStage.Count discipline) — never an eager static initializer.
 public static int Count => Items.Count;

 // The EPD wire token resolves the Name column ordinal-insensitively through the kernel row bridge.
 public static Fin<ImpactCategory> Parse(string name, Op key) =>
  key.Row<string, int, ImpactCategory>(name, static c => c.Name, Some<IEqualityComparer<string>>(StringComparer.OrdinalIgnoreCase));
}

// --- [MODELS] -----------------------------------------------------------------------------
// CLASS-root [Union] + per-case [Equatable] (the MaterialComposition discipline): the Node.Material drill
// descends each case's columns, so the StructuralMerge localizes a changed property to its member.
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
  // YoungsReduction/YieldReduction carry the TEMPERATURE-REDUCTION factor curves (axis °C, values the fraction
  // of the 20 °C datum — the EN 1993-1-2 kE,θ/ky,θ and EN 1992-1-2 kc(θ) tables) the fire route reads beside the
  // steady-state scalars — the Thermal.ConductivityCurve shape: each rides BESIDE its scalar, arrives already
  // admitted through SampledCurve.Of, and None is a material whose source declared no curve, never a fabricated table.
  public Option<SampledCurve> YoungsReduction { get; } = youngsReduction;
  public Option<SampledCurve> YieldReduction { get; } = yieldReduction;
  // Isotropic shear modulus G = E/(2(1+ν)) — a DERIVED read, never a stored column that could drift from E/ν
  // (the seam Mechanical is isotropic: one E, one ν, so G is exact, not an independent datum). Both operands are
  // ADMITTED — E finite and strictly positive through the Positive slot, ν in [0,0.5] through the isotropic-range
  // pattern — so 2(1+ν) ∈ [2,3], G ∈ [E/3, E/2], and the derivation is TOTAL: the trusted Reproject re-mint
  // carries E's OWN proven signature forward, so the read stays a bare MeasureValue and never wears a rail —
  // or a ThrowIfFail — over an infallible op.
  public MeasureValue ShearModulus =>
   MeasureValue.Reproject(QuantitySignature.Of(YoungsModulus), YoungsModulus.Si / (2.0 * (1.0 + PoissonsRatio)));
 }
  // The directional structural case: E1∥/E2⊥ and a MEASURED G the isotropic Mechanical structurally cannot
  // model (timber G_mean ≈ E0/16, never E/(2(1+ν))) — the timber family's seam home; also Discipline.Structural, the
  // case TYPE the discriminant.
 [Equatable]
 public sealed partial class Orthotropic(MeasureValue density, MeasureValue e1Parallel, Option<MeasureValue> e005Parallel, MeasureValue e2Perpendicular, MeasureValue shearModulus, MeasureValue strength1Parallel, MeasureValue strength2Perpendicular, double thermalExpansionPerK, PropertyEvidence evidence, Option<SampledCurve> modulusReduction = default, Option<SampledCurve> strengthReduction = default) : MaterialPropertySet(evidence) {
  public MeasureValue Density { get; } = density;
  public MeasureValue E1Parallel { get; } = e1Parallel;
  // E005 is the MEASURED fifth-percentile parallel stiffness (EN 338 / EN 14080 print it per grade; timber's
  // lowering always fills it) — Some carries the printed value, None is a directional source whose standard
  // declares no fractile, and the EN 1995-1-1 §6.3.2/§6.3.3 stability kernels refuse on None rather than
  // reconstructing the softwood-only 0.67·E0 ratio (GL24c measures 0.827, D30 0.836).
  public Option<MeasureValue> E005 { get; } = e005Parallel;
  public MeasureValue E2Perpendicular { get; } = e2Perpendicular;
  public MeasureValue ShearModulus { get; } = shearModulus;
  public MeasureValue Strength1Parallel { get; } = strength1Parallel;
  public MeasureValue Strength2Perpendicular { get; } = strength2Perpendicular;
  public double ThermalExpansionPerK { get; } = thermalExpansionPerK;
  // ModulusReduction/StrengthReduction mirror the Mechanical pair for the directional carrier (the EN 1995-1-2
  // kθ tables give ONE factor curve per property class covering both principal directions, so the pair scales
  // E1/E2 and the two strengths uniformly); factor vs °C, admitted through SampledCurve.Of, None when undeclared.
  public Option<SampledCurve> ModulusReduction { get; } = modulusReduction;
  public Option<SampledCurve> StrengthReduction { get; } = strengthReduction;
 }
 [Equatable]
 public sealed partial class Thermal(MeasureValue conductivity, MeasureValue specificHeat, Option<MeasureValue> uValue, double vapourResistanceFactor, PropertyEvidence evidence, Option<SampledCurve> conductivityCurve = default) : MaterialPropertySet(evidence) {
  public MeasureValue Conductivity { get; } = conductivity;
  public MeasureValue SpecificHeat { get; } = specificHeat;
  // UValue stays None for a SUBSTANCE — the EN ISO 6946 assembly fold owns U; Some carries a genuinely
  // PRODUCT-declared transmittance (an IGU or window-part datasheet), and None is every bulk material.
  public Option<MeasureValue> UValue { get; } = uValue;
  public double VapourResistanceFactor { get; } = vapourResistanceFactor;
  // ConductivityCurve carries the TEMPERATURE-DEPENDENT λ(θ) the case's charter cites: the EN 1992-1-2 / EN 1993-1-2 fire
  // routes and every transient thermal solve read conductivity AT a temperature, and a scalar alone forces every
  // consumer to either assume the 20 °C value across a 1000 °C fire curve or carry its own table. It rides BESIDE the scalar
  // rather than replacing it — the scalar stays the steady-state ISO 6946 datum every U-value fold
  // reads — and it arrives already admitted through SampledCurve.Of, exactly as the Hygrothermal curves do; None
  // is a material whose source declared no curve, never a fabricated one-point table.
  public Option<SampledCurve> ConductivityCurve { get; } = conductivityCurve;
 }
 [Equatable]
 public sealed partial class Acoustic(global::Rasm.Element.Composition.Acoustic spectrum, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public global::Rasm.Element.Composition.Acoustic Spectrum { get; } = spectrum;
  // Every forward is corpus-live: the Fabrication ingress marshals all nine and the wire codec reads the
  // intrinsic constants — the case is the consumer contract, Spectrum the algebra owner.
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
 // Reaction is an OPTION: a producer whose fire truth is a TESTED-SYSTEM resistance alone (an IGU build with EI minutes
 // and no substance reaction class to author) declares absence rather than fabricating a class; the Suffix rides the
 // reaction declaration, so an absent reaction carries NotSpecified sub-classes by construction.
 [Equatable]
 public sealed partial class Fire(Option<FireRating> reaction, EuroclassSuffix suffix, FireResistance resistance, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public Option<FireRating> Reaction { get; } = reaction;
  public EuroclassSuffix Suffix { get; } = suffix;
  public FireResistance Resistance { get; } = resistance;
 }
 // BASIS-AWARE: every cell is its indicator's characterization quantity PER the Basis unit — the Compute
 // folds scale each ply by the basis-matching quantity (the same DeclaredQuantity derivation the cost fold
 // uses), never a forced per-m³ normalization that demanded a density and dropped an area/item EPD.
 [Equatable]
 public sealed partial class Environmental(MeasurementBasis basis, ImmutableArray<double> impacts, Option<double> recycledContent, Option<double> endOfLifeRecovery, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasurementBasis Basis { get; } = basis;
  [property: OrderedEquality] public ImmutableArray<double> Impacts { get; } = impacts;
  // Two EN 15804 resource fractions ride OPTIONS: scenario data many declarations omit, where a required
  // slot forces the fabricated fraction the admission exists to refuse — absence is the honest undeclared state,
  // distinct from a measured zero.
  public Option<double> RecycledContent { get; } = recycledContent;
  public Option<double> EndOfLifeRecovery { get; } = endOfLifeRecovery;

  // IndicatorAt is the one general read: an (indicator, stage) cell off the row-major flat matrix — every named convenience below
  // derives from it, never a per-indicator span scan. An out-of-arity (indicator, stage) reads 0.0 (a not-declared cell is
  // zero impact), so a partial EPD (carbon-only, the EC3 ingress) zeroes the un-declared indicator rows rather than faulting.
  public double IndicatorAt(ImpactCategory category, LifecycleStage stage) {
   int i = category.Index * LifecycleStage.Count + stage.Index;
   return i >= 0 && i < Impacts.Length ? Impacts[i] : 0.0;
  }
  // WholeLife folds ONE indicator cradle-to-grave across every lifecycle stage — the general WholeLifeGwp the
  // Rasm.Compute embodied-carbon rollup reads per indicator, derived from IndicatorAt over the stage vocabulary.
  public double WholeLife(ImpactCategory category) =>
   LifecycleStage.Items.Sum(stage => IndicatorAt(category, stage));
  // DERIVED, never stored: the headline cradle-to-gate A1-A3 carbon IS the (GwpTotal, A1A3) matrix cell — a parallel stored
  // GlobalWarmingPotential scalar is a double-store of one fact (DERIVED_LOGIC) the acoustic carrier never admits. Gwp/
  // StageAt/WholeLifeGwp are the carbon-keyed convenience projections over the general IndicatorAt/WholeLife, so a
  // carbon-only consumer reads one hop while the matrix stays the ONE owner; the OfEnvironmental admission guards Impacts
  // finiteness so these reads never surface a NaN.
  public double Gwp => IndicatorAt(ImpactCategory.GwpTotal, LifecycleStage.A1A3);   // cradle-to-gate A1-A3 GwpTotal (per Basis unit)
  public double StageAt(LifecycleStage stage) => IndicatorAt(ImpactCategory.GwpTotal, stage);  // per-stage GwpTotal (per Basis unit)
  public double WholeLifeGwp => WholeLife(ImpactCategory.GwpTotal);                 // cradle-to-grave GwpTotal (per Basis unit)
  // StageGwp reads the GwpTotal row FLAT — the per-module carbon vector the Compute fold reads one-hop,
  // DERIVED from the matrix, never a parallel stored 6-vector.
  public ImmutableArray<double> StageGwp =>
   [.. LifecycleStage.Items.OrderBy(static s => s.Index).Select(s => IndicatorAt(ImpactCategory.GwpTotal, s))];
  // The WRITE dual of StageGwp: a carbon-only per-module stage vector (the EC3 ingress) embeds into a full
  // zeroed matrix, so arity stays invariant and every read path is one.
  public static ImmutableArray<double> CarbonMatrix(ReadOnlyMemory<double> stageGwp) {
   double[] matrix = new double[MatrixArity];
   ReadOnlySpan<double> row = stageGwp.Span;
   int gwpRow = ImpactCategory.GwpTotal.Index * LifecycleStage.Count;
   int stages = Math.Min(row.Length, LifecycleStage.Count);
   for (int s = 0; s < stages; s++) { matrix[gwpRow + s] = row[s]; }
   return [.. matrix];
  }
  // MatrixArity gives the row arity OfEnvironmental admits against (every indicator × every stage) — an accessor, so the
  // arity always reads the materialized vocabularies and never races a static-init order.
  public static int MatrixArity => ImpactCategory.Count * LifecycleStage.Count;
  // Zero-impact accumulator seed (lazy: MatrixArity reads generated Items counts — LOOKUP_LIFECYCLE).
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
  // DYNAMIC carrier: ζ the EN 1998 fraction-of-critical ratio, optional per-material Rayleigh (α, β) pair,
  // StructuralLossFactor = 2ζ the case's OWN derived FE input (never the acoustic small-strain η).
 [Equatable]
 public sealed partial class Damping(double dampingRatio, Option<(double AlphaPerS, double BetaS)> rayleigh, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double DampingRatio { get; } = dampingRatio;
  public Option<(double AlphaPerS, double BetaS)> Rayleigh { get; } = rayleigh;
  public double StructuralLossFactor => 2.0 * DampingRatio;
 }
  // EN 15026 TRANSIENT carrier: porosity, the two sorption anchors (MoistureStorage-typed kg/m³), the
  // optional capillary A-value (kg/(m²·√s) — √t inexpressible in the 7-vector, unit in the name).
 [Equatable]
 public sealed partial class Hygrothermal(double porosity, MeasureValue waterContent80Rh, MeasureValue freeWaterSaturation, Option<double> waterAbsorptionKgPerM2SqrtS, Option<SampledCurve> sorptionIsotherm, Option<SampledCurve> liquidTransport, Option<SampledCurve> moistureConductivity, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double Porosity { get; } = porosity;
  public MeasureValue WaterContent80Rh { get; } = waterContent80Rh;
  public MeasureValue FreeWaterSaturation { get; } = freeWaterSaturation;
  public Option<double> WaterAbsorptionKgPerM2SqrtS { get; } = waterAbsorptionKgPerM2SqrtS;
  // The three measured functions (w(φ), Dw(w), λ(w)) ride the ONE SampledCurve carrier, pass-through admitted.
  public Option<SampledCurve> SorptionIsotherm { get; } = sorptionIsotherm;
  public Option<SampledCurve> LiquidTransport { get; } = liquidTransport;
  public Option<SampledCurve> MoistureConductivity { get; } = moistureConductivity;
 }
  // fib Model Code service-life carrier: carbonation K (mm/√year, unit-in-name), chloride D_RCM
  // (ChlorideDiffusivity-typed), ageing exponent.
 [Equatable]
 public sealed partial class Durability(double carbonationRateMmPerSqrtYear, MeasureValue chlorideDiffusion, double ageingExponent, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double CarbonationRateMmPerSqrtYear { get; } = carbonationRateMmPerSqrtYear;
  public MeasureValue ChlorideDiffusion { get; } = chlorideDiffusion;
  public double AgeingExponent { get; } = ageingExponent;
 }
  // SOLAR-OPTICAL carrier (EN 410 / EnergyPlus WindowMaterial:Glazing): side-asymmetric fronts/backs (a
  // coated pane is directional); absorptances are DERIVED conservation remainders, never stored.
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
  // SolarAbsorptance gives the EnergyPlus opaque-surface α and the EN 410 g-value secondary-heat input — the conservation
  // remainder per side, non-negative by the OfOptical conservation refinement.
  public double SolarAbsorptanceFront => 1.0 - SolarTransmittance - SolarReflectanceFront;
  public double SolarAbsorptanceBack => 1.0 - SolarTransmittance - SolarReflectanceBack;
 }
  // SUBSTANCE electrical carrier (IEC 60364/NEC): resistivity (ElectricResistivity-typed), εr >= 1, optional
  // dielectric strength and μr — component facts (ampacity, derating) stay DetailSchema rows.
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

  // Case ordinal + evidence axes + payload through kernel Optional/Doubles — presence-prefixed and
  // count-framed, so ordinals never shift and the flat impact matrix stays self-delimiting.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  mechanical:    m => CaseBytes(w, 0).Measure(m.Density).Measure(m.YoungsModulus).Measure(m.YieldStrength).Measure(m.UltimateStrength).Double(m.PoissonsRatio).Double(m.ThermalExpansionPerK)
   .Optional(m.YoungsReduction, static (c, run) => c.CanonicalBytes(run)).Optional(m.YieldReduction, static (c, run) => c.CanonicalBytes(run)),
  thermal:       t => CaseBytes(w, 1).Measure(t.Conductivity).Measure(t.SpecificHeat)
   .Optional(t.UValue, static (u, run) => run.Measure(u)).Double(t.VapourResistanceFactor)
   .Optional(t.ConductivityCurve, static (c, run) => c.CanonicalBytes(run)),
  acoustic:      a => { CaseBytes(w, 2); a.Spectrum.CanonicalBytes(w); return w; },   // the banded carrier's own projection returns void, so the arm returns the writer it wrote through
  fire:          f => CaseBytes(w, 3)
   .Optional(f.Reaction, static (r, run) => run.String(r.Key))
   .String(f.Suffix.Smoke).String(f.Suffix.Droplets)
   .Optional(f.Resistance.LoadBearingMinutes, static (m, run) => run.Ordinal(m))
   .Optional(f.Resistance.IntegrityMinutes, static (m, run) => run.Ordinal(m))
   .Optional(f.Resistance.InsulationMinutes, static (m, run) => run.Ordinal(m)),
  environmental: e => CaseBytes(w, 4).String(e.Basis.Key).Doubles(e.Impacts.AsSpan())
   .Optional(e.RecycledContent, static (r, run) => run.Double(r))
   .Optional(e.EndOfLifeRecovery, static (r, run) => run.Double(r)),
  cost:          c => CaseBytes(w, 5).String(c.Basis.Key).String(c.Currency.Value).Double(c.SupplyPerUnit).Double(c.InstallPerUnit).Double(c.LifecyclePerUnit),
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

 // Evidence axes are identity-bearing (two property sets from two EPDs are two facts): source, optional
 // reference, calendar expiry, grade rank, attestation, and the run audit through its own canonical fold.
 CanonicalWriter CaseBytes(CanonicalWriter w, int ordinal) =>
  w.Ordinal(ordinal).String(Evidence.Source)
   .Optional(Evidence.Reference, static (r, run) => run.String(r))
   .Optional(Evidence.ValidUntil, static (d, run) => run.Ordinal(d.Year).Ordinal(d.Month).Ordinal(d.Day))
   .Ordinal(Evidence.Grade.Key)
   .Optional(Evidence.Attested, static (a, run) => run.String(a.Role.Key).String(a.Credential).U128(a.Payload.Value).I64(a.At.ToUnixTimeTicks()))
   .Optional(Evidence.Run, static (r, run) => r.CanonicalBytes(run));

 public static Fin<MaterialPropertySet> OfMechanical(double density, double youngsModulus, double yieldStrength, double ultimateStrength, double poissons, double thermalExpansion, Op key, PropertyEvidence evidence = default, Option<SampledCurve> youngsReduction = default, Option<SampledCurve> yieldReduction = default) =>
  (Coerce(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter, key),
   Coerce(youngsModulus, UnitsNet.Units.PressureUnit.Megapascal, key),
   Coerce(yieldStrength, UnitsNet.Units.PressureUnit.Megapascal, key),
   Coerce(ultimateStrength, UnitsNet.Units.PressureUnit.Megapascal, key))
  .Apply(static (rho, e, fy, fu) => (Density: rho, Youngs: e, Yield: fy, Ultimate: fu))
  .As().ToFin()
  .Bind(column => OfMechanical(column.Density, column.Youngs, column.Yield, column.Ultimate, poissons, thermalExpansion, key, evidence, youngsReduction, yieldReduction));

  // OfMechanical's TYPED arity is the ONE body: a producer that already minted its columns through a QuantityRow (the
 // Rasm.Materials catalogue lowering, an assessed-column rebuild) hands the MeasureValues straight in rather than
 // unwrapping to a raw double in a declared unit and re-coercing — a round trip that re-scales an SI magnitude by the
 // unit's own factor and silently drops the propagated MeasureBand.
 public static Fin<MaterialPropertySet> OfMechanical(MeasureValue density, MeasureValue youngsModulus, MeasureValue yieldStrength, MeasureValue ultimateStrength, double poissons, double thermalExpansion, Op key, PropertyEvidence evidence = default, Option<SampledCurve> youngsReduction = default, Option<SampledCurve> yieldReduction = default) =>
  (Positive(density, "mechanical-density", key),
   Positive(youngsModulus, "mechanical-youngs-modulus", key),
   Positive(yieldStrength, "mechanical-yield-strength", key),
   Positive(ultimateStrength, "mechanical-ultimate-strength", key),
   InRange(poissons, 0.0, 0.5, "poisson-isotropic", key),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite", key))
  .Apply((d, e, y, u, nu, a) => (MaterialPropertySet)new Mechanical(d, e, y, u, nu, a, evidence, youngsReduction, yieldReduction))
  .As().ToFin();

  // RAW arity: coerce the declared-unit doubles (accumulating among themselves), delegate to the typed body.
 public static Fin<MaterialPropertySet> OfOrthotropic(double density, double e1Parallel, Option<double> e005Parallel, double e2Perpendicular, double shearModulus, double strength1Parallel, double strength2Perpendicular, double thermalExpansion, Op key, PropertyEvidence evidence = default, Option<SampledCurve> modulusReduction = default, Option<SampledCurve> strengthReduction = default) =>
  (Coerce(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter, key),
   Coerce(e1Parallel, UnitsNet.Units.PressureUnit.Megapascal, key),
   e005Parallel.Match(
    Some: e => Coerce(e, UnitsNet.Units.PressureUnit.Megapascal, key).Map(Option<MeasureValue>.Some),
    None: () => Validation<Error, Option<MeasureValue>>.Success(Option<MeasureValue>.None)),
   Coerce(e2Perpendicular, UnitsNet.Units.PressureUnit.Megapascal, key),
   Coerce(shearModulus, UnitsNet.Units.PressureUnit.Megapascal, key),
   Coerce(strength1Parallel, UnitsNet.Units.PressureUnit.Megapascal, key),
   Coerce(strength2Perpendicular, UnitsNet.Units.PressureUnit.Megapascal, key))
  .Apply(static (rho, e1, e05, e2, g, s1, s2) => (Density: rho, E1: e1, E005: e05, E2: e2, Shear: g, S1: s1, S2: s2))
  .As().ToFin()
  .Bind(column => OfOrthotropic(column.Density, column.E1, column.E005, column.E2, column.Shear, column.S1, column.S2, thermalExpansion, key, evidence, modulusReduction, strengthReduction));

  // OfOrthotropic's TYPED arity is the ONE body — the wire decode arm and a QuantityRow-minted producer hand admitted
 // MeasureValues straight in, keeping the propagated MeasureBand instead of unwrapping to a declared-unit double and
 // re-coercing (the OfMechanical typed-arity discipline). No isotropic Poisson guard — G is a measured datum here,
 // not the derived E/(2(1+ν)). Eight independent slots (the tuple `.Apply` family reaches ten), every offending column
 // reported at once — the fractile slot guards Positive only when Some, absence passing through as the typed None the
 // stability kernels refuse on. A material lowers EITHER an isotropic Mechanical OR a directional Orthotropic, never
 // both — the case TYPE is the discriminant the structural runner reads.
 public static Fin<MaterialPropertySet> OfOrthotropic(MeasureValue density, MeasureValue e1Parallel, Option<MeasureValue> e005Parallel, MeasureValue e2Perpendicular, MeasureValue shearModulus, MeasureValue strength1Parallel, MeasureValue strength2Perpendicular, double thermalExpansion, Op key, PropertyEvidence evidence = default, Option<SampledCurve> modulusReduction = default, Option<SampledCurve> strengthReduction = default) =>
  (Positive(density, "orthotropic-density", key),
   Positive(e1Parallel, "orthotropic-e1-parallel", key),
   e005Parallel.Match(
    Some: e => Positive(e, "orthotropic-e005-parallel", key).Map(Option<MeasureValue>.Some),
    None: () => Validation<Error, Option<MeasureValue>>.Success(Option<MeasureValue>.None)),
   Positive(e2Perpendicular, "orthotropic-e2-perpendicular", key),
   Positive(shearModulus, "orthotropic-shear-modulus", key),
   Positive(strength1Parallel, "orthotropic-strength1-parallel", key),
   Positive(strength2Perpendicular, "orthotropic-strength2-perpendicular", key),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite", key))
  .Apply((rho, e1, e05, e2, g, s1, s2, a) => (MaterialPropertySet)new Orthotropic(rho, e1, e05, e2, g, s1, s2, a, evidence, modulusReduction, strengthReduction))
  .As().ToFin();

  // OfThermal's RAW arity takes the OfMechanical shape: coerce the three declared-unit doubles, then delegate to the typed
 // body that owns every slot.
 public static Fin<MaterialPropertySet> OfThermal(double conductivity, double specificHeat, Option<double> uValue, double vapourResistanceFactor, Op key, PropertyEvidence evidence = default, Option<SampledCurve> conductivityCurve = default) =>
  (Coerce(conductivity, UnitsNet.Units.ThermalConductivityUnit.WattPerMeterKelvin, key),
   Coerce(specificHeat, UnitsNet.Units.SpecificEntropyUnit.JoulePerKilogramKelvin, key),
   uValue.Match(
    Some: u => MeasureValue.Of(u, UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, key).Map(Option<MeasureValue>.Some).ToValidation(),
    None: () => Validation<Error, Option<MeasureValue>>.Success(Option<MeasureValue>.None)))
  .Apply(static (lambda, cp, u) => (Conductivity: lambda, SpecificHeat: cp, UValue: u))
  .As().ToFin()
  .Bind(column => OfThermal(column.Conductivity, column.SpecificHeat, column.UValue, vapourResistanceFactor, key, evidence, conductivityCurve));

  // OfThermal's TYPED arity is the ONE body — a caller holding QuantityRow-minted columns keeps its bands. The
 // vapour-resistance factor μ is dimensionless and >= 1 by definition (μ = 1 is still air, no material resists vapour
 // LESS than air), so the `is >= 1.0` relational pattern accepts unity-and-above AND rejects NaN in one test — a bare
 // `< 1.0` admits NaN. Conductivity / specific-heat / U-value are strictly positive physical quantities the per-column
 // Positive slot rejects with the offending column NAMED, all misses accumulated.
 public static Fin<MaterialPropertySet> OfThermal(MeasureValue conductivity, MeasureValue specificHeat, Option<MeasureValue> uValue, double vapourResistanceFactor, Op key, PropertyEvidence evidence = default, Option<SampledCurve> conductivityCurve = default) =>
  (Positive(conductivity, "thermal-conductivity", key),
   Positive(specificHeat, "thermal-specific-heat", key),
   uValue.Match(
    Some: u => Positive(u, "thermal-u-value", key).Map(Option<MeasureValue>.Some),
    None: () => Validation<Error, Option<MeasureValue>>.Success(Option<MeasureValue>.None)),
   Guarded(vapourResistanceFactor is >= 1.0, vapourResistanceFactor, "vapour-resistance-factor-below-unity", key))
  .Apply((c, s, u, mu) => (MaterialPropertySet)new Thermal(c, s, u, mu, evidence, conductivityCurve))
  .As().ToFin();

 public static MaterialPropertySet OfAcoustic(global::Rasm.Element.Composition.Acoustic spectrum, PropertyEvidence evidence = default) =>
  new Acoustic(spectrum, evidence);

  // OfFire's 2-arg form defaults the smoke/droplet sub-class (NotSpecified) for a reaction-class-only datasheet — or takes an
 // ABSENT reaction for a tested-system resistance-only record; the full form admits the complete EN 13501-1 "B-s1,d0"
 // classification and therefore demands the concrete reaction its sub-classes qualify. Both total — a
 // FireRating/FireResistance carry their own admission.
 public static MaterialPropertySet OfFire(Option<FireRating> reaction, FireResistance resistance, PropertyEvidence evidence = default) =>
  new Fire(reaction, EuroclassSuffix.NotSpecified, resistance, evidence);

 public static MaterialPropertySet OfFire(FireRating reaction, EuroclassSuffix suffix, FireResistance resistance, PropertyEvidence evidence = default) =>
  new Fire(Option<FireRating>.Some(reaction), smoke, droplets, resistance, evidence);

 // Cells are characterization magnitudes per the declared Basis (domain bases, not SI dimensions), so the
 // Matrix slot guards ARITY-then-FINITE (dependent checks on one input bind inside the slot); EPD identity +
 // expiry ride the evidence argument, never per-case columns.
 public static Fin<MaterialPropertySet> OfEnvironmental(MeasurementBasis basis, ImmutableArray<double> impacts, Option<double> recycledContent, Option<double> endOfLifeRecovery, Op key, PropertyEvidence evidence = default) =>
  (Matrix(impacts, key),
   recycledContent.Match(
    Some: r => In(r, Band.Unit, "environmental-recycled-content", key).Map(Option<double>.Some),
    None: () => Validation<Error, Option<double>>.Success(Option<double>.None)),
   endOfLifeRecovery.Match(
    Some: r => In(r, Band.Unit, "environmental-recovery", key).Map(Option<double>.Some),
    None: () => Validation<Error, Option<double>>.Success(Option<double>.None)))
  .Apply((m, recycled, recovery) => (MaterialPropertySet)new Environmental(basis, m, recycled, recovery, evidence))
  .As().ToFin();

  // OfCost holds every cost column finite and non-negative — `IsFinite` rejects NaN/±∞ and `>= 0.0` the negative, per NAMED column,
 // all misses accumulated: a NaN or infinite per-unit cost would otherwise enter the content hash through the
 // raw-double columns the MeasureValue finiteness gate never sees, so the seam guards the cost columns the way it
 // guards a measure.
 public static Fin<MaterialPropertySet> OfCost(MeasurementBasis basis, Currency currency, double supply, double install, double lifecycle, Op key, PropertyEvidence evidence = default) =>
  (In(supply, Band.Nonnegative, "cost-supply", key),
   In(install, Band.Nonnegative, "cost-install", key),
   In(lifecycle, Band.Nonnegative, "cost-lifecycle", key))
  .Apply((s, i, l) => (MaterialPropertySet)new Cost(basis, currency, s, i, l, evidence))
  .As().ToFin();

  // OfDamping admits the dynamic-analysis columns: ζ the fraction-of-critical damping ratio in [0,1) (an at-or-over-critical material
 // datum is a datasheet error, not a material), the optional Rayleigh (α, β) pair finite and non-negative when Some
 // (a zero leg is a legitimate pure mass- or stiffness-proportional model, so non-negative not strictly-positive).
 public static Fin<MaterialPropertySet> OfDamping(double dampingRatio, Option<(double AlphaPerS, double BetaS)> rayleigh, Op key, PropertyEvidence evidence = default) =>
  (In(dampingRatio, Band.Fractional, "damping-ratio", key),
   Rayleigh(rayleigh, key))
  .Apply((zeta, pair) => (MaterialPropertySet)new Damping(zeta, pair, evidence))
  .As().ToFin();

  // Leaves accumulate; the wf >= w80 refinement and the curve↔anchor agreement bind AFTER them
  // (COMPOSITE_ADMISSION order).
 public static Fin<MaterialPropertySet> OfHygrothermal(double porosity, double waterContent80Rh, double freeWaterSaturation, Option<double> waterAbsorption, Op key, PropertyEvidence evidence = default,
  Option<SampledCurve> sorptionIsotherm = default, Option<SampledCurve> liquidTransport = default, Option<SampledCurve> moistureConductivity = default) =>
  (In(porosity, Band.Unit, "hygrothermal-porosity", key),
   PositiveSi(waterContent80Rh, QuantityType.Create("MoistureStorage"), Dimension.DensityDim, "hygrothermal-w80", key),
   PositiveSi(freeWaterSaturation, QuantityType.Create("MoistureStorage"), Dimension.DensityDim, "hygrothermal-free-saturation", key),
   Optional(waterAbsorption, Band.Positive, "hygrothermal-water-absorption", key))
  .Apply((phi, w80, wf, a) => (Phi: phi, W80: w80, Wf: wf, A: a))
  .As().ToFin()
  .Bind(t => t.Wf.Si < t.W80.Si
   ? Fin.Fail<MaterialPropertySet>(new ElementFault.ValueRejected(key, $"<hygrothermal-isotherm-inverted:w80={t.W80.Si:R}:wf={t.Wf.Si:R}>"))
   : Anchors(sorptionIsotherm, t.W80.Si, t.Wf.Si, key)
      .Map(_ => (MaterialPropertySet)new Hygrothermal(t.Phi, t.W80, t.Wf, t.A, sorptionIsotherm, liquidTransport, moistureConductivity, evidence)));

 // IsothermAnchorTolerance bounds the relative isotherm-anchor agreement the curve↔anchor refinement reads.
 private const double IsothermAnchorTolerance = 0.02;
 private static Fin<Unit> Anchors(Option<SampledCurve> curve, double w80, double wf, Op key) =>
  curve.Match(
   Some: sample => (sample.AtAdmitted(0.8, key), sample.AtAdmitted(1.0, key))
    .Apply((at80, at100) => (At80: at80, At100: at100)).As().ToFin()
    .Bind(at => Disagrees(at.At80, w80) || Disagrees(at.At100, wf)
     ? Fin.Fail<Unit>(new ElementFault.ValueRejected(key, "<hygrothermal-isotherm-anchor-mismatch>"))
     : Fin.Succ(unit)),
   None: () => Fin.Succ(unit));
 private static bool Disagrees(double curve, double anchor) => Math.Abs(curve - anchor) > IsothermAnchorTolerance * Math.Max(Math.Abs(anchor), 1.0);

  // OfDurability admits the fib Model Code service-life columns: K non-negative (0 = carbonation-immune), D_RCM strictly positive and
 // minted on the L²T⁻¹ chloride-diffusivity signature, the ageing exponent a [0,1] fraction.
 public static Fin<MaterialPropertySet> OfDurability(double carbonationRate, double chlorideDiffusion, double ageingExponent, Op key, PropertyEvidence evidence = default) =>
  (In(carbonationRate, Band.Nonnegative, "durability-carbonation-rate", key),
   PositiveSi(chlorideDiffusion, QuantityType.Create("ChlorideDiffusivity"), Dimension.Create(2, 0, -1, 0, 0, 0, 0), "durability-chloride-diffusion", key),
   In(ageingExponent, Band.Unit, "durability-ageing-exponent", key))
  .Apply((k, d, alpha) => (MaterialPropertySet)new Durability(k, d, alpha, evidence))
  .As().ToFin();

  // Nine in-unit leaves accumulate first; the six Conserves refinements run as the SECOND stage.
 public static Fin<MaterialPropertySet> OfOptical(double visibleTransmittance, double visibleReflectanceFront, double visibleReflectanceBack, double solarTransmittance, double solarReflectanceFront, double solarReflectanceBack, double thermalIrTransmittance, double thermalIrEmissivityFront, double thermalIrEmissivityBack, Op key, PropertyEvidence evidence = default) =>
  (In(visibleTransmittance, Band.Unit, "optical-visible-transmittance", key),
   In(visibleReflectanceFront, Band.Unit, "optical-visible-reflectance-front", key),
   In(visibleReflectanceBack, Band.Unit, "optical-visible-reflectance-back", key),
   In(solarTransmittance, Band.Unit, "optical-solar-transmittance", key),
   In(solarReflectanceFront, Band.Unit, "optical-solar-reflectance-front", key),
   In(solarReflectanceBack, Band.Unit, "optical-solar-reflectance-back", key),
   In(thermalIrTransmittance, Band.Unit, "optical-ir-transmittance", key),
   In(thermalIrEmissivityFront, Band.Unit, "optical-ir-emissivity-front", key),
   In(thermalIrEmissivityBack, Band.Unit, "optical-ir-emissivity-back", key))
  .Apply((tv, rvf, rvb, te, rsf, rsb, tir, ef, eb) => new Optical(tv, rvf, rvb, te, rsf, rsb, tir, ef, eb, evidence))
  .As().ToFin()
  .Bind(o =>
   (Conserves(o.VisibleTransmittance, o.VisibleReflectanceFront, "visible", "front", key),
    Conserves(o.VisibleTransmittance, o.VisibleReflectanceBack, "visible", "back", key),
    Conserves(o.SolarTransmittance, o.SolarReflectanceFront, "solar", "front", key),
    Conserves(o.SolarTransmittance, o.SolarReflectanceBack, "solar", "back", key),
    Conserves(o.ThermalIrTransmittance, o.ThermalIrEmissivityFront, "ir", "front", key),
    Conserves(o.ThermalIrTransmittance, o.ThermalIrEmissivityBack, "ir", "back", key))
   .Apply((_, _, _, _, _, _) => (MaterialPropertySet)o)
   .As().ToFin());

  // Resistivity through the registry admission; the registry-less DielectricStrength mints typed OfSi over
  // its composed dimension (V/m = [L·M·T⁻³·I⁻¹]).
 public static Fin<MaterialPropertySet> OfElectrical(double resistivityOhmM, double relativePermittivity, Option<double> dielectricStrengthVPerM, Option<double> magneticPermeabilityRelative, Op key, PropertyEvidence evidence = default) =>
  (Positive(MeasureValue.Of(resistivityOhmM, UnitsNet.Units.ElectricResistivityUnit.OhmMeter, key), "electrical-resistivity", key),
   Guarded(relativePermittivity is >= 1.0, relativePermittivity, "electrical-relative-permittivity-below-unity", key),
   dielectricStrengthVPerM.Match(
    Some: v => PositiveSi(v, QuantityType.Create("DielectricStrength"), Dimension.Create(1, 1, -3, -1, 0, 0, 0), "electrical-dielectric-strength", key).Map(Option<MeasureValue>.Some),
    None: () => Validation<Error, Option<MeasureValue>>.Success(Option<MeasureValue>.None)),
   Optional(magneticPermeabilityRelative, Band.Positive, "electrical-permeability", key))
  .Apply((rho, er, ds, mu) => (MaterialPropertySet)new Electrical(rho, er, ds, mu, evidence))
  .As().ToFin();

 // --- [ADMISSION_SLOTS]
 // Kernel admission slots own universal scalar bands; material combinators retain semantic refinements.
 private static Validation<Error, Unit> Conserves(double transmittance, double counterpart, string band, string side, Op key) =>
  transmittance + counterpart <= 1.0
   ? unit
   : new ElementFault.ValueRejected(key, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"<optical-{band}-{side}-conservation:{transmittance + counterpart:R}>"));

 // The ONE declared-unit coercion slot the raw arities compose (accumulating among themselves before delegation).
 private static Validation<Error, MeasureValue> Coerce(double value, Enum unit, Op key) =>
  MeasureValue.Of(value, unit, key).ToValidation();

 private static Validation<Error, double> Guarded(bool valid, double value, string name, Op key) =>
  valid ? value : new KernelFault.OutOfRange(name, value, "satisfy the declared scalar predicate", Some(key));

 private static Validation<Error, MeasureValue> Positive(Fin<MeasureValue> column, string name, Op key) =>
  column.Bind(m => m.Si > 0.0 ? Fin.Succ(m) : new KernelFault.OutOfRange(name, m.Si, "be positive", Some(key))).ToValidation();

 private static Validation<Error, MeasureValue> Positive(MeasureValue column, string name, Op key) =>
  Positive(Fin.Succ(column), name, key);

 private static Validation<Error, MeasureValue> PositiveSi(double value, QuantityType type, Dimension dimension, string name, Op key) =>
  double.IsFinite(value) && value > 0.0
   ? MeasureValue.OfSi(type, dimension, value).ToValidation()
   : new KernelFault.OutOfRange(name, value, "be finite and positive", Some(key));

 private static Validation<Error, Option<(double AlphaPerS, double BetaS)>> Rayleigh(Option<(double AlphaPerS, double BetaS)> pair, Op key) =>
  pair.Match(
   Some: r => (In(r.AlphaPerS, Band.Nonnegative, "damping-rayleigh-alpha", key),
    In(r.BetaS, Band.Nonnegative, "damping-rayleigh-beta", key)).Apply((alpha, beta) => Some((alpha, beta))).As(),
   None: () => Success<Error, Option<(double AlphaPerS, double BetaS)>>(None));

  // Matrix is the dependent slot: arity gates the finiteness scan of the SAME array (dependence binds inside one slot;
 // independence accumulates across slots — EXPRESSION_SPINE's carrier-selected algebra in one constructor).
 private static Validation<Error, ImmutableArray<double>> Matrix(ImmutableArray<double> impacts, Op key) =>
  impacts.IsDefaultOrEmpty || impacts.Length != Environmental.MatrixArity
   ? new ElementFault.ValueRejected(key, $"<environmental-impact-arity:{(impacts.IsDefault ? -1 : impacts.Length)}:expected={Environmental.MatrixArity}>")
   : Indexed(impacts.AsSpan(), double.IsFinite, key, "environmental-impact").Map(_ => impacts);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class MaterialPropertyAccess {
 extension(Seq<MaterialPropertySet> properties) {
  // ONE polymorphic typed read — the case TYPE is the discriminant (recoverable from the value per MODAL_ARITY); the
  // per-case reads below DERIVE from this one `Choose p is T` body (the repeated arms collapse to one), and a
  // case with no named read (a future discipline) is read generically via properties.Property<MaterialPropertySet.X>().
  private Option<T> Property<T>() where T : MaterialPropertySet =>
   properties.Choose(static p => p is T t ? Some(t) : None).Head;

  // Per-discipline reads the Rasm.Compute aggregator consumes (props.Thermal/Mechanical/Environmental) —
  // each a one-line projection of the generic owner, the consumer-contract surface, never a re-implemented Choose body.
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

  // The cross-case substance read over both stiffness carriers (Option coalesce of the two named forwards),
  // answering the TYPED MeasureValue so a fold composes Multiply/Scale on the carrier.
  public Option<MeasureValue> Density =>
   Mechanical.Map(static m => m.Density) | Orthotropic.Map(static o => o.Density);

 }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
