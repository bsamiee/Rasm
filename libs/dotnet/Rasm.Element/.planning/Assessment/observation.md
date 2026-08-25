# [ELEMENT_OBSERVATION]

`ObservationSeries` owns measured evidence — the `Graph/element#NODE_MODEL` `Node.Observation` case wraps it beside the computed `Assessment/assessment#ASSESSMENT_NODE` assessment as the model's SECOND evidence modality, what the built asset REPORTS against what an analysis PREDICTED. One series binds one deployed sensor to one observed aspect of one element: the `SensorId` deployment identity, the observed `Properties/property#PROPERTY_BAG` `PropertyName` aspect (a wall reports surface temperature AND heat flux, so the `Properties/quantity#MEASURE_VALUE` `QuantityType` alone under-discriminates), the admitted `Properties/quantity#MEASURE_STAT` `QuantitySignature` every decoded sample re-mints a `MeasureValue` through, the `SamplingKind` temporal-aggregation semantics, the nominal `Duration` cadence, the `Interval` observation extent, and an interval-ordered run of content-keyed `ObservationChunk` rows addressing the sample bytes BY REFERENCE in the kernel SHA-256 artifact store the geometry blobs and the `Assessment/assessment#ASSESSMENT_NODE` `ResultArtifact` use — never an inlined sample buffer on the node. Each series attaches to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Observation`), so a commissioning comparison — a declared U-value against a metered heat flux, a predicted annual energy against a metered total — is a `Graph/element#ELEMENT_GRAPH` `Bake` read over `element.Observations` beside `element.Assessments`, never a join against an external historian.

`SamplingKind` carries the load-bearing axis: an instantaneous temperature, an interval-averaged mean radiant temperature, an interval-total delivered energy, and a cumulative meter register are FOUR algebras over one row shape, and folding them alike double-counts a total or averages an odometer. Each row answers three columns — `Interval` (this sample summarizes the preceding cadence window or one instant), `Monotone` (this run admits only a non-decreasing sequence), and the duration-weighted `Combine` merge that makes downsampling, cross-chunk rollup, and the summary merge ONE body per row. `SeriesStatistics` derives off that algebra so a completeness screen, a range bound, and the representative figure read flat off the node, while the chunk bytes stay the truth the summary rebuilds from.

## [01]-[INDEX]

- [02]-[OBSERVATION_SERIES]: `ObservationSeries` carries the node payload beside `SensorId` deployment identity, the `SamplingKind` vocabulary with its `SamplingCapability` set, its `Combine` column, and the `Downsample` fold they drive, `ObservationGrade` per-sample quality, `ObservationChunk` by-reference sample runs, `SensorProvenance` instrument audit, `Open`/`Append`/`Rehydrate` admission, `ChunkAt`/`ChunksIn`/`Expected` fetch reads, the `Value` sample lift, and `CanonicalBytes` stream identity.
- [03]-[SERIES_STATISTICS]: `SeriesStatistics` derives the summary — the `From` run derivation owning the register-span rule, the graded `Census`, `Completeness`/`Consumable` screens, the column-driven `Fold` merge over adjacent summaries, and the `Representative` figure a commissioning route subtracts against a computed assessment result.

## [02]-[OBSERVATION_SERIES]

