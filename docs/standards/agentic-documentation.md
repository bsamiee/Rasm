# [AGENTIC_DOCUMENTATION]

This standard owns position and agent cognition: where controlling content sits inside a unit, how serial-position salience governs that placement, and how machine-facing surfaces publish repository truth so agents find, trust, and refresh it. It does not decide container form, sentence craft, or evidence strength. Documentation reduces ambiguity; it does not make model output trustworthy, authorize access, or sanitize untrusted content.

## [1][USE_WHEN]

Apply this standard when a document or surface places high-value content for an agent reader, or when you publish a machine-facing surface:

- ordering a unit so the controlling rule leads and the binding constraint closes;
- writing or revising local instruction files such as `AGENTS.md`;
- publishing `llms.txt`, generated mirrors, or machine-readable indexes;
- defining retrieval stores, chunk provenance, or access boundaries;
- cataloging MCP resources, prompts, and tools, or specifying structured-output contracts;
- separating durable documentation from task prompts and state artifacts.

Container choice, sentence mechanics, and evidence strength belong to the form, craft, and evidence standards. Document-type routing belongs to the index.

## [2][SERIAL_POSITION_SALIENCE]

Place the controlling rule first and the binding constraint last in every unit. Attention concentrates at sequence edges, and middle-position degradation can still appear in long-context work. Treat the advertised context window as capacity, not as a reason to bury constraints.

Apply the pattern recursively at every scale. Put high-value content at the unit edges; container choice belongs to `information-structure.md`, and sentence craft belongs to `style-guide.md`.

- Document: lead with scope and the highest-risk constraint; close with boundaries and the proof or route that makes the document safe to reuse.
- Section: open with what the section controls; end on its boundary or owner.
- Paragraph and sentence: [style-guide.md](style-guide.md) owns the prose shape that realizes the ring.

Salience is relative, not a hard cutoff: the middle down-weights content, it does not erase it. If a constraint is load-bearing and the unit is long, restate it in compact form at the close, near where an agent acts on it. Keep low-value inventories, history, and incidental detail out of the lead. Never bury a high-risk constraint, exception, or route-away rule mid-unit.

Use bracketed headings, table rubrics, `[INDEX]` rows, and compact status markers as salience aids, not as decoration. They help an agent find boundaries, compare rows, and filter state; they do not replace the front-and-close rule, claim-level evidence, or precise prose.

`Source of truth:` this corpus's local position-ring rule, plus the linked provider prompt-engineering documentation in [Provider behavior](#13providerbehavior). `Last verified:` 2026-06-04.

## [3][CONTEXT_INVARIANCE]

The position ring holds across every unit, whatever its lifetime or reader: controlling content at the edges, supporting detail in the middle. Three instantiations carry it.

- Durable document: a standard or a reference page leads with scope and the highest-risk constraint and closes on boundaries and route; an `AGENTS.md` overlay is a position-ring instance of this same durable-document shape.
- Task prompt: put objective, durable constraints, and output expectations at an edge, place bulky source material in the middle, and close with the immediate ask plus the binding constraints the model must obey while answering. When long documents lead, anchor the final ask with a bridge phrase such as `Based on the information above`, then restate the critical constraints in compact form.
- Retrieval chunk: lead each chunk with the identity and constraint that let it stand alone once retrieval strips surrounding structure.

Do not rewrite an ordinary durable document into task-prompt shape, and do not bury a durable rule inside transient task framing.

## [4][CONSTRAINT_STACKING]

Rank instructions so an agent resolves conflicts deterministically. State the rank, not just the rule:

- Invariants: rules that must never break. State them first and absolutely.
- Preferences: defaults that hold unless a stated condition overrides them.
- Defaults: starting choices an agent may replace when evidence is stronger.

Remove contradictory instructions rather than layering a caveat on top of a conflicting rule; unresolved contradiction degrades instruction-following more than any single weak rule. If two rules can both apply, name which controls.

