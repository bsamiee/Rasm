# ruff: noqa: T201 — stdout banner lines are this server's launch contract
"""Loopback return-channel server for html-studio artifacts.

serve  : host one artifact with an injected return-channel meta tag; print banner, run until stopped.
status : print the live server's state and receipt count.
stop   : terminate the live server recorded in the state file.

The served page gains `<meta name="artifact-return" content="/submit?t=<token>">`; an artifact
opened from file:// never sees the meta and degrades to clipboard/download export. Submissions
append as JSON lines to the receipts file the banner names; the agent reads receipts, never stdout.
"""

from __future__ import annotations

import argparse
import contextlib
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
from pathlib import Path
import re
from secrets import token_hex
import signal
import sys
import tempfile
import time
from typing import override
from urllib.parse import parse_qs, urlsplit


# --- [CONSTANTS] ------------------------------------------------------------------------

HOST = "127.0.0.1"
MAX_BODY_BYTES = 256 * 1024
HEAD_RE = re.compile(rb"<head[^>]*>", re.IGNORECASE)


def _state_path() -> Path:
    session = os.environ.get("CLAUDE_CODE_SESSION_ID", "no-session")
    return Path(tempfile.gettempdir()) / f"html-studio-server-{session}.json"


# --- [SERVICES] -------------------------------------------------------------------------


class _Handler(BaseHTTPRequestHandler):
    artifact: bytes = b""
    token: str = ""
    receipts: Path = Path()

    @override
    def log_message(self, format: str, *args: object) -> None:
        del format, args

    def _reject(self, code: int, reason: str) -> None:
        body = json.dumps({"ok": False, "reason": reason}).encode()
        self.send_response(code)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def _host_ok(self) -> bool:
        host = (self.headers.get("Host") or "").split(":")[0]
        return host in {HOST, "localhost"}

    def do_GET(self) -> None:
        """Serve the meta-injected artifact at the root path."""
        if not self._host_ok():
            self._reject(403, "host")
            return
        if urlsplit(self.path).path not in {"/", "/index.html"}:
            self._reject(404, "path")
            return
        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.send_header("Content-Length", str(len(self.artifact)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(self.artifact)

    def do_POST(self) -> None:
        """Accept one token-gated JSON submission and append its receipt."""
        if not self._host_ok():
            self._reject(403, "host")
            return
        split = urlsplit(self.path)
        if split.path != "/submit":
            self._reject(404, "path")
            return
        sent = parse_qs(split.query).get("t", [""])[0] or self.headers.get("X-Artifact-Token", "")
        if sent != self.token:
            self._reject(403, "token")
            return
        length = int(self.headers.get("Content-Length") or 0)
        if not 0 < length <= MAX_BODY_BYTES:
            self._reject(413, "size")
            return
        raw = self.rfile.read(length)
        try:
            payload = json.loads(raw)
        except json.JSONDecodeError:
            self._reject(400, "json")
            return
        receipt = {"received": time.strftime("%Y-%m-%dT%H:%M:%S%z"), "payload": payload}
        with self.receipts.open("a", encoding="utf-8") as sink:
            sink.write(json.dumps(receipt, ensure_ascii=False) + "\n")
        body = json.dumps({"ok": True}).encode()
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _inject_meta(html: bytes, token: str) -> bytes:
    meta = f'<meta name="artifact-return" content="/submit?t={token}">'.encode()
    match = HEAD_RE.search(html)
    if match is None:
        return meta + html
    at = match.end()
    return html[:at] + meta + html[at:]


def serve(args: argparse.Namespace) -> int:
    """Host the artifact on a loopback port; print URL/RECEIPTS/STATE, then serve until stopped.

    Returns:
        Process exit code.
    """
    artifact = Path(args.artifact).resolve()
    if not artifact.is_file():
        print(f"ERROR=missing artifact {artifact}", file=sys.stderr)
        return 2
    state_file = _state_path()
    if state_file.exists():
        print(f"ERROR=state file exists ({state_file}); run stop first", file=sys.stderr)
        return 2
    token = token_hex(16)
    receipts = (
        Path(args.receipts).resolve()
        if args.receipts
        else artifact.with_suffix(".receipts.jsonl")
    )
    receipts.parent.mkdir(parents=True, exist_ok=True)
    handler = type(
        "BoundHandler",
        (_Handler,),
        {
            "artifact": _inject_meta(artifact.read_bytes(), token),
            "token": token,
            "receipts": receipts,
        },
    )
    server = ThreadingHTTPServer((HOST, args.port), handler)
    port = server.server_address[1]
    state = {
        "pid": os.getpid(),
        "port": port,
        "artifact": str(artifact),
        "receipts": str(receipts),
    }
    state_file.write_text(json.dumps(state), encoding="utf-8")
    print(f"URL=http://{HOST}:{port}/")
    print(f"RECEIPTS={receipts}")
    print(f"STATE={state_file}")
    sys.stdout.flush()

    def _shutdown(_sig: int, _frame: object) -> None:
        state_file.unlink(missing_ok=True)
        raise SystemExit(0)

    signal.signal(signal.SIGTERM, _shutdown)
    signal.signal(signal.SIGINT, _shutdown)
    try:
        server.serve_forever()
    finally:
        state_file.unlink(missing_ok=True)
    return 0


def status(_args: argparse.Namespace) -> int:
    """Print the live server's state and receipt count.

    Returns:
        Process exit code.
    """
    state_file = _state_path()
    if not state_file.exists():
        print("STATUS=INACTIVE")
        return 0
    state = json.loads(state_file.read_text(encoding="utf-8"))
    receipts = Path(state["receipts"])
    count = sum(1 for _ in receipts.open(encoding="utf-8")) if receipts.exists() else 0
    print("STATUS=ACTIVE")
    print(f"URL=http://{HOST}:{state['port']}/")
    print(f"ARTIFACT={state['artifact']}")
    print(f"RECEIPTS={state['receipts']}")
    print(f"RECEIPT_COUNT={count}")
    return 0


def stop(_args: argparse.Namespace) -> int:
    """Terminate the live server recorded in the state file.

    Returns:
        Process exit code.
    """
    state_file = _state_path()
    if not state_file.exists():
        print("STATUS=INACTIVE")
        return 0
    state = json.loads(state_file.read_text(encoding="utf-8"))
    with contextlib.suppress(ProcessLookupError):
        os.kill(int(state["pid"]), signal.SIGTERM)
    state_file.unlink(missing_ok=True)
    print("STATUS=STOPPED")
    print(f"RECEIPTS={state['receipts']}")
    return 0


# --- [ENTRY] ----------------------------------------------------------------------------


def main() -> int:
    """Dispatch the serve/status/stop verb.

    Returns:
        Process exit code.
    """
    parser = argparse.ArgumentParser(description=__doc__)
    verbs = parser.add_subparsers(dest="verb", required=True)
    serve_p = verbs.add_parser("serve", help="host one artifact with a return channel")
    serve_p.add_argument("artifact", help="path to the artifact html file")
    serve_p.add_argument("--port", type=int, default=0, help="port; 0 selects an ephemeral port")
    serve_p.add_argument("--receipts", default="", help="receipts jsonl path; defaults beside the artifact")
    serve_p.set_defaults(run=serve)
    verbs.add_parser("status", help="print live server state").set_defaults(run=status)
    verbs.add_parser("stop", help="terminate the live server").set_defaults(run=stop)
    args = parser.parse_args()
    return int(args.run(args))


if __name__ == "__main__":
    raise SystemExit(main())
