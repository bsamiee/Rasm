"""RNA property definitions, stored values, and their native write boundary."""

from collections.abc import Callable
from functools import reduce
from operator import getitem, setitem
import os
import re
from typing import Annotated, Generic, Self, TypeVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
from numpy.typing import NDArray

from rasm.blender.data.ids.store import blocks
from rasm.blender.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult

# --- [TYPES] ----------------------------------------------------------------------------

type Scalar = bool | int | float | str
type Value = Scalar | tuple[Scalar, ...] | IdName | None
type Native = bpy.types.bpy_struct[object] | bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]]
P_co = TypeVar("P_co", bound=bpy.types.Property, covariant=True, default=bpy.types.Property)

# --- [MODELS] ---------------------------------------------------------------------------


class IdName(msgspec.Struct, frozen=True, kw_only=True):
    """Data-block identity within its local or linked library."""

    name: str
    library: str | None = None

    @classmethod
    def of(cls, block: bpy.types.ID) -> Self:
        """Returns the data-block's name and its library path when linked."""
        return cls(name=block.name, library=Option.of_optional(block.library).map(lambda library: library.filepath).to_optional())


class Descriptor(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """Property definition resolved from a root type without retaining live data."""

    root: "type[Native]"
    owner: str
    prop: P_co
    index: Option[int] = Nothing

    # --- [RESOLUTION]

    @property
    def path(self) -> str:
        """RNA path from the root to the property."""
        suffix = self.index.map(lambda index: "".join(f"[{coordinate}]" for coordinate in np.unravel_index(index, self.dimensions(self.prop)))).default_value("")
        return f"{self.owner}.{self.prop.identifier}{suffix}".removeprefix(".")

    @staticmethod
    def resolve[K: bpy.types.Property](root: "type[bpy.types.bpy_struct[object]] | Native", path: str, kind: type[K]) -> BlenderResult["Descriptor[K]"]:
        """Returns the resolved definition or the absent path or refused property kind."""
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
            if not isinstance(prop, kind):
                return Error((BlenderChoiceFault(path=f"{definition.identifier}.{path}", identifier=type(prop).__name__, accepted=(kind.__name__,)),))
            return indexed(prop, last[2]).map(
                lambda index: Descriptor[K](
                    root=type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root, owner=path[: last.start()].removesuffix("."), prop=prop, index=index
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

    def owner_of(self, context: Native, item: "Option[Native]" = Nothing) -> "Option[Native]":
        """Returns an applicable live owner from a root or native context."""
        root = item.filter(lambda value: isinstance(value, self.root)).or_else_with(
            lambda: Some(context) if isinstance(context, self.root) else root_in(self.root, context) if isinstance(context, bpy.types.Context) else Nothing
        )
        return root.bind(lambda value: struct_at(value, self.owner)).filter(
            lambda owner: owner.bl_rna.properties.get(self.prop.identifier) == self.prop  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        )

    # --- [VALUES]

    @staticmethod
    def dimensions(prop: bpy.types.Property) -> tuple[int, ...]:
        """Returns a numeric array property's declared dimensions, empty for a scalar or another kind."""
        match prop:
            case bpy.types.BoolProperty() | bpy.types.IntProperty() | bpy.types.FloatProperty():
                return tuple(size for size in prop.array_dimensions if size)  # type: ignore[attr-defined]
            case _:
                return ()

    def shape(self, owner: Native) -> tuple[int, ...]:
        """Returns the array dimensions the live owner's class declares, which a subtype class can narrow."""
        return self.dimensions(owner.bl_rna.properties[self.prop.identifier])  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def read(self, owner: Native) -> Value:
        """Returns a scalar, flat array, enum identifiers, block identity, or null pointer."""
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

    def equal(self, held: "Value | bpy.types.bpy_struct[object]", desired: "Value | bytes | bpy.types.bpy_struct[object]") -> bool:
        """Compares native float32 bits and other values exactly."""
        if isinstance(self.prop, bpy.types.StringProperty) and isinstance(desired, bytes):
            desired = msgspec.to_builtins(desired) if self.prop.subtype == "BYTE_STRING" else os.fsdecode(desired)
        return (
            bool(np.array_equal(np.asarray(held, np.float32).view(np.uint32), np.asarray(desired, np.float32).view(np.uint32))) if isinstance(self.prop, bpy.types.FloatProperty) else held == desired
        )

    def write(self, owner: Native, value: "Value | bytes | bpy.types.bpy_struct[object]") -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
        """Returns the prior value after assignment, Nothing if equal, or a native refusal."""
        name = self.prop.identifier
        try:
            if isinstance(self.prop, bpy.types.FloatProperty):
                array = self.prop.is_array
                source = tuple(float(item) if isinstance(item, bool) else item for item in value) if isinstance(value, tuple) else float(value) if isinstance(value, bool) else value
                number = msgspec.convert(source, tuple[float, ...]) if array and self.index.is_none() else msgspec.convert(source, float)
                match self.quantized(self.prop, number):
                    case Result(tag="ok", ok=target):
                        value = tuple(target.tolist()) if array and self.index.is_none() else float(target)
                    case Result(error=faults):
                        return Error(faults)
            elif isinstance(self.prop, bpy.types.IntProperty):
                low, high, array = self.prop.hard_min, self.prop.hard_max, self.prop.is_array
                source = tuple(int(item) if isinstance(item, bool) else item for item in value) if isinstance(value, tuple) else int(value) if isinstance(value, bool) else value
                limits = msgspec.Meta(ge=low, le=high)
                value = msgspec.convert(source, tuple[Annotated[int, limits], ...] if array and self.index.is_none() else Annotated[int, limits])
            elif isinstance(self.prop, bpy.types.StringProperty):
                value = msgspec.convert(value, bytes) if self.prop.subtype == "BYTE_STRING" else value if isinstance(value, bytes) else msgspec.convert(value, str)
                match self.admitted(self.prop, value):
                    case Result(tag="ok", ok=payload):
                        value = payload
                    case Result(error=faults):
                        return Error(faults)
            prior = getattr(owner, name) if isinstance(self.prop, bpy.types.PointerProperty) else self.read(owner)
            compared = IdName.of(prior) if isinstance(prior, bpy.types.ID) and isinstance(value, IdName) else prior

            def published(_: object) -> "Option[Value | bpy.types.bpy_struct[object]]":
                if self.prop.is_runtime and isinstance(owner, bpy.types.bpy_struct):
                    bpy.msgbus.publish_rna(key=owner.path_resolve(name, False))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                return Some(prior)

            def assigned(change: Callable[[], object]) -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
                return Ok(published(change()))

            match self.prop, value, self.index:
                case _, _, _ if self.equal(compared, value):
                    return Ok(Nothing)
                case (
                    bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True),
                    bool() | int() | float(),
                    Option(tag="some", some=index),
                ) if isinstance(owner, bpy.types.bpy_struct):
                    coordinates = np.unravel_index(index, self.shape(owner))
                    return assigned(lambda: setitem(reduce(getitem, coordinates[:-1], owner.path_resolve(name, False)), coordinates[-1], value))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case bpy.types.EnumProperty() as prop, _, _:
                    identifiers = msgspec.convert(value, tuple[str, ...]) if prop.is_enum_flag else (msgspec.convert(value, str),)
                    accepted = tuple(item.identifier for item in prop.enum_items if bpy.types.UILayout.enum_item_name(owner, name, item.identifier))
                    refusals = tuple(
                        BlenderChoiceFault(path=f"{owner!r}.{name}", identifier=identifier, accepted=accepted)
                        for identifier in identifiers
                        if not bpy.types.UILayout.enum_item_name(owner, name, identifier)
                    )
                    match refusals:
                        case (first, *rest):
                            return Error((first, *rest))
                        case _:
                            return assigned(lambda: setattr(owner, name, set(identifiers) if prop.is_enum_flag else identifiers[0]))
                case bpy.types.PointerProperty(), bpy.types.bpy_struct(), _:
                    return assigned(lambda: setattr(owner, name, value))
                case bpy.types.PointerProperty(fixed_type=bpy.types.ID() as pointed), IdName() as identity, _:
                    found = next((held for held in blocks(bpy.data, type(pointed)) if IdName.of(held) == identity), None)  # type: ignore[unreachable]
                    return assigned(lambda: setattr(owner, name, found)) if found is not None else Error((BlenderAbsentFault(owner=type(pointed).__name__, key=identity.name),))
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), tuple(), _:
                    return assigned(lambda: setattr(owner, name, np.asarray(value).reshape(self.shape(owner)).tolist()))
                case _:
                    return assigned(lambda: setattr(owner, name, value))

        except msgspec.ValidationError as error:
            return Error((BlenderReadFault(request=f"{owner!r}.{name}", error=error),))
        except (AttributeError, ReferenceError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=f"{owner!r}.{name}", error=error),))

    @staticmethod
    def quantized(prop: bpy.types.FloatProperty, value: float | tuple[float, ...] | NDArray[np.float64]) -> BlenderResult[NDArray[np.float32]]:
        """Returns native float32 storage admitted against the property's hard interval."""
        try:
            with np.errstate(over="ignore"):
                target = np.asarray(value, dtype=np.float32)
            return (
                Error((BlenderReadFault(request=repr(prop), error=ValueError(f"Value outside native interval [{prop.hard_min}, {prop.hard_max}]")),))
                if np.any((target < prop.hard_min) | (target > prop.hard_max))
                else Ok(target)
            )
        except (TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=repr(prop), error=error),))

    @staticmethod
    def admitted[T: str | bytes](prop: bpy.types.StringProperty, value: T) -> BlenderResult[T]:
        """Returns a string payload admitted by the native property's representation."""
        try:
            if prop.subtype != "BYTE_STRING":
                msgspec.convert(os.fsdecode(value) if prop.subtype in {"FILE_PATH", "DIR_PATH", "FILE_NAME"} else value, Annotated[str, msgspec.Meta(pattern=r"^[^\x00]*$")])
            payload = (os.fsencode(value) if prop.subtype in {"FILE_PATH", "DIR_PATH", "FILE_NAME"} else value.encode("utf-8")) if isinstance(value, str) else value
            if prop.length_max:
                msgspec.convert(payload, Annotated[bytes, msgspec.Meta(max_length=prop.length_max - 1)])
            return Ok(value)
        except msgspec.ValidationError as error:
            return Error((BlenderReadFault(request=repr(prop), error=error),))
        except (TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=repr(prop), error=error),))

    def shown(self, value: Value) -> str:
        """Returns the host enum label or display text of a stored value."""
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


