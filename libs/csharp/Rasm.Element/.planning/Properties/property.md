# [ELEMENT_PROPERTY]

The typed property vocabulary the `PropertySet`/`QuantitySet` bag nodes carry: one `PropertyValue` `[Union]` closing the IFC-value family (`Text`/`Measure`/`Boolean`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`) so a property carries its data type rather than a stringly-typed value, one `PropertyName` `[ValueObject<string>]` key, the `PropertyBag`/`QuantityBag` named bags an `Graph/element#NODE_MODEL` `Node.PropertySet`/`Node.QuantitySet` case wraps, and the `InheritanceMode` `[SmartEnum<string>]` the type→occurrence merge fold dispatches on. This collapses the migration source's stringly-typed `PropertyBinding(string, string, string)`/`QuantityBinding(string, string, double, string)` into typed bags: a measured property is a `PropertyValue.Measure` over a `Properties/quantity#MEASURE_VALUE` SI-coerced `MeasureValue`, a bounded property a `Bounded` carrying lower/upper/setpoint measures, a list/table the recursive `List`/`Table` arms — and the type-versus-occurrence precedence is the one `InheritanceMode`-driven `Merge` fold the `Bake` (`Graph/element#ELEMENT_GRAPH`) applies, never a per-call-site merge. The IFC `Pset_*` roster, the bSDD template resolution, the base-quantity derivation from geometry, and the `IfcRelDefinesByProperties` round-trip STAY in `Rasm.Bim` — the seam owns the typed value shape and the inheritance algebra, the projector owning the IFC template vocabulary; the seam carries no `Pset_*` names and no geometry. The page composes `Properties/quantity#MEASURE_VALUE` for the measured arm; a malformed value rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [01]-[PROPERTY_VALUE]: the `PropertyValue` `[Union]` typed IFC-value family (`Text`/`Measure`/`Boolean`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`), the `PropertyName` key, and the canonical render fold.
- [02]-[PROPERTY_BAG]: the `PropertyBag`/`QuantityBag` named bags, the `InheritanceMode` `[SmartEnum]`, and the type→occurrence `Merge` precedence fold the `Bake` applies.

## [02]-[PROPERTY_VALUE]

- Owner: `PropertyValue` the `[Union]` typed IFC-value family; `PropertyName` the `[ValueObject<string>]` property key (trimmed, ordinal-ignore-case); the closed eight-case value vocabulary a property carries.
- Cases: `Text` (a verbatim string) · `Measure` (a `Properties/quantity#MEASURE_VALUE` SI-coerced `MeasureValue` — the `IfcMeasureValue` family) · `Boolean` (a `bool` — `IfcBoolean`/`IfcLogical`) · `Enumerated` (a chosen value plus its allowed set — `IfcPropertyEnumeratedValue`) · `Reference` (a `NodeId` pointing to another node — `IfcPropertyReferenceValue`) · `Bounded` (lower/upper/setpoint measures — `IfcPropertyBoundedValue`) · `List` (a recursive `Seq<PropertyValue>` — `IfcPropertyListValue`) · `Table` (defining→defined value rows — `IfcPropertyTableValue`); a property is a `PropertyValue` case, never a stringly-typed value.
- Entry: the case constructors are the typed admissions (`new PropertyValue.Measure(measure)`, `new PropertyValue.Bounded(lower, upper, setpoint)`); `Render()` is the canonical string projection the `Switch` produces for a text-only consumer; `CanonicalBytes(writer, tolerance)` writes the case discriminant and the typed payload into the `Projection/address#CANONICAL_WRITER` so a property value contributes to the node content key deterministically.
- Auto: `Render` dispatches the generated total `Switch` — `Text` is verbatim, `Measure` renders the SI magnitude, `Boolean` renders `TRUE`/`FALSE`, `Enumerated` the chosen value, `Reference` the target id, `Bounded` the `[lower, upper]` interval, `List`/`Table` the recursive join — so a stringly consumer reads one projection rather than a per-case branch; `CanonicalBytes` writes the case ordinal then the payload (a `Measure` quantized to tolerance, a `List`/`Table` recursing through the same writer) so the content key is stable across runtimes.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<string>]`), LanguageExt.Core (`Seq`/`Option`), `Rasm` (the kernel `Op` op-key).
- Growth: a new IFC value kind is one `PropertyValue` arm (a `Quantity`-list, a complex property) carrying its payload; a recursive composite rides the `List`/`Table` arms; never a per-Pset value type and never a stringly-typed value field; the `PropertyName` key admits once and a raw `string` property key crossing a bag is the named defect.
- Boundary: `PropertyValue` is the ONE typed value owner — the migration `PropertyBinding(string SetName, string Name, string Value)` stringly triple is the deleted form, the typed arm carrying the IFC data type so the `Bim` IDS property facet matches a typed value; the IFC-dataType narrowing (`IfcLengthMeasure` → `Measure`, `IfcBoolean` → `Boolean`) is the `Rasm.Bim` projector's at ingest, the seam carrying only the typed cases so a `Pset_*` name or an `IfcText` string never crosses a seam signature; the `Reference` arm carries a `NodeId` (a graph reference resolved through the `Graph/element#ELEMENT_GRAPH` `Nodes` index), never a raw GlobalId string; the recursive `List`/`Table` arms are the closed composite forms (the IFC `IfcComplexProperty` decomposes into these), so a nested property never needs a parallel container type.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PropertyName {
 static partial void NormalizeValidate(ref string value) => value = value.Trim();
}

