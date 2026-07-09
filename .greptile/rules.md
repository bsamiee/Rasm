# Review context — Rasm

C#/.NET-primary monorepo with TypeScript and Python lanes. Language doctrine is codified under docs/stacks/{csharp,typescript,python} — review against those documents, not generic community convention. Where a finding and the doctrine disagree, the doctrine wins.

## Design paradigms

- Functional programming exclusively; monadic railway-oriented flows unify error handling — never dual paradigms in one surface.
- The unit of design is the polymorphic dispatch surface, not the file. Fewer deep surfaces beat many shallow ones; a new capability is a case, row, or dispatch arm on the owning surface.
- Boundary kernels may use language-native control flow only where it preserves correctness, typing, performance, or interop clarity; domain logic never does.
- Central manifests own versions: Directory.Packages.props for NuGet, per-package package.json + committed pnpm lockfile for TS, pyproject.toml + uv for Python.
- Topology: strata depend only upward; geometry, meshing, and IFC each have exactly one owner per runtime; branches couple only through wire contracts and companion/offline seams.
- Planning pages under libs/.planning are the product: fences are transcription-complete implementation, verified against .api/ catalogues, held naive until they survive adversarial reading.

## Universal bar

Anticipate 10x functionality growth: surfaces absorb new modalities as rows, cases, or dispatch arms — never as new files, flags, or knobs. Defects: knob/param/flag spam, hardcoded values, fragile string plumbing, naive happy-path logic, hand-rolled reimplementations of capability the ecosystem already provides. External packages are first-class implementation material at full power, newest stable versions. Everything ships agent-first: composable, receipt-bearing, self-describing. Collapse spam relentlessly.

## Review priorities

1. Doctrine regressions (rails, dispatch, package custody) outrank style and naming.
2. New public surfaces demand justification against extending an existing owner.
3. Generated or lock content is never review substrate.

## Load-bearing exceptions

Code that violates generic best practice on purpose — do not flag:

- Aggressive API breaks with every call site updated in the same change are the sanctioned rename path, not regressions.
- Dense single-expression bodies and heavy polymorphic dispatch are the bar, not obfuscation.
- Absent defensive guards inside domain logic reflect admission-once boundaries, not missing error handling.
- Sparse 1-2 line agent-facing comments are compliance with comment law, not missing documentation.
- Fences in .planning pages are implementation, not documentation examples — hold them to source standards, never suggest simplifying them into sketches.
- A large file that owns one full concern is sanctioned; never recommend splitting by size.

## Durable prose and skill detection

Durable markdown — docs, standards, skills, prompts — is agent-facing law. Flag:

- No-op intensifiers: quality adjectives (careful, high-quality, robust, thorough) in a sentence with no owner, action, trigger, or gate.
- Filler lead-ins: "it is important to note", "note that", "make sure to", "be sure to", "remember to", "keep in mind".
- Restated harness obligations: telling an agent to follow CLAUDE.md/AGENTS.md, use available tools, or obey system instructions.
- Quality ladders (good/better/best, minimum/ideal) where a contract gate belongs.
- Command catalogs with no task trigger or acceptance signal per row.
- Generic lifecycle sequences (think, plan, implement, validate, summarize) and mandated reasoning shapes.
- Closing checklists with no machine-checkable gate.
- Process ledgers: ship-status markers, decision tags, freshness stamps, session narration in durable prose.
- Meta-commentary: sentences whose subject is the document itself (this skill, this file, this section) outside routing rows.
- Defensive caveats: hedges (may, might, generally, usually, when possible) softening settled rules; contract qualifiers (optional, if present, where supported, unless) survive.
- Bare abstractions: three or more abstract guidance bullets with no paired rejected/accepted example, template, or gate.
- Fixed output skeletons: one mandated report shape (summary, findings, recommendations, next steps) regardless of consumer.
- Skill bundles (.claude/skills/**): first/second-person frontmatter descriptions — quoted user-utterance trigger phrases are not voice; over-broad or keyword-stuffed trigger descriptions; SKILL.md over 500 lines or carrying reference banks inline; references that only route to other references; deterministic multi-step procedures narrated in prose where a bundled script belongs; instructed network fetches or global installs inside skill bodies, except an owned install surface naming exact source, scope, and verification.
