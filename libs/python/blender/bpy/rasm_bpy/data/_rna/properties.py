"""`bpy.props` definitions and `EnumProperty` items copied from host properties, enum types, and module rows, and form registration with callbacks."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
import enum
from functools import partial, reduce
import inspect
from types import MappingProxyType
from typing import Final, get_args, get_type_hints, Never, overload, ReadOnly, TYPE_CHECKING, TypeAliasType, TypedDict
import zlib

import bpy
from expression import Option, Result
from expression.extra.result import catch
from makefun import create_function
import numpy as np

from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.store import Handle, holder
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import Struct
from rasm_records import Fault, Faults, Outcome, reached, words

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import PropertySubtypeStringItems

# --- [TYPES] ----------------------------------------------------------------------------

type EnumItem = tuple[str, str, str, str, int]
type IconedItem = tuple[str, str, str, int, int]
type EnumRow = tuple[str, str, str] | tuple[str, str, str, int] | EnumItem | IconedItem | None
type Changed = Callable[[Struct, bpy.types.Context], None]


class Callbacks[T: Struct = Struct](TypedDict, total=False):
    """Host callbacks of one property definition over owners of `T`, as `bpy.props` takes them, `items` also taking its static item rows."""

    items: ReadOnly[tuple[EnumRow, ...] | Callable[[T, bpy.types.Context | None], Iterable[EnumRow]]]
    get: ReadOnly[Callable[[T], object]]
    set: ReadOnly[Callable[[T, Never], None]]
    get_transform: ReadOnly[Callable[[T, Never, bool], object]]
    set_transform: ReadOnly[Callable[[T, Never, Never, bool], object]]
    poll: ReadOnly[Callable[[T, Never], bool]]
    update: ReadOnly[Callable[[T, bpy.types.Context], None]]
    search: ReadOnly[Callable[[T, bpy.types.Context, str], Iterable[str | tuple[str, str]]]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_NONE: Final[tuple[EnumItem]] = (("NONE", "None", "", "NONE", 0),)
_ARRAYED: Final = frozenset({
    "COLOR",
    "TRANSLATION",
    "DIRECTION",
    "VELOCITY",
    "ACCELERATION",
    "MATRIX",
    "EULER",
    "QUATERNION",
    "AXISANGLE",
    "XYZ",
    "XYZ_LENGTH",
    "COLOR_GAMMA",
    "COORDINATES",
    "LAYER",
    "LAYER_MEMBER",
})
_TEXTUAL: Final = frozenset({"FILE_PATH", "DIR_PATH", "FILE_NAME", "BYTE_STRING", "PASSWORD"})
_NUMBERS: Final[Mapping[tuple[type[bpy.types.Property], bool], Callable[..., bpy.props._PropertyDeferred]]] = MappingProxyType({  # type: ignore[explicit-any]
    (bpy.types.FloatProperty, False): bpy.props.FloatProperty,
    (bpy.types.FloatProperty, True): bpy.props.FloatVectorProperty,
    (bpy.types.IntProperty, False): bpy.props.IntProperty,
    (bpy.types.IntProperty, True): bpy.props.IntVectorProperty,
    (bpy.types.BoolProperty, False): bpy.props.BoolProperty,
    (bpy.types.BoolProperty, True): bpy.props.BoolVectorProperty,
})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DEFINITIONS]


@overload
def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str | None] | EnumItem]) -> tuple[EnumItem, ...]: ...
@overload
def choices(rows: Iterable[IconedItem]) -> tuple[IconedItem, ...]: ...
@overload
def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str | None] | EnumItem | IconedItem]) -> tuple[EnumItem | IconedItem, ...]: ...
def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str | None] | EnumItem | IconedItem]) -> tuple[EnumItem | IconedItem, ...]:
    """Returns `EnumProperty` items, each value fixed per identifier.

    Host items the owning property declares statically keep their value, config items and module rows take a value hashed from the identifier.
    Interface buttons compare enum values as float32, and hashed values stay within its exact integers for an expanded row to show its item.

    Args:
        rows: Host enum items, generated `(identifier, name, description)` rows, a `None` description the host's empty one, or full native items.

    Returns:
        Five-part items in row order.

    Raises:
        ValueError: External rows give one identifier different values or distinct identifiers one generated value.
    """
    mask = (1 << (np.finfo(np.float32).nmant + 1)) - 1

    def hashed(identifier: str) -> int:
        return zlib.crc32(identifier.encode()) & mask

    def item(row: bpy.types.EnumPropertyItem | tuple[str, str, str | None] | EnumItem | IconedItem) -> tuple[EnumItem | IconedItem, bool]:
        match row:
            case bpy.types.EnumPropertyItem() as host:
                fixed = any(host.identifier in prop.enum_items_static for prop in host.rna_ancestors() if isinstance(prop, bpy.types.EnumProperty))
                return (host.identifier, host.name, host.description, host.icon, host.value if fixed else hashed(host.identifier)), not fixed
            case (identifier, name, None):
                return (identifier, name, "", "NONE", hashed(identifier)), True
            case (identifier, name, str(description)):
                return (identifier, name, description, "NONE", hashed(identifier)), True
            case _:
                return row, False

    items: list[EnumItem | IconedItem] = []
    seen: dict[int | str, tuple[int | str, bool]] = {}
    aliases: dict[int | str, int | str] = {}
    for row in rows:
        current, generated = item(row)
        identifier, value = current[0], current[4]
        for key, target in ((value, identifier), (identifier, value)):
            prior, prior_generated = seen.setdefault(key, (target, generated))
            prefix = () if isinstance(key, int) else (key,)
            if (conflict := (*prefix, prior, target) if target != prior and (generated or prior_generated) else (*prefix, target, aliases[key]) if generated and key in aliases else None) is not None:
                raise ValueError(conflict)
            if target != prior:
                aliases[key] = target
            seen[key] = (prior, prior_generated or generated)
        items.append(current)
    return tuple(items)


def _offered[R: EnumItem | IconedItem](rows: tuple[R, ...], /) -> tuple[R | EnumItem, *tuple[R | EnumItem, ...]]:
    """Returns a dynamic enum's rows, the host's `NONE` item at value 0 for no rows, every enum read naming an item and its first row a total read."""
    match rows:
        case (first, *rest):
            return (first, *rest)
        case _:
            return _NONE


def _flagged[F: enum.Flag](kind: type[F], names: Iterable[str]) -> F:
    """Returns the flag an `ENUM_FLAG` property's identifier set holds, its members iterating in declaration order."""
    return reduce(kind.__or__, (kind[name] for name in names), kind(0))


