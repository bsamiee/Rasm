# [AGENTS_STANDARD_FINALIZER]

Files read: `CLAUDE.md`, `docs/standards/README.md`, `docs/standards/agents-md.md`, `docs/standards/information-structure.md`, `docs/standards/agentic-documentation.md`, `docs/standards/proof.md`, all `track-owner-routing/*.md` reports present in `docs/standards/_reports/agents-md-050626/`, and the requested context reports `track-source-scans/04-docs-standards-context.md`, `track-source-scans/05-frontier-instruction-practice.md`, `track-source-scans/12-tests-bridge-context.md`, `track-source-scans/01-assay-context.md`, and `poly-*.md`.

## [1][FINAL_DIRECTION]

Rebuild `agents-md.md` around one governing model: an `AGENTS.md` file is a scoped behavioral delta whose rule must name layer, owner, action, proof mode, and route-away. The current file is close, but it still reads like several adjacent doctrines. The final edit should collapse it into an authoring contract that answers five questions in order:

- Which layer owns this rule: root, parent, or leaf?
- Is the sentence a target standard, a current-state claim, or a proof gap?
- Which local owner rail grows before anything is exposed?
- Which conditional contract applies: tests, tools, runtime, trust, delegation, provider/load, generated surface?
- What content routes away because it is not local agent behavior?

The standard should not get longer by adding examples. It should get sharper by converting current broad sections into slot grammar, layer profiles, and rejection-ready wording.

## [2][FINAL_SECTION_ORDER]

Use this order:

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

Keep the H1 and bracketed heading idiom. Use grouped lists and compact contrast records, not wide tables. The file is a root standard, so it needs `BOUNDARIES` and `VALIDATION` style closure through sections `[13]` and `[14]`; this does not license produced `AGENTS.md` overlays to add generic validation ladders.

## [3][SECTION_SPEC]

`[1][USE_WHEN]`: Keep the deletion test and add two triggers: reviewing whether an overlay should exist, and deciding root/parent/leaf placement. Keep the authoring questions only if they feed later slots; remove questions that restate the same owner/proof/route decision.

`[2][CONTENT_MODEL]`: Replace `AGENT_USE`. This section must define root/parent/leaf, target/current/gap, and lead ordering. It is the new spine.

Exact wording block:
```markdown conceptual
Root routes, parent deduplicates, and leaf specializes. Write only the action-changing delta from parent guidance. State target standards as normative rules; state present behavior only with proof or an explicit proof gap.
```

`[3][LOAD_SEMANTICS]`: Keep only provider-loading facts that change author action. Codex load behavior belongs here because it affects root-to-leaf authoring. Provider prompt-shape comparisons stay in `agentic-documentation.md`.

Exact wording block:
```markdown conceptual
Provider-loading facts are current-behavior claims. Keep them only when they change author action, and refresh them through `proof.md` when provider documentation or local tool output changes.
```

`[4][PRODUCED_SLOTS]`: Replace broad produced-functions prose with slot grammar. Required base slots: `Lead`, `Scope`, `Read behavior`, `Target/current split`, `Owner contract`, `Internalization before exposure`, `Route-away`, `Rejections`, `Close or stop behavior`. Conditional slots: `TEST_CONTRACT`, `TOOL_AUTHORIZATION`, `TRUST_BOUNDARY`, `RUNTIME_BOUNDARY`, `GENERATED_SURFACE`, `DELEGATION`, `LOCAL_VOCABULARY`.

`[5][SECTION_CARDINALITY]`: Split into `[REQUIRED_BASE]`, `[CONDITIONAL_SLOTS]`, and `[REJECTED_SLOTS]`. Add `TEST_CONTRACT` explicitly. Preserve the rejection of generic validation ladders in produced overlays.

`[6][LAYER_PROFILES]`: Replace root-only profile with root, parent, and leaf profiles:
- Root overlay: instruction router, load semantics, nested overlay discovery, trusted-source conflict rule, cross-stack owner routes, root-change preservation audit, root-only rejections.
- Parent overlay: shared sibling delta, 3-or-more sibling extraction, family-wide read triggers, shared runtime/generated/trust boundary, repeated stop condition.
- Leaf overlay: exact local owner identifiers, local extension action, local route-away record, proof gap, stop condition.

`[7][LOCAL_EXTENSION]`: Keep the current accepted/rejected grammar but add the two missing patterns: no baseline caveat, and internalization before exposure. This is the most important edit.

Exact wording block:
```markdown conceptual
When current implementation lags the target, do not encode lag as a standard. State the replacement target, route present-tense facts to proof, and mark missing proof as a gap.
```