def root_in[K: Native](root: type[K], context: bpy.types.Context) -> Option[K]:
    """Returns a context-held native root, or Nothing when that root is absent."""
    candidates = (
        (context,)
        if root is bpy.types.Context
        else Option.of_optional(context.preferences.addons.get(root.bl_idname)).map(lambda addon: addon.preferences).to_list()  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        if issubclass(root, bpy.types.AddonPreferences)
        else (getattr(context, prop.identifier) for prop in bpy.types.Context.bl_rna.properties if isinstance(prop, bpy.types.PointerProperty) and issubclass(root, type(prop.fixed_type)))
    )
    return Option.of_optional(next((value for value in candidates if isinstance(value, root)), None))


def id_path(struct: "bpy.types.bpy_struct[object]") -> Option[str]:
    """Returns the RNA path from the struct's data-block, empty for the data-block itself, absent where Blender defines no path."""
    match struct:
        case bpy.types.ID():
            return Some("")
        case _:
            try:
                return Some(struct.path_from_id())
            except ValueError:
                return Nothing


def struct_at(root: Native, path: str) -> "Option[Native]":
    """Returns the struct or collection at a relative path, or Nothing for an absent step, an empty pointer, or a value."""
    found: object
    try:
        match root:
            case _ if not path:
                found = root
            case bpy.types.bpy_struct():
                found = root.path_resolve(path)
            case _:
                owner: bpy.types.bpy_struct[object] = root.data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                identifier = next(prop.identifier for prop in owner.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) and getattr(owner, prop.identifier) == root)
                found = owner.path_resolve(f"{identifier}{'' if path.startswith('[') else '.'}{path}")
    except (ValueError, ReferenceError):
        return Nothing
    return Some(found) if isinstance(found, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else Nothing


def written(struct: Native, identifier: str, value: "Value | bytes | bpy.types.bpy_struct[object]") -> "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]":
    """Resolves and writes a live member, returning absence or the native write refusal."""
    return Descriptor.resolve(struct, identifier, bpy.types.Property).bind(lambda descriptor: descriptor.write(struct, value))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Descriptor", "IdName", "Native", "Scalar", "Value", "id_path", "root_in", "struct_at", "written"]
