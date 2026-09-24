# ty: ignore[unresolved-attribute, unresolved-import]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, misc, explicit-any, no-any-return"
# ruff: file-ignore[import-outside-top-level]
"""Grasshopper 2's settings as rows the Rhino script converges: its `Settings` fields, canvas skin, snap feedback, preview guises, editor rectangle, ribbon layout, fonts, display rule and sketch defaults, the scripted start's banner, and component tab rules."""

from collections.abc import Callable, Mapping
from functools import partial, reduce
from importlib import import_module
import math
from pathlib import Path
from types import ModuleType, SimpleNamespace

from AppKit import NSWindow, NSWindowStyle
import clr
from CoreGraphics import CGRect
from Eto.Drawing import Color, Point, Size
from Eto.Forms import Screen
from Eto.Mac import Mac64Extensions
from layout import NODE_EDITOR
from radix import Rgb
from Rhino import RhinoDoc
from Rhino.PlugIns import PlugIn
import System
from System.Drawing import Rectangle
from theme import Axis, GRID_MINOR_ALPHA, Guide, Line, Selection, SELECTION_FILL_ALPHA, Surface, Typography

# --- [TYPES] ----------------------------------------------------------------------------

type Row = tuple[str, Callable[[], object], Callable[..., object], object]

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [LOADING]
def loaded() -> tuple[System.Guid, ModuleType]:
    """Grasshopper 2's plug-in id and its module, the plug-in loaded first."""
    PlugIn.LoadPlugIn(plugin := PlugIn.IdFromName("Grasshopper2"))
    return plugin, import_module("Grasshopper2")


# --- [ROWS]
def member(label: str, owner: object, name: str, *, target: object) -> Row:
    """Row of a named property."""
    return (f"{label} {name}", partial(getattr, owner, name), partial(setattr, owner, name), target)


def found[T](result: tuple[bool, T]) -> T | None:
    """The value of a `TryGet` read, None when the key holds none."""
    ok, value = result
    return value if ok else None


# --- [SKIN]
def dash(edge: object, width: float) -> str:
    """A stock round-capped edge's dash pattern in the skin's spelling for a wire of the width, each dash and gap segment of the pattern Eto's pen hands CoreGraphics keeping its stock length."""
    from Grasshopper2.UI.Skinning import SkinDefinitionText

    return SkinDefinitionText.WriteDash(System.Array[System.Single]([(length * edge.Width + (edge.Width - width) * (1 if index % 2 else -1)) / width for index, length in enumerate(edge.Dash.Dashes)]))


def saved_skin(skins: object, name: str) -> tuple[str, tuple[str, ...]] | None:
    """A stored skin's text as its definition writes it with the faulty lines its load skips, None while the skins folder holds no skin of the name."""
    match skins.Load(name) if skins.Exists(name) else None:
        case (definition, errors):
            return (definition.ToText(), tuple(errors))
        case _:
            return None


# --- [GUISES]
def eto(rgb: Rgb) -> Color:
    """Opaque Eto color of a role."""
    return Color.FromArgb(*rgb)


def number(value: float) -> float | None:
    """A guise size, stroke, or luster, None for the NaN an unset value holds."""
    return None if math.isnan(value) else value


def rgba(value: object) -> tuple[int, ...]:
    """Red, green, blue, and alpha bytes of an Eto color."""
    return (value.Rb, value.Gb, value.Bb, value.Ab)


def slot(item: object, display: object) -> tuple[object, ...]:
    """Color bytes, size or stroke, and symbol, dashes, or stripe of one guise slot."""
    match item:
        case display.GuisePoint():
            return (rgba(item.Colour), number(item.Size), str(item.Symbol))
        case display.GuisePlane():
            return (rgba(item.ColourXAxis), rgba(item.ColourYAxis), rgba(item.ColourLines), number(item.AxisStroke), number(item.LineStroke))
        case display.GuiseFacet():
            return (rgba(item.Colour), number(item.Luster), str(item.Stripe))
        case _:
            return (rgba(item.Colour), number(item.Stroke), str(item.Dashes))


def guise_facts(guises: object, display: object) -> tuple[object, ...]:
    """Every slot of the guises, standard then selected, in the order the guise type declares them."""
    names = [prop.Name for prop in clr.GetClrType(display.Guise).GetProperties() if not prop.PropertyType.IsValueType]
    return tuple(slot(getattr(guise, name), display) for guise in (guises.Standard, guises.Selected) for name in names)


