# Python quality tooling — Rasm monorepo reference

**Quadrant:** Reference. **Pinned toolchain (local):** Ruff 0.15.12, ty 0.0.34, mypy 1.20.2, ast-grep 0.42.1, Python 3.14 (`requires-python >=3.14`), validate-pyproject 0.25, pydantic 2.13.3 (`uv.lock`).

**Audience:** Contributors changing Python policy, `pyproject.toml`, `.rules/python/`, or `tools/py_analyzer`. Doctrine procedure lives in `.claude/skills/coding-python/SKILL.md`; this file states what CI runs and where each rule lives.

---

## 1. Purpose and scope

This document maps **what Rasm runs**, **where rules live**, **what each tool can and cannot enforce**, and **how to add policy** without duplicating rails. It focuses on **advanced/modern** posture (PEP 649/695, ty beta, Ruff `ALL`+`preview`, LibCST semantic analyzer), not generic linter introductions.

**Out of scope:** C# `tools/cs-analyzer` and `uv run python -m tools.quality` static/test/bridge rails (see `tools/quality/README.md`, `CLAUDE.md` §5.2).

**Python surface today:** ~39 `.py` files (mostly `tools/quality`, `tools/py_analyzer`, `.claude/**/scripts`, tests). `py_analyzer` path rules apply under `**/domain/**` and `**/application/**` when those trees exist; current repo Python is mostly tooling/neutral scope.

### 1.1 Custom rules and doctrine

**Can ty/mypy enforce `coding-python` shape?** No. Neither is a doctrine engine: ty has no plugins; mypy plugins are type-centric (`pydantic.mypy`). They prove assignability, protocols, and narrowing—not expression-first domain design, `pipe` composition, decorator order, path-scoped imperative bans, `Result`/`Option` rails, frozen-model policy, or cross-module duplicate shapes. Rasm splits enforcement: Ruff + `banned-api` (mechanical); ty (primary types) + mypy (Pydantic, `exhaustive-match`); ast-grep (syntax); `py_analyzer` PYS* (semantics); Hypothesis + `@beartype` (laws / CLI runtime); review for undecidable style (`pipe`, decorator stacks).

**Objective categories:** lexical/format · import/banned API · annotation presence vs correctness · static types · type-level control-flow · bandit-class security · complexity · syntax policy (ast-grep) · path-scoped semantics (LibCST) · packaging · runtime tests · property laws · coverage · boundary runtime types.

---

## 2. Canonical gate: `pnpm check:py`

Authoritative decode of `package.json` `scripts.check:py` (single `&&`-chained shell line).

| Step | Command / tool | Scope / notes |
|------|----------------|---------------|
| −1 | `export UV_CACHE_DIR`, `HYPOTHESIS_STORAGE_DIRECTORY` | `.cache/uv`, `.cache/hypothesis` |
| 0 | `uv run validate-pyproject pyproject.toml` | PEP 621 / build metadata (`validate-pyproject[all]` dev dep) |
| 1 | `ruff format --check $PY_FILES` | See **PY_FILES** below |
| 2 | `ruff check $PY_FILES` | Same; includes `tests/tools/ast-grep/fail/**` (ty/mypy exclude only those) |
| 3 | `ty check` | `tools/py_analyzer`, `tests/tools/py_analyzer`, `tools/quality`, `tests/tools/quality`, `.claude/hooks`, `.claude/skills/*/scripts` |
| 4 | `mypy --explicit-package-bases` | Four package trees only (no `.claude/**`) |
| 5 | `fd` + per-file `mypy` | All repo `.py` (`fd -H -e py . .claude/hooks .claude/skills/*/scripts`); `MYPYPATH=$(dirname file)` — overlaps step 4 on package trees plus e.g. `tests/conftest.py` |
| 6 | `pnpm exec ast-grep scan` `^no-` | Excludes `tests/tools/ast-grep/**`, `.artifacts`, `.claude`, `.cache`, `.git`, `.nx`, `.venv`, `coverage`, `node_modules`, `test-results`, `tmp` |
| 7 | `fd` + `rg` + `!` | Ban helper-style filenames (excludes `.claude`, `tests/tools/ast-grep`, caches — same family as step 6) |
| 8–9 | ast-grep `pass` / inverted `fail` | Fixture contract |
| 10 | `python -m tools.py_analyzer check --root .` | LibCST semantic scan |
| 11 | `pytest` | `tests/tools/py_analyzer`, `tests/tools/quality` only |

