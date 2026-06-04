---
description: Standards for agent-facing documents, indexes, retrieval, and generated mirrors
---

# Agentic documentation

Agent-facing documentation helps tools and maintainers find, trust, retrieve,
reuse, and refresh canonical repository truth. It does not make model output
trustworthy, enforce authorization, or replace the source documents it points
to.

## Use when

Use this standard for:

- local instruction files such as `AGENTS.md`;
- `llms.txt`, `llms-full.txt`, generated mirrors, and machine-readable indexes;
- retrieval stores, chunk metadata, and source provenance rules;
- MCP catalogs, resource catalogs, tool catalogs, and structured output
  contracts;
- durable handoff or state artifacts that record validated progress;
- evaluation notes that prove a machine-facing surface works as intended.

Do not use it as a general prose guide, task guide, or product API reference.
Use [style-guide.md](style-guide.md), [documentation-system.md](documentation-system.md),
and [api.md](reference/api.md) for those concerns.

## Source order

Use this source order for agent-facing surfaces:

1. Current repository source, manifests, contracts, generated output, and
   maintained product documentation.
2. Official protocols, specifications, and vendor documentation for the surface
   being published.
3. Active local standards for document type, structure, style, and proof.
4. Local generated mirrors, indexes, summaries, or hints.

Do not claim crawler adoption, ranking, answer correctness, access control, or
injection resistance from documentation alone. State only what the repository
publishes and how it is refreshed.

