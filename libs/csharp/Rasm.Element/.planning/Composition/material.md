# [ELEMENT_MATERIAL]

The host-neutral material owner: the `MaterialId` `[ValueObject<string>]` a `Material` node keys on, one `MaterialComposition` `[Union]` closing the type-level material-set structure (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), and one `MaterialPropertySet` `[Union]` closing the typed engineering-property family (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) keyed to the one `Classification/classification#DISCIPLINE_AXIS` `Discipline`. A material is a FULL engineering object: a `Material` node carries its composition and its property sets over one `MaterialId`, so a consumer reads a material's U-value, sound spectrum, fire REI rating, structural grade, embodied carbon, and cost from one node — never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface, never a per-discipline material type. This is the seam home the migration source's two parallel owners collapse onto: the `Rasm.Materials` `MaterialAssignment` trichotomy and its `MaterialProperty` unions become the seam `MaterialComposition` and `MaterialPropertySet`, the `Rasm.Materials` `MaterialProjector` lowering its material subgraph onto `Material` nodes and the `Rasm.Bim` projector reading them. The occurrence usage binding (layer direction/offset, profile cardinal point) is NOT here — it rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, this owner carrying only the type-level SET structure. The page composes `Properties/quantity#MEASURE_VALUE` for every measured column (`MeasureValue.Of` SI coercion + the `Dimension` discriminator), `Composition/acoustic#ACOUSTIC_FOLDS` for the `Acoustic` case, and `Classification/classification#DISCIPLINE_AXIS` for the property-to-discipline key; a non-finite or out-of-range admission rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page references NO VividOrange — a `ProfileSet` carries a neutral `ProfileRef` PLUS the neutral `SectionProperties` the `Rasm.Materials` projector resolves ONE-HOP (M7) and BAKES on (`WithSection`), so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf`) without re-resolving or admitting VividOrange. The `SectionProperties` receipt carries the FULL structural-design and fire column set the `Rasm.Compute` design-code checks read off the seam (the AISC 360 / EN 1993 / AISI S100 / ACI 318 / NDS / TMS 402 flexure-shear-compression and the EN 1993-1-2 / EN 1992-1-2 fire routes) — a CONSUMER-CONTRACT-driven shape, the `Rasm.Materials` projector resolving the elastic columns from the VividOrange polygon solver and computing the plastic moduli / torsion constant / shear areas the solver does not expose, never a per-element-type section receipt.

## [01]-[INDEX]

- [01]-[MATERIAL_COMPOSITION]: the `MaterialId` key, the `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) with the `Of`-prefixed admissions, the private-constructor admission gate on the invariant-bearing cases, the `MaterialLayer`/`MaterialConstituent` rows, the `ProfileRef` neutral section reference, and the consumer-shaped neutral `SectionProperties` the M7 bake stamps.
- [02]-[MATERIAL_PROPERTY]: the `MaterialPropertySet` `[Union]` (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) keyed to `Discipline`, the `FireRating`/`SmokeClass`/`DropletClass` reaction vocabulary and the `FireResistance` REI criteria, the derived isotropic shear modulus, the `Of` admissions, and the one generic `Property<T>()` typed lookup the named per-discipline reads derive from.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialId` the `[ValueObject<string>]` material-identity key a `Material` node carries; `MaterialComposition` the `[Union]` type-level material-set structure; `MaterialLayer` the layer row (`MaterialId` + `Dimension`-length `Thickness` + name); `MaterialConstituent` the constituent row (`MaterialId` + category + fraction); `ProfileRef` the neutral section-profile reference (`Standard` + `Designation` + content key) a `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalogue; `SectionProperties` the neutral baked section receipt over `Properties/quantity#MEASURE_VALUE` columns.
- Cases: `Single` (one homogeneous `MaterialId` — `IfcMaterial`) · `LayerSet` (a `Seq<MaterialLayer>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`) · `ProfileSet` (one `MaterialId` per extruded `ProfileRef`, members — `IfcMaterialProfileSet`) · `ConstituentSet` (a `Seq<MaterialConstituent>` of fraction-weighted keyword-tagged components, composites — `IfcMaterialConstituentSet`); the closed IFC material-definition family (`IfcMaterialList` deprecated and never admitted), a composition selecting how the material resolves.
- Entry: `MaterialComposition.OfSingle(material)` and `OfProfileSet(material, profile)` are TOTAL constructors (no admission invariant — the `MaterialPropertySet.OfAcoustic`/`OfFire` total shape, never a `Fin` wrapper over a total op, never an `Op` key the body discards); `OfLayerSet(layers, key)` and `OfConstituentSet(constituents, key)` are `Fin<T>` admissions railing `ElementFault.ValueRejected` on an empty set, a non-positive layer thickness, or a constituent fraction set that does not normalize to unity within tolerance; the four are `Of`-prefixed so the factory name never collides with the same-named nested case type (the `MaterialPropertySet.Of*` convention — a bare `Single(...)` static method and a nested `Single` case are one declaration space, a compile collision). `Materials` projects the assigned `MaterialId` set, `PrimaryMaterial` the appearance/structural-default key, `TotalThickness` the layer buildup depth.
- Auto: the invariant-bearing `LayerSet`/`ConstituentSet` cases carry a PRIVATE constructor and an internal `Seed` re-hydration escape (the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage.ProfileSet` and `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` admission shape), so the only public admission is the `Of` factory and an empty/degenerate set is UNREPRESENTABLE — `PrimaryMaterial`'s `OrderByDescending(...).First()` is then total, never a latent throw on an empty layer/constituent set that the prior public positional ctor admitted; `Materials` dispatches the generated `Switch` projecting the `MaterialId` set each case carries; `OfLayerSet` guards each `MaterialLayer.Thickness` positive (the SI metre magnitude of the `Properties/quantity#MEASURE_VALUE` length); `OfConstituentSet` guards each fraction finite and the fraction sum to one within tolerance so a composite mixture normalizes once at construction (the `Rasm.Compute` `AssemblyAggregator` rule-of-mixtures reads the normalized fractions and never re-guards them).
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<string>]`), LanguageExt.Core (`Seq`/`Fin`/`Option`), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter` the `MaterialComposition.CanonicalBytes` projection + the `ProfileRef.Of` content key write through, `ContentAddress.Of` minting the profile key), `Rasm` (the kernel `Op` op-key).
- Growth: the IFC material-definition family is closed at four cases; a new layer attribute is one `MaterialLayer` column, a new constituent keyword one `MaterialConstituent` column; a new structural/fire `SectionProperties` column is one `MeasureValue` field the `Rasm.Materials` section resolver fills and a `Rasm.Compute` design-code check reads; never a fifth composition case, never a per-element-type composition, and never a per-element-type section receipt; a new section catalogue is one `ProfileRef.Standard` token the projector resolves, never a seam edit.
- Boundary: `MaterialComposition` is the ONE composition owner — a per-element-type composition class is the deleted form; the composition is the TYPE-LEVEL set structure only, the occurrence usage binding (`LayerSetUsage` direction/sense/offset, `ProfileSetUsage` cardinal-point/extent) riding the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, so a layer set's geometric usage never duplicates onto the composition; a `ProfileSet` carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section-property type — the seam references no VividOrange, the `Rasm.Materials` projector resolving the `ProfileRef` one-hop and BAKING the neutral `SectionProperties` (`WithSection`) so a structural consumer reads the resolved section once; the `SectionProperties` is the consumer-contract column set the `Rasm.Compute` design-code routes read (`Area`/`Iyy`/`Izz`/`J`/`Iw`/`Wely`/`Welz`/`Wply`/`Wplz`/`AvY`/`AvZ`/radii/`Depth`/`Width`/`HeatedPerimeter`/`AxisDistance`) — the seam carries the baked scalars, never a VividOrange type, and the projector computes the plastic moduli/torsion/warping/shear-area columns the VividOrange polygon solver does not expose (the `Iw` warping constant the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling routes require, never derivable from `J` alone); a `MaterialLayer.Thickness` is a `Properties/quantity#MEASURE_VALUE` `Dimension`-length-checked measure read SI-native through `.Si`, never a bare double; the composition serializes to the IFC 4.3 material-definition family at the `Rasm.Bim` boundary, host-neutral here.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
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
// Of(designation) is the single-token form the Profiles catalogue key composes, Of(standard, designation) the keyed form.
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
// HeatedPerimeter/AxisDistance via OfSi(QuantityType.Length, Dimension.LengthDim, _)) — each column TYPE-tagged so a QTO
// accessor resolves and a section modulus never collides with a volume by content key, never the dimension-anonymous 2-arg
// OfSi(Dimension, _). The Rasm.Materials section resolver
// fills the elastic/inertia/radius/perimeter/bbox columns from the VividOrange polygon solver (Area/MomentOfInertiaYy,Zz/
// ElasticSectionModulusYy,Zz/RadiusOfGyrationYy,Zz/Perimeter/Extends) and COMPUTES the plastic moduli, torsion constant,
// warping constant, and shear areas the polygon solver does not expose — resolved ONCE ABOVE the seam so a Rasm.Compute structural/fire
// runner reads the section (ElementGraph.SectionOf) without re-resolving per call OR admitting VividOrange.
public readonly record struct SectionProperties(
 MeasureValue Area, MeasureValue Iyy, MeasureValue Izz, MeasureValue J, MeasureValue Iw,
 MeasureValue Wely, MeasureValue Welz, MeasureValue Wply, MeasureValue Wplz,
 MeasureValue AvY, MeasureValue AvZ, MeasureValue RadiusOfGyrationMajor, MeasureValue RadiusOfGyrationMinor,
 MeasureValue Depth, MeasureValue Width, MeasureValue HeatedPerimeter, MeasureValue AxisDistance) {
 // EN 1992-1-2 concrete-fire minimum cross-section dimension — derived min(Depth, Width), never a stored column
 // that could drift from the bounding extents the projector sources.
 public MeasureValue LeastDimension => Depth.Si <= Width.Si ? Depth : Width;
 public static readonly SectionProperties Zero = new(
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
  MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero);
}

