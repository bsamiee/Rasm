"""Keyword arguments restating literal defaults."""

from textwrap import dedent

from tools.refurb.defaults import ErrorRestatedDefaultError
lazy from tests.python.support.doubles import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_restated_function_and_field_defaults_report(findings: Findings) -> None:
    """Keyword arguments equal to a same-module function default and a struct field default, both reported."""
    source = dedent("""
        import msgspec

        def framed(text: str, *, width: int = 80) -> str:
            return text.center(width)

        class Frame(msgspec.Struct, frozen=True, kw_only=True):
            width: int = 80

        framed("title", width=80)
        Frame(width=80)
    """)
    assert findings(ErrorRestatedDefaultError, source) == [
        (10, "`width=80` restates the default `framed` declares, omit `width`"),
        (11, "`width=80` restates the default `Frame` declares, omit `width`"),
    ]


def test_other_module_defaults_and_new_values_pass(findings: Findings) -> None:
    """Another module's default and a differing value, not reported."""
    source = dedent("""
        import textwrap

        def framed(text: str, *, width: int = 80) -> str:
            return text.center(width)

        framed("title", width=60)
        textwrap.shorten("title text", 5, placeholder=" [...]")
    """)
    assert findings(ErrorRestatedDefaultError, source) == []
