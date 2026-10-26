"""Fixture type-checking sample source under repository mypy options and returning the findings one check reports."""

import re
lazy from collections.abc import Callable
lazy from pathlib import Path

import pytest
lazy from mypy.build import build
lazy from mypy.main import process_options
lazy from refurb import types
lazy from refurb.error import Error
lazy from refurb.loader import load_checks
lazy from refurb.settings import load_settings
lazy from refurb.visitor import RefurbVisitor

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[type[Error], str], list[tuple[int, str]]]

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(params=["3.15"])
def findings(request: pytest.FixtureRequest, tmp_path: Path) -> Findings:
    """Returns line and message of each finding of one error class over a sample module at the requested Python version."""
    settings = load_settings(["--python-version", request.param])

    def reported(kind: type[Error], source: str) -> list[tuple[int, str]]:
        module = re.sub(r"\W+", "_", f"{tmp_path.parent.name}_{request.node.name}")
        (path := tmp_path / f"{module}.py").write_text(source, encoding="utf-8")
        [sample], options = process_options([str(path), *settings.mypy_args])
        options.preserve_asts = True
        options.python_version = settings.get_python_version()
        result = build([sample], options=options)
        builtins, tree = result.graph["builtins"], result.graph[sample.module].tree
        if builtins.tree is None:
            builtins.load_tree()
        assert builtins.tree is not None
        assert tree is not None
        types.BUILTINS_MYPY_FILE = builtins.tree
        visitor = RefurbVisitor(load_checks(settings), settings)
        visitor.accept(tree)
        return [(error.line, error.msg) for error in visitor.errors if isinstance(error, kind)]

    return reported
