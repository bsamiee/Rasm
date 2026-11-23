"""RASM102: keyword arguments restating the literal default a same-module function or a `msgspec.Struct` field declares, which a caller can omit with no change to what the call returns or prints."""

import ast
from contextlib import redirect_stdout
from io import StringIO
from textwrap import dedent

import pytest
lazy from refurb.error import Error

from rasm.refurb.defaults import ErrorRestatedDefaultError

# --- [CONSTANTS] ------------------------------------------------------------------------

RULES = (ErrorRestatedDefaultError,)
CASES = frozendict(
    declared=dedent("""\
        import textwrap
        import msgspec
        from typing import override

        WIDTH = 80

        def framed(text: str, *, width: int = 80, height: int = WIDTH) -> str:
            return text.center(width + height)

        def counted(width: int = 80, /, **keywords: int) -> int:
            return width + keywords["width"]

        class Frame(msgspec.Struct, frozen=True, kw_only=True):
            width: int = 80

        class Observed(msgspec.StructMeta):
            @override
            def __call__(cls, *args: object, **fields: object) -> object:
                print(fields)
                return super().__call__(*args, **fields)

        class ObservedFrame(msgspec.Struct, metaclass=Observed, kw_only=True):
            width: int = 80

        class Generic[T](msgspec.Struct, kw_only=True):
            value: T
            width: int = 80

        framed("title", width=80)  # expect: RASM102 `width=80` `framed`
        Frame(width=80)  # expect: RASM102 `width=80` `Frame`
        Generic[int](value=1, width=80)  # expect: RASM102 `width=80` `Generic`
        framed("title", width=60, height=WIDTH)
        counted(width=80)
        ObservedFrame(width=80)
        textwrap.shorten("title text", 5, placeholder=" [...]")
    """),
    resolved=dedent("""\
        from typing import final, overload
        import msgspec

        def framed(*, width: int = 80) -> int:
            return width

        class Service:
            def framed(self, *, width: int = 80) -> int:
                return width

            @final
            def fixed(self, *, width: int = 80) -> int:
                return width

        class Frame(msgspec.Struct, kw_only=True):
            width: int = 80
            title: str = msgspec.field(default="Title")

        class Required(Frame, kw_only=True):
            width: int

        class Changed(Frame, kw_only=True):
            width: int = 60

        class Factory(Frame, kw_only=True):
            width: int = msgspec.field(default_factory=int)

        @final
        class Tools:
            @staticmethod
            def static(*, width: int = 80) -> int:
                return width

            @classmethod
            def classed(cls, *, width: int = 80) -> int:
                return width

        @overload
        def sized(value: int, *, width: int = 80) -> int: ...
        @overload
        def sized(value: str, *, width: int = 80) -> str: ...
        def sized(value: int | str, *, width: int = 90) -> int | str:
            return value

        alias = framed
        Other = Frame
        alias(width=80)  # expect: RASM102 `width=80` `framed`
        Other(width=80)  # expect: RASM102 `width=80` `Frame`
        Service().framed(width=80)
        Service().fixed(width=80)  # expect: RASM102 `width=80` `fixed`
        Required(width=80)
        Changed(width=80)
        Factory(width=80)
        Tools.static(width=80)  # expect: RASM102 `width=80` `static`
        Tools.classed(width=80)  # expect: RASM102 `width=80` `classed`
        shown = t"{framed(width=80)}"  # expect: RASM102 `width=80` `framed`
        Changed(width=60, title="Title")  # expect: RASM102 `width=60` `Changed`; RASM102 `title="Title"` `Changed`
        sized(1, width=80)
        sized("title", width=90)  # expect: RASM102 `width=90` `sized`
    """),
    wrapped=dedent("""\
        from collections.abc import Callable

        def plain(*, width: int = 80) -> int:
            return width

        def observed[**P, R](function: Callable[P, R]) -> Callable[P, R]:
            def wrapped(*args: P.args, **kwargs: P.kwargs) -> R:
                print("width" in kwargs, plain(width=80))  # expect: RASM102 `width=80` `plain`
                return function(*args, **kwargs)
            return wrapped

        @observed
        def framed(*, width: int = 80) -> int:
            return width

        def observer(**keywords: int) -> int:
            return len(keywords)

        alias = framed
        framed(width=80)
        alias(width=80)

        def reassigned() -> int:
            local = plain
            local = observer
            return local(width=80)

        reassigned()
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ran(call: str, namespace: dict[str, object], /) -> tuple[object, str]:
    """Value one call expression returns in the case namespace, and the text it prints."""
    with redirect_stdout(StringIO()) as printed:
        value = eval(call, namespace)
    return value, printed.getvalue()


@pytest.mark.parametrize("case", ["declared", "resolved", "wrapped"])
def test_a_keyword_restating_the_literal_default_its_call_resolves_to_reports(unmatched: frozendict[str, tuple[tuple[str, ...], tuple[str, ...]]], case: str) -> None:
    """A function keyword and a struct field equal to their literal default report, through every declaration a call resolves to.

    Aliases, final methods and final classes, overload implementations, struct subclasses, and template interpolations report.
    A changed value, a non-literal or factory default, a keyword a variadic parameter takes, a struct whose metaclass observes its fields, another module's default, an overridable method, a required field, a decorated function, and a rebound name pass.
    """
    assert unmatched[case] == ((), ())


@pytest.mark.parametrize("case", ["declared", "resolved", "wrapped"])
def test_omitting_a_reported_keyword_leaves_the_call_result_and_output_unchanged(findings: frozendict[str, tuple[Error, ...]], case: str) -> None:
    """Running the case, each reported call returns an equal value and prints the same text with its reported keyword omitted, so a callee, wrapper, or metaclass that observes the keyword shows the difference."""
    namespace: dict[str, object] = {}
    with redirect_stdout(StringIO()):
        exec(CASES[case], namespace)
    calls = {(keyword.value.lineno, keyword.value.col_offset): (call, keyword) for call in ast.walk(ast.parse(CASES[case])) if isinstance(call, ast.Call) for keyword in call.keywords}
    assert findings[case], f"{case} reports no keyword to omit"
    for error in findings[case]:
        call, dropped = calls[error.line, error.column]
        kept, omitted = ast.unparse(call), ast.unparse(ast.Call(call.func, call.args, [keyword for keyword in call.keywords if keyword is not dropped]))
        assert _ran(omitted, namespace) == _ran(kept, namespace), f"{kept} against {omitted}"
