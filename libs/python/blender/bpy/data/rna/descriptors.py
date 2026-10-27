"""RNA paths, property definitions, stored values, native writes, and `bpy.props` mirrors."""

import ast
from collections.abc import Callable, Iterable, Mapping
from functools import partial
from itertools import pairwise
import os
from types import MappingProxyType
from typing import Annotated, Final, Generic, NotRequired, overload, Protocol, TYPE_CHECKING, TypedDict, TypeVar
import zlib

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.ids.store import IdName
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import PropertyFlagEnumItems, PropertySubtypeNumberItems, PropertySubtypeStringItems, PropertyUnitItems

# --- [TYPES] ----------------------------------------------------------------------------

type Scalar = bool | int | float | str
type Value = Scalar | tuple[Scalar, ...] | IdName | None
type Struct = bpy.types.bpy_struct[object]
type Native = Struct | bpy.types.bpy_prop_collection[Struct]
type _Step = str | int | tuple[str]
type RnaPath = tuple[_Step, ...]
type _Item = tuple[str, str, str, str, int]
P_co = TypeVar("P_co", bound=bpy.types.Property, covariant=True, default=bpy.types.Property)


class _Array(Protocol):
    """Uncoerced host array, which the stubs type as a bare property."""

    def __len__(self) -> int: ...

    def foreach_get(self, seq: NDArray[np.generic], /) -> None: ...


class _FloatMirror(TypedDict):
    """`bpy.props.FloatProperty` keywords of a scalar float member."""

    name: str
    description: str
    subtype: "PropertySubtypeNumberItems"
    unit: "PropertyUnitItems"
    min: float
    max: float
    soft_min: float
    soft_max: float
    step: float
    precision: int
    default: float


class _IntMirror(TypedDict):
    """`bpy.props.IntProperty` keywords of a scalar integer member."""

    name: str
    description: str
    subtype: "PropertySubtypeNumberItems"
    min: int
    max: int
    soft_min: int
    soft_max: int
    step: int
    default: int


class _BoolMirror(TypedDict):
    """`bpy.props.BoolProperty` keywords of a scalar boolean member."""

    name: str
    description: str
    subtype: "PropertySubtypeNumberItems"
    default: bool


class _StringMirror(TypedDict):
    """`bpy.props.StringProperty` keywords of a string member."""

    name: str
    description: str
    subtype: "PropertySubtypeStringItems"
    default: str
    maxlen: int


class _EnumMirror(TypedDict):
    """`bpy.props.EnumProperty` keywords of an enum member, `options` present for a flag enum."""

    name: str
    description: str
    items: tuple[_Item, ...]
    default: str | set[str]
    options: NotRequired[set["PropertyFlagEnumItems"]]


type _Mirror = _FloatMirror | _IntMirror | _BoolMirror | _StringMirror | _EnumMirror


