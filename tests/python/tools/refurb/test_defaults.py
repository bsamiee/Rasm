"""Keyword arguments restating literal defaults."""

from textwrap import dedent

lazy from rasm_refurb.defaults import ErrorRestatedDefaultError
lazy from tools.refurb.conftest import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_restated_function_and_field_defaults_report(findings: Findings) -> None:
    """Keyword arguments equal to a same-module function default and a struct field default, both reported."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorRestatedDefaultError)] == [11, 12]


def test_other_module_defaults_and_new_values_pass(findings: Findings) -> None:
    """Another module's default and a differing value, not reported."""
    source = dedent("""
        import textwrap

        def framed(text: str, *, width: int = 80) -> str:
            return text.center(width)

        framed("title", width=60)
        textwrap.shorten("title text", 5, placeholder=" [...]")
    """)
    assert findings(source, ErrorRestatedDefaultError) == ()


def test_aliased_function_method_and_struct_defaults_report(findings: Findings) -> None:
    """Resolved function and struct aliases retain declared defaults."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorRestatedDefaultError)] == [17, 19]


def test_positional_only_names_in_keywords_pass(findings: Findings) -> None:
    """Keywords named after positional-only parameters belong to variadic keywords."""
    source = dedent("""
        def framed(width: int = 80, /, **keywords: int) -> int:
            return width + keywords["width"]

        framed(width=80)
    """)
    assert findings(source, ErrorRestatedDefaultError) == ()


def test_struct_overrides_replace_inherited_defaults(findings: Findings) -> None:
    """Required fields, factories, and changed defaults replace inherited values."""
    source = dedent("""
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
    """)
    assert [(error.line, error.column) for error in findings(source, ErrorRestatedDefaultError)] == [(20, 14), (20, 24)]


def test_overloads_use_implementation_defaults(findings: Findings) -> None:
    """Overload declarations leave runtime defaults to their implementation."""
    source = dedent("""
        from typing import overload

        @overload
        def framed(value: int, *, width: int = 80) -> int: ...
        @overload
        def framed(value: str, *, width: int = 80) -> str: ...
        def framed(value: int | str, *, width: int = 90) -> int | str:
            return value

        framed(1, width=80)
        framed("title", width=90)
    """)
    assert [error.line for error in findings(source, ErrorRestatedDefaultError)] == [12]


def test_generic_struct_constructor_defaults_report(findings: Findings) -> None:
    """Type applications preserve struct defaults."""
    source = dedent("""
        import msgspec

        class Frame[T](msgspec.Struct, kw_only=True):
            value: T
            width: int = 80

        Frame[int](value=1, width=80)
    """)
    assert [error.line for error in findings(source, ErrorRestatedDefaultError)] == [8]


def test_wrappers_keep_observable_keyword_presence(findings: Findings) -> None:
    """Wrapped calls retain observable keyword presence while ordinary calls inside a wrapper still report."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorRestatedDefaultError)] == [11]


def test_native_final_facts_bound_method_default_replacements(findings: Findings) -> None:
    """Final methods and classes retain native decorator defaults, and compatible overrides retain explicit keyword presence."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorRestatedDefaultError)] == [39, 40, 41, 42, 43, 45]
