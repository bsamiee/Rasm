# [AGENTIC_DOCUMENTATION]

This standard carries position and agent cognition: where controlling rules, constraints, proof routes, source provenance, and access boundaries sit inside a unit. It also governs how machine-facing surfaces publish repository truth so agents can inspect and refresh it. It does not decide container form, sentence craft, or evidence strength. Documentation can reduce ambiguity. It cannot make model output trustworthy, authorize access, or sanitize untrusted content.

## [1][USE_WHEN]

Apply this standard when a document or surface places controlling content for an agent reader, or when you publish a machine-facing surface:
- ordering a unit so the controlling rule leads and the binding constraint closes
- writing or revising local instruction files such as `AGENTS.md`
- publishing `llms.txt`, generated mirrors, or machine-readable indexes
- defining retrieval stores, chunk provenance, or access boundaries
- cataloging MCP resources, prompts, and tools
- specifying structured-output contracts
- separating durable documentation from task prompts and state artifacts

Route the surface before applying detailed rules:

| [INDEX] | [SURFACE] | [SECTION] |
| :-----: | :-------- | :-------- |
|   [1]   | unit ordering and salience | `[2][SERIAL_POSITION_SALIENCE]` |
|   [2]   | durable docs, prompt assets, task instructions, state artifacts | `[6][ARTIFACT_SEPARATION]` |
|   [3]   | task prompts and structured outputs | `[7][TASK_OUTPUT_CONTRACTS]` |
|   [4]   | `AGENTS.md` positioning behavior | `[8][AGENTS_MD_AUTHORING]` |
|   [5]   | indexes, retrieval, MCP catalogs, generated mirrors | `[9]` through `[12]` |
|   [6]   | provider authoring defaults | `[13][PROVIDER_BEHAVIOR]` |

Container choice belongs to [information-structure.md](information-structure.md). Sentence mechanics belong to [style-guide.md](style-guide.md). Evidence strength belongs to [proof.md](proof.md). Document-type routing belongs to [README.md](README.md).

## [2][SERIAL_POSITION_SALIENCE]

Place the controlling rule first and the binding constraint last in every unit. Attention concentrates at sequence edges, and middle-position degradation can still appear in long-context work. Treat the advertised context window as capacity, not as a reason to bury constraints.

Apply the pattern recursively at every scale. Put controlling rules, constraints, proof routes, source provenance, and access boundaries at the unit edges.

| [INDEX] | [UNIT] | [LEAD] | [CLOSE] |
| :-----: | :----- | :----- | :------ |
|   [1]   | Document | scope and highest-risk constraint | boundary, proof route, or next route |
|   [2]   | Section | what the section controls | boundary or route |
|   [3]   | Paragraph and sentence | prose shape from [style-guide.md](style-guide.md) | consequence, proof route, or route-away |

Container choice belongs to [information-structure.md](information-structure.md). Sentence craft belongs to [style-guide.md](style-guide.md).

Salience is relative, not a hard cutoff. The middle down-weights content; it does not erase it. If a constraint is load-bearing and the unit is long enough to need scanning, restate it in compact form at the close, near where an agent acts on it. Keep inventories, history, and non-actionable detail out of the lead. Never bury a failure-impacting constraint, exception, or route-away rule mid-unit.

Use bracketed headings, table rubrics, `[INDEX]` rows, and compact status markers as salience aids, not as decoration. They help an agent find boundaries, compare rows, and filter state; they do not replace the front-and-close rule, claim-level evidence, or precise prose.

This corpus uses the position ring as its normative placement rule. Provider-behavior claims are proved only through the evidence labels and freshness rules defined by [proof.md](proof.md).

## [3][CONTEXT_INVARIANCE]

The position ring holds across every unit, whatever its lifetime or reader: controlling content at the edges, supporting detail in the middle. Use the three shapes below according to artifact lifetime.

