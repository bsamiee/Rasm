# [ELEMENT_MATERIAL]

The host-neutral material owner: the `MaterialId` `[ValueObject<string>]` a `Material` node keys on, one `MaterialComposition` `[Union]` closing the type-level material-set structure (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), and one `MaterialPropertySet` `[Union]` closing the typed engineering-property family (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`/`Damping`/`Hygrothermal`/`Durability`/`Optical`) keyed to the one `Classification/classification#DISCIPLINE_AXIS` `Discipline`. A material is a FULL engineering object: a `Material` node carries its composition and its property sets over one `MaterialId`, so a consumer reads a material's U-value, sound spectrum, fire REI rating, structural grade, seismic damping, moisture-storage curve, service-life diffusion data, solar-optical constants, embodied carbon, and cost from one node — never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface, never a per-discipline material type. Every multi-column admission ACCUMULATES: the `Of*` smart-constructors run each independent column as a `Validation<Error,_>` slot joined by the tuple `.Apply` and collapse `.ToFin()` once, so a bad datasheet reports every offending column in one `Fin.Fail` (`ManyErrors`), never first-fault-wins. This is the seam home the migration source's two parallel owners collapse onto: the `Rasm.Materials` `MaterialAssignment` trichotomy and its `MaterialProperty` unions become the seam `MaterialComposition` and `MaterialPropertySet`, the `Rasm.Materials` projector lowering its material subgraph onto `Material` nodes and the `Rasm.Bim` projector reading them. The occurrence usage binding (layer direction/offset, profile cardinal point) is NOT here — it rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, this owner carrying only the type-level SET structure. The page composes `Properties/quantity#MEASURE_VALUE` for every measured column (`MeasureValue.Of` SI coercion + the `Dimension` discriminator), `Composition/acoustic#ACOUSTIC_FOLDS` for the `Acoustic` case, and `Classification/classification#DISCIPLINE_AXIS` for the property-to-discipline key; a non-finite or out-of-range admission rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page references NO VividOrange — a `ProfileSet` carries a neutral `ProfileRef` PLUS the neutral `SectionProperties` the `Rasm.Materials` projector resolves ONE-HOP (M7) and BAKES on (`WithSection`), so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf`) without re-resolving or admitting VividOrange. The `SectionProperties` receipt carries the FULL structural-design and fire column set the `Rasm.Compute` design-code checks read off the seam (the AISC 360 / EN 1993 / AISI S100 / ACI 318 / NDS / TMS 402 flexure-shear-compression and the EN 1993-1-2 / EN 1992-1-2 fire routes) — a CONSUMER-CONTRACT-driven shape, the `Rasm.Materials` projector resolving the elastic columns from the VividOrange polygon solver and computing the plastic moduli / torsion constant / shear areas / shear-centre offsets / mono-symmetry factor the solver does not expose (the asymmetric-section columns the EN 1993-1-1 §6.3.2 general LTB route needs for a channel/tee/angle, zero on a doubly-symmetric section), never a per-element-type section receipt.

## [01]-[INDEX]

- [01]-[MATERIAL_COMPOSITION]: the `MaterialId` key, the `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) with the `Of`-prefixed admissions, the private-constructor admission gate on the invariant-bearing cases, the `MaterialLayer`/`MaterialConstituent` rows, the `ProfileRef` neutral section reference, and the consumer-shaped neutral `SectionProperties` the M7 bake stamps.
- [02]-[MATERIAL_PROPERTY]: the class-root `MaterialPropertySet` `[Union]` + `[Equatable]` (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`/`Damping`/`Hygrothermal`/`Durability`/`Optical`) keyed to `Discipline`, the `FireRating`/`SmokeClass`/`DropletClass` reaction vocabulary and the `FireResistance` REI criteria, the `ImpactCategory` EN 15804+A2 indicator vocabulary the `Environmental` case bands its `(ImpactCategory × LifecycleStage)` impact matrix over, the derived isotropic shear modulus (and the realized `Orthotropic` directional-stiffness case the `Rasm.Materials` `timber#TIMBER_FAMILY` lowers into), the EN 1998 `Damping` / EN 15026 `Hygrothermal` / fib Model Code `Durability` discipline carriers, the accumulating `Of` admissions over the `[ADMISSION_SLOTS]` combinators, and the one generic `Property<T>()` typed lookup the named per-discipline reads derive from.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialId` the `[ValueObject<string>]` material-identity key a `Material` node carries; `MaterialComposition` the `[Union]` type-level material-set structure; `MaterialLayer` the layer row (`MaterialId` + `Dimension`-length `Thickness` + name); `MaterialConstituent` the constituent row (`MaterialId` + category + fraction); `ProfileRef` the neutral section-profile reference (`Standard` + `Designation` + content key) a `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalogue; `SectionProperties` the neutral baked section receipt over `Properties/quantity#MEASURE_VALUE` columns.
- Cases: `Single` (one homogeneous `MaterialId` — `IfcMaterial`) · `LayerSet` (a `Seq<MaterialLayer>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`) · `ProfileSet` (one `MaterialId` per extruded `ProfileRef`, members — `IfcMaterialProfileSet`) · `ConstituentSet` (a `Seq<MaterialConstituent>` of fraction-weighted keyword-tagged components, composites — `IfcMaterialConstituentSet`); the closed IFC material-definition family (`IfcMaterialList` deprecated and never admitted), a composition selecting how the material resolves.
- Entry: `MaterialComposition.OfSingle(material)` and `OfProfileSet(material, profile)` are TOTAL constructors (no admission invariant — the `MaterialPropertySet.OfAcoustic`/`OfFire` total shape, never a `Fin` wrapper over a total op, never an `Op` key the body discards); `OfLayerSet(layers, key)` and `OfConstituentSet(constituents, key)` are `Fin<T>` admissions railing `ElementFault.ValueRejected` on an empty set, a non-positive layer thickness, or a constituent fraction set that does not normalize to unity within tolerance; the four are `Of`-prefixed so the factory name never collides with the same-named nested case type (the `MaterialPropertySet.Of*` convention — a bare `Single(...)` static method and a nested `Single` case are one declaration space, a compile collision). `Materials` projects the assigned `MaterialId` set, `PrimaryMaterial` the appearance/structural-default key, `TotalThickness` (a `LayerSet` read) the layer buildup depth.
- Auto: the invariant-bearing `LayerSet`/`ConstituentSet` cases carry a PRIVATE constructor and an internal `Seed` re-hydration escape (the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage.ProfileSet` and `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` admission shape), so the only public admission is the `Of` factory and an empty/degenerate set is UNREPRESENTABLE — `PrimaryMaterial`'s `OrderByDescending(...).First()` is then total, never a latent throw on an empty layer/constituent set that the prior public positional ctor admitted; `Materials` dispatches the generated `Switch` projecting the `MaterialId` set each case carries; `OfLayerSet` guards each `MaterialLayer.Thickness` positive (the SI metre magnitude of the `Properties/quantity#MEASURE_VALUE` length); `OfConstituentSet` guards each fraction finite and the fraction sum to one within tolerance so a composite mixture normalizes once at construction (the `Rasm.Compute` `AssemblyAggregator` rule-of-mixtures reads the normalized fractions and never re-guards them).
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<string>]`), Generator.Equals (`[Equatable]` the class-root `MaterialComposition` union's structural equality + the member diff, `[OrderedEquality]` the `Layers`/`Constituents` sequences, and the `[Equatable]` `MaterialLayer`/`MaterialConstituent`/`SectionProperties` rows the diff drills to `.Thickness`/`.Fraction`/`.<column>`), LanguageExt.Core (`Seq`/`Fin`/`Option`), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter` the `MaterialComposition.CanonicalBytes` projection + the `ProfileRef.Of` content key write through, `ContentAddress.Of` minting the profile key), `Rasm` (the kernel `Op` op-key).
- Growth: the IFC material-definition family is closed at four cases; a new layer attribute is one `MaterialLayer` column, a new constituent keyword one `MaterialConstituent` column; a new structural/fire `SectionProperties` column is one `MeasureValue` field the `Rasm.Materials` section resolver fills and a `Rasm.Compute` design-code check reads; never a fifth composition case, never a per-element-type composition, and never a per-element-type section receipt; a new section catalogue is one `ProfileRef.Standard` token the projector resolves, never a seam edit.
- Boundary: `MaterialComposition` is the ONE composition owner — a per-element-type composition class is the deleted form; the composition is the TYPE-LEVEL set structure only, the occurrence usage binding (`LayerSetUsage` direction/sense/offset, `ProfileSetUsage` cardinal-point/extent) riding the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, so a layer set's geometric usage never duplicates onto the composition; a `ProfileSet` carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section-property type — the seam references no VividOrange, the `Rasm.Materials` projector resolving the `ProfileRef` one-hop and BAKING the neutral `SectionProperties` (`WithSection`) so a structural consumer reads the resolved section once; the `SectionProperties` is the consumer-contract column set the `Rasm.Compute` design-code routes read (`Area`/`Iyy`/`Izz`/`J`/`Iw`/`Wely`/`Welz`/`Wply`/`Wplz`/`AvY`/`AvZ`/radii/`Depth`/`Width`/`HeatedPerimeter`/`AxisDistance`/`ShearCentreY`/`ShearCentreZ`/`MonosymmetryFactor`) — the seam carries the baked scalars, never a VividOrange type, and the projector computes the plastic moduli/torsion/warping/shear-area/asymmetry columns the VividOrange polygon solver does not expose (the `Iw` warping constant the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling routes require, never derivable from `J` alone, AND the `ShearCentreY`/`ShearCentreZ` shear-centre offsets + the `MonosymmetryFactor` β_y the EN 1993-1-1 §6.3.2 GENERAL LTB route requires for a channel/tee/angle — all zero for a doubly-symmetric section, so a PFC/tee is no longer the unbuckle-checkable thin slice the symmetric-only column set left); a `MaterialLayer.Thickness` is a `Properties/quantity#MEASURE_VALUE` `Dimension`-length-checked measure read SI-native through `.Si`, never a bare double; `MaterialComposition` is a CLASS-root `[Union]` + `[Equatable]` and the `MaterialLayer`/`MaterialConstituent`/`SectionProperties` rows are `[Equatable]` record structs so the `Rasm.Persistence` `StructuralMerge` drills a changed layer thickness / constituent fraction / section column to `Composition.Layers[i].Thickness` / `.Constituents[i].Fraction` / `.Section.<column>` rather than replacing the whole composition (the record-root opaque-leaf form is deleted); the composition serializes to the IFC 4.3 material-definition family at the `Rasm.Bim` boundary, host-neutral here.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using Generator.Equals;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class MaterialId {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();
 public static MaterialId Of(string value) => Create(value);   // the codebase .Of factory the cross-package catalogues key on
}

// A neutral section-profile reference: a catalogue designation (+ optional standard + content key) the
// Rasm.Materials projector resolves ONE-HOP (M7) to the neutral SectionProperties it BAKES onto the ProfileSet
// composition, so a Rasm.Compute structural consumer reads graph.SectionOf(member) without re-resolving or
// admitting VividOrange. The content key projects (standard, designation) through the ONE canonical codec
// (CanonicalWriter, the Projection/address writer), length-prefixed so two standards never collide on one designation;
// Of(designation) is the single-token form the Rasm.Materials section catalogue key composes, Of(standard, designation) the keyed form.
public readonly record struct ProfileRef(string Standard, string Designation, UInt128 ContentKey) {
 public static ProfileRef Of(string designation) => Of("", designation);
 public static ProfileRef Of(string standard, string designation) {
  CanonicalWriter w = new(0.0);
  w.String(standard).String(designation);
  return new(standard, designation, ContentAddress.Of(w.ToBytes().Span).Value);
 }
}

// The neutral section-property receipt the Rasm.Materials projector BAKES onto a ProfileSet composition (M7) — the
// FULL structural-design + fire column set the Rasm.Compute design-code checks read off the seam (a CONSUMER CONTRACT,
// not a source slice): Area, both-axis second moment of area (Iyy/Izz), St-Venant torsion constant (J), warping
// constant (Iw, the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling input the bare J cannot supply), both-axis
// elastic AND plastic section moduli (Wely/Welz, Wply/Wplz), both-axis shear areas (AvY/AvZ for the major- and
// minor-axis shear capacity Av·fy/√3), both-axis radii of gyration, the bounding Depth/Width, the fire-exposed
// HeatedPerimeter (the EN 1993-1-2 section factor Am/V = HeatedPerimeter/Area), and the EN 1992-1-2 reinforcement
// AxisDistance (cover from the exposed concrete face to the main-reinforcement centroid; Zero for steel/timber). Each
// is a dimensioned MeasureValue the Rasm.Materials projector stamps SI-native through the TYPED MeasureValue.OfSi(QuantityType,
// Dimension, _) overload (Area via OfSi(QuantityType.Area, Dimension.AreaDim, _); the m³ section moduli Wely/Welz/Wply/Wplz via
// OfSi(QuantityType.Create("SectionModulus"), Dimension.VolumeDim, _); the m⁴ second moments Iyy/Izz via
// OfSi(QuantityType.Create("SecondMomentOfArea"), Dimension.Create(4,0,0,0,0,0,0), _) and the torsion constant J via OfSi(QuantityType.Create("TorsionConstant"), Dimension.Create(4,0,0,0,0,0,0), _) — distinct quantity-types over the SAME m⁴ dimension so a torsion constant never reads as a second moment under the QuantityType discriminator; the m⁶ warping constant Iw via OfSi(QuantityType.Create("WarpingConstant"), Dimension.Create(6,0,0,0,0,0,0), _) over the m⁶ warping dimension; the lengths radii/Depth/Width/
// HeatedPerimeter/AxisDistance/ShearCentreY/ShearCentreZ via OfSi(QuantityType.Length, Dimension.LengthDim, _)) — each
// column TYPE-tagged so a QTO accessor resolves and a section modulus never collides with a volume by content key, never the
// dimension-anonymous 2-arg OfSi(Dimension, _). The four Create tokens (SectionModulus/SecondMomentOfArea/TorsionConstant/
// WarpingConstant) are CONSUMER-DOMAIN quantity-type names UnitsNet has no quantity for (it carries AreaMomentOfInertia, never
// these as distinct quantities) — minted through the OPEN Properties/quantity#DIMENSION QuantityType.Create the owner sanctions
// for an engineering quantity the registry lacks, so the discriminator is a consumer-minted domain NAME for these four (the
// Area/Length columns alone carrying a real UnitsNet QuantityInfo.Name); Dimension.Create(int x7) is the [ComplexValueObject]
// seven-exponent generated factory the same owner exposes for the L^4 (second-moment/torsion) and L^6 (warping) signatures the
// static dimension rows omit. The Rasm.Materials section resolver
// fills the elastic/inertia/radius/perimeter/bbox columns from the VividOrange polygon solver (Area/MomentOfInertiaYy,Zz/
// ElasticSectionModulusYy,Zz/RadiusOfGyrationYy,Zz/Perimeter/Extends) and COMPUTES the plastic moduli, torsion constant,
// warping constant, shear areas, and the asymmetric-section LTB columns the polygon solver does not expose — resolved ONCE
// ABOVE the seam so a Rasm.Compute structural/fire runner reads the section (ElementGraph.SectionOf) without re-resolving per
// call OR admitting VividOrange. The shear-centre offsets (ShearCentreY/ShearCentreZ — the centroid→shear-centre distance per
// axis) and the mono-symmetry factor (MonosymmetryFactor — the EN 1993-1-1 NCCI SN030 β_y / ψ parameter) are the
// non-doubly-symmetric LTB inputs the EN 1993-1-1 §6.3.2 general route requires for a channel (PFC, one offset), a tee (one
// offset), or an angle (both offsets): a doubly-symmetric I-section places its shear centre AT the centroid so all three read
// ZERO, never smuggling an asymmetry onto the symmetric baseline — the prior symmetric-only column set could not flexural-
// torsional-buckling-check a PFC/tee, the deleted thin slice the resolver closes by sourcing these from the section geometry.
// A CLASS-ROOT [Equatable] is NOT used (no discriminator, every field present) — the record-struct is [Equatable] so the
// Rasm.Persistence 3-way StructuralMerge drills a changed section column to Composition.Section.<column> rather than replacing
// the whole Option<SectionProperties>; the drill bottoms at the MeasureValue column (a native-equality record struct the
// Generator.Equals DefaultEqualityComparer compares atomically — the measure IS the merge leaf, Properties/quantity).
[Equatable]
public readonly partial record struct SectionProperties(
 MeasureValue Area, MeasureValue Iyy, MeasureValue Izz, MeasureValue J, MeasureValue Iw,
 MeasureValue Wely, MeasureValue Welz, MeasureValue Wply, MeasureValue Wplz,
 MeasureValue AvY, MeasureValue AvZ, MeasureValue RadiusOfGyrationMajor, MeasureValue RadiusOfGyrationMinor,
 MeasureValue Depth, MeasureValue Width, MeasureValue HeatedPerimeter, MeasureValue AxisDistance,
 MeasureValue ShearCentreY, MeasureValue ShearCentreZ, double MonosymmetryFactor) {
 // EN 1992-1-2 concrete-fire minimum cross-section dimension — derived min(Depth, Width), never a stored column
 // that could drift from the bounding extents the projector sources.
 public MeasureValue LeastDimension => Depth.Si <= Width.Si ? Depth : Width;
 // A doubly-symmetric section: the shear centre coincides with the centroid (both offsets zero) AND the mono-symmetry
 // factor vanishes — the predicate the EN 1993-1-1 §6.3.2 route reads to take the simplified (doubly-symmetric) LTB form
 // rather than the general one, derived from the offsets the resolver stamped, never a stored "isDoublySymmetric" flag.
 public bool IsDoublySymmetric => ShearCentreY.Si == 0.0 && ShearCentreZ.Si == 0.0 && MonosymmetryFactor == 0.0;
 public static readonly SectionProperties Zero = new(
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, 0.0);
}

