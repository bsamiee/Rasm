"""Project-agnostic seam doubles, fixture writers, loopback capsules, and decode oracles."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable, Iterable
from contextlib import asynccontextmanager
from pathlib import Path
from types import TracebackType  # noqa: TC003  # Protocol dunder __aexit__ annotation requires runtime presence
from typing import Protocol, Self, TYPE_CHECKING
from unittest.mock import create_autospec, MagicMock

import msgspec


if TYPE_CHECKING:
    from collections.abc import AsyncGenerator, Awaitable, Mapping

    import pytest


# --- [TYPES] ----------------------------------------------------------------------------

# Member names are diagnostic only; dispatch keys on the Shape variant.
type SeamRecord = tuple[str, tuple[object, ...], dict[str, object]]
type Recorder = Callable[[tuple[object, ...], dict[str, object]], None]
type SeamLog = Callable[[str, tuple[object, ...], dict[str, object]], None]
type Variant = bytes | object


class _AsyncServer(Protocol):
    """Awaited server that enters as its own async context manager."""

    async def __aenter__(self) -> Self: ...
    async def __aexit__(self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: TracebackType | None) -> object: ...


class KitFactory[S](Protocol):
    """Project a tmp root into a suite-specific settings or harness payload."""

    def __call__(self, root: Path, /) -> S: ...


# --- [MODELS] ---------------------------------------------------------------------------

# --- [RECORDING_PATCH]


def _noproject[A](_args: tuple[object, ...]) -> tuple[A, ...]:
    """Return no captured projections for call-log-only probes."""
    return ()


class Sync[R](msgspec.Struct, frozen=True, gc=False):
    """Synchronous seam: ``(*args, **kwargs) -> value`` (the append-only sink uses ``Sync(None)``)."""

    value: R

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs; each variant owns its own call shape.

        Returns:
            The monkeypatch-ready callable for this seam.
        """
        _ = log

        def run_sync(*args: object, **kwargs: object) -> R:
            record(args, kwargs)
            return self.value

        return run_sync


