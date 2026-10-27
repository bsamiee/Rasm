"""Output name grammars, root containment, and versioned paths."""

from collections.abc import Callable, Mapping
from enum import auto, StrEnum
import functools
from itertools import chain, starmap
from pathlib import Path
import re
import string
from typing import Annotated, Final, Self

from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case
from blender.rendering.faults import RenderingReason

# --- [TYPES] ----------------------------------------------------------------------------

type Field = Slot | Number
type _Parts = Text | Field
type _Width = Annotated[int, msgspec.Meta(ge=1, le=9)]


class Word(StrEnum):
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


class Count(StrEnum):
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


class Slot(Case, frozen=True, kw_only=True):
    """Text field."""

    key: Word


class Number(Case, frozen=True, kw_only=True):
    """Number field zero-padded to width."""

    key: Count
    width: _Width


class Fields(msgspec.Struct, frozen=True, kw_only=True):
    """Output name field values."""

    words: Mapping[Word, str] = msgspec.field(default_factory=dict)
    counts: Mapping[Count, Annotated[int, msgspec.Meta(ge=0)]] = msgspec.field(default_factory=dict)


class OutputName(msgspec.Struct, frozen=True, kw_only=True):
    """Literal text and fields in path order."""

    parts: tuple[_Parts, ...]

    @classmethod
    def of(cls, template: str) -> Outcome[Self]:
        """Parses `{word}` and `{count:width}` fields, refusing braces in text, unknown fields, unseparated fields, and one count at two widths."""
        refused = functools.partial(Fault.of, RenderingReason.TEMPLATE, template)

        def width(spec: str) -> Option[int]:
            return catch(msgspec.json.decode, exception=msgspec.DecodeError)(spec, type=_Width).to_option() if spec.isdigit() else Nothing

        def parted(text: str, name: str | None, spec: str | None, conversion: str | None) -> Outcome[tuple[_Parts, ...]]:
            literal: tuple[_Parts, ...] = (Text(text=text),) if text else ()
            match name, spec, conversion:
                case _ if _UNLITERAL.search(text):
                    return Error(refused(f"Literal {text!r} holds a brace or NUL"))
                case None, _, _:
                    return Ok(literal)
                case str(), "", None if name in Word:
                    return Ok((*literal, Slot(key=Word(name))))
                case str(), str(), None if name in Count:
                    return width(spec).map(lambda held: (*literal, Number(key=Count(name), width=held))).to_result_with(lambda: refused(f"Field {name!r} takes a width digit, got {spec!r}"))
                case _:
                    return Error(refused(f"Field {name!r} with conversion {conversion!r} and width {spec!r} names no output field"))

        def parsed(pieces: tuple[tuple[str, str | None, str | None, str | None], ...]) -> Outcome[Self]:
            return paired(
                accumulated(starmap(parted, pieces)),
                Ok(pieces[1:]).filter_with(
                    lambda later: all(_SEPARATOR.search(text) for text, name, _, _ in later if name is not None), lambda _: refused("Adjacent fields need a non-word or underscore separator")
                ),
                Ok({(name, spec) for _, name, spec, _ in pieces if name is not None and name in Count}).filter_with(
                    lambda counts: len(counts) == len({name for name, _ in counts}), lambda _: refused("A count field repeats at another width")
                ),
            ).map(lambda found: cls(parts=tuple(chain.from_iterable(found[0]))))

        return catch(tuple, exception=ValueError)(string.Formatter().parse(template)).map_error(refused).bind(parsed)

    @property
    def slots(self) -> frozenset[Field]:
        """Text and number fields in template."""
        return frozenset(part for part in self.parts if not isinstance(part, Text))

    def patterns(self) -> dict[Field, re.Pattern[str]]:
        """Returns value pattern per field, excluding template separators."""
        excluded = re.escape("".join(dict.fromkeys((*[char for part in self.parts if isinstance(part, Text) for char in part.text if not char.isalnum()], "/", "\\"))))

        def pattern(part: Field) -> re.Pattern[str]:
            match part:
                case Slot(key=Word.OUTPUT):
                    return re.compile("|".join(Output))
                case Slot(key=Word.DATE):
                    return re.compile(r"[0-9]{4}-[0-9]{2}-[0-9]{2}")
                case Slot():
                    return re.compile(rf"[^{RESERVED}{excluded}]+")
                case Number(width=width):
                    return re.compile(rf"[0-9]{{{width}}}|[1-9][0-9]{{{width},}}")

        return {part: pattern(part) for part in self.slots}

    def compose(self, fields: Fields, written: Callable[[Field], Option[str]] = lambda _: Nothing) -> Outcome[str]:
        """Composes name, accumulating field refusals."""
        patterns = self.patterns()

        def spelled(part: _Parts) -> Outcome[str]:
            match part:
                case Text(text=text):
                    return Ok(text)
                case Slot(key=key):
                    value = Option.of_optional(fields.words.get(key))
                case Number(key=key, width=width):
                    value = Option.of_optional(fields.counts.get(key)).map(lambda count: f"{count:0{width}d}")
            pattern = patterns[part]
            unmatched = Fault.of(RenderingReason.UNMATCHED, value.default_value(f"{{{key}}}"), member=pattern.pattern)
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
                    words={part.key: found[part.key] for part in self.slots if isinstance(part, Slot)}, counts={part.key: int(found[part.key]) for part in self.slots if isinstance(part, Number)}
                )
            )
            .to_result_with(functools.partial(Fault.of, RenderingReason.UNMATCHED, text, member=grammar.pattern))
        )

    def path(self, root: Path, fields: Fields) -> Outcome[Path]:
        """Composes path, refusing one resolving outside root."""
        return (
            self
            .compose(fields)
            .map(root.joinpath)
            .filter_with(lambda path: path.resolve().is_relative_to(root.resolve()), lambda path: Fault.of(RenderingReason.ROOT, str(path.resolve()), member=str(root.resolve())))
        )

    def next_version(self, root: Path, fields: Fields) -> int:
        """Returns next version past matching files under root, ignoring version and frame."""
        held = {key: count for key, count in fields.counts.items() if key not in {Count.VERSION, Count.FRAME}}
        return 1 + max(
            (
                name.counts[Count.VERSION]
                for file in root.rglob("*")
                if file.is_file()
                for name in self.parse(file.relative_to(root).with_suffix("").as_posix()).to_option().to_list()
                if Count.VERSION in name.counts and fields.words.items() <= name.words.items() and held.items() <= name.counts.items()
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
        Option
        .of_optional(next(reversed(_VERSION.findall(text)), None))
        .map(int)
        .filter(lambda last: last + offset >= 0)
        .map(lambda last: _VERSION.sub(lambda found: found[0].removesuffix(found[1]) + f"{last + offset:0{len(found[1])}d}" if int(found[1]) == last else found[0], text))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RESERVED", "Count", "Field", "Fields", "Number", "Output", "OutputName", "Slot", "Text", "Word", "stepped"]
