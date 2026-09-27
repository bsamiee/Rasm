# ruff: file-ignore[suspicious-xml-etree-import]
"""Edits of the files Rhino reads at launch, made while it is closed."""

import codecs
from collections.abc import Callable, Mapping, Sequence
from copy import deepcopy
import ctypes
from enum import auto, StrEnum
from functools import reduce
from io import BytesIO
import math
from pathlib import Path
from typing import Final
import uuid
import xml.etree.ElementTree as ET
from xml.parsers import expat

import msgspec

from interface import report
from interface.host import Change, Error, Plugin
from interface.rhino.packages import Package
from interface.rhino.window import Extent, osnap_height, Panel, RETURN_BOTTOM, RETURN_TOP, RIGHT_BOTTOM, RIGHT_TOP, Site, Toolbar
from interface.roles import Guide, RIGHT_COLUMN, Status, Text, TREE_ROWS

# --- [TYPES] ----------------------------------------------------------------------------

type Value = bool | int | float | str | tuple[str, ...]


class Node(StrEnum):
    """Element and attribute names of the toolbar, container, and settings files."""

    MACROS = auto()
    MACRO_ITEM = auto()
    SCRIPT = auto()
    ICONS = auto()
    ICON = auto()
    BITMAP_ID = auto()
    TOOL_BARS = auto()
    TOOL_BAR_ITEM = auto()
    TEXT = auto()
    LEFT_MACRO_ID = auto()
    RIGHT_MACRO_ID = auto()
    BUTTON_STYLE = auto()
    BUTTON_DISPLAY_MODE = auto()
    LINK = auto()
    STYLE = auto()
    TOOL_BAR_GROUPS = auto()
    TOOL_BAR_GROUP = auto()
    TOOL_BAR_GROUP_ITEM = auto()
    TOOL_BAR_ID = auto()
    DOCK_BAR_INFO = auto()
    ACTIVE_TOOL_BAR_GROUP = auto()
    SOURCE_GROUP = auto()
    DOCK_BARS = auto()
    DOCK_BAR = auto()
    PLACEMENT = auto()
    TABS = auto()
    NAME = auto()
    LOCALE_1033 = auto()
    TOOL_BAR = auto()
    FILE = auto()
    PANEL = auto()
    DOCK_SITES = auto()
    DOCK_SITE = auto()
    BAND = auto()
    LAST_COLLECTION_PANEL_WAS_IN = auto()
    ITEM = auto()
    SETTINGS = auto()
    CHILD = auto()
    COMMAND = auto()
    ENTRY = auto()
    LIST = auto()
    VALUE = auto()
    ID = auto()
    GUID = auto()
    KEY = auto()
    HIDDEN = auto()
    LOCATION = auto()
    SIZE = auto()
    AUTO_HIDE = auto()
    SELECTED_ITEM = auto()
    DISPLAY_STYLE = auto()
    DOCK_LOCATION = auto()
    RECENT_DOCK_LOCATION = auto()
    DOCKED_PLACEMENT = auto()
    DOCK_BAND_SIZE = auto()
    VISIBLE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

SCHEME: Final = "Scheme__Default"
ASSETS: Final = ("Contents", "Frameworks", "RhMaterialEditor.framework", "Versions", "A", "Resources", "assets")

# --- [MODELS] ---------------------------------------------------------------------------


