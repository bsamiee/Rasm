"""Acrobat's preference leaves, each a typecode and value under the `DC` hive of its domain, and the crash reporter's send choice."""

from enum import IntEnum
import math
import struct
from typing import Final

from interface.adobe.session import Unscripted
from interface.adobe.stores import Default
from interface.roles import Alpha, blend, fractions, Guide, Line, Selection, Status, Surface
from interface.units import Length, Units

# --- [TYPES] ----------------------------------------------------------------------------


class Typecode(IntEnum):
    """Leaf typecode Acrobat's accessors write ahead of each value."""

    BOOLEAN = 0
    INTEGER = 1
    ATOM = 2
    REAL = 3
    TEXT = 4
    BINARY = 6
    CABINET = 8


# --- [CONSTANTS] ------------------------------------------------------------------------

DOMAIN: Final = "com.adobe.Acrobat.Pro"

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [LEAVES]
def fixed(number: float) -> int:
    """ASFixed 16.16 integer of the number."""
    return round(math.ldexp(number, 16))


def atom(name: str) -> tuple[Typecode, bytes]:
    """Leaf of an atom as its NUL-terminated UTF-8 name."""
    return (Typecode.ATOM, f"{name}\0".encode())


def leaf(*keys: str, node: tuple[object, ...]) -> Default:
    """Leaf at the keys under the `DC` hive, each key between the section and the last a cabinet."""
    section, *nested, key = keys
    below = (section, *(step for name in nested for step in (name, (Typecode.CABINET,))), key)
    return Default(DOMAIN, ("DC", *below), node)


