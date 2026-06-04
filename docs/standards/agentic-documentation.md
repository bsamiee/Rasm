---
description: Position, agent cognition, and machine-facing publication surfaces
---

# Agentic documentation

This standard owns position and agent cognition: where controlling content sits inside a unit, how serial-position salience governs that placement, and how machine-facing surfaces publish repository truth so agents find, trust, and refresh it. It does not decide container form, sentence craft, or evidence strength. It also does not make model output trustworthy, authorize access, or sanitize untrusted content; documentation reduces ambiguity, never enforces it.

## Use when

Apply this standard when a document or surface places high-value content for an agent reader, or when you publish a machine-facing surface:

- ordering a unit so the controlling rule leads and the binding constraint closes;
- writing or revising local instruction files such as `AGENTS.md`;
- publishing `llms.txt`, generated mirrors, or machine-readable indexes;
- defining retrieval stores, chunk provenance, or access boundaries;
- cataloging MCP resources, prompts, and tools, or specifying structured-output contracts;
- separating durable documentation from task prompts and state artifacts.

Container choice, sentence mechanics, and evidence strength belong to the form, craft, and evidence standards. Document-type routing belongs to the index.

## Serial-position salience

Place the controlling rule first and the binding constraint last in every unit, because attention concentrates at sequence edges and that bias persists in current long-context frontier models rather than fading as windows grow. Treat the advertised context window as an upper bound, not a working budget: effective working context is roughly 60-70% of the nominal window, and the middle of a long unit is where high-value content is lost.

Apply the pattern recursively at every scale; this recursion — where high-value content sits, in what order, scale-invariant — is the rule this standard owns, distinct from which container carries it (information-structure) and the prose that realizes it (style-guide).

- Document: lead with scope and the highest-risk constraint; close with boundaries and the proof or route that makes the document safe to reuse.
- Section: open with what the section controls; end on its boundary or owner.
- Paragraph and sentence: [style-guide.md](style-guide.md) owns the prose shape that realizes the ring.

Salience is relative, not a hard cutoff: the middle down-weights content, it does not erase it. If a constraint is load-bearing and the unit is long, restate it in compact form at the close, near where an agent acts on it. Keep low-value inventories, history, and incidental detail out of the lead, and never bury a high-risk constraint, exception, or route-away rule mid-unit.

`Source of truth:` Anthropic, OpenAI, and Google prompt-engineering documentation; "Lost in the Middle" (Liu et al., 2023) and RULER (NVIDIA, 2024). `Last verified:` 2026-06-04.

## Context invariance

The position ring holds across every unit, whatever its lifetime or reader: controlling content at the edges, supporting detail in the middle. Three instantiations carry it.

- Durable document: a standard or a reference page leads with scope and the highest-risk constraint and closes on boundaries and route; an `AGENTS.md` overlay is a position-ring instance of this same durable-document shape.
- Task prompt: order high-level role and constraints first, source material and long context next, and the immediate ask last. If documents lead and the immediate ask closes, anchor the ask with a transition phrase such as "Based on the information above" so the model re-binds to the supplied context before reasoning, then restate the binding constraints in compact form.
- Retrieval chunk: lead each chunk with the identity and constraint that let it stand alone once retrieval strips surrounding structure.

Do not rewrite an ordinary durable document into task-prompt shape, and do not bury a durable rule inside transient task framing.

## Constraint stacking

Rank instructions so an agent resolves conflicts deterministically. State the rank, not just the rule:

- Invariants: rules that must never break. State them first and absolutely.
- Preferences: defaults that hold unless a stated condition overrides them.
- Defaults: starting choices an agent may replace when evidence is stronger.

Remove contradictory instructions rather than layering a caveat on top of a conflicting rule; unresolved contradiction degrades instruction-following more than any single weak rule. If two rules can both apply, name which controls.

## Framing for instruction-following

Write instructions as positive imperatives that name the action to take, because agents follow an explicit target more reliably than a prohibition. Convert "do not leave the constraint implicit" into "state the constraint first." Reserve negative form for genuine hard boundaries, and pair each prohibition with the positive action that replaces it. Keep guidance clear, consistent, and in a high-salience position; current models are comparatively strong at following clear instructions and comparatively weak at reconciling vague or contradicted ones.

If factual grounding matters, require evidence extraction before synthesis: have the agent quote or cite the source spans first, then reason from them. This quote-before-synthesis order reduces unsupported claims and keeps the proof trail inspectable.

## Artifact separation

Keep durable documentation, task instructions, and state artifacts in distinct artifacts; each has a different lifetime and reader contract.

- Durable documentation holds stable policy, conventions, reusable examples, canonical links, and tool contracts. It outlives any single interaction.
- Task instructions hold one objective, the done condition, the current evidence to inspect, hard constraints, the output contract, and the stop rule for one interaction, in the canonical field order the Task and output contracts section owns.
- State artifacts hold validated facts, current status, the next safe action, open questions, provenance, and known proof gaps for resumable work.

