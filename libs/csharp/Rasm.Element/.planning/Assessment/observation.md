# [ELEMENT_OBSERVATION]

`ObservationSeries` owns measured evidence — the `Graph/element#NODE_MODEL` `Node.Observation` case wraps it beside the computed `Assessment/assessment#ASSESSMENT_NODE` receipt as the model's SECOND evidence modality, what the built asset REPORTS against what an analysis PREDICTED. One series binds one deployed sensor to one observed aspect of one element: the `SensorId` deployment identity, the observed `Properties/property#PROPERTY_BAG` `PropertyName` aspect (a wall reports surface temperature AND heat flux, so the `Properties/quantity#MEASURE_VALUE` `QuantityType` alone under-discriminates), the quantity's `QuantityType`/`Dimension`/canonical-unit triple every decoded sample re-mints a `MeasureValue` through, the `SamplingKind` temporal-aggregation semantics, the nominal `Duration` cadence, the `Interval` observation extent, and an interval-ordered run of content-keyed `ObservationChunk` rows addressing the sample bytes BY REFERENCE in the same seed-zero `XxHash128` object store the geometry blobs and the `Assessment/assessment#ASSESSMENT_NODE` `ResultBlob` use — never an inlined sample buffer on the node. Each series attaches to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Observation`), so a commissioning comparison — a declared U-value against a metered heat flux, a predicted annual energy against a metered total — is a `Graph/element#ELEMENT_GRAPH` `Bake` read over `element.Observations` beside `element.Assessments`, never a join against an external historian.

`SamplingKind` carries the load-bearing axis: an instantaneous temperature, an interval-averaged mean radiant temperature, an interval-total delivered energy, and a cumulative meter register are FOUR algebras over one row shape, and folding them alike double-counts a total or averages an odometer. Each row answers three columns — `Interval` (this sample summarizes the preceding cadence window or one instant), `Monotone` (this run admits only a non-decreasing sequence), and the duration-weighted `Combine` merge that makes downsampling, cross-chunk rollup, and the summary merge ONE body per row. `SeriesStatistics` derives off that algebra so a completeness screen, a range bound, and the representative figure read flat off the node, while the chunk bytes stay the truth the summary rebuilds from.

## [01]-[INDEX]

- [02]-[OBSERVATION_SERIES]: `ObservationSeries` carries the node payload beside `SensorId` deployment identity, the `SamplingKind` vocabulary with its `Interval`/`Monotone`/`Combine` columns and the `Downsample` fold they drive, `ObservationGrade` per-sample quality, `ObservationChunk` by-reference sample runs, `SensorProvenance` instrument audit, `Open`/`Append`/`Rehydrate` admission, `ChunkAt`/`ChunksIn`/`Expected` fetch reads, the `Value` sample lift, and `CanonicalBytes` stream identity.
- [03]-[SERIES_STATISTICS]: `SeriesStatistics` derives the summary — the `From` run derivation owning the register-span rule, the graded `Census`, `Completeness`/`Consumable` screens, the column-driven `Fold` merge over adjacent summaries, and the `Representative` figure a commissioning route subtracts against a computed assessment result.

## [02]-[OBSERVATION_SERIES]

