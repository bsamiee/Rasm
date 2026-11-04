"""Native replacements retain proved subcases and reject changes to values or bindings."""

import string
from textwrap import dedent
lazy from collections import defaultdict

from hypothesis import example, given, strategies
import pytest
from refurb.checks.builtin.no_ignored_enumerate import ErrorInfo as EnumerateError
from refurb.checks.builtin.no_slice_copy import ErrorInfo as SliceError
from refurb.checks.builtin.writelines import ErrorInfo as WriteError
from refurb.checks.flow.no_with_assign import ErrorInfo as WithError
from refurb.checks.iterable.implicit_readlines import ErrorInfo as ReadlinesError
from refurb.checks.readability.no_copy_with_merge import ErrorInfo as MergeError
from refurb.checks.readability.no_len_cmp import ErrorInfo as LengthError
from refurb.checks.readability.no_or_default import ErrorInfo as DefaultError
from refurb.checks.readability.no_temp_class_object import ErrorInfo as ConstructorError
from refurb.checks.readability.use_isinstance_bool import ErrorInfo as BoolError
from refurb.checks.secrets.simplify_token_function import ErrorInfo as TokenError
from refurb.checks.shlex.use_join import ErrorInfo as JoinError
from refurb.checks.string.no_multiline_lstrip import ErrorInfo as NewlineError
from refurb.checks.string.simplify_strip import ErrorInfo as StripError
from refurb.checks.string.startswith import ErrorInfo as PrefixError
from refurb.checks.third_party.fastapi.simplify_query import ErrorInfo as QueryError
lazy from mypy.nodes import ArgKind, CallExpr, MemberExpr, StrExpr
lazy from refurb.checks.string import no_multiline_lstrip
lazy from refurb.error import Error
lazy from refurb.settings import Settings

lazy from tests.python.tools.refurb.conftest import Findings
lazy from tools.refurb.rewrites import RewriteVisitor

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_prefix_replacements_preserve_evaluation(findings: Findings) -> None:
    """Stable values report while call receivers and eager prefix calls retain evaluation order."""
    source = dedent("""
        from itertools import cycle

        texts = cycle(('ba', 'a'))

        def fetched() -> str:
            return next(texts)

        def matched(text: str, prefix: tuple[str]) -> tuple[bool, ...]:
            return (
                'ab'.startswith('a') or 'ab'.startswith('b'),
                text.startswith('a') or text.startswith('b'),
                fetched().startswith('a') or fetched().startswith('b'),
                'ab'.startswith('a') or 'ab'.startswith(fetched()),
                'ab'.startswith('a') or 'ab'.startswith('' * (1 // 0)),
                'ab'.startswith(*prefix) or 'ab'.startswith('b'),
                'ab'.startswith('a') or 'ab'.startswith(prefix),
            )
    """)
    assert [error.line for error in findings(PrefixError, source)] == [11]


def test_container_replacements_preserve_values_and_consumption(findings: Findings) -> None:
    """Exact builtins report while iterator consumption, subclass truthiness and copying, and unpacking remain explicit."""
    source = dedent("""
        from collections.abc import Iterator
        from typing import override

        class Counted(list[int]):
            @override
            def __len__(self) -> int:
                return 0

            def __bool__(self) -> bool:
                return True

            @override
            def copy(self) -> list[int]:
                return []

        def empty(iterator: Iterator[int], values: list[int], positions: range) -> list[int]:
            return [
                value for value in values
                if len(list(iterator)) == 0
                if len(values) == 0
                if len([]) == 0
                if len(()) == 0
                if len(frozendict(a=1)) == 0
                if len(positions) == 0
                if len(range(10**100)) == 0
                if len(*([],)) == 0
                if len(list(*([],))) == 0
                if len(Counted()) == 0
            ]

        def copied(values: list[int], raw: bytearray, fixed: tuple[int, ...]) -> tuple[list[int], bytearray, tuple[int, ...]]:
            return values[:], raw[:], fixed[:]

        constructed = [1, 2][:], bytearray(b'ab')[:]
        observed = copied(Counted([1, 2]), bytearray(b'ab'), (1, 2))
    """)
    assert [error.line for error in findings(LengthError, source)] == [22, 23, 24]
    assert [(error.line, error.column) for error in findings(SliceError, source)] == [(35, 14), (35, 25)]