# --- [CONSTANTS] ------------------------------------------------------------------------

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
_GATED = tuple(
    (bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[member], bpy.types.ImageFormatSettings.bl_rna.properties["color_management"], "OVERRIDE")
    for member in ("linear_colorspace_settings", "view_settings", "display_settings")
)
_PATHS: Final = frozenset({"FILE_PATH", "DIR_PATH", "FILE_NAME"})
DTYPES: Final[Mapping[type[bpy.types.Property], type[np.bool | np.int32 | np.float32]]] = MappingProxyType({
    bpy.types.BoolProperty: np.bool,
    bpy.types.IntProperty: np.int32,
    bpy.types.FloatProperty: np.float32,
    bpy.types.EnumProperty: np.int32,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Descriptor(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """Property definition from root type, no live data."""

    root: type[Native]
    owner: RnaPath = ()
    prop: P_co
    coordinates: tuple[int, ...] = ()

    # --- [RESOLUTION]

    @property
    def path(self) -> RnaPath:
        """RNA path from root to property with element coordinates."""
        return (*self.owner, self.prop.identifier, *self.coordinates)

    @property
    def index(self) -> Option[int]:
        """Flat element index the host's `index=` keyword takes, Nothing for a whole member."""
        return Some(self.coordinates).filter(bool).map(lambda at: int(np.ravel_multi_index(at, self.dimensions(self.prop))))

    @staticmethod
    def resolve[K: bpy.types.Property](root: type[Struct] | Native, path: str | RnaPath, kind: type[K]) -> Outcome["Descriptor[K]"]:
        """Resolves path to descriptor, choice fault for another kind.

        Args:
            root: Struct type whose definitions the path walks, or live struct whose refined definitions answer.
            path: RNA path, host path text read through `parsed`.
            kind: Property class the member must hold.

        Returns:
            Descriptor, absent fault for a path naming no member or element.
        """
        stated = type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root

        def owned(owner: RnaPath) -> Option[bpy.types.Struct]:
            return struct_at(root, owner).bind(_defined) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else _walked(definition(root), owner)

        def within(prop: bpy.types.Property, coordinates: tuple[int, ...]) -> bool:
            dimensions = Descriptor.dimensions(prop)
            return not coordinates or (len(coordinates) == len(dimensions) and all(0 <= coordinate < size for coordinate, size in zip(coordinates, dimensions)))

        return _steps(root, path).bind(
            lambda steps: (
                _split(steps)
                .bind(lambda split: owned(split[0]).bind(lambda owner: Option.of_optional(owner.properties.get(split[1]))).map(lambda prop: (split, prop)))
                .filter(lambda found: within(found[1], found[0][2]))
                .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(root), member=rna(steps)))
                .bind(lambda found: typed(found[1], kind, rna(steps)).map(lambda prop: Descriptor[K](root=stated, owner=found[0][0], prop=prop, coordinates=found[0][2])))
            )
        )

    @staticmethod
    def declared[K: bpy.types.Property](root: type[Struct], path: str | RnaPath, kind: type[K]) -> "Descriptor[K]":
        """Resolves path of a declaration over the pinned host, a missing member raising as a defect."""
        match Descriptor.resolve(root, path, kind):
            case Result(tag="ok", ok=descriptor):
                return descriptor
            case Result(error=faults):
                raise LookupError(path, faults)

    @staticmethod
    def saved(root: type[Struct] | Native, where: Callable[[bpy.types.Property], bool] = lambda _: True) -> "tuple[Descriptor, ...]":
        """Returns members a saved file stores, ID, name, hidden, and registered members left out.

        Args:
            root: Struct type, or live struct whose refined definition answers.
            where: Members kept beyond the saved rule.

        Returns:
            Descriptors of root's own members in declaration order.
        """
        held = definition(root)
        base = bpy.types.ID.bl_rna.properties.values()
        return tuple(
            Descriptor(root=type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root, prop=prop)
            for prop in held.properties
            if not (prop.is_hidden or prop.is_skip_save or prop.is_deprecated or prop.is_registered or prop.is_registered_optional or prop == held.name_property or prop in base) and where(prop)
        )

    def owner_of(self, context: Native, item: Option[Native] = Nothing) -> Option[Native]:
        """Returns live property owner from item, root, or context."""
        return (
            item
            .filter(lambda value: isinstance(value, self.root))
            .or_else_with(lambda: Some(context) if isinstance(context, self.root) else root_in(self.root, context) if isinstance(context, bpy.types.Context) else Nothing)
            .bind(lambda value: struct_at(value, self.owner))
            .filter(lambda owner: _defined(owner).bind(lambda held: Option.of_optional(held.properties.get(self.prop.identifier))) == Some(self.prop) and effective(owner))
        )

    # --- [BOUNDS]

    @property
    def meta(self) -> msgspec.Meta:
        """Record constraint of the member: hard range of a number."""
        match self.prop:
            case bpy.types.IntProperty() | bpy.types.FloatProperty() as number:
                return msgspec.Meta(ge=number.hard_min, le=number.hard_max)
            case _:
                return msgspec.Meta()

    # --- [VALUES]

    @staticmethod
    def dimensions(prop: bpy.types.Property) -> tuple[int, ...]:
        """Returns declared numeric array dimensions, empty for other properties."""
        match prop:
            case bpy.types.BoolProperty() | bpy.types.IntProperty() | bpy.types.FloatProperty() as array:
                sizes: Iterable[int] = array.array_dimensions  # type: ignore[assignment]
                return tuple(size for size in sizes if size)
            case _:
                return ()

    def shape(self, owner: Struct) -> tuple[int, ...]:
        """Returns live array dimensions, outer length read from the owner's uncoerced array, a flat `MATRIX` array keeping its flat length."""
        return (len(_uncoerced(owner, self.prop.identifier)), *self.dimensions(definition(owner).properties[self.prop.identifier])[1:])

    def read(self, owner: Native) -> Value:
        """Returns scalar, flat array, element, enum identifiers, `IdName`, struct identifier, or None."""
        name = self.prop.identifier
        match self.prop, owner:
            case bpy.types.EnumProperty(is_enum_flag=True), _:
                return tuple(sorted(getattr(owner, name)))
            case bpy.types.EnumProperty(is_runtime=True), _ if isinstance(stored := owner.get(name), str):
                return stored
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()), _:
                return Option.of_optional(getattr(owner, name)).map(IdName.of).to_optional()  # type: ignore[unreachable]
            case bpy.types.PointerProperty(), _:
                return Option.of_optional(getattr(owner, name)).map(lambda struct: definition(struct).identifier).to_optional()
            case bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as prop, bpy.types.bpy_struct():
                shape = self.shape(owner)
                buffer = np.empty(int(np.prod(shape)), DTYPES[type(prop)])
                _uncoerced(owner, name).foreach_get(buffer)
                return buffer.reshape(shape)[self.coordinates].item() if self.coordinates else tuple(buffer.tolist())
            case bpy.types.BoolProperty(is_array=True), _:
                flags = np.asarray(getattr(owner, name), np.bool)
                return flags[self.coordinates].item() if self.coordinates else tuple(flags.ravel().tolist())
            case _:
                return msgspec.convert(msgspec.to_builtins(getattr(owner, name)), bool | int | float | str)

    def equal(self, held: Value | Struct, desired: object) -> bool:
        """Compares floats by float32 bits, other values exactly."""
        if isinstance(self.prop, bpy.types.StringProperty) and isinstance(desired, bytes):
            desired = msgspec.to_builtins(desired) if self.prop.subtype == "BYTE_STRING" else os.fsdecode(desired)
        return np.array_equal(np.asarray(held, np.float32).view(np.uint32), np.asarray(desired, np.float32).view(np.uint32)) if isinstance(self.prop, bpy.types.FloatProperty) else held == desired

    def write(self, owner: Native, value: Value | bytes | Struct) -> Outcome[Option[Value | Struct]]:
        """Assigns value through the host, returning prior value or Nothing when equal.

        Args:
            owner: Struct holding the member.
            value: Stored value, `IdName` of a pointed ID, or struct.

        Returns:
            Prior value, domain fault outside a range the host clamps, refusal fault carrying the host's error.
        """
        name = self.prop.identifier
        prior = getattr(owner, name) if isinstance(self.prop, bpy.types.PointerProperty) else self.read(owner)
        held = IdName.of(prior) if isinstance(prior, bpy.types.ID) and isinstance(value, IdName) else prior

        def published(_: object) -> Option[Value | Struct]:
            if self.prop.is_runtime and isinstance(owner, bpy.types.bpy_struct):
                key: bpy.types.Property = owner.path_resolve(name, False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                bpy.msgbus.publish_rna(key=key)
            return Some(prior)

        def stored() -> Outcome[object]:
            match self.prop, value:
                case bpy.types.FloatProperty() as real, _:
                    number: Outcome[float | tuple[float, ...]] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, float | tuple[float, ...]).map_error(
                        partial(_outside, repr(real), value)
                    )
                    return number.bind(partial(self.quantized, real)).map(lambda held: tuple(held.tolist()) if held.ndim else float(held))
                case bpy.types.IntProperty(hard_min=low, hard_max=high) as integer, _:
                    bounded = Annotated[int, msgspec.Meta(ge=low, le=high)]
                    return _converted(value, bounded | tuple[bounded, ...], repr(integer))
                case bpy.types.StringProperty(subtype="BYTE_STRING") as text, str(encoded):
                    decoded: Outcome[bytes] = catch(msgspec.convert, exception=msgspec.ValidationError)(encoded, bytes).map_error(partial(_outside, repr(text), value))
                    return decoded.bind(partial(self.validated, text))
                case bpy.types.StringProperty() as text, str() | bytes() as content:
                    return self.validated(text, content)
                case _:
                    return Ok(value)

        return (
            Ok(Nothing)
            if self.equal(held, value)
            else stored()
            .bind(lambda native: self._native(owner, native))
            .bind(lambda native: _assign(owner, name, native).map_error(partial(Fault.of, BlenderReason.REFUSED, rna(self.path))))
            .map(published)
        )

    def _native(self, owner: Native, stored: object) -> Outcome[object]:
        """Returns the host form of a stored value: the whole array an element write sets, an enum flag set, a pointed ID, or a live-shaped array."""
        match self.prop, stored:
            case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), _ if self.coordinates and isinstance(
                owner, bpy.types.bpy_struct
            ):
                buffer = np.array(msgspec.structs.replace(self, coordinates=()).read(owner)).reshape(self.shape(owner))
                buffer[self.coordinates] = stored
                return Ok(buffer.tolist())
            case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), tuple() if isinstance(owner, bpy.types.bpy_struct):
                return Ok(np.asarray(stored).reshape(self.shape(owner)).tolist())
            case bpy.types.EnumProperty(is_enum_flag=True), tuple(chosen):
                return Ok(set(chosen))
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID() as pointed), IdName() as identity:
                return identity.resolve(type(pointed)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, type(pointed).__name__, member=identity.name))  # type: ignore[unreachable]
            case _:
                return Ok(stored)

    @staticmethod
    @np.errstate(over="ignore")
    def quantized(prop: bpy.types.FloatProperty, value: float | tuple[float, ...] | NDArray[np.float64]) -> Outcome[NDArray[np.float32]]:
        """Returns float32 storage, domain fault outside hard range."""
        return Ok(np.asarray(value, dtype=np.float32)).filter_with(
            lambda target: not np.any((target < prop.hard_min) | (target > prop.hard_max)),
            lambda _: Fault.of(RecordReason.OUTSIDE, repr(prop), str(value), member=f"values within hard range [{prop.hard_min}, {prop.hard_max}]"),
        )

    @staticmethod
    def validated[T: str | bytes](prop: bpy.types.StringProperty, value: T) -> Outcome[T]:
        """Returns storable value, domain fault for NUL or text past the host's length."""
        encoded = (os.fsencode(value) if prop.subtype in _PATHS else value.encode("utf-8")) if isinstance(value, str) else value
        text = Ok(value) if prop.subtype == "BYTE_STRING" else _converted(os.fsdecode(value) if prop.subtype in _PATHS else value, Annotated[str, msgspec.Meta(pattern=r"^[^\x00]*$")], repr(prop))
        return text.bind(lambda _: _converted(encoded, Annotated[bytes, msgspec.Meta(max_length=prop.length_max - 1)], repr(prop)) if prop.length_max else Ok(encoded)).map(lambda _: value)

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
# --- [PATHS]


