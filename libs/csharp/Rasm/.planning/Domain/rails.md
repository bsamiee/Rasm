# [RASM_RAILS]

The kernel ROP substrate (`Rasm.Domain`). This page owns the operation key `Op` with its fault and acceptance factory, the kernel fault band `Expected`/`Fault`, the `Lease<T>` resource-ownership rail, the corpus-wide validity fold (`IValidityEvidence` + `ValidityClaim`), the union-ops generator contracts, and the ONE Op-threading law every kernel page obeys. Nothing in the kernel compiles without this floor: every fallible surface fails through `Fault`, every disposable crossing rides `Lease<T>`, every receipt proves itself through the fold, and every operation is keyed by one `Op` value.

The namespace mirrors the folder path and is consumer-pinned: the union-ops generator emits `global::Rasm.Domain.Op.Of`, eleven Grasshopper sources alias `using Op = Rasm.Domain.Op`, `Directory.Build.props` injects `Rasm.Domain` as the Grasshopper-aware global using, and the sibling planning corpus anchors `Rasm.Domain` by name.

## [01]-[INDEX]

- [02]-[OPERATION_KEY]: `Op` — the caller-member-name operation key `[ValueObject<string>]` with the full fault factory, the acceptance bridge, and the `Catch`/`Side` boundary-exception rail.
- [03]-[GENERATOR_CONTRACTS]: `GenerateUnionOpsAttribute` — the strictly opt-in codegen marker steering per-case `SelfOp` emission; an unmarked union receives nothing.
- [04]-[FAULT_BAND]: `Expected` + `Fault` — the twelve-case kernel fault union, `FaultExtensions.Category`, and the explicit two-family seam against the robust-core `GeometryFault` band 2400.
- [05]-[RESOURCE_RAIL]: `Lease<T>` — Owned/Borrowed disposal discipline with `Use`/`Resource`/`Dispose` folds.
- [06]-[VALIDITY_FOLD]: `IValidityEvidence` + `ValidityClaim` — the one receipt-validity mechanism that retires the corpus-wide hand-rolled `IsValid` predicate swarm.
- [07]-[THREADING_LAW]: the ONE Op-threading law — `Op` as explicit value key, `Eff<Env>` as runtime carriage, no dual paradigm.

## [02]-[OPERATION_KEY]

