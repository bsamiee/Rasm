# [H1][ENGINE]
>**Dictum:** *One executor folds any `Check` into `Result[Completed, Fault]`; variance is data on three axes, capacity-bounded fan-out runs under exactly one event loop.*

## [1][PURPOSE]

`core/engine.py` is the sole process executor (§3 abstraction model). It generalizes `tools/quality/process.py` from dotnet-only (`DotnetInvocation`, `dotnet_args`) to any `Runner`: the dotnet `--artifacts-path` splice collapses to one `_splice` `match` arm, and the lease machinery ports with a `psutil` liveness upgrade. Surface is exactly two public functions — `run_check` and `fan_out` (D14) — both sync façades that call `anyio.run` exactly once and weave `compose_spawn(retried()) ▷ compose(checked, traced)` on the inner `_guarded` spawn (§6, D3: no `logged` at the engine seam, so retry correlation rides `traced`). `routed`/`scope`/`deadline` flow as **parameters** (D8/D45/D47), never as `Check` fields. Faults never raise across the rail boundary; everything rides `Result[Completed, Fault]`. Verified against `anyio` 4.13.0 (`run_process`, `open_process`, `create_task_group`, `CapacityLimiter`, `fail_after`, `move_on_after`, `CancelScope(shield=True)`), `psutil` 7.2.2 (`Process.is_running`, `create_time`, `oneshot`, `cpu_count`, `NoSuchProcess`, `AccessDenied`), `stamina` 26.1.0 (`AsyncRetryingCaller`), `asyncssh` 2.23.0 (`connect` async-ctx-mgr → `SSHClientConnection`; `conn.run(*, env, encoding, check=False) -> SSHCompletedProcess{exit_status,returncode,stdout,stderr}`; `conn.create_process(*, encoding, stdin) -> SSHClientProcess{stdout/stderr: SSHReader, exit_status, wait, close, wait_closed}`; `DEVNULL`).

The subprocess seam is one backend (`_run_process_backend`) folding two axes — `streaming` (data) and `settings.exec_target` (config) — as nested `match` arms; `_capture`/`_stream` are thin callers and `_guarded` selects on `Mode.stream`. The `case "":` arm is the **local hot path**, preserved byte-identical (`anyio.open_process`+tee+`_reap` or `anyio.run_process(check=False)`), so a local run pays zero remote cost. The `case target:` arm is **transparent REMOTE-EXEC** (`_run_remote`): `ssh://[user@]host[:port]` parsed by `urllib.parse.urlsplit`, run over `asyncssh` against a `cd <cwd> && KEY=v … <argv>` shell string (every token `shlex.quote`-escaped) with `encoding=None` for raw `bytes`. The engine still owns span enrichment at every `Fault` (Tier-A `record_exception` + `fault.resource_snapshot`); it does **not** build the wire `Diagnostic` — that is `registry._emit` (D28: `Fault` stays `{argv,status,message}`, no `Fault.context`).

## [2][CANONICAL_SHAPES]

This doc owns the executor surface; the `Completed`/`Fault`/`receipt` shapes are imported from `core/model.py` (verbatim per `model-status.py.md`). `scope`/`routed` are `run_check` **parameters**, never `Check` fields (D8). The four tuning knobs (`scoped_verbs`, `stream_tail_bytes`, `stream_chunk_bytes`, `lease_drift_tolerance`) are **settings fields**, not module constants: the engine reads them from `AssaySettings`, and the settings defaults (`frozenset(("build","clean","msbuild","pack","publish","restore","run","test"))`, `4096`, `65536`, `1.0`) are the canonical values. The exec-target pair is also settings: `exec_target: str = ""` (`""` local / `ssh://[user@]host[:port]` remote, validated at the pydantic boundary) and `exec_known_hosts: str | None = None` (asyncssh `known_hosts` path; `None` disables the host-key check for ephemeral fleet nodes). The engine keeps only true module constants (lock flags, the `fault.resource_snapshot` event name, the cached `_DECODER`, the POSIX-bound `fcntl` members, the `assay.engine` lease logger).

