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
from enum import auto, StrEnum
import os
from pathlib import Path
import re
import sys
lazy from collections.abc import Iterator, Mapping
lazy import tempfile

import anyio
import msgspec
from msgspec.structs import replace

# --- [CONSTANTS] ------------------------------------------------------------------------

_RUNS = 3
_TURNS = 2
_TAG = "recall"

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

    @property
    def counted(self) -> tuple[tuple[str | type[msgspec.Struct] | None, str], ...]:
        return ((self.case.owner, self.verdict), (type(self.case), self.verdict), *((stem, "stray") for stem in self.stems if stem != self.case.owner))


class _Failed(_Session, frozen=True, kw_only=True):
    cause: str


class _Baseline(msgspec.Struct, frozen=True, kw_only=True):
    case: _Should | _Near
    reply: str
    answered: bool
    cost: float


class _Input(msgspec.Struct, frozen=True):
    file_path: str = ""


class _Block(msgspec.Struct, frozen=True):
    input: _Input = _Input()


class _Message(msgspec.Struct, frozen=True):
    content: tuple[_Block, ...] = ()


class _Grade(msgspec.Struct, frozen=True):
    answered: bool = False


class _Event(msgspec.Struct, frozen=True):
    message: _Message = _Message()
    is_error: bool = False
    num_turns: int = 0
    total_cost_usd: float = 0.0
    result: str = ""
    structured_output: _Grade = _Grade()


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True)
class _Launcher:
    limiter: anyio.CapacityLimiter
    root: Path
    folder: Path
    conditions: tuple[str, ...]

    async def __call__(self, env: Mapping[str, str] | None, prompt: str, /, *args: str) -> tuple[_Event | None, list[_Event], str]:
        async with self.limiter:
            done = await anyio.run_process(
                ("claude", "-p", "--output-format", "stream-json", "--verbose", "--no-session-persistence", "--strict-mcp-config", *args), input=prompt.encode(), cwd=self.root, env=env, check=False
            )
        events = msgspec.json.Decoder(_Event).decode_lines(done.stdout)
        return next((event for event in events if event.num_turns), None), events, done.stderr.decode()

    async def opened(self, job: tuple[_Should | _Near, int], /) -> _Opened | _Failed:
        case, run = job
        result, events, stderr = await self(None, case.request, *self.conditions)
        stems = frozenset(
            path.relative_to(self.folder).with_suffix("").as_posix()
            for event in events
            for block in event.message.content
            if block.input.file_path and (path := Path(block.input.file_path)).is_relative_to(self.folder) and path.suffix == ".md" and path.name != "MEMORY.md"
        )
        match result:
            case None:
                return _Failed(case=case, run=run, cost=0.0, cause=stderr)
            case _Event(is_error=True, num_turns=turns, total_cost_usd=cost, result=cause) if turns <= _TURNS:
                return _Failed(case=case, run=run, cost=cost, cause=cause)
            case _Event(total_cost_usd=cost):
                return _Opened(case=case, run=run, cost=cost, stems=stems, verdict=_verdict(case.owner, stems))

    async def baseline(self, case: _Should | _Near, /) -> _Baseline:
        unremembered = {**os.environ, "CLAUDE_CODE_DISABLE_AUTO_MEMORY": "1"}
        reply, _, stderr = await self(unremembered, case.request, *self.conditions)
        match reply:
            case _Event(is_error=False, result=answer, total_cost_usd=cost) if answer:
                body = await anyio.Path(self.folder, f"{case.owner}.md").read_text(encoding="utf-8")
                question = f"Memory file `{case.owner}.md`:\n\n{body}\n\nRequest: {case.request}\n\nReply written with no memory available:\n\n{answer}\n\nDoes the reply already act as the memory file requires for this request, so opening the file would change nothing?"
                grade, _, _ = await self(
                    unremembered, question, "--model", "sonnet", "--tools", "", "--json-schema", '{"type":"object","properties":{"answered":{"type":"boolean"}},"required":["answered"]}'
                )
                return _Baseline(case=case, reply=answer, answered=bool(grade and grade.structured_output.answered), cost=cost + (grade.total_cost_usd if grade else 0.0))
            case _:
                return _Baseline(case=case, reply=reply.result if reply else stderr, answered=False, cost=reply.total_cost_usd if reply else 0.0)


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


async def _git(folder: Path, /, *args: str) -> tuple[int, str]:
    done = await anyio.run_process(("git", "-C", str(folder), *args), check=False)
    return done.returncode, done.stdout.decode().strip()


# --- [COMPOSITION] ----------------------------------------------------------------------