def _titled(kind: type, /) -> str:
    """Returns a class name split into title-cased words, an acronym run kept whole (`CDL`, `HDRTone` as `HDR Tone`)."""
    return " ".join(word if word.isupper() else word.capitalize() for word in words(kind.__name__))


def case_choices(union: TypeAliasType) -> tuple[EnumItem, ...]:
    """Returns `choices` over a tagged union alias's cases: tag text as identifier, class name in `titled` words as name, docstring as description."""
    return choices((str(case.__struct_config__.tag), _titled(case), inspect.getdoc(case) or "") for case in get_args(union.__value__))


def enum_choices(kind: type[enum.Enum], /) -> tuple[EnumItem, ...]:
    """Returns `EnumProperty` items of an `Enum` type, member name as identifier and name, a `Flag` bit as value, others the value `choices` fixes."""
    rows = tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind)
    return tuple((*row, "NONE", member.value) for row, member in zip(rows, kind)) if issubclass(kind, enum.Flag) else choices(rows)


def _redeclared(prop: bpy.types.FloatProperty | bpy.types.IntProperty | bpy.types.BoolProperty | bpy.types.StringProperty | bpy.types.EnumProperty, /) -> bpy.props._PropertyDeferred:
    """Returns the `bpy.props` definition matching a host property's text, subtype, unit, bounds, step, precision, default, array shape, and items.

    Subtypes `bpy.props` refuses (array on scalars, number on arrays, string `TRANSLATION`) declare as `NONE`, the unit keeping its quantity.
    Dynamic arrays declare at their current shape.
    """
    match prop:
        case bpy.types.FloatProperty() | bpy.types.IntProperty() | bpy.types.BoolProperty():
            shape, arrayed = Descriptor.dimensions(prop), prop.subtype in _ARRAYED
            return _NUMBERS[type(prop), bool(shape)](
                name=prop.name,
                description=prop.description,
                translation_context=prop.translation_context,
                **(
                    {"size": shape, "default": np.resize(np.asarray(prop.default_array), shape).tolist(), "subtype": prop.subtype if arrayed else "NONE"}
                    if shape
                    else {"default": prop.default, "subtype": "NONE" if arrayed else prop.subtype}
                ),
                **({} if isinstance(prop, bpy.types.BoolProperty) else {"min": prop.hard_min, "max": prop.hard_max, "soft_min": prop.soft_min, "soft_max": prop.soft_max, "step": prop.step}),
                **({"unit": prop.unit, "precision": prop.precision} if isinstance(prop, bpy.types.FloatProperty) else {}),
            )
        case bpy.types.StringProperty():
            text: PropertySubtypeStringItems = prop.subtype if prop.subtype in _TEXTUAL else "NONE"  # ty: ignore[invalid-assignment]
            return bpy.props.StringProperty(name=prop.name, description=prop.description, translation_context=prop.translation_context, subtype=text, default=prop.default, maxlen=prop.length_max)
        case bpy.types.EnumProperty():
            definition = partial(bpy.props.EnumProperty, name=prop.name, description=prop.description, translation_context=prop.translation_context, items=choices(prop.enum_items))
            return definition(default=set(prop.default_flag), options={"ENUM_FLAG"}) if prop.is_enum_flag else definition(default=prop.default)