- Owner: `Op` `[ValueObject<string>]` readonly struct — ordinal equality AND ordinal ordering, one collation, so comparison-zero and equality never diverge for case-differing member names — the identity of one kernel operation. Every fault minted through the key's factory carries the `Op` that raised it; every acceptance gate is keyed by the `Op` that demanded it. The ambient cases carry their own evidence instead — a check-row key, a rejected scalar with its requirement, a unit system — because no single operation identity exists where they arise.
- Entry: `Op.Of([CallerMemberName] string name = "")` mints the key from the calling member with zero ceremony; a `[GenerateUnionOps]` union case carries its generated `SelfOp` instead of re-minting per call. Public polymorphic surfaces accept `Op? key = null` and resolve through `OrDefault()` (the extension is `validation.md`'s); internal kernels demand a required `Op key` tail parameter.
- Cases: fault factories `MissingContext()`/`InvalidInput()`/`InvalidResult(detail?)`/`Unsupported(geometryType, outputType)`/`Caution(concern)` → `Error`; acceptance bridges `AcceptInput`/`AcceptValue`/`AcceptText`/`Confirm`/`Need`(class + `Option<T>`) → `Fin<T>` delegating to `OpAcceptance` (`validation.md`'s oracle); scalar guards `Finite`/`Positive` → `Fin<double>` lifting the `[06]` claim rows; boundary-exception rail `Catch<T>(Func<Fin<T>>)` + side-effect brackets `Side(Action)`/`SideWhen(bool, Action)`.
- Law: `Catch` is the one inbound exception funnel — `Try.lift` captures the throwing body, a captured `OperationCanceledException` surfaces as `Fault.Cancelled` (`Error.HasException<E>` discriminates, recursing `ManyErrors`, so a host call cancelled mid-body keeps its category — derived cancellations included — instead of masquerading as a result failure), every other capture survives as the `InvalidResult` detail, and the self-flattening `Match` collapses the outer `Try` rail into the body's inner `Fin`. A bare `try`/`catch` in domain flow is the deleted form.
- Law: `Finite`/`Positive` lift the `[06]` claim rows (`ValidityClaim.Finite`/`Positive`) into key-bound admission — the host predicate is stated once, on the claim row, never re-spelled here; collection- and shape-level admission is `validation.md`'s `Admit` vocabulary, never re-spelled per kernel.
- Boundary: `Op` is a key, never a message channel — diagnostic text lives on the `Fault` case payloads; the key renders inside the case `Message` and nowhere else. The `ValidateFactoryArguments` partial rejects a blank key at mint — `[CallerMemberName]` never supplies one, so a whitespace literal is a caller defect surfacing at the generated factory, never a silent empty identity.

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
    public static Unit Side(Action action) {
        ArgumentNullException.ThrowIfNull(argument: action);
        action();
        return unit;
    }
    [BoundaryAdapter] public static Unit SideWhen(bool condition, Action action) => condition ? Side(action: action) : unit;
}
```

## [03]-[GENERATOR_CONTRACTS]

- Owner: `GenerateUnionOpsAttribute` — the one local analyzer/generator marker. The union-ops generator resolves it by metadata name `Rasm.Domain.GenerateUnionOpsAttribute` (`ForAttributeWithMetadataName`); the spelling is frozen contract.
- Auto: for every sealed record case of a `[GenerateUnionOps]` union, the generator emits `internal static readonly global::Rasm.Domain.Op SelfOp = global::Rasm.Domain.Op.Of(name: nameof(<Case>));` into a partial case declaration — each case carries its own operation key, minted once, named after the case.
- Law: emission is strictly opt-in — `SelfOp` exists only for `[GenerateUnionOps]`-marked unions, whose cases are operations. A `[Union]` whose cases are carriers, resources, or requests simply carries no marker (`Fault` and `Lease<T>` on this page — failure and resource cases are carriers, never operations): the generator never visits an unmarked union, so no suppression attribute exists or is needed, and a `[SkipUnionOps]` opt-out marker is the deleted form.
- Boundary: the attribute is designed vocabulary, not runtime behavior — it carries no members, applies to classes and structs, and never inherits; a marked union with no sealed record cases is inert (the generator emits nothing). The generator and its analyzer rules live with the repository analyzer; this page owns only the contract name and the emitted `SelfOp` shape.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateUnionOpsAttribute : Attribute;
```

## [04]-[FAULT_BAND]

- Owner: `Expected` — the abstract `Error` bridge pinning `IsExpected`/`IsExceptional` and lowering into the LanguageExt exception protocol through `WrappedErrorExpectedException` — plus `Fault`, the closed `[Union]` of every kernel-substrate failure (cases are failure carriers keyed by the raising `Op` payload, so the union carries no `[GenerateUnionOps]` — a per-case `SelfOp` is twelve dead fields), and `FaultExtensions`, the `extension(Error)` block projecting `Category` off any `Error`.
- Cases: `MissingOperation` (Operation) · `MissingContext(Op)` (Operation) · `InvalidInput(Op)` (Input) · `InvalidResult(Op, Option<string>)` (Result) · `Cancelled` (Cancelled) · `Unsupported(Op, Type, Type)` (Unsupported, code 9104) · `ComputationFailed(string)` (Computation) · `MissingGeometry` (Geometry) · `InvalidGeometry(Type, string, string)` (Geometry) · `OutOfRange(string, double, string, Option<Op>)` (Tolerance) · `InvalidUnitSystem(UnitSystem, string)` (Context) · `Caution(Op, string)` (Caution) — twelve cases, each carrying its typed payload and rendering its own `Message`.
- Law: `Unsupported` is the only coded case — `UnsupportedCode` 9104 is the discriminant the Grasshopper drain reads to distinguish an unsupported projection from a hard failure; every other case discriminates by `Category` string and case type, and recovery predicates match on the case, never on rendered text.
- Law: payloads are evidence, never live resources — `InvalidGeometry` carries the failing `Type`, not the geometry reference: coercion leases dispose before a fault surfaces, so a live payload hands consumers a disposed native object and retains host memory inside accumulating `Validation` rails. `OutOfRange` carries an optional `Op` — `None` from keyless factory admission (`context.md`'s triad), re-keyed to the demanding operation by the `AcceptValidated` bridge (`validation.md`).
- Law: the two-family seam is an explicit decision — `Fault` is the kernel-substrate rail (`Expected`-derived records, direct `Error` subtyping, payloads addressable by pattern match); `Rasm.Numerics`'s `GeometryFault` band 2400 is the robust-core rail (ordinal-coded, `ToError()`-lowered). Two families, two altitudes, never merged: a substrate failure — missing context, invalid input, cancelled run, unsupported projection — is a `Fault` case; a robust-core geometry failure — degenerate offset, stalled skeleton, singular constraint system — is a `GeometryFault` case. Neither absorbs the other; a page composing both rails converts nothing — both are already `Error`.
- Growth: a new substrate failure is one `Fault` case with its typed payload and `Category`; a parallel error type, a bare `Error.New` in domain flow, or a case minted for a robust-core concern is the rejected form.
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
- Entry: `Use(project)` and the state-threaded `Use(state, project)` — the sole consumption gate: an `Owned` value is projected inside a `using` window and disposed the moment the projection returns; a `Borrowed` value is projected untouched. `Resource` reads the live value where the caller manages the extent; `Dispose()` releases `Owned` and no-ops `Borrowed`.
- Law: ownership is a case, never a flag — the coercion lattice (`Domain/normalization.md`) returns `Fin<Lease<Curve|Surface|Brep>>` deciding owned-versus-borrowed per recovery path, `Requirement`'s lease-aware checks (`validation.md`) thread it, and the projection carriers ride `Lease<GeometryBase>`; a raw `IDisposable` field, a scattered `using`, or a parallel owned/borrowed wrapper pair is the deleted form.
- Law: the state-threaded `Use` overload keeps projections closure-free — state rides the fold, lambdas stay `static`.
- Boundary: unmarked by declaration — resource cases are not operations, so `Lease<T>` never carries `[GenerateUnionOps]` and no `SelfOp` is emitted. The `using` statement inside `Owned.Project` is the named platform-forced disposal seam.

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

- Owner: `IValidityEvidence` — the corpus-wide evidence floor (one member, `IsValid`) every kernel receipt and carrier implements — plus `ValidityClaim`, the claim vocabulary whose `All` fold is the one mechanism a receipt's `IsValid` body composes.
- Entry: a receipt spells `public bool IsValid => ValidityClaim.All(...)` over claim rows; the implicit `bool` conversion makes the fold the body. Claim rows: `Of(bool)` · `Finite(double)` · `Finite(Point3d)` · `Finite(Vector3d)` · `Finite(ReadOnlySpan<double>)` · `Nonnegative(double)` · `Positive(double)` · `UnitInterval(double)` · `Ordered(lower, upper)` · `CountAtLeast(count, floor)` · `CountExactly(count, expected)` · `Evidence(IValidityEvidence?)` for nested receipts.
- Law: this fold retires the receipt-validity swarm — roughly forty mature receipts each hand-rolled an `IsValid` as a private `&&` chain re-deriving finiteness, non-negativity, count, and order semantics per receipt. The claim vocabulary states each predicate once; a receipt declares WHICH claims hold, never HOW a predicate is computed. A hand-rolled predicate chain in a receipt body is the deleted form.
- Law: predicate policy is named once, HERE — the scalar `Finite` is `RhinoMath.IsValidDouble`, which screens both non-finite values and the host `RhinoMath.UnsetValue` sentinel, because scalar fields on kernel receipts can carry host-read material; the span `Finite` is the vectorized `TensorPrimitives.IsFiniteAll` gate, correct for solver-produced arrays that never carry the host sentinel. `Admit` (`validation.md`) lifts these same claim rows into `Op`-keyed admission faults — one predicate statement serves both rails. A host-neutral-shaped receipt (`Numerics/*` pages) folds `Of(...)` claims over its owned `double.IsFinite` + epsilon policy — same mechanism, page-owned predicate.
- Law: implementing `IValidityEvidence` is oracle registration — `OpAcceptance.ValidityOf` (`validation.md`) reads one `IValidityEvidence` arm, so a new receipt reaches the acceptance oracle with zero oracle edits. `ClosestHit` (`evaluation.md`), `TopologyProjection` (`normalization.md`), `Stat`/`Distribution` (`stats.md`), the Analysis result receipts (`Analysis/*`), and every kernel receipt register through this floor; the mature per-type oracle arms and the `AnalysisAcceptance` fork are the deleted forms.
- Boundary: the fold is validity evidence, never admission — admission rejects raw material at the boundary with typed faults (`validation.md`); the fold answers whether an already-constructed receipt carries coherent evidence. The span loop inside `All` is the named kernel exemption.

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

The ONE Op-threading law. Every kernel page obeys it; no page re-decides it.

- Law: `Op` is an explicit VALUE — minted once at the public entry through `Op.Of()` caller-member-name or read off a union case's generated `SelfOp`, threaded as the trailing parameter of every fallible kernel (`Op key` required on internal kernels, `Op? key = null` resolved through `OrDefault()` on public polymorphic surfaces), and read by every fault factory. The key identifies the operation that failed; it is never runtime capability. Repeated `OrDefault()` inside ONE member is value-identical, never a split key: `Op` is a string-keyed `[ValueObject<string>]` and `[CallerMemberName]` resolves lexically to the enclosing member (lambdas included), so every resolution in that member mints the equal value — bind `Op op = key.OrDefault();` once for read clarity, but the law is value identity, not call count.
- Law: `Eff<Env>` is the runtime CARRIAGE — a pipeline needing tolerance context, progress, or cancellation is `Eff<Env, T>` composing `Env.Asks`/`Env.EnvAsks`; `Env` carries `Context`, `IProgress<double>?`, and `CancellationToken`, and nothing else rides it. The `Op` key never enters `Env`, and no ambient static, `AsyncLocal`, or second key mechanism exists anywhere in the kernel.
- Law: below the `Eff` floor, the synchronous rails thread `Context` and `CancellationToken` as explicit parameters (`Requirement.Apply(context, value, cancel)` is the canonical shape); at the floor and above, `Env` carries both. One operation is written in exactly one paradigm — a kernel is a `Fin`/`Validation` body with a key tail, or an `Eff<Env, T>` pipeline threading the same key as a value — never both, never a hybrid.
- Boundary: `Env` is `Analysis/query.md`'s frozen record — the Grasshopper binding constructs it directly; this page legislates the carriage law, that page owns the record and demonstrates the pipeline shape.

## [08]-[DENSITY_BAR]

One substrate floor; growth is a case, a claim row, or a generated `SelfOp` — never a sibling rail.

| [INDEX] | [CONCERN]          | [OWNER]                               | [KIND]                                                | [RAIL]                                      | [CASES] |
| :-----: | :----------------- | :------------------------------------ | :---------------------------------------------------- | :------------------------------------------ | :-----: |
|  [01]   | Operation identity | `Op`                                  | `[ValueObject<string>]` + fault/acceptance factory    | `Op → Error` / `Op → Fin<T>`                |   17    |
|  [02]   | Codegen contract   | `GenerateUnionOps`                    | opt-in marker attribute + generated per-case `SelfOp` | `[Union] case → Op`                         |    1    |
|  [03]   | Substrate faults   | `Expected` + `Fault`                  | unmarked `[Union]`, typed payloads, code 9104         | `Fault → Error` (direct subtype)            |   12    |
|  [04]   | Resource ownership | `Lease<T>`                            | unmarked `[Union]` Owned/Borrowed                     | `Lease<T>.Use → TResult`, disposal folded   |    2    |
|  [05]   | Receipt validity   | `IValidityEvidence` + `ValidityClaim` | evidence floor + claim fold, implicit `bool`          | `ValidityClaim.All → bool` → the one oracle |   13    |
