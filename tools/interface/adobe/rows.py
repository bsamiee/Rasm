"""Rows of Illustrator, Photoshop, InDesign, and Acrobat in one unit system, each with the accessor that reaches its store."""

from collections.abc import Mapping
import ctypes
from enum import Enum, StrEnum
from fractions import Fraction
from functools import partial
from itertools import starmap
import math
from typing import Self

import msgspec

from interface.adobe import aliases, workspaces
from interface.adobe.aliases import LABEL, Prompt
from interface.adobe.workspaces import Frame
from interface.render import DPI
from interface.roles import Alpha, blend, Guide, Ink, Line, Node, POINT_WIDTH, Selection, Status, Surface, SWATCHES, Tag, TAGS, Text, Typography
from interface.units import ANGLE_STEP, Length, Pen, Units

# --- [TYPES] ----------------------------------------------------------------------------

type Scalar = bool | float | str
type Value = Boolean | Integer | Double | UnitDouble | Enumerated | RgbColor | Members
type Target = Scalar | tuple[int, int, int] | Fixed | Value | tuple[str, ...] | tuple[Swatch, ...] | Toolbar | Library | DocumentPresets | Frame | Prompt | Template


class Kind(StrEnum):
    """Illustrator preference type as its typed getter and setter spell it."""

    REAL = "Real"
    INTEGER = "Integer"
    BOOLEAN = "Boolean"


class Script(msgspec.Struct, frozen=True):
    """Dictionary command that runs `script.jsx` in a product with its arguments besides the source."""

    command: str
    arguments: Mapping[str, str] = frozendict({})


class Product(Enum):
    """Adobe application by the bundle id its installed bundle declares and the script command that reaches it, none for Acrobat, which the file stage alone reaches."""

    ILLUSTRATOR = ("com.adobe.illustratorBeta", Script("do javascript"))
    PHOTOSHOP = ("com.adobe.Photoshop", Script("do javascript"))
    INDESIGN = ("com.adobe.InDesign", Script("do script", frozendict({"language": "javascript"})))
    ACROBAT = ("com.adobe.Acrobat.Pro", None)

    def __init__(self, identifier: str, script: Script | None) -> None:
        """Bind the member's tuple to its named fields."""
        self.identifier, self.script = identifier, script


class Unit(float, Enum):
    """Length unit keyed by its length in meters, with each product's spelling of it and the rounded inch ratios Photoshop converts a held length with."""

    POINTS = (Length.POINTS, 2, "POINTS", "rulerPoints", "pointsUnit", 0, Fraction(1, round(Length.INCHES / Length.POINTS)), Fraction(round(Length.INCHES / Length.POINTS)))
    INCHES = (Length.INCHES, 0, "INCHES", "rulerInches", "inchesUnit", 1, Fraction(1), Fraction(1))
    MILLIMETERS = (Length.MILLIMETERS, 1, "MM", "rulerMm", "millimetersUnit", 2, Fraction("0.03937"), Fraction("25.4"))

    def __new__(cls, length: float, *_fields: object) -> Self:
        """Member whose value is its length in meters."""
        member = float.__new__(cls, length)
        member._value_ = length
        return member

    def __init__(self, _length: float, illustrator: int, photoshop: str, ruler: str, preset_id: str, acrobat: int, inches: Fraction, per_inch: Fraction) -> None:
        """Bind the member's product spellings and held-inch pair to their named fields."""
        self.illustrator, self.photoshop, self.ruler, self.preset_id, self.acrobat, self.inches, self.per_inch = illustrator, photoshop, ruler, preset_id, acrobat, inches, per_inch


# --- [MODELS] ---------------------------------------------------------------------------


class Boolean(msgspec.Struct, frozen=True, array_like=True, tag="boolean"):
    """Descriptor boolean."""

    value: bool


class Integer(msgspec.Struct, frozen=True, array_like=True, tag="integer"):
    """Descriptor integer."""

    value: int


class Double(msgspec.Struct, frozen=True, array_like=True, tag="double"):
    """Descriptor double."""

    value: float


class UnitDouble(msgspec.Struct, frozen=True, array_like=True, tag="unitDouble"):
    """Descriptor double in the unit the store already holds for its key."""

    value: float


class Enumerated(msgspec.Struct, frozen=True, array_like=True, tag="enumerated"):
    """Descriptor enumeration member by the string ids of its enumeration and value."""

    enumeration: str
    value: str


class RgbColor(msgspec.Struct, frozen=True, array_like=True, tag="RGBColor"):
    """Descriptor `RGBColor` object in 0-255 channels, compared as the bytes it rounds to."""

    red: int
    green: int
    blue: int


