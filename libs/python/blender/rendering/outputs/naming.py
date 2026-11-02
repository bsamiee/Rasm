"""Output name grammars, root containment, and versioned paths."""

from collections.abc import Callable, Mapping
from enum import auto, StrEnum
import functools
from itertools import chain, starmap
from pathlib import Path
import re
import string
from types import MappingProxyType
from typing import Annotated, Final, Self

from expression import Error, Nothing, Ok, Option
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Field = TextField | NumberField
type _Part = Text | Field
type _Width = Annotated[int, msgspec.Meta(ge=1, le=9)]


class TextKey(StrEnum):
    """Text field of output name, valued by its template token."""

    PROJECT = auto()
    SHOT = auto()
    SCENE = auto()
    LAYER = auto()
    CAMERA = auto()
    PASS = auto()
    OUTPUT = auto()
    FILE = auto()
    DATE = auto()


class NumberKey(StrEnum):
    """Number field of output name, valued by its template token."""

    VERSION = auto()
    FRAME = auto()
    ORDER = auto()


class Output(StrEnum):
    """Output class with own file format, valued by its output field text."""

    BEAUTY = auto()
    DATA = auto()
    MATTE = auto()
    PREVIEW = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

RESERVED: Final = r'\x00-\x1f"*:<>?|'
_UNLITERAL = re.compile(r"[{}\x00]")
_SEPARATOR = re.compile(r"[\W_]")
_VERSION = re.compile(r"(?i)(?<![^\W\d_])(?:version|v)[._]?(\d+)")

# --- [MODELS] ---------------------------------------------------------------------------


class Text(Case, frozen=True, kw_only=True):
    """Literal text."""

    text: str


class TextField(Case, frozen=True, kw_only=True):
    """Text field."""

    key: TextKey


class NumberField(Case, frozen=True, kw_only=True):
    """Number field zero-padded to width."""

    key: NumberKey
    width: _Width


class Fields(msgspec.Struct, frozen=True, kw_only=True):
    """Output name field values."""

    texts: Mapping[TextKey, str] = MappingProxyType({})
    numbers: Mapping[NumberKey, int] = MappingProxyType({})


