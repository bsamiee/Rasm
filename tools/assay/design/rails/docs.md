# [H1][DOCS_RAIL]
>**Dictum:** *Docs is a thin fold over the shared `thin_rail`: one `mmdc` row, glob-routed over changed Markdown/Mermaid, never a bespoke surface.*

<br>

## [1][PURPOSE]

`rails/docs.py` owns the lone `docs check` verb. It is a *thin* rail: no `Detail` variant, no lease, no executor logic. It binds `Claim.DOCS` to the shared `thin_rail(settings, scope, params, *, claim, verb, mode)`, which routes inputs, selects the `mmdc` catalog row, fans it out read-only through the Engine, and folds outcomes into one `Report`. The single driven program is `mmdc` (`@mermaid-js/mermaid-cli`) — rendering a diagram source *is* its validation: a parse/layout failure is a non-zero exit, i.e. `Completed{status=FAILED}`, never a `Fault`. Markdown files route only to harvest their embedded Mermaid fences; there is no separate prose-linter row. `Language.DOCS` is a day-one routing arm with `strategy="glob"`.

## [2][CANONICAL_SHAPES]

This doc owns `DocsParams` and the `Language.DOCS` row contract; every other shape is reused verbatim from `core/model.py` and `composition/catalog.py`. `DocsParams` is a frozen `kw_only` `@dataclass` (`TID251` bans `NamedTuple`; Cyclopts flattens via `Parameter(name="*")`) that **extends `BaseParams`** — it inherits `paths`/`language` and adds only `strict`, a rail-level promotion flag, never a row field.

```python
# --- [MODELS] ---------------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class DocsParams(BaseParams):  # inherits paths: tuple[str, ...] = (), language: Language | None = None
    strict: bool = False  # promotes EMPTY/SKIP -> FAULTED Fault at the registry seam; a flag, not a status member
```

`Language.DOCS` carries `strategy="glob"` + `suffixes=frozenset((".md", ".mmd"))`; the routing arm globs **changed** `*.md`/`*.mmd` rather than walking a project closure. The single catalog row is a positional `Tool` msgspec struct (owned by `catalog.py`, restated here for the seam):

| [FIELD] | [VALUE] | [WHY] |
| ------- | ------- | ----- |
| `name` | `"mmdc"` | `@mermaid-js/mermaid-cli`. |
| `runner` | `Runner.PNPM` | prefix `("pnpm","exec")`. |
| `command` | `("mmdc","-a",".artifacts/mermaid","-q")` | autoload config dir + quiet; input completed by `place`. |
| `input` | `Input.INCLUDE` | `flag=("--include",)`, `scoped=False`. |
| `language` | `Language.DOCS` | `strategy="glob"`. |
| `claim` | `Claim.DOCS` | this rail. |
| `mode` | `Mode.CHECK` (struct default; `stream=False`, `writes=False`) | render-to-validate is read-only fan-out. |
| `parser` | `None` (struct default) | no `Detail`; render failure rides `from_returncode` → `FAILED`. |

The fold produces a plain `Report{claim=DOCS, verb="check", status, counts, artifacts, results, detail=None}`. Rendered SVG/PNG outputs project into `Report.artifacts` as `Artifact{id, kind, path, bytes, lines}` (`ArtifactKind` enum, no free string); failed-diagram references project into `Report.results` as `Match` rows. No `VerifySummary`/`ApiSurface`/`PackageRun` here — those are bespoke-rail evidence and out of scope for docs.

## [3][VALIDATED_SNIPPET]

`docs.check` is a one-line adapter; the canonical core is the shared `thin_rail` fold and its two pure projectors `_outcomes`/`_done` plus the `_strict` promoter. `thin_rail` is a **plain** function returning `Result[Report, Fault]` (no `@effect.result` — it has no `yield`); `match` is statement form only; `Fault` carries `{argv, status, message}` ONLY. `route` returns its `Ok` change-set through `.map` (not `.bind` — the tail is pure), and `fan_out` returns an ordered `tuple[Result[Completed, Fault], ...]`, one slot per `Check`.

