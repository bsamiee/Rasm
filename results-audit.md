# `Domain/results.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/results.md`

Apply the moves in source order. Counts are authored, nonblank C# fence lines exactly as shown. The ordered result removes 16 fenced LOC and four hand-authored members without adding a helper, declared type, enum, compatibility surface, stored mirror, or consumer migration.

API authority used: the shared `libs/dotnet/.api/api-languageext.md` and `api-thinktecture-runtime-extensions.md` catalogues, plus the complete package-local `libs/dotnet/Rasm/.api/` tier. The package-local tier contributes no additional result-carrier API used by these moves. `docs/stacks/csharp/shapes.md` supplies the regular-union universal-column law; `language.md` and `results-and-effects.md` supply the explicit-type, helper-collapse, carrier, and atomic-cell laws. No external lookup is needed.

## 1. Make `Capture` the sole ordinary-exception normalizer

This is one owner collapse expressed as four small edits. `Capture` takes over exception-to-`Error` admission, both ordinary `Catch` overloads use it, the typed classifier refines its result, and the displaced private helper disappears.

### 1A. Put ordinary capture in its public owner

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:84-87`, anchor `public Error Capture(Exception raised`

**From:**

```csharp
public Error Capture(Exception raised, CancellationToken token = default) {
    ArgumentNullException.ThrowIfNull(raised);
    return Captured(raised: raised, token: token);
}
```

**To:**

```csharp
public Error Capture(Exception raised, CancellationToken token = default) {
    ArgumentNullException.ThrowIfNull(raised);
    Error captured = Error.New(raised.Message, raised);
    return raised is OperationCanceledException && token.IsCancellationRequested
        ? new KernelFault.Cancelled(Cause: captured)
        : captured;
}
```

### 1B. Route both ordinary catch arms through `Capture`

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:91` and `:102`, anchors `catch (Exception raised)` in the synchronous and asynchronous ordinary `Catch<T>` overloads

**From:**

```csharp
catch (Exception raised) { return Fin.Fail<T>(Captured(raised: raised, token: token)); }
```

**To:**

```csharp
catch (Exception raised) { return Fin.Fail<T>(Capture(raised: raised, token: token)); }
```

Apply the replacement at both anchored occurrences.

### 1C. Make typed classification refine an already captured error

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:77-83`, anchor `private static Error Classify<TFault>`

**From:**

```csharp
private static Error Classify<TFault>(Exception raised, CancellationToken token, Func<Error, Option<TFault>> provider)
    where TFault : Fault, ICausedFault {
    Error captured = Error.New(raised.Message, raised);
    return raised is OperationCanceledException && token.IsCancellationRequested
        ? new KernelFault.Cancelled(Cause: captured)
        : provider(captured).Map(static fault => (Error)fault).IfNone(captured);
}
```

**To:**

```csharp
private Error Classify<TFault>(Exception raised, CancellationToken token, Func<Error, Option<TFault>> provider)
    where TFault : Fault, ICausedFault {
    Error captured = Capture(raised: raised, token: token);
    return captured is KernelFault.Cancelled ? captured
        : provider(captured).Map(static fault => (Error)fault).IfNone(captured);
}
```

### 1D. Delete the displaced private helper

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:71-76`, anchor `private static Error Captured`

**From:**

```csharp
private static Error Captured(Exception raised, CancellationToken token) {
    Error captured = Error.New(raised.Message, raised);
    return raised is OperationCanceledException && token.IsCancellationRequested
        ? new KernelFault.Cancelled(Cause: captured)
        : captured;
}
```

**To:**

```csharp
```

**Effect:** fenced LOC `19 -> 15` (`-4`); hand-authored members `-1 private` (`Op.Captured`); module-level types and public members `0`.

**API/consumer proof:** the shared LanguageExt catalogue owns `Error.New(string, Exception)` and `Option.Map`/`IfNone`. `Capture` is already the public exception-valued boundary, and `Interaction/input.md:573` calls `op.Capture(raised)`. Both typed `Catch` overloads already call `Classify`; both ordinary overloads contain the two anchored `Captured` calls. The revised path still creates one exceptional error, recognizes cancellation only when the supplied token proves it, never invokes a typed provider for that cancellation, and otherwise preserves the provider's optional refinement. A caught `Exception` cannot trip `Capture`'s null guard.

**Ripples:** none outside the four same-file edits. Public signatures, fault shapes, and the section prose naming `Capture` remain unchanged.

## 2. Collapse the void-host adapter without an `Option` stage

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:122-125`, anchor `public Fin<Unit> Catch(Action body)`

**From:**

```csharp
public Fin<Unit> Catch(Action body) {
    Op self = this;
    return Optional(body).ToFin(Fail: self.InvalidInput()).Bind(valid => self.Catch(() => Fin.Succ(value: Side(action: valid))));
}
```

**To:**

```csharp
public Fin<Unit> Catch(Action body) =>
    body is null ? Fin.Fail<Unit>(error: InvalidInput()) : Catch(() => Fin.Succ(value: Side(action: body)));
```

**Effect:** fenced LOC `4 -> 2` (`-2`); declared symbols `0`; the pre-funnel `Optional -> Fin -> Bind` stages collapse to one null branch.

**API/consumer proof:** the null arm returns the same `KernelFault.InvalidInput`. The admitted lambda returns `Fin<Unit>`, so overload resolution selects `Catch<T>(Func<Fin<T>>)` rather than recursing into the `Action` overload. `Side` still executes inside the exception funnel; a thrown body reaches the same `Capture` path and success remains `Fin<Unit>`.

**Ripples:** none; every consumer keeps the same public overload and result semantics.

## 3. Use the carrier's direct optional lookup

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:344-345`, anchor `public static Option<FaultBand> OwnerOf`

**From:**

```csharp
public static Option<FaultBand> OwnerOf(BandKind kind, int code) =>
    toSeq(Items).Filter(band => band.Kind == kind && code >= band.Key && code < band.Key + band.Span).Head;
```

**To:**

```csharp
public static Option<FaultBand> OwnerOf(BandKind kind, int code) => toSeq(Items).Find(band => band.Kind == kind && code >= band.Key && code < band.Key + band.Span);
```

**Effect:** fenced LOC `2 -> 1` (`-1`); declared symbols `0`; the `Filter(...).Head` pipeline becomes the direct optional-search operator.

**API/consumer proof:** `libs/dotnet/.api/api-languageext.md` declares `FoldableExtensions.Find(Func<A,bool>) -> Option<A>`. It returns the same first matching band as `Filter(predicate).Head`. `FaultExtensions.Owner`, telemetry, port, and event-audit consumers retain the exact `Option<FaultBand>` result; `BandKind` continues partitioning the overlapping fault and event code spaces as required by `libs/dotnet/.planning/RULINGS.md`.

**Ripples:** none.

## 4. Express `Fault.Inner` as its single type test

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:402-405`, anchor `public sealed override Option<Error> Inner`

**From:**

```csharp
public sealed override Option<Error> Inner => this switch {
    ICausedFault caused => Some(caused.Cause),
    _ => None,
};
```

**To:**

```csharp
public sealed override Option<Error> Inner => this is ICausedFault caused ? Some(caused.Cause) : None;
```

**Effect:** fenced LOC `4 -> 1` (`-3`); declared symbols `0`.

**API/consumer proof:** the original switch has one meaningful pattern and one fallback. The conditional preserves the same `ICausedFault.Cause`, the same `Some(Error)`, and the same absent `None`; no union dispatch or open-family routing is being replaced.

**Ripples:** none.

## 5. Inline the two single-use owned projections into `Lease.Use`

The two `Owned.Project` overloads only place one lexical `using` around one corresponding `Lease.Use` arm. Put that boundary at each sole caller and return `Owned` to a payload-only case.

### 5A. Put disposal in the two projection call sites

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:588-590`, anchors `public TResult Use<TResult>` and `public TResult Use<TState, TResult>`

**From:**

```csharp
public TResult Use<TResult>(Func<T, TResult> project) => Switch(state: project, owned: static (use, owned) => owned.Project(project: use), borrowed: static (use, borrowed) => use(arg: borrowed.Value));
public TResult Use<TState, TResult>(TState state, Func<TState, T, TResult> project) =>
    Switch(state: (State: state, Project: project), owned: static (use, owned) => owned.Project(state: use.State, project: use.Project), borrowed: static (use, borrowed) => use.Project(arg1: use.State, arg2: borrowed.Value));
```

**To:**

```csharp
public TResult Use<TResult>(Func<T, TResult> project) => Switch(state: project, owned: static (use, owned) => { using T resource = owned.Value; return use(resource); }, borrowed: static (use, borrowed) => use(borrowed.Value));
public TResult Use<TState, TResult>(TState state, Func<TState, T, TResult> project) =>
    Switch(state: (State: state, Project: project), owned: static (use, owned) => { using T resource = owned.Value; return use.Project(use.State, resource); }, borrowed: static (use, borrowed) => use.Project(use.State, borrowed.Value));
```

### 5B. Remove the exhausted nested member surface

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:576-579`, anchor `public sealed record Owned(T Value)`

**From:**

```csharp
public sealed record Owned(T Value) : Lease<T> {
    internal TResult Project<TResult>(Func<T, TResult> project) { using T owned = Value; return project(arg: owned); }
    internal TResult Project<TState, TResult>(TState state, Func<TState, T, TResult> project) { using T owned = Value; return project(arg1: state, arg2: owned); }
}
```

**To:**

```csharp
public sealed record Owned(T Value) : Lease<T>;
```

**Effect:** fenced LOC `7 -> 4` (`-3`); hand-authored members `-2 internal` (both `Lease<T>.Owned.Project` overloads); module-level types and public members `0`.

**API/consumer proof:** Thinktecture's generated stateful `Switch` passes the delegate as state, keeping both arms closure-free. In each owned arm, `using T resource` preserves disposal after a normal return or a throw; borrowed arms remain non-disposing. Repository-wide search finds no code consumer of `Owned.Project`; the two methods have exactly the two anchored same-file calls and contain no other behavior.

**Ripples:** one same-target prose repair at current line 566: replace “`Owned.Project`'s `using`” with “the owned arms of `Lease<T>.Use` hold the `using` boundary.” No external code or prose changes.

## 6. Compose the general optional-claim owner

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:824-827`, anchors `public static ValidityClaim Evidence<T>` and `WhenPresent<T>`

**From:**

```csharp
public static ValidityClaim Evidence<T>(Option<T> evidence) where T : IValidityEvidence =>
    new(Holds: evidence.Map(static value => value.IsValid).IfNone(noneValue: true));
public static ValidityClaim WhenPresent<T>(Option<T> facet, Func<T, ValidityClaim> claim) =>
    new(Holds: facet.Map(claim).IfNone(noneValue: true));
```

**To:**

```csharp
public static ValidityClaim Evidence<T>(Option<T> evidence) where T : IValidityEvidence => WhenPresent(evidence, static value => value.IsValid);
public static ValidityClaim WhenPresent<T>(Option<T> facet, Func<T, ValidityClaim> claim) => facet.Map(claim).IfNone(true);
```

**Effect:** fenced LOC `4 -> 2` (`-2`); declared symbols `0`; duplicate optional folds `2 -> 1`.

**API/consumer proof:** `Evidence` is exactly the `IValidityEvidence.IsValid` specialization of `WhenPresent`. The declared implicit `bool -> ValidityClaim` conversion admits both the expression-lambda result and the absent `true`; LanguageExt `Option.Map`/`IfNone` returns the required carrier. Absence stays non-falsifying. Both public methods have broad live use, so composition preserves rather than deletes their ergonomic surface.

**Ripples:** none.

## 7. Seat lazy converters and inline the single-use mint

`Cell.Claim` deliberately evaluates its supplied `mint` before entering the CAS. A raw converter candidate therefore runs reflection on every cache hit and on every losing first claimant, contradicting this section's once-per-closed-type law. Cache a lazy candidate so only the instance selected from the settled map runs reflection, and absorb the now-single-use `Mint` body.

**Location:** `libs/dotnet/Rasm/.planning/Domain/results.md:894-905`, anchors `static readonly Atom<HashMap<Type, JsonConverter>> Minted`, `public override JsonConverter CreateConverter`, and `static JsonConverter Mint(Type type)`

**From:**

```csharp
static readonly Atom<HashMap<Type, JsonConverter>> Minted = Atom(HashMap<Type, JsonConverter>());

public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) =>
    Cell.Claim(cell: Minted, key: type, mint: () => Mint(type: type)).Current[type];

static JsonConverter Mint(Type type) {
    CarrierRow row = Carriers[type.GetGenericTypeDefinition()];
    return (JsonConverter)Activator.CreateInstance(type: row.Converter.MakeGenericType(row.Close(type)))!;
}
```

**To:**

```csharp
static readonly Atom<HashMap<Type, Lazy<JsonConverter>>> Minted = Atom(HashMap<Type, Lazy<JsonConverter>>());

public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) =>
    Cell.Claim(cell: Minted, key: type, mint: () => new(valueFactory: () => {
        CarrierRow row = Carriers[type.GetGenericTypeDefinition()];
        return (JsonConverter)Activator.CreateInstance(type: row.Converter.MakeGenericType(row.Close(type)))!;
    }, mode: LazyThreadSafetyMode.ExecutionAndPublication)).Current[type].Value;
```

**Effect:** fenced LOC `7 -> 6` (`-1`); hand-authored members `-1 private` (`LanguageExtJsonConverterFactory.Mint`); reflection-created converter candidates fall from once per `CreateConverter` call to once for the seated closed type.

**API/consumer proof:** the shared LanguageExt catalogue states that `Atom.Swap` is a CAS loop and can re-run its callback, while the target's `Cell.Claim` visibly evaluates `TValue candidate = mint()` once before that loop. With `Lazy<JsonConverter>`, each call can still allocate a cheap losing wrapper, but only the wrapper read from the post-swap map is evaluated. `ExecutionAndPublication` makes the seated wrapper's factory single-run. `Mint` has one caller and reads only the captured `type`. The factory's public `CanConvert`/`CreateConverter` contract and every AppHost, Compute, Fabrication, Persistence, and Domain-event registration remain unchanged.

**Ripples:** none outside this same-file block. The section law at current line 864 becomes true without a separate `Mint` member.

## Deliberate non-moves

- Keep `BandKind`, `FaultBand`, and `FaultId` intact. `BandKind` partitions overlapping event and fault spaces, and the branch ruling requires `FaultBand.OwnerOf` to retain that coordinate.
- Keep `KernelFault.InvalidValue` and `OutOfRange` distinct. Their payloads and fault identities carry different evidence and are consumed across the branch; merging them is semantic loss plus a broad constructor migration, not a type-count win.
- Keep `Retriability.Key` derived by the generated total `Switch`. The three literals are a secondary correspondence over the union case, not independent payload every instance should store; replacing the projection with an auto-property saves three source lines by adding a backing field and a second stored discriminant, contrary to the derivation law and the rule that a fault family carries no category mirror.
- Keep `Redrive.Merge`, `RedrivePolicy.Exhausted`, and `RedrivePolicy.Next`. Each names a live algebra boundary; inlining `Merge` nests the recursive aggregate fold without reducing logic, while both policy projections have external consumers.
- Keep the token-returning and tokenless `Cell.Seat` implementations separate. Delegating the token form through the tokenless overload removes lines but adds another capturing delegate on the CAS path; the existing forms each mint once and enter one swap directly.
- Keep `Transition<TState>.Current` as its generated dispatch projection. Moving `Current` onto the root while preserving every leaf's public `State` duplicates arbitrary `TState` storage in every result; deleting leaf storage instead forces a branch-wide consumer and constructor migration, while naming both root and positional payload `State` risks the exact CS8907 suppression documented by the C# shape law. None is a surgical reduction.
- Do not mix the `Cell.Converge` split-read correction into this reduction ledger. Capturing the initial `cell.Value` once is a legitimate concurrency correction but adds a fenced line; it should land as a separately authorized correctness repair, not be hidden inside LOC arithmetic.
- Keep the span-based `Custody.Released` loop. Routing it through the `Seq` overload copies the span and constructs a carrier only to recover the current reverse, all-attempted semantics.
- Keep `CarrierBuild<TCarrier, T>`. `ReadOnlySpan<T>` requires the custom delegate for the reflection-created collection builder. Nesting it or the converter classes only relocates declarations and yields no LOC reduction.
- Keep the public `ValidityClaim.Evidence` and `WhenPresent` names. Both have broad consumers and express distinct call-site intent; move 6 shares their implementation without deleting useful surface.
- Keep `ValidityClaim` as the bool-backed claim value. A Thinktecture value object adds generated factory, parsing, comparison, and codec surface; collapsing it to raw `bool` weakens `Op.Demand` and the optional-claim delegates while forcing removal of `.Holds` at 156 live consumer lines for only three hand-authored member deletions. Neither trade is surgical or surface-cheaper after ripples.
