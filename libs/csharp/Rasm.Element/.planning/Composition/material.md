# [ELEMENT_MATERIAL]

The host-neutral material owner: the `MaterialId` `[ValueObject<string>]` a `Material` node keys on, one `MaterialComposition` `[Union]` closing the type-level material-set structure (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), and one `MaterialPropertySet` `[Union]` closing the typed engineering-property family (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) keyed to the one `Classification/classification#DISCIPLINE_AXIS` `Discipline`. A material is a FULL engineering object: a `Material` node carries its composition and its property sets over one `MaterialId`, so a consumer reads a material's U-value, sound spectrum, fire rating, structural grade, embodied carbon, and cost from one node — never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface, never a per-discipline material type. This is the seam home the migration source's two parallel owners collapse onto: the `Rasm.Materials` `MaterialAssignment` trichotomy and its `MaterialProperty` unions become the seam `MaterialComposition` and `MaterialPropertySet`, the `Rasm.Materials` `MaterialProjector` lowering its material subgraph onto `Material` nodes and the `Rasm.Bim` projector reading them. The occurrence usage binding (layer direction/offset, profile cardinal point) is NOT here — it rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, this owner carrying only the type-level SET structure. The page composes `Properties/quantity#MEASURE_VALUE` for every measured column, `Composition/acoustic#ACOUSTIC_FOLDS` for the `Acoustic` case, and `Classification/classification#DISCIPLINE_AXIS` for the property-to-discipline key; a non-finite or out-of-range admission rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page references NO VividOrange — a `ProfileSet` carries a neutral `ProfileRef` PLUS the neutral `SectionProperties` the `Rasm.Materials` projector resolves ONE-HOP (M7) and BAKES on (`WithSection`), so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf`) without re-resolving or admitting VividOrange.

## [01]-[INDEX]

- [01]-[MATERIAL_COMPOSITION]: the `MaterialId` key, the `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), the `MaterialLayer`/`MaterialConstituent` rows, and the `ProfileRef` neutral section reference.
- [02]-[MATERIAL_PROPERTY]: the `MaterialPropertySet` `[Union]` (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) keyed to `Discipline`, the `FireRating` reaction-class vocabulary, the `Of` admissions, and the typed lookup accessors.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialId` the `[ValueObject<string>]` material-identity key a `Material` node carries; `MaterialComposition` the `[Union]` type-level material-set structure; `MaterialLayer` the layer row (`MaterialId` + `Dimension` thickness + name); `MaterialConstituent` the constituent row (`MaterialId` + category + fraction); `ProfileRef` the neutral section-profile reference (`Standard` + `Designation` + content key) a `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalog.
- Cases: `Single` (one homogeneous `MaterialId` — `IfcMaterial`) · `LayerSet` (a `Seq<MaterialLayer>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`) · `ProfileSet` (one `MaterialId` per extruded `ProfileRef`, members — `IfcMaterialProfileSet`) · `ConstituentSet` (a `Seq<MaterialConstituent>` of fraction-weighted keyword-tagged components, composites — `IfcMaterialConstituentSet`); the closed IFC material-definition family (`IfcMaterialList` deprecated and never admitted), a composition selecting how the material resolves.
- Entry: `MaterialComposition.Single(material, key)` · `LayerSet(layers, key)` · `ProfileSet(material, profile, key)` · `ConstituentSet(constituents, key)` — the four smart-constructors discriminating the composition shape, each `Fin<T>` railing `ElementFault.ValueRejected` on an empty set, a non-positive layer thickness, or a constituent fraction set that does not sum to one within tolerance; `Materials` projects the assigned `MaterialId` set, `TotalThickness` the layer buildup depth.
- Auto: `Materials` dispatches the generated `Switch` projecting the `MaterialId` set each case carries (a `Single` one, a `LayerSet` its layers', a `ProfileSet` its one, a `ConstituentSet` its constituents'); `LayerSet` admission guards each `MaterialLayer.ThicknessMm` positive through the `Properties/quantity#MEASURE_VALUE` `Dimension` length check; `ConstituentSet` admission guards the fraction sum to one within tolerance so a composite normalizes once at construction.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<string>]`), LanguageExt.Core (`Seq`/`Fin`), `Rasm` (the kernel `Op` op-key).
- Growth: the IFC material-definition family is closed at four cases; a new layer attribute is one `MaterialLayer` column, a new constituent keyword one `MaterialConstituent` column; never a fifth composition case and never a per-element-type composition; a new section catalog is one `ProfileRef.Standard` token the projector resolves, never a seam edit.
- Boundary: `MaterialComposition` is the ONE composition owner — a per-element-type composition class is the deleted form; the composition is the TYPE-LEVEL set structure only, the occurrence usage binding (`LayerSetUsage` direction/sense/offset, `ProfileSetUsage` cardinal-point/extent) riding the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, so a layer set's geometric usage never duplicates onto the composition; a `ProfileSet` carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section-property type — the seam references no VividOrange, the `Rasm.Materials` projector resolving the `ProfileRef` one-hop to the catalog and caching the section properties so a structural consumer reads the resolved section once; a `MaterialLayer.ThicknessMm` is a `Properties/quantity#MEASURE_VALUE` `Dimension`-length-checked measure, never a bare double; the composition serializes to the IFC 4.3 material-definition family at the `Rasm.Bim` boundary, host-neutral here.

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
 static partial void NormalizeValidate(ref string value) => value = value.Trim();
 public static MaterialId Of(string value) => Create(value);   // the codebase .Of factory the cross-package catalogues key on
}

