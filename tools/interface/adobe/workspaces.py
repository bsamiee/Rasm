"""Essentials frame of each Adobe product in the column roles every product shares, and its arrangement of a workspace the product wrote in the product's OWL form."""

import base64
from collections.abc import Mapping
from enum import auto, StrEnum
import io
from itertools import batched, count
import re
from types import MappingProxyType
from typing import Final

import msgspec
from psd_tools.psd.descriptor import Descriptor, Integer, RawData, String

# --- [TYPES] ----------------------------------------------------------------------------


class Form(StrEnum):
    """Product whose OWL dialect a workspace takes: how a panel names itself, holds its size, and closes."""

    ILLUSTRATOR = "illustrator"
    PHOTOSHOP = "photoshop"
    INDESIGN = "indesign"


class Mode(StrEnum):
    """Column display: panels expanded in a column, or collapsed to icons."""

    EXPANDED = "expanded"
    ICON = "icon"


class Role(StrEnum):
    """Tab group's place in the right dock, named for the panels it holds."""

    LIBRARIES = auto()
    TRANSFORM = auto()
    CHARACTER = auto()
    STYLES = auto()
    GLYPHS = auto()
    ASSETS = auto()
    PATTERN = auto()
    BLEND = auto()
    EFFECTS = auto()
    PROPERTIES = auto()
    ALIGNMENT = auto()
    DOCUMENT = auto()
    COLOR = auto()
    AUTOMATION = auto()
    STRUCTURE = auto()
    EXPORT = auto()
    INFORMATION = auto()
    OUTPUT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

NAME: Final = "Essentials"
GROUP: Final = "tab-group"
ENTITIES: Final = MappingProxyType({"hash": "#", "lt": "<", "quot": '"'})
ENTRY: Final = re.compile(r"\t\t/((?:\\.|[^ \r\\])+) (\[ \d+\r(?:\t\t\t[0-9a-f]*\r)*\t\t\]|[^\r]*)\r")
BOOKMARK: Final = "OWLBookMark"
PREFIX: Final = 6

# --- [MODELS] ---------------------------------------------------------------------------


class Column(msgspec.Struct, frozen=True):
    """Right-dock column by its display and the roles of its tab groups top to bottom."""

    mode: Mode
    roles: tuple[Role, ...]


class Frame(msgspec.Struct, frozen=True):
    """Product's workspace: its toolbar, control bar, each role's tab group of panel ids with the shown panel first, the expanded columns' widths, the panels holding their own width, the panels closed in their group, and a None catalog attribute removing its key."""

    name: str
    form: Form
    toolbar: str | int
    bar: bool
    groups: Mapping[Role, tuple[str | int, ...]]
    widths: tuple[int, ...]
    fixed: frozenset[str | int] = frozenset()
    hidden: frozenset[str | int] = frozenset()
    states: Mapping[str | int, int] = MappingProxyType({})
    options: Mapping[str | int, Mapping[str, Mapping[str, str]]] = MappingProxyType({})
    attributes: Mapping[str, int | None] = MappingProxyType({})

    @property
    def placed(self) -> frozenset[str | int]:
        """Every panel id a tab group holds, and the toolbar id."""
        return frozenset((self.toolbar, *(panel for group in self.groups.values() for panel in group)))


class Element(msgspec.Struct, frozen=True):
    """OWL element with its tag, its attributes in written order, and its children."""

    tag: str
    attributes: tuple[tuple[str, str], ...]
    children: tuple["Element", ...] = ()

    def __getitem__(self, key: str) -> str:
        """Attribute's value."""
        return dict(self.attributes)[key]

    def set(self, **values: str | bool) -> Element:
        """Element with the attributes replaced in place and new ones appended, an underscore spelling a hyphen and a boolean spelled lowercase."""
        named = {key.replace("_", "-"): str(value).lower() if isinstance(value, bool) else value for key, value in values.items()}
        return msgspec.structs.replace(self, attributes=(*((key, named.pop(key, value)) for key, value in self.attributes), *named.items()))

    def elements(self) -> tuple[Element, ...]:
        """Element and every descendant in document order."""
        return (self, *(inner for child in self.children for inner in child.elements()))


# --- [TABLES] ---------------------------------------------------------------------------

