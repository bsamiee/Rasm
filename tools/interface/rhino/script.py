# ty: ignore[unresolved-attribute, unresolved-import, unsupported-operator, invalid-argument-type, no-matching-overload]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, misc, call-overload, operator, no-any-return"
# ruff: file-ignore[banned-api, import-outside-top-level]
"""Rows of Rhino's application settings and declared packages converged inside Rhino's CPython, and the entry points the host calls."""

from collections.abc import Callable, Iterable, Iterator, Mapping, Sequence
from functools import partial, reduce
from importlib import import_module
import json
import math
from operator import attrgetter
from pathlib import Path
from typing import Final

import clr
from Eto.Forms import Screen
from Foundation import NSArray, NSString, NSUserDefaults, NSUserDefaultsType
import Rhino
import Rhino.ApplicationSettings as Settings
from Rhino.Display import DisplayModeDescription, DisplayPipelineAttributes, PointStyle
from Rhino.DocObjects import ObjectType
from Rhino.DocObjects.Tables import RestoreLayerProperties
from Rhino.PlugIns import PlugIn, PlugInLoadTime
from Rhino.Render import SupportOptions
from Rhino.Runtime import HostUtils, NamedParametersEventArgs
import System
from System import Array, Guid, String
from System.Collections.Generic import List
from System.Drawing import Color, ColorTranslator
from System.Globalization import CultureInfo
from System.Net.Sockets import TcpClient
from System.Reflection import BindingFlags
from System.Text import Encoding

from interface.aliases import Alias
from interface.render import MATERIALS, stocked
from interface.report import Kind, line, Row
from interface.rhino import template
from interface.rhino.grasshopper import configuration
from interface.rhino.rows import absent, color, emit, found, hex_color, internal_setting, key, member, SWATCHES
from interface.rhino.window import bands, Extent, Panel, RIGHT_BOTTOM, RIGHT_TOP, Site
from interface.roles import Accent, Alpha, Axis, blend, Guide, Ink, Line, POINT_WIDTH, Selection, Status, Surface, Text, Typography
from interface.units import ANGLE_STEP

# --- [CONSTANTS] ------------------------------------------------------------------------

OPTIONS: Final = "Options"

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SETTINGS]
def internal(name: str, value: str) -> object:
    """Member of an internal Rhino enum by its assembly-qualified type name."""
    return System.Enum.Parse(System.Type.GetType(name, throwOnError=True), value)


