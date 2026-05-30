---
name: coding-csharp
description: >-
  Enforces C# 14 + LanguageExt v5 functional/ROP style for the Rasm
  Rhino/Grasshopper plugin. Use when writing, editing, reviewing, or refactoring
  .cs modules. Drives polymorphic collapse (Thinktecture [Union]/[SmartEnum]/[ValueObject]),
  greenfield API breaking over shims, dense algorithmic logic over flat structures,
  singular OOP boundary capsules, unified Fin/Validation/Eff rails, and matching the
  established RasmCommand/RhinoUi/RasmOverlay templates. Activates on concept-density
  pressure points: 3 or more parallel types, 3 or more sibling factories, 3 or more
  repeated switch arms, 3 or more single-call private helpers.
---

# [H1][CODING-CSHARP]
>**Dictum:** *C# + LanguageExt style, type discipline, and module organization govern all C# work.*

All code follows six governing principles:
- **Polymorphic** — one entrypoint per concern, generic over specific, extend over duplicate
- **Functional + ROP** — pure pipelines, typed error rails, monadic composition
- **Strongly typed** — explicit types everywhere, one canonical shape per concept, zero `var`
- **Programmatic** — variable-driven dispatch, named parameters, bounded vocabularies
- **Algorithmic** — reduce branching through transforms, folds, and discriminant-driven projection
- **AOP-driven** — cross-cutting concerns via decorator composition, not in-method duplication


## Paradigm

- **Immutability**: `readonly record struct` for values, `with`-expressions for transitions, `Atom<T>` for managed state
- **Typed error channels**: sealed DU error hierarchies for file-internal errors (never exported), shared `Error` subtypes for domain-level boundary-crossing errors; `Fin<T>` sync, `Validation<Error,T>` parallel, `Eff<RT,T>` effectful
- **Exhaustive dispatch**: `switch` expressions on sealed DU hierarchies, Thinktecture `Switch`/`Map` for generated dispatch
- **Type anchoring**: `readonly record struct` + `Fin<T>` for primitives, `sealed abstract record` for DUs, `[ValueObject<T>]` for boundary wrappers — derive projections, never parallel types
- **Expression control flow**: LINQ comprehension (`from..in..select`), `Bind`/`Map`/`BiMap`, zero statement branching
- **Programmatic logic**: named parameters at domain call sites, `SmartEnum<T>` over strings, bounded vocabularies
- **Surface ownership**: one polymorphic entrypoint, `params ReadOnlySpan<T>` for arity collapse, no helpers
- **Private integration**: module logic is the export's implementation, not its neighbor — `private`/`internal` members are nested types, closures, or inline compositions inside the public class/service, not standalone file-level declarations consumed by a single caller
- **Cross-cutting composition**: Decorator composition for service-level AOP (host-supplied DI when present), `K<F,A>` for higher-kinded abstraction


## Architecture trinity

Every module exhibits three layers — singular boundary, unified rails, dense FP internals.

1. **Singular OOP boundary** — exactly one sealed boundary capsule per native-OOP integration point. The override surface is `sealed`; the only virtual surface returns `Fin<T>` (or `Eff<RT,T>`). Native disposables live inside `try…finally` blocks annotated `// BOUNDARY ADAPTER — reason`. Examples in this codebase: `RasmCommand<TSelf>.RunCommand` (Commands/Command.cs), `RhinoUi.Use<T>(UiIntent<T>)` (UI/Ui.cs), `RasmOverlay<TState>` (UI/Overlay.cs).
2. **Unified rails** — one rail per failure semantics within a file: `Fin<T>` (sync fallible), `Validation<Error,T>` (parallel accumulation), `Eff<RT,T>` (effectful), `IO<A>` (boundary effects). No mixing within the file. No raw `Task<T>` in domain. No `null` for absence. No `Exception` for control flow.
3. **FP internal logic** — dense expression bodies, exhaustive `switch` on `[Union]`/`SmartEnum`, LINQ comprehensions (`from..in..select`) for multi-step monadic composition, `Atom<TState>` for managed state, host decision monoids (`OverlayDecision`, GH `Components.Decision`), polymorphic dispatch tables, fold algebras over recursive structures. Prefer one deep complex surface over many shallow loose ones.


## Greenfield posture

