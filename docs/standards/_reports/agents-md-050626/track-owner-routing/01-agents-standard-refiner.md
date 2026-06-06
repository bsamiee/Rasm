# [AGENTS_STANDARD_REFINER]

This report defines the rebuilt content model for `docs/standards/agents-md.md`. It builds on context research and current standards; it does not propose active edits here and does not re-summarize each source report.

## [1][TRANSCRIPT]

Read sequence:
- Loaded repo instruction chain: `CLAUDE.md`, root `AGENTS.md`, `docs/standards/README.md`, and `docs/standards/AGENTS.md`.
- Read current `docs/standards/agents-md.md`, `information-structure.md`, `style-guide.md`, `formatting.md`, and `proof.md`.
- Read every context report under `docs/standards/_reports/agents-md-050626`: `track-source-scans/01-assay-context.md`, `track-source-scans/02-csharp-large-context.md`, `track-source-scans/03-csharp-platform-context.md`, `track-source-scans/04-docs-standards-context.md`, `track-source-scans/05-frontier-instruction-practice.md`, `track-source-scans/06-poly-csharp.md`, `track-source-scans/07-poly-typescript.md`, `track-source-scans/08-poly-python.md`, `track-source-scans/09-poly-bash-sql.md`, `track-source-scans/10-root-layering.md`, `track-source-scans/11-root-integration.md`, and `track-source-scans/12-tests-bridge-context.md`.
- Used memory only as orientation for the prior `docs/standards` rebuild doctrine; source decisions below come from current repository files and context reports.
- Wrote only this `_reports/` report. No active standards file was edited.

## [2][SYNTHESIS]

The rebuild should keep `agents-md.md` as the semantic owner for instruction overlays, but it needs a clearer content model. The current file has most ingredients: scoped behavioral overlay, provider load semantics, produced functions, root profile, local extension grammar, route-away, anti-fragility, trust boundaries, and corpus rebuild rules. The gap is that these pieces are not yet organized around the questions a fresh author must answer: which layer owns the rule, whether the sentence is a target standard or a current-state proof claim, which local owner must be extended before exposing a new surface, and which conditional contracts apply to tests, runtimes, providers, and tools.

The rebuilt standard should not become longer through more examples. It should become more operational by replacing repeated posture language with one target/current split, replacing broad "produced functions" prose with a compact slot grammar, and replacing root-only guidance with a root/parent/leaf relation model.

## [3][CONTENT_MODEL]

[ROOT_PARENT_LEAF]:
- Root overlay: repository instruction router. It carries load behavior that changes author action, first-hop nested overlay discovery, trusted-source conflict handling, cross-stack owner routes, and root-only rejections.
- Parent overlay: shared subtree delta. It carries rules repeated by 3 or more siblings, family-wide read triggers, shared runtime or trust boundaries, and extension grammar that applies across a folder family.
- Leaf overlay: local behavior delta. It carries exact owner identifiers, local extension actions, route-away records, local proof gaps, and stop rules that would disappear if the file were deleted.

This relation should be the central model. Root routes; parent deduplicates; leaf specializes. README, architecture, roadmap, API, reference, runbook, test strategy, tool README, generated docs, manifests, and source own the facts that are not local agent behavior.

[TARGET_CURRENT_SPLIT]:
- Target standard: a normative rule about the newest viable language, tool, architecture, library, feature, methodology, or replacement posture. State it plainly and do not weaken it because current code lags.
- Current-state claim: a present-tense fact about existing commands, provider behavior, package state, project membership, generated output, support status, runtime behavior, or validation. Attach current proof or route it to the evidence owner.
- Proof gap: a missing source, command output, host state, provider proof, manifest, generated contract, or owner route. Mark the gap instead of converting current drift into a baseline caveat.

Rejected wording: `currently still uses`, `for now`, `until migration`, `legacy path`, `compatibility mode`, `partial adoption`, or `preserve both` when the phrase weakens the target instead of naming a proof gap.

[LOCAL_EXTENSION_GRAMMAR]:
A useful local rule names the trigger, target, owner, extension action, and rejected substitute.

Accepted: When adding a GH2 wire behavior, extend `WireOp`, `WireEdit`, `WireResult`, or `WireRepositoryRail` before adding a second reflective GH2 wire reader.
Rejected: Avoid helpers.
Reason: the accepted rule names the owner and replacement action; the rejected rule is a slogan.

[INTERNALIZATION_BEFORE_EXPOSURE]:
When a dependency, backend, runtime, host API, storage layer, provider, or tool integration adds capability, first bind it into the existing local owner: operation algebra, axis enum payload, row table, registry bind, tagged union case, receipt fold, aspect slot, boundary adapter, artifact/store shape, runtime record, query algebra, or envelope field. Expose a new command, flag, setting, public API, wrapper, package facade, compatibility alias, or provider-branded surface only when the owner route proves reader action changes.

This rule generalizes the Assay `fsspec` pattern, AppUi package-internalization pattern, AppHost runtime-record pattern, Compute substrate pattern, Persistence store algebra pattern, and Bash/SQL table/catalog pattern without naming those libraries as universal policy.