// A neutral section-profile reference: a catalogue designation (+ optional standard + content key) the
// Rasm.Materials projector resolves ONE-HOP (M7) to the neutral SectionProperties it BAKES onto the ProfileSet
// composition, so a Rasm.Compute structural consumer reads graph.SectionOf(member) without re-resolving or
// admitting VividOrange. Of(designation) is the single-token factory a Profiles catalogue key composes.
public readonly record struct ProfileRef(string Standard, string Designation, UInt128 ContentKey) {
 public static ProfileRef Of(string designation) =>
 new("", designation, ContentAddress.Of(System.Text.Encoding.UTF8.GetBytes(designation)).Value);
}

// The neutral section-property receipt the Rasm.Materials projector BAKES onto a ProfileSet composition (M7):
// the full structural-design column set — area, both-axis second moment of area + torsion constant, elastic AND
// plastic section moduli, shear area, both-axis radii of gyration, the bounding depth/width, the fire heated
// perimeter, the least dimension, and the EN 1992-1-2 reinforcement AxisDistance (the cover from the exposed concrete
// face to the centroid of the main reinforcement — Zero for a steel/timber section that carries no rebar) — each a
// dimensioned MeasureValue (constructed SI-native via MeasureValue.OfSi), resolved ONCE from the VividOrange catalogue
// ABOVE the seam so a Rasm.Compute structural/fire runner reads the section (ElementGraph.SectionOf) without
// re-resolving per call OR admitting VividOrange; the seam carries the baked scalars, never a VividOrange type.
public readonly record struct SectionProperties(
 MeasureValue Area, MeasureValue Iyy, MeasureValue Izz, MeasureValue J,
 MeasureValue Wely, MeasureValue Welz, MeasureValue Wply, MeasureValue Wplz,
 MeasureValue AvY, MeasureValue RadiusOfGyrationMajor, MeasureValue RadiusOfGyrationMinor,
 MeasureValue Depth, MeasureValue Width, MeasureValue HeatedPerimeter, MeasureValue LeastDimension,
 MeasureValue AxisDistance) {
 public static readonly SectionProperties Zero = new(
 MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
 MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
 MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero,
 MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero, MeasureValue.Zero);
}

public readonly record struct MaterialLayer(MaterialId Material, MeasureValue ThicknessMm, string LayerName);

