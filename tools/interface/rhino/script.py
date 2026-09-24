# ty: ignore[unresolved-attribute, no-matching-overload, unsupported-operator, invalid-argument-type, unresolved-import, redundant-condition-strict]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, no-any-return, arg-type, call-overload, operator"
# ruff: file-ignore[print]
"""Rhino settings as rows of a label, a read, a write, and a target, converged and reported inside Rhino's CPython."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from configparser import ConfigParser
from datetime import timedelta
from functools import partial, reduce
from importlib import import_module
import math
from pathlib import Path
from string import Template
from tempfile import TemporaryDirectory
from types import MappingProxyType, SimpleNamespace
from typing import Final

import clr
from Foundation import NSArray, NSString, NSUserDefaults, NSUserDefaultsType
import location
from radix import Rgb
from render import (
    CAUSTICS,
    DIFFUSE_BOUNCES,
    DPI,
    FILTER_GLOSSY,
    FRAME_SIZE,
    GLOSSY_BOUNCES,
    INDIRECT_CLAMP,
    MATERIALS,
    MAX_BOUNCES,
    NOISE_THRESHOLD,
    SAMPLES,
    stocked,
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
import Rhino
from rhino import window
import Rhino.ApplicationSettings as Settings
from Rhino.Display import DefinedViewportProjection, DisplayModeDescription
from Rhino.DocObjects import DimensionStyle, HatchPattern, Linetype, ObjectSectionFillRule, ObjectType, SectionBackgroundFillMode, SectionStyle
from Rhino.DocObjects.Tables import RestoreLayerProperties
from Rhino.FileIO import File3dm, File3dmWriteOptions, FileWriteOptions
from Rhino.Geometry import BoundingBox, MeshingParameterStyle, Vector3d
from rhino.grasshopper import settings as grasshopper
from rhino.grasshopper.settings import found, member, Row
from Rhino.PlugIns import PlugIn, PlugInLoadTime
from Rhino.Render import ContentUuids, RenderContent, RenderContentType, RenderSettings, SupportOptions
from Rhino.Runtime import HostUtils, NamedParametersEventArgs
from rhino.window import Extent, Panel, Scope, Site
import System
from System import Array, DateTime, DateTimeKind, Guid, String
from System.Collections.Generic import List
from System.Drawing import Color as DrawingColor, ColorTranslator, Size
from System.Globalization import CultureInfo
from System.Reflection import BindingFlags
import theme
from theme import Accent, Axis, blend, GRID_MAJOR_ALPHA, GRID_MINOR_ALPHA, Guide, Line, Selection, SELECTION_FILL_ALPHA, Status, Surface, Tag, Text, Typography
from units import GRID_EXTENT, GRID_THICK_EVERY, INCH, MILLIMETER, NUDGE, Units

# --- [TYPES] ----------------------------------------------------------------------------

type Facts = dict[str, object]
type Reading = tuple[str, Callable[[], object], object]

# --- [CONSTANTS] ------------------------------------------------------------------------

CUT_STYLE: Final = "Cut"
SKY_USAGES: Final = tuple(System.Enum.GetValues(clr.GetClrType(RenderSettings.EnvironmentUsage)))
SKY_SLOT: Final = "texture"
SKY_SUN: Final = "use-document-sun"
SKY_GAIN: Final = "rdk-texture-adjust-multiplier"
SKY_MULTIPLIER: Final = 8.21
SUN_INTENSITY: Final = 1.115
RENDER_KEYS: Final = MappingProxyType({
    "UseDocumentSamples": True,
    "Samples": SAMPLES,
    "AdaptiveThreshold": NOISE_THRESHOLD,
    "MaxBounce": MAX_BOUNCES,
    "MaxDiffuseBounce": DIFFUSE_BOUNCES,
    "MaxGlossyBounce": GLOSSY_BOUNCES,
    "MaxTransmissionBounce": TRANSMISSION_BOUNCES,
    "MaxVolumeBounce": VOLUME_BOUNCES,
    "TransparentMaxBounce": TRANSPARENT_BOUNCES,
    "SampleClampDirect": 0.0,
    "SampleClampIndirect": INDIRECT_CLAMP,
    "FilterGlossy": FILTER_GLOSSY,
    "CausticsReflective": CAUSTICS,
    "CausticsRefractive": CAUSTICS,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Placeholders(Template):
    """A resource file whose placeholders name a theme export or a run-time value by dotted path, `$Surface.WELL` or `$PointStyle.RoundDot`."""

    idpattern = r"(?a:[_a-z][_a-z0-9]*(?:\.[_a-z][_a-z0-9]*)?)"


class DocumentUnits:
    """Units, tolerance, distance displays and precisions, grid spacing, snap, and line count, dimension style and length displays, and linetype patterns a document takes from one units declaration."""

    def __init__(self, units: Units) -> None:
        """Derive every fact from the declaration, the other declaration's model unit giving the alternate dimension."""
        other = next(system for system in Units if system is not units)
        alternate = unit_system(other.length)
        self.model, self.page = unit_system(units.length), unit_system(units.page)
        scale = Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Meters, self.model)
        self.tolerance, self.grid, self.snap, self.lines = units.tolerance * scale, units.grid * scale, units.snap * scale, round(GRID_EXTENT / units.grid)
        self.model_display, self.page_display = distance_display(self.model, page=False), distance_display(self.page, page=True)
        self.model_precision, self.page_precision = precision(units, self.model, self.model_display), precision(units, self.page, self.page_display)
        self.dimension_display, self.alternate_display = length_display(self.model), length_display(alternate)
        self.alternate_precision = precision(other, alternate, distance_display(alternate, page=False))
        self.style = "Template Foot-Inch Architectural" if units is Units.IMPERIAL else "Template Millimeter Architectural"
        self.linetypes = default_linetypes(units.resolution / MILLIMETER)


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ROWS]
def color(rgb: Rgb, alpha: float = 1.0) -> DrawingColor:
    """System color of a role at the alpha fraction."""
    return DrawingColor.FromArgb(round(alpha * 255), *rgb)


def hex_color(rgb: Rgb) -> str:
    """A role as `#RRGGBB`."""
    return "#{:02X}{:02X}{:02X}".format(*rgb)


def token(value: object, rgb: Callable[[Rgb], str]) -> str:
    """A value in a resource file's spelling: an alpha fraction as a percentage, an enum member as its number, a color in the given spelling, and any other value as text."""
    match value:
        case float():
            return f"{round(value * 100)}%"
        case System.Enum():
            return str(int(value))
        case tuple():
            return rgb(value)
        case _:
            return str(value)


def render(template: str, values: Mapping[str, object], rgb: Callable[[Rgb], str]) -> str:
    """The template with each placeholder the run-time value or theme export its dotted path names, in the resource file's spelling."""
    text = Placeholders(template)
    names = SimpleNamespace(**values, **{name: getattr(theme, name) for name in theme.__all__})
    return text.substitute({name: token(reduce(getattr, name.split("."), names), rgb) for name in text.get_identifiers()})


def internal(name: str, value: str) -> object:
    """The named member of an enum Rhino declares internal, its type read by assembly-qualified name."""
    return System.Enum.Parse(System.Type.GetType(name, throwOnError=True), value)


def key(child: Rhino.PersistentSettings, name: str, *, target: bool | int | str | Guid | tuple[str, ...] | DrawingColor, label: str = "key") -> Row:
    """Row of a settings key through the accessor pair the API names for the target's type."""
    match target:
        case bool():
            return (f"{label} {name}", lambda: found(child.TryGetBool(name)), partial(child.SetBool, name), target)
        case int():
            return (f"{label} {name}", lambda: found(child.TryGetInteger(name)), partial(child.SetInteger, name), target)
        case str():
            return (f"{label} {name}", lambda: found(child.TryGetString(name)), partial(child.SetString, name), target)
        case Guid():
            return (f"{label} {name}", lambda: found(child.TryGetGuid(name)), partial(child.SetGuid, name), target)
        case tuple():
            return (f"{label} {name}", lambda: found(child.TryGetStringList(name)), lambda value: child.SetStringList(name, Array[String](list(value))), target)
        case DrawingColor():
            return (f"{label} {name}", lambda: found(child.TryGetColor(name)), partial(child.SetColor, name), target)


def internal_setting(kind: object, name: str, *, target: object, instance: object = None) -> Row:
    """Row of a property of a type Rhino declares internal, labeled by the type, read through its getter and written through its setter as a typed delegate, static without an instance."""
    prop = kind.GetProperty(name)
    setter = prop.SetMethod.CreateDelegate(System.Type.GetType("System.Action`1", throwOnError=True).MakeGenericType(prop.PropertyType), instance)
    return (f"{kind.Name} {name}", partial(prop.GetValue, instance), setter, target)


def absent(child: Rhino.PersistentSettings, name: str, *, label: str) -> Row:
    """Row holding a settings key absent, deleted through its child when present."""
    return (f"{label} {name}", lambda: name in child.Keys, lambda _: child.DeleteItem(name), False)


def plain(value: object) -> object:
    """A value as Python compares it: a system color as its alpha, red, green, and blue bytes, an id as its text, and an array as a tuple."""
    match value:
        case DrawingColor():
            return (value.A, value.R, value.G, value.B)
        case Guid():
            return str(value)
        case System.Array():
            return tuple(value)
        case _:
            return value


