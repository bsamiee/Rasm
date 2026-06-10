# Admission Rails — Deepening

[GENERATED_SPINE_INTERNALS]:
- No factory path guards the user hooks: the generated `Validate` invokes `ValidateFactoryArguments(ref validationError, ref value)` bare, `TryCreate` is literally `validationError = Validate(...); return validationError is null;`, and `Create` only converts the returned fault to a throw — so a hook that itself throws detonates every admission surface including the nominally non-throwing `TryCreate`. The error channel is the only rail-safe failure path; a hook wrapping a throwing parser must absorb the exception internally and convert it to the fault family, because the bridge's no-throw assumption rests entirely on hook totality.
- `Create` and `TryCreate` hard-code `null` as the `IFormatProvider` argument when delegating to `Validate` — only a direct `Validate` call and the generated parse surface ever thread a real provider. Culture-sensitive normalization in hooks must treat a null provider as the invariant default; a rail bridge that enters through `Validate` is the only admission spelling that can carry culture at all.
- Complex owners emit one null guard per non-nullable reference member, ahead of the hook, in member declaration order, each returning immediately with a fault manufactured through the family's `Create(string)` — admission inside a single owner is short-circuit by construction, at both the null tier and the hook tier (one `ref` error slot). Accumulation has exactly two homes: inside the hook by folding into the family's aggregate case, or across owners at the bridge; the generated spine itself never accumulates. This is the structural argument for pushing admission down to leaf owners: independence between faults must be reified as separate owners before the applicative bridge can surface them together.
- The empty-string-as-absence knob deletes `[NotNullWhen(true)]` from both generated `TryCreate` overloads — under that knob `TryCreate(...) == true` no longer proves a non-null instance, so the boolean surface inherits the three-valued contract without any signature change; flow analysis goes quiet rather than wrong. The null-yields-null knob instead decorates `Create` with `[return: NotNullIfNotNull(...)]`, which keeps flow analysis exact. The two knobs thus differ not only in semantics but in how much of the contract survives in annotations — a bridge generated-contract audit reads the attributes, not the docs.
- When the key argument is itself named `provider`, the generator renames the format-provider parameter to `formatProvider`; named-argument call sites using `provider:` then bind the raw key. Bridges pass positionally and never notice; reflective or named-argument callers can silently swap roles.

[HOOK_STATE_CHANNEL]:
- Declaring the factory hook with a non-void return type rewires the generated spine: `Validate` captures the hook's return value and forwards it to `FactoryPostInit(<T> value)`, whose generated partial declaration gains a parameter of exactly that type. The result is a typed state channel from admission computation to post-construction initialization — parse once in the hook, consume the parsed artifact after the instance exists, with zero re-computation and no static side channel:

```csharp
[ValueObject<string>]
[ValidationError<Fault>]
public sealed partial class Locator
{
    private Uri _resolved = null!;

    private static partial Uri? ValidateFactoryArguments(ref Fault? validationError, ref string value)
    {
        value = value.Trim();
        var ok = Uri.TryCreate(value, UriKind.Absolute, out var parsed);
        validationError = ok ? null : Fault.Create($"not an absolute locator: '{value}'");
        return parsed;
    }

    partial void FactoryPostInit(Uri? resolved) => _resolved = resolved!;
}
```

- The non-void hook flips to `private static partial` in the generated defining declaration, so the implementing half must also be private — the channel is invisible outside the owner by construction.
- The channel runs on every factory path (factories, conversion operators, framework deserialization routed through the factory), so derived state initialized this way holds for wire-admitted instances too — the one population it cannot reach is trusted-rehydration construction, which bypasses `Validate` entirely and must derive the state in the corresponding constructor.

