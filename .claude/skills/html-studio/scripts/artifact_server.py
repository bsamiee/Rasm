#!/usr/bin/env -S uv run
# /// script
# requires-python = ">=3.13"
# dependencies = [
#   "anyio", "cyclopts", "expression", "httpx", "msgspec", "psutil", "stamina",
#   "structlog", "watchfiles", "xxhash",
#   "beartype @ git+https://github.com/beartype/beartype.git@f370a0b1733413681e7a72bf36fbe839e60b3c85",
# ]
# ///
# ruff: noqa: T201, D101, D102, D103, D107
"""Loopback return-channel runtime for html-studio artifacts.

Verbs: serve | status | stop | receipts | self-test. The served page gains two head metas —
`artifact-return` (the submit path) and `artifact-token` (the bearer the page sends back as
`X-Artifact-Token`); a page opened from file:// sees neither and degrades to export-only.
Receipts and lifecycle events append as one tagged JSONL stream; `receipts --last 1` is the
agent's canonical post-review read.
"""

# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------

from collections.abc import Iterator, Mapping
from datetime import datetime, UTC
from enum import IntEnum, StrEnum
from functools import partial
import hmac
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import os
from pathlib import Path
import re
from secrets import token_hex
from signal import SIGINT, SIGTERM
import sys
import tempfile
import threading
from typing import Annotated, override
from urllib.parse import urlsplit
import webbrowser

import anyio
from anyio import Path as APath
import anyio.to_thread
from beartype import beartype
from cyclopts import App, Parameter
from expression import Error, Ok, Result
import httpx
import msgspec
import psutil
import stamina
import structlog
import watchfiles
from xxhash import xxh3_128_hexdigest


# --- [TYPES] ---------------------------------------------------------------------------------


class FaultKind(StrEnum):
    BAD_ARTIFACT = "bad-artifact"
    BAD_ENVELOPE = "bad-envelope"
    BAD_HOST = "bad-host"
    BAD_JSON = "bad-json"
    BAD_LENGTH = "bad-length"
    BAD_ORIGIN = "bad-origin"
    BAD_TOKEN = "bad-token"
    BAD_TYPE = "bad-type"
    NOT_FOUND = "not-found"
    OVERSIZE = "oversize"
    PORT_BUSY = "port-busy"
    RECEIPT_IO = "receipt-io"
    SELF_TEST = "self-test"
    STATE_BUSY = "state-busy"
    STATE_UNREADABLE = "state-unreadable"
    STOP_TIMEOUT = "stop-timeout"


class EventKind(StrEnum):
    STARTED = "server.started"
    CHANGED = "artifact.changed"
    REJECTED = "submission.rejected"
    TTL_EXPIRED = "server.ttl-expired"
    STOPPED = "server.stopped"


class Exit(IntEnum):
    OK = 0
    USAGE = 2
    STATE = 3
    IO = 4
    NET = 5
    CONTRACT = 6


class OutputMode(StrEnum):
    BANNER = "banner"
    JSON = "json"


type Session = Annotated[str, Parameter(env_var="CLAUDE_CODE_SESSION_ID")]

# --- [CONSTANTS] -----------------------------------------------------------------------------

HOST = "127.0.0.1"
LOOPBACK = frozenset({"127.0.0.1", "localhost", "::1", "[::1]"})
MAX_BODY = 256 * 1024
HEAD_RE = re.compile(rb"<head[^>]*>", re.IGNORECASE)
TOKEN_META_RE = re.compile(r'name="artifact-token" content="([0-9a-f]{32})"')
HTTP_STATUS: Mapping[FaultKind, int] = {
    FaultKind.BAD_HOST: 403,
    FaultKind.BAD_ORIGIN: 403,
    FaultKind.BAD_TOKEN: 403,
    FaultKind.NOT_FOUND: 404,
    FaultKind.BAD_TYPE: 415,
    FaultKind.BAD_LENGTH: 411,
    FaultKind.OVERSIZE: 413,
    FaultKind.BAD_JSON: 400,
    FaultKind.BAD_ENVELOPE: 422,
    FaultKind.RECEIPT_IO: 500,
    FaultKind.BAD_ARTIFACT: 500,
}
EXIT_CODE: Mapping[FaultKind, Exit] = {
    FaultKind.BAD_ARTIFACT: Exit.IO,
    FaultKind.RECEIPT_IO: Exit.IO,
    FaultKind.PORT_BUSY: Exit.NET,
    FaultKind.SELF_TEST: Exit.CONTRACT,
    FaultKind.STATE_BUSY: Exit.STATE,
    FaultKind.STATE_UNREADABLE: Exit.STATE,
    FaultKind.STOP_TIMEOUT: Exit.STATE,
}

