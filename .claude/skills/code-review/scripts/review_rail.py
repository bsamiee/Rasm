#!/usr/bin/env -S uv run
# /// script
# requires-python = ">=3.14"
# dependencies = ["cyclopts", "msgspec"]
# ///
# ruff: noqa: T201, D100, D101, D103

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import fcntl
from functools import reduce
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import tempfile
import time
from typing import Annotated, assert_never, Literal

from cyclopts import App, Parameter
import msgspec
from msgspec.structs import replace


# --- [TYPES] ----------------------------------------------------------------------------

type Severity = Literal["critical", "major", "minor", "trivial", "info"]
type Status = Literal["launched", "running", "completed", "failed", "stalled", "stopped"]
type Grouping = Literal["severity", "file"]
type Source = Literal["stream", "store"]


# --- [CONSTANTS] ------------------------------------------------------------------------

GRACE_S = 10.0
IDLE_STALL_S = 900.0
KILL_SETTLE_S = 5.0
PID_CAP = 99999
POLL_S = 5.0
SNIPPET = 120
STALL_AFTER_S = 2700.0
STORE_WINDOW_S = 120.0
LOCK_NAME = "launch.lock"
STATE_ROOT = ".cache/coderabbit"
SEVERITIES: tuple[Severity, ...] = ("critical", "major", "minor", "trivial", "info")
TERMINAL: frozenset[Status] = frozenset({"completed", "failed", "stopped"})
STORE = Path.home() / ".coderabbit" / "reviews"
SLUG = re.compile(r"[^A-Za-z0-9._]+")
APP = App(help="Supervise detached CodeRabbit agent reviews: launch, status, findings, stop.")
ENCODER = msgspec.json.Encoder()


# --- [MODELS] -----------------------------------------------------------------------------


class Counts(msgspec.Struct, frozen=True):
    critical: int = 0
    major: int = 0
    minor: int = 0
    trivial: int = 0
    info: int = 0


class ReviewContext(msgspec.Struct, frozen=True, tag_field="type", tag="review_context", rename="camel"):
    review_type: str = ""
    current_branch: str = ""
    base_branch: str = ""
    working_directory: str = ""


class PhaseEvent(msgspec.Struct, frozen=True, tag_field="type", tag="status"):
    phase: str = ""
    status: str = ""


class Heartbeat(msgspec.Struct, frozen=True, tag_field="type", tag="heartbeat"):
    status: str = ""


class Finding(msgspec.Struct, frozen=True, tag_field="type", tag="finding", rename="camel"):
    severity: Severity = "info"
    file_name: str = ""
    comment: str = ""
    codegen_instructions: str = ""
    suggestions: tuple[str, ...] = ()


class Complete(msgspec.Struct, frozen=True, tag_field="type", tag="complete", rename="camel"):
    status: str = ""
    findings: int = 0
    reviewed_files: tuple[str, ...] = ()
    message: str = ""


class ErrorEvent(msgspec.Struct, frozen=True, tag_field="type", tag="error", rename="camel"):
    error_type: str = ""
    message: str = ""
    recoverable: bool = False


type Event = ReviewContext | PhaseEvent | Heartbeat | Finding | Complete | ErrorEvent


class RunState(msgspec.Struct, frozen=True):
    pid: int
    pgid: int
    lstart: str
    argv: tuple[str, ...]
    scope: str
    started: float
    deadline: float
    status: Status
    offset: int = 0
    inode: int = 0
    counts: Counts = msgspec.field(default_factory=Counts)
    phase: str = ""
    ended: float | None = None
    error: str | None = None
    warning: str | None = None


class Delta(msgspec.Struct, frozen=True):
    offset: int
    inode: int
    events: tuple[Event, ...]
    noise: int


class Observation(msgspec.Struct, frozen=True):
    state: RunState
    verdict: Status
    fresh: tuple[Finding, ...]
    noise: int
    idle_s: float
    elapsed_s: float


class GitMeta(msgspec.Struct, frozen=True, rename="camel"):
    working_directory: str = ""
    base_branch: str = ""
    current_branch: str = ""
    timestamp: float = 0.0


class RichFinding(msgspec.Struct, frozen=True, rename="camel"):
    severity: Severity = "info"
    comment_category: str = ""
    indicator_types: tuple[str, ...] = ()
    file_name: str = ""
    start_line: int = 0
    end_line: int = 0
    title: str = ""
    comment: str = ""
    codegen_instructions: str = ""
    suggestions: tuple[str, ...] = ()
    fingerprint: str = ""


