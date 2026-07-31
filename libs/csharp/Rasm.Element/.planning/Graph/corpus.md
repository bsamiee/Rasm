# [ELEMENT_CORPUS]

`GraphForge` owns deterministic synthetic models and the graded roster shared by benchmarks, property specifications, and cross-runtime parity. `CorpusProfile` closes occurrence count, edge density, bag width, discipline mix, composition depth, and seed. `GraphForge.Mint` admits every forged node and edge through `GraphDelta.AdmitOnto` over `Genesis`, exercising `LegalLink`, freeze, and incidence construction.

Occurrence ids derive from kernel `ContentHash` over `(seed, lane, ordinal)` and carry the Guid-v7 layout. Type ids use `NodeId.RootedType`; non-rooted ids use `NodeId.Content`. Magnitudes and index draws derive from the kernel `Deterministic` splitmix stream, never from a digest, so identity and derivation stay two rails. Each grade therefore reproduces one snapshot fingerprint on every runtime sharing the seed-zero content rail.

`GraphForge` composes the existing `ElementGraph`, `GraphDelta`, `ContentAddress`, `CanonicalWriter`, and `ElementWire` owners. Forged payloads re-enter `Classification.Of`, `PropertyValue.Of`, `MeasureValue.OfSi`, `AssessmentPayload.Computed`, and `AnalysisRoute.Of`; every refusal carries `ElementFault`.

## [01]-[INDEX]

- [02]-[GRAPH_FORGE]: `CorpusProfile` closes the parameter record and `GraphForge` mints deterministically — the seeded id stream, the payload kernels, and the one `AdmitOnto` realization every forged model crosses.
- [03]-[CORPUS_ROSTER]: `CorpusGrade` size rows, the `CorpusOp` hot-path vocabulary, `CorpusModel` and `CorpusWitness`, and the `CorpusGate` mint/determinism entries consumed by tests-estate benchmark and property lanes.

## [02]-[GRAPH_FORGE]

