Question: Which task and learning type standards define local formatting, information-structure, proof, status, checklist, and route-away systems that should stay type-local or ripple into shared owners?
Type: repo-scan
Lane: track-type-corpus
Merge key: type-corpus :: task-learning local systems :: owner-routing catalogue
Target owner: docs/standards/task/*.md and docs/standards/learning/*.md
Source basis: current repo proof from CLAUDE.md, AGENTS.md, docs/standards/README.md, docs/standards/AGENTS.md, .reports/AGENTS.md, target type standards, and shared owners information-structure.md, formatting.md, proof.md, style-guide.md
Promotion target: active owners named per finding
Outcome: PROMOTE

## [FINDINGS]

### [F1][CONTRIBUTING_GUIDE_SYSTEMS]

Path: `docs/standards/task/contributing.md`
Evidence: lines 23-30 define the authoring contract, required produced structure, section cardinality, adjacent checks, and maintenance triggers; lines 44-93 define base and conditional sections; lines 101-137 define accepted-path selector tables, one-path definition blocks, and compact contrast labels; lines 166-199 define quality-gate tables, one-gate definition blocks, result reporting, and `Gap`; lines 205-220 define the shared adjacent-document relation record; lines 222-244 define pull-request body packets and a `Required self-check` checklist; lines 246-263 define review-profile tables; lines 292-316 define grouped validation checklists.
Catalogue:
- Lifecycle/status vocabulary: none local beyond contributor-facing gate/result reporting. Gate result examples use bracketed result markers such as `[PASS]` and `[SKIP]`, which route to `formatting.md`.
- Task records: accepted contribution paths use either a comparison table or a definition block with `Intent`, `Entry artifact`, `Prerequisite`, and `Scope bound`.
- Proof fields: gate records use `Report` and `Gap`; PR packets use `Results`, `Gap`, `Scope`, and optional `Evidence`.
- Checklists: PR self-check and validation sections use checkbox checklists.
- Route-away records: documentation-change triggers use the shared relation record with `Changed fact`, `Consumed by`, `Use in this document`, `Update when`, `Close when`, and `Route-away`.
- Examples: compact contrast labels are `Accepted`, `Rejected`, `Near miss`, and `Reason`; tables are used only when two or more paths, gates, or profiles are comparable.
Active owner: `contributing.md` for contribution workflow, path selector shape, PR packet shape, review profile shape, and security-report routing; `information-structure.md` for relation-record field order and table/definition-block choice; `proof.md` for evidence and proof-gap semantics; `formatting.md` for result markers and checkbox notation.
Ripple: Keep `Gap` as a contributor-report field, but ensure shared proof wording remains the route for proof semantics when a gate was unrun or unsupported. The type file should not promote a generic `Gap:` proof field outside contributor/PR/gate packets.
Decision: KEEP type-local packet fields. PROMOTE only the boundary rule that local packet `Gap` fields must map to `Proof gap:` semantics when the field asserts missing proof rather than contributor scope.
Proof gap: No host settings, branch protection, PR template, CODEOWNERS, or CI configuration was checked because this report audits the standard shape, not a produced contributing guide.

### [F2][HOW_TO_GUIDE_SYSTEMS]

Path: `docs/standards/task/how-to.md`
Evidence: lines 15-22 define the authoring contract; lines 30-74 define required and conditional sections; lines 76-107 define a minimal pattern with procedure step records, verification checklists, and claim-level evidence; lines 109-130 define task-specific adjacent-route handoff records; lines 131-146 define prerequisite definition blocks; lines 148-188 define ordered procedure records, `Optional:` and `Irreversible:` step markers, compact accepted/rejected command records, and a decision table; lines 189-228 define verification, rollback records, `Evidence`, and `Last verified`; lines 229-247 define troubleshooting H3 records; lines 248-253 route format choices; lines 274-295 define grouped validation checklists.
Catalogue:
- Lifecycle/status vocabulary: none local. The guide uses outcome and verification states, not living lifecycle records.
- Task records: command steps carry `Operation`, `Expected result`, and optional `If wrong`; prerequisites use `Access`, `Target context`, `Tools`, and `Prepared artifact`; rollback records use `Reverse action`, `Expected result`, `Verification`, or `Recovery route`.
- Proof fields: `Evidence` and `Last verified` appear beside drift-prone examples and route to `proof.md`.
- Checklists: verification uses checklist form when several independent outcome conditions must hold; single-condition verification may be a proof statement.
- Route-away records: task-specific handoff records use the shared relation fields at point of consumption.
- Examples: minimal how-to skeleton, command step record, accepted/rejected command contrast, decision table, rollback records, proof record, and troubleshooting record.
Active owner: `how-to.md` for competent-reader procedure shape, step markers, rollback/troubleshooting boundaries, and outcome-first verification; `information-structure.md` for container order and relation-record shape; `style-guide.md` for imperative, condition-first procedure wording; `proof.md` for evidence fields.
Ripple: The local `Optional:` and `Irreversible:` markers are currently declared as how-to-owned at lines 267-271. `formatting.md` should not generalize them into ordinary marker families unless another type adopts the same leading-step semantics.
Decision: KEEP type-local. The how-to file already states the container order clearly: numbered branch, decision table, then Mermaid only when branch sequence and rejoin need a rendered flow.
Proof gap: No rendered Mermaid, link, or anchor validation was run. No current-source research was required because no external tooling or provider behavior claim was evaluated.

### [F3][RUNBOOK_SYSTEMS]

Path: `docs/standards/task/runbook.md`
Evidence: lines 17-24 define the authoring contract; lines 26-31 define local incident-process source precedence; lines 32-91 define response-profile fields, allowed `none` use, profile blockers, and an optional Mermaid response path with text equivalent; lines 102-148 define required and conditional sections plus stable trigger/profile fields; lines 149-170 define entry, response, closure, and metric-threshold requirements; lines 171-199 define step records and `Safe mutation: none`; lines 200-220 define operational handoff records; lines 222-243 define triage ordering and decision-table use; lines 244-267 define mitigation, rollback, abort, and contradiction rules; lines 268-294 define escalation, communication, evidence-capture packets, `Gap`, and close criteria; lines 296-309 define format choices and accepted/rejected command records; lines 331-356 define grouped validation checklists.
Catalogue:
- Lifecycle/status vocabulary: local incident-process profile values are explicitly external to this standard. The runbook standard carries response-profile fields but not a universal severity taxonomy.
- Task records: response profile definition block, trigger template fields, triage and mitigation step records, mitigation `Safe mutation: none` packet, operational handoff record, escalation/communication/handoff packet.
- Runbook packets: packet fields include `Trigger`, `Impact`, `Profile`, `Hypothesis`, `Actions taken`, `Evidence`, `Blocked or unsafe next step`, `Communication`, and `Gap`.
- Proof fields: `Evidence` and `Gap` sit in response packets; recovery path proof routes to `proof.md` rather than a page-wide stamp.
- Checklists: validation checklists cover trigger/profile, triage/action, and closure/structure.
- Route-away records: operational handoff records use the shared relation fields and route topology, lifecycle tables, lookup catalogs, normal tasks, agent ramps, contributor workflows, and policy bodies away.
- Examples: profile definition block, profile-blocker packet, Mermaid flowchart with accessibility text, action records, safe-mutation packet, decision table, and accepted/rejected command record.
Active owner: `runbook.md` for symptom-to-response shape, profile-field placement, safe-mutation semantics, escalation/evidence packet shape, and operational handoff usage; incident-process docs for severity/profile values; `information-structure.md` for tables, records, and Mermaid container rules; `proof.md` for evidence and gap semantics; `formatting.md` for fenced block intent labels.
Ripple: `runbook.md` legitimately uses literal `none` for a narrow set of domain fields at line 47. This is a type-local exception to the general omit-absent rule in `docs/standards/AGENTS.md`, and it should remain explicitly bounded to runbook fields where `none` means a maintained local profile permits no obligation.
Decision: PROMOTE the exception boundary if the shared omit-absent rule is tightened: `none` may appear only when a type standard declares it as a domain value, not as filler for unknown or absent data.
Proof gap: The Mermaid diagram was read as source, but no renderer proof was run. The incident-process source is not checked because this report audits the runbook standard, not a produced operational runbook.

### [F4][AGENT_RAMP_SYSTEMS]

Path: `docs/standards/learning/onboarding.md`
Evidence: lines 15-20 define the authoring contract; lines 22-81 define required structure, conditional sections, conditional-addition template, section cardinality, and ramp-local availability; lines 83-107 define the ramp chooser; lines 109-124 define scope records and rejected broad area examples; lines 126-139 define source path records; lines 140-153 define read-first question records; lines 155-175 define constraint records and routes; lines 177-189 define first-safe-action records; lines 191-202 define readiness-gate records; lines 204-220 define stop-rule records and a compact table; lines 221-233 define generated-surface records; lines 235-250 define drift triggers and vocabulary placement; lines 276-296 define grouped validation checklists.
Catalogue:
- Lifecycle/status vocabulary: ramp-local availability set is `READY`, `PROVISIONAL`, `BLOCKED`, and `DROPPED`; the file explicitly omits `QUEUED`, `ACTIVE`, `DEFERRED`, `COMPLETE`, and `CANCELED`.
- Task records: source path, read-first, constraint, first safe action, readiness gate, stop rule, generated surface, and drift-trigger structures.
- Learning-path progress: this file rejects progress percentages, phase names, completion bars, role metadata, and permission metadata for ramp docs.
- Proof fields: `Evidence` and proof-gap wording appear inside source, first-safe-action, and readiness-gate records; `Source of truth` appears in generated-surface records.
- Checklists: validation checklists close scope routing, action gate, and density proof.
- Route-away records: read-first and constraint records carry `Route-away`; generated-surface records route generated contract meaning, generator operation, and evidence strength elsewhere.
- Examples: conditional section table, ramp chooser table, stop-rule table, compact definition blocks, and accepted/rejected area/reading examples.
Active owner: `onboarding.md` for agent ramp shape, availability vocabulary, first-safe-action and readiness-gate semantics, and stop rules; `information-structure.md` for status-tagged record requirements and table/record choice; `proof.md` for evidence fields; `formatting.md` for status marker rendering if availability is mirrored inline.
Ripple: The availability set is a narrowed domain-specific status vocabulary. It satisfies the shared requirement in `information-structure.md` because it declares exact casing, omitted default statuses, and removal behavior before record examples. If shared status rules are tightened, this file is the positive example for how to declare a narrowed type-local set.
Decision: KEEP and use as a model for type-local status declarations.
Proof gap: No produced ramp was checked for placeholder removal, link correctness, or readiness-gate execution.

### [F5][TUTORIAL_AND_LEARNING_PATH_SYSTEMS]

Path: `docs/standards/learning/tutorial.md`
Evidence: lines 19-25 define the authoring contract; lines 27-73 define single-tutorial structure, cardinality, and conditional learner-trap recovery; lines 75-91 define path and scope discipline; lines 92-101 define single tutorial versus learning-path structure choice; lines 115-176 define learning-path spine, availability vocabulary, path-entry records, and dependency ordering; lines 177-213 define end-state preview text and Mermaid examples with text equivalent; lines 214-240 define checkpoint step records; lines 241-260 define execution tags; lines 262-278 define result and `Done when` checklists; lines 279-303 define learner-trap table and H3 record escalation; lines 304-336 define next steps and execution closure; lines 338-360 define boundaries; lines 362-394 define grouped validation checklists.
Catalogue:
- Lifecycle/status vocabulary: path `Availability` is `AVAILABLE`, `DRAFT`, `BLOCKED`, `DEFERRED`, and `DROPPED`; learning paths explicitly omit `QUEUED`, `ACTIVE`, `COMPLETE`, and `CANCELED`.
- Execution vocabulary: step-level `Execution` tags are `NEEDS-FIXTURE` and `UNVERIFIED-REQUIRES-<X>`.
- Learning-path progress: completion is proven by result and validation, not progress bars. A path requires three or more tested lessons and ordered dependency consumption.
- Task records: path entries are subsection-per-record blocks with `ID`, `Availability`, `Changed fact`, `Consumed by`, `Use in this document`, `Depends`, `Evidence`, `Estimated time`, `Update when`, `Close when`, and `Route-away`; lesson steps are numbered checkpoint records with `Operation`, `Expected`, `Working state`, optional `Action`, optional `Execution`, conditional `Notice`, and optional `If wrong`.
- Proof fields: path entries and learner-trap rows use `Evidence`; execution tags express unverified dependencies and constrain publication state.
- Checklists: `Done when` checks and validation checklists assert result and closure.
- Route-away records: path-entry relation fields show lesson consumption; `Next steps` routes second lessons, variants, incidents, contribution workflow, and adjacent lookup away.
- Examples: title contrast, learning-path template, end-state text output, Mermaid diagram with text equivalent, step record, execution tag, result gate, learner-trap table, and learner-trap H3 record.
Active owner: `tutorial.md` for lesson shape, learning-path status, execution tags, checkpoint records, result gate, and learner-trap recovery; `information-structure.md` for subsection-per-record escalation, relation fields, checklists, and diagram/container choice; `proof.md` for evidence and execution proof; `style-guide.md` for copy-safe commands and exact terminology.
Ripple: Line 353 says lookup facts, command catalogs, option tables, and status vocabularies route to `reference.md`. That should not be read as moving tutorial `Availability` or `Execution` vocabularies out of the type standard, because those vocabularies control tutorial publication state and proof closure. The route-away applies to standalone lookup status vocabularies, not type-local publication status.
Decision: PROMOTE a clarification either in `tutorial.md` boundaries or the shared routing language: type-local status vocabularies stay in the type standard when they change produced-document validity, publication state, or proof closure.
Proof gap: Mermaid renderer proof, screenshot proof, and produced tutorial execution were not run; this report only audits the current standard text.

## [CROSS_FILE_CATALOGUE]

### [C1][STATUS_AND_LIFECYCLE_VOCABULARIES]

Files:
- `contributing.md`: no standalone lifecycle set; local gate/report fields use result markers and `Gap`.
- `how-to.md`: no standalone lifecycle set; outcome proof and rollback records are task-local.
- `runbook.md`: profile values route to a maintained incident-process source; `Safe mutation: none` is a declared domain value.
- `onboarding.md`: `READY`, `PROVISIONAL`, `BLOCKED`, `DROPPED`.
- `tutorial.md`: `AVAILABLE`, `DRAFT`, `BLOCKED`, `DEFERRED`, `DROPPED`; execution tags `NEEDS-FIXTURE` and `UNVERIFIED-REQUIRES-<X>`.
Active owner: `information-structure.md` lines 122-139 define the default status vocabulary and the rule that type standards may define domain-specific status sets only when they declare casing, omitted states, and removal behavior before examples.
Ripple: `onboarding.md` and `tutorial.md` are conforming positive cases. `formatting.md` line 44 also permits type-local markers only when the type standard declares the closed vocabulary before use.
Decision: KEEP. Future shared-owner work should reference these two learning files as examples of narrowed status sets instead of inventing another lifecycle vocabulary.
Proof gap: No current produced docs were scanned for misuse of these vocabularies.

### [C2][RELATION_AND_ROUTE_AWAY_RECORDS]

Files:
- `contributing.md` uses the relation record for documentation-change triggers.
- `how-to.md` uses it when adjacent truth controls procedure decisions, targets, artifacts, inputs, safety boundaries, or verification.
- `runbook.md` uses it when adjacent truth changes triage, safe mutation, escalation, verification, or responder routing.
- `tutorial.md` embeds relation fields in learning-path entries.
- `onboarding.md` uses `Route-away` in read-first and constraint records, but its records are not the full adjacent-document relation packet unless the ramp consumes adjacent truth.
Active owner: `information-structure.md` lines 160-189 define field order and relation-record use; `README.md` lines 114-129 define split/link behavior.
Ripple: The type files correctly specialize placeholders while preserving the shared field order. The main consolidation opportunity is a shared note that relation records can be embedded into type-specific records when the type declares lifecycle or publication state.
Decision: PROMOTE this clarification to `information-structure.md` only if later reports find agents treating embedded relation fields as invalid duplication.
Proof gap: No conflicting active-standard example was found in this read set.

### [C3][CHECKLIST_SYSTEMS]

Files:
- `contributing.md` uses PR self-check and validation checklists.
- `how-to.md` uses verification checklists only when multiple independent outcome conditions must hold.
- `runbook.md` uses validation checklists, while response/evidence closure stays in records and packets.
- `onboarding.md` uses validation checklists and rejects progress bars.
- `tutorial.md` uses `Done when` and validation checklists.
Active owner: `information-structure.md` lines 195-211 define verification, acceptance, and status checklists; `formatting.md` lines 175-180 define field-line rendering for checklist fields.
Ripple: No type file should promote its checklist fields into a generic status checklist unless the checklist item carries ongoing `Status`, `Depends`, or proof fields. The task/learning files mostly use verification checklist form.
Decision: KEEP current split. Shared owner is sufficient.
Proof gap: No Markdown task-list renderer or link/anchor validation was run.

### [C4][RUNBOOK_AND_TASK_PACKETS]

Files:
- `contributing.md`: contribution path selector, quality gate record, PR packet, review-profile record.
- `how-to.md`: command step, prerequisite record, rollback record, troubleshooting record.
- `runbook.md`: profile block, response packet, `Safe mutation: none`, escalation/evidence packet.
- `onboarding.md`: source path, read-first, first safe action, readiness gate, stop rule, generated surface.
- `tutorial.md`: path-entry record, checkpoint step record, execution tag, learner-trap table or H3 record.
Active owner: type files for packet semantics; `information-structure.md` lines 336-349 for packet categories; `proof.md` lines 37-68 for proof field order; `style-guide.md` lines 76-82 for condition-before-action wording.
Ripple: `information-structure.md` names only a small subset of task packets: how-to command step, runbook packet, adjacent-route record. This report finds more stable packet families in contributing, onboarding, and tutorial. A future promotion could expand the shared catalogue without moving field semantics out of the type files.
Decision: PROMOTE a shared catalogue expansion only if synthesis wants a single retrieval map for packet families. Do not centralize all packet fields; type owners need the domain semantics.
Proof gap: No generated index consumes these packet names today in the read set.

### [C5][EXAMPLES_AND_RENDERED_VISUALS]

Files:
- `how-to.md` uses compact records and decision tables; it allows Mermaid only when branch sequence and rejoin need rendering.
- `runbook.md` includes a Mermaid response path and text equivalent.
- `tutorial.md` includes exact output preview, Mermaid artifact preview, and text equivalent.
- `contributing.md` and `onboarding.md` rely mostly on template fences, tables, definition blocks, and compact contrast examples.
Active owner: `information-structure.md` lines 266-304 define fence intent labels, lines 436-452 define Mermaid use, and lines 575-582 define examples; `formatting.md` lines 206-215 define heading idiom and marker families; `proof.md` lines 154-165 define renderer-claim proof obligations.
Ripple: The type files use `mermaid` fences correctly as renderer-local fences. Any claim that these examples render correctly needs local renderer proof or an explicit proof gap.
Decision: KEEP examples local. PROMOTE no renderer claim without running the renderer.
Proof gap: Mermaid render proof and docs-build proof were not run.

### [C6][CO_LOCATION_AND_ROUTE_BOUNDARIES]

Files:
- `contributing.md` routes onboarding, normal procedures, runbooks, test strategy, support matrix, roadmap, security policy, and repository policy away before workflow examples.
- `how-to.md` routes teaching, operational recovery, contribution workflow, lookup catalogs, support facts, and concepts away.
- `runbook.md` routes severity taxonomy, communication policy, postmortem templates, normal tasks, and topology background away.
- `onboarding.md` routes tutorials, how-to guides, runbooks, contributing guides, test strategy, architecture, API, support matrix, reference, and README choices away.
- `tutorial.md` routes competent-reader procedures, agent ramps, runbooks, contributing workflow, concepts, planned lesson sequence, API, reference, support matrix, and code documentation away.
Active owner: `README.md` for reader-need routing and split/link rules; type files for boundary language that changes local author action.
Ripple: The corpus has strong co-location discipline, but one boundary needs clarification: `tutorial.md` route-away for status vocabularies should not pull its own `Availability` vocabulary into reference. See F5.
Decision: PROMOTE F5 clarification; otherwise keep route-away records type-local and shared split/link rules central.
Proof gap: No full active-corpus route audit beyond the requested files was performed.

## [RECOMMENDATIONS]

1. Promote a narrow shared rule for domain values such as runbook `none`: a type standard may declare a literal absence-like value only when it is a domain value with explicit allowed fields and is not a placeholder for unknown proof.
2. Clarify that `reference.md` owns standalone lookup status vocabularies, while type standards own local status vocabularies that determine produced-document validity, publication state, readiness, or proof closure.
3. Consider expanding the shared packet catalogue in `information-structure.md` to include contribution PR packets, onboarding ramp records, and tutorial checkpoint/path records as retrieval names only. Keep field semantics in the type standards.
4. Preserve the current type-local placement of `Optional:`, `Irreversible:`, `Safe mutation: none`, `Availability`, and `Execution` tags. They change reader action in their type contexts and should not become decorative global notation.

## [PROOF_GAPS]

- No active standards were edited.
- No current external-source research was required because this report makes no new provider, product, renderer, or tooling-behavior claim.
- No Mermaid renderer, docs build, link checker, anchor checker, or produced-document validation gate was run.
- The session folder `.reports/standards-structure-notation-060626/` has track directories but no `README.md` manifest at the time of this report. Per `.reports/AGENTS.md`, that is an archive-maintenance gap, not a blocker for this named single report.
