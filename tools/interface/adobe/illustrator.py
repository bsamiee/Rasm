"""Illustrator's rows, toolbar, and Essentials frame, with the store text, New Document preset, and swatch exchange files it reads."""

from collections.abc import Mapping, Sequence
from enum import StrEnum
from functools import partial, reduce
from itertools import accumulate, starmap
from pathlib import Path
import re
import struct
from typing import Final

from lxml import etree
import msgspec

from interface import host
from interface.adobe import window
from interface.adobe.rows import Active, Paper, papers, Row, STROKE_UNITS, text_scale, Toolbar
from interface.adobe.session import Scripted
from interface.adobe.stores import Default, File, Folder
from interface.adobe.window import Role
from interface.render import DPI
from interface.report import single
from interface.roles import Alpha, blend, Guide, Ink, Line, Node, POINT_WIDTH, Selection, Surface, SWATCHES, TAGS
from interface.units import ANGLE_STEP, Length, Units

# --- [TYPES] ----------------------------------------------------------------------------

type Stored = int | float | str | bytes | tuple[tuple[str, Stored], ...]


class PreferenceType(StrEnum):
    """Type name the typed getters and setters of `app.preferences` spell."""

    REAL = "Real"
    INTEGER = "Integer"
    BOOLEAN = "Boolean"


# --- [CONSTANTS] ------------------------------------------------------------------------

ENCODING: Final = "latin-1"
PREFS: Final = "Adobe Illustrator Prefs"
PRESETS: Final = "PresetDocumentProfileDataV10.json"
ATTRIBUTES: Final = ("collection1", "attributes")
BOOKMARK: Final = "OWLBookMark"
TOOLBOX: Final = Toolbar(
    (
        ("Adobe Select Tool", "Adobe Crop Tool"),
        ("Adobe Direct Select Tool", "Adobe Direct Object Select Tool", "Adobe Direct Lasso Tool", "Adobe Magic Wand Tool"),
        ("Adobe Curvature Tool", "Adobe Pen Tool", "Adobe Add Anchor Point Tool", "Adobe Delete Anchor Point Tool", "Adobe Anchor Point Tool"),
        ("Adobe Shaper Tool", "Adobe Freehand Tool"),
        ("Adobe Freehand Smooth Tool", "Adobe Freehand Erase Tool"),
        ("Adobe Corner Join Tool",),
        ("Adobe Brush Tool", "Adobe Blob Brush Tool"),
        ("Adobe Line Tool", "Adobe Rectangle Shape Tool", "Adobe Ellipse Shape Tool", "Adobe Rounded Rectangle Tool"),
        ("Adobe Shape Construction Regular Polygon Tool", "Adobe Shape Construction Star Tool"),
        ("Adobe Arc Tool",),
        ("Adobe Shape Construction Spiral Tool",),
        ("Adobe Rectangular Grid Tool", "Adobe Polar Grid Tool"),
        ("Adobe Flare Tool", *(f"Adobe Symbol {kind} Tool" for kind in ("Sprayer", "Shifter", "Scruncher", "Sizer", "Spinner", "Stainer", "Screener", "Styler"))),
        tuple(f"Adobe {kind} Graph Tool" for kind in ("Column", "Stacked Column", "Bar", "Stacked Bar", "Line", "Area", "Scatter", "Pie", "Radar")),
        ("Perspective Grid Tool", "Perspective Selection Tool"),
        tuple(f"Adobe {kind} Tool" for kind in ("Type", "Area Type", "Path Type", "Vertical Type", "Vertical Area Type", "Vertical Path Type", "Touch Type")),
        ("Adobe Gradient Vector Tool",),
        ("Adobe Mesh Editing Tool",),
        ("Adobe Shape Builder Tool", "Adobe Planar Paintbucket Tool", "Adobe Planar Face Select Tool"),
        ("Adobe Eyedropper Tool",),
        ("Adobe Rotate Tool", "Adobe Reflect Tool"),
        ("Adobe Scale Tool", "Adobe Shear Tool"),
        ("Adobe Free Transform Tool", "Adobe Puppet Warp Tool"),
        tuple(f"Adobe {kind} Tool" for kind in ("Width", "Warp", "New Twirl", "Pucker", "Bloat", "Scallop", "Cyrstallize", "Wrinkle")),
        ("Adobe Blend Tool", "Adobe Constraints Tool"),
        ("Adobe Reshape Tool",),
        ("Adobe Eraser Tool", "Adobe Scissors Tool", "Adobe Knife Tool"),
        ("Adobe Dimension Tool", "Adobe Measure Tool"),
        ("Adobe Slice Tool", "Adobe Slice Select Tool"),
        ("Adobe Scroll Tool", "Adobe Zoom Tool", "Adobe Rotate Canvas Tool", "Adobe Page Tool"),
    ),
    frozendict({"CustomToolboxFillStroke": 1, "CustomToolboxColorMode": 0, "CustomToolboxDrawMode": 1, "CustomToolboxScreenMode": 0, "CustomToolboxGroupingMode": 0}),
)