## [5][FRAMING_INSTRUCTION_FOLLOWING]

Write instructions as positive imperatives that name the action to take, because agent readers follow an explicit target more reliably than a prohibition. Convert "do not leave the constraint implicit" into "state the constraint first." Reserve negative form for genuine hard boundaries, and pair each prohibition with the positive action that replaces it. Keep guidance clear, consistent, and in a high-salience position; vague or contradicted instructions degrade compliance.

If factual grounding matters, require evidence extraction before synthesis: have the agent cite source spans or quote brief excerpts within access and copyright limits first, then reason from them. When quoting is unsafe or unavailable, require source IDs, line references, headings, or field paths. This evidence-before-synthesis order reduces unsupported claims and keeps the proof trail inspectable.

## [6][ARTIFACT_SEPARATION]

Keep durable documentation, task instructions, prompt assets, and state artifacts in distinct artifacts; each has a different lifetime and reader contract.

- Durable documentation holds stable policy, conventions, reusable examples, canonical links, and tool contracts. It outlives any single interaction.
- Prompt assets hold reusable task shapes, input slots, constraints, output contracts, and stop rules. They do not outrank `CLAUDE.md`, `AGENTS.md`, or this standards library; promote only durable policy from them into the owning instruction or standards file.
- Task instructions hold one objective, the done condition, the current evidence to inspect, hard constraints, the output contract, and the stop rule for one interaction, in the canonical field order the Task and output contracts section owns.
- State artifacts hold validated facts, current status, the next safe action, open questions, provenance, and known proof gaps for resumable work.

Do not publish task-specific interaction material as durable documentation. Promote only the stable rule into its owning standard, and leave transient context in the artifact that owns the interaction.

## [7][TASK_OUTPUT_CONTRACTS]

Order a context-heavy task instruction to the position ring, and bind it to a contract the consumer can check. Invariant constraints and exclusions stay near an edge even when bulky evidence must sit before the final ask.

Order a task instruction as:

1. Objective.
2. Done condition.
3. Hard constraints and exclusions.
4. Current evidence or source material.
5. Evidence extraction rule when factual grounding matters.
6. Output contract and missing-evidence behavior.
7. Validation and stop rule.

Bind machine-consumed output to the narrowest contract the consumer actually validates: a schema, a typed tool input, a generated model, a catalog entry, or a documented field list. Close and total JSON-object schemas so the consumer validates a fully specified shape rather than a narrow-but-open one. Use `additionalProperties: false` with every property required, and represent optionality with an explicit nullable value where the provider supports it.

Prefer provider schema enforcement over a schema described in prose where the surface supports it. Treat prompt-only JSON as a weaker fallback that requires validation and repair handling. A schema proves shape, not truth; keep semantic validation, refusal handling, and downstream suitability checks visible in the proof path. If a human reviews the contract instead of tooling enforcing it, state that proof gap.

## [8][AGENTS_MD_AUTHORING]

Write `AGENTS.md` as a durable, behavioral overlay for one directory: what changes here, which standards and commands apply, and which patterns are forbidden. It complements the human-facing README; it does not duplicate it. Keep root guidance short and place specialized rules near the subtree they govern; provider-specific loading behavior belongs in the provider-behavior section.

Start from the section set below. It is a suggested baseline, not a closed vocabulary: a directory may extend it with additional bracketed sections or relabel a section where a clearer name fits its needs. What binds is the include/exclude rule, not the exact section names. Whatever sections you keep, include only stable, universal, behavioral content an agent acts on:

- `Scope`: directory scope and purpose, and the read order an agent follows into the deeper rules;
- `Routing`: routing to the standards and reference files that own deeper rules, and ownership of conflicting guidance;
- `Execution rules`: build, test, and quality commands, code-style and review expectations, and commit and pull-request expectations;
- `Exclusions`: forbidden patterns, security constraints, and known gotchas;
- `Validation`: the gate that proves a change here, plus `Source of truth:` and `Last verified:` for any provider-behavior claim.

