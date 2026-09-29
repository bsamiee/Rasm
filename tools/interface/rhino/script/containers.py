# ty: ignore[unresolved-import, unresolved-attribute, unsupported-operator, invalid-argument-type, invalid-return-type, no-matching-overload]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, operator, arg-type, no-any-return, dict-item"
# ruff: file-ignore[banned-api, suspicious-xml-etree-import, suspicious-xml-element-tree-usage]
"""Rhino's content panel settings, the measures of its window, and the window layout restored from the one the measures size."""

from collections.abc import Callable, Iterator, Mapping
from copy import deepcopy
from itertools import starmap
import json
from pathlib import Path
import tempfile
import uuid
import xml.etree.ElementTree as ET

import clr
from Eto.Forms import Control, Splitter, TreeGridView
import Rhino
from Rhino.DocObjects.Tables import RestoreLayerProperties
from Rhino.PlugIns import PlugIn
from Rhino.Render import SupportOptions
from Rhino.Runtime import HostUtils, NamedParametersEventArgs
from Rhino.UI import Panels, RhinoEtoApp
import System
from System import Array, String, UInt32
from System.Drawing import ColorTranslator
from System.IO import MemoryStream
from System.Reflection import BindingFlags

from interface.frame import Task
from interface.render import MATERIALS
from interface.report import changes, Kind, line, Row
from interface.rhino.markup import canonical, element
from interface.rhino.script.accessors import absent, color, guid, Internal, key, unsigned
from interface.rhino.window import Band, Bar, Extent, Layout, layout, Measured, PanelId, RibbonTab, Site
from interface.roles import Surface

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [MEASURES]
def measured(doc: Rhino.RhinoDoc, bundled: ET.Element) -> Measured:
    """Dock site heights, extents, and Layers column default widths of the window, the Osnap chrome under a lone side-docked panel's gripper, and a contentless Command History bar at its bundled band height, each opened panel closed again."""
    bars, serial = Internal.TAB_PANEL_DOCK_BARS.type, UInt32(doc.RuntimeSerialNumber)
    dock_sites = Internal.TAB_PANEL_DOCK_SITES.type.GetMethod("FromDocument", Array[System.Type]([clr.GetClrType(Rhino.RhinoDoc)])).Invoke(None, Array[System.Object]([doc]))

    def container(bar: System.Guid) -> Control | None:
        """Dock bar's control at the size its band gives it, None while the bar shows no content."""
        return bars.GetMethod("FromDockBarId").Invoke(None, Array[System.Object]([bar])).HasContent(serial)

    def contained(control: object, kind: System.Type) -> object:
        """First descendant control of the kind."""
        return next(each for each in control.Children if kind.IsInstanceOfType(each))

    def inset(shell: object, grid: object) -> float:
        """Width the container spends beside the tree grid's visible columns."""
        outline = grid.ControlObject
        columns = tuple(outline.TableColumns())
        shown = [index for index, column in enumerate(columns) if not column.Hidden]
        span = outline.RectForColumn(System.IntPtr(max(shown))).Right.Value - sum(columns[index].Width.Value for index in shown)
        return shell.Size.Width - outline.EnclosingScrollView.ContentView.Frame.Size.Width.Value + span

    def private(owner: object, kind: System.Type, name: str) -> object:
        """Value of the private instance field the type declares on the owner."""
        return kind.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner)

    def layers(panel: object, shell: object) -> dict[Extent, float]:
        """Measures of the Layers container and its tree."""
        grid = contained(panel, Internal.LAYER_TREE_GRID_VIEW.type)
        outline = grid.ControlObject
        return {
            Extent.LAYERS_CHROME: shell.Size.Height - grid.Size.Height,
            Extent.LAYERS_HEADER: outline.HeaderView.Frame.Size.Height.Value,
            Extent.LAYERS_ROW: outline.RowHeight.Value + outline.IntercellSpacing.Height.Value,
            Extent.LAYERS_INSET: inset(shell, grid),
        }

    def materials(panel: object, _: object) -> dict[Extent, float]:
        """Material editor's strip above its list and its row pitch."""
        editor = panel.GetType().BaseType
        thumbnails = private(panel, editor, "m_thumbnail_list")
        thumbview = private(thumbnails, thumbnails.GetType(), "m_thumbview")
        return {
            Extent.MATERIALS_STRIP: private(panel, editor, "m_thumb_list_table").Spacing.Height + private(panel, editor, "m_view_mode_button_table").GetPreferredSize().Height,
            Extent.MATERIALS_ROW: thumbnails.ViewModel.ThumbHeigth + private(thumbview, thumbview.GetType(), "m_space_between_items"),
        }

    def libraries(panel: object, shell: object) -> dict[Extent, float]:
        """Measures of the Libraries container, its folder tree, and its list."""
        splitter, tree = contained(panel, clr.GetClrType(Splitter)), contained(panel, clr.GetClrType(TreeGridView))
        folders = tree.ControlObject
        return {
            Extent.LIBRARIES_CHROME: shell.Size.Height - (splitter.Height - splitter.SplitterWidth),
            Extent.LIBRARIES_BORDER: tree.Height - folders.EnclosingScrollView.ContentView.Frame.Size.Height.Value,
            Extent.LIBRARIES_ROW: folders.RowHeight.Value + folders.IntercellSpacing.Height.Value,
            Extent.LIBRARIES_FOLDERS: folders.RowCount.ToInt64(),
            Extent.LIBRARIES_LIST_MINIMUM: splitter.Panel2MinimumSize,
        }

    def osnap(panel: object, _: object) -> dict[Extent, float]:
        """Osnap grid's toggle count, pitch, and inset from the panel's own metrics, and its chrome under the gripper a lone side-docked panel shows."""
        kind, hidden = panel.GetType(), BindingFlags.NonPublic
        spacing = 4 if kind.GetMethod("UseToggleButtons", BindingFlags.Instance | hidden).Invoke(panel, None) else 0
        cell = private(panel, kind, "m_control_size")
        padding = kind.GetMethod("GeometryPadding", BindingFlags.Static | hidden).Invoke(None, Array[System.Object]([System.Enum.Parse(kind.GetNestedType("LayoutGeometry", hidden), "Grid")]))
        gripper = 2 if Internal.TAB_PANEL_SETTINGS.type.GetProperty("LockDockedWindows").GetValue(None) else Internal.TAB_PANEL_GRIPPER_CONTROL.type.GetProperty("FixedHeight").GetValue(None)
        return {
            Extent.OSNAP_TOGGLES: private(panel, kind, "m_item_count"),
            Extent.OSNAP_PITCH_X: cell.Width + spacing,
            Extent.OSNAP_PITCH_Y: cell.Height + spacing,
            Extent.OSNAP_INSET: padding.Horizontal,
            Extent.OSNAP_CHROME: gripper + padding.Vertical,
        }

    top, window = Panels.PanelDockBar(guid(PanelId.PROPERTIES)), RhinoEtoApp.MainWindowForDocument(doc).ControlObject.ContentView
    homes = {panel: Panels.PanelDockBar(guid(panel)) for panel in (PanelId.OSNAP, PanelId.LAYERS, PanelId.LAYOUTS, PanelId.MATERIALS, PanelId.LIBRARIES)}

    def selected(panel: PanelId, measure: Callable[[object, object], dict[Extent, float]]) -> dict[Extent, float]:
        """Measures of the panel laid out as its container's selected tab."""
        identity = guid(panel)
        Panels.OpenPanel(top if homes[panel] == System.Guid.Empty else homes[panel], identity, makeSelectedPanel=True)
        window.LayoutSubtreeIfNeeded()
        return measure(Panels.GetPanel(identity, doc), container(Panels.PanelDockBar(identity)))

    try:
        window.LayoutSubtreeIfNeeded()
        match container(guid(Bar.COMMAND_HISTORY)):
            case None:
                history = int(next(band for band in bundled.iterfind("dock_sites/dock_site/band") if band.find(f"dock_bar[@guid='{Bar.COMMAND_HISTORY}']") is not None).attrib["size"])
            case shown:
                history = shown.Size.Height
        toolbar = Internal.TOOLBAR_SETTINGS.type.GetProperty("Instance").GetValue(None)
        style = Internal.TAB_PANEL_SETTINGS.type.GetProperty("HorizontalDisplayStyle").GetValue(None)
        return Measured(
            sites={site: getattr(dock_sites, site).Control.Size.Height for site in Site},
            extents={
                Extent.TAB_STRIP: Internal.BASE_TAB_CONTROL.type.GetMethod("CalculateTabHeight", BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([style])),
                Extent.BUTTON: toolbar.Buttons.TotalButtonSize,
                Extent.RESIZER: Internal.DOCK_SITE_RESIZER.type.GetProperty("ResizerWidth").GetValue(None),
                Extent.COMMAND_HISTORY: history,
                **selected(PanelId.OSNAP, osnap),
                **selected(PanelId.LAYERS, layers),
                **selected(PanelId.LAYOUTS, lambda panel, shell: {Extent.LAYOUTS_INSET: inset(shell, contained(panel, Internal.LAYOUT_TREE_GRID_VIEW.type))}),
                **selected(PanelId.MATERIALS, materials),
                **selected(PanelId.LIBRARIES, libraries),
            },
            columns={
                str(column): Internal.LAYER_COLUMNS.type.GetMethod("DefaultWidth").Invoke(None, Array[System.Object]([column])) for column in System.Enum.GetValues(Internal.LAYER_COLUMN_TYPE.type)
            },
        )
    finally:
        for panel in (panel for panel, home in homes.items() if home == System.Guid.Empty):
            Panels.ClosePanel(guid(panel), doc)
        Panels.OpenPanel(top, guid(PanelId.PROPERTIES), makeSelectedPanel=True)


