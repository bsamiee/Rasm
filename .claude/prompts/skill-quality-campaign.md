# [SKILL_REFINEMENT_CAMPAIGN]

Raise a named set of `.claude/skills/` bundles to a verified 11/10, where a naive agent that loads the skill cold produces world-class output — dense, rich, correct, zero naivety or bloat — held to the same bar the repo demands of code and spec-sheets. The lever is iterative refinement, never accretion: trim no-op and dead weight, consolidate weak references, structure the root and its references correctly, then harden and extend from real work, proven each round by an adversarial before/after eval an independent grader scores. `hooks-builder` is the finished exemplar of this bar and process (it moved a cold agent from ~4-5/10 to 11/10 by finding and closing real behavioral defects linting could not see); mine it for the finished shape, never as a shortcut past a skill's own research.

## [01]-[GROUNDING]

Before touching any bundle, internalize the standard the work is measured against.

- Load the `docgen` skill (the prose register, the named-defect catalog, the deterministic prose gate) and the `skill-writer` skill (bundle anatomy, root schema, trigger economics, the eval loop). Every durable line answers to docgen; every bundle-shape decision answers to skill-writer.
- Read `docs/standards/{design-doctrine,formatting,information-structure,style-guide}.md` — the durable-prose law.
- Read `docs/stacks/<language>/README.md` (any one; python is the fullest) for its PAGE-CRAFT law. It is the model for how a corpus of references implicitly cohere: each page owns one disjoint layer, re-teaches no sibling, and every later page composes the earlier ones as settled law without restating them. A skill's `references/` set follows this same discipline — cohesive per subject, mutually non-duplicating, each implicitly holding the others' standards.
- Read the `hooks-builder` bundle in full — `SKILL.md`, every `references/*.md`, `templates/*`, `examples/*` — as the finished bar: a lean router, cohesive references that own one concern each, templates that establish every implementation-type structure, and examples that are complete ultra-advanced compositions used in conjunction with the templates.

## [02]-[THE_BAR]

11/10 is an output property, not a document-length property. A cold agent that loads the skill and does the work it owns must produce world-class output: dense, polymorphic, correct against current provider/library reality, following the skill's own stated law, with zero naive patterns, zero bloat, zero no-op filler. The bundle earns this when:

- The `SKILL.md` is a correct router plus common-path law — under 500 lines, charter lead then routing then concern sections, the bundle's only router.
- Every `references/` file is whole on one subject, one hop from the root, and holds create-grade doctrine an author executes — not narration, not table-stakes, not a stored artifact. Weak, confusing, or low-value reference content is dropped or rebuilt; a new reference is minted only when a real cohesive concern earns it, never to pad.
- Templates establish every structure an implementation type needs; examples are focused, ultra-advanced, world-class patterns used in conjunction with templates — integrated or dropped in ad-hoc — never raw-markdown illustration, never cosmetic toys, never an `examples/` folder that duplicates what a template already owns.
- The description is a load-bearing trigger surface: third person, deliverable-first, discriminating objects and verbs plus the phrasings a request arrives wearing, a boundary naming the sibling it refuses. The listing budget is raised globally, so the concern is trigger precision, never character-count.

Acceptance is graded, not asserted: an independent agent scores the skill's OUTPUT against both modern/advanced external standards and the skill's own stated guidelines, and the before/after run shows a real lift. A skill is done when the cold-load re-eval converges to world-class with no surviving naivety — reached by refinement and trimming, never by bloat.

## [03]-[DELEGATION_MODEL]

One coherent owner per skill, with delegation strictly bounded to keep the owner's judgment uncontaminated.

- OWNER — one `opus` agent per skill performs ALL improvement, iteration, and edits itself, so the bundle carries one coherent voice and standard.
- RESEARCH and NAVIGATION — the owner delegates to `opus` agents for research, exploration, and navigation ONLY. They return FINDINGS: facts, exact locations, real evidence, verified references to top OSS skill-authoring projects and Anthropic's own skill docs. They NEVER return suggestions, opinions, or design ideas — sub-agent ideation poisons the owner's judgment and is the failure this boundary exists to prevent.
- TESTING — the owner runs the skill before and after: a naive agent loads the bundle cold and produces the skill's deliverable, and the owner observes adherence and output quality. For cost-sensitive skills whose concern IS delegation or model economics, test with CHEAP agents (sonnet), never expensive ones — the test proves the pattern, not the model.
- GRADING — a SEPARATE grader agent scores each output against modern/advanced standards and the skill's own guidelines. The owner never grades its own output.
- Iterate the owner→research→test→grade loop until the skill is 100% refined. Every round trims, consolidates, or extends with justified depth; no round adds bloat or spam.

## [04]-[PROCESS_PER_SKILL]

The proven sequence (validated on `hooks-builder`). Run it per skill, one skill fully to seal before the next.

