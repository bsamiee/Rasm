"""Native diagnostics from source compiled once per interpreter and sample."""

lazy from collections.abc import Callable
lazy from functools import cache

import pytest
lazy from refurb.error import Error
lazy from refurb.settings import load_settings

lazy from tools.refurb.__main__ import checked

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[type[Error], str], tuple[Error, ...]]

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session", params=["3.15"])
def findings(request: pytest.FixtureRequest, tmp_path_factory: pytest.TempPathFactory) -> Findings:
    """Returns native findings by class, sharing one compiled answer across queries and reporting compiler failures directly."""
    path = tmp_path_factory.mktemp("refurb") / "sample.py"

    @cache
    def analyzed(source: str) -> tuple[Error, ...]:
        path.write_text(source, encoding="utf-8")
        found = checked(load_settings([str(path), "--python-version", request.param]))
        if messages := tuple(error for error in found if isinstance(error, str)):
            pytest.fail("\n".join(messages), pytrace=False)
        return tuple(error for error in found if isinstance(error, Error))

    return lambda kind, source: tuple(error for error in analyzed(source) if isinstance(error, kind))