- Owner: `ObservationSeries` the sensor-bound measured-evidence descriptor the `Node.Observation` case wraps; `SensorId` the `[ValueObject<string>]` DEPLOYMENT identity (a re-mounted instrument takes a new id, so one series never spans two mounting positions); `SamplingKind` the `[SmartEnum<string>]` temporal-aggregation vocabulary carrying the `Interval` window-versus-instant column, the `Monotone` register column, and the duration-weighted `Combine` merge column its own `Downsample` fold drives; `ObservationGrade` the `[SmartEnum<string>]` per-sample quality vocabulary with its `Consumable` column (the grade the chunk bytes encode per sample and the `[03]` census counts by); `ObservationChunk` the interval-anchored by-reference sample block (window + content key + sample count) owning the one blob codec every runtime reads; `SensorProvenance` the instrument audit (make/model/serial, calibration date, and the zero-centred `Properties/quantity#MEASURE_VALUE` `MeasureBand` tolerance every lifted sample shifts onto its own magnitude).
- Cases: `SamplingKind` closes at six rows spanning the metering space a BMS, meter, or structural-health stream reports — `Instantaneous` (a point reading; a resample PICKS, never sums), `Averaged` (the cadence-window mean; merges duration-weighted), `Total` (the cadence-window sum; merges additively), `Cumulative` (a monotone register whose value is an odometer, not a rate; merges last-wins and refuses a decreasing run), `Minimum` and `Maximum` (the cadence-window extremes; merge by extremum) — one case per algebra, never a `kind` string a consumer re-interprets.
- Entry: `ObservationSeries.Open(sensor, aspect, observed, signature, unit, sampling, cadence, start, provenance, key)` opens an empty series at its deployment instant, `Fin<T>` railing `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a blank canonical unit or a non-positive cadence; `Append(chunk, statistics, key)` is the ONE growth transition — the chunk's window opens at or after the current `Window.End`, closes after it opens, and carries at least one sample, and the recomputed `[03]` summary rides with it under a census total matching the grown run, so `Window.End` advances while the node id holds; `Rehydrate(...)` is the CROSS-ASSEMBLY decoder gate re-validating the whole stored run against the same invariants `Append` maintains, the single-sample degenerate-window exemption included (bounded window, strictly advancing non-overlapping chunks each at least one sample wide, a window spanning AND bracketing the run, a coherent census equal to the run it summarizes) so a tampered store cannot mint an overlapping, unbounded, self-contradicting, or emptily-summarized series — the `Assessment/assessment#ASSESSMENT_NODE` `Rehydrate` distrust posture; `ChunkAt(instant)` and `ChunksIn(window, key)` are the metadata-selected fetch reads (the `Geospatial/coverage#COVERAGE_NODE` `SliceAt`/`SliceWindow` shape — a consumer resolves WHICH blobs answer a question before a byte moves), the windowed read railing an unbounded query rather than selecting nothing; `Expected(window)` derives the sample count a regular cadence owes over a window (`None` for an event-driven series or an unbounded window, both denominators unanswerable by construction); `Value(si, key)` lifts one decoded SI scalar into a typed `MeasureValue` under the series' own quantity triple with the instrument tolerance shifted onto that magnitude; `SamplingKind.Downsample(samples)` folds a decoded run of `(Si, Span)` pairs into one value under the row's own algebra; `ObservationChunk.Encode(samples, key)` mints the block and its bytes together off one projection, railing a sampleless run and any adjacent pair that fails to strictly advance, and the instance `Decode(blob, key)` inverts it against its own `SeriesKey`, railing a key mismatch, an unrepresentable declared count, a truncated run, and an unminted grade token.
- Auto: the series identity is the STREAM, never its contents — `CanonicalBytes` writes the sensor, the aspect, the observed `QuantityType`, the seven `Dimension` exponents, the canonical unit, the sampling key, the presence-delimited cadence, and `Window.Start` ALONE, so an append that extends `Window.End` and adds a chunk mutates the SAME content-keyed node in place exactly as an `Assessment/assessment#ASSESSMENT_NODE` `Advance` flip does, while a re-deployment opening at a fresh instant mints a fresh node; the chunk run, the advancing `Window.End`, the derived `[03]` statistics, and the `SensorProvenance` audit are EXCLUDED from that projection (the `Provenance`-exclusion discipline the assessment payload holds), so a re-calibration and a recomputed summary are both content-key-inert; `Open` seeds the degenerate `Interval(start, start)` and every later `Window.End` derives from the last appended chunk, so the extent is never hand-set; `Append` and `Rehydrate` both prove the run strictly advancing through the `Zip`-adjacent law the coverage pyramid and timeline hold, so `ChunkAt` resolves at most one block and `ChunksIn`'s half-open overlap clip is total over stored order once its query window admits; `Expected` divides the window duration by the cadence under the `SamplingKind.Interval` column — an interval row owes one sample per closed window while an instant row owes one per boundary, the off-by-one a flat division swallows; the blob layout is FIXED at `Encode` and never inferred — a count prefix then one `(I64 Unix-tick instant, IEEE-754 `Double` magnitude, `ObservationGrade` key)` triple per sample in stored order, written through the same `CanonicalWriter` canon the node identity uses, so the bytes and the `SeriesKey` that addresses them derive from one projection and a peer runtime decodes byte-identically off the one seed; instants stay ABSOLUTE because `Cadence` is `Option` and a delta encoding strands exactly the event-driven streams whose completeness is already unanswerable, and the writer opens at zero tolerance so a model's geometric quantization never rounds a reading; `Value` re-mints through `MeasureValue.OfAdmitted(Type, Dimension, si, CanonicalUnit)` and shifts the zero-centred instrument band onto the sample through `MeasureBand.Admit` before `WithUncertainty`, so the band contains its own nominal by construction and the `Properties/quantity#MEASURE_VALUE` propagation algebra carries instrument error through every downstream fold with no call-site arithmetic.
- Receipt: the `ObservationSeries` is the measured evidence a `Bake`-derived `Element` carries flat in `element.Observations` — `element.Observations.Filter(series => series.Observed == QuantityType.Create("HeatTransferCoefficient"))` reads every metered transmittance stream, `series.Statistics.Completeness(series.Expected(window))` screens a gappy stream before it decides anything, `series.Statistics.Representative(series.Sampling, key)` reads the ONE comparable figure the sampling row designates, and `series.ChunksIn(window, key)` resolves the blob set a windowed fetch reads by content key; the commissioning comparison is then one subtraction in `Rasm.Compute` between that figure and the matching `element.Assessments` `ResultMeasure(name)` — the seam delivering both evidence kinds off one baked element, the verdict staying with the discipline that owns it.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` the `Combine` merge column, `[ValueObject<string>]`, `[ValidationError<ElementFault>]`), Generator.Equals (`[Equatable]` the payload's member-level diff + `[OrderedEquality]` the interval-ordered chunk run, so the `Graph/element#NODE_MODEL` drill descends to `Nodes[id].Series.Chunks[i]` and `Nodes[id].Series.Statistics`), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin` + the `Validation<Error,_>` carrier), `Projection/fault#ADMISSION_SLOTS` (the `Gate` slot and `Accumulate` fold every series transition and the chunk codec admit through), NodaTime (`Instant` the sample anchor, `Duration` the cadence and span, `Interval` the observation extent, `LocalDate` the calibration stamp), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter` the `CanonicalBytes` projection and the chunk blob both write through), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-key entry the chunk `SeriesKey` shares with `Projection/address#CONTENT_ADDRESS`), BCL inbox (`BinaryPrimitives`/`BitConverter`/`Encoding.UTF8` the decode cursor inverts the writer's primitives through).
- Growth: a new metering algebra is one `SamplingKind` row answering all three columns — every fetch, fold, downsample, and summary absorbs it with zero edits; a new sample quality class is one `ObservationGrade` row carrying its `Consumable` column; a new observed aspect is one `PropertyName` a projector stamps (the seam mints no aspect roster, the SAME neutrality the `AnalysisRoute` token holds for the analysis-route roster); a new instrument audit column is one `SensorProvenance` field; a new sample column is one write in `Encode` beside its read in `Decode`, the two halves of one layout moving together; never a per-quantity series type, never a per-vendor payload, never a sample buffer on the node.
- Boundary: `ObservationSeries` holds the samples BY REFERENCE — each `ObservationChunk` addresses its block in the same seed-zero `XxHash128` object store the geometry and assessment blobs use, so an inlined sample array, a vendor stream handle, or a second hasher on the seam is the named defect; the seam owns the blob LAYOUT while the store owns the bytes, so a residence, an exporter, and a peer runtime each read one declared codec rather than negotiating a per-producer sample format, and `Decode` re-proves the content key before a sample crosses because a persisted blob is untrusted exactly as a persisted series is; the series is OCCURRENCE-ONLY — a `Component` is a catalogue entry and is never instrumented, so `Graph/element#ELEMENT_GRAPH` `Bake` gathers observations from the occurrence's own incidence alone and the named type→occurrence inheritance does NOT carry them, a type-inherited series claiming every realization of one `Component` reports one sensor's data being the deleted form; `SensorId` is DEPLOYMENT-scoped and `Window.Start` folds into the identity, so a re-mounted instrument opens a fresh series rather than splicing two positions into one record; NodaTime `Interval.Start`/`Interval.End` THROW unless `HasStart`/`HasEnd` holds, so every admitted window is bounded at BOTH ends and `Open`/`Append`/`Rehydrate` construct through the two-`Instant` constructor exclusively — a nullable-endpoint `Interval` reaching an interior read is the deleted form, and a CALLER's query window crosses the same bar at the read that takes it, `ChunksIn` railing it and `Expected` answering the absent denominator it is; `SamplingKind` discriminates the algebra, so a fold that sums a `Cumulative` register, averages a `Total`, or interpolates an `Instantaneous` across a gap is the defect the `Combine` column deletes, and an interval row is never linearly interpolated across a gap because the window it summarizes did not happen; the per-sample `ObservationGrade` lives in the chunk BYTES and only its census crosses onto the node, so a per-sample flag column on the seam is the deleted form; `SensorProvenance.Tolerance` is the instrument band centred on ZERO (the manufacturer's ±δ), shifted onto each sample by `Value` — an absolute band stored against one nominal magnitude fails `WithUncertainty`'s containment gate for every other sample, the deleted form; the observation attaches through an `Assign` edge, never an inlined back-reference on the `Object` node, and the seam owns the evidence while every verdict — a commissioning pass, a degradation trend, a fault-detection rule — lives in `Rasm.Compute` and writes back as an `Assessment/assessment#ASSESSMENT_NODE` receipt whose `DependsOn` records the series it consumed.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Text;
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Element.Projection.AdmissionSlots;

namespace Rasm.Element.Assessment;

// --- [TYPES] ------------------------------------------------------------------------------
// SensorId keys one DEPLOYMENT, never a catalogue serial (that rides SensorProvenance.Serial): a device moved to a
// second mounting position takes a NEW id, because this key plus Window.Start separate two streams of one physical
// instrument, and a shared id across positions splices two unrelated measurement records into one.
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ElementFault>]
public sealed partial class SensorId {
 private static partial void ValidateFactoryArguments(ref ElementFault? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = ElementFault.ValueRejected(Op.Of(name: nameof(SensorId)), "sensor id requires a non-blank token"); return; }
  value = value.Trim();
 }

 // Of re-keys the seam-rail admission to the CALLER's Op (the AnalysisRoute.Of re-stamp discipline).
 public static Fin<SensorId> Of(string token, Op key) =>
  Validate(token, null, out SensorId? sensor) is { } fault
   ? ElementFault.ValueRejected(key, fault.Message)
   : sensor is { } admitted
    ? Fin.Succ(admitted)
    : ElementFault.ValueRejected(key, "sensor id admission returned no value");
}

