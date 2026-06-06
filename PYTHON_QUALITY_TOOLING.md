# [PYTHON_QUALITY_TOOLING]

Look up what Rasm's Python quality gate runs, where each rule lives, what each tool can and cannot enforce, and how to add policy without duplicating rails. This is a reference leaf: a working contributor extracts one fact per scan. The two embedded procedures (decode the `pnpm check:py` gate; add a policy rule) and the doctrine explanation (why no type checker enforces `coding-python` style) carry cross-links in Boundaries.

This file states what CI runs and where each rule lives. Doctrine procedure lives in `.claude/skills/coding-python/SKILL.md`.

Audience: contributors changing Python policy, `pyproject.toml`, `.rules/python/`, or `tools/py_analyzer`.

Scope: the `pnpm check:py` Python gate and its rule sources. Out of scope: C# `tools/cs-analyzer` and the `uv run python -m tools.quality` static/test/bridge rails, which the quality operator and `CLAUDE.md` §5.2 own.

Posture: advanced/modern, not a generic linter introduction. The gate targets PEP 649/695, ty beta, Ruff `ALL`+`preview`, and a LibCST semantic analyzer.

Python surface today is roughly 39 `.py` files, mostly `tools/quality`, `tools/py_analyzer`, `.claude/**/scripts`, and tests. `py_analyzer` path rules apply under `**/domain/**` and `**/application/**` when those trees exist; current repo Python is mostly tooling/neutral scope.

## Source of truth

The gate, rule files, and pins are repository truth; this prose summarizes the facts a local reader looks up and links the controlling source for the rest.

Source of truth: `package.json` `scripts.check:py`; `pyproject.toml`; `sgconfig.yml`; `.rules/**/*.yml`; `tools/py_analyzer/**`; `uv.lock`.
Last verified: 2026-06-03 against repo.
Review trigger: any change to `check:py`, `pyproject.toml`, `.rules/**`, the analyzer, or a pinned tool version.

## Pinned toolchain

Local pins govern every claim about tool behavior below; a version bump can invalidate a rule-name or capability fact, so verify against the manifest after any bump.

Ruff: 0.15.12
ty: 0.0.34
mypy: 1.20.2
ast-grep: 0.42.1
Python: 3.14 (`requires-python >=3.14`)
validate-pyproject: 0.25
pydantic: 2.13.3

Evidence: `uv.lock`; `pnpm-workspace.yaml` (`@ast-grep/cli` 0.42.1); `pyproject.toml` `requires-python`.
Last verified: 2026-06-03.
Review trigger: `uv.lock` or `pnpm-workspace.yaml` version change.

## Enforcement split: which engine owns what

Rasm splits enforcement across engines because no single tool covers the surface. Neither ty nor mypy is a doctrine engine: ty has no plugins; mypy plugins are type-centric (`pydantic.mypy`). They prove assignability, protocols, and narrowing — not expression-first domain design, `pipe` composition, decorator order, path-scoped imperative bans, `Result`/`Option` rails, frozen-model policy, or cross-module duplicate shapes.

The split assigns each enforcement concern to its owning engine.

| Engine | Owns |
|---|---|
| Ruff + `banned-api` | Mechanical: style, imports, security, modernization, complexity, import/API bans |
| ty (primary) + mypy (secondary) | Static types; mypy adds Pydantic and `exhaustive-match` |
| ast-grep | Syntax fingerprints (YAML rules + fixtures) |
| `py_analyzer` (LibCST) | Path-scoped semantics, `PYS*` rules, cross-file facts |
| Hypothesis + `@beartype` | Algebraic laws; CLI runtime call-site shapes |
| Review | Undecidable style: `pipe`, decorator stacks |

Objective categories the split covers: lexical/format; import/banned API; annotation presence vs correctness; static types; type-level control-flow; bandit-class security; complexity; syntax policy (ast-grep); path-scoped semantics (LibCST); packaging; runtime tests; property laws; coverage; boundary runtime types.

## Canonical gate: `pnpm check:py`

`pnpm check:py` is the authoritative Python gate, a single `&&`-chained shell line decoded below. The steps run in order; mechanical layout/imports go to Ruff, assignability to ty (primary) then mypy (secondary, Pydantic), repo semantics to `py_analyzer`, and syntax fingerprints to ast-grep plus fixtures.

