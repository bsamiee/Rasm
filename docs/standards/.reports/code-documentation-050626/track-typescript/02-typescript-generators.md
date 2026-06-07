# [TYPESCRIPT_GENERATORS]

TypeScript generated documentation should use API Extractor as the package API contract gate when release tags, declaration rollups, API reports, or `.api.json` doc models matter, and TypeDoc as the generated browsing surface when readers need navigable HTML, JSON, or plugin-rendered Markdown. Rasm currently has no checked-in TypeDoc, API Extractor, API Documenter, or TSDoc package/config route, so every generator claim below is upstream capability until a manifest, config, generated artifact, or command output adds local proof.

## [1][SOURCE_SNAPSHOT]

Source model: current primary tool docs plus npm package metadata.
Evidence: `npm view typedoc version time.modified dist-tags --json`; `npm view @microsoft/api-extractor version time.modified dist-tags --json`; `npm view @microsoft/api-documenter version time.modified dist-tags --json`; `npm view @microsoft/tsdoc version time.modified dist-tags --json`; official TypeDoc, API Extractor, and TSDoc pages listed below.
Source of truth: upstream docs for tool behavior; Rasm `package.json` and `pnpm-workspace.yaml` for configured local package truth.
Last verified: 2026-06-05
Review trigger: TypeDoc, API Extractor, API Documenter, TSDoc, Rasm package catalog, generated-doc config, or standards wording changes.

| [INDEX] | [PACKAGE]                   | [LATEST]  | [NPM_MODIFIED] | [LOCAL_RASM_STATUS]                                                        |
| :-----: | :-------------------------- | :-------: | :------------- | :------------------------------------------------------------------------- |
|   [1]   | `typedoc`                   | `0.28.19` | 2026-04-12     | absent from `package.json`, `pnpm-workspace.yaml`, and active repo configs |
|   [2]   | `@microsoft/api-extractor`  | `7.58.7`  | 2026-04-23     | absent from `package.json`, `pnpm-workspace.yaml`, and active repo configs |
|   [3]   | `@microsoft/api-documenter` | `7.30.5`  | 2026-04-23     | absent from `package.json`, `pnpm-workspace.yaml`, and active repo configs |
|   [4]   | `@microsoft/tsdoc`          | `0.16.0`  | 2026-04-23     | absent from `package.json`, `pnpm-workspace.yaml`, and active repo configs |

Local scan: `rg -n "typedoc|api-extractor|api-documenter|@microsoft/tsdoc|tsdoc|docModel|dtsRollup|@public|@beta|@alpha|@internal" package.json pnpm-workspace.yaml pnpm-lock.yaml tsconfig*.json apps libs tools docs -g '!.reports/**' -g '!node_modules/**'` found only active standards wording, not configured tooling.

## [2][OWNER_SPLIT]

Use one generator owner per job. API Extractor is the stricter gate for published package API integrity because it validates TSDoc release tags, missing or incompatible release status, forgotten exports, declaration references, API reports, `.d.ts` rollups, and `.api.json` doc model output. TypeDoc is the browsing renderer because it converts TypeScript source into a reflection project and renders HTML or JSON by default, with Markdown available through a plugin.

| [INDEX] | [CONCERN]                     | [API_EXTRACTOR]                                                | [TYPEDOC]                                                                                       | [STANDARD_IMPACT]                                                                  |
| :-----: | :---------------------------- | :------------------------------------------------------------- | :---------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------- |
|   [1]   | Strict TSDoc package API gate | primary owner                                                  | secondary parser and renderer                                                                   | keep “strict comment canon” for package APIs only when `api-extractor.json` exists |
|   [2]   | Browsing docs                 | emits `.api.json`; `api-documenter` can render basic Markdown  | primary HTML/JSON renderer; plugin route for Markdown                                           | call generated browsing output an API/reference mirror, not source comment truth   |
|   [3]   | Release tags                  | enforces missing, extra, and incompatible release tags         | displays and filters some modifier tags, but `@public` has TypeDoc-specific visibility behavior | distinguish API Extractor release contract from TypeDoc visibility behavior        |
|   [4]   | Link resolution               | TSDoc declaration references relative to entry point           | TypeScript link resolution by default; declaration references as fallback                       | require resolvable `{@link ...}` and note which resolver owns the check            |
|   [5]   | Generated model               | `.api.json` doc model for downstream API docs                  | TypeDoc JSON reflection model                                                                   | never hand-maintain one model as the other                                         |
|   [6]   | Generated mirrors             | generated API report, rollup, doc model, `tsdoc-metadata.json` | generated HTML/JSON/Markdown output                                                             | edit source comments/config, then regenerate; do not edit mirrors independently    |

