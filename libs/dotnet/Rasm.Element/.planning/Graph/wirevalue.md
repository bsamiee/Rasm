# [ELEMENT_WIRE_VALUE]

`WireCodec`'s typed-value plane projects the fourteen-case recursive value, five temporal leaves, measures, named bag rows, grouping rows, and evidence envelope directly onto generated messages.

## [01]-[INDEX]

- [02]-[VALUE_CODEC]: recursive `PropertyValue`/`TemporalValue` folds, the measure and bag codecs, and the `PropertyEvidence`/`Attestation` envelope.

## [02]-[VALUE_CODEC]

- Cases: `PropertyValue` 14 arms and `TemporalValue` 5 arms — census rows [02]/[03] at `Graph/wire#NODE_CODEC`.
- Law: generated repeated named rows stay sorted at encode and re-enter the domain's normalized uniqueness gate at decode.
- Law: Node references cross as sixteen bytes; enum correspondence is explicit and never derives through string formatting.
- Law: calendar date, local moment, local time, and instant use their generated message types; calendar `Period` retains its lossless ISO spelling.
- Packages: Google.Protobuf, Mapperly, NodaTime.Serialization.Protobuf, LanguageExt, and Thinktecture compose the generated support closure coordinated at `Graph/wire#NODE_CODEC`.
- Growth: a new column is one append-only corpus field and one transcription member; a new union case also updates the `CrossingFamily` arm count so the parity census rejects a half-landed pair.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Numerics;
using System.Diagnostics;
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.Contracts.Element;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class WireCodec {
 internal static PropertySetWire ToWire(PropertyBag bag) {
  PropertySetWire wire = new() {
   SetName = bag.SetName,
   Inheritance = ToWire(bag.Inheritance),
   SourceRank = ToWire(bag.Source),
  };
  wire.Values.AddRange(bag.Values.OrderBy(static pair => pair.Key.Value, StringComparer.Ordinal)
   .Select(static pair => new NamedValueWire { Name = pair.Key.Value, Value = ToWire(pair.Value) }));
  return wire;
 }

 internal static QuantitySetWire ToWire(QuantityBag bag) {
  QuantitySetWire wire = new() {
   SetName = bag.SetName,
   Inheritance = ToWire(bag.Inheritance),
   SourceRank = ToWire(bag.Source),
  };
  wire.Values.AddRange(bag.Values.OrderBy(static pair => pair.Key.Value, StringComparer.Ordinal)
   .Select(static pair => new NamedMeasureWire { Name = pair.Key.Value, Value = ToWire(pair.Value) }));
  wire.Groups.AddRange(bag.Groups.OrderBy(static pair => pair.Key, StringComparer.Ordinal).Select(static pair => {
   GroupIdentityWire identity = new();
   pair.Value.Discrimination.IfSome(value => identity.Discrimination = value);
   pair.Value.Quality.IfSome(value => identity.Quality = value);
   pair.Value.Usage.IfSome(value => identity.Usage = value);
   return new GroupWire { Prefix = pair.Key, Identity = identity };
  }));
  return wire;
 }

 internal static PropertyEvidenceWire ToWire(PropertyEvidence evidence) {
  PropertyEvidenceWire w = new() { Source = evidence.Source, Grade = ToWire(evidence.Grade) };
  evidence.Reference.IfSome(r => w.Reference = r);
  evidence.ValidUntil.IfSome(d => w.ValidUntil = NodaTime.Text.LocalDatePattern.Iso.Format(d));
  evidence.Attested.IfSome(a => w.Attested = new AttestationWire {
   Role = ToWire(a.Role), Credential = a.Credential, Payload = ToWire(a.Payload.Value), At = a.At.ToTimestamp(),
  });
  evidence.Run.IfSome(run => w.Run = ToWire(run));
  return w;
 }

 internal static Fin<MeasureBand> ToBand(MeasureBandWire w, Op key) =>
  ToMeasureBand(w, key);

 internal static PropertyValueWire ToWire(PropertyValue value) => value.Switch<PropertyValueWire>(
  text: v => new() { Text = v.Value },
  measure: v => new() { Measure = ToWire(v.Value) },
  boolean: v => new() { Boolean = v.Value },
  logical: v => { LogicalWire l = new(); v.Value.IfSome(b => l.Value = b); return new() { Logical = l }; },
  enumerated: v => { EnumeratedWire e = new(); e.Selected.AddRange(v.Selected.Map(ToWire)); e.Allowed.AddRange(v.Allowed.Map(ToWire)); return new() { Enumerated = e }; },
  reference: v => { ReferenceWire r = new() { Target = ToWire(v.Target) }; v.UsageName.IfSome(u => r.UsageName = u); return new() { Reference = r }; },
  bounded: v => { BoundedWire b = new(); v.Lower.IfSome(m => b.Lower = ToWire(m)); v.Upper.IfSome(m => b.Upper = ToWire(m)); v.SetPoint.IfSome(m => b.SetPoint = ToWire(m)); return new() { Bounded = b }; },
  list: v => { ListWire l = new(); l.Values.AddRange(v.Values.Map(ToWire)); return new() { List = l }; },
  table: v => { TableWire t = new() { Interpolation = ToWire(v.Interp) }; t.Rows.AddRange(v.Rows.Map(r => new TableRowWire { Defining = ToWire(r.Defining), Defined = ToWire(r.Defined) })); return new() { Table = t }; },
  complex: v => { ComplexWire c = new() { UsageName = v.UsageName }; c.Properties.AddRange(v.Properties.OrderBy(static pair => pair.Key.Value, StringComparer.Ordinal).Select(static pair => new NamedValueWire { Name = pair.Key.Value, Value = ToWire(pair.Value) })); return new() { Complex = c }; },
  temporal: v => new() { Temporal = v.Value.Switch<TemporalWire>(
   date: static t => new() { Date = t.Value.ToDate() },
   moment: static t => new() { Moment = ToWire(t.Value) },
   time: static t => new() { Time = t.Value.ToTimeOfDay() },
   span: static t => new() { Span = NodaTime.Text.PeriodPattern.Roundtrip.Format(t.Value) },
   stamp: static t => new() { Stamp = t.Value.ToTimestamp() }) },
  integer: static v => new() { Integer = ByteString.CopyFrom(v.Value.ToByteArray(isUnsigned: false, isBigEndian: true)) },
  number: static v => new() { Number = v.Value },
  binary: static v => new() { Binary = ByteString.CopyFrom(v.Value.ToArray()) });

 internal static Fin<PropertyValue> ToValue(PropertyValueWire w, Op key) => RawValue(w, key).Bind(v => PropertyValue.Of(v, key));

 static Fin<PropertyValue> RawValue(PropertyValueWire w, Op key) => w.ValueCase switch {
  PropertyValueWire.ValueOneofCase.Text => Fin.Succ((PropertyValue)new PropertyValue.Text(w.Text)),
  PropertyValueWire.ValueOneofCase.Measure => ToMeasure(w.Measure, key).Map(static m => (PropertyValue)new PropertyValue.Measure(m)),
  PropertyValueWire.ValueOneofCase.Boolean => Fin.Succ((PropertyValue)new PropertyValue.Boolean(w.Boolean)),
  PropertyValueWire.ValueOneofCase.Logical => Fin.Succ((PropertyValue)new PropertyValue.Logical(Opt(w.Logical.HasValue, w.Logical.Value))),
  PropertyValueWire.ValueOneofCase.Enumerated => toSeq(w.Enumerated.Selected).TraverseM(v => RawValue(v, key)).As().Bind(selected =>
   toSeq(w.Enumerated.Allowed).TraverseM(v => RawValue(v, key)).As().Map(allowed => (PropertyValue)new PropertyValue.Enumerated(selected, allowed))),
  PropertyValueWire.ValueOneofCase.Reference => ToNodeId(w.Reference.Target, key).Map(target => (PropertyValue)new PropertyValue.Reference(target, Opt(w.Reference.HasUsageName, w.Reference.UsageName))),
  PropertyValueWire.ValueOneofCase.Bounded =>
   (OptMeasure(w.Bounded.Lower, key), OptMeasure(w.Bounded.Upper, key), OptMeasure(w.Bounded.SetPoint, key))
    .Apply(static (lower, upper, setPoint) => (PropertyValue)new PropertyValue.Bounded(lower, upper, setPoint)).As(),
  PropertyValueWire.ValueOneofCase.List => toSeq(w.List.Values).TraverseM(v => RawValue(v, key)).As().Map(vs => (PropertyValue)new PropertyValue.List(vs)),
  PropertyValueWire.ValueOneofCase.Table => ToInterpolation(w.Table.Interpolation, key)
   .Bind(interp => toSeq(w.Table.Rows).TraverseM(r => RawValue(r.Defining, key).Bind(d => RawValue(r.Defined, key).Map(x => (Defining: d, Defined: x)))).As()
    .Map(rows => (PropertyValue)new PropertyValue.Table(rows, interp))),
  PropertyValueWire.ValueOneofCase.Complex => toSeq(w.Complex.Properties).TraverseM(p =>
   key.AcceptValidated<PropertyName>(p.Name).Bind(name => RawValue(p.Value, key).Map(v => (Name: name, Value: v)))).As()
   .Bind(pairs => Named(pairs, key))
   .Map(properties => (PropertyValue)new PropertyValue.Complex(w.Complex.UsageName, properties)),
  PropertyValueWire.ValueOneofCase.Temporal => ToTemporal(w.Temporal, key).Map(static t => (PropertyValue)new PropertyValue.Temporal(t)),
  PropertyValueWire.ValueOneofCase.Integer => ToInteger(w.Integer, key),
  PropertyValueWire.ValueOneofCase.Number => Fin.Succ((PropertyValue)new PropertyValue.Number(w.Number)),
  PropertyValueWire.ValueOneofCase.Binary => Fin.Succ((PropertyValue)new PropertyValue.Binary(toSeq(w.Binary.ToByteArray()))),
 _ => new KernelFault.InvalidValue("element-wire.property-value", "one value arm is required", Some(key)),
 };

 static Fin<PropertyValue> ToInteger(ByteString bytes, Op key) {
  BigInteger value = new(bytes.Span, isUnsigned: false, isBigEndian: true);
  byte[] canonical = value.ToByteArray(isUnsigned: false, isBigEndian: true);
  return bytes.Span.SequenceEqual(canonical)
   ? Fin.Succ((PropertyValue)new PropertyValue.Integer(value))
   : Fin.Fail<PropertyValue>(new KernelFault.InvalidValue(
    "element-wire.property-value.integer", "use minimal-width two's-complement big-endian bytes", Some(key)));
 }

 static Fin<TemporalValue> ToTemporal(TemporalWire w, Op key) => w.ValueCase switch {
  TemporalWire.ValueOneofCase.Date => key.Catch(() => Fin.Succ((TemporalValue)new TemporalValue.Date(w.Date.ToLocalDate()))),
  TemporalWire.ValueOneofCase.Moment => ToMoment(w.Moment, key).Map(static v => (TemporalValue)new TemporalValue.Moment(v)),
  TemporalWire.ValueOneofCase.Time => key.Catch(() => Fin.Succ((TemporalValue)new TemporalValue.Time(w.Time.ToLocalTime()))),
  TemporalWire.ValueOneofCase.Span => Iso(NodaTime.Text.PeriodPattern.Roundtrip, w.Span, key).Map(static v => (TemporalValue)new TemporalValue.Span(v)),
  TemporalWire.ValueOneofCase.Stamp => Fin.Succ((TemporalValue)new TemporalValue.Stamp(w.Stamp.ToInstant())),
  _ => new KernelFault.InvalidValue("element-wire.temporal", "one temporal arm is required", Some(key)),
 };

 static Google.Type.DateTime ToWire(NodaTime.LocalDateTime value) => new() {
  Year = value.Year,
  Month = value.Month,
  Day = value.Day,
  Hours = value.Hour,
  Minutes = value.Minute,
  Seconds = value.Second,
  Nanos = value.NanosecondOfSecond,
 };

 static Fin<NodaTime.LocalDateTime> ToMoment(Google.Type.DateTime value, Op key) =>
  value.TimeOffsetCase == Google.Type.DateTime.TimeOffsetOneofCase.None
   ? key.Catch(() => Fin.Succ(new NodaTime.LocalDateTime(
      value.Year, value.Month, value.Day, value.Hours, value.Minutes, value.Seconds)
     .PlusNanoseconds(value.Nanos)))
   : Fin.Fail<NodaTime.LocalDateTime>(new KernelFault.InvalidValue(
    "element-wire.temporal.moment", "carry a local moment without an offset or time zone", Some(key)));

 static Fin<PropertyEvidence> ToEvidence(PropertyEvidenceWire? w, Op key) =>
  from row in Present(w, "property-set.evidence", key)
  from validUntil in ToDate(row.HasValidUntil, row.ValidUntil, key)
  from grade in Opt(row.HasGrade, row.Grade).Traverse(rank => ToEvidenceGrade(rank, key)).As()
  from attested in Optional(row.Attested).Traverse(a => ToAttestation(a, key)).As()
  from run in Optional(row.Run).Traverse(r => ToEvidenceRun(r, key)).As()
  select PropertyEvidence.Of(row.Source, grade.IfNone(EvidenceGrade.Catalogue),
    Opt(row.Reference.Length > 0, row.Reference), validUntil, attested, run);

 static Fin<Attestation> ToAttestation(AttestationWire w, Op key) =>
  from role in ToAttestationRole(w.Role, key)
  from at in Present(w.At, "attestation.at", key)
  from payload in ToKey(w.Payload, key)
  select new Attestation(role, w.Credential, ContentAddress.Of(payload), at.ToInstant());

 static Fin<PropertyBag> ToBag(PropertySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   ToValueMap(w.Values, key).Map(values => new PropertyBag(w.SetName, values, axes.Mode, axes.Rank)));

 static Fin<QuantityBag> ToBag(QuantitySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   toSeq(w.Values).TraverseM(p =>
    key.AcceptValidated<PropertyName>(p.Name).Bind(name => ToMeasure(p.Value, key).Map(m => (Name: name, Value: m)))).As()
   .Bind(pairs => Named(pairs, key))
    .Bind(values => ToGroups(w.Groups, key).Map(groups => new QuantityBag(w.SetName, values, axes.Mode, axes.Rank, groups))));

 static Fin<Map<string, GroupIdentity>> ToGroups(IEnumerable<GroupWire> entries, Op key) =>
  UniqueMap(toSeq(entries).Map(static row => (Key: row.Prefix, Value: new GroupIdentity(
   Opt(row.Identity.HasDiscrimination, row.Identity.Discrimination),
   Opt(row.Identity.HasQuality, row.Identity.Quality),
   Opt(row.Identity.HasUsage, row.Identity.Usage)))), "quantity-set.groups", key);

 static Fin<(InheritanceMode Mode, EvidenceGrade Rank)> BagAxes(
  Rasm.Contracts.Element.InheritanceMode inheritance,
  Rasm.Contracts.Element.EvidenceGrade sourceRank,
  Op key) =>
  (ToInheritance(inheritance, key), ToEvidenceGrade(sourceRank, key))
   .Apply(static (mode, rank) => (mode, rank)).As().ToFin();

 static Fin<Map<PropertyName, PropertyValue>> ToValueMap(IEnumerable<NamedValueWire> entries, Op key) =>
  toSeq(entries).TraverseM(p =>
   key.AcceptValidated<PropertyName>(p.Name).Bind(name => ToValue(p.Value, key).Map(v => (Name: name, Value: v)))).As()
   .Bind(pairs => Named(pairs, key));

 static Rasm.Contracts.Element.InheritanceMode ToWire(InheritanceMode value) => value == InheritanceMode.OccurrenceWins
  ? Rasm.Contracts.Element.InheritanceMode.OccurrenceWins
  : value == InheritanceMode.TypeDrivenOverride
   ? Rasm.Contracts.Element.InheritanceMode.TypeDrivenOverride
   : value == InheritanceMode.TypeDrivenOnly
    ? Rasm.Contracts.Element.InheritanceMode.TypeDrivenOnly
    : throw new UnreachableException();

 static Fin<InheritanceMode> ToInheritance(Rasm.Contracts.Element.InheritanceMode value, Op key) => value switch {
  Rasm.Contracts.Element.InheritanceMode.OccurrenceWins => Fin.Succ(InheritanceMode.OccurrenceWins),
  Rasm.Contracts.Element.InheritanceMode.TypeDrivenOverride => Fin.Succ(InheritanceMode.TypeDrivenOverride),
  Rasm.Contracts.Element.InheritanceMode.TypeDrivenOnly => Fin.Succ(InheritanceMode.TypeDrivenOnly),
  _ => Fin.Fail<InheritanceMode>(key.InvalidInput(nameof(PropertySetWire.Inheritance))),
 };

 static Rasm.Contracts.Element.EvidenceGrade ToWire(EvidenceGrade value) => value.Key switch {
  10 => Rasm.Contracts.Element.EvidenceGrade.Catalogue,
  15 => Rasm.Contracts.Element.EvidenceGrade.Defined,
  20 => Rasm.Contracts.Element.EvidenceGrade.Import,
  25 => Rasm.Contracts.Element.EvidenceGrade.Measured,
  30 => Rasm.Contracts.Element.EvidenceGrade.Derived,
  40 => Rasm.Contracts.Element.EvidenceGrade.User,
  _ => throw new UnreachableException(),
 };

 static Fin<EvidenceGrade> ToEvidenceGrade(Rasm.Contracts.Element.EvidenceGrade value, Op key) => value switch {
  Rasm.Contracts.Element.EvidenceGrade.Catalogue => Fin.Succ(EvidenceGrade.Catalogue),
  Rasm.Contracts.Element.EvidenceGrade.Defined => Fin.Succ(EvidenceGrade.Defined),
  Rasm.Contracts.Element.EvidenceGrade.Import => Fin.Succ(EvidenceGrade.Import),
  Rasm.Contracts.Element.EvidenceGrade.Measured => Fin.Succ(EvidenceGrade.Measured),
  Rasm.Contracts.Element.EvidenceGrade.Derived => Fin.Succ(EvidenceGrade.Derived),
  Rasm.Contracts.Element.EvidenceGrade.User => Fin.Succ(EvidenceGrade.User),
  _ => Fin.Fail<EvidenceGrade>(key.InvalidInput(nameof(PropertySetWire.SourceRank))),
 };

 static Rasm.Contracts.Element.Interpolation ToWire(Interpolation value) => value == Interpolation.NotDefined
  ? Rasm.Contracts.Element.Interpolation.NotDefined
  : value == Interpolation.Linear
   ? Rasm.Contracts.Element.Interpolation.Linear
   : value == Interpolation.LogLinear
    ? Rasm.Contracts.Element.Interpolation.LogLinear
    : value == Interpolation.LogLog
     ? Rasm.Contracts.Element.Interpolation.LogLog
     : throw new UnreachableException();

 static Fin<Interpolation> ToInterpolation(Rasm.Contracts.Element.Interpolation value, Op key) => value switch {
  Rasm.Contracts.Element.Interpolation.NotDefined => Fin.Succ(Interpolation.NotDefined),
  Rasm.Contracts.Element.Interpolation.Linear => Fin.Succ(Interpolation.Linear),
  Rasm.Contracts.Element.Interpolation.LogLinear => Fin.Succ(Interpolation.LogLinear),
  Rasm.Contracts.Element.Interpolation.LogLog => Fin.Succ(Interpolation.LogLog),
  _ => Fin.Fail<Interpolation>(key.InvalidInput(nameof(TableWire.Interpolation))),
 };

 static Rasm.Contracts.Element.AttestationRole ToWire(AttestationRole value) => value == AttestationRole.Manufacturer
  ? Rasm.Contracts.Element.AttestationRole.Manufacturer
  : value == AttestationRole.ManufacturerAuthorized
   ? Rasm.Contracts.Element.AttestationRole.ManufacturerAuthorized
   : value == AttestationRole.Purchaser
    ? Rasm.Contracts.Element.AttestationRole.Purchaser
    : value == AttestationRole.Independent
     ? Rasm.Contracts.Element.AttestationRole.Independent
     : value == AttestationRole.Quality
      ? Rasm.Contracts.Element.AttestationRole.Quality
      : value == AttestationRole.Regulator
       ? Rasm.Contracts.Element.AttestationRole.Regulator
       : value == AttestationRole.WeldingInspector
        ? Rasm.Contracts.Element.AttestationRole.WeldingInspector
        : value == AttestationRole.CalibrationLaboratory
         ? Rasm.Contracts.Element.AttestationRole.CalibrationLaboratory
         : value == AttestationRole.MaterialReviewBoard
          ? Rasm.Contracts.Element.AttestationRole.MaterialReviewBoard
          : value == AttestationRole.SustainabilityVerifier
           ? Rasm.Contracts.Element.AttestationRole.SustainabilityVerifier
           : throw new UnreachableException();

 static Fin<AttestationRole> ToAttestationRole(Rasm.Contracts.Element.AttestationRole value, Op key) => value switch {
  Rasm.Contracts.Element.AttestationRole.Manufacturer => Fin.Succ(AttestationRole.Manufacturer),
  Rasm.Contracts.Element.AttestationRole.ManufacturerAuthorized => Fin.Succ(AttestationRole.ManufacturerAuthorized),
  Rasm.Contracts.Element.AttestationRole.Purchaser => Fin.Succ(AttestationRole.Purchaser),
  Rasm.Contracts.Element.AttestationRole.Independent => Fin.Succ(AttestationRole.Independent),
  Rasm.Contracts.Element.AttestationRole.Quality => Fin.Succ(AttestationRole.Quality),
  Rasm.Contracts.Element.AttestationRole.Regulator => Fin.Succ(AttestationRole.Regulator),
  Rasm.Contracts.Element.AttestationRole.WeldingInspector => Fin.Succ(AttestationRole.WeldingInspector),
  Rasm.Contracts.Element.AttestationRole.CalibrationLaboratory => Fin.Succ(AttestationRole.CalibrationLaboratory),
  Rasm.Contracts.Element.AttestationRole.MaterialReviewBoard => Fin.Succ(AttestationRole.MaterialReviewBoard),
  Rasm.Contracts.Element.AttestationRole.SustainabilityVerifier => Fin.Succ(AttestationRole.SustainabilityVerifier),
  _ => Fin.Fail<AttestationRole>(key.InvalidInput(nameof(AttestationWire.Role))),
 };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
