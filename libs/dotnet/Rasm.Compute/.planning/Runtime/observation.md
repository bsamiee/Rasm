# [COMPUTE_OBSERVATION]

Rasm.Compute owns the durable half of the sensor wire: `ObservationLane` accumulates the decoded stream per binding and flushes each closed window through the `Rasm.Element` `Assessment/observation#OBSERVATION_SERIES` production chain into content-keyed chunks and one `GraphDelta` carrying the `Node.Observation` and its occurrence `Assign` edge. One identity regime holds the page — a metered stream becoming graph evidence — and its whole vocabulary is the Element contract's; this is the only Compute page that reaches `Node`/`GraphDelta`.

`Runtime/ingest` owns the broker boundary and the `CaptureAdmission` fan whose second leg reaches this lane; `Runtime/channels` owns the gRPC mechanics neither leg touches. The contract arrives settled — the `Open`/`Encode`/`From`/`Fold`/`Append` production chain, the `SamplingKind` temporal algebra, the `QuantitySignature` admitted triple, the chunk blob codec and its `BlobKey` addressing, and the `SeriesStatistics` adjacent merge are contract-owned — so this page holds accumulation policy, binding custody, and the graph landing alone. Package spine: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, CloudNative.CloudEvents, Rasm.Element, and Rasm.

## [01]-[INDEX]

- [02]-[OBSERVATION_LANE]: the `SensorQuality` peer-flag map, the `SensorBinding` custody row over one admitted `QuantitySignature`, the flush/window/ceiling/tolerance policy, the per-binding accumulation state with its hand-off claim, the typed `ObservationSink` ports, and the one validated cell every run lives in.

## [02]-[OBSERVATION_LANE]

