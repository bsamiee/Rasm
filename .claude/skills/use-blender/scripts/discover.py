# mypy: disable-error-code="attr-defined, union-attr, arg-type"
# ty: ignore[unresolved-attribute, invalid-argument-type]
"""Operators, RNA types, and add-on settings matching words across stock Blender and every enabled add-on."""

from collections.abc import Mapping
import contextlib
import inspect
import io
from itertools import accumulate

import attrs
import bpy
from rna import BPyOpFunction, operators, plain, stored
from scene import viewport

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Operator:
    """Registered operator with the facts that decide its call, `poll_in_view` `None` in a background process or when no window shows a 3D Viewport."""

    call: str
    label: str
    description: str
    owner: str | None
    source: str | None
    params: Mapping[str, str]
    poll: bool
    poll_in_view: bool | None


@attrs.frozen
class Type:
    """Registered RNA type outside operators, `base` its nearest ancestor `bpy.types` exposes, `exposed` true when `bpy.types.<identifier>` reaches it."""

    identifier: str
    label: str
    base: str | None
    owner: str | None
    source: str | None
    exposed: bool


@attrs.frozen
class Setting:
    """Stored property an add-on registered on an ID type or in its preferences, with its value by ID name or add-on module."""

    path: str
    type: str
    values: dict[str, object]


@attrs.frozen
class Discovery:
    """Hits for the words, grouped by kind."""

    operators: tuple[Operator, ...]
    types: tuple[Type, ...]
    settings: tuple[Setting, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def discover(*words: str) -> Discovery:
    """Operators, RNA types, and add-on settings with every word in their text, polls answered in the calling context and in the largest 3D Viewport, and the tracebacks add-on polls and enum callbacks print kept out of the call's output."""
    needles = tuple(word.casefold() for word in words)
    addons = {key: addon.module for addon in bpy.context.preferences.addons for key in {addon.module, addon.module.rpartition(".")[2]}}
    members = tuple(bpy.context.copy().values())
    largest = None if bpy.app.background else viewport()
    view = None if largest is None else bpy.context.temp_override(window=largest.window, area=largest.area, region=largest.region)

    def matches(text: str) -> bool:
        return all(needle in text.casefold() for needle in needles)

    def descendants(cls: "type[bpy.types.bpy_struct[object]]") -> "set[type[bpy.types.bpy_struct[object]]]":
        return {cls, *(found for sub in cls.__subclasses__() for found in descendants(sub))}

    def own(struct: bpy.types.Struct) -> tuple[bpy.types.Property, ...]:
        """Properties the struct declares, its base's left out."""
        return tuple(p for p in struct.properties if struct.base is None or p.identifier not in struct.base.properties)

    def owner(cls: type) -> str | None:
        """Enabled add-on with a module or extension id naming the package holding the class, `None` for Blender's own classes and executed code."""
        return next((addons[package] for package in accumulate(cls.__module__.split("."), lambda head, part: f"{head}.{part}") if package in addons), None)

    def source(cls: type) -> str | None:
        """File defining the class, `None` for a compiled class or one from executed code."""
        try:
            return inspect.getsourcefile(cls)
        except (OSError, TypeError):
            return None

    def base(struct: bpy.types.Struct) -> str | None:
        """Nearest ancestor `bpy.types` exposes, `None` for a root struct."""
        return None if struct.base is None else struct.base.identifier if hasattr(bpy.types, struct.base.identifier) else base(struct.base)

    def in_view(entry: BPyOpFunction) -> bool | None:
        if view is None:
            return None
        with view:
            return entry.poll()

    def operator(entry: BPyOpFunction) -> Operator | None:
        rna, call = entry.get_rna_type(), f"bpy.ops.{entry.idname_py()}"
        if not matches(f"{call} {rna.name} {rna.description}"):
            return None
        cls = bpy.types.Operator.bl_rna_get_subclass_py(entry.idname())
        who, file = (None, None) if cls is None else (owner(cls), source(cls))
        return Operator(call, rna.name, rna.description, who, file, {p.identifier: p.type for p in own(rna)}, entry.poll(), in_view(entry))

    def rna_type(cls: "type[bpy.types.bpy_struct[object]]") -> Type | None:
        rna = cls.bl_rna
        if issubclass(cls, bpy.types.Operator | bpy.types.Macro | bpy.types.OperatorProperties) or not matches(f"{rna.identifier} {rna.name} {rna.description}"):
            return None
        return Type(rna.identifier, rna.name, base(rna), owner(cls), source(cls), hasattr(bpy.types, rna.identifier))

    def setting(struct: bpy.types.Struct, prop: bpy.types.Property, owners: "Mapping[str, bpy.types.bpy_struct[object]]") -> Setting | None:
        path = f"{struct.identifier}.{prop.identifier}"
        if not (stored(prop) and matches(f"{path} {prop.name} {prop.description}")):
            return None
        kind = prop.fixed_type.identifier if isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) else prop.type
        return Setting(path, kind, {name: plain(getattr(owned, prop.identifier)) for name, owned in owners.items()})

    named = (cls for name in dir(bpy.types) if isinstance(cls := getattr(bpy.types, name), type) and issubclass(cls, bpy.types.bpy_struct))
    structs = sorted((cls for cls in {*named, *descendants(bpy.types.bpy_struct)} if "bl_rna" in vars(cls)), key=lambda cls: cls.bl_rna.identifier)
    scoped = (
        setting(host.bl_rna, prop, {member.name: member for member in members if isinstance(member, host)})
        for host in structs
        if issubclass(host, bpy.types.ID)
        for prop in own(host.bl_rna)
        if prop.is_runtime
    )
    preferred = (
        setting(addon.preferences.bl_rna, prop, {addon.module: addon.preferences})
        for addon in bpy.context.preferences.addons
        if addon.preferences is not None
        for prop in own(addon.preferences.bl_rna)
    )
    with contextlib.redirect_stderr(io.StringIO()):
        return Discovery(tuple(filter(None, map(operator, operators()))), tuple(filter(None, map(rna_type, structs))), tuple(filter(None, (*scoped, *preferred))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Discovery", "Operator", "Setting", "Type", "discover"]