# --- [MODELS] ---------------------------------------------------------------------------


class LargeInteger(int):
    """Serializer's largeInteger, written in digits with an `L` suffix."""


class Raw(bytes):
    """Serializer's raw data, written as its length and hex lines in angle brackets."""


class Preference(msgspec.Struct, frozen=True, tag=True):
    """`app.preferences` key through the typed getter and setter of its type."""

    key: str
    kind: PreferenceType


class Slots(msgspec.Struct, frozen=True, tag=True):
    """Integer key `<key>_<n>` of every startup profile slot `n` whose `<slot>_<n>` key the preferences hold."""

    key: str
    slot: str


class Profile(msgspec.Struct, frozen=True, tag=True):
    """Defaults of each startup profile document the `<slot>_<n>` keys name, compared in the document's color space and saved on difference."""

    slot: str


class Preset(msgspec.Struct, frozen=True, rename="camel"):
    """New Document preset row by its id, title, and source category."""

    id: str
    title: str
    preset_source: str


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [STORE_TEXT]
def entries(data: bytes) -> tuple[tuple[str, Stored], ...]:
    """Entries of store text to the first line that opens none, each key unescaped and each byte string keeping its escapes."""
    lines = iter(re.split(r"\r\n?|\n", data.decode(ENCODING)))

    def directory() -> tuple[tuple[str, Stored], ...]:
        return tuple(iter(lambda: entry(next(lines, "")), None))

    def entry(line: str) -> tuple[str, Stored] | None:
        match re.fullmatch(r"(\t*)/((?:\\.|[^ \\])*) (.*)", line):
            case None:
                return None
            case found:
                return re.sub(r"\\(.)", r"\1", found[2]), value(found[1], found[3])

    def value(tabs: str, token: str) -> Stored:
        match token[:1]:
            case "{":
                return directory()
            case "[":
                return bytes.fromhex("".join(line.strip() for line in iter(lambda: next(lines), f"{tabs}]"))).decode()
            case "<":
                return Raw(bytes.fromhex("".join(line.strip() for line in iter(lambda: next(lines), f"{tabs}>"))))
            case "(":
                return token[1:-1].encode(ENCODING)
            case _ if token.endswith("L"):
                return LargeInteger(token[:-1])
            case _:
                return int(token) if re.fullmatch(r"-?\d+", token) else float(token)

    return directory()


