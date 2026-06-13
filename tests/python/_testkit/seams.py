"""Seam engine: project-agnostic, polymorphic test doubles distilling seam MECHANISM from PAYLOAD.

Surfaces: a recording monkeypatch installer dispatched by call SHAPE (``SeamProbe`` + the ``Shape`` algebra);
a spec-bound psutil double (``autospec_proc`` / ``psutil_module_double`` / ``install_module_attr``); a
leak-free loopback capsule (``Loopback`` / ``loopback_server``); a table-driven payload-variant writer
(``VariantWriter``); an injected-settings tmp-root harness (``TmpRoot`` / ``tmp_root``); and an NDJSON decode
oracle (``NdjsonOracle``). Every domain dependency enters through an injected ``Callable``/``Decoder``/owner
— the engine imports NO project package; dispatch is STRUCTURAL (``match`` on the variant TYPE / payload
SHAPE), never stringly-typed, and the closed ``Shape`` union proves exhaustiveness.
"""

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

# SeamRecord carries member name for log readability; dispatch keys on Shape VARIANT, not the string.
type SeamRecord = tuple[str, tuple[object, ...], dict[str, object]]
type Variant = bytes | object


class _AsyncServer(Protocol):
    """Structural bound for an awaited server object that is its own async context manager (enters to itself)."""

    async def __aenter__(self) -> Self: ...
    async def __aexit__(self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: TracebackType | None) -> object: ...


class KitFactory[S](Protocol):
    """Per-suite kit constructor: project a tmp root into the suite's settings/harness payload.

    The single injection seam every suite kit shares — one suite binds its settings model, another binds
    ``None`` for a bare module-tree kit — so ``tmp_root`` and everything built on ``TmpRoot`` never import
    a project type.
    """

    def __call__(self, root: Path, /) -> S: ...


# --- [MODELS] ---------------------------------------------------------------------------

# --- [RECORDING_PATCH]


def _noproject[A](_args: tuple[object, ...]) -> tuple[A, ...]:
    """Return the empty tuple; default for ``SeamProbe.project`` when callers only need the call log."""
    return ()


class Sync[R](msgspec.Struct, frozen=True, gc=False):
    """Synchronous seam: ``(*args, **kwargs) -> value`` (the append-only sink uses ``Sync(None)``)."""

    value: R


class Async[R](msgspec.Struct, frozen=True, gc=False):
    """Awaited seam: ``async (*args, **kwargs) -> value`` for a coroutine the SUT ``await``s."""

    value: R


class FanOut[R](msgspec.Struct, frozen=True, gc=False):
    """Batch seam: ``(items, **kwargs) -> values`` recording the ``items`` tuple as the sole positional."""

    values: tuple[R, ...]


class Factory[R](msgspec.Struct, frozen=True, gc=False):
    """Curried seam: ``(bind...) -> (call...) -> value`` recording bind-layer then ``inner_label`` call-layer."""

    value: R
    inner_label: str = "<factory>.run"


type Shape[R] = Sync[R] | Async[R] | FanOut[R] | Factory[R]


class SeamProbe[A](msgspec.Struct, frozen=True, gc=False):
    """Recording monkeypatch host: install a canned seam by call SHAPE and capture every invocation.

    ``project`` maps the positional args of each recorded call to the capture stream ``captured``; the default
    captures nothing (pure call recorder). ``install`` matches the closed ``Shape`` union — sync / awaited /
    fan-out / curried-factory — binding the matching canned callable via the single impure boundary
    ``mp.setattr(owner, member, fn)``. ``projected`` derives an arbitrary view over the raw ``calls`` log.
    """

    project: Callable[[tuple[object, ...]], Iterable[A]] = _noproject
    calls: list[SeamRecord] = msgspec.field(default_factory=list)
    captured: list[A] = msgspec.field(default_factory=list)

    def install[R](self, mp: pytest.MonkeyPatch, owner: object, member: str, shape: Shape[R]) -> None:
        """Bind ``owner.member`` to the canned callable selected by the ``shape`` variant.

        ``owner`` must be the production resolution site (the module re-binding the seam), not the definition
        site, so the patch mirrors how production resolves the name.
        """

        def record(args: tuple[object, ...], kwargs: dict[str, object]) -> None:
            self.calls.append((member, args, kwargs))
            self.captured.extend(self.project(args))

        match shape:
            case Async(value=value):

                async def run_async(*args: object, **kwargs: object) -> R:  # noqa: RUF029  # async required: production callsite awaits this seam
                    record(args, kwargs)
                    return value

                mp.setattr(owner, member, run_async)
            case FanOut(values=values):

                def run_fan(items: object, **kwargs: object) -> tuple[R, ...]:
                    record((items,), kwargs)
                    return values

                mp.setattr(owner, member, run_fan)
            case Factory(value=value, inner_label=inner_label):

                def run_factory(*bind: object, **bind_kwargs: object) -> Callable[..., R]:
                    record(bind, bind_kwargs)

                    def run_call(*call: object, **call_kwargs: object) -> R:
                        self.calls.append((inner_label, call, call_kwargs))
                        return value

                    return run_call

                mp.setattr(owner, member, run_factory)
            case Sync(value=value):

                def run_sync(*args: object, **kwargs: object) -> R:
                    record(args, kwargs)
                    return value

                mp.setattr(owner, member, run_sync)

    def projected[K](self, pick: Callable[[SeamRecord], Iterable[K]]) -> list[K]:
        return [item for call in self.calls for item in pick(call)]


