# [TYPESCRIPT_LANGUAGE_FEATURES_RESEARCH]

Modern TypeScript language features should reduce source comments, not create new comment obligations. `satisfies`, `const` type parameters, exact optional properties, schema annotations, and declaration emit own machine shape; TSDoc comments own only the caller-visible semantic delta that remains after those sources are read.

## [1][SOURCE_BASIS]

Research scope: `satisfies`, `const` type parameters, decorators and metadata, exact optional fields, schema and model generated docs, public API surface comments, and declaration-comment tooling for the TypeScript capsule in `docs/standards/reference/code-documentation.md`.

Local source truth:
- `package.json`: root `check:ts` runs `tsc --noEmit -p tsconfig.base.json`, Biome, ast-grep rules, Knip, Sherif, and Vitest; `typescript` is catalog-managed.
- `pnpm-workspace.yaml`: `typescript: 6.0.3`, `effect: 3.21.2`, and `@effect/*` package versions are current local catalog facts.
- `tsconfig.base.json`: `exactOptionalPropertyTypes`, `erasableSyntaxOnly`, `strict`, `noUncheckedIndexedAccess`, `noUncheckedSideEffectImports`, `moduleResolution: bundler`, and `target: esnext` are enabled; no `experimentalDecorators` or `emitDecoratorMetadata` flag appears in the root config.
- File discovery found no root `api-extractor.*`, `typedoc.*`, or `tsdoc.*` config. API Extractor and TypeDoc are valid profile candidates, not configured Rasm generation rails in the current root evidence.

Current primary sources checked:
- TypeScript 6.0 release notes: `https://www.typescriptlang.org/docs/handbook/release-notes/typescript-6-0.html`.
- TypeScript 5.0 release notes for standard decorators and `const` type parameters: `https://www.typescriptlang.org/docs/handbook/release-notes/typescript-5-0.html`.
- TypeScript 5.2 release notes for decorator metadata: `https://www.typescriptlang.org/docs/handbook/release-notes/typescript-5-2.html`.
- TypeScript 4.9 release notes for `satisfies`: `https://www.typescriptlang.org/docs/handbook/release-notes/typescript-4-9.html`.
- TypeScript 4.4 release notes and TSConfig reference for `exactOptionalPropertyTypes`, `experimentalDecorators`, and `emitDecoratorMetadata`: `https://www.typescriptlang.org/docs/handbook/release-notes/typescript-4-4.html`, `https://www.typescriptlang.org/tsconfig/`.
- TSDoc tag docs: `https://tsdoc.org/pages/tags/remarks/`, `https://tsdoc.org/pages/tags/inheritdoc/`.
- API Extractor release-tag diagnostic: `https://api-extractor.com/pages/messages/ae-extra-release-tag/`.
- TypeDoc tag and comment-option docs: `https://typedoc.org/documents/Tags.html`, `https://typedoc.org/documents/Options.Comments.html`, `https://typedoc.org/documents/Tags._param.html`.
- Effect generated API docs for `SchemaAST` and `JSONSchema`: `https://effect-ts.github.io/effect/effect/SchemaAST.ts.html`, `https://effect-ts.github.io/effect/effect/JSONSchema.ts.html`.
- JSON Schema annotation reference: `https://json-schema.org/understanding-json-schema/reference/annotations`.

Context7 lookup used `/microsoft/typescript-website` for TypeScript language features and `/microsoft/tsdoc` for declaration-comment tags.

## [2][LOCAL_BASELINE]

Rasm is on TypeScript `6.0.3`, not a released TypeScript 7 toolchain. The current standard's `TYPESCRIPT_7` heading can stay as a local forward-looking capsule name only if active wording avoids claiming TypeScript 7 stable behavior; source-backed facts should cite the repo catalog or current TypeScript release notes.

TypeScript 6.0 matters for code-documentation because it is a transition release toward TypeScript 7, not because it changes comment syntax. Declaration output can change ordering across the 6.0-to-7.0 line, and `--stableTypeOrdering` is documented as a diagnostic aid for comparing declaration emit, not a long-term feature. Generated declaration docs should therefore resolve symbols through the configured generator and source, not treat declaration order as durable proof.

