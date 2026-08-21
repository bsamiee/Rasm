# [ELEMENT_PROPERTY]

`PropertyValue` closes the IFC-value family (`Text`/`Measure`/`Boolean`/`Logical`/`Integer`/`Number`/`Binary`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex`/`Temporal`) as one `[Union]`, so a property carries its data type rather than a stringly value; `PropertyName` keys it, the ONE `ValueBag<V>` generic carries it under the `PropertyBag`/`QuantityBag` aliases, and `InheritanceMode` owns the type→occurrence precedence fold.

`Rasm.Bim` keeps the IFC `Pset_*` roster, bSDD template resolution, geometry-true base-quantity derivation, and `IfcRelDefinesByProperties` round-trip. `DetailSchema` pins the neutral detail-bag and takeoff-quantity seams. `PropertyValue.Of` rails malformed scalar or composite values, and `CanonicalBytes` preserves every scalar discriminant so the same rendering never aliases different typed evidence.

## [01]-[INDEX]

- [02]-[PROPERTY_VALUE]: `PropertyValue` `[Union]` typed IFC-value family, `PropertyName` key, `Interpolation` table-curve rule, `TemporalValue` NodaTime-carried temporal leaf family, the fallible `Of` structural admission, the recursive `Remap` node-id rewrite and its `References` reachability dual the `Relations/relation#EDGE_ALGEBRA` `Generic` edge composes (renumber and cascade in lockstep), and the canonical `Render`/`CanonicalBytes` folds.
- [03]-[PROPERTY_BAG]: `ValueBag<V>` the ONE named inheritance-stamped value bag (`PropertyBag`/`QuantityBag` aliases), `InheritanceMode` `[SmartEnum]` owning the generic `Resolve<V>` precedence algebra, `EvidenceGrade` rank, `GroupIdentity` dot-path group axis, and the type→occurrence `Merge` the `Bake` applies.
- [04]-[DETAIL_SCHEMA]: `DetailSchema` the ONE neutral schema over the bag aliases — the neutral `SetName`s the `Rasm.Bim` egress maps to IFC Psets, the stamped precedence, the `JointType` allowed-set, the canonical detail and takeoff `PropertyName` vocabulary, and the conforming `Bag`/`Quantities`/`Joint` factories.

## [02]-[PROPERTY_VALUE]

- Owner: `PropertyValue` the `[Union]` typed IFC-value family; `PropertyName` the `[ValueObject<string>]` property key; `Interpolation` the table-curve rule; `TemporalValue` the NodaTime temporal leaf family; the closed fourteen-case value vocabulary a property carries.
- Cases: `Text` (verbatim string) · `Measure` (SI-coerced `MeasureValue`) · `Boolean` (strict two-valued) · `Logical` (three-valued) · `Integer` (unbounded signed integer) · `Number` (finite IEEE-754 real) · `Binary` (byte-exact payload) · `Enumerated` (selected and allowed typed scalar members) · `Reference` (target and optional usage) · `Bounded` (lower/upper/setpoint measures) · `List` (ordered recursive values) · `Table` (defining→defined rows and interpolation) · `Complex` (named sub-properties) · `Temporal` (`Date`/`Moment`/`Time`/`Span`/`Stamp`). `PropertyValue` preserves the full `IfcValue` scalar family and the structured property forms without stringification.
- Entry: `PropertyValue.Of(value, key)` is the fallible admission a raw author crosses — returning `KernelFault.OutOfRange` for a non-finite `Number` and `ElementFault.ValueRejected` for an empty/cross-type/inverted `Bounded`, a non-subset or composite-membered `Enumerated`, an empty `Table`, or an empty `Complex`, and recursively re-admitting nested values. `Integer` carries unbounded `BigInteger`, `Number` carries finite IEEE-754, and `Binary` carries byte-exact `Seq<byte>`; none collapse to `Text`.
- Auto: `Render` dispatches the generated total `Switch` — `Text` verbatim, `Measure` the SI magnitude and canonical unit, `Boolean`/`Logical` `TRUE`/`FALSE`(/`UNKNOWN`), `Enumerated` the recursive selected-member join, `Reference` the target id, `Bounded` the `[lower, upper, setpoint]` interval, `List`/`Table` the recursive join, `Complex` the `usage{name=value;…}` named-bag join, `Temporal` the ISO-8601 token — one projection, never a per-case consumer branch; `CanonicalBytes` writes the case ordinal then the payload (a `Measure` quantized to tolerance, the `Logical` a presence bit and the bool, an `Enumerated` member through its own typed `CanonicalBytes` so two members sharing one text spelling under different types hash apart, a `Temporal` its arm ordinal and ISO token, every collection count-prefixed so the encoding is injective, the `Complex` sub-properties name-sorted `Ordinal`) so the content key is byte-stable across runtimes.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch` the `Of`/`Render`/`CanonicalBytes`/`Remap` folds dispatch, `[ValueObject<string>]`/`[SmartEnum<string>]`/`ComparerAccessors`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Map` + the `Seq.Choose`/`Seq.TraverseM`/`Map.Fold`/`Option.Match` combinators the `Of` admission composes), `Rasm` (the kernel `Op` op-key), `Projection/fault#FAULT_BAND` (`ElementFault.ValueRejected`).
- Growth: a new IFC value kind is one `PropertyValue` arm carrying its payload; a new table-curve rule is one `Interpolation` row; a recursive composite rides the existing `List`/`Table`/`Complex` arms; never a per-Pset value type, never a stringly-typed value field, and a raw `string` property key crossing a bag is the named defect.
- Boundary: `PropertyValue` is the ONE typed value owner — the migration `PropertyBinding(string SetName, string Name, string Value)`/`QuantityBinding(string, string, double, string)` stringly tuples are the deleted form, and the IFC-dataType narrowing (`IfcLengthMeasure`→`Measure`, `IfcLogical`→`Logical`, `IfcBoolean`→`Boolean`) is the `Rasm.Bim` projector's at ingest, so a `Pset_*` name or an `IfcValue` type string never crosses a seam signature; `Boolean` is strict two-valued and `Logical` three-valued (`None` = `UNKNOWN`, never silently coerced to `false`); `Enumerated` carries the SELECTED set so a multi-value property is never truncated to one value (an empty `Selected` is the unset `OPTIONAL` `EnumerationValues` state, admitted), its members TYPED `PropertyValue` scalars so an `IfcValue`-typed enumeration member (a measured tolerance class, a numeric grade) keeps its discriminant, membership compares by typed record equality, and the canonical bytes separate same-text different-type members — the `Seq<string>` member narrowing that stringified the IFC value domain is the deleted form; `Temporal` carries the `IfcDate`/`IfcDateTime`/`IfcTime`/`IfcDuration`/`IfcTimeStamp` leaves as NodaTime values (a date-valued Pset row crossing as `Text` — losing the typed read and the calendar comparison a durability/procurement filter folds on — is the deleted form), the ONE ISO-8601 `Iso()` projection serving render and hash; `Reference` carries a `NodeId` resolved through the `Graph/element#ELEMENT_GRAPH` `Nodes` index, never a raw GlobalId string; `Table` carries its `Interpolation` rule so a lookup-table consumer reads the curve semantics rather than re-inferring them; `List`/`Table`/`Complex` are the closed composite forms, so a nested property never needs a parallel container type; `PropertyValue.Of` is the ONE fallible admission gating a value into a bag (a per-arm validating factory family or an unvalidated composite crossing a bag is the deleted form), its recursion runtime-stack-bounded — hostile nesting depth is the wire admission's depth gate, the `Graph/wire#WIRE_CODEC` `CodedInputStream.CreateWithLimits` recursion bound on `PropertyValueWire` decode, never a seam re-check; the `Bounded` structural law is exactly the single-`QuantityType` guard and the ONE present lower/upper ordering — the setpoint is a free nominal the fence's `AdmitBounded` pins, and constraining it inside the interval rejects legal IFC.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// PropertyBag and QuantityBag alias the ONE generic ValueBag<V> owner GLOBALLY — declared package-wide so the
// Node.PropertySet/QuantitySet cases, the Bake merge, and the Rasm.Bim projector all resolve the alias without a
// per-file restatement; global usings precede the ordinary directives by language law (CS8915).
global using PropertyBag = Rasm.Element.Properties.ValueBag<Rasm.Element.Properties.PropertyValue>;
global using QuantityBag = Rasm.Element.Properties.ValueBag<Rasm.Element.Properties.MeasureValue>;

using System.Globalization;
using System.Numerics;
using Generator.Equals;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Properties;

// --- [TYPES] ------------------------------------------------------------------------------
// BOTH comparer axes declare one policy: the ordered Map<PropertyName, V> resolves keys through the comparison axis,
// so an equality-only declaration would let a culture-default CompareTo miss a key equality calls equal.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PropertyName {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim();
  validationError = value.Length == 0 ? new ValidationError("<property-name-blank>") : validationError;
 }
}

// Interpolation rules the lookup-table curve a Table value carries (the neutral IfcCurveInterpolationEnum mirror)
// as a wire-keyed token ONLY — the kernel deleted its keyed InterpolationRoute roster for the typed Interpolant
// factories, whose per-row capability types no keyed roster can return, so the consumer maps token onto factory
// at ITS edge (Linear -> Interpolant.Linear, LogLinear -> Interpolant.LogLinear; LogLog is the declared MathNet
// gap — an OfTransformed ln/exp composition over Linear at the consumer).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Interpolation {
 public static readonly Interpolation NotDefined = new("notdefined");
 public static readonly Interpolation Linear = new("linear");
 public static readonly Interpolation LogLinear = new("log-linear");
 public static readonly Interpolation LogLog = new("log-log");
}

// IFC temporal leaves as NodaTime values — compared on the calendar, never as string spellings. Iso() is the
// ONE canonical projection render and hash share; CaseOrdinal separates arms whose grammars overlap.
[Union]
public abstract partial record TemporalValue {
 private TemporalValue() { }

 public sealed record Date(LocalDate Value) : TemporalValue;
 public sealed record Moment(LocalDateTime Value) : TemporalValue;
 public sealed record Time(LocalTime Value) : TemporalValue;
 public sealed record Span(Period Value) : TemporalValue;
 public sealed record Stamp(Instant Value) : TemporalValue;