# --- [LAYOUT]
def exported(doc: Rhino.RhinoDoc) -> ET.Element:
    """Live window layout of the document's window under the Modeling name, as the writer of `containers.xml` serializes it."""
    stream = MemoryStream()
    try:
        Internal.WINDOW_LAYOUT_FILE.type.GetMethod("Write").Invoke(None, Array[System.Object]([doc, stream, Task.MODELING.value, None, False]))
        return ET.fromstring(bytes(stream.ToArray()))
    finally:
        stream.Dispose()


def bundled_layout() -> ET.Element:
    """Default window layout Rhino.UI includes."""
    stream, held = Internal.TAB_PANEL_DOCK_BARS.type.Assembly.GetManifestResourceStream("Rhino.UI.Resources.rui.default.rhw"), MemoryStream()
    try:
        stream.CopyTo(held)
        return ET.fromstring(bytes(held.ToArray()))
    finally:
        stream.Dispose()
        held.Dispose()


def resolved(root: ET.Element, plan: Layout) -> dict[Bar | tuple[PanelId, ...], ET.Element]:
    """Dock bar of each bar the layout's bands name, a panel container no bar holds created on its band's first held bar's placement."""
    bars, held = list(root.iterfind("dock_bars/dock_bar")), dict[Bar | tuple[PanelId, ...], ET.Element]()

    def created(head: PanelId) -> str:
        """Id of the container the layout creates for the panel it holds first."""
        return str(uuid.uuid5(uuid.UUID(head), Task.MODELING))

    def holds(bar: ET.Element, name: Bar | tuple[PanelId, ...]) -> bool:
        """Whether the dock bar is the named bar, or a free bar holding the container's first panel or carrying its created id."""
        identity = bar.get("guid")
        match name:
            case Bar():
                return identity == name
            case _:
                return bar not in held.values() and (identity == created(name[0]) or bar.find(f"tabs/panel[@guid='{name[0]}']") is not None)

    for band_bars, name in ((band["bars"], name) for band in plan["bands"].values() for name, _ in band["bars"]):
        holders, siblings = [bar for bar in bars if holds(bar, name)], [held[other] for other, _ in band_bars if other in held]
        if holders:
            held[name] = holders[0]
        elif isinstance(name, tuple) and siblings:
            held[name] = ET.SubElement(element(root, "dock_bars"), "dock_bar", {"guid": created(name[0])})
            held[name].extend(deepcopy(placement) for placement in siblings[0].iterfind("placement"))
            ET.SubElement(held[name], "tabs")
    return held