def declared_guises(display: object, point_size: float) -> object:
    """Preview guises from the roles: geometry points and curves the edges inherit, the axis triad on planes, the neutral surface, and the selection pair with its glow percent."""
    point, curve, plane, facet = display.GuisePoint, display.GuiseCurve, display.GuisePlane, display.GuiseFacet
    axes = plane(eto(Axis.X), eto(Axis.Y), eto(Line.GRID), 1.0, 1.0)
    standard = display.Guise(
        points=point(eto(Line.GEOMETRY), point_size, display.Symbol.Cross),
        planes=axes,
        curves=curve(eto(Line.GEOMETRY), 1.0, display.Dashes.Solid),
        planarNatural=facet(eto(Surface.SHADED), 0.0, display.Stripe.Flat),
    )
    chosen = display.Guise(
        points=point(eto(Selection.ITEM), point_size, display.Symbol.Unset),
        planes=axes.WithoutLineColours(),
        curves=curve(eto(Selection.ITEM), 1.0, display.Dashes.Solid),
        planarNatural=facet(eto(Selection.ITEM), SELECTION_FILL_ALPHA * 100, display.Stripe.Flat),
    )
    return display.Guises(standard, chosen)


# --- [KEYS]
def stored(file: object, names: Mapping[str, object]) -> dict[str, object]:
    """Each named key's value in a Grasshopper settings file reloaded from disk, None for a key the file does not hold."""
    from GrasshopperIO import Name

    file.TryLoadSettingsFromFile()
    return {name: None if (item := file.Node.FindItem(Name(name))) is None else item.RawData for name in names}


def save(file: object, values: Mapping[str, object]) -> bool:
    """Whether the values, set over a Grasshopper settings file reloaded from disk, reached the file."""
    file.TryLoadSettingsFromFile()
    for name, value in values.items():
        file.Set(name, value)
    return file.TrySaveSettingsToFile()


# --- [EDITOR]
def window_keys(editor: object) -> dict[str, int]:
    """The open editor's frame origin and client size under the settings keys its close writes."""
    return {"LeftEdge": editor.Location.X, "TopEdge": editor.Location.Y, "Width": editor.ClientSize.Width, "Height": editor.ClientSize.Height}