[FACTORY_CONTRACT_SURFACE]:
- The factory contract declares its raw-value parameter as `TValue : notnull, allows ref struct` — the admission interface itself admits byref-like raw types. A generic rail bridge whose raw-type parameter omits `allows ref struct` silently narrows the contract and excludes every span-keyed admission overload from the polymorphic path; the anti-constraint must be mirrored on the bridge's type parameter. The raw value is consumed inside the call, so the rail carrier never needs to hold the span — only the parameter position is byref-like.
- Declaring an additional admission source generates only wiring: the owner-marker interface, the typed factory interface, conditionally the outbound-conversion interface, and one runtime metadata row (value type, validation-error type, framework flags, optional constructor-conversion expression) — the `Validate` body is compelled by the static abstract and authored by the owner. The admission grammar is therefore always owner code under compile-time obligation, and the owner kind is unrestricted: keyed and keyless enumerations, simple and complex value objects, ad-hoc unions, and regular union roots all take the attribute.
- A regular union root carrying the factory attribute becomes a single admission seam for its whole closed family — the user `Validate` discriminates the grammar and constructs the case, the same generic bridge projects the outcome, and case selection collapses into admission instead of trailing it:

```csharp
[Union]
[ObjectFactory<string>]
[ValidationError<Fault>]
public abstract partial record Source
{
    public sealed record ByPath(string Path) : Source;
    public sealed record ByUrn(string Urn) : Source;

    public static Fault? Validate(string? value, IFormatProvider? provider, out Source? item)
    {
        item = value switch
        {
            ['/', ..]            => new ByPath(value),
            ['u', 'r', 'n', ':', ..] => new ByUrn(value),
            _                    => null,
        };
        return item is null ? Fault.Create($"unrecognized source grammar: '{value}'") : null;
    }
}
```

- Two-way conversion is not opt-in by method but by consequence: any serialization framework flag or persistence flag on the factory attribute forces the outbound interface, whose `ToValue()` the owner must implement — the round-trip law (admit after project is identity modulo normalization) becomes a compile-visible obligation the moment the owner faces a wire.
- The trusted-rehydration flag declares a constructor accepting the foreign value, letting materialization from already-admitted storage skip `Validate` entirely; it is rejected on enumerations (their identity map cannot be bypassed). This is the single sanctioned no-validation path, and its scope is precisely data that passed admission before persistence — using it for any other ingress reintroduces the unvalidated interior the whole architecture exists to prevent.
- Multiple factory attributes on one owner are analyzer-partitioned: overlapping serialization frameworks, duplicate model-binding claims, and duplicate persistence claims across attributes are compile errors — each framework concern resolves to exactly one admission grammar, so wire routing is total and unambiguous at definition time.

[RAIL_CONSTRAINT_TOPOLOGY]:
- The accumulating carrier's failure-combination constraint lives in different places per spelling, and the spellings disagree on enforcement. The named combinators (`Apply`, `Action`, `Combine`) and the K-typed operator overloads carry a compile-time semigroup constraint and read the trait's static instance directly. The operators defined on the concrete carrier (`&` collecting successes into a sequence) and the trait-routed applicative path instead resolve an optional runtime instance by reflecting for the trait's static property on the constructed trait type — an exact-type lookup where an instance inherited from a base failure type is invisible.
- The degradation map on instance miss is asymmetric and entirely silent on the hot paths: the applicative apply/action fallback keeps only the first failure; the success-collecting combination keeps the first failure; the alternative-combination fallback keeps the last failure; only the identity operations (alternative empty, monoid-unit contexts) fail loud, with an unsupported-operation throw naming the missing monoid. Carrier documentation claims a type-load throw for incompatible failure types; the implementation substitutes option-fallbacks instead — the documented failure mode and the real one diverge, so the only reliable contract is to give the fault family its trait instances or use the error base as the failure currency.
- Which overload binds is decided by the static type of the operands: values typed as the concrete carrier bind the runtime-resolved operators, values typed as the trait-kinded abstraction bind the compile-constrained ones. The same expression text can be constraint-checked or constraint-erased depending on whether an intermediate variable was declared as the concrete type — a reason to let the bridge return the concrete carrier and keep composition in one expression.
- The unary `+` operator on the trait-kinded carrier is the downcast to the concrete type — operator chains stay concrete without `.As()` ceremony; `>>` sequences dependent admissions and a `>>` with a lowering marker closes a trait-typed pipeline back to the concrete carrier.
- The public construction surface (`Success`, `Fail`, `Empty`) demands the full monoid at compile time, and the sequence-accepting `Fail` folds a precomputed fault batch through the monoid before wrapping — a batch of faults collected out-of-band enters the rail as one combined failure, not as a sequence bolted beside the carrier.

