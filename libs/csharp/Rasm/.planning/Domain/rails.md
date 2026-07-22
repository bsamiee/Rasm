# [RASM_RAILS]

Kernel ROP substrate (`Rasm.Domain`). Every fallible kernel surface fails through one `Fault` band, every disposable crossing rides `Lease<T>`, every operation is keyed by one `Op` value, and every receipt proves itself through the `IValidityEvidence` fold — the floor no kernel page compiles without.

`Rasm.Domain` is a consumer-pinned namespace: the union-ops generator emits `global::Rasm.Domain.Op.Of`, Grasshopper sources alias `using Op = Rasm.Domain.Op`, and `Directory.Build.props` injects it as the Grasshopper-aware global using, so the name is frozen contract.

## [01]-[INDEX]

- [02]-[OPERATION_KEY]: `Op` — the caller-member-name operation key `[ValueObject<string>]`, its fault factory, acceptance bridge, and the `Catch`/`Side` boundary-exception rail.
- [03]-[GENERATOR_CONTRACTS]: `GenerateUnionOpsAttribute` — the opt-in codegen marker steering per-case `SelfOp` emission.
- [04]-[FAULT_BAND]: `Expected` + `Fault` — the closed kernel fault union and `FaultExtensions.Category`.
- [05]-[RESOURCE_RAIL]: `Lease<T>` — Owned/Borrowed disposal discipline with `Use`/`Resource`/`Dispose` folds.
- [06]-[VALIDITY_FOLD]: `IValidityEvidence` + `ValidityClaim` — the one receipt-validity fold; a receipt declares which claims hold, never how.
- [07]-[THREADING_LAW]: `Op` as explicit value key, `Eff<Env>` as runtime carriage, telemetry as a tap, one paradigm per operation.

## [02]-[OPERATION_KEY]

