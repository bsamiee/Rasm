Question: Which report-session frames in the standards-structure-notation task list and lane reports must stay out of active standards promotion?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: standards-structure-notation-060626 :: report-boundary leakage :: correct before promotion
Target owner: `docs/standards/_reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`; `docs/standards/_reports/standards-structure-notation-060626/README.md`; `docs/standards/_reports/AGENTS.md`
Source basis: mandatory instruction files, session manifest, collective task list, and representative reports from root-corpus, information-structure, formatting-notation, type-corpus, repo-markdown, and external-research tracks
Promotion target: none yet
Outcome: CORRECT

## [FINDINGS]

### [F1][WAVE_AND_REVIEW_GATE_LANGUAGE]

Task-list refs:
- `track-synthesis/00-collective-task-list.md:5-6` uses `wave-one`, `wave-two`, `wave-three`, and `review-agent pass` as the implementation gate.
- `track-synthesis/00-collective-task-list.md:64-65`, `112-113`, and `468-469` use wave or review-agent language as proof-gap or validation wording.
- `track-synthesis/00-collective-task-list.md:668-680` correctly says source-scan record principles should promote without prompt wave choreography, but still names `transcript-boundary critique` as validation.
- `README.md:71` rejects durable wave framing; `README.md:75` still says promotion happens after wave-two research and wave-three critique.

Evidence:
- `docs/standards/AGENTS.md:11` says reports, session history, task framing, fixed wave counts, and report structure are source material only.
- `docs/standards/_reports/AGENTS.md:21-23`, `80`, `105`, `149`, and `158` reject phase, wave, worker-role, transcript, validation-boilerplate, and report-frame promotion.
- `track-repo-markdown/01-claude-markdown-patterns.md:17-20` identifies the useful field contract but says wave, phase, and sub-agent language is task-process shape.

Issue: The task list already knows wave choreography must not promote, but its own top-level implementation rule and several validation/proof-gap fields still encode waves and review-agent passes as named prerequisites. A later implementation pass can copy those phrases into active standards as if they were durable process policy.

Proposed correction: Normalize task-list and manifest wording before promotion:
- Replace `wave-one corpus reports` with `prior corpus reports`.
- Replace `wave-two research` with `source research`.
- Replace `wave-three critique` with `adversarial critique`.
- Replace `review-agent pass` with `owner-route review` or `critique report`.
- Replace `transcript-boundary critique` with `report-boundary critique`.

Disposition: CORRECT the session task list and manifest wording. Do not promote any wave number, worker role, or review-agent label into active standards.

### [F2][SUB_AGENT_SOURCE_AS_REPORT_LEDGER]

Task-list refs:
- `track-synthesis/00-collective-task-list.md:15`, `29`, `45`, `59`, `77`, `91`, `107`, `121`, `135`, `149`, `163`, `177`, `191`, `205`, `219`, `233`, `247`, `261`, `275`, `289`, `305`, `319`, `333`, `347`, `361`, `375`, `391`, `405`, `419`, `433`, `447`, `463`, `479`, `497`, `511`, `525`, `541`, `555`, `569`, `583`, `599`, `613`, `629`, `643`, `659`, `673`, `689`, `705`, `719`, `733`, `749`, and `763` use `Sub-agent source`.
- `README.md:71` tells later agents to read tracks named by a task's `Sub-agent source`.

Evidence:
- `docs/standards/_reports/AGENTS.md:84-95` defines report identity fields and does not include worker identity.
- `docs/standards/_reports/AGENTS.md:141-143` says promoted rules must name owner files and report paths, not report roles.
- `docs/standards/_reports/AGENTS.md:157-158` rejects worker roles, prompt wording, task narration, and fixed agent counts in active owners.

Issue: `Sub-agent source` is acceptable as session-local provenance, but it is a worker-frame field. If copied into active standards, it trains agents to preserve report production roles rather than source paths, owner routes, and proof classes.