1. RESEARCH (opus, findings-only). Deep-research the real top OSS skill-creators, writers, and builders in this skill's domain, plus Anthropic's authoritative skill-authoring docs (via the `claudeCodeDocs` MCP) and the domain's own current sources (via `context7`/`exa`/`tavily`). Navigate the current bundle. Return findings, never suggestions.
2. ASSESS. Read the whole bundle. Classify every `SKILL.md` section as root-keeper, reference-candidate, or delete; every reference as keep, consolidate, rebuild, or drop; every template and example against the template-vs-example law. VERIFY every candidate defect against the actual rules before acting — over-eager flags waste effort (`[NN]-[PRIMARY]-[EXTRA]` qualifier chains are legal; a single-file-kind bare routing list needs no group label; a genuinely ordered pipeline is legitimately numbered).
3. TRIM and CLEAN. Zero no-op words and phrasing. DROP all validation-checkbox-style content anywhere it appears — it is a remnant of old, weak skills and belongs to no bundle. Drop, combine, or rebuild weak reference files on truthful assessment.
4. STRUCTURE. Fix the `SKILL.md` to router-plus-common-path discipline. Fix the skill↔reference relation: one hop, each reference consolidated on one subject, the set mutually non-duplicating and implicitly holding each other's standards per the page-craft model. Make templates own every implementation-type structure and examples own the ultra-advanced composition patterns; delete any unearned `examples/` weight.
5. HARDEN and EXTEND. From the real work and the research, push world-class density, richness, and power — new depth lands as a case, row, pattern, or reference on the owning surface, never as flat additive spam.
6. CRITIQUE and REDTEAM. An adversarial pass over both the bundle and the output it produces.
7. BATTERY TEST. Cold-load the finished bundle, produce the deliverable, have the grader score it against standards and the skill's own law, compare before/after, and iterate to convergence.

Every touched markdown passes the docgen prose gate before seal; bundled `.py` passes `ruff`/`ty`/`mypy` with zero findings and leaves no orphaned process.

## [05]-[UNIVERSAL_LAWS]

- Template versus example: templates establish the full structure of each implementation type; examples are focused ultra-advanced patterns composed in conjunction with templates. An example that only illustrates, or an `examples/` folder that restates a template, is deleted.
- Reference cohesion: each reference is whole on one subject and one hop from the root; there is no `README.md` or secondary index in a bundle; no reference routes onward to a third file.
- docgen adherence: load `docgen`, pass its gate. The `[VALUE]:` rule — a `[VALUE]:` label with a blank-line gap reads as a list indicator, so a value card drops the gap; a `[VALUE]:` leading into a markdown table KEEPS the gap.
- Zero no-op and zero bloat are absolute. Density, richness, and collapse are the only path to a smaller, stronger bundle; deleting capability to shrink is a defect, and padding to look complete is the opposite defect.
- Provision any new validation tool or CLI through `Parametric_Forge`, never per-repo. Treat archived or prior skill versions as material to mine, never blueprints to copy.

## [06]-[THE_SKILL_SET]

The core set, each with its owning focus. `hooks-builder` is complete and is the exemplar.

- [PULUMI]: high focus and demand. Deep research on real top infrastructure-as-code and provider-authoring skills so the doctrine is not invented. Full critique and redteam pass; proper `SKILL.md` and references.
- [SKILL_WRITER]: the meta skill — it must exemplify its own law flawlessly. High focus and demand, deep research on top skill-authoring skills, full critique and redteam. Carries a surgical pre-fix in [07].
- [MERMAID_DIAGRAMMING]: the visual style is already settled. The work is guidance and adherence — how an agent picks the correct diagram type for a concept, catches logic errors, and builds a sound structure — and replacing the weak validation with real quality checking: drive Chrome headless and the better tooling available to render and inspect the SVG for quality (no crossed edges, no edges over nodes or components, logical layout), not a shallow parse. Hard.
- [INTERVIEWING]: research the real top elicitation and question-driving skills (the grill / grill-me lineage and peers). Push question→answer quality and true intent-understanding. Integrate the skill with `html-studio` — its templates and examples are html-studio deliverables, not raw markdown, and the interaction is valuable to driving decisions, never cosmetic. Hard.
- [HOSTINGER]: extend beyond VPS and SSH to the full Hostinger MCP surface — account and setup management, domains, DNS, email/reach, ecommerce, hosting and WordPress, billing — with guidance, integration, and scripts for the whole capability, not a VPS-only slice.
- [APPLESCRIPT]: raise to the full bar — research top macOS-automation and OSA skills, correct `SKILL.md` and references, templates and examples held to the template-vs-example law.
- [AGENT_DISPATCH]: test with CHEAP agents, never expensive ones, since the skill's concern is delegation economics. Identify the truthfully optimized delegation usage — the right task types for each surface, the correct research/navigation/exploration fan-out — so the delegation this skill teaches produces the repo's world-class, dense, complex code universally and never spam.

Beyond the core, the complex authoring skills `html-studio`, `workflow-creator`, `codex`, `harness-config`, and `github-actions` are second-wave candidates for the same treatment. The thin MCP-wrapper and coding-standard skills need only the light consistency pass unless real use exposes a gap.

## [07]-[SURGICAL_PRE_FIXES]

Land these small, unambiguous fixes directly before the per-skill campaigns.

- `skill-writer`: `examples/repairs.md` is reference material misfiled as an example, exactly as `hooks-builder`'s `fragments.md` was. Move it to `references/repairs.md`, update the `SKILL.md` routing from the `[EXAMPLES]` group to the `[REFERENCES]` group, and delete the now-empty `examples/` directory — `skill-writer` needs no examples.

## [08]-[ACCEPTANCE]

Per skill: a naive cold-load produces world-class output; the independent grader confirms it against both modern standards and the skill's own guidelines; the before/after run shows a real lift; the bundle carries zero no-op and zero bloat; the `SKILL.md` is a correct router and the references are cohesive and one-hop; templates and examples obey the template-vs-example law; every touched file passes its gate. A skill is 100% refined when the re-eval converges with no surviving naivety, reached through iterative refinement and never through accretion.

## [09]-[GUARDRAILS]

- Research agents return findings, never suggestions — the boundary that keeps sub-agent ideation from poisoning the owner.
- The owner is one `opus` per skill and does all edits; the grader is a separate agent; cost-sensitive skills test with cheap agents.
- Verify every defect flag against the real rule before acting; reject the over-eager ones.
- Never bloat to hit the bar. Density, consolidation, and trimming are the only moves that raise it.
