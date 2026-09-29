"""Files Rhino reads at launch, edited once it quit: its settings file, the plug-in settings files, and the toolbar file."""

import codecs
from collections.abc import Callable, Mapping, Sequence
from copy import deepcopy
from enum import StrEnum
from functools import reduce
import math
from pathlib import Path, PurePosixPath
from typing import Final
import uuid
import zipfile

from lxml import etree
from lxml.builder import E
import msgspec

from interface import report
from interface.frame import RIGHT_COLUMN
from interface.host import Bundle, Change
from interface.rhino.markup import canonical, element
from interface.rhino.packages import Package
from interface.rhino.window import Extent, layers_height, Measured, RibbonTab, Site, upper_length
from interface.roles import Guide, Status, Text

# --- [TYPES] ----------------------------------------------------------------------------


class Editor(StrEnum):
    """Content editors docked in the right column, by the RDK settings id of each."""

    MATERIALS = "{0F0FB7B6-C7D9-0E70-B65A-BFEF546316A9}"
    ENVIRONMENTS = "{24C22CED-5138-0E70-B573-933DE4DD0F5A}"
    TEXTURES = "{4A5570F9-7CF0-0E70-A3F5-B5272154837C}"


# --- [CONSTANTS] ------------------------------------------------------------------------

LABEL: Final = "text/locale_1033"

# --- [MODELS] ---------------------------------------------------------------------------


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Settings child by key path under the file's settings or a command block, with its entries and the keys left to Rhino's factory value."""

    path: tuple[str, ...]
    entries: Mapping[str, bool | int | float | str | tuple[str, ...]] = frozendict()
    factory: frozenset[str] = frozenset()
    command: str | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [XML]
def label(path: Path) -> str:
    """Report label of a file by its last three path parts."""
    return report.subscript("file", PurePosixPath(*path.parts[-3:]).as_posix())


def written(path: Path, held: bytes, root: etree._Element, projection: Callable[[etree._Element], str]) -> Change | None:
    """Change of the file once the indented tree is written with the byte-order mark it held, None while the projection reads the held bytes alike."""
    etree.indent(root)
    before, after = projection(etree.fromstring(held)), projection(root)
    if after == before:
        return None
    path.write_bytes((codecs.BOM_UTF8 if held.startswith(codecs.BOM_UTF8) else b"") + etree.tostring(root.getroottree(), encoding="utf-8", xml_declaration=True))
    return Change(label(path), before, after)


# --- [SETTINGS]
def layer_columns(columns: Mapping[str, int], visible: Sequence[str], inset: float) -> dict[str, tuple[str, ...]]:
    """Order, Width, and Visible lists of a Layers column group showing the visible columns in order, the first taking the right column's rest."""
    stretch, *fixed = visible
    widths = {**columns, stretch: math.floor(RIGHT_COLUMN - inset - sum(columns[name] for name in fixed))}
    order = (*visible, *(name for name in columns if name not in visible))
    return {"Order": tuple(str(order.index(name)) for name in columns), "Width": tuple(str(widths[name]) for name in columns), "Visible": tuple(str(int(name in visible)) for name in columns)}


def registered(root: etree._Element) -> dict[str, str]:
    """Plug-in ids by the English name Rhino's plug-in registry records."""
    return {name: record.attrib["key"] for record in root.iterfind("settings/child[@key='PlugInRegistry']/child/child") if (name := record.findtext("entry[@key='Name']")) is not None}


def settings(measured: Measured, registry: Mapping[str, str]) -> tuple[tuple[Child, ...], dict[str, tuple[Child, ...]]]:
    """Children of Rhino's settings file and of each plug-in settings folder by name, the keys no running owner writes."""
    extents = measured["extents"]
    continuity = {"BadHairColor": Status.ERROR, "GoodHairColor": Status.SUCCESS, "MaxHairColor": Guide.CONSTRUCTION, "TextColor": Text.PRIMARY}
    material_rows, layouts = 4, {"Name": 80, "PageNumber": 25, "PageSize": 50}
    border, pitch = extents[Extent.LIBRARIES_BORDER], extents[Extent.LIBRARIES_ROW]
    span = upper_length(measured, Site.RIGHT, layers_height(extents)) - extents[Extent.LIBRARIES_CHROME]
    folders = border + pitch * min(extents[Extent.LIBRARIES_FOLDERS], math.floor((span - extents[Extent.LIBRARIES_LIST_MINIMUM] - border) / pitch))
    editors = {
        Editor.MATERIALS: {"ShowLabels": True, "ShowUnits": False, "AutoUpdate": True, "SplitterHorzLayoutA": round(extents[Extent.MATERIALS_STRIP] + material_rows * extents[Extent.MATERIALS_ROW])},
        Editor.ENVIRONMENTS: {},
        Editor.TEXTURES: {},
    }
    return (
        (
            Child(path=("LayersPanel",), entries={"ColumnSorting": False}),
            *(
                Child(path=("LayersPanel", group), entries=layer_columns(measured["columns"], visible, extents[Extent.LAYERS_INSET]))
                for group, visible in (
                    ("LayerColumnGroup.Model", ("Name", "Current", "Locked", "Color", "Material", "ViewportVisible", "Visible")),
                    ("LayerColumnGroup.Viewport", ("Name", "Current", "ViewportVisible", "NewDetailOn", "Locked", "Color", "ViewportColor", "ViewportPrintColor", "Visible")),
                )
            ),
            Child(path=("LayoutsPanel",), entries={"Width": tuple(map(str, (*layouts.values(), math.floor(RIGHT_COLUMN - extents[Extent.LAYOUTS_INSET] - sum(layouts.values()))))), "Expanded": True}),
            Child(path=("Options", "EdgeContinuity"), entries={name: ",".join(map(str, (255, *rgb))) for name, rgb in continuity.items()}),
            Child(path=(), entries={"Thumbnails": False}, command="NamedView"),
            *(Child(path=("Options", group), factory=frozenset({key})) for group, key in (("General", "StartupCommands"), ("Display", "MSAASampleCount"))),
        ),
        {
            f"{name} ({registry[name]})": children
            for name, children in (
                (
                    "Renderer Development Kit",
                    tuple(Child(path=("Settings", "Editors", editor), entries={"PreviewMode": "List", "ListPreviewSizePercent": 5, **held}) for editor, held in editors.items()),
                ),
                ("RDK_EtoUI", (Child(path=("Libraries",), entries={"splitter": folders / span}),)),
                ("Snapshots", (Child(path=(), entries={"Thumbnails": False}),)),
            )
        },
    )


