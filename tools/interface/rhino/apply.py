# ruff: file-ignore[django-extra, suspicious-xml-element-tree-usage, suspicious-xml-etree-import]
"""Applies the interface to Rhino: runs `script.py` inside Rhino through its document listener, rewrites the files Rhino reads at launch after a quit, and reads every value back after a relaunch."""

import codecs
from collections.abc import Callable, Mapping, Sequence
from copy import deepcopy
from datetime import timedelta
from enum import auto, StrEnum
from functools import partial, reduce
import hashlib
from itertools import pairwise, starmap
import math
from pathlib import Path
import struct
from typing import Final
import uuid
import xml.etree.ElementTree as ET

import anyio
from anyio.abc import SocketAttribute, SocketStream
from anyio.streams.stapled import MultiListener
from host import application, Applied, Change, DEADLINE, Error, Failed, Host, LOOPBACK, Outcome, parse, quit_application
import layout
from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client
from mcp.types import TextContent
import msgspec
import psutil
from rhino import window
from rhino.window import Extent, Panel, Site
from theme import Guide, Status, Text

# --- [TYPES] ----------------------------------------------------------------------------

type Edit = Callable[[ET.Element], None]


class Node(StrEnum):
    """Element and attribute names of the toolbar, container, and settings files the stage reads and writes."""

    MACRO_ITEM = auto()
    SCRIPT = auto()
    TOOL_BAR_ITEM = auto()
    TEXT = auto()
    LEFT_MACRO_ID = auto()
    RIGHT_MACRO_ID = auto()
    BUTTON_STYLE = auto()
    LINK = auto()
    TOOL_BAR_GROUP = auto()
    TOOL_BAR_GROUP_ITEM = auto()
    TOOL_BAR_ID = auto()
    DOCK_BARS = auto()
    DOCK_BAR = auto()
    PLACEMENT = auto()
    TABS = auto()
    NAME = auto()
    LOCALE_1033 = auto()
    TOOL_BAR = auto()
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


# --- [TABLES] ---------------------------------------------------------------------------

RETURN_TOP: Final = (
    Panel.ENVIRONMENTS,
    Panel.LIBRARIES,
    Panel.NOTES,
    Panel.NAMED_POSITIONS,
    Panel.NAMED_CPLANES,
    Panel.TEXTURES,
    Panel.RENDERING,
    Panel.GROUND_PLANE,
    Panel.LIGHTS,
    Panel.CONTEXT_HELP,
    Panel.BLOCK_CONTENT,
    Panel.FILE_EXPLORER,
)
RETURN_BOTTOM: Final = (Panel.LAYER_STATES,)

# --- [MODELS] ---------------------------------------------------------------------------


class Router(msgspec.Struct, frozen=True):
    """The router row's arguments."""

    args: tuple[str, ...]


class Servers(msgspec.Struct, frozen=True, rename={"router": "rhino-mcp-platform"}):
    """The server rows Rhino's run reads."""

    router: Router


class Environment(msgspec.Struct, frozen=True, rename="upper"):
    """Variables naming the user's home and the folder the router keeps its state in when one moves it."""

    home: str
    rhino_mcp_home: str | None = None


class Output(msgspec.Struct, frozen=True):
    """The text blocks of a `run_python` result merged: the script's streams, and the error and message of a raise."""

    stdout: str | None = None
    stderr: str | None = None
    error: str | None = None
    message: str | None = None
    guidance: str | None = None


class Reach(msgspec.Struct, frozen=True):
    """A live listener's process and port, and the application bundle the process runs from."""

    pid: int
    port: int
    bundle: Path


class Rhino(msgspec.Struct, frozen=True):
    """Rhino's bundle id and the folder its listeners announce into."""

    identifier: str
    listeners: anyio.Path


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [LISTENER]
async def running(rhino: Rhino) -> dict[int, Path]:
    """Bundle folder of each running process whose executable is the Rhino bundle's main executable, by process id."""
    return {
        process.pid: bundle.path
        for process in psutil.process_iter(["exe"])
        if (exe := process.info["exe"]) and (bundle := await application(Path(exe))) is not None and bundle.identifier == rhino.identifier
    }