- Owner: `CorpusProfile` the closed generation-parameter record — occurrence count, `[0,1]` edge density, bag width, discipline mix, composition depth, seed — railed through `Of`; `GraphForge` the deterministic realization fold.
- Entry: `CorpusProfile.Of(nodes, density, bagWidth, disciplines, depth, seed, key)` admits positive counts, a unit-interval density, a non-empty discipline mix, and a depth of at least one, railing `ElementFault.ValueRejected` otherwise; `GraphForge.Mint(profile, key)` realizes the profile into `Fin<(ElementGraph Graph, GraphDelta Delta)>` — the frozen snapshot a benchmark folds and the normal-form event body the delta legs decode.
- Auto: `Mint` builds one shared corpus header (`Header.Default` over the fixed corpus instant, so header bytes never fork a grade), one `Node.Material` per type slot, one deterministic Type `Object` per slot (id through the production `NodeId.RootedType` over `ToTypeSeedBytes`), then per occurrence one seeded Guid-v7 `Object`, one property bag of `BagWidth` seed-derived `Number` rows, one quantity bag row through `MeasureValue.OfSi`, one `Computed` assessment cycling the discipline mix, and — every `ObservationStride`-th occurrence — one `ObservationSeries` opened at the corpus instant and grown by one `Encode`-minted chunk under a `From`-derived summary; edges land as the `Aggregate` fanout spine (depth-derived fanout), the `PropertyDefinition`/`Assessment`/`TypeDefinition`/`Observation` assigns, the material `Associate`, and `⌊density·nodes⌋` seeded `Connect` adjacencies; the assembled normal-form delta admits through `AdmitOnto(Genesis(header))` so `LegalLink` runs per forged edge.
- Receipt: the mint result carries the frozen graph and normal-form delta; `ContentAddress.OfGraph` supplies its reproducibility fingerprint.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Map`/`TraverseM`), Thinktecture.Runtime.Extensions (generated owners), NodaTime (`Instant.FromUnixTimeTicks`/`Duration.Zero` fixed provenance), `Rasm` (`ContentHash` the id rail, `Deterministic.Stream`/`Unit`/`NextBelow` the draw rail, `Op`), and System.Buffers.Binary (`BinaryPrimitives` Guid shaping).
- Growth: a new payload family in the forge is one kernel arm beside the existing node kernels; a new generation axis is one `CorpusProfile` column threaded into the kernels, and a new random axis is one draw lane on `Deterministic` — never a sibling forge, never a parameter whose value the seed cannot replay, and never a magnitude projected off an id digest.
- Boundary: the forge composes ONLY the seam's own admissions — a raw case constructor bypassing `Classification.Of`, `MeasureValue.OfSi`, or `AssessmentPayload.Computed` forges models no production projector can produce, so every railed admission the graph demands runs inside `Mint`; the delta is constructed wholesale in normal form (ids unique by the seeded stream) and still crosses `AdmitOnto` — `ReplayOnto` trusts only seam-produced deltas and the forge counts as foreign to the structural law; determinism never rides a runtime PRNG — a `Random(seed)` stream couples the corpus to a BCL implementation — and it splits by AXIS across the two kernel owners: an ID replays through `ContentHash` over `(seed, lane, ordinal)` and a MAGNITUDE or index draw through `Deterministic`, so neither a hash-seeded sampler (which the kernel rejects by design) nor a modulo-biased projection off a digest survives here; the generation loops are the named measured-kernel statement seam, confined to the forge kernels.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using Google.Protobuf;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Graph;

// --- [MODELS] -----------------------------------------------------------------------------
// Closed generation-parameter record: Nodes counts OCCURRENCE objects (types, materials, bags, and
// assessments derive), Density in [0,1] scales the extra Connect adjacencies, BagWidth the property rows per
// bag, Disciplines the assessment cycle, Depth the Aggregate spine depth, Seed the whole id/value stream.
public sealed record CorpusProfile {
 private CorpusProfile(int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, long seed) =>
  (Nodes, Density, BagWidth, Disciplines, Depth, Seed) = (nodes, density, bagWidth, disciplines, depth, seed);

 public int Nodes { get; }
 public double Density { get; }
 public int BagWidth { get; }
 public Seq<Discipline> Disciplines { get; }
 public int Depth { get; }
 public long Seed { get; }

 // Aggregate spine fanout implied by depth: each occurrence i > 0 composes under occurrence (i-1)/Fanout.
 public int Fanout => Math.Max(2, (int)Math.Ceiling(Math.Pow(Nodes, 1.0 / Depth)));

 public static Fin<CorpusProfile> Of(int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, long seed, Op key) =>
  nodes > 0 && density is >= 0.0 and <= 1.0 && bagWidth > 0 && !disciplines.IsEmpty && depth > 0
   ? Fin.Succ(new CorpusProfile(nodes, density, bagWidth, disciplines, depth, seed))
   : ElementFault.ValueRejected(key, $"<corpus-profile-invalid:{nodes}:{density}:{bagWidth}:{depth}>");
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Deterministic realization fold on TWO kernel rails that never cross: every ID derives from the ContentHash over
// (Seed, lane, ordinal) — a content key, replayable across runtimes — while every MAGNITUDE and every index draw
// derives from Deterministic, the kernel's one draw owner. The kernel rules a sampler seeded from a ContentHash a
// design defect, and the concrete cost the split repays is bias: a hash modulo a non-power-of-two ceiling weights
// its low residues, so a `% 1000` magnitude and a `% Nodes` index each skewed the corpus in the exact direction a
// parity gate cannot see. Deterministic.Unit takes the top 53 bits and NextBelow rejects the biased tail, so the
// forged distribution is flat by construction. The assembled normal-form delta still crosses AdmitOnto over Genesis
// so LegalLink runs per forged edge — the forge is FOREIGN to the structural law, exactly like a wire payload.
// Generation loops are the named measured-kernel statement seam.
public static class GraphForge {
 const long CorpusUnixTicks = 17_672_256_000_000_000L;
 // Every ObservationStride-th occurrence carries a measured series of ObservationSamples readings, so the eighth
 // node case, its ordinal-7 canonical arm, the wire oneof arm, the occurrence-only assign legality, and the chunk
 // blob codec all cross every parity gate instead of riding untested behind arms no witness reaches.
 const int ObservationStride = 4;
 const int ObservationSamples = 16;
 // The two DRAW lanes, distinct from the id lanes the Seed fold owns: identity and derivation never share a stream,
 // so a lane number here can never collide with one there even though both are keyed off the same profile seed.
 const long ObservationLane = 6L;
 const long AdjacencyLane = 4L;
 static readonly Instant CorpusInstant = Instant.FromUnixTimeTicks(CorpusUnixTicks);
 static readonly Duration CorpusCadence = Duration.FromMinutes(15);

 public static Fin<(ElementGraph Graph, GraphDelta Delta)> Mint(CorpusProfile profile, Op key) {
  Header header = Header.Default(CorpusInstant);
  double tol = header.Tolerance;
  int typeCount = Math.Max(1, profile.Nodes / 16);
  return Classification.Of("corpus", "component", key).Bind(typeClass =>
   Classification.Of("corpus", "occurrence", key).Bind(occClass =>
    AnalysisRoute.Of("corpus.forge", key).Bind(route =>
     Assessments(profile, route, key).Bind(payloads =>
      Bags(profile, key).Bind(bags =>
       Series(profile, key).Map(series => Assembled(profile, header, tol, typeCount, typeClass, occClass, payloads, bags, series)))
       .Bind(delta => delta.AdmitOnto(ElementGraph.Genesis(header), key))))));
 }

 // One measured series per strided occurrence, each opened at the corpus instant and grown by ONE real encoded
 // chunk — the forge crosses Open, Encode, From, and Append exactly as a live producer does, so a codec or
 // admission regression surfaces at the parity gate rather than at the first metered deployment. ONE chunk is
 // what makes the per-block summary exact: Append proves the census total against the WHOLE grown run, and a
 // fresh Open carries zero prior samples, so this block's From result IS that total. A second chunk on one
 // series therefore recomputes From over the concatenated run — a per-chunk summary appended second refuses at
 // the census gate rather than landing a series whose statistics describe its tail alone.
 static Fin<Seq<(int Index, ObservationSeries Series)>> Series(CorpusProfile profile, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).Filter(static i => i % ObservationStride == 0).TraverseM(i =>
   SensorId.Of($"corpus-sensor-{i}", key).Bind(sensor =>
    ObservationSeries.Open(
      sensor, PropertyName.Create("corpus-aspect"), QuantityType.Create("Temperature"),
      Dimension.Create(0, 0, 0, 0, 1, 0, 0), "K", SamplingKind.Averaged, Some(CorpusCadence),
      CorpusInstant, new SensorProvenance("corpus", "GraphForge", $"{i}"), key)
     .Bind(opened => Chunked(profile, i, key).Bind(block =>
      SeriesStatistics
       .From(block.Run, SamplingKind.Averaged, QuantityType.Create("Temperature"), Dimension.Create(0, 0, 0, 0, 1, 0, 0), "K", key)
       .Bind(summary => opened.Append(block.Chunk, summary, key))))
     .Map(grown => (i, grown)))).As();

 // Seeded sample run: instants advance by the declared cadence off the corpus instant and each magnitude derives
 // from the one seed fold, so bytes, content key, and summary replay identically on every runtime sharing the seed.
 // A positive cadence multiplied by a strictly rising ordinal is what carries Encode's strict-adjacency gate: the
 // run never repeats or reverses an instant, so the gate holds by construction rather than by a sorted fixture.
 // Every eighth reading grades Suspect, so the completeness screen reads a real consumable share rather than 1.0.
 static Fin<(ObservationChunk Chunk, Seq<(Instant At, double Si, ObservationGrade Grade)> Run)> Chunked(
  CorpusProfile profile, int index, Op key) {
  Seq<(Instant At, double Si, ObservationGrade Grade)> run =
   toSeq(Enumerable.Range(0, ObservationSamples)).Map(s => (
    CorpusInstant + (CorpusCadence * s),
    290.0 + (Deterministic.Unit(lanes: [ObservationLane, index, s], seed: profile.Seed) * 10.0),
    s % 8 == 7 ? ObservationGrade.Suspect : ObservationGrade.Measured));
  return ObservationChunk.Encode(run, key).Map(block => (block.Chunk, run));
 }

 // Seed-derived rooted id: kernel ContentHash over (seed, lane, ordinal) shaped into the Guid-v7 layout
 // NodeId.Rooted emits — version nibble and RFC variant forced, time-sortability surrendered for replay.
 static NodeId Seeded(long seed, int lane, int ordinal) {
  Span<byte> b = stackalloc byte[16];
  BinaryPrimitives.WriteUInt128LittleEndian(b, Seed(seed, lane, ordinal));
  b[7] = (byte)((b[7] & 0x0F) | 0x70);
  b[8] = (byte)((b[8] & 0x3F) | 0x80);
  return NodeId.Create(new Guid(b).ToString("N"));
 }

 // One seed fold owns every lane; id and payload projections consume its UInt128 result.
 static UInt128 Seed(long seed, int lane, int ordinal) {
  CanonicalWriter w = new(0.0);
  w.I64(seed).Ordinal(lane).Ordinal(ordinal);
  return ContentHash.Of(w.ToBytes().Span);
 }

 // Non-rooted mint: content-derive the id from the draft's own canonical bytes (id-excluded, so a shared
 // placeholder-safe: bytes exclude the id) and re-stamp — the NodeId.Content regime at the forge altitude.
 static Node Contented(Node draft, double tol) =>
  draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tol).Span));

 static Fin<Seq<AssessmentPayload>> Assessments(CorpusProfile profile, AnalysisRoute route, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).TraverseM(i =>
   PropertyValue.Of(new PropertyValue.Number(0.5 + (i % 7) * 0.05), key).Bind(utilization =>
    AssessmentPayload.Computed(
     profile.Disciplines[i % profile.Disciplines.Count], route, Seed(profile.Seed, lane: 3, i),
     Map((PropertyName.Create("corpus-utilization"), utilization)), None,
     new Provenance("corpus", "GraphForge", "1", CorpusInstant, Duration.Zero, None, None, 1), key,
     Seq<NodeId>()))).As();

 // One property bag (BagWidth seeded Number rows) and one quantity bag (a single [L3] volume row through the
 // OfSi registry gate) per occurrence — the counted-bag canonical layout every parity consumer re-hashes.
 static Fin<Seq<(PropertyBag Props, QuantityBag Qty)>> Bags(CorpusProfile profile, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).TraverseM(i =>
   toSeq(Enumerable.Range(0, profile.BagWidth)).TraverseM(j =>
     PropertyValue.Of(new PropertyValue.Number((profile.Seed % 97) + i + j * 0.25), key)
      .Map(value => (PropertyName.Create($"corpus-p{j}"), value))).As()
    .Bind(rows => MeasureValue.OfSi(QuantityType.Create("Volume"), Dimension.Create(3, 0, 0, 0, 0, 0, 0), 1.0 + i * 0.5)
     .Map(volume => (
      new PropertyBag("corpus-pset", rows.Fold(Map<PropertyName, PropertyValue>(), static (m, r) => m.Add(r.Item1, r.Item2)), InheritanceMode.OccurrenceWins, PropertySource.Derived),
      new QuantityBag("corpus-qset", Map((PropertyName.Create("corpus-q0"), volume)), InheritanceMode.OccurrenceWins, PropertySource.Derived))))).As();

 // Wholesale normal-form assembly: ids are unique by the seeded stream, so the delta is IsNormalForm by
 // construction and AdmitOnto supplies the structural proof.
 static GraphDelta Assembled(
  CorpusProfile profile, Header header, double tol, int typeCount, Classification typeClass, Classification occClass,
  Seq<AssessmentPayload> payloads, Seq<(PropertyBag Props, QuantityBag Qty)> bags,
  Seq<(int Index, ObservationSeries Series)> series) {
  Seq<Node> materials = toSeq(Enumerable.Range(0, typeCount)).Map(t => {
   MaterialId material = MaterialId.Of($"corpus-material-{t}");
   return Contented(new Node.Material(
    Seeded(profile.Seed, lane: 0, t), material, MaterialComposition.OfSingle(material), Seq<MaterialPropertySet>()), tol);
  });
  Seq<Node.Object> types = toSeq(Enumerable.Range(0, typeCount)).Map(t => {
   Node.Object draft = new(
    Id: Seeded(profile.Seed, lane: 1, t), Kind: ObjectKind.Type, ExternalId: None, Classification: typeClass,
    PredefinedType: PredefinedType.NotDefined, ObjectType: None, Name: $"corpus-type-{t}", Tag: "",
    Representations: RepresentationContentHash.Empty, History: None, Span: SchemaSpan.From(header.Schema));
   return (Node.Object)draft.Relabel(NodeId.RootedType(draft.ToTypeSeedBytes(tol).Span));
  });
  Seq<Node.Object> occurrences = toSeq(Enumerable.Range(0, profile.Nodes)).Map(i => new Node.Object(
   Id: Seeded(profile.Seed, lane: 2, i), Kind: ObjectKind.Occurrence, ExternalId: None, Classification: occClass,
   PredefinedType: PredefinedType.NotDefined, ObjectType: None, Name: $"corpus-occ-{i}", Tag: $"{i}",
   Representations: RepresentationContentHash.Empty, History: None, Span: SchemaSpan.From(header.Schema)));
  Seq<Node> propertySets = bags.Map(pair => Contented(new Node.PropertySet(Seeded(0, 0, 0), pair.Props), tol));
  Seq<Node> quantitySets = bags.Map(pair => Contented(new Node.QuantitySet(Seeded(0, 0, 0), pair.Qty), tol));
  Seq<Node> assessments = payloads.Map(payload => Contented(new Node.Assessment(Seeded(0, 0, 0), payload), tol));
  // Observation nodes content-key off the STREAM identity exactly as a live one does, so a re-mint of the same
  // profile addresses the same node and the strided share stays a stable fraction of every grade.
  Seq<(int Index, Node Node)> observations = series.Map(row =>
   (row.Index, Contented(new Node.Observation(Seeded(0, 0, 0), row.Series), tol)));
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
     new Relationship.Associate(occ, materials[i % typeCount].Id, new MaterialUsage.None()));
  });
  // Observation assigns ride the OCCURRENCE alone, which is exactly the legality arm LegalAssign enforces — a type
  // slot never receives one, so the forge exercises the refusal boundary by construction rather than by a fixture.
  Seq<Relationship> measured = observations.Map(row =>
   (Relationship)new Relationship.Assign(occurrences[row.Index].Id, row.Node.Id, AssignKind.Observation));
  Seq<Relationship> adjacencies = toSeq(Enumerable.Range(0, (int)(profile.Density * profile.Nodes)))
   .Choose(c => {
    ulong state = Deterministic.Stream(lanes: [AdjacencyLane, c], seed: profile.Seed);
    int from = Deterministic.NextBelow(state: ref state, exclusiveCeiling: profile.Nodes);
    int to = Deterministic.NextBelow(state: ref state, exclusiveCeiling: profile.Nodes);
    return from == to ? None : Some((Relationship)new Relationship.Connect(occurrences[from].Id, occurrences[to].Id, ConnectKind.Element, None, None));
   })
   .Distinct();
  return new GraphDelta(
   materials + types.Map(static t => (Node)t) + occurrences.Map(static o => (Node)o)
    + propertySets + quantitySets + assessments + observations.Map(static row => row.Node),
   Seq<NodeId>(), Seq<(Node, Node)>(), edges + adjacencies + measured, Seq<Relationship>(), Some(header));
 }
}
```