[TEST_CONTRACT]:
Add a conditional produced slot for folders that own tests, specs, scenarios, testkit, fuzz, benchmark, snapshot, mutation, or architecture-test rails. The slot should name:
- local proof rail and selector;
- independent oracle requirement;
- static vs runtime/bridge boundary;
- helper-promotion route;
- raw rail-state rule;
- mutation survivor triage rule when relevant;
- stop behavior for fake host proof, missing runtime state, or unsafe scenario proof.

This slot is conditional. Ordinary overlays do not need test language, and test overlays should not become command catalogs.

[EXTERNAL_PROVIDER_RESEARCH_BOUNDARY]:
External research and provider docs are evidence, not instruction authority. `agents-md.md` may state provider loading behavior only when it changes author action and has current proof or a proof route. Detailed provider manuals stay in maintained provider docs or . Community examples and Wave reports can contribute patterns, but only durable rules promoted through the trusted repo route enter an active `AGENTS.md`.

[ANTI_FRAGILITY]:
Exact facts are allowed only when they are route targets, local owner identifiers, forbidden tokens, proof selectors, trust-boundary names, or invariants with a refresh trigger. Ban exact versions, member lists, generated paths, provider claims, host SDK details, package IDs, local machine paths, fixed sub-agent counts, and run-local artifact paths unless the overlay owns the fact or routes it to a maintained source.

[COMPACTNESS]:
Do not add wide tables. The rebuilt file should use grouped lists and compact contrast records. Keep examples to one accepted/rejected pair per high-risk distinction. Target shape: roughly current length plus a short validation section, not a doubled document.

## [4][SECTION_PLAN]

`Lead`: change. Keep the current definition of `AGENTS.md` as a scoped behavioral overlay, but add the governing distinction: it states local behavior deltas by layer and separates target rules from current proof claims.

`## [1][USE_WHEN]`: keep and sharpen. Add "reviewing whether an overlay should exist" and "deciding whether a rule belongs in root, parent, leaf, or a route-away document." Keep the deletion test: if deleting the file removes no non-derivable local behavior delta, the overlay should not exist.

`## [2][AGENT_USE]`: replace with `## [2][CONTENT_MODEL]`. This section should carry root/parent/leaf relation, overlay existence test, target/current split, and the lead ordering rule. Move loose authoring questions into this model or delete them if they duplicate later slots.

Recommended wording:

```markdown conceptual
Root routes, parent deduplicates, and leaf specializes. Write only the action-changing delta from parent guidance. State target standards as normative rules; state present behavior only with proof or an explicit proof gap.
```

`## [3][OFFICIAL_LOAD_SEMANTICS]`: keep but trim. State only Codex facts that change authoring behavior and add a maintenance/proof route sentence. Do not expand into Claude/Gemini manuals; owns provider prompt-shape comparisons.

Recommended addition:

```markdown conceptual
Provider-loading facts are current-behavior claims. Keep them only when they change author action, and refresh them through `proof.md` when provider documentation or local tool output changes.
```

`## [4][PRODUCED_FUNCTIONS]`: change to slot grammar. Keep scope, read behavior, future-standard posture, owner contract, route-away, rejections, and close/stop behavior, but add `Target/current split` and `Internalization before exposure`. Make `Test contract`, `Tool authorization`, `Trust boundaries`, and `Delegation` conditional slots, not generic required material.

Recommended compact slot run:
- Lead.
- Scope.
- Read behavior.
- Target/current split.
- Owner contract.
- Internalization before exposure.
- Route-away.
- Rejections.
- Conditional contracts.
- Close or stop behavior.

`## [5][SECTION_CARDINALITY]`: change. Split into `[REQUIRED_BASE]`, `[CONDITIONAL_SLOTS]`, and `[REJECTED_SLOTS]`. Add `TEST_CONTRACT` to conditional slots. Keep "no generic validation ladder" for produced overlays, but do not prevent this standard itself from having a validation checklist.

`## [6][ROOT_PROFILE]`: replace with `## [6][LAYER_PROFILES]`. Keep root profile content but fold in parent and leaf profiles. Root should preserve `CLAUDE.md` precedence, load semantics, nested overlay discovery, conflict/trust rule, owner routes, and preservation audit. Parent should carry shared sibling deltas and deduplicated rules. Leaf should carry exact local owner/action/stop rules.

Remove from this section:
- any repeated future-forward paragraph already handled by target/current split;
- any subtree implementation example that belongs in local overlays;
- any command syntax, package truth, provider manual, or generated-contract body.

`## [7][LOCAL_EXTENSION_GRAMMAR]`: expand narrowly. Keep the current accepted/rejected shape, then add internalization-before-exposure and a no-baseline-caveat contrast.

Recommended wording:

```markdown conceptual
When current implementation lags the target, do not encode lag as a standard. State the replacement target, route present-tense facts to proof, and mark missing proof as a gap.

Accepted: When adding artifact storage backend behavior, extend the store/settings owner and preserve the existing artifact and envelope folds before documenting operator workflow.
Rejected: Add a storage helper, cloud-mode flag, and backend-specific report.
Reason: the accepted form internalizes capability into the existing owner rail; the rejected form exposes implementation as surface area.
```

