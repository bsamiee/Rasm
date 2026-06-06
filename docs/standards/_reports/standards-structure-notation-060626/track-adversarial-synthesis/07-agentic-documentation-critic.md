Question: Are agent-facing task-output, provider-loading, prompt, skill, and retrieval-provenance findings bounded so provider claims do not leak without proof?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: docs/standards/agentic-documentation.md :: agent-facing provider and provenance boundaries :: critique synthesis bounds
Target owner: docs/standards/agentic-documentation.md; docs/standards/agents-md.md; docs/standards/proof.md; docs/standards/information-structure.md; docs/standards/_reports/AGENTS.md
Source basis: current active standards, `.claude/README.md`, `.claude/prompts/fix-standards-docs.md`, session manifest, collective task list, `.claude` repo scan, provider/proof external reports, machine-consumed Markdown report
Promotion target: track-synthesis/00-collective-task-list.md first; active owner files only after accepted correction routing
Outcome: MERGE

## [PRIOR_PASS]

This pass extends:
- `track-root-corpus/01-root-shared-corpus.md` [F2] and [F5], which held provider-loading and strict-schema mechanics for current provider proof.
- `track-repo-markdown/01-claude-markdown-patterns.md` [F1], [F7], [F8], and [F10], which treated `.claude/**` as source material and held skill/provider claims.
- `track-external-research/07-proof-uncertainty-stale-sources.md` [Finding 5], which says source provenance is not semantic validation.
- `track-external-research/10-machine-consumed-markdown-ledgers.md`, which adds parser-owned Markdown as a no-normalize exception.
- `track-synthesis/00-collective-task-list.md` items at lines 86-96, 242-266, 342-352, 446-452, 654-678, and 728-738.

The existing synthesis is directionally correct. It blocks the most dangerous provider claims. The gaps are boundary precision: provider proof, user governance choices, report-shape promotion, skill-format proof, retrieval provenance, and parser-owned Markdown need separate stop conditions so one unresolved class does not authorize another.

## [FINDINGS]

### [F1][PROVIDER_PROOF_AND_GOVERNANCE_ARE_CONFLATED]

Evidence:
- `docs/standards/agentic-documentation.md:211-247` says provider-specific guidance is a local default, not proof, and forbids universal model behavior, guaranteed loading, tool safety, renderer support, or answer correctness without proof.
- `docs/standards/agents-md.md:38-52` lists Codex load semantics, then states provider-loading facts are current-behavior claims that must refresh through `proof.md`.
- `track-synthesis/00-collective-task-list.md:86-96` holds provider-loading and strict-schema mechanics pending current provider documentation.
- `track-synthesis/00-collective-task-list.md:654-664` holds `.claude` prompt and skill governance as a user choice plus provider skill-format research.

Weakness: the task list treats several different blockers as one provider-shaped hold. Current provider proof answers whether a capability claim can be stated. User governance answers whether `.claude/prompts/**` and `.claude/skills/**` should be normalized as ripple surfaces. Skill-format research answers what a Claude skill file is allowed to contain. These are independent gates.

Correction: split the synthesis into three stop states:
- Provider capability proof: required before stating loading, delimiter, schema, state, output-control, renderer, or skill-format behavior as current provider behavior.
- `.claude` governance choice: required before applying active standards normalization to prompts or skills.
- Local ripple validation: required only after `.claude` is in scope; it covers headings, links, anchors, tables, and fence labels.

Owner route: `agents-md.md` for load semantics, `agentic-documentation.md` for provider posture and artifact separation, `proof.md` for current-source proof, `_reports/AGENTS.md` for report-source boundaries.

Decision: MERGE.

Proof gap: no current provider documentation was read in this pass. This critique does not validate Codex, Claude Code, Gemini, or Claude skill-format behavior.

### [F2][PROMPT_REPORT_FIELDS_SHOULD_NOT_PROMOTE_AS GENERAL TASK OUTPUT]

Evidence:
- `.claude/prompts/fix-standards-docs.md` requires source set, report path, findings, evidence, owner, ripple files, decision, proof gap, and status in its report and synthesis contracts.
- `track-repo-markdown/01-claude-markdown-patterns.md:16-23` recommends promoting only the record-schema principle and stripping wave counts, worker roles, and choreography.
- `docs/standards/_reports/AGENTS.md` already owns report record shape, lane outcome, merge key, correction, promotion, and duplicate rejection.
- `docs/standards/agentic-documentation.md:107-133` owns task-output contracts and separates shape enforcement, source provenance, semantic validation, and runtime safety.

