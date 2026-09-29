"""Illustrator's rows, alias commands, toolbar, and Essentials frame, with the store text, New Document preset, swatch exchange, and action files it reads."""

from collections.abc import Mapping, Sequence
import ctypes
from enum import StrEnum
from functools import partial, reduce
from itertools import accumulate, batched, starmap
from pathlib import Path
import re
import struct
from typing import Final

from lxml import etree
import msgspec

from interface import host
from interface.adobe import window
from interface.adobe.rows import ActionSet, Active, Menu, Paper, papers, PROMPT_NAME, prompt_source, Row, STROKE_UNITS, Tool, Toolbar
from interface.adobe.session import Scripted
from interface.adobe.stores import Default, File, Folder, UxpPlugin
from interface.aliases import Alias
from interface.frame import Role
from interface.render import DPI
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
LEADER: Final = 9
PREFS: Final = "Adobe Illustrator Prefs"
PRESETS: Final = "PresetDocumentProfileDataV10.json"
ATTRIBUTES: Final = ("collection1", "attributes")
BOOKMARK: Final = "OWLBookMark"
COMMANDS: Final[frozendict[Alias, Tool | Menu]] = frozendict({
    Alias.Q: Tool("Adobe Line Tool"),
    Alias.QQ: Tool("Adobe Pen Tool"),
    Alias.QW: Tool("Adobe Arc Tool"),
    Alias.QE: Tool("Adobe Curvature Tool"),
    Alias.QR: Menu("Path Blend Make", selection=True),
    Alias.W1: Tool("Adobe Rectangle Shape Tool"),
    Alias.W3: Tool("Adobe Rounded Rectangle Tool"),
    Alias.WQ: Tool("Adobe Shape Construction Regular Polygon Tool"),
    Alias.E: Tool("Adobe Ellipse Shape Tool"),
    Alias.R: Tool("Adobe Rotate Tool"),
    Alias.R2: Tool("Adobe Reflect Tool"),
    Alias.T: Tool("Adobe Type Tool"),
    Alias.TT: Menu("outline", selection=True),
    Alias.T3: Tool("Adobe Scale Tool"),
    Alias.T4: Menu("Transform3", selection=True),
    Alias.TW: Tool("Adobe Shear Tool"),
    Alias.AQ: Tool("Adobe Shape Builder Tool"),
    Alias.D: Tool("Adobe Measure Tool"),
    Alias.FF: Menu("join", selection=True),
    Alias.FQ: Tool("Adobe Scissors Tool"),
    Alias.FD: Tool("Adobe Knife Tool"),
    Alias.G: Menu("group", selection=True),
    Alias.GU: Menu("ungroup", selection=True),
    Alias.GH: Menu("hide", selection=True),
    Alias.GJ: Menu("showAll", selection=False),
    Alias.GL: Menu("lock", selection=True),
    Alias.GP: Menu("unlockAll", selection=False),
    Alias.GW: Menu("makeguide", selection=True),
    Alias.GE: Menu("clearguide", selection=False),
    Alias.Z: Tool("Adobe Zoom Tool"),
    Alias.ZE: Menu("fitall", selection=False),
    Alias.V: Tool("Adobe Select Tool"),
    Alias.VA: Menu("AdobeAlignObjects2", selection=False),
    Alias.VO: Menu("selectall", selection=False),
    Alias.VI: Menu("Inverse menu item", selection=False),
    Alias.B: Menu("Adobe New Symbol Shortcut", selection=True),
    Alias.BE: Menu("Adobe Symbol Palette", selection=False),
    Alias.IM: Menu("AI Place", selection=False),
})
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
            return "".join(f"{row}\r" for row in [f"{indent}\t{''.join(chunk)}" for chunk in batched(data.hex(), 64, strict=False)] or [""])

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


def edited(path: Sequence[str], value: Stored, held: bytes | None) -> bytes | host.Error:
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


def profiles(presets: tuple[Paper, ...], held: bytes | None) -> bytes | host.Error:
    """Preset list with every row outside the papers kept and one row per paper after them copied from the Letter preset: size in 32-bit points, the paper's unit, and the render resolution."""
    units, source = frozendict({Length.INCHES: "inchesUnit", Length.MILLIMETERS: "millimetersUnit"}), "print_0"
    listed = () if held is None else tuple(zip(msgspec.json.decode(held, type=tuple[msgspec.Raw, ...]), msgspec.json.decode(held, type=tuple[Preset, ...]), strict=True))
    kept = tuple((row, preset) for row, preset in listed if preset.title not in {paper.title for paper in presets})

    def raw(item: object) -> msgspec.Raw:
        return msgspec.Raw(msgspec.json.encode(item))

    def made(row: msgspec.Raw, origin: Preset, index: int, paper: Paper) -> bytes:
        fields = msgspec.json.decode(row, type=dict[str, msgspec.Raw])
        specific = msgspec.json.decode(fields["appSpecificKey"], type=dict[str, msgspec.Raw])
        width, height = (ctypes.c_float(side / Length.POINTS).value for side in paper.value.document)
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