| [INDEX] | [ARTIFACT]        | [LEAD]                                             | [MIDDLE]              | [CLOSE] |
| :-----: | :---------------- | :------------------------------------------------- | :-------------------- | :------ |
|   [1]   | Durable document  | scope and highest-risk constraint                  | reusable support      | boundaries and route |
|   [2]   | Task prompt       | objective, durable constraints, output expectation | bulky source material | immediate ask and binding constraints |
|   [3]   | Retrieval chunk   | identity and standalone constraint                 | source detail         | omitted unless the chunk needs a refresh route |

When long documents lead, anchor the final ask with a bridge phrase such as `Based on the information above`, then restate the critical constraints in compact form. An `AGENTS.md` overlay is a durable-document shape, not a task prompt.

Do not rewrite an ordinary durable document into task-prompt shape, and do not bury a durable rule inside transient task framing.

## [4][CONSTRAINT_STACKING]

Rank instructions so an agent resolves conflicts deterministically. State the rank, not just the rule.

| [INDEX] | [RANK]      | [MEANING] |
| :-----: | :---------- | :-------- |
|   [1]   | Invariant   | rule that must not break unless a higher-ranked rule names an exception |
|   [2]   | Preference  | default that holds unless a stated condition overrides it |
|   [3]   | Default     | starting choice an agent may replace when evidence is stronger |

Remove contradictory instructions rather than layering a caveat on top of a conflicting rule; unresolved contradiction degrades instruction-following more than any single weak rule. If two rules can both apply, name which controls.

## [5][FRAMING_INSTRUCTION_FOLLOWING]

Write instructions as positive imperatives that name the action to take. Agent readers follow an explicit target more reliably than a prohibition.

| [RESULT] | [INSTRUCTION] |
| :------- | :------------ |
| Accepted | State the constraint first. |
| Rejected | Do not leave the constraint implicit. |

Reason: The accepted sentence names the action; the rejected sentence only names the failure.

Reserve negative form for genuine hard boundaries and pair each prohibition with the positive action that replaces it. Keep guidance clear, consistent, and in a high-salience position; vague or contradicted instructions degrade compliance.

If factual grounding matters, require evidence extraction before synthesis. First, have the agent cite source spans or quote brief excerpts within access and copyright limits. Then reason from them. When quoting is unsafe or unavailable, require source IDs, line references, headings, or field paths.

## [6][ARTIFACT_SEPARATION]

Keep durable documentation, task instructions, prompt assets, and state artifacts in distinct artifacts. Each has a different lifetime and reader contract.

| [INDEX] | [ARTIFACT]              | [HOLDS] | [DOES_NOT_HOLD] |
| :-----: | :---------------------- | :------ | :-------------- |
|   [1]   | Durable documentation   | stable policy, conventions, reusable examples, canonical links, and tool contracts | single-interaction state |
|   [2]   | Prompt assets           | reusable task shapes, input slots, constraints, output contracts, and stop rules | policy that outranks `CLAUDE.md`, `AGENTS.md`, or this standards library |
|   [3]   | Task instructions       | one objective, done condition, current evidence, hard constraints, output contract, and stop rule | durable standards |
|   [4]   | State artifacts         | validated facts, current status, next safe action, open questions, provenance, and proof gaps | reusable policy |

Promote only durable policy from prompt assets into the controlling instruction or standards file.

Do not publish task-specific interaction material as durable documentation. Promote only the stable rule into its source standard and leave transient context in the artifact that carries the interaction.

Prompt-era task ledgers, stage numbers, agent dictums, validation snippets, and chat-derived design dossiers are source material only. Promote durable facts into the source document type, and delete or archive the prompt frame instead of publishing it as a section pattern.

## [7][TASK_OUTPUT_CONTRACTS]

Order a task instruction to the position ring when source material spans multiple documents or sits before the final ask. Bind it to a contract the consumer can check. Invariant constraints and exclusions stay near an edge.

Order a task instruction as:
1. Objective.
2. Done condition.
3. Hard constraints and exclusions.
4. Current evidence or source material.
5. Evidence extraction rule when factual grounding matters.
6. Output contract and missing-evidence behavior.
7. Validation and stop rule.