def converged(row: Row) -> tuple[str, object, object, object]:
    """The row's label, its value before, its value after a write on difference, and its target."""
    label, read, write, target = row
    if (before := plain(read())) != plain(target):
        write(target)
    return (label, before, plain(read()), plain(target))


def observed(row: Row | Reading) -> tuple[str, object, object, object]:
    """The row's label, its value read twice, and its target."""
    label, read, *_, target = row
    value = plain(read())
    return (label, value, value, plain(target))


def lines(label: str, before: object, after: object, target: object) -> Iterable[tuple[str, object, object, object]]:
    """The report lines of a row, one per key when its target is a mapping."""
    match before, after, target:
        case Mapping() as held, Mapping() as now, Mapping() as wanted:
            return ((f"{label} {name}", held.get(name), now.get(name), value) for name, value in wanted.items())
        case _:
            return ((label, before, after, target),)


def emit(results: Iterable[tuple[str, object, object, object]], skipped: Iterable[str], measured: Mapping[Site | Extent, float]) -> None:
    """Print the `app` line, a `skip` line per absent plug-in or library, a `measure` line per extent, a `change` line per value a write changed, and an `error` line per value that reads other than its target, each value in its one-line `repr`."""
    print(f"app\t{Rhino.RhinoApp.Version}\t{Path(Rhino.RhinoApp.GetDataDirectory(localUser=True, forceDirectoryCreation=False)) / 'settings'}")
    for name in skipped:
        print(f"skip\t{name}")
    for name, value in measured.items():
        print(f"measure\t{name}\t{value!r}")
    for label, before, after, target in (line for result in results for line in lines(*result)):
        if after != before:
            print(f"change\t{label}\t{before!r}\t{after!r}")
        if after != target:
            print(f"error\t{label} holds {after!r} and needs {target!r}")


def rounded(value: float) -> float:
    """A length rounded past the noise a unit conversion leaves."""
    return round(value, 9)


# --- [SETTINGS]
def presentation() -> int | None:
    """Where command options present, as the command line's own callback reads it."""
    args = NamedParametersEventArgs()
    try:
        HostUtils.ExecuteNamedCallback("Rhino.UI.Internal.DockBars.CommandLine.GetPresentationStyle", args)
        return found(args.TryGetInt("mode"))
    finally:
        args.Dispose()


def present(mode: int) -> None:
    """Command options presented where the mode names, through the command line's own callback."""
    args = NamedParametersEventArgs()
    try:
        args.Set("mode", mode)
        HostUtils.ExecuteNamedCallback("Rhino.UI.Internal.DockBars.CommandLine.SetPresentationStyle", args)
    finally:
        args.Dispose()


def prompt_font(family: str) -> None:
    """The command prompt font family, set through the appearance state the native owner reads."""
    state = Settings.AppearanceSettings.GetCurrentState()
    state.CommandPromptFontName = family
    Settings.AppearanceSettings.UpdateFromState(state)