// [Equatable] so the StructuralMerge drill reaches Composition.Layers[i].Thickness / Constituents[i].Fraction rather than
// stopping at the whole layer/constituent — the Graph/element#NODE_MODEL [STRUCTURAL_EQUALITY] mandate that every nested
// payload owner the diff descends carry [Equatable]; the drill bottoms at the MeasureValue field (Properties/quantity).
[Equatable]
public readonly partial record struct MaterialLayer(MaterialId Material, MeasureValue Thickness, string LayerName);

[Equatable]
public readonly partial record struct MaterialConstituent(MaterialId Material, string Category, double Fraction);

// --- [MODELS] -----------------------------------------------------------------------------
// Provenance is the ONE evidence carrier every property case rides — Source the ingress kind ("catalogue"/"epd"/
// "datasheet"), Reference the declaration identity (the EPD registration number, the datasheet id), ValidUntil the
// exact NodaTime LocalDate expiry the procurement/durability filter compares against — a coarse int YEAR was the
// deleted lossy form (the EC3 ingress carries a full expiry date the year truncation discarded).
public readonly record struct PropertyEvidence(string Source, string Reference, Option<LocalDate> ValidUntil) {
 public static readonly PropertyEvidence Catalogue = new("catalogue", "", Option<LocalDate>.None);

 public static PropertyEvidence Declaration(string source, string reference, LocalDate validUntil) =>
  new(source, reference, Some(validUntil));

 // Total over `default`: a defaulted evidence argument (null-string fields) normalizes to Catalogue, so the union
 // base ctor never stores a null Source/Reference and a blank source IS the curated-catalogue provenance.
 public PropertyEvidence Normalized() {
  string source = (Source ?? "").Trim();
  return string.IsNullOrWhiteSpace(source)
   ? Catalogue
   : new(source.ToLowerInvariant(), (Reference ?? "").Trim(), ValidUntil);
 }
}

// A CLASS-root [Union] + [Equatable] (the [GRAPH_FAMILY] form), NOT a record-root: a class-root union surrenders
// Thinktecture's record-generated equality, so structural equality AND the member-level structured diff ride Generator.Equals
// [Equatable] (never stacked on a record-root union). [Equatable] is LOAD-BEARING, not decorative: the Graph/element#NODE_MODEL
// Node.Material carries this MaterialComposition as a member, the Node.Material [Equatable] drill descends ONE [Equatable] link
// per hop, so a record-root MaterialComposition would be an opaque equality leaf and the Rasm.Persistence 3-way StructuralMerge
// would key Nodes[id].Composition WHOLE-composition (the deleted coarse form). As a class-root [Equatable] union with the
// Layers/Constituents [OrderedEquality]-marked Seq members (themselves [Equatable] MaterialLayer/MaterialConstituent, drilling
// to .Thickness/.Fraction) and the [Equatable] SectionProperties section, the drill localizes a changed layer thickness to
// Nodes[id].Composition.Layers[2].Thickness — the member granularity the RFC 6902 patch egress requires. The generated Switch/
// Map survive [Equatable] (only equality moves to Generator.Equals); a class-root case has NO `with`, so ProfileSet.With and
// WithSection RECONSTRUCT through the public positional ctor rather than a copy across the private-ctor type boundary.
[Union]
[Equatable]
public abstract partial class MaterialComposition {
 private MaterialComposition() { }

 // Single / ProfileSet carry no admission invariant — a public positional ctor is safe; the Of-prefixed factories
 // mirror them as TOTAL constructors so the four-factory family is named uniformly without a fake Fin rail.
 public sealed partial class Single(MaterialId material) : MaterialComposition { public MaterialId Material { get; } = material; }

 public sealed partial class ProfileSet(MaterialId material, ProfileRef profile, Option<SectionProperties> section) : MaterialComposition {
  public MaterialId Material { get; } = material;
  public ProfileRef Profile { get; } = profile;
  public Option<SectionProperties> Section { get; } = section;
  // The M7 section bake lives HERE: a class-root [Union] case has NO compiler-generated `with`, so With RECONSTRUCTS the
  // case through the public positional ctor (the base WithSection delegates here so the base never copies a case across the
  // type boundary). Section is the ONLY field that changes; the resolved neutral SectionProperties stamps once at projection.
  public ProfileSet With(SectionProperties section) => new(Material, Profile, Some(section));
 }