| Step | Command / tool | Scope / notes |
|---|---|---|
| −1 | `export UV_CACHE_DIR`, `HYPOTHESIS_STORAGE_DIRECTORY` | `.cache/uv`, `.cache/hypothesis` |
| 0 | `uv run validate-pyproject pyproject.toml` | PEP 621 / build metadata (`validate-pyproject[all]` dev dep) |
| 1 | `ruff format --check $PY_FILES` | See `PY_FILES` below |
| 2 | `ruff check $PY_FILES` | Same; includes `tests/tools/ast-grep/fail/**` (ty/mypy exclude only those) |
| 3 | `ty check` | `tools/py_analyzer`, `tests/tools/py_analyzer`, `tools/quality`, `tests/tools/quality`, `.claude/hooks`, `.claude/skills/*/scripts` |
| 4 | `mypy --explicit-package-bases` | Four package trees only (no `.claude/**`) |
| 5 | `fd` + per-file `mypy` | All repo `.py` (`fd -H -e py . .claude/hooks .claude/skills/*/scripts`); `MYPYPATH=$(dirname file)` — overlaps step 4 on package trees plus e.g. `tests/conftest.py` |
| 6 | `pnpm exec ast-grep scan` `^no-` | Excludes `tests/tools/ast-grep/**`, `.artifacts`, `.claude`, `.cache`, `.git`, `.nx`, `.venv`, `coverage`, `node_modules`, `test-results`, `tmp` |
| 7 | `fd` + `rg` + `!` | Ban helper-style filenames (excludes `.claude`, `tests/tools/ast-grep`, caches — same family as step 6) |
| 8–9 | ast-grep `pass` / inverted `fail` | Fixture contract |
| 10 | `python -m tools.py_analyzer check --root .` | LibCST semantic scan |
| 11 | `pytest` | `tests/tools/py_analyzer`, `tests/tools/quality` only |

`PY_FILES` resolves to `fd -H -e py .` plus named roots — effectively all repository `.py` files, including `tests/conftest.py` and ast-grep fixtures, not only the six named directories.

Outside this gate: `beartype` on quality CLI entrypoints; `pnpm check:py:coverage` (`fail-under=100` on `tools/quality` only); `pnpm check:py:mutation`; full `tests/` pytest; the `uv run python -m tools.quality` C# rails. `tests/conftest.py` is in Ruff `PY_FILES` and the mypy step 5 per-file pass, but not in the ty step 3 roots.

Evidence: `package.json` `scripts.check:py` (single `&&`-chained line).
Review trigger: any edit to the `check:py` script.

## ast-grep discovery: `sgconfig.yml`

`sgconfig.yml` at the repository root defines rule-discovery paths and TS extension mapping only — it declares no rules.

```yaml template
# copy-safe — sgconfig.yml; discovery config, declares no rules
ruleDirs:
  - .rules/python
  - .rules/typescript

languageGlobs:
  tsx:
    - "**/*.ts"
    - "**/*.tsx"
    - "**/*.mts"
    - "**/*.cts"
```

| Field | Role |
|---|---|
| `ruleDirs` | Directories scanned for YAML rules (`*.yml`). |
| `languageGlobs.tsx` | Maps TS/TSX/MTS/CTS to the ast-grep `Tsx` engine (one rule pack for all TS dialects). |

Integration: `pnpm exec ast-grep scan --config sgconfig.yml` runs in `check:py` (`--filter '^no-'`) and `check:ts` (`--filter '^ts-domain-'`). Catalog pin: `@ast-grep/cli` 0.42.1 in `pnpm-workspace.yaml`.

Evidence: `sgconfig.yml`; `package.json` `check:py`/`check:ts`.

## ast-grep structural rules: `.rules/`

`.rules/**/*.yml` holds ast-grep structural policy, split by language and filtered by rule-ID prefix in CI.

Python rules in `.rules/python/` — 3 rules, CI filter `^no-`, main scan excludes `.claude/**`:

| Rule ID | File | Enforces |
|---|---|---|
| `no-direct-annotations` | `no-direct-annotations.yml` | Ban `obj.__annotations__`, `__dict__["__annotations__"]`, `.get("__annotations__")`. Prefer `annotationlib.get_annotations()` / `inspect.get_annotations()` (PEP 649/749). |
| `no-typing-no-type-check` | `no-typing-no-type-check.yml` | Ban `no_type_check` import/usage (`typing`, `typing_extensions`). |
| `no-helper-import` | `no-helper-import.yml` | Ban top-level `helpers`, `helper`, `utils`, `util` imports. |

TypeScript rules in `.rules/typescript/` — 2 rules, filter `^ts-domain-`:

