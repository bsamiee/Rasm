#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["anyio", "cyclopts", "expression", "msgspec", "rich"]
# ///
"""Previews a built Claude Code mod: validation, terminal paint per layout, recorded calls and trees, kit mounts per surface, and a headless session on the remote surfaces."""

from bisect import bisect_right
from difflib import SequenceMatcher
from enum import StrEnum
from functools import cache, partial, reduce
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from itertools import accumulate, groupby, pairwise, starmap, takewhile
import operator
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import time
from typing import Annotated, Any, override
lazy from collections import Counter
lazy from collections.abc import AsyncIterator, Iterable, Iterator, Mapping, Sequence
lazy import shlex
lazy import shutil
lazy import tempfile
lazy import threading

import anyio
from expression.extra.result import catch
import msgspec
lazy from anyio.abc import ByteReceiveStream, ByteSendStream
lazy from anyio.streams.buffered import BufferedByteReceiveStream
lazy from cyclopts import App, Parameter, Token
lazy from cyclopts.types import ExistingDirectory
lazy from cyclopts.validators import Number
lazy from rich.ansi import AnsiDecoder
lazy from rich.cells import cell_len
lazy from rich.style import Style
lazy from rich.text import Text

# --- [TYPES] ----------------------------------------------------------------------------


class _Layout(StrEnum):
    """Terminal renderer, its value the `CLAUDE_CODE_NO_FLICKER` value that selects it."""

    FULLSCREEN = "1"
    MAIN = "0"


class _Settled(StrEnum):
    """How a step ended, its value the report's wording over the step's seconds."""

    SETTLED = "settled {:.2f} s"
    UNSETTLED = "unsettled after {:.1f} s"
    UNCHANGED = "no change after {:.1f} s"
    KILLED = "killed after {:.1f} s"


# --- [CONSTANTS] ------------------------------------------------------------------------

_RECORDER = Path(__file__).resolve().parent / "recorder"
_REMOTE = ("desktop", "vscode", "mobile")
_SURFACES = ("terminal", *_REMOTE)
_LOGIN = ("HOME", "LANG", "LOGNAME", "PATH", "SHELL", "TMPDIR", "USER")
_SETTINGS = frozendict(remoteControlAtStartup=False, spinnerTipsEnabled=False, promptSuggestionEnabled=False, enabledPlugins=frozendict({"cc-plugin-promos@builtin": False}))
_TMUX = (
    *("set", "-g", "default-terminal", "tmux-256color", ";", "set", "-g", "focus-events", "on", ";", "set", "-g", "window-size", "manual", ";"),
    *("set", "-g", "bell-action", "none", ";", "set", "-g", "status", "off", ";"),
)
_FLAGS = ("bold", "dim", "italic", "underline", "reverse", "strike", "blink")
_FAILED = frozenset({"caught", "expired", "kept", "rejected", "skipped"})
_ENTRY = re.compile(r'\d{4}-\d\d-\d\dT\S+ \[(?P<level>[A-Z]+)\] (?:(?P<quoted>"(?:[^"\\]|\\.)*")\n?$|(?P<text>.*))', re.DOTALL)
_START = re.compile(r"\n(?=\d{4}-\d\d-\d\dT)")
_RECORD = re.compile(r"\[preview-recorder\] \$\.ui\.log \(to debug\): PREVIEW (?P<order>\d+) (?P<more>[01]) (?P<chunk>.*)", re.DOTALL)
_MODULE = re.compile(r"hooks module (?P<plugin>\S+)@inline (?P<verdict>(?:loaded|failed to load|reloaded).*)", re.DOTALL)
_STARTED = re.compile(r"session\.start: raised|hooks modules not loaded")
_LOGGED = re.compile(r"^\$\.ui\.log(?: \(to (?P<to>debug)\))?: ")
_SITE = re.compile(r"ui\.render\{component=(?P<component>[\w|?]+)(?:, requestId=(?P<request>[^,}]+))?")
_RAISED = re.compile(r"Raised on (?:every attached surface|the (?P<surfaces>[a-z, ]+?) surfaces? only)\.\s*\*/\s*(?P<component>\w+)\??:")
_BROKE = re.compile(r"\b(?:skipped|refused|threw|failed|did not load)\b")
_MEASURES = re.compile(r" in \d+(?:\.\d+)?ms|\(\d+ more times?\)|plugin-toast-\S+")
_QUIET = 0.5
_ECHO = 0.15
_TICK = 0.05
_ANSWER = 10.0
_EXIT = 5.0
_GAP = 2
_CUT = 400

# --- [MODELS] ---------------------------------------------------------------------------


class _Step(msgspec.Struct, frozen=True, tag_field="kind", tag=lambda name: name.removeprefix("_").lower()):
    value: Annotated[str, msgspec.Meta(min_length=1)]

    @property
    def label(self) -> str:
        return f"{self.__struct_config__.tag} {self.value}"


class _Key(_Step, frozen=True): ...


class _Type(_Step, frozen=True): ...


class _Wait(_Step, frozen=True):
    def __post_init__(self) -> None:
        """Refuses a value that does not compile as a regular expression."""
        re.compile(self.value)


class _Resize(_Step, frozen=True):
    value: Annotated[str, msgspec.Meta(pattern=r"^[1-9]\d* [1-9]\d*$")]


class _Call(_Step, frozen=True):
    value: Annotated[str, msgspec.Meta(pattern=r"^\S+ \{.*\}$")]

    def __post_init__(self) -> None:
        """Refuses an input that is not a JSON object."""
        msgspec.json.decode(self.value.partition(" ")[2], type=dict[str, Any])


type _Steps = _Key | _Type | _Wait | _Resize | _Call
type _Cells = tuple[tuple[str, str], ...]


class _Fact(msgspec.Struct, frozen=True):
    """One report line, `label: value`, its parts indented under it; a fault counts toward the verdict."""

    label: str
    value: str = ""
    parts: tuple[_Fact, ...] = ()
    fault: bool = False

    def lines(self, depth: int = 0, /) -> Iterator[str]:
        yield f"{'  ' * depth}{self.label}{f': {self.value}' if self.value else ''}"
        yield from (line for part in self.parts for line in part.lines(depth + 1))

    @property
    def faults(self) -> frozenset[tuple[str, str]]:
        return frozenset({*(((self.label, self.value),) if self.fault else ()), *(fault for part in self.parts for fault in part.faults)})


class _Finding(msgspec.Struct, frozen=True):
    message: str
    path: str = ""


