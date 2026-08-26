# `Domain/context.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/context.md`

Apply the moves in order. Counts are authored, nonblank C# fence lines, including section comments; generated members are excluded. The ordered result removes 28 fenced LOC, six declared members, six publicly reachable members, and three method-local bindings without adding a type, helper, enum, carrier, policy rail, or generated surface.

Authority: `CLAUDE.md`; the owning `libs/`, `libs/dotnet/`, and `libs/dotnet/Rasm/` architecture and ruling surfaces; `docs/stacks/csharp/`; both checked-in `.api` tiers, especially LanguageExt, Thinktecture, UnitsNet, RhinoCommon, and the Rasm Rhino partition; and direct `Context`, `ToleranceLane`, `Tolerance`, and `ModelUnit` consumers across `libs/dotnet/`.

## 1. Use target-typed `Fin<Tolerance>` lifts

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, anchor `public static Fin<Tolerance> Of(ToleranceLane lane, double value, Op key)`.

**From:**

```csharp
public static Fin<Tolerance> Of(ToleranceLane lane, double value, Op key) =>
    lane.Band.Admits(value: value)
        ? Fin.Succ(value: new Tolerance(Lane: lane, Value: value))
        : Fin.Fail<Tolerance>(error: new KernelFault.OutOfRange(
            Label: lane.Key,
            Scalar: value,
            Requirement: lane.Band.Refuse(label: lane.Key, value: value).Message,
            Key: Some(key)));
```

**To:**

```csharp
public static Fin<Tolerance> Of(ToleranceLane lane, double value, Op key) =>
    lane.Band.Admits(value)
        ? new Tolerance(lane, value)
        : new KernelFault.OutOfRange(
            lane.Key, value, lane.Band.Refuse(lane.Key, value).Message, Some(key));
```

**Effect:** fenced LOC `8 -> 5` (`-3`); declared members `0`; explicit carrier constructors `2 -> 0`.

**API/consumer proof:** `docs/stacks/csharp/results-and-effects.md` establishes bare success-value and `Error` lifts in a target-typed `Fin<T>` position. `KernelFault.OutOfRange` is `(Label, Scalar, Requirement, Key)` and derives from `Error`; `Band.Admits` and `Band.Refuse` remain the only range authority. The signature and result do not change, so `Context.Build`, `Context.Override`, and the `Domain/objective.md` caller remain source-identical.

**Ripples:** none.

## 2. Use the same lift in `ModelUnit.Of(UnitSystem)`

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, anchor `public static Fin<ModelUnit> Of(UnitSystem value, Op key)`.

**From:**

```csharp
public static Fin<ModelUnit> Of(UnitSystem value, Op key) => value switch {
    var unknown when !Enum.IsDefined(value: unknown) =>
        Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(Units: unknown, Requirement: "must be a defined unit system")),
    UnitSystem.Unset or UnitSystem.None =>
        Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(Units: value, Requirement: "must be a model unit system")),
    UnitSystem.CustomUnits =>
        Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(Units: value, Requirement: "must carry custom name and scale")),
    _ => key.Catch(() => Of(value: LengthUnit.FromKnownUnitSystem(knownUnitSystem: value), key: key)),
};
```

**To:**

```csharp
public static Fin<ModelUnit> Of(UnitSystem value, Op key) => value switch {
    var unknown when !Enum.IsDefined(unknown) => new KernelFault.InvalidUnitSystem(unknown, "must be a defined unit system"),
    UnitSystem.Unset or UnitSystem.None => new KernelFault.InvalidUnitSystem(value, "must be a model unit system"),
    UnitSystem.CustomUnits => new KernelFault.InvalidUnitSystem(value, "must carry custom name and scale"),
    _ => key.Catch(() => Of(LengthUnit.FromKnownUnitSystem(value), key)),
};
```

**Effect:** fenced LOC `9 -> 6` (`-3`); declared members `0`; explicit carrier constructors `3 -> 0`.