```python
def run_check(check: Check, *, settings: AssaySettings, scope: ArtifactScope | None,
              routed: Routed, deadline: float | None = None) -> Result[Completed, Fault]: ...
def fan_out(checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None,
            routed: Routed, deadline: float | None = None) -> tuple[Result[Completed, Fault], ...]: ...

def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> tuple[str, ...]:
    tool = check.tool
    body = _splice(tool.runner, tool.command, scope, settings.scoped_verbs)  # scope axis: dotnet flags as ONE arm
    tails = place(routed, tool, settings=settings)          # routing.place — fan of tuples per Check
    return (*tool.runner.prefix, *body, *(part for tail in tails for part in tail))

def _splice(runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None,
            scoped_verbs: frozenset[str]) -> tuple[str, ...]:
    match (runner, command, scope):                         # dotnet artifact-scope splice == one arm
        case (Runner.DOTNET, (verb, *_), ArtifactScope() as s) if verb in scoped_verbs:
            cut = command.index("--") if "--" in command else len(command)
            return (*command[:cut], *s.dotnet_flags, *command[cut:])
        case _:
            return command
```

`Runner.prefix` payloads (§3): `DIRECT=()`, `MODULE=("uv","run","python","-m")`, `UV=("uv","run")`, `DOTNET=("dotnet",)`, `PNPM=("pnpm","exec")`. Tool-driver dotnet verbs (`format`, `tool`) carry `Input.INCLUDE`/`NONE` and never match the splice, so artifact flags never reach a verb that rejects them. `ArtifactScope.build(closure)` yields the **stable** `.artifacts/assay/build/<closure>/` path (D32, closure = `sha256(sorted-projects)[:16]` or `"solution"`); run-scoped scopes use `<claim>/<run_id>/`.

| [SHAPE] | [FIELDS / SIGNATURE] | [D] |
| --- | --- | --- |
| `Completed` | `{argv, returncode, stdout, stderr, status=EMPTY, notes}` (model) | D11 |
| `Fault` | `{argv, status=FAULTED, message}` — **no** `returncode`/`detail` | D28 |
| `receipt` | `(argv, rc, *, stdout, stderr, status=None, notes) -> Completed` | D11 |
| `exclusive_lease` | `(resource, run_id, *, settings, project="", mode="exclusive")` — `@contextmanager` yielding `Result[_Held, Fault]` | D34 |
| `leased[T]` | `(resource, action, *, settings, run_id, project="", mode="exclusive") -> Result[T, Fault]` | D34/D40 |
| `_LeaseOwner` | `{resource, run_id, pid, create_time, cwd="", project="", mode="exclusive", started_at=0.0, target=""}` — `msgspec.Struct(frozen, gc=False, omit_defaults)`; `target` (`settings.exec_target`) stamped in `_write_owner` records WHERE the holder ran | D34 |
| `_run_process_backend` | `(argv, *, cwd, env, settings, streaming, tail_cap, chunk) -> Completed` — `match settings.exec_target`: `case "":` local hot path byte-identical, `case target:` → `_run_remote` (asyncssh) | ENGINE |
| `ResourceBusyError` | `class ResourceBusyError(Exception)` — defined in `core/model.py`, re-exported here; engine maps live contention to a `Fault(BUSY)` **value**, never raises it | D40 |

## [3][VALIDATED_SNIPPET]

Two sync façades, each calling `anyio.run` exactly once (D33). `run_check` runs one `_guarded` spawn; `fan_out` opens one task group + `CapacityLimiter(_governed(settings))` and fills ordered `Result` slots pre-sized to `[None] * len(checks)` (no phantom `_pending`). The spawn loop `for i, check in enumerate(checks): tg.start_soon(_into, i, check)` is boundary glue — task-spawn fan-out, not domain logic — so the imperative-loop ban does not bind it (the loop carries no fold state; the fold is in `model.fold` downstream, D46). The shared aspect-woven `_spawn` is awaited **directly** inside the group; `_overlay`/`_splice`/`_into`/`_spawn` never reach for a nested loop. `routed`/`scope`/`deadline` arrive as parameters (D45/D47); `Check` holds none of them (D8). D12: non-zero exit → `Completed` via `receipt`+`from_returncode`, **never** `Fault`. Tracing is entirely woven — there is **no** inline OTel tracer or parent `engine.fan_out` span; the `traced` aspect opens the per-spawn child span and is where retry correlation binds.