## [03]-[CORPUS_ROSTER]

- Owner: `CorpusGrade` the `[SmartEnum<string>]` size roster; `CorpusOp` the `[SmartEnum<string>]` hot-path vocabulary with a witness-returning run column; `CorpusModel` the minted carrier with its snapshot fingerprint; `CorpusWitness` the operation evidence; `CorpusGate` the mint/determinism capability.
- Cases: `CorpusGrade` rows `S`/`M`/`L`/`XL` (64 → 262 144 occurrences, density, bag width, depth, and discipline mix widening together); `CorpusOp` rows `Bake` (every object root through the memoized fold), `Freeze` (thaw-and-freeze rebuild of the frozen structures), `CanonicalBytes` (the delta content-key projection), `Encode` (the full snapshot wire lowering), `DecodeGraph` (the round-trip re-admission under `WireLimits.Default`); the closed hot-path family.
- Entry: `CorpusGate.Mint(grade, key)` forges once and stamps its snapshot fingerprint. `Stable(grade, key)` forges twice and rejects address drift. Each `CorpusOp.Run(model, key)` returns a `CorpusWitness` with grade, operation, magnitude, snapshot, and operation artifact address; freeze and decode rows reject snapshot drift.
- Receipt: `CorpusWitness` proves traversal and binds each operation result to its input fingerprint. Tests-estate timing maps it into AppHost `BenchmarkReceipt`: `Suite = "Rasm.Element"`, `Case = $"{grade}/{op}"`, `Corpus = Some(witness.Snapshot.Value)`, and `Operations = witness.Magnitude`; AppHost owns host evidence, timing, allocation, verdict, artifact key, and correlation.
- Packages: BenchmarkDotNet and CsCheck consume this roster FROM THE TESTS ESTATE — the benchmark lane iterates `CorpusOp.Items` per grade and the property lane drives `Stable` and the wire round-trip over seeds; both are central-manifest packages with no `Rasm.Element` reference, so the seam page names the consumption shape and no benchmark member.
- Growth: a new scale is one `CorpusGrade` row; a new hot path is one `CorpusOp` row and witness arm. Benchmarks reuse one minted model per grade.
- Boundary: Element owns models, operation vocabulary, and semantic witnesses. AppHost owns benchmark receipts and regression verdicts. Snapshot literals remain outside settled code until the corpus harness produces exact current addresses; an unset placeholder cannot masquerade as a parity gate. `XL` belongs to benchmark hosts, never unit defaults.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Minted carrier reused per operation, with the input fingerprint computed once.
public sealed record CorpusModel(CorpusGrade Grade, ElementGraph Graph, GraphDelta Delta, ContentAddress Snapshot);