async def reach(rhino: Rhino, pid: int, bundle: Path) -> Reach | None:
    """The process's lowest listening port that one listing of the folder names by its announcement or by the file RhinoAI renames over it."""
    names = {path.name async for path in rhino.listeners.iterdir()}
    ports = sorted(connection.laddr.port for connection in psutil.Process(pid).net_connections("tcp") if connection.status == psutil.CONN_LISTEN)
    return next((Reach(pid, port, bundle) for port in ports if not names.isdisjoint((f"{pid}-{port}.json", f"{pid}-{port}.json.tmp"))), None)


async def connected(ready: MultiListener[SocketStream]) -> int:
    """Rhino's process id, sent over the first connection the listener accepts."""
    send, receive = anyio.create_memory_object_stream[int](1)
    with send, receive:
        async with anyio.create_task_group() as group:

            async def handle(stream: SocketStream) -> None:
                async with stream:
                    send.send_nowait(int(await stream.receive()))
                group.cancel()

            await ready.serve(handle, group)
        return receive.receive_nowait()


async def launch(rhino: Rhino) -> Reach | None:
    """The running Rhino's listener, or with no Rhino process that of Rhino launched in the background running `window.py` as its startup script, which sends back its process id."""
    match next(iter((await running(rhino)).items()), None):
        case (pid, bundle):
            return await reach(rhino, pid, bundle)
        case None:
            async with await anyio.create_tcp_listener(local_host=LOOPBACK) as ready:
                endpoint = f"{window.READY_VARIABLE}={LOOPBACK}:{ready.extra(SocketAttribute.local_port)}"
                await anyio.run_process(["/usr/bin/open", "-g", "-b", rhino.identifier, "--env", endpoint, "--args", f'-runscript=_-ScriptEditor _Run "{window.__file__}"'])
                with anyio.move_on_after(DEADLINE):
                    pid = await connected(ready)
                    return await reach(rhino, pid, (await running(rhino))[pid])
            return None


async def run(host: Host, listener: Reach, name: str) -> Outcome:
    """Outcome of one function of `script.py` run through the listener, every module under the folder's module roots evicted from the listener's cache first."""
    roots = tuple(sorted({*[path.stem async for path in anyio.Path(host.folder).glob("*.py")], host.app}))
    source = (
        f"# env: {host.folder}\nimport runpy, sys\nsys.dont_write_bytecode = True\n"
        f"for name in [name for name in sys.modules if name.partition('.')[0] in {roots!r}]:\n    del sys.modules[name]\n"
        f"runpy.run_path({str(host.folder / host.app / 'script.py')!r})[{name!r}]()\n"
    )
    async with streamable_http_client(f"http://{LOOPBACK}:{listener.port}/") as (read, write, _), ClientSession(read, write, read_timeout_seconds=timedelta(seconds=DEADLINE)) as session:
        await session.initialize()
        result = await session.call_tool("run_python", {"script": source})
    output = msgspec.convert({key: value for block in result.content if isinstance(block, TextContent) for key, value in msgspec.json.decode(block.text, type=dict[str, str]).items()}, Output)
    match result.isError, output.stdout:
        case False, str() as stdout:
            return parse(host.app, stdout)
        case _:
            return Failed(host.app, tuple(line for text in (output.error, output.message, output.guidance, output.stderr) if text is not None for line in text.splitlines()))


async def documents(host: Host, listener: Reach) -> tuple[str, ...]:
    """Errors of the document step `script.py` runs before the quit, none once every document is saved or marked unmodified."""
    match await run(host, listener, "close"):
        case Failed(errors=errors):
            return errors
        case Applied():
            return ()


# --- [XML]
def tree(text: bytes) -> ET.Element:
    """The root of a document's tree with its comments kept."""
    parser = ET.XMLParser(target=ET.TreeBuilder(insert_comments=True))
    parser.feed(text)
    return parser.close()


def child(owner: ET.Element, tag: str, *keys: tuple[str, str]) -> ET.Element:
    """The owner's first child with the tag and attributes, created when absent."""
    match [each for each in owner.findall(tag) if all(each.get(name) == value for name, value in keys)]:
        case [found, *_]:
            return found
        case _:
            return ET.SubElement(owner, tag, dict(keys))


def nested(owner: ET.Element, *keys: str) -> ET.Element:
    """The owner's settings child at the key path, each level created when absent."""
    return reduce(lambda held, key: child(held, Node.CHILD, (Node.KEY, key)), keys, owner)


def entry(owner: ET.Element, key: str, *, value: bool | int | str) -> None:
    """The owner's settings entry holding the value's text."""
    child(owner, Node.ENTRY, (Node.KEY, key)).text = str(value)


