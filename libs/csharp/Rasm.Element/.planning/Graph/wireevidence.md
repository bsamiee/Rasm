# [ELEMENT_WIRE_EVIDENCE]

`WireCodec`'s evidence plane: the `AssessmentPayload` codec deriving the ONE `PayloadContent` value from the wire's flat results/diagnostic/blob columns and re-admitting through the owner's `Open` coherence gate, the `EvidenceRun` audit codec re-entering the owner's railed `Of`, `Diagnostic`, and the measured-observation family — series, chunks (`BlobKey`-addressed), `SensorProvenance` (absence rides the caller's traversal), and the `SeriesStatistics` codec whose kernel moment group crosses append-only (ledger row [15]) so the `Stat` receipt round-trips whole and re-proves its own validity at decode.

## [01]-[INDEX]

- [02]-[EVIDENCE_CODEC]: assessment, evidence-run, diagnostic, and observation-family codecs.

## [02]-[EVIDENCE_CODEC]

- Cases: `AssessmentWire`/`ObservationWire` flat payloads (no oneof — the `PayloadContent` union derives at the boundary); census exemption stated at `Graph/wire#WIRE_CODEC`.
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
using Rasm.Element.Assessment;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ---------------------------------------------------------------------------
// One partial part of the ONE `[Mapper]` WireCodec family — the attribute, the parity census, the key codecs, and
// the shared decode gates ride `Graph/wire#WIRE_CODEC`; this part owns the assessment and observation evidence transcriptions.
internal static partial class WireCodec {
 // Content is HAND-CROSSED whole: the wire keeps its flat results/diagnostic/blob columns (frozen fields) and the
 // ONE PayloadContent value fans onto them through the derived one-hop reads — the union never crosses as a case.
 [MapProperty(nameof(AssessmentPayload.DependsOn), nameof(AssessmentWire.DependsOnIds))]
 [MapperIgnoreSource(nameof(AssessmentPayload.Content))]
 [MapperIgnoreSource(nameof(AssessmentPayload.ResultBlob))]
 private static partial AssessmentWire Shell(AssessmentPayload payload);
 [UserMapping(Default = true)] internal static AssessmentWire ToWire(AssessmentPayload payload) {
  AssessmentWire w = Shell(payload); payload.ResultBlob.IfSome(k => w.ResultBlob = ToWire(k.Value)); return w;
 }

 [UserMapping] internal static ProvenanceWire ToWire(EvidenceRun p) {
  ProvenanceWire w = new() { Author = p.Author, Tool = p.Tool, Version = p.Version, At = p.At.ToTimestamp(), Elapsed = p.Elapsed.ToProtobufDuration(), Attempt = p.Attempt };
  p.Window.IfSome(i => { w.WindowStart = i.Start.ToTimestamp(); w.WindowEnd = i.End.ToTimestamp(); });
  // `CorrelationId` carries the kernel's own `ISpanFormattable` "D" render, so the wire text and the
  // `Guid.TryParse` decode below stay one round-trippable spelling.
  p.Correlation.IfSome(c => w.Correlation = c.ToString("D", CultureInfo.InvariantCulture));
  return w;
 }

 [UserMapping] internal static DiagnosticWire? ToWire(Option<Diagnostic> diagnostic) => diagnostic.Match<DiagnosticWire?>(
  static d => { DiagnosticWire w = new() { Phase = d.Phase.Key, Kind = d.Kind.Key, Message = d.Message }; d.Code.IfSome(c => w.Code = c); return w; },
  static () => null);

 // Hand-owned like ToWire(GeoReference): the Interval flattens to a bounded column PAIR and the census map keys on a
 // generated row, neither a shape Mapperly bridges. Both window ends are bounded by seam admission, so the columns
 // are unconditional and no presence flag stands in for an unbounded side.
 [UserMapping] internal static ObservationWire ToWire(ObservationSeries series) {
  ObservationWire w = new() {
   Sensor = series.Sensor.Value, Aspect = series.Aspect.Value, Observed = series.Observed.Value,
   DimLength = series.Signature.Length, DimMass = series.Signature.Mass, DimTime = series.Signature.Time,
   DimCurrent = series.Signature.Current, DimTemperature = series.Signature.Temperature,
   DimAmount = series.Signature.Amount, DimLuminousIntensity = series.Signature.LuminousIntensity,
   CanonicalUnit = series.CanonicalUnit, Sampling = series.Sampling.Key,
   WindowStart = series.Window.Start.ToTimestamp(), WindowEnd = series.Window.End.ToTimestamp(),
   Statistics = ToWire(series.Statistics),
  };
  series.Provenance.IfSome(audit => w.Provenance = ToWire(audit));
  series.Cadence.IfSome(cadence => w.Cadence = cadence.ToProtobufDuration());
  w.Chunks.AddRange(series.Chunks.Map(static chunk => new ObservationChunkWire {
   WindowStart = chunk.Window.Start.ToTimestamp(), WindowEnd = chunk.Window.End.ToTimestamp(),
   SeriesKey = ToWire(chunk.SeriesKey.Value), SampleCount = chunk.SampleCount,
  }));
  return w;
 }

 [UserMapping] internal static SensorProvenanceWire ToWire(SensorProvenance provenance) {
  SensorProvenanceWire w = new() { Manufacturer = provenance.Manufacturer, Model = provenance.Model, Serial = provenance.Serial };
  provenance.CalibratedAt.IfSome(date => w.CalibratedAt = NodaTime.Text.LocalDatePattern.Iso.Format(date)); provenance.Tolerance.IfSome(band => w.Tolerance = ToWire(band)); return w;
 }

 // The one-hop min/max/mean scalars stay peer-informative; the kernel moment group crosses beside them so the
 // receipt round-trips WHOLE (ledger row [15]).
 [UserMapping] internal static SeriesStatisticsWire ToWire(SeriesStatistics statistics) {
  SeriesStatisticsWire w = new() { Span = statistics.Span.ToProtobufDuration() };
  foreach ((ObservationGrade grade, int count) in statistics.Census) { w.Census[grade.Key] = count; }
  statistics.Minimum.IfSome(measure => w.Minimum = ToWire(measure)); statistics.Maximum.IfSome(measure => w.Maximum = ToWire(measure));
  statistics.Mean.IfSome(measure => w.Mean = ToWire(measure)); statistics.Total.IfSome(measure => w.Total = ToWire(measure));
  statistics.Figures.IfSome(figures => {
   w.StatCount = figures.Figures.Count; w.StatRejected = figures.Figures.Rejected; w.StatMass = figures.Figures.Mass;
   w.StatM2 = figures.Figures.M2; w.StatM3 = figures.Figures.M3; w.StatM4 = figures.Figures.M4;
  });
  return w;
 }

 static Fin<AssessmentPayload> ToAssessment(AssessmentWire w, Op key) =>
  from discipline in Discipline.Parse(w.Discipline, key)
  from route in AnalysisRoute.Of(w.Route, key)
  from outcome in key.Row<string, AssessmentOutcome>(w.Outcome)
  from results in ToValueMap(w.Results, key)
  from diagnostic in ToDiagnostic(w.Diagnostic, key)
  from content in ToContent(results, diagnostic, Opt(w.HasResultBlob, w.ResultBlob).Map(b => BlobKey.Of(ToKey(b))), key)
  from audit in Present(w.Provenance, "assessment.provenance", key)
  from provenance in ToEvidenceRun(audit, key)
  from payload in AssessmentPayload.Open(
   discipline, route, ToKey(w.InputKey), outcome, content, provenance, key,
   toSet(toSeq(w.DependsOnIds).Map(NodeId.Create)))
  select payload;

 // The wire keeps its flat columns; the seam's ONE PayloadContent value derives at the boundary — a diagnostic is
 // the Failure case, results-or-artifact the gated Results mint, neither the Empty case — and Open's per-row
 // Coherent law then refuses a case its outcome forbids, so a hostile record carrying both a diagnostic and
 // results lands the Failure case its outcome must admit or refuses whole.
 static Fin<PayloadContent> ToContent(
  Map<PropertyName, PropertyValue> results, Option<Diagnostic> diagnostic, Option<BlobKey> artifact, Op key) =>
  diagnostic.Match(
   Some: d => Fin.Succ(PayloadContent.Failure(d)),
   None: () => results.IsEmpty && artifact.IsNone
    ? Fin.Succ(PayloadContent.Empty)
    : PayloadContent.Results(results, artifact, key));

 // ToObservation decodes the measured series: every token re-crosses its generated row gate, every required message
 // column and every flattened window rebuilds through the presence-and-order gate the BOUNDED NodaTime Interval both
 // seam ends require, and the whole run re-enters through Rehydrate — so the advancing-chunk, bracketing-window, and
 // census-coherence invariants re-prove against hostile input rather than riding the producer's word, and an unset
 // statistics or provenance message names itself on the rail instead of dereferencing inside the residual funnel.
 // Sample bytes stay in the object store; only content keys cross.
 static Fin<ObservationSeries> ToObservation(ObservationWire w, Op key) =>
  from sensor in SensorId.Of(w.Sensor, key)
  from sampling in key.Row<string, SamplingKind>(w.Sampling)
  from quantity in ToSignature(w, key)
  from window in ToInterval(w.WindowStart, w.WindowEnd, "observation.window", key)
  from chunks in toSeq(w.Chunks).TraverseM(chunk =>
   ToInterval(chunk.WindowStart, chunk.WindowEnd, "observation.chunk.window", key)
    .Map(span => new ObservationChunk(span, BlobKey.Of(ToKey(chunk.SeriesKey)), chunk.SampleCount))).As()
  from statistics in ToStatistics(w.Statistics, quantity, key)
  from provenance in Optional(w.Provenance).Traverse(audit => ToSensorProvenance(audit, key)).As()
  from aspect in key.AcceptValidated<PropertyName>(w.Aspect)
  from series in ObservationSeries.Rehydrate(
   sensor, aspect, quantity, sampling,
   Optional(w.Cadence).Map(static c => c.ToNodaDuration()),
   window, chunks, statistics, provenance, key)
  select series;

 // The loose (type, dimension, unit) triple re-admits as the ONE QuantitySignature — coherence proves at ITS gate,
 // so Rehydrate re-checks nothing; a blank wire unit is the seam's absence.
 static Fin<QuantitySignature> ToSignature(ObservationWire w, Op key) =>
  from type in key.AcceptValidated<QuantityType>(w.Observed)
  from signature in key.AcceptValidated<QuantitySignature>(QuantitySignature.Validate(
   type,
   Dimension.Create(w.DimLength, w.DimMass, w.DimTime, w.DimCurrent, w.DimTemperature, w.DimAmount, w.DimLuminousIntensity),
   Opt(w.CanonicalUnit.Length > 0, w.CanonicalUnit), out QuantitySignature admitted), admitted)
  select signature;

 // Census keys re-cross the generated ObservationGrade gate, so an unknown grade rails rather than silently dropping
 // a bucket the completeness ratio then over-counts against; the summary message and its span column admit before
 // either read; the whole summary re-enters the OWNER's railed Of so an incoherent record refuses at its own mint.
 static Fin<SeriesStatistics> ToStatistics(SeriesStatisticsWire? w, QuantitySignature quantity, Op key) =>
  from summary in Present(w, "observation.statistics", key)
  from span in Present(summary.Span, "observation.statistics.span", key)
  from census in toSeq(summary.Census).TraverseM(entry =>
   key.Row<string, ObservationGrade>(entry.Key)
    .Map(row => (Grade: row, entry.Value))).As()
  from figures in Figures(summary, quantity, key)
  from total in OptMeasure(summary.Total, key)
  from statistics in SeriesStatistics.Of(
   census.Fold(Map<ObservationGrade, int>(), static (map, entry) => map.AddOrUpdate(entry.Grade, entry.Value)),
   span.ToNodaDuration(), figures, total, key)
  select statistics;

 // The moment group re-founds the kernel receipt WHOLE (presence on stat_mass gates it — an elder payload without
 // moments decodes a figure-less summary, never a fabricated one); the extreme scalars re-admit through
 // Scalar.From and the rebuilt receipt re-proves its OWN IsValid evidence — the decode distrust posture over a
 // foreign fold state.
 static Fin<Option<MeasureStat>> Figures(SeriesStatisticsWire w, QuantitySignature quantity, Op key) =>
  !w.HasStatMass
   ? Fin.Succ(Option<MeasureStat>.None)
   : from minimum in Present(w.Minimum, "observation.statistics.minimum", key).Bind(m => ToMeasure(m, key))
     from maximum in Present(w.Maximum, "observation.statistics.maximum", key).Bind(m => ToMeasure(m, key))
     from mean in Present(w.Mean, "observation.statistics.mean", key).Bind(m => ToMeasure(m, key))
     from low in Scalar.From(minimum.Si)
     from high in Scalar.From(maximum.Si)
     from stat in Rebuilt(new Stat<Scalar>(w.StatCount, w.StatRejected, w.StatMass, low, high, mean.Si,
       w.StatM2, w.StatM3, w.StatM4, StatContext.None), key)
     select Some(new MeasureStat(quantity, stat));

 // The rebuilt receipt re-proves its OWN IsValid evidence — a foreign fold state never crosses on trust.
 static Fin<Stat<Scalar>> Rebuilt(Stat<Scalar> stat, Op key) =>
  stat.IsValid ? Fin.Succ(stat) : new KernelFault.InvalidValue("element-wire.statistics", "moments must satisfy the statistic invariant", Some(key));

 // Absence rides the caller's Optional traversal (the blank-string Unattributed sentinel died at the owner); a
 // PRESENT audit decodes whole.
 static Fin<SensorProvenance> ToSensorProvenance(SensorProvenanceWire audit, Op key) =>
  from calibrated in ToDate(audit.HasCalibratedAt, audit.CalibratedAt, key)
  from tolerance in Optional(audit.Tolerance).Traverse(band => ToBand(band, key)).As()
  select new SensorProvenance(audit.Manufacturer, audit.Model, audit.Serial, calibrated, tolerance);

 // Absence is total through the Option traversal; a present diagnostic's two INDEPENDENT token gates accumulate.
 static Fin<Option<Diagnostic>> ToDiagnostic(DiagnosticWire? w, Op key) =>
  Optional(w).Traverse(d =>
   (key.Row<string, SolvePhase>(d.Phase),
    key.Row<string, FailureKind>(d.Kind))
    .Apply(static (phase, kind) => (phase, kind)).As()
    .Bind(t => Diagnostic.Of(t.phase, t.kind, d.Message, key, Opt(d.HasCode, d.Code)))).As();

 // Message fields carry presence by nullness (proto3 message presence); the window is both-or-neither through the
 // shared gate, and the whole run re-enters the OWNER's railed Of so a blank author or negative attempt refuses on
 // the owning kernel or semantic refusal exactly as an in-process mint — the pre-initialised mutable correlation and the trusting
 // positional construction both died here.
 static Fin<EvidenceRun> ToEvidenceRun(ProvenanceWire w, Op key) =>
  from _window in BothOrNeither(w.WindowStart is not null, w.WindowEnd is not null, "provenance-window", key)
  from correlation in Opt(w.HasCorrelation, w.Correlation).Traverse(token =>
    Guid.TryParse(token, out Guid parsed)
     ? Fin.Succ(CorrelationId.Create(parsed))
     : (Fin<CorrelationId>)new KernelFault.InvalidValue("correlation-id", $"parse {token} as a Guid", Some(key))).As()
  from at in Present(w.At, "provenance.at", key)
  from elapsed in Present(w.Elapsed, "provenance.elapsed", key)
  from window in Optional(w.WindowStart).Traverse(start => ToInterval(start, w.WindowEnd, "provenance.window", key)).As()
  from run in EvidenceRun.Of(w.Author, w.Tool, w.Version, at.ToInstant(), key,
    elapsed.ToNodaDuration(), window, correlation, w.Attempt)
  select run;
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
