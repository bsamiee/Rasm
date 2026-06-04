# [H1][ENGINE_DESIGN]
>**Dictum:** *One executor folds any `Check` into `Completed | Fault`; variance is data on three axes, never a code path.*

Generalizes `tools/quality/process.py` from dotnet-only (`DotnetInvocation`, `dotnet`, `dotnet_args`, `dotnet_build`) to any `Runner`. The dotnet artifact splice becomes one `Input`/scope arm, not a module. Faults are `Result[Completed, Fault]`; nothing raises across the rail boundary. Verified against anyio 4.13.0 (`run_process`, `open_process` async-CM, `create_task_group`, `fail_after`, `CancelScope(shield=True)`).

**Canonical:** [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) · [`snippets/model-status.py.md`](snippets/model-status.py.md) (`receipt`, `Completed.status`) · [`AOT.md`](AOT.md) §3 (`run_check` seam) · [`snippets/aspect.py.md`](snippets/aspect.py.md) (`compose_spawn` + `retried`). Surface names: **`run_check`**, **`fan_out`** — no `Engine.run`, no `Engine` Protocol.

---
## [1][ARGV_COMPOSITION]
>**Dictum:** *`argv = Runner.prefix ++ scope-spliced command ++ Input placement`; two `match` arms own all variance.*

`Check` is a `Tool` bound to inlined routed scalars (`paths`, `owner`, `solution`, `glob` from `routing.route(language, paths)`); `ArtifactScope` is passed as a **parameter** to `run_check(check, *, settings, scope)` — never stored on `Check` (`model.md` §3). Composition is a pure fold of three orthogonal axes; the dotnet `--artifacts-path` splice is the single scope-aware case inside `_splice`, generalizing `DotnetInvocation.argv()` (`tools/quality/process.py` L147-L153).

```python
def _argv(check: Check, routed: Routed, *, scope: ArtifactScope | None) -> tuple[str, ...]:
    tool = check.tool
    body = _splice(tool.runner, tool.command, scope)   # scope axis: dotnet flags as ONE case
    tails = place(routed, tool)                        # routing.place — one tuple per fan-out Check
    return (*tool.runner.prefix, *body, *tails[0]) if tails else (*tool.runner.prefix, *body)

def _check_from(tool: Tool, routed: Routed) -> Check:
    return Check(tool=tool, paths=routed.files, owner=routed.projects[0] if routed.projects else "",
                 solution="", glob=_glob_literal(routed, tool.language))

def _splice(runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None) -> tuple[str, ...]:
    match (runner, command, scope):                    # dotnet artifact-scope splice == one arm, not a module
        case (Runner.DOTNET, (verb, *_), ArtifactScope() as s) if verb in _SCOPED_VERBS:
            cut = command.index("--") if "--" in command else len(command)
            return (*command[:cut], *s.dotnet_flags, *command[cut:])
        case _:
            return command
```

`Runner.prefix` payloads (`StrEnum.__new__`): `DIRECT=()`, `MODULE=("uv","run","python","-m")`, `UV=("uv","run")`, `DOTNET=("dotnet",)`, `PNPM=("pnpm","exec")`. `_SCOPED_VERBS = frozenset(("build","clean","msbuild","pack","publish","restore","run","test"))` (ports L52). Tool-driver dotnet verbs (`format`, `tool`) carry `Input.INCLUDE`/`NONE` and never match the splice, so artifact flags never reach a verb that rejects them.

---
## [2][CHECK_RUN_AND_PRIMITIVE]
>**Dictum:** *`run_check` resolves argv/env; one `run` primitive owns launch, capture, stream, timeout; process exit → `receipt` → `Completed`.*

