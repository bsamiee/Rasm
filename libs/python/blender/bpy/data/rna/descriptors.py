"""RNA property definitions, stored values, and native writes."""

from collections.abc import Callable, Iterable, Mapping
from functools import reduce
from itertools import pairwise
from operator import getitem, setitem
import os
import re
import typing
from typing import Annotated, Generic, Self, TypeVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.ids.store import blocks
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult, EditRefusal
from blender.imaging.faults import accumulated

# --- [TYPES] ----------------------------------------------------------------------------

type Scalar = bool | int | float | str
type Value = Scalar | tuple[Scalar, ...] | IdName | None
type Native = bpy.types.bpy_struct[object] | bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]]
P_co = TypeVar("P_co", bound=bpy.types.Property, covariant=True, default=bpy.types.Property)

# --- [TABLES] ---------------------------------------------------------------------------

LEADING = tuple(
    struct.bl_rna.properties[name]
    for struct, name in (
        (bpy.types.RenderSettings, "engine"),
        (bpy.types.ImageFormatSettings, "media_type"),
        (bpy.types.ImageFormatSettings, "file_format"),
        (bpy.types.ImageFormatSettings, "color_management"),
        (bpy.types.ColorManagedDisplaySettings, "display_device"),
        (bpy.types.ColorManagedViewSettings, "view_transform"),
        (bpy.types.UnitSettings, "system"),
        (bpy.types.DriverTarget, "id_type"),
    )
)
GATED = tuple(
    (bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[member], bpy.types.ImageFormatSettings.bl_rna.properties["color_management"], "OVERRIDE")
    for member in ("linear_colorspace_settings", "view_settings", "display_settings")
)

# --- [MODELS] ---------------------------------------------------------------------------


class IdName(msgspec.Struct, frozen=True, kw_only=True):
    """ID name in local data or linked library."""

    name: str
    library: str | None = None

    @classmethod
    def of(cls, block: bpy.types.ID) -> Self:
        """Returns block name with library path when linked."""
        return cls(name=block.name, library=Option.of_optional(block.library).map(lambda library: library.filepath).to_optional())


