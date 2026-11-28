#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["anyio", "cyclopts", "duckdb", "msgspec"]
#
# [tool.ty.environment]
# python-platform = "all"
#
# [tool.uv]
# prerelease = "allow"
# ///
"""Reads GitHub Actions run history through gh into duckdb and reports concurrency waits, the last job, job and step percentiles, cache outcomes, and cold jobs, or one run's failures."""

from bisect import bisect_right
from datetime import datetime
from enum import auto, StrEnum
import io
from itertools import groupby
import math
import os
from pathlib import Path
import re
import subprocess
import sys
from typing import Annotated
import zipfile
lazy from collections.abc import Iterable, Iterator

import anyio
from cyclopts import App, Parameter, validators
import duckdb
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------


class _Conclusion(StrEnum):
    SUCCESS = auto()
    FAILURE = auto()
    CANCELLED = auto()
    SKIPPED = auto()
    TIMED_OUT = auto()
    NEUTRAL = auto()
    ACTION_REQUIRED = auto()
    STARTUP_FAILURE = auto()
    STALE = auto()


class _Outcome(StrEnum):
    HIT = auto()
    PARTIAL = auto()
    MISS = auto()
    SAVED = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_PAGE = 100
_CALLS = 8
_CONTEXT = 12
_SHOWN = 20
_SHARE = 0.1
_WAITED = 60
_HOSTED = "GitHub Actions"
_LOGGED = 60
_FAILED = frozenset({_Conclusion.FAILURE, _Conclusion.TIMED_OUT, _Conclusion.STARTUP_FAILURE})
_OUTCOMES = frozendict({"Cache hit for": _Outcome.HIT, "Cache hit for restore-key": _Outcome.PARTIAL, "Cache not found for input keys": _Outcome.MISS, "Cache saved with key": _Outcome.SAVED})
_LINE = re.compile(rf"^(?P<at>\S+) (?:(?P<outcome>{'|'.join(map(re.escape, _OUTCOMES))}): (?P<key>[^,\s]+)|##\[error\](?P<error>.*))$")
_ESCAPE = re.compile(r"\x1b\[[0-9;]*[A-Za-z]")
_STAMP = re.compile(r"^\S+Z ")
_UNSAFE = str.maketrans({"/": "_", "\\": "_", ":": None, "*": None, "?": None, '"': None, "<": None, ">": None, "|": None})
_URL = re.compile(r"https://github\.com/(?P<repo>[^/]+/[^/]+)/actions/runs/(?P<run>\d+)(?:/.*)?")
_REPO = re.compile(r"[\w.-]+/[\w.-]+")
_STORE = Path(os.environ.get("XDG_CACHE_HOME") or Path.home() / ".cache") / "github-actions"

# --- [MODELS] ---------------------------------------------------------------------------


class _Step(msgspec.Struct, frozen=True, gc=False):
    name: str
    number: int
    conclusion: _Conclusion | None = None
    started_at: datetime | None = None
    completed_at: datetime | None = None


class _Job(msgspec.Struct, frozen=True, gc=False):
    id: int
    run_id: int
    name: str
    created_at: datetime
    started_at: datetime
    conclusion: _Conclusion | None = None
    completed_at: datetime | None = None
    labels: tuple[str, ...] = ()
    runner_group_name: str | None = None
    steps: tuple[_Step, ...] = ()

    @property
    def folder(self) -> str:
        return self.name.translate(_UNSAFE)

    @property
    def seconds(self) -> float:
        return (self.completed_at or self.started_at).timestamp() - self.started_at.timestamp()


class _Run(msgspec.Struct, frozen=True, gc=False):
    id: int
    name: str
    path: str
    event: str
    head_sha: str
    created_at: datetime
    display_title: str
    html_url: str
    run_attempt: int = 1
    head_branch: str | None = None
    conclusion: _Conclusion | None = None


class _Page(msgspec.Struct, frozen=True, gc=False):
    total_count: int
    workflow_runs: tuple[_Run, ...]


class _Cache(msgspec.Struct, frozen=True, gc=False):
    key: str
    ref: str
    size_in_bytes: int


class _Usage(msgspec.Struct, frozen=True, gc=False):
    active_caches_size_in_bytes: int
    active_caches_count: int


class _Limit(msgspec.Struct, frozen=True, gc=False):
    max_cache_size_gb: int


class _Annotation(msgspec.Struct, frozen=True, gc=False):
    path: str
    start_line: int
    annotation_level: str
    message: str