For `AGENTS.md` format context, use [agents.md](https://agents.md/) as
open-format background. For Codex discovery, precedence, fallback filename, and
byte-limit behavior, use the official OpenAI Codex documentation.

## Artifact boundaries

Keep durable docs, task instructions, and state artifacts separate.

- Durable docs contain stable policy, repo conventions, reusable examples,
  canonical links, and tool contracts.
- Task instructions contain the current objective, current evidence or context
  to inspect, hard constraints, done condition, output contract, validation
  rule, and stop rule for a specific interaction.
- State artifacts contain validated facts, current status, unresolved
  questions, next safe action, progress, provenance, evidence, and proof gaps.

Do not publish task-specific interaction material as durable docs. Convert only
stable rules into the owning standard.

## Publication surfaces

- `AGENTS.md`: scoped operating instructions for agents working in a directory.
- `llms.txt`: curated map to canonical docs for a published site or corpus.
- `llms-full.txt`: expanded context only when generated from canonical docs,
  reviewed, and marked as generated.
- Markdown or MDX mirrors: allowed when they preserve canonical meaning and
  identify the source page.
- Retrieval indexes or vector stores: allowed when source, refresh path,
  chunking policy, and access boundary are documented.
- MCP catalogs: list resources, prompts, and tools before detailed schemas.

Keep indexes shallow. Link to canonical README, architecture, API, reference,
how-to, runbook, support, and standards pages instead of copying their bodies.

## Instruction files

Instruction files are local operating overlays. They should be short,
scope-bound, and delta-only: tell the agent what changes in this directory,
which standards to read, which commands or proof gates apply, and which patterns
are forbidden.

Use this shape when a directory needs an instruction file:

1. Scope.
2. Read order.
3. Routing or ownership rules.
4. Execution or rewrite rules.
5. Exclusions.
6. Validation.

Do not use an instruction file as a style guide, architecture document, command
catalog, or task diary. If the instruction depends on provider behavior, include
`Source of truth:`, `Last verified:`, and `Review trigger:` beside the claim.

## Retrieval and chunks

Use [information-structure.md](information-structure.md) for source-shaped
headings, examples, tables, diagrams, and chunk boundaries. This standard adds
machine-facing provenance and safety rules.

Retrieval documentation must state:

- canonical corpus or source documents;
- source path or URL;
- heading path or parent document identity;
- document type and owner when known;
- generated status;
- refresh owner or generation path;
- access class and filtering rule when content is not public;
- review trigger for drift-prone content.

Use hybrid retrieval, reranking, or provider-specific search only when the
actual stack supports it and proof names the configured provider, command, or
source of truth.

## MCP and tool catalogs

Document MCP surfaces by control type:

- Resources are passive data or content the client reads as context.
- Prompts are reusable interaction templates.
- Tools are executable operations with typed inputs and outputs.

Catalog first. Name the capability, when to inspect it, required authorization
or local setup, and the canonical reference location. Detailed schemas belong in
API or reference docs. A documented tool is not safe to call merely because it
is documented; safety comes from permissions, review, and runtime controls.

## Metadata and generated content

Use front matter or sidecar metadata only when a renderer, indexer, generator,
retrieval store, or review workflow consumes it.

Durable fields may include:

- `description`;
- `owner`;
- `source`;
- `source_of_truth`;
- `evidence`;
- `generated`;
- `generated_from`;
- `last_reviewed`;
- `last_verified`;
- `review_trigger`;
- `access`;
- `parent`.

Use snake-case field names when a machine reads the metadata. Map
`source_of_truth`, `evidence`, `last_verified`, and `generated_from` to the
human-facing proof labels `Source of truth:`, `Evidence:`, `Last verified:`,
and `Generated from:`.

Generated or mirrored files must link the canonical source, state the generator
or workflow when maintained, preserve heading hierarchy where possible, mark
omissions, and exclude secrets, personal data, task-specific notes, and private
machine details. Do not hand-edit generated mirrors as independent truth.

## Long context and structured outputs

When assembling context-heavy task instructions, put durable source material
before the specific ask and require evidence extraction before synthesis when
factual grounding matters. Do not rewrite ordinary docs into task-instruction
shape.

Task-facing templates may use this order:

1. Objective.
2. Done condition.
3. Current evidence or source material.
4. Hard constraints and exclusions.
5. Output contract.
6. Validation and stop rule.

Machine-consumed outputs should use the narrowest contract that the consumer
actually validates: schema, typed tool input, generated model, catalog entry,
or documented field list. State proof gaps when a contract is reviewed by a
human rather than enforced by tooling.

## Evaluation and safety

Treat agent-facing surfaces like machine-readable contracts when they affect
retrieval, generated mirrors, tool use, or structured outputs.

Useful proof includes:

- representative questions or tasks from real maintenance failures;
- baseline comparison against the previous surface, manual route, or known
  failure;
- repeated trials when stochastic output, retrieval ranking, or tool selection
  is the claim;
- exact checks for format and link correctness;
- source trace review for retrieved or generated answers;
- unsupported-claim review;
- tool-call failure review;
- model or provider version, configured tool set, token or context budget,
  latency, tool errors, and trace ID when the evaluation surface owns those
  facts;
- latency, cost, and context-budget signals when the surface owns them.

Separate public, internal, restricted, and secret material into distinct corpora
or enforce equivalent filters. Documentation can reduce ambiguity, but it
cannot authorize access or sanitize untrusted content by itself.

## Boundaries

- `README.md` and hub pages route readers to canonical docs.
- Reference docs own lookup facts and catalogs.
- API docs own generated and contract-backed API truth.
- Proof standards own evidence strength and freshness.
- Information structure owns page shape and chunk boundaries.
- This standard owns machine-facing publication, retrieval, metadata, and
  generated mirror safety.

## Review checklist

- [ ] Indexes link to canonical docs instead of copying them.
- [ ] Optional context is marked as optional.
- [ ] `llms.txt` is treated as a map, not an enforcement mechanism.
- [ ] MCP resources, prompts, and tools are separated.
- [ ] Retrieval metadata preserves source, heading path, owner, access, and
      freshness when those facts matter.
- [ ] Generated mirrors identify source and generation status.
- [ ] Metadata exists only where a consumer reads it.
- [ ] Task-instruction contracts include objective, done condition, context,
      constraints, output contract, validation, and stop rule.
- [ ] State artifacts separate validated facts, status, next action, evidence,
      and proof gaps.
- [ ] Evaluations include baseline, repeated trials when needed, source trace,
      tool errors, and unsupported-claim review.
- [ ] Provider-specific claims have current primary-source proof.
- [ ] No secrets, private machine details, task-specific interaction material,
      or unverified provider-behavior claims are exposed.