 public int CaseOrdinal => Map(date: 0, moment: 1, time: 2, span: 3, stamp: 4);

 public string Iso() => Switch(
  date: static t => LocalDatePattern.Iso.Format(t.Value),
  moment: static t => LocalDateTimePattern.ExtendedIso.Format(t.Value),
  time: static t => LocalTimePattern.ExtendedIso.Format(t.Value),
  span: static t => PeriodPattern.Roundtrip.Format(t.Value),
  stamp: static t => InstantPattern.ExtendedIso.Format(t.Value));
}

[Union]
public abstract partial record PropertyValue {
 private PropertyValue() { }

 public sealed record Text(string Value) : PropertyValue;
 public sealed record Measure(MeasureValue Value) : PropertyValue;
 public sealed record Boolean(bool Value) : PropertyValue;
 public sealed record Logical(Option<bool> Value) : PropertyValue;
 public sealed record Integer(BigInteger Value) : PropertyValue;
 public sealed record Number(double Value) : PropertyValue;
 public sealed record Binary(Seq<byte> Value) : PropertyValue;
 // Typed members, never Seq<string>: an IfcValue-typed enumeration member (measured, numeric, boolean, temporal)
 // keeps its discriminant; the Of admission closes membership to the SCALAR arms.
 public sealed record Enumerated(Seq<PropertyValue> Selected, Seq<PropertyValue> Allowed) : PropertyValue;
 public sealed record Reference(NodeId Target, Option<string> UsageName = default) : PropertyValue;
 public sealed record Bounded(Option<MeasureValue> Lower, Option<MeasureValue> Upper, Option<MeasureValue> SetPoint) : PropertyValue;
 public sealed record List(Seq<PropertyValue> Values) : PropertyValue;
 public sealed record Table(Seq<(PropertyValue Defining, PropertyValue Defined)> Rows, Interpolation Interp) : PropertyValue;
 public sealed record Complex(string UsageName, Map<PropertyName, PropertyValue> Properties) : PropertyValue;
 public sealed record Temporal(TemporalValue Value) : PropertyValue;

 // Dispatched through the generated total Switch (no runtime-silent _ — a new case breaks Of at compile time):
 // total for the scalar arms (payloads admit upstream — a Measure wraps an already-SI-coerced MeasureValue, a
 // Reference a resolved NodeId), structural + RECURSIVE for the composite arms.
 public static Fin<PropertyValue> Of(PropertyValue value, Op key) => value.Switch(
  text: static p => Fin.Succ((PropertyValue)p),
  measure: static p => Fin.Succ((PropertyValue)p),
  boolean: static p => Fin.Succ((PropertyValue)p),
  logical: static p => Fin.Succ((PropertyValue)p),
  integer: static p => Fin.Succ((PropertyValue)p),
  number: p => double.IsFinite(p.Value)
   ? Fin.Succ((PropertyValue)p)
   : new KernelFault.OutOfRange("property-number", p.Value, "be finite", Some(key)),
  binary: static p => Fin.Succ((PropertyValue)p),
  reference: static p => Fin.Succ((PropertyValue)p),
  temporal: static p => Fin.Succ((PropertyValue)p),
  // Membership is TYPED record equality over the scalar arms (IfcValue admits no aggregate member), so a selected
  // "30" Text never matches a Measure(30) allowed row and a composite/reference member rails at admission.
  enumerated: p =>
   from allowed in p.Allowed.IsEmpty
    ? new ElementFault.ValueRejected(key, "<enumerated-allowed-empty>")
    : p.Allowed.TraverseM(v => AdmitScalar(v, key, "<enumerated-member-not-scalar>")).As()
   from selected in p.Selected.TraverseM(v => AdmitScalar(v, key, "<enumerated-member-not-scalar>")).As()
   from _ in selected.Exists(s => !allowed.Contains(s))
    ? new ElementFault.ValueRejected(key, "<enumerated-selected-not-allowed>")
    : Fin.Succ(unit)
   select (PropertyValue)new Enumerated(selected, allowed),
  bounded: p => AdmitBounded(p, key),
  list: p => p.Values.TraverseM(v => Of(v, key)).As().Map(static vs => (PropertyValue)new List(vs)),
  table: p => p.Rows.IsEmpty
   ? new ElementFault.ValueRejected(key, "<table-rows-empty>")
   : p.Rows.TraverseM(r =>
      from defining in AdmitScalar(r.Defining, key, "<table-defining-not-scalar>")
      from defined in AdmitScalar(r.Defined, key, "<table-defined-not-scalar>")
      select (Defining: defining, Defined: defined))
     .As().Map(rows => (PropertyValue)new Table(rows, p.Interp)),
  complex: p => p.Properties.IsEmpty
   ? new ElementFault.ValueRejected(key, "<complex-properties-empty>")
   : p.Properties.Fold(Fin.Succ(Map<PropertyName, PropertyValue>()), (acc, k, v) => acc.Bind(m => Of(v, key).Map(x => m.AddOrUpdate(k, x)))).Map(m => (PropertyValue)new Complex(p.UsageName, m)));

 public string Render() => Switch(
  text: static p => p.Value,
  measure: static p => RenderMeasure(p.Value),
  boolean: static p => p.Value ? "TRUE" : "FALSE",
  logical: static p => p.Value.Match(Some: static b => b ? "TRUE" : "FALSE", None: static () => "UNKNOWN"),
  integer: static p => p.Value.ToString(CultureInfo.InvariantCulture),
  number: static p => p.Value.ToString("R", CultureInfo.InvariantCulture),
  binary: static p => Convert.ToHexString(p.Value.ToArray()),
  enumerated: static p => string.Join(',', p.Selected.Map(static v => v.Render())),
  reference: static p => p.Target.Value,
  bounded: static p => $"[{Bound(p.Lower)}, {Bound(p.Upper)}, {Bound(p.SetPoint)}]",
  list: static p => string.Join(';', p.Values.Map(static v => v.Render())),
  table: static p => string.Join(';', p.Rows.Map(static r => $"{r.Defining.Render()}={r.Defined.Render()}")),
  complex: static p => $"{p.UsageName}{{{string.Join(';', p.Properties.OrderBy(static e => e.Key.Value, StringComparer.Ordinal).Select(static e => $"{e.Key.Value}={e.Value.Render()}"))}}}",
  temporal: static p => p.Value.Iso());