class _Event(msgspec.Struct, frozen=True, gc=False, array_like=True):
    job: str
    step: int
    outcome: _Outcome
    key: str


class _Failure(msgspec.Struct, frozen=True, gc=False):
    job: str
    step: int
    lines: tuple[str, ...]


class _Log(msgspec.Struct, frozen=True, gc=False):
    events: tuple[_Event, ...] = ()
    failures: tuple[_Failure, ...] = ()
    read: bool = False


class _Record(msgspec.Struct, frozen=True, gc=False):
    run: int
    jobs: tuple[_Job, ...]
    events: tuple[_Event, ...]
    read: bool


class _Repository(msgspec.Struct, frozen=True, gc=False):
    name: str


class _Target(msgspec.Struct, frozen=True, gc=False):
    repo: str
    run: int


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _gh(limiter: anyio.CapacityLimiter, *arguments: str, check: bool = True) -> bytes:
    async with limiter:
        done = await anyio.run_process(("gh", "api", *arguments), check=check)
    return done.stdout if done.returncode == 0 else b""


def _lines(text: str, /) -> list[str]:
    return [_STAMP.sub("", _ESCAPE.sub("", line)) for line in text.splitlines()]


def _log(jobs: Iterable[_Job], raw: bytes, /) -> _Log:
    if not raw:
        return _Log()
    named = {job.folder: job for job in jobs}
    events: list[_Event] = []
    failures: list[_Failure] = []
    with zipfile.ZipFile(io.BytesIO(raw)) as archive:
        texts = [
            (named[folder], archive.read(member).decode("utf-8", "replace"))
            for member in archive.namelist()
            if "/" not in member and (folder := member.partition("_")[2].removesuffix(".txt")) in named
        ]
    for job, text in texts:
        ran = [step for step in job.steps if step.started_at]
        starts = [step.started_at.timestamp() for step in ran if step.started_at]
        hits = [(index, datetime.fromisoformat(hit["at"]).timestamp(), hit) for index, line in enumerate(text.splitlines()) if (hit := _LINE.match(line))]
        events.extend(_Event(job.name, ran[slot].number if (slot := bisect_right(starts, at) - 1) >= 0 else 0, _OUTCOMES[hit["outcome"]], hit["key"]) for _, at, hit in hits if hit["outcome"])
        windows = [(step, step.started_at.timestamp(), (step.completed_at or step.started_at).timestamp() + 1) for step in ran if step.conclusion in _FAILED and step.started_at]
        spans = [(step, [index for index, at, hit in hits if hit["error"] is not None and start <= at < end]) for step, start, end in windows]
        failures.extend(_Failure(job=job.name, step=step.number, lines=tuple(_lines(text)[max(errors[0] - _CONTEXT, 0) : errors[-1] + 1])) for step, errors in spans if errors)
    return _Log(events=tuple(events), failures=tuple(failures), read=True)


async def _jobs(limiter: anyio.CapacityLimiter, repo: str, run: int, attempt: int, /) -> tuple[_Job, ...]:
    raw = await _gh(limiter, "--paginate", "--jq", ".jobs[]", f"repos/{repo}/actions/runs/{run}/attempts/{attempt}/jobs?per_page={_PAGE}")
    return tuple(msgspec.json.Decoder(_Job).decode_lines(raw))


async def _record(limiter: anyio.CapacityLimiter, repo: str, run: int, /) -> _Record:
    stored = anyio.Path(_STORE / repo / f"{run}.json")
    try:
        return msgspec.json.decode(await stored.read_bytes(), type=_Record)
    except FileNotFoundError, msgspec.DecodeError:
        jobs, raw = await anyio.gather(_jobs(limiter, repo, run, 1), _gh(limiter, f"repos/{repo}/actions/runs/{run}/attempts/1/logs", check=False))
    log = _log(jobs, raw)
    record = _Record(run=run, jobs=jobs, events=log.events, read=log.read)
    await stored.parent.mkdir(parents=True, exist_ok=True)
    await stored.write_bytes(msgspec.json.encode(record))
    return record