class Stage(msgspec.Struct, frozen=True):
    """Facts the file edit reads from the run before the quit, the dock site heights and extents by their measure names, and the declared packages."""

    settings: Path
    bundle: Path
    measures: Mapping[Site | Extent, float]
    sizes: Mapping[Site, int]
    columns: Mapping[str, float]
    plugins: tuple[Plugin, ...]
    packages: tuple[Package, ...]

    @property
    def present(self) -> tuple[Package, ...]:
        """Declared packages Rhino holds a plug-in of, in declared order."""
        return tuple(package for package in self.packages if package.id in {plugin.package for plugin in self.plugins})

    @property
    def plugin_toolbars(self) -> tuple[Path, ...]:
        """Toolbar files the packages' plug-ins include, each once."""
        return tuple(dict.fromkeys(Path(plugin.toolbars) for plugin in self.plugins if plugin.toolbars is not None))

    @property
    def containers(self) -> Path:
        """Window layout Rhino reads at launch."""
        return self.settings / SCHEME / "containers.xml"

    @property
    def toolbars(self) -> Path:
        """Toolbar file Rhino loads."""
        return self.settings.parent / "UI" / "default.rui"

    @property
    def bundled_layout(self) -> Path:
        """Window layout the bundle includes."""
        return self.bundle.joinpath(*ASSETS, "default.rhw")

    @property
    def bundled_toolbars(self) -> Path:
        """Toolbar file the bundle includes."""
        return self.bundle.joinpath(*ASSETS, "default.rui")

    @property
    def file(self) -> Path:
        """Rhino's own settings file."""
        return self.settings / f"settings-{SCHEME}.xml"


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Settings child by key path under the file's settings or a command block, with its values and the entries left to Rhino's factory value."""

    path: tuple[str, ...]
    entries: Mapping[str, Value] = {}
    factory: Mapping[str, Value | None] = {}
    command: str | None = None
    hidden: bool = False


class Store(msgspec.Struct, frozen=True):
    """Settings file by its path and the children the edit states in it."""

    path: Path
    children: tuple[Child, ...]


class References(msgspec.Struct, frozen=True):
    """Elements of the bundled layout, the bundled toolbar file, and the written layout the edits locate by, and the toolbar file's id."""

    bundled_ribbon: ET.Element
    ribbon_group: ET.Element
    ribbon: ET.Element
    ribbon_band: ET.Element
    top: ET.Element
    osnap: ET.Element
    history: ET.Element
    toolbar_file: str


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [XML]
def tree(text: bytes) -> ET.Element:
    """Root of a document's tree with its comments and namespace prefixes kept."""
    builder, parser = ET.TreeBuilder(insert_comments=True), expat.ParserCreate()
    parser.buffer_text = True
    parser.StartElementHandler, parser.EndElementHandler, parser.CharacterDataHandler, parser.CommentHandler = builder.start, builder.end, builder.data, builder.comment
    parser.ParseFile(BytesIO(text))
    return builder.close()


def element(owner: ET.Element, tag: str, *keys: tuple[str, str]) -> ET.Element:
    """Owner's first child with the tag and attributes, created when absent."""
    match [each for each in owner.findall(tag) if all(each.get(name) == value for name, value in keys)]:
        case [found, *_]:
            return found
        case _:
            return ET.SubElement(owner, tag, dict(keys))


def nested(owner: ET.Element, *keys: str) -> ET.Element:
    """Owner's settings child at the key path, each level created when absent."""
    return reduce(lambda held, key: element(held, Node.CHILD, (Node.KEY, key)), keys, owner)


def values(owner: ET.Element, key: str, texts: Sequence[str]) -> None:
    """Set the owner's settings entry to the texts as its list."""
    held = element(owner, Node.ENTRY, (Node.KEY, key))
    del held[:]
    listed = ET.SubElement(held, Node.LIST)
    for text in texts:
        ET.SubElement(listed, Node.VALUE).text = text


def spelled(value: Value) -> str | tuple[str, ...]:
    """Value as a settings file spells it, a list as its texts."""
    return value if isinstance(value, tuple) else str(value)


def canonical(root: ET.Element) -> str:
    """Short digest of a tree's canonical XML, blind to the indentation Rhino writes and a rebuilt list drops."""
    return report.digest(ET.canonicalize(ET.tostring(root, encoding="unicode"), strip_text=True).encode())


def label(path: Path) -> str:
    """Report label of a file by its last three path parts."""
    return f"file {Path(*path.parts[-3:])}"


def read(path: Path) -> bytes:
    """File's bytes, empty settings when it is absent."""
    return path.read_bytes() if path.exists() else codecs.BOM_UTF8 + ET.tostring(ET.Element(Node.SETTINGS, {Node.ID: "2.0"}))


def written(path: Path, held: bytes, root: ET.Element, projection: Callable[[ET.Element], str] = canonical) -> Change | None:
    """Write the edited tree with the byte-order mark the file held and return the change when it differs from the held bytes under the projection."""
    ET.indent(root)
    before, after = projection(tree(held)), projection(root)
    if after == before:
        return None
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes((codecs.BOM_UTF8 if held.startswith(codecs.BOM_UTF8) else b"") + ET.tostring(root, encoding="utf-8", xml_declaration=True))
    return Change(label(path), before, after)


# --- [CONTAINERS]
def panel_bars(containers: ET.Element, panel: Panel) -> list[ET.Element]:
    """Dock bars whose tabs hold the panel."""
    return [bar for bar in containers.iterfind(f"{Node.DOCK_BARS}/{Node.DOCK_BAR}") if bar.find(f"{Node.TABS}/{Node.PANEL}[@{Node.GUID}='{panel}']") is not None]