- Owner: `Op` `[ValueObject<string>]` readonly struct — the identity of one kernel operation, ordinal in both equality and ordering under one collation so comparison-zero and equality never diverge for case-differing member names. Every fault the key's factory mints carries the `Op` that raised it, and every acceptance gate is keyed by the `Op` that demanded it. Ambient faults carry their own evidence instead — a check-row key, a rejected scalar with its requirement, a unit system — where no single operation identity exists.
- Entry: `Op.Of([CallerMemberName] string name = "")` mints the key from the calling member; a `[GenerateUnionOps]` union case carries its generated `SelfOp` instead of re-minting per call. Public polymorphic surfaces accept `Op? key = null` resolved through `OrDefault()` (`validation.md`'s extension); internal kernels demand a required `Op key` tail parameter.
- Cases: four factory families partition by return rail — fault factories mint `Error`, acceptance bridges delegate to `OpAcceptance` (`validation.md`'s oracle) returning `Fin<T>`, scalar guards lift the `[06]` claim rows to `Fin<double>`, and the boundary-exception rail funnels host bodies to `Fin<Unit>`; collection- and shape-level admission is `validation.md`'s `Admit`, not a kernel guard.
- Law: `Catch` is the one inbound exception funnel — a captured `OperationCanceledException` surfaces as `Fault.Cancelled` (derived cancellations included, through `Error.HasException` recursion) so a mid-body cancel keeps its category instead of masquerading as a result failure, and every other capture becomes the `InvalidResult` detail. Input shape selects the arm: a rail-returning body rides `Catch<T>`, a void host body rides `Catch(Action)` onto the same `Fin<Unit>` funnel.
- Boundary: `Op` is a key, never a message channel — diagnostic text lives on the `Fault` case payloads and the key renders only inside the case `Message`. `ValidateFactoryArguments` rejects a blank key at mint — `[CallerMemberName]` never supplies one, so a whitespace literal is a caller defect surfacing at the generated factory, never a silent empty identity.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct Op {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value: value) ? new ValidationError(message: "Op requires a non-whitespace member name.") : null;
    [BoundaryAdapter] public static Op Of([CallerMemberName] string name = "") => Create(value: name);
    [BoundaryAdapter] public Error MissingContext() => new Fault.MissingContext(Key: this);
    [BoundaryAdapter] public Error InvalidContext() => new Fault.InvalidContext(Key: this);
    [BoundaryAdapter] public Error InvalidInput() => new Fault.InvalidInput(Key: this);
    [BoundaryAdapter] public Error InvalidResult(string? detail = null) => new Fault.InvalidResult(Key: this, Detail: Optional(detail).Filter(static text => !string.IsNullOrWhiteSpace(value: text)));
    [BoundaryAdapter] public Error Unsupported(Type geometryType, Type outputType) => new Fault.Unsupported(Key: this, GeometryType: geometryType, OutputType: outputType);
    [BoundaryAdapter] public Error Caution(string concern) => new Fault.Caution(Key: this, Concern: concern);
    [BoundaryAdapter] public Fin<T> AcceptInput<T>(T value) => OpAcceptance.AcceptInput(key: this, value: value);
    [BoundaryAdapter] public Fin<T> AcceptValue<T>(T value) => OpAcceptance.AcceptValue(key: this, value: value);
    [BoundaryAdapter] public Fin<string> AcceptText(string value) => AcceptValue(value: value).Map(static text => text.Trim());
    [BoundaryAdapter] public Fin<Unit> Confirm(bool success) => success ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: InvalidResult());
    [BoundaryAdapter] public Fin<T> Need<T>(T? value) where T : class => Optional(value).ToFin(Fail: InvalidInput());
    [BoundaryAdapter] public Fin<T> Need<T>(Option<T> value) => value.ToFin(Fail: InvalidInput());
    [BoundaryAdapter] public Fin<double> Finite(double value) => guard(ValidityClaim.Finite(value: value), InvalidInput()).ToFin().Map(_ => value);
    [BoundaryAdapter] public Fin<double> Positive(double value) => guard(ValidityClaim.Positive(value: value), InvalidInput()).ToFin().Map(_ => value);
    [BoundaryAdapter]
    public Fin<T> Catch<T>(Func<Fin<T>> body) {
        Op self = this;
        return Optional(body).ToFin(Fail: self.InvalidInput()).Bind(valid =>
            Try.lift<Fin<T>>(f: valid).Run().Match(
                Succ: static result => result,
                Fail: error => Fin.Fail<T>(error: error.HasException<OperationCanceledException>() ? new Fault.Cancelled() : self.InvalidResult(detail: error.Message))));
    }
    [BoundaryAdapter]
    public Fin<Unit> Catch(Action body) {
        Op self = this;
        return Optional(body).ToFin(Fail: self.InvalidInput()).Bind(valid => self.Catch(() => Fin.Succ(value: Side(action: valid))));
    }
    [BoundaryAdapter]
    public static Unit Side(Action action) {
        ArgumentNullException.ThrowIfNull(argument: action);
        action();
        return unit;
    }
    [BoundaryAdapter] public static Unit SideWhen(bool condition, Action action) => condition ? Side(action: action) : unit;
}
```

## [03]-[GENERATOR_CONTRACTS]

- Owner: `GenerateUnionOpsAttribute` — the one local analyzer/generator marker, resolved by metadata name `Rasm.Domain.GenerateUnionOpsAttribute` (`ForAttributeWithMetadataName`); the spelling is frozen contract.
- Auto: for every sealed record case of a `[GenerateUnionOps]` union the generator emits `internal static readonly global::Rasm.Domain.Op SelfOp = global::Rasm.Domain.Op.Of(name: nameof(<Case>));` into a partial case declaration — each case carries its own operation key, minted once, named after the case.
- Law: emission is strictly opt-in — `SelfOp` exists only for `[GenerateUnionOps]`-marked unions, whose cases are operations; the generator never visits an unmarked union, so a `[Union]` of carriers, resources, or requests (`Fault`, `Lease<T>`) receives nothing.
- Boundary: the attribute is designed vocabulary, not runtime behavior; a marked union with no sealed record cases is inert. Generator and analyzer rules home at the repository analyzer — this page owns only the contract name and the emitted `SelfOp` shape.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateUnionOpsAttribute : Attribute;
```

## [04]-[FAULT_BAND]