def values(owner: ET.Element, key: str, texts: Sequence[str]) -> None:
    """The owner's settings entry holding the texts as its list."""
    element = child(owner, Node.ENTRY, (Node.KEY, key))
    for each in list(element):
        element.remove(each)
    listed = ET.SubElement(element, Node.LIST)
    for text in texts:
        ET.SubElement(listed, Node.VALUE).text = text


# --- [CONTAINERS]
def single(value: float) -> str:
    """A fraction as Rhino writes a band share: rounded to single precision and printed as the double it widens to."""
    return repr(struct.unpack("f", struct.pack("f", value))[0])


def panel_bars(containers: ET.Element, panel: Panel) -> list[ET.Element]:
    """The dock bars whose tabs hold the panel."""
    return [bar for bar in containers.iterfind(f"{Node.DOCK_BARS}/{Node.DOCK_BAR}") if bar.find(f"{Node.TABS}/{Node.PANEL}[@{Node.GUID}='{panel}']") is not None]


def bottom_bar(containers: ET.Element, top: ET.Element) -> ET.Element:
    """The container holding Layers apart from the top one, created from the top one's placement when Layers shares it."""
    match [bar for bar in panel_bars(containers, Panel.LAYERS) if bar is not top]:
        case [held, *_]:
            return held
        case _:
            title = "Layers"
            bar = ET.SubElement(child(containers, Node.DOCK_BARS), Node.DOCK_BAR, {Node.GUID: str(uuid.uuid5(uuid.UUID(top.attrib[Node.GUID]), Panel.LAYERS))})
            bar.extend(deepcopy(placement) for placement in top.findall(Node.PLACEMENT))
            tabs = ET.SubElement(bar, Node.TABS, {Node.NAME: title, Node.SELECTED_ITEM: Panel.LAYERS, Node.DISPLAY_STYLE: child(top, Node.TABS).attrib[Node.DISPLAY_STYLE]})
            ET.SubElement(ET.SubElement(tabs, Node.NAME), Node.LOCALE_1033).text = title
            return bar


def split(sites: ET.Element, measures: Mapping[str, float], side: Site) -> tuple[float, float]:
    """A side band's height once the top and bottom bands take their sizes, less the resizer between its two containers, and the upper container's part of it, the lower one taking Layers' rows on the right and Osnap's preferred height on the left."""
    resizer = measures[Extent.RESIZER]
    stacked = sum(
        measures[site] - sum(float(band.attrib[Node.SIZE]) + resizer for band in sites.iterfind(f"{Node.DOCK_SITE}[@{Node.LOCATION}='{site}']/{Node.BAND}")) for site in (Site.TOP, Site.BOTTOM)
    )
    lower = measures[Extent.LAYERS_CHROME] + measures[Extent.LAYERS_HEADER] + layout.ORGANIZER_ROWS * measures[Extent.LAYERS_ROW] if side is Site.RIGHT else measures[Extent.OSNAP]
    height = measures[side] + stacked - resizer
    return height, height - lower


