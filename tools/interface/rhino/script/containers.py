# ty: ignore[unresolved-import, unresolved-attribute, unsupported-operator, invalid-argument-type, no-matching-overload, redundant-condition-strict]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, operator, arg-type, no-any-return, typeddict-item"
# ruff: file-ignore[banned-api, suspicious-xml-etree-import, suspicious-xml-element-tree-usage]
"""Rhino's content panel settings, its Rendering panel sections, the measures of its window, and the window layout restored from the one the measures size."""

from collections.abc import Callable, Iterable, Iterator, Mapping, Sequence
from copy import deepcopy
from itertools import starmap
import json
import math
from pathlib import Path
import tempfile
import uuid
import xml.etree.ElementTree as ET

from AppKit import NSTableHeaderCell, NSTextFieldCell
import clr
from CoreGraphics import CGRect
from Eto.Forms import Control, Splitter, TextArea, TreeGridView
import Rhino
from Rhino.Display import DisplayModeDescription
from Rhino.DocObjects.Tables import RestoreLayerProperties
from Rhino.PlugIns import PlugIn
from Rhino.Render import SupportOptions
from Rhino.Resources import EtoFonts
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
from interface.rhino.script.accessors import color, disposed, guid, Internal, key
from interface.rhino.script.template import Labels, labels, Template
from interface.rhino.window import Band, Bar, Extent, Grid, layout, Measured, PanelId, RETURNS, RibbonTab, Site, TOGGLES
from interface.roles import Surface
from interface.units import Units

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [CALLBACKS]
def called(doc: Rhino.RhinoDoc, entry: str, serial: str, **arguments: str | System.Guid) -> bool:
    """Result the named callback answers for the document, its serial number set under the key the callback reads it by, and the named arguments."""
    with disposed(NamedParametersEventArgs()) as held:
        held.Set.Overloads[String, UInt32](serial, doc.RuntimeSerialNumber)
        for name, value in arguments.items():
            held.Set(name, value)
        HostUtils.ExecuteNamedCallback(entry, held)
        _, result = held.TryGetBool("result")
        return result


# --- [MEASURES]
def fit(outline: object, index: int, title: str, texts: Iterable[str]) -> int:
    """Smallest Eto width of the column at the display index drawing its header title whole in the header font and each text whole in the grid's bold small font, a text beside the indent of the outline column's level-0 cell frame."""
    column, bounds = outline.TableColumns()[index], CGRect(0, 0, 1e5, 1e5)
    indent = column.Width.Value - outline.GetCellFrame(System.IntPtr(index), System.IntPtr(0)).Width.Value if column.Identifier == outline.OutlineTableColumn.Identifier else 0

    def size(kind: Callable[[str], object], text: str, font: object) -> float:
        """Width a cell of the kind needs to draw the text whole in the font."""
        cell = kind(text)
        cell.Font = font
        return cell.CellSizeForBounds(bounds).Width.Value

    header = size(NSTableHeaderCell, title, column.HeaderCell.Font)
    return math.ceil(max((header, *(size(NSTextFieldCell, text, EtoFonts.SmallBoldFont.ControlObject) + indent for text in texts))) + outline.IntercellSpacing.Width.Value)


def inset(shell: object, grid: TreeGridView) -> float:
    """Width the container spends beside the Eto widths of the tree grid's visible columns: its own chrome beside the scroll view's visible rect and the outline's padding before the first and after the last visible column."""
    outline = grid.ControlObject
    shown = [index for index, column in enumerate(outline.TableColumns()) if not column.Hidden]
    first, last = (outline.RectForColumn(System.IntPtr(index)) for index in (shown[0], shown[-1]))
    widths = sum(column.Width for column in grid.Columns if column.Visible)
    return shell.Size.Width - outline.EnclosingScrollView.DocumentVisibleRect.Size.Width.Value + (last.Right.Value + first.X.Value - widths)


