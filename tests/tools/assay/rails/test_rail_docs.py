"""Docs rail: check/strict, FaultedPromotion → FAULTED mapping.

Source surface: ``tools/assay/rails/docs.py`` — ``thin_rail``, ``DocsParams``, ``FaultedPromotion``.
Laws: FaultedPromotion.STRICT elevates FAILED → FAULTED on strict=True, FaultedPromotion.LENIENT keeps
FAILED as FAILED, DocsParams.bound('check') with extra positionals → Fault(FAULTED).
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