def serialized(tree: Sequence[tuple[str, Stored]]) -> bytes:
    """Store text of the entries as Illustrator's serializer writes them, each line ended by CR."""

    def lines(indent: str, tree: Sequence[tuple[str, Stored]]) -> str:
        def digits(data: bytes) -> str:
            return "".join(f"{indent}\t{data[start : start + 32].hex()}\r" for start in range(0, len(data), 32)) or "\r"

        def line(key: str, value: Stored) -> str:
            head = f"{indent}/{re.sub(r'[ \\]', r'\\\g<0>', key)}"
            match value:
                case tuple():
                    return f"{head} {{\r{lines(f'{indent}\t', value)}{indent}}}\r"
                case Raw():
                    return f"{head} < {len(value)}\r{digits(value)}{indent}>\r"
                case bytes():
                    return f"{head} ({value.decode(ENCODING)})\r"
                case str():
                    encoded = value.encode()
                    return f"{head} [ {len(encoded)}\r{digits(encoded)}{indent}]\r"
                case float():
                    real = f"{value:.10f}".rstrip("0")
                    return f"{head} {real}0\r" if real.endswith(".") else f"{head} {real}\r"
                case LargeInteger():
                    return f"{head} {value}L\r"
                case int():
                    return f"{head} {value}\r"

        return "".join(starmap(line, tree))

    return lines("", tree).encode(ENCODING)


def at(tree: Stored, path: Sequence[str]) -> Stored | None:
    """Value of the first entry at the directory path, None where no entry sits there."""
    match path, tree:
        case [], _:
            return tree
        case [head, *rest], tuple():
            return next((found for key, held in tree if key == head and (found := at(held, rest)) is not None), None)
        case _:
            return None


def replaced(tree: tuple[tuple[str, Stored], ...], path: Sequence[str], value: Stored) -> tuple[tuple[str, Stored], ...]:
    """Entries with the value in the first entry at the directory path that is a directory where the path continues and of the value's kind where it ends, each missing one appended."""
    head, *rest = path
    match tree:
        case ((key, tuple() as held), *after) if key == head and rest:
            return ((key, replaced(held, rest, value)), *after)
        case ((key, held), *after) if key == head and not rest and isinstance(held, tuple) == isinstance(value, tuple):
            return ((key, value), *after)
        case (first, *after):
            return (first, *replaced(tuple(after), path, value))
        case _:
            return ((head, replaced((), rest, value) if rest else value),)


# --- [FILES]
def ordered(tree: tuple[tuple[str, Stored], ...], held: bytes | None) -> bytes:
    """Store text of the entries, the held file's own where it holds them in any order."""

    def unordered(tree: Sequence[tuple[str, Stored]]) -> frozenset[tuple[str, type, object]]:
        return frozenset((key, type(value), unordered(value) if isinstance(value, tuple) else value) for key, value in tree)

    return held if held is not None and unordered(entries(held)) == unordered(tree) else serialized(tree)


def edited(path: Sequence[str], value: Stored, held: bytes | None) -> host.Result[bytes]:
    """Preferences with the entry at the directory path holding the value, each missing directory made, or the error naming the absent file."""
    match held:
        case None:
            return host.Error(f"{PREFS} is absent")
        case data:
            return serialized(replaced(entries(data), path, value))


def catalog(toolbar: Toolbar) -> tuple[tuple[str, Stored], ...]:
    """Toolbar catalog of one collection: a slot holding one tool by name and a flyout as a counted item set, the options as footer flags, then the catalog keys."""
    sets = tuple(accumulate((len(slot) > 1 for slot in toolbar.slots), initial=0))

    def items(index: int, slot: tuple[str, ...]) -> tuple[tuple[str, Stored], ...]:
        match slot:
            case (tool,):
                return ((f"CustomToolboxItem{index}", tool.encode()),)
            case _:
                return (
                    (f"CustomToolboxItem{index}", f"CustomToolboxItemSet{sets[index]}_Count_{len(slot)}".encode()),
                    *((f"CustomToolboxItemSet{sets[index]}_{member}", tool.encode()) for member, tool in enumerate(slot)),
                )

    attributes = (("collectionName", FRAME.toolbar), *(item for index, slot in enumerate(toolbar.slots) for item in items(index, slot)), *toolbar.options.items())
    return (
        ("collection1", (("attributes", attributes), ("canEdit", 1), ("canDelete", 1))),
        ("Sketch", (("Version", 0), ("Description", b"Adobe Custom Toolbar"), ("Owner", b""))),
        ("NumberOfCollections", 1),
        ("CatalogName", b"Adobe Custom Toolbar"),
    )