public readonly record struct MaterialConstituent(MaterialId Material, string Category, double Fraction);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record MaterialComposition {
 private MaterialComposition() { }

 public sealed record Single(MaterialId Material) : MaterialComposition;
 public sealed record LayerSet(Seq<MaterialLayer> Layers) : MaterialComposition {
 public double TotalThickness => Layers.Sum(static l => l.ThicknessMm.Si);
 }
 public sealed record ProfileSet(MaterialId Material, ProfileRef Profile, Option<SectionProperties> Section) : MaterialComposition;
 public sealed record ConstituentSet(Seq<MaterialConstituent> Constituents) : MaterialComposition;

 public Seq<MaterialId> Materials => Switch(
 single: static s => Seq1(s.Material),
 layerSet: static s => s.Layers.Map(static l => l.Material),
 profileSet: static s => Seq1(s.Material),
 constituentSet: static s => s.Constituents.Map(static c => c.Material));

 // The principal material an occurrence keys appearance/structural defaults on: the single/profile material,
 // the thickest layer, or the largest-fraction constituent — one MaterialId, never a parallel "primary" flag.
 public MaterialId PrimaryMaterial => Switch(
 single: static s => s.Material,
 layerSet: static s => s.Layers.OrderByDescending(static l => l.ThicknessMm.Si).First().Material,
 profileSet: static s => s.Material,
 constituentSet: static s => s.Constituents.OrderByDescending(static c => c.Fraction).First().Material);

 // The M7 bake: the Rasm.Materials projector resolves the ProfileRef one-hop and stamps the neutral section onto
 // a ProfileSet composition, so a structural consumer reads it through graph.SectionOf without re-resolving.
 public MaterialComposition WithSection(SectionProperties section) =>
 this is ProfileSet ps ? ps with { Section = Some(section) } : this;

 // The canonical projection through the Projection/address#CONTENT_ADDRESS writer: case ordinal then
 // the typed set members in declared order, layer thicknesses quantized through MeasureValue.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
 single: s => w.Ordinal(0).String(s.Material.Value),
 layerSet: s => { w.Ordinal(1).Ordinal(s.Layers.Count); foreach (var l in s.Layers) { w.String(l.Material.Value).Measure(l.ThicknessMm).String(l.LayerName); } return w; },
 profileSet: s => w.Ordinal(2).String(s.Material.Value).String(s.Profile.Standard).String(s.Profile.Designation).U128(s.Profile.ContentKey),
 constituentSet: s => { w.Ordinal(3).Ordinal(s.Constituents.Count); foreach (var c in s.Constituents) { w.String(c.Material.Value).String(c.Category).Double(c.Fraction); } return w; });

 const double FractionTolerance = 1e-3;

 public static Fin<MaterialComposition> Single(MaterialId material, Op key) => Fin.Succ<MaterialComposition>(new Single(material));

 public static Fin<MaterialComposition> LayerSet(Seq<MaterialLayer> layers, Op key) =>
 layers.IsEmpty || layers.Exists(static l => l.ThicknessMm.Si <= 0.0)
 ? ElementFault.ValueRejected(key, "<layer-set-empty-or-nonpositive-thickness>")
 : Fin.Succ<MaterialComposition>(new LayerSet(layers));

 public static Fin<MaterialComposition> ProfileSet(MaterialId material, ProfileRef profile, Op key) =>
 Fin.Succ<MaterialComposition>(new ProfileSet(material, profile, Option<SectionProperties>.None));

 public static Fin<MaterialComposition> ConstituentSet(Seq<MaterialConstituent> constituents, Op key) =>
 constituents.IsEmpty
 ? ElementFault.ValueRejected(key, "<constituent-set-empty>")
 : Math.Abs(constituents.Sum(static c => c.Fraction) - 1.0) > FractionTolerance
 ? ElementFault.ValueRejected(key, $"<constituent-fraction-not-normalized:{constituents.Sum(static c => c.Fraction):R}>")
 : Fin.Succ<MaterialComposition>(new ConstituentSet(constituents));
}
```

## [03]-[MATERIAL_PROPERTY]

- Owner: `MaterialPropertySet` the `[Union]` typed engineering-property family keyed to `Discipline`; `FireRating` the `[SmartEnum<string>]` reaction-to-fire class; the `Of` admissions coercing each measured column through `Properties/quantity#MEASURE_VALUE`.
- Cases: `Mechanical` (density / Young's modulus / shear modulus / yield strength / ultimate strength as `MeasureValue`, Poisson's ratio + thermal-expansion as guarded dimensionless doubles — `Discipline.Structural`) · `Thermal` (conductivity / specific heat / U-value as `MeasureValue` + vapour-resistance factor μ as a guarded dimensionless double for EN 13788 Glaser condensation — `Discipline.Thermal`) · `Acoustic` (the `Composition/acoustic#ACOUSTIC_FOLDS` banded carrier — `Discipline.Acoustic`) · `Fire` (a `FireRating` reaction class + resistance minutes — `Discipline.Fire`) · `Environmental` (a `MeasurementBasis` declared unit + the cradle-to-gate GWP `MeasureValue` + the per-`LifecycleStage` `StageGwp` band vector + `RecycledContent`/`EndOfLifeRecovery` fractions + EPD provenance, with the intrinsic `WholeLifeGwp`/`StageAt` folds — `Discipline.Environmental`) · `Cost` (supply / install / lifecycle per-unit columns over a `Currency` + `MeasurementBasis` — `Discipline.Cost`); a property is a `MaterialPropertySet` case over a `MaterialId`, never a property subtype.
- Entry: `MaterialPropertySet.OfMechanical(...)` / `OfThermal(...)` / `OfAcoustic(acoustic)` / `OfFire(rating, minutes)` / `OfEnvironmental(...)` / `OfCost(...)` — the typed smart-constructors coercing each measured column through `MeasureValue.Of` to its SI base and guarding the dimensionless ratios, `Fin<T>` railing `ElementFault.ValueRejected` on a non-finite or out-of-range column; `Discipline` reads the case-to-discipline map; the typed lookup accessors (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`) project a case from a `Material` node's property set so a consumer (the `Rasm.Compute` aggregator) composes them seam-direct rather than re-deriving an `is`-cast.
- Auto: `Discipline` dispatches the generated `Switch` mapping each case to its row (`Mechanical`→`Structural`, `Thermal`→`Thermal`, `Acoustic`→`Acoustic`, `Fire`→`Fire`, `Environmental`→`Environmental`, `Cost`→`Cost`); the `Of` constructors route each dimensioned value through `MeasureValue.Of(value, unit, key)` so the column carries its SI base and `Dimension`, the Poisson's ratio guarded to `[0,1]` and the fractions finite; the `Acoustic` case wraps the `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic` carrier whose `Nrc`/`Saa`/`StcWeighted` are derived reads.
- Receipt: a `Seq<MaterialPropertySet>` on a `Material` node is the full engineering profile a `Bake`-derived `Element` reads flat — `material.Thermal.Map(t => t.UValue)`, `material.Mechanical.Map(m => m.YieldStrength)`, `material.Acoustic.Map(a => a.StcWeighted)` — one node carrying every discipline keyed by `Discipline`; the `Rasm.Compute` analysis route reads the `MeasureValue` columns by `Discipline`, and the assembly aggregation (series-resistance U-value, rule-of-mixtures density, layered STC) folds the `MaterialComposition` plies in Compute, never re-keyed per assembly.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`), LanguageExt.Core (`Seq`/`Option`/`Fin`), UnitsNet (via `MeasureValue`), `Rasm` (the kernel `Op` op-key).
- Growth: a new engineering property shared across materials is one column on its `MaterialPropertySet` case; a new property discipline with no fit is one `MaterialPropertySet` case carrying its `Discipline` — never a parallel `Eco`/`Cost` owner; a new fire-reaction class is one `FireRating` row, a new acoustic rating one fold on the `Acoustic` carrier; the family grows by case and column, never by a per-discipline material type.
- Boundary: `MaterialPropertySet` is the ONE typed property family — a `StructuralMaterial`/`ThermalMaterial` per-discipline material type is the deleted form, a property being a case over a `MaterialId`; every measured column admits through `MeasureValue.Of` to its SI base (the `Density`/`YoungsModulus`/`Conductivity`/`UValue`/`Gwp` columns), never a bare double, the dimensionless `PoissonsRatio` guarded `[0,1]` so an out-of-range ratio is unrepresentable, the `FireRating` a closed reaction-class vocabulary so a non-standard class is a row never a free string; the `Acoustic` case is the banded `Composition/acoustic#ACOUSTIC_FOLDS` carrier, never a scalar STC; the per-case-to-`Discipline` map is the one correspondence the `Assessment/assessment#ASSESSMENT_NODE` and `Rasm.Compute` analysis route share, so the property family and the assessment family key on one axis; the `Cost` case carries neutral per-unit doubles over a `Currency` `[SmartEnum<string>]` + a `MeasurementBasis` declared unit (the seam references no money library — the `Rasm.Bim` `NodaMoney` cost algebra meets the per-unit double at the quantity×rate join), and the `Environmental` case carries the cradle-to-gate `Gwp` `MeasureValue` plus the per-`LifecycleStage` `StageGwp` band vector (the EN 15978 A1-A3/A4/A5/B/C/D modules, the SAME banded-carrier shape as the `Acoustic` case) the intrinsic `WholeLifeGwp` fold sums and the `Rasm.Compute` EC3 embodied-carbon route reads; the `Gwp` and every `StageGwp` module are on the case's `MeasurementBasis` (per-m³/per-m²/per-kg/per-item) — the SAME basis axis the `Cost` case carries, so the `Rasm.Compute` `AggregateEnvironmental` fold scales each ply by the basis-matching element quantity through the SAME basis-aware `DeclaredQuantity` derivation the cost fold uses, never a forced per-m³ normalization that demanded a density at ingress and dropped an area/item EPD; a baked catalogue declaration is curated `PerM3`, an EC3-resolved declaration carries the EPD's native `declared_unit` basis the `Rasm.Compute` EC3 ingress tags, both folding correctly under the basis-aware scale.

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

