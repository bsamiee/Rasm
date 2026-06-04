# [H1][ASSAY_OPERATOR]
>**Dictum:** *One claim owns one proof; one Envelope owns stdout.*


[IMPORTANT] `tools.assay` is an **agent-only** CLI. Humans may run it, but the contract targets automated agents parsing JSON. Report `claim`, `verb`, `exit_code`, `run_id`, and artifact paths from the Envelope — never scrape stderr for outcomes.

```bash
uv run python -m tools.assay <claim> <verb> [args]
```

Rails stay orthogonal: `static` never runs tests, `test` never opens Rhino, `bridge` never replaces compile proof, `api` never launches Rhino. Polyglot `static`/`test`/`docs` fold catalog rows; C#-only `bridge`/`package`/`api` own live or bespoke evidence.

[CRITICAL] **Stdout contract:** every verb writes exactly **one** JSON `Envelope` line to stdout. Fields: `claim`, `verb`, `status`, `exit_code`, `run_id`, `duration_ms`, `report`, `error`, `truncated`, `notes`, `schema_version`. `status` is one of `ok | empty | skip | busy | timeout | unsupported | failed`. Proof nests under `report` (`counts`, `artifacts`, `matches`, tagged `detail`) — not a parallel `data`/`evidence` ladder. Process bytes, `dotnet` streams, and structlog diagnostics go to **stderr only**. No raw-text passthrough; no World-A/World-B duality.

Implementation status: spine modules are landing per `IMPLEMENTATION.md`; until parity is proven, fall back to `tools/quality` for the same claim (`tools/quality/README.md`).

---
## [1][PURPOSE]
>**Dictum:** *Replace shape sprawl with one engine and one wire.*


`assay` supersedes `tools/quality` as the monorepo quality operator: C#, Python, TypeScript, and docs proofs through one `Engine`, one `Report`, one Envelope. Programs are `Tool` rows in `composition/catalog.py`, not per-binary Python modules. The design optimizes **dozens of concurrent agents** — disjoint `run_id` artifact trees, read-only polyglot fan-out, and fail-fast leases instead of blocking flock.

Agent docs: `AGENTS.md` (edit routing), `ARCHITECTURE.md` (design), `IMPLEMENTATION.md` (build order), `.design/TYPE_SYSTEM.md` (wire shapes), `.design/AOT.md` (aspect seams).

---
## [2][RAIL_MAP]
>**Dictum:** *Route by proof claim, not habit.*

Diagram: `ARCHITECTURE.md` §2. Flow: argv → `registry` → `routing` + `catalog` → `run_check`/`fan_out` → `Report` → one `Envelope`.

| [INDEX] | [CLAIM]   | [VERBS] (target parity)                                      | [OWNERSHIP]                     |
| :-----: | --------- | ------------------------------------------------------------ | ------------------------------- |
|   [1]   | `static`  | `fix`, `report`, `build`, `full`, `plan`                     | Format, lint, analyze, compile. |
|   [2]   | `test`    | `run`, `list`, `coverage`                                    | Unit, discovery, coverage.      |
|   [3]   | `bridge`  | `verify`, `check`, `doctor`, `launch`, `quit`, `build-bridge`, … | Live RhinoWIP evidence.         |
|   [4]   | `package` | `package`, `plan`, `list`, `deploy`, `publish`               | Yak lifecycle.                  |
|   [5]   | `api`     | `doctor`, `resolve`, `query`, `show`                         | Host and package API truth.     |
|   [6]   | `docs`    | `check` (Mermaid via `mmdc`)                                 | Markdown diagram validation.    |

Use the Python module entrypoint only. Do not add package-manager aliases for this operator.

---
## [3][COMMAND_SURFACE]
>**Dictum:** *Verb names encode cost; flags stay on frozen Params objects.*

Run from any worktree path; `AssaySettings.anchor()` finds `Workspace.slnx`. Cyclopts flattens frozen `@dataclass` params (`Parameter(name="*")`) — no flag spam; use defaults and `ASSAY_*` env.

