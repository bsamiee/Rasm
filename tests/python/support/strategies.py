"""Hypothesis strategy construction for msgspec and pydantic-core schemas."""

import builtins
from collections.abc import Callable, Collection, Mapping, Set as AbstractSet
import dataclasses
import datetime as dt
from decimal import Decimal
from fractions import Fraction
import functools
from math import ceil, floor
import ntpath
from pathlib import Path
import string
from types import get_original_bases
import typing
from typing import get_args, get_origin, get_type_hints, TypeAliasType, TypedDict, TypeForm, TypeVar

from hypothesis import strategies as st
import msgspec
import msgspec.inspect
import msgspec.msgpack
import msgspec.structs
import pydantic
from pydantic_core import core_schema, CoreSchema

# --- [TYPES] ----------------------------------------------------------------------------


class _Size(TypedDict):
    min_size: int
    max_size: int


# --- [CONSTANTS] ------------------------------------------------------------------------

_NUMERIC_CEILING = 1_000_000
_COLLECTION_CAP = 3

_JSON: st.SearchStrategy[object] = st.recursive(
    st.none() | st.booleans() | st.integers(min_value=-1_000, max_value=1_000) | st.text(max_size=16),
    lambda inner: st.lists(inner, max_size=3) | st.dictionaries(st.text(min_size=1, max_size=8), inner, max_size=3),
    max_leaves=8,
)
_PATH_PART = st.text(alphabet=string.ascii_lowercase + string.digits, min_size=1, max_size=8).filter(lambda part: not ntpath.isreserved(part))
_PATH = st.lists(_PATH_PART, min_size=1, max_size=3).map(lambda parts: Path(*parts))

# --- [CONSTRAINTS] ----------------------------------------------------------------------


def _size(minimum: object, maximum: object, cap: int) -> _Size:
    lower = minimum if isinstance(minimum, int) else 0
    return {"min_size": lower, "max_size": max(lower, min(maximum, cap) if isinstance(maximum, int) else cap)}


def _integer_bound(inclusive: object, exclusive: object, offset: int, absent: int | None) -> int | None:
    match inclusive, exclusive:
        case int() as bound, _:
            return bound
        case _, int() as bound:
            return bound + offset
        case _:
            return absent


def _numeric_bound(inclusive: object, exclusive: object, absent: float | None) -> tuple[float | Decimal | None, bool]:
    match inclusive, exclusive:
        case Decimal() as bound, _:
            return bound, False
        case int() | float() as bound, _:
            return float(bound), False
        case _, Decimal() as bound:
            return bound, True
        case _, int() | float() as bound:
            return float(bound), True
        case _:
            return absent, False


def _timezones(*, tz: bool | None) -> st.SearchStrategy[dt.tzinfo | None]:
    match tz:
        case True:
            return st.timezones()
        case False:
            return st.none()
        case None:
            return st.none() | st.timezones()


def _multiples[N](lower: object, upper: object, step: object, convert: Callable[[Decimal], N], *, exclude_lower: bool = False, exclude_upper: bool = False) -> st.SearchStrategy[N]:
    """Return a strategy over the in-range multiples of ``step`` that draws the integer multiplier."""
    decimal_step = Decimal(str(step))
    lower_quotient = Fraction(str(-_NUMERIC_CEILING if lower is None else lower)) / Fraction(decimal_step)
    upper_quotient = Fraction(str(_NUMERIC_CEILING if upper is None else upper)) / Fraction(decimal_step)
    lower_ceiling = ceil(lower_quotient)
    upper_floor = floor(upper_quotient)
    minimum_multiplier = lower_ceiling + (1 if exclude_lower and lower_quotient == lower_ceiling else 0)
    maximum_multiplier = upper_floor - (1 if exclude_upper and upper_quotient == upper_floor else 0)
    return (
        st.integers(min_value=minimum_multiplier, max_value=maximum_multiplier).map(lambda multiplier: convert(Decimal(multiplier) * decimal_step))
        if minimum_multiplier <= maximum_multiplier
        else st.nothing()
    )


def _text(minimum: object, maximum: object, pattern: object) -> st.SearchStrategy[str]:
    lower = minimum if isinstance(minimum, int) else 1
    cap = 64
    upper = min(maximum, cap) if isinstance(maximum, int) else cap
    if lower > upper:
        return st.nothing()
    return st.from_regex(pattern, fullmatch=True).filter(lambda s: lower <= len(s) <= upper) if isinstance(pattern, str) else st.text(min_size=lower, max_size=upper)


# --- [MSGSPEC_SCHEMAS] ------------------------------------------------------------------