def settings_rows(options: Rhino.PersistentSettings) -> tuple[Row, ...]:
    """Navigation, selection, dialogs, undo, input devices, language, prompt, history echo, gumball, SmartTrack, snaps, nudges, strips, docked containers, notes, crash reports, and update checks."""
    root = Rhino.PersistentSettings.RhinoAppSettings
    general, advanced, appearance, mouse = (options.AddChild(name) for name in ("General", "Advanced", "Appearance", "Mouse"))
    scale = Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Meters, unit_system(Units.IMPERIAL.length))
    defaults, monitor = NSUserDefaults.StandardUserDefaults, NSUserDefaults("com.mcneel.rhinoceros.RhinoMonitor", NSUserDefaultsType.SuiteName)
    english, languages = CultureInfo(1033), "AppleLanguages"
    osnaps = Settings.OsnapModes
    tab_panels, notes = (System.Type.GetType(name, throwOnError=True) for name in ("Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI", "Rhino.UI.Runtime.Settings, Rhino.UI"))
    toolbar = System.Type.GetType("Rhino.UI.Internal.TabPanels.ToolbarSettings, Rhino.UI", throwOnError=True).GetProperty("Instance").GetValue(None)
    buttons, tooltips = (toolbar.GetType().GetProperty(name).GetValue(toolbar) for name in ("Buttons", "ToolTips"))
    tab_style = partial(internal, "Rhino.UI.Internal.TabPanels.TabControlDisplayStyle, Rhino.UI")
    icon_size = 16
    icon_buttons = int(internal("Rhino.UI.DialogPanels.OSnapPanel+OSnapButtonDisplay, Rhino.UI", "IconOnly"))
    return (
        member("view", Settings.ViewSettings, "RotateViewAroundObjectAtMouseCursor", target=True),
        member("view", Settings.ViewSettings, "RotateViewAroundAutogumball", target=False),
        member("view", Settings.ViewSettings, "AlwaysPanParallelViews", target=True),
        member("view", Settings.ViewSettings, "PanPlanParallelViewsWithControlShiftRMB", target=True),
        member("view", Settings.ViewSettings, "ZoomScale", target=0.913),
        member("view", Settings.ViewSettings, "AutoAdjustTargetDepth", target=True),
        member("view", Settings.ViewSettings, "ViewRotation", target=Settings.ViewSettings.ViewRotationStyle.RotateAroundWorldAxes),
        member("view", Settings.ViewSettings, "SingleClickMaximize", target=False),
        member("view", Settings.ViewSettings, "LinkedViewports", target=False),
        member("general", Settings.GeneralSettings, "MiddleMouseMode", target=Settings.MiddleMouseMode.PopupToolbar),
        member("general", Settings.GeneralSettings, "MiddleMousePopupToolbar", target="Popup"),
        member("general", Settings.GeneralSettings, "MouseSelectMode", target=Settings.MouseSelectMode.Combo),
        member("general", Settings.GeneralSettings, "MinimumUndoSteps", target=100),
        member("general", Settings.GeneralSettings, "MaximumUndoMemoryMb", target=4096),
        key(general, "MiddleMousePlainButtonRotateMode", target=True),
        key(general, "MiddleMouseViewManipulationMode", target=True),
        key(general, "MiddleMouseShiftControlSwap", target=False),
        key(general, "EnableTrackpadScrolling", target=True),
        key(general, "EnableContextMenu", target=True),
        key(general, "ContextMenuDelayInMillisecond", target=300),
        key(general, "UsageStatisticsEnabled", target=False),
        key(advanced, "EnableCheckForUpdates", target=False),
        key(advanced, "DisableModelAndPageUnitsDifferDialog", target=True),
        key(advanced, "DisablePageUnitsNotInchesOrMMDialog", target=True),
        key(advanced, "MacDisplayOldVersionAutosaveWarning", target=False),
        key(advanced, "DisplayNonOriginModelBasepointWarning", target=False),
        key(advanced, "UseCompressionWhenSaving", target=False),
        key(advanced, "UseEtoCommandUI", target=True),
        key(advanced, "AllowUnadornedShortcuts", target=False),
        key(advanced, "NotesUseSpacesForTabs", target=False),
        key(advanced, "NotesTabWidth", target=8),
        key(root.AddChild("Warnings"), "MissingFontWarning", target=False),
        key(options.AddChild("PackageManager"), "CheckForUpdates", target=False),
        key(options.AddChild("FileSettings"), "AutoSaveVersionsEnabled", target=True),
        key(mouse, "EnableUnselectedObjectDrag", target=False),
        key(mouse, "EnableUnselectedGripDrag", target=True),
        key(mouse, "EnableMouseScrollBallRotation", target=False),
        key(mouse, "EnableMagicMouseGestures", target=True),
        key(mouse, "EnableMagicMouseRotation", target=False),
        key(mouse, "DisableRightClickAsEnter", target=False),
        key(mouse, "MouseButton4Macro", target="'_Zoom _Selected"),
        member("appearance", Settings.AppearanceSettings, "LanguageIdentifier", target=english.LCID),
        member("appearance", Settings.AppearanceSettings, "HelpLanguageIdentifier", target=0),
        member("appearance", Settings.AppearanceSettings, "EchoCommandsToHistoryWindow", target=True),
        member("appearance", Settings.AppearanceSettings, "EchoPromptsToHistoryWindow", target=True),
        member("appearance", Settings.AppearanceSettings, "ShowViewportTitles", target=True),
        member("appearance", Settings.AppearanceSettings, "ShowCrosshairs", target=True),
        member("appearance", Settings.AppearanceSettings, "ShowOsnapBar", target=True),
        member("appearance", Settings.AppearanceSettings, "ShowSelectionFilterBar", target=False),
        member("appearance", Settings.AppearanceSettings, "ShowLayoutDropShadow", target=False),
        member("appearance", Settings.AppearanceSettings, "CommandPromptFontSize", target=110),
        ("appearance CommandPromptFontName", lambda: found(appearance.TryGetString("CommandPromptFontName")), prompt_font, Typography.INTERFACE.family),
        key(appearance, "AutocompleteCommands", target=True),
        key(appearance, "FuzzyAutocomplete", target=True),
        key(appearance, "ShowStatusbar", target=True),
        key(appearance, "StatusbarInfoPaneMode", target=int(internal("Rhino.UI.Internal.TabPanels.Controls.StatusBarInfoPaneMode, Rhino.UI", "selected_object_count"))),
        key(appearance, "AlwaysShowGeneralObjectProperties", target=True),
        key(appearance, "ShowSideBar", target=True),
        key(appearance, "DirectionArrowThickness", target=2),
        ("command options presentation", presentation, present, int(internal("Rhino.UI.CommandPromptLocation, RhinoCommon", "SideBar"))),
        key(options, "CommandPromptStyle", target=int(internal("Rhino.UI.CommandPromptStyle, RhinoCommon", "Graphical"))),
        absent(options, "CommandPromptLocation", label="key"),
        member("file", Settings.FileSettings, "ClipboardOnExit", target=Settings.ClipboardState.DeleteData),
        member("file", Settings.FileSettings, "FileLockingOpenWarning", target=False),
        member("file", Settings.FileSettings, "CreateOtherBackupFiles", target=False),
        member("file", Settings.FileSettings, "SaveViewChanges", target=False),
        member("gumball", Settings.GumballSettings, "EnableGumball", target=True),
        member("gumball", Settings.GumballSettings, "SnappyGumball", target=True),
        member("gumball", Settings.GumballSettings, "MergeFacesAfterExtrude", target=True),
        *(member("gumball", Settings.GumballSettings, name, target=2) for name in ("AxisThickness", "ArcThickness")),
        member("smarttrack", Settings.SmartTrackSettings, "UseSmartTrack", target=True),
        member("smarttrack", Settings.SmartTrackSettings, "SmartOrtho", target=True),
        member("smarttrack", Settings.SmartTrackSettings, "Parallels", target=True),
        key(options.AddChild("SmartTrack"), "MaximumSmartPoints", target=4),
        member("modelaid", Settings.ModelAidSettings, "Osnap", target=True),
        member("modelaid", Settings.ModelAidSettings, "OsnapModes", target=osnaps.End | osnaps.Point | osnaps.Midpoint | osnaps.Center | osnaps.Intersection | osnaps.Perpendicular | osnaps.Quadrant),
        member("modelaid", Settings.ModelAidSettings, "Ortho", target=True),
        member("modelaid", Settings.ModelAidSettings, "GridSnap", target=False),
        member("modelaid", Settings.ModelAidSettings, "Planar", target=False),
        member("modelaid", Settings.ModelAidSettings, "ProjectToCPlaneInPlanParallelViews", target=True),
        member("modelaid", Settings.ModelAidSettings, "ProjectSnapToCPlane", target=False),
        member("modelaid", Settings.ModelAidSettings, "SnapToFiltered", target=False),
        member("modelaid", Settings.ModelAidSettings, "ExtendToApparentIntersection", target=False),
        member("modelaid", Settings.ModelAidSettings, "DragStartsWindowSelection", target=True),
        member("modelaid", Settings.ModelAidSettings, "AltPlusArrow", target=True),
        member("modelaid", Settings.ModelAidSettings, "NudgeMode", target=1),
        *(
            (f"modelaid {name}", lambda name=name: rounded(getattr(Settings.ModelAidSettings, name)), partial(setattr, Settings.ModelAidSettings, name), rounded(length * scale))
            for name, length in zip(("NudgeKeyStep", "CtrlNudgeKeyStep", "ShiftNudgeKeyStep"), NUDGE, strict=True)
        ),
        member("chooseone", Settings.ChooseOneObjectSettings, "ShowObjectLayer", target=True),
        member("chooseone", Settings.ChooseOneObjectSettings, "ShowObjectTypeDetails", target=True),
        member("chooseone", Settings.ChooseOneObjectSettings, "ShowAllOption", target=True),
        member("chooseone", Settings.ChooseOneObjectSettings, "ShowTitlebarAndBorder", target=False),
        member("selectionfilter", Settings.SelectionFilterSettings, "GlobalGeometryFilter", target=ObjectType.AnyObject),
        member("selectionfilter", Settings.SelectionFilterSettings, "Enabled", target=Settings.SelectionFilterSettings.GetDefaultState().Enabled),
        member("tooltip", Settings.CursorTooltipSettings, "TooltipsEnabled", target=True),
        member("tooltip", Settings.CursorTooltipSettings, "DistancePane", target=True),
        key(options.AddChild("Grid"), "AxisLineWidth", target=1),
        *(
            internal_setting(tab_panels, name, target=target)
            for name, target in (
                ("LockDockedWindows", True),
                ("TabIconSize", icon_size),
                ("ToolBarImageSize", window.BUTTON_SIZE),
                ("HorizontalDisplayStyle", tab_style("Text")),
                ("VerticalDisplayStyle", tab_style("Bitmap")),
                ("FloatingDisplayStyle", tab_style("Bitmap")),
                ("HideSingleToolBarTab", True),
                ("CascadeDelay", 400),
                ("ForceVerticalToTop", False),
                ("UseIconsInStatusBar", False),
            )
        ),
        *(
            internal_setting(buttons.GetType(), name, target=target, instance=buttons)
            for name, target in (
                ("PanelButtonSize", icon_size),
                ("ButtonPadding", window.BUTTON_PADDING),
                ("SpacerSize", 5),
                ("Cascade", internal("Rhino.UI.Internal.TabPanels.CascadeStyle, Rhino.UI", "AsPanel")),
                ("MiddleMouseDelay", 400),
            )
        ),
        *(internal_setting(tooltips.GetType(), name, target=True, instance=tooltips) for name in ("IncludeShortcut", "IncludeAlias")),
        key(root, "OSnapButtonDisplay", target=icon_buttons),
        key(root, "OSnapIconSize", target=icon_size),
        key(root, "OSnapStretchButtons", target=True),
        key(root, "SelectionFilterButtonDisplay", target=int(internal("Rhino.UI.DialogPanels.SelectionFilterUi+ButtonDisplay, Rhino.UI", "IconOnly"))),
        key(root, "SelectionFilterIconSize", target=icon_size),
        key(root, "SelectionFilterUseCheckedColor", target=True),
        key(root, "SelectionFilterStretchButtons", target=True),
        key(root, "AnnotationSpellCheck", target=False),
        key(root.AddChild("PropertiesEditor").AddChild("Options"), "DisplayPagesOnIdle", target=True),
        internal_setting(notes, "NotesRestoreCursorPosition", target=False),
        *(
            (f"appkit {name}", partial(read, name), lambda value, write=write, name=name: write(value, name), target)
            for read, write, name, target in (
                (defaults.BoolForKey, defaults.SetBool, "AppleReduceDesktopTinting", True),
                (defaults.StringForKey, defaults.SetString, "MRLanguage", english.Parent.Name),
                (monitor.BoolForKey, monitor.SetBool, "MRShouldIncludeModelFileInReport", False),
            )
        ),
        (
            f"appkit {languages}",
            partial(defaults.StringArrayForKey, languages),
            lambda value: defaults.SetValueForKey(NSArray.FromStrings(Array[String](list(value))), NSString(languages)),
            (english.Parent.Name,),
        ),
    )


# --- [PANELS]
def panel_rows() -> tuple[Row, ...]:
    """List views and restore memory of the panels, the material preview checker, the block preview in the declared mode, and each right-hand panel open where the host's file stage orders it."""
    rdk, eto_panels, commands = (Rhino.PersistentSettings.FromPlugInId(PlugIn.IdFromName(name)) for name in ("Renderer Development Kit", "RDK_EtoUI", "Commands"))
    settings = rdk.AddChild("Settings")
    support = settings.AddChild("RendererSupport")
    checkers = {"LightPreviewCheckerColor": Surface.BOX, "DarkPreviewCheckerColor": Surface.PANEL}
    layer_states = commands.AddChild("LayerStates")
    restored = RestoreLayerProperties.Visible | RestoreLayerProperties.Locked | RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.NewDetailOn
    preview = Rhino.PersistentSettings.RhinoAppSettings.AddChild("ObjectManager").AddChild("Preview")
    panels = Rhino.UI.Panels
    head, *_ = window.RIGHT_TOP
    container = panels.PanelDockBar(Guid(str(head)))
    return (
        key(settings, "LibrariesViewMode", target=1, label="libraries"),
        key(settings, "LibrariesListSizePercentage", target=0, label="libraries"),
        *(
            (f"rdk {name}", lambda name=name: found(support.TryGetUnsignedInteger(name)), partial(support.SetUnsignedInteger, name), ColorTranslator.ToWin32(color(rgb)))
            for name, rgb in checkers.items()
        ),
        *(key(eto_panels.AddChild(panel), "ViewMode", target=1, label=panel) for panel in ("BlockContent", "FileExplorer")),
        ("libraries Libraries_ShowDocuments", SupportOptions.Libraries_ShowDocuments, SupportOptions.Libraries_SetShowDocuments, False),
        ("block content BlockContent_ShowDocuments", SupportOptions.BlockContent_ShowDocuments, SupportOptions.BlockContent_SetShowDocuments, False),
        (
            "layer states RestoreLayerProperties",
            lambda: found(layer_states.TryGetUnsignedInteger("RestoreLayerProperties")),
            partial(layer_states.SetUnsignedInteger, "RestoreLayerProperties"),
            int(restored),
        ),
        key(layer_states, "ModelPropertiesChecked", target=True, label="layer states"),
        key(layer_states, "ViewportPropertiesChecked", target=True, label="layer states"),
        key(preview, "DisplayModeId", target=Guid(str(window.MODE_ID)), label="block preview"),
        *(
            (
                f"panel {panel.name} open",
                lambda panel=panel: panels.PanelDockBar(Guid(str(panel))) != Guid.Empty,
                lambda _, panel=panel: panels.OpenPanel(container, Guid(str(panel)), makeSelectedPanel=False),
                True,
            )
            for panel in (*window.RIGHT_TOP, *window.RIGHT_BOTTOM)
        ),
    )