def profiles(presets: tuple[Paper, ...], held: bytes | None) -> host.Result[bytes]:
    """Preset list with every row outside the papers kept and one row per paper after them copied from the Letter preset: size in 32-bit points, the paper's unit, and the render resolution."""
    units, source = frozendict({Length.INCHES: "inchesUnit", Length.MILLIMETERS: "millimetersUnit"}), "print_0"
    listed = () if held is None else tuple(zip(msgspec.json.decode(held, type=tuple[msgspec.Raw, ...]), msgspec.json.decode(held, type=tuple[Preset, ...]), strict=True))
    kept = tuple((row, preset) for row, preset in listed if preset.title not in {paper.title for paper in presets})

    def raw(item: object) -> msgspec.Raw:
        return msgspec.Raw(msgspec.json.encode(item))

    def made(row: msgspec.Raw, origin: Preset, index: int, paper: Paper) -> bytes:
        fields = msgspec.json.decode(row, type=dict[str, msgspec.Raw])
        specific = msgspec.json.decode(fields["appSpecificKey"], type=dict[str, msgspec.Raw])
        width, height = (single(side / Length.POINTS) for side in paper.value.document)
        size = f"{round(width, 2):g} x {round(height, 2):g} pt"
        return msgspec.json.encode({
            **fields,
            "appSpecificKey": raw({**specific, "rasterEffectSettings": raw(float(DPI))}),
            "description": raw(size),
            "height": raw(height),
            "id": raw(f"{origin.preset_source}_{sum(preset.preset_source == origin.preset_source for _, preset in kept) + index}"),
            "tip": raw(f"Start a new {paper.title} document - {size}"),
            "title": raw(paper.title),
            "units": raw(units[paper.value.page]),
            "width": raw(width),
        })

    match held, [(row, preset) for row, preset in listed if preset.id == source]:
        case None, _:
            return host.Error(f"{PRESETS} is absent")
        case _, [(row, origin), *_]:
            return b"[" + b",".join((*(row for row, _ in kept), *(made(row, origin, index, paper) for index, paper in enumerate(presets)))) + b"]"
        case _:
            return host.Error(f"{PRESETS} holds no preset {source}")