class OutputName(msgspec.Struct, frozen=True, kw_only=True):
    """Literal text and fields in path order."""

    parts: tuple[_Part, ...]

    @classmethod
    def of(cls, template: str) -> Outcome[Self]:
        """Parses `{word}` and `{count:width}` fields, refusing braces in text, unknown fields, unseparated fields, and one count at two widths."""
        refused = functools.partial(Fault.of, Reason.INVALID, template)

        def width(spec: str) -> Option[int]:
            return catch(msgspec.json.decode, exception=msgspec.DecodeError)(spec, type=_Width).to_option() if spec.isdigit() else Nothing

        def parted(text: str, name: str | None, spec: str | None, conversion: str | None) -> Outcome[tuple[_Part, ...]]:
            literal: tuple[_Part, ...] = (Text(text=text),) if text else ()
            match name, spec, conversion:
                case _ if _UNLITERAL.search(text):
                    return Error(refused(f"Literal {text!r} holds a brace or NUL"))
                case None, _, _:
                    return Ok(literal)
                case str(), "", None if name in TextKey:
                    return Ok((*literal, TextField(key=TextKey(name))))
                case str(), str(), None if name in NumberKey:
                    return width(spec).map(lambda held: (*literal, NumberField(key=NumberKey(name), width=held))).to_result_with(lambda: refused(f"Field {name!r} takes a width digit, got {spec!r}"))
                case _:
                    return Error(refused(f"Field {name!r} with conversion {conversion!r} and width {spec!r} names no output field"))

        def parsed(pieces: tuple[tuple[str, str | None, str | None, str | None], ...]) -> Outcome[Self]:
            return paired(
                accumulated(starmap(parted, pieces)),
                Ok(pieces[1:]).filter_with(
                    lambda later: all(_SEPARATOR.search(text) for text, name, _, _ in later if name is not None), lambda _: refused("Adjacent fields need a non-word or underscore separator")
                ),
                Ok({(name, spec) for _, name, spec, _ in pieces if name is not None and name in NumberKey}).filter_with(
                    lambda counts: len(counts) == len({name for name, _ in counts}), lambda _: refused("A count field repeats at another width")
                ),
            ).map(lambda found: cls(parts=tuple(chain.from_iterable(found[0]))))

        return catch(tuple, exception=ValueError)(string.Formatter().parse(template)).map_error(refused).bind(parsed)

    @property
    def fields(self) -> frozenset[Field]:
        """Text and number fields in template."""
        return frozenset(part for part in self.parts if not isinstance(part, Text))

    @property
    def keys(self) -> frozenset[TextKey | NumberKey]:
        """Keys of the text and number fields in template."""
        return frozenset(part.key for part in self.fields)

    def patterns(self) -> dict[Field, re.Pattern[str]]:
        """Returns value pattern per field, excluding template separators."""
        excluded = re.escape("".join(dict.fromkeys((*[char for part in self.parts if isinstance(part, Text) for char in part.text if not char.isalnum()], "/", "\\"))))

        def pattern(part: Field) -> re.Pattern[str]:
            match part:
                case TextField(key=TextKey.OUTPUT):
                    return re.compile("|".join(Output))
                case TextField(key=TextKey.DATE):
                    return re.compile(r"[0-9]{4}-[0-9]{2}-[0-9]{2}")
                case TextField():
                    return re.compile(rf"[^{RESERVED}{excluded}]+")
                case NumberField(width=width):
                    return re.compile(rf"[0-9]{{{width}}}|[1-9][0-9]{{{width},}}")

        return {part: pattern(part) for part in self.fields}

    def compose(self, fields: Fields, written: Callable[[Field], Option[str]] = lambda _: Nothing) -> Outcome[str]:
        """Composes name, accumulating field refusals."""
        patterns = self.patterns()

        def spelled(part: _Part) -> Outcome[str]:
            match part:
                case Text(text=text):
                    return Ok(text)
                case TextField(key=key):
                    value = Option.of_optional(fields.texts.get(key))
                case NumberField(key=key, width=width):
                    value = Option.of_optional(fields.numbers.get(key)).map(lambda count: f"{count:0{width}d}")
            pattern = patterns[part]
            unmatched = Fault.of(Reason.OUTSIDE, f"{{{key}}}", value.default_value("no value"), member=pattern.pattern)
            return written(part).or_else(value.filter(lambda held: pattern.fullmatch(held) is not None)).to_result(unmatched)

        return accumulated(map(spelled, self.parts)).map("".join)

    def parse(self, text: str) -> Outcome[Fields]:
        """Parses relative extensionless name, requiring repeated fields to agree."""
        grammar = _grammar(self)
        return (
            Option
            .of_optional(grammar.fullmatch(text))
            .map(
                lambda found: Fields(
                    texts={part.key: found[part.key] for part in self.fields if isinstance(part, TextField)},
                    numbers={part.key: int(found[part.key]) for part in self.fields if isinstance(part, NumberField)},
                )
            )
            .to_result_with(functools.partial(Fault.of, Reason.OUTSIDE, type(self).__name__, text, member=grammar.pattern))
        )

    def path(self, root: Path, fields: Fields) -> Outcome[Path]:
        """Composes path, refusing one resolving outside root."""
        return (
            self
            .compose(fields)
            .map(root.joinpath)
            .filter_with(lambda path: path.resolve().is_relative_to(root.resolve()), lambda path: Fault.of(Reason.OUTSIDE, str(root.resolve()), str(path.resolve()), member="paths beneath it"))
        )

    def next_version(self, root: Path, fields: Fields) -> int:
        """Returns next version past matching files under root, ignoring version and frame."""
        held = {key: count for key, count in fields.numbers.items() if key not in {NumberKey.VERSION, NumberKey.FRAME}}
        return 1 + max(
            (
                name.numbers[NumberKey.VERSION]
                for file in root.rglob("*")
                if file.is_file()
                for name in self.parse(file.relative_to(root).with_suffix("").as_posix()).to_option().to_list()
                if NumberKey.VERSION in name.numbers and fields.texts.items() <= name.texts.items() and held.items() <= name.numbers.items()
            ),
            default=0,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def _grammar(name: OutputName) -> re.Pattern[str]:
    """Returns the pattern a name's files match, each repeated field matching its first value."""
    patterns = name.patterns()
    return re.compile(
        "".join(
            re.escape(part.text) if isinstance(part, Text) else f"(?P<{part.key}>{patterns[part].pattern})" if name.parts.index(part) == index else f"(?P={part.key})"
            for index, part in enumerate(name.parts)
        )
    )


def stepped(text: str, offset: int) -> Option[str]:
    """Steps version tokens matching last version, keeping token widths."""
    return (
        Block
        .of_seq(reversed(_VERSION.findall(text)))
        .try_head()
        .map(int)
        .filter(lambda last: last + offset >= 0)
        .map(lambda last: _VERSION.sub(lambda found: found[0].removesuffix(found[1]) + f"{last + offset:0{len(found[1])}d}" if int(found[1]) == last else found[0], text))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RESERVED", "Field", "Fields", "NumberField", "NumberKey", "Output", "OutputName", "Text", "TextField", "TextKey", "stepped"]
