"""RASM102: keyword arguments restating the literal default a same-module function or a `msgspec.Struct` field declares."""

from textwrap import dedent

import pytest
lazy from refurb.error import Error

from rasm.refurb.defaults import ErrorRestatedDefaultError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("rule", [ErrorRestatedDefaultError], scope="module")

CASES = frozendict(
    function_and_field=dedent("""\
        import msgspec
        from typing import override

        def framed(text: str, *, width: int = 80) -> str:
            return text.center(width)

        class Frame(msgspec.Struct, frozen=True, kw_only=True):
            width: int = 80

        framed("title", width=80)
        Frame(width=80)

        class Observed(msgspec.StructMeta):
            @override
            def __call__(cls, *args: object, **fields: object) -> object:
                print(fields)
                return super().__call__(*args, **fields)

        class ObservedFrame(msgspec.Struct, metaclass=Observed, kw_only=True):
            width: int = 80

        ObservedFrame(width=80)
    """),
    other_module_and_new_value=dedent("""\
        import textwrap

        WIDTH = 80

        def framed(text: str, *, width: int = 80, height: int = WIDTH) -> str:
            return text.center(width + height)

        framed("title", width=60, height=WIDTH)
        textwrap.shorten("title text", 5, placeholder=" [...]")
    """),
    aliases=dedent("""\
        import msgspec

        def framed(*, width: int = 80) -> int:
            return width

        class Service:
            def framed(self, *, width: int = 80) -> int:
                return width

        class Frame(msgspec.Struct, kw_only=True):
            width: int = msgspec.field(default=80)

        Other = Frame
        alias = framed
        method = Service().framed
        alias(width=80)
        method(width=80)
        Other(width=80)
    """),
    template_string=dedent("""\
        def framed(*, width: int = 80) -> int:
            return width

        shown = t"{framed(width=80)}"
    """),
    positional_only=dedent("""\
        def framed(width: int = 80, /, **keywords: int) -> int:
            return width + keywords["width"]

        framed(width=80)
    """),
    struct_overrides=dedent("""\
        import msgspec

        class Frame(msgspec.Struct, kw_only=True):
            width: int = 80
            title: str = msgspec.field(default="Title")

        class Required(Frame, kw_only=True):
            width: int

        class Factory(Frame, kw_only=True):
            width: int = msgspec.field(default_factory=int)

        class Changed(Frame, kw_only=True):
            width: int = 60

        Required(width=80)
        Factory(width=80)
        Changed(width=80)
        Changed(width=60, title="Title")
    """),
    overloads=dedent("""\
        from typing import overload

        @overload
        def framed(value: int, *, width: int = 80) -> int: ...
        @overload
        def framed(value: str, *, width: int = 80) -> str: ...
        def framed(value: int | str, *, width: int = 90) -> int | str:
            return value

        framed(1, width=80)
        framed("title", width=90)
    """),
    generic_struct=dedent("""\
        import msgspec

        class Frame[T](msgspec.Struct, kw_only=True):
            value: T
            width: int = 80

        Frame[int](value=1, width=80)
    """),
    wrappers=dedent("""\
        from collections.abc import Callable
        from typing import overload

        def plain(*, width: int = 80) -> int:
            return width

        def observed[**P, R](function: Callable[P, R]) -> Callable[P, R]:
            def wrapped(*args: P.args, **kwargs: P.kwargs) -> R:
                print('width' in kwargs)
                plain(width=80)
                return function(*args, **kwargs)
            return wrapped

        @observed
        def framed(*, width: int = 80) -> int:
            return width

        alias = framed
        framed(width=80)
        alias(width=80)

        @overload
        def overloaded(value: int, *, width: int = 80) -> int: ...
        @overload
        def overloaded(value: str, *, width: int = 80) -> str: ...
        @observed
        def overloaded(value: int | str, *, width: int = 80) -> int | str:
            return value

        overloaded(1, width=80)

        def observer(**keywords: int) -> int:
            return len(keywords)

        def reassigned() -> int:
            alias = plain
            alias = observer
            return alias(width=80)
    """),
    final_methods=dedent("""\
        from typing import final, overload, override

        class Base:
            def changed(self, *, width: int = 80) -> int:
                return width

            @final
            def fixed(self, *, width: int = 80) -> int:
                return width

        class Child(Base):
            @override
            def changed(self, **keywords: int) -> int:
                return len(keywords)

        @final
        class FinalClass:
            @staticmethod
            def static(*, width: int = 80) -> int:
                return width

            @classmethod
            def classed(cls, *, width: int = 80) -> int:
                return width

            @overload
            def framed(self, value: int, *, width: int = 80) -> int: ...
            @overload
            def framed(self, value: str, *, width: int = 80) -> str: ...
            def framed(self, value: int | str, *, width: int = 90) -> int | str:
                return value

        def invoked(base: Base) -> tuple[int, ...]:
            static = FinalClass.static
            classed = FinalClass.classed
            return (
                base.changed(width=80),
                base.fixed(width=80),
                static(width=80),
                classed(width=80),
                FinalClass.static(width=80),
                FinalClass.classed(width=80),
                FinalClass().framed(1, width=80),
                FinalClass().framed(1, width=90),
            )

        observed = invoked(Child())
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "case, lines",
    [
        ("function_and_field", [10, 11]),
        ("other_module_and_new_value", []),
        ("aliases", [16, 18]),
        ("template_string", [4]),
        ("positional_only", []),
        ("struct_overrides", [19, 19]),
        ("overloads", [11]),
        ("generic_struct", [7]),
        ("wrappers", [10]),
        ("final_methods", [38, 39, 40, 41, 42, 44]),
    ],
)
def test_a_restated_literal_default_reports(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """Each keyword argument equal to the literal default its resolved declaration states reports at its value, and a changed value, a non-literal default, another module's default, an inherited default replaced, a keyword a wrapper or override observes, and a variadic keyword pass."""
    assert [error.line for error in findings[case]] == lines