```python
# --- [OPERATIONS] -----------------------------------------------------------------------
def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams) -> Result[Report, Fault]:
    return thin_rail(settings, scope, params, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)


def thin_rail(  # shared by static/test/docs; the rail aspect stack + seam are owned by registry.py
    settings: AssaySettings, scope: ArtifactScope, params: DocsParams, *, claim: Claim, verb: str, mode: Mode
) -> Result[Report, Fault]:
    return route(Language.DOCS, params.paths).map(  # GLOB over changed *.md/*.mmd; Fault short-circuits on Error
        lambda routed: _strict(
            _outcomes(routed, settings=settings, scope=scope, claim=claim, verb=verb, mode=mode),
            strict=params.strict,
        )
    )


def _outcomes(routed: Routed, *, settings, scope, claim: Claim, verb: str, mode: Mode) -> Report:
    checks = tuple(Check(tool=t, glob=" ".join(routed.files)) for t in select(claim, routed.language) if t.mode is mode)
    slots = fan_out(checks, settings=settings, scope=scope, routed=routed)  # read-only; CapacityLimiter; no lease
    return fold(claim, verb, tuple(done for slot in slots if (done := _done(slot)) is not None))


def _strict(report: Report, *, strict: bool) -> Report:
    match (strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            raise FaultedPromotion(report)  # caught at registry seam -> Fault{status=FAULTED}
        case _:
            return report
```

`_done` projects each slot via `match slot: case Result(tag="ok", ok=done): return done; case _: return None` — the walrus-guarded comprehension keeps the fold a single pure projection, so the Error channel (`FAULTED`/`BUSY`/`TIMEOUT`) never enters the success monoid. `_strict` discriminates `(strict, status)` as a tuple pattern — never `return match …`; its promotion raises a sentinel the registry seam converts to `Fault{argv=(), status=FAULTED, message="--strict: no docs changed"}` (exit 2). `FAILED` (a real diagram defect) rides its own status and is never re-promoted.

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| ---------- | ----------- | ---------- |
| `composition/registry.py` | inbound | `Bind(Claim.DOCS, "check", docs_rail.check, DocsParams, "Markdown + Mermaid validation.")`. `rail(bind)` weaves the `checked ▷ logged ▷ traced` aspect stack over `_narrow(bind.handler)`, threads `_strict` (the registry-side `--strict` promoter) and the `_guard` `FaultedPromotion → Fault{FAULTED}` catch between handler and `_emit` (the sole stdout writer). Retry correlation is bound in `traced` (the engine seam emits a `tool.name` child span carrying `run_id`), not in `logged`. |
| `core/routing.py` | called | `route(Language.DOCS, paths, *, source=None) → Result[Routed, Fault]` globs changed `*.md`/`*.mmd` via `strategy="glob"`; the default `source` is the git+`fd`+`pathlib` `LOCAL` binding. `place(routed, tool, *, settings)` projects the `Input.INCLUDE` argv tail from `routed.groups`. |
| `composition/catalog.py` | called | `select(Claim.DOCS, Language.DOCS) → tuple[Tool, ...]` returns the single `mmdc` row; deterministic sort key `(language.value, mode.value, name, command)`. |
| `core/engine.py` | called | `fan_out(checks, *, settings, scope, routed, deadline=None) → tuple[Result[Completed, Fault], ...]`; `run_check` weaves `checked ▷ traced ▷ retried` over `_guarded`. No lease (read-only). |
| `core/model.py` | reused | `fold`, `Report`, `Completed`, `Check`, `Counts`, `Artifact`, `Match`, `RailStatus`, `BaseParams`, `Fault` — zero new wire shapes. |

## [5][EXTENSIBILITY]

A second docs validator (a prose linter, a link checker) is **one** `Tool` row in `catalog.py` with `claim=Claim.DOCS` and `mode=Mode.CHECK` — `select` picks it up and `_outcomes` folds it; no change to `docs.py`, `DocsParams`, or any rail signature. A new doc suffix is one entry in `Language.DOCS.suffixes`.

## [6][CONSIDERATIONS]

- `mmdc` is render-as-validation: it has no JSON diagnostic mode, so the rail relies on the exit code alone — `from_returncode` maps `0→EMPTY` and any non-zero to `FAILED`. A clean run therefore yields `EMPTY`, not `OK` (no parser affirms `OK`); `--strict` exists precisely to convert that ambiguous `EMPTY`/`SKIP` into a hard `FAULTED` when an agent asserts "docs changed and must validate."
- `mmdc` requires a headless browser (Puppeteer/Chromium); under the fleet its launches are CPU-heavy yet leaseless, so the `CapacityLimiter(max_checks)` is the only backpressure. If diagram count grows, bound it at the routing arm (cap `routed.files`) rather than adding a lease — a docs check must never contend on the `bridge`/`mutation`/`package` locks.
- `--strict` promotion crosses the rail boundary as the `FaultedPromotion` sentinel, not a `Result` — it is the one place docs raises rather than returns. The sentinel rides exactly one `raise` (in `_strict`) and one `except` (the registry `_guard`); the registry-side `_strict` handles the same `EMPTY`/`SKIP` promotion for every other rail through `getattr(params, "strict", False)`, so docs and the rest converge on one identical `Fault{FAULTED}` shape.