def references(containers: ET.Element, bundled: ET.Element, toolbars: ET.Element) -> References | Error:
    """Elements the edits locate by, an error naming each one the files lack."""
    bars = f"{Node.DOCK_BARS}/{Node.DOCK_BAR}"
    bundled_ribbon = next(
        (bar for bar in bundled.iterfind(bars) if bar.find(f"{Node.PLACEMENT}[@{Node.DOCK_LOCATION}='{Site.TOP}']") is not None and bar.find(f"{Node.TABS}/{Node.TOOL_BAR}") is not None), None
    )
    if bundled_ribbon is None:
        return Error("default.rhw holds no ribbon docked at the top")
    ribbon, (head, *_) = bundled_ribbon.attrib[Node.GUID], RIGHT_TOP
    group = toolbars.find(f"{Node.TOOL_BAR_GROUPS}/{Node.TOOL_BAR_GROUP}[@{Node.GUID}='{bundled_ribbon.get(Node.SOURCE_GROUP)}']")
    tabs = {row.get(Node.GUID) for row in bundled_ribbon.iterfind(f"{Node.TABS}/{Node.TOOL_BAR}")} & {item.findtext(Node.TOOL_BAR_ID) for item in toolbars.iter(Node.TOOL_BAR_GROUP_ITEM)}
    found = {
        "ribbon toolbar group": group,
        "ribbon": containers.find(f"{bars}[@{Node.GUID}='{ribbon}']"),
        "ribbon band": next((band for band in containers.iterfind(f"{Node.DOCK_SITES}/{Node.DOCK_SITE}/{Node.BAND}") if band.find(f"{Node.DOCK_BAR}[@{Node.GUID}='{ribbon}']") is not None), None),
        "right column's top container": next(iter(panel_bars(containers, head)), None),
        "Osnap container": next(iter(panel_bars(containers, Panel.OSNAP)), None),
        "Command History bar": containers.find(f"{bars}[@{Node.GUID}='{Panel.COMMAND_HISTORY}']"),
    }
    file = toolbars.get(Node.GUID)
    missing = [*(name for name, each in found.items() if each is None), *(f"ribbon tab {toolbar.name}" for toolbar in Toolbar if toolbar not in tabs), *(() if file else ("toolbar file id",))]
    match tuple(found.values()), file, missing:
        case (ET.Element() as grouped, ET.Element() as held, ET.Element() as band, ET.Element() as top, ET.Element() as osnap, ET.Element() as history), str(), []:
            return References(bundled_ribbon, grouped, held, band, top, osnap, history, file)
        case _:
            return Error(f"bundled and written files hold no {', '.join(missing)}")


def bottom_bar(containers: ET.Element, top: ET.Element) -> ET.Element:
    """Container holding Layers apart from the top one, created from the top one's placement when Layers shares it."""
    match [bar for bar in panel_bars(containers, Panel.LAYERS) if bar is not top]:
        case [held, *_]:
            return held
        case _:
            title = "Layers"
            bar = ET.SubElement(element(containers, Node.DOCK_BARS), Node.DOCK_BAR, {Node.GUID: str(uuid.uuid5(uuid.UUID(top.attrib[Node.GUID]), Panel.LAYERS))})
            bar.extend(deepcopy(placement) for placement in top.findall(Node.PLACEMENT))
            tabs = ET.SubElement(bar, Node.TABS, {Node.NAME: title, Node.SELECTED_ITEM: Panel.LAYERS, Node.DISPLAY_STYLE: element(top, Node.TABS).attrib[Node.DISPLAY_STYLE]})
            ET.SubElement(ET.SubElement(tabs, Node.NAME), Node.LOCALE_1033).text = title
            return bar


def order_ribbon(held: References) -> None:
    """Order the ribbon tabs by role, keeping the rows Rhino wrote, dropping every other tab, and selecting the first."""
    tabs = element(held.ribbon, Node.TABS)
    bundled = {row.get(Node.GUID): row for row in held.bundled_ribbon.iterfind(f"{Node.TABS}/{Node.TOOL_BAR}")}
    rows = {row.get(Node.GUID): row for row in tabs.findall(Node.TOOL_BAR)}
    tabs[:] = [*(row for row in tabs if row.tag != Node.TOOL_BAR), *(rows.get(toolbar, deepcopy(bundled[toolbar])) for toolbar in Toolbar)]
    tabs.set(Node.SELECTED_ITEM, next(iter(Toolbar)))


