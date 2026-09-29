# ty: ignore[invalid-exception-caught, no-matching-overload, unresolved-import]
# mypy: disable-error-code="arg-type, import-not-found, import-untyped, misc, no-any-return, no-any-unimported"
"""Grasshopper 2's settings, skin, ribbon, preview, snapping, fonts, palette, and editor rows the Rhino run converges."""

from collections.abc import Iterable, Mapping
from functools import partial
from itertools import starmap
from math import sumprod
from operator import sub
from pathlib import Path
from typing import Final, TYPE_CHECKING

from AppKit import NSWindow, NSWindowStyle
import clr
from CoreGraphics import CGRect
from Eto.Drawing import Color
from Eto.Forms import Screen
from Eto.Mac import Mac64Extensions
from Grasshopper2 import Folders, ResourceFolder, Settings, SettingsFile, SettingsFolder
from Grasshopper2.Diagnostics import Density
from Grasshopper2.Display import Defaults, Guise, GuiseCurve, GuiseFacet, GuisePlane, GuisePoint, Guises, Stripe, Symbol
from Grasshopper2.Doc import AutoSaveReason
from Grasshopper2.SpecialObjects import ScratchObject
from Grasshopper2.Types.Colour import ColourImportance, NamedColour, NamedPalette, Space
from Grasshopper2.UI.Canvas import SnappingSettings
from Grasshopper2.UI.Canvas.Shapes import ArrowStyle
from Grasshopper2.UI.ColourPicker import ColourPickerFormat
from Grasshopper2.UI.Skinning import GridSkin, PropertyId, SkinDefinition, SkinProperties, SkinServer
from Grasshopper2.UI.TabbedPanel import Layout
from GrasshopperIO import Name
import Rhino
from Rhino.PlugIns import PlugIn
import System
from System import Array, Single
from System.IO import FileNotFoundException

from interface.frame import LOWER_EDITOR
from interface.report import Row
from interface.rhino.script.accessors import defaulted, hex_color, Internal, key, member
from interface.roles import Alpha, Axis, blend, Guide, Line, Modality, Selection, substituted, Surface, SWATCHES, Tag, TAGS, Text, Typography, Wire

if TYPE_CHECKING:
    type Setting = Settings.Setting
else:
    Setting = getattr(Settings, "Setting`1")

# --- [CONSTANTS] ------------------------------------------------------------------------

SKIN: Final = "Dark"
CORNER_RADIUS: Final = 3

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SKIN]
def spelled(value: object) -> str:
    """Placeholder value in Grasshopper 2's file spelling, an opacity fraction as a whole percent and a role as `#RRGGBB`."""
    match value:
        case float():
            return f"{round(value * 100)}%"
        case tuple():
            return hex_color(value)
        case _:
            return str(value)


def definition(scale: float) -> SkinDefinition:
    """Skin the template renders from the roles at the screen scale, each dashed wire's stock pattern kept at its drawn width, raising on a line Grasshopper 2 cannot parse."""
    columns, cell, spread = 5, GridSkin.DefaultRegularDim.Cell, tuple(map(sub, Text.SECONDARY, Surface.PANEL))
    skin, errors = SkinDefinition.Parse(
        substituted(
            Path(__file__).with_name(f"{SKIN}.ghskin").read_text(encoding="utf-8"),
            spelled,
            GRID_THIN_ALPHA=Alpha.GRID_MINOR / scale,
            GRID_THICK_ALPHA=Alpha.GRID_MAJOR / scale,
            GRID_COLUMNS=columns,
            GRID_ROWS=round(columns * cell.Width / cell.Height),
            VEIL_ALPHA=sumprod(spread, map(sub, Text.SECONDARY, Text.DISABLED)) / sumprod(spread, spread),
            SELECTED_WIRE=blend(Wire.SELECTED, Wire.CORE, Alpha.WIRE_SELECTED),
            CORNER_RADIUS=CORNER_RADIUS,
        )
    )
    if errors:
        raise ValueError(f"{SKIN}.ghskin holds lines Grasshopper 2 cannot parse: {'; '.join(errors)}")
    for dash in (each for each in SkinProperties.All if SkinProperties.Mode(each) == PropertyId.Dim and SkinProperties.Role(each) == PropertyId.Dash):
        width = SkinProperties.WithRole(dash, PropertyId.Width)
        stock, drawn = SkinDefinition.Fallback(width).Number, skin.Resolve(width).Number
        skin.Set(dash, Array[Single]([(length * stock + (stock - drawn) * (1 if index % 2 else -1)) / drawn for index, length in enumerate(SkinDefinition.Fallback(dash).Dash)]))
    return skin