def place(editor: object, keys: Mapping[str, int]) -> bool:
    """Whether the open editor's settings file took the keys once its client area took their size and its frame their origin."""
    editor.ClientSize, editor.Location = Size(keys["Width"], keys["Height"]), Point(keys["LeftEdge"], keys["TopEdge"])
    return save(editor.EditorSettings, keys)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(point_size: float, rendered: Callable[[str, Mapping[str, object]], str]) -> tuple[Row, ...]:
    """Every Grasshopper 2 row but the editor rectangle, with preview points at the point size and each resource file's placeholders rendered."""
    plugin, grasshopper = loaded()
    banner, (skin_file, rules_file) = PlugIn.Find(plugin).CommandSettings("GH2"), (Path(__file__).with_name(name) for name in ("canvas.ghskin", "tabs.rules"))
    settings, skinning, snapping, ribbon = grasshopper.Settings, grasshopper.UI.Skinning, grasshopper.UI.Canvas.SnappingSettings, grasshopper.UI.TabbedPanel.Layout
    skins, reasons, scratch = skinning.SkinServer, grasshopper.Doc.AutoSaveReason, grasshopper.SpecialObjects.ScratchObject
    radius, wire, scale = 4, 1, Screen.PrimaryScreen.LogicalPixelSize
    dashes = SimpleNamespace(**{str(kind): dash(edge, wire) for kind in System.Enum.GetValues(clr.GetClrType(skinning.WireKind)) if not (edge := skinning.WiresSkin.DefaultDim[kind].Outer).IsSolid})
    values = {"GRID_THIN_ALPHA": GRID_MINOR_ALPHA / scale, "CORNER_RADIUS": radius, "WIRE_WIDTH": wire, "Dash": dashes}
    (definition, faults), rules = skinning.SkinDefinition.Parse(rendered(skin_file.read_text(encoding="utf-8"), values)), rendered(rules_file.read_text(encoding="utf-8"), {})
    guises = declared_guises(grasshopper.Display, point_size)
    ribbon_sizes = (("ItemSize", 24), ("ItemGap", 4), ("PanelGap", 5), ("PanelBar", 15), ("TabHeight", 25), ("TabPadding", 5), ("SizingBar", 5), ("TabColour", 2), ("TabRadius", radius))
    fonts = Path(grasshopper.Folders.ResourceFolder(grasshopper.ResourceFolder.Fonts))
    families = clr.GetClrType(scratch).GetMethod("SetDefaultStyle").GetParameters()[0].ParameterType.GetGenericArguments()[0]
    family, stroke, doubling = System.Enum.Parse(families, "Orange"), 3.0, False
    tint, style = {"DefaultColour": rendered("$Guide.CONSTRUCTION", {})}, {"Colour": int(family), "Stroke": stroke, "Double": doubling}
    rule_editor, tabs = grasshopper.SettingsFile.InDefaultFolder("DisplayRuleEditor"), grasshopper.SettingsFolder("ComponentTabs")
    control, current = tabs.GetSettings("Control"), {"CurrentRuleSet": rules_file.name}

    def setting(name: str, *, target: object) -> Row:
        field = getattr(settings, name)
        return (f"grasshopper {name}", lambda: field.Value, partial(setattr, field, "Value"), target)

    return (
        ("grasshopper skin", partial(saved_skin, skins, skin_file.stem), lambda _: skins.Save(skin_file.stem, definition), (definition.ToText(), tuple(faults))),
        ("grasshopper GH2 ShowBanner", lambda: found(banner.TryGetBool("ShowBanner")), partial(banner.SetBool, "ShowBanner"), False),
        setting("DarkMode", target=True),
        setting("CanvasSkin", target=skin_file.stem),
        setting("LooseWindow", target=False),
        setting("CanvasHints", target=False),
        setting("CanvasSnapToObjects", target=True),
        setting("MultiThreading", target=3),
        setting("UserDays", target=max(settings.UserDays.Value, 15)),
        setting("AutoSaveReasons", target=sum(flag for flag in map(int, System.Enum.GetValues(clr.GetClrType(reasons))) if flag.bit_count() == 1 and flag != int(reasons.Force))),
        (
            "grasshopper snap feedback",
            lambda: (snapping.Current.Feedback, rgba(snapping.Current.Colour)),
            lambda _: setattr(snapping, "Current", snapping.Current.WithFeedback(drawFeedback=True, colour=eto(Guide.TRACKING))),
            (True, (*Guide.TRACKING, 255)),
        ),
        (
            "grasshopper preview guises",
            lambda: guise_facts(grasshopper.Display.Defaults.UserDefault, grasshopper.Display),
            lambda _: setattr(grasshopper.Display.Defaults, "UserDefault", guises),
            guise_facts(guises, grasshopper.Display),
        ),
        *(member("grasshopper ribbon", ribbon, name, target=size) for name, size in ribbon_sizes),
        *(
            (f"grasshopper font {name}", partial((fonts / name).read_text, encoding="utf-8"), partial((fonts / name).write_text, encoding="utf-8"), face.family)
            for name, face in (("SansSerif.txt", Typography.INTERFACE), ("Monospace.txt", Typography.MONOSPACE))
        ),
        ("grasshopper DisplayRuleEditor", partial(stored, rule_editor, tint), partial(save, rule_editor), tint),
        (
            "grasshopper ScratchObject",
            partial(stored, grasshopper.SettingsFile.InDefaultFolder("ScratchObject"), style),
            lambda _: scratch.SetDefaultStyle(family, stroke, doubling, None, None),
            style,
        ),
        ("grasshopper tab rules", partial(tabs.GetText, rules_file.name), partial(tabs.SetText, rules_file.name), rules),
        ("grasshopper tab rule set", partial(stored, control, current), partial(save, control), current),
    )


def editor_rectangle() -> Row:
    """The editor's rectangle over the active document's views as the settled window lays them out: their left edge and width, the node editor height, and the bottom on theirs."""
    _, grasshopper = loaded()
    editor, editor_file, scale = grasshopper.UI.Editor.Instance, grasshopper.SettingsFile.InDefaultFolder("Editor"), Screen.PrimaryScreen.LogicalPixelSize
    views = reduce(Rectangle.Union, (area for view in RhinoDoc.ActiveDoc.Views if not (area := view.ScreenRectangle).IsEmpty))
    title = Mac64Extensions.ToEtoSize(NSWindow.FrameRectFor(CGRect.Empty, NSWindowStyle.Titled).Size).Height
    rectangle = {"LeftEdge": round(views.Left / scale), "TopEdge": round(views.Bottom / scale) - title - NODE_EDITOR, "Width": round(views.Width / scale), "Height": NODE_EDITOR}
    placement = (partial(stored, editor_file, rectangle), partial(save, editor_file)) if editor is None else (partial(window_keys, editor), partial(place, editor))
    return ("grasshopper editor rectangle", *placement, rectangle)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Row", "editor_rectangle", "found", "member", "rows"]