def write_containers(containers: ET.Element, shipped: ET.Element, measures: Mapping[str, float]) -> None:
    """Ribbon tabs in Rhino's order, held rows as Rhino wrote them, bands sized from the measures, Layers at its rows' height, Osnap under the sidebar at its content height, history at the bottom, return homes, stray bars removed, each side's share over its band and the resizer Rhino lays out on."""
    sidebar_columns, half_point = 6, 0.5
    cell = round(measures[Extent.BUTTON])
    owners = child(containers, Node.DOCK_BARS)
    factory = next(
        bar
        for bar in shipped.iterfind(f"{Node.DOCK_BARS}/{Node.DOCK_BAR}")
        if bar.find(f"{Node.PLACEMENT}[@{Node.DOCK_LOCATION}='{Site.TOP}']") is not None and bar.find(f"{Node.TABS}/{Node.TOOL_BAR}") is not None
    )
    ribbon = next(owners.iterfind(f"{Node.DOCK_BAR}[@{Node.GUID}='{factory.attrib[Node.GUID]}']"))
    ribbon_tabs, factory_tabs = child(ribbon, Node.TABS), child(factory, Node.TABS)
    held = {row.get(Node.GUID): row for row in ribbon_tabs.findall(Node.TOOL_BAR)}
    for row in held.values():
        ribbon_tabs.remove(row)
    ribbon_tabs.extend(held.get(row.get(Node.GUID), deepcopy(row)) for row in factory_tabs.findall(Node.TOOL_BAR))
    ribbon_tabs.set(Node.SELECTED_ITEM, factory_tabs.attrib[Node.SELECTED_ITEM])
    head, *_ = window.RIGHT_TOP
    top = panel_bars(containers, head)[0]
    bottom = bottom_bar(containers, top)
    columns = ((top, window.RIGHT_TOP), (bottom, window.RIGHT_BOTTOM))
    panels = {panel.get(Node.GUID): panel for bar in owners.findall(Node.DOCK_BAR) for panel in bar.findall(f"{Node.TABS}/{Node.PANEL}")}
    for bar, order in columns:
        tabs = child(bar, Node.TABS)
        for panel in tabs.findall(Node.PANEL):
            tabs.remove(panel)
        tabs.extend(panels[panel] for panel in order if panel in panels)
        tabs.set(Node.SELECTED_ITEM, order[0])
    docked = {panel_bars(containers, Panel.OSNAP)[0]: (Site.LEFT, 1), next(owners.iterfind(f"{Node.DOCK_BAR}[@{Node.GUID}='{Panel.COMMAND_HISTORY}']")): (Site.BOTTOM, 0)}
    sites = child(containers, Node.DOCK_SITES)
    bands = {site: child(child(sites, Node.DOCK_SITE, (Node.LOCATION, site)), Node.BAND) for site in (Site.LEFT, Site.RIGHT, Site.BOTTOM)}
    banded = {row.get(Node.GUID) for row in sites.iterfind(f"{Node.DOCK_SITE}/{Node.BAND}/{Node.DOCK_BAR}")}
    for bar in [bar for bar in owners.findall(Node.DOCK_BAR) if bar not in {top, bottom, *docked} and bar.get(Node.GUID) not in banded and bar.find(f"{Node.TABS}/{Node.PANEL}") is not None]:
        owners.remove(bar)
    placed = [(bar, slot, None) for slot, (bar, _) in enumerate(columns)] + [(bar, slot, side) for bar, (side, slot) in docked.items()]
    for bar, slot, side in placed:
        for placement in bar.findall(Node.PLACEMENT):
            placement.attrib.update({Node.DOCKED_PLACEMENT: f"0,{slot}", Node.VISIBLE: str(True), **({} if side is None else {Node.DOCK_LOCATION: side, Node.RECENT_DOCK_LOCATION: side})})
    for element in bands[Site.RIGHT].findall(Node.DOCK_BAR):
        bands[Site.RIGHT].remove(element)
    bands[Site.RIGHT].extend(ET.Element(Node.DOCK_BAR, {Node.GUID: bar.attrib[Node.GUID]}) for bar, _ in columns)
    moved, rows = {bar.attrib[Node.GUID] for bar in docked}, {}
    for site_band in sites.iterfind(f"{Node.DOCK_SITE}/{Node.BAND}"):
        for row in [each for each in site_band.findall(Node.DOCK_BAR) if each.get(Node.GUID) in moved]:
            site_band.remove(row)
            rows[row.attrib[Node.GUID]] = row
    for bar, (side, slot) in docked.items():
        bands[side].insert(slot, rows.get(bar.attrib[Node.GUID], ET.Element(Node.DOCK_BAR, {Node.GUID: bar.attrib[Node.GUID]})))
    ribbon_band = next(band for band in sites.iterfind(f"{Node.DOCK_SITE}/{Node.BAND}") if band.find(f"{Node.DOCK_BAR}[@{Node.GUID}='{ribbon.attrib[Node.GUID]}']") is not None)
    sized = (
        (ribbon_band, round(measures[Extent.TAB_STRIP]) + cell, 1),
        (bands[Site.LEFT], sidebar_columns * cell, 0),
        (bands[Site.RIGHT], layout.RIGHT_COLUMN, 0),
        (bands[Site.BOTTOM], layout.BOTTOM_STRIP, 1),
    )
    for band, size, axis in sized:
        band.set(Node.SIZE, str(size))
        for placement in (placement for row in band.findall(Node.DOCK_BAR) for placement in owners.iterfind(f"{Node.DOCK_BAR}[@{Node.GUID}='{row.get(Node.GUID)}']/{Node.PLACEMENT}")):
            placement.set(Node.DOCK_BAND_SIZE, ",".join(str(size) if index == axis else extent for index, extent in enumerate(placement.attrib[Node.DOCK_BAND_SIZE].split(","))))
    for site in sites.iterfind(Node.DOCK_SITE):
        site.set(Node.AUTO_HIDE, str(False))
        for empty in [each for each in site.findall(Node.BAND) if not list(each)]:
            site.remove(empty)
    for side in (Site.RIGHT, Site.LEFT):
        height, upper = split(sites, measures, side)
        share = (upper + half_point) / (height + measures[Extent.RESIZER])
        for row, fraction in zip(bands[side].findall(Node.DOCK_BAR), (share, 1 - share), strict=True):
            row.set(Node.SIZE, single(fraction))
    returns = child(containers, Node.LAST_COLLECTION_PANEL_WAS_IN)
    homes = {**dict.fromkeys((*window.RIGHT_TOP, *RETURN_TOP), top), **dict.fromkeys((*window.RIGHT_BOTTOM, *RETURN_BOTTOM), bottom)}
    for home, holder in homes.items():
        child(returns, Node.ITEM, (Node.GUID, home)).set(Node.DOCK_BAR, holder.attrib[Node.GUID])


