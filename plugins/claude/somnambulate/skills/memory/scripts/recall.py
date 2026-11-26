#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["anyio", "msgspec"]
#
# [tool.uv]
# prerelease = "allow"
# ///
"""Replays recall requests through live sessions and scores which memory files each one opens."""

from collections import Counter
from dataclasses import dataclass
from datetime import datetime, UTC
from enum import auto, StrEnum
import os
from pathlib import Path
import re
import sys
lazy from collections.abc import Iterator, Mapping
lazy import tempfile

import anyio
import msgspec

# --- [CONSTANTS] ------------------------------------------------------------------------

_TURNS = 2
_TAG = "recall"
_SINCE = datetime.min.replace(tzinfo=UTC)

# --- [MODELS] ---------------------------------------------------------------------------


class _Verdict(StrEnum):
    HIT = auto()
    CO_LOAD = auto()
    ANSWERED = auto()
    MISS = auto()
    STEAL = auto()


class _Tagged(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=lambda name: name.removeprefix("_").lower()): ...


class _Request(_Tagged, frozen=True, kw_only=True):
    request: str


class _Should(_Request, frozen=True, kw_only=True):
    owner: str

    @property
    def files(self) -> frozenset[str]:
        return frozenset((self.owner,))


class _Near(_Request, frozen=True, kw_only=True):
    owner: str | None
    near: str

    @property
    def files(self) -> frozenset[str]:
        return frozenset(stem for stem in (self.owner, self.near) if stem is not None)


class _Session(_Tagged, frozen=True, kw_only=True):
    case: _Should | _Near
    run: int
    cost: float


class _Opened(_Session, frozen=True, kw_only=True):
    stems: frozenset[str]
    verdict: _Verdict
    reply: str
    applied: bool | None

    @property
    def counted(self) -> Counter[tuple[str | None, str]]:
        owner, near = self.case.owner, self.case.near if isinstance(self.case, _Near) else None
        tested = Counter({(near, "near"): near is not None, (near, "near opened"): near in self.stems, (owner, "graded"): self.applied is not None, (owner, "applied"): self.applied is True})
        return Counter({(owner, self.verdict): 1, (owner, "runs"): 1, (owner, "opened"): len(self.stems)}) + tested + Counter((stem, "stray") for stem in self.stems if stem != owner)


class _Failed(_Session, frozen=True, kw_only=True):
    cause: str


class _Baseline(msgspec.Struct, frozen=True, kw_only=True):
    case: _Should | _Near
    reply: str
    answered: bool
    cost: float


class _Result(msgspec.Struct, frozen=True, kw_only=True):
    files: frozendict[str, frozendict[str, int]]
    total: frozendict[str, int]


class _Input(msgspec.Struct, frozen=True):
    file_path: str = ""


class _Block(msgspec.Struct, frozen=True):
    name: str = ""
    input: _Input = _Input()


class _Message(msgspec.Struct, frozen=True):
    content: tuple[_Block, ...] | str = ()

    def stems(self, folder: Path, /) -> Iterator[str]:
        return (
            path.relative_to(folder).with_suffix("").as_posix()
            for block in self.content
            if isinstance(block, _Block) and block.name == "Read" and (path := Path(block.input.file_path)).is_relative_to(folder) and path.suffix == ".md" and path.name != "MEMORY.md"
        )


class _Grade(msgspec.Struct, frozen=True):
    verdict: bool = False


class _Event(msgspec.Struct, frozen=True):
    message: _Message = _Message()
    timestamp: datetime = _SINCE
    is_error: bool = False
    num_turns: int = 0
    total_cost_usd: float = 0.0
    result: str = ""
    structured_output: _Grade = _Grade()


# --- [TABLES] ---------------------------------------------------------------------------

_RATES = (("recall", (_Verdict.HIT, _Verdict.CO_LOAD, _Verdict.ANSWERED), "runs"), ("near-open", ("near opened",), "near"), ("applied", ("applied",), "graded"), ("breadth", ("opened",), "runs"))

# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True)
class _Launcher:
    limiter: anyio.CapacityLimiter
    root: Path
    folder: Path
    conditions: tuple[str, ...]

    async def __call__(self, cwd: Path, env: Mapping[str, str] | None, prompt: str, /, *args: str) -> tuple[_Event | None, list[_Event], str]:
        async with self.limiter:
            done = await anyio.run_process(
                ("claude", "-p", "--output-format", "stream-json", "--verbose", "--no-session-persistence", "--strict-mcp-config", *args), input=prompt.encode(), cwd=cwd, env=env, check=False
            )
        events = msgspec.json.Decoder(_Event).decode_lines(done.stdout)
        return next((event for event in events if event.num_turns), None), events, done.stderr.decode()

    async def graded(self, case: _Should | _Near, reply: str, ask: str, /) -> tuple[bool, float]:
        if not reply:
            return False, 0.0
        body = await anyio.Path(self.folder, f"{case.owner}.md").read_text(encoding="utf-8")
        question = f"Memory file `{case.owner}.md`:\n\n{body}\n\nRequest: {case.request}\n\nReply:\n\n{reply}\n\n{ask}"
        schema = '{"type":"object","properties":{"verdict":{"type":"boolean"}},"required":["verdict"]}'
        grade, _, _ = await self(self.folder, {**os.environ, "CLAUDE_CODE_DISABLE_AUTO_MEMORY": "1"}, question, "--model", "sonnet", "--tools", "", "--json-schema", schema)
        return bool(grade and grade.structured_output.verdict), grade.total_cost_usd if grade else 0.0

    async def opened(self, job: tuple[_Should | _Near, int], /) -> _Opened | _Failed:
        case, run = job
        result, events, stderr = await self(self.root, None, case.request, *self.conditions)
        stems = frozenset(stem for event in events for stem in event.message.stems(self.folder))
        match result:
            case None:
                return _Failed(case=case, run=run, cost=0.0, cause=stderr)
            case _Event(is_error=True, num_turns=turns, total_cost_usd=cost, result=cause) if turns <= _TURNS:
                return _Failed(case=case, run=run, cost=cost, cause=cause)
            case _Event(total_cost_usd=cost, result=reply):
                applied, grading = await self.graded(case, reply, "Does the reply apply what the memory file says for this request?") if case.owner in stems and reply else (None, 0.0)
                return _Opened(case=case, run=run, cost=cost + grading, stems=stems, verdict=_verdict(case.owner, stems), reply=reply, applied=applied)

    async def baseline(self, case: _Should | _Near, /) -> _Baseline:
        reply, _, stderr = await self(self.root, {**os.environ, "CLAUDE_CODE_DISABLE_AUTO_MEMORY": "1"}, case.request, *self.conditions)
        answer = reply.result if reply and not reply.is_error else ""
        answered, grading = await self.graded(
            case, answer, "This reply was written with no memory available. Does it already act as the memory file requires for this request, so opening the file would change nothing?"
        )
        return _Baseline(case=case, reply=answer or stderr, answered=answered, cost=(reply.total_cost_usd if reply else 0.0) + grading)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _verdict(owner: str | None, stems: frozenset[str], /) -> _Verdict:
    match owner in stems, any(stem != owner for stem in stems), owner:
        case True, False, _:
            return _Verdict.HIT
        case True, True, _:
            return _Verdict.CO_LOAD
        case False, False, None:
            return _Verdict.HIT
        case False, False, _:
            return _Verdict.MISS
        case _:
            return _Verdict.STEAL


def _roots(name: str, folder: Path, /) -> Iterator[Path]:
    for child in sorted(folder.iterdir()):
        match re.sub(r"[^a-zA-Z0-9]", "-", str(child)):
            case sanitized if sanitized == name and child.is_dir():
                yield child
            case sanitized if name.startswith(f"{sanitized}-") and child.is_dir():
                yield from _roots(name, child)


def _usage(source: Path, since: datetime, present: frozenset[str], /) -> Counter[str]:
    decoder, needle = msgspec.json.Decoder(_Event), str(source).encode()
    return Counter(
        stem
        for transcript in source.parent.rglob("*.jsonl")
        if transcript.stat().st_mtime > since.timestamp()
        for raw in transcript.read_bytes().splitlines()
        if needle in raw and (line := decoder.decode(raw)).timestamp > since
        for stem in line.message.stems(source)
        if stem in present
    )