// SamplingKind fixes the TEMPORAL-AGGREGATION algebra a series reports under — how two adjacent windows merge, whether
// a run may decrease, and which summary column answers as the comparable figure. Three columns carry it:
// Interval — this sample summarizes the PRECEDING cadence window (Averaged/Total/Minimum/Maximum) versus one instant
//   (Instantaneous/Cumulative); Expected reads it for the closed-window-versus-boundary count, and an interval row
//   never interpolates across a gap because the window it summarizes did not happen.
// Monotone — this run admits only a non-decreasing sequence (Cumulative alone): a register stepping backwards is a
//   rollover or a replacement, never negative consumption, so the [03] From derivation — the one owner reading both
//   the run and the row that governs it — refuses the run rather than minting a negative register span from it.
// Combine — DURATION-WEIGHTED merge of two adjacent windows (left earlier): an average weights by span, a total adds,
//   a register and an instant keep the later reading, an extreme takes the extremum. Downsample folds a decoded run
//   through it and [03] Fold reuses the Averaged and Total rows for the mean and total columns, so weighted-mean and
//   additive arithmetic stay owned ONCE here rather than re-spelled per consumer. Weights stay positive by admission.
[SmartEnum<string>]
public sealed partial class SamplingKind {
 public static readonly SamplingKind Instantaneous = new("instantaneous", interval: false, monotone: false, static (left, leftSeconds, right, rightSeconds) => right);
 public static readonly SamplingKind Averaged = new("averaged", interval: true, monotone: false, static (left, leftSeconds, right, rightSeconds) => leftSeconds + rightSeconds > 0d ? ((left * leftSeconds) + (right * rightSeconds)) / (leftSeconds + rightSeconds) : right);
 public static readonly SamplingKind Total = new("total", interval: true, monotone: false, static (left, leftSeconds, right, rightSeconds) => left + right);
 public static readonly SamplingKind Cumulative = new("cumulative", interval: false, monotone: true, static (left, leftSeconds, right, rightSeconds) => right);
 public static readonly SamplingKind Minimum = new("minimum", interval: true, monotone: false, static (left, leftSeconds, right, rightSeconds) => Math.Min(left, right));
 public static readonly SamplingKind Maximum = new("maximum", interval: true, monotone: false, static (left, leftSeconds, right, rightSeconds) => Math.Max(left, right));

 public bool Interval { get; }
 public bool Monotone { get; }

 [UseDelegateFromConstructor] public partial double Combine(double left, double leftSeconds, double right, double rightSeconds);

 // Downsample folds a time-ordered run of (SI value, covered span) pairs into the single value this row's algebra
 // designates, spans accumulating so a weighted mean stays correct across unequal windows — the ONE downsample every
 // consumer of decoded chunk bytes composes. None over an empty run; a fabricated zero reads as a real measurement.
 public Option<double> Downsample(Seq<(double Si, Duration Span)> samples) =>
  samples.Head.Map(head => samples.Tail
   .Fold((Si: head.Si, Seconds: head.Span.TotalSeconds), (state, next) =>
    (Combine(state.Si, state.Seconds, next.Si, next.Span.TotalSeconds), state.Seconds + next.Span.TotalSeconds))
   .Si);
}

// ObservationGrade grades one sample inside the chunk BYTES and the [03] census counts by it, Consumable carrying the
// whole policy: a completeness screen folds the consumable grades over the expected count, so a stream reporting on
// cadence yet grading three-quarters Suspect reads as the gap it is. Substituted marks a gap-filled estimate that
// stays consumable BY DECLARATION so a filled series is usable and honestly labelled; Missing occupies a slot the
// cadence owed and nothing arrived for, counting against completeness rather than vanishing from the denominator.
[SmartEnum<string>]
public sealed partial class ObservationGrade {
 public static readonly ObservationGrade Measured = new("measured", consumable: true);
 public static readonly ObservationGrade Validated = new("validated", consumable: true);
 public static readonly ObservationGrade Substituted = new("substituted", consumable: true);
 public static readonly ObservationGrade Suspect = new("suspect", consumable: false);
 public static readonly ObservationGrade Missing = new("missing", consumable: false);