// The procurement money + declared-unit axes the Cost case carries — a non-standard token is a row, never a free string.
[SmartEnum<string>]
public sealed partial class Currency {
 public static readonly Currency Usd = new("USD");
 public static readonly Currency Eur = new("EUR");
 public static readonly Currency Gbp = new("GBP");
 public static Fin<Currency> Parse(string code, Op key) =>
 TryGet(code, out Currency? c) && c is { } v ? Fin.Succ(v) : ElementFault.ValueRejected(key, $"<currency-unknown:{code}>");
}

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

 public sealed record Mechanical(MeasureValue Density, MeasureValue YoungsModulus, MeasureValue ShearModulus, MeasureValue YieldStrength, MeasureValue UltimateStrength, double PoissonsRatio, double ThermalExpansionPerK) : MaterialPropertySet;
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
 public sealed record Fire(FireRating Reaction, double ResistanceMinutes) : MaterialPropertySet;
 // BASIS-AWARE: Gwp + every StageGwp module are kgCO2e PER the Basis unit (per-m³/per-m²/per-kg/per-item) — the SAME
 // MeasurementBasis the Cost case carries, so the Rasm.Compute AggregateEnvironmental fold scales each ply by the
 // basis-matching element quantity through the SAME basis-aware DeclaredQuantity derivation the cost fold uses
 // (per-m³ → volume, per-m² → face area, per-kg → volume×density, per-item → unit), NOT a forced per-m³ normalization
 // that demanded a density at ingress and SKIPPED an area/item EPD. A baked catalogue declaration is curated PerM3; an
 // EC3-resolved declaration carries the EPD's native declared_unit basis the EC3 ingress tags (Analysis/lifecycle Normalize).
 public sealed record Environmental(MeasurementBasis Basis, MeasureValue GlobalWarmingPotential, ReadOnlyMemory<double> StageGwp, double RecycledContent, double EndOfLifeRecovery, string Epd, int ValidUntilYear) : MaterialPropertySet {
 public double Gwp => GlobalWarmingPotential.Si;                                          // cradle-to-gate A1-A3 (kgCO2e per Basis unit)
 public double StageAt(LifecycleStage stage) => stage.Index < StageGwp.Length ? StageGwp.Span[stage.Index] : 0.0;  // per-module kgCO2e per Basis unit
 public double WholeLifeGwp { get { double total = 0.0; foreach (double m in StageGwp.Span) { total += m; } return total; } }  // cradle-to-grave kgCO2e per Basis unit
 // The zero-impact baseline a Rasm.Compute embodied-carbon fold seeds an element-set GWP rollup from (PerM3, the curated default basis).
 public static readonly Environmental Empty = new(MeasurementBasis.PerM3, MeasureValue.Zero, new double[LifecycleStage.Count], 0.0, 0.0, "", 0);
 }
 public sealed record Cost(MeasurementBasis Basis, Currency Currency, double SupplyPerUnit, double InstallPerUnit, double LifecyclePerUnit) : MaterialPropertySet;

 public Discipline Discipline => Switch(
 mechanical: static _ => Discipline.Structural,
 thermal: static _ => Discipline.Thermal,
 acoustic: static _ => Discipline.Acoustic,
 fire: static _ => Discipline.Fire,
 environmental: static _ => Discipline.Environmental,
 cost: static _ => Discipline.Cost);

 public static Fin<MaterialPropertySet> OfMechanical(double density, double youngsModulus, double shearModulus, double yieldStrength, double ultimateStrength, double poissons, double thermalExpansion, Op key) =>
 poissons is < 0.0 or > 1.0
 ? ElementFault.ValueRejected(key, $"<poisson-out-of-unit:{poissons:R}>")
 : from d in MeasureValue.Of(density, UnitsNet.Units.DensityUnit.KilogramPerCubicMeter, key)
 from e in MeasureValue.Of(youngsModulus, UnitsNet.Units.PressureUnit.Megapascal, key)
 from g in MeasureValue.Of(shearModulus, UnitsNet.Units.PressureUnit.Megapascal, key)
 from y in MeasureValue.Of(yieldStrength, UnitsNet.Units.PressureUnit.Megapascal, key)
 from u in MeasureValue.Of(ultimateStrength, UnitsNet.Units.PressureUnit.Megapascal, key)
 select (MaterialPropertySet)new Mechanical(d, e, g, y, u, poissons, thermalExpansion);

 public static Fin<MaterialPropertySet> OfThermal(double conductivity, double specificHeat, double uValue, double vapourResistanceFactor, Op key) =>
 vapourResistanceFactor < 1.0
 ? ElementFault.ValueRejected(key, $"<vapour-resistance-factor-below-unity:{vapourResistanceFactor:R}>")
 : from c in MeasureValue.Of(conductivity, UnitsNet.Units.ThermalConductivityUnit.WattPerMeterKelvin, key)
 from s in MeasureValue.Of(specificHeat, UnitsNet.Units.SpecificEntropyUnit.JoulePerKilogramKelvin, key)
 from u in MeasureValue.Of(uValue, UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, key)
 select (MaterialPropertySet)new Thermal(c, s, u, vapourResistanceFactor);

 public static MaterialPropertySet OfAcoustic(global::Rasm.Element.Acoustic spectrum) => new Acoustic(spectrum);

 public static MaterialPropertySet OfFire(FireRating reaction, double resistanceMinutes) => new Fire(reaction, resistanceMinutes);

 // gwpKgCo2e + every stageGwp module are kgCO2e PER the basis unit — the caller declares the EPD's native
 // MeasurementBasis (the EC3 ingress tags the declared_unit basis, the Materials catalogue passes PerM3 for its curated
 // rows), so the Compute AggregateEnvironmental scales each ply by the basis-matching quantity (the SAME basis-aware
 // DeclaredQuantity derivation the cost fold uses); the Mass coercion carries the kgCO2e magnitude only (CO2e is a domain basis, not an SI dimension).
 public static Fin<MaterialPropertySet> OfEnvironmental(MeasurementBasis basis, double gwpKgCo2e, ReadOnlyMemory<double> stageGwp, double recycledContent, double endOfLifeRecovery, string epd, int validUntilYear, Op key) =>
 stageGwp.Length != LifecycleStage.Count
 ? ElementFault.ValueRejected(key, $"<environmental-stage-arity:{stageGwp.Length}:expected={LifecycleStage.Count}>")
 : recycledContent is < 0.0 or > 1.0 || endOfLifeRecovery is < 0.0 or > 1.0
 ? ElementFault.ValueRejected(key, "<environmental-fraction-out-of-unit>")
 : MeasureValue.Of(gwpKgCo2e, UnitsNet.Units.MassUnit.Kilogram, key).Map(g => (MaterialPropertySet)new Environmental(basis, g, stageGwp, recycledContent, endOfLifeRecovery, epd, validUntilYear));

 public static Fin<MaterialPropertySet> OfCost(Currency currency, MeasurementBasis basis, double supply, double install, double lifecycle, Op key) =>
 supply < 0.0 || install < 0.0 || lifecycle < 0.0
 ? ElementFault.ValueRejected(key, "<cost-negative-column>")
 : Fin.Succ<MaterialPropertySet>(new Cost(basis, currency, supply, install, lifecycle));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class MaterialPropertyAccess {
 extension(Seq<MaterialPropertySet> properties) {
 public Option<MaterialPropertySet.Mechanical> Mechanical => properties.Choose(static p => p is MaterialPropertySet.Mechanical m ? Some(m) : None).HeadOrNone();
 public Option<MaterialPropertySet.Thermal> Thermal => properties.Choose(static p => p is MaterialPropertySet.Thermal t ? Some(t) : None).HeadOrNone();
 public Option<MaterialPropertySet.Acoustic> Acoustic => properties.Choose(static p => p is MaterialPropertySet.Acoustic a ? Some(a) : None).HeadOrNone();
 public Option<MaterialPropertySet.Fire> Fire => properties.Choose(static p => p is MaterialPropertySet.Fire f ? Some(f) : None).HeadOrNone();
 public Option<MaterialPropertySet.Environmental> Environmental => properties.Choose(static p => p is MaterialPropertySet.Environmental e ? Some(e) : None).HeadOrNone();
 public Option<MaterialPropertySet.Cost> Cost => properties.Choose(static p => p is MaterialPropertySet.Cost c ? Some(c) : None).HeadOrNone();
 public Option<MaterialPropertySet> ForDiscipline(Discipline discipline) => properties.Find(p => p.Discipline == discipline);
 }
}
```

## [04]-[RESEARCH]

- [MATERIAL_COLLAPSE]: the migration source carried TWO parallel material owners — `Rasm.Materials` `MaterialAssignment` (the `LayerSet`/`ProfileSet`/`ConstituentSet` trichotomy) and `MaterialProperty` (the `Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost` unions keyed by `MaterialId`) — never joined to an element; this owner collapses both onto the seam `Material` node so one node over one `MaterialId` carries its composition AND its full property profile, the `Rasm.Materials` `MaterialProjector` lowering its subgraph onto `Material` nodes and the `Rasm.Bim` projector reading them at the IFC boundary; the `Single` case is the addition the migration trichotomy lacked (a homogeneous `IfcMaterial`), the four cases now the full IFC material-definition family.
- [PROFILE_REF_RESOLUTION]: the `ProfileRef` (`Standard` + `Designation` + content key, minted by `ProfileRef.Of(designation)`) is the M7 one-hop resolution seam — the `Rasm.Materials` projector resolves it ONCE through its `Profiles` catalogue to a section and BAKES the resulting neutral seam `SectionProperties` onto the `ProfileSet` composition (`WithSection`), so a `Rasm.Compute` structural consumer reads the section off the seam graph (`ElementGraph.SectionOf(member)`) without re-resolving per call OR admitting VividOrange; the seam itself references no VividOrange — it carries the neutral `ProfileRef` and the baked neutral `SectionProperties`, the catalogue resolution living above the seam where the VividOrange package is admitted.
- [USAGE_ON_EDGE]: the occurrence usage binding (the IFC `IfcMaterialLayerSetUsage` `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and the `IfcMaterialProfileSetUsage` `CardinalPoint`/`ReferenceExtent`) rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, NOT this owner — the `MaterialComposition` is the type-level SET structure shared across occurrences, the usage the per-occurrence geometric binding on the edge, so a layer set's direction never duplicates onto the composition and a wall and its mirror share one `LayerSet` with two `Associate` usages.
