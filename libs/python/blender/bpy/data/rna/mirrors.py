"""`bpy.props` keywords and `EnumProperty` items mirroring host properties and module rows, and the form declarations groups register with."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
import enum
import functools
from functools import partial, reduce
import inspect
import operator
import re
from typing import Final, get_args, NotRequired, overload, TYPE_CHECKING, TypeAliasType, TypedDict
import zlib

import bpy
from expression import Ok, Option, Result, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import PropertyFlagEnumItems, PropertySubtypeNumberItems, PropertySubtypeStringItems, PropertyUnitItems

# --- [TYPES] ----------------------------------------------------------------------------

type _Item = tuple[str, str, str, str, int]


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
type Changed = Callable[[Struct, bpy.types.Context], None]

# --- [CONSTANTS] ------------------------------------------------------------------------

_REGISTER: Final[Callable[[type[Struct], str, bpy.props._PropertyDeferred], Result[None, ValueError]]] = catch(setattr, exception=ValueError)

# --- [MODELS] ---------------------------------------------------------------------------


class Updated(msgspec.Struct, frozen=True, kw_only=True):
    """Form classes registered with a change callback after each value property's declared update, held through `updating` while registered."""

    groups: tuple[type[Struct], ...]
    change: Changed

    @contextmanager
    def acquisitions(self, _identity: Identity, /) -> Generator[Outcome[tuple[type[Struct], ...]]]:
        """Yields the classes while their fresh definitions stand."""
        held: Outcome[tuple[type[Struct], ...]] = Ok(self.groups)
        with updating(self.groups, self.change):
            yield held


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MIRRORS]


def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str]]) -> tuple[_Item, ...]:
    """Returns `EnumProperty` items, each value fixed per identifier.

    Host items the owning property declares statically keep their value. Config items and module rows take a value hashed from the identifier, which stays fixed when rows move or the config changes.

    Args:
        rows: Host enum items or module `(identifier, name, description)` rows.

    Returns:
        Five-part items in row order.

    Raises:
        ValueError: Two rows share a value.
    """

    def item(row: bpy.types.EnumPropertyItem | tuple[str, str, str]) -> _Item:
        match row:
            case bpy.types.EnumPropertyItem() as host:
                fixed = (
                    Block
                    .of_seq(reversed(host.rna_ancestors()))
                    .choose(lambda parent: Option.of_optional(try_downcast(bpy.types.EnumProperty, parent)))
                    .filter(lambda prop: host.identifier in prop.enum_items_static)
                )
                return (host.identifier, host.name, host.description, host.icon, host.value if fixed.try_head().is_some() else zlib.crc32(host.identifier.encode()) & 0x7FFFFFFF)
            case (identifier, name, description):
                return (identifier, name, description, "NONE", zlib.crc32(identifier.encode()) & 0x7FFFFFFF)

    items = tuple(map(item, rows))
    values = tuple(held[4] for held in items)
    if len(set(values)) != len(values):
        raise ValueError(tuple(held[0] for held in items if values.count(held[4]) > 1))
    return items


def flag_choices(kind: type[enum.Flag]) -> tuple[_Item, ...]:
    """Returns `ENUM_FLAG` items of a `Flag` type, each member's name split into words as name and its bit as value."""
    return tuple((name, bpy.path.display_name(name.lower()), "", "NONE", member.value) for member in kind if (name := member.name) is not None)


@functools.cache
def held_choices(rows: tuple[tuple[str, str, str], ...], /) -> tuple[_Item, ...]:
    """Returns `choices` of a dynamic enum's rows, cached by content so the strings Blender reads stay referenced while the rows stand."""
    return choices(rows)


def flagged[F: enum.Flag](kind: type[F], names: Iterable[str]) -> F:
    """Returns the flag an `ENUM_FLAG` property's identifier set holds, its members iterating in declaration order."""
    return reduce(operator.or_, (kind[name] for name in names), kind(0))