def settings_rows() -> tuple[Row, ...]:
    """Rows of Rhino's application settings, the Settings window closed first."""
    root = Rhino.PersistentSettings.RhinoAppSettings
    options = root.AddChild(OPTIONS)
    general, advanced, appearance, mouse = (options.AddChild(name) for name in ("General", "Advanced", "Appearance", "Mouse"))
    preferences = Rhino.UI.RhinoEtoApp.ApplicationPreferencesWindowForPage(None)
    defaults, monitor = NSUserDefaults.StandardUserDefaults, NSUserDefaults("com.mcneel.rhinoceros.RhinoMonitor", NSUserDefaultsType.SuiteName)
    english, languages = CultureInfo(1033), "AppleLanguages"
    osnaps = Settings.OsnapModes
    tab_panels, notes = (System.Type.GetType(name, throwOnError=True) for name in ("Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI", "Rhino.UI.Runtime.Settings, Rhino.UI"))
    toolbar = System.Type.GetType("Rhino.UI.Internal.TabPanels.ToolbarSettings, Rhino.UI", throwOnError=True).GetProperty("Instance").GetValue(None)
    buttons, tooltips = toolbar.Buttons, toolbar.ToolTips
    tab_style = partial(internal, "Rhino.UI.Internal.TabPanels.TabControlDisplayStyle, Rhino.UI")
    icon_size = 16

    def presentation(entry: str, **parameters: int) -> int | None:
        """Command options presentation mode the command line's named callback returns, None while it names no mode."""
        args = NamedParametersEventArgs()
        try:
            for name, value in parameters.items():
                args.Set(name, value)
            HostUtils.ExecuteNamedCallback(f"Rhino.UI.Internal.DockBars.CommandLine.{entry}", args)
            return found(args.TryGetInt("mode"))
        finally:
            args.Dispose()

    def prompt_font(family: str) -> None:
        """Set the command prompt font family through the appearance state the native owner reads."""
        state = Settings.AppearanceSettings.GetCurrentState()
        state.CommandPromptFontName = family
        Settings.AppearanceSettings.UpdateFromState(state)

    return (
        Row(label="settings window open", read=lambda: preferences is not None and preferences.Visible, write=lambda _: preferences.Close(), target=False),
        *(
            member("view", Settings.ViewSettings, name, target=True)
            for name in ("RotateViewAroundObjectAtMouseCursor", "AlwaysPanParallelViews", "PanPlanParallelViewsWithControlShiftRMB", "AutoAdjustTargetDepth")
        ),
        *(member("view", Settings.ViewSettings, name, target=False) for name in ("RotateViewAroundAutogumball", "SingleClickMaximize", "LinkedViewports")),
        member("view", Settings.ViewSettings, "ZoomScale", target=1 / math.sqrt(1.2)),
        member("view", Settings.ViewSettings, "ViewRotation", target=Settings.ViewSettings.ViewRotationStyle.RotateAroundWorldAxes),
        member("view", Settings.ViewSettings, "RotateCircleIncrement", target=360 // ANGLE_STEP),
        member("general", Settings.GeneralSettings, "MiddleMouseMode", target=Settings.MiddleMouseMode.PopupToolbar),
        member("general", Settings.GeneralSettings, "MiddleMousePopupToolbar", target="Popup"),
        member("general", Settings.GeneralSettings, "MouseSelectMode", target=Settings.MouseSelectMode.Combo),
        member("general", Settings.GeneralSettings, "MinimumUndoSteps", target=100),
        member("general", Settings.GeneralSettings, "MaximumUndoMemoryMb", target=4096),
        member("general", Settings.GeneralSettings, "EnableContextMenu", target=True),
        member("general", Settings.GeneralSettings, "ContextMenuDelay", target=System.TimeSpan(0, 0, 0, 0, 300)),
        *(
            key(general, name, target=True)
            for name in ("MiddleMousePlainButtonRotateMode", "MiddleMouseViewManipulationMode", "EnableTrackpadScrolling", "MouseOverHighlight", "SilhouetteHighlighting")
        ),
        *(key(general, name, target=False) for name in ("MiddleMouseShiftControlSwap", "UsageStatisticsEnabled")),
        key(general, "SilhouetteThickness", target=3),
        *(key(advanced, name, target=True) for name in ("DisableModelAndPageUnitsDifferDialog", "DisablePageUnitsNotInchesOrMMDialog", "UseEtoCommandUI")),
        *(
            key(advanced, name, target=False)
            for name in (
                "EnableCheckForUpdates",
                "MacDisplayOldVersionAutosaveWarning",
                "DisplayNonOriginModelBasepointWarning",
                "UseCompressionWhenSaving",
                "AllowUnadornedShortcuts",
                "NotesUseSpacesForTabs",
            )
        ),
        key(advanced, "NotesTabWidth", target=8),
        absent(advanced, "DarkMode", label="key"),
        key(root.AddChild("Warnings"), "MissingFontWarning", target=False),
        member("plugin", PlugIn, "AskOnLoadProtection", target=False),
        key(options.AddChild("PackageManager"), "CheckForUpdates", target=False),
        key(options.AddChild("FileSettings"), "AutoSaveVersionsEnabled", target=True),
        *(key(mouse, name, target=False) for name in ("EnableUnselectedObjectDrag", "EnableMouseScrollBallRotation", "EnableMagicMouseRotation", "DisableRightClickAsEnter")),
        *(key(mouse, name, target=True) for name in ("EnableUnselectedGripDrag", "EnableMagicMouseGestures")),
        key(mouse, "MouseButton4Macro", target="'_Zoom _Selected"),
        member("appearance", Settings.AppearanceSettings, "LanguageIdentifier", target=english.LCID),
        member("appearance", Settings.AppearanceSettings, "HelpLanguageIdentifier", target=0),
        *(
            member("appearance", Settings.AppearanceSettings, name, target=True)
            for name in ("EchoCommandsToHistoryWindow", "EchoPromptsToHistoryWindow", "ShowViewportTitles", "ShowCrosshairs", "ShowCursorWhenCrosshairsVisible", "ShowOsnapBar")
        ),
        member("appearance", Settings.AppearanceSettings, "ShowLayoutDropShadow", target=False),
        member("appearance", Settings.AppearanceSettings, "CommandPromptFontSize", target=110),
        Row(label="appearance CommandPromptFontName", read=lambda: found(appearance.TryGetString("CommandPromptFontName")), write=prompt_font, target=Typography.INTERFACE.family),
        *(key(appearance, name, target=True) for name in ("AutocompleteCommands", "FuzzyAutocomplete", "ShowStatusbar", "AlwaysShowGeneralObjectProperties", "ShowSideBar")),
        key(appearance, "StatusbarInfoPaneMode", target=int(internal("Rhino.UI.Internal.TabPanels.Controls.StatusBarInfoPaneMode, Rhino.UI", "selected_object_count"))),
        key(appearance, "DirectionArrowThickness", target=2),
        Row(
            label="command options presentation",
            read=partial(presentation, "GetPresentationStyle"),
            write=lambda mode: presentation("SetPresentationStyle", mode=mode),
            target=int(internal("Rhino.UI.CommandPromptLocation, RhinoCommon", "SideBar")),
        ),
        key(options, "CommandPromptStyle", target=int(internal("Rhino.UI.CommandPromptStyle, RhinoCommon", "Graphical"))),
        absent(options, "CommandPromptLocation", label="key"),
        member("file", Settings.FileSettings, "ClipboardOnExit", target=Settings.ClipboardState.DeleteData),
        *(member("file", Settings.FileSettings, name, target=False) for name in ("FileLockingOpenWarning", "CreateOtherBackupFiles", "SaveViewChanges")),
        member("opengl", Settings.OpenGLSettings, "AntialiasLevel", target=Rhino.AntialiasLevel.Good),
        *(member("gumball", Settings.GumballSettings, name, target=True) for name in ("EnableGumball", "SnappyGumball", "MergeFacesAfterExtrude")),
        *(member("gumball", Settings.GumballSettings, name, target=2) for name in ("AxisThickness", "ArcThickness")),
        *(member("smarttrack", Settings.SmartTrackSettings, name, target=True) for name in ("UseSmartTrack", "SmartOrtho", "Parallels", "UseDottedLines")),
        key(options.AddChild("SmartTrack"), "MaximumSmartPoints", target=4),
        *(member("modelaid", Settings.ModelAidSettings, name, target=True) for name in ("Osnap", "Ortho", "ProjectToCPlaneInPlanParallelViews", "DragStartsWindowSelection", "AltPlusArrow")),
        *(member("modelaid", Settings.ModelAidSettings, name, target=False) for name in ("GridSnap", "Planar", "ProjectSnapToCPlane", "SnapToFiltered", "ExtendToApparentIntersection")),
        member("modelaid", Settings.ModelAidSettings, "OsnapModes", target=osnaps.End | osnaps.Point | osnaps.Midpoint | osnaps.Center | osnaps.Intersection | osnaps.Perpendicular | osnaps.Quadrant),
        member("modelaid", Settings.ModelAidSettings, "OrthoAngle", target=math.radians(6 * ANGLE_STEP)),
        member("modelaid", Settings.ModelAidSettings, "NudgeMode", target=1),
        *(member("chooseone", Settings.ChooseOneObjectSettings, name, target=True) for name in ("ShowObjectLayer", "ShowObjectTypeDetails", "ShowAllOption")),
        member("chooseone", Settings.ChooseOneObjectSettings, "ShowTitlebarAndBorder", target=False),
        member("selectionfilter", Settings.SelectionFilterSettings, "GlobalGeometryFilter", target=ObjectType.AnyObject),
        member("selectionfilter", Settings.SelectionFilterSettings, "Enabled", target=Settings.SelectionFilterSettings.GetDefaultState().Enabled),
        *(member("tooltip", Settings.CursorTooltipSettings, name, target=True) for name in ("TooltipsEnabled", "DistancePane")),
        key(options.AddChild("Grid"), "AxisLineWidth", target=1),
        *(
            internal_setting(tab_panels, name, target=target)
            for name, target in (
                ("LockDockedWindows", True),
                ("TabIconSize", icon_size),
                ("ToolBarImageSize", 24),
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
            member(type(buttons).__name__, buttons, name, target=target)
            for name, target in (
                ("PanelButtonSize", icon_size),
                ("ButtonPadding", 3),
                ("SpacerSize", 5),
                ("Cascade", internal("Rhino.UI.Internal.TabPanels.CascadeStyle, Rhino.UI", "AsPanel")),
                ("MiddleMouseDelay", 400),
            )
        ),
        *(member(type(tooltips).__name__, tooltips, name, target=True) for name in ("IncludeShortcut", "IncludeAlias")),
        key(root, "OSnapButtonDisplay", target=int(internal("Rhino.UI.DialogPanels.OSnapPanel+OSnapButtonDisplay, Rhino.UI", "IconOnly"))),
        key(root, "SelectionFilterButtonDisplay", target=int(internal("Rhino.UI.DialogPanels.SelectionFilterUi+ButtonDisplay, Rhino.UI", "IconOnly"))),
        *(key(root, name, target=icon_size) for name in ("OSnapIconSize", "SelectionFilterIconSize")),
        *(key(root, name, target=True) for name in ("OSnapStretchButtons", "SelectionFilterUseCheckedColor", "SelectionFilterStretchButtons")),
        key(root, "AnnotationSpellCheck", target=False),
        key(root.AddChild("PropertiesEditor").AddChild(OPTIONS), "DisplayPagesOnIdle", target=True),
        internal_setting(notes, "NotesRestoreCursorPosition", target=False),
        *(
            Row(label=f"appkit {name}", read=partial(read, name), write=lambda value, write=write, name=name: write(value, name), target=target)
            for read, write, name, target in (
                (defaults.BoolForKey, defaults.SetBool, "AppleReduceDesktopTinting", True),
                (defaults.BoolForKey, defaults.SetBool, "SUAutomaticallyUpdate", False),
                (lambda name: defaults.IntForKey(name).ToInt64(), lambda value, name: defaults.SetInt(System.IntPtr(value), name), "AppleAccentColor", 4),
                (defaults.StringForKey, defaults.SetString, "AppleHighlightColor", " ".join((*(f"{channel / 255:.6f}" for channel in Accent.TEXT_SELECTED), "Other"))),
                (defaults.StringForKey, defaults.SetString, "MRLanguage", english.Parent.Name),
                (monitor.BoolForKey, monitor.SetBool, "MRShouldIncludeModelFileInReport", False),
            )
        ),
        Row(
            label=f"appkit {languages}",
            read=partial(defaults.StringArrayForKey, languages),
            write=lambda value: defaults.SetValueForKey(NSArray.FromStrings(Array[String](list(value))), NSString(languages)),
            target=(english.Parent.Name,),
        ),
    )


# --- [PANELS]
def panel_rows() -> tuple[Row, ...]:
    """Rows of the panels' settings and each right-hand panel open."""
    rdk, eto_panels, commands = (Rhino.PersistentSettings.FromPlugInId(PlugIn.IdFromName(name)) for name in ("Renderer Development Kit", "RDK_EtoUI", "Commands"))
    settings = rdk.AddChild("Settings")
    support = settings.AddChild("RendererSupport")
    checkers = {"LightPreviewCheckerColor": Surface.BOX, "DarkPreviewCheckerColor": Surface.PANEL}
    library = (
        (
            ("Libraries_CustomPathList", SupportOptions.Libraries_CustomPathList, SupportOptions.Libraries_SetCustomPathList, str(MATERIALS)),
            ("Libraries_InitialLocation", SupportOptions.Libraries_InitialLocation, SupportOptions.Libraries_SetInitialLocation, SupportOptions.RdkInitialLocation.CustomFolder),
            ("Libraries_InitialLocationCustomFolder", SupportOptions.Libraries_InitialLocationCustomFolder, SupportOptions.Libraries_SetInitialLocationCustomFolder, str(MATERIALS)),
        )
        if stocked(MATERIALS)
        else ()
    )
    layer_states = commands.AddChild("LayerStates")
    restored = RestoreLayerProperties.Visible | RestoreLayerProperties.Locked | RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.NewDetailOn
    preview = Rhino.PersistentSettings.RhinoAppSettings.AddChild("ObjectManager").AddChild("Preview")
    panels = Rhino.UI.Panels
    head, *_ = RIGHT_TOP

    def open_panel(panel: Panel) -> None:
        """Open the panel in the right column's top container, or in its last container while that one is closed."""
        held, opened = panels.PanelDockBar(Guid.Parse(str(head))), Guid.Parse(str(panel))
        if held == Guid.Empty:
            panels.OpenPanel(opened)
        else:
            panels.OpenPanel(held, opened, makeSelectedPanel=False)

    return (
        key(settings, "LibrariesViewMode", target=1, label="libraries"),
        key(settings, "LibrariesListSizePercentage", target=0, label="libraries"),
        *(
            Row(label=f"rdk {name}", read=lambda name=name: found(support.TryGetUnsignedInteger(name)), write=partial(support.SetUnsignedInteger, name), target=ColorTranslator.ToWin32(color(rgb)))
            for name, rgb in checkers.items()
        ),
        *(key(eto_panels.AddChild(panel), "ViewMode", target=1, label=panel) for panel in ("BlockContent", "FileExplorer")),
        Row(label="libraries Libraries_ShowDocuments", read=SupportOptions.Libraries_ShowDocuments, write=SupportOptions.Libraries_SetShowDocuments, target=False),
        *(Row(label=f"libraries {name}", read=read, write=write, target=target) for name, read, write, target in library),
        Row(label="block content BlockContent_ShowDocuments", read=SupportOptions.BlockContent_ShowDocuments, write=SupportOptions.BlockContent_SetShowDocuments, target=False),
        Row(
            label="layer states RestoreLayerProperties",
            read=lambda: found(layer_states.TryGetUnsignedInteger("RestoreLayerProperties")),
            write=partial(layer_states.SetUnsignedInteger, "RestoreLayerProperties"),
            target=int(restored),
        ),
        key(layer_states, "ModelPropertiesChecked", target=True, label="layer states"),
        key(layer_states, "ViewportPropertiesChecked", target=True, label="layer states"),
        absent(preview, "DisplayModeId", label="block preview"),
        *(
            Row(label=f"panel {panel.name} open", read=lambda panel=panel: panels.PanelDockBar(Guid.Parse(str(panel))) != Guid.Empty, write=lambda _, panel=panel: open_panel(panel), target=True)
            for panel in (*RIGHT_TOP, *RIGHT_BOTTOM)
        ),
    )


# --- [COLORS]
def theme_rows() -> tuple[Row, ...]:
    """Rows of each theme key a Rhino control draws in its role, every other theme key holding a color deleted."""
    theme_settings = Rhino.PersistentSettings.RhinoAppSettings.AddChild("UI").AddChild("ThemeSettings")
    shared = {
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
        "Button.Disabled.Text": Text.DISABLED,
    }
    zones = {
        "Frame": (
            Surface.FRAME,
            ("Background", "Edge"),
            {
                "Text.Secondary": Text.SECONDARY,
                "GripperDot": Line.BORDER,
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
    declared = {f"{zone}.{suffix}": color(rgb) for zone, (ground, grounded, overrides) in zones.items() for suffix, rgb in {**shared, **dict.fromkeys(grounded, ground), **overrides}.items()} | {
        f"{zone}.Button.Disabled.Background": color(Surface.PANEL, 0.0) for zone in zones
    }

    def strays() -> tuple[str, ...]:
        """Sorted theme keys holding a color the declaration leaves out."""
        return tuple(sorted(name for name in theme_settings.Keys if name not in declared and found(theme_settings.TryGetColor(name)) is not None))

    def clear(_: object) -> None:
        """Delete every undeclared theme key holding a color."""
        for name in strays():
            theme_settings.DeleteItem(name)

    return (*(key(theme_settings, name, target=target, label="theme") for name, target in declared.items()), Row(label="undeclared theme keys", read=strays, write=clear, target=()))


def color_rows() -> tuple[Row, ...]:
    """Rows of Rhino's color settings in their roles, each color no macOS code draws at its factory value."""
    appearance, gumball, tooltip = Settings.AppearanceSettings, Settings.GumballSettings, Settings.CursorTooltipSettings
    factory, root = appearance.GetDefaultState(), Rhino.PersistentSettings.RhinoAppSettings
    arrow_owner, ui_settings = root.AddChild(OPTIONS).AddChild("Appearance"), root.AddChild("UI").AddChild("Settings")
    axes = tuple(color(axis) for axis in (Axis.X, Axis.Y, Axis.Z))
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
                "PageviewPaperColor": color(Surface.PAPER),
                "FrameBackgroundColor": factory.FrameBackgroundColor,
                "CommandPromptHypertextColor": factory.CommandPromptHypertextColor,
                "GridThickLineColor": color(blend(Line.GRID, Surface.CANVAS, Alpha.GRID_MAJOR)),
                "GridThinLineColor": color(blend(Line.GRID, Surface.CANVAS, Alpha.GRID_MINOR)),
                "LockedObjectColor": color(Line.LOCKED),
                "SelectedObjectColor": color(Selection.ITEM),
                "EditCandidateColor": color(Selection.HOVER),
                "SelectionWindowStrokeColor": color(Selection.ITEM),
                "SelectionWindowFillColor": color(Selection.ITEM, Alpha.SELECTION_FILL),
                "SelectionWindowCrossingStrokeColor": color(Selection.ITEM),
                "SelectionWindowCrossingFillColor": color(Selection.ITEM, Alpha.CROSSING_FILL),
                "FeedbackColor": color(Ink.SCREEN),
                "TrackingColor": color(Guide.TRACKING),
                "CrosshairColor": color(Guide.HANDLE),
                "DefaultLayerColor": color(Ink.DOCUMENT),
                "DefaultObjectColor": color(Ink.DOCUMENT),
                "CommandPromptBackgroundColor": color(Surface.WELL),
                "CommandPromptTextColor": color(Text.PRIMARY),
                "BlackWhiteSwitching": True,
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
        ("gumball", gumball, {"MenuBallColor": color(Guide.HANDLE)}),
        ("curvaturegraph", Settings.CurvatureGraphSettings, {"CurveHairColor": color(Guide.CONSTRUCTION), "SurfaceUHairColor": color(Axis.X), "SurfaceVHairColor": color(Axis.Y)}),
        ("directionanalysis", Settings.DirectionAnalysisSettings, {"Color": color(Guide.CONSTRUCTION)}),
        ("edgeanalysis", Settings.EdgeAnalysisSettings, {"ShowEdgeColor": color(Status.ERROR)}),
        ("zebraanalysis", Settings.ZebraAnalysisSettings, {"StripeColor": color(Ink.DOCUMENT)}),
        ("tooltip", tooltip, {name: getattr(tooltip.GetDefaultState(), name) for name in ("BackgroundColor", "TextColor")}),
    )
    return (
        *(member(label, owner, name, target=target) for label, owner, targets in members for name, target in targets.items()),
        *(member("axis", owner, name, target=axis) for owner, names in triads for name, axis in zip(names, axes, strict=True)),
        *(
            Row(label=f"widget {widget}", read=partial(appearance.GetWidgetColor, widget), write=partial(appearance.SetWidgetColor, widget), target=axis)
            for widget, axis in zip(widgets, axes, strict=True)
        ),
        *(key(arrow_owner, name, target=axis) for name, axis in zip(arrows, axes, strict=True)),
        key(root.AddChild("SoftTransformSettings"), "FalloffColor", target=color(Guide.TRACKING)),
        *(key(root, f"SelectionFilterCheckedColor{scheme}", target=color(Accent.CONTROL_PRESSED)) for scheme in ("Dark", "Light")),
        key(ui_settings, "ColorPanelSwatches", target=tuple(map(hex_color, SWATCHES.values()))),
    )


# --- [COMMANDS]
def command_rows() -> tuple[Row, ...]:
    """Rows of the declared aliases as the whole set, shortcuts, and the settings of plug-ins Rhino bundles."""
    shortcuts, registry = Settings.ShortcutKeySettings, Rhino.PersistentSettings.RhinoAppSettings.AddChild("PlugInRegistry")
    entries = [entry.partition(" ") for entry in Path(__file__).with_name("aliases.txt").read_text(encoding="utf-8").splitlines() if entry]
    aliases, instant = {Alias[name]: macro for name, _, macro in entries if macro}, {Alias[name] for name, _, macro in entries if not macro}
    clr.AddReference("RhinoCyclesCore")
    from Commands.Commands import Alerter
    import RhinoCyclesCore.Core

    shortcut_macros = (
        (Settings.ShortcutKey.F3, "! _Properties"),
        *((getattr(Settings.ShortcutKey, f"CtrlF{index}"), f"'_SetMaximizedViewport {view}") for index, view in enumerate(("Top", "Front", "Right", "Perspective"), start=1)),
    )

    def folded(rows: Iterable[tuple[str, str, bool]]) -> dict[str, tuple[str, bool]]:
        """Each alias's macro, case-folded as Rhino compares macros, and its instant flag, by upper-cased alias name."""
        return {name.upper(): (macro.casefold(), flag) for name, macro, flag in rows}

    def registry_child(plugin: Guid) -> Rhino.PersistentSettings:
        """Plug-in's registry record under the registry version holding it."""
        return next(held for version in registry.ChildKeys if (held := found(registry.AddChild(version).TryGetChild(str(plugin)))) is not None)

    return (
        Row(
            label="aliases",
            read=lambda: folded((held.Alias, held.Macro, held.Instant) for held in map(Settings.CommandAliasList.GetAlias, range(Settings.CommandAliasList.Count))),
            write=lambda _: Settings.CommandAliasList.Update(
                List[Settings.CommandAlias]([Settings.CommandAlias(alias.name, macro, instant=alias in instant) for alias, macro in aliases.items()]), replaceAll=True
            ),
            target=folded((alias.name, macro, alias in instant) for alias, macro in aliases.items()),
        ),
        *(Row(label=f"shortcut {each}", read=partial(shortcuts.GetMacro, each), write=partial(shortcuts.SetMacro, each), target=macro) for each, macro in shortcut_macros),
        *(
            key(registry_child(PlugIn.IdFromName(name)), "LoadMode", target=int(PlugInLoadTime.WhenNeeded), label=f"plugin {name}")
            for name in (f"3DxRhino.{Rhino.RhinoApp.ExeVersion}", "PanelingTools")
        ),
        member("alerter", Alerter.AlerterCommand.Instance, "Enabled", target=False),
        *(
            member("cycles", RhinoCyclesCore.Core.RcCore.It.AllSettings, name, target=target)
            for name, target in (("ThrottleMs", 100), ("SelectedDeviceStr", "-1"), ("IntermediateSelectedDeviceStr", "-1"), ("PixelSize", 1))
        ),
        Row(
            label="render DefaultRenderPlugInId",
            read=lambda: Rhino.Render.Utilities.DefaultRenderPlugInId,
            write=Rhino.Render.Utilities.SetDefaultRenderPlugIn,
            target=PlugIn.IdFromName("Rhino Render"),
        ),
    )


# --- [PACKAGES]
def defines_plugin(path: Path) -> bool:
    """Whether the assembly file exports a concrete `PlugIn` subclass, read from its metadata and that of the assemblies beside it without loading any."""
    clr.AddReference("System.Reflection.Metadata")
    from System.IO import File
    from System.Reflection import TypeAttributes
    from System.Reflection.Metadata import AssemblyReferenceHandle, HandleKind, PEReaderExtensions, TypeDefinitionHandle, TypeReferenceHandle, TypeSpecificationHandle
    from System.Reflection.PortableExecutable import PEReader

    rhino = clr.GetClrType(PlugIn)

    def exported(reader: object, held: object) -> bool:
        """Whether the type definition is visible outside its assembly."""
        visibility = held.Attributes & TypeAttributes.VisibilityMask
        return visibility == TypeAttributes.Public or (visibility == TypeAttributes.NestedPublic and exported(reader, reader.GetTypeDefinition(held.GetDeclaringType())))

    def defined(reader: object, namespace: str, name: str) -> object:
        """Type definition of the namespace and name, None when the assembly defines none."""
        return next((held for held in map(reader.GetTypeDefinition, reader.TypeDefinitions) if reader.GetString(held.Namespace) == namespace and reader.GetString(held.Name) == name), None)

    def derived(reader: object, base: object) -> bool:
        """Whether the base type resolves to a RhinoCommon type `PlugIn` is assignable from."""
        match base.Kind:
            case HandleKind.TypeDefinition:
                return derived(reader, reader.GetTypeDefinition(TypeDefinitionHandle.op_Explicit(base)).BaseType)
            case HandleKind.TypeSpecification:
                signature = reader.GetBlobReader(reader.GetTypeSpecification(TypeSpecificationHandle.op_Explicit(base)).Signature)
                signature.ReadSignatureTypeCode()
                signature.ReadSignatureTypeCode()
                return derived(reader, signature.ReadTypeHandle())
            case HandleKind.TypeReference:
                held = reader.GetTypeReference(TypeReferenceHandle.op_Explicit(base))
                scope, namespace, name = held.ResolutionScope, reader.GetString(held.Namespace), reader.GetString(held.Name)
                owner = reader.GetString(reader.GetAssemblyReference(AssemblyReferenceHandle.op_Explicit(scope)).Name) if scope.Kind == HandleKind.AssemblyReference else None
                sibling = readers.get(owner)
                definition = None if sibling is None else defined(sibling, namespace, name)
                return (
                    rhino.IsAssignableFrom(rhino.Assembly.GetType(f"{namespace}.{name}"))
                    if owner == rhino.Assembly.GetName().Name
                    else definition is not None and derived(sibling, definition.BaseType)
                )
            case _:
                return False

    images = [PEReader(File.OpenRead(str(file))) for file in (path, *sorted(path.parent.glob("*.dll")))]
    try:
        own, *_ = metadata = [PEReaderExtensions.GetMetadataReader(image) for image in images if image.HasMetadata]
        readers = {reader.GetString(reader.GetAssemblyDefinition().Name): reader for reader in metadata if reader.IsAssembly}
        return any(exported(own, held) and not held.Attributes.HasFlag(TypeAttributes.Abstract) and derived(own, held.BaseType) for held in map(own.GetTypeDefinition, own.TypeDefinitions))
    finally:
        for image in images:
            image.Dispose()


def installed(packages: Iterable[str]) -> dict[str, dict[Path, Guid | None]]:
    """Plug-in files of each installed declared package by id in declared order, each with its plug-in id or None for a Grasshopper 2 library."""
    root = Path(HostUtils.AutoInstallPlugInFolder(currentUser=True))
    folders = {path.relative_to(root).parts[0].casefold(): path for path in (Path(folder.FullName) for folder in HostUtils.GetActivePlugInVersionFolders()) if path.is_relative_to(root)}
    return {
        package: {path: PlugIn.IdFromPath(str(path)) if defines_plugin(path) else None for path in sorted(folder.glob("*.rhp"))}
        for package in packages
        if (folder := folders.get(package.casefold())) is not None
    }


def plugins(held: Mapping[str, Mapping[Path, Guid | None]]) -> Iterator[tuple[str, Path, Guid]]:
    """Package id, file, and plug-in id of each installed Rhino plug-in."""
    return ((package, path, plugin) for package, files in held.items() for path, plugin in files.items() if plugin is not None)


def package_rows(held: Mapping[str, Mapping[Path, Guid | None]]) -> tuple[Row, ...]:
    """Rows of silent package plug-in loads, the marker keeping Rhino from installing each Grasshopper 2 library as a plug-in, the listener's agent settings, and the unwelded edge command settings of the plug-in declaring that command."""
    agents, edges = System.Type.GetType("Rhino.AI.AISettings, RhinoAI", throwOnError=True), "ShowUnweldedEdges"

    def command_settings(plugin: Guid) -> Rhino.PersistentSettings:
        """Settings of the plug-in's unwelded edge command, the plug-in loaded first."""
        PlugIn.LoadPlugIn(plugin)
        return PlugIn.Find(plugin).CommandSettings(edges)

    return (
        *(
            Row(label=f"plugin {package} {path.name} loads silently", read=lambda plugin=plugin: found(PlugIn.GetLoadProtection(plugin)), write=partial(PlugIn.SetLoadProtection, plugin), target=True)
            for package, path, plugin in plugins(held)
        ),
        *(
            Row(label=f"plugin {package} {path.name} grasshopper-only", read=marker.is_file, write=lambda _, marker=marker: marker.touch(), target=True)
            for package, files in held.items()
            for path, plugin in files.items()
            if plugin is None
            for marker in (path.with_name(f"{path.name}.grasshopper-only"),)
        ),
        *(internal_setting(agents, name, target=target) for name, target in (("AutoLoadMCP", True), ("DefaultAgentName", "claude"), ("DisabledAgents", ()))),
        *(
            row
            for package, _, plugin in plugins(held)
            if edges in PlugIn.GetEnglishCommandNames(plugin)
            for settings, label in ((command_settings(plugin), f"plugin {package} {edges}"),)
            for row in (key(settings, "Color", target=color(Status.ERROR), label=label), key(settings, "Thickness", target=1, label=label))
        ),
    )


# --- [DISPLAY]
def mode_value(mode: Guid, read: Callable[[DisplayPipelineAttributes], object]) -> object:
    """Value the read takes from a built-in display mode's attributes."""
    return read(DisplayModeDescription.GetDisplayMode(mode).DisplayAttributes)


def point_width() -> float:
    """Logical width in points a point draws at so it spans the declared device pixels on the primary screen."""
    return POINT_WIDTH / Screen.PrimaryScreen.LogicalPixelSize


def display_rows() -> tuple[Row, ...]:
    """Rows deleting every display mode beside Rhino's own, setting every color of each built-in mode to its role over the mode's canvas or paper ground, configuring the Shaded, X-Ray, and Ghosted modes alike, and listing the shown built-in modes in the menus."""
    stored = Rhino.PersistentSettings.RhinoAppSettings.AddChild(OPTIONS).AddChild("DisplayAttributesManager")
    logical = point_width()
    methods, grip = System.Type.GetType("UnsafeNativeMethods, RhinoCommon", throwOnError=True), internal("UnsafeNativeMethods+DisplayAttributesInt, RhinoCommon", "PCGripSize")
    members, sides = (System.Type.GetType(f"UnsafeNativeMethods+{name}, RhinoCommon", throwOnError=True) for name in ("DisplayAttrsColor", "DisplayAttributesMaterialIdx"))
    emission = internal("UnsafeNativeMethods+DisplayAttrsMaterialColor, RhinoCommon", "Emission")
    wires = internal("UnsafeNativeMethods+DisplayPipelineAttributesBool, RhinoCommon", "SingleMeshWireColor")
    modeling = (DisplayModeDescription.ShadedId, DisplayModeDescription.XRayId, DisplayModeDescription.GhostedId)
    papers = (DisplayModeDescription.PenId, DisplayModeDescription.AmbientOcclusionId)
    rendered = (DisplayModeDescription.RenderedId, DisplayModeDescription.RaytracedId)
    screen_lines = {"TECH_HIDDENLINES": True, "TECH_EDGES": False, "TECH_SILHOUETTES": True, "TECH_CREASES": False, "TECH_SEAMS": False, "TECH_INTERSECTIONS": False}
    attributes = {
        "ShadingEnabled": True,
        "UseCustomObjectMaterial": True,
        "UseCustomObjectColor": True,
        "FrontMaterialShine": 0.0,
        "BackfaceDisplayStyle": DisplayPipelineAttributes.BackfaceStyle.UseFrontFaceSettings,
        "LightingScheme": DisplayPipelineAttributes.LightingSchema.DefaultLighting,
        "CastShadows": False,
        "ShowIsoCurves": False,
        "ShowSurfaceEdges": True,
        "SurfaceEdgeThicknessScale": 1.0,
        "ShowTangentEdges": False,
        "ShowTangentSeams": False,
        "ShowSurfaceNakedEdge": False,
        "MeshSpecificAttributes.ShowMeshWires": False,
        "ShowMeshEdges": True,
        "MeshEdgeColorReduction": 0,
        "MeshEdgeThickness": 1,
        "ShowMeshNakedEdges": False,
        "MeshNakedEdgeThickness": 1,
        "LayersFollowLockUsage": True,
        "ControlPolygonUseSolidLines": True,
        "ControlPolygonStyle": PointStyle.RoundDot,
        "PointStyle": PointStyle.RoundSimple,
        "PointRadius": (logical - 1) / 2,
        "PointCloudStyle": PointStyle.RoundSimple,
        "PointCloudRadius": logical,
        "ShowSubDEdges": False,
        "ShowSubDNonmanifoldEdges": False,
        "SubDCreaseInteriorEdgeThickness": 1.0,
        "SubDBoundaryEdgeThickness": 1.0,
        "SubDBoundaryThicknessScale": 1.0,
        "SubDReflectionAxisLineThickness": 1.0,
        "ViewSpecificAttributes.DrawGrid": True,
        "ViewSpecificAttributes.DrawGridAxes": True,
        "ViewSpecificAttributes.DrawWorldAxes": True,
        "ViewSpecificAttributes.DrawZAxis": False,
        "GridTransparency": 0,
        "UseSectionStyles": False,
        "ClippingEdgeThickness": 1,
    }

    def ground(mode: Guid) -> tuple[int, int, int]:
        """Ground the mode draws on, paper for the drawing modes and the canvas for the rest."""
        return Surface.PAPER if mode in papers else Surface.CANVAS

    def palette(mode: Guid) -> dict[str, tuple[int, int, int]]:
        """Role of each color the mode's store keeps, every line in the ink of its ground, SubD edge colors where their usage draws one color."""
        paper = mode in papers
        ink = Ink.DOCUMENT if paper else Ink.SCREEN
        inked = ("MeshWireColor", "CurveColor", "EdgeColor", "IsoColor", "IsoUColor", "IsoVColor", "MeshEdgeColor", "ClippingEdgeColor")
        subd = ("SubDSmoothInteriorEdgeColor", "SubDCreaseInteriorEdgeColor", "SubDBoundaryEdgeColor") if paper else ()
        defects = ("NakedEdgeColor", "MeshNakedEdgeColor", "MeshNonmanifoldEdgeColor", *(() if mode in rendered else ("SubDNonManifoldEdgeColor",)))
        return {
            **dict.fromkeys((*inked, *subd, "TechnicalLine", "TechnicalEdge", "TechnicalSilhouette", "TechnicalIntersection"), ink),
            **dict.fromkeys(defects, Status.ERROR),
            **dict.fromkeys(("GradTopLeft", "GradBottomLeft", "GradTopRight", "GradBottomRight"), ground(mode)),
            **dict.fromkeys(("SubDReflectionAxisLineColor", "SubDReflectionPlaneColor"), Guide.CONSTRUCTION),
            "WxColor": Axis.X,
            "WyColor": Axis.Y,
            "WzColor": Axis.Z,
            "AmbientColor": Surface.AMBIENT if mode in modeling else Surface.SHADOW,
            "ShadowColor": Surface.SHADOW,
            "ClippingSurfaceColor": Surface.SECTION,
            "ClippingCPColor": Selection.BODY,
            "CPColor": Guide.HANDLE,
            "LockedColor": Line.LOCKED,
            "GridPlaneColor": Line.GRID,
        }

    def drawing(mode: Guid) -> dict[str, object]:
        """Usage flags drawing the ground and routing lines to its ink: object colors on the canvas, which switching turns white, fixed ink on paper, where switching follows the dark canvas."""
        paper = mode in papers
        edges = DisplayPipelineAttributes.SubDEdgeColorUse.SingleColorForAll if paper else DisplayPipelineAttributes.SubDEdgeColorUse.ObjectColor
        return {
            "FillMode": DisplayPipelineAttributes.FrameBufferFillMode.SolidColor if paper else DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor,
            "LinearWorkflowUsage": DisplayPipelineAttributes.LinearWorkflowUsages.Custom,
            "PreProcessColors": False,
            "PreProcessTextures": False,
            "PostProcessFrameBuffer": False,
            "ControlPolygonUseFixedSingleColor": True,
            "UseSingleCurveColor": paper,
            "SurfaceEdgeColorUsage": DisplayPipelineAttributes.SurfaceEdgeColorUse.SingleColorForAll if paper else DisplayPipelineAttributes.SurfaceEdgeColorUse.ObjectColor,
            "SurfaceNakedEdgeColorUsage": DisplayPipelineAttributes.SurfaceNakedEdgeColorUse.SingleColorForAll,
            "SurfaceIsoSingleColor": paper,
            "SurfaceIsoColorsUsed": False,
            "SubDSmoothInteriorEdgeColorUsage": edges,
            "SubDCreaseInteriorEdgeColorUsage": edges,
            "SubDBoundaryEdgeColorUsage": edges,
            "SubDNonManifoldEdgeColorUsage": DisplayPipelineAttributes.SubDEdgeColorUse.SingleColorForAll,
            "ClippingPlaneFillColorUsage": DisplayPipelineAttributes.ClippingPlaneFillColorUse.SolidColor,
            "ClippingEdgeColorUsage": DisplayPipelineAttributes.ClippingEdgeColorUse.SolidColor,
        }

    def native(entry: str, owner: object, *arguments: object) -> object:
        """Result of the named RhinoCommon internal native entry called on the owner's native pointer and the arguments."""
        pointer = owner.GetType().GetMethod("NonConstPointer", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, None)
        return methods.GetMethod(entry, BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([pointer, *arguments]))

    def assign(path: str, held: DisplayPipelineAttributes, value: object) -> None:
        """Set the display attribute member at the dotted path to the value."""
        *owners, name = path.split(".")
        setattr(reduce(getattr, owners, held), name, value)

    def store[T](mode: Guid, write: Callable[[DisplayModeDescription, T], object], value: T) -> None:
        """Write the value into a built-in mode's description and store the mode in the settings file."""
        description = DisplayModeDescription.GetDisplayMode(mode)
        write(description, value)
        DisplayModeDescription.UpdateDisplayMode(description)

    def fill(held: DisplayPipelineAttributes, value: Color) -> None:
        """Set the solid fill color, keeping the fill mode `SetFill` switches to a solid fill."""
        mode = held.FillMode
        held.SetFill(value)
        held.FillMode = mode

    def row[T](mode: Guid, name: str, read: Callable[[DisplayPipelineAttributes], object], write: Callable[[DisplayPipelineAttributes, T], object], *, target: T) -> Row:
        """Row of one member of a built-in mode's attributes."""
        return Row(
            label=f"display mode {DisplayModeDescription.GetDisplayMode(mode).EnglishName} {name}",
            read=partial(mode_value, mode, read),
            write=partial(store, mode, lambda description, value: write(description.DisplayAttributes, value)),
            target=target,
        )

    def member_rows(mode: Guid, values: Mapping[str, object]) -> Iterator[Row]:
        """Rows of the named attribute members at their values."""
        return (row(mode, path, attrgetter(path), partial(assign, path), target=value) for path, value in values.items())

    def mode_rows(mode: Guid) -> Iterator[Row]:
        """Rows of the usage flags of each mode Rhino's own pipeline fills and the Shaded configuration of the modeling modes, Ghosted keeping its see-through material, then of every color the mode's store keeps in its role."""
        if mode not in rendered:
            yield from member_rows(mode, drawing(mode))
            yield row(mode, "SolidColor", lambda display: display.GetFill()[0], fill, target=color(ground(mode)))
            yield row(
                mode,
                "SingleMeshWireColor",
                lambda display: native("CDisplayPipelineAttributes_GetBool", display, wires),
                lambda display, value: native("CDisplayPipelineAttributes_SetBool", display, wires, value),
                target=mode in papers,
            )
            yield from (
                row(
                    mode,
                    f"{kind} fixed color",
                    lambda display, bit=bit: native("CDisplayPipelineAttributes_GetTechnicalUsage", display, bit),
                    lambda display, value, bit=bit: native("CDisplayPipelineAttributes_SetTechnicalUsage", display, bit, value),
                    target=mode in papers or screen,
                )
                for kind, screen in screen_lines.items()
                for bit in (System.UInt32(int(internal("Rhino.Display.DisplayPipelineAttributes+TechnicalModeParameter, RhinoCommon", kind))),)
            )
        if mode in modeling:
            yield from member_rows(mode, {**attributes, "FrontOverrideObjectTransparency": mode == DisplayModeDescription.GhostedId})
            yield row(mode, "PerPixelLightning", partial(native, "CDisplayPipelineAttributes_GetPerPixelLightning"), partial(native, "CDisplayPipelineAttributes_SetPerPixelLightning"), target=True)
            yield row(
                mode,
                "PCGripSize",
                lambda display: native("CDisplayPipelineAttributes_GetInt", display, grip),
                lambda display, value: native("CDisplayPipelineAttributes_SetInt", display, grip, System.Int32(value)),
                target=round((logical - 1) / 2),
            )
        yield from (
            row(
                mode,
                name,
                lambda display, which=which: Color.FromArgb(native("CDisplayPipelineAttributes_GetColor", display, which)),
                lambda display, value, which=which: native("CDisplayPipelineAttributes_SetColor", display, which, value.ToArgb()),
                target=color(rgb),
            )
            for name, rgb in palette(mode).items()
            for which in (System.Enum.Parse(members, name),)
        )
        if mode in modeling or mode_value(mode, attrgetter("UseCustomObjectMaterial")):
            yield from member_rows(mode, {"FrontDiffuse": color(Surface.SHADED)})
        yield from (
            row(
                mode,
                f"{side} Emission",
                lambda display, side=side: Color.FromArgb(native("CDisplayAttributeMaterial_GetColor", display, side, emission)),
                lambda display, value, side=side: native("CDisplayAttributeMaterial_SetColor", display, side, emission, value.ToArgb()),
                target=color(Surface.SHADOW),
            )
            for side in System.Enum.GetValues(sides)
        )

    def rank(mode: DisplayModeDescription) -> object:
        """Order of the mode in the display attributes manager's list, negative for a built-in mode."""
        return native("DisplayAttrsMgrListDesc_Order", mode)

    def custom_modes() -> tuple[str, ...]:
        """Ids of every display mode beside Rhino's own."""
        return tuple(str(mode.Id) for mode in DisplayModeDescription.GetDisplayModes() if rank(mode) >= 0)

    def remove(_: object) -> None:
        """Delete every display mode beside Rhino's own and its stored settings, then save the display modes."""
        for mode in custom_modes():
            DisplayModeDescription.DeleteDisplayMode(Guid.Parse(mode))
            stored.DeleteChild(mode)
        DisplayModeDescription.SaveDisplayModes()

    shown = (DisplayModeDescription.ShadedId, DisplayModeDescription.WireframeId, DisplayModeDescription.XRayId, DisplayModeDescription.RenderedId, DisplayModeDescription.RaytracedId)
    built_in = tuple(mode.Id for mode in DisplayModeDescription.GetDisplayModes() if rank(mode) < 0)
    return (
        Row(label="custom display modes", read=custom_modes, write=remove, target=()),
        *(each for mode in built_in for each in mode_rows(mode)),
        *(
            Row(
                label=f"display mode {DisplayModeDescription.GetDisplayMode(mode).EnglishName} in menu",
                read=lambda mode=mode: DisplayModeDescription.GetDisplayMode(mode).InMenu,
                write=partial(store, mode, lambda description, value: setattr(description, "InMenu", value)),
                target=mode in shown,
            )
            for mode in (*shown, *(mode for mode in built_in if mode not in shown))
        ),
    )


# --- [MEASURES]
def docks(doc: Rhino.RhinoDoc) -> dict[Site, object]:
    """Dock site of the document's main window at each location."""
    sites = System.Type.GetType("Rhino.UI.Internal.TabPanels.TabPanelDockSites, Rhino.UI", throwOnError=True)
    docked = sites.GetMethod("FromDocument", Array[System.Type]([clr.GetClrType(Rhino.RhinoDoc)])).Invoke(None, Array[System.Object]([doc]))
    return {site: getattr(docked, site) for site in Site}


def extents(doc: Rhino.RhinoDoc) -> dict[Site | Extent, float]:
    """Lengths in points and row and toggle counts the file edit sizes from, each panel closed again after its measure when it was closed."""
    panels, (head, *_) = Rhino.UI.Panels, RIGHT_TOP
    bars, tab_panels, toolbar_settings, tabs, resizer, layer_grid, layout_grid = (
        System.Type.GetType(f"Rhino.UI.{name}, Rhino.UI", throwOnError=True)
        for name in (
            "Internal.TabPanels.TabPanelDockBars",
            "Internal.TabPanels.TabPanelSettings",
            "Internal.TabPanels.ToolbarSettings",
            "Internal.TabPanels.Controls.BaseTabControl",
            "Internal.TabPanels.Controls.DockSiteResizer",
            "DialogPanels.LayerTreeGridView",
            "DialogPanels.LayoutTreeGridView",
        )
    )
    serial = System.UInt32(doc.RuntimeSerialNumber)

    def container(bar: Guid) -> object:
        """Dock bar's control at the size its band gives it."""
        return bars.GetMethod("FromDockBarId").Invoke(None, Array[System.Object]([bar])).HasContent(serial)

    def contained(control: object, kind: object) -> object:
        """First descendant control of the kind."""
        return next(each for each in control.Children if kind.IsInstanceOfType(each))

    def inset(shell: object, grid: object) -> float:
        """Width the container spends beside the tree grid's visible columns."""
        outline = grid.ControlObject
        columns = tuple(outline.TableColumns())
        shown = [index for index, column in enumerate(columns) if not column.Hidden]
        span = outline.RectForColumn(System.IntPtr(max(shown))).Right.Value - sum(columns[index].Width.Value for index in shown)
        return shell.Size.Width - outline.EnclosingScrollView.ContentView.Frame.Size.Width.Value + span

    def private(owner: object, kind: object, name: str) -> object:
        """Value of the private instance field the type declares on the owner."""
        return kind.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner)

    def layers(panel: object, shell: object) -> dict[Site | Extent, float]:
        """Measures of the Layers container and its tree."""
        grid = contained(panel, layer_grid)
        outline = grid.ControlObject
        return {
            Extent.LAYERS_CHROME: shell.Size.Height - grid.Size.Height,
            Extent.LAYERS_HEADER: outline.HeaderView.Frame.Size.Height.Value,
            Extent.LAYERS_ROW: outline.RowHeight.Value + outline.IntercellSpacing.Height.Value,
            Extent.LAYERS_INSET: inset(shell, grid),
        }

    def materials(panel: object, _: object) -> dict[Site | Extent, float]:
        """Material editor's strip above its list and its row pitch."""
        editor = panel.GetType().BaseType
        thumbnails = private(panel, editor, "m_thumbnail_list")
        thumbview = private(thumbnails, thumbnails.GetType(), "m_thumbview")
        return {
            Extent.MATERIALS_STRIP: private(panel, editor, "m_thumb_list_table").Spacing.Height + private(panel, editor, "m_view_mode_button_table").GetPreferredSize().Height,
            Extent.MATERIALS_ROW: thumbnails.ViewModel.ThumbHeigth + private(thumbview, thumbview.GetType(), "m_space_between_items"),
        }

    def libraries(panel: object, shell: object) -> dict[Site | Extent, float]:
        """Measures of the Libraries container, its folder tree, and its list."""
        splitter, tree = (contained(panel, System.Type.GetType(f"Eto.Forms.{name}, Eto", throwOnError=True)) for name in ("Splitter", "TreeGridView"))
        folders = tree.ControlObject
        return {
            Extent.LIBRARIES_CHROME: shell.Size.Height - (splitter.Height - splitter.SplitterWidth),
            Extent.LIBRARIES_BORDER: tree.Height - folders.EnclosingScrollView.ContentView.Frame.Size.Height.Value,
            Extent.LIBRARIES_ROW: folders.RowHeight.Value + folders.IntercellSpacing.Height.Value,
            Extent.LIBRARIES_FOLDERS: folders.RowCount.ToInt64(),
            Extent.LIBRARIES_LIST_MINIMUM: splitter.Panel2MinimumSize,
        }

    top, window = panels.PanelDockBar(Guid.Parse(str(head))), Rhino.UI.RhinoEtoApp.MainWindowForDocument(doc).ControlObject.ContentView
    homes = {panel: panels.PanelDockBar(Guid.Parse(str(panel))) for panel in (Panel.LAYERS, Panel.LAYOUTS, Panel.MATERIALS, Panel.LIBRARIES)}

    def selected(panel: Panel, measure: Callable[[object, object], dict[Site | Extent, float]]) -> dict[Site | Extent, float]:
        """Measures of the panel laid out as its container's selected tab, a background tab holding its last layout's frames."""
        guid = Guid.Parse(str(panel))
        panels.OpenPanel(top if homes[panel] == Guid.Empty else homes[panel], guid, makeSelectedPanel=True)
        window.LayoutSubtreeIfNeeded()
        return measure(panels.GetPanel(guid, doc), container(panels.PanelDockBar(guid)))

    try:
        window.LayoutSubtreeIfNeeded()
        width, style, toolbar = (kind.GetProperty(name).GetValue(None) for kind, name in ((resizer, "ResizerWidth"), (tab_panels, "HorizontalDisplayStyle"), (toolbar_settings, "Instance")))
        osnap = panels.GetPanel(Guid.Parse(str(Panel.OSNAP)), doc)
        grid = contained(osnap, System.Type.GetType("Rhino.UI.Controls.ControlGridLayout, Rhino.UI", throwOnError=True))
        pitch = grid.ItemSize + grid.ItemPadding.Size
        return {
            **{site: owner.Control.Size.Height for site, owner in docks(doc).items()},
            Extent.TAB_STRIP: tabs.GetMethod("CalculateTabHeight", BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([style])),
            Extent.BUTTON: toolbar.Buttons.TotalButtonSize,
            Extent.RESIZER: width,
            Extent.OSNAP_TOGGLES: len(grid.Items),
            Extent.OSNAP_PITCH_X: pitch.Width,
            Extent.OSNAP_PITCH_Y: pitch.Height,
            Extent.OSNAP_INSET: osnap.Padding.Horizontal,
            Extent.OSNAP_CHROME: container(panels.PanelDockBar(Guid.Parse(str(Panel.OSNAP)))).Size.Height - osnap.Size.Height + osnap.GetPreferredSize().Height - grid.Rows * pitch.Height,
            **selected(Panel.LAYERS, layers),
            **selected(Panel.LAYOUTS, lambda panel, shell: {Extent.LAYOUTS_INSET: inset(shell, contained(panel, layout_grid))}),
            **selected(Panel.MATERIALS, materials),
            **selected(Panel.LIBRARIES, libraries),
        }
    finally:
        for panel in (panel for panel, home in homes.items() if home == Guid.Empty):
            panels.ClosePanel(Guid.Parse(str(panel)), doc)
        panels.OpenPanel(top, Guid.Parse(str(head)), makeSelectedPanel=True)


def shifts(doc: Rhino.RhinoDoc, measured: Mapping[Site | Extent, float]) -> dict[Site, float]:
    """Length each dock site grows by across its dock axis once its first band takes its settled size, zero for a site the file edit leaves."""
    sizes, resizer, private = bands(measured), measured[Extent.RESIZER], BindingFlags.Instance | BindingFlags.NonPublic
    return {
        site: sum(band.Size + resizer for band in tuple(owner.GetType().GetProperty("Bands", private).GetValue(owner))[1:]) + sizes[site] + resizer - owner.Control.DockSiteSize
        if site in sizes
        else 0.0
        for site, owner in docks(doc).items()
    }


# --- [COMPOSITION] ----------------------------------------------------------------------


def ready(address: str, port: int) -> None:
    """Send Rhino's process id over one connection to the host listening at the address and port."""
    client, sent = TcpClient(address, port), Encoding.ASCII.GetBytes(str(System.Environment.ProcessId))
    try:
        client.GetStream().Write(sent, 0, sent.Length)
    finally:
        client.Dispose()


def applied(doc: Rhino.RhinoDoc, packages: Sequence[str]) -> Iterator[Row | str]:
    """Every row in write order and the report lines the file edit reads, Rhino's Shaded mode configured before the template's views take it and the Grasshopper 2 rows sized from the measured extents."""
    held = installed(packages)
    columns, kind = (System.Type.GetType(f"Rhino.UI.DialogPanels.{name}, Rhino.UI", throwOnError=True) for name in ("LayerColumns", "LayerColumns+ColumnType"))
    search = System.Type.GetType("Rhino.UI.Internal.RuiIo.RuiFile, Rhino.UI", throwOnError=True).GetMethod("RuiFileNameFromPlugIn", BindingFlags.Static | BindingFlags.NonPublic)
    yield from (row for step in (settings_rows, theme_rows, color_rows, command_rows, partial(package_rows, held), display_rows, template.rows, panel_rows) for row in step())
    measured = extents(doc)
    widths = {str(column): columns.GetMethod("DefaultWidth").Invoke(None, Array[System.Object]([column])) for column in System.Enum.GetValues(kind)}
    yield from configuration.rows(doc, point_width(), shifts(doc, measured))
    toolbars = [[package, search.Invoke(None, Array[System.Object]([str(path)]))] for package, path, _ in plugins(held)]
    yield line(Kind.MEASUREMENT, json.dumps({"measures": {**measured, **widths}, "plugins": toolbars}))
    yield from (line(Kind.SKIP, package) for package in packages if package not in held)
    yield from (() if stocked(MATERIALS) else (line(Kind.SKIP, str(MATERIALS)),))


def main(doc: Rhino.RhinoDoc, packages: Sequence[str]) -> None:
    """Converge and report every row, then flush the settings on every path."""
    try:
        emit(applied(doc, packages))
    finally:
        PlugIn.FlushSettingsSavedQueue()


def release() -> None:
    """Drop the unsaved edits of untitled Rhino and Grasshopper 2 documents and report an error naming each titled one with unsaved edits."""
    grasshopper = import_module("Grasshopper2.Doc").Document.AllDocuments if PlugIn.GetPlugInInfo(PlugIn.IdFromName("Grasshopper2")).IsLoaded else ()
    for each in [each for each in Rhino.RhinoDoc.OpenDocuments() if each.Modified and not each.Path]:
        each.Modified = False
    for each in [each for each in grasshopper if each.Modified and not each.File.Path]:
        each.Unmodify()
    names = (*(f"Rhino document {each.Path}" for each in Rhino.RhinoDoc.OpenDocuments() if each.Modified), *(f"Grasshopper 2 document {each.File.Path}" for each in grasshopper if each.Modified))
    emit(line(Kind.ERROR, f"{name} holds unsaved edits. Save or close it and rerun") for name in names)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main", "ready", "release"]