# --- [NETWORK_LOOPBACK]


class Loopback(msgspec.Struct, frozen=True, gc=False):
    """A bound loopback endpoint projecting host/port to a connect target.

    ``target`` always emits an explicit user because credential normalizers (e.g. asyncssh ``saslprep``)
    reject a ``None`` username at connect.
    """

    host: str
    port: int

    def target(self, scheme: str = "ssh", user: str = "x") -> str:
        return f"{scheme}://{user}@{self.host}:{self.port}"


@asynccontextmanager
async def loopback_server[S: _AsyncServer](
    listen: Callable[[], Awaitable[S]], port_of: Callable[[S], int], *, host: str = "127.0.0.1"
) -> AsyncGenerator[Loopback]:
    """Bind a loopback server for the duration of the ``async with`` and yield its endpoint capsule.

    Teardown is tied to the server's own ``async with`` — no daemon threads, no manual close, no
    ResourceWarning under ``filterwarnings=["error"]``.

    Yields:
        A ``Loopback`` carrying ``host`` and the bound port.
    """
    async with await listen() as server:
        yield Loopback(host=host, port=port_of(server))


# --- [FIXTURE_WRITERS]


class VariantWriter[V](msgspec.Struct, frozen=True, gc=False):
    """Table-driven payload-variant writer: materialize ``{variant: payload-or-raw-bytes}`` to a directory.

    Dispatch is structural on the variant's membership in ``absent`` and its payload SHAPE (raw ``bytes`` vs
    an object handed to ``encode``) — the variant family is data, never N hand-written methods.
    """

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
    """Isolated tmp-tree harness: a ``write`` primitive plus an injected ``settings`` projection.

    ``make_settings`` (via ``tmp_root``) is the sole injection seam, so the engine never imports any
    project settings type.
    """

    root: Path
    settings: S

    def write(self, rel: str | Path, text: str = "", *, mode: int | None = None) -> Path:
        """Write ``text`` under ``rel`` (empty for a touch), creating parents, optionally ``chmod``-ing.

        Returns:
            The absolute path written.
        """
        path = self.root / Path(rel)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        path.chmod(mode) if mode is not None else None
        return path


def tmp_root[S](root: Path, make_settings: KitFactory[S]) -> TmpRoot[S]:
    """Build a ``TmpRoot`` rooted at ``root``; ``make_settings`` is the sole project-settings injection seam.

    Returns:
        A ``TmpRoot`` carrying the root and its derived settings.
    """
    return TmpRoot(root=root, settings=make_settings(root))


# --- [DECODE_ORACLES]


class NdjsonOracle[T](msgspec.Struct, frozen=True, gc=False):
    """NDJSON decode oracle: assert an exact line count and decode the first line via the injected decoder."""

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
    """Build a ``create_autospec(spec, instance=True)`` double from plain fields and method return values.

    ``methods`` returns are assigned onto the autospec CHILD mocks (``p.<m>.return_value = ...``), not the
    parent; ``dead=True`` stamps a private ``_dead`` sentinel that ``psutil_module_double``'s factory reads
    to raise not-found instead of returning the double.

    Returns:
        A spec-bound ``MagicMock`` carrying the requested field values and method returns.
    """
    proc: MagicMock = create_autospec(spec, instance=True)
    [setattr(proc, name, value) for name, value in fields.items()]
    [setattr(getattr(proc, name), "return_value", value) for name, value in methods.items()]
    proc._dead = dead
    return proc


def psutil_module_double[E: BaseException](
    real: object, procs: Mapping[int | None, MagicMock], *, not_found: Callable[[int | None], E], extra: Mapping[str, object] = {}
) -> MagicMock:
    """Wrap a pid→double mapping into a ``MagicMock(spec=real)`` module whose ``Process`` factory dispatches it.

    ``procs[None]`` is the self-process; integer keys are explicit pids. An unregistered pid raises
    ``not_found(pid)`` (no silent fallback); a double stamped ``_dead`` raises ``not_found(double.pid)``.
    ``extra`` re-binds the module's real Error/NoSuchProcess/AccessDenied/cpu_count so SUT ``except`` clauses
    stay catchable under the double — the engine itself never imports psutil.

    Returns:
        A ``psutil`` module double with a pid-dispatching ``Process`` side effect.
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
    """Pin ``double`` onto ``owner.attr`` via monkeypatch.

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
