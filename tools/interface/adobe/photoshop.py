# ruff: file-ignore[private-member-access]
"""Photoshop's rows, toolbar, and Essentials frame, with the settings, swatch, toolbar, and workspace files they render."""

import base64
import codecs
from collections.abc import Iterable, Mapping, Sequence
from fractions import Fraction
from functools import partial
import io
import math
from pathlib import Path
import struct
from typing import Final
import uuid

from lxml import etree
import msgspec
from psd_tools.psd import descriptor
from psd_tools.psd.base import BaseElement
from psd_tools.psd.bin_utils import read_fmt
from psd_tools.psd.descriptor import Bool, Descriptor, DescriptorBlock, Integer, List, RawData, String
from psd_tools.psd.tagged_blocks import TaggedBlock

from interface import host
from interface.adobe import window
from interface.adobe.rows import member, Paper, papers, Row, Toolbar
from interface.adobe.session import Scripted
from interface.adobe.stores import File, Folder, Plugin
from interface.adobe.window import Role
from interface.render import DPI
from interface.roles import Guide, Line, Selection, Status, Surface, SWATCHES, TAGS, TEXT_POINTS
from interface.units import Length, Units

# --- [CONSTANTS] ------------------------------------------------------------------------

HEADER_8BPF: Final = b"8BPF" + (1).to_bytes(2)
WORKSPACE_HEADER: Final = (2).to_bytes(2)
MACHINE_PREFS: Final = "MachinePrefs.psp"
SWATCH_LIST: Final = "Swatches.psp"
WORKSPACE_PREFS: Final = "Workspace Prefs.psp"
ESSENTIALS: Final = f"WorkSpaces (Modified)/{window.WORKSPACE}.psw"
BLOCK: Final = "photoshop-panel-configuration/workspace"
TOOLBAR: Final = Toolbar(
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

# --- [MODELS] ---------------------------------------------------------------------------


class ApplicationProperty(msgspec.Struct, frozen=True, tag=True):
    """Application descriptor property by its owner key, read through `executeActionGet` and set through `setd`."""

    owner: str


class PaletteFont(msgspec.Struct, frozen=True, tag=True):
    """UI font size option, read back as the `fontSmallSize` the held option draws native panels at and written as the `interfacePrefs` option by name."""

    option: str


class PageUnit(msgspec.Struct, frozen=True):
    """Page unit as Photoshop spells it: the ruler's `Units` member, the grid's units enumerator, the New Document preset unit, and the inches per unit a grid length converts through."""

    ruler_units: str
    grid_units: str
    preset: str
    inches: Fraction


UNITS: Final = frozendict({Length.INCHES: PageUnit("INCHES", "rulerInches", "inchesUnit", Fraction(1)), Length.MILLIMETERS: PageUnit("MM", "rulerMm", "millimetersUnit", Fraction("0.03937"))})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [DESCRIPTORS]
def described(class_id: bytes, items: Mapping[bytes, BaseElement]) -> Descriptor:
    """Action descriptor of the class holding the items in order under the one-NUL name Photoshop writes."""
    made = Descriptor(name="\0", classID=class_id)
    made.update(items)
    return made


def listed(items: Iterable[BaseElement]) -> List:
    """Action list of the items in order."""
    made = List()
    made.extend(items)
    return made


# --- [FILES]
def machine_prefs(held: bytes | None) -> bytes | host.Error:
    """Machine preferences with the AI Assisted button hidden, or the error of the absent file."""
    if held is None:
        return host.Error(f"{MACHINE_PREFS} is absent")
    record = DescriptorBlock.frombytes(held[len(HEADER_8BPF) :])
    record[b"showAIAssistedButton"] = Bool(value=False)
    return HEADER_8BPF + record.tobytes(padding=1)


def customization() -> bytes:
    """Toolbar record: its version and pad, the options as booleans, the slots as lists of `toolKey` objects, and empty Extra Tools."""
    record = DescriptorBlock(name="\0", classID=b"null")
    record.update({
        b"tver": Integer(1),
        b"tpad": Integer(0),
        **{option.encode(): Bool(value) for option, value in TOOLBAR.options.items()},
        b"tlst": listed(listed(described(b"null", {b"toolKey": Integer(int.from_bytes(tool.encode()))}) for tool in slot) for slot in TOOLBAR.slots),
        b"oflt": List(),
    })
    return HEADER_8BPF + record.tobytes(padding=1)


def swatch_list(held: bytes | None) -> bytes | host.Error:
    """Swatch list with the tag group of the role swatches in place of the held group of that name, else after the held groups, each channel its byte scaled to 16 bits."""
    if held is None:
        return host.Error(f"{SWATCH_LIST} is absent")
    stream, title = io.BytesIO(held), f"{TAGS}\0"
    stream.seek(10 * read_fmt("2H", stream)[1], io.SEEK_CUR)
    colors = iter([(stream.read(10), String.read(stream)) for _ in range(read_fmt("2H", stream)[1])])
    read_fmt("8sI", stream)
    hierarchy = DescriptorBlock.read(stream)
    paired = [(entry, next(colors) if entry.classID == b"preset" else None) for entry in hierarchy[b"hierarchy"]]
    opened = next((index for index, (entry, _) in enumerate(paired) if entry.classID == b"Grup" and entry[b"Nm  "] == title), len(paired))
    closed = next((index + 1 for index, (entry, _) in enumerate(paired[opened:], opened) if entry.classID == b"groupEnd"), len(paired))
    paired[opened:closed] = [
        (described(b"Grup", {b"Nm  ": String(title), b"zuid": String(f"{uuid.uuid5(uuid.NAMESPACE_URL, TAGS)}\0")}), None),
        *((described(b"preset", {}), (struct.pack(">5H", 0, *(byte * 0x101 for byte in color), 0), String(f"{swatch}\0"))) for swatch, color in SWATCHES.items()),
        (described(b"groupEnd", {}), None),
    ]
    hierarchy[b"hierarchy"][:] = [entry for entry, _ in paired]
    presets = [color for _, color in paired if color is not None]
    versions = ((1, [color for color, _ in presets]), (2, [color + name.tobytes() for color, name in presets]))
    return b"".join((*(struct.pack(">2H", version, len(items)) + b"".join(items) for version, items in versions), TaggedBlock(key=b"phry", data=hierarchy.tobytes(padding=1)).tobytes()))


def sizes(presets: Sequence[Paper]) -> bytes:
    """New Document presets file of the user section alone, one preset per paper in its unit at the render resolution, indented as Photoshop writes it."""
    section = "user"
    user = [
        {
            "name": paper.title,
            "identifier": "",
            "group": section,
            "width": width / paper.value.page,
            "height": height / paper.value.page,
            "units": UNITS[paper.value.page].preset,
            "profile": "default",
            "resolution": float(DPI),
            "resolutionUnits": UNITS[Length.INCHES].preset,
            "depth": 8,
            "scale": 1.0,
            "mode": "RGB",
            "fill": "white",
            "guides": [],
            "artboards": [],
        }
        for paper in presets
        for width, height in (paper.value.document,)
    ]
    return msgspec.json.format(msgspec.json.encode({"sections": [{"section": section, "presets": user}]}), indent=4)


# --- [WORKSPACE]
def app_data(element: etree._Element) -> DescriptorBlock:
    """Action descriptor a palette or toolbar holds in its `app-data`."""
    return DescriptorBlock.frombytes(base64.b64decode(element.attrib[window.Attribute.DATA]))


def panel(element: etree._Element) -> str:
    """Panel id a palette or toolbar holds as its content view id."""
    return str(app_data(element)[b"owlContentViewStringiID"].value).rstrip("\0")


def sized(element: etree._Element, width: int) -> None:
    """Set every preferred size of the panel to the width."""
    data = app_data(element)
    for size in data[b"owlContentViewAppData"][b"owlPalettePreferredSizeArray"]:
        size.update(dict.fromkeys((b"Left", b"Rght"), Integer(width)))
    element.set(window.Attribute.DATA, base64.b64encode(data.tobytes(padding=1)).decode())


def workspace_prefs(factory: Sequence[etree._Element], held: bytes | None) -> bytes | tuple[host.Skip, ...]:
    """Workspace preferences with the live block arranged in the frame and Essentials active, or the skip of a file Photoshop has yet to write."""
    if held is None:
        return (host.Skip(WORKSPACE_PREFS),)
    record = DescriptorBlock.frombytes(held[len(WORKSPACE_HEADER) :])
    bookmark = record[b"OPM "]
    match window.arranged(FRAME, window.parsed(bookmark[b"owlWorkspaceBookmark"].value), factory):
        case tuple() as skipped:
            return skipped
        case root:
            bookmark[b"owlWorkspaceBookmark"] = RawData(f"{window.serialized(root)}\n".encode())
            record[b"Nm  "] = String(f"{window.WORKSPACE}\0")
            return WORKSPACE_HEADER + record.tobytes(padding=1)


def essentials(factory: Sequence[etree._Element], held: bytes | None) -> bytes | tuple[host.Skip, ...] | host.Error:
    """Modified Essentials workspace with its block arranged in the frame inside the file's own framing, the skip of a file Photoshop has yet to write, or the error of a file without the block."""
    if held is None:
        return (host.Skip(ESSENTIALS),)
    document = etree.fromstring(held)
    match document.find(BLOCK):
        case None:
            return host.Error(f"{ESSENTIALS} holds no {BLOCK}")
        case block:
            match window.arranged(FRAME, window.parsed(etree.tostring(block, with_tail=False)), factory):
                case tuple() as skipped:
                    return skipped
                case root:
                    block[:] = list(root)
                    window.indented(block)
                    info = document.getroottree().docinfo
                    return codecs.BOM_UTF8 + f'<?xml version="{info.xml_version}" encoding="{info.encoding}"?>\n{etree.tostring(document, encoding="unicode")}\n'.encode()


def workspace(factory: Sequence[bytes]) -> tuple[File, ...]:
    """Workspace preferences and the modified Essentials workspace, each arranged from its own held block, taking panels it lacks from the factory workspaces in name order."""
    stock = tuple(block for data in factory for block in window.parsed(data).iterfind(BLOCK))
    return (File(Folder.SETTINGS, WORKSPACE_PREFS, partial(workspace_prefs, stock)), File(Folder.SETTINGS, ESSENTIALS, partial(essentials, stock)))


# --- [ROWS]
def setting(owner: str, target: object) -> Row:
    """Row of the application descriptor property the owner names, an object's keys set in one descriptor."""
    return Row(owner, ApplicationProperty(owner), target)


def folders(bundle: host.Bundle, base: Mapping[Folder, Path]) -> Mapping[Folder, Path]:
    """Photoshop's folders beside the session's: the bundle's factory workspaces and the UXP plug-in data storage of the bundle's channel and major version."""
    storage = "PHSP" if bundle.channel is None else f"PHSP{bundle.channel.upper()}"
    return frozendict({
        Folder.FACTORY: bundle.path.joinpath("Contents", "Required", "Workspaces"),
        Folder.PLUGIN_DATA: base[Folder.UXP].joinpath("PluginsStorage", storage, bundle.version.partition(".")[0], "External"),
    })


def rows(units: Units, _: host.Bundle) -> tuple[Row | File, ...]:
    """Photoshop's rows with native panel text at the interface text size, lengths in the system's page unit, type in points, resolutions as the 16.16 fixed pixels per inch Photoshop holds, and smart guides at the magenta step its custom color store holds."""
    page, points = UNITS[units.page], round(Length.INCHES / Length.POINTS)
    fonts = frozendict({9: "preferTinyPaletteFontType", 10: "preferSmallPaletteFontType", 11: "preferMediumPaletteFontType", 12: "preferLargePaletteFontType"})
    grid = float(Fraction(math.floor(Fraction(units.snap / units.page) * page.inches * 10**5), 10**5) * Fraction(Length.INCHES / units.page).limit_denominator())
    resolution = float(Fraction(math.floor(Fraction(DPI, points) * 2**16), 2**16) * points**2)
    screens = ("screenModeStandard", "screenModeFullScreenWithMenubar", "screenModeArtboard", "screenModeFullScreen")
    colors = (
        ("guides", Guide.CONSTRUCTION),
        ("activeArtboardGuides", Guide.CONSTRUCTION),
        ("nonActiveArtboardGuides", Guide.CONSTRUCTION),
        ("smartGuides", Guide.TRACKING_STORED),
        ("grid", Line.PAPER_GRID),
        ("hoverBounds", Selection.HOVER),
    )
    return (
        setting(
            "interfacePrefs",
            {
                "kuiBrightnessLevel": "kPanelBrightnessDarkGray",
                "highlightColorOption": "uiBlueHighlightColor",
                "paletteUIScaledTypeKey": False,
                "canvasBackgroundColors": [{"screenMode": screen, "color": Surface.CANVAS, "canvasColorMode": "custom", "canvasFrame": "none"} for screen in screens],
            },
        ),
        Row("fontSmallSize", PaletteFont(fonts[TEXT_POINTS]), TEXT_POINTS),
        setting("workspacePreferences", {"enableLargeTabs": False, "enableNarrowOptionBar": True}),
        setting("generalPreferences", {"autoShowHomeScreen": False, "useClassicFileNewDialog": False}),
        setting("notificationsPreferences", {"showWhatsNew": False, "showFeatureOnboarding": False, "useRichToolTips": False, "quietMode": True}),
        member("preferences", "askBeforeSavingLayeredTIFF", target=False),
        setting("historyLogPreferences", {"contentCredentialsDocumentAsk": False}),
        member("preferences", "fontPreviewSize", "SMALL", enumeration="FontPreviewType"),
        setting("layerThumbnailSize", "small"),
        setting("experimentalFeatures", {"DroverUI": True}),
        setting("toolsPreferences", {"zoomWithScrollWheel": True, "enableGestures": True, "animationKey": False}),
        setting("displayPrefs", {"cursorStrokeRope": True, "cursorStrokeRopeColor": Guide.TRACKING}),
        setting("transparencyPrefs", {"gamutWarning": Status.WARNING}),
        setting("typePreferences", {"textComposerChoice": "middleEasternInterface"}),
        member("preferences", "rulerUnits", page.ruler_units, enumeration="Units"),
        member("preferences", "typeUnits", "POINTS", enumeration="TypeUnits"),
        member("preferences", "pointSize", "POSTSCRIPT", enumeration="PointType"),
        setting(
            "guidesPrefs",
            {
                **dict.fromkeys((f"{stem}Color" for stem, _ in colors), "customEnum"),
                **{f"{stem}CustomColor": color for stem, color in colors},
                **dict.fromkeys(("guidesStyle", "nonActiveArtboardGuidesStyle", "gridStyle"), "lens"),
                "gridUnits": page.grid_units,
                "gridMajor": grid,
                "gridMinor": round(units.snap / units.resolution),
            },
        ),
        setting("unitsPrefs", {"newDocPresetPrintResolution": resolution}),
        File(Folder.SETTINGS, MACHINE_PREFS, machine_prefs),
        File(Folder.SETTINGS, "New Doc Sizes.json", lambda _: sizes(papers(units))),
        File(Folder.SETTINGS, SWATCH_LIST, swatch_list),
        File(Folder.SETTINGS, "Toolbar Customization.psp", lambda _: customization()),
        *(
            File(Folder.PLUGIN_DATA, f"{plugin}/PluginData/{file}", lambda _: f"#{bytes(Selection.ITEM).hex()}".encode(), Plugin("PS", plugin))
            for plugin, file in (("com.tk.multimask", "FXOverlayColor.ini"), ("com.tk.comboV8", "overlayColor.ini"))
        ),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

descriptor._TERMS.update({b"tver", b"tpad", *(option.encode() for option in TOOLBAR.options), b"tlst", b"oflt"})
FRAME: Final = window.Frame(
    toolbar="panelid.static.toolbar",
    bar=True,
    groups=frozendict({
        role: tuple(f"panelid.{name}" for name in group)
        for role, group in (
            (Role.LIBRARIES, ("dynamic.uxp/com.adobe.cclibrariespanel/ccLibrariesPanel",)),
            (Role.CHARACTER, ("static.textcharacter", "static.textparagraph")),
            (Role.STYLES, ("static.textparastyle", "static.textcharstyle")),
            (Role.GLYPHS, ("static.textglyphspanel",)),
            (Role.ASSETS, ("static.brushpresets", "static.styles", "static.customshapes", "static.brushstyler")),
            (Role.PROPERTIES, ("static.properties", "static.create")),
            (Role.COLOR, ("static.swatches", "static.picker", "static.gradients", "static.patterns")),
            (Role.AUTOMATION, ("static.actions", "static.history", "dynamic.uxp/com.adobe.pluginspanel/pluginsPanel", "dynamic.uxp/com.adobe.ccx.comments-webview/ccx-comments-uxp-webview")),
            (Role.STRUCTURE, ("static.layers", "static.navigator", "static.channels", "static.paths")),
            (
                Role.EXPORT,
                ("dynamic.uxp/com.tk.export/tkexportv9", "dynamic.uxp/com.tk.multimask/tkmultimaskv9", "dynamic.uxp/com.tk.comboV8/tkcombocxv9", "dynamic.uxp/com.tk.myactionsV8/tkmyactions"),
            ),
            (Role.INFORMATION, ("static.info",)),
        )
    }),
    hidden=frozenset({"panelid.dynamic.uxp/com.adobe.pluginspanel/pluginsPanel", "panelid.dynamic.uxp/com.adobe.ccx.comments-webview/ccx-comments-uxp-webview"}),
    identity=panel,
    sized=sized,
    closed=window.docked,
)
PRODUCT: Final = Scripted(name="photoshop", identifiers=("com.adobe.Photoshop",), command="«event miscDjxM»", arguments="given «class JArg»:", rows=rows, folders=folders, workspace=workspace)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FRAME", "PRODUCT", "folders", "rows", "workspace"]