 // Case ordinal then typed payload through the CONTENT_ADDRESS CanonicalWriter — count-prefixed collections keep the
 // encoding injective; Complex name-sorts Ordinal.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  text: v => w.Ordinal(0).String(v.Value),
  measure: v => w.Ordinal(1).Measure(v.Value),
  boolean: v => w.Ordinal(2).Bool(v.Value),
  logical: v => w.Ordinal(3).Optional(v.Value, static (b, run) => run.Bool(b)),
  enumerated: v => w.Ordinal(4)
   .Rows(v.Selected, static (member, run) => member.CanonicalBytes(run))
   .Rows(v.Allowed, static (member, run) => member.CanonicalBytes(run)),
  reference: v => w.Ordinal(5).Optional(v.UsageName, static (u, run) => run.String(u)).String(v.Target.Value),
  bounded: v => w.Ordinal(6)
   .Optional(v.Lower, static (m, run) => run.Measure(m))
   .Optional(v.Upper, static (m, run) => run.Measure(m))
   .Optional(v.SetPoint, static (m, run) => run.Measure(m)),
  list: v => w.Ordinal(7).Rows(v.Values, static (inner, run) => inner.CanonicalBytes(run)),
  table: v => w.Ordinal(8).String(v.Interp.Key)
   .Rows(v.Rows, static (row, run) => { row.Defining.CanonicalBytes(run); row.Defined.CanonicalBytes(run); }),
  // Sorted IS the owner-published canonical order for the name-keyed map (DIGEST_OVER_UNORDERED_CONTAINER).
  complex: v => w.Ordinal(9).String(v.UsageName)
   .Sorted(toSeq(v.Properties), static e => e.Key.Value, StringComparer.Ordinal,
    static (e, run) => { run.String(e.Key.Value); e.Value.CanonicalBytes(run); }),
  temporal: v => w.Ordinal(10).Ordinal(v.Value.CaseOrdinal).String(v.Value.Iso()),
  integer: v => WriteBytes(w.Ordinal(11), v.Value.ToByteArray(isUnsigned: false, isBigEndian: true)),
  number: v => w.Ordinal(12).Double(v.Value),
  binary: v => WriteBytes(w.Ordinal(13), v.Value.ToArray()));

 // Reference is the ONLY arm carrying a NodeId; composites recurse, scalars are identity — Enumerated included,
 // because the Of scalar-member law makes a buried Reference unrepresentable there. A new case breaks at compile.
 public PropertyValue Remap(Func<NodeId, NodeId> map) => Switch<PropertyValue>(
  text: static p => p,
  measure: static p => p,
  boolean: static p => p,
  logical: static p => p,
  integer: static p => p,
  number: static p => p,
  binary: static p => p,
  enumerated: static p => p,
  reference: p => p with { Target = map(p.Target) },
  bounded: static p => p,
  list: p => new List(p.Values.Map(v => v.Remap(map))),
  table: p => new Table(p.Rows.Map(r => (Defining: r.Defining.Remap(map), Defined: r.Defined.Remap(map))), p.Interp),
  complex: p => new Complex(p.UsageName, p.Properties.Map((_, v) => v.Remap(map))),
  temporal: static p => p);

 // The recursive dual of Remap: every NodeId the value BURIES, so the Generic edge's buried attribute refs are a
 // LIVE reachability set the incidence index and DropNode cascade sweep — renumber and cascade stay in lockstep.
 public Seq<NodeId> References() => Switch(
  text: static _ => Seq<NodeId>(),
  measure: static _ => Seq<NodeId>(),
  boolean: static _ => Seq<NodeId>(),
  logical: static _ => Seq<NodeId>(),
  integer: static _ => Seq<NodeId>(),
  number: static _ => Seq<NodeId>(),
  binary: static _ => Seq<NodeId>(),
  enumerated: static _ => Seq<NodeId>(),
  reference: static p => Seq(p.Target),
  bounded: static _ => Seq<NodeId>(),
  list: static p => p.Values.Bind(static v => v.References()),
  table: static p => p.Rows.Bind(static r => r.Defining.References() + r.Defined.References()),
  complex: static p => p.Properties.Values.ToSeq().Bind(static v => v.References()),
  temporal: static _ => Seq<NodeId>());

 // The ONE measure→string projection the Measure arm and Bounded bounds share, INVARIANT culture; the absent-unit
 // blank is chosen HERE at the render boundary, so no fabricated token reaches the value the content key reads.
 private static string RenderMeasure(MeasureValue measure) =>
  measure.CanonicalUnit.Match(
   Some: unit => string.Create(CultureInfo.InvariantCulture, $"{measure.Si:R} {unit}"),
   None: () => string.Create(CultureInfo.InvariantCulture, $"{measure.Si:R}"));

 // Bound projects the three bound places over the shared RenderMeasure body — an absent bound renders "*" (the IFC
 // half-open interval).
 private static string Bound(Option<MeasureValue> bound) =>
  bound.Map(RenderMeasure).IfNone("*");

 // Three INDEPENDENT slots accumulate — presence, one QuantityType across present members, a present pair
 // ordered — so a malformed bound reports every violation at once. SetPoint is IfcPropertyBoundedValue's free
 // NOMINAL with NO WHERE rule binding it inside the interval; a third pair check REJECTS valid IFC.
 private static Fin<PropertyValue> AdmitBounded(Bounded b, Op key) {
  Seq<MeasureValue> present = Seq(b.Lower, b.Upper, b.SetPoint).Choose(static o => o);
  return Accumulate(Seq(
    Gate(!present.IsEmpty, key, "<bounded-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(present.Head.ForAll(head => present.ForAll(m => m.Type == head.Type)), key, "<bounded-cross-type>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(!Inverted(b.Lower, b.Upper), key, "<bounded-inverted>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .ToFin()
   .Map(_ => (PropertyValue)b);
 }

 // Inverted only when BOTH ends are present and low.Si strictly exceeds high.Si — an open end is never inverted.
 private static bool Inverted(Option<MeasureValue> low, Option<MeasureValue> high) =>
  low.Match(Some: lo => high.Match(Some: hi => lo.Si > hi.Si, None: static () => false), None: static () => false);

 private static CanonicalWriter WriteBytes(CanonicalWriter writer, ReadOnlySpan<byte> bytes) =>
  writer.Ordinal(bytes.Length).Raw(bytes);

 // TOTAL generated Map, never a positional type-pattern list: a fifteenth case is a BUILD ERROR here, where the
 // pattern-list form silently answered false and admitted a composite into an Enumerated member or Table cell.
 public bool IsScalar => Map(
  text: true, measure: true, boolean: true, logical: true, integer: true,
  number: true, binary: true, temporal: true,
  enumerated: false, reference: false, bounded: false, list: false, table: false, complex: false);

 private static Fin<PropertyValue> AdmitScalar(PropertyValue value, Op key, string detail) =>
  value.IsScalar ? Of(value, key) : new ElementFault.ValueRejected(key, detail);
}
```

## [03]-[PROPERTY_BAG]

- Owner: `ValueBag<V>` the ONE named inheritance-and-source-stamped value bag (`SetName` + `Map<PropertyName, V>` + `InheritanceMode` + `EvidenceGrade` + `Map<string, GroupIdentity>`) — `PropertyBag` (`ValueBag<PropertyValue>`) the `Graph/element#NODE_MODEL` `Node.PropertySet` case wraps and `QuantityBag` (`ValueBag<MeasureValue>`) the `Node.QuantitySet` case wraps are its two GLOBAL `using` aliases, the value type the only varying axis so it rides a type parameter (the SHAPE_BUDGET + DERIVED_TYPES collapse); `InheritanceMode` owns the type→occurrence precedence fold; `EvidenceGrade` owns the six-row attributable evidence-grade rank (S-E3 — the retired `PropertySource` collapsed into it, wire tokens append-only); `GroupIdentity` owns the dot-path group axis.
- Entry: `ValueBag<V>.Merge(type, occurrence)` folds a type-bound bag and an occurrence bag into one by delegating to `occurrence.Inheritance.Resolve(type.Values, occurrence.Values)` — the ONE generic precedence fold the mode owns — preserves the higher `EvidenceGrade` rank, and unions the `Groups` maps occurrence-first, so the `Graph/element#ELEMENT_GRAPH` `Bake` applies inheritance once per bag; `ValueBag<V>.Empty(setName, inheritance, source)` mints an empty source-stamped bag; `With(name, value)`/`Find(name)` are the immutable add and keyed read both alias kinds share.
- Auto: `Resolve<V>` dispatches the generated total `Switch` over the LanguageExt `Map` three-argument `Fold` — `OccurrenceWins` folds type entries onto the occurrence map adding only absent keys, `TypeDrivenOverride` folds with `AddOrUpdate` (type-wins), `TypeDrivenOnly` returns the type map — one generic fold serving both bag aliases identically, the mode (not the bag) owning the precedence; the `[SmartEnum<string>]` round-trips the mode token at the wire so a persisted bag re-admits its precedence; the mode is stamped at Bim ingress, the seam never inferring it.
- Receipt: the merged `ValueBag<V>` is the typed property evidence the `Bake`-derived `Element` carries flat in its `Seq<PropertyBag>`/`Seq<QuantityBag>` fields, so a consumer reads `element.Properties.Find(b => b.SetName == set).Bind(b => b.Find(name))` as one `Option<PropertyValue>`; `Source` records whether the winning bag came from Materials catalogue data, IFC import, Bim-derived quantities, or Rhino/user override — Compute assessments and Persistence provenance stay typed nodes/events, not bag sources.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[SmartEnum<int>]`), LanguageExt.Core (`Map`/`Option`/`Seq`), Generator.Equals (`[Equatable]`/`[UnorderedEquality]` — the bag is the drillable equality owner the `StructuralMerge` descends through to `Bag.Values[name]`).
- Growth: a new bag attribute shared by all bags is one column on `ValueBag<V>` (both aliases gain it); a new value kind a bag carries is one `using` alias over `ValueBag<TNew>`; a new inheritance precedence is one `InheritanceMode` row carrying its `Resolve` arm; a new source tier is one `EvidenceGrade` row with its rank; a new grouping fact is one column on `GroupIdentity` (both the canonical bytes and the wire gain it in the same edit).
- Boundary: `ValueBag<V>` is the ONE property store — a per-`Pset_*` class family, a second property model, or a hand-written `PropertyBag`-beside-`QuantityBag` pair duplicating every member (the SHAPE_BUDGET parallel-type defect) is the deleted form; the type→occurrence precedence is owned by `InheritanceMode.Resolve` and applied once in `Merge` by the stamped mode — never a per-call-site merge, a per-bag-type re-expression, or a seam inference; `InheritanceMode` is the bag-merge precedence ALONE — the named type→occurrence `Bake` inheritance over a baked element's materials, section, and classifications is a SEPARATE `Bake` dimension, never a fourth row here; the bag content is typed (`PropertyValue`/`MeasureValue`), a stringly-keyed property lookup the named defect; `Groups` is the QUANTITY grouping axis — a QUANTITY bag populates it (a property bag's nesting is the `[02]` `Complex` case, so its map is empty by construction), an EMPTY map is the ordinary ungrouped bag rather than a second bag kind, and the axis is IDENTITY-BEARING: the `quantitySet` canonical-bytes arm writes it count-prefixed, so two bags carrying identical values under different grouping identities key apart and a group whose identity rode a key spelling alone is the deleted lossy form.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class InheritanceMode {
 public static readonly InheritanceMode OccurrenceWins = new("occurrence-wins");
 public static readonly InheritanceMode TypeDrivenOverride = new("type-driven-override");
 public static readonly InheritanceMode TypeDrivenOnly = new("type-driven-only");

 // Resolve is the precedence algebra the mode OWNS — one state-threaded generic fold serving both bag aliases.
 public Map<PropertyName, V> Resolve<V>(Map<PropertyName, V> type, Map<PropertyName, V> occurrence) => Switch(
  state: (Type: type, Occurrence: occurrence),
  occurrenceWins: static s => s.Type.Fold(s.Occurrence, static (acc, k, v) => acc.ContainsKey(k) ? acc : acc.Add(k, v)),
  typeDrivenOverride: static s => s.Type.Fold(s.Occurrence, static (acc, k, v) => acc.AddOrUpdate(k, v)),
  typeDrivenOnly: static s => s.Type);
}

// EvidenceGrade is the ONE evidence-grade axis (S-E3): the int key IS the precedence rank ValueBag.Merge reads
// (generated relational operators), Token the FROZEN wire spelling (append-only — the four original tokens are
// unchanged; defined/measured appended), Attributable whether the grade names an accountable source (the column
// PropertyEvidence.Citable and the audit sweeps read).
[SmartEnum<int>]
public sealed partial class EvidenceGrade {
 public static readonly EvidenceGrade Catalogue = new(10, "catalogue", attributable: true);
 public static readonly EvidenceGrade Defined = new(15, "defined", attributable: true);
 public static readonly EvidenceGrade Import = new(20, "import", attributable: true);
 public static readonly EvidenceGrade Measured = new(25, "measured", attributable: true);
 public static readonly EvidenceGrade Derived = new(30, "derived", attributable: false);
 public static readonly EvidenceGrade User = new(40, "user", attributable: false);

 public string Token { get; }
 public bool Attributable { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// GroupIdentity carries the grouping identity a bag's dot-path prefix owns — the classifying Discrimination beside the
// qualifying Quality/Usage pair. Every column is absence-carrying: an unstated grouping string is UNSET, and lifting
// it to an empty spelling re-authors a group the source never qualified.
public sealed record GroupIdentity(Option<string> Discrimination, Option<string> Quality, Option<string> Usage);

// One value bag, the value type a TYPE PARAMETER. [Equatable] is LOAD-BEARING: StructuralMerge drills a changed
// property to Nodes[id].Bag.Values[name]; Values/Groups ride [UnorderedEquality]; V owns its own value equality.
// Groups keys the dot-path PREFIX; an ungrouped bag carries the empty map (default, no construction spells it).
[Equatable]
public sealed partial record ValueBag<V>(string SetName, [property: UnorderedEquality] Map<PropertyName, V> Values, InheritanceMode Inheritance, EvidenceGrade Source, [property: UnorderedEquality] Map<string, GroupIdentity> Groups = default) {
 public static ValueBag<V> Empty(string setName, InheritanceMode inheritance, EvidenceGrade source) =>
  new(setName, Map<PropertyName, V>(), inheritance, source);

 public Option<V> Find(PropertyName name) => Values.Find(name);

 public ValueBag<V> With(PropertyName name, V value) =>
  this with { Values = Values.AddOrUpdate(name, value) };

 // THREE column rules, deliberately different (do not unify): Values by the STAMPED mode, Source by MAX rank,
 // Groups by key union occurrence-first — a grouping identity is the occurrence's own evidence of how rows nest.
 public static ValueBag<V> Merge(ValueBag<V> type, ValueBag<V> occurrence) {
  Map<PropertyName, V> inherited = occurrence.Inheritance.Resolve(type.Values, occurrence.Values);
  EvidenceGrade source = occurrence.Source >= type.Source ? occurrence.Source : type.Source;
  Map<string, GroupIdentity> groups = type.Groups.Fold(occurrence.Groups, static (acc, prefix, group) => acc.ContainsKey(prefix) ? acc : acc.Add(prefix, group));
  return occurrence with { Values = inherited, Source = source, Groups = groups };
 }
}
```

## [04]-[DETAIL_SCHEMA]

- Owner: `DetailSchema` the ONE neutral schema mechanism over the `ValueBag<V>` aliases — a neutral `SetName`, an `InheritanceMode`, and an optional `JointType` allowed-set — and the canonical `PropertyName` vocabulary both bag families key on; `PropertyCategory` the owner-blessed producer scope every package mints its own row names through; `StructuralRows` the cross-package support, release, load, offset, and topology vocabulary a Bim projector stamps onto a `Generic` edge and a Compute runner reads back, with `QuantityRows`, `EnvelopeRows`, `BoundaryRows`, and `PortRows` its siblings over the baked base-quantity takeoff, the building-envelope `Pset` rows, the space-boundary edge payload, and the distribution-port flow attributes. `DetailSchema.Realization` owns realizing fastener/rebar/connector/joint detail with the masonry work-size tolerance, cmu profile-subtype, and concrete/post-tensioning/fireproofing/cladding trade rows; `DetailSchema.Product` owns panel board/deck/membrane product geometry with the IGU build rows and the curtain-wall, precast, insulation, membrane, pipework, ductwork, and electrical-containment trade rows; `DetailSchema.Takeoff` owns the type-level per-running-metre quantity rows; `DetailSchema.Appearance` owns the appearance node's own bag — `TextureSet` the baked-set content address and `DoubleSided` the render-sidedness bit — the RULINGS-landed escape hatch that keeps the frozen `AppearanceSummary` preimage from widening.
- Entry: `PropertyCategory.<scope>.Row(name)` mints a producer-scoped row name, `PropertyCategory.Seam` carrying the empty prefix so the schema's own statics keep the bare names an IFC round-trip froze; `StructuralRows.Translation`/`Rotation`/`Warping` project the joint's support families and `ReleaseTranslation`/`ReleaseRotation`/`ReleaseWarping` the member end's own stated release, `Dofs` and `Releases` reading the two rosters in ONE slot order, `ReleaseCore` the six-row presence a stated release always carries whole, `ReleaseOf` the support-row→release-row correspondence zipped off that pair, and `Offset` the stated rigid-end offset vector in the connection's own `Frame`; `Force`/`Moment`/`PlanarForce`/`Start`/`End` the applied-load component families and `DeltaT` the `Gradients`-keyed thermal family; `QuantityRows.SurfaceArea`/`FloorArea`/`FootprintArea`/`CrossSection`/`Volume`/`Weight` project the ordered net-over-gross takeoff chains a reader folds first-hit-wins; `DetailSchema.Realization` the canonical realizing schema; `DetailSchema.Product` the canonical product-detail schema; `DetailSchema.Takeoff` the canonical type-quantity schema; `DetailSchema.Appearance` the canonical appearance-bag schema its `TextureSet`/`DoubleSided` rows key on; `schema.Bag(source = default)` mints the empty conforming source-stamped `PropertyBag` and `schema.Quantities(source = default)` its `QuantityBag` counterpart, the omitted source deriving `EvidenceGrade.Catalogue`; `schema.Joint(selected, key)` the `JointType` row VALUE as a `PropertyValue.Enumerated` over the schema's closed allowed-set, railed because the token crosses the `Of` admission.
- Auto: `Bag` and `Quantities` pin `SetName` and `InheritanceMode` from the schema and stamp the resolved source rank, so neither author nor reader hand-spells the set-name string, re-stamps precedence, or drops source rank; `Joint(selected)` constructs the typed `PropertyValue.Enumerated` over `Text`-wrapped tokens (the selected token against the schema's closed `JointTypes` allowed-set) so the `Properties/property#PROPERTY_VALUE` `Of` admission holds.
- Receipt: the conforming `PropertyBag` lands on the seam `ElementGraph` as a `Graph/element#NODE_MODEL` `Node.PropertySet` and the conforming `QuantityBag` as a `Node.QuantitySet`, each bound by one `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge, the `Bake` fold merging them into `element.Properties` and `element.Quantities` — a takeoff bound to a Type reaches every occurrence through that same type-bag merge, so no occurrence re-mints it; both bags mint through `NodeId.Of(NodeSeed.Content)` over the node's own `CanonicalBytes` projection (id excluded) so two structurally-identical bags dedup to one node, never a second `(GeometryKey, DetailKey)` hasher.
- Packages: LanguageExt.Core (`Seq`/`Map` + the `Prelude` constructors), Thinktecture.Runtime.Extensions (the `PropertyName` `Create` factory + the `InheritanceMode` statics), `Properties/quantity#MEASURE_VALUE` (both `MeasureValue.OfSi` mints — the typed identity and the dimension-anonymous fallback the bag law elects between), and the seam `PropertyBag`/`PropertyValue`/`PropertyName`/`InheritanceMode` owners this cluster composes.
- Growth: a new producer scope is one `PropertyCategory` row and a new producer-local row family is one static roster in the owning package minted through that row's `Row`; a new structural axis is one `StructuralRows.Axes` entry every coordinate family absorbs — the support, release, and offset families with it, so a degree of freedom reaches both restraint wires in one edit — and a new thermal gradient one `Gradients` entry, while a family keyed on neither — a warping/bimoment restraint, a further torsional degree of freedom — is one `Family(stem, keys)` call carrying its own roster and one term on `Dofs` and `Releases` alike, never a seventh entry bent into `Axes` that every coordinate family then answers for; a new realizing-detail, product, or takeoff row is one `static readonly PropertyName` the author writes and the reader reads by name; a new joint modality is one token on `Realization.JointTypes`; a material-property→`Pset` bag is ANOTHER `DetailSchema` instance — ONE schema mechanism over the bag aliases, never a parallel schema type, a per-row bag class, or a per-call-site allowed-set literal.
- Boundary: `DetailSchema` is the ONE seam-declared detail contract and the seam carries NO IFC name (the Pset roster, bSDD resolution, egress mapping, and `GlobalId` assignment stay in the `Rasm.Bim` `SemanticProjector`); cross-peer realizing invariants are Bim-implemented `IGraphConstraint`s and material bindings ride the Materials `Associate` edge, never rows here; an authored bag carrying a subset of rows is a faithfully different node; a bag row's `MeasureValue` carries its `QuantityType` exactly where every producer names it truthfully (`[05]` `[TAKEOFF_QUANTITY_IDENTITY]`), provision follows the WIRE FAMILY (`[05]` `[WIRE_FAMILY_PROVISION]`), and the key space closes over OWNER PROVISION — a name two packages key on is a static here, a name one package owns mints through its `PropertyCategory` row, and a call-site `PropertyName.Create` in any writer or reader is the fork this custody deletes.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// PropertyCategory is the owner-blessed producer scope a package mints its OWN row names under. The statics below
// carry the CROSS-package vocabulary — every name a second package keys on — while a category-minted name is
// namespaced by its producer, so a row one package writes and no peer reads can neither collide with a peer's
// spelling nor drift from it. Seam carries the empty prefix, so the schema's own statics keep the bare names an IFC
// round-trip already froze. A call-site PropertyName.Create in a writer or a reader is exactly what this pair
// deletes: a name reaches a bag through an owner static or through its own producer's category, never a literal.
[SmartEnum<string>]
public sealed partial class PropertyCategory {
 public static readonly PropertyCategory Seam = new("seam", prefix: "");
 public static readonly PropertyCategory Bim = new("bim", prefix: "bim.");
 public static readonly PropertyCategory Materials = new("materials", prefix: "materials.");
 public static readonly PropertyCategory Fabrication = new("fabrication", prefix: "fabrication.");
 public static readonly PropertyCategory Compute = new("compute", prefix: "compute.");

 public string Prefix { get; }

 // Row is the ONE mint a producer roster composes; the declaring static lives in the producing package, so the
 // vocabulary stays where its domain is while the key space stays partitioned by this owner.
 public PropertyName Row(string name) => PropertyName.Create($"{Prefix}{name}");
}

// --- [MODELS] -----------------------------------------------------------------------------
// StructuralRows carries the support, release, load, offset, and topology rows a Bim projector STAMPS onto a Generic
// edge's attribute map and a Compute analysis runner READS back. Every column an analysis row asserts is STATED on
// this wire — a release the connection declared and the offset it placed the member end at, never a behaviour the
// reader reconstructs off a neighbouring reading. Both packages are non-referencing peers, so a
// literal at either end forks silently the moment one side renames — these statics are the single spelling both
// compose, and every component family generates off its own key roster rather than enumerating, so a new axis or
// gradient is unreachable by typo. They key an EDGE attribute map rather than a DetailSchema bag, which is why they
// sit beside the schema rather than inside it.
public static class StructuralRows {
 static PropertyName Row(string name) => PropertyCategory.Seam.Row(name);
 public static readonly PropertyName AtStart = Row(nameof(AtStart));
 public static readonly PropertyName Station = Row(nameof(Station));
 public static readonly PropertyName SupportedLength = Row(nameof(SupportedLength));
 public static readonly PropertyName Frame = Row(nameof(Frame));
 public static readonly PropertyName LoadKind = Row(nameof(LoadKind));
 public static readonly PropertyName Case = Row(nameof(Case));

 // Axis is the whole discriminant of a COORDINATE family, so each one is a projection over it and a seventh degree
 // of freedom or a second load component set lands as one axis row rather than three hand-typed statics per family.
 public static readonly Seq<string> Axes = Seq("X", "Y", "Z");

 // Temperature keys its gradient family on the action's own shape — a uniform rise with two bending gradients —
 // never on the coordinate axes, so it hands its own roster to the same mint instead of bending Axes to fit it.
 public static readonly Seq<string> Gradients = Seq("Constant", "Y", "Z");

 // Warping: the seventh (bimoment) restraint, EN 1993-1-1 §6.3.3 / AISC 360 App.1 — keyed on the MEMBER AXIS
 // alone, so it hands its own single-key roster to Family rather than bending Axes every coordinate family reads.
 public static readonly Seq<string> WarpingKeys = Seq("Axial");

 public static readonly Map<string, PropertyName> Translation = Family("Translation");
 public static readonly Map<string, PropertyName> Rotation = Family("Rotation");
 public static readonly Map<string, PropertyName> Warping = Family("Warping", Some(WarpingKeys));

 // The member END's own release, STATED by the connection that carries it rather than inferred from the joint the
 // end attaches to. The two facts are independent in both directions: an end pinned into a joint some other member
 // braces reads RESTRAINED on every support row and RELEASED here, while a continuous end at a free joint reads
 // free on both. A reader recovering releases from a both-free support pair therefore reads the braced pinned end
 // as continuous and cannot recover this wire from that one. Same key rosters and the same three-way value shape as
 // the support families — Boolean fixity, Measure spring, nothing else — so the seventh degree of freedom reached
 // this wire the day it reached that one and ONE reader shape keys both.
 public static readonly Map<string, PropertyName> ReleaseTranslation = Family("ReleaseTranslation");
 public static readonly Map<string, PropertyName> ReleaseRotation = Family("ReleaseRotation");
 public static readonly Map<string, PropertyName> ReleaseWarping = Family("ReleaseWarping", Some(WarpingKeys));

 // The rigid-end OFFSET the connection STATES as a vector in its own Frame: the analytical member end sits where
 // the projector placed it, the along-axis component being the rigid-end length a frame condensation reads. A
 // reader halving SupportedLength re-derives a modelling decision it does not own — SupportedLength is the physical
 // support's own length an IFC round-trip froze, and an end whose eccentricity the file carries as connection
 // constraint geometry has an offset no length scalar reaches. A condensation carrying no transverse rigid offset
 // lowers the Y/Z pair by NAME the way the warping slot lowers, never by reading the axial row as the whole offset.
 public static readonly Map<string, PropertyName> Offset = Family("Offset");

 public static readonly Map<string, PropertyName> Force = Family("Force");
 public static readonly Map<string, PropertyName> Moment = Family("Moment");
 public static readonly Map<string, PropertyName> PlanarForce = Family("PlanarForce");
 public static readonly Map<string, PropertyName> Start = Family("Start");
 public static readonly Map<string, PropertyName> End = Family("End");
 public static readonly Map<string, PropertyName> DeltaT = Family("DeltaT", Some(Gradients));

 // Fixity and stiffness ride ONE row per DOF — the value's own case says boolean restraint or spring MeasureValue;
 // a parallel <dof>Stiffness roster strands the spring magnitude on every boolean-keyed reader.
 public static Seq<PropertyName> Dofs => Translation.Values.ToSeq() + Rotation.Values.ToSeq() + Warping.Values.ToSeq();

 // Dofs is the SUPPORT roster and Releases its release peer in the same slot order, both projected off the same key
 // rosters, so a reader folds either wire through one shape and ReleaseOf is a ZIP of the two rather than a
 // hand-kept mirror that drifts the day an eighth degree of freedom lands. Both rosters order X/Y/Z then the
 // warping key, because a coordinate family's keys sort ordinally and both sides take the same roster.
 public static Seq<PropertyName> Releases => ReleaseTranslation.Values.ToSeq() + ReleaseRotation.Values.ToSeq() + ReleaseWarping.Values.ToSeq();
 public static Map<PropertyName, PropertyName> ReleaseOf => toMap(Dofs.Zip(Releases));

 // ADMISSION and ABSENCE. The projector stamps the six coordinate release rows WHOLE or stamps none — one boundary
 // condition yields all six, an absent condition yields nothing — so ReleaseCore present IS the statement,
 // ReleaseCore absent is UNSTATED, and a partial core is malformed input, never a partial reading. A reader admits
 // its release column only off a whole core and carries the absence in an option its own frame row refuses on BY
 // NAME: continuity read into a member the file never released is an assertion no analysis retracts downstream, and
 // that is exactly what an empty release set defaulted off a fully-restrained support pair asserts. ReleaseWarping
 // rides OUTSIDE the core because the seventh stiffness is sealed at the source and unauthorable, so its absence
 // inside a stated release is the same NAMED lowering the support side already carries — never a seventh silence
 // that voids the six.
 public static Seq<PropertyName> ReleaseCore => ReleaseTranslation.Values.ToSeq() + ReleaseRotation.Values.ToSeq();

 // RC shear-link triple: Materials capacity screen STAMPS, Compute member check READS (non-referencing peers);
 // absence means no links or no published yield. Named statics — consumers key each column at compile time.
 public static readonly PropertyName ShearLinkArea = Row(nameof(ShearLinkArea));
 public static readonly PropertyName ShearLinkYield = Row(nameof(ShearLinkYield));
 public static readonly PropertyName ShearLinkCeiling = Row(nameof(ShearLinkCeiling));

 // EC9 buckling pair (Table 6.6 α, plateau λ̄0), same stamp/read custody; absence means no published curve.
 public static readonly PropertyName BucklingAlpha = Row(nameof(BucklingAlpha));
 public static readonly PropertyName BucklingPlateau = Row(nameof(BucklingPlateau));

 // ONE mint per row family: the key roster is the whole discriminant and Axes is its canonical default, so a
 // coordinate family reads Family(stem) and a family keyed on its own action shape hands its roster in.
 public static Map<string, PropertyName> Family(string stem, Option<Seq<string>> keys = default) =>
  keys.IfNone(Axes).Fold(Map<string, PropertyName>(), (map, key) => map.Add(key, PropertyName.Create($"{stem}{key}")));
}

// Baked base-quantity takeoff: Bim/Fabrication STAMP, every Compute runner READS — bare IFC Qto_* spellings a
// round-trip froze (Seam prefix). The ordered net-over-gross CHAINS are row data every discipline reads once.
public static class QuantityRows {
 static PropertyName Row(string name) => PropertyCategory.Seam.Row(name);
 // Base-quantity SPINE, grouped by dimension family (lengths, areas, volumes, weights): the bare IFC Qto spellings
 // every element family shares, so a beam's Length and a wall's Length are one row.
 public static readonly PropertyName Length = Row(nameof(Length));
 public static readonly PropertyName Width = Row(nameof(Width));
 public static readonly PropertyName Height = Row(nameof(Height));
 public static readonly PropertyName Depth = Row(nameof(Depth));
 public static readonly PropertyName Perimeter = Row(nameof(Perimeter));
 public static readonly PropertyName Area = Row(nameof(Area));
 public static readonly PropertyName GrossArea = Row(nameof(GrossArea));
 public static readonly PropertyName NetArea = Row(nameof(NetArea));
 public static readonly PropertyName GrossSideArea = Row(nameof(GrossSideArea));
 public static readonly PropertyName NetSideArea = Row(nameof(NetSideArea));
 public static readonly PropertyName GrossFloorArea = Row(nameof(GrossFloorArea));
 public static readonly PropertyName NetFloorArea = Row(nameof(NetFloorArea));
 public static readonly PropertyName GrossFootprintArea = Row(nameof(GrossFootprintArea));
 public static readonly PropertyName NetFootprintArea = Row(nameof(NetFootprintArea));
 // Member-takeoff custody anchor; the DetailSchema rebar CrossSectionArea shares the IFC spelling on a DIFFERENT
 // wire family — two custody anchors, never one fact twinned.
 public static readonly PropertyName CrossSectionArea = Row(nameof(CrossSectionArea));
 public static readonly PropertyName NetCrossSectionArea = Row(nameof(NetCrossSectionArea));
 public static readonly PropertyName GrossCrossSectionArea = Row(nameof(GrossCrossSectionArea));
 // OuterSurfaceArea is the linear member's coating/fire-protection envelope; the Gross/Net surface pair the
 // profile-member sets carry beside it.
 public static readonly PropertyName OuterSurfaceArea = Row(nameof(OuterSurfaceArea));
 public static readonly PropertyName GrossSurfaceArea = Row(nameof(GrossSurfaceArea));
 public static readonly PropertyName NetSurfaceArea = Row(nameof(NetSurfaceArea));
 public static readonly PropertyName GrossVolume = Row(nameof(GrossVolume));
 public static readonly PropertyName NetVolume = Row(nameof(NetVolume));
 public static readonly PropertyName GrossWeight = Row(nameof(GrossWeight));
 public static readonly PropertyName NetWeight = Row(nameof(NetWeight));
 public static readonly PropertyName GlazingArea = Row(nameof(GlazingArea));
 public static readonly PropertyName GlazingPerimeter = Row(nameof(GlazingPerimeter));

 // Fabrication lowers NestYield.WasteAreaMm2 onto the element's quantity bag under this row, so off-cut waste joins the
 // material basis the carbon and cost folds distribute by.
 public static readonly PropertyName NestWasteArea = Row(nameof(NestWasteArea));

 // Set names the projector stamps; ONE Qto_<Stem>BaseQuantities mint — CurtainWall is the roster's one Base-less
 // spelling and states its own literal rather than bending the mint.
 private static string Qto(string stem) => $"Qto_{stem}BaseQuantities";
 public static readonly string BeamBaseQuantities = Qto("Beam");
 public static readonly string ColumnBaseQuantities = Qto("Column");
 public static readonly string MemberBaseQuantities = Qto("Member");
 public static readonly string PlateBaseQuantities = Qto("Plate");
 public static readonly string SlabBaseQuantities = Qto("Slab");
 public static readonly string WallBaseQuantities = Qto("Wall");
 public static readonly string RoofBaseQuantities = Qto("Roof");
 public static readonly string CoveringBaseQuantities = Qto("Covering");
 public static readonly string DoorBaseQuantities = Qto("Door");
 public static readonly string WindowBaseQuantities = Qto("Window");
 public static readonly string StairFlightBaseQuantities = Qto("StairFlight");
 public static readonly string RampFlightBaseQuantities = Qto("RampFlight");
 public static readonly string RailingBaseQuantities = Qto("Railing");
 public static readonly string FootingBaseQuantities = Qto("Footing");
 public static readonly string PileBaseQuantities = Qto("Pile");
 public static readonly string DuctSegmentBaseQuantities = Qto("DuctSegment");
 public static readonly string PipeSegmentBaseQuantities = Qto("PipeSegment");
 public static readonly string CableSegmentBaseQuantities = Qto("CableSegment");
 public static readonly string CableCarrierSegmentBaseQuantities = Qto("CableCarrierSegment");
 public static readonly string SpaceBaseQuantities = Qto("Space");
 public static readonly string BuildingStoreyBaseQuantities = Qto("BuildingStorey");
 public static readonly string CurtainWallQuantities = "Qto_CurtainWallQuantities";

 // One chain per quantity family, net before gross, folded first-hit-wins.
 public static readonly Seq<PropertyName> SurfaceArea = Seq(NetSideArea, NetArea, NetSurfaceArea, GrossSideArea, GrossArea, GrossSurfaceArea);
 public static readonly Seq<PropertyName> FloorArea = Seq(NetFloorArea, GrossFloorArea);
 public static readonly Seq<PropertyName> FootprintArea = Seq(NetFootprintArea, GrossFootprintArea);
 public static readonly Seq<PropertyName> CrossSection = Seq(NetCrossSectionArea, CrossSectionArea, GrossCrossSectionArea);
 public static readonly Seq<PropertyName> Volume = Seq(NetVolume, GrossVolume);
 public static readonly Seq<PropertyName> Weight = Seq(NetWeight, GrossWeight);
}

// Envelope Pset rows a physics/energy runner reads and the Bim projector writes (non-referencing peers).
public static class EnvelopeRows {
 static PropertyName Row(string name) => PropertyCategory.Seam.Row(name);
 // Spacer Ψg the glazing family lowers onto the ELEMENT Pset, never onto MaterialPropertySet.Thermal, which carries
 // no perimeter-bridge column.
 public static readonly PropertyName GlazingEdgePsi = Row(nameof(GlazingEdgePsi));
 public static readonly PropertyName IsExternal = Row(nameof(IsExternal));

 public const string SpaceCommon = "Pset_SpaceCommon";
}

// Space-boundary rows on the neutral Generic edge: Element declares, the Bim projector stamps, the energy and
// circulation runners (python energy peer included) read back. Host discriminates an OPENING from an opaque base
// surface; BoundaryLevel the 1st/2nd boundary generation (a blank reads 1st-equivalent).
public static class BoundaryRows {
 static PropertyName Row(string name) => PropertyCategory.Seam.Row(name);
 public static readonly PropertyName Host = Row(nameof(Host));
 public static readonly PropertyName BoundaryLevel = Row(nameof(BoundaryLevel));
}

// IfcDistributionPort pair riding the synthesized Import bag: Bim ingest stamps, the systems trace reads (an
// unsurfaced FlowDirection reads NOTDEFINED and degrades the directed trace to undirected reachability).
public static class PortRows {
 static PropertyName Row(string name) => PropertyCategory.Seam.Row(name);
 public static readonly PropertyName FlowDirection = Row(nameof(FlowDirection));
 public static readonly PropertyName SystemType = Row(nameof(SystemType));
}

// The ONE neutral detail schema over PropertyBag — Materials authors, Bim round-trips; SetName→IFC Pset maps at
// the Bim Emit, never here.
public sealed record DetailSchema(string SetName, InheritanceMode Inheritance, Seq<string> JointTypes) {
 static PropertyName Row(string name) => PropertyCategory.Seam.Row(name);
 // Canonical NEUTRAL vocabulary both author and reader key on; PropertyName itself stays an OPEN key — these are
 // the seam rows the Component projection writes and the Bim reader recovers one-hop, never a closed vocabulary.
 public static readonly PropertyName JointType = Row(nameof(JointType));
 public static readonly PropertyName FastenerType = Row(nameof(FastenerType));
 public static readonly PropertyName AccessoryType = Row(nameof(AccessoryType));
 public static readonly PropertyName BarType = Row(nameof(BarType));
 public static readonly PropertyName NominalDiameter = Row(nameof(NominalDiameter));
 public static readonly PropertyName NominalLength = Row(nameof(NominalLength));
 public static readonly PropertyName CrossSectionArea = Row(nameof(CrossSectionArea));
 public static readonly PropertyName CarriedMemberWidth = Row(nameof(CarriedMemberWidth));
 public static readonly PropertyName CarriedMemberDepth = Row(nameof(CarriedMemberDepth));
 public static readonly PropertyName EffectiveThroat = Row(nameof(EffectiveThroat));
 public static readonly PropertyName BondLine = Row(nameof(BondLine));
 public static readonly PropertyName Overlap = Row(nameof(Overlap));
 // MASONRY: EN 771 work-vs-actual size band (T/R classes, special shape), UnitHeight the bed-plane height the
 // W×L profile cannot carry, CourseHeight the coursing height (unit + bed joint) tessellation reads.
 public static readonly PropertyName SizeTolerance = Row(nameof(SizeTolerance));
 public static readonly PropertyName SizeRange = Row(nameof(SizeRange));
 public static readonly PropertyName SpecialShape = Row(nameof(SpecialShape));
 public static readonly PropertyName UnitHeight = Row(nameof(UnitHeight));
 public static readonly PropertyName CourseHeight = Row(nameof(CourseHeight));
 // CMU: occupancy-derived IFC profile-def subtype token (WithVoids iff any ungrouted cell) — derivation stays
 // Materials-owned, the wire carries the datum, the Bim egress profile lane selects the authored entity.
 public static readonly PropertyName ProfileSubtype = Row(nameof(ProfileSubtype));
 // SHOP-DELIVERABLE rows: the reinforcement/joint/fastener/connector arms author, Fabrication schedule folds read.
 public static readonly PropertyName BendShapeCode = Row(nameof(BendShapeCode));
 public static readonly PropertyName BendSchedule = Row(nameof(BendSchedule));
 public static readonly PropertyName PartThickness = Row(nameof(PartThickness));
 public static readonly PropertyName WeldPrep = Row(nameof(WeldPrep));
 public static readonly PropertyName StudGrade = Row(nameof(StudGrade));
 public static readonly PropertyName FastenerForm = Row(nameof(FastenerForm));
 public static readonly PropertyName ConnectorPlate = Row(nameof(ConnectorPlate));
 public static readonly PropertyName EvaluationReport = Row(nameof(EvaluationReport));
 // NESTED Complex keys are seam vocabulary under the SAME single-owner law as their parents — a call-site mint
 // would fork the key space, so every nested key declares here. WeldPrep: identity, electrode, profile, prep.
 public static readonly PropertyName WeldType = Row(nameof(WeldType));
 public static readonly PropertyName Electrode = Row(nameof(Electrode));
 public static readonly PropertyName Specification = Row(nameof(Specification));
 public static readonly PropertyName Face = Row(nameof(Face));
 public static readonly PropertyName RootTreatment = Row(nameof(RootTreatment));
 public static readonly PropertyName Reinforcement = Row(nameof(Reinforcement));
 public static readonly PropertyName ToeRadius = Row(nameof(ToeRadius));
 public static readonly PropertyName RootFace = Row(nameof(RootFace));
 public static readonly PropertyName Groove = Row(nameof(Groove));
 public static readonly PropertyName Penetration = Row(nameof(Penetration));
 public static readonly PropertyName Backing = Row(nameof(Backing));
 public static readonly PropertyName Process = Row(nameof(Process));
 public static readonly PropertyName RootOpening = Row(nameof(RootOpening));
 // StudGrade nested keys: the grade token and its published strength pair.
 public static readonly PropertyName Grade = Row(nameof(Grade));
 public static readonly PropertyName YieldStrength = Row(nameof(YieldStrength));
 public static readonly PropertyName UltimateStrength = Row(nameof(UltimateStrength));
 // ConnectorPlate nested keys: the stamped-plate stock geometry the shop's cut sheet reads.
 public static readonly PropertyName SheetThickness = Row(nameof(SheetThickness));
 public static readonly PropertyName BendRadius = Row(nameof(BendRadius));
 public static readonly PropertyName HoleDiameter = Row(nameof(HoleDiameter));
 public static readonly PropertyName HolePitch = Row(nameof(HolePitch));
 public static readonly PropertyName HoleCount = Row(nameof(HoleCount));
 public static readonly PropertyName DevelopedWidth = Row(nameof(DevelopedWidth));
 // BendSchedule nested keys: the placement's bend angle beside the ACI inside-bend diameter, the floored hook
 // extension, and the EN mandrel diameter — the bend block a bar-bending schedule is bought with.
 public static readonly PropertyName BendAngle = Row(nameof(BendAngle));
 public static readonly PropertyName InsideBendDiameter = Row(nameof(InsideBendDiameter));
 public static readonly PropertyName HookExtension = Row(nameof(HookExtension));
 public static readonly PropertyName MandrelDiameter = Row(nameof(MandrelDiameter));
 // FastenerForm keys: ISO 68-1 thread algebra + ISO 4014/4032/7089 hex envelope; bearing/fillet diameters only
 // where the product standard publishes them.
 public static readonly PropertyName FlankAngle = Row(nameof(FlankAngle));
 public static readonly PropertyName Pitch = Row(nameof(Pitch));
 public static readonly PropertyName MinorDiameter = Row(nameof(MinorDiameter));
 public static readonly PropertyName PitchDiameter = Row(nameof(PitchDiameter));
 public static readonly PropertyName RootDiameter = Row(nameof(RootDiameter));
 public static readonly PropertyName AcrossCorners = Row(nameof(AcrossCorners));
 public static readonly PropertyName ThreadRunout = Row(nameof(ThreadRunout));
 public static readonly PropertyName ThreadLength = Row(nameof(ThreadLength));
 public static readonly PropertyName UnthreadedShank = Row(nameof(UnthreadedShank));
 public static readonly PropertyName HeadHeight = Row(nameof(HeadHeight));
 public static readonly PropertyName NutHeight = Row(nameof(NutHeight));
 public static readonly PropertyName WasherInner = Row(nameof(WasherInner));
 public static readonly PropertyName WasherOuter = Row(nameof(WasherOuter));
 public static readonly PropertyName WasherThickness = Row(nameof(WasherThickness));
 public static readonly PropertyName BearingDiameter = Row(nameof(BearingDiameter));
 public static readonly PropertyName FilletDiameter = Row(nameof(FilletDiameter));
 // PANEL rows: board build, fastener pitches, deck corrugation, membrane lap, strength axis, operating envelope.
 public static readonly PropertyName EdgeProfile = Row(nameof(EdgeProfile));
 public static readonly PropertyName PanelThickness = Row(nameof(PanelThickness));
 public static readonly PropertyName FieldSpacing = Row(nameof(FieldSpacing));
 public static readonly PropertyName EdgeSpacing = Row(nameof(EdgeSpacing));
 public static readonly PropertyName RibDepth = Row(nameof(RibDepth));
 public static readonly PropertyName RibPitch = Row(nameof(RibPitch));
 public static readonly PropertyName MembraneSeam = Row(nameof(MembraneSeam));
 public static readonly PropertyName BoardLength = Row(nameof(BoardLength));
 public static readonly PropertyName PanelOrientation = Row(nameof(PanelOrientation));
 public static readonly PropertyName CoreClass = Row(nameof(CoreClass));
 public static readonly PropertyName SpanRating = Row(nameof(SpanRating));
 // RoofSpan/FloorSpan: MEASURED spans beside the SpanRating TOKEN — one product publishes different values per
 // use, so the pair is two rows; Materials' panel projector converts the token, span consumers read the rows.
 public static readonly PropertyName RoofSpan = Row(nameof(RoofSpan));
 public static readonly PropertyName FloorSpan = Row(nameof(FloorSpan));
 // Remaining panel product facts, each published by the one panel projector.
 public static readonly PropertyName RoofSpanUnsupported = Row(nameof(RoofSpanUnsupported));
 public static readonly PropertyName CompressiveStrength = Row(nameof(CompressiveStrength));
 public static readonly PropertyName FasteningMethod = Row(nameof(FasteningMethod));
 public static readonly PropertyName BondClass = Row(nameof(BondClass));
 public static readonly PropertyName FoamClass = Row(nameof(FoamClass));
 public static readonly PropertyName FacerClass = Row(nameof(FacerClass));
 public static readonly PropertyName ThermalResistance = Row(nameof(ThermalResistance));
 public static readonly PropertyName DeckForm = Row(nameof(DeckForm));
 // IGU rows: the glazing build inputs the seed-time EN 673 Ug / EN 410 g,τv / mass-law Rw receipts compute from.
 public static readonly PropertyName PaneBuild = Row(nameof(PaneBuild));
 public static readonly PropertyName CavityBuild = Row(nameof(CavityBuild));
 public static readonly PropertyName SpacerType = Row(nameof(SpacerType));
 public static readonly PropertyName EdgeSeal = Row(nameof(EdgeSeal));
 public static readonly PropertyName MuntinGrid = Row(nameof(MuntinGrid));
 public static readonly PropertyName FireResistanceEi = Row(nameof(FireResistanceEi));
 // IGU nested keys: per-pane optics/coating/laminate, per-cavity gas/vacuum/width, EN 1279-2 seal, muntin grid.
 public static readonly PropertyName Glass = Row(nameof(Glass));
 public static readonly PropertyName Thickness = Row(nameof(Thickness));
 public static readonly PropertyName CoatingOutboard = Row(nameof(CoatingOutboard));
 public static readonly PropertyName CoatingInboard = Row(nameof(CoatingInboard));
 public static readonly PropertyName Interlayer = Row(nameof(Interlayer));
 public static readonly PropertyName InterlayerThickness = Row(nameof(InterlayerThickness));
 public static readonly PropertyName Gas = Row(nameof(Gas));
 public static readonly PropertyName FillFraction = Row(nameof(FillFraction));
 public static readonly PropertyName Balance = Row(nameof(Balance));
 public static readonly PropertyName Width = Row(nameof(Width));
 public static readonly PropertyName ResidualPressure = Row(nameof(ResidualPressure));
 public static readonly PropertyName PillarRadius = Row(nameof(PillarRadius));
 public static readonly PropertyName PillarPitch = Row(nameof(PillarPitch));
 public static readonly PropertyName Primary = Row(nameof(Primary));
 public static readonly PropertyName Secondary = Row(nameof(Secondary));
 public static readonly PropertyName Desiccant = Row(nameof(Desiccant));
 public static readonly PropertyName CorneredKeys = Row(nameof(CorneredKeys));
 public static readonly PropertyName Style = Row(nameof(Style));
 public static readonly PropertyName HorizontalBars = Row(nameof(HorizontalBars));
 public static readonly PropertyName VerticalBars = Row(nameof(VerticalBars));
 public static readonly PropertyName BarWidth = Row(nameof(BarWidth));
 public static readonly PropertyName BarDepth = Row(nameof(BarDepth));
 // Per-running-metre takeoffs minted ONCE off the resolved section and substance density; each carries its own
 // QuantityType so a takeoff consumer multiplies through the measure algebra — an anonymous kg/m strands the product.
 public static readonly PropertyName MassPerLength = Row(nameof(MassPerLength));
 public static readonly PropertyName SurfaceAreaPerLength = Row(nameof(SurfaceAreaPerLength));
 public static readonly PropertyName VolumePerLength = Row(nameof(VolumePerLength));
 // CONCRETE: c_nom durability cover (SectionProperties.AxisDistance stays the fire cover — a different fact),
 // mix/exposure/cast tokens.
 public static readonly PropertyName ConcreteCover = Row(nameof(ConcreteCover));
 public static readonly PropertyName MixDesignation = Row(nameof(MixDesignation));
 public static readonly PropertyName ExposureClass = Row(nameof(ExposureClass));
 public static readonly PropertyName CastMethod = Row(nameof(CastMethod));
 // PRECAST product rows (precast runs DetailLane.Product): LiftingInsert the cast-in lifting-insert token the rigging schedule reads,
 // BearingLength the seat length the erection check reads, JointGrout the joint-grout specification token.
 public static readonly PropertyName LiftingInsert = Row(nameof(LiftingInsert));
 public static readonly PropertyName BearingLength = Row(nameof(BearingLength));
 public static readonly PropertyName JointGrout = Row(nameof(JointGrout));
 // POST-TENSIONING: anchorage token, duct diameter (the strand keeps its own NominalDiameter beside it), drape profile.
 public static readonly PropertyName AnchorageType = Row(nameof(AnchorageType));
 public static readonly PropertyName DuctDiameter = Row(nameof(DuctDiameter));
 public static readonly PropertyName TendonProfile = Row(nameof(TendonProfile));
 // CURTAIN-WALL/ALUMINUM product rows: MullionProfile the framing-profile token, ThermalBreak the isolator class,
 // GlazingPocket the pocket depth the IGU seat check reads.
 public static readonly PropertyName MullionProfile = Row(nameof(MullionProfile));
 public static readonly PropertyName ThermalBreak = Row(nameof(ThermalBreak));
 public static readonly PropertyName GlazingPocket = Row(nameof(GlazingPocket));
 // INSULATION: InstallMethod the placement mode; R-value rides ThermalResistance and facing FacerClass — no twins.
 public static readonly PropertyName InstallMethod = Row(nameof(InstallMethod));
 // MEMBRANE/BARRIER product rows: PermRating the vapour-permeance rating, BarrierClass the air/vapour barrier class
 // token; the seam lap stays the existing MembraneSeam, never a SeamMethod twin.
 public static readonly PropertyName PermRating = Row(nameof(PermRating));
 public static readonly PropertyName BarrierClass = Row(nameof(BarrierClass));
 // PIPEWORK: schedule, DN bore, and PressureClass — ONE pressure-classification row BOTH fluid-segment trades share;
 // the joint modality rides JointType over the widened Realization allowed-set.
 public static readonly PropertyName PipeSchedule = Row(nameof(PipeSchedule));
 public static readonly PropertyName PressureClass = Row(nameof(PressureClass));
 public static readonly PropertyName NominalBore = Row(nameof(NominalBore));
 // DUCTWORK product rows: DuctGauge the sheet-gauge token, SealClass the duct seal class, LinerClass the liner
 // class; the duct pressure classification rides PressureClass above.
 public static readonly PropertyName DuctGauge = Row(nameof(DuctGauge));
 public static readonly PropertyName SealClass = Row(nameof(SealClass));
 public static readonly PropertyName LinerClass = Row(nameof(LinerClass));
 // ELECTRICAL product rows — COMPONENT facts by law (substance constants live on MaterialPropertySet.Electrical;
 // ampacity never becomes a substance column).
 public static readonly PropertyName ConductorSize = Row(nameof(ConductorSize));
 public static readonly PropertyName InsulationClass = Row(nameof(InsulationClass));
 public static readonly PropertyName AmpacityBasis = Row(nameof(AmpacityBasis));
 // FIREPROOFING: applied thickness, the applied-SYSTEM tested R minutes it buys, SFRM density class —
 // FireResistanceEi stays the glazing EI row and the substance R/E/I the typed Fire case: three criteria, three owners.
 public static readonly PropertyName FireproofingThickness = Row(nameof(FireproofingThickness));
 public static readonly PropertyName RatingMinutes = Row(nameof(RatingMinutes));
 public static readonly PropertyName DensityClass = Row(nameof(DensityClass));
 // CLADDING realization row: AnchorType the embedded/tied anchor system (a stone panel carries a kerf ANCHOR and
 // stainless FASTENERS on one bag, so FastenerType stays the discrete fastener); panel thickness rides PanelThickness.
 public static readonly PropertyName AnchorType = Row(nameof(AnchorType));

 // Realization: OccurrenceWins; the JointType allowed-set closes the realizing modalities across the structural
 // AND fluid-segment trades — one token per modality, the set's own growth arm.
 public static readonly DetailSchema Realization =
  new("Realization", InheritanceMode.OccurrenceWins, Seq("Bolted", "Welded", "Bonded", "Bearing", "Cast", "Threaded", "Grooved", "Fused", "Compression", "Brazed"));

 public static readonly DetailSchema Product =
  new("Product", InheritanceMode.TypeDrivenOverride, Seq<string>());

 // Takeoff: TypeDrivenOverride — a type-level quantity takeoff drives every occurrence of its type (the IFC
 // QTO_TYPEDRIVENOVERRIDE rule), so the Bake type-bag merge inherits it without a per-occurrence mint; no joint set.
 public static readonly DetailSchema Takeoff =
  new("Takeoff", InheritanceMode.TypeDrivenOverride, Seq<string>());

 // Appearance: the content-keyed appearance bag on the appearance node's own Associate edge; two non-referencing
 // packages key on each row, so every static is owner-declared here.
 public static readonly DetailSchema Appearance =
  new("Appearance", InheritanceMode.TypeDrivenOverride, Seq<string>());

 public static readonly PropertyName TextureSet = Row(nameof(TextureSet));

 // DoubleSided has TWO producers (Bim IfcSurfaceSide, Materials OpenPBR thin-walled), so it declares HERE rather
 // than under either producer's category ([05] APPEARANCE_BAG_ESCAPE owns the summary-vs-bag law).
 public static readonly PropertyName DoubleSided = Row(nameof(DoubleSided));

 // SetName/InheritanceMode pinned by the schema, the omitted source deriving EvidenceGrade.Catalogue (ONE Option
 // entry, never a sibling overload pair).
 public PropertyBag Bag(Option<EvidenceGrade> source = default) =>
  new(SetName, Map<PropertyName, PropertyValue>(), Inheritance, source.IfNone(EvidenceGrade.Catalogue));

 // ADMISSION PATH keeps Bag and Quantities distinct, never value shape: a PropertyBag row crosses the Of gate at
 // write while a QuantityBag row arrives SI-coerced through MeasureValue — one generic mint erases that gate.
 public QuantityBag Quantities(Option<EvidenceGrade> source = default) =>
  new(SetName, Map<PropertyName, MeasureValue>(), Inheritance, source.IfNone(EvidenceGrade.Catalogue));

 // Joint mints the JointType row VALUE over THIS schema's closed allowed-set — the schema owns Allowed (Text-typed
 // tokens over the typed Enumerated members), so an out-of-set token rails ElementFault.ValueRejected at Of.
 public Fin<PropertyValue> Joint(string selected, Op key) => PropertyValue.Of(
  new PropertyValue.Enumerated(
   Seq<PropertyValue>(new PropertyValue.Text(selected.Trim())),
   JointTypes.Map(static token => (PropertyValue)new PropertyValue.Text(token))),
  key);
}
```

## [05]-[IMPLEMENTATION_LAW]

- [IFC_VALUE_FAMILY]: `PropertyValue` preserves the full scalar select in its fourteen-case union — string, measure, boolean, logical, arbitrary integer, finite real/number, binary, and temporal — beside the typed enumeration/reference/bounded/list/table/complex property forms. `CanonicalBytes` writes distinct ordinals and count-prefixed payloads, so `Text("1")`, `Integer(1)`, and `Number(1.0)` never alias even when a text renderer emits the same glyphs.
- [INHERITANCE_PRECEDENCE]: `InheritanceMode` mints the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` (and `PSET_*` sibling) inheritance rule as three rows — Bim ingress stamps the precedence DECISION onto the bag node, the mode owns the merge ALGEBRA as the generic `Resolve<V>` fold the ONE `ValueBag<V>.Merge` delegates to — so `Bake` merges type→occurrence once within the seam: type bag through the `Assign.TypeDefinition` edge, occurrence bag on its own node.
- [PERFORMANCE_DRIVEN_RESOLUTION]: IFC `PSET_PERFORMANCEDRIVEN` associates performance history rather than ruling merge precedence, so the Bim projector resolves it to `OccurrenceWins` at ingress and the three modes close merge precedence, never a per-template inheritance variant.
- [REALIZING_DETAIL_SCHEMA]: `DetailSchema.Realization` owns the `Rasm_ConnectionRealization` analogue under `OccurrenceWins` with the closed `JointType` allowed-set — fastener, weld, adhesive, stud, cast, rebar, and connector detail beside the shop-deliverable rows the Fabrication schedule folds read by name, the masonry EN 771 work-size tolerance rows, and the cmu occupancy-derived IFC profile-def token the `Rasm.Bim` egress profile lane resolves.
- [PRODUCT_DETAIL_SCHEMA]: `DetailSchema.Product` owns the panel, deck, and membrane product-form rows with the IGU build inputs the seed-time EN 673/EN 410/mass-law receipts compute from, under `TypeDrivenOverride` and no joint allowed-set.
- [TYPE_TAKEOFF_SCHEMA]: `DetailSchema.Takeoff` owns the type-level per-running-metre quantity rows (`MassPerLength`/`SurfaceAreaPerLength`/`VolumePerLength`) over the `QuantityBag` alias under `TypeDrivenOverride` inheritance — the IFC `QTO_TYPEDRIVENOVERRIDE` rule, so one Type-bound bag drives every occurrence through the `Bake` type-bag merge and a per-occurrence takeoff mint is the deleted form. Deriving a row stays the producing projector's obligation.
- [APPEARANCE_BAG_ESCAPE]: `DetailSchema.Appearance` carries the field-valued appearance facts the frozen `AppearanceSummary` refuses as columns, under `TypeDrivenOverride` on the appearance node's own `Associate` edge. `DoubleSided` stays off the summary: summary values answer how a painted face reflects while this one answers WHICH faces the material paints, so folding it into that frozen preimage re-keys every stored `Node.Appearance` on a fact no BSDF reads.
- [ROW_NAME_CUSTODY]: every bag and edge row name resolves to an owner-declared static — `StructuralRows` the cross-package structural vocabulary, `QuantityRows`/`EnvelopeRows`/`BoundaryRows`/`PortRows` the takeoff-quantity, building-envelope `Pset`, space-boundary-edge, and distribution-port vocabularies, `PropertyCategory` each producer's own — because non-referencing writer and reader peers fork a duplicated literal on the first rename.
- [OPEN_PROPERTY_KEY]: `PropertyName` stays an OPEN key at `[02]` for an ingested foreign `Pset` name, so authored rows need custody rather than a closed vocabulary.
- [WIRE_FAMILY_PROVISION]: provision follows the WIRE FAMILY, not the row — the restraint set is the whole `Dofs` roster and its `Releases` peer with the one positional `Frame` list and the `Offset` vector, and the applied-load set the `Force`/`Moment`/`PlanarForce`/`Start`/`End` axis families beside the `DeltaT` gradient family — so a component family only its producer writes still declares here, and a reader keys one roster rather than resolving each component against a different custody tier.
- [STATED_ANALYSIS_COLUMN]: an analysis column that asserts member BEHAVIOUR — the end release and the rigid-end offset — is stated by the producer that read it and never reconstructed by the consumer off a neighbouring row. A release recovered from a both-free support pair reads a braced pinned end as continuous, and an offset re-derived as half a `SupportedLength` moves a modelling decision to the reader that owns neither the support geometry nor the eccentricity constraint. The release rows carry the support rows' own three-way verdict shape (Boolean fixity, `Measure` spring, typed rejection) so ONE admission serves both wires; absence is the WHOLE `ReleaseCore` missing, which the reader carries as an option and refuses on by name rather than defaulting to the continuity the file never declared.
- [TAKEOFF_QUANTITY_IDENTITY]: `MeasureValue` carries its `QuantityType` identity on a `ValueBag` row exactly where EVERY producer of that row can name the identity truthfully, and takes the dimension-anonymous `MeasureValue.OfSi(Dimension, double)` mint only where a name is unspellable. Deleted form: a unit-bearing measure forking the key on a display token no identity carries.
- [TYPED_ROW_SOURCES]: TAKEOFF rows derive at projection from a registry quantity their producer names, so each carries the full type a costing or carbon consumer composes the `Properties/quantity#MEASURE_VALUE` algebra on, where a dimension-anonymous kg/m strands every downstream product. ROUND-TRIPPED rows carry the foreign measure-type name their importing projector resolved off its own roster, so each content-keys on the identity the file declared and re-exports typed rather than flattened.
- [ANONYMOUS_ROW_CONDITIONS]: rows stay ANONYMOUS on exactly two conditions — the value derives from no named quantity at all (a normalized fraction, a code factor, a bare-real source attribute), or two non-referencing peers mint it and only one holds a name, since a name one side cannot spell forks the content key the pair exists to share, which is why a realizing-detail row an authoring peer seeds and an importing peer recovers stays anonymous at BOTH ends.
- [QUANTITY_IDENTITY_REKEY]: moving a row from anonymous to typed RE-KEYS the bags it reaches exactly once, and no pin window is declared anywhere in the estate (the `RULINGS` `ValueBag` identity row establishes that precedent and its terms), so every corpus snapshot key derives at its own landing rather than as a migration.
- [PER_METRE_TAKEOFF_ROWS]: `MassPerLength` and `VolumePerLength` carry the `UnitsNet` registry identities `LinearDensity` (the kg/m signature an `Area × Density` product composes to) and `VolumePerLength` (the `AreaDim` m3/m signature), so both round-trip a display unit through `MeasureValue.In`; `SurfaceAreaPerLength` mints as a consumer name over `LengthDim` because m2/m reduces to m with no registry quantity — the NAME keeping a per-metre coating area from reading as a `Length` under `As`.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