```python
def run_check(check: Check, *, settings: AssaySettings, scope: ArtifactScope | None,
              routed: Routed) -> Result[Completed, Fault]:
    return run(_argv(check, routed, scope=scope), cwd=check.cwd or settings.root,
               env=_overlay(check, settings, scope), stream=check.tool.mode.stream,
               timeout=check.tool.timeout, artifact_dir=_stream_dir(check, settings, scope)
               ).map(lambda done: receipt(done.argv, done.returncode, stdout=done.stdout,
                                         stderr=done.stderr, duration_ms=done.duration_ms))

def run(argv: tuple[str, ...], *, cwd: Path | None = None, env: Mapping[str, str] | None = None,
        stream: bool = False, timeout: float | None = None,
        artifact_dir: Path | None = None) -> Result[Completed, Fault]:
    return anyio.run(_guarded, argv, cwd, env, stream, timeout, artifact_dir)   # one loop per invocation
```

`stream` dispatch (`Mode.stream` payload on the unified `Mode` enum — `aot-architecture.md` §3.iii) mirrors `run`/`_stream` (L499-L577):
- **`stream=False`** — `await anyio.run_process(list(argv), cwd=..., env=..., check=False)`; `Completed(argv, returncode, stdout, stderr)` holds full bytes (zero re-decode; `text`/`lines` decode lazily).
- **`stream=True`** — `async with await anyio.open_process(...)`; two `create_task_group` readers tee each chunk to the bounded tail deque, `sys.stderr.buffer`, and `artifact_dir/<command-id>/{stdout,stderr}.log` (`wb`, one writer per run). `Completed.stdout` = `b"".join(tail)`.

`receipt` / `RailStatus.from_returncode` (`status.md` §3, snippet §1): process `0→EMPTY`, `5→BUSY`, `124→TIMEOUT`, else `FAILED` on **`Completed`** — analyzer/lint non-zero is **`RailStatus.FAILED`**, not `Envelope.error`. Reserve **`Fault`** for spawn failure, lease busy, timeout before a complete process result, cancellation. Timeout via `anyio.fail_after` → `Fault(..., returncode=124, status=TIMEOUT)` (boundary exemption for `TimeoutError` only). Parsers may set `Completed.status=OK` on affirmed success (`status.md` §3).

---
## [3][CONCURRENCY_AND_LEASES]
>**Dictum:** *Read-only checks fan out under one task group; exclusive resources take non-blocking flock leases that fail fast to BUSY/exit 5.*

```python
def fan_out(checks: tuple[Check, ...], *, settings: AssaySettings) -> tuple[Result[Completed, Fault], ...]:
    async def _run() -> tuple[Result[Completed, Fault], ...]:
        slots: list[Result[Completed, Fault]] = [_pending()] * len(checks)
        async with anyio.create_task_group() as tg:
            for i, check in enumerate(checks):
                tg.start_soon(_into, slots, i, check, settings)   # bounded by caller's check count
        return tuple(slots)
    return anyio.run(_run)
```

Each `Check` still runs through the same `run` primitive (each its own `open_process`/`run_process`); the task group only parallelizes independent read-only rows across languages under one `run_id`. Exclusive resources (dotnet build closure, Stryker mutation, yak stage, bridge) reuse `exclusive_lease`/`leased` verbatim (L260-L308): `fcntl.flock(LOCK_EX|LOCK_NB)`; `BlockingIOError → ResourceBusyError → Fault(status=BUSY, returncode=5)`; `OSError|RuntimeError → Fault`. Leases never block. Structured teardown on cancel preserves L555-L559: `process.kill()` then `with anyio.CancelScope(shield=True): await process.wait()` so no orphaned child survives a `fail_after` or group cancellation.

---
## [4][ASPECT_SEAMS]
>**Dictum:** *beartype and aspects wrap the two public Engine entrypoints; inner closures stay bare.*

`run_check` and `fan_out` are the only `__all__` exports and the only beartype-and-aspect seams; `_argv`, `place` (imported from `routing`), `_splice`, `_overlay`, `_guarded`, `_stream`, `_collect_stream` are private closures/module internals checked statically by `ty`, not at runtime.

