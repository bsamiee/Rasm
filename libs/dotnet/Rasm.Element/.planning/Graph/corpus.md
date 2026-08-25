# [ELEMENT_CORPUS]

`GraphForge` owns deterministic synthetic models and the graded roster shared by benchmarks and property specifications. `CorpusProfile` closes occurrence count, edge density, bag width, discipline mix, composition depth, observation and flavor cadences, and seed. `GraphForge.Mint` admits forged nodes and edges through `GraphDelta.AdmitOnto` over `Genesis`, exercising native graph law and producing a second mutation delta.

Occurrence ids derive from kernel `ContentHash` over `(seed, lane, ordinal)` and carry the Guid-v7 layout. Type ids mint through `NodeId.Of(NodeSeed.TypeSeed)`; non-rooted ids through `NodeId.Of(NodeSeed.Content)`. Magnitudes and index draws derive from the kernel `Deterministic` splitmix stream, never from a digest, so identity and derivation stay two rails. Each grade therefore reproduces one snapshot fingerprint on every runtime sharing the seed-zero content rail.

`GraphForge` composes the existing `ElementGraph`, `GraphDelta`, `ContentAddress`, `CanonicalWriter`, `ElementWire`, and `GraphTable` owners. `ElementWire` is exercised only for the generated `NodeWire` support seam used by Persistence edits; the corpus never promotes a graph or delta into a peer contract.

## [01]-[INDEX]

- [02]-[GRAPH_FORGE]: `CorpusProfile` closes the parameter record and `GraphForge` mints deterministically — the seeded id stream, the payload kernels, and the one `AdmitOnto` realization every forged model crosses.
- [03]-[CORPUS_ROSTER]: `CorpusGrade` size rows, the `CorpusOp` hot-path vocabulary, `CorpusModel` and `CorpusWitness`, and the `CorpusGate` mint/determinism entries consumed by tests-estate benchmark and property lanes.

## [02]-[GRAPH_FORGE]

- Owner: `CorpusProfile` the closed generation-parameter record — occurrence count, `[0,1]` edge density, bag width, discipline mix, composition depth, type-reuse ratio, observation stride and sample count, flavor stride, seed — railed through `Of`; `GraphForge` the deterministic realization fold.
- Entry: `CorpusProfile.Of(nodes, density, bagWidth, disciplines, depth, seed, key, typeRatio, observationStride, observationSamples, flavorStride)` admits positive counts, a unit-interval density, a non-empty discipline mix, a depth of at least one, and positive reuse, stride, and sample columns; numeric refusals stay `KernelFault.OutOfRange` while the empty discipline mix remains an Element semantic refusal; `GraphForge.Mint(profile, key)` realizes the profile into `Fin<(ElementGraph Graph, GraphDelta Delta, GraphDelta Mutation)>` — the frozen snapshot a benchmark folds, the creating event body, and a native change record.
- Auto: `Mint` builds one shared corpus header (`Header.Default` over the fixed corpus instant, WIDENED with a populated `StepHeader` and a one-override/one-axis `UnitScheme` — constants, so header bytes still never fork a grade), one `Node.Material` per type slot whose composition cycles the four `MaterialComposition` arms by slot ordinal and whose property bag carries the WHOLE twelve-case engineering roster, one deterministic Type `Object` per slot (id through the production `NodeId.Of(NodeSeed.TypeSeed)` streamed over `WriteIdentity`), then per occurrence one seeded Guid-v7 `Object`, one property bag of `BagWidth` rows whose value case steps the fourteen-case `PropertyValue` family off the flat slot ordinal (its quotient stepping the five `TemporalValue` leaves), one quantity bag row through `MeasureValue.OfSi`, one `Computed` assessment cycling the discipline mix, and — every `ObservationStride`-th occurrence — one `ObservationSeries` opened at the corpus instant and grown by one `Encode`-minted chunk under a `From`-derived summary; one shared `Node.Appearance` and one `Node.Coverage` whose smallest-admissible level run carries the one-cell base and its `Coarsen`-derived successor (both blob keys off the corpus lattice lane) that the whole model associates to; edges land as the `Aggregate` fanout spine (depth-derived fanout), the `PropertyDefinition`/`Assessment`/`TypeDefinition`/`Observation` assigns, the material `Associate`s carrying the `MaterialUsage` arm their material's composition slot selects (`LayerSet` and `ProfileSet` usage on the matching composition slots, the explicit `Unbound` arm elsewhere), the witness-resource `Associate`s under `MaterialUsage.Unbound`, `⌊density·nodes⌋` seeded `Connect` adjacencies, and — every `FlavorStride`-th occurrence — the `Compose.Contain`/`Nest`/`Reference` flavors, a `Void`, a realized `Connect` (on the strided pairs past occurrence zero) carrying occurrence zero as its distinct realizing intermediary beside a content-lane `Interface` key, and a `Generic` burying a `PropertyValue.Reference` in its attribute map. `AdmitOnto(Genesis(header))` admits the assembled normal-form delta, so `LegalLink` runs per forged edge.
- Law: `Mint`'s witness run closes exactly six families, the optional-slot and presence duals (the realized `Connect` pair, the strided `OwnerHistory` with its alternating `Modified`, the alternating `EvidenceRun` window/correlation, the cycling `EvidenceGrade` roster with its expiry/attestation/run duals, the baked `ProfileSet` section), and two section pairs, and claims nothing wider — every `Node` case, every `Relationship` flavor, every `MaterialComposition` arm, every `MaterialPropertySet` case, every `MaterialUsage` arm, and every `PropertyValue` case with every `TemporalValue` leaf cross a graded witness; the strided `Connect` carries both optional slots (the realizing intermediary and the interface key), the coverage witness carries its two-level `Coarsen` run, and the mutation delta carries the removal and revision sections. Each cycle steps an ORDINAL whose run outsizes its family at the SMALLEST grade — four type slots against four composition arms and against the three usage arms riding the same slot ordinal, the whole twelve-case roster on every material, `Nodes × BagWidth` value slots against the fourteen cases times five leaves — so totality is a property of the arithmetic rather than of a grade's size. Sampled-curve coverage rides the thermal witness's `Some` conductivity curve, so `SampledCurveWire`, the `OptCurve` decode re-admission, and the `Curve` canon write cross every grade; the remaining curve columns ride `None`, so both presence duals witness.
- Receipt: the mint result carries the frozen graph, the creating normal-form delta, and the mutation delta; `ContentAddress.OfGraph` supplies the snapshot's reproducibility fingerprint and `GraphDelta.Address` the mutation's.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Map`/`TraverseM`), Thinktecture.Runtime.Extensions (generated owners), NodaTime (temporal leaves), `Rasm` (content identity, deterministic streams, admission, numerics, and drawing), `Graph/wire#NODE_CODEC` (`ElementWire` and the `CrossingFamily` widths), and `Graph/table#TABULATE_FOLD` (the `Tabulate` row).
- Growth: a new `Node` case or `Relationship` flavor is one witness row in the assembly; a new arm on any cycled family is one factory row beside its incremented arity const, which is what keeps the ordinal cycle total; a new payload family in the forge is one kernel arm beside the existing node kernels; a new generation axis is one `CorpusProfile` column threaded into the kernels, and a new random axis is one draw lane on `Deterministic` — never a sibling forge, never a parameter whose value the seed cannot replay, and never a magnitude projected off an id digest.
- Boundary: the forge composes ONLY the seam's own admissions — a raw case constructor bypassing `Classification.Of`, `MeasureValue.OfSi`, or `AssessmentPayload.Open` forges models no production projector can produce, so every railed admission the graph demands runs inside `Mint`; the delta is constructed wholesale in normal form (ids unique by the seeded stream) and still crosses `AdmitOnto` — `ReplayOnto` trusts only seam-produced deltas and the forge counts as foreign to the structural law; determinism never rides a runtime PRNG — a `Random(seed)` stream couples the corpus to a BCL implementation — and it splits by AXIS across the two kernel owners: an ID replays through `ContentHash` over `(seed, lane, ordinal)` and a MAGNITUDE or index draw through `Deterministic`, so neither a hash-seeded sampler (which the kernel rejects by design) nor a modulo-biased projection off a digest survives here; the generation loops are the named measured-kernel statement seam, confined to the forge kernels.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using CommunityToolkit.HighPerformance.Buffers;
using Google.Protobuf;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;
using LatticeAxis = Rasm.Numerics.Dimension;