| Rule ID | File | Scope (`files:` globs) |
|---|---|---|
| `ts-domain-no-let` | `ts-domain-no-let.yml` | `**/domain/**`, `**/application/**` under apps/libs/tools src; includes `tests/tools/ast-grep/**` |
| `ts-domain-no-statement-flow` | `ts-domain-no-statement-flow.yml` | Same; bans `if`, loops, `try`, `throw`, etc. |

Not handled by ast-grep for Python: domain `if`/`for`/`while`/`try`/`raise` is `PYS0001` in `tools/py_analyzer` (path-scoped). The TypeScript equivalent is `ts-domain-no-statement-flow`.

Proposed Python additions, not yet in repo: `no-getattr-annotations`, `no-no-type-check-on-def` (kind `decorated_definition`), `no-hasattr-probe`, `no-isinstance-dispatch`, `no-asyncio-unstructured`. Each needs a `.rules/python/*.yml` plus `tests/tools/ast-grep/{pass,fail}/` fixtures.

Evidence: `.rules/python/*.yml`, `.rules/typescript/*.yml`.

## ast-grep fixture contract: `tests/tools/ast-grep/`

`tests/tools/ast-grep/` holds golden fixtures that prove each ast-grep rule fires correctly; it is not production code.

| Directory | Expectation |
|---|---|
| `pass/` | `ast-grep scan --filter '^no-'` (or `^ts-domain-`) produces no errors |
| `fail/` | Scan must match; the shell inverts success (`! ast-grep scan ... fail`) |

Python fixtures (examples):

- `pass/annotations_api.py` — `annotationlib.get_annotations`
- `pass/match_case.py` — allowed `match`/`assert_never`
- `fail/direct_annotations.py` — `__annotations__`
- `fail/helper_import.py` — `from utils import`
- `fail/no_type_check.py` — typing escape

TypeScript fixtures: `pass/expression_flow.*`, `fail/statement_flow.*` (imperative `if` chains).

Fixture exclusions and ownership: `ty` and `mypy` exclude `tests/tools/ast-grep/fail/**`. `IGNORE_FIXTURE_PREFIXES` includes `tests/tools/ast-grep/` and `tests/tools/py_analyzer/`. `check:ts` runs `^ts-domain-` on TS fixtures; `check:py` runs `^no-` only. `fail/statement_flow.*` exercises `ts-domain-no-statement-flow`, and the `.mts` variant also hits `ts-domain-no-let`. `pass/match_case.py` is owned by no ast-grep rule (mypy/py_analyzer doctrine governs it).

## `pyproject.toml` custom enforcement

`pyproject.toml` carries no Ruff plugins, no ty plugins, and no custom mypy modules. "Custom enforcement" is configuration of built-in engines.

Active and valid sections:

| Section | Real? | Role |
|---|---|---|
| `[tool.ruff]` `select = ["ALL"]`, `preview`, `target-version = "py314"` | Yes | Full native rule surface minus `ignore` list |
| `[tool.ruff.lint.flake8-tidy-imports.banned-api]` | Yes (TID251) | 66 import/API bans with custom `.msg` strings |
| `[tool.ruff.lint.flake8-tidy-imports.banned-module-level-imports]` | Yes | Scientific stack (`numpy`, `pandas`, …) quarantined to `scientific` dependency group |
| `[tool.ruff.lint.per-file-ignores]` | Yes | `tools/quality/**` (CLI/TC relaxations), `tests/**`, skill scripts (full TID251 off on `**/.claude/skills/**/scripts/**`) |
| `line-length = 150`, Google pydocstyle | Yes | Active formatting/doc conventions |
| `[tool.ty.rules]` `all = "error"`; `[tool.ty.analysis]` `respect-type-ignore-comments = false` | Yes | Primary type gate; the parity overrides below are recommended, not yet applied |
| `[tool.mypy]` `strict`, `plugins = ["pydantic.mypy"]`, `enable_error_code = [...]` | Yes | Secondary types + Pydantic |
| `[tool.pytest]` | Yes | Strict pytest, socket lockdown, 30s timeout; `required_plugins` includes `pytest-cov` |
| `[tool.coverage.*]` | Yes | `source = ["tools/quality"]`, `branch = true`, `fail_under = 100` — runs in `pnpm check:py:coverage`, not `check:py` |

Not in `pyproject.toml`:

- ast-grep rules → `.rules/**/*.yml` + `sgconfig.yml`
- semantic doctrine → `tools/py_analyzer` (`PYS0001`–`PYS0010`)
- helper filenames → shell in `check:py`

