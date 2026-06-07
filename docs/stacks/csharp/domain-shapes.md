# [DOMAIN_SHAPES]

Generated domain shapes own admission, identity, bounded vocabulary, closed variants, generated factories, validation partials, equality, comparison, and dispatch. Thinktecture is the normal implementation surface for those decisions; LanguageExt carries admitted values and typed failures after the generated owner accepts or rejects raw input.

## [1][SHAPE_CHOOSER]

Choose the owner from modeling pressure. A generated type is justified when it removes primitive admission, unowned comparison, external behavior tables, nullable payload bags, or repeated dispatch from callers.

| [INDEX] | [PRESSURE]                     | [OWNER]                         | [REJECT]                         |
| :-----: | :----------------------------- | :------------------------------ | :------------------------------- |
|   [1]   | one raw value with invariants   | `[ValueObject<T>]`              | primitive parameter policy       |
|   [2]   | several fields form one value   | `[ComplexValueObject]`          | positional record identity       |
|   [3]   | bounded rows with behavior      | `[SmartEnum<TKey>]`             | enum plus dictionary             |
|   [4]   | closed payload variants         | `[Union]`                       | nullable payload bag             |
|   [5]   | generated operation identity    | `[Union]` plus `GenerateUnionOps` | parallel operation table       |
|   [6]   | closed result or evidence       | `[Union]` plus `SkipUnionOps`   | generated operations by habit    |
|   [7]   | adapter-only raw alternatives   | ad-hoc type-list union          | public domain ad-hoc variants    |
|   [8]   | generator-unsupported control   | manual owner                    | hand-written generated clone     |
|   [9]   | external protocol shape         | boundary DTO                    | serializer-shaped domain owner   |
|  [10]   | inert data with no invariant    | `record` or `record struct`     | decorative generated type        |

Generated owners are the default when the generator can express the invariant. Manual owners are reserved for generator-proven unsupported shape, stack-confined or open-ended state, type-indexed projection that cannot be modeled by `[Union]`, or total dispatch the generated surface cannot express. Generic unions are allowed when the generator supports the shape and the type parameter is semantic payload, state, or result evidence.

## [2][ADMISSION_STACK]

Admission is a stack. Raw input is normalized at the boundary, generated factories enforce the invariant, validation partials express owner-local rejection, comparer attributes declare identity, and one rail bridge projects the generated outcome into `Fin<T>` or `Validation<Error,T>`.

Raw candidate:
    Owner: boundary adapter.
    Job: trim, parse, decode, or project foreign shape only when the protocol requires it.
    Reject: carrying raw strings, numbers, nulls, or sentinel values into domain signatures.

Generated factory:
    Owner: `[ValueObject<T>]`, `[ComplexValueObject]`, or `[SmartEnum<TKey>]`.
    Job: expose generated `Create`, `TryCreate`, `Validate`, item lookup, and parsing directly.
    Reject: helper factories that only rename generated members.

Validation partial:
    Owner: generated partial hook.
    Signature: `ValidateFactoryArguments(ref ValidationError?, ref T1, ref T2, ...)` or the custom validation error type declared by `[ValidationError<TFault>]`.
    Rule: camelCase `ref` parameters match generated property names.
    Reject: post-construction validation that lets invalid values exist.

Typed failure:
    Owner: `[ValidationError<TFault>]` and the fault union.
    Rule: use a generated fault only when callers need the domain fault at admission time.
    Reject: stringly validation messages passed through reusable domain logic.

Comparer policy:
    Owner: generated key or member comparer attributes.
    Use: `[KeyMemberEqualityComparer]`, `[KeyMemberComparer]`, member comparers, and declared string comparison policy when identity has non-default comparison.
    Reject: hand-written comparer classes beside generated owners.

Rail bridge:
    Owner: boundary adapter or one declared projection when several boundary values share the same ingress shape.
    Rule: generated success or failure converts once into `Fin<T>` or `Validation<Error,T>`.
    Reject: both a generated factory wrapper and a second `Validate` wrapper for the same value.