`## [8][ROUTE_AWAY]`: keep but compress. Replace long document-type explanation with route categories:
- documents own orientation, architecture, sequence, procedures, and lookup facts;
- proof/tool docs own command syntax, gates, freshness, generated output, and runtime evidence;
- source/manifests/generated contracts own current truth;
- README owns public workflow when reader action changes.

Keep the rule that stale compatibility prose is deleted, not routed, unless a current owner and proof route exist.

`## [9][ANTI_FRAGILITY]`: keep and sharpen around exact-fact tests. Add run-local artifacts, fixed sub-agent counts, provider claims, and memory-derived policy to rejected fragile facts. Preserve allowable exact facts when they are local owners, forbidden tokens, route targets, or proof selectors.

`## [10][TRUST_BOUNDARIES]`: keep and add the external/provider research boundary. State that Wave reports, task prompts, generated mirrors, memory notes, retrieved chunks, logs, and external examples are evidence until an explicit trusted route promotes a durable rule.

`## [11][CORPUS_REBUILD_RULES]`: change. Keep inventory, sibling extraction, leaf-local rule, trigger-driven reads, local extension grammar, validation removal, and route verification. Add:
- parent overlays may absorb a rule only when 3 or more siblings share it or one shared runtime/trust/generated boundary requires it;
- every leaf rule must name a local owner, action, route, proof gap, or stop condition;
- test-owning overlays get the conditional test contract;
- root edits require a preservation audit for removed paths, commands, routes, qualifiers, triggers, and provider claims.

`## [12][FORMAT_TONE]`: keep. Add "no wide tables" and "examples only beside high-risk distinctions." Keep bracketed headings and invocation-marker rules.

`## [13][MAINTENANCE]`: keep but shorten. Update triggers should include provider load semantics changes, new overlay layer profiles, repeated failure patterns, test-contract failures, and internalization/exposure mistakes.

Add `## [14][VALIDATION]`: new compact checklist. This file is a standard and should close with validation, even though produced `AGENTS.md` overlays should not add generic validation ladders.

Recommended checklist groups:
- `[LAYERING]`: root routes, parent deduplicates, leaf specializes; no copied owner bodies.
- `[PROOF_SPLIT]`: target rules are not weakened by current drift; current-state claims have proof or gaps.
- `[LOCAL_GRAMMAR]`: local rules name trigger, owner, extension action, and rejected substitute.
- `[CONDITIONAL_SLOTS]`: test, tool, trust, runtime, and delegation slots appear only when triggered.
- `[COMPACTNESS]`: no wide tables, command catalogs, provider manuals, session notes, baseline caveats, placeholder fields, or duplicate route bodies.

## [5][REMOVE_OR_AVOID]

Remove repeated future-forward wording that appears in multiple sections. Keep one target/current split and reference it from layer profiles and local extension grammar.

Remove generic "validation section" ambiguity. Produced overlays should avoid generic validation ladders, but `agents-md.md` itself should have a validation checklist because it is a standards file.

Remove any impulse to add a language-by-language doctrine section. C#, TypeScript, Python, Bash, SQL, tests, bridge, and Assay context reports supply examples of the same content model; the active standard should encode the model, not the language tutorials.

Avoid a root/parent/leaf table. A short grouped list is enough and avoids the wide-table failure mode.

Avoid provider comparison tables. Provider behavior belongs in unless `AGENTS.md` load semantics directly affect authoring behavior.

Avoid fixed sub-agent counts, fixed report shapes, `_reports/` source names, run-local artifact paths, or memory-derived policy in the active standard.

## [6][CONCRETE_OUTLINE]

Recommended rebuilt outline:

```markdown conceptual
# [AGENTS_MD_STANDARDS]

## [1][USE_WHEN]
## [2][CONTENT_MODEL]
## [3][LOAD_SEMANTICS]
## [4][PRODUCED_SLOTS]
## [5][SECTION_CARDINALITY]
## [6][LAYER_PROFILES]
## [7][LOCAL_EXTENSION]
## [8][ROUTE_AWAY]
## [9][ANTI_FRAGILITY]
## [10][TRUST_BOUNDARIES]
## [11][CORPUS_REBUILD]
## [12][FORMAT_TONE]
## [13][MAINTENANCE]
## [14][VALIDATION]
```

This outline keeps every current owner concept, adds the missing proof split and layer profiles, and makes conditional slots explicit without expanding the document into a research digest.

## [7][CONFIDENCE]

Confidence: high.

Basis:
- All requested current standards and all 12 context reports were read.
- The recommendations align with current `agents-md.md` rather than replacing it with a new doctrine.
- The same missing model appears independently across context reports: root/parent/leaf placement, target/current proof split, internalization before exposure, exact local owner grammar, conditional test contract, provider proof boundaries, and compactness.

Limits:
- This pass did not edit active standards and did not run docs validation gates.
- External provider facts were taken from context reports only as pattern inputs; no new provider behavior is asserted here.