**`PY_FILES`:** `fd -H -e py .` plus named roots — effectively **all repository `.py` files** (including `tests/conftest.py`, ast-grep fixtures), not only the six named directories.

**Precedence:** mechanical layout/imports → Ruff; assignability → ty (primary), mypy (secondary, Pydantic); repo semantics → `py_analyzer`; syntax fingerprints → ast-grep + fixtures.

**Outside this gate:** `beartype` on quality CLI entrypoints; `pnpm check:py:coverage` (`fail-under=100` on `tools/quality` only); `pnpm check:py:mutation`; full `tests/` pytest; `uv run python -m tools.quality` C# rails. `tests/conftest.py`: in Ruff `PY_FILES` and mypy step 5; not in ty step 4.

---

## 3. `sgconfig.yml`

**Path:** repository root.

```yaml
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
|-------|------|
| `ruleDirs` | Directories scanned for YAML rules (`*.yml`). |
| `languageGlobs.tsx` | Maps TS/TSX/MTS/CTS to ast-grep **Tsx** engine (one rule pack for all TS dialects). |

**Does not define rules** — only discovery paths and TS extension mapping.

**Integration:** `pnpm exec ast-grep scan --config sgconfig.yml` in `check:py` (`--filter '^no-'`) and `check:ts` (`--filter '^ts-domain-'`). Catalog: `@ast-grep/cli` 0.42.1 in `pnpm-workspace.yaml`.

---

## 4. `.rules/` — structural policy (ast-grep)

### 4.1 `.rules/python/` (3 rules; CI filter `^no-`; main scan excludes `.claude/**`)

| Rule ID | File | Enforces |
|---------|------|----------|
| `no-direct-annotations` | `no-direct-annotations.yml` | Ban `obj.__annotations__`, `__dict__["__annotations__"]`, `.get("__annotations__")`. Prefer `annotationlib.get_annotations()` / `inspect.get_annotations()` (PEP 649/749). |
| `no-typing-no-type-check` | `no-typing-no-type-check.yml` | Ban `no_type_check` import/usage (`typing`, `typing_extensions`). |
| `no-helper-import` | `no-helper-import.yml` | Ban top-level `helpers`, `helper`, `utils`, `util` imports. |

**Not ast-grep for Python:** domain `if`/`for`/`while`/`try`/`raise` — **`PYS0001`** in `tools/py_analyzer` (path-scoped). TypeScript: `ts-domain-no-statement-flow` in `.rules/typescript/`.

**Proposed additions (not yet in repo):** `no-getattr-annotations`, `no-no-type-check-on-def` (kind `decorated_definition`), `no-hasattr-probe`, `no-isinstance-dispatch`, `no-asyncio-unstructured` — each needs `.rules/python/*.yml` + `tests/tools/ast-grep/{pass,fail}/` fixtures.

### 4.2 `.rules/typescript/` (2 rules; filter `^ts-domain-`)

| Rule ID | File | Scope (`files:` globs) |
|---------|------|-------------------------|
| `ts-domain-no-let` | `ts-domain-no-let.yml` | `**/domain/**`, `**/application/**` under apps/libs/tools src; includes `tests/tools/ast-grep/**` |
| `ts-domain-no-statement-flow` | `ts-domain-no-statement-flow.yml` | Same; bans `if`, loops, `try`, `throw`, etc. |

---

## 5. `tests/tools/ast-grep/` — contract harness

**Purpose:** Golden fixtures for ast-grep rules (not production code).

| Directory | Expectation |
|-----------|-------------|
| `pass/` | `ast-grep scan --filter '^no-'` (or `^ts-domain-`) produces **no errors** |
| `fail/` | Scan **must** match (shell inverts success: `! ast-grep scan ... fail`) |

**Python fixtures (examples):**

- `pass/annotations_api.py` — `annotationlib.get_annotations`
- `pass/match_case.py` — allowed `match`/`assert_never`
- `fail/direct_annotations.py` — `__annotations__`
- `fail/helper_import.py` — `from utils import`
- `fail/no_type_check.py` — typing escape

**TypeScript:** `pass/expression_flow.*`, `fail/statement_flow.*` (imperative `if` chains).

**Exclusions:** `ty` and `mypy` exclude `tests/tools/ast-grep/fail/**`. `IGNORE_FIXTURE_PREFIXES` includes `tests/tools/ast-grep/` and `tests/tools/py_analyzer/`. `check:ts` runs `^ts-domain-` on TS fixtures; `check:py` runs `^no-` only. `fail/statement_flow.*` exercises `ts-domain-no-statement-flow` (and `.mts` also hits `ts-domain-no-let`). `pass/match_case.py` is not owned by an ast-grep rule (mypy/py_analyzer doctrine).

---

## 6. `pyproject.toml` — what is “custom” and valid

There are **no** Ruff plugins, ty plugins, or custom mypy modules in-repo. “Custom enforcement” is **configuration of built-in engines**:

### 6.1 Active and valid

| Section | Real? | Role |
|---------|-------|------|
| `[tool.ruff]` `select = ["ALL"]`, `preview`, `target-version = "py314"` | Yes | Full native rule surface minus `ignore` list |
| `[tool.ruff.lint.flake8-tidy-imports.banned-api]` | Yes (**TID251**) | **66** import/API bans with custom `.msg` strings |
| `[tool.ruff.lint.flake8-tidy-imports.banned-module-level-imports]` | Yes | Scientific stack (`numpy`, `pandas`, …) quarantined to `scientific` dependency group |
| `[tool.ruff.lint.per-file-ignores]` | Yes | `tools/quality/**` (CLI/TC relaxations), `tests/**`, skill scripts (**full TID251 off** on `**/.claude/skills/**/scripts/**`) |
| `line-length = 150`, Google pydocstyle | Yes | Active formatting/doc conventions |
| `[tool.ty.rules]` `all = "error"`; `[tool.ty.analysis]` `respect-type-ignore-comments = false` | Yes | Primary type gate; §6.4 parity overrides **recommended, not yet applied** |
| `[tool.mypy]` `strict`, `plugins = ["pydantic.mypy"]`, `enable_error_code = [...]` | Yes | Secondary types + Pydantic |
| `[tool.pytest]` | Yes | Strict pytest, socket lockdown, 30s timeout; `required_plugins` includes `pytest-cov` |
| `[tool.coverage.*]` | Yes | `source = ["tools/quality"]`, `branch = true`, `fail_under = 100` — **`pnpm check:py:coverage`**, not `check:py` |

### 6.2 Not in `pyproject.toml`

- ast-grep rules → `.rules/**/*.yml` + `sgconfig.yml`
- Semantic doctrine → `tools/py_analyzer` (`PYS0001`–`PYS0010`)
- Helper **filenames** → shell in `check:py`

### 6.4 `[tool.ty.rules]` — mypy `enable_error_code` parity (ty 0.0.34)

ty has **no** global `preview` / `experimental` switch (0.0.34): preview rules are ordinary rule names with default severities; set per-rule overrides or rely on `all = "error"` plus `[tool.ty.terminal] error-on-warning = true`.

| mypy `enable_error_code` | ty 0.0.34 rule | Notes |
|--------------------------|----------------|-------|
| `unused-awaitable` | `unused-awaitable = "error"` | Preview; default `warn` |
| `explicit-override` | `invalid-explicit-override` | Already `error` via `all`; flags *spurious* `@override` |
| `explicit-override` (strict) | `missing-override-decorator` | **Unknown in 0.0.34**; lands in **ty ≥ 0.0.41** (preview, default `ignore`) |
| `ignore-without-code` | `ignore-comment-unknown-rule`, `unused-ignore-comment` | Pair with `respect-type-ignore-comments = false` → suppressions must be `ty: ignore[rule]`; bare `type: ignore` does not silence ty |
| `exhaustive-match` | — | Keep on **mypy**; ty has `invalid-match-pattern` (syntax only), not exhaustiveness |
| `redundant-self` | — | **mypy only** |
| `truthy-bool`, `truthy-iterable` | — | **mypy only**; not `unsupported-bool-conversion` (runtime `__bool__` failures) |

Recommended `[tool.ty.rules]` overrides (merge into `pyproject.toml`; `[tool.ty.environment]`, `[tool.ty.analysis]`, `[tool.ty.src]` already match; bump ty before `missing-override-decorator`):

```toml
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