class Fault(msgspec.Struct, frozen=True):
    fault: str
    detail: str = ""


class Orphan(msgspec.Struct, frozen=True):
    pid: int
    cwd: str


class ProcessProbe(msgspec.Struct, frozen=True):
    pid: int
    alive: bool


class NewFinding(msgspec.Struct, frozen=True, rename="camel"):
    severity: Severity
    file_name: str
    snippet: str


class LaunchReceipt(msgspec.Struct, frozen=True):
    run: str
    pid: int
    pgid: int
    lstart: str
    scope: str
    argv: tuple[str, ...]
    state: str
    stream: str
    reaped: tuple[str, ...]
    orphans: tuple[Orphan, ...]


class StatusDigest(msgspec.Struct, frozen=True):
    run: str
    verdict: Status
    phase: str
    elapsed_s: float
    idle_s: float
    counts: Counts
    new_findings: tuple[NewFinding, ...]
    noise_lines: int
    process: ProcessProbe
    orphans: tuple[Orphan, ...]
    warning: str | None = None
    error: str | None = None


class FindingsDigest(msgspec.Struct, frozen=True):
    run: str
    source: Source
    verdict: Status
    total: int
    counts: Counts
    groups: dict[str, tuple[RichFinding, ...]]
    orphans: tuple[Orphan, ...]


class StopReceipt(msgspec.Struct, frozen=True):
    run: str
    pid: int
    pgid: int
    escalated: bool
    waited_s: float
    status: Status


# --- [BOUNDARIES] -------------------------------------------------------------------------

EVENTS = msgspec.json.Decoder(Event)
STATES = msgspec.json.Decoder(RunState)
META = msgspec.json.Decoder(GitMeta)
RICH = msgspec.json.Decoder(RichFinding)
# POSIX-only bindings localize checker suppressions for fcntl/os/signal members under the platform-agnostic ty environment.
_FLOCK, _LOCK_EX, _LOCK_NB = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB  # ty: ignore[possibly-missing-attribute]
_GETPGID, _KILLPG, _SIGKILL = os.getpgid, os.killpg, signal.SIGKILL  # ty: ignore[possibly-missing-attribute]


def emitted(payload: object) -> None:
    print(ENCODER.encode(payload).decode())


def refused(fault: Fault) -> int:
    emitted(fault)
    return 1


def written(path: Path, state: RunState) -> None:
    handle, staged = tempfile.mkstemp(dir=str(path.parent), prefix=".state-")
    os.write(handle, ENCODER.encode(state))
    os.fsync(handle)
    os.close(handle)
    Path(staged).replace(path)


def loaded(path: Path) -> RunState | Fault:
    try:
        return STATES.decode(path.read_bytes())
    except (OSError, msgspec.DecodeError) as defect:
        return Fault(fault="state-unreadable", detail=f"{path}: {type(defect).__name__}")


def decoded[T](decoder: msgspec.json.Decoder[T], path: Path) -> T | None:
    try:
        return decoder.decode(path.read_bytes())
    except OSError, msgspec.DecodeError:
        return None


def alive(pid: int) -> bool:
    if not 1 < pid <= PID_CAP:
        return False
    try:
        os.kill(pid, 0)
    except ProcessLookupError:
        return False
    except PermissionError:
        return True
    return True


def lstarted(pid: int) -> str:
    if not 1 < pid <= PID_CAP:
        return ""
    probe = subprocess.run(("ps", "-p", str(pid), "-o", "lstart="), capture_output=True, text=True, check=False)
    return probe.stdout.strip() if probe.returncode == 0 else ""


def identity_live(state: RunState) -> bool:
    return bool(state.lstart) and alive(state.pid) and lstarted(state.pid) == state.lstart


def group_signalled(pgid: int, signum: int) -> None:
    try:
        _KILLPG(pgid, signum)
    except ProcessLookupError:
        return


def resolved(directory: Path | None) -> Path | Fault:
    anchor = directory or Path.cwd()
    probe = subprocess.run(("git", "-C", str(anchor), "rev-parse", "--show-toplevel"), capture_output=True, text=True, check=False)
    return Path(probe.stdout.strip()) if probe.returncode == 0 else Fault(fault="not-a-repo", detail=str(anchor))