Standard wording should avoid saying TypeDoc consumes API Extractor's `.api.json` by default. API Extractor's companion is `api-documenter`; TypeDoc owns its own converted reflection model unless a custom pipeline or plugin explicitly bridges models.

## [3][API_EXTRACTOR]

API Extractor is the better standard anchor for release-tag enforcement and package-public API shape. Its docs state that `docModel.enabled` writes extracted signatures and doc comments into an intermediary `.api.json` doc model, defaulting to `<projectFolder>/temp/<unscopedPackageName>.api.json`; `api-documenter` can then generate basic Markdown from that model. Its `api-extractor.json` docs also define `docModel.apiJsonFilePath`, `docModel.includeForgottenExports`, and `docModel.projectFolderUrl`, so generated API documentation can carry source links when configured.

API Extractor release tags form the package maturity contract. It requires API declarations to carry one of `@alpha`, `@beta`, `@public`, or `@internal` by default at the outer container level, reports `ae-extra-release-tag` when a comment has more than one release tag, and reports `ae-incompatible-release-tags` when a more visible API references a less visible type. The visibility order is `@public` > `@beta` > `@alpha` > `@internal`, and `.d.ts` trimming uses that order for alpha, beta, and public rollups.

API Extractor assumes a single `mainEntryPointFilePath` for `.d.ts` rollup generation. Multi-entry packages can still use API reports and documentation by pointing `mainEntryPointFilePath` at a dummy entrypoint that reexports the public entrypoints, but the standard should not promise rollup correctness for path-based imports without that adapter.

Generated mirror boundary: the `.api.md` report, `.d.ts` rollup, `.api.json` doc model, and `tsdoc-metadata.json` are generated artifacts. A future Rasm standard should say to repair source exports, TSDoc comments, `api-extractor.json`, or package entrypoint shape first, then regenerate or compare generated output.

## [4][TYPEDOC]

TypeDoc is a renderer and validation layer, not the same contract gate as API Extractor. Its output options ship `html` and `json` by default, and the `outputs` array can render multiple targets; Markdown output requires `typedoc-plugin-markdown`. Its validation defaults warn on `notExported`, `invalidLink`, `invalidPath`, `rewrittenLink`, and `unusedMergeModuleWith`, while `notDocumented` is off unless paired with `requiredToBeDocumented`.

TypeDoc link behavior differs from API Extractor. `{@link ...}` resolves with TypeScript's symbol-in-scope rules by default through `useTsLinkResolution`, then falls back to declaration references if TypeScript resolution fails or the option is off. `@see SomeSymbol` is not enough for a link in TypeDoc; a link-bearing `@see` entry needs an explicit `{@link ...}`. External symbol links require `externalSymbolLinkMappings` when TypeDoc cannot infer a stable target.

TypeDoc tag behavior is broad but not identical to API Extractor's release contract. It supports a configured tag set from `tsdoc.json` or the `blockTags`, `inlineTags`, and `modifierTags` options and warns on unknown tags. `@public` is especially dangerous as a standards example because TypeDoc says it generally should not be used and treats it as visibility override behavior for compatibility, not pure release maturity. API authors using TypeDoc alone are encouraged by TypeDoc to treat exported members as public unless they carry `@alpha`, `@beta`, `@experimental`, or `@internal`.

Generated mirror boundary: TypeDoc generated HTML, JSON, and plugin Markdown are mirrors of source comments, signatures, options, and TypeDoc's conversion model. The source standard may name TypeDoc as a browsing renderer, but generated TypeDoc pages belong under `api.md` generated-library-reference rules and must not be copied into source comments or hand-maintained reference leaves.

## [5][TSDOC_LINKS]

TSDoc is the syntax substrate, but each tool resolves it differently. TSDoc defines `@link` as an inline tag for URLs and API-item references and warns that declaration-reference notation is still evolving. API Extractor resolves declaration references relative to an entrypoint, not the source file or current declaration scope. TypeDoc defaults to TypeScript symbol resolution before declaration-reference fallback, so a link that works in TypeDoc can still fail API Extractor if it relies on local lexical scope instead of package entrypoint resolution.

Use this decision rule for standards language:
- If API Extractor is configured, require API Extractor-valid declaration references for package-public API comments and treat `ae-unresolved-link`, release-tag, and forgotten-export diagnostics as the strict gate.
- If only TypeDoc is configured, require TypeDoc validation with `invalidLink`, `notExported`, `invalidPath`, and `rewrittenLink`, and document `useTsLinkResolution` when link semantics matter.
- If neither is configured, describe TSDoc/TypeDoc/API Extractor as candidate generator profiles, not as current Rasm gates.

## [6][STANDARDS_IMPACT]

The current `code-documentation.md` TypeScript capsule is directionally correct where it says source comments should be TSDoc, `{@link ...}` should be resolvable, and release tags should be used at most once. It needs sharper generator boundaries if active standards are edited later.

