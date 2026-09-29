# ty: ignore[invalid-argument-type, invalid-return-type, no-matching-overload, unresolved-import]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, return-value, no-any-return, call-overload, arg-type, unreachable"
"""Rhino settings rows by settings path, member, and internal type, and the plain form a row compares."""

from collections.abc import Callable, Mapping
from enum import StrEnum
from functools import partial, reduce
import math

from Eto.Drawing import Color as EtoColor
import Rhino
from Rhino.Commands import Command
from Rhino.DocObjects import Font
from Rhino.PlugIns import PlugIn
import System
from System import Array, Guid, String
from System.Drawing import Color

from interface.report import Row

# --- [TYPES] ----------------------------------------------------------------------------

type SettingsPath = tuple[str, ...] | tuple[Command | Guid, *tuple[str, ...]]


class Internal(StrEnum):
    """Type Rhino reaches by assembly-qualified name alone."""

    ACTION = "System.Action`1"
    AI_HOST = "Rhino.AI.RhinoAIHost, RhinoAI"
    AI_SETTINGS = "Rhino.AI.AISettings, RhinoAI"
    BASE_TAB_CONTROL = "Rhino.UI.Internal.TabPanels.Controls.BaseTabControl, Rhino.UI"
    BASE_TAB_CONTROL_ITEM = "Rhino.UI.Internal.TabPanels.Controls.BaseTabControlItem, Rhino.UI"
    CASCADE_STYLE = "Rhino.UI.Internal.TabPanels.CascadeStyle, Rhino.UI"
    DISPLAY_ATTRIBUTES_INT = "UnsafeNativeMethods+DisplayAttributesInt, RhinoCommon"
    DISPLAY_ATTRS_COLOR = "UnsafeNativeMethods+DisplayAttrsColor, RhinoCommon"
    DISPLAY_PIPELINE_ATTRIBUTES_BOOL = "UnsafeNativeMethods+DisplayPipelineAttributesBool, RhinoCommon"
    DOCK_SITE_RESIZER = "Rhino.UI.Internal.TabPanels.Controls.DockSiteResizer, Rhino.UI"
    LAYER_COLUMNS = "Rhino.UI.DialogPanels.LayerColumns, Rhino.UI"
    LAYER_COLUMN_TYPE = "Rhino.UI.DialogPanels.LayerColumns+ColumnType, Rhino.UI"
    LAYER_TREE_GRID_VIEW = "Rhino.UI.DialogPanels.LayerTreeGridView, Rhino.UI"
    LAYOUT_TREE_GRID_VIEW = "Rhino.UI.DialogPanels.LayoutTreeGridView, Rhino.UI"
    OPEN_COLOR = "Eto.Drawing.OpenColor, Grasshopper2"
    OSNAP_BUTTON_DISPLAY = "Rhino.UI.DialogPanels.OSnapPanel+OSnapButtonDisplay, Rhino.UI"
    RUNTIME_SETTINGS = "Rhino.UI.Runtime.Settings, Rhino.UI"
    SELECTION_FILTER_BUTTON_DISPLAY = "Rhino.UI.DialogPanels.SelectionFilterUi+ButtonDisplay, Rhino.UI"
    STATUS_BAR_INFO_PANE_MODE = "Rhino.UI.Internal.TabPanels.Controls.StatusBarInfoPaneMode, Rhino.UI"
    TAB_CONTROL_DISPLAY_STYLE = "Rhino.UI.Internal.TabPanels.TabControlDisplayStyle, Rhino.UI"
    TAB_PANEL_DOCK_BARS = "Rhino.UI.Internal.TabPanels.TabPanelDockBars, Rhino.UI"
    TAB_PANEL_DOCK_SITES = "Rhino.UI.Internal.TabPanels.TabPanelDockSites, Rhino.UI"
    TAB_PANEL_SETTINGS = "Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI"
    TECHNICAL_MODE_PARAMETER = "Rhino.Display.DisplayPipelineAttributes+TechnicalModeParameter, RhinoCommon"
    TOOLBAR_SETTINGS = "Rhino.UI.Internal.TabPanels.ToolbarSettings, Rhino.UI"
    UNSAFE_NATIVE_METHODS = "UnsafeNativeMethods, RhinoCommon"
    WINDOW_LAYOUT_FILE = "Rhino.UI.Internal.TabPanels.WindowLayoutFile, Rhino.UI"

    @property
    def type(self) -> System.Type:
        """Type the name resolves to."""
        return System.Type.GetType(self.value, throwOnError=True)

    def parsed(self, name: str) -> System.Enum:
        """Member of the enum type by name."""
        return System.Enum.Parse(self.type, name)

    def setting(self, name: str, *, target: object) -> Row:
        """Row of the type's static property, written through its setter as a typed delegate."""
        held = self.type.GetProperty(name)
        return Row(
            label=f"{self.type.Name}.{name}",
            read=partial(held.GetValue, None),
            write=System.Delegate.CreateDelegate(Internal.ACTION.type.MakeGenericType(held.PropertyType), held.SetMethod),
            target=target,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SETTINGS]
def found[T](result: tuple[bool, T]) -> T | None:
    """Value of a `TryGet` read, None when the key holds none."""
    held, value = result
    return value if held else None


def rooted(path: SettingsPath) -> tuple[Rhino.PersistentSettings, str, tuple[str, ...]]:
    """Settings the path opens at, a command's, a plug-in id's, or Rhino's application settings, with its owner label and the child names under it."""
    match path:
        case (Command() as command, *names):
            return command.Settings, f'PlugIns["{command.PlugIn.Name}"].{command.EnglishName}', tuple(names)
        case (Guid() as plugin, *names):
            return Rhino.PersistentSettings.FromPlugInId(plugin), f'PlugIns["{PlugIn.GetPlugInInfo(plugin).Name}"]', tuple(names)
        case names:
            return Rhino.PersistentSettings.RhinoAppSettings, "RhinoAppSettings", tuple(names)


