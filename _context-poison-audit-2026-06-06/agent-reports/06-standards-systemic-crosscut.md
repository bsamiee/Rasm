# Standards Systemic Cross-Cut Audit

Auditor: F
Scope: active `docs/standards` Markdown files, excluding `_reports/**` content, with `docs/standards/_reports/AGENTS.md` included because the task named it.
Mode: read-only audit except this report.

## Executive Finding

The standards corpus is directionally strong: it repeatedly rejects report leakage, owner-body copying, transient prompt/process language, decorative structure, false proof, generated-catalog drift, and stale compatibility prose. The main systemic risk is not missing policy; it is over-consolidated policy being copied into every type page. The repeated `AUTHORING_CONTRACT` blocks, adjacent-check lists, exact produced skeletons, proof-field packets, and local status vocabularies can train future generated docs and prompts to reproduce standards machinery instead of writing the smallest document that changes reader action.

The correction should not add another meta-standard. Collapse duplicated opening-contract, root-audit, proof-field, and report-session mechanics back to their owners; keep type standards as local deltas over the shared standards.

## Top Systemic Findings

### [F1] Type standards repeat the same authoring-contract body instead of referencing one owner

Every type standard follows the same opening grammar: `Agent use`, `Required produced structure`, `Section cardinality`, `Adjacent checks`, and `Maintenance triggers`. The local overlay actively requires that every type standard state eight opening-contract items before examples or background.

Supporting locations:
- `docs/standards/AGENTS.md:65-73` requires every type standard to state purpose, use, route-away, agent use, produced structure, cardinality, adjacent checks, and maintenance triggers.
- `docs/standards/task/how-to.md:15-20` repeats the full contract pattern for how-to guides.
- `docs/standards/task/runbook.md:17-22` repeats the same pattern for runbooks.
- `docs/standards/reference/api.md:20-26` repeats the same pattern for API documentation and adds stale-prevention.
- `docs/standards/learning/tutorial.md:19-25` repeats the same pattern for tutorials and learning paths.

Why this poisons context:
- Future generated documents can copy the authoring metadata itself, especially `Adjacent checks` and `Maintenance triggers`, into produced docs.
- The repeated neighbor-route lists become soft owner bodies; they are long enough to drift and broad enough to invite "check every adjacent doc" behavior.
- The standards already have owners for these mechanics: `README.md` owns type routing, `information-structure.md` owns cardinality/records, and `proof.md` owns evidence fields.

Correction:
- Keep one shared type-standard opening-contract rule in `docs/standards/AGENTS.md` or `information-structure.md`.
- In each type standard, keep only the local discriminants: document purpose, route-away boundary, minimal produced contract, type-local statuses, and local proof slots.
- Replace broad adjacent-check lists with local relation triggers: "add a relation record only when this changed fact changes reader action, proof, status interpretation, validation, or maintenance."

### [F2] Root-audit mechanics leak across owners and risk becoming a generic document-review template

The five-axis audit rule is valid for standards-root reviews, but it appears in multiple active owners and validation sections. That makes a root-audit process look like a general produced-document requirement.

Supporting locations:
- `docs/standards/AGENTS.md:43-55` defines root-file audits, required finding fields, and read-only/edit audit proof language.
- `docs/standards/README.md:91-99` defines the 5 shared axes and tells root audits to use only those axes.
- `docs/standards/README.md:210-214` repeats root-file audit validation in the router's closing checklist.
- `docs/standards/information-structure.md:475-479` validates that root-file audits can be recorded against the 5 axes.
- `docs/standards/style-guide.md:201-202` validates root-file audit prose and local grading vocabulary.

Why this poisons context:
- A future agent can import "root-file audit" fields into normal docs, prompts, or reports.
- The rule crosses owner boundaries: `AGENTS.md` should own local audit behavior; `README.md` should route; `style-guide.md` should only carry prose quality; `information-structure.md` should only carry container fit.

Correction:
- Keep exact audit mechanics in `docs/standards/AGENTS.md`.
- Let `README.md` only name the five axes as owner routes.
- Remove root-audit validation bullets from shared standards unless they state a genuinely local prose/container rule without reproducing audit process.