def _msgspec_strategy(schema: msgspec.inspect.Type) -> st.SearchStrategy[object]:
    """Return a bounded strategy for a ``msgspec.inspect`` schema."""
    match schema:
        case msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt, multiple_of=step):
            lower, upper = _integer_bound(ge, gt, 1, -_NUMERIC_CEILING), _integer_bound(le, lt, -1, _NUMERIC_CEILING)
            return _multiples(lower, upper, step, int) if step is not None else st.integers(min_value=lower, max_value=upper)
        case msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt, multiple_of=float_step):
            (float_lower, open_lower), (float_upper, open_upper) = _numeric_bound(ge, gt, -_NUMERIC_CEILING), _numeric_bound(le, lt, _NUMERIC_CEILING)
            return (
                _multiples(float_lower, float_upper, float_step, float, exclude_lower=open_lower, exclude_upper=open_upper)
                if float_step is not None
                else st.floats(min_value=float_lower, max_value=float_upper, exclude_min=open_lower, exclude_max=open_upper, allow_nan=False, allow_infinity=False)
            )
        case msgspec.inspect.StrType(min_length=minimum, max_length=maximum, pattern=pattern):
            return _text(minimum, maximum, pattern)
        case msgspec.inspect.BoolType():
            return st.booleans()
        case msgspec.inspect.BytesType(min_length=minimum, max_length=maximum):
            return st.binary(**_size(minimum, maximum, 256))
        case msgspec.inspect.ByteArrayType(min_length=minimum, max_length=maximum):
            return st.binary(**_size(minimum, maximum, 256)).map(bytearray)
        case msgspec.inspect.MemoryViewType(min_length=minimum, max_length=maximum):
            return st.binary(**_size(minimum, maximum, 256)).map(memoryview)
        case msgspec.inspect.EnumType(cls=cls):
            return st.sampled_from(list(cls))
        case msgspec.inspect.LiteralType(values=values):
            return st.sampled_from(list(values))
        case msgspec.inspect.DateTimeType(tz=tz):
            return st.datetimes(timezones=_timezones(tz=tz))
        case msgspec.inspect.TimeType(tz=tz):
            return st.times(timezones=_timezones(tz=tz))
        case msgspec.inspect.DateType():
            return st.dates()
        case msgspec.inspect.TimeDeltaType():
            return st.timedeltas()
        case msgspec.inspect.DecimalType():
            return st.decimals(allow_nan=False, allow_infinity=False)
        case msgspec.inspect.UUIDType():
            return st.uuids()
        case msgspec.inspect.NoneType():
            return st.none()
        case msgspec.inspect.UnionType(types=types):
            return st.one_of(*(_msgspec_strategy(member) for member in types))
        case msgspec.inspect.VarTupleType(item_type=item, min_length=minimum, max_length=maximum):
            return st.lists(_msgspec_strategy(item), **_size(minimum, maximum, _COLLECTION_CAP)).map(tuple)
        case msgspec.inspect.TupleType(item_types=items):
            return st.tuples(*(_msgspec_strategy(item) for item in items))
        case msgspec.inspect.SetType(item_type=item, min_length=minimum, max_length=maximum):
            return st.sets(_msgspec_strategy(item), **_size(minimum, maximum, _COLLECTION_CAP))
        case msgspec.inspect.FrozenSetType(item_type=item, min_length=minimum, max_length=maximum):
            return st.frozensets(_msgspec_strategy(item), **_size(minimum, maximum, _COLLECTION_CAP))
        case msgspec.inspect.CollectionType(item_type=item, min_length=minimum, max_length=maximum):
            return st.lists(_msgspec_strategy(item), **_size(minimum, maximum, _COLLECTION_CAP))
        case msgspec.inspect.DictType(key_type=key, value_type=value, min_length=minimum, max_length=maximum):
            return st.dictionaries(_msgspec_strategy(key), _msgspec_strategy(value), **_size(minimum, maximum, _COLLECTION_CAP))
        case msgspec.inspect.StructType(cls=cls) | msgspec.inspect.DataclassType(cls=cls) | msgspec.inspect.TypedDictType(cls=cls) | msgspec.inspect.NamedTupleType(cls=cls):
            return strategy_for(cls)
        case msgspec.inspect.RawType():
            return _JSON.map(lambda value: msgspec.Raw(msgspec.json.encode(value)))
        case msgspec.inspect.AnyType():
            return _JSON
        case msgspec.inspect.CustomType(cls=cls):
            return owned if (owned := _owned(cls)) is not None else st.from_type(cls)
        case msgspec.inspect.ExtType():
            return st.tuples(st.integers(min_value=0, max_value=127), st.binary(max_size=16)).map(lambda pair: msgspec.msgpack.Ext(*pair))
        case msgspec.inspect.Metadata(type=inner):
            return _msgspec_strategy(inner)
        case _:  # pragma: no cover
            raise AssertionError(f"unsupported msgspec schema {type(schema).__name__}")