# --- [DECLARATIONS]


def annotated(form: type) -> dict[str, bpy.props._PropertyDeferred]:
    """Property definitions a form class and its bases declare, bases first, as Blender registers them."""
    return {name: definition for name, definition in get_type_hints(form).items() if isinstance(definition, bpy.props._PropertyDeferred)}


def _declared(definition: bpy.props._PropertyDeferred, callbacks: Mapping[str, object], /) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition with callbacks replacing their declared keywords.

    Args:
        definition: Property definition the callbacks extend.
        callbacks: Callbacks per `bpy.props` keyword.

    Returns:
        Definition retaining every other keyword.
    """
    fresh: bpy.props._PropertyDeferred = definition.function(**{**definition.keywords, **callbacks})  # ty: ignore[unsound-assignment]
    return fresh


def _hosted(definition: bpy.props._PropertyDeferred, /) -> bpy.props._PropertyDeferred:
    """Returns native callback functions, keeping dynamic enum strings for the property's registration lifetime."""
    callbacks = get_type_hints(Callbacks)

    def function(name: str, callback: object) -> object:
        if not callable(callback):
            return callback
        if name == "items":
            supplied: Callable[[Struct, bpy.types.Context | None], Iterable[EnumRow]] = callback  # ty: ignore[invalid-assignment]
            strings: dict[str, str] = {}

            def items(owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[object, ...] | None, ...]:
                return tuple(None if row is None else tuple(strings.setdefault(value, value) if isinstance(value, str) else value for value in row) for row in supplied(owner, context))

            return items
        arguments, _ = get_args(callbacks[name])
        signature = inspect.Signature(tuple(inspect.Parameter(f"arg{index}", inspect.Parameter.POSITIONAL_ONLY) for index in range(len(arguments))))
        return create_function(signature, callback)

    return _declared(definition, {name: function(name, value) for name, value in definition.keywords.items() if name in callbacks})


def _rebound[T: Struct](form: type[T], /, **callbacks: Callbacks | Callbacks[T]) -> type[T]:
    """Returns a same-named subclass of form declaring each named property with its callbacks, the class one registration holds of a shared form.

    Args:
        form: Form class declaring each named property.
        **callbacks: Callbacks per property name.

    Returns:
        Form subclass the host registers in place of form.
    """
    held = annotated(form)
    derived: type[T] = type(form.__name__, (form,), {"__annotations__": {name: _declared(held[name], bound) for name, bound in callbacks.items()}})  # ty: ignore[invalid-assignment, unsupported-dynamic-base]
    return derived


def _nested(form: type[bpy.types.PropertyGroup]) -> tuple[type[bpy.types.PropertyGroup], ...]:
    """Property group classes a form's pointer and collection properties reach, each once."""

    def targets(group: type[bpy.types.PropertyGroup]) -> tuple[type[bpy.types.PropertyGroup], ...]:
        return tuple(
            target
            for definition in annotated(group).values()
            if definition.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
            for target in (definition.keywords["type"],)
            if issubclass(target, bpy.types.PropertyGroup)
        )

    return tuple(group for group in reached((form,), targets) if group is not form)  # type: ignore[type-var]


