# [CSHARP_03_ROP_LANGUAGEEXT]

This research packet covers C# 14 source-comment guidance for Rasm's LanguageExt-style functional and ROP surfaces. It is source material for `docs/standards/reference/code-documentation.md`; it does not edit active standards.

## [1][SCOPE]

Focus: `Fin<T>`, `Eff<RT,T>`, `Validation<Error,T>`, `IO<T>`, `Bracket`, `K<F,T>`, typed data failure versus actual exceptions, terminal run ownership, and resource ownership in XML documentation comments.

Output file: `docs/standards/_reports/code-documentation-050626/track-csharp/03-csharp-rop-languageext.md`

Assigned constraint: write only this report file; do not edit active standards.

## [2][READ_TRANSCRIPT]

Repository instructions and standards read:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/proof.md`
- `docs/standards/formatting.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/reference/api.md`
- `docs/standards/reference/reference.md`
- `docs/standards/reference/support-matrix.md`
- `docs/standards/reference/readme.md`

C# and LanguageExt repo-local sources read:
- `.claude/skills/coding-csharp/SKILL.md`
- `.claude/skills/coding-csharp/references/effects.md`
- `.claude/skills/coding-csharp/references/transforms.md`
- `.claude/skills/coding-csharp/references/patterns.md`
- `.claude/skills/coding-csharp/references/validation.md`
- `docs/external-libs/languageext/api.md`
- `docs/external-libs/languageext/effects.md`
- `docs/external-libs/languageext/rasm.md`
- `docs/external-libs/languageext/combinators.md`
- `docs/external-libs/languageext/traits.md`
- `Directory.Packages.props`
- `Directory.Build.props`

Commands and outcomes:
- `rg` over memory and repo standards located current docs-standards preferences, the target standard, and current LanguageExt package truth.
- `fd -a ... docs/standards` located governing standards and reference-family files.
- `rg -n "LanguageExt|LanguageExt.Core|LangVersion|TargetFramework|net10..." Directory.Packages.props Directory.Build.props **/*.csproj` confirmed `LanguageExt.Core` `5.0.0-beta-77`, `net10.0`, C# `14.0`, nullable enabled, and global LanguageExt usings.
- `uv run /Users/bardiasamiee/.codex/skills/context7-tools/scripts/context7.py resolve LanguageExt ...` returned no Context7 library.
- `uv run /Users/bardiasamiee/.codex/skills/context7-tools/scripts/context7.py lookup LanguageExt ...` returned no Context7 library.
- `uv run python -m tools.quality api resolve LanguageExt.Core all` resolved the pinned package assembly, XML, and nuspec under `.cache/nuget/packages/languageext.core/5.0.0-beta-77`.
- `uv run python -m tools.quality api query LanguageExt.Core Fin` succeeded and produced decompile/API artifacts.
- `uv run python -m tools.quality api query LanguageExt.Core Eff` succeeded and produced decompile/API artifacts.
- `uv run python -m tools.quality api query LanguageExt.Core Validation` succeeded and produced decompile/API artifacts.
- `uv run python -m tools.quality api query LanguageExt.Core IO` succeeded and produced decompile/API artifacts.
- `git ls-remote --tags https://github.com/louthy/language-ext.git ...` confirmed tag `v5.0.0-beta-77` at `1f629efd9d4de54f88b358613e2debde872fd88a`.
- `git ls-remote https://github.com/louthy/language-ext.git HEAD` confirmed current upstream `HEAD` at `041f74a00f667aba34b927f396dba39774645670`.

## [3][SOURCE_LEDGER]

Repo source of truth:
- `Directory.Packages.props`: `LanguageExt.Core` is pinned at `5.0.0-beta-77`.
- `Directory.Build.props`: Rasm uses `net10.0`, `LangVersion` `14.0`, nullable enabled, and global usings for `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`, `LanguageExt.Effects`, `LanguageExt.Pretty`, `LanguageExt.Traits.Domain`, and static `LanguageExt.Prelude`.
- `docs/external-libs/languageext/rasm.md`: Rasm rail policy maps native value admission to `Fin<T>`, independent requirements to `Validation<Error,T>`, host runtime work to `Eff<RT,T>`, deferred effects/resources to `IO<T>`, and GH UI parallel checks to `Validation<Seq<UiFault>,T>`.
- `docs/external-libs/languageext/effects.md`: terminal collapse stays at command/component/tool edges; `Eff.runtime<RT>()` owns runtime records; `IO<T>.Bracket`, `BracketFail`, `Finally`, and `Prelude.use` own v5 resource scope.
- `docs/external-libs/languageext/traits.md`: `K<F,A>` is effect-polymorphic and should appear only when it removes real duplication; runtime-record `Eff<RT,T>` remains the default host effect shape.

Pinned package evidence:
- `.cache/nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.xml`
- `.cache/nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.dll`
- `.cache/nuget/packages/languageext.core/5.0.0-beta-77/languageext.core.nuspec`

Current upstream sources:
- NuGet package page: [LanguageExt.Core](https://www.nuget.org/packages/LanguageExt.Core) lists `5.0.0-beta-77` as a prerelease version updated `12/30/2025`.
- GitHub release page: [louthy/language-ext releases](https://github.com/louthy/language-ext/releases) lists v5 prereleases and the v5 operator work across `Eff<A>`, `Eff<RT,A>`, `Fin<A>`, `IO<A>`, `Validation<F,A>`, and `K<F,A>`.
- GitHub wiki page: [How to deal with side effects](https://github.com/louthy/language-ext/wiki/How-to-deal-with-side-effects) is official but marks itself as pre-v5-review. Use it only as background for terminal edge concepts such as running at `Main` or request edges, not as v5 API proof.
- ReadTheDocs page: [C# Functional Language Extensions](https://languageext.readthedocs.io/en/latest/README.html) is useful background for functional intent and wrapped-value posture, but the pinned package XML outranks it for v5 carrier details.

## [4][CURRENT_STANDARD_BASELINE]

`docs/standards/reference/code-documentation.md` already makes the correct high-level move: C# XML comments document caller-visible semantics that declarations cannot express, not carrier names or type facts. Its C# capsule currently names these LanguageExt carriers:
- `Fin<T>`: success value and domain `Error` meanings.
- `Validation<Error,T>`: independent obligations and accumulated failure semantics.
- `Eff<RT,T>`: runtime capabilities, deferred execution, cancellation or interruption behavior, retry or repeat `Schedule`, resource scope, and terminal `Run` owner.
- `IO<T>`: boundary action, resource ownership, and execution point.
- `Bracket`: acquire/use/release ownership and whether release runs on success, failure, and cancellation.
- `K<F,T>` or trait-polymorphic APIs: algebraic obligation on `F` only when the constraint does not explain caller meaning.

The existing anti-patterns also align with this research:
- A fake `<exception>` tag for a typed rail is rejected.
- A carrier echo in `<returns>` is rejected.
- FP/ROP surfaces must name success, every failure variant, runtime-context requirements, resource contracts, and terminal boundaries where caller-visible.

## [5][PRIMARY_FINDINGS]

### [5.1][FIN]

Pinned XML describes `Fin<A>` as equivalent to `Either<Error,A>` and the concrete result of a computation. Its observable states are `Succ` and `Fail`; the fail value is `LanguageExt.Common.Error`.

Documentation consequence:
- Use `<returns>` to name the success payload and each caller-visible domain `Error` family, code, or subtype.
- Do not write `<returns>Returns a Fin.</returns>`.
- Do not add `<exception>` for failures that are returned as `Error`.
- If native exceptions are converted into `Error`, state the boundary conversion and preserved diagnostic fields in `<remarks>` or `<returns>`, not as thrown exceptions.

Rasm consequence:
- `Fin<T>` comments should usually mention operation identity, native type, tolerance, host sentinel, or domain failure stage only when public callers need that meaning.
- Internal `Fin<T>` pipelines do not need comments when the domain error type and method name already carry the rail.

### [5.2][VALIDATION]

Pinned XML describes `Validation<F,A>` as Either-like but accumulating multiple failed values. It also states `F` needs `Monoid<F>` or `Semigroup<F>` support, with runtime trait resolution causing `TypeLoadException` for incompatible generic failure types.

Documentation consequence:
- `Validation<Error,T>` comments should name the independent obligations evaluated and the accumulated failure meaning.
- Comments should state whether failures are accumulated applicatively or collapsed to `Fin<T>` before a boundary.
- Do not document validation as a sequential guard chain.
- Do not add `<exception>` for ordinary accumulated failures.
- Add an actual `<exception>` only for a public generic validation surface where the caller can supply an incompatible `F` and the surface intentionally exposes LanguageExt's runtime trait-resolution failure. Rasm's usual `Validation<Error,T>` and `Validation<Seq<UiFault>,T>` surfaces should not carry that exception tag.

Rasm consequence:
- Domain comments should prefer `Validation<Error,T>` and name the requirement set.
- GH UI comments may use `Validation<Seq<UiFault>,T>` only where the UI fault union is the public caller-visible accumulation carrier.

### [5.3][EFF]

Pinned XML describes `Eff<RT,A>` as encapsulating side effects, exception capture, and dependency injection through the `RT` runtime. The API rail shows `Eff` is fallible and IO-capable, and Rasm uses concrete runtime records rather than legacy runtime trait DI.

Documentation consequence:
- `<typeparam name="RT">` should document the runtime record capability only when the type parameter's constraint or name does not make caller obligations clear.
- `<returns>` should name the success value, typed failure rail, deferred execution, and terminal owner when public callers decide where to run the effect.
- `<remarks>` should name required runtime capabilities, cancellation/interruption behavior, retry/repeat schedule, resource scope, and side effects when caller-visible.
- Do not write comments that make `Eff<RT,T>` sound already executed.
- Do not hide terminal collapse; public APIs that call `.Run`, `.RunAsync`, or `.RunIO` must name the owner of that collapse and the error/status mapping.

Rasm consequence:
- Comment only the public boundary where a Rhino command, GH component, CLI, test rail, or host service runs the effect.
- Pure builders returning `Eff<RT,T>` need comments only when the builder adds runtime requirements, resource ownership, retry policy, or failure semantics not visible from the signature.

### [5.4][IO_AND_BRACKET]

Pinned XML describes `IO<A>` as a computation that performs IO when run and says the edge should be `Main`, a web-request handler, or another application edge. It describes `IO<A>.Bracket` as creating a local resource environment in which acquired resources are released when the computation completes. `BracketFail` releases resources on failure only. The monad-unlift XML also states retry/repeat release resources acquired inside failing or repeated IO computations and warns that repeated computations cannot return resources acquired inside the repeat.

Documentation consequence:
- `IO<T>` comments should state the boundary action, who performs it, and what resource or side effect is deferred.
- `Bracket` comments should name acquisition, use, release, and release mode: completion, failure-only, finalizer, retry, repeat, cancellation, or uninterruptible scope where applicable.
- `Retry` or `Repeat` comments should state the schedule and whether resources are reacquired per attempt.
- A public API returning a resource from inside a repeated IO computation should be rejected or explicitly documented as impossible if the signature/source shape is being reviewed.
- Cancellation comments should name local cancellation behavior only when the caller can observe it. The XML notes local cancellation still propagates as an exception, so comments should not claim cancellation is silently swallowed.

Rasm consequence:
- Boundary resource docs matter for Rhino/GH native disposables, view/canvas subscriptions, file handles, EF `DbContext` shells, process runners, and bridge sessions.
- Public comments should distinguish "returns an effect that will allocate" from "runs and owns disposal now."

### [5.5][K_AND_TRAITS]

Upstream release notes for v5 operator work state generic extensions return abstract `K<F,A>` while bespoke versions return concrete types for usability. The pinned XML exposes many APIs over `LanguageExt.Traits.K<F,A>` and trait constraints such as `Monad`, `Fallible`, `MonadIO`, and `MonadUnliftIO`.

Documentation consequence:
- XML comments should document the algebraic obligation on `F` only when the generic constraint does not explain caller meaning.
- A `K<F,T>` comment should name what the caller must provide: `Monad`, `Fallible`, `Traversable`, `MonadIO`, `MonadUnliftIO`, `Natural`, or lowering route.
- Do not add decorative "higher-kinded" comments for ordinary `Fin`, `Validation`, or `Eff` APIs.
- Comments should name the concrete boundary projection when caller-visible, such as `.As()` for `Eff`/`IO` or `>> lower` where that route is the maintained local pattern.

Rasm consequence:
- `K<F,A>` belongs in comments only when a public trait-polymorphic algorithm exists and callers must understand which carrier family supplies success, failure, IO, or traversal.
- Runtime host dependencies should stay in `Eff<RT,T>` comments, not be hidden behind trait vocabulary.

### [5.6][ERROR_VERSUS_EXCEPTION]

Pinned XML for `LanguageExt.Common.Error` and `ErrorException` separates data errors from exception hierarchy interop. `Error` can represent expected or exceptional errors, and `ErrorException` exists so code unable to handle `Error` can throw and later convert back.

Documentation consequence:
- Treat data failure and actual throws as separate channels.
- `<exception>` belongs only to actual thrown exceptions on a throwing surface.
- Typed rails carry failure semantics through `<returns>` or `<remarks>`.
- If a boundary converts a native exception to `Error`, document the conversion result, not a thrown exception.
- If a terminal runner converts `Error` to process exit, HTTP status, Rhino command result, GH message, or test result, document that terminal mapping at the runner surface.

Rasm consequence:
- Source comments should not make domain failures look like runtime exceptions.
- Boundary adapters may document native exception conversion, but internal domain pipelines should preserve typed `Error`/fault unions.

## [6][COMMENT_SHAPES]

Use these shapes as update candidates for the C# capsule. They are conceptual, not copy-paste replacements.

Fin rail:
```csharp conceptual
/// <summary>Projects one accepted Rhino object into a topology receipt.</summary>
/// <returns>
/// A successful receipt when the object is supported; otherwise a domain error naming the operation,
/// native type, and unsupported topology stage.
/// </returns>
```

Validation rail:
```csharp conceptual
/// <summary>Validates one import request without short-circuiting independent requirements.</summary>
/// <returns>
/// The accepted request, or accumulated validation errors for source path, version, units, and host context.
/// </returns>
```

Eff rail:
```csharp conceptual
/// <summary>Builds one deferred export operation for a file-runtime boundary.</summary>
/// <typeparam name="RT">Runtime record that provides document, scheduler, file endpoint, and cancellation capability.</typeparam>
/// <returns>
/// An effect that writes the export when the host runner supplies <typeparamref name="RT"/> and collapses the rail.
/// </returns>
```

IO and bracket:
```csharp conceptual
/// <summary>Creates a bracketed database operation.</summary>
/// <returns>
/// A deferred IO computation that opens one context, runs the supplied operation, and disposes the context when the computation completes.
/// </returns>
```

Trait-polymorphic:
```csharp conceptual
/// <summary>Traverses each admitted sample in the caller's effect carrier.</summary>
/// <typeparam name="F">Carrier that supplies monadic bind and fallible error construction for rejected samples.</typeparam>
/// <returns>A carrier value that preserves the caller's success and failure semantics until the boundary lowers it.</returns>
```

## [7][RECOMMENDED_STANDARD_DELTAS]

These are candidate refinements for `docs/standards/reference/code-documentation.md`; they are not applied here.

1. Strengthen `Fin<T>` from "success value and domain `Error` meanings" to "success payload, caller-visible domain `Error` variants, and native-exception-to-Error conversion only where a boundary exposes it."
2. Strengthen `Validation<Error,T>` from "independent obligations and accumulated failure semantics" to include "Monoid-backed accumulation; no `<exception>` for ordinary accumulated failures; actual `TypeLoadException` only on public generic `Validation<F,T>` surfaces that let callers choose incompatible `F`."
3. Strengthen `Eff<RT,T>` to require terminal owner language only at public boundaries that run the effect. Builders document runtime capabilities and deferred execution; runners document collapse.
4. Strengthen `IO<T>` and `Bracket` to distinguish completion release, failure-only release, retry/repeat reacquisition, finalizer behavior, and cancellation visibility.
5. Strengthen `K<F,T>` to require naming the trait obligation and lowering route only when caller-visible; reject decorative trait vocabulary.
6. Add an exception/data-failure sentence to the C# capsule: `<exception>` is for actual thrown exceptions only; typed rails use `<returns>` and `<remarks>` for failure data.
7. Add a terminal-runner sentence: public runners that call `Run`, `RunAsync`, `RunIO`, process exit, Rhino command completion, GH component messaging, or test bridge collapse must document the owner and status mapping.

## [8][REJECTIONS]

Reject these comment patterns:
- `<returns>Returns <see cref="Fin{T}"/>.</returns>`
- `<exception cref="Error">Thrown when validation fails.</exception>`
- `<exception cref="Exception">Any failure.</exception>` on a typed rail.
- `<summary>Runs an effect.</summary>` for a builder that returns an unrun `Eff<RT,T>`.
- `<typeparam name="F">The higher-kinded type.</typeparam>` without a trait obligation or caller action.
- Resource comments that say "disposed eventually" without acquisition owner, release owner, and release condition.
- Comments copied from LanguageExt wiki `Aff` guidance as v5 proof.

## [9][OPEN_QUESTIONS]

- The existing standard names `C# 14 extension blocks`; verify the exact compiler/XML documentation behavior for extension blocks before adding new tag-level guidance beyond receiver invariants.
- If Rasm later generates DocFX output from XML, verify whether multiline `<returns>` and `<remarks><list>` rendering meet the code-block and Markdown-heavy XML rejection already in the standard.
- If a public `K<F,T>` API is added, inspect its exact trait constraints before standardizing a single comment recipe; trait obligations differ materially between `Monad`, `Fallible`, `MonadIO`, and `MonadUnliftIO`.

## [10][VALIDATION]

No active standards were edited. Validation run:
- `git diff --check -- docs/standards/_reports/code-documentation-050626/track-csharp/03-csharp-rop-languageext.md`: passed.
- `LC_ALL=C rg -n '[^\x00-\x7F]' docs/standards/_reports/code-documentation-050626/track-csharp/03-csharp-rop-languageext.md`: no non-ASCII bytes found.

Optional follow-up: run the local link checker if this `_reports/` research folder is included in docs link validation.