**ty cannot enforce (keep mypy + `py_analyzer`):** domain `if`/`for`/`try`/`raise` (PYS0001–0002), `Result`/`Option` signature rails (PYS0004/0007), frozen/mutable model policy (PYS0008/0009), `@effect.result` recovery (PYS0010), cross-module duplicate models / single-call privates (PYS0005/0006), `pipe`/decorator-order doctrine, Pydantic field/init semantics (`pydantic.mypy`).

**ty 2026 roadmap (Rasm-relevant):** Stable-milestone **native Pydantic** ([#2403](https://github.com/astral-sh/ty/issues/2403)); **ty-1.1** `missing-override-decorator` ([#155](https://github.com/astral-sh/ty/issues/155) / PR #25111); type-powered lint for always-truthy `if` ([#1073](https://github.com/astral-sh/ty/issues/1073)); PEP **728** `closed=True` TypedDict narrowing ([#154](https://github.com/astral-sh/ty/issues/154)); **no plugin system** — third-party support in-core only; **0.0.x** diagnostic breakage until GA ([#1889](https://github.com/astral-sh/ty/issues/1889)).

### 6.3 `banned-api` (policy-as-data)

Ruff **TID251** matches:

- **Import statements:** exact module or parent prefix (e.g. `"asyncio"` bans `import asyncio`, not necessarily `asyncio.run` unless listed).
- **Attribute access:** exact qualified name (e.g. `"json.loads"`).

Bypass: `importlib`, aliases, re-exports — documented Ruff limitation; complement with member-level keys or `py_analyzer`.

---

## 7. `tools/py_analyzer` — LibCST semantic gate

**CLI:** `uv run python -m tools.py_analyzer check --root . --format text|json|github`

### 7.1 Rules (`PYS0000`–`PYS0010`)

| ID | Category | Enforces |
|----|----------|----------|
| PYS0000 | Infrastructure | Parse/read failures |
| PYS0001 | FunctionalDiscipline | No `if`/`for`/`while`/`try`/`raise` in **domain/application** paths |
| PYS0002 | Governance | Boundary imperative flow requires `# RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=<vocab> ticket=... expires=YYYY-MM-DD rationale=...` on the line above; `reason` ∈ `protocol-required`, `cleanup-finally`, `cancellation-guard`, `adapter-normalization` |
| PYS0003 | TypeDiscipline | No bare primitives/`Any`/mutable containers in public domain/application signatures |
| PYS0004 | FunctionalDiscipline | Fallible names / `Optional` returns → `Result`/`Option` |
| PYS0005 | SurfaceArea | Single-call **same-module** private functions |
| PYS0006 | TypeDiscipline | Duplicate model field shapes (**cross-module** global fold) |
| PYS0007 | FunctionalDiscipline | No `.unwrap()` / `.value_or()` in domain/application |
| PYS0008 | TypeDiscipline | Models must declare frozen policy |
| PYS0009 | TypeDiscipline | No mutable field annotations on domain models |
| PYS0010 | FunctionalDiscipline | No `.or_else_with` recovery inside `@effect.result` builders |

### 7.2 Scope classification (path-based)

From `classify_scope()` on path **segments** (precedence: **tooling → boundary → domain → application**):

- Segment `domain` → domain; `application` → application
- Segments `adapters`, `boundary`, `boundaries` → boundary (e.g. `src/domain/adapters/http.py` is boundary, not domain)
- `tools/py_analyzer`, `tools/quality`, `.claude/hooks`, `.claude/skills/**/scripts` → tooling
- Else → neutral (no PYS0001)

**Discovery excludes:** `EXCLUDED_DIRS` (`.artifacts`, `.cache`, `.git`, `.nx`, `.venv`, `coverage`, `node_modules`, `test-results`, `tmp`) plus prefix `tests/tools/ast-grep` (`EXCLUDED_PREFIXES`) — **not** all of `tests/`. `tests/tools/py_analyzer/**` and `tests/tools/quality/**` are scanned on `--root .` but are tooling/neutral scope. Unit tests also use `tmp_path` + explicit paths for domain examples.

**Allowed in domain (not matched by PYS0001 visitor):** `match`, `with`, `assert` — only `If|For|While|Try|Raise` are banned.

**Proposed PYS0011+ (LibCST):** `DomainComprehensionFlow` (`Comprehension`/`GeneratorExp`), `DomainResourceStatement` (`With`), `DomainAssertGuard` (`Assert`).

### 7.3 Architecture

Per-module LibCST visitor → `ModuleFacts` → **global fold PYS0006 only**; PYS0005 aggregates per-module private call counts. Uses `PositionProvider`, `ParentNodeProvider`.

---

## 8. Per-tool: coverage and custom-rule capability

### 8.1 Summary matrix

| Tool | Custom user rules? | Primary domains |
|------|-------------------|-----------------|
| **Ruff** | No (Rust only); policy via `banned-api`, ignores | Style, imports, security, modernization, complexity |
| **ty** | No; built-in rules, severity/overrides only | Types, protocols, narrowing, async |
| **mypy** | Yes (plugins); hooks are type-centric | Types + **pydantic.mypy** |
| **ast-grep** | Yes (YAML) | Syntax / structure |
| **LibCST / py_analyzer** | Yes (Python visitor) | Scoped semantics, cross-file facts |
| **validate-pyproject[all]** | No; schema + formats | PEP 621/735/build-system |
| **pytest / Hypothesis** | Tests | Runtime behavior, laws |
| **beartype** | Runtime `@beartype` | Call-site shapes when CLI runs |

### 8.2 Best tool per enforcement category

| Category | Best owner | Notes |
|----------|------------|-------|
| Formatting / PEP8 layout | Ruff format + Ruff E/W | |
| Import order / TC | Ruff I, TC | `flake8-type-checking.strict = true` |
| Banned library/API | Ruff `banned-api` | Extend before new tools |
| Annotation *presence* | Ruff ANN | ty proves correctness |
| Type assignability | **ty** (primary), mypy (secondary) | |
| Pydantic `__init__` / fields | **mypy + pydantic.mypy** | Until ty native Pydantic parity |
| `match` exhaustiveness | mypy `exhaustive-match` + ty narrowing | |
| Syntax fingerprint | **ast-grep** | Fast, fixture-tested |
| Domain no-`if`/`for`, Result rails | **py_analyzer** | Not ty/Ruff |
| Cross-module duplicate models | **py_analyzer** | |
| Packaging metadata | **validate-pyproject** | First in `check:py` |
| Algebraic laws | **Hypothesis** | Not linter rules |
| CLI invocation contracts | **beartype** | Not in CI static gate |

---

## 9. Objective enforcement taxonomy

| # | Axis | Decidable? | Scope | Runtime? | Rasm today | Gap |
|---|------|------------|-------|----------|------------|-----|
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
| 12 | Execution / warnings | — | test subset | **yes** | pytest two dirs | full `tests/` not gated |
| 13 | Algebraic laws | — | repo | **yes** | Hypothesis in `test_quality.py` | `@pytest.mark.property` unused |
| 14 | Coverage | H | `tools/quality` | **yes** | `fail_under=100` | `pnpm check:py:coverage`; not in `check:py` |
| 15 | Boundary runtime types | H | call sites | **yes** | `@beartype` on quality CLI | not static CI |

**Niche axes teams skip:** effect/IO isolation (no `Eff` rail); type-level import cycles; decorator/`pipe` order; PEP 649 identity vs `getattr` probes; comprehension-as-control-flow (PYS0011).

**Legend:** D = decidable fragment; A = approximated solver; H = heuristic.

---

## 10. `coding-python` doctrine — can ty/mypy enforce it?

**No single type checker enforces architectural style** (expression-first domain, `pipe` composition, decorator stack order, exhaustive `match` as sole dispatch).

| Doctrine | Automated today | Tool |
|----------|-----------------|------|
| `frozen=True` models | Partial | Ruff + PYS0008/0009 |
| `Result`/`Option` rails | Partial | PYS0004/0007; Ruff bans `returns` |
| No domain `if`/`for` | Yes (scoped paths) | PYS0001 |
| `match` + `assert_never` | Partial | mypy exhaustive-match; review |
| `pipe` / no method chains | **No** | Review / future LibCST |
| Decorator ordering | **No** | Review |
| `annotationlib` not `__annotations__` | Partial | ast-grep where scanned (not `.claude/**` in main `^no-` pass) |
| anyio not asyncio | Partial | TID251 `asyncio` import; member keys for `asyncio.run` etc. |
| No helper modules | Partial | ast-grep + filename ban (both skip `.claude/**` in `check:py`) + PYS0005 |

**Intended split (SKILL):** Ruff + ty + mypy for mechanical/types; ast-grep syntax-only; **py_analyzer** semantics.

---

## 11. Consolidation recommendations

1. Keep `check:py` order; use separate `check:py:coverage` and `check:py:mutation` when needed.  
2. Extend **`banned-api`** (member-level `asyncio.*`, remaining rail libraries) before new analyzers.  
3. Add ast-grep rules with **pass/fail fixtures** (`getattr` annotations, kind-scoped `no_type_check`).  
4. Expand **PYS0011+** in LibCST for comprehension/`try` gaps, neutral-path drift — not mypy plugins.  
5. ty: apply §6.4 `[tool.ty.rules]` overrides; bump ty to ≥0.0.41 before enabling `missing-override-decorator`.  
6. Consider `lint.future-annotations = true` (Ruff) with existing strict TC.  
7. **coverage:** adopt `pnpm check:py:coverage` in CI (`fail-under=100` on `tools/quality`); extend `source` before gating `tools/py_analyzer`.  
8. Optional `validate-pyproject[all,store]` for `[tool.ruff]`/`[tool.ty]` typo guard.  
9. Do **not** add Semgrep for doctrine; optional security/SCA overlay only — see **§11.9**.

### 11.9 Semgrep vs ast-grep + Ruff + py_analyzer (2025–2026)

**Posture:** No Semgrep for coding-python doctrine. Semgrep is an optional **security/SCA** layer when Python gains product boundaries (HTTP/CLI + non-trivial deps), not a style gate.

| Axis | ast-grep + fixtures | Ruff (`ALL`/S/TID251) | py_analyzer (LibCST) | Semgrep CE | Semgrep Pro (Code/SCA) |
|------|---------------------|----------------------|----------------------|------------|------------------------|
| Taint / dataflow | No | No (per-file sinks) | No | Intraprocedural only | Interprocedural + Python interfile (`--pro`, `interfile: true`) |
| Supply chain | No | No | No | Lockfile parse | CVE + **reachability** to import/call sites (`uv.lock`) |
| Custom policy | Syntax YAML; fast | `banned-api`, ignores | Path semantics (PYS*) | Pattern/taint YAML | + managed packs, AppSec policies |

| Need | Owner | Semgrep only when |
|------|-------|-------------------|
| Doctrine (`if`/Result/frozen) | py_analyzer | Never — conflicts PYS* |
| PEP 649 / helper imports | ast-grep + Ruff | Never (duplicate fingerprints) |
| Bandit-class APIs | Ruff S (in `ALL`) | Pro if wrappers hide sinks from S-rules |
| Injection / SSRF across helpers | — | Pro taint (not LibCST without a taint engine) |
| CVE triage | `uv`/OSV + Renovate | Pro SCA reachability on **direct** vulnerable API use |

**Second-engine cost:** CE scans seconds–minutes vs ast-grep ~ms; Pro interfile needs multi‑GB RAM (~8 GB/core per Semgrep docs) and slower PR jobs; Ruff `ALL` already emits S* — registry overlap needs dedupe; `.semgrep.yml` + AppSec token for Pro SCA; doctrine drift if Semgrep encodes style. **Rasm today (~39 `.py`):** negative ROI until domain/application Python with external input.

**Adopt trigger:** product Python under `**/domain/**` / `**/application/**` + boundary deps; narrow Pro ruleset, not CE registry sweep.

**Three rules worth Semgrep only if adopted (Pro):** (1) boundary → `subprocess`/`os.system`/`shell=True` taint across wrappers; (2) boundary → outbound HTTP client URL (SSRF beyond Ruff `S310` scheme audit); (3) SCA reachability — vulnerable deserialize/YAML/pickle-class dep **and** interfile path from untrusted input (beyond Ruff `S301` single file).

**Never Semgrep:** PYS0001 flow, TID251 rails, ast-grep `no-*` — extend §11.2–11.4 owners instead.

---

## 12. Repo mentions index (Python quality)

| Location | Mentions |
|----------|----------|
| `package.json` | `check:py`, `check:py:coverage`, `check:py:mutation`, `check:ts`, ast-grep |
| `pyproject.toml` | ruff, ty, mypy, pytest, coverage |
| `sgconfig.yml`, `.rules/**` | ast-grep |
| `tools/py_analyzer/**` | LibCST analyzer |
| `tools/quality/**` | beartype, cyclopts; C# rails separate |
| `.claude/skills/coding-python/**` | PEP map, validation checklist |
| `AGENTS.md` | ast-grep for structural search |
| `CLAUDE.md` §5.2 | Quality rails (C#-centric operator) |

---

## 13. Advanced considerations

1. **Comprehension loophole:** PYS0001 uses `cst.For`, not comprehension generators — skill “no `for` in domain” is only partially enforced.  
2. **Dual type gates:** ty + mypy duplicate some diagnostics; keep mypy for Pydantic until ty ships native support.  
3. **TID251 import vs use:** module ban `asyncio` ≠ ban `asyncio.run` — add explicit member keys.  
4. **Double mypy:** package pass then per-file pass on same `tools/quality` modules — watch for divergent diagnostics.  
5. **coverage 100%** on `tools/quality`: enforced by `pnpm check:py:coverage`, not by `check:py`.  
6. **beartype:** O(1) container sampling; `Result[T,E]` payload types not validated at runtime.  
7. **Python 3.15:** Ruff TID254/255 lazy-import rules inactive at `py314`.  
8. **C# parity:** `tools/cs-analyzer` owns semantic CSP rules; Python mirror is `py_analyzer`, not ty.

---

## 14. Ruff policy extensions (recommended TOML)

Member-level `banned-api` gaps vs coding-python: add `"asyncio.create_task"`, `"asyncio.gather"`, `"asyncio.run"`, `"asyncio.sleep"`, `"asyncio.to_thread"`, `"returns.flow"`, `"multiprocessing.Process"` with `.msg` strings. Optional `[tool.ruff.lint] future-annotations = true`. At Python 3.15: `explicit-preview-rules` + `TID254`/`TID255` lazy-import rules.

**Minimal mypy scope (future):** shrink to `tools/quality/settings.py` (+ `process.py` for `unused-awaitable`) once ty ≥0.0.41 and native Pydantic land; drop duplicate mypy on `tools/py_analyzer/**`.

---

## 15. Bleeding-edge tools — skip for Rasm (2025–2026)

| Tool | Verdict | Why |
|------|---------|-----|
| Ruffian | **Skip** | Duplicates ast-grep + py_analyzer; alpha; Ruff has no plugin surface |
| basedpyright | **Skip** | Fourth type rail; no Pydantic plugin / PYS doctrine |
| Zuban/zmypy | **Skip** | AGPL; duplicates mypy migration mid-ty |
| Pylint 4.x / “pylint-ai” | **Skip** | GPL; overlaps Ruff `ALL`; no fixture contract |
| Pyrefly | **Defer** | Revisit if ty misses Pydantic/override milestones |
| Semgrep CE | **Defer** | Security/taint only per §11.9 |

---

## 16. Related paths

| Path | Role |
|------|------|
| `tools/quality/README.md` | Quality operator |
| `tests/tools/ast-grep/README.md` | (optional) fixture catalog |
| `.claude/skills/coding-python/references/validation.md` | Law matrix G* codes |
| `docs/system-api-map` | BCL / package policy |
| `uv.lock` | Pinned pydantic 2.13.3 for mypy plugin coupling |


*Last validated against repo: 2026-06-03.*