`[8][ROUTE_AWAY]`: Compress into route categories. Documents own orientation, architecture, sequence, procedures, and lookup facts. Tool READMEs own command syntax and public wire behavior. Source, manifests, generated contracts, and local tool output own current truth. `proof.md` owns evidence labels, freshness, proof gaps, and preservation. Delete stale compatibility prose unless a current owner and proof route exist.

`[9][ANTI_FRAGILITY]`: Keep exact-fact rules but sharpen the test: exact facts are allowed only as route targets, local owner identifiers, forbidden tokens, proof selectors, trust-boundary names, or invariants with refresh triggers. Explicitly reject fixed sub-agent counts, memory-derived policy, run-local artifact paths, provider claims without proof route, package/version prose, and generated path catalogs.

`[10][TRUST_BOUNDARIES]`: Keep. Add that Wave reports, task prompts, generated mirrors, memory notes, retrieved chunks, logs, transcripts, tool output, community examples, and external research are evidence until a trusted repo route promotes the durable rule.

`[11][CORPUS_REBUILD]`: Keep inventory and sibling extraction, but make it executable:
1. Classify every overlay as root, parent, leaf, tool/operator, runtime, standards/docs, test-owning, generated/catalog-adjacent, or package/host boundary.
2. Move a rule to parent only when 3 or more siblings share it or one shared boundary requires it.
3. Keep a leaf rule only when it names local owner, action, route, proof gap, or stop condition.
4. Convert broad read inventories into trigger-driven reads.
5. Add `TEST_CONTRACT` only to test-owning overlays.
6. Remove ordinary validation sections and command catalogs.
7. Verify every route target exists, is conditionalized with `where present`, or is marked as a proof gap.
8. For root edits, preserve or route every removed command, path, version, flag, qualifier, trigger, provider-loading claim, proof selector, and owner pointer.

`[12][FORMAT_TONE]`: Keep bracketed headings and invocation-marker rules. Add "no wide tables" and "examples only beside high-risk distinctions." Remove any phrasing that names research waves, task history, or source inventories as durable structure.

`[13][MAINTENANCE]`: Shorten. Update this standard when provider load semantics change, a new recurring overlay profile appears, repeated failure patterns prove a durable rule, `TEST_CONTRACT` hazards repeat, or internalization/exposure mistakes recur.

`[14][VALIDATION]`: Add a compact checklist grouped by `[LAYERING]`, `[PROOF_SPLIT]`, `[LOCAL_GRAMMAR]`, `[CONDITIONAL_SLOTS]`, and `[COMPACTNESS]`.

## [4][CRITICAL_ADDITIONS]

### [4.1][INTERNALIZATION_BEFORE_EXPOSURE]

Add this as a required produced slot and reinforce it in local extension grammar.

Exact wording block:
```markdown conceptual
Internalization before exposure: when a dependency, backend, runtime, host API, storage layer, provider, or tool integration adds capability, first bind it into the existing local owner: operation algebra, typed case, row table, registry bind, tagged union case, receipt fold, aspect slot, boundary adapter, artifact/store shape, runtime record, query algebra, source-owned scenario, or envelope field. Expose a new command, flag, setting, public API, wrapper, package facade, compatibility alias, or provider-branded surface only when the owner route proves reader action changes.
```

Accepted/rejected contrast:
```markdown conceptual
Accepted: When adding artifact storage backend behavior, extend the store/settings owner and preserve the existing artifact and envelope folds before documenting operator workflow.
Rejected: Add a storage helper, cloud-mode flag, and backend-specific report.
Reason: the accepted form internalizes capability into the existing owner rail; the rejected form exposes implementation as surface area.
```

### [4.2][TEST_CONTRACT]

Add this as a conditional produced slot, not as a test tutorial.

Exact wording block:
```markdown conceptual
TEST_CONTRACT: required when the folder owns specs, scenarios, testkit, fuzz, benchmark, architecture checks, mutation, snapshot or visual checks, or host/runtime proof. It names the local proof rail, oracle source, runtime boundary, helper-promotion route, raw rail-state rule, mutation-survivor triage rule where relevant, rejected fake-proof shapes, and stop behavior without listing runner commands.
```

Accepted/rejected contrast:
```markdown conceptual
Accepted: Before adding a Rhino-facing assertion, classify the behavior as static-managed law or host-runtime scenario proof; put native success in a source-owned scenario and keep xUnit on guards, categories, receipts, and pure managed contracts.
Rejected: Add a skipped xUnit test for Rhino behavior.
Reason: the accepted form chooses the proof rail before the assertion; the rejected form turns a runtime proof gap into fake static proof.
```

### [4.3][TARGET_CURRENT_SPLIT]