### [F3] Exact produced skeletons are too sticky and can generate over-structured docs

The corpus says to omit empty conditional headings, but many type standards still publish exact required skeletons with many mandatory sections, renumbering rules, and field packets. The more exact the skeleton, the more likely future generation produces boilerplate headings and relation records before the document earns them.

Supporting locations:
- `docs/standards/task/contributing.md:53-69` requires a 9-section contributing skeleton; `docs/standards/task/contributing.md:86-90` restates required and conditional section cardinality.
- `docs/standards/task/runbook.md:131-147` defines conditional sections, required response-critical facts, stable trigger/profile fields, and exact label prohibitions.
- `docs/standards/learning/tutorial.md:31-54` requires a 10-section tutorial spine; `docs/standards/learning/tutorial.md:59-71` requires preview, learning outcome, prerequisites, start state, 3 to 12 steps, result, notice, next steps, boundaries, and validation.
- `docs/standards/task/how-to.md:42-74` is the healthier pattern: a minimal core plus conditional sections and explicit rejection of empty placeholders, but even it carries fixed opening-order language at `docs/standards/task/how-to.md:22`.

Why this poisons context:
- Produced docs can become standards-shaped rather than reader-shaped.
- The exact spines encourage "fill every field" behavior despite explicit omit rules.
- The high cardinality is especially risky for lightweight docs, where the useful output may be one goal, one path, and one proof statement.

Correction:
- Rephrase skeletons as "minimum valid contract" plus "allowed local sections", not mandatory document architecture.
- Keep exact section order only for documents where order is safety-critical, such as runbooks.
- Move "renumber headings in document order" and repeated conditional-heading mechanics to formatting/information structure, then cite it rather than restating it per type.

### [F4] Proof-field packets are copied into type-local templates even though proof has one owner

`proof.md` correctly states that it is the single route for proof labels and that type standards should not restate generic proof. Several type standards still include full `Evidence`, `Generated from`, `Controlling source`, `Proof gap`, `Last verified`, and `Review trigger` packets in local templates.

Supporting locations:
- `docs/standards/proof.md:39-54` defines proof labels, exact order, and field meanings as the single route.
- `docs/standards/proof.md:67` says to carry only the fields the claim needs.
- `docs/standards/proof.md:282-284` says type standards may name artifact-specific proof slots but must not restate generic proof per type.
- `docs/standards/reference/api.md:91-108` includes a full contract-record template with generic proof fields.
- `docs/standards/learning/tutorial.md:157-170` includes a path-entry template with evidence and update/close/route fields.
- `docs/standards/information-structure.md:157-162` defines default record and proof sub-block ordering, which type standards then echo.

Why this poisons context:
- Future documents can acquire proof fields as decoration.
- `Last verified` and `Review trigger` can spread to stable claims where event-based freshness or no proof field is sufficient.
- The duplicated proof packets invite inconsistent field subsets and order.

Correction:
- In type standards, name only artifact-specific proof slots, such as "status evidence", "contract source", or "front-to-back run".
- Replace full proof packets with "use the proof-field run from `proof.md` only when the claim can drift or the local proof slot requires it."
- Keep complete field templates in `proof.md` and `information-structure.md`, not in every type page.

### [F5] The `_reports` overlay is correctly quarantined but highly poisonous if promoted

`docs/standards/_reports/AGENTS.md` is intentionally a leaf overlay for source-material mechanics, not an active standard. It is detailed enough to be useful for report hygiene, but those mechanics must not leak into active standards, prompts, or generated docs.

Supporting locations:
- `docs/standards/_reports/AGENTS.md:3-9` states `_reports` is source material only and owns no active standards, commands, proof vocabulary, or validation gates.
- `docs/standards/_reports/AGENTS.md:17` excludes `_reports` from active-corpus counts, audits, link sweeps, generated-contract reads, and validation claims unless named.
- `docs/standards/_reports/AGENTS.md:21-31` defines session folder, track, and report numbering mechanics.
- `docs/standards/_reports/AGENTS.md:35-49` defines required session manifest sections and table constraints.
- `docs/standards/_reports/AGENTS.md:84-105` defines report header fields, allowed sections, and forbidden transcript/confidence/validation frames.
- `docs/standards/_reports/AGENTS.md:157-160` rejects turning `_reports` into a second corpus and rejects copying report frames into active owners.