// Semantic operation evidence. AppHost maps this witness into its BenchmarkReceipt owner at the tests edge.
public sealed record CorpusWitness(
 CorpusGrade Grade, CorpusOp Operation, long Magnitude, ContentAddress Snapshot, ContentAddress Artifact);

// --- [TABLES] -----------------------------------------------------------------------------
// Graded roster: one CorpusProfile per row, with discipline mix and density widening by scale.
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

 // Roster rows are declaration-total: a failed profile literal is a construction defect, so the throwing unwrap
 // stays at roster materialization and never reaches call sites.
 static CorpusProfile Row(int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, long seed) =>
  CorpusProfile.Of(nodes, density, bagWidth, disciplines, depth, seed, Op.Of(name: nameof(CorpusGrade))).ThrowIfFail();
}

// Benchmark-operation vocabulary over the graph hot paths — each row's run column returns the witness that
// proves the operation traversed the model (a timed fold that returns no witness can silently short-circuit).
[SmartEnum<string>]
public sealed partial class CorpusOp {
 public static readonly CorpusOp Bake = new("bake", RunBake);
 public static readonly CorpusOp Freeze = new("freeze", RunFreeze);
 public static readonly CorpusOp CanonicalBytes = new("canonical-bytes", RunCanonicalBytes);
 public static readonly CorpusOp Encode = new("encode", RunEncode);
 public static readonly CorpusOp DecodeGraph = new("decode-graph", RunDecodeGraph);