def rna(path: str | RnaPath) -> str:
    """Returns the host path text of a path, the one text the host reads, host path text passing as it is."""

    def token(step: _Step) -> str:
        match step:
            case str():
                return f".{step}"
            case int():
                return f"[{step}]"
            case (key,):
                return f'["{bpy.utils.escape_identifier(key)}"]'

    return path if isinstance(path, str) else "".join(map(token, path)).removeprefix(".")


def parsed(root: Native | type[Native], text: str) -> Outcome[RnaPath]:
    """Reads host path text as a path, a live root first passing the text through the host resolver.

    Args:
        root: Live struct or collection the text resolves under, or the type a declaration or host builder wrote it for.
        text: Host path text.

    Returns:
        Path, absent fault for text the host refuses.
    """
    refused = partial(Fault.of, BlenderReason.ABSENT, repr(root), member=text)
    accepted: Outcome[None] = (
        Ok(None) if isinstance(root, type) else _anchored(root).to_result_with(refused).bind(lambda anchor: _resolved(anchor[0], f"{rna(anchor[1])}{text}").map(lambda _: None).map_error(refused))
    )
    return (
        Ok(()) if not text else accepted.bind(lambda _: _expression(f"_{text}" if text.startswith("[") else f"_.{text}").map_error(refused).bind(lambda node: _stepped(node).to_result_with(refused)))
    )