 public bool Consumable { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// ObservationChunk addresses one by-reference sample block: the half-open window it covers, the content key reaching
// its bytes in the same seed-zero XxHash128 store the geometry blobs and the assessment ResultBlob use, and the sample
// count the window carries (the completeness numerator and the Append positivity gate). Admission BOUNDS both window
// ends — NodaTime Interval.Start/End throw on an unbounded side — so every interior read of an admitted chunk is
// total. Mirrors the Geospatial/coverage#COVERAGE_NODE TimeSlice row shape onto a sample axis.
public readonly partial record struct ObservationChunk(Interval Window, UInt128 SeriesKey, int SampleCount) {
 // Encode FIXES the blob layout the SeriesKey addresses and mints the chunk from it in one call, so the bytes and
 // their content key can never derive from two projections. The run writes through the SAME CanonicalWriter the node
 // identity uses — fixed-width little-endian I64 Unix ticks, the IEEE-754 Double canon, the count prefix — so a
 // Python or TypeScript reader over the one XxHash128 seed decodes byte-identically with no second codec. Instants
 // are ABSOLUTE: Cadence is Option, so an event-driven stream has no delta base, and a delta encoding would make the
 // one layout unreadable for exactly the streams whose completeness is already unanswerable. Grade rides its own
 // ObservationGrade key, the same token the [03] census counts by, so blob and summary share one vocabulary.
 // ONE strict-adjacency gate carries the whole ordering law: every later instant exceeds its predecessor, so a
 // duplicate stamp refuses beside a backward one and a run of two or more spans a real window by construction —
 // a first-versus-last window comparison beside it re-proves what adjacency already proved and rejects the honest
 // SINGLE-sample block, whose window is legitimately degenerate.
 public static Fin<(ObservationChunk Chunk, ReadOnlyMemory<byte> Bytes)> Encode(
  Seq<(Instant At, double Si, ObservationGrade Grade)> samples, Op key) =>
  Accumulate(Seq(
    Gate(!samples.IsEmpty, key, "<observation-chunk-sampleless>"),
    Gate(!samples.Zip(samples.Tail).Exists(static pair => pair.Item2.At <= pair.Item1.At), key, "<observation-samples-not-advancing>")))
   .Map(_ => Written(samples))
   .ToFin();

 // Decode is the RAILED inverse every consumer of a fetched blob takes: a truncated run, a trailing partial triple,
 // or a grade token the vocabulary never minted refuses here rather than lifting a fabricated sample through Value.
 // INSTANCE-bound because the mismatch gate compares the fetched bytes against THIS row's own SeriesKey — the block
 // addressing the blob is the only value that knows which content the store owed.
 public Fin<Seq<(Instant At, double Si, ObservationGrade Grade)>> Decode(ReadOnlyMemory<byte> blob, Op key) =>
  ContentHash.Of(blob.Span) == SeriesKey
   ? Read(blob, key)
   : ElementFault.ValueRejected(key, $"<observation-chunk-key-mismatch:{SeriesKey:x32}>");

 // IsBounded gates every OTHER read on this row. Interval.Start/End THROW on an unbounded side, and the admission
 // slots evaluate eagerly (each Gate is an argument, not a lazy arm), so an ordering or spanning gate reading an
 // endpoint before the boundedness gate refuses would THROW past the rail instead of railing — every such gate
 // therefore short-circuits on this predicate and leaves the refusal to the one gate that owns it.
 public bool IsBounded => Window.HasStart && Window.HasEnd;

 public bool Covers(Instant at) => Window.Contains(at);

 // Overlaps reads half-open, so two chunks touching at one instant do NOT overlap — the adjacency Append mints.
 // BOTH sides short-circuit on boundedness under this row's own IsBounded law, so the predicate stays total over a
 // caller-supplied query window and the typed refusal belongs to the ChunksIn gate that owns it.
 public bool Overlaps(Interval other) =>
  IsBounded && other.HasStart && other.HasEnd && Window.Start < other.End && other.Start < Window.End;

 public Duration Span => Window.Duration;

 public void CanonicalBytes(CanonicalWriter w) =>
  w.I64(Window.Start.ToUnixTimeTicks()).I64(Window.End.ToUnixTimeTicks()).U128(SeriesKey).Ordinal(SampleCount);

 // Layout runs Ordinal(count) then one (I64 tick, Double si, String grade-key) triple per sample in stored order.
 // Tolerance plays no part in a sample run, so the writer opens at zero — quantizing a measurement to the model's
 // geometric tolerance would silently round every reading a millimetre-scale model happens to declare.
 private static (ObservationChunk, ReadOnlyMemory<byte>) Written(Seq<(Instant At, double Si, ObservationGrade Grade)> samples) {
  CanonicalWriter w = new(0d);
  w.Ordinal(samples.Count);
  foreach ((Instant at, double si, ObservationGrade grade) in samples) { w.I64(at.ToUnixTimeTicks()).Double(si).String(grade.Key); }
  ReadOnlyMemory<byte> bytes = w.ToBytes();
  // The window is the run's OWN extent, degenerate for a single sample — a synthetic tick of width is fabricated
  // evidence, so every window-positivity gate downstream (Append's and Rehydrate's alike) carries the
  // single-sample exemption instead and a one-reading block reports the instant it actually holds.
  return (new ObservationChunk(
   new Interval(samples[0].At, samples[samples.Count - 1].At),
   ContentHash.Of(bytes.Span), samples.Count), bytes);
 }

 // Read inverts the writer's own primitives — LE Int32 count, LE Int64 ticks, LE Int64 IEEE-754 bits, LE Int32
 // length-prefixed UTF-8 token — walking a cursor the span bounds-check guards. Every shortfall refuses through the
 // one Truncated slot, so a clipped blob never yields a partial run a completeness screen then reads as a real gap,
 // and a grade token the vocabulary never minted refuses at its own generated gate.
 private static Fin<Seq<(Instant At, double Si, ObservationGrade Grade)>> Read(ReadOnlyMemory<byte> blob, Op key) {
  ReadOnlySpan<byte> span = blob.Span;
  if (span.Length < sizeof(int)) { return Truncated(key); }
  int declared = BinaryPrimitives.ReadInt32LittleEndian(span);
  int cursor = sizeof(int);
  // The count prefix is UNTRUSTED metadata a persisted blob carries: a negative one clamped to an empty array
  // decodes as a legitimately empty run, and an inflated one allocates the whole array before the first bounds
  // check refuses it. The declared count therefore admits against the SMALLEST record the layout can carry — two
  // fixed-width I64s and a zero-length token prefix — over the bytes that actually remain.
  if (declared < 0 || declared > (span.Length - sizeof(int)) / ((sizeof(long) * 2) + sizeof(int))) { return Truncated(key); }
  (Instant, double, ObservationGrade)[] run = new (Instant, double, ObservationGrade)[declared];
  for (int index = 0; index < declared; index++) {
   if (cursor + (sizeof(long) * 2) + sizeof(int) > span.Length) { return Truncated(key); }
   long ticks = BinaryPrimitives.ReadInt64LittleEndian(span[cursor..]);
   double si = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(span[(cursor + sizeof(long))..]));
   int length = BinaryPrimitives.ReadInt32LittleEndian(span[(cursor + (sizeof(long) * 2))..]);
   cursor += (sizeof(long) * 2) + sizeof(int);
   if (length < 0 || cursor + length > span.Length) { return Truncated(key); }
   string token = Encoding.UTF8.GetString(span.Slice(cursor, length));
   cursor += length;
   if (!ObservationGrade.TryGet(token, out ObservationGrade? grade) || grade is not { } admitted) {
    return ElementFault.ValueRejected(key, $"<observation-grade-unknown:{token}>");
   }
   run[index] = (Instant.FromUnixTimeTicks(ticks), si, admitted);
  }
  return cursor == span.Length ? Fin.Succ(toSeq(run)) : Truncated(key);
 }

 private static Fin<Seq<(Instant At, double Si, ObservationGrade Grade)>> Truncated(Op key) =>
  ElementFault.ValueRejected(key, "<observation-chunk-truncated>");
}

// SensorProvenance audits the instrument a measured figure is only as good as: make/model/serial identify the device,
// CalibratedAt dates the last traceable calibration (a consumer screens a stale calibration before trusting a
// verdict), and Tolerance carries the manufacturer's stated uncertainty as the seam's OWN MeasureBand centred on ZERO
// (a ±0.5 K spec spells [-0.5, +0.5]) — reusing the quantity owner's uncertainty carrier rather than minting a second
// accuracy shape, and centred so Value SHIFTS it onto each sample's magnitude and WithUncertainty's containment gate
// holds for every sample rather than for the one nominal an absolute band was written around. Rides a SEPARATE
// additive axis the content key never folds (the OwnerHistory exclusion): a re-calibration never re-keys the stream.
public readonly record struct SensorProvenance(
 string Manufacturer, string Model, string Serial,
 Option<LocalDate> CalibratedAt = default, Option<MeasureBand> Tolerance = default) {
 public static readonly SensorProvenance Unattributed = new("", "", "");
}

// [Equatable] is LOAD-BEARING (the Graph/element#NODE_MODEL [STRUCTURAL_EQUALITY] drill): the diff descends into a
// node member only when the member is itself [Equatable], and the id keys on the stream identity alone, so an Append
// mutates the SAME node in place and surfaces as Nodes[id].Series.Chunks[i] / .Statistics member paths — the
// Rasm.Persistence StructuralMerge granularity that reconciles two branches appending disjoint windows. A plain
// record is an opaque equality leaf (whole-series replacement on every append, the deleted form). Chunks take the
// ORDERED comparer because the run's order IS its law; Statistics is a derived leaf replaced wholesale.
[Equatable]
public sealed partial record ObservationSeries {
 public SensorId Sensor { get; }
 public PropertyName Aspect { get; }
 public QuantityType Observed { get; }
 public Dimension Signature { get; }
 public string CanonicalUnit { get; }
 public SamplingKind Sampling { get; }
 // Cadence declares the NOMINAL sampling interval — None for an event-driven or change-of-value stream, whose
 // completeness stays unanswerable by construction (Expected returns None rather than fabricating a denominator).
 public Option<Duration> Cadence { get; }
 public Interval Window { get; }
 [OrderedEquality] public Seq<ObservationChunk> Chunks { get; }
 public SeriesStatistics Statistics { get; }
 public SensorProvenance Provenance { get; }