 [UseDelegateFromConstructor]
 public partial Fin<CorpusWitness> Run(CorpusModel model, Op key);

 static Fin<CorpusWitness> RunBake(CorpusModel model, Op key) =>
  model.Graph.ObjectNodes.TraverseM(root => model.Graph.Bake(root.Id, key)).As()
   .Bind(elements => Witness(model, Bake, elements.Count, model.Snapshot, key));

 static Fin<CorpusWitness> RunFreeze(CorpusModel model, Op key) {
  ElementGraph frozen = WorkingGraph.Thaw(model.Graph).Freeze(model.Graph.Header);
  ContentAddress artifact = ContentAddress.OfGraph(frozen);
  return artifact == model.Snapshot
   ? Witness(model, Freeze, frozen.Nodes.Count, artifact, key)
   : ElementFault.ValueRejected(key, $"<corpus-freeze-drift:{model.Grade.Key}>");
 }

 static Fin<CorpusWitness> RunCanonicalBytes(CorpusModel model, Op key) {
  ReadOnlyMemory<byte> bytes = model.Delta.ToCanonicalBytes(model.Graph.Header.Tolerance);
  return Witness(model, CanonicalBytes, bytes.Length, ContentAddress.Of(bytes.Span), key);
 }

 static Fin<CorpusWitness> RunEncode(CorpusModel model, Op key) {
  byte[] bytes = ElementWire.Encode(model.Graph).ToByteArray();
  return Witness(model, Encode, bytes.LongLength, ContentAddress.Of(bytes), key);
 }