def lacking(held: Mapping[Bar | tuple[PanelId, ...], ET.Element], plan: Layout, bundled_tabs: Mapping[str, ET.Element]) -> tuple[str, ...]:
    """Name of each bar the layout's bands name that no dock bar holds, and of each ribbon tab neither the ribbon nor the bundled layout holds."""
    live = {row.attrib["guid"] for name, bar in held.items() if name == Bar.RIBBON for row in bar.iterfind("tabs/tool_bar")}
    return (
        *(f"dock bar {name.name}" if isinstance(name, Bar) else f"container of {name[0].name}" for band in plan["bands"].values() for name, _ in band["bars"] if name not in held),
        *(f"ribbon tab {tab.name}" for tab in RibbonTab if tab not in live and tab not in bundled_tabs),
    )


def band_element(sites: ET.Element, band: Band, held: Mapping[Bar | tuple[PanelId, ...], ET.Element]) -> ET.Element:
    """Band holding the layout band's bars in order at their shares, keeping the attributes Rhino wrote on the first bar's band and on each row."""
    banded = {row.attrib["guid"]: (owner, row) for owner in sites.iterfind("dock_site/band") for row in owner}
    guids = [held[name].attrib["guid"] for name, _ in band["bars"]]
    made = ET.Element("band", {**(banded[guids[0]][0].attrib if guids[0] in banded else {}), "size": str(band["size"])})
    for bar, (_, share) in zip(guids, band["bars"], strict=True):
        row = ET.SubElement(made, "dock_bar", dict(banded[bar][1].attrib) if bar in banded else {"guid": bar})
        if share is not None:
            row.set("size", repr(share))
    return made


