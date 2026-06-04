# [PROMPT] Assay — Final Closeout (docstrings → refinement, no new functionality)

Paste to run the FINAL closeout pass on `tools/assay`. The tool is feature-complete and green; this pass **adds nothing** — it only refactors docstrings, refines/optimizes/cleans, surgically reduces spam/surface/LOC, and hunts hidden bugs/weak logic. **Ultracode**: agents do the work in two strictly-ordered passes (**docstrings FIRST, then code**), each independently reviewed and fixed; the orchestrator holds the value filter and **re-verifies every "green" with a live invocation**. When it's all green, we are done.

## [1] WHERE / WHAT / GATE

- **Tool:** `tools/assay/` — `core/{status,model,engine,routing,aspect}.py`, `composition/{settings,catalog,registry}.py`, `rails/{static,code,test,docs,bridge,package,api}.py`, `automation/{model,engine}.py`, `__init__.py`, `__main__.py`. One Engine, one rail per claim, one `Envelope`, aspect stack at two seams. Read `README.md` (9 invariants + Envelope contract) + `AGENTS.md` (one-touch-per-concern) before editing.
- **Scope of this pass (HARD):** NO new functionality, NO new arms/verbs/flags/shapes, NO behavior changes. Only: (a) docstring/comment refactor, (b) surgical code refinement — optimize, collapse spam, reduce surface/LOC where it preserves capability, fix hidden bugs/fragile logic. Functionality is preserved exactly; density is concept count, not byte count — never delete capability to hit a number.
- **Gate (PIN TO REPO ROOT, COLD CACHE — run after every change):**
  ```bash
  uv run ruff check --no-cache tools/assay && uv run ruff format --check tools/assay
  uv run ty check --python-platform all tools/assay
  uv run mypy --strict --explicit-package-bases tools/assay
  uv run python -m tools.assay self-test
  ```
  Then a REAL runtime invocation of every arm touched (D72: static-green is a hypothesis until a live call — a `TYPE_CHECKING`-stranded type in a `@checked` signature passes all gates and crashes every invocation).

## [2] PASS 1 — DOCSTRING + COMMENT REFACTOR (FIRST, surgical, docstrings-only)

The current docstrings are over-dense, multi-paragraph, backtick-heavy prose — replace them with **proper modern Google style** (consistent with `pyproject` ruff `pydocstyle convention="google"`, line-length 150, `D100/D104/D105/D107` ignored). This pass touches ONLY docstrings/comments; zero code/behavior change; gates stay green.

- **Module/file headers:** ONE focused line for most files. A genuinely complex module (the Engine, the aspect weave, routing) may carry a SHORT additional paragraph — only when it earns it. Dense, real, copy-ready paths; no staging/`_TMP` language.
- **Public functions/classes:** ONE well-made focused line for the simple majority. A **full structured Google docstring (Args/Returns/Raises)** ONLY where the function is genuinely complex and the structure adds real signal (e.g. the lease seam, `_guarded`, the polymorphic `api` dispatch, `fold`). Default to one line; escalate only when justified.
- **Private (`_`-prefixed) functions:** STRIP the docstring entirely. Reserve a SHORT inline comment only for a non-obvious "why"/invariant/boundary exception — never a "what" comment restating the code. (VERIFIED: ruff D-rules fire on PUBLIC functions only — pydocstyle treats `_`-prefixed as private — so stripping a private def's docstring keeps `ruff check --select D` green. A claim that "D103 requires a docstring on every function in assay" is FALSE; confirmed by stripping a private docstring and re-running the gate clean. Public defs DO require their 1-line docstring.)
- **Comments:** keep only non-obvious why, load-bearing invariants, and marked boundary exceptions. Delete what-comments and restated-signature prose.
- This is the FIRST pass and a SEPARATE commit boundary: prove it is docstring/comment-only (`git diff` shows no logic lines moved) and gates stay green before any code change.

## [3] PASS 2 — CODE CLOSEOUT (SECOND: hidden-bug hunt + surgical refinement)

Run a deep, line-by-line workflow (parallel readers per module → synthesis → implement → independent review → independent fix → cold-green). **Adversarially verify EVERY finding before it lands — no speculation; past audits produced false "P0 bugs."** Two intertwined goals:

- **Hidden-bug / weak-logic hunt (the part static checking CANNOT catch):** reason through each body's logic for dead-ends, off-by-one/boundary errors, fragile assumptions, latent races, sentinel/`None` mishandling, exhaustiveness gaps a `match` silently swallows, resource/lease/fd edge cases, codec/boundary seams that can raise un-caught, ordering/timing assumptions, any path that returns a silently-wrong answer rather than a typed `Fault`/`EMPTY`. Confirm each with a concrete reasoned trace or a live repro — then fix it INHERENTLY (typed degradation, never a flag). This is logic-level scrutiny, not a gate re-run.
- **Surgical refinement / spam reduction / optimization:** collapse residual surface into denser polymorphic shapes (merge mergeable pairs, fold single-use helpers into callers, unify near-duplicate factories, flatten cascading/nested types/constants/strings), reduce stringiness, root-shrink `type:ignore`/`# noqa` clusters to the irreducible minimum (document each survivor), and speed up hot paths with the fastest correct ecosystem primitive — all WITHOUT changing behavior or capability. Report before/after concept + LOC counts.

## [4] DOCTRINE (every change is judged against this)

Singular advanced polymorphism — one shape per concept; unified rails; no proliferation of strings/types/constants/literals/factories; no rat-nesting (type-of-type, struct-in-struct, constant-of-constant). Algorithmic/programmatic bodies — `match`/folds/comprehensions/`expression` Result rails; ZERO `if`/`for`/`while`/`try` in domain logic (`try` only at a marked seam). Deep external-lib leverage at full power (verify against `.venv` source, never training data). No fragile logic, no hardcoding, no project coupling, no dead vestiges, no single-call thin helpers. Inherent resilience — a command cannot be mis-invoked; bad input degrades typed, never a traceback. Output: one `Envelope` per invocation, `_emit` the sole stdout writer, never truncate silently. NONE of this adds agent-facing surface.

## [5] CONSTRAINTS

Gates green from the repo root (cold cache) after every change + a live invocation of every touched arm. Lint wall holds (no stdlib `json`/`asyncio`/`logging`/`abc`/env-reads/`cast`/`Any`/`typing.Optional`-for-fallibility). Functional/ROP throughout. `Fault` stays `{argv, status, message}`. Pass 1 (docstrings) is committed separately from Pass 2 (code) so each diff is provable. No new functionality/arms/verbs/flags/shapes; no behavior change; no capability removed. Adversarially verify every bug/refactor — kill false positives with a written reason. Persist locked decisions + rejections + the bug/refactor ledger to project memory.

## [6] START HERE

1. Step 0: exhaustive line-by-line read of every `tools/assay` module + `README.md`/`AGENTS.md` + `pyproject` docstring/lint config; confirm cold-green baseline.
2. Pass 1 ([2]): the docstring/comment refactor workflow → prove docstring-only + cold-green → commit.
3. Pass 2 ([3]): the hidden-bug + surgical-refinement workflow → adversarially-verified fixes + collapses, independently reviewed and fixed → cold-green + live → commit.
4. Final gate cold + a live invocation of every claim/verb; persist the ledger to memory. When green, we are done.