def state(root: etree._Element, child: Child) -> None:
    """Write the child's entries into the file, creating its levels, and remove its factory keys where it exists."""
    base = element(root, "settings") if child.command is None else element(root, "command", name=child.command)
    levels = reduce(
        lambda found, key: [element(owner, "child", key=key) for owner in found] if child.entries else [each for owner in found for each in owner.xpath("child[@key = $key]", key=key)],
        child.path,
        [base],
    )
    for held in levels:
        for key, value in child.entries.items():
            entry = element(held, "entry", key=key)
            match value:
                case tuple():
                    entry[:] = [E.list(*map(E.value, value))]
                case _:
                    entry.text = str(value)
        held[:] = [each for each in held if each.tag != "entry" or each.get("key") not in child.factory]


def stated(path: Path, held: bytes | None, children: Sequence[Child]) -> Change | None:
    """Change of a settings file once every child is stated in it, None for a file whose folder Rhino has not created."""
    if held is None:
        return None
    root = etree.fromstring(held)
    for child in children:
        state(root, child)
    return written(path, held, root, lambda tree: canonical(etree.tostring(tree, encoding="unicode")))


def read(path: Path) -> bytes | None:
    """Plug-in settings file's bytes, empty settings while Rhino holds no value off its default, None while Rhino has not created the plug-in's settings folder."""
    try:
        return path.read_bytes()
    except FileNotFoundError:
        return codecs.BOM_UTF8 + etree.tostring(etree.Element("settings", id="2.0")) if path.parent.is_dir() else None


# --- [TOOLBARS]
def shown(rui: etree._Element) -> str:
    """Digest of what a toolbar file shows: each toolbar's items by resolved macro script, text, icon, button style, and link, and each group's toolbar ids."""
    macros = {macro.attrib["guid"]: (" ".join(macro.findtext("script", "").split()), macro.get("bitmap_id", ""), macro.findtext("button_text/locale_1033", "")) for macro in rui.iter("macro_item")}

    def resolved(item: etree._Element, side: str) -> tuple[str, str, str]:
        match item.findtext(side):
            case None:
                return ("", "", "")
            case guid:
                return macros[guid.strip()]

    def projected(item: etree._Element) -> tuple[str, ...]:
        (script, icon, text), (right, *_) = resolved(item, "left_macro_id"), resolved(item, "right_macro_id")
        style = (item.get("button_style", "normal"), item.get("button_display_mode", ""))
        return (item.findtext(LABEL) or text, script, right, icon, *style, item.findtext("link", "").strip())

    bars = sorted((bar.attrib["guid"], bar.findtext(LABEL, ""), tuple(map(projected, bar.iterfind("tool_bar_item")))) for bar in rui.iter("tool_bar"))
    groups = sorted((group.attrib["guid"], tuple(item.findtext("tool_bar_id", "") for item in group.iterfind("tool_bar_group_item"))) for group in rui.iter("tool_bar_group"))
    return report.digest(repr((bars, groups)).encode())


def adopted(rui: etree._Element, source: etree._Element, macro: str) -> str:
    """Id the toolbar file runs a macro by, its own in the same file or that of a copy under a derived id with the icon its bitmap names."""
    guid, located = macro if source is rui else str(uuid.uuid5(uuid.UUID(rui.attrib["guid"]), macro)), "macros/macro_item[@guid = $guid]"
    if not rui.xpath(located, guid=guid):
        icons = element(rui, "icons")
        for held in source.xpath(located, guid=macro):
            copy = deepcopy(held)
            copy.set("guid", guid)
            element(rui, "macros").append(copy)
            icons.extend(deepcopy(icon) for icon in source.xpath("icons/icon[@guid = $icon]", icon=copy.get("bitmap_id", "")) if not icons.xpath("icon[@guid = $icon]", icon=icon.get("guid")))
    return guid