def order_group(group: ET.Element) -> None:
    """Order the ribbon's toolbar group by role, dropping every other tab and activating the first."""
    items = {item.findtext(Node.TOOL_BAR_ID): item for item in group.findall(Node.TOOL_BAR_GROUP_ITEM)}
    group[:] = [*(item for item in group if item.tag != Node.TOOL_BAR_GROUP_ITEM), *(items[toolbar] for toolbar in Toolbar)]
    group.set(Node.ACTIVE_TOOL_BAR_GROUP, next(iter(Toolbar)))


def order_columns(containers: ET.Element, columns: Mapping[ET.Element, tuple[Panel, ...]]) -> None:
    """Hold each container's panels in the declared order alone, the first one selected."""
    panels = {row.get(Node.GUID): row for bar in containers.iterfind(f"{Node.DOCK_BARS}/{Node.DOCK_BAR}") for row in bar.findall(f"{Node.TABS}/{Node.PANEL}")}
    for bar, (head, *rest) in columns.items():
        tabs = element(bar, Node.TABS)
        tabs[:] = [*(row for row in tabs if row.tag != Node.PANEL), *(panels[panel] for panel in (head, *rest) if panel in panels)]
        tabs.set(Node.SELECTED_ITEM, head)


def remove_strays(containers: ET.Element, kept: set[ET.Element], file: str) -> None:
    """Remove every other panel container and its band row, each one reclaiming its panels at launch, and hide every other unbanded container of another toolbar file's toolbars in Rhino's form."""
    owners, rows = element(containers, Node.DOCK_BARS), [(band, row) for band in containers.iterfind(f"{Node.DOCK_SITES}/{Node.DOCK_SITE}/{Node.BAND}") for row in band.findall(Node.DOCK_BAR)]
    holders = {bar.get(Node.GUID) for bar in owners.findall(Node.DOCK_BAR) if bar not in kept and bar.find(Node.TABS) is not None and bar.find(f"{Node.TABS}/{Node.TOOL_BAR}") is None}
    for held in (owners, *dict.fromkeys(band for band, _ in rows)):
        held[:] = [row for row in held if row.get(Node.GUID) not in holders]
    banded = {row.get(Node.GUID) for _, row in rows}
    strays = [bar for bar in owners.findall(Node.DOCK_BAR) if bar not in kept and bar.get(Node.GUID) not in banded]
    foreign = (bar for bar in strays if file not in (files := {row.get(Node.FILE) for row in bar.iterfind(f"{Node.TABS}/{Node.TOOL_BAR}")}) and files)
    for placement in (placement for bar in foreign for placement in bar.findall(Node.PLACEMENT)):
        placement.attrib.pop(Node.VISIBLE, None)


def place(columns: Sequence[ET.Element], docked: Mapping[ET.Element, tuple[Site, int]]) -> None:
    """Show each right-hand and docked container at its slot, each docked one on its site."""
    for bar, slot in (*((bar, slot) for slot, bar in enumerate(columns)), *((bar, slot) for bar, (_, slot) in docked.items())):
        for placement in bar.findall(Node.PLACEMENT):
            placement.attrib.update({Node.DOCKED_PLACEMENT: f"0,{slot}", Node.VISIBLE: str(True)})
    for bar, (site, _) in docked.items():
        for placement in bar.findall(Node.PLACEMENT):
            placement.attrib.update({Node.DOCK_LOCATION: site, Node.RECENT_DOCK_LOCATION: site})


def fill_bands(sites: ET.Element, bands: Mapping[Site, ET.Element], columns: Sequence[ET.Element], docked: Mapping[ET.Element, tuple[Site, int]]) -> None:
    """Fill the right band with the right-hand containers and move each docked container's row into its site's band at its slot with no share."""
    right, moved = bands[Site.RIGHT], {bar.attrib[Node.GUID] for bar in docked}
    right[:] = [*(row for row in right if row.tag != Node.DOCK_BAR), *(ET.Element(Node.DOCK_BAR, {Node.GUID: bar.attrib[Node.GUID]}) for bar in columns)]
    for band in sites.iterfind(f"{Node.DOCK_SITE}/{Node.BAND}"):
        band[:] = [row for row in band if row.get(Node.GUID) not in moved]
    for bar, (site, slot) in docked.items():
        bands[site].insert(slot, ET.Element(Node.DOCK_BAR, {Node.GUID: bar.attrib[Node.GUID]}))


