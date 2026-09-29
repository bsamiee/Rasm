"""Records the Adobe product modules share: setting rows with their shared accessors, the script argument, alias commands and their prompt source, toolbars, papers, and the interface scale a panel font theme gives."""

from collections.abc import Mapping
from enum import Enum
from pathlib import Path
from typing import Final

from lxml import etree
import msgspec

from interface import host
from interface.aliases import Alias, families
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
PROMPT_NAME: Final = "Alias"
STROKE_UNITS: Final = frozendict({Units.IMPERIAL: Length.POINTS, Units.METRIC: Length.MILLIMETERS})

# --- [MODELS] ---------------------------------------------------------------------------


class Member(msgspec.Struct, frozen=True, tag=True):
    """Member of the `app` property the owner names or of that collection's named item, added when absent, holding a member of the named DOM enumeration where one is named."""

    owner: str
    item: str | None
    name: str
    enumeration: str | None


class Active(msgspec.Struct, frozen=True, tag=True):
    """Product's active workspace by name."""


class ActionSet(msgspec.Struct, frozen=True, tag=True):
    """Action set by name and the action file under the artifacts folder that loads it."""

    name: str
    file: str


class Row(msgspec.Struct, frozen=True):
    """Setting with its report label, the accessor that reaches its store, and the target the script writes."""

    label: str
    access: msgspec.Struct
    target: object


class Release(msgspec.Struct, frozen=True, tag=True):
    """Script argument closing the untitled documents and reporting the titled ones."""


class Converge(msgspec.Struct, frozen=True, tag=True):
    """Script argument writing the rows with each action file read from the artifacts folder."""

    rows: tuple[Row, ...]
    artifacts: str


class Tool(msgspec.Struct, frozen=True, tag=True):
    """Tool a product selects by an Illustrator tool name, a Photoshop tool class string id, or an InDesign `UITools` member."""

    id: str


class Menu(msgspec.Struct, frozen=True, tag=True):
    """Menu command a product runs by an Illustrator command name, a Photoshop event string id, or an InDesign menu action id, and whether it acts on a selection alone."""

    id: str | int
    selection: bool


class Toolbar(msgspec.Struct, frozen=True):
    """Toolbar with its slots in order, each a flyout of tool ids with the shown tool first, and its footer options by key."""

    slots: tuple[tuple[str, ...], ...]
    options: Mapping[str, int]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ROWS]
def member(owner: str, name: str, target: object, *, item: str | None = None, enumeration: str | None = None) -> Row:
    """Row of the DOM member of the `app` property or of its named item, labeled `owner.name` or `owner["item"].name`."""
    return Row(f"{owner}.{name}" if item is None else f'{owner}["{item}"].{name}', Member(owner, item, name, enumeration), target)


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


# --- [SOURCE]
def prompt_source(name: str, commands: Mapping[Alias, Tool | Menu]) -> str:
    """ExtendScript source running the alias prompt in the named product over each alias's label and command and the families holding them."""
    table = {
        "product": name,
        "title": PROMPT_NAME,
        "families": {family.name: family.value for family in families(commands)},
        "commands": {alias.name: (alias.value, command) for alias, command in commands.items()},
    }
    return host.rendered(t"{(SCRIPT / 'prompt.jsx').read_text(encoding='utf-8').rstrip()!s}({table});\n")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "PROMPT_NAME",
    "SCRIPT",
    "STROKE_UNITS",
    "ActionSet",
    "Active",
    "Converge",
    "Member",
    "Menu",
    "Paper",
    "Release",
    "Row",
    "Tool",
    "Toolbar",
    "member",
    "papers",
    "prompt_source",
    "text_scale",
]
