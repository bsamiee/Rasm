"""Host client of the MCP extension's execute protocol on a session's loopback port."""

from typing import Final

import anyio
from anyio.streams.buffered import BufferedByteReceiveStream
import msgspec

# --- [CONSTANTS] ------------------------------------------------------------------------

LOOPBACK: Final = "127.0.0.1"

# --- [MODELS] ---------------------------------------------------------------------------


class Done(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    """Reply to code that ran, with its `result` dict and the output it printed."""

    result: dict[str, object]
    stdout: str = ""
    stderr: str = ""


class Raised(msgspec.Struct, frozen=True, tag="error", tag_field="status"):
    """Reply to code that raised, with its traceback and the output it printed."""

    message: str
    stdout: str = ""
    stderr: str = ""


# --- [OPERATIONS] -----------------------------------------------------------------------


async def execute(port: int, code: str, *, strict_json: bool) -> Done | Raised:
    """Return the extension's reply to `code` on the loopback port, and `strict_json` rejects a `result` JSON cannot hold."""
    async with await anyio.connect_tcp(LOOPBACK, port) as stream:
        await stream.send(msgspec.json.encode({"type": "execute", "code": code, "strict_json": strict_json}) + b"\0")
        return msgspec.json.decode(await BufferedByteReceiveStream(stream).receive_until(b"\0", 1 << 24), type=Done | Raised)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LOOPBACK", "Done", "Raised", "execute"]