```csharp conceptual
[Union]
public abstract partial record <Fault> : Error, IValidationError<<Fault>> {
    private <Fault>() : base() { }

    public sealed record InvalidRange(int Start, int End) : <Fault>;
    public sealed record InvalidRaw(string Detail) : <Fault>;

    public override string Message => Switch(
        invalidRange: static fault => $"invalid range {fault.Start}:{fault.End}",
        invalidRaw: static fault => fault.Detail);

    public static <Fault> Create(string message) => new InvalidRaw(Detail: message);
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct <CodeValue> {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim().ToUpperInvariant();
        validationError = value.Length is >= 3 and <= 12
            ? null
            : new ValidationError(message: "<CodeValue> must contain 3 to 12 characters.");
    }
}

[ComplexValueObject(DefaultStringComparison = StringComparison.Ordinal)]
[ValidationError<<Fault>>]
public readonly partial struct <RangeValue> {
    public <CodeValue> Code { get; }
    public int Start { get; }
    public int End { get; }

    static partial void ValidateFactoryArguments(
        ref <Fault>? validationError,
        ref <CodeValue> code,
        ref int start,
        ref int end) =>
        validationError = start < end ? null : new <Fault>.InvalidRange(Start: start, End: end);
}

public static Fin<<CodeValue>> AdmitCode(string raw) =>
    <CodeValue>.TryCreate(value: raw, obj: out <CodeValue> code)
        ? Fin.Succ(code)
        : Fin.Fail<<CodeValue>>(new <Fault>.InvalidRaw(Detail: "raw value was rejected"));
```

## [3][GENERATED_OWNERS]

Scalar value object:
    Shape: `[ValueObject<T>]`.
    Use: one primitive representation with admission, normalization, equality, comparison, parsing, or operator policy.
    Gate: keep `Create` for trusted construction and `TryCreate` or `Validate` for untrusted ingress.
    Default structs: allow zero-init only when zero is a real domain value.
    Reject: public raw primitive signatures after the value object exists.

Complex value object:
    Shape: `[ComplexValueObject]`.
    Use: multi-field identity with generated construction and equality.
    Gate: use a partial class or partial struct with get-only properties.
    Ordering: write owner-local comparison only when multi-field ordering is a domain operation.
    Reject: records, record structs, positional record parameters, and `{ get; init; }` identity fields.

Smart enum:
    Shape: `[SmartEnum<TKey>]`.
    Use: bounded cases with stable keys, lookup, parsing, generated item sets, and case-local behavior.
    Behavior: put `[UseDelegateFromConstructor]` members on the smart enum when each case owns the operation.
    Allow: private static item factories only when they compress repeated case initializers inside the same owner.
    Allow: derived read-mostly indexes from generated `Items` only when the index does not own behavior.
    Reject: native enum plus parallel dictionaries, switch tables, or behavior services.

```csharp conceptual
[SmartEnum<int>]
public sealed partial class <Mode> {
    public static readonly <Mode> Strict = new(
        key: 1,
        apply: static input => input.Score > 0
            ? Fin.Succ(new <Receipt>(Code: input.Code, Count: input.Score))
            : Fin.Fail<<Receipt>>(new <Fault>.InvalidRaw(Detail: "score was rejected")));

    public static readonly <Mode> Lenient = new(
        key: 2,
        apply: static input => Fin.Succ(new <Receipt>(Code: input.Code, Count: Math.Max(input.Score, 0))));

    [UseDelegateFromConstructor]
    internal partial Fin<<Receipt>> Apply(<Input> input);

    internal static Fin<<Mode>> Parse(int key) =>
        TryGet(key: key, item: out <Mode>? mode) && mode is not null
            ? Fin.Succ(mode)
            : Fin.Fail<<Mode>>(new <Fault>.InvalidRaw(Detail: "mode was rejected"));
}
```