Proposed correction: In the task list, rename the field to `Source reports:` or `Evidence reports:`. In active standards, collapse it further to `Evidence:` or `Source basis:` and list only durable source classes or report paths. Do not preserve `Sub-agent`, `main-agent`, or similar role labels in promoted wording.

Disposition: MERGE the underlying provenance. DROP the worker-frame field name from any active-standard candidate wording.

### [F3][VALIDATION_BOILERPLATE_AND_UNRUN_GATES]

Task-list refs:
- `track-synthesis/00-collective-task-list.md:21` and `35` record session-file and review critique checks as validation.
- `track-synthesis/00-collective-task-list.md:112-113`, `469`, and `679` use review-agent or transcript-boundary review as validation.
- `track-synthesis/00-collective-task-list.md:742-756` correctly scopes report-session whitespace proof to `_reports`.
- `track-synthesis/00-collective-task-list.md:758-770` correctly scopes active-standard proof after implementation, but it must not imply link, anchor, docs-build, or renderer gates exist unless configured.

Report refs:
- `track-external-research/02-mermaid-renderer-proof.md:217-221` has a top-level `[VALIDATION]` section for a report path.
- `track-external-research/03-information-architecture-structure-chooser-a.md:245-249`, `track-formatting-notation/01-formatting-token-systems.md:66-69`, and `track-type-corpus/03-task-learning-type-corpus.md:182-187` correctly keep unrun gates as proof gaps instead.

Evidence:
- `docs/standards/_reports/AGENTS.md:9` says `_reports/` does not own validation gates.
- `docs/standards/_reports/AGENTS.md:97-105` allows `[PROOF_GAPS]` but forbids top-level `VALIDATION`.
- `docs/standards/AGENTS.md:56` says read-only audits state no validation gate ran; edit audits name exact commands only when they ran or current status proves them.
- Root `AGENTS.md:87` forbids static, test, bridge, docs build, renderer, provider, CI, package, deploy, publish, or tool-pass claims unless the exact command ran in the change and the changed surface owns that gate.

Issue: The task list mixes three different things under `Validation`: report-session whitespace close, future active-standard proof, and critique/review steps. The Mermaid report also introduces a forbidden top-level report `VALIDATION` section. If these frames promote, active standards can become a validation ledger instead of a proof selector.

Proposed correction:
- In reports, move path-specific `git diff --check` reminders into `[PROOF_GAPS]` before execution or omit them after execution. Do not use top-level `[VALIDATION]`.
- In the task list, replace review-style validation labels with `Close proof:` only when a concrete command or source check is selected.
- Keep active proof selection under `docs/standards/proof.md`; promote only the configured-gate distinction from `track-synthesis/00-collective-task-list.md:400-412`.
- Phrase unavailable link, anchor, docs-build, renderer, or provider gates as `Proof gap:` unless current repo source or current command output names the configured gate.

Disposition: CORRECT report and task-list validation wording before active standards are edited.

### [F4][STALE_ARCHIVE_MECHANICS]

Task-list refs:
- `track-synthesis/00-collective-task-list.md:10-22` marks `Create the missing session manifest` as `COMPLETE`.
- `README.md:3-9` now provides the session card and boundary.

Report refs:
- `track-root-corpus/01-root-shared-corpus.md:67-76` says the session README is absent and recommends creating it later.
- `track-information-structure/01-information-structure-container-toolbelt.md:176-177`, `track-repo-markdown/01-claude-markdown-patterns.md:23` and `179`, and `track-type-corpus/03-task-learning-type-corpus.md:187` retain missing-manifest proof gaps.

Evidence:
- `docs/standards/_reports/AGENTS.md:121-125` says current repository source wins when it conflicts with a report and stale report findings need correction.
- `docs/standards/_reports/AGENTS.md:147-151` says reports that only preserve archive mechanics, validation boilerplate, or duplicated synthesis should be deleted or marked `DROPPED` after promotion.

