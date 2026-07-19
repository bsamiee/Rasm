---
name: skill-writer
description: >-
    Designs, authors, reviews, and repairs skill bundles — SKILL.md roots, trigger descriptions,
    references, templates, bundled scripts, frontmatter policy — and proves them with trigger and
    adherence evals. Use when creating a skill, rewriting or auditing an existing bundle, writing
    or tuning a trigger description, splitting an oversized root, deciding whether material earns
    a skill or a cheaper owner, pricing how rigid an instruction is allowed to be, measuring
    trigger and adherence quality, or auditing a skill estate for trigger collisions, listing
    overflow, and forks. Prose register and the deterministic prose gate belong to the
    docgen skill; codex-native format and discovery mechanics belong to the codex skill;
    runnable orchestration scripts belong to workflow-creator; placement across memory files,
    rules, settings, and hooks belongs to harness-steering.
---

# [SKILL_WRITER]

A skill is deployed law plus packaged capability: the description competes for selection in every session, the body competes with the task's own context once loaded, and every bundled file is either routed capability or dead weight.

## [01]-[ROUTING]

[REFERENCES]:
- [01]-[ANATOMY](references/anatomy.md): bundle anatomy — tiers, file kinds, template and example doctrine, root schema, frontmatter policy, freedom calibration
- [02]-[TRIGGERS](references/triggers.md): trigger science, listing economics, invocation modes, description tuning
- [03]-[DEFECTS](references/defects.md): authoring defect catalog, each class with its copyable repair

[TEMPLATES]:
- [01]-[SKILL](templates/skill.template.md): root schema as a copyable skeleton; fill the slots, leave zero residual tokens

[SCRIPTS]:
- [01]-[ESTATE_AUDIT](scripts/estate_audit.py): one-pass fleet sweep; invocation in the estate section

## [02]-[BUNDLE_LAW]

- Byte-truth and mechanism live one hop down when reference lanes are situational — a typical invocation consuming a proper subset; a bundle whose every invocation consumes the whole set stays flat, since splitting always-co-loaded mechanism buys no context and arms the partial-load mis-steer. In a split bundle a root sentence survives only when deleting it loses a ruling needed before any reference opens, and the universal layer is a handful of rulings, never a file.
- A fact lives once in a bundle — the root or exactly one reference owns it, the root's route row is the only pointer, and when copies are found the strongest body wins with every nuance absorbed before the twin goes silent.
- A quality pass is net-negative: near-similar rules accumulated across root and references restructure into one higher-resolution law at the ruled owner under the docgen SAME_DECISION_SPREAD reframe, so resolution rises while line count falls, and a bundle that grew under review is the prime suspect.
- A member, flag, signature, or constant asserted in bundle prose is verified against its live surface before authoring; an unverifiable claim lands as a research row carrying its verification route, never settled prose.
- One technique lives in one tier — reference fence, template slot, or example body — and every shipped template and example passes the bundle's own `scripts/` validator before landing; the estate audit's `NO_VALIDATOR` check owns the coverage census.
- A mixed task routes by deliverable, never by topic: prose to the prose owner, fences to the fence owner, bundles to this skill, in sequence.

## [03]-[EVALS]

A skill is proven, never assumed, and the eval precedes the body: a baseline run without the skill demonstrates the gap the body exists to close before extensive law is authored. Trigger evals show it fires on the tasks it owns and stays silent on its neighbors; adherence evals show the loaded body changes output against the baseline; a body rule that never changes an output across the suite is dead weight and is deleted. Self-preference bias is empirical, so the generator never judges its own bundle: grading runs in a fresh context or a second lens against transcripts, never the author's read of its own artifact. Evals ride the estate's ruled model alone; the estate model law overrides the multi-model sweep.

## [04]-[GATE]

Every bundle file passes the docgen skill's prose gate before return, and register inside every bundle file answers to the docgen catalog: this skill owns what the bundle is, docgen owns how its prose speaks.

## [05]-[ESTATE]

An installed fleet is one selection system: descriptions compete in a shared listing, the budget is common property, and a defect in one bundle degrades its neighbors' selection. Shared doctrine has one owning skill every sibling composes silently. `${CLAUDE_SKILL_DIR}/scripts/estate_audit.py <roots...>` sweeps every bundle beneath the given roots in one pass — text rows by default, `--json` for machine consumers, nonzero exit on hard failures; the script's `Check` vocabulary owns the census.