Union:
    Shape: `[Union]`.
    Use: closed variants with named payloads and generated `Switch` or `Map` dispatch.
    Result rule: dispatch arms must unify to one exact result type, including generic parameters.
    Context rule: use `SwitchMapStateParameterName` or generated state-threaded overloads when 3 or more arms share context.
    Collapse rule: same-payload collapse targets passive, non-generic, non-error unions with repeated payload shape. It does not target empty marker cases, behavior-only cases, `Expected` or `Error` failures, two-case pairs, semantically different member names, or cases with owned behavior.
    Reject: optional payload bags, repeated switch arms, generated case aliases, and helper dispatch.

```csharp conceptual
[GenerateUnionOps]
[Union(SwitchMapStateParameterName = "runtime")]
public abstract partial record <Command> {
    private <Command>() { }

    public sealed record CreateCase(<RangeValue> Value) : <Command>;
    public sealed record MeasureCase(Seq<<CodeValue>> Codes) : <Command>;
    public sealed record RecoverCase(string Raw) : <Command>;

    public Eff<<Runtime>, <Receipt>> Run() =>
        from runtime in Eff.runtime<<Runtime>>().As()
        from receipt in Switch(
            runtime: runtime,
            createCase: static (runtime, command) =>
                runtime.Mode.Apply(new <Input>(Code: command.Value.Code, Score: command.Value.End - command.Value.Start)),
            measureCase: static (runtime, command) =>
                command.Codes
                    .TraverseM(code => runtime.Mode.Apply(new <Input>(Code: code, Score: 1)))
                    .As()
                    .Map(static receipts => receipts.Fold(<Receipt>.Empty, static (sum, item) => sum + item)),
            recoverCase: static (runtime, command) =>
                AdmitCode(command.Raw).Bind(code => runtime.Mode.Apply(new <Input>(Code: code, Score: 1))))
        select receipt;
}
```

Ad-hoc union:
    Shape: type-list union.
    Use: narrow adapter alternatives where foreign input arrives in several raw representations.
    Gate: project into the domain owner before domain logic.
    Reject: public domain variants hidden inside ad-hoc unions.

Manual variant owner:
    Shape: sealed manual owner with total dispatch.
    Use: generator-proven unsupported shape, stack-confined state, open-ended state, type-indexed projection, or constraints the generated union cannot express.
    Gate: keep construction closed and dispatch total.
    Reject: manual unions that only reproduce generated `Switch` or `Map`.

```csharp conceptual
public readonly ref struct <FrameOwner> {
    private readonly ReadOnlySpan<<Input>> inputs;

    public <FrameOwner>(ReadOnlySpan<<Input>> inputs) =>
        this.inputs = inputs;

    public <Result> Fold(<State> state, Func<<State>, <Input>, <State>> step, Func<<State>, <Result>> finish) {
        <State> current = state;
        foreach (<Input> input in inputs) {
            current = step(current, input);
        }

        return finish(current);
    }
}
```

Boundary DTO:
    Shape: protocol-owned carrier.
    Use: external wire shape, host API shape, or serialization contract that differs from the domain owner.
    Gate: translate at the boundary and keep the domain owner canonical.
    Reject: duplicate DTOs beside generated owners when no external protocol demands them.

## [4][DISPATCH_POLICY]

Generated dispatch is the behavior owner when a closed vocabulary or variant controls execution. Add behavior to the generated owner before adding another service, helper, dictionary, or wrapper.

State-threaded dispatch:
    Use: pass shared context through generated state overloads when repeated arms capture the same value.
    Result: branch lambdas stay static and return the same rail or value type.
    Reject: capturing the same context in every branch.

Smart-enum row behavior:
    Use: constructor delegates or generated members when each case has behavior.
    Result: adding a case forces its behavior row to be supplied with the case.
    Reject: external switch tables, lookup dictionaries, and case-name branching.

Union operation routing:
    Use: generated `Switch`, `Map`, and selected overloads for total case dispatch.
    Boundary stop: partial overloads are allowed only when the owner deliberately stops dispatch at a named case boundary.
    Reject: case aliases that only forward to generated construction.

