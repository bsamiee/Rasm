# ty: ignore[unresolved-import, redundant-condition-strict]
# mypy: disable-error-code="import-untyped"
"""Rhino's theme keys and appearance colors in their color roles."""

from collections.abc import Mapping
from functools import partial

from Rhino.ApplicationSettings import (
    AppearanceSettings,
    ChooseOneObjectSettings,
    CursorTooltipSettings,
    CurvatureGraphSettings,
    DirectionAnalysisSettings,
    EdgeAnalysisSettings,
    GumballSettings,
    SmartTrackSettings,
    SoftTransformSettings,
    WidgetColor,
    ZebraAnalysisSettings,
)
from System.Drawing import Color

from interface.report import Row
from interface.rhino.script.accessors import action, color, found, hex_color, key, labeled, located, member, opened, preference
from interface.roles import Accent, Alpha, Axis, blend, Guide, Ink, Line, Selection, Status, Surface, SWATCHES, Text

# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> tuple[Row, ...]:
    """Rows of each theme key a Rhino control draws and each other color key by its settings path, every other theme key holding a color deleted, then of each appearance color by its owner, an undrawn member at its drawn sibling's color."""
    path = ("UI", "ThemeSettings")
    uniform = {
        "Text.Disabled": color(Text.DISABLED),
        "Button.Enabled.Background": color(Surface.FIELD),
        "Button.Enabled.Border": color(Line.BORDER),
        "Button.Enabled.Text": color(Text.PRIMARY),
        "Button.EnabledHover.Background": color(Surface.HOVER),
        "Button.EnabledHover.Text": color(Text.PRIMARY),
        "Button.EnabledPressed.Border": color(Surface.FIELD),
        "Button.Checked.Background": color(Accent.CONTROL_PRESSED),
        "Button.Checked.Text": color(Text.PRIMARY),
        "Button.CheckedHover.Background": color(Accent.CONTROL_PRESSED),
        "Button.CheckedPressed.Background": color(Accent.CONTROL_PRESSED),
        "Button.Disabled.Background": color(Surface.PANEL, Alpha.DISABLED_BUTTON),
        "Button.Disabled.Text": color(Text.DISABLED),
    }
    zones = {
        "Frame": (
            color(Surface.FRAME),
            ("Background", "Edge"),
            {
                "Text.Secondary": color(Text.SECONDARY),
                "GripperDot": color(Line.BORDER),
                "Highlight": color(Accent.CONTROL_PRESSED),
                "Button.EnabledHover.Border": color(Surface.HOVER),
                "Tab.EnabledHover.Background": color(Surface.WELL),
                "Tab.EnabledHover.Text": color(Text.PRIMARY),
                "Tab.Checked.Background": color(Surface.PANEL),
                "Tab.Checked.Text": color(Text.PRIMARY),
                "Tab.Unchecked.Text": color(Text.SECONDARY),
                "List.Enabled.Background": color(Surface.FIELD),
            },
        ),
        "Content": (
            color(Surface.PANEL),
            ("Background", "Tab.Enabled.Border"),
            {
                "Text.Enabled": color(Text.PRIMARY),
                "Text.Highlight": color(Text.PRIMARY),
                "Highlight": color(Surface.HOVER),
                "HighlightHover": color(Surface.HOVER),
                "Button.EnabledHover.Border": color(Line.BORDER_HOVER),
                "Button.CheckedPressed.Background": color(Accent.CONTROL_PRESSED_DISABLED),
                "Entry.Enabled.Background": color(Surface.FIELD),
                "Entry.Enabled.Border": color(Line.BORDER),
                "Entry.Enabled.Text": color(Text.PRIMARY),
                "Entry.Disabled.Text": color(Text.DISABLED),
                "List.Checked.Background": color(Accent.ROW_SELECTED),
                "List.Enabled.Background": color(Surface.WELL),
                "List.Enabled.Text": color(Text.PRIMARY),
                "List.Disabled.Text": color(Text.DISABLED),
            },
        ),
    }
    declared = {f"{zone}.{suffix}": value for zone, (ground, grounded, overrides) in zones.items() for suffix, value in {**uniform, **dict.fromkeys(grounded, ground), **overrides}.items()}
    axes = tuple(color(axis) for axis in (Axis.X, Axis.Y, Axis.Z))
    members: dict[object, Mapping[str, object]] = {
        AppearanceSettings: {
            "ViewportBackgroundColor": color(Surface.CANVAS),
            "PageviewPaperColor": color(Surface.PAPER),
            "FrameBackgroundColor": declared["Frame.Background"],
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
            **dict.fromkeys(("CommandPromptTextColor", "CommandPromptHypertextColor"), color(Text.PRIMARY)),
            **dict(zip(("GridXAxisLineColor", "GridYAxisLineColor", "GridZAxisLineColor"), axes, strict=True)),
            **dict(zip(("WorldCoordIconXAxisColor", "WorldCoordIconYAxisColor", "WorldCoordIconZAxisColor"), axes, strict=True)),
            "BlackWhiteSwitching": True,
        },
        SmartTrackSettings: {
            "LineColor": color(Guide.TRACKING),
            "GuideColor": color(Guide.CONSTRUCTION),
            "TanPerpLineColor": color(Guide.TRACKING),
            "PointColor": color(Guide.TRACKING),
            "ActivePointColor": color(Guide.TRACKING_ACTIVE),
        },
        ChooseOneObjectSettings: {"HighlightColor": color(Selection.HOVER), "UseCustomColor": True},
        GumballSettings: {"MenuBallColor": color(Guide.HANDLE), **dict(zip(("XAxisColor", "YAxisColor", "ZAxisColor"), axes, strict=True))},
        CurvatureGraphSettings: {"CurveHairColor": color(Guide.CONSTRUCTION), "SurfaceUHairColor": color(Axis.X), "SurfaceVHairColor": color(Axis.Y)},
        DirectionAnalysisSettings: {"Color": color(Guide.CONSTRUCTION)},
        EdgeAnalysisSettings: {"ShowEdgeColor": color(Status.ERROR)},
        ZebraAnalysisSettings: {"StripeColor": color(Ink.DOCUMENT)},
        CursorTooltipSettings: {"BackgroundColor": declared["Content.Background"], "TextColor": declared["Content.Text.Enabled"]},
        SoftTransformSettings: {"FalloffColor": color(Guide.TRACKING)},
    }
    keys: dict[tuple[str, ...], Mapping[str, Color | tuple[str, ...]]] = {
        path: declared,
        ("Options", "Appearance"): dict(zip(("DirectionArrowColorU", "DirectionArrowColorV", "DirectionArrowColorW"), axes, strict=True)),
        (): dict.fromkeys(("SelectionFilterCheckedColorDark", "SelectionFilterCheckedColorLight"), color(Accent.CONTROL_PRESSED)),
        ("UI", "Settings"): {"ColorPanelSwatches": tuple(map(hex_color, SWATCHES.values()))},
    }

    def strays() -> tuple[str, ...]:
        """Sorted theme keys holding a color the declaration leaves out, none while the store holds no theme child."""
        theme = located(path)
        return () if theme is None else tuple(sorted(name for name in theme.Keys if name not in declared and found(theme.TryGetColor(name)) is not None))

    def clear() -> None:
        """Delete every undeclared theme key holding a color."""
        theme = opened(path)
        for name in strays():
            theme.DeleteItem(name)

    return (
        *(key(owner, name, target=target) for owner, targets in keys.items() for name, target in targets.items()),
        action(label=labeled(path, "Keys"), read=strays, act=clear, target=()),
        *(member(owner, name, target=target) for owner, targets in members.items() for name, target in targets.items()),
        *(
            preference(
                label=f'AppearanceSettings.WidgetColor["{widget}"]', read=partial(AppearanceSettings.GetWidgetColor, widget), write=partial(AppearanceSettings.SetWidgetColor, widget), target=axis
            )
            for widget, axis in zip((WidgetColor.UAxisColor, WidgetColor.VAxisColor, WidgetColor.WAxisColor), axes, strict=True)
        ),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