- Owner: `SensorQuality` `[SmartEnum<string>]` the peer-flag row map projecting each publisher quality token onto its `Rasm.Element` `ObservationGrade`, read by name off the envelope's own populated set because the solution roster deliberately declares no vendor grading attribute; `SensorBinding` the per-stream custody row binding one deployed sensor to one observed aspect of one occupied OCCURRENCE beside the admitted quantity signature, sampling algebra, nominal cadence, and optional instrument audit every `Open` needs; `ObservationPolicy` the flush-cadence, window-bound, pending-ceiling, model-tolerance, and flush-retry policy row; `ObservationRun` the per-binding accumulation state carrying the open series, the pending window, the claimed hand-off slot, and the shed tally; `ObservationSink` the two typed durable ports — the `BlobKey`-keyed content write and the delta landing — as ONE contract value; `ObservationLane` the boundary owner holding the admitted binding roster, the policy, the quality attribute, the sink, and the ONE validated cell every run lives in.
- Law: the binding roster is admitted ONCE, at `ObservationLane.Of`. The contract's own `Open` gates a named canonical unit and a positive cadence, so proving every roster row at composition turns the per-sample `Open` from a live gate into a re-check whose refusal arm is unreachable, and a composition seating a blank-unit signature fails at boot rather than on the first delivery from that stream. NAMED LOSS: `Of` now fails, so a composition root binds `Fin<ObservationLane>`. Witness: an unadmitted roster row surfaced only when its sensor first published, which on a commissioning stream can be weeks.
- Law: the effectful flush runs OUTSIDE the cell. `Atom.Swap` re-runs its function on every losing CAS attempt, so an encode, a store write, or a contract `Append` inside it repeats per attempt and a losing attempt's write outlives the value it was computed for; the transition therefore CLOSES the window by moving it into `Claimed` and installs that value, the caller reads the claim off the returned state, runs the effect, and commits through a second swap that touches only the series and the claim slot — a delivery that arrived mid-flight is already in `Pending` and survives, where installing the flushed run whole would drop it.
- Law: the in-flight re-drive is a `Schedule` VALUE, never a loop. `ObservationPolicy.Retry` composes the backoff curve and its cumulative budget, `IO.Retry` drives it, and exhaustion returns a TYPED fault naming the binding and the window rather than a success-shaped fall-through. The standing claim is the outer re-drive and stays: an exhausted flush leaves the window claimed so the next delivery on that binding re-drives it, and `PendingCap` bounds how far the backlog grows meanwhile. NAMED LOSS: a binding that goes quiet after an exhausted flush still strands its window until the ceiling sheds — the two re-drives are in-flight and delivery-driven, and neither is a timer this lane owns.
- Law: chunk emission is ORDERED-AWAIT. One binding's flush is a single awaited chain under its own claim, so the appended run and the landed delta advance in window order and the contract's strict-adjacency gate never sees a later window arrive first; a parallel fan over one binding's windows is the deleted form the folder ruling names.
- Entry: `ObservationLane.Of(bindings, policy, sink)` mints the lane and its cell together, admitting every roster row and declaring the pending ceiling once; `Admit(SensorReading<TwinSignal> reading)` is the ONE typed entry — resolve the binding off the wire signal id, grade the reading off its own peer quality flag, accumulate, and flush whatever closed window the swap hands back — returning `IO<Fin<Unit>>` so a refusal is data the `Runtime/ingest` fan parks rather than a throw the drain absorbs.
- Auto: `ObservationSeries.CanonicalBytes` folds the STREAM identity alone — sensor, aspect, the `QuantitySignature`'s own canonical projection, sampling key, cadence, and `Window.Start` — excluding the chunk run, the advancing `Window.End`, the derived statistics, and the provenance, so every flush re-addresses the SAME `NodeId` and a later flush is a same-id revision the `PutNode` upsert lands rather than a fresh node per chunk; the `Assign` edge therefore lands exactly once, on the flush whose PRE-append run still carries no chunk, so no `linked` flag survives to drift from the run it describes. The whole-run summary the `Append` gate demands is reached by folding the carried `Series.Statistics` with the window's own `SeriesStatistics.From` through the contract's designated adjacent merge, so the lane never re-fetches a stored blob to re-derive a figure the node already carries and never keeps the whole run in memory to recompute it.
- Law: the landed `GraphDelta` IS the evidence — and a shed sample rides `ObservationRun.Shed` on the value the swap installs beside the `WorkLane.CaptureIngest` lane-pressure count the twin leg already carries, so the two refusal mechanisms stay partitioned exactly as the broker-redelivery and lane-shed pair does.
- Packages: LanguageExt.Core (`Atom`/`Prelude.Atom` with its validator, `SwapIO`, `Schedule`, `IO.Retry`, `HashMap`, `Seq`, `Option`, `Fin`, `Validation`), NodaTime (`Instant` the sample anchor, `Duration` the flush window and cadence), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` the quality row map), CloudNative.CloudEvents (`CloudEvent.GetPopulatedAttributes` the untyped peer-extension read), Rasm.Element (project — `ObservationSeries`/`ObservationChunk`/`SeriesStatistics`/`ObservationGrade`/`SamplingKind`/`SensorId`/`SensorProvenance`/`QuantitySignature`/`BlobKey`, `Node.Observation`/`NodeId`, `GraphDelta`/`Relationship.Assign`/`AssignKind`, `PropertyName`), Rasm (kernel — the `Op` op-key the contract API re-stamps refusals under)
- Growth: a new publisher quality token is one `SensorQuality` row; a new instrumented stream is one `SensorBinding` in the composition roster the boot admission then proves; a new flush edge is one `ObservationPolicy` column read by `Closed`; a new backoff shape is a `Schedule` composition at the policy mint, never a member here; a new metering algebra, sample column, or summary column lands wholly at the contract and reaches this lane with no edit; never a lane-local grade enum, never a per-quantity binding type, never a second store. HDF5 is REFUSED as an observation container by shape: this lane is APPEND-shaped — an open series accumulates and flushes forever — while the archive owner is create-only with no append, no in-place edit, and no re-open-for-write, so a chunked HDF5 series here would re-encode the whole run per flush or violate the write-once law; the content-keyed chunk-and-delta hand-off IS the accumulating form, and the recorded negative closes the question.
- Boundary: `Rasm.AppHost` is the S1 spine and cannot reference the Element contract, so the sensor-series PRODUCER seats here — the AppHost livewire stays the transport coercing a BMS reading to canonical SI and this lane turns that coerced stream into durable graph evidence; a producer minted at the spine is unreachable by construction. The binding roster is a COMPOSITION value, never derived from an envelope body: a binding read off the wire lets a publisher name the occurrence it reports against and write into any element's evidence. The lane ADDRESSES bytes and never fetches them — `ObservationChunk.Encode` mints the block and its `BlobKey` from ONE projection, so the sink WRITES under the key the chunk already carries and a store returning its own key is the second hasher the contract's one seed forecloses; the port is therefore key-TAKING and a key-minting twin anywhere in this package is its deleted inverse. The chunk's bytes cross once, at that write: the contract retired `Span` and `CanonicalBytes` on the chunk, so no reader here re-derives a payload the `Encode` pair already handed over. The occurrence is occurrence-scoped by the contract's own admission — `AssignKind.Observation` refuses a Type subject because a `Component` names no instrument — so a binding pointing at a Type fails at `AdmitOnto` rather than minting a series the named-type fold would skip. `Rasm.Element` owns the sampling algebra, the chunk codec, the statistics derivation, and every admission gate; this lane holds accumulation policy, binding custody, and the delta hand-off, so a lane-local downsample, a lane-local completeness screen, or a lane-computed representative figure is the deleted form. The reading's finite-magnitude gate is this lane's own and NOT the twin's `TwinSignal.Invalid` predicate, which additionally demands a non-empty `OperatingPoint` — a surrogate-scoring need, not a metering one — so a shared predicate would refuse honest readings the twin has no use for. `ObservationGrade.Missing` is not publisher-reachable: it marks a cadence slot nothing arrived for, which only a gap-filling pass mints, so no `SensorQuality` row projects onto it. Absence in the instrument audit rides the `Option` the contract declares, so a blank-string sentinel provenance is unspellable here.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensorQuality {
    public static readonly SensorQuality Good = new("good", grade: ObservationGrade.Measured);
    public static readonly SensorQuality Validated = new("validated", grade: ObservationGrade.Validated);
    public static readonly SensorQuality Estimated = new("estimated", grade: ObservationGrade.Substituted);
    public static readonly SensorQuality Uncertain = new("uncertain", grade: ObservationGrade.Suspect);
    public static readonly SensorQuality Bad = new("bad", grade: ObservationGrade.Suspect);
    public static readonly SensorQuality Stale = new("stale", grade: ObservationGrade.Suspect);

    public ObservationGrade Grade { get; }

    public static ObservationGrade Of(Option<string> token) =>
        token.Match(
            Some: static flag => TryGet(flag, out SensorQuality? row) && row is { } quality ? quality.Grade : ObservationGrade.Suspect,
            None: static () => ObservationGrade.Measured);
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record ObservationPolicy(
    int FlushSamples, Duration FlushWindow, int PendingCap, double Tolerance, Schedule Retry) {
    public static readonly ObservationPolicy Canonical = new(
        FlushSamples: 512,
        FlushWindow: Duration.FromMinutes(15),
        PendingCap: 4096,
        Tolerance: EpsilonPolicy.ZeroTolerance,
        Retry: Schedule.exponential(TimeSpan.FromSeconds(1)) | Schedule.maxCumulativeDelay(TimeSpan.FromMinutes(2)));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SensorBinding(
    NodeId Occurrence, SensorId Sensor, PropertyName Aspect,
    QuantitySignature Quantity, SamplingKind Sampling, Option<Duration> Cadence,
    Option<SensorProvenance> Provenance);

public readonly record struct ObservationRun(
    ObservationSeries Series,
    Seq<(Instant At, double Si, ObservationGrade Grade)> Pending,
    Seq<(Instant At, double Si, ObservationGrade Grade)> Claimed,
    int Shed) {

    private static readonly Seq<(Instant At, double Si, ObservationGrade Grade)> Drained = Seq<(Instant At, double Si, ObservationGrade Grade)>();

    public static ObservationRun Opened(ObservationSeries series, (Instant At, double Si, ObservationGrade Grade) sample) =>
        new(series, Seq(sample), Drained, 0);

    public static ObservationRun Landed(ObservationSeries series) => new(series, Drained, Drained, 0);

    public ObservationRun Absorb((Instant At, double Si, ObservationGrade Grade) sample, ObservationPolicy policy) =>
        Pending.Count >= policy.PendingCap
            ? this with { Shed = Shed + 1 }
            : (this with { Pending = Pending.Add(sample) }).Closed(policy);

    private ObservationRun Closed(ObservationPolicy policy) =>
        Claimed.IsEmpty && !Pending.IsEmpty
        && (Pending.Count >= policy.FlushSamples
            || Pending[Pending.Count - 1].At - Pending[0].At >= policy.FlushWindow)
            ? this with { Claimed = Pending, Pending = Drained }
            : this;

    public ObservationRun Committed(ObservationSeries grown) => this with { Series = grown, Claimed = Drained };
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed record ObservationSink(
    Func<ArtifactContent, ReadOnlyMemory<byte>, Fin<Unit>> Blob,
    Func<GraphDelta, Fin<Unit>> Land);

public sealed record ObservationLane(
    HashMap<string, SensorBinding> Bindings,
    ObservationPolicy Policy,
    ObservationSink Sink,
    Atom<HashMap<string, ObservationRun>> Runs) {

    private static readonly Op Key = Op.Of(name: nameof(ObservationLane));

    private const string QualityName = "sensorquality";

    public static Fin<ObservationLane> Of(
        HashMap<string, SensorBinding> bindings, ObservationPolicy policy, ObservationSink sink) =>
        bindings.Values.Traverse(Admissible).As()
            .Map(_ => new ObservationLane(bindings, policy, sink,
                Atom(HashMap<string, ObservationRun>(), runs => runs.ForAll(pair => pair.Value.Pending.Count <= policy.PendingCap))))
            .ToFin();

    private static Validation<Error, Unit> Admissible(SensorBinding binding) =>
        Series(binding, Instant.MinValue).Map(static _ => unit).ToValidation();

    public IO<Fin<Unit>> Admit(SensorReading<TwinSignal> reading) =>
        Bindings.Find(reading.Data.SignalId).Match(
            None: () => IO.pure(Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))),
            Some: binding => Sample(reading, binding)
                .Bind(sample => Series(binding, sample.At).Map(opened => (Sample: sample, Opened: opened)))
                .Match(
                    Succ: seed => Runs
                        .SwapIO(runs => runs.AddOrUpdate(
                            reading.Data.SignalId,
                            run => run.Absorb(seed.Sample, Policy),
                            ObservationRun.Opened(seed.Opened, seed.Sample)))
                        .Bind(runs => runs.Find(reading.Data.SignalId).Match(
                            Some: run => run.Claimed.IsEmpty ? IO.pure(Fin.Succ(unit)) : Flush(binding, reading.Data.SignalId, run),
                            None: static () => IO.pure(Fin.Succ(unit)))),
                    Fail: error => IO.pure(Fin.Fail<Unit>(error))));

    private IO<Fin<Unit>> Flush(SensorBinding binding, string signal, ObservationRun run) =>
        IO.lift(() => Produced(binding, run))
            .Retry(Policy.Retry)
            .Try().Run()
            .Map(outcome => outcome.MapFail(error => (Error)error))
            .Bind(grown => grown.Match(
                Succ: series => Runs
                    .SwapIO(runs => runs.AddOrUpdate(signal, held => held.Committed(series), ObservationRun.Landed(series)))
                    .Map(static _ => Fin.Succ(unit)),
                Fail: error => IO.pure(Fin.Fail<Unit>(error))));

    private Fin<ObservationSeries> Produced(SensorBinding binding, ObservationRun run) =>
        ObservationChunk.Encode(run.Claimed, Key).Bind(block =>
            Sink.Blob(block.Chunk.Series, block.Bytes)
                .Bind(_ => SeriesStatistics.From(run.Claimed, run.Series.Sampling, run.Series.Quantity, Key))
                .Bind(window => SeriesStatistics.Fold(run.Series.Statistics, window, Key))
                .Bind(whole => run.Series.Append(block.Chunk, whole, Key))
                .Bind(grown => Sink.Land(Delta(binding, run.Series, grown)).Map(_ => grown)));

    private GraphDelta Delta(SensorBinding binding, ObservationSeries opened, ObservationSeries grown) {
        NodeId id = NodeId.Of(new NodeSeed.Content(new Node.Observation(NodeId.Of(new NodeSeed.Placement()), grown), Policy.Tolerance));
        GraphDelta delta = GraphDelta.Empty.Put(new Node.Observation(id, grown));
        return opened.Chunks.IsEmpty
            ? delta.Link(new Relationship.Assign(binding.Occurrence, id, AssignKind.Observation))
            : delta;
    }

    private static Fin<ObservationSeries> Series(SensorBinding binding, Instant start) =>
        ObservationSeries.Open(
            binding.Sensor, binding.Aspect, binding.Quantity, binding.Sampling,
            binding.Cadence, start, binding.Provenance, Key);

    private static Fin<(Instant At, double Si, ObservationGrade Grade)> Sample(SensorReading<TwinSignal> reading, SensorBinding binding) =>
        double.IsFinite(reading.Data.Measured)
            ? Fin.Succ((reading.Data.At, reading.Data.Measured, SensorQuality.Of(Flag(reading.Envelope))))
            : Fin.Fail<(Instant At, double Si, ObservationGrade Grade)>(
                new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(reading.Data.Measured))));

    private static Option<string> Flag(CloudEvent envelope) =>
        toSeq(envelope.GetPopulatedAttributes())
            .Find(static populated => StringComparer.Ordinal.Equals(populated.Key.Name, QualityName))
            .Map(static populated => populated.Key.Format(populated.Value));
}
```