Treat every refactor and every file as greenfield. No shims, no adapters, no legacy aliases, no `[Obsolete]` wrappers, no migration helpers, no `internal static T Adapt(LegacyShape)` indirections. Aggressive API breaking is the default when it produces:
- A denser polymorphic surface
- A unified rail where two parallel rails existed
- A boundary capsule where scattered OOP escaped containment
- Higher algorithmic density via `[Union]`/`SmartEnum`/fold algebra

When the analyzer rejects, treat the rejection as architectural pressure, not as a target to suppress. Search for the collapse before considering an exemption attribute.


## Conventions

| Layer               | Library      | Owns                                                                                   |
| ------------------- | ------------ | -------------------------------------------------------------------------------------- |
| ROP / Effects       | LanguageExt  | `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `IO<A>`, `K<F,A>`, `Option<T>`, `Seq<T>` |
| Value objects / DUs | Thinktecture | `[ValueObject<T>]`, `[SmartEnum<T>]`, `[Union]`, generated dispatch                    |
| Numerics / Symbolics | MathNet | Linear algebra, solvers, statistics, and symbolic formulas after Rhino/GH ownership is settled |
| Scheduling          | LanguageExt  | `Schedule.exponential` / `spaced` / `jitter` / `recurs` / `upto` algebra               |
| State               | LanguageExt  | `Atom<T>` validators, `Ref<T>` + `atomic` STM, `Bracket` resource scope                |

- One library's error/option type per module — no mixing across libraries in same file.
- Resilience is composition-root concern via verified LanguageExt recovery combinators + `Schedule` — never a domain primitive (no retry-as-policy library).
- DI is host-supplied; this codebase favours runtime records over service-locator scans.


## Contracts

**Type discipline**
- Zero `var` — all types explicit in declarations and lambda parameters.
- Named parameters at every domain call site. Framework/LINQ single-arg lambdas may use positional.
- `readonly record struct` for domain primitives with `Fin<T>` smart constructors.
- `sealed abstract record` base + `sealed record` cases for DUs. Static factories on abstract base.
- One canonical type per concept; derive projections, never parallel types.
- File-scoped namespaces only. Explicit accessibility on every member.

**Control flow**
- Zero `if`/`else`/`while`/`for`/`foreach`/`try`/`catch`/`throw` in domain transforms.
- `switch` expressions with exhaustive arms on sealed DU hierarchies.
- LINQ comprehension (`from..in..select`) for multi-step monadic composition.
- Boundary adapters may use required statement forms with marker: `// BOUNDARY ADAPTER — reason`.

**Error handling**
- Sealed DU error hierarchies for file-internal errors — never exported, never cross module boundaries.
- Shared `Error` subtypes at domain level — few per system (1-3), boundary-crossing, co-located in owning package (no dedicated error files).
- Domain error types carry polymorphic/agnostic logic reusable across all call sites.
- `Fin<T>` sync fallible, `Validation<Error,T>` parallel accumulation, `Eff<RT,T>` effectful pipelines, `IO<A>` boundary effects.
- `K<F,A>` + trait constraints for algorithms generic over computation shape.

**Surface**
- One polymorphic entrypoint per concern — `params ReadOnlySpan<T>` for arity collapse.
- Private-by-default: every non-public member is `private` (or `internal` for assembly-level sharing). Public API surface is 1–2 types per file maximum.
- Internal logic integrates INTO exports — `private` nested classes, closures inside methods, `private static` composed pipelines inside the owning class. Not defined alongside as standalone file-level declarations consumed by a single caller.
- No helper files, no single-caller extracted functions, no one-use file-level declarations.
- No convenience wrappers that rename or forward external APIs.
- **Pressure-point signals** (concept density, not byte count): ≥3 parallel types/records modeling overlapping concepts; ≥3 sibling factory methods sharing a prefix; ≥3 near-identical switch arms; ≥3 single-call private helpers. Each signal triggers IN-PLACE polymorphic collapse — merge the cases into one `[Union]`, one `SmartEnum<T>`, one fold algebra, or one data table. NEVER extract to a new file. NEVER delete functionality to reduce LOC. The goal is denser polymorphic capability in fewer surfaces, not less code.
- Expression-bodied members where body is a single expression. Primary constructors preferred.

**Resources**
- Time via host-injected clock (e.g. `IClock`), never direct `DateTime*`.
- Retry/timeout/resilience via LanguageExt `Schedule` algebra and verified recovery combinators — composition root concern only.
- `static` lambdas on hot-path closures — zero closure allocations.