# --- [SETTINGS]
def layer_lists(visible: Sequence[str]) -> tuple[tuple[str, ...], tuple[str, ...], tuple[str, ...]]:
    """Order, width, and visibility lists of a column group showing the columns in order, Name taking the rest of the band."""
    stretch = "Name"
    columns = (
        stretch,
        "Current",
        "Visible",
        "Locked",
        "Color",
        "Material",
        "Linetype",
        "PrintColor",
        "PrintWidth",
        "Section",
        "NewDetailOn",
        "ViewportVisible",
        "ViewportColor",
        "ViewportPrintColor",
        "ViewportPrintWidth",
        "Description",
    )
    text_widths, icon_width, inset = {"Material": 80, "Linetype": 70, "PrintWidth": 70, "Section": 80, "ViewportPrintWidth": 70, "Description": 80}, 20, 36
    order = (*visible, *(name for name in columns if name not in visible))
    widths = {name: text_widths.get(name, icon_width) for name in columns[1:]}
    widths[stretch] = 5 * math.floor((layout.RIGHT_COLUMN - inset - sum(widths[name] for name in visible if name != stretch)) / 5)
    return (tuple(str(order.index(name)) for name in columns), tuple(str(widths[name]) for name in columns), tuple(str(int(name in visible)) for name in columns))


def write_settings(settings: ET.Element) -> None:
    """The Layers panel column groups and header sorting, the Layouts panel widths, the edge continuity colors, the Named Views command memory, and neither startup commands nor an antialiasing override."""
    groups = (
        ("LayerColumnGroup.Model", ("Name", "Current", "Locked", "Color", "Material", "ViewportVisible", "Visible")),
        ("LayerColumnGroup.Viewport", ("Name", "Current", "ViewportVisible", "NewDetailOn", "Locked", "Color", "ViewportColor", "ViewportPrintColor", "Visible")),
    )
    continuity_colors = (("BadHairColor", Status.ERROR), ("GoodHairColor", Text.SECONDARY), ("MaxHairColor", Guide.CONSTRUCTION), ("TextColor", Text.PRIMARY))
    root = child(settings, Node.SETTINGS)
    layers, options = nested(root, "LayersPanel"), nested(root, "Options")
    entry(layers, "ColumnSorting", value=False)
    for name, visible in groups:
        group = nested(layers, name)
        group.set(Node.HIDDEN, str(True))
        for key, texts in zip(("Order", "Width", "Visible"), layer_lists(visible), strict=True):
            values(group, key, texts)
    layouts = nested(root, "LayoutsPanel")
    values(layouts, "Width", ("80", "25", "50", "105"))
    entry(layouts, "Expanded", value=True)
    continuity = nested(options, "EdgeContinuity")
    for name, rgb in continuity_colors:
        entry(continuity, name, value=",".join(map(str, (255, *rgb))))
    entry(child(settings, Node.COMMAND, (Node.NAME, "NamedView")), "Thumbnails", value=False)
    for owner, name in (("General", "StartupCommands"), ("Display", "MSAASampleCount")):
        for held in options.iterfind(f"{Node.CHILD}[@{Node.KEY}='{owner}']"):
            for stale in held.findall(f"{Node.ENTRY}[@{Node.KEY}='{name}']"):
                held.remove(stale)