class Async[R](msgspec.Struct, frozen=True, gc=False):
    """Awaited seam: ``async (*args, **kwargs) -> value`` for a coroutine the SUT ``await``s."""

    value: R

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs; each variant owns its own call shape.

        Returns:
            The monkeypatch-ready callable for this seam.
        """
        _ = log

        async def run_async(*args: object, **kwargs: object) -> R:  # noqa: RUF029  # async required: production callsite awaits this seam
            record(args, kwargs)
            return self.value

        return run_async


class FanOut[R](msgspec.Struct, frozen=True, gc=False):
    """Batch seam: ``(items, **kwargs) -> values`` recording the ``items`` tuple as the sole positional."""

    values: tuple[R, ...]

    def bind(self, record: Recorder, log: SeamLog) -> Callable[..., object]:
        """Build the recording runner this seam installs; each variant owns its own call shape.

        Returns:
            The monkeypatch-ready callable for this seam.
        """
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
        """Build the recording runner this seam installs; each variant owns its own call shape.

        Returns:
            The monkeypatch-ready callable for this seam.
        """

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
async def loopback_server[S: _AsyncServer](
    listen: Callable[[], Awaitable[S]], port_of: Callable[[S], int], *, host: str = "127.0.0.1"
) -> AsyncGenerator[Loopback]:
    """Bind a loopback server for the duration of the ``async with``.

    Yields:
        A ``Loopback`` carrying ``host`` and the bound port.
    """
    async with await listen() as server:
        yield Loopback(host=host, port=port_of(server))


# --- [FIXTURE_WRITERS]


class VariantWriter[V](msgspec.Struct, frozen=True, gc=False):
    """Table-driven payload-variant writer for raw bytes or encoded objects."""

    directory: Path
    names: "Mapping[V, str]"
    payloads: "Mapping[V, Variant]"
    encode: Callable[[object], bytes] = msgspec.json.encode
    absent: frozenset[V] = frozenset()

    def path(self, variant: V) -> Path:
        """Materialize one variant; ``absent`` variants are never written.

        Returns:
            The path to the (possibly unwritten) variant file.
        """
        target = self.directory / self.names[variant]
        match variant in self.absent, self.payloads.get(variant):
            case True, _:
                return target
            case _, bytes() as raw:
                return self._emit(target, raw)
            case _, payload:
                return self._emit(target, self.encode(payload))
        return target  # unreachable: the `_, payload` arm is irrefutable (ty cannot prove 2-tuple exhaustiveness)

    def write_all(self) -> dict[V, Path]:
        return {variant: self.path(variant) for variant in self.names}

    @staticmethod
    def _emit(target: Path, raw: bytes) -> Path:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(raw)
        return target


class TmpRoot[S](msgspec.Struct, frozen=True, gc=False):
    """Isolated tmp-tree harness with a write primitive and injected settings projection."""

    root: Path
    settings: S

    def write(self, rel: str | Path, text: str = "", *, mode: int | None = None) -> Path:
        """Write ``text`` under ``rel`` and optionally apply a mode.

        Returns:
            The absolute path written.
        """
        path = self.root / Path(rel)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        path.chmod(mode) if mode is not None else None
        return path


def tmp_root[S](root: Path, make_settings: KitFactory[S]) -> TmpRoot[S]:
    """Build a ``TmpRoot`` from an injected settings projector.

    Returns:
        A ``TmpRoot`` carrying the root and its derived settings.
    """
    return TmpRoot(root=root, settings=make_settings(root))


# --- [DECODE_ORACLES]


class NdjsonOracle[T](msgspec.Struct, frozen=True, gc=False):
    """NDJSON oracle that checks line count before decoding the first row."""

    decoder: msgspec.json.Decoder[T]
    expect_lines: int = 1

    def one(self, raw: bytes) -> T:
        rows = raw.splitlines()
        assert len(rows) == self.expect_lines, f"expected exactly {self.expect_lines} NDJSON line(s), got {len(rows)}: {rows!r}"
        return self.decoder.decode(rows[0])

    def from_capture(self, cap: pytest.CaptureFixture[bytes] | pytest.CaptureFixture[str]) -> T:
        out = cap.readouterr().out
        return self.one(out if isinstance(out, bytes) else out.encode())


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESS_DOUBLES]


def autospec_proc(spec: type, *, fields: Mapping[str, object] = {}, methods: Mapping[str, object] = {}, dead: bool = False) -> MagicMock:
    """Build a spec-bound process double from fields and method return values.

    Returns:
        ``MagicMock`` with requested field values, child mock returns, and death sentinel.
    """
    proc: MagicMock = create_autospec(spec, instance=True)
    [setattr(proc, name, value) for name, value in fields.items()]
    [setattr(getattr(proc, name), "return_value", value) for name, value in methods.items()]
    proc._dead = dead
    return proc


def psutil_module_double[E: BaseException](
    real: object, procs: Mapping[int | None, MagicMock], *, not_found: Callable[[int | None], E], extra: Mapping[str, object] = {}
) -> MagicMock:
    """Build a psutil module double whose ``Process`` factory dispatches by pid.

    Returns:
        Module-shaped ``MagicMock`` with pid dispatch and injected exception/capability bindings.
    """
    fake = MagicMock(spec=real)
    [setattr(fake, name, value) for name, value in extra.items()]

    def process_factory(pid: int | None = None) -> MagicMock:
        match procs.get(pid):
            case None:
                raise not_found(pid)
            case proc if getattr(proc, "_dead", False):
                raise not_found(proc.pid)
            case proc:
                return proc

    fake.Process.side_effect = process_factory
    return fake


def install_module_attr[D](mp: pytest.MonkeyPatch, owner: object, attr: str, double: D) -> D:
    """Pin ``double`` onto ``owner.attr``.

    Returns:
        ``double`` unchanged, for fluent ``.cpu_percent = ...``-style post-configuration.
    """
    mp.setattr(owner, attr, double)
    return double


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Async",
    "Factory",
    "FanOut",
    "KitFactory",
    "Loopback",
    "NdjsonOracle",
    "SeamProbe",
    "SeamRecord",
    "Shape",
    "Sync",
    "TmpRoot",
    "Variant",
    "VariantWriter",
    "autospec_proc",
    "install_module_attr",
    "loopback_server",
    "psutil_module_double",
    "tmp_root",
]
