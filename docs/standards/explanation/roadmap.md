---
description: Standard for roadmap documents
---

# Roadmap standards

Roadmaps communicate planned sequence, milestone intent, dependencies, exit
criteria, and proof. They help readers understand what must happen next, what
has cleared its exit bar, and what evidence will show that work is done.

## Use when

Use a roadmap when readers need:

- planned sequence across milestones;
- dependency and blocker visibility;
- exit criteria for a build, release, or capability sequence;
- proof surfaces that close milestones.

Do not use roadmaps for current architecture, durable decisions, compatibility
policy, test taxonomy, task procedures, or implementation notes.

## External basis

Use maintained milestone, roadmap, linked-issue, release, and pull-request
preview systems as source truth when they own dates, blockers, status, or exit
evidence. A Markdown roadmap summarizes that contract and links the live source.

## Placement

- Owner-local roadmap: `{owner}/ROADMAP.md`.
- Concern roadmap: `{owner}/{concern}/ROADMAP.md`.
- Repo-wide roadmap: root `ROADMAP.md` only when the whole repository has a
  single coordinated sequence.
- Published product roadmap: maintained product documentation or planning
  surface when the roadmap is external-facing.

Keep the roadmap near the owner that can refresh it.

## Profiles

- Phased build: multi-phase package, tool, or platform work.
- Concern spec: new sub-concern inside a larger owner.
- Foundation status: stable capability set with known extension points.
- Release train: recurring release or iteration plan with repeated gates.
- Public product roadmap: planned work with explicit non-commitment language.
- No roadmap: mature scope where architecture and ADRs already carry truth.

Name one profile in the opening paragraph. Split the document when one page
needs more than one profile.

## Status vocabulary

Define only statuses used by the roadmap:

- `Planned`: accepted for sequencing, but not actively executing.
- `Active`: work is in progress and still inside milestone scope.
- `Blocked`: a named dependency or decision prevents progress.
- `Complete`: exit criteria passed and proof is linked.
- `Deferred`: intentionally moved outside the current sequence.
- `Cancelled`: removed from the plan with a reason.

For public roadmaps, distinguish exploration, design, preview, and general
availability only when the product or release process uses those meanings.

## Required structure

```markdown
# <Scope> roadmap

Status: Planned | Active | Blocked | Complete | Deferred | Cancelled
Profile: Phased build | Concern spec | Foundation status | Release train | Public product roadmap
Owner: <owner role or group>

## Scope
## Current status
## Principles and constraints
## Milestones
## Dependencies and blockers
## Exit proof
## Deferred or out-of-scope work
## Review trigger
## Related
```

`Principles and constraints` is optional when the sequence is purely mechanical.
`Related` is optional when owner-local links already exist.

## Milestone rules

Each milestone needs:

- goal;
- status from the roadmap vocabulary;
- deliverables or scoped outcomes;
- exit criteria;
- proof surface;
- dependencies, blockers, or prerequisite milestones when present;
- owner or accountable review group when ownership is not obvious;
- deferred work when something is intentionally excluded.

Separate intent from proof. Close a milestone only when exit criteria and proof
surface agree.

## Dependencies and blockers

Expose sequencing risk without becoming a task tracker:

- Link the issue, pull request, epic, milestone, release, design, ADR, or
  support record that owns the live dependency.
- Mark `blocks`, `blocked by`, and prerequisite relationships when the planning
  system supports them.
- State external dependencies by owner, contract, or vendor source.
- Record go/no-go decision points when the next milestone depends on evidence.
- Move tactical subtasks to the tracker or design document.

## Exit proof

Roadmap proof is milestone-level. Each milestone must name the smallest evidence
that proves exit:

- source path, manifest, generated contract, or release artifact;
- exact command, build, test, documentation build, link check, or preview build;
- status check or pull request review gate;
- operational verification, runtime run, screenshot, or captured artifact;
- product metric, support signal, adoption threshold, or owner sign-off when
  the milestone is rollout-oriented.

Use [proof.md](../proof.md) for evidence fields and freshness rules.

## Public roadmap rules

Public roadmaps need stronger reader boundaries:

- State whether the roadmap is directional, committed, or historical.
- Avoid exact dates unless the source of truth maintains them.
- Distinguish shipped work from planned work.
- Link shipped items to release notes, issue closures, or other evidence.
- Keep support status and end-of-life truth in a support matrix when those facts
  outgrow the roadmap.

## Boundaries

- Architecture owns structure, invariants, and owner boundaries.
- ADRs own durable decisions.
- Design documents own pre-code proposals.
- Test strategy owns gate taxonomy and flake policy.
- Support matrices own supported versions and lifecycle status.
- Reference documents own command, API, release, and compatibility lookup.
- How-to guides and runbooks own procedures.

## Review checklist

- [ ] Profile is named.
- [ ] Scope is clear.
- [ ] Status vocabulary is defined and used consistently.
- [ ] Every milestone has goal, status, deliverables, exit criteria, proof, and
      dependencies when present.
- [ ] Blockers and prerequisites link the live source of truth.
- [ ] Exit proof is milestone-level and reproducible.
- [ ] Public roadmap content has non-commitment language when applicable.
- [ ] Deferred work is explicit.
- [ ] Adjacent document types are linked instead of embedded.
