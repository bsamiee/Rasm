# ty: ignore[invalid-argument-type, invalid-return-type, no-matching-overload, too-many-positional-arguments]
# mypy: disable-error-code="arg-type, call-arg, call-overload, import-untyped, no-any-return, no-any-unimported, return-value, unreachable"
"""Rhino settings rows by settings path, member, and internal type, the scope disposing a .NET resource, and the plain form every Rhino row and action compares in."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from enum import StrEnum
from functools import partial, reduce
import math

from Eto.Drawing import Color as EtoColor
import Rhino
from Rhino import PersistentSettings
from Rhino.Display import Color4f
from Rhino.DocObjects import Font
from Rhino.Geometry import Point3d, Vector2d, Vector3d
from Rhino.PlugIns import PlugIn
import System
from System import Array, Guid, IDisposable, UInt32
from System.Drawing import Color, Size

from interface.report import Action, Row

# --- [TYPES] ----------------------------------------------------------------------------

type SettingsPath = tuple[str, ...] | tuple[Guid, *tuple[str, ...]] | tuple[PlugIn, str, *tuple[str, ...]]


class Internal(StrEnum):
    """Type Rhino reaches by assembly-qualified name alone."""

    ACTION = "System.Action`1"
    AGX_TONE_MAPPING = "Darkroom.AgXToneMapping, Darkroom"
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
    LAYOUT_COLUMN_TYPE = "Rhino.UI.DialogPanels.LayoutTreeGridView+ColumnType, Rhino.UI"
    LAYOUT_TREE_GRID_VIEW = "Rhino.UI.DialogPanels.LayoutTreeGridView, Rhino.UI"
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
        return preference(
            label=f"{self.type.Name}.{name}",
            read=partial(held.GetValue, None),
            write=System.Delegate.CreateDelegate(Internal.ACTION.type.MakeGenericType(held.PropertyType), held.SetMethod),
            target=target,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SCOPES]
@contextmanager
def disposed[T: IDisposable](resource: T) -> Generator[T]:
    """Scope holding the .NET resource, disposed at its exit."""
    try:
        yield resource
    finally:
        resource.Dispose()


# --- [SETTINGS]
def found[T](result: tuple[bool, T]) -> T | None:
    """Value of a `TryGet` read, None when the key holds none."""
    held, value = result
    return value if held else None


def rooted(path: SettingsPath) -> tuple[PersistentSettings, str, tuple[str, ...]]:
    """Settings the path opens at, a loaded plug-in's settings of the command its English name names, a plug-in id's, or Rhino's application settings, with its owner label and the child names under it."""
    match path:
        case (PlugIn() as plugin, str() as command, *names):
            return plugin.CommandSettings(command), f'PlugIns["{plugin.Name}"].{command}', tuple(names)
        case (Guid() as plugin, *names):
            return PersistentSettings.FromPlugInId(plugin), f'PlugIns["{PlugIn.GetPlugInInfo(plugin).Name}"]', tuple(names)
        case names:
            return PersistentSettings.RhinoAppSettings, "RhinoAppSettings", tuple(names)


def located(path: SettingsPath) -> PersistentSettings | None:
    """Settings child at the path as the store holds it, None while a level is absent."""
    root, _, names = rooted(path)
    return reduce(lambda held, name: None if held is None else found(held.TryGetChild(name)), names, root)


def opened(path: SettingsPath) -> PersistentSettings:
    """Settings child at the path, each absent level added."""
    root, _, names = rooted(path)
    return reduce(lambda held, name: held.AddChild(name), names, root)


def labeled(path: SettingsPath, name: str) -> str:
    """Report label of the key at the path, `owner.child.key`."""
    _, owner, names = rooted(path)
    return ".".join((owner, *names, name))


def key(path: SettingsPath, name: str, *, target: bool | int | str | Guid | tuple[str, ...] | Color, stored: type[UInt32] | None = None, default: bool | int | Guid | None = None) -> Row:
    """Row of a settings key through the unbound accessor pair of its stored type, the target's own unless named, read from the child the store holds, the owner's default while the child or key is absent, and written into the child added on demand."""

    def row[T](get: Callable[[PersistentSettings, str], tuple[bool, T]], put: Callable[[PersistentSettings, str, T], None], value: T) -> Row:
        return preference(
            label=labeled(path, name),
            read=lambda: default if (child := located(path)) is None or (held := found(get(child, name))) is None else held,
            write=lambda written: put(opened(path), name, written),
            target=value,
        )

    match target:
        case bool():
            return row(PersistentSettings.TryGetBool, PersistentSettings.SetBool, target)
        case int() if stored is None:
            return row(PersistentSettings.TryGetInteger, PersistentSettings.SetInteger, target)
        case int():
            return row(PersistentSettings.TryGetUnsignedInteger, PersistentSettings.SetUnsignedInteger, target)
        case str():
            return row(PersistentSettings.TryGetString, PersistentSettings.SetString, target)
        case Guid():
            return row(PersistentSettings.TryGetGuid, PersistentSettings.SetGuid, target)
        case tuple():
            return row(PersistentSettings.TryGetStringList, PersistentSettings.SetStringList, target)
        case Color():
            return row(PersistentSettings.TryGetColor, PersistentSettings.SetColor, target)


def member(owner: object, name: str, *, target: object) -> Row:
    """Row of a named property of a class or an instance."""
    kind = owner if isinstance(owner, type) else type(owner)
    return preference(label=f"{kind.__name__}.{name}", read=partial(getattr, owner, name), write=partial(setattr, owner, name), target=target)


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
        case Size():
            return (value.Width, value.Height)
        case Vector2d():
            return (plain(value.X), plain(value.Y))
        case Vector3d() | Point3d():
            return (plain(value.X), plain(value.Y), plain(value.Z))
        case Color4f():
            return tuple(map(plain, (value.R, value.G, value.B, value.A)))
        case Guid() | System.Enum():
            return str(value)
        case float() if math.isnan(value):
            return None
        case float():
            return round(value, 9)
        case _:
            return value


def preference[T](*, label: str, read: Callable[[], object], write: Callable[[T], object], target: T) -> Row:
    """Row of the setting compared in its plain form."""
    return Row(label=label, read=read, write=write, target=target, plain=plain)


def action(*, label: str, read: Callable[[], object], act: Callable[[], object], target: object) -> Action:
    """Action of the setting compared in its plain form."""
    return Action(label=label, read=read, act=act, target=target, plain=plain)


def color(rgb: tuple[int, int, int], alpha: float = 1.0) -> Color:
    """System color of a role at the alpha fraction."""
    return Color.FromArgb(round(alpha * 255), *rgb)


def hex_color(rgb: tuple[int, int, int]) -> str:
    """Role as `#RRGGBB`."""
    return f"#{bytes(rgb).hex().upper()}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Internal", "action", "color", "disposed", "found", "guid", "hex_color", "key", "labeled", "located", "member", "opened", "plain", "port", "preference"]
