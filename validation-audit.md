# `validation.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/validation.md`

This audit keeps the declared validation domain intact and removes only proved duplication, forwarding, and hand-lifted carrier machinery. The checked-in `libs/dotnet/.api` and `libs/dotnet/Rasm/.api` catalogues are the API authority. Every move preserves fault identity, failure order, carrier posture, generated-owner admission, and native lease scope. Moves are ordered so the generated-surface improvement lands before local callers and helpers collapse.

## 1. Bind `AdmissionProjection.SmartEnum` to generated lookup

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:299` — in the non-generic `AdmissionProjection` owner, anchor `SmartEnumLookup<TRaw, TModel>` and `SmartEnum<TRaw, TModel>`.

### From — hand-declared lookup shape

```csharp
public static class AdmissionProjection {
    public delegate bool SmartEnumLookup<TRaw, TModel>(TRaw? key, out TModel? item)
        where TRaw : notnull
        where TModel : class, ISmartEnum<TRaw>;
```

### To

```csharp
public static class AdmissionProjection {
```

### From — caller-supplied signature

```csharp
    public static Fin<AdmissionProjection<TRaw, TModel>> SmartEnum<TRaw, TModel>(
        SmartEnumLookup<TRaw, TModel>? lookup,
        Op? key = null)
        where TRaw : notnull
        where TModel : class, ISmartEnum<TRaw> {
```

### To — owner-constrained signature

```csharp
    public static Fin<AdmissionProjection<TRaw, TModel>> SmartEnum<TRaw, TModel>(Op? key = null)
        where TRaw : notnull
        where TModel : class, ISmartEnum<TRaw, TModel, ValidationError>, IConvertible<TRaw> {
```

### From — caller-supplied body

```csharp
        Op op = key.OrDefault();
        return Optional(lookup).ToFin(op.InvalidInput()).Bind(valid =>
            AdmissionProjection<TRaw, TModel>.Of(
                render: static model => model.ToValue(),
                admit: raw => valid(key: raw, item: out TModel? item) && item is TModel admitted
                    ? Fin.Succ(admitted)
                    : Fin.Fail<TModel>(error: op.InvalidInput()),
                key: op));
    }
```

### To — generated body

```csharp
        Op op = key.OrDefault();
        return AdmissionProjection<TRaw, TModel>.Of(
            render: static model => model.ToValue(),
            admit: raw => TModel.TryGet(raw, out TModel? item) && item is TModel admitted
                ? Fin.Succ(admitted)
                : Fin.Fail<TModel>(error: op.InvalidInput()),
            key: op);
    }
```

### Effect

- Target fenced C# LOC: `-6` nonblank lines.
- Nested types: `-1` (`SmartEnumLookup<,>`).
- Parameters: `-1` public caller-supplied operation.
- Logic: removes one nullable delegate admission and one `Fin.Bind` layer.

### API / consumer proof

The checked-in Thinktecture catalogue proves that `ISmartEnum<TKey,T,TValidationError>` carries the static generated roster and keyed lookup contract, `TryGet(TKey?, out T)` is the non-throwing generated lookup, and `IConvertible<TRaw>.ToValue()` is the generated outbound projection. Constraining the owner to those contracts lets the projection call its owner directly; accepting the same operation back from every caller is a wrapper over a generated member. The failure arm remains `op.InvalidInput()`, and `AdmissionProjection<,>.Of` still validates and stores the resulting render/admit pair.

Repository-wide search finds no current C# consumer of `AdmissionProjection.SmartEnum`, so the signature change has no call-site edit. The owner remains because it is the declared bidirectional admission boundary; zero current consumers is not a deletion proof under the C# deep-surface law.

### Ripples

None outside this fence. Keep the `AdmissionProjection` index, owner prose, boundary law, and density row.

## 2. Remove the duplicate class-only complex-value-object lifter

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:387` — in `OpExtensions`, anchor `AcceptValidated<TVO>(ValidationError? fault, TVO? admitted) where TVO : class`.

### From

```csharp
        public Fin<TVO> AcceptValidated<TVO>(ValidationError? fault, TVO? admitted) where TVO : class =>
            (fault, admitted) switch {
                (null, TVO owner) => Fin.Succ(value: owner),
                (ValidationError refusal, _) => Fin.Fail<TVO>(error: InvalidValueOf<TVO>(op: op, refusal: refusal)),
                _ => Fin.Fail<TVO>(error: op.InvalidResult()),
            };
```

### To

```csharp
```

### Effect

- Target fenced C# LOC: `-6`.
- Public members: `-1` extension overload.

### API / consumer proof

The immediately preceding `AcceptValidated<TVO>(ValidationError? fault, object? admitted) where TVO : notnull` has the identical three-arm truth table. All twelve two-argument sites explicitly supply only `TVO`; after deletion the object-shaped overload remains the only one-generic-argument candidate, while the `<TVO,TRaw>` factory overload cannot bind from a partially supplied generic argument list. Ten already use the valid `admitted:` name and remain byte-identical; two Rhino geometry sites carry a pre-existing `value:` name that matches neither current overload and are a separate consumer defect, not a ripple introduced by this deletion. Reference values convert to `object?` without allocation, existing struct complex owners already use the object arm through boxing, and the same `InvalidValueOf<TVO>` and `InvalidResult` faults remain.

### Ripples

None. Existing valid `fault:` and `admitted:` named arguments remain valid.

## 3. Lift terminal readiness faults directly into `Validation`

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:59` and `:62` — the missing-value and missing-context arms of `Requirement.Apply<T>`.

### From

```csharp
            (null, _, _) => Fin.Fail<T>(error: new KernelFault.MissingGeometry()).ToValidation(),
            _ => Fin.Fail<T>(error: new KernelFault.MissingContext(Key: Operand)).ToValidation(),
```

### To

```csharp
            (null, _, _) => new KernelFault.MissingGeometry(),
            _ => new KernelFault.MissingContext(Key: Operand),
```

### Effect

- Target fenced C# LOC and symbols: unchanged.
- Logic: `-2` artificial `Fin<T>` constructions and `-2` `ToValidation` conversions.

### API / consumer proof

The checked-in LanguageExt catalogue explicitly records that a concrete `Validation<Error,T>` return slot accepts an `Error` through its implicit lift. Both arms are terminal failures carrying the same `KernelFault` values, with no success projection or accumulation order to alter. Keep `Operand.AcceptInput(candidate).ToValidation()` because that arm begins as a real `Fin<T>` computation.

### Ripples

None.

## 4. Collapse `Requirement.Add` into the `+` owner

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:50` — anchor `operator +` and `Requirement.Add`.

### From

```csharp
    public static Requirement operator +(Requirement left, Requirement right) => Add(left: left, right: right);
    public static Requirement Add(Requirement left, Requirement right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(checks: left.checks.Union(right.checks));
    }
```

### To

```csharp
    public static Requirement operator +(Requirement left, Requirement right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(checks: left.checks.Union(right.checks));
    }
```

### Effect

- Target fenced C# LOC: `-1`.
- Public members: `-1` (`Add`).

### API / consumer proof

Repository-wide search finds no `Requirement.Add` call. Every composition uses the already-declared `+` set-union algebra. Moving the only body one hop inward preserves left-before-right null checking and the same `Set<Check>.Union` result. Retain `IsEmpty`; `Analysis/query.md` reads it directly.

### Ripples

None.

## 5. Inline the single-use lease-check forwarder

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:88` and `:95` — anchor the `Capability.Form.Admits` switch arm and `RunLeaseChecks<T>`.

### From — call site

```csharp
            object value when Capability.Form.Admits(type: value.GetType()) =>
                RunLeaseChecks(lease: value.GeometryForm(key: Operand), checks: checks, context: context, original: original, cancel: cancel),
```

### To

```csharp
            object value when Capability.Form.Admits(type: value.GetType()) => value.GeometryForm(key: Operand).ToValidation()
                .Bind(native => native.Use(geometry => RunChecks(
                    checks: checks, context: context, geometry: geometry, original: original, cancel: cancel))),
```

### From — forwarder

```csharp
    private static Validation<Error, T> RunLeaseChecks<T>(Fin<Lease<GeometryBase>> lease, Set<Check> checks, Context context, T original, CancellationToken cancel)
        where T : notnull =>
        lease.ToValidation()
            .Bind(native => native.Use(geometry => RunChecks(checks: checks, context: context, geometry: geometry, original: original, cancel: cancel)));
```

### To

```csharp
```

### Effect

- Target fenced C# LOC: `-3`.
- Private members: `-1` generic helper.

### API / consumer proof

The helper is one `ToValidation().Bind(...)` expression with one caller and no independent name-level policy. `Lease.Use` remains the custody boundary around the exact same `RunChecks` computation; acquisition failure, check accumulation, and disposal timing do not move.

### Ripples

None.

## 6. Delete the one-use `OpAcceptance.Demand` member and the duplicate enum fast path

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:226` and `:231` — anchor the `ValidityOf(source: value).Case` switch and private `Demand<T>` extension member.

### From

```csharp
        internal Fin<T> AcceptValue<T>(T value) =>
            value switch {
                null => Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key)),
                Enum => Fin.Succ(value),
                _ => ValidityOf(source: value).Case switch {
                    bool ok => key.Demand(condition: ok, value: value),
                    _ => Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key)),
                },
            };
```

### To

```csharp
        internal Fin<T> AcceptValue<T>(T value) =>
            value switch {
                null => Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key)),
                _ => ValidityOf(source: value).Exists(static valid => valid)
                    ? Fin.Succ(value)
                    : Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key)),
            };
```

### From — one-use helper

```csharp
        private Fin<T> Demand<T>(bool condition, T value) =>
            condition ? Fin.Succ(value) : Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key));
```

### To — one-use helper

```csharp
```

### Effect

- Target fenced C# LOC: `-4`.
- Private members: `-1` extension member.

### API / consumer proof

The checked-in LanguageExt surface defines `Option.Exists(predicate)` as false for `None` and as the predicate result for `Some`. Therefore `Some(true)` remains the only success, while `Some(false)` and `None` converge on the same `KernelFault.InvalidResult`. `ValidityOf` already maps every `Enum` to `Some(true)`, so the preceding `Enum` success arm duplicates that exact route. The result replaces the raw `.Case` inspection with the carrier's predicate fold; the outer null arm remains first.

### Ripples

None.

## 7. Replace two bare-boolean switch shells with conditionals

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:148` — anchor `Check.Demand` and `Check.Apply`.

### From — verdict

```csharp
        internal Fin<Unit> Demand(GeometryBase geometry, bool condition, string log) =>
            condition switch {
                true => Fin.Succ(unit),
                false => Fin.Fail<Unit>(error: new KernelFault.InvalidGeometry(Shape: geometry.GetType(), Check: Key, Log: log)),
            };
```

### To

```csharp
        internal Fin<Unit> Demand(GeometryBase geometry, bool condition, string log) =>
            condition
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(error: new KernelFault.InvalidGeometry(Shape: geometry.GetType(), Check: Key, Log: log));
```

### From — cancellation/application

```csharp
        internal Fin<Unit> Apply(Context context, GeometryBase geometry, CancellationToken cancel) =>
            cancel.IsCancellationRequested switch {
                true => Fin.Fail<Unit>(error: Errors.Cancelled),
                false => Applies(geometry: geometry) ? Run(check: this, context: context, geometry: geometry) : Fin.Succ(unit),
            };
```

### To

```csharp
        internal Fin<Unit> Apply(Context context, GeometryBase geometry, CancellationToken cancel) =>
            cancel.IsCancellationRequested
                ? Fin.Fail<Unit>(error: Errors.Cancelled)
                : Applies(geometry: geometry) ? Run(check: this, context: context, geometry: geometry) : Fin.Succ(unit);
```

### Effect

- Target fenced C# LOC: `-2`.
- Symbols: unchanged.

### API / consumer proof

Both switches exhaust a bare `bool` and carry no pattern payload. The conditional forms preserve lazy fault construction, cancellation-first evaluation, and the applicability skip. A tuple switch would evaluate `Applies` before cancellation and is therefore not equivalent; an eager `guard` would mint refusal evidence on the success path.

### Ripples

None.

## 8. Collapse `Masked`'s payload-identical cases to one verdict column

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:663` — anchor the `Masked` union in `[08]-[VERDICT_CARRIERS]`.

### From

```csharp
[Union]
public abstract partial record Masked {
    private Masked() { }
    public sealed record Unchanged(string Value) : Masked;
    public sealed record Redacted(string Value) : Masked;
    public string Value => Switch(unchanged: static row => row.Value, redacted: static row => row.Value);
}
```

### To

```csharp
public sealed record Masked(string Value, bool Changed);
```

### Effect

- Target fenced C# LOC: `-6`.
- Public type symbols: `-2` (`Masked.Unchanged`, `Masked.Redacted`).
- Authored surface: removes one forwarding `Value` body and generated two-arm dispatch; adds the one discriminant column the cases encoded.

### API / consumer proof

Both cases carry the same `string Value`; no case owns distinct evidence, admission, identity, or behavior. The only surviving distinction is whether the transform changed the value, which is exactly one boolean axis on the result owner rather than two payload-identical types. Repository-wide reading finds one producer (`RedactedText.Mask`), one discriminant read (`MaskTally.Of`), and one shared-payload read (`MaskLedger.Recorded`). The producer already computes change from ordinal equality at the boundary, so no caller is asked to re-derive it and length-preserving redaction remains counted. This is a named record rather than the rejected raw `(string, bool)` tuple, so the result remains a domain owner and argument order remains named.

### Ripples

`libs/dotnet/Rasm.AppHost/.planning/Observability/telemetry.md` — keep the verdict authored where redaction runs.

```csharp
public static Masked Mask(Redactor redactor, string value) =>
    redactor.Redact(value) switch {
        var masked when string.Equals(masked, value, StringComparison.Ordinal) => new Masked.Unchanged(masked),
        var masked => new Masked.Redacted(masked),
    };
```

```csharp
public static Masked Mask(Redactor redactor, string value) {
    string masked = redactor.Redact(value);
    return new(Value: masked, Changed: !string.Equals(masked, value, StringComparison.Ordinal));
}
```

Consumer fenced C# LOC: `-1`.

`libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md` — read the stored axis directly; the shared `verdict.Value` read at `MaskLedger.Recorded` is unchanged.

```csharp
public static MaskTally Of(Masked verdict) =>
    new(verdict.Switch(unchanged: static _ => 0, redacted: static _ => 1));
```

```csharp
public static MaskTally Of(Masked verdict) =>
    new(verdict.Changed ? 1 : 0);
```

Prose ripples are exact: in the target, update the `[01]` `Masked` index entry, `[08]` Owner/Entry/Law/Growth/Packages bullets, and `[10]` Masked density row from case/`Switch` wording to the `Changed` verdict column; in `Observability/telemetry.md`, update the redaction Entry/Law/Packages statements; in `Observability/bundles.md`, update the settled-composition, tally Law, and Packages statements. Do not change redaction policy or tally semantics.

## 9. Inline the single-use `Admit.Frame` predicate into `Plane`

### Location

`libs/dotnet/Rasm/.planning/Domain/validation.md:747` — anchor adjacent `Admit.Frame` and `Admit.Plane`.

### From

```csharp
    internal static ValidityClaim Frame(Plane basis) =>
        ValidityClaim.All(
            basis.IsValid,
            ValidityClaim.Finite(basis.Origin),
            ValidityClaim.Finite(basis.XAxis),
            ValidityClaim.Finite(basis.YAxis),
            ValidityClaim.Finite(basis.ZAxis),
            Vector3d.AreOrthonormal(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis),
            Vector3d.AreRighthanded(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis));
    internal static Fin<Plane> Plane(Plane basis, Op key) => guard(Frame(basis: basis), key.InvalidInput()).ToFin().Map(_ => basis);
```

### To

```csharp
    internal static Fin<Plane> Plane(Plane basis, Op key) =>
        guard(ValidityClaim.All(
            basis.IsValid,
            ValidityClaim.Finite(basis.Origin),
            ValidityClaim.Finite(basis.XAxis),
            ValidityClaim.Finite(basis.YAxis),
            ValidityClaim.Finite(basis.ZAxis),
            Vector3d.AreOrthonormal(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis),
            Vector3d.AreRighthanded(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis)), key.InvalidInput()).ToFin().Map(_ => basis);
```

### Effect

- Target fenced C# LOC: `-1`.
- Internal members: `-1` predicate helper.

### API / consumer proof

Repository-wide search finds no `Admit.Frame` or static-imported `Frame` consumer; `Plane` is the only read. `Frame` carries no independent policy, fault, carrier, or reuse axis: it exists solely to forward one predicate into `Plane`. Every Rhino frame condition, `ValidityClaim.All` order, and `key.InvalidInput()` refusal remains exact.

### Ripples

None.

## Rejected attractive rewrites

- Do not delete `AdmissionProjection<TRaw,TModel>` because it has no current call site. It is the declared bidirectional boundary owner, holds the operation key and exception-admission posture once, and is explicitly protected by the C# rule that zero current consumers never lowers a deep surface's domain bar. Move 1 removes only its caller-supplied copy of Thinktecture lookup.
- Do not inline `CurveSelfIntersectionReport` into the `Check.CurveSelfIntersection` row. The helper is the named statement/resource-lifetime exemption: it owns a nullable disposable `CurveIntersections` lease and a multi-arm diagnostic fold. Inlining removes a symbol by pushing an eight-line statement kernel into the smart-enum declaration, increasing roster complexity without reducing logic.
- Do not inline `ValueShapes` into `ValueValidity`. `ValueShapes` is the named residual authority for Rhino value shapes not derivable from `Kind.Items`; the dictionary is a secondary index derived from that roster. An inline array deletes the primary semantic name and merely relocates the same literals.
- Do not route `AcceptResults<TValue,TOut>` through `OutputBinding`. That boxes every value, materializes a second sequence, and can change null-enumerable versus unsupported-type fault precedence. It is relocation with a semantic delta.
- Do not make `ICapability<TSelf>` inherit the string-keyed Thinktecture interface merely to delete `Lookup`. Live capability owners include `int` and host-enum keys (`MassKind`, `ActionGate`, `PluginKind`, and `LicenseGrant`); pinning the generic floor to `string` is false, while adding a second key type parameter would ripple through the entire capability corpus for one derived index.
- Do not collapse the raw-specific `AcceptValidated<TVO>` overloads into `AcceptValidated<TVO,TRaw>`. C# does not partially infer method type arguments, and the one-type-argument call shape used throughout the corpus depends on those overloads.
- Do not replace `ValidationClause` with `string` or `ValidationError`. The clause is reusable typed evidence consumed by both the generated-hook projection and the kernel-fault projection; `ValidationError` is the final message-only factory outcome, not a clause identity. The apparent type deletion erases the distinction and does not reduce the 161 clause mints.
- Do not rewrite `AdmissionSlots.Indexed` or `Finite` as span-to-sequence traversals. A `ReadOnlySpan` must be copied before a lambda can escape; the existing statement kernels accumulate the same ordered faults without the extra materialization.
- Do not merge the two `AdmissionSlots.Accumulate` overloads. `Seq<T>` invariance keeps concrete `Validation<Error,Unit>` slots distinct from already-erased `K<Validation<Error>,Unit>` slots, exactly as the checked-in LanguageExt catalogue records.
- Do not inline `MeshReport`, `HasUsableDomain`, or `RequirementContext.Validate`. Each has multiple consumers or owns an independent lifetime/semantic boundary; deleting the name would duplicate or obscure its logic.

## Net accepted result

If all nine moves land together:

- Target fenced C# LOC: `-29` nonblank lines.
- Consumer fenced C# LOC: `-1` nonblank line.
- Nested/public case types: `-3`.
- Public members: `-2`.
- Internal/private members: `-3`.
- Public parameters: `-1`.

The totals give no credit for prose and count the two carrier-direct lifts only as logic removal. The result preserves every fault, carrier, admission, and native resource boundary while deleting one hand-shaped generated API, one duplicate overload, two payload-identical verdict cases, and four forwarding or one-use members.