```python
type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None],
                       Coroutine[None, None, Result[Completed, Fault]]]

def _spawn(check: Check, settings: AssaySettings) -> _Woven:     # aspect-woven async spawn, awaited directly
    span = traced(span=check.tool.name,                          # child span keyed by tool.name (D50)
                  attrs=lambda *_a, **_k: {"assay.run_id": settings.run_id, "assay.tool": check.tool.name})
    layered = compose_spawn(retried())(_guarded)                 # retried wraps the Spawn only (D2)
    return compose(checked(), span)(layered)                     # lift checked ▷ traced Hom; no logged (D3)

def run_check(check: Check, *, settings: AssaySettings, scope: ArtifactScope | None,
              routed: Routed, deadline: float | None = None) -> Result[Completed, Fault]:
    return anyio.run(_spawn(check, settings), check, settings, scope, routed, deadline)  # one loop

def fan_out(checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None,
            routed: Routed, deadline: float | None = None) -> tuple[Result[Completed, Fault], ...]:
    async def _run() -> tuple[Result[Completed, Fault], ...]:
        limiter = anyio.CapacityLimiter(_governed(settings))     # min(max_checks, logical CPUs)
        slots: list[Result[Completed, Fault] | None] = [None] * len(checks)   # ordered, total on exit
        async def _into(i: int, check: Check) -> None:
            async with limiter:                                  # bound concurrency, not task count
                slots[i] = await _spawn(check, settings)(check, settings, scope, routed, deadline)
        with anyio.move_on_after(deadline - time.monotonic() if deadline is not None else None):
            async with anyio.create_task_group() as tg:
                for i, check in enumerate(checks):               # task-spawn glue (no fold state)
                    tg.start_soon(_into, i, check)
        return tuple(_total(slot) for slot in slots)             # unfinished None → Fault(TIMEOUT)
    return anyio.run(_run)                                       # exactly one loop per invocation (D33)

async def _guarded(check: Check, settings: AssaySettings, scope: ArtifactScope | None,
                   routed: Routed, deadline: float | None) -> Result[Completed, Fault]:
    argv = _argv(check, routed, settings=settings, scope=scope)
    budget = deadline - time.monotonic() if deadline is not None else check.tool.timeout
    cwd = str(UPath(check.cwd or settings.root).path)            # LOCAL path string — local-exec boundary
    env = _overlay(scope)
    trace.get_current_span().set_attribute("exec.target", settings.exec_target)
    spawn = _stream if check.tool.mode.stream else _capture     # typed refs; thin callers of _run_process_backend
    started = time.monotonic()
    try:
        with anyio.fail_after(budget):                          # None disables; TIMEOUT path below
            done = await spawn(argv, cwd=cwd, env=env, settings=settings)
        return Ok(_elapsed(done, started))                      # stamp duration_ms onto the receipt

async def _run_process_backend(argv, *, cwd, env, settings, streaming, tail_cap, chunk) -> Completed:
    match settings.exec_target:                                 # config axis: "" local hot path / ssh:// remote
        case "":                                                # BYTE-IDENTICAL local: open_process+tee / run_process
            match streaming:
                case True:  ...                                 # anyio.open_process + dual _drain tee + shielded _reap
                case False: ...                                 # anyio.run_process(check=False)
        case target:
            return await _run_remote(argv, target, cwd=cwd, env=env, settings=settings,
                                     streaming=streaming, tail_cap=tail_cap, chunk=chunk)

async def _run_remote(argv, target, *, cwd, env, settings, streaming, tail_cap, chunk) -> Completed:
    parts = urlsplit(target); port = parts.port if parts.port is not None else ()
    command = _remote_command(argv, cwd=cwd, env=env)           # cd <cwd> && KEY=v … argv, shlex.quote'd
    async with asyncssh.connect(parts.hostname or "", port, username=parts.username,
                                known_hosts=settings.exec_known_hosts) as conn:
        match streaming:                                        # encoding=None → raw bytes (== Completed shape)
            case True:  ...   # conn.create_process + _drain_reader tee + shielded close/wait_closed
            case False:       # conn.run; exit_status or 0 == returncode
                done = await conn.run(command, encoding=None, check=False)
                return receipt(argv, done.exit_status or 0,
                               stdout=_as_bytes(done.stdout), stderr=_as_bytes(done.stderr))
    except TimeoutError as exc:                                 # sole boundary exemption
        _diagnose(exc)                                          # span: record_exception + resource snapshot
        return Error(Fault(argv, status=RailStatus.TIMEOUT, message="deadline exceeded"))
    except OSError as exc:                                      # spawn failure after retried exhausts
        _diagnose(exc)                                          # span: record_exception + resource snapshot
        return Error(Fault(argv, status=RailStatus.FAULTED, message=str(exc)))

def _diagnose(exc: BaseException) -> None:                      # inherent, knob-free; span events, never stdout
    span = trace.get_current_span()                             # the woven traced child span (no-op if non-recording)
    span.record_exception(exc)
    span.add_event("fault.resource_snapshot", attributes=_snapshot())  # psutil oneshot: rss/cpu/num_fds
```