 // LayerSet / ConstituentSet carry admission invariants — a PRIVATE ctor + internal Seed re-hydration forces every
 // admission through the Of factory (the relation#MaterialUsage.ProfileSet / acoustic#Acoustic shape), so an empty
 // or degenerate set is unrepresentable and PrimaryMaterial's First() is total, never a latent throw. The Layers Seq
 // is ORDERED (an IfcMaterialLayerSet is an ordered buildup, layer 1 = the reference-line-side ply) so [OrderedEquality]
 // matches both the physical order semantics AND the stored-order CanonicalBytes iteration; Constituents likewise iterate
 // in stored order in CanonicalBytes, so [OrderedEquality] keeps equality aligned with the order-sensitive content key.
 public sealed partial class LayerSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialLayer> Layers { get; }
  private LayerSet(Seq<MaterialLayer> layers) => Layers = layers;
  internal static LayerSet Seed(Seq<MaterialLayer> layers) => new(layers);
  public double TotalThickness => Layers.Sum(static l => l.Thickness.Si);
 }

 public sealed partial class ConstituentSet : MaterialComposition {
  [property: OrderedEquality] public Seq<MaterialConstituent> Constituents { get; }
  private ConstituentSet(Seq<MaterialConstituent> constituents) => Constituents = constituents;
  internal static ConstituentSet Seed(Seq<MaterialConstituent> constituents) => new(constituents);
 }

 public Seq<MaterialId> Materials => Switch(
  single: static s => Seq(s.Material),
  layerSet: static s => s.Layers.Map(static l => l.Material),
  profileSet: static s => Seq(s.Material),
  constituentSet: static s => s.Constituents.Map(static c => c.Material));

 // The principal material an occurrence keys appearance/structural defaults on: the single/profile material,
 // the thickest layer, or the largest-fraction constituent — one MaterialId, never a parallel "primary" flag.
 // First() is total: the LayerSet/ConstituentSet private-ctor admission guarantees a non-empty set.
 public MaterialId PrimaryMaterial => Switch(
  single: static s => s.Material,
  layerSet: static s => s.Layers.OrderByDescending(static l => l.Thickness.Si).First().Material,
  profileSet: static s => s.Material,
  constituentSet: static s => s.Constituents.OrderByDescending(static c => c.Fraction).First().Material);

 // The M7 bake: the Rasm.Materials projector resolves the ProfileRef one-hop and stamps the neutral section onto
 // a ProfileSet composition, so a structural consumer reads it through graph.SectionOf without re-resolving.
 public MaterialComposition WithSection(SectionProperties section) =>
  this is ProfileSet ps ? ps.With(section) : this;

 // The canonical projection through the Projection/address#CONTENT_ADDRESS writer: case ordinal then
 // the typed set members in declared order, layer thicknesses quantized through MeasureValue.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  single: s => w.Ordinal(0).String(s.Material.Value),
  layerSet: s => { w.Ordinal(1).Ordinal(s.Layers.Count); foreach (var l in s.Layers) { w.String(l.Material.Value).Measure(l.Thickness).String(l.LayerName); } return w; },
  profileSet: s => w.Ordinal(2).String(s.Material.Value).String(s.Profile.Standard).String(s.Profile.Designation).U128(s.Profile.ContentKey),
  constituentSet: s => { w.Ordinal(3).Ordinal(s.Constituents.Count); foreach (var c in s.Constituents) { w.String(c.Material.Value).String(c.Category).Double(c.Fraction); } return w; });

 const double FractionTolerance = 1e-3;

 // Single / ProfileSet are TOTAL — no admission invariant, so no Fin and no Op key (the OfAcoustic/OfFire shape).
 public static MaterialComposition OfSingle(MaterialId material) => new Single(material);

 public static MaterialComposition OfProfileSet(MaterialId material, ProfileRef profile) =>
  new ProfileSet(material, profile, Option<SectionProperties>.None);

 // The two causes NAME themselves and can never co-fail (thickness is vacuous over an empty set), so the
 // dependency-headed chain IS the accumulating disposition; a merged two-cause fault token is the deleted
 // anonymous form.
 public static Fin<MaterialComposition> OfLayerSet(Seq<MaterialLayer> layers, Op key) =>
  layers.IsEmpty
   ? ElementFault.ValueRejected(key, "<layer-set-empty>")
   : layers.Exists(static l => l.Thickness.Si <= 0.0)
    ? ElementFault.ValueRejected(key, "<layer-thickness-non-positive>")
    : Fin.Succ<MaterialComposition>(LayerSet.Seed(layers));

 // A constituent set is a composite MIXTURE: the fractions are proportions of the whole, so they normalize to one
 // within tolerance (the Rasm.Compute rule-of-mixtures fold reads them normalized and never re-guards) — a set that
 // does not sum to unity is a malformed mixture the admission rejects, the seam carrying only valid composites.
 public static Fin<MaterialComposition> OfConstituentSet(Seq<MaterialConstituent> constituents, Op key) =>
  constituents.IsEmpty
   ? ElementFault.ValueRejected(key, "<constituent-set-empty>")
   : constituents.Exists(static c => !double.IsFinite(c.Fraction) || c.Fraction is < 0.0 or > 1.0)
    ? ElementFault.ValueRejected(key, "<constituent-fraction-out-of-unit>")
    : Math.Abs(constituents.Sum(static c => c.Fraction) - 1.0) > FractionTolerance
     ? ElementFault.ValueRejected(key, $"<constituent-fraction-not-normalized:{constituents.Sum(static c => c.Fraction):R}>")
     : Fin.Succ<MaterialComposition>(ConstituentSet.Seed(constituents));
}
```

## [03]-[MATERIAL_PROPERTY]

- Owner: `MaterialPropertySet` the `[Union]` typed engineering-property family keyed to `Discipline`; `FireRating` the `[SmartEnum<string>]` reaction-to-fire class with `SmokeClass`/`DropletClass` the EN 13501-1 sub-classifications; `FireResistance` the EN 13501-2 R/E/I criteria; the `Of` admissions coercing each measured column through `Properties/quantity#MEASURE_VALUE`.
- Cases: `Mechanical` (density / Young's modulus / yield strength / ultimate strength as `MeasureValue`, Poisson's ratio + thermal-expansion as guarded dimensionless doubles, the isotropic shear modulus DERIVED `G = E/(2(1+ν))` — `Discipline.Structural`) · `Orthotropic` (density / the two principal moduli `E1∥`/`E2⊥` / the INDEPENDENT measured shear modulus `G` / the two principal strengths as `MeasureValue` + thermal-expansion — the directional-stiffness carrier the isotropic `Mechanical` structurally cannot model, the `Rasm.Materials` `timber#TIMBER_FAMILY` consumer's seam home, also `Discipline.Structural` so the case TYPE discriminates an isotropic from a directional material) · `Thermal` (conductivity / specific heat / U-value as `MeasureValue` + vapour-resistance factor μ as a guarded dimensionless double for EN 13788 Glaser condensation — `Discipline.Thermal`) · `Acoustic` (the `Composition/acoustic#ACOUSTIC_FOLDS` banded carrier with its `DynamicStiffnessMNPerM3`/`FlowResistivityPaSPerM2`/`LossFactor` intrinsic constants forwarded — `Discipline.Acoustic`) · `Fire` (a `FireRating` reaction class + `SmokeClass`/`DropletClass` sub-class + a `FireResistance` R/E/I rating — `Discipline.Fire`) · `Environmental` (a `MeasurementBasis` declared unit + the EN 15804+A2 `(ImpactCategory × LifecycleStage)` row-major flat `Impacts` matrix that is the ONE impact store + `RecycledContent`/`EndOfLifeRecovery` fractions + EPD provenance, with the general `IndicatorAt(category, stage)` cell read and the `WholeLife(category)` cross-stage fold, plus the carbon-keyed convenience projections `Gwp` (the DERIVED cradle-to-gate `(GwpTotal, A1A3)` cell, never a parallel stored scalar), `StageAt`, `WholeLifeGwp`, and the DERIVED `StageGwp` per-module GwpTotal-row vector (the `[A1A3..D]` carbon row sliced from the matrix via `IndicatorAt(GwpTotal, stage)`, never a parallel stored 6-vector, the `Rasm.Compute` carbon fold reads one-hop) over them — `Discipline.Environmental`) · `Cost` (supply / install / lifecycle per-unit columns over a `Currency` + `MeasurementBasis` — `Discipline.Cost`) · `Damping` (the EN 1998-1 fraction-of-critical `DampingRatio` ζ + the optional per-material Rayleigh `(α, β)` proportional-damping pair a time-history FE model reads + the DERIVED hysteretic `StructuralLossFactor = 2ζ` — `Discipline.Dynamic`) · `Hygrothermal` (the EN 15026/WUFI transient-simulation inputs the steady-state `Thermal` case cannot model: `Porosity`, the `WaterContent80Rh`/`FreeWaterSaturation` sorption-isotherm anchors as MoistureStorage-typed kg/m³ measures, the optional capillary `WaterAbsorptionKgPerM2SqrtS` A-value — `Discipline.Hygrothermal`) · `Durability` (the fib Model Code service-life inputs: `CarbonationRateMmPerSqrtYear` K, the `ChlorideDiffusion` D_RCM as a ChlorideDiffusivity-typed m²/s measure, the `AgeingExponent` decay fraction — `Discipline.Durability`) · `Optical` (the IFC `Pset_MaterialOptical` / EnergyPlus `WindowMaterial:Glazing` solar-optical record: per-band transmittance plus side-asymmetric front/back reflectances for the visible and solar bands, thermal-IR transmittance plus front/back hemispherical emissivities, the band absorptances DERIVED conservation remainders — `Discipline.Energy`); a property is a `MaterialPropertySet` case over a `MaterialId`, never a property subtype, and a single-indicator GWP-only environmental model is the deleted 1-of-13 slice of the EN 15804+A2 indicator family.
- Entry: `MaterialPropertySet.OfMechanical(density, youngsModulus, yieldStrength, ultimateStrength, poissons, thermalExpansion, key)` / `OfOrthotropic(density, e1Parallel, e2Perpendicular, shearModulus, strength1Parallel, strength2Perpendicular, thermalExpansion, key)` / `OfThermal(conductivity, specificHeat, uValue, vapourResistanceFactor, key)` / `OfAcoustic(acoustic)` / `OfFire(rating, resistance)` (+ the full `OfFire(rating, smoke, droplets, resistance)`) / `OfEnvironmental(basis, impacts, recycledContent, endOfLifeRecovery, key)` (the `impacts` an `ImmutableArray<double>` of arity `ImpactCategory.Count × LifecycleStage.Count`; EPD identity + `LocalDate` expiry ride the `evidence` argument as `PropertyEvidence.Declaration("epd", id, validUntil)`, never per-case columns) / `OfCost(currency, basis, supply, install, lifecycle, key)` / `OfDamping(dampingRatio, rayleigh, key)` / `OfHygrothermal(porosity, waterContent80Rh, freeWaterSaturation, waterAbsorption, key)` / `OfDurability(carbonationRate, chlorideDiffusion, ageingExponent, key)` / `OfOptical(visibleTransmittance, visibleReflectanceFront, visibleReflectanceBack, solarTransmittance, solarReflectanceFront, solarReflectanceBack, thermalIrTransmittance, thermalIrEmissivityFront, thermalIrEmissivityBack, key)` — the typed smart-constructors coercing each measured column through `MeasureValue.Of`/`MeasureValue.OfSi` to its SI base and guarding the dimensionless ratios, every multi-column form an ACCUMULATING admission (each independent column one `Validation<Error,_>` `[ADMISSION_SLOTS]` combinator, the tuple `.Apply` unioning every `ElementFault.ValueRejected` through `Error.Combine`/`ManyErrors`, `.ToFin()` collapsing once at the seam return — the public rail stays `Fin<T>`, so consumers are untouched while a bad datasheet reports ALL offending columns; the total `OfAcoustic`/`OfFire` carry no invariant and return the bare case; `OfHygrothermal` binds its `wf >= w80` isotherm refinement AFTER the accumulated leaves, and `OfOptical` accumulates its six per-band-per-side `τ + ρ <= 1`/`τIR + ε <= 1` conservation refinements as a SECOND stage after the nine in-unit leaves, the COMPOSITE_ADMISSION order); `Discipline` reads the case-to-discipline map; the generic `Property<T>()` is the one polymorphic typed lookup, the named per-discipline reads (`Mechanical`/`Orthotropic`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) deriving from it as the ergonomic consumer surface the `Rasm.Compute` aggregator reads (`props.Thermal`/`props.Mechanical`/`props.Environmental`), and `ForDiscipline(discipline)` the dual by-discipline-value read.
- Auto: `MaterialPropertySet` is a CLASS-root `[Union]` + `[Equatable]` (the `[GRAPH_FAMILY]` form), so the generated `Switch`/`Map` survive while structural equality and the member diff ride `Generator.Equals` — the `Graph/element#NODE_MODEL` `Node.Material` `[Equatable]` drill descends into each case's columns (a record-root case would be an opaque equality leaf collapsing the `Rasm.Persistence` `StructuralMerge` to whole-property replacement); `Discipline` dispatches the generated `Switch` mapping each case to its row (`Mechanical`/`Orthotropic`→`Structural`, `Damping`→`Dynamic`, `Hygrothermal`→`Hygrothermal`, `Durability`→`Durability`, `Optical`→`Energy`, …, `Cost`→`Cost`); the `Of` constructors route each dimensioned value through `MeasureValue.Of(value, UnitsNet.Units.X, key)` (or the TYPED `OfSi` for the registry-less MoistureStorage/ChlorideDiffusivity signatures) so the column carries its SI base and `Dimension`, the Poisson's ratio guarded to the physical isotropic `[0,0.5]` range (the `is >= 0.0 and <= 0.5` relational pattern rejecting an out-of-range ratio AND a `NaN`), every density/stiffness/strength/conductivity column guarded finite-AND-strictly-positive through the per-column `Positive` slot (a negative MPa is finite, so the `MeasureValue.Of` finiteness gate alone admits a physically-impossible negative-stiffness material the seam rejects BY NAME), the dimensionless ratios and the `MeasurementBasis`-relative fractions guarded finite-and-in-unit through the same NaN-rejecting relational patterns, and the raw-double cost columns guarded finite-and-non-negative (the `MeasureValue` finiteness gate never sees the raw-double `Cost`/`Environmental`-fraction carriers, so a `NaN`/∞ that a bare `< 0.0` guard would admit into the content hash is rejected at admission) — every such miss ACCUMULATED across the constructor's independent slots, never first-fault-wins; the `Mechanical` shear modulus is a DERIVED read off `E` and `ν` (the isotropic relation `G = E/(2(1+ν))`), never a stored column that could drift; the `Acoustic` case wraps the `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` carrier whose `Nrc`/`Saa`/`StcWeighted` are derived reads; the `Fire` case carries the EN 13501-1 reaction class plus its smoke/droplet sub-class and the EN 13501-2 R/E/I `FireResistance`; the `Environmental` case stores the EN 15804+A2 impact matrix row-major flat and `OfEnvironmental` guards its `Environmental.MatrixArity` and finiteness once so the derived `IndicatorAt`/`Gwp`/`WholeLife` reads trust the admission.
- Receipt: a `Seq<MaterialPropertySet>` on a `Material` node is the full engineering profile a `Bake`-derived `Element` reads flat — `props.Thermal.Map(t => t.UValue)`, `props.Mechanical.Map(m => m.YieldStrength)`, `props.Acoustic.Map(a => a.StcWeighted)`, `props.Damping.Map(d => d.DampingRatio)`, `props.Durability.Map(u => u.ChlorideDiffusion)`, or the generic `props.Property<T>()` for a future case before its named forward lands — one node carrying every discipline keyed by `Discipline`; the `Rasm.Compute` analysis route reads the `MeasureValue` columns by `Discipline`, and the assembly aggregation (series-resistance U-value, rule-of-mixtures density, layered STC) folds the `MaterialComposition` plies in Compute, never re-keyed per assembly.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`/`[ValueObject<string>]`), Generator.Equals (`[Equatable]` the class-root `MaterialPropertySet` union's structural equality + the member diff the `Rasm.Persistence` `StructuralMerge` drills, `[OrderedEquality]` the `Environmental.Impacts` matrix), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Validation<Error,_>` the accumulating admission slots joined by the tuple `.Apply` and collapsed `.ToFin()`/`Choose`/`Find`), NodaTime (`LocalDate` the `PropertyEvidence.ValidUntil` calendar expiry — the exact EPD/declaration date the procurement filter compares, over the deleted lossy int-year), UnitsNet (via `MeasureValue`), System.Collections.Immutable (`ImmutableArray<double>` the immutable impact-matrix store), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter` the `MaterialPropertySet.CanonicalBytes` content projection writes through), `Rasm` (the kernel `Op` op-key).
- Growth: a new engineering property shared across materials is one column on its `MaterialPropertySet` case; a new property discipline with no fit is one `MaterialPropertySet` case carrying its `Discipline` — never a parallel `Eco`/`Cost` owner (the `Damping`/`Hygrothermal`/`Durability`/`Optical` cases are this law EXECUTED: each one case + one `Discipline` row + one next-free `CanonicalBytes` ordinal + one named forward, zero new surfaces beside the union); a new fire-reaction class is one `FireRating` row, a new acoustic rating one fold on the `Acoustic` carrier, a new EN 15804 environmental indicator one `ImpactCategory` row (the `Impacts` matrix widens by one indicator row and `IndicatorAt`/`WholeLife` read it with no new column or method); a new admission invariant is one `[ADMISSION_SLOTS]` combinator slot, never a per-constructor guard chain; the family grows by case, column, and vocabulary row, never by a per-discipline material type, and the typed lookup grows by ONE generic `Property<T>()` over the case type plus an ergonomic named forward, never a per-case roster of independent `Choose` bodies.
- Boundary: `MaterialPropertySet` is the ONE typed property family — a `StructuralMaterial`/`ThermalMaterial` per-discipline material type is the deleted form, a property being a case over a `MaterialId`; every dimensioned measured column admits through `MeasureValue.Of` to its SI base (the `Density`/`YoungsModulus`/`Conductivity`/`UValue` columns), never a bare double, the dimensionless `PoissonsRatio` guarded to the isotropic `[0,0.5]` so an out-of-range ratio is unrepresentable; the `Mechanical` shear modulus is DERIVED from `E`/`ν` (DERIVED_LOGIC — a stored `G` independent of `E`/`ν` on the ISOTROPIC owner is the named drift defect), and a material whose `G` IS an independent measured datum (timber along/across grain) carries it on the distinct `Orthotropic` case (its `E1∥`/`E2⊥`/`G` all stored, no Poisson derivation), never smuggled onto the isotropic `Mechanical` as a stored field; the `FireRating` is a closed reaction-class vocabulary (a non-standard class is a row never a free string) and fire resistance is a typed R/E/I `FireResistance` (a single resistance scalar cannot distinguish a load-bearing `R 90` column from a separating `EI 60` wall — the deleted form); the typed read is the one generic `Property<T>()` over the case type (the eleven near-identical per-case reads DERIVE from it — one `Choose p is T` body, never eleven — yet stay as the consumer-contract ergonomic surface the `Rasm.Compute` aggregator reads), `ForDiscipline` the dual by-discipline-value read; the `Acoustic` case is the banded `Composition/acoustic#ACOUSTIC_FOLDS` carrier, never a scalar STC; the per-case-to-`Discipline` map is the one correspondence the `Assessment/assessment#ASSESSMENT_NODE` and `Rasm.Compute` analysis route share; the typed columns lower at the `Rasm.Bim` egress into the ONE neutral detail schema `Properties/property#PROPERTY_BAG` declares over the canonical `PropertyName` vocabulary, the seam carrying NO `Pset_Material*` IFC name — `Rasm.Bim` maps the neutral detail schema to the IFC material property sets at `Emit`, the `MaterialPropertySet` case the seam-native store and the neutral bag the egress projection (never an element-level `PropertyBag` on the `Material` node); the `Cost` case carries neutral per-unit doubles over a `Currency` `[ValueObject<string>]` (an OPAQUE ISO 4217 alpha-3 token the seam shape-validates but never rosters — the SAME neutrality `AnalysisRoute`/`Classification.System` hold, a closed three-row currency enum being the deleted naive slice) + a `MeasurementBasis` declared unit (the seam references no money library — the `Rasm.Bim` `NodaMoney` cost algebra owns the ISO 4217 roster and meets the per-unit double at the quantity×rate join), and the `Environmental` case carries the EN 15804+A2 `(ImpactCategory × LifecycleStage)` row-major flat `Impacts` matrix (the thirteen core impact indicators — `GwpTotal`/`GwpFossil`/`GwpBiogenic`/`GwpLuluc`/`Odp`/`Ap`/`EpFreshwater`/`EpMarine`/`EpTerrestrial`/`Pocp`/`AdpMinerals`/`AdpFossil`/`Wdp` — over the EN 15978 A1-A3/A4/A5/B/C/D stage modules) as the ONE impact store — a GWP-only single-vector store was the deleted 1-of-13 slice, and a `Map<ImpactCategory, ImmutableArray<double>>` is the deleted form because Generator.Equals would key its dictionary-value arrays by REFERENCE not content; the general read is `IndicatorAt(category, stage)` and the cradle-to-gate `Gwp` is the DERIVED convenience `IndicatorAt(GwpTotal, A1A3)`, a parallel stored `GlobalWarmingPotential` `MeasureValue` beside that cell the named double-store defect (DERIVED_LOGIC — the same one-owner discipline the `Acoustic` carrier holds, every rating derived from the spectrum), so `OfEnvironmental` guards the matrix arity (`Environmental.MatrixArity`) and FINITE once (`AllFinite` over `Impacts.AsSpan()`) and the derived `IndicatorAt`/`Gwp`/`WholeLife`/`WholeLifeGwp` reads then trust the admission; `Impacts` is an `ImmutableArray<double>` not a `ReadOnlyMemory<double>` because the immutable owner forbids the post-admission mutable-aliasing a memory-over-array admits AND is `IEnumerable<double>` so `[OrderedEquality]` gives content (not reference) equality and the `Rasm.Persistence` `StructuralMerge` drills a changed cell to `Properties[i].Impacts[k]`; an EPD declaring fewer indicators (the carbon-only EC3 ingress) zeroes the un-declared indicator rows so the matrix arity is invariant and the `Rasm.Compute` EC3 embodied-carbon route reads `WholeLife(GwpTotal)`; every `Impacts` cell is on the case's `MeasurementBasis` (per-m³/per-m²/per-kg/per-item) — the SAME basis axis the `Cost` case carries, so the `Rasm.Compute` `AggregateEnvironmental` fold scales each ply by the basis-matching element quantity through the SAME basis-aware `DeclaredQuantity` derivation the cost fold uses, never a forced per-m³ normalization that demanded a density at ingress and dropped an area/item EPD; provenance is SINGLE-stored on the base `PropertyEvidence` (`Source`/`Reference`/`ValidUntil` `LocalDate`) — the prior `Environmental.Epd`/`ValidUntilYear` per-case columns were a double-store of the evidence concept the Bim egress dodged with a suppression flag, and the int YEAR was lossy against the EC3 declaration's full expiry date; the `Damping.DampingRatio` is the LARGE-strain design ζ (EN 1998 response spectrum) and never derives from or duplicates the `Composition/acoustic#ACOUSTIC_FOLDS` small-strain `LossFactor` η — different measurement standards at amplitude regimes orders of magnitude apart, so the two are independent measured data while `Damping.StructuralLossFactor = 2ζ` is the case's OWN derived FE input; the `Optical` case is the ENGINEERING solar-optical record (the IFC `Pset_MaterialOptical` member of the material property-set family, the EN 410 / EnergyPlus `WindowMaterial:Glazing` column set) — nine `[0,1]` spectral-average fractions with side-asymmetric front/back reflectances and emissivities (a coated pane is directional), the per-band absorptances DERIVED conservation remainders (`SolarAbsorptanceFront = 1 − τe − ρe,f`, the EnergyPlus opaque-surface α and the g-value secondary-heat input; a stored absorptance beside its τ/ρ pair is the double-store defect), the `OfOptical` admission enforcing `τ + ρ <= 1` per band and side and `τIR + ε <= 1` per side (Kirchhoff) so an unphysical datasheet is unrepresentable — the RENDER appearance (BSDF, texture) stays the `Rasm.Materials` Appearance owner's, this case carrying only the measured engineering constants the energy/daylight routes read; the fractional-exponent quantities (`CarbonationRateMmPerSqrtYear` mm/√year, `WaterAbsorptionKgPerM2SqrtS` kg/(m²·√s)) are raw doubles with the declared unit in the NAME because their √t dimension is inexpressible in the integer `Dimension` 7-vector (the `ThermalExpansionPerK` precedent), while every integer-dimension column stays a typed `MeasureValue` — the MoistureStorage/ChlorideDiffusivity `QuantityType.Create` mints keep a water content from reading as a bulk density and a diffusion coefficient from reading as a kinematic viscosity under the discriminator (the `SectionProperties` same-dimension-distinct-type discipline).

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
  TryGet(reaction, out FireRating? rating) && rating is { } r ? Fin.Succ(r) : ElementFault.ValueRejected(key, $"<fire-reaction-unknown:{reaction}>");
}

