# [H1][ASSAY_CATALOG]
>**Dictum:** *Every external program is one dense `Tool` row; a new program adds a row, a new language adds one member + one routing arm + rows — never a module, type, or function.*

## [1][PURPOSE]

`composition/catalog.py` (stage 7, depends on `core/model.py` only) owns `TOOLS: tuple[Tool, ...]` — the full polyglot row set across C#, Python, TypeScript, Bash, SQL, Docs — plus `select(claim, language)` and the six stdout parsers. Variance is carried entirely by the five axis enums of ARCHITECTURE §3 (`Runner`, `Input`, `Language`, `Mode`, `Claim`); the catalog body is data, not control flow. The `parse_*` callables and their internal `msgspec.Struct` decode schemas live **in this file** and attach to the deviating rows **by reference** (D22), never as registry-key strings; only the evidence-variant types (`TestRun`/`VerifySummary`/`ApiSurface`) and the axis/row types are imported from `core/model.py`. This file is the single locus where a program's identity (runner, command tail, input placement, language, claim, mode, optional parser/timeout) is declared.

## [2][CANONICAL_SHAPES]

The row shape is fixed by ARCHITECTURE §4 — defined verbatim, with the load-bearing field order and defaults (D4 unified `Mode`, D5 default `Mode.CHECK`, D22 `Parser` by reference):

```python
class Tool(Base, frozen=True):
    name: str                          # write/check twins share name; (name, mode) is the twin identity
    runner: Runner                     # argv prefix payload (Runner.prefix)
    command: tuple[str, ...]           # program + static flags; routed paths appended by place()
    input: Input                       # placement strategy for routed paths
    language: Language                 # routing strategy + suffixes
    claim: Claim                       # owning rail
    mode: Mode = Mode.CHECK            # stream/writes payload + operation kind
    parser: Parser | None = None       # Callable[[Completed], AnyDetail | None], attached by reference
    timeout: float | None = None       # program-intrinsic deadline (e.g. dotnet-stryker)
```

`Parser = Callable[[Completed], AnyDetail | None]` (PEP 695 alias, D22 — no `Engine`/`Parser` `Protocol`). The `AnyDetail` tagged union (`TestRun`/`VerifySummary`/`ApiSurface`, short tags `test`/`verify`/`api`, D13) is the parser output; `Completed` and `AnyDetail` import under `TYPE_CHECKING` (forward-ref only — no runtime cost). Rows whose evidence is "ran, exit code N" carry `parser=None` and the rail folds via `RailStatus.from_returncode` (D12). No factory functions — a row is a literal `Tool(...)`; the `Runner.prefix` payload (`MODULE=("uv","run","python","-m")` per D7) is the only place argv prefixes repeat.

## [3][VALIDATED_SNIPPET]

The block below is the canonical CORE pattern of the module: the in-file parser (its `msgspec` decode schema + the by-reference callable that returns either an `AnyDetail` evidence variant or `None`), a representative `TOOLS` slice carrying the `(name, mode)` write/check twins and a parser row, and the deterministic `select`. Imports, schemas, and parsers match the final signatures exactly; the full 39-row census is §3.1.

