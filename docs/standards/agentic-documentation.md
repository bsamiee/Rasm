# [AGENTIC_DOCUMENTATION]

This standard optimizes prose, documentation, prompts, and machine-readable documentation surfaces for agentic reading performance. It controls where rules, constraints, proof routes, source provenance, and access boundaries sit so an agent can find, weight, and act on them correctly. It is not a standard for building agentic tools, implementing agent runtimes, designing MCP servers, or claiming provider capability. Documentation can reduce ambiguity. It cannot make model output trustworthy, authorize access, or sanitize untrusted content.

## [1][USE_WHEN]

Apply this standard when prose, documentation, prompt text, or a documentation-derived machine surface places controlling content for an agent reader:
- ordering a unit so the controlling rule leads and the binding constraint closes
- writing or revising local instruction files such as `AGENTS.md`
- publishing maintained `llms.txt`, generated mirrors, or machine-readable indexes
- defining deployed retrieval stores, chunk provenance, or access boundaries
- documenting maintained MCP resources, prompts, and tools for agent readers
- specifying structured-output contracts for a surface that validates them
- separating durable documentation from task prompts and state artifacts

Route the surface before applying detailed rules:

| [INDEX] | [SURFACE]                                                       | [SECTION]                       |
| :-----: | :-------------------------------------------------------------- | :------------------------------ |
|   [1]   | unit ordering and salience                                      | `[2][SERIAL_POSITION_SALIENCE]` |
|   [2]   | durable docs, prompt assets, task instructions, state artifacts | `[6][ARTIFACT_SEPARATION]`      |
|   [3]   | task prompts and structured outputs                             | `[7][TASK_OUTPUT_CONTRACTS]`    |
|   [4]   | `AGENTS.md` positioning behavior                                | `[8][AGENTS_MD_AUTHORING]`      |
|   [5]   | indexes, retrieval, MCP catalogs, generated mirrors             | `[9]` through `[12]`            |

Container choice belongs to [information-structure.md](information-structure.md). Sentence mechanics belong to [style-guide.md](style-guide.md). Evidence strength belongs to [proof.md](proof.md). Document-type routing belongs to [README.md](README.md).

## [2][SERIAL_POSITION_SALIENCE]

Place the controlling rule first and the binding constraint last in every unit. Attention concentrates at sequence edges, and middle-position degradation can still appear in long-context work. Treat the advertised context window as capacity, not as a reason to bury constraints.

Apply the pattern recursively at every scale. Put controlling rules, constraints, proof routes, source provenance, and access boundaries at the unit edges.

| [INDEX] | [UNIT]                 | [LEAD]                                            | [CLOSE]                                 |
| :-----: | :--------------------- | :------------------------------------------------ | :-------------------------------------- |
|   [1]   | Document               | scope and highest-risk constraint                 | boundary, proof route, or next route    |
|   [2]   | Section                | what the section controls                         | boundary or route                       |
|   [3]   | Paragraph and sentence | prose shape from [style-guide.md](style-guide.md) | consequence, proof route, or route-away |

Container choice belongs to [information-structure.md](information-structure.md). Sentence craft belongs to [style-guide.md](style-guide.md).

Salience is relative, not a hard cutoff. The middle down-weights content; it does not erase it. If a constraint is load-bearing and the unit is long enough to need scanning, restate it in compact form at the close, near where an agent acts on it. Keep inventories, history, and non-actionable detail out of the lead. Never bury a failure-impacting constraint, exception, or route-away rule mid-unit.

Use bracketed headings, table rubrics, `[INDEX]` rows, and compact status markers as salience aids, not as decoration. They help an agent find boundaries, compare rows, and filter state; they do not replace the front-and-close rule, claim-level evidence, or precise prose.

This corpus uses the position ring as its normative placement rule. Claims about model, provider, tool, or runtime behavior are evidence questions handled through [proof.md](proof.md), not prose-position rules.