def test_loop_replacements_keep_observed_bindings(findings: Findings) -> None:
    """Loop-to-method rewrites retain targets referenced outside their loops."""
    source = dedent("""
        from io import StringIO
        from pathlib import Path

        def write() -> str:
            with StringIO() as handle:
                for line in ('a', 'b'):
                    handle.write(line)
            return line

        def write_all(path: Path, lines: list[str]) -> None:
            with path.open('w') as handle:
                for line in lines:
                    handle.write(line)

        def unpacked() -> None:
            with StringIO() as handle:
                for line in ('ab', 'cd'):
                    handle.write(*line)

        def buffered() -> None:
            with StringIO() as handle:
                for line in ('a', 'b'):
                    handle.write(line)

        with StringIO() as handle:
            for published_line in ('a', 'b'):
                handle.write(published_line)

        class Namespace:
            with StringIO() as handle:
                for class_line in ('a', 'b'):
                    handle.write(class_line)
    """)
    assert [error.line for error in findings(WriteError, source)] == [23]


def test_assignment_replacements_require_nonsuppressing_contexts(findings: Findings) -> None:
    """Nonsuppressing contexts report while suppressors and reads of the initial value retain it."""
    source = dedent("""
        from collections.abc import Iterator
        from contextlib import nullcontext, suppress

        first = 'initial'
        with nullcontext():
            first = 'new'

        second = 'initial'
        with suppress(ValueError):
            second = str(int('invalid'))

        third = 'initial'
        with nullcontext():
            third = third.upper()

        class Observing:
            def __enter__(self) -> None:
                print(observed)

            def __exit__(self, *cause: object) -> None:
                pass

        observed = 'initial'
        with Observing():
            observed = 'new'

        def seen() -> Iterator[None]:
            print(expanded)
            yield None

        arguments = seen()
        expanded = 'initial'
        with nullcontext(*arguments):
            expanded = 'new'
    """)
    assert [error.line for error in findings(WithError, source)] == [5]


def test_readlines_replacements_require_fresh_buffers(findings: Findings) -> None:
    """Fresh native buffers report while opaque streams retain their eager line snapshots."""
    source = dedent("""
        from io import StringIO, TextIOWrapper

        def read(handle: TextIOWrapper) -> None:
            for line in handle.readlines():
                last = line

        def buffered() -> None:
            for line in StringIO('a\\nb').readlines():
                last = line
    """)
    assert [error.line for error in findings(ReadlinesError, source)] == [9]


def test_defaults_preserve_mutable_aliases_and_signed_zero(findings: Findings) -> None:
    """Canonical booleans report while mutable aliases, signed zero, and distinct empty objects retain their expressions."""
    source = dedent("""
        def defaults(values: list[int], number: float, text: str, flag: bool) -> tuple[list[int], float, str, bool]:
            return values or [], number or 0.0, text or '', flag or False

        empty: frozenset[int] = frozenset()
        distinct_empty = empty or frozenset()
    """)
    assert [(error.line, error.column) for error in findings(DefaultError, source)] == [(3, 52)]


def test_enumerate_replacements_preserve_mutable_iteration(findings: Findings) -> None:
    """Primitive source values and native local bindings bound replacements without changing iteration or lifetimes."""
    source = dedent("""
        def mutable(values: list[int]) -> None:
            for index, _ in enumerate(values):
                values.append(index)

        def immutable(values: tuple[int, ...]) -> None:
            for index, _ in enumerate(values):
                print(index)

        def constructed() -> None:
            for index, _ in enumerate((1, 2)):
                print(index)

        def unused(values: list[int]) -> None:
            for index, value in enumerate(values):
                print(value)

        def observed_else() -> None:
            for index, value in enumerate((1, 2)):
                pass
            else:
                print(index, value)

        def unused_else() -> None:
            for index, value in enumerate(b'ab'):
                print(value)
            else:
                print('done')

        def unpacked(groups: tuple[list[int]]) -> tuple[int, ...]:
            for index, value in enumerate(*groups):
                print(value)
            return tuple(value for index, value in enumerate(*groups))

        for module_index, published in enumerate((1, 2)):
            print(published)

        class Namespace:
            for class_index, class_value in enumerate((1, 2)):
                print(class_value)

        def mixed() -> None:
            global published
            for published, local_value in enumerate((1, 2)):
                pass

        def comprehensions(values: tuple[int, ...]) -> tuple[list[int], tuple[int, ...], dict[int, int], set[int]]:
            return (
                [value for index, value in enumerate(values)],
                tuple(index for index, value in enumerate((1, 2))),
                {index: value for index, value in enumerate(values) if index},
                {index for index, value in enumerate(values) if value},
            )

        def shadowed() -> None:
            for index, value in enumerate({1, 2}):
                def consume(index: int) -> int:
                    return index + 1
                print(consume(value))

        def captured() -> None:
            for index, value in enumerate((1, 2)):
                def consume() -> int:
                    return index + 1
                print(consume() + value)

        class Observed:
            def __del__(self) -> None:
                print('released')

        def retained() -> None:
            for index, value in enumerate((Observed(),)):
                print(index)
            for position, element in enumerate(range(10**100)):
                print(position)

        def rebound() -> None:
            for index, value in enumerate(Observed() for _ in (0,)):
                del value
                print('body')
    """)
    assert [error.line for error in findings(EnumerateError, source)] == [11, 25, 44, 49, 50, 56]