def write_rdk(settings: ET.Element, measures: Mapping[str, float]) -> None:
    """The docked material, environment, and texture editors in list view at the smallest list size, the material editor labeled, unitless, auto-updating, and its list, above the editor in Rhino's default layout, at its strip and whole rows."""
    material_rows = 4
    editors = nested(child(settings, Node.SETTINGS), "Settings", "Editors")
    material, environment, texture = (nested(editors, node) for node in ("{0F0FB7B6-C7D9-0E70-B65A-BFEF546316A9}", "{24C22CED-5138-0E70-B573-933DE4DD0F5A}", "{4A5570F9-7CF0-0E70-A3F5-B5272154837C}"))
    for editor in (material, environment, texture):
        editor.set(Node.HIDDEN, str(True))
        entry(editor, "PreviewMode", value="List")
        entry(editor, "ListPreviewSizePercent", value=5)
    for name, shown in (("ShowLabels", True), ("ShowUnits", False), ("AutoUpdate", True)):
        entry(material, name, value=shown)
    entry(material, "SplitterHorzLayoutA", value=round(measures[Extent.MATERIALS_STRIP] + material_rows * measures[Extent.MATERIALS_ROW]))


def write_libraries(settings: ET.Element, containers: ET.Element, measures: Mapping[str, float]) -> None:
    """The Libraries splitter at the share of its span, the top container the written containers give less its chrome, that the folder tree's whole rows take above the file list's minimum, the file list below taking the rest."""
    border, pitch = measures[Extent.LIBRARIES_BORDER], measures[Extent.LIBRARIES_ROW]
    _, top = split(child(containers, Node.DOCK_SITES), measures, Site.RIGHT)
    span = top - measures[Extent.LIBRARIES_CHROME]
    folders = border + pitch * min(measures[Extent.LIBRARIES_FOLDERS], math.floor((span - measures[Extent.LIBRARIES_LIST_MINIMUM] - border) / pitch))
    libraries = nested(child(settings, Node.SETTINGS), "Libraries")
    libraries.set(Node.HIDDEN, str(True))
    entry(libraries, "splitter", value=repr(folders / span))


def write_snapshots(settings: ET.Element) -> None:
    """The Snapshots panel in list view."""
    entry(child(settings, Node.SETTINGS), "Thumbnails", value=False)


# --- [FILES]
def digest(root: ET.Element) -> str:
    """Short digest of a tree's canonical content, blind to the indentation Rhino writes and a rebuilt list drops."""
    return hashlib.sha256(ET.canonicalize(ET.tostring(root, encoding="unicode"), strip_text=True).encode()).hexdigest()[:12]


def staged(path: Path, edit: Edit) -> Change | Error | None:
    """The file's change when its edit changed it and it was written, an error when a second edit of the written content changes it again, an absent file read as empty settings."""
    held = path.read_bytes() if path.exists() else codecs.BOM_UTF8 + ET.tostring(ET.Element(Node.SETTINGS, {Node.ID: "2.0"}))
    root = tree(held)
    before = digest(root)
    edit(root)
    written = (codecs.BOM_UTF8 if held.startswith(codecs.BOM_UTF8) else b"") + ET.tostring(root, encoding="utf-8", xml_declaration=True)
    again = tree(written)
    edit(again)
    if (after := digest(root)) != before:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(written)
    if digest(again) != after:
        return Error(f"{path.name} changes again under its own edit")
    return None if after == before else Change(f"file {Path(*path.parts[-3:])}", before, after)


def presented(rui: ET.Element) -> str:
    """Short digest of what a toolbar file shows: each toolbar's label and its items' scripts, style, and flyout, and each group's toolbars, blind to the item ids, macro copies, and text order Rhino writes at quit."""
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
                for item in bar.iterfind(Node.TOOL_BAR_ITEM)
            ),
        )
        for bar in rui.iter(Node.TOOL_BAR)
    )
    groups = sorted((group.get(Node.GUID, ""), tuple(item.findtext(Node.TOOL_BAR_ID, "") for item in group.iter(Node.TOOL_BAR_GROUP_ITEM))) for group in rui.iter(Node.TOOL_BAR_GROUP))
    return hashlib.sha256(repr((bars, groups)).encode()).hexdigest()[:12]