def definition(struct: Native | type[Struct]) -> bpy.types.Struct:
    """Returns the RNA definition of a live struct, a collection holding its own definition, or a struct type."""
    held: bpy.types.Struct = struct.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
    return held


def id_path(struct: Struct) -> Option[RnaPath]:
    """Returns path from ID to struct, empty for ID itself.

    Structs whose type builds no RNA path (`CurveMapping`, `Stereo3dFormat`) take their path through the ancestor they were reached from.
    """
    match struct:
        case bpy.types.ID():
            return Some(())
        case _:
            return (
                catch(struct.path_from_id, exception=ValueError)()
                .to_option()
                .bind(lambda text: parsed(bpy.types.ID, text).to_option())
                .or_else_with(lambda: Block.of_seq(reversed(struct.rna_ancestors())).try_head().bind(partial(_held, struct)))
            )


def struct_at(root: Native, path: str | RnaPath) -> Option[Native]:
    """Returns struct or collection at relative path, Nothing for a value or a removed struct."""
    return _steps(root, path).to_option().bind(partial(_located, root))


# --- [CONTEXT]


def typed[T](value: object, kind: type[T], path: str) -> Outcome[T]:
    """Returns value of kind, choice fault otherwise."""
    return Option.of_optional(try_downcast(kind, value)).to_result_with(lambda: Fault.of(RecordReason.OUTSIDE, path, type(value).__name__, member=kind.__name__))


