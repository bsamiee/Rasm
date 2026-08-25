# [BIM_PROJECTION_RAISE]

`Rasm.Bim` owns the egress value raise: the exact inverse of `Projection/value#PROPERTY_LOWERING`, re-authoring every typed seam `PropertyValue` case and every `MeasureValue` back into the IFC entity that carried it. Numeric, binary, temporal, measured, logical, aggregate, bounded, table, and complex values re-author through their own GeometryGym entities rather than a `Render` string, and the property/quantity bag rebuilds its `Groups` prefix nesting whole.

Both election tables DERIVE from the ingress rosters — the typed-measure mint off `PropertyLowering.MeasureDimensions`, the physical-quantity mint off `PropertyLowering.QuantityTypes` — so the two directions read one authority and a second hand-rostered raise table is the named defect. Drops accumulate on `Projection/fidelity#FIDELITY_LEDGER` `Fidelity`; faults rail `Model/faults#FAULT_BAND` `BimFault` through their `Detail` row; the re-author leg that composes this raise is `Projection/egress#IFC_EGRESS`.

## [01]-[INDEX]

- [02]-[VALUE_RAISE]: `ValueRaise` — the bag admission, the generated total property and value dispatches, the derived measure/quantity mint tables under one two-rung election, the `BoundSlot` roster, and the group-nested quantity rebuild.

## [02]-[VALUE_RAISE]

