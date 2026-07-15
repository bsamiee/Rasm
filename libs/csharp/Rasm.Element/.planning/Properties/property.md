# [ELEMENT_PROPERTY]

The typed property vocabulary the `PropertySet`/`QuantitySet` bag nodes carry: one `PropertyValue` `[Union]` closing the IFC-value family (`Text`/`Measure`/`Boolean`/`Logical`/`Integer`/`Number`/`Binary`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex`/`Temporal`) so a property carries its data type rather than a stringly value, one `PropertyName` key, the ONE `ValueBag<V>` generic the `PropertyBag`/`QuantityBag` aliases close over, and the `InheritanceMode` vocabulary owning the type→occurrence precedence fold. The IFC `Pset_*` roster, bSDD template resolution, geometry-true base-quantity derivation, and `IfcRelDefinesByProperties` round-trip stay in the `Rasm.Bim` projector. `DetailSchema` pins the neutral detail-bag seam. `PropertyValue.Of` rails malformed scalar or composite values, and `CanonicalBytes` preserves every scalar discriminant so the same rendering never aliases different typed evidence.

## [01]-[INDEX]

- [02]-[PROPERTY_VALUE]: the `PropertyValue` `[Union]` typed IFC-value family, the `PropertyName` key, the `Interpolation` table-curve rule, the `TemporalValue` NodaTime-carried temporal leaf family, the fallible `Of` structural admission, the recursive `Remap` node-id rewrite and its `References` reachability dual the `Relations/relation#EDGE_ALGEBRA` `Generic` edge composes (renumber and cascade in lockstep), and the canonical `Render`/`CanonicalBytes` folds.
- [03]-[PROPERTY_BAG]: the one `ValueBag<V>` named inheritance-stamped value bag (the `PropertyBag`/`QuantityBag` aliases), the `InheritanceMode` `[SmartEnum]` owning the generic `Resolve<V>` precedence algebra, the `PropertySource` rank, and the type→occurrence `Merge` the `Bake` applies.
- [04]-[DETAIL_SCHEMA]: the one neutral `DetailSchema` over `PropertyBag` — the neutral `SetName`s the `Rasm.Bim` egress maps to IFC Psets, the stamped precedence, the `JointType` allowed-set, the canonical detail `PropertyName` vocabulary, and the conforming `Bag`/`Joint` factories.

## [02]-[PROPERTY_VALUE]

- Owner: `PropertyValue` the `[Union]` typed IFC-value family; `PropertyName` the `[ValueObject<string>]` property key; `Interpolation` the table-curve rule; `TemporalValue` the NodaTime temporal leaf family; the closed fourteen-case value vocabulary a property carries.
- Cases: `Text` (verbatim string) · `Measure` (SI-coerced `MeasureValue`) · `Boolean` (strict two-valued) · `Logical` (three-valued) · `Integer` (unbounded signed integer) · `Number` (finite IEEE-754 real) · `Binary` (byte-exact payload) · `Enumerated` (selected and allowed typed scalar members) · `Reference` (target plus optional usage) · `Bounded` (lower/upper/setpoint measures) · `List` (ordered recursive values) · `Table` (defining→defined rows plus interpolation) · `Complex` (named sub-properties) · `Temporal` (`Date`/`Moment`/`Time`/`Span`/`Stamp`). The union preserves the full `IfcValue` scalar family and the structured property forms without stringification.
- Entry: `PropertyValue.Of(value, key)` is the fallible admission a raw author crosses — railing `ElementFault.ValueRejected` on a non-finite `Number`, an empty/cross-type/inverted `Bounded`, a non-subset or composite-membered `Enumerated`, an empty `Table`, or an empty `Complex`, and recursively re-admitting nested values. `Integer` carries unbounded `BigInteger`, `Number` carries finite IEEE-754, and `Binary` carries byte-exact `Seq<byte>`; none collapse to `Text`.
- Auto: `Render` dispatches the generated total `Switch` — `Text` verbatim, `Measure` the SI magnitude plus canonical unit, `Boolean`/`Logical` `TRUE`/`FALSE`(/`UNKNOWN`), `Enumerated` the recursive selected-member join, `Reference` the target id, `Bounded` the `[lower, upper, setpoint]` interval, `List`/`Table` the recursive join, `Complex` the `usage{name=value;…}` named-bag join, `Temporal` the ISO-8601 token — one projection, never a per-case consumer branch; `CanonicalBytes` writes the case ordinal then the payload (a `Measure` quantized to tolerance, the `Logical` a presence bit plus the bool, an `Enumerated` member through its own typed `CanonicalBytes` so two members sharing one text spelling under different types hash apart, a `Temporal` its arm ordinal plus ISO token, every collection count-prefixed so the encoding is injective, the `Complex` sub-properties name-sorted `Ordinal`) so the content key is byte-stable across runtimes.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch` the `Of`/`Render`/`CanonicalBytes`/`Remap` folds dispatch, `[ValueObject<string>]`/`[SmartEnum<string>]`/`ComparerAccessors`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Map` + the `Seq.Choose`/`Seq.TraverseM`/`Map.Fold`/`Option.Match` combinators the `Of` admission composes), `Rasm` (the kernel `Op` op-key), `Projection/fault#FAULT_BAND` (`ElementFault.ValueRejected`).
- Growth: a new IFC value kind is one `PropertyValue` arm carrying its payload; a new table-curve rule is one `Interpolation` row; a recursive composite rides the existing `List`/`Table`/`Complex` arms; never a per-Pset value type, never a stringly-typed value field, and a raw `string` property key crossing a bag is the named defect.
- Boundary: `PropertyValue` is the ONE typed value owner — the migration `PropertyBinding(string SetName, string Name, string Value)`/`QuantityBinding(string, string, double, string)` stringly tuples are the deleted form, and the IFC-dataType narrowing (`IfcLengthMeasure`→`Measure`, `IfcLogical`→`Logical`, `IfcBoolean`→`Boolean`) is the `Rasm.Bim` projector's at ingest, so a `Pset_*` name or an `IfcValue` type string never crosses a seam signature; `Boolean` is strict two-valued and `Logical` three-valued (`None` = `UNKNOWN`, never silently coerced to `false`); `Enumerated` carries the SELECTED set so a multi-value property is never truncated to one value (an empty `Selected` is the unset `OPTIONAL` `EnumerationValues` state, admitted), its members TYPED `PropertyValue` scalars so an `IfcValue`-typed enumeration member (a measured tolerance class, a numeric grade) keeps its discriminant, membership compares by typed record equality, and the canonical bytes separate same-text different-type members — the `Seq<string>` member narrowing that stringified the IFC value domain is the deleted form; `Temporal` carries the `IfcDate`/`IfcDateTime`/`IfcTime`/`IfcDuration`/`IfcTimeStamp` leaves as NodaTime values (a date-valued Pset row crossing as `Text` — losing the typed read and the calendar comparison a durability/procurement filter folds on — is the deleted form), the ONE ISO-8601 `Iso()` projection serving render and hash; `Reference` carries a `NodeId` resolved through the `Graph/element#ELEMENT_GRAPH` `Nodes` index, never a raw GlobalId string; `Table` carries its `Interpolation` rule so a lookup-table consumer reads the curve semantics rather than re-inferring them; `List`/`Table`/`Complex` are the closed composite forms, so a nested property never needs a parallel container type; `PropertyValue.Of` is the ONE fallible admission gating a value into a bag (a per-arm validating factory family or an unvalidated composite crossing a bag is the deleted form), its recursion runtime-stack-bounded — hostile nesting depth is the wire admission's depth gate, the `Graph/wire#WIRE_CODEC` `CodedInputStream.CreateWithLimits` recursion bound on `PropertyValueWire` decode, never a seam re-check; the `Bounded` structural law is exactly the single-`QuantityType` guard plus the ONE present lower/upper ordering — the setpoint is a free nominal the fence's `AdmitBounded` pins, and constraining it inside the interval rejects legal IFC.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// The two named bags are GLOBAL `using` aliases of the ONE generic ValueBag<V> owner — declared package-wide so the
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