 // PRIVATE ctor + GET-ONLY members — every instance crosses Open, the order-gated Append, or the re-validating
 // Rehydrate, so an overlapping run, an unbounded window, or a census exceeding its own sample count is
 // UNREPRESENTABLE even off a tampered store; no init/set survives for an external `with` to bypass.
 private ObservationSeries(
  SensorId sensor, PropertyName aspect, QuantityType observed, Dimension signature, string canonicalUnit,
  SamplingKind sampling, Option<Duration> cadence, Interval window, Seq<ObservationChunk> chunks,
  SeriesStatistics statistics, SensorProvenance provenance) =>
  (Sensor, Aspect, Observed, Signature, CanonicalUnit, Sampling, Cadence, Window, Chunks, Statistics, Provenance) =
   (sensor, aspect, observed, signature, canonicalUnit, sampling, cadence, window, chunks, statistics, provenance);

 // Open seeds an EMPTY series at its deployment instant — the degenerate Interval(start, start) states the honest
 // extent of a stream carrying no data yet, and every later End derives from an appended chunk, never a hand-set
 // bound. Gates ACCUMULATE (the shared Projection/fault#ADMISSION_SLOTS fold), so a blank unit and a non-positive
 // cadence report together.
 public static Fin<ObservationSeries> Open(
  SensorId sensor, PropertyName aspect, QuantityType observed, Dimension signature, string canonicalUnit,
  SamplingKind sampling, Option<Duration> cadence, Instant start, SensorProvenance provenance, Op key) =>
  Accumulate(Seq(
    Gate(!string.IsNullOrWhiteSpace(canonicalUnit), key, "<observation-unit-blank>"),
    Gate(cadence.ForAll(static span => span > Duration.Zero), key, "<observation-cadence-non-positive>")))
   .Map(_ => new ObservationSeries(
    sensor, aspect, observed, signature, canonicalUnit.Trim(), sampling, cadence,
    new Interval(start, start), Seq<ObservationChunk>(), SeriesStatistics.Empty, provenance))
   .ToFin();

 // Append owns the ONE growth transition: a chunk opens at or after the current End (adjacency, never overlap — the
 // half-open window makes a touching pair legal and a straddling pair illegal), closes after it opens, and carries at
 // least one sample. Window.End advances to that chunk's end and the recomputed [03] summary rides with it, while the
 // node id holds because CanonicalBytes folds Window.Start alone — an appended chunk is the in-place mutation the
 // Assessment Advance flip is for a lifecycle, so a live stream never re-keys its own node. The rider summary is the
 // WHOLE-RUN recompute, so its census total re-proves against the run it claims to summarize: the [03] plane rebuilds
 // from the chunk bytes and carries no authority, and the one figure this seam can check without fetching a blob is
 // exactly that count — a stale or foreign summary riding a growing series is what the anchor refuses.
 public Fin<ObservationSeries> Append(ObservationChunk chunk, SeriesStatistics statistics, Op key) =>
  Accumulate(Seq(
    Gate(chunk.IsBounded, key, "<observation-chunk-unbounded>"),
    // A SINGLE-sample block's window is degenerate by construction (its extent IS one instant), so the positivity
    // law is "at least one sample wide", never "at least one tick wide" — the exemption lives at every window gate
    // (here and at Rehydrate) rather than in a fabricated tick at Encode, which writes an end no reading occurred at.
    Gate(!chunk.IsBounded || chunk.Window.End > chunk.Window.Start || chunk.SampleCount == 1, key, "<observation-chunk-window-empty>"),
    Gate(!chunk.IsBounded || chunk.Window.Start >= Window.End, key, $"<observation-chunk-overlaps:{Sensor.Value}>"),
    Gate(chunk.SampleCount > 0, key, "<observation-chunk-sampleless>"),
    Gate(statistics.IsCoherent, key, "<observation-statistics-incoherent>"),
    Gate(statistics.Observed == SampleCount + chunk.SampleCount, key, "<observation-census-not-run-total>")))
   .Map(_ => new ObservationSeries(
    Sensor, Aspect, Observed, Signature, CanonicalUnit, Sampling, Cadence,
    new Interval(Window.Start, chunk.Window.End), Chunks.Add(chunk), statistics, Provenance))
   .ToFin();

 // Rehydrate gates every CROSS-ASSEMBLY reconstruction the Rasm.Persistence and Graph/wire#WIRE_CODEC decoders take —
 // PUBLIC because those decoders live across the assembly boundary, RAILED because a persisted run is NOT trusted
 // truth (the ContentAddress.Verify posture): the stored run re-proves bounded-and-advancing through the Zip-adjacent
 // monotone law (the coverage pyramid/timeline discipline), the window re-proves it BRACKETS the run at both ends,
 // and the census re-proves coherent AND equal to the run it summarizes — so a tampered store cannot mint an
 // overlapping, unbounded, or self-contradicting series, nor an EMPTY one wearing a wide window and a fabricated
 // summary, which is the shape a completeness screen reads as full coverage over data that never arrived.
 public static Fin<ObservationSeries> Rehydrate(
  SensorId sensor, PropertyName aspect, QuantityType observed, Dimension signature, string canonicalUnit,
  SamplingKind sampling, Option<Duration> cadence, Interval window, Seq<ObservationChunk> chunks,
  SeriesStatistics statistics, SensorProvenance provenance, Op key) =>
  Accumulate(Seq(
    Gate(!string.IsNullOrWhiteSpace(canonicalUnit), key, "<observation-unit-blank>"),
    Gate(cadence.ForAll(static span => span > Duration.Zero), key, "<observation-cadence-non-positive>"),
    Gate(window.HasStart && window.HasEnd && window.End >= window.Start, key, "<observation-window-unbounded>"),
    // The chunk gate carries the SAME single-sample exemption Append and Encode admit: a one-reading block's window
    // is legitimately degenerate (its extent IS one instant), so the positivity law reads "at least one sample wide".
    // A gate without the disjunct refuses a run this page's own growth transition grew, and the series then cannot
    // rehydrate from the store it was legally written into.
    Gate(chunks.ForAll(static chunk => chunk.IsBounded && (chunk.Window.End > chunk.Window.Start || chunk.SampleCount == 1) && chunk.SampleCount > 0), key, "<observation-chunk-degenerate>"),
    Gate(!chunks.Zip(chunks.Tail).Exists(static pair => pair.Item1.IsBounded && pair.Item2.IsBounded && pair.Item2.Window.Start < pair.Item1.Window.End), key, "<observation-chunks-not-advancing>"),
    Gate(Spans(window, chunks), key, "<observation-window-does-not-span-chunks>"),
    Gate(Brackets(window, chunks), key, "<observation-window-does-not-bracket-run>"),
    Gate(statistics.IsCoherent, key, "<observation-statistics-incoherent>"),
    Gate(statistics.Observed == chunks.Fold(0, static (total, chunk) => total + chunk.SampleCount), key, "<observation-census-not-run-total>"),
    Gate(!chunks.IsEmpty || statistics == SeriesStatistics.Empty, key, "<observation-empty-series-summarized>")))
   .Map(_ => new ObservationSeries(
    sensor, aspect, observed, signature, canonicalUnit.Trim(), sampling, cadence,
    window, chunks, statistics, provenance))
   .ToFin();

 // --- [FETCH_READS] ---------------------------------------------------------------------
 // ChunkAt and ChunksIn select from METADATA alone (the CoverageGrid SliceAt/SliceWindow discipline): a consumer
 // resolves WHICH blocks answer its question off the node, then reads only those blobs by content key. Fetching the
 // whole series to answer a one-day window is the deleted form.
 public Option<ObservationChunk> ChunkAt(Instant at) => Chunks.Find(chunk => chunk.Covers(at));

