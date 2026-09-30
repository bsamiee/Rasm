"""Records the Adobe product modules share: setting rows with their shared accessors, the script argument, toolbars, papers, and the interface scale a panel font theme gives."""

from collections.abc import Mapping
from enum import Enum
from pathlib import Path
from typing import Final

from lxml import etree
import msgspec

from interface.roles import TEXT_POINTS
from interface.units import Length, Units

# --- [TYPES] ----------------------------------------------------------------------------


class Paper(Enum):
    """New Document papers by sheet name, each valued by the unit system whose document size, page unit, and margin it takes."""

    LETTER = Units.IMPERIAL
    A4 = Units.METRIC

    @property
    def sheet(self) -> str:
        """Sheet name the products list the paper by."""
        return self.name.title()

    @property
    def title(self) -> str:
        """Preset title naming the paper's unit system and sheet."""
        return f"{self.value.name.title()} {self.sheet}"


# --- [CONSTANTS] ------------------------------------------------------------------------

SCRIPT: Final = Path(__file__).with_name("script")
STROKE_UNITS: Final = frozendict({Units.IMPERIAL: Length.POINTS, Units.METRIC: Length.MILLIMETERS})

# --- [MODELS] ---------------------------------------------------------------------------


class Member(msgspec.Struct, frozen=True, tag=True):
    """Member of the `app` property the owner names, holding a member of the named DOM enumeration where one is named."""

    owner: str
    name: str
    enumeration: str | None


class Active(msgspec.Struct, frozen=True, tag=True):
    """Product's active workspace by name."""


class Row(msgspec.Struct, frozen=True):
    """Setting with its report label, the accessor that reaches its store, and the target the script writes."""

    label: str
    access: msgspec.Struct
    target: object


class Release(msgspec.Struct, frozen=True, tag=True):
    """Script argument closing the untitled documents and reporting the titled ones."""


class Converge(msgspec.Struct, frozen=True, tag=True):
    """Script argument writing the rows."""

    rows: tuple[Row, ...]


class Toolbar(msgspec.Struct, frozen=True):
    """Toolbar with its slots in order, each a flyout of tool ids with the shown tool first, and its footer options by key."""

    slots: tuple[tuple[str, ...], ...]
    options: Mapping[str, int]


# --- [OPERATIONS] -----------------------------------------------------------------------


def member(owner: str, name: str, target: object, *, enumeration: str | None = None) -> Row:
    """Row of the DOM member of the `app` property, labeled `owner.name`."""
    return Row(f"{owner}.{name}", Member(owner, name, enumeration), target)


def papers(units: Units) -> tuple[Paper, ...]:
    """New Document paper of every unit system, the system's own first."""
    return (Paper(units), *(paper for paper in Paper if paper.value is not units))


def text_scale(theme: Path) -> float:
    """Interface scale drawing the panel font theme's main text, one size in every font size scheme, at the interface text size."""
    tree = etree.parse(theme)
    match tree.docinfo.internalDTD:
        case None:
            raise ValueError(f"{theme} declares no text ids")
        case declared:
            main = {entity.name: entity.content for entity in declared.iterentities()}["kText_Main_Large"]
            (size,) = {float(row.attrib["size"]) for row in tree.iterfind(f"ThemeFonts/FontSize[@name='{main}']")}
            return TEXT_POINTS / size


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SCRIPT", "STROKE_UNITS", "Active", "Converge", "Member", "Paper", "Release", "Row", "Toolbar", "member", "papers", "text_scale"]
