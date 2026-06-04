---
description: Standard for how-to guides
---

# How-to standards

How-to guides help a competent reader complete one normal task with a practical
goal. They assume the reader knows the domain, can exercise judgment, and needs
reliable action rather than teaching, background, or lookup catalogs.

## Use when

Use a how-to guide when:

- the reader already understands the domain enough to act;
- the document solves one normal task;
- success can be verified at the end;
- background and lookup facts can be linked rather than embedded.

Do not use a how-to for a first learning path, operational symptom response,
contribution workflow, API catalog, or conceptual explanation. Use a tutorial,
runbook, contributing guide, reference, or explanation document instead.

## External basis

Use Diataxis for the how-to reader need, procedure guidance for action mechanics,
and modular documentation practice to keep task, concept, reference, and
learning content distinct.

## Placement

- Shared normal task: `docs/how-to/<task>.md`.
- Owner-local normal task: `{owner}/docs/how-to/<task>.md` or the nearest
  maintained docs corpus for that owner.
- Contribution workflow: [contributing.md](contributing.md).
- Operational symptom, incident response, recovery, or escalation:
  [runbook.md](runbook.md).
- Learning path for a new skill or first success:
  [tutorial.md](../learning/tutorial.md).

## Required structure

```markdown
# How to <task>

## Goal
## Prerequisites
## Procedure
## Verification
## Troubleshooting
## Related
```

`Troubleshooting` is optional when there are no likely failure modes. `Related`
may be omitted when no maintained adjacent document adds value.

## Scope rules

- One guide solves one real task or problem.
- State the task outcome in the title and `Goal`.
- Start and end in reasonable places for competent readers.
- Include only prerequisites required for this task.
- Keep action in the guide and move background, concepts, API catalogs, option
  inventories, and broad troubleshooting elsewhere.
- Prefer one accessible successful path.
- Allow judgment points only when real work requires them. State the decision
  condition and then return to action.

## Procedure rules

- Use numbered steps when order matters.
- Start each step with an imperative verb.
- State the place of action before the action when the tool, UI, shell, host,
  directory, or document context is not obvious.
- State a condition before the action it controls: `If <signal>, do <action>`.
- Use input-neutral UI verbs from [style-guide.md](../style-guide.md).
- Mark optional steps with `Optional:` at the start of the step.
- Combine simple actions that happen in the same place and produce one logical
  result.
- Link repeated procedures instead of copying them.
- Keep command examples exact, copyable, and scoped to the task.
- Include expected results near the step when success is observable before final
  verification.

## Verification and troubleshooting

- End with verification when success can be observed.
- Verification must prove the task outcome, not merely that a command ran.
- Link reference sources for flags, configuration fields, API behavior, and
  support status.
- Keep troubleshooting to likely failure modes that block this task and have
  actionable recovery.
- Move operational recovery, rollback, escalation, and incident evidence to a
  runbook unless the risky action is part of normal task completion.

## Boundaries

- Tutorial teaches; how-to completes.
- Reference lists facts; how-to uses facts.
- Runbook responds to operational symptoms; how-to performs normal tasks.
- Contributing owns contribution workflow, review evidence, and pull request
  expectations.
- Onboarding owns learning checklists, first safe tasks, shadowing, and
  readiness criteria.

## Review checklist

- [ ] Title starts with `How to`.
- [ ] Goal names one real task outcome.
- [ ] Prerequisites are minimal.
- [ ] Procedure steps are imperative, ordered when needed, condition-first, and
      context-clear.
- [ ] Optional paths are separated or explicitly marked.
- [ ] Verification proves the task outcome.
- [ ] Troubleshooting is task-local and actionable.
- [ ] Background, catalogs, tutorials, runbooks, and contribution workflow are
      linked, not embedded.
