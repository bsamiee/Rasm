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

from interface.render import LIGHT_TREE
from interface.report import Row, single
from interface.rhino.script.accessors import Internal, key, member, opened, preference
from interface.rhino.script.template import SUN_LIGHT_FACTOR
from interface.roles import Accent, TEXT_POINTS
from interface.units import ANGLE_STEP

# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> tuple[Row, ...]:
    """Rows of each owner's members, the Alerter's and Rhino Render's among them, and each settings path's keys, then the internal, user default, and default renderer rows."""
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

    members = {
        ViewSettings: {
            **dict.fromkeys(("RotateViewAroundObjectAtMouseCursor", "AlwaysPanParallelViews", "PanPlanParallelViewsWithControlShiftRMB", "AutoAdjustTargetDepth"), True),
            **dict.fromkeys(("RotateViewAroundAutogumball", "SingleClickMaximize", "LinkedViewports"), False),
            "ZoomScale": 1 / math.sqrt(1.2),
            "ViewRotation": ViewSettings.ViewRotationStyle.RotateAroundWorldAxes,
            "RotateCircleIncrement": 360 // ANGLE_STEP,
        },
        GeneralSettings: {
            "MiddleMouseMode": MiddleMouseMode.PopupToolbar,
            "MiddleMousePopupToolbar": "Popup",
            "MouseSelectMode": MouseSelectMode.Combo,
            "MinimumUndoSteps": 100,
            "MaximumUndoMemoryMb": 4096,
            "EnableContextMenu": True,
            "ContextMenuDelay": System.TimeSpan(0, 0, 0, 0, 300),
        },
        PlugIn: {"AskOnLoadProtection": False},
        AppearanceSettings: {
            "LanguageIdentifier": english.LCID,
            "HelpLanguageIdentifier": 0,
            **dict.fromkeys(("EchoCommandsToHistoryWindow", "EchoPromptsToHistoryWindow", "ShowViewportTitles", "ShowCrosshairs", "ShowCursorWhenCrosshairsVisible", "ShowOsnapBar"), True),
            "ShowLayoutDropShadow": False,
            "CommandPromptFontSize": TEXT_POINTS * 10,
        },
        FileSettings: {"ClipboardOnExit": ClipboardState.DeleteData, **dict.fromkeys(("FileLockingOpenWarning", "CreateOtherBackupFiles", "SaveViewChanges"), False)},
        OpenGLSettings: {"AntialiasLevel": AntialiasLevel.Good},
        GumballSettings: {**dict.fromkeys(("EnableGumball", "SnappyGumball", "MergeFacesAfterExtrude"), True), **dict.fromkeys(("AxisThickness", "ArcThickness"), 2)},
        SmartTrackSettings: dict.fromkeys(("UseSmartTrack", "SmartOrtho", "Parallels", "UseDottedLines"), True),
        ModelAidSettings: {
            **dict.fromkeys(("Osnap", "Ortho", "ProjectToCPlaneInPlanParallelViews", "DragStartsWindowSelection", "AltPlusArrow"), True),
            **dict.fromkeys(("GridSnap", "Planar", "ProjectSnapToCPlane", "SnapToFiltered", "ExtendToApparentIntersection"), False),
            "OsnapModes": OsnapModes.End | OsnapModes.Point | OsnapModes.Midpoint | OsnapModes.Center | OsnapModes.Intersection | OsnapModes.Perpendicular | OsnapModes.Quadrant,
            "OrthoAngle": math.radians(6 * ANGLE_STEP),
            "NudgeMode": 1,
        },
        ChooseOneObjectSettings: {**dict.fromkeys(("ShowObjectLayer", "ShowObjectTypeDetails", "ShowAllOption"), True), "ShowTitlebarAndBorder": False},
        SelectionFilterSettings: {"GlobalGeometryFilter": ObjectType.AnyObject, "Enabled": SelectionFilterSettings.GetDefaultState().Enabled},
        CursorTooltipSettings: dict.fromkeys(("TooltipsEnabled", "DistancePane"), True),
        toolbar.Buttons: {
            "PanelButtonSize": max(toolbar.Buttons.PanelButtonSizesMinimum, min(toolbar.Buttons.PanelButtonSizesMaximum, tab_icon)),
            "ButtonPadding": (tool_row - glyph) // 2,
            "SpacerSize": 5,
            "Cascade": Internal.CASCADE_STYLE.parsed("AsPanel"),
            "MiddleMouseDelay": 400,
        },
        toolbar.ToolTips: dict.fromkeys(("IncludeShortcut", "IncludeAlias"), True),
        AlerterCommand.Instance: {"Enabled": False},
        RcCore.It.AllSettings: {
            "ThrottleMs": 100,
            "SelectedDeviceStr": "-1",
            "IntermediateSelectedDeviceStr": "-1",
            "PixelSize": 1,
            "StartGpuKernelCompiler": True,
            "UseLightTree": LIGHT_TREE,
            "SunLightFactor": single(SUN_LIGHT_FACTOR),
        },
    }
    keys: dict[tuple[str, ...], dict[str, bool | int | str]] = {
        general: {
            **dict.fromkeys(("MiddleMousePlainButtonRotateMode", "MiddleMouseViewManipulationMode", "EnableTrackpadScrolling", "MouseOverHighlight", "SilhouetteHighlighting"), True),
            **dict.fromkeys(("MiddleMouseShiftControlSwap", "UsageStatisticsEnabled"), False),
            "SilhouetteThickness": 3,
        },
        advanced: {
            **dict.fromkeys(("DisableModelAndPageUnitsDifferDialog", "DisablePageUnitsNotInchesOrMMDialog", "UseEtoCommandUI"), True),
            **dict.fromkeys(
                (
                    "EnableCheckForUpdates",
                    "MacDisplayOldVersionAutosaveWarning",
                    "DisplayNonOriginModelBasepointWarning",
                    "UseCompressionWhenSaving",
                    "AllowUnadornedShortcuts",
                    "NotesUseSpacesForTabs",
                ),
                False,
            ),
            "NotesTabWidth": 8,
        },
        ("Warnings",): {"MissingFontWarning": False},
        (options, "PackageManager"): {"CheckForUpdates": False},
        (options, "FileSettings"): {"AutoSaveVersionsEnabled": True},
        mouse: {
            **dict.fromkeys(("EnableUnselectedObjectDrag", "EnableMouseScrollBallRotation", "EnableMagicMouseRotation", "DisableRightClickAsEnter"), False),
            **dict.fromkeys(("EnableUnselectedGripDrag", "EnableMagicMouseGestures"), True),
            "MouseButton4Macro": "'_Zoom _Selected",
        },
        appearance: {
            **dict.fromkeys(("AutocompleteCommands", "FuzzyAutocomplete", "ShowStatusbar", "AlwaysShowGeneralObjectProperties", "ShowSideBar"), True),
            "StatusbarInfoPaneMode": int(Internal.STATUS_BAR_INFO_PANE_MODE.parsed("selected_object_count")),
            "DirectionArrowThickness": 2,
        },
        (options, "SmartTrack"): {"MaximumSmartPoints": 4},
        (options, "Grid"): {"AxisLineWidth": 1},
        (): {
            "OSnapButtonDisplay": int(Internal.OSNAP_BUTTON_DISPLAY.parsed("IconAndText")),
            "SelectionFilterButtonDisplay": int(Internal.SELECTION_FILTER_BUTTON_DISPLAY.parsed("IconAndText")),
            **{
                name: min(max(tab_icon, display.type.DeclaringType.GetField("MinIconSize", BindingFlags.Static | BindingFlags.NonPublic).GetValue(None)), 32)
                for name, display in (("OSnapIconSize", Internal.OSNAP_BUTTON_DISPLAY), ("SelectionFilterIconSize", Internal.SELECTION_FILTER_BUTTON_DISPLAY))
            },
            **dict.fromkeys(("OSnapStretchButtons", "SelectionFilterUseCheckedColor", "SelectionFilterStretchButtons"), True),
            "AnnotationSpellCheck": False,
        },
        ("PropertiesEditor", options): {"DisplayPagesOnIdle": True},
    }
    return (
        *(member(owner, name, target=target) for owner, targets in members.items() for name, target in targets.items()),
        *(key(path, name, target=target) for path, targets in keys.items() for name, target in targets.items()),
        preference(label="AppearanceSettings.CommandPromptFontName", read=lambda: AppearanceSettings.GetCurrentState().CommandPromptFontName, write=prompt_font, target=system_family),
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
        Internal.RUNTIME_SETTINGS.setting("NotesRestoreCursorPosition", target=False),
        *(
            preference(label=f'{domain}["{name}"]', read=partial(read, name), write=lambda value, write=write, name=name: write(value, name), target=target)
            for domain, read, write, name, target in (
                (standard, defaults.BoolForKey, defaults.SetBool, "AppleReduceDesktopTinting", True),
                (standard, defaults.BoolForKey, defaults.SetBool, "SUAutomaticallyUpdate", False),
                (standard, lambda name: defaults.IntForKey(name).ToInt64(), lambda value, name: defaults.SetInt(System.IntPtr(value), name), "AppleAccentColor", 4),
                (standard, defaults.StringForKey, defaults.SetString, "AppleHighlightColor", " ".join((*(f"{channel / 255:.6f}" for channel in Accent.TEXT_SELECTED), "Other"))),
                (standard, defaults.StringForKey, defaults.SetString, "MRLanguage", english.Parent.Name),
                ("RhinoMonitor", monitor.BoolForKey, monitor.SetBool, "MRShouldIncludeModelFileInReport", False),
            )
        ),
        preference(
            label=f'{standard}["{languages}"]',
            read=partial(defaults.StringArrayForKey, languages),
            write=lambda value: defaults.SetValueForKey(NSArray.FromStrings(Array[String](list(value))), NSString(languages)),
            target=(english.Parent.Name,),
        ),
        preference(label="Utilities.DefaultRenderPlugInId", read=lambda: Utilities.DefaultRenderPlugInId, write=Utilities.SetDefaultRenderPlugIn, target=PlugIn.IdFromName("Rhino Render")),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