**API/consumer proof:** each switch arm is target-typed as `Fin<ModelUnit>`. The three admission predicates, requirements, fault fields, and `Op.Catch` boundary are unchanged. Both `Context.Of(UnitSystem)` paths plus the `Drawing`, `Interaction`, and Rhino-boundary callers keep the same signature and outcome.

**Ripples:** none.

## 3. Delete the unconsumed unit-wrapper surface and compose UnitsNet at `Override`

`Context.Override` is the only live dynamic value-conversion consumer. `ModelUnit.In` is its one-hop wrapper; `ModelUnit.Convert` has no other caller; `Converter<TQuantity>` has no caller anywhere in `libs/dotnet/`, despite the prose claiming a Fabrication hot path. `ModelUnit.Dimension` is the constant `UnitsNet.Length.BaseDimensions`, read only by that same ingress. None earns a module member.

### 3a. Delete the constant dimension projection

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, anchor `public UnitsNet.BaseDimensions Dimension`.

**From:**

```csharp
public UnitsNet.BaseDimensions Dimension => UnitsNet.Length.BaseDimensions;
```

**To:**

```csharp
```

### 3b. Delete the one-call `In` wrapper and its orphaned subsection marker

**Location:** same file, anchors `// --- [UNIT_BRIDGE]` and `internal Fin<double> In`.

**From:**

```csharp
// --- [UNIT_BRIDGE]
internal Fin<double> In(double value, Enum unit, Op key) =>
    Convert(value: value, from: unit, to: UnitsNet.Units.LengthUnit.Meter, key: key)
        .Map(metres => metres / MetersPerUnit);
```

**To:**

```csharp
```

### 3c. Delete the now-zero-call dynamic wrapper

**Location:** same file, anchor `public static Fin<double> Convert(double value, Enum from, Enum to, Op? key = null)`.

**From:**

```csharp
public static Fin<double> Convert(double value, Enum from, Enum to, Op? key = null) =>
    UnitsNet.UnitConverter.TryConvert(value, from, to, out double converted) && double.IsFinite(d: converted)
        ? Fin.Succ(value: converted)
        : Fin.Fail<double>(error: key.OrDefault().InvalidInput());
```

**To:**

```csharp
```

### 3d. Delete the speculative cached-delegate wrapper

**Location:** same file, anchor `public static Fin<Func<TQuantity, TQuantity>> Converter<TQuantity>`.

**From:**

```csharp
public static Fin<Func<TQuantity, TQuantity>> Converter<TQuantity>(Enum from, Enum to, Op? key = null) where TQuantity : UnitsNet.IQuantity =>
    UnitsNet.UnitConverter.Default.TryGetConversionFunction<TQuantity>(from, to, out UnitsNet.ConversionFunction? conversion)
        ? Fin.Succ<Func<TQuantity, TQuantity>>(value: quantity => (TQuantity)conversion(quantity))
        : Fin.Fail<Func<TQuantity, TQuantity>>(error: key.OrDefault().InvalidInput());
```

**To:**

```csharp
```

### 3e. Compose the admitted package directly and remove `self`

**Location:** same file, anchor `public Fin<Context> Override(ToleranceLane lane, double value, Enum unit, Op? key = null)`.

**From:**

```csharp
public Fin<Context> Override(ToleranceLane lane, double value, Enum unit, Op? key = null) {
    Op op = key.OrDefault();
    Context self = this;
    return from converted in lane.Dimension.Equals(Unit.Dimension)
               ? Unit.In(value: value, unit: unit, key: op)
               : Fin.Succ(value: value)
           from admitted in Tolerance.Of(lane: lane, value: converted, key: op)
           select self with { Overrides = self.Overrides.AddOrUpdate(key: lane, value: admitted) };
}
```

**To:**

```csharp
public Fin<Context> Override(ToleranceLane lane, double value, Enum unit, Op? key = null) {
    Op op = key.OrDefault();
    return from converted in !lane.Dimension.Equals(UnitsNet.Length.BaseDimensions)
               ? Fin.Succ(value)
               : UnitsNet.UnitConverter.TryConvert(value, unit, UnitsNet.Units.LengthUnit.Meter, out double metres)
                 && double.IsFinite(metres)
                   ? Fin.Succ(metres / Unit.MetersPerUnit) : Fin.Fail<double>(op.InvalidInput())
           from admitted in Tolerance.Of(lane, converted, op)
           select this with { Overrides = Overrides.AddOrUpdate(lane, admitted) };
}
```

