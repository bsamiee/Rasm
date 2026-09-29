# ty: ignore[unresolved-attribute, unresolved-import]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, no-any-return"
# ruff: file-ignore[import-outside-top-level]
"""Grasshopper 2's configuration as rows the Rhino script converges."""

from collections.abc import Mapping
from functools import partial
from importlib import import_module
from math import sumprod
from operator import sub
from pathlib import Path
from types import ModuleType, SimpleNamespace

from AppKit import NSWindow, NSWindowStyle
import clr
from CoreGraphics import CGRect
from Eto.Drawing import Color, Rectangle
from Eto.Forms import Screen
from Eto.Mac import Mac64Extensions
import Rhino
from Rhino.PlugIns import PlugIn
import System

from interface.frame import LOWER_EDITOR
from interface.report import Row
from interface.rhino.rows import hex_color, key, member, plain
from interface.rhino.window import Site
from interface.roles import Alpha, Axis, blend, Guide, Line, Modality, Selection, substituted, Surface, SWATCHES, Tag, TAGS, Text, Typography, Wire

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [TEMPLATES]
def spelled(value: object) -> str:
    """Placeholder value in Grasshopper 2's file spelling, an opacity fraction as a percent and a role as `#RRGGBB`."""
    match value:
        case float():
            return f"{round(value * 100)}%"
        case tuple():
            return hex_color(value)
        case _:
            return str(value)


# --- [FACTS]
def facts(item: object) -> tuple[object, ...]:
    """Every public value-typed property of a .NET object in declaration order."""
    return tuple(plain(prop.GetValue(item)) for prop in item.GetType().GetProperties() if prop.PropertyType.IsValueType)


def guise_facts(guises: object) -> tuple[tuple[object, ...], ...]:
    """Every slot of the resolved guises, standard then selected, in declaration order."""
    return tuple(facts(prop.GetValue(guise)) for guise in (guises.Standard, guises.Selected) for prop in guise.GetType().GetProperties() if not prop.PropertyType.IsValueType)


def swatches(palette: object) -> tuple[tuple[object, ...], ...] | None:
    """Named palette's colors as name, importance, and bytes in name order, None while no palette answers the name."""
    return None if palette is None else tuple(sorted((color.Name, plain(color.Importance), plain(color.Colour)) for color in palette.Colours))


# --- [SKIN]
def dash(writer: object, edge: object, width: float) -> str:
    """Stock round-capped edge's dash pattern in the skin's spelling for a wire of the width, each segment at the stock length Eto's pen hands CoreGraphics."""
    return writer.WriteDash(System.Array[System.Single]([(length * edge.Width + (edge.Width - width) * (1 if index % 2 else -1)) / width for index, length in enumerate(edge.Dash.Dashes)]))


def saved_skin(skins: object, name: str) -> str | None:
    """Stored skin's text as its definition writes it, None while the skins folder holds no skin of the name."""
    return skins.Load(name)[0].ToText() if skins.Exists(name) else None


# --- [GUISES]
def declared_guises(display: object, point_width: float, stroke: float) -> object:
    """Preview guises from the roles, standard and selected."""
    ink, selected = Color.FromArgb(*Modality.DISPLAY.mark), Color.FromArgb(*Selection.ITEM)
    curves, clear = display.GuiseCurve(ink, stroke), display.GuiseCurve(Color.FromArgb(*Modality.DISPLAY.mark, 0), stroke)
    axes = display.GuisePlane(Color.FromArgb(*Axis.X), Color.FromArgb(*Axis.Y), Color.FromArgb(*Line.DATUM_GRID), stroke, stroke)
    shaded = display.GuiseFacet(Color.FromArgb(*Surface.SHADED), 0.0, display.Stripe.Flat)
    standard = display.Guise(points=display.GuisePoint(ink, point_width, display.Symbol.Circle), planes=axes, curves=curves, isocurves=clear, planarNatural=shaded)
    chosen = display.Guise(
        points=display.GuisePoint(selected, point_width, display.Symbol.Unset), planes=axes.WithoutLineColours(), curves=display.GuiseCurve(selected, stroke), isocurves=clear, planarNatural=shaded
    )
    return display.Guises(standard, chosen)


# --- [KEYS]
def stored(file: object, names: Mapping[str, object]) -> dict[str, object]:
    """Each named key's value in a Grasshopper settings file reloaded from disk, None for a key the file lacks."""
    from GrasshopperIO import Name

    file.TryLoadSettingsFromFile()
    return {name: None if (item := file.Node.FindItem(Name(name))) is None else item.RawData for name in names}


