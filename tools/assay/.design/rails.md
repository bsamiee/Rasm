# [H1][ASSAY_RAILS_DESIGN]
>**Dictum:** *A rail is a fold over a claim; evidence is data in `Report.detail`, never a new return shape.*

Every handler in `rails/` is `handler(settings, scope, params) -> Result[Report, Fault]`. No rail re-encodes;
no rail branches on language (routing owns that); no rail calls a cross-cutting library inline (aspects own that).
The `tools/quality` ladder — `StaticOutcome`, `TestRunReport`, `VerifyReport`, `Package*Report`, `Api*Report`,
raw `bytes` payloads — collapses into one `Report` whose `detail` is a `msgspec` tagged union keyed by `kind`.

**Canonical:** [`snippets/model-status.py.md`](snippets/model-status.py.md) (`fold`, `Completed.status`/`notes` only — no `Completed.artifacts`) · [`engine.md`](engine.md) (`fan_out`, `run_check`) · [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) §4. **Superseded:** `bind_check`, `Engine.run_all`, per-rail fold counting `Completed.status.exit_code` (old §1 prose).

---
## [1][COMMON_FOLD]
>**Dictum:** *Select rows, route inputs, bind checks, run once, fold once.*

The thin rails are one function parameterized by `claim` (and an optional `detail` projector). The fold is written
ONCE; `static`, `test`, `docs` differ only in the `Claim` member they select and the verb-shaped `Mode` they carry.

```python
def thin_rail(settings, scope, params, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    return route(params.language, params.paths).bind(
        lambda routed: fan_out(
            tuple(_check_from(t, routed) for t in select(claim, params.language) if t.mode is mode),
            settings=settings,
        )
    ).map(lambda outcomes: fold(claim, verb, tuple(o for r in outcomes for o in (r,) if isinstance(o, Completed)),
                                 parser=_merge_parser(select(claim))))
```

`fan_out` + `run_check` are the only executors (`engine.md` §3). Each spawn returns `Result[Completed, Fault]`; operational failures are `Fault`, process exits are `Completed` with `status` from `receipt`. **`fold`** is the module function in `core/model.py` (snippet §1) — not reimplemented per rail:

```python
# snippets/model-status.py.md — authoritative fold algebra
def fold(claim, verb, outcomes: tuple[Completed, ...], *, detail=None, parser=None) -> Report: ...
```

| [RULE]      | [FOLD SEMANTICS]                                                                              |
| ----------- | -------------------------------------------------------------------------------------------- |
| `status`    | `RailStatus.fold(*(o.status for o in outcomes))`; seed semantics in `status.md` §3.          |
| `exit_code` | From folded `Report.status.exit_code` only; rails never compute exit codes.                   |
| `counts`    | `_count(done)` per `Completed.status` on `Report.counts` — not on `Detail` variants.          |
| `artifacts` | Rail handler projects paths into `Report.artifacts` (not stored on `Completed`).            |
| `results`   | Parser / rail projects `Match` rows into `Report.results`.                                    |
| `detail`    | `None` for thin rails; one tagged variant for bespoke rails (§3).                             |

---
## [2][THIN_RAILS]
>**Dictum:** *Same fold, different claim; verbs select `Mode`, never a code path.*

`static`, `test`, `docs` are the common fold with `claim` fixed and `params.mode` selecting catalog rows and engine
posture. Multi-language reach is entirely in `routing.route(language, paths)` and `select(claim, language)`; the
handler body is language-blind.