def size_bands(containers: ET.Element, bands: Mapping[Site, ET.Element], sizes: Mapping[Site, int]) -> None:
    """Set each band with a settled size to that size in the band and in the placement of every container it holds."""
    owners = element(containers, Node.DOCK_BARS)
    for site, size in sizes.items():
        band = bands[site]
        band.set(Node.SIZE, str(size))
        for placement in (placement for row in band.findall(Node.DOCK_BAR) for placement in owners.iterfind(f"{Node.DOCK_BAR}[@{Node.GUID}='{row.get(Node.GUID)}']/{Node.PLACEMENT}")):
            width, height = placement.attrib[Node.DOCK_BAND_SIZE].split(",")
            placement.set(Node.DOCK_BAND_SIZE, f"{size},{height}" if site in {Site.LEFT, Site.RIGHT} else f"{width},{size}")


def show_sites(sites: ET.Element) -> None:
    """Show every dock site and remove its empty bands."""
    for site in sites.iterfind(Node.DOCK_SITE):
        site.set(Node.AUTO_HIDE, str(False))
        site[:] = [held for held in site if held.tag != Node.BAND or len(held) > 0]


def layers(measures: Mapping[Site | Extent, float]) -> float:
    """Height of the Layers container showing the tree's rows."""
    return measures[Extent.LAYERS_CHROME] + measures[Extent.LAYERS_HEADER] + TREE_ROWS * measures[Extent.LAYERS_ROW]


def side_height(sites: ET.Element, measures: Mapping[Site | Extent, float], site: Site) -> float:
    """Side band's whole height once the top and bottom bands take their written sizes, a band Rhino sizes itself counting none."""
    return measures[site] + sum(
        measures[site] - sum(float(size) + measures[Extent.RESIZER] for band in sites.iterfind(f"{Node.DOCK_SITE}[@{Node.LOCATION}='{site}']/{Node.BAND}") if (size := band.get(Node.SIZE)) is not None)
        for site in (Site.TOP, Site.BOTTOM)
    )


def shares(whole: float, resizer: float, lower: float) -> tuple[float, float]:
    """Shares of a side band's two containers, the lower one at its content height and the upper one taking the rest at the middle of the whole point Rhino truncates it to."""
    upper = (whole - resizer - lower + 0.5) / whole
    return (upper, 1 - upper)


def share_sides(sites: ET.Element, bands: Mapping[Site, ET.Element], measures: Mapping[Site | Extent, float], sizes: Mapping[Site, int]) -> None:
    """Share each side band among its containers, Layers' rows on the right and the Osnap strip wrapped to its band width under the sidebar on the left."""
    for site, lower in ((Site.RIGHT, layers(measures)), (Site.LEFT, osnap_height(measures, sizes[Site.LEFT]))):
        for row, share in zip(bands[site].findall(Node.DOCK_BAR), shares(side_height(sites, measures, site), measures[Extent.RESIZER], lower), strict=True):
            row.set(Node.SIZE, repr(ctypes.c_float(share).value))


def home_panels(containers: ET.Element, top: ET.Element, bottom: ET.Element, present: Sequence[Package]) -> None:
    """Return each right-hand panel, each panel that opens on demand, and each present package's panel to its container."""
    returns = element(containers, Node.LAST_COLLECTION_PANEL_WAS_IN)
    homes = {**dict.fromkeys((*RIGHT_TOP, *RETURN_TOP, *(Panel[name] for package in present for name in package.panels)), top), **dict.fromkeys((*RIGHT_BOTTOM, *RETURN_BOTTOM), bottom)}
    for panel, bar in homes.items():
        element(returns, Node.ITEM, (Node.GUID, panel)).set(Node.DOCK_BAR, bar.attrib[Node.GUID])


def arrange(containers: ET.Element, held: References, stage: Stage) -> None:
    """Edit the layout into the declared one."""
    bottom = bottom_bar(containers, held.top)
    columns = {held.top: RIGHT_TOP, bottom: RIGHT_BOTTOM}
    docked = {held.osnap: (Site.LEFT, 1), held.history: (Site.BOTTOM, 0)}
    sites = element(containers, Node.DOCK_SITES)
    bands = {**{site: element(element(sites, Node.DOCK_SITE, (Node.LOCATION, site)), Node.BAND) for site in (Site.LEFT, Site.RIGHT, Site.BOTTOM)}, Site.TOP: held.ribbon_band}
    order_ribbon(held)
    order_columns(containers, {**columns, held.osnap: (Panel.OSNAP,)})
    remove_strays(containers, {*columns, *docked}, held.toolbar_file)
    place(tuple(columns), docked)
    fill_bands(sites, bands, tuple(columns), docked)
    size_bands(containers, bands, stage.sizes)
    show_sites(sites)
    share_sides(sites, bands, stage.measures, stage.sizes)
    home_panels(containers, held.top, bottom, stage.present)