[Union]
public abstract partial record PropertyValue {
 private PropertyValue() { }

 public sealed record Text(string Value) : PropertyValue;
 public sealed record Measure(MeasureValue Value) : PropertyValue;
 public sealed record Boolean(bool Value) : PropertyValue;
 public sealed record Enumerated(string Value, Seq<string> Allowed) : PropertyValue;
 public sealed record Reference(NodeId Target) : PropertyValue;
 public sealed record Bounded(Option<MeasureValue> Lower, Option<MeasureValue> Upper, Option<MeasureValue> SetPoint) : PropertyValue;
 public sealed record List(Seq<PropertyValue> Values) : PropertyValue;
 public sealed record Table(Seq<(PropertyValue Defining, PropertyValue Defined)> Rows) : PropertyValue;

 public string Render() => Switch(
 text: static p => p.Value,
 measure: static p => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{p.Value.Si:R} {p.Value.CanonicalUnit}").Trim(),
 boolean: static p => p.Value ? "TRUE" : "FALSE",
 enumerated: static p => p.Value,
 reference: static p => p.Target.Value,
 bounded: static p => $"[{p.Lower.Map(static m => m.Si).Map(static s => s.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).IfNone("*")}, {p.Upper.Map(static m => m.Si).Map(static s => s.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).IfNone("*")}, {p.SetPoint.Map(static m => m.Si).Map(static s => s.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).IfNone("*")}]",
 list: static p => string.Join(';', p.Values.Map(static v => v.Render())),
 table: static p => string.Join(';', p.Rows.Map(static r => $"{r.Defining.Render()}={r.Defined.Render()}")));

 // The canonical projection through the Projection/address#CONTENT_ADDRESS writer: case ordinal then
 // the typed payload, the recursive List/Table arms recursing through the same writer.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
 text: v => w.Ordinal(0).String(v.Value),
 measure: v => w.Ordinal(1).Measure(v.Value),
 boolean: v => w.Ordinal(2).Bool(v.Value),
 enumerated: v => { w.Ordinal(3).String(v.Value); foreach (string a in v.Allowed) { w.String(a); } return w; },
 reference: v => w.Ordinal(4).String(v.Target.Value),
 bounded: v => { w.Ordinal(5).Bool(v.Lower.IsSome); v.Lower.IfSome(m => w.Measure(m)); w.Bool(v.Upper.IsSome); v.Upper.IfSome(m => w.Measure(m)); w.Bool(v.SetPoint.IsSome); v.SetPoint.IfSome(m => w.Measure(m)); return w; },
 list: v => { w.Ordinal(6).Ordinal(v.Values.Count); foreach (PropertyValue inner in v.Values) { inner.CanonicalBytes(w); } return w; },
 table: v => { w.Ordinal(7).Ordinal(v.Rows.Count); foreach (var (defining, defined) in v.Rows) { defining.CanonicalBytes(w); defined.CanonicalBytes(w); } return w; });
}
```

## [03]-[PROPERTY_BAG]

- Owner: `PropertyBag` the named property bag (`SetName` + `Map<PropertyName, PropertyValue>` + `InheritanceMode`) the `Graph/element#NODE_MODEL` `Node.PropertySet` case wraps; `QuantityBag` the named quantity bag (`SetName` + `Map<PropertyName, MeasureValue>` + `InheritanceMode`) the `Node.QuantitySet` case wraps; `InheritanceMode` the `[SmartEnum<string>]` (`OccurrenceWins`/`TypeDrivenOverride`/`TypeDrivenOnly`) stamped per bag at ingest so the `Bake` fold applies correct type→occurrence precedence wholly within the seam.
- Entry: `PropertyBag.Merge(type, occurrence)`/`QuantityBag.Merge(type, occurrence)` fold a type-bound bag and an occurrence bag into one bag applying the occurrence bag's `InheritanceMode` precedence — `OccurrenceWins` unions with occurrence-wins, `TypeDrivenOverride` unions with type-wins, `TypeDrivenOnly` takes the type bag only — so the `Graph/element#ELEMENT_GRAPH` `Bake` applies the IFC `QTO_*` inheritance once per bag rather than a per-call-site merge; `PropertyBag.With(name, value)` is the immutable add.
- Auto: `Merge` reads the occurrence bag's stamped `InheritanceMode` and dispatches the generated `Switch` — the `OccurrenceWins` arm folds the type entries then the occurrence entries (occurrence overwriting), the `TypeDrivenOverride` arm folds occurrence then type (type overwriting), the `TypeDrivenOnly` arm returns the type bag — over the LanguageExt `Map` `AddOrUpdate` so the merge is one immutable fold; the `[SmartEnum<string>]` round-trips the mode token at the wire so a persisted bag re-admits its precedence; the mode is stamped at Bim ingress, the seam never inferring it.
- Receipt: the merged `PropertyBag`/`QuantityBag` is the typed property evidence the `Bake`-derived `Element` carries flat, so a consumer reads `element.Properties.Find(name)` or `element.Quantities.Find(name)` as one `Option<PropertyValue>`/`Option<MeasureValue>` read; the same-name quantity aggregate across an element set folds through `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum`, never a manual `double` accumulation.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Map`/`Option`), `Rasm` (the kernel `Op` op-key).
- Growth: a new bag attribute shared by all bags is one column on `PropertyBag`/`QuantityBag`; a new inheritance precedence is one `InheritanceMode` row carrying its merge behavior; never a per-Pset bag type and never a per-call-site precedence branch.
- Boundary: the bags are the ONE property store — a per-`Pset_*` `WallProperties`/`SlabProperties` class family or a second property model is the deleted form, the `PropertyName` keying the one bag; the type→occurrence precedence is the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` inheritance applied once in `Merge` by the stamped `InheritanceMode`, never a per-call-site merge or an inference the seam performs; the bag content is typed (`PropertyValue`/`MeasureValue`) so a stringly-keyed property lookup is the named defect; the `Pset_*` template roster, the bSDD resolution, and the geometry-true base-quantity derivation stay the `Rasm.Bim` `Semantics/properties` projector's — the seam carries the typed bag and the inheritance algebra, never the IFC template vocabulary.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class InheritanceMode {
 public static readonly InheritanceMode OccurrenceWins = new("occurrence-wins");
 public static readonly InheritanceMode TypeDrivenOverride = new("type-driven-override");
 public static readonly InheritanceMode TypeDrivenOnly = new("type-driven-only");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PropertyBag(string SetName, Map<PropertyName, PropertyValue> Properties, InheritanceMode Inheritance) {
 public Option<PropertyValue> Find(PropertyName name) => Properties.Find(name);

 public PropertyBag With(PropertyName name, PropertyValue value) =>
 this with { Properties = Properties.AddOrUpdate(name, value) };

 public static PropertyBag Merge(PropertyBag type, PropertyBag occurrence) =>
 occurrence.Inheritance.Switch(
 occurrenceWins: () => occurrence with { Properties = type.Properties.Fold(occurrence.Properties, static (acc, kv) => acc.ContainsKey(kv.Key) ? acc : acc.Add(kv.Key, kv.Value)) },
 typeDrivenOverride: () => occurrence with { Properties = type.Properties.Fold(occurrence.Properties, static (acc, kv) => acc.AddOrUpdate(kv.Key, kv.Value)) },
 typeDrivenOnly: () => occurrence with { Properties = type.Properties });
}