[FALLBACK_GRAMMARS]:
- Alternative admission grammars have two composition spellings with different evidence semantics, both first-success: choice (`|`) returns the first grammar's failure untouched when all grammars reject; semigroup combination returns the failures of every grammar combined. The choice spelling reads as "the canonical grammar's rejection is the diagnosis"; the combining spelling reads as "the value matched no grammar, show all rejections" — a deliberate decision per seam, not a style default:

```csharp
Validation<Error, Endpoint> first = Endpoint.Admit(text) | Endpoint.AdmitCompact(text);
Validation<Error, Endpoint> total = Endpoint.Admit(text).Combine(Endpoint.AdmitCompact(text));
```

- The carrier implements the predicate-gated catch trait, so the effect-world recovery vocabulary composes directly on admission results: a catch value on the right of `|` substitutes a recovery admission only for matching faults, with non-matching faults passing through unchanged — typed-fault triage without leaving the rail or writing a match.
- `BindFail` substitutes an entire recovery admission for the failure side and may simultaneously re-type the failure (the target failure type carries its own monoid obligation); `MapFail` re-codes faults in place, and its most common bridge use is lifting a typed family into the error-base currency once, immediately before joining streams that accumulate heterogeneous faults.

[EVIDENCE_FREE_FAILURES]:
- `Filter` and `Where` on the accumulating carrier manufacture the failure monoid's identity as the rejection value — for the error base that is the empty aggregate: a failure with zero members, count zero, empty flag set, expectedness vacuously true, and a fixed aggregate code. A `where` clause inside an admission comprehension therefore converts predicate failure into evidence-free rejection that no code-keyed, type-keyed, or membership-keyed recovery will ever match; cross-field refinement must be spelled as a guard with an explicit fault, and the lazy-thunk guard overload defers fault construction to the rejection path.
- The empty aggregate doubles as the miss-marker of typed-fault extraction: filtering an error for a fault subtype returns it when nothing matches. Triage that counts faults or folds over members must gate on emptiness first — an empty aggregate is still a failure, and treating "no matching faults" as "no failure" inverts the rail.
- The bottom error exists precisely for value-less evaluation states and its diagnostic text names uninitialized structs and filtered-out expressions as causes — the rail has a designated citizen for "no evidence" failures, and admission code should never mint a second one.

[FAULT_CODE_ALGEBRA]:
- Catch-style matching on the error base is two-mode: a non-zero code makes `Is` compare codes and ignore messages (localization-stable matching by design); a zero code degrades `Is` to message-string equality. A fault family that leaves the code at its default has opted into string-matching recovery without noticing — every structured case claims a non-zero code, and the string-bearing generator-text case is the only tolerable zero-code member.
- The runtime reserves a contiguous negative code band for control-signal errors — bottom, cancellation, timeout, empty-sequence, closed, parse, the aggregate marker, lifting-unsupported, end-of-stream, validation-failed, source/sink states — and two distinct reserved errors share one code within it. Domain fault codes must be allocated outside this band: code-keyed recovery is equality on integers, and a collision makes a domain fault impersonate a cancellation or timeout to every generic handler upstream.
- Exception-captured faults take the exception's `HResult` as their code — the code space of exceptional errors is the host's HRESULT space, not the domain's. Code-keyed recovery distinguishes the two populations by expectedness first; a numeric code only means what its side of the expected/exceptional split says it means.
- Aggregates recurse every membership predicate (`Is`, code possession, type possession, exception possession) over their members but expose their own fixed code as a property — an aggregate never satisfies code-possession for its own aggregate code unless a member carries it. Recovery must never key on the aggregate marker code; it keys on member evidence reached through the recursive predicates or through typed extraction.