def exchange(name: str, swatches: Mapping[str, tuple[int, int, int]]) -> bytes:
    """Adobe Swatch Exchange file of one group of RGB process swatches, each name u16-counted UTF-16 and each channel its byte over 255 as a 32-bit float."""

    def named(label: str) -> bytes:
        encoded = f"{label}\0".encode("utf-16-be")
        return struct.pack(">H", len(encoded) // 2) + encoded

    def block(kind: int, body: bytes) -> bytes:
        return struct.pack(">HI", kind, len(body)) + body

    blocks = (block(0xC001, named(name)), *(block(0x0001, named(swatch) + b"RGB " + struct.pack(">3fH", *(byte / 255 for byte in color), 2)) for swatch, color in swatches.items()), block(0xC002, b""))
    return struct.pack(">4sHHI", b"ASEF", 1, 0, len(blocks)) + b"".join(blocks)


# --- [WORKSPACE]
def workspace_file(factory: Sequence[etree._Element], file: str, held: bytes | None) -> bytes | tuple[host.Skip, ...] | host.Error:
    """Workspace catalog with its bookmark's block arranged in the frame, the panel view attributes set, and the Links panel's unread attributes gone, else the skip of a file or panel no workspace holds, or the error naming the bookmark the file lacks."""
    views = {
        **{
            f"{prefix} {key}": 3
            for prefix in ("AdobeSwatch_", "AdobeFillSwatchesPopup", "AdobeStrokeSwatchesPopup", "PatternOptionsPanel")
            for key in ("ShowAllListStyle", "ColorListStyle", "GradientListStyle", "PatternListStyle", "GroupsListStyle", "CurrentListStyle")
        },
        "NamedStyle StylesView": 1,
        "NamedStyle ThumbnailType": 0,
        "Symbols VisibleKind": 1,
        "Links List Entry Height": 16,
        "Links Show Icons (boolean)": 0,
        "ShowCommentsPanelBool": 0,
        "ShowDLPanelBool": 0,
        "AgentsUIPanelClosedByUser": 1,
    }
    unread = frozenset(f"AdobeLinkPalette {key}" for key in ("List Entry Height", "Show Icons (boolean)", "Show Transparency Interaction"))
    tree = () if held is None else entries(held)
    match held, at(tree, ATTRIBUTES), at(tree, (*ATTRIBUTES, BOOKMARK)):
        case None, _, _:
            return (host.Skip(file),)
        case _, tuple() as attributes, str() as block:
            match window.arranged(FRAME, window.parsed(block.encode()), factory):
                case tuple() as skipped:
                    return skipped
                case root:
                    kept = tuple((key, value) for key, value in attributes if key not in unread)
                    viewed = reduce(lambda directory, key: replaced(directory, (key,), views[key]), views, kept)
                    return serialized(replaced(tree, ATTRIBUTES, replaced(viewed, (BOOKMARK,), f"{window.serialized(root)}\n")))
        case _:
            return host.Error(f"{file} holds no {BOOKMARK}")


def workspace(factory: Sequence[bytes]) -> tuple[File, ...]:
    """Modified Essentials workspace arranged from the one Illustrator wrote, taking panels it lacks from the factory workspaces in name order."""
    stock = tuple(window.parsed(block.encode()) for data in factory if isinstance(block := at(entries(data), (*ATTRIBUTES, BOOKMARK)), str))
    file = f"Modified Workspaces/{window.WORKSPACE}"
    return (File(Folder.PREFERENCES, file, partial(workspace_file, stock, file)),)


# --- [ROWS]
def preference(key: str, *, target: bool | float) -> Row:
    """Row of an `app.preferences` key through the typed accessor of the target's type, a real as the ten decimals of its 32-bit float the prefs file keeps."""
    label = f'preferences["{key}"]'
    match target:
        case bool():
            return Row(label, Preference(key, PreferenceType.BOOLEAN), target)
        case int():
            return Row(label, Preference(key, PreferenceType.INTEGER), target)
        case float():
            return Row(label, Preference(key, PreferenceType.REAL), round(single(target), 10))


def folders(bundle: host.Bundle, base: Mapping[Folder, Path]) -> Mapping[Folder, Path]:
    """Illustrator's settings and preferences folders of the bundle's settings name in the reported folder's locale, a Beta's name holding its version and a release's its major version, and the factory workspaces of that locale beside the bundle."""
    beta, release = PRODUCT.identifiers
    named, locale = {beta: f"Adobe Illustrator {bundle.version} Beta Settings", release: f"Adobe Illustrator {bundle.version.partition('.')[0]}"}[bundle.identifier], base[Folder.SETTINGS].name
    return frozendict({
        Folder.SETTINGS: base[Folder.HOME].joinpath("Library", "Application Support", "Adobe", named, locale),
        Folder.PREFERENCES: base[Folder.HOME].joinpath("Library", "Preferences", named, locale),
        Folder.FACTORY: bundle.path.parent.joinpath("Presets.localized", locale, "Workspaces"),
    })


def rows(units: Units, bundle: host.Bundle) -> tuple[Row | File | Default, ...]:
    """Illustrator's rows with the interface scaled to draw panel text at the interface text size, lengths in the system's page unit, type in points, and strokes in the system's stroke unit, the grid solved over paper at the darkest theme's grid alpha, color channels as reals or 16-bit integers."""
    rgb, capitalized, grid_alpha, anchors = ("red", "green", "blue"), ("Red", "Green", "Blue"), 0.3, 0x2AA0
    scale = text_scale(bundle.path.joinpath("Contents", "Required", "Plug-ins", "UserInterface.aip", "Contents", "Resources", "xml", "FontTheme_Panel.xml"))
    codes = frozendict({Length.INCHES: 0, Length.MILLIMETERS: 1, Length.POINTS: 2})
    anchor = min((size for size in range(anchors.bit_length()) if anchors >> size & 1), key=lambda size: abs(size + 2 * int(2 * (scale % 1)) - POINT_WIDTH))
    angles = tuple(float(index * ANGLE_STEP) for index in range(6))
    ruler, profile, axes = "rulerType", "startupFileType", ("Horizontal", "Vertical")
    reals = {
        "Guide/Color/{}": (rgb, Guide.CONSTRUCTION),
        "Grid/Color/Dark/{}": (("r", "g", "b"), blend(Line.PAPER_GRID, Surface.PAPER, 1 / grid_alpha)),
        "Grid/Color/Lite/{}": (("r", "g", "b"), blend(blend(Line.PAPER_GRID, Surface.PAPER, Alpha.GRID_MINOR), Surface.PAPER, 1 / grid_alpha)),
        "snapomatic/Color/{}_19_2": (rgb, Guide.TRACKING),
        "snapomatic/GlyphColor/{}": (rgb, Guide.TRACKING),
        "ArtboardBBColor{}": (capitalized, Node.BORDER),
    }
    wide = {
        "plugin/AdobeSlicingPlugin/feedback/{}": (rgb, Guide.HANDLE),
        "Planar/MergeTool/Highlight/StrokeColor/{}": (capitalized, Selection.HOVER),
        **{f"Planar/{tool}/Highlight/Color/{{}}": (rgb, Selection.HOVER) for tool in ("FaceSelect", "Paintbucket")},
        "Planar/GapDetection/GapColor/Color/{}": (rgb, Guide.TRACKING),
    }
    preferences = {
        "uiBrightness": 0.0,
        "uiCanvasIsWhite": False,
        "UIPreferences/appScaleFactor": scale,
        "UIPreferences/workspaceTabsSize": 1,
        "uiShareButtonIsBlue": False,
        "text/fontMenu/faceSizeMultiplier": 0.0,
        "Hello/ShowHomeScreenWS": False,
        "Hello/NewDoc": False,
        "aiShowSystemCompatibilityIssuesAtStartup": False,
        "plugin/AIAgenticSystem/ConsentSendDataToThirdParty": False,
        "plugin/AIMCPServer/ServerEnabled": True,
        "plugin/AIMCPServer/ShowConnectionStatusOnHeader": False,
        "plugin/AgenticUI/ShowAgenticUIPanelPreference2": False,
        "showHelpBar": False,
        "ContextualTaskBarEnabled": False,
        "Performance/AnimZoom": False,
        "showToolTips": True,
        "showRichToolTips": False,
        "zoomWithMouseWheel": True,
        "globalRulersVisible": True,
        "showBoundingBox": True,
        "anchorSizePref": anchor,
        "DontShowMissingFontDialogPreference": True,
        "AI WorldReadiness Dict Key": 2,
        "Guide/Style": 0,
        "Grid/Style": 0,
        "Grid/Posn": False,
        **{
            key: value
            for group in ("angles", "customAngles")
            for key, value in ((f"smartGuides/{group}Count", len(angles)), *((f"smartGuides/{group}{index}", angle) for index, angle in enumerate(angles)))
        },
        "ArtboardBBWidth": 1.0,
        "Planar/MergeTool/Highlight/StrokeColorIndex": 0,
        **dict.fromkeys((f"Planar/{tool}/Highlight" for tool in ("FaceSelect", "Paintbucket")), True),
        ruler: codes[units.page],
        "strokeUnits": codes[STROKE_UNITS[units]],
        "text/units": codes[Length.POINTS],
        "cursorKeyLength": units.resolution / Length.POINTS,
        **dict.fromkeys((f"Grid/{axis}/Spacing" for axis in axes), units.snap / Length.POINTS),
        **dict.fromkeys((f"Grid/{axis}/Ticks" for axis in axes), round(units.snap / units.resolution)),
        **{template.format(name): byte / 255 for template, (names, color) in reals.items() for name, byte in zip(names, color, strict=True)},
        **{template.format(name): byte * 257 for template, (names, color) in wide.items() for name, byte in zip(names, color, strict=True)},
    }
    return (
        *(preference(key, target=target) for key, target in preferences.items()),
        Row(f'preferences["{ruler}_<n>"]', Slots(ruler, profile), codes[units.page]),
        Row(f'preferences["{profile}_<n>"]', Profile(profile), {"stroke": Ink.DOCUMENT, "resolution": float(DPI), "group": TAGS, "swatches": tuple(SWATCHES.items())}),
        Row('preferences["plugin/WorkspacePrefix/Last Used Workspace Name"]', Active(), window.WORKSPACE),
        File(Folder.SETTINGS, f"Swatches/{TAGS}.ase", lambda _: exchange(TAGS, SWATCHES)),
        File(Folder.PREFERENCES, "Tools/Tools Panel Presets", partial(ordered, catalog(TOOLBOX))),
        File(Folder.PREFERENCES, PRESETS, partial(profiles, papers(units))),
        File(Folder.PREFERENCES, PREFS, partial(edited, ("plugin", "AdobeBrush", "ThumbnailView"), 0)),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

FRAME: Final = window.Frame(
    toolbar="Default Toolbar",
    bar=False,
    groups=frozendict({
        Role.LIBRARIES: ("CSXSExtension_com.adobe.DesignLibraries.angular_0",),
        Role.TRANSFORM: ("Transform",),
        Role.CHARACTER: ("Character", "Paragraph", "AdobeOpenTypeFont"),
        Role.STYLES: ("AdobeParaStyles", "AdobeCharStyles"),
        Role.GLYPHS: ("AdobeAltGlyphsPanel",),
        Role.ASSETS: ("AdobeBrush", "NamedStyle", "Symbols"),
        Role.PATTERN: ("PatternOptionsPanel",),
        Role.BLEND: ("AdobeBlendOptionsPanel",),
        Role.EFFECTS: ("Vectorize", "3D and Materials"),
        Role.PROPERTIES: ("AdobePropertiesPanel", "Appearance", "Stroke", "Transparency"),
        Role.ALIGNMENT: ("Align", "Pathfinder"),
        Role.DOCUMENT: ("Artboards", "Links"),
        Role.COLOR: ("AdobeSwatch_", "Color", "AdobeGradientEditor"),
        Role.AUTOMATION: ("Actions", "History"),
        Role.STRUCTURE: ("AdobeLayerPalette", "Navigator"),
        Role.EXPORT: ("Export",),
        Role.INFORMATION: ("Document Info", "Info"),
        Role.OUTPUT: ("AdobeSeparationPreview", "AdobeObjectAttributes"),
    }),
    fixed=frozenset({"AdobePropertiesPanel", "Stroke", "Transparency", "Align", "Pathfinder", "AdobeGradientEditor"}),
    hidden=frozenset({"Stroke", "Transparency", "AdobeBlendOptionsPanel"}),
    states=frozendict({"Character": 2, "Paragraph": 1, "Transform": 1, "Align": 1, "Color": 1}),
    identity=lambda element: element.attrib[window.Attribute.DATA],
    sized=window.sized,
    closed=partial(window.floated, closed=True),
)
PRODUCT: Final = Scripted(
    name="illustrator", identifiers=("com.adobe.illustratorBeta", "com.adobe.illustrator"), command="«event miscDjxM»", arguments="given «class JArg»:", rows=rows, folders=folders, workspace=workspace
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FRAME", "PRODUCT", "folders", "rows", "workspace"]