def aia(key: int) -> bytes:
    """Alias action set on the key index playing the Scripts menu item of the alias entry file through the Access Menu Item event."""
    parameters = (("itnm", ""), ("lcnm", PROMPT_NAME))
    return serialized((
        ("version", 3),
        ("name", PROMPT_NAME),
        ("isOpen", 0),
        ("actionCount", 1),
        (
            "action-1",
            (
                ("name", PROMPT_NAME),
                ("keyIndex", key),
                ("colorIndex", 0),
                ("isOpen", 0),
                ("eventCount", 1),
                (
                    "event-1",
                    (
                        ("useRulersIn1stQuadrant", 1),
                        ("internalName", b"adobe_commandManager"),
                        ("localizedName", "Access Menu Item"),
                        ("isOpen", 0),
                        ("isOn", 1),
                        ("hasDialog", 0),
                        ("parameterCount", len(parameters)),
                        *(
                            (f"parameter-{index}", (("key", int.from_bytes(name.encode())), ("showInPalette", 0xFFFFFFFF), ("type", b"ustring"), ("value", value)))
                            for index, (name, value) in enumerate(parameters, start=1)
                        ),
                    ),
                ),
            ),
        ),
    ))


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
            return Row(label, Preference(key, PreferenceType.REAL), round(ctypes.c_float(target).value, 10))


def channels(template: str, names: tuple[str, str, str], target: tuple[int, int, int], *, sixteen_bit: bool = False) -> tuple[Row, ...]:
    """Rows of a color held as one preference key per channel name the template spells, each byte over 255 as a real or scaled to 16 bits as an integer."""
    return tuple(preference(template.format(name), target=byte * 257 if sixteen_bit else byte / 255) for name, byte in zip(names, target, strict=True))


def folders(bundle: host.Bundle, base: Mapping[Folder, Path]) -> Mapping[Folder, Path]:
    """Illustrator's preferences folder of the reported settings folder's name and locale, and the factory workspaces and Scripts folder of that locale beside the bundle."""
    settings = base[Folder.SETTINGS]
    presets = bundle.path.parent.joinpath("Presets.localized", settings.name)
    return frozendict({
        Folder.PREFERENCES: base[Folder.HOME].joinpath("Library", "Preferences", settings.parent.name, settings.name),
        Folder.FACTORY: presets / "Workspaces",
        Folder.SCRIPTS: presets / "Scripts",
    })