`_spawn` returns the shared aspect-woven `_Woven` async callable: `compose_spawn(retried())` weaves the `Spawn`-only `retried` (D2) onto `_guarded`, then `compose(checked(), traced(...))` lifts the `checked ▷ traced` `Hom` layers (D1/D3 — **no** `logged`, so retry correlation binds in `traced`). `run_check` wraps `_spawn` in its own single `anyio.run`; `fan_out`'s `_into` `await`s the *same* `_spawn` directly inside the one `anyio.run(_run)`, so there is never a second loop (D33 — calling the sync `run_check` from inside the group would `RuntimeError`). `retried` (stamina) gates `_transient` (aspect) — `ResourceBusyError`/`TIMEOUT` are **never** retried (D29/D40). `_guarded` selects `_stream` vs `_capture` on `check.tool.mode.stream` (`spawn = _stream if … else _capture` — both are typed named functions sharing the `(argv, *, cwd, env, settings) -> Completed` shape, so the conditional stays a checker-typed reference, never an untyped lambda); each is a thin caller of the one `_run_process_backend(streaming=…)` seam. That backend `match`es `settings.exec_target`: `case "":` is the **local hot path**, byte-identical to the prior `_stream`/`_capture` bodies (`open_process` + dual bounded-tail tee + shielded `_reap`, or `run_process(check=False)`), so a local run pays no remote cost. `case target:` is `_run_remote` over asyncssh. Both arms seed the same `Completed` receipt. The spawn `cwd` is still materialized to a **local** path string via `str(UPath(check.cwd or settings.root).path)` — `.path` strips any protocol prefix, so a *local* child launches against the real local FS even when `settings.root` rides a non-`file://` `UPath` (artifact I/O rides the `UPath`/`ArtifactStore` seam; the spawn cwd is the local-exec boundary). For the **remote** arm the same local cwd string is `cd`-prefixed into the remote command, so the remote shell roots at the path the local FS would have. Process non-zero exit is a value through `receipt(argv, returncode)` → `from_returncode` (`0→EMPTY`, `5→BUSY`, `124→TIMEOUT`, else `FAILED`) on the **success channel**; only spawn/timeout/lease-miss/strict take the `Error` channel.

### REMOTE-EXEC backend (`_run_remote`)

`urllib.parse.urlsplit(target)` projects `ssh://[user@]host[:port]` to `(username, hostname, port)`; a missing port maps to the `()` `DefTuple` sentinel asyncssh treats as default-22. `asyncssh.connect(host, port, *, username, known_hosts)` is an **async context manager** (`async with … as conn`), so the connection is torn down deterministically on every exit path. Two asyncssh quirks shape the command:

- **No native cwd** — asyncssh exposes no working-directory channel, so `_remote_command` builds a single `cd <cwd> && KEY=v … <argv>` shell string with every token `shlex.quote`-escaped (paths/values with spaces or metacharacters must not break the remote parse).
- **`AcceptEnv` whitelist** — `conn.run(env=…)`/`create_process(env=…)` requires the remote `sshd` to whitelist each key in `AcceptEnv`, which most do not; the scope-isolation overlay (`DOTNET_CLI_HOME`/`MSBUILDDISABLENODEREUSE`) would be silently dropped. So env is **inlined** as `KEY=value` exports in the command body instead, surviving any `AcceptEnv` policy.