def subclass[T: Struct](base: type[T], identifier: str) -> Outcome[type[T]]:
    """Returns the `base` subclass `bpy.types` names by identifier, building a native class on first read."""
    match getattr(bpy.types, identifier, None):
        case type() as kind if issubclass(kind, base) and kind is not base:
            return Ok(kind)
        case _:
            return Error(Fault.of(BlenderReason.ABSENT, f"bpy.types.{base.__name__}", member=identifier))


def root_in[K: Native](root: type[K], context: bpy.types.Context) -> Option[K]:
    """Returns context-held root of type."""
    preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    candidates: Iterable[object]
    if root is bpy.types.Context:
        candidates = (context,)
    elif issubclass(root, bpy.types.AddonPreferences):
        candidates = Option.of_optional(preferences.addons.get(root.bl_idname)).map(lambda addon: addon.preferences).to_list()
    else:
        candidates = (getattr(context, member) for member in _members(root))
    return Block.of_seq(candidates).choose(lambda value: Option.of_optional(try_downcast(root, value))).try_head()


def rooted[K: Native](root: type[K], context: bpy.types.Context) -> Outcome[K]:
    """Returns context-held root, absent fault naming first matching member."""
    return root_in(root, context).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, "bpy.context", member=Block.of_seq(_members(root)).try_head().default_value(root.__name__)))


def effective(struct: Native) -> bool:
    """Whether host reads struct values, false under a gated member its governor disables."""
    return all(
        getattr(parent, governor.identifier) == value
        for parent, child in pairwise((*struct.rna_ancestors(), struct) if isinstance(struct, bpy.types.bpy_struct) else ())
        for owner, member, governor, value in _GATED
        if isinstance(parent, owner) and getattr(parent, member.identifier) == child
    )


# --- [PROPERTIES]

custom_read: Final[Callable[[Struct, str], Result[object, TypeError | ReferenceError]]] = catch(catch(bpy.types.bpy_struct.get, exception=ReferenceError), exception=TypeError)


# --- [WRITES]


def written(struct: Native, path: str | RnaPath, value: Value | bytes | Struct) -> Outcome[Option[Value | Struct]]:
    """Writes member at path through `Descriptor.write`."""
    return Descriptor.resolve(struct, path, bpy.types.Property).bind(
        lambda descriptor: (
            struct_at(struct, descriptor.owner).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(struct), member=rna(descriptor.path))).bind(lambda owner: descriptor.write(owner, value))
        )
    )