// The EN 13501-1 additional smoke-production class (s1 least … s3 most) — a Euroclass reaction rating is incomplete
// without it (a "B" is reported "B-s1,d0"); NotSpecified carries an unclassified product.
[SmartEnum<string>]
public sealed partial class SmokeClass {
 public static readonly SmokeClass NotSpecified = new("");
 public static readonly SmokeClass S1 = new("s1");
 public static readonly SmokeClass S2 = new("s2");
 public static readonly SmokeClass S3 = new("s3");
}

// The EN 13501-1 additional flaming-droplet/particle class (d0 none … d2 most).
[SmartEnum<string>]
public sealed partial class DropletClass {
 public static readonly DropletClass NotSpecified = new("");
 public static readonly DropletClass D0 = new("d0");
 public static readonly DropletClass D1 = new("d1");
 public static readonly DropletClass D2 = new("d2");
}

// The EN 13501-2 fire-resistance criteria in minutes — R load-bearing capacity, E integrity, I insulation, each rated
// independently (a column is "R 90", a separating wall "EI 60", a load-bearing wall "REI 90"); a single resistance
// scalar cannot distinguish them, so the three criteria are first-class. Zero on a criterion = not classified for it.
public readonly record struct FireResistance(int LoadBearingMinutes, int IntegrityMinutes, int InsulationMinutes) {
 public static readonly FireResistance None = default;
 public static FireResistance Rei(int minutes) => new(minutes, minutes, minutes);
 public static FireResistance R(int minutes) => new(minutes, 0, 0);
 public static FireResistance Ei(int minutes) => new(0, minutes, minutes);
}

// The EN 15978 lifecycle-stage modules the Environmental case bands the COLUMN axis of its (ImpactCategory × LifecycleStage)
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

// The unit-cost currency the Cost case carries — an OPAQUE ISO 4217 alpha-3 token (the SAME neutrality the
// Assessment/assessment#ASSESSMENT_NODE AnalysisRoute and Classification/classification#CLASSIFICATION_AXIS
// Classification.System tokens hold), NOT a closed seam roster: the currency roster (180+ active codes plus the
// historic/crypto long tail) is owned by the Rasm.Bim NodaMoney cost algebra, so the seam admits ANY well-formed
// alpha-3 code and a JPY/CHF/CNY cost needs NO seam edit — a closed three-row enum was the naive slice that forced a
// seam edit per currency. Shape (alpha-3) is the seam invariant the ValidateFactoryArguments hook enforces; ISO 4217
// MEMBERSHIP is NodaMoney's, the SAME shape-vs-roster split Classification.System (code shape vs standards roster) holds.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Currency {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim().ToUpperInvariant();
  if (value.Length != 3 || !value.All(char.IsAsciiLetterUpper)) { validationError = new ValidationError("currency requires an ISO 4217 alpha-3 code"); }
 }
 // The seam-rail admission a wire/catalog token takes — the FireRating.Parse/Discipline.Parse-consistent form
 // railing ElementFault.ValueRejected on a malformed (non-alpha-3) code; the ISO 4217 roster check is NodaMoney's.
 public static Fin<Currency> Parse(string code, Op key) =>
  TryCreate(code, out Currency? c) && c is { } v ? Fin.Succ(v) : ElementFault.ValueRejected(key, $"<currency-malformed:{code}>");
}

// The declared-unit basis the Cost AND Environmental cases share — a genuinely CLOSED four-set (per mass/area/
// volume/item), so here a non-standard basis IS a row, never a free string (unlike the OPEN Currency above whose
// roster lives downstream): the four bases are the complete declared-unit closure, never a downstream-owned roster.
[SmartEnum<string>]
public sealed partial class MeasurementBasis {
 public static readonly MeasurementBasis PerKg = new("per-kg");
 public static readonly MeasurementBasis PerM2 = new("per-m2");
 public static readonly MeasurementBasis PerM3 = new("per-m3");
 public static readonly MeasurementBasis PerItem = new("per-item");
 public static Fin<MeasurementBasis> Parse(string token, Op key) =>
  TryGet(token, out MeasurementBasis? b) && b is { } v ? Fin.Succ(v) : ElementFault.ValueRejected(key, $"<measurement-basis-unknown:{token}>");
}

// The EN 15804+A2 environmental-impact indicator the Environmental case bands the ROW axis of its (ImpactCategory ×
// LifecycleStage) Impacts matrix over — the CORE indicator set an EN 15804+A2 EPD declares, NOT a GWP-only slice: modeling
// carbon alone where the governing standard mandates ozone-depletion, acidification, eutrophication, photochemical-ozone, and
// abiotic-depletion indicators is the naive 1-of-13 slice this vocabulary closes. Keyed on the STABLE matrix-row INDEX (so
// the flat row-major Impacts vector offset is indicator.Index * LifecycleStage.Count + stage.Index and a new indicator
// appended takes the next free index without shifting an already-projected EPD's cells — the SAME stable-ordinal discipline
// the property-case CanonicalBytes ordinals hold), carrying the EPD wire Name token and its declared characterization Unit as
// columns. GwpTotal is the headline the intrinsic Gwp read keys on; the GwpFossil/GwpBiogenic/GwpLuluc split is the EN
// 15804+A2 sub-decomposition a fuller carbon report reads. Count is the row arity the OfEnvironmental matrix admission checks.
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

 // The EPD wire token is the Name (not the int matrix-row Key), so the wire admission resolves by Name through a lazy
 // ordinal-insensitive frozen index projected from Items (the [LOOKUP_LIFECYCLE] derived-index-through-accessor pattern,
 // O(1) steady-state, materialized on first Parse), the FireRating.Parse-consistent rail railing ElementFault.ValueRejected
 // on an unknown indicator name; the int Key stays the internal matrix-row offset Index reads O(1) on the hot IndicatorAt path.
 static readonly Lazy<FrozenDictionary<string, ImpactCategory>> ByName =
  new(static () => Items.ToFrozenDictionary(static c => c.Name, StringComparer.OrdinalIgnoreCase), LazyThreadSafetyMode.ExecutionAndPublication);

 public static Fin<ImpactCategory> Parse(string name, Op key) =>
  ByName.Value.TryGetValue(name, out ImpactCategory? c) && c is { } v ? Fin.Succ(v) : ElementFault.ValueRejected(key, $"<impact-category-unknown:{name}>");
}

// --- [MODELS] -----------------------------------------------------------------------------
// A CLASS-root [Union] + [Equatable] (the [GRAPH_FAMILY] form), NOT a record-root: a class-root union surrenders
// Thinktecture's record-generated equality, so structural equality AND the member-level diff ride Generator.Equals [Equatable]
// (never stacked on a record-root union). LOAD-BEARING per the Graph/element#NODE_MODEL [STRUCTURAL_EQUALITY] mandate: the
// Node.Material [UnorderedEquality] Seq<MaterialPropertySet> Properties member's element compare DRILLS only into an [Equatable]
// element, so a record-root case would key Nodes[id].Properties[i] WHOLE-property in the Rasm.Persistence 3-way StructuralMerge
// (the deleted coarse form) where a class-root [Equatable] case localizes a changed yield strength / U-value / one impact-stage
// cell to Nodes[id].Properties[i].<column> — the RFC 6902 patch member granularity. The scalar MeasureValue/double columns are
// the ATOMIC value-equality LEAVES the drill BOTTOMS at — a MeasureValue is a native-equality readonly record struct
// (Properties/quantity) the Generator.Equals DefaultEqualityComparer compares atomically by its record value-equality (the
// column IS the leaf at Nodes[id].Properties[i].<column>, never a descent into .<column>.Si — it carries no [Equatable] and
// needs none, adding it would redundantly re-derive the field compare the record already gives); the one IEnumerable-shaped
// member (the Environmental Impacts ImmutableArray<double> — an ImmutableArray IS IEnumerable<double> where a
// ReadOnlyMemory<double> is NOT) carries [OrderedEquality] for content equality, NOT the reference equality a bare
// ReadOnlyMemory<double> member would take.
// The generated Switch/Map survive [Equatable]; a class-root case has NO `with`.
[Union]
[Equatable]
public abstract partial class MaterialPropertySet {
 private MaterialPropertySet(PropertyEvidence evidence) {
  Evidence = evidence.Normalized();
 }

 public PropertyEvidence Evidence { get; }

