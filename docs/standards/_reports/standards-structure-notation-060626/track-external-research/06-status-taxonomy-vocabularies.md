Question: What should Rasm standardize for status taxonomies, controlled vocabularies, lifecycle states, terminal or blocked states, and display value versus machine value?
Type: standards-research
Lane: track-external-research
Merge key: information-structure.md :: status taxonomy vocabularies :: promote
Target owner: docs/standards/information-structure.md; docs/standards/formatting.md; affected type standards
Source basis: maintained external specifications and product docs plus active local standards read for `information-structure.md`, `formatting.md`, `proof.md`, `adr.md`, `architecture.md`, `design-doc.md`, `roadmap.md`, `test-strategy.md`, `support-matrix.md`, `onboarding.md`, `tutorial.md`, and `runbook.md`
Promotion target: docs/standards/information-structure.md for closed vocabulary cards; docs/standards/formatting.md for glyph and marker rendering; type standards for local status values
Outcome: PROMOTE

## [FINDINGS]

### [1][CONTROLLED_VOCABULARIES_NEED_CONCEPT_CARDS]

Controlled vocabulary sources converge on a simple rule: define the concept set, name the preferred term or label, preserve alternates or hidden labels only as retrieval aids, and state relationships explicitly. SKOS separates concepts from `prefLabel`, `altLabel`, and `hiddenLabel`; NISO Z39.19 treats equivalence, hierarchy, and association as explicit relationship classes and requires reciprocal relationship expression. That supports a Rasm vocabulary card rather than more prose inside `Structured records`.

Local evidence already points there. `information-structure.md:128` defines the default `Status` set and permits type-local sets when they declare casing, active, blocked, returnable, terminal, and removal behavior. `track-information-structure/02-information-structure-cross-type-fit.md:87` already recommends a `[CLOSED_VOCABULARIES]` section, and `track-synthesis/00-collective-task-list.md:116-126` records the same task with the unresolved display-value question.

Recommendation state: `ADD` a first-class closed vocabulary card in `information-structure.md`.

### [2][MACHINE_VALUE_AND_DISPLAY_VALUE_MUST_SEPARATE]

API and platform sources repeatedly separate machine-oriented values from human explanation. Google AIP-126 requires enum values to include a deterministic unspecified zero value because generated client libraries differ in default behavior. Redocly documents OpenAPI enum values as short and machine-readable, then adds `x-enumDescriptions` so generated docs can explain the values. Kubernetes gives the strongest documentation analogy: it warns not to confuse the `kubectl` display `STATUS` field with the Pod API `phase`, which is the explicit data-model field.

Rasm has the same split today. ADR `Status` is lowercase semantic vocabulary at `adr.md:61-67`; support labels are reader-facing display states at `support-matrix.md:162`; roadmap milestone status is uppercase lifecycle at `roadmap.md:74-83`; architecture path-state markers are bracketed inline editing cues at `architecture.md:182-234`. A shared rule that forces one casing or one spelling family across all of them would destroy useful domain meaning.

Recommendation state: `ADD` `Machine value` and `Display value` fields to the vocabulary card, with omission allowed when they are identical.

### [3][STATUS_IS_NOT_RESULT_OR_CONCLUSION]

GitHub status checks separate an in-progress check `status` from the terminal `conclusion` once the check is completed. Kubernetes separates Pod phase from container state and from display-only kubectl status. Temporal and Durable Task style workflow APIs separate running state from multiple terminal outcomes such as completed, failed, canceled, terminated, timed out, or continued-as-new. These sources support the current local instinct: status values must not flatten lifecycle, result, support policy, and operational display into one family.

Local status families already encode this distinction: `formatting.md:23-30` separates result, change, lifecycle, compact glyph, and explicit state token families; `test-strategy.md:142` says quarantine statuses are not lifecycle `Status`; `design-doc.md:308` uses final-check state values separate from proposal lifecycle status; `tutorial.md:245` defines execution tags that are not path availability values.

Recommendation state: `KEEP` separate families and `CHANGE` the shared standard to make family declaration mandatory before first use.

### [4][BLOCKED_DEFERRED_AND_TERMINAL_STATES_REQUIRE_EXIT_EVENTS]

State-machine sources make transition semantics explicit: SCXML defines active state configurations and transitions, while Kubernetes documents terminal Pod phases and deletion behavior. GitHub marks stale conclusions for checks left incomplete too long. Asana dependency docs distinguish a blocked task from the task that blocks it, making the unblocking dependency part of the state fact rather than a synonym for "not started".

Local standards already need that precision. `roadmap.md:83` states that `DEFERRED` keeps its ID and needs a return event; `onboarding.md:73` narrows availability to `READY`, `PROVISIONAL`, `BLOCKED`, and `DROPPED`; `runbook.md:289` uses `Blocked or unsafe next step` as an action field, not a lifecycle value. The vocabulary card should require `Entry condition`, `Exit or return event`, and `Removal behavior` for blocked, deferred, terminal, and dropped values.

Recommendation state: `ADD` transition and removal fields to the vocabulary card.