namespace Rasm.Element.Graph;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LaneUse {
 public static readonly LaneUse Id = new("id");
 public static readonly LaneUse Draw = new("draw");
}

[SmartEnum<int>]
public sealed partial class CorpusLane {
 public static readonly CorpusLane Material = new(0, LaneUse.Id);
 public static readonly CorpusLane Type = new(1, LaneUse.Id);
 public static readonly CorpusLane Occurrence = new(2, LaneUse.Id);
 public static readonly CorpusLane AssessmentInput = new(3, LaneUse.Id);
 public static readonly CorpusLane Adjacency = new(4, LaneUse.Draw);
 public static readonly CorpusLane Value = new(5, LaneUse.Draw);
 public static readonly CorpusLane Observation = new(6, LaneUse.Draw);
 public static readonly CorpusLane Lattice = new(7, LaneUse.Id);
 public static readonly CorpusLane Interface = new(8, LaneUse.Id);

 public LaneUse Use { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CorpusProfile {
 private CorpusProfile(
  int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, int typeRatio,
  int observationStride, int observationSamples, int flavorStride, long seed) =>
  (Nodes, Density, BagWidth, Disciplines, Depth, TypeRatio, ObservationStride, ObservationSamples, FlavorStride, Seed) =
   (nodes, density, bagWidth, disciplines, depth, typeRatio, observationStride, observationSamples, flavorStride, seed);

 public int Nodes { get; }
 public double Density { get; }
 public int BagWidth { get; }
 public Seq<Discipline> Disciplines { get; }
 public int Depth { get; }
 public int TypeRatio { get; }
 public int ObservationStride { get; }
 public int ObservationSamples { get; }
 public int FlavorStride { get; }
 public long Seed { get; }

 public int TypeSlots => Math.Max(1, Nodes / TypeRatio);

 public int Fanout => Math.Max(2, (int)Math.Ceiling(Math.Pow(Nodes, 1.0 / Depth)));

 public static Fin<CorpusProfile> Of(
  int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, long seed, Op key,
  int typeRatio = 16, int observationStride = 4, int observationSamples = 16, int flavorStride = 8) =>
  Accumulate(Seq(
    In(nodes, Band.Positive, "corpus-profile-nodes", key).Map(static _ => unit),
    In(density, Band.Unit, "corpus-profile-density", key).Map(static _ => unit),
    In(bagWidth, Band.Positive, "corpus-profile-bag-width", key).Map(static _ => unit),
    Gate(!disciplines.IsEmpty, key, "<corpus-profile-disciplines-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    In(depth, Band.Positive, "corpus-profile-depth", key).Map(static _ => unit),
    In(typeRatio, Band.Positive, "corpus-profile-type-ratio", key).Map(static _ => unit),
    In(observationStride, Band.Positive, "corpus-profile-observation-stride", key).Map(static _ => unit),
    In(observationSamples, Band.Positive, "corpus-profile-observation-samples", key).Map(static _ => unit),
    In(flavorStride, Band.Positive, "corpus-profile-flavor-stride", key).Map(static _ => unit)))
   .Map(_ => new CorpusProfile(nodes, density, bagWidth, disciplines, depth, typeRatio, observationStride, observationSamples, flavorStride, seed))
   .ToFin();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GraphForge {
 const long CorpusUnixTicks = 17_672_256_000_000_000L;
 static int PropertyCases => CrossingFamily.PropertyValue.Arms;
 static int TemporalArms => CrossingFamily.TemporalValue.Arms;
 static int CompositionArms => CrossingFamily.MaterialComposition.Arms;
 static int PropertySetCases => CrossingFamily.MaterialPropertySet.Arms;
 const long CorpusLatticeSeed = 0L;
 static readonly Instant CorpusInstant = Instant.FromUnixTimeTicks(CorpusUnixTicks);
 static readonly NodeId Draft = Seeded(0, CorpusLane.Material, 0);
 static readonly Duration CorpusCadence = Duration.FromMinutes(15);

 public static Fin<(ElementGraph Graph, GraphDelta Delta, GraphDelta Mutation)> Mint(CorpusProfile profile, Op key) {
  Header header = Header.Default(CorpusInstant) with {
   Step = new StepHeader(Seq("corpus"), "corpus-model", CorpusInstant, Seq("GraphForge"), Seq("Rasm"),
    "GraphForge", "Rasm.Element", Seq(ReleaseVersion.Ifc4X3Add2.Key)),
   Units = new UnitScheme(
    Map((QuantityType.Create("Length"), "Millimeter")),
    Map((DimensionAxis.Length, new UnitAxis(0.001, 0.0, "mm"))),
    "", "G"),
  };
  double tol = header.Tolerance;
  int typeCount = profile.TypeSlots;
  return Classification.Of("corpus", "component", key).Bind(typeClass =>
   Classification.Of("corpus", "occurrence", key).Bind(occClass =>
    AnalysisRoute.Of("corpus.forge", key).Bind(route =>
     Assessments(profile, route, key).Bind(payloads =>
      Bags(profile, key).Bind(bags =>
       Series(profile, key).Bind(series =>
        Materials(profile, typeCount, tol, key).Bind(materials =>
         Usages(typeCount, key).Bind(usages =>
          Witnesses(header, tol, key).Map(witness =>
           Assembled(profile, header, tol, typeCount, typeClass, occClass, payloads, bags, series, materials, usages, witness))
           .Bind(built => Normalized(built.Delta, "mint", key).Bind(delta =>
            Normalized(built.Mutation, "mutation", key).Bind(mutation =>
             delta.AdmitOnto(ElementGraph.Genesis(header), key).Map(step =>
              (step.Graph, step.Delta, mutation)))))))))))));
 }

 static Fin<GraphDelta> Normalized(GraphDelta delta, string section, Op key) =>
  delta.NormalForm(key).ToFin().Map(_ => delta);

 static Fin<Seq<(int Index, ObservationSeries Series)>> Series(CorpusProfile profile, Op key) {
  QuantitySignature quantity = QuantitySignature.Create(
   QuantityType.Create("Temperature"), Dimension.Create(0, 0, 0, 0, 1, 0, 0), Some("K"));
  return toSeq(Enumerable.Range(0, profile.Nodes)).Filter(i => i % profile.ObservationStride == 0).TraverseM(i =>
   SensorId.Of($"corpus-sensor-{i}", key).Bind(sensor =>
    ObservationSeries.Open(
      sensor, PropertyName.Create("corpus-aspect"), quantity, SamplingKind.Averaged, Some(CorpusCadence),
      CorpusInstant, Some(new SensorProvenance("corpus", "GraphForge", $"{i}")), key)
     .Bind(opened => Chunked(profile, i, key).Bind(block =>
      SeriesStatistics
       .From(block.Run, SamplingKind.Averaged, quantity, key)
       .Bind(summary => opened.Append(block.Chunk, summary, key))))
     .Map(grown => (i, grown)))).As();
 }

 static Fin<(ObservationChunk Chunk, Seq<(Instant At, double Si, ObservationGrade Grade)> Run)> Chunked(
  CorpusProfile profile, int index, Op key) {
  Seq<(Instant At, double Si, ObservationGrade Grade)> run =
   toSeq(Enumerable.Range(0, profile.ObservationSamples)).Map(s => (
    CorpusInstant + (CorpusCadence * s),
    290.0 + (Deterministic.Unit(lanes: [CorpusLane.Observation.Key, index, s], seed: profile.Seed) * 10.0),
    s % 8 == 7 ? ObservationGrade.Suspect : ObservationGrade.Measured));
  return ObservationChunk.Encode(run, key).Map(block => (block.Chunk, run));
 }

 static NodeId Seeded(long seed, CorpusLane lane, int ordinal) =>
  NodeId.Create(SeededGuid(seed, lane, ordinal).ToString("N"));

 static Guid SeededGuid(long seed, CorpusLane lane, int ordinal) {
  Span<byte> b = stackalloc byte[16];
  ContentHash.Wire(Seed(seed, lane, ordinal)).Span.CopyTo(b);
  b[6] = (byte)((b[6] & 0x0F) | 0x70);
  b[8] = (byte)((b[8] & 0x3F) | 0x80);
  return new Guid(b, bigEndian: true);
 }

 static UInt128 Seed(long seed, CorpusLane lane, int ordinal) =>
  ContentHash.Of((seed, lane, ordinal), static (s, w) => w.I64(s.seed).Ordinal(s.lane.Key).Ordinal(s.ordinal));

 static Node Contented(Node draft, double tol) =>
  draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, tol)));

 static Fin<Seq<AssessmentPayload>> Assessments(CorpusProfile profile, AnalysisRoute route, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).TraverseM(i =>
   PropertyValue.Of(new PropertyValue.Number(0.5 + (i % 7) * 0.05), key).Bind(utilization =>
    from run in EvidenceRun.Of("corpus", "GraphForge", "1", CorpusInstant, key,
      elapsed: Duration.FromSeconds(1 + (i % 3)),
      window: i % 2 == 0 ? Some(new Interval(CorpusInstant, CorpusInstant + Duration.FromHours(1))) : None,
      correlation: i % 2 == 1 ? Some(CorrelationId.Create(SeededGuid(profile.Seed, CorpusLane.AssessmentInput, i))) : None,
      attempt: 1)
    from content in PayloadContent.Results(Map((PropertyName.Create("corpus-utilization"), utilization)), None, key)
    from payload in AssessmentPayload.Open(
     profile.Disciplines[i % profile.Disciplines.Count], route, Seed(profile.Seed, CorpusLane.AssessmentInput, i),
     AssessmentOutcome.Computed, content, run, key)
    select payload)).As();

 static Fin<Seq<(PropertyBag Props, QuantityBag Qty)>> Bags(CorpusProfile profile, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).TraverseM(i =>
   toSeq(Enumerable.Range(0, profile.BagWidth)).TraverseM(j =>
     Valued(profile, (i * profile.BagWidth) + j, i, key)
      .Map(value => (PropertyName.Create($"corpus-p{j}"), value))).As()
    .Bind(rows => MeasureValue.OfSi(QuantityType.Create("Volume"), Dimension.Create(3, 0, 0, 0, 0, 0, 0), 1.0 + i * 0.5)
     .Map(volume => (
      new PropertyBag("corpus-pset", rows.Fold(Map<PropertyName, PropertyValue>(), static (m, r) => m.Add(r.Item1, r.Item2)), InheritanceMode.OccurrenceWins, EvidenceGrade.Derived),
      new QuantityBag("corpus-qset", Map((PropertyName.Create("corpus-q0"), volume)), InheritanceMode.OccurrenceWins, EvidenceGrade.Derived))))).As();

 static Fin<PropertyValue> Valued(CorpusProfile profile, int slot, int occurrence, Op key) =>
  Raw(profile, slot, occurrence, key).Bind(value => PropertyValue.Of(value, key));

 static Fin<PropertyValue> Raw(CorpusProfile profile, int slot, int occurrence, Op key) {
  double draw = Deterministic.Unit(lanes: [CorpusLane.Value.Key, slot], seed: profile.Seed);
  return (slot % PropertyCases) switch {
   0 => Fin.Succ((PropertyValue)new PropertyValue.Text($"corpus-text-{slot}")),
   1 => Metre(draw + 1.0).Map(static m => (PropertyValue)new PropertyValue.Measure(m)),
   2 => Fin.Succ((PropertyValue)new PropertyValue.Boolean(slot % 2 == 0)),
   3 => Fin.Succ((PropertyValue)new PropertyValue.Logical(slot % 3 == 0 ? None : Some(slot % 3 == 1))),
   4 => Fin.Succ((PropertyValue)new PropertyValue.Integer(new System.Numerics.BigInteger(slot))),
   5 => Fin.Succ((PropertyValue)new PropertyValue.Number(draw)),
   6 => Fin.Succ((PropertyValue)new PropertyValue.Binary(toSeq(BitConverter.GetBytes(slot)))),
   7 => Fin.Succ((PropertyValue)new PropertyValue.Enumerated(
    Seq<PropertyValue>(new PropertyValue.Text($"corpus-grade-{slot % 3}")),
    Seq<PropertyValue>(new PropertyValue.Text("corpus-grade-0"), new PropertyValue.Text("corpus-grade-1"), new PropertyValue.Text("corpus-grade-2")))),
   8 => Fin.Succ((PropertyValue)new PropertyValue.Reference(Seeded(profile.Seed, CorpusLane.Occurrence, occurrence), Some("corpus-usage"))),
   9 => Metre(draw + 1.0).Bind(lower => Metre(draw + 2.0).Map(upper =>
    (PropertyValue)new PropertyValue.Bounded(Some(lower), Some(upper), None))),
   10 => Metre(draw + 1.0).Map(static m => (PropertyValue)new PropertyValue.List(
    Seq<PropertyValue>(new PropertyValue.Number(0.0), new PropertyValue.Measure(m)))),
   11 => Fin.Succ((PropertyValue)new PropertyValue.Table(
    Seq((Defining: (PropertyValue)new PropertyValue.Number(0.0), Defined: (PropertyValue)new PropertyValue.Number(draw))),
    Interpolation.Items[slot % Interpolation.Items.Count])),
   12 => Fin.Succ((PropertyValue)new PropertyValue.Complex("corpus-complex",
    Map((PropertyName.Create("corpus-inner"), (PropertyValue)new PropertyValue.Number(draw))))),
   13 => Timed(slot, key).Map(static value => (PropertyValue)new PropertyValue.Temporal(value)),
   var unreached => throw new InvalidOperationException($"<corpus-arm-unreached:property-value:{unreached}>"),
  };
 }

 static Fin<TemporalValue> Timed(int slot, Op key) => ((slot / PropertyCases) % TemporalArms) switch {
  0 => Fin.Succ<TemporalValue>(new TemporalValue.Date(CorpusInstant.InUtc().Date.PlusDays(slot))),
  1 => Fin.Succ<TemporalValue>(new TemporalValue.Moment(CorpusInstant.InUtc().LocalDateTime.PlusHours(slot))),
  2 => Fin.Succ<TemporalValue>(new TemporalValue.Time(CorpusInstant.InUtc().TimeOfDay.PlusMinutes(slot))),
  3 => Fin.Succ<TemporalValue>(new TemporalValue.Span(Period.FromDays(slot + 1))),
  4 => Fin.Succ<TemporalValue>(new TemporalValue.Stamp(CorpusInstant + (CorpusCadence * slot))),
  var unreached => throw new InvalidOperationException($"<corpus-arm-unreached:temporal:{unreached}>"),
 };

 static Fin<MeasureValue> Metre(double si) =>
  MeasureValue.OfSi(QuantityType.Create("Length"), Dimension.Create(1, 0, 0, 0, 0, 0, 0), si);

 static Fin<Seq<Node>> Materials(CorpusProfile profile, int typeCount, double tol, Op key) =>
  toSeq(Enumerable.Range(0, typeCount)).TraverseM(t => {
   MaterialId material = MaterialId.Of($"corpus-material-{t}");
   return Composed(material, t, key).Bind(composition =>
    Properties(t, key).Map(properties =>
     Contented(new Node.Material(Seeded(profile.Seed, CorpusLane.Material, t), material, composition, properties), tol)));
  }).As();

 static Fin<MaterialComposition> Composed(MaterialId material, int slot, Op key) => (slot % CompositionArms) switch {
  0 => Fin.Succ(MaterialComposition.OfSingle(material)),
  1 => Metre(0.1 + slot * 0.01).Bind(thickness => MaterialComposition.OfLayerSet(
   Seq(new MaterialLayer(material, thickness, $"corpus-layer-{slot}", Some(slot % 101), "corpus", Some(false))), key)),
  2 => MaterialComposition.OfProfileSet(
    Seq(new MaterialProfile(material, ProfileRef.Of("corpus", $"CP{slot}"), Some(slot % 101), "corpus", Seq<MeasureValue>())), key)
   .Bind(admitted => Section(slot, key).Map(admitted.WithSection)),
  3 => MaterialComposition.OfConstituentSet(
   Seq(new MaterialConstituent(material, "corpus", 0.5, "corpus-a"), new MaterialConstituent(material, "corpus", 0.5, "corpus-b")), key),
  var unreached => throw new InvalidOperationException($"<corpus-arm-unreached:composition:{unreached}>"),
 };

 static Fin<Seq<MaterialUsage>> Usages(int typeCount, Op key) =>
  toSeq(Enumerable.Range(0, typeCount)).TraverseM(t => (t % CompositionArms) switch {
   1 => Metre(0.05 + (t * 0.01)).Bind(offset => Metre(3.0 + t).Bind(extent =>
    MaterialUsage.LayerSet.Of(
     LayerSetDirection.Items[t % LayerSetDirection.Items.Count],
     DirectionSense.Items[t % DirectionSense.Items.Count],
     Some(offset), Some(extent), key))),
   2 => Metre(3.0 + t).Bind(extent =>
    MaterialUsage.ProfileSet.Of(Some(1 + (t % CardinalPoint.Items.Count)), Some(extent), key)),
   0 or 3 => Fin.Succ((MaterialUsage)new MaterialUsage.Unbound()),
   var unreached => throw new InvalidOperationException($"<corpus-arm-unreached:usage:{unreached}>"),
  }).As();

 static Fin<Seq<MaterialPropertySet>> Properties(int slot, Op key) =>
  toSeq(Enumerable.Range(0, PropertySetCases)).TraverseM(c => Property(c, slot, key)).As();

 static Fin<MaterialPropertySet> Property(int ordinal, int slot, Op key) {
  double scale = 1.0 + (slot % 8) * 0.125;
  return Evidence(ordinal, slot, key).Bind(evidence => ordinal switch {
   0 => MaterialPropertySet.OfMechanical(2400.0 * scale, 30_000.0 * scale, 400.0 * scale, 550.0 * scale, 0.2, 1.0e-5, key, evidence),
   1 => MaterialPropertySet.OfOrthotropic(500.0 * scale, 11_000.0 * scale, Some(7_400.0 * scale), 370.0 * scale, 690.0 * scale, 24.0 * scale, 2.5 * scale, 5.0e-6, key, evidence),
   2 => SampledCurve.Of(new[] { 20.0, 200.0, 600.0 }, new[] { 1.0 * scale, 1.1 * scale, 1.4 * scale }, key)
    .Bind(curve => MaterialPropertySet.OfThermal(1.7 * scale, 880.0 * scale, 0.25 * scale, 120.0, key, evidence, Some(curve))),
   3 => Acoustic.Of(Bands(static band => 0.05 + band * 0.05), Bands(band => 20.0 + band + scale), key)
    .Map(spectrum => MaterialPropertySet.OfAcoustic(spectrum, evidence)),
   4 => FireResistance.Of(Some(60), Some(90), Some(30), key)
    .Map(resistance => MaterialPropertySet.OfFire(FireRating.A1, resistance, evidence)),
   5 => MaterialPropertySet.OfEnvironmental(
    MeasurementBasis.PerM3, [.. Enumerable.Repeat(scale, ImpactCategory.Count * LifecycleStage.Count)], 0.3, 0.6, key, evidence),
   6 => Currency.Parse("EUR", key).Bind(currency =>
    MaterialPropertySet.OfCost(MeasurementBasis.PerM2, currency, 12.0 * scale, 8.0 * scale, 25.0 * scale, key, evidence)),
   7 => MaterialPropertySet.OfDamping(0.02 * scale, Some((0.5, 0.001)), key, evidence),
   8 => MaterialPropertySet.OfHygrothermal(0.18, 45.0 * scale, 180.0 * scale, Some(0.02), key, evidence),
   9 => MaterialPropertySet.OfDurability(3.5 * scale, 1.0e-12, 0.3, key, evidence),
   10 => MaterialPropertySet.OfOptical(0.6, 0.2, 0.2, 0.5, 0.25, 0.25, 0.0, 0.84, 0.84, key, evidence),
   _ => MaterialPropertySet.OfElectrical(1.72e-8 * scale, 2.0 + scale, Some(2.0e7 * scale), Some(1.0 + scale), key, evidence),
  });
 }

 static Fin<PropertyEvidence> Evidence(int ordinal, int slot, Op key) =>
  (ordinal % 3 == 2
    ? EvidenceRun.Of("corpus", "GraphForge", "1", CorpusInstant, key).Map(Some)
    : Fin.Succ(Option<EvidenceRun>.None))
   .Map(run => PropertyEvidence.Of(
     $"corpus-{ordinal}",
     EvidenceGrade.Items[ordinal % EvidenceGrade.Items.Count],
     reference: Some($"corpus-ref-{slot}"),
     validUntil: ordinal % 2 == 0 ? Some(CorpusInstant.InUtc().Date.PlusYears(1)) : None,
     attested: ordinal % 4 == 3
      ? Some(new Attestation(AttestationRole.Items[slot % AttestationRole.Items.Count], "corpus-cert",
         ContentAddress.Of(Seed(0, CorpusLane.Interface, ordinal)), CorpusInstant))
      : None,
     run: run));

 static Fin<SectionProperties> Section(int slot, Op key) {
  double s = 1.0 + (slot % 4) * 0.25;
  return Metre(1.16 * s).Bind(perimeter => SectionProperties.OfMillimetres(
   5_380.0 * s, 83_560_000.0 * s, 6_040_000.0 * s, 201_000.0 * s, 125_900_000_000.0 * s,
   557_000.0 * s, 80_500.0 * s, 628_000.0 * s, 125_200.0 * s,
   2_568.0 * s, 3_070.0 * s, 124.6, 33.5,
   300.0, 150.0, 1_160.0 * s, 35.0,
   0.0, 0.0, 0.0,
   new Vector3(75.0, 150.0, 0.0), Some(new SectionForm(12, 4, 0.35, perimeter)), key));
 }

 static double[] Bands(Func<int, double> shape) => [.. Enumerable.Range(0, AcousticBand.Count).Select(shape)];

 static Fin<(Node Appearance, Node Coverage)> Witnesses(Header header, double tol, Op key) =>
  AppearanceSummary.Of(AppearanceVector.Create(0.5, 0.5, 0.5, 0.0, 0.5, 1.0, transmissive: false), key).Bind(summary =>
   CellLattice.Of([1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0],
     LatticeAxis.Create(1), LatticeAxis.Create(1), LatticeAxis.Create(1), ceiling: 1L, key).Bind(lattice =>
    lattice.Coarsen(key).Bind(coarse =>
     CoverageBand.Of(0, "corpus-band", ChannelDtype.Float32, BandRole.Gray, key).Bind(band =>
      (from raster in ArtifactContent.Of(ContentHash.Wire(Seed(CorpusLatticeSeed, CorpusLane.Lattice, 0)).ToByteArray(), key)
       from overview in ArtifactContent.Of(ContentHash.Wire(Seed(CorpusLatticeSeed, CorpusLane.Lattice, 1)).ToByteArray(), key)
       from grid in CoverageGrid.Of(
        CoverageKind.Field, Seq(new OverviewLevel(lattice, raster), new OverviewLevel(coarse, overview)),
        Seq(band), header.Reference, key)
       select grid)
       .Map(grid => (
        Contented(new Node.Appearance(Draft, summary), tol),
        Contented(new Node.Coverage(Draft, grid), tol)))))));

 static (GraphDelta Delta, GraphDelta Mutation) Assembled(
  CorpusProfile profile, Header header, double tol, int typeCount, Classification typeClass, Classification occClass,
  Seq<AssessmentPayload> payloads, Seq<(PropertyBag Props, QuantityBag Qty)> bags,
  Seq<(int Index, ObservationSeries Series)> series, Seq<Node> materials, Seq<MaterialUsage> usages,
  (Node Appearance, Node Coverage) witness) {
  Seq<Node.Object> types = toSeq(Enumerable.Range(0, typeCount)).Map(t => {
   Node.Object draft = new(
    Id: Seeded(profile.Seed, CorpusLane.Type, t), Kind: ObjectKind.Type, ExternalId: None, Classification: typeClass,
    PredefinedType: PredefinedType.NotDefined, ObjectType: None, Name: $"corpus-type-{t}", Tag: "",
    Representations: RepresentationContentHash.Empty, History: None, Span: SchemaSpan.From(header.Schema));
   return (Node.Object)draft.Relabel(NodeId.Of(new NodeSeed.TypeSeed(draft, tol)));
  });
  Seq<Node.Object> occurrences = toSeq(Enumerable.Range(0, profile.Nodes)).Map(i => new Node.Object(
   Id: Seeded(profile.Seed, CorpusLane.Occurrence, i), Kind: ObjectKind.Occurrence, ExternalId: None, Classification: occClass,
   PredefinedType: PredefinedType.NotDefined, ObjectType: None, Name: $"corpus-occ-{i}", Tag: $"{i}",
   Representations: RepresentationContentHash.Empty,
   History: i % profile.FlavorStride == 0
    ? Some(new OwnerHistory("corpus-user", "GraphForge", CorpusInstant,
       i % (2 * profile.FlavorStride) == 0 ? Some(CorpusInstant + Duration.FromHours(1)) : None,
       "ADDED", "readwrite"))
    : None,
   Span: SchemaSpan.From(header.Schema)));
  Seq<Node> propertySets = bags.Map(pair => Contented(new Node.PropertySet(Draft, pair.Props), tol));
  Seq<Node> quantitySets = bags.Map(pair => Contented(new Node.QuantitySet(Draft, pair.Qty), tol));
  Seq<Node> assessments = payloads.Map(payload => Contented(new Node.Assessment(Draft, payload), tol));
  Seq<(int Index, Node Node)> observations = series.Map(row =>
   (row.Index, Contented(new Node.Observation(Draft, row.Series), tol)));
  Seq<Relationship> edges = toSeq(Enumerable.Range(0, profile.Nodes)).Bind(i => {
   NodeId occ = occurrences[i].Id;
   Seq<Relationship> spine = i == 0 ? Seq<Relationship>() : Seq<Relationship>(
    new Relationship.Compose(occurrences[(i - 1) / profile.Fanout].Id, occ, ComposeKind.Aggregate, None));
   return spine
    + Seq<Relationship>(
     new Relationship.Assign(occ, propertySets[i].Id, AssignKind.PropertyDefinition),
     new Relationship.Assign(occ, quantitySets[i].Id, AssignKind.PropertyDefinition),
     new Relationship.Assign(occ, assessments[i].Id, AssignKind.Assessment),
     new Relationship.Assign(occ, types[i % typeCount].Id, AssignKind.TypeDefinition),
     new Relationship.Associate(occ, materials[i % typeCount].Id, usages[i % typeCount]));
  });
  Seq<Relationship> measured = observations.Map(row =>
   (Relationship)new Relationship.Assign(occurrences[row.Index].Id, row.Node.Id, AssignKind.Observation));
  Seq<Relationship> resources = occurrences.Bind(occ => Seq<Relationship>(
   new Relationship.Associate(occ.Id, witness.Appearance.Id, new MaterialUsage.Unbound()),
   new Relationship.Associate(occ.Id, witness.Coverage.Id, new MaterialUsage.Unbound())));
  Seq<int> strided = toSeq(Enumerable.Range(1, Math.Max(0, profile.Nodes - 1))).Filter(i => i % profile.FlavorStride == 0);
  Seq<Relationship> flavors = strided
   .Bind(i => Seq<Relationship>(
    new Relationship.Compose(occurrences[i - 1].Id, occurrences[i].Id, ComposeKind.Contain, None),
    new Relationship.Compose(occurrences[i - 1].Id, occurrences[i].Id, ComposeKind.Nest, Some(i)),
    new Relationship.Compose(occurrences[i - 1].Id, occurrences[i].Id, ComposeKind.Reference, None),
    new Relationship.Void(occurrences[i - 1].Id, occurrences[i].Id, VoidKind.Void),
    new Relationship.Generic(
     WireName.Create("corpus.generic"), occurrences[i - 1].Id, occurrences[i].Id,
     Map((PropertyName.Create("corpus-buried-ref"), (PropertyValue)new PropertyValue.Reference(occurrences[i].Id))),
     Seq(new RelationshipParticipant(occurrences[i].Id, RoleName.Create("corpus-participant"), None)))));
  Seq<Relationship> realized = strided.Filter(static i => i > 1).Map(i => (Relationship)new Relationship.Connect(
   occurrences[i - 1].Id, occurrences[i].Id, ConnectKind.Element,
   Some(occurrences[0].Id), Some(Seed(profile.Seed, CorpusLane.Interface, i))));
  Seq<Relationship> adjacencies = toSeq(Enumerable.Range(0, (int)(profile.Density * profile.Nodes)))
   .Choose(c => {
    ulong state = Deterministic.Stream(lanes: [CorpusLane.Adjacency.Key, c], seed: profile.Seed);
    int from = Deterministic.NextBelow(state: ref state, exclusiveCeiling: profile.Nodes);
    int to = Deterministic.NextBelow(state: ref state, exclusiveCeiling: profile.Nodes);
    return from == to ? None : Some((Relationship)new Relationship.Connect(occurrences[from].Id, occurrences[to].Id, ConnectKind.Element, None, None));
   })
   .Distinct();
  GraphDelta mutation = new(
   Seq<Node>(),
   strided.Map(i => assessments[i].Id),
   strided.Map(i => ((Node)occurrences[i], (Node)new Node.Object(
    Id: occurrences[i].Id, Kind: ObjectKind.Occurrence, ExternalId: None, Classification: occClass,
    PredefinedType: PredefinedType.NotDefined, ObjectType: None, Name: occurrences[i].Name, Tag: $"{i}-revised",
    Representations: RepresentationContentHash.Empty, History: None, Span: SchemaSpan.From(header.Schema)))),
   Seq<Relationship>(),
   strided.Map(i => (Relationship)new Relationship.Assign(occurrences[i].Id, assessments[i].Id, AssignKind.Assessment)),
   None);
  return (new GraphDelta(
   materials + types.Map(static t => (Node)t) + occurrences.Map(static o => (Node)o)
    + propertySets + quantitySets + assessments + observations.Map(static row => row.Node)
    + Seq(witness.Appearance, witness.Coverage),
   Seq<NodeId>(), Seq<(Node, Node)>(),
   edges + adjacencies + measured + resources + flavors + realized, Seq<Relationship>(), Some(header)), mutation);
 }
}
```

## [03]-[CORPUS_ROSTER]

- Owner: `CorpusGrade` the `[SmartEnum<string>]` size roster; `CorpusOp` the `[SmartEnum<string>]` hot-path vocabulary with a witness-returning run column; `CorpusModel` the minted carrier with its snapshot fingerprint; `CorpusWitness` the operation evidence; `CorpusGate` the mint/determinism capability.
- Cases: `CorpusGrade` rows `S`/`M`/`L`/`XL`; `CorpusOp` rows `Bake`, native `Freeze`, `CanonicalBytes`, `EncodeNode`, `DecodeNode`, and `Tabulate`. Node encoding exercises the generated support closure without inventing a graph actor.
- Entry: `CorpusGate.Mint(grade, key)` forges once and stamps its native snapshot fingerprint. `Stable(grade, key)` proves integer-wire canonicality, forges twice, and rejects native-address drift. Each `CorpusOp.Run` returns a witness; node decode rejects content-address drift against the selected native node.
- Receipt: `CorpusWitness` proves traversal and binds each operation result to its input fingerprint. Tests-estate timing maps it into AppHost `BenchmarkReceipt`: `Suite = "Rasm.Element"`, `Case = $"{grade}/{op}"`, `Corpus = Some(witness.Snapshot.Value)`, and `Operations = witness.Magnitude`; AppHost owns host evidence, timing, allocation, verdict, artifact key, and correlation.
- Packages: Google.Protobuf writes and parses generated node messages; CommunityToolkit.HighPerformance supplies the pooled encode sink; `Rasm` supplies `Op.Catch`. BenchmarkDotNet and CsCheck consume this roster from tests.
- Growth: a new scale is one `CorpusGrade` row; a new hot path is one `CorpusOp` row and witness arm. Benchmarks reuse one minted model per grade.
- Boundary: Element owns models, operation vocabulary, and semantic witnesses. AppHost owns benchmark receipts and regression verdicts. `XL` belongs to benchmark hosts, never unit defaults. This corpus is branch-local test data, not a manifest actor or a hand-maintained cross-language mirror; `CorpusGate.Stable` proves native repeatability.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record CorpusModel(
 CorpusGrade Grade, ElementGraph Graph, GraphDelta Delta, GraphDelta Mutation, ContentAddress Snapshot);

public sealed record CorpusWitness(
 CorpusGrade Grade, CorpusOp Operation, long Magnitude, ContentAddress Snapshot, ContentAddress Artifact);

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CorpusGrade {
 public static readonly CorpusGrade S = new("s",
  Row(64, 0.10, 4, Seq(Discipline.Structural, Discipline.Thermal), 2, 1001));
 public static readonly CorpusGrade M = new("m",
  Row(1_024, 0.15, 8, Seq(Discipline.Structural, Discipline.Thermal, Discipline.Energy), 3, 1002));
 public static readonly CorpusGrade L = new("l",
  Row(16_384, 0.20, 12, Seq(Discipline.Structural, Discipline.Thermal, Discipline.Energy, Discipline.Acoustic), 4, 1003));
 public static readonly CorpusGrade XL = new("xl",
  Row(262_144, 0.25, 16, Seq(Discipline.Structural, Discipline.Thermal, Discipline.Energy, Discipline.Acoustic, Discipline.Fire), 5, 1004));

 public CorpusProfile Profile { get; }

 static CorpusProfile Row(int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, long seed) =>
  CorpusProfile.Of(nodes, density, bagWidth, disciplines, depth, seed, Op.Of(name: nameof(CorpusGrade))).Match(
   Succ: static profile => profile,
   Fail: static _ => throw new InvalidOperationException("Corpus grade declaration violates CorpusProfile admission."));
}

[SmartEnum<string>]
public sealed partial class CorpusOp {
 public static readonly CorpusOp Bake = new("bake", RunBake);
 public static readonly CorpusOp Freeze = new("freeze", RunFreeze);
 public static readonly CorpusOp CanonicalBytes = new("canonical-bytes", RunCanonicalBytes);
 public static readonly CorpusOp EncodeNode = new("encode-node", RunEncodeNode);
 public static readonly CorpusOp DecodeNode = new("decode-node", RunDecodeNode);
 public static readonly CorpusOp Tabulate = new("tabulate", RunTabulate);

 [UseDelegateFromConstructor]
 public partial Fin<CorpusWitness> Run(CorpusModel model, Op key);

 static Fin<CorpusWitness> RunBake(CorpusModel model, Op key) =>
  model.Graph.ObjectNodes.TraverseM(root => model.Graph.Bake(root.Id, key)).As()
   .Bind(elements => Witness(model, Bake, elements.Count, model.Snapshot, key));

 static Fin<CorpusWitness> RunFreeze(CorpusModel model, Op key) {
  ElementGraph frozen = WorkingGraph.Thaw(model.Graph).Freeze(model.Graph.Header);
  return Checked(model, Freeze, frozen.Nodes.Count, ContentAddress.OfGraph(frozen), model.Snapshot, key);
 }

 static Fin<CorpusWitness> RunCanonicalBytes(CorpusModel model, Op key) =>
  model.Delta.ToCanonicalBytes(model.Graph.Header.Tolerance, key).Bind(bytes =>
   Witness(model, CanonicalBytes, bytes.Length, ContentAddress.Of(bytes.Span), key));

 static Node Selected(CorpusModel model) =>
  model.Graph.Nodes.Values.OrderBy(static node => node.Id.Value, StringComparer.Ordinal).First();

 static Fin<CorpusWitness> RunEncodeNode(CorpusModel model, Op key) {
  Node node = Selected(model);
  return ElementWire.Encode(node, model.Graph.Header.Tolerance, key).Bind(wire => key.Catch(() => {
   using ArrayPoolBufferWriter<byte> sink = new();
   wire.WriteTo(sink);
   return Witness(model, EncodeNode, sink.WrittenCount, ContentAddress.Of(sink.WrittenSpan), key);
  }));
 }

 static Fin<CorpusWitness> RunDecodeNode(CorpusModel model, Op key) {
  Node node = Selected(model);
  double tolerance = model.Graph.Header.Tolerance;
  return ElementWire.Encode(node, tolerance, key).Bind(wire => key.Catch(() => {
   using MemoryStream payload = new();
   wire.WriteTo(payload);
   payload.Position = 0;
   global::Rasm.Contracts.Element.NodeWire parsed =
    global::Rasm.Contracts.Element.NodeWire.Parser.ParseFrom(payload);
   return ElementWire.Decode(parsed, key).Bind(decoded =>
    Checked(
     model, DecodeNode, payload.Length,
     ContentAddress.Of(decoded, tolerance), ContentAddress.Of(node, tolerance), key));
  }));
 }

 static Fin<CorpusWitness> RunTabulate(CorpusModel model, Op key) =>
  GraphTable.Tabulate(model.Graph, key).Bind(snapshot =>
   snapshot.Batches(key).Bind(_ => Witness(model, Tabulate, snapshot.Rows.Count, snapshot.Address, key)));
 static Fin<CorpusWitness> Checked(
  CorpusModel model, CorpusOp operation, long magnitude, ContentAddress artifact, ContentAddress expected, Op key) =>
  artifact == expected
   ? Witness(model, operation, magnitude, artifact, key)
   : new ElementFault.ValueRejected(key, $"<corpus-drift:{operation.Key}:{model.Grade.Key}>");

 static Fin<CorpusWitness> Witness(
  CorpusModel model, CorpusOp operation, long magnitude, ContentAddress artifact, Op key) =>
  magnitude > 0
   ? Fin.Succ(new CorpusWitness(model.Grade, operation, magnitude, model.Snapshot, artifact))
   : new ElementFault.ValueRejected(key, $"<corpus-operation-empty:{model.Grade.Key}:{operation.Key}>");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CorpusGate {
 static readonly Seq<(ByteString Bytes, bool Accepted)> IntegerWireCases = Seq(
  (ByteString.CopyFrom([0x00]), true),
  (ByteString.CopyFrom([0xFF]), true),
  (ByteString.CopyFrom([0x00, 0x80]), true),
  (ByteString.CopyFrom([0xFF, 0x7F]), true),
  (ByteString.Empty, false),
  (ByteString.CopyFrom([0x00, 0x00]), false),
  (ByteString.CopyFrom([0x00, 0x01]), false),
  (ByteString.CopyFrom([0xFF, 0xFF]), false),
  (ByteString.CopyFrom([0xFF, 0xFE]), false));

 public static Fin<CorpusModel> Mint(CorpusGrade grade, Op key) =>
  GraphForge.Mint(grade.Profile, key).Map(step =>
   new CorpusModel(grade, step.Graph, step.Delta, step.Mutation, ContentAddress.OfGraph(step.Graph)));

 public static Fin<ContentAddress> Stable(CorpusGrade grade, Op key) =>
  IntegerCanonicality(key).Bind(_ => Mint(grade, key).Bind(first => Mint(grade, key).Bind(second =>
   first.Snapshot == second.Snapshot
    ? Fin.Succ(first.Snapshot)
    : Fin.Fail<ContentAddress>(new ElementFault.ValueRejected(key, $"<corpus-nondeterministic:{grade.Key}>")))));

 static Fin<Unit> IntegerCanonicality(Op key) => IntegerWireCases.TraverseM(row => {
  Fin<PropertyValue> admitted = WireCodec.ToValue(
   new global::Rasm.Contracts.Element.PropertyValueWire { Integer = row.Bytes }, key);
  return admitted.IsSucc == row.Accepted
   ? Fin.Succ(unit)
   : Fin.Fail<Unit>(new ElementFault.ValueRejected(
      key, $"<integer-wire-canonicality:{Convert.ToHexString(row.Bytes.Span)}:expected={row.Accepted}>"));
 }).As().Map(static _ => unit);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
