"""Recording call stubs, an autojumping virtual clock, fixture file writers, and NDJSON line-count assertions."""

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
    """Curried double, ``(*bind) -> (*call) -> value``, the inner call records under ``<member>()``."""

    value: R


type Stub[R] = Sync[R] | Async[R] | Factory[R]

# --- [CALL_RECORDING] -------------------------------------------------------------------


def install(monkeypatch: pytest.MonkeyPatch, target: object, stubs: Mapping[str, Stub[object]]) -> Sequence[CallRecord]:
    """Replace each ``target.member`` with its stub and return the one log of ``(member, args, kwargs)`` every call appends in call order."""
    calls: list[CallRecord] = []

    def runner(member: str, stub: Stub[object]) -> Callable[..., object]:
        match stub:
            case Sync(value):

                def sync(*args: object, **kwargs: object) -> object:
                    calls.append((member, args, kwargs))
                    return value

                return sync
            case Async(value):

                async def coroutine(*args: object, **kwargs: object) -> object:
                    calls.append((member, args, kwargs))
                    await anyio.lowlevel.checkpoint()
                    return value

                return coroutine
            case Factory(value):

                def factory(*args: object, **kwargs: object) -> Callable[..., object]:
                    calls.append((member, args, kwargs))

                    def call(*call_args: object, **call_kwargs: object) -> object:
                        calls.append((f"{member}()", call_args, call_kwargs))
                        return value

                    return call

                return factory

    for member, stub in stubs.items():
        monkeypatch.setattr(target, member, runner(member, stub))
    return calls


# --- [VIRTUAL_TIME] ---------------------------------------------------------------------


def autojump_backend() -> tuple[str, dict[str, object]]:
    """Return the ``anyio_backend`` parameter for Trio's autojumping clock."""
    return ("trio", {"clock": trio.testing.MockClock(autojump_threshold=0)})


# --- [FIXTURE_WRITERS] ------------------------------------------------------------------


def write_fixtures(directory: Path, files: Mapping[str, object], encode: Callable[[object], bytes] = msgspec.json.encode) -> dict[str, Path]:
    """Write each named file under ``directory``, raw bytes as given and any other content through ``encode``, and return the paths by name."""
    paths = {name: directory / name for name in files}
    for name, path in paths.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content if isinstance(content := files[name], bytes) else encode(content))
    return paths


# --- [DECODE_ASSERTIONS] ----------------------------------------------------------------


def decoded_lines[T](decoder: msgspec.json.Decoder[T], raw: bytes | str, count: int) -> list[T]:
    """Decode newline-delimited JSON and assert the exact line count."""
    rows = decoder.decode_lines(raw)
    assert len(rows) == count, f"expected exactly {count} NDJSON line(s), got {len(rows)}: {raw!r}"
    return rows


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Async", "CallRecord", "Factory", "Stub", "Sync", "autojump_backend", "decoded_lines", "install", "write_fixtures"]
