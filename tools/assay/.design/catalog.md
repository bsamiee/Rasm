# [H1][ASSAY_CATALOG_DESIGN]
>**Dictum:** *Every external program is one dense row; adding a program adds a row, never a module, type, or function.*

`composition/catalog.py` owns `TOOLS: tuple[Tool, ...]` and `select(claim, language)`. It imports only `core/model.py`. Variance lives in three axis enums (`Runner`, `Input`, `Language`) plus the `Claim`/`Mode` fields; the catalog body is data.

**Canonical:** [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) §2 (`Mode` unified) · [`model.md`](model.md) (`Parser` callable on row) · [`routing.md`](routing.md) (`place`) · [`CRITIQUE-SHAPES.md`](CRITIQUE-SHAPES.md) — no `parser: str` registry key (supersedes `research-msgspec.md` §6 item 3).

---
## [1][ROW_SET]

`Tool(name, runner, command, input, language, claim, mode=Mode.CHECK, parser=None, timeout=None)`. `Mode.writes` replaces `mutates`; `Mode.stream` drives engine tee. Below, defaulted fields are omitted. Each row is one program invocation verified against `tools/quality` and `package.json`.

**STATIC — Python** (`check:py` steps 0-6,10):

```python
Tool("validate-pyproject", UV, ("validate-pyproject", "pyproject.toml"), NONE, PYTHON, STATIC)
Tool("ruff-format", UV, ("ruff", "format", "--check"), FILES, PYTHON, STATIC)
Tool("ruff-format", UV, ("ruff", "format"), FILES, PYTHON, STATIC, mode=Mode.WRITE)
Tool("ruff", UV, ("ruff", "check"), FILES, PYTHON, STATIC)
Tool("ruff", UV, ("ruff", "check", "--fix"), FILES, PYTHON, STATIC, mode=Mode.WRITE)
Tool("ty", UV, ("ty", "check", "--no-progress"), FILES, PYTHON, STATIC)
Tool("mypy", UV, ("mypy", "--explicit-package-bases"), FILES, PYTHON, STATIC)
Tool("ast-grep-py", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^no-", "--error"), GLOB, PYTHON, STATIC)
Tool("py-analyzer", Runner.MODULE, ("tools.py_analyzer", "check", "--root", ".", "--format", "json"), NONE, PYTHON, STATIC, parser=parse_findings)
```

**STATIC — TypeScript** (`check:ts` non-test steps):

```python
Tool("tsc", PNPM, ("tsc", "--noEmit", "-p", "tsconfig.base.json"), NONE, TYPESCRIPT, STATIC)
Tool("biome", PNPM, ("biome", "ci", "--files-ignore-unknown=true"), FILES, TYPESCRIPT, STATIC)
Tool("biome", PNPM, ("biome", "check", "--write"), FILES, TYPESCRIPT, STATIC, mode=Mode.WRITE)
Tool("ast-grep-ts", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error"), GLOB, TYPESCRIPT, STATIC)
Tool("knip", PNPM, ("knip", "--exclude", "catalog", "--no-config-hints"), NONE, TYPESCRIPT, STATIC)
Tool("sherif", PNPM, ("sherif",), NONE, TYPESCRIPT, STATIC)
```

**STATIC — C#** (`tools/quality/rails/static.py`, `process.dotnet_args`):

```python
Tool("dotnet-format", DOTNET, ("format", "--severity", "error", "--verify-no-changes"), INCLUDE, CSHARP, STATIC)
Tool("dotnet-format", DOTNET, ("format", "--severity", "error"), INCLUDE, CSHARP, STATIC, mode=Mode.WRITE)
Tool("dotnet-restore", DOTNET, ("restore", "--locked-mode"), PROJECT, CSHARP, STATIC, mode=RESTORE)
Tool("dotnet-build", DOTNET, ("build", "--no-restore", "-v:quiet", "/clp:ErrorsOnly"), PROJECT, CSHARP, STATIC, mode=BUILD, parser=parse_build)
```

**TEST** (`tools/quality/rails/test.py`, `vitest`, `pytest`):

```python
Tool("dotnet-test", DOTNET, ("test", "--minimum-expected-tests", "1"), PROJECT, CSHARP, TEST, mode=RUN)
Tool("dotnet-test", DOTNET, ("test", "--list-tests"), PROJECT, CSHARP, TEST, mode=LIST, parser=parse_tests)
Tool("dotnet-stryker", DOTNET, ("tool", "run", "dotnet-stryker"), PROJECT, CSHARP, TEST, mode=MUTATION, timeout=3600.0)
Tool("pytest", UV, ("pytest",), FILES, PYTHON, TEST, mode=RUN)
Tool("vitest", PNPM, ("vitest", "run"), FILES, TYPESCRIPT, TEST, mode=RUN)
```

**BRIDGE / PACKAGE / API — C#** (`rails/bridge.py`, `package.py`, `api.py`):

```python
Tool("rasm-bridge", DOTNET, ("run", "--no-build", "--"), PROJECT, CSHARP, BRIDGE, mode=CLIENT)
Tool("rasm-bridge", DOTNET, ("run", "--no-build", "--", "verify"), PROJECT, CSHARP, BRIDGE, mode=VERIFY, parser=parse_verify)
Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--"), FILES, CSHARP, API, mode=QUERY, parser=parse_surface)
Tool("yak", DIRECT, ("build",), NONE, CSHARP, PACKAGE, mode=STAGE)
Tool("yak", DIRECT, ("install",), NONE, CSHARP, PACKAGE, mode=DEPLOY)
Tool("yak", DIRECT, ("push",), NONE, CSHARP, PACKAGE, mode=PUBLISH)
```

**DOCS** (`mmdc`, `tools/quality/README.md` maintenance):