def arranged(root: ET.Element, held: Mapping[Bar | tuple[PanelId, ...], ET.Element], plan: Layout, bundled_tabs: Mapping[str, ET.Element]) -> None:
    """Edit the exported layout into the plan with the command prompt in the sidebar, every other dock bar hidden and holding no panel, and a Right dock location left unwritten as Rhino's writer leaves its default."""
    panels, ribbon = {row.attrib["guid"]: row for row in root.iterfind("dock_bars/dock_bar/tabs/panel")}, held[Bar.RIBBON]
    live = {row.attrib["guid"]: row for row in ribbon.iterfind("tabs/tool_bar")}
    placed = {held[name]: (site, band["size"], slot) for site, band in plan["bands"].items() for slot, (name, _) in enumerate(band["bars"])}
    element(root, "dock_bars").attrib.update({"command_location": "SideBar", "command_style": "Graphical"})
    for tabs in root.iterfind("dock_bars/dock_bar/tabs"):
        tabs[:] = [row for row in tabs if row.tag != "panel"]
    for name, tabs in ((name, element(bar, "tabs")) for name, bar in held.items() if isinstance(name, tuple)):
        tabs.extend(deepcopy(panels[panel]) if panel in panels else ET.Element("panel", {"guid": panel}) for panel in name)
        tabs.set("selected_item", name[0])
    tabs = element(ribbon, "tabs")
    tabs[:] = [*(row for row in tabs if row.tag != "tool_bar"), *(live[tab] if tab in live else deepcopy(bundled_tabs[tab]) for tab in RibbonTab)]
    tabs.set("selected_item", RibbonTab.STANDARD)
    sites = element(root, "dock_sites")
    bands = {site: band_element(sites, band, held) for site, band in plan["bands"].items()}
    for site, band in bands.items():
        owner = element(sites, "dock_site", location=site)
        owner[:] = [*(each for each in owner if each.tag != "band"), band]
        owner.set("auto_hide", str(False))
    for bar, placement in ((each, placement) for each in root.iterfind("dock_bars/dock_bar") for placement in each.iterfind("placement")):
        match placed.get(bar):
            case None:
                placement.attrib.pop("visible", None)
            case site, size, slot:
                width, height = placement.attrib["dock_band_size"].split(",")
                placement.attrib = {name: value for name, value in placement.attrib.items() if name not in {"dock_location", "recent_dock_location"}} | {
                    "docked_placement": f"0,{slot}",
                    "visible": str(True),
                    "dock_band_size": f"{size},{height}" if site in {Site.LEFT, Site.RIGHT} else f"{width},{size}",
                    **({} if site is Site.RIGHT else {"dock_location": site, "recent_dock_location": site}),
                }
    returns = element(root, "last_collection_panel_was_in")
    for panel, name in plan["returns"].items():
        element(returns, "item", guid=panel).attrib.update({"dock_bar": held[name].attrib["guid"]})


def called(doc: Rhino.RhinoDoc, entry: str, name: str, value: str) -> bool:
    """Result a window layout named callback answers for the document and the named string argument."""
    arguments = NamedParametersEventArgs()
    try:
        arguments.Set.Overloads[String, UInt32]("rhino_doc_sn", doc.RuntimeSerialNumber)
        arguments.Set(name, value)
        HostUtils.ExecuteNamedCallback(f"Rhino.UI.Internal.TabPanels.NamedCallbacks.{entry}", arguments)
        _, result = arguments.TryGetBool("result")
        return result
    finally:
        arguments.Dispose()