- Owner: `Expected` the abstract `Error` bridge pinning `IsExpected`/`IsExceptional` and lowering into the LanguageExt exception protocol through `WrappedErrorExpectedException`; `Fault` the closed `[Union]` of every kernel-substrate failure keyed by the raising `Op` payload (failure carriers, not operations, so no `[GenerateUnionOps]`); and `FaultExtensions` the `extension(Error)` block projecting `Category` off any `Error`.
- Cases: each case carries its typed payload keyed by the raising `Op` and renders its own `Message` and `Category`.
- Law: `Unsupported` is the only coded case — `UnsupportedCode` 9104 is the discriminant the Grasshopper drain reads to distinguish an unsupported projection from a hard failure; every other case discriminates by `Category` string and case type, and recovery predicates match on the case, never on rendered text.
- Law: payloads are evidence, never live resources — `InvalidGeometry` carries the failing `Type`, not the geometry reference, because coercion leases dispose before a fault surfaces, so a live payload hands consumers a disposed native object and retains host memory inside accumulating `Validation` rails. `OutOfRange` carries an optional `Op` — `None` from keyless factory admission (`context.md`'s triad), re-keyed to the demanding operation by the `AcceptValidated` bridge (`validation.md`); `InvalidValue` is its non-scalar sibling, the generated-factory rejection carrying the owner label and generated requirement text, minted only by that bridge.
- Law: `InvalidContext` names an execution-context refusal — a main-thread-affinity guard, a released lease, a dead conduit or live-state gate — distinct from `MissingContext` (no model context supplied) and `InvalidInput` (the value itself is unsound); recovery differs by case (marshal or re-acquire versus repair the argument), so host thread and lifecycle gates raise `InvalidContext`.
- Law: the two-family seam holds from the kernel side — a substrate failure (missing context, invalid input, cancelled run, unsupported projection) is a `Fault` case, a robust-core geometry failure is a `Rasm.Numerics` `GeometryFault`, and neither absorbs the other; a page composing both rails converts nothing, both already `Error`.
- Growth: a new substrate failure is one `Fault` case with its typed payload and `Category`.
- Boundary: `Fault` crosses the kernel, the `Rasm.Analysis` runtime, and the Grasshopper boundary as the one failure vocabulary; only self-sufficient `Message`, `Category`, and the 9104 code are read outside the kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Domain;

// --- [ERRORS] -------------------------------------------------------------------------------
[BoundaryAdapter]
public abstract record Expected : Error {
    protected Expected() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    public virtual string Category => "Fault";
}

[Union]
public abstract partial record Fault : Expected {
    private Fault() : base() { }
    internal const int UnsupportedCode = 9104;
    public sealed partial record MissingOperation : Fault { public override string Message => "Geometry operation is required."; public override string Category => "Operation"; }
    public sealed partial record MissingContext(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' requires a model context."; public override string Category => "Operation"; }
    public sealed partial record InvalidContext(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' was invoked outside its live execution context."; public override string Category => "Context"; }
    public sealed partial record InvalidInput(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' received invalid Rhino input."; public override string Category => "Input"; }
    public sealed partial record InvalidResult(Op Key, Option<string> Detail = default) : Fault {
        public override string Message => $"Geometry operation '{Key}' produced no valid Rhino result{Detail.Map(static d => $": {d}").IfNone(static () => ".")}";
        public override string Category => "Result";
    }
    public sealed partial record Cancelled : Fault { public override string Message => "Geometry operation was cancelled."; public override string Category => "Cancelled"; }
    public sealed partial record Unsupported(Op Key, Type GeometryType, Type OutputType) : Fault {
        public override string Message => $"Geometry operation '{Key}' does not support geometry '{GeometryType.Name}' with output '{OutputType.Name}'.";
        public override int Code => UnsupportedCode;
        public override string Category => "Unsupported";
    }
    public sealed partial record ComputationFailed(string Label) : Fault { public override string Message => $"Rhino {Label} computation failed."; public override string Category => "Computation"; }
    public sealed partial record MissingGeometry : Fault { public override string Message => "Geometry input is required."; public override string Category => "Geometry"; }
    public sealed partial record InvalidGeometry(Type Shape, string Check, string Log) : Fault {
        public override string Message => string.IsNullOrWhiteSpace(value: Log)
            ? $"Geometry validation failed for {Shape.Name} under check '{Check}'."
            : $"Geometry validation failed for {Shape.Name} under check '{Check}': {Log}";
        public override string Category => "Geometry";
    }
    public sealed partial record InvalidValue(string Label, string Requirement, Option<Op> Key = default) : Fault {
        public override string Message => $"Value '{Label}'{Key.Map(static k => $" (operation '{k}')").IfNone(static () => string.Empty)} is invalid: {Requirement}";
        public override string Category => "Input";
    }
    public sealed partial record OutOfRange(string Label, double Scalar, string Requirement, Option<Op> Key = default) : Fault {
        public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}'{Key.Map(static k => $" (operation '{k}')").IfNone(static () => string.Empty)} must be {Requirement}; actual={Scalar:R}.");
        public override string Category => "Tolerance";
    }
    public sealed partial record InvalidUnitSystem(UnitSystem Units, string Requirement) : Fault { public override string Message => $"Model unit system must be {Requirement}; actual={Units}."; public override string Category => "Context"; }
    public sealed partial record Caution(Op Key, string Concern) : Fault { public override string Message => $"Geometry operation '{Key}' raised a recoverable concern: {Concern}."; public override string Category => "Caution"; }
}

public static class FaultExtensions {
    extension(Error error) {
        public string Category => error switch {
            Expected expected => expected.Category,
            _ => "Fault",
        };
    }
}
```

## [05]-[RESOURCE_RAIL]

- Owner: `Lease<T>` — the closed `[Union]` over disposal ownership for any `T : class, IDisposable`. `Owned` carries a value this rail must dispose; `Borrowed` carries a value the host still owns.
- Entry: `Use(project)` and the state-threaded `Use(state, project)` are the sole consumption gate — an `Owned` value is projected inside a `using` window and disposed the moment the projection returns, a `Borrowed` value projected untouched; `Resource` reads the live value where the caller manages the extent, and `Dispose()` releases `Owned` and no-ops `Borrowed`.
- Law: ownership is a case, never a flag — the coercion lattice (`normalization.md`) returns `Fin<Lease<Curve|Surface|Brep>>` deciding owned-versus-borrowed per recovery path, `Requirement`'s lease-aware checks (`validation.md`) thread it, and projection carriers ride `Lease<GeometryBase>`.
- Law: the state-threaded `Use` overload keeps projections closure-free — state rides the fold, lambdas stay `static`.
- Boundary: `Owned.Project`'s `using` is the platform-forced disposal seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record Lease<T> where T : class, IDisposable {
    private Lease() { }
    public sealed record Owned(T Value) : Lease<T> {
        internal TResult Project<TResult>(Func<T, TResult> project) { using T owned = Value; return project(arg: owned); }
        internal TResult Project<TState, TResult>(TState state, Func<TState, T, TResult> project) { using T owned = Value; return project(arg1: state, arg2: owned); }
    }
    public sealed record Borrowed(T Value) : Lease<T>;
    public TResult Use<TResult>(Func<T, TResult> project) => Switch(state: project, owned: static (use, owned) => owned.Project(project: use), borrowed: static (use, borrowed) => use(arg: borrowed.Value));
    public TResult Use<TState, TResult>(TState state, Func<TState, T, TResult> project) =>
        Switch(state: (State: state, Project: project), owned: static (use, owned) => owned.Project(state: use.State, project: use.Project), borrowed: static (use, borrowed) => use.Project(arg1: use.State, arg2: borrowed.Value));
    public T Resource => Switch(owned: static owned => owned.Value, borrowed: static borrowed => borrowed.Value);
    public Unit Dispose() => Switch(owned: static owned => { owned.Value.Dispose(); return unit; }, borrowed: static _ => unit);
}
```

## [06]-[VALIDITY_FOLD]

- Owner: `IValidityEvidence` the corpus-wide evidence floor (one member, `IsValid`) every kernel receipt and carrier implements, and `ValidityClaim` the claim vocabulary whose `All` fold is the one mechanism a receipt's `IsValid` body composes.
- Entry: a receipt spells `public bool IsValid => ValidityClaim.All(...)` over its claim rows, the implicit `bool` conversion making the fold the body.
- Law: one claim vocabulary states each predicate once; a receipt declares which claims hold, never how a predicate is computed.
- Law: predicate policy is named once here — the scalar `Finite` is `RhinoMath.IsValidDouble`, screening both non-finite values and the host `RhinoMath.UnsetValue` sentinel because scalar fields on kernel receipts can carry host-read material; the span `Finite` is the vectorized `TensorPrimitives.IsFiniteAll` gate, correct for solver-produced arrays that never carry the host sentinel. `Admit` (`validation.md`) lifts these same claim rows into `Op`-keyed admission faults, one predicate statement serving both rails; a host-neutral-shaped receipt (`Numerics/*`) folds `Of(...)` claims over its own `double.IsFinite` and epsilon policy.
- Law: implementing `IValidityEvidence` registers a receipt with the acceptance oracle — `OpAcceptance.ValidityOf` (`validation.md`) reads the one `IValidityEvidence` arm, so a new receipt reaches the oracle with zero oracle edits.
- Boundary: the fold is validity evidence, never admission — admission rejects raw material at the boundary with typed faults (`validation.md`), the fold answers whether an already-constructed receipt carries coherent evidence. `All`'s span loop is the named kernel exemption.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics.Tensors;
using Rasm.Csp;
using Rhino;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
public interface IValidityEvidence { public bool IsValid { get; } }

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ValidityClaim(bool Holds) {
    public static ValidityClaim Of(bool holds) => new(Holds: holds);
    public static ValidityClaim Finite(double value) => new(Holds: RhinoMath.IsValidDouble(x: value));
    public static ValidityClaim Finite(Point3d point) => new(Holds: point.IsValid);
    public static ValidityClaim Finite(Vector3d vector) => new(Holds: vector.IsValid);
    public static ValidityClaim Finite(ReadOnlySpan<double> values) => new(Holds: TensorPrimitives.IsFiniteAll(values));
    public static ValidityClaim Nonnegative(double value) => new(Holds: RhinoMath.IsValidDouble(x: value) && value >= 0.0);
    public static ValidityClaim Positive(double value) => new(Holds: RhinoMath.IsValidDouble(x: value) && value > 0.0);
    public static ValidityClaim UnitInterval(double value) => new(Holds: RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= 1.0);
    public static ValidityClaim Ordered(double lower, double upper) => new(Holds: RhinoMath.IsValidDouble(x: lower) && RhinoMath.IsValidDouble(x: upper) && lower <= upper);
    public static ValidityClaim CountAtLeast(int count, int floor) => new(Holds: count >= floor);
    public static ValidityClaim CountExactly(int count, int expected) => new(Holds: count == expected);
    public static ValidityClaim Evidence(IValidityEvidence? evidence) => new(Holds: evidence is { IsValid: true });
    public static ValidityClaim Evidence<T>(Option<T> evidence) where T : IValidityEvidence =>
        new(Holds: evidence.Map(static value => value.IsValid).IfNone(noneValue: true));
    // Span fold kernel: the one statement-shaped body in the mechanism.
    public static ValidityClaim All(params ReadOnlySpan<ValidityClaim> claims) {
        foreach (ValidityClaim claim in claims) {
            if (!claim.Holds) { return new(Holds: false); }
        }
        return new(Holds: true);
    }
    public static implicit operator bool(ValidityClaim claim) => claim.Holds;
}
```

## [07]-[THREADING_LAW]

One Op-threading law rules every kernel page; no page re-decides it.

- Law: `Op` is an explicit VALUE — minted once at the public entry through `Op.Of()` or read off a union case's generated `SelfOp`, threaded as the trailing key parameter of every fallible kernel, and read by every fault factory; it identifies the failed operation, never runtime capability. Repeated `OrDefault()` inside one member is value-identical, never a split key: `[CallerMemberName]` resolves lexically to the enclosing member (lambdas included), so every resolution mints the equal `[ValueObject<string>]` value — the law is value identity, not call count.
- Law: `Eff<Env>` is the runtime CARRIAGE — a pipeline needing tolerance context, progress, or cancellation is `Eff<Env, T>` composing `Env.Asks`/`Env.EnvAsks`. No `Op` key enters `Env`, and no ambient static, `AsyncLocal`, or second key mechanism exists in the kernel.
- Law: below the `Eff` floor the synchronous rails thread `Context` and `CancellationToken` as explicit parameters (`Requirement.Apply(context, value, cancel)` is the canonical shape); at the floor and above, `Env` carries both. One operation is written in exactly one paradigm — a `Fin`/`Validation` body with a key tail, or an `Eff<Env, T>` pipeline threading the same key as a value.
- Law: telemetry is a TAP, never a rail — the `TelemetrySink` (`telemetry.md`) rides `Env` at the `Eff` floor or enters a synchronous gate point as one explicit trailing parameter beside `Context`/`CancellationToken`; facts publish through its one `Tap`, and an observe-side subscriber fault isolates onto the tap receipt, never failing the tapped operation.
- Boundary: `Env` is `Analysis/query.md`'s frozen record — this page legislates the carriage law, that page owns the record and the pipeline shape.

## [08]-[DENSITY_BAR]

One substrate floor; growth is a case, a claim row, or a generated `SelfOp`, never a sibling rail.

| [INDEX] | [CONCERN]          | [OWNER]                               | [KIND]                               | [RAIL]                     |
| :-----: | :----------------- | :------------------------------------ | :----------------------------------- | :------------------------- |
|  [01]   | Operation identity | `Op`                                  | `[ValueObject<string>]` fault/accept | `Op → Error`/`Op → Fin<T>` |
|  [02]   | Codegen contract   | `GenerateUnionOps`                    | opt-in marker + generated `SelfOp`   | `[Union] case → Op`        |
|  [03]   | Substrate faults   | `Expected` + `Fault`                  | typed `Expected`/`Fault` payloads    | `Fault → Error` subtype    |
|  [04]   | Resource ownership | `Lease<T>`                            | `[Union]` Owned/Borrowed cases       | `Lease<T>.Use → TResult`   |
|  [05]   | Receipt validity   | `IValidityEvidence` + `ValidityClaim` | evidence floor + claim fold          | `ValidityClaim.All → bool` |

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
