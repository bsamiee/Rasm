"""Hypothesis strategy construction for msgspec and pydantic-core schemas."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Mapping
import dataclasses
import datetime as dt
from decimal import Decimal
import enum
from fractions import Fraction
import functools
from itertools import starmap
from math import ceil, floor
from pathlib import Path
from typing import get_args, get_type_hints, TYPE_CHECKING, TypeAliasType, TypedDict, TypeForm

from hypothesis import strategies as st
import msgspec
import msgspec.inspect
import msgspec.msgpack
import pydantic

if TYPE_CHECKING:
    from collections.abc import Callable
    from typing import TypeIs


# --- [TYPES] ----------------------------------------------------------------------------

type _Schema = Mapping[str, object]


class _Size(TypedDict):
    min_size: int
    max_size: int


# --- [CONSTANTS] ------------------------------------------------------------------------

_CAP = 64
_NUM_CEILING = 1_000_000

_JSON_SCALAR: st.SearchStrategy[object] = st.one_of(
    st.none(), st.booleans(), st.integers(min_value=-1_000, max_value=1_000), st.text(min_size=0, max_size=16)
)

# --- [JSON_VALUES] ----------------------------------------------------------------------


def _json_value(depth: int = 0) -> st.SearchStrategy[object]:
    return (
        _JSON_SCALAR
        if depth >= 2
        else st.one_of(
            _JSON_SCALAR, st.lists((inner := _json_value(depth + 1)), max_size=3), st.dictionaries(st.text(min_size=1, max_size=8), inner, max_size=3)
        )
    )


_RAW_VALUES: st.SearchStrategy[msgspec.Raw] = _json_value().map(lambda value: msgspec.Raw(msgspec.json.encode(value)))

# --- [CONSTRAINTS] ----------------------------------------------------------------------


def _size(node: object, cap: int) -> _Size:
    mn, mx = getattr(node, "min_length", None), getattr(node, "max_length", None)
    return {"min_size": mn if isinstance(mn, int) else 0, "max_size": min(mx, cap) if isinstance(mx, int) else cap}


def _tz_arg(tz: bool | None) -> st.SearchStrategy[dt.tzinfo | None]:  # ruff:ignore[boolean-type-hint-positional-argument]
    match tz:
        case True:
            return st.timezones()
        case False:
            return st.none()
        case None:
            return st.none() | st.timezones()


def _multiples[N](
    lower: object, upper: object, step: object, convert: Callable[[Decimal], N], *, exclude_lower: bool = False, exclude_upper: bool = False
) -> st.SearchStrategy[N]:
    """Return a strategy drawing the multiplier k directly, every value is a valid in-range multiple with zero rejection.

    Fraction bounds are exact for int, float, and Decimal inputs, an exclusive bound equal to a multiple shrinks the k window by one and excludes the boundary itself.
    """
    decimal_step = Decimal(str(step))
    lower_quotient = Fraction(str(lower)) / Fraction(decimal_step)
    upper_quotient = Fraction(str(upper)) / Fraction(decimal_step)
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
    hi = min(mx, _CAP) if isinstance(mx, int) else _CAP
    if lo > hi:
        return st.nothing()
    return (
        st.from_regex(pattern, fullmatch=True).filter(lambda s: lo <= len(s) <= hi) if isinstance(pattern, str) else st.text(min_size=lo, max_size=hi)
    )


def _decimal_bounds(md: object, dp: object) -> tuple[Decimal | None, Decimal | None]:
    """Return the lowest and highest decimal the digit and place limits admit, unbounded when either limit is absent."""
    if not (isinstance(md, int) and isinstance(dp, int)):
        return None, None
    limit = Decimal(10) ** (md - dp) - Decimal(10) ** (-dp)
    return -limit, limit


# --- [MSGSPEC_SCHEMAS] ------------------------------------------------------------------


def _msgspec_strategy(schema: msgspec.inspect.Type) -> st.SearchStrategy[object]:  # ruff:ignore[complex-structure]
    """Return a bounded strategy for a ``msgspec.inspect`` schema.

    Raises:
        AssertionError: The schema kind is unsupported.
    """
    match schema:
        case msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
            # First bound set wins, an exclusive integer bound folds inward by one
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
        case msgspec.inspect.BytesType():
            return st.binary(**_size(schema, 256))
        case msgspec.inspect.ByteArrayType():
            return st.binary(**_size(schema, 256)).map(bytearray)
        case msgspec.inspect.MemoryViewType():
            return st.binary(**_size(schema, 256)).map(memoryview)
        case msgspec.inspect.EnumType(cls=cls):
            return st.sampled_from(list(cls))
        case msgspec.inspect.LiteralType(values=values):
            return st.sampled_from(list(values))
        case msgspec.inspect.DateTimeType(tz=tz):
            return st.datetimes(timezones=_tz_arg(tz))
        case msgspec.inspect.TimeType(tz=tz):
            return st.times(timezones=_tz_arg(tz))
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
        case msgspec.inspect.VarTupleType(item_type=item):
            return st.lists(_msgspec_strategy(item), **_size(schema, 3)).map(tuple)
        case msgspec.inspect.TupleType(item_types=items):
            return st.tuples(*(_msgspec_strategy(item) for item in items))
        case msgspec.inspect.ListType(item_type=item) | msgspec.inspect.CollectionType(item_type=item):
            return st.lists(_msgspec_strategy(item), **_size(schema, 3))
        case msgspec.inspect.SetType(item_type=item) | msgspec.inspect.FrozenSetType(item_type=item):
            return st.frozensets(_msgspec_strategy(item), **_size(schema, 3))
        case msgspec.inspect.DictType(key_type=key, value_type=val):
            return st.dictionaries(_msgspec_strategy(key), _msgspec_strategy(val), **_size(schema, 3))
        case (
            msgspec.inspect.StructType(cls=cls)
            | msgspec.inspect.DataclassType(cls=cls)
            | msgspec.inspect.TypedDictType(cls=cls)
            | msgspec.inspect.NamedTupleType(cls=cls)
        ):
            return strategy_for(cls)
        case msgspec.inspect.RawType():
            return _RAW_VALUES
        case msgspec.inspect.AnyType():
            return _json_value()
        case msgspec.inspect.CustomType(cls=cls):
            return st.from_type(cls)
        case msgspec.inspect.ExtType():
            return st.tuples(st.integers(min_value=0, max_value=127), st.binary(max_size=16)).map(lambda cd: msgspec.msgpack.Ext(*cd))
        case _:  # pragma: no cover
            raise AssertionError(f"unsupported msgspec schema {type(schema).__name__}")


# --- [PYDANTIC_CORE_SCHEMAS] ------------------------------------------------------------


def _is_schema(v: object) -> TypeIs[_Schema]:
    return isinstance(v, Mapping)


def _schema_member(schema: _Schema, key: str) -> _Schema:
    v = schema.get(key)
    return v if _is_schema(v) else {}


def _schema_members(schema: _Schema, key: str) -> list[_Schema]:
    v = schema.get(key)
    return [c for c in v if _is_schema(c)] if isinstance(v, list) else []


def _integer_bound(schema: _Schema, inclusive_key: str, exclusive_key: str, offset: int) -> int | None:
    match schema.get(inclusive_key), schema.get(exclusive_key):
        case int() as bound, _:
            return bound
        case _, int() as bound:
            return bound + offset
        case _:
            return None


def _numeric_bound(schema: _Schema, inclusive_key: str, exclusive_key: str) -> tuple[float | Decimal | None, bool]:
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


def _construct(cls: type) -> Callable[[object], object]:
    return lambda fields: cls(**fields) if _is_schema(fields) else cls()


def _unwrap_function_schema(schema: _Schema) -> _Schema:
    return _unwrap_function_schema(_schema_member(schema, "schema")) if str(schema.get("type", "")).startswith("function-") else schema


def _pydantic_strategy(schema: _Schema, definitions: dict[str, _Schema]) -> st.SearchStrategy[object]:  # ruff:ignore[complex-structure]
    """Return a constraint-aware strategy for a ``pydantic-core`` schema and its definitions."""
    leaf = _unwrap_function_schema(schema)
    match leaf.get("type"):
        case "int":
            lower = _integer_bound(leaf, "ge", "gt", 1)
            upper = _integer_bound(leaf, "le", "lt", -1)
            multiple_of = leaf.get("multiple_of")
            return (
                _multiples(lower if lower is not None else -_NUM_CEILING, upper if upper is not None else _NUM_CEILING, multiple_of, int)
                if isinstance(multiple_of, int)
                else st.integers(min_value=lower, max_value=upper)
            )
        case "float":
            float_lower, exclude_lower = _numeric_bound(leaf, "ge", "gt")
            float_upper, exclude_upper = _numeric_bound(leaf, "le", "lt")
            multiple_of = leaf.get("multiple_of")
            return (
                _multiples(
                    float_lower if float_lower is not None else -float(_NUM_CEILING),
                    float_upper if float_upper is not None else float(_NUM_CEILING),
                    multiple_of,
                    float,
                    exclude_lower=exclude_lower,
                    exclude_upper=exclude_upper,
                )
                if isinstance(multiple_of, int | float)
                else st.floats(
                    min_value=float_lower,
                    max_value=float_upper,
                    exclude_min=exclude_lower,
                    exclude_max=exclude_upper,
                    allow_nan=False,
                    allow_infinity=False,
                )
            )
        case "decimal":
            decimal_lower, exclude_lower = _numeric_bound(leaf, "ge", "gt")
            decimal_upper, exclude_upper = _numeric_bound(leaf, "le", "lt")
            places, digits = leaf.get("decimal_places"), leaf.get("max_digits")
            dp: int | None
            match places, digits:
                case int(), _:
                    dp = places
                case _, int():
                    dp = 0
                case _:
                    dp = None
            digit_min, digit_max = _decimal_bounds(digits, dp)
            effective_lower = decimal_lower if decimal_lower is not None else digit_min
            effective_upper = decimal_upper if decimal_upper is not None else digit_max
            multiple_of = leaf.get("multiple_of")
            if isinstance(multiple_of, int | float | Decimal):
                return _multiples(
                    effective_lower if effective_lower is not None else -_NUM_CEILING,
                    effective_upper if effective_upper is not None else _NUM_CEILING,
                    multiple_of,
                    lambda value: value,
                    exclude_lower=exclude_lower,
                    exclude_upper=exclude_upper,
                )
            values = st.decimals(min_value=effective_lower, max_value=effective_upper, places=dp, allow_nan=False, allow_infinity=False)
            return (
                values.filter(
                    lambda value: (
                        (not exclude_lower or effective_lower is None or value > effective_lower)
                        and (not exclude_upper or effective_upper is None or value < effective_upper)
                    )
                )
                if (exclude_lower or exclude_upper)
                else values
            )
        case "str":
            return _text(leaf.get("min_length"), leaf.get("max_length"), leaf.get("pattern"))
        case "bytes":
            minimum_length, maximum_length = leaf.get("min_length"), leaf.get("max_length")
            return st.binary(
                min_size=minimum_length if isinstance(minimum_length, int) else 0, max_size=maximum_length if isinstance(maximum_length, int) else 256
            )
        case "bool":
            return st.booleans()
        case "none":
            return st.none()
        case "any":
            return _json_value()
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
            cls = leaf.get("cls")
            return st.sampled_from(list(cls)) if isinstance(cls, type) and issubclass(cls, enum.Enum) else st.none()
        case "literal":
            expected = leaf.get("expected")
            return st.sampled_from(expected) if isinstance(expected, list) and expected else st.none()
        case "nullable":
            return st.none() | _pydantic_strategy(_schema_member(leaf, "schema"), definitions)
        case "default":
            return _pydantic_strategy(_schema_member(leaf, "schema"), definitions)
        case "list":
            minimum_length = leaf.get("min_length")
            maximum_length = leaf.get("max_length")
            return st.lists(
                _pydantic_strategy(_schema_member(leaf, "items_schema"), definitions),
                min_size=minimum_length if isinstance(minimum_length, int) else 0,
                max_size=maximum_length if isinstance(maximum_length, int) else 3,
            )
        case "set" | "frozenset" as kind:
            elements = st.lists(_pydantic_strategy(_schema_member(leaf, "items_schema"), definitions), max_size=3, unique=True)
            return elements.map(frozenset if kind == "frozenset" else set)
        case "tuple":
            items = _schema_members(leaf, "items_schema")
            return st.tuples(*(_pydantic_strategy(item, definitions) for item in items)) if items else st.tuples()
        case "dict":
            return st.dictionaries(
                _pydantic_strategy(_schema_member(leaf, "keys_schema"), definitions),
                _pydantic_strategy(_schema_member(leaf, "values_schema"), definitions),
                max_size=3,
            )
        case "union":
            return st.one_of(*(_pydantic_strategy(choice, definitions) for choice in _schema_members(leaf, "choices")))
        case "tagged-union":
            choices = leaf.get("choices")
            return (
                st.one_of(*(_pydantic_strategy(choice, definitions) for choice in choices.values() if _is_schema(choice)))
                if isinstance(choices, Mapping)
                else st.none()
            )
        case "model" | "dataclass":
            cls = leaf.get("cls")
            field_values = _pydantic_strategy(_schema_member(leaf, "schema"), definitions)
            return field_values.map(_construct(cls)) if isinstance(cls, type) else field_values
        case "model-fields" | "dataclass-args" | "typed-dict":
            fields = leaf.get("fields")
            if not _is_schema(fields):
                return st.fixed_dictionaries({})
            required = {
                str(name): _pydantic_strategy(member, definitions)
                for name, field in fields.items()
                if _is_schema(field) and (member := _schema_member(field, "schema")).get("type") != "default"
            }
            optional = {
                str(name): _pydantic_strategy(member, definitions)
                for name, field in fields.items()
                if _is_schema(field) and (member := _schema_member(field, "schema")).get("type") == "default"
            }
            return st.fixed_dictionaries(required, optional=optional)
        case "model-field" | "dataclass-field" | "typed-dict-field":
            return _pydantic_strategy(_schema_member(leaf, "schema"), definitions)
        case "definitions":
            merged = dict(definitions)
            merged.update({
                reference: definition for definition in _schema_members(leaf, "definitions") if isinstance(reference := definition.get("ref"), str)
            })
            return _pydantic_strategy(_schema_member(leaf, "schema"), merged)
        case "definition-ref":
            ref = leaf.get("schema_ref")
            return (
                st.deferred(lambda: _pydantic_strategy(definitions[ref], definitions)) if isinstance(ref, str) and ref in definitions else st.none()
            )
        case _:
            return st.none()


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

        def _case_strategy(name: str, hint: TypeForm[object]) -> st.SearchStrategy[object]:
            return strategy_for(hint).map(lambda value: subject(**{name: value}))

        st.register_type_strategy(subject, lambda _: st.one_of(*starmap(_case_strategy, cases.items())))
    elif issubclass(subject, pydantic.BaseModel):
        model = subject

        def _pydantic_build() -> st.SearchStrategy[object]:
            schema = model.__pydantic_core_schema__
            return _pydantic_strategy(schema, {}) if _is_schema(schema) else st.builds(model)

        st.register_type_strategy(subject, lambda _: _pydantic_build())
    else:
        match msgspec.inspect.type_info(subject):
            case (
                msgspec.inspect.StructType(fields=fields)
                | msgspec.inspect.DataclassType(fields=fields)
                | msgspec.inspect.NamedTupleType(fields=fields)
            ):

                def _struct_build() -> st.SearchStrategy[object]:
                    required = {field.name: _msgspec_strategy(field.type) for field in fields if field.required}
                    optional = {field.name: _msgspec_strategy(field.type) for field in fields if not field.required}
                    return st.fixed_dictionaries(required, optional=optional).map(lambda arguments: subject(**arguments))

                st.register_type_strategy(subject, lambda _: _struct_build())
            case msgspec.inspect.TypedDictType(fields=fields):

                def _typed_dict_build() -> st.SearchStrategy[object]:
                    return st.fixed_dictionaries(
                        {field.name: _msgspec_strategy(field.type) for field in fields if field.required},
                        optional={field.name: _msgspec_strategy(field.type) for field in fields if not field.required},
                    )

                st.register_type_strategy(subject, lambda _: _typed_dict_build())
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

_SEGMENT: st.SearchStrategy[str] = st.text(alphabet="abcdefghijklmnopqrstuvwxyz0123456789", min_size=1, max_size=8)
st.register_type_strategy(Path, st.lists(_SEGMENT, min_size=1, max_size=3).map(lambda parts: Path(*parts)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["strategy_for"]
