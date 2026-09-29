# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type, unsupported-operator]
# mypy: disable-error-code="import-not-found, import-untyped, call-overload, misc, operator"
# ruff: file-ignore[import-outside-top-level]
"""Rhino's application settings with the Alerter, Rhino Render, and default renderer rows."""

from functools import partial
import math

import clr
from Foundation import NSArray, NSString, NSUserDefaults, NSUserDefaultsType
from Rhino import AntialiasLevel
from Rhino.ApplicationSettings import (
    AppearanceSettings,
    ChooseOneObjectSettings,
    ClipboardState,
    CursorTooltipSettings,
    FileSettings,
    GeneralSettings,
    GumballSettings,
    MiddleMouseMode,
    ModelAidSettings,
    MouseSelectMode,
    OpenGLSettings,
    OsnapModes,
    SelectionFilterSettings,
    SmartTrackSettings,
    ViewSettings,
)
from Rhino.DocObjects import ObjectType
from Rhino.PlugIns import PlugIn
from Rhino.Render import Utilities
import System
from System import Array, String
from System.Globalization import CultureInfo
from System.Reflection import BindingFlags

from interface.report import Row
from interface.rhino.script.accessors import Internal, key, member, opened
from interface.roles import Accent, TEXT_POINTS
from interface.units import ANGLE_STEP

# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> tuple[Row, ...]:
    """Rows of the application settings, then of the Alerter, Rhino Render, and default renderer."""
    clr.AddReference("RhinoCyclesCore")
    from Commands.Commands.Alerter import AlerterCommand
    from RhinoCyclesCore.Core import RcCore

    defaults, monitor = NSUserDefaults.StandardUserDefaults, NSUserDefaults("com.mcneel.rhinoceros.RhinoMonitor", NSUserDefaultsType.SuiteName)
    english, languages = CultureInfo(1033), "AppleLanguages"
    toolbar = Internal.TOOLBAR_SETTINGS.type.GetProperty("Instance").GetValue(None)
    glyph, tool_row, ribbon, options, standard = 18, 24, Internal.TAB_CONTROL_DISPLAY_STYLE.parsed("Text"), "Options", "StandardUserDefaults"
    tab_icon = max(
        Internal.TAB_PANEL_SETTINGS.type.GetProperty("MinimumToolBarImageSize").GetValue(None),
        Internal.BASE_TAB_CONTROL.type.GetMethod("CalculateTabHeight", BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([ribbon]))
        - 2 * Internal.BASE_TAB_CONTROL_ITEM.type.GetProperty("ItemPadding").GetValue(None).Height,
    )
    general, advanced, mouse, appearance = ((options, name) for name in ("General", "Advanced", "Mouse", "Appearance"))
    text = clr.GetClrType(String)
    _, system_family = opened(appearance).TryGetDefault.Overloads[text, text.MakeByRefType()]("CommandPromptFontName")

    def prompt_font(family: str) -> None:
        """Set the command prompt font family through the appearance state the native owner reads."""
        state = AppearanceSettings.GetCurrentState()
        state.CommandPromptFontName = family
        AppearanceSettings.UpdateFromState(state)

    return (
        *(member(ViewSettings, name, target=True) for name in ("RotateViewAroundObjectAtMouseCursor", "AlwaysPanParallelViews", "PanPlanParallelViewsWithControlShiftRMB", "AutoAdjustTargetDepth")),
        *(member(ViewSettings, name, target=False) for name in ("RotateViewAroundAutogumball", "SingleClickMaximize", "LinkedViewports")),
        member(ViewSettings, "ZoomScale", target=1 / math.sqrt(1.2)),
        member(ViewSettings, "ViewRotation", target=ViewSettings.ViewRotationStyle.RotateAroundWorldAxes),
        member(ViewSettings, "RotateCircleIncrement", target=360 // ANGLE_STEP),
        member(GeneralSettings, "MiddleMouseMode", target=MiddleMouseMode.PopupToolbar),
        member(GeneralSettings, "MiddleMousePopupToolbar", target="Popup"),
        member(GeneralSettings, "MouseSelectMode", target=MouseSelectMode.Combo),
        member(GeneralSettings, "MinimumUndoSteps", target=100),
        member(GeneralSettings, "MaximumUndoMemoryMb", target=4096),
        member(GeneralSettings, "EnableContextMenu", target=True),
        member(GeneralSettings, "ContextMenuDelay", target=System.TimeSpan(0, 0, 0, 0, 300)),
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
        key(("Warnings",), "MissingFontWarning", target=False),
        member(PlugIn, "AskOnLoadProtection", target=False),
        key((options, "PackageManager"), "CheckForUpdates", target=False),
        key((options, "FileSettings"), "AutoSaveVersionsEnabled", target=True),
        *(key(mouse, name, target=False) for name in ("EnableUnselectedObjectDrag", "EnableMouseScrollBallRotation", "EnableMagicMouseRotation", "DisableRightClickAsEnter")),
        *(key(mouse, name, target=True) for name in ("EnableUnselectedGripDrag", "EnableMagicMouseGestures")),
        key(mouse, "MouseButton4Macro", target="'_Zoom _Selected"),
        member(AppearanceSettings, "LanguageIdentifier", target=english.LCID),
        member(AppearanceSettings, "HelpLanguageIdentifier", target=0),
        *(
            member(AppearanceSettings, name, target=True)
            for name in ("EchoCommandsToHistoryWindow", "EchoPromptsToHistoryWindow", "ShowViewportTitles", "ShowCrosshairs", "ShowCursorWhenCrosshairsVisible", "ShowOsnapBar")
        ),
        member(AppearanceSettings, "ShowLayoutDropShadow", target=False),
        member(AppearanceSettings, "CommandPromptFontSize", target=TEXT_POINTS * 10),
        Row(label="AppearanceSettings.CommandPromptFontName", read=lambda: AppearanceSettings.GetCurrentState().CommandPromptFontName, write=prompt_font, target=system_family),
        *(key(appearance, name, target=True) for name in ("AutocompleteCommands", "FuzzyAutocomplete", "ShowStatusbar", "AlwaysShowGeneralObjectProperties", "ShowSideBar")),
        key(appearance, "StatusbarInfoPaneMode", target=int(Internal.STATUS_BAR_INFO_PANE_MODE.parsed("selected_object_count"))),
        key(appearance, "DirectionArrowThickness", target=2),
        member(FileSettings, "ClipboardOnExit", target=ClipboardState.DeleteData),
        *(member(FileSettings, name, target=False) for name in ("FileLockingOpenWarning", "CreateOtherBackupFiles", "SaveViewChanges")),
        member(OpenGLSettings, "AntialiasLevel", target=AntialiasLevel.Good),
        *(member(GumballSettings, name, target=True) for name in ("EnableGumball", "SnappyGumball", "MergeFacesAfterExtrude")),
        *(member(GumballSettings, name, target=2) for name in ("AxisThickness", "ArcThickness")),
        *(member(SmartTrackSettings, name, target=True) for name in ("UseSmartTrack", "SmartOrtho", "Parallels", "UseDottedLines")),
        key((options, "SmartTrack"), "MaximumSmartPoints", target=4),
        *(member(ModelAidSettings, name, target=True) for name in ("Osnap", "Ortho", "ProjectToCPlaneInPlanParallelViews", "DragStartsWindowSelection", "AltPlusArrow")),
        *(member(ModelAidSettings, name, target=False) for name in ("GridSnap", "Planar", "ProjectSnapToCPlane", "SnapToFiltered", "ExtendToApparentIntersection")),
        member(
            ModelAidSettings,
            "OsnapModes",
            target=OsnapModes.End | OsnapModes.Point | OsnapModes.Midpoint | OsnapModes.Center | OsnapModes.Intersection | OsnapModes.Perpendicular | OsnapModes.Quadrant,
        ),
        member(ModelAidSettings, "OrthoAngle", target=math.radians(6 * ANGLE_STEP)),
        member(ModelAidSettings, "NudgeMode", target=1),
        *(member(ChooseOneObjectSettings, name, target=True) for name in ("ShowObjectLayer", "ShowObjectTypeDetails", "ShowAllOption")),
        member(ChooseOneObjectSettings, "ShowTitlebarAndBorder", target=False),
        member(SelectionFilterSettings, "GlobalGeometryFilter", target=ObjectType.AnyObject),
        member(SelectionFilterSettings, "Enabled", target=SelectionFilterSettings.GetDefaultState().Enabled),
        *(member(CursorTooltipSettings, name, target=True) for name in ("TooltipsEnabled", "DistancePane")),
        key((options, "Grid"), "AxisLineWidth", target=1),
        *(
            Internal.TAB_PANEL_SETTINGS.setting(name, target=target)
            for name, target in (
                ("LockDockedWindows", True),
                ("TabIconSize", tab_icon),
                ("ToolBarImageSize", glyph),
                ("HorizontalDisplayStyle", ribbon),
                ("VerticalDisplayStyle", Internal.TAB_CONTROL_DISPLAY_STYLE.parsed("Bitmap")),
                ("FloatingDisplayStyle", Internal.TAB_CONTROL_DISPLAY_STYLE.parsed("Bitmap")),
                ("HideSingleToolBarTab", True),
                ("CascadeDelay", 400),
                ("ForceVerticalToTop", False),
                ("UseIconsInStatusBar", False),
            )
        ),
        *(
            member(toolbar.Buttons, name, target=target)
            for name, target in (
                ("PanelButtonSize", max(toolbar.Buttons.PanelButtonSizesMinimum, min(toolbar.Buttons.PanelButtonSizesMaximum, tab_icon))),
                ("ButtonPadding", (tool_row - glyph) // 2),
                ("SpacerSize", 5),
                ("Cascade", Internal.CASCADE_STYLE.parsed("AsPanel")),
                ("MiddleMouseDelay", 400),
            )
        ),
        *(member(toolbar.ToolTips, name, target=True) for name in ("IncludeShortcut", "IncludeAlias")),
        key((), "OSnapButtonDisplay", target=int(Internal.OSNAP_BUTTON_DISPLAY.parsed("IconAndText"))),
        key((), "SelectionFilterButtonDisplay", target=int(Internal.SELECTION_FILTER_BUTTON_DISPLAY.parsed("IconAndText"))),
        *(
            key((), name, target=max(tab_icon, display.type.DeclaringType.GetField("MinIconSize", BindingFlags.Static | BindingFlags.NonPublic).GetValue(None)))
            for name, display in (("OSnapIconSize", Internal.OSNAP_BUTTON_DISPLAY), ("SelectionFilterIconSize", Internal.SELECTION_FILTER_BUTTON_DISPLAY))
        ),
        *(key((), name, target=True) for name in ("OSnapStretchButtons", "SelectionFilterUseCheckedColor", "SelectionFilterStretchButtons")),
        key((), "AnnotationSpellCheck", target=False),
        key(("PropertiesEditor", options), "DisplayPagesOnIdle", target=True),
        Internal.RUNTIME_SETTINGS.setting("NotesRestoreCursorPosition", target=False),
        *(
            Row(label=f'{domain}["{name}"]', read=partial(read, name), write=lambda value, write=write, name=name: write(value, name), target=target)
            for domain, read, write, name, target in (
                (standard, defaults.BoolForKey, defaults.SetBool, "AppleReduceDesktopTinting", True),
                (standard, defaults.BoolForKey, defaults.SetBool, "SUAutomaticallyUpdate", False),
                (standard, lambda name: defaults.IntForKey(name).ToInt64(), lambda value, name: defaults.SetInt(System.IntPtr(value), name), "AppleAccentColor", 4),
                (standard, defaults.StringForKey, defaults.SetString, "AppleHighlightColor", " ".join((*(f"{channel / 255:.6f}" for channel in Accent.TEXT_SELECTED), "Other"))),
                (standard, defaults.StringForKey, defaults.SetString, "MRLanguage", english.Parent.Name),
                ("RhinoMonitor", monitor.BoolForKey, monitor.SetBool, "MRShouldIncludeModelFileInReport", False),
            )
        ),
        Row(
            label=f'{standard}["{languages}"]',
            read=partial(defaults.StringArrayForKey, languages),
            write=lambda value: defaults.SetValueForKey(NSArray.FromStrings(Array[String](list(value))), NSString(languages)),
            target=(english.Parent.Name,),
        ),
        member(AlerterCommand.Instance, "Enabled", target=False),
        *(member(RcCore.It.AllSettings, name, target=target) for name, target in (("ThrottleMs", 100), ("SelectedDeviceStr", "-1"), ("IntermediateSelectedDeviceStr", "-1"), ("PixelSize", 1))),
        Row(label="Utilities.DefaultRenderPlugInId", read=lambda: Utilities.DefaultRenderPlugInId, write=Utilities.SetDefaultRenderPlugIn, target=PlugIn.IdFromName("Rhino Render")),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