- Owner: `ObservationSeries` the sensor-bound measured-evidence descriptor the `Node.Observation` case wraps; `SensorId` the `[ValueObject<string>]` DEPLOYMENT identity (a re-mounted instrument takes a new id, so one series never spans two mounting positions); `SamplingKind` the `[SmartEnum<string>]` temporal-aggregation vocabulary carrying a kernel `CapabilitySet<SamplingCapability>` (the `Interval` window-versus-instant axis and the `Monotone` register axis as capability rows, never boolean columns) and the duration-weighted `Combine` merge column its own `Downsample` fold drives; `ObservationGrade` the `[SmartEnum<string>]` per-sample quality vocabulary with its `CapabilitySet<GradeCapability>` (the grade the chunk bytes encode per sample and the `[03]` census counts by); `ObservationChunk` the interval-anchored by-reference sample block (window + `Projection/address#CONTENT_ADDRESS` `BlobKey` + sample count) owning the one blob codec every runtime reads; `SensorProvenance` the instrument audit (make/model/serial, calibration date, and the zero-centred `Properties/quantity#MEASURE_VALUE` `MeasureBand` tolerance every lifted sample shifts onto its own magnitude).
- Cases: `SamplingKind` closes at six rows spanning the metering space a BMS, meter, or structural-health stream reports — `Instantaneous` (a point reading; a resample PICKS, never sums), `Averaged` (the cadence-window mean; merges duration-weighted), `Total` (the cadence-window sum; merges additively), `Cumulative` (a monotone register whose value is an odometer, not a rate; merges last-wins and refuses a decreasing run), `Minimum` and `Maximum` (the cadence-window extremes; merge by extremum) — one case per algebra, never a `kind` string a consumer re-interprets.
- Entry: `ObservationSeries.Open(sensor, aspect, quantity, sampling, cadence, start, provenance, key)` opens an empty series at its deployment instant over one admitted `QuantitySignature` and an `Option<SensorProvenance>` audit, `Fin<T>` routing an unnamed canonical unit through `KernelFault.InvalidValue` and a non-positive cadence through `KernelFault.OutOfRange`; `Append(chunk, statistics, key)` is the ONE growth transition — the chunk's window opens at or after the current `Window.End`, closes after it opens, and carries at least one sample, and the recomputed `[03]` summary rides with it under a census total matching the grown run, so `Window.End` advances while the node id holds; `Rehydrate(...)` is the CROSS-ASSEMBLY decoder gate re-validating the whole stored run against the same invariants `Append` maintains, the single-sample degenerate-window exemption included (bounded window, strictly advancing non-overlapping chunks each at least one sample wide, a window spanning AND bracketing the run, a coherent census equal to the run it summarizes) so a tampered store cannot mint an overlapping, unbounded, self-contradicting, or emptily-summarized series — the `Assessment/assessment#ASSESSMENT_NODE` `Rehydrate` distrust posture; `ChunkAt(instant)` and `ChunksIn(window, key)` are the metadata-selected fetch reads (the `Geospatial/coverage#COVERAGE_NODE` `LevelFor`/`Window` discipline — a consumer resolves WHICH blobs answer a question before a byte moves), the windowed read railing an unbounded query rather than selecting nothing; `Expected(window)` derives the sample count a regular cadence owes over a window (`None` for an event-driven series or an unbounded window, both denominators unanswerable by construction); `Value(si, key)` lifts one decoded SI scalar into a typed `MeasureValue` under the series' own signature with the instrument tolerance shifted onto that magnitude; `SamplingKind.Downsample(samples)` folds a decoded run of `(Si, Span)` pairs into one value under the row's own algebra; `ObservationChunk.Encode(samples, key)` mints the block and its bytes together off one projection, railing a sampleless run and any adjacent pair that fails to strictly advance, and the instance `Decode(blob, key)` inverts it against its own `Series`, railing a key mismatch, an unrepresentable declared count, a truncated run, and an unminted grade token.
- Auto: the series identity is the STREAM, never its contents — `CanonicalBytes` writes the sensor, the aspect, the `QuantitySignature`'s own canonical projection (type, seven exponents, presence-delimited unit — composed off its owner, never respelled), the sampling key, the kernel-`Optional` cadence, and `Window.Start` ALONE, so an append that extends `Window.End` and adds a chunk mutates the SAME content-keyed node in place exactly as an `Assessment/assessment#ASSESSMENT_NODE` `Advance` flip does, while a re-deployment opening at a fresh instant mints a fresh node; the chunk run, the advancing `Window.End`, the derived `[03]` statistics, and the `SensorProvenance` audit are EXCLUDED from that projection (the `Provenance`-exclusion discipline the assessment payload holds), so a re-calibration and a recomputed summary are both content-key-inert; `Open` seeds the degenerate `Interval(start, start)` and every later `Window.End` derives from the last appended chunk, so the extent is never hand-set; `Append` and `Rehydrate` both prove the run strictly advancing through the `Zip`-adjacent law the coverage pyramid and timeline hold, so `ChunkAt` resolves at most one block and `ChunksIn`'s half-open overlap clip is total over stored order once its query window admits; `Expected` divides the window duration by the cadence under the `SamplingCapability.Interval` axis — an interval row owes one sample per closed window while an instant row owes one per boundary, the off-by-one a flat division swallows; the blob layout is FIXED at `Encode` and never inferred — a count prefix then one `(I64 Unix-tick instant, IEEE-754 `Double` magnitude, `ObservationGrade` key)` triple per sample in stored order, written through the same `CanonicalWriter` canon the node identity uses, so the bytes and the `Series` that addresses them derive from one projection and a peer runtime decodes byte-identically off the one seed; instants stay ABSOLUTE because `Cadence` is `Option` and a delta encoding strands exactly the event-driven streams whose completeness is already unanswerable, and the writer opens at zero tolerance so a model's geometric quantization never rounds a reading; `Value` re-mints through the trusted `MeasureValue.OfSi(…, UnitProvenance.Carried(…))` arm and shifts the zero-centred instrument band onto the sample through `MeasureBand.Admit` before `WithUncertainty`, so the band contains its own nominal by construction and the `Properties/quantity#MEASURE_VALUE` propagation algebra carries instrument error through every downstream fold with no call-site arithmetic.
- Output: the `ObservationSeries` is the measured evidence a `Bake`-derived `Element` carries flat in `element.Observations` — `element.Observations.Filter(series => series.Observed == QuantityType.Create("HeatTransferCoefficient"))` reads every metered transmittance stream, `series.Statistics.Completeness(series.Expected(window))` screens a gappy stream before it decides anything, `series.Statistics.Representative(series.Sampling, key)` reads the ONE comparable figure the sampling row designates, and `series.ChunksIn(window, key)` resolves the blob set a windowed fetch reads by content key; the commissioning comparison is then one subtraction in `Rasm.Compute` between that figure and the matching `element.Assessments` `ResultMeasure(name)` — the seam delivering both evidence kinds off one baked element, the verdict staying with the discipline that owns it.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` the `Combine` merge column, `[ValueObject<string>]`, `[ValidationError]`), Generator.Equals (`[Equatable]` the payload's member-level diff + `[OrderedEquality]` the interval-ordered chunk run, so the `Graph/element#NODE_MODEL` drill descends to `Nodes[id].Series.Chunks[i]` and `Nodes[id].Series.Statistics`), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin` + the `Validation<Error,_>` carrier), NodaTime (`Instant` the sample anchor, `Duration` the cadence and span, `Interval` the observation extent, `LocalDate` the calibration stamp), `Rasm/Domain/identity#CONTENT_KEY` (`CanonicalWriter` the `CanonicalBytes` projection and the chunk blob both write through), `Rasm` (the kernel `Op` op-key, `CapabilitySet`/`ICapability`, and `Rasm/Domain/validation#ADMISSION_SLOTS`), `Projection/address#CONTENT_ADDRESS` (`BlobKey` the seed-zero by-reference payload key the chunk carries and `Encode` mints), BCL inbox (`BinaryPrimitives`/`BitConverter`/`Encoding.UTF8` the decode cursor inverts the writer's primitives through).
- Growth: a new metering algebra is one `SamplingKind` row declaring its capability set and its `Combine` merge — every fetch, fold, downsample, and summary absorbs it with zero edits; a new sample quality class is one `ObservationGrade` row declaring its capability set; a new observed aspect is one `PropertyName` a projector stamps (the seam mints no aspect roster, the SAME neutrality the `AnalysisRoute` token holds for the analysis-route roster); a new instrument audit column is one `SensorProvenance` field; a new sample column is one write in `Encode` beside its read in `Decode`, the two halves of one layout moving together; never a per-quantity series type, never a per-vendor payload, never a sample buffer on the node.
- Boundary: `ObservationSeries` holds the samples BY REFERENCE — each `ObservationChunk` addresses its block in the kernel SHA-256 artifact store the geometry and assessment blobs use, so an inlined sample array, a vendor stream handle, or a second hasher on the seam is the named defect; the seam owns the blob LAYOUT while the store owns the bytes, so a residence, an exporter, and a peer runtime each read one declared codec rather than negotiating a per-producer sample format, and `Decode` re-proves the content key before a sample crosses because a persisted blob is untrusted exactly as a persisted series is; the series is OCCURRENCE-ONLY — a `Component` is a catalogue entry and is never instrumented, so `Graph/element#ELEMENT_GRAPH` `Bake` gathers observations from the occurrence's own incidence alone and the named type→occurrence inheritance does NOT carry them, a type-inherited series claiming every realization of one `Component` reports one sensor's data being the deleted form; `SensorId` is DEPLOYMENT-scoped and `Window.Start` folds into the identity, so a re-mounted instrument opens a fresh series rather than splicing two positions into one record; NodaTime `Interval.Start`/`Interval.End` THROW unless `HasStart`/`HasEnd` holds, so every admitted window is bounded at BOTH ends and `Open`/`Append`/`Rehydrate` construct through the two-`Instant` constructor exclusively — a nullable-endpoint `Interval` reaching an interior read is the deleted form, and a CALLER's query window crosses the same bar at the read that takes it, `ChunksIn` railing it and `Expected` answering the absent denominator it is; `SamplingKind` discriminates the algebra, so a fold that sums a `Cumulative` register, averages a `Total`, or interpolates an `Instantaneous` across a gap is the defect the capability set and the `Combine` column delete, and an interval row is never linearly interpolated across a gap because the window it summarizes did not happen; the per-sample `ObservationGrade` lives in the chunk BYTES and only its census crosses onto the node, so a per-sample flag column on the seam is the deleted form; `SensorProvenance.Tolerance` is the instrument band centred on ZERO (the manufacturer's ±δ), shifted onto each sample by `Value` — an absolute band stored against one nominal magnitude fails `WithUncertainty`'s containment gate for every other sample, the deleted form; the observation attaches through an `Assign` edge, never an inlined back-reference on the `Object` node, and the seam owns the evidence while every verdict — a commissioning pass, a degradation trend, a fault-detection rule — lives in `Rasm.Compute` and writes back as an `Assessment/assessment#ASSESSMENT_NODE` node whose `DependsOn` records the series it consumed.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Buffers.Binary;
using System.Text;
using System.Security.Cryptography;
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
using Band = Rasm.Numerics.Band;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Assessment;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class SensorId {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = new ValidationError("sensor id requires a non-blank token"); return; }
  value = value.Trim();
 }

 public static Fin<SensorId> Of(string token, Op key) =>
  key.AcceptValidated<SensorId>(token);
}

