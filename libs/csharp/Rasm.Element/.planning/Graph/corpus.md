# [ELEMENT_CORPUS]

`GraphForge` owns deterministic synthetic models and the graded roster shared by benchmarks, property specifications, and cross-runtime parity. `CorpusProfile` closes occurrence count, edge density, bag width, discipline mix, composition depth, the observation and flavor witness cadences, and the seed. `GraphForge.Mint` admits every forged node and edge through `GraphDelta.AdmitOnto` over `Genesis`, exercising `LegalLink`, freeze, and incidence construction, and yields a second MUTATION delta carrying the removal and revision sections a creating delta leaves empty.

Occurrence ids derive from kernel `ContentHash` over `(seed, lane, ordinal)` and carry the Guid-v7 layout. Type ids use `NodeId.RootedType`; non-rooted ids use `NodeId.Content`. Magnitudes and index draws derive from the kernel `Deterministic` splitmix stream, never from a digest, so identity and derivation stay two rails. Each grade therefore reproduces one snapshot fingerprint on every runtime sharing the seed-zero content rail.

`GraphForge` composes the existing `ElementGraph`, `GraphDelta`, `ContentAddress`, `CanonicalWriter`, `ElementWire`, and `GraphTable` owners. Forged payloads re-enter `Classification.Of`, `PropertyValue.Of`, `MeasureValue.OfSi`, `MaterialComposition.Of*`, `MaterialPropertySet.Of*`, `AssessmentPayload.Computed`, and `AnalysisRoute.Of`; every refusal carries `ElementFault`.

## [01]-[INDEX]

- [02]-[GRAPH_FORGE]: `CorpusProfile` closes the parameter record and `GraphForge` mints deterministically — the seeded id stream, the payload kernels, and the one `AdmitOnto` realization every forged model crosses.
- [03]-[CORPUS_ROSTER]: `CorpusGrade` size rows, the `CorpusOp` hot-path vocabulary, `CorpusModel` and `CorpusWitness`, and the `CorpusGate` mint/determinism entries consumed by tests-estate benchmark and property lanes.

## [02]-[GRAPH_FORGE]

- Owner: `CorpusProfile` the closed generation-parameter record — occurrence count, `[0,1]` edge density, bag width, discipline mix, composition depth, type-reuse ratio, observation stride and sample count, flavor stride, seed — railed through `Of`; `GraphForge` the deterministic realization fold.
- Entry: `CorpusProfile.Of(nodes, density, bagWidth, disciplines, depth, seed, key, typeRatio, observationStride, observationSamples, flavorStride)` admits positive counts, a unit-interval density, a non-empty discipline mix, a depth of at least one, and positive reuse, stride, and sample columns, railing `ElementFault.ValueRejected` otherwise; `GraphForge.Mint(profile, key)` realizes the profile into `Fin<(ElementGraph Graph, GraphDelta Delta, GraphDelta Mutation)>` — the frozen snapshot a benchmark folds, the creating event body, and the change record the delta wire leg decodes.
- Auto: `Mint` builds one shared corpus header (`Header.Default` over the fixed corpus instant, so header bytes never fork a grade), one `Node.Material` per type slot whose composition cycles the four `MaterialComposition` arms by slot ordinal and whose property bag carries the WHOLE eleven-case engineering roster, one deterministic Type `Object` per slot (id through the production `NodeId.RootedType` over `ToTypeSeedBytes`), then per occurrence one seeded Guid-v7 `Object`, one property bag of `BagWidth` rows whose value case steps the fourteen-case `PropertyValue` family off the flat slot ordinal (its quotient stepping the five `TemporalValue` leaves), one quantity bag row through `MeasureValue.OfSi`, one `Computed` assessment cycling the discipline mix, and — every `ObservationStride`-th occurrence — one `ObservationSeries` opened at the corpus instant and grown by one `Encode`-minted chunk under a `From`-derived summary; one shared `Node.Appearance` and one smallest-admissible `Node.Coverage` the whole model associates to; edges land as the `Aggregate` fanout spine (depth-derived fanout), the `PropertyDefinition`/`Assessment`/`TypeDefinition`/`Observation` assigns, the material and witness-resource `Associate`s, `⌊density·nodes⌋` seeded `Connect` adjacencies, and — every `FlavorStride`-th occurrence — the `Compose.Contain`/`Nest`/`Reference` flavors, a `Void`, and a `Generic` burying a `PropertyValue.Reference` in its attribute map. `AdmitOnto(Genesis(header))` admits the assembled normal-form delta, so `LegalLink` runs per forged edge.
- Law: `Mint`'s witness run closes exactly five families and one section pair, and claims nothing wider — every `Node` case, every `Relationship` flavor, every `MaterialComposition` arm, every `MaterialPropertySet` case, and every `PropertyValue` case with every `TemporalValue` leaf cross a graded witness, and the mutation delta carries the removal and revision sections. Each cycle steps an ORDINAL whose run outsizes its family at the SMALLEST grade — four type slots against four composition arms, the whole eleven-case roster on every material, `Nodes × BagWidth` value slots against the fourteen cases times five leaves — so totality is a property of the arithmetic rather than of a grade's size. Still riding one arm: the `MaterialUsage` family beyond `None`, the optional `Connect` realizing and interface slots, and the coverage overview and slice runs.
- Receipt: the mint result carries the frozen graph, the creating normal-form delta, and the mutation delta; `ContentAddress.OfGraph` supplies the snapshot's reproducibility fingerprint and `GraphDelta.ToCanonicalBytes` the mutation's.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Map`/`TraverseM`), Thinktecture.Runtime.Extensions (generated owners), NodaTime (`Instant.FromUnixTimeTicks`/`Duration.Zero` fixed provenance, `Period`/`LocalDate`/`LocalTime` the temporal leaves), `Rasm` (`ContentHash` the id rail, `Deterministic.Stream`/`Unit`/`NextBelow` the draw rail, `Op`), `Projection/fault#ADMISSION_SLOTS` (`Gate` the normal-form slot), and System.Buffers.Binary (`BinaryPrimitives` Guid shaping).
- Growth: a new `Node` case or `Relationship` flavor is one witness row in the assembly; a new arm on any cycled family is one factory row beside its incremented arity const, which is what keeps the ordinal cycle total; a new payload family in the forge is one kernel arm beside the existing node kernels; a new generation axis is one `CorpusProfile` column threaded into the kernels, and a new random axis is one draw lane on `Deterministic` — never a sibling forge, never a parameter whose value the seed cannot replay, and never a magnitude projected off an id digest.
- Boundary: the forge composes ONLY the seam's own admissions — a raw case constructor bypassing `Classification.Of`, `MeasureValue.OfSi`, or `AssessmentPayload.Computed` forges models no production projector can produce, so every railed admission the graph demands runs inside `Mint`; the delta is constructed wholesale in normal form (ids unique by the seeded stream) and still crosses `AdmitOnto` — `ReplayOnto` trusts only seam-produced deltas and the forge counts as foreign to the structural law; determinism never rides a runtime PRNG — a `Random(seed)` stream couples the corpus to a BCL implementation — and it splits by AXIS across the two kernel owners: an ID replays through `ContentHash` over `(seed, lane, ordinal)` and a MAGNITUDE or index draw through `Deterministic`, so neither a hash-seeded sampler (which the kernel rejects by design) nor a modulo-biased projection off a digest survives here; the generation loops are the named measured-kernel statement seam, confined to the forge kernels.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
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
using static Rasm.Element.Projection.AdmissionSlots;
// LatticeAxis names the kernel lattice-axis reading of Dimension — the kernel count and the seam's physical
// 7-vector share the spelling — so the enclosing-namespace Dimension stays the bare SI signature.
using LatticeAxis = Rasm.Numerics.Dimension;

