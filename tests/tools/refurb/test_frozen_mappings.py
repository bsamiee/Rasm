"""Literal mappings wrapped in `types.MappingProxyType` on Python 3.15."""

from textwrap import dedent
lazy from collections.abc import Callable, Mapping

import pytest
lazy from refurb.error import Error

lazy from rasm.refurb.frozen_mappings import ErrorLiteralMappingProxyError

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[str | Mapping[str, str], *tuple[type[Error], ...]], tuple[Error, ...]]

# --- [CONSTANTS] ------------------------------------------------------------------------

SOURCE = dedent("""
    from types import MappingProxyType

    held: dict[str, int] = {"a": 1}
    table = MappingProxyType({"a": 1})
    squares = MappingProxyType({key: len(key) for key in held})
    view = MappingProxyType(held)
""")

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_literal_proxy_reports_on_315(findings: Findings) -> None:
    """A dictionary display and a comprehension wrapped in a proxy report, a view over a held dictionary passes."""
    assert [error.line for error in findings(SOURCE, ErrorLiteralMappingProxyError)] == [5, 6]


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_literal_proxy_passes_below_315(findings: Findings) -> None:
    """Every proxy passes where `frozendict` does not exist."""
    assert findings(SOURCE, ErrorLiteralMappingProxyError) == ()