Buffered → `conn.run(command, encoding=None, check=False)` → `SSHCompletedProcess`; streaming → `conn.create_process(command, encoding=None, stdin=DEVNULL)` → `SSHClientProcess`, its `stdout`/`stderr` `SSHReader`s teed to bounded tails by `_drain_reader` (the remote analogue of `_drain`; `SSHReader.read(n)` returns `b""` at EOF, the recursion sentinel) and torn down shielded (`proc.close()` + `wait_closed()` under `CancelScope(shield=True)`). `encoding=None` yields raw `bytes`, so the `Completed{stdout,stderr}` shape is identical to the local arm (`_as_bytes` normalizes the `bytes|str|None` asyncssh signature: `None` redirected → `b""`); `exit_status` (`None` on a remote signal kill) folds to `returncode` via `or 0` exactly as the local streaming arm does.

`deadline` is an absolute monotonic wall-clock (D47). `_guarded` converts it to a per-spawn budget via `fail_after(deadline - now)`; with no deadline it falls back to `check.tool.timeout`. `fan_out` wraps the whole group in `move_on_after(deadline - now)`: on elapse, in-flight spawns cancel and `_total` back-fills any slot still `None` with `Fault(status=TIMEOUT)` — best-effort partial results, never a stranded loop. Span granularity (D50): the woven `traced` layer opens one child span per `_guarded` keyed by `check.tool.name` with `run_id`/`tool` as attributes — the engine itself never opens a span.

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| --- | --- | --- |
| `core/model.py` | imports | `Check`, `Completed`, `Fault{argv,status,message}`, `receipt`, `ArtifactKind`, `Runner`, `ResourceBusyError` (D8/D11/D28/D40). |
| `core/status.py` | imports | `RailStatus` — the BUSY/TIMEOUT/FAULTED channels stamped on `Fault` and the exit-code algebra (D29). |
| `core/routing.py` | imports | `place(routed, tool, *, settings)` is the sole argv-tail projector (D23); `Routed.files/projects/scope`. |
| `core/aspect.py` | wraps | `compose_spawn(retried())` weaves the `Spawn`-only `retried`; `compose(checked(), traced(...))` lifts the `checked ▷ traced` `Hom`; retry correlation binds in `traced`, never `logged` (D1/D3). |
| `composition/settings.py` | imports | `AssaySettings.max_checks`, `.root`, `.run_id`, the four tuning knobs `scoped_verbs`/`stream_tail_bytes`/`stream_chunk_bytes`/`lease_drift_tolerance`, and the exec-target pair `exec_target`/`exec_known_hosts` (settings-driven, no module constants); `ArtifactScope` is the build-scoped path value (`ArtifactScope.build(closure)` stable-path factory, D32), consumed as the `scope` parameter. |
| `asyncssh` (2.23.0) | REMOTE-EXEC | `_run_remote` opens `asyncssh.connect(host, port, *, username, known_hosts)` (async ctx mgr) and runs the `cd … && argv` command via `conn.run`/`conn.create_process` with `encoding=None`; the engine never touches asyncssh on the `exec_target == ""` local hot path. |
| `opentelemetry`/`structlog`/`psutil` | diagnostics | `_diagnose` reads `trace.get_current_span()` (the woven `traced` child) and calls `record_exception` + `add_event("fault.resource_snapshot", _snapshot())` at every `Fault`; `_guarded` stamps `exec.target` as a span attribute; the lease path stamps `holder.*`/`run_id` span attrs (`_stamp_holder`) and logs the before/after holder around a stale-steal via `structlog` (`assay.engine`). All knob-free; span events + stderr logs, never stdout. |
| `rails/*` | callers | `static`/`test`/`docs` fold `fan_out` results; `bridge`/`package` call `run_check` under `exclusive_lease`. |