Algebraic operators:
    Use: owner-local operators only when the type has explicit algebraic laws.
    Gate: generated unions do not imply domain `+` or `|`.
    Reject: operator sugar that only hides dispatch.

## [5][LOCAL_GENERATION_POLICY]

`GenerateUnionOps` and `SkipUnionOps` are repository-local policy attributes for generated union operation shape. They are not Thinktecture package options.

GenerateUnionOps:
    Use: operation or intent unions that own generated per-case operation identity.
    Effect: emit the owner-local operation surface for each case.
    Reject: separate operation tables that drift from generated cases.

SkipUnionOps:
    Use: result unions, evidence unions, DTO-shaped unions, private implementation unions, or generated dispatch surfaces where operation routing belongs elsewhere.
    Effect: opt out explicitly so the absence is intentional.
    Reject: unqualified generated unions in functional surfaces.

Analyzer feedback:
    Rule: analyzer findings around generated dispatch, generated case aliases, manual closed unions, closed-union plan fusion, passive sibling surfaces, and decorative generated shapes are architecture pressure.
    Action: fix the domain shape first.
    Exception: refine the analyzer only when current source proves a false positive.

## [6][BOUNDARIES]

Rail boundary:
    Rule: generated validation admits or rejects raw values; LanguageExt transports the admitted value or typed failure.
    Reject: generated validation exceptions or raw generated failures leaking through domain code.

Serialization:
    Rule: keep serialization policy explicit at the boundary.
    Gate: integration packages stay inactive until package graph and an accepted owner prove adoption.
    Reject: serializer-shaped domain owners.

Factory hiding:
    Option: `SkipFactoryMethods`.
    Use: only when a stronger owner deliberately hides generated factory surface behind an algebraic or rail-returning constructor.
    Rule: keep construction inside the generated owner; do not hand-write constructor infrastructure the generator still owns.
    Reject: helper-only replacement factories.

Generated storage:
    Option: `UseSingleBackingField`.
    Use: only when memory shape or generated semantics require one backing field.
    Gate: keep the choice with the generated owner.

Conversion operators:
    Use: public boundary ergonomics only.
    Gate: domain admission remains explicit.
    Reject: implicit conversion that bypasses validation.

Partial generated splits:
    Use: generator-owned declarations, implementations, validation hooks, and behavior members.
    Reject: arbitrary file fragmentation to make generated owners look smaller.

## [7][REJECTIONS]

Primitive obsession:
    Wrong: raw strings, numbers, nulls, and sentinels in reusable domain signatures.
    Replacement: generated value object admission followed by one rail bridge.

Enum plus dictionary:
    Wrong: native enum keys with behavior, labels, delegates, or comparer policy in parallel maps.
    Replacement: smart enum rows with generated lookup and owner-local behavior.

Switch table:
    Wrong: caller-side case-name branching for closed variants.
    Replacement: generated union dispatch or smart-enum delegate behavior.

Nullable payload bag:
    Wrong: one record with many optional members and a discriminator.
    Replacement: generated union with named payload cases.

Helper-only factories:
    Wrong: methods that only rename generated `Create`, `TryCreate`, lookup, or validation.
    Replacement: generated surface or one real boundary projection into a rail.

Decorative generated shape:
    Wrong: generated attributes on inert carriers with no invariant, dispatch, admission, or behavior.
    Replacement: plain `record` or `record struct`.

## [8][VALIDATION]

- [ ] The shape follows modeling pressure, not package inventory.
- [ ] Generated factories, validation partials, equality, comparison, and dispatch are used directly.
- [ ] Manual owners are limited to generator-proven unsupported shape, stack-confined state, open-ended state, type-indexed projection, or total dispatch the generator cannot express.
- [ ] Generic unions are not rejected merely because they are generic.
- [ ] Rail conversion happens once after generated admission.
- [ ] Smart-enum behavior and union dispatch stay on the generated owner.
- [ ] `GenerateUnionOps` and `SkipUnionOps` are deliberate on functional unions.
- [ ] Package versions, graph state, globals, and integration-package adoption stay out of generated-shape policy.
