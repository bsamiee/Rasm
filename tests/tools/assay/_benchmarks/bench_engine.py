"""Lease-storm throughput and acquire-release latency perf laws.

Perf mandate: N=32 multiprocessing.spawn contenders → exactly 1 winner, 31 BUSY, no deadlock.
acquire-release round-trip sub-10ms (``benchmark.pedantic``, rounds=5, warmup_rounds=1).
Correctness: lock file size == 0 after all workers exit (truncated on release).

Note: thread-based storm CANNOT test fcntl exclusivity (fcntl is process-scoped, not thread-scoped).
Use ``multiprocessing.get_context('spawn')`` workers. ``@pytest.mark.serial`` prevents xdist concurrency.
"""

import pytest


pytestmark = pytest.mark.benchmark


@pytest.mark.skip(reason="bedrock: perf laws pending — multiprocessing.spawn storm requires dedicated CI lease")
def bench_lease_storm() -> None:
    """32 spawn-contenders → exactly 1 winner; requires dedicated CI lease (skipped by default)."""