```python
from typing import TYPE_CHECKING

import msgspec

from tools.assay.core.model import (  # noqa: PLC2701  # intra-package import
    ApiSurface, Claim, Input, Language, Mode, Runner, SourceKind, SymbolShape, TestRun, Tool, VerifySummary,
)

if TYPE_CHECKING:
    from tools.assay.core.model import AnyDetail, Completed  # forward-ref only; no runtime import

# --- [MODELS] (decode schemas are private to the parser that owns them) ------------------
class _TestTelemetry(msgspec.Struct, frozen=True, gc=False):
    mutation: str = "off"
    coverage: float | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0

# --- [CONSTANTS] -------------------------------------------------------------------------
_TESTS = msgspec.json.Decoder(_TestTelemetry)
DIRECT, MODULE, UV, DOTNET, PNPM = Runner.DIRECT, Runner.MODULE, Runner.UV, Runner.DOTNET, Runner.PNPM
FILES, INCLUDE, PROJECT, SOLUTION, GLOB, NONE = Input.FILES, Input.INCLUDE, Input.PROJECT, Input.SOLUTION, Input.GLOB, Input.NONE
PY, TS, CS, BASH, SQL, DOCS = Language.PYTHON, Language.TYPESCRIPT, Language.CSHARP, Language.BASH, Language.SQL, Language.DOCS

# --- [OPERATIONS] (parser: decode + project, or validate-only -> None) -------------------
def parse_tests(done: "Completed") -> "AnyDetail | None":
    t = _TESTS.decode(done.stdout or b"{}")
    return TestRun(mutation=t.mutation, coverage=t.coverage, killed=t.killed, survived=t.survived, selected=t.selected)

def parse_build(done: "Completed") -> "AnyDetail | None":
    _ = done  # CSP#### are exit-code evidence (from_returncode), not JSON; no variant constructed
    return None

# --- [TABLES] ----------------------------------------------------------------------------
TOOLS: tuple[Tool, ...] = (
    Tool("ruff", UV, ("ruff", "check"), FILES, PY, Claim.STATIC),
    Tool("ruff", UV, ("ruff", "check", "--fix"), FILES, PY, Claim.STATIC, mode=Mode.WRITE),
    Tool("dotnet-build", DOTNET, ("build", "--no-restore", "-v:quiet", "/clp:ErrorsOnly"), PROJECT, CS, Claim.STATIC, mode=Mode.BUILD, parser=parse_build),
    Tool("dotnet-test", DOTNET, ("test", "--list-tests"), PROJECT, CS, Claim.TEST, mode=Mode.LIST, parser=parse_tests),
    Tool("dotnet-stryker", DOTNET, ("tool", "run", "dotnet-stryker", "--", "--test-runner", "mtp", "--mutation-level", "Standard"), PROJECT, CS, Claim.TEST, mode=Mode.MUTATION, timeout=3600.0),
    Tool("shellcheck", DIRECT, ("shellcheck", "-f", "json1"), FILES, BASH, Claim.STATIC, parser=parse_shellcheck),
    Tool("sqlfluff", UV, ("sqlfluff", "lint", "--dialect", "postgres"), FILES, SQL, Claim.STATIC),
    Tool("yak", DIRECT, ("yak", "build"), NONE, CS, Claim.PACKAGE, mode=Mode.STAGE),
    Tool("mmdc", PNPM, ("mmdc", "-a", ".artifacts/mermaid", "-q"), INCLUDE, DOCS, Claim.DOCS),
)

def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )
```

`select` is pure data filtering: no I/O, no dispatch, no branch. The explicit key `(language.value, mode.value, name, command)` makes execution order independent of authoring order, sorts the `(name, mode)` write/check twins adjacently, and groups by language for fan-out scheduling. `language=None` is the polyglot request (`static`/`test`/`docs` fold across every language); a C#-only rail (`bridge`/`package`/`api`) passes `Language.CSHARP`. The rail narrows by mode with a trivial comprehension (`t for t in select(Claim.STATIC) if t.mode is Mode.CHECK` for `report`, `Mode.WRITE` for `fix`) — still data, no table. Each parser either decodes-and-projects to an `AnyDetail` variant or validates-the-schema-and-returns-`None` (the rail then folds via `from_returncode`).

### [3.1][FULL_INVENTORY]

Every catalog row, one per line, by `(language × claim × mode)`; 39 rows, complete. `[PARSER]` is the by-reference callable (D22) or `—` for exit-code evidence (`from_returncode`, D12). `[CFG]` marks in-repo configuration status: `[X]` configured, `[CFG-PEND]` row specced day-one but linter/binary not yet configured in-repo (D36). Twins share `name`, discriminated by `(name, mode)` (§6).