| [DECORATOR] | [WHERE] | [WHY] |
| ----------- | ------------------ | ----------------------------------------------------------------- |
| `@checked` (beartype) | `run_check`, `fan_out` | Enforce `Check`/`AssaySettings`→`Result` shapes once at the boundary; hot inner loop pays nothing. |
| `@traced` (otel) | `run_check` | One child span per `Check` under the rail parent; argv head, runner, language, status as attributes. |
| `@retried` (stamina) | `run_check` (spawn only) | Retry only transient spawn faults (`OSError` on exec, restore probes); never retry a non-zero analyzer exit. |

**No `@logged` on the engine seam** — structlog binds `claim`/`verb`/`run_id` at the rail runner only (`aspect.md` §3); span attributes carry per-`Check` receipts. Optional per-`Check` `@logged` is debug-only and off by default.

Decorators live in `core/aspect.py` and compose **outer→inner** `checked ▷ traced ▷ retried ▷ run_check` (`aspect.md` §2). The rail runner uses `checked ▷ logged ▷ traced` with **no** `retried`. The engine imports the composed stack; it owns no inline trace/log/retry call.

---
## [5][OPTIMIZATION]
>**Dictum:** *Zero-copy bytes, bounded memory, one loop, with resource enrichment as a future seam.*

- **Zero-copy bytes** — `Completed.stdout/stderr` stay `bytes`; `text`/`lines` decode lazily with `errors="replace"`. No decode in the stream reader.
- **Bounded tail deque** — `_collect_stream` retains ≤ `_STREAM_TAIL_BYTES` (128 KiB) per stream via `deque.popleft()` while teeing the full stream to the artifact log (L563-L577); long dotnet/Stryker builds stay O(1) in memory yet keep a complete on-disk log.
- **One event loop per invocation** — `anyio.run` is entered once per `run`/`fan_out`; no nested loops, no global backend state.
- **psutil (FUTURE)** — enrich `Completed` with peak RSS/CPU-time by sampling the `open_process` PID inside the reader group; lands as an optional `resources` field on `Completed`, no signature change to `run`. Listed in `ARCHITECTURE.md` §6 as a `Completed`-receipt enrichment.

---
## [6][FAILURE_MODES_AND_OPEN_DECISIONS]
>**Dictum:** *Every failure is a typed `Fault`; open seams are env overlay and stream layout.*

| [FAILURE] | [CHANNEL] |
| ------------------------ | ------------------------------------------------- |
| Non-zero process exit | `Ok(Completed)` via `receipt` + `from_returncode(rc)`; stderr tail stays on `Completed`. |
| Timeout (no complete result) | `Error(Fault(returncode=124, status=TIMEOUT))`. |
| Held exclusive lease | `Fault(returncode=5, status=BUSY)`, owner block in detail. |
| Spawn failure (`OSError`) | `Fault(status=FAILED)` after `@retried` exhausts. |
| Cancellation | child killed + shielded wait; exception re-raised inside loop, surfaced as `Fault` by the rail. |

**Open decisions.** (1) **Stream artifact layout** — `_process_dir` slugs `argv[:6]` (L580-L584); under assay's per-run `.artifacts/assay/<rail>/<run_id>/process/<command-id>/` two distinct rows could still collide on identical heads (e.g. two `dotnet build` configs). Resolve by keying on `check.id` (catalog row id) rather than argv slug. (2) **Env overlay** — `_overlay` must fold `scope.dotnet_env` with the apphost/runtime-root resolution (`_dotnet_root`/`dotnet_apphost_env`, L427-L459) for apphost-deployed tools (`ilspycmd`, rhinocode) while SDK verbs self-locate via the muxer; open question is whether the overlay is a `Runner.DOTNET`-keyed arm in the engine or a `Tool.env_profile` field carried as catalog data — the latter keeps the engine runner-agnostic and is preferred.
