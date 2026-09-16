"""Recording call stubs, the virtual-clock backend, fixture file writers, and the NDJSON oracle."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Mapping
from pathlib import Path

import msgspec
import msgspec.json
import pytest
import trio.testing

# --- [TYPES] ----------------------------------------------------------------------------

type CallRecord = tuple[str, tuple[object, ...], dict[str, object]]


class Sync[R](msgspec.Struct, frozen=True):
    """Synchronous double, ``(*args, **kwargs) -> value``, an append-only sink is ``Sync(None)``."""

    value: R


class Async[R](msgspec.Struct, frozen=True):
    """Awaited double, ``async (*args, **kwargs) -> value``, for a coroutine the subject awaits."""

    value: R


class Factory[R](msgspec.Struct, frozen=True):
    """Curried double, ``(*bind) -> (*call) -> value``, the inner call records under ``<member>()``."""

    value: R


type Stub[R] = Sync[R] | Async[R] | Factory[R]

# --- [CALL_RECORDING] -------------------------------------------------------------------


def install[R](monkeypatch: pytest.MonkeyPatch, target: object, member: str, stub: Stub[R], calls: list[CallRecord]) -> None:
    """Replace ``target.member`` with a stub that appends ``(member, args, kwargs)`` to ``calls`` at every call."""
    runner: Callable[..., object]
    match stub:
        case Sync(value):

            def sync(*args: object, **kwargs: object) -> R:
                calls.append((member, args, kwargs))
                return value

            runner = sync
        case Async(value):

            async def coroutine(*args: object, **kwargs: object) -> R:  # ruff:ignore[unused-async]
                calls.append((member, args, kwargs))
                return value

            runner = coroutine
        case Factory(value):

            def factory(*args: object, **kwargs: object) -> Callable[..., R]:
                calls.append((member, args, kwargs))

                def call(*call_args: object, **call_kwargs: object) -> R:
                    calls.append((f"{member}()", call_args, call_kwargs))
                    return value

                return call

            runner = factory
    monkeypatch.setattr(target, member, runner)


# --- [VIRTUAL_TIME] ---------------------------------------------------------------------


def autojump_backend() -> tuple[str, dict[str, object]]:
    """Return the ``anyio_backend`` parameter for Trio's autojumping clock, every ``anyio.sleep`` and deadline advances once the loop idles."""
    return ("trio", {"clock": trio.testing.MockClock(autojump_threshold=0)})


# --- [FIXTURE_WRITERS] ------------------------------------------------------------------


def write_fixtures(directory: Path, files: Mapping[str, object], encode: Callable[[object], bytes] = msgspec.json.encode) -> dict[str, Path]:
    """Write each named file under ``directory``, raw bytes as given and any other content through ``encode``, and return the paths by name."""
    paths = {name: directory / name for name in files}
    for name, path in paths.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content if isinstance(content := files[name], bytes) else encode(content))
    return paths


# --- [DECODE_ORACLES] -------------------------------------------------------------------


def decoded_lines[T](decoder: msgspec.json.Decoder[T], raw: bytes | str, count: int) -> list[T]:
    """Decode newline-delimited JSON and assert the exact line count."""
    rows = decoder.decode_lines(raw)
    assert len(rows) == count, f"expected exactly {count} NDJSON line(s), got {len(rows)}: {raw!r}"
    return rows


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Async", "CallRecord", "Factory", "Stub", "Sync", "autojump_backend", "decoded_lines", "install", "write_fixtures"]
