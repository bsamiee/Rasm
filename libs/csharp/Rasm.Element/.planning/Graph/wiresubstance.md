# [ELEMENT_WIRE_SUBSTANCE]

`WireCodec`'s material plane: the `Node.Material` payload, the four-arm `MaterialComposition` fold with its layer/profile/constituent rows and `ProfileRef` content-key re-derivation, the three-arm `MaterialUsage` fold, the twelve-case `MaterialPropertySet` family (evidence riding the ENVELOPE, each arm re-entering its accumulating `Of*` admission), the `SectionColumns` one-table section codec whose row position is simultaneously traversal order, frozen field order, and ctor position, `SampledCurve`, and the EN 13501 fire family (the `EuroclassSuffix` re-join over the two frozen token columns).

## [01]-[INDEX]

- [02]-[SUBSTANCE_CODEC]: composition/usage/property-set folds, the section one-table codec, and their accumulating re-admissions.

## [02]-[SUBSTANCE_CODEC]

- Cases: `MaterialUsage` 3 arms (fields PERMUTED against canon — ledger), `MaterialComposition` 4 arms, `MaterialPropertySet` 12 arms — census rows [05]/[06]/[07] at `Graph/wire#WIRE_CODEC`.
- Law: this page is one PARTIAL PART of the `Graph/wire#WIRE_CODEC` `[Mapper]` family — the `[Mapper]` attribute, the `[UNION_PARITY]` census, the `[KEY_CODECS]`, the shared decode gates (`Present`/`Opt`/`Row`/`Named`/`Iso`/`ToInterval`/`ToDate`/`BothOrNeither`/`OptMeasure`/`OptCurve`), the `[PRESENCE_SHELLS]` and carrier-codec laws, `ElementWire`, and the frozen-number ledger all live THERE; a member landing here lands its census/ledger row there in the same edit.
- Law: every decoded value re-crosses its OWNER's admission gate — the decoder constructs no case directly and trusts no carried invariant (the `ContentAddress.Verify` distrust posture); every optional column crosses by EXPLICIT presence, never a defaulted zero, blank, or sentinel.
- Packages: Google.Protobuf, Riok.Mapperly, NodaTime.Serialization.Protobuf, LanguageExt.Core, Thinktecture.Runtime.Extensions (the generated total `Switch` encode dispatch and `TryGet` row gates) — the manifest triad rides `Graph/wire#WIRE_CODEC`.
- Growth: a new column on a family this page owns is one append-only numbered field at the corpus proto, one ledger row at `Graph/wire#WIRE_CODEC`, and one transcription member here; a new union case also lands its `CrossingFamily` arm count and its oneof mirror in the same edit — the parity census refuses a half-landed pair.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ---------------------------------------------------------------------------
// One partial part of the ONE `[Mapper]` WireCodec family — the attribute, the parity census, the key codecs, and
// the shared decode gates ride `Graph/wire#WIRE_CODEC`; this part owns the material composition, usage, and engineering-property transcriptions.
internal static partial class WireCodec {
 // --- [CASE_TRANSCRIPTIONS] — Mapperly generates the flat-column width per case; every union-valued member rides
 // an explicit envelope fold below, every MESSAGE-shaped Option crossing rides a nullable-return [UserMapping]
 // carrier codec, every optional SCALAR/STRING column rides a hand IfSome presence write (the [PRESENCE_SHELLS]
 // law below), and [MapProperty] pins every seam→wire name seam so the generator never silently skips a member.
 [MapperIgnoreSource(nameof(Node.Material.Id))]
 [MapProperty(nameof(Node.Material.Properties), nameof(MaterialWire.PropertySets))]
 internal static partial MaterialWire ToWire(Node.Material node);

 // LeastDimension re-derives from the Depth/Width pair and IsDoublySymmetric from the shear-centre offsets and the
 // mono-symmetry factor — stored columns that DO cross — so neither derived member crosses; a wire field for either
 // double-stores one fact, the same law the property-set rosters below hold.
 [MapperIgnoreSource(nameof(SectionProperties.LeastDimension))]
 [MapperIgnoreSource(nameof(SectionProperties.IsDoublySymmetric))]
 internal static partial SectionPropertiesWire ToWire(SectionProperties section);