| [RAIL]   | [CLAIM]  | [VERBS / `Mode`]                | [SELECTED ROWS (per `Language`)]                                              | [NOTES]                                                  |
| -------- | -------- | ------------------------------- | ----------------------------------------------------------------------------- | -------------------------------------------------------- |
| `static` | `STATIC` | `fix`, `report`, `build`, `plan` | C#: `dotnet format` (`Input.INCLUDE`, `Mode.WRITE`) + `dotnet build` closure; Py: `ruff`, `ty`, `mypy`, `ast-grep`, `py_analyzer`; TS: `tsc`, `biome`, `knip`, `sherif`, `ast-grep`. | `fix` selects `Mode.WRITE` rows; `report`/`build`/`plan` select `CHECK`/`BUILD`. `plan` runs zero checks — folds resolved argv into `notes`/`artifacts`. `build` closure + `--artifacts-path` lease per §4. |
| `test`   | `TEST`   | `run`, `list`, `coverage`        | C#: `dotnet test` (MTP), `dotnet-stryker` (mutation, opt-in); Py: `pytest`; TS: `vitest`.    | Target selection (`--target`/`--all`/`--test-modules`) is a routing/param concern; mutation is a separate row gated by `Mode` + eligibility (§6). |
| `docs`   | `DOCS`   | `check`                         | Docs: `mmdc` Mermaid validation (`Language.DOCS`).                            | `Input.GLOB` over changed `*.md`; pure read-only fan-out. |

Verb modeling stays uniform: a verb is a `Mode` enum member carried in `params`, consumed by `select` (which rows)
and by `run_check` (`Mode.stream` / `Mode.writes` on the row). `static plan` is the degenerate fold — it binds the same
rows but emits their resolved argv as `Artifact`/`notes` instead of running them, so the plan payload is not a
bespoke `StaticPlanReport` but the common `Report` with `status=ok`.

---
## [3][BESPOKE_CSHARP_RAILS]
>**Dictum:** *Bespoke is legitimate only when the proof is typed evidence, not a different status shape.*

`bridge`, `package`, `api` reuse the SAME handler signature, the SAME `Engine`, and the SAME `RailStatus`/`Counts`
fold. They are bespoke ONLY in that they attach exactly one `Report.detail` variant carrying algorithm-specific,
route/status/sampling evidence that a flat `Match` stream cannot encode. They are C#-only: `select` yields no rows
for other languages, so routing never multiplexes them.

| [RAIL]    | [`Report.detail` VARIANT]              | [TYPED EVIDENCE (why bespoke)]                                                                 | [DRIVING ROW(S) / LEASE]                          |
| --------- | -------------------------------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| `bridge`  | `VerifySummary` (`tag="verify"`)        | Per-scenario `name`, `RailStatus`, `report_path`, fold-derived `facts`/`captures` (regex-scanned scenario stdout), `exception_reports`, `first_failure`, retention TTL. Encodes route+status+sampling proof. | `bridge` client `dotnet run` rows; one process-global `bridge.lock` exclusive lease over discover→build→launch→run→summary. |
| `package` | `PackageRun` (`tag="package"`)          | `slug`, evaluated MSBuild `PackageMeta` (yak platform/path/pattern), atomic `stage` path, ordered `PackageStepKind` policy (`quit`/`install`/`refresh`/`push`). Encodes a staged lifecycle, not a set of matches. | `dotnet build` + `yak` rows; per-`package_dir` `package-stage` lease for cleanup→commit; `quit`/`refresh` steps re-enter the `bridge` lease. |
| `api`     | `ApiSurface` (`tag="api"`) + `Raw` escape | `source` (host vs NuGet, version, assembly/xml), `shape` (`index`/`namespace`/`type`/`member`/`search`), `signature`, `doc`, ranked `Match`. External `ilspycmd`/MSBuild JSON decoded via `msgspec` (`_MsbuildProps`-style structs); irregular host JSON lands in a `Raw`/`defstruct` variant. | `ilspycmd` (`-l cisde` surface, `-t` decompile), `dotnet msbuild`/`restore` rows; read-only, NO lease, NO Rhino launch. Surface cached per (key, assembly fingerprint). |