[SmartEnum<string>]
public sealed partial class SamplingKind {
 public static readonly SamplingKind Instantaneous = new("instantaneous", CapabilitySet<SamplingCapability>.None, static (left, leftSeconds, right, rightSeconds) => right);
 public static readonly SamplingKind Averaged = new("averaged", CapabilitySet<SamplingCapability>.Of(SamplingCapability.Interval), static (left, leftSeconds, right, rightSeconds) => leftSeconds + rightSeconds > 0d ? ((left * leftSeconds) + (right * rightSeconds)) / (leftSeconds + rightSeconds) : right);
 public static readonly SamplingKind Total = new("total", CapabilitySet<SamplingCapability>.Of(SamplingCapability.Interval), static (left, leftSeconds, right, rightSeconds) => left + right);
 public static readonly SamplingKind Cumulative = new("cumulative", CapabilitySet<SamplingCapability>.Of(SamplingCapability.Monotone), static (left, leftSeconds, right, rightSeconds) => right);
 public static readonly SamplingKind Minimum = new("minimum", CapabilitySet<SamplingCapability>.Of(SamplingCapability.Interval), static (left, leftSeconds, right, rightSeconds) => Math.Min(left, right));
 public static readonly SamplingKind Maximum = new("maximum", CapabilitySet<SamplingCapability>.Of(SamplingCapability.Interval), static (left, leftSeconds, right, rightSeconds) => Math.Max(left, right));

