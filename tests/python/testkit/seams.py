"""Project-agnostic seam doubles, loopback capsules, fixture writers, and decode oracles."""

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

type SeamRecord = tuple[str, tuple[object, ...], dict[str, object]]
type Recorder = Callable[[tuple[object, ...], dict[str, object]], None]
type SeamLog = Callable[[str, tuple[object, ...], dict[str, object]], None]
type Variant = bytes | object


class _AsyncServer(Protocol):
    """Awaited server that enters as its own async context manager."""

    async def __aenter__(self) -> Self: ...
    async def __aexit__(self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: TracebackType | None) -> object: ...


# --- [MODELS] ---------------------------------------------------------------------------

# --- [RECORDING_PATCH]


def _noproject[A](_args: tuple[object, ...]) -> tuple[A, ...]:
    return ()


class Sync[R](msgspec.Struct, frozen=True, gc=False):
    """Synchronous seam: ``(*args, **kwargs) -> value`` (the append-only sink uses ``Sync(None)``)."""

    value: R

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs."""
        _ = log

        def run_sync(*args: object, **kwargs: object) -> R:
            record(args, kwargs)
            return self.value

        return run_sync


class Async[R](msgspec.Struct, frozen=True, gc=False):
    """Awaited seam: ``async (*args, **kwargs) -> value`` for a coroutine the SUT ``await``s."""

    value: R

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs."""
        _ = log

        async def run_async(*args: object, **kwargs: object) -> R:  # ruff:ignore[unused-async]
            record(args, kwargs)
            return self.value

        return run_async


class FanOut[R](msgspec.Struct, frozen=True, gc=False):
    """Batch seam: ``(items, **kwargs) -> values`` recording the ``items`` tuple as the sole positional."""

    values: tuple[R, ...]

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs."""
        _ = log

        def run_fan(items: object, **kwargs: object) -> tuple[R, ...]:
            record((items,), kwargs)
            return self.values

        return run_fan


class Factory[R](msgspec.Struct, frozen=True, gc=False):
    """Curried seam: ``(bind...) -> (call...) -> value`` recording bind-layer then ``inner_label`` call-layer."""

    value: R
    inner_label: str = "<factory>.run"

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs."""

        def run_factory(*bind_args: object, **bind_kwargs: object) -> Callable[..., R]:
            record(bind_args, bind_kwargs)

            def run_call(*call: object, **call_kwargs: object) -> R:
                log(self.inner_label, call, call_kwargs)
                return self.value

            return run_call

        return run_factory


type Shape[R] = Sync[R] | Async[R] | FanOut[R] | Factory[R]


class SeamProbe[A](msgspec.Struct, frozen=True, gc=False):
    """Recording monkeypatch host for canned call-shape seams."""

    project: Callable[[tuple[object, ...]], Iterable[A]] = _noproject
    calls: list[SeamRecord] = msgspec.field(default_factory=list)
    captured: list[A] = msgspec.field(default_factory=list)

    def install[R](self, mp: pytest.MonkeyPatch, owner: object, member: str, shape: Shape[R]) -> None:
        """Bind ``owner.member`` at the production resolution site."""

        def record(args: tuple[object, ...], kwargs: dict[str, object]) -> None:
            self.calls.append((member, args, kwargs))
            self.captured.extend(self.project(args))

        def log(label: str, args: tuple[object, ...], kwargs: dict[str, object]) -> None:
            self.calls.append((label, args, kwargs))

        mp.setattr(owner, member, shape.bind(record, log))

    def projected[K](self, pick: Callable[[SeamRecord], Iterable[K]]) -> list[K]:
        return [item for call in self.calls for item in pick(call)]


# --- [NETWORK_LOOPBACK]


class Loopback(msgspec.Struct, frozen=True, gc=False):
    """Bound loopback endpoint that projects host/port into connection targets."""

    host: str
    port: int

    def target(self, scheme: str = "ssh", user: str = "x") -> str:
        return f"{scheme}://{user}@{self.host}:{self.port}"


@asynccontextmanager
async def loopback_server[S: _AsyncServer](listen: Callable[[], Awaitable[S]], port_of: Callable[[S], int], *, host: str = "127.0.0.1") -> AsyncGenerator[Loopback]:
    """Bind a loopback server for the duration of the ``async with``, yielding its ``Loopback``."""
    async with await listen() as server:
        yield Loopback(host=host, port=port_of(server))


# --- [VIRTUAL_TIME]


def autojump_backend(threshold: float = 0.0) -> tuple[str, dict[str, object]]:
    """Mint an ``anyio_backend`` parameter running the law under trio's autojumping virtual clock.

    Every ``anyio.sleep`` and deadline advances instantly once the loop idles past ``threshold``, so retry, drain, and
    timeout laws prove in microseconds of wall time; the asyncssh double skips itself under this backend.
    """
    return ("trio", {"clock": trio.testing.MockClock(autojump_threshold=threshold)})


# --- [FIXTURE_WRITERS]


class VariantWriter[V](msgspec.Struct, frozen=True, gc=False):
    """Table-driven payload-variant writer for raw bytes or encoded objects."""

    directory: Path
    names: "Mapping[V, str]"
    payloads: "Mapping[V, Variant]"
    encode: Callable[[object], bytes] = msgspec.json.encode
    absent: frozenset[V] = frozenset()

    def path(self, variant: V) -> Path:
        """Materialize one variant and return its path; ``absent`` variants are never written."""
        target = self.directory / self.names[variant]
        payload = self.payloads.get(variant)
        return target if variant in self.absent else self._emit(target, payload if isinstance(payload, bytes) else self.encode(payload))

    def write_all(self) -> dict[V, Path]:
        return {variant: self.path(variant) for variant in self.names}

    @staticmethod
    def _emit(target: Path, raw: bytes) -> Path:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(raw)
        return target


# --- [DECODE_ORACLES]


class NdjsonOracle[T](msgspec.Struct, frozen=True, gc=False):
    """NDJSON oracle that gates the exact line count and decodes every row."""

    decoder: msgspec.json.Decoder[T]
    expect_lines: int = 1

    def rows(self, raw: bytes) -> tuple[T, ...]:
        lines = raw.splitlines()
        assert len(lines) == self.expect_lines, f"expected exactly {self.expect_lines} NDJSON line(s), got {len(lines)}: {lines!r}"
        return tuple(self.decoder.decode(line) for line in lines)

    def one(self, raw: bytes) -> T:
        assert self.expect_lines == 1, f"one() reads a single-write oracle; this oracle expects {self.expect_lines} lines — use rows()"
        return self.rows(raw)[0]

    def from_capture(self, cap: pytest.CaptureFixture[bytes] | pytest.CaptureFixture[str]) -> T:
        out = cap.readouterr().out
        return self.one(out if isinstance(out, bytes) else out.encode())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Async", "Factory", "FanOut", "Loopback", "NdjsonOracle", "SeamProbe", "SeamRecord", "Shape", "Sync", "Variant", "VariantWriter", "autojump_backend", "loopback_server"]
