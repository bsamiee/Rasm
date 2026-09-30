"""InDesign's rows and Essentials frame, with the defaults database and workspace file it reads."""

from collections.abc import Mapping, Sequence
from enum import IntEnum
from functools import partial
from pathlib import Path
import re
import struct
from typing import Final

from lxml import etree
import msgspec

from interface import host
from interface.adobe import window
from interface.adobe.rows import Active, member, papers, Row, STROKE_UNITS, text_scale
from interface.adobe.session import Scripted
from interface.adobe.stores import File, Folder
from interface.adobe.window import Role
from interface.roles import Guide, Ink, Line, Status, Surface, SWATCHES, Tag, Text, Typography
from interface.units import Length, Pen, Units

# --- [TYPES] ----------------------------------------------------------------------------


class Panel(IntEnum):
    """InDesign panel by the widget id its workspace data holds."""

    TOOLS = 4353
    TRANSFORM = 30254
    CHARACTER = 26901
    PARAGRAPH = 27162
    PARAGRAPH_STYLES = 8466
    CHARACTER_STYLES = 8465
    GLYPHS = 53519
    OBJECT_STYLES = 113153
    PROPERTIES = 71040
    ALIGN = 25871
    PATHFINDER = 99625
    LINKS = 132609
    SWATCHES = 16385
    COLOR = 29241
    GRADIENT = 24065
    SCRIPTS = 52229
    HISTORY = 149248
    PAGES = 6199
    LAYERS = 18464
    INFO = 89089
    SEPARATIONS_PREVIEW = 71937
    ATTRIBUTES = 11031


# --- [CONSTANTS] ------------------------------------------------------------------------

DEFAULTS: Final = "InDesign Defaults"
BOOKMARK: Final = "PaletteWorkspace/OWLWorkspaceBookmark"
ENTITIES: Final = frozendict({"hash": "#", "lt": "<", "quot": '"'})

# --- [MODELS] ---------------------------------------------------------------------------


class Named(msgspec.Struct, frozen=True, tag=True):
    """Application collection converged to the sorted names of its items, every item outside them removed."""

    collection: str


class Item(msgspec.Struct, frozen=True, tag=True):
    """Member of the named item of the `app` collection the owner names, the item added when absent, holding a member of the named DOM enumeration where one is named."""

    owner: str
    item: str
    name: str
    enumeration: str | None


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [PANELS]
def decoded(element: etree._Element) -> etree._Element:
    """Panel data a workspace element's `app-data` holds in its entity spellings."""
    return etree.fromstring(re.sub(rf"#({'|'.join(ENTITIES)});", lambda found: ENTITIES[found[1]], element.attrib[window.Attribute.DATA]))


def identity(element: etree._Element) -> str | int:
    """Panel id a workspace element's data names: its UXP or CEP extension id, else its widget id."""
    match dict(decoded(element).attrib):
        case {"UXPExtensionID": extension} | {"CSXSExtensionID": extension}:
            return extension
        case attributes:
            return int(attributes["id"])