# --- [PYDANTIC_CORE_SCHEMAS] ------------------------------------------------------------


def _pydantic_strategy(schema: CoreSchema, definitions: dict[str, CoreSchema]) -> st.SearchStrategy[object]:
    """Return a constraint-aware strategy for a ``pydantic-core`` schema and its definitions."""
    match schema["type"]:
        case "int":
            lower, upper = _integer_bound(schema.get("ge"), schema.get("gt"), 1, None), _integer_bound(schema.get("le"), schema.get("lt"), -1, None)
            multiple_of = schema.get("multiple_of")
            return _multiples(lower, upper, multiple_of, int) if multiple_of is not None else st.integers(min_value=lower, max_value=upper)
        case "float":
            float_lower, exclude_lower = _numeric_bound(schema.get("ge"), schema.get("gt"), None)
            float_upper, exclude_upper = _numeric_bound(schema.get("le"), schema.get("lt"), None)
            multiple_of = schema.get("multiple_of")
            return (
                _multiples(float_lower, float_upper, multiple_of, float, exclude_lower=exclude_lower, exclude_upper=exclude_upper)
                if multiple_of is not None
                else st.floats(min_value=float_lower, max_value=float_upper, exclude_min=exclude_lower, exclude_max=exclude_upper, allow_nan=False, allow_infinity=False)
            )
        case "decimal":
            decimal_lower, exclude_lower = _numeric_bound(schema.get("ge"), schema.get("gt"), None)
            decimal_upper, exclude_upper = _numeric_bound(schema.get("le"), schema.get("lt"), None)
            places: int | None
            digit_lower: Decimal | None
            digit_upper: Decimal | None
            match schema.get("decimal_places"), schema.get("max_digits"):
                case int() as places, int() as digits:
                    digit_upper = Decimal(10) ** (digits - places) - Decimal(10) ** (-places)
                    digit_lower = -digit_upper
                case int() as places, _:
                    digit_lower = digit_upper = None
                case _, int() as digits:
                    places = 0
                    digit_upper = Decimal(10) ** digits - 1
                    digit_lower = -digit_upper
                case _:
                    places = digit_lower = digit_upper = None
            effective_lower = decimal_lower if decimal_lower is not None else digit_lower
            effective_upper = decimal_upper if decimal_upper is not None else digit_upper
            if (multiple_of := schema.get("multiple_of")) is not None:
                return _multiples(effective_lower, effective_upper, multiple_of, lambda value: value, exclude_lower=exclude_lower, exclude_upper=exclude_upper)
            values = st.decimals(min_value=effective_lower, max_value=effective_upper, places=places, allow_nan=False, allow_infinity=False)
            return (
                values.filter(lambda value: (not exclude_lower or effective_lower is None or value > effective_lower) and (not exclude_upper or effective_upper is None or value < effective_upper))
                if (exclude_lower or exclude_upper)
                else values
            )
        case "str":
            return _text(schema.get("min_length"), schema.get("max_length"), schema.get("pattern"))
        case "bytes":
            return st.binary(**_size(schema.get("min_length"), schema.get("max_length"), 256))
        case "list":
            return st.lists(_pydantic_strategy(schema.get("items_schema", core_schema.any_schema()), definitions), **_size(schema.get("min_length"), schema.get("max_length"), _COLLECTION_CAP))
        case "bool":
            return st.booleans()
        case "none":
            return st.none()
        case "any":
            return _JSON
        case "datetime":
            return st.datetimes(timezones=st.just(dt.UTC))
        case "date":
            return st.dates()
        case "time":
            return st.times()
        case "timedelta":
            return st.timedeltas()
        case "uuid":
            return st.uuids()
        case "enum":
            return st.sampled_from(schema["members"])
        case "literal":
            return st.sampled_from(schema["expected"])
        case "nullable":
            return st.none() | _pydantic_strategy(schema["schema"], definitions)
        case "default" | "function-before" | "function-after" | "function-wrap":
            return _pydantic_strategy(schema["schema"], definitions)
        case "set" | "frozenset" as kind:
            elements = st.lists(_pydantic_strategy(schema.get("items_schema", core_schema.any_schema()), definitions), max_size=_COLLECTION_CAP, unique=True)
            return elements.map(frozenset if kind == "frozenset" else set)
        case "tuple":
            return st.tuples(*(_pydantic_strategy(item, definitions) for item in schema["items_schema"]))
        case "dict":
            keys = _pydantic_strategy(schema.get("keys_schema", core_schema.any_schema()), definitions)
            return st.dictionaries(keys, _pydantic_strategy(schema.get("values_schema", core_schema.any_schema()), definitions), max_size=_COLLECTION_CAP)
        case "union":
            choices = schema["choices"]
            return st.one_of(
                *(_pydantic_strategy(choice, definitions) for choice in choices if not isinstance(choice, tuple)),
                *(_pydantic_strategy(choice, definitions) for choice, _ in (tagged for tagged in choices if isinstance(tagged, tuple))),
            )
        case "tagged-union":
            return st.one_of(*(_pydantic_strategy(choice, definitions) for choice in schema["choices"].values()))
        case "model" | "dataclass":
            cls = schema["cls"]
            return _pydantic_strategy(schema["schema"], definitions).map(lambda fields: cls(**fields) if isinstance(fields, Mapping) else cls())
        case "model-fields" | "typed-dict" | "dataclass-args":
            members = (
                {field["name"]: field["schema"] for field in schema["fields"] if field.get("init", True)}
                if schema["type"] == "dataclass-args"
                else {name: field["schema"] for name, field in schema["fields"].items()}
            )
            return st.fixed_dictionaries(
                {name: _pydantic_strategy(member, definitions) for name, member in members.items() if member["type"] != "default"},
                optional={name: _pydantic_strategy(member, definitions) for name, member in members.items() if member["type"] == "default"},
            )
        case "definitions":
            return _pydantic_strategy(schema["schema"], definitions | {reference: definition for definition in schema["definitions"] if isinstance(reference := definition.get("ref"), str)})
        case "definition-ref":
            ref = schema["schema_ref"]
            return st.deferred(lambda: _pydantic_strategy(definitions[ref], definitions))
        case _:
            return st.none()