# --- [MODELS] --------------------------------------------------------------------------------


class Envelope(msgspec.Struct, frozen=True, forbid_unknown_fields=True):
    kind: str
    artifact: str
    version: Annotated[int, msgspec.Meta(ge=1)]
    data: msgspec.Raw


class ReceiptRow(msgspec.Struct, frozen=True, tag="receipt", tag_field="row"):
    id: str
    received: str
    kind: str
    artifact: str
    payload: msgspec.Raw


class EventRow(msgspec.Struct, frozen=True, tag="event", tag_field="row"):
    id: str
    received: str
    kind: EventKind
    detail: str


class ServerState(msgspec.Struct, frozen=True):
    version: int
    pid: int
    create_time: float
    port: int
    artifact: str
    artifact_digest: str
    receipts: str
    started_at: str
    token_digest: str


class Reply(msgspec.Struct, frozen=True, omit_defaults=True):
    ok: bool
    id: str = ""
    fault: str = ""


class Fault(msgspec.Struct, frozen=True):
    kind: FaultKind
    detail: str = ""


class Output(msgspec.Struct, frozen=True, omit_defaults=True):
    status: str
    url: str = ""
    artifact: str = ""
    receipts: str = ""
    state: str = ""
    receipt_count: int = 0
    detail: str = ""


# --- [SERVICES] ------------------------------------------------------------------------------

ENC = msgspec.json.Encoder()
DEC_ENVELOPE = msgspec.json.Decoder(Envelope)
DEC_ROW = msgspec.json.Decoder(ReceiptRow | EventRow)
DEC_STATE = msgspec.json.Decoder(ServerState)
structlog.configure(
    processors=[structlog.processors.add_log_level, structlog.processors.TimeStamper(fmt="iso"), structlog.processors.JSONRenderer()],
    logger_factory=structlog.PrintLoggerFactory(sys.stderr),
)
LOG = structlog.get_logger()


class Sink:
    """Single locked append path for the tagged receipt/event JSONL stream."""

    __slots__ = ("_lock", "path")

    def __init__(self, path: Path) -> None:
        self.path = path
        self._lock = threading.Lock()

    def append(self, row: ReceiptRow | EventRow) -> None:
        with self._lock, self.path.open("ab") as sink:
            sink.write(ENC.encode(row) + b"\n")
            sink.flush()

    def rows(self) -> Iterator[ReceiptRow | EventRow]:
        if not self.path.is_file():
            return iter(())
        return (DEC_ROW.decode(line) for line in self.path.read_bytes().splitlines() if line.strip())

    def event(self, kind: EventKind, detail: str) -> None:
        received = _utc()
        self.append(EventRow(id=xxh3_128_hexdigest(f"{kind}{received}{detail}"), received=received, kind=kind, detail=detail))


class Runtime(msgspec.Struct, frozen=True):
    artifact: str
    token: str
    receipts: str

    @property
    def sink(self) -> Sink:
        return Sink(Path(self.receipts))