**Effect:** combined fenced LOC `22 -> 10` (`-12`); declared members `-4` (`Dimension`, `In`, `Convert`, `Converter`); public members `-3`; internal members `-1`; method-local bindings `-1` (`self`).

**API/consumer proof:** repository-wide code-fence search finds no call to `Converter<TQuantity>`, no call to `Convert` outside `In`, and no `Dimension` read outside `Override`. `api-unitsnet.md` proves `UnitConverter.TryConvert(QuantityValue, Enum, Enum, out double)` is the guarded dynamic conversion; the direct composition preserves the metre target, finite-result gate, `InvalidInput` fault, admitted `MetersPerUnit` division, and final `Tolerance.Of` band admission. `HashMap.AddOrUpdate(K,V)` remains unchanged. No generated Thinktecture surface replaces any deleted member.

**Ripples:** in `[03]-[MODEL_CONTEXT]`, remove the `ModelUnit.Dimension`, `Convert`, `Converter<TQuantity>`, hot-path, and fabricated Fabrication-consumer claims; state that `Override` compares a lane to `UnitsNet.Length.BaseDimensions` and composes `UnitConverter.TryConvert` directly. Keep UnitsNet in the Packages row. No other file names these members.

## 4. Keep cross-regime scaling on `ModelUnit` alone

Every live scale call already holds two admitted `ModelUnit` values. Tighten that internal owner to a non-null target and delete the unconsumed `Context.ScaleTo` forwarding entrypoint.

### 4a. Tighten and compress `ModelUnit.ScaleTo`

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, anchor `internal Fin<double> ScaleTo(ModelUnit? target, Op key)`.

**From:**

```csharp
internal Fin<double> ScaleTo(ModelUnit? target, Op key) =>
    from destination in Optional(target).ToFin(Fail: key.MissingContext())
    let scale = MetersPerUnit / destination.MetersPerUnit
    from admitted in double.IsFinite(d: scale) && scale > 0d
        ? Fin.Succ(value: scale)
        : Fin.Fail<double>(error: key.InvalidResult())
    select admitted;
```

**To:**

```csharp
internal Fin<double> ScaleTo(ModelUnit target, Op key) =>
    (MetersPerUnit / target.MetersPerUnit) switch {
        double scale when double.IsFinite(scale) && scale > 0d => scale,
        _ => key.InvalidResult(),
    };
```

### 4b. Delete `Context.ScaleTo`

**Location:** same file, anchor `public Fin<double> ScaleTo(Context? target)`.

**From:**

```csharp
public Fin<double> ScaleTo(Context? target) {
    Op op = Op.Of(name: nameof(ScaleTo));
    return Optional(target).ToFin(Fail: op.MissingContext())
        .Bind(destination => Unit.ScaleTo(target: destination.Unit, key: op));
}
```

**To:**

```csharp
```

**Effect:** combined fenced LOC `12 -> 5` (`-7`); declared members `-1` (`Context.ScaleTo`); public members `-1`; method-local bindings `-2` (`destination`, `admitted`).

**API/consumer proof:** direct consumers are `Drawing/sheet.md`, this page's `Default`, and six Rasm.Rhino fences. Every call passes a non-null admitted `ModelUnit`; none calls `Context.ScaleTo`. `Rasm.csproj` grants `InternalsVisibleTo` to `Rasm.Rhino`, so those calls do not require widening `ScaleTo` to public. The quotient, positive-finite predicate, and `InvalidResult` failure are unchanged for every admitted call. Target-typed switch arms lift the scalar and fault to `Fin<double>`.

**Ripples:** in `[03]-[MODEL_CONTEXT]`, replace the `Context.ScaleTo(Context)` claim with internal `ModelUnit.ScaleTo(ModelUnit, Op)` as the consumed cross-regime operation. No consumer code changes.

