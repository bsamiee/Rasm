# [TYPESCRIPT_01_TSDOC_SPEC_RESEARCH]

This report verifies the current TSDoc specification and adjacent official tool behavior for the TypeScript capsule in `docs/standards/reference/code-documentation.md`. It recommends only targeted clarification work for the active standard; it does not edit the active standards corpus.

## [1][SCOPE]

Focus: TypeScript source documentation, TSDoc tag kinds, block tags, inline tags, modifier tags, release tags, `@throws`, `{@inheritDoc ...}`, and `@packageDocumentation`.

Assigned output: `docs/standards/_reports/code-documentation-050626/track-typescript/01-typescript-tsdoc-spec.md`.

Active standard inspected:
- `docs/standards/reference/code-documentation.md`: current TypeScript capsule and cross-language anti-patterns.
- `docs/standards/style-guide.md`: prose, links, code-safe Markdown, examples, and final proofing.
- ``: edge placement, evidence-before-synthesis, artifact separation, and provider/tooling proof.
- `docs/standards/README.md`: reader-need routing, source precedence, placement, lifecycle, and mixed-file routing.
- `docs/standards/AGENTS.md`: read scope, rule owners, active-corpus constraints, and close checks.

No active standard was edited.

## [2][SOURCE_SET]

[PRIMARY_CURRENT_SOURCES]:
- TSDoc, [What is TSDoc?](https://tsdoc.org/), accessed 2026-06-05.
- TSDoc, [Tag kinds](https://tsdoc.org/pages/spec/tag_kinds/), accessed 2026-06-05.
- TSDoc, [Standardization groups](https://tsdoc.org/pages/spec/standardization_groups/), accessed 2026-06-05.
- TSDoc, [`@remarks`](https://tsdoc.org/pages/tags/remarks/), accessed 2026-06-05.
- TSDoc, [`@param`](https://tsdoc.org/pages/tags/param/), accessed 2026-06-05.
- TSDoc, [`@typeParam`](https://tsdoc.org/pages/tags/typeparam/), accessed 2026-06-05.
- TSDoc, [`@returns`](https://tsdoc.org/pages/tags/returns/), accessed 2026-06-05.
- TSDoc, [`@throws`](https://tsdoc.org/pages/tags/throws/), accessed 2026-06-05.
- TSDoc, [`{@link}`](https://tsdoc.org/pages/tags/link/), accessed 2026-06-05.
- TSDoc, [`@see`](https://tsdoc.org/pages/tags/see/), accessed 2026-06-05.
- TSDoc, [`{@inheritDoc}`](https://tsdoc.org/pages/tags/inheritdoc/), accessed 2026-06-05.
- TSDoc, [`@packageDocumentation`](https://tsdoc.org/pages/tags/packagedocumentation/), accessed 2026-06-05.
- TSDoc, [`@alpha`](https://tsdoc.org/pages/tags/alpha/), [`@beta`](https://tsdoc.org/pages/tags/beta/), [`@public`](https://tsdoc.org/pages/tags/public/), and [`@internal`](https://tsdoc.org/pages/tags/internal/), accessed 2026-06-05.
- API Extractor, [Doc comment syntax](https://api-extractor.com/pages/tsdoc/doc_comment_syntax/), accessed 2026-06-05.
- API Extractor, [`@packageDocumentation`](https://api-extractor.com/pages/tsdoc/tag_packagedocumentation/), accessed 2026-06-05.
- API Extractor, [`ae-extra-release-tag`](https://api-extractor.com/pages/messages/ae-extra-release-tag/), [`ae-missing-release-tag`](https://api-extractor.com/pages/messages/ae-missing-release-tag/), [`ae-incompatible-release-tags`](https://api-extractor.com/pages/messages/ae-incompatible-release-tags/), [`ae-internal-mixed-release-tag`](https://api-extractor.com/pages/messages/ae-internal-mixed-release-tag/), [`ae-misplaced-package-tag`](https://api-extractor.com/pages/messages/ae-misplaced-package-tag/), [`ae-unresolved-inheritdoc-reference`](https://api-extractor.com/pages/messages/ae-unresolved-inheritdoc-reference/), and [`ae-cyclic-inherit-doc`](https://api-extractor.com/pages/messages/ae-cyclic-inherit-doc/), accessed 2026-06-05.
- TypeDoc, [Tags](https://typedoc.org/documents/Tags.html), [`@throws`](https://typedoc.org/documents/Tags._throws.html), [`{@inheritDoc}`](https://typedoc.org/documents/Tags.__inheritDoc_.html), [`@packageDocumentation`](https://typedoc.org/documents/Tags._packageDocumentation.html), and [TSDoc Support](https://typedoc.org/documents/Doc_Comments.TSDoc_Support.html), accessed 2026-06-05.

[TOOL_DOCS_CHECK]:
- Context7 resolved `/microsoft/tsdoc` and `/typestrong/typedoc` and returned current documentation excerpts for tag kinds, custom tag configuration, standard tags, and TypeDoc tag support. Context7 agreed with the official pages above, but the report relies on the official TSDoc, API Extractor, and TypeDoc pages as primary sources.

[LOCAL_REPO_SOURCES]:
- `docs/standards/reference/code-documentation.md:195-228`: current TypeScript capsule.
- `docs/standards/reference/code-documentation.md:331-366`: lifecycle and anti-pattern rules that affect release tags and `@throws`.
- `docs/standards/reference/code-documentation.md:383-417`: validation rules for syntax, generation, references, and docs-only proof.
- `docs/standards/style-guide.md:15-24`: wording precedence.
- `docs/standards/style-guide.md:124-136`: code-safe Markdown and link rules.
- `:52-57`: evidence-before-synthesis rule.
- `:58-70`: artifact separation.
- `docs/standards/README.md:14-23`: source precedence.
- `docs/standards/README.md:48-59`: code documentation route as source-symbol reference.
- `docs/standards/AGENTS.md:42-50`: edit invariants and omit-absent-fields rule.

## [3][CURRENT_STANDARD_SNAPSHOT]

The current TypeScript capsule is directionally correct and already rejects the main high-risk errors:
- It says TSDoc applies to exported TypeScript APIs that form package, module, service, schema, model, runner, or testkit contracts.
- It assigns machine shape to TypeScript syntax, exported schemas, models, and `Effect<A, E, R>`.
- It names API Extractor as strict comment canon and TypeDoc as browsing renderer.
- It keeps summary before block tags.
- It limits `@throws` to actual thrown exceptions or terminal-runner edges.
- It requires `{@link ...}` or linked `@see` routes.
- It limits `{@inheritDoc ...}` to exact inherited contracts.
- It limits `@packageDocumentation` to package entrypoint contracts.
- It says release tags are external support or release contracts and allows at most one release tag per exported API.
- It rejects JSDoc type-expression syntax, duplicate type info in `@param`, broad `@throws` for typed `E`, `@see SomeSymbol` without a link route, generated package catalogs, release tags on internal greenfield surfaces, copied TypeDoc output, and Promise comments that hide terminal `Effect` boundaries.

The weak point is not wrong doctrine. The weak point is that the TypeScript capsule compresses several TSDoc grammar and API Extractor validation rules into single clauses, leaving future agents to infer the difference between TSDoc syntax, TSDoc standardization group, API Extractor enforcement, and TypeDoc renderer compatibility.

## [4][FINDINGS]

### [4.1][TSDOC_GRAMMAR]

Finding: TSDoc has three tag syntax kinds: block tags, modifier tags, and inline tags. Tag names start with `@` and use ASCII camelCase. A tag is defined as exactly one syntax kind; a tag such as `@link` must not be written as a block tag because it is an inline tag.

Evidence:
- TSDoc's tag-kind page defines the three syntax kinds and says a tag has exactly one form.
- TSDoc says block tags should appear as the first element on a line. The content after a block tag continues until the next block tag or modifier tag.
- TSDoc says content before the first block tag is the summary section.
- TSDoc says modifier tags indicate a special API quality, normally have empty content, and appear at the bottom in normalized form.
- TSDoc says inline tags appear inside Markdown-like content and are surrounded by braces.

Recommendation:
- Change the TypeScript capsule to state the syntax-kind split explicitly before listing `@remarks`, `@typeParam`, `@param`, `@returns`, `@throws`, `{@link ...}`, `@see`, `{@inheritDoc ...}`, and `@packageDocumentation`.
- Add one short rule that `{@link ...}` and `{@inheritDoc ...}` are inline tags, while `@see` and `@throws` are block tags and `@packageDocumentation` plus release tags are modifier tags.

Confidence: high.

### [4.2][STANDARDIZATION_GROUPS]

Finding: TSDoc classifies tags by standardization group as `Core`, `Extended`, or `Discretionary`. Core tags are expected across compatible tools. Extended tags are optional, but their syntax and semantics should follow the standard when supported. Discretionary tags have specified syntax, while semantics are implementation-specific or only similar across tools.

Evidence:
- TSDoc's standardization-groups page defines `Core`, `Extended`, and `Discretionary`.
- `@packageDocumentation`, `@remarks`, `@param`, `@typeParam`, `@returns`, and `{@link}` are core tags in the inspected pages.
- `@throws`, `@see`, and `{@inheritDoc}` are extended tags in the inspected pages.
- `@alpha`, `@beta`, `@public`, and `@internal` are discretionary modifier tags in the inspected pages.

Recommendation:
- Change the TypeScript capsule to separate "TSDoc syntax accepted by the spec" from "API Extractor package-API policy." This prevents agents from treating extended or discretionary TSDoc tags as universally enforced by every renderer.
- Keep API Extractor as the strict canon when Rasm config proves API Extractor owns the package API surface. Otherwise, phrase API Extractor rules as the reference implementation and package-governance dialect, not as generic TSDoc grammar.

Confidence: high.

### [4.3][COMMENT_STRUCTURE]

Finding: API Extractor's doc-comment syntax page matches TSDoc's structure: summary before first block tag, optional `@remarks`, additional block tags, modifier tags at the end, and inline tags inside sections. It parses comments only when the comment appears in emitted `.d.ts`, begins with `/**`, and appears immediately before an exported declaration or at the top of the entrypoint with `@packageDocumentation`.

Evidence:
- API Extractor says the comment must appear in emitted `.d.ts`, begin with `/**`, and be immediately before an exported declaration or be the top entrypoint `@packageDocumentation` comment.
- API Extractor says only the closest declaration comment block is considered when multiple comments precede an export.
- API Extractor names summary, remarks, additional blocks, modifier tags, and inline tags as the comment anatomy.

Recommendation:
- Change the capsule's toolchain sentence to add the emitted `.d.ts` and adjacency constraints only if active Rasm TypeScript packages are actually using API Extractor. If no active API Extractor configuration exists, preserve the current generic wording and route this fact to generated-reference proof.
- Add a no-change note elsewhere in the report or future edit that TypeScript declaration syntax remains the machine-shape source; TSDoc is not a type annotation substitute.

Confidence: medium-high. The rule is official API Extractor behavior, but whether it should become durable Rasm standard text depends on local package configuration proof.

### [4.4][BLOCK_TAGS]

Finding: The current capsule lists the correct block-tag family, but it should better distinguish TSDoc grammar from Rasm semantic ownership.

Evidence:
- `@remarks` is a core block tag that starts the detailed remarks section after the summary.
- `@param` is a core block tag with syntax `@param name - description`.
- `@typeParam` is a core block tag with syntax `@typeParam name - description`.
- `@returns` is a core block tag for a function's returned value.
- `@throws` is an extended block tag for exception types that a function or property may throw.
- `@see` is an extended block tag for related references; TSDoc requires explicit `{@link}` for hyperlinks.

Recommendation:
- Add grammar bullets only where they prevent misuse:
  - `@param <name> - <semantic obligation>` and `@typeParam <name> - <semantic relationship>`.
  - `@see` is a block tag, but linked references inside it still use `{@link ...}`.
  - `@throws` is repeatable by exception type and does not constrain the runtime to throw only documented types.
- Keep existing semantic policy that comments must not echo TypeScript types, parameter names, or carrier shape.

Confidence: high.

### [4.5][INLINE_TAGS]

Finding: `{@link ...}` and `{@inheritDoc ...}` are inline tags. The current capsule is correct to require resolvable public references and exact inherited contracts, but it does not mention that TSDoc declaration-reference notation is still not finalized.

Evidence:
- TSDoc says `{@link}` creates hyperlinks to documentation pages, URLs, or API items and supports declaration references.
- TSDoc says declaration-reference notation has not been finalized.
- TSDoc says `{@inheritDoc ...}` is an inline tag that copies documentation from another API item.
- TypeDoc says it recognizes `{@inheritDoc}` and also parses block-form `@inheritDoc` for JSDoc compatibility, but the TSDoc standard defines it as inline.

Recommendation:
- Change the active capsule to prefer `{@inheritDoc ...}` only, not bare `@inheritDoc`, because the standard should teach TSDoc syntax even if TypeDoc is permissive.
- Add a short caution that declaration-reference spelling must be resolved by the configured toolchain, because TSDoc's reference notation is still not finalized and TypeDoc documents its own parser behavior.
- Keep the rejection of `@see SomeSymbol` without `{@link ...}`.

Confidence: high.

### [4.6][RELEASE_TAGS]

Finding: TSDoc's release tags are discretionary modifier tags, while API Extractor makes them operational for package API review, `.d.ts` trimming, and compatibility checks. API Extractor's release tag set is `@public`, `@beta`, `@alpha`, and `@internal`; TSDoc also has `@experimental`, and TSDoc marks it as a synonym of `@beta`.

Evidence:
- TSDoc classifies `@alpha`, `@beta`, `@public`, and `@internal` as discretionary modifier tags.
- TSDoc says `@beta` has synonym `@experimental`.
- API Extractor says the four release tags are `@internal`, `@alpha`, `@beta`, and `@public`.
- API Extractor says a documentation comment should contain at most one release tag.
- API Extractor says release tags are inherited from containing declarations.
- API Extractor says it can report missing release tags for API declarations and incompatible release-tag references.
- API Extractor says `@deprecated` can combine with any release tag and is not one of the four release tags.

Recommendation:
- Change `@public`, `@beta`, `@alpha`, `@internal`, and `@deprecated`: external support or release contract; use at most one release tag per exported API.
- To: `@public`, `@beta`, `@alpha`, and `@internal` are API Extractor release tags; use at most one per exported API when the package API rail consumes them. `@deprecated` is a lifecycle warning that may combine with a release tag only when a support contract names replacement path, behavior delta, and removal or migration trigger.
- Add `@experimental` only as a route-away note: TSDoc treats it as a `@beta` synonym, but API Extractor's release-tag set is the four-tag set. Prefer `@beta` unless a configured toolchain or package policy consumes `@experimental`.

Confidence: high.

### [4.7][THROWS]

Finding: TSDoc `@throws` is informational. It documents exception types that may be thrown, should use a separate block for each exception type, and does not restrict other exception types from being thrown. This supports the active Rasm rule that `@throws` belongs only to actual thrown exceptions or terminal-runner boundary edges, not typed `Effect` failure data.

Evidence:
- TSDoc describes `@throws` as an extended block tag for thrown exception types.
- TSDoc recommends one `@throws` block per exception type.
- TSDoc says the tag is informational and does not restrict other types from being thrown.
- TypeDoc's `@throws` page describes it as a block tag for exceptions thrown by a function or method.
- The active standard already distinguishes throwing surfaces from rail surfaces and rejects throw tags for typed rails.

Recommendation:
- Keep the current `@throws`: actual thrown exception or terminal-runner edge only.
- Add a precision clause: For an `Effect<A, E, R>` or other typed rail, document expected `E` variants in `@returns`, `@remarks`, or the rail contract, not `@throws`; reserve `@throws` for exceptions that escape the public boundary or Promise rejections created by a terminal runner.
- Add "repeat one `@throws` block per escaped exception class" only if example pressure justifies it; otherwise the current anti-pattern already carries the essential rule.

Confidence: high.

### [4.8][INHERITDOC]

Finding: TSDoc `{@inheritDoc ...}` copies only the summary, `@remarks`, `@param`, `@typeParam`, and `@returns`. It does not copy every tag, and comments that use `{@inheritDoc ...}` may not also specify a summary or `@remarks`. API Extractor validates unresolved and cyclic inherit-doc references.

Evidence:
- TSDoc's `@inheritDoc` page lists the copied components and says other tags such as `@defaultValue` or `@example` must be explicitly included.
- TSDoc says summary and `@remarks` may not be specified when `{@inheritDoc ...}` is present.
- API Extractor reports unresolved inherit-doc references.
- API Extractor reports inherit-doc cycles.
- TypeDoc follows the TSDoc copied-element set but recognizes bare `@inheritDoc` as a compatibility extension.

Recommendation:
- Change the active capsule's `{@inheritDoc ...}` clause from "only for exact inherited contracts" to a fuller rule: use `{@inheritDoc ...}` only when the inherited summary, remarks, parameters, type parameters, and returns are exact; add non-copied tags explicitly; do not pair it with local summary or `@remarks`.
- Add "resolved and acyclic under the configured generator" to the cross-reference rule if API Extractor or TypeDoc generated output is configured.

Confidence: high.

### [4.9][PACKAGE_DOCUMENTATION]

Finding: TSDoc `@packageDocumentation` is a core modifier tag for a comment that describes the whole NPM package, not an individual API item. It belongs in the entrypoint `.d.ts` file and should be the first `/**` comment encountered there. API Extractor says the package documentation comment should not have a release tag.

Evidence:
- TSDoc says `@packageDocumentation` describes an entire NPM package and should never describe an individual API item.
- TSDoc says it is found in the `.d.ts` entrypoint and should be first.
- API Extractor says API Documenter displays the content on the package page.
- API Extractor says the `@packageDocumentation` comment should not have a release tag.
- API Extractor reports misplaced package documentation when the tag appears outside the top of the configured entrypoint.
- TypeDoc treats it as a file comment marker and recommends placing it at the top of the file before imports.

Recommendation:
- Change the active capsule from "`@packageDocumentation` only for package entrypoint contracts" to a clearer package-entry rule:
  - Use one `@packageDocumentation` comment only for the package entrypoint or file-level package/module page.
  - Place it before imports as the first documentation comment for TypeDoc and ensure emitted `.d.ts` entrypoint placement for API Extractor.
  - Do not combine it with a release tag.
  - Do not use it for individual exported symbols.

Confidence: high.

### [4.10][TYPEDOC_RENDERER]

Finding: TypeDoc supports TSDoc tags but is not identical to API Extractor. It has default supported block, inline, and modifier tag lists; unknown tags emit warnings; custom tags can be configured through `tsdoc.json` or TypeDoc options. TypeDoc recognizes some compatibility forms, including bare `@inheritDoc`, that the Rasm standard should not teach as preferred TSDoc.

Evidence:
- TypeDoc says many JSDoc tags are unsupported because TypeScript can infer the same information directly from code.
- TypeDoc supports custom tags through `tsdoc.json` or `--blockTags`, `--inlineTags`, and `--modifierTags`.
- TypeDoc says inline tags include `@link` and `@inheritDoc`.
- TypeDoc says `@inheritDoc` follows TSDoc copied elements, but TypeDoc parses both inline `{@inheritDoc}` and block-style `@inheritDoc`.
- TypeDoc says `@packageDocumentation` marks a file-level comment and must be the first comment in the file.

Recommendation:
- Keep "API Extractor is the strict comment canon, and TypeDoc is the browsing renderer" if local Rasm TypeScript packages use both.
- If local package proof is absent, soften this to "API Extractor is the package-API validation profile when configured; TypeDoc is the browsing renderer when configured."
- Keep rejecting copied TypeDoc output in source comments.

Confidence: medium-high. Tool behavior is current; repo applicability depends on local TypeScript package configuration.

## [5][ADD_REMOVE_CHANGE_RECOMMENDATIONS]

### [5.1][ADD]

Add one compact grammar note to the TypeScript capsule:

```text
TSDoc tag forms: block tags start sections, modifier tags mark API qualities and carry no content, and inline tags appear inside text with braces. Use `@remarks`, `@param`, `@typeParam`, `@returns`, `@throws`, and `@see` as block tags; use `@public`, `@beta`, `@alpha`, `@internal`, and `@packageDocumentation` as modifier tags; use `{@link ...}` and `{@inheritDoc ...}` as inline tags.
```

Add one release-tag precision note:

```text
API Extractor release tags are `@public`, `@beta`, `@alpha`, and `@internal`; use at most one where the package API rail consumes release status. `@deprecated` is a lifecycle warning, not a release tag, and must name replacement path, behavior delta, and removal or migration trigger when used.
```

Add one inherit-doc precision note:

```text
Use `{@inheritDoc ...}` only when the inherited summary, `@remarks`, `@param`, `@typeParam`, and `@returns` are exact. Add non-copied tags explicitly, and do not combine `{@inheritDoc ...}` with local summary or `@remarks`.
```

Add one package-documentation precision note:

```text
Use `@packageDocumentation` only for the package entrypoint or file-level package page. Keep it before imports as the first documentation comment for TypeDoc, ensure emitted `.d.ts` entrypoint placement for API Extractor, omit release tags from that comment, and never use it for an individual API item.
```

### [5.2][CHANGE]

Change "API Extractor is the strict comment canon, and TypeDoc is the browsing renderer" only if local package configuration proves those tools are present. The safer generic form is:

```text
API Extractor is the strict package-API validation profile when configured, and TypeDoc is the browsing renderer when configured.
```

If local Rasm configuration proves both tools are the active TypeScript documentation rail, keep the current stronger sentence and add a generated-reference proof route rather than weakening it.

Change the `@throws` line only for precision:

```text
`@throws`: escaped exception or terminal-runner Promise rejection only; typed `E` failures belong in `@returns`, `@remarks`, or the rail contract.
```

Change the reference line to preserve TSDoc syntax and TypeDoc compatibility:

```text
`{@link ...}` and linked `@see` blocks provide public references; `{@inheritDoc ...}` applies only to exact inherited contracts, resolved by the configured generator, and copied fields must match.
```

### [5.3][REMOVE]

Remove no current TypeScript doctrine outright. The existing capsule is compact and mostly correct.

Potential removal only if local configuration disproves a tool:
- Remove or soften "API Extractor is the strict comment canon" if no package uses API Extractor.
- Remove or soften "TypeDoc is the browsing renderer" if no generated browsing renderer uses TypeDoc.

### [5.4][NO_CHANGE]

Keep these active-standard rules:
- Keep comments limited to caller-visible semantics that TypeScript declarations, schemas, models, or `Effect<A, E, R>` do not express.
- Keep the summary-before-block-tags rule.
- Keep `@param` and `@typeParam` as semantic obligations or relationships, not type echoes.
- Keep `@returns` as success, typed failure, environment, deferred execution, terminal behavior, or stream semantics.
- Keep `@throws` out of typed `E` failure documentation.
- Keep `{@link ...}` as the link route and reject `@see SomeSymbol` without a link.
- Keep `@packageDocumentation` out of individual symbol comments.
- Keep release tags tied to external support contracts, not internal greenfield preservation.
- Keep rejection of copied TypeDoc output and generated package catalogs.
- Keep TypeScript references resolved through the controlling toolchain or maintained route.

## [6][STANDARD_PATCH_SHAPE]

If the active standard is edited later, make the TypeScript capsule denser rather than longer. A replacement capsule could use this shape:

```text
Toolchain: TSDoc for exported TypeScript 7 `.ts` APIs that form a package, module, service, schema, model, runner, or testkit contract. TypeScript syntax, exported schemas, models, and `Effect<A, E, R>` carry machine shape; API Extractor is the strict package-API validation profile when configured, and TypeDoc is the browsing renderer when configured.

[TSDOC_FORMS]:
- Block tags start sections: `@remarks`, `@param`, `@typeParam`, `@returns`, `@throws`, and `@see`.
- Modifier tags mark API qualities and carry no content: `@public`, `@beta`, `@alpha`, `@internal`, and `@packageDocumentation`.
- Inline tags appear inside text with braces: `{@link ...}` and `{@inheritDoc ...}`.

[COMMENT_OWNS]:
- Summary: controlling purpose before the first block tag.
- `@remarks`: invariants, lifecycle, resource, retry, cancellation, security, terminal runner, or observability semantics.
- `@typeParam`: semantic generic relationship, not type-expression echo.
- `@param`: caller obligation, unit, ownership, trust boundary, or resource meaning.
- `@returns`: success, typed failure, environment, deferred execution, terminal behavior, or stream semantics.
- `@throws`: escaped exception or terminal-runner Promise rejection only; typed `E` failures belong in `@returns`, `@remarks`, or the rail contract.
- `{@link ...}` and linked `@see` blocks: resolvable public references.
- `{@inheritDoc ...}`: exact copied summary, `@remarks`, `@param`, `@typeParam`, and `@returns`; non-copied tags are explicit, and local summary or `@remarks` is omitted.
- `@packageDocumentation`: package entrypoint or file-level package page only; first documentation comment for TypeDoc and emitted `.d.ts` entrypoint placement for API Extractor; no release tag and no individual symbol use.
```

This patch shape preserves the existing semantic owner rules and adds only grammar that prevents misuse.

## [7][CONFIDENCE]

Overall confidence: high.

High confidence:
- TSDoc tag kinds, standardization groups, block-tag syntax, inline-tag syntax, modifier-tag syntax, `@throws` semantics, `{@inheritDoc ...}` copied fields, and `@packageDocumentation` placement are directly supported by current official TSDoc pages.
- API Extractor release-tag, package-documentation, missing-release-tag, extra-release-tag, incompatible-release-tag, misplaced-package-tag, unresolved-inherit-doc, and cyclic-inherit-doc behavior is directly supported by current official API Extractor pages.
- TypeDoc's compatibility behavior for `@inheritDoc`, tag lists, custom tags, and `@packageDocumentation` is directly supported by current official TypeDoc pages.

Medium confidence:
- Whether the active standard should say API Extractor and TypeDoc are configured Rasm rails requires local package-config proof outside this assigned source list. The current standard may already know this from repo context, but this report did not inspect package manifests or tool configs beyond the assigned standards files.

Low-risk conclusion:
- The active TypeScript capsule needs clarification, not doctrinal reversal.

## [8][NO_CHANGE_CONFIRMATIONS]

[CONFIRMED_KEEP]:
- Source comments own semantic contract only when declaration syntax, schema metadata, or generated type shape cannot express it.
- Public visibility creates a documentation question, not automatic comment creation.
- TypeScript type expressions belong in TypeScript, not TSDoc comment tags.
- `Effect<A, E, R>` failures are typed rail semantics and should not be documented as broad `@throws`.
- `@see` needs `{@link ...}` for resolvable hyperlinks under TSDoc.
- `@packageDocumentation` is package-level or file-level documentation, not individual API documentation.
- Release and lifecycle tags should serve external support, generated reference, package policy, or compatibility contracts, not greenfield stale-surface preservation.
- Generated API pages and package catalogs should be regenerated or linked, not copied into source comments.

[CONFIRMED_GAPS]:
- This report did not prove active Rasm TypeScript package configuration for API Extractor, TypeDoc, `tsdoc.json`, or generated API output. Any active-standard edit that turns tool behavior into a Rasm gate should inspect local manifests and command output first.
- This report did not run generated-reference, static, test, or bridge rails because the task was docs research and no active standard or executable source was changed.

## [9][VALIDATION_NOTES]

Read scope completed:
- `docs/standards/reference/code-documentation.md`: full file read.
- `docs/standards/style-guide.md`: full file read.
- ``: full file read.
- `docs/standards/README.md`: full file read.
- `docs/standards/AGENTS.md`: full file read.
- `CLAUDE.md` and root `AGENTS.md`: read for repo instruction chain.

Workspace note:
- `git status --short -- docs/standards/reference/code-documentation.md` showed that `docs/standards/reference/code-documentation.md` was already modified before this report was written. This report did not touch that active standard.

Close check:
- Active standards edited: no.
- Assigned report file edited: yes.
- Generated-reference rails run: no.
- Static, test, bridge, C#, TypeScript, Python, Bash, and SQL rails run: no.
- Recommended next verification if active standards are later edited: `git diff --check -- docs/standards`.
