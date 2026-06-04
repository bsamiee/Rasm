---
description: Standard for onboarding documentation
---

# Onboarding standards

Onboarding documents help a new contributor, maintainer, or operator become
effective in a role through guided learning, safe practice, shadowing, and
observable readiness criteria. They prepare people for responsibility; they are
not tutorials, contributing guides, runbooks, or reference catalogs.

## Use when

Use onboarding when a person needs to become ready for:

- contribution;
- maintainership or review authority;
- operational responsibility;
- support or cross-functional review work.

Do not use onboarding for a single product lesson, normal task procedure,
incident response, contribution workflow, or lookup facts.

## External basis

Use SRE onboarding practice for role-readiness patterns, ordered learning,
shadowing, progressive access, and readiness gates. Use open-source maintainer
practice for process documentation, sustainability, and stale-doc handling.
Repository truth owns factual claims, prerequisites, commands, permissions, and
readiness proof.

## Placement

- Repository onboarding: `ONBOARDING.md`.
- Maintainer onboarding: `docs/onboarding/maintainers.md` or the maintained
  governance corpus when authority is repo-wide.
- Operator onboarding: `docs/onboarding/operators.md` or the owning operations
  corpus when the role includes on-call or production responsibility.
- Owner-local onboarding: `{owner}/ONBOARDING.md` when the learning path,
  boundary map, exercises, and readiness criteria are local.

Keep onboarding near the role owner that can refresh it.

## Profiles

- Contributor ramp: first safe contribution, local context, review
  expectations, and handoff to the contributing guide.
- Maintainer ramp: review authority, triage, release or security posture,
  governance boundaries, and process stewardship.
- Operator ramp: system boundaries, observability, incident context, shadowing,
  and operational readiness.
- Cross-functional ramp: enough context to review, support, or coordinate work
  without owning the system.

Choose one primary profile. Split the document when maintainer authority,
contributor workflow, and operational readiness each need separate criteria.

## Required structure

```markdown
# <Audience> onboarding

## Audience
## Prerequisites
## Readiness target
## Boundary map
## Learning checklist
## First safe tasks
## Shadowing and review path
## Readiness criteria
## Owner roles
## Documentation refresh task
## Related
```

`Shadowing and review path` may be omitted only when the role has no supervised
practice or authority transition. `Documentation refresh task` may be omitted
only when a separate maintained checklist owns it.

## Learning checklist

Each checklist item should state:

- boundary, subsystem, workflow, or responsibility being learned;
- canonical reading or source path;
- concept or behavior the learner must understand before moving on;
- exercise, comprehension question, review task, or observation that proves
  progress;
- owner role for questions and review;
- access level required, when access matters;
- evidence or sign-off that closes the item.

Order checklist items by how understanding builds: request flow, dependency
chain, lifecycle stage, operational escalation path, or maintainer
responsibility. Do not order by file tree unless the file tree is the learning
path.

## First safe tasks

First safe tasks give the learner useful ownership without exposing users,
production state, release authority, or broad repository risk.

Good first safe tasks include:

- correcting a stale onboarding or reference section with owner review;
- answering a bounded comprehension question from source truth;
- drawing and reviewing a current boundary map;
- reproducing a documented local verification path;
- triaging a non-urgent issue against existing policy;
- reviewing a low-risk change with a maintainer's second review;
- observing an operational event and writing a reviewed summary.

A task is safe because its blast radius is small and supervision is clear, not
because it is low value.

## Shadowing and review

Shadowing must define what the learner observes, what the mentor does, and what
evidence closes the session:

- activity to shadow, such as review, triage, release, support, incident
  response, or on-call;
- prerequisites before the first shadow session;
- expected learner notes, questions, or debrief artifact;
- mentor or owner role responsible for feedback;
- when the learner may reverse-shadow or lead under supervision;
- abort criteria when the activity becomes too risky for training;
- readiness signal before independent authority.

Operational onboarding should include live or realistic practice when possible.
Maintainer onboarding should include supervised issue triage, pull request
review, release rehearsal, security process review, and governance decisions
before elevated permissions.

## Readiness and proof

Readiness criteria must be observable and role-specific:

- learner completed the checklist items that gate the role;
- learner explained the boundary map and failure modes to the owner role;
- learner completed first safe tasks with accepted review;
- learner shadowed required activities and debriefed with the mentor;
- learner led a supervised review, drill, release rehearsal, or incident
  response segment without unsafe escalation;
- owner role approved the authority transition and recorded remaining limits.

Proof must include source paths or maintained docs, exact commands or exercises
when runnable behavior matters, owner sign-off, access boundary when authority
changes, and a review trigger for drift-prone sections.

## Boundaries

- Tutorial teaches a learner through a product or tool path.
- Onboarding prepares a person for contribution, maintenance, review,
  governance, support, or operational responsibility.
- Contributing defines workflow and pull request evidence for everyone.
- How-to helps a competent reader complete one normal task.
- Reference lists lookup facts.
- Runbook defines recovery steps under pressure.
- Test strategy defines gate policy.
- Support matrix defines supported versions, platforms, features, and limits.

## Review checklist

- [ ] Audience is explicit.
- [ ] Profile and readiness target are clear.
- [ ] Learning checklist is ordered by understanding.
- [ ] Boundary map links canonical current docs and source truth.
- [ ] First safe tasks are useful, supervised, and low risk.
- [ ] Shadowing or supervised review is defined when authority changes.
- [ ] Readiness criteria are observable and role-specific.
- [ ] Owner roles are present.
- [ ] Documentation refresh task is included or intentionally owned elsewhere.
- [ ] Proof names evidence, access boundaries, sign-off, and refresh triggers.
- [ ] Tutorial, contributing, how-to, reference, runbook, support, and test
      strategy content is linked instead of embedded.
