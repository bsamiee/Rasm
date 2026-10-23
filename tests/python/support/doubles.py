"""Call-recording stubs, Trio autojump clock, fixture writer, and NDJSON line-count check."""

from collections.abc import Callable, Mapping, Sequence
from pathlib import Path

import anyio.lowlevel
import msgspec
import msgspec.json
import pytest
import trio.testing

# --- [TYPES] ----------------------------------------------------------------------------

type CallRecord = tuple[str, tuple[object, ...], dict[str, object]]

# --- [MODELS] ---------------------------------------------------------------------------


class Sync[R](msgspec.Struct, frozen=True):
    """Synchronous double, ``(*args, **kwargs) -> value``."""

    value: R


class Async[R](msgspec.Struct, frozen=True):
    """Awaited double, ``async (*args, **kwargs) -> value``."""

    value: R


class Factory[R](msgspec.Struct, frozen=True):
    """Curried double ``(*bind) -> (*call) -> value`` logging its inner call as ``<member>()``."""

    value: R


type Stub[R] = Sync[R] | Async[R] | Factory[R]

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [RECORDING]


def install(monkeypatch: pytest.MonkeyPatch, target: object, stubs: Mapping[str, Stub[object]]) -> Sequence[CallRecord]:
    """Replace each ``target.member`` with its stub and return shared call log in call order."""
    calls: list[CallRecord] = []

    def runner(member: str, stub: Stub[object]) -> object:
        def sync(*args: object, **kwargs: object) -> object:
            calls.append((member, args, kwargs))
            match stub:
                case Factory(value):
                    return runner(f"{member}()", Sync(value))
                case Sync(value) | Async(value):
                    return value

        async def coroutine(*args: object, **kwargs: object) -> object:
            value = sync(*args, **kwargs)
            await anyio.lowlevel.checkpoint()
            return value

        return coroutine if isinstance(stub, Async) else sync

    for member, stub in stubs.items():
        monkeypatch.setattr(target, member, runner(member, stub))
    return calls


# --- [CLOCKS]


def autojump_backend() -> tuple[str, dict[str, object]]:
    """Return ``anyio_backend`` parameter for Trio autojumping clock."""
    return ("trio", {"clock": trio.testing.MockClock(autojump_threshold=0)})


# --- [FIXTURES]


def write_fixtures(directory: Path, files: Mapping[str, object], encode: Callable[[object], bytes] = msgspec.json.encode) -> dict[str, Path]:
    """Write each file under ``directory``, non-bytes content through ``encode``, and return paths by name."""
    paths = {name: directory / name for name in files}
    for name, path in paths.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content if isinstance(content := files[name], bytes) else encode(content))
    return paths


# --- [DECODING]


def decoded_lines[T](decoder: msgspec.json.Decoder[T], raw: bytes | str, count: int) -> list[T]:
    """Decode newline-delimited JSON and assert ``count`` lines."""
    rows = decoder.decode_lines(raw)
    assert len(rows) == count, f"expected {count} NDJSON lines, got {len(rows)} in {raw!r}"
    return rows


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Async", "CallRecord", "Factory", "Stub", "Sync", "autojump_backend", "decoded_lines", "install", "write_fixtures"]
