"""Hypothesis strategy construction for msgspec and pydantic-core schemas."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Mapping
import dataclasses
import datetime as dt
from decimal import Decimal
from fractions import Fraction
import functools
from itertools import starmap
from math import ceil, floor
import ntpath
from pathlib import Path
import string
from typing import get_args, get_type_hints, TypeAliasType, TypedDict, TypeForm

from hypothesis import strategies as st
import msgspec
import msgspec.inspect
import msgspec.msgpack
import pydantic
from pydantic_core import core_schema, CoreSchema

# --- [TYPES] ----------------------------------------------------------------------------


class _Size(TypedDict):
    min_size: int
    max_size: int


# --- [CONSTANTS] ------------------------------------------------------------------------

_NUM_CEILING = 1_000_000
_TEXT_CAP = 64

_JSON: st.SearchStrategy[object] = st.recursive(
    st.none() | st.booleans() | st.integers(min_value=-1_000, max_value=1_000) | st.text(max_size=16),
    lambda inner: st.lists(inner, max_size=3) | st.dictionaries(st.text(min_size=1, max_size=8), inner, max_size=3),
    max_leaves=8,
)
_PATH_PART = st.text(alphabet=string.ascii_lowercase + string.digits, min_size=1, max_size=8).filter(lambda part: not ntpath.isreserved(part))
_PATH = st.lists(_PATH_PART, min_size=1, max_size=3).map(lambda parts: Path(*parts))

# --- [CONSTRAINTS] ----------------------------------------------------------------------


def _size(mn: object, mx: object, cap: int) -> _Size:
    lo = mn if isinstance(mn, int) else 0
    return {"min_size": lo, "max_size": max(lo, min(mx, cap) if isinstance(mx, int) else cap)}


def _timezones(tz: bool | None) -> st.SearchStrategy[dt.tzinfo | None]:  # ruff:ignore[boolean-type-hint-positional-argument]
    match tz:
        case True:
            return st.timezones()
        case False:
            return st.none()
        case None:
            return st.none() | st.timezones()


def _multiples[N](lower: object, upper: object, step: object, convert: Callable[[Decimal], N], *, exclude_lower: bool = False, exclude_upper: bool = False) -> st.SearchStrategy[N]:
    """Return a strategy drawing the multiplier k directly, every value is a valid in-range multiple with zero rejection.

    Fraction bounds are exact for int, float, and Decimal inputs.
    A None bound is the numeric ceiling on that side.
    An exclusive bound equal to a multiple shrinks the k window by one and excludes the boundary itself.
    """
    decimal_step = Decimal(str(step))
    lower_quotient = Fraction(str(-_NUM_CEILING if lower is None else lower)) / Fraction(decimal_step)
    upper_quotient = Fraction(str(_NUM_CEILING if upper is None else upper)) / Fraction(decimal_step)
    lower_ceiling = ceil(lower_quotient)
    upper_floor = floor(upper_quotient)
    minimum_multiplier = lower_ceiling + (1 if exclude_lower and lower_quotient == lower_ceiling else 0)
    maximum_multiplier = upper_floor - (1 if exclude_upper and upper_quotient == upper_floor else 0)
    return (
        st.integers(min_value=minimum_multiplier, max_value=maximum_multiplier).map(lambda multiplier: convert(Decimal(multiplier) * decimal_step))
        if minimum_multiplier <= maximum_multiplier
        else st.nothing()
    )


def _text(mn: object, mx: object, pattern: object) -> st.SearchStrategy[str]:
    lo = mn if isinstance(mn, int) else 1
    hi = min(mx, _TEXT_CAP) if isinstance(mx, int) else _TEXT_CAP
    if lo > hi:
        return st.nothing()
    return st.from_regex(pattern, fullmatch=True).filter(lambda s: lo <= len(s) <= hi) if isinstance(pattern, str) else st.text(min_size=lo, max_size=hi)


# --- [MSGSPEC_SCHEMAS] ------------------------------------------------------------------


def _msgspec_strategy(schema: msgspec.inspect.Type) -> st.SearchStrategy[object]:
    """Return a bounded strategy for a ``msgspec.inspect`` schema.

    Raises:
        AssertionError: The schema kind is unsupported.
    """
    match schema:
        case msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
            lo = next(bound for bound in (ge, gt, -_NUM_CEILING) if bound is not None) + (ge is None and gt is not None)
            hi = next(bound for bound in (le, lt, _NUM_CEILING) if bound is not None) - (le is None and lt is not None)
            step = schema.multiple_of
            return _multiples(lo, hi, step, int) if isinstance(step, int) else st.integers(min_value=lo, max_value=hi)
        case msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt):
            lo_f = next(bound for bound in (ge, gt, -float(_NUM_CEILING)) if bound is not None)
            hi_f = next(bound for bound in (le, lt, float(_NUM_CEILING)) if bound is not None)
            open_lo, open_hi = ge is None and gt is not None, le is None and lt is not None
            step_f = schema.multiple_of
            return (
                _multiples(lo_f, hi_f, step_f, float, exclude_lower=open_lo, exclude_upper=open_hi)
                if isinstance(step_f, int | float)
                else st.floats(min_value=lo_f, max_value=hi_f, exclude_min=open_lo, exclude_max=open_hi, allow_nan=False, allow_infinity=False)
            )
        case msgspec.inspect.StrType(min_length=mn, max_length=mx, pattern=pat):
            return _text(mn, mx, pat)
        case msgspec.inspect.BoolType():
            return st.booleans()
        case msgspec.inspect.BytesType(min_length=mn, max_length=mx):
            return st.binary(**_size(mn, mx, 256))
        case msgspec.inspect.ByteArrayType(min_length=mn, max_length=mx):
            return st.binary(**_size(mn, mx, 256)).map(bytearray)
        case msgspec.inspect.MemoryViewType(min_length=mn, max_length=mx):
            return st.binary(**_size(mn, mx, 256)).map(memoryview)
        case msgspec.inspect.EnumType(cls=cls):
            return st.sampled_from(list(cls))
        case msgspec.inspect.LiteralType(values=values):
            return st.sampled_from(list(values))
        case msgspec.inspect.DateTimeType(tz=tz):
            return st.datetimes(timezones=_timezones(tz))
        case msgspec.inspect.TimeType(tz=tz):
            return st.times(timezones=_timezones(tz))
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
        case msgspec.inspect.VarTupleType(item_type=item, min_length=mn, max_length=mx):
            return st.lists(_msgspec_strategy(item), **_size(mn, mx, 3)).map(tuple)
        case msgspec.inspect.TupleType(item_types=items):
            return st.tuples(*(_msgspec_strategy(item) for item in items))
        case msgspec.inspect.ListType(item_type=item, min_length=mn, max_length=mx) | msgspec.inspect.CollectionType(item_type=item, min_length=mn, max_length=mx):
            return st.lists(_msgspec_strategy(item), **_size(mn, mx, 3))
        case msgspec.inspect.SetType(item_type=item, min_length=mn, max_length=mx) | msgspec.inspect.FrozenSetType(item_type=item, min_length=mn, max_length=mx):
            return st.frozensets(_msgspec_strategy(item), **_size(mn, mx, 3))
        case msgspec.inspect.DictType(key_type=key, value_type=val, min_length=mn, max_length=mx):
            return st.dictionaries(_msgspec_strategy(key), _msgspec_strategy(val), **_size(mn, mx, 3))
        case msgspec.inspect.StructType(cls=cls) | msgspec.inspect.DataclassType(cls=cls) | msgspec.inspect.TypedDictType(cls=cls) | msgspec.inspect.NamedTupleType(cls=cls):
            return strategy_for(cls)
        case msgspec.inspect.RawType():
            return _JSON.map(lambda value: msgspec.Raw(msgspec.json.encode(value)))
        case msgspec.inspect.AnyType():
            return _JSON
        case msgspec.inspect.CustomType(cls=cls):
            return st.from_type(cls)
        case msgspec.inspect.ExtType():
            return st.tuples(st.integers(min_value=0, max_value=127), st.binary(max_size=16)).map(lambda cd: msgspec.msgpack.Ext(*cd))
        case _:  # pragma: no cover
            raise AssertionError(f"unsupported msgspec schema {type(schema).__name__}")


# --- [PYDANTIC_CORE_SCHEMAS] ------------------------------------------------------------


def _integer_bound(schema: Mapping[str, object], inclusive_key: str, exclusive_key: str, offset: int) -> int | None:
    match schema.get(inclusive_key), schema.get(exclusive_key):
        case int() as bound, _:
            return bound
        case _, int() as bound:
            return bound + offset
        case _:
            return None


def _numeric_bound(schema: Mapping[str, object], inclusive_key: str, exclusive_key: str) -> tuple[float | Decimal | None, bool]:
    match schema.get(inclusive_key), schema.get(exclusive_key):
        case Decimal() as bound, _:
            return bound, False
        case int() | float() as bound, _:
            return float(bound), False
        case _, Decimal() as bound:
            return bound, True
        case _, int() | float() as bound:
            return float(bound), True
        case _:
            return None, False


def _pydantic_strategy(schema: CoreSchema, definitions: dict[str, CoreSchema]) -> st.SearchStrategy[object]:
    """Return a constraint-aware strategy for a ``pydantic-core`` schema and its definitions."""
    match schema["type"]:
        case "int":
            lower = _integer_bound(schema, "ge", "gt", 1)
            upper = _integer_bound(schema, "le", "lt", -1)
            multiple_of = schema.get("multiple_of")
            return _multiples(lower, upper, multiple_of, int) if multiple_of is not None else st.integers(min_value=lower, max_value=upper)
        case "float":
            float_lower, exclude_lower = _numeric_bound(schema, "ge", "gt")
            float_upper, exclude_upper = _numeric_bound(schema, "le", "lt")
            multiple_of = schema.get("multiple_of")
            return (
                _multiples(float_lower, float_upper, multiple_of, float, exclude_lower=exclude_lower, exclude_upper=exclude_upper)
                if multiple_of is not None
                else st.floats(min_value=float_lower, max_value=float_upper, exclude_min=exclude_lower, exclude_max=exclude_upper, allow_nan=False, allow_infinity=False)
            )
        case "decimal":
            decimal_lower, exclude_lower = _numeric_bound(schema, "ge", "gt")
            decimal_upper, exclude_upper = _numeric_bound(schema, "le", "lt")
            dp: int | None
            digit_lower: Decimal | None
            digit_upper: Decimal | None
            match schema.get("decimal_places"), schema.get("max_digits"):
                case int() as dp, int() as digits:
                    digit_upper = Decimal(10) ** (digits - dp) - Decimal(10) ** (-dp)
                    digit_lower = -digit_upper
                case int() as dp, _:
                    digit_lower = digit_upper = None
                case _, int() as digits:
                    dp = 0
                    digit_upper = Decimal(10) ** digits - 1
                    digit_lower = -digit_upper
                case _:
                    dp = digit_lower = digit_upper = None
            effective_lower = decimal_lower if decimal_lower is not None else digit_lower
            effective_upper = decimal_upper if decimal_upper is not None else digit_upper
            if (multiple_of := schema.get("multiple_of")) is not None:
                return _multiples(effective_lower, effective_upper, multiple_of, lambda value: value, exclude_lower=exclude_lower, exclude_upper=exclude_upper)
            values = st.decimals(min_value=effective_lower, max_value=effective_upper, places=dp, allow_nan=False, allow_infinity=False)
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
            return st.lists(_pydantic_strategy(schema.get("items_schema", core_schema.any_schema()), definitions), **_size(schema.get("min_length"), schema.get("max_length"), 3))
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
            elements = st.lists(_pydantic_strategy(schema.get("items_schema", core_schema.any_schema()), definitions), max_size=3, unique=True)
            return elements.map(frozenset if kind == "frozenset" else set)
        case "tuple":
            return st.tuples(*(_pydantic_strategy(item, definitions) for item in schema["items_schema"]))
        case "dict":
            keys = _pydantic_strategy(schema.get("keys_schema", core_schema.any_schema()), definitions)
            return st.dictionaries(keys, _pydantic_strategy(schema.get("values_schema", core_schema.any_schema()), definitions), max_size=3)
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


# --- [REGISTRY] -------------------------------------------------------------------------


def _tagged_cases(subject: type) -> dict[str, TypeForm[object]] | None:
    """Return the case fields of an ``expression`` ``@tagged_union`` class mapped to type hints, or ``None`` for any other subject.

    The decorator leaves every dataclass field ``init=False`` and ``kw_only`` behind a leading ``tag`` discriminator and replaces ``__init__`` with an exactly-one-case constructor, field-wise sampling builds invalid unions, and detection keys on the structural signature.
    """
    if not (dataclasses.is_dataclass(subject) and isinstance(subject, type)):
        return None
    fields = dataclasses.fields(subject)
    if not (len(fields) >= 2 and fields[0].name == "tag" and all(not field.init and field.kw_only for field in fields)):
        return None
    hints: dict[str, TypeForm[object]] = get_type_hints(subject, include_extras=True)
    return {f.name: hints[f.name] for f in fields[1:]}


@functools.cache
def _register(subject: type) -> None:
    """Register the Hypothesis strategy of a class once, a tagged union, a pydantic model, or a msgspec-described type.

    The registry takes the strategy as a function of the type, resolved at the first draw, a strategy value would resolve at registration and re-enter this function through the fields that name the class.
    """
    if (cases := _tagged_cases(subject)) is not None:

        def _case(name: str, hint: TypeForm[object]) -> st.SearchStrategy[object]:
            return strategy_for(hint).map(lambda value: subject(**{name: value}))

        st.register_type_strategy(subject, lambda _: st.one_of(*starmap(_case, cases.items())))
    elif issubclass(subject, pydantic.BaseModel):
        model = subject
        st.register_type_strategy(subject, lambda _: _pydantic_strategy(model.__pydantic_core_schema__, {}))
    else:
        match msgspec.inspect.type_info(subject):
            case (
                msgspec.inspect.StructType(fields=fields) | msgspec.inspect.DataclassType(fields=fields) | msgspec.inspect.NamedTupleType(fields=fields) | msgspec.inspect.TypedDictType(fields=fields)
            ):
                st.register_type_strategy(
                    subject,
                    lambda _: st.fixed_dictionaries(
                        {field.name: _msgspec_strategy(field.type) for field in fields if field.required},
                        optional={field.name: _msgspec_strategy(field.type) for field in fields if not field.required},
                    ).map(lambda arguments: subject(**arguments)),
                )
            case _:
                pass


def strategy_for[T](subject: TypeForm[T]) -> st.SearchStrategy[T]:
    """Return a bounded strategy for a type, PEP 695 alias, union, ``Literal``, ``Annotated``, or type expression."""
    if isinstance(subject, TypeAliasType):
        return strategy_for(subject.__value__)
    if not isinstance(subject, type):
        for member in get_args(subject):
            if isinstance(member, type | TypeAliasType):
                strategy_for(member)
        try:
            node = msgspec.inspect.type_info(subject)
        except TypeError:
            return st.from_type(subject)  # ty: ignore[invalid-argument-type]
        return _msgspec_strategy(node)  # type: ignore[return-value]  # ty: ignore[invalid-return-type]
    _register(subject)
    return st.from_type(subject)


# --- [COMPOSITION] ----------------------------------------------------------------------

st.register_type_strategy(Path, lambda _: _PATH)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["strategy_for"]