def sequenced[W](writes: Iterable[tuple[RnaPath, Struct, Descriptor, W]]) -> tuple[tuple[RnaPath, Struct, Descriptor, W], ...]:
    """Orders writes in host order.

    `LEADING` members go first, each resetting members it governs.
    Others group by owner path in holder class declaration order (socket `subtype` and `dimensions` before `default_value`).
    Elements follow their whole array.

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
                definition(write[1]).properties.find(write[2].prop.identifier),
                write[2].coordinates,
            ),
        )
    )


def differing[K: str | RnaPath | Descriptor](root: Native, members: Mapping[K, Value]) -> dict[K, Value]:
    """Returns members whose owner under root is absent or holds another value under `Descriptor.equal`.

    Args:
        root: Struct where descriptor owner paths and paths start.
        members: Value per member descriptor or path.

    Returns:
        Differing members under their given keys, an unresolved path among them.
    """
    return {
        key: value
        for key, value in members.items()
        if not any(descriptor.equal(descriptor.read(owner), value) for descriptor in _described(root, key).to_option().to_list() for owner in struct_at(root, descriptor.owner).to_list())
    }


def assigned[K: str | RnaPath | Descriptor](root: Struct, members: Mapping[K, Value]) -> Outcome[None]:
    """Writes members under root in `sequenced` order.

    Each write re-resolves root from its ID, writes (`dimensions`) refining root class.

    Args:
        root: Struct where descriptor owner paths and paths start.
        members: Value per member descriptor or path.

    Returns:
        Faults per unresolved path, unresolved owner, and refused write, accumulated.
    """
    described = tuple((_described(root, key), value) for key, value in members.items())
    located = tuple((descriptor, value, struct_at(root, descriptor.owner).bind(_struct)) for found, value in described for descriptor in found.to_option().to_list())

    def write(descriptor: Descriptor, value: Value) -> Outcome[None]:
        current = id_path(root).bind(lambda path: struct_at(block, path)).default_value(root) if isinstance(block := root.id_data, bpy.types.ID) else root
        return (
            struct_at(current, descriptor.owner)
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(root), member=rna(descriptor.path)))
            .bind(lambda owner: descriptor.write(owner, value))
            .map(lambda _: None)
        )

    return accumulated((
        *(found.map(lambda _: None) for found, _ in described if found.is_error()),
        *(Error(Fault.of(BlenderReason.ABSENT, repr(root), member=rna(descriptor.path))) for descriptor, _, holder in located if holder.is_none()),
        *(write(descriptor, value) for _, _, descriptor, value in sequenced((descriptor.owner, held, descriptor, value) for descriptor, value, holder in located for held in holder.to_list())),
    )).map(lambda _: None)


# --- [MIRRORS]


def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str]]) -> tuple[_Item, ...]:
    """Returns `EnumProperty` items, each value fixed per identifier.

    Host items the owning property declares statically keep their value. Config items and module rows take a value hashed from the identifier, which stays fixed when rows move or the config changes.

    Args:
        rows: Host enum items and module `(identifier, name, description)` rows.

    Returns:
        Five-part items in row order.

    Raises:
        ValueError: Two rows share a value.
    """

    def item(row: bpy.types.EnumPropertyItem | tuple[str, str, str]) -> _Item:
        match row:
            case (identifier, name, description):
                return (identifier, name, description, "NONE", zlib.crc32(identifier.encode()) & 0x7FFFFFFF)
            case bpy.types.EnumPropertyItem() as host:
                fixed = (
                    Block
                    .of_seq(reversed(host.rna_ancestors()))
                    .choose(lambda parent: Option.of_optional(try_downcast(bpy.types.EnumProperty, parent)))
                    .filter(lambda prop: host.identifier in prop.enum_items_static)
                )
                return (host.identifier, host.name, host.description, host.icon, host.value if fixed.try_head().is_some() else zlib.crc32(host.identifier.encode()) & 0x7FFFFFFF)

    items = tuple(map(item, rows))
    values = tuple(held[4] for held in items)
    if len(set(values)) != len(values):
        raise ValueError(tuple(held[0] for held in items if values.count(held[4]) > 1))
    return items


@overload
def mirrored(prop: bpy.types.FloatProperty) -> _FloatMirror: ...
@overload
def mirrored(prop: bpy.types.IntProperty) -> _IntMirror: ...
@overload
def mirrored(prop: bpy.types.BoolProperty) -> _BoolMirror: ...
@overload
def mirrored(prop: bpy.types.StringProperty) -> _StringMirror: ...
@overload
def mirrored(prop: bpy.types.EnumProperty) -> _EnumMirror: ...
def mirrored(prop: bpy.types.FloatProperty | bpy.types.IntProperty | bpy.types.BoolProperty | bpy.types.StringProperty | bpy.types.EnumProperty) -> _Mirror:
    """Returns the `bpy.props` keywords declaring a scalar host property's name, description, subtype, bounds, default, and items."""
    match prop:
        case bpy.types.FloatProperty() as real:
            numeric: PropertySubtypeNumberItems = real.subtype  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return _FloatMirror(
                name=real.name,
                description=real.description,
                subtype=numeric,
                unit=real.unit,
                min=real.hard_min,
                max=real.hard_max,
                soft_min=real.soft_min,
                soft_max=real.soft_max,
                step=real.step,
                precision=real.precision,
                default=real.default,
            )
        case bpy.types.IntProperty() as integer:
            integral: PropertySubtypeNumberItems = integer.subtype  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return _IntMirror(
                name=integer.name,
                description=integer.description,
                subtype=integral,
                min=integer.hard_min,
                max=integer.hard_max,
                soft_min=integer.soft_min,
                soft_max=integer.soft_max,
                step=integer.step,
                default=integer.default,
            )
        case bpy.types.BoolProperty() as flag:
            logical: PropertySubtypeNumberItems = flag.subtype  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return _BoolMirror(name=flag.name, description=flag.description, subtype=logical, default=flag.default)
        case bpy.types.StringProperty() as text:
            textual: PropertySubtypeStringItems = text.subtype  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return _StringMirror(name=text.name, description=text.description, subtype=textual, default=text.default, maxlen=text.length_max)
        case bpy.types.EnumProperty(is_enum_flag=True) as enum:
            return _EnumMirror(name=enum.name, description=enum.description, items=choices(enum.enum_items), default=set(enum.default_flag), options={"ENUM_FLAG"})
        case bpy.types.EnumProperty() as enum:
            return _EnumMirror(name=enum.name, description=enum.description, items=choices(enum.enum_items), default=enum.default)