```bash
uv run python -m tools.assay static fix libs/csharp/Rasm
uv run python -m tools.assay static build tools/assay
uv run python -m tools.assay test run --target tests/csharp/libs/Rasm/Rasm.Tests.csproj
uv run python -m tools.assay bridge verify tests/csharp/libs/Rasm/Vectors/scenarios
```

---
## [4][AGENT_ROUTING]
>**Dictum:** *Pick the narrowest claim that owns the proof.*


| [SITUATION]                         | [USE]                                      | [AVOID]                          |
| ----------------------------------- | ------------------------------------------ | -------------------------------- |
| C# format / style on touched files  | `static fix` then `static build`           | Raw `dotnet format` as compile proof |
| Central package / slnx / analyzer   | `static full`                              | Leaf `.slnx` builds for every edit |
| Python / TS lint and types          | `static` rows (replaces `pnpm check:py/ts`) | Shell `package.json` gates       |
| Unit proof after green build        | `test run --no-build`                      | Implicit mutation on every run   |
| Rhino scenario / GH smoke           | `bridge verify` / `bridge check`           | xUnit for host-runtime facts     |
| Host SDK member truth               | `api query` / `api doctor`                 | Guessing from stale snippets     |
| Diagram in Markdown                 | `docs check`                               | `static build`                   |

When `status` is `busy` (exit `5`), **do not wait** on locks — retry another claim, another closure, or another agent slot. Orchestrators should treat busy as capacity signal, not failure.

---
## [5][CONCURRENCY_AND_LEASES]
>**Dictum:** *Isolation by path; exclusivity by fail-fast lease.*


| [RESOURCE]        | [LOCK]                              | [PARALLELISM]                    |
| ----------------- | ----------------------------------- | -------------------------------- |
| MSBuild closure   | `locks/build-<closure>.lock`        | One winner per closure hash      |
| Stryker mutation  | `locks/mutation.lock`               | Global exclusive                 |
| Live Rhino        | `locks/bridge.lock`                 | Global exclusive                 |
| Yak stage         | `locks/package-stage.lock`          | Per stage directory              |
| Read-only checks  | none                                | `fan_out` + distinct `run_id`    |

Paths: `.artifacts/assay/<claim>/<run_id>/` (scope + `DOTNET_CLI_HOME`), `.artifacts/assay/build/<closure>/` (warm MSBuild), `.artifacts/test/<slice>/<run_id>/`, `.artifacts/rhino/verify/<run_id>/`, fingerprinted API surface cache.

`run_id` defaults to timestamp + pid; set `ASSAY_RUN_ID` for CI correlation. `@retried` never retries `busy` — see `.design/AOT.md`.

At scale: hundreds of agents on **read-only** `static report` / `api query` / `docs check` scale with disk and bounded fan-out; hundreds on the **same** `static build` closure yield `busy` — shard by project or queue closures.

---
## [6][MIGRATION_FROM_QUALITY]
>**Dictum:** *Same claims, cleaner wire, broader languages.*


| [QUALITY]                              | [ASSAY]                                      |
| -------------------------------------- | -------------------------------------------- |
| `uv run python -m tools.quality static …` | `uv run python -m tools.assay static …`      |
| `Envelope.rail` / `data` / `evidence`  | `claim` / `report` / artifacts in `report`   |
| `.artifacts/quality/<rail>/<run_id>/`    | `.artifacts/assay/<claim>/<run_id>/`         |
| C# + separate `check:py` / `check:ts`   | Polyglot catalog rows under `static`/`test`  |
| Eight-projector `rail()`                 | One `_emit` + `Report` fold                  |

Retain `tools/quality` until assay registry reaches command parity (`AUDIT.md` §4). Bridge scenario semantics stay in `tools/rhino-bridge/README.md`; only the operator module path changes.

---
## [7][MAINTENANCE]
>**Dictum:** *Validate the surface you edit.*

Python edits: `.claude/skills/coding-python/SKILL.md`. v1 deps in root `pyproject.toml`: `msgspec`, `cyclopts`, `structlog`, `beartype`, `opentelemetry-*`, `stamina`. Gated: `watchfiles`, `psutil`, `fsspec`.

```bash
uv run ruff check tools/assay && uv run ty check tools/assay
uv run pytest tests/tools/assay -q
git diff --check
uv run python -m tools.assay static plan
```