Rasm's root TypeScript config already encodes strict machine shape: optional properties are exact, unchecked indexed access is surfaced, side-effect-only imports are checked, and `erasableSyntaxOnly` narrows language constructs to syntax that can be erased without runtime TypeScript transforms. Source comments should not compensate for relaxed TypeScript settings that are not actually relaxed in this repo.

## [3][FEATURE_FINDINGS]

### [3.1][SATISFIES]

`satisfies` validates that an expression conforms to a target type while preserving the expression's more specific inferred type. That makes it a machine-shape owner for config objects, lookup tables, route maps, discriminant maps, and schema registries.

Comment rule: do not add a source comment that restates the target type or says an object "satisfies" a contract. Add a TSDoc note only when the target type leaves a caller-visible semantic contract unexpressed, such as why a route map must be exhaustive, which domain owns extra keys, or how a validated table is consumed by generated docs.

Accepted: use `satisfies` on a public registry and document the runtime or generated-reference consequence only if callers consume that consequence.
Rejected: add `@remarks` that says the object satisfies `Record<Route, Handler>`.
Reason: the rejected comment repeats a type-checking fact and adds no semantic delta.

For JavaScript files, TypeScript documents a JSDoc `@satisfies` form, and TypeDoc lists `@satisfies` among supported tags. Rasm's TypeScript capsule should reject `@satisfies` comments in `.ts` public APIs unless a JavaScript type-checking surface is explicitly in scope; `.ts` source should use the language operator.

### [3.2][CONST_TYPE_PARAMS]

`const` type parameters let API authors request const-like inference for object, array, and primitive expressions written at the call. The feature is a declaration-level inference contract, not prose.

Comment rule: do not explain that `const T` preserves literals when the declaration itself shows that contract. Use `@typeParam` only for semantic relationships the type parameter cannot show: algebraic role, closed vocabulary meaning, readonly caller obligation, evidence carried by a literal tuple, or the reason the caller must pass an inline literal instead of a pre-widened variable.

Accepted: `@typeParam T - literal route set whose keys must match generated navigation anchors.`
Rejected: `@typeParam T - const generic preserving literal inference.`
Reason: the rejected text restates compiler behavior; the accepted text names the source-contract consequence.

The TypeScript 5.0 docs warn that `const` type parameters do not make mutable constraints immutable and affect only expressions written in the call. If that difference is caller-visible, the comment should state the caller obligation in domain terms, not describe inference mechanics.

### [3.3][DECORATORS_METADATA]

Decorator behavior is split across modern standard decorators, legacy experimental decorators, and metadata surfaces. TypeScript 5.0 states standard decorators differ from legacy `--experimentalDecorators`, are emitted and type-checked differently, are not compatible with `--emitDecoratorMetadata`, and do not allow parameter decorators. TypeScript 5.2 adds a standard-decorator `context.metadata` surface. The handbook's legacy metadata path still requires `experimentalDecorators`, `emitDecoratorMetadata`, and `reflect-metadata`.

Local rule: Rasm's root `tsconfig.base.json` does not enable `experimentalDecorators` or `emitDecoratorMetadata`, so no standard should claim decorator metadata exists in Rasm TypeScript APIs without a nearer project config or source proof. If a project enables decorators, the code-documentation standard should require the comment to name the public runtime contract: decorator target, metadata key or context field, caller-visible side effect, registration timing, and whether generated reference tools consume the decorator.

TSDoc and TypeDoc support a `@decorator` tag, but that tag documents decorator use; it does not prove runtime metadata, validation, dependency injection, or generated schema behavior. Treat decorator docs as public API comments only when decorators are part of the exported contract or generated model surface.

Accepted: `@remarks` says the decorator registers the class under a stable metadata key consumed by the plugin loader, and names the config that enables the metadata path.
Rejected: `@decorator` on a local implementation decorator with no public generated-reference or runtime consumer.
Reason: comments must document caller-visible behavior, not decorate implementation internals.

