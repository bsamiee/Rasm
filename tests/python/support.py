"""Recursive tolerance predicate and assertion, deterministic codec round trips, mock installs logging every call in order, fixture file writer, and benchmark cases with time budgets."""

from collections.abc import Callable
from datetime import timedelta
from typing import Protocol
lazy import cmath
lazy from collections.abc import Iterable, Mapping, Sequence
lazy from decimal import Decimal
lazy from fractions import Fraction
lazy from pathlib import Path
lazy from unittest.mock import Mock

import msgspec
import msgspec.json
import msgspec.msgpack
from pytest_benchmark.stats import Stats
lazy from expression import Option, Result
lazy from expression.collections import Block
lazy import numpy as np
lazy import pint
lazy import psutil
lazy import pytest
lazy from pytest_benchmark.fixture import BenchmarkFixture

# --- [TYPES] ----------------------------------------------------------------------------


class _Subject[*Ts](Protocol):
    __qualname__: str

    def __call__(self, *args: *Ts) -> object: ...


class _Sized(Protocol):
    @property
    def sizes(self) -> tuple[int, ...]: ...


class _Member(Protocol):
    __name__: str


# --- [MODELS] ---------------------------------------------------------------------------


class BenchmarkCase[*Ts](msgspec.Struct, frozen=True, kw_only=True):
    """Subject under test, its argument workload per size, sizes, and time budget over one timing statistic."""

    subject: _Subject[*Ts]
    workload: Callable[[int], tuple[*Ts]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget: timedelta = timedelta(milliseconds=100)
    statistic: Callable[[Stats], float] = lambda stats: stats.median

    def measure(self, benchmark: BenchmarkFixture, size: int) -> object:
        """Time ``subject`` over the workload at ``size``, record resident memory growth, and fail a timed run past the budget."""
        process, arguments, millisecond = psutil.Process(), self.workload(size), timedelta(milliseconds=1)
        benchmark.group = self.subject.__qualname__
        rss_before = process.memory_info().rss
        result = benchmark(self.subject, *arguments)
        observed = Option.of_optional(benchmark.stats).map(lambda metadata: timedelta(seconds=self.statistic(metadata.stats)))
        benchmark.extra_info.update(
            rss_delta_bytes=process.memory_info().rss - rss_before, budget_ms=self.budget / millisecond, observed_ms=observed.map(lambda time: time / millisecond).to_optional(), size=size
        )
        match observed:
            case Option(tag="some", some=time) if time > self.budget:
                pytest.fail(f"{time} exceeds budget {self.budget}")
        return result


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TOLERANCE]