Engine-internal symbols (defined in `core/engine.py`, named here for snippet self-containment): `_overlay(scope) -> Mapping[str, str]` clones `os.environ` and folds `ArtifactScope.dotnet_env` on top (it takes only `scope` — `cwd` is resolved separately in `_guarded`); `_splice(runner, command, scope) -> tuple[str, ...]` injects dotnet `--artifacts-path` flags as one `match` arm; `_argv(check, routed, *, settings, scope)` flattens `prefix ▷ spliced-body ▷ routed-tails`; `_spawn(check, settings) -> _Woven` is the shared aspect-woven async callable (`compose_spawn(retried()) ▷ compose(checked, traced)`) consumed by both `run_check` and `_into`; `_into(i, check) -> None` is the task-spawn closure awaiting `_spawn` and writing `slots[i]`; `_guarded(check, settings, scope, routed, deadline) -> Result[Completed, Fault]` is the inner async spawn the `traced` layer wraps with a `tool.name` child span (it stamps `exec.target`, selects `_stream`/`_capture` on `Mode.stream`, `_elapsed`-stamps `duration_ms`, and owns the sole `try/except`); `_stream`/`_capture` are thin callers of `_run_process_backend(argv, *, cwd, env, settings, streaming, tail_cap, chunk)`, the one subprocess seam that `match`es `settings.exec_target` into the byte-identical local arm or `_run_remote(argv, target, …)` (asyncssh); `_remote_command(argv, *, cwd, env)` builds the `shlex.quote`d `cd … && KEY=v … argv` string, `_as_bytes` normalizes asyncssh `bytes|str|None` output, `_drain_reader(reader, *, tail_cap, chunk)` is the remote `SSHReader` tee; `_drain(stream, *, tail_cap, chunk)`/`_next_chunk(stream, *, chunk)`/`_reap` are the local streaming tee (tail/chunk bounds threaded from settings) + shielded teardown; `_total` back-fills unfinished slots; `_governed(settings) -> int` is the `min(max_checks, psutil.cpu_count(logical=True))` fleet governor; `_diagnose(exc) -> None` stamps the current span with `record_exception` + a `fault.resource_snapshot` event; `_snapshot() -> dict[str, int|float]` is the one psutil-oneshot `{mem.rss_bytes, cpu.percent, proc.num_fds}` projection (`_num_fds` degrades to `-1` where the platform withholds the member); `_stale(owner, tolerance) -> bool` is the psutil liveness predicate (drift band from `settings.lease_drift_tolerance`); `_claim(fd, resource, *, run_id, tolerance, target)`/`_write_owner(fd, resource, *, run_id, target)` are the fcntl acquire/steal + owner-stamp split out of `exclusive_lease` (`target = settings.exec_target` stamps the holder block so a fleet sees WHERE a lock holder ran); `_holder(owner)`/`_stamp_holder(owner, *, run_id)` project the contended holder identity onto the steal log + span attrs.

Lease seam (D34): `exclusive_lease` / `leased` port the quality lease — `fcntl.flock(LOCK_EX|LOCK_NB)`, an `_LeaseOwner` block (`resource/run_id/pid/create_time/cwd/project/mode/started_at/target`), truncate-on-release, **never block, never raise across the rail**. `_claim` validates the held owner's `(pid, create_time)` via `_stale` (`psutil.Process(pid).oneshot()` → `is_running() and abs(create_time() - owner.create_time) < tolerance`, the drift band from `settings.lease_drift_tolerance`); a `not is_running()` / mismatch / `NoSuchProcess`/`AccessDenied` holder is **stale** and the lock is stolen with a second `LOCK_NB` acquire. Before/after the steal `_claim` stamps the contended holder `(pid, create_time)` + acquirer `run_id` onto the current span (`_stamp_holder`) and logs `lease.steal`→`lease.stolen` (`lost`/`won` holder identities) via `structlog`, so a fleet sees exactly who lost and who won a recycled lock. A *live* holder makes `_claim` return `None`, which `exclusive_lease` yields as `Error(Fault(status=BUSY))` (exit 5, D40) — `ResourceBusyError` is imported for the exit-code algebra but the engine never raises it. `leased[T]` holds the lease for the duration of `action` (a thunk run only under an `Ok` lease) and threads its `Result` out unchanged; an `OSError` at the lock fd → `_diagnose(exc)` then `Fault(FAULTED)`. Lock paths per §7: `build-<closure>.lock`, `mutation.lock`, `bridge.lock`, `package-stage.lock` via `settings.artifact(ArtifactKind.LOCKS, …)`.