class _Part(msgspec.Struct, frozen=True):
    errors: tuple[_Finding, ...] = ()
    warnings: tuple[_Finding, ...] = ()
    notes: tuple[str, ...] = ()


class _Validation(msgspec.Struct, frozen=True):
    success: bool
    manifest: _Part
    contents: tuple[_Part, ...] = ()

    @property
    def findings(self) -> tuple[_Fact, ...]:
        found = ((kind, held) for part in (self.manifest, *self.contents) for kind, kept in (("error", part.errors), ("warning", part.warnings)) for held in kept)
        return tuple(_Fact(kind, f"{f'{held.path}: ' if held.path else ''}{_cut(held.message)}", fault=kind == "error") for kind, held in found)

    def sites(self, raised: Iterable[str], /) -> tuple[tuple[str, str | None], ...]:
        named = tuple(raised)
        return tuple(
            dict.fromkeys(
                (component, hit["request"])
                for part in self.contents
                for note in part.notes
                for hit in _SITE.finditer(note)
                for component in (named if hit["component"] == "?" else hit["component"].split("|"))
            )
        )

    def targets(self, panes: tuple[str, ...], raised: Iterable[str], heard: frozenset[str] | None, /) -> tuple[tuple[str, str | None], ...]:
        sites = self.sites(raised if heard is None else (component for component in raised if component in heard))
        hooked = any(component == "Pane" for component, _ in sites)
        return (*(site for site in sites if site != ("Pane", None) or not panes), *(("Pane", pane) for pane in panes if hooked))


class _Manifest(msgspec.Struct, frozen=True):
    name: str


class _Link(msgspec.Struct, frozen=True):
    plugin: str
    outcome: str
    reason: str | None = None


class _Record(msgspec.Struct, frozen=True):
    order: int
    event: str
    origin: str
    args: Mapping[str, Any]
    answer: Any = None
    rewrote: Mapping[str, Any] = msgspec.field(default_factory=dict)
    links: tuple[_Link, ...] = ()

    @property
    def site(self) -> str:
        component = str(self.args.get("component", self.event))
        return _label(component, self.args.get("requestId")) if component == "Pane" else component


class _Entry(msgspec.Struct, frozen=True):
    level: str
    text: str


class _State(msgspec.Struct, frozen=True):
    label: str
    seconds: float
    settled: _Settled
    rows: tuple[_Cells, ...]
    stop: int


class _Live(msgspec.Struct, frozen=True):
    layout: _Layout
    seconds: float
    states: tuple[_State, ...]
    entries: tuple[_Entry, ...]
    records: tuple[tuple[int, _Record], ...]
    watched: _Fact | None


class _Drawn(msgspec.Struct, frozen=True):
    trees: tuple[tuple[str, str, Any], ...]
    fact: _Fact
    panes: tuple[str, ...] = ()


class _Plan(msgspec.Struct, frozen=True):
    """Choices one preview hands every stage, built once from the command line."""

    work: Path
    name: str
    steps: tuple[_Steps, ...]
    size: tuple[int, int]
    limit: float
    model: str

    @property
    def copy(self) -> Path:
        return self.work / "copy"

    @property
    def raised(self) -> frozendict[str, tuple[str, ...]]:
        declarations = self.copy / ".claude-plugin" / "types" / "claude-code" / "index.d.ts"
        text = catch(Path.read_text, exception=OSError)(declarations, encoding="utf-8").default_value("")
        return frozendict({hit["component"]: tuple(surface for surface in _SURFACES if hit["surfaces"] is None or surface in hit["surfaces"]) for hit in _RAISED.finditer(text)})

    @property
    def environment(self) -> dict[str, str]:
        login = {key: os.environ[key] for key in _LOGIN if key in os.environ}
        return {"LANG": "en_US.UTF-8", **login, "CLAUDE_CODE_SANDBOXED": "1", "ANTHROPIC_BASE_URL": self.model}

    def flags(self, log: Path, /) -> tuple[str, ...]:
        plugins = (part for folder in (self.work / "recorder", self.copy) for part in ("--plugin-dir", str(folder)))
        return ("--setting-sources", "project", "--strict-mcp-config", "--settings", str(self.work / "settings.json"), *plugins, "--debug-file", str(log))


# --- [SERVICES] -------------------------------------------------------------------------


