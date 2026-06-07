# [POLY_01_CSHARP]

## [TRANSCRIPT]

I loaded `coding-csharp` because the task asks for C# design guidance, and I loaded the repository documentation route because the deliverable is Markdown under `docs/standards/**`. The controlling local chain was `CLAUDE.md` -> root `AGENTS.md` -> `docs/standards/AGENTS.md` -> `docs/standards/README.md` -> `docs/standards/agents-md.md`; for C# library translation I also read `libs/csharp/AGENTS.md`, `Directory.Build.props`, `Directory.Packages.props`, and the `coding-csharp` advanced references.

Relevant local findings:
- `CLAUDE.md` already defines the repo as polymorphic, canonical-shape-first, newest-viable, docs/source-verified, and C# routed through `coding-csharp` for C# mechanics and `docs/standards` for Markdown mechanics: `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:3`, `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:14`, `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:27`.
- The root engineering contract already says to collapse repeated case families into operation algebras, smart enums, unions, folds, projection carriers, typed receipts, or source-owned tables, and to convert nullable/bool/exception/native-runtime failures into typed rails at the owning boundary: `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:35`.
- The repo C# surface is currently `net10.0`, `LangVersion 14.0`, nullable-enabled, latest analysis, warnings-as-errors, and analyzer-backed: `/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Build.props:51`, `/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Build.props:63`.
- The central package truth includes `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `MathNet.Numerics`, `MathNet.Symbolics`, and `CSparse`, with LanguageExt and Thinktecture globally referenced for workspace libraries: `/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Packages.props:7`, `/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Packages.props:12`, `/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Build.props:130`, `/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Build.props:215`.
- `coding-csharp` defines the implementation posture as one entrypoint per concern, typed FP/ROP rails, generated Thinktecture dispatch, no helper files, no shims, and pressure-point collapse at 3+ parallel types, factories, repeated switch arms, or single-call helpers: `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md:15`, `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md:24`, `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md:78`, `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md:101`.
- `agents-md.md` says an `AGENTS.md` file must remain a local behavioral overlay, not a README, architecture map, command catalog, validation ladder, provider manual, transcript, or research summary; useful code-owning rules name trigger, target, owner, extension action, and rejected substitute: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:1`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:30`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:81`.

