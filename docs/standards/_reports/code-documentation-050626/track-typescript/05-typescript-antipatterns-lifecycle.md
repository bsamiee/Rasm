# TYPESCRIPT 5 Anti-Patterns and Lifecycle Research

Report path: `docs/standards/_reports/code-documentation-050626/track-typescript/05-typescript-antipatterns-lifecycle.md`
Focus: TypeScript 5, advanced TypeScript/TSDoc/JSDoc anti-patterns, public/private package documentation, Promise boundary comments, `@deprecated`, `@internal`, and thrown defects versus typed `E`.
Date: 2026-06-05.

## Scope

This report is source material for a later active-standard edit. It does not edit `docs/standards/reference/code-documentation.md` or any other active standard.

The controlling local question is whether the TypeScript capsule in `code-documentation.md` needs stronger TypeScript 5-era guidance around comment syntax, release tags, generated package visibility, Promise terminal boundaries, and Effect-style typed failure versus defects.

## Read Transcript

Local instruction and target files read fully:

- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/reference/code-documentation.md`

Shared governing standards read for routing, proof, form, prose, and notation:

- `docs/standards/proof.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/formatting.md`

Temporary sibling report checked for output shape:

- `docs/standards/_reports/code-documentation-050626/track-general/05-general-lifecycle.md`

External primary-source checks:

- TypeScript Handbook, JSDoc Reference, last updated 2026-06-05.
- TypeScript TSConfig `stripInternal`.
- TSDoc tag reference for `@deprecated`, `@public`, `@internal`, `@packageDocumentation`, `@remarks`, `@param`, `@returns`, `@typeParam`, and `@throws`.
- API Extractor `.d.ts` rollup trimming and release-tag diagnostics.
- API Extractor `@internal` reference.
- TypeDoc tag reference, TypeScript tag handling, `@packageDocumentation`, `@deprecated`, `{@link}`, and JSDoc support.
- Effect official documentation for expected errors, unexpected errors, error-channel operations, resource management, and generated API reference for `runPromise`, `runPromiseExit`, `Cause`, and `catchAllDefect`.

## Source Notes

Local source notes:

- `code-documentation.md` already rejects JSDoc type-expression syntax in `.ts`, duplicate type info in `@param`, broad `@throws` for typed `E`, and Promise-return comments that hide the terminal `Effect` boundary.
- `code-documentation.md` already treats TypeScript signatures, schemas, models, and `Effect<A, E, R>` as machine shape while comments carry caller-visible semantics.
- `code-documentation.md` already says `@throws` belongs to actual thrown exceptions or terminal-runner edges only.
- `code-documentation.md` already says `@public`, `@beta`, `@alpha`, `@internal`, and `@deprecated` are external support or release contracts, but its current wording can blur release tags and deprecation tags.
- `code-documentation.md` already says lifecycle tags preserve only external support contracts, and greenfield internal stale surfaces should be deleted or replaced.

External source notes:

- TypeScript's JSDoc page says only documentation tags are supported in TypeScript files; type tags such as `@type`, `@param`, `@returns`, `@typedef`, `@template`, and `@satisfies` are supported for JavaScript type checking, not as TypeScript source truth.
- TypeScript recognizes `@deprecated`, `@see`, and `@link` as documentation tags in both JavaScript and TypeScript, and it surfaces `@deprecated` in editor diagnostics and display.
- TypeScript ignores unsupported JSDoc tags, and its legacy JSDoc type synonyms can collapse to broad or `any`-like meanings. This supports rejecting closure-style type comments and type echo in `.ts`.
- TypeScript `stripInternal` omits declarations annotated with `@internal`, but the TSConfig page calls it an internal compiler option and warns that the compiler does not validate the resulting declarations.
- TSDoc treats `@deprecated` as a block tag that describes unsupported API and should name the recommended alternative. It treats `@public` and `@internal` as modifier release tags.
- TSDoc `@packageDocumentation` belongs to a package or entrypoint comment, should be the first doc comment in the entry file, and must not describe an individual API item.
- TSDoc `@throws` is informational only; it documents exception types but does not restrict what JavaScript can throw.
- API Extractor treats `@public`, `@beta`, `@alpha`, and `@internal` as release tags, supports rollup trimming by those tags, and diagnoses incompatible visibility when a more visible API references a less visible type.
- API Extractor's `@internal` page says third parties should never use internal APIs and expects an underscore prefix for explicitly internal declarations.
- TypeDoc documents exports from entry points, supports known TSDoc and common JSDoc tags, warns on unrecognized tags, removes TypeScript type-only compatibility tags from generated docs, and does not parse `@see` contents as links.
- TypeDoc `@internal` is a visibility marker for `--visibilityFilters` or `--excludeInternal`, while `@hidden` or `@ignore` removes a reflection entirely. These are renderer controls, not source-level security controls.
- Effect's expected errors are tracked in the `Effect<Success, Error, Requirements>` type; failure variants belong in `E`, not in `@throws`.
- Effect defects are unexpected errors that usually terminate the fiber; `catchAllDefect` is boundary-oriented and does not handle expected errors or interruptions.
- Effect `runPromise` is effectful and belongs at program edges; it resolves success and rejects failures, while `runPromiseExit` resolves an `Exit` so callers can inspect success, failure `Cause`, defects, and interruption without Promise rejection.
- Effect resource finalizers receive `Exit` and run across success, failure, and interruption, so comments on resourceful APIs need scope/finalizer semantics when callers own the boundary.

## Findings

### Finding 1: TypeScript `.ts` comments should stay TSDoc-first, not JSDoc-type-first.

The current rejection is correct. TypeScript source syntax owns type shape in `.ts`; JSDoc type tags are primarily the JavaScript type-checking bridge, and TypeScript says only documentation tags are supported in TypeScript files. TypeDoc also removes TypeScript compatibility tags such as `@type`, `@typedef`, and `@satisfies` from generated documentation.

Confidence: high.

Recommendation: keep the current rejection and tighten it into a positive rule if later edited: exported `.ts` APIs use TSDoc semantic tags only; JavaScript JSDoc type tags appear only in `.js` or migration surfaces where TypeScript annotations are unavailable.

### Finding 2: `@deprecated` must be split from release tags.

The current TypeScript capsule groups `@deprecated` with release tags, then says to use at most one release tag per exported API. TSDoc and API Extractor separate these concepts: `@public`, `@beta`, `@alpha`, and `@internal` are release-stage markers; `@deprecated` is a block tag for an API that remains visible but should not be used and needs a recommended alternative.

Confidence: high.

Recommendation: change the later active standard to say:

- `@public`, `@beta`, `@alpha`, and `@internal`: release-stage contract; use at most one per exported API item when API Extractor owns package visibility.
- `@deprecated`: deprecation contract; include replacement path, behavior delta, migration or removal condition, generated-reference route, and review trigger.

### Finding 3: `@internal` is package visibility, not security, secrecy, or stale-surface preservation.

TypeScript `stripInternal` can redact annotated declarations from `.d.ts`, but TypeScript warns that it does not validate the resulting declarations. API Extractor and TypeDoc provide stronger generated-reference and declaration-trimming behavior, but they still operate on emitted docs and declarations, not runtime access. `@internal` therefore supports package docs and release trimming; it does not protect secrets, enforce authorization, or justify keeping a stale greenfield API alive.

Confidence: high.

Recommendation: add a TypeScript-specific boundary sentence: `@internal` means excluded or trimmed from public package docs/declarations by the configured toolchain; it is not a security boundary, and internal stale surfaces are deleted or replaced when no external package contract consumes them.

### Finding 4: Public/private package docs need entrypoint and release-trim ownership.

TSDoc `@packageDocumentation` and TypeDoc both place package-level comments at the top of the entry file. API Extractor assumes package API analysis from entry points and warns that path-based imports make folder layout part of the public contract. This matters for Rasm's source-comment rule: package docs should document package entry contracts and official visibility, not every local module or internal path.

Confidence: high.

Recommendation: add a package-doc handoff under the TypeScript capsule:

- `@packageDocumentation` owns package entrypoint contract, package-level security or runtime obligations, and official public surface route.
- API Extractor owns release-tag trimming and API report visibility.
- TypeDoc owns browsing presentation.
- README or reference owns package maps and curated lookup facts.
- Comments do not publish private path catalogs or path-import compatibility unless a package manifest and generated reference prove those paths are supported.

### Finding 5: `@throws` should stay narrow because JavaScript can throw anything.

TSDoc `@throws` is informational and cannot restrict thrown values. In this repo's Effect-first TypeScript style, expected domain failures should appear as typed `E`, with variant semantics in `@returns` or `@remarks`; `@throws` should document only true thrown exceptions at interop boundaries, terminal runner edges, or defects that intentionally cross into Promise rejection or process-level handling.

Confidence: high.

Recommendation: keep `@throws` out of ordinary `Effect<A, E, R>` comments. Add one explicit contrast if the later editor needs clarity:

Accepted: `@returns` names `Effect<ImportReceipt, ImportFailure, ImportServices>` success, failure variants, service requirements, interruption, and scope.
Rejected: `@throws ImportFailure`.
Reason: `ImportFailure` is typed `E`; `@throws` is reserved for actual thrown exceptions or terminal runner rejection.

### Finding 6: Promise comments must name the terminal boundary and loss of typed failure precision.

Effect's generated API reference says `runPromise` executes an effect and returns a `Promise`; failures reject, and `runPromiseExit` resolves an `Exit` instead. Runtime docs call `runPromise` edge-only. A comment that says only `returns Promise<T>` hides whether typed `E`, defects, interruption, finalizers, signal cancellation, logging, spans, or process exit mapping remain observable to the caller.

Confidence: high.

Recommendation: add a TypeScript Promise-boundary rule:

- Internal exported APIs return `Effect<A, E, R>` or another typed rail where possible and document `A`, `E`, `R`, interruption, resource scope, retry, and observability.
- Promise-returning APIs are boundary adapters and document the terminal runner, rejection mapping, abort-signal behavior, lost or preserved `Cause` detail, and whether callers should use `runPromiseExit` for full outcome inspection.
- Comments never use `Promise<T>` as a substitute for explaining typed `E` and defects at the edge.

### Finding 7: Defects and typed `E` need a stronger lifecycle/security split.

Effect's expected-error docs make typed `E` recoverable and visible at the type level. Unexpected-error docs frame defects as unexpected, usually terminating, and only recoverable at boundaries for diagnostic or external-system reasons. Resource docs show finalizers receive `Exit`, so a public resourceful API's comment must say who owns `Scope`, what finalizers release, and what happens on success, typed failure, defect, and interruption when caller-visible.

Confidence: high.

Recommendation: add a defect-boundary note under `EFFECT_CHANNELS` or `SPECIAL_SHAPES`:

- Expected domain and policy failures: document as typed `E` variants.
- Defects: document only at the boundary that converts, reports, crashes, or Promise-rejects them.
- `Cause` or `Exit`: document when callers inspect all outcomes, not as routine replacement for typed `E`.
- Resource finalizers: document release behavior across success, failure, and interruption when caller-owned scope is part of the contract.

## Add Recommendations

- Add a TypeScript-specific `JSDoc versus TSDoc` rule:
  - TypeScript source owns type shape.
  - TSDoc owns semantic comments for exported `.ts` APIs.
  - JavaScript JSDoc type tags stay in `.js` or migration surfaces.
  - Unsupported or generator-ignored tags are rejected unless the configured toolchain consumes them.

- Add a TypeScript lifecycle split:
  - `@public`, `@beta`, `@alpha`, `@internal`: release-stage tags, exactly one when API Extractor owns visibility.
  - `@deprecated`: deprecation warning, replacement path, behavior delta, migration/removal condition, generated-reference route, and review trigger.
  - `@internal`: package visibility/trimming only, not security or greenfield preservation.

- Add a package-doc route:
  - `@packageDocumentation`: package entry contract only.
  - API Extractor: API report, `.d.ts` rollup, release-tag trimming.
  - TypeDoc: browsable reference presentation.
  - README/reference: package maps and lookup facts.

- Add a Promise-boundary rule:
  - `Effect<A, E, R>` comments document `A`, `E`, `R`, interruption, resource scope, retry, and observability.
  - `Promise<T>` comments document terminal runner, rejection mapping, abort signal, `Cause` loss or preservation, and `runPromiseExit` route when full outcome inspection matters.

- Add a defect-versus-typed-failure note:
  - typed `E` for recoverable expected failures;
  - defect only for unrecoverable bug or invariant break;
  - `@throws` only when an actual throw or terminal runner rejection crosses the public boundary.

## Remove Recommendations

- Remove any wording that classifies `@deprecated` as one of the mutually exclusive release tags.
- Remove any implication that TypeScript `stripInternal`, TypeDoc `@internal`, or API Extractor trimming is a security boundary.
- Remove `@param`, `@returns`, `@type`, `@typedef`, `@template`, or Closure-style type comments from `.ts` examples unless the example is explicitly rejected or describes a JavaScript migration surface.
- Remove broad `@throws` examples for Effect typed failures.
- Remove Promise-return comments that stop at `Promise<T>` and omit terminal runner and rejection semantics.

## Change Recommendations

- Change the current TypeScript capsule release-tag bullet from grouped lifecycle wording to split release-stage and deprecation wording.
- Change the `@packageDocumentation` sentence to say it belongs to package entrypoint contracts and must not document individual API items.
- Change the Promise boundary rejection into a positive required-field rule for boundary adapters.
- Change the defect guidance to explicitly say `catchAllDefect`-style recovery belongs at external-system boundaries, not ordinary domain recovery.
- Change validation wording only if needed:
  - current: TypeScript comments use syntax and tags their toolchains parse.
  - proposed: exported `.ts` comments use TSDoc semantic tags; release, deprecation, Promise-boundary, `@throws`, and internal-visibility tags are accepted only when TypeScript, API Extractor, TypeDoc, or the package support route consumes them.

## No-Change Confirmations

- Keep TypeScript machine shape in source signatures, schemas, models, and `Effect<A, E, R>`.
- Keep comments focused on caller-visible obligations, outcomes, failure channels, side effects, resource contracts, security exposure, lifecycle signals, and routes.
- Keep generated package catalogs and TypeDoc output out of hand-maintained source comments.
- Keep API Extractor and TypeDoc as generated-reference routes, not independent durable source truth.
- Keep support-matrix ownership for support status, compatibility windows, lifecycle dates, and terminal-row retention.
- Keep greenfield internal stale surfaces on the delete-or-replace path.

## Source List

- TypeScript JSDoc Reference: https://www.typescriptlang.org/docs/handbook/jsdoc-supported-types.html
- TypeScript TSConfig `stripInternal`: https://www.typescriptlang.org/tsconfig/stripInternal.html
- TSDoc `@deprecated`: https://tsdoc.org/pages/tags/deprecated/
- TSDoc `@public`: https://tsdoc.org/pages/tags/public/
- TSDoc `@internal`: https://tsdoc.org/pages/tags/internal/
- TSDoc `@packageDocumentation`: https://tsdoc.org/pages/tags/packagedocumentation/
- TSDoc `@throws`: https://tsdoc.org/pages/tags/throws/
- TSDoc `@remarks`: https://tsdoc.org/pages/tags/remarks/
- TSDoc `@param`: https://tsdoc.org/pages/tags/param/
- TSDoc `@returns`: https://tsdoc.org/pages/tags/returns/
- TSDoc `@typeParam`: https://tsdoc.org/pages/tags/typeparam/
- API Extractor `.d.ts` rollup trimming: https://api-extractor.com/pages/setup/configure_rollup/
- API Extractor `@internal`: https://api-extractor.com/pages/tsdoc/tag_internal/
- API Extractor `ae-incompatible-release-tags`: https://api-extractor.com/pages/messages/ae-incompatible-release-tags/
- API Extractor `ae-different-release-tags`: https://api-extractor.com/pages/messages/ae-different-release-tags/
- TypeDoc tags: https://typedoc.org/documents/Tags.html
- TypeDoc JSDoc support: https://typedoc.org/documents/Doc_Comments.JSDoc_Support.html
- TypeDoc `@packageDocumentation`: https://typedoc.org/documents/Tags._packageDocumentation.html
- TypeDoc `@deprecated`: https://typedoc.org/documents/Tags._deprecated.html
- TypeDoc `{@link}`: https://typedoc.org/documents/Tags.__link_.html
- Effect expected errors: https://effect.website/docs/error-management/expected-errors/
- Effect unexpected errors: https://effect.website/docs/error-management/unexpected-errors/
- Effect error channel operations: https://effect.website/docs/error-management/error-channel-operations/
- Effect resource management introduction: https://effect.website/docs/resource-management/introduction/
- Effect generated API reference for `Effect.runPromise` and `Effect.runPromiseExit`: https://effect-ts.github.io/effect/effect/Effect.ts.html
- Effect generated API reference for `Cause`: https://effect-ts.github.io/effect/effect/Cause.ts.html

## Validation

- Report file created only under `_reports/`.
- Active standards not edited.
- Current primary sources checked for TypeScript, TSDoc, API Extractor, TypeDoc, and Effect claims.
- No static, test, bridge, TypeScript build, or generated-reference rails run because this is a research report only.