async def recall(memory: Path, /) -> int:
    """Run each request in scope 3 times from the project root, score the memory files opened, and move tag `recall` to HEAD when none misses or steals."""
    folder = Path(await anyio.Path(memory).resolve())
    code, origin = await _git(folder, "remote", "get-url", "origin")
    name = (Path(origin) if code == 0 else folder).parent.name
    if (root := next(_roots(name, Path(folder.anchor)), None)) is None:
        sys.stderr.write(f"no folder's path sanitizes to {name}, the project folder holding this memory\n")
        return 1
    rows = msgspec.json.Decoder(_Should | _Near).decode_lines(await anyio.Path(folder, ".recall.jsonl").read_bytes())
    match await _git(folder, "rev-parse", "-q", "--verify", f"refs/tags/{_TAG}"):
        case 0, tag:
            changed = "\n".join([(await _git(folder, "diff", "--name-only", "--no-renames", _TAG, "--"))[1], (await _git(folder, "ls-files", "--others", "--exclude-standard"))[1]])
            stems = frozenset(path.stem for path in map(Path, changed.splitlines()) if path.suffix == ".md" and path.name != "MEMORY.md")
            cases = tuple(row for row in rows if row.files & stems)
            scope = f"rows naming {', '.join(sorted(stems)) or 'no file'}, changed since tag {_TAG} at {tag[:7]}, committed or not"
        case _:
            cases, scope = tuple(rows), f"every row, as tag {_TAG} is absent"
    present = frozenset([path.stem async for path in anyio.Path(folder).glob("[!.]*.md")])
    stale = tuple(case for case in cases if not case.files <= present)
    jobs = [(case, run) for case in cases if case.files <= present for run in range(1, _RUNS + 1)]
    sys.stderr.write(f"scope: {scope}\n{len(cases) - len(stale)} requests, {_RUNS} runs each, {len(jobs)} sessions from {root}, {len(stale)} stale rows skipped\n")
    settings = msgspec.json.encode({"autoMemoryDirectory": str(folder)}).decode()
    plugin = (await anyio.Path(__file__).resolve()).parents[3]
    launch = _Launcher(anyio.CapacityLimiter(os.process_cpu_count() or 1), root, folder, ("--tools", "Read", "--max-turns", str(_TURNS), "--settings", settings, "--plugin-dir", str(plugin)))
    first = [await launch.opened(job) for job in jobs[:1]]
    sys.stderr.writelines(f"first session ${session.cost:.2f}, about ${session.cost * len(jobs):.2f} for {len(jobs)}\n" for session in first)
    sessions = [*first, *await anyio.amap(launch.opened, jobs[1:])]
    missed = list(dict.fromkeys(session.case for session in sessions if isinstance(session, _Opened) and session.verdict is _Verdict.MISS))
    baselines = await anyio.amap(launch.baseline, missed)
    answered = {baseline.case for baseline in baselines if baseline.answered}
    sessions = [replace(session, verdict=_Verdict.ANSWERED) if isinstance(session, _Opened) and session.verdict is _Verdict.MISS and session.case in answered else session for session in sessions]
    scored = [session for session in sessions if isinstance(session, _Opened)]
    failed = [session for session in sessions if isinstance(session, _Failed)]
    tally = Counter(key for session in scored for key in session.counted)
    columns = (*_Verdict, "stray")
    findings = Counter((session.verdict, session.case, session.stems) for session in scored if session.verdict is not _Verdict.HIT)
    passed = not failed and not stale and not any(verdict in {_Verdict.MISS, _Verdict.STEAL} for verdict, _, _ in findings)
    moved = passed and (await _git(folder, "tag", "--no-sign", "--force", _TAG, "HEAD"))[0] == 0
    with tempfile.NamedTemporaryFile(prefix="recall-", suffix=".json", delete=False) as results:
        results.write(msgspec.json.encode({"sessions": sessions, "baselines": baselines}))
    sys.stdout.writelines((
        *(
            f"{verdict} x{count}: {case.owner or 'no owner'} <- {case.request!r}, opened {', '.join(sorted(stems)) or 'nothing'}\n"
            for (verdict, case, stems), count in sorted(findings.items(), key=lambda item: (item[0][0], item[0][1].owner or ""))
        ),
        *(f"failed: {session.case.request!r} run {session.run}: {session.cause.strip()}\n" for session in failed),
        *(f"stale: {case.request!r} names {', '.join(sorted(case.files - present))}, which no memory file holds\n" for case in stale),
        *(
            f"{stem or 'no owner'}: {', '.join(f'{column} {tally[stem, column]}' for column in columns if tally[stem, column])}\n"
            for stem in sorted({subject for subject, _ in tally if not isinstance(subject, type)}, key=lambda stem: stem or "")
            if any(tally[stem, column] for column in columns[1:])
        ),
        *(
            f"{info.tag}: {', '.join(f'{verdict} {tally[info.cls, verdict]}' for verdict in _Verdict if tally[info.cls, verdict])} of {sum(tally[info.cls, verdict] for verdict in _Verdict)} runs\n"
            for info in msgspec.inspect.multi_type_info((_Should, _Near))
            if isinstance(info, msgspec.inspect.StructType) and any(tally[info.cls, verdict] for verdict in _Verdict)
        ),
        f"baselines: {len(baselines)} missed requests, {len(answered)} answered with no memory\n",
        f"cost: ${sum(session.cost for session in sessions) + sum(baseline.cost for baseline in baselines):.2f} over {len(sessions)} sessions and {len(baselines)} baselines\n",
        f"results: {results.name}\n",
        f"tag {_TAG}: {'moved to HEAD' if moved else 'stays, as a row is stale or a run missed, stole, or failed'}\n",
    ))
    return int(not passed)


if __name__ == "__main__":
    _, folder = sys.argv
    sys.exit(anyio.run(recall, Path(folder)))