Do not publish task-specific interaction material as durable documentation. Promote only the stable rule into its owning standard, and leave the transient context in the artifact that owns the interaction.

## Provider behavior

Treat provider-specific guidance as preferred patterns within a converging ecosystem, not as iron laws, and adjust for model class: reasoning-tuned models want leaner prompts with fewer in-prompt examples, whereas standard models benefit from explicit few-shot structure. Keep a single delimiter family — XML-style tags or Markdown sectioning, not both — consistent within one prompt.

- Claude: structure prompts with explicit sections and XML-style tags marking instructions, context, and inputs; place high-level instructions early, long source documents in the middle, and the immediate task plus the critical reminder at the end. Favor durable workspace norms in `CLAUDE.md` or `AGENTS.md`.
- GPT and Codex: state the outcome and success criteria first, remove conflicting instructions, and control length with an explicit verbosity or word budget. Move output schemas out of the prose and into the provider's structured-output mechanism wherever the result feeds a tool.
- Gemini: give direct, task-oriented instructions with an explicit output specification, use schema-backed structured output for extraction and agent-to-agent exchange, and run verification as a distinct step after generation rather than folding it into the same call.

`Source of truth:` current Anthropic, OpenAI, and Google prompt-engineering and structured-output documentation. `Last verified:` 2026-06-04. `Review trigger:` provider prompt-engineering guidance changes.

## Task and output contracts

Order a context-heavy task instruction to the position ring, and bound it to a contract the consumer can check.

Order a task instruction as:

1. Objective.
2. Done condition.
3. Current evidence or source material.
4. Hard constraints and exclusions.
5. Output contract.
6. Validation and stop rule.

Bind machine-consumed output to the narrowest contract the consumer actually validates: a schema, a typed tool input, a generated model, a catalog entry, or a documented field list. Close and total the schema — `additionalProperties: false` with every field marked required — so the consumer validates a fully-specified shape rather than a narrow-but-open one. Prefer a provider's schema-enforced structured output over a schema described in prose where the surface supports it, and treat output format as a tunable choice rather than a fixed default. If a human reviews the contract instead of tooling enforcing it, state that proof gap.

## AGENTS.md authoring

Write `AGENTS.md` as a durable, behavioral overlay for one directory: what changes here, which standards and commands apply, and which patterns are forbidden. It complements the human-facing README; it does not duplicate it.

Start from a recommended minimal section set, shared by both the include-list below and the rendered skeleton so the bullets and the skeleton headings line up. This set is the suggested baseline, not a closed vocabulary: a directory may extend it with additional bracketed sections (for example load order, navigation context, routing tables, or rewrite rules) or relabel a section where a clearer name fits its needs. What binds is the include/exclude content rule below, not the exact section names. Whatever sections you keep, include only stable, universal, behavioral content an agent acts on:

- `Scope`: directory scope and purpose, and the read order an agent follows into the deeper rules;
- `Routing`: routing to the standards and reference files that own deeper rules, and ownership of conflicting guidance;
- `Execution rules`: build, test, and quality commands, code-style and review expectations, and commit and pull-request expectations;
- `Exclusions`: forbidden patterns, security constraints, and known gotchas;
- `Validation`: the gate that proves a change here, plus `Source of truth:` and `Last verified:` for any provider-behavior claim.

Render the overlay from this copy-safe heading skeleton, an illustrative template rather than a fixed set, so an author copies the shape rather than reconstructing it from prose and then extends or relabels sections as the directory requires:

```markdown conceptual
# <Directory> agents

<Lead: what changes in this directory and the one rule an agent must not break.>

## Scope
<What this directory owns; the read order into the standards and reference files below.>

## Routing
<Which standards and reference files own the deeper rules; which file wins on conflict.>

## Execution rules
<Build, test, and quality commands; code-style, review, and commit or pull-request expectations.>

## Exclusions
<Forbidden patterns, security constraints, and known gotchas for this directory.>

## Validation
<The gate that proves a change here; `Source of truth:` and `Last verified:` for any provider claim.>
```

Exclude content that rots or wastes context:

- full README or marketing copy;
- exhaustive path or file enumerations that change with the tree;
- large auto-generated reference dumps;
- logs, transient state, or per-session task notes.

Layer files by scope and keep each lean. Resolution concatenates from repository root toward the edited file, so the closest file wins on conflict while higher-level files contribute non-conflicting defaults. Place a file only where a directory carries guidance specific to that level and needed across sessions; every word loaded costs reasoning budget, so a top-level overlay should stay near one screen. Start from the recommended `Scope`, `Routing`, `Execution rules`, `Exclusions`, `Validation` skeleton above, then extend or relabel its sections where the directory needs more. Iterate the file from observed agent failures rather than from speculation, and record provider-behavior claims with `Source of truth:` and `Last verified:` beside the claim.

## llms.txt and indexes

Keep machine-readable indexes shallow and curated. Link to the canonical README, architecture, API, reference, how-to, runbook, support, and standards pages; do not copy their bodies into the index.