def case_choices(union: TypeAliasType) -> tuple[_Item, ...]:
    """Returns `choices` over a tagged union alias's cases: the case tag as identifier, its class name split into words as name, its docstring as description."""
    return choices((case.__name__, bpy.path.display_name(re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", case.__name__)), inspect.getdoc(case) or "") for case in get_args(union.__value__))


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

    def numeric(held: bpy.types.FloatProperty | bpy.types.IntProperty | bpy.types.BoolProperty) -> "PropertySubtypeNumberItems":
        subtype: PropertySubtypeNumberItems = held.subtype  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return subtype

    match prop:
        case bpy.types.FloatProperty() as real:
            return _FloatMirror(
                name=real.name,
                description=real.description,
                subtype=numeric(real),
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
            return _IntMirror(
                name=integer.name,
                description=integer.description,
                subtype=numeric(integer),
                min=integer.hard_min,
                max=integer.hard_max,
                soft_min=integer.soft_min,
                soft_max=integer.soft_max,
                step=integer.step,
                default=integer.default,
            )
        case bpy.types.BoolProperty() as flag:
            return _BoolMirror(name=flag.name, description=flag.description, subtype=numeric(flag), default=flag.default)
        case bpy.types.StringProperty() as text:
            textual: PropertySubtypeStringItems = text.subtype  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return _StringMirror(name=text.name, description=text.description, subtype=textual, default=text.default, maxlen=text.length_max)
        case bpy.types.EnumProperty(is_enum_flag=True) as enum:
            return _EnumMirror(name=enum.name, description=enum.description, items=choices(enum.enum_items), default=set(enum.default_flag), options={"ENUM_FLAG"})
        case bpy.types.EnumProperty() as enum:
            return _EnumMirror(name=enum.name, description=enum.description, items=choices(enum.enum_items), default=enum.default)


# --- [DECLARATIONS]


def annotated(form: type[Struct]) -> dict[str, bpy.props._PropertyDeferred]:
    """Property definitions a form class and its bases declare, bases first, as Blender registers them."""
    return {name: definition for kind in reversed(form.__mro__) for name, definition in vars(kind).get("__annotations__", {}).items() if isinstance(definition, bpy.props._PropertyDeferred)}


def nested(form: type[bpy.types.PropertyGroup]) -> tuple[type[bpy.types.PropertyGroup], ...]:
    """Property group classes a form's pointer and collection properties hold, children first."""
    return tuple(
        child
        for definition in annotated(form).values()
        if definition.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
        for target in (definition.keywords["type"],)
        if issubclass(target, bpy.types.PropertyGroup)
        for child in (*nested(target), target)
    )


def updated(definition: bpy.props._PropertyDeferred, change: Changed) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition of a value property running the change callback after its declared update, any other definition as declared."""
    valued = definition.function is not bpy.props.CollectionProperty and not issubclass(definition.keywords.get("type", bpy.types.ID), bpy.types.PropertyGroup)
    declared: Changed | None = definition.keywords.get("update")

    def changed(owner: Struct, context: bpy.types.Context) -> None:
        if declared is not None:
            declared(owner, context)
        change(owner, context)

    return definition.function(**{**definition.keywords, "update": changed}) if valued else definition


@contextmanager
def updating(groups: tuple[type[Struct], ...], change: Changed) -> Generator[None]:
    """Declares every value property of the groups and their bases through `updated` while the scope registers them.

    Each declaring class holds the fresh definitions for the scope and its own definitions again at exit, no definition changing.

    Args:
        groups: Form classes the scope registers.
        change: Update callback every value property runs.

    Yields:
        None while the fresh definitions stand.
    """
    with ExitStack() as stack:
        for kind in dict.fromkeys(kind for group in groups for kind in reversed(group.__mro__)):
            declared: dict[str, object] = vars(kind).get("__annotations__", {})
            for name, definition in tuple(declared.items()):
                if isinstance(definition, bpy.props._PropertyDeferred):
                    declared[name] = updated(definition, change)
                    stack.callback(declared.__setitem__, name, definition)
        yield


@contextmanager
def pointed(owner: type[Struct], name: str, definition: bpy.props._PropertyDeferred) -> Generator[Outcome[None]]:
    """Registers property under a free name for scope duration, the host refusing a name past its byte limit."""
    request = f"{owner.__name__}.{name}"
    rna: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with ExitStack() as stack:
        yield (
            Result[None, Faults](ok=None)
            .filter_with(lambda _: not (name in rna.properties or name in rna.functions or hasattr(owner, name)), lambda _: Fault.of(BlenderReason.TAKEN, request, member=owner.__name__))
            .bind(lambda _: _REGISTER(owner, name, definition).map_error(partial(Fault.of, BlenderReason.REFUSED, request)))
            .map(lambda _: stack.callback(delattr, owner, name))
            .map(lambda _: None)
        )


@contextmanager
def reconciling[T: bpy.types.ID](kind: type[T], write: Callable[[Handle[T], Identity], Outcome[Option[str]]], identity: Identity) -> Generator[Changed]:
    """Yields the update callback deferring one reconcile per changed holder of kind, an attachment binding kind and write through `partial`.

    Args:
        kind: Holder class write resolves.
        write: Deferred write reconciling holder.
        identity: Extension whose reports defer the write.

    Yields:
        Update callback while the deferral stands.
    """
    with identity.reports.deferred(write) as deferred:

        def changed(owner: Struct, _context: bpy.types.Context) -> None:
            for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                if isinstance(found, kind):
                    deferred(Handle.of(found), identity)

        yield changed


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Changed", "Updated", "annotated", "case_choices", "choices", "flag_choices", "flagged", "held_choices", "mirrored", "nested", "pointed", "reconciling", "updated", "updating"]
