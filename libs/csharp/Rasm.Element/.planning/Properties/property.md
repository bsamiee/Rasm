# [ELEMENT_PROPERTY]

The typed property vocabulary the `PropertySet`/`QuantitySet` bag nodes carry: one `PropertyValue` `[Union]` closing the IFC-value family (`Text`/`Measure`/`Boolean`/`Logical`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex`) so a property carries its data type rather than a stringly-typed value, one `PropertyName` `[ValueObject<string>]` key, the `PropertyBag`/`QuantityBag` named bags a `Graph/element#NODE_MODEL` `Node.PropertySet`/`Node.QuantitySet` case wraps, and the `InheritanceMode` `[SmartEnum<string>]` that OWNS the type→occurrence precedence fold (`Resolve`) the bag `Merge` delegates to. This collapses the migration source's stringly-typed `PropertyBinding(string, string, string)`/`QuantityBinding(string, string, double, string)` into typed bags: a measured property is a `PropertyValue.Measure` over a `Properties/quantity#MEASURE_VALUE` SI-coerced `MeasureValue`, a three-valued logical a `Logical` carrying the `UNKNOWN` state a `bool` cannot, a bounded property a `Bounded` carrying ordered lower/upper bounds plus a free nominal setpoint, a list/table the recursive `List`/`Table` arms — and the type-versus-occurrence precedence is the one `InheritanceMode.Resolve` generic fold both bag `Merge`s share and the `Bake` (`Graph/element#ELEMENT_GRAPH`) applies, never a per-call-site merge and never re-expressed per bag type. The IFC `Pset_*` roster, the bSDD template resolution, the base-quantity derivation from geometry, and the `IfcRelDefinesByProperties` round-trip STAY in `Rasm.Bim` — the seam owns the typed value shape and the inheritance algebra, the projector owning the IFC template vocabulary; the seam carries no `Pset_*` names and no geometry. The one neutral `DetailSchema` pins the realizing-detail bag's seam contract — a neutral set name, the `OccurrenceWins` precedence, the closed `JointType` allowed-set, and the canonical realizing-detail `PropertyName` vocabulary — the SINGLE schema over `PropertyBag` the `Rasm.Materials` `Component` projection authors and the `Rasm.Bim` `Semantics/connection` reader round-trips, NEUTRAL so the seam names no `Rasm_ConnectionRealization`/`Pset_*` IFC set: the IFC Pset name, the egress mapping, and the `GlobalId` assignment stay in `Rasm.Bim`'s `SemanticProjector`, and any cross-peer realizing invariant is a `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`, never an IFC column on the seam. The page composes `Properties/quantity#MEASURE_VALUE` for the measured arm, the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` for the content-byte projection, and the kernel `Rasm.Domain` `Op` op-key for the admission rail; the fallible `PropertyValue.Of` structural admission rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a malformed composite value (an empty/cross-type/inverted `Bounded`, a non-subset or empty-`Allowed` `Enumerated`, an empty `Table`, an empty `Complex`) and recurses the `List`/`Table`/`Complex` arms, while the case constructors stay the already-validated construction.

## [01]-[INDEX]

- [02]-[PROPERTY_VALUE]: the `PropertyValue` `[Union]` typed IFC-value family (`Text`/`Measure`/`Boolean`/`Logical`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex`), the `PropertyName` key, the `Interpolation` table-curve rule, the fallible `Of` structural admission, the recursive `Remap` node-id rewrite the `Relations/relation#EDGE_ALGEBRA` `Generic` edge composes, and the canonical `Render`/`CanonicalBytes` folds.
- [03]-[PROPERTY_BAG]: the one `ValueBag<V>` named inheritance-stamped value bag (the `PropertyBag`/`QuantityBag` aliases the SHAPE_BUDGET collapse yields), the `InheritanceMode` `[SmartEnum]` owning the generic `Resolve<V>` precedence algebra, and the type→occurrence `Merge` the `Bake` applies.
- [04]-[DETAIL_SCHEMA]: the one neutral `DetailSchema` over `PropertyBag` (the neutral `SetName` the `Rasm.Bim` egress maps to the IFC Pset, the `OccurrenceWins` precedence, the `JointType` allowed-set, the canonical realizing-detail `PropertyName` vocabulary, the conforming `Bag` factory, and the `Joint` enumerated-row factory) the `Rasm.Materials` `Component` projection authors and the `Rasm.Bim` `Semantics/connection` reader round-trips.

## [02]-[PROPERTY_VALUE]

- Owner: `PropertyValue` the `[Union]` typed IFC-value family; `PropertyName` the `[ValueObject<string>]` property key (trimmed, ordinal-ignore-case); `Interpolation` the `[SmartEnum<string>]` lookup-table curve rule a `Table` value carries; the closed ten-case value vocabulary a property carries.
- Cases: `Text` (a verbatim string — `IfcText`/`IfcLabel`/`IfcIdentifier`) · `Measure` (a `Properties/quantity#MEASURE_VALUE` SI-coerced `MeasureValue` — the `IfcMeasureValue`/`IfcDerivedMeasureValue` family) · `Boolean` (a `bool` — the strict two-valued `IfcBoolean`) · `Logical` (an `Option<bool>` — the three-valued `IfcLogical`, `None` the `UNKNOWN` state `IfcLogicalEnum` carries) · `Enumerated` (the SELECTED value set plus the allowed set — `IfcPropertyEnumeratedValue`, whose `EnumerationValues` is a `LIST [1:?]`) · `Reference` (a `NodeId` target plus an optional `UsageName` — `IfcPropertyReferenceValue`) · `Bounded` (lower/upper/setpoint measures — `IfcPropertyBoundedValue`) · `List` (a recursive `Seq<PropertyValue>` — `IfcPropertyListValue`) · `Table` (defining→defined value rows plus an `Interpolation` curve rule — `IfcPropertyTableValue`) · `Complex` (a `UsageName` plus a `Map<PropertyName, PropertyValue>` of named sub-properties — `IfcComplexProperty`, whose `HasProperties` is a SET [1:?] of NAMED properties the order-only `List` and the defining→defined `Table` cannot carry); a property is a `PropertyValue` case, never a stringly-typed value.
- Entry: the case constructors are the typed admissions (`new PropertyValue.Measure(measure)`, `new PropertyValue.Logical(None)`, `new PropertyValue.Bounded(lower, upper, setpoint)`); `PropertyValue.Of(value, key)` is the fallible structural admission a raw author crosses — railing `ElementFault.ValueRejected` on an empty/cross-type/inverted `Bounded`, a non-subset or empty-`Allowed` `Enumerated`, an empty `Table`, or an empty `Complex`, and recursing the `List`/`Table`/`Complex` arms so a malformed nested value never rides a well-formed composite — while the case constructors stay the already-validated construction the `Rasm.Bim` projector and the `Bake` fold use; `Render()` is the canonical string projection the `Switch` produces for a text-only consumer; `CanonicalBytes(CanonicalWriter)` writes the case discriminant and the typed payload into the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` so a property value contributes to the node content key deterministically; `Remap(Func<NodeId, NodeId>)` is the ONE recursive node-id rewrite — the `Reference` arm rewrites its `Target` (the only `NodeId` any arm carries), the `List`/`Table`/`Complex` arms recurse, every scalar arm is identity — the `Relations/relation#EDGE_ALGEBRA` `Relationship.Remap` `Generic` arm composes over each attribute value so a `Rasm.Persistence` `Version/merge#STRUCTURAL_MERGE` `Reconcile` never strands a graph reference buried in a passthrough edge's payload.
- Auto: `Render` dispatches the generated total `Switch` — `Text` is verbatim, `Measure` renders the SI magnitude plus canonical unit, `Boolean` renders `TRUE`/`FALSE`, `Logical` renders `TRUE`/`FALSE`/`UNKNOWN`, `Enumerated` joins the selected set, `Reference` the target id, `Bounded` the `[lower, upper, setpoint]` interval, `List`/`Table` the recursive join, `Complex` the `usage{name=value;…}` named-bag join — so a stringly consumer reads one projection rather than a per-case branch; `CanonicalBytes` writes the case ordinal then the payload (a `Measure` quantized to tolerance, the three-valued `Logical` as a presence bit plus the bool, every collection — `Enumerated`/`List`/`Table`/`Complex` — length-prefixed by its count so the encoding is injective, a `List`/`Table`/`Complex` recursing through the same writer, the `Complex` sub-properties name-sorted `Ordinal` so the named bag hashes byte-stably) so the content key is stable across runtimes.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch` the `Of`/`Render`/`CanonicalBytes` folds dispatch, `[ValueObject<string>]`/`[SmartEnum<string>]`/`ComparerAccessors`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Map` + the `Seq.Choose`/`Seq.TraverseM`/`Map.Fold`/`Option.Match` combinators the `Of` admission composes — the `Complex` arm folds a `Map<PropertyName, PropertyValue>`), `Rasm` (the kernel `Op` op-key the `Of` admission rails on), `Projection/fault#FAULT_BAND` (`ElementFault.ValueRejected`).
- Growth: a new IFC value kind is one `PropertyValue` arm (a `Quantity`-list arm) carrying its payload; a three-valued truth is the `Logical` arm distinct from the strict `Boolean`; a recursive composite rides the `List`/`Table`/`Complex` arms — the ordered same-type list `List`, the defining→defined lookup `Table`, the named nested bag `Complex`; a new table-curve rule is one `Interpolation` row; never a per-Pset value type and never a stringly-typed value field; the `PropertyName` key admits once and a raw `string` property key crossing a bag is the named defect.
- Boundary: `PropertyValue` is the ONE typed value owner — the migration `PropertyBinding(string SetName, string Name, string Value)` stringly triple is the deleted form, the typed arm carrying the IFC data type so the `Bim` IDS property facet matches a typed value; the IFC-dataType narrowing (`IfcLengthMeasure` → `Measure`, `IfcLogical` → `Logical`, `IfcBoolean` → `Boolean`) is the `Rasm.Bim` projector's at ingest, the seam carrying only the typed cases so a `Pset_*` name or an `IfcText` string never crosses a seam signature; `Boolean` is the strict two-valued `IfcBoolean` and `Logical` the three-valued `IfcLogical`, the `None` carrying `UNKNOWN` so a logical property's third state is never silently coerced to `false`; `Enumerated` carries the SELECTED set (the IFC `EnumerationValues` LIST), so a multi-value enumerated property is never truncated to one value; the `Reference` arm carries a `NodeId` (a graph reference resolved through the `Graph/element#ELEMENT_GRAPH` `Nodes` index) and an optional `UsageName`, never a raw GlobalId string; `Table` carries the `Interpolation` rule so a lookup-table consumer reads the curve semantics rather than re-inferring them; the recursive `List`/`Table`/`Complex` arms are the closed composite forms — the ordered same-type list rides `List` (`IfcPropertyListValue`), the defining→defined lookup rides `Table` (`IfcPropertyTableValue`), and the NAMED nested bag rides `Complex` (`IfcComplexProperty`, whose `HasProperties` carry their own `PropertyName`s — names the order-only `List` and the lookup `Table` cannot hold) — so a nested property never needs a parallel container type; the canonical bytes count-prefix every collection so two distinct values can never forge one byte sequence; `PropertyValue.Of` is the ONE fallible admission gating a value into a bag — a per-arm validating factory family or an unvalidated composite crossing a bag is the deleted form, the structural invariants (a single-`QuantityType` `Bounded` whose present lower/upper PAIR is interval-ordered — the IFC `IfcPropertyBoundedValue.SetPointValue` carries NO WHERE rule binding it inside the bounds, so a setpoint at or beyond a bound is VALID and only the lower/upper pair is ordered; constraining the setpoint inside the interval rejects legal IFC — an `Enumerated` selected-within-a-non-empty-`Allowed`, a non-empty `Table`) enforced once at `Of` while the scalar arms admit total.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using Generator.Equals;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

// The two named bags are GLOBAL `using` aliases of the ONE generic ValueBag<V> owner (declared package-wide so the
// `Node.PropertySet`/`Node.QuantitySet` cases, the `Bake` merge, and the Rasm.Bim projector all resolve the alias
// without a per-file restatement) — `new PropertyBag(setName, map, mode)` constructs the closed generic, `PropertyBag.Merge`
// resolves the static, so a named bag names a domain concept while the value-bag owner carries Find/With/Merge once.
global using PropertyBag = Rasm.Element.ValueBag<Rasm.Element.PropertyValue>;
global using QuantityBag = Rasm.Element.ValueBag<Rasm.Element.MeasureValue>;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PropertyName {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();
}

// The lookup-table curve rule a Table value carries (the neutral IfcCurveInterpolationEnum mirror) — the interpolation a
// Rasm.Compute consumer applies between the defining→defined rows. A wire-keyed token only; the seam runs no interpolation.
[SmartEnum<string>]
public sealed partial class Interpolation {
 public static readonly Interpolation NotDefined = new("notdefined");
 public static readonly Interpolation Linear = new("linear");
 public static readonly Interpolation LogLinear = new("log-linear");
 public static readonly Interpolation LogLog = new("log-log");
}

[Union]
public abstract partial record PropertyValue {
 private PropertyValue() { }

 public sealed record Text(string Value) : PropertyValue;
 public sealed record Measure(MeasureValue Value) : PropertyValue;
 public sealed record Boolean(bool Value) : PropertyValue;
 public sealed record Logical(Option<bool> Value) : PropertyValue;
 public sealed record Enumerated(Seq<string> Selected, Seq<string> Allowed) : PropertyValue;
 public sealed record Reference(NodeId Target, Option<string> UsageName = default) : PropertyValue;
 public sealed record Bounded(Option<MeasureValue> Lower, Option<MeasureValue> Upper, Option<MeasureValue> SetPoint) : PropertyValue;
 public sealed record List(Seq<PropertyValue> Values) : PropertyValue;
 public sealed record Table(Seq<(PropertyValue Defining, PropertyValue Defined)> Rows, Interpolation Interp) : PropertyValue;
 public sealed record Complex(string UsageName, Map<PropertyName, PropertyValue> Properties) : PropertyValue;

 // The fallible structural admission a raw author crosses before a value enters a bag, dispatched through the generated
 // total Switch (NO runtime-silent _ — a new value case breaks Of at compile time, the anticipatory-collapse the C#
 // switch's catch-all swallows): total for the scalar arms (their payloads admit upstream — a Measure wraps an
 // already-SI-coerced MeasureValue, a Reference a resolved NodeId — so the arm re-admits the value as-is), structural
 // for the composite arms, and RECURSIVE through List/Table/Complex so a malformed nested value never rides a well-formed
 // composite. A composite miss rails ElementFault.ValueRejected; the case constructors stay the already-validated
 // construction the Rasm.Bim projector and the Bake fold use, Of the fallible ingress.
 public static Fin<PropertyValue> Of(PropertyValue value, Op key) => value.Switch(
  text: static p => Fin.Succ((PropertyValue)p),
  measure: static p => Fin.Succ((PropertyValue)p),
  boolean: static p => Fin.Succ((PropertyValue)p),
  logical: static p => Fin.Succ((PropertyValue)p),
  reference: static p => Fin.Succ((PropertyValue)p),
  enumerated: p => p.Allowed.IsEmpty
   ? ElementFault.ValueRejected(key, "<enumerated-allowed-empty>")
   : p.Selected.Exists(s => !p.Allowed.Contains(s))
    ? ElementFault.ValueRejected(key, "<enumerated-selected-not-allowed>")
    : Fin.Succ((PropertyValue)p),
  bounded: p => AdmitBounded(p, key),
  list: p => p.Values.TraverseM(v => Of(v, key)).As().Map(static vs => (PropertyValue)new List(vs)),
  table: p => p.Rows.IsEmpty
   ? ElementFault.ValueRejected(key, "<table-rows-empty>")
   : p.Rows.TraverseM(r => Of(r.Defining, key).Bind(d => Of(r.Defined, key).Map(x => (Defining: d, Defined: x)))).As().Map(rows => (PropertyValue)new Table(rows, p.Interp)),
  complex: p => p.Properties.IsEmpty
   ? ElementFault.ValueRejected(key, "<complex-properties-empty>")
   : p.Properties.Fold(Fin.Succ(Map<PropertyName, PropertyValue>()), (acc, k, v) => acc.Bind(m => Of(v, key).Map(x => m.AddOrUpdate(k, x)))).Map(m => (PropertyValue)new Complex(p.UsageName, m)));

 public string Render() => Switch(
  text: static p => p.Value,
  measure: static p => RenderMeasure(p.Value),
  boolean: static p => p.Value ? "TRUE" : "FALSE",
  logical: static p => p.Value.Match(Some: static b => b ? "TRUE" : "FALSE", None: static () => "UNKNOWN"),
  enumerated: static p => string.Join(',', p.Selected),
  reference: static p => p.Target.Value,
  bounded: static p => $"[{Bound(p.Lower)}, {Bound(p.Upper)}, {Bound(p.SetPoint)}]",
  list: static p => string.Join(';', p.Values.Map(static v => v.Render())),
  table: static p => string.Join(';', p.Rows.Map(static r => $"{r.Defining.Render()}={r.Defined.Render()}")),
  complex: static p => $"{p.UsageName}{{{string.Join(';', p.Properties.Select(static e => $"{e.Key.Value}={e.Value.Render()}"))}}}");

 // The canonical projection through the Projection/address#CONTENT_ADDRESS CanonicalWriter: case ordinal then the typed payload,
 // every collection count-prefixed for an injective encoding, the recursive List/Table/Complex arms recursing through
 // the same writer (a Complex's named sub-properties name-sorted Ordinal so the nested bag hashes byte-stably cross-runtime).
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  text: v => w.Ordinal(0).String(v.Value),
  measure: v => w.Ordinal(1).Measure(v.Value),
  boolean: v => w.Ordinal(2).Bool(v.Value),
  logical: v => { w.Ordinal(3).Bool(v.Value.IsSome); v.Value.IfSome(b => w.Bool(b)); return w; },
  enumerated: v => { w.Ordinal(4).Ordinal(v.Selected.Count); foreach (string s in v.Selected) { w.String(s); } w.Ordinal(v.Allowed.Count); foreach (string a in v.Allowed) { w.String(a); } return w; },
  reference: v => { w.Ordinal(5).Bool(v.UsageName.IsSome); v.UsageName.IfSome(u => w.String(u)); return w.String(v.Target.Value); },
  bounded: v => { w.Ordinal(6).Bool(v.Lower.IsSome); v.Lower.IfSome(m => w.Measure(m)); w.Bool(v.Upper.IsSome); v.Upper.IfSome(m => w.Measure(m)); w.Bool(v.SetPoint.IsSome); v.SetPoint.IfSome(m => w.Measure(m)); return w; },
  list: v => { w.Ordinal(7).Ordinal(v.Values.Count); foreach (PropertyValue inner in v.Values) { inner.CanonicalBytes(w); } return w; },
  table: v => { w.Ordinal(8).String(v.Interp.Key).Ordinal(v.Rows.Count); foreach (var (defining, defined) in v.Rows) { defining.CanonicalBytes(w); defined.CanonicalBytes(w); } return w; },
  complex: v => { w.Ordinal(9).String(v.UsageName).Ordinal(v.Properties.Count); foreach (var (n, inner) in v.Properties.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); inner.CanonicalBytes(w); } return w; });

 // The ONE recursive PropertyValue node-id rewrite the Relations/relation#EDGE_ALGEBRA Relationship.Remap Generic arm
 // composes over each attribute value (a Rasm.Persistence Version/merge#STRUCTURAL_MERGE Reconcile rewrites a re-ingested
 // graph onto durable node ids, and a Generic edge's Attributes can bury a graph Reference the reconcile MUST rewrite or
 // strand): Reference is the ONLY arm carrying a NodeId (its Target rewrites, the UsageName intact), List/Table/Complex
 // are the ONLY recursive arms (each recurses its children), and every scalar arm (Text/Measure/Boolean/Logical/
 // Enumerated/Bounded carry no NodeId) returns identity. Dispatched through the generated total Switch<PropertyValue> (NO
 // runtime-silent _ — a new value case breaks the rewrite at compile time rather than silently dropping a buried id), the
 // SAME func-form total dispatch Relationship.Remap takes; PropertyValue is a RECORD-root [Union] so the Reference arm
 // rewrites through the compiler-generated `with` (a future field on Reference rides along untouched) and the composite
 // arms reconstruct over the recursed children. Identity for an unmapped id is the caller's `map` contract (an unmapped
 // NodeId passes through), so a value carrying no Reference and no composite arm round-trips byte-identical.
 public PropertyValue Remap(Func<NodeId, NodeId> map) => Switch<PropertyValue>(
  text: static p => p,
  measure: static p => p,
  boolean: static p => p,
  logical: static p => p,
  enumerated: static p => p,
  reference: p => p with { Target = map(p.Target) },
  bounded: static p => p,
  list: p => new List(p.Values.Map(v => v.Remap(map))),
  table: p => new Table(p.Rows.Map(r => (Defining: r.Defining.Remap(map), Defined: r.Defined.Remap(map))), p.Interp),
  complex: p => new Complex(p.UsageName, p.Properties.Map((_, v) => v.Remap(map))));

 // The ONE measure→string projection both the Measure arm and the Bounded bounds compose (DERIVED_LOGIC: a bounded
 // interval's lower/upper/setpoint ARE MeasureValues and render IDENTICALLY to a single Measure — a 21 degC setpoint
 // reads "21 degC", never a unit-stripped "21" the bound projection otherwise diverges into): the SI magnitude
 // plus the canonical SI unit token under the INVARIANT culture (a cross-runtime render MUST NOT format the magnitude
 // under the server's ambient culture, or a decimal-comma locale forks the text), the unit trimmed off when absent (a
 // dimensionless Count/Scalar carries an empty CanonicalUnit). A second per-arm magnitude spelling is the deleted form.
 static string RenderMeasure(MeasureValue measure) =>
  string.Create(CultureInfo.InvariantCulture, $"{measure.Si:R} {measure.CanonicalUnit}").Trim();

 // The three-place bound projection the Bounded Render arm folds over its lower/upper/setpoint — one-hop over the shared
 // RenderMeasure body, an absent bound the "*" open end (the IFC half-open interval).
 static string Bound(Option<MeasureValue> bound) =>
  bound.Map(RenderMeasure).IfNone("*");

 // A Bounded property carries at least one of (lower, upper, setpoint), EVERY present member shares one QuantityType
 // (a measure mixing Pressure and Temperature is malformed), and a present lower/upper PAIR is ordered (lower.Si <=
 // upper.Si). The SetPoint is the IFC `IfcPropertyBoundedValue.SetPointValue` — an INDEPENDENT optional NOMINAL/default
 // value carrying NO WHERE rule binding it inside `[LowerBoundValue, UpperBoundValue]`; a control setpoint legitimately
 // sits at or beyond a bound (a frost-protection setpoint AT the lower limit, a design setpoint a comfort band brackets
 // tolerance AROUND), so constraining it inside the interval REJECTS valid IFC — the deleted over-constraint. The
 // structural law is therefore exactly the cross-type guard plus the SINGLE lower<=upper ordering (one `Inverted` read
 // over the one present pair), never three pair checks pretending the setpoint is a third interval endpoint. LanguageExt
 // v5 `Seq.Head` is `Option<A>`, so the head-type read threads through `Match` (the empty arm is the miss), never a
 // direct `.Type` on the Option.
 static Fin<PropertyValue> AdmitBounded(Bounded b, Op key) {
  Seq<MeasureValue> present = Seq(b.Lower, b.Upper, b.SetPoint).Choose(static o => o);
  return present.Head.Match(
   None: () => ElementFault.ValueRejected(key, "<bounded-empty>"),
   Some: head => present.Tail.Exists(m => m.Type != head.Type)
    ? ElementFault.ValueRejected(key, "<bounded-cross-type>")
    : Inverted(b.Lower, b.Upper)
     ? ElementFault.ValueRejected(key, "<bounded-inverted>")
     : Fin.Succ((PropertyValue)b));
 }

 // The interval ordering predicate AdmitBounded reads: a (low, high) pair is inverted when BOTH ends are present and
 // low.Si strictly exceeds high.Si (an open end — None — is never inverted, the IFC half-open interval). The ONE
 // ordering rule the present lower/upper pair composes; the setpoint is a free nominal value, never an interval endpoint.
 static bool Inverted(Option<MeasureValue> low, Option<MeasureValue> high) =>
  low.Match(Some: lo => high.Match(Some: hi => lo.Si > hi.Si, None: static () => false), None: static () => false);
}
```

## [03]-[PROPERTY_BAG]

- Owner: `ValueBag<V>` the ONE named inheritance-stamped value bag (`SetName` + `Map<PropertyName, V>` + `InheritanceMode`) — `PropertyBag` (`ValueBag<PropertyValue>`) the `Graph/element#NODE_MODEL` `Node.PropertySet` case wraps and `QuantityBag` (`ValueBag<MeasureValue>`) the `Node.QuantitySet` case wraps are its two GLOBAL `using` aliases, the value type the only axis that varies so it rides a type parameter rather than a parallel `PropertyBag`/`QuantityBag` pair sharing every field and member (the SHAPE_BUDGET + DERIVED_TYPES collapse); `InheritanceMode` the `[SmartEnum<string>]` (`OccurrenceWins`/`TypeDrivenOverride`/`TypeDrivenOnly`) that both stamps the bag's type→occurrence precedence at ingest AND OWNS the generic `Resolve<V>` fold applying it, so the `Bake` runs the IFC `QTO_*` inheritance wholly within the seam across both value kinds.
- Entry: `ValueBag<V>.Merge(type, occurrence)` (called through the `PropertyBag.Merge`/`QuantityBag.Merge` aliases) folds a type-bound bag and an occurrence bag into one by delegating to `occurrence.Inheritance.Resolve(type.Values, occurrence.Values)` — the ONE generic precedence fold the mode owns — so the `Graph/element#ELEMENT_GRAPH` `Bake` applies the inheritance once per bag rather than a per-call-site or per-bag-type merge; `InheritanceMode.Resolve<V>(type, occurrence)` is the precedence algebra (`OccurrenceWins` fills only the occurrence's gaps, `TypeDrivenOverride` lets the type overwrite, `TypeDrivenOnly` takes the type map); `ValueBag<V>.With(name, value)`/`Find(name)` are the immutable add and the keyed read both alias kinds share.
- Auto: `Resolve<V>` reads the mode and dispatches the generated total `Switch` over the LanguageExt `Map` three-argument `Fold` (`(acc, key, value)`) — the `OccurrenceWins` arm folds the type entries onto the occurrence map adding only absent keys (`ContainsKey(k) ? acc : acc.Add(k, v)`, occurrence-wins), the `TypeDrivenOverride` arm folds with `AddOrUpdate(k, v)` (type-wins), the `TypeDrivenOnly` arm returns the type map — so one generic fold serves the `PropertyBag` and `QuantityBag` merges identically (one `ValueBag<V>.Merge` body over both value kinds) and the mode, not the bag, owns the precedence; the `[SmartEnum<string>]` round-trips the mode token at the wire so a persisted bag re-admits its precedence; the mode is stamped at Bim ingress, the seam never inferring it.
- Receipt: the merged `ValueBag<V>` is the typed property evidence the `Bake`-derived `Element` carries flat in its `Seq<PropertyBag>`/`Seq<QuantityBag>` fields, so a consumer reads `element.Properties.Find(b => b.SetName == set).Bind(b => b.Find(name))` as one `Option<PropertyValue>` and `element.Quantities` the `Option<MeasureValue>` counterpart; the same-name quantity aggregate across an element set folds through `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum`, never a manual `double` accumulation.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Map`/`Option`/`Seq`), Generator.Equals (`[Equatable]`/`[UnorderedEquality]` so the `ValueBag<V>` is the drillable equality leaf the `Graph/element#ELEMENT_GRAPH` `StructuralMerge` descends through to `Bag.Values[name]`).
- Growth: a new bag attribute shared by all bags is one column on `ValueBag<V>` (both aliases gain it); a new value kind a bag carries is one `using` alias over `ValueBag<TNew>` (never a hand-written parallel bag type); a new inheritance precedence is one `InheritanceMode` row carrying its `Resolve` arm; never a per-Pset bag type and never a per-call-site precedence branch.
- Boundary: `ValueBag<V>` is the ONE property store — a per-`Pset_*` `WallProperties`/`SlabProperties` class family, a second property model, OR a hand-written `PropertyBag`-beside-`QuantityBag` pair duplicating every field and member (the SHAPE_BUDGET parallel-type defect the generic owner closes) is the deleted form, the `PropertyName` keying the one bag; the type→occurrence precedence is the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` inheritance owned by `InheritanceMode.Resolve` and applied once in `Merge` by the stamped mode, never a per-call-site merge, a per-bag-type re-expression, or an inference the seam performs; `InheritanceMode` is the `PropertyBag`/`QuantityBag` merge precedence ALONE — the named type→occurrence `Bake` inheritance the `Graph/element#ELEMENT_GRAPH` `Bake` applies to a baked element's materials, section, and classifications is a SEPARATE `Bake` dimension, never a fourth `InheritanceMode` row; the bag content is typed (`PropertyValue`/`MeasureValue`) so a stringly-keyed property lookup is the named defect; the `Pset_*` template roster, the bSDD resolution, and the geometry-true base-quantity derivation stay the `Rasm.Bim` `Semantics/properties` projector's — the seam carries the typed bag and the inheritance algebra, never the IFC template vocabulary.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class InheritanceMode {
 public static readonly InheritanceMode OccurrenceWins = new("occurrence-wins");
 public static readonly InheritanceMode TypeDrivenOverride = new("type-driven-override");
 public static readonly InheritanceMode TypeDrivenOnly = new("type-driven-only");

 // The type→occurrence precedence algebra OWNED by the mode, not re-expressed per bag: fold the type entries onto the
 // occurrence map by the mode's rule — occurrence-wins adds only absent keys, type-driven-override overwrites, type-only
 // discards the occurrence entries. ONE generic fold over the LanguageExt Map serves the PropertyBag and QuantityBag
 // merges identically; the three-argument Fold(state, (acc,k,v)=>...) threads the immutable accumulator.
 public Map<PropertyName, V> Resolve<V>(Map<PropertyName, V> type, Map<PropertyName, V> occurrence) => Switch(
  occurrenceWins: () => type.Fold(occurrence, static (acc, k, v) => acc.ContainsKey(k) ? acc : acc.Add(k, v)),
  typeDrivenOverride: () => type.Fold(occurrence, static (acc, k, v) => acc.AddOrUpdate(k, v)),
  typeDrivenOnly: () => type);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The ONE named inheritance-stamped value bag — ONE concept (a `SetName` + a `PropertyName`-keyed value map + the
// type→occurrence `InheritanceMode`), the value type the ONLY axis that varies, so it rides a TYPE PARAMETER, never a
// parallel `PropertyBag`/`QuantityBag` pair sharing every field and member (the [03]-[COLLAPSE_SCAN] [07]
// several-types-one-concept + [02] signatures-differ-only-by-type trigger, collapsed per SHAPE_BUDGET + DERIVED_TYPES:
// one declaration yields the family). `PropertyBag`/`QuantityBag` are the two `using` aliases the seam carries (the
// `Node.PropertySet`/`Node.QuantitySet` cases, the `Bake` merge, and the `Rasm.Bim` projector author each through the
// alias, `new PropertyBag(setName, map, mode)` constructing the closed generic), so the named bag names a domain concept
// while the one owner carries `Find`/`With`/`Merge` once. `Find`/`With` are the immutable read/add; `Merge(type,
// occurrence)` delegates to `occurrence.Inheritance.Resolve(type, occurrence)` — the ONE generic precedence fold the
// mode owns (`Resolve<V>` already generic over the value), so the `Bake` applies the IFC type→occurrence inheritance
// once per bag rather than a per-bag-type re-expression, the precedence owned in one place across BOTH value kinds.
// [Equatable] (NOT the record-default equality alone) is LOAD-BEARING: the Graph/element#NODE_MODEL Node.PropertySet/
// QuantitySet cases are [Equatable] and the Rasm.Persistence 3-way StructuralMerge drills a changed property through
// `Nodes[id].Bag.Values[name]` — Generator.Equals descends into a nested member ONLY when that member is itself
// [Equatable], so a plain record bag is an opaque equality leaf the drill stops at (`Nodes[id].Bag`, whole-bag
// replacement, the deleted form element.md's [STRUCTURAL_EQUALITY] forbids). The `Values` Map rides [UnorderedEquality]
// (it implements IEnumerable<(PropertyName, V)>, so a key-set delta nests as a per-entry Added/Removed/Key diff,
// order-independent as a Pset bag requires) and the element comparison composes PropertyName's value equality with V's
// own value equality — the bag is the [Equatable] drill OWNER and V is the ATOMIC value-equality LEAF the
// [UnorderedEquality] comparer composes through Generator.Equals' DefaultEqualityComparer: a PropertyValue (the
// PropertyBag element) is a RECORD-root [Union] carrying Thinktecture record-generated value equality, a MeasureValue
// (the QuantityBag element, Properties/quantity#MEASURE_VALUE) a native-equality readonly record struct — each correct
// by-value WITHOUT [Equatable] (adding it redundantly re-derives the same field compare), so the drill bottoms at
// Nodes[id].Bag.Values[name] (the whole value, a Pset entry the merge replaces wholesale), the leaf the merge keys on.
[Equatable]
public sealed partial record ValueBag<V>(string SetName, [property: UnorderedEquality] Map<PropertyName, V> Values, InheritanceMode Inheritance) {
 public Option<V> Find(PropertyName name) => Values.Find(name);

 public ValueBag<V> With(PropertyName name, V value) =>
  this with { Values = Values.AddOrUpdate(name, value) };

 public static ValueBag<V> Merge(ValueBag<V> type, ValueBag<V> occurrence) =>
  occurrence with { Values = occurrence.Inheritance.Resolve(type.Values, occurrence.Values) };
}
```

## [04]-[DETAIL_SCHEMA]

- Owner: `DetailSchema` the ONE neutral realizing-detail schema over `PropertyBag` — a neutral `SetName`, the `OccurrenceWins` `InheritanceMode`, and the `JointType` allowed-set — plus the canonical realizing-detail `PropertyName` vocabulary (the eighteen row-name statics: the twelve discrete-part diameter/throat/lap rows plus the six panel board-realization rows). `DetailSchema.Realization` is the canonical realizing-detail instance both the `Rasm.Materials` `Component` projection authors and the `Rasm.Bim` `Semantics/connection` reader round-trips, so an authored realizing element and a re-imported IFC one are one `Node.PropertySet` family on the one graph.
- Entry: `DetailSchema.Realization` the canonical schema; `schema.Bag()` mints the empty conforming `PropertyBag` (the `SetName` + `OccurrenceWins` pinned, the author filling rows through `ValueBag<V>.With`); `schema.Joint(selected)` the `JointType` row VALUE as a `PropertyValue.Enumerated` over the schema's closed allowed-set (`Bolted`/`Welded`/`Bonded`/`Bearing`/`Cast`/`Fastened`); the eighteen `static readonly PropertyName` row names (`DetailSchema.JointType`/`NominalDiameter`/`EffectiveThroat`/… plus the panel `EdgeProfile`/`PanelThickness`/`FieldSpacing`/`EdgeSpacing`/`RibDepth`/`RibPitch`) the keys both author and reader compose; a dimensional row rides a `PropertyValue.Measure` over a dimension-only `Properties/quantity#MEASURE_VALUE` `MeasureValue.OfSi(Dimension, double)`, a modality row the `Joint` enumerated value or a `PropertyValue.Text` token.
- Auto: `Bag()` constructs `new PropertyBag(SetName, Map<PropertyName, PropertyValue>(), Inheritance)` so neither author nor reader hand-spells the set-name string or re-stamps the precedence; `Joint(selected)` constructs `new PropertyValue.Enumerated(Seq(selected), JointTypes)` so the `JointType` row carries the schema's closed allowed-set and the `Properties/property#PROPERTY_VALUE` `Of` admission (a `Selected` subset of a non-empty `Allowed`) holds, a token outside `JointTypes` railing `ElementFault.ValueRejected` at `Of`; each row-name static admits once through `PropertyName.Create` so a row key is a value-object, never a raw `string`.
- Receipt: the conforming `PropertyBag` lands on the seam `ElementGraph` as a `Graph/element#NODE_MODEL` `Node.PropertySet` bound by a `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge onto the realizing element, the `Graph/element#ELEMENT_GRAPH` `Bake` fold merging it into `element.Properties` so a consumer reads `element.Properties.Find(b => b.SetName == DetailSchema.Realization.SetName).Bind(b => b.Find(DetailSchema.NominalDiameter))` for the realizing detail; the bag mints through `Graph/element#NODE_MODEL` `NodeId.Content` over `Node.ToCanonicalBytes` (id excluded) so two structurally-identical detail bags dedup to one node, never a second `(GeometryKey, DetailKey)` hasher.
- Packages: LanguageExt.Core (`Seq`/`Map` + the `Seq`/`Map<,>()` `Prelude` constructors), Thinktecture.Runtime.Extensions (the `PropertyName` `[ValueObject<string>]` `Create` factory + the `InheritanceMode` `[SmartEnum<string>]` `OccurrenceWins` static), `Properties/quantity#MEASURE_VALUE` (the dimension-only `MeasureValue.OfSi`), and the seam `PropertyBag`/`PropertyValue`/`PropertyName`/`InheritanceMode` owners this cluster composes.
- Growth: a new realizing-detail row is one `static readonly PropertyName` the author writes and the reader reads by name; a new joint modality is one token on `Realization.JointTypes`; a material-property→`Pset` bag is ANOTHER `DetailSchema` instance (its own neutral `SetName` + `OccurrenceWins`, `JointTypes` empty) the `Rasm.Materials` property projection authors — ONE schema mechanism over `PropertyBag`, never a parallel schema type, a per-row bag class, or a per-call-site allowed-set literal.
- Boundary: `DetailSchema` is the ONE seam-declared realizing-detail contract and the seam carries NO IFC name — the neutral `SetName` is what both the in-graph bag and the schema carry, and the IFC Pset name (`Rasm_ConnectionRealization`), the `Pset_*` roster, the bSDD resolution, the egress mapping, and the `GlobalId` assignment stay in the `Rasm.Bim` `SemanticProjector` (a `Pset_*`/`Rasm_ConnectionRealization` literal on the seam is the deleted form); a cross-peer realizing invariant (a fastener diameter against its member, a weld throat against its leg) is a `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`, never an IFC column on this bag; the realizing element's MATERIAL binding (grade, capacity, embodied carbon, classification, appearance) rides the `Rasm.Materials` projector's `Relations/relation#EDGE_ALGEBRA` `Associate` edge onto the element `NodeId`, never a `SteelGrade`/`EmbodiedCarbon` row on this bag (the named seam violation); the joint TOPOLOGY (which members a weld/stud realizes) is a `Relations/relation#EDGE_ALGEBRA` `Connect` edge carrying its realizing element on the `Connect.Realizing` `Option<NodeId>` field (over the `Path`/`Port` medium the `ConnectKind` closes at — realizing-ness is the field, never a `ConnectKind.Realizing` row the edge algebra rejects), never a detail row; an authored bag carrying a subset of rows is a faithfully different node, never a forced byte-match; the dimensional rows carry the dimension-only `MeasureValue.OfSi(Dimension, double)` (SI-base, the `QuantityType` dimension-anonymous) so a re-imported bag content-keys identically, never a unit-bearing or typed-`QuantityType` measure that forks the key; `InheritanceMode` stays the bag-merge precedence the schema stamps, never the named `Bake` type→occurrence inheritance over materials/section/classifications the `Graph/element#ELEMENT_GRAPH` `Bake` owns as a separate dimension.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// The ONE seam-declared NEUTRAL realizing-detail schema over PropertyBag — the SINGLE contract both the Rasm.Materials
// Component projection (author) and the Rasm.Bim Semantics/connection reader (egress) target, so an authored realizing
// element and a re-imported IFC one are one content-keyed Node.PropertySet on the one graph. NEUTRAL: the seam names no
// Rasm_ConnectionRealization/Pset_* IFC set — the neutral SetName is what the in-graph bag carries, and the Rasm.Bim
// SemanticProjector maps it to the IFC Pset name at Emit (the IFC name + the egress mapping + the GlobalId assignment
// Bim-only). The realizing-detail bag is THIS schema's Bag; a material-property->Pset bag is ANOTHER DetailSchema
// instance (its own neutral SetName + OccurrenceWins, JointTypes empty) the Materials property projection authors — ONE
// schema mechanism over PropertyBag, never a parallel schema type. InheritanceMode stays the bag-merge precedence
// (OccurrenceWins here so a re-imported occurrence value wins the type default); the named type->occurrence Bake
// inheritance over a baked element's materials/section/classifications is the Graph/element#ELEMENT_GRAPH Bake's
// SEPARATE dimension, never a fourth InheritanceMode row.
public sealed record DetailSchema(string SetName, InheritanceMode Inheritance, Seq<string> JointTypes) {
 // The canonical NEUTRAL realizing-detail row-name vocabulary — the closed row-name set both author and reader key on,
 // each a well-known PropertyName the realizing-detail bag carries. PropertyName itself stays an OPEN key (any Pset
 // property name admits through PROPERTY_VALUE Of); these statics are the realizing-detail rows the Component projection
 // writes and the Bim Semantics/connection reader recovers one-hop, never a closed vocabulary the PropertyName key forces.
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
 // The PANEL board-realization rows the Rasm.Materials Component panel arm authors and a sheathing generator round-trips
 // (the board layup + fastening + steel-deck rib a laid board carries, distinct from the discrete-part diameter/throat
 // rows above): EdgeProfile the board-edge token, PanelThickness the board build, FieldSpacing/EdgeSpacing the fastener
 // station pitches, RibDepth/RibPitch the steel-deck corrugation. Neutral names — the IFC Pset mapping stays Rasm.Bim's.
 public static readonly PropertyName EdgeProfile = PropertyName.Create("EdgeProfile");
 public static readonly PropertyName PanelThickness = PropertyName.Create("PanelThickness");
 public static readonly PropertyName FieldSpacing = PropertyName.Create("FieldSpacing");
 public static readonly PropertyName EdgeSpacing = PropertyName.Create("EdgeSpacing");
 public static readonly PropertyName RibDepth = PropertyName.Create("RibDepth");
 public static readonly PropertyName RibPitch = PropertyName.Create("RibPitch");

 // The ONE realizing-detail schema instance — the neutral SetName, the OccurrenceWins precedence, and the JointType
 // allowed-set (the six neutral modalities a JointType row's Enumerated Allowed set carries, the Bim egress facet
 // validates against). Bolted/Welded/Bonded/Bearing/Cast close the discrete-realization modalities a fastener/weld/
 // adhesive/stud/cast realizing element selects; Fastened is the screwed/nailed-board modality the Rasm.Materials panel
 // arm's Joint("Fastened") keys on (a laid sheathing board is fastened, not bolted/welded). The Component projection
 // authors THIS schema's Bag and the Bim reader round-trips it.
 public static readonly DetailSchema Realization =
  new("Realization", InheritanceMode.OccurrenceWins, Seq("Bolted", "Welded", "Bonded", "Bearing", "Cast", "Fastened"));

 // The empty conforming PropertyBag the author fills through ValueBag<V>.With(name, value) and the reader reads through
 // Find(name) — the SetName and the OccurrenceWins InheritanceMode pinned by the schema so neither side hand-spells a
 // set-name string or re-stamps the precedence (the deleted drift form). A dimensional row the author adds rides a
 // PropertyValue.Measure over a dimension-only MeasureValue.OfSi(Dimension, double); a modality row the Joint value or a token.
 public PropertyBag Bag() => new PropertyBag(SetName, Map<PropertyName, PropertyValue>(), Inheritance);

 // The JointType row VALUE as a PropertyValue.Enumerated over THIS schema's closed JointType allowed-set — the schema owns
 // the Allowed set so the author never hand-spells the six modalities and the PROPERTY_VALUE Of admission (Selected a
 // subset of a non-empty Allowed) holds; a selected token outside JointTypes rails ElementFault.ValueRejected at Of.
 public PropertyValue Joint(string selected) => new PropertyValue.Enumerated(Seq(selected), JointTypes);
}
```

## [05]-[RESEARCH]

- [IFC_VALUE_FAMILY]: the `PropertyValue` ten-case union mirrors the IFC `IfcSimpleProperty` hierarchy in full (the nine simple-property cases) plus the `IfcComplexProperty` aggregate — `IfcPropertySingleValue` (the typed scalar over the `IfcValue` select: `Text` for `IfcText`/`IfcLabel`/`IfcIdentifier`, `Measure` for `IfcMeasureValue`/`IfcDerivedMeasureValue`, `Boolean` for the two-valued `IfcBoolean`, and `Logical` for the three-valued `IfcLogical` whose `IfcLogicalEnum` carries `TRUE`/`FALSE`/`UNKNOWN`), `IfcPropertyEnumeratedValue` (`Enumerated`, whose `EnumerationValues` LIST cardinality is the `Seq<string> Selected` multi-select), `IfcPropertyReferenceValue` (`Reference` with its `UsageName`), `IfcPropertyBoundedValue` (`Bounded`), `IfcPropertyListValue` (`List`), and `IfcPropertyTableValue` (`Table` carrying the `IfcCurveInterpolationEnum` rule as the neutral `Interpolation`), with `IfcComplexProperty` (the named nested set whose `HasProperties` keep their own `PropertyName`s) riding the recursive `Complex` arm — distinct from the order-only `List` and the defining→defined `Table`, neither of which carries per-sub-property names — so the seam carries the full value family the `Rasm.Bim` projector narrows from `IfcValue`/`IfcMeasureValue` at ingest without losing the three-valued logical, the multi-value enumeration, the table-curve semantics, or the named nested sub-properties; the seam never names a `Pset_*` template or an `IfcValue` data-type string, the typed arm being the only crossing, and the canonical bytes count-prefix every collection so the content key is injective across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed.
- [INHERITANCE_PRECEDENCE]: the `InheritanceMode` three-row vocabulary is the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` (and the `PSET_*` siblings) property-inheritance rule the migration source applied per-call-site in `Rasm.Bim`'s `PropertySet.Resolve`; the precedence DECISION is relocated to a stamp the Bim ingress writes on the bag node AND the merge ALGEBRA is relocated onto the mode itself as the generic `InheritanceMode.Resolve<V>` fold the ONE `ValueBag<V>.Merge` (both bag aliases) delegates to — so the `Bake` fold applies the correct type→occurrence merge once wholly within the seam (the type bag resolved through the `Assign.TypeDefinition` edge `Relations/relation#EDGE_ALGEBRA`, the occurrence bag on the element's own `PropertySet` node, the two merged by the stamped mode), the precedence owned in one place rather than re-expressed per value kind; the IFC `PSET_PERFORMANCEDRIVEN` template type is a performance-history association rather than a merge-precedence rule, so the Bim projector resolves it to `OccurrenceWins` at ingress — the seam's three modes being the complete merge-precedence closure, never a per-template inheritance variant.
- [REALIZING_DETAIL_SCHEMA]: the `DetailSchema` neutral realizing-detail bag unifies the migration source's two parallel shapes — the `Rasm_ConnectionRealization` `PropertySet` bag (the bolt diameter / weld throat / reinforcing cover the `Rasm.Bim` `Semantics/connection` reader lowers from the GeometryGym realizing surface `IfcMechanicalFastener`/`IfcFastener`/`IfcDiscreteAccessory`/`IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcBearing`) and the material-property→`Pset_*` column shape — into ONE seam-declared schema over `PropertyBag` both the `Rasm.Materials` `Component` projection (author) and the `Rasm.Bim` reader (egress) target, so an authored realizing element (a bolt, a weld, a cast bar) and a re-imported IFC one are one content-keyed `Node.PropertySet` on the one graph. The seam carries the NEUTRAL contract only — the neutral `SetName`, the `OccurrenceWins` precedence, the closed `JointType` allowed-set (`Bolted`/`Welded`/`Bonded`/`Bearing`/`Cast`/`Fastened`, the six modalities the Bim egress facet validates against — `Fastened` the screwed/nailed-board modality the `Rasm.Materials` panel arm keys on, distinct from the five discrete-realization modalities), and the realizing-detail `PropertyName` vocabulary (`JointType`/`FastenerType`/`AccessoryType`/`BarType`/`NominalDiameter`/`NominalLength`/`CrossSectionArea`/`CarriedMemberWidth`/`CarriedMemberDepth`/`EffectiveThroat`/`BondLine`/`Overlap` plus the panel board-realization rows `EdgeProfile`/`PanelThickness`/`FieldSpacing`/`EdgeSpacing`/`RibDepth`/`RibPitch`) — the IFC Pset name (`Rasm_ConnectionRealization`), the `Pset_*` roster, the bSDD resolution, the egress mapping, and the `GlobalId` assignment staying in the `Rasm.Bim` `SemanticProjector`, and any cross-peer realizing invariant a `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint` rather than an IFC column on the bag; the dimensional rows ride the dimension-only `Properties/quantity#MEASURE_VALUE` `MeasureValue.OfSi(Dimension, double)` (SI-base, `QuantityType` dimension-anonymous) so an authored and a re-imported `NominalDiameter` content-key byte-identically, and the bag mints through `Graph/element#NODE_MODEL` `NodeId.Content` over `Node.ToCanonicalBytes` so two structurally-identical detail bags dedup to one node — an authored bag carrying a subset of rows a faithfully different node, never a forced byte-match.