 static Fin<CorpusWitness> RunDecodeGraph(CorpusModel model, Op key) {
  using MemoryStream payload = new(ElementWire.Encode(model.Graph).ToByteArray());
  return ElementWire.DecodeGraph(payload, WireLimits.Verified, key).Bind(graph => {
   ContentAddress artifact = ContentAddress.OfGraph(graph);
   return artifact == model.Snapshot
    ? Witness(model, DecodeGraph, graph.Nodes.Count, artifact, key)
    : ElementFault.ValueRejected(key, $"<corpus-decode-drift:{model.Grade.Key}>");
  });
 }

 static Fin<CorpusWitness> Witness(
  CorpusModel model, CorpusOp operation, long magnitude, ContentAddress artifact, Op key) =>
  magnitude > 0
   ? Fin.Succ(new CorpusWitness(model.Grade, operation, magnitude, model.Snapshot, artifact))
   : ElementFault.ValueRejected(key, $"<corpus-operation-empty:{model.Grade.Key}:{operation.Key}>");
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Tests-estate gate: Mint forges once and fingerprints the snapshot; Stable proves repeatability by double-forge.
public static class CorpusGate {
 public static Fin<CorpusModel> Mint(CorpusGrade grade, Op key) =>
  GraphForge.Mint(grade.Profile, key).Map(step =>
   new CorpusModel(grade, step.Graph, step.Delta, ContentAddress.OfGraph(step.Graph)));

 public static Fin<ContentAddress> Stable(CorpusGrade grade, Op key) =>
  Mint(grade, key).Bind(first => Mint(grade, key).Bind(second =>
   first.Snapshot == second.Snapshot
    ? Fin.Succ(first.Snapshot)
    : Fin.Fail<ContentAddress>(ElementFault.ValueRejected(key, $"<corpus-nondeterministic:{grade.Key}>"))));
}
```

## [04]-[RESEARCH]

- [CORPUS_SNAPSHOT_PINS]-[BLOCKED]: what exact `S`/`M`/`L`/`XL` `ContentAddress` values does `GraphForge.Mint` produce; execute every grade from `tests/csharp`, commit the four literal pins here, and mirror them in `libs/python` and `libs/typescript/core`. Blocked on execution, not on knowledge: no source tree exists to run, `libs/.planning/ARCHITECTURE.md` `[05]-[PLANNING_LIFECYCLE]` authoring it only once code is written, and every forge edit re-derives the four values, so a pin committed ahead of a settled forge is a value nothing produced.
