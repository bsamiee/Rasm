"""RASM108: literal mappings wrapped in `types.MappingProxyType` where Python 3.15 `frozendict` holds them."""

import pytest
lazy from refurb.error import Error

from rasm.refurb.frozen_mappings import ErrorLiteralMappingProxyError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("rule", [ErrorLiteralMappingProxyError], scope="module")

CASES = frozendict(
    dictionary_display='from types import MappingProxyType\n\ntable = MappingProxyType({"a": 1})\n',
    dictionary_comprehension="from types import MappingProxyType\n\nsquares = MappingProxyType({key: len(key) for key in ('a', 'bb')})\n",
    held_dictionary='from types import MappingProxyType\n\nheld: dict[str, int] = {"a": 1}\nview = MappingProxyType(held)\n',
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("case, lines", [("dictionary_display", [3]), ("dictionary_comprehension", [3]), ("held_dictionary", [])])
def test_a_literal_mapping_in_a_proxy_reports_on_315(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """A dictionary display or comprehension wrapped in a proxy reports, a view over a held dictionary passes."""
    assert [error.line for error in findings[case]] == lines


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_every_proxy_passes_below_315(findings: frozendict[str, tuple[Error, ...]]) -> None:
    """No proxy reports where `frozendict` does not exist."""
    assert findings == frozendict.fromkeys(CASES, ())
