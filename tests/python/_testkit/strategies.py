"""Hypothesis strategy resolver for msgspec and pydantic-core schema algebras."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Mapping
import dataclasses
import datetime as dt
from decimal import Decimal
import enum
from fractions import Fraction
from itertools import starmap
from math import ceil, floor
from pathlib import Path
from typing import get_args, get_type_hints, TYPE_CHECKING, TypeAliasType, TypedDict, TypeForm

from hypothesis import strategies as st
import msgspec
import msgspec.inspect as _mi
import msgspec.msgpack
import pydantic


if TYPE_CHECKING:
    from collections.abc import Callable
    from typing import TypeIs


# --- [TYPES] ----------------------------------------------------------------------------

# Per-access narrowing keeps schema walkers free of Any/cast.
type _Schema = Mapping[str, object]


class _Size(TypedDict):
    min_size: int
    max_size: int


# --- [CONSTANTS] ------------------------------------------------------------------------

_EMPTY: _Schema = {}
_CAP = 64  # Bound generated payloads for cheap encoding and shrinking.
_NUM_CEILING = 1_000_000  # Unconstrained numerics span both signs; a one-sided default hides sign defects.

_JSON_SCALAR: st.SearchStrategy[object] = st.one_of(
    st.none(), st.booleans(), st.integers(min_value=-1_000, max_value=1_000), st.text(min_size=0, max_size=16)
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [JSON_ALGEBRA]


def _json_value(depth: int = 0) -> st.SearchStrategy[object]:
    return (
        _JSON_SCALAR
        if depth >= 2
        else st.one_of(
            _JSON_SCALAR,
            st.lists(_json_value(depth + 1), max_size=3),
            st.dictionaries(st.text(min_size=1, max_size=8), _json_value(depth + 1), max_size=3),
        )
    )


# Depth and size caps keep Raw payload generation finite.
_RAW_ST: st.SearchStrategy[msgspec.Raw] = _json_value().map(lambda v: msgspec.Raw(msgspec.json.encode(v)))

# --- [META_SURFACE_ALGEBRA]


def _size(node: object, cap: int) -> _Size:
    mn, mx = getattr(node, "min_length", None), getattr(node, "max_length", None)
    return {"min_size": mn if isinstance(mn, int) else 0, "max_size": min(mx, cap) if isinstance(mx, int) else cap}


# msgspec exposes timezone policy as a tri-state field, not a boolean flag.
def _tz_arg(tz: bool | None) -> st.SearchStrategy[dt.tzinfo | None]:  # ruff:ignore[boolean-type-hint-positional-argument]
    return st.none() if tz is False else st.timezones() if tz else st.none() | st.timezones()


def _multiples[N](
    lo: object, hi: object, step: object, project: Callable[[Decimal], N], *, excl_lo: bool = False, excl_hi: bool = False
) -> st.SearchStrategy[N]:
    """Draw multiplier k directly so every value is a valid in-range multiple with zero rejection.

    Fraction bounds are exact for int, float, and Decimal inputs; an exclusive bound that lands
    exactly on a multiple shrinks the k window by one, so the boundary itself is never drawn.

    Returns:
        Strategy over ``project(k * step)``; ``st.nothing()`` when the window is provably empty.
    """
    s = Decimal(str(step))
    qlo, qhi = Fraction(str(lo)) / Fraction(s), Fraction(str(hi)) / Fraction(s)
    klo = ceil(qlo) + (1 if excl_lo and qlo == ceil(qlo) else 0)
    khi = floor(qhi) - (1 if excl_hi and qhi == floor(qhi) else 0)
    return st.integers(min_value=klo, max_value=khi).map(lambda k: project(Decimal(k) * s)) if klo <= khi else st.nothing()


def _text(mn: object, mx: object, pattern: object) -> st.SearchStrategy[str]:
    # from_regex carries no length bound; constrained patterns need a length filter.
    lo = mn if isinstance(mn, int) else 1
    hi = min(mx, _CAP) if isinstance(mx, int) else _CAP
    return (
        st.nothing()
        if lo > hi  # short-circuit provably-empty window before from_regex exhausts its filter budget
        else st.from_regex(pattern, fullmatch=True).filter(lambda s: lo <= len(s) <= hi)
        if isinstance(pattern, str)
        else st.text(min_size=lo, max_size=hi)
    )


def _decimal_max(md: object, dp: object) -> Decimal | None:
    # Decimal digit budget ceiling for max_digits/decimal_places constraints.
    return Decimal(10) ** (md - dp) - Decimal(10) ** (-dp) if isinstance(md, int) and isinstance(dp, int) else None


# --- [MSGSPEC_NODE_ALGEBRA]


# One polymorphic surface over the closed msgspec node taxonomy.
def _node(node: _mi.Type) -> st.SearchStrategy[object]:  # ruff:ignore[complex-structure, too-many-return-statements]
    """Map one ``msgspec.inspect`` node to a codec-bounded strategy.

    Returns:
        A bounded ``SearchStrategy`` for the node.

    Raises:
        AssertionError: If the node type falls outside the supported msgspec taxonomy (unreachable in
            practice; the match arms cover the complete closed set).
    """
    match node:
        case _mi.IntType(ge=ge, gt=gt, le=le, lt=lt):
            lo = ge if ge is not None else (gt + 1 if gt is not None else -_NUM_CEILING)
            hi = le if le is not None else (lt - 1 if lt is not None else _NUM_CEILING)
            step = node.multiple_of
            return _multiples(lo, hi, step, int) if isinstance(step, int) else st.integers(min_value=lo, max_value=hi)
        case _mi.FloatType(ge=ge, gt=gt, le=le, lt=lt):
            lo_f = ge if ge is not None else (gt if gt is not None else -float(_NUM_CEILING))
            hi_f = le if le is not None else (lt if lt is not None else float(_NUM_CEILING))
            open_lo, open_hi = ge is None and gt is not None, le is None and lt is not None
            step_f = node.multiple_of
            return (
                _multiples(lo_f, hi_f, step_f, float, excl_lo=open_lo, excl_hi=open_hi)
                if isinstance(step_f, int | float)
                else st.floats(min_value=lo_f, max_value=hi_f, exclude_min=open_lo, exclude_max=open_hi, allow_nan=False, allow_infinity=False)
            )
        case _mi.StrType(min_length=mn, max_length=mx, pattern=pat):
            return _text(mn, mx, pat)
        case _mi.BoolType():
            return st.booleans()
        case _mi.BytesType() | _mi.ByteArrayType() | _mi.MemoryViewType():
            binary = st.binary(**_size(node, 256))
            return (
                binary.map(bytearray)
                if isinstance(node, _mi.ByteArrayType)
                else binary.map(memoryview)
                if isinstance(node, _mi.MemoryViewType)
                else binary
            )
        case _mi.EnumType(cls=cls):
            return st.sampled_from(list(cls))
        case _mi.LiteralType(values=values):
            return st.sampled_from(list(values))
        case _mi.DateTimeType(tz=tz):
            return st.datetimes(timezones=_tz_arg(tz))  # msgspec JSON requires RFC 3339-aware datetimes when tz=True
        case _mi.TimeType(tz=tz):
            return st.times(timezones=_tz_arg(tz))
        case _mi.DateType():
            return st.dates()
        case _mi.TimeDeltaType():
            return st.timedeltas()
        case _mi.DecimalType():
            return st.decimals(allow_nan=False, allow_infinity=False)
        case _mi.UUIDType():
            return st.uuids()
        case _mi.NoneType():
            return st.none()
        case _mi.UnionType(types=types):
            return st.one_of(*(_node(t) for t in types))
        case _mi.VarTupleType(item_type=item):
            return st.lists(_node(item), **_size(node, 3)).map(tuple)
        case _mi.TupleType(item_types=items):
            return st.tuples(*(_node(t) for t in items))
        case _mi.ListType(item_type=item) | _mi.CollectionType(item_type=item):
            return st.lists(_node(item), **_size(node, 3))
        case _mi.SetType(item_type=item) | _mi.FrozenSetType(item_type=item):
            return st.frozensets(_node(item), **_size(node, 3))
        case _mi.DictType(key_type=key, value_type=val):
            return st.dictionaries(_node(key), _node(val), **_size(node, 3))
        case _mi.StructType(cls=cls) | _mi.DataclassType(cls=cls) | _mi.TypedDictType(cls=cls) | _mi.NamedTupleType(cls=cls):
            return resolve(cls)
        case _mi.RawType():
            return _RAW_ST
        case _mi.AnyType():
            return _json_value()
        case _mi.CustomType(cls=cls):
            # Schema-opaque leaves route through hypothesis's own registry: a suite-registered
            # `st.register_type_strategy` row resolves; an unregistered class fails loudly at draw.
            return st.from_type(cls)
        case _mi.ExtType():
            return st.tuples(st.integers(min_value=0, max_value=127), st.binary(max_size=16)).map(lambda cd: msgspec.msgpack.Ext(*cd))
        case _:  # pragma: no cover  # provably unreachable: the msgspec node taxonomy above is exhaustive
            raise AssertionError(f"unhandled msgspec node {type(node).__name__}")


# --- [PYDANTIC_CORE_NODE_ALGEBRA]


def _is_schema(v: object) -> TypeIs[_Schema]:
    return isinstance(v, Mapping)


def _sub(schema: _Schema, key: str) -> _Schema:
    v = schema.get(key)
    return v if _is_schema(v) else _EMPTY


def _subs(schema: _Schema, key: str) -> list[_Schema]:
    v = schema.get(key)
    return [c for c in v if _is_schema(c)] if isinstance(v, list) else []


def _ibound(schema: _Schema, incl: str, excl: str, step: int) -> int | None:
    a = schema.get(incl)
    b = schema.get(excl)
    return int(a) if isinstance(a, int) else (int(b) + step if isinstance(b, int) else None)


def _fbound(schema: _Schema, incl: str, excl: str) -> tuple[float | Decimal | None, bool]:
    # Decimal bounds stay unconverted to avoid ULP drift at high ``decimal_places``.
    a, b = schema.get(incl), schema.get(excl)
    return (
        (a if isinstance(a, Decimal) else float(a), False)
        if isinstance(a, int | float | Decimal)
        else (b if isinstance(b, Decimal) else float(b), True)
        if isinstance(b, int | float | Decimal)
        else (None, False)
    )


def _construct(cls: type) -> Callable[[object], object]:
    return lambda payload: cls(**payload) if _is_schema(payload) else cls()


def _unwrap(schema: _Schema) -> _Schema:
    return _unwrap(_sub(schema, "schema")) if str(schema.get("type", "")).startswith("function-") else schema


# One polymorphic surface over the pydantic-core schema algebra.
def _pyd_node(schema: _Schema, defs: dict[str, _Schema]) -> st.SearchStrategy[object]:  # ruff:ignore[complex-structure, too-many-return-statements]
    """Map one ``pydantic-core`` schema node to a constraint-honoring strategy.

    Args:
        schema: A single pydantic-core schema dict node.
        defs: The accumulated ``definition-ref`` table from the enclosing ``definitions`` node.

    Returns:
        A constraint-bounded ``SearchStrategy`` for the node.
    """
    leaf = _unwrap(schema)
    match leaf.get("type"):
        case "int":
            lo_i = _ibound(leaf, "ge", "gt", 1)
            hi_i = _ibound(leaf, "le", "lt", -1)
            mo = leaf.get("multiple_of")
            return (
                _multiples(lo_i if lo_i is not None else -_NUM_CEILING, hi_i if hi_i is not None else _NUM_CEILING, mo, int)
                if isinstance(mo, int)
                else st.integers(min_value=lo_i, max_value=hi_i)
            )
        case "float":
            lo, excl_lo = _fbound(leaf, "ge", "gt")
            hi, excl_hi = _fbound(leaf, "le", "lt")
            mo_f = leaf.get("multiple_of")
            return (
                _multiples(
                    lo if lo is not None else -float(_NUM_CEILING),
                    hi if hi is not None else float(_NUM_CEILING),
                    mo_f,
                    float,
                    excl_lo=excl_lo,
                    excl_hi=excl_hi,
                )
                if isinstance(mo_f, int | float)
                else st.floats(min_value=lo, max_value=hi, exclude_min=excl_lo, exclude_max=excl_hi, allow_nan=False, allow_infinity=False)
            )
        case "decimal":
            lo_d, excl_lo = _fbound(leaf, "ge", "gt")
            hi_d, excl_hi = _fbound(leaf, "le", "lt")
            places, digits = leaf.get("decimal_places"), leaf.get("max_digits")
            # A digit budget without declared places generates at places=0: every draw stays digit-bounded.
            dp = places if isinstance(places, int) else (0 if isinstance(digits, int) else None)
            digit_max = _decimal_max(digits, dp)
            lo_eff = lo_d if lo_d is not None else (-digit_max if digit_max is not None else None)
            hi_eff = hi_d if hi_d is not None else digit_max
            mo_d = leaf.get("multiple_of")
            if isinstance(mo_d, int | float | Decimal):
                return _multiples(
                    lo_eff if lo_eff is not None else -_NUM_CEILING,
                    hi_eff if hi_eff is not None else _NUM_CEILING,
                    mo_d,
                    lambda value: value,
                    excl_lo=excl_lo,
                    excl_hi=excl_hi,
                )
            base = st.decimals(min_value=lo_eff, max_value=hi_eff, places=dp, allow_nan=False, allow_infinity=False)
            # st.decimals has no exclusivity knobs; the filter rejects only exact-boundary draws.
            return (
                base.filter(lambda d: (not excl_lo or lo_eff is None or d > lo_eff) and (not excl_hi or hi_eff is None or d < hi_eff))
                if (excl_lo or excl_hi)
                else base
            )
        case "str":
            return _text(leaf.get("min_length"), leaf.get("max_length"), leaf.get("pattern"))
        case "bytes":
            mn_b, mx_b = leaf.get("min_length"), leaf.get("max_length")
            return st.binary(min_size=mn_b if isinstance(mn_b, int) else 0, max_size=mx_b if isinstance(mx_b, int) else 256)
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
            exp = leaf.get("expected")
            return st.sampled_from(exp) if isinstance(exp, list) and exp else st.none()
        case "nullable":
            return st.none() | _pyd_node(_sub(leaf, "schema"), defs)
        case "default":
            return _pyd_node(_sub(leaf, "schema"), defs)
        case "list":
            mn_l = leaf.get("min_length")
            mx_l = leaf.get("max_length")
            return st.lists(
                _pyd_node(_sub(leaf, "items_schema"), defs),
                min_size=mn_l if isinstance(mn_l, int) else 0,
                max_size=mx_l if isinstance(mx_l, int) else 3,
            )
        case "set" | "frozenset":
            elems = st.lists(_pyd_node(_sub(leaf, "items_schema"), defs), max_size=3, unique=True)
            return elems.map(frozenset) if leaf.get("type") == "frozenset" else elems.map(set)
        case "tuple":
            items = _subs(leaf, "items_schema")
            return st.tuples(*(_pyd_node(i, defs) for i in items)) if items else st.tuples()
        case "dict":
            return st.dictionaries(_pyd_node(_sub(leaf, "keys_schema"), defs), _pyd_node(_sub(leaf, "values_schema"), defs), max_size=3)
        case "union":
            return st.one_of(*(_pyd_node(c, defs) for c in _subs(leaf, "choices")))
        case "tagged-union":
            choices = leaf.get("choices")
            return st.one_of(*(_pyd_node(c, defs) for c in choices.values() if _is_schema(c))) if isinstance(choices, Mapping) else st.none()
        case "model" | "dataclass":
            cls = leaf.get("cls")
            fields_strat = _pyd_node(_sub(leaf, "schema"), defs)
            return fields_strat.map(_construct(cls)) if isinstance(cls, type) else fields_strat
        case "model-fields" | "dataclass-args" | "typed-dict":
            fields = leaf.get("fields")
            if not _is_schema(fields):
                return st.fixed_dictionaries({})
            required = {
                str(n): _pyd_node(_sub(f, "schema"), defs) for n, f in fields.items() if _is_schema(f) and _sub(f, "schema").get("type") != "default"
            }
            optional = {
                str(n): _pyd_node(_sub(f, "schema"), defs) for n, f in fields.items() if _is_schema(f) and _sub(f, "schema").get("type") == "default"
            }
            return st.fixed_dictionaries(required, optional=optional)
        case "model-field" | "dataclass-field" | "typed-dict-field":
            return _pyd_node(_sub(leaf, "schema"), defs)
        case "definitions":
            merged = dict(defs)
            merged.update({ref: d for d in _subs(leaf, "definitions") if isinstance(ref := d.get("ref"), str)})
            return _pyd_node(_sub(leaf, "schema"), merged)
        case "definition-ref":
            ref = leaf.get("schema_ref")
            return st.deferred(_deferred_ref(ref, defs)) if isinstance(ref, str) and ref in defs else st.none()
        case _:
            return st.none()  # url/json/call/custom leaves have no schema-derivable generator.


def _deferred_ref(ref: str, defs: dict[str, _Schema]) -> Callable[[], st.SearchStrategy[object]]:
    return lambda: _pyd_node(defs[ref], defs)


def _tagged_cases(subject: type) -> dict[str, TypeForm[object]] | None:
    """Detect an ``expression`` ``@tagged_union`` class and map its case fields to type hints.

    The decorator leaves every dataclass field ``init=False``/``kw_only`` behind a leading ``tag``
    discriminator and replaces ``__init__`` with an exactly-one-case constructor, so field-wise
    sampling constructs invalid unions; detection keys on that structural signature.

    Returns:
        Case-name to type-hint mapping, or ``None`` when the subject is not a tagged union.
    """
    if not (dataclasses.is_dataclass(subject) and isinstance(subject, type)):
        return None
    fields = dataclasses.fields(subject)
    shaped = len(fields) >= 2 and fields[0].name == "tag" and all(not f.init and f.kw_only for f in fields)
    if not shaped:
        return None
    hints: dict[str, TypeForm[object]] = get_type_hints(subject, include_extras=True)  # keep Annotated constraints on case fields
    return {f.name: hints[f.name] for f in fields[1:]}


_REGISTERED: set[type] = set()


# One entry point over alias, pydantic, msgspec, and native type-form algebras.
def resolve[T](subject: TypeForm[T]) -> st.SearchStrategy[T]:
    """Register a constraint-bounded strategy for ``subject`` and return its strategy.

    Args:
        subject: Concrete type, PEP 695 alias, union, ``Literal``, ``Annotated``, or type expression.

    Returns:
        A strategy producing valid, encode-clean instances of ``T``.
    """
    if isinstance(subject, TypeAliasType):
        return resolve(subject.__value__)
    if not isinstance(subject, type):
        # Non-class type forms — unions, Annotated metadata, Literal — route through the msgspec node
        # algebra so constraints survive; member types register first so any from_type fallback lands
        # on registered strategies instead of unconstrained builds.
        for member in get_args(subject):
            resolve(member) if isinstance(member, type | TypeAliasType) else None
        try:
            node = _mi.type_info(subject)
        except TypeError:
            # hypothesis accepts non-class type forms at runtime; its stub under-declares from_type as type[T].
            return st.from_type(subject)  # ty: ignore[invalid-argument-type]
        # The node algebra proves the element type; TypeForm cannot recover it statically.
        return _node(node)  # type: ignore[return-value]  # ty: ignore[invalid-return-type]
    if subject not in _REGISTERED:
        _REGISTERED.add(subject)
        if (cases := _tagged_cases(subject)) is not None:
            union = subject

            # Exactly one case per draw: the constructor rejects zero-case and multi-case payloads.
            def _union_arm(name: str, hint: TypeForm[object]) -> st.SearchStrategy[object]:
                return resolve(hint).map(lambda v: union(**{name: v}))

            def _union_build() -> st.SearchStrategy[object]:
                return st.one_of(*starmap(_union_arm, cases.items()))

            st.register_type_strategy(subject, st.deferred(_union_build))
        elif issubclass(subject, pydantic.BaseModel):
            model = subject

            def _pyd_build() -> st.SearchStrategy[object]:
                cs = model.__pydantic_core_schema__
                return _pyd_node(cs, {}) if _is_schema(cs) else st.builds(model)

            st.register_type_strategy(subject, st.deferred(_pyd_build))
        else:
            match _mi.type_info(subject):
                case _mi.StructType(fields=fields) | _mi.DataclassType(fields=fields) | _mi.NamedTupleType(fields=fields):
                    struct = subject

                    # Defaulted fields sample presence AND absence: msgspec collapses `T | UnsetType`
                    # to a required=False node, so omission is the only route to the UNSET wire lane.
                    def _struct_build() -> st.SearchStrategy[object]:
                        required = {f.name: _node(f.type) for f in fields if f.required}
                        optional = {f.name: _node(f.type) for f in fields if not f.required}
                        return st.fixed_dictionaries(required, optional=optional).map(lambda kw: struct(**kw))

                    st.register_type_strategy(subject, st.deferred(_struct_build))
                case _mi.TypedDictType(fields=fields):

                    def _td_build() -> st.SearchStrategy[object]:
                        return st.fixed_dictionaries(
                            {f.name: _node(f.type) for f in fields if f.required}, optional={f.name: _node(f.type) for f in fields if not f.required}
                        )

                    st.register_type_strategy(subject, st.deferred(_td_build))
                case _:
                    pass  # enums, leaves, unions, Literal, containers: from_type resolves natively

    return st.from_type(subject)


# --- [COMPOSITION] ----------------------------------------------------------------------

# Kit-owned opaque-leaf baseline: Path fields draw short portable relative paths (msgspec encodes
# Path as its string form), so struct resolution never degrades to a single-value builds(Path).
_SEGMENT: st.SearchStrategy[str] = st.text(alphabet="abcdefghijklmnopqrstuvwxyz0123456789", min_size=1, max_size=8)
st.register_type_strategy(Path, st.lists(_SEGMENT, min_size=1, max_size=3).map(lambda parts: Path(*parts)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["resolve"]