# --- [TYPE_FORMS] -----------------------------------------------------------------------


def _tagged_cases(subject: type) -> dict[str, TypeForm[object]] | None:
    """Return the case fields of an ``expression`` ``@tagged_union`` class mapped to type hints, or ``None`` for any other subject."""
    if not (dataclasses.is_dataclass(subject) and isinstance(subject, type)):
        return None
    fields = dataclasses.fields(subject)
    if not (len(fields) >= 2 and fields[0].name == "tag" and all(not field.init and field.kw_only for field in fields)):
        return None
    hints: dict[str, TypeForm[object]] = get_type_hints(subject, include_extras=True)
    return {f.name: hints[f.name] for f in fields[1:]}


def _binding(subject: type, form: TypeForm[object]) -> dict[object, TypeForm[object]]:
    """Return the type parameters of ``subject`` in ``typing``'s order, its ``Generic`` base's arguments else every base's type variables by first appearance, bound to the arguments of ``form``."""
    bases = get_original_bases(subject)
    parameters = next((get_args(base) for base in bases if get_origin(base) is typing.Generic), tuple(dict.fromkeys(arg for base in bases for arg in get_args(base) if isinstance(arg, TypeVar))))
    return dict(zip(parameters, get_args(form), strict=False))


def _substituted(hint: TypeForm[object], binding: Mapping[object, TypeForm[object]]) -> TypeForm[object]:
    """Return ``hint`` with each type variable ``binding`` holds replaced by its argument, a parameterized form rebuilt through its origin and returned as a form with nothing left to bind."""
    match hint:
        case TypeVar():
            return binding.get(hint, hint)
        case _ if binding and (origin := get_origin(hint)) is not None:
            return _substituted(origin[tuple([_substituted(item, binding) for item in arg] if isinstance(arg, list) else _substituted(arg, binding) for arg in get_args(hint))], {})
        case _:
            return hint


def _record[F: (msgspec.structs.FieldInfo, msgspec.inspect.Field)](
    subject: type, fields: tuple[F, ...], draw: Callable[[F, Mapping[object, TypeForm[object]]], st.SearchStrategy[object]]
) -> Callable[[TypeForm[object]], st.SearchStrategy[object]]:
    def _drawn(form: TypeForm[object]) -> st.SearchStrategy[object]:
        binding = _binding(subject, form)
        return st.fixed_dictionaries(
            {field.name: draw(field, binding) for field in fields if field.required}, optional={field.name: draw(field, binding) for field in fields if not field.required}
        ).map(lambda arguments: subject(**arguments))

    return _drawn