class Handler(BaseHTTPRequestHandler):
    runtime: Runtime

    @override
    def log_message(self, format: str, *args: object) -> None:
        del format, args

    def _respond(self, code: int, body: bytes, ctype: str) -> None:
        self.send_response(code)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(body)

    def _fault(self, fault: Fault) -> None:
        LOG.warning("request.rejected", fault=fault.kind, detail=fault.detail)
        self._respond(HTTP_STATUS.get(fault.kind, 400), ENC.encode(Reply(ok=False, fault=fault.kind)), "application/json")

    def _token_ok(self) -> bool:
        return hmac.compare_digest(self.headers.get("X-Artifact-Token", ""), self.runtime.token)

    def _gate(self) -> Fault | None:
        host = (self.headers.get("Host") or "").rsplit(":", 1)[0]
        origin = self.headers.get("Origin", "")
        checks = (
            Fault(FaultKind.BAD_HOST, host) if host not in LOOPBACK else None,
            Fault(FaultKind.BAD_ORIGIN, origin) if origin and urlsplit(origin).hostname not in LOOPBACK else None,
        )
        return next((fault for fault in checks if fault), None)

    def do_GET(self) -> None:
        route = urlsplit(self.path).path
        if fault := self._gate():
            return self._fault(fault)
        if route in {"/", "/index.html"}:
            page = fresh_page(Path(self.runtime.artifact), self.runtime.token)
            return self._respond(200, page.ok, "text/html; charset=utf-8") if page.is_ok() else self._fault(page.error)
        if route == "/receipts":
            if not self._token_ok():
                return self._fault(Fault(FaultKind.BAD_TOKEN))
            body = Path(self.runtime.receipts).read_bytes() if Path(self.runtime.receipts).is_file() else b""
            return self._respond(200, body, "application/x-ndjson")
        return self._fault(Fault(FaultKind.NOT_FOUND, route))

    def do_POST(self) -> None:
        sink, length = self.runtime.sink, self.headers.get("Content-Length", "")
        checks = (
            self._gate(),
            Fault(FaultKind.NOT_FOUND, self.path) if urlsplit(self.path).path != "/submit" else None,
            Fault(FaultKind.BAD_TOKEN) if not self._token_ok() else None,
            Fault(FaultKind.BAD_TYPE) if not (self.headers.get("Content-Type") or "").startswith("application/json") else None,
            Fault(FaultKind.BAD_LENGTH, length) if not length.isdigit() else None,
            Fault(FaultKind.OVERSIZE, length) if length.isdigit() and not 0 < int(length) <= MAX_BODY else None,
        )
        if fault := next((fault for fault in checks if fault), None):
            sink.event(EventKind.REJECTED, fault.kind)
            return self._fault(fault)
        raw = self.rfile.read(int(length))
        try:
            envelope = DEC_ENVELOPE.decode(raw)
        except msgspec.DecodeError as exc:
            kind = FaultKind.BAD_JSON if isinstance(exc, msgspec.DecodeError) and "JSON" in str(exc) else FaultKind.BAD_ENVELOPE
            sink.event(EventKind.REJECTED, kind)
            return self._fault(Fault(kind, str(exc)[:120]))
        row = ReceiptRow(id=xxh3_128_hexdigest(raw), received=_utc(), kind=envelope.kind, artifact=envelope.artifact, payload=msgspec.Raw(raw))
        try:
            sink.append(row)
        except OSError as exc:
            return self._fault(Fault(FaultKind.RECEIPT_IO, type(exc).__name__))
        LOG.info("submission.received", id=row.id, kind=row.kind)
        return self._respond(200, ENC.encode(Reply(ok=True, id=row.id)), "application/json")


# --- [OPERATIONS] ----------------------------------------------------------------------------


def _utc() -> str:
    return datetime.now(UTC).isoformat(timespec="seconds")


def _state_path(session: str) -> Path:
    return Path(tempfile.gettempdir()) / f"html-studio-server-{session}.json"


@beartype
def admit_artifact(path: Path) -> Result[Path, Fault]:
    resolved = path.resolve()
    admitted = resolved.suffix == ".html" and resolved.is_file()
    return Ok(resolved) if admitted else Error(Fault(FaultKind.BAD_ARTIFACT, str(resolved)))