| [LANG] | [NAME] | [RUNNER] | [CLAIM] | [MODE] | [INPUT] | [PARSER] | [CFG] |
| ------ | ------ | -------- | ------- | ------ | ------- | -------- | :---: |
| Python | `validate-pyproject` | UV | STATIC | CHECK | PROJECT | — | [X] |
| Python | `ruff` | UV | STATIC | CHECK | FILES | — | [X] |
| Python | `ruff` | UV | STATIC | WRITE | FILES | — | [X] |
| Python | `ruff-format` | UV | STATIC | CHECK | FILES | — | [X] |
| Python | `ruff-format` | UV | STATIC | WRITE | FILES | — | [X] |
| Python | `ty` | UV | STATIC | CHECK | INCLUDE | — | [X] |
| Python | `mypy` | UV | STATIC | CHECK | INCLUDE | — | [X] |
| Python | `ast-grep-py` | PNPM | STATIC | CHECK | GLOB | — | [X] |
| Python | `ast-grep-py` | PNPM | TEST | VERIFY | FILES | — | [X] |
| Python | `py-analyzer` | MODULE | STATIC | CHECK | NONE | `parse_findings` | [X] |
| Python | `pytest` | UV | TEST | RUN | INCLUDE | — | [X] |
| Python | `pytest-deadfixtures` | UV | TEST | LIST | INCLUDE | — | [X] |
| Python | `pytest-benchmark` | UV | TEST | RUN | INCLUDE | — | [X] |
| Python | `coverage` | UV | TEST | RUN | INCLUDE | — | [X] |
| Python | `mutmut` | UV | TEST | MUTATION | PROJECT | — | [X] |
| TypeScript | `tsc` | PNPM | STATIC | BUILD | PROJECT | — | [X] |
| TypeScript | `biome` | PNPM | STATIC | CHECK | GLOB | — | [X] |
| TypeScript | `knip` | PNPM | STATIC | CHECK | PROJECT | — | [X] |
| TypeScript | `sherif` | PNPM | STATIC | CHECK | PROJECT | — | [X] |
| TypeScript | `ast-grep-ts` | PNPM | STATIC | CHECK | GLOB | — | [X] |
| TypeScript | `ast-grep-ts` | PNPM | TEST | VERIFY | FILES | — | [X] |
| TypeScript | `vitest` | PNPM | TEST | RUN | NONE | — | [X] |
| C# | `dotnet-format` | DOTNET | STATIC | CHECK | INCLUDE | — | [X] |
| C# | `dotnet-format` | DOTNET | STATIC | WRITE | INCLUDE | — | [X] |
| C# | `dotnet-restore` | DOTNET | STATIC | RESTORE | PROJECT | — | [X] |
| C# | `dotnet-build` | DOTNET | STATIC | BUILD | PROJECT | `parse_build` | [X] |
| C# | `dotnet-test` | DOTNET | TEST | RUN | PROJECT | — | [X] |
| C# | `dotnet-test` | DOTNET | TEST | LIST | PROJECT | `parse_tests` | [X] |
| C# | `dotnet-stryker` | DOTNET | TEST | MUTATION | PROJECT | — | [X] |
| C# | `rasm-bridge` | DOTNET | BRIDGE | VERIFY | PROJECT | `parse_verify` | [X] |
| C# | `ilspycmd` | DOTNET | API | QUERY | NONE | `parse_surface` | [X] |
| C# | `yak` | DIRECT | PACKAGE | STAGE | NONE | — | [X] |
| Bash | `shellcheck` | DIRECT | STATIC | CHECK | FILES | `parse_shellcheck` | [CFG-PEND] |
| Bash | `shfmt` | DIRECT | STATIC | CHECK | FILES | — | [CFG-PEND] |
| Bash | `shfmt` | DIRECT | STATIC | WRITE | FILES | — | [CFG-PEND] |
| SQL | `sqlfluff` | UV | STATIC | CHECK | FILES | — | [CFG-PEND] |
| SQL | `sqlfluff` | UV | STATIC | WRITE | FILES | — | [CFG-PEND] |
| SQL | `squawk` | UV | STATIC | CHECK | FILES | — | [CFG-PEND] |
| Docs | `mmdc` | PNPM | DOCS | CHECK | INCLUDE | — | [X] |

Census: 39 rows, complete — every `package.json` quality step and every `tools/quality` verb's orchestrated *program* maps to a present, correct row. Six rows carry a by-reference parser (`parse_findings`/`parse_build`/`parse_tests`/`parse_verify`/`parse_surface`/`parse_shellcheck`); the rest fold via `from_returncode` (D12). The six `[CFG-PEND]` Bash/SQL rows are day-one per D36: their linters (`shellcheck`/`shfmt`/`sqlfluff`/`squawk`) are not yet configured in-repo (the dossier confirms zero `.shellcheckrc`/`.sqlfluff`/`.squawk` config and zero orchestrated `*.sh`/`*.sql` files), so `api doctor` reports a missing binary as `FAULTED` (exit 2) and an empty change-set folds to `EMPTY` (§6). The bespoke `api resolve`/`show` and `bridge`/`package` lifecycle modes are not `select`-folded catalog data (D35) — they live in `rails/{bridge,package,api}.py`, not `TOOLS`.

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| ---------- | :---------: | ---------- |
| `core/model.py` | imports | `Tool`/`Completed`/`AnyDetail`, the five axis enums, `SourceKind`/`SymbolShape`, and the evidence variants `TestRun`/`VerifySummary`/`ApiSurface`. Sole dependency; the `parse_*` callables themselves are defined in `catalog.py`. |
| `core/routing.py` | consumed by rail | `place(routed, tool, *, settings)` (D23) reads `tool.input`/`tool.command` to project routed paths onto the argv tail; catalog never places paths itself. |
| `core/engine.py` | consumed by rail | `Check` binds a `Tool` + routed scalars; `run_check` prepends `tool.runner.prefix`, honors `tool.mode.stream`/`writes` and `tool.timeout`, and invokes `tool.parser` on the `Completed`. |
| `rails/{static,test,docs}.py` | callers | `thin_rail` calls `select(claim, language)` then folds; per-verb behavior is `Mode` + `Params`, not a function (REGISTRY §13). |
| `rails/{package,api}.py` | callers | C#-only rails `select(claim, Language.CSHARP)` for yak (D35) / ilspycmd rows and emit a bespoke `PackageRun`/`ApiSurface` detail. |