@functools.cache
def _builder(subject: type) -> Callable[[TypeForm[object]], st.SearchStrategy[object]] | None:
    """Return the strategy builder over each parameterization of a tagged union, pydantic model, or msgspec-described class, or ``None`` for any other subject."""
    if (cases := _tagged_cases(subject)) is not None:

        def _case(name: str, hint: TypeForm[object]) -> st.SearchStrategy[object]:
            return strategy_for(hint).map(lambda value: subject(**{name: value}))

        def _cased(form: TypeForm[object]) -> st.SearchStrategy[object]:
            binding = _binding(subject, form)
            return st.one_of(*(_case(name, _substituted(hint, binding)) for name, hint in cases.items()))

        return _cased
    if issubclass(subject, pydantic.BaseModel):
        model = subject
        return lambda _: _pydantic_strategy(model.__pydantic_core_schema__, {})
    if issubclass(subject, msgspec.Struct):
        return _record(subject, msgspec.structs.fields(subject), lambda field, binding: strategy_for(_substituted(field.type, binding)))
    if isinstance(info := msgspec.inspect.type_info(subject), msgspec.inspect.DataclassType | msgspec.inspect.NamedTupleType | msgspec.inspect.TypedDictType):
        return _record(subject, info.fields, lambda field, _: _msgspec_strategy(field.type))
    return None


def _owned(subject: TypeForm[object]) -> st.SearchStrategy[object] | None:
    """Return the deferred strategy the class owning ``subject``, its origin or the subject itself, builds for it, or ``None`` where that class builds none."""
    match get_origin(subject) or subject:
        case type() as owner if (build := _builder(owner)) is not None:
            return st.deferred(lambda: build(subject))
        case _:
            return None


def _decomposed(subject: TypeForm[object], size: _Size) -> st.SearchStrategy[object]:
    """Return the strategy of a form msgspec rejects or reduces to its bare class, its origin's own value sized by ``size`` over the strategies of its arguments."""
    match get_origin(subject), get_args(subject):
        case _ if (owned := _owned(subject)) is not None:
            return owned
        case typing.Union, members:
            return st.one_of(*map(strategy_for, members))
        case typing.Literal, values:
            return st.sampled_from(values)
        case typing.Annotated, (inner, *metadata):
            metas = [meta for meta in metadata if isinstance(meta, msgspec.Meta)]
            minimum, maximum = next((meta.min_length for meta in metas if meta.min_length is not None), None), next((meta.max_length for meta in metas if meta.max_length is not None), None)
            return _decomposed(inner, _size(minimum, maximum, _COLLECTION_CAP))
        case builtins.tuple, (item, builtins.Ellipsis):
            return st.lists(strategy_for(item), **size).map(tuple)
        case builtins.tuple, items:
            return st.tuples(*map(strategy_for, items))
        case builtins.frozenset, (item,):
            return st.frozensets(strategy_for(item), **size)
        case type() as kind, (key, value) if issubclass(kind, Mapping):
            return st.dictionaries(strategy_for(key), strategy_for(value), **size)
        case type() as kind, (item,) if issubclass(kind, AbstractSet):
            return st.sets(strategy_for(item), **size)
        case type() as kind, (item,) if issubclass(kind, Collection):
            return st.lists(strategy_for(item), **size)
        case kind, (_, returns) if kind is Callable:
            return st.functions(like=lambda *_: None, returns=strategy_for(returns))
        case builtins.type, (item,):
            return st.just(item)
        case None, _ if isinstance(subject, type):
            return strategy_for(subject)
        case _:  # pragma: no cover
            raise AssertionError(f"unsupported type form {subject!r}")


def strategy_for(subject: TypeForm[object]) -> st.SearchStrategy[object]:
    """Return a bounded strategy for a type, PEP 695 alias, union, ``Literal``, ``Annotated``, or type expression."""
    if isinstance(subject, TypeAliasType):
        return strategy_for(subject.__value__)
    if (owned := _owned(subject)) is not None:
        return owned
    unsized = _size(None, None, _COLLECTION_CAP)
    try:
        node = msgspec.inspect.type_info(subject)
    except TypeError:
        return _decomposed(subject, unsized)
    match node:
        case msgspec.inspect.CustomType() if get_args(subject):
            return _decomposed(subject, unsized)
        case _:
            return _msgspec_strategy(node)


# --- [COMPOSITION] ----------------------------------------------------------------------

st.register_type_strategy(Path, lambda _: _PATH)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["strategy_for"]
