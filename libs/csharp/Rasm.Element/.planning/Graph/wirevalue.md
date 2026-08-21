# [ELEMENT_WIRE_VALUE]

`WireCodec`'s typed-value plane: the recursive fourteen-case `PropertyValue` fold under the `WireLimits` depth budget, the five-leaf `TemporalValue` ISO crossings, the `MeasureValue`/`MeasureBand` DECODE legs re-minting through the owner's `OfSi` finite gate (the encode legs ride the peer-reachable `SeamConverters` at `Graph/wire`), the `PropertyBag`/`QuantityBag` transcriptions with their `EvidenceGrade` rank gate, `GroupIdentity`, and the S-E3 `PropertyEvidence`/`Attestation` envelope (grade, attestation, and the `EvidenceRun` audit link crossing append-only — ledger row [16]).

## [01]-[INDEX]

- [02]-[VALUE_CODEC]: recursive `PropertyValue`/`TemporalValue` folds, the measure and bag codecs, and the `PropertyEvidence`/`Attestation` envelope.

## [02]-[VALUE_CODEC]

- Cases: `PropertyValue` 14 arms and `TemporalValue` 5 arms — census rows [03]/[04] at `Graph/wire#WIRE_CODEC`.
- Law: this page is one PARTIAL PART of the `Graph/wire#WIRE_CODEC` `[Mapper]` family — the `[Mapper]` attribute, the `[UNION_PARITY]` census, the `[KEY_CODECS]`, the shared decode gates (`Present`/`Opt`/`Row`/`Named`/`Iso`/`ToInterval`/`ToDate`/`BothOrNeither`/`OptMeasure`/`OptCurve`), the `[PRESENCE_SHELLS]` and carrier-codec laws, `ElementWire`, and the frozen-number ledger all live THERE; a member landing here lands its census/ledger row there in the same edit.
- Law: every decoded value re-crosses its OWNER's admission gate — the decoder constructs no case directly and trusts no carried invariant (the `ContentAddress.Verify` distrust posture); every optional column crosses by EXPLICIT presence, never a defaulted zero, blank, or sentinel.
- Packages: Google.Protobuf, Riok.Mapperly, NodaTime.Serialization.Protobuf, LanguageExt.Core, Thinktecture.Runtime.Extensions (the generated total `Switch` encode dispatch and `TryGet` row gates) — the manifest triad rides `Graph/wire#WIRE_CODEC`.
- Growth: a new column on a family this page owns is one append-only numbered field at the corpus proto, one ledger row at `Graph/wire#WIRE_CODEC`, and one transcription member here; a new union case also lands its `CrossingFamily` arm count and its oneof mirror in the same edit — the parity census refuses a half-landed pair.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Numerics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ---------------------------------------------------------------------------
// One partial part of the ONE `[Mapper]` WireCodec family — the attribute, the parity census, the key codecs, and
// the shared decode gates ride `Graph/wire#WIRE_CODEC`; this part owns the typed value, measure, bag, and evidence-envelope transcriptions.
internal static partial class WireCodec {
 // The property bag's Groups is empty by construction (its nesting is the PropertyValue.Complex case) and
 // PropertySetWire declares no counterpart, so the source member is ignored EXPLICITLY — RequiredMappingStrategy.Both
 // faults an unmapped source member, and that fault is the signal a group-bearing property bag would owe a wire field.
 [MapperIgnoreSource(nameof(PropertyBag.Groups))]
 [MapProperty(nameof(PropertyBag.Source), nameof(PropertySetWire.SourceRank))]
 internal static partial PropertySetWire ToWire(PropertyBag bag);

 [MapProperty(nameof(QuantityBag.Source), nameof(QuantitySetWire.SourceRank))]
 internal static partial QuantitySetWire ToWire(QuantityBag bag);

 // Existing-target carrier codecs for the MapField members — hand-owned because the SOURCE is a LanguageExt Map:
 // the generator member-maps Map's Keys/Values PROPERTIES onto MapField's read-only Keys/Values collections and the
 // emitted Add throws, while a BCL dictionary source crosses clean, so the fill exists for the Map source shape,
 // never for the get-only target; keys cross as the PropertyName string, values recurse.
 [UserMapping] internal static void ToWire(Map<PropertyName, PropertyValue> values, [MappingTarget] MapField<string, PropertyValueWire> wire) { foreach (var (n, v) in values) { wire[n.Value] = ToWire(v); } }
 [UserMapping] internal static void ToWire(Map<PropertyName, MeasureValue> values, [MappingTarget] MapField<string, MeasureValueWire> wire) { foreach (var (n, m) in values) { wire[n.Value] = ToWire(m); } }
 // The group run keys on the dot-path prefix string (not a PropertyName), and each Option column writes CONDITIONALLY
 // so an unstated qualifier leaves its proto3 optional unset rather than crossing as an empty spelling.
 [UserMapping] internal static void ToWire(Map<string, GroupIdentity> groups, [MappingTarget] MapField<string, GroupIdentityWire> wire) { foreach (var (prefix, group) in groups) { GroupIdentityWire row = new(); group.Discrimination.IfSome(d => row.Discrimination = d); group.Quality.IfSome(q => row.Quality = q); group.Usage.IfSome(u => row.Usage = u); wire[prefix] = row; } }

