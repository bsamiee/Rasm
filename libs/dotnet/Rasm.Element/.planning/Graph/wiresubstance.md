# [ELEMENT_WIRE_SUBSTANCE]

`WireCodec`'s material plane projects generated composition, property-set, curve, acoustic, fire, and environmental families. Native `MaterialUsage` remains relationship-local and has no generated projection.

## [01]-[INDEX]

- [02]-[SUBSTANCE_CODEC]: composition/property-set folds, the section table codec, and accumulating re-admissions.

## [02]-[SUBSTANCE_CODEC]

- Cases: `MaterialComposition` 4 arms and `MaterialPropertySet` 12 arms — census rows [04]/[05] at `Graph/wire#NODE_CODEC`.
- Law: descriptor validation owns corpus constraints; the inverse then re-crosses domain admissions and preserves explicit absence.
- Law: the acoustic and environmental runs are semantic-keyed tensors, so wire order is irrelevant and exact closed coverage is required.
- Law: `Thermal.UValue` is valid product evidence but has no substance-wire field; `Fin` encode refuses `Some` before lowering.
- Packages: Google.Protobuf, Mapperly, NodaTime.Serialization.Protobuf, LanguageExt, and Thinktecture compose the generated support closure coordinated at `Graph/wire#NODE_CODEC`.
- Growth: a new column is one append-only corpus field and one transcription member; a new union case also updates the `CrossingFamily` arm count so the parity census rejects a half-landed pair.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class WireCodec {
 // --- [CASE_TRANSCRIPTIONS]
 internal static Fin<MaterialWire> ToWire(Node.Material node, Op key) =>
  toSeq(node.Properties).TraverseM(set => ToWire(set, key)).As().Map(properties => {
   MaterialWire wire = new() {
    MaterialKey = node.MaterialKey.Value,
    Composition = ToWire(node.Composition),
   };
   wire.PropertySets.AddRange(properties);
   return wire;
  });

 [MapperIgnoreSource(nameof(SectionProperties.LeastDimension))]
 [MapperIgnoreSource(nameof(SectionProperties.IsDoublySymmetric))]
 internal static partial SectionPropertiesWire ToWire(SectionProperties section);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.ShearModulus))]
 internal static partial MechanicalWire ToWire(MaterialPropertySet.Mechanical set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.E005))]
 internal static partial OrthotropicWire ToWire(MaterialPropertySet.Orthotropic set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.UValue))]
 internal static partial ThermalWire ToWire(MaterialPropertySet.Thermal set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Cost.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Cost.Discipline))]
 internal static partial CostWire ToWire(MaterialPropertySet.Cost set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Durability.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Durability.Discipline))]
 internal static partial DurabilityWire ToWire(MaterialPropertySet.Durability set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.SolarAbsorptanceFront))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.SolarAbsorptanceBack))]
 internal static partial OpticalWire ToWire(MaterialPropertySet.Optical set);

 [UserMapping] internal static FireWire ToWire(MaterialPropertySet.Fire set) {
  FireWire w = new() { Resistance = ToWire(set.Resistance) };
  set.Reaction.IfSome(reaction => w.Reaction = ToWire(reaction));
  if (set.Suffix.Smoke.Length > 0) { w.Smoke = ToSmokeClass(set.Suffix.Smoke); }
  if (set.Suffix.Droplets.Length > 0) { w.Droplets = ToDropletClass(set.Suffix.Droplets); }
  return w;
 }

 [UserMapping] internal static FireResistanceWire ToWire(FireResistance resistance) {
  FireResistanceWire w = new(); resistance.LoadBearingMinutes.IfSome(m => w.LoadBearingMinutes = m); resistance.IntegrityMinutes.IfSome(m => w.IntegrityMinutes = m); resistance.InsulationMinutes.IfSome(m => w.InsulationMinutes = m); return w;
 }

 [UserMapping] internal static EnvironmentalWire ToWire(MaterialPropertySet.Environmental set) {
  EnvironmentalWire w = new() { Basis = ToWire(set.Basis) };
  w.Impacts.AddRange(ImpactCategory.Items.OrderBy(static row => row.Key).SelectMany(category =>
   LifecycleStage.Items.OrderBy(static row => row.Key).Select(stage => new BandCellWire {
    Category = (Rasm.Contracts.Declaration.ImpactCategory)(category.Key + 1),
    Band = (Rasm.Contracts.Element.LifecycleBand)(stage.Key + 1),
    Value = set.IndicatorAt(category, stage),
   })));
  set.RecycledContent.IfSome(v => w.RecycledContent = v);
  set.EndOfLifeRecovery.IfSome(v => w.EndOfLifeRecovery = v);
  return w;
 }

 [MapperIgnoreSource(nameof(MaterialPropertySet.Hygrothermal.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Hygrothermal.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Hygrothermal.WaterAbsorptionKgPerM2SqrtS))]
 private static partial HygrothermalWire Shell(MaterialPropertySet.Hygrothermal set);
 [UserMapping(Default = true)] internal static HygrothermalWire ToWire(MaterialPropertySet.Hygrothermal set) {
  HygrothermalWire w = Shell(set); set.WaterAbsorptionKgPerM2SqrtS.IfSome(v => w.WaterAbsorptionKgPerM2SqrtS = v); return w;
 }

 [MapperIgnoreSource(nameof(MaterialPropertySet.Electrical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Electrical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Electrical.MagneticPermeabilityRelative))]
 private static partial ElectricalWire Shell(MaterialPropertySet.Electrical set);
 [UserMapping(Default = true)] internal static ElectricalWire ToWire(MaterialPropertySet.Electrical set) {
  ElectricalWire w = Shell(set); set.MagneticPermeabilityRelative.IfSome(v => w.MagneticPermeabilityRelative = v); return w;
 }

 internal static MaterialCompositionWire ToWire(MaterialComposition composition) => composition.Switch<MaterialCompositionWire>(
  single: c => new() { Single = c.Material.Value },
  layerSet: c => { LayerSetWire w = new(); w.Layers.AddRange(c.Layers.Map(static l => ToWire(l))); return new() { LayerSet = w }; },
  profileSet: c => { ProfileSetWire w = new(); w.Profiles.AddRange(c.Profiles.Map(static p => ToWire(p))); c.Composite.IfSome(r => w.Composite = ToWire(r)); c.Section.IfSome(s => w.Section = ToWire(s)); return new() { ProfileSet = w }; },
  constituentSet: c => { ConstituentSetWire w = new(); w.Constituents.AddRange(c.Constituents.Map(static x => new MaterialConstituentWire { MaterialKey = x.Material.Value, Category = x.Category, Fraction = x.Fraction, PartName = x.PartName })); return new() { ConstituentSet = w }; });

 internal static MaterialLayerWire ToWire(MaterialLayer layer) {
  MaterialLayerWire w = new() { MaterialKey = layer.Material.Value, Thickness = ToWire(layer.Thickness), LayerName = layer.LayerName, Category = layer.Category };
  layer.Priority.IfSome(p => w.Priority = p); layer.Ventilated.IfSome(v => w.Ventilated = v); return w;
 }

 internal static MaterialProfileWire ToWire(MaterialProfile profile) {
  MaterialProfileWire w = new() { MaterialKey = profile.Material.Value, Profile = ToWire(profile.Profile), Category = profile.Category };
  profile.Priority.IfSome(p => w.Priority = p); w.Offsets.AddRange(profile.Offsets.Map(static o => ToWire(o))); return w;
 }

 internal static ProfileRefWire ToWire(ProfileRef profile) =>
  new() { Standard = profile.Standard, Designation = profile.Designation, ContentKey = ToWire(profile.ContentKey) };

 internal static Fin<MaterialPropertySetWire> ToWire(MaterialPropertySet set, Op key) => set.Switch<Fin<MaterialPropertySetWire>>(
  mechanical: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Mechanical = ToWire(x) }),
  orthotropic: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Orthotropic = ToWire(x) }),
  thermal: x => x.UValue.IsSome
   ? Fin.Fail<MaterialPropertySetWire>(new ElementFault.ValueRejected(key, "<substance-wire-product-u-value>"))
   : Fin.Succ(new() { Evidence = ToWire(x.Evidence), Thermal = ToWire(x) }),
  acoustic: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Acoustic = ToWire(x) }),
  fire: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Fire = ToWire(x) }),
  environmental: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Environmental = ToWire(x) }),
  cost: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Cost = ToWire(x) }),
  damping: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Damping = ToWire(x) }),
  hygrothermal: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Hygrothermal = ToWire(x) }),
  durability: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Durability = ToWire(x) }),
  optical: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Optical = ToWire(x) }),
  electrical: x => Fin.Succ(new() { Evidence = ToWire(x.Evidence), Electrical = ToWire(x) }));

 [UserMapping] internal static AcousticWire ToWire(MaterialPropertySet.Acoustic set) {
  AcousticWire w = new();
  w.Absorption.AddRange(set.AbsorptionSpectrum.Select(static (value, index) => new BandValueWire {
   Band = (Rasm.Contracts.Element.AcousticBand)(index + 1), Value = value,
  }));
  w.SoundReductionIndexDb.AddRange(set.SoundReductionIndexDb.Select(static (value, index) => new BandValueWire {
   Band = (Rasm.Contracts.Element.AcousticBand)(index + 1), Value = value,
  }));
  set.DynamicStiffnessMNPerM3.IfSome(v => w.DynamicStiffnessMnPerM3 = v); set.FlowResistivityPaSPerM2.IfSome(v => w.FlowResistivityPaSPerM2 = v);
  set.LossFactor.IfSome(v => w.LossFactor = v); return w;
 }

 [UserMapping] internal static DampingWire ToWire(MaterialPropertySet.Damping set) {
  DampingWire w = new() { DampingRatio = set.DampingRatio };
  set.Rayleigh.IfSome(r => w.Rayleigh = new RayleighWire { AlphaPerS = r.AlphaPerS, BetaS = r.BetaS }); return w;
 }

 internal static SampledCurveWire ToWire(SampledCurve curve) {
  SampledCurveWire wire = new();
  wire.Points.AddRange(curve.Axis.Zip(curve.Values, static (at, value) => new CurvePointWire { At = at, Value = value }));
  return wire;
 }

 static Fin<Node> ToMaterial(NodeId id, MaterialWire w, Op key) =>
  Present(w.Composition, "material.composition", key).Bind(c => ToComposition(c, key)).Bind(composition =>
   toSeq(w.PropertySets).TraverseM(p => ToPropertySet(p, key)).As().Map(sets =>
    (Node)new Node.Material(id, MaterialId.Of(w.MaterialKey), composition, sets)));

 static Fin<MaterialComposition> ToComposition(MaterialCompositionWire w, Op key) => w.CompositionCase switch {
  MaterialCompositionWire.CompositionOneofCase.Single => Fin.Succ(MaterialComposition.OfSingle(MaterialId.Of(w.Single))),
  MaterialCompositionWire.CompositionOneofCase.LayerSet =>
   toSeq(w.LayerSet.Layers).TraverseM(l => ToMeasure(l.Thickness, key).Map(t => new MaterialLayer(
     MaterialId.Of(l.MaterialKey), t, l.LayerName,
     Opt(l.HasPriority, l.Priority), l.Category,
     Opt(l.HasVentilated, l.Ventilated)))).As()
    .Bind(layers => MaterialComposition.OfLayerSet(layers, key)),
  MaterialCompositionWire.CompositionOneofCase.ProfileSet =>
   from profiles in toSeq(w.ProfileSet.Profiles).TraverseM(p => ToProfile(p, key)).As()
   from composite in Optional(w.ProfileSet.Composite).Traverse(c => ToProfileRef(c, key)).As()
   from admitted in MaterialComposition.OfProfileSet(profiles, key, composite)
   from section in Optional(w.ProfileSet.Section).Traverse(s => ToSection(s, key)).As()
   select section.Match(Some: admitted.WithSection, None: () => admitted),
  MaterialCompositionWire.CompositionOneofCase.ConstituentSet => MaterialComposition.OfConstituentSet(
   toSeq(w.ConstituentSet.Constituents).Map(c => new MaterialConstituent(MaterialId.Of(c.MaterialKey), c.Category, c.Fraction, c.PartName)), key),
  _ => new KernelFault.InvalidValue("element-wire.material-composition", "one composition arm is required", Some(key)),
 };

 static Fin<MaterialProfile> ToProfile(MaterialProfileWire w, Op key) =>
  from row in Present(w.Profile, "profile.ref", key)
  from profile in ToProfileRef(row, key)
  from offsets in toSeq(w.Offsets).TraverseM(o => ToMeasure(o, key)).As()
  select new MaterialProfile(MaterialId.Of(w.MaterialKey), profile, Opt(w.HasPriority, w.Priority), w.Category, offsets);

 static Fin<ProfileRef> ToProfileRef(ProfileRefWire w, Op key) =>
  ToKey(w.ContentKey, key).Bind(content => ProfileRef.Rehydrate(w.Standard, w.Designation, content, key));

 static readonly (string Slot, Func<SectionPropertiesWire, MeasureValueWire> Read)[] SectionColumns = [
  ("area", static w => w.Area), ("iyy", static w => w.Iyy), ("izz", static w => w.Izz), ("j", static w => w.J),
  ("iw", static w => w.Iw), ("wely", static w => w.Wely), ("welz", static w => w.Welz), ("wply", static w => w.Wply),
  ("wplz", static w => w.Wplz), ("av-y", static w => w.AvY), ("av-z", static w => w.AvZ),
  ("radius-of-gyration-major", static w => w.RadiusOfGyrationMajor), ("radius-of-gyration-minor", static w => w.RadiusOfGyrationMinor),
  ("depth", static w => w.Depth), ("width", static w => w.Width), ("heated-perimeter", static w => w.HeatedPerimeter),
  ("axis-distance", static w => w.AxisDistance), ("shear-centre-y", static w => w.ShearCentreY), ("shear-centre-z", static w => w.ShearCentreZ)];

 static Fin<SectionProperties> ToSection(SectionPropertiesWire w, Op key) =>
  toSeq(SectionColumns)
   .Traverse(column => Present(column.Read(w), $"section.{column.Slot}", key)
    .Bind(cell => ToMeasure(cell, key))
    .ToValidation())
   .As().ToFin()
   .Map(m => new SectionProperties(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15], m[16], m[17], m[18], w.MonosymmetryFactor));

 static Fin<double[]> ToSpectrum(IEnumerable<BandValueWire> rows, string column, Op key) =>
  UniqueMap(toSeq(rows).Map(static row => (Key: (int)row.Band, Value: row.Value)), column, key)
   .Bind(values => values.Count == 18 && Enumerable.Range(1, 18).All(values.ContainsKey)
    ? Fin.Succ(Enumerable.Range(1, 18).Select(index => values[index]).ToArray())
    : Fin.Fail<double[]>(new KernelFault.InvalidValue(
     $"element-wire.{column}", "carry every acoustic band exactly once", Some(key))));

 static Rasm.Contracts.Element.FireRating ToWire(FireRating value) => value == FireRating.A1
  ? Rasm.Contracts.Element.FireRating.A1
  : value == FireRating.A2
   ? Rasm.Contracts.Element.FireRating.A2
   : value == FireRating.B
    ? Rasm.Contracts.Element.FireRating.B
    : value == FireRating.C
     ? Rasm.Contracts.Element.FireRating.C
     : value == FireRating.D
      ? Rasm.Contracts.Element.FireRating.D
      : value == FireRating.E
       ? Rasm.Contracts.Element.FireRating.E
       : value == FireRating.F
        ? Rasm.Contracts.Element.FireRating.F
        : throw new UnreachableException();

 static Fin<FireRating> ToFireRating(Rasm.Contracts.Element.FireRating value, Op key) => value switch {
  Rasm.Contracts.Element.FireRating.A1 => Fin.Succ(FireRating.A1),
  Rasm.Contracts.Element.FireRating.A2 => Fin.Succ(FireRating.A2),
  Rasm.Contracts.Element.FireRating.B => Fin.Succ(FireRating.B),
  Rasm.Contracts.Element.FireRating.C => Fin.Succ(FireRating.C),
  Rasm.Contracts.Element.FireRating.D => Fin.Succ(FireRating.D),
  Rasm.Contracts.Element.FireRating.E => Fin.Succ(FireRating.E),
  Rasm.Contracts.Element.FireRating.F => Fin.Succ(FireRating.F),
  _ => Fin.Fail<FireRating>(key.InvalidInput(nameof(FireWire.Reaction))),
 };

 static Rasm.Contracts.Element.SmokeClass ToSmokeClass(string value) => value switch {
  "s1" => Rasm.Contracts.Element.SmokeClass.S1,
  "s2" => Rasm.Contracts.Element.SmokeClass.S2,
  "s3" => Rasm.Contracts.Element.SmokeClass.S3,
  _ => throw new UnreachableException(),
 };

 static Rasm.Contracts.Element.DropletClass ToDropletClass(string value) => value switch {
  "d0" => Rasm.Contracts.Element.DropletClass.D0,
  "d1" => Rasm.Contracts.Element.DropletClass.D1,
  "d2" => Rasm.Contracts.Element.DropletClass.D2,
  _ => throw new UnreachableException(),
 };

 static string ToSmokeToken(FireWire wire) => !wire.HasSmoke ? "" : wire.Smoke switch {
  Rasm.Contracts.Element.SmokeClass.S1 => "s1",
  Rasm.Contracts.Element.SmokeClass.S2 => "s2",
  Rasm.Contracts.Element.SmokeClass.S3 => "s3",
  _ => throw new UnreachableException(),
 };

 static string ToDropletToken(FireWire wire) => !wire.HasDroplets ? "" : wire.Droplets switch {
  Rasm.Contracts.Element.DropletClass.D0 => "d0",
  Rasm.Contracts.Element.DropletClass.D1 => "d1",
  Rasm.Contracts.Element.DropletClass.D2 => "d2",
  _ => throw new UnreachableException(),
 };

 static Rasm.Contracts.Element.MeasurementBasis ToWire(MeasurementBasis value) => value == MeasurementBasis.PerKg
  ? Rasm.Contracts.Element.MeasurementBasis.PerKg
  : value == MeasurementBasis.PerM2
   ? Rasm.Contracts.Element.MeasurementBasis.PerM2
   : value == MeasurementBasis.PerM3
    ? Rasm.Contracts.Element.MeasurementBasis.PerM3
    : value == MeasurementBasis.PerItem
     ? Rasm.Contracts.Element.MeasurementBasis.PerItem
     : throw new UnreachableException();

 static Fin<MeasurementBasis> ToMeasurementBasis(Rasm.Contracts.Element.MeasurementBasis value, Op key) => value switch {
  Rasm.Contracts.Element.MeasurementBasis.PerKg => Fin.Succ(MeasurementBasis.PerKg),
  Rasm.Contracts.Element.MeasurementBasis.PerM2 => Fin.Succ(MeasurementBasis.PerM2),
  Rasm.Contracts.Element.MeasurementBasis.PerM3 => Fin.Succ(MeasurementBasis.PerM3),
  Rasm.Contracts.Element.MeasurementBasis.PerItem => Fin.Succ(MeasurementBasis.PerItem),
  _ => Fin.Fail<MeasurementBasis>(key.InvalidInput(nameof(EnvironmentalWire.Basis))),
 };

 static Fin<ImmutableArray<double>> ToImpactMatrix(IEnumerable<BandCellWire> rows, Op key) =>
  UniqueMap(toSeq(rows).Map(static row => (
   Key: (Category: (int)row.Category, Band: (int)row.Band),
   Value: row.Value)), "environmental.impacts", key)
  .Bind(values => values.Count == MaterialPropertySet.Environmental.MatrixArity
   && Enumerable.Range(1, ImpactCategory.Count)
    .All(category => Enumerable.Range(1, LifecycleStage.Count).All(band => values.ContainsKey((category, band))))
   ? Fin.Succ(Enumerable.Range(1, ImpactCategory.Count)
     .SelectMany(category => Enumerable.Range(1, LifecycleStage.Count).Select(band => values[(category, band)]))
     .ToImmutableArray())
   : Fin.Fail<ImmutableArray<double>>(new KernelFault.InvalidValue(
    "element-wire.environmental.impacts", "carry the full unique impact-category by lifecycle-band tensor", Some(key))));

 static Fin<MaterialPropertySet> ToPropertySet(MaterialPropertySetWire w, Op key) =>
  ToEvidence(w.Evidence, key)
   .Bind(evidence => {
   return w.PropertySetCase switch {
    MaterialPropertySetWire.PropertySetOneofCase.Mechanical =>
     (ToMeasure(w.Mechanical.Density, key), ToMeasure(w.Mechanical.YoungsModulus, key), ToMeasure(w.Mechanical.YieldStrength, key), ToMeasure(w.Mechanical.UltimateStrength, key), OptCurve(w.Mechanical.YoungsReduction, key), OptCurve(w.Mechanical.YieldReduction, key))
      .Apply(static (density, youngs, yield, ultimate, youngsReduction, yieldReduction) => (density, youngs, yield, ultimate, youngsReduction, yieldReduction)).As()
      .Bind(t => MaterialPropertySet.OfMechanical(t.density, t.youngs, t.yield, t.ultimate, w.Mechanical.PoissonsRatio, w.Mechanical.ThermalExpansionPerK, key, evidence, t.youngsReduction, t.yieldReduction)),
    MaterialPropertySetWire.PropertySetOneofCase.Orthotropic =>
     (ToMeasure(w.Orthotropic.Density, key), ToMeasure(w.Orthotropic.E1Parallel, key), ToMeasure(w.Orthotropic.E2Perpendicular, key), ToMeasure(w.Orthotropic.ShearModulus, key), ToMeasure(w.Orthotropic.Strength1Parallel, key), ToMeasure(w.Orthotropic.Strength2Perpendicular, key), OptCurve(w.Orthotropic.ModulusReduction, key), OptCurve(w.Orthotropic.StrengthReduction, key))
      .Apply(static (density, e1, e2, shear, f1, f2, modulusReduction, strengthReduction) => (density, e1, e2, shear, f1, f2, modulusReduction, strengthReduction)).As()
      .Bind(t => MaterialPropertySet.OfOrthotropic(t.density, t.e1, None, t.e2, t.shear, t.f1, t.f2, w.Orthotropic.ThermalExpansionPerK, key, evidence, t.modulusReduction, t.strengthReduction)),
    MaterialPropertySetWire.PropertySetOneofCase.Thermal =>
     (ToMeasure(w.Thermal.Conductivity, key), ToMeasure(w.Thermal.SpecificHeat, key), OptCurve(w.Thermal.ConductivityCurve, key))
      .Apply(static (conductivity, specificHeat, conductivityCurve) => (conductivity, specificHeat, conductivityCurve)).As()
      .Bind(t => MaterialPropertySet.OfThermal(
       t.conductivity, t.specificHeat, None, w.Thermal.VapourResistanceFactor, key, evidence, t.conductivityCurve)),
    MaterialPropertySetWire.PropertySetOneofCase.Acoustic =>
     (ToSpectrum(w.Acoustic.Absorption, "acoustic.absorption", key),
      ToSpectrum(w.Acoustic.SoundReductionIndexDb, "acoustic.sound-reduction", key))
     .Apply(static (absorption, reduction) => (absorption, reduction)).As()
     .ToFin()
     .Bind(rows => Acoustic.Of(
      rows.absorption, rows.reduction, key,
      Opt(w.Acoustic.HasDynamicStiffnessMnPerM3, w.Acoustic.DynamicStiffnessMnPerM3),
      Opt(w.Acoustic.HasFlowResistivityPaSPerM2, w.Acoustic.FlowResistivityPaSPerM2),
      Opt(w.Acoustic.HasLossFactor, w.Acoustic.LossFactor)))
     .Map(spectrum => MaterialPropertySet.OfAcoustic(spectrum, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Fire => Present(w.Fire.Resistance, "fire.resistance", key)
     .Bind(r => FireResistance.Of(
      Opt(r.HasLoadBearingMinutes, r.LoadBearingMinutes),
      Opt(r.HasIntegrityMinutes, r.IntegrityMinutes),
      Opt(r.HasInsulationMinutes, r.InsulationMinutes), key))
     .Bind(resistance => !w.Fire.HasReaction
      ? Fin.Succ(MaterialPropertySet.OfFire(None, resistance, evidence))
      : ToFireRating(w.Fire.Reaction, key).Bind(reaction => !w.Fire.HasSmoke
       ? Fin.Succ(MaterialPropertySet.OfFire(Some(reaction), resistance, evidence))
       : key.AcceptValidated<EuroclassSuffix>($"{ToSmokeToken(w.Fire)},{ToDropletToken(w.Fire)}")
        .Map(suffix => MaterialPropertySet.OfFire(reaction, suffix, resistance, evidence)))),
    MaterialPropertySetWire.PropertySetOneofCase.Environmental => ToMeasurementBasis(w.Environmental.Basis, key).Bind(basis =>
     ToImpactMatrix(w.Environmental.Impacts, key).Bind(impacts => MaterialPropertySet.OfEnvironmental(basis, impacts,
      Opt(w.Environmental.HasRecycledContent, w.Environmental.RecycledContent),
      Opt(w.Environmental.HasEndOfLifeRecovery, w.Environmental.EndOfLifeRecovery), key, evidence))),
    MaterialPropertySetWire.PropertySetOneofCase.Cost => ToMeasurementBasis(w.Cost.Basis, key).Bind(basis =>
     Currency.Parse(w.Cost.Currency, key).Bind(currency =>
      MaterialPropertySet.OfCost(basis, currency, w.Cost.SupplyPerUnit, w.Cost.InstallPerUnit, w.Cost.LifecyclePerUnit, key, evidence))),
    MaterialPropertySetWire.PropertySetOneofCase.Damping => MaterialPropertySet.OfDamping(
     w.Damping.DampingRatio, Optional(w.Damping.Rayleigh).Map(static r => (r.AlphaPerS, r.BetaS)), key, evidence),
    MaterialPropertySetWire.PropertySetOneofCase.Hygrothermal =>
     (ToMeasure(w.Hygrothermal.WaterContent80Rh, key), ToMeasure(w.Hygrothermal.FreeWaterSaturation, key),
      OptCurve(w.Hygrothermal.SorptionIsotherm, key), OptCurve(w.Hygrothermal.LiquidTransport, key), OptCurve(w.Hygrothermal.MoistureConductivity, key))
      .Apply(static (waterContent, saturation, sorption, liquid, conductivity) => (waterContent, saturation, sorption, liquid, conductivity)).As()
      .Bind(t => MaterialPropertySet.OfHygrothermal(w.Hygrothermal.Porosity, t.waterContent.Si, t.saturation.Si,
       Opt(w.Hygrothermal.HasWaterAbsorptionKgPerM2SqrtS, w.Hygrothermal.WaterAbsorptionKgPerM2SqrtS), key, evidence, t.sorption, t.liquid, t.conductivity)),
    MaterialPropertySetWire.PropertySetOneofCase.Durability =>
     ToMeasure(w.Durability.ChlorideDiffusion, key).Bind(chloride => MaterialPropertySet.OfDurability(
      w.Durability.CarbonationRateMmPerSqrtYear, chloride.Si, w.Durability.AgeingExponent, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Optical => MaterialPropertySet.OfOptical(
     w.Optical.VisibleTransmittance, w.Optical.VisibleReflectanceFront, w.Optical.VisibleReflectanceBack, w.Optical.SolarTransmittance, w.Optical.SolarReflectanceFront, w.Optical.SolarReflectanceBack, w.Optical.ThermalIrTransmittance, w.Optical.ThermalIrEmissivityFront, w.Optical.ThermalIrEmissivityBack, key, evidence),
    MaterialPropertySetWire.PropertySetOneofCase.Electrical =>
     (ToMeasure(w.Electrical.Resistivity, key), OptMeasure(w.Electrical.DielectricStrength, key))
      .Apply(static (resistivity, dielectric) => (resistivity, dielectric)).As()
      .Bind(t => MaterialPropertySet.OfElectrical(
       t.resistivity.Si, w.Electrical.RelativePermittivity, t.dielectric.Map(static m => m.Si),
       Opt(w.Electrical.HasMagneticPermeabilityRelative, w.Electrical.MagneticPermeabilityRelative), key, evidence)),
    _ => new KernelFault.InvalidValue("element-wire.material-property", "one property arm is required", Some(key)),
   };
  });
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