Evidence: `pyproject.toml`.
Review trigger: any `[tool.ruff]`, `[tool.ty]`, `[tool.mypy]`, `[tool.pytest]`, or `[tool.coverage]` edit.

### `banned-api` policy-as-data

Ruff TID251 matches two shapes, with one documented bypass.

- Import statements: exact module or parent prefix. `"asyncio"` bans `import asyncio`, but not necessarily `asyncio.run` unless that member is listed.
- Attribute access: exact qualified name, e.g. `"json.loads"`.

Bypass: `importlib`, aliases, and re-exports evade TID251 — a documented Ruff limitation. Complement with member-level keys or `py_analyzer`.

Evidence: `pyproject.toml` `[tool.ruff.lint.flake8-tidy-imports.banned-api]`; Ruff `flake8-tidy-imports` docs at 0.15.12.
Last verified: 2026-06-03.
Review trigger: Ruff version bump or `banned-api` table edit.

### `[tool.ty.rules]` parity with mypy `enable_error_code`

ty 0.0.34 has no global `preview` or `experimental` switch: preview rules are ordinary rule names with default severities. Set per-rule overrides, or rely on `all = "error"` plus `[tool.ty.terminal] error-on-warning = true`.

This table maps each mypy `enable_error_code` to its ty 0.0.34 equivalent.

| mypy `enable_error_code` | ty 0.0.34 rule | Notes |
|---|---|---|
| `unused-awaitable` | `unused-awaitable = "error"` | Preview; default `warn` |
| `explicit-override` | `invalid-explicit-override` | Already `error` via `all`; flags spurious `@override` |
| `explicit-override` (strict) | `missing-override-decorator` | Unknown in 0.0.34; lands in ty ≥ 0.0.41 (preview, default `ignore`) |
| `ignore-without-code` | `ignore-comment-unknown-rule`, `unused-ignore-comment` | Pair with `respect-type-ignore-comments = false` → suppressions must be `ty: ignore[rule]`; bare `type: ignore` does not silence ty |
| `exhaustive-match` | — | Keep on mypy; ty has `invalid-match-pattern` (syntax only), not exhaustiveness |
| `redundant-self` | — | mypy only |
| `truthy-bool`, `truthy-iterable` | — | mypy only; not `unsupported-bool-conversion` (runtime `__bool__` failures) |

Recommended `[tool.ty.rules]` overrides — merge into `pyproject.toml`. `[tool.ty.environment]`, `[tool.ty.analysis]`, and `[tool.ty.src]` already match; bump ty before enabling `missing-override-decorator`.

```toml template
# conceptual — recommended ty config; not yet applied to pyproject.toml
[tool.ty.environment]
python-version = "3.14"
python-platform = "all"

[tool.ty.analysis]
respect-type-ignore-comments = false

[tool.ty.src]
exclude = [
    ".artifacts", ".cache", ".claude/skills/**/.cache", ".claude/worktrees",
    ".nx", ".venv", "coverage", "node_modules", "test-results",
    "tests/tools/ast-grep/fail/**", "tmp",
]

[tool.ty.rules]
all = "error"
unused-awaitable = "error"
ignore-comment-unknown-rule = "error"
unused-ignore-comment = "error"
unused-type-ignore-comment = "warn"
possibly-missing-implicit-call = "error"
undefined-reveal = "error"
# missing-override-decorator = "error"  # ty >= 0.0.41; maps mypy explicit-override strict arm

[tool.ty.terminal]
error-on-warning = true
```

ty cannot enforce these — keep mypy and `py_analyzer`: domain `if`/`for`/`try`/`raise` (PYS0001–0002), `Result`/`Option` signature rails (PYS0004/0007), frozen/mutable model policy (PYS0008/0009), `@effect.result` recovery (PYS0010), cross-module duplicate models and single-call privates (PYS0005/0006), `pipe`/decorator-order doctrine, and Pydantic field/init semantics (`pydantic.mypy`).