def test_constructor_replacements_preserve_initialization(findings: Findings) -> None:
    """Plain object allocation reports while initialization, destruction, and descriptor binding retain their effects."""
    source = dedent("""
        class Plain:
            @staticmethod
            def value() -> int:
                return 1

        class Initialized:
            def __init__(self) -> None:
                print('created')

            @staticmethod
            def value() -> int:
                return 1

        class Released:
            def __del__(self) -> None:
                print('released')

            @staticmethod
            def value() -> int:
                return 1

        plain = Plain().value()
        initialized = Initialized().value()
        released = Released().value()

        from collections.abc import Callable

        class Descriptor:
            def __get__(self, instance: object | None, owner: type[object]) -> Callable[[], bool]:
                return lambda: instance is None

        def described(function: Callable[[], bool]) -> Descriptor:
            return Descriptor()

        class Wrapped:
            @described
            @staticmethod
            def value() -> bool:
                return True

        decorated = Wrapped().value()
    """)
    assert [error.line for error in findings(ConstructorError, source)] == [23]


def test_merge_replacements_keep_left_snapshot(findings: Findings) -> None:
    """Exact copies report while nominal receivers and right-side mutation retain their snapshots."""
    source = dedent("""
        values: dict[str, int] = {'a': 1}

        def changed() -> dict[str, int]:
            values['a'] = 2
            return {}

        left_snapshot = values.copy() | changed()
        right_snapshot = changed() | values.copy()
        constructed = dict(a=1).copy() | {'b': 2}
        constructed_right = changed() | {'b': 2}.copy()
    """)
    assert [error.line for error in findings(MergeError, source)] == [10, 11]


def test_newline_replacements_reach_each_native_method(findings: Findings) -> None:
    """The compiler and public visitor retain each safe multiline strip, lstrip, and rstrip replacement."""
    source = 'both = """\nx\n""".strip()\nleading = """\nx  """.lstrip()\ntrailing = """  x\n""".rstrip()\n'
    assert [(error.line, error.column) for error in findings(NewlineError, source)] == [(1, 7), (4, 10), (6, 11)]


@pytest.mark.parametrize("chars", [None, "\n"])
@example(value="\n x\n")
@example(value="\nx  ")
@example(value="\nx\n\n")
@example(value="  x\n")
@given(value=strategies.text(alphabet=strategies.one_of(strategies.sampled_from(string.whitespace), strategies.characters(categories=("Zs", "Zl", "Zp")), strategies.characters())))
def test_newline_rewrites_preserve_unicode_whitespace(value: str, chars: str | None) -> None:
    """Native suggestions survive exactly when their boundary edits preserve stripping across Unicode whitespace and repeated newlines."""
    for strip in (str.lstrip, str.rstrip, str.strip):
        node = CallExpr(MemberExpr(StrExpr(value), strip.__name__), [StrExpr(chars)] if chars is not None else [], [ArgKind.ARG_POS] if chars is not None else [], [])
        node.set_line(1, 0, end_line=2)
        proposed: list[Error] = []
        no_multiline_lstrip.check(node, proposed)
        visitor = RewriteVisitor(defaultdict(list), Settings())
        visitor.run_check(node, no_multiline_lstrip.check)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        leading = strip is not str.rstrip and value.startswith("\n") and len(value) > 1 and not value[1].isspace()
        trailing = strip is not str.lstrip and value.endswith("\n") and len(value) > 1 and not value[-2].isspace()
        replacement = value[int(leading) : len(value) - int(trailing)]
        assert visitor.errors == (proposed if strip(value, chars) == replacement else [])


