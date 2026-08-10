# [ELEMENT_PROPERTY]

`PropertyValue` closes the IFC-value family (`Text`/`Measure`/`Boolean`/`Logical`/`Integer`/`Number`/`Binary`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex`/`Temporal`) as one `[Union]`, so a property carries its data type rather than a stringly value; `PropertyName` keys it, the ONE `ValueBag<V>` generic carries it under the `PropertyBag`/`QuantityBag` aliases, and `InheritanceMode` owns the type→occurrence precedence fold.

`Rasm.Bim` keeps the IFC `Pset_*` roster, bSDD template resolution, geometry-true base-quantity derivation, and `IfcRelDefinesByProperties` round-trip. `DetailSchema` pins the neutral detail-bag and takeoff-quantity seams. `PropertyValue.Of` rails malformed scalar or composite values, and `CanonicalBytes` preserves every scalar discriminant so the same rendering never aliases different typed evidence.

## [01]-[INDEX]

- [02]-[PROPERTY_VALUE]: `PropertyValue` `[Union]` typed IFC-value family, `PropertyName` key, `Interpolation` table-curve rule, `TemporalValue` NodaTime-carried temporal leaf family, the fallible `Of` structural admission, the recursive `Remap` node-id rewrite and its `References` reachability dual the `Relations/relation#EDGE_ALGEBRA` `Generic` edge composes (renumber and cascade in lockstep), and the canonical `Render`/`CanonicalBytes` folds.
- [03]-[PROPERTY_BAG]: `ValueBag<V>` the ONE named inheritance-stamped value bag (`PropertyBag`/`QuantityBag` aliases), `InheritanceMode` `[SmartEnum]` owning the generic `Resolve<V>` precedence algebra, `PropertySource` rank, `GroupIdentity` dot-path group axis, and the type→occurrence `Merge` the `Bake` applies.
- [04]-[DETAIL_SCHEMA]: `DetailSchema` the ONE neutral schema over the bag aliases — the neutral `SetName`s the `Rasm.Bim` egress maps to IFC Psets, the stamped precedence, the `JointType` allowed-set, the canonical detail and takeoff `PropertyName` vocabulary, and the conforming `Bag`/`Quantities`/`Joint` factories.

## [02]-[PROPERTY_VALUE]

- Owner: `PropertyValue` the `[Union]` typed IFC-value family; `PropertyName` the `[ValueObject<string>]` property key; `Interpolation` the table-curve rule; `TemporalValue` the NodaTime temporal leaf family; the closed fourteen-case value vocabulary a property carries.
- Cases: `Text` (verbatim string) · `Measure` (SI-coerced `MeasureValue`) · `Boolean` (strict two-valued) · `Logical` (three-valued) · `Integer` (unbounded signed integer) · `Number` (finite IEEE-754 real) · `Binary` (byte-exact payload) · `Enumerated` (selected and allowed typed scalar members) · `Reference` (target and optional usage) · `Bounded` (lower/upper/setpoint measures) · `List` (ordered recursive values) · `Table` (defining→defined rows and interpolation) · `Complex` (named sub-properties) · `Temporal` (`Date`/`Moment`/`Time`/`Span`/`Stamp`). `PropertyValue` preserves the full `IfcValue` scalar family and the structured property forms without stringification.
- Entry: `PropertyValue.Of(value, key)` is the fallible admission a raw author crosses — railing `ElementFault.ValueRejected` on a non-finite `Number`, an empty/cross-type/inverted `Bounded`, a non-subset or composite-membered `Enumerated`, an empty `Table`, or an empty `Complex`, and recursively re-admitting nested values. `Integer` carries unbounded `BigInteger`, `Number` carries finite IEEE-754, and `Binary` carries byte-exact `Seq<byte>`; none collapse to `Text`.
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
// as a wire-keyed token only; the interpolation runs in Rasm.Compute, never on the seam.
[SmartEnum<string>]
public sealed partial class Interpolation {
 public static readonly Interpolation NotDefined = new("notdefined");
 public static readonly Interpolation Linear = new("linear");
 public static readonly Interpolation LogLinear = new("log-linear");
 public static readonly Interpolation LogLog = new("log-log");
}

// TemporalValue wraps the IFC temporal leaves (IfcDate/IfcDateTime/IfcTime/IfcDuration/IfcTimeStamp) the Temporal
// case carries — NodaTime-carried so the value compares on the calendar, never as a string spelling. Iso() is the
// ONE canonical projection render and hash share (the NodaTime ISO patterns are invariant by construction);
// CaseOrdinal discriminates the arm in the canonical bytes so grammar overlap between arms never aliases two values.
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
  number: p => double.IsFinite(p.Value) ? Fin.Succ((PropertyValue)p) : ElementFault.ValueRejected(key, $"<number-non-finite:{p.Value:R}>"),
  binary: static p => Fin.Succ((PropertyValue)p),
  reference: static p => Fin.Succ((PropertyValue)p),
  temporal: static p => Fin.Succ((PropertyValue)p),
  // Membership is TYPED record equality over the scalar arms (IfcValue admits no aggregate member), so a selected
  // "30" Text never matches a Measure(30) allowed row and a composite/reference member rails at admission.
  enumerated: p =>
   from allowed in p.Allowed.IsEmpty
    ? ElementFault.ValueRejected(key, "<enumerated-allowed-empty>")
    : p.Allowed.TraverseM(v => AdmitScalar(v, key, "<enumerated-member-not-scalar>")).As()
   from selected in p.Selected.TraverseM(v => AdmitScalar(v, key, "<enumerated-member-not-scalar>")).As()
   from _ in selected.Exists(s => !allowed.Contains(s))
    ? ElementFault.ValueRejected(key, "<enumerated-selected-not-allowed>")
    : Fin.Succ(unit)
   select (PropertyValue)new Enumerated(selected, allowed),
  bounded: p => AdmitBounded(p, key),
  list: p => p.Values.TraverseM(v => Of(v, key)).As().Map(static vs => (PropertyValue)new List(vs)),
  table: p => p.Rows.IsEmpty
   ? ElementFault.ValueRejected(key, "<table-rows-empty>")
   : p.Rows.TraverseM(r =>
      from defining in AdmitScalar(r.Defining, key, "<table-defining-not-scalar>")
      from defined in AdmitScalar(r.Defined, key, "<table-defined-not-scalar>")
      select (Defining: defining, Defined: defined))
     .As().Map(rows => (PropertyValue)new Table(rows, p.Interp)),
  complex: p => p.Properties.IsEmpty
   ? ElementFault.ValueRejected(key, "<complex-properties-empty>")
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
  logical: v => { w.Ordinal(3).Bool(v.Value.IsSome); v.Value.IfSome(b => w.Bool(b)); return w; },
  enumerated: v => { w.Ordinal(4).Ordinal(v.Selected.Count); foreach (PropertyValue s in v.Selected) { s.CanonicalBytes(w); } w.Ordinal(v.Allowed.Count); foreach (PropertyValue a in v.Allowed) { a.CanonicalBytes(w); } return w; },
  reference: v => { w.Ordinal(5).Bool(v.UsageName.IsSome); v.UsageName.IfSome(u => w.String(u)); return w.String(v.Target.Value); },
  bounded: v => { w.Ordinal(6).Bool(v.Lower.IsSome); v.Lower.IfSome(m => w.Measure(m)); w.Bool(v.Upper.IsSome); v.Upper.IfSome(m => w.Measure(m)); w.Bool(v.SetPoint.IsSome); v.SetPoint.IfSome(m => w.Measure(m)); return w; },
  list: v => { w.Ordinal(7).Ordinal(v.Values.Count); foreach (PropertyValue inner in v.Values) { inner.CanonicalBytes(w); } return w; },
  table: v => { w.Ordinal(8).String(v.Interp.Key).Ordinal(v.Rows.Count); foreach ((PropertyValue defining, PropertyValue defined) in v.Rows) { defining.CanonicalBytes(w); defined.CanonicalBytes(w); } return w; },
  complex: v => { w.Ordinal(9).String(v.UsageName).Ordinal(v.Properties.Count); foreach (KeyValuePair<PropertyName, PropertyValue> entry in v.Properties.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(entry.Key.Value); entry.Value.CanonicalBytes(w); } return w; },
  temporal: v => w.Ordinal(10).Ordinal(v.Value.CaseOrdinal).String(v.Value.Iso()),
  integer: v => WriteBytes(w.Ordinal(11), v.Value.ToByteArray(isUnsigned: false, isBigEndian: true)),
  number: v => w.Ordinal(12).Double(v.Value),
  binary: v => WriteBytes(w.Ordinal(13), v.Value.ToArray()));

 // Reference is the ONLY arm carrying a NodeId (Target rewrites via the compiler-generated `with`, so a future
 // field rides along untouched); List/Table/Complex recurse their children, every scalar arm is identity —
 // Enumerated included, because the Of scalar-member law makes a buried Reference unrepresentable there. A new
 // case breaks the rewrite at compile time; identity for an unmapped id is the caller's `map` contract.
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

 // References runs the recursive dual of Remap — every graph-node NodeId the value BURIES (the Reference target,
 // recursed through the List/Table/Complex composites) — so the Relations/relation#EDGE_ALGEBRA Generic edge's buried
 // attribute references are a LIVE reachability set the incidence index and the DropNode cascade sweep, symmetric with
 // Remap rewriting them (Remap renumbers, References reaches — an edge whose Members omitted a ref Remap still rewrote
 // is the deleted asymmetry that stranded a dangling attribute Reference). Scalar arms bury none; a new case breaks it
 // at compile time.
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

 // RenderMeasure is the ONE measure→string projection the Measure arm and the Bounded bounds share (a bound and a single Measure
 // render IDENTICALLY, never a unit-stripped fork): SI magnitude + canonical unit under the INVARIANT culture (a
 // decimal-comma locale must never fork a cross-runtime render). The unit is OPTIONAL at the model — a tally, a
 // consumer-minted type, and a dimension-anonymous product each carry none — and the blank is chosen HERE, at the
 // render boundary that owns display, so no fabricated token ever reaches the value the content key reads.
 private static string RenderMeasure(MeasureValue measure) =>
  measure.CanonicalUnit.Match(
   Some: unit => string.Create(CultureInfo.InvariantCulture, $"{measure.Si:R} {unit}"),
   None: () => string.Create(CultureInfo.InvariantCulture, $"{measure.Si:R}"));

 // Bound projects the three bound places over the shared RenderMeasure body — an absent bound renders "*" (the IFC
 // half-open interval).
 private static string Bound(Option<MeasureValue> bound) =>
  bound.Map(RenderMeasure).IfNone("*");

 // At least one of (lower, upper, setpoint) present, EVERY present member one QuantityType, a present lower/upper
 // PAIR ordered (lower.Si <= upper.Si). SetPoint is IfcPropertyBoundedValue.SetPointValue — an INDEPENDENT optional
 // NOMINAL with NO WHERE rule binding it inside the interval; a setpoint legitimately sits at or beyond a bound, so
 // a third pair check REJECTS valid IFC. v5 Seq.Head is Option<A>; the head-type read threads through Match.
 private static Fin<PropertyValue> AdmitBounded(Bounded b, Op key) {
  Seq<MeasureValue> present = Seq(b.Lower, b.Upper, b.SetPoint).Choose(static o => o);
  return present.Head.Match(
   None: () => ElementFault.ValueRejected(key, "<bounded-empty>"),
   Some: head => present.Tail.Exists(m => m.Type != head.Type)
    ? ElementFault.ValueRejected(key, "<bounded-cross-type>")
    : Inverted(b.Lower, b.Upper)
     ? ElementFault.ValueRejected(key, "<bounded-inverted>")
     : Fin.Succ((PropertyValue)b));
 }

 // Inverted only when BOTH ends are present and low.Si strictly exceeds high.Si — an open end is never inverted.
 private static bool Inverted(Option<MeasureValue> low, Option<MeasureValue> high) =>
  low.Match(Some: lo => high.Match(Some: hi => lo.Si > hi.Si, None: static () => false), None: static () => false);

 private static CanonicalWriter WriteBytes(CanonicalWriter writer, ReadOnlySpan<byte> bytes) =>
  writer.Ordinal(bytes.Length).Raw(bytes);

 // SCALAR membership is a TOTAL projection over the union, not a positional type-pattern list: the generated Map
 // forces one arm per case, so a fifteenth case is a BUILD ERROR here rather than a silent `false` that would admit
 // a composite into an Enumerated member or a Table cell and strand its buried References outside the reachability
 // sweep. The pattern-list form degraded exactly that way — a new case simply fell off the `or` chain.
 // IfcValue admits no aggregate member, so every composite arm reads false and every leaf arm true.
 public bool IsScalar => Map(
  text: true, measure: true, boolean: true, logical: true, integer: true,
  number: true, binary: true, temporal: true,
  enumerated: false, reference: false, bounded: false, list: false, table: false, complex: false);

 private static Fin<PropertyValue> AdmitScalar(PropertyValue value, Op key, string detail) =>
  value.IsScalar ? Of(value, key) : ElementFault.ValueRejected(key, detail);
}
```

## [03]-[PROPERTY_BAG]

- Owner: `ValueBag<V>` the ONE named inheritance-and-source-stamped value bag (`SetName` + `Map<PropertyName, V>` + `InheritanceMode` + `PropertySource` + `Map<string, GroupIdentity>`) — `PropertyBag` (`ValueBag<PropertyValue>`) the `Graph/element#NODE_MODEL` `Node.PropertySet` case wraps and `QuantityBag` (`ValueBag<MeasureValue>`) the `Node.QuantitySet` case wraps are its two GLOBAL `using` aliases, the value type the only varying axis so it rides a type parameter (the SHAPE_BUDGET + DERIVED_TYPES collapse); `InheritanceMode` owns the type→occurrence precedence fold; `PropertySource` owns catalogue/import/derived/user source rank; `GroupIdentity` owns the dot-path group axis.
- Entry: `ValueBag<V>.Merge(type, occurrence)` folds a type-bound bag and an occurrence bag into one by delegating to `occurrence.Inheritance.Resolve(type.Values, occurrence.Values)` — the ONE generic precedence fold the mode owns — preserves the higher `PropertySource` rank, and unions the `Groups` maps occurrence-first, so the `Graph/element#ELEMENT_GRAPH` `Bake` applies inheritance once per bag; `ValueBag<V>.Empty(setName, inheritance, source)` mints an empty source-stamped bag; `With(name, value)`/`Find(name)` are the immutable add and keyed read both alias kinds share.
- Auto: `Resolve<V>` dispatches the generated total `Switch` over the LanguageExt `Map` three-argument `Fold` — `OccurrenceWins` folds type entries onto the occurrence map adding only absent keys, `TypeDrivenOverride` folds with `AddOrUpdate` (type-wins), `TypeDrivenOnly` returns the type map — one generic fold serving both bag aliases identically, the mode (not the bag) owning the precedence; the `[SmartEnum<string>]` round-trips the mode token at the wire so a persisted bag re-admits its precedence; the mode is stamped at Bim ingress, the seam never inferring it.
- Receipt: the merged `ValueBag<V>` is the typed property evidence the `Bake`-derived `Element` carries flat in its `Seq<PropertyBag>`/`Seq<QuantityBag>` fields, so a consumer reads `element.Properties.Find(b => b.SetName == set).Bind(b => b.Find(name))` as one `Option<PropertyValue>`; `Source` records whether the winning bag came from Materials catalogue data, IFC import, Bim-derived quantities, or Rhino/user override — Compute assessments and Persistence provenance stay typed nodes/events, not bag sources.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[SmartEnum<int>]`), LanguageExt.Core (`Map`/`Option`/`Seq`), Generator.Equals (`[Equatable]`/`[UnorderedEquality]` — the bag is the drillable equality owner the `StructuralMerge` descends through to `Bag.Values[name]`).
- Growth: a new bag attribute shared by all bags is one column on `ValueBag<V>` (both aliases gain it); a new value kind a bag carries is one `using` alias over `ValueBag<TNew>`; a new inheritance precedence is one `InheritanceMode` row carrying its `Resolve` arm; a new source tier is one `PropertySource` row with its rank; a new grouping fact is one column on `GroupIdentity` (both the canonical bytes and the wire gain it in the same edit).
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

// PropertySource keys on a comparable int, which grants the generated ordering and relational operators — no
// comparer accessor exists or is needed for int (ComparerAccessors carries only the string accessors and Default<T>).
[SmartEnum<int>]
public sealed partial class PropertySource {
 public static readonly PropertySource Catalogue = new(10, "catalogue");
 public static readonly PropertySource Import = new(20, "import");
 public static readonly PropertySource Derived = new(30, "derived");
 public static readonly PropertySource User = new(40, "user");

 public string Token { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// GroupIdentity carries the grouping identity a bag's dot-path prefix owns — the classifying Discrimination beside the
// qualifying Quality/Usage pair. Every column is absence-carrying: an unstated grouping string is UNSET, and lifting
// it to an empty spelling re-authors a group the source never qualified.
public sealed record GroupIdentity(Option<string> Discrimination, Option<string> Quality, Option<string> Usage);

// ValueBag<V> varies on the value type ALONE — a TYPE PARAMETER, never a parallel bag pair. [Equatable] is
// LOAD-BEARING: StructuralMerge drills a changed property to Nodes[id].Bag.Values[name] — a plain record bag is an
// opaque equality leaf forcing whole-bag replacement; Values and Groups ride [UnorderedEquality] (order-independent
// per-entry diffs); V is the ATOMIC leaf (PropertyValue/MeasureValue own value equality — [Equatable] there is
// ceremony). Groups keys on the dot-path PREFIX a flattened group mints, so its member rows are the `<prefix>.`
// values in Values and a bag that groups nothing carries the empty map (the LanguageExt Map default), so the column
// trails with a default no existing construction spells.
[Equatable]
public sealed partial record ValueBag<V>(string SetName, [property: UnorderedEquality] Map<PropertyName, V> Values, InheritanceMode Inheritance, PropertySource Source, [property: UnorderedEquality] Map<string, GroupIdentity> Groups = default) {
 public static ValueBag<V> Empty(string setName, InheritanceMode inheritance, PropertySource source) =>
  new(setName, Map<PropertyName, V>(), inheritance, source);

 public Option<V> Find(PropertyName name) => Values.Find(name);

 public ValueBag<V> With(PropertyName name, V value) =>
  this with { Values = Values.AddOrUpdate(name, value) };

 // Values take the STAMPED mode's precedence; Groups take KEY UNION with occurrence-wins on collision under NO
 // mode, because a grouping identity is the occurrence's own evidence of how its rows nest — the type contributes
 // only prefixes the occurrence never grouped, and a type-driven override would re-label an occurrence's own group.
 public static ValueBag<V> Merge(ValueBag<V> type, ValueBag<V> occurrence) {
  Map<PropertyName, V> inherited = occurrence.Inheritance.Resolve(type.Values, occurrence.Values);
  PropertySource source = occurrence.Source >= type.Source ? occurrence.Source : type.Source;
  Map<string, GroupIdentity> groups = type.Groups.Fold(occurrence.Groups, static (acc, prefix, group) => acc.ContainsKey(prefix) ? acc : acc.Add(prefix, group));
  return occurrence with { Values = inherited, Source = source, Groups = groups };
 }
}
```