# --- [FILES]
def stored(file: str, names: Iterable[str]) -> dict[str, object]:
    """Each named key's raw value in the settings file as it sits on disk, None for a key the file lacks."""
    node = SettingsFile.InDefaultFolder(file).Node
    return {name: None if (item := node.FindItem(Name(name))) is None else item.RawData for name in names}


def saved(file: str, values: Mapping[str, object]) -> None:
    """Set each value in the settings file as it sits on disk and write the file at once."""
    settings = SettingsFile.InDefaultFolder(file)
    for name, value in values.items():
        settings.Set(name, value)
    if not settings.TrySaveSettingsToFile():
        raise OSError(f"Grasshopper 2 settings file {file} was not written")


def file_row(file: str, values: Mapping[str, object]) -> Row:
    """Row of named keys in a settings file, each read and write through a fresh instance of the file."""
    return Row(label=f'SettingsFile["{file}"]', read=partial(stored, file, values), write=partial(saved, file), target=values)


# --- [MEMBERS]
def parts(guise: Guise) -> tuple[dict[str, GuisePoint], dict[str, GuisePlane], dict[str, GuiseCurve], dict[str, GuiseFacet]]:
    """Parts of a guise by the constructor parameter each fills, grouped by part type."""
    return (
        {"points": guise.Points},
        {"planes": guise.Planes},
        {
            "curves": guise.Curves,
            "isocurves": guise.Isocurves,
            "nakedEdges": guise.NakedEdges,
            "innerEdges": guise.InnerEdges,
            "nakedTrims": guise.NakedTrims,
            "innerTrims": guise.InnerTrims,
            "nonManifold": guise.NonManifold,
        },
        {"planarNatural": guise.PlanarNatural, "curvedNatural": guise.CurvedNatural, "planarTrimmed": guise.PlanarTrimmed, "curvedTrimmed": guise.CurvedTrimmed},
    )


def slots(guises: Guises) -> dict[str, dict[str, dict[str, object]]]:
    """Constructor arguments of every part of the resolved standard and selected guises, by parameter name at each level."""

    def members(guise: Guise) -> dict[str, dict[str, object]]:
        points, planes, curves, facets = parts(guise)
        return {
            **{slot: {"colour": point.Colour, "size": point.Size, "symbol": point.Symbol} for slot, point in points.items()},
            **{
                slot: {"colourX": plane.ColourXAxis, "colourY": plane.ColourYAxis, "colourL": plane.ColourLines, "axisStroke": plane.AxisStroke, "lineStroke": plane.LineStroke}
                for slot, plane in planes.items()
            },
            **{slot: {"colour": curve.Colour, "stroke": curve.Stroke, "dashes": curve.Dashes} for slot, curve in curves.items()},
            **{
                slot: {"colour": facet.Colour, "luster": facet.Luster, "stripe": facet.Stripe, "wireframe": facet.Wireframe, "vigour": facet.Vigour, "emission": facet.EmissionColour}
                for slot, facet in facets.items()
            },
        }

    return {"standard": members(guises.Standard), "selected": members(guises.Selected)}


def dressed(held: Mapping[str, Mapping[str, Mapping[str, object]]]) -> Guises:
    """Guises built from each side's part constructor arguments, each part of the type an empty guise holds at its parameter."""
    kinds = {slot: type(part) for group in parts(Guise()) for slot, part in group.items()}
    return Guises(**{side: Guise(**{slot: kinds[slot](**values) for slot, values in members.items()}) for side, members in held.items()})