What is NOT a license to be bespoke: a differing status string (use `RailStatus`), a `dict` artifact bag (use
`tuple[Artifact, ...]`), or a verb-shaped variation (use `Mode`). `api`'s former `ApiQueryReport`/`ApiDoctorReport`
tagged union becomes the single `ApiSurface` detail plus the common `Report.results`/`artifacts`; external decode
stays at the boundary (`msgspec.convert(..., strict=False)`), and only genuinely irregular host payloads use the
sanctioned `defstruct`/`Raw` escape rather than a hand-written one-off struct.

---
## [4][LEASE_AND_CONCURRENCY]
>**Dictum:** *Read-only fans out; exclusive resources fail fast to `busy`, never block.*

Leases live in the Engine, not in rails. `run_check` / `fan_out` inspect each `Tool`'s declared resource and thread
`exclusive_lease` around the contended segment; a held lease yields `RailStatus.BUSY` (exit 5) immediately.

| [RESOURCE]            | [LEASE]                         | [RAILS]                              | [CONTENTION]                          |
| --------------------- | ------------------------------- | ------------------------------------ | ------------------------------------- |
| Live `RhinoWIP.app`   | `bridge.lock` (process-global)   | `bridge`; `package` `quit`/`refresh` | Non-blocking → `busy`/exit 5.          |
| Build closure         | `build-<closure>.lock`           | `static build`/`full`                | Same closure serializes; distinct closures run concurrently under isolated `--artifacts-path`. |
| Stryker mutation      | `mutation.lock`                  | `test --mutation changed\|full`      | Non-blocking → `busy`/exit 5.          |
| Yak stage dir         | `package-stage.lock`             | `package`                            | Per-`package_dir`; cleanup→commit serialized. |
| Read-only metadata    | none                            | `static report`/`plan`, `test list`, `docs`, `api` | Fan out under `anyio`; distinct `run_id` per invocation. |

Each invocation opens one artifact scope `.artifacts/assay/<rail>/<run_id>/` with isolated `DOTNET_CLI_HOME`, so
read-only rails are concurrency-safe by construction; only the table's exclusive resources take a lease.

---
## [5][NEW_RAIL_RECIPE]
>**Dictum:** *A new rail is one registry row, one handler, and at most one `detail` tag.*

1. Add one `REGISTRY` row mapping `claim x verb -> handler` (`composition/registry.py`); the runner supplies scope,
   aspects, and the `Envelope` wrap.
2. Write `rails/<name>.py` as the §1 fold with a new `Claim` member; add catalog rows for any new program and, for a
   new language, one `Language` member plus one `routing.route` arm.
3. If — and only if — the proof is typed evidence beyond `Match`/`Artifact`, add ONE `Detail` variant (`tag=<name>`,
   `forbid_unknown_fields`) and project it in the fold; otherwise return `detail=None` and lean on `results`.

No new status type, no new return shape, no `Engine` change, no per-binary module.

---
## [6][OPEN_DECISIONS]

| [DECISION]               | [OPTIONS]                                                                                              | [LEAN]                                                                 |
| ------------------------ | ----------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Verb modeling per rail   | (a) one shared `Mode` enum across rails; (b) per-rail `Mode` enums.                                    | (a) one `Mode` with payload-carrying members; `select` filters by `(claim, mode)`. |
| Mutation opt-in          | (a) mutation as a distinct catalog row gated by `Mode.MUTATION` + eligibility preflight; (b) a `test run` flag folded into the row. | (a) — keeps `test run` unit-only by default; mutation row only selected when explicitly requested, preserving `tools/quality` discipline. |
| Partial fan-out failure  | (a) fold mixed outcomes into worst-status `Report` (continue all); (b) cancel the task group on first `Fault`. | (a) for read-only diagnostics (collect every signal); (b) for staged/exclusive rails (`package`, `bridge`) where a failed step invalidates successors. Encode the choice as a `Mode`/`Tool` flag, not a branch in the fold. |
| `api` external decode    | (a) typed `msgspec` structs per known JSON; (b) `Raw`/`defstruct` for irregular host payloads.          | Both — typed at the boundary, `Raw` only for genuinely irregular shapes. |