def _rated(row: Mapping[str, int], before: Mapping[str, int], /) -> str:
    rates = {name: [sum(side.get(part, 0) for part in parts) / side[base] if side.get(base, 0) else None for side in (row, before)] for name, parts, base in _RATES}
    return ", ".join((
        *(f"{column} {row[column]}" for column in (*_Verdict, "stray") if row.get(column, 0)),
        *(f"{name} {now:.2f}" + ("" if then is None else f" ({now - then:+.2f})") for name, (now, then) in rates.items() if now is not None),
        f"reads {row.get('reads', 0)}",
    ))


async def _git(folder: Path, /, *args: str) -> tuple[int, str]:
    done = await anyio.run_process(("git", "-C", str(folder), *args), check=False)
    return done.returncode, done.stdout.decode().strip()


# --- [COMPOSITION] ----------------------------------------------------------------------


async def recall(memory: Path, /) -> int:
    """Run each request in scope 3 times (env `RECALL_RUNS` overrides), score recall, precision, usefulness, breadth, and usage per file, store them, and move tag `recall` to HEAD on a clean run."""
    folder = Path(await anyio.Path(memory).resolve())
    code, origin = await _git(folder, "remote", "get-url", "origin")
    source = Path(origin) if code == 0 else folder
    if (root := next(_roots(source.parent.name, Path(folder.anchor)), None)) is None:
        sys.stderr.write(f"no folder's path sanitizes to {source.parent.name}, the project folder holding this memory\n")
        return 1
    try:
        rows, held = msgspec.json.Decoder(_Should | _Near).decode_lines(await anyio.Path(folder, ".recall.jsonl").read_bytes()), ""
    except FileNotFoundError:
        rows, held = [], ", and the folder holds no rows, as .recall.jsonl is absent"
    script = await anyio.Path(__file__).resolve()
    indexed = await anyio.run_process(
        ("uv", "run", "--locked", "--script", str(script.with_name("index.py")), str(folder)), env={key: value for key, value in os.environ.items() if key != "CLAUDE_CODE_SESSION_ID"}, check=False
    )
    sys.stderr.write(f"index: {indexed.stdout.decode().strip().rpartition(chr(10))[2] or indexed.stderr.decode().strip()}\n")
    match await _git(folder, "rev-parse", "-q", "--verify", f"refs/tags/{_TAG}"):
        case 0, tag:
            changed = "\n".join([(await _git(folder, "diff", "--name-only", "--no-renames", _TAG, "--"))[1], (await _git(folder, "ls-files", "--others", "--exclude-standard"))[1]])
            stems = frozenset(path.stem for path in map(Path, changed.splitlines()) if path.suffix == ".md" and path.name != "MEMORY.md")
            cases = tuple(row for row in rows if row.files & stems)
            scope = f"rows naming {', '.join(sorted(stems)) or 'no file'}, changed since tag {_TAG} at {tag[:7]}, committed or not"
        case _:
            cases, scope = tuple(rows), f"every row, as tag {_TAG} is absent"
    match max([path async for path in anyio.Path(folder, ".recall-results").glob("*.json")], default=None):
        case None:
            before, since = _Result(files=frozendict(), total=frozendict()), _SINCE
        case previous:
            before, since = msgspec.json.decode(await previous.read_bytes(), type=_Result), datetime.fromisoformat(previous.stem)
    present = frozenset([path.stem async for path in anyio.Path(folder).glob("[!.]*.md")])
    reads = await anyio.to_thread.run_sync(_usage, source, since, present)
    runs = int(os.environ.get("RECALL_RUNS", "3"))
    stale = tuple(case for case in cases if not case.files <= present)
    jobs = [(case, run) for case in cases if case.files <= present for run in range(1, runs + 1)]
    sys.stderr.write(f"scope: {scope}{held}\n{len(cases) - len(stale)} requests, {runs} runs each, {len(jobs)} sessions from {root}, {len(stale)} stale rows skipped\n")
    settings = msgspec.json.encode({"autoMemoryDirectory": str(folder)}).decode()
    launch = _Launcher(
        anyio.CapacityLimiter(os.process_cpu_count() or 1), root, folder, ("--tools", "Read", "--max-turns", str(_TURNS), "--settings", settings, "--plugin-dir", str(script.parents[3]))
    )
    first = [await launch.opened(job) for job in jobs[:1]]
    sys.stderr.writelines(f"first session ${session.cost:.2f}, about ${session.cost * len(jobs):.2f} for {len(jobs)}\n" for session in first)
    sessions = [*first, *await anyio.amap(launch.opened, jobs[1:])]
    missed = list(dict.fromkeys(session.case for session in sessions if isinstance(session, _Opened) and session.verdict is _Verdict.MISS))
    baselines = await anyio.amap(launch.baseline, missed)
    answered = {baseline.case for baseline in baselines if baseline.answered}
    sessions = [
        msgspec.structs.replace(session, verdict=_Verdict.ANSWERED) if isinstance(session, _Opened) and session.verdict is _Verdict.MISS and session.case in answered else session
        for session in sessions
    ]
    scored = [session for session in sessions if isinstance(session, _Opened)]
    failed = [session for session in sessions if isinstance(session, _Failed)]
    tally = sum((session.counted for session in scored), Counter[tuple[str | None, str]]())
    columns = (*_Verdict, "stray", "runs", "opened", "near", "near opened", "graded", "applied")
    result = _Result(
        files=frozendict({
            stem: frozendict({column: count for column in columns if (count := tally[stem, column])} | ({"reads": reads[stem]} if reads[stem] else {}))
            for stem in sorted({stem for stem, _ in tally if stem is not None} | set(reads))
        }),
        total=frozendict(sum((Counter({column: count}) for (_, column), count in tally.items()), Counter({"reads": reads.total()}))),
    )
    broad = [stem for stem, row in result.files.items() if row.get("near opened", 0) * 2 > row.get("near", 0)]
    findings = Counter((session.verdict, session.case, session.stems) for session in scored if session.verdict is not _Verdict.HIT)
    passed = not failed and not stale and not broad and not any(verdict in {_Verdict.MISS, _Verdict.STEAL} for verdict, _, _ in findings)
    moved = passed and (await _git(folder, "tag", "--no-sign", "--force", _TAG, "HEAD"))[0] == 0
    stored = anyio.Path(folder, ".recall-results", f"{datetime.now(UTC):%Y%m%dT%H%M%SZ}.json")
    await stored.parent.mkdir(exist_ok=True)
    await stored.write_bytes(msgspec.json.encode(result, order="sorted"))
    with tempfile.NamedTemporaryFile(prefix="recall-", suffix=".json", delete=False) as details:
        details.write(msgspec.json.encode({"sessions": sessions, "baselines": baselines}))
    sys.stdout.writelines((
        *(
            f"{verdict} x{count}: {case.owner or 'no owner'} <- {case.request!r}, opened {', '.join(sorted(stems)) or 'nothing'}\n"
            for (verdict, case, stems), count in sorted(findings.items(), key=lambda item: (item[0][0], item[0][1].owner or ""))
        ),
        *(f"failed: {session.case.request!r} run {session.run}: {session.cause.strip()}\n" for session in failed),
        *(f"stale: {case.request!r} names {', '.join(sorted(case.files - present))}, which no memory file holds\n" for case in stale),
        *(f"broad: {stem} opened in {result.files[stem]['near opened']} of {result.files[stem]['near']} runs of near rows testing it\n" for stem in broad),
        *(f"{stem}: {_rated(row, before.files.get(stem, {}))}\n" for stem, row in result.files.items()),
        f"set: {_rated(result.total, before.total)}\n",
        f"baselines: {len(baselines)} missed requests, {len(answered)} answered with no memory\n",
        f"cost: ${sum(session.cost for session in sessions) + sum(baseline.cost for baseline in baselines):.2f} over {len(sessions)} sessions and {len(baselines)} baselines\n",
        f"stored: {stored}, compared with {'no earlier result' if since is _SINCE else f'the result of {since:%Y-%m-%d %H:%M} UTC'}\n",
        f"details: {details.name}\n",
        f"tag {_TAG}: {'moved to HEAD' if moved else 'stays, as a row is stale or broad, or a run missed, stole, or failed'}\n",
    ))
    return int(not passed)


if __name__ == "__main__":
    _, folder = sys.argv
    sys.exit(anyio.run(recall, Path(folder)))