def located(path: SettingsPath) -> Rhino.PersistentSettings | None:
    """Settings child at the path as the store holds it, None while a level is absent."""
    root, _, names = rooted(path)
    return reduce(lambda held, name: None if held is None else found(held.TryGetChild(name)), names, root)


def opened(path: SettingsPath) -> Rhino.PersistentSettings:
    """Settings child at the path, each absent level added."""
    root, _, names = rooted(path)
    return reduce(lambda held, name: held.AddChild(name), names, root)


def labeled(path: SettingsPath, name: str) -> str:
    """Report label of the key at the path, `owner.child.key`."""
    _, owner, names = rooted(path)
    return ".".join((owner, *names, name))


def key(path: SettingsPath, name: str, *, target: bool | int | str | Guid | tuple[str, ...] | Color) -> Row:
    """Row of a settings key through the accessor pair of the target's stored type, read from the child the store holds and written into the child added on demand."""
    accessors: dict[type, tuple[Callable[[Rhino.PersistentSettings], tuple[bool, object]], Callable[[Rhino.PersistentSettings, object], None]]] = {
        bool: (lambda child: child.TryGetBool(name), lambda child, value: child.SetBool(name, value)),
        int: (lambda child: child.TryGetInteger(name), lambda child, value: child.SetInteger(name, value)),
        str: (lambda child: child.TryGetString(name), lambda child, value: child.SetString(name, value)),
        Guid: (lambda child: child.TryGetGuid(name), lambda child, value: child.SetGuid(name, value)),
        tuple: (lambda child: child.TryGetStringList(name), lambda child, value: child.SetStringList(name, Array[String](list(value)))),
        Color: (lambda child: child.TryGetColor(name), lambda child, value: child.SetColor(name, value)),
    }
    get, put = accessors[type(target)]
    return Row(label=labeled(path, name), read=lambda: None if (child := located(path)) is None else found(get(child)), write=lambda value: put(opened(path), value), target=target)


def unsigned(path: SettingsPath, name: str, *, target: int) -> Row:
    """Row of a settings key its owner stores as an unsigned integer."""
    return Row(
        label=labeled(path, name),
        read=lambda: None if (child := located(path)) is None else found(child.TryGetUnsignedInteger(name)),
        write=lambda value: opened(path).SetUnsignedInteger(name, value),
        target=target,
    )


def defaulted(path: SettingsPath, name: str, *, target: bool | int, default: bool | int) -> Row:
    """Row of a settings key read through the defaulting getter its owner reads with, the default while the child is absent."""
    accessors: dict[type, tuple[Callable[[Rhino.PersistentSettings], bool | int], Callable[[Rhino.PersistentSettings, bool | int], None]]] = {
        bool: (lambda child: child.GetBool(name, default), lambda child, value: child.SetBool(name, value)),
        int: (lambda child: child.GetInteger(name, default), lambda child, value: child.SetInteger(name, value)),
    }
    get, put = accessors[type(target)]
    return Row(label=labeled(path, name), read=lambda: default if (child := located(path)) is None else get(child), write=lambda value: put(opened(path), value), target=target)


def absent(path: SettingsPath, name: str) -> Row:
    """Row holding a settings key absent, deleted from its child when present."""
    return Row(label=labeled(path, name), read=lambda: (child := located(path)) is not None and name in child.Keys, write=lambda _: opened(path).DeleteItem(name), target=False)


def member(owner: object, name: str, *, target: object) -> Row:
    """Row of a named property of a class or an instance."""
    kind = owner if isinstance(owner, type) else type(owner)
    return Row(label=f"{kind.__name__}.{name}", read=partial(getattr, owner, name), write=partial(setattr, owner, name), target=target)


# --- [VALUES]
def guid(identifier: StrEnum) -> Guid:
    """Guid of an id enum member."""
    return Guid.Parse(identifier.value)


def port(doc: Rhino.RhinoDoc) -> int | None:
    """Listener port of the document, None while it holds none."""
    arguments = Array[System.Object]([doc, None])
    return arguments.GetValue(1) if Internal.AI_HOST.type.GetMethod("TryGetPortFor").Invoke(None, arguments) else None


def plain(value: object) -> object:
    """.NET or Python value as Python compares it, the NaN of an unset number as None and a float past the noise a unit conversion leaves."""
    match value:
        case Mapping():
            return {name: plain(held) for name, held in value.items()}
        case tuple() | System.Array():
            return tuple(map(plain, value))
        case Color():
            return (value.A, value.R, value.G, value.B)
        case EtoColor():
            return (value.Ab, value.Rb, value.Gb, value.Bb)
        case Font():
            return value.QuartetName
        case Guid() | System.Enum():
            return str(value)
        case float() if math.isnan(value):
            return None
        case float():
            return round(value, 9)
        case _:
            return value


def color(rgb: tuple[int, int, int], alpha: float = 1.0) -> Color:
    """System color of a role at the alpha fraction."""
    return Color.FromArgb(round(alpha * 255), *rgb)


def hex_color(rgb: tuple[int, int, int]) -> str:
    """Role as `#RRGGBB`."""
    return f"#{bytes(rgb).hex().upper()}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Internal", "absent", "color", "defaulted", "found", "guid", "hex_color", "key", "labeled", "located", "member", "opened", "plain", "port", "unsigned"]
