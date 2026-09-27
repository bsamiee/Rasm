# ty: ignore[unresolved-import, unresolved-attribute]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined"
# ruff: file-ignore[print]
"""Rhino setting rows with their readers and the report a run prints."""

from collections.abc import Iterable
from functools import partial
import math
from pathlib import Path
import sys
import traceback

from Eto.Drawing import Color as EtoColor
import Rhino
import System
from System import Array, Guid, String
from System.Drawing import Color as DrawingColor

from interface import roles
from interface.report import converged, Kind, line, Row

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [READERS]
def found[T](result: tuple[bool, T]) -> T | None:
    """Value of a `TryGet` read, None when the key holds none."""
    ok, value = result
    return value if ok else None


def member(label: str, owner: object, name: str, *, target: object) -> Row:
    """Row of a named property."""
    return Row(label=f"{label} {name}", read=partial(getattr, owner, name), write=partial(setattr, owner, name), target=target)


def key(child: Rhino.PersistentSettings, name: str, *, target: bool | int | str | Guid | tuple[str, ...] | DrawingColor, default: bool | int | str | None = None, label: str = "key") -> Row:
    """Row of a settings key through the accessor pair the API names for the target's type, read through the defaulting getter its owner reads with when the owner registers a default."""
    label = f"{label} {name}"
    match target, default:
        case bool(), bool():
            return Row(label=label, read=partial(child.GetBool, name, default), write=partial(child.SetBool, name), target=target)
        case bool(), _:
            return Row(label=label, read=lambda: found(child.TryGetBool(name)), write=partial(child.SetBool, name), target=target)
        case int(), int():
            return Row(label=label, read=partial(child.GetInteger, name, default), write=partial(child.SetInteger, name), target=target)
        case int(), _:
            return Row(label=label, read=lambda: found(child.TryGetInteger(name)), write=partial(child.SetInteger, name), target=target)
        case str(), str():
            return Row(label=label, read=partial(child.GetString, name, default), write=partial(child.SetString, name), target=target)
        case str(), _:
            return Row(label=label, read=lambda: found(child.TryGetString(name)), write=partial(child.SetString, name), target=target)
        case Guid(), _:
            return Row(label=label, read=lambda: found(child.TryGetGuid(name)), write=partial(child.SetGuid, name), target=target)
        case tuple(), _:
            return Row(label=label, read=lambda: found(child.TryGetStringList(name)), write=lambda value: child.SetStringList(name, Array[String](list(value))), target=target)
        case DrawingColor(), _:
            return Row(label=label, read=lambda: found(child.TryGetColor(name)), write=partial(child.SetColor, name), target=target)


def internal_setting(kind: object, name: str, *, target: object, instance: object = None) -> Row:
    """Row of an internal Rhino type's property labeled by the type, written through its setter as a typed delegate, static without an instance."""
    prop = kind.GetProperty(name)
    setter = prop.SetMethod.CreateDelegate(System.Type.GetType("System.Action`1", throwOnError=True).MakeGenericType(prop.PropertyType), instance)
    return Row(label=f"{kind.Name} {name}", read=partial(prop.GetValue, instance), write=setter, target=target)


def absent(child: Rhino.PersistentSettings, name: str, *, label: str) -> Row:
    """Row holding a settings key absent, deleted through its child when present."""
    return Row(label=f"{label} {name}", read=lambda: name in child.Keys, write=lambda _: child.DeleteItem(name), target=False)


# --- [REPORT]
def plain(value: object) -> object:
    """.NET value as Python compares it, the NaN of an unset number as None."""
    match value:
        case DrawingColor():
            return (value.A, value.R, value.G, value.B)
        case EtoColor():
            return (value.Rb, value.Gb, value.Bb, value.Ab)
        case Guid() | System.Enum():
            return str(value)
        case System.Array():
            return tuple(value)
        case float() if math.isnan(value):
            return None
        case _:
            return value


def emit(entries: Iterable[Row | str]) -> None:
    """Print the header and each entry's report lines, a raise ending the entries printed as an error line naming its row and frames."""
    settings = Path(Rhino.RhinoApp.GetDataDirectory(localUser=True, forceDirectoryCreation=False)) / "settings"
    print(line(Kind.HEADER, str(Rhino.RhinoApp.Version), str(settings)))
    held = "the first row"
    try:
        for entry in entries:
            match entry:
                case Row(label=label):
                    held = label
                    print("".join(f"{text}\n" for text in converged(entry, plain)), end="")
                    held = f"the row after {label}"
                case str():
                    print(entry)
    finally:
        if (error := sys.exception()) is not None:
            frames = " <- ".join(f"{frame.name}:{frame.lineno}" for frame in reversed(traceback.extract_tb(error.__traceback__)))
            print(line(Kind.ERROR, f"{held} raised {error!r} at {frames}"))


# --- [COLORS]
def color(rgb: roles.Rgb, alpha: float = 1.0) -> DrawingColor:
    """System color of a role at the alpha fraction."""
    return DrawingColor.FromArgb(round(alpha * 255), *rgb)


def hex_color(rgb: roles.Rgb) -> str:
    """Role as `#RRGGBB`."""
    return "#{:02X}{:02X}{:02X}".format(*rgb)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["absent", "color", "emit", "found", "hex_color", "internal_setting", "key", "member", "plain"]