ty 2026 roadmap, Rasm-relevant: stable-milestone native Pydantic ([#2403](https://github.com/astral-sh/ty/issues/2403)); ty-1.1 `missing-override-decorator` ([#155](https://github.com/astral-sh/ty/issues/155) / PR #25111); type-powered lint for always-truthy `if` ([#1073](https://github.com/astral-sh/ty/issues/1073)); PEP 728 `closed=True` TypedDict narrowing ([#154](https://github.com/astral-sh/ty/issues/154)); no plugin system, third-party support in-core only; 0.0.x diagnostic breakage until GA ([#1889](https://github.com/astral-sh/ty/issues/1889)).

Evidence: `pyproject.toml` `[tool.ty.*]`, `[tool.mypy] enable_error_code`; ty issue tracker links above.
Last verified: 2026-06-03.
Review trigger: ty version bump (especially to ≥ 0.0.41) or a linked issue reaching GA.

## `tools/py_analyzer` LibCST semantic gate

`tools/py_analyzer` is the LibCST semantic gate, invoked as `uv run python -m tools.py_analyzer check --root . --format text|json|github`. It owns the path-scoped semantics that ast-grep and the type checkers cannot decide.

### Rules `PYS0000`–`PYS0010`

| ID | Category | Enforces |
|---|---|---|
| PYS0000 | Infrastructure | Parse/read failures |
| PYS0001 | FunctionalDiscipline | No `if`/`for`/`while`/`try`/`raise` in domain/application paths |
| PYS0002 | Governance | Boundary imperative flow requires `# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=<vocab> ticket=... expires=YYYY-MM-DD rationale=...` on the line above; `reason` ∈ `protocol-required`, `cleanup-finally`, `cancellation-guard`, `adapter-normalization` |
| PYS0003 | TypeDiscipline | No bare primitives/`Any`/mutable containers in public domain/application signatures |
| PYS0004 | FunctionalDiscipline | Fallible names / `Optional` returns → `Result`/`Option` |
| PYS0005 | SurfaceArea | Single-call same-module private functions |
| PYS0006 | TypeDiscipline | Duplicate model field shapes (cross-module global fold) |
| PYS0007 | FunctionalDiscipline | No `.unwrap()` / `.value_or()` in domain/application |
| PYS0008 | TypeDiscipline | Models must declare frozen policy |
| PYS0009 | TypeDiscipline | No mutable field annotations on domain models |
| PYS0010 | FunctionalDiscipline | No `.or_else_with` recovery inside `@effect.result` builders |

### Scope classification by path

`classify_scope()` reads path segments with precedence tooling → boundary → domain → application.

- Segment `domain` → domain; segment `application` → application.
- Segments `adapters`, `boundary`, `boundaries` → boundary (e.g. `src/domain/adapters/http.py` is boundary, not domain).
- `tools/py_analyzer`, `tools/quality`, `.claude/hooks`, `.claude/skills/**/scripts` → tooling.
- Else → neutral (no PYS0001).

Discovery excludes `EXCLUDED_DIRS` (`.artifacts`, `.cache`, `.git`, `.nx`, `.venv`, `coverage`, `node_modules`, `test-results`, `tmp`) plus the prefix `tests/tools/ast-grep` (`EXCLUDED_PREFIXES`) — not all of `tests/`. `tests/tools/py_analyzer/**` and `tests/tools/quality/**` are scanned on `--root .` but are tooling/neutral scope. Unit tests also use `tmp_path` plus explicit paths for domain examples.

Allowed in domain (not matched by the PYS0001 visitor): `match`, `with`, `assert`. Only `If|For|While|Try|Raise` are banned.

Proposed PYS0011+ (LibCST): `DomainComprehensionFlow` (`Comprehension`/`GeneratorExp`), `DomainResourceStatement` (`With`), `DomainAssertGuard` (`Assert`).

### Analyzer architecture

A per-module LibCST visitor produces `ModuleFacts`; the global fold runs PYS0006 only, and PYS0005 aggregates per-module private call counts. The visitor uses `PositionProvider` and `ParentNodeProvider`.

Evidence: `tools/py_analyzer/**`; `classify_scope()` source.
Review trigger: analyzer rule, scope, or visitor change.

## Per-tool capability and best owner

Each tool supports a different custom-rule mechanism and owns a different set of categories. Use the capability matrix to know whether a tool accepts custom rules, and the owner matrix to route a new enforcement need to the right engine.

Custom-rule capability per tool:

| Tool | Custom user rules? | Primary domains |
|---|---|---|
| Ruff | No (Rust only); policy via `banned-api`, ignores | Style, imports, security, modernization, complexity |
| ty | No; built-in rules, severity/overrides only | Types, protocols, narrowing, async |
| mypy | Yes (plugins); hooks are type-centric | Types + `pydantic.mypy` |
| ast-grep | Yes (YAML) | Syntax / structure |
| LibCST / py_analyzer | Yes (Python visitor) | Scoped semantics, cross-file facts |
| validate-pyproject[all] | No; schema + formats | PEP 621/735/build-system |
| pytest / Hypothesis | Tests | Runtime behavior, laws |
| beartype | Runtime `@beartype` | Call-site shapes when CLI runs |

Best owner per enforcement category:

| Category | Best owner | Notes |
|---|---|---|
| Formatting / PEP8 layout | Ruff format + Ruff E/W | |
| Import order / TC | Ruff I, TC | `flake8-type-checking.strict = true` |
| Banned library/API | Ruff `banned-api` | Extend before new tools |
| Annotation presence | Ruff ANN | ty proves correctness |
| Type assignability | ty (primary), mypy (secondary) | |
| Pydantic `__init__` / fields | mypy + `pydantic.mypy` | Until ty native Pydantic parity |
| `match` exhaustiveness | mypy `exhaustive-match` + ty narrowing | |
| Syntax fingerprint | ast-grep | Fast, fixture-tested |
| Domain no-`if`/`for`, Result rails | py_analyzer | Not ty/Ruff |
| Cross-module duplicate models | py_analyzer | |
| Packaging metadata | validate-pyproject | First in `check:py` |
| Algebraic laws | Hypothesis | Not linter rules |
| CLI invocation contracts | beartype | Not in CI static gate |

## Objective enforcement taxonomy

This matrix is the decidability map for every enforcement axis: whether each axis is decidable, its scope, whether it runs at runtime, what Rasm enforces today, and the open gap.

| # | Axis | Decidable? | Scope | Runtime? | Rasm today | Gap |
|---|---|---|---|---|---|---|
| 1 | Lexical / format | D | file | — | Ruff format/check | — |
| 2 | Import graph / banned APIs | D/H | repo `PY_FILES` | — | TID251 + ast-grep | alias/`importlib`; incomplete `asyncio.*` |
| 3 | Annotation presence vs correctness | D / A | package + fd mypy | — | Ruff ANN; ty; mypy | dual-gate drift |
| 4 | Static types | A | ty roots + mypy | — | ty `all=error`; mypy strict | Pydantic on mypy until ty #2403 |
| 5 | Control-flow integrity | A/D | ty + mypy | — | unreachable, match syntax | no ty truthy-`if` yet |
| 6 | Security (bandit-class) | H | file | — | Ruff `S*` in `ALL` | no cross-fn taint |
| 7 | Complexity | H | file | — | PLR/C90 | no concept-density metric |
| 8 | Syntax policy | D | file | — | ast-grep `^no-` | 5 proposed rules not landed |
| 9 | Path-scoped imperative | D | module path | — | PYS0001–0002, 0004/7–10 | comprehension/`With`/`Assert` |
| 10 | Cross-file structure | H | repo fold | — | PYS0006; PYS0005 per-module | not import-graph aware |
| 11 | Packaging | D | manifest | — | validate-pyproject | no `[store]` schema yet |
| 12 | Execution / warnings | — | test subset | yes | pytest two dirs | full `tests/` not gated |
| 13 | Algebraic laws | — | repo | yes | Hypothesis in `test_quality.py` | `@pytest.mark.property` unused |
| 14 | Coverage | H | `tools/quality` | yes | `fail_under=100` | `pnpm check:py:coverage`; not in `check:py` |
| 15 | Boundary runtime types | H | call sites | yes | `@beartype` on quality CLI | not static CI |

Legend: D = decidable fragment; A = approximated solver; H = heuristic.

Niche axes teams skip: effect/IO isolation (no `Eff` rail); type-level import cycles; decorator/`pipe` order; PEP 649 identity vs `getattr` probes; comprehension-as-control-flow (PYS0011).

## `coding-python` doctrine: what type checkers cannot enforce

No single type checker enforces architectural style — expression-first domain, `pipe` composition, decorator stack order, or exhaustive `match` as sole dispatch. This table records, per doctrine rule, how much is automated today and by which tool.

| Doctrine | Automated today | Tool |
|---|---|---|
| `frozen=True` models | Partial | Ruff + PYS0008/0009 |
| `Result`/`Option` rails | Partial | PYS0004/0007; Ruff bans `returns` |
| No domain `if`/`for` | Yes (scoped paths) | PYS0001 |
| `match` + `assert_never` | Partial | mypy exhaustive-match; review |
| `pipe` / no method chains | No | Review / future LibCST |
| Decorator ordering | No | Review |
| `annotationlib` not `__annotations__` | Partial | ast-grep where scanned (not `.claude/**` in main `^no-` pass) |
| anyio not asyncio | Partial | TID251 `asyncio` import; member keys for `asyncio.run` etc. |
| No helper modules | Partial | ast-grep + filename ban (both skip `.claude/**` in `check:py`) + PYS0005 |

Intended split per the SKILL: Ruff + ty + mypy for mechanical/types; ast-grep syntax-only; py_analyzer semantics.

## How to add a policy rule without duplicating rails

Add a new policy rule to the engine that already owns its category; do not introduce a fourth or fifth engine for a concern an existing one covers. The full step-by-step procedure with verification belongs in the `coding-python` SKILL and the ast-grep fixture README, named in Boundaries; the routing rules below state which engine receives each rule shape.

1. Mechanical import/API ban → extend `banned-api` (member-level `asyncio.*`, remaining rail libraries) before reaching for a new analyzer.
2. Syntax fingerprint → add an ast-grep rule with both pass and fail fixtures (e.g. `getattr` annotations, kind-scoped `no_type_check`).
3. Path-scoped or cross-file semantics → add a PYS0011+ LibCST rule for comprehension/`try` gaps or neutral-path drift, not a mypy plugin.
4. ty parity → apply the recommended `[tool.ty.rules]` overrides; bump ty to ≥ 0.0.41 before enabling `missing-override-decorator`.

Keep the `check:py` step order. Use separate `check:py:coverage` and `check:py:mutation` invocations when needed rather than folding them into the gate.

Further consolidation candidates:

- Consider Ruff `lint.future-annotations = true` alongside the existing strict TC.
- Adopt `pnpm check:py:coverage` in CI (`fail-under=100` on `tools/quality`); extend `source` before gating `tools/py_analyzer`.
- Optional `validate-pyproject[all,store]` for a `[tool.ruff]`/`[tool.ty]` typo guard.
- Do not add Semgrep for doctrine; treat it as an optional security/SCA overlay only (see the Semgrep section).

### Recommended Ruff policy extensions

Member-level `banned-api` gaps versus coding-python: add `"asyncio.create_task"`, `"asyncio.gather"`, `"asyncio.run"`, `"asyncio.sleep"`, `"asyncio.to_thread"`, `"returns.flow"`, and `"multiprocessing.Process"` with `.msg` strings. Add optional `[tool.ruff.lint] future-annotations = true`. At Python 3.15, add `explicit-preview-rules` plus the `TID254`/`TID255` lazy-import rules.

Minimal future mypy scope: once ty ≥ 0.0.41 and native Pydantic land, shrink mypy to `tools/quality/settings.py` (plus `process.py` for `unused-awaitable`) and drop the duplicate mypy pass on `tools/py_analyzer/**`.

## Semgrep posture: optional security overlay, never doctrine

No Semgrep for coding-python doctrine. Semgrep is an optional security/SCA layer when Python gains product boundaries (HTTP/CLI plus non-trivial deps), not a style gate. Adopt only when product Python lands under `**/domain/**` or `**/application/**` with boundary deps, and then with a narrow Pro ruleset, not a CE registry sweep.

Engine comparison across the axes Semgrep would touch:

| Axis | ast-grep + fixtures | Ruff (`ALL`/S/TID251) | py_analyzer (LibCST) | Semgrep CE | Semgrep Pro (Code/SCA) |
|---|---|---|---|---|---|
| Taint / dataflow | No | No (per-file sinks) | No | Intraprocedural only | Interprocedural + Python interfile (`--pro`, `interfile: true`) |
| Supply chain | No | No | No | Lockfile parse | CVE + reachability to import/call sites (`uv.lock`) |
| Custom policy | Syntax YAML; fast | `banned-api`, ignores | Path semantics (PYS*) | Pattern/taint YAML | + managed packs, AppSec policies |

Owner per need, and when Semgrep is justified:

| Need | Owner | Semgrep only when |
|---|---|---|
| Doctrine (`if`/Result/frozen) | py_analyzer | Never — conflicts PYS* |
| PEP 649 / helper imports | ast-grep + Ruff | Never (duplicate fingerprints) |
| Bandit-class APIs | Ruff S (in `ALL`) | Pro if wrappers hide sinks from S-rules |
| Injection / SSRF across helpers | — | Pro taint (not LibCST without a taint engine) |
| CVE triage | `uv`/OSV + Renovate | Pro SCA reachability on direct vulnerable API use |

Second-engine cost: CE scans take seconds to minutes versus ast-grep's milliseconds; Pro interfile needs multi-GB RAM (roughly 8 GB/core per Semgrep docs) and slows PR jobs; Ruff `ALL` already emits S*, so registry overlap needs dedupe; Pro SCA needs `.semgrep.yml` plus an AppSec token; encoding style in Semgrep causes doctrine drift. For Rasm today (roughly 39 `.py`), ROI is negative until domain/application Python takes external input.

Three rules worth Semgrep only if adopted (Pro): (1) boundary → `subprocess`/`os.system`/`shell=True` taint across wrappers; (2) boundary → outbound HTTP client URL (SSRF beyond Ruff `S310` scheme audit); (3) SCA reachability — a vulnerable deserialize/YAML/pickle-class dep and an interfile path from untrusted input (beyond Ruff `S301` single file).

Never Semgrep: PYS0001 flow, TID251 rails, ast-grep `no-*`. Extend the owning engine instead.

Evidence: Semgrep CE/Pro docs (taint, interfile, SCA reachability, RAM guidance), 2025–2026.
Last verified: 2026-06-03.
Review trigger: product Python with external input lands, or Semgrep changes its CE/Pro feature split.

## Bleeding-edge tools to skip

Each tool below was evaluated and rejected or deferred for Rasm in 2025–2026; the verdict and reason let a future evaluator avoid re-litigating a settled call.

| Tool | Verdict | Why |
|---|---|---|
| Ruffian | Skip | Duplicates ast-grep + py_analyzer; alpha; Ruff has no plugin surface |
| basedpyright | Skip | Fourth type rail; no Pydantic plugin / PYS doctrine |
| Zuban/zmypy | Skip | AGPL; duplicates mypy migration mid-ty |
| Pylint 4.x / "pylint-ai" | Skip | GPL; overlaps Ruff `ALL`; no fixture contract |
| Pyrefly | Defer | Revisit if ty misses Pydantic/override milestones |
| Semgrep CE | Defer | Security/taint only per the Semgrep section |

Last verified: 2026-06-03.
Review trigger: a listed tool ships a Rasm-relevant capability (plugin surface, Pydantic, license change).

## Advanced considerations

These are the non-obvious gaps and couplings a maintainer should hold in mind; each is a real edge, not a hypothetical.

1. Comprehension loophole: PYS0001 uses `cst.For`, not comprehension generators, so the skill's "no `for` in domain" is only partially enforced.
2. Dual type gates: ty and mypy duplicate some diagnostics; keep mypy for Pydantic until ty ships native support.
3. TID251 import vs use: banning module `asyncio` does not ban `asyncio.run` — add explicit member keys.
4. Double mypy: a package pass then a per-file pass on the same `tools/quality` modules — watch for divergent diagnostics.
5. Coverage 100% on `tools/quality` is enforced by `pnpm check:py:coverage`, not by `check:py`.
6. beartype: O(1) container sampling; `Result[T,E]` payload types are not validated at runtime.
7. Python 3.15: Ruff TID254/255 lazy-import rules are inactive at `py314`.
8. C# parity: `tools/cs-analyzer` owns semantic CSP rules; the Python mirror is `py_analyzer`, not ty.

## Repo mentions index

Where each Python quality concern is referenced across the repo.

| Location | Mentions |
|---|---|
| `package.json` | `check:py`, `check:py:coverage`, `check:py:mutation`, `check:ts`, ast-grep |
| `pyproject.toml` | ruff, ty, mypy, pytest, coverage |
| `sgconfig.yml`, `.rules/**` | ast-grep |
| `tools/py_analyzer/**` | LibCST analyzer |
| `tools/quality/**` | beartype, cyclopts; C# rails separate |
| `.claude/skills/coding-python/**` | PEP map, validation checklist |
| `AGENTS.md` | ast-grep for structural search |
| `CLAUDE.md` §5.2 | Quality rails (C#-centric operator) |

## Boundaries

- `.claude/skills/coding-python/SKILL.md` owns the doctrine procedure and the step-by-step path to author or validate a Python policy rule.
- `.claude/skills/coding-python/references/validation.md` owns the law matrix `G*` codes.
- `tests/tools/ast-grep/README.md`, if present, owns the ast-grep fixture catalog and the procedure to add pass/fail fixtures.
- `tools/quality/README.md` owns the quality operator and the C# static/test/bridge rails.
- `docs/system-api-map` owns BCL and package policy.
- `CLAUDE.md` §5.2 owns the C#-centric quality-rail operator surface.
- `uv.lock` pins pydantic 2.13.3 for the mypy-plugin coupling and is the source of truth for tool versions.

Last validated against repo: 2026-06-03.