### [3.4][EXACT_OPTIONAL_FIELDS]

`exactOptionalPropertyTypes` means an optional property is interpreted as written; TypeScript does not silently add `| undefined` to the property type. Rasm enables this option, so absence and explicit `undefined` are distinct at the type boundary.

Comment rule: comments must not say "optional" when the real contract is "may be omitted" or "may be explicitly `undefined`." The type carries optionality; comments carry only domain absence semantics, default interpretation, serialization behavior, patch semantics, merge semantics, or schema emission effects that the property marker cannot express.

Accepted: `@param options.theme - omit to inherit the system theme; pass "dark" or "light" to override it.`
Rejected: `@param options.theme - optional string.`
Reason: the rejected comment repeats shape and hides the domain meaning of absence.

Schema generated docs need the same split. A JSON Schema or Effect Schema should own field requiredness, optionality, default annotation, title, description, and examples when those values are consumed by generated schema docs. TSDoc should own cross-field invariants and public model purpose that a single field annotation cannot carry.

### [3.5][SCHEMA_MODEL_DOCS]

Effect exposes schema annotations such as title, description, documentation, default, examples, identifier, and JSON Schema annotations in generated API docs. Effect's JSON Schema annotation interface includes `title`, `description`, `default`, and `examples`. JSON Schema defines annotations as non-validation metadata; `title` and `description` describe schema purpose, `default` does not fill missing values during validation, and `examples` help explain use without enforcing validation.

Comment rule: schema and model generated docs should be sourced from schema annotations when a generator or schema consumer reads them. Do not duplicate field descriptions in TSDoc comments for exported schemas when annotation metadata is the machine-consumed source. Use TSDoc for the schema export's public purpose, cross-field invariant, failure carrier, parse side effect, security exposure, or generation route.

Accepted: field `description` lives in the Effect Schema annotation, while the exported schema's TSDoc summary says what boundary the schema validates and which parse failures callers can receive.
Rejected: a TypeScript interface TSDoc block manually copies every generated schema field description.
Reason: generated schema docs should refresh from schema metadata, not a hand-maintained source-comment catalog.

If no configured schema documentation generator exists, the standard should avoid claiming generated schema output. It can still require schema annotations when the runtime schema itself is the public contract.

### [3.6][PUBLIC_API_COMMENTS]

TSDoc separates the summary section from `@remarks`; TSDoc `@inheritDoc` copies only summary, remarks, params, type params, and returns, not tags such as `@deprecated` or examples. API Extractor treats `@public`, `@beta`, `@alpha`, and `@internal` as release tags and reports more than one release tag on a comment. TypeDoc supports many TSDoc tags and warns on unknown tags unless configured; it also states that TSDoc `@param` tags should omit types and use a hyphen after the parameter name, even if TypeDoc is permissive for compatibility.

Comment rule: public TypeScript comments should be TSDoc semantic comments, not JSDoc type-expression comments. Types, optionality, literal inference, decorators, and schema shape belong in TypeScript syntax or schema metadata. TSDoc block tags should carry only caller obligation, semantic generic relationship, success and failure meaning, resource scope, lifecycle route, and generated-reference links.

API Extractor and TypeDoc should remain toolchain candidates until repo config proves adoption. When adopted, the standard should name which tool owns strictness. API Extractor-backed contracts should rely on comments that survive emitted declaration visibility; TypeDoc-only discovery should be treated as renderer behavior unless TypeDoc is the declared generated-reference owner.

## [4][RECOMMENDATIONS]

[ADD]:
- Add a TypeScript source-truth line: language features and schema annotations own machine shape before TSDoc comments are considered.
- Add exact optional wording: with `exactOptionalPropertyTypes`, comments must distinguish omitted from explicit `undefined` only when that distinction changes caller behavior.
- Add decorator metadata guard: claim decorator metadata only when nearest TypeScript config and runtime source prove the standard or legacy metadata path.
- Add schema/model route: generated schema docs consume schema annotations; TSDoc comments should not hand-copy field catalogs.
- Add `satisfies` and `const` type parameter rules as comment-omission examples, because both are common sources of type-echo comments.