# --- [SETTINGS]
def layer_columns(columns: Mapping[str, float], visible: Sequence[str], inset: float) -> dict[str, tuple[str, ...]]:
    """Order, Width, and Visible lists of a Layers column group showing the visible columns in order, the first taking the right column's rest."""
    stretch, *fixed = visible
    widths = {name: round(width) for name, width in columns.items()}
    widths[stretch] = math.floor(RIGHT_COLUMN - inset - sum(widths[name] for name in fixed))
    order = (*visible, *(name for name in columns if name not in visible))
    return {"Order": tuple(str(order.index(name)) for name in columns), "Width": tuple(str(widths[name]) for name in columns), "Visible": tuple(str(int(name in visible)) for name in columns)}


def registered(settings: ET.Element) -> dict[str, str]:
    """Plug-in ids by the English name Rhino's plug-in registry records."""
    records = settings.iterfind(f"{Node.SETTINGS}/{Node.CHILD}[@{Node.KEY}='PlugInRegistry']/{Node.CHILD}/{Node.CHILD}")
    return {name: record.attrib[Node.KEY] for record in records if (name := record.findtext(f"{Node.ENTRY}[@{Node.KEY}='Name']")) is not None}


def declared(stage: Stage, containers: ET.Element, registry: Mapping[str, str]) -> tuple[Store, ...]:
    """Settings files and children the edit states, plug-ins by registry id."""
    measures = stage.measures
    continuity = {"BadHairColor": Status.ERROR, "GoodHairColor": Status.SUCCESS, "MaxHairColor": Guide.CONSTRUCTION, "TextColor": Text.PRIMARY}
    material_rows, layouts = 4, {"Name": 80, "PageNumber": 25, "PageSize": 50}
    editors: dict[str, Mapping[str, Value]] = {
        "{0F0FB7B6-C7D9-0E70-B65A-BFEF546316A9}": {
            "ShowLabels": True,
            "ShowUnits": False,
            "AutoUpdate": True,
            "SplitterHorzLayoutA": round(measures[Extent.MATERIALS_STRIP] + material_rows * measures[Extent.MATERIALS_ROW]),
        },
        "{24C22CED-5138-0E70-B573-933DE4DD0F5A}": {},
        "{4A5570F9-7CF0-0E70-A3F5-B5272154837C}": {},
    }
    border, pitch = measures[Extent.LIBRARIES_BORDER], measures[Extent.LIBRARIES_ROW]
    span = side_height(element(containers, Node.DOCK_SITES), measures, Site.RIGHT) - measures[Extent.RESIZER] - layers(measures) - measures[Extent.LIBRARIES_CHROME]
    folders = border + pitch * min(measures[Extent.LIBRARIES_FOLDERS], math.floor((span - measures[Extent.LIBRARIES_LIST_MINIMUM] - border) / pitch))
    return (
        Store(
            stage.file,
            (
                Child(path=("LayersPanel",), entries={"ColumnSorting": False}),
                *(
                    Child(path=("LayersPanel", group), entries=layer_columns(stage.columns, visible, measures[Extent.LAYERS_INSET]), hidden=True)
                    for group, visible in (
                        ("LayerColumnGroup.Model", ("Name", "Current", "Locked", "Color", "Material", "ViewportVisible", "Visible")),
                        ("LayerColumnGroup.Viewport", ("Name", "Current", "ViewportVisible", "NewDetailOn", "Locked", "Color", "ViewportColor", "ViewportPrintColor", "Visible")),
                    )
                ),
                Child(
                    path=("LayoutsPanel",), entries={"Width": tuple(map(str, (*layouts.values(), math.floor(RIGHT_COLUMN - measures[Extent.LAYOUTS_INSET] - sum(layouts.values()))))), "Expanded": True}
                ),
                Child(path=("Options", "EdgeContinuity"), entries={name: ",".join(map(str, (255, *rgb))) for name, rgb in continuity.items()}),
                Child(path=(), entries={"Thumbnails": False}, command="NamedView"),
                *(Child(path=("Options", group), factory=factory) for group, factory in (("General", {"StartupCommands": None}), ("Display", {"MSAASampleCount": 4}))),
            ),
        ),
        *(
            Store(stage.settings.parent / "Plug-ins" / f"{name} ({registry[name]})" / "settings" / stage.file.name, children)
            for name, children in (
                (
                    "Renderer Development Kit",
                    tuple(Child(path=("Settings", "Editors", editor), entries={"PreviewMode": "List", "ListPreviewSizePercent": 5, **held}, hidden=True) for editor, held in editors.items()),
                ),
                ("RDK_EtoUI", (Child(path=("Libraries",), entries={"splitter": folders / span}, hidden=True),)),
                ("Snapshots", (Child(path=(), entries={"Thumbnails": False}),)),
            )
        ),
    )


