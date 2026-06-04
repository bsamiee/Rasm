---
description: Standard for tutorials and learning paths
---

# Tutorial standards

Tutorials are lessons. They teach by guiding a learner through concrete action,
visible results, and a complete tested path. A tutorial is not a task shortcut,
reference catalog, onboarding checklist, operational runbook, or explanation
essay.

## Use when

Use a tutorial when:

- the reader is learning rather than completing normal work;
- the path has fixed inputs and visible results;
- the document can guide a complete first success;
- later lessons build on earlier skills, vocabulary, or artifacts.

Do not use a tutorial for competent-reader procedures, role readiness, incident
response, lookup facts, or conceptual explanation.

## External basis

Use Diataxis for tutorial intent and the tutorial/how-to boundary. Use learning
path practice only when multiple lessons build toward a broader skill. Audience
guidance owns learner fit: the path must account for what the learner already
knows and what they need next.

## Placement

- General tutorial: `docs/tutorials/<lesson>.md`.
- Tutorial path index: `docs/tutorials/<path>/README.md`.
- Tutorial path lesson: `docs/tutorials/<path>/<lesson>.md`.
- Owner-local tutorial: `{owner}/docs/tutorials/<lesson>.md` when the lesson is
  meaningful only inside that owner.
- Owner-local tutorial path: `{owner}/docs/tutorials/<path>/README.md` with
  lessons beside it when the ordered path is local.

Do not create empty tutorial-path directories to satisfy a taxonomy. Add a path
only when multiple tested lessons build on each other.

## Tutorial structure

Use this structure for one tutorial:

```markdown
# Build <observable result>

## What we will build
## Prerequisites
## Start state
## Steps
## Result
## What to notice
## Next steps
```

Use an outcome title that names the thing the learner will complete. Avoid
titles that promise internal learning when the path can name an observable
result.

## Learning path structure

Use this structure for a path index:

```markdown
# <Skill or outcome> learning path

## Audience
## Outcome
## Prerequisites
## Path
## Completion proof
## Related
```

Each path entry must state lesson title, lesson outcome, prerequisite lesson or
starting condition, and observable result or completion proof.

Keep lessons independently testable, but order them so later lessons reuse
skills, vocabulary, or artifacts introduced earlier. If entries can be read in
any order without loss, the document is a hub index, not a learning path.

## Authoring rules

- Teach one coherent learner outcome per tutorial.
- Give the learner meaningful work before abstract explanation.
- Use fixed inputs, sample data, and deterministic commands before variants.
- Introduce terminology near the step that needs it.
- Keep explanations short, concrete, and adjacent to the action they support.
- Show visible results early and often.
- State expected output, file changes, screenshots, UI state, or other signals
  that confirm the learner is on the right path.
- Point out what the learner should notice after important results.
- Prefer repeatable, reversible, or disposable exercises when possible.
- Link deeper explanation, command catalogs, option matrices, and API details.
- End with a result the learner can compare against.
- Link how-to guides only after the learner finishes the lesson.

Learner-facing language such as `We will build ...` is acceptable because the
document acts as a tutor. Do not add author notes, task history, interaction
fragments, or temporary local details.

## Proof

The full tutorial path must be executed as written before publication. Proof
must include:

- exact commands, UI actions, or repository paths used in the path;
- final observable result;
- expected intermediate signals when a learner could otherwise lose confidence;
- source, version, or `Last verified: YYYY-MM-DD` for drift-prone external
  facts;
- known unverified steps when execution depends on reviewer access, hardware,
  credentials, or a live service.

A learning path must also prove lesson order: prerequisites exist, later lessons
do not rely on unexplained state, and completion proof for earlier lessons feeds
the next lesson when the path says it does.

## Boundaries

- Tutorial teaches a learner through a successful first experience.
- Learning path orders multiple lessons or modules toward a broader skill.
- How-to guide helps a competent reader complete one real task.
- Onboarding guide prepares a person for readiness in a role.
- Reference document lists lookup facts.
- Explanation document develops context, concepts, trade-offs, or architecture.
- Runbook starts from an operational trigger and guides recovery.

## Review checklist

- [ ] The document is a tutorial or a learning path, not a blended doc.
- [ ] Audience and start state are clear.
- [ ] Title names an observable outcome.
- [ ] Path is linear until the learner reaches the result.
- [ ] Inputs are fixed, bounded, or intentionally disposable.
- [ ] Explanations are short and placed beside the action that needs them.
- [ ] Expected intermediate results are visible.
- [ ] Final result is observable and comparable.
- [ ] Full path was tested as written, or proof gaps are stated.
- [ ] Learning-path entries are ordered because later lessons build on earlier
      lessons.
- [ ] Reference, how-to, onboarding, runbook, and explanation material is linked
      instead of embedded.