Issue: Several representative reports still contain a now-stale archive-maintenance gap. The task list has the correct current status, but the stale claims remain easy to quote as if the manifest is still missing.

Proposed correction: Add a manifest `[CORRECTIONS]` entry or a small correction report naming each stale missing-manifest claim, with disposition `SUPERSEDED` by `README.md`. Do not promote missing-manifest text, one-file-write constraints, or archive-maintenance chronology into active standards.

Disposition: CORRECT stale report claims through report-session correction mechanics. Active standards need no change.

### [F5][REPORT_SECTION_FRAMES_AS_FALSE_STANDARDS]

Report refs:
- `track-external-research/02-mermaid-renderer-proof.md:10-42` uses `[SOURCE_SET_READ]` and `217-221` uses `[VALIDATION]`.
- `track-external-research/03-information-architecture-structure-chooser-a.md:184-200` uses `[USER_CHOICES]`.
- `track-root-corpus/01-root-shared-corpus.md:78-130` uses `[SYSTEM_CATALOG]`.
- `track-type-corpus/03-task-learning-type-corpus.md:96-173` uses `[CROSS_FILE_CATALOGUE]`.

Evidence:
- `docs/standards/_reports/AGENTS.md:97-103` lists the allowed report sections.
- `docs/standards/_reports/AGENTS.md:105` says to preserve source lists and proof gaps, but remove process narration.
- `docs/standards/_reports/AGENTS.md:141` says to promote rules, not report frames.

Issue: The reports contain useful material, but several top-level section names are report-local conveniences rather than normalized report shape. If copied into active standards, they would create new section families that describe the research process instead of document behavior.

Proposed correction:
- Move source-read lists under `[EVIDENCE]`.
- Move user-choice blockers under `[PROOF_GAPS]` or `[RECOMMENDATIONS]`.
- Convert system catalogues and cross-file catalogues into either `[EVIDENCE]` source summaries or active-owner candidate wording.
- Do not promote report section names as active-standard headings.

Disposition: MERGE the evidence and candidate wording. DROP the report frame names from active promotions.

### [F6][SOURCE_MATERIAL_SCOPE_FOR_CLAUDE_AND_REPO_MARKDOWN]

Task-list refs:
- `track-synthesis/00-collective-task-list.md:4` includes `.claude/**` and targeted repository Markdown in session scope.
- `track-synthesis/00-collective-task-list.md:654-666` holds whether `.claude/prompts/**` and `.claude/skills/**` are standards-governed ripple surfaces.
- `track-synthesis/00-collective-task-list.md:668-680` asks to promote source-scan record principles without prompt wave choreography.
- `track-synthesis/00-collective-task-list.md:684-696` holds broad repo Markdown ripple until standards wording lands.
- `track-synthesis/00-collective-task-list.md:728-740` blocks `.claude` governance on user choice and provider skill-format research.

Evidence:
- `README.md:41-45` classifies `.claude/**` and non-standards Markdown as source material.
- `track-repo-markdown/01-claude-markdown-patterns.md:171-175` recommends stripping wave and worker-role narration, keeping report decisions report-local, treating provider and renderer claims as proof gaps, and preserving `.claude` prompt assets as useful source material rather than authority.
- Root `AGENTS.md:91-93` says prompt assets, reports, and external research are evidence only unless a trusted owner promotes the durable rule.

Issue: The task list correctly blocks broad ripple, but several owner-file rows name active standards plus `.claude` or `_reports/AGENTS.md` together. That can blur three surfaces: active documentation standards, report-lane mechanics, and prompt or skill assets.

Proposed correction:
- For `Promote source-scan record principles without prompt wave choreography`, route report-specific field order only to `_reports/AGENTS.md`.
- Route generic task-output provenance principles to without naming `.claude` prompts.
- Route carrier shape to `information-structure.md` only as a generic field-packet rule, not a report contract copied from `.claude/prompts/fix-standards-docs.md`.
- Keep `.claude/**` and targeted repo Markdown as source material until the blocked user-choice tasks resolve.