def state(root: ET.Element, child: Child) -> None:
    """Write the child's values into the file, creating its levels, and remove its factory entries where it exists."""
    base = element(root, Node.SETTINGS) if child.command is None else element(root, Node.COMMAND, (Node.NAME, child.command))
    if (held := nested(base, *child.path) if child.entries else base.find("/".join((".", *(f"{Node.CHILD}[@{Node.KEY}='{key}']" for key in child.path))))) is None:
        return
    if child.hidden:
        held.set(Node.HIDDEN, str(True))
    for key, value in child.entries.items():
        match spelled(value):
            case tuple() as texts:
                values(held, key, texts)
            case str() as text:
                element(held, Node.ENTRY, (Node.KEY, key)).text = text
    held[:] = [each for each in held if each.tag != Node.ENTRY or each.get(Node.KEY) not in child.factory]


def stated(store: Store) -> Change | None:
    """Settings file's change once every child of the store is stated in it."""
    held = read(store.path)
    root = tree(held)
    for child in store.children:
        state(root, child)
    return written(store.path, held, root)


# --- [TOOLBARS]
def shown(rui: ET.Element) -> str:
    """Short digest of what a toolbar file shows, blind to item ids, macro copies, text order, and each item Rhino's reader drops after an empty sibling."""
    scripts = {macro.get(Node.GUID): " ".join(macro.findtext(Node.SCRIPT, "").split()) for macro in rui.iter(Node.MACRO_ITEM)}
    bars = sorted(
        (
            bar.get(Node.GUID, ""),
            bar.findtext(f"{Node.TEXT}/{Node.LOCALE_1033}", ""),
            tuple(
                (
                    scripts.get(item.findtext(Node.LEFT_MACRO_ID, "").strip(), ""),
                    scripts.get(item.findtext(Node.RIGHT_MACRO_ID, "").strip(), ""),
                    item.get(Node.BUTTON_STYLE, ""),
                    item.findtext(Node.LINK, "").strip(),
                )
                for previous, item in zip((None, *bar), bar, strict=False)
                if item.tag == Node.TOOL_BAR_ITEM and (previous is None or len(previous) > 0 or bool(previous.text) or bool(previous.tail))
            ),
        )
        for bar in rui.iter(Node.TOOL_BAR)
    )
    groups = sorted((group.get(Node.GUID, ""), tuple(item.findtext(Node.TOOL_BAR_ID, "") for item in group.iter(Node.TOOL_BAR_GROUP_ITEM))) for group in rui.iter(Node.TOOL_BAR_GROUP))
    return report.digest(repr((bars, groups)).encode())


def labeled(owner: ET.Element, text: str) -> ET.Element:
    """Owner with its English text."""
    ET.SubElement(ET.SubElement(owner, Node.TEXT), Node.LOCALE_1033).text = text
    return owner


def adopted(rui: ET.Element, source: ET.Element, macro: ET.Element) -> str:
    """Id the toolbar file runs a macro by, its own in the same file or that of a copy under a derived id with the icon its bitmap names."""
    guid = macro.attrib[Node.GUID] if source is rui else str(uuid.uuid5(uuid.UUID(rui.attrib[Node.GUID]), macro.attrib[Node.GUID]))
    if rui.find(f"{Node.MACROS}/{Node.MACRO_ITEM}[@{Node.GUID}='{guid}']") is None:
        copy, icons, icon = deepcopy(macro), element(rui, Node.ICONS), f"{Node.ICON}[@{Node.GUID}='{macro.get(Node.BITMAP_ID)}']"
        copy.set(Node.GUID, guid)
        element(rui, Node.MACROS).append(copy)
        icons.extend(deepcopy(held) for held in source.iterfind(f"{Node.ICONS}/{icon}") if icons.find(icon) is None)
    return guid