def branched(repo: Path) -> str:
    probe = subprocess.run(("git", "-C", str(repo), "rev-parse", "--abbrev-ref", "HEAD"), capture_output=True, text=True, check=False)
    return slugged(probe.stdout.strip() or "detached")


def working_dir(pid: int) -> str:
    try:
        probe = subprocess.run(("lsof", "-a", "-p", str(pid), "-d", "cwd", "-Fn"), capture_output=True, text=True, check=False, timeout=5.0)
    except subprocess.TimeoutExpired:
        return ""
    return next((line[1:] for line in probe.stdout.splitlines() if line.startswith("n")), "")


def orphaned(root: Path, repo: Path) -> tuple[Orphan, ...]:
    probe = subprocess.run(("pgrep", "-f", "coderabbit review"), capture_output=True, text=True, check=False)
    if probe.returncode != 0:
        return ()
    owned = {state.pid for _, state in states(root)}
    strays = (int(token) for token in probe.stdout.split() if token.isdigit())
    return tuple(Orphan(pid=pid, cwd=str(repo)) for pid in strays if pid not in owned and pid != os.getpid() and working_dir(pid) == str(repo))


def delta(stream: Path, offset: int, inode: int) -> Delta:
    try:
        stat = stream.stat()
        start = offset if stat.st_ino == inode and stat.st_size >= offset else 0
        with stream.open("rb") as feed:
            feed.seek(start)
            chunk = feed.read()
    except OSError:
        return Delta(offset=offset, inode=inode, events=(), noise=0)
    lines = chunk.split(b"\n")
    fragment = lines.pop()
    events: list[Event] = []
    noise = 0
    for line in lines:
        bare = line.strip()
        if not bare:
            continue
        if not bare.startswith(b"{"):
            noise += 1
            continue
        try:
            events.append(EVENTS.decode(bare))
        except msgspec.DecodeError:
            noise += 1
    return Delta(offset=start + len(chunk) - len(fragment), inode=stat.st_ino, events=tuple(events), noise=noise)


# --- [OPERATIONS] -------------------------------------------------------------------------


def slugged(text: str) -> str:
    return SLUG.sub("-", text).strip("-").lower() or "x"


def scoped(target: str | None, base: str | None, base_commit: str | None) -> str:
    parts = (target or "all", f"base-{slugged(base)}" if base else "", f"commit-{slugged(base_commit)[:12]}" if base_commit else "")
    return "-".join(part for part in parts if part)


def command(target: str | None, base: str | None, base_commit: str | None, directory: Path | None, config: tuple[Path, ...]) -> tuple[str, ...]:
    scope_flags = (
        *(("-t", target) if target else ()),
        *(("--base", base) if base else ()),
        *(("--base-commit", base_commit) if base_commit else ()),
        *(("--dir", str(directory)) if directory else ()),
        *(part for entry in config for part in ("-c", str(entry))),
    )
    return ("coderabbit", "review", "--agent", *scope_flags)


def bumped(counts: Counts, severity: Severity) -> Counts:
    match severity:
        case "critical":
            return replace(counts, critical=counts.critical + 1)
        case "major":
            return replace(counts, major=counts.major + 1)
        case "minor":
            return replace(counts, minor=counts.minor + 1)
        case "trivial":
            return replace(counts, trivial=counts.trivial + 1)
        case "info":
            return replace(counts, info=counts.info + 1)
        case _ as unreachable:
            assert_never(unreachable)


def totaled(counts: Counts) -> int:
    return counts.critical + counts.major + counts.minor + counts.trivial + counts.info


def promoted(status: Status) -> Status:
    return status if status in TERMINAL else "running"


def folded(state: RunState, moved: Delta, now: float) -> tuple[RunState, tuple[Finding, ...]]:
    def stepped(acc: tuple[RunState, tuple[Finding, ...]], event: Event) -> tuple[RunState, tuple[Finding, ...]]:
        live, fresh = acc
        match event:
            case ReviewContext():
                return replace(live, status=promoted(live.status)), fresh
            case PhaseEvent(status=step):
                return replace(live, phase=step, status=promoted(live.status)), fresh
            case Heartbeat():
                return replace(live, phase="reviewing", status=promoted(live.status)), fresh
            case Finding() as hit:
                return replace(live, counts=bumped(live.counts, hit.severity), status=promoted(live.status)), (*fresh, hit)
            case Complete(findings=declared):
                streamed = totaled(live.counts)
                warning = None if declared == streamed else f"stream carried {streamed} findings, complete declared {declared}"
                return replace(live, status="completed", phase="complete", ended=now, warning=warning), fresh
            case ErrorEvent() as blast:
                return replace(live, status="failed", ended=now, error=f"{blast.error_type}: {blast.message}"), fresh
            case _ as unreachable:
                assert_never(unreachable)

    seed: tuple[RunState, tuple[Finding, ...]] = (state, ())
    settled, fresh = reduce(stepped, moved.events, seed)
    return replace(settled, offset=moved.offset, inode=moved.inode), fresh