- `llms.txt`: a curated map to canonical documents for a published corpus, treated as a routing map and never as an enforcement or ranking mechanism.
- `llms-full.txt`: expanded context only when generated from canonical documents, reviewed, and marked as generated.
- Markdown or MDX mirrors: allowed when they preserve canonical meaning and name their source page.

Claim only what the repository publishes and how it refreshes. Do not claim crawler adoption, ranking, answer correctness, access control, or injection resistance from documentation alone.

## Retrieval and provenance

Document a retrieval surface with the provenance an agent needs to trust and refresh a chunk. State, per corpus or chunk:

- the canonical corpus or source documents;
- the source path or URL and the heading path or parent identity;
- the document type and owner when known;
- generated status and the refresh owner or generation path;
- the access class and filter rule when content is not public;
- the review trigger for drift-prone content.

Carry these as a per-chunk provenance record, one `label: value` per line, so every retrievable chunk header is copy-safe rather than reconstructed from prose:

```markdown conceptual
source_path: <repo path or URL of the source document>
heading_path: <H1 > H2 > H3 trail, or the parent identity>
doc_type: <reference | runbook | architecture | ...>
owner: <accountable role or group, when known>
generated: <true | false; generation path or workflow when true>
access: <public | internal | restricted | secret; filter rule when not public>
review_trigger: <event that makes the chunk stale>
```

Each retrievable chunk should carry enough context to stand alone, because retrieval strips surrounding structure. Claim hybrid retrieval, reranking, or provider-specific search only where the deployed stack supports it and the proof names the configured provider, command, or source of truth. Keep very large tables and long records chunked rather than passed whole, because the same long-context degradation that erodes prose erodes oversized structured content.

## MCP and tool catalogs

Catalog MCP surfaces by control type before detailing any schema:

- Resources: passive data or content the client reads as context.
- Prompts: reusable interaction templates.
- Tools: executable operations with typed inputs and outputs.

For each capability, name what it is, when to inspect it, the required authorization or local setup, and where its canonical reference lives. For a tool, also name the precondition that gates invocation and the case where it must not be invoked. Detailed schemas belong in API or reference documentation. A documented tool is not safe to call merely because it is documented; safety comes from permissions, review, and runtime controls, never from the catalog entry.

## Metadata and generated mirrors

Add front matter or sidecar metadata only when a renderer, indexer, generator, retrieval store, or review workflow consumes it. Use snake-case field names when a machine reads the field. Durable fields may include `description`, `owner`, `source`, `source_of_truth`, `evidence`, `generated`, `generated_from`, `last_reviewed`, `last_verified`, `review_trigger`, `access`, and `parent`. Map each machine field to its human-facing proof label so the same fact reads the same way in prose and in metadata; the proof labels are the ones proof.md owns.

| Frontmatter field | Proof label        |
| ----------------- | ------------------ |
| `source_of_truth` | `Source of truth:` |
| `evidence`        | `Evidence:`        |
| `last_verified`   | `Last verified:`   |
| `review_trigger`  | `Review trigger:`  |
| `generated_from`  | `Generated from:`  |

Fields with no proof-label counterpart — `description`, `owner`, `source`, `generated`, `last_reviewed`, `access`, `parent` — are triage or routing metadata and do not assert a claim, so they carry no prose label.

A generated or mirrored file must link its canonical source, name the generator or workflow where one is maintained, preserve heading hierarchy where possible, mark omissions, and exclude secrets, personal data, task notes, and private machine details. Do not hand-edit a generated mirror as independent truth.

Separate public, internal, restricted, and secret material into distinct corpora or enforce equivalent filters at the boundary. Documentation can describe an access class; it cannot enforce one.

## Boundaries

- [information-structure.md](information-structure.md) owns container form, diagrams, page anatomy, and chunk shape; this standard owns where high-value content sits within them.
- [style-guide.md](style-guide.md) owns sentence and word craft; this standard owns the cognition rationale for positive, imperative framing.
- [proof.md](proof.md) owns evidence strength, freshness fields, and the evaluation discipline for machine-facing surfaces.
- [formatting.md](formatting.md) owns the markers and styling that render the constraints this standard places.
- [README.md](README.md) owns document-type routing and is the single index that links across standards.

## Review checklist

- [ ] The controlling rule leads each unit and the binding constraint closes it.
- [ ] Load-bearing constraints in long units are restated near the close.
- [ ] Durable docs, task instructions, and state artifacts stay separate.
- [ ] Instructions are positive imperatives with ranked constraints.
- [ ] Provider claims carry a current primary source and `Last verified` date.
- [ ] `AGENTS.md` files are behavioral, lean, and free of rot-prone enumeration.
- [ ] Indexes link to canonical docs; `llms.txt` is treated as a map.
- [ ] Retrieval chunks carry source, heading path, owner, access, and freshness.
- [ ] MCP resources, prompts, and tools are separated before schemas.
- [ ] Metadata exists only where a consumer reads it.
- [ ] Generated mirrors identify source and generation status.
- [ ] No secrets, private paths, or unverified provider claims are exposed.
