# [ELEMENT_WIRE_EVIDENCE]

`WireCodec`'s evidence plane derives `PayloadContent` from generated assessment columns and re-admits through `Open`; it also transcribes `EvidenceRun`, diagnostics, observations, chunks, sensor provenance, and the append-only statistics moment group, with each decode re-proving its native validity.

## [01]-[INDEX]

- [02]-[EVIDENCE_CODEC]: assessment, evidence-run, diagnostic, and observation-family codecs.

## [02]-[EVIDENCE_CODEC]

- Cases: `AssessmentWire`/`ObservationWire` are flat node payloads; `PayloadContent` derives at the native boundary, so no additional oneof census row exists at `Graph/wire#NODE_CODEC`.
- Law: this page is one partial part of the `Graph/wire#NODE_CODEC` mapper family and composes its shared identity, presence, interval, and optional-value gates.
- Law: every decoded value re-crosses its OWNER's admission gate — the decoder constructs no case directly and trusts no carried invariant (the `ContentAddress.Verify` distrust posture); every optional column crosses by EXPLICIT presence, never a defaulted zero, blank, or sentinel.
- Packages: Google.Protobuf, Mapperly, NodaTime.Serialization.Protobuf, LanguageExt, and Thinktecture compose the generated support closure coordinated at `Graph/wire#NODE_CODEC`.
- Growth: a new column is one append-only corpus field and one transcription member; a new seated union case also updates the owning parity census.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.BoundaryConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class WireCodec {
 [UserMapping(Default = true)] internal static AssessmentWire ToWire(global::Rasm.Element.Assessment.AssessmentPayload payload) {
  AssessmentWire w = new() {
   Discipline = ToWire(payload.Discipline), Route = payload.Route.Value, InputKey = ToWire(payload.InputKey),
   Outcome = ToWire(payload.Outcome), Provenance = ToWire(payload.Provenance),
  };
  w.Results.AddRange(payload.Results.OrderBy(static pair => pair.Key.ToValue(), StringComparer.Ordinal)
   .Select(static pair => new NamedValueWire { Name = pair.Key.ToValue(), Value = ToWire(pair.Value) }));
  payload.Diagnostic.IfSome(value => w.Diagnostic = ToWire(value));
  payload.ResultArtifact.IfSome(value => w.ResultArtifact = ToWire(value));
  w.DependsOn.AddRange(payload.DependsOn.OrderBy(static id => id.ToValue(), StringComparer.Ordinal).Select(ToWire));
  return w;
 }

 [UserMapping] internal static ProvenanceWire ToWire(global::Rasm.Element.Assessment.EvidenceRun p) {
  ProvenanceWire w = new() { Author = p.Author, Tool = p.Tool, Version = p.Version, At = p.At.ToTimestamp(), Elapsed = p.Elapsed.ToProtobufDuration(), Attempt = checked((uint)p.Attempt) };
  p.Window.IfSome(i => { w.WindowStart = i.Start.ToTimestamp(); w.WindowEnd = i.End.ToTimestamp(); });
  p.Correlation.IfSome(c => w.Correlation = ByteString.CopyFrom(((Guid)c).ToByteArray(bigEndian: true)));
  return w;
 }

 [UserMapping] internal static DiagnosticWire? ToWire(Option<global::Rasm.Element.Assessment.Diagnostic> diagnostic) => diagnostic.Match<DiagnosticWire?>(
  static d => ToWire(d),
  static () => null);

 static DiagnosticWire ToWire(global::Rasm.Element.Assessment.Diagnostic diagnostic) {
  DiagnosticWire w = new() { Phase = ToWire(diagnostic.Phase), Kind = ToWire(diagnostic.Kind), Message = diagnostic.Message };
  diagnostic.Code.IfSome(value => w.Code = value);
  return w;
 }

 [UserMapping] internal static ObservationWire ToWire(global::Rasm.Element.Assessment.ObservationSeries series) {
  ObservationWire w = new() {
   Sensor = series.Sensor.Value, Aspect = series.Aspect.Value,
   Dimension = new DimensionWire {
    QuantityType = series.Observed.ToValue(),
    Length = series.Signature.Length, Mass = series.Signature.Mass, Time = series.Signature.Time,
    Current = series.Signature.Current, Temperature = series.Signature.Temperature,
    Amount = series.Signature.Amount, LuminousIntensity = series.Signature.LuminousIntensity,
   },
   CanonicalUnit = series.CanonicalUnit, Sampling = ToWire(series.Sampling),
   WindowStart = series.Window.Start.ToTimestamp(), WindowEnd = series.Window.End.ToTimestamp(),
   Statistics = ToWire(series.Statistics),
  };
  series.Provenance.IfSome(audit => w.Provenance = ToWire(audit));
  series.Cadence.IfSome(cadence => w.Cadence = cadence.ToProtobufDuration());
  w.Chunks.AddRange(series.Chunks.Map(static chunk => new ObservationChunkWire {
   WindowStart = chunk.Window.Start.ToTimestamp(), WindowEnd = chunk.Window.End.ToTimestamp(),
   SeriesArtifact = ToWire(chunk.Series), SampleCount = checked((uint)chunk.SampleCount),
  }));
  return w;
 }

 [UserMapping] internal static SensorProvenanceWire ToWire(global::Rasm.Element.Assessment.SensorProvenance provenance) {
  SensorProvenanceWire w = new() { Manufacturer = provenance.Manufacturer, Model = provenance.Model, Serial = provenance.Serial };
  provenance.CalibratedAt.IfSome(date => w.CalibratedAt = date.ToDate());
  provenance.Tolerance.IfSome(band => w.Tolerance = ToWire(band));
  return w;
 }

 [UserMapping] internal static SeriesStatisticsWire ToWire(global::Rasm.Element.Assessment.SeriesStatistics statistics) {
  SeriesStatisticsWire w = new() { Span = statistics.Span.ToProtobufDuration() };
  w.Census.AddRange(statistics.Census.OrderBy(static pair => pair.Key.Key, StringComparer.Ordinal)
   .Select(static pair => new GradeCountWire { Grade = ToWire(pair.Key), Count = checked((uint)pair.Value) }));
  statistics.Minimum.IfSome(measure => w.Minimum = ToWire(measure)); statistics.Maximum.IfSome(measure => w.Maximum = ToWire(measure));
  statistics.Mean.IfSome(measure => w.Mean = ToWire(measure)); statistics.Total.IfSome(measure => w.Total = ToWire(measure));
  statistics.Figures.IfSome(figures => {
   w.Moments = new MomentsWire {
    Count = checked((uint)figures.Figures.Count), Rejected = checked((uint)figures.Figures.Rejected),
    Mass = figures.Figures.Mass, M2 = figures.Figures.M2, M3 = figures.Figures.M3, M4 = figures.Figures.M4,
   };
  });
  return w;
 }

 static Fin<global::Rasm.Element.Assessment.AssessmentPayload> ToAssessment(AssessmentWire w) =>
  from discipline in ToDiscipline(w.Discipline, key)
  from route in FactoryBridge.Accept<AnalysisRoute>(w.Route)
  from input in ToKey(w.InputKey, key)
  from outcome in ToOutcome(w.Outcome, key)
  from results in ToValueMap(w.Results, key)
  from diagnostic in ToDiagnostic(w.Diagnostic, key)
  from artifact in Optional(w.ResultArtifact).Traverse(value => ToArtifactContent(value, "assessment.result_artifact", key)).As()
  from content in ToContent(results, diagnostic, artifact, key)
  from audit in Present(w.Provenance, "assessment.provenance", key)
  from provenance in ToEvidenceRun(audit, key)
  from dependsOn in toSeq(w.DependsOn).TraverseM(value => ToNodeId(value, key)).As()
  from payload in global::Rasm.Element.Assessment.AssessmentPayload.Open(
   discipline, route, input, outcome, content, provenance, key, toSet(dependsOn))
  select payload;

 static Fin<PayloadContent> ToContent(
  Map<PropertyName, PropertyValue> results, Option<global::Rasm.Element.Assessment.Diagnostic> diagnostic, Option<ArtifactContent> artifact) =>
  diagnostic.Match(
   Some: d => Fin.Succ(PayloadContent.Failure(d)),
   None: () => results.IsEmpty && artifact.IsNone
    ? Fin.Succ(PayloadContent.Empty)
    : PayloadContent.Results(results, artifact, key));

 static Fin<global::Rasm.Element.Assessment.ObservationSeries> ToObservation(ObservationWire w) =>
  from sensor in FactoryBridge.Accept<SensorId>(w.Sensor)
  from sampling in ToSampling(w.Sampling, key)
  from quantity in ToSignature(w, key)
  from window in ToInterval(w.WindowStart, w.WindowEnd, "observation.window", key)
  from chunks in toSeq(w.Chunks).TraverseM(chunk =>
   from span in ToInterval(chunk.WindowStart, chunk.WindowEnd, "observation.chunk.window", key)
   from artifact in ToArtifactContent(chunk.SeriesArtifact, "observation.chunk.series_artifact", key)
   select new ObservationChunk(span, artifact, checked((int)chunk.SampleCount))).As()
  from statistics in ToStatistics(w.Statistics, quantity, key)
  from provenance in Optional(w.Provenance).Traverse(audit => ToSensorProvenance(audit, key)).As()
  from aspect in FactoryBridge.Accept<PropertyName>(w.Aspect)
  from series in global::Rasm.Element.Assessment.ObservationSeries.Rehydrate(
   sensor, aspect, quantity, sampling,
   Optional(w.Cadence).Map(static c => c.ToNodaDuration()),
   window, chunks, statistics, provenance, key)
  select series;

 static Fin<QuantitySignature> ToSignature(ObservationWire w) =>
  from dimension in Present(w.Dimension, "observation.dimension")
  from type in FactoryBridge.Accept<QuantityType>(dimension.QuantityType)
  from signature in FactoryBridge.Accept<QuantitySignature>(QuantitySignature.Validate(
   type,
   Dimension.Create(
    dimension.Length, dimension.Mass, dimension.Time, dimension.Current,
    dimension.Temperature, dimension.Amount, dimension.LuminousIntensity),
   Opt(w.CanonicalUnit.Length > 0, w.CanonicalUnit), out QuantitySignature admitted), admitted)
  select signature;

 static Fin<global::Rasm.Element.Assessment.SeriesStatistics> ToStatistics(SeriesStatisticsWire? w, QuantitySignature quantity) =>
  from summary in Present(w, "observation.statistics")
  from span in Present(summary.Span, "observation.statistics.span")
  from census in toSeq(summary.Census).TraverseM(entry =>
   ToGrade(entry.Grade, key).Map(row => (Grade: row, Count: checked((int)entry.Count)))).As()
  from figures in Figures(summary, quantity, key)
  from total in OptMeasure(summary.Total, key)
  from statistics in global::Rasm.Element.Assessment.SeriesStatistics.Of(
   census.Fold(Map<global::Rasm.Element.Assessment.ObservationGrade, int>(), static (map, entry) => map.Add(entry.Grade, entry.Count)),
   span.ToNodaDuration(), figures, total, key)
  select statistics;

 static Fin<Option<MeasureStat>> Figures(SeriesStatisticsWire w, QuantitySignature quantity) =>
  w.Moments is null
   ? Fin.Succ(Option<MeasureStat>.None)
   : from minimum in Present(w.Minimum, "observation.statistics.minimum", key).Bind(m => ToMeasure(m, key))
     from maximum in Present(w.Maximum, "observation.statistics.maximum", key).Bind(m => ToMeasure(m, key))
     from mean in Present(w.Mean, "observation.statistics.mean", key).Bind(m => ToMeasure(m, key))
     from low in Scalar.From(minimum.Si)
     from high in Scalar.From(maximum.Si)
     from stat in Rebuilt(new Stat<Scalar>(
       checked((int)w.Moments.Count), checked((int)w.Moments.Rejected), w.Moments.Mass,
       low, high, mean.Si, w.Moments.M2, w.Moments.M3, w.Moments.M4, Option<StatContext>.None), key)
     select Some(new MeasureStat(quantity, stat));

 static Fin<Stat<Scalar>> Rebuilt(Stat<Scalar> stat) =>
  stat.IsValid ? Fin.Succ(stat) : new KernelFault.InvalidValue("element-wire.statistics", "moments must satisfy the statistic invariant");

 static Fin<global::Rasm.Element.Assessment.SensorProvenance> ToSensorProvenance(SensorProvenanceWire audit) =>
  from calibrated in Optional(audit.CalibratedAt).Traverse(date => Try.lift(() => date.ToLocalDate()).Run().Bind(static inner => inner)).As()
  from tolerance in Optional(audit.Tolerance).Traverse(band => ToBand(band, key)).As()
  select new global::Rasm.Element.Assessment.SensorProvenance(audit.Manufacturer, audit.Model, audit.Serial, calibrated, tolerance);

 static Fin<Option<global::Rasm.Element.Assessment.Diagnostic>> ToDiagnostic(DiagnosticWire? w) =>
  Optional(w).Traverse(d =>
   (ToSolvePhase(d.Phase, key), ToFailureKind(d.Kind, key))
    .Apply(static (phase, kind) => (phase, kind)).As()
    .Bind(t => global::Rasm.Element.Assessment.Diagnostic.Of(t.phase, t.kind, d.Message, key, Opt(d.HasCode, d.Code)))).As();

 static Fin<global::Rasm.Element.Assessment.EvidenceRun> ToEvidenceRun(ProvenanceWire w) =>
  from _window in BothOrNeither(w.WindowStart is not null, w.WindowEnd is not null, "provenance-window")
  from correlation in Opt(w.HasCorrelation, w.Correlation).Traverse(bytes =>
   bytes.Length == 16
    ? Fin.Succ(CorrelationId.Create(new Guid(bytes.Span, bigEndian: true)))
    : (Fin<CorrelationId>)new KernelFault.InvalidValue("correlation-id", "carry 16 RFC-4122 bytes")).As()
  from at in Present(w.At, "provenance.at")
  from elapsed in Present(w.Elapsed, "provenance.elapsed")
  from window in Optional(w.WindowStart).Traverse(start => ToInterval(start, w.WindowEnd, "provenance.window")).As()
  from run in global::Rasm.Element.Assessment.EvidenceRun.Of(w.Author, w.Tool, w.Version, at.ToInstant(),
    elapsed.ToNodaDuration(), window, correlation, checked((int)w.Attempt))
  select run;

 static ArtifactRef ToWire(ArtifactContent reference) => new() {
  Sha256 = ByteString.CopyFrom(Convert.FromHexString(reference.Sha256)), ArtifactBytes = reference.Bytes,
 };

 static Fin<ArtifactContent> ToArtifactContent(ArtifactRef? wire, string slot) =>
  from admitted in Present(wire, slot, key)
  from reference in ArtifactContent.Of(admitted.Sha256.Span, admitted.ArtifactBytes, key)
  select reference;

 static Rasm.Contracts.Element.Discipline ToWire(global::Rasm.Element.Classification.Discipline value) =>
  value.Switch(
   structural: static () => Rasm.Contracts.Element.Discipline.Structural,
   seismic: static () => Rasm.Contracts.Element.Discipline.Seismic,
   wind: static () => Rasm.Contracts.Element.Discipline.Wind,
   dynamic: static () => Rasm.Contracts.Element.Discipline.Dynamic,
   thermal: static () => Rasm.Contracts.Element.Discipline.Thermal,
   hygrothermal: static () => Rasm.Contracts.Element.Discipline.Hygrothermal,
   energy: static () => Rasm.Contracts.Element.Discipline.Energy,
   daylight: static () => Rasm.Contracts.Element.Discipline.Daylight,
   acoustic: static () => Rasm.Contracts.Element.Discipline.Acoustic,
   fire: static () => Rasm.Contracts.Element.Discipline.Fire,
   circulation: static () => Rasm.Contracts.Element.Discipline.Circulation,
   water: static () => Rasm.Contracts.Element.Discipline.Water,
   electrical: static () => Rasm.Contracts.Element.Discipline.Electrical,
   durability: static () => Rasm.Contracts.Element.Discipline.Durability,
   circularity: static () => Rasm.Contracts.Element.Discipline.Circularity,
   environmental: static () => Rasm.Contracts.Element.Discipline.Environmental,
   cost: static () => Rasm.Contracts.Element.Discipline.Cost);

 static Fin<global::Rasm.Element.Classification.Discipline> ToDiscipline(Rasm.Contracts.Element.Discipline value) => value switch {
  Rasm.Contracts.Element.Discipline.Structural => Fin.Succ(global::Rasm.Element.Classification.Discipline.Structural),
  Rasm.Contracts.Element.Discipline.Seismic => Fin.Succ(global::Rasm.Element.Classification.Discipline.Seismic),
  Rasm.Contracts.Element.Discipline.Wind => Fin.Succ(global::Rasm.Element.Classification.Discipline.Wind),
  Rasm.Contracts.Element.Discipline.Dynamic => Fin.Succ(global::Rasm.Element.Classification.Discipline.Dynamic),
  Rasm.Contracts.Element.Discipline.Thermal => Fin.Succ(global::Rasm.Element.Classification.Discipline.Thermal),
  Rasm.Contracts.Element.Discipline.Hygrothermal => Fin.Succ(global::Rasm.Element.Classification.Discipline.Hygrothermal),
  Rasm.Contracts.Element.Discipline.Energy => Fin.Succ(global::Rasm.Element.Classification.Discipline.Energy),
  Rasm.Contracts.Element.Discipline.Daylight => Fin.Succ(global::Rasm.Element.Classification.Discipline.Daylight),
  Rasm.Contracts.Element.Discipline.Acoustic => Fin.Succ(global::Rasm.Element.Classification.Discipline.Acoustic),
  Rasm.Contracts.Element.Discipline.Fire => Fin.Succ(global::Rasm.Element.Classification.Discipline.Fire),
  Rasm.Contracts.Element.Discipline.Circulation => Fin.Succ(global::Rasm.Element.Classification.Discipline.Circulation),
  Rasm.Contracts.Element.Discipline.Water => Fin.Succ(global::Rasm.Element.Classification.Discipline.Water),
  Rasm.Contracts.Element.Discipline.Electrical => Fin.Succ(global::Rasm.Element.Classification.Discipline.Electrical),
  Rasm.Contracts.Element.Discipline.Durability => Fin.Succ(global::Rasm.Element.Classification.Discipline.Durability),
  Rasm.Contracts.Element.Discipline.Circularity => Fin.Succ(global::Rasm.Element.Classification.Discipline.Circularity),
  Rasm.Contracts.Element.Discipline.Environmental => Fin.Succ(global::Rasm.Element.Classification.Discipline.Environmental),
  Rasm.Contracts.Element.Discipline.Cost => Fin.Succ(global::Rasm.Element.Classification.Discipline.Cost),
  _ => Unmapped<global::Rasm.Element.Classification.Discipline>("discipline", value, key),
 };

 static Rasm.Contracts.Element.SamplingKind ToWire(global::Rasm.Element.Assessment.SamplingKind value) =>
  value.Switch(
   instantaneous: static () => Rasm.Contracts.Element.SamplingKind.Instantaneous,
   averaged: static () => Rasm.Contracts.Element.SamplingKind.Averaged,
   total: static () => Rasm.Contracts.Element.SamplingKind.Total,
   cumulative: static () => Rasm.Contracts.Element.SamplingKind.Cumulative,
   minimum: static () => Rasm.Contracts.Element.SamplingKind.Minimum,
   maximum: static () => Rasm.Contracts.Element.SamplingKind.Maximum);

 static Fin<global::Rasm.Element.Assessment.SamplingKind> ToSampling(Rasm.Contracts.Element.SamplingKind value) => value switch {
  Rasm.Contracts.Element.SamplingKind.Instantaneous => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Instantaneous),
  Rasm.Contracts.Element.SamplingKind.Averaged => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Averaged),
  Rasm.Contracts.Element.SamplingKind.Total => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Total),
  Rasm.Contracts.Element.SamplingKind.Cumulative => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Cumulative),
  Rasm.Contracts.Element.SamplingKind.Minimum => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Minimum),
  Rasm.Contracts.Element.SamplingKind.Maximum => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Maximum),
  _ => Unmapped<global::Rasm.Element.Assessment.SamplingKind>("sampling-kind", value, key),
 };

 static Rasm.Contracts.Element.ObservationGrade ToWire(global::Rasm.Element.Assessment.ObservationGrade value) =>
  value.Switch(
   measured: static () => Rasm.Contracts.Element.ObservationGrade.Measured,
   validated: static () => Rasm.Contracts.Element.ObservationGrade.Validated,
   substituted: static () => Rasm.Contracts.Element.ObservationGrade.Substituted,
   suspect: static () => Rasm.Contracts.Element.ObservationGrade.Suspect,
   missing: static () => Rasm.Contracts.Element.ObservationGrade.Missing);

 static Fin<global::Rasm.Element.Assessment.ObservationGrade> ToGrade(Rasm.Contracts.Element.ObservationGrade value) => value switch {
  Rasm.Contracts.Element.ObservationGrade.Measured => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Measured),
  Rasm.Contracts.Element.ObservationGrade.Validated => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Validated),
  Rasm.Contracts.Element.ObservationGrade.Substituted => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Substituted),
  Rasm.Contracts.Element.ObservationGrade.Suspect => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Suspect),
  Rasm.Contracts.Element.ObservationGrade.Missing => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Missing),
  _ => Unmapped<global::Rasm.Element.Assessment.ObservationGrade>("observation-grade", value, key),
 };

 static Rasm.Contracts.Element.AssessmentOutcome ToWire(global::Rasm.Element.Assessment.AssessmentOutcome value) =>
  value.Switch(
   pending: static () => Rasm.Contracts.Element.AssessmentOutcome.Pending,
   queued: static () => Rasm.Contracts.Element.AssessmentOutcome.Queued,
   running: static () => Rasm.Contracts.Element.AssessmentOutcome.Running,
   computed: static () => Rasm.Contracts.Element.AssessmentOutcome.Computed,
   failed: static () => Rasm.Contracts.Element.AssessmentOutcome.Failed,
   cancelled: static () => Rasm.Contracts.Element.AssessmentOutcome.Cancelled,
   stale: static () => Rasm.Contracts.Element.AssessmentOutcome.Stale,
   superseded: static () => Rasm.Contracts.Element.AssessmentOutcome.Superseded);

 static Fin<global::Rasm.Element.Assessment.AssessmentOutcome> ToOutcome(Rasm.Contracts.Element.AssessmentOutcome value) => value switch {
  Rasm.Contracts.Element.AssessmentOutcome.Pending => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Pending),
  Rasm.Contracts.Element.AssessmentOutcome.Queued => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Queued),
  Rasm.Contracts.Element.AssessmentOutcome.Running => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Running),
  Rasm.Contracts.Element.AssessmentOutcome.Computed => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Computed),
  Rasm.Contracts.Element.AssessmentOutcome.Failed => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Failed),
  Rasm.Contracts.Element.AssessmentOutcome.Cancelled => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Cancelled),
  Rasm.Contracts.Element.AssessmentOutcome.Stale => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Stale),
  Rasm.Contracts.Element.AssessmentOutcome.Superseded => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Superseded),
  _ => Unmapped<global::Rasm.Element.Assessment.AssessmentOutcome>("assessment-outcome", value, key),
 };

 static Rasm.Contracts.Element.SolvePhase ToWire(global::Rasm.Element.Assessment.SolvePhase value) =>
  value.Switch(
   admission: static () => Rasm.Contracts.Element.SolvePhase.Admission,
   solve: static () => Rasm.Contracts.Element.SolvePhase.Solve,
   extraction: static () => Rasm.Contracts.Element.SolvePhase.Extraction,
   publication: static () => Rasm.Contracts.Element.SolvePhase.Publication);

 static Fin<global::Rasm.Element.Assessment.SolvePhase> ToSolvePhase(Rasm.Contracts.Element.SolvePhase value) => value switch {
  Rasm.Contracts.Element.SolvePhase.Admission => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Admission),
  Rasm.Contracts.Element.SolvePhase.Solve => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Solve),
  Rasm.Contracts.Element.SolvePhase.Extraction => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Extraction),
  Rasm.Contracts.Element.SolvePhase.Publication => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Publication),
  _ => Unmapped<global::Rasm.Element.Assessment.SolvePhase>("solve-phase", value, key),
 };

 static Rasm.Contracts.Element.FailureKind ToWire(global::Rasm.Element.Assessment.FailureKind value) =>
  value.Switch(
   input: static () => Rasm.Contracts.Element.FailureKind.Input,
   numeric: static () => Rasm.Contracts.Element.FailureKind.Numeric,
   resource: static () => Rasm.Contracts.Element.FailureKind.Resource,
   timeout: static () => Rasm.Contracts.Element.FailureKind.Timeout,
   aborted: static () => Rasm.Contracts.Element.FailureKind.Aborted,
   foreign: static () => Rasm.Contracts.Element.FailureKind.Foreign);

 static Fin<global::Rasm.Element.Assessment.FailureKind> ToFailureKind(Rasm.Contracts.Element.FailureKind value) => value switch {
  Rasm.Contracts.Element.FailureKind.Input => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Input),
  Rasm.Contracts.Element.FailureKind.Numeric => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Numeric),
  Rasm.Contracts.Element.FailureKind.Resource => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Resource),
  Rasm.Contracts.Element.FailureKind.Timeout => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Timeout),
  Rasm.Contracts.Element.FailureKind.Aborted => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Aborted),
  Rasm.Contracts.Element.FailureKind.Foreign => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Foreign),
  _ => Unmapped<global::Rasm.Element.Assessment.FailureKind>("failure-kind", value, key),
 };

 static Fin<T> Unmapped<T>(string slot, Enum value) =>
  new KernelFault.InvalidValue(slot, $"admit defined value {value}");
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