class Descriptor(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """Property definition from root type, no live data."""

    root: type[Native]
    owner: str
    prop: P_co
    index: Option[int] = Nothing

    # --- [RESOLUTION]

    @property
    def path(self) -> str:
        """RNA path from root to property."""
        return member_path(self.owner, self.member)

    @property
    def member(self) -> str:
        """Property path under owner with element coordinates."""
        return self.prop.identifier + self.index.map(lambda index: "".join(f"[{coordinate}]" for coordinate in np.unravel_index(index, self.dimensions(self.prop)))).default_value("")

    @staticmethod
    def resolve[K: bpy.types.Property](root: "type[bpy.types.bpy_struct[object]] | Native", path: str, kind: type[K]) -> BlenderResult["Descriptor[K]"]:
        """Resolves dotted path to descriptor, choice fault for another kind."""
        try:
            definition: bpy.types.Struct = root.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
        except ReferenceError as error:
            return Error((BlenderEditFault(request=f"{root!r}.{path}", error=error),))
        selector = r'\[(?:"(?:[^"\\]|\\.)*"|\d+)\]'
        step = rf"(\w+)((?:{selector})*)"
        steps = tuple(re.finditer(step, path)) if re.fullmatch(rf"{step}(?:\.{step})*", path) else ()

        def member(struct: bpy.types.Struct, at: re.Match[str]) -> BlenderResult[bpy.types.Property]:
            return Option.of_optional(struct.properties.get(at[1])).to_result_with(lambda: (BlenderAbsentFault(owner=struct.identifier, key=at[0]),))

        def entered(struct: bpy.types.Struct, at: re.Match[str]) -> BlenderResult[bpy.types.Struct]:
            def target(prop: bpy.types.Property) -> Option[bpy.types.Struct]:
                match prop:
                    case bpy.types.CollectionProperty() if not at[2]:
                        return Option.of_optional(prop.srna)
                    case bpy.types.CollectionProperty() if re.fullmatch(selector, at[2]):
                        return Option.of_optional(prop.fixed_type)
                    case bpy.types.PointerProperty() if not at[2]:
                        return Option.of_optional(prop.fixed_type)
                    case _:
                        return Nothing

            return member(struct, at).bind(lambda prop: target(prop).to_result_with(lambda: (BlenderAbsentFault(owner=struct.identifier, key=at[0]),)))

        def indexed(prop: bpy.types.Property, suffix: str) -> BlenderResult[Option[int]]:
            dimensions, coordinates = Descriptor.dimensions(prop), tuple(map(int, re.findall(r"\[(\d+)\]", suffix)))
            match suffix:
                case "":
                    return Ok(Nothing)
                case _ if re.fullmatch(r"(?:\[\d+\])+", suffix) and len(coordinates) == len(dimensions) and all(coordinate < size for coordinate, size in zip(coordinates, dimensions)):
                    return Ok(Some(int(np.ravel_multi_index(coordinates, dimensions))))
                case _:
                    return Error((BlenderAbsentFault(owner=definition.identifier, key=path),))

        def kinded(prop: bpy.types.Property, last: re.Match[str]) -> BlenderResult[Descriptor[K]]:
            return typed(prop, kind, f"{definition.identifier}.{path}").bind(
                lambda prop: indexed(prop, last[2]).map(
                    lambda index: Descriptor[K](
                        root=type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root, owner=path[: last.start()].removesuffix("."), prop=prop, index=index
                    )
                )
            )

        match steps, root:
            case (*inside, last), type():
                return (
                    reduce(lambda state, at: state.bind(lambda struct: entered(struct, at)), inside, Result[bpy.types.Struct, BlenderFaults](ok=definition))
                    .bind(lambda struct: member(struct, last))
                    .bind(lambda prop: kinded(prop, last))
                )
            case (*_, last), bpy.types.bpy_struct() | bpy.types.bpy_prop_collection():
                match struct_at(root, owner := path[: last.start()].removesuffix(".")):
                    case Option(tag="some", some=struct):
                        return member(struct.bl_rna, last).bind(lambda prop: kinded(prop, last))  # type: ignore[arg-type, union-attr]  # ty: ignore[invalid-argument-type, unresolved-attribute]
                    case Option():
                        return Error((BlenderAbsentFault(owner=definition.identifier, key=owner),))
            case _:
                return Error((BlenderAbsentFault(owner=definition.identifier, key=path),))

    @staticmethod
    def declared[K: bpy.types.Property](root: "type[bpy.types.bpy_struct[object]]", path: str, kind: type[K]) -> "Descriptor[K]":
        """Resolves dotted path of a declaration over the pinned host, a missing member raising as a defect."""
        match Descriptor.resolve(root, path, kind):
            case Result(tag="ok", ok=descriptor):
                return descriptor
            case Result(error=faults):
                raise LookupError(path, faults)

    def owner_of(self, context: Native, item: Option[Native] = Nothing) -> Option[Native]:
        """Returns live property owner from item, root, or context."""
        return (
            item
            .filter(lambda value: isinstance(value, self.root))
            .or_else_with(lambda: Some(context) if isinstance(context, self.root) else root_in(self.root, context) if isinstance(context, bpy.types.Context) else Nothing)
            .bind(lambda value: struct_at(value, self.owner))
            .filter(
                lambda owner: owner.bl_rna.properties.get(self.prop.identifier) == self.prop and effective(owner)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )
        )

    # --- [VALUES]

    @staticmethod
    def dimensions(prop: bpy.types.Property) -> tuple[int, ...]:
        """Returns declared numeric array dimensions, empty for other properties."""
        match prop:
            case bpy.types.BoolProperty() | bpy.types.IntProperty() | bpy.types.FloatProperty():
                return tuple(size for size in prop.array_dimensions if size)  # type: ignore[attr-defined]
            case _:
                return ()

    def shape(self, owner: Native) -> tuple[int, ...]:
        """Returns live array dimensions, outer length read from owner."""
        return (len(getattr(owner, self.prop.identifier)), *self.dimensions(owner.bl_rna.properties[self.prop.identifier])[1:])  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def read(self, owner: Native) -> Value:
        """Returns scalar, flat array, enum identifiers, `IdName`, struct identifier, or None."""
        name = self.prop.identifier
        match self.prop:
            case bpy.types.EnumProperty(is_enum_flag=True):
                return tuple(sorted(getattr(owner, name)))
            case bpy.types.EnumProperty(is_runtime=True) if isinstance(stored := owner.get(name), str):
                return stored
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()):
                return Option.of_optional(getattr(owner, name)).map(IdName.of).to_optional()  # type: ignore[unreachable]
            case bpy.types.PointerProperty():
                return Option.of_optional(getattr(owner, name)).map(lambda struct: struct.bl_rna.identifier).to_optional()
            case bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as prop if isinstance(owner, bpy.types.bpy_struct):
                buffer = [prop.default] * int(np.prod(self.shape(owner)))
                owner.path_resolve(name, False).foreach_get(buffer)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                return self.index.map(lambda index: buffer[index]).default_with(lambda: tuple(buffer))
            case bpy.types.BoolProperty(is_array=True):
                value = tuple(getattr(owner, name))
                return self.index.map(lambda index: value[index]).default_value(value)
            case _:
                return msgspec.convert(msgspec.to_builtins(getattr(owner, name)), bool | int | float | str)

    def equal(self, held: "Value | bpy.types.bpy_struct[object]", desired: object) -> bool:
        """Compares floats by float32 bits, other values exactly."""
        if isinstance(self.prop, bpy.types.StringProperty) and isinstance(desired, bytes):
            desired = msgspec.to_builtins(desired) if self.prop.subtype == "BYTE_STRING" else os.fsdecode(desired)
        return np.array_equal(np.asarray(held, np.float32).view(np.uint32), np.asarray(desired, np.float32).view(np.uint32)) if isinstance(self.prop, bpy.types.FloatProperty) else held == desired

    def write(self, owner: Native, value: "Value | bytes | bpy.types.bpy_struct[object]") -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
        """Assigns value, returning prior value or Nothing when equal."""
        name, request = self.prop.identifier, f"{owner!r}.{self.prop.identifier}"
        numeric = tuple(int(item) if isinstance(item, bool) else item for item in value) if isinstance(value, tuple) else int(value) if isinstance(value, bool) else value
        kind: object
        match self.prop:
            case bpy.types.FloatProperty(is_array=array):
                source, kind = numeric, tuple[float, ...] if array and self.index.is_none() else float
            case bpy.types.IntProperty(hard_min=low, hard_max=high, is_array=array):
                bounded = Annotated[int, msgspec.Meta(ge=low, le=high)]
                source, kind = numeric, tuple[bounded, ...] if array and self.index.is_none() else bounded
            case bpy.types.StringProperty(subtype=subtype):
                source, kind = value, bytes if subtype == "BYTE_STRING" or isinstance(value, bytes) else str
            case bpy.types.EnumProperty(is_enum_flag=flag):
                source, kind = value, tuple[str, ...] if flag else str
            case _:
                source, kind = value, object
        try:
            target = msgspec.convert(source, kind)
        except msgspec.ValidationError as error:
            return Error((BlenderReadFault(request=request, error=error),))
        try:
            prior = getattr(owner, name) if isinstance(self.prop, bpy.types.PointerProperty) else self.read(owner)
        except typing.get_args(EditRefusal.__value__) as error:
            return Error((BlenderEditFault(request=request, error=error),))

        def assigned(assignment: Callable[[], object]) -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
            def applied() -> "Option[Value | bpy.types.bpy_struct[object]]":
                assignment()
                if self.prop.is_runtime and isinstance(owner, bpy.types.bpy_struct):
                    bpy.msgbus.publish_rna(key=owner.path_resolve(name, False))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                return Some(prior)

            return BlenderEditFault.attempted(request, applied)

        def changed(stored: object) -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
            match self.prop, stored, self.index:
                case (
                    bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True),
                    bool() | int() | float(),
                    Option(tag="some", some=index),
                ) if isinstance(owner, bpy.types.bpy_struct):
                    coordinates = np.unravel_index(index, self.shape(owner))
                    return assigned(lambda: setitem(reduce(getitem, coordinates[:-1], owner.path_resolve(name, False)), coordinates[-1], stored))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case bpy.types.EnumProperty() as prop, tuple() | str() as chosen, _:
                    identifiers, native = (chosen, set(chosen)) if isinstance(chosen, tuple) else ((chosen,), chosen)
                    refusals = tuple(
                        BlenderChoiceFault(
                            path=request, identifier=identifier, accepted=tuple(item.identifier for item in prop.enum_items if bpy.types.UILayout.enum_item_name(owner, name, item.identifier))
                        )
                        for identifier in identifiers
                        if not bpy.types.UILayout.enum_item_name(owner, name, identifier)
                    )
                    return Error((refusals[0], *refusals[1:])) if refusals else assigned(lambda: setattr(owner, name, native))
                case bpy.types.PointerProperty(fixed_type=bpy.types.ID() as pointed), IdName() as identity, _:
                    return (  # type: ignore[unreachable]
                        Option
                        .of_optional(next((held for held in blocks(bpy.data, type(pointed)) if IdName.of(held) == identity), None))
                        .map(lambda found: assigned(lambda: setattr(owner, name, found)))
                        .default_with(lambda: Error((BlenderAbsentFault(owner=type(pointed).__name__, key=identity.name),)))
                    )
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), tuple(), _:
                    return assigned(lambda: setattr(owner, name, np.asarray(stored).reshape(self.shape(owner)).tolist()))
                case _:
                    return assigned(lambda: setattr(owner, name, stored))

        match self.prop, target:
            case _ if self.equal(IdName.of(prior) if isinstance(prior, bpy.types.ID) and isinstance(target, IdName) else prior, target):
                return Ok(Nothing)
            case bpy.types.FloatProperty() as real, float() | tuple() as number:
                return self.quantized(real, number).bind(lambda stored: changed(tuple(stored.tolist()) if stored.ndim else float(stored)))
            case bpy.types.StringProperty() as text, str() | bytes() as content:
                return self.validated(text, content).bind(changed)
            case _:
                return changed(target)

    @staticmethod
    def quantized(prop: bpy.types.FloatProperty, value: float | tuple[float, ...] | NDArray[np.float64]) -> BlenderResult[NDArray[np.float32]]:
        """Returns float32 storage, read fault outside hard range."""
        with np.errstate(over="ignore"):
            target = np.asarray(value, dtype=np.float32)
        return (
            Error((BlenderReadFault(request=repr(prop), error=ValueError(f"Value outside hard range [{prop.hard_min}, {prop.hard_max}]")),))
            if np.any((target < prop.hard_min) | (target > prop.hard_max))
            else Ok(target)
        )

    @staticmethod
    def validated[T: str | bytes](prop: bpy.types.StringProperty, value: T) -> BlenderResult[T]:
        """Returns storable value, read fault for NUL or overlong text."""
        try:
            if prop.subtype != "BYTE_STRING":
                msgspec.convert(os.fsdecode(value) if prop.subtype in {"FILE_PATH", "DIR_PATH", "FILE_NAME"} else value, Annotated[str, msgspec.Meta(pattern=r"^[^\x00]*$")])
            encoded = (os.fsencode(value) if prop.subtype in {"FILE_PATH", "DIR_PATH", "FILE_NAME"} else value.encode("utf-8")) if isinstance(value, str) else value
            if prop.length_max:
                msgspec.convert(encoded, Annotated[bytes, msgspec.Meta(max_length=prop.length_max - 1)])
            return Ok(value)
        except msgspec.ValidationError as error:
            return Error((BlenderReadFault(request=repr(prop), error=error),))
        except (TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=repr(prop), error=error),))

    def shown(self, value: Value) -> str:
        """Returns enum label or display text of stored value."""
        match self.prop, value:
            case bpy.types.EnumProperty() as prop, str():
                return Option.of_optional(prop.enum_items.get(value)).map(lambda item: item.name).default_value(value)
            case _, bool():
                return "on" if value else "off"
            case bpy.types.StringProperty(subtype="BYTE_STRING"), str():
                return repr(msgspec.convert(value, bytes))
            case _, str():
                return f'"{value}"'
            case _, IdName(name=block):
                return block
            case _, tuple():
                return ", ".join(map(self.shown, value))
            case _, None:
                return "none"
            case _:
                return str(value)


