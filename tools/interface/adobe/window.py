"""Essentials frame every Adobe product shares in its right dock columns, and its arrangement of a `<workspace>` block a product wrote."""

from collections import ChainMap
from collections.abc import Callable, Mapping, Sequence
import copy
from enum import StrEnum
from typing import Final

from lxml import etree
from lxml.builder import E
import msgspec

from interface import host
from interface.frame import RIGHT_COLUMN, Role

# --- [TYPES] ----------------------------------------------------------------------------


class Markup(StrEnum):
    """Workspace element tags the arrangement moves."""

    DOCK = "dock"
    PANE = "tab-pane"
    GROUP = "tab-group"
    PALETTE = "palette"
    TOOLBAR = "toolbar"


class Attribute(StrEnum):
    """Workspace element attributes by their written names, `app-data` holding a product's panel data."""

    ID = "id"
    DATA = "app-data"
    ANCHOR = "anchor"
    ORIGIN = "origin"
    MODE = "mode"
    ICON_LENGTH = "preferred-iconic-length"
    LAYOUT = "layout-mode"
    ACTIVE = "active-palette"
    CLOSED = "is-closed"
    STATE = "current-state"
    MINIMIZED = "is-minimized"
    VARIANT = "size-variant"
    UNCONSTRAINED = "preferred-unconstrained-size"
    CONSTRAINED = "preferred-constrained-size"


class Anchor(StrEnum):
    """Dock edges, a floating dock anchored to none."""

    TOP = "top"
    BOTTOM = "bottom"
    LEFT = "left"
    RIGHT = "right"
    NONE = "none"


class Mode(StrEnum):
    """Tab pane display, its panels expanded or collapsed to icons."""

    EXPANDED = "expanded"
    ICON = "icon"


class Layout(StrEnum):
    """Tab pane layout, groups flowing down a docked column or placed by hand in a floating dock."""

    AUTO_FLOW = "auto-flow"
    MANUAL = "manual"


# --- [CONSTANTS] ------------------------------------------------------------------------

WORKSPACE: Final = "Essentials"

# --- [MODELS] ---------------------------------------------------------------------------


class Column(msgspec.Struct, frozen=True):
    """Right dock column by its display and the roles of its tab groups top to bottom."""

    mode: Mode
    roles: tuple[Role, ...]


class Frame(msgspec.Struct, frozen=True, kw_only=True):
    """Product's Essentials workspace: toolbar, control bar, role groups shown panel first, fixed-width, closed, and stateful panels, and the product's panel id reader, width writer, and closed group form."""

    toolbar: str | int
    bar: bool
    groups: Mapping[Role, tuple[str | int, ...]]
    fixed: frozenset[str | int] = frozenset()
    hidden: frozenset[str | int] = frozenset()
    states: Mapping[str | int, int] = frozendict()
    identity: Callable[[etree._Element], str | int]
    sized: Callable[[etree._Element, int], None]
    closed: Callable[[etree._Element, Sequence[etree._Element]], None]