# --- [COLORS]
def theme_rows() -> tuple[Row, ...]:
    """Every theme key a Rhino control draws, in the role its meaning takes, each zone's grounded keys on its ground."""
    theme_settings = Rhino.PersistentSettings.RhinoAppSettings.AddChild("UI").AddChild("ThemeSettings")
    roles = {
        "Text.Disabled": Text.DISABLED,
        "Button.Enabled.Background": Surface.FIELD,
        "Button.Enabled.Border": Line.BORDER,
        "Button.Enabled.Text": Text.PRIMARY,
        "Button.EnabledHover.Background": Surface.HOVER,
        "Button.EnabledHover.Text": Text.PRIMARY,
        "Button.EnabledPressed.Border": Surface.FIELD,
        "Button.Checked.Background": Accent.CONTROL_PRESSED,
        "Button.Checked.Text": Text.PRIMARY,
        "Button.CheckedHover.Background": Accent.CONTROL_PRESSED,
        "Button.CheckedPressed.Background": Accent.CONTROL_PRESSED,
        "Button.Disabled.Background": Surface.PANEL,
        "Button.Disabled.Text": Text.DISABLED,
    }
    zones = {
        "Frame": (
            Surface.FRAME,
            ("Background", "Edge"),
            {
                "Text.Secondary": Text.SECONDARY,
                "GripperDot": Line.GRID,
                "Highlight": Accent.CONTROL_PRESSED,
                "Button.EnabledHover.Border": Surface.HOVER,
                "Tab.EnabledHover.Background": Surface.WELL,
                "Tab.EnabledHover.Text": Text.PRIMARY,
                "Tab.Checked.Background": Surface.PANEL,
                "Tab.Checked.Text": Text.PRIMARY,
                "Tab.Unchecked.Text": Text.SECONDARY,
                "List.Enabled.Background": Surface.FIELD,
            },
        ),
        "Content": (
            Surface.PANEL,
            ("Background", "Tab.Enabled.Border"),
            {
                "Text.Enabled": Text.PRIMARY,
                "Text.Highlight": Text.PRIMARY,
                "Highlight": Surface.HOVER,
                "HighlightHover": Surface.HOVER,
                "Button.EnabledHover.Border": Line.BORDER_HOVER,
                "Button.CheckedPressed.Background": Accent.CONTROL_PRESSED_DISABLED,
                "Entry.Enabled.Background": Surface.FIELD,
                "Entry.Enabled.Border": Line.BORDER,
                "Entry.Enabled.Text": Text.PRIMARY,
                "Entry.Disabled.Text": Text.DISABLED,
                "List.Checked.Background": Accent.ROW_SELECTED,
                "List.Enabled.Background": Surface.WELL,
                "List.Enabled.Text": Text.PRIMARY,
                "List.Disabled.Text": Text.DISABLED,
            },
        ),
    }
    clear = {"Button.Disabled.Background"}
    return tuple(
        key(theme_settings, f"{zone}.{suffix}", target=color(rgb, 0.0 if suffix in clear else 1.0), label="theme")
        for zone, (ground, grounded, overrides) in zones.items()
        for suffix, rgb in {**roles, **dict.fromkeys(grounded, ground), **overrides}.items()
    )


def color_rows(options: Rhino.PersistentSettings, ui_settings: Rhino.PersistentSettings) -> tuple[Row, ...]:
    """The color roles on the canvas, grid, axes, selection, feedback, command prompt, SmartTrack, choose-one highlight, gumball, widgets, analysis, filter strip, and color picker swatches."""
    appearance, gumball = Settings.AppearanceSettings, Settings.GumballSettings
    root = Rhino.PersistentSettings.RhinoAppSettings
    general, arrow_owner = options.AddChild("General"), options.AddChild("Appearance")
    axes = tuple(color(axis) for axis in (Axis.X, Axis.Y, Axis.Z))
    fill = color(Selection.ITEM, SELECTION_FILL_ALPHA)
    triads = (
        (appearance, ("GridXAxisLineColor", "GridYAxisLineColor", "GridZAxisLineColor")),
        (appearance, ("WorldCoordIconXAxisColor", "WorldCoordIconYAxisColor", "WorldCoordIconZAxisColor")),
        (gumball, ("XAxisColor", "YAxisColor", "ZAxisColor")),
    )
    widgets = System.Enum.GetValues(clr.GetClrType(Settings.WidgetColor))
    arrows = ("DirectionArrowColorU", "DirectionArrowColorV", "DirectionArrowColorW")
    members = (
        (
            "appearance",
            appearance,
            {
                "ViewportBackgroundColor": color(Surface.CANVAS),
                "PageviewPaperColor": color(Surface.CANVAS),
                "GridThickLineColor": color(blend(Line.GRID, Surface.CANVAS, GRID_MAJOR_ALPHA)),
                "GridThinLineColor": color(blend(Line.GRID, Surface.CANVAS, GRID_MINOR_ALPHA)),
                "LockedObjectColor": color(Line.LOCKED),
                "SelectedObjectColor": color(Selection.ITEM),
                "EditCandidateColor": color(Selection.HOVER),
                "SelectionWindowStrokeColor": color(Selection.ITEM),
                "SelectionWindowFillColor": fill,
                "SelectionWindowCrossingStrokeColor": color(Selection.ITEM),
                "SelectionWindowCrossingFillColor": fill,
                "FeedbackColor": color(Line.GEOMETRY),
                "TrackingColor": color(Guide.TRACKING),
                "CrosshairColor": color(Guide.HANDLE),
                "DefaultLayerColor": color(Line.GEOMETRY),
                "DefaultObjectColor": color(Line.GEOMETRY),
                "CommandPromptBackgroundColor": color(Surface.WELL),
                "CommandPromptTextColor": color(Text.PRIMARY),
                "CommandPromptHypertextColor": color(Accent.ITEM_HOVER),
                "BlackWhiteSwitching": False,
            },
        ),
        (
            "smarttrack",
            Settings.SmartTrackSettings,
            {
                "LineColor": color(Guide.TRACKING),
                "GuideColor": color(Guide.CONSTRUCTION),
                "TanPerpLineColor": color(Guide.TRACKING),
                "PointColor": color(Guide.TRACKING),
                "ActivePointColor": color(Guide.TRACKING_ACTIVE),
            },
        ),
        ("chooseone", Settings.ChooseOneObjectSettings, {"HighlightColor": color(Selection.HOVER), "UseCustomColor": True}),
        ("gumball", gumball, {"MenuBallColor": color(Text.PRIMARY)}),
        ("curvaturegraph", Settings.CurvatureGraphSettings, {"CurveHairColor": color(Guide.CONSTRUCTION), "SurfaceUHairColor": color(Axis.X), "SurfaceVHairColor": color(Axis.Y)}),
        ("directionanalysis", Settings.DirectionAnalysisSettings, {"Color": color(Guide.CONSTRUCTION)}),
        ("edgeanalysis", Settings.EdgeAnalysisSettings, {"ShowEdgeColor": color(Status.ERROR)}),
        ("zebraanalysis", Settings.ZebraAnalysisSettings, {"StripeColor": color(Line.GEOMETRY)}),
    )
    swatches = (hex_color(Line.GEOMETRY), *(hex_color(tag.solid) for tag in Tag))
    return (
        *(member(label, owner, name, target=target) for label, owner, targets in members for name, target in targets.items()),
        *(member("axis", owner, name, target=axis) for owner, names in triads for name, axis in zip(names, axes, strict=True)),
        *((f"widget {widget}", partial(appearance.GetWidgetColor, widget), partial(appearance.SetWidgetColor, widget), axis) for widget, axis in zip(widgets, axes, strict=True)),
        *(key(arrow_owner, name, target=axis) for name, axis in zip(arrows, axes, strict=True)),
        key(root.AddChild("SoftTransformSettings"), "FalloffColor", target=color(Guide.TRACKING)),
        *(key(general, name, target=color(rgb)) for name, rgb in (("HiddenLineColor", Line.GEOMETRY), ("SelectedObjectCornerColor", Selection.ITEM))),
        key(root, "SelectionFilterCheckedColorDark", target=color(Accent.CONTROL_PRESSED)),
        key(ui_settings, "ColorPanelSwatches", target=swatches),
    )


# --- [COMMANDS]
def alias_table() -> dict[str, str]:
    """Rhino's factory aliases overlaid with the declared alias file, names upper-cased and macros case-folded as Rhino compares them."""
    declared = (line.partition(" ") for line in Path(__file__).with_name("aliases.txt").read_text(encoding="utf-8").splitlines() if line)
    return {**{pair.Key.upper(): pair.Value for pair in Settings.CommandAliasList.GetDefaults()}, **{name.upper(): macro for name, _, macro in declared}}


def write_aliases(aliases: Mapping[str, str]) -> None:
    """Replace Rhino's whole alias list with the table, none of them instant."""
    Settings.CommandAliasList.Update(List[Settings.CommandAlias]([Settings.CommandAlias(name, macro, instant=False) for name, macro in aliases.items()]), replaceAll=True)