// The lookup-table curve rule a Table value carries (the neutral IfcCurveInterpolationEnum mirror) — a wire-keyed
// token only; the interpolation runs in Rasm.Compute, never on the seam.
[SmartEnum<string>]
public sealed partial class Interpolation {
 public static readonly Interpolation NotDefined = new("notdefined");
 public static readonly Interpolation Linear = new("linear");
 public static readonly Interpolation LogLinear = new("log-linear");
 public static readonly Interpolation LogLog = new("log-log");
}

// The IFC temporal leaf family (IfcDate/IfcDateTime/IfcTime/IfcDuration/IfcTimeStamp) the Temporal case wraps —
// NodaTime-carried so the value compares on the calendar, never as a string spelling. Iso() is the ONE canonical
// projection render and hash share (the NodaTime ISO patterns are invariant by construction); CaseOrdinal
// discriminates the arm in the canonical bytes so grammar overlap between arms can never alias two values.
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

 // Case ordinal then typed payload through the CONTENT_ADDRESS CanonicalWriter — count-prefixed collections keep
 // the encoding injective; Complex name-sorts Ordinal.
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

 // The recursive dual of Remap — every graph-node NodeId the value BURIES (the Reference target, recursed through the
 // List/Table/Complex composites) — so the Relations/relation#EDGE_ALGEBRA Generic edge's buried attribute references are
 // a LIVE reachability set the incidence index and the DropNode cascade sweep, symmetric with Remap rewriting them
 // (Remap renumbers, References reaches — an edge whose Members omitted a ref Remap still rewrote is the deleted
 // asymmetry that stranded a dangling attribute Reference). Scalar arms bury none; a new case breaks it at compile time.
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

 // The ONE measure→string projection the Measure arm and the Bounded bounds share (a bound and a single Measure
 // render IDENTICALLY, never a unit-stripped fork): SI magnitude + canonical unit under the INVARIANT culture (a
 // decimal-comma locale must never fork a cross-runtime render), the unit trimmed when absent (dimensionless).
 private static string RenderMeasure(MeasureValue measure) =>
  string.Create(CultureInfo.InvariantCulture, $"{measure.Si:R} {measure.CanonicalUnit}").Trim();

 // The three-place bound projection over the shared RenderMeasure body — an absent bound renders "*" (the IFC
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

 private static Fin<PropertyValue> AdmitScalar(PropertyValue value, Op key, string detail) =>
  value is Text or Measure or Boolean or Logical or Integer or Number or Binary or Temporal
   ? Of(value, key)
   : ElementFault.ValueRejected(key, detail);
}
```

## [03]-[PROPERTY_BAG]

- Owner: `ValueBag<V>` the ONE named inheritance-and-source-stamped value bag (`SetName` + `Map<PropertyName, V>` + `InheritanceMode` + `PropertySource`) — `PropertyBag` (`ValueBag<PropertyValue>`) the `Graph/element#NODE_MODEL` `Node.PropertySet` case wraps and `QuantityBag` (`ValueBag<MeasureValue>`) the `Node.QuantitySet` case wraps are its two GLOBAL `using` aliases, the value type the only varying axis so it rides a type parameter (the SHAPE_BUDGET + DERIVED_TYPES collapse); `InheritanceMode` owns the type→occurrence precedence fold; `PropertySource` owns catalogue/import/derived/user source rank.
- Entry: `ValueBag<V>.Merge(type, occurrence)` folds a type-bound bag and an occurrence bag into one by delegating to `occurrence.Inheritance.Resolve(type.Values, occurrence.Values)` — the ONE generic precedence fold the mode owns — and preserves the higher `PropertySource` rank, so the `Graph/element#ELEMENT_GRAPH` `Bake` applies inheritance once per bag; `ValueBag<V>.Empty(setName, inheritance, source)` mints an empty source-stamped bag; `With(name, value)`/`Find(name)` are the immutable add and keyed read both alias kinds share.
- Auto: `Resolve<V>` dispatches the generated total `Switch` over the LanguageExt `Map` three-argument `Fold` — `OccurrenceWins` folds type entries onto the occurrence map adding only absent keys, `TypeDrivenOverride` folds with `AddOrUpdate` (type-wins), `TypeDrivenOnly` returns the type map — one generic fold serving both bag aliases identically, the mode (not the bag) owning the precedence; the `[SmartEnum<string>]` round-trips the mode token at the wire so a persisted bag re-admits its precedence; the mode is stamped at Bim ingress, the seam never inferring it.
- Receipt: the merged `ValueBag<V>` is the typed property evidence the `Bake`-derived `Element` carries flat in its `Seq<PropertyBag>`/`Seq<QuantityBag>` fields, so a consumer reads `element.Properties.Find(b => b.SetName == set).Bind(b => b.Find(name))` as one `Option<PropertyValue>`; `Source` records whether the winning bag came from Materials catalogue data, IFC import, Bim-derived quantities, or Rhino/user override — Compute assessments and Persistence provenance stay typed nodes/events, not bag sources.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[SmartEnum<int>]`), LanguageExt.Core (`Map`/`Option`/`Seq`), Generator.Equals (`[Equatable]`/`[UnorderedEquality]` — the bag is the drillable equality owner the `StructuralMerge` descends through to `Bag.Values[name]`).
- Growth: a new bag attribute shared by all bags is one column on `ValueBag<V>` (both aliases gain it); a new value kind a bag carries is one `using` alias over `ValueBag<TNew>`; a new inheritance precedence is one `InheritanceMode` row carrying its `Resolve` arm; a new source tier is one `PropertySource` row with its rank.
- Boundary: `ValueBag<V>` is the ONE property store — a per-`Pset_*` class family, a second property model, or a hand-written `PropertyBag`-beside-`QuantityBag` pair duplicating every member (the SHAPE_BUDGET parallel-type defect) is the deleted form; the type→occurrence precedence is owned by `InheritanceMode.Resolve` and applied once in `Merge` by the stamped mode — never a per-call-site merge, a per-bag-type re-expression, or a seam inference; `InheritanceMode` is the bag-merge precedence ALONE — the named type→occurrence `Bake` inheritance over a baked element's materials, section, and classifications is a SEPARATE `Bake` dimension, never a fourth row here; the bag content is typed (`PropertyValue`/`MeasureValue`), a stringly-keyed property lookup the named defect.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class InheritanceMode {
 public static readonly InheritanceMode OccurrenceWins = new("occurrence-wins");
 public static readonly InheritanceMode TypeDrivenOverride = new("type-driven-override");
 public static readonly InheritanceMode TypeDrivenOnly = new("type-driven-only");

 // The precedence algebra OWNED by the mode — one state-threaded generic fold serving both bag aliases.
 public Map<PropertyName, V> Resolve<V>(Map<PropertyName, V> type, Map<PropertyName, V> occurrence) => Switch(
  state: (Type: type, Occurrence: occurrence),
  occurrenceWins: static s => s.Type.Fold(s.Occurrence, static (acc, k, v) => acc.ContainsKey(k) ? acc : acc.Add(k, v)),
  typeDrivenOverride: static s => s.Type.Fold(s.Occurrence, static (acc, k, v) => acc.AddOrUpdate(k, v)),
  typeDrivenOnly: static s => s.Type);
}

// The comparable int key grants the generated ordering and relational operators — no comparer accessor exists or is
// needed for int (ComparerAccessors carries only the string accessors plus Default<T>).
[SmartEnum<int>]
public sealed partial class PropertySource {
 public static readonly PropertySource Catalogue = new(10, "catalogue");
 public static readonly PropertySource Import = new(20, "import");
 public static readonly PropertySource Derived = new(30, "derived");
 public static readonly PropertySource User = new(40, "user");

 public string Token { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// The value type is the ONLY varying axis — a TYPE PARAMETER, never a parallel bag pair. [Equatable] is
// LOAD-BEARING: StructuralMerge drills a changed property to Nodes[id].Bag.Values[name] — a plain record bag is an
// opaque equality leaf forcing whole-bag replacement; Values rides [UnorderedEquality] (order-independent per-entry
// diffs); V is the ATOMIC leaf (PropertyValue/MeasureValue own value equality — [Equatable] there is ceremony).
[Equatable]
public sealed partial record ValueBag<V>(string SetName, [property: UnorderedEquality] Map<PropertyName, V> Values, InheritanceMode Inheritance, PropertySource Source) {
 public static ValueBag<V> Empty(string setName, InheritanceMode inheritance, PropertySource source) =>
  new(setName, Map<PropertyName, V>(), inheritance, source);

 public Option<V> Find(PropertyName name) => Values.Find(name);

 public ValueBag<V> With(PropertyName name, V value) =>
  this with { Values = Values.AddOrUpdate(name, value) };

 public static ValueBag<V> Merge(ValueBag<V> type, ValueBag<V> occurrence) {
  Map<PropertyName, V> inherited = occurrence.Inheritance.Resolve(type.Values, occurrence.Values);
  PropertySource source = occurrence.Source >= type.Source ? occurrence.Source : type.Source;
  return occurrence with { Values = inherited, Source = source };
 }
}
```

