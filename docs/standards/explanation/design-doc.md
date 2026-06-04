---
description: Standard for pre-implementation design documents
---

# Design document standards

Design documents are collaborative pre-implementation proposals. They frame a
problem, compare options, collect owner feedback, define review slices, and
state validation before durable decisions or implementation changes land.

## Use when

Use a design document when work needs:

- pre-code consensus across owners or boundaries;
- option comparison and trade-off review;
- review slices before implementation;
- a bounded final-objection window;
- validation planning before the change lands.

Do not use a design document for accepted architectural decisions, milestone
sequence, runbook response, reference catalogs, or contributor workflow. Link
the owning document type instead.

## External basis

Use design-doc practice for pre-code consensus, small-change review practice for
review slices, and final-comment or Last Call practice for bounded final
objections after discussion has produced a clear direction.

## Placement

- Owner-local design: `{owner}/design/{topic}.md`.
- Research spike: `{owner}/design/research-{topic}.md`.
- Shared design corpus: `docs/design/{topic}.md` when the proposal spans owners
  or the repository already maintains that corpus.
- Optional hub: `{owner}/design/README.md` or `docs/design/README.md`.

Keep design documents near the owner or corpus that will maintain the proposal
history.

## Lifecycle

Use these statuses:

- `Draft`: authors are still shaping problem, goals, and viable approaches.
- `Discussion`: reviewers are testing trade-offs, alternatives, scope, and
  review slices.
- `Last Call`: the selected direction is stable and remaining objections must be
  closed, assigned, or accepted as risk before acceptance.
- `Accepted`: accountable owners approve implementation.
- `Implemented`: implementation and validation landed, with evidence linked.
- `Abandoned`: the proposal will not continue, with the reason recorded.

RFC-style review is a lifecycle mode of a design document. Do not fork content
into a separate RFC artifact unless a hosting or governance process requires it.

## Required structure

```markdown
# <Title>

Status: Draft | Discussion | Last Call | Accepted | Implemented | Abandoned
Date: YYYY-MM-DD
Authors: <names or owner roles>
Reviewers: <consulted owners or review groups>

## Problem
## Goals
## Non-goals
## Context
## Proposed approach
## Alternatives considered
## Review slices
## Cross-cutting implications
## Risks and open questions
## Last Call record
## Validation and proof plan
```

`Last Call record` is required only for documents that reach `Last Call` or
later.

## Section rules

- Problem states the user, product, operational, or engineering pressure.
- Goals and non-goals bound scope before the solution.
- Context links current source truth, prior designs, issues, ADRs, or standards.
- Proposed approach describes design shape and key decisions, not every task.
- Alternatives include the strongest rejected options with trade-offs.
- Review slices define self-contained changes, dependency order, reviewer
  focus, and rollback or abort boundaries when relevant.
- Cross-cutting implications cover security, privacy, accessibility,
  internationalization, data, operational, compatibility, and runtime concerns.
- Risks and open questions stay active until closed, assigned, deferred with an
  owner, or accepted as risk.
- Last Call record summarizes selected direction, unresolved objections,
  objection owners, deadline, notification channel, and final disposition.
- Validation and proof plan names the commands, contracts, runtime checks,
  review gates, artifacts, or acceptance criteria that will prove the design.

## Review slices

A review slice is one self-contained change that can be understood, reviewed,
validated, and reverted without requiring the whole proposal to land at once.

Use slices to separate:

- preparatory refactors or migrations;
- contract or schema changes;
- implementation changes;
- tests and validation artifacts;
- rollout, cleanup, or removal work.

Do not use slice tables to hide a roadmap. If the sequence becomes a milestone
plan, link a roadmap and keep only the review shape in the design.

## Last Call

Enter `Last Call` only when reviewers can evaluate the final proposed direction
without rediscovering the discussion. If substantial new information appears,
move the document back to `Discussion` and update the trade-off summary before
opening a new Last Call.

## ADR handoff

Create an ADR after acceptance when the design binds multiple owners, packages,
runtime boundaries, external contracts, or other durable architecture decisions.
The ADR records selected decision, drivers, consequences, and confirmation
evidence; the design remains the proposal and review history.

## Boundaries

- ADRs own accepted durable decisions.
- Roadmaps own sequence and milestone exit proof.
- Architecture owns current structure and invariants.
- Reference and API docs own lookup and generated contract truth.

## Review checklist

- [ ] Status follows the design lifecycle.
- [ ] Goals and non-goals are explicit.
- [ ] Reviewers or consulted owners are listed.
- [ ] Alternatives are real options with trade-offs.
- [ ] Review slices are self-contained and ordered.
- [ ] Cross-cutting implications are considered.
- [ ] Last Call risks are closed, assigned, deferred, or accepted.
- [ ] Proof plan is specific and evidence-backed.