# --- [ROWS]
def rows(units: Units) -> tuple[Default, ...]:
    """Acrobat's leaves for the unit system, its page unit shown and its grid and leader lengths in points."""
    rgb_space = 1
    return (
        leaf("AVGeneral", "HonorOSTheme", node=(Typecode.BOOLEAN, False)),
        leaf("AVGeneral", "ActiveUITheme", node=atom("DarkTheme")),
        leaf("AVGeneral", "AV2ViewerLHPState", node=(Typecode.TEXT, b"hidden\0")),
        leaf("AVGeneral", "Dockables", "GenTechAcrobatAI", "TabVisible", node=(Typecode.BOOLEAN, False)),
        *(leaf("AVGeneral", "Dockables", panel, "TabVisible", node=(Typecode.BOOLEAN, True)) for panel in ("OCGs", "FileAttachmentDockable")),
        leaf("Selection", "EnableContextualToolbar", node=(Typecode.BOOLEAN, False)),
        leaf("AVGeneral", "ShowPageHoverMenu", node=(Typecode.BOOLEAN, False)),
        leaf("AVGeneral", "AlwaysUseFileNameAsDocTitle", node=(Typecode.BOOLEAN, True)),
        leaf("AVGeneral", "ToolHotkeys", node=(Typecode.BOOLEAN, True)),
        leaf("AVGeneral", "PromptBeforeClosingMultipleTabs", node=(Typecode.BOOLEAN, False)),
        leaf("HandTool", "MouseWheelZooms", node=(Typecode.BOOLEAN, True)),
        *(
            leaf("Originals", key, node=node)
            for key, node in (
                ("PageViewLayoutMode", (Typecode.INTEGER, 2)),
                ("DefaultZoomType", (Typecode.INTEGER, 1)),
                ("PageUnits", (Typecode.INTEGER, frozendict({Length.INCHES: 1, Length.MILLIMETERS: 2})[units.page])),
                *((f"Grid{side}", (Typecode.INTEGER, fixed(units.snap / Length.POINTS))) for side in ("Width", "Height")),
                *((f"Grid{axis}Offset", (Typecode.INTEGER, 0)) for axis in ("H", "V")),
                ("GridSubdivisions", (Typecode.INTEGER, round(units.snap / units.resolution))),
            )
        ),
        *(
            leaf(section, key, f"{key}{name}", node=(Typecode.INTEGER, value))
            for section, key, rgb in (
                ("Originals", "GridColor", Line.PAPER_GRID),
                ("Originals", "GridMinorColor", blend(Line.PAPER_GRID, Surface.PAPER, Alpha.GRID_MINOR)),
                ("Measuring", "HintColor", Guide.TRACKING),
            )
            for name, value in (*zip(("Red", "Green", "Blue"), map(fixed, fractions(rgb)), strict=True), ("Space", rgb_space))
        ),
        leaf("UnitsAndGuides", "RulersVisible", node=(Typecode.BOOLEAN, True)),
        leaf("UnitsAndGuides", "GuideColor", "ColorSpace", node=(Typecode.INTEGER, rgb_space)),
        *(leaf("UnitsAndGuides", "GuideColor", f"value{index}", node=(Typecode.REAL, math.ldexp(fixed(channel), -16))) for index, channel in enumerate((*fractions(Guide.CONSTRUCTION), 0), start=1)),
        *(
            leaf("Measuring", f"Leader{name}", node=(Typecode.INTEGER, round(length / Length.POINTS)))
            for name, length in (("Length", units.first_offset), ("Extend", units.extension), ("Offset", units.offset))
        ),
        leaf("IPM", "DoNotCheckForMessage", node=(Typecode.BOOLEAN, True)),
        leaf("AVGeneral", "AcrobatRHPBottomBannerIPMEnabled", node=(Typecode.BOOLEAN, False)),
        leaf("ToolRecommenderSection", "OnDocNextToolRecommendation", node=(Typecode.BOOLEAN, False)),
        leaf("AVGeneral", "DisableStudioHome", node=(Typecode.BOOLEAN, True)),
        leaf("HomeWelcome", "LastShowStatus", node=(Typecode.BOOLEAN, False)),
        leaf("DocumentStatus", "HomeScreenOptionWhenDocClosed", node=(Typecode.BOOLEAN, False)),
        leaf("ScanOCRDMB", "NumberOfTimesDMBCrossClicked", node=(Typecode.INTEGER, 3)),
        *(
            leaf(*keys, node=(Typecode.BOOLEAN, False))
            for keys in (
                ("FTEDialog", "ShowInstallFTE"),
                ("AVGeneral", "IsNewUser"),
                ("AVGeneral", "WhatsNewEnabled"),
                ("IPM", "ShowMsgAtLaunch"),
                ("ToolSuggestion", "IsNewUser"),
                ("QuickToolsFrequent", "FrequentlyUsedToolsVisibility", "Visible"),
                ("AVPrivate", "AIVideoStripExpUserPref"),
                *(("HelpAndLearn", key) for key in ("IsNewUserForContextualHelp", "HelpAndLearnV2NewUsers")),
                *(
                    ("Gentech", key)
                    for key in (
                        "ConsentProvided",
                        "AutoOpenPanel",
                        "EnableNBA",
                        "SummaryDMBEnabled",
                        "SLModelOverviewEnabledPref",
                        "SmartHighlightsEnabledPref",
                        "ShouldShowGTPromotionForCommentsPanel",
                    )
                ),
            )
        ),
        leaf(
            "AVGeneral",
            "AV2FavoritesCommandsDesktop",
            node=(
                Typecode.CABINET,
                {
                    str(index): atom(name)
                    for index, name in enumerate((
                        "SelectMenuItem",
                        "LineArrow",
                        "Square",
                        "PolygonCloud",
                        "FreeTextCallout",
                        "Stamp",
                        "RotatePagesCW",
                        "Measure",
                        "DIGSIG:CompareDocuments",
                        "Annots:Tool:RedactMenuItem",
                    ))
                },
            ),
        ),
        leaf("Intl", "TranslateBannerSuggestedPromptsPref", node=(Typecode.BOOLEAN, False)),
        *(leaf("Intl", key, node=(Typecode.BOOLEAN, True)) for key in ("Ligature", "ComplexScript")),
        leaf("Annots", "Prefs", "copyTextToMarkupAnnot", node=(Typecode.BOOLEAN, True)),
        *(
            leaf("FormsPrefs", key, "Data", node=(Typecode.BINARY, struct.pack("<B3x4i", rgb_space, *map(fixed, fractions(rgb)), 0)))
            for key, rgb in (
                ("RequiredFieldHLColor", Status.ERROR),
                ("RuntimeBGIdleColor", Surface.FORM_FIELD),
                ("RuntimeBGFocusColor", Surface.PAPER),
                ("RuntimeBorderIdleColor", Line.FORM_FIELD),
                ("RuntimeBorderFocusColor", Selection.ACTIVE),
                ("RuntimeBorderRolloverColor", Selection.HOVER),
            )
        ),
        Default("com.adobe.crashreporter", ("always_never_send",), 2),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

PRODUCT: Final = Unscripted(name="acrobat", identifiers=(DOMAIN,), rows=rows)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PRODUCT"]