- Owner: `ValueRaise` the egress value re-author — `Bag` the bag-node admission, `RaiseProperty` the generated total `IfcProperty` dispatch, `RaiseValue` the generated total cell dispatch, `MeasureMints`/`QuantityMints` the two ingress-derived mint tables, `BaseIdentities` the five base-dimension rows both canonical rungs read, `Elect` the one two-rung election, `RaiseQuantities`/`Nest` the group-prefix rebuild; `BoundSlot` the `[SmartEnum<string>]` naming the three IFC bound slots beside their seam reader and entity binder; `IfcDurationMapper` the `[Mapper]` crossing a NodaTime `Period` to the seven-scalar `IfcDuration`.
- Entry: `ValueRaise.Bag(target, node, authored, scale, key)` returns `Option<WriterT<FidelityLog, Fin, IfcPropertySetDefinition>>` — `None` where the node is not a bag or carries no values, the writer where it lowers; `ValueRaise.Quantity(target, name, measure, scale, key)` returns `Fin<IfcPhysicalQuantity>` because a physical quantity narrows losslessly or faults.
- Law: the two carriers answer two questions. The `Option` is the ADMISSION verdict — a node that is not a bag, or a bag with no values — and the writer is the lowering with its ledger; one carrier answering both made a node this emit never authored indistinguishable from one it authored losslessly. The empty-bag skip is load-bearing rather than defensive: the `IfcPropertySet`/`IfcElementQuantity` ctors derive their database from their FIRST member, so an empty set throws at the boundary.
- Law: election is ONE two-rung ladder — the ingested identity first, the base-dimension canonical second, the leg's own last rung last — instantiated per target mint rather than re-spelled per leg. The measure leg's last rung is a COUNTED flatten onto `IfcReal`; the quantity leg has no last rung and faults, because a bare real is still a measure while a wrong `IfcQuantityCount` claims a quantity type the source never carried.
- Law: `Dimensionless` carries NO canonical row. `Count`, `Number`, `Ratio`, and `Angle` all sign the zero vector, so a dimension key cannot separate an integral tally from a real one; the derived dimensions carry none either, their preimage not being injective (`PressureDim` answers four measure types).
- Auto: `RaiseProperty` and `RaiseValue` are both the generated TOTAL `Switch` over the one seam union, so a new case breaks BOTH at compile time and neither can fall into a string arm. Their targets differ genuinely — a property is a NAMED entity, a cell is a bare value — and the eight scalar property arms read `RaiseValue` and wrap, so every scalar mint has ONE owner; the six composite cases have no IFC value spelling at all and each DECLARES its refusal as its own arm rather than sharing a catch-all tail. `RaiseMeasure` folds SI to declared through the seam `UnitScheme.Render` before the mint, so the magnitude a declared-millimetre deliverable carries is millimetres.
- Output: the measure-flatten tail is the one bounded drop this raise incurs, RETURNED on the carrier with the measure type as its anchor, so a consuming tool reads which measures lost their `IfcValue` identity instead of discovering it in the file.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, Riok.Mapperly, LanguageExt.Core, NodaTime
- Growth: a new seam value case is one arm on each of the two generated dispatches; a new measure type re-raises from its ingress `MeasureDimensions` row with ZERO edit here; a new physical-quantity entity re-raises from its ingress `QuantityTypes` row with ZERO edit here; a new base-dimension identity is one `BaseIdentities` row serving both canonical rungs; a new IFC bound slot is one `BoundSlot` row.
- Boundary: a `Bounded`/`List`/`Complex`/`Table` property degrading to its `Render` string is the deleted lossy form — `Text` alone is the string arm; a `Measure` re-authoring as a bare `IfcReal` while its `QuantityType` names a GeometryGym `IfcValue` type or its dimension a base measure is the deleted flattening, and because both mint tables DERIVE from the ingress rosters the two directions cannot drift; a quantity bag re-authoring its `Groups` rows as dotted flat names is the deleted lossy form — the prefix carried the nesting and nothing carried the grouping identity, so a classified takeoff hierarchy re-emitted as one flat set; the three bound slots are ROWS, so the mutable log cell and the three duplicated read-raise-assign bodies that mirrored one another are both deleted — the accumulation is the carrier's own; unit DECLARATION (the `IfcUnitAssignment` the emitted database carries) is `Projection/egress#IFC_EGRESS`, this page raising magnitudes under an already-resolved regime.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Numerics;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm.Bim.Model;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoundSlot {
    public static readonly BoundSlot Lower = new("lower",
        static bounded => bounded.Lower, static (raised, bound) => { raised.LowerBoundValue = bound; return raised; });
    public static readonly BoundSlot Upper = new("upper",
        static bounded => bounded.Upper, static (raised, bound) => { raised.UpperBoundValue = bound; return raised; });
    public static readonly BoundSlot SetPoint = new("setpoint",
        static bounded => bounded.SetPoint, static (raised, bound) => { raised.SetPointValue = bound; return raised; });

    [UseDelegateFromConstructor]
    public partial Option<MeasureValue> Read(PropertyValue.Bounded bounded);

    [UseDelegateFromConstructor]
    public partial IfcPropertyBoundedValue Bind(IfcPropertyBoundedValue raised, IfcValue bound);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper]
[MapperIgnoreSource(nameof(Period.Weeks))]
[MapperIgnoreSource(nameof(Period.Milliseconds))]
[MapperIgnoreSource(nameof(Period.Ticks))]
[MapperIgnoreSource(nameof(Period.Nanoseconds))]
[MapperIgnoreSource(nameof(Period.HasDateComponent))]
[MapperIgnoreSource(nameof(Period.HasTimeComponent))]
internal static partial class IfcDurationMapper {
    [MapPropertyFromSource(nameof(IfcDuration.Days), Use = nameof(DaysOf))]
    [MapPropertyFromSource(nameof(IfcDuration.Seconds), Use = nameof(SecondsOf))]
    public static partial IfcDuration Raise(Period span);

    [UserMapping] static int DaysOf(Period span) => span.Days + checked((int)(span.Weeks * 7));

