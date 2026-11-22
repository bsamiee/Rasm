"""Findings of one Rasm rule over the case table of the requesting test module, from one refurb build per module and Python version, run as the member's own entry runs it."""

from contextlib import chdir
import sys
lazy from dataclasses import replace

import pytest
lazy from refurb.error import Error, ErrorCode
lazy from refurb.main import run_refurb
lazy from refurb.settings import load_settings

from rasm.refurb import RasmError

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="module", params=[".".join(map(str, sys.version_info[:2]))])
def findings(request: pytest.FixtureRequest, rule: type[RasmError], tmp_path_factory: pytest.TempPathFactory) -> frozendict[str, tuple[Error, ...]]:
    """Builds every source of the module's `CASES` as a module named by its case inside one folder, runs the module's `rule` parameter alone from that folder as the member's own entry runs from its root, and keys each finding by its case.

    A compiler failure fails the test with its messages.

    Returns:
        Findings of each case, in source order.
    """
    folder = tmp_path_factory.mktemp("refurb")
    for case, source in request.module.CASES.items():
        (folder / f"{case}.py").write_text(source, encoding="utf-8")
    settings = load_settings(["--python-version", request.param])
    with chdir(folder):
        found = run_refurb(replace(settings, files=sorted(str(folder / f"{case}.py") for case in request.module.CASES), enable_all=False, disable_all=True, enable={ErrorCode.from_error(rule)}))
    if messages := tuple(error for error in found if isinstance(error, str)):
        pytest.fail("\n".join(messages), pytrace=False)
    return frozendict({case: tuple(error for error in found if isinstance(error, Error) and error.filename == str(folder / f"{case}.py")) for case in request.module.CASES})