@beartype
def fresh_page(artifact: Path, token: str) -> Result[bytes, Fault]:
    try:
        raw = artifact.read_bytes()
    except OSError as exc:
        return Error(Fault(FaultKind.BAD_ARTIFACT, type(exc).__name__))
    match = HEAD_RE.search(raw)
    if match is None:
        return Error(Fault(FaultKind.BAD_ARTIFACT, "no <head>"))
    meta = f'<meta name="artifact-return" content="/submit"><meta name="artifact-token" content="{token}">'.encode()
    return Ok(raw[: match.end()] + meta + raw[match.end() :])


def read_state(state_file: Path) -> Result[ServerState | None, Fault]:
    if not state_file.is_file():
        return Ok(None)
    try:
        return Ok(DEC_STATE.decode(state_file.read_bytes()))
    except (OSError, msgspec.DecodeError) as exc:
        return Error(Fault(FaultKind.STATE_UNREADABLE, f"{state_file}: {type(exc).__name__}"))


def live_process(state: ServerState) -> psutil.Process | None:
    try:
        process = psutil.Process(state.pid)
        return process if process.is_running() and abs(process.create_time() - state.create_time) < 1.0 else None
    except psutil.NoSuchProcess:
        return None


def write_state(state_file: Path, state: ServerState) -> None:
    scratch = state_file.with_suffix(".tmp")
    scratch.write_bytes(ENC.encode(state))
    scratch.chmod(0o600)
    scratch.replace(state_file)


def emit(output: Output, mode: OutputMode) -> None:
    if mode is OutputMode.JSON:
        print(ENC.encode(output).decode())
    else:
        pairs = (
            ("URL", output.url),
            ("ARTIFACT", output.artifact),
            ("RECEIPTS", output.receipts),
            ("STATE", output.state),
            ("RECEIPT_COUNT", str(output.receipt_count or "")),
            ("DETAIL", output.detail),
        )
        print("\n".join((f"STATUS={output.status}", *(f"{key}={value}" for key, value in pairs if value))))
    sys.stdout.flush()


def fail(fault: Fault, mode: OutputMode) -> int:
    emit(Output(status="ERROR", detail=f"{fault.kind}: {fault.detail}".strip(": ")), mode)
    return int(EXIT_CODE.get(fault.kind, Exit.USAGE))


# --- [COMPOSITION] ---------------------------------------------------------------------------


async def supervise(server: ThreadingHTTPServer, runtime: Runtime, ttl: float | None) -> None:
    sink = runtime.sink

    async def signals(scope: anyio.CancelScope) -> None:
        with anyio.open_signal_receiver(SIGTERM, SIGINT) as stream:
            async for _ in stream:
                scope.cancel()
                return

    async def watch() -> None:
        async for _ in watchfiles.awatch(runtime.artifact):
            page = APath(runtime.artifact)
            digest = xxh3_128_hexdigest(await page.read_bytes()) if await page.is_file() else "deleted"
            sink.event(EventKind.CHANGED, digest)
            LOG.info("artifact.changed", digest=digest)

    async def expiry(scope: anyio.CancelScope, seconds: float) -> None:
        await anyio.sleep(seconds)
        sink.event(EventKind.TTL_EXPIRED, f"{seconds}s")
        scope.cancel()

    async with anyio.create_task_group() as tg:
        _ = tg.start_soon(partial(anyio.to_thread.run_sync, server.serve_forever, abandon_on_cancel=True))
        _ = tg.start_soon(signals, tg.cancel_scope)
        _ = tg.start_soon(watch)
        if ttl:
            _ = tg.start_soon(expiry, tg.cancel_scope, ttl)
    server.shutdown()
    server.server_close()
    sink.event(EventKind.STOPPED, "supervisor exit")