    [UserMapping] static double SecondsOf(Period span) =>
        span.Seconds + ((double)span.Nanoseconds / NodaConstants.NanosecondsPerSecond);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ValueRaise {
    static readonly FrozenDictionary<string, Func<double, IfcValue>> MeasureMints =
        PropertyLowering.MeasureDimensions.Keys.AsIterable()
            .Choose(static name => Optional(typeof(IfcValue).Assembly.GetType($"{typeof(IfcValue).Namespace}.{name}"))
                .Filter(static shape => typeof(IfcValue).IsAssignableFrom(shape))
                .Bind(static shape => Optional(shape.GetConstructor([typeof(double)])))
                .Map(ctor => (Name: name, Mint: (Func<double, IfcValue>)(si => (IfcValue)ctor.Invoke([si])))))
            .ToFrozenDictionary(static row => row.Name, static row => row.Mint, StringComparer.Ordinal);

    static readonly FrozenDictionary<QuantityType, Func<DatabaseIfc, string, double, IfcPhysicalQuantity>> QuantityMints =
        PropertyLowering.QuantityTypes.AsIterable()
            .Choose(static row => Optional(row.Key.GetConstructor([typeof(DatabaseIfc), typeof(string), typeof(double)]))
                .Map(ctor => (Quantity: row.Value,
                    Mint: (Func<DatabaseIfc, string, double, IfcPhysicalQuantity>)((db, name, si) =>
                        (IfcPhysicalQuantity)ctor.Invoke([db, name, si])))))
            .ToFrozenDictionary(static row => row.Quantity, static row => row.Mint);

    static readonly Seq<(Dimension Dim, string Measure, QuantityType Quantity)> BaseIdentities = Seq(
        (Dimension.LengthDim, "IfcLengthMeasure", QuantityType.Length),
        (Dimension.AreaDim, "IfcAreaMeasure", QuantityType.Area),
        (Dimension.VolumeDim, "IfcVolumeMeasure", QuantityType.Volume),
        (Dimension.MassDim, "IfcMassMeasure", QuantityType.Mass),
        (Dimension.DurationDim, "IfcTimeMeasure", QuantityType.Duration));

    static readonly FrozenDictionary<Dimension, string> CanonicalMeasures =
        BaseIdentities.ToFrozenDictionary(static row => row.Dim, static row => row.Measure);

    static readonly FrozenDictionary<Dimension, QuantityType> CanonicalQuantities =
        BaseIdentities.ToFrozenDictionary(static row => row.Dim, static row => row.Quantity);

    // --- [ELECTION]

    static Option<TMint> Elect<TIdentity, TMint>(TIdentity identity, Dimension dimension,
        FrozenDictionary<TIdentity, TMint> mints, FrozenDictionary<Dimension, TIdentity> canonical)
        where TIdentity : notnull =>
        Row(mints, identity) | Row(canonical, dimension).Bind(fallback => Row(mints, fallback));

    static Option<TValue> Row<TKey, TValue>(FrozenDictionary<TKey, TValue> rows, TKey key) where TKey : notnull =>
        rows.TryGetValue(key, out TValue? found) ? Optional(found) : None;

    // --- [BAG_ADMISSION]

    public static Option<WriterT<FidelityLog, Fin, IfcPropertySetDefinition>> Bag(
        DatabaseIfc target, Node node, Map<NodeId, IfcObjectDefinition> authored, UnitScheme scale, Op key) => node switch {
        Node.PropertySet ps when !ps.Bag.Values.IsEmpty => Some(ps.Bag.Values.AsIterable().ToSeq()
            .Traverse(kv => RaiseProperty(target, authored, kv.Key, kv.Value, scale, key)).As()
            .Map(properties => (IfcPropertySetDefinition)new IfcPropertySet(ps.Bag.SetName, properties))),
        Node.QuantitySet qs when !qs.Bag.Values.IsEmpty => Some(
            Fidelity.Lift(Quantities(target, qs.Bag, scale, key))
                .Map(quantities => (IfcPropertySetDefinition)new IfcElementQuantity(qs.Bag.SetName, quantities))),
        _ => Option<WriterT<FidelityLog, Fin, IfcPropertySetDefinition>>.None,
    };

    // --- [PROPERTY_RAISE]

    public static WriterT<FidelityLog, Fin, IfcProperty> RaiseProperty(
        DatabaseIfc target, Map<NodeId, IfcObjectDefinition> authored, PropertyName name, PropertyValue value, UnitScheme scale, Op key) =>
        value.Switch<(DatabaseIfc Db, Map<NodeId, IfcObjectDefinition> Authored, PropertyName Name, UnitScheme Scale, Op Key), WriterT<FidelityLog, Fin, IfcProperty>>(
            state: (Db: target, Authored: authored, Name: name, Scale: scale, Key: key),
            text:       static (s, t) => Single(s, t),
            measure:    static (s, m) => Single(s, m),
            boolean:    static (s, b) => Single(s, b),
            logical:    static (s, l) => Single(s, l),
            integer:    static (s, i) => Single(s, i),
            number:     static (s, n) => Single(s, n),
            binary:     static (s, b) => Single(s, b),
            temporal:   static (s, t) => Single(s, t),
            enumerated: static (s, e) =>
                from selected in e.Selected.Traverse(v => RaiseValue(v, s.Scale, s.Key)).As()
                from allowed in e.Allowed.Traverse(v => RaiseValue(v, s.Scale, s.Key)).As()
                select (IfcProperty)(e.Allowed.IsEmpty
                    ? new IfcPropertyEnumeratedValue(s.Db, s.Name.Value, selected)
                    : new IfcPropertyEnumeratedValue(s.Name.Value, selected, new IfcPropertyEnumeration(s.Db, s.Name.Value, allowed))),
            reference:  static (s, r) => Fidelity.Clean(Reference(s.Db, s.Authored, s.Name, r)),
            bounded:    static (s, b) => Bounded(s.Db, s.Name, b, s.Scale).Map(IfcProperty (raised) => raised),
            list:       static (s, l) => l.Values.Traverse(v => RaiseValue(v, s.Scale, s.Key)).As()
                .Map(values => (IfcProperty)new IfcPropertyListValue(s.Db, s.Name.Value, values)),
            table:      static (s, t) => Table(s.Db, s.Name, t, s.Scale, s.Key).Map(IfcProperty (raised) => raised),
            complex:    static (s, c) => c.Properties.AsIterable().ToSeq()
                .Traverse(kv => RaiseProperty(s.Db, s.Authored, kv.Key, kv.Value, s.Scale, s.Key)).As()
                .Map(members => (IfcProperty)new IfcComplexProperty(s.Db, s.Name.Value, c.UsageName, members)));

    static WriterT<FidelityLog, Fin, IfcProperty> Single(
        (DatabaseIfc Db, Map<NodeId, IfcObjectDefinition> Authored, PropertyName Name, UnitScheme Scale, Op Key) s, PropertyValue value) =>
        RaiseValue(value, s.Scale, s.Key).Map(raised => (IfcProperty)new IfcPropertySingleValue(s.Db, s.Name.Value, raised));

    static IfcPropertyReferenceValue Reference(
        DatabaseIfc db, Map<NodeId, IfcObjectDefinition> authored, PropertyName name, PropertyValue.Reference reference) {
        IfcPropertyReferenceValue raised = new(db, name.Value) { UsageName = reference.UsageName.IfNone("") };
        authored.Find(reference.Target).Iter(entity => { if (entity is IfcObjectReferenceSelect select) { raised.PropertyReference = select; } });
        return raised;
    }

    static WriterT<FidelityLog, Fin, IfcPropertyBoundedValue> Bounded(
        DatabaseIfc target, PropertyName name, PropertyValue.Bounded bounded, UnitScheme scale) =>
        BoundSlot.Items.AsIterable().ToSeq()
            .Traverse(slot => slot.Read(bounded).Match(
                Some: measure => RaiseMeasure(measure, scale).Map(bound => (Slot: slot, Bound: Some(bound))),
                None: () => Fidelity.Clean((Slot: slot, Bound: Option<IfcValue>.None)))).As()
            .Map(rows => rows.Fold(new IfcPropertyBoundedValue(target, name.Value),
                static (raised, row) => row.Bound.Match(Some: bound => row.Slot.Bind(raised, bound), None: () => raised)));

    static WriterT<FidelityLog, Fin, IfcPropertyTableValue> Table(
        DatabaseIfc target, PropertyName name, PropertyValue.Table table, UnitScheme scale, Op key) =>
        from defining in table.Rows.Traverse(r => RaiseValue(r.Defining, scale, key)).As()
        from defined in table.Rows.Traverse(r => RaiseValue(r.Defined, scale, key)).As()
        select Filled(new IfcPropertyTableValue(target, name.Value) { CurveInterpolation = Interp(table.Interp) }, defining, defined);

    static IfcPropertyTableValue Filled(IfcPropertyTableValue raised, Seq<IfcValue> defining, Seq<IfcValue> defined) {
        raised.DefiningValues.AddRange(defining);
        raised.DefinedValues.AddRange(defined);
        return raised;
    }

    // --- [VALUE_RAISE]

    static WriterT<FidelityLog, Fin, IfcValue> RaiseValue(PropertyValue value, UnitScheme scale, Op key) =>
        value.Switch<(UnitScheme Scale, Op Key), WriterT<FidelityLog, Fin, IfcValue>>(
            state: (Scale: scale, Key: key),
            text:       static (s, t) => Fidelity.Clean<IfcValue>(new IfcLabel(t.Value)),
            measure:    static (s, m) => RaiseMeasure(m.Value, s.Scale),
            boolean:    static (s, b) => Fidelity.Clean<IfcValue>(new IfcBoolean(b.Value)),
            logical:    static (s, l) => Fidelity.Clean<IfcValue>(new IfcLogical(Logical(l.Value))),
            integer:    static (s, i) => Fidelity.Clean<IfcValue>(new IfcInteger(checked((long)i.Value))),
            number:     static (s, n) => Fidelity.Clean<IfcValue>(new IfcReal(n.Value)),
            binary:     static (s, b) => Fidelity.Clean<IfcValue>(new IfcBinary(b.Value.ToArray())),
            temporal:   static (s, t) => Fidelity.Clean(Temporal(t.Value)),
            enumerated: static (s, e) => Uncellable(s.Key, nameof(PropertyValue.Enumerated)),
            reference:  static (s, r) => Uncellable(s.Key, nameof(PropertyValue.Reference)),
            bounded:    static (s, b) => Uncellable(s.Key, nameof(PropertyValue.Bounded)),
            list:       static (s, l) => Uncellable(s.Key, nameof(PropertyValue.List)),
            table:      static (s, t) => Uncellable(s.Key, nameof(PropertyValue.Table)),
            complex:    static (s, c) => Uncellable(s.Key, nameof(PropertyValue.Complex)));

    static WriterT<FidelityLog, Fin, IfcValue> Uncellable(Op key, string shape) =>
        Fidelity.Lift(Fin.Fail<IfcValue>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "value-cell-unraisable", shape }))));

    static WriterT<FidelityLog, Fin, IfcValue> RaiseMeasure(MeasureValue measure, UnitScheme scale) =>
        scale.Render(measure).Value is var declared
        && Elect(measure.Type.Value, measure.Dimension, MeasureMints, CanonicalMeasures).Case is Func<double, IfcValue> mint
            ? Fidelity.Clean(mint(declared))
            : Fidelity.Drop<IfcValue>(FidelityDrop.MeasureFlattened, measure.Type.Value, new IfcReal(declared));

    static IfcLogicalEnum Logical(Option<bool> logical) =>
        logical.Match(Some: static flag => flag ? IfcLogicalEnum.TRUE : IfcLogicalEnum.FALSE, None: static () => IfcLogicalEnum.UNKNOWN);

    static IfcCurveInterpolationEnum Interp(Interpolation interp) => interp.Switch(
        notDefined: static () => IfcCurveInterpolationEnum.NOTDEFINED,
        linear:     static () => IfcCurveInterpolationEnum.LINEAR,
        logLinear:  static () => IfcCurveInterpolationEnum.LOG_LINEAR,
        logLog:     static () => IfcCurveInterpolationEnum.LOG_LOG);

    static IfcValue Temporal(TemporalValue temporal) => temporal.Switch<IfcValue>(
        date: static value => new IfcDate(value.Value.AtMidnight().ToDateTimeUnspecified()),
        moment: static value => new IfcDateTime(value.Value.ToDateTimeUnspecified()),
        time: static value => new IfcTime { Value = value.Value.On(new LocalDate(1970, 1, 1)).ToDateTimeUnspecified() },
        span: static value => IfcDurationMapper.Raise(value.Value),
        stamp: static value => new IfcTimeStamp(checked((int)value.Value.ToUnixTimeSeconds())));

    // --- [QUANTITY_NESTING]

    static Fin<Seq<IfcPhysicalQuantity>> Quantities(DatabaseIfc target, QuantityBag bag, UnitScheme scale, Op key) =>
        bag.Values.AsIterable().ToSeq().TraverseM(kv => Member(target, bag.Groups, kv.Key, kv.Value, scale, key)).As()
            .Map(raised => Nest(bag.Groups, raised, ""));

    static Fin<(string Owner, IfcPhysicalQuantity Quantity)> Member(
        DatabaseIfc target, Map<string, GroupIdentity> groups, PropertyName name, MeasureValue measure, UnitScheme scale, Op key) =>
        from owner in Fin.Succ(OwnerOf(groups, name))
        from quantity in Quantity(target, owner.Length == 0 ? name : PropertyCategory.Seam.Row(Leaf(name.Value)), measure, scale, key)
        select (Owner: owner, Quantity: quantity);

    static string OwnerOf(Map<string, GroupIdentity> groups, PropertyName name) =>
        toSeq(toSeq(groups.Keys)
            .Filter(prefix => name.Value.StartsWith($"{prefix}.", StringComparison.Ordinal))
            .OrderByDescending(static prefix => prefix.Length))
            .Head.IfNone("");

    static Seq<IfcPhysicalQuantity> Nest(Map<string, GroupIdentity> groups, Seq<(string Owner, IfcPhysicalQuantity Quantity)> raised, string parent) =>
        raised.Filter(row => row.Owner == parent).Map(static row => row.Quantity)
        + toSeq(groups).Filter(entry => ParentOf(entry.Key) == parent).Choose(entry =>
            Nest(groups, raised, entry.Key) is { IsEmpty: false } children
                ? Some((IfcPhysicalQuantity)new IfcPhysicalComplexQuantity(Leaf(entry.Key), children, entry.Value.Discrimination.IfNone("")) {
                    Quality = entry.Value.Quality.IfNone(""),
                    Usage = entry.Value.Usage.IfNone(""),
                })
                : Option<IfcPhysicalQuantity>.None);

    static string ParentOf(string path) => path.LastIndexOf('.') is var cut && cut > 0 ? path[..cut] : "";

    static string Leaf(string path) => path.LastIndexOf('.') is var cut && cut >= 0 ? path[(cut + 1)..] : path;

    public static Fin<IfcPhysicalQuantity> Quantity(
        DatabaseIfc target, PropertyName name, MeasureValue measure, UnitScheme scale, Op key) =>
        Elect(measure.Type, measure.Dimension, QuantityMints, CanonicalQuantities)
            .Map(mint => mint(target, name.Value, scale.Render(measure).Value))
            .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "quantity-type-unmapped", name.Value, measure.Type.Value })));
}
```

## [03]-[RESEARCH]

(none)