### [5][GLYPHS_ARE_RENDERING_NOT_SEMANTICS]

Accessibility and technical-writing sources agree that meaning cannot rely on color or symbols alone. Google technical-writing guidance recommends combining text, color, and symbols so removing the visual cue does not remove meaning. Clarity Design System requires status icons to be labeled for assistive technology. That supports the local report finding that compact glyphs cannot carry implicit per-author meanings.

Local formatting already states the boundary: `formatting.md:7-18` says formatting renders lifecycle and status semantics after another standard defines them, and `formatting.md:44-48` says type-local markers are valid only after a closed vocabulary declaration. Existing report `track-formatting-notation/02-formatting-cross-corpus-usage.md:69-71` found that compact glyphs are listed globally but mostly unmapped.

Recommendation state: `CHANGE` glyph handling so status semantics live in the vocabulary card and glyphs live in formatting as a declared projection only.

## [EVIDENCE]

External sources:
- W3C SKOS Reference: `skosxl:prefLabel`, `skosxl:altLabel`, and `skosxl:hiddenLabel` model preferred, alternate, and hidden labels for concepts. Source: https://www.w3.org/TR/skos-reference/
- ANSI/NISO Z39.19-2005 (R2010): controlled vocabularies use equivalence, hierarchy, and association relationships; equivalence selects a preferred term when multiple terms express one concept. Source: https://www.anzsi.org/wp-content/uploads/2019/05/z39-19-2005r2010.pdf
- W3C SCXML: state machines have active state configurations, transitions, final states, and legal state specifications. Source: https://www.w3.org/TR/scxml/
- Kubernetes Pod lifecycle: Pod `phase` values are data-model values, while some kubectl `STATUS` values are display fields; terminal phases are `Failed` or `Succeeded` for deleted Pods with documented exceptions. Source: https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/
- GitHub status checks: checks have statuses while moving toward completion; a completed check has a conclusion value. Source: https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/collaborating-on-repositories-with-code-quality-features/about-status-checks
- Google AIP-126 linter rule: enum values need an explicit unspecified first value for generated client-library safety. Source: https://linter.aip.dev/126/unspecified
- Redocly `x-enumDescriptions`: OpenAPI enum values stay short and machine-readable; descriptions make them human-readable in docs. Source: https://redocly.com/docs/realm/content/api-docs/openapi-extensions/x-enum-descriptions
- Google technical writing accessibility: do not rely on color alone; pair text, color, and symbols. Source: https://developers.google.com/tech-writing/accessibility/self-study/visual-cues
- Clarity Design System icon accessibility: status icons need accessible labels when they convey indicator meaning. Source: https://core.clarity.design/foundation/icons/accessibility/
- Microsoft Durable Task runtime status: orchestration runtime status separates running, pending, suspended, and multiple terminal outcomes. Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.durabletask.client.orchestrationruntimestatus

Local source anchors:
- `docs/standards/information-structure.md:128-139`: default status vocabulary plus type-local declaration rule.
- `docs/standards/formatting.md:23-48`: token families and type-local marker rule.
- `docs/standards/explanation/adr.md:61-67`: lowercase ADR dispositions and terminal behavior.
- `docs/standards/explanation/architecture.md:182-234`: architecture path-state markers and removal trigger.
- `docs/standards/explanation/design-doc.md:43-60`: design lifecycle groups and proposal statuses.
- `docs/standards/explanation/roadmap.md:67-83`: roadmap document state and milestone status.
- `docs/standards/explanation/test-strategy.md:142`: quarantine status values are not lifecycle status.
- `docs/standards/reference/support-matrix.md:133-164`: support display labels and active or terminal reader-decision states.
- `docs/standards/learning/onboarding.md:73-79`: ramp-local availability vocabulary omits shared lifecycle states.
- `docs/standards/learning/tutorial.md:146-153`: learning-path availability vocabulary and removal behavior.
- `docs/standards/task/runbook.md:32-60`: response profile fields and local incident-process source requirement.

## [RECOMMENDATIONS]

### [1][CLOSED_VOCABULARY_CARD]

Promote a shared card in `information-structure.md` before `Structured records`. Use it for every status, availability, phase, support, result, execution, profile, or marker family whose values are closed or locally projected.

Candidate card fields:
- `Vocabulary`: stable name of the value family.
- `Applies to`: field, record family, table column, marker projection, or produced document type.
- `Semantic kind`: lifecycle, result, support policy, availability, execution tag, runtime state, display status, profile, or glyph projection.
- `Values`: exact allowed values in source casing.
- `Machine value`: exact parser, API, table, or field value; omit when identical to `Display value`.
- `Display value`: reader-facing label; omit when identical to `Machine value`.
- `Meaning`: local semantics, not just synonym.
- `Active states`: values that keep work or support live.
- `Blocked states`: values that cannot advance without a dependency, access grant, proof, or decision.
- `Returnable states`: values that can re-enter active flow and the required return event.
- `Terminal states`: values that close normal advancement.
- `Omitted shared states`: default lifecycle values intentionally unavailable in this vocabulary.
- `Initial value`: required only when a state machine or generated enum needs one.
- `Entry condition`: event or proof that permits the value.
- `Exit or return event`: event that changes the value.
- `Projection rule`: mapping to bracketed markers, glyphs, progress, support labels, or adjacent lifecycle records.
- `Removal behavior`: when a record remains, routes away, or is deleted.
- `Unknown value rule`: how unknown, unspecified, omitted, `null`, not announced, or not applicable values differ.
- `Evidence`: source path, maintained source, command, generated contract, or proof gap.
- `Review trigger`: event that makes the vocabulary stale.