def snapping(settings: SnappingSettings) -> dict[str, object]:
    """Snapping settings by their seven constructor members."""
    return {
        "rules": settings.Rules,
        "verticalGap": settings.VerticalGapSize,
        "horizontalGap": settings.HorizontalGapSize,
        "edgeRadius": settings.EdgeRadius,
        "wireRadius": settings.WireRadius,
        "feedback": settings.Feedback,
        "colour": settings.Colour,
    }


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(doc: Rhino.RhinoDoc, point_width: float, curve_width: float) -> tuple[Row, ...]:
    """Every Grasshopper 2 row in write order, previews at Rhino's point and curve widths and the editor docked under the document's views."""
    plugin, scale = PlugIn.IdFromName("Grasshopper2"), Screen.PrimaryScreen.LogicalPixelSize
    command = next(each for each in PlugIn.Find(plugin).GetCommands() if each.EnglishName == "GH2")
    matched = Internal.OPEN_COLOR.type.GetMethod("MatchFamily", Array[System.Type]([clr.GetClrType(Color)]))
    skin, family = definition(scale), matched.Invoke(None, Array[System.Object]([Color.FromArgb(*Guide.CONSTRUCTION)]))
    ink, chosen = Color.FromArgb(*Modality.DISPLAY.mark), Color.FromArgb(*Selection.ITEM)
    clear, shaded = GuiseCurve(Color.FromArgb(*Modality.DISPLAY.mark, 0), curve_width), GuiseFacet(Color.FromArgb(*Surface.SHADED), 0.0, Stripe.Flat)
    axes = GuisePlane(Color.FromArgb(*Axis.X), Color.FromArgb(*Axis.Y), Color.FromArgb(*Line.DATUM_GRID), curve_width, curve_width)
    guises = Guises(
        Guise(points=GuisePoint(ink, point_width, Symbol.Circle), planes=axes, curves=GuiseCurve(ink, curve_width), isocurves=clear, planarNatural=shaded),
        Guise(points=GuisePoint(chosen, point_width, Symbol.Unset), planes=axes.WithoutLineColours(), curves=GuiseCurve(chosen, curve_width), isocurves=clear, planarNatural=shaded),
    )
    fonts, palette = Path(Folders.ResourceFolder(ResourceFolder.Fonts)), Path(Folders.ResourceFolder(ResourceFolder.Colours)) / f"{TAGS}.ghdic"
    areas = [area for view in doc.Views if not (area := view.ScreenRectangle).IsEmpty]
    left, right, bottom = min(area.Left for area in areas) / scale, max(area.Right for area in areas) / scale, max(area.Bottom for area in areas) / scale
    title = Mac64Extensions.ToEtoSize(NSWindow.FrameRectFor(CGRect.Empty, NSWindowStyle.Titled).Size).Height
    sketch = {"Colour": int(family), "Stroke": 3.0, "Double": False, "ArrowHead": int(ArrowStyle.End), "ArrowFactor": 1.0, "DefaultShape": 0}
    decided = (
        (Settings.DarkMode, True),
        (Settings.CanvasSkin, SKIN),
        (Settings.SeededSkins, "\n".join(SkinServer.StandardNames())),
        (Settings.CanvasHints, False),
        (Settings.CanvasSnapToObjects, True),
        (Settings.MultiThreading, 3),
        (Settings.AutoSaveReasons, sum(flag for flag in map(int, System.Enum.GetValues(clr.GetClrType(AutoSaveReason))) if flag.bit_count() == 1 and flag != int(AutoSaveReason.Force))),
        (Settings.UserDays, max(Settings.UserDays.Value, 15)),
    )
    factory = (
        Settings.ZoomThresholdDetailed,
        Settings.ZoomThresholdStandard,
        Settings.ReinstateSession,
        Settings.LooseWindow,
        Settings.CanvasAutoSearch,
        Settings.DisplayExperimentalShader,
        Settings.CanvasDrawFullNames,
        Settings.CanvasDrawLabels,
        Settings.CanvasLabelAbove,
        Settings.CanvasLabelName,
        Settings.CanvasLabelPlugin,
        Settings.CanvasLabelProfiler,
        Settings.CanvasLabelSlowOnly,
        Settings.CanvasFrameDiagnostics,
        Settings.CanvasShowIcons,
        Settings.CanvasMessages,
        Settings.TooltipEnabled,
        Settings.ShowObscure,
        Settings.MultiFileUi,
        Settings.GlobalClipboard,
        Settings.PrimaryUiLang,
        Settings.AutomaticDocumentationContent,
        Settings.ReducedThreads,
        Settings.MeritWarning,
        Settings.DuplicateInputs,
        Settings.DelayPeriod,
        Settings.TranslateObjects,
    )

    def setting_row(setting: Setting, target: object) -> Row:
        """Row of one central setting through its value."""

        def valued(value: object) -> None:
            setting.Value = value

        return Row(label=f"Settings.{setting.Name}", read=lambda: setting.Value, write=valued, target=target)

    def user_default(held: Mapping[str, Mapping[str, Mapping[str, object]]]) -> None:
        """Write the user default guises built from the held part arguments."""
        Defaults.UserDefault = dressed(held)

    def snapped(members: Mapping[str, object]) -> None:
        """Write the current snapping settings from their constructor members."""
        SnappingSettings.Current = SnappingSettings(**members)

    def sketched(values: Mapping[str, object]) -> None:
        """Write the default sketch style through its owner, then the default shape into the same settings file."""
        ScratchObject.SetDefaultStyle(System.Enum.ToObject(matched.ReturnType, values["Colour"]), values["Stroke"], values["Double"], ArrowStyle(values["ArrowHead"]), values["ArrowFactor"])
        saved("ScratchObject", {"DefaultShape": values["DefaultShape"]})

    def swatches() -> dict[str, tuple[Color, ColourImportance]] | None:
        """Colors of the tag palette by name with their importance, None while no palette answers the name."""
        held = NamedPalette.ImpliedPalette(TAGS)
        return None if held is None else {named.Name: (named.Colour, named.Importance) for named in held.Colours}

    def loaded() -> str | None:
        """Text of the installed skin, None while the skin server holds no file of the name."""
        try:
            return SkinServer.Load(SKIN)[0].ToText()
        except FileNotFoundException:
            return None

    return (
        Row(label=f'SkinServer["{SKIN}"]', read=loaded, write=lambda written: SkinServer.Save(SKIN, SkinDefinition.Parse(written)[0]), target=skin.ToText()),
        *starmap(setting_row, (*decided, *((setting, setting.Default) for setting in factory))),
        defaulted((command,), "ShowBanner", target=False, default=True),
        defaulted((command,), "ShowEditor", target=True, default=True),
        defaulted((command,), "LoadLevel", target=3, default=3),
        key((plugin,), "AutoDeleteOldLogs", target=True),
        key((plugin,), "LogDensity", target=int(Density.Verbose)),
        member(Folders, "DocumentationAuthoringFolder", target=""),
        *(
            member(Layout, name, target=size)
            for name, size in {
                "TabRadius": CORNER_RADIUS,
                "TabColour": 2,
                "TabPadding": 5,
                "TabHeight": 25,
                "TabOverlap": 2,
                "ItemSize": 24,
                "ItemGap": 4,
                "PanelGap": 5,
                "PanelBar": 15,
                "SizingBar": 5,
            }.items()
        ),
        *(
            row
            for tabs, rules, count in (("ComponentTabs", "components.rules", 2), ("FunctionTabs", "functions.rules", 1))
            for folder in (SettingsFolder(tabs),)
            for row in (
                Row(
                    label=f'{tabs}["{rules}"]',
                    read=partial(folder.GetText, rules),
                    write=partial(folder.SetText, rules),
                    target=substituted(Path(__file__).with_name(rules).read_text(encoding="utf-8"), spelled),
                ),
                file_row(f"{tabs}/Control", {"CurrentRuleSet": rules, "RowCount": count, "TabPreview": True}),
            )
        ),
        Row(label="Defaults.UserDefault", read=lambda: slots(Defaults.UserDefault), write=user_default, target=slots(guises)),
        Row(
            label="SnappingSettings.Current",
            read=lambda: snapping(SnappingSettings.Current),
            write=snapped,
            target=snapping(SnappingSettings.Default.WithFeedback(drawFeedback=True, colour=Color.FromArgb(*Guide.TRACKING))),
        ),
        *(
            Row(label=f'Fonts["{name}"]', read=partial((fonts / name).read_text, encoding="utf-8"), write=partial((fonts / name).write_text, encoding="utf-8"), target=face.family)
            for name, face in (("SansSerif.txt", Typography.INTERFACE), ("Monospace.txt", Typography.MONOSPACE), ("Serif.txt", Typography.INTERFACE), ("Script.txt", Typography.INTERFACE))
        ),
        Row(
            label=f'NamedPalette["{TAGS}"]',
            read=swatches,
            write=lambda colors: NamedPalette.WriteToFile(
                str(palette), NamedPalette(Array[NamedColour]([NamedColour(name, color, importance=importance) for name, (color, importance) in colors.items()]))
            ),
            target={name: (Color.FromArgb(*rgb), ColourImportance.Primary) for name, rgb in SWATCHES.items()},
        ),
        file_row("colourpicker", {"MostRecentPalette": TAGS, "MostRecentDiscrete": True, "MostRecentFormat": int(ColourPickerFormat.Hexadecimal), "MostRecentSpace": int(Space.Rgb)}),
        file_row(
            "DisplayRuleEditor",
            {
                "DefaultColour": hex_color(Tag.COLOR_01.value),
                "DefaultStroke": f"{curve_width:g}",
                "DefaultRadius": f"{point_width:g}",
                "DefaultSymbol": str(Symbol.Circle),
                "DefaultDashes": "Medium",
                "Width": 300,
                "Height": 500,
                "RuleHeight": 100,
                "FeedHeight": 200,
            },
        ),
        Row(label='SettingsFile["ScratchObject"]', read=partial(stored, "ScratchObject", sketch), write=sketched, target=sketch),
        file_row("Editor", {"LeftEdge": round(left), "TopEdge": round(bottom) - title - LOWER_EDITOR, "Width": round(right - left), "Height": LOWER_EDITOR}),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
