"""Catalog: Tool row completeness, select(Claim, Language) non-empty, parser callable bind.

Source surface: ``tools/assay/composition/catalog.py`` — ``TOOLS``, ``select``, parser callables
(``parse_tests``, ``parse_findings``, ``parse_verify``).
Laws: select(STATIC, CSHARP) → ≥1 DOTNET tool, select(STATIC, PYTHON) → ≥1 UV tool, every Tool has
non-empty name/runner/command/input/language/claim, parse_* callables are total on malformed input,
select over every (Claim, Language) pair returns a tuple (possibly empty, never raises).
"""

import pytest


def test_bedrock_placeholder() -> None:
    pytest.skip("bedrock: coverage pending")