# --- [OPERATIONS] -----------------------------------------------------------------------


def member_path(owner: str, member: str) -> str:
    """Joins owner path and member, no dot after empty owner or colon or before element."""
    return f"{owner}{member}" if not owner or owner.endswith(":") or member.startswith("[") else f"{owner}.{member}"


def beneath(path: str, root: str) -> bool:
    """Whether path lies inside root path."""
    return path.startswith((f"{root}.", f"{root}["))


def typed[T](value: object, kind: type[T], path: str) -> BlenderResult[T]:
    """Returns value of kind, choice fault otherwise."""
    return Ok(value) if isinstance(value, kind) else Error((BlenderChoiceFault(path=path, identifier=type(value).__name__, accepted=(kind.__name__,)),))


def root_in[K: Native](root: type[K], context: bpy.types.Context) -> Option[K]:
    """Returns context-held root of type."""
    candidates = (
        (context,)
        if root is bpy.types.Context
        else Option.of_optional(context.preferences.addons.get(root.bl_idname)).map(lambda addon: addon.preferences).to_list()  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        if issubclass(root, bpy.types.AddonPreferences)
        else (getattr(context, member) for member in _members(root))
    )
    return Option.of_optional(next((value for value in candidates if isinstance(value, root)), None))


def rooted[K: Native](root: type[K], context: bpy.types.Context) -> BlenderResult[K]:
    """Returns context-held root, absent fault naming first matching member."""
    return root_in(root, context).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key=next(iter(_members(root)), root.__name__)),))