def serve(
    artifact: Path,
    *,
    port: int = 0,
    receipts: Path | None = None,
    ttl: float | None = None,
    output: OutputMode = OutputMode.BANNER,
    open_page: Annotated[bool, Parameter(name="--open")] = False,
    session: Session = "no-session",
) -> int:
    """Host one artifact with an injected return channel until stopped, TTL, or signal.

    Returns:
        Process exit code from the closed `Exit` vocabulary.
    """
    admitted = admit_artifact(artifact)
    if admitted.is_error():
        return fail(admitted.error, output)
    resolved = admitted.ok
    state_file = _state_path(session)
    state = read_state(state_file)
    if state.is_error():
        return fail(state.error, output)
    if state.ok is not None and live_process(state.ok):
        return fail(Fault(FaultKind.STATE_BUSY, f"pid {state.ok.pid} live at port {state.ok.port}"), output)
    if state.ok is not None:
        state_file.unlink(missing_ok=True)
        LOG.info("state.stale-healed", pid=state.ok.pid)
    token = token_hex(16)
    receipt_path = (receipts or resolved.with_suffix(".receipts.jsonl")).resolve()
    receipt_path.parent.mkdir(parents=True, exist_ok=True)
    runtime = Runtime(artifact=str(resolved), token=token, receipts=str(receipt_path))
    Handler.runtime = runtime
    try:
        server = ThreadingHTTPServer((HOST, port), Handler)
    except OSError as exc:
        return fail(Fault(FaultKind.PORT_BUSY, str(exc)), output)
    me = psutil.Process()
    bound = int(server.server_address[1])
    write_state(
        state_file,
        ServerState(
            version=1,
            pid=me.pid,
            create_time=me.create_time(),
            port=bound,
            artifact=str(resolved),
            artifact_digest=xxh3_128_hexdigest(resolved.read_bytes()),
            receipts=str(receipt_path),
            started_at=_utc(),
            token_digest=xxh3_128_hexdigest(token),
        ),
    )
    runtime.sink.event(EventKind.STARTED, f"port {bound}")
    emit(Output(status="ACTIVE", url=f"http://{HOST}:{bound}/", artifact=str(resolved), receipts=str(receipt_path), state=str(state_file)), output)
    if open_page:
        webbrowser.open(f"http://{HOST}:{bound}/")
    try:
        anyio.run(supervise, server, runtime, ttl)
    finally:
        state_file.unlink(missing_ok=True)
    return int(Exit.OK)


def status(*, output: OutputMode = OutputMode.BANNER, session: Session = "no-session") -> int:
    """Report liveness-proven server state and receipt count.

    Returns:
        Process exit code from the closed `Exit` vocabulary.
    """
    state_file = _state_path(session)
    state = read_state(state_file)
    if state.is_error():
        return fail(state.error, output)
    if state.ok is None:
        emit(Output(status="INACTIVE"), output)
        return int(Exit.OK)
    if live_process(state.ok) is None:
        state_file.unlink(missing_ok=True)
        emit(Output(status="STALE", state=str(state_file), detail=f"pid {state.ok.pid} gone; state retired"), output)
        return int(Exit.OK)
    count = sum(1 for row in Sink(Path(state.ok.receipts)).rows() if isinstance(row, ReceiptRow))
    emit(
        Output(
            status="ACTIVE",
            url=f"http://{HOST}:{state.ok.port}/",
            artifact=state.ok.artifact,
            receipts=state.ok.receipts,
            state=str(state_file),
            receipt_count=count,
        ),
        output,
    )
    return int(Exit.OK)


def stop(*, grace: float = 2.0, output: OutputMode = OutputMode.BANNER, session: Session = "no-session") -> int:
    """Terminate the liveness-proven server; never signal an unmatched pid.

    Returns:
        Process exit code from the closed `Exit` vocabulary.
    """
    state_file = _state_path(session)
    state = read_state(state_file)
    if state.is_error():
        return fail(state.error, output)
    if state.ok is None:
        emit(Output(status="INACTIVE"), output)
        return int(Exit.OK)
    process = live_process(state.ok)
    if process is None:
        state_file.unlink(missing_ok=True)
        emit(Output(status="STALE", detail="state retired without signal"), output)
        return int(Exit.OK)
    process.terminate()
    try:
        process.wait(timeout=grace)
    except psutil.TimeoutExpired:
        return fail(Fault(FaultKind.STOP_TIMEOUT, f"pid {process.pid} alive after {grace}s"), output)
    state_file.unlink(missing_ok=True)
    emit(Output(status="STOPPED", receipts=state.ok.receipts), output)
    return int(Exit.OK)