## [3][CONTEXT_INVARIANCE]

The position ring holds across every unit, whatever its lifetime or reader: controlling content at the edges, supporting detail in the middle. Use the three shapes below according to artifact lifetime.

| [INDEX] | [ARTIFACT]       | [LEAD]                        | [BODY]                | [CLOSE]                     |
| :-----: | :--------------- | :---------------------------- | :-------------------- | :-------------------------- |
|   [1]   | Durable document | scope and highest-risk rule   | reusable support      | boundary or route           |
|   [2]   | Task prompt      | objective and output contract | bulky source material | ask and binding constraints |
|   [3]   | Retrieval chunk  | identity and standalone rule  | source detail         | refresh route when needed   |

When long documents lead, anchor the final ask with a bridge phrase such as `Based on the information above`, then restate the critical constraints in compact form. An `AGENTS.md` overlay is a durable-document shape, not a task prompt.

Do not rewrite an ordinary durable document into task-prompt shape, and do not bury a durable rule inside transient task framing.

## [4][CONSTRAINT_STACKING]

Rank instructions so an agent resolves conflicts deterministically. State the rank, not just the rule.

| [INDEX] | [RANK]     | [MEANING]                                                               |
| :-----: | :--------- | :---------------------------------------------------------------------- |
|   [1]   | Invariant  | rule that must not break unless a higher-ranked rule names an exception |
|   [2]   | Preference | default that holds unless a stated condition overrides it               |
|   [3]   | Default    | starting choice an agent may replace when evidence is stronger          |

Remove contradictory instructions rather than layering a caveat on top of a conflicting rule; unresolved contradiction degrades instruction-following more than any single weak rule. If two rules can both apply, name which controls.

## [5][FRAMING_INSTRUCTION_FOLLOWING]

Write instructions as positive imperatives that name the action to take. Agent readers follow an explicit target more reliably than a prohibition.

Accepted: State the constraint first.
Rejected: Do not leave the constraint implicit.
Reason: The accepted sentence names the action; the rejected sentence only names the failure.

Reserve negative form for genuine hard boundaries and pair each prohibition with the positive action that replaces it. Keep guidance clear, consistent, and in a high-salience position; vague or contradicted instructions degrade compliance.

If factual grounding matters, require evidence extraction before synthesis. First, have the agent cite source spans or quote brief excerpts within access and copyright limits. Then reason from them. When quoting is unsafe or unavailable, require source IDs, line references, headings, or field paths.

## [6][ARTIFACT_SEPARATION]

Keep durable documentation, task instructions, prompt assets, and state artifacts in distinct artifacts. Each has a different lifetime and reader contract.

| [INDEX] | [ARTIFACT]            | [HOLDS]                                    | [EXCLUDES]               |
| :-----: | :-------------------- | :----------------------------------------- | :----------------------- |
|   [1]   | Durable documentation | stable policy, examples, links, contracts  | single-interaction state |
|   [2]   | Prompt assets         | reusable task shape and output contract    | higher-ranked policy     |
|   [3]   | Task instructions     | one objective, evidence, constraints, stop | durable standards        |
|   [4]   | State artifacts       | facts, status, next action, gaps           | reusable policy          |

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

Bind structured output to the narrowest contract the consumer actually validates: a schema, a typed tool input, a generated model, a catalog entry, or a documented field list. The contract-strength order is strict schema, typed tool input, generated model, prompt-only JSON, then human-reviewed field list. Use closed, total JSON object schemas only on strict JSON-schema or tool-schema surfaces that support those mechanics. Use `additionalProperties: false`, every property required, and explicit nullable values only where current provider or validator proof supports that shape.

Prefer provider schema enforcement over a schema described in prose where the surface supports it and current proof exists. Treat prompt-only JSON as a weaker fallback that requires validation and repair handling. A schema proves shape, not truth; keep semantic validation, refusal handling, and downstream suitability checks visible in the proof path. If a human reviews the contract instead of tooling enforcing it, state that proof gap.