Bind structured output to the narrowest contract the consumer actually validates: a schema, a typed tool input, a generated model, a catalog entry, or a documented field list. Use closed, total JSON object schemas only on strict JSON-schema or tool-schema surfaces that support those mechanics. Use `additionalProperties: false`, every property required, and explicit nullable values only where the provider or validator supports that shape.

Prefer provider schema enforcement over a schema described in prose where the surface supports it. Treat prompt-only JSON as a weaker fallback that requires validation and repair handling. A schema proves shape, not truth; keep semantic validation, refusal handling, and downstream suitability checks visible in the proof path. If a human reviews the contract instead of tooling enforcing it, state that proof gap.

Keep four checks separate in every machine-facing contract:

| [INDEX] | [CHECK] | [PROVES] | [DOES_NOT_PROVE] |
| :-----: | :------ | :------- | :--------------- |
|   [1]   | Shape enforcement | schema, typed tool input, generated model, parser, or field list validates the container | truth, safety, freshness |
|   [2]   | Source provenance | source path, heading, contract, command, or field path proves where a fact came from | semantic correctness |
|   [3]   | Semantic validation | source trace, evaluator, application check, or human review proves the shaped output is correct for the claim | authorization or downstream safety |
|   [4]   | Runtime safety | permissions, destructive-action guard, injection boundary, authorization, and downstream suitability prove usable boundaries | factual correctness |

Do not let a single check imply the others. A valid JSON object can still be unsupported, unsafe, stale, or semantically wrong.

## [8][AGENTS_MD_AUTHORING]

Write `AGENTS.md` as a durable, behavioral overlay for one directory. [agents-md.md](agents-md.md) owns structure and anti-fragility; this file owns salience and provider behavior for instruction surfaces.

This standard still controls salience and provider behavior: keep the highest-risk invariant at the lead, close with the binding proof or route, layer files by scope, and prove provider-loading claims through [proof.md](proof.md). `AGENTS.md` files complement README files; they do not duplicate them, summarize them, carry process notes, or publish session state.

Iterate instruction files from observed agent failures and durable local behavior. Speculative rules, fixed subagent counts, provider manuals, and copied command catalogs belong outside the overlay unless they are local invariants with a maintained proof route.

## [9][LLMS_TXT_INDEXES]

Keep machine-readable indexes as link-only maps to canonical targets selected by [README.md](README.md) routing. Do not copy canonical document bodies into the index.

- `llms.txt`: publish as a routing map to canonical documents for a published corpus, never as an enforcement or ranking mechanism.
- `llms-full.txt`: expanded context only when generated from canonical documents, reviewed, and marked as generated.
- Markdown or MDX mirrors: allowed when they preserve canonical meaning and name their source page.

State only published repository surfaces and their refresh route. Do not claim crawler adoption, ranking, answer correctness, access control, or injection resistance from documentation alone.

## [10][RETRIEVAL_PROVENANCE]

Document a retrieval surface with the provenance an agent needs to inspect and refresh a chunk.

[RETRIEVAL_FIELDS]:
- Canonical corpus: source documents or corpus root.
- Source path: path or URL plus heading path or parent identity.
- Document route: document type and route when known.
- Generated status: generated marker plus refresh route or generation path.
- Access class: access class and filter rule when content is not public.
- Drift condition: event that makes drift-prone content stale.

Carry provenance fields only in the schema or header shape the deployed retrieval store consumes. When the provenance includes proof, source, freshness, or generation facts, use the label meanings defined by [proof.md](proof.md) instead of defining a parallel vocabulary here.

Each retrievable chunk should carry enough context to stand alone because retrieval strips surrounding structure. Claim hybrid retrieval, reranking, or provider-specific search only where the deployed stack supports it and the proof names the configured provider, command, or controlling source. Split chunk tables and records that exceed the size limits in [information-structure.md](information-structure.md).

## [11][MCP_TOOL_CATALOGS]

Catalog MCP surfaces by control type before detailing any schema:

[RESOURCES]:
- Purpose: passive data or content the client reads as context.
- Inspect when: context is needed before action.
- Authorization/setup: required local or remote access.
- Canonical reference: owning source.

[PROMPTS]:
- Purpose: reusable interaction templates.
- Inspect when: the template changes task shape or output contract.
- Authorization/setup: required local or remote access.
- Canonical reference: owning source.

[TOOLS]:
- Purpose: executable operations with typed inputs and outputs.
- Invoke when: precondition is satisfied.
- Must not invoke when: authorization, destructive-action, or suitability boundary is not satisfied.
- Canonical reference: owning source.

Detailed schemas belong in API or reference documentation. A documented tool is not safe to call merely because it is documented; safety comes from permissions, review, and runtime controls, never from the catalog entry.

Keep `Resources`, `Prompts`, and `Tools` as separate record families even when one MCP server publishes all three. Each family declares its own field casing, invocation or inspection rule, proof route, and omission behavior. Do not merge a passive resource into a tool table merely because both are listed by one provider.

## [12][GENERATED_MIRRORS]

A generated or mirrored file uses a field packet:

[GENERATED_MIRROR]:
- Canonical source: source page or corpus.
- Generated from: generator, workflow, or `Proof gap:` when no generator is maintained.
- Hierarchy: heading preservation rule or exception.
- Omissions: omitted sections or fields.
- Exclusions: secrets, personal data, task notes, and private machine details.
- Edit rule: edit the source, not the generated mirror.

Access-class separation belongs to retrieval and generated-surface boundaries. Documentation can describe an access class; it cannot enforce one.

## [13][PROVIDER_BEHAVIOR]

Provider-specific guidance is a local authoring default, not proof of current provider behavior. Keep the portable contract stable: outcome, constraints, evidence boundary, output shape, and stop rule. Adapt delimiter, long-context placement, and schema mechanics only to the provider surface that will run the prompt. Use one delimiter family in a prompt, either XML-style tags or Markdown sectioning, unless an external template requires otherwise.

### [13.1][OPENAI_CODEX]

Preferred structure: outcome, success criteria, constraints, available evidence, and final-answer shape before process detail.
Delimiter: Markdown sectioning is the default for durable instruction files; use another delimiter only when the target surface or template requires it.
Long context: keep critical constraints at the opening and closing edge; use compact restatement near the final ask when bulky source material sits in the middle.
Structured output: put machine-consumed contracts in structured outputs or strict tool schemas where the API supports them; prose may steer JSON shape, but it does not enforce it.
State: keep `AGENTS.md` lean and scoped; loading behavior requires provider or local proof before it is stated as current capability.
Caching: use prompt caching only for stable prefixes and treat it as latency and cost optimization, not state or correctness proof.
Output control: control answer length with explicit word, section, or verbosity contracts; do not treat verbosity as hidden reasoning control.

### [13.2][CLAUDE_CODE]

Preferred structure: separate instructions, context, examples, documents, and variable inputs when the prompt mixes those concerns.
Delimiter: use XML-style tags for complex prompts, with descriptive names and nesting only where hierarchy is real.
Long context: place source documents near the top and the query or task at the end; identify each source and content block explicitly.
Structured output: state the exact output fields, formats, and missing-data behavior; use schema enforcement where the surface supplies it.
File format: `CLAUDE.md`, skills, commands, and task prompts use Markdown structure.
XML tags: XML tags are a prompt-clarity tool inside complex prompt content, not a Claude Code file-format requirement.
`AGENTS.md` loading: `AGENTS.md` affects Claude Code only when `CLAUDE.md` imports or links it.
Output control: ask for evidence, quoted or cited source spans within access and copyright limits, and final answer shape before synthesis when factual grounding matters.

### [13.3][GEMINI]