def folded(aliases: Mapping[str, str]) -> dict[str, str]:
    """The table with macros case-folded."""
    return {name: macro.casefold() for name, macro in aliases.items()}


def registry_child(plugin: Guid) -> Rhino.PersistentSettings | None:
    """The plug-in registry record of the plug-in under the registry version that holds it, none when no version holds one."""
    registry, record = Rhino.PersistentSettings.RhinoAppSettings.AddChild("PlugInRegistry"), str(plugin)
    return next((held for version in registry.ChildKeys if (held := found(registry.AddChild(version).TryGetChild(record))) is not None), None)


def plugin_loads() -> tuple[tuple[str, PlugInLoadTime, Rhino.PersistentSettings | None], ...]:
    """Each declared plug-in with its load time and its registry record, none where the plug-in is not installed."""
    on_demand = PlugInLoadTime.WhenNeeded
    return tuple(
        (name, mode, registry_child(PlugIn.IdFromName(name)))
        for name, mode in ((f"3DxRhino.{Rhino.RhinoApp.ExeVersion}", on_demand), ("PanelingTools", on_demand), ("RhinoAI", PlugInLoadTime.AtStartup))
    )


def command_rows() -> tuple[Row, ...]:
    """The aliases as a whole set, the shortcuts, installed plug-ins' load modes, RhinoAI's agents, alerts off, Cycles on the automatic device, and Rhino Render current."""
    table = alias_table()
    shortcuts = Settings.ShortcutKeySettings
    clr.AddReference("RhinoCyclesCore")
    cycles = import_module("RhinoCyclesCore.Core").RcCore.It.AllSettings
    alerter = import_module("Commands.Commands.Alerter").AlerterCommand.Instance
    loads = tuple((name, mode, record) for name, mode, record in plugin_loads() if record is not None)
    shortcut_macros = (
        (Settings.ShortcutKey.Ctrl7, window.mode_macro(Scope.ACTIVE)),
        (Settings.ShortcutKey.F3, "! _Properties"),
        *((getattr(Settings.ShortcutKey, f"CtrlF{index}"), f"'_SetMaximizedViewport {view}") for index, view in enumerate(("Top", "Front", "Right", "Perspective"), start=1)),
    )
    return (
        ("aliases", lambda: folded({pair.Key.upper(): pair.Value for pair in Settings.CommandAliasList.ToDictionary()}), lambda _: write_aliases(table), folded(table)),
        *((f"shortcut {each}", partial(shortcuts.GetMacro, each), partial(shortcuts.SetMacro, each), macro) for each, macro in shortcut_macros),
        *(key(record, "LoadMode", target=int(mode), label=f"plugin {name}") for name, mode, record in loads),
        *(absent(record, "LoadProtection", label=f"plugin {name}") for name, mode, record in loads if mode == PlugInLoadTime.AtStartup),
        internal_setting(System.Type.GetType("Rhino.AI.AISettings, RhinoAI", throwOnError=True), "DisabledAgents", target=()),
        member("alerter", alerter, "Enabled", target=False),
        *(member("cycles", cycles, name, target=target) for name, target in (("ThrottleMs", 100), ("SelectedDeviceStr", "-1"), ("IntermediateSelectedDeviceStr", "-1"), ("PixelSize", 1))),
        ("render DefaultRenderPlugInId", lambda: Rhino.Render.Utilities.DefaultRenderPlugInId, Rhino.Render.Utilities.SetDefaultRenderPlugIn, PlugIn.IdFromName("Rhino Render")),
    )


# --- [DISPLAY]
def mode_description() -> DisplayModeDescription:
    """The display mode Rhino holds under the id the INI declares, None before the first import."""
    return DisplayModeDescription.GetDisplayMode(Guid(str(window.MODE_ID)))


def mode_keys(text: str) -> dict[str, str]:
    """Every key of a display-mode INI as its section below the mode and its lower-cased name, the bare `=` line of an empty texture section read as a comment."""
    parser = ConfigParser(interpolation=None, comment_prefixes=("=",))
    parser.read_string(text)
    return {f"{'\\'.join(section.split('\\')[2:])} {name}".strip(): value for section in parser.sections() for name, value in parser.items(section)}


def held_mode(keys: Iterable[str]) -> dict[str, str]:
    """The stated keys of the declared display mode as Rhino exports it, none while the mode is absent."""
    with TemporaryDirectory() as folder:
        path = Path(folder, "held.ini")
        mode = mode_description()
        held = mode_keys(path.read_text(encoding="utf-8-sig")) if mode is not None and DisplayModeDescription.ExportToFile(mode, str(path)) else {}
    return {name: held[name] for name in keys if name in held}


def import_mode(text: str) -> None:
    """The display mode deleted and imported fresh from the rendered INI under its declared id."""
    if (mode := mode_description()) is not None:
        DisplayModeDescription.DeleteDisplayMode(mode.Id)
    with TemporaryDirectory() as folder:
        (source := Path(folder, window.DISPLAY_MODE_FILE.name)).write_text(text, encoding="utf-8")
        DisplayModeDescription.ImportFromFile(str(source))


def in_menu(mode: Guid, *, shown: bool) -> None:
    """The mode listed in or left out of the display menus and the Display panel dropdown."""
    description = DisplayModeDescription.GetDisplayMode(mode)
    description.InMenu = shown
    DisplayModeDescription.UpdateDisplayMode(description)


def display_rows() -> tuple[Row, ...]:
    """The declared display mode, each key the INI states compared with Rhino's export and the mode imported on a difference, and the menu listing of every built-in mode."""
    technical = partial(internal, "Rhino.Display.DisplayPipelineAttributes+TechnicalModeParameter, RhinoCommon")
    values = {clr.GetClrType(each).Name: each for each in (Rhino.Display.PointStyle, Rhino.Display.DisplayPipelineAttributes.ClippingPlaneFillColorUse)} | {
        "TechnicalMask": sum(int(technical(name)) for name in ("TECH_EDGES", "TECH_SILHOUETTES", "TECH_CREASES", "TECH_INTERSECTIONS"))
    }
    text = render(window.DISPLAY_MODE_FILE.read_text(encoding="utf-8"), values, lambda rgb: ",".join(map(str, rgb)))
    wanted = mode_keys(text)
    modes = DisplayModeDescription
    shown = (Guid(str(window.MODE_ID)), modes.WireframeId, modes.XRayId, modes.RenderedId, modes.RaytracedId)
    hidden = (modes.ShadedId, modes.GhostedId, modes.TechId, modes.ArtisticId, modes.PenId, modes.MonochromeId, modes.AmbientOcclusionId, Guid("881f20dd-a78e-4930-a7c3-690e5e6b0927"))
    return (
        ("display mode", partial(held_mode, tuple(wanted)), lambda _: import_mode(text), wanted),
        *(
            (f"display mode {mode} in menu", lambda mode=mode: DisplayModeDescription.GetDisplayMode(mode).InMenu, lambda value, mode=mode: in_menu(mode, shown=value), listed)
            for listed, ids in ((True, shown), (False, hidden))
            for mode in ids
        ),
    )


# --- [TEMPLATE]
def unit_system(unit: str) -> Rhino.UnitSystem:
    """The unit system a units token names."""
    return System.Enum.Parse(clr.GetClrType(Rhino.UnitSystem), unit, ignoreCase=True)


def distance_display(system: Rhino.UnitSystem, *, page: bool) -> Rhino.UI.DistanceDisplayMode:
    """Distance display of a unit system: feet and inches for a feet model, fractions for an inch page, decimals otherwise."""
    match system, page:
        case Rhino.UnitSystem.Feet, False:
            return Rhino.UI.DistanceDisplayMode.FeetInches
        case Rhino.UnitSystem.Inches, True:
            return Rhino.UI.DistanceDisplayMode.Fractional
        case _:
            return Rhino.UI.DistanceDisplayMode.Decimal


def length_display(system: Rhino.UnitSystem) -> DimensionStyle.LengthDisplay:
    """Dimension length display of a unit system: feet and inches for feet, millimeters for millimeters, the model's unit otherwise."""
    match system:
        case Rhino.UnitSystem.Feet:
            return DimensionStyle.LengthDisplay.FeetAndInches
        case Rhino.UnitSystem.Millimeters:
            return DimensionStyle.LengthDisplay.Millmeters
        case _:
            return DimensionStyle.LengthDisplay.ModelUnits


def precision(units: Units, system: Rhino.UnitSystem, mode: Rhino.UI.DistanceDisplayMode) -> int:
    """Display precision reaching the declared resolution: binary digits of an inch for fractions, decimal digits of the unit otherwise."""
    decimal = mode == Rhino.UI.DistanceDisplayMode.Decimal
    unit = 1 / Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Meters, system) if decimal else INCH
    return round(math.log(unit / units.resolution, 10 if decimal else 2))


def new_hatches() -> tuple[HatchPattern, ...]:
    """Rhino's current hatch pattern set without the system patterns `HatchPattern.Defaults` names, Solid kept."""
    system = frozenset(str(prop.GetValue(None).Id) for prop in clr.GetClrType(HatchPattern.Defaults).GetProperties()) - {str(HatchPattern.Defaults.Solid.Id)}
    return tuple(pattern for pattern in HatchPattern.GetDefaultHatchPatterns() if str(pattern.Id) not in system)


