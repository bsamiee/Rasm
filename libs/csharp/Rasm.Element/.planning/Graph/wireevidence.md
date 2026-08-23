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

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.Contracts.Artifact.V1;
using Rasm.Contracts.Element.V1;
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
// the shared decode gates ride `Graph/wire#NODE_CODEC`; this part owns the assessment and observation evidence transcriptions.
internal static partial class WireCodec {
 [UserMapping(Default = true)] internal static AssessmentWire ToWire(global::Rasm.Element.Assessment.AssessmentPayload payload) {
  AssessmentWire w = new() {
   Discipline = ToWire(payload.Discipline), Route = payload.Route.Value, InputKey = ToWire(payload.InputKey),
   Outcome = ToWire(payload.Outcome), Provenance = ToWire(payload.Provenance),
  };
  w.Results.AddRange(payload.Results.OrderBy(static pair => pair.Key.Value, StringComparer.Ordinal)
   .Select(static pair => new NamedValueWire { Name = pair.Key.Value, Value = ToWire(pair.Value) }));
  payload.Diagnostic.IfSome(value => w.Diagnostic = ToWire(value));
  payload.ResultArtifact.IfSome(value => w.ResultArtifact = ToWire(value));
  w.DependsOn.AddRange(payload.DependsOn.OrderBy(static id => id.Value, StringComparer.Ordinal).Select(ToWire));
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

 // Hand-owned like ToWire(GeoReference): the Interval flattens to a bounded column PAIR and the census map keys on a
 // generated row, neither a shape Mapperly bridges. Both window ends are bounded by seam admission, so the columns
 // are unconditional and no presence flag stands in for an unbounded side.
 [UserMapping] internal static ObservationWire ToWire(global::Rasm.Element.Assessment.ObservationSeries series) {
  ObservationWire w = new() {
   Sensor = series.Sensor.Value, Aspect = series.Aspect.Value,
   Dimension = new DimensionWire {
    QuantityType = series.Observed.Value,
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

 // The one-hop min/max/mean scalars stay peer-informative; the kernel moment group crosses beside them so the
 // receipt round-trips whole through the generated statistics moment group.
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

 static Fin<global::Rasm.Element.Assessment.AssessmentPayload> ToAssessment(AssessmentWire w, Op key) =>
  from discipline in ToDiscipline(w.Discipline, key)
  from route in AnalysisRoute.Of(w.Route, key)
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

 // The wire keeps its flat columns; the seam's ONE PayloadContent value derives at the boundary — a diagnostic is
 // the Failure case, results-or-artifact the gated Results mint, neither the Empty case — and Open's per-row
 // Coherent law then refuses a case its outcome forbids, so a hostile record carrying both a diagnostic and
 // results lands the Failure case its outcome must admit or refuses whole.
 static Fin<PayloadContent> ToContent(
  Map<PropertyName, PropertyValue> results, Option<global::Rasm.Element.Assessment.Diagnostic> diagnostic, Option<ArtifactContent> artifact, Op key) =>
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
 static Fin<global::Rasm.Element.Assessment.ObservationSeries> ToObservation(ObservationWire w, Op key) =>
  from sensor in SensorId.Of(w.Sensor, key)
  from sampling in ToSampling(w.Sampling, key)
  from quantity in ToSignature(w, key)
  from window in ToInterval(w.WindowStart, w.WindowEnd, "observation.window", key)
  from chunks in toSeq(w.Chunks).TraverseM(chunk =>
   from span in ToInterval(chunk.WindowStart, chunk.WindowEnd, "observation.chunk.window", key)
   from artifact in ToArtifactContent(chunk.SeriesArtifact, "observation.chunk.series_artifact", key)
   select new ObservationChunk(span, artifact, checked((int)chunk.SampleCount))).As()
  from statistics in ToStatistics(w.Statistics, quantity, key)
  from provenance in Optional(w.Provenance).Traverse(audit => ToSensorProvenance(audit, key)).As()
  from aspect in key.AcceptValidated<PropertyName>(w.Aspect)
  from series in global::Rasm.Element.Assessment.ObservationSeries.Rehydrate(
   sensor, aspect, quantity, sampling,
   Optional(w.Cadence).Map(static c => c.ToNodaDuration()),
   window, chunks, statistics, provenance, key)
  select series;

 // The loose (type, dimension, unit) triple re-admits as the ONE QuantitySignature — coherence proves at ITS gate,
 // so Rehydrate re-checks nothing; a blank wire unit is the seam's absence.
 static Fin<QuantitySignature> ToSignature(ObservationWire w, Op key) =>
  from dimension in Present(w.Dimension, "observation.dimension", key)
  from type in key.AcceptValidated<QuantityType>(dimension.QuantityType)
  from signature in key.AcceptValidated<QuantitySignature>(QuantitySignature.Validate(
   type,
   Dimension.Create(
    dimension.Length, dimension.Mass, dimension.Time, dimension.Current,
    dimension.Temperature, dimension.Amount, dimension.LuminousIntensity),
   Opt(w.CanonicalUnit.Length > 0, w.CanonicalUnit), out QuantitySignature admitted), admitted)
  select signature;

 // Census keys re-cross the generated ObservationGrade gate, so an unknown grade rails rather than silently dropping
 // a bucket the completeness ratio then over-counts against; the summary message and its span column admit before
 // either read; the whole summary re-enters the OWNER's railed Of so an incoherent record refuses at its own mint.
 static Fin<global::Rasm.Element.Assessment.SeriesStatistics> ToStatistics(SeriesStatisticsWire? w, QuantitySignature quantity, Op key) =>
  from summary in Present(w, "observation.statistics", key)
  from span in Present(summary.Span, "observation.statistics.span", key)
  from census in toSeq(summary.Census).TraverseM(entry =>
   ToGrade(entry.Grade, key).Map(row => (Grade: row, Count: checked((int)entry.Count)))).As()
  from figures in Figures(summary, quantity, key)
  from total in OptMeasure(summary.Total, key)
  from statistics in global::Rasm.Element.Assessment.SeriesStatistics.Of(
   census.Fold(Map<global::Rasm.Element.Assessment.ObservationGrade, int>(), static (map, entry) => map.Add(entry.Grade, entry.Count)),
   span.ToNodaDuration(), figures, total, key)
  select statistics;

 // The moment group re-founds the kernel receipt WHOLE (presence on stat_mass gates it — an elder payload without
 // moments decodes a figure-less summary, never a fabricated one); the extreme scalars re-admit through
 // Scalar.From and the rebuilt receipt re-proves its OWN IsValid evidence — the decode distrust posture over a
 // foreign fold state.
 static Fin<Option<MeasureStat>> Figures(SeriesStatisticsWire w, QuantitySignature quantity, Op key) =>
  w.Moments is null
   ? Fin.Succ(Option<MeasureStat>.None)
   : from minimum in Present(w.Minimum, "observation.statistics.minimum", key).Bind(m => ToMeasure(m, key))
     from maximum in Present(w.Maximum, "observation.statistics.maximum", key).Bind(m => ToMeasure(m, key))
     from mean in Present(w.Mean, "observation.statistics.mean", key).Bind(m => ToMeasure(m, key))
     from low in Scalar.From(minimum.Si)
     from high in Scalar.From(maximum.Si)
     from stat in Rebuilt(new Stat<Scalar>(
       checked((int)w.Moments.Count), checked((int)w.Moments.Rejected), w.Moments.Mass,
       low, high, mean.Si, w.Moments.M2, w.Moments.M3, w.Moments.M4, StatContext.None), key)
     select Some(new MeasureStat(quantity, stat));

 // The rebuilt receipt re-proves its OWN IsValid evidence — a foreign fold state never crosses on trust.
 static Fin<Stat<Scalar>> Rebuilt(Stat<Scalar> stat, Op key) =>
  stat.IsValid ? Fin.Succ(stat) : new KernelFault.InvalidValue("element-wire.statistics", "moments must satisfy the statistic invariant", Some(key));

 // Absence rides the caller's Optional traversal (the blank-string Unattributed sentinel died at the owner); a
 // PRESENT audit decodes whole.
 static Fin<global::Rasm.Element.Assessment.SensorProvenance> ToSensorProvenance(SensorProvenanceWire audit, Op key) =>
  from calibrated in Optional(audit.CalibratedAt).Traverse(date => key.Catch(() => date.ToLocalDate())).As()
  from tolerance in Optional(audit.Tolerance).Traverse(band => ToBand(band, key)).As()
  select new global::Rasm.Element.Assessment.SensorProvenance(audit.Manufacturer, audit.Model, audit.Serial, calibrated, tolerance);

 // Absence is total through the Option traversal; a present diagnostic's two INDEPENDENT token gates accumulate.
 static Fin<Option<global::Rasm.Element.Assessment.Diagnostic>> ToDiagnostic(DiagnosticWire? w, Op key) =>
  Optional(w).Traverse(d =>
   (ToSolvePhase(d.Phase, key), ToFailureKind(d.Kind, key))
    .Apply(static (phase, kind) => (phase, kind)).As()
    .Bind(t => global::Rasm.Element.Assessment.Diagnostic.Of(t.phase, t.kind, d.Message, key, Opt(d.HasCode, d.Code)))).As();

 // Message fields carry presence by nullness (proto3 message presence); the window is both-or-neither through the
 // shared gate, and the whole run re-enters the OWNER's railed Of so a blank author or negative attempt refuses on
 // the owning kernel or semantic refusal exactly as an in-process mint — the pre-initialised mutable correlation and the trusting
 // positional construction both died here.
 static Fin<global::Rasm.Element.Assessment.EvidenceRun> ToEvidenceRun(ProvenanceWire w, Op key) =>
  from _window in BothOrNeither(w.WindowStart is not null, w.WindowEnd is not null, "provenance-window", key)
  from correlation in Opt(w.HasCorrelation, w.Correlation).Traverse(bytes =>
   bytes.Length == 16
    ? Fin.Succ(CorrelationId.Create(new Guid(bytes.Span, bigEndian: true)))
    : (Fin<CorrelationId>)new KernelFault.InvalidValue("correlation-id", "carry 16 RFC-4122 bytes", Some(key))).As()
  from at in Present(w.At, "provenance.at", key)
  from elapsed in Present(w.Elapsed, "provenance.elapsed", key)
  from window in Optional(w.WindowStart).Traverse(start => ToInterval(start, w.WindowEnd, "provenance.window", key)).As()
  from run in global::Rasm.Element.Assessment.EvidenceRun.Of(w.Author, w.Tool, w.Version, at.ToInstant(), key,
    elapsed.ToNodaDuration(), window, correlation, checked((int)w.Attempt))
  select run;

 static ArtifactRef ToWire(ArtifactContent reference) => new() {
  Sha256 = ByteString.CopyFrom(Convert.FromHexString(reference.Sha256)), ArtifactBytes = reference.Bytes,
 };

 static Fin<ArtifactContent> ToArtifactContent(ArtifactRef? wire, string slot, Op key) =>
  from admitted in Present(wire, slot, key)
  from reference in ArtifactContent.Of(admitted.Sha256.Span, admitted.ArtifactBytes, key)
  select reference;

 static Rasm.Contracts.Element.V1.Discipline ToWire(global::Rasm.Element.Classification.Discipline value) =>
  value == global::Rasm.Element.Classification.Discipline.Structural ? Rasm.Contracts.Element.V1.Discipline.Structural
  : value == global::Rasm.Element.Classification.Discipline.Seismic ? Rasm.Contracts.Element.V1.Discipline.Seismic
  : value == global::Rasm.Element.Classification.Discipline.Wind ? Rasm.Contracts.Element.V1.Discipline.Wind
  : value == global::Rasm.Element.Classification.Discipline.Dynamic ? Rasm.Contracts.Element.V1.Discipline.Dynamic
  : value == global::Rasm.Element.Classification.Discipline.Thermal ? Rasm.Contracts.Element.V1.Discipline.Thermal
  : value == global::Rasm.Element.Classification.Discipline.Hygrothermal ? Rasm.Contracts.Element.V1.Discipline.Hygrothermal
  : value == global::Rasm.Element.Classification.Discipline.Energy ? Rasm.Contracts.Element.V1.Discipline.Energy
  : value == global::Rasm.Element.Classification.Discipline.Daylight ? Rasm.Contracts.Element.V1.Discipline.Daylight
  : value == global::Rasm.Element.Classification.Discipline.Acoustic ? Rasm.Contracts.Element.V1.Discipline.Acoustic
  : value == global::Rasm.Element.Classification.Discipline.Fire ? Rasm.Contracts.Element.V1.Discipline.Fire
  : value == global::Rasm.Element.Classification.Discipline.Circulation ? Rasm.Contracts.Element.V1.Discipline.Circulation
  : value == global::Rasm.Element.Classification.Discipline.Water ? Rasm.Contracts.Element.V1.Discipline.Water
  : value == global::Rasm.Element.Classification.Discipline.Electrical ? Rasm.Contracts.Element.V1.Discipline.Electrical
  : value == global::Rasm.Element.Classification.Discipline.Durability ? Rasm.Contracts.Element.V1.Discipline.Durability
  : value == global::Rasm.Element.Classification.Discipline.Circularity ? Rasm.Contracts.Element.V1.Discipline.Circularity
  : value == global::Rasm.Element.Classification.Discipline.Environmental ? Rasm.Contracts.Element.V1.Discipline.Environmental
  : value == global::Rasm.Element.Classification.Discipline.Cost ? Rasm.Contracts.Element.V1.Discipline.Cost
  : throw new UnreachableException($"unseated discipline {value.Key}");

 static Fin<global::Rasm.Element.Classification.Discipline> ToDiscipline(Rasm.Contracts.Element.V1.Discipline value, Op key) => value switch {
  Rasm.Contracts.Element.V1.Discipline.Structural => Fin.Succ(global::Rasm.Element.Classification.Discipline.Structural),
  Rasm.Contracts.Element.V1.Discipline.Seismic => Fin.Succ(global::Rasm.Element.Classification.Discipline.Seismic),
  Rasm.Contracts.Element.V1.Discipline.Wind => Fin.Succ(global::Rasm.Element.Classification.Discipline.Wind),
  Rasm.Contracts.Element.V1.Discipline.Dynamic => Fin.Succ(global::Rasm.Element.Classification.Discipline.Dynamic),
  Rasm.Contracts.Element.V1.Discipline.Thermal => Fin.Succ(global::Rasm.Element.Classification.Discipline.Thermal),
  Rasm.Contracts.Element.V1.Discipline.Hygrothermal => Fin.Succ(global::Rasm.Element.Classification.Discipline.Hygrothermal),
  Rasm.Contracts.Element.V1.Discipline.Energy => Fin.Succ(global::Rasm.Element.Classification.Discipline.Energy),
  Rasm.Contracts.Element.V1.Discipline.Daylight => Fin.Succ(global::Rasm.Element.Classification.Discipline.Daylight),
  Rasm.Contracts.Element.V1.Discipline.Acoustic => Fin.Succ(global::Rasm.Element.Classification.Discipline.Acoustic),
  Rasm.Contracts.Element.V1.Discipline.Fire => Fin.Succ(global::Rasm.Element.Classification.Discipline.Fire),
  Rasm.Contracts.Element.V1.Discipline.Circulation => Fin.Succ(global::Rasm.Element.Classification.Discipline.Circulation),
  Rasm.Contracts.Element.V1.Discipline.Water => Fin.Succ(global::Rasm.Element.Classification.Discipline.Water),
  Rasm.Contracts.Element.V1.Discipline.Electrical => Fin.Succ(global::Rasm.Element.Classification.Discipline.Electrical),
  Rasm.Contracts.Element.V1.Discipline.Durability => Fin.Succ(global::Rasm.Element.Classification.Discipline.Durability),
  Rasm.Contracts.Element.V1.Discipline.Circularity => Fin.Succ(global::Rasm.Element.Classification.Discipline.Circularity),
  Rasm.Contracts.Element.V1.Discipline.Environmental => Fin.Succ(global::Rasm.Element.Classification.Discipline.Environmental),
  Rasm.Contracts.Element.V1.Discipline.Cost => Fin.Succ(global::Rasm.Element.Classification.Discipline.Cost),
  _ => Unmapped<global::Rasm.Element.Classification.Discipline>("discipline", value, key),
 };

 static Rasm.Contracts.Element.V1.SamplingKind ToWire(global::Rasm.Element.Assessment.SamplingKind value) =>
  value == global::Rasm.Element.Assessment.SamplingKind.Instantaneous ? Rasm.Contracts.Element.V1.SamplingKind.Instantaneous
  : value == global::Rasm.Element.Assessment.SamplingKind.Averaged ? Rasm.Contracts.Element.V1.SamplingKind.Averaged
  : value == global::Rasm.Element.Assessment.SamplingKind.Total ? Rasm.Contracts.Element.V1.SamplingKind.Total
  : value == global::Rasm.Element.Assessment.SamplingKind.Cumulative ? Rasm.Contracts.Element.V1.SamplingKind.Cumulative
  : value == global::Rasm.Element.Assessment.SamplingKind.Minimum ? Rasm.Contracts.Element.V1.SamplingKind.Minimum
  : value == global::Rasm.Element.Assessment.SamplingKind.Maximum ? Rasm.Contracts.Element.V1.SamplingKind.Maximum
  : throw new UnreachableException($"unseated sampling kind {value.Key}");

 static Fin<global::Rasm.Element.Assessment.SamplingKind> ToSampling(Rasm.Contracts.Element.V1.SamplingKind value, Op key) => value switch {
  Rasm.Contracts.Element.V1.SamplingKind.Instantaneous => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Instantaneous),
  Rasm.Contracts.Element.V1.SamplingKind.Averaged => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Averaged),
  Rasm.Contracts.Element.V1.SamplingKind.Total => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Total),
  Rasm.Contracts.Element.V1.SamplingKind.Cumulative => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Cumulative),
  Rasm.Contracts.Element.V1.SamplingKind.Minimum => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Minimum),
  Rasm.Contracts.Element.V1.SamplingKind.Maximum => Fin.Succ(global::Rasm.Element.Assessment.SamplingKind.Maximum),
  _ => Unmapped<global::Rasm.Element.Assessment.SamplingKind>("sampling-kind", value, key),
 };

 static Rasm.Contracts.Element.V1.ObservationGrade ToWire(global::Rasm.Element.Assessment.ObservationGrade value) =>
  value == global::Rasm.Element.Assessment.ObservationGrade.Measured ? Rasm.Contracts.Element.V1.ObservationGrade.Measured
  : value == global::Rasm.Element.Assessment.ObservationGrade.Validated ? Rasm.Contracts.Element.V1.ObservationGrade.Validated
  : value == global::Rasm.Element.Assessment.ObservationGrade.Substituted ? Rasm.Contracts.Element.V1.ObservationGrade.Substituted
  : value == global::Rasm.Element.Assessment.ObservationGrade.Suspect ? Rasm.Contracts.Element.V1.ObservationGrade.Suspect
  : value == global::Rasm.Element.Assessment.ObservationGrade.Missing ? Rasm.Contracts.Element.V1.ObservationGrade.Missing
  : throw new UnreachableException($"unseated observation grade {value.Key}");

 static Fin<global::Rasm.Element.Assessment.ObservationGrade> ToGrade(Rasm.Contracts.Element.V1.ObservationGrade value, Op key) => value switch {
  Rasm.Contracts.Element.V1.ObservationGrade.Measured => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Measured),
  Rasm.Contracts.Element.V1.ObservationGrade.Validated => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Validated),
  Rasm.Contracts.Element.V1.ObservationGrade.Substituted => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Substituted),
  Rasm.Contracts.Element.V1.ObservationGrade.Suspect => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Suspect),
  Rasm.Contracts.Element.V1.ObservationGrade.Missing => Fin.Succ(global::Rasm.Element.Assessment.ObservationGrade.Missing),
  _ => Unmapped<global::Rasm.Element.Assessment.ObservationGrade>("observation-grade", value, key),
 };

 static Rasm.Contracts.Element.V1.AssessmentOutcome ToWire(global::Rasm.Element.Assessment.AssessmentOutcome value) =>
  value == global::Rasm.Element.Assessment.AssessmentOutcome.Pending ? Rasm.Contracts.Element.V1.AssessmentOutcome.Pending
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Queued ? Rasm.Contracts.Element.V1.AssessmentOutcome.Queued
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Running ? Rasm.Contracts.Element.V1.AssessmentOutcome.Running
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Computed ? Rasm.Contracts.Element.V1.AssessmentOutcome.Computed
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Failed ? Rasm.Contracts.Element.V1.AssessmentOutcome.Failed
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Cancelled ? Rasm.Contracts.Element.V1.AssessmentOutcome.Cancelled
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Stale ? Rasm.Contracts.Element.V1.AssessmentOutcome.Stale
  : value == global::Rasm.Element.Assessment.AssessmentOutcome.Superseded ? Rasm.Contracts.Element.V1.AssessmentOutcome.Superseded
  : throw new UnreachableException($"unseated assessment outcome {value.Key}");

 static Fin<global::Rasm.Element.Assessment.AssessmentOutcome> ToOutcome(Rasm.Contracts.Element.V1.AssessmentOutcome value, Op key) => value switch {
  Rasm.Contracts.Element.V1.AssessmentOutcome.Pending => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Pending),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Queued => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Queued),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Running => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Running),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Computed => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Computed),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Failed => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Failed),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Cancelled => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Cancelled),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Stale => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Stale),
  Rasm.Contracts.Element.V1.AssessmentOutcome.Superseded => Fin.Succ(global::Rasm.Element.Assessment.AssessmentOutcome.Superseded),
  _ => Unmapped<global::Rasm.Element.Assessment.AssessmentOutcome>("assessment-outcome", value, key),
 };

 static Rasm.Contracts.Element.V1.SolvePhase ToWire(global::Rasm.Element.Assessment.SolvePhase value) =>
  value == global::Rasm.Element.Assessment.SolvePhase.Admission ? Rasm.Contracts.Element.V1.SolvePhase.Admission
  : value == global::Rasm.Element.Assessment.SolvePhase.Solve ? Rasm.Contracts.Element.V1.SolvePhase.Solve
  : value == global::Rasm.Element.Assessment.SolvePhase.Extraction ? Rasm.Contracts.Element.V1.SolvePhase.Extraction
  : value == global::Rasm.Element.Assessment.SolvePhase.Publication ? Rasm.Contracts.Element.V1.SolvePhase.Publication
  : throw new UnreachableException($"unseated solve phase {value.Key}");

 static Fin<global::Rasm.Element.Assessment.SolvePhase> ToSolvePhase(Rasm.Contracts.Element.V1.SolvePhase value, Op key) => value switch {
  Rasm.Contracts.Element.V1.SolvePhase.Admission => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Admission),
  Rasm.Contracts.Element.V1.SolvePhase.Solve => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Solve),
  Rasm.Contracts.Element.V1.SolvePhase.Extraction => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Extraction),
  Rasm.Contracts.Element.V1.SolvePhase.Publication => Fin.Succ(global::Rasm.Element.Assessment.SolvePhase.Publication),
  _ => Unmapped<global::Rasm.Element.Assessment.SolvePhase>("solve-phase", value, key),
 };

 static Rasm.Contracts.Element.V1.FailureKind ToWire(global::Rasm.Element.Assessment.FailureKind value) =>
  value == global::Rasm.Element.Assessment.FailureKind.Input ? Rasm.Contracts.Element.V1.FailureKind.Input
  : value == global::Rasm.Element.Assessment.FailureKind.Numeric ? Rasm.Contracts.Element.V1.FailureKind.Numeric
  : value == global::Rasm.Element.Assessment.FailureKind.Resource ? Rasm.Contracts.Element.V1.FailureKind.Resource
  : value == global::Rasm.Element.Assessment.FailureKind.Timeout ? Rasm.Contracts.Element.V1.FailureKind.Timeout
  : value == global::Rasm.Element.Assessment.FailureKind.Aborted ? Rasm.Contracts.Element.V1.FailureKind.Aborted
  : value == global::Rasm.Element.Assessment.FailureKind.Foreign ? Rasm.Contracts.Element.V1.FailureKind.Foreign
  : throw new UnreachableException($"unseated failure kind {value.Key}");

 static Fin<global::Rasm.Element.Assessment.FailureKind> ToFailureKind(Rasm.Contracts.Element.V1.FailureKind value, Op key) => value switch {
  Rasm.Contracts.Element.V1.FailureKind.Input => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Input),
  Rasm.Contracts.Element.V1.FailureKind.Numeric => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Numeric),
  Rasm.Contracts.Element.V1.FailureKind.Resource => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Resource),
  Rasm.Contracts.Element.V1.FailureKind.Timeout => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Timeout),
  Rasm.Contracts.Element.V1.FailureKind.Aborted => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Aborted),
  Rasm.Contracts.Element.V1.FailureKind.Foreign => Fin.Succ(global::Rasm.Element.Assessment.FailureKind.Foreign),
  _ => Unmapped<global::Rasm.Element.Assessment.FailureKind>("failure-kind", value, key),
 };

 static Fin<T> Unmapped<T>(string slot, Enum value, Op key) =>
  new KernelFault.InvalidValue(slot, $"admit defined value {value}", Some(key));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