Current Microsoft documentation checked:
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14): C# 14 adds extension members, extension properties/static extension members/operators, first-class span conversions, simple lambda parameter modifiers, `field`, partial events/constructors, user-defined compound assignment, and null-conditional assignment.
- [Extension members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods) and [C# 14 extension member speclet](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-14.0/extensions): extension blocks can group instance and static extension members for a receiver type, including operators, without wrapper objects.
- [Generic math](https://learn.microsoft.com/en-us/dotnet/standard/generics/math) and [static virtual interface members](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/interface-implementation/static-virtual-interface-members): static abstract/static virtual interface members make numeric/operator contracts available to generic algorithms at compile time.
- [Union types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union), [union speclet](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/unions), and [switch expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression): Microsoft now documents union types and union exhaustiveness in current/reference-preview material, but the report treats this as a target surface until local compiler/runtime proof confirms it in Rasm. Today, Rasm's proven rail remains Thinktecture `[Union]`, `SmartEnum`, sealed-record DUs, and analyzer-backed exhaustive/generated dispatch.
- [What's new in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview): .NET 10 is the platform that carries C# 14 in the current Microsoft docs.

## [MODERN_CAPABILITIES]

Advanced C# polymorphism in 2026 should mean more than inheritance or interface dispatch. It is a layered dispatch strategy:

- Closed alternatives: native C# union types where locally proven, otherwise Thinktecture `[Union]` or sealed `abstract record` cases. The goal is a closed vocabulary with exhaustive dispatch, not marker interfaces plus fall-through `_` arms.
- Generated value and case surfaces: Thinktecture `[ValueObject<T>]`, `[ComplexValueObject]`, `[SmartEnum]`, `[SmartEnum<TKey>]`, and `[Union]` carry construction, validation, dispatch, and bounded vocabularies without hand-written wrappers. The repo-local advanced reference says Thinktecture 10.2.0 supports generated union switch/map state threading, smart enum delegate behavior, value objects, validation errors, and equality comparers, while generic/ref-struct constrained sums still use plain abstract records plus manual switch: `/Users/bardiasamiee/.codex/skills/coding-csharp/references/advanced-surface.md:60`.
- Static polymorphism: `static abstract` and `static virtual` interface members let algorithms require operators, identities, factories, and numeric behavior from type parameters. In this repo, that supports generic math over domain units, vectors, dimensions, measurements, and solver scalars without widening everything to `double`, `object`, or an unbounded interface.
- Extension blocks: C# 14 extension blocks are a dense syntax for domain-local extension members, static extension members, and extension operators. They should be used to attach algebraic operations to the owned domain vocabulary when the operation is genuinely external to the receiver type but not to create a shadow wrapper API.
- Generic math and first-class span: `INumber<TSelf>`-style constraints, operator-bearing traits, `params ReadOnlySpan<T>`, and C# 14 span conversions support arity-polymorphic, allocation-aware folds. The repo already treats `params ReadOnlySpan<T>` as the arity-collapse path and flags overload families as bloat: `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md:101`, `/Users/bardiasamiee/.codex/skills/coding-csharp/references/advanced-surface.md:91`.
- Higher-kinded/effect polymorphism: LanguageExt `K<F,A>` lets algorithms abstract over computation shape, while `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `IO<A>`, `Option<T>`, `Seq<T>`, `TraverseM`, and applicative operators encode control flow as values. The advanced reference explicitly assigns `K<F,A>` operators, `TraverseM`, `Validation.Apply`, `guard`, runtime-record `Eff`, and `Atom` state to the pinned LanguageExt surface: `/Users/bardiasamiee/.codex/skills/coding-csharp/references/advanced-surface.md:20`, `/Users/bardiasamiee/.codex/skills/coding-csharp/references/advanced-surface.md:41`.
- Algebraic dispatch: dispatch should usually be one of: generated union `Switch`/`Map`, smart enum behavior, a fold/catamorphism, a source-owned policy table, or a trait-constrained generic algorithm. The result is one deep surface where new cases are added as data/cases, not as sibling methods.
- Typed rails: `Option<T>` owns absence, `Fin<T>` owns synchronous failure, `Validation<Error,T>` owns parallel accumulation, `Eff<RT,T>` owns effectful computation, and boundary adapters convert nulls, exceptions, native handles, disposables, async callbacks, and host runtime failures into those rails.
- Minimal objects: classes remain boundary capsules or durable domain owners. Single-implementation interfaces, parameter bags, DTO echoes, wrapper services, and helper types are not polymorphism; they are surface area.

## [REPO_TRANSLATION]

For Rasm, "advanced polymorphism" should mean this concrete standard:

1. Collapse case families before adding surface. When a change introduces 3+ parallel records, 3+ sibling factories, 3+ near-identical switch arms, 3+ overloads, or 3+ mutation receipt buckets, the default response is a union/smart enum/fold/table/typed rail in the existing owner. The root and C# skill already define these pressure points: `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:58`, `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md:101`.
2. Model variation at the type level. A new operation family should become an operation algebra, not `DoX`, `DoY`, `TryDoX`, `TryDoY`, and `DoXOptions`. Host-specific behavior belongs behind the owning host boundary project, because `libs/csharp/AGENTS.md` requires host isolation and acyclic library graph ownership: `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/AGENTS.md:13`, `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/AGENTS.md:42`.
3. Prefer generated dispatch when the package owns it. With current central pins, Thinktecture is an approved in-graph domain dependency. Therefore `[Union]`, `[SmartEnum]`, and `[ValueObject<T>]` are primary surfaces for closed alternatives, bounded vocabularies, and validated primitives; hand-written wrappers are rejected unless they add invariant enforcement not expressible in the generator.
4. Use native C# union types as an adoption target, not a blind migration. Microsoft's union docs are current, but they also mention runtime support caveats in preview material. Rasm should add a local proof step before replacing Thinktecture `[Union]`: `dotnet --info`, compiler version, targeted build, and a small source proof against the actual repo SDK. Until then, AGENTS wording should say "use locally proven native unions where the repo has enabled them; otherwise use the current Thinktecture/sealed-record rail."
5. Use static abstract interface members and generic math for domain algorithms, not service abstractions. Numeric kernels, measurements, dimensions, vectors, receipts with additive/monoidal behavior, and solver policies should express their algebra through type constraints, identities, and folds. App/service boundaries still use concrete capsules and runtime records.
6. Use C# 14 extension blocks to colocate algebraic operations with the owning namespace when the receiver cannot own the member directly. Do not use extension blocks to hide a weak domain model or to create renamed pass-throughs over external APIs.
7. Keep one typed error rail per file or concern. If a file mixes `bool`, `null`, exceptions, `Option`, `Fin`, and ad-hoc result records for one failure semantics, the fix is a rail collapse, not another wrapper record.
8. Treat analyzers as design feedback. `CLAUDE.md` says CSP diagnostics are hypotheses: fix true-positive code and refine the analyzer if the diagnostic forces less native or larger code. That matters for polymorphism because the best rule is not "always union"; it is "collapse when the generated or typed replacement reduces surface while preserving capability."

## [AGENTS_MD_WORDING]

Use wording like this in C#-owning `AGENTS.md` overlays. Keep it local, trigger-driven, and short:

- When adding a new operation family, extend the owning operation algebra or generated union before adding sibling methods, option bags, or caller-side switches.
- When adding a bounded vocabulary, use the existing `SmartEnum` or union rail for that concern; add raw enums, strings, or constants only when an external contract owns the exact tokens.
- When adding a validated primitive, extend the existing value-object or `readonly record struct` smart-constructor rail; do not add parallel branded wrappers for the same concept.
- When adding numeric or measurement behavior, express required operators and identities through static abstract interface constraints and generic math; do not widen the algorithm to `double`, `object`, or service callbacks.
- When adding repeated validation or failure handling, collapse the concern into one `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, or `Option<T>` rail at the owning boundary; do not mix null, bool, exceptions, and result records for the same failure semantics.
- When adding host or native API behavior, keep one sealed OOP boundary capsule and convert host nulls, exceptions, disposables, callbacks, and handles into typed rails before domain code sees them.
- When adding arity variants, prefer `params ReadOnlySpan<T>` plus a fold or operation table; do not add overload ladders unless the external API requires distinct signatures.
- When adding extension members, use C# 14 extension blocks only for owned domain algebra near the owning namespace; do not create extension wrappers that merely rename external APIs.
- When a native C# feature appears stronger than the current generated rail, prove it against the repo SDK and central build before rewriting the instruction. Route exact compiler/package facts to `Directory.Build.props`, `Directory.Packages.props`, `docs/system-api-map`, or the project architecture, not to local prose.

Do not paste the capability explanation into each overlay. A leaf overlay should name the local owner and replacement action:

- Accepted shape: "When adding a Grasshopper canvas command, extend `WireOp` and its typed intent/projection rail before adding component-local switches."
- Rejected shape: "Use advanced polymorphism and avoid helpers."

That follows `agents-md.md`: code-owning rules should name trigger, target, owner, extension action, and rejected substitute, and `AGENTS.md` files should not become research summaries or provider manuals: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:81`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:93`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:112`.

## [ANTI_PATTERNS]

- "Polymorphism" as one interface per implementation. This increases navigation and hides the concrete rail. Reserve interfaces for real multiple implementations, static abstract trait constraints, or external contracts.
- "Polymorphism" as inheritance trees with open-ended virtual behavior. Prefer closed unions/smart enums/folds unless the host framework requires an override surface.
- Native union hype without repo proof. Microsoft union docs are relevant, but Rasm should not rewrite pinned Thinktecture guidance until the exact SDK/compiler/runtime path is proven locally.
- Wrapper-only extension blocks. C# 14 extension members are useful for domain algebra; they are harmful when they merely rename APIs or hide missing ownership.
- Options records and parameter bags. They move branching into mutable or nullable fields instead of making each valid case explicit.
- Overload ladders. In this repo, arity and shape variation should usually be `params ReadOnlySpan<T>`, a query/operation algebra, or a fold.
- Catch-all switch arms over closed domains. `_` hides future cases. Use generated dispatch or explicit case handling; only boundary defensive code should use a catch-all with a typed failure.
- Mixed failure semantics. `null`, `false`, exceptions, `Try*`, and `Fin<T>` in the same domain concern mean the rail has not been chosen.
- Helper extraction as cleanup. Single-call private helpers and `*Helper`/`*Util` files are rejected. Collapse into the owning surface or inline.
- Generic receipt abstraction. Root policy explicitly rejects replacing algorithm-specific proof receipts with generic `IReceipt`, ledger, or reported-value abstractions when typed evidence fields carry route/status/sampling/solver/spectral/mesh/extraction semantics: `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:74`.
- AGENTS bloat. Do not put a C# 14 tutorial, package matrix, command ladder, exact generated member list, transcript, or research summary into a local overlay. Route stable package truth to manifests, command truth to tool docs, and C# mechanics to the skill.

## [CONFIDENCE]

[HIGH]: The repo translation is strongly supported by current local policy and manifests. `CLAUDE.md`, root `AGENTS.md`, `libs/csharp/AGENTS.md`, `Directory.Build.props`, `Directory.Packages.props`, and `coding-csharp` all point in the same direction: C# 14/.NET 10, LanguageExt/Thinktecture as approved first-class surfaces, typed FP/ROP rails, dense operation algebras, and no helper/wrapper/shim growth.

[MEDIUM]: Native C# union adoption should be treated as promising but not yet repo-proven. Microsoft Learn now documents union types and switch exhaustiveness, but Rasm should require a local compiler/runtime/build proof before changing instructions from "Thinktecture/sealed-record rail" to "native union rail."

[HIGH]: `AGENTS.md` wording should stay compact. The local `agents-md.md` standard explicitly rejects research summaries, provider manuals, copied body content, generic validation ladders, and current-baseline caveats; it wants trigger-driven local extension grammar and route-away rules.

[OPEN_CHECKS]:
- Prove native C# union syntax against this repo's actual installed .NET SDK and Roslyn toolchain before promoting it from target capability to required implementation rail.
- If a future overlay needs exact symbols like `WireOp`, `CaptureRecipe`, or a project-specific operation algebra, read that nearest project `AGENTS.md` and source owner first; this report stays cross-C# and instruction-surface level.