COLUMNS: Final = (
    Column(Mode.ICON, (Role.LIBRARIES, Role.TRANSFORM, Role.CHARACTER, Role.STYLES, Role.GLYPHS, Role.ASSETS, Role.PATTERN, Role.BLEND, Role.EFFECTS)),
    Column(Mode.EXPANDED, (Role.PROPERTIES, Role.ALIGNMENT, Role.DOCUMENT)),
    Column(Mode.EXPANDED, (Role.COLOR, Role.AUTOMATION, Role.STRUCTURE)),
    Column(Mode.ICON, (Role.EXPORT, Role.INFORMATION, Role.OUTPUT)),
)
ILLUSTRATOR: Final = Frame(
    NAME,
    Form.ILLUSTRATOR,
    "Default Toolbar",
    bar=False,
    groups=MappingProxyType({
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
    widths=(250, 280),
    fixed=frozenset({"Stroke", "Transparency", "Align", "Pathfinder", "AdobeGradientEditor"}),
    hidden=frozenset({"Stroke", "Transparency", "AdobeBlendOptionsPanel"}),
    states=MappingProxyType({"Character": 2, "Paragraph": 1, "Transform": 1, "Align": 1, "Color": 1}),
    attributes=MappingProxyType({
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
        **{f"AdobeLinkPalette {key}": None for key in ("List Entry Height", "Show Icons (boolean)", "Smart Linking (boolean)", "Show Transparency Interaction")},
        "ShowCommentsPanelBool": 0,
        "ShowDLPanelBool": 0,
        "AgentsUIPanelClosedByUser": 1,
    }),
)
PHOTOSHOP: Final = Frame(
    NAME,
    Form.PHOTOSHOP,
    "panelid.static.toolbar",
    bar=True,
    groups=MappingProxyType({
        role: tuple(f"panelid.{panel}" for panel in group)
        for role, group in (
            (Role.LIBRARIES, ("dynamic.uxp/com.adobe.cclibrariespanel/ccLibrariesPanel",)),
            (Role.CHARACTER, ("static.textcharacter", "static.textparagraph")),
            (Role.STYLES, ("static.textparastyle", "static.textcharstyle")),
            (Role.GLYPHS, ("static.textglyphspanel",)),
            (Role.ASSETS, ("static.brushpresets", "static.styles", "static.customshapes", "static.brushstyler")),
            (Role.PROPERTIES, ("static.properties", "static.create")),
            (Role.COLOR, ("static.swatches", "static.picker", "static.gradients", "static.patterns")),
            (
                Role.AUTOMATION,
                (
                    "static.actions",
                    "static.history",
                    "dynamic.uxp/com.tk.multimask/tkmultimaskv9",
                    "dynamic.uxp/com.tk.comboV8/tkcombocxv9",
                    "dynamic.uxp/com.tk.myactionsV8/tkmyactions",
                    "dynamic.uxp/com.adobe.pluginspanel/pluginsPanel",
                    "dynamic.uxp/com.adobe.ccx.comments-webview/ccx-comments-uxp-webview",
                ),
            ),
            (Role.STRUCTURE, ("static.layers", "static.navigator", "static.channels", "static.paths")),
            (Role.EXPORT, ("dynamic.uxp/com.tk.export/tkexportv9",)),
            (Role.INFORMATION, ("static.info",)),
        )
    }),
    widths=(345, 345),
    hidden=frozenset({"panelid.dynamic.uxp/com.adobe.pluginspanel/pluginsPanel", "panelid.dynamic.uxp/com.adobe.ccx.comments-webview/ccx-comments-uxp-webview"}),
)
INDESIGN: Final = Frame(
    NAME,
    Form.INDESIGN,
    4353,
    bar=False,
    groups=MappingProxyType({
        Role.LIBRARIES: ("com.adobe.DesignLibraries.angular",),
        Role.TRANSFORM: (30254,),
        Role.CHARACTER: (26901, 27162),
        Role.STYLES: (8466, 8465),
        Role.GLYPHS: (53519,),
        Role.ASSETS: (113153,),
        Role.PROPERTIES: (71040,),
        Role.ALIGNMENT: (25871, 99625),
        Role.DOCUMENT: (132609,),
        Role.COLOR: (16385, 29241, 24065),
        Role.AUTOMATION: (52229, 149248),
        Role.STRUCTURE: (6199, 18464),
        Role.INFORMATION: (89089,),
        Role.OUTPUT: (71937, 11031),
    }),
    widths=(252, 250),
    fixed=frozenset({25871, 99625, 29241, 24065}),
    options=MappingProxyType({18464: MappingProxyType({"LayerPaneOptions": MappingProxyType({"SmallRows": "true"})})}),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [OWL]
def parsed(text: str) -> Element:
    """Element tree of a `<workspace>` block, attribute values kept as written."""
    stack: list[tuple[str, tuple[tuple[str, str], ...], list[Element]]] = [("", (), [])]
    for closing, tag, attributes, empty in re.findall(r'<(/?)([\w-]+)((?:\s+[\w-]+="[^"]*")*)(/?)>', text):
        match closing, empty, tuple(re.findall(r'([\w-]+)="([^"]*)"', attributes)):
            case "/", _, _:
                name, pairs, children = stack.pop()
                stack[-1][2].append(Element(name, pairs, tuple(children)))
            case _, "/", written:
                stack[-1][2].append(Element(tag, written))
            case _, _, written:
                stack.append((tag, written, []))
    return stack[0][2][0]


def serialized(element: Element, depth: int = 0) -> str:
    """Element as the products write it: tab-indented, one element per line, empty elements self-closed."""
    head = "\t" * depth + "<" + element.tag + "".join(f' {key}="{value}"' for key, value in element.attributes)
    body = "\n".join(serialized(child, depth + 1) for child in element.children)
    return f"{head}/>" if not element.children else f"{head}>\n{body}\n" + "\t" * depth + f"</{element.tag}>"


# --- [PANELS]
def decoded(data: str) -> str:
    """InDesign panel data with its markup entities decoded."""
    return re.sub(rf"#({'|'.join(ENTITIES)});", lambda found: ENTITIES[found[1]], data)


def encoded(text: str) -> str:
    """InDesign panel data with its markup entities encoded."""
    names = {plain: name for name, plain in ENTITIES.items()}
    return re.sub(f"[{''.join(names)}]", lambda found: f"#{names[found[0]]};", text)


def descriptor(data: str) -> tuple[bytes, Descriptor]:
    """Photoshop panel data: its version prefix and its action descriptor."""
    raw = base64.b64decode(data)
    return raw[:4], Descriptor.read(io.BytesIO(raw[4:]))


def identity(form: Form, element: Element) -> str | int:
    """Panel id the element carries: Illustrator's data string, Photoshop's content view id, InDesign's extension id, else its integer widget id."""
    match form:
        case Form.ILLUSTRATOR:
            return element["app-data"]
        case Form.PHOTOSHOP:
            _, held = descriptor(element["app-data"])
            return str(held[b"owlContentViewStringiID"].value).rstrip("\0")
        case Form.INDESIGN:
            match dict(parsed(decoded(element["app-data"])).attributes):
                case {"UXPExtensionID": extension} | {"CSXSExtensionID": extension}:
                    return extension
                case named:
                    return int(named["id"])


def sized(form: Form, element: Element, width: int) -> Element:
    """Panel with every preferred width set to the width."""
    match form:
        case Form.PHOTOSHOP:
            version, held = descriptor(element["app-data"])
            for rect in held[b"owlContentViewAppData"][b"owlPalettePreferredSizeArray"]:
                rect.update(dict.fromkeys((b"Left", b"Rght"), Integer(width)))
            buffer = io.BytesIO()
            held.write(buffer)
            return element.set(app_data=base64.b64encode(version + buffer.getvalue()).decode())
        case _:
            return element.set(**{key.replace("-", "_"): f"{width} {element[key].split()[1]}" for key in ("preferred-unconstrained-size", "preferred-constrained-size")})


def optioned(element: Element, options: Mapping[str, Mapping[str, str]]) -> Element:
    """InDesign panel with each option element's attributes set."""
    text = decoded(element["app-data"])
    for tag, values in options.items():
        for key, value in values.items():
            text = re.sub(rf'(<{tag}\b[^>]*?\b{key}=")[^"]*(")', rf"\g<1>{value}\g<2>", text)
    return element.set(app_data=encoded(text))


# --- [ARRANGEMENT]
def panels(form: Form, base: Element) -> Mapping[str | int, Element]:
    """Every palette and toolbar the workspace holds by panel id."""
    return {identity(form, element): element for element in base.elements() if element.tag in {"palette", "toolbar"}}


def shown(form: Form, frame: Frame, element: Element, width: int | None) -> Element:
    """Placed panel open or closed in its group as the frame declares, at its column's width unless the product holds its own, in its declared state, with its declared options."""
    panel = identity(form, element)
    opened = element.set(is_closed=panel in frame.hidden, current_state=str(frame.states.get(panel, element["current-state"])), is_minimized=False)
    resized = opened if width is None or panel in frame.fixed else sized(form, opened, width)
    return optioned(resized, frame.options[panel]) if panel in frame.options else resized


def pane(form: Form, frame: Frame, column: Column, width: int | None, held: Mapping[str | int, Element]) -> Element:
    """Column of the frame's tab groups for the column's roles in role order, its panels at the width."""
    groups = tuple(
        Element(GROUP, (), tuple(shown(form, frame, held[panel], width) for panel in frame.groups[role])).set(active_palette="", is_closed=False) for role in column.roles if role in frame.groups
    )
    return Element("tab-pane", (("mode", column.mode), ("preferred-iconic-length", "39" if column.mode is Mode.ICON else "0"), ("layout-mode", "auto-flow")), groups)


def closed(form: Form, base: Element, placed: frozenset[str | int]) -> tuple[Element, ...]:
    """Every held tab group without its placed panels in the dialect's closed form: its own closed floating dock in Illustrator and InDesign, a closed group of one pane in Photoshop, each panel closed except in InDesign, whose panels close with their dock."""
    kept = tuple(
        (held, msgspec.structs.replace(held, children=remaining).set(is_closed=form is Form.PHOTOSHOP))
        for held in base.elements()
        if held.tag == GROUP
        for remaining in [tuple(panel.set(is_closed=form is not Form.INDESIGN) for panel in held.children if identity(form, panel) not in placed)]
        if remaining
    )
    match form:
        case Form.PHOTOSHOP:
            return (Element("tab-pane", (("mode", Mode.EXPANDED), ("preferred-iconic-length", "0"), ("layout-mode", "auto-flow")), tuple(group for _, group in kept)),)
        case _:
            floating = {group: dock for dock in base.children if dock["anchor"] == "none" for group in dock.elements() if group.tag == GROUP}
            template = next(iter(floating.values())).set(origin="0 0")
            manual = (("mode", Mode.EXPANDED), ("preferred-iconic-length", "0"), ("layout-mode", "manual"))
            return tuple(msgspec.structs.replace(floating.get(held, template), children=(Element("tab-pane", manual, (group,)),)).set(is_closed=True) for held, group in kept)


def numbered(root: Element) -> Element:
    """Workspace with its elements numbered in document order from 1 and each tab group showing its first panel."""
    counter = count(1)

    def walk(element: Element) -> Element:
        own = element.set(id=str(next(counter))) if "id" in dict(element.attributes) else element
        children = tuple(walk(child) for child in element.children)
        return msgspec.structs.replace(own.set(active_palette=children[0]["id"]) if own.tag == GROUP else own, children=children)

    return walk(root)


def arranged(form: Form, frame: Frame, base: Element) -> Element:
    """Held workspace in the frame: the control bar shown or closed, the bottom dock empty, every toolbar in the left dock with the frame's open at two columns, each column holding a frame role in the right dock with an expanded one at the frame's width for it, and every other panel closed."""
    held, placed = panels(form, base), frame.placed
    widths = dict(zip((column for column in COLUMNS if column.mode is Mode.EXPANDED), frame.widths, strict=True))
    docks = {dock["anchor"]: dock for dock in base.children if dock["anchor"] != "none"}
    bar = tuple(child.set(is_closed=not frame.bar) for child in docks["top"].children)
    toolbars = tuple(
        element.set(is_closed=False, size_variant="vertical-wide") if identity(form, element) == frame.toolbar else element.set(is_closed=True)
        for element in base.elements()
        if element.tag == "toolbar"
    )
    shaped = (pane(form, frame, column, widths.get(column), held) for column in COLUMNS if any(role in frame.groups for role in column.roles))
    right = (*shaped, *(closed(form, base, placed) if form is Form.PHOTOSHOP else ()))
    return numbered(
        msgspec.structs.replace(
            base,
            children=(
                msgspec.structs.replace(docks["top"], children=bar),
                msgspec.structs.replace(docks["bottom"], children=()),
                msgspec.structs.replace(docks["left"], children=toolbars),
                msgspec.structs.replace(docks["right"], children=right),
                *(() if form is Form.PHOTOSHOP else closed(form, base, placed)),
            ),
        )
    )


# --- [FILES]
def prefs(held: bytes) -> Descriptor:
    """Action descriptor Photoshop's workspace preferences hold after their header."""
    return Descriptor.read(io.BytesIO(held[PREFIX:]))


def hex_string(text: str, depth: int) -> str:
    """Store text value of an entry at the depth: the UTF-8 length, the hex digits 64 to a line one tab deeper, a blank line for no bytes, and the closing bracket at the depth, each line ended by CR."""
    data, indent = text.encode(), "\t" * depth
    rows = [f"{indent}\t{''.join(chunk)}" for chunk in batched(data.hex(), 64, strict=False)] or [""]
    return "".join((f"[ {len(data)}\r", *(f"{row}\r" for row in rows), f"{indent}]"))


def split(text: str) -> tuple[str, str, str]:
    """InDesign workspace file's text before its `<workspace>` block, the block, and the text after it."""
    head, opening, rest = text.partition('<workspace version="1">')
    body, closing, tail = rest.partition("</workspace>")
    return head, f"{opening}{body}{closing}", tail


def block(frame: Frame, held: bytes) -> str:
    """`<workspace>` block of the workspace file the product wrote."""
    match frame.form:
        case Form.ILLUSTRATOR:
            return bytes.fromhex("".join(dict(ENTRY.findall(held.decode("latin-1")))[BOOKMARK].split("\r")[1:-1]).replace("\t", "")).decode()
        case Form.PHOTOSHOP:
            return bytes(prefs(held)[b"OPM "][b"owlWorkspaceBookmark"].value).decode()
        case Form.INDESIGN:
            _, owl, _ = split(held.decode())
            return owl


def missing(frame: Frame, held: bytes) -> frozenset[str | int]:
    """Panel ids the frame places that the written workspace does not hold."""
    return frame.placed - panels(frame.form, parsed(block(frame, held))).keys()


def rendered(frame: Frame, held: bytes) -> tuple[bytes, ...]:
    """Frame's files from the workspace file the product wrote: that file arranged in the frame, with Illustrator's catalog attributes and Photoshop's active name, then each saved copy."""
    owl = serialized(arranged(frame.form, frame, parsed(block(frame, held))))
    match frame.form:
        case Form.ILLUSTRATOR:
            _, head, body, tail, _ = re.split(r"(.*?\t/attributes \{\r)(.*?)(\t\}\r\t/canEdit .*)", held.decode("latin-1"), flags=re.DOTALL)
            declared = {key.replace(" ", "\\ "): value for key, value in frame.attributes.items()}
            merged = {**dict(ENTRY.findall(body)), BOOKMARK: hex_string(f"{owl}\n", 2), **{key: str(value) for key, value in declared.items() if value is not None}}
            return (f"{head}{''.join(f'\t\t/{key} {value}\r' for key, value in merged.items() if key not in declared or declared[key] is not None)}{tail}".encode("latin-1"),)
        case Form.PHOTOSHOP:
            live = prefs(held)
            live.update({b"Nm  ": String(f"{frame.name}\0")})
            live[b"OPM "].update({b"owlWorkspaceBookmark": RawData(f"{owl}\n".encode())})
            buffer = io.BytesIO()
            live.write(buffer)
            saved = (
                f'<?xml version="1.0" encoding="UTF-8"?>\n<photoshop-workspace version="2.0">\n\t<photoshop-panel-configuration>\n{owl}\n\t</photoshop-panel-configuration>\n</photoshop-workspace>\n'
            )
            return (held[:PREFIX] + buffer.getvalue(), saved.encode("utf-8-sig"))
        case Form.INDESIGN:
            head, _, tail = split(held.decode())
            return (f"{head}{owl}{re.sub(r'(<menu-set [^>]*\bname=")[^"]*(")', r'\g<1>InDesign Defaults\g<2>', tail)}".encode(),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COLUMNS", "ILLUSTRATOR", "INDESIGN", "PHOTOSHOP", "Column", "Form", "Frame", "Role", "block", "hex_string", "missing", "parsed", "rendered", "serialized"]
