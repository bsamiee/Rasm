# GENERAL 5 Lifecycle Research

Report path: `docs/standards/_reports/code-documentation-050626/track-general/05-general-lifecycle.md`
Focus: cross-language lifecycle, deprecation, release-tag semantics, support-matrix routing, and compatibility versus greenfield deletion.
Date: 2026-06-05.

## Scope

This report is source material for a later active-standard edit. It does not edit `docs/standards/reference/code-documentation.md` or any other active standard.

The controlling local question is whether `code-documentation.md` needs stronger GENERAL 5 guidance around lifecycle tags, cross-language deprecation markers, support-matrix handoff, and deletion of stale compatibility surfaces in greenfield code.

## Read Transcript

Local instruction and target files read fully:

- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/style-guide.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`

Adjacent standards read for routing and proof context:

- `docs/standards/reference/support-matrix.md`
- `docs/standards/reference/reference.md`
- `docs/standards/proof.md`

Targeted local scans:

- `rg -n "GENERAL 5|lifecycle|deprecat|release|support matrix|support-matrix|compatib|greenfield|delete|stale|version|tag|obsolete|breaking|legacy" docs/standards -g '*.md'`
- `nl -ba docs/standards/reference/code-documentation.md | sed -n '1,120p;280,380p;390,430p'`
- `nl -ba docs/standards/reference/support-matrix.md | sed -n '1,180p;320,410p'`
- `nl -ba docs/standards/README.md | sed -n '1,170p'`
- `nl -ba docs/standards/AGENTS.md | sed -n '1,90p'`

External primary-source checks:

- Microsoft Learn, .NET `ObsoleteAttribute`, `view=net-10.0`.
- Microsoft Learn, recommended C# XML documentation tags, updated 2026-02 class of source freshness from search result.
- TSDoc `@public`, `@alpha`, `@beta`, `@internal`, and `@deprecated` tag pages.
- API Extractor release-tag diagnostics `ae-extra-release-tag`, `ae-different-release-tags`, and `ae-missing-release-tag`.
- PEP 702.
- Python 3.15 `warnings.deprecated` documentation.
- OpenAPI Specification v3.2.0.
- RFC 9745, The Deprecation HTTP Response Header Field.
- Semantic Versioning 2.0.0.
- PostgreSQL 18 `COMMENT`.
- GNU Bash Reference Manual 5.3.

## Source Notes

Local source notes:

- `code-documentation.md` already routes support lifecycle to `support-matrix.md` from the opening route-away paragraph and from the decision router.
- The produced-shape contract already has a conditional lifecycle-tag record that appears only when an external support contract consumes a marker.
- `code-documentation.md` already defines closed lifecycle state classes: `supported`, `preview`, `deprecated`, `removed`, and `internal`.
- `code-documentation.md` already says greenfield internal stale surfaces should be deleted or replaced rather than preserved with lifecycle tags.
- `support-matrix.md` already owns support profiles, support regimes, lifecycle baselines, status vocabulary, distinct lifecycle dates, deprecation records, migration anchors, and placement.
- `README.md` already says dead documentation should be deleted or replaced and that old compatibility notes require live product support plus evidence.
- `AGENTS.md` already rejects stale compatibility prose, legacy aliases, and duplicated owner bodies.

External source notes:

- .NET `ObsoleteAttribute` exposes compiler-facing lifecycle data: `IsError`, `Message`, `DiagnosticId`, and `UrlFormat`. This supports treating `[Obsolete]` as a warning or error contract with documentation routing, not just prose.
- C# XML `cref` references are compiler-checked for code elements, while `href` is the link form for external URLs. This supports the current code-reference routing.
- TSDoc treats `@deprecated` as a block tag for APIs no longer supported and expects a recommended alternative. It recursively applies to members of a deprecated container.
- TSDoc release tags are modifier tags. `@public` marks supported contract, `@alpha` and `@beta` mark provisional release stages, and `@internal` marks API not planned for third-party developers.
- API Extractor treats `@public`, `@beta`, `@alpha`, and `@internal` as release tags, reports more than one release tag on one comment, and by default expects API declarations to have release tags.
- PEP 702 adds `warnings.deprecated()` and expects type checkers to diagnose uses. Its message may include removal version and replacement information.
- Python 3.15 docs state the `warnings.deprecated` message is saved in `__deprecated__` and cite PEP 702.
- OpenAPI v3.2.0 treats deprecated OAS fields or features as still usable and directs authors to use newer replacement fields where documented.
- RFC 9745 says HTTP deprecation communicates lifecycle, can link deprecation documentation, can be combined with Sunset for non-operational timing, and does not by itself change resource behavior.
- SemVer 2.0.0 requires a minor version increment when public API functionality is marked deprecated and a major version increment for backward-incompatible public API changes.
- PostgreSQL 18 `COMMENT` stores comments on database objects, comments are viewable through `psql` describe and description functions, and no security mechanism restricts viewing comments.
- GNU Bash Reference Manual 5.3 is current enough to support the local Bash 5.3 baseline, but it does not add a native lifecycle-tag mechanism.

## Findings

### Finding 1: The local lifecycle direction is mostly correct.

The active standard already expresses the right rule: source comments carry lifecycle signals only when a caller-visible, generator-visible, or support-policy contract consumes them. This aligns with .NET, TSDoc, Python, OpenAPI, HTTP, SemVer, PostgreSQL, and Bash source behavior.

Confidence: high.

Recommendation: no broad rewrite. Preserve the current section and only tighten trigger and field language.

### Finding 2: Lifecycle tag semantics need a cross-language field contract.

The current lifecycle section says every tag names replacement path, behavior delta, removal or migration condition, consuming route, and review trigger. That is correct, but the language capsules do not consistently name the marker-specific fields that make this check actionable:

- C# `[Obsolete]`: warning or error behavior, message, diagnostic ID, documentation URL format, replacement, and support route.
- TypeScript release tags: exactly one of `@public`, `@beta`, `@alpha`, or `@internal` when API Extractor owns the package contract; `@deprecated` remains a separate block tag that needs an alternative.
- Python `warnings.deprecated`: message, static diagnostic expectation, optional removal version, replacement, and runtime warning behavior where used.
- OpenAPI `deprecated`: machine-readable field plus replacement route; deprecation alone does not remove the operation or schema member.
- HTTP `Deprecation`: deprecation date, optional deprecation link, optional Sunset date, scope, and explicit behavior caveat.
- PostgreSQL `COMMENT ON`: lifecycle meaning only when object metadata is the durable catalog route; never put sensitive details in comments.
- Bash: no native lifecycle tag; lifecycle belongs in script contract comments only if the script surface is externally supported, otherwise delete or replace stale functions.

Confidence: high.

Recommendation: add one compact `Lifecycle marker fields` record or table inside `code-documentation.md` section 7. Keep it marker-focused and avoid restating support-matrix status fields.

### Finding 3: `@deprecated` and release tags are currently adjacent but not identical.

The TypeScript capsule says `@public`, `@beta`, `@alpha`, `@internal`, and `@deprecated` are external support or release contracts, and then says use at most one release tag per exported API. External source evidence separates release tags from `@deprecated`: API Extractor names the release tag set as `@public`, `@beta`, `@alpha`, and `@internal`, while TSDoc defines `@deprecated` as a block tag.

Confidence: high.

Recommendation: change the TypeScript capsule wording later from "`@public`, `@beta`, `@alpha`, `@internal`, and `@deprecated`: external support or release contract; use at most one release tag per exported API" to a split wording:

- `@public`, `@beta`, `@alpha`, and `@internal`: release-stage contract; use at most one per API item when API Extractor owns the package contract.
- `@deprecated`: deprecation contract; include the recommended alternative and support or migration route when the exported API remains available.

This is a `change` recommendation because the current sentence can be misread as classifying `@deprecated` as one of the mutually exclusive release tags.

### Finding 4: Support-matrix routing is already strong; code docs need only a sharper handoff trigger.

`support-matrix.md` already owns row-comparable lifecycle facts: version status, compatibility bounds, support phases, fix classes, lifecycle dates, deprecation records, migration anchors, and terminal-row retention rules. `code-documentation.md` should not import that body.

The code-doc handoff trigger should be explicit:

- Use source comments or generator-visible markers when the caller sees the warning at the symbol, operation, schema, script, or catalog object.
- Use support matrix rows when the reader asks whether a version, platform, feature, integration, package, or public surface remains supported and under what dates or conditions.
- Use migration how-to only for steps.
- Use roadmap only for future removal sequence.
- Use architecture only for current path-state consequences.

Confidence: high.

Recommendation: add a short route discriminator under `Lifecycle references`, not a new support table.

### Finding 5: Compatibility notes should remain exceptional.

The active standard and repo instructions converge: do not preserve stale paths or internal greenfield surfaces as lifecycle wrappers. External source behavior does not require compatibility preservation. Even RFC 9745 treats deprecation as communication while behavior stays the same; it does not mandate retaining removed resources forever.

Confidence: high.

Recommendation: preserve current greenfield deletion language. Add one accepted/rejected contrast that distinguishes a public package support window from an internal stale compatibility shim:

Accepted: external package API remains callable during a sourced support window and carries a generated warning plus replacement route.
Rejected: internal stale method gets `[Obsolete]` only to avoid updating callers.
Reason: support contracts warn external callers; greenfield internal code deletes or replaces stale surfaces.

### Finding 6: The closed state set is useful, but it should not become a support status vocabulary.

`code-documentation.md` state classes are comment-review states. `support-matrix.md` has display statuses and source phases. These should remain separate to avoid mixing source comments with row-level support policy.

Confidence: high.

Recommendation: no change to the closed `supported`, `preview`, `deprecated`, `removed`, `internal` state set. Add a sentence that support-matrix rows may map these signals to `Supported`, `Limited`, `Deprecated`, `End of support`, `Retired`, or `Unsupported`, but code comments do not define those display statuses.

## Add Recommendations

- Add a compact lifecycle-marker field record under `code-documentation.md` section 7:
  - `Marker`
  - `Consumes`
  - `Required source-comment fields`
  - `Generated or runtime signal`
  - `Support route`
  - `Delete instead when`

- Add a route discriminator:
  - source comment owns marker and caller-visible warning text.
  - generated API reference owns rendered marker.
  - support matrix owns support status, support phase, dates, compatibility bounds, terminal state, and row retention.
  - how-to owns migration steps.
  - roadmap owns future removal sequence.
  - architecture owns current path-state consequences.

- Add a TypeScript-specific correction that separates release tags from `@deprecated`.

- Add a greenfield deletion accepted/rejected contrast as described in Finding 5.

## Remove Recommendations

- Remove any wording that implies `@deprecated` is one of the mutually exclusive TSDoc/API Extractor release tags.
- Remove any future edit that tries to put support-matrix state definitions, lifecycle date fields, or support-channel fields inside code comments.
- Remove compatibility-preservation examples that do not name an external caller, package, generated reference, support matrix, or sourced compatibility policy.

## Change Recommendations

- Change "lifecycle tags preserve only external support contracts" only if needed to "lifecycle markers preserve only external support contracts and generator-visible warnings." This keeps runtime markers such as HTTP `Deprecation` and OpenAPI `deprecated` inside the rule without making comments a support policy.
- Change TypeScript capsule wording as noted in Finding 3.
- Change validation wording only if later editors need a stronger checklist item:
  - current: lifecycle-tag states come from closed vocabulary and serve external support contracts only.
  - proposed: lifecycle markers come from the closed vocabulary, carry replacement and support routes, and route dates or support status to the support matrix.

## No-Change Confirmations

- Keep `support-matrix.md` as the owner for support status, lifecycle dates, compatibility bounds, dependency floors, deprecation records, migration anchors, and terminal-row retention.
- Keep `README.md` as the owner for document-type routing, placement, lifecycle, and stale documentation deletion.
- Keep `proof.md` as the owner for freshness, evidence fields, and current-source requirements.
- Keep `style-guide.md` as the owner for wording and code-safe Markdown.
- Keep `agentic-documentation.md` as the owner for placement, salience, artifact separation, and provider behavior.
- Keep code comments limited to caller-visible semantics, generator-visible marker text, failure channels, resource contracts, and non-obvious rationale.
- Keep greenfield internal stale surfaces on the delete-or-replace path.

## Source List

- Microsoft Learn, `ObsoleteAttribute`: https://learn.microsoft.com/en-us/dotnet/api/system.obsoleteattribute?view=net-10.0
- Microsoft Learn, recommended XML documentation tags: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
- TSDoc `@deprecated`: https://tsdoc.org/pages/tags/deprecated/
- TSDoc `@public`: https://tsdoc.org/pages/tags/public/
- TSDoc `@alpha`: https://tsdoc.org/pages/tags/alpha/
- TSDoc `@beta`: https://tsdoc.org/pages/tags/beta/
- TSDoc `@internal`: https://tsdoc.org/pages/tags/internal/
- API Extractor `ae-extra-release-tag`: https://api-extractor.com/pages/messages/ae-extra-release-tag/
- API Extractor `ae-different-release-tags`: https://api-extractor.com/pages/messages/ae-different-release-tags/
- API Extractor `ae-missing-release-tag`: https://api-extractor.com/pages/messages/ae-missing-release-tag/
- PEP 702: https://peps.python.org/pep-0702/
- Python 3.15 warnings: https://docs.python.org/3.15/library/warnings.html
- OpenAPI Specification v3.2.0: https://spec.openapis.org/oas/latest
- RFC 9745: https://www.rfc-editor.org/rfc/rfc9745
- Semantic Versioning 2.0.0: https://semver.org/
- PostgreSQL 18 `COMMENT`: https://www.postgresql.org/docs/18/sql-comment.html
- GNU Bash Reference Manual 5.3: https://www.gnu.org/software/bash/manual/html_node/index.html

## Validation

- Report file created only under `_reports/`.
- Active standards not edited.
- Current primary sources checked for drift-prone lifecycle and release-tag claims.
- No static, test, bridge, or generated-reference rails run because this is a research report only.