def segments(linetype: Linetype) -> tuple[float, ...]:
    """Segment lengths of a linetype in millimeters, each gap negative."""
    return tuple(length if solid else -length for length, solid in map(linetype.GetSegment, range(linetype.SegmentCount)))


def default_linetypes(scale: float) -> dict[str, tuple[float, ...]]:
    """Segment lengths at the scale by name of each linetype Rhino loads on demand, read from a document the call creates and disposes."""
    doc = Rhino.RhinoDoc.CreateHeadless(None)
    try:
        doc.Linetypes.LoadDefaultLinetypes()
        return {linetype.Name: tuple(rounded(length * scale) for length in segments(linetype)) for linetype in doc.Linetypes}
    finally:
        doc.Dispose()


def cut_style() -> SectionStyle:
    """The cut section style: the section surface as a solid fill on solid objects, and its boundary at the cut pen weight."""
    style = SectionStyle()
    style.Name, style.BoundaryWidthScale, style.BoundaryPlotWeightMillimeters = CUT_STYLE, 1.0, 0.35
    style.BackgroundFillMode, style.SectionFillRule = SectionBackgroundFillMode.SolidColor, ObjectSectionFillRule.SolidObjects
    style.BackgroundFillColor = style.BackgroundFillPrintColor = color(Surface.SECTION)
    return style


def section_facts(style: SectionStyle) -> tuple[object, ...]:
    """Name, boundary width scale and print weight, background fill mode, fill colors, and fill rule of a section style."""
    return (
        style.Name,
        style.BoundaryWidthScale,
        style.BoundaryPlotWeightMillimeters,
        str(style.BackgroundFillMode),
        style.BackgroundFillColor.ToArgb(),
        style.BackgroundFillPrintColor.ToArgb(),
        str(style.SectionFillRule),
    )


def section_styles(doc: Rhino.RhinoDoc) -> tuple[SectionStyle, ...]:
    """The document's section styles not deleted, read by table index."""
    return tuple(style for style in map(doc.SectionStyles.FindIndex, range(doc.SectionStyles.Count)) if not style.IsDeleted)


def saved_states(doc: Rhino.RhinoDoc) -> tuple[tuple[object, tuple[str, ...]], ...]:
    """Each saved-state table of the document, named views, construction planes, positions, and layer states, with the names it holds."""
    return (
        (doc.NamedViews, tuple(view.Name for view in doc.NamedViews)),
        (doc.NamedConstructionPlanes, tuple(plane.Name for plane in doc.NamedConstructionPlanes)),
        (doc.NamedPositions, tuple(doc.NamedPositions.Names)),
        (doc.NamedLayerStates, tuple(doc.NamedLayerStates.Names)),
    )


def site() -> tuple[tuple[float, float], tuple[float, float], float, bool, int, float]:
    """The model north and east axes, the time zone hours, the daylight saving switch and minutes, and the sun's north the location declaration gives a document."""
    north = math.radians(location.NORTH)
    sine, cosine = math.sin(north), math.cos(north)
    return (-sine, cosine), (cosine, sine), location.OFFSET / timedelta(hours=1), timedelta() < location.DAYLIGHT, location.DAYLIGHT // timedelta(minutes=1), 90 + location.NORTH


def template_target(measures: DocumentUnits) -> Facts:
    """Every template fact the units, location, render, section, and table declarations decide."""
    grid = (rounded(measures.grid), rounded(measures.snap), measures.lines, GRID_THICK_EVERY)
    geometry = color(Line.GEOMETRY).ToArgb()
    north, east, *sun = site()
    return {
        "model units": (str(measures.model), rounded(measures.tolerance), str(measures.model_display), measures.model_precision),
        "page units": ((page := str(measures.page)), str(measures.page_display), measures.page_precision),
        "views": ((str(window.MODE_ID), grid),),
        "cameras": ((True, (0.0, 0.0, 0.0)),),
        "grid defaults": grid,
        "dimension styles": 1,
        "dimension style": (measures.style, str(measures.dimension_display), measures.model_precision, str(measures.alternate_display), measures.alternate_precision, False, False),
        "render": (page, False, FRAME_SIZE, DPI, True, RENDER_KEYS),
        "render mesh": str(MeshingParameterStyle.Quality),
        "sun": (True, location.LATITUDE, location.LONGITUDE, *sun, location.MOMENT.replace(tzinfo=None).isoformat(timespec="minutes"), SUN_INTENSITY),
        "earth anchor": (location.LATITUDE, location.LONGITUDE, location.ELEVATION, tuple(map(rounded, north)), tuple(map(rounded, east))),
        "environment": (str(Rhino.Display.BackgroundStyle.Environment), (str(ContentUuids.PhysicalSkyTextureType), True, SKY_MULTIPLIER), 1, 1),
        "layers": ((geometry, geometry, CUT_STYLE),),
        "section styles": (section_facts(cut_style()),),
        "saved states": (),
        "hatch patterns": tuple(sorted(pattern.Name for pattern in new_hatches())),
        "linetypes": measures.linetypes,
    }


def sky(doc: Rhino.RhinoDoc) -> tuple[str, bool, float] | None:
    """Texture type, document-sun switch, and gain of the document's background environment, none without one."""
    settings = doc.RenderSettings
    environment = doc.RenderEnvironments.Find(settings.RenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentPurpose.Standard))
    texture = None if environment is None else environment.FindChild(SKY_SLOT)
    return None if texture is None else (str(texture.TypeId), System.Convert.ToBoolean(texture.GetParameter(SKY_SUN)), System.Convert.ToDouble(texture.GetParameter(SKY_GAIN)))


def file_sky(file: File3dm) -> tuple[str, tuple[str, bool, float] | None, int, int]:
    """Background style, the texture type, document-sun switch, and gain of the background environment, how many environments the usages name, and how many the file holds."""
    settings = file.Settings.RenderSettings
    ids = {usage: settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard) for usage in SKY_USAGES}
    background = ids[RenderSettings.EnvironmentUsage.Background]
    texture = next((child for environment in file.RenderEnvironments if environment.Id == background for child in environment.Children if child.ChildSlotName == SKY_SLOT), None)
    return (
        str(settings.BackgroundStyle),
        None if texture is None else (str(texture.TypeId), texture.GetParameter(SKY_SUN).ToBool(), System.Convert.ToDouble(texture.GetParameter(SKY_GAIN))),
        len(set(ids.values())),
        len(tuple(file.RenderEnvironments)),
    )


def template_facts(path: str) -> Facts:
    """Every template fact the declarations decide, read from the file and a headless document opened on it, none when the file is absent."""
    if not Path(path).exists():
        return {}
    file, doc = File3dm.Read(path), Rhino.RhinoDoc.CreateHeadless(path)
    try:
        grid_defaults = doc.GetGridDefaults()
        planes = tuple((view.ActiveViewport.DisplayMode.Id, view.ActiveViewport.GetConstructionPlane()) for view in doc.Views)
        style = doc.DimStyles.Current
        settings = doc.RenderSettings
        sun = settings.Sun
        anchor = doc.EarthAnchorPoint
        dictionary = settings.UserDictionary
        return {
            "model units": (str(doc.ModelUnitSystem), rounded(doc.ModelAbsoluteTolerance), str(doc.ModelDistanceDisplayMode), doc.ModelDistanceDisplayPrecision),
            "page units": (str(doc.PageUnitSystem), str(doc.PageDistanceDisplayMode), doc.PageDistanceDisplayPrecision),
            "views": tuple(sorted({(str(mode), (rounded(plane.GridSpacing), rounded(plane.SnapSpacing), plane.GridLineCount, plane.ThickLineFrequency)) for mode, plane in planes})),
            "cameras": tuple(
                sorted({
                    (view.Maximized == view.Viewport.IsPerspectiveProjection, (rounded(target.X), rounded(target.Y), rounded(target.Z)))
                    for view in file.Views
                    for target in (view.Viewport.TargetPoint,)
                })
            ),
            "grid defaults": (rounded(grid_defaults.GridSpacing), rounded(grid_defaults.SnapSpacing), grid_defaults.GridLineCount, grid_defaults.GridThickFrequency),
            "dimension styles": doc.DimStyles.Count,
            "dimension style": (
                style.Name,
                str(style.DimensionLengthDisplay),
                style.LengthResolution,
                str(style.AlternateDimensionLengthDisplay),
                style.AlternateLengthResolution,
                style.AlternateUnitsDisplay,
                style.DrawTextMask,
            ),
            "render": (
                str(settings.ImageUnitSystem),
                settings.UseViewportSize,
                (settings.ImageSize.Width, settings.ImageSize.Height),
                settings.ImageDpi,
                settings.Dithering.Enabled,
                {name: found(dictionary.TryGetValue(name)) for name in RENDER_KEYS},
            ),
            "render mesh": str(doc.MeshingParameterStyle),
            "sun": (
                sun.Enabled,
                sun.Latitude,
                sun.Longitude,
                sun.TimeZone,
                sun.DaylightSavingOn,
                sun.DaylightSavingMinutes,
                sun.North,
                sun.GetDateTime(DateTimeKind.Local).ToString("yyyy-MM-ddTHH:mm"),
                sun.Intensity,
            ),
            "earth anchor": (
                anchor.EarthBasepointLatitude,
                anchor.EarthBasepointLongitude,
                anchor.EarthBasepointElevation,
                (rounded(anchor.ModelNorth.X), rounded(anchor.ModelNorth.Y)),
                (rounded(anchor.ModelEast.X), rounded(anchor.ModelEast.Y)),
            ),
            "environment": file_sky(file),
            "layers": tuple(
                (layer.Color.ToArgb(), layer.PlotColor.ToArgb(), doc.SectionStyles[layer.SectionStyleIndex].Name if layer.SectionStyleIndex >= 0 else None)
                for layer in doc.Layers
                if not layer.IsDeleted
            ),
            "section styles": tuple(section_facts(style) for style in section_styles(doc)),
            "saved states": tuple(name for _, names in saved_states(doc) for name in names),
            "hatch patterns": tuple(sorted(pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted)),
            "linetypes": {linetype.Name: tuple(map(rounded, segments(linetype))) for linetype in doc.Linetypes if not linetype.IsDeleted},
        }
    finally:
        file.Dispose()
        doc.Dispose()