COLUMNS: Final = (
    Column(Mode.ICON, (Role.LIBRARIES, Role.TRANSFORM, Role.CHARACTER, Role.STYLES, Role.GLYPHS, Role.ASSETS, Role.PATTERN, Role.BLEND, Role.EFFECTS)),
    Column(Mode.EXPANDED, (Role.PROPERTIES, Role.ALIGNMENT, Role.DOCUMENT)),
    Column(Mode.EXPANDED, (Role.COLOR, Role.AUTOMATION, Role.STRUCTURE)),
    Column(Mode.ICON, (Role.EXPORT, Role.INFORMATION, Role.OUTPUT)),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [FORMS]
def pane(mode: Mode, layout: Layout, *groups: etree._Element) -> etree._Element:
    """Tab pane in the mode and layout holding the groups, at the iconic length in the icon mode."""
    return E(Markup.PANE, {Attribute.MODE: mode, Attribute.ICON_LENGTH: "39" if mode is Mode.ICON else "0", Attribute.LAYOUT: layout}, *groups)


def shut(element: etree._Element, *, closed: bool) -> None:
    """Set the element's closed flag in the workspace's lowercase boolean spelling."""
    element.set(Attribute.CLOSED, str(closed).lower())


def sized(panel: etree._Element, width: int) -> None:
    """Set both preferred sizes of an Illustrator or InDesign panel to the width at their held heights."""
    panel.attrib.update({key: f"{width} {panel.attrib[key].partition(' ')[2]}" for key in (Attribute.UNCONSTRAINED, Attribute.CONSTRAINED)})


def docked(root: etree._Element, groups: Sequence[etree._Element]) -> None:
    """Close Photoshop's groups and their panels in one expanded column after the right dock's columns."""
    column = pane(Mode.EXPANDED, Layout.AUTO_FLOW, *groups)
    for element in column.iter(Markup.GROUP, Markup.PALETTE):
        shut(element, closed=True)
    next(dock for dock in root.iterchildren(Markup.DOCK) if dock.get(Attribute.ANCHOR) == Anchor.RIGHT).append(column)


def floated(root: etree._Element, groups: Sequence[etree._Element], *, closed: bool) -> None:
    """Hold each group open in its own closed floating dock with its panels closed as the flag states, in the attributes of the floating dock it sat in, else of the first floating dock at the origin."""
    template = {**next(dock for dock in root.iterchildren(Markup.DOCK) if dock.get(Attribute.ANCHOR) == Anchor.NONE).attrib, Attribute.ORIGIN: "0 0"}
    for group in groups:
        attributes = next((dict(dock.attrib) for dock in group.iterancestors(Markup.DOCK)), template)
        etree.SubElement(root, Markup.DOCK, attributes | {Attribute.CLOSED: "true"}).append(pane(Mode.EXPANDED, Layout.MANUAL, group))
        shut(group, closed=False)
        for panel in group:
            shut(panel, closed=closed)


# --- [BLOCKS]
def parsed(data: bytes) -> etree._Element:
    """Root of a `<workspace>` block or a workspace document, whitespace between elements dropped."""
    return etree.fromstring(data, etree.XMLParser(remove_blank_text=True))


def indented(root: etree._Element) -> etree._Element:
    """Root with its tree laid out as the products write a block: tab indented, one element per line."""
    etree.indent(root, space="\t")
    return root


def serialized(root: etree._Element) -> str:
    """Block text of the root laid out as the products write it."""
    return etree.tostring(indented(root), encoding="unicode")


# --- [ARRANGEMENT]
def arranged(frame: Frame, root: etree._Element, factory: Sequence[etree._Element]) -> etree._Element | tuple[host.Skip, ...]:
    """Held `<workspace>` root in the frame's layout with every other held panel closed, a placed panel it lacks taken from the first factory root holding it, else one skip per placed panel neither holds."""
    held, *stock = ({frame.identity(element): element for element in tree.iter(Markup.PALETTE, Markup.TOOLBAR)} for tree in (root, *factory))
    found = ChainMap(held, *stock)
    wanted = (frame.toolbar, *(panel for panels in frame.groups.values() for panel in panels))
    if absent := tuple(host.Skip(str(panel)) for panel in wanted if panel not in found):
        return absent
    placed = {panel: held[panel] if panel in held else copy.deepcopy(found[panel]) for panel in wanted}
    panes = {column: pane(column.mode, Layout.AUTO_FLOW) for column in COLUMNS}
    shown = [(column, role) for column in COLUMNS for role in column.roles if role in frame.groups]
    for column, role in shown:
        group = etree.SubElement(panes[column], Markup.GROUP, {Attribute.ACTIVE: "", Attribute.CLOSED: "false"})
        for panel in frame.groups[role]:
            group.append(placed[panel])
            shut(placed[panel], closed=panel in frame.hidden)
            placed[panel].set(Attribute.MINIMIZED, "false")
            if column.mode is Mode.EXPANDED and panel not in frame.fixed:
                frame.sized(placed[panel], RIGHT_COLUMN)
    for panel, state in frame.states.items():
        placed[panel].set(Attribute.STATE, str(state))
    remaining = [group for group in root.iter(Markup.GROUP) if len(group)]
    docks = {dock.get(Attribute.ANCHOR): dock for dock in root.iterchildren(Markup.DOCK)}
    floating = [dock for dock in root.iterchildren(Markup.DOCK) if dock.get(Attribute.ANCHOR) == Anchor.NONE]
    toolbars = {panel: element for panel, element in held.items() if element.tag == Markup.TOOLBAR} | {frame.toolbar: placed[frame.toolbar]}
    for element in docks[Anchor.TOP]:
        shut(element, closed=not frame.bar)
    for panel, element in toolbars.items():
        shut(element, closed=panel != frame.toolbar)
    placed[frame.toolbar].set(Attribute.VARIANT, "vertical-wide")
    del docks[Anchor.BOTTOM][:]
    docks[Anchor.LEFT][:] = toolbars.values()
    docks[Anchor.RIGHT][:] = [holder for holder in panes.values() if len(holder)]
    frame.closed(root, remaining)
    root[:] = [dock for dock in root if dock not in floating]
    for number, element in enumerate(root.iterfind(f".//*[@{Attribute.ID}]"), 1):
        element.set(Attribute.ID, str(number))
    for group in root.iter(Markup.GROUP):
        group.set(Attribute.ACTIVE, group[0].attrib[Attribute.ID])
    return root


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WORKSPACE", "Attribute", "Frame", "Markup", "arranged", "docked", "floated", "indented", "parsed", "serialized", "sized"]