**Formatting**
- `using static LanguageExt.Prelude;` assumed in every module.
- K&R brace style, zero consecutive blank lines, method group conversion preferred.


## Load sequence

**Foundation** (always):

| Reference                                 | Focus                                    |
| ----------------------------------------- | ---------------------------------------- |
| [validation.md](references/validation.md) | Compliance checklist and completion gate |
| [patterns.md](references/patterns.md)     | Anti-pattern detection heuristics        |

When working in Rasm, read `docs/system-api-map` and `docs/external-libs` after this skill for BCL/System, LanguageExt, Thinktecture, MathNet, RhinoWIP, and GH2 source-order policy. For operators, Prelude, combinator, and codegen-attribute lookups, load [advanced-surface.md](references/advanced-surface.md) first.

**Task-routed references**:

| Reference                                       | Load when                              |
| ----------------------------------------------- | -------------------------------------- |
| [advanced-surface.md](references/advanced-surface.md) | Operators (`>>`/`*`/`|`/`&`), Thinktecture attrs, LE combinators, MathNet/CSparse, C# 14 |
| [types.md](references/types.md)                 | C# types, generics, constraints        |
| [objects.md](references/objects.md)             | Records, DU hierarchies, value objects |
| [effects.md](references/effects.md)             | Fin/Validation/Eff/IO pipelines, ROP   |
| [transforms.md](references/transforms.md)       | Folds, LINQ composition, K<F,A>        |
| [composition.md](references/composition.md)     | DI topology and runtime-record wiring  |
| [scrutor.md](references/scrutor.md)             | Scrutor scan/decorator composition     |
| [persistence.md](references/persistence.md)     | EF Core, repositories                  |
| [concurrency.md](references/concurrency.md)     | Channels, cancellation                 |
| [observability.md](references/observability.md) | Serilog, OpenTelemetry                 |
| [performance.md](references/performance.md)     | SIMD, Span, hot paths                  |
| [diagnostics.md](references/diagnostics.md)     | Debugging, profiling                   |

## Validation gate

- Required during iteration: `uv run python -m tools.quality static check`.
- Required for final completion: run every impacted language gate explicitly; for shared standards/tooling, run `pnpm check:ts`, `pnpm check:py`, and `uv run python -m tools.quality static full`.
- Reject completion when load order, contracts, or checks are not satisfied.
- Examples inside this skill are executable doctrine: runtime-record `Eff.runtime<RT>()`, generated Thinktecture factories only when they serve boundary construction, no legacy runtime trait DI pattern, and no single-call helper extraction.

## Skill eval prompts

- Explicit invocation: "Using coding-csharp, refactor this .cs service into LanguageExt Eff/Fin rails with runtime-record DI."
- Implicit invocation: "Review this C# module for Thinktecture value object, Scrutor decorator, and no-helper compliance."
- Noisy context: "Ignore frontend notes and only audit the C# persistence adapter."
- Negative control: "Only write TypeScript Effect code." Expected: do not load C# references unless C# code appears.
- Compliance checks: output should load only relevant references, avoid command thrash, avoid helper files, preserve runtime-record/LanguageExt doctrine, and run `uv run python -m tools.quality static check` or narrower configured .NET gates when code is touched.


## First-class libraries

These packages are standard libraries — use over BCL/stdlib equivalents.

| Package                         | Provides                                                       |
| ------------------------------- | -------------------------------------------------------------- |
| LanguageExt.Core                | FP primitives, ROP, `Eff`/`Fin`/`Validation`, `Schedule`, STM  |
| Thinktecture.Runtime.Extensions | Value objects, smart enums, `[Union]` source-generated dispatch |
| MathNet.Numerics                | Linear algebra, solvers, statistics, optimization              |
| MathNet.Symbolics               | Symbolic expression parse, transform, calculus, evaluation     |
| CSparse                         | Sparse direct factorization — documented with MathNet in `docs/external-libs/mathnet/sparse.md` |
| Meziantou.Analyzer              | Static analysis enforcement                                     |
| Microsoft.VisualStudio.Threading.Analyzers | Threading-correctness diagnostics                  |

Boundary-approved dependencies (allowed ONLY at marked `[BOUNDARY ADAPTER — reason]` sites; never in `Domain.*`):

