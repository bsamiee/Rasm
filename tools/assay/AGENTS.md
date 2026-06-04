# Assay Agent Guide

Scope: `tools/assay/` only. Root `AGENTS.md` and `CLAUDE.md` own universal policy; this file is the canonical assay operator delta.

Operator reference is `tools/assay/README.md` (Envelope contract, rail map, concurrency, quality migration). Design depth: `ARCHITECTURE.md`, build order: `IMPLEMENTATION.md`, wire law: `.design/TYPE_SYSTEM.md`, aspect law: `.design/AOT.md`.

## Load order

1. `CLAUDE.md` §5.2 quality gates (until assay parity, quality commands still apply).
2. Root `AGENTS.md` → this file.
3. `tools/assay/README.md` before invoking any verb.
4. Touching shapes: `.design/TYPE_SYSTEM.md`; touching aspects: `.design/AOT.md`.
5. `.claude/skills/coding-python/SKILL.md` before any `.py` edit.

Codex budget: keep this file delta-only; encyclopedic detail stays in `ARCHITECTURE.md` and `.design/`.

## What belongs where

| [PATH] | [OWNS] | [NEVER] |
| ------ | ------ | ------- |
| `core/model.py` | Axis enums, `Tool`, `Check`, `Report`, `Detail`, `Envelope`, `Parser` alias | Env settings, CLI params, per-program argv builders |
| `core/status.py` | `RailStatus` only | Second status enum or string status fields |
| `core/engine.py` | `run_check`, `fan_out`, leases, process capture | Rail-specific folds, Cyclopts |
| `core/routing.py` | `route`, `place`; `Source` Protocol (sole justified Protocol) | Catalog rows, Envelope emit |
| `core/aspect.py` | `compose`, `@checked`, `@logged`, `@traced`, `@retried` | Inline structlog/otel/stamina in rails |
| `composition/catalog.py` | `TOOLS` tuple, `select` | Handlers, registry |
| `composition/registry.py` | `REGISTRY`, `rail`, `_emit` (sole stdout writer) | Tool argv logic |
| `composition/settings.py` | `AssaySettings`, `artifact(kind, *parts)` | msgspec wire structs |
| `rails/*.py` | One claim fold → `Result[Report, Fault]` | New status types, helper modules |
| `__main__.py` | Cyclopts tree, `resolve_returncode` | Business logic |

Add a program: one `Tool` row in `catalog.py`. Add a verb: one `Bind` row in `registry.py` + frozen `@dataclass` params. Add a rail: one `rails/<claim>.py` handler — not a new engine module.

## Spam tripwires

Stop and collapse before merging if any appear:

- Free `typing.Literal` aliases for vocabularies already on a `StrEnum`.
- `NamedTuple` CLI params (`TID251`); use frozen `@dataclass` + Cyclopts flatten.
- Second model system on wire shapes (pydantic `BaseModel` for `Report`/`Envelope`).
- Dual pydantic + msgspec on the same field.
- `Engine` or `Parser` `Protocol` — engine is module functions; `Parser = Callable[[Completed], Detail | None]`.
- Per-rail report structs (`StaticPlanReport`, …) — use `Report` + `detail` tag.
- `helpers.py`, `*Util`, `common_*` for single-call indirection.
- `artifact_paths: dict[str, str]` — use `tuple[Artifact, ...]`.
- `@tool` / `@rail` registration decorators — registration is data (`TOOLS`, `REGISTRY`).
- `@retried` on the rail runner; `@logged` on `run_check` (see `.design/AOT.md` seam table).
- stdout writes outside `_emit` (including cataloged tools printing JSON).

## Decorator and AOT rules

Cross-cutting behavior is **only** aspect decorators at two seams:

- Rail runner: `checked ▷ logged ▷ traced` (no `retried`).
- `run_check`: `checked ▷ traced ▷ retried` (no `logged` on hot path).

`structlog.configure` runs once at `tools/assay/__init__.py` import; processors target **stderr**. OpenTelemetry exports only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set. `beartype` at seams only — not `beartype_this_package()`. Full stack order and stamina predicates: `.design/AOT.md`.

## Shape rules

- msgspec: all wire/evidence structs; `Detail` tagged union with explicit short tags and `forbid_unknown_fields`.
- pydantic-settings: `AssaySettings` only; sources `(init_settings, env_settings)` — no `CliSettingsSource`.
- One `StrEnum` member = Cyclopts token = msgspec wire value = `match` key.
- `Envelope` uses `claim` + `report`, not quality’s `rail` + `data` + `evidence` ladder.
- `RailStatus.join` is the sole status fold; exit codes live on enum payloads.

Canonical catalog: `.design/TYPE_SYSTEM.md` §4. Snippet spine: `.design/snippets/cli.py.md`.

## When to invoke assay

Use `uv run python -m tools.assay` when the registry row exists and owns the proof:

- Polyglot static/test/docs on changed paths (replaces `pnpm check:py` / `check:ts` at parity).
- C# static/test with the same verbs as `tools.quality` but Envelope `claim`/`report`.
- New operator features that extend the catalog table — not new CLI scripts.

## When NOT to invoke (yet)

- Claims not registered in `REGISTRY` — use `tools/quality` per `tools/quality/README.md`.
- Pure xUnit specs without Rhino — `uv run python -m tools.quality test run` until assay `test` parity.
- Bridge scenario authoring rules — `tools/rhino-bridge/AGENTS.md` (operator path may still be quality today).

## Quality bar

Per `IMPLEMENTATION.md` §5 after each touched file:

```bash
uv run ruff check tools/assay
uv run ty check tools/assay
```

Wire laws (when `tests/tools/assay/` exists): `encode(decode(x)) == x` for `Envelope`/`Report`; unknown `detail` field fails decode.

Do not run full static/test/bridge rails for markdown-only edits unless the user requests proof.

## Dependencies from day one

Declare consumers in root `pyproject.toml` before use — no shadow imports:

| [LIB] | [ROLE] |
| ----- | ------ |
| `msgspec` | Wire structs, Envelope encode |
| `pydantic-settings` | `AssaySettings` |
| `cyclopts` | CLI tree from `REGISTRY` |
| `expression` / `effect` | `Result` rails |
| `anyio` | Process exec, `fan_out` task groups |
| `structlog` | stderr diagnostics; configure at import |
| `beartype` | `@checked` seams |
| `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http` | optional trace export |
| `stamina` | `@retried` on spawn only |

**Gated (do not add until a registry consumer exists):** `watchfiles`, `psutil`, `fsspec` — listed in `ARCHITECTURE.md` §6 as future capabilities.

## Validation ladder (implementation phase)

```bash
uv run ruff check tools/assay
uv run ty check tools/assay
uv run pytest tests/tools/assay -q
uv run python -m tools.assay static plan
```

After C#-touching rails: `uv run python -m tools.quality static build [paths]` until assay `static build` is registered.