class Members(msgspec.Struct, frozen=True, array_like=True, tag="object"):
    """Descriptor object of the class its string id names, holding the members a row sets."""

    kind: str
    members: Mapping[str, Value]


class Fixed(msgspec.Struct, frozen=True):
    """Acrobat 16.16 fixed-point number: an integer leaf holds it times 0x10000, a real leaf that integer over 0x10000."""

    value: float

    @classmethod
    def channel(cls, byte: int) -> Self:
        """Fraction of full intensity a color byte states."""
        return cls(byte / 255)


class Swatch(msgspec.Struct, frozen=True, array_like=True):
    """Named RGB process swatch."""

    name: str
    color: tuple[int, int, int]


class Paper(msgspec.Struct, frozen=True):
    """New Document paper with its name, its width and height in meters, and the unit it shows."""

    name: str
    size: tuple[float, float]
    unit: Unit


class DocumentPresets(msgspec.Struct, frozen=True):
    """New Document presets with their papers in order."""

    papers: tuple[Paper, ...]


class Library(msgspec.Struct, frozen=True):
    """Swatch library with its group name and its swatches in order."""

    name: str
    swatches: tuple[Swatch, ...]


class Toolbar(msgspec.Struct, frozen=True):
    """Toolbar with its slots in order, each a flyout of tool ids with the shown tool first, and its footer options by key."""

    slots: tuple[tuple[str, ...], ...]
    options: Mapping[str, int]


class Template(msgspec.Struct, frozen=True):
    """Startup profile document defaults a new document takes from its profile: default stroke color, raster effects resolution, and swatch group."""

    stroke: tuple[int, int, int]
    resolution: float
    library: Library


class Access(msgspec.Struct, frozen=True, tag_field="access", tag=str.lower):
    """Store a row reaches, tagged by the accessor `script.jsx` or the file stage serves it through."""


class Preference(Access, frozen=True):
    """Illustrator `app.preferences` key through the typed accessor of its kind."""

    key: str
    kind: Kind


class Channels(Access, frozen=True):
    """Illustrator color held as one preference key of one kind per channel, each channel stored as its byte over 255 times the scale."""

    keys: tuple[str, ...]
    kind: Kind
    scale: int


class Slots(Access, frozen=True):
    """Illustrator integer key `<key>_<n>` for every startup profile slot `n` whose `<slot>_<n>` key the store holds."""

    key: str
    slot: str


class Descriptor(Access, frozen=True):
    """Photoshop application descriptor property, read through `executeActionGet` and set through `setd`."""

    owner: str


class Canvas(Access, frozen=True):
    """Photoshop canvas of every screen mode the list at the owner descriptor's key holds, drawn in a custom color without a frame."""

    owner: str
    key: str


class Property(Access, frozen=True):
    """Application property by its dotted path from `app`, a color read as the bytes it rounds to, holding a member of the named DOM enumeration where one is named."""

    path: str
    enumeration: str | None = None


class Preset(Access, frozen=True):
    """Property of the named InDesign document preset, the preset made when absent."""

    preset: str
    name: str


class Presets(Access, frozen=True):
    """Names of the InDesign document presets past the reserved ones, every other name outside the target removed."""

    reserved: tuple[str, ...]


class Panel(Access, frozen=True):
    """Member of the named InDesign panel holding a member of the named DOM enumeration."""

    panel: str
    member: str
    enumeration: str


class Swatches(Access, frozen=True):
    """InDesign application swatches in panel order past the ones InDesign refuses to delete, converged whole to the target."""

    reserved: tuple[str, ...]


class Active(Access, frozen=True):
    """Name of the product's active workspace, the named workspace applied through the application command."""

    command: str


class Profile(Access, frozen=True):
    """Defaults of each Illustrator startup profile document the `<slot>_<n>` keys name, written into the document and saved, compared in the color space the document holds."""

    slot: str


class Leaf(Access, frozen=True):
    """Acrobat preference domain leaf under the `DC` hive by its key path and typecode."""

    path: tuple[str, ...]
    code: int


class Shared(Access, frozen=True):
    """Top-level key of a preference domain every Adobe application shares, by the domain and the key."""

    domain: str
    key: str


class Serialized(Access, frozen=True):
    """Top-level item of a Photoshop settings file holding one serialized action descriptor (8BPF), by the file name, the item key, and its four-character type."""

    file: str
    key: str
    code: str


class Rendered(Access, frozen=True):
    """File under the product's folder the file stage renders whole from a target of its type: Illustrator's user swatch library, or Photoshop's swatch list, New Document presets, and toolbar record."""

    file: str


class Catalog(Access, frozen=True):
    """Illustrator toolbar catalog under the settings folder holding one collection of the name, the toolbar's options its footer flags."""

    file: str
    collection: str