## [04]-[DETAIL_SCHEMA]

- Owner: `DetailSchema` the ONE neutral detail-schema mechanism over `PropertyBag` — a neutral `SetName`, an `InheritanceMode`, and an optional `JointType` allowed-set — plus the canonical detail `PropertyName` vocabulary. `DetailSchema.Realization` owns realizing fastener/rebar/connector/joint detail plus the masonry size-envelope rows; `DetailSchema.Product` owns panel board/deck/membrane product geometry plus the IGU build rows.
- Entry: `DetailSchema.Realization` the canonical realizing schema; `DetailSchema.Product` the canonical product-detail schema; `schema.Bag(source = default)` mints the empty conforming source-stamped `PropertyBag`, the omitted source deriving `PropertySource.Catalogue`; `schema.Joint(selected)` the `JointType` row VALUE as a `PropertyValue.Enumerated` over the schema's closed allowed-set.
- Auto: `Bag` pins `SetName` and `InheritanceMode` from the schema and stamps the resolved source rank, so neither author nor reader hand-spells the set-name string, re-stamps precedence, or drops source rank; `Joint(selected)` constructs the typed `PropertyValue.Enumerated` over `Text`-wrapped tokens (the selected token against the schema's closed `JointTypes` allowed-set) so the `Properties/property#PROPERTY_VALUE` `Of` admission holds.
- Receipt: the conforming `PropertyBag` lands on the seam `ElementGraph` as a `Graph/element#NODE_MODEL` `Node.PropertySet` bound by a `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge onto the realizing element, the `Bake` fold merging it into `element.Properties`; the bag mints through `NodeId.Content` over `Node.ToCanonicalBytes` (id excluded) so two structurally-identical detail bags dedup to one node, never a second `(GeometryKey, DetailKey)` hasher.
- Packages: LanguageExt.Core (`Seq`/`Map` + the `Prelude` constructors), Thinktecture.Runtime.Extensions (the `PropertyName` `Create` factory + the `InheritanceMode` statics), `Properties/quantity#MEASURE_VALUE` (the dimension-only `MeasureValue.OfSi`), and the seam `PropertyBag`/`PropertyValue`/`PropertyName`/`InheritanceMode` owners this cluster composes.
- Growth: a new realizing-detail or product row is one `static readonly PropertyName` the author writes and the reader reads by name; a new joint modality is one token on `Realization.JointTypes`; a material-property→`Pset` bag is ANOTHER `DetailSchema` instance — ONE schema mechanism over `PropertyBag`, never a parallel schema type, a per-row bag class, or a per-call-site allowed-set literal.
- Boundary: `DetailSchema` is the ONE seam-declared detail contract and the seam carries NO IFC name — the neutral `SetName` is what both the in-graph bag and the schema carry, while the IFC Pset name (`Rasm_ConnectionRealization`), the `Pset_*` roster, the bSDD resolution, the egress mapping, and the `GlobalId` assignment stay in the `Rasm.Bim` `SemanticProjector`; a cross-peer realizing invariant (a fastener diameter against its member, a weld throat against its leg) is a `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`, never an IFC column on this bag; the realizing element's MATERIAL binding (grade, capacity, embodied carbon, classification, appearance) rides the `Rasm.Materials` projector's `Associate` edge, never a `SteelGrade`/`EmbodiedCarbon` row here; the joint TOPOLOGY rides the `Connect` edge's `Connect.Realizing` `Option<NodeId>` field (realizing-ness is the field, never a `ConnectKind.Realizing` row), never a detail row; an authored bag carrying a subset of rows is a faithfully different node, never a forced byte-match; the dimensional rows carry the dimension-only `MeasureValue.OfSi(Dimension, double)` (SI-base, `QuantityType` dimension-anonymous) so a re-imported bag content-keys identically — never a unit-bearing or typed-`QuantityType` measure that forks the key; `InheritanceMode` stays the bag-merge precedence the schema stamps ([03] owns the disjunction from the `Bake` inheritance).

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// The ONE seam-declared NEUTRAL detail schema over PropertyBag — authored by the Materials Component projection,
// round-tripped by the Bim Semantics/connection reader; the Bim SemanticProjector maps SetName to the IFC Pset at
// Emit, never the seam.
public sealed record DetailSchema(string SetName, InheritanceMode Inheritance, Seq<string> JointTypes) {
 // The canonical NEUTRAL row-name vocabulary — the closed row-name set both author and reader key on. PropertyName
 // itself stays an OPEN key (any Pset property name admits through PROPERTY_VALUE Of); these statics are the seam
 // rows the Component projection writes and the Bim reader recovers one-hop, never a closed key vocabulary.
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
 // The MASONRY realization rows — the EN 771 work-vs-actual size envelope (tolerance class T1/T2/Tm, range class
 // R1/R2/Rm, special-shape token) plus UnitHeight the bed-plane unit height (the W×L profile carries width and
 // length, so height has no other landing surface) and CourseHeight the coursing height (unit height + bed joint)
 // the coursing tolerance and GLB tessellation read off the laid unit's bag.
 public static readonly PropertyName SizeTolerance = PropertyName.Create("SizeTolerance");
 public static readonly PropertyName SizeRange = PropertyName.Create("SizeRange");
 public static readonly PropertyName SpecialShape = PropertyName.Create("SpecialShape");
 public static readonly PropertyName UnitHeight = PropertyName.Create("UnitHeight");
 public static readonly PropertyName CourseHeight = PropertyName.Create("CourseHeight");
 // The PANEL product rows the Component panel arm authors and a sheathing generator round-trips: EdgeProfile the
 // board-edge token, PanelThickness/BoardLength the board build, FieldSpacing/EdgeSpacing the fastener station
 // pitches, RibDepth/RibPitch/DeckForm the steel-deck corrugation, MembraneSeam the membrane lap, PanelOrientation
 // the strength-axis token, and CoreClass/SpanRating/BondClass/FoamClass/FacerClass/ThermalResistance the board
 // performance envelope.
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
 public static readonly PropertyName BondClass = PropertyName.Create("BondClass");
 public static readonly PropertyName FoamClass = PropertyName.Create("FoamClass");
 public static readonly PropertyName FacerClass = PropertyName.Create("FacerClass");
 public static readonly PropertyName ThermalResistance = PropertyName.Create("ThermalResistance");
 public static readonly PropertyName DeckForm = PropertyName.Create("DeckForm");
 // The IGU product rows — the glazing build inputs the seed-time EN 673 `Ug` / EN 410 `g`/`τv` / mass-law `Rw`
 // receipts compute from: PaneBuild/CavityBuild recursive List-of-Complex rows (per-pane optics/coating, per-cavity
 // gas/width), the EN 1279-2 EdgeSeal, the SpacerType, the MuntinGrid, and the EI fire-resistance minutes.
 public static readonly PropertyName PaneBuild = PropertyName.Create("PaneBuild");
 public static readonly PropertyName CavityBuild = PropertyName.Create("CavityBuild");
 public static readonly PropertyName SpacerType = PropertyName.Create("SpacerType");
 public static readonly PropertyName EdgeSeal = PropertyName.Create("EdgeSeal");
 public static readonly PropertyName MuntinGrid = PropertyName.Create("MuntinGrid");
 public static readonly PropertyName FireResistanceEi = PropertyName.Create("FireResistanceEi");

 // Realization: OccurrenceWins — a re-imported occurrence value wins the type default; the JointType allowed-set
 // closes the realizing modalities. Product: TypeDrivenOverride — product form is type-driven, no joint set.
 public static readonly DetailSchema Realization =
  new("Realization", InheritanceMode.OccurrenceWins, Seq("Bolted", "Welded", "Bonded", "Bearing", "Cast"));

 public static readonly DetailSchema Product =
  new("Product", InheritanceMode.TypeDrivenOverride, Seq<string>());

 // SetName/InheritanceMode pinned by the schema, the omitted source deriving PropertySource.Catalogue (ONE Option
 // entry, never a sibling overload pair); dimensional rows ride the dimension-only MeasureValue.OfSi(Dimension, double).
 public PropertyBag Bag(Option<PropertySource> source = default) =>
  new(SetName, Map<PropertyName, PropertyValue>(), Inheritance, source.IfNone(PropertySource.Catalogue));

 // The JointType row VALUE over THIS schema's closed allowed-set — the schema owns Allowed (Text-typed tokens over
 // the typed Enumerated members), so an out-of-set token rails ElementFault.ValueRejected at Of.
 public Fin<PropertyValue> Joint(string selected, Op key) => PropertyValue.Of(
  new PropertyValue.Enumerated(
   Seq<PropertyValue>(new PropertyValue.Text(selected.Trim())),
   JointTypes.Map(static token => (PropertyValue)new PropertyValue.Text(token))),
  key);
}
```

## [05]-[IMPLEMENTATION_LAW]

- [IFC_VALUE_FAMILY]: the `PropertyValue` fourteen-case union preserves the full scalar select — string, measure, boolean, logical, arbitrary integer, finite real/number, binary, and temporal — plus typed enumeration/reference/bounded/list/table/complex property forms. `CanonicalBytes` writes distinct ordinals and count-prefixed payloads, so `Text("1")`, `Integer(1)`, and `Number(1.0)` never alias even when a text renderer emits the same glyphs.
- [INHERITANCE_PRECEDENCE]: the `InheritanceMode` three-row vocabulary is the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` (and `PSET_*` sibling) property-inheritance rule the migration source applied per-call-site in `Rasm.Bim`'s `PropertySet.Resolve` — the precedence DECISION relocated to a stamp the Bim ingress writes on the bag node, the merge ALGEBRA relocated onto the mode as the generic `Resolve<V>` fold the ONE `ValueBag<V>.Merge` delegates to, so the `Bake` fold applies the correct type→occurrence merge once wholly within the seam (the type bag resolved through the `Assign.TypeDefinition` edge, the occurrence bag on the element's own node, merged by the stamped mode). The IFC `PSET_PERFORMANCEDRIVEN` template type is a performance-history association rather than a merge-precedence rule, so the Bim projector resolves it to `OccurrenceWins` at ingress — the three modes being the complete merge-precedence closure, never a per-template inheritance variant.
- [REALIZING_DETAIL_SCHEMA]: the `DetailSchema` neutral detail bags split realization from product form. `DetailSchema.Realization` owns the `Rasm_ConnectionRealization` analogue for fastener/weld/adhesive/stud/cast/rebar/connector details (`JointType`/`FastenerType`/`AccessoryType`/`BarType`/`NominalDiameter`/`NominalLength`/`CrossSectionArea`/`CarriedMemberWidth`/`CarriedMemberDepth`/`EffectiveThroat`/`BondLine`/`Overlap`) plus the masonry EN 771 size-envelope rows (`SizeTolerance`/`SizeRange`/`SpecialShape`/`UnitHeight`/`CourseHeight`), with `OccurrenceWins` inheritance and the closed `JointType` allowed-set `Bolted`/`Welded`/`Bonded`/`Bearing`/`Cast`. `DetailSchema.Product` owns the panel/deck/membrane product rows (`EdgeProfile`/`PanelThickness`/`BoardLength`/`FieldSpacing`/`EdgeSpacing`/`RibDepth`/`RibPitch`/`DeckForm`/`MembraneSeam`/`PanelOrientation`/`CoreClass`/`SpanRating`/`BondClass`/`FoamClass`/`FacerClass`/`ThermalResistance`) plus the IGU build rows (`PaneBuild`/`CavityBuild`/`SpacerType`/`EdgeSeal`/`MuntinGrid`/`FireResistanceEi` — the inputs the seed-time EN 673/EN 410/mass-law receipts compute from), with `TypeDrivenOverride` inheritance and no joint allowed-set.
