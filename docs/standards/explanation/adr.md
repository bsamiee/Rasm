---
description: Standard for Architecture Decision Records
---

# ADR standards

ADRs record durable architectural decisions after enough discussion has occurred
to choose an option. They explain why the decision exists, which alternatives
were rejected, and how the decision can be confirmed later.

## Use when

Use an ADR when a decision:

- binds more than one owner, package, runtime boundary, or long-lived contract;
- accepts a trade-off that future maintainers must understand;
- rejects a plausible option that is likely to return;
- supersedes an earlier architectural decision.

Do not use ADRs for implementation plans, meeting notes, rollout sequence,
operational recovery, or temporary proposals. Use a design document before
acceptance and a roadmap when sequence and exit criteria are the main concern.

## External basis

Use the ADR community vocabulary, MADR status model, and Michael Nygard's
decision lifecycle for status, supersession, and immutable accepted records.

## Placement

- Default directory: `docs/decisions/`.
- Default file name: `NNNN-short-title.md`.
- Decision log index: `docs/decisions/README.md`.

Use one decision corpus per repository unless owner-local decision logs already
exist. Keep the established file-name pattern inside an existing corpus. New
corpora use four-digit monotonic numbers, lowercase dash-separated titles, and
`.md` files. Numbers are never reused. Gaps are allowed.

## Lifecycle

Allowed statuses:

- `proposed`: under review and not binding.
- `accepted`: agreed, binding, and ready to implement or enforce.
- `rejected`: considered and declined.
- `deprecated`: no longer relevant, with no replacement.
- `superseded`: replaced by a newer ADR.

Use lower-case status values in front matter. Record replacement decisions in
`superseded_by` instead of embedding a replacement number inside `status`.

Accepted ADR bodies are immutable except for typo fixes, broken links, or narrow
context clarifications that do not change the decision. Change a decision by
superseding it with a new ADR. A superseded ADR must link to its replacement,
and the replacing ADR must link back to every ADR it supersedes.

## Required structure

```markdown
---
status: proposed | accepted | rejected | deprecated | superseded
superseded_by: <NNNN or none>
date: YYYY-MM-DD
decision-makers: <names, teams, or owner roles>
consulted: <reviewed owners or none>
informed: <affected owners or none>
---

# <Decision title>

## Context and problem statement
## Decision drivers
## Considered options
## Decision outcome
### Consequences
### Confirmation
## Pros and cons of the options
## More information
```

## Section rules

- Context is value-neutral and states the forces that make a decision necessary.
- Decision drivers are criteria, not preferred outcomes.
- Considered options include at least two plausible choices unless the ADR
  records a rejected proposal.
- Decision outcome names the selected or rejected option and states the
  rationale.
- Consequences include positive, negative, and neutral effects when present.
- Pros and cons record trade-offs for each material option. Use `Good`,
  `Neutral`, and `Bad` only when a compact option matrix is clearer than prose.
- Confirmation states the future evidence that shows the decision is being
  followed.
- More information links designs, architecture docs, issues, or source contracts
  when they explain the decision.

## Design handoff

Promote a design document to an ADR only when an accepted decision becomes
durable architecture policy. Derive the ADR from final drivers, selected option,
rejected alternatives, consequences, and confirmation evidence. Do not copy the
full design body.

## Boundaries

- Design documents own proposal discussion and review history before acceptance.
- Architecture documents own current structure and invariants.
- Roadmaps own sequence, milestones, and exit proof.
- ADRs own decision rationale and confirmation.

## Review checklist

- [ ] Status, superseded_by, date, decision-makers, consulted, and informed
      fields are present.
- [ ] Context is neutral.
- [ ] Drivers and options are separated.
- [ ] Outcome is explicit.
- [ ] Negative and neutral consequences are recorded when present.
- [ ] Confirmation evidence is stated.
- [ ] Supersession links are bidirectional when applicable.