Preferred structure: direct task instruction with explicit constraints, accepted values, output format, missing-data behavior, and validation rule.
Delimiter: use one delimiter family, Markdown or XML-style tags, and keep the same family across instructions, context, and examples.
Long context: place source material before the specific question or instruction and use a bridge phrase such as `Based on the information above` to bind the final ask to the supplied material.
Structured output: prefer schema-backed structured output for extraction, classification, JSON payloads, and agent-to-agent exchange, but keep semantic validation in application or review logic.
Verification split: when source availability or capability is uncertain, verify the relevant information exists first, then answer only inside the verified scope.
Output control: state the desired length, sections, and refusal or missing-data behavior explicitly.

Do not require exposed chain-of-thought in published prompt standards. Ask for evidence, checks, summaries, or a decision trail the consumer can inspect, and rely on provider reasoning controls where the runtime exposes them. Use examples only when they materially teach format, tone, edge handling, or tool choice; remove examples that merely repeat the rule.

Provider-specific rules are local authoring defaults, not proof of current provider behavior. Treat a capability, loading, delimiter, schema, state, or output-control claim as provisional until current primary documentation or local tool output proves it, then record freshness through [proof.md](proof.md).

Provider-specific guidance may define preferred defaults only within a proven surface. It must not claim universal model behavior, guaranteed loading, tool safety, renderer support, or answer correctness without the proof route. When current provider proof is absent, state the default as a local authoring convention and leave capability proof to [proof.md](proof.md).

Source-map and reference-catalog retrieval rules live in `[10][RETRIEVAL_PROVENANCE]`.

## [14][BOUNDARIES]

- [information-structure.md](information-structure.md) carries container form, diagrams, page anatomy, and chunk shape; this standard carries where controlling rules, constraints, proof routes, and source provenance sit within them.
- [style-guide.md](style-guide.md) carries sentence and word craft; this standard carries the cognition rationale for positive, imperative framing.
- [proof.md](proof.md) carries evidence strength, proof details, and the evaluation discipline for machine-facing surfaces.
- [formatting.md](formatting.md) carries the markers and styling that render the constraints this standard places.
- [agents-md.md](agents-md.md) carries the produced structure and anti-fragility rules for `AGENTS.md` files.
- [README.md](README.md) carries document-type routing and is the single index that links across standards.

## [15][VALIDATION]

Use this verification checklist by group:

[POSITION_INSTRUCTIONS]:
- [ ] The controlling rule leads each unit and the binding constraint closes it.
- [ ] Load-bearing constraints in long units are restated near the close.
- [ ] Durable docs, prompt assets, task instructions, and state artifacts stay separate.
- [ ] Instructions are positive imperatives with ranked constraints.
- [ ] Contradictory instructions are removed or the controlling rule is named.
- [ ] Factual tasks require evidence extraction before synthesis.

[PROVIDER_PROOF]:
- [ ] Provider claims carry current maintained-source or local-output proof when they can drift.
- [ ] Machine-facing contracts separate shape enforcement, source provenance, semantic validation, and runtime safety.
- [ ] Provider-specific prompt rules remain preferred patterns and do not claim enforcement, correctness, or universal superiority.
- [ ] Prompt-only JSON names parser or validator, repair behavior, and proof gap when tooling does not enforce shape.

[AGENT_SURFACES]:
- [ ] Indexes link to canonical docs; `llms.txt` is treated as a map.
- [ ] Retrieval chunks carry source, heading path, route, access, and freshness.
- [ ] MCP resources, prompts, and tools are separated before schemas.
- [ ] Generated mirrors identify source and generation status.
- [ ] Agent surfaces carry deterministic proof receipts through [proof.md](proof.md) and add rigor fields when stochastic, ranking, tool-selection, latency, or provider behavior is claimed.
- [ ] `AGENTS.md` files preserve salience, provider-loading proof, scope layering, and artifact separation without duplicating README bodies or session state.

[ROOT_AUDIT]:
- [ ] A reviewer can identify the document lead rule, section closes, and final route boundary.
- [ ] Task prompts, state artifacts, and durable standards are not mixed inside the scored documentation surface.
- [ ] No secrets, nonpublic local paths, or unverified provider claims are exposed.