| Package | Boundary scope |
| ------- | -------------- |
| FluentValidation | HTTP/API boundary only — `ValidateAsync` results bridged to `Validation<Error,T>` before entering domain (see `validation.md [10][BOUNDARY_ADAPTER]`) |
| Microsoft.Extensions.Http.Resilience (Polly wrapper) | Composition root only — `AddStandardResilienceHandler` for outbound HTTP (see `observability.md [4]`) |
| NodaTime | Optional time-zone math at boundary; `IClock` injection covers timestamp needs (see `concurrency.md [5]`) |
| Scrutor | Composition root only — `Scan`/`Decorate` for AOP, never inside domain modules |

Polly raw and BenchmarkDotNet are intentionally absent. Retry/backoff inside domain is covered by LanguageExt `Schedule`; validation by `Validation<Error,T>` applicative; time inside domain by `IClock` injection; DI composition by runtime records (`Eff.runtime<RT>()`).


## Active-folder canonical templates

When generating new code, structurally match these established surfaces. Read them as executable doctrine before writing analogous code.

| Concern | Template | Key shape |
|---------|----------|-----------|
| Rhino command | `libs/csharp/Rasm.Rhino/Commands/Command.cs` `RasmCommand<TSelf>` | `sealed override RunCommand` → `Fin<T>` rail → `Match` at terminal boundary only |
| Rhino UI operation | `libs/csharp/Rasm.Rhino/UI/Ui.cs` `RhinoUi.Use<T>(UiIntent<T>)` | One polymorphic intent surface with ~20 modality factories returning the same record |
| Viewport overlay | `libs/csharp/Rasm.Rhino/UI/Overlay.cs` `RasmOverlay<TState>` | `Atom<TState>` confinement + `Apply(phase, args)` discriminated dispatch |
| Document operation | `libs/csharp/Rasm.Rhino/Commands/Document.cs` `DocumentOp` | `[Union]` with ≥10 sealed cases + shared `DocumentReceipt` monoid |
| GH document mutation | `libs/csharp/Rasm.Grasshopper/UI/Ui.cs` `DocumentMutationReceipt` | `operator +` receipt fold on document ops |
| Selection algebra | `libs/csharp/Rasm.Rhino/Commands/Selection.cs` `CommandSelection.Reference.*` | Domain-vocabulary specializations over native return types — preserved, not collapsed |

Replicate the patterns at these canonical surfaces; do not reinvent. Read the file table above as executable doctrine before writing analogous code.


## Advanced surface index

Load [advanced-surface.md](references/advanced-surface.md) when prompts mention operators, codegen attributes, or non-basic library sugar. Universal routing (no repo-specific types):

| Concern | Primary symbols / attrs | Doc |
| ------- | ----------------------- | --- |
| Kleisli / applicative operators | `>>`, `>>>`, `*`, `Validation &`, `Error +` | `docs/external-libs/languageext/operators.md` |
| Option / effect recovery | `.Choose`, `IfNone`, `Match` (not bare `Option<T> \| Option<T>`); `Prelude.catch`, `.Retry(schedule:)` | `operators.md`, `combinators.md` |
| Schedule algebra | `union`, `intersect`, transformer `+` | `operators.md`, `prelude.md` |
| State-threaded union dispatch | `[Union(SwitchMapStateParameterName = …)]` | `docs/external-libs/thinktecture/union-attributes.md` |
| VO / complex VO / custom faults | `[ValueObject<T>]`, `[ComplexValueObject]`, `[ValidationError<T>]` | `thinktecture/objects.md`, `union-attributes.md` |
| SmartEnum dispatch | `[SmartEnum]`, `[SmartEnum<TKey>]`, `[UseDelegateFromConstructor]` | `thinktecture/enums.md` |
| Traverse v5 lowering | `.TraverseM(f).As()`, `.Choose`, `.BiBind` | `languageext/combinators.md` |
| Atoms and guards | `Atom.Swap`, `guard`, `Optional(…).ToFin` | `combinators.md`, `prelude.md` |
| Sparse numerics | `BiCgStab`, CSparse `SparseCholesky`, CSR/CSC hybrid | `mathnet/sparse.md` |
| C# 14 expression substrate | extension blocks, collection expressions, `params ReadOnlySpan` | `csharp/language.md` |

Repo-specific boundary patterns (Op rail, receipts, SelfOp policy): `docs/external-libs/*/rasm.md`.