## 5. Delete the unused `Context.Units` forwarding alias

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, immediately after `public ModelUnit Unit { get; }`.

**From:**

```csharp
public UnitSystem Units => Unit.System;
```

**To:**

```csharp
```

**Effect:** fenced LOC `1 -> 0` (`-1`); declared members `-1`; public members `-1`.

**API/consumer proof:** no code fence in `libs/dotnet/` reads `Context.Units`. `ModelUnit.System` is the admitted value already reachable at `Context.Unit.System`; the alias adds no invariant or policy.

**Ripples:** none. A future consumer uses `context.Unit.System`; the greenfield spec owes no compatibility alias.

## 6. Close the override map behind its admitted read/write pair

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, anchor `public HashMap<ToleranceLane, Tolerance> Overrides`.

**From:**

```csharp
public HashMap<ToleranceLane, Tolerance> Overrides { get; init; } = HashMap<ToleranceLane, Tolerance>.Empty;
```

**To:**

```csharp
private HashMap<ToleranceLane, Tolerance> Overrides { get; init; } = HashMap<ToleranceLane, Tolerance>.Empty;
```

**Effect:** fenced LOC `1 -> 1` (`0`); declared members `0`; public members `-1`; private members `+1`; public write bypass removed.

**API/consumer proof:** `Override` is the sole code-fence write and performs conversion plus `Tolerance.Of`; `For` is the ruled sole read. Public `init` permits `context with { Overrides = ... }` to bypass both gates. No external code fence reads or writes the map.

**Ripples:** replace `Context.Overrides` with `Context.Override` in this page's Boundary card and in `libs/dotnet/Rasm.Rhino/.planning/Objects/state.md`, anchor `the WITNESS is Context.Overrides`. No code-fence ripple.

## 7. Remove the redundant explicit `Lazy<T>` mode

**Location:** `libs/dotnet/Rasm/.planning/Domain/context.md`, anchor `private static readonly Lazy<Context> Whole`.

**From:**

```csharp
private static readonly Lazy<Context> Whole = new(
    static () => Of(units: UnitSystem.Millimeters).ToFin().ThrowIfFail(),
    LazyThreadSafetyMode.ExecutionAndPublication);
```

**To:**

```csharp
private static readonly Lazy<Context> Whole = new(static () => Of(UnitSystem.Millimeters).ToFin().ThrowIfFail());
```

**Effect:** fenced LOC `3 -> 1` (`-2`); declared members `0`; behavior `0`.

**API/consumer proof:** `Lazy<T>(Func<T>)` already defaults to execution-and-publication thread safety. Static capture, first-read deferral, one cached result, and failure caching remain identical.

**Ripples:** none.

## Protected non-moves

- Retain every `ToleranceLane` row. Distinct keys are independently override-addressable, and the C# `DEEP_SURFACES` law explicitly rejects deleting a modeled axis merely because it has no current consumer.
- Retain `Tolerance.IsValid => ValidityClaim.All(...)`. `Domain/results.md` defines that fold as the corpus-wide `IValidityEvidence` convention; bypassing it saves no symbol.
- Do not convert `Tolerance` to `[ComplexValueObject]` or change `Context.For` to `Fin<Tolerance>` in this queue. The generated factory would conflict with the ruled invalid `0.0` device evidence, while a `Fin` return would force a broad consumer-carrier migration rather than a surgical reduction.
- Retain `Context.Fractional`; it is read repeatedly by Rhino fractional-tolerance APIs, and deleting it would duplicate `For(ToleranceLane.Fraction).Value` across consumers.
- Retain `Build`, `Default`, `DefaultRelative`, and lazy `Whole`. They own, respectively, independent applicative accumulation, the shared dependent scale chain, the named relative policy, and canonical-only failure timing. Replacing `Whole` with eager static initialization would change every `Context` static touch, not merely reduce a private symbol.
- Do not broaden `Override` beyond the existing length-versus-raw behavior. A general lane-dimension conversion requires a proved dimension-to-base-unit correspondence and is not an equivalent refinement.