Why this poisons context:
- The report shape is necessarily process-heavy: lanes, merge keys, outcomes, tracks, manifests, corrections, promotion logs, pruning logs.
- If imported into active standards, it would create the exact poison the corpus tries to reject: worker-role, wave, prompt, confidence, and validation-ledger artifacts.

Correction:
- Keep `_reports/AGENTS.md` as a quarantined leaf overlay.
- Active standards should mention `_reports` only as excluded source material and promotion boundary.
- Do not use report headings, report validation frames, session manifests, merge keys, or track mechanics as examples in ordinary standards.

### [F6] Status vocabularies and progress notation are at risk of becoming default decoration

The status and marker system is internally coherent, but it is broad and appears in many type standards. The corpus already says markers must encode state and not decorate. The risk is future generated docs adding lifecycle fields, availability terms, progress bars, or bracketed tokens because the standards expose them as attractive machinery.

Supporting locations:
- `docs/standards/information-structure.md:120-134` defines a default closed lifecycle vocabulary and requires type standards to declare narrowed subsets before examples.
- `docs/standards/information-structure.md:157-162` makes lifecycle and relation record field order authoritative.
- `docs/standards/formatting.md:77-88` defines distinct marker families and exactly 20-cell progress bar mechanics.
- `docs/standards/formatting.md:128-132` limits invocation markers to instruction files and rejects notation spam.
- `docs/standards/learning/tutorial.md:146-153` defines a type-local learning availability vocabulary and explicitly forbids tutorial progress bars.

Why this poisons context:
- Status fields can be added to records that are not actually living state.
- Progress bars can tempt dashboards, reports, and roadmaps into false precision.
- Bracketed tokens can spread from standards notation into ordinary prose.

Correction:
- Add a corpus-level "state earns status" rule near the record chooser: use status only when a maintained actor filters, updates, or removes the item by state.
- Keep progress bars only where the numerator, denominator, closure rule, and proof surface are immediately visible.
- Prefer deletion of unnecessary state fields over declaring local vocabularies.

## What Is Working

- The source-material boundary is explicit and repeated in the right places: `docs/standards/AGENTS.md:9-11`, `docs/standards/agentic-documentation.md:89-102`, `docs/standards/agents-md.md:197-203`, and `docs/standards/_reports/AGENTS.md:3-17`.
- Owner routing is clear at the top level: `docs/standards/README.md:91-99` maps the five shared axes to one owner each, and `docs/standards/AGENTS.md:28-41` maps standards-folder rule owners.
- Several files contain the antidote to their own risk: `docs/standards/README.md:194-199` prefers route links and deletion over duplicated guidance; `docs/standards/proof.md:67` says to carry only needed fields; `docs/standards/information-structure.md:221-225` rejects contrast records for every rule; `docs/standards/formatting.md:128-132` rejects notation spam.

## Recommended Cleanup Order

1. Collapse the type-standard `AUTHORING_CONTRACT` pattern into one shared owner, then reduce each type page to local deltas.
2. Move root-audit mechanics entirely into `docs/standards/AGENTS.md`; leave other shared standards with only their local prose/container/proof/notation rule.
3. Replace exact produced skeletons with minimum valid contracts and conditional local sections, keeping exact order only where safety-critical.
4. Remove generic proof-field packets from type templates; cite `proof.md` and keep only type-local proof slot names.
5. Keep `_reports/AGENTS.md` quarantined and avoid active-standard examples that use report sessions, lanes, merge keys, or report close frames.

## Validation

No active `docs/standards` source files were edited by this audit. `git diff --check -- _context-poison-audit-2026-06-06/agent-reports/06-standards-systemic-crosscut.md` passed after writing this report. No link, anchor, renderer, docs build, or standards validation gate was run.