# --- [DATABASE]
def checksum(page: bytes) -> int:
    """Checksum InDesign stores in the page's last four bytes, which it computes with those bytes zeroed."""
    body, lanes = page[:-4], 8
    return sum(body) % 0xFFF1 | sum(byte * (len(page) // lanes * (lanes - offset % lanes) - offset // lanes) for offset, byte in enumerate(body)) % 0xFFF1 << 16


def recorded(slider: float, held: bytes | None) -> bytes | host.Error:
    """Defaults database with every UIScalingPrefs record at the interface scale slider position the user set and each changed page checksummed, else the error of the absent database or record."""
    record, page_size = struct.Struct("<dHdiH"), 0x1000
    header = struct.pack("<II", 0x21898, record.size)
    match held:
        case None:
            return host.Error(f"{DEFAULTS} is absent")
        case bytes() if header not in held:
            return host.Error(f"{DEFAULTS} holds no UIScalingPrefs record")
        case bytes():
            first, *records = held.split(header)
            body = header.join((first, *(record.pack(slider, *record.unpack_from(chunk)[1:4], 1) + chunk[record.size :] for chunk in records)))
            return b"".join(
                page if (page := body[start : start + page_size]) == held[start : start + page_size] else page[:-4] + checksum(page).to_bytes(4, "little") for start in range(0, len(body), page_size)
            )


# --- [WORKSPACE]
def opened(document: bytes) -> etree._Element:
    """Root of a workspace file with its bookmark's CDATA section kept."""
    return etree.fromstring(document, etree.XMLParser(strip_cdata=False))


def bookmark(document: etree._Element) -> etree._Element:
    """Root of the `<workspace>` block in a workspace file's bookmark, each newline in an attribute value kept as a character reference the XML parser would fold to a space."""
    return window.parsed(re.sub(r'="[^"]*"', lambda found: found[0].replace("\n", "&#10;"), document.findtext(BOOKMARK, "")).encode())


def workspace_file(file: str, factory: Sequence[etree._Element], held: bytes | None) -> bytes | tuple[host.Skip, ...] | host.Error:
    """Workspace file with its block arranged in the frame, the Layers panel's small rows set, and InDesign's default menu set, else the skip of a file InDesign has yet to write or of each panel no workspace holds, or the error of Layers panel data without its pane options."""
    if held is None:
        return (host.Skip(file),)
    document = opened(held)
    match window.arranged(FRAME, bookmark(document), factory):
        case tuple() as skipped:
            return skipped
        case arranged:
            layers = next(element for element in arranged.iter(window.Markup.PALETTE) if identity(element) == Panel.LAYERS)
            data = decoded(layers)
            if (options := data.find("LayerPaneOptions")) is None:
                return host.Error(f"panel {Panel.LAYERS} data holds no LayerPaneOptions")
            options.set("SmallRows", "true")
            escapes = str.maketrans({plain: f"#{name};" for name, plain in ENTITIES.items()})
            layers.set(window.Attribute.DATA, etree.tostring(data, encoding="unicode").translate(escapes) + "\n")
            (mark,), (menus,) = document.iterfind(BOOKMARK), document.iterfind("menu-set")
            mark.text = etree.CDATA(f"\n{window.serialized(arranged).replace('&#10;', '\n').replace('&gt;', '>')}\n\n")
            menus.set("name", "InDesign Defaults")
            info = document.getroottree().docinfo
            same = etree.tostring(document, method="c14n") == etree.tostring(opened(held), method="c14n")
            return held if same else f'<?xml version="{info.xml_version}" encoding="{info.encoding}" standalone="yes"?>\n{etree.tostring(document, encoding="unicode")}'.encode()


def workspace(factory: Sequence[bytes]) -> tuple[File, ...]:
    """Essentials workspace file arranged over the blocks of the factory workspace files."""
    file = f"Workspaces/{window.WORKSPACE}_CurrentWorkspace.xml"
    return (File(Folder.SETTINGS, file, partial(workspace_file, file, tuple(bookmark(opened(data)) for data in factory))),)


# --- [ROWS]
def folders(bundle: host.Bundle, base: Mapping[Folder, Path]) -> Mapping[Folder, Path]:
    """InDesign's folders beside the session's: the factory workspaces of the settings folder's locale beside the bundle."""
    return frozendict({Folder.FACTORY: bundle.path.parent / "Presets" / "InDesign_Workspaces" / base[Folder.SETTINGS].name})


def rows(units: Units, bundle: host.Bundle) -> tuple[Row | File, ...]:
    """InDesign's rows with the interface scaled to draw panel text at the interface text size through the slider's quarter steps above the unscaled interface, lengths in points, the system's page unit shown on the rulers, strokes in the system's stroke unit, and type in points."""
    presets = papers(units)
    slider = (text_scale(bundle.path.parent.joinpath("Presets", "themeXMLs", "FontTheme_Panel_MAC_enUS.xml")) - 1) / 0.25
    reserved, edges, axes = ("None", "Registration", "Paper", "Black"), ("top", "bottom", "left", "right"), ("horizontal", "vertical")
    swatches = {name: rgb for name, rgb in SWATCHES.items() if name not in reserved}
    measured = {
        f"{kind}MeasurementUnits": unit.name
        for unit, kinds in ((units.page, (*axes, "printDialog")), (STROKE_UNITS[units], ("stroke",)), (Length.POINTS, ("typographic", "textSize")))
        for kind in kinds
    }
    enumerations = {
        "toolsPanel": "ToolsPanelOptions",
        "toolTips": "ToolTipOptions",
        **dict.fromkeys(measured, "MeasurementUnits"),
        "rulerOrigin": "RulerOrigin",
        **dict.fromkeys(("iconSize", "masterIconSize"), "IconSizes"),
        "model": "ColorModel",
        "space": "ColorSpace",
    }
    members: dict[str, dict[str, object]] = {
        "generalPreferences": {
            "uiBrightnessPreference": 0.0,
            "pasteboardColorPreference": 1,
            **dict.fromkeys(("panelTabHeightPreference", "showStartWorkspace", "showWhatsNewOnStartup", "contextBarVisible", "showStockPurchaseAdornment"), False),
            **dict.fromkeys(("useApplicationFrame", "enableMultiTouchGestures"), True),
            "toolsPanel": "DOUBLE_COLUMN",
            "toolTips": "NORMAL",
        },
        "gpuPerformancePreferences": {"enableAnimatedZoom": False},
        "typeContextualUiPrefs": dict.fromkeys(("showAlternatesUi", "showFractionsUi"), False),
        "pasteboardPreferences": {"matchPreviewBackgroundToThemeColor": False, "previewBackgroundColor": Surface.CANVAS, **dict.fromkeys(("bleedGuideColor", "slugGuideColor"), Guide.CONSTRUCTION)},
        "guidePreferences": {"rulerGuidesColor": Guide.CONSTRUCTION},
        "documentPreferences": {"marginGuideColor": Guide.CONSTRUCTION, "columnGuideColor": Line.DATUM_GRID, "pageSize": presets[0].sheet},
        "smartGuidePreferences": {"guideColor": Guide.TRACKING},
        "gridPreferences": {
            **dict.fromkeys(("gridColor", "baselineColor"), Line.PAPER_GRID),
            **dict.fromkeys(("documentGridShown", "baselineGridShown"), False),
            "gridsInBack": True,
            **{f"{axis}{key}": value for axis in axes for key, value in (("GridlineDivision", units.snap / Length.POINTS), ("GridSubdivision", round(units.snap / units.resolution)))},
        },
        "baselineFrameGridOptions": {"baselineFrameGridColor": Line.PAPER_GRID},
        "spellPreferences": {"misspelledWordColor": Status.ERROR, **dict.fromkeys(("repeatedWordColor", "uncapitalizedWordColor", "uncapitalizedSentenceColor"), Status.WARNING)},
        "xmlPreferences": {f"default{kind}TagColor": tag.value for kind, tag in zip(("Story", "Table", "Cell", "Image"), Tag, strict=False)},
        "galleyPreferences": {"backgroundColor": Surface.WELL, "textColor": Text.PRIMARY, "displayFont": Typography.INTERFACE.family, "displayFontSize": 10.0},
        "watermarkPreferences": {"watermarkFontColor": Ink.DOCUMENT},
        "viewPreferences": {"pointsPerInch": float(round(Length.INCHES / Length.POINTS)), "cursorKeyIncrement": units.resolution / Length.POINTS, **measured, "rulerOrigin": "PAGE_ORIGIN"},
        "marginPreferences": dict.fromkeys(edges, units.margin / Length.POINTS),
        "pageItemDefaults": {"strokeWeight": Pen.THIN / Length.POINTS},
    }
    items: dict[tuple[str, str], dict[str, object]] = {
        ("panels", "$ID/Pages"): dict.fromkeys(("iconSize", "masterIconSize"), "EXTRA_SMALL_ICON"),
        **{("documentPresets", paper.title): {"pageSize": paper.sheet, **dict.fromkeys(edges, paper.value.margin / Length.POINTS)} for paper in presets},
        **{("colors", name): {"model": "PROCESS", "space": "RGB", "colorValue": rgb} for name, rgb in swatches.items()},
    }
    return (
        *(member(owner, name, target, enumeration=enumerations.get(name)) for owner, targets in members.items() for name, target in targets.items()),
        *(Row(f'{owner}["{item}"].{name}', Item(owner, item, name, enumerations.get(name)), target) for (owner, item), targets in items.items() for name, target in targets.items()),
        Row("documentPresets", Named("documentPresets"), tuple(sorted(("[Default]", *(paper.title for paper in presets))))),
        Row("swatches", Named("swatches"), tuple(sorted({*reserved, *SWATCHES}))),
        Row("generalPreferences.setActiveWorkspace", Active(), window.WORKSPACE),
        File(Folder.SETTINGS, DEFAULTS, partial(recorded, slider)),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

FRAME: Final = window.Frame(
    toolbar=Panel.TOOLS,
    bar=False,
    groups=frozendict({
        Role.LIBRARIES: ("com.adobe.DesignLibraries.angular",),
        Role.TRANSFORM: (Panel.TRANSFORM,),
        Role.CHARACTER: (Panel.CHARACTER, Panel.PARAGRAPH),
        Role.STYLES: (Panel.PARAGRAPH_STYLES, Panel.CHARACTER_STYLES),
        Role.GLYPHS: (Panel.GLYPHS,),
        Role.ASSETS: (Panel.OBJECT_STYLES,),
        Role.PROPERTIES: (Panel.PROPERTIES,),
        Role.ALIGNMENT: (Panel.ALIGN, Panel.PATHFINDER),
        Role.DOCUMENT: (Panel.LINKS,),
        Role.COLOR: (Panel.SWATCHES, Panel.COLOR, Panel.GRADIENT),
        Role.AUTOMATION: (Panel.SCRIPTS, Panel.HISTORY),
        Role.STRUCTURE: (Panel.PAGES, Panel.LAYERS),
        Role.INFORMATION: (Panel.INFO,),
        Role.OUTPUT: (Panel.SEPARATIONS_PREVIEW, Panel.ATTRIBUTES),
    }),
    fixed=frozenset({Panel.PROPERTIES, Panel.ALIGN, Panel.PATHFINDER, Panel.COLOR, Panel.GRADIENT}),
    identity=identity,
    sized=window.sized,
    closed=partial(window.floated, closed=False),
)
PRODUCT: Final = Scripted(
    name="indesign", identifiers=("com.adobe.InDesign",), command="«event K2  dosc»", arguments="given «class doLg»:«constant ScLgJSLg», «class wArg»:", rows=rows, folders=folders, workspace=workspace
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FRAME", "PRODUCT", "folders", "rows", "workspace"]