public readonly record struct MaterialLayer(MaterialId Material, MeasureValue Thickness, string LayerName);

public readonly record struct MaterialConstituent(MaterialId Material, string Category, double Fraction);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record MaterialComposition {
 private MaterialComposition() { }

 // Single / ProfileSet carry no admission invariant — a public positional ctor is safe; the Of-prefixed factories
 // mirror them as TOTAL constructors so the four-factory family is named uniformly without a fake Fin rail.
 public sealed record Single(MaterialId Material) : MaterialComposition;

 public sealed record ProfileSet(MaterialId Material, ProfileRef Profile, Option<SectionProperties> Section) : MaterialComposition {
  // The M7 section bake lives HERE: a sealed record's copy constructor is PRIVATE (reachable only inside the case),
  // so `with` is illegal from the base type — the base WithSection delegates to this method rather than copying a
  // private-ctor record across the type boundary.
  public ProfileSet With(SectionProperties section) => this with { Section = Some(section) };
 }

 // LayerSet / ConstituentSet carry admission invariants — a PRIVATE ctor + internal Seed re-hydration forces every
 // admission through the Of factory (the relation#MaterialUsage.ProfileSet / acoustic#Acoustic shape), so an empty
 // or degenerate set is unrepresentable and PrimaryMaterial's First() is total, never a latent throw.
 public sealed record LayerSet : MaterialComposition {
  public Seq<MaterialLayer> Layers { get; }
  private LayerSet(Seq<MaterialLayer> layers) => Layers = layers;
  internal static LayerSet Seed(Seq<MaterialLayer> layers) => new(layers);
  public double TotalThickness => Layers.Sum(static l => l.Thickness.Si);
 }

 public sealed record ConstituentSet : MaterialComposition {
  public Seq<MaterialConstituent> Constituents { get; }
  private ConstituentSet(Seq<MaterialConstituent> constituents) => Constituents = constituents;
  internal static ConstituentSet Seed(Seq<MaterialConstituent> constituents) => new(constituents);
 }

 public Seq<MaterialId> Materials => Switch(
  single: static s => Seq1(s.Material),
  layerSet: static s => s.Layers.Map(static l => l.Material),
  profileSet: static s => Seq1(s.Material),
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

 public static Fin<MaterialComposition> OfLayerSet(Seq<MaterialLayer> layers, Op key) =>
  layers.IsEmpty || layers.Exists(static l => l.Thickness.Si <= 0.0)
   ? ElementFault.ValueRejected(key, "<layer-set-empty-or-nonpositive-thickness>")
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
- Cases: `Mechanical` (density / Young's modulus / yield strength / ultimate strength as `MeasureValue`, Poisson's ratio + thermal-expansion as guarded dimensionless doubles, the isotropic shear modulus DERIVED `G = E/(2(1+ν))` — `Discipline.Structural`) · `Thermal` (conductivity / specific heat / U-value as `MeasureValue` + vapour-resistance factor μ as a guarded dimensionless double for EN 13788 Glaser condensation — `Discipline.Thermal`) · `Acoustic` (the `Composition/acoustic#ACOUSTIC_FOLDS` banded carrier — `Discipline.Acoustic`) · `Fire` (a `FireRating` reaction class + `SmokeClass`/`DropletClass` sub-class + a `FireResistance` R/E/I rating — `Discipline.Fire`) · `Environmental` (a `MeasurementBasis` declared unit + the per-`LifecycleStage` `StageGwp` band vector that is the ONE GWP store + `RecycledContent`/`EndOfLifeRecovery` fractions + EPD provenance, with the intrinsic `Gwp` (the DERIVED cradle-to-gate A1-A3 read `StageAt(LifecycleStage.A1A3)`, never a parallel stored scalar), `StageAt`, and `WholeLifeGwp` folds — `Discipline.Environmental`) · `Cost` (supply / install / lifecycle per-unit columns over a `Currency` + `MeasurementBasis` — `Discipline.Cost`); a property is a `MaterialPropertySet` case over a `MaterialId`, never a property subtype.
- Entry: `MaterialPropertySet.OfMechanical(density, youngsModulus, yieldStrength, ultimateStrength, poissons, thermalExpansion, key)` / `OfThermal(conductivity, specificHeat, uValue, vapourResistanceFactor, key)` / `OfAcoustic(acoustic)` / `OfFire(rating, resistance)` (+ the full `OfFire(rating, smoke, droplets, resistance)`) / `OfEnvironmental(...)` / `OfCost(...)` — the typed smart-constructors coercing each measured column through `MeasureValue.Of` to its SI base and guarding the dimensionless ratios, `Fin<T>` railing `ElementFault.ValueRejected` on a non-finite or out-of-range column (the total `OfAcoustic`/`OfFire` carry no invariant and return the bare case); `Discipline` reads the case-to-discipline map; the generic `Property<T>()` is the one polymorphic typed lookup, the named per-discipline reads (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) deriving from it as the ergonomic consumer surface the `Rasm.Compute` aggregator reads (`props.Thermal`/`props.Mechanical`/`props.Environmental`), and `ForDiscipline(discipline)` the dual by-discipline-value read.
- Auto: `Discipline` dispatches the generated `Switch` mapping each case to its row (`Mechanical`→`Structural`, …, `Cost`→`Cost`); the `Of` constructors route each dimensioned value through `MeasureValue.Of(value, UnitsNet.Units.X, key)` so the column carries its SI base and `Dimension`, the Poisson's ratio guarded to the physical isotropic `[0,0.5]` range and the fractions finite; the `Mechanical` shear modulus is a DERIVED read off `E` and `ν` (the isotropic relation `G = E/(2(1+ν))`), never a stored column that could drift; the `Acoustic` case wraps the `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` carrier whose `Nrc`/`Saa`/`StcWeighted` are derived reads; the `Fire` case carries the EN 13501-1 reaction class plus its smoke/droplet sub-class and the EN 13501-2 R/E/I `FireResistance`.
- Receipt: a `Seq<MaterialPropertySet>` on a `Material` node is the full engineering profile a `Bake`-derived `Element` reads flat — `props.Thermal.Map(t => t.UValue)`, `props.Mechanical.Map(m => m.YieldStrength)`, `props.Acoustic.Map(a => a.StcWeighted)`, or the generic `props.Property<MaterialPropertySet.Fire>()` for a case with no named read — one node carrying every discipline keyed by `Discipline`; the `Rasm.Compute` analysis route reads the `MeasureValue` columns by `Discipline`, and the assembly aggregation (series-resistance U-value, rule-of-mixtures density, layered STC) folds the `MaterialComposition` plies in Compute, never re-keyed per assembly.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`/`[ValueObject<string>]`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Choose`/`Find`), UnitsNet (via `MeasureValue`), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter` the `MaterialPropertySet.CanonicalBytes` content projection writes through), `Rasm` (the kernel `Op` op-key).
- Growth: a new engineering property shared across materials is one column on its `MaterialPropertySet` case; a new property discipline with no fit is one `MaterialPropertySet` case carrying its `Discipline` — never a parallel `Eco`/`Cost` owner; a new fire-reaction class is one `FireRating` row, a new acoustic rating one fold on the `Acoustic` carrier; the family grows by case and column, never by a per-discipline material type, and the typed lookup grows by ONE generic `Property<T>()` over the case type plus an ergonomic named forward, never six independent `Choose` bodies.
- Boundary: `MaterialPropertySet` is the ONE typed property family — a `StructuralMaterial`/`ThermalMaterial` per-discipline material type is the deleted form, a property being a case over a `MaterialId`; every dimensioned measured column admits through `MeasureValue.Of` to its SI base (the `Density`/`YoungsModulus`/`Conductivity`/`UValue` columns), never a bare double, the dimensionless `PoissonsRatio` guarded to the isotropic `[0,0.5]` so an out-of-range ratio is unrepresentable; the shear modulus is DERIVED from `E`/`ν` (DERIVED_LOGIC — a stored `G` independent of `E`/`ν` on an isotropic owner is the named drift defect; an orthotropic material with an independent measured `G` is a future richer case, never a stored field here); the `FireRating` is a closed reaction-class vocabulary (a non-standard class is a row never a free string) and fire resistance is a typed R/E/I `FireResistance` (a single resistance scalar cannot distinguish a load-bearing `R 90` column from a separating `EI 60` wall — the deleted form); the typed read is the one generic `Property<T>()` over the case type (the six near-identical per-discipline reads DERIVE from it — one `Choose p is T` body, never six — yet stay as the consumer-contract ergonomic surface the `Rasm.Compute` aggregator reads), `ForDiscipline` the dual by-discipline-value read; the `Acoustic` case is the banded `Composition/acoustic#ACOUSTIC_FOLDS` carrier, never a scalar STC; the per-case-to-`Discipline` map is the one correspondence the `Assessment/assessment#ASSESSMENT_NODE` and `Rasm.Compute` analysis route share; the `Cost` case carries neutral per-unit doubles over a `Currency` `[ValueObject<string>]` (an OPAQUE ISO 4217 alpha-3 token the seam shape-validates but never rosters — the SAME neutrality `AnalysisRoute`/`Classification.System` hold, a closed three-row currency enum being the deleted naive slice) + a `MeasurementBasis` declared unit (the seam references no money library — the `Rasm.Bim` `NodaMoney` cost algebra owns the ISO 4217 roster and meets the per-unit double at the quantity×rate join), and the `Environmental` case carries the per-`LifecycleStage` `StageGwp` band vector (the EN 15978 A1-A3/A4/A5/B/C/D modules, the SAME banded-carrier shape as the `Acoustic` case) as the ONE GWP store — the cradle-to-gate `Gwp` is the DERIVED read `StageAt(LifecycleStage.A1A3)` and a parallel stored `GlobalWarmingPotential` `MeasureValue` beside `StageGwp[A1A3.Index]` is the named double-store defect (DERIVED_LOGIC — the same one-owner discipline the `Acoustic` carrier holds, every rating derived from the spectrum), so `OfEnvironmental` guards `StageGwp` FINITE once (`AllFinite`) and the derived `Gwp`/`StageAt`/`WholeLifeGwp` reads then trust the admission; the `WholeLifeGwp` fold sums the vector and the `Rasm.Compute` EC3 embodied-carbon route reads it; every `StageGwp` module is on the case's `MeasurementBasis` (per-m³/per-m²/per-kg/per-item) — the SAME basis axis the `Cost` case carries, so the `Rasm.Compute` `AggregateEnvironmental` fold scales each ply by the basis-matching element quantity through the SAME basis-aware `DeclaredQuantity` derivation the cost fold uses, never a forced per-m³ normalization that demanded a density at ingress and dropped an area/item EPD.

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

// The EN 15978 lifecycle-stage modules the Environmental case bands its StageGwp vector over.
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
 public static readonly int Count = Items.Count;
}