def restore(doc: Rhino.RhinoDoc, target: ET.Element) -> str | None:
    """Failure of restoring the live layout from the target, the Modeling layout deleted and imported from it first and the window laid out after, None once the restore ran."""
    with tempfile.TemporaryDirectory() as folder:
        path = Path(folder, f"{Task.MODELING}.rhw")
        ET.ElementTree(target).write(path, encoding="utf-8", xml_declaration=True)
        called(doc, "DeleteWindowLayout", "name", Task.MODELING.value)
        imported = called(doc, "ImportWindowLayout", "filename", str(path))
    if not imported:
        return "import read no window layout from the rendered file"
    if not called(doc, "RestoreWindowLayout", "name", Task.MODELING.value):
        return f"restore found no window layout named {Task.MODELING}"
    RhinoEtoApp.MainWindowForDocument(doc).ControlObject.ContentView.LayoutSubtreeIfNeeded()
    return None


def window_layout(doc: Rhino.RhinoDoc, bundled: ET.Element, *, restored: bool) -> Iterator[str]:
    """Measured record and report lines of the live window layout restored from the render of the measures taken under it, measured and rendered again once a restore gave the containers their rendered forms, an error line naming what the render lacks or the restore refused."""
    record, label = measured(doc, bundled), f'WindowLayouts["{Task.MODELING}"]'
    measurement = line(Kind.MEASUREMENT, json.dumps(record))
    root, plan, ribbon = exported(doc), layout(record), {row.attrib["guid"]: row for row in bundled.iterfind(f"dock_bars/dock_bar[@guid='{Bar.RIBBON}']/tabs/tool_bar")}
    target = deepcopy(root)
    held = resolved(target, plan)
    if missing := lacking(held, plan, ribbon):
        yield from (measurement, line(Kind.ERROR, f"{label} export holds no {', '.join(missing)}"))
        return
    arranged(target, held, plan, ribbon)
    if (before := canonical(ET.tostring(root, encoding="unicode"))) == (after := canonical(ET.tostring(target, encoding="unicode"))):
        yield measurement
        return
    match restore(doc, target):
        case str() as failure:
            yield from (measurement, line(Kind.ERROR, f"{label} {failure}"))
        case None if restored:
            yield from (*changes(label, before, after), measurement)
        case None:
            yield from changes(label, before, after)
            yield from window_layout(doc, bundled, restored=True)


# --- [ROWS]
def support(name: str, target: object) -> Row:
    """Row of a `SupportOptions` getter and the setter its name derives."""
    prefix, _, member = name.partition("_")
    return Row(label=f"SupportOptions.{name}", read=getattr(SupportOptions, name), write=getattr(SupportOptions, f"{prefix}_Set{member}"), target=target)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(doc: Rhino.RhinoDoc) -> Iterator[Row | str]:
    """Rows of the content panels, layer states, and block preview with the material library's skip line, then the window layout's measured record and lines."""
    rdk, eto, commands = (PlugIn.IdFromName(name) for name in ("Renderer Development Kit", "RDK_EtoUI", "Commands"))
    library, folder = any(path.is_file() and not path.name.startswith(".") for path in MATERIALS.rglob("*")), str(MATERIALS)
    flags = RestoreLayerProperties.Visible | RestoreLayerProperties.Locked | RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.NewDetailOn
    yield from (unsigned((rdk, "Settings"), name, target=target) for name, target in (("LibrariesViewMode", 1), ("LibrariesListSizePercentage", 0)))
    yield from (
        unsigned((rdk, "Settings", "RendererSupport"), name, target=ColorTranslator.ToWin32(color(rgb)))
        for name, rgb in (("LightPreviewCheckerColor", Surface.BOX), ("DarkPreviewCheckerColor", Surface.PANEL))
    )
    yield from (key((eto, panel), "ViewMode", target=1) for panel in ("BlockContent", "FileExplorer"))
    yield from starmap(
        support,
        (
            ("Libraries_ShowDocuments", False),
            *(
                (("Libraries_CustomPathList", folder), ("Libraries_InitialLocation", SupportOptions.RdkInitialLocation.CustomFolder), ("Libraries_InitialLocationCustomFolder", folder))
                if library
                else ()
            ),
            ("BlockContent_ShowDocuments", False),
        ),
    )
    yield unsigned((commands, "LayerStates"), "RestoreLayerProperties", target=int(flags))
    yield from (key((commands, "LayerStates"), name, target=True) for name in ("ModelPropertiesChecked", "ViewportPropertiesChecked"))
    yield absent(("ObjectManager", "Preview"), "DisplayModeId")
    yield from (() if library else (line(Kind.SKIP, folder),))
    yield from window_layout(doc, bundled_layout(), restored=False)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