def id_path(struct: "bpy.types.bpy_struct[object]") -> Option[str]:
    """Returns path from ID to struct, empty for ID itself."""
    match struct:
        case bpy.types.ID():
            return Some("")
        case _:
            try:
                return Some(struct.path_from_id())
            except ValueError:
                return Nothing


def struct_at(root: Native, path: str) -> Option[Native]:
    """Returns struct or collection at relative path, Nothing for value."""
    try:
        found = (
            root
            if not path
            else root.path_resolve(path)
            if isinstance(root, bpy.types.bpy_struct)
            else (owner := root.data).path_resolve(  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                member_path(next(prop.identifier for prop in owner.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) and getattr(owner, prop.identifier) == root), path)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )
        )
    except (ValueError, ReferenceError):
        return Nothing
    return Some(found) if isinstance(found, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else Nothing


def effective(struct: Native) -> bool:
    """Whether host reads struct values, false under `GATED` member its governor disables."""
    return all(
        getattr(parent, governor.identifier) == value
        for parent, child in pairwise((*struct.rna_ancestors(), struct) if isinstance(struct, bpy.types.bpy_struct) else ())
        for owner, member, governor, value in GATED
        if isinstance(parent, owner) and getattr(parent, member.identifier) == child
    )


def written(struct: Native, identifier: str, value: "Value | bytes | bpy.types.bpy_struct[object]") -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
    """Writes member at dotted path through `Descriptor.write`."""
    return Descriptor.resolve(struct, identifier, bpy.types.Property).bind(
        lambda descriptor: struct_at(struct, descriptor.owner).map(lambda owner: descriptor.write(owner, value)).default_with(lambda: Error((BlenderAbsentFault(owner=repr(struct), key=identifier),)))
    )


def sequenced[W](writes: Iterable[tuple[str, Native, Descriptor, W]]) -> tuple[tuple[str, Native, Descriptor, W], ...]:
    """Orders writes in host order.

    `LEADING` members go first, each resetting members it governs.
    Others group by owner path in holder class declaration order (socket `subtype` and `dimensions` before `default_value`).
    Indexed elements follow their whole array.

    Args:
        writes: Owner path, holder struct, member descriptor, and value per write.

    Returns:
        Sorted writes.
    """
    return tuple(
        sorted(
            writes,
            key=lambda write: (
                LEADING.index(write[2].prop) if write[2].prop in LEADING else len(LEADING),
                write[0],
                tuple(prop.identifier for prop in write[1].bl_rna.properties).index(write[2].prop.identifier),  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                write[2].index.is_some(),
            ),
        )
    )


def differing(members: Mapping[Descriptor, Value], root: Native) -> dict[Descriptor, Value]:
    """Returns members with owner under root absent or holding another value."""
    return {descriptor: value for descriptor, value in members.items() if not any(descriptor.equal(descriptor.read(owner), value) for owner in struct_at(root, descriptor.owner).to_list())}


def holds(struct: Native, identifier: str, value: Value) -> bool:
    """Whether member at dotted path equals value under `Descriptor.equal`."""
    return Descriptor.resolve(struct, identifier, bpy.types.Property).map(lambda descriptor: not differing({descriptor: value}, struct)).default_value(False)


def assigned(members: Mapping[Descriptor, Value], root: "bpy.types.bpy_struct[object]") -> BlenderResult[None]:
    """Writes members under root in `sequenced` order.

    Each write re-resolves root from its ID, writes (`dimensions`) refining root class.

    Args:
        members: Value per member descriptor.
        root: Struct where descriptor owner paths start.

    Returns:
        Faults per unresolved owner and refused write, accumulated.
    """
    located = tuple((descriptor, value, struct_at(root, descriptor.owner)) for descriptor, value in members.items())

    def write(descriptor: Descriptor, value: Value) -> BlenderResult[None]:
        current = id_path(root).bind(lambda path: struct_at(block, path)).default_value(root) if isinstance(block := root.id_data, bpy.types.ID) else root
        return (
            struct_at(current, descriptor.owner)
            .map(lambda owner: descriptor.write(owner, value))
            .default_value(Error((BlenderAbsentFault(owner=repr(root), key=descriptor.path),)))
            .map(lambda _: None)
        )

    return accumulated((
        *(Error((BlenderAbsentFault(owner=repr(root), key=descriptor.path),)) for descriptor, _, holder in located if holder.is_none()),
        *(write(descriptor, value) for _, _, descriptor, value in sequenced((descriptor.owner, held, descriptor, value) for descriptor, value, holder in located for held in holder.to_list())),
    )).map(lambda _: None)


def _members[K: Native](root: type[K]) -> tuple[str, ...]:
    """Returns context pointers able to hold root type."""
    return tuple(prop.identifier for prop in bpy.types.Context.bl_rna.properties if isinstance(prop, bpy.types.PointerProperty) and issubclass(root, type(prop.fixed_type)))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "GATED",
    "LEADING",
    "Descriptor",
    "IdName",
    "Native",
    "P_co",
    "Scalar",
    "Value",
    "assigned",
    "beneath",
    "differing",
    "effective",
    "holds",
    "id_path",
    "member_path",
    "root_in",
    "rooted",
    "sequenced",
    "struct_at",
    "typed",
    "written",
]