def measured(doc: Rhino.RhinoDoc, root: ET.Element, shown: Sequence[Labels]) -> Measured:
    """Dock site heights, extents, Layers column default widths, Layers and Layouts columns fitted to every template's texts, and each toggle panel's grid, every panel measured as its container's selected tab and the container left with the tab the exported layout selects."""
    bars, serial = Internal.TAB_PANEL_DOCK_BARS.type, UInt32(doc.RuntimeSerialNumber)
    dock_sites = Internal.TAB_PANEL_DOCK_SITES.type.GetMethod("FromDocument", Array[System.Type]([clr.GetClrType(Rhino.RhinoDoc)])).Invoke(None, Array[System.Object]([doc]))

    def container(bar: System.Guid) -> Control | None:
        """Dock bar's control at the size its band gives it, None while the bar shows no content."""
        return bars.GetMethod("FromDockBarId").Invoke(None, Array[System.Object]([bar])).HasContent(serial)

    def contained(control: object, kind: System.Type) -> object:
        """First descendant control of the kind."""
        return next(each for each in control.Children if kind.IsInstanceOfType(each))

    def private(owner: object, kind: System.Type, name: str) -> object:
        """Value of the private instance field the type declares on the owner."""
        return kind.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner)

    def columns(panel: object, view: Internal, kind: Internal) -> tuple[TreeGridView, dict[str, tuple[int, object]]]:
        """Tree grid of the view type in the panel, and each of its columns with its display index by the column type's member name."""
        grid = contained(panel, view.type)
        return grid, {System.Enum.GetName(kind.type, int(column.Identifier)): (index, column) for index, column in enumerate(grid.ControlObject.TableColumns())}

    def layers(panel: object, shell: object) -> tuple[dict[Extent, float], dict[str, int]]:
        """Measures of the Layers container and its tree, and the fitted width of each column a template shows text in by `LayerColumns.ColumnType` name, titled by its tooltip since Rhino blanks a narrow column's header."""
        grid, held = columns(panel, Internal.LAYER_TREE_GRID_VIEW, Internal.LAYER_COLUMN_TYPE)
        outline, texted = grid.ControlObject, {name for each in shown for name in each["layers"]}
        extents = {
            Extent.LAYERS_CHROME: shell.Size.Height - grid.Size.Height,
            Extent.LAYERS_HEADER: outline.HeaderView.Frame.Size.Height.Value,
            Extent.LAYERS_ROW: outline.RowHeight.Value + outline.IntercellSpacing.Height.Value,
            Extent.LAYERS_INSET: inset(shell, grid),
        }
        return extents, {name: fit(outline, index, column.HeaderToolTip, (text for each in shown for text in each["layers"][name])) for name, (index, column) in held.items() if name in texted}

    def layouts(panel: object, shell: object) -> tuple[dict[Extent, float], dict[str, int]]:
        """Measures of the Layouts container and the fitted width of each of its columns by `LayoutTreeGridView.ColumnType` name, a column no template shows text in fitted to its header."""
        grid, held = columns(panel, Internal.LAYOUT_TREE_GRID_VIEW, Internal.LAYOUT_COLUMN_TYPE)
        return {Extent.LAYOUTS_INSET: inset(shell, grid)}, {
            name: fit(grid.ControlObject, index, column.HeaderCell.Title, (text for each in shown for text in each["layouts"].get(name, ()))) for name, (index, column) in held.items()
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

    def toggles(panel: object, shell: object) -> Grid:
        """Toggle count and the cell pitch the panel wraps its toggles at under either geometry, and the width and height its container spends around the panel's content."""
        kind = panel.GetType()
        cell, spacing = private(panel, kind, "m_control_size"), 4 if kind.GetMethod("UseToggleButtons", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(panel, None) else 0
        return Grid(
            count=private(panel, kind, "m_item_count"),
            pitch_x=cell.Width + spacing,
            pitch_y=cell.Height + spacing,
            inset=shell.Size.Width - panel.Content.Width,
            chrome=shell.Size.Height - panel.Content.Height,
        )

    top, window = Panels.PanelDockBar(guid(PanelId.PROPERTIES)), RhinoEtoApp.MainWindowForDocument(doc).ControlObject.ContentView

    def selected[T](panel: PanelId, measure: Callable[[object, object], T]) -> T:
        """Measures of the panel laid out as the selected tab of the bar holding it with the head of the container it returns to, the panel's tab removed again where no such bar held it and the bar's exported tab selected again."""
        identity, (head, *_) = guid(panel), RETURNS[panel]
        home = next((bar for bar in Panels.PanelDockBars(identity) if bar in Panels.PanelDockBars(guid(head))), System.Guid.Empty)
        bar = top if home == System.Guid.Empty else home
        Panels.OpenPanel(bar, identity, makeSelectedPanel=True)
        try:
            window.LayoutSubtreeIfNeeded()
            return measure(Panels.GetPanel(identity, doc), container(bar))
        finally:
            if home == System.Guid.Empty:
                called(doc, "Rhino.UI.Internal.NamedCallbacks.RhinoUiCloseDockbarTab", "documentSerialNumber", factoryId=identity)
            Panels.OpenPanel(bar, System.Guid.Parse(next(root.iterfind(f"dock_bars/dock_bar[@guid='{bar}']/tabs")).attrib["selected_item"]), makeSelectedPanel=True)

    window.LayoutSubtreeIfNeeded()
    toolbar = Internal.TOOLBAR_SETTINGS.type.GetProperty("Instance").GetValue(None)
    style = Internal.TAB_PANEL_SETTINGS.type.GetProperty("HorizontalDisplayStyle").GetValue(None)
    (layer_extents, layer_fitted), (layout_extents, layout_fitted) = selected(PanelId.LAYERS, layers), selected(PanelId.LAYOUTS, layouts)
    return Measured(
        sites={site: getattr(dock_sites, site).Control.Size.Height for site in Site},
        extents={
            Extent.TAB_STRIP: Internal.BASE_TAB_CONTROL.type.GetMethod("CalculateTabHeight", BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([style])),
            Extent.BUTTON: toolbar.Buttons.TotalButtonSize,
            Extent.RESIZER: Internal.DOCK_SITE_RESIZER.type.GetProperty("ResizerWidth").GetValue(None),
            Extent.STATUS_BAR: dock_sites.StatusBar.Height,
            Extent.HISTORY_LINE: contained(container(guid(Bar.COMMAND_HISTORY)), clr.GetClrType(TextArea)).Font.LineHeight,
            **layer_extents,
            **layout_extents,
            **selected(PanelId.MATERIALS, materials),
            **selected(PanelId.LIBRARIES, libraries),
        },
        grids={panel: selected(panel, toggles) for panel in TOGGLES},
        columns={str(column): Internal.LAYER_COLUMNS.type.GetMethod("DefaultWidth").Invoke(None, Array[System.Object]([column])) for column in System.Enum.GetValues(Internal.LAYER_COLUMN_TYPE.type)},
        fitted={PanelId.LAYERS: layer_fitted, PanelId.LAYOUTS: layout_fitted},
    )


# --- [LAYOUT]
def exported(doc: Rhino.RhinoDoc) -> ET.Element:
    """Live window layout of the document's window under the Modeling name, as the writer of `containers.xml` serializes it."""
    with disposed(MemoryStream()) as stream:
        Internal.WINDOW_LAYOUT_FILE.type.GetMethod("Write").Invoke(None, Array[System.Object]([doc, stream, Task.MODELING.value, None, False]))
        return ET.fromstring(bytes(stream.ToArray()))


def bundled_layout() -> ET.Element:
    """Default window layout Rhino.UI includes."""
    with disposed(Internal.TAB_PANEL_DOCK_BARS.type.Assembly.GetManifestResourceStream("Rhino.UI.Resources.rui.default.rhw")) as stream, disposed(MemoryStream()) as held:
        stream.CopyTo(held)
        return ET.fromstring(bytes(held.ToArray()))


def resolved(root: ET.Element, bands: Mapping[Site, Band]) -> dict[Bar | tuple[PanelId, ...], ET.Element]:
    """Dock bar of each bar the bands name, a panel container no bar holds created on its band's first held bar's placement."""
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

    for band_bars, name in ((band["bars"], name) for band in bands.values() for name, _ in band["bars"]):
        holders, siblings = [bar for bar in bars if holds(bar, name)], [held[other] for other, _ in band_bars if other in held]
        if holders:
            held[name] = holders[0]
        elif isinstance(name, tuple) and siblings:
            held[name] = ET.SubElement(element(root, "dock_bars"), "dock_bar", {"guid": created(name[0])})
            held[name].extend(deepcopy(placement) for placement in siblings[0].iterfind("placement"))
            ET.SubElement(held[name], "tabs")
    return held


def lacking(held: Mapping[Bar | tuple[PanelId, ...], ET.Element], bands: Mapping[Site, Band], bundled_tabs: Mapping[str, ET.Element]) -> tuple[str, ...]:
    """Name of each bar the bands name that no dock bar holds, and of each ribbon tab neither the ribbon nor the bundled layout holds."""
    live = {row.attrib["guid"] for name, bar in held.items() if name == Bar.RIBBON for row in bar.iterfind("tabs/tool_bar")}
    return (
        *(f"dock bar {name.name}" if isinstance(name, Bar) else f"container of {name[0].name}" for band in bands.values() for name, _ in band["bars"] if name not in held),
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


def arranged(root: ET.Element, held: Mapping[Bar | tuple[PanelId, ...], ET.Element], bands: Mapping[Site, Band], bundled_tabs: Mapping[str, ET.Element]) -> None:
    """Edit the exported layout into the bands and the containers panels return to, the command prompt in the sidebar, every other dock bar hidden and holding no panel, and a Right dock location left unwritten as Rhino's writer leaves its default."""
    panels, ribbon = {row.attrib["guid"]: row for row in root.iterfind("dock_bars/dock_bar/tabs/panel")}, held[Bar.RIBBON]
    live = {row.attrib["guid"]: row for row in ribbon.iterfind("tabs/tool_bar")}
    placed = {held[name]: (site, band["size"], slot) for site, band in bands.items() for slot, (name, _) in enumerate(band["bars"])}
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
    for site, made in {site: band_element(sites, band, held) for site, band in bands.items()}.items():
        owner = element(sites, "dock_site", location=site)
        owner[:] = [*(each for each in owner if each.tag != "band"), made]
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
    for panel, name in RETURNS.items():
        element(returns, "item", guid=panel).attrib.update({"dock_bar": held[name].attrib["guid"]})


def restore(doc: Rhino.RhinoDoc, target: ET.Element) -> str | None:
    """Failure of restoring the live layout from the target, the Modeling layout deleted and imported from it first and the window laid out after, None once the restore ran."""
    serial = "rhino_doc_sn"
    with tempfile.TemporaryDirectory() as folder:
        path = Path(folder, f"{Task.MODELING}.rhw")
        ET.ElementTree(target).write(path, encoding="utf-8", xml_declaration=True)
        called(doc, "Rhino.UI.Internal.TabPanels.NamedCallbacks.DeleteWindowLayout", serial, name=Task.MODELING.value)
        imported = called(doc, "Rhino.UI.Internal.TabPanels.NamedCallbacks.ImportWindowLayout", serial, filename=str(path))
    if not imported:
        return "import read no window layout from the rendered file"
    if not called(doc, "Rhino.UI.Internal.TabPanels.NamedCallbacks.RestoreWindowLayout", serial, name=Task.MODELING.value):
        return f"restore found no window layout named {Task.MODELING}"
    RhinoEtoApp.MainWindowForDocument(doc).ControlObject.ContentView.LayoutSubtreeIfNeeded()
    return None


def window_layout(doc: Rhino.RhinoDoc, bundled: ET.Element, shown: Sequence[Labels], *, restored: bool) -> Iterator[str]:
    """Measured record and report lines of the live window layout, exported after the measures, restored from their render, measured and rendered again once a restore gave the containers their rendered forms, an error line naming what the render lacks or the restore refused."""
    record, label = measured(doc, exported(doc), shown), f'WindowLayouts["{Task.MODELING}"]'
    root, measurement = exported(doc), line(Kind.MEASUREMENT, json.dumps(record))
    bands, ribbon = layout(record), {row.attrib["guid"]: row for row in bundled.iterfind(f"dock_bars/dock_bar[@guid='{Bar.RIBBON}']/tabs/tool_bar")}
    target = deepcopy(root)
    held = resolved(target, bands)
    if missing := lacking(held, bands, ribbon):
        yield from (measurement, line(Kind.ERROR, f"{label} export holds no {', '.join(missing)}"))
        return
    arranged(target, held, bands, ribbon)
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
            yield from window_layout(doc, bundled, shown, restored=True)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(doc: Rhino.RhinoDoc, targets: Mapping[Units, Template]) -> Iterator[Row | str]:
    """Rows of the content panels, each Rendering panel section expanded under its type's id and owner, layer states, and the block preview's display mode at its owner's default with the material library's skip line, then the window layout's record and lines over every template's grid texts."""
    rdk, eto, commands = (PlugIn.IdFromName(name) for name in ("Renderer Development Kit", "RDK_EtoUI", "Commands"))
    library, folder = any(path.is_file() and not path.name.startswith(".") for path in MATERIALS.rglob("*")), str(MATERIALS)
    flags = RestoreLayerProperties.Visible | RestoreLayerProperties.Locked | RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.NewDetailOn
    unsigned = {
        (rdk, "Settings"): {"LibrariesViewMode": 1, "LibrariesListSizePercentage": 0},
        (rdk, "Settings", "RendererSupport"): {"LightPreviewCheckerColor": ColorTranslator.ToWin32(color(Surface.BOX)), "DarkPreviewCheckerColor": ColorTranslator.ToWin32(color(Surface.PANEL))},
        (commands, "LayerStates"): {"RestoreLayerProperties": int(flags)},
    }
    rendering, cycles = PlugIn.Find(eto).Assembly, clr.AddReference("RhinoCyclesCore")
    initially = {
        **dict.fromkeys(("CurrentRenderer", "View", "Resolution", "WireFrame", "DitheringColorAdjustment", "RenderChannels"), False),
        **dict.fromkeys(("Background", "GroundPlane", "Lighting"), True),
    }
    sections = {
        **{rendering.GetType(f"RDK.Rendering.RdkRendering{name}Section", throwOnError=True): held for name, held in initially.items()},
        cycles.GetType("RhinoCyclesCore.Settings.AdvancedSettingsSection", throwOnError=True): True,
    }
    yield from (key(path, name, target=target, stored=UInt32) for path, held in unsigned.items() for name, target in held.items())
    yield from (key((eto, panel), "ViewMode", target=1) for panel in ("BlockContent", "FileExplorer"))
    yield from (
        key((commands if (owner := PlugIn.Find(kind.Assembly)) is None else owner.Id, "section-expanded", str(kind.GUID)), "expanded", target=True, default=held) for kind, held in sections.items()
    )
    yield Row(label="SupportOptions.Libraries_ShowDocuments", read=SupportOptions.Libraries_ShowDocuments, write=SupportOptions.Libraries_SetShowDocuments, target=False)
    yield from (
        (
            Row(label="SupportOptions.Libraries_CustomPathList", read=SupportOptions.Libraries_CustomPathList, write=SupportOptions.Libraries_SetCustomPathList, target=folder),
            Row(
                label="SupportOptions.Libraries_InitialLocation",
                read=SupportOptions.Libraries_InitialLocation,
                write=SupportOptions.Libraries_SetInitialLocation,
                target=SupportOptions.RdkInitialLocation.CustomFolder,
            ),
            Row(
                label="SupportOptions.Libraries_InitialLocationCustomFolder",
                read=SupportOptions.Libraries_InitialLocationCustomFolder,
                write=SupportOptions.Libraries_SetInitialLocationCustomFolder,
                target=folder,
            ),
        )
        if library
        else ()
    )
    yield Row(label="SupportOptions.BlockContent_ShowDocuments", read=SupportOptions.BlockContent_ShowDocuments, write=SupportOptions.BlockContent_SetShowDocuments, target=False)
    yield from (key((commands, "LayerStates"), name, target=True) for name in ("ModelPropertiesChecked", "ViewportPropertiesChecked"))
    yield key(("ObjectManager", "Preview"), "DisplayModeId", target=DisplayModeDescription.WireframeId, default=DisplayModeDescription.WireframeId)
    yield from (() if library else (line(Kind.SKIP, folder),))
    yield from window_layout(doc, bundled_layout(), tuple(starmap(labels, targets.items())), restored=False)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