```python
def _stale(owner: _LeaseOwner, tolerance: float) -> bool:        # D34: dead/reused holder → steal
    try:
        proc = psutil.Process(owner.pid)
        with proc.oneshot():                                     # one batched is_running + create_time
            return not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance)
    except (psutil.NoSuchProcess, psutil.AccessDenied):
        return True

def _claim(fd: int, resource: str, *, run_id: str, tolerance: float) -> _LeaseOwner | None:  # None == live → BUSY
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)                          # never block (LOCK_NB)
        return _write_owner(fd, resource, run_id=run_id)
    except BlockingIOError:
        held = os.read(fd, 4096)
        owner = _DECODER.decode(held) if held else None
        _stamp_holder(owner, run_id=run_id)                     # span attrs: holder.(pid,create_time) + run_id
        match owner is not None and not _stale(owner, tolerance):
            case True:
                return None                                      # live → caller yields Fault(BUSY)
            case False:
                _LOG.warning("lease.steal", resource=resource, run_id=run_id, lost=_holder(owner))   # before
                won = (_FLOCK(fd, _LOCK_EX | _LOCK_NB), _write_owner(fd, resource, run_id=run_id))[1]  # steal
                _LOG.info("lease.stolen", resource=resource, run_id=run_id, lost=_holder(owner), won=_holder(won))  # after
                return won

@contextlib.contextmanager
def exclusive_lease(resource: str, run_id: str, *, settings: AssaySettings,
                    project: str = "", mode: str = "exclusive") -> Generator[Result[_Held, Fault]]:
    path = settings.artifact(ArtifactKind.LOCKS, f"{resource}.lock")
    path.parent.mkdir(parents=True, exist_ok=True)
    fd = os.open(str(path), _LOCKS_OPEN_FLAGS, _LOCK_MODE)
    try:
        match _claim(fd, resource, run_id=run_id, tolerance=settings.lease_drift_tolerance):
            case None:
                yield Error(Fault((), status=RailStatus.BUSY, message=f"{resource} held by a live process"))
            case owner:
                stamped = msgspec.structs.replace(owner, run_id=run_id, cwd=str(settings.root),
                                                  project=project, mode=mode)
                os.ftruncate(fd, 0); os.lseek(fd, 0, os.SEEK_SET); os.write(fd, msgspec.json.encode(stamped))
                yield Ok(_Held(stamped))
    finally:
        os.ftruncate(fd, 0); _FLOCK(fd, _LOCK_UN); os.close(fd)  # truncate-on-release
```
> The doc snippet writes the steal as a tuple-indexed expression for brevity; the implementation uses two statements (`_FLOCK(...)` then `won = _write_owner(...)`) so the before/after `_LOG` rail brackets the actual steal. `leased`'s `OSError` arm also calls `_diagnose(exc)` before its `Fault(FAULTED)`.

## [5][EXTENSIBILITY]

A new program is one `Tool` row (no engine change); a new runner is one `Runner` member with a `prefix` payload; a scope-aware splice for a non-dotnet runner is one added `_splice` `match` arm — the executor surface (`run_check`/`fan_out`) never grows entrypoints. A new execution backend (e.g. a container exec) is one added `_run_process_backend` `match` arm keyed off `settings.exec_target` — `_capture`/`_stream`/`_guarded` are untouched, and the existing local + `ssh://` arms keep their behavior; the backend axis is config (`exec_target`), the streaming axis is data (`Mode.stream`), and the two compose as nested `match` arms in one seam.

## [6][CONSIDERATIONS]