def button(rui: ET.Element, sources: Sequence[ET.Element], command: str, guid: uuid.UUID) -> ET.Element:
    """Button running the command alone, copied from a toolbar file's button or built on a macro, searching the toolbar file before the package's own."""
    macros = [(source, macro) for source in (rui, *sources) for macro in source.iter(Node.MACRO_ITEM) if macro.findtext(Node.SCRIPT, "").split() == ["!", f"_{command}"]]
    held = [(source, item) for source, macro in macros for item in source.iter(Node.TOOL_BAR_ITEM) if item.findtext(Node.LEFT_MACRO_ID, "").strip() == macro.attrib[Node.GUID]]
    match held, macros:
        case [(source, item), *_], _:
            made, owned = deepcopy(item), {macro.attrib[Node.GUID]: macro for macro in source.iter(Node.MACRO_ITEM)}
            made[:] = [each for each in made if each.tag != Node.LINK]
            for each in (*made.iterfind(Node.LEFT_MACRO_ID), *made.iterfind(Node.RIGHT_MACRO_ID)):
                each.text = adopted(rui, source, owned[(each.text or "").strip()])
        case [], [(source, macro), *_]:
            made = labeled(ET.Element(Node.TOOL_BAR_ITEM, {} if macro.get(Node.BITMAP_ID) else {Node.BUTTON_DISPLAY_MODE: "text_only"}), macro.findtext(f"{Node.TEXT}/{Node.LOCALE_1033}", command))
            ET.SubElement(made, Node.LEFT_MACRO_ID).text = adopted(rui, source, macro)
        case _:
            macro = labeled(ET.SubElement(element(rui, Node.MACROS), Node.MACRO_ITEM, {Node.GUID: str(uuid.uuid5(uuid.UUID(rui.attrib[Node.GUID]), command))}), command)
            ET.SubElement(macro, Node.SCRIPT).text = f"! _{command}"
            return button(rui, sources, command, guid)
    made.set(Node.GUID, str(guid))
    return made


def placed(rui: ET.Element, stage: Stage) -> None:
    """Place each present package's commands at the end of their role's ribbon tab after one spacer, several opening as a flyout named by package and tab."""
    namespace, tabs = uuid.UUID(rui.attrib[Node.GUID]), {bar.attrib[Node.GUID]: bar for bar in rui.iterfind(f"{Node.TOOL_BARS}/{Node.TOOL_BAR}")}
    sources = {package.id: tuple(tree(Path(plugin.toolbars).read_bytes()) for plugin in stage.plugins if plugin.package == package.id and plugin.toolbars is not None) for package in stage.present}
    groups = [(Toolbar[name], package.id, commands) for package in stage.present for name, commands in package.commands.items()]
    for toolbar in dict.fromkeys(toolbar for toolbar, *_ in groups):
        ET.SubElement(tabs[toolbar], Node.TOOL_BAR_ITEM, {Node.GUID: str(uuid.uuid5(namespace, toolbar)), Node.BUTTON_STYLE: "spacer"})
    for toolbar, package, (head, *rest) in groups:
        base = uuid.uuid5(namespace, f"{toolbar}/{package}")
        made = button(rui, sources[package], head, base)
        if rest:
            flyout = labeled(
                ET.SubElement(element(rui, Node.TOOL_BARS), Node.TOOL_BAR, {Node.GUID: str(uuid.uuid5(base, package))}), f"{package} {tabs[toolbar].findtext(f'{Node.TEXT}/{Node.LOCALE_1033}')}"
            )
            flyout.extend(button(rui, sources[package], command, uuid.uuid5(base, command)) for command in (head, *rest))
            ET.SubElement(made, Node.LINK, {Node.STYLE: "normal"}).text = flyout.attrib[Node.GUID]
        tabs[toolbar].append(made)


def concealed(path: Path) -> Change | None:
    """Plug-in toolbar file's change once each group it declares opens hidden, the state Rhino gives the container it creates from a group when it loads the file."""
    held = path.read_bytes()
    root = tree(held)
    for info in root.iterfind(f"{Node.TOOL_BAR_GROUPS}/{Node.TOOL_BAR_GROUP}/{Node.DOCK_BAR_INFO}"):
        info.set(Node.VISIBLE, str(False))
    return written(path, held, root)


# --- [COMPOSITION] ----------------------------------------------------------------------


def edit(stage: Stage) -> tuple[Change, ...] | Error:
    """Changes of the files edited with Rhino closed, or an error naming each missing element with nothing written."""
    held, toolbars = read(stage.containers), tree(stage.bundled_toolbars.read_bytes())
    containers = tree(held)
    match references(containers, tree(stage.bundled_layout.read_bytes()), toolbars):
        case Error() as error:
            return error
        case References() as found:
            arrange(containers, found, stage)
            order_group(found.ribbon_group)
            placed(toolbars, stage)
            settings = declared(stage, containers, registered(tree(read(stage.file))))
            changes = (written(stage.containers, held, containers), written(stage.toolbars, read(stage.toolbars), toolbars, shown), *map(stated, settings), *map(concealed, stage.plugin_toolbars))
            return tuple(change for change in changes if change is not None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Stage", "edit"]
