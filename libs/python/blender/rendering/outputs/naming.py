"""Output name grammars, root containment, and versioned paths."""

from collections.abc import Callable
from pathlib import Path
import re
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option
import msgspec

from blender.imaging.faults import accumulated
from blender.imaging.parameters import Case, Choice, markers
from blender.rendering.faults import RenderingNameFault, RenderingResult, RenderingRootFault

# --- [TYPES] ----------------------------------------------------------------------------

type Field = Slot | Number
type Parts = Text | Field
type Width = Annotated[int, msgspec.Meta(ge=1, le=9)]


class Word(Choice):
    """Text field of output name."""

    PROJECT = 1
    SHOT = 2
    SCENE = 3
    LAYER = 4, "View Layer"
    CAMERA = 5
    PASS = 6
    OUTPUT = 7
    FILE = 8
    DATE = 9


class Count(Choice):
    """Number field of output name."""

    VERSION = 1
    FRAME = 2
    ORDER = 3


class Output(Choice):
    """Output class with own file format."""

    BEAUTY = 1
    DATA = 2
    MATTE = 3
    PREVIEW = 4

    @property
    def token(self) -> str:
        """Output field text."""
        return self.name.lower()


# --- [CONSTANTS] ------------------------------------------------------------------------

_RESERVED = r'\x00-\x1f"*:<>?|'


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
        """Parses template, refusing unknown fields, bad widths, and unseparated fields."""
        bounds = markers(Width, msgspec.Meta)[0]
        digits = f"[{bounds.ge}-{bounds.le}]"
        words, counts = (tuple(member.name.lower() for member in choice) for choice in (Word, Count))
        parts = tuple(
            Text(text=found["text"]) if found["text"] else Slot(key=Word[found["word"].upper()]) if found["word"] else Number(key=Count[found["count"].upper()], width=int(found["width"]))
            for found in re.finditer(rf"\{{(?:(?P<word>{'|'.join(words)})|(?P<count>{'|'.join(counts)}):(?P<width>{digits}))\}}|(?P<text>[^{{}}]+)", template)
        )
        pinned = {part.key: part.width for part in reversed(parts) if isinstance(part, Number)}
        field = rf"\{{(?:{'|'.join((*words, *(f'{key.name.lower()}:{width}' for key, width in pinned.items())))})\}}"
        grammar = re.compile(rf"(?![\s\S]*\x00)[^{{}}]*(?:{field}(?:[^{{}}]*(?:[^\w{{}}]|_)[^{{}}]*{field})*[^{{}}]*)?")
        return Ok(cls(parts=parts)) if grammar.fullmatch(template) else Error((RenderingNameFault(request=template, error=grammar),))

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
                    return re.compile("|".join(member.token for member in Output))
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