class Profiles(Access, frozen=True):
    """Illustrator New Document preset list under the settings folder, each paper a row copied from the source preset opening the source's profile in the profile folder under the application support settings folder."""

    file: str
    source: str
    folder: str


class Overlay(Access, frozen=True):
    """UXP plug-in data file under the plug-in's storage folder holding one color as lowercase `#rrggbb`, written while the product's plug-in registry lists the plug-in enabled."""

    file: str
    registry: str
    product: str
    plugin: str


class ActionSet(Access, frozen=True):
    """Alias action set in the product's saved action sets under the settings folder with one action on the leader function key running the entry script under the presets folder, through the Scripts menu item in Illustrator and `$.evalFile` in Photoshop."""

    file: str
    entry: str


class Nested(Access, frozen=True):
    """Integer entry of Illustrator's preferences under the settings folder by its directory path, for a panel that reads it at launch and writes its own value back at quit."""

    file: str
    path: tuple[str, ...]


class Workspace(Access, frozen=True):
    """Workspace files under the settings folder the product's write launch reports, the one the product rewrites at quit first, arranged in the frame from the workspace the product wrote at that launch's quit."""

    files: tuple[str, ...]


class Record(Access, frozen=True):
    """Field of every copy of an object record in an InDesign paged database under the settings folder the product's write launch reports, by the file, the record's implementation id and stream length, and the field's body offset and struct format, edited after that launch's quit."""

    file: str
    implementation: int
    length: int
    offset: int
    form: str


class Plugin(Access, frozen=True):
    """UXP command plug-in under Adobe's UXP folder by its id, version, and host app token, its one command on the leader shortcut running the alias entry."""

    plugin: str
    version: str
    host: str


Filed = Leaf | Shared | Serialized | Rendered | Catalog | Profiles | Overlay | ActionSet | Nested | Workspace | Record | Plugin


class Row(msgspec.Struct, frozen=True):
    """Setting with its report label, the accessor that reaches its store, and the target the run writes."""

    label: str
    path: Access
    target: Target


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ILLUSTRATOR]
def preference(key: str, *, target: bool | float) -> Row:
    """Row of an `app.preferences` key through the typed accessor of the target's type, a real at the 32-bit precision Illustrator holds."""
    match target:
        case bool():
            return Row(key, Preference(key, Kind.BOOLEAN), target)
        case int():
            return Row(key, Preference(key, Kind.INTEGER), target)
        case float():
            return Row(key, Preference(key, Kind.REAL), ctypes.c_float(target).value)


def channels(template: str, names: tuple[str, str, str], target: tuple[int, int, int], *, kind: Kind = Kind.REAL, scale: int = 1) -> Row:
    """Row of a color stored as three preference keys, one per channel name the template spells, the target in bytes."""
    return Row(template.format("*"), Channels(tuple(map(template.format, names)), kind, scale), target)


# --- [PHOTOSHOP]
def settings(owner: str, key: str, value: Value) -> Row:
    """Row of one member of an application descriptor object whose class is its owner."""
    return Row(f"{owner} {key}", Descriptor(owner), Members(owner, {key: value}))


def custom(owner: str, key: str, target: tuple[int, int, int]) -> Row:
    """Row of a guide color key of an application descriptor set to its custom value, with its `<key>`-to-`<key>Custom` color key holding the target."""
    return Row(f"{owner} {key}", Descriptor(owner), Members(owner, {key: Enumerated("guideGridColor", "customEnum"), key.replace("Color", "CustomColor"): RgbColor(*target)}))


# --- [DOM]
def dom(path: str, *, target: Scalar | tuple[int, int, int]) -> Row:
    """Row of an application property by its dotted path from `app`."""
    return Row(path, Property(path), target)


def enumerator(path: str, enumeration: str, member: str) -> Row:
    """Row of an application property by its dotted path from `app` holding a member of the named DOM enumeration."""
    return Row(path, Property(path, enumeration), member)


# --- [INDESIGN]
def preset(name: str, key: str, target: float | str) -> Row:
    """Row of a property of the named document preset."""
    return Row(f"documentPresets {name} {key}", Preset(name, key), target)


