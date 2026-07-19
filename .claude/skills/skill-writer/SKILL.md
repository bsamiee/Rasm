---
name: skill-writer
description: >-
    Authors, reviews, and repairs skill bundles — the `SKILL.md` root, the description that wins
    selection, and the references, templates, examples, and scripts one hop down. Use when
    creating a skill, auditing or rebuilding one, ruling a fact's tier, pricing instruction
    rigidity, proving a trigger with evals, or sweeping an installed fleet for colliding triggers,
    shadowed names, cross-bundle forks, unvalidated artifacts, and listing overflow — "why won't
    my skill fire", "this one fires on everything", "audit my skills". Prose register and its
    deterministic gate belong to docgen; where an instruction lives — memory files, rules,
    settings, hooks — belongs to harness-steering.
---

# [SKILL_WRITER]

A skill is deployed law and packaged capability: the description competes for selection in every session, the body competes with the task's own context once loaded, and every bundled file is either routed capability or dead weight.

## [01]-[ROUTING]

[REFERENCES]:
- [01]-[ANATOMY](references/anatomy.md): bundle anatomy — tiers, file kinds, templates, root schema, frontmatter policy, freedom calibration
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
- One technique lives in exactly one tier, and every shipped template and example passes the estate audit's artifact proofs before landing — the audit runs the generic suffix toolchain table itself, and a bundle ships its own `scripts/` validator only for domain proofs the table cannot own.
- A mixed task routes by deliverable, never by topic: prose to the prose owner, fences to the fence owner, bundles to this skill, in sequence.

## [03]-[EVALS]

A skill is proven, never assumed: a baseline run without the skill proves the gap the body closes before law is authored. Trigger evals show it fires on the tasks it owns and stays silent on its neighbors; adherence evals show the loaded body changes output, and a rule that never changes an output across the suite is dead weight, deleted. Self-preference bias is empirical: grading runs in a fresh context or a second lens against transcripts, never the author's read of its own artifact. Evals ride the estate's ruled model alone, overriding the multi-model sweep.

## [04]-[GATE]

Every bundle file passes the docgen skill's prose gate before return, and register inside every bundle file answers to the docgen catalog: this skill owns what the bundle is, docgen owns how its prose speaks.

## [05]-[ESTATE]

An installed fleet is one selection system: descriptions compete in a shared listing, the budget is common property, and a defect in one bundle degrades its neighbors' selection. Shared doctrine has one owning skill every sibling composes silently. `${CLAUDE_SKILL_DIR}/scripts/estate_audit.py <roots...>` sweeps every bundle beneath the given roots in one pass — text rows by default, `--json` for machine consumers, nonzero exit on hard failures; the script's `Check` vocabulary owns the census.