def _updated(definition: bpy.props._PropertyDeferred, change: Changed) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition of a value property running the change callback after its declared update, any other definition as declared."""
    valued = definition.function is not bpy.props.CollectionProperty and not issubclass(definition.keywords.get("type", bpy.types.ID), bpy.types.PropertyGroup)
    prior: Changed | None = definition.keywords.get("update")  # ty: ignore[unsound-assignment]

    def changed(owner: Struct, context: bpy.types.Context) -> None:
        if prior is not None:
            prior(owner, context)
        change(owner, context)

    return _declared(definition, Callbacks(update=changed)) if valued else definition


@contextmanager
def _declaring(form: type[bpy.types.PropertyGroup], callbacks: Mapping[str, Callbacks[Never]], change: Option[Changed]) -> Generator[tuple[type[bpy.types.PropertyGroup], ...]]:
    """Declares the properties of a form and its nested groups while the scope registers them.

    Form properties take their named callbacks, then every value property runs `change` after its declared update.
    Concrete group classes hold fresh definitions for the scope and their original annotation mappings again at exit.

    Args:
        form: Form class the scope registers after the groups it nests.
        callbacks: Callbacks per property name of the form.
        change: Update callback every value property runs.

    Yields:
        Groups under the fresh definitions.
    """
    groups = (*_nested(form), form)
    with ExitStack() as stack:
        for group in groups:
            if (prior := vars(group).get("__annotations__")) is None:
                stack.callback(delattr, group, "__annotations__")
            else:
                stack.callback(setattr, group, "__annotations__", prior)
            group.__annotations__ = {
                **(prior if prior is not None else {}),
                **{
                    name: change.map(partial(_updated, bound)).default_value(bound)
                    for name, definition in annotated(group).items()
                    for bound in (_declared(definition, callbacks[name]) if group is form and name in callbacks else definition,)
                },
            }
        yield groups


@contextmanager
def _hosting(form: type, /) -> Generator[None]:
    """Supplies native callback definitions while registration transfers callback ownership to the host."""
    prior = vars(form).get("__annotations__")
    with ExitStack() as stack:
        if prior is None:
            stack.callback(delattr, form, "__annotations__")
        else:
            stack.callback(setattr, form, "__annotations__", prior)
        form.__annotations__ = {**(prior if prior is not None else {}), **{name: _hosted(definition) for name, definition in annotated(form).items()}}
        yield


@contextmanager
def _pointed(owner: type[Struct], name: str, definition: bpy.props._PropertyDeferred) -> Generator[Outcome[None]]:
    """Registers property under a free name for scope duration, the host refusing a name past its byte limit."""
    request = f"{owner.__name__}.{name}"
    rna: bpy.types.Struct = owner.bl_rna  # ty: ignore[invalid-assignment]
    with ExitStack() as stack:
        yield (
            Result[None, Faults](ok=None)
            .filter_with(lambda _: not (name in rna.properties or name in rna.functions or hasattr(owner, name)), lambda _: Fault.of(Refusal.TAKEN, request, member=owner.__name__))
            .bind(lambda _: catch(setattr, exception=ValueError)(owner, name, _hosted(definition)).map_error(partial(Fault.of, Refusal.REFUSED, request)))
            .map(lambda _: stack.callback(delattr, owner, name))
            .map(lambda _: None)
        )


@contextmanager
def _reconciling[T: bpy.types.ID](kind: type[T], write: Callable[[Handle[T], Identity], Outcome[Option[str]]], identity: Identity) -> Generator[Changed]:
    """Yields the update callback deferring one reconcile per changed holder of kind, an attachment binding kind and write through `partial`.

    Args:
        kind: Holder class write resolves.
        write: Deferred write reconciling holder.
        identity: Extension whose reports defer the write.

    Yields:
        Update callback while the deferral is open.
    """
    with identity.reports.deferred(write) as deferred:

        def changed(owner: Struct, _context: bpy.types.Context) -> None:
            for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                if isinstance(found, kind):
                    deferred(Handle.of(found), identity)

        yield changed


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Callbacks", "Changed", "EnumItem", "EnumRow", "IconedItem", "annotated", "case_choices", "choices", "enum_choices"]
