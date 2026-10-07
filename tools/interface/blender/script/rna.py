# ty: ignore[invalid-argument-type]
# mypy: disable-error-code="arg-type"
"""Blender's color role declaration, the members held for a scope, and the rows of declared RNA members in the installed extension's value forms."""

from collections.abc import Callable, Generator, Iterable, Iterator, Mapping
from contextlib import contextmanager
from functools import partial
from itertools import chain
from pathlib import Path
from types import ModuleType
from typing import TYPE_CHECKING

from attrs import frozen
import bpy
from mathutils import Color

from interface.report import Row
from interface.roles import fractions

if TYPE_CHECKING:
    from interface.blender.extension.unit_system import Group

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Paint:
    """Color role at an alpha, stored in the form the member's reader reads, `display` marking a `COLOR` member its add-on draws through builtin shaders or blf, which read display colors."""

    rgb: tuple[int, int, int]
    alpha: float = 1.0
    display: bool = False

    def stored(self, prop: bpy.types.FloatProperty) -> tuple[float, ...]:
        """Channels of the member: byte fractions for a `COLOR_GAMMA` member or a display-drawn one, scene-linear channels for any other `COLOR` member, and the alpha at its byte step for a four-channel member."""
        display = fractions(self.rgb)
        channels = display if prop.subtype == "COLOR_GAMMA" or self.display else tuple(Color(display).from_srgb_to_scene_linear())
        match prop.array_length:
            case 3 if self.alpha != 1:
                raise ValueError(f"{prop.identifier} stores three channels, so the declared alpha {self.alpha} has no channel")
            case 3:
                return channels
            case _:
                return (*channels, round(self.alpha * 255) / 255)


# --- [OPERATIONS] -----------------------------------------------------------------------


def held(collection: "bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]]", name: str) -> str | None:
    """Name when the collection holds an item by it, None otherwise."""
    return name if name in collection else None


@contextmanager
def assigned(*changes: "tuple[bpy.types.bpy_struct[object], str, object]") -> Generator[None]:
    """Members set for the scope and restored in order after it, an array member held as its values."""

    def put(rows: "Iterable[tuple[bpy.types.bpy_struct[object], str, object]]") -> None:
        for owner, key, value in rows:
            setattr(owner, key, value)

    saved = [(owner, key, tuple(value) if isinstance(value := getattr(owner, key), bpy.types.bpy_prop_array) else value) for owner, key, _ in changes]
    try:
        put(changes)
        yield
    finally:
        put(saved)


def converge(
    unit_system: ModuleType, label: str, owner: "bpy.types.bpy_struct[object]", declared: Mapping[str, object], assign: "Callable[[bpy.types.bpy_struct[object], str, object], object]" = setattr
) -> Iterator[Row]:
    """Row of every declared path under the owner in order, written through the assignment and compared in the extension's stored form, a color role in the channels its member stores, and a `DIR_PATH` string member's folder created before its row."""
    for path, target in declared.items():
        struct, name = unit_system.member(owner, path)
        if isinstance(prop := struct.bl_rna.properties[name], bpy.types.StringProperty) and prop.subtype == "DIR_PATH" and target:
            Path(str(target)).mkdir(parents=True, exist_ok=True)
        value = target.stored(prop) if isinstance(target, Paint) else target
        yield Row(label=f"{label}.{path}", read=partial(unit_system.current, struct, name), write=partial(assign, struct, name), target=value, plain=unit_system.stored)


def converge_groups(unit_system: ModuleType, groups: "Iterable[Group]") -> Iterator[Row]:
    """Rows of every group in order, each member written through its group's assignment."""
    return chain.from_iterable(converge(unit_system, group.label, group.struct, group.values, group.assign) for group in groups)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Paint", "assigned", "converge", "converge_groups", "held"]