 public sealed partial class Mechanical(MeasureValue density, MeasureValue youngsModulus, MeasureValue yieldStrength, MeasureValue ultimateStrength, double poissonsRatio, double thermalExpansionPerK, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasureValue Density { get; } = density;
  public MeasureValue YoungsModulus { get; } = youngsModulus;
  public MeasureValue YieldStrength { get; } = yieldStrength;
  public MeasureValue UltimateStrength { get; } = ultimateStrength;
  public double PoissonsRatio { get; } = poissonsRatio;
  public double ThermalExpansionPerK { get; } = thermalExpansionPerK;
  // Isotropic shear modulus G = E/(2(1+ν)) — a DERIVED read, never a stored column that could drift from E/ν
  // (the seam Mechanical is isotropic: one E, one ν, so G is exact, not an independent datum). PoissonsRatio
  // admits in [0,0.5], so 2(1+ν) ∈ [2,3] and G ∈ [E/3, E/2] is always finite and positive. It carries the Pressure
  // QuantityType (matching YoungsModulus.Type) through the TYPED OfSi overload, never the dimension-anonymous 2-arg form.
  public MeasureValue ShearModulus => MeasureValue.OfSi(QuantityType.Create("Pressure"), Dimension.PressureDim, YoungsModulus.Si / (2.0 * (1.0 + PoissonsRatio)));
 }
 // The ORTHOTROPIC structural case — a material whose along-axis (E1) and across-axis (E2) elastic moduli AND its shear
 // modulus G are INDEPENDENT measured data the isotropic Mechanical case structurally cannot model: timber along/across
 // grain (G_mean ≈ E0/16, not E/(2(1+ν))), a fibre-reinforced lamina, a cold-formed orthotropic deck. The DEFERRED
 // "future richer case" the ISOTROPIC_SHEAR research note named is realized HERE because a real consumer (the
 // Rasm.Materials timber#TIMBER_FAMILY family) produces it now — ANTICIPATORY_COLLAPSE on the present contract, never a
 // speculative case. Same Discipline.Structural as Mechanical (both structural-stiffness carriers, discriminated by the
 // case TYPE the generic Property<T> reads), so a structural runner reads props.Property<Orthotropic>() for a directional
 // material and props.Mechanical for an isotropic one. The columns are NEUTRAL MeasureValue Pressure scalars (the seam
 // references no VividOrange — the Rasm.Materials projector lowers timber's directional moduli/strengths through the
 // TYPED OfSi(QuantityType.Create("Pressure"), Dimension.PressureDim, _) overload), so a directional G never reads as an
 // isotropic-derived one and the seam stays host/library-neutral. E1Parallel/E2Perpendicular the two principal moduli, G
 // the independent in-plane shear, Strength1Parallel/Strength2Perpendicular the two principal compression/bearing
 // strengths (timber's fc0k/fc90k); a third out-of-plane axis is one further column when a genuinely-3D orthotropic
 // consumer admits it, never a parallel case.
 public sealed partial class Orthotropic(MeasureValue density, MeasureValue e1Parallel, MeasureValue e2Perpendicular, MeasureValue shearModulus, MeasureValue strength1Parallel, MeasureValue strength2Perpendicular, double thermalExpansionPerK, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasureValue Density { get; } = density;
  public MeasureValue E1Parallel { get; } = e1Parallel;
  public MeasureValue E2Perpendicular { get; } = e2Perpendicular;
  public MeasureValue ShearModulus { get; } = shearModulus;
  public MeasureValue Strength1Parallel { get; } = strength1Parallel;
  public MeasureValue Strength2Perpendicular { get; } = strength2Perpendicular;
  public double ThermalExpansionPerK { get; } = thermalExpansionPerK;
 }
 public sealed partial class Thermal(MeasureValue conductivity, MeasureValue specificHeat, MeasureValue uValue, double vapourResistanceFactor, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasureValue Conductivity { get; } = conductivity;
  public MeasureValue SpecificHeat { get; } = specificHeat;
  public MeasureValue UValue { get; } = uValue;
  public double VapourResistanceFactor { get; } = vapourResistanceFactor;
 }
 public sealed partial class Acoustic(global::Rasm.Element.Acoustic spectrum, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public global::Rasm.Element.Acoustic Spectrum { get; } = spectrum;
  // Forwarding reads so the Rasm.Materials marshaller and the Rasm.Compute layered-STC fold read the
  // single-material ratings off the case directly (a.SoundReductionIndexDb / a.StcWeighted), never .Spectrum.x;
  // the intrinsic constants forward likewise — a.DynamicStiffnessMNPerM3 is the EN 12354-2 floating-floor ΔL_w
  // input the Compute impact fold reads (the deferred impact RatingContour row's material leg).
  // The spectra forward as ImmutableArray<double> — the OWNER's storage type (Composition/acoustic#ACOUSTIC_FOLDS):
  // an ImmutableArray has NO implicit conversion to ReadOnlyMemory<double> (only .AsMemory()/.AsSpan()), so a
  // ReadOnlyMemory<double> forward would not compile, and forwarding the owner's type lets the Rasm.Compute Fit
  // consumer take the zero-copy .AsSpan() the contour kernel reads without an intervening copy.
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
 public sealed partial class Fire(FireRating reaction, SmokeClass smoke, DropletClass droplets, FireResistance resistance, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public FireRating Reaction { get; } = reaction;
  public SmokeClass Smoke { get; } = smoke;
  public DropletClass Droplets { get; } = droplets;
  public FireResistance Resistance { get; } = resistance;
 }
 // BASIS-AWARE: every Impacts cell is its ImpactCategory's characterization quantity PER the Basis unit (per-m³/per-m²/per-kg/
 // per-item) — the SAME MeasurementBasis the Cost case carries, so the Rasm.Compute AggregateEnvironmental fold scales each ply
 // by the basis-matching element quantity through the SAME basis-aware DeclaredQuantity derivation the cost fold uses (per-m³ →
 // volume, per-m² → face area, per-kg → volume×density, per-item → unit), NOT a forced per-m³ normalization that demanded a
 // density at ingress and SKIPPED an area/item EPD. A baked catalogue declaration is curated PerM3; an EC3-resolved declaration
 // carries the EPD's native declared_unit basis the EC3 ingress tags (Analysis/lifecycle Normalize). Impacts is the FULL EN
 // 15804+A2 (ImpactCategory × LifecycleStage) matrix stored ROW-MAJOR FLAT (ImmutableArray<double>, length ImpactCategory.Count
 // * LifecycleStage.Count) — the ONE impact store: modeling carbon alone where the standard mandates the full indicator family
 // was the deleted 1-of-13 slice, and a parallel per-indicator vector or a Map<ImpactCategory, ImmutableArray<double>> (whose
 // dictionary-value arrays would take REFERENCE equality, NOT content) is the deleted form. ImmutableArray (not ReadOnlyMemory)
 // is the immutable owner: it forbids the post-admission mutable-aliasing a ReadOnlyMemory<double> over a double[] admits, and
 // it IS IEnumerable<double> so [OrderedEquality] gives content equality and the StructuralMerge drills a changed cell to
 // Properties[i].Impacts[k]. RecycledContent/EndOfLifeRecovery are EN 15804 resource fractions. EPD provenance rides the
 // base Evidence (Declaration("epd", registrationNumber, validUntil LocalDate)) — the prior per-case Epd/ValidUntilYear
 // columns were a DOUBLE-STORE of the evidence concept (the Bim egress carried an includeValidUntilYear suppression flag
 // to dodge the duplicate), the deleted parallel provenance.
 public sealed partial class Environmental(MeasurementBasis basis, ImmutableArray<double> impacts, double recycledContent, double endOfLifeRecovery, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasurementBasis Basis { get; } = basis;
  [property: OrderedEquality] public ImmutableArray<double> Impacts { get; } = impacts;
  public double RecycledContent { get; } = recycledContent;
  public double EndOfLifeRecovery { get; } = endOfLifeRecovery;

  // The one general read: an (indicator, stage) cell off the row-major flat matrix — every named carbon convenience below
  // derives from it, never a per-indicator span scan. An out-of-arity (indicator, stage) reads 0.0 (a not-declared cell is
  // zero impact), so a partial EPD (carbon-only, the EC3 ingress) zeroes the un-declared indicator rows rather than faulting.
  public double IndicatorAt(ImpactCategory category, LifecycleStage stage) {
   int i = category.Index * LifecycleStage.Count + stage.Index;
   return i >= 0 && i < Impacts.Length ? Impacts[i] : 0.0;
  }
  // The whole-life (cradle-to-grave) fold of ONE indicator across every lifecycle stage — the general WholeLifeGwp the
  // Rasm.Compute embodied-carbon rollup reads per indicator, derived from IndicatorAt over the stage vocabulary.
  public double WholeLife(ImpactCategory category) =>
   LifecycleStage.Items.Sum(stage => IndicatorAt(category, stage));
  // DERIVED, never stored: the headline cradle-to-gate A1-A3 carbon IS the (GwpTotal, A1A3) matrix cell — a parallel stored
  // GlobalWarmingPotential scalar is a double-store of one fact (DERIVED_LOGIC) the acoustic carrier never admits. Gwp/
  // StageAt/WholeLifeGwp are the carbon-keyed convenience projections over the general IndicatorAt/WholeLife so the prior
  // GWP-only consumer reads unchanged while the matrix is the ONE owner; the OfEnvironmental admission guards Impacts
  // finiteness so these reads never surface a NaN.
  public double Gwp => IndicatorAt(ImpactCategory.GwpTotal, LifecycleStage.A1A3);   // cradle-to-gate A1-A3 GwpTotal (per Basis unit)
  public double StageAt(LifecycleStage stage) => IndicatorAt(ImpactCategory.GwpTotal, stage);  // per-stage GwpTotal (per Basis unit)
  public double WholeLifeGwp => WholeLife(ImpactCategory.GwpTotal);                 // cradle-to-grave GwpTotal (per Basis unit)
  // The GwpTotal-per-stage row READ FLAT — the per-module [A1A3, A4, A5, B, C, D] carbon vector the Rasm.Compute
  // Analysis/aggregator AssemblyAggregator.AddScaled scales per ply and the Analysis/lifecycle StageFacts emits, a
  // DERIVED single-row slice of the (ImpactCategory × LifecycleStage) matrix (the GwpTotal row across every stage),
  // NEVER a parallel stored 6-vector beside the matrix (DERIVED_LOGIC — the same one-owner discipline Gwp/WholeLifeGwp
  // hold). This is the consumer-contract one-hop the carbon aggregation reads INSTEAD of re-slicing IndicatorAt(GwpTotal,
  // stage) per stage at the call site (the deleted per-stage span scan): the seam owns the matrix-row projection so a
  // Compute fold reads env.StageGwp and never re-derives the row off Impacts, the SAME shape AggregateCost reads the
  // per-unit cost scalars. A fresh array per read (the Impacts matrix is the immutable store; this projects its
  // GwpTotal row in Index order — the stage order the matrix lays out and CanonicalBytes writes — as one
  // collection-expression fold, never a mutable write loop).
  public ImmutableArray<double> StageGwp =>
   [.. LifecycleStage.Items.OrderBy(static s => s.Index).Select(s => IndicatorAt(ImpactCategory.GwpTotal, s))];
  // The WRITE dual of StageGwp — embed a CARBON-ONLY per-module GwpTotal stage row (the [A1A3..D] vector a baked
  // catalogue row, an EC3 declaration, or a generic float-glass figure carries — LifecycleStage.Count entries) into the
  // FULL (ImpactCategory × LifecycleStage) row-major flat matrix OfEnvironmental admits: the GwpTotal indicator row at
  // its stable offset (GwpTotal.Index * LifecycleStage.Count + stage), EVERY other indicator row left ZERO — the seam's
  // partial-EPD invariant (a carbon-only declaration zeroes the un-declared indicator rows, the MatrixArity invariant).
  // This is the ONE carbon-row → matrix owner every carbon-only producer admits through (the Rasm.Materials catalogue
  // Lower, the GlazingSection lowering, the Rasm.Compute EC3 ingress), so a per-stage GWP vector reaches OfEnvironmental
  // as the full matrix WITHOUT each caller re-spelling the offset arithmetic (the deleted per-site embed) — the WRITE
  // peer of the StageGwp READ projection above, both owning the GwpTotal-row ↔ matrix correspondence at one site. A
  // future producer carrying the full EN 15804+A2 indicator set passes its matrix to OfEnvironmental directly, bypassing
  // this carbon-row convenience; a short row (fewer stages than LifecycleStage.Count) writes only the cells it carries.
  public static ImmutableArray<double> CarbonMatrix(ReadOnlyMemory<double> stageGwp) {
   double[] matrix = new double[MatrixArity];
   ReadOnlySpan<double> row = stageGwp.Span;
   int gwpRow = ImpactCategory.GwpTotal.Index * LifecycleStage.Count;
   int stages = Math.Min(row.Length, LifecycleStage.Count);
   for (int s = 0; s < stages; s++) { matrix[gwpRow + s] = row[s]; }
   return [.. matrix];
  }
  // The matrix row arity OfEnvironmental admits against (every indicator × every stage) — an accessor, so the
  // arity always reads the materialized vocabularies and never races a static-init order.
  public static int MatrixArity => ImpactCategory.Count * LifecycleStage.Count;
  // The zero-impact baseline a Rasm.Compute embodied-carbon fold seeds an element-set rollup from (PerM3, the curated default basis).
  public static readonly Environmental Empty = new(MeasurementBasis.PerM3, [.. new double[MatrixArity]], 0.0, 0.0, PropertyEvidence.Catalogue);
 }
 public sealed partial class Cost(MeasurementBasis basis, Currency currency, double supplyPerUnit, double installPerUnit, double lifecyclePerUnit, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public MeasurementBasis Basis { get; } = basis;
  public Currency Currency { get; } = currency;
  public double SupplyPerUnit { get; } = supplyPerUnit;
  public double InstallPerUnit { get; } = installPerUnit;
  public double LifecyclePerUnit { get; } = lifecyclePerUnit;
 }
 // The DYNAMIC-analysis carrier (Discipline.Dynamic): DampingRatio the fraction-of-critical ζ the EN 1998-1 response-
 // spectrum and EN 1990-A1.4.4 footfall routes read (welded steel ≈ 0.02, RC ≈ 0.05, timber ≈ 0.08 — LARGE-strain
 // design damping, independent of the Composition/acoustic small-strain LossFactor η whose amplitude regime differs by
 // orders of magnitude); Rayleigh the OPTIONAL per-material (α, β) proportional-damping pair a time-history FE model
 // reads (C = αM + βK — a per-material FE input, None when uncalibrated, a zero leg a legitimate pure mass- or
 // stiffness-proportional model); StructuralLossFactor the DERIVED hysteretic FE input s = 2ζ (exact by the linear-
 // viscous FE convention, never a stored column that could drift from ζ).
 public sealed partial class Damping(double dampingRatio, Option<(double AlphaPerS, double BetaS)> rayleigh, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double DampingRatio { get; } = dampingRatio;
  public Option<(double AlphaPerS, double BetaS)> Rayleigh { get; } = rayleigh;
  public double StructuralLossFactor => 2.0 * DampingRatio;
 }
 // The EN 15026 TRANSIENT hygrothermal carrier (Discipline.Hygrothermal) — the WUFI-class simulation inputs the
 // steady-state Thermal case (whose Glaser vapour-μ stays there) structurally cannot model: Porosity the open-pore
 // fraction; WaterContent80Rh/FreeWaterSaturation the two sorption-isotherm anchors (w80 at 80 % RH, wf at free
 // saturation — the two-point isotherm approximation the transient solver builds its storage function from), each a
 // MoistureStorage-typed kg/m³ MeasureValue (QuantityType.Create over Dimension.DensityDim so a water content never
 // reads as a bulk density under the discriminator — the SectionProperties same-dimension-distinct-type discipline);
 // WaterAbsorptionKgPerM2SqrtS the OPTIONAL capillary A-value (liquid transport; None for non-capillary materials) — a
 // raw double because its kg/(m²·√s) dimension carries a FRACTIONAL time exponent the integer Dimension 7-vector
 // cannot express, the declared unit riding the name per the ThermalExpansionPerK precedent.
 public sealed partial class Hygrothermal(double porosity, MeasureValue waterContent80Rh, MeasureValue freeWaterSaturation, Option<double> waterAbsorptionKgPerM2SqrtS, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double Porosity { get; } = porosity;
  public MeasureValue WaterContent80Rh { get; } = waterContent80Rh;
  public MeasureValue FreeWaterSaturation { get; } = freeWaterSaturation;
  public Option<double> WaterAbsorptionKgPerM2SqrtS { get; } = waterAbsorptionKgPerM2SqrtS;
 }
 // The fib Model Code service-life carrier (Discipline.Durability): CarbonationRateMmPerSqrtYear the carbonation
 // ingress coefficient K (mm/√year — a raw double, the √t dimension's fractional exponent outside the integer
 // Dimension vector; 0 a carbonation-immune material); ChlorideDiffusion the NT Build 492 migration coefficient
 // D_RCM as a ChlorideDiffusivity-typed m²/s MeasureValue (Create over the L²T⁻¹ signature); AgeingExponent the fib
 // decay exponent α in D(t) = D₀(t₀/t)^α, a [0,1] fraction — the three inputs the EN 206 exposure-class service-life
 // verification reads per material.
 public sealed partial class Durability(double carbonationRateMmPerSqrtYear, MeasureValue chlorideDiffusion, double ageingExponent, PropertyEvidence evidence) : MaterialPropertySet(evidence) {
  public double CarbonationRateMmPerSqrtYear { get; } = carbonationRateMmPerSqrtYear;
  public MeasureValue ChlorideDiffusion { get; } = chlorideDiffusion;
  public double AgeingExponent { get; } = ageingExponent;
 }
 // The SOLAR-OPTICAL carrier (Discipline.Energy) — the IFC Pset_MaterialOptical / EnergyPlus WindowMaterial:Glazing
 // column set the energy and daylight routes read per material: the visible and solar band each carry a transmittance
 // plus front/back reflectances (coated glass is side-asymmetric), the thermal-IR band a transmittance plus front/back
 // hemispherical emissivities (Kirchhoff: ε IS the IR absorptance). All nine are [0,1] spectral-average fractions;
 // the band absorptances are DERIVED remainders (1 − τ − ρ), never stored columns that could break conservation.
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
  // The EnergyPlus opaque-surface α and the EN 410 g-value secondary-heat input — the conservation remainder per side,
  // non-negative by the OfOptical conservation refinement.
  public double SolarAbsorptanceFront => 1.0 - SolarTransmittance - SolarReflectanceFront;
  public double SolarAbsorptanceBack => 1.0 - SolarTransmittance - SolarReflectanceBack;
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
  optical: static _ => Discipline.Energy);

