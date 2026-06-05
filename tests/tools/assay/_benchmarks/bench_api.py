"""_fingerprint cache-hit ratio and doctor latency perf laws.

Perf mandate: _fingerprint(6 x 512KB fake DLLs) rounds=20, warmup_rounds=3; len(result) == 16.
Cache-hit ratio: avg_miss / avg_hit >= 10.0 (sha256 vs is_file() check).
Doctor headless: benchmark.stats['mean'] <= 2.0s, @pytest.mark.timeout(15), @skip_no_ilspycmd.
"""

import pytest

from tests.tools.assay.conftest import skip_no_ilspycmd


@pytest.mark.skip(reason="bedrock: perf laws pending — ilspycmd required")
@skip_no_ilspycmd
def bench_fingerprint_cache_ratio() -> None:
    """sha256 cache-hit ratio >= 10x vs cold — requires ilspycmd (skipped by default)."""


@pytest.mark.skip(reason="bedrock: perf laws pending — ilspycmd required")
@skip_no_ilspycmd
def bench_doctor_latency() -> None:
    """Api doctor latency <= 2.0s headless — requires ilspycmd (skipped by default)."""