namespace Rasm.Element.Graph;

// --- [MODELS] -----------------------------------------------------------------------------
// Closed generation-parameter record: Nodes counts OCCURRENCE objects (types, materials, bags, and
// assessments derive), Density in [0,1] scales the extra Connect adjacencies, BagWidth the property rows per
// bag, Disciplines the assessment cycle, Depth the Aggregate spine depth, Seed the whole id/value stream.
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
 // Occurrences per TYPE slot — the reuse axis a grade tunes exactly like density or bag width, so the
 // deduplication pressure a Type-id regression shows up under is a declared parameter, never a body literal.
 public int TypeRatio { get; }
 // ObservationStride, ObservationSamples, and FlavorStride declare the witness cadences: every ObservationStride-th
 // occurrence carries a measured series of ObservationSamples readings, and every FlavorStride-th carries the
 // edge-flavor run AND the mutation delta's revision-and-removal pair. Each is a declared column a grade tunes exactly
 // like density, so the measured and flavor shares of an edge census are parameters a benchmark host reaches rather
 // than forge-body literals.
 public int ObservationStride { get; }
 public int ObservationSamples { get; }
 public int FlavorStride { get; }
 public long Seed { get; }

 public int TypeSlots => Math.Max(1, Nodes / TypeRatio);

 // Aggregate spine fanout implied by depth: each occurrence i > 0 composes under occurrence (i-1)/Fanout.
 public int Fanout => Math.Max(2, (int)Math.Ceiling(Math.Pow(Nodes, 1.0 / Depth)));

 // ValueSlots counts the flat property-value run a grade forges — BagWidth rows on each of Nodes occurrences.
 // Closing both closed families needs PropertyCases * TemporalArms slots, which the smallest grade clears.
 public int ValueSlots => Nodes * BagWidth;

 public static Fin<CorpusProfile> Of(
  int nodes, double density, int bagWidth, Seq<Discipline> disciplines, int depth, long seed, Op key,
  int typeRatio = 16, int observationStride = 4, int observationSamples = 16, int flavorStride = 8) =>
  nodes > 0 && density is >= 0.0 and <= 1.0 && bagWidth > 0 && !disciplines.IsEmpty && depth > 0 && typeRatio > 0
   && observationStride > 0 && observationSamples > 0 && flavorStride > 0
   ? Fin.Succ(new CorpusProfile(nodes, density, bagWidth, disciplines, depth, typeRatio, observationStride, observationSamples, flavorStride, seed))
   : ElementFault.ValueRejected(key, $"<corpus-profile-invalid:{nodes}:{density}:{bagWidth}:{depth}:{typeRatio}:{observationStride}:{observationSamples}:{flavorStride}>");
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
 // Draw lanes stay distinct from the id lanes the Seed fold owns: identity and derivation never share a stream, so a
 // lane number here never collides with one there even though both key off the same profile seed.
 const long ObservationLane = 6L;
 const long AdjacencyLane = 4L;
 const long ValueLane = 5L;
 // Each arity const counts a closed family declared at its own owner, stepped by the ordinal cycles: a new arm
 // there lands as one factory row here beside the incremented count, and a cycle running short of its family is
 // precisely the unwitnessed arm the totality claim forbids.
 const int PropertyCases = 14;
 const int TemporalArms = 5;
 const int CompositionArms = 4;
 const int PropertySetCases = 11;
 // CorpusLatticeSeed keys the witness coverage's raster off its own lane, so a model's fingerprint never depends on the profile.
 const long CorpusLatticeSeed = 0L;
 static readonly Instant CorpusInstant = Instant.FromUnixTimeTicks(CorpusUnixTicks);
 // Draft carries the non-rooted placeholder id: every node minted through Contented re-stamps from its OWN canonical
 // bytes, which exclude the id, so the draft slot holds one named placeholder rather than a Seeded(0,0,0) triple a
 // reader has to prove is never a real lane.
 static readonly NodeId Draft = Seeded(0, 0, 0);
 static readonly Duration CorpusCadence = Duration.FromMinutes(15);

 // Mint yields THREE values: the frozen snapshot, the Genesis-rooted creating delta, and the MUTATION delta — a
 // second change record over the same minted nodes carrying the removal and revision sections a creating delta leaves
 // empty, so the delta wire's own removed/revised sections and DecodeDelta's normal-form gate reach a graded witness.
 // Both deltas cross IsNormalForm before use: the seeded id stream makes normal form true by construction, and the
 // gate is what turns a new witness row's id-lane collision into a named refusal rather than a silent coalesce.
 public static Fin<(ElementGraph Graph, GraphDelta Delta, GraphDelta Mutation)> Mint(CorpusProfile profile, Op key) {
  Header header = Header.Default(CorpusInstant);
  double tol = header.Tolerance;
  int typeCount = profile.TypeSlots;
  return Classification.Of("corpus", "component", key).Bind(typeClass =>
   Classification.Of("corpus", "occurrence", key).Bind(occClass =>
    AnalysisRoute.Of("corpus.forge", key).Bind(route =>
     Assessments(profile, route, key).Bind(payloads =>
      Bags(profile, key).Bind(bags =>
       Series(profile, key).Bind(series =>
        Materials(profile, typeCount, tol, key).Bind(materials =>
         Witnesses(header, tol, key).Map(witness =>
          Assembled(profile, header, tol, typeCount, typeClass, occClass, payloads, bags, series, materials, witness))
          .Bind(built => Normalized(built.Delta, "mint", key).Bind(delta =>
           Normalized(built.Mutation, "mutation", key).Bind(mutation =>
            delta.AdmitOnto(ElementGraph.Genesis(header), key).Map(step =>
             (step.Graph, step.Delta, mutation))))))))))));
 }

 // Normalized is the shape gate every forged delta crosses before use or admission.
 static Fin<GraphDelta> Normalized(GraphDelta delta, string section, Op key) =>
  Gate(delta.IsNormalForm, key, $"<corpus-delta-not-normal-form:{section}>").ToFin().Map(_ => delta);

 // One measured series per strided occurrence, each opened at the corpus instant and grown by ONE real encoded
 // chunk — the forge crosses Open, Encode, From, and Append exactly as a live producer does, so a codec or
 // admission regression surfaces at the parity gate rather than at the first metered deployment. ONE chunk is
 // what makes the per-block summary exact: Append proves the census total against the WHOLE grown run, and a
 // fresh Open carries zero prior samples, so this block's From result IS that total. Appending a second chunk to
 // one series recomputes From over the concatenated run — a per-chunk summary appended second refuses at the
 // census gate rather than landing a series whose statistics describe its tail alone.
 static Fin<Seq<(int Index, ObservationSeries Series)>> Series(CorpusProfile profile, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).Filter(i => i % profile.ObservationStride == 0).TraverseM(i =>
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
 // Positive cadence times a strictly rising ordinal carries Encode's strict-adjacency gate: the run never repeats
 // or reverses an instant, so the gate holds by construction rather than by a sorted fixture.
 // Every eighth reading grades Suspect, so the completeness screen reads a real consumable share rather than 1.0.
 static Fin<(ObservationChunk Chunk, Seq<(Instant At, double Si, ObservationGrade Grade)> Run)> Chunked(
  CorpusProfile profile, int index, Op key) {
  Seq<(Instant At, double Si, ObservationGrade Grade)> run =
   toSeq(Enumerable.Range(0, profile.ObservationSamples)).Map(s => (
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

 // One property bag (BagWidth rows) and one quantity bag (a single [L3] volume row through the OfSi registry gate)
 // per occurrence — the counted-bag canonical layout every parity consumer re-hashes. Each property row's VALUE CASE
 // steps the closed fourteen-case family off the FLAT slot ordinal i*BagWidth + j rather than off j, so the cycle
 // closes over the whole model instead of over one bag: the smallest grade forges 256 slots against the
 // PropertyCases * TemporalArms the two cycles need, so every value case and every temporal leaf crosses at every grade.
 static Fin<Seq<(PropertyBag Props, QuantityBag Qty)>> Bags(CorpusProfile profile, Op key) =>
  toSeq(Enumerable.Range(0, profile.Nodes)).TraverseM(i =>
   toSeq(Enumerable.Range(0, profile.BagWidth)).TraverseM(j =>
     Valued(profile, (i * profile.BagWidth) + j, i, key)
      .Map(value => (PropertyName.Create($"corpus-p{j}"), value))).As()
    .Bind(rows => MeasureValue.OfSi(QuantityType.Create("Volume"), Dimension.Create(3, 0, 0, 0, 0, 0, 0), 1.0 + i * 0.5)
     .Map(volume => (
      new PropertyBag("corpus-pset", rows.Fold(Map<PropertyName, PropertyValue>(), static (m, r) => m.Add(r.Item1, r.Item2)), InheritanceMode.OccurrenceWins, PropertySource.Derived),
      new QuantityBag("corpus-qset", Map((PropertyName.Create("corpus-q0"), volume)), InheritanceMode.OccurrenceWins, PropertySource.Derived))))).As();

 // Every forged value re-enters PropertyValue.Of, so the structural admission runs over a forged composite exactly as
 // it does over a decoded one and a malformed nesting is a forge defect rather than a corpus a consumer trusts.
 static Fin<PropertyValue> Valued(CorpusProfile profile, int slot, int occurrence, Op key) =>
  Raw(profile, slot, occurrence, key).Bind(value => PropertyValue.Of(value, key));

 // Slot ordinal selects the case and the seeded draw supplies the magnitude — identity and derivation stay the
 // two rails the forge splits on. The Reference arm targets the occurrence that OWNS the bag, whose id is computable
 // from the seed stream before the occurrence is minted, so a buried reference resolves against a real node and the
 // Members sweep and Remap rewrite have a subject rather than a dangling id.
 static Fin<PropertyValue> Raw(CorpusProfile profile, int slot, int occurrence, Op key) {
  double draw = Deterministic.Unit(lanes: [ValueLane, slot], seed: profile.Seed);
  return (slot % PropertyCases) switch {
   0 => Fin.Succ((PropertyValue)new PropertyValue.Text($"corpus-text-{slot}")),
   1 => Metre(draw + 1.0).Map(static m => (PropertyValue)new PropertyValue.Measure(m)),
   2 => Fin.Succ((PropertyValue)new PropertyValue.Boolean(slot % 2 == 0)),
   // Three-valued by construction: the UNKNOWN arm is a third of the run, never a coerced false.
   3 => Fin.Succ((PropertyValue)new PropertyValue.Logical(slot % 3 == 0 ? None : Some(slot % 3 == 1))),
   4 => Fin.Succ((PropertyValue)new PropertyValue.Integer(new System.Numerics.BigInteger(slot))),
   5 => Fin.Succ((PropertyValue)new PropertyValue.Number(draw)),
   6 => Fin.Succ((PropertyValue)new PropertyValue.Binary(toSeq(BitConverter.GetBytes(slot)))),
   // TYPED enumeration members, so the canonical bytes separate two same-text members of different case.
   7 => Fin.Succ((PropertyValue)new PropertyValue.Enumerated(
    Seq<PropertyValue>(new PropertyValue.Text($"corpus-grade-{slot % 3}")),
    Seq<PropertyValue>(new PropertyValue.Text("corpus-grade-0"), new PropertyValue.Text("corpus-grade-1"), new PropertyValue.Text("corpus-grade-2")))),
   8 => Fin.Succ((PropertyValue)new PropertyValue.Reference(Seeded(profile.Seed, lane: 2, occurrence), Some("corpus-usage"))),
   // ONE QuantityType across both present bounds and a rising pair — exactly the Bounded structural law.
   9 => Metre(draw + 1.0).Bind(lower => Metre(draw + 2.0).Map(upper =>
    (PropertyValue)new PropertyValue.Bounded(Some(lower), Some(upper), None))),
   10 => Metre(draw + 1.0).Map(static m => (PropertyValue)new PropertyValue.List(
    Seq<PropertyValue>(new PropertyValue.Number(0.0), new PropertyValue.Measure(m)))),
   11 => Fin.Succ((PropertyValue)new PropertyValue.Table(
    Seq((Defining: (PropertyValue)new PropertyValue.Number(0.0), Defined: (PropertyValue)new PropertyValue.Number(draw))),
    Interpolation.Items[slot % Interpolation.Items.Count])),
   12 => Fin.Succ((PropertyValue)new PropertyValue.Complex("corpus-complex",
    Map((PropertyName.Create("corpus-inner"), (PropertyValue)new PropertyValue.Number(draw))))),
   _ => Fin.Succ((PropertyValue)new PropertyValue.Temporal(Timed(slot))),
  };
 }

 // Timed steps the five temporal leaves off the slot's own QUOTIENT, so a grade reaching the Temporal case repeatedly
 // walks the whole leaf family rather than one arm — the second cycle the flat slot ordinal makes total.
 static TemporalValue Timed(int slot) => ((slot / PropertyCases) % TemporalArms) switch {
  0 => new TemporalValue.Date(CorpusInstant.InUtc().Date.PlusDays(slot)),
  1 => new TemporalValue.Moment(CorpusInstant.InUtc().LocalDateTime.PlusHours(slot)),
  2 => new TemporalValue.Time(CorpusInstant.InUtc().TimeOfDay.PlusMinutes(slot)),
  3 => new TemporalValue.Span(Period.FromDays(slot + 1)),
  _ => new TemporalValue.Stamp(CorpusInstant + (CorpusCadence * slot)),
 };

 static Fin<MeasureValue> Metre(double si) =>
  MeasureValue.OfSi(QuantityType.Create("Length"), Dimension.Create(1, 0, 0, 0, 0, 0, 0), si);

 // One Material node per TYPE slot, its composition cycling the four MaterialComposition arms by slot ordinal and its
 // property bag carrying the WHOLE eleven-case engineering roster. The roster is per-material rather than cycled
 // across materials because a grade's type-slot count is smaller than the case count at the small end, so cycling
 // there would leave arms no grade reaches; carrying all eleven also puts the two Discipline.Structural cases in one
 // bag, which is exactly the same-discipline tie the Node.Material canonical sort breaks on full property bytes.
 static Fin<Seq<Node>> Materials(CorpusProfile profile, int typeCount, double tol, Op key) =>
  toSeq(Enumerable.Range(0, typeCount)).TraverseM(t => {
   MaterialId material = MaterialId.Of($"corpus-material-{t}");
   return Composed(material, t, key).Bind(composition =>
    Properties(t, key).Map(properties =>
     Contented(new Node.Material(Seeded(profile.Seed, lane: 0, t), material, composition, properties), tol)));
  }).As();

 // Composed cycles the four composition arms by type-slot ordinal; the smallest grade's four slots already forge each arm.
 static Fin<MaterialComposition> Composed(MaterialId material, int slot, Op key) => (slot % CompositionArms) switch {
  0 => Fin.Succ(MaterialComposition.OfSingle(material)),
  1 => Metre(0.1 + slot * 0.01).Bind(thickness => MaterialComposition.OfLayerSet(
   Seq(new MaterialLayer(material, thickness, $"corpus-layer-{slot}", Some(slot % 101), "corpus", Some(false))), key)),
  2 => MaterialComposition.OfProfileSet(
   Seq(new MaterialProfile(material, ProfileRef.Of("corpus", $"CP{slot}"), Some(slot % 101), "corpus", Seq<MeasureValue>())), key),
  // Two constituents normalizing to unity, the fraction-sum gate's own admissible shape.
  _ => MaterialComposition.OfConstituentSet(
   Seq(new MaterialConstituent(material, "corpus", 0.5, "corpus-a"), new MaterialConstituent(material, "corpus", 0.5, "corpus-b")), key),
 };

 // Magnitudes scale off the slot ordinal rather than a draw lane, because every column here is a physical
 // constant the owner's accumulating admission bounds — a seeded magnitude risks tripping a positivity or
 // conservation slot the forge exists to satisfy.
 static Fin<Seq<MaterialPropertySet>> Properties(int slot, Op key) =>
  toSeq(Enumerable.Range(0, PropertySetCases)).TraverseM(c => Property(c, slot, key)).As();

 static Fin<MaterialPropertySet> Property(int ordinal, int slot, Op key) {
  PropertyEvidence evidence = new($"corpus-{ordinal}", $"corpus-ref-{slot}", None);
  double scale = 1.0 + (slot % 8) * 0.125;
  return ordinal switch {
   0 => MaterialPropertySet.OfMechanical(2400.0 * scale, 30_000.0 * scale, 400.0 * scale, 550.0 * scale, 0.2, 1.0e-5, key, evidence),
   1 => MaterialPropertySet.OfOrthotropic(500.0 * scale, 11_000.0 * scale, 370.0 * scale, 690.0 * scale, 24.0 * scale, 2.5 * scale, 5.0e-6, key, evidence),
   2 => MaterialPropertySet.OfThermal(1.7 * scale, 880.0 * scale, 0.25 * scale, 120.0, key, evidence),
   3 => Acoustic.Of(Bands(static band => 0.05 + band * 0.05), Bands(band => 20.0 + band + scale), key)
    .Map(spectrum => MaterialPropertySet.OfAcoustic(spectrum, evidence)),
   4 => FireResistance.Of(Some(60), Some(90), Some(30), key)
    .Map(resistance => MaterialPropertySet.OfFire(FireRating.A1, resistance, evidence)),
   5 => MaterialPropertySet.OfEnvironmental(
    MeasurementBasis.PerM3, [.. Enumerable.Repeat(scale, ImpactCategory.Count * LifecycleStage.Count)], 0.3, 0.6, key, evidence),
   6 => Currency.Parse("EUR", key).Bind(currency =>
    MaterialPropertySet.OfCost(MeasurementBasis.PerM2, currency, 12.0 * scale, 8.0 * scale, 25.0 * scale, key, evidence)),
   7 => MaterialPropertySet.OfDamping(0.02 * scale, Some((0.5, 0.001)), key, evidence),
   // Free-water saturation above the 80%RH content, the isotherm refinement's own ordering.
   8 => MaterialPropertySet.OfHygrothermal(0.18, 45.0 * scale, 180.0 * scale, Some(0.02), key, evidence),
   9 => MaterialPropertySet.OfDurability(3.5 * scale, 1.0e-12, 0.3, key, evidence),
   // Per-band per-side transmittance plus reflectance under unity, the optical conservation refinement's own shape.
   _ => MaterialPropertySet.OfOptical(0.6, 0.2, 0.2, 0.5, 0.25, 0.25, 0.0, 0.84, 0.84, key, evidence),
  };
 }

 // Bands shapes the vectors the acoustic arity gate takes off the band index, so absorption stays inside its unit
 // range and the reduction spectrum stays finite; the length derives from the band roster, never a literal.
 static double[] Bands(Func<int, double> shape) => [.. Enumerable.Range(0, AcousticBand.Count).Select(shape)];

 // Witnesses mints the two node cases no occurrence payload reaches — Appearance and Coverage — ONCE per model,
 // shared by every occurrence's Associate edge, which is exactly the dedup shape a real projector produces. Without
 // them the sixth and seventh Node arms, their canonical-bytes ordinals, their wire oneof arms, and the Associate
 // legality closure over the Material/Appearance/Coverage triple all ride untested behind arms no parity vector
 // reaches. Coverage stays the smallest admissible one — one lattice cell, one Gray band, the identity georeference —
 // so it proves the arms without inflating a grade's fingerprint surface.
 static Fin<(Node Appearance, Node Coverage)> Witnesses(Header header, double tol, Op key) =>
  AppearanceSummary.Of(0.5, 0.5, 0.5, 0.0, 0.5, 1.0, transmissive: false, key).Bind(summary =>
   CellLattice.Of([1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0],
     LatticeAxis.Create(1), LatticeAxis.Create(1), LatticeAxis.Create(1), ceiling: 1L, key).Bind(lattice =>
    CoverageGrid.Of(
      CoverageKind.Field, Seed(CorpusLatticeSeed, lane: 7, 0), lattice,
      Seq(new CoverageBand(0, "corpus-band", ChannelDtype.Float32, BandRole.Gray)),
      header.Reference, key)
     .Map(grid => (
      Contented(new Node.Appearance(Draft, summary), tol),
      Contented(new Node.Coverage(Draft, grid), tol)))));

 // Wholesale normal-form assembly: ids are unique by the seeded stream, so both deltas are IsNormalForm by
 // construction, the Normalized gate proves it, and AdmitOnto supplies the structural proof for the creating one.
 static (GraphDelta Delta, GraphDelta Mutation) Assembled(
  CorpusProfile profile, Header header, double tol, int typeCount, Classification typeClass, Classification occClass,
  Seq<AssessmentPayload> payloads, Seq<(PropertyBag Props, QuantityBag Qty)> bags,
  Seq<(int Index, ObservationSeries Series)> series, Seq<Node> materials, (Node Appearance, Node Coverage) witness) {
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
  Seq<Node> propertySets = bags.Map(pair => Contented(new Node.PropertySet(Draft, pair.Props), tol));
  Seq<Node> quantitySets = bags.Map(pair => Contented(new Node.QuantitySet(Draft, pair.Qty), tol));
  Seq<Node> assessments = payloads.Map(payload => Contented(new Node.Assessment(Draft, payload), tol));
  // Observation nodes content-key off the STREAM identity exactly as a live one does, so a re-mint of the same
  // profile addresses the same node and the strided share stays a stable fraction of every grade.
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
     new Relationship.Associate(occ, materials[i % typeCount].Id, new MaterialUsage.None()));
  });
  // Observation assigns ride the OCCURRENCE alone, which is exactly the legality arm LegalAssign enforces — a type
  // slot never receives one, so the forge exercises the refusal boundary by construction rather than by a fixture.
  Seq<Relationship> measured = observations.Map(row =>
   (Relationship)new Relationship.Assign(occurrences[row.Index].Id, row.Node.Id, AssignKind.Observation));
  // Resource associations reach the two shared witness nodes — one Associate per occurrence to each, so the
  // Appearance and Coverage arms of the Associate legality closure and their node ordinals cross every grade.
  Seq<Relationship> resources = occurrences.Bind(occ => Seq<Relationship>(
   new Relationship.Associate(occ.Id, witness.Appearance.Id, new MaterialUsage.None()),
   new Relationship.Associate(occ.Id, witness.Coverage.Id, new MaterialUsage.None())));
  // Every remaining edge FLAVOR crosses on the strided occurrences, so no case, sub-kind, or buried-reference sweep
  // rides an arm no witness reaches: the three non-Aggregate Compose flavors (Contain the owning spatial edge Bake
  // descends, Nest the ordered child run, Reference the NON-owning association BakeParts must skip), Void, and a
  // Generic passthrough whose attribute map BURIES a PropertyValue.Reference — the one shape that proves Members
  // sweeps and Remap rewrites a buried id in lockstep, which no typed case can exercise.
  Seq<int> strided = toSeq(Enumerable.Range(1, Math.Max(0, profile.Nodes - 1))).Filter(i => i % profile.FlavorStride == 0);
  Seq<Relationship> flavors = strided
   .Bind(i => Seq<Relationship>(
    new Relationship.Compose(occurrences[i - 1].Id, occurrences[i].Id, ComposeKind.Contain, None),
    new Relationship.Compose(occurrences[i - 1].Id, occurrences[i].Id, ComposeKind.Nest, Some(i)),
    new Relationship.Compose(occurrences[i - 1].Id, occurrences[i].Id, ComposeKind.Reference, None),
    new Relationship.Void(occurrences[i - 1].Id, occurrences[i].Id, VoidKind.Opening),
    new Relationship.Generic(
     "corpus.generic", occurrences[i - 1].Id, occurrences[i].Id,
     Map((PropertyName.Create("corpus-buried-ref"), (PropertyValue)new PropertyValue.Reference(occurrences[i].Id))),
     Seq(new RelationshipParticipant(occurrences[i].Id, "corpus-participant", None)))));
  Seq<Relationship> adjacencies = toSeq(Enumerable.Range(0, (int)(profile.Density * profile.Nodes)))
   .Choose(c => {
    ulong state = Deterministic.Stream(lanes: [AdjacencyLane, c], seed: profile.Seed);
    int from = Deterministic.NextBelow(state: ref state, exclusiveCeiling: profile.Nodes);
    int to = Deterministic.NextBelow(state: ref state, exclusiveCeiling: profile.Nodes);
    return from == to ? None : Some((Relationship)new Relationship.Connect(occurrences[from].Id, occurrences[to].Id, ConnectKind.Element, None, None));
   })
   .Distinct();
  // Mutation witness rides the SAME strided occurrences the flavor run does: each one's computed assessment node
  // is removed together with the Assign edge that reached it — a removal and its cascade in one record — while the
  // occurrence itself is REVISED in place. Revision targets a rooted Object precisely because a Guid-v7 occurrence id
  // is placement identity independent of content, so an edited payload keeps its id; revising a content-keyed node
  // would be a remove-plus-add wearing a revision's name. The header stays absent here, so the creating delta and the
  // mutation cross the header-present and header-absent decode arms between them.
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
   edges + adjacencies + measured + resources + flavors, Seq<Relationship>(), Some(header)), mutation);
 }
}
```

## [03]-[CORPUS_ROSTER]

- Owner: `CorpusGrade` the `[SmartEnum<string>]` size roster; `CorpusOp` the `[SmartEnum<string>]` hot-path vocabulary with a witness-returning run column; `CorpusModel` the minted carrier with its snapshot fingerprint; `CorpusWitness` the operation evidence; `CorpusGate` the mint/determinism capability.
- Cases: `CorpusGrade` rows `S`/`M`/`L`/`XL` (64 → 262 144 occurrences, density, bag width, depth, and discipline mix widening together); `CorpusOp` rows `Bake` (every object root through the memoized fold), `Freeze` (thaw-and-freeze rebuild of the frozen structures), `CanonicalBytes` (the delta content-key projection), `Encode` (the full snapshot wire lowering), `DecodeGraph` (the snapshot round-trip under `WireLimits.Verified`), `DecodeDelta` (the mutation round-trip through the normal-form gate under `WireLimits.Default`), `Tabulate` (the columnar egress fold through its declaration-versus-projection gate); the closed hot-path family.
- Entry: `CorpusGate.Mint(grade, key)` forges once and stamps its snapshot fingerprint. `Stable(grade, key)` forges twice and rejects address drift. Each `CorpusOp.Run(model, key)` returns a `CorpusWitness` with grade, operation, magnitude, snapshot, and operation artifact address; freeze and both decode rows reject content drift against the input they round-tripped.
- Receipt: `CorpusWitness` proves traversal and binds each operation result to its input fingerprint. Tests-estate timing maps it into AppHost `BenchmarkReceipt`: `Suite = "Rasm.Element"`, `Case = $"{grade}/{op}"`, `Corpus = Some(witness.Snapshot.Value)`, and `Operations = witness.Magnitude`; AppHost owns host evidence, timing, allocation, verdict, artifact key, and correlation.
- Packages: Google.Protobuf (`MessageExtensions.WriteTo(IBufferWriter<byte>)` and `WriteTo(Stream)` — the wire legs stage through one buffer, so no hot-path row allocates a whole-snapshot array), System.Buffers (`ArrayBufferWriter<byte>` the encode sink whose `WrittenSpan` the digest reads in place). BenchmarkDotNet and CsCheck consume this roster FROM THE TESTS ESTATE — the benchmark lane iterates `CorpusOp.Items` per grade and the property lane drives `Stable` and the wire round-trip over seeds; both are central-manifest packages with no `Rasm.Element` reference, so the seam page names the consumption shape and no benchmark member.
- Growth: a new scale is one `CorpusGrade` row; a new hot path is one `CorpusOp` row and witness arm. Benchmarks reuse one minted model per grade.
- Boundary: Element owns models, operation vocabulary, and semantic witnesses. AppHost owns benchmark receipts and regression verdicts. `XL` belongs to benchmark hosts, never unit defaults. [SPIKE]: the four graded `S`/`M`/`L`/`XL` snapshot addresses converge only on the first sanctioned execution of the settled forge — the `tests/contracts/MANIFEST.md` `[02.25]` blocker carries the arming, the DESIGN-PIN law rejects any transcribed stand-in ahead of it, and the pins land here and mirror to the `libs/python` `_CORPUS` `element-corpus` row and the `libs/typescript/core` bit-parity gate in one change; the deterministic floor is `CorpusGate.Stable`'s double-forge repeatability and the per-op drift gates, total without them — an unset placeholder cannot masquerade as a parity gate.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Minted carrier reused per operation, with the input fingerprint computed once. Mutation is the change record the
// creating Delta cannot be — it carries the removed and revised sections a Genesis-rooted delta leaves empty.
public sealed record CorpusModel(
 CorpusGrade Grade, ElementGraph Graph, GraphDelta Delta, GraphDelta Mutation, ContentAddress Snapshot);

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
 public static readonly CorpusOp DecodeDelta = new("decode-delta", RunDecodeDelta);
 public static readonly CorpusOp Tabulate = new("tabulate", RunTabulate);

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

 // Wire legs write through the buffer-writer entry the wire page's own boundary law names, so a whole-snapshot
 // ToByteArray copy never lands: the encode row digests the written span in place, and the decode rows write into one
 // stream buffer and rewind rather than allocating a second array to hand the parser.
 static Fin<CorpusWitness> RunEncode(CorpusModel model, Op key) {
  ArrayBufferWriter<byte> sink = new();
  ElementWire.Encode(model.Graph).WriteTo(sink);
  return Witness(model, Encode, sink.WrittenCount, ContentAddress.Of(sink.WrittenSpan), key);
 }

 static Fin<CorpusWitness> RunDecodeGraph(CorpusModel model, Op key) {
  using MemoryStream payload = new();
  ElementWire.Encode(model.Graph).WriteTo(payload);
  payload.Position = 0;
  return ElementWire.DecodeGraph(payload, WireLimits.Verified, key).Bind(graph => {
   ContentAddress artifact = ContentAddress.OfGraph(graph);
   return artifact == model.Snapshot
    ? Witness(model, DecodeGraph, graph.Nodes.Count, artifact, key)
    : ElementFault.ValueRejected(key, $"<corpus-decode-drift:{model.Grade.Key}>");
  });
 }

 // RunDecodeDelta crosses the mutation whole and re-enters through DecodeDelta's normal-form gate, so the removed
 // and revised sections, the ABSENT-header arm, and the shape gate itself are all timed and witnessed. The artifact
 // is the decoded delta's own content key, which the mutation's key must reproduce.
 static Fin<CorpusWitness> RunDecodeDelta(CorpusModel model, Op key) {
  using MemoryStream payload = new();
  ElementWire.Encode(model.Mutation).WriteTo(payload);
  payload.Position = 0;
  double tolerance = model.Graph.Header.Tolerance;
  return ElementWire.DecodeDelta(payload, WireLimits.Default, key).Bind(delta => {
   ContentAddress artifact = ContentAddress.Of(delta.ToCanonicalBytes(tolerance).Span);
   return artifact == ContentAddress.Of(model.Mutation.ToCanonicalBytes(tolerance).Span)
    ? Witness(model, DecodeDelta, delta.NodeCount + delta.EdgeCount, artifact, key)
    : ElementFault.ValueRejected(key, $"<corpus-delta-drift:{model.Grade.Key}>");
  });
 }

 // RunTabulate folds the columnar egress: every row family the snapshot carries crosses its declaration-versus-projection
 // gate, so a column roster that drifted from its payload arm fails here rather than at a landing that infers nothing.
 static Fin<CorpusWitness> RunTabulate(CorpusModel model, Op key) =>
  GraphTable.Tabulate(model.Graph, key).Bind(snapshot =>
   snapshot.Batches(key).Bind(_ => Witness(model, Tabulate, snapshot.Rows.Count, snapshot.Address, key)));

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
   new CorpusModel(grade, step.Graph, step.Delta, step.Mutation, ContentAddress.OfGraph(step.Graph)));

 public static Fin<ContentAddress> Stable(CorpusGrade grade, Op key) =>
  Mint(grade, key).Bind(first => Mint(grade, key).Bind(second =>
   first.Snapshot == second.Snapshot
    ? Fin.Succ(first.Snapshot)
    : Fin.Fail<ContentAddress>(ElementFault.ValueRejected(key, $"<corpus-nondeterministic:{grade.Key}>"))));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