public sealed record QuantityBag(string SetName, Map<PropertyName, MeasureValue> Quantities, InheritanceMode Inheritance) {
 public Option<MeasureValue> Find(PropertyName name) => Quantities.Find(name);

 public QuantityBag With(PropertyName name, MeasureValue value) =>
 this with { Quantities = Quantities.AddOrUpdate(name, value) };

 public static QuantityBag Merge(QuantityBag type, QuantityBag occurrence) =>
 occurrence.Inheritance.Switch(
 occurrenceWins: () => occurrence with { Quantities = type.Quantities.Fold(occurrence.Quantities, static (acc, kv) => acc.ContainsKey(kv.Key) ? acc : acc.Add(kv.Key, kv.Value)) },
 typeDrivenOverride: () => occurrence with { Quantities = type.Quantities.Fold(occurrence.Quantities, static (acc, kv) => acc.AddOrUpdate(kv.Key, kv.Value)) },
 typeDrivenOnly: () => occurrence with { Quantities = type.Quantities });
}
```

## [04]-[RESEARCH]

- [IFC_VALUE_FAMILY]: the `PropertyValue` eight-case union mirrors the IFC `IfcSimpleProperty` hierarchy — `IfcPropertySingleValue` (the typed scalar: `Text`/`Measure`/`Boolean`), `IfcPropertyEnumeratedValue` (`Enumerated`), `IfcPropertyReferenceValue` (`Reference`), `IfcPropertyBoundedValue` (`Bounded`), `IfcPropertyListValue` (`List`), and `IfcPropertyTableValue` (`Table`), with `IfcComplexProperty` decomposing into the recursive `List`/`Table` arms — so the seam carries the full value family the `Rasm.Bim` projector narrows from `IfcValue`/`IfcMeasureValue` at ingest; the seam never names a `Pset_*` template or an `IfcValue` data-type string, the typed arm being the only crossing.
- [INHERITANCE_PRECEDENCE]: the `InheritanceMode` three-row vocabulary is the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` property-inheritance rule the migration source applied per-call-site in `Rasm.Bim`'s `PropertySet.Resolve`; the precedence DECISION is relocated to a stamp the Bim ingress writes on the bag node, so the `Bake` fold applies the correct type→occurrence merge wholly within the seam — the type bag resolved through the `DefinesByType` `Assign` edge (`Relations/relation#EDGE_ALGEBRA`), the occurrence bag on the element's own `PropertySet` node, the two merged once by the stamped mode.
