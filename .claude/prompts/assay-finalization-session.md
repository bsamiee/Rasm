# [PROMPT] Assay (keychain) — Next Finalization Session

Paste to start the next session on the `assay` rebuild. Methodology baseline: `prompts/tool-rebuild.md` (read it first). **Ultracode**: author and run a workflow per phase; correctness/exhaustiveness over cost; orchestrator holds architectural authority and the value filter.

---

## [1] WHERE / WHAT / STATE

- **Tool:** `tools/assay/` — the "keychain" **agent-first nerve-center** (claims `static`/`test`/`bridge`/`package`/`api`/`docs` across **C#, Python, TypeScript, Bash, SQL**, plus an `automation` arm and inherent remote-exec + auto-observability). It supersedes `tools/quality/` (3,897 LOC, C#-only, 9 files) **and** the `package.json` quality scripts (`check:py*`, `check:ts`, `test:cs`, `verify:rhino`).
- **Design corpus:** `tools/assay/design/` — 18 per-module docs + `ARCHITECTURE.md` (the keystone; **decision ledger D1–D67** in §11, **parity matrix** §12, density record §16), `README.md`, `AGENTS.md`.
- **Validated impl (the reference):** `tools/assay/_TMP/` — 18 modules (~6,400 LOC), a self-contained absolute-import package `tools.assay._TMP.*`. **Passes, from the repo root:** `ruff check`(select=ALL, py314), `ruff format --check`, `ty check`, `mypy --strict`, `py_compile`, and `import tools.assay._TMP.__main__`.
- **Deps already added** (`pyproject.toml`, `>=` floors): `psutil`, `fsspec`, `watchfiles`, `aiocron`, `universal-pathlib` (UPath = the path type), `asyncssh` (transparent remote-exec).
- **Phase:** this is the **pre-promotion finalization** phase. Keep working in `_TMP/`; keep `design/` ↔ `_TMP/` synchronized; do **not** promote to real module paths until the work below is done. Promotion later = mechanical scrub of `._TMP` + move.

**Run commands (repo root):** ours → `uv run python -m tools.assay._TMP <claim> <verb> [args]`; original → `uv run python -m tools.quality <verb> [args]`. **Pin every gate and run to the repo root** — an agent that runs `mypy`/the tool from the wrong cwd reports phantom errors; the toolchain/CPython from repo root is the only ground truth.

## [2] METHODOLOGY FOR THIS SESSION

- **You (orchestrator) hold authority.** Agents research/implement; you synthesize, apply the value filter, and **reject bloat/grab-bag**. No new shape for an existing concept — extend via a tagged-union variant / enum member / data row.
- **Verify everything yourself.** Library APIs against installed source under `.venv/.../site-packages` (never training data); gate results by running the toolchain yourself; load-bearing claims by a **real runtime invocation**, not reasoning.
- **Sync the corpus.** Every `_TMP` change is mirrored into its `design/` doc; reconcile `ARCHITECTURE.md` (ledger amendments, honest measured density).

## [3] PHASE 1 — REFINEMENT + CAPABILITY (do this first)

Deepen the tool **within existing folders**; gate-validated; folded into design docs.

- **Ultra-advanced Python / typings / integrations.** Remove indirection and shrink the residual `type:ignore`/`# noqa` clusters at their root (e.g. evaluate a generic `Bind[P]` to retire `_narrow`; tighten the `_spawn` `Spawn`↔`Hom` compose re-scope; the fcntl platform-conditional). Adopt the deferred lib-depth wins where they genuinely densify: `msgspec` `enc_hook`/`dec_hook`/`schema`, `expression` `map2`/`Block.collect`/`partition`, `beartype.vale.Is[]` validators, `opentelemetry` span Links, `anyio` memory-object-streams / `TaskGroup.start`, deeper `pydantic` `computed_field`/validators. Each must reduce hand-rolling or add real capability — not ceremony.
- **Improve functionality / add capability in existing domains.** Sharper parsers (richer `Match`/`Detail` without new shapes), deeper routing/engine robustness, fuller catalog parity, better automation triggers/actions.
- **New external libs.** Identify genuinely high-value additions (LOC reduction or new capability) — be skeptical, reject grab-bag. Add to `pyproject.toml` with floors, configured in-manifest.
- **New arms (NOT secrets — better ideas).** Judge for high-value + clean ripple + reuse of the unified Engine/Report/Envelope/aspect. Strongest candidate: a **code-intelligence arm** (structural search + codemod via `ast-grep`/tree-sitter as a `Claim`) — lets agents do structural refactors they'd otherwise hand-roll fragilely. Also weigh: an **ephemeral-env / sandbox** arm (uv-driven isolated execution for safe agent runs), a **closure/graph introspection** arm. Each new arm slots as a `Claim` + handlers + catalog/registry rows, inherent and auto-integrated, no flag spam.

## [4] PHASE 2 — AGGRESSIVE REAL A/B + ISOLATION TESTING (after Phase 1)

Everything here is **real usage, not static tests.** Goal: prove our tool returns correct information in every original capacity, is resilient/robust (no fragile logic, no unusual hardcoding), and is properly agent-usable.

1. **Understand the rebuild driver.** Read `tools/quality/` (`README.md`, `__main__.py`, `process.py`, `rails/*`) and confirm against `ARCHITECTURE.md` §1 what the old tool HAD and **why it became untenable** (C#-only reach; ~25 single-use `Literal` aliases; 14 report structs; three model systems; eight-callable projector ceremony with a `data`/`evidence` ladder; shared `.artifacts` with no polyglot fan-out; inconsistent exit codes). This is the "why" our design answers — verify no misunderstanding.
2. **Enumerate ALL old-tool outputs at RUNTIME.** For every `tools.quality` verb + sub-verb, run it (and `--help`) and record the exact Envelope fields, output shapes, exit codes, status set. Map each to OURS via the §12 parity matrix — now **runtime-verified**, not on paper. Flag any field/shape/exit we changed and confirm it's intentional + documented.
3. **Run A vs B in every original capacity** (same inputs, compare outputs):
   - `static report|build|fix|full|plan`, `test run|list|coverage`, `docs check`, `api doctor|resolve|query|show`.
   - **API is the critical one:** verify decompile/search of **local installed tooling/packages/versions** works for **all five languages** — an agent must be able to resolve/query/show API and source info without issues, and on a miss get the **richer-on-failure** feedback (`ApiResolution` candidates+scores+reason), not a bare empty result.
   - **Host-gated capacities** (`bridge` = live RhinoWIP, `package` = yak) need the real host — run them where the host is available; otherwise verify structurally + via the in-process paths. Note explicitly which were host-gated vs run.
4. **Run OURS in isolation across ALL functionality** — exercise every claim/verb/language for real, including the automation arm (a watch/schedule fire), remote-exec (one live `ASSAY_EXEC_TARGET=ssh://…` smoke test if a host is reachable — the one claim still only locally-verified), and the auto-observability path (force a failure, confirm the single Envelope's `error_context` carries the distilled diagnostic). Confirm: correct info return, all sources work, no crash on environment (point-and-go from any cwd / CI), no fragile/hardcoded behavior.
5. **Optimize for agents.** Reduce flag counts and surface complexity — auto-integrate wherever possible, **no flag spam**, prefer config/context over knobs. Every verb should be invokable with minimal/zero flags and self-describe on failure.
6. **Iterate.** Keep `design/` ↔ `_TMP/` synced; re-run A-vs-B then isolation until the tool is provably real, correct, and agent-resilient. Re-gate from repo root after every change.

## [5] CONSTRAINTS

- Stay in `_TMP/` (pre-promotion); gates green from repo root after every change; lint wall holds (no stdlib `json`/`asyncio`/`logging`/env-reads/`abc`); functional/ROP (`match`/no `if`/`for`/`while`/`try` in domain logic); `Fault` stays `{argv,status,message}` (D28); no new shape for an existing concept; reject bloat/grab-bag with a reason.

## [6] START HERE

1. Read `tools/assay/ARCHITECTURE.md` §11 (ledger D1–D67), §12 (parity), §16 (density), then skim the 18 `design/` docs and `AGENTS.md`.
2. Confirm the green baseline from repo root (`ruff check` / `ty check` / `mypy --explicit-package-bases tools/assay/_TMP` / `import tools.assay._TMP.__main__`).
3. Run Phase 1 (refinement/capability waves), then Phase 2 (real A/B + isolation). Persist the locked decisions, rejections (with reasons), and any A/B output deltas to project memory so they aren't re-chased.