# --- [READERS]


def _steps(root: Native | type[Native], path: str | RnaPath) -> Outcome[RnaPath]:
    """Returns path itself, or host path text read under root."""
    return parsed(root, path) if isinstance(path, str) else Ok(path)


@catch(exception=SyntaxError)
def _expression(text: str) -> ast.expr:
    """Parses path text over a `_` root through Python's own grammar, which the host's path grammar is a subset of."""
    return ast.parse(text, mode="eval").body


def _stepped(node: ast.expr) -> Option[RnaPath]:
    """Reads an attribute and subscript chain over the `_` root as steps, Nothing for any other expression."""
    match node:
        case ast.Name(id="_"):
            return Some(())
        case ast.Attribute(value=inner, attr=name):
            return _stepped(inner).map(lambda steps: (*steps, name))
        case ast.Subscript(value=inner, slice=ast.Constant(value=int(index))):
            return _stepped(inner).map(lambda steps: (*steps, index))
        case ast.Subscript(value=inner, slice=ast.Constant(value=str(key))):
            return _stepped(inner).map(lambda steps: (*steps, (key,)))
        case _:
            return Nothing


def _split(path: RnaPath, coordinates: tuple[int, ...] = ()) -> Option[tuple[RnaPath, str, tuple[int, ...]]]:
    """Splits a member path at its last identifier into owner path, identifier, and trailing coordinates."""
    match path:
        case (*owner, str(identifier)):
            return Some((tuple(owner), identifier, coordinates))
        case (*rest, int(coordinate)):
            return _split(tuple(rest), (coordinate, *coordinates))
        case _:
            return Nothing


def _walked(held: bpy.types.Struct, steps: RnaPath) -> Option[bpy.types.Struct]:
    """Walks definitions through pointers, collection elements, and a collection's own definition."""

    def entered(prop: bpy.types.Property, rest: RnaPath) -> Option[bpy.types.Struct]:
        match prop, rest:
            case bpy.types.CollectionProperty(), (int() | tuple(), *inner):
                return Option.of_optional(prop.fixed_type).bind(lambda fixed: _walked(fixed, tuple(inner)))
            case bpy.types.CollectionProperty(), _:
                return Option.of_optional(prop.srna).bind(lambda own: _walked(own, rest))
            case bpy.types.PointerProperty(), _:
                return Option.of_optional(prop.fixed_type).bind(lambda fixed: _walked(fixed, rest))
            case _:
                return Nothing

    match steps:
        case ():
            return Some(held)
        case (str(name), *rest):
            return Option.of_optional(held.properties.get(name)).bind(lambda prop: entered(prop, tuple(rest)))
        case _:
            return Nothing


def _located(root: Native, steps: RnaPath) -> Option[Native]:
    """Returns root for an empty path, else the struct or collection the host resolves."""
    match steps:
        case ():
            return Some(root)
        case _:
            return _anchored(root).bind(lambda anchor: _resolved(anchor[0], rna((*anchor[1], *steps))).to_option()).bind(_native)


