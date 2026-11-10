"""`bpy.props` definitions and `EnumProperty` items copied from host properties, enum types, and module rows, and form registration with callbacks."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
import enum
from functools import partial, reduce
import inspect
from types import MappingProxyType
from typing import Final, get_args, get_type_hints, Never, overload, ReadOnly, TypeAliasType, TypedDict
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
from rasm_bpy.data._rna.paths import rna_definition, Struct
from rasm_records import Fault, Faults, Outcome, reached, words

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
_NUMBERS: Final[Mapping[tuple[type[bpy.types.Property], bool], bpy.props._PropertyDeferred]] = MappingProxyType({
    (bpy.types.FloatProperty, False): bpy.props.FloatProperty(),
    (bpy.types.FloatProperty, True): bpy.props.FloatVectorProperty(),
    (bpy.types.IntProperty, False): bpy.props.IntProperty(),
    (bpy.types.IntProperty, True): bpy.props.IntVectorProperty(),
    (bpy.types.BoolProperty, False): bpy.props.BoolProperty(),
    (bpy.types.BoolProperty, True): bpy.props.BoolVectorProperty(),
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


def offered[R: EnumItem | IconedItem](rows: tuple[R, ...], /) -> tuple[R | EnumItem, *tuple[R | EnumItem, ...]]:
    """Returns a dynamic enum's rows, the host's `NONE` item at value 0 for no rows, every enum read naming an item and its first row a total read."""
    match rows:
        case (first, *rest):
            return (first, *rest)
        case _:
            return _NONE


def flagged[F: enum.Flag](kind: type[F], names: Iterable[str]) -> F:
    """Returns the flag an `ENUM_FLAG` property's identifier set holds, its members iterating in declaration order."""
    return reduce(kind.__or__, (kind[name] for name in names), kind(0))


def titled(kind: type, /) -> str:
    """Returns a class name split into title-cased words, an acronym run kept whole (`CDL`, `HDRTone` as `HDR Tone`)."""
    return " ".join(word if word.isupper() else word.capitalize() for word in words(kind.__name__))


def case_choices(union: TypeAliasType) -> tuple[EnumItem, ...]:
    """Returns `choices` over a tagged union alias's cases: tag text as identifier, class name in `titled` words as name, docstring as description."""
    return choices((str(case.__struct_config__.tag), titled(case), inspect.getdoc(case) or "") for case in get_args(union.__value__))


def enum_choices(kind: type[enum.Enum], /) -> tuple[EnumItem, ...]:
    """Returns `EnumProperty` items of an `Enum` type, member name as identifier and name, a `Flag` bit as value, others the value `choices` fixes."""
    rows = tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind)
    return tuple((*row, "NONE", member.value) for row, member in zip(rows, kind)) if issubclass(kind, enum.Flag) else choices(rows)


def redeclared(prop: bpy.types.FloatProperty | bpy.types.IntProperty | bpy.types.BoolProperty | bpy.types.StringProperty | bpy.types.EnumProperty, /) -> bpy.props._PropertyDeferred:
    """Returns the `bpy.props` definition matching a host property's text, subtype, unit, bounds, step, precision, default, array shape, and items.

    Subtypes `bpy.props` refuses (array on scalars, number on arrays, string `TRANSLATION`) declare as `NONE`, the unit keeping its quantity.
    Dynamic arrays declare at their current shape.
    """
    match prop:
        case bpy.types.FloatProperty() | bpy.types.IntProperty() | bpy.types.BoolProperty():
            shape, arrayed = Descriptor.dimensions(prop), prop.subtype in _ARRAYED
            return replaced_keywords(
                _NUMBERS[type(prop), bool(shape)],
                {
                    "name": prop.name,
                    "description": prop.description,
                    "translation_context": prop.translation_context,
                    **(
                        {"size": shape, "default": np.resize(np.asarray(prop.default_array), shape).tolist(), "subtype": prop.subtype if arrayed else "NONE"}
                        if shape
                        else {"default": prop.default, "subtype": "NONE" if arrayed else prop.subtype}
                    ),
                    **({} if isinstance(prop, bpy.types.BoolProperty) else {"min": prop.hard_min, "max": prop.hard_max, "soft_min": prop.soft_min, "soft_max": prop.soft_max, "step": prop.step}),
                    **({"unit": prop.unit, "precision": prop.precision} if isinstance(prop, bpy.types.FloatProperty) else {}),
                },
            )
        case bpy.types.StringProperty():
            text = partial(bpy.props.StringProperty, name=prop.name, description=prop.description, translation_context=prop.translation_context, default=prop.default, maxlen=prop.length_max)
            match prop.subtype:
                case "FILE_PATH" | "DIR_PATH" | "FILE_NAME" | "BYTE_STRING" | "PASSWORD" as subtype:
                    return text(subtype=subtype)
                case _:
                    return text()
        case bpy.types.EnumProperty():
            definition = partial(bpy.props.EnumProperty, name=prop.name, description=prop.description, translation_context=prop.translation_context, items=choices(prop.enum_items))
            return definition(default=set(prop.default_flag), options={"ENUM_FLAG"}) if prop.is_enum_flag else definition(default=prop.default)


# --- [DECLARATIONS]