[EXCEPTION_CHANNEL_FIDELITY]:
- Typed faults round-trip exception-only channels with reference fidelity: throwing an expected fault produces a wrapper exception that holds the fault instance, and the capture constructor on the error base unwraps any of the wrapper exceptions back to the original object — a derived fault record crosses a boundary that only speaks throw/catch and arrives as the same case, same payload, no serialization. The catch-side spelling is always the capture constructor, never direct exceptional construction, because only the former performs the unwrap.
- The same capture constructor is a normalization tree: cancellation exceptions (both task and operation flavors) collapse to the reserved cancellation error, timeouts to the reserved timeout, aggregate exceptions flatten into the many-errors carrier, and only the remainder becomes exceptional. Boundary capture therefore auto-classifies host control-flow exceptions into the reserved vocabulary — catch-side code that re-wraps a cancellation as a domain fault is fighting the rail's own taxonomy.
- The exception mirror of the error algebra is itself a monoid (combinable wrappers, a many-exceptions carrier, an empty), and every wrapper writes the fault code into the exception's `HResult` — foreign frames that know nothing of the rail can still key on the domain code through the standard exception surface.
- Exceptional errors deliberately drop their captured exception on deserialization (cross-process leak prevention): stack fidelity is process-local, and the in-process throw path preserves the original stack via dispatch-info capture while the rehydrated path throws a synthetic carrier. Fault payloads that must survive process hops carry their evidence in the expected-fault record, never in the wrapped exception.
- For admission surfaces not under generated control — foreign factories that throw — the lazy capture monad is the one-hop projection: a record around a result-thunk whose lift wraps the computation and whose run double-guards it, composing lazily so the foreign parse executes only at the terminal run, with the capture constructor's normalization applied to whatever escapes. Its combination operator is itself accumulating on failure, giving foreign-grammar fallback the same evidence semantics as native accumulation. Generated owners need none of this; the capture monad exists exactly for the admission code you cannot make total.

[DEFAULT_STRUCT_GHOSTS]:
- Struct owners that deny default values receive a marker interface from the generator, but enforcement is analyzer-only and syntactic: explicit `default` expressions and parameterless construction are compile errors, while generic type parameters, array allocation, and uninitialized fields manufacture zero-ghosts the analyzer cannot see — and the generator emits no runtime initialization check anywhere in the construction or access paths. A ghost is a struct whose key member is the key type's default, never observed by any hook.
- The runtime gate therefore belongs to the seam that receives possibly-defaulted storage: infrastructure detecting the marker interface inspects the key member directly (null for reference keys, zero-pattern for value keys) and rejects with a typed fault before the ghost crosses inward. Reading the key beats calling equality against a default instance, because generated equality routes through key comparers whose null handling is comparer-specific.
- Deserializers and materializers that construct structs without factories are the dominant ghost source; routing them through the declared admission source type (factory-based wire conversion) closes the hole at definition time, which is the stronger fix — the runtime gate is for storage shapes that cannot be rerouted.

[TWO_PHASE_COMPOSITION]:
- Layered admission has a canonical two-phase shape: leaf owners admit raw fields applicatively (independent, all faults surface), and the composite owner — whose generated member-wise `Validate` accepts already-admitted leaf types — binds monadically on top, because composite admission is data-dependent on leaf success and can only fail on cross-member invariants. The applicative stage yields a nested carrier; flattening is the join between phases, and a guard carries any post-construction refinement:

```csharp
public static Validation<Error, Window> Admit(RawWindow raw) =>
    from window in (fun((Lo lo, Hi hi) => Window.Admit(lo, hi)) * Lo.Admit(raw.Lo) * Hi.Admit(raw.Hi)).Flatten()
    from _ in guard(window.Span > 0, () => (Error)Fault.Create($"degenerate window: {window}"))
    select window;
```

- The carrier lattice is bidirectional and lossless in both directions because the error base is the monoid: collapsing the accumulator to the sequential carrier packs the accumulated aggregate into the single error slot intact (flat member stream, counts, typed extraction all survive), and the sequential carrier lifts back to the accumulator with the aggregate re-exposed. Direction is therefore a statement about where accumulation continues, not about evidence retention: collapse at the frontier where flow becomes sequential, lift back where a later seam resumes parallel admission.
- The sequential carrier's terminal boundary spelling is the throw-if-failed projection, which rethrows through dispatch-info capture — original stack for exceptional faults, wrapper exception with code-bearing `HResult` for expected ones — making the outermost host boundary a one-liner that still preserves the full fidelity story.
- Comprehension syntax over the accumulating carrier supports guards natively in both positions — guard-first (precondition before any admission) and guard-after (refinement over admitted values) — via dedicated bind support on the guard shape, so refinement never forces a detour through the sequential carrier and back.