### [2][TYPE_LOCAL_VALUES_STAY_LOCAL]

Keep domain values in type standards. `information-structure.md` should own the card shape, required semantics, and omission rule; `formatting.md` should own rendered tokens and glyph spelling; type standards should own the actual ADR, roadmap, support, onboarding, tutorial, design, test, and runbook values.

This avoids a central enum registry that would become stale and would blur values that intentionally differ: ADR `accepted`, roadmap `COMPLETE`, support `End of support`, and tutorial `AVAILABLE` do not mean the same thing.

### [3][GLYPH_STATUS_SEPARATION]

Use this promotion rule:
- A `Status:` field stores the exact vocabulary value, unbracketed unless the domain source itself uses brackets.
- A bracketed marker such as `[ACTIVE]` is an inline projection for scanning, not the source value.
- A compact glyph such as `[x]` or `[?]` is valid only when the local surface declares a closed glyph alphabet before first use.
- A glyph projection must name the vocabulary value it projects from and provide adjacent text when the glyph carries meaning.
- Do not use glyphs or color as the only status carrier.

### [4][USER_CHOICES]

These choices block exact active-standard wording:
- Display and machine value: choose whether every vocabulary card must always include both fields or whether one field may be omitted when identical. Recommendation: omit identical fields to keep cards short, but require both when casing, punctuation, localization, API value, or reader label differs.
- Compact glyph alphabet: choose global map, local-declare-only rule, or minimal global `[x]` fail marker only. Recommendation: local-declare-only, with `[x]` retained only as the current compact fail marker until a global map is chosen.
- Unknown and unspecified: choose whether the shared card mandates an explicit `Unknown value rule` for every vocabulary or only when unknowns can appear. Recommendation: require it for API, support, generated, imported, or external-source vocabularies; omit it for closed local authoring vocabularies where unknown cannot be valid.
- Projection labels: choose whether `Projection rule` may map to lifecycle categories without requiring visible bracketed markers. Recommendation: yes; projections should include non-rendered lifecycle grouping because support and ADR statuses should not be forced into `[ACTIVE]` or `[COMPLETE]`.

## [CANDIDATE_WORDING]

Add to `information-structure.md` before `Structured records`:

```markdown template
## [N][CLOSED_VOCABULARIES]

Declare a closed vocabulary before the first field, row, marker, diagram label, or record that uses it. Use a vocabulary card when values are filtered, parsed, projected, updated independently, or mapped to lifecycle, result, support, availability, or display status. The card owns the value family; type standards own domain values, and formatting owns rendered markers.

Vocabulary: <stable value-family name>
Applies to: <field, table column, record family, marker projection, or produced document type>
Semantic kind: <lifecycle | result | support policy | availability | execution tag | runtime state | display status | profile | glyph projection>
Values: <exact allowed values in source casing>
Machine value: <parser, API, table, or field value; omit when identical to display>
Display value: <reader-facing label; omit when identical to machine>
Active states: <values that keep the item live>
Blocked states: <values that require a named dependency, access grant, proof, or decision>
Returnable states: <values that can re-enter active flow, plus return event>
Terminal states: <values that close normal advancement>
Omitted shared states: <default lifecycle values intentionally unavailable>
Initial value: <required only when a state machine or generated enum needs one>
Entry condition: <event or proof that permits the value>
Exit or return event: <event that changes the value>
Projection rule: <mapping to markers, glyphs, progress, support labels, or adjacent lifecycle records>
Removal behavior: <when records remain, route away, or are deleted>
Unknown value rule: <unknown, unspecified, omitted, null, not announced, or not-applicable handling>
Evidence: <source path, maintained source, command, generated contract, or proof gap>
Review trigger: <event that makes the vocabulary stale>
```

Add to `formatting.md` under status/result markers:

```markdown template
Formatting renders status; it does not define status semantics. A bracketed lifecycle marker mirrors a declared vocabulary value for inline scanning. A compact glyph is valid only when the local surface declares a closed glyph alphabet, maps each glyph to a vocabulary value or result, and provides adjacent text when the glyph carries meaning. Do not use glyphs, color, or badges as the only status carrier.
```

## [PROOF_GAPS]

- User choice is still needed for display-versus-machine field cardinality, compact glyph policy, and unknown-value cardinality.
- No active standards were edited in this pass.
- External research used maintained sources and stable specifications; no local renderer, link checker, or docs build claim is made by this report.