 // The QUERY window is the one un-admitted input this page takes — every stored window crossed Open, Append, or
 // Rehydrate — so the fetch read admits it before a single chunk is compared, RAILED because an unbounded extent
 // selects nothing while looking like a legitimately empty answer and because NodaTime endpoint reads THROW on an
 // unbounded side, past the rail rather than onto it.
 public Fin<Seq<ObservationChunk>> ChunksIn(Interval window, Op key) =>
  window.HasStart && window.HasEnd
   ? Fin.Succ(Chunks.Filter(chunk => chunk.Overlaps(window)))
   : ElementFault.ValueRejected(key, "<observation-query-window-unbounded>");

 public int SampleCount => Chunks.Fold(0, static (total, chunk) => total + chunk.SampleCount);

 // Expected derives what a REGULAR cadence owes over a window: an Interval row summarizes closed windows so it owes a
 // whole number of cadence spans, while an instant row samples boundaries so it owes one more. None for an
 // event-driven stream — an unanswerable denominator reads as absence, never a fabricated 1 — and None for an
 // UNBOUNDED query window, whose duration is unanswerable in exactly the same sense and whose endpoint read would
 // THROW past the rail rather than answer it.
 public Option<int> Expected(Interval window) =>
  window.HasStart && window.HasEnd
   ? Cadence.Map(cadence =>
     (int)(window.Duration.ToInt128Nanoseconds() / cadence.ToInt128Nanoseconds()) + (Sampling.Interval ? 0 : 1))
   : None;

 // Lift ONE decoded SI scalar into the seam's typed measure under THIS series' quantity triple, the instrument
 // tolerance SHIFTED onto that magnitude (the zero-centred band re-admitted around si, so WithUncertainty's
 // containment invariant holds for every sample) — so a consumer that decoded chunk bytes never re-derives the
 // quantity type, the dimension, the canonical unit, or the instrument error, and every downstream Multiply/Divide/Sum
 // propagates that error through the Properties/quantity#MEASURE_VALUE algebra with zero call-site arithmetic.
 public Fin<MeasureValue> Value(double si, Op key) =>
  MeasureValue.OfAdmitted(Observed, Signature, si, CanonicalUnit).Bind(measure =>
   Provenance.Tolerance.Match(
    Some: tolerance => MeasureBand
     .Admit(tolerance.Kind, si + tolerance.LowerSi, si + tolerance.UpperSi, tolerance.StandardDeviationSi, tolerance.CoverageFactor, key)
     .Bind(band => measure.WithUncertainty(band, key)),
    None: () => Fin.Succ(measure)));

 // CanonicalBytes writes the STREAM identity — sensor, aspect, quantity triple, sampling algebra, cadence, deployment
 // instant. It EXCLUDES the chunk run, the advancing Window.End, the derived Statistics, and the SensorProvenance
 // audit, so an append or a re-calibration mutates the node in place while a re-deployment at a fresh instant mints a
 // fresh node. Every instant rides the writer's fixed-width I64 Unix-tick canon, so identity stays byte-stable
 // across every C#/Python/TypeScript runtime sharing the one XxHash128 seed.
 public void CanonicalBytes(CanonicalWriter w) {
  w.String(Sensor.Value).String(Aspect.Value).String(Observed.Value);
  w.Ordinal(Signature.Length).Ordinal(Signature.Mass).Ordinal(Signature.Time).Ordinal(Signature.Current)
   .Ordinal(Signature.Temperature).Ordinal(Signature.Amount).Ordinal(Signature.LuminousIntensity);
  w.String(CanonicalUnit).String(Sampling.Key).Bool(Cadence.IsSome);
  Cadence.IfSome(cadence => w.I64(cadence.BclCompatibleTicks));
  w.I64(Window.Start.ToUnixTimeTicks());
 }

 // --- [ADMISSION_GATES] -----------------------------------------------------------------
 // Spans proves the window brackets the whole run — every chunk opens at or after Window.Start and closes at or
 // before Window.End. Folds rather than reading a Head/Last pair, so an empty run stays vacuous; an unbounded window
 // or chunk passes vacuously here and refuses at the gate that owns it, never through a throwing endpoint read.
 private static bool Spans(Interval window, Seq<ObservationChunk> chunks) =>
  !window.HasStart || !window.HasEnd
  || chunks.ForAll(chunk => !chunk.IsBounded || (chunk.Window.Start >= window.Start && chunk.Window.End <= window.End));