 [UserMapping] internal static PropertyEvidenceWire ToWire(PropertyEvidence evidence) {
  PropertyEvidenceWire w = new() { Source = evidence.Source, Grade = evidence.Grade.Key };
  evidence.Reference.IfSome(r => w.Reference = r);
  evidence.ValidUntil.IfSome(d => w.ValidUntil = NodaTime.Text.LocalDatePattern.Iso.Format(d));
  evidence.Attested.IfSome(a => w.Attested = new AttestationWire {
   Role = a.Role.Key, Credential = a.Credential, Payload = ToWire(a.Payload.Value), At = a.At.ToTimestamp(),
  });
  evidence.Run.IfSome(run => w.Run = ToWire(run));
  return w;
 }

 [UserMapping] internal static Fin<MeasureBand> ToBand(MeasureBandWire w, Op key) =>
  key.Row<string, UncertaintyKind>(w.Kind).Bind(kind => MeasureBand.Admit(
   kind, w.LowerSi, w.UpperSi,
   Opt(w.HasStandardDeviationSi, w.StandardDeviationSi), Opt(w.HasCoverageFactor, w.CoverageFactor), key));

 internal static PropertyValueWire ToWire(PropertyValue value) => value.Switch<PropertyValueWire>(
  text: v => new() { Text = v.Value },
  measure: v => new() { Measure = ToWire(v.Value) },
  boolean: v => new() { Boolean = v.Value },
  logical: v => { LogicalWire l = new(); v.Value.IfSome(b => l.Value = b); return new() { Logical = l }; },
  enumerated: v => { EnumeratedWire e = new(); e.Selected.AddRange(v.Selected.Map(ToWire)); e.Allowed.AddRange(v.Allowed.Map(ToWire)); return new() { Enumerated = e }; },
  reference: v => { ReferenceWire r = new() { TargetId = v.Target.Value }; v.UsageName.IfSome(u => r.UsageName = u); return new() { Reference = r }; },
  bounded: v => { BoundedWire b = new(); v.Lower.IfSome(m => b.Lower = ToWire(m)); v.Upper.IfSome(m => b.Upper = ToWire(m)); v.SetPoint.IfSome(m => b.SetPoint = ToWire(m)); return new() { Bounded = b }; },
  list: v => { ListWire l = new(); l.Values.AddRange(v.Values.Map(ToWire)); return new() { List = l }; },
  table: v => { TableWire t = new() { Interpolation = v.Interp.Key }; t.Rows.AddRange(v.Rows.Map(r => new TableRowWire { Defining = ToWire(r.Defining), Defined = ToWire(r.Defined) })); return new() { Table = t }; },
  complex: v => { ComplexWire c = new() { UsageName = v.UsageName }; foreach (var (n, inner) in v.Properties) { c.Properties[n.Value] = ToWire(inner); } return new() { Complex = c }; },
  temporal: v => new() { Temporal = v.Value.Switch<TemporalWire>(
   date: static t => new() { Date = NodaTime.Text.LocalDatePattern.Iso.Format(t.Value) },
   moment: static t => new() { Moment = NodaTime.Text.LocalDateTimePattern.ExtendedIso.Format(t.Value) },
   time: static t => new() { Time = NodaTime.Text.LocalTimePattern.ExtendedIso.Format(t.Value) },
   span: static t => new() { Span = NodaTime.Text.PeriodPattern.Roundtrip.Format(t.Value) },
   stamp: static t => new() { Stamp = t.Value.ToTimestamp() }) },
  integer: static v => new() { Integer = ByteString.CopyFrom(v.Value.ToByteArray(isUnsigned: false, isBigEndian: true)) },
  number: static v => new() { Number = v.Value },
  binary: static v => new() { Binary = ByteString.CopyFrom(v.Value.ToArray()) });