Keep four checks separate in every machine-facing contract:

| [INDEX] | [CHECK]             | [PROVES]                           | [LIMIT]                         |
| :-----: | :------------------ | :--------------------------------- | :------------------------------ |
|   [1]   | Shape enforcement   | container matches schema or parser | truth, safety, freshness        |
|   [2]   | Source provenance   | fact origin and refresh route      | semantic correctness            |
|   [3]   | Semantic validation | shaped output matches the claim    | authorization or runtime safety |
|   [4]   | Runtime safety      | permission and suitability bounds  | factual correctness             |

Do not let a single check imply the others. A valid JSON object can still be unsupported, unsafe, stale, or semantically wrong.

Source provenance proves origin and refresh route only. Shape enforcement proves the container. Semantic validation proves the claim matches the source. Evaluation receipts prove retrieval quality, ranking, tool choice, latency, or provider behavior. Runtime safety proves authorization and downstream suitability.

## [8][AGENTS_MD_AUTHORING]

Write `AGENTS.md` as a durable, behavioral overlay for one directory. [agents-md.md](agents-md.md) owns structure and anti-fragility; this file owns salience and proof-route positioning for instruction surfaces.

This standard still controls salience: keep the highest-risk invariant at the lead, close with the binding proof or route, layer files by scope, and route provider-loading claims through [proof.md](proof.md). `AGENTS.md` files complement README files; they do not duplicate them, summarize them, carry process notes, or publish session state.

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

Each retrievable chunk should carry enough context to stand alone because retrieval strips surrounding structure. Claim hybrid retrieval, reranking, or provider-specific search only where the deployed stack supports it and the proof names the configured source. Split chunk tables and records that exceed the size limits in [information-structure.md](information-structure.md).

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

## [13][BOUNDARIES]

- [information-structure.md](information-structure.md) carries container form, diagrams, page anatomy, and chunk shape; this standard carries where controlling rules, constraints, proof routes, and source provenance sit within them.
- [style-guide.md](style-guide.md) carries sentence and word craft; this standard carries the cognition rationale for positive, imperative framing.
- [proof.md](proof.md) carries evidence strength, proof details, and the evaluation discipline for machine-facing surfaces.
- [formatting.md](formatting.md) carries the markers and styling that render the constraints this standard places.
- [agents-md.md](agents-md.md) carries the produced structure and anti-fragility rules for `AGENTS.md` files.
- [README.md](README.md) carries document-type routing and is the single index that links across standards.

## [14][VALIDATION]

Use this verification checklist by group:

[POSITION_INSTRUCTIONS]:
- [ ] The controlling rule leads each unit and the binding constraint closes it.
- [ ] Load-bearing constraints in long units are restated near the close.
- [ ] Durable docs, prompt assets, task instructions, and state artifacts stay separate.
- [ ] Instructions are positive imperatives with ranked constraints.
- [ ] Contradictory instructions are removed or the controlling rule is named.
- [ ] Factual tasks require evidence extraction before synthesis.

[AGENT_SURFACES]:
- [ ] Indexes link to canonical docs; `llms.txt` is treated as a map.
- [ ] Retrieval chunks carry source, heading path, route, access, and freshness.
- [ ] MCP resources, prompts, and tools are separated before schemas.
- [ ] Generated mirrors identify source and generation status.
- [ ] Agent surfaces carry deterministic proof receipts through [proof.md](proof.md) and add stochastic fields only when the claim requires them.
- [ ] `AGENTS.md` files preserve salience, scope layering, and artifact separation without duplicating README bodies or session state.

[ROOT_AUDIT]:
- [ ] A reviewer can identify the document lead rule, section closes, and final route boundary.
- [ ] Task prompts, state artifacts, and durable standards are not mixed inside the scored documentation surface.
- [ ] No secrets, nonpublic local paths, or unverified provider claims are exposed.