// The procurement currency the Cost case carries — an OPAQUE ISO 4217 alpha-3 token (the SAME neutrality the
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

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record MaterialPropertySet {
 private MaterialPropertySet() { }

 public sealed record Mechanical(MeasureValue Density, MeasureValue YoungsModulus, MeasureValue YieldStrength, MeasureValue UltimateStrength, double PoissonsRatio, double ThermalExpansionPerK) : MaterialPropertySet {
  // Isotropic shear modulus G = E/(2(1+ν)) — a DERIVED read, never a stored column that could drift from E/ν
  // (the seam Mechanical is isotropic: one E, one ν, so G is exact, not an independent datum). PoissonsRatio
  // admits in [0,0.5], so 2(1+ν) ∈ [2,3] and G ∈ [E/3, E/2] is always finite and positive. It carries the Pressure
  // QuantityType (matching YoungsModulus.Type) through the TYPED OfSi overload, never the dimension-anonymous 2-arg form.
  public MeasureValue ShearModulus => MeasureValue.OfSi(QuantityType.Create("Pressure"), Dimension.PressureDim, YoungsModulus.Si / (2.0 * (1.0 + PoissonsRatio)));
 }
 public sealed record Thermal(MeasureValue Conductivity, MeasureValue SpecificHeat, MeasureValue UValue, double VapourResistanceFactor) : MaterialPropertySet;
 public sealed record Acoustic(global::Rasm.Element.Acoustic Spectrum) : MaterialPropertySet {
  // Forwarding reads so the Rasm.Materials marshaller and the Rasm.Compute layered-STC fold read the
  // single-material ratings off the case directly (a.SoundReductionIndexDb / a.StcWeighted), never .Spectrum.x.
  public ReadOnlyMemory<double> AbsorptionSpectrum => Spectrum.AbsorptionSpectrum;
  public ReadOnlyMemory<double> SoundReductionIndexDb => Spectrum.SoundReductionIndexDb;
  public double Nrc => Spectrum.Nrc;
  public double Saa => Spectrum.Saa;
  public int StcWeighted => Spectrum.StcWeighted;
 }
 public sealed record Fire(FireRating Reaction, SmokeClass Smoke, DropletClass Droplets, FireResistance Resistance) : MaterialPropertySet;
 // BASIS-AWARE: Gwp + every StageGwp module are kgCO2e PER the Basis unit (per-m³/per-m²/per-kg/per-item) — the SAME
 // MeasurementBasis the Cost case carries, so the Rasm.Compute AggregateEnvironmental fold scales each ply by the
 // basis-matching element quantity through the SAME basis-aware DeclaredQuantity derivation the cost fold uses
 // (per-m³ → volume, per-m² → face area, per-kg → volume×density, per-item → unit), NOT a forced per-m³ normalization
 // that demanded a density at ingress and SKIPPED an area/item EPD. A baked catalogue declaration is curated PerM3; an
 // EC3-resolved declaration carries the EPD's native declared_unit basis the EC3 ingress tags (Analysis/lifecycle Normalize).
 public sealed record Environmental(MeasurementBasis Basis, ReadOnlyMemory<double> StageGwp, double RecycledContent, double EndOfLifeRecovery, string Epd, int ValidUntilYear) : MaterialPropertySet {
  // DERIVED, never stored: the cradle-to-gate A1-A3 GWP IS the A1A3 stage module — a parallel GlobalWarmingPotential
  // scalar read by Gwp beside StageGwp[A1A3.Index] is a double-store of one fact (DERIVED_LOGIC) the acoustic carrier
  // never admits (every rating derives from the spectrum), so Gwp folds StageAt(A1A3) and the StageGwp vector is the ONE
  // owner. The OfEnvironmental admission guards StageGwp finiteness so this read (and WholeLifeGwp) never surfaces a NaN.
  public double Gwp => StageAt(LifecycleStage.A1A3);                                        // cradle-to-gate A1-A3 (kgCO2e per Basis unit)
  public double StageAt(LifecycleStage stage) => stage.Index < StageGwp.Length ? StageGwp.Span[stage.Index] : 0.0;  // per-module kgCO2e per Basis unit
  public double WholeLifeGwp { get { double total = 0.0; foreach (double m in StageGwp.Span) { total += m; } return total; } }  // cradle-to-grave kgCO2e per Basis unit
  // The zero-impact baseline a Rasm.Compute embodied-carbon fold seeds an element-set GWP rollup from (PerM3, the curated default basis).
  public static readonly Environmental Empty = new(MeasurementBasis.PerM3, new double[LifecycleStage.Count], 0.0, 0.0, "", 0);
 }
 public sealed record Cost(MeasurementBasis Basis, Currency Currency, double SupplyPerUnit, double InstallPerUnit, double LifecyclePerUnit) : MaterialPropertySet;

 public Discipline Discipline => Switch(
  mechanical: static _ => Discipline.Structural,
  thermal: static _ => Discipline.Thermal,
  acoustic: static _ => Discipline.Acoustic,
  fire: static _ => Discipline.Fire,
  environmental: static _ => Discipline.Environmental,
  cost: static _ => Discipline.Cost);

 // The canonical content contribution the Graph/element#NODE_MODEL Node.Material arm folds per property (the bag
 // ordered by Discipline before this projection): the case ordinal then every SCALAR column — MeasureValue measures
 // tolerance-quantized through Measure, dimensionless ratios as exact IEEE-754 through Double, vocabulary rows through
 // their Key, the integer fire-resistance minutes / EPD validity year through Ordinal, the Acoustic spectrum delegating
 // to its OWN banded CanonicalBytes (Composition/acoustic) — so two Material nodes identical in MaterialKey + Composition
 // + disciplines but differing in ANY property scalar (a yield strength, a U-value, one StageGwp module) content-address
 // distinctly and never dedup-collide under the content-keyed NodeId.Content mint. The property VALUES are content, not
 // the discipline keys alone; the per-property write order is fixed by the case ordinal so the projection is self-delimiting.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  mechanical:    m => w.Ordinal(0).Measure(m.Density).Measure(m.YoungsModulus).Measure(m.YieldStrength).Measure(m.UltimateStrength).Double(m.PoissonsRatio).Double(m.ThermalExpansionPerK),
  thermal:       t => w.Ordinal(1).Measure(t.Conductivity).Measure(t.SpecificHeat).Measure(t.UValue).Double(t.VapourResistanceFactor),
  acoustic:      a => { w.Ordinal(2); return a.Spectrum.CanonicalBytes(w); },
  fire:          f => w.Ordinal(3).String(f.Reaction.Key).String(f.Smoke.Key).String(f.Droplets.Key).Ordinal(f.Resistance.LoadBearingMinutes).Ordinal(f.Resistance.IntegrityMinutes).Ordinal(f.Resistance.InsulationMinutes),
  environmental: e => { w.Ordinal(4).String(e.Basis.Key); foreach (double s in e.StageGwp.Span) { w.Double(s); } return w.Double(e.RecycledContent).Double(e.EndOfLifeRecovery).String(e.Epd).Ordinal(e.ValidUntilYear); },
  cost:          c => w.Ordinal(5).String(c.Basis.Key).String(c.Currency.Value).Double(c.SupplyPerUnit).Double(c.InstallPerUnit).Double(c.LifecyclePerUnit));

 public static Fin<MaterialPropertySet> OfMechanical(double density, double youngsModulus, double yieldStrength, double ultimateStrength, double poissons, double thermalExpansion, Op key) =>
  poissons is < 0.0 or > 0.5
   ? ElementFault.ValueRejected(key, $"<poisson-out-of-isotropic-range:{poissons:R}>")
   : from d in MeasureValue.Of(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter, key)
     from e in MeasureValue.Of(youngsModulus, UnitsNet.Units.PressureUnit.Megapascal, key)
     from y in MeasureValue.Of(yieldStrength, UnitsNet.Units.PressureUnit.Megapascal, key)
     from u in MeasureValue.Of(ultimateStrength, UnitsNet.Units.PressureUnit.Megapascal, key)
     select (MaterialPropertySet)new Mechanical(d, e, y, u, poissons, thermalExpansion);

 public static Fin<MaterialPropertySet> OfThermal(double conductivity, double specificHeat, double uValue, double vapourResistanceFactor, Op key) =>
  vapourResistanceFactor < 1.0
   ? ElementFault.ValueRejected(key, $"<vapour-resistance-factor-below-unity:{vapourResistanceFactor:R}>")
   : from c in MeasureValue.Of(conductivity, UnitsNet.Units.ThermalConductivityUnit.WattPerMeterKelvin, key)
     from s in MeasureValue.Of(specificHeat, UnitsNet.Units.SpecificEntropyUnit.JoulePerKilogramKelvin, key)
     from u in MeasureValue.Of(uValue, UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, key)
     select (MaterialPropertySet)new Thermal(c, s, u, vapourResistanceFactor);

 public static MaterialPropertySet OfAcoustic(global::Rasm.Element.Acoustic spectrum) => new Acoustic(spectrum);

 // The 2-arg form defaults the smoke/droplet sub-class (NotSpecified) for a reaction-class-only datasheet; the full
 // form admits the complete EN 13501-1 "B-s1,d0" classification. Both total — a FireRating/FireResistance carry their own admission.
 public static MaterialPropertySet OfFire(FireRating reaction, FireResistance resistance) =>
  new Fire(reaction, SmokeClass.NotSpecified, DropletClass.NotSpecified, resistance);

 public static MaterialPropertySet OfFire(FireRating reaction, SmokeClass smoke, DropletClass droplets, FireResistance resistance) =>
  new Fire(reaction, smoke, droplets, resistance);

 // Every stageGwp module is kgCO2e PER the basis unit — the caller declares the EPD's native MeasurementBasis (the EC3
 // ingress tags the declared_unit basis, the Materials catalogue passes PerM3 for its curated rows), so the Compute
 // AggregateEnvironmental scales each ply by the basis-matching quantity (the SAME basis-aware DeclaredQuantity derivation
 // the cost fold uses). The cradle-to-gate A1-A3 GWP rides the StageGwp[A1A3.Index] slot (the derived Gwp read), NOT a
 // separate GlobalWarmingPotential argument — the prior parallel scalar was a double-store of one fact (DERIVED_LOGIC). CO2e
 // is a domain basis not an SI dimension, so the modules carry raw kgCO2e magnitudes guarded FINITE here (the admission the
 // derived Gwp/StageAt/WholeLifeGwp reads then trust — a NaN module is rejected once, never surfaced through a stage read).
 public static Fin<MaterialPropertySet> OfEnvironmental(MeasurementBasis basis, ReadOnlyMemory<double> stageGwp, double recycledContent, double endOfLifeRecovery, string epd, int validUntilYear, Op key) =>
  stageGwp.Length != LifecycleStage.Count
   ? ElementFault.ValueRejected(key, $"<environmental-stage-arity:{stageGwp.Length}:expected={LifecycleStage.Count}>")
   : !AllFinite(stageGwp.Span)
    ? ElementFault.ValueRejected(key, "<environmental-stage-gwp-non-finite>")
    : recycledContent is < 0.0 or > 1.0 || endOfLifeRecovery is < 0.0 or > 1.0
     ? ElementFault.ValueRejected(key, "<environmental-fraction-out-of-unit>")
     : Fin.Succ<MaterialPropertySet>(new Environmental(basis, stageGwp, recycledContent, endOfLifeRecovery, epd, validUntilYear));

 // The per-module finiteness guard the Environmental admission folds the StageGwp vector through — a span scan
 // (ReadOnlyMemory<double> is a raw BCL receiver, no LanguageExt combinator), the value primitive the lifecycle vector
 // admission shares so the derived Gwp/StageAt/WholeLifeGwp reads never surface a NaN the content hash would also fork on.
 static bool AllFinite(ReadOnlySpan<double> values) {
  foreach (double v in values) { if (!double.IsFinite(v)) { return false; } }
  return true;
 }

 public static Fin<MaterialPropertySet> OfCost(Currency currency, MeasurementBasis basis, double supply, double install, double lifecycle, Op key) =>
  supply < 0.0 || install < 0.0 || lifecycle < 0.0
   ? ElementFault.ValueRejected(key, "<cost-negative-column>")
   : Fin.Succ<MaterialPropertySet>(new Cost(basis, currency, supply, install, lifecycle));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class MaterialPropertyAccess {
 extension(Seq<MaterialPropertySet> properties) {
  // ONE polymorphic typed read — the case TYPE is the discriminant (recoverable from the value per MODAL_ARITY); the
  // six per-discipline reads below DERIVE from this one `Choose p is T` body (the repeated arms collapse to one), and a
  // case with no named read (a future discipline) is read generically via properties.Property<MaterialPropertySet.X>().
  public Option<T> Property<T>() where T : MaterialPropertySet =>
   properties.Choose(static p => p is T t ? Some(t) : None).Head;

  // The ergonomic per-discipline reads the Rasm.Compute aggregator consumes (props.Thermal/Mechanical/Environmental) —
  // each a one-line projection of the generic owner, the consumer-contract surface, never a re-implemented Choose body.
  public Option<MaterialPropertySet.Mechanical> Mechanical => properties.Property<MaterialPropertySet.Mechanical>();
  public Option<MaterialPropertySet.Thermal> Thermal => properties.Property<MaterialPropertySet.Thermal>();
  public Option<MaterialPropertySet.Acoustic> Acoustic => properties.Property<MaterialPropertySet.Acoustic>();
  public Option<MaterialPropertySet.Fire> Fire => properties.Property<MaterialPropertySet.Fire>();
  public Option<MaterialPropertySet.Environmental> Environmental => properties.Property<MaterialPropertySet.Environmental>();
  public Option<MaterialPropertySet.Cost> Cost => properties.Property<MaterialPropertySet.Cost>();

  // The dual by-discipline read a Rasm.Compute analysis route takes when it holds a runtime Discipline value
  // rather than a static case type — dispatching on the case→Discipline map, never a parallel discipline scan.
  public Option<MaterialPropertySet> ForDiscipline(Discipline discipline) => properties.Find(p => p.Discipline == discipline);
 }
}
```

## [04]-[RESEARCH]

- [MATERIAL_COLLAPSE]: the migration source carried TWO parallel material owners — `Rasm.Materials` `MaterialAssignment` (the `LayerSet`/`ProfileSet`/`ConstituentSet` trichotomy) and `MaterialProperty` (the `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost` unions keyed by `MaterialId`) — never joined to an element; this owner collapses both onto the seam `Material` node so one node over one `MaterialId` carries its composition AND its full property profile, the `Rasm.Materials` `MaterialProjector` lowering its subgraph onto `Material` nodes and the `Rasm.Bim` projector reading them at the IFC boundary; the `Single` case is the addition the migration trichotomy lacked (a homogeneous `IfcMaterial`), the four cases now the full IFC material-definition family. The `Of`-prefixed factory family (`OfSingle`/`OfLayerSet`/`OfProfileSet`/`OfConstituentSet`) mirrors the sibling `MaterialPropertySet.Of*` convention so a smart-constructor never shadows its same-named nested case type (a bare `Single(...)` method and a nested `Single` case share one declaration space — a compile collision), and the invariant-bearing `LayerSet`/`ConstituentSet` cases gate admission through a private constructor + internal `Seed` (the `relation#MaterialUsage.ProfileSet` / `acoustic#Acoustic` shape) so a degenerate set is unrepresentable and `PrimaryMaterial`'s `First()` is total.
- [PROFILE_REF_RESOLUTION]: the `ProfileRef` (`Standard` + `Designation` + content key, the content key projected through the ONE `Projection/address#CONTENT_ADDRESS` `CanonicalWriter`) is the M7 one-hop resolution seam — the `Rasm.Materials` projector resolves it ONCE through its `Profiles` catalogue to a section and BAKES the resulting neutral seam `SectionProperties` onto the `ProfileSet` composition (`WithSection`), so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf(member)`) without re-resolving per call OR admitting VividOrange. The `SectionProperties` is the FULL design-code column set the `Rasm.Compute` checks read (a CONSUMER CONTRACT): `Area`/`Iyy`/`Izz`/`Wely`/`Welz`/`RadiusOfGyrationMajor`/`Minor` the `VividOrange.Sections.SectionProperties` polygon solver SOURCES (`Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`), `Depth`/`Width` from its `Extends` bounding extents and `HeatedPerimeter` from its `Perimeter`, plus the plastic moduli (`Wply`/`Wplz`), the St-Venant torsion constant (`J`), the warping constant (`Iw`, the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F lateral-torsional-buckling input the bare `J` cannot supply), and the both-axis shear areas (`AvY`/`AvZ`) the polygon solver does NOT expose — the `Rasm.Materials` section resolver computes those from the section geometry — and the EN 1992-1-2 reinforcement `AxisDistance` cover (the RC section). The `LeastDimension` the EN 1992-1-2 concrete-fire check reads is a DERIVED `min(Depth, Width)`, never a stored column. The asymmetric-section shear-centre offset and monosymmetry parameter (the general-case flexural-torsional-buckling inputs) are a future richer column set the resolver fills when a non-doubly-symmetric section is admitted, never smuggled onto the symmetric baseline. The seam declares the consumer-required shape; sourcing the full column set is the `Rasm.Materials` `ComputedSection` resolver's obligation, never a seam concern.
- [USAGE_ON_EDGE]: the occurrence usage binding (the IFC `IfcMaterialLayerSetUsage` `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and the `IfcMaterialProfileSetUsage` `CardinalPoint`/`ReferenceExtent`) rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, NOT this owner — the `MaterialComposition` is the type-level SET structure shared across occurrences, the usage the per-occurrence geometric binding on the edge, so a layer set's direction never duplicates onto the composition and a wall and its mirror share one `LayerSet` with two `Associate` usages.
- [ISOTROPIC_SHEAR]: the `Mechanical` shear modulus is the DERIVED isotropic relation `G = E/(2(1+ν))`, not a stored column — the seam `Mechanical` models an isotropic material (one `YoungsModulus`, one `PoissonsRatio`), so a stored `G` independent of `E`/`ν` could only ever drift from the relation it must satisfy; `PoissonsRatio` admits in the physical isotropic `[0,0.5]` (rejecting the thermodynamically-impossible `ν > 0.5` the prior `[0,1]` guard allowed), so `2(1+ν) ∈ [2,3]` and `G` is always finite and positive. `UltimateStrength` is a first-class `MeasureValue` column (the `Rasm.Compute` ACI 318 concrete `f'c` and EN 1993 net-section/connection checks read `Strength.UltimateStrength`), so it is not derivable from yield and stays stored; an orthotropic material (timber along/across grain) carries an INDEPENDENT measured `G` and is a future richer `MaterialPropertySet` case, never a stored field smuggled onto the isotropic owner.
- [FIRE_CLASSIFICATION]: a material's fire performance is two EN standards, not one scalar — EN 13501-1 reaction-to-fire (the `FireRating` Euroclass `A1`…`F` plus the `SmokeClass` `s1`…`s3` and `DropletClass` `d0`…`d2` sub-classes that complete a "B-s1,d0" classification) and EN 13501-2 fire resistance (the `FireResistance` R load-bearing / E integrity / I insulation criteria, each an independent minute rating, so a load-bearing `R 90` column, a separating `EI 60` wall, and a load-bearing separating `REI 90` wall are distinct); a single resistance scalar conflating R/E/I is the deleted form, and `FireResistance.Rei`/`R`/`Ei` are the convenience constructors for the common ratings.
- [TYPED_LOOKUP_COLLAPSE]: the per-case property reads collapse to ONE generic `Property<T>()` over the case type — the six `Choose(p => p is T t ? Some(t) : None).Head` bodies a per-discipline reader would enumerate share one generative structure, so the body lives ONCE on the generic owner and the named per-discipline accessors (`props.Mechanical`/`props.Thermal`/…, the consumer-contract surface the `Rasm.Compute` aggregator reads) are one-line projections of it, never six re-implemented `Choose` bodies; `ForDiscipline(discipline)` is the orthogonal dual (a runtime `Discipline` value rather than a static case type), the `Rasm.Compute` analysis route reading a discipline's property set through whichever entry it holds.