def rows(units: Units) -> tuple[Row | File | Default | UxpPlugin, ...]:
    """Illustrator's rows with lengths in the system's page unit, type in points, and strokes in the system's stroke unit, the grid solved over paper at the darkest theme's grid alpha."""
    rgb, capitalized, scale, grid_alpha, anchors = ("red", "green", "blue"), ("Red", "Green", "Blue"), 1.0, 0.3, 0x2AA0
    codes = frozendict({Length.INCHES: 0, Length.MILLIMETERS: 1, Length.POINTS: 2})
    sixteen_bit = partial(channels, sixteen_bit=True)
    anchor = min((size for size in range(anchors.bit_length()) if anchors >> size & 1), key=lambda size: abs(size + 2 * int(2 * (scale % 1)) - POINT_WIDTH))
    angles = tuple(float(index * ANGLE_STEP) for index in range(6))
    ruler, profile, actions = "rulerType", "startupFileType", f"{PROMPT_NAME}.aia"
    return (
        preference("uiBrightness", target=0.0),
        preference("uiCanvasIsWhite", target=False),
        preference("UIPreferences/appScaleFactor", target=scale),
        preference("UIPreferences/workspaceTabsSize", target=0),
        preference("uiShareButtonIsBlue", target=False),
        preference("text/fontMenu/faceSizeMultiplier", target=0.0),
        preference("Hello/ShowHomeScreenWS", target=False),
        preference("Hello/NewDoc", target=False),
        preference("aiShowSystemCompatibilityIssuesAtStartup", target=False),
        preference("plugin/AIAgenticSystem/ConsentSendDataToThirdParty", target=False),
        preference("plugin/AIMCPServer/ServerEnabled", target=True),
        preference("plugin/AIMCPServer/ShowConnectionStatusOnHeader", target=False),
        preference("plugin/AgenticUI/ShowAgenticUIPanelPreference2", target=False),
        preference("showHelpBar", target=False),
        preference("ContextualTaskBarEnabled", target=False),
        preference("Performance/AnimZoom", target=False),
        preference("showToolTips", target=True),
        preference("showRichToolTips", target=False),
        preference("zoomWithMouseWheel", target=True),
        preference("globalRulersVisible", target=True),
        preference("showBoundingBox", target=True),
        preference("anchorSizePref", target=anchor),
        preference("ReplacingLinks", target=0),
        preference("DontShowMissingFontDialogPreference", target=True),
        preference("AI WorldReadiness Dict Key", target=2),
        *channels("Guide/Color/{}", rgb, Guide.CONSTRUCTION),
        preference("Guide/Style", target=0),
        *channels("Grid/Color/Dark/{}", ("r", "g", "b"), blend(Line.PAPER_GRID, Surface.PAPER, 1 / grid_alpha)),
        *channels("Grid/Color/Lite/{}", ("r", "g", "b"), blend(blend(Line.PAPER_GRID, Surface.PAPER, Alpha.GRID_MINOR), Surface.PAPER, 1 / grid_alpha)),
        preference("Grid/Style", target=0),
        preference("Grid/Posn", target=False),
        *channels("snapomatic/Color/{}_19_2", rgb, Guide.TRACKING),
        *channels("snapomatic/GlyphColor/{}", rgb, Guide.TRACKING),
        *(
            row
            for group in ("angles", "customAngles")
            for row in (preference(f"smartGuides/{group}Count", target=len(angles)), *(preference(f"smartGuides/{group}{index}", target=angle) for index, angle in enumerate(angles)))
        ),
        *channels("ArtboardBBColor{}", capitalized, Node.BORDER),
        preference("ArtboardBBWidth", target=1.0),
        *sixteen_bit("plugin/AdobeSlicingPlugin/feedback/{}", rgb, Guide.HANDLE),
        preference("Planar/MergeTool/Highlight/StrokeColorIndex", target=0),
        *sixteen_bit("Planar/MergeTool/Highlight/StrokeColor/{}", capitalized, Selection.HOVER),
        *(preference(f"Planar/{tool}/Highlight", target=True) for tool in ("FaceSelect", "Paintbucket")),
        *(row for tool in ("FaceSelect", "Paintbucket") for row in sixteen_bit(f"Planar/{tool}/Highlight/Color/{{}}", rgb, Selection.HOVER)),
        *sixteen_bit("Planar/GapDetection/GapColor/Color/{}", rgb, Guide.TRACKING),
        preference(ruler, target=codes[units.page]),
        Row(f'preferences["{ruler}_<n>"]', Slots(ruler, profile), codes[units.page]),
        preference("strokeUnits", target=codes[STROKE_UNITS[units]]),
        preference("text/units", target=codes[Length.POINTS]),
        preference("cursorKeyLength", target=units.resolution / Length.POINTS),
        *(preference(f"Grid/{axis}/Spacing", target=units.snap / Length.POINTS) for axis in ("Horizontal", "Vertical")),
        *(preference(f"Grid/{axis}/Ticks", target=round(units.snap / units.resolution)) for axis in ("Horizontal", "Vertical")),
        Row(f'preferences["{profile}_<n>"]', Profile(profile), {"stroke": Ink.DOCUMENT, "resolution": float(DPI), "group": TAGS, "swatches": tuple(SWATCHES.items())}),
        Row(f'preferences["plugin/Action/SavedSets"]["{PROMPT_NAME}"].keyIndex', ActionSet(PROMPT_NAME, actions), LEADER),
        Row('preferences["plugin/WorkspacePrefix/Last Used Workspace Name"]', Active(), window.WORKSPACE),
        File(Folder.ARTIFACTS, actions, lambda _: aia(LEADER)),
        File(Folder.SCRIPTS, f"{PROMPT_NAME}.jsx", lambda _: prompt_source(PRODUCT.name, COMMANDS).encode()),
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

__all__ = ["COMMANDS", "FRAME", "LEADER", "PRODUCT", "folders", "rows", "workspace"]