# --- [ACROBAT]
def leaf(*path: str, code: int, target: Scalar | tuple[int, int, int] | Fixed | tuple[str, ...]) -> Row:
    """Row of an Acrobat domain leaf by its key path under `DC` and typecode, a color in bytes and a cabinet as its atoms in order."""
    return Row("/".join(path), Leaf(path, code), target)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(units: Units) -> Mapping[Product, tuple[Row, ...]]:
    """Every product's rows with lengths in the system's page units, type in points, and strokes in points under IMPERIAL and millimeters under METRIC."""
    page, increment, snap, ticks = Unit[units.page.name], units.resolution / Length.POINTS, units.snap / Length.POINTS, round(units.snap / units.resolution)
    stroke_unit, type_unit, paper = {Units.IMPERIAL: Unit.POINTS, Units.METRIC: Unit.MILLIMETERS}[units], Unit.POINTS, {Units.IMPERIAL: "Letter", Units.METRIC: "A4"}
    presets = {member: f"{member.name.title()} {paper[member]}" for member in Units}
    lines = Enumerated("guideGridStyle", "lens")
    rgb, edges, axes, theme_alpha, app_scale = ("red", "green", "blue"), ("top", "bottom", "left", "right"), ("horizontal", "vertical"), 0.3, 1.0
    capitalized, sixteen_bit = ("Red", "Green", "Blue"), partial(channels, kind=Kind.INTEGER, scale=65535)
    anchor = min((stop for stop in range(14) if 0x2AA0 >> stop & 1), key=lambda stop: (abs(stop + 2 * int(2 * (app_scale % 1)) - POINT_WIDTH), stop))
    angles = tuple(float(index * ANGLE_STEP) for index in range(6))
    document_presets = DocumentPresets(tuple(Paper(name, member.document, Unit[member.page.name]) for member, name in presets.items()))
    swatches, reserved = tuple(starmap(Swatch, SWATCHES.items())), ("None", "Registration", "Paper", "Black")
    tags, entry, startup = Library(TAGS, swatches), f"Scripts/{LABEL}.jsx", Profile("startupFileType")
    toolbox = Toolbar(
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
    toolbar = Toolbar(
        (
            ("arwT", "ArtT"),
            ("rgmt", "elmt", "srmt", "scmt"),
            ("slbr", "laso", "pgon", "mlas"),
            ("mgla", "AMaF", "qksl", "wand"),
            ("pcsT", "ptha"),
            ("Rect", "Elps", "TrSh", "Poly", "Star", "linT", "CuSh"),
            ("FpoT",),
            ("penT", "mpen", "caTr", "cpen", "aknt", "dknt", "cknt"),
            ("txBx", "txBV", "vtyS", "typS"),
            ("sphB", "caft", "stmm", "ptch", "rcmp", "rdey"),
            ("stam", "stmp"),
            ("pntb", "penc", "crbt", "wetb"),
            ("hstB", "ahbt"),
            ("adjb",),
            ("eras", "sera", "mera"),
            ("bndT", "buck"),
            ("blur", "shar", "smud"),
            ("dodg", "burn", "satu"),
            ("crop", "pcrp", "SlcT", "SlST"),
            ("eyed", "cols", "meaT", "TxtA", "coun"),
            ("hand", "rott"),
            ("zoom",),
        ),
        frozendict({"fgbg": 1, "qkmm": 1, "scmd": 1, "shov": 1, "nana": 0, "ddkb": 0}),
    )
    return frozendict({
        Product.ILLUSTRATOR: (
            preference("uiBrightness", target=0.0),
            preference("uiCanvasIsWhite", target=False),
            preference("UIPreferences/appScaleFactor", target=app_scale),
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
            channels("Guide/Color/{}", rgb, Guide.CONSTRUCTION),
            preference("Guide/Style", target=0),
            channels("Grid/Color/Dark/{}", ("r", "g", "b"), blend(Line.PAPER_GRID, Surface.PAPER, 1 / theme_alpha)),
            channels("Grid/Color/Lite/{}", ("r", "g", "b"), blend(blend(Line.PAPER_GRID, Surface.PAPER, Alpha.GRID_MINOR), Surface.PAPER, 1 / theme_alpha)),
            preference("Grid/Style", target=0),
            preference("Grid/Posn", target=False),
            channels("snapomatic/Color/{}_19_2", rgb, Guide.TRACKING),
            channels("snapomatic/GlyphColor/{}", rgb, Guide.TRACKING),
            *(
                row
                for group in ("angles", "customAngles")
                for row in (preference(f"smartGuides/{group}Count", target=len(angles)), *(preference(f"smartGuides/{group}{index}", target=angle) for index, angle in enumerate(angles)))
            ),
            channels("ArtboardBBColor{}", capitalized, Node.BORDER),
            preference("ArtboardBBWidth", target=1.0),
            sixteen_bit("plugin/AdobeSlicingPlugin/feedback/{}", rgb, Guide.HANDLE),
            preference("Planar/MergeTool/Highlight/StrokeColorIndex", target=0),
            sixteen_bit("Planar/MergeTool/Highlight/StrokeColor/{}", capitalized, Selection.HOVER),
            *(preference(f"Planar/{tool}/Highlight", target=True) for tool in ("FaceSelect", "Paintbucket")),
            *(sixteen_bit(f"Planar/{tool}/Highlight/Color/{{}}", rgb, Selection.HOVER) for tool in ("FaceSelect", "Paintbucket")),
            sixteen_bit("Planar/GapDetection/GapColor/Color/{}", rgb, Guide.TRACKING),
            preference("rulerType", target=page.illustrator),
            Row("rulerType_<n>", Slots("rulerType", startup.slot), page.illustrator),
            preference("strokeUnits", target=stroke_unit.illustrator),
            preference("text/units", target=type_unit.illustrator),
            preference("cursorKeyLength", target=increment),
            *(preference(f"Grid/{axis}/Spacing", target=snap) for axis in ("Horizontal", "Vertical")),
            *(preference(f"Grid/{axis}/Ticks", target=ticks) for axis in ("Horizontal", "Vertical")),
            Row("Tools/Tools Panel Presets", Catalog("Tools/Tools Panel Presets", "Default Toolbar"), toolbox),
            Row("PresetDocumentProfileDataV10.json", Profiles("PresetDocumentProfileDataV10.json", "print_0", "New Document Profiles"), document_presets),
            Row(f"Swatches/{TAGS}.ase", Rendered(f"Swatches/{TAGS}.ase"), tags),
            Row("startup profiles", startup, Template(Ink.DOCUMENT, float(DPI), tags)),
            Row(f"Adobe Illustrator Prefs {LABEL}", ActionSet("Adobe Illustrator Prefs", entry), aliases.ILLUSTRATOR),
            Row("Adobe Illustrator Prefs plugin/AdobeBrush/ThumbnailView", Nested("Adobe Illustrator Prefs", ("plugin", "AdobeBrush", "ThumbnailView")), 0),
            Row("workspace", Active("switchWorkspace"), workspaces.ILLUSTRATOR.name),
            Row("workspace", Workspace((f"Modified Workspaces/{workspaces.ILLUSTRATOR.name}",)), workspaces.ILLUSTRATOR),
        ),
        Product.PHOTOSHOP: (
            settings("interfacePrefs", "kuiBrightnessLevel", Enumerated("uiBrightnessLevelEnumType", "kPanelBrightnessDarkGray")),
            settings("interfacePrefs", "highlightColorOption", Enumerated("highlightColorOptionEnumType", "uiBlueHighlightColor")),
            settings("interfacePrefs", "paletteEnhancedFontTypeKey", Enumerated("paletteFontType", "preferTinyPaletteFontType")),
            settings("interfacePrefs", "paletteUIScaledTypeKey", Boolean(value=False)),
            Row("interfacePrefs canvasBackgroundColors", Canvas("interfacePrefs", "canvasBackgroundColors"), Surface.CANVAS),
            settings("workspacePreferences", "enableLargeTabs", Boolean(value=False)),
            settings("workspacePreferences", "enableNarrowOptionBar", Boolean(value=True)),
            settings("generalPreferences", "autoShowHomeScreen", Boolean(value=False)),
            settings("generalPreferences", "useClassicFileNewDialog", Boolean(value=False)),
            settings(
                "preferences",
                "notificationsPreferences",
                Members(
                    "notificationsPreferences",
                    {"showWhatsNew": Boolean(value=False), "showFeatureOnboarding": Boolean(value=False), "useRichToolTips": Boolean(value=False), "quietMode": Boolean(value=True)},
                ),
            ),
            dom("preferences.askBeforeSavingLayeredTIFF", target=False),
            settings("historyLogPreferences", "contentCredentialsDocumentAsk", Boolean(value=False)),
            enumerator("preferences.fontPreviewSize", "FontPreviewType", "SMALL"),
            Row("layerThumbnailSize", Descriptor("layerThumbnailSize"), Enumerated("size", "small")),
            settings("experimentalFeatures", "DroverUI", Boolean(value=True)),
            settings("toolsPreferences", "zoomWithScrollWheel", Boolean(value=True)),
            settings("toolsPreferences", "enableGestures", Boolean(value=True)),
            settings("toolsPreferences", "animationKey", Boolean(value=False)),
            settings("displayPrefs", "cursorStrokeRope", Boolean(value=True)),
            settings("displayPrefs", "cursorStrokeRopeColor", RgbColor(*Guide.TRACKING)),
            settings("transparencyPrefs", "gamutWarning", RgbColor(*Status.WARNING)),
            settings("typePreferences", "textComposerChoice", Enumerated("textCompMode", "middleEasternInterface")),
            *(custom("guidesPrefs", key, Guide.CONSTRUCTION) for key in ("guidesColor", "activeArtboardGuidesColor", "nonActiveArtboardGuidesColor")),
            custom("guidesPrefs", "smartGuidesColor", Guide.TRACKING),
            custom("guidesPrefs", "gridColor", Line.PAPER_GRID),
            custom("guidesPrefs", "hoverBoundsColor", Selection.HOVER),
            *(settings("guidesPrefs", key, lines) for key in ("guidesStyle", "nonActiveArtboardGuidesStyle", "gridStyle")),
            enumerator("preferences.rulerUnits", "Units", page.photoshop),
            enumerator("preferences.typeUnits", "TypeUnits", type_unit.photoshop),
            enumerator("preferences.pointSize", "PointType", "POSTSCRIPT"),
            settings("guidesPrefs", "gridUnits", Enumerated("rulerUnits", page.ruler)),
            settings("guidesPrefs", "gridMajor", Double(float(math.floor(Fraction(units.snap / units.page) * page.inches * 10**5) * page.per_inch / 10**5))),
            settings("guidesPrefs", "gridMinor", Integer(ticks)),
            settings("unitsPrefs", "newDocPresetPrintResolution", UnitDouble(float(DPI * Unit.POINTS.per_inch))),
            Row("MachinePrefs.psp showAIAssistedButton", Serialized("MachinePrefs.psp", "showAIAssistedButton", "bool"), target=False),
            Row("New Doc Sizes.json", Rendered("New Doc Sizes.json"), document_presets),
            Row(f"Swatches.psp {TAGS}", Rendered("Swatches.psp"), tags),
            Row("Toolbar Customization.psp", Rendered("Toolbar Customization.psp"), toolbar),
            *(
                Row(f"{plugin} {file}", Overlay(file, "PS.json", "PHSP", plugin), Selection.ITEM)
                for plugin, file in (("com.tk.multimask", "FXOverlayColor.ini"), ("com.tk.comboV8", "overlayColor.ini"))
            ),
            Row(f"Actions Palette.psp {LABEL}", ActionSet("Actions Palette.psp", entry), aliases.PHOTOSHOP),
            Row("workspace", Workspace(("Workspace Prefs.psp", f"WorkSpaces (Modified)/{workspaces.PHOTOSHOP.name}.psw")), workspaces.PHOTOSHOP),
        ),
        Product.INDESIGN: (
            dom("generalPreferences.uiBrightnessPreference", target=0.0),
            dom("generalPreferences.pasteboardColorPreference", target=1),
            enumerator("generalPreferences.toolsPanel", "ToolsPanelOptions", "DOUBLE_COLUMN"),
            *(dom(f"generalPreferences.{key}", target=False) for key in ("panelTabHeightPreference", "showStartWorkspace", "showWhatsNewOnStartup", "contextBarVisible", "showStockPurchaseAdornment")),
            *(dom(f"generalPreferences.{key}", target=True) for key in ("useApplicationFrame", "enableMultiTouchGestures")),
            enumerator("generalPreferences.toolTips", "ToolTipOptions", "NORMAL"),
            dom("gpuPerformancePreferences.enableAnimatedZoom", target=False),
            *(dom(f"typeContextualUiPrefs.{key}", target=False) for key in ("showAlternatesUi", "showFractionsUi")),
            dom("pasteboardPreferences.matchPreviewBackgroundToThemeColor", target=False),
            dom("pasteboardPreferences.previewBackgroundColor", target=Surface.CANVAS),
            *(dom(path, target=Guide.CONSTRUCTION) for path in ("guidePreferences.rulerGuidesColor", "documentPreferences.marginGuideColor")),
            dom("documentPreferences.columnGuideColor", target=Line.DATUM_GRID),
            *(dom(f"pasteboardPreferences.{key}", target=Guide.CONSTRUCTION) for key in ("bleedGuideColor", "slugGuideColor")),
            dom("smartGuidePreferences.guideColor", target=Guide.TRACKING),
            *(dom(path, target=Line.PAPER_GRID) for path in ("gridPreferences.gridColor", "gridPreferences.baselineColor", "baselineFrameGridOptions.baselineFrameGridColor")),
            *(dom(f"gridPreferences.{key}", target=False) for key in ("documentGridShown", "baselineGridShown")),
            dom("gridPreferences.gridsInBack", target=True),
            dom("spellPreferences.misspelledWordColor", target=Status.ERROR),
            *(dom(f"spellPreferences.{key}", target=Status.WARNING) for key in ("repeatedWordColor", "uncapitalizedWordColor", "uncapitalizedSentenceColor")),
            *(
                dom(f"xmlPreferences.default{kind}TagColor", target=tag.value)
                for kind, tag in zip(("Story", "Table", "Cell", "Image"), (Tag.COLOR_01, Tag.COLOR_02, Tag.COLOR_03, Tag.COLOR_04), strict=True)
            ),
            dom("galleyPreferences.backgroundColor", target=Surface.WELL),
            dom("galleyPreferences.textColor", target=Text.PRIMARY),
            dom("galleyPreferences.displayFont", target=Typography.INTERFACE.family),
            dom("galleyPreferences.displayFontSize", target=10.0),
            dom("watermarkPreferences.watermarkFontColor", target=Ink.DOCUMENT),
            dom("viewPreferences.pointsPerInch", target=float(round(Length.INCHES / Length.POINTS))),
            *(enumerator(f"viewPreferences.{axis}MeasurementUnits", "MeasurementUnits", page.name) for axis in (*axes, "printDialog")),
            enumerator("viewPreferences.strokeMeasurementUnits", "MeasurementUnits", stroke_unit.name),
            *(enumerator(f"viewPreferences.{kind}MeasurementUnits", "MeasurementUnits", type_unit.name) for kind in ("typographic", "textSize")),
            enumerator("viewPreferences.rulerOrigin", "RulerOrigin", "PAGE_ORIGIN"),
            dom("viewPreferences.cursorKeyIncrement", target=increment),
            *(dom(f"gridPreferences.{axis}GridlineDivision", target=snap) for axis in axes),
            *(dom(f"gridPreferences.{axis}GridSubdivision", target=ticks) for axis in axes),
            dom("documentPreferences.pageSize", target=paper[units]),
            *(dom(f"marginPreferences.{edge}", target=units.margin / Length.POINTS) for edge in edges),
            dom("pageItemDefaults.strokeWeight", target=Pen.THIN / Length.POINTS),
            *(row for member, name in presets.items() for row in (preset(name, "pageSize", paper[member]), *(preset(name, edge, member.margin / Length.POINTS) for edge in edges))),
            Row("documentPresets", Presets(("[Default]",)), tuple(presets.values())),
            *(Row(f"Pages {member}", Panel("Pages", member, "IconSizes"), "EXTRA_SMALL_ICON") for member in ("iconSize", "masterIconSize")),
            Row("colors", Swatches(reserved), tuple(swatch for swatch in swatches if swatch.name not in reserved)),
            *(
                Row(f"UIScalingPrefs {name}", Record("InDesign Defaults", 0x21898, 24, offset, form), value)
                for name, offset, form, value in (("slider", 0x00, "<d", 0.0), ("user set", 0x16, "<H", True))
            ),
            Row(f"UXP {LABEL}", Plugin(LABEL.lower(), "1.0.0", "ID"), aliases.INDESIGN),
            Row("workspace", Active("applyWorkspace"), workspaces.INDESIGN.name),
            Row("workspace", Workspace((f"Workspaces/{workspaces.INDESIGN.name}_CurrentWorkspace.xml",)), workspaces.INDESIGN),
        ),
        Product.ACROBAT: (
            leaf("AVGeneral", "HonorOSTheme", code=0, target=False),
            leaf("AVGeneral", "ActiveUITheme", code=2, target="DarkTheme"),
            leaf("AVGeneral", "AV2ViewerLHPState", code=4, target="hidden"),
            leaf("AVGeneral", "Dockables", "GenTechAcrobatAI", "TabVisible", code=0, target=False),
            *(leaf("AVGeneral", "Dockables", panel, "TabVisible", code=0, target=True) for panel in ("OCGs", "FileAttachmentDockable")),
            leaf("Selection", "EnableContextualToolbar", code=0, target=False),
            leaf("AVGeneral", "ShowPageHoverMenu", code=0, target=False),
            leaf("AVGeneral", "AlwaysUseFileNameAsDocTitle", code=0, target=True),
            leaf("AVGeneral", "ToolHotkeys", code=0, target=True),
            leaf("AVGeneral", "PromptBeforeClosingMultipleTabs", code=0, target=False),
            leaf("HandTool", "MouseWheelZooms", code=0, target=True),
            leaf("Originals", "PageViewLayoutMode", code=1, target=2),
            leaf("Originals", "DefaultZoomType", code=1, target=1),
            leaf("Originals", "PageUnits", code=1, target=page.acrobat),
            *(leaf("Originals", f"Grid{side}", code=1, target=Fixed(snap)) for side in ("Width", "Height")),
            *(leaf("Originals", f"Grid{axis}Offset", code=1, target=Fixed(0.0)) for axis in ("H", "V")),
            leaf("Originals", "GridSubdivisions", code=1, target=ticks),
            *(
                row
                for (*section, key), shade in (
                    (("Originals", "GridColor"), Line.PAPER_GRID),
                    (("Originals", "GridMinorColor"), blend(Line.PAPER_GRID, Surface.PAPER, Alpha.GRID_MINOR)),
                    (("Measuring", "HintColor"), Guide.TRACKING),
                )
                for row in (
                    *(leaf(*section, key, f"{key}{name}", code=1, target=Fixed.channel(channel)) for name, channel in zip(capitalized, shade, strict=True)),
                    leaf(*section, key, f"{key}Space", code=1, target=1),
                )
            ),
            leaf("UnitsAndGuides", "RulersVisible", code=0, target=True),
            leaf("UnitsAndGuides", "GuideColor", "ColorSpace", code=1, target=1),
            *(leaf("UnitsAndGuides", "GuideColor", f"value{index}", code=3, target=Fixed.channel(channel)) for index, channel in enumerate((*Guide.CONSTRUCTION, 0), start=1)),
            *(
                leaf("Measuring", f"Leader{name}", code=1, target=round(length / Length.POINTS))
                for name, length in (("Length", units.first_offset), ("Extend", units.extension), ("Offset", units.offset))
            ),
            leaf("IPM", "DoNotCheckForMessage", code=0, target=True),
            leaf("AVGeneral", "AcrobatRHPBottomBannerIPMEnabled", code=0, target=False),
            leaf("ToolRecommenderSection", "OnDocNextToolRecommendation", code=0, target=False),
            leaf("AVGeneral", "DisableStudioHome", code=0, target=True),
            leaf("HomeWelcome", "LastShowStatus", code=0, target=False),
            leaf("DocumentStatus", "HomeScreenOptionWhenDocClosed", code=0, target=False),
            leaf("ScanOCRDMB", "NumberOfTimesDMBCrossClicked", code=1, target=3),
            *(
                leaf(*path, code=0, target=False)
                for path in (
                    ("FTEDialog", "ShowInstallFTE"),
                    ("AVGeneral", "IsNewUser"),
                    ("AVGeneral", "WhatsNewEnabled"),
                    ("IPM", "ShowMsgAtLaunch"),
                    ("ToolSuggestion", "IsNewUser"),
                    ("QuickToolsFrequent", "FrequentlyUsedToolsVisibility", "Visible"),
                    ("AVPrivate", "AIVideoStripExpUserPref"),
                    *(("HelpAndLearn", key) for key in ("IsNewUserForContextualHelp", "HelpAndLearnV2NewUsers")),
                    *(
                        ("Gentech", key)
                        for key in (
                            "ConsentProvided",
                            "AutoOpenPanel",
                            "EnableNBA",
                            "SummaryDMBEnabled",
                            "SLModelOverviewEnabledPref",
                            "SmartHighlightsEnabledPref",
                            "ShouldShowGTPromotionForCommentsPanel",
                        )
                    ),
                )
            ),
            leaf(
                "AVGeneral",
                "AV2FavoritesCommandsDesktop",
                code=8,
                target=("SelectMenuItem", "LineArrow", "Square", "PolygonCloud", "FreeTextCallout", "Stamp", "RotatePagesCW", "Measure", "DIGSIG:CompareDocuments", "Annots:Tool:RedactMenuItem"),
            ),
            leaf("Intl", "TranslateBannerSuggestedPromptsPref", code=0, target=False),
            *(leaf("Intl", key, code=0, target=True) for key in ("Ligature", "ComplexScript")),
            leaf("Annots", "Prefs", "copyTextToMarkupAnnot", code=0, target=True),
            *(
                leaf("FormsPrefs", key, "Data", code=6, target=shade)
                for key, shade in (
                    ("RequiredFieldHLColor", Status.ERROR),
                    ("RuntimeBGIdleColor", Surface.FORM_FIELD),
                    ("RuntimeBGFocusColor", Surface.PAPER),
                    ("RuntimeBorderIdleColor", Line.FORM_FIELD),
                    ("RuntimeBorderFocusColor", Selection.ACTIVE),
                    ("RuntimeBorderRolloverColor", Selection.HOVER),
                )
            ),
            Row("com.adobe.crashreporter always_never_send", Shared("com.adobe.crashreporter", "always_never_send"), 2),
        ),
    })


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ActionSet",
    "Catalog",
    "DocumentPresets",
    "Filed",
    "Fixed",
    "Leaf",
    "Library",
    "Nested",
    "Overlay",
    "Paper",
    "Plugin",
    "Product",
    "Profiles",
    "Record",
    "Rendered",
    "Row",
    "Serialized",
    "Shared",
    "Swatch",
    "Template",
    "Toolbar",
    "Unit",
    "Workspace",
    "rows",
]