- `CapacityLimiter` (not `Semaphore`) is the correct fan-out primitive: it tracks borrow/release per task so a task killed mid-flight returns its token automatically (lib-anyio gotcha). `Semaphore` would require manual release pairing and strand capacity on cancellation. `acquire_on_behalf_of` is available if a future pooled-resource (warm MSBuild node) needs token accounting decoupled from task identity.
- `start_soon` is fire-and-forget: a raised exception inside `_into` is collected by `__aexit__` and re-raised as an `ExceptionGroup` on group exit, *bypassing* the slot (lib-anyio gotcha). `_into` cannot raise because it only awaits `_spawn`, whose inner `_guarded` `try/except` converts every spawn failure into an `Error(Fault)` value before the result reaches the slot — so the group always exits cleanly and slots stay total. Drop that `try/except` and a single `OSError` collapses the whole fan-out into an unhandled `ExceptionGroup`.
- Deadline partial-results use `move_on_after`, not `fail_after`: on elapse the scope cancels silently (no `TimeoutError`) so the group `__aexit__` runs to completion and `fan_out` still returns total slots, back-filling each unfinished `None` with `Fault(status=TIMEOUT)` (D47). `fail_after` would raise across the group boundary and forfeit the slots already filled — the asymmetry between the per-spawn `fail_after` (raise → one `Fault`) and the group-level `move_on_after` (silent → partial) is deliberate.
- Span pairing (D50) is fully delegated to the woven `traced` aspect: each `_guarded` gets one child span keyed `check.tool.name` with `run_id`/`tool` attributes — the engine opens **no** parent span of its own (the rail's own `traced` layer is the parent). Retry correlation therefore binds in `traced` at the engine seam, not in `logged` (which the engine never weaves), so a retried spawn's attempts share the tool span and the structlog `claim` keys correlate via the shared `run_id` attribute without a hand-rolled `engine.fan_out` span.
- Structured teardown on cancel must shield the reaper: `process.kill()` then `async with anyio.CancelScope(shield=True): await process.wait()`, so a `fail_after`/`move_on_after` deadline or parent-group cancellation never leaves an orphaned dotnet/Stryker child. This matters most for streaming (`Mode.stream`) rows where `open_process` tees output to a bounded tail deque + artifact log; the shielded wait is the only guarantee the child PID is reaped before the lease releases.
- Diagnostics are **inherent and knob-free** — there is no `--diagnose` flag and no setting. `_diagnose` fires at *every* `Fault` (the two `_guarded` arms + the `leased` `OSError`) so a Fault always auto-carries an agent's two most-wanted forensics: the exact exception (`span.record_exception`, which captures the stacktrace as a span event) and a one-`oneshot` resource snapshot (`rss`/`cpu`/`num_fds`). They are **span events, never stdout** — a fault storm cannot pollute the one-Envelope wire, and a non-recording span (no OTel exporter configured) absorbs both calls as no-ops, so the cost is zero when tracing is off. `num_fds` is POSIX-only; `_num_fds` degrades to `-1` rather than masking the very fault it annotates.
- The lease steal is **observable on two channels at once**: span attributes (`holder.pid`/`holder.create_time`/`assay.run_id`, queryable in a trace backend) and structlog `lease.steal`→`lease.stolen` lines (`lost`/`won` holder identities, visible in the stderr rail). The before/after bracket is the only way a fleet operator can answer "which run recycled my lock and when" without reconstructing it from PID reuse — the recorded `create_time` makes the `lost` holder unambiguous even after the PID is reissued.
- The spawn `cwd` is the **local-exec boundary**: `str(UPath(check.cwd or settings.root).path)` strips any `UPath` protocol to a bare local-FS string before it reaches `open_process`/`run_process`. `memory://` and remote `UPath` backends exist for *artifact I/O* (the `ArtifactStore` seam) and test isolation, never for a child process working directory — a local subprocess can only `chdir` into a real local path. Routing a non-local `settings.root` into a child cwd would be a category error; `.path` enforces the boundary at the one site every spawn flows through. The `exec_target` REMOTE-EXEC axis is **orthogonal** to the `UPath` I/O axis: `exec_target` moves the *process* off-box (over asyncssh), while `settings.root`/`ArtifactStore` decide where *bytes* live. The same local cwd string is reused as the remote `cd` prefix, so a `ssh://` run roots at the path the local FS would have — the two axes never conflate (artifact I/O still rides `UPath`; process exec rides `exec_target`).
- The **local hot path is provably unchanged**: `_run_process_backend`'s `case "":` arm is the verbatim prior `_stream`/`_capture` body (`anyio.open_process`+dual-tee+`_reap`, or `anyio.run_process(check=False)`), reached by a single `match` arm with no asyncssh on the call stack. `_capture`/`_stream` are thin one-line forwarders; `import asyncssh` is module-level but executes zero code on a local run. So the only added local cost is one `match settings.exec_target` discriminant — the engine remains the original on the dominant path.
- REMOTE-EXEC carries two asyncssh-specific quirks the seam absorbs once. (1) **No native cwd** — asyncssh has no working-directory channel, so cwd rides a `shlex.quote`d `cd <cwd> && …` command prefix. (2) **`AcceptEnv` whitelist** — `conn.run(env=…)` only sets vars the remote `sshd` whitelists, which would silently drop the scope-isolation overlay; env is therefore inlined as `KEY=value` command-line exports, which always take effect. `encoding=None` keeps the wire raw `bytes` so `Completed{stdout,stderr}` is byte-identical to the local arm; `exit_status` (`None` on a remote signal kill) folds via `or 0` exactly as the local streaming arm. Streaming teardown shields `proc.close()` + `wait_closed()` so a deadline cancel reaps the remote channel before the lease releases, mirroring the local `_reap`.