def restored(path: Path, shipped: ET.Element) -> Change | None:
    """The toolbar file's change when what it shows differs from the file Rhino ships and it was deleted, for Rhino to write its own at launch."""
    held, factory = (presented(tree(path.read_bytes())) if path.exists() else None), presented(shipped)
    if held is None or held == factory:
        return None
    path.unlink()
    return Change(f"file {Path(*path.parts[-3:])}", held, factory)


def apply_files(app: str, bundle: Path, settings: Path, measures: Mapping[str, float]) -> tuple[Change, ...] | Failed:
    """Changes of the toolbar file and of each file the stage edits, the containers and panel splitters sized from the in-process measures and the Libraries splitter from the written containers, failed naming a file whose edit does not settle."""
    plugins, scheme = settings.parent / "Plug-ins", "Scheme__Default"
    file, containers = f"settings-{scheme}.xml", settings / scheme / "containers.xml"
    assets = bundle.joinpath("Contents", "Frameworks", "RhMaterialEditor.framework", "Versions", "A", "Resources", "assets")
    toolbars = restored(settings.parent / "UI" / "default.rui", tree((assets / "default.rui").read_bytes()))
    placed = staged(containers, partial(write_containers, shipped=tree((assets / "default.rhw").read_bytes()), measures=measures))
    plugin_edits = (
        ("Renderer Development Kit (16592d58-4a2f-401d-bf5e-3b87741c1b1b)", partial(write_rdk, measures=measures)),
        ("RDK_EtoUI (638a0098-0511-482b-95bf-8cf47fd32c17)", partial(write_libraries, containers=tree(containers.read_bytes()), measures=measures)),
        ("Snapshots (73b88f43-c32c-4306-93b5-1d0082fffee8)", write_snapshots),
    )
    stages: tuple[tuple[Path, Edit], ...] = ((settings / file, write_settings), *((plugins / folder / "settings" / file, edit) for folder, edit in plugin_edits))
    results = (placed, *starmap(staged, stages))
    failures = tuple(result.text for result in results if isinstance(result, Error))
    return Failed(app, failures) if failures else tuple(result for result in (toolbars, *results) if isinstance(result, Change))


# --- [RELAUNCH]
def proven(settled: Applied, changes: tuple[Change, ...], proof: Outcome) -> Outcome:
    """The settings outcome with the file changes and the relaunched read, failed when the read failed."""
    match proof:
        case Applied():
            return msgspec.structs.replace(settled, changes=(*settled.changes, *changes, *proof.changes), skipped=(*settled.skipped, *proof.skipped))
        case Failed():
            return proof


async def relaunch(host: Host, rhino: Rhino, listener: Reach, settled: Applied) -> Outcome:
    """Quit Rhino, rewrite the files it reads at launch, relaunch it, and read everything back."""
    if errors := await documents(host, listener) or await quit_application(rhino.identifier, [psutil.Process(listener.pid)]):
        return Failed(host.app, errors)
    files = await anyio.to_thread.run_sync(apply_files, host.app, listener.bundle, Path(settled.settings), settled.measures)
    relaunched = await launch(rhino)
    match files, relaunched:
        case Failed() as failed, _:
            return failed
        case _, None:
            return Failed(host.app, (f"no listener announced in {rhino.listeners} after the relaunch",))
        case changes, found:
            return proven(settled, changes, await run(host, found, "layout"))


async def settle(host: Host, rhino: Rhino) -> Outcome:
    """The settings inside a live Rhino, then the file stage and the relaunched read."""
    await rhino.listeners.mkdir(parents=True, exist_ok=True)
    match await launch(rhino):
        case None:
            return Failed(host.app, (f"no listener announced in {rhino.listeners}",))
        case listener:
            match await run(host, listener, "main"):
                case Applied() as settled:
                    return await relaunch(host, rhino, listener, settled)
                case Failed() as failed:
                    return failed


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> Outcome:
    """Rhino's outcome, failed when the router row names no version."""
    arguments = msgspec.json.decode(host.servers, type=Servers).router.args
    environment = msgspec.convert(host.environ, Environment)
    home = anyio.Path(Path(environment.home, "Library", "Application Support", "McNeel") if environment.rhino_mcp_home is None else environment.rhino_mcp_home, "rhino-mcp")
    match next((value for flag, value in pairwise(arguments) if flag == "--default-version"), None):
        case None:
            return Failed(host.app, (f"router arguments {arguments} name no --default-version",))
        case version:
            return await settle(host, Rhino(f"com.mcneel.rhinoceros.{version}", home / "listeners"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