Use this heading template as the starting shape. Extend or relabel sections only when the directory needs a sharper local contract:

```markdown template
# [DIRECTORY_AGENTS]

<Lead: what changes in this directory and the one rule an agent must not break.>

## [1][SCOPE]

<What this directory owns; the read order into the standards and reference files below.>

## [2][ROUTING]

<Which standards and reference files own the deeper rules; which file wins on conflict.>

## [3][EXECUTION_RULES]

<Build, test, and quality commands; code-style, review, and commit or pull-request expectations.>

## [4][EXCLUSIONS]

<Forbidden patterns, security constraints, and known gotchas for this directory.>

## [5][VALIDATION]

<The gate that proves a change here; `Source of truth:` and `Last verified:` for any provider claim.>
```

Exclude content that rots or wastes context:

- full README or marketing copy;
- exhaustive path or file enumerations that change with the tree;
- large auto-generated reference dumps;
- thread-scoped goals, plans, or completion state;
- logs, transient state, or per-session task notes.

Layer files by scope and keep each lean. Lower directories should carry only guidance specific to that level, and higher-level files should contribute only broad defaults. Place a file only where a directory carries guidance needed across sessions. Every word loaded costs reasoning budget, so a top-level overlay should stay near one screen.

Iterate the file from observed agent failures rather than speculation. Record provider-behavior claims with `Source of truth:` and `Last verified:` beside the claim.

## [9][LLMS_TXT_INDEXES]

Keep machine-readable indexes shallow and curated. Link to the canonical README, architecture, API, reference, how-to, runbook, support, and standards pages; do not copy their bodies into the index.

- `llms.txt`: a curated map to canonical documents for a published corpus, treated as a routing map and never as an enforcement or ranking mechanism.
- `llms-full.txt`: expanded context only when generated from canonical documents, reviewed, and marked as generated.
- Markdown or MDX mirrors: allowed when they preserve canonical meaning and name their source page.

Claim only what the repository publishes and how it refreshes. Do not claim crawler adoption, ranking, answer correctness, access control, or injection resistance from documentation alone.

## [10][RETRIEVAL_PROVENANCE]

Document a retrieval surface with the provenance an agent needs to trust and refresh a chunk. State, per corpus or chunk:

- the canonical corpus or source documents;
- the source path or URL and the heading path or parent identity;
- the document type and owner when known;
- generated status and the refresh owner or generation path;
- the access class and filter rule when content is not public;
- the review trigger for drift-prone content.

Carry these as a per-chunk provenance record, one `label: value` per line, so every retrievable chunk header is a template rather than reconstructed from prose:

```markdown template
source_path: <repo path or URL of the source document>
heading_path: <H1 > H2 > H3 trail, or the parent identity>
doc_type: <reference | runbook | architecture | ...>
owner: <accountable role or group, when known>
generated: <true | false; generation path or workflow when true>
access: <public | internal | restricted | secret; filter rule when not public>
review_trigger: <event that makes the chunk stale>
```

Each retrievable chunk should carry enough context to stand alone, because retrieval strips surrounding structure. Claim hybrid retrieval, reranking, or provider-specific search only where the deployed stack supports it and the proof names the configured provider, command, or source of truth. Keep very large tables and long records chunked rather than passed whole; long-context degradation erodes oversized structured content too.

## [11][MCP_TOOL_CATALOGS]

Catalog MCP surfaces by control type before detailing any schema:

- Resources: passive data or content the client reads as context.
- Prompts: reusable interaction templates.
- Tools: executable operations with typed inputs and outputs.

For each capability, name what it is, when to inspect it, the required authorization or local setup, and where its canonical reference lives. For a tool, also name the precondition that gates invocation and the case where it must not be invoked.

Detailed schemas belong in API or reference documentation. A documented tool is not safe to call merely because it is documented; safety comes from permissions, review, and runtime controls, never from the catalog entry.