 // Build the tree raw off the closed ValueCase, then ONE PropertyValue.Of at the envelope — Of recurses the
 // composites itself, so the structural admission runs exactly once over the whole decoded value.
 internal static Fin<PropertyValue> ToValue(PropertyValueWire w, Op key) => RawValue(w, key).Bind(v => PropertyValue.Of(v, key));

 static Fin<PropertyValue> RawValue(PropertyValueWire w, Op key) => w.ValueCase switch {
  PropertyValueWire.ValueOneofCase.Text => Fin.Succ((PropertyValue)new PropertyValue.Text(w.Text)),
  PropertyValueWire.ValueOneofCase.Measure => ToMeasure(w.Measure, key).Map(static m => (PropertyValue)new PropertyValue.Measure(m)),
  PropertyValueWire.ValueOneofCase.Boolean => Fin.Succ((PropertyValue)new PropertyValue.Boolean(w.Boolean)),
  PropertyValueWire.ValueOneofCase.Logical => Fin.Succ((PropertyValue)new PropertyValue.Logical(Opt(w.Logical.HasValue, w.Logical.Value))),
  PropertyValueWire.ValueOneofCase.Enumerated => toSeq(w.Enumerated.Selected).TraverseM(v => RawValue(v, key)).As().Bind(selected =>
   toSeq(w.Enumerated.Allowed).TraverseM(v => RawValue(v, key)).As().Map(allowed => (PropertyValue)new PropertyValue.Enumerated(selected, allowed))),
  PropertyValueWire.ValueOneofCase.Reference => Fin.Succ((PropertyValue)new PropertyValue.Reference(NodeId.Create(w.Reference.TargetId), Opt(w.Reference.HasUsageName, w.Reference.UsageName))),
  PropertyValueWire.ValueOneofCase.Bounded =>
   (OptMeasure(w.Bounded.Lower, key), OptMeasure(w.Bounded.Upper, key), OptMeasure(w.Bounded.SetPoint, key))
    .Apply(static (lower, upper, setPoint) => (PropertyValue)new PropertyValue.Bounded(lower, upper, setPoint)).As(),
  PropertyValueWire.ValueOneofCase.List => toSeq(w.List.Values).TraverseM(v => RawValue(v, key)).As().Map(vs => (PropertyValue)new PropertyValue.List(vs)),
  PropertyValueWire.ValueOneofCase.Table => key.Row<string, Interpolation>(w.Table.Interpolation)
   .Bind(interp => toSeq(w.Table.Rows).TraverseM(r => RawValue(r.Defining, key).Bind(d => RawValue(r.Defined, key).Map(x => (Defining: d, Defined: x)))).As()
    .Map(rows => (PropertyValue)new PropertyValue.Table(rows, interp))),
  PropertyValueWire.ValueOneofCase.Complex => toSeq(w.Complex.Properties).TraverseM(p =>
   key.AcceptValidated<PropertyName>(p.Key).Bind(name => RawValue(p.Value, key).Map(v => (Name: name, Value: v)))).As()
   .Bind(pairs => Named(pairs, key))
   .Map(properties => (PropertyValue)new PropertyValue.Complex(w.Complex.UsageName, properties)),
  PropertyValueWire.ValueOneofCase.Temporal => ToTemporal(w.Temporal, key).Map(static t => (PropertyValue)new PropertyValue.Temporal(t)),
  PropertyValueWire.ValueOneofCase.Integer => Fin.Succ((PropertyValue)new PropertyValue.Integer(new BigInteger(w.Integer.Span, isUnsigned: false, isBigEndian: true))),
  PropertyValueWire.ValueOneofCase.Number => Fin.Succ((PropertyValue)new PropertyValue.Number(w.Number)),
  PropertyValueWire.ValueOneofCase.Binary => Fin.Succ((PropertyValue)new PropertyValue.Binary(toSeq(w.Binary.ToByteArray()))),
  _ => new KernelFault.InvalidValue("element-wire.property-value", "one value arm is required", Some(key)),
 };

