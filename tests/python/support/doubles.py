"""Reusable call stubs, loopback servers, fixture writers, and decode assertions."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Iterable
from contextlib import asynccontextmanager
from pathlib import Path
from types import TracebackType
from typing import Protocol, Self, TYPE_CHECKING

import msgspec
lazy import pytest
lazy import trio.testing

if TYPE_CHECKING:
    from collections.abc import AsyncGenerator, Awaitable, Mapping


# --- [TYPES] ----------------------------------------------------------------------------

type CallRecord = tuple[str, tuple[object, ...], dict[str, object]]
type _Recorder = Callable[[tuple[object, ...], dict[str, object]], None]
type _CallLog = Callable[[str, tuple[object, ...], dict[str, object]], None]
type Variant = bytes | object


class _AsyncServer(Protocol):
    """Awaited server that enters as its own async context manager."""

    async def __aenter__(self) -> Self: ...
    async def __aexit__(self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: TracebackType | None) -> object: ...


# --- [MODELS] ---------------------------------------------------------------------------

# --- [RECORDING_PATCH]


def _no_projection[A](_args: tuple[object, ...]) -> tuple[A, ...]:
    return ()


class Sync[R](msgspec.Struct, frozen=True, gc=False):
    """Synchronous double, ``(*args, **kwargs) -> value``, the append-only sink uses ``Sync(None)``."""

    value: R

    def bind(self, record: _Recorder, log: _CallLog) -> Callable[..., object]:
        """Build the recording runner ``CallSpy.install`` sets on the target."""
        _ = log

        def run_sync(*args: object, **kwargs: object) -> R:
            record(args, kwargs)
            return self.value

        return run_sync


class Async[R](msgspec.Struct, frozen=True, gc=False):
    """Awaited double: ``async (*args, **kwargs) -> value`` for a coroutine the SUT ``await``s."""

    value: R

    def bind(self, record: _Recorder, log: _CallLog) -> Callable[..., object]:
        """Build the recording runner ``CallSpy.install`` sets on the target."""
        _ = log

        async def run_async(*args: object, **kwargs: object) -> R:  # ruff:ignore[unused-async]
            record(args, kwargs)
            return self.value

        return run_async


class Batch[R](msgspec.Struct, frozen=True, gc=False):
    """Batch stub returning a fixed result sequence and recording the input collection."""

    values: tuple[R, ...]

    def bind(self, record: _Recorder, log: _CallLog) -> Callable[..., object]:
        """Build the recording runner ``CallSpy.install`` sets on the target."""
        _ = log

        def run_batch(items: object, **kwargs: object) -> tuple[R, ...]:
            record((items,), kwargs)
            return self.values

        return run_batch


class Factory[R](msgspec.Struct, frozen=True, gc=False):
    """Curried double: ``(bind...) -> (call...) -> value`` recording construction and invocation separately."""

    value: R
    inner_label: str = "<factory>.run"

    def bind(self, record: _Recorder, log: _CallLog) -> Callable[..., object]:
        """Build the recording runner ``CallSpy.install`` sets on the target."""

        def run_factory(*bind_args: object, **bind_kwargs: object) -> Callable[..., R]:
            record(bind_args, bind_kwargs)

            def run_call(*call: object, **call_kwargs: object) -> R:
                log(self.inner_label, call, call_kwargs)
                return self.value

            return run_call

        return run_factory


type StubBehavior[R] = Sync[R] | Async[R] | Batch[R] | Factory[R]


class CallSpy[A](msgspec.Struct, frozen=True, gc=False):
    """Monkeypatch helper recording every call made to the installed stubs."""

    project: Callable[[tuple[object, ...]], Iterable[A]] = _no_projection
    calls: list[CallRecord] = msgspec.field(default_factory=list)
    captured: list[A] = msgspec.field(default_factory=list)

    def install[R](self, monkeypatch: pytest.MonkeyPatch, target: object, member: str, behavior: StubBehavior[R]) -> None:
        """Replace ``target.member`` with a recording stub."""

        def record(args: tuple[object, ...], kwargs: dict[str, object]) -> None:
            self.calls.append((member, args, kwargs))
            self.captured.extend(self.project(args))

        def log(label: str, args: tuple[object, ...], kwargs: dict[str, object]) -> None:
            self.calls.append((label, args, kwargs))

        monkeypatch.setattr(target, member, behavior.bind(record, log))

    def projected[K](self, pick: Callable[[CallRecord], Iterable[K]]) -> list[K]:
        return [item for call in self.calls for item in pick(call)]


# --- [NETWORK_LOOPBACK]


class Loopback(msgspec.Struct, frozen=True, gc=False):
    """Bound loopback host and port with connection-target formatting."""

    host: str
    port: int

    def target(self, scheme: str = "ssh", user: str = "test-user") -> str:
        return f"{scheme}://{user}@{self.host}:{self.port}"


@asynccontextmanager
async def loopback_server[S: _AsyncServer](
    listen: Callable[[], Awaitable[S]], port_of: Callable[[S], int], *, host: str = "127.0.0.1"
) -> AsyncGenerator[Loopback]:
    """Bind a loopback server for the duration of the ``async with``, yielding its ``Loopback``."""
    async with await listen() as server:
        yield Loopback(host=host, port=port_of(server))


# --- [VIRTUAL_TIME]


def autojump_backend(threshold: float = 0.0) -> tuple[str, dict[str, object]]:
    """Return an ``anyio_backend`` parameter using Trio's autojumping virtual clock.

    Every ``anyio.sleep`` and deadline advances instantly once the loop idles past ``threshold``, retry, drain, and timeout tests complete without real-time sleeps, and the asyncssh double skips itself under this backend.
    """
    return ("trio", {"clock": trio.testing.MockClock(autojump_threshold=threshold)})


# --- [FIXTURE_WRITERS]


class VariantWriter[V](msgspec.Struct, frozen=True, gc=False):
    """Table-driven variant writer for raw bytes or encoded objects."""

    directory: Path
    names: "Mapping[V, str]"
    contents: "Mapping[V, Variant]"
    encode: Callable[[object], bytes] = msgspec.json.encode
    absent: frozenset[V] = frozenset()

    def path(self, variant: V) -> Path:
        """Write a variant and return its path, ``absent`` variants are never written."""
        target = self.directory / self.names[variant]
        content = self.contents.get(variant)
        return target if variant in self.absent else self._write(target, content if isinstance(content, bytes) else self.encode(content))

    def write_all(self) -> dict[V, Path]:
        return {variant: self.path(variant) for variant in self.names}

    @staticmethod
    def _write(target: Path, raw: bytes) -> Path:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(raw)
        return target


# --- [DECODE_ORACLES]


class NdjsonOracle[T](msgspec.Struct, frozen=True, gc=False):
    """NDJSON decoder that asserts the exact line count."""

    decoder: msgspec.json.Decoder[T]
    expect_lines: int = 1

    def rows(self, raw: bytes) -> tuple[T, ...]:
        lines = raw.splitlines()
        assert len(lines) == self.expect_lines, f"expected exactly {self.expect_lines} NDJSON line(s), got {len(lines)}: {lines!r}"
        return tuple(self.decoder.decode(line) for line in lines)

    def one(self, raw: bytes) -> T:
        assert self.expect_lines == 1, f"one() decodes a single-write line, expect_lines is {self.expect_lines}, use rows()"
        return self.rows(raw)[0]

    def from_capture(self, cap: pytest.CaptureFixture[bytes] | pytest.CaptureFixture[str]) -> T:
        out = cap.readouterr().out
        return self.one(out if isinstance(out, bytes) else out.encode())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Async",
    "Batch",
    "Factory",
    "Loopback",
    "NdjsonOracle",
    "CallSpy",
    "CallRecord",
    "StubBehavior",
    "Sync",
    "Variant",
    "VariantWriter",
    "autojump_backend",
    "loopback_server",
]