Exact wording block:
```markdown conceptual
Target standard: a normative rule about the newest viable language, feature, tool, architecture, dependency, or methodology target. State it plainly and do not weaken it because current code lags.
Current-state claim: a present-tense fact about existing commands, provider behavior, package state, project membership, generated output, support status, runtime behavior, or validation. Attach current proof or route it to the evidence owner.
Proof gap: a missing source, command output, host state, provider proof, manifest, generated contract, or owner route. Mark the gap instead of converting current drift into a baseline caveat.
```

## [5][CUT_LIST]

Cut or consolidate:
- Repeated future-forward paragraphs across produced functions, root profile, local extension grammar, and maintenance. Keep one target/current split.
- Provider tutorial detail beyond load semantics that change authoring behavior.
- Any language-by-language capability doctrine. C#, TypeScript, Python, Bash, SQL, Assay, and bridge reports are evidence for the model, not sections in the active standard.
- Wide tables for layer profiles or anti-pattern catalogs. Use grouped lists and compact contrast records.
- Generic "validation section" ambiguity. Produced overlays reject generic validation ladders; this standard still needs a validation checklist.
- `_reports/`, wave, transcript, current task, or fixed sub-agent-count language.
- Exact package/library names in universal wording unless they are examples clearly routed to local overlays.
- Compatibility prose that preserves old names, shims, aliases, deprecation windows, partial adoption, or current baseline caveats.

## [6][WEAK_FORMULATIONS_TO_REJECT]

Reject these forms in the final edit or produced overlays:
- "Avoid helpers."
- "Use advanced polymorphism."
- "Keep it clean."
- "Run the relevant tests."
- "Follow best practices."
- "This folder currently still uses X, so preserve both."
- "Use the new backend flag when needed."
- "Add a wrapper around the package API."
- "List all commands here for convenience."
- "This `AGENTS.md` prevents unsafe commands."
- "Provider behavior works this way" without current proof route.
- "Use bridge for extra confidence" without host/runtime classification.
- "Skip the unit test until bridge proof exists."
- "Store artifact path as proof."
- "Move this into a helper to reduce file length."
- "Create a new schema/model/DTO for this variant."
- "Document both old and new paths until migration."

Replace them with trigger-owner-action wording:
```markdown conceptual
When <change class>, read <owner source> and extend <owner rail> by <case, row, fold, projection, receipt, boundary adapter, scenario, or envelope field>. Do not add <specific rejected substitute>; route <non-owned fact> to <README, architecture, roadmap, source, manifest, generated contract, tool README, or proof owner>.
```

## [7][VALIDATION_BLOCK_TO_ADD]

Use this compact checklist:

```markdown conceptual
## [14][VALIDATION]

[LAYERING]:
- [ ] Root routes, parent deduplicates, and leaf specializes.
- [ ] No overlay copies owner bodies from README, architecture, roadmap, API, tool docs, source, or generated contracts.
- [ ] Root edits account for every removed path, command, route, qualifier, trigger, provider claim, and proof selector.

[PROOF_SPLIT]:
- [ ] Target standards are not weakened by current drift.
- [ ] Current-state claims have current proof, a maintained route, or an explicit proof gap.
- [ ] Provider-loading claims are kept only when they change author action.

[LOCAL_GRAMMAR]:
- [ ] Local rules name trigger, owner, extension action, and rejected substitute.
- [ ] Dependency, backend, runtime, host, storage, provider, and tool capability is internalized before exposure.
- [ ] Prohibitions are paired with replacement owner routes.

[CONDITIONAL_SLOTS]:
- [ ] `TEST_CONTRACT` appears only when the folder owns test or proof rails, and it rejects fake proof without listing runner commands.
- [ ] Tool, runtime, generated-surface, trust, delegation, and vocabulary slots appear only when triggered.
- [ ] Stop rules name the missing proof, wrong owner, unavailable runtime, unsafe state, or current source required before continuing.

[COMPACTNESS]:
- [ ] No command catalogs, provider manuals, session notes, fixed sub-agent counts, package/version prose, run-local artifact paths, or baseline caveats remain.
- [ ] Examples sit beside high-risk distinctions and do not become tutorials.
- [ ] Exact facts are route targets, local owner identifiers, forbidden tokens, proof selectors, trust-boundary names, or invariants with refresh triggers.
```

## [8][FINAL_STANDARD_SHAPE]

The rebuilt standard should read like an overlay compiler:

- Input: a directory and the parent instruction chain.
- Decision: root, parent, or leaf.
- Rule kind: target, current claim, or proof gap.
- Owner: local rail that grows.
- Exposure check: internalize before public surface.
- Conditional contract: tests, tool/runtime, trust, generated, delegation, vocabulary.
- Output: compact local instructions with route-away and stop behavior.
- Rejection: no slogans, no command catalogs, no provider manuals, no compatibility caveats, no fake proof.

That is the level higher than refinement: the final file should not merely describe good `AGENTS.md` content. It should make invalid overlay content mechanically recognizable.