Weakness: the synthesis item at `track-synthesis/00-collective-task-list.md:668-678` routes prompt field packets to `agentic-documentation.md`, `information-structure.md`, and `_reports/AGENTS.md`, but it does not name which fields are report-only. If promoted loosely, report fields such as `Sub-agent source`, `Decision`, `Status`, and fixed wave-derived structure can leak into durable task-output contracts.

Correction: route the durable rule this way:
- `_reports/AGENTS.md` owns report-only fields: `Question`, `Type`, `Lane`, `Merge key`, `Target owner`, `Source basis`, `Promotion target`, `Outcome`, prior-pass extension, correction records, and lane outcomes.
- `information-structure.md` owns the generic carrier shape for source-scan records.
- `agentic-documentation.md` owns only the principle that factual task outputs require source extraction, missing-evidence behavior, and a consumer-checkable contract.

Decision: CORRECT.

Proof gap: none for routing. Exact active wording still needs a wording packet or task-list update before implementation.

### [F3][SKILL_FILES_ARE SOURCE MATERIAL UNTIL TWO PROOFS EXIST]

Evidence:
- `track-repo-markdown/01-claude-markdown-patterns.md:131-140` holds skill file normalization because provider-specific Claude skill file requirements were not checked.
- `track-synthesis/00-collective-task-list.md:342-352` holds adding `.claude/skills/**` to invocation-marker surfaces pending user choice.
- `track-synthesis/00-collective-task-list.md:654-664` holds whether prompts and skills are standards-governed ripple surfaces.
- `docs/standards/agents-md.md:184-193` classifies provider-loading behavior, package state, generated catalogs, provider knobs, and exact fragile facts as unsafe without proof or route.

Weakness: the current tasks correctly hold skill normalization, but they should require two proofs, not one. User choice can put `.claude/skills/**` in scope for repo normalization, but it cannot prove Claude skill frontmatter, load sequence, or provider semantics. Provider proof can prove skill mechanics, but it cannot decide whether this repo wants to normalize the `.claude` assets during this standards pass.

Correction: require both gates before touching `.claude/skills/**` semantics:
- Governance gate: user selects `.claude/skills/**` as standards-governed ripple material for this implementation.
- Provider-format gate: maintained Claude Code documentation or local provider-surface output proves any skill-file mechanics being stated as current behavior.

Allowed without provider-format proof: source-material observations about existing local headings, tables, fences, and invocation markers, as long as they are written as local corpus findings and not provider requirements.

Decision: MERGE.

Proof gap: current Claude skill documentation or local provider output is absent from the bounded source set.

### [F4][RETRIEVAL_PROVENANCE_NEEDS A STRONGER SEMANTIC-PASS BOUNDARY]

Evidence:
- `docs/standards/agentic-documentation.md:120-131` separates schema or typed-input shape, source provenance, semantic validation, and runtime safety.
- `docs/standards/agentic-documentation.md:153-166` says retrieval chunks carry enough context to inspect and refresh source, and provider-specific retrieval claims require configured-provider proof.
- `docs/standards/proof.md:171-204` requires evaluation receipts for machine-facing surfaces and rigor fields for stochastic output, ranking, tool selection, latency, or provider behavior.
- `track-external-research/07-proof-uncertainty-stale-sources.md:61-65` recommends the sentence: a source trace is not a semantic pass.
- `track-synthesis/00-collective-task-list.md:446-452` already proposes adding that boundary and warns not to imply retrieval quality without evaluation receipts.

Weakness: the synthesis correctly captures the source-trace boundary, but it should explicitly forbid three common leak paths: treating chunk provenance as answer correctness, treating retrieval source maps as ranking quality, and treating a generated mirror's source path as proof that the mirror is current.

Correction: strengthen the task-list item to require this exact split:
- Provenance proves origin and refresh route.
- Shape enforcement proves the retrieval or generated-surface container.
- Semantic validation proves the claim matches the source.
- Evaluation receipts prove retrieval quality, ranking, tool choice, latency, or provider behavior.
- Runtime safety proves authorization, destructive-action boundaries, and downstream suitability.