```python
Tool("mmdc", PNPM, ("mmdc", "-a", ".artifacts/mermaid", "-q"), INCLUDE, DOCS, DOCS)
```

`cs-analyzer` is absent by design: its Roslyn rules execute inside `dotnet-build` via MSBuild analyzer references; `parse_build` surfaces `CSP####` counts from that row's `Completed`.

---
## [2][NO_CONSTANT_SPAM]

Three mechanisms keep rows literal and repetition-free:

1. **Runner prefix lives in the enum, once.** `Runner.__new__` carries the argv prefix payload; `Check.argv` prepends it. `("uv","run")`, `("pnpm","exec")`, `("dotnet",)`, `("uv","run","python","-m")`, `()` appear exactly once each — never per row.

```python
class Runner(StrEnum):
    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    MODULE = "module", ("uv", "run", "python", "-m")   # repo canonical prefix
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "exec")
    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self: ...
```

2. **Input placement is enum behavior, not a per-row string.** `place(routed, tool)` in `core/routing.py` owns where routed paths land (`routing.md` §1). `FILES` appends; `INCLUDE` emits `(project, "--include", *files)` per group; `GLOB` derives suffix globs from `routed.files`; `PROJECT`/`SOLUTION` insert closure targets; `NONE` ignores routing.

3. **Field defaults absorb the common case.** `mode=Mode.CHECK`, `parser=None`, `timeout=None` are defaults; only deviating rows set `Mode.WRITE`, `Mode.BUILD`, parsers, or timeouts. **No factory functions**; a row is a literal `Tool(...)`.

---
## [3][SELECT]

`select` is pure data filtering with no I/O and no dispatch:

```python
def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )
```

`language=None` is the "all languages" request a polyglot rail makes (`static`, `test`, `docs` fan across C#/Python/TS); a C#-only rail passes `Language.CSHARP`. **Determinism:** the explicit sort key `(language, mode, name, command)` makes ordering independent of `TOOLS` authoring order, so reordering rows never changes execution order and the write/check twins sort adjacently. The rail narrows further by mode with a trivial comprehension (`t for t in select(STATIC) if t.mode in (CHECK,)` for `report`, `WRITE` for `fix`) — still pure data, no branch table.

---
## [4][PARSERS]

`parser: Parser | None` where `Parser = Callable[[Completed], Detail | None]` (`model.md` §3). Rows whose evidence is "ran, exit code N" carry `parser=None`; the rail fold uses `RailStatus.from_returncode` and generic counts. Only rows with irregular evidence name a decoder, attached **by reference**:

| [ROW] | [PARSER] | [`detail` VARIANT] |
| --- | --- | --- |
| `py-analyzer` | `parse_findings` | `Match` tuple of `PYS####` rows |
| `dotnet-build` | `parse_build` | `CSP####` counts (cs-analyzer evidence) |
| `dotnet-test` LIST | `parse_tests` | `Match` tuple of test ids |
| `rasm-bridge` VERIFY | `parse_verify` | `VerifySummary` |
| `ilspycmd` QUERY | `parse_surface` | `ApiSurface` |

Parsers are decoders co-located in `core/model.py` as `msgspec.convert`/`defstruct` shapers, not catalog logic; shared shapes (a "lint produced N diagnostics" count) reuse one decoder across rows, so the parser count stays far below the row count.

---
## [5][ONE_ROW_GROWTH]

Add `pyright` — one row, no other file touched:

```python
Tool("pyright", PNPM, ("pyright",), FILES, PYTHON, STATIC)
```

Add a language (Rust) — one `Language.RUST` member, one `core/routing.py` arm resolving `*.rs` via the git change-set, then rows:

```python
Tool("cargo-clippy", DIRECT, ("cargo", "clippy", "--", "-D", "warnings"), NONE, RUST, STATIC)
Tool("cargo-test", DIRECT, ("cargo", "test"), NONE, RUST, TEST, mode=RUN)
```

`select(STATIC)` (no language) now includes clippy with zero rail edits; the `static` rail iterates whatever rows match. This satisfies INVARIANT 3 (program = one row; language = one member + one routing arm + rows).

---
## [6][OPEN_DECISIONS]

1. **Per-row timeout/mode overrides.** `timeout: float | None` and `mode: Mode` are row fields with `None`/`CHECK` defaults; the engine binds `timeout` into the `Check` (`dotnet-stryker` sets `3600.0`). Decision: keep both on the row rather than in settings, because they are program-intrinsic, not environment config. Reconsider only if a value must vary by host.
2. **cs-analyzer placement.** Confirmed inside `dotnet-build`; `parse_build` reads the analyzer diagnostics. No standalone row, matching `ARCHITECTURE.md` §2.
3. **restore vs build.** Modeled as two rows (`RESTORE`, `BUILD`); the `static` rail orders `RESTORE` before `BUILD` by `Mode` and serializes a closure under the build lease (engine concern). Alternative — a single `BUILD` row whose engine implies restore — hides an invocation and is rejected.
4. **Configurations / `--all` target.** Debug/Release expansion and the `Workspace.slnx` (`SOLUTION` input) vs owner-closure (`PROJECT`) choice stay rail+routing concerns (`settings.static_configurations`), not row data, so the C# rows remain target-agnostic.
5. **write/check twins.** Two rows per formatter/linter sharing a `name` but differing by `mode` (`CHECK` vs `WRITE`) and command tail. The rail picks the twin by verb (`fix`→`WRITE`, `report`→`CHECK`).
6. **`ast-grep` dual rows.** `ast-grep-py` and `ast-grep-ts` are the canonical proof that `Language` is a field: identical program, two rows differing only in `--filter`, `language`, and the glob set `GLOB` resolves.