[CHANGE]:
- Reword `Toolchain: TSDoc for exported TypeScript 7` to separate repo-current compiler truth from future-facing local policy. Suggested direction: `Toolchain: TSDoc for exported TypeScript APIs; current repo compiler truth comes from pnpm catalog and nearest tsconfig.`
- Change "API Extractor is the strict comment canon, and TypeDoc is the browsing renderer" to "API Extractor is the strict canon when configured; TypeDoc is the browsing renderer when configured." Current root evidence does not prove either tool is configured.
- Change `@see` guidance to prefer `{@link ...}` or `@see {@link ...}` for resolvable routes; keep rejecting bare `@see SomeSymbol`.
- Change generated-reference handoff wording so source-comment clarity changes do not imply adjacent API/reference updates unless generated output, anchor identity, or reader action changes.

[REMOVE]:
- Remove any implication that `@satisfies`, JSDoc type syntax, `@typedef`, `@callback`, or type-bearing `@param` forms belong in `.ts` API comments.
- Remove any implication that decorators metadata is available by default in modern TypeScript. Standard decorators, legacy decorators, legacy `emitDecoratorMetadata`, and standard `context.metadata` are separate surfaces.
- Remove any field-by-field TSDoc catalog pattern for schemas or models when schema annotations or generated contracts own those fields.

## [5][NO_CHANGE_CONFIRMATIONS]

Keep the current TypeScript capsule's core boundary: TypeScript syntax, exported schemas, models, and `Effect<A, E, R>` carry machine shape; comments carry semantic contract. Current sources reinforce that split.

Keep the rejection of broad `@throws` for typed `E`. TypeScript and TSDoc can document actual thrown exceptions, but Effect expected failures are typed data until a terminal runner translates them.

Keep release tags limited to external support contracts. API Extractor's release-tag diagnostic supports the existing "at most one release tag" rule, and Rasm greenfield policy should still delete or replace stale internal surfaces instead of preserving them with release tags.

Keep `{@inheritDoc ...}` only for exact inherited contracts. TSDoc copies a limited subset of comment fields, so lifecycle tags, examples, defaults, and deprecation routes must be explicit when they apply.

## [6][CANDIDATE_CAPSULE_PATCH]

Use this as source material for a later active-standard edit, not as a direct patch:

```markdown conceptual
[LANGUAGE_FEATURES]:
- `satisfies`: source-owned conformance check; comments add only the semantic consequence of the validated value.
- `const` type parameters: declaration-owned literal inference; `@typeParam` adds only caller-visible generic role or readonly/domain obligation.
- exact optional fields: `?` means omission under `exactOptionalPropertyTypes`; comments state absence, default, patch, merge, or serialization semantics only when observable.
- decorators: document only exported decorator contracts, runtime metadata keys, registration side effects, and generated consumers proven by nearest config and source.
- schema annotations: generated schema/model docs consume schema metadata; TSDoc owns schema purpose, cross-field invariants, parse failures, and generation routes.
```

## [7][PROOF_GAPS]

- No active standards were edited.
- No TypeScript rail was run; this was a docs research report, and no TypeScript source or config was changed.
- No TypeDoc, API Extractor, or schema-doc generation was run because no current root config proves those tools are configured generation rails.
- External sources were checked on 2026-06-05; re-check TypeScript, TSDoc, API Extractor, TypeDoc, Effect, and JSON Schema docs before promoting drift-prone tool behavior into active standards.

## [8][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, shared governing standards, active reference siblings, and the full target `docs/standards/reference/code-documentation.md`.
- [x] Used current primary sources and Context7 for TypeScript and TSDoc documentation.
- [x] Checked local TypeScript manifests and configs for repo-current compiler and generator truth.
- [x] Edited only `docs/standards/_reports/code-documentation-050626/track-typescript/04-typescript-language-features.md`.
- [x] Left active standards and sibling worker reports untouched.