## [12][METADATA_GENERATED_MIRRORS]

Add YAML frontmatter, sidecar metadata, or generated index fields only when a named renderer, indexer, generator, retrieval store, or review workflow consumes that exact shape. Use an in-body definition block for ordinary document metadata. Do not add YAML frontmatter as a speculative summary, fake routing layer, or authoring habit.

Use snake-case field names when a machine reads the field. Durable fields may include `description`, `owner`, `source`, `source_of_truth`, `evidence`, `generated`, `generated_from`, `last_reviewed`, `last_verified`, `review_trigger`, `access`, and `parent`. Map each machine field to its human-facing proof label so the same fact reads the same way in prose and metadata; proof.md owns the proof labels.

| [INDEX] | [METADATA_FIELD]  | [PROOF_LABEL]      |
| :-----: | :---------------- | :----------------- |
|   [1]   | `source_of_truth` | `Source of truth:` |
|   [2]   | `evidence`        | `Evidence:`        |
|   [3]   | `last_verified`   | `Last verified:`   |
|   [4]   | `review_trigger`  | `Review trigger:`  |
|   [5]   | `generated_from`  | `Generated from:`  |

Fields with no proof-label counterpart — `description`, `owner`, `source`, `generated`, `last_reviewed`, `access`, `parent` — are triage or routing metadata and do not assert a claim, so they carry no prose label.

A generated or mirrored file must link its canonical source, name the generator or workflow where one is maintained, preserve heading hierarchy where possible, mark omissions, and exclude secrets, personal data, task notes, and private machine details. Do not hand-edit a generated mirror as independent truth.

Separate public, internal, restricted, and secret material into distinct corpora or enforce equivalent filters at the boundary. Documentation can describe an access class; it cannot enforce one.

## [13][PROVIDER_BEHAVIOR]

Treat provider-specific guidance as preferred patterns within a converging ecosystem, not as iron laws. Keep the stable contract portable: outcome, constraints, evidence boundary, output shape, and stop rule. Then adapt delimiter, long-context placement, and schema mechanics to the provider surface that will run the prompt. Use one delimiter family in a prompt — XML-style tags or Markdown sectioning, not both — unless an external template requires otherwise.

### [13.1][OPENAI_CODEX]