Decision: MERGE.

Proof gap: no retrieval evaluation receipts or local retrieval index runs were inspected in this pass.

### [F5][PARSER_OWNED_MARKDOWN_MUST BLOCK NORMALIZATION BEFORE FORMATTING_RULES]

Evidence:
- `track-external-research/10-machine-consumed-markdown-ledgers.md:15-41` proves parser-owned Markdown needs a no-normalize rule and separate validation for consumer shape.
- `track-synthesis/00-collective-task-list.md:242-252` promotes moving machine-consumed Markdown earlier and naming parser-owned exceptions.
- `docs/standards/agentic-documentation.md:186-199` separates generated mirrors and access boundaries.
- `docs/standards/proof.md:120-131` says machine-consumed fields may use their own casing only when the consumer names the shape.

Weakness: this item is correctly present, but the active implementation order should put the no-normalize exception before any code-fence, heading, table, glyph, or field-packet normalization tasks. Otherwise an implementation pass can "fix" Roslyn release ledgers or generated parser surfaces into standards-compliant prose while breaking the consumer.

Correction: update synthesis ordering so machine-consumed Markdown and parser-owned ledger rules land before ordinary code-fence grammar, heading idiom, table rubric, absence marker, or proof-field normalization. The correction should also state that local release-discipline tests prove row parse and descriptor consistency, not official Roslyn `AdditionalFiles` integration.

Decision: CORRECT.

Proof gap: official Roslyn release-tracking analyzer activation remains unproved from the bounded local read.

### [F6][STRICT_SCHEMA_RULE_CAN BE PARTLY UNBLOCKED WITHOUT PROVIDER_MECHANICS]

Evidence:
- `docs/standards/agentic-documentation.md:120-133` contains both provider-sensitive mechanics and provider-independent boundaries.
- `track-root-corpus/01-root-shared-corpus.md` [F5] holds strict-schema mechanics because provider documentation was not collected.
- `track-synthesis/00-collective-task-list.md:86-96` holds provider-loading and strict-schema mechanics together.

Weakness: holding every strict-schema sentence until provider research is too broad. The provider-sensitive pieces are `additionalProperties: false`, every property required, nullable mechanics, and schema enforcement availability by surface. The provider-independent pieces are already valid local standards: bind output to the narrowest consumer-validated contract; prompt-only JSON is weaker than enforced shape; schema proves shape, not truth; semantic validation and runtime safety remain separate.

Correction: split the task into:
- Immediate MERGE: restructure the paragraph into surfaces: strict schema, typed tool input, generated model, prompt-only JSON, human-reviewed field list. Keep shape/provenance/semantic/safety split.
- HOLD: provider-specific strict JSON-schema details until maintained provider docs or local tool-schema output proves supported mechanics.

Decision: CORRECT.

Proof gap: current provider schema documentation was not collected.

## [RECOMMENDATIONS]

[UPDATE_SYNTHESIS]:
- Split provider proof, `.claude` governance choice, and skill-format proof into separate blockers.
- Add a report-field routing note so prompt-derived report packets do not become general task-output schemas.
- Move machine-consumed Markdown before ordinary normalization tasks in implementation order.
- Split strict-schema work into provider-independent contract separation and provider-specific mechanics.
- Strengthen retrieval provenance wording so source trace, semantic validation, evaluation receipt, and runtime safety cannot substitute for one another.

[KEEP]:
- Keep `.claude/**` as source material until user governance is resolved.
- Keep provider-specific claims as local authoring defaults or proof gaps unless current provider documentation or local provider output proves them.
- Keep wave counts, worker roles, and prompt choreography out of active standards.

[DROP]:
- Do not promote a global skill anatomy from sampled `.claude/skills/**` files.
- Do not promote `transcript`, `schema`, or provider-specific XML delimiter behavior as global standard vocabulary from prompt-source observations alone.
- Do not use `Proof gap: none` filler in produced docs; omit the field when proved.

## [PROOF_GAPS]

- No current provider documentation was researched in this pass.
- No local provider-surface output for Codex, Claude Code, Gemini, or Claude skill loading was inspected.
- No retrieval evaluation, ranking benchmark, or generated mirror freshness check was run.
- No `.claude/**` link, anchor, table, renderer, or fence validation was run.
- This report changes source material only; it does not edit active standards or the collective task list.