def observed(run_dir: Path, state: RunState, now: float, idle_after: float) -> Observation:
    stream = run_dir / "stream.jsonl"
    moved = delta(stream, state.offset, state.inode)
    settled, fresh = folded(state, moved, now)
    try:
        idle = now - stream.stat().st_mtime
    except OSError:
        idle = now - state.started
    verdict: Status
    if settled.status in TERMINAL:
        verdict = settled.status
    elif not identity_live(settled):
        settled = replace(settled, status="failed", ended=now, error="process died without a terminal event")
        verdict = "failed"
    elif now > settled.deadline or (idle > idle_after and settled.phase == "reviewing"):
        settled = replace(settled, status="stalled")
        verdict = "stalled"
    else:
        verdict = settled.status
    return Observation(state=settled, verdict=verdict, fresh=fresh, noise=moved.noise, idle_s=idle, elapsed_s=now - state.started)


def states(root: Path) -> tuple[tuple[Path, RunState], ...]:
    pairs = ((path.parent, loaded(path)) for path in sorted(root.glob("*/state.json")))
    return tuple((run_dir, state) for run_dir, state in pairs if isinstance(state, RunState))


def reaped(root: Path, now: float) -> tuple[str, ...]:
    marked: list[str] = []
    for run_dir, state in states(root):
        if state.status in TERMINAL:
            continue
        if alive(state.pid):
            if lstarted(state.pid) == state.lstart:
                continue
            written(run_dir / "state.json", replace(state, status="failed", ended=now, error="pid recycled: live pid carries a different lstart"))
            marked.append(run_dir.name)
            continue
        settled, _ = folded(state, delta(run_dir / "stream.jsonl", state.offset, state.inode), now)
        final = settled if settled.status in TERMINAL else replace(settled, status="failed", ended=now, error="process died without a terminal event")
        written(run_dir / "state.json", final)
        marked.append(run_dir.name)
    return tuple(marked)


def selected(root: Path, run: str | None) -> tuple[Path, RunState] | Fault:
    if run is not None:
        state = loaded(root / run / "state.json")
        return state if isinstance(state, Fault) else (root / run, state)
    pool = states(root)
    return max(pool, key=lambda pair: pair[1].started) if pool else Fault(fault="no-runs", detail=str(root))


def stored(repo: Path, state: RunState, now: float) -> tuple[RichFinding, ...]:
    low, high = state.started - STORE_WINDOW_S, (state.ended or now) + STORE_WINDOW_S
    best: tuple[int, Path] | None = None
    for meta_path in STORE.glob("*/*/reviews/*/git.json"):
        meta = decoded(META, meta_path)
        epoch_dir = meta_path.parent
        if meta is None or meta.working_directory != str(repo) or not low <= meta.timestamp <= high or not epoch_dir.name.isdigit():
            continue
        epoch = int(epoch_dir.name)
        if best is None or epoch > best[0]:
            best = (epoch, epoch_dir)
    if best is None:
        return ()
    found = (decoded(RICH, finding_path) for finding_path in sorted(best[1].glob("*.json")) if finding_path.name != "git.json")
    return tuple(hit for hit in found if hit is not None)


def projected(hit: Finding) -> RichFinding:
    return RichFinding(
        severity=hit.severity,
        file_name=hit.file_name,
        comment=hit.comment,
        codegen_instructions=hit.codegen_instructions,
        suggestions=hit.suggestions,
    )


def snipped(hit: Finding) -> NewFinding:
    return NewFinding(severity=hit.severity, file_name=hit.file_name, snippet=(hit.codegen_instructions or hit.comment)[:SNIPPET])


def counted(hits: tuple[RichFinding, ...]) -> Counts:
    return reduce(lambda acc, hit: bumped(acc, hit.severity), hits, Counts())