 // The canonical content contribution the Graph/element#NODE_MODEL Node.Material arm folds per property (the bag
 // ordered by Discipline before this projection): the case ordinal then every SCALAR column — MeasureValue measures
 // tolerance-quantized through Measure, dimensionless ratios as exact IEEE-754 through Double, vocabulary rows through
 // their Key, the integer fire-resistance minutes / evidence-expiry calendar fields through Ordinal, Option columns
 // Bool-prefixed then written when present, the Acoustic spectrum delegating to its OWN banded CanonicalBytes
 // (Composition/acoustic) — so two Material nodes identical in MaterialKey + Composition + disciplines but differing
 // in ANY property scalar (a yield strength, a U-value, one impact-matrix cell) content-address distinctly and never
 // dedup-collide under the content-keyed NodeId.Content mint. The property VALUES are content, not the discipline keys
 // alone; the per-property write order is fixed by the case ordinal so the projection is self-delimiting.
 // Ordinals are STABLE content-key tags, never declaration positions: Orthotropic took the next free ordinal 6 (after
 // cost=5) rather than renumbering 1-5, and Damping/Hygrothermal/Durability/Optical take 7/8/9/10 the same way, so an
 // already-projected Material node keeps its prior content key and the cross-runtime golden vectors do not shift. The
 // environmental arm count-prefixes the flat impact matrix (Impacts.Length) before its cells so the layout is
 // self-delimiting, the SAME count-then-Double discipline the acoustic banded arm uses.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  mechanical:    m => CaseBytes(w, 0).Measure(m.Density).Measure(m.YoungsModulus).Measure(m.YieldStrength).Measure(m.UltimateStrength).Double(m.PoissonsRatio).Double(m.ThermalExpansionPerK),
  thermal:       t => CaseBytes(w, 1).Measure(t.Conductivity).Measure(t.SpecificHeat).Measure(t.UValue).Double(t.VapourResistanceFactor),
  acoustic:      a => { CaseBytes(w, 2); return a.Spectrum.CanonicalBytes(w); },
  fire:          f => CaseBytes(w, 3).String(f.Reaction.Key).String(f.Smoke.Key).String(f.Droplets.Key).Ordinal(f.Resistance.LoadBearingMinutes).Ordinal(f.Resistance.IntegrityMinutes).Ordinal(f.Resistance.InsulationMinutes),
  environmental: e => { CaseBytes(w, 4).String(e.Basis.Key).Ordinal(e.Impacts.Length); foreach (double v in e.Impacts.AsSpan()) { w.Double(v); } return w.Double(e.RecycledContent).Double(e.EndOfLifeRecovery); },
  cost:          c => CaseBytes(w, 5).String(c.Basis.Key).String(c.Currency.Value).Double(c.SupplyPerUnit).Double(c.InstallPerUnit).Double(c.LifecyclePerUnit),
  orthotropic:   o => CaseBytes(w, 6).Measure(o.Density).Measure(o.E1Parallel).Measure(o.E2Perpendicular).Measure(o.ShearModulus).Measure(o.Strength1Parallel).Measure(o.Strength2Perpendicular).Double(o.ThermalExpansionPerK),
  damping:       d => { CaseBytes(w, 7).Double(d.DampingRatio).Bool(d.Rayleigh.IsSome); d.Rayleigh.IfSome(r => w.Double(r.AlphaPerS).Double(r.BetaS)); return w; },
  hygrothermal:  h => { CaseBytes(w, 8).Double(h.Porosity).Measure(h.WaterContent80Rh).Measure(h.FreeWaterSaturation).Bool(h.WaterAbsorptionKgPerM2SqrtS.IsSome); h.WaterAbsorptionKgPerM2SqrtS.IfSome(v => w.Double(v)); return w; },
  durability:    u => CaseBytes(w, 9).Double(u.CarbonationRateMmPerSqrtYear).Measure(u.ChlorideDiffusion).Double(u.AgeingExponent),
  optical:       o => CaseBytes(w, 10).Double(o.VisibleTransmittance).Double(o.VisibleReflectanceFront).Double(o.VisibleReflectanceBack).Double(o.SolarTransmittance).Double(o.SolarReflectanceFront).Double(o.SolarReflectanceBack).Double(o.ThermalIrTransmittance).Double(o.ThermalIrEmissivityFront).Double(o.ThermalIrEmissivityBack));

 CanonicalWriter CaseBytes(CanonicalWriter w, int ordinal) {
  w.Ordinal(ordinal).String(Evidence.Source).String(Evidence.Reference).Bool(Evidence.ValidUntil.IsSome);
  Evidence.ValidUntil.IfSome(d => w.Ordinal(d.Year).Ordinal(d.Month).Ordinal(d.Day));
  return w;
 }

 // EVERY multi-column smart-constructor is an ACCUMULATING admission (VALIDATION_MONOID): each independent column is
 // one Validation<Error,_> slot, the tuple .Apply unions every ValueRejected through Error.Combine/ManyErrors, and
 // .ToFin() collapses ONCE at the seam return — so a datasheet with three bad columns reports THREE named faults in
 // one Fin.Fail, never first-fault-wins (a from/select chain over independent columns is the rejected form that
 // silently switches accumulation off). The public rail stays Fin<T>, so every consumer call site is untouched.
 // A negative or zero density / stiffness / strength is a physically-impossible material the MeasureValue.Of
 // finiteness gate alone does NOT catch (a negative MPa is finite) — the Positive slot guards finite AND strictly
 // positive per named column. ThermalExpansionPerK is EXCLUDED from positivity — a negative coefficient is physical
 // (a negative-thermal-expansion material), so its slot guards finiteness only; the Poisson ratio keeps its own
 // [0,0.5] isotropic-range relational pattern at the call site (the Guarded slot owns only the lift and fault mint).
 public static Fin<MaterialPropertySet> OfMechanical(double density, double youngsModulus, double yieldStrength, double ultimateStrength, double poissons, double thermalExpansion, Op key, PropertyEvidence evidence = default) =>
  (Positive(MeasureValue.Of(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter, key), "mechanical-density", key),
   Positive(MeasureValue.Of(youngsModulus, UnitsNet.Units.PressureUnit.Megapascal, key), "mechanical-youngs-modulus", key),
   Positive(MeasureValue.Of(yieldStrength, UnitsNet.Units.PressureUnit.Megapascal, key), "mechanical-yield-strength", key),
   Positive(MeasureValue.Of(ultimateStrength, UnitsNet.Units.PressureUnit.Megapascal, key), "mechanical-ultimate-strength", key),
   Guarded(poissons is >= 0.0 and <= 0.5, poissons, "poisson-out-of-isotropic-range", key),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite", key))
  .Apply((d, e, y, u, nu, a) => (MaterialPropertySet)new Mechanical(d, e, y, u, nu, a, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfMechanical(MeasureValue density, MeasureValue youngsModulus, MeasureValue yieldStrength, MeasureValue ultimateStrength, double poissons, double thermalExpansion, Op key, PropertyEvidence evidence = default) =>
  (Positive(density, "mechanical-density", key),
   Positive(youngsModulus, "mechanical-youngs-modulus", key),
   Positive(yieldStrength, "mechanical-yield-strength", key),
   Positive(ultimateStrength, "mechanical-ultimate-strength", key),
   Guarded(poissons is >= 0.0 and <= 0.5, poissons, "poisson-out-of-isotropic-range", key),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite", key))
  .Apply((d, e, y, u, nu, a) => (MaterialPropertySet)new Mechanical(d, e, y, u, nu, a, evidence))
  .As().ToFin();

 // The orthotropic structural admission — the two principal moduli (E1∥/E2⊥), the INDEPENDENT shear modulus G, and the
 // two principal strengths as RAW MPa doubles coerced to SI Pressure through the SAME UnitsNet registry OfMechanical
 // uses (the seam owns the coercion; the Rasm.Materials timber family passes the EN-grade E0Mean/E90Mean/GMean/Fc0k/Fc90k
 // data RAW). No isotropic Poisson guard — G is a measured datum here, not the derived E/(2(1+ν)). Seven independent
 // slots (the tuple `.Apply` family reaches ten), every offending column reported at once. A material lowers EITHER an
 // isotropic Mechanical OR a directional Orthotropic, never both — the case TYPE is the discriminant the structural runner reads.
 public static Fin<MaterialPropertySet> OfOrthotropic(double density, double e1Parallel, double e2Perpendicular, double shearModulus, double strength1Parallel, double strength2Perpendicular, double thermalExpansion, Op key, PropertyEvidence evidence = default) =>
  (Positive(MeasureValue.Of(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter, key), "orthotropic-density", key),
   Positive(MeasureValue.Of(e1Parallel, UnitsNet.Units.PressureUnit.Megapascal, key), "orthotropic-e1-parallel", key),
   Positive(MeasureValue.Of(e2Perpendicular, UnitsNet.Units.PressureUnit.Megapascal, key), "orthotropic-e2-perpendicular", key),
   Positive(MeasureValue.Of(shearModulus, UnitsNet.Units.PressureUnit.Megapascal, key), "orthotropic-shear-modulus", key),
   Positive(MeasureValue.Of(strength1Parallel, UnitsNet.Units.PressureUnit.Megapascal, key), "orthotropic-strength1-parallel", key),
   Positive(MeasureValue.Of(strength2Perpendicular, UnitsNet.Units.PressureUnit.Megapascal, key), "orthotropic-strength2-perpendicular", key),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite", key))
  .Apply((rho, e1, e2, g, s1, s2, a) => (MaterialPropertySet)new Orthotropic(rho, e1, e2, g, s1, s2, a, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfOrthotropic(MeasureValue density, MeasureValue e1Parallel, MeasureValue e2Perpendicular, MeasureValue shearModulus, MeasureValue strength1Parallel, MeasureValue strength2Perpendicular, double thermalExpansion, Op key, PropertyEvidence evidence = default) =>
  (Positive(density, "orthotropic-density", key),
   Positive(e1Parallel, "orthotropic-e1-parallel", key),
   Positive(e2Perpendicular, "orthotropic-e2-perpendicular", key),
   Positive(shearModulus, "orthotropic-shear-modulus", key),
   Positive(strength1Parallel, "orthotropic-strength1-parallel", key),
   Positive(strength2Perpendicular, "orthotropic-strength2-perpendicular", key),
   Guarded(double.IsFinite(thermalExpansion), thermalExpansion, "thermal-expansion-non-finite", key))
  .Apply((rho, e1, e2, g, s1, s2, a) => (MaterialPropertySet)new Orthotropic(rho, e1, e2, g, s1, s2, a, evidence))
  .As().ToFin();

 // The vapour-resistance factor μ is dimensionless and >= 1 by definition (μ = 1 is still air, no material resists
 // vapour LESS than air), so the `is >= 1.0` relational pattern accepts unity-and-above AND rejects NaN in one test —
 // a bare `< 1.0` admits NaN. Conductivity / specific-heat / U-value are strictly positive physical quantities the
 // per-column Positive slot rejects with the offending column NAMED, all misses accumulated.
 public static Fin<MaterialPropertySet> OfThermal(double conductivity, double specificHeat, double uValue, double vapourResistanceFactor, Op key, PropertyEvidence evidence = default) =>
  (Positive(MeasureValue.Of(conductivity, UnitsNet.Units.ThermalConductivityUnit.WattPerMeterKelvin, key), "thermal-conductivity", key),
   Positive(MeasureValue.Of(specificHeat, UnitsNet.Units.SpecificEntropyUnit.JoulePerKilogramKelvin, key), "thermal-specific-heat", key),
   Positive(MeasureValue.Of(uValue, UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, key), "thermal-u-value", key),
   Guarded(vapourResistanceFactor is >= 1.0, vapourResistanceFactor, "vapour-resistance-factor-below-unity", key))
  .Apply((c, s, u, mu) => (MaterialPropertySet)new Thermal(c, s, u, mu, evidence))
  .As().ToFin();

 public static Fin<MaterialPropertySet> OfThermal(MeasureValue conductivity, MeasureValue specificHeat, MeasureValue uValue, double vapourResistanceFactor, Op key, PropertyEvidence evidence = default) =>
  (Positive(conductivity, "thermal-conductivity", key),
   Positive(specificHeat, "thermal-specific-heat", key),
   Positive(uValue, "thermal-u-value", key),
   Guarded(vapourResistanceFactor is >= 1.0, vapourResistanceFactor, "vapour-resistance-factor-below-unity", key))
  .Apply((c, s, u, mu) => (MaterialPropertySet)new Thermal(c, s, u, mu, evidence))
  .As().ToFin();

 public static MaterialPropertySet OfAcoustic(global::Rasm.Element.Acoustic spectrum, PropertyEvidence evidence = default) =>
  new Acoustic(spectrum, evidence);

 // The 2-arg form defaults the smoke/droplet sub-class (NotSpecified) for a reaction-class-only datasheet; the full
 // form admits the complete EN 13501-1 "B-s1,d0" classification. Both total — a FireRating/FireResistance carry their own admission.
 public static MaterialPropertySet OfFire(FireRating reaction, FireResistance resistance, PropertyEvidence evidence = default) =>
  new Fire(reaction, SmokeClass.NotSpecified, DropletClass.NotSpecified, resistance, evidence);

 public static MaterialPropertySet OfFire(FireRating reaction, SmokeClass smoke, DropletClass droplets, FireResistance resistance, PropertyEvidence evidence = default) =>
  new Fire(reaction, smoke, droplets, resistance, evidence);

 // Every Impacts cell is its ImpactCategory's characterization quantity PER the basis unit — the caller declares the EPD's
 // native MeasurementBasis (the EC3 ingress tags the declared_unit basis, the Materials catalogue passes PerM3 for its
 // curated rows), so the Compute AggregateEnvironmental scales each ply by the basis-matching quantity (the SAME basis-aware
 // DeclaredQuantity derivation the cost fold uses). The headline cradle-to-gate A1-A3 carbon rides the (GwpTotal, A1A3) cell
 // (the derived Gwp read), NOT a separate GlobalWarmingPotential argument — the prior parallel scalar was a double-store of one
 // fact (DERIVED_LOGIC); EPD identity + LocalDate expiry ride the evidence argument (Declaration("epd", id, validUntil)),
 // never per-case columns. The matrix is the EN 15804+A2 (ImpactCategory × LifecycleStage) row-major flat ImmutableArray; an
 // EPD declaring fewer indicators (the carbon-only EC3 ingress) zeroes the un-declared rows so the matrix arity is invariant.
 // The characterization units are domain bases not SI dimensions, so the cells carry raw magnitudes the Matrix slot guards
 // ARITY-then-FINITE (dependent checks on ONE input bind inside the slot; the two resource fractions are independent slots
 // beside it) — the admission the derived IndicatorAt/Gwp/WholeLife reads then trust.
 public static Fin<MaterialPropertySet> OfEnvironmental(MeasurementBasis basis, ImmutableArray<double> impacts, double recycledContent, double endOfLifeRecovery, Op key, PropertyEvidence evidence = default) =>
  (Matrix(impacts, key),
   Guarded(recycledContent is >= 0.0 and <= 1.0, recycledContent, "environmental-recycled-content-out-of-unit", key),
   Guarded(endOfLifeRecovery is >= 0.0 and <= 1.0, endOfLifeRecovery, "environmental-recovery-out-of-unit", key))
  .Apply((m, recycled, recovery) => (MaterialPropertySet)new Environmental(basis, m, recycled, recovery, evidence))
  .As().ToFin();

 // A cost column is finite and non-negative — `IsFinite` rejects NaN/±∞ and `>= 0.0` the negative, per NAMED column,
 // all misses accumulated: a NaN or infinite per-unit cost would otherwise enter the content hash through the
 // raw-double columns the MeasureValue finiteness gate never sees, so the seam guards the cost columns the way it
 // guards a measure.
 public static Fin<MaterialPropertySet> OfCost(Currency currency, MeasurementBasis basis, double supply, double install, double lifecycle, Op key, PropertyEvidence evidence = default) =>
  (Guarded(double.IsFinite(supply) && supply >= 0.0, supply, "cost-supply-non-finite-or-negative", key),
   Guarded(double.IsFinite(install) && install >= 0.0, install, "cost-install-non-finite-or-negative", key),
   Guarded(double.IsFinite(lifecycle) && lifecycle >= 0.0, lifecycle, "cost-lifecycle-non-finite-or-negative", key))
  .Apply((s, i, l) => (MaterialPropertySet)new Cost(basis, currency, s, i, l, evidence))
  .As().ToFin();

 // The dynamic-analysis admission: ζ the fraction-of-critical damping ratio in [0,1) (an at-or-over-critical material
 // datum is a datasheet error, not a material), the optional Rayleigh (α, β) pair finite and non-negative when Some
 // (a zero leg is a legitimate pure mass- or stiffness-proportional model, so non-negative not strictly-positive).
 public static Fin<MaterialPropertySet> OfDamping(double dampingRatio, Option<(double AlphaPerS, double BetaS)> rayleigh, Op key, PropertyEvidence evidence = default) =>
  (Guarded(dampingRatio is >= 0.0 and < 1.0, dampingRatio, "damping-ratio-out-of-critical-fraction", key),
   Rayleigh(rayleigh, key))
  .Apply((zeta, pair) => (MaterialPropertySet)new Damping(zeta, pair, evidence))
  .As().ToFin();

 // The EN 15026 transient-hygrothermal admission: the leaf columns accumulate applicatively, THEN the composite
 // isotherm refinement binds AFTER every leaf succeeds (COMPOSITE_ADMISSION — free saturation is the isotherm's upper
 // anchor, so wf < w80 is an inverted sorption curve the seam rejects as one cross-field fault).
 public static Fin<MaterialPropertySet> OfHygrothermal(double porosity, double waterContent80Rh, double freeWaterSaturation, Option<double> waterAbsorption, Op key, PropertyEvidence evidence = default) =>
  (Guarded(porosity is >= 0.0 and <= 1.0, porosity, "hygrothermal-porosity-out-of-unit", key),
   PositiveSi(waterContent80Rh, QuantityType.Create("MoistureStorage"), Dimension.DensityDim, "hygrothermal-w80", key),
   PositiveSi(freeWaterSaturation, QuantityType.Create("MoistureStorage"), Dimension.DensityDim, "hygrothermal-free-saturation", key),
   OptionalPositive(waterAbsorption, "hygrothermal-water-absorption", key))
  .Apply((phi, w80, wf, a) => (Phi: phi, W80: w80, Wf: wf, A: a))
  .As().ToFin()
  .Bind(t => t.Wf.Si >= t.W80.Si
   ? Fin.Succ<MaterialPropertySet>(new Hygrothermal(t.Phi, t.W80, t.Wf, t.A, evidence))
   : Fin.Fail<MaterialPropertySet>(ElementFault.ValueRejected(key, $"<hygrothermal-isotherm-inverted:w80={t.W80.Si:R}:wf={t.Wf.Si:R}>")));

 // The fib Model Code service-life admission: K non-negative (0 = carbonation-immune), D_RCM strictly positive and
 // minted on the L²T⁻¹ chloride-diffusivity signature, the ageing exponent a [0,1] fraction.
 public static Fin<MaterialPropertySet> OfDurability(double carbonationRate, double chlorideDiffusion, double ageingExponent, Op key, PropertyEvidence evidence = default) =>
  (Guarded(double.IsFinite(carbonationRate) && carbonationRate >= 0.0, carbonationRate, "durability-carbonation-rate-non-finite-or-negative", key),
   PositiveSi(chlorideDiffusion, QuantityType.Create("ChlorideDiffusivity"), Dimension.Create(2, 0, -1, 0, 0, 0, 0), "durability-chloride-diffusion", key),
   Guarded(ageingExponent is >= 0.0 and <= 1.0, ageingExponent, "durability-ageing-exponent-out-of-unit", key))
  .Apply((k, d, alpha) => (MaterialPropertySet)new Durability(k, d, alpha, evidence))
  .As().ToFin();

 // The solar-optical admission: the nine [0,1] leaf fractions accumulate first (each an in-unit NaN-rejecting slot),
 // THEN the six per-band-per-side conservation refinements (τ + ρ ≤ 1 visible/solar front/back; τIR + ε ≤ 1 per side —
 // Kirchhoff over an opaque-or-transparent layer) accumulate as a SECOND stage over the constructed case, so a
 // datasheet violating two bands reports both (COMPOSITE_ADMISSION — leaves before refinements, refinements
 // accumulating among themselves because each reads a disjoint column pair).
 public static Fin<MaterialPropertySet> OfOptical(double visibleTransmittance, double visibleReflectanceFront, double visibleReflectanceBack, double solarTransmittance, double solarReflectanceFront, double solarReflectanceBack, double thermalIrTransmittance, double thermalIrEmissivityFront, double thermalIrEmissivityBack, Op key, PropertyEvidence evidence = default) =>
  (Guarded(visibleTransmittance is >= 0.0 and <= 1.0, visibleTransmittance, "optical-visible-transmittance-out-of-unit", key),
   Guarded(visibleReflectanceFront is >= 0.0 and <= 1.0, visibleReflectanceFront, "optical-visible-reflectance-front-out-of-unit", key),
   Guarded(visibleReflectanceBack is >= 0.0 and <= 1.0, visibleReflectanceBack, "optical-visible-reflectance-back-out-of-unit", key),
   Guarded(solarTransmittance is >= 0.0 and <= 1.0, solarTransmittance, "optical-solar-transmittance-out-of-unit", key),
   Guarded(solarReflectanceFront is >= 0.0 and <= 1.0, solarReflectanceFront, "optical-solar-reflectance-front-out-of-unit", key),
   Guarded(solarReflectanceBack is >= 0.0 and <= 1.0, solarReflectanceBack, "optical-solar-reflectance-back-out-of-unit", key),
   Guarded(thermalIrTransmittance is >= 0.0 and <= 1.0, thermalIrTransmittance, "optical-ir-transmittance-out-of-unit", key),
   Guarded(thermalIrEmissivityFront is >= 0.0 and <= 1.0, thermalIrEmissivityFront, "optical-ir-emissivity-front-out-of-unit", key),
   Guarded(thermalIrEmissivityBack is >= 0.0 and <= 1.0, thermalIrEmissivityBack, "optical-ir-emissivity-back-out-of-unit", key))
  .Apply((tv, rvf, rvb, te, rsf, rsb, tir, ef, eb) => new Optical(tv, rvf, rvb, te, rsf, rsb, tir, ef, eb, evidence))
  .As().ToFin()
  .Bind(o =>
   (Guarded(o.VisibleTransmittance + o.VisibleReflectanceFront <= 1.0, o.VisibleTransmittance + o.VisibleReflectanceFront, "optical-visible-front-conservation", key),
    Guarded(o.VisibleTransmittance + o.VisibleReflectanceBack <= 1.0, o.VisibleTransmittance + o.VisibleReflectanceBack, "optical-visible-back-conservation", key),
    Guarded(o.SolarTransmittance + o.SolarReflectanceFront <= 1.0, o.SolarTransmittance + o.SolarReflectanceFront, "optical-solar-front-conservation", key),
    Guarded(o.SolarTransmittance + o.SolarReflectanceBack <= 1.0, o.SolarTransmittance + o.SolarReflectanceBack, "optical-solar-back-conservation", key),
    Guarded(o.ThermalIrTransmittance + o.ThermalIrEmissivityFront <= 1.0, o.ThermalIrTransmittance + o.ThermalIrEmissivityFront, "optical-ir-front-conservation", key),
    Guarded(o.ThermalIrTransmittance + o.ThermalIrEmissivityBack <= 1.0, o.ThermalIrTransmittance + o.ThermalIrEmissivityBack, "optical-ir-back-conservation", key))
   .Apply((_, _, _, _, _, _) => (MaterialPropertySet)o)
   .As().ToFin());

 // --- [ADMISSION_SLOTS]
 // The accumulating column combinators every multi-column Of* composes — each returns one Validation<Error,_> slot
 // whose fault NAMES the offending column, so the tuple .Apply reports the union. Guarded owns only the lift + fault
 // mint (the call site keeps its NaN-rejecting relational pattern); Positive gates a measured column finite-and-
 // strictly-positive (the raw-double form after the MeasureValue.Of SI coercion, the MeasureValue form direct);
 // PositiveSi guards-then-mints a consumer-domain quantity through the TYPED OfSi (the registry-less signatures);
 // OptionalPositive/Rayleigh gate Option-carried columns when Some and pass None (absence is legal, never a sentinel).
 // Every ternary target-types the Validation lift (value implicit on the success arm, the bare fault on the fail arm) —
 // the spelled-out cast is the deleted ceremony.
 static Validation<Error, double> Guarded(bool valid, double value, string name, Op key) =>
  valid ? value : ElementFault.ValueRejected(key, $"<{name}:{value:R}>");

 static Validation<Error, MeasureValue> Positive(Fin<MeasureValue> column, string name, Op key) =>
  column.Bind(m => m.Si > 0.0 ? Fin.Succ(m) : ElementFault.ValueRejected(key, $"<{name}-non-positive:{m.Si:R}>")).ToValidation();

 static Validation<Error, MeasureValue> Positive(MeasureValue column, string name, Op key) =>
  Positive(Fin.Succ(column), name, key);

 static Validation<Error, MeasureValue> PositiveSi(double value, QuantityType type, Dimension dimension, string name, Op key) =>
  double.IsFinite(value) && value > 0.0
   ? MeasureValue.OfSi(type, dimension, value)
   : ElementFault.ValueRejected(key, $"<{name}-non-positive:{value:R}>");

 static Validation<Error, Option<double>> OptionalPositive(Option<double> value, string name, Op key) =>
  value.Exists(static v => !double.IsFinite(v) || v <= 0.0)
   ? ElementFault.ValueRejected(key, $"<{name}-non-positive>")
   : value;

 static Validation<Error, Option<(double AlphaPerS, double BetaS)>> Rayleigh(Option<(double AlphaPerS, double BetaS)> pair, Op key) =>
  pair.Exists(static r => !double.IsFinite(r.AlphaPerS) || r.AlphaPerS < 0.0 || !double.IsFinite(r.BetaS) || r.BetaS < 0.0)
   ? ElementFault.ValueRejected(key, "<damping-rayleigh-non-finite-or-negative>")
   : pair;

 // The dependent matrix slot: arity gates the finiteness scan of the SAME array (dependence binds inside one slot;
 // independence accumulates across slots — EXPRESSION_SPINE's carrier-selected algebra in one constructor).
 static Validation<Error, ImmutableArray<double>> Matrix(ImmutableArray<double> impacts, Op key) =>
  impacts.IsDefaultOrEmpty || impacts.Length != Environmental.MatrixArity
   ? ElementFault.ValueRejected(key, $"<environmental-impact-arity:{(impacts.IsDefault ? -1 : impacts.Length)}:expected={Environmental.MatrixArity}>")
   : !AllFinite(impacts.AsSpan())
    ? ElementFault.ValueRejected(key, "<environmental-impact-non-finite>")
    : impacts;

 // The cell finiteness guard the Matrix slot folds the impact matrix through — a span scan so the derived
 // IndicatorAt/Gwp/WholeLife reads never surface a NaN the content hash would also fork on.
 static bool AllFinite(ReadOnlySpan<double> values) {
  foreach (double v in values) { if (!double.IsFinite(v)) { return false; } }
  return true;
 }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class MaterialPropertyAccess {
 extension(Seq<MaterialPropertySet> properties) {
  // ONE polymorphic typed read — the case TYPE is the discriminant (recoverable from the value per MODAL_ARITY); the
  // ten per-case reads below DERIVE from this one `Choose p is T` body (the repeated arms collapse to one), and a
  // case with no named read (a future discipline) is read generically via properties.Property<MaterialPropertySet.X>().
  public Option<T> Property<T>() where T : MaterialPropertySet =>
   properties.Choose(static p => p is T t ? Some(t) : None).Head;

  // The ergonomic per-discipline reads the Rasm.Compute aggregator consumes (props.Thermal/Mechanical/Environmental) —
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
  public Option<MaterialPropertySet.Durability> Durability => properties.Property<MaterialPropertySet.Durability>();
  public Option<MaterialPropertySet.Cost> Cost => properties.Property<MaterialPropertySet.Cost>();

  // The dual by-discipline read a Rasm.Compute analysis route takes when it holds a runtime Discipline value
  // rather than a static case type — dispatching on the case→Discipline map, never a parallel discipline scan.
  public Option<MaterialPropertySet> ForDiscipline(Discipline discipline) => properties.Find(p => p.Discipline == discipline);
 }
}
```

## [04]-[RESEARCH]

- [MATERIAL_COLLAPSE]: the migration source carried TWO parallel material owners — `Rasm.Materials` `MaterialAssignment` (the `LayerSet`/`ProfileSet`/`ConstituentSet` trichotomy) and `MaterialProperty` (the `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost` unions keyed by `MaterialId`) — never joined to an element; this owner collapses both onto the seam `Material` node so one node over one `MaterialId` carries its composition AND its full property profile, the `Rasm.Materials` projector lowering its subgraph onto `Material` nodes and the `Rasm.Bim` projector reading them at the IFC boundary; the `Single` case is the addition the migration trichotomy lacked (a homogeneous `IfcMaterial`), the four cases now the full IFC material-definition family. The `Of`-prefixed factory family (`OfSingle`/`OfLayerSet`/`OfProfileSet`/`OfConstituentSet`) mirrors the sibling `MaterialPropertySet.Of*` convention so a smart-constructor never shadows its same-named nested case type (a bare `Single(...)` method and a nested `Single` case share one declaration space — a compile collision), and the invariant-bearing `LayerSet`/`ConstituentSet` cases gate admission through a private constructor + internal `Seed` (the `relation#MaterialUsage.ProfileSet` / `acoustic#Acoustic` shape) so a degenerate set is unrepresentable and `PrimaryMaterial`'s `First()` is total.
- [PROFILE_REF_RESOLUTION]: the `ProfileRef` (`Standard` + `Designation` + content key, the content key projected through the ONE `Projection/address#CONTENT_ADDRESS` `CanonicalWriter`) is the M7 one-hop resolution seam — the `Rasm.Materials` projector resolves it ONCE through the section catalogue to a section and BAKES the resulting neutral seam `SectionProperties` onto the `ProfileSet` composition (`WithSection`), so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf(member)`) without re-resolving per call OR admitting VividOrange. The `SectionProperties` is the FULL design-code column set the `Rasm.Compute` checks read (a CONSUMER CONTRACT): `Area`/`Iyy`/`Izz`/`Wely`/`Welz`/`RadiusOfGyrationMajor`/`Minor` the `VividOrange.Sections.SectionProperties` polygon solver SOURCES (`Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`), `Depth`/`Width` from its `Extends` bounding extents and `HeatedPerimeter` from its `Perimeter`, plus the plastic moduli (`Wply`/`Wplz`), the St-Venant torsion constant (`J`), the warping constant (`Iw`, the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling input the bare `J` cannot supply), the both-axis shear areas (`AvY`/`AvZ`), and the asymmetric-section flexural-torsional-buckling columns — the both-axis shear-centre offsets (`ShearCentreY`/`ShearCentreZ`, the centroid→shear-centre distance per axis) and the mono-symmetry factor (`MonosymmetryFactor`, the EN 1993-1-1 NCCI SN030 β_y) — the polygon solver does NOT expose, the `Rasm.Materials` section resolver computing those from the section geometry. The asymmetry columns are first-class (not deferred) and read ZERO for a doubly-symmetric section, so a channel/tee/angle's EN 1993-1-1 §6.3.2 general LTB check is supplied off the seam where the prior symmetric-only column set could not LTB-check a PFC/tee — the `IsDoublySymmetric` predicate the §6.3.2 route reads to take the simplified form is DERIVED from those offsets, never a stored flag. The EN 1992-1-2 reinforcement `AxisDistance` cover (the RC section) and the `LeastDimension` the EN 1992-1-2 concrete-fire check reads (a DERIVED `min(Depth, Width)`) complete the fire columns. The seam declares the consumer-required shape; sourcing the full column set is the `Rasm.Materials` `ComputedSection` resolver's obligation, never a seam concern.
- [USAGE_ON_EDGE]: the occurrence usage binding (the IFC `IfcMaterialLayerSetUsage` `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and the `IfcMaterialProfileSetUsage` `CardinalPoint`/`ReferenceExtent`) rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, NOT this owner — the `MaterialComposition` is the type-level SET structure shared across occurrences, the usage the per-occurrence geometric binding on the edge, so a layer set's direction never duplicates onto the composition and a wall and its mirror share one `LayerSet` with two `Associate` usages.
- [ISOTROPIC_SHEAR]: the `Mechanical` shear modulus is the DERIVED isotropic relation `G = E/(2(1+ν))`, not a stored column — the seam `Mechanical` models an isotropic material (one `YoungsModulus`, one `PoissonsRatio`), so a stored `G` independent of `E`/`ν` could only ever drift from the relation it must satisfy; `PoissonsRatio` admits in the physical isotropic `[0,0.5]` (rejecting the thermodynamically-impossible `ν > 0.5` the prior `[0,1]` guard allowed), so `2(1+ν) ∈ [2,3]` and `G` is always finite and positive. `UltimateStrength` is a first-class `MeasureValue` column (the `Rasm.Compute` ACI 318 concrete `f'c` and EN 1993 net-section/connection checks read `Strength.UltimateStrength`), so it is not derivable from yield and stays stored; an orthotropic material (timber along/across grain) carries an INDEPENDENT measured `G` plus distinct principal moduli `E1∥`/`E2⊥` — the `Orthotropic` `MaterialPropertySet` case (REALIZED, the `Rasm.Materials` `timber#TIMBER_FAMILY` family the consumer that produces it now, ANTICIPATORY_COLLAPSE on the present contract), distinct from the isotropic `Mechanical` and discriminated by the case TYPE the generic `Property<T>` reads, never a stored `G` field smuggled onto the isotropic owner.
- [FIRE_CLASSIFICATION]: a material's fire performance is two EN standards, not one scalar — EN 13501-1 reaction-to-fire (the `FireRating` Euroclass `A1`…`F` plus the `SmokeClass` `s1`…`s3` and `DropletClass` `d0`…`d2` sub-classes that complete a "B-s1,d0" classification) and EN 13501-2 fire resistance (the `FireResistance` R load-bearing / E integrity / I insulation criteria, each an independent minute rating, so a load-bearing `R 90` column, a separating `EI 60` wall, and a load-bearing separating `REI 90` wall are distinct); a single resistance scalar conflating R/E/I is the deleted form, and `FireResistance.Rei`/`R`/`Ei` are the convenience constructors for the common ratings.
- [TYPED_LOOKUP_COLLAPSE]: the per-case property reads collapse to ONE generic `Property<T>()` over the case type — the eleven `Choose(p => p is T t ? Some(t) : None).Head` bodies a per-case reader would enumerate share one generative structure, so the body lives ONCE on the generic owner and the named per-case accessors (`props.Mechanical`/`props.Thermal`/…, the consumer-contract surface the `Rasm.Compute` aggregator reads) are one-line projections of it, never eleven re-implemented `Choose` bodies; `ForDiscipline(discipline)` is the orthogonal dual (a runtime `Discipline` value rather than a static case type), the `Rasm.Compute` analysis route reading a discipline's property set through whichever entry it holds.
- [ENVIRONMENTAL_INDICATOR_FAMILY]: a material's environmental profile is the EN 15804+A2 CORE INDICATOR set, not GWP alone — the standard mandates global-warming (the `GwpTotal`/`GwpFossil`/`GwpBiogenic`/`GwpLuluc` decomposition), ozone-depletion (`Odp`), acidification (`Ap`), eutrophication (`EpFreshwater`/`EpMarine`/`EpTerrestrial`), photochemical-ozone (`Pocp`), abiotic-depletion (`AdpMinerals`/`AdpFossil`), and water-deprivation (`Wdp`), each declared per lifecycle module, so a single `StageGwp` GWP vector was a 1-of-13 slice of the impact concept. The `Environmental` case carries the full `(ImpactCategory × LifecycleStage)` matrix row-major flat in ONE `ImmutableArray<double>` Impacts store — the next indicator is one `ImpactCategory` row, not a parallel field, and the basis-aware aggregation the `Rasm.Compute` `AggregateEnvironmental` fold runs is indicator-agnostic (it scales any cell by the basis-matching quantity), so the matrix is the ANTICIPATORY_COLLAPSE shape the moment a second indicator beyond carbon is conceivable. `ImmutableArray<double>` over `ReadOnlyMemory<double>` is load-bearing twice — it forbids the post-admission mutable-aliasing a memory-over-`double[]` admits (the "admitted once" invariant the content hash depends on) AND it IS `IEnumerable<double>` so the `[OrderedEquality]` member compares by content and the `Rasm.Persistence` `StructuralMerge` drills a changed cell to `Properties[i].Impacts[k]`, where a `ReadOnlyMemory<double>` member would take reference equality and a `Map<ImpactCategory, ImmutableArray<double>>` would key its dictionary-value arrays by reference; the carbon-keyed `Gwp`/`StageAt`/`WholeLifeGwp` scalar reads AND the `StageGwp` per-module GwpTotal-row vector survive as one-line convenience projections over the general `IndicatorAt`/`WholeLife` so the EC3 embodied-carbon consumer reads unchanged while the matrix is the ONE store — `StageGwp` is the `[A1A3, A4, A5, B, C, D]` carbon row the `Rasm.Compute` `AggregateEnvironmental` fold scales per ply (`AddScaled`) and the `Analysis/lifecycle` `StageFacts` emits, a DERIVED matrix-row slice the seam owns so the carbon aggregation reads it one-hop rather than re-slicing `IndicatorAt(GwpTotal, stage)` per stage, never a parallel stored 6-vector beside the matrix — and a carbon-only EPD zeroes the un-declared indicator rows so the matrix arity stays invariant.
- [ACCUMULATED_ADMISSION]: the multi-column smart-constructors are VALIDATION_MONOID admissions — each independent column is one `Validation<Error,_>` slot from the `[ADMISSION_SLOTS]` combinator family (`Guarded` the raw-double lift whose call site keeps the NaN-rejecting relational pattern, `Positive` the finite-and-strictly-positive measured column over `MeasureValue.Of`'s SI coercion via `Fin.ToValidation()`, `PositiveSi` the guard-then-mint for the registry-less TYPED `OfSi` signatures, `OptionalPositive`/`Rayleigh` the Option-carried gates that pass `None`, `Matrix` the one DEPENDENT slot whose arity check gates the finiteness scan of the same array), the tuple `.Apply` joins the slots so `Error.Combine` unions every `ElementFault.ValueRejected` into one `ManyErrors`, and `.ToFin()` collapses ONCE at the seam return — the public rail stays `Fin<T>` so every consumer call site is untouched while a three-bad-column datasheet reports three NAMED faults in one `Fin.Fail`; a `from`/`select` chain over independent columns is the rejected form that silently switches accumulation off, and dependence binds INSIDE a slot while independence accumulates ACROSS slots (`OfHygrothermal` additionally binds its `wf >= w80` cross-field isotherm refinement AFTER the accumulated leaves, and `OfOptical` accumulates its six mutually-independent conservation refinements as a second `.Apply` stage over the constructed case — the COMPOSITE_ADMISSION leaf-before-refinement order, refinements themselves accumulating where they read disjoint column pairs); the deleted forms are the boolean `Positive` span guard that collapsed every offending column into one anonymous `<mechanical-non-positive-column>` fault and the raw-overload delegation that re-ran every guard on the measured overload.
- [DISCIPLINE_CASE_EXTENSION]: the `Damping`/`Hygrothermal`/`Durability`/`Optical` cases close the discipline space the seven-case family under-modeled, each the growth law executed (one case + one `Discipline` row + one next-free `CanonicalBytes` ordinal + one named forward, zero surfaces beside the union) — `Damping` carries the EN 1998-1 fraction-of-critical `DampingRatio` ζ the response-spectrum and footfall routes read (welded steel ≈ `0.02`, RC ≈ `0.05`, timber ≈ `0.08`), the OPTIONAL per-material Rayleigh `(α, β)` pair a time-history FE model consumes (`C = αM + βK`; `None` when uncalibrated, a zero leg a legitimate one-sided model), and the DERIVED hysteretic `StructuralLossFactor = 2ζ` (exact by the linear-viscous FE convention — the material's ACOUSTIC small-strain `η` stays on the `Composition/acoustic#ACOUSTIC_FOLDS` carrier because the two standards measure amplitude regimes orders of magnitude apart, independent data never a derivation pair); `Hygrothermal` carries the EN 15026 transient-simulation inputs the steady-state `Thermal` case cannot model (the Glaser vapour-`μ` STAYS on `Thermal`) — `Porosity`, the `WaterContent80Rh`/`FreeWaterSaturation` two-point sorption-isotherm anchors as MoistureStorage-typed kg/m³ measures with the `wf >= w80` monotonicity refinement, the optional capillary A-value — the WUFI-class material record; `Durability` carries the fib Model Code service-life inputs the EN 206 exposure verification reads — carbonation `K` (mm/√year), the NT Build 492 `ChlorideDiffusion` `D_RCM` on the L²T⁻¹ ChlorideDiffusivity signature, the ageing exponent `α` of `D(t) = D₀(t₀/t)^α`; `Optical` carries the IFC `Pset_MaterialOptical` / EnergyPlus `WindowMaterial:Glazing` solar-optical record the `Discipline.Energy` and `Daylight` routes read — per-band transmittance plus side-asymmetric front/back reflectances (visible + solar) and thermal-IR transmittance plus front/back emissivities, the `τ + ρ <= 1` / `τIR + ε <= 1` conservation refinements accumulated as a second admission stage and the band absorptances derived remainders, the render BSDF staying the `Rasm.Materials` Appearance owner's; the EPD/declaration expiry rides `PropertyEvidence.ValidUntil` as a NodaTime `LocalDate` (the EC3 ingress carries a full date the prior int-year truncated), single-stored on the base evidence every case already writes through `CaseBytes`.
- [STRUCTURAL_EQUALITY_DRILL]: `MaterialComposition` and `MaterialPropertySet` are CLASS-root `[Union]` + `[Equatable]` (the `[GRAPH_FAMILY]` form), and `MaterialLayer`/`MaterialConstituent`/`SectionProperties` are `[Equatable]` record structs — the `Graph/element#NODE_MODEL` `Node.Material` `[Equatable]` drill descends ONE `[Equatable]` link per hop, so a record-root `MaterialComposition`/`MaterialPropertySet` would be an opaque equality leaf and the `Rasm.Persistence` 3-way `StructuralMerge` would key `Nodes[id].Composition` / `Nodes[id].Properties[i]` WHOLE rather than localizing a changed layer thickness / constituent fraction / section column / property column to `Nodes[id].Composition.Layers[2].Thickness` etc. (the RFC 6902 patch member granularity the merge keys its egress on). A class-root `[Union]` surrenders Thinktecture's record-generated equality (so `[Equatable]` is the SOLE equality owner, never stacked on a record-root union) but keeps the generated `Switch`/`Map`; a class-root case has no compiler `with`, so `ProfileSet.With`/`WithSection` reconstruct through the public positional ctor. The drill bottoms at the `Properties/quantity#MEASURE_VALUE` `MeasureValue` leaf (a native-equality record struct the `Generator.Equals` `DefaultEqualityComparer` compares atomically — the measure IS the leaf, no `[Equatable]` needed), the `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` carrier the `MaterialPropertySet.Acoustic` case wraps (itself `[Equatable]` there), and the flat `ImmutableArray<double>` impact matrix (`[OrderedEquality]` content equality).
