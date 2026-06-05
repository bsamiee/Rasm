"""Package rail: list/plan typed detail P1/P2, YakMeta.from_props, validate preconditions, _commit atomic rotate.

Source surface: ``tools/assay/rails/package.py`` — ``YakMeta``, ``list``, ``plan``, ``validate``,
``_commit``, ``_slug_from_bytes``.
Laws: YakMeta.from_props round-trip, _commit rotates staged/ → yak/ atomically, validate rejects .dll target_ext,
validate rejects win platform, list returns Ok(Report) when tree populated, P1 list detail is None not PackageRun
(xfail strict), P2 plan detail omits version (xfail strict).
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