def _diverge(a: object, b: object, path: str, *, rel_tol: float, abs_tol: float) -> str | None:
    """Return first tolerance divergence at its structural path, or ``None``."""
    match (a, b):
        case (bool() | str() | bytes(), _) | (_, bool() | str() | bytes()):
            return None if isinstance(a, bool) == isinstance(b, bool) and a == b else f"{path}: {a!r} != {b!r}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()) if (left := np.asarray(a)).shape != (right := np.asarray(b)).shape:
            return f"{path}: shape {left.shape} != {right.shape}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()):
            left, right = np.atleast_1d(np.asarray(a), np.asarray(b))
            far = np.argwhere(~np.isclose(left, right, rtol=rel_tol, atol=abs_tol, equal_nan=True))
            return None if far.size == 0 else f"{path}{far[0].tolist()}: {left[tuple(far[0])]!r} !~ {right[tuple(far[0])]!r}"
        case ((int() | float() | complex() | Decimal() | Fraction()) as left, (int() | float() | complex() | Decimal() | Fraction()) as right):
            fa, fb = complex(left), complex(right)
            return None if cmath.isclose(fa, fb, rel_tol=rel_tol, abs_tol=abs_tol) or (cmath.isnan(fa) and cmath.isnan(fb)) else f"{path}: {a!r} !~ {b!r} at rel_tol={rel_tol}, abs_tol={abs_tol}"
        case (Result(tag=tag), Result(tag=other)) | (Option(tag=tag), Option(tag=other)) if tag != other:
            return f"{path}: tags differ, {a!r} != {b!r}"
        case (Result(tag="ok" as tag, ok=x), Result(ok=y)) | (Result(tag="error" as tag, error=x), Result(error=y)) | (Option(tag="some" as tag, some=x), Option(some=y)):
            children: Iterable[tuple[str, object, object]] = ((f".{tag}", x, y),)
        case (Option(), Option()):
            return None
        case (msgspec.Struct(), msgspec.Struct()) if type(a) is type(b):
            children = ((f".{name}", x, y) for (name, x), y in zip(msgspec.structs.asdict(a).items(), msgspec.structs.astuple(b), strict=True))
        case (pint.Quantity() as left, pint.Quantity() as right) if left.units != right.units:
            return f"{path}: units {left.units!r} != {right.units!r}"
        case (pint.Quantity() as left, pint.Quantity() as right):
            children = ((".magnitude", left.magnitude, right.magnitude),)
        case (Mapping(), Mapping()) if set(a) != set(b):
            return f"{path}: unshared keys {sorted(map(repr, set(a) ^ set(b)))}"
        case (Mapping(), Mapping()):
            children = ((f"[{key!r}]", value, b[key]) for key, value in a.items())
        case (Block() as left, Block() as right) | (Sequence() as left, Sequence() as right) if len(left) != len(right):
            return f"{path}: length {len(left)} != {len(right)}"
        case (Block() as left, Block() as right) | (Sequence() as left, Sequence() as right):
            children = ((f"[{index}]", x, y) for index, (x, y) in enumerate(zip(left, right, strict=True)))
        case _:
            return None if a == b else f"{path}: {a!r} != {b!r}"
    return next((divergence for suffix, x, y in children if (divergence := _diverge(x, y, f"{path}{suffix}", rel_tol=rel_tol, abs_tol=abs_tol)) is not None), None)


def close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> bool:
    """Return whether ``actual`` and ``expected`` agree within tolerance at every structural path."""
    return _diverge(actual, expected, "$", rel_tol=rel_tol, abs_tol=abs_tol) is None


def assert_close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> None:
    """Assert recursive tolerance equality, naming first diverging structural path."""
    assert (divergence := _diverge(actual, expected, "$", rel_tol=rel_tol, abs_tol=abs_tol)) is None, f"tolerance violation at {divergence}"


# --- [CODECS]


def assert_roundtrip[T](value: T, encoder: msgspec.json.Encoder | msgspec.msgpack.Encoder, decoder: msgspec.json.Decoder[T] | msgspec.msgpack.Decoder[T]) -> T:
    """Return decoded value after asserting equality and byte-identical re-encode."""
    raw = encoder.encode(value)
    decoded = decoder.decode(raw)
    assert decoded == value, f"{decoder.type!r} decode: {decoded!r} != {value!r}"
    assert (again := encoder.encode(decoded)) == raw, f"{decoder.type!r} re-encode: {again!r} != {raw!r}"
    return decoded


# --- [RECORDING]


def install(monkeypatch: pytest.MonkeyPatch, target: object, stubs: Mapping[_Member, Mock]) -> Mock:
    """Replace each member of ``target`` with its mock and return the parent whose ``mock_calls`` log every call in order."""
    log = Mock()
    for member, stub in stubs.items():
        log.attach_mock(stub, member.__name__)
        monkeypatch.setattr(target, member.__name__, stub)
    return log


# --- [FIXTURES]


def write_fixtures(directory: Path, files: Mapping[str, object], encode: Callable[[object], bytes] = msgspec.json.encode) -> frozendict[str, Path]:
    """Write each file under ``directory``, non-bytes content through ``encode``, and return paths by name."""
    paths = frozendict({name: directory / name for name in files})
    for name, path in paths.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content if isinstance(content := files[name], bytes) else encode(content))
    return paths


# --- [BENCHMARKS]


def benchmarks(cases: Iterable[_Sized]) -> pytest.MarkDecorator:
    """Parametrize ``case`` and ``size`` over every case at each of its sizes, identified by subject and size."""
    return pytest.mark.parametrize("case,size", [(case, size) for case in cases for size in case.sizes], ids=lambda value: value.subject.__qualname__ if isinstance(value, BenchmarkCase) else None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchmarkCase", "assert_close", "assert_roundtrip", "benchmarks", "close", "install", "write_fixtures"]
