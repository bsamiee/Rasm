# [POLY_02_TYPESCRIPT]

## [TRANSCRIPT]

I ran this as context / Advanced Polymorphism TypeScript research, without editing active files. I loaded the required local route in order: `CLAUDE.md`, root `AGENTS.md`, `docs/standards/README.md`, `docs/standards/agents-md.md`, `docs/standards/AGENTS.md`, and the shared proof/agentic/form standards. I also loaded the runtime `coding-ts` skill because this report translates TypeScript and Effect doctrine into instruction-file wording.

Repository checks used `fd` and `rg`. The current TypeScript footprint is root tooling/config plus AST-grep fixtures: `vite.config.ts`, `vite.factory.ts`, `vitest.config.ts`, and `tests/tools/ast-grep/*.{ts,tsx,mts,cts}`. `package.json` exposes `check:ts` as the TypeScript validation rail, and the catalog already includes `typescript`, `effect`, `@effect/platform`, `@effect/platform-browser`, `@effect/experimental`, and `@effect/vitest`.

Current-source checks used Context7 for `/microsoft/typescript` and `/websites/effect_website`, plus official Microsoft TypeScript blog and TypeScript docs. TypeScript 7 is currently a beta/native compiler transition: the April 21, 2026 Microsoft announcement says TS 7 Beta is available through `@typescript/native-preview` and `tsgo`, preserves TypeScript 6 type-checking/CLI behavior, and lacks stable programmatic API until at least TS 7.1. Therefore “TypeScript 7 style” should mean TS 7-ready posture and TS 6+ semantics, not an invented new type-system feature set.

## [MODERN_CAPABILITIES]

Advanced polymorphism in TypeScript means the domain varies by typed values, not by caller-specific functions, wrapper files, or unchecked strings.

- Discriminated unions encode closed variant families with a stable tag field. Exhaustive dispatch uses `never` or Effect `Match.exhaustive`/tag matching so adding a variant creates a compile-time obligation at every dispatch surface. Source: TypeScript narrowing handbook, `Discriminated unions` and `Exhaustiveness checking`; Effect `code-style/pattern-matching`.
- `satisfies` validates that a vocabulary or policy table conforms to a target shape while preserving literal inference. Use it for dispatch tables, route maps, and bounded config where the keys and values must remain specific. Source: TypeScript 4.9 release notes.
- `const` type parameters preserve literal inference at generic API boundaries without forcing callers to scatter `as const`. Use them for polymorphic builders, table factories, and schema-derived operations that must retain literal keys. Source: TypeScript 5.0 release notes.
- `exactOptionalPropertyTypes` distinguishes an absent property from a present `undefined` value. Use it to keep boundary payload semantics honest and avoid treating optionality as a weak nullable rail. Source: TypeScript 4.4 release notes and TSConfig reference.
- `Effect.Effect<A, E, R>` makes success, typed failure, and required services first-class. `A` is the value rail, `E` is the bounded error rail, and `R` is the dependency/service requirement rail. Source: Effect `getting-started/the-effect-type` and `requirements-management/services`.
- Services and Layers move dependency variation into typed requirements and composition roots. A folder should vary behavior by `Context.Tag`, `Effect.Service`, and `Layer`, not ambient globals, optional constructor bags, or branchy runtime probes. Source: Effect `requirements-management/services` and `requirements-management/layers`.
- Schema-owned behavior means decode, encode, brands, tagged classes/errors, and derived projections stay attached to the runtime authority. `Schema.TaggedClass`, `Schema.TaggedError`, `Schema.tag`, and `Schema.transformOrFail` support discriminants, validation, and dependency-aware decoding without parallel model/type sprawl. Source: Effect `schema/classes`, `schema/basic-usage`, and `schema/transformations`.

## [REPO_TRANSLATION]

Local policy already points in this direction. `CLAUDE.md` requires newest viable verified tools, TypeScript skill routing, external-lib-first dependencies, no weak erased types, no imperative domain branching, no mutable accumulation, no exception-style control flow, and no schema/model proliferation. It also requires collapsing variants into one polymorphic surface and driving logic through bounded vocabularies, discriminants, and reusable projections.

`AGENTS.md` translates that into repo overlay behavior: root guidance is a router, root-started work must discover nested overlays, and implementation should extend canonical owners before adding new rails. It names operation algebras, unions, folds, projection carriers, typed receipts, and source-owned tables as the preferred collapse mechanisms.

`docs/standards/agents-md.md` is the authoring constraint for any future AGENTS wording. A useful TypeScript rule must name the trigger, target, owner, extension action, and rejected substitute. A weak rule like “use advanced TypeScript” should not ship because it does not tell the next agent which local value, algebra, service, schema, or layer to extend.

Current repo TS truth is thin but important. `package.json` line 8 wires `check:ts`; `package.json` lines 17-95 list TypeScript/Effect/Vite/Vitest dependencies; `pnpm-workspace.yaml` lines 13-27 and 54-73 catalog Effect/Nx/Vite/Vitest/TypeScript-adjacent packages; `vite.factory.ts` already uses Effect imports plus Schema-derived config; `vitest.config.ts` centralizes Vitest configuration. No production `tools/**/src/**/*.ts` package is present in the current file inventory, so per-folder TS overlays should be conditional until a folder actually owns TypeScript behavior.