 // Every property-set case ignores its non-crossing source members BY NAME, never by suppression: the base Evidence
 // column rides the ENVELOPE (MaterialPropertySetWire.evidence, the Switch fold below), the base Discipline read
 // is the case-to-discipline map the far end re-reads off the decoded case, and every DERIVED member (the isotropic
 // ShearModulus, the Environmental carbon projections, the Optical absorptance remainders) re-derives from the stored
 // columns that DO cross — a wire field for any of them would double-store one fact. The explicit roster keeps
 // RequiredMappingStrategy.Both's source-side RMG020 proof live for every stored column; the Acoustic/Damping arms
 // carry hand [UserMapping] bodies below, so no roster applies to them, and the Fire/Environmental/Hygrothermal/
 // Electrical arms ride [PRESENCE_SHELLS] whose optional scalar/string columns are roster-named HAND-CROSSED
 // members, never non-crossing ones.
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.ShearModulus))]
 internal static partial MechanicalWire ToWire(MaterialPropertySet.Mechanical set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.Discipline))]
 // E005 does not cross: the wire declares no fractile column, and the decode arm re-states the typed absence.
 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.E005))]
 internal static partial OrthotropicWire ToWire(MaterialPropertySet.Orthotropic set);

 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.Discipline))]
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

 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Reaction))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Suffix))]
 private static partial FireWire Shell(MaterialPropertySet.Fire set);
 // Suffix is HAND-CROSSED: the one EuroclassSuffix product fans onto the two frozen smoke/droplets token columns.
 [UserMapping(Default = true)] internal static FireWire ToWire(MaterialPropertySet.Fire set) {
  FireWire w = Shell(set); w.Smoke = set.Suffix.Smoke; w.Droplets = set.Suffix.Droplets;
  set.Reaction.IfSome(r => w.Reaction = r.Key); return w;
 }

 // All three EN 13501-2 criteria are optional scalars, so the whole row is presence writes — the one nested message
 // the Fire shell reaches through a hand mapping rather than a generated one.
 [UserMapping] internal static FireResistanceWire ToWire(FireResistance resistance) {
  FireResistanceWire w = new(); resistance.LoadBearingMinutes.IfSome(m => w.LoadBearingMinutes = m); resistance.IntegrityMinutes.IfSome(m => w.IntegrityMinutes = m); resistance.InsulationMinutes.IfSome(m => w.InsulationMinutes = m); return w;
 }

 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.Gwp))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.WholeLifeGwp))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.StageGwp))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.RecycledContent))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.EndOfLifeRecovery))]
 private static partial EnvironmentalWire Shell(MaterialPropertySet.Environmental set);
 [UserMapping(Default = true)] internal static EnvironmentalWire ToWire(MaterialPropertySet.Environmental set) {
  EnvironmentalWire w = Shell(set); set.RecycledContent.IfSome(v => w.RecycledContent = v); set.EndOfLifeRecovery.IfSome(v => w.EndOfLifeRecovery = v); return w;
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

 internal static MaterialUsageWire ToWire(MaterialUsage usage) => usage.Switch<MaterialUsageWire>(
  none: static _ => new() { None = new Google.Protobuf.WellKnownTypes.Empty() },
  layerSet: u => { LayerSetUsageWire wire = new() { Direction = u.Direction.Key, Sense = u.Sense.Key }; u.OffsetFromReferenceLine.IfSome(value => wire.OffsetFromReferenceLine = ToWire(value)); u.ReferenceExtent.IfSome(value => wire.ReferenceExtent = ToWire(value)); return new() { LayerSet = wire }; },
  profileSet: u => { ProfileSetUsageWire wire = new(); u.CardinalPoint.IfSome(value => wire.CardinalPoint = value.Key); u.ReferenceExtent.IfSome(value => wire.ReferenceExtent = ToWire(value)); return new() { ProfileSet = wire }; });

 // Every optional row column writes through explicit protobuf presence — an IfSome assignment, never a defaulted zero or
 // false that a decoder cannot distinguish from an author's real value.
 internal static MaterialCompositionWire ToWire(MaterialComposition composition) => composition.Switch<MaterialCompositionWire>(
  single: c => new() { Single = new SingleWire { MaterialKey = c.Material.Value } },
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

 // ONE ProfileRef projection serves the row and the set-level composite — a second inline construction is the fork
 // that lets one leg drop the content key the Rehydrate gate re-checks.
 internal static ProfileRefWire ToWire(ProfileRef profile) =>
  new() { Standard = profile.Standard, Designation = profile.Designation, ContentKey = ToWire(profile.ContentKey) };

 // Evidence rides the envelope (the base-class column), each arm its generated flat mapping over the registered
 // Option carriers — the sampled-curve carrier included, so the reduction, λ(θ), and hygrothermal curve columns
 // generate; the Acoustic/Damping arms carry repeated spectra and a tuple flatten no carrier bridges, so their
 // bodies are owned here beside the fold.
 internal static MaterialPropertySetWire ToWire(MaterialPropertySet set) => set.Switch<MaterialPropertySetWire>(
  mechanical: x => new() { Evidence = ToWire(x.Evidence), Mechanical = ToWire(x) },
  orthotropic: x => new() { Evidence = ToWire(x.Evidence), Orthotropic = ToWire(x) },
  thermal: x => new() { Evidence = ToWire(x.Evidence), Thermal = ToWire(x) },
  acoustic: x => new() { Evidence = ToWire(x.Evidence), Acoustic = ToWire(x) },
  fire: x => new() { Evidence = ToWire(x.Evidence), Fire = ToWire(x) },
  environmental: x => new() { Evidence = ToWire(x.Evidence), Environmental = ToWire(x) },
  cost: x => new() { Evidence = ToWire(x.Evidence), Cost = ToWire(x) },
  damping: x => new() { Evidence = ToWire(x.Evidence), Damping = ToWire(x) },
  hygrothermal: x => new() { Evidence = ToWire(x.Evidence), Hygrothermal = ToWire(x) },
  durability: x => new() { Evidence = ToWire(x.Evidence), Durability = ToWire(x) },
  optical: x => new() { Evidence = ToWire(x.Evidence), Optical = ToWire(x) },
  electrical: x => new() { Evidence = ToWire(x.Evidence), Electrical = ToWire(x) });

 [UserMapping] internal static AcousticWire ToWire(MaterialPropertySet.Acoustic set) {
  AcousticWire w = new();
  w.AbsorptionSpectrum.AddRange(set.AbsorptionSpectrum); w.SoundReductionIndexDb.AddRange(set.SoundReductionIndexDb);
  set.DynamicStiffnessMNPerM3.IfSome(v => w.DynamicStiffnessMnPerM3 = v); set.FlowResistivityPaSPerM2.IfSome(v => w.FlowResistivityPaSPerM2 = v);
  set.LossFactor.IfSome(v => w.LossFactor = v); return w;
 }

 [UserMapping] internal static DampingWire ToWire(MaterialPropertySet.Damping set) {
  DampingWire w = new() { DampingRatio = set.DampingRatio };
  set.Rayleigh.IfSome(r => w.Rayleigh = new RayleighWire { AlphaPerS = r.AlphaPerS, BetaS = r.BetaS }); return w;
 }

 // Both repeated runs fill natively — the generator emits its own guarded fill from the two ImmutableArray columns.
 internal static partial SampledCurveWire ToWire(SampledCurve curve);

 internal static Fin<MaterialUsage> ToUsage(MaterialUsageWire? w, Op key) => w?.UsageCase switch {
  MaterialUsageWire.UsageOneofCase.None => Fin.Succ((MaterialUsage)new MaterialUsage.Unbound()),
  MaterialUsageWire.UsageOneofCase.LayerSet =>
   from direction in key.Row<string, LayerSetDirection>(w.LayerSet.Direction)
   from sense in key.Row<string, DirectionSense>(w.LayerSet.Sense)
   from offset in OptMeasure(w.LayerSet.OffsetFromReferenceLine, key)
   from extent in OptMeasure(w.LayerSet.ReferenceExtent, key)
   from usage in MaterialUsage.LayerSet.Of(direction, sense, offset, extent, key)
   select usage,
  MaterialUsageWire.UsageOneofCase.ProfileSet =>
   from extent in OptMeasure(w.ProfileSet.ReferenceExtent, key)
   from usage in MaterialUsage.ProfileSet.Of(Opt(w.ProfileSet.HasCardinalPoint, w.ProfileSet.CardinalPoint), extent, key)
   select usage,
  null => new KernelFault.InvalidValue("element-wire.material-usage", "one usage arm is required", Some(key)),
  _ => new KernelFault.InvalidValue("element-wire.material-usage", "usage arm is unknown", Some(key)),
 };

 static Fin<Node> ToMaterial(NodeId id, MaterialWire w, Op key) =>
  Present(w.Composition, "material.composition", key).Bind(c => ToComposition(c, key)).Bind(composition =>
   toSeq(w.PropertySets).TraverseM(p => ToPropertySet(p, key)).As().Map(sets =>
    (Node)new Node.Material(id, MaterialId.Of(w.MaterialKey), composition, sets)));

 // Every arm re-enters the seam Of* admission (the row-count, thickness, priority-range, offset-arity, and normalization
 // gates hold for hostile wire bytes exactly as for an in-process author), and each optional row column reads through the
 // generated Has* presence probe — a defaulted zero priority or false ventilation never forges an author's value. The
 // ProfileSet arm admits the rows FIRST and stamps the baked section afterwards through WithSection, so the private-ctor
 // case is never constructed directly and the head-row derivation stays total.
 static Fin<MaterialComposition> ToComposition(MaterialCompositionWire w, Op key) => w.CompositionCase switch {
  MaterialCompositionWire.CompositionOneofCase.Single => Fin.Succ(MaterialComposition.OfSingle(MaterialId.Of(w.Single.MaterialKey))),
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

 // One compound-profile row: every offset re-crosses the MeasureValue finite gate beside the row's own ProfileRef admission.
 static Fin<MaterialProfile> ToProfile(MaterialProfileWire w, Op key) =>
  from row in Present(w.Profile, "profile.ref", key)
  from profile in ToProfileRef(row, key)
  from offsets in toSeq(w.Offsets).TraverseM(o => ToMeasure(o, key)).As()
  select new MaterialProfile(MaterialId.Of(w.MaterialKey), profile, Opt(w.HasPriority, w.Priority), w.Category, offsets);

 // ONE ProfileRef admission serves the row and the set-level composite: Rehydrate re-derives the content key off the
 // normalized (standard, designation) and rails when a persisted key disagrees, so no wire leg trusts a carried digest.
 static Fin<ProfileRef> ToProfileRef(ProfileRefWire w, Op key) =>
  ProfileRef.Rehydrate(w.Standard, w.Designation, ToKey(w.ContentKey), key);

 // ONE column table owns the section's measured run: each row pairs the wire slot's own name with its accessor, and
 // ROW POSITION is simultaneously the traversal order, the frozen SectionPropertiesWire field order, and the ctor
 // position — so a slot moves once and both directions follow. The slot name is load-bearing on the rail: a
 // non-finite column names ITSELF rather than reporting the quantity token nineteen columns share. The positional
 // rebuild survives because a C# constructor takes no splat; the table's own order is what pins it, and the arity is
 // proved by the table rather than restated.
 static readonly (string Slot, Func<SectionPropertiesWire, MeasureValueWire> Read)[] SectionColumns = [
  ("area", static w => w.Area), ("iyy", static w => w.Iyy), ("izz", static w => w.Izz), ("j", static w => w.J),
  ("iw", static w => w.Iw), ("wely", static w => w.Wely), ("welz", static w => w.Welz), ("wply", static w => w.Wply),
  ("wplz", static w => w.Wplz), ("av-y", static w => w.AvY), ("av-z", static w => w.AvZ),
  ("radius-of-gyration-major", static w => w.RadiusOfGyrationMajor), ("radius-of-gyration-minor", static w => w.RadiusOfGyrationMinor),
  ("depth", static w => w.Depth), ("width", static w => w.Width), ("heated-perimeter", static w => w.HeatedPerimeter),
  ("axis-distance", static w => w.AxisDistance), ("shear-centre-y", static w => w.ShearCentreY), ("shear-centre-z", static w => w.ShearCentreZ)];

 // Nineteen measure columns re-cross the OfSi finite gate, which a Mapperly partial cannot thread, and they accumulate:
 // a datasheet with three bad columns names all three, matching the owning admission's own accumulating shape.
 static Fin<SectionProperties> ToSection(SectionPropertiesWire w, Op key) =>
  toSeq(SectionColumns)
   .Traverse(column => Present(column.Read(w), $"section.{column.Slot}", key)
    .Bind(cell => ToMeasure(cell, key))
    .ToValidation())
   .As().ToFin()
   .Map(m => new SectionProperties(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15], m[16], m[17], m[18], w.MonosymmetryFactor));

 // Every arm re-enters the canonical MaterialPropertySet.Of* admission rail — the decoder NEVER constructs a case
 // directly, so the physical bounds, finite gates, matrix arity, and cross-field refinements the owner declares hold
 // for hostile wire bytes exactly as for an in-process author; the raw-double columns pass through verbatim and the
 // measured columns re-cross as admitted MeasureValues (or their SI scalars where the owner mints the type itself).
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
      // The wire declares no fractile column, so the decode arm states the typed absence — the stability
      // kernels downstream refuse on it rather than reading a reconstructed ratio.
      .Bind(t => MaterialPropertySet.OfOrthotropic(t.density, t.e1, None, t.e2, t.shear, t.f1, t.f2, w.Orthotropic.ThermalExpansionPerK, key, evidence, t.modulusReduction, t.strengthReduction)),
    MaterialPropertySetWire.PropertySetOneofCase.Thermal =>
     (ToMeasure(w.Thermal.Conductivity, key), ToMeasure(w.Thermal.SpecificHeat, key), OptMeasure(w.Thermal.UValue, key), OptCurve(w.Thermal.ConductivityCurve, key))
      .Apply(static (conductivity, specificHeat, uValue, conductivityCurve) => (conductivity, specificHeat, uValue, conductivityCurve)).As()
      .Bind(t => MaterialPropertySet.OfThermal(t.conductivity, t.specificHeat, t.uValue, w.Thermal.VapourResistanceFactor, key, evidence, t.conductivityCurve)),
    MaterialPropertySetWire.PropertySetOneofCase.Acoustic => Acoustic.Of(
     w.Acoustic.AbsorptionSpectrum.ToArray(), w.Acoustic.SoundReductionIndexDb.ToArray(), key,
     Opt(w.Acoustic.HasDynamicStiffnessMnPerM3, w.Acoustic.DynamicStiffnessMnPerM3), Opt(w.Acoustic.HasFlowResistivityPaSPerM2, w.Acoustic.FlowResistivityPaSPerM2), Opt(w.Acoustic.HasLossFactor, w.Acoustic.LossFactor))
     .Map(spectrum => MaterialPropertySet.OfAcoustic(spectrum, evidence)),
    // Absent reactions ride the 2-arg OfFire (NotSpecified sub-classes by construction); a present token admits
    // its full EN 13501-1 classification, the three INDEPENDENT token gates accumulating applicatively so a
    // hostile record with a bad rating AND a bad sub-class names both in one failure.
    MaterialPropertySetWire.PropertySetOneofCase.Fire => Present(w.Fire.Resistance, "fire.resistance", key)
     .Bind(r => FireResistance.Of(
      Opt(r.HasLoadBearingMinutes, r.LoadBearingMinutes),
      Opt(r.HasIntegrityMinutes, r.IntegrityMinutes),
      Opt(r.HasInsulationMinutes, r.InsulationMinutes), key))
     .Bind(resistance => !w.Fire.HasReaction
      ? Fin.Succ(MaterialPropertySet.OfFire(None, resistance, evidence))
      // The two frozen token columns re-join into the ONE EuroclassSuffix [ObjectFactory<string>] grammar (the
      // former SmokeClass/DropletClass TryGet planes died at the owner; the hand Parse died for the factory
      // plane, whose hook-authored smoke/droplets detail now surfaces unaltered); the two INDEPENDENT admissions accumulate.
      : (FireRating.Parse(w.Fire.Reaction, key),
         key.AcceptValidated<EuroclassSuffix>($"{w.Fire.Smoke},{w.Fire.Droplets}"))
         .Apply((reaction, suffix) => MaterialPropertySet.OfFire(reaction, suffix, resistance, evidence)).As()),
    MaterialPropertySetWire.PropertySetOneofCase.Environmental => MeasurementBasis.Parse(w.Environmental.Basis, key).Bind(basis =>
     MaterialPropertySet.OfEnvironmental(basis, [.. w.Environmental.Impacts],
      Opt(w.Environmental.HasRecycledContent, w.Environmental.RecycledContent),
      Opt(w.Environmental.HasEndOfLifeRecovery, w.Environmental.EndOfLifeRecovery), key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Cost => MeasurementBasis.Parse(w.Cost.Basis, key).Bind(basis =>
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
    // Both measured columns re-cross the decode measure gate, then pass their SI scalars into the owner's own
    // admission — resistivity re-entering the registry ElectricResistivity mint at its OhmMeter base, the breakdown
    // field the DielectricStrength OfSi mint (Ω·m and V/m ARE the SI bases, so both scalars cross verbatim — the
    // Durability chloride-diffusion shape); the optional μr rides the generated presence probe, never a defaulted unity.
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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