class _Endpoint(BaseHTTPRequestHandler):
    """Model endpoint answering each streamed request with one fixed reply and keeping every request body, so no step spends tokens."""

    def __init__(self, bodies: list[bytes], /, *handled: Any) -> None:
        self.bodies = bodies
        super().__init__(*handled)

    def do_POST(self) -> None:
        body = self.rfile.read(int(self.headers.get("content-length", 0)))
        self.bodies.append(body)
        request = msgspec.json.decode(body or b"{}", type=dict[str, Any])
        streamed = request.get("stream") is True
        self._reply("text/event-stream" if streamed else "application/json", _answer(request) if streamed else b'{"input_tokens":1}')

    def do_GET(self) -> None:
        self._reply("application/json", b"{}")

    def _reply(self, kind: str, payload: bytes, /) -> None:
        self.send_response(200)
        self.send_header("content-type", kind)
        self.send_header("content-length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)

    @override
    def log_message(self, format: str, *args: Any) -> None:
        return


class _Log:
    """Debug log read forward from its last offset into whole entries, the target's own lines marking activity."""

    def __init__(self, path: Path, names: tuple[str, ...], /) -> None:
        self.path, self.names, self.offset, self.pending = anyio.Path(path), names, 0, ""
        self.entries: list[_Entry] = []
        self.active = float("-inf")

    async def read(self, *, final: bool = False) -> tuple[_Entry, ...]:
        chunk = b""
        if await self.path.exists():
            async with await anyio.open_file(self.path, "rb") as stream:
                await stream.seek(self.offset)
                chunk = await stream.read()
        self.offset += len(chunk)
        *whole, self.pending = _START.split(self.pending + chunk.decode(errors="replace") + ("\n" if final else ""))
        found = tuple(_Entry(hit["level"], msgspec.json.decode(hit["quoted"], type=str) if hit["quoted"] else hit["text"].rstrip("\n")) for text in whole if (hit := _ENTRY.match(text)))
        self.entries.extend(found)
        self.active = anyio.current_time() if any(name in entry.text for entry in found for name in self.names) else self.active
        return tuple(self.entries)


class _Tmux:
    """One tmux control client: replies to its commands in order, `%output` as screen activity, `%exit` as the session's end."""

    def __init__(self, commands: ByteSendStream, output: ByteReceiveStream, log: _Log, /) -> None:
        self.commands, self.output_stream, self.log = commands, output, log
        self.drawn = float("-inf")
        self.output, self.exited = anyio.Event(), anyio.Event()
        self.sender, self.replies = anyio.create_memory_object_stream[tuple[str, ...]](64)

    async def read(self) -> None:
        block: list[str] | None = None
        with anyio.CancelScope(shield=True):
            async for line in _lined(self.output_stream):
                match block, line.split(" ", 1)[0]:
                    case list(), "%end" | "%error":
                        await self.sender.send(tuple(block))
                        block = None
                    case list(), _:
                        block.append(line)
                    case None, "%begin":
                        block = []
                    case None, "%output":
                        self.drawn = anyio.current_time()
                        self.output.set()
                        self.output = anyio.Event()
                    case None, "%exit":
                        break
                    case _:
                        pass
        self.exited.set()

    async def run(self, command: str, /) -> tuple[str, ...]:
        await self.commands.send(f"{command}\n".encode())
        with anyio.move_on_after(_ANSWER):
            return await self.replies.receive()
        return ()

    async def client(self) -> tuple[str, int]:
        await self.commands.send(b"display -p 'preview #{client_name} #{pane_pid}'\n")
        with anyio.move_on_after(_ANSWER):
            while True:
                match " ".join((await self.replies.receive())[-1:]).split():
                    case ["preview", client, pid] if pid.isdecimal():
                        return client, int(pid)
                    case _:
                        pass
        raise ValueError("tmux control client: no reply to display")

    async def settle(self, since: float, limit: float, pattern: re.Pattern[str] | None, /, *, quiet: float = _QUIET) -> _Settled:
        mark = len(self.log.entries)
        with anyio.move_on_after(since + limit - anyio.current_time()):
            while not self.exited.is_set():
                await self.log.read()
                last = max(self.drawn, self.log.active)
                matched = pattern is None or any(pattern.search(entry.text) for entry in self.log.entries[mark:])
                if last > since and anyio.current_time() - last >= quiet and matched:
                    return _Settled.SETTLED
                with anyio.move_on_after(_TICK):
                    await self.output.wait()
        return _Settled.UNSETTLED if max(self.drawn, self.log.active) > since else _Settled.UNCHANGED

    async def act(self, step: _Steps | None, /) -> None:
        match step:
            case _Key(value=names):
                for part in re.split(r"(?<=\bEscape)\s+", names):
                    await self.run(f"send-keys -t preview {part}")
                    if part.endswith("Escape"):
                        await self.settle(anyio.current_time(), _QUIET, None, quiet=_ECHO)
            case _Type(value=text) | _Call(value=text):
                await self.run(f"send-keys -t preview -l {shlex.quote(text if isinstance(step, _Type) else f'call:{text}')}")
                await self.settle(anyio.current_time(), 2.0, None, quiet=_ECHO)
                await self.run("send-keys -t preview Enter")
            case _Resize(value=size):
                columns, rows = size.split()
                await self.run(f"resize-window -t preview -x {columns} -y {rows}")
            case _Wait() | None:
                pass

    async def screen(self) -> tuple[_Cells, ...]:
        return tuple(map(_cells, AnsiDecoder().decode("\n".join(await self.run("capture-pane -p -e -N -t preview")))))


class _Headless:
    """One `claude -p` stream-json session: control replies in order, `ui_*` pushes, and a result per user turn."""

    def __init__(self, commands: ByteSendStream, output: ByteReceiveStream, /) -> None:
        self.commands, self.output = commands, output
        self.pushes: list[dict[str, Any]] = []
        self.pushed, self.ended = anyio.Event(), anyio.Event()
        self.sender, self.replies = anyio.create_memory_object_stream[dict[str, Any]](64)
        self.asked = 0

    async def read(self) -> None:
        with anyio.CancelScope(shield=True):
            async for line in _lined(self.output):
                match msgspec.json.decode(line):
                    case {"type": "control_response", "response": dict(response)}:
                        await self.sender.send(response)
                    case {"type": "system", "subtype": str(kind)} as push if kind.startswith("ui_"):
                        self.pushes.append(push)
                        self.pushed.set()
                        self.pushed = anyio.Event()
                    case {"type": "result"}:
                        self.ended.set()
                        self.ended = anyio.Event()
                    case _:
                        pass

    async def ask(self, request: dict[str, Any], /) -> dict[str, Any]:
        self.asked += 1
        await self.commands.send(msgspec.json.encode({"type": "control_request", "request_id": f"r{self.asked}", "request": request}) + b"\n")
        with anyio.move_on_after(_ANSWER):
            return await self.replies.receive()
        return {"subtype": "error", "error": f"no answer in {_ANSWER:g} s"}

    async def say(self, text: str, /) -> None:
        ended = self.ended
        await self.commands.send(msgspec.json.encode({"type": "user", "message": {"role": "user", "content": text}}) + b"\n")
        with anyio.move_on_after(_ANSWER):
            await ended.wait()

    async def quiet(self) -> None:
        with anyio.move_on_after(_ANSWER):
            while True:
                with anyio.move_on_after(_QUIET) as gap:
                    await self.pushed.wait()
                if gap.cancelled_caught:
                    return


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _lined(stream: ByteReceiveStream, /) -> AsyncIterator[str]:
    buffered = BufferedByteReceiveStream(stream)
    while True:
        try:
            line = await buffered.receive_until(b"\n", sys.maxsize)
        except anyio.IncompleteRead:
            return
        yield line.decode(errors="replace")


def _answer(request: dict[str, Any], /) -> bytes:
    messages = request.get("messages") or [{}]
    content = next((message.get("content", "") for message in reversed(messages) if message.get("role") == "user"), "")
    blocks = ({"type": "text", "text": content},) if isinstance(content, str) else tuple(content)
    asked = next((block["text"] for block in blocks if block.get("type") == "text" and str(block.get("text")).startswith("call:")), None)
    match asked, request.get("tools"), any(block.get("type") == "tool_result" for block in blocks):
        case str(), list(), False:
            tool, _, given = asked.removeprefix("call:").partition(" ")
            block, delta, stop = {"type": "tool_use", "id": f"toolu_preview{len(messages)}", "name": tool, "input": {}}, {"type": "input_json_delta", "partial_json": given}, "tool_use"
        case _:
            block, delta, stop = {"type": "text", "text": ""}, {"type": "text_delta", "text": "PREVIEW-REPLY"}, "end_turn"
    events = (
        ("message_start", {"message": {"id": "msg_preview", "type": "message", "role": "assistant", "model": "preview", "content": [], "usage": {"input_tokens": 1, "output_tokens": 1}}}),
        ("content_block_start", {"index": 0, "content_block": block}),
        ("content_block_delta", {"index": 0, "delta": delta}),
        ("content_block_stop", {"index": 0}),
        ("message_delta", {"delta": {"stop_reason": stop}, "usage": {"output_tokens": 1}}),
        ("message_stop", {}),
    )
    return b"".join(f"event: {kind}\ndata: {msgspec.json.encode({'type': kind, **data}).decode()}\n\n".encode() for kind, data in events)


def _label(component: str, request: str | None, /) -> str:
    return component if request is None else f"{component} {request}"


def _cut(text: str, /) -> str:
    flat = text.replace("\n", "\\n")
    return flat if len(flat) <= _CUT else f"{flat[:_CUT]} (+{len(flat) - _CUT} chars)"


def _value(value: object, /) -> str:
    match value:
        case str():
            return value
        case bool():
            return str(value).lower()
        case _:
            return msgspec.json.encode(value).decode()


def _props(changed: Mapping[str, Any], /) -> str:
    return ", ".join(f"props.{key}={_cut(_value(value))}" for key, value in changed.items())


def _jsx(node: object, /) -> str:
    match node:
        case str() | int() | float():
            return str(node)
        case {"type": "engine"}:
            return "<engine/>"
        case {"type": str(kind), **element}:
            props, children = element.get("props"), element.get("children")
            attributes = "".join(f" {key}" if value is True else f" {key}={_value(value)}" for key, value in (props.items() if isinstance(props, dict) else ()))
            children = "".join(map(_jsx, children if isinstance(children, list) else ()))
            return f"<{kind}{attributes}>{children}</{kind}>" if children else f"<{kind}{attributes}/>"
        case _:
            return _value(node)


def _texts(node: object, /) -> Iterator[str]:
    match node:
        case str():
            yield node
        case {"children": list(children)}:
            yield from (text for child in children for text in _texts(child))
        case _:
            pass


@cache
def _look(style: Style, glyph: str, /) -> str:
    shown = Style(bgcolor=style.bgcolor, underline=style.underline, strike=style.strike) if not glyph.strip() and style.reverse is not True else style
    colors = (f"{lead}{color.name}" for lead, color in (("", shown.color), ("on ", shown.bgcolor)) if color is not None and not color.is_default)
    return " ".join((*colors, *(flag for flag in _FLAGS if getattr(shown, flag))))


def _cells(text: Text, /) -> _Cells:
    styles = [Style()] * len(text.plain)
    for span in text.spans:
        styles[span.start : span.end] = [span.style if isinstance(span.style, Style) else Style.parse(span.style)] * (span.end - span.start)
    looks = (_look(style, glyph) for glyph, style in zip(text.plain, styles, strict=True))
    return tuple(cell for glyph, look in zip(text.plain, looks, strict=True) for cell in ((glyph, look), *(("", look),) * (cell_len(glyph) - 1)))


def _spans(row: _Cells, before: _Cells | None, /) -> tuple[tuple[int, str, str], ...]:
    differing = (x for x, cell in enumerate(row) if (cell[0].strip() or cell[1]) and (before is None or x >= len(before) or before[x] != cell))
    bounds: tuple[tuple[int, int], ...] = reduce(lambda held, x: (*held[:-1], (held[-1][0], x)) if held and x - held[-1][1] <= _GAP + 1 else (*held, (x, x)), differing, ())
    runs = ((start, stop, groupby((x for x in range(start, stop + 1) if row[x][0].strip() or row[x][1]), key=lambda x: row[x][1])) for start, stop in bounds)
    return tuple(
        (
            start,
            "".join(glyph for glyph, _ in row[start : stop + 1]).rstrip() or "(blank)",
            ", ".join(f"{xs[0]}{f'-{xs[-1]}' if len(xs) > 1 else ''} {look}" for look, group in grouped if look for xs in (tuple(group),)),
        )
        for start, stop, grouped in runs
    )


def _screen(state: _State, previous: _State | None, /) -> tuple[_Fact, ...]:
    before = previous.rows if previous else ()
    codes = SequenceMatcher(a=before, b=state.rows, autojunk=False).get_opcodes()
    paired = (
        (index, before[old] if tag == "replace" and old < old_stop else None)
        for tag, old_start, old_stop, start, stop in codes
        if tag in {"replace", "insert"}
        for index, old in zip(range(start, stop), range(old_start, old_start + stop - start), strict=True)
    )
    patched = tuple((index, spans) for index, row in paired if (spans := _spans(state.rows[index], row)))
    gone = sum(max(0, (old_stop - old_start) - (stop - start)) for tag, old_start, old_stop, start, stop in codes if tag in {"replace", "delete"})
    folded = ((tuple(index for _, (index, _) in group), spans) for (_, spans), group in groupby(enumerate(patched), key=lambda item: (item[1][0] - item[0], item[1][1])))
    return (
        _Fact("rows", f"{len(patched)} {'drawn' if previous is None else 'changed'} of {len(state.rows)}" + (f", {gone} scrolled away" if gone else "")),
        *(
            _Fact(f"{indices[0] + 1:02}{f'-{indices[-1] + 1:02}' if len(indices) > 1 else ''} @{start} {text}", parts=(_Fact(runs),) if runs else ())
            for indices, spans in folded
            for start, text, runs in spans
        ),
    )


def _answered(answer: object, done: str, /) -> str:
    match answer:
        case {"deny": deny}:
            return f"denied: {deny}"
        case {"value": {"isPlaced": False, "reason": reason}} | {"isPlaced": False, "reason": reason}:
            return f"waits: {reason}"
        case {"value": {"isSent": False, "reason": reason}}:
            return f"not sent: {reason}"
        case _:
            return done


def _recorded(record: _Record, name: str, /) -> tuple[_Fact, ...]:
    args, event = record.args, record.event.removeprefix("ui.")
    beneath, own = any(link.plugin == name for link in record.links), name in {record.origin, args.get("plugin")}
    failed = (
        _Fact(f"fault {record.site}", f"{link.outcome}{f', {_cut(link.reason)}' if link.reason is not None else ''}", fault=True)
        for link in record.links
        if link.plugin == name and link.outcome in _FAILED
    )
    match record.event:
        case "ui.render" if record.answer is not None and (own or beneath):
            fact = _Fact(f"draw {record.site}", _cut(_jsx(record.answer)))
        case "ui.render" if record.rewrote and beneath:
            fact = _Fact(f"rewrite {record.site}", _props(record.rewrote))
        case "ui.render":
            fact = None
        case _ if not own:
            fact = None
        case "ui.toast":
            fact = _Fact(event, _cut(args["text"]) + (f", {args['timeoutMs']} ms" if "timeoutMs" in args else ""))
        case "ui.status":
            fact = _Fact(event, _cut(args["text"]) if args.get("text") is not None else "cleared")
        case "ui.open" | "ui.place":
            fact = _Fact(f"{event} {args['id']}", _answered(record.answer, "placed"))
        case "ui.close":
            fact = _Fact(f"close {args['id']}", f"by {args.get('origin', {}).get('kind', name)}")
        case "ui.notify":
            fact = _Fact(event, f"{_cut(args.get('text', ''))}, {_answered(record.answer, 'sent')}")
        case "command.register":
            fact = _Fact("command", f"/{args['name']}")
        case "ui.press" | "ui.input" | "ui.select":
            fact = _Fact(f"{event} {args.get('element')}", f"{args.get('component')}, {_answered(record.answer, 'ran')}")
        case "session.append":
            message = args.get("message", {})
            content = message.get("content", "")
            text = content if isinstance(content, str) else " ".join(block.get("text", "") for block in content)
            fact = _Fact(f"append {args.get('door')}", f"{message.get('type')}, {'model reads' if message.get('role') else 'model never reads'}, {_cut(text)}")
        case _:
            fact = _Fact(event, _cut(_value(args)))
    return (*((fact,) if fact else ()), *failed)


def _logged(name: str, entry: _Entry, /) -> _Fact | None:
    own = entry.text.removeprefix(f"[{name}] ")
    match entry, _MODULE.match(entry.text), _LOGGED.match(own):
        case _, hit, _ if hit is not None and hit["plugin"] == name:
            return _Fact("load", _cut(hit["verdict"]), fault=hit["verdict"].startswith("failed"))
        case _, _, hit if hit is not None and own != entry.text:
            return _Fact(f"log {hit['to']}" if hit["to"] else "log", _cut(_MEASURES.sub("", own[hit.end() :])))
        case _, _, _ if own != entry.text:
            return _Fact(_cut(_MEASURES.sub("", own)))
        case _Entry(level="ERROR" | "WARN" as level, text=text), _, _ if re.search(rf"(?<![\w-]){re.escape(name)}(?![\w-])", text) and _RECORD.match(text) is None:
            return _Fact(level.lower(), _cut(_MEASURES.sub("", text)), fault=True)
        case _:
            return None


def _engine(entries: Iterable[_Entry], name: str, /) -> tuple[_Fact, ...]:
    counted = Counter(found for entry in entries if (found := _logged(name, entry)))
    return tuple(msgspec.structs.replace(found, value=f"{found.value}, {count} times") if count > 1 else found for found, count in counted.items())


def _records(entries: Sequence[_Entry], /) -> tuple[tuple[int, _Record], ...]:
    chunks = groupby(((index, hit) for index, entry in enumerate(entries) if (hit := _RECORD.match(entry.text))), key=lambda item: item[1]["order"])
    whole = (group for _, found in chunks if (group := tuple(found))[-1][1]["more"] == "0")
    return tuple((group[-1][0], msgspec.json.decode("".join(hit["chunk"] for _, hit in group), type=_Record)) for group in whole)


def _steps(_: object, tokens: Sequence[Token], /) -> tuple[_Steps, ...]:
    try:
        steps: tuple[_Steps, ...] = msgspec.convert([dict(zip(("kind", "value"), token.value.split(":", 1))) for token in tokens], type=tuple[_Steps, ...])
    except (msgspec.ValidationError, msgspec.DecodeError, re.error) as error:
        raise ValueError(f"step: takes key:KEYS, type:TEXT, wait:REGEX, resize:COLUMNS ROWS, or call:TOOL JSON; {error}") from error
    return steps


def _signalled(pid: int, number: int, /) -> bool:
    try:
        os.kill(pid, number)
    except ProcessLookupError:
        return False
    except PermissionError:
        return True
    return True


def _owner(work: Path, /) -> int:
    held = work / "pid"
    return int(catch(Path.read_text, exception=OSError)(held, encoding="utf-8").default_value("") or 0)


def _purged(work: Path, held: Path, /) -> int:
    servers = sum(subprocess.run(("tmux", "-S", socket.name, "kill-server"), cwd=held, capture_output=True, check=False).returncode == 0 for socket in held.glob("*.sock"))
    subprocess.run(("claude", "purge", str(work), "--yes"), capture_output=True, check=False)
    catch(shutil.rmtree, exception=FileNotFoundError)(held)
    return servers


def _claimed(source: Path, target: Path, /) -> bool:
    try:
        source.rename(target)
    except OSError:
        return False
    return True


def _workspace(root: Path, name: str, /) -> tuple[Path, tuple[_Fact, ...]]:
    root.mkdir(parents=True, exist_ok=True)
    staged = Path(tempfile.mkdtemp(prefix=".claim-", dir=root))
    (staged / "pid").write_text(str(os.getpid()), encoding="utf-8")
    slot, swept = 0, list[_Fact]()
    while not _claimed(staged, work := root / (name if slot == 0 else f"{name}-{slot}")):
        trash = staged.with_name(f".swept-{staged.name}-{slot}")
        if (pid := _owner(work)) and not _signalled(pid, 0) and _claimed(work, trash):
            swept.append(_Fact(f"swept {work.name}", f"{_purged(work, trash)} tmux servers"))
        else:
            slot += 1
    return work, tuple(swept)


def _trees(live: Sequence[_Live], drawn: Sequence[tuple[str, _Drawn]], name: str, /) -> tuple[_Fact, ...]:
    finals = {record.site: _jsx(record.answer) for run in live for _, record in run.records if record.event == "ui.render" and record.answer is not None and record.args.get("plugin") in {name, None}}
    found = sorted(((site, _jsx(tree), f"{source}/{surface}") for source, stage in drawn for site, surface, tree in stage.trees), key=lambda item: item[:2])
    grouped = ((site, jsx, " ".join(where for *_, where in group)) for (site, jsx), group in groupby(found, key=lambda item: item[:2]))
    return tuple(_Fact(f"{site} {where}", "same as live") if finals.get(site) == jsx else _Fact(f"{site} {where}", parts=(_Fact(_cut(jsx)),)) for site, jsx, where in grouped)


def _model(bodies: Sequence[bytes], live: Sequence[_Live], name: str, /) -> _Fact:
    sent = tuple(body.decode(errors="replace") for body in bodies)
    texts = {
        text
        for run in live
        for _, record in run.records
        if name in {record.origin, record.args.get("plugin")}
        for text in (*_texts(record.answer), *(value for value in record.args.values() if isinstance(value, str) and record.origin == name))
        if len(text) >= 6 and any(text in body for body in sent)
    }
    return _Fact("model", f"{len(sent)} requests, {sum(map(len, bodies)):,} bytes", parts=tuple(_Fact("reads", _cut(text)) for text in sorted(texts)))


def _run(run: _Live, name: str, /) -> _Fact:
    stops = tuple(state.stop for state in run.states)
    grouped = {state: tuple(record for _, record in group) for state, group in groupby(run.records, key=lambda item: bisect_right(stops, item[0]))}
    records = tuple(grouped.get(index, ()) for index in range(len(stops)))
    finals = tuple({record.site: facts for record in sorted(held, key=lambda record: record.order) if record.event == "ui.render" and (facts := _recorded(record, name))} for held in records)
    shown = tuple(accumulate(finals[:-1], operator.or_, initial={}))
    engine = tuple(_engine(run.entries[start:stop], name) for start, stop in pairwise((0, *stops)))
    loaded = () if any((hit := _MODULE.match(entry.text)) and hit["plugin"] == name for entry in run.entries) else (_Fact("load", "no hooks module loaded", fault=True),)
    steps = (
        _Fact(
            f"step {state.label}",
            state.settled.format(state.seconds),
            parts=(
                *(fact for record in held if record.event != "ui.render" for fact in _recorded(record, name)),
                *(fact for site, facts in final.items() if before.get(site) != facts for fact in facts),
                *facts,
                *(_screen(state, run.states[index - 1] if index else None) if state.rows else ()),
            ),
        )
        for index, (state, held, final, before, facts) in enumerate(zip(run.states, records, finals, shown, engine, strict=True))
    )
    return _Fact(f"live {run.layout.name.lower()}", f"{len(run.states) - 2} steps, {run.seconds:.1f} s", parts=(*filter(None, (run.watched,)), *loaded, *steps))


def _drawing(raised: frozendict[str, tuple[str, ...]], site: str, request: dict[str, Any], answer: dict[str, Any], /) -> _Fact | None:
    match answer:
        case {"subtype": "success", "response": {"rewritten": True, "props": dict(props)}} if props != request["props"]:
            return _Fact(f"rewrite {site} {request['surface']}", _props({key: value for key, value in props.items() if request["props"].get(key) != value}))
        case {"subtype": "success"}:
            return None
        case {"error": error}:
            return _Fact(f"refused {site} {request['surface']}", _cut(str(error).split(",", 1)[0]), fault=bool(raised))
        case _:
            return _Fact(f"refused {site} {request['surface']}", _cut(_value(answer)), fault=bool(raised))


def _pushed(push: dict[str, Any], name: str, /) -> _Fact | None:
    match push:
        case {"subtype": "ui_toast", "plugin": plugin, "text": text} if plugin == name:
            return _Fact("toast", _cut(text))
        case {"subtype": "ui_status", "plugin": plugin, "text": text} if plugin == name:
            return _Fact("status", _cut(text or "cleared"))
        case {"subtype": "ui_panes", "panes": list(panes), "shown_id": shown} if any(pane.get("plugin") == name for pane in panes):
            return _Fact("panes", f"{', '.join(pane['id'] for pane in panes if pane.get('plugin') == name)}, shown {shown}")
        case {"subtype": "ui_notify" | "ui_placed" as kind, "plugin": plugin, **rest} if plugin == name:
            return _Fact(kind.removeprefix("ui_"), _cut(_value(rest)))
        case _:
            return None


# --- [COMPOSITION] ----------------------------------------------------------------------


async def _watched(socket: Path, title: str, /) -> _Fact | None:
    if shutil.which("zellij") is None:
        return None
    outside = {key: value for key, value in os.environ.items() if not key.startswith("ZELLIJ")}
    listed = (await anyio.run_process(("zellij", "list-sessions", "-n"), env=outside, check=False)).stdout
    exited = re.search(rb"^agents \[.*EXITED", listed, re.MULTILINE) is not None
    if exited:
        await anyio.run_process(("zellij", "delete-session", "--force", "agents"), env=outside, check=False)
    if exited or re.search(rb"^agents \[", listed, re.MULTILINE) is None:
        await anyio.run_process(("zellij", "attach", "--create-background", "agents"), env=outside, check=False)
    with anyio.move_on_after(_ANSWER):
        while b"terminal_" not in (await anyio.run_process(("zellij", "--session", "agents", "action", "list-panes"), env=outside, check=False)).stdout:
            pass
    viewer = ("tmux", "-S", socket.name, "attach", "-f", "read-only,ignore-size", "-t", "preview")
    opened = await anyio.run_process(("zellij", "--session", "agents", "run", "--close-on-exit", "--cwd", str(socket.parent), "--name", title, "--", *viewer), env=outside, check=False)
    pane = opened.stdout.decode().strip()
    if opened.returncode or not pane:
        return _Fact("watch", f"zellij run failed, exit {opened.returncode}, {_cut(opened.stderr.decode().strip())}")
    await anyio.run_process(("zellij", "--session", "agents", "action", "mute-pane", "--pane-id", pane), env=outside, check=False)
    return None


async def _live(plan: _Plan, layout: _Layout, /) -> _Live:
    began = anyio.current_time()
    label = layout.name.lower()
    socket, path = plan.work / f"{label}.sock", plan.work / f"{label}.log"
    session = ("new-session", "-s", "preview", "-x", str(plan.size[0]), "-y", str(plan.size[1]), "-c", str(plan.work), "-e", f"CLAUDE_CODE_NO_FLICKER={layout.value}", "-e", "COLORTERM=truecolor")
    process = await anyio.open_process(
        ("tmux", "-u", "-S", socket.name, "-f", os.devnull, "-C", *_TMUX, *session, "--", "claude", *plan.flags(path)), cwd=plan.work, env=plan.environment, stderr=subprocess.DEVNULL
    )
    if process.stdin is None or process.stdout is None:
        raise ValueError(process)
    log = _Log(path, (plan.name, "preview-recorder"))
    terminal, states, pid, watched, ended = _Tmux(process.stdin, process.stdout, log), list[_State](), 0, None, _EXIT
    async with anyio.create_task_group() as group:
        group.start_soon(terminal.read)
        try:
            client, pid = await terminal.client()
            await terminal.run(f"set-hook -g client-detached \"if -F '#{{==:#{{hook_client}},{client}}}' kill-server\"")
            watched = await _watched(socket, f"preview {plan.name} {label}")
            for step in (None, *plan.steps):
                mark = anyio.current_time()
                await terminal.act(step)
                settled = await terminal.settle(began if step is None else mark, plan.limit, _STARTED if step is None else re.compile(step.value) if isinstance(step, _Wait) else None)
                rows = await terminal.screen()
                states.append(_State(step.label if step else "start", anyio.current_time() - mark, settled, rows, len(log.entries)))
        finally:
            with anyio.CancelScope(shield=True):
                ending = anyio.current_time()
                await terminal.run("send-keys -t preview C-c C-c")
                with anyio.move_on_after(_EXIT):
                    await terminal.exited.wait()
                ended = anyio.current_time() - ending
                await anyio.run_process(("tmux", "-S", socket.name, "kill-server"), cwd=plan.work, check=False, stderr=subprocess.DEVNULL)
                if pid:
                    _signalled(pid, signal.SIGTERM)
    await process.aclose()
    entries = await log.read(final=True)
    exited = _State("exit", ended, _Settled.SETTLED if ended < _EXIT else _Settled.KILLED, (), len(entries))
    return _Live(layout, anyio.current_time() - began, (*states, exited), entries, _records(entries), watched)


async def _remote(plan: _Plan, validation: _Validation, props: dict[str, Any], panes: tuple[str, ...], heard: frozenset[str] | None, /) -> _Drawn:
    began, path = anyio.current_time(), plan.work / "session.log"
    command = ("claude", "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose", "--no-session-persistence", *plan.flags(path))
    process = await anyio.open_process(command, cwd=plan.work, env=plan.environment, stderr=subprocess.DEVNULL)
    if process.stdin is None or process.stdout is None:
        raise ValueError(process)
    raised, session, attached, answers = plan.raised, _Headless(process.stdin, process.stdout), list[tuple[str, dict[str, Any]]](), list[tuple[str, dict[str, Any], dict[str, Any]]]()
    async with anyio.create_task_group() as group:
        group.start_soon(session.read)
        try:
            viewport = {"columns": plan.size[0], "rows": plan.size[1], "isFullscreen": True}
            attached += [(surface, await session.ask({"subtype": "ui_attach", "surface": surface, "client_id": f"preview-{surface}", "viewport": viewport})) for surface in _REMOTE]
            await session.quiet()
            for text in (step.value if isinstance(step, _Type) else f"call:{step.value}" for step in plan.steps if isinstance(step, _Type | _Call)):
                await session.say(text)
                await session.quiet()
            panes = tuple(dict.fromkeys((*panes, *(pane["id"] for push in session.pushes if push.get("subtype") == "ui_panes" for pane in push.get("panes", ()) if pane.get("plugin") == plan.name))))
            asked = (
                (site, {"surface": surface, "client_id": f"preview-{surface}", "component": component, "instance_id": request or component, "props": props.get(site, {})})
                for component, request in validation.targets(panes, raised, heard)
                for site in (_label(component, request),)
                for surface in _REMOTE
                if surface in raised.get(component, _SURFACES)
            )
            answers += [(site, request, await session.ask({"subtype": "ui_render", **request})) for site, request in asked]
        finally:
            with anyio.CancelScope(shield=True):
                await process.stdin.aclose()
                with anyio.move_on_after(_ANSWER):
                    await process.wait()
                if process.returncode is None:
                    process.kill()
    entries = await _Log(path, (plan.name,)).read(final=True)
    pushed = dict.fromkeys(fact for push in session.pushes if (fact := _pushed(push, plan.name)))
    refused = (_Fact(f"refused attach {surface}", _cut(_value(answer.get("error"))), fault=True) for surface, answer in attached if answer.get("subtype") != "success")
    trees = tuple((site, request["surface"], answer["response"]["tree"]) for site, request, answer in answers if answer.get("subtype") == "success")
    head = f"{' '.join(_REMOTE)}, {len(session.pushes)} pushes, {anyio.current_time() - began:.1f} s"
    typed = () if raised else (_Fact("types", "none in the bundle, every remote surface asked, refusals not counted"),)
    drawn = filter(None, starmap(partial(_drawing, raised), answers))
    return _Drawn(trees, _Fact("session", head, parts=(*typed, *pushed, *refused, *drawn, *_engine(entries, plan.name))), panes)


async def _mounted(plan: _Plan, sites: tuple[tuple[str, str | None], ...], props: dict[str, Any], /) -> _Drawn:
    began, raised = anyio.current_time(), plan.raised
    table = tuple((component, request, raised.get(component, _SURFACES), props.get(_label(component, request), {})) for component, request in sites)
    if not table:
        return _Drawn((), _Fact("mount", "no render sites"))
    kit = plan.work / "kit"
    shutil.copytree(plan.copy, kit, ignore=shutil.ignore_patterns("*.test.ts", "*.test.tsx"))
    (kit / "preview.test.ts").write_text(
        "import { test } from 'claude-code/testing'\n"
        f"const NAME = {msgspec.json.encode(plan.name).decode()}, SITES = {msgspec.json.encode(table).decode()}\n"
        "const mark = (...fields) => console.log('@@' + JSON.stringify(fields))\n"
        "const keys = node => node && typeof node === 'object' ? [...(node.type === 'Button' && node.props?.key ? [node.props.key] : []), ...(node.children ?? []).flatMap(keys)] : []\n"
        "test('preview', async $ => {\n"
        "  for (const [component, requestId, surfaces, props] of SITES) for (const surface of surfaces) {\n"
        "    const label = [component, requestId].filter(Boolean).join(' ')\n"
        "    let ui\n"
        "    try { ui = await $.ui.mount({ plugin: NAME, surface, component, requestId: requestId ?? undefined, props }) }\n"
        "    catch (error) { mark('refused', surface, label, '', String(error)); continue }\n"
        "    const tree = await ui.drawn()\n"
        "    mark('tree', surface, label, '', tree)\n"
        "    for (const key of keys(tree)) {\n"
        "      try { mark('press', surface, label, key, (await ui.press({ key })) ?? null) }\n"
        "      catch (error) { mark('unpressed', surface, label, key, String(error)) }\n"
        "    }\n"
        "    await ui.unmount()\n"
        "  }\n"
        "})\n",
        encoding="utf-8",
    )
    ran = await anyio.run_process(("claude", "plugin", "test", str(kit)), check=False, stderr=subprocess.STDOUT)
    catch(shutil.rmtree, exception=FileNotFoundError)(kit)
    output = ran.stdout.decode(errors="replace").splitlines()
    marked = tuple(msgspec.json.decode(line.removeprefix("@@"), type=tuple[str, str, str, str, Any]) for line in output if line.startswith("@@["))
    reported = dict.fromkeys(line.strip() for block in "\n".join(output).split("the engine reported:")[1:] for line in takewhile(str.strip, block.split("\n")[1:]))
    acts = sorted(
        (
            (kind != "press", f"{'fault press' if kind == 'unpressed' else kind} {label}{f' {key}' if key else ''}", _cut(_answered(value, "ran") if kind == "press" else _value(value)), surface)
            for kind, surface, label, key, value in marked
            if kind != "tree"
        ),
        key=lambda act: act[:3],
    )
    trees = tuple((label, surface, value) for kind, surface, label, _, value in marked if kind == "tree")
    head = f"{len(trees)} trees of {sum(len(surfaces) for _, _, surfaces, _ in table)} mounts, kit {'passed' if ran.returncode == 0 else 'failed'}, {anyio.current_time() - began:.1f} s"
    parts = (
        *(_Fact(f"{act} {' '.join(surface for *_, surface in group)}", text, fault=fault) for (fault, act, text), group in groupby(acts, key=lambda act: act[:3])),
        *(_Fact("fault" if _BROKE.search(line) else "kit", _cut(_MEASURES.sub("", line)), fault=_BROKE.search(line) is not None) for line in reported if plan.name in line),
        *(_Fact("kit", _cut(line)) for line in tuple(line for line in output if line.strip())[:12] if ran.returncode and not marked),
    )
    return _Drawn(trees, _Fact("mount", head, parts=parts))


async def _interrupted(scope: anyio.CancelScope, /) -> None:
    with anyio.open_signal_receiver(signal.SIGINT, signal.SIGTERM) as signals:
        async for _ in signals:
            scope.cancel()


async def _staged(plan: _Plan, validation: _Validation, layouts: tuple[_Layout, ...], source: Path, bodies: list[bytes], /) -> tuple[_Fact, ...] | None:
    shutil.copytree(source, plan.copy)
    shutil.copytree(_RECORDER, plan.work / "recorder")
    (plan.work / "settings.json").write_bytes(msgspec.json.encode({**_SETTINGS, "showTurnDuration": any(component == "TurnDuration" for component, _ in validation.sites(plan.raised))}))
    async with anyio.create_task_group() as group:
        group.start_soon(_interrupted, group.cancel_scope)
        live = tuple([await _live(plan, layout) for layout in layouts])
        records = tuple(record for run in live for _, record in run.records)
        props = {record.site: record.args["props"] for record in records if record.event == "ui.render" and isinstance(record.args.get("props"), dict)}
        heard = frozenset(str(record.args.get("component")) for record in records if record.event == "ui.render" and any(link.plugin == plan.name for link in record.links)) if live else None
        remote = await _remote(plan, validation, props, tuple(dict.fromkeys(record.args["id"] for record in records if record.event == "ui.open" and record.origin == plan.name)), heard)
        mounted = await _mounted(plan, validation.targets(remote.panes, plan.raised, heard), props)
        group.cancel_scope.cancel()
        trees = _trees(live, (("mount", mounted), ("session", remote)), plan.name)
        return (*(_run(run, plan.name) for run in live), mounted.fact, remote.fact, *((_Fact("trees", parts=trees),) if trees else ()), _model(bodies, live, plan.name))
    return None


def preview(
    bundle: ExistingDirectory,
    /,
    *,
    step: Annotated[tuple[_Steps, ...], Parameter(converter=_steps, metavar="KIND:VALUE", negative="")] = (),
    layout: tuple[_Layout, ...] = (_Layout.FULLSCREEN, _Layout.MAIN),
    size: Annotated[tuple[int, int], Parameter(validator=Number(gt=0), negative="")] = (120, 40),
    limit: Annotated[float, Parameter(validator=Number(gt=_QUIET))] = 5.0,
) -> int:
    """Validate a built mod, run it in a terminal per layout beside a recorder, draw its sites in a headless session on the remote surfaces, mount them on each surface that raises them, and report each stage.

    Each terminal run opens a read-only pane in Zellij session agents when zellij is installed.

    Args:
        bundle: Built plugin folder holding .claude-plugin/plugin.json, as nx build writes it.
        step: key:KEYS as tmux names them (key:C-x Tab), type:TEXT typed then Enter, call:TOOL JSON typed as a prompt the model answers with that one tool call, wait:REGEX over debug-log lines, or resize:COLUMNS ROWS; each step settles and adds a state.
        layout: Terminal layouts run, each a session of its own; --empty-layout runs none.
        size: Terminal columns and rows.
        limit: Seconds a step waits for activity and then quiet.

    Returns:
        1 on a validation error or a fault, 130 on an interrupt, else 0.
    """
    began = time.monotonic()
    source = Path(bundle).resolve()
    name = msgspec.json.decode((source / ".claude-plugin" / "plugin.json").read_bytes(), type=_Manifest).name
    validation = msgspec.json.decode(subprocess.run(("claude", "plugin", "validate", "--json", str(source)), capture_output=True, check=False).stdout, type=_Validation)
    errors = sum(fact.fault for fact in validation.findings)
    validated = _Fact(
        "validate", f"{'passed' if validation.success else 'failed'}, {errors} errors, {len(validation.findings) - errors} warnings, {time.monotonic() - began:.1f} s", parts=validation.findings
    )
    work, swept = _workspace(Path(tempfile.gettempdir()).resolve() / "mod-preview", name)
    bodies: list[bytes] = []
    server = ThreadingHTTPServer(("127.0.0.1", 0), partial(_Endpoint, bodies))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    plan = _Plan(work, name, step, size, limit, f"http://127.0.0.1:{server.server_address[1]}")
    body = anyio.run(_staged, plan, validation, layout, source, bodies) if validation.success else ()
    server.shutdown()
    _purged(work, work)
    report = (*swept, validated, *(body or ()))
    faults = len(frozenset[tuple[str, str]]().union(*(fact.faults for fact in report)))
    verdict, code = ("interrupted", 130) if body is None else (f"failed, {faults} faults", 1) if faults else ("passed", 0)
    sys.stdout.writelines(f"{line}\n" for fact in (_Fact(f"preview {name}", f"{verdict}, {time.monotonic() - began:.1f} s"), *report) for line in fact.lines())
    return code


if __name__ == "__main__":
    app = App(help=__doc__)
    app.default(preview)
    sys.exit(app())
