"""Acrobat's preference leaves, each a typecode and value under the `DC` hive of its domain, and the crash reporter's send choice."""

from collections.abc import Iterator, Mapping
from enum import IntEnum
import math
import struct
from typing import Final

from interface.adobe.session import Unscripted
from interface.adobe.stores import Default
from interface.roles import Alpha, blend, fractions, Guide, Line, Selection, Status, Surface
from interface.units import Length, Units

# --- [TYPES] ----------------------------------------------------------------------------

type Tree = Mapping[str, Tree | tuple[Typecode, object]]


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


def leaves(tree: Tree, *keys: str) -> Iterator[Default]:
    """Leaf of each node in the tree at its keys under the `DC` hive, each key between the section and the node's own a cabinet."""
    for key, held in tree.items():
        match held:
            case Mapping():
                yield from leaves(held, *keys, key)
            case node:
                section, *nested = keys
                below = (section, *(step for name in nested for step in (name, (Typecode.CABINET,))), key)
                yield Default(DOMAIN, ("DC", *below), node)


# --- [ROWS]
def rows(units: Units) -> tuple[Default, ...]:
    """Acrobat's leaves for the unit system by section, its page unit shown and its grid and leader lengths in points, then the crash reporter's send choice."""
    rgb_space, off, on = 1, (Typecode.BOOLEAN, False), (Typecode.BOOLEAN, True)

    def channels(key: str, rgb: tuple[int, int, int]) -> Tree:
        """Color cabinet of the key: each channel's 16.16 fraction and the RGB space, named after the cabinet."""
        return {f"{key}{name}": (Typecode.INTEGER, value) for name, value in (*zip(("Red", "Green", "Blue"), map(fixed, fractions(rgb)), strict=True), ("Space", rgb_space))}

    tree: Tree = {
        "AVGeneral": {
            "HonorOSTheme": off,
            "ActiveUITheme": atom("DarkTheme"),
            "AV2ViewerLHPState": (Typecode.TEXT, b"hidden\0"),
            "Dockables": {"GenTechAcrobatAI": {"TabVisible": off}, **{panel: {"TabVisible": on} for panel in ("OCGs", "FileAttachmentDockable")}},
            **dict.fromkeys(("ShowPageHoverMenu", "PromptBeforeClosingMultipleTabs", "AcrobatRHPBottomBannerIPMEnabled", "IsNewUser", "WhatsNewEnabled"), off),
            **dict.fromkeys(("AlwaysUseFileNameAsDocTitle", "ToolHotkeys", "DisableStudioHome"), on),
            "AV2FavoritesCommandsDesktop": (
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
        },
        "Selection": {"EnableContextualToolbar": off},
        "HandTool": {"MouseWheelZooms": on},
        "Originals": {
            "PageViewLayoutMode": (Typecode.INTEGER, 2),
            "DefaultZoomType": (Typecode.INTEGER, 1),
            "PageUnits": (Typecode.INTEGER, frozendict({Length.INCHES: 1, Length.MILLIMETERS: 2})[units.page]),
            **dict.fromkeys((f"Grid{side}" for side in ("Width", "Height")), (Typecode.INTEGER, fixed(units.snap / Length.POINTS))),
            **dict.fromkeys((f"Grid{axis}Offset" for axis in ("H", "V")), (Typecode.INTEGER, 0)),
            "GridSubdivisions": (Typecode.INTEGER, round(units.snap / units.resolution)),
            "GridColor": channels("GridColor", Line.PAPER_GRID),
            "GridMinorColor": channels("GridMinorColor", blend(Line.PAPER_GRID, Surface.PAPER, Alpha.GRID_MINOR)),
        },
        "Measuring": {
            "HintColor": channels("HintColor", Guide.TRACKING),
            **{f"Leader{name}": (Typecode.INTEGER, round(length / Length.POINTS)) for name, length in (("Length", units.first_offset), ("Extend", units.extension), ("Offset", units.offset))},
        },
        "UnitsAndGuides": {
            "RulersVisible": on,
            "GuideColor": {
                "ColorSpace": (Typecode.INTEGER, rgb_space),
                **{f"value{index}": (Typecode.REAL, math.ldexp(fixed(channel), -16)) for index, channel in enumerate((*fractions(Guide.CONSTRUCTION), 0), start=1)},
            },
        },
        "IPM": {"DoNotCheckForMessage": on, "ShowMsgAtLaunch": off},
        "HomeWelcome": {"LastShowStatus": off},
        "DocumentStatus": {"HomeScreenOptionWhenDocClosed": off},
        "ScanOCRDMB": {"NumberOfTimesDMBCrossClicked": (Typecode.INTEGER, 3)},
        "FTEDialog": {"ShowInstallFTE": off},
        "ToolSuggestion": {"IsNewUser": off},
        "QuickToolsFrequent": {"FrequentlyUsedToolsVisibility": {"Visible": off}},
        "AVPrivate": {"AIVideoStripExpUserPref": off},
        "HelpAndLearn": dict.fromkeys(("IsNewUserForContextualHelp", "HelpAndLearnV2NewUsers"), off),
        "Gentech": dict.fromkeys(
            ("ConsentProvided", "AutoOpenPanel", "EnableNBA", "SummaryDMBEnabled", "SLModelOverviewEnabledPref", "SmartHighlightsEnabledPref", "ShouldShowGTPromotionForCommentsPanel"), off
        ),
        "Intl": {"TranslateBannerSuggestedPromptsPref": off, **dict.fromkeys(("Ligature", "ComplexScript"), on)},
        "Annots": {"Prefs": {"copyTextToMarkupAnnot": on}},
        "FormsPrefs": {
            key: {"Data": (Typecode.BINARY, struct.pack("<B3x4i", rgb_space, *map(fixed, fractions(rgb)), 0))}
            for key, rgb in (
                ("RequiredFieldHLColor", Status.ERROR),
                ("RuntimeBGIdleColor", Surface.FORM_FIELD),
                ("RuntimeBGFocusColor", Surface.PAPER),
                ("RuntimeBorderIdleColor", Line.FORM_FIELD),
                ("RuntimeBorderFocusColor", Selection.ACTIVE),
                ("RuntimeBorderRolloverColor", Selection.HOVER),
            )
        },
    }
    return (*leaves(tree), Default("com.adobe.crashreporter", ("always_never_send",), 2))


# --- [COMPOSITION] ----------------------------------------------------------------------

PRODUCT: Final = Unscripted(name="acrobat", identifiers=(DOMAIN,), rows=rows)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PRODUCT"]