def write_units(doc: Rhino.RhinoDoc, measures: DocumentUnits) -> None:
    """Model and page units, tolerance, display, each view at its factory projection around the origin in the declared mode, grid defaults, and the one dimension style from Rhino's built-in for the units."""
    doc.AdjustModelUnitSystem(measures.model, scale=False)
    doc.AdjustPageUnitSystem(measures.page, scale=False)
    doc.ModelAbsoluteTolerance = measures.tolerance
    doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision = measures.model_display, measures.model_precision
    doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision = measures.page_display, measures.page_precision
    mode, extent = mode_description(), GRID_THICK_EVERY * measures.grid
    for view in doc.Views:
        viewport = view.ActiveViewport
        viewport.SetProjection(System.Enum.Parse(clr.GetClrType(DefinedViewportProjection), viewport.Name), viewport.Name, updateConstructionPlane=True)
        viewport.ZoomBoundingBox(BoundingBox(-extent, -extent, 0, extent, extent, 0))
        plane = viewport.GetConstructionPlane()
        plane.GridSpacing, plane.SnapSpacing, plane.GridLineCount, plane.ThickLineFrequency = measures.grid, measures.snap, measures.lines, GRID_THICK_EVERY
        viewport.SetConstructionPlane(plane)
        viewport.DisplayMode = mode
    defaults = doc.GetGridDefaults()
    defaults.GridSpacing, defaults.SnapSpacing, defaults.GridLineCount, defaults.GridThickFrequency = measures.grid, measures.snap, measures.lines, GRID_THICK_EVERY
    doc.SetGridDefaults(defaults)
    style = doc.DimStyles.Current
    for extra in [each for each in doc.DimStyles if each.Id != style.Id]:
        doc.DimStyles.Delete(extra.Index, quiet=True)
    style.CopyFrom(next(each for each in doc.DimStyles.BuiltInStyles if each.Name == measures.style))
    style.Name = measures.style
    style.DimensionLengthDisplay, style.LengthResolution = measures.dimension_display, measures.model_precision
    style.AlternateDimensionLengthDisplay, style.AlternateLengthResolution = measures.alternate_display, measures.alternate_precision
    style.AlternateUnitsDisplay, style.DrawTextMask = False, False
    doc.DimStyles.Modify(style, style.Id, quiet=True)


def write_render(doc: Rhino.RhinoDoc, page: Rhino.UnitSystem) -> None:
    """Frame at the page unit, the engine's samples, noise threshold, light paths, clamps, glossy filter, and caustics, quality render meshes, sun and its intensity, earth anchor, and the physical sky at its gain as the one environment for every usage, from the render and location declarations."""
    doc.MeshingParameterStyle = MeshingParameterStyle.Quality
    settings = doc.RenderSettings
    settings.ImageUnitSystem = page
    settings.UseViewportSize, settings.ImageSize, settings.ImageDpi = False, Size(*FRAME_SIZE), DPI
    settings.Dithering.Enabled = True
    for name, value in RENDER_KEYS.items():
        settings.UserDictionary.Set(name, val=value)
    north, east, zone, daylight, daylight_minutes, sun_north = site()
    sun = settings.Sun
    sun.Enabled, sun.Latitude, sun.Longitude, sun.Intensity = True, location.LATITUDE, location.LONGITUDE, SUN_INTENSITY
    sun.TimeZone, sun.DaylightSavingOn, sun.DaylightSavingMinutes, sun.North = zone, daylight, daylight_minutes, sun_north
    moment = location.MOMENT
    sun.SetDateTime(DateTime(moment.year, moment.month, moment.day, moment.hour, moment.minute, 0), DateTimeKind.Local)
    for held in tuple(doc.RenderEnvironments):
        doc.RenderEnvironments.Remove(held)
    environment = RenderContentType.NewContentFromTypeId(ContentUuids.BasicEnvironmentType, doc)
    environment.Name = "Sky"
    texture = RenderContentType.NewContentFromTypeId(ContentUuids.PhysicalSkyTextureType, doc)
    texture.BeginChange(RenderContent.ChangeContexts.Program)
    texture.SetParameter(SKY_SUN, value=True)
    texture.SetParameter(SKY_GAIN, value=SKY_MULTIPLIER)
    texture.EndChange()
    environment.SetChild(texture, SKY_SLOT)
    doc.RenderEnvironments.Add(environment)
    settings.BackgroundStyle = Rhino.Display.BackgroundStyle.Environment
    for usage in SKY_USAGES:
        settings.SetRenderEnvironmentId(usage, environment.Id)
    doc.RenderSettings = settings
    anchor = doc.EarthAnchorPoint
    anchor.EarthBasepointLatitude, anchor.EarthBasepointLongitude, anchor.EarthBasepointElevation = location.LATITUDE, location.LONGITUDE, location.ELEVATION
    anchor.ModelNorth, anchor.ModelEast = Vector3d(*north, 0), Vector3d(*east, 0)
    doc.EarthAnchorPoint = anchor


def write_tables(doc: Rhino.RhinoDoc, linetypes: Mapping[str, tuple[float, ...]]) -> None:
    """Rhino's current hatch set with Solid, the cut section style alone, the current layer alone in black with the cut style, its linetype set at the given segment lengths and no other linetype, and no saved state."""
    hatches = {pattern.Name: pattern for pattern in new_hatches()}
    for pattern in [pattern for pattern in doc.HatchPatterns if pattern.Name not in hatches]:
        doc.HatchPatterns.Delete(pattern.Index, quiet=True)
    held = {pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted}
    for name in [name for name in hatches if name not in held]:
        doc.HatchPatterns.Add(hatches[name])
    style = cut_style()
    held_index = doc.SectionStyles.Find(CUT_STYLE)
    index = doc.SectionStyles.Add(style) if held_index < 0 else held_index
    doc.SectionStyles.Modify(style, index, quiet=True)
    current = doc.Layers.CurrentLayerIndex
    for layer in [layer for layer in doc.Layers if not layer.IsDeleted and layer.Index != current]:
        doc.Layers.Delete(layer.Index, quiet=True)
    layer = doc.Layers.CurrentLayer
    layer.Color, layer.PlotColor, layer.SectionStyleIndex = color(Line.GEOMETRY), color(Line.GEOMETRY), index
    doc.Layers.Modify(layer, current, quiet=True)
    for other in [other for other in section_styles(doc) if other.Name != CUT_STYLE]:
        doc.SectionStyles.Delete(other.Index, quiet=True)
    doc.Linetypes.LoadDefaultLinetypes()
    for name, lengths in linetypes.items():
        linetype = doc.Linetypes.FindName(name)
        linetype.SetSegments(Array[float](lengths))
        doc.Linetypes.Modify(linetype, linetype.Index, quiet=True)
    for linetype in [linetype for linetype in doc.Linetypes if not linetype.IsDeleted and linetype.Name not in linetypes]:
        doc.Linetypes.Delete(linetype.Index, quiet=True)
    for table, names in saved_states(doc):
        for name in names:
            table.Delete(name)


def write_template(path: str, measures: DocumentUnits) -> None:
    """The template rewritten through a headless document opened on it, or on the default template when it is absent, then its file's views with Perspective alone maximized."""
    doc = Rhino.RhinoDoc.CreateHeadless(path if Path(path).exists() else Settings.FileSettings.TemplateFile)
    try:
        write_units(doc, measures)
        write_render(doc, measures.page)
        write_tables(doc, measures.linetypes)
        doc.WriteFile(path, FileWriteOptions())
    finally:
        doc.Dispose()
    file = File3dm.Read(path)
    try:
        for view in file.Views:
            view.Maximized = view.Viewport.IsPerspectiveProjection
        file.Write(path, File3dmWriteOptions())
    finally:
        file.Dispose()


def template_rows() -> tuple[Row, ...]:
    """The imperial default template and the metric one beside it, each converged as one set of facts."""
    default = Settings.FileSettings.TemplateFile
    paths = ((default, Units.IMPERIAL), (str(Path(default).with_name(f"{Units.METRIC.name.title()}.3dm")), Units.METRIC))
    return tuple(
        (f"template {Path(path).stem}", partial(template_facts, path), lambda _, path=path, measures=measures: write_template(path, measures), template_target(measures))
        for path, units in paths
        for measures in [DocumentUnits(units)]
    )