def receipts(path: Path, *, last: int = 1, kind: str = "", json: bool = False) -> int:
    """Print the newest receipt rows from a receipts stream; the agent's canonical read.

    Returns:
        Process exit code from the closed `Exit` vocabulary.
    """
    rows = [row for row in Sink(path).rows() if isinstance(row, ReceiptRow) and (not kind or row.kind == kind)]
    for row in rows[-last:] if last else rows:
        print(ENC.encode(row).decode() if json else f"{row.received} {row.kind} {row.id} {bytes(row.payload).decode()}")
    return int(Exit.OK)


def self_test() -> int:
    """Prove serve -> GET -> POST -> reject -> receipt readback -> stop on a temp artifact.

    Returns:
        Process exit code from the closed `Exit` vocabulary.
    """

    async def circuit() -> Fault | None:
        scratch = APath(tempfile.mkdtemp(prefix="html-studio-selftest-")) / "probe.html"
        await scratch.write_text("<!doctype html><html><head><title>probe</title></head><body>probe</body></html>")
        env = dict(os.environ) | {"CLAUDE_CODE_SESSION_ID": f"selftest-{os.getpid()}"}
        process = await anyio.open_process([sys.executable, str(APath(__file__)), "serve", str(scratch), "--output", "json"], env=env)
        try:
            stdout = process.stdout
            if stdout is None:
                return Fault(FaultKind.SELF_TEST, "subprocess stdout not piped")
            banner = msgspec.json.decode(await stdout.receive(), type=Output)

            async def fetch(client: httpx.AsyncClient) -> httpx.Response:
                async for attempt in stamina.retry_context(on=httpx.TransportError, attempts=8, timeout=10.0):
                    with attempt:
                        return await client.get(banner.url)
                raise httpx.TransportError("server never became reachable")

            async with httpx.AsyncClient(timeout=httpx.Timeout(5.0)) as client:
                served = await fetch(client)
                token_match = TOKEN_META_RE.search(served.text)
                if token_match is None:
                    return Fault(FaultKind.SELF_TEST, "token meta missing from served page")
                headers = {"X-Artifact-Token": token_match.group(1), "Content-Type": "application/json"}
                envelope = {"kind": "self-test", "artifact": "probe", "version": 1, "data": {"ok": True}}
                posted = await client.post(f"{banner.url}submit", content=ENC.encode(envelope), headers=headers)
                reply = msgspec.json.decode(posted.content, type=Reply)
                if posted.status_code != 200 or not reply.ok:
                    return Fault(FaultKind.SELF_TEST, f"submit failed: {posted.status_code}")
                bad = await client.post(f"{banner.url}submit", content=b"{}", headers=headers | {"X-Artifact-Token": "0" * 32})
                if bad.status_code != 403:
                    return Fault(FaultKind.SELF_TEST, f"bad token accepted: {bad.status_code}")
                stream = await client.get(f"{banner.url}receipts", headers=headers)
                rows = [DEC_ROW.decode(line) for line in stream.content.splitlines() if line.strip()]
                if not any(isinstance(row, ReceiptRow) and row.id == reply.id for row in rows):
                    return Fault(FaultKind.SELF_TEST, "receipt row missing from readback")
        finally:
            process.terminate()
            await process.wait()
        return None

    fault = anyio.run(circuit)
    if fault is not None:
        return fail(fault, OutputMode.BANNER)
    print("STATUS=SELF_TEST_OK")
    return int(Exit.OK)


# --- [ENTRY] ---------------------------------------------------------------------------------

app = App(name="artifact-server", result_action="return_int_as_exit_code_else_zero")
app.command(serve)
app.command(status)
app.command(stop)
app.command(receipts)
app.command(self_test, name="self-test")

if __name__ == "__main__":
    sys.exit(app())