Preferred structure: outcome, success criteria, constraints, available evidence, and final-answer shape before process detail.
Delimiter: Markdown sectioning is the default for durable instruction files; use another delimiter only when the target surface or template requires it.
Long context: keep critical constraints at the opening and closing edge; use compact restatement near the final ask when bulky source material sits in the middle.
Structured output: put machine-consumed contracts in structured outputs or strict tool schemas where the API supports them; prose may steer JSON shape, but it does not enforce it.
State and caching: keep `AGENTS.md` lean and scoped because Codex loads the instruction chain from global guidance through the working directory and closer files override earlier conflicting guidance. Use prompt caching only for stable prefixes and treat it as latency and cost optimization, not state or correctness proof.
Output control: control answer length with explicit word, section, or verbosity contracts; do not treat verbosity as hidden reasoning control.
Source of truth: [OpenAI prompting guide](https://platform.openai.com/docs/guides/prompting), [Structured Outputs guide](https://platform.openai.com/docs/guides/structured-outputs), [Prompt caching guide](https://platform.openai.com/docs/guides/prompt-caching), and [Codex `AGENTS.md` documentation](https://github.com/openai/codex/blob/main/docs/agents_md.md).
Last verified: 2026-06-04

### [13.2][CLAUDE_CODE]

Preferred structure: separate instructions, context, examples, documents, and variable inputs when the prompt mixes those concerns.
Delimiter: use XML-style tags for complex prompts, with descriptive names and nesting only where hierarchy is real.
Long context: place source documents near the top and the query or task at the end; wrap multi-document inputs with source and content metadata.
Structured output: state the exact output fields, formats, and missing-data behavior; use schema enforcement where the surface supplies it.
Claude Code files: `CLAUDE.md`, skills, commands, and task prompts use Markdown structure. XML tags are a prompt-clarity tool inside complex prompt content, not a Claude Code file-format requirement. `AGENTS.md` affects Claude Code only when `CLAUDE.md` imports or links it.
Output control: ask for evidence, quoted or cited source spans within access and copyright limits, and final answer shape before synthesis when factual grounding matters.
Source of truth: [Anthropic prompting best practices](https://docs.anthropic.com/en/docs/build-with-claude/prompt-engineering/claude-prompting-best-practices), [Claude Code memory documentation](https://docs.anthropic.com/en/docs/claude-code/memory), and [extended thinking documentation](https://docs.anthropic.com/en/docs/build-with-claude/extended-thinking).
Last verified: 2026-06-04

### [13.3][GEMINI]

Preferred structure: direct task instruction with explicit constraints, accepted values, output format, missing-data behavior, and validation rule.
Delimiter: use one delimiter family, Markdown or XML-style tags, and keep the same family across instructions, context, and examples.
Long context: place source material before the specific question or instruction and use a bridge phrase such as `Based on the information above` to bind the final ask to the supplied material.
Structured output: prefer schema-backed structured output for extraction, classification, JSON payloads, and agent-to-agent exchange, but keep semantic validation in application or review logic.
Verification split: when source availability or capability is uncertain, verify the relevant information exists first, then answer only inside the verified scope.
Output control: state the desired length, sections, and refusal or missing-data behavior explicitly.
Source of truth: [Gemini prompt design strategies](https://ai.google.dev/gemini-api/docs/prompting-strategies), [Gemini long context guide](https://ai.google.dev/gemini-api/docs/long-context), and [Gemini structured output guide](https://ai.google.dev/gemini-api/docs/structured-output).
Last verified: 2026-06-04

Do not require exposed chain-of-thought in published prompt standards. Ask for evidence, checks, summaries, or a decision trail the consumer can inspect, and rely on provider reasoning controls where the runtime exposes them. Use examples only when they materially teach format, tone, edge handling, or tool choice; remove examples that merely repeat the rule.

`Source of truth:` the linked provider sources named beside each provider rule above. `Last verified:` 2026-06-04. `Review trigger:` provider prompt-engineering, structured-output, memory, or context guidance changes.

## [14][BOUNDARIES]

- [information-structure.md](information-structure.md) owns container form, diagrams, page anatomy, and chunk shape; this standard owns where high-value content sits within them.
- [style-guide.md](style-guide.md) owns sentence and word craft; this standard owns the cognition rationale for positive, imperative framing.
- [proof.md](proof.md) owns evidence strength, freshness fields, and the evaluation discipline for machine-facing surfaces.
- [formatting.md](formatting.md) owns the markers and styling that render the constraints this standard places.
- [README.md](README.md) owns document-type routing and is the single index that links across standards.

## [15][REVIEW_CHECKLIST]

- [ ] The controlling rule leads each unit and the binding constraint closes it.
- [ ] Load-bearing constraints in long units are restated near the close.
- [ ] Durable docs, prompt assets, task instructions, and state artifacts stay separate.
- [ ] Instructions are positive imperatives with ranked constraints.
- [ ] Provider claims carry a current primary source and `Last verified` date.
- [ ] `AGENTS.md` files are behavioral, lean, and free of rot-prone enumeration.
- [ ] Provider-specific prompt rules remain preferred patterns and do not claim enforcement, correctness, or universal superiority.
- [ ] Indexes link to canonical docs; `llms.txt` is treated as a map.
- [ ] Retrieval chunks carry source, heading path, owner, access, and freshness.
- [ ] MCP resources, prompts, and tools are separated before schemas.
- [ ] Metadata exists only where a consumer reads it.
- [ ] Generated mirrors identify source and generation status.
- [ ] No secrets, nonpublic local paths, or unverified provider claims are exposed.