# --- [MEASURES]
def extents() -> dict[Site | Extent, float]:
    """Lengths in points and row counts the file stage sizes from: the dock sites, button cell, resizer, and tab strip, and the Layers, Osnap, Materials, and Libraries parts, each panel read shown in its container and closed after when it was closed before."""
    doc, panels, (head, *_) = Rhino.RhinoDoc.ActiveDoc, Rhino.UI.Panels, window.RIGHT_TOP
    sites, bars, tab_panels, toolbar_settings, tabs, resizer, grid_kind = (
        System.Type.GetType(f"Rhino.UI.{name}, Rhino.UI", throwOnError=True)
        for name in (
            "Internal.TabPanels.TabPanelDockSites",
            "Internal.TabPanels.TabPanelDockBars",
            "Internal.TabPanels.TabPanelSettings",
            "Internal.TabPanels.ToolbarSettings",
            "Internal.TabPanels.Controls.BaseTabControl",
            "Internal.TabPanels.Controls.DockSiteResizer",
            "DialogPanels.LayerTreeGridView",
        )
    )

    def height(control: object) -> float:
        """Height of an Eto control of any runtime type."""
        return control.GetType().GetProperty("Height").GetValue(control)

    def container(panel: Panel) -> object:
        """The control of the dock bar holding the panel, as tall as its band gives it."""
        bar = bars.GetMethod("FromDockBarId").Invoke(None, Array[System.Object]([panels.PanelDockBar(Guid(str(panel)))]))
        return bar.GetType().GetMethod("HasContent").Invoke(bar, Array[System.Object]([System.UInt32(doc.RuntimeSerialNumber)]))

    def private(owner: object, kind: object, name: str) -> object:
        """Value of the private instance field the type declares on the owner."""
        return kind.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner)

    top = panels.PanelDockBar(Guid(str(head)))
    homes = {panel: panels.PanelDockBar(Guid(str(panel))) for panel in (Panel.LAYERS, Panel.MATERIALS, Panel.LIBRARIES)}
    for panel, home in homes.items():
        panels.OpenPanel(top if home == Guid.Empty else home, Guid(str(panel)), makeSelectedPanel=True)
    try:
        Rhino.UI.RhinoEtoApp.MainWindowForDocument(doc).ControlObject.ContentView.LayoutSubtreeIfNeeded()
        docked = sites.GetMethod("FromDocument", Array[System.Type]([clr.GetClrType(Rhino.RhinoDoc)])).Invoke(None, Array[System.Object]([doc]))
        controls = {location: held.GetType().GetProperty("Control").GetValue(held) for location in Site for held in [docked.GetType().GetProperty(location.value).GetValue(docked)]}
        style = tab_panels.GetProperty("HorizontalDisplayStyle").GetValue(None)
        toolbar = toolbar_settings.GetProperty("Instance").GetValue(None)
        buttons = toolbar.GetType().GetProperty("Buttons").GetValue(toolbar)
        layers, osnap, materials, libraries = (panels.GetPanel(Guid(str(panel)), doc) for panel in (Panel.LAYERS, Panel.OSNAP, Panel.MATERIALS, Panel.LIBRARIES))
        grid = next(each for each in layers.GetType().GetProperty("Children").GetValue(layers) if grid_kind.IsInstanceOfType(each))
        outline = grid.GetType().GetProperty("ControlObject").GetValue(grid)
        editor = materials.GetType().BaseType
        thumbnails = private(materials, editor, "m_thumbnail_list")
        thumbview = private(thumbnails, thumbnails.GetType(), "m_thumbview")
        model = thumbnails.GetType().GetProperty("ViewModel").GetValue(thumbnails)
        splitter, tree = (private(libraries, libraries.GetType(), name) for name in ("m_splitter", "m_tree_grid"))
        folders = tree.ControlObject
        return {
            **{location: height(control) for location, control in controls.items()},
            Extent.TAB_STRIP: tabs.GetMethod("CalculateTabHeight", BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([style])),
            Extent.BUTTON: buttons.GetType().GetProperty("TotalButtonSize").GetValue(buttons),
            Extent.RESIZER: resizer.GetProperty("ResizerWidth").GetValue(None),
            Extent.LAYERS_CHROME: height(container(Panel.LAYERS)) - height(grid),
            Extent.LAYERS_HEADER: outline.HeaderView.Frame.Size.Height.Value,
            Extent.LAYERS_ROW: outline.RowHeight.Value + outline.IntercellSpacing.Height.Value,
            Extent.OSNAP: height(container(Panel.OSNAP)) - height(osnap) + osnap.GetPreferredSize().Height,
            Extent.MATERIALS_STRIP: private(materials, editor, "m_thumb_list_table").Spacing.Height + private(materials, editor, "m_view_mode_button_table").GetPreferredSize().Height,
            Extent.MATERIALS_ROW: model.GetType().GetProperty("ThumbHeigth").GetValue(model) + private(thumbview, thumbview.GetType(), "m_space_between_items"),
            Extent.LIBRARIES_CHROME: height(container(Panel.LIBRARIES)) - (splitter.Height - splitter.SplitterWidth),
            Extent.LIBRARIES_BORDER: tree.Height - folders.EnclosingScrollView.ContentView.Frame.Size.Height.Value,
            Extent.LIBRARIES_ROW: folders.RowHeight.Value + folders.IntercellSpacing.Height.Value,
            Extent.LIBRARIES_FOLDERS: folders.RowCount.ToInt64(),
            Extent.LIBRARIES_LIST_MINIMUM: splitter.Panel2MinimumSize,
        }
    finally:
        for panel in (panel for panel, home in homes.items() if home == Guid.Empty):
            panels.ClosePanel(Guid(str(panel)), doc)
        panels.OpenPanel(top, Guid(str(head)), makeSelectedPanel=True)


# --- [DOCUMENTS]
def released(document: Rhino.RhinoDoc) -> bool:
    """Whether the document holds unsaved edits once a titled one is saved and an untitled one is marked unmodified."""
    if document.Path:
        document.Save()
    else:
        document.Modified = False
    return document.Modified


# --- [PROOF]
def window_readings() -> tuple[Reading, ...]:
    """The layout the host wrote, as the relaunched Rhino holds it: the top panels sharing one container, Layers in a container of its own, Osnap under the sidebar, and Selection Filters beside Osnap."""
    panels, (head, *_) = Rhino.UI.Panels, window.RIGHT_TOP
    top = panels.PanelDockBar(Guid(str(head)))
    return (
        *((f"panel {panel.name} in the top container", lambda panel=panel: panels.PanelDockBar(Guid(str(panel))) == top, True) for panel in window.RIGHT_TOP),
        *((f"panel {panel.name} in its own container", lambda panel=panel: panels.PanelDockBar(Guid(str(panel))) not in {top, Guid.Empty}, True) for panel in window.RIGHT_BOTTOM),
        ("panel Osnap outside the top container", lambda: panels.PanelDockBar(Guid(str(Panel.OSNAP))) not in {top, Guid.Empty}, True),
        ("panel Selection Filters beside Osnap", lambda: panels.PanelDockBar(Guid(str(Panel.OSNAP))) in panels.PanelDockBars(Guid(str(Panel.SELECTION_FILTERS))), True),
    )


def document_readings() -> tuple[Reading, ...]:
    """The document Rhino opened from the template at launch: every view in the declared mode and the physical sky environment."""
    doc = Rhino.RhinoDoc.ActiveDoc
    return (
        *((f"document view {view.MainViewport.Name} mode", lambda view=view: str(view.ActiveViewport.DisplayMode.Id), str(window.MODE_ID)) for view in doc.Views),
        ("document environment", partial(sky, doc), (str(ContentUuids.PhysicalSkyTextureType), True, SKY_MULTIPLIER)),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> Iterator[Row]:
    """Every setting row in write order, the display mode imported before the Grasshopper preview reads its point size."""
    root = Rhino.PersistentSettings.RhinoAppSettings
    options, ui_settings = root.AddChild("Options"), root.AddChild("UI").AddChild("Settings")
    steps = (
        partial(settings_rows, options),
        theme_rows,
        partial(color_rows, options, ui_settings),
        command_rows,
        display_rows,
        template_rows,
        lambda: grasshopper.rows(mode_description().DisplayAttributes.PointRadius, partial(render, rgb=hex_color)),
        panel_rows,
    )
    return (row for step in steps for row in step())


def main() -> None:
    """Every row written on difference, the settings flushed, and the report printed with each absent plug-in and an empty materials library skipped and the extents the file stage sizes from."""
    results = tuple(map(converged, rows()))
    PlugIn.FlushSettingsSavedQueue()
    absent_plugins = tuple(name for name, _, record in plugin_loads() if record is None)
    emit(results, absent_plugins if stocked(MATERIALS) else (*absent_plugins, str(MATERIALS)), extents())


def close() -> None:
    """The document step the host runs before it quits Rhino, each modified document saved or marked unmodified and reported as a change."""
    emit(
        (
            (f"document {document.RuntimeSerialNumber} unsaved edits", True, held, False)
            for document in [each for each in Rhino.RhinoDoc.OpenDocuments() if each.Modified]
            for held in (released(document),)
        ),
        (),
        {},
    )


def layout() -> None:
    """Every row, the host's window layout, and the document opened from the template, read in the relaunched Rhino, then the Grasshopper 2 editor's rectangle written on difference over the views the settled layout gives."""
    emit((*map(observed, (*rows(), *window_readings(), *document_readings())), converged(grasshopper.editor_rectangle())), (), {})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["close", "extents", "layout", "main"]