async def _listed(limiter: anyio.CapacityLimiter, repo: str, workflow: str | None, count: int, branch: str | None, event: str | None, /) -> tuple[tuple[_Run, ...], int]:
    scope = f"workflows/{workflow}/runs" if workflow is not None else "runs"
    query = "".join(f"&{name}={value}" for name, value in (("branch", branch), ("event", event)) if value is not None)
    path = f"repos/{repo}/actions/{scope}?status=completed&per_page={_PAGE}{query}"
    first = msgspec.json.decode(await _gh(limiter, f"{path}&page=1"), type=_Page)
    later = range(2, math.ceil(min(count, first.total_count) / _PAGE) + 1)
    pages = await anyio.gather(*(_gh(limiter, f"{path}&page={page}") for page in later))
    runs = (*first.workflow_runs, *(run for raw in pages for run in msgspec.json.decode(raw, type=_Page).workflow_runs))
    return runs[:count], first.total_count


def _duration(seconds: float | None, /) -> str:
    whole = round(seconds or 0)
    hours, rest = divmod(whole, 3600)
    return f"{hours}h{rest // 60:02d}m" if hours else f"{rest // 60}m{rest % 60:02d}s" if rest >= 60 else f"{rest}s"


def _size(count: float | None, /) -> str:
    units = ("B", "KiB", "MiB", "GiB", "TiB")
    exponent = min(max(int(count or 0).bit_length() - 1, 0) // 10, len(units) - 1)
    return f"{(count or 0) / (1 << 10 * exponent):.1f}{units[exponent]}"


def _row(reference: str, /, **fields: object) -> str:
    return " ".join((reference, *(f"{label}={value}" for label, value in fields.items() if value not in {None, "", 0})))


def _cut(shown: int, total: int, /) -> str:
    return f"{shown} of {total}" if shown < total else f"{total}"


def _loaded(runs: tuple[_Run, ...], records: tuple[_Record, ...], caches: tuple[_Cache, ...], /) -> duckdb.DuckDBPyConnection:
    database = duckdb.connect()
    jobs = [
        {
            **msgspec.structs.asdict(job),
            "steps": [msgspec.structs.asdict(step) for step in job.steps],
            "seconds": job.seconds,
            "hosted": job.runner_group_name == _HOSTED,
            "labels": ",".join(job.labels),
        }
        for record in records
        for job in record.jobs
    ]
    events = [msgspec.structs.asdict(event) | {"run": record.run} for record in records for event in record.events]
    tables = {"runs": [msgspec.structs.asdict(run) for run in runs], "jobs": jobs, "events": events, "caches": [msgspec.structs.asdict(cache) for cache in caches]}
    for name, rows in ((name, rows) for name, rows in tables.items() if rows):
        database.sql("SELECT unnest(row) FROM (SELECT unnest($rows) AS row)", params={"rows": rows}).create(name)
    database.execute("CREATE TABLE IF NOT EXISTS events (job VARCHAR, step INTEGER, outcome VARCHAR, key VARCHAR, run BIGINT)")
    database.execute("CREATE TABLE IF NOT EXISTS caches (key VARCHAR, ref VARCHAR, size_in_bytes BIGINT)")
    database.execute("CREATE MACRO shape(text) AS regexp_replace(text, '[0-9a-f]{16,}', '*', 'g')")
    database.execute("""
        CREATE VIEW ran AS
        SELECT j.*, epoch(j.started_at) - epoch(j.created_at) AS queue, epoch(j.created_at) - epoch(min(j.created_at) OVER (PARTITION BY j.run_id)) AS start,
               EXISTS (SELECT 1 FROM events e WHERE e.run = j.run_id AND e.job = j.name AND e.outcome = 'miss') AS cold
        FROM jobs j WHERE j.conclusion IS DISTINCT FROM 'skipped'
    """)
    database.execute("""
        CREATE VIEW spans AS
        SELECT r.id, epoch(min(j.created_at)) - epoch(r.created_at) AS pending, epoch(max(j.completed_at)) - epoch(min(j.created_at)) AS wall, arg_max(j.name, j.completed_at) AS last
        FROM runs r JOIN ran j ON j.run_id = r.id GROUP BY r.id, r.created_at
    """)
    return database


def _analysis(database: duckdb.DuckDBPyConnection, default: str, usage: _Usage, limit: int | None, /) -> Iterator[str]:
    facts = database.execute(
        """
        SELECT quantile_cont(pending, 0.5), quantile_cont(pending, 0.9), count(*) FILTER (pending > $waited), quantile_cont(wall, 0.5), quantile_cont(wall, 0.9),
               (SELECT string_agg(last || ' ' || ended, ', ' ORDER BY ended DESC, last) FROM (SELECT last, count(*) AS ended FROM spans GROUP BY last))
        FROM spans
        """,
        {"waited": _WAITED},
    ).fetchall()
    for pending50, pending90, waited, wall50, wall90, last in facts:
        yield f"  pending: p50 {_duration(pending50)}, p90 {_duration(pending90)}, {waited} runs waited past {_WAITED}s"
        yield f"  wall: p50 {_duration(wall50)}, p90 {_duration(wall90)}"
        yield f"  last: {last}"
    jobs = database.execute(
        """
        WITH j AS (SELECT name, conclusion = 'skipped' AS skipped, run_id FROM jobs),
             s AS (SELECT name, count(*) FILTER (skipped) AS skipped FROM j GROUP BY name),
             f AS (SELECT a.name, count(*) AS flaky FROM ran a JOIN runs r ON r.id = a.run_id
                   WHERE list_contains($failed, a.conclusion) AND r.run_attempt > 1 AND r.conclusion = 'success' GROUP BY a.name)
        SELECT s.name, string_agg(DISTINCT labels, ' '), count(a.name), quantile_cont(seconds, 0.5), quantile_cont(seconds, 0.9),
               max(seconds) FILTER (a.conclusion = 'success'), quantile_cont(queue, 0.9), quantile_cont(start, 0.5),
               count(*) FILTER (list_contains($failed, a.conclusion)), any_value(f.flaky), any_value(s.skipped), sum(ceil(seconds / 60)) FILTER (hosted),
               count(*) FILTER (cold), quantile_cont(seconds, 0.5) FILTER (cold), quantile_cont(seconds, 0.5) FILTER (NOT cold)
        FROM s LEFT JOIN ran a ON a.name = s.name LEFT JOIN f ON f.name = s.name GROUP BY s.name ORDER BY 5 DESC NULLS LAST, 1
        """,
        {"failed": sorted(_FAILED)},
    ).fetchall()
    steps = database.execute(
        """
        WITH s AS (SELECT name AS job, unnest(steps, recursive := true) FROM ran),
             t AS (SELECT job, shape(name) AS name, min(number) AS number, quantile_cont(epoch(completed_at) - epoch(started_at), 0.5) AS p50,
                          quantile_cont(epoch(completed_at) - epoch(started_at), 0.9) AS p90
                   FROM s WHERE conclusion IS DISTINCT FROM 'skipped' AND started_at IS NOT NULL GROUP BY job, shape(name))
        SELECT t.job, t.name, t.p50, t.p90 FROM t JOIN (SELECT name, quantile_cont(seconds, 0.9) AS p90 FROM ran GROUP BY name) j ON j.name = t.job
        WHERE t.p90 >= $share * j.p90 ORDER BY t.job, t.number
        """,
        {"share": _SHARE},
    ).fetchall()
    shown = {job: tuple(rows) for job, rows in groupby(steps, key=lambda row: row[0])}
    yield f"jobs: {len(jobs)}"
    for name, runner, count, p50, p90, longest, queue, start, failed, flaky, skipped, billed, cold, cold50, warm50 in jobs:
        timings = {"p50": _duration(p50), "p90": _duration(p90), "max": _duration(longest), "queue": _duration(queue), "start": _duration(start)} if count else {}
        yield f"  {_row(name, runner=runner, n=f'{count}', **timings, failed=failed, flaky=flaky, skipped=skipped, billed=f'{int(billed)}min' if billed else None)}"
        yield from (f"    {_row(step, p50=_duration(s50), p90=_duration(s90))}" for _, step, s50, s90 in shown.get(name, ()))
        yield from (f"    {_row('cold', n=cold, p50=_duration(cold50), warm=_duration(warm50))}",) * bool(cold)
    caches = database.execute(
        """
        WITH c AS (SELECT shape(key) AS shape, count(*) AS entries, sum(size_in_bytes) AS size,
                          count(*) FILTER (ref <> $ref) AS other FROM caches GROUP BY shape),
             e AS (SELECT shape(key) AS shape, count(*) FILTER (outcome = 'hit') AS hit,
                          count(*) FILTER (outcome = 'partial') AS partial, count(*) FILTER (outcome = 'miss') AS miss, count(*) FILTER (outcome = 'saved') AS saved FROM events GROUP BY shape)
        SELECT coalesce(c.shape, e.shape), c.entries, c.size, c.other, e.hit, e.partial, e.miss, e.saved FROM c FULL JOIN e ON e.shape = c.shape
        ORDER BY c.size DESC NULLS LAST, e.miss + e.partial DESC
    """,
        {"ref": f"refs/heads/{default}"},
    ).fetchall()
    yield f"caches: {_cut(min(len(caches), _SHOWN), len(caches))} keys, {_size(usage.active_caches_size_in_bytes)} of {f'{limit}GiB' if limit is not None else 'an unread limit'}, {usage.active_caches_count} entries"
    yield from (
        f"  {_row(shape, entries=entries, size=_size(size) * bool(size), other=other, hit=hit, partial=partial, miss=miss, saved=saved)}"
        for shape, entries, size, other, hit, partial, miss, saved in caches[:_SHOWN]
    )


def _analyzed(
    repo: str, workflow: str | None, runs: tuple[_Run, ...], total: int, records: tuple[_Record, ...], caches: tuple[_Cache, ...], usage: _Usage, limit: int | None, default: str, /
) -> Iterator[str]:
    counts = sorted((f"{conclusion}", sum(1 for _ in group)) for conclusion, group in groupby(sorted(f"{run.conclusion}" for run in runs)))
    read = sum(record.read for record in records)
    failed = next((run for run in runs if run.conclusion in _FAILED), None)
    yield f"runs {repo} {workflow if workflow is not None else 'all'}"
    yield from (f"  window: {oldest.created_at:%Y-%m-%dT%H:%MZ}..{newest.created_at:%Y-%m-%dT%H:%MZ}" for oldest, newest in ((runs[-1], runs[0]),) if runs)
    yield f"  runs: {_cut(len(runs), total)}, {', '.join(f'{count} {name}' for name, count in counts) or 'none completed'}"
    yield from (f"  cut: {total - len(runs)} older runs past --runs {len(runs)}",) * (len(runs) < total)
    yield f"  first attempt: {sum(run.run_attempt == 1 and run.conclusion == _Conclusion.SUCCESS for run in runs)} of {len(runs)} success"
    yield from (f"  gap: {len(records) - read} runs hold no logs, cache outcomes count {read} runs",) * (read < len(records))
    yield from _analysis(_loaded(runs, records, caches), default, usage, limit) if any(record.jobs for record in records) else ()
    yield from (f"next: uv run --locked --script {Path(__file__).resolve()} {run.html_url}" for run in (failed,) if run)


def _diagnosed(run: _Run, jobs: tuple[_Job, ...], log: _Log, notes: dict[int, tuple[_Annotation, ...]], /) -> Iterator[str]:
    first = min((job.created_at for job in jobs), default=run.created_at)
    last = max((job.completed_at or job.started_at for job in jobs), default=first)
    steps = {(job.name, step.number): step.name for job in jobs for step in job.steps}
    failed = {job: tuple(step for step in job.steps if step.conclusion in _FAILED) for job in jobs if job.conclusion in _FAILED}
    lines = {(failure.job, failure.step): failure.lines for failure in log.failures}
    yield f"run {run.id} {run.name} {run.conclusion}"
    yield f"  url: {run.html_url}"
    yield f"  commit: {run.head_sha[:9]} {run.display_title}"
    yield f"  event: {run.event} {run.head_branch or ''}".rstrip()
    yield f"  attempt: {run.run_attempt}"
    yield f"  pending: {_duration(first.timestamp() - run.created_at.timestamp())}"
    yield f"  wall: {_duration(last.timestamp() - first.timestamp())}"
    yield from ("  gap: logs expired or unread, failures list steps alone",) * (not log.read)
    yield f"jobs: {len(jobs)}"
    yield from (
        f"  {_row(job.name, runner=','.join(job.labels), result=f'{job.conclusion}', queue=_duration(job.started_at.timestamp() - job.created_at.timestamp()), duration=_duration(job.seconds))}"
        for job in jobs
    )
    yield f"failed: {len(failed)} jobs, {sum(map(len, failed.values()))} steps"
    for job, chosen in failed.items():
        yield f"  {_row(job.name, result=f'{job.conclusion}')}"
        yield from (f"    annotation: {note.path}:{note.start_line} {note.annotation_level} {next(iter(note.message.splitlines()), '')}" for note in notes.get(job.id, ()))
        for step in chosen:
            shown = lines.get((job.name, step.number), ())
            yield f"    {_row(f'step {step.number} {step.name}', result=f'{step.conclusion}')}"
            yield from (f"      cut: {len(shown) - _LOGGED} earlier lines",) * (len(shown) > _LOGGED)
            yield from (f"      log: {line}" for line in shown[-_LOGGED:])
    yield f"caches: {len(log.events)}"
    yield from (f"  {_row(f'{event.job} / {steps.get((event.job, event.step), event.step)}', outcome=f'{event.outcome}', key=event.key)}" for event in log.events)


def _target(target: str | None, current: str, /) -> _Repository | _Target:
    match target:
        case None:
            return _Repository(name=current)
        case str() if target.isdigit():
            return _Target(repo=current, run=int(target))
        case str() if hit := _URL.fullmatch(target):
            return _Target(repo=hit["repo"], run=int(hit["run"]))
        case str() if _REPO.fullmatch(target):
            return _Repository(name=target)
        case _:
            raise ValueError(target)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def _analyze(limiter: anyio.CapacityLimiter, repo: str, workflow: str | None, count: int, branch: str | None, event: str | None, /) -> Iterator[str]:
    (runs, total), default, raw, usage, limit = await anyio.gather(
        _listed(limiter, repo, workflow, count, branch, event),
        _gh(limiter, "--jq", ".default_branch", f"repos/{repo}"),
        _gh(limiter, "--paginate", "--jq", ".actions_caches[]", f"repos/{repo}/actions/caches?per_page={_PAGE}"),
        _gh(limiter, f"repos/{repo}/actions/cache/usage"),
        _gh(limiter, f"repos/{repo}/actions/cache/storage-limit", check=False),
    )
    records = tuple(await anyio.gather(*(_record(limiter, repo, run.id) for run in runs)))
    caches = tuple(msgspec.json.Decoder(_Cache).decode_lines(raw))
    ceiling = msgspec.json.decode(limit, type=_Limit).max_cache_size_gb if limit else None
    return _analyzed(repo, workflow, runs, total, records, caches, msgspec.json.decode(usage, type=_Usage), ceiling, default.decode().strip())


async def _diagnose(limiter: anyio.CapacityLimiter, repo: str, number: int, /) -> Iterator[str]:
    run = msgspec.json.decode(await _gh(limiter, f"repos/{repo}/actions/runs/{number}"), type=_Run)
    jobs, raw = await anyio.gather(_jobs(limiter, repo, run.id, run.run_attempt), _gh(limiter, f"repos/{repo}/actions/runs/{run.id}/attempts/{run.run_attempt}/logs", check=False))
    failed = tuple(job for job in jobs if job.conclusion in _FAILED)
    notes = await anyio.gather(*(_gh(limiter, f"repos/{repo}/check-runs/{job.id}/annotations") for job in failed))
    annotations = {job.id: msgspec.json.decode(note, type=tuple[_Annotation, ...]) for job, note in zip(failed, notes, strict=True)}
    return _diagnosed(run, jobs, _log(jobs, raw), annotations)


app = App(help=__doc__, result_action="sys_exit", default_parameter=Parameter(negative=()))


@app.default
async def runs(
    target: str | None = None,
    /,
    *,
    workflow: str | None = None,
    count: Annotated[int, Parameter(name="--runs", validator=validators.Number(gte=1, lte=1000))] = 100,
    branch: str | None = None,
    event: str | None = None,
) -> int:
    """Report run history of a repository, or the failures of one run.

    Args:
        target: Repository as owner/name, run URL, or run id, the gh repository of the working folder when absent; a run selects its failures.
        workflow: Workflow file name or id the history reads, every workflow when absent.
        count: Newest completed runs the history reads, GitHub listing at most 1000.
        branch: Branch the history reads, every branch when absent.
        event: Triggering event the history reads, every event when absent.

    Returns:
        Exit status, the status of a failed gh call.
    """
    limiter = anyio.CapacityLimiter(_CALLS)
    report: Iterator[str] = iter(())
    refused: subprocess.CalledProcessError | None = None
    try:
        current = "" if target is not None and "/" in target else (await _gh(limiter, "--jq", ".full_name", "repos/{owner}/{repo}")).decode().strip()
        match _target(target, current):
            case _Repository(name=repo):
                report = await _analyze(limiter, repo, workflow, count, branch, event)
            case _Target(repo=repo, run=number):
                report = await _diagnose(limiter, repo, number)
    except* subprocess.CalledProcessError as group:
        refused = next(error for error in group.exceptions if isinstance(error, subprocess.CalledProcessError))
    match refused:
        case subprocess.CalledProcessError(cmd=command, returncode=code, stderr=stderr):
            reason = next(iter((stderr or b"").decode().strip().splitlines()[-1:]), "no stderr")
            sys.stdout.write(f"runs {target}\n  failed: {' '.join(command)}, {reason}\n")
            return code
        case _:
            sys.stdout.writelines(f"{line}\n" for line in report)
            return 0


if __name__ == "__main__":
    app()