[KEEP]:
- Keep source comments as semantic source truth for public TypeScript APIs.
- Keep `@param`, `@returns`, `@typeParam`, `@remarks`, `@throws`, `{@link ...}`, `@see`, `{@inheritDoc ...}`, and `@packageDocumentation` as the source-comment vocabulary, subject to the configured parser.
- Keep generated package catalogs and copied TypeDoc output out of active prose.
- Keep release tags tied to external support or generated-reference contracts.

[REFINE]:
- Replace a broad “API Extractor is the strict comment canon, and TypeDoc is the browsing renderer” claim with a conditional statement: API Extractor is strict only when a package adopts `api-extractor.json`; TypeDoc validates and renders only when `typedoc` config or command output exists.
- Add that `@public` has conflicting tool semantics: API Extractor treats it as a release tag, while TypeDoc warns that its `@public` behavior changes effective visibility and generally should not be used for ordinary exported members.
- State that TypeDoc Markdown is plugin-owned, not built-in default output.
- State that API Extractor's doc model is `.api.json`, while TypeDoc JSON is a separate reflection model.
- Tie generated library reference placement to `api.md`, with `code-documentation.md` owning source-comment semantics only.

[REJECT]:
- Do not say TypeDoc enforces API Extractor release-tag policy.
- Do not say API Extractor renders rich browsing docs by itself; `api-documenter` produces basic Markdown, and custom pipelines may consume the doc model.
- Do not say TSDoc declaration references are always source-file-relative.
- Do not require `@public` on all exported TypeScript declarations unless API Extractor or a local release policy requires it.
- Do not hand-edit API reports, `.api.json`, TypeDoc JSON, generated HTML, or generated Markdown as independent standards truth.

## [7][SOURCE_LINKS]

Primary sources:
- [TypeDoc validation options](https://typedoc.org/documents/Options.Validation.html): validation defaults, `treatWarningsAsErrors`, `treatValidationWarningsAsErrors`, and `requiredToBeDocumented`.
- [TypeDoc tags](https://typedoc.org/documents/Tags.html): supported tag groups, custom tag configuration, and unknown-tag warnings.
- [TypeDoc `{@link}`](https://typedoc.org/documents/Tags.__link_.html): TypeScript link resolution default and declaration-reference fallback.
- [TypeDoc comments options](https://typedoc.org/documents/Options.Comments.html): `commentStyle`, `useTsLinkResolution`, `externalSymbolLinkMappings`, `notRenderedTags`, and tag list options.
- [TypeDoc output options](https://typedoc.org/documents/Options.Output.html): built-in `html` and `json` output and plugin-owned Markdown output.
- [TypeDoc `@public`](https://typedoc.org/documents/Tags._public.html): TypeDoc-specific warning that `@public` is a visibility override and generally should not be used.
- [API Extractor generating docs](https://api-extractor.com/pages/setup/generating_docs/): `docModel.enabled`, default `.api.json` path, and `api-documenter` Markdown route.
- [API Extractor config](https://api-extractor.com/pages/configs/api-extractor_json/): `docModel`, `.d.ts` rollup, `tsdocMetadata`, and trimming output paths.
- [API Extractor declaration references](https://api-extractor.com/pages/tsdoc/declaration_references/): entrypoint-relative declaration-reference resolution and unsupported advanced features.
- [API Extractor rollup configuration](https://api-extractor.com/pages/setup/configure_rollup/): single-entrypoint assumption and release-tag-based trimming.
- [API Extractor `ae-missing-release-tag`](https://api-extractor.com/pages/messages/ae-missing-release-tag/), [`ae-extra-release-tag`](https://api-extractor.com/pages/messages/ae-extra-release-tag/), [`ae-incompatible-release-tags`](https://api-extractor.com/pages/messages/ae-incompatible-release-tags/), and [`ae-forgotten-export`](https://api-extractor.com/pages/messages/ae-forgotten-export/): release tag and public API validation diagnostics.
- [API Extractor `tsdoc.json`](https://api-extractor.com/pages/configs/tsdoc_json/): shared TSDoc configuration and `.api.json` serialization of custom tags.
- [TSDoc `@link`](https://tsdoc.org/pages/tags/link/) and [TSDoc spec overview](https://tsdoc.org/pages/spec/overview/): syntax substrate and evolving declaration-reference notation.

## [8][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, shared standards, `docs/standards/reference/api.md`, `docs/standards/reference/reference.md`, and `docs/standards/reference/code-documentation.md`.
- [x] Used current primary sources: official TypeDoc, API Extractor, TSDoc docs, Context7 current-doc queries, and npm package metadata.
- [x] Kept active standards unchanged.
- [x] Run path-limited Markdown whitespace validation after writing this report.
