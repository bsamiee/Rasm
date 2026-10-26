"""Output name grammars, root containment, and versioned paths."""

from collections.abc import Callable
from enum import auto, StrEnum
from itertools import chain, starmap
from pathlib import Path
import re
import string
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case
from blender.rendering.faults import RenderingFaults, RenderingNameFault, RenderingResult, RenderingRootFault

# --- [TYPES] ----------------------------------------------------------------------------

type Field = Slot | Number
type Parts = Text | Field
type Width = Annotated[int, msgspec.Meta(ge=1, le=9)]


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

_RESERVED = r'\x00-\x1f"*:<>?|'
_UNLITERAL = re.compile(r"[{}\x00]")
_SEPARATOR = re.compile(r"[\W_]")

# --- [MODELS] ---------------------------------------------------------------------------


class Text(Case, frozen=True, kw_only=True, tag=1):
    """Literal text."""

    text: str


class Slot(Case, frozen=True, kw_only=True, tag=2):
    """Text field."""

    title = "Text Field"
    key: Word


class Number(Case, frozen=True, kw_only=True, tag=3):
    """Number field zero-padded to width."""

    title = "Number Field"
    key: Count
    width: Width


class Fields(msgspec.Struct, frozen=True, kw_only=True):
    """Output name field values."""

    words: dict[Word, str] = msgspec.field(default_factory=dict)
    counts: dict[Count, Annotated[int, msgspec.Meta(ge=0)]] = msgspec.field(default_factory=dict)


class OutputName(msgspec.Struct, frozen=True, kw_only=True):
    """Literal text and fields in path order."""

    parts: tuple[Parts, ...]

    @classmethod
    def of(cls, template: str) -> RenderingResult[Self]:
        """Parses `{word}` and `{count:width}` fields, refusing braces in text, unknown fields, unseparated fields, and one count at two widths."""

        def refused(reason: str) -> RenderingFaults:
            return (RenderingNameFault(request=template, error=ValueError(reason)),)

        def width(spec: str) -> Option[int]:
            return catch(lambda: msgspec.json.decode(spec, type=Width), exception=msgspec.DecodeError)().to_option() if spec.isdigit() else Nothing

        def parted(text: str, name: str | None, spec: str | None, conversion: str | None) -> RenderingResult[tuple[Parts, ...]]:
            literal: tuple[Parts, ...] = (Text(text=text),) if text else ()
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

        def checked(reason: str, *, holds: bool) -> RenderingResult[None]:
            return Ok(None) if holds else Error(refused(reason))

        try:
            pieces = tuple(string.Formatter().parse(template))
        except ValueError as error:
            return Error((RenderingNameFault(request=template, error=error),))
        counts = {(name, spec) for _, name, spec, _ in pieces if name is not None and name in Count}
        return paired(
            accumulated(starmap(parted, pieces)),
            checked("Adjacent fields need a non-word or underscore separator", holds=all(_SEPARATOR.search(text) for text, name, _, _ in pieces[1:] if name is not None)),
            checked("A count field repeats at another width", holds=len(counts) == len({name for name, _ in counts})),
        ).map(lambda found: cls(parts=tuple(chain.from_iterable(found[0]))))

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
                    return re.compile(rf"[^{_RESERVED}{excluded}]+")
                case Number(width=width):
                    return re.compile(rf"[0-9]{{{width}}}|[1-9][0-9]{{{width},}}")

        return {part: pattern(part) for part in self.parts if not isinstance(part, Text)}

    def compose(self, fields: Fields, written: Callable[[Field], Option[str]] = lambda _: Nothing) -> RenderingResult[str]:
        """Composes name, accumulating field refusals."""
        patterns = self.patterns()

        def spelled(part: Parts) -> RenderingResult[str]:
            match part:
                case Text(text=text):
                    return Ok(text)
                case Slot(key=key):
                    value = fields.words.get(key, "")
                case Number(key=key, width=width):
                    value = Option.of_optional(fields.counts.get(key)).map(lambda count: f"{count:0{width}d}").default_value("")
            pattern = patterns[part]
            return written(part).map(Ok).default_with(lambda: Ok(value) if pattern.fullmatch(value) else Error((RenderingNameFault(request=value, error=pattern),)))

        return accumulated(map(spelled, self.parts)).map("".join)

    def parse(self, text: str) -> RenderingResult[Fields]:
        """Parses relative extensionless name, requiring repeated fields to agree."""
        patterns = self.patterns()
        names = {part: f"{type(part).__name__}_{part.key.name}" for part in patterns}
        first = {part: index for index, part in reversed(tuple(enumerate(self.parts))) if not isinstance(part, Text)}
        grammar = re.compile(
            "".join(
                re.escape(part.text) if isinstance(part, Text) else f"(?P<{names[part]}>{patterns[part].pattern})" if first[part] == index else f"(?P={names[part]})"
                for index, part in enumerate(self.parts)
            )
        )
        return (
            Option
            .of_optional(grammar.fullmatch(text))
            .map(
                lambda found: Fields(
                    words={part.key: found[names[part]] for part in patterns if isinstance(part, Slot)}, counts={part.key: int(found[names[part]]) for part in patterns if isinstance(part, Number)}
                )
            )
            .to_result_with(lambda: (RenderingNameFault(request=text, error=grammar),))
        )

    def path(self, root: Path, fields: Fields) -> RenderingResult[Path]:
        """Composes path, refusing one resolving outside root."""
        return self.compose(fields).bind(
            lambda text: Ok(root / text) if (path := (root / text).resolve()).is_relative_to(base := root.resolve()) else Error((RenderingRootFault(request=path, error=base),))
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


def stepped(text: str, offset: int) -> Option[str]:
    """Steps version tokens matching last version, keeping token widths."""
    token = re.compile(r"(?i)(?<![^\W\d_])(?:version|v)[._]?(\d+)")
    return (
        Option
        .of_optional(next(reversed(token.findall(text)), None))
        .map(int)
        .filter(lambda last: last + offset >= 0)
        .map(lambda last: token.sub(lambda found: found[0].removesuffix(found[1]) + f"{last + offset:0{len(found[1])}d}" if int(found[1]) == last else found[0], text))
    )


def stored_name(name: str) -> str:
    """Replaces characters Blender reserves in File Output item names."""
    return re.sub(f"[{_RESERVED}]", "_", name)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Count", "Field", "Fields", "Number", "Output", "OutputName", "Parts", "Slot", "Text", "Width", "Word", "stepped", "stored_name"]