 // Brackets states the window-to-run correspondence Open and Append MAINTAIN, restated as data for a decoded run:
 // an empty run stands at the degenerate deployment instant Open seeds, and a populated run closes the window at its
 // last chunk exactly as Append advances End. Spans alone passes an empty run under any width, which is how a
 // rehydrated series claims a year of coverage holding nothing. Unbounded sides pass vacuously and refuse at the
 // gate that owns them, the Spans discipline.
 private static bool Brackets(Interval window, Seq<ObservationChunk> chunks) =>
  !window.HasStart || !window.HasEnd
  || (chunks.IsEmpty
   ? window.End == window.Start
   : !chunks[chunks.Count - 1].IsBounded || chunks[chunks.Count - 1].Window.End == window.End);
}
```

## [03]-[SERIES_STATISTICS]

- Owner: `SeriesStatistics` the DERIVED per-series summary the node carries so a completeness screen, a range bound, and the comparison figure read flat off the graph — the graded `Census`, the covered `Span`, and the `Minimum`/`Maximum`/`Mean`/`Total` aggregates as typed `Properties/quantity#MEASURE_VALUE` `MeasureValue`s.
- Entry: `SeriesStatistics.From(run, sampling, type, signature, unit, key)` derives the whole summary off one decoded sample run — the ONE owner of the register-span rule and of the monotone-run refusal that rule presupposes; `SeriesStatistics.Of(census, span, minimum, maximum, mean, total)` takes an already-computed column set; `Fold(left, right, key)` merges two ADJACENT summaries — the ONE merge every rollup composes; `Observed` sums the census and `Consumable` folds it over the `ObservationGrade.Consumable` column; `Completeness(expected)` reads the consumable share against a cadence-derived denominator (`None` where the denominator is unanswerable); `Representative(sampling, key)` reads the ONE comparable figure the sampling row designates — the mean for an averaged or instantaneous stream, the total for a totalled or cumulative one, the extremum for a min or max one; `IsCoherent` is the shape law `Append` and `Rehydrate` re-impose.
- Auto: `From`'s four column legs are INDEPENDENT, so each is one concrete `Validation<Error,_>` slot the tuple `Apply` unions and one `.ToFin()` collapses at the return, a run failing two columns reporting both; the census is a `Map<ObservationGrade, int>` keyed by the closed grade vocabulary, so a new grade row lands with zero census-shape edit and the consumable fold derives from the row's own column rather than a hand-maintained allowed set; `Fold` is COLUMN-driven, not sampling-driven — the mean column always merges duration-weighted and the total column always adds, each reusing the `SamplingKind.Averaged` and `SamplingKind.Total` `Combine` rows so the weighted-mean and additive arithmetic are owned once at the vocabulary, the extremes merge by extremum, the census sums key-wise, and the spans add; `Completeness` divides the consumable count by the expected count, so a stream reporting on cadence yet graded three-quarters `Suspect` reads as the gap it is rather than as full coverage; `IsCoherent` holds when every census count is non-negative, the span is non-negative, every populated column shares one quantity identity (type, dimension signature, canonical unit), and the extremes bracket the mean wherever all three are present — the same invariant `From` and `Fold` guarantee by construction, restated as data so a summary reaching `Of` or the record constructor re-proves against itself the way the `Assessment/assessment#ASSESSMENT_NODE` `Coherent` column re-proves a persisted lifecycle.
- Receipt: `series.Statistics` is the pre-decode screen every consumer reads first — `Completeness` gates whether a verdict may be drawn at all, `Minimum`/`Maximum` bound a plausibility check, and `Representative` is the single figure a `Rasm.Compute` commissioning route subtracts against the matching `Assessment/assessment#ASSESSMENT_NODE` `ResultMeasure`, so the declared-versus-metered comparison reads two flat values off one baked element and the verdict writes back as an `Assessment` node whose `DependsOn` names the series.
- Packages: Thinktecture.Runtime.Extensions (the `ObservationGrade`/`SamplingKind` generated `Key`/`Items`/`Switch` surfaces the census and the representative dispatch read), LanguageExt.Core (`Map`/`Option`/`Fin`/`Seq` + the `Fold` reductions and the concrete `Validation<Error,_>` column slots the tuple `Apply` accumulates through before one `.ToFin()` collapse), NodaTime (`Duration` the covered span and its integer division), `Properties/quantity#MEASURE_VALUE` (`MeasureValue` the typed aggregates, `MeasureValue.OfAdmitted` the trusted re-mint the derivation and the merge both stamp their columns through under the series' own quantity triple).
- Growth: a new summary column is one `SeriesStatistics` field with its `From` derivation, its `Fold` arm, and its `IsCoherent` clause; a new grade is one `ObservationGrade` row the census absorbs with no shape edit; a new representative rule is one `SamplingKind` row the total `Switch` already forces to answer; never a per-quantity statistics type and never a second summary beside this one.
- Boundary: `SeriesStatistics` carries ZERO authority (`libs/.planning/RULINGS.md` truth-plane law) — the `ObservationChunk` bytes are the truth plane, this summary is an accelerator that DROPS at warm-up cost and rebuilds by re-reading the referenced blobs, and its retention floor is exactly the chunk retention; it is canon-EXCLUDED from `ObservationSeries.CanonicalBytes`, so a recomputed summary never forks the node id and a summary disagreeing with its blobs repairs by rebuild rather than by trusting the stored figure; a consumer reading `Mean` where the raw distribution decides — a percentile, a duty-cycle histogram, a spectral fold — reads the chunk bytes instead, because the mean is the accelerator and the samples are the record; `Representative` is a SEAM projection of the sampling semantics the seam owns, never a verdict — the comparison, the tolerance band, and the pass rule are `Rasm.Compute`'s; `From` owns the register-span rule so no producer re-derives it — a `Cumulative` run's `Total` is its last reading less its first, the consumption across the window rather than the odometer, and every other algebra folds through `SamplingKind.Total`; `Of` survives for a producer that already holds the columns, and the run stays the caller's to supply because this page addresses bytes and never fetches them.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// SeriesStatistics DERIVES off the chunk bytes: zero authority, drops at warm-up cost, rebuilds from the blobs the
// chunk run addresses. Canon-EXCLUDED from the series identity, so a recompute never forks the node id. Census keys
// on the closed ObservationGrade vocabulary, so the consumable fold derives from the row's own column and a new grade
// needs no shape edit; a hand-maintained allowed set beside the rows is the deleted form.
public readonly record struct SeriesStatistics(
 Map<ObservationGrade, int> Census, Duration Span,
 Option<MeasureValue> Minimum, Option<MeasureValue> Maximum, Option<MeasureValue> Mean, Option<MeasureValue> Total) {

 public static readonly SeriesStatistics Empty =
  new(Map<ObservationGrade, int>(), Duration.Zero, None, None, None, None);

 public static SeriesStatistics Of(
  Map<ObservationGrade, int> census, Duration span,
  Option<MeasureValue> minimum, Option<MeasureValue> maximum, Option<MeasureValue> mean, Option<MeasureValue> total) =>
  new(census, span, minimum, maximum, mean, total);

 // From derives the WHOLE summary off one decoded run, so the register-span rule is owned HERE rather than
 // re-derived by every producer: a Cumulative run's Total is its LAST reading less its FIRST — the consumption
 // across the window — while every other algebra folds through SamplingKind.Total's additive Combine. Mean always
 // folds duration-weighted through SamplingKind.Averaged, matching Fold's column-driven law, and each sample's
 // weight is its span to the next reading with the final sample carrying the run's own mean weight, so an
 // irregular run never over-weights its tail. The seam takes the RUN, never the blob — Encode/Decode own the bytes.
 public static Fin<SeriesStatistics> From(
  Seq<(Instant At, double Si, ObservationGrade Grade)> run,
  SamplingKind sampling, QuantityType type, Dimension signature, string unit, Op key) {
  if (run.IsEmpty) { return Fin.Succ(Empty); }
  // A Monotone row's register may only climb, so a backward step is a rollover or a replacement, never negative
  // consumption — and the register-span Total below would mint exactly that negative figure from it. The refusal
  // therefore owns the run BEFORE any column derives, so no consumer receives a summary the algebra forbids.
  if (sampling.Monotone && run.Zip(run.Tail).Exists(static pair => pair.Item2.Si < pair.Item1.Si)) {
   return ElementFault.ValueRejected(key, "<observation-register-decreasing>");
  }
  Duration span = run[run.Count - 1].At - run[0].At;
  Seq<(double Si, Duration Span)> weighted = Weighted(run, span);
  Map<ObservationGrade, int> census = run.Fold(Map<ObservationGrade, int>(),
   static (map, sample) => map.AddOrUpdate(sample.Grade, static count => count + 1, 1));
  // The four columns are INDEPENDENT of one another, so they ACCUMULATE (VALIDATION_MONOID): each leg is one
  // concrete Validation<Error,_> slot, the tuple .Apply unions every ValueRejected through Error.Combine/ManyErrors,
  // and .ToFin() collapses ONCE at the return — a run whose extremum and whose fold both refuse reports both, the
  // grammar every sibling admission on this branch rides. Downsample answers None only over an EMPTY weighted run,
  // which the empty-run return above already excluded, so the absent arm RAILS rather than substituting a
  // first-sample default that would report one reading as the whole run's total.
  return (Value(type, signature, run.Fold(run[0].Si, static (low, s) => Math.Min(low, s.Si)), unit),
    Value(type, signature, run.Fold(run[0].Si, static (high, s) => Math.Max(high, s.Si)), unit),
    Folded(SamplingKind.Averaged, weighted, type, signature, unit, key),
    sampling.Monotone
     ? Value(type, signature, run[run.Count - 1].Si - run[0].Si, unit)
     : Folded(SamplingKind.Total, weighted, type, signature, unit, key))
   .Apply((low, high, mean, sum) => new SeriesStatistics(census, span, low, high, mean, sum)).As().ToFin();
 }

 // One accumulating downsample leg: the row's own algebra folds the weighted run, and an unfoldable run refuses on
 // the concrete Validation carrier the derivation joins, the bare-fault ternary lift the admission grammar rides.
 private static Validation<Error, Option<MeasureValue>> Folded(
  SamplingKind algebra, Seq<(double Si, Duration Span)> weighted,
  QuantityType type, Dimension signature, string unit, Op key) =>
  algebra.Downsample(weighted).Case is double si
   ? Value(type, signature, si, unit)
   : ElementFault.ValueRejected(key, $"<observation-summary-unfoldable:{algebra.Key}>");

 // Each weight is the gap to the next reading; the final sample takes the run's own average gap, so a single-sample
 // run weighs one positive second rather than zero and a weighted mean never divides by nothing.
 private static Seq<(double Si, Duration Span)> Weighted(
  Seq<(Instant At, double Si, ObservationGrade Grade)> run, Duration span) {
  Duration tail = run.Count > 1 ? span / (run.Count - 1) : Duration.FromSeconds(1);
  return run.Map((sample, index) =>
   (sample.Si, index + 1 < run.Count ? run[index + 1].At - sample.At : tail));
 }

 // The trusted RE-MINT of one derived column under the series' own stamped triple, lifted onto the accumulating
 // carrier through .ToValidation() — the ONE conversion shape a Fin column takes into this derivation.
 private static Validation<Error, Option<MeasureValue>> Value(QuantityType type, Dimension signature, double si, string unit) =>
  MeasureValue.OfAdmitted(type, signature, si, unit).ToValidation().Map(static measure => Some(measure)).As();

 // Every sample the census accounts for — the Append coherence anchor and the completeness numerator's partner.
 // Both reads take the map's own KEY-BEARING three-argument fold: the carrier-generic fold carries the VALUE alone,
 // so a pair-shaped lambda binds the count to the element slot and fails at the first member read.
 public int Observed => Census.Fold(0, static (sum, _, count) => sum + count);

 // Consumable folds the grade row's own column, so a Suspect-heavy stream reads as the gap it is, never full coverage.
 public int Consumable => Census.Fold(0, static (sum, grade, count) => sum + (grade.Consumable ? count : 0));

 // Consumable coverage against the cadence-derived denominator; None where the series is event-driven (Expected
 // returns None) or the window owes nothing — a fabricated 1.0 over a zero denominator is the deleted form.
 public Option<double> Completeness(Option<int> expected) =>
  expected.Filter(static count => count > 0).Map(count => Consumable / (double)count);

 // Columns of ONE series carry ONE quantity identity — From mints all four through the series' own triple and Fold
 // re-mints under the left operand's — so the whole set is comparable by construction at both owners. Of and the
 // record's own constructor take an already-composed column set, which is the route a decoded or hand-composed
 // summary takes, so the agreement restates as data here: a mismatched quantity type, dimension signature, or
 // canonical unit across two columns puts two quantities under one summary and every downstream subtraction,
 // extremum, and merge then reads them as one magnitude.
 private Seq<MeasureValue> Columns =>
  Seq(Minimum, Maximum, Mean, Total).Fold(Seq<MeasureValue>(),
   static (kept, column) => column.Match(Some: kept.Add, None: () => kept));

 // IsCoherent states the shape law Append and Rehydrate re-impose: no negative census count, a non-negative span,
 // one quantity identity across every populated column, and extremes bracketing the mean wherever all three are
 // present.
 public bool IsCoherent =>
  Census.ForAll(static (_, count) => count >= 0)
  && Span >= Duration.Zero
  // CanonicalUnit is Option<string> at its owner, so the agreement reads the carrier's own equality — Some/None and
  // the ordinal string comparison in one test, where a bare string.Equals cannot take the optional at all.
  && Columns.Head.Map(first => Columns.ForAll(column =>
    column.Type == first.Type && column.Dimension == first.Dimension
    && column.CanonicalUnit == first.CanonicalUnit)).IfNone(true)
  && (Minimum.Case, Maximum.Case, Mean.Case) switch {
   (MeasureValue low, MeasureValue high, MeasureValue centre) => low.Si <= centre.Si && centre.Si <= high.Si,
   _ => true,
  };

 // Fold merges two ADJACENT summaries (left earlier), COLUMN-driven rather than sampling-driven: a mean stays a mean
 // under every sampling algebra so it merges duration-weighted, and a total stays a total so it adds — each reuses
 // whichever SamplingKind row already owns that arithmetic rather than re-spelling it here. Passing the series' own
 // sampling row would merge a Total series' mean additively, the arithmetic error this split forecloses. Every
 // rollup — a downsample, a cross-chunk summary, a whole-series total — composes THIS body.
 public static Fin<SeriesStatistics> Fold(SeriesStatistics left, SeriesStatistics right, Op key) =>
  Merged(left.Mean, right.Mean, left.Span, right.Span, SamplingKind.Averaged, key).Bind(mean =>
   Merged(left.Total, right.Total, left.Span, right.Span, SamplingKind.Total, key).Map(total =>
    new SeriesStatistics(
     right.Census.Fold(left.Census, static (census, grade, count) => census.AddOrUpdate(grade, existing => existing + count, count)),
     left.Span + right.Span,
     Extremum(left.Minimum, right.Minimum, static (a, b) => a.Si <= b.Si),
     Extremum(left.Maximum, right.Maximum, static (a, b) => a.Si >= b.Si),
     mean, total)));

 // Representative reads the single comparable figure the sampling row designates — what a commissioning route
 // subtracts a computed assessment result against. Cumulative reads its TOTAL column, which From derives as the
 // register SPAN over the window — last reading less first — never the raw odometer; owning that at From keeps
 // every producer from re-deriving it per metering route. The generated total Switch forces every row to answer.
 public Fin<MeasureValue> Representative(SamplingKind sampling, Op key) =>
  sampling.Switch(
   instantaneous: () => Figure(Mean, key, "mean"),
   averaged: () => Figure(Mean, key, "mean"),
   total: () => Figure(Total, key, "total"),
   cumulative: () => Figure(Total, key, "register-span"),
   minimum: () => Figure(Minimum, key, "minimum"),
   maximum: () => Figure(Maximum, key, "maximum"));

 // --- [MERGE_PRIMITIVES] ----------------------------------------------------------------
 // Merged re-mints under the left operand's type and unit — both sides share one series, so the quantity triple is
 // identical by construction — and a non-finite product rails through MeasureValue's own finite gate rather than
 // landing in the summary. One-sided columns carry through untouched: a chunk recording no total never zeros the run's.
 private static Fin<Option<MeasureValue>> Merged(
  Option<MeasureValue> left, Option<MeasureValue> right, Duration leftSpan, Duration rightSpan, SamplingKind algebra, Op key) =>
  (left.Case, right.Case) switch {
   (MeasureValue a, MeasureValue b) =>
    MeasureValue.OfAdmitted(a.Type, a.Dimension, algebra.Combine(a.Si, leftSpan.TotalSeconds, b.Si, rightSpan.TotalSeconds), a.CanonicalUnit)
     .Map(static merged => Some(merged)),
   (MeasureValue, _) => Fin.Succ(left),
   _ => Fin.Succ(right),
  };

 private static Option<MeasureValue> Extremum(
  Option<MeasureValue> left, Option<MeasureValue> right, Func<MeasureValue, MeasureValue, bool> wins) =>
  (left.Case, right.Case) switch {
   (MeasureValue a, MeasureValue b) => wins(a, b) ? left : right,
   (MeasureValue, _) => left,
   _ => right,
  };

 private static Fin<MeasureValue> Figure(Option<MeasureValue> value, Op key, string column) =>
  value.Match(Some: Fin.Succ, None: () => ElementFault.ValueRejected(key, $"<observation-representative-absent:{column}>"));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