def button(rui: etree._Element, sources: Sequence[etree._Element], command: str, guid: uuid.UUID) -> etree._Element:
    """Button running the command alone, copied from a toolbar file's button or built on a macro, searching the toolbar file before the package's own."""
    macros = [(source, macro) for source in (rui, *sources) for macro in source.iter("macro_item") if macro.findtext("script", "").split() == ["!", f"_{command}"]]
    held = [(source, item) for source, macro in macros for item in source.iter("tool_bar_item") if item.findtext("left_macro_id", "").strip() == macro.attrib["guid"]]
    match held, macros:
        case [(source, item), *_], _:
            made = deepcopy(item)
            made[:] = [each for each in made if each.tag != "link"]
            for each in made.xpath("left_macro_id | right_macro_id"):
                each.text = adopted(rui, source, (each.text or "").strip())
        case [], [(source, macro), *_]:
            style = {} if macro.get("bitmap_id") else {"button_display_mode": "text_only"}
            made = E.tool_bar_item(E.text(E.locale_1033(macro.findtext("button_text/locale_1033", command))), E.left_macro_id(adopted(rui, source, macro.attrib["guid"])), **style)
        case _:
            element(rui, "macros").append(E.macro_item(E.button_text(E.locale_1033(command)), E.script(f"! _{command}"), guid=str(uuid.uuid5(uuid.UUID(rui.attrib["guid"]), command))))
            return button(rui, sources, command, guid)
    made.set("guid", str(guid))
    return made


def toolbar_roots(archive: Path) -> tuple[etree._Element, ...]:
    """Roots of the toolbar files a staged package archive holds."""
    with zipfile.ZipFile(archive) as contents:
        return tuple(etree.fromstring(contents.read(name)) for name in contents.namelist() if name.endswith(".rui"))


def toolbars(bundled: bytes, packages: Sequence[Package], cache: Path) -> etree._Element:
    """Bundled toolbar file with the ribbon group holding the ribbon tabs in order, and each package's commands after one spacer at the end of their tab, several opening as a flyout named by package and tab."""
    rui = etree.fromstring(bundled)
    namespace, tabs = uuid.UUID(rui.attrib["guid"]), {bar.attrib["guid"]: bar for bar in rui.iterfind("tool_bars/tool_bar")}
    (group,) = (group for group in rui.iterfind("tool_bar_groups/tool_bar_group") if group.xpath("tool_bar_group_item/tool_bar_id = $tab", tab=RibbonTab.STANDARD))
    items = {item.findtext("tool_bar_id"): item for item in group.iterfind("tool_bar_group_item")}
    group[:] = [*(each for each in group if each not in items.values()), *(items[tab] for tab in RibbonTab)]
    group.set("active_tool_bar_group", RibbonTab.STANDARD)
    sources = {package.id: toolbar_roots(package.archive(cache)) for package in packages if package.commands}
    placed = [(tab, package.id, commands) for package in packages for tab, commands in package.commands.items()]
    for tab in dict.fromkeys(tab for tab, *_ in placed):
        tabs[tab].append(E.tool_bar_item(guid=str(uuid.uuid5(namespace, tab)), button_style="spacer"))
    for tab, package, (head, *rest) in placed:
        base = uuid.uuid5(namespace, f"{tab}/{package}")
        made = button(rui, sources[package], head, base)
        if rest:
            flyout = E.tool_bar(
                E.text(E.locale_1033(f"{package} {tabs[tab].findtext(LABEL)}")),
                *(button(rui, sources[package], command, uuid.uuid5(base, command)) for command in (head, *rest)),
                guid=str(uuid.uuid5(base, package)),
            )
            element(rui, "tool_bars").append(flyout)
            made.append(E.link(flyout.attrib["guid"], style="normal"))
        tabs[tab].append(made)
    return rui


# --- [COMPOSITION] ----------------------------------------------------------------------


def edit(folder: Path, bundle: Bundle, measured: Measured, packages: Sequence[Package], cache: Path) -> tuple[Change, ...]:
    """Changes of the files edited once Rhino quit under the settings folder the report names, with the package toolbars from the cached archives."""
    rui = toolbars(bundle.path.joinpath("Contents", "Frameworks", "RhMaterialEditor.framework", "Versions", "A", "Resources", "assets", "default.rui").read_bytes(), packages, cache)
    main, toolbar_file = folder / "settings-Scheme__Default.xml", folder.parent / "UI" / "default.rui"
    held = main.read_bytes()
    own, plugins = settings(measured, registered(etree.fromstring(held)))
    files = {folder.parent / "Plug-ins" / name / "settings" / main.name: children for name, children in plugins.items()}
    changes = (written(toolbar_file, toolbar_file.read_bytes(), rui, shown), stated(main, held, own), *(stated(path, read(path), children) for path, children in files.items()))
    return tuple(change for change in changes if change is not None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["edit"]
