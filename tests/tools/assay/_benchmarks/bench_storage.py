"""ArtifactStore file and memory backend perf laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from itertools import count
from typing import Protocol, TYPE_CHECKING

import pytest


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.composition.settings import ArtifactStore


# --- [TYPES] ----------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark


class StorageBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    def pedantic[T](self, target: Callable[[], T], *, rounds: int, warmup_rounds: int) -> T: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_PAYLOAD = b"x" * 4096


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_artifact_store_file_backend(benchmark: StorageBenchmark, assay_root: AssayHarness) -> None:
    """File-backed ArtifactStore writes and reads a bounded payload through fsspec."""
    store = assay_root.settings.store()
    keys = count()

    def write_read() -> bytes:
        path = store.write_bytes(_PAYLOAD, "benchmark", f"{next(keys)}.bin")
        return store.read_path(path)

    result = benchmark.pedantic(write_read, rounds=100, warmup_rounds=5)

    assert result == _PAYLOAD


def bench_artifact_store_memory_backend(benchmark: StorageBenchmark, mem_store: ArtifactStore) -> None:
    """Memory-backed ArtifactStore keeps the same store API and metadata path."""
    keys = count()

    def write_read() -> bytes:
        path = mem_store.write_bytes(_PAYLOAD, "benchmark", f"{next(keys)}.bin")
        return mem_store.read_path(path)

    result = benchmark.pedantic(write_read, rounds=100, warmup_rounds=5)

    assert result == _PAYLOAD