def test_strip_replacements_preserve_sequential_removal(findings: Findings) -> None:
    """Exact strings retain nested reductions while nominal receivers and independent same-side removals remain sequential."""
    source = dedent("""
        from typing import override

        class Text(str):
            @override
            def lstrip(self, chars: str | None = None, /) -> str:
                return 'custom'

        def stripped(text: str) -> tuple[str, ...]:
            return (
                ' -x- '.lstrip(' ').lstrip('-'),
                ' -x- '.lstrip(' -').lstrip('-'),
                ' -x- '.lstrip('-').lstrip(' -'),
                ' -x- '.lstrip(' ').rstrip(' '),
                ' -x- '.lstrip().rstrip(),
                text.lstrip().rstrip(),
                ' -x- '.lstrip(*' -').rstrip(' -'),
                ' -x- '.lstrip(' -').rstrip(*' -'),
            )

        observed = stripped(Text(' -x- '))
    """)
    assert [error.line for error in findings(StripError, source)] == [12, 13, 14, 15]


def test_token_conversion_preserves_generation_length(findings: Findings) -> None:
    """Hex conversion reports without replacing a slice with a longer random output."""
    source = dedent("""
        from secrets import token_bytes, token_hex

        converted = token_bytes().hex()
        bytes_slice = token_bytes()[:100]
        hex_slice = token_hex()[:100]
    """)
    assert [error.line for error in findings(TokenError, source)] == [4]


def test_join_replacements_preserve_generator_bindings(findings: Findings) -> None:
    """Outer names stay in generators while element and transformed-element joins report."""
    source = dedent("""
        import shlex

        def render(prefix: str, arguments: list[str]) -> tuple[str, str, str, str]:
            return (
                " ".join(shlex.quote(prefix) for value in arguments),
                " ".join(shlex.quote(value) for value in arguments),
                " ".join(shlex.quote(prefix + value) for value in arguments),
                " ".join(shlex.quote(prefix) for value in arguments if value),
            )
    """)
    assert [error.line for error in findings(JoinError, source)] == [7, 8, 9]


def test_join_replacements_keep_destructuring(findings: Findings) -> None:
    """Destructuring keeps the selected string instead of passing tuples to shlex.join."""
    source = dedent("""
        import shlex

        def render(pairs: list[tuple[str, str]]) -> str:
            return " ".join(shlex.quote(first) for first, second in pairs)
    """)
    assert findings(JoinError, source) == ()


def test_bool_replacements_preserve_equality_and_evaluation(findings: Findings) -> None:
    """Identity tests and known booleans report without changing integer equality or repeated calls."""
    source = dedent("""
        from itertools import cycle

        flags = cycle((False, True))

        def next_flag() -> bool:
            return next(flags)

        def tested(value: object, flag: bool) -> tuple[bool, ...]:
            return (
                value in {True, False},
                value == True or value == False,
                value is True or value is False,
                flag in {True, False},
                next_flag() is True or next_flag() is False,
            )
    """)
    assert [error.line for error in findings(BoolError, source)] == [13, 14]


def test_query_replacements_keep_scalar_parameters(findings: Findings) -> None:
    """Scalar, literal, enum, and optional arguments report beside list and unresolved custom arguments."""
    source = dedent("""
        from enum import Enum, auto
        from typing import Literal
        from fastapi import Query

        class Choice(Enum):
            OPEN = auto()

        class Value:
            pass

        def route(
            text: str = Query(),
            count: int | None = Query(None),
            choice: Choice = Query(Choice.OPEN),
            token: Literal['x'] = Query('x'),
            tupled: str = Query(*('x',)),
            items: list[str] = Query([]),
            custom: Value = Query(),
        ) -> None:
            pass
    """)
    assert [error.line for error in findings(QueryError, source)] == [13, 14, 15, 16]