def grouped(hits: tuple[RichFinding, ...], grouping: Grouping) -> dict[str, tuple[RichFinding, ...]]:
    match grouping:
        case "severity":
            return {level: bucket for level in SEVERITIES if (bucket := tuple(hit for hit in hits if hit.severity == level))}
        case "file":
            return {name: tuple(hit for hit in hits if hit.file_name == name) for name in sorted({hit.file_name for hit in hits})}
        case _ as unreachable:
            assert_never(unreachable)


# --- [ENTRY] --------------------------------------------------------------------------------


def followed(run_dir: Path, state: RunState, idle_after: float) -> int:
    announced = state.phase == "reviewing"
    while True:
        watch = observed(run_dir, state, time.time(), idle_after)
        state = watch.state
        written(run_dir / "state.json", state)
        if not announced and state.phase == "reviewing" and watch.verdict not in TERMINAL:
            announced = True
            print(f"[REVIEWING] run={run_dir.name} pid={state.pid} elapsed={watch.elapsed_s:.0f}s")
        if watch.verdict in TERMINAL or watch.verdict == "stalled":
            tail = f" error={state.error}" if state.error else ""
            print(f"[{watch.verdict.upper()}] run={run_dir.name} findings={totaled(state.counts)} elapsed={watch.elapsed_s:.0f}s{tail}")
            return 0 if watch.verdict == "completed" else 1
        time.sleep(POLL_S)


@APP.command
def launch(
    *,
    target: Annotated[str | None, Parameter(name=("-t", "--target"))] = None,
    base: str | None = None,
    base_commit: str | None = None,
    directory: Annotated[Path | None, Parameter(name=("--dir", "--directory"))] = None,
    config: Annotated[tuple[Path, ...], Parameter(name=("-c", "--config"))] = (),
    stall_after: float = STALL_AFTER_S,
) -> int:
    repo = resolved(directory)
    if isinstance(repo, Fault):
        return refused(repo)
    root = repo / STATE_ROOT
    root.mkdir(parents=True, exist_ok=True)
    now = time.time()
    marks = reaped(root, now)
    strays = orphaned(root, repo)
    scope = scoped(target, base, base_commit)
    with (root / LOCK_NAME).open("a") as latch:
        try:
            _FLOCK(latch.fileno(), _LOCK_EX | _LOCK_NB)
        except BlockingIOError:
            return refused(Fault(fault="launch-locked", detail="another launch holds the critical section"))
        twin = next(
            (run_dir.name for run_dir, held in states(root) if held.status not in TERMINAL and held.scope == scope and identity_live(held)), None
        )
        if twin is not None:
            return refused(Fault(fault="duplicate-run", detail=f"live run {twin} already covers scope {scope}"))
        key = f"{branched(repo)}-{scope}-{int(now * 1000)}"
        run_dir = root / key
        run_dir.mkdir()
        stream = run_dir / "stream.jsonl"
        argv = command(target, base, base_commit, directory, config)
        try:
            with stream.open("ab") as sink:
                child = subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, cwd=str(repo), start_new_session=True)
        except OSError as spawn:
            return refused(Fault(fault="spawn-failed", detail=f"{argv[0]}: {spawn}"))
        pgid = _GETPGID(child.pid)
        if pgid != child.pid or pgid <= 1:
            os.kill(child.pid, signal.SIGTERM)
            return refused(Fault(fault="pgid-invariant", detail=f"pid {child.pid} landed in foreign group {pgid}"))
        state = RunState(
            pid=child.pid, pgid=pgid, lstart=lstarted(child.pid), argv=argv, scope=scope, started=now, deadline=now + stall_after, status="launched"
        )
        written(run_dir / "state.json", state)
    receipt = LaunchReceipt(
        run=key,
        pid=child.pid,
        pgid=pgid,
        lstart=state.lstart,
        scope=scope,
        argv=argv,
        state=str(run_dir / "state.json"),
        stream=str(stream),
        reaped=marks,
        orphans=strays,
    )
    emitted(receipt)
    return 0