Disposition: HOLD `.claude` and broad repo ripple. PROMOTE only generic, owner-routed principles.

### [F7][LATEST_AND_INSTALLED_TOOLING_CLAIMS]

Task-list refs:
- `track-synthesis/00-collective-task-list.md:386-398` asks for a renderer-support proof packet and correctly notes that upstream Mermaid latest is not local installed truth.
- `track-synthesis/00-collective-task-list.md:400-412` asks to separate configured docs gates from desired gate classes.

Report refs:
- `track-external-research/02-mermaid-renderer-proof.md:174-194` compares upstream `mermaid-cli` `11.15.0` with local installed `11.14.0`.

Evidence:
- Root `AGENTS.md:77` requires current source, tool output, generated contract proof, maintained provider docs, or explicit proof route for current behavior claims.
- `docs/standards/_reports/AGENTS.md:157-158` rejects provider manuals, command gates, and report source material as active authority.

Issue: The upstream-vs-local split is a useful proof concept, but active standards should not preserve a transient upstream latest release number unless the standard itself owns a freshness record and review trigger. The durable rule is evidence-class separation, not the June 2026 release comparison.

Proposed correction: Promote the proof rule that maintained upstream docs prove generic syntax, while local manifests and command output prove available repository tooling. Keep exact upstream latest versions in reports or proof records only when the active standard has `Last verified:` and `Review trigger:` and the number changes reader action.

Disposition: MERGE evidence-class split. DROP transient latest-version comparisons from active standards unless a proof owner explicitly needs them.

## [RECOMMENDATIONS]

1. Before editing active standards, normalize `track-synthesis/00-collective-task-list.md` field names: `Sub-agent source` becomes `Source reports`, wave labels become source-class labels, and review/transcript validation becomes either concrete close proof or a proof gap.
2. Add a session manifest correction entry for stale missing-README claims in representative reports.
3. Treat report-only sections such as `[SOURCE_SET_READ]`, `[USER_CHOICES]`, `[SYSTEM_CATALOG]`, `[CROSS_FILE_CATALOGUE]`, and `[VALIDATION]` as source-material frames. Promote only their durable findings through owner files.
4. Keep active standards free of `.claude` prompt names, worker roles, wave counts, report-validation reminders, and archive chronology.
5. When a promoted rule comes from reports, cite report paths as evidence and restate the rule in the active owner's vocabulary.

## [EVIDENCE]

Instruction and boundary files read:
- `CLAUDE.md`.
- `AGENTS.md`.
- `docs/standards/README.md`.
- `docs/standards/AGENTS.md`.
- `docs/standards/_reports/AGENTS.md`.
- `docs/standards/_reports/standards-structure-notation-060626/README.md`.
- `docs/standards/_reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`.

Representative reports read:
- `track-root-corpus/01-root-shared-corpus.md`.
- `track-information-structure/01-information-structure-container-toolbelt.md`.
- `track-formatting-notation/01-formatting-token-systems.md`.
- `track-type-corpus/03-task-learning-type-corpus.md`.
- `track-repo-markdown/01-claude-markdown-patterns.md`.
- `track-external-research/02-mermaid-renderer-proof.md`.
- `track-external-research/03-information-architecture-structure-chooser-a.md`.

Session inventory used:
- `fd -t f . docs/standards/_reports/standards-structure-notation-060626 | sort`.
- `wc -l docs/standards/_reports/standards-structure-notation-060626/track-*/*.md docs/standards/_reports/standards-structure-notation-060626/README.md`.

## [PROOF_GAPS]

- This critique did not edit active standards.
- This critique did not render Mermaid, run link checks, run anchor checks, or verify provider behavior.
- This critique sampled representative reports from every populated track; it did not reread every external-research report line by line.