## [AGENTS_MD_WORDING]

Root or parent wording should stay general and route to the skill:

- When touching `.ts` or `.tsx`, use `coding-ts`; treat `effect` and declared `@effect/*` packages as first-class implementation libraries before local reinvention.
- Collapse TypeScript variation into discriminated unions, bounded vocabulary tables, `satisfies`-checked dispatch records, const-generic builders, Effect services/layers, and schema-derived projections before adding sibling functions, wrapper modules, ad hoc aliases, or parallel schemas.
- Keep domain flows expression-first: `Effect`/`Option`/`Either`, `Match.exhaustive`, immutable folds, and typed error rails replace `if`/`switch`/`throw`/mutable loops except in marked boundary adapters.
- Treat TypeScript 7 as a compiler/tooling transition until official stable docs say otherwise; validate TS 7 adoption through the maintained package/tooling route because the current beta preserves TS 6 semantics and defers stable programmatic API.

Folder wording should appear only where local TS exists:

- For root TS tooling files: “When changing Vite, Vitest, or root TS config, extend the existing root config factory or central project table before adding child config files; preserve `check:ts` ownership in `package.json` unless a tool README becomes the command owner.”
- For `tests/tools/ast-grep`: “When adding TypeScript rule fixtures, extend the pass/fail fixture matrix for the existing AST-grep rule family; fixtures demonstrate allowed expression dispatch and rejected statement flow, not production API design.”
- For a future `tools/<name>/src/**/*.ts`: “When adding a tool operation, extend the tool operation algebra, Effect service, Schema model, and Layer wiring in the owning module before adding another command function, helper file, or optional dependency bag.”
- For a future TS app/lib folder: “When adding a domain variant, extend the discriminated union/schema authority and exhaustive dispatch table; reject parallel DTO/type/schema aliases and route one-off external names through boundary mapping.”

## [ANTI_PATTERNS]

- Treating TypeScript 7 Beta as permission to invent new type semantics; official current guidance frames it as native compiler/language-service parity with TS 6 behavior.
- Adding `FooHelper.ts`, `utils.ts`, wrapper-only service files, or `run`/`runSafe`/`runV2` surfaces instead of one polymorphic operation surface.
- Duplicating one concept as `type`, `interface`, `Schema.Struct`, branded primitive, DTO, and model class when one Schema/Class authority can derive projections.
- Using optional properties or `undefined` as failure/absence rails instead of `Option`, tagged errors, or exact boundary decoding.
- Replacing a bounded dispatch table with `if`/`else`, `switch`, or predicate chains that duplicate vocabulary knowledge.
- Exporting broad untyped environment bags instead of typed Effect service requirements and Layers.
- Putting generic TypeScript doctrine into every `AGENTS.md`; root and parent policy should carry the general rule, while leaf overlays carry only local owner-extension deltas.

## [CONFIDENCE]

High for the TypeScript/Effect capability summary and AGENTS translation. The claims come from current official Microsoft TypeScript docs/blog posts, official Effect docs via Context7, and local repository files read in this run.

Medium for TS 7 adoption wording because the current source is beta-era as of 2026-06-05. The safe instruction is to treat TS 7 as a tooling transition and require current package/tool output before changing repo policy.

Local references:
- `CLAUDE.md:3`, `CLAUDE.md:18`, `CLAUDE.md:28-35`, `CLAUDE.md:45-51`, `CLAUDE.md:61-80`, `CLAUDE.md:150-181`
- `AGENTS.md:7-11`, `AGENTS.md:17`, `AGENTS.md:35-47`, `AGENTS.md:71-82`
- `docs/standards/agents-md.md:1-16`, `docs/standards/agents-md.md:30-41`, `docs/standards/agents-md.md:81-91`, `docs/standards/agents-md.md:112-131`
- `package.json:8`, `package.json:17-95`
- `pnpm-workspace.yaml:13-27`, `pnpm-workspace.yaml:54-73`
- `vite.factory.ts`, `vitest.config.ts`, `tests/tools/ast-grep/pass/expression_flow.ts`

External references:
- https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/
- https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/
- https://www.typescriptlang.org/docs/handbook/2/narrowing.html
- https://www.typescriptlang.org/docs/handbook/release-notes/typescript-4-9.html
- https://www.typescriptlang.org/docs/handbook/release-notes/typescript-5-0.html
- https://www.typescriptlang.org/docs/handbook/release-notes/typescript-4-4.html
- https://www.typescriptlang.org/tsconfig/exactOptionalPropertyTypes.html
- https://effect.website/docs/getting-started/the-effect-type
- https://effect.website/docs/requirements-management/services
- https://effect.website/docs/requirements-management/layers
- https://effect.website/docs/code-style/pattern-matching
- https://effect.website/docs/schema/classes
- https://effect.website/docs/schema/transformations