@APP.command
def status(
    *,
    directory: Annotated[Path | None, Parameter(name=("--dir", "--directory"))] = None,
    run: str | None = None,
    follow: bool = False,
    idle_after: float = IDLE_STALL_S,
) -> int:
    repo = resolved(directory)
    if isinstance(repo, Fault):
        return refused(repo)
    root = repo / STATE_ROOT
    if not root.is_dir():
        return refused(Fault(fault="no-runs", detail=str(root)))
    now = time.time()
    reaped(root, now)
    strays = orphaned(root, repo)
    picked = selected(root, run)
    if isinstance(picked, Fault):
        return refused(picked)
    run_dir, state = picked
    if follow:
        return followed(run_dir, state, idle_after)
    watch = observed(run_dir, state, now, idle_after)
    written(run_dir / "state.json", watch.state)
    digest = StatusDigest(
        run=run_dir.name,
        verdict=watch.verdict,
        phase=watch.state.phase,
        elapsed_s=round(watch.elapsed_s, 1),
        idle_s=round(watch.idle_s, 1),
        counts=watch.state.counts,
        new_findings=tuple(snipped(hit) for hit in watch.fresh),
        noise_lines=watch.noise,
        process=ProcessProbe(pid=state.pid, alive=alive(state.pid)),
        orphans=strays,
        warning=watch.state.warning,
        error=watch.state.error,
    )
    emitted(digest)
    return 0


@APP.command
def findings(
    *,
    directory: Annotated[Path | None, Parameter(name=("--dir", "--directory"))] = None,
    run: str | None = None,
    severity: Severity | None = None,
    group: Grouping = "severity",
) -> int:
    repo = resolved(directory)
    if isinstance(repo, Fault):
        return refused(repo)
    root = repo / STATE_ROOT
    if not root.is_dir():
        return refused(Fault(fault="no-runs", detail=str(root)))
    now = time.time()
    reaped(root, now)
    strays = orphaned(root, repo)
    picked = selected(root, run)
    if isinstance(picked, Fault):
        return refused(picked)
    run_dir, state = picked
    hits = stored(repo, state, now) if state.status in TERMINAL else ()
    source: Source = "store" if hits else "stream"
    if not hits:
        moved = delta(run_dir / "stream.jsonl", 0, 0)
        hits = tuple(projected(event) for event in moved.events if isinstance(event, Finding))
    if severity is not None:
        hits = tuple(hit for hit in hits if hit.severity == severity)
    emitted(
        FindingsDigest(
            run=run_dir.name, source=source, verdict=state.status, total=len(hits), counts=counted(hits), groups=grouped(hits, group), orphans=strays
        )
    )
    return 0


@APP.command
def stop(
    *, directory: Annotated[Path | None, Parameter(name=("--dir", "--directory"))] = None, run: str | None = None, grace: float = GRACE_S
) -> int:
    repo = resolved(directory)
    if isinstance(repo, Fault):
        return refused(repo)
    root = repo / STATE_ROOT
    if not root.is_dir():
        return refused(Fault(fault="no-runs", detail=str(root)))
    now = time.time()
    reaped(root, now)
    picked = selected(root, run)
    if isinstance(picked, Fault):
        return refused(picked)
    run_dir, state = picked
    if state.status in TERMINAL:
        emitted(StopReceipt(run=run_dir.name, pid=state.pid, pgid=state.pgid, escalated=False, waited_s=0.0, status=state.status))
        return 0
    if state.pgid <= 1 or state.pgid != state.pid:
        return refused(Fault(fault="pgid-guard", detail=f"refusing killpg on group {state.pgid}"))
    if not identity_live(state):
        written(run_dir / "state.json", replace(state, status="failed", ended=now, error="process already dead at stop"))
        emitted(StopReceipt(run=run_dir.name, pid=state.pid, pgid=state.pgid, escalated=False, waited_s=0.0, status="failed"))
        return 0
    group_signalled(state.pgid, signal.SIGTERM)
    opened = time.time()
    while alive(state.pid) and time.time() - opened < grace:
        time.sleep(0.2)
    escalated = alive(state.pid)
    if escalated:
        group_signalled(state.pgid, _SIGKILL)
        settle = time.time() + KILL_SETTLE_S
        while alive(state.pid) and time.time() < settle:
            time.sleep(0.2)
    written(run_dir / "state.json", replace(state, status="stopped", ended=time.time()))
    emitted(
        StopReceipt(run=run_dir.name, pid=state.pid, pgid=state.pgid, escalated=escalated, waited_s=round(time.time() - opened, 2), status="stopped")
    )
    return 0


if __name__ == "__main__":
    sys.exit(APP(sys.argv[1:], result_action="return_value"))