def annotated(form: type) -> dict[str, bpy.props._PropertyDeferred]:
    """Property definitions a form class and its bases declare, bases first, as Blender registers them, read from an `Annotated[<value type>, <definition>]` hint or a bare definition."""
    return {name: definition for name, hint in get_type_hints(form, include_extras=True).items() for definition in (hint, *get_args(hint)) if isinstance(definition, bpy.props._PropertyDeferred)}


def replaced_keywords(definition: bpy.props._PropertyDeferred, keywords: Mapping[str, object], /) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition with keywords replacing their declared values.

    Args:
        definition: Property definition the keywords extend.
        keywords: Values per `bpy.props` keyword, callbacks among them.

    Returns:
        Definition retaining every other keyword.

    Raises:
        TypeError: The definition's function returned no definition.
    """
    match definition.function(**{**definition.keywords, **keywords}):
        case bpy.props._PropertyDeferred() as fresh:
            return fresh
        case other:
            raise TypeError(other)


def _hosted(definition: bpy.props._PropertyDeferred, /) -> bpy.props._PropertyDeferred:
    """Returns native callback functions, keeping dynamic enum strings for the property's registration lifetime."""
    callbacks, strings = get_type_hints(Callbacks), dict[str, str]()

    def items(owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[object, ...] | None, ...]:
        return tuple(None if row is None else tuple(strings.setdefault(value, value) if isinstance(value, str) else value for value in row) for row in definition.keywords["items"](owner, context))

    return replaced_keywords(
        definition,
        {
            name: items
            if name == "items"
            else create_function(inspect.Signature(tuple(inspect.Parameter(f"arg{index}", inspect.Parameter.POSITIONAL_ONLY) for index in range(len(get_args(callbacks[name])[0])))), callback)
            for name, callback in definition.keywords.items()
            if name in callbacks and callable(callback)
        },
    )


def rebound[T: Struct](form: type[T], /, **callbacks: Callbacks | Callbacks[T]) -> type[T]:
    """Returns a same-named subclass of form declaring each named property with its callbacks, the class one registration holds of a shared form.

    Args:
        form: Form class declaring each named property.
        **callbacks: Callbacks per property name.

    Returns:
        Form subclass the host registers in place of form.
    """
    held = annotated(form)
    derived: type[T] = type(form.__name__, (form,), {"__annotations__": {name: replaced_keywords(held[name], bound) for name, bound in callbacks.items()}})  # ty: ignore[invalid-assignment, unsupported-dynamic-base]
    return derived


def nested(form: type[bpy.types.PropertyGroup]) -> tuple[type[bpy.types.PropertyGroup], ...]:
    """Property group classes a form's pointer and collection properties reach, each once."""

    def targets(group: type) -> tuple[type, ...]:
        return tuple(
            target
            for definition in annotated(group).values()
            if definition.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
            for target in (definition.keywords["type"],)
            if isinstance(target, type)
        )

    return tuple(group for group in reached(targets(form), targets) if issubclass(group, bpy.types.PropertyGroup) and group is not form)


def _updated(definition: bpy.props._PropertyDeferred, change: Changed) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition of a value property running the change callback after its declared update, any other definition as declared."""
    valued = definition.function is not bpy.props.CollectionProperty and not issubclass(definition.keywords.get("type", bpy.types.ID), bpy.types.PropertyGroup)
    prior = definition.keywords.get("update")

    def changed(owner: Struct, context: bpy.types.Context) -> None:
        if prior is not None:
            prior(owner, context)
        change(owner, context)

    return replaced_keywords(definition, Callbacks(update=changed)) if valued else definition


@contextmanager
def declaring(form: type[bpy.types.PropertyGroup], callbacks: Mapping[str, Callbacks[Never]], change: Option[Changed]) -> Generator[tuple[type[bpy.types.PropertyGroup], ...]]:
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
    groups = (*nested(form), form)
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
                    for bound in (replaced_keywords(definition, callbacks[name]) if group is form and name in callbacks else definition,)
                },
            }
        yield groups


@contextmanager
def hosting(form: type, /) -> Generator[None]:
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
def pointed(owner: type[Struct], name: str, definition: bpy.props._PropertyDeferred) -> Generator[Outcome[None]]:
    """Registers property under a free name for scope duration, the host refusing a name past its byte limit."""
    request = f"{owner.__name__}.{name}"
    rna = rna_definition(owner)
    with ExitStack() as stack:
        yield (
            Result[None, Faults](ok=None)
            .filter_with(lambda _: not (name in rna.properties or name in rna.functions or hasattr(owner, name)), lambda _: Fault.of(Refusal.TAKEN, request, member=owner.__name__))
            .bind(lambda _: catch(setattr, exception=ValueError)(owner, name, _hosted(definition)).map_error(partial(Fault.of, Refusal.REFUSED, request)))
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
        Update callback while the deferral is open.
    """
    with identity.reports.deferred(write) as deferred:

        def changed(owner: Struct, _context: bpy.types.Context) -> None:
            for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                if isinstance(found, kind):
                    deferred(Handle.of(found), identity)

        yield changed


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Callbacks",
    "Changed",
    "EnumItem",
    "EnumRow",
    "IconedItem",
    "annotated",
    "case_choices",
    "choices",
    "declaring",
    "enum_choices",
    "flagged",
    "hosting",
    "nested",
    "offered",
    "pointed",
    "rebound",
    "reconciling",
    "redeclared",
    "replaced_keywords",
    "titled",
]
