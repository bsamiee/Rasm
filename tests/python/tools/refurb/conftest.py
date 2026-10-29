"""Fixture running repository refurb checks over a sample module and returning the findings one check reports."""

import re
lazy from collections.abc import Callable
lazy from pathlib import Path

import pytest
lazy from refurb.error import Error
lazy from refurb.settings import load_settings

lazy from tools.refurb.__main__ import refurbished

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[type[Error], str], list[tuple[int, str]]]

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(params=["3.15"])
def findings(request: pytest.FixtureRequest, tmp_path: Path) -> Findings:
    """Returns line and message of each finding of one error class over a sample module at the requested Python version, and each mypy compile message at line 0."""

    def reported(kind: type[Error], source: str) -> list[tuple[int, str]]:
        module = re.sub(r"\W+", "_", f"{tmp_path.parent.name}_{request.node.name}")
        (path := tmp_path / f"{module}.py").write_text(source, encoding="utf-8")
        found = refurbished(load_settings([str(path), "--python-version", request.param]))
        return [(error.line, error.msg) if isinstance(error, Error) else (0, error) for error in found if not isinstance(error, Error) or isinstance(error, kind)]

    return reported
