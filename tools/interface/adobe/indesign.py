"""InDesign's rows, alias commands, and Essentials frame, with the defaults database, alias plug-in, and workspace file it reads."""

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
from interface.adobe.rows import Active, member, Menu, papers, PROMPT_NAME, prompt_source, Row, STROKE_UNITS, Tool
from interface.adobe.session import Scripted
from interface.adobe.stores import File, Folder, UxpPlugin
from interface.aliases import Alias
from interface.frame import Role
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

LEADER: Final = "Ctrl+G"
DEFAULTS: Final = "InDesign Defaults"
CDATA_OPEN: Final = "<![CDATA["
CDATA_CLOSE: Final = "]]>"
ENTITIES: Final = frozendict({"hash": "#", "lt": "<", "quot": '"'})
COMMANDS: Final[frozendict[Alias, Tool | Menu]] = frozendict({
    Alias.Q: Tool("LINE_TOOL"),
    Alias.QQ: Tool("PEN_TOOL"),
    Alias.W1: Tool("RECTANGLE_TOOL"),
    Alias.WQ: Tool("POLYGON_TOOL"),
    Alias.E: Tool("ELLIPSE_TOOL"),
    Alias.R: Tool("ROTATE_TOOL"),
    Alias.T: Tool("TYPE_TOOL"),
    Alias.TT: Menu(61405, selection=True),
    Alias.T3: Tool("SCALE_TOOL"),
    Alias.TW: Tool("SHEAR_TOOL"),
    Alias.D: Tool("MEASURE_TOOL"),
    Alias.FF: Menu(99621, selection=True),
    Alias.FQ: Tool("SCISSORS_TOOL"),
    Alias.G: Menu(118844, selection=True),
    Alias.GU: Menu(118845, selection=True),
    Alias.GH: Menu(118856, selection=True),
    Alias.GJ: Menu(118857, selection=False),
    Alias.GL: Menu(11304, selection=True),
    Alias.GP: Menu(11395, selection=False),
    Alias.GE: Menu(118850, selection=False),
    Alias.Z: Tool("ZOOM_TOOL"),
    Alias.ZE: Menu(118787, selection=False),
    Alias.V: Tool("SELECTION_TOOL"),
    Alias.VA: Menu(25857, selection=False),
    Alias.VO: Menu(276, selection=False),
    Alias.IM: Menu(113409, selection=False),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Named(msgspec.Struct, frozen=True, tag=True):
    """Application collection converged to the sorted names of its items, every item outside them removed."""

    collection: str


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


# --- [PLUGIN]
def packaged(source: str) -> UxpPlugin:
    """Alias plug-in with a manifest binding the leader to its one command and a main script running the source as ExtendScript."""
    identifier, version, application, minimum, main = "alias", "1.0.0", "ID", "18.5.0", "index.js"
    manifest = {
        "manifestVersion": 5,
        "id": identifier,
        "name": PROMPT_NAME,
        "version": version,
        "main": main,
        "host": {"app": application, "minVersion": minimum},
        "entrypoints": [{"type": "command", "id": identifier, "label": PROMPT_NAME, "shortcut": {"mac": LEADER}}],
    }
    script = host.rendered(
        t'const {{ app, ScriptLanguage }} = require("indesign");\nrequire("uxp").entrypoints.setup({{ commands: {{ {identifier}: () => app.doScript({source}, ScriptLanguage.JAVASCRIPT) }} }});\n'
    )
    return UxpPlugin(frozendict({"manifest.json": msgspec.json.format(msgspec.json.encode(manifest), indent=4), main: script.encode()}))


# --- [DATABASE]
def checksum(page: bytes) -> int:
    """Checksum InDesign stores in the page's last four bytes, which it computes with those bytes zeroed."""
    body, lanes = page[:-4], 8
    return sum(body) % 0xFFF1 | sum(byte * (len(page) // lanes * (lanes - offset % lanes) - offset // lanes) for offset, byte in enumerate(body)) % 0xFFF1 << 16


def recorded(held: bytes | None) -> bytes | host.Error:
    """Defaults database with every UIScalingPrefs record at the 100 % interface scale the user set and each changed page checksummed, else the error of the absent database or record."""
    record, page_size = struct.Struct("<dHdiH"), 0x1000
    header = struct.pack("<II", 0x21898, record.size)
    match held:
        case None:
            return host.Error(f"{DEFAULTS} is absent")
        case bytes() if header not in held:
            return host.Error(f"{DEFAULTS} holds no UIScalingPrefs record")
        case bytes():
            first, *records = held.split(header)
            body = header.join((first, *(record.pack(0.0, *record.unpack_from(chunk)[1:4], 1) + chunk[record.size :] for chunk in records)))
            return b"".join(
                page if (page := body[start : start + page_size]) == held[start : start + page_size] else page[:-4] + checksum(page).to_bytes(4, "little") for start in range(0, len(body), page_size)
            )


# --- [WORKSPACE]
def bookmark(document: bytes) -> etree._Element:
    """Root of the `<workspace>` block in a workspace file's CDATA section, each newline in an attribute value kept as a character reference."""
    block = document.decode().partition(CDATA_OPEN)[2].partition(CDATA_CLOSE)[0]
    return window.parsed(re.sub(r'="[^"]*"', lambda found: found[0].replace("\n", "&#10;"), block).encode())


def workspace_file(file: str, factory: Sequence[etree._Element], held: bytes | None) -> bytes | tuple[host.Skip, ...] | host.Error:
    """Workspace file with its block arranged in the frame, the Layers panel's small rows set, and InDesign's default menu set, else the skip of a file InDesign has yet to write or of each panel no workspace holds, or the error of Layers panel data without its pane options."""
    if held is None:
        return (host.Skip(file),)
    match window.arranged(FRAME, bookmark(held), factory):
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
            head, _, rest = held.decode().partition(CDATA_OPEN)
            block = window.serialized(arranged).replace("&#10;", "\n").replace("&gt;", ">")
            tail = re.sub(r'(<menu-set [^>]*name=")[^"]*', r"\g<1>InDesign Defaults", rest.partition(CDATA_CLOSE)[2])
            return f"{head}{CDATA_OPEN}{block}\n{CDATA_CLOSE}{tail}".encode()


def workspace(factory: Sequence[bytes]) -> tuple[File, ...]:
    """Essentials workspace file arranged over the blocks of the factory workspace files."""
    file = f"Workspaces/{window.WORKSPACE}_CurrentWorkspace.xml"
    return (File(Folder.SETTINGS, file, partial(workspace_file, file, tuple(map(bookmark, factory)))),)


# --- [ROWS]
def folders(bundle: host.Bundle, base: Mapping[Folder, Path]) -> Mapping[Folder, Path]:
    """InDesign's folders beside the session's: the factory workspaces of the settings folder's locale beside the bundle."""
    return frozendict({Folder.FACTORY: bundle.path.parent / "Presets" / "InDesign_Workspaces" / base[Folder.SETTINGS].name})


def rows(units: Units) -> tuple[Row | File | UxpPlugin, ...]:
    """InDesign's rows with lengths in points, the system's page unit shown on the rulers, strokes in the system's stroke unit, and type in points."""
    presets = papers(units)
    reserved, edges, axes = ("None", "Registration", "Paper", "Black"), ("top", "bottom", "left", "right"), ("horizontal", "vertical")
    swatches = {name: rgb for name, rgb in SWATCHES.items() if name not in reserved}
    return (
        member("generalPreferences", "uiBrightnessPreference", 0.0),
        member("generalPreferences", "pasteboardColorPreference", 1),
        member("generalPreferences", "toolsPanel", "DOUBLE_COLUMN", enumeration="ToolsPanelOptions"),
        *(member("generalPreferences", key, target=False) for key in ("panelTabHeightPreference", "showStartWorkspace", "showWhatsNewOnStartup", "contextBarVisible", "showStockPurchaseAdornment")),
        *(member("generalPreferences", key, target=True) for key in ("useApplicationFrame", "enableMultiTouchGestures")),
        member("generalPreferences", "toolTips", "NORMAL", enumeration="ToolTipOptions"),
        member("gpuPerformancePreferences", "enableAnimatedZoom", target=False),
        *(member("typeContextualUiPrefs", key, target=False) for key in ("showAlternatesUi", "showFractionsUi")),
        member("pasteboardPreferences", "matchPreviewBackgroundToThemeColor", target=False),
        member("pasteboardPreferences", "previewBackgroundColor", Surface.CANVAS),
        *(member(owner, key, Guide.CONSTRUCTION) for owner, key in (("guidePreferences", "rulerGuidesColor"), ("documentPreferences", "marginGuideColor"))),
        member("documentPreferences", "columnGuideColor", Line.DATUM_GRID),
        *(member("pasteboardPreferences", key, Guide.CONSTRUCTION) for key in ("bleedGuideColor", "slugGuideColor")),
        member("smartGuidePreferences", "guideColor", Guide.TRACKING),
        *(member(owner, key, Line.PAPER_GRID) for owner, key in (("gridPreferences", "gridColor"), ("gridPreferences", "baselineColor"), ("baselineFrameGridOptions", "baselineFrameGridColor"))),
        *(member("gridPreferences", key, target=False) for key in ("documentGridShown", "baselineGridShown")),
        member("gridPreferences", "gridsInBack", target=True),
        member("spellPreferences", "misspelledWordColor", Status.ERROR),
        *(member("spellPreferences", key, Status.WARNING) for key in ("repeatedWordColor", "uncapitalizedWordColor", "uncapitalizedSentenceColor")),
        *(member("xmlPreferences", f"default{kind}TagColor", tag.value) for kind, tag in zip(("Story", "Table", "Cell", "Image"), Tag, strict=False)),
        member("galleyPreferences", "backgroundColor", Surface.WELL),
        member("galleyPreferences", "textColor", Text.PRIMARY),
        member("galleyPreferences", "displayFont", Typography.INTERFACE.family),
        member("galleyPreferences", "displayFontSize", 10.0),
        member("watermarkPreferences", "watermarkFontColor", Ink.DOCUMENT),
        member("viewPreferences", "pointsPerInch", float(round(Length.INCHES / Length.POINTS))),
        *(
            member("viewPreferences", f"{kind}MeasurementUnits", unit.name, enumeration="MeasurementUnits")
            for unit, kinds in ((units.page, (*axes, "printDialog")), (STROKE_UNITS[units], ("stroke",)), (Length.POINTS, ("typographic", "textSize")))
            for kind in kinds
        ),
        member("viewPreferences", "rulerOrigin", "PAGE_ORIGIN", enumeration="RulerOrigin"),
        member("viewPreferences", "cursorKeyIncrement", units.resolution / Length.POINTS),
        *(
            member("gridPreferences", f"{axis}{key}", value)
            for axis in axes
            for key, value in (("GridlineDivision", units.snap / Length.POINTS), ("GridSubdivision", round(units.snap / units.resolution)))
        ),
        member("documentPreferences", "pageSize", presets[0].sheet),
        *(member("marginPreferences", edge, units.margin / Length.POINTS) for edge in edges),
        member("pageItemDefaults", "strokeWeight", Pen.THIN / Length.POINTS),
        *(member("documentPresets", key, value, item=paper.title) for paper in presets for key, value in {"pageSize": paper.sheet, **dict.fromkeys(edges, paper.value.margin / Length.POINTS)}.items()),
        Row("documentPresets", Named("documentPresets"), tuple(sorted(("[Default]", *(paper.title for paper in presets))))),
        *(member("panels", key, "EXTRA_SMALL_ICON", item="Pages", enumeration="IconSizes") for key in ("iconSize", "masterIconSize")),
        *(
            row
            for name, rgb in swatches.items()
            for row in (
                member("colors", "model", "PROCESS", item=name, enumeration="ColorModel"),
                member("colors", "space", "RGB", item=name, enumeration="ColorSpace"),
                member("colors", "colorValue", rgb, item=name),
            )
        ),
        Row("swatches", Named("swatches"), tuple(sorted({*reserved, *SWATCHES}))),
        Row("generalPreferences.setActiveWorkspace", Active(), window.WORKSPACE),
        File(Folder.SETTINGS, DEFAULTS, recorded),
        packaged(prompt_source(PRODUCT.name, COMMANDS)),
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

__all__ = ["COMMANDS", "FRAME", "LEADER", "PRODUCT", "folders", "rows", "workspace"]