 // TemporalValue arms re-admit through NodaTime ISO patterns (the seam Iso() canon reversed); a malformed
 // token rails the kernel representation refusal, and the epoch stamp rides the Timestamp adapter untouched.
 static Fin<TemporalValue> ToTemporal(TemporalWire w, Op key) => w.ValueCase switch {
  TemporalWire.ValueOneofCase.Date => Iso(NodaTime.Text.LocalDatePattern.Iso, w.Date, key).Map(static v => (TemporalValue)new TemporalValue.Date(v)),
  TemporalWire.ValueOneofCase.Moment => Iso(NodaTime.Text.LocalDateTimePattern.ExtendedIso, w.Moment, key).Map(static v => (TemporalValue)new TemporalValue.Moment(v)),
  TemporalWire.ValueOneofCase.Time => Iso(NodaTime.Text.LocalTimePattern.ExtendedIso, w.Time, key).Map(static v => (TemporalValue)new TemporalValue.Time(v)),
  TemporalWire.ValueOneofCase.Span => Iso(NodaTime.Text.PeriodPattern.Roundtrip, w.Span, key).Map(static v => (TemporalValue)new TemporalValue.Span(v)),
  TemporalWire.ValueOneofCase.Stamp => Fin.Succ((TemporalValue)new TemporalValue.Stamp(w.Stamp.ToInstant())),
  _ => new KernelFault.InvalidValue("element-wire.temporal", "one temporal arm is required", Some(key)),
 };

 // The evidence envelope re-admits through the OWNER's total Of — grade absent (an elder payload) reads Catalogue,
 // the roster's floor and the owner's own defaulted-struct state, never a guessed rank; a present rank re-crosses
 // the generated int-key gate the bag SourceRank column shares.
 static Fin<PropertyEvidence> ToEvidence(PropertyEvidenceWire? w, Op key) =>
  from row in Present(w, "property-set.evidence", key)
  from validUntil in ToDate(row.HasValidUntil, row.ValidUntil, key)
  from grade in Opt(row.HasGrade, row.Grade).Traverse(rank => key.Row<int, EvidenceGrade>(rank)).As()
  from attested in Optional(row.Attested).Traverse(a => ToAttestation(a, key)).As()
  from run in Optional(row.Run).Traverse(r => ToEvidenceRun(r, key)).As()
  select PropertyEvidence.Of(row.Source, grade.IfNone(EvidenceGrade.Catalogue),
    Opt(row.Reference.Length > 0, row.Reference), validUntil, attested, run);

 static Fin<Attestation> ToAttestation(AttestationWire w, Op key) =>
  from role in key.Row<string, AttestationRole>(w.Role)
  from at in Present(w.At, "attestation.at", key)
  select new Attestation(role, w.Credential, ContentAddress.Of(ToKey(w.Payload)), at.ToInstant());

 static Fin<PropertyBag> ToBag(PropertySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   ToValueMap(w.Values, key).Map(values => new PropertyBag(w.SetName, values, axes.Mode, axes.Rank)));

 static Fin<QuantityBag> ToBag(QuantitySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   toSeq(w.Values).TraverseM(p =>
    key.AcceptValidated<PropertyName>(p.Key).Bind(name => ToMeasure(p.Value, key).Map(m => (Name: name, Value: m)))).As()
    .Bind(pairs => Named(pairs, key))
    .Map(values => new QuantityBag(w.SetName, values, axes.Mode, axes.Rank, ToGroups(w.Groups))));

 // The group run re-admits TOTAL: the three columns are free grouping text under no seam gate, so absence is the
 // whole decision each Has* presence pair answers and no rail is owed. A prefix naming no value row is admitted —
 // an authored group whose members a partial crossing omitted is data, not a malformed payload. The dot-path keys
 // are bare ORDINAL strings on both sides, so the parser-deduped run lands whole through toMap.
 static Map<string, GroupIdentity> ToGroups(IEnumerable<KeyValuePair<string, GroupIdentityWire>> entries) =>
  toMap(toSeq(entries).Map(static entry => (entry.Key, new GroupIdentity(
   Opt(entry.Value.HasDiscrimination, entry.Value.Discrimination),
   Opt(entry.Value.HasQuality, entry.Value.Quality),
   Opt(entry.Value.HasUsage, entry.Value.Usage)))));

 static Fin<(InheritanceMode Mode, EvidenceGrade Rank)> BagAxes(string inheritance, int sourceRank, Op key) =>
  (key.Row<string, InheritanceMode>(inheritance), key.Row<int, EvidenceGrade>(sourceRank))
   .Apply(static (mode, rank) => (mode, rank)).As().ToFin();

 static Fin<Map<PropertyName, PropertyValue>> ToValueMap(IEnumerable<KeyValuePair<string, PropertyValueWire>> entries, Op key) =>
  toSeq(entries).TraverseM(p =>
   key.AcceptValidated<PropertyName>(p.Key).Bind(name => ToValue(p.Value, key).Map(v => (Name: name, Value: v)))).As()
   .Bind(pairs => Named(pairs, key));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