def save(file: object, values: Mapping[str, object]) -> None:
    """Set the values over a Grasshopper settings file reloaded from disk and save the file."""
    file.TryLoadSettingsFromFile()
    for name, value in values.items():
        file.Set(name, value)
    file.TrySaveSettingsToFile()


# --- [TABS]
def tab_rows(grasshopper: ModuleType, name: str, rules: Path, count: int) -> tuple[Row, Row]:
    """Rows of a tab control's rule set rendered from its file and of its Control settings naming that set."""
    folder = grasshopper.SettingsFolder(name)
    control, settings = {"CurrentRuleSet": rules.name, "RowCount": count, "TabPreview": True}, folder.GetSettings("Control")
    return (
        Row(label=f"grasshopper {name} rule set", read=partial(folder.GetText, rules.name), write=partial(folder.SetText, rules.name), target=substituted(rules.read_text(encoding="utf-8"), spelled)),
        Row(label=f"grasshopper {name} Control", read=partial(stored, settings, control), write=partial(save, settings), target=control),
    )


# --- [EDITOR]
def editor_row(grasshopper: ModuleType, doc: Rhino.RhinoDoc, scale: float, shift: Mapping[Site, float]) -> Row:
    """Row of the editor's rectangle over the document's views, each dock site grown by its shift and the lower editor height tall, through the open editor's settings or its settings file."""
    areas = [area for view in doc.Views if not (area := view.ScreenRectangle).IsEmpty]
    left, right = min(area.Left for area in areas) / scale + shift[Site.LEFT], max(area.Right for area in areas) / scale - shift[Site.RIGHT]
    title = Mac64Extensions.ToEtoSize(NSWindow.FrameRectFor(CGRect.Empty, NSWindowStyle.Titled).Size).Height
    bounds = Rectangle(round(left), round(max(area.Bottom for area in areas) / scale - shift[Site.BOTTOM]) - title - LOWER_EDITOR, round(right - left), LOWER_EDITOR)

    def keys(frame: Rectangle) -> dict[str, int]:
        """Editor's frame origin and client size under the settings keys its close writes."""
        return {"LeftEdge": frame.X, "TopEdge": frame.Y, "Width": frame.Width, "Height": frame.Height}

    label, target, editor = "grasshopper editor rectangle", keys(bounds), grasshopper.UI.Editor.Instance
    match editor:
        case None:
            file = grasshopper.SettingsFile.InDefaultFolder("Editor")
            return Row(label=label, read=partial(stored, file, target), write=partial(save, file), target=target)
        case _:

            def place(values: Mapping[str, int]) -> None:
                editor.ClientSize, editor.Location = bounds.Size, bounds.Location
                save(editor.EditorSettings, values)

            return Row(label=label, read=lambda: keys(Rectangle(editor.Location, editor.ClientSize)), write=place, target=target)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(doc: Rhino.RhinoDoc, point_width: float, shift: Mapping[Site, float]) -> tuple[Row, ...]:
    """Every Grasshopper 2 row, the plug-in loaded first, points at Rhino's point width and the rectangle over the views grown by each dock site's shift."""
    PlugIn.LoadPlugIn(plugin := PlugIn.IdFromName("Grasshopper2"))
    grasshopper = import_module("Grasshopper2")
    settings, skinning, snapping, ribbon, display = grasshopper.Settings, grasshopper.UI.Skinning, grasshopper.UI.Canvas.SnappingSettings, grasshopper.UI.TabbedPanel.Layout, grasshopper.Display
    folder, value = Path(__file__).parent, "Value"
    skin_file = folder / "Nippon.ghskin"
    fonts = Path(grasshopper.Folders.ResourceFolder(grasshopper.ResourceFolder.Fonts))
    swatch_file = Path(grasshopper.Folders.ResourceFolder(grasshopper.ResourceFolder.Colours)) / f"{TAGS}.ghdic"
    loaded_plugin = PlugIn.Find(plugin)
    command, logs, skins, reasons, scratch = loaded_plugin.CommandSettings("GH2"), loaded_plugin.Settings, skinning.SkinServer, grasshopper.Doc.AutoSaveReason, grasshopper.SpecialObjects.ScratchObject
    colors, picker, arrows = grasshopper.Types.Colour, grasshopper.UI.ColourPicker, grasshopper.UI.Canvas.Shapes.ArrowStyle
    radius, columns, wire_width, scale, author = 3, 5, 1, Screen.PrimaryScreen.LogicalPixelSize, Rhino.RhinoApp.LicenseUserName.strip()
    stroke, cell, spread = (scale + 1) / (2 * scale), skinning.CanvasSkin.DefaultDim.GridSkin.Cell, tuple(map(sub, Text.SECONDARY, Surface.PANEL))
    dashes = SimpleNamespace(**{
        str(kind): dash(skinning.SkinDefinitionText, edge, wire_width)
        for kind in System.Enum.GetValues(clr.GetClrType(skinning.WireKind))
        if not (edge := skinning.WiresSkin.DefaultDim[kind].Outer).IsSolid
    })
    definition, _ = skinning.SkinDefinition.Parse(
        substituted(
            skin_file.read_text(encoding="utf-8"),
            spelled,
            AUTHOR=author,
            GRID_THIN_ALPHA=Alpha.GRID_MINOR / scale,
            GRID_THICK_ALPHA=Alpha.GRID_MAJOR / scale,
            GRID_COLUMNS=columns,
            GRID_ROWS=round(columns * cell.Width / cell.Height),
            VEIL_ALPHA=sumprod(spread, map(sub, Text.SECONDARY, Text.DISABLED)) / sumprod(spread, spread),
            SELECTED_WIRE=blend(Wire.SELECTED, Wire.CORE, Alpha.WIRE_SELECTED),
            CORNER_RADIUS=radius,
            WIRE_WIDTH=wire_width,
            Dash=dashes,
        )
    )
    guises, snap = declared_guises(display, point_width, stroke), snapping.Default.WithFeedback(drawFeedback=True, colour=Color.FromArgb(*Guide.TRACKING))
    match_family = System.Type.GetType("Eto.Drawing.OpenColor, Grasshopper2", throwOnError=True).GetMethod("MatchFamily", System.Array[System.Type]([clr.GetClrType(Color)]))
    family = match_family.Invoke(None, System.Array[System.Object]([Color.FromArgb(*Guide.CONSTRUCTION)]))
    palette = colors.NamedPalette(System.Array[colors.NamedColour]([colors.NamedColour(name, Color.FromArgb(*rgb)) for name, rgb in SWATCHES.items()]))
    sketch_stroke, doubling, arrow_factor, bezier = 3.0, False, 1.0, 0
    rule_editor = {
        "DefaultColour": hex_color(Tag.COLOR_01.value),
        "DefaultStroke": f"{stroke:g}",
        "DefaultRadius": f"{point_width:g}",
        "DefaultSymbol": str(display.Symbol.Circle),
        "DefaultDashes": "Medium",
        "Width": 300,
        "Height": 500,
        "RuleHeight": 100,
        "FeedHeight": 200,
    }
    sketch = {"Colour": int(family), "Stroke": sketch_stroke, "Double": doubling, "ArrowHead": int(arrows.End), "ArrowFactor": arrow_factor, "DefaultShape": bezier}
    rule_file, sketch_file, picker_file = (grasshopper.SettingsFile.InDefaultFolder(name) for name in ("DisplayRuleEditor", "ScratchObject", "colourpicker"))
    choices = {"MostRecentPalette": TAGS, "MostRecentDiscrete": True, "MostRecentFormat": int(picker.ColourPickerFormat.Hexadecimal), "MostRecentSpace": int(colors.Space.Rgb)}
    ribbon_sizes = (
        ("ItemSize", 24),
        ("ItemGap", 4),
        ("PanelGap", 5),
        ("PanelBar", 15),
        ("TabHeight", 25),
        ("TabOverlap", 2),
        ("TabPadding", 5),
        ("SizingBar", 5),
        ("TabColour", 2),
        ("TabRadius", radius),
    )
    decided = (
        (settings.DarkMode, True),
        (settings.CanvasSkin, skin_file.stem),
        (settings.CanvasHints, False),
        (settings.CanvasSnapToObjects, True),
        (settings.MultiThreading, 3),
        (settings.UserName, author),
        (settings.SeededSkins, "\n".join(skins.StandardNames())),
        (settings.AutoSaveReasons, sum(flag for flag in map(int, System.Enum.GetValues(clr.GetClrType(reasons))) if flag.bit_count() == 1 and flag != int(reasons.Force))),
    )
    factory = (
        settings.ZoomThresholdStandard,
        settings.ReinstateSession,
        settings.LooseWindow,
        settings.CanvasAutoSearch,
        settings.DisplayExperimentalShader,
        settings.CanvasDrawFullNames,
        settings.CanvasDrawLabels,
        settings.CanvasLabelAbove,
        settings.CanvasLabelName,
        settings.CanvasLabelPlugin,
        settings.CanvasLabelProfiler,
        settings.CanvasLabelSlowOnly,
        settings.CanvasFrameDiagnostics,
        settings.CanvasShowIcons,
        settings.CanvasMessages,
        settings.TooltipEnabled,
        settings.ShowObscure,
        settings.MultiFileUi,
        settings.PrimaryUiLang,
        settings.ReducedThreads,
        settings.AutomaticDocumentationContent,
        settings.GlobalClipboard,
        settings.MeritWarning,
        settings.DuplicateInputs,
        settings.DelayPeriod,
    )

    def styled(_: object) -> None:
        """Write the scratch object's default style through its owner and its default shape into the same settings file."""
        scratch.SetDefaultStyle(family, sketch_stroke, doubling, arrows.End, arrow_factor)
        save(sketch_file, {"DefaultShape": bezier})

    return (
        Row(label="grasshopper skin", read=partial(saved_skin, skins, skin_file.stem), write=lambda _: skins.Save(skin_file.stem, definition), target=definition.ToText()),
        key(command, "ShowBanner", target=False, default=True, label="grasshopper GH2"),
        key(command, "ShowEditor", target=True, default=True, label="grasshopper GH2"),
        key(command, "LoadLevel", target=3, default=3, label="grasshopper GH2"),
        key(logs, "AutoDeleteOldLogs", target=True, label="grasshopper plugin"),
        key(logs, "LogDensity", target=int(grasshopper.Diagnostics.Density.Verbose), label="grasshopper plugin"),
        Row(
            label="grasshopper DocumentationAuthoringFolder",
            read=lambda: grasshopper.Folders.DocumentationAuthoringFolder,
            write=partial(setattr, grasshopper.Folders, "DocumentationAuthoringFolder"),
            target="",
        ),
        *(
            Row(label=f"grasshopper {field.Name}", read=partial(getattr, field, value), write=partial(setattr, field, value), target=target)
            for field, target in (*decided, *((field, field.Default) for field in factory))
        ),
        Row(label="grasshopper UserDays small section labels", read=lambda: settings.UserDays.Value >= 15, write=lambda _: setattr(settings.UserDays, value, 15), target=True),
        Row(label="grasshopper CanvasSnapping", read=lambda: facts(snapping.Current), write=lambda _: setattr(snapping, "Current", snap), target=facts(snap)),
        Row(label="grasshopper UserDisplayStyle", read=lambda: guise_facts(display.Defaults.UserDefault), write=lambda _: setattr(display.Defaults, "UserDefault", guises), target=guise_facts(guises)),
        *(member("grasshopper ribbon", ribbon, name, target=size) for name, size in ribbon_sizes),
        *(
            Row(label=f"grasshopper font {name}", read=partial((fonts / name).read_text, encoding="utf-8"), write=partial((fonts / name).write_text, encoding="utf-8"), target=face.family)
            for name, face in (("SansSerif.txt", Typography.INTERFACE), ("Serif.txt", Typography.INTERFACE), ("Script.txt", Typography.INTERFACE), ("Monospace.txt", Typography.MONOSPACE))
        ),
        Row(
            label=f"grasshopper palette {TAGS}",
            read=lambda: swatches(colors.NamedPalette.ImpliedPalette(TAGS)),
            write=lambda _: colors.NamedPalette.WriteToFile(str(swatch_file), palette),
            target=swatches(palette),
        ),
        Row(label="grasshopper colourpicker", read=partial(stored, picker_file, choices), write=partial(save, picker_file), target=choices),
        Row(label="grasshopper DisplayRuleEditor", read=partial(stored, rule_file, rule_editor), write=partial(save, rule_file), target=rule_editor),
        Row(label="grasshopper ScratchObject", read=partial(stored, sketch_file, sketch), write=styled, target=sketch),
        *(row for name, rules, count in (("ComponentTabs", folder / "components.rules", 2), ("FunctionTabs", folder / "functions.rules", 1)) for row in tab_rows(grasshopper, name, rules, count)),
        editor_row(grasshopper, doc, scale, shift),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