## [04]-[DETAIL_SCHEMA]

- Owner: `DetailSchema` the ONE neutral schema mechanism over the `ValueBag<V>` aliases — a neutral `SetName`, an `InheritanceMode`, and an optional `JointType` allowed-set — and the canonical `PropertyName` vocabulary both bag families key on; `PropertyCategory` the owner-blessed producer scope every package mints its own row names through; `StructuralRows` the cross-package restraint, load, and topology vocabulary a Bim projector stamps onto a `Generic` edge and a Compute runner reads back, with `QuantityRows`, `EnvelopeRows`, and `BoundaryRows` its siblings over the baked base-quantity takeoff, the building-envelope `Pset` rows, and the space-boundary edge payload. `DetailSchema.Realization` owns realizing fastener/rebar/connector/joint detail with the masonry work-size tolerance and cmu profile-subtype rows; `DetailSchema.Product` owns panel board/deck/membrane product geometry with the IGU build rows; `DetailSchema.Takeoff` owns the type-level per-running-metre quantity rows; `DetailSchema.Appearance` owns the appearance node's own bag — `TextureSet` the baked-set content address and `DoubleSided` the render-sidedness bit — the RULINGS-landed escape hatch that keeps the frozen `AppearanceSummary` preimage from widening.
- Entry: `PropertyCategory.<scope>.Row(name)` mints a producer-scoped row name, `PropertyCategory.Seam` carrying the empty prefix so the schema's own statics keep the bare names an IFC round-trip froze; `StructuralRows.Translation`/`Rotation`/`Warping` project the restraint families and `Dofs` reads the whole degree-of-freedom roster, `Force`/`Moment`/`PlanarForce`/`Start`/`End` the applied-load component families and `DeltaT` the `Gradients`-keyed thermal family; `QuantityRows.SurfaceArea`/`Volume` project the ordered net-over-gross takeoff chains a reader folds first-hit-wins; `DetailSchema.Realization` the canonical realizing schema; `DetailSchema.Product` the canonical product-detail schema; `DetailSchema.Takeoff` the canonical type-quantity schema; `DetailSchema.Appearance` the canonical appearance-bag schema its `TextureSet`/`DoubleSided` rows key on; `schema.Bag(source = default)` mints the empty conforming source-stamped `PropertyBag` and `schema.Quantities(source = default)` its `QuantityBag` counterpart, the omitted source deriving `PropertySource.Catalogue`; `schema.Joint(selected, key)` the `JointType` row VALUE as a `PropertyValue.Enumerated` over the schema's closed allowed-set, railed because the token crosses the `Of` admission.
- Auto: `Bag` and `Quantities` pin `SetName` and `InheritanceMode` from the schema and stamp the resolved source rank, so neither author nor reader hand-spells the set-name string, re-stamps precedence, or drops source rank; `Joint(selected)` constructs the typed `PropertyValue.Enumerated` over `Text`-wrapped tokens (the selected token against the schema's closed `JointTypes` allowed-set) so the `Properties/property#PROPERTY_VALUE` `Of` admission holds.
- Receipt: the conforming `PropertyBag` lands on the seam `ElementGraph` as a `Graph/element#NODE_MODEL` `Node.PropertySet` and the conforming `QuantityBag` as a `Node.QuantitySet`, each bound by one `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge, the `Bake` fold merging them into `element.Properties` and `element.Quantities` — a takeoff bound to a Type reaches every occurrence through that same type-bag merge, so no occurrence re-mints it; both bags mint through `NodeId.Content` over `Node.ToCanonicalBytes` (id excluded) so two structurally-identical bags dedup to one node, never a second `(GeometryKey, DetailKey)` hasher.
- Packages: LanguageExt.Core (`Seq`/`Map` + the `Prelude` constructors), Thinktecture.Runtime.Extensions (the `PropertyName` `Create` factory + the `InheritanceMode` statics), `Properties/quantity#MEASURE_VALUE` (both `MeasureValue.OfSi` mints — the typed identity and the dimension-anonymous fallback the bag law elects between), and the seam `PropertyBag`/`PropertyValue`/`PropertyName`/`InheritanceMode` owners this cluster composes.
- Growth: a new producer scope is one `PropertyCategory` row and a new producer-local row family is one static roster in the owning package minted through that row's `Row`; a new structural axis is one `StructuralRows.Axes` entry every coordinate family absorbs and a new thermal gradient one `Gradients` entry, while a family keyed on neither — a warping/bimoment restraint, a further torsional degree of freedom — is one `Family(stem, keys)` call carrying its own roster and one `Dofs` term, never a seventh entry bent into `Axes` that every coordinate family then answers for; a new realizing-detail, product, or takeoff row is one `static readonly PropertyName` the author writes and the reader reads by name; a new joint modality is one token on `Realization.JointTypes`; a material-property→`Pset` bag is ANOTHER `DetailSchema` instance — ONE schema mechanism over the bag aliases, never a parallel schema type, a per-row bag class, or a per-call-site allowed-set literal.
- Boundary: `DetailSchema` is the ONE seam-declared detail contract and the seam carries NO IFC name — the neutral `SetName` is what both the in-graph bag and the schema carry, while the IFC Pset name (`Rasm_ConnectionRealization`), the `Pset_*` roster, the bSDD resolution, the egress mapping, and the `GlobalId` assignment stay in the `Rasm.Bim` `SemanticProjector`.
- Boundary: a cross-peer realizing invariant (a fastener diameter against its member, a weld throat against its leg) is a `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`, never an IFC column on this bag.
- Boundary: the realizing element's MATERIAL binding (grade, capacity, embodied carbon, classification, appearance) rides the `Rasm.Materials` projector's `Associate` edge, never a `SteelGrade`/`EmbodiedCarbon` row here; the joint TOPOLOGY rides the `Connect` edge's `Connect.Realizing` `Option<NodeId>` field, never a detail row.
- Boundary: an authored bag carrying a subset of rows is a faithfully different node, never a forced byte-match.
- Boundary: a bag row's `MeasureValue` carries its `QuantityType` where every producer of that row can name the identity truthfully and stays dimension-anonymous otherwise — ONE law over both bag families, stated at `[05]` `[TAKEOFF_QUANTITY_IDENTITY]` and never restated per family.
- Boundary: `InheritanceMode` stays the bag-merge precedence the schema stamps; `[03]` owns its disjunction from the `Bake` inheritance.
- Boundary: the key space closes over OWNER PROVISION — a name two packages key on is a static here, a name one package owns is a static in that package minted through its `PropertyCategory` row, and a call-site `PropertyName.Create` in any writer or reader is the fork between non-referencing peers this pair deletes, so a projector's ingest-only enrichment row is either promoted here the moment a consumer keys on it or carried under its own category.
- Boundary: provision follows the WIRE FAMILY, not the row — ONE law over every crossing family, stated at `[05]` `[WIRE_FAMILY_PROVISION]` and never restated per family.

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
// StructuralRows carries the restraint, load, and topology rows a Bim projector STAMPS onto a Generic
// edge's attribute map and a Compute analysis runner READS back. Both packages are non-referencing peers, so a
// literal at either end forks silently the moment one side renames — these statics are the single spelling both
// compose, and every component family generates off its own key roster rather than enumerating, so a new axis or
// gradient is unreachable by typo. They key an EDGE attribute map rather than a DetailSchema bag, which is why they
// sit beside the schema rather than inside it.
public static class StructuralRows {
 public static readonly PropertyName AtStart = PropertyName.Create("AtStart");
 public static readonly PropertyName Station = PropertyName.Create("Station");
 public static readonly PropertyName SupportedLength = PropertyName.Create("SupportedLength");
 public static readonly PropertyName Frame = PropertyName.Create("Frame");
 public static readonly PropertyName LoadKind = PropertyName.Create("LoadKind");
 public static readonly PropertyName Case = PropertyName.Create("Case");

 // Axis is the whole discriminant of a COORDINATE family, so each one is a projection over it and a seventh degree
 // of freedom or a second load component set lands as one axis row rather than three hand-typed statics per family.
 public static readonly Seq<string> Axes = Seq("X", "Y", "Z");

 // Temperature keys its gradient family on the action's own shape — a uniform rise with two bending gradients —
 // never on the coordinate axes, so it hands its own roster to the same mint instead of bending Axes to fit it.
 public static readonly Seq<string> Gradients = Seq("Constant", "Y", "Z");

 // Warping is the SEVENTH degree of freedom — the bimoment/warping restraint an open thin-walled section carries
 // under the EN 1993-1-1 §6.3.3 / AISC 360 App.1 torsional-buckling routes. It keys on the MEMBER AXIS alone, never
 // on the coordinate triple, so it hands its own single-key roster to the same Family mint: the growth arm the
 // six-DOF roster admits is a NEW FAMILY row, never a seventh entry bent into Axes that every coordinate family
 // then answers for.
 public static readonly Seq<string> WarpingKeys = Seq("Axial");

 public static readonly Map<string, PropertyName> Translation = Family("Translation");
 public static readonly Map<string, PropertyName> Rotation = Family("Rotation");
 public static readonly Map<string, PropertyName> Warping = Family("Warping", Some(WarpingKeys));
 public static readonly Map<string, PropertyName> Force = Family("Force");
 public static readonly Map<string, PropertyName> Moment = Family("Moment");
 public static readonly Map<string, PropertyName> PlanarForce = Family("PlanarForce");
 public static readonly Map<string, PropertyName> Start = Family("Start");
 public static readonly Map<string, PropertyName> End = Family("End");
 public static readonly Map<string, PropertyName> DeltaT = Family("DeltaT", Some(Gradients));

 // Fixity and stiffness ride ONE row per degree of freedom, the value's own PropertyValue case carrying whether the
 // producer stamped a boolean restraint or a spring MeasureValue — a parallel `<dof>Stiffness` roster is the twin
 // that strands the spring magnitude on every reader keying only the boolean.
 public static Seq<PropertyName> Dofs => Translation.Values.ToSeq() + Rotation.Values.ToSeq() + Warping.Values.ToSeq();

 // ShearLink rows carry the RC triple a Materials capacity screen STAMPS and the Compute member check READS — the
 // two-leg link area, the link design yield, and the section-decidable web-crushing ceiling, each a measured
 // PropertyValue in SI; the same non-referencing-peer custody every row here holds, absence meaning the section
 // carries no links or its grade published no yield.
 public static readonly PropertyName ShearLinkArea = PropertyName.Create("ShearLinkArea");
 public static readonly PropertyName ShearLinkYield = PropertyName.Create("ShearLinkYield");
 public static readonly PropertyName ShearLinkCeiling = PropertyName.Create("ShearLinkCeiling");

 // ONE mint per row family: the key roster is the whole discriminant and Axes is its canonical default, so a
 // coordinate family reads Family(stem) and a family keyed on its own action shape hands its roster in.
 private static Map<string, PropertyName> Family(string stem, Option<Seq<string>> keys = default) =>
  keys.IfNone(Axes).Fold(Map<string, PropertyName>(), (map, key) => map.Add(key, PropertyName.Create($"{stem}{key}")));
}

// QuantityRows carries the baked base-quantity takeoff a Bim or Fabrication projector STAMPS onto a QuantitySet and
// every Rasm.Compute analysis runner READS back — the same non-referencing-peer custody StructuralRows holds, one
// wire further out. The names are the bare IFC Qto_*BaseQuantities spellings a round-trip froze, so they carry the
// Seam empty prefix. The ordered CHAINS are row data too: net-over-gross is one preference every discipline reads
// rather than four re-spellings of the same fallback that drift apart on the first added row.
public static class QuantityRows {
 public static readonly PropertyName Area = PropertyName.Create("Area");
 public static readonly PropertyName NetArea = PropertyName.Create("NetArea");
 public static readonly PropertyName NetSideArea = PropertyName.Create("NetSideArea");
 public static readonly PropertyName GrossSideArea = PropertyName.Create("GrossSideArea");
 public static readonly PropertyName NetFloorArea = PropertyName.Create("NetFloorArea");
 public static readonly PropertyName NetVolume = PropertyName.Create("NetVolume");
 public static readonly PropertyName GrossVolume = PropertyName.Create("GrossVolume");
 public static readonly PropertyName Width = PropertyName.Create("Width");
 public static readonly PropertyName GlazingArea = PropertyName.Create("GlazingArea");
 public static readonly PropertyName GlazingPerimeter = PropertyName.Create("GlazingPerimeter");

 // Fabrication lowers NestYield.WasteAreaMm2 onto the element's quantity bag under this row, so off-cut waste joins the
 // material basis the carbon and cost folds distribute by.
 public static readonly PropertyName NestWasteArea = PropertyName.Create("NestWasteArea");

 // Bag set names the projector stamps: a set-scoped read narrows to one bag, an unscoped one scans every bound bag.
 public const string SpaceBaseQuantities = "Qto_SpaceBaseQuantities";

 public static readonly Seq<PropertyName> SurfaceArea = Seq(NetSideArea, NetArea, GrossSideArea);
 public static readonly Seq<PropertyName> Volume = Seq(NetVolume, GrossVolume);
}

// EnvelopeRows carries the element PropertySet rows a building-envelope physics or energy runner reads and a Bim
// projector writes. They sit beside the schema rather than inside it for the StructuralRows reason: the producer and the
// consumer are non-referencing peers, and a literal at either end forks on the first rename.
public static class EnvelopeRows {
 // Spacer Ψg the glazing family lowers onto the ELEMENT Pset, never onto MaterialPropertySet.Thermal, which carries
 // no perimeter-bridge column.
 public static readonly PropertyName GlazingEdgePsi = PropertyName.Create("GlazingEdgePsi");
 public static readonly PropertyName IsExternal = PropertyName.Create("IsExternal");

 public const string SpaceCommon = "Pset_SpaceCommon";
}

// BoundaryRows carries the space-boundary edge payload a Bim projector stamps onto the neutral Generic edge and the
// energy, acoustic, and circulation runners read back — Host discriminating an OPENING from an opaque base surface,
// Level the 1st/2nd boundary generation an export declares (a blank reading 1st-equivalent).
public static class BoundaryRows {
 public static readonly PropertyName Host = PropertyName.Create("Host");
 public static readonly PropertyName Level = PropertyName.Create("BoundaryLevel");
}

// DetailSchema declares the ONE NEUTRAL detail schema over PropertyBag — authored by the Materials Component
// projection, round-tripped by the Bim Semantics/connection reader; the Bim SemanticProjector maps SetName to the
// IFC Pset at Emit, never the seam.
public sealed record DetailSchema(string SetName, InheritanceMode Inheritance, Seq<string> JointTypes) {
 // Row-name statics below carry the canonical NEUTRAL vocabulary both author and reader key on. PropertyName itself
 // stays an OPEN key (any Pset property name admits through PROPERTY_VALUE Of); these statics are the seam rows the
 // Component projection writes and the Bim reader recovers one-hop, never a closed key vocabulary.
 public static readonly PropertyName JointType = PropertyName.Create("JointType");
 public static readonly PropertyName FastenerType = PropertyName.Create("FastenerType");
 public static readonly PropertyName AccessoryType = PropertyName.Create("AccessoryType");
 public static readonly PropertyName BarType = PropertyName.Create("BarType");
 public static readonly PropertyName NominalDiameter = PropertyName.Create("NominalDiameter");
 public static readonly PropertyName NominalLength = PropertyName.Create("NominalLength");
 public static readonly PropertyName CrossSectionArea = PropertyName.Create("CrossSectionArea");
 public static readonly PropertyName CarriedMemberWidth = PropertyName.Create("CarriedMemberWidth");
 public static readonly PropertyName CarriedMemberDepth = PropertyName.Create("CarriedMemberDepth");
 public static readonly PropertyName EffectiveThroat = PropertyName.Create("EffectiveThroat");
 public static readonly PropertyName BondLine = PropertyName.Create("BondLine");
 public static readonly PropertyName Overlap = PropertyName.Create("Overlap");
 // MASONRY realization rows carry the EN 771 work-vs-actual size band (tolerance class T1/T2/Tm, range class
 // R1/R2/Rm, special-shape token), UnitHeight the bed-plane unit height (the W×L profile carries width and length,
 // so height has no other landing surface), and CourseHeight the coursing height (unit height + bed joint) that
 // coursing tolerance and GLB tessellation read off the laid unit's bag.
 public static readonly PropertyName SizeTolerance = PropertyName.Create("SizeTolerance");
 public static readonly PropertyName SizeRange = PropertyName.Create("SizeRange");
 public static readonly PropertyName SpecialShape = PropertyName.Create("SpecialShape");
 public static readonly PropertyName UnitHeight = PropertyName.Create("UnitHeight");
 public static readonly PropertyName CourseHeight = PropertyName.Create("CourseHeight");
 // CMU realization row carries the occupancy-derived IFC profile-def subtype token (IfcArbitraryProfileDefWithVoids
 // iff any ungrouted cell, IfcRectangleProfileDef solid/fully-grouted) that the cmu seed computes off its fill-state
 // lattice and the Bim egress profile lane reads to select the authored profile entity: derivation stays
 // Materials-owned, the wire carries the datum.
 public static readonly PropertyName ProfileSubtype = PropertyName.Create("ProfileSubtype");
 // SHOP-DELIVERABLE realization rows carry what the reinforcement, joint, fastener, and connector arms author and the
 // Fabrication schedule folds read by name: bend identity and the full bend block, weld part thickness and prep,
 // stud grade, fastener form, the connector's stamped plate, and the evaluation-report identity a sourced
 // allowable carries.
 public static readonly PropertyName BendShapeCode = PropertyName.Create("BendShapeCode");
 public static readonly PropertyName BendSchedule = PropertyName.Create("BendSchedule");
 public static readonly PropertyName PartThickness = PropertyName.Create("PartThickness");
 public static readonly PropertyName WeldPrep = PropertyName.Create("WeldPrep");
 public static readonly PropertyName StudGrade = PropertyName.Create("StudGrade");
 public static readonly PropertyName FastenerForm = PropertyName.Create("FastenerForm");
 public static readonly PropertyName ConnectorPlate = PropertyName.Create("ConnectorPlate");
 public static readonly PropertyName EvaluationReport = PropertyName.Create("EvaluationReport");
 // PANEL product rows carry what the Component panel arm authors and a sheathing generator round-trips: EdgeProfile
 // names the board-edge token, PanelThickness/BoardLength the board build, FieldSpacing/EdgeSpacing the fastener
 // station pitches, RibDepth/RibPitch/DeckForm the steel-deck corrugation, MembraneSeam the membrane lap, and
 // PanelOrientation the strength-axis token, while CoreClass/SpanRating/BondClass/FoamClass/FacerClass/ThermalResistance
 // bound the board's operating envelope.
 public static readonly PropertyName EdgeProfile = PropertyName.Create("EdgeProfile");
 public static readonly PropertyName PanelThickness = PropertyName.Create("PanelThickness");
 public static readonly PropertyName FieldSpacing = PropertyName.Create("FieldSpacing");
 public static readonly PropertyName EdgeSpacing = PropertyName.Create("EdgeSpacing");
 public static readonly PropertyName RibDepth = PropertyName.Create("RibDepth");
 public static readonly PropertyName RibPitch = PropertyName.Create("RibPitch");
 public static readonly PropertyName MembraneSeam = PropertyName.Create("MembraneSeam");
 public static readonly PropertyName BoardLength = PropertyName.Create("BoardLength");
 public static readonly PropertyName PanelOrientation = PropertyName.Create("PanelOrientation");
 public static readonly PropertyName CoreClass = PropertyName.Create("CoreClass");
 public static readonly PropertyName SpanRating = PropertyName.Create("SpanRating");
 // RoofSpan and FloorSpan carry the MEASURED spans (mm) beside the SpanRating TOKEN: a deck or board publishes a
 // rated span per use, and the roof and floor cases carry different values on one product, so the pair is two rows
 // rather than one row a consumer disambiguates by reading which use it happened to be looking at. Materials' panel
 // projector is the sole producer — it converts the rating token — and a span consumer reads these rows, never the token.
 public static readonly PropertyName RoofSpan = PropertyName.Create("RoofSpan");
 public static readonly PropertyName FloorSpan = PropertyName.Create("FloorSpan");
 // RoofSpanUnsupported, CompressiveStrength, and FasteningMethod carry the unsupported roof span beside the rated
 // one, the sandwich-core compressive strength, and the fastening-method token — the panel arm's remaining product
 // facts, each published by the one panel projector.
 public static readonly PropertyName RoofSpanUnsupported = PropertyName.Create("RoofSpanUnsupported");
 public static readonly PropertyName CompressiveStrength = PropertyName.Create("CompressiveStrength");
 public static readonly PropertyName FasteningMethod = PropertyName.Create("FasteningMethod");
 public static readonly PropertyName BondClass = PropertyName.Create("BondClass");
 public static readonly PropertyName FoamClass = PropertyName.Create("FoamClass");
 public static readonly PropertyName FacerClass = PropertyName.Create("FacerClass");
 public static readonly PropertyName ThermalResistance = PropertyName.Create("ThermalResistance");
 public static readonly PropertyName DeckForm = PropertyName.Create("DeckForm");
 // IGU product rows carry the glazing build inputs the seed-time EN 673 `Ug` / EN 410 `g`/`τv` / mass-law `Rw`
 // receipts compute from: PaneBuild/CavityBuild recursive List-of-Complex rows (per-pane optics/coating, per-cavity
 // gas/width), the EN 1279-2 EdgeSeal, the SpacerType, the MuntinGrid, and the EI fire-resistance minutes.
 public static readonly PropertyName PaneBuild = PropertyName.Create("PaneBuild");
 public static readonly PropertyName CavityBuild = PropertyName.Create("CavityBuild");
 public static readonly PropertyName SpacerType = PropertyName.Create("SpacerType");
 public static readonly PropertyName EdgeSeal = PropertyName.Create("EdgeSeal");
 public static readonly PropertyName MuntinGrid = PropertyName.Create("MuntinGrid");
 public static readonly PropertyName FireResistanceEi = PropertyName.Create("FireResistanceEi");
 // Takeoff rows carry the per-running-metre quantities a projector mints ONCE off the resolved section and the
 // substance density: MassPerLength the kg/m linear mass a tonnage and cost join reads, SurfaceAreaPerLength the
 // m2/m coating and fire-protection area, VolumePerLength the m3/m material volume. Their sole producer names each
 // identity off the registry, so every value carries its own QuantityType and Dimension under the bag law's naming
 // condition — a takeoff consumer multiplies through the measure algebra, where an anonymous kg/m strands the product.
 public static readonly PropertyName MassPerLength = PropertyName.Create("MassPerLength");
 public static readonly PropertyName SurfaceAreaPerLength = PropertyName.Create("SurfaceAreaPerLength");
 public static readonly PropertyName VolumePerLength = PropertyName.Create("VolumePerLength");

 // Realization: OccurrenceWins — a re-imported occurrence value wins the type default; the JointType allowed-set
 // closes the realizing modalities. Product: TypeDrivenOverride — product form is type-driven, no joint set.
 public static readonly DetailSchema Realization =
  new("Realization", InheritanceMode.OccurrenceWins, Seq("Bolted", "Welded", "Bonded", "Bearing", "Cast"));

 public static readonly DetailSchema Product =
  new("Product", InheritanceMode.TypeDrivenOverride, Seq<string>());

 // Takeoff: TypeDrivenOverride — a type-level quantity takeoff drives every occurrence of its type (the IFC
 // QTO_TYPEDRIVENOVERRIDE rule), so the Bake type-bag merge inherits it without a per-occurrence mint; no joint set.
 public static readonly DetailSchema Takeoff =
  new("Takeoff", InheritanceMode.TypeDrivenOverride, Seq<string>());

 // Appearance: TypeDrivenOverride — the Materials projector links a content-keyed appearance bag onto the
 // appearance node's own Associate edge (the baked-set address under TextureSet, the double-sided shell flag under
 // DoubleSided), and the Bim graph reader resolves both back; two non-referencing packages key on each row, so
 // every static is owner-declared here.
 public static readonly DetailSchema Appearance =
  new("Appearance", InheritanceMode.TypeDrivenOverride, Seq<string>());

 public static readonly PropertyName TextureSet = PropertyName.Create("TextureSet");

 // DoubleSided reaches this bag from TWO producers — the Bim IfcSurfaceSide attribute an imported style declares
 // and the Materials OpenPBR thin-walled row a projected appearance carries — which is why it declares HERE rather
 // than under either producer's PropertyCategory ([05] APPEARANCE_BAG_ESCAPE owns the summary-vs-bag law).
 public static readonly PropertyName DoubleSided = PropertyName.Create("DoubleSided");

 // SetName/InheritanceMode pinned by the schema, the omitted source deriving PropertySource.Catalogue (ONE Option
 // entry, never a sibling overload pair).
 public PropertyBag Bag(Option<PropertySource> source = default) =>
  new(SetName, Map<PropertyName, PropertyValue>(), Inheritance, source.IfNone(PropertySource.Catalogue));

 // Quantities mints the QUANTITY bag beside Bag, pinning SetName, precedence, and source rank identically so a
 // takeoff author hand-spells none of them. ADMISSION PATH keeps the two entries distinct, never value shape: a
 // PropertyBag row crosses the PROPERTY_VALUE Of structural gate at write time while a QuantityBag row arrives
 // already SI-coerced and finite-gated through MeasureValue, so one mint over a type parameter erases the gate
 // each family answers to and spells its argument at every existing call site besides.
 public QuantityBag Quantities(Option<PropertySource> source = default) =>
  new(SetName, Map<PropertyName, MeasureValue>(), Inheritance, source.IfNone(PropertySource.Catalogue));

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
- [ROW_NAME_CUSTODY]: every bag and edge row name resolves to an owner-declared static — `StructuralRows` the cross-package structural vocabulary, `QuantityRows`/`EnvelopeRows`/`BoundaryRows` the takeoff-quantity, building-envelope `Pset`, and space-boundary-edge vocabularies, `PropertyCategory` each producer's own — because non-referencing writer and reader peers fork a duplicated literal on the first rename.
- [OPEN_PROPERTY_KEY]: `PropertyName` stays an OPEN key at `[02]` for an ingested foreign `Pset` name, so authored rows need custody rather than a closed vocabulary.
- [WIRE_FAMILY_PROVISION]: provision follows the WIRE FAMILY, not the row — the restraint set is the whole `Dofs` roster with the one positional `Frame` list, and the applied-load set the `Force`/`Moment`/`PlanarForce`/`Start`/`End` axis families beside the `DeltaT` gradient family — so a component family only its producer writes still declares here, and a reader keys one roster rather than resolving each component against a different custody tier.
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