 public CapabilitySet<SamplingCapability> Capabilities { get; }

 [UseDelegateFromConstructor] public partial double Combine(double left, double leftSeconds, double right, double rightSeconds);

 public Option<double> Downsample(Seq<(double Si, Duration Span)> samples) =>
  samples.Head.Map(head => samples.Tail
   .Fold((Si: head.Si, Seconds: head.Span.TotalSeconds), (state, next) =>
    (Combine(state.Si, state.Seconds, next.Si, next.Span.TotalSeconds), state.Seconds + next.Span.TotalSeconds))
   .Si);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SamplingCapability : ICapability<SamplingCapability> {
 public static readonly SamplingCapability Interval = new("interval");
 public static readonly SamplingCapability Monotone = new("monotone");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradeCapability : ICapability<GradeCapability> {
 public static readonly GradeCapability Consumable = new("consumable");
}

[SmartEnum<string>]
public sealed partial class ObservationGrade {
 public static readonly ObservationGrade Measured = new("measured", CapabilitySet<GradeCapability>.Of(GradeCapability.Consumable));
 public static readonly ObservationGrade Validated = new("validated", CapabilitySet<GradeCapability>.Of(GradeCapability.Consumable));
 public static readonly ObservationGrade Substituted = new("substituted", CapabilitySet<GradeCapability>.Of(GradeCapability.Consumable));
 public static readonly ObservationGrade Suspect = new("suspect", CapabilitySet<GradeCapability>.None);
 public static readonly ObservationGrade Missing = new("missing", CapabilitySet<GradeCapability>.None);

 public CapabilitySet<GradeCapability> Capabilities { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly partial record struct ObservationChunk(Interval Window, ArtifactContent Series, int SampleCount) {
 private const int RecordStride = (sizeof(long) * 2) + sizeof(int);

 public static Fin<(ObservationChunk Chunk, ReadOnlyMemory<byte> Bytes)> Encode(
  Seq<(Instant At, double Si, ObservationGrade Grade)> samples, Op key) =>
  Accumulate(Seq(
    Gate(!samples.IsEmpty, key, "<observation-chunk-sampleless>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(!samples.Zip(samples.Tail).Exists(static pair => pair.Item2.At <= pair.Item1.At), key, "<observation-samples-not-advancing>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .ToFin()
   .Bind(_ => Written(samples, key));

 public Fin<Seq<(Instant At, double Si, ObservationGrade Grade)>> Decode(ReadOnlyMemory<byte> blob, Op key) =>
  Convert.ToHexStringLower(SHA256.HashData(blob.Span)) == Series.Sha256
   && checked((ulong)blob.Length) == Series.Bytes
   ? Read(blob, key)
   : new ElementFault.AddressUnstable(key, $"<observation-chunk-artifact-mismatch:{Series.Sha256}:{Series.Bytes}>");

 public bool IsBounded => Window.HasStart && Window.HasEnd;

 public bool Covers(Instant at) => Window.Contains(at);

 public bool Overlaps(Interval other) =>
  IsBounded && other.HasStart && other.HasEnd && Window.Start < other.End && other.Start < Window.End;

 private static Fin<(ObservationChunk Chunk, ReadOnlyMemory<byte> Bytes)> Written(
  Seq<(Instant At, double Si, ObservationGrade Grade)> samples, Op key) =>
  CanonicalWriter.Retaining(0d)
   .Rows(samples, static (sample, w) => w.I64(sample.At.ToUnixTimeTicks()).Double(sample.Si).String(sample.Grade.Key))
   .ToBytes(key)
   .Bind(bytes => ArtifactContent.Of(bytes, key).Map(reference => (new ObservationChunk(
    new Interval(samples[0].At, samples[samples.Count - 1].At), reference, samples.Count), bytes)));

 private static Fin<Seq<(Instant At, double Si, ObservationGrade Grade)>> Read(ReadOnlyMemory<byte> blob, Op key) {
  ReadOnlySpan<byte> span = blob.Span;
  if (span.Length < sizeof(int)) { return Truncated(key, "count-prefix"); }
  int declared = BinaryPrimitives.ReadInt32LittleEndian(span);
  int cursor = sizeof(int);
  if (declared < 0 || declared > (span.Length - sizeof(int)) / RecordStride) { return Truncated(key, "count-claim"); }
  (Instant, double, ObservationGrade)[] run = new (Instant, double, ObservationGrade)[declared];
  for (int index = 0; index < declared; index++) {
   if (cursor + RecordStride > span.Length) { return Truncated(key, "record"); }
   long ticks = BinaryPrimitives.ReadInt64LittleEndian(span[cursor..]);
   double si = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(span[(cursor + sizeof(long))..]));
   int length = BinaryPrimitives.ReadInt32LittleEndian(span[(cursor + (sizeof(long) * 2))..]);
   cursor += RecordStride;
   if (length < 0 || cursor + length > span.Length) { return Truncated(key, "token"); }
   string token = Encoding.UTF8.GetString(span.Slice(cursor, length));
   cursor += length;
   if (!ObservationGrade.TryGet(token, out ObservationGrade? grade) || grade is not { } admitted) {
    return new KernelFault.InvalidValue("observation-grade", $"resolve {token}", Some(key));
   }
   run[index] = (Instant.FromUnixTimeTicks(ticks), si, admitted);
  }
  return cursor == span.Length ? Fin.Succ(toSeq(run)) : Truncated(key, "tail");
 }

 private static Fin<Seq<(Instant At, double Si, ObservationGrade Grade)>> Truncated(Op key, string stage) =>
  new KernelFault.InvalidValue("observation-chunk", $"contain a complete {stage}", Some(key));
}

public readonly record struct SensorProvenance(
 string Manufacturer, string Model, string Serial,
 Option<LocalDate> CalibratedAt = default, Option<MeasureBand> Tolerance = default);

[Equatable]
public sealed partial record ObservationSeries {
 public SensorId Sensor { get; }
 public PropertyName Aspect { get; }
 public QuantitySignature Quantity { get; }
 public SamplingKind Sampling { get; }
 public Option<Duration> Cadence { get; }
 public Interval Window { get; }
 [OrderedEquality] public Seq<ObservationChunk> Chunks { get; }
 public SeriesStatistics Statistics { get; }
 public Option<SensorProvenance> Provenance { get; }

 public QuantityType Observed => Quantity.Type;
 public Dimension Signature => Quantity.Dimension;
 public string CanonicalUnit => Quantity.CanonicalUnit.IfNone(string.Empty);

 private ObservationSeries(
  SensorId sensor, PropertyName aspect, QuantitySignature quantity, SamplingKind sampling,
  Option<Duration> cadence, Interval window, Seq<ObservationChunk> chunks,
  SeriesStatistics statistics, Option<SensorProvenance> provenance) =>
  (Sensor, Aspect, Quantity, Sampling, Cadence, Window, Chunks, Statistics, Provenance) =
   (sensor, aspect, quantity, sampling, cadence, window, chunks, statistics, provenance);

 public static Fin<ObservationSeries> Open(
  SensorId sensor, PropertyName aspect, QuantitySignature quantity, SamplingKind sampling,
  Option<Duration> cadence, Instant start, Option<SensorProvenance> provenance, Op key) =>
  Accumulate(Seq(Named(quantity, key), Cadenced(cadence, key)))
   .Map(_ => new ObservationSeries(
    sensor, aspect, quantity, sampling, cadence,
    new Interval(start, start), Seq<ObservationChunk>(), SeriesStatistics.Empty, provenance))
   .ToFin();

 public Fin<ObservationSeries> Append(ObservationChunk chunk, SeriesStatistics statistics, Op key) =>
  Accumulate(Seq(
    Lawful(chunk, key),
    Gate(!chunk.IsBounded || chunk.Window.Start >= Window.End, key, $"<observation-chunk-overlaps:{Sensor.Value}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Summarized(statistics, SampleCount + chunk.SampleCount, key)))
   .Map(_ => new ObservationSeries(
    Sensor, Aspect, Quantity, Sampling, Cadence,
    new Interval(Window.Start, chunk.Window.End), Chunks.Add(chunk), statistics, Provenance))
   .ToFin();

 public static Fin<ObservationSeries> Rehydrate(
  SensorId sensor, PropertyName aspect, QuantitySignature quantity, SamplingKind sampling,
  Option<Duration> cadence, Interval window, Seq<ObservationChunk> chunks,
  SeriesStatistics statistics, Option<SensorProvenance> provenance, Op key) =>
  Accumulate(Seq(
    Named(quantity, key), Cadenced(cadence, key),
    Bounded(window, key).Map(static _ => unit),
    Accumulate(chunks.Map(chunk => Lawful(chunk, key))),
    Gate(!chunks.Zip(chunks.Tail).Exists(static pair => pair.Item1.IsBounded && pair.Item2.IsBounded && pair.Item2.Window.Start < pair.Item1.Window.End), key, "<observation-chunks-not-advancing>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(Spans(window, chunks), key, "<observation-window-does-not-span-chunks>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(Brackets(window, chunks), key, "<observation-window-does-not-bracket-run>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Summarized(statistics, chunks.Fold(0, static (total, chunk) => total + chunk.SampleCount), key),
    Gate(!chunks.IsEmpty || statistics == SeriesStatistics.Empty, key, "<observation-empty-series-summarized>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => new ObservationSeries(
    sensor, aspect, quantity, sampling, cadence,
    window, chunks, statistics, provenance))
   .ToFin();

 // --- [FETCH_READS] --------------------------------------------------------------------
 public Option<ObservationChunk> ChunkAt(Instant at) => Chunks.Find(chunk => chunk.Covers(at));

 public Fin<Seq<ObservationChunk>> ChunksIn(Interval window, Op key) =>
  Bounded(window, key).Map(admitted => Chunks.Filter(chunk => chunk.Overlaps(admitted))).ToFin();

 public int SampleCount => Chunks.Fold(0, static (total, chunk) => total + chunk.SampleCount);

 public Option<int> Expected(Interval window) =>
  window.HasStart && window.HasEnd
   ? Cadence.Map(cadence =>
     (int)(window.Duration.ToInt128Nanoseconds() / cadence.ToInt128Nanoseconds()) + (Sampling.Capabilities.Admits(SamplingCapability.Interval) ? 0 : 1))
   : None;

 public Fin<MeasureValue> Value(double si, Op key) =>
  MeasureValue.OfSi(Quantity.Type, Quantity.Dimension, si, Some(UnitProvenance.Carried(Quantity.CanonicalUnit))).Bind(measure =>
   Provenance.Bind(static audit => audit.Tolerance).Match(
    Some: tolerance => MeasureBand
     .Admit(tolerance.Kind, si + tolerance.LowerSi, si + tolerance.UpperSi, tolerance.StandardDeviationSi, tolerance.CoverageFactor, key)
     .Bind(band => measure.WithUncertainty(band, key)),
    None: () => Fin.Succ(measure)));

 public void CanonicalBytes(CanonicalWriter w) {
  w.String(Sensor.Value).String(Aspect.Value);
  Quantity.CanonicalBytes(w);
  w.String(Sampling.Key)
   .Optional(Cadence, static (cadence, writer) => writer.I64(cadence.BclCompatibleTicks))
   .I64(Window.Start.ToUnixTimeTicks());
 }

 // --- [ADMISSION_GATES] ----------------------------------------------------------------
 private static Validation<Error, Unit> Named(QuantitySignature quantity, Op key) =>
  Gate(quantity.CanonicalUnit.Exists(static unit => !string.IsNullOrWhiteSpace(unit)), "observation-unit", key,
   static (label, op) => (Error)new KernelFault.InvalidValue(label, "carry a non-blank canonical unit", Some(op)));

 private static Validation<Error, Unit> Cadenced(Option<Duration> cadence, Op key) =>
  cadence.Match(
   Some: span => In(span.TotalSeconds, Band.Positive, "observation-cadence-seconds", key).Map(static _ => unit),
   None: () => Success<Error, Unit>(unit));

 private static Validation<Error, Unit> Lawful(ObservationChunk chunk, Op key) =>
  Accumulate(Seq(
   Gate(chunk.IsBounded, key, "<observation-chunk-unbounded>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(!chunk.IsBounded || chunk.Window.End > chunk.Window.Start || chunk.SampleCount == 1, key, "<observation-chunk-window-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(chunk.SampleCount > 0, key, "<observation-chunk-sampleless>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))));

 private static Validation<Error, Unit> Summarized(SeriesStatistics statistics, int run, Op key) =>
  Accumulate(Seq(
   Gate(statistics.IsCoherent, key, "<observation-statistics-incoherent>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(statistics.Observed == run, key, "<observation-census-not-run-total>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))));

 private static bool Spans(Interval window, Seq<ObservationChunk> chunks) =>
  !window.HasStart || !window.HasEnd
  || chunks.ForAll(chunk => !chunk.IsBounded || (chunk.Window.Start >= window.Start && chunk.Window.End <= window.End));

 private static bool Brackets(Interval window, Seq<ObservationChunk> chunks) =>
  !window.HasStart || !window.HasEnd
  || (chunks.IsEmpty
   ? window.End == window.Start
   : !chunks[chunks.Count - 1].IsBounded || chunks[chunks.Count - 1].Window.End == window.End);
}
```

## [03]-[SERIES_STATISTICS]

- Owner: `SeriesStatistics` the DERIVED per-series summary the node carries so a completeness screen, a range bound, and the comparison figure read flat off the graph — the graded `Census`, the covered `Span`, the `Properties/quantity#MEASURE_STAT` `MeasureStat` as `Figures` (minimum, maximum, duration-weighted mean, and the variance/skewness/kurtosis/RMS moments beside them, one fold), and the `Total` aggregate as a typed `MeasureValue`.
- Entry: `SeriesStatistics.From(run, sampling, quantity, key)` derives the whole summary off one decoded sample run through ONE weighted `MeasureStat.Of` fold — the ONE owner of the register-span rule and of the monotone-run refusal that rule presupposes; `Of(census, span, figures, total, key)` takes an already-composed column set RAILED on the same `IsCoherent` law the series transitions re-impose; `Fold(left, right, key)` merges two ADJACENT summaries — `Figures` through the kernel `Stat.Merge` Pebay join, `Total` additively, the census key-wise, the span as covered duration — the ONE merge every rollup composes; `Minimum`/`Maximum`/`Mean` are one-hop reads over `Figures`, so every pre-kernel consumer column survives verbatim; `Observed` sums the census and `Consumable` folds it over the `GradeCapability.Consumable` axis; `Completeness(expected)` reads the consumable share against a cadence-derived denominator (`None` where the denominator is unanswerable); `Representative(sampling, key)` reads the ONE comparable figure the sampling row designates — the mean for an averaged or instantaneous stream, the total for a totalled or cumulative one, the extremum for a min or max one.
- Auto: the four hand-spelled column traversals COLLAPSED into the kernel fold — `MeasureStat.Of` runs one weighted Welford pass whose weights are each sample's span to its successor (the final sample carrying the run's mean gap), so the weighted mean IS the duration-weighted figure and the extremes ride the same pass; `Fold` needs no per-column arithmetic because the kernel Pebay join is mass-weighted over those same duration weights, and both `Total` algebras ADD across adjacent windows (sums add, register spans over touching windows telescope); `Span` merges as COVERED duration — the time the samples span, never end-minus-start across the pair — so a gap between chunks never inflates the completeness denominator's partner; the census is a `Map<ObservationGrade, int>` keyed by the closed grade vocabulary, so a new grade row lands with zero census-shape edit and the consumable fold derives from the row's own capability set rather than a hand-maintained allowed set; `IsCoherent` states only what construction cannot already hold — non-negative counts, non-negative span, one signature across `Figures` and `Total` — because `MeasureStat` holds `min <= mean <= max` structurally and `MeasureStat`'s own admission proves member-signature agreement, so the bracket clause and the per-column quantity walk the pre-kernel shape needed both died with it.
- Output: `series.Statistics` is the pre-decode screen every consumer reads first — `Completeness` gates whether a verdict may be drawn at all, `Minimum`/`Maximum` bound a plausibility check, and `Representative` is the single figure a `Rasm.Compute` commissioning route subtracts against the matching `Assessment/assessment#ASSESSMENT_NODE` `ResultMeasure`, so the declared-versus-metered comparison reads two flat values off one baked element and the verdict writes back as an `Assessment` node whose `DependsOn` names the series.
- Packages: `Properties/quantity#MEASURE_STAT` (`MeasureStat`/`QuantitySignature` the figures carrier and its signature gate, the `UnitProvenance.Carried` trusted re-mint the `Total` column stamps through), `Rasm` (the kernel `Stat<Scalar>` Welford/Pebay engine under `MeasureStat`, `CapabilitySet` the consumable fold reads), Thinktecture.Runtime.Extensions (the `ObservationGrade`/`SamplingKind` generated `Key`/`Items`/`Switch` surfaces the census and the representative dispatch read), LanguageExt.Core (`Map`/`Option`/`Fin`/`Seq` the carriers and folds), NodaTime (`Duration` the covered span).
- Growth: a new summary aggregate is a kernel `Stat` read surfaced as one one-hop projection over `Figures`, never a stored column with its own derivation, merge arm, and coherence clause; a new grade is one `ObservationGrade` row the census absorbs with no shape edit; a new representative rule is one `SamplingKind` row the total `Switch` already forces to answer; never a per-quantity statistics type and never a second summary beside this one.
- Boundary: `SeriesStatistics` carries ZERO authority (`libs/.planning/RULINGS.md` truth-plane law) — the `ObservationChunk` bytes are the truth plane, this summary is an accelerator that DROPS at warm-up cost and rebuilds by re-reading the referenced blobs, and its retention floor is exactly the chunk retention; it is canon-EXCLUDED from `ObservationSeries.CanonicalBytes`, so a recomputed summary never forks the node id and a summary disagreeing with its blobs repairs by rebuild rather than by trusting the stored figure; a consumer reading `Mean` where the raw distribution decides — a percentile, a duty-cycle histogram, a spectral fold — reads the chunk bytes instead, because the mean is the accelerator and the samples are the record; `Representative` is a SEAM projection of the sampling semantics the seam owns, never a verdict — the comparison, the tolerance band, and the pass rule are `Rasm.Compute`'s; `From` owns the register-span rule so no producer re-derives it — a `Cumulative` run's `Total` is its last reading less its first, the consumption across the window rather than the odometer; `Of` survives for a producer that already holds the columns, RAILED so a decoded summary re-proves against itself, and the run stays the caller's to supply because this page addresses bytes and never fetches them.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SeriesStatistics(
 Map<ObservationGrade, int> Census, Duration Span, Option<MeasureStat> Figures, Option<MeasureValue> Total) {

 public static readonly SeriesStatistics Empty = new(Map<ObservationGrade, int>(), Duration.Zero, None, None);

 public static Fin<SeriesStatistics> Of(
  Map<ObservationGrade, int> census, Duration span, Option<MeasureStat> figures, Option<MeasureValue> total, Op key) =>
  new SeriesStatistics(census, span, figures, total) is { IsCoherent: true } admitted
   ? Fin.Succ(admitted)
   : new ElementFault.ValueRejected(key, "<observation-statistics-incoherent>");

 public static Fin<SeriesStatistics> From(
  Seq<(Instant At, double Si, ObservationGrade Grade)> run, SamplingKind sampling, QuantitySignature quantity, Op key) {
  if (run.IsEmpty) { return Fin.Succ(Empty); }
  if (sampling.Capabilities.Admits(SamplingCapability.Monotone) && run.Zip(run.Tail).Exists(static pair => pair.Item2.Si < pair.Item1.Si)) {
   return new ElementFault.ValueRejected(key, "<observation-register-decreasing>");
  }
  Duration span = run[run.Count - 1].At - run[0].At;
  Map<ObservationGrade, int> census = run.Fold(Map<ObservationGrade, int>(),
   static (map, sample) => map.AddOrUpdate(sample.Grade, static count => count + 1, 1));
  return MeasureStat.Of(quantity, run.Map(static sample => sample.Si), key,
    Some(Weighted(run, span).Map(static sample => sample.Span.TotalSeconds)))
   .Bind(figures => Mint(quantity,
     sampling.Capabilities.Admits(SamplingCapability.Monotone)
      ? run[run.Count - 1].Si - run[0].Si
      : run.Fold(0d, static (sum, sample) => sum + sample.Si))
    .Map(total => new SeriesStatistics(census, span, Some(figures), Some(total))));
 }

 private static Seq<(double Si, Duration Span)> Weighted(
  Seq<(Instant At, double Si, ObservationGrade Grade)> run, Duration span) {
  Duration tail = run.Count > 1 ? span / (run.Count - 1) : Duration.FromSeconds(1);
  return run.Map((sample, index) =>
   (sample.Si, index + 1 < run.Count ? run[index + 1].At - sample.At : tail));
 }

 private static Fin<MeasureValue> Mint(QuantitySignature quantity, double si) =>
  MeasureValue.OfSi(quantity.Type, quantity.Dimension, si, Some(UnitProvenance.Carried(quantity.CanonicalUnit)));

 public Option<MeasureValue> Minimum => Figures.Map(static figures => figures.Minimum);
 public Option<MeasureValue> Maximum => Figures.Map(static figures => figures.Maximum);
 public Option<MeasureValue> Mean => Figures.Map(static figures => figures.Mean);

 public int Observed => Census.Fold(0, static (sum, _, count) => sum + count);

 public int Consumable => Census.Fold(0, static (sum, grade, count) => sum + (grade.Capabilities.Admits(GradeCapability.Consumable) ? count : 0));

 public Option<double> Completeness(Option<int> expected) =>
  expected.Filter(static count => count > 0).Map(count => Consumable / (double)count);

 public bool IsCoherent =>
  Census.ForAll(static (_, count) => count >= 0)
  && Span >= Duration.Zero
  && (Figures.Case, Total.Case) switch {
   (MeasureStat figures, MeasureValue total) => figures.Signature == QuantitySignature.Of(total),
   _ => true,
  };

 public static Fin<SeriesStatistics> Fold(SeriesStatistics left, SeriesStatistics right, Op key) =>
  Joined(left.Figures, right.Figures, key).Bind(figures =>
   Added(left.Total, right.Total).Map(total =>
    new SeriesStatistics(
     right.Census.Fold(left.Census, static (census, grade, count) => census.AddOrUpdate(grade, existing => existing + count, count)),
     left.Span + right.Span,
     figures, total)));

 public Fin<MeasureValue> Representative(SamplingKind sampling, Op key) =>
  sampling.Switch(
   instantaneous: () => Figure(Mean, key, "mean"),
   averaged: () => Figure(Mean, key, "mean"),
   total: () => Figure(Total, key, "total"),
   cumulative: () => Figure(Total, key, "register-span"),
   minimum: () => Figure(Minimum, key, "minimum"),
   maximum: () => Figure(Maximum, key, "maximum"));

 // --- [MERGE_PRIMITIVES] ---------------------------------------------------------------
 private static Fin<Option<MeasureStat>> Joined(Option<MeasureStat> left, Option<MeasureStat> right, Op key) =>
  (left.Case, right.Case) switch {
   (MeasureStat a, MeasureStat b) => MeasureStat.Merge(a, b, key).Map(Some),
   (MeasureStat, _) => Fin.Succ(left),
   _ => Fin.Succ(right),
  };

 private static Fin<Option<MeasureValue>> Added(Option<MeasureValue> left, Option<MeasureValue> right) =>
  (left.Case, right.Case) switch {
   (MeasureValue a, MeasureValue b) => Mint(QuantitySignature.Of(a), a.Si + b.Si).Map(Some),
   (MeasureValue, _) => Fin.Succ(left),
   _ => Fin.Succ(right),
  };

 private static Fin<MeasureValue> Figure(Option<MeasureValue> value, Op key, string column) =>
  value.Match(Some: Fin.Succ, None: () => new ElementFault.ValueRejected(key, $"<observation-representative-absent:{column}>"));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