Parser routing (D22; the `parse_*` callables and their `msgspec` decode schemas are in-file, not imported): `py-analyzer`→`parse_findings` (validates `PYS####` `_Finding` rows, returns `None` — `Match` projection rides the rail), `dotnet-build`→`parse_build` (`CSP####` are exit-code evidence — no JSON, returns `None`), `dotnet-test` LIST→`parse_tests` (`TestRun`), `rasm-bridge` VERIFY→`parse_verify` (`VerifySummary`), `ilspycmd` QUERY→`parse_surface` (`ApiSurface`), `shellcheck`→`parse_shellcheck` (validates `_ShellMessage` rows, returns `None`). All other rows carry `parser=None`.

## [5][EXTENSIBILITY]

Add a program — one literal row (e.g. `Tool("pyright", PNPM, ("pyright",), FILES, PY, Claim.STATIC)`); `select` includes it with zero rail edits (INVARIANT 4). Add a language — one `Language` member + one `core/routing.py` arm resolving its suffixes + rows; no rail signature changes. Add a new evidence shape — one `AnyDetail` variant in `core/model.py` plus an in-file `parse_*` callable named on its row.

## [6][CONSIDERATIONS]

- **`(name, mode)` is the twin identity, not `name` alone.** ruff/dotnet-format/shfmt/sqlfluff each declare a `CHECK` and a `WRITE` row sharing `name`; the `select` sort key places them adjacently and the rail's `fix`/`report` verb picks the twin by `Mode` — so a future de-dup pass keying solely on `name` would silently drop the write twin. `ruff` (`("ruff","check")`) and `ruff-format` (`("ruff","format")`) are two distinct programs that differ by `name` *and* command, not a duplicate.
- **Parsers and their decode schemas are in-file (D22).** Each of the six `[PARSER]` entries is a module-local `Callable[[Completed], AnyDetail | None]` whose `msgspec.Struct` schema is private to it; the catalog holds the callable identity, not a lookup token, so a renamed decoder is a type error at import, not a silent miss. Two of the six (`parse_findings`, `parse_shellcheck`) validate-and-return-`None` because no `AnyDetail` variant models per-finding/per-comment rows — that `Match` evidence projects onto `Report.results` at the rail. `parse_build` returns `None` because `CSP####` is free-form exit-code evidence, not JSON.
- **SQL/Bash rows are specced day-one but config-pending (`[CFG-PEND]`).** Per D36 the rows ship regardless so the sixth-language extension cost stays one row + one routing arm. `api doctor` treats a missing orchestrated binary as `FAULTED` (operational, exit 2) rather than `FAILED`, so a fresh checkout that lacks `squawk` does not masquerade as a lint defect; the `static report` fold over an empty SQL change-set yields `EMPTY` (exit 0) and never touches the binary. Configuring a linter is a config-file landing, not a catalog edit.
- **The ast-grep expect-no-match row inverts semantics at the rail, not the catalog.** The fail-case acceptance row (`tests/tools/ast-grep/fail`) is modeled `Mode.VERIFY`/`Claim.TEST` with `Input.FILES`: a non-zero process exit is the *success* signal. Encoding that inversion as catalog data would corrupt the uniform `from_returncode` contract; instead the `test` rail's VERIFY fold maps "rules fired" → `OK`. The `fd helpers|utils|… | rg .` anti-naming check is likewise rail logic, not a row (no `GREP`/`FD` runner exists; inverted-exit checks live in the fold). Per-hook `mypy` (`MYPYPATH=… mypy <file>`) is input-placement fan-out the routing layer threads — one batch row, many invocations — not a second catalog row.
