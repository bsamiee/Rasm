# [PROMPT] Tool — Final Closeout

Use this for the final closeout pass on `<target-tool>`. The tool is feature-complete; this pass adds no functionality. It refactors docstrings, removes low-value comments, reduces spam and surface area, and fixes hidden fragile logic while preserving behavior.

## [1] WHERE / WHAT

- **Tool:** `<target-tool>` — one engine, one rail per claim, one machine envelope, and an aspect stack at the execution seams.
- **Scope:** no new functionality, arms, verbs, flags, shapes, or behavior changes. Only docstring/comment cleanup, surgical refinement, optimization, duplication collapse, and hidden-bug fixes.

## [2] PASS 1 — DOCSTRINGS AND COMMENTS

Replace over-dense docstrings with focused Google-style prose:

- Most module headers: one focused line.
- Complex modules: one short orienting paragraph only when it adds signal.
- Public functions/classes: one focused line by default; structured `Args`/`Returns`/`Raises` only where the shape is genuinely complex.
- Private functions: remove docstrings unless the invariant is not obvious from the body.
- Comments: keep only non-obvious why, invariants, and boundary exceptions.

This pass should touch documentation/comment text only.

## [3] PASS 2 — CODE CLOSEOUT

Run a deep logic pass over every body:

- Find dead ends, off-by-one errors, fragile assumptions, latent races, sentinel/absence mistakes, exhaustiveness gaps, resource leaks, codec/spawn/file-system failures, and paths that return silent wrong answers.
- Collapse residual surface into denser polymorphic shapes: merge mergeable pairs, fold single-use helpers, unify near-duplicate factories, flatten type/string constant chains, and root-shrink suppressions.
- Speed up hot paths with the fastest correct primitive already available to the project.

Every change preserves capability and behavior unless it fixes a proven bug.

## [4] DOCTRINE

Singular advanced polymorphism: one shape per concept, unified rails, no parallel factories, no dead surface. Algorithmic bodies use match/folds/comprehensions/result rails where they fit. Resilience is inherent: bad input degrades to typed output, not a traceback. Output remains one `Envelope` per invocation, with `_emit` as the only stdout writer.

## [5] START HERE

1. Pass 1: docstring/comment refactor only.
2. Pass 2: hidden-bug hunt and surgical refinement.
3. Report the capability-preserving collapses, fixed bugs, and remaining intentional constraints.
