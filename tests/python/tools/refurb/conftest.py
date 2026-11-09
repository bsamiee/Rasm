"""Native diagnostics for selected rules over a source sample."""

import sys
lazy from collections.abc import Mapping
lazy from dataclasses import replace

import pytest
lazy from refurb.error import Error, ErrorCode
lazy from refurb.settings import load_settings

lazy from rasm_refurb import checked, Findings

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session", params=[".".join(map(str, sys.version_info[:2]))])
def findings(request: pytest.FixtureRequest, tmp_path_factory: pytest.TempPathFactory) -> Findings:
    """Runs selected Refurb rules over one typed build and reports compiler failures directly."""
    directory = tmp_path_factory.mktemp("refurb")
    settings = load_settings(["--python-version", request.param])

    def analyzed(sources: str | Mapping[str, str], /, *rules: type[Error]) -> tuple[Error, ...]:
        samples = {directory / f"{name}.py": source for name, source in ({"sample": sources} if isinstance(sources, str) else sources).items()}
        for path, source in samples.items():
            path.write_text(source, encoding="utf-8")
        found = checked(replace(settings, files=list(map(str, samples)), enable_all=False, disable_all=True, enable={ErrorCode.from_error(rule) for rule in rules}))
        if messages := tuple(error for error in found if isinstance(error, str)):
            pytest.fail("\n".join(messages), pytrace=False)
        return tuple(error for error in found if isinstance(error, Error))

    return analyzed