def _anchored(root: Native) -> Option[tuple[Struct, RnaPath]]:
    """Returns the struct a path resolves against, a collection through its holder and holding member."""
    match root:
        case bpy.types.bpy_struct():
            return Some((root, ()))
        case _:
            return Option.of_optional(root.data).bind(
                lambda holder: (
                    Block
                    .of_seq(definition(holder).properties)
                    .filter(lambda prop: isinstance(prop, bpy.types.CollectionProperty) and getattr(holder, prop.identifier) == root)
                    .try_head()
                    .map(lambda prop: (holder, (prop.identifier,)))
                )
            )


def _path_resolved(root: Struct, text: str) -> object:
    """Returns what host text resolves to under root, uncoerced."""
    found: object = root.path_resolve(text, False)
    return found


_resolved: Final[Callable[[Struct, str], Result[object, ValueError | ReferenceError]]] = catch(catch(_path_resolved, exception=ReferenceError), exception=ValueError)
_assign: Final[Callable[[Native, str, object], Result[object, AttributeError | TypeError | ValueError]]] = catch(
    catch(catch(setattr, exception=AttributeError), exception=TypeError), exception=ValueError
)


def _uncoerced(owner: Struct, name: str) -> _Array:
    """Returns a member's uncoerced host array."""
    held: _Array = owner.path_resolve(name, False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held


def _outside(subject: str, value: object, error: Exception) -> Faults:
    """Returns the domain fault of a value a conversion refused."""
    return Fault.of(RecordReason.OUTSIDE, subject, str(value), member=str(error))


def _converted(value: object, kind: object, subject: str) -> Outcome[object]:
    """Returns value converted to kind, domain fault naming the refused constraint."""
    held: Outcome[object] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, kind).map_error(partial(_outside, subject, value))
    return held


def _native(found: object) -> Option[Native]:
    """Returns a struct or collection, Nothing for a value."""
    match found:
        case bpy.types.bpy_struct() | bpy.types.bpy_prop_collection():
            return Some(found)
        case _:
            return Nothing


def _struct(native: Native) -> Option[Struct]:
    """Returns a struct, Nothing for a collection."""
    return Option.of_optional(try_downcast(bpy.types.bpy_struct, native))


def _defined(native: Native) -> Option[bpy.types.Struct]:
    """Returns the definition of a struct, Nothing for a collection."""
    return _struct(native).map(definition)


def _held(struct: Struct, parent: Struct) -> Option[RnaPath]:
    """Returns path of the pointer member or collection element of parent holding struct."""
    return (
        Block
        .of_seq(definition(parent).properties)
        .filter(lambda prop: isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) and isinstance(struct, type(prop.fixed_type)))
        .choose(partial(_holding, parent, struct))
        .try_head()
        .bind(lambda steps: id_path(parent).map(lambda base: (*base, *steps)))
    )


def _holding(parent: Struct, struct: Struct, prop: bpy.types.Property) -> Option[RnaPath]:
    """Returns the steps under parent's member that reach struct."""
    match prop:
        case bpy.types.PointerProperty():
            return Some((prop.identifier,)).filter(lambda _: getattr(parent, prop.identifier) == struct)
        case _:
            return Block.of_seq(enumerate(getattr(parent, prop.identifier))).filter(lambda pair: pair[1] == struct).try_head().map(lambda pair: (prop.identifier, pair[0]))


def _described(root: Native, key: str | RnaPath | Descriptor) -> Outcome[Descriptor]:
    """Returns descriptor key itself, or the member a path names under root."""
    return Ok(key) if isinstance(key, Descriptor) else Descriptor.resolve(root, key, bpy.types.Property)


def _members[K: Native](root: type[K]) -> tuple[str, ...]:
    """Returns context pointers able to hold root type."""
    return tuple(prop.identifier for prop in bpy.types.Context.bl_rna.properties if isinstance(prop, bpy.types.PointerProperty) and issubclass(root, type(prop.fixed_type)))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "DTYPES",
    "LEADING",
    "Descriptor",
    "Native",
    "P_co",
    "RnaPath",
    "Scalar",
    "Struct",
    "Value",
    "assigned",
    "choices",
    "custom_read",
    "definition",
    "differing",
    "effective",
    "id_path",
    "mirrored",
    "parsed",
    "rna",
    "root_in",
    "rooted",
    "sequenced",
    "struct_at",
    "subclass",
    "typed",
    "written",
]
