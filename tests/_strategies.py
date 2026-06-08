"""Generalized Hypothesis strategy resolver over the msgspec and pydantic-core schema node algebras.

Project-agnostic force-multiplier: ``resolve(T)`` registers a bounded strategy for any structured type so
``st.from_type(T)`` yields VALID, encode-clean instances with real field values. Two structurally-isomorphic
walkers drive it — ``_node`` over ``msgspec.inspect`` nodes (Struct / dataclass / TypedDict / leaves), and
``_pyd_node`` over ``__pydantic_core_schema__`` (the only source that exposes ``pattern``, ``multiple_of``,
and nested-container item constraints — native ``st.from_type`` silently drops these and generates instances
that fail validation). Both read the constraint metadata exactly so generated values survive a round-trip and
their own model validators. Custom ``@field_validator`` / ``@model_validator`` bodies are opaque: instances are
generated from the declared schema constraints, and a model whose validator rejects them must register an
explicit strategy via ``st.register_type_strategy`` before calling ``resolve``.
"""

# --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------

from collections.abc import Mapping
import datetime as dt
from decimal import Decimal
import enum
from math import ceil, floor
from typing import TYPE_CHECKING, TypeAliasType, TypedDict, TypeForm

from hypothesis import strategies as st
import msgspec
import msgspec.inspect as _mi
import pydantic


if TYPE_CHECKING:
    from collections.abc import Callable
    from typing import TypeIs


# --- [TYPES] --------------------------------------------------------------------------------

# `__pydantic_core_schema__` is runtime-materialized dict data; modelled as a string-keyed object map and
# narrowed per-access. This is the type-clean boundary shape — no `Any`, no `cast`.
type _Schema = Mapping[str, object]


# Exactly the two size kwargs the collection strategies accept, so the `**` spread type-resolves precisely.
class _Size(TypedDict):
    min_size: int
    max_size: int


# --- [CONSTANTS] ----------------------------------------------------------------------------


_REGISTERED: set[type] = set()
_EMPTY: _Schema = {}
_CAP = 64  # collection/string size cap keeps generated payloads encode-cheap

# Bounded recursive JSON-value strategy: covers all JSON leaf + container shapes while keeping depth/size small
# so generated msgspec.Raw payloads are deterministic-encode-safe and round-trip cleanly.
_JSON_SCALAR: st.SearchStrategy[object] = st.one_of(
    st.none(), st.booleans(), st.integers(min_value=-1_000, max_value=1_000), st.text(min_size=0, max_size=16)
)


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


_RAW_ST: st.SearchStrategy[msgspec.Raw] = _json_value().map(lambda v: msgspec.Raw(msgspec.json.encode(v)))

# --- [OPERATIONS] ---------------------------------------------------------------------------
# --- [META_SURFACE_ALGEBRA] -----------------------------------------------------------------
# Shared constraint reads both walkers call so the file's contract ("both read the constraint metadata
# exactly") holds: one length reader, one multiple-of reader, one constructive multiples generator, one
# tz tri-state, one length-bounded text/regex composer, one max-digits-bounded decimal ceiling.


def _len(node: object) -> tuple[int | None, int | None]:
    # Common `min_length`/`max_length` surface shared by every container/bytes/str msgspec node.
    return (getattr(node, "min_length", None), getattr(node, "max_length", None))


def _size(node: object, cap: int) -> _Size:
    # Thread the node's length bounds into `min_size`/`max_size`, clamped to `cap` to stay encode-cheap.
    mn, mx = _len(node)
    return {"min_size": mn if isinstance(mn, int) else 0, "max_size": min(mx, cap) if isinstance(mx, int) else cap}


def _mult(node: object) -> int | float | None:
    return getattr(node, "multiple_of", None)


def _tz_arg(tz: bool | None) -> st.SearchStrategy[dt.tzinfo | None]:  # noqa: FBT001  # `tz` mirrors the msgspec node's own `bool | None` tri-state field, not a boolean flag
    # Tri-state RFC 3339 tz policy: False -> naive only, True -> aware only, None -> either.
    return st.none() if tz is False else st.timezones() if tz else st.none() | st.timezones()


def _multiples_int(lo: int, hi: int, step: int) -> st.SearchStrategy[int]:
    # Constructive: draw the multiplier directly so every value is a valid multiple (zero rejection). An empty
    # multiplier range (no multiple of `step` in [lo, hi]) yields `st.nothing()` so the draw never raises.
    klo, khi = ceil(lo / step), floor(hi / step)
    return st.integers(min_value=klo, max_value=khi).map(lambda k: k * step) if klo <= khi else st.nothing()


def _multiples_float(lo: float, hi: float, step: float) -> st.SearchStrategy[float]:
    # Decimal intermediate keeps `k*step` free of binary drift before projecting back to float; an empty
    # multiplier range collapses to `st.nothing()` rather than letting hypothesis raise on draw.
    s = Decimal(str(step))
    klo, khi = ceil(Decimal(str(lo)) / s), floor(Decimal(str(hi)) / s)
    return st.integers(min_value=klo, max_value=khi).map(lambda k: float(Decimal(k) * s)) if klo <= khi else st.nothing()


def _text(mn: object, mx: object, pattern: object) -> st.SearchStrategy[str]:
    # `from_regex` carries NO length bound and overruns `max_length` on unbounded patterns, so a length
    # filter is mandatory when a pattern is present; otherwise a directly-bounded `st.text`. Inputs are the
    # raw `object`-typed schema/node values; narrowed here so both walkers share one length-bounded composer.
    lo = mn if isinstance(mn, int) else 1
    hi = min(mx, _CAP) if isinstance(mx, int) else _CAP
    return (
        st.from_regex(pattern, fullmatch=True).filter(lambda s: lo <= len(s) <= hi) if isinstance(pattern, str) else st.text(min_size=lo, max_size=hi)
    )


def _decimal_max(md: object, dp: object) -> Decimal | None:
    # Largest value within the digit budget: 10^(max_digits-decimal_places) - 10^(-decimal_places).
    return Decimal(10) ** (md - dp) - Decimal(10) ** (-dp) if isinstance(md, int) and isinstance(dp, int) else None


# --- [MSGSPEC_NODE_ALGEBRA] -----------------------------------------------------------------


def _node(node: _mi.Type) -> st.SearchStrategy[object]:  # noqa: C901, PLR0911, PLR0912, PLR0914, PLR0915  # closed taxonomy: one polymorphic surface over the msgspec node algebra
    """Map one ``msgspec.inspect`` node to a codec-bounded strategy.

    Reads ``ge``/``gt``/``le``/``lt``/``max_length`` so generated values survive a msgspec JSON
    encode/decode round-trip. ``CustomType`` (Callable, Path, …) maps to ``st.none()`` — the safe default
    for JSON-opaque leaves. Struct-like arms recurse through ``resolve`` so nested types are registered too.

    Returns:
        A bounded ``SearchStrategy`` for the node.

    Raises:
        AssertionError: When the node type is absent from the supported taxonomy.
    """
    match node:
        case _mi.IntType(ge=ge, gt=gt, le=le, lt=lt):
            lo = ge if ge is not None else (gt + 1 if gt is not None else 0)
            hi = le if le is not None else (lt - 1 if lt is not None else 1_000_000)
            step = _mult(node)
            return _multiples_int(lo, hi, step) if isinstance(step, int) else st.integers(min_value=lo, max_value=hi)
        case _mi.FloatType(ge=ge, gt=gt, le=le, lt=lt):
            lo_f = ge if ge is not None else (gt if gt is not None else 0.0)
            hi_f = le if le is not None else (lt if lt is not None else 1_000_000.0)
            step_f = _mult(node)
            return (
                _multiples_float(lo_f, hi_f, step_f)
                if isinstance(step_f, int | float)
                else st.floats(
                    min_value=lo_f,
                    max_value=hi_f,
                    exclude_min=ge is None and gt is not None,
                    exclude_max=le is None and lt is not None,
                    allow_nan=False,
                    allow_infinity=False,
                )
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
            return st.datetimes(timezones=_tz_arg(tz))  # msgspec JSON requires tz-aware (RFC 3339) datetimes when tz is True
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
        case _mi.CustomType() | _mi.ExtType():
            return st.none()  # JSON-opaque / schema-opaque leaves (Callable, Path, Ext): no stable projection → encode-clean None
        case _:  # pragma: no cover  # provably unreachable: the msgspec node taxonomy above is exhaustive
            raise AssertionError(f"unhandled msgspec node {type(node).__name__}")


# --- [PYDANTIC_CORE_NODE_ALGEBRA] -----------------------------------------------------------


def _is_schema(v: object) -> TypeIs[_Schema]:
    # pydantic-core schema nodes are always string-keyed dicts; TypeIs declares the narrowing isinstance can't.
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


def _fbound(schema: _Schema, incl: str, excl: str) -> tuple[float | None, bool]:
    # Decimal-typed bounds: pydantic-core emits `ge`/`le` as `Decimal` when the constraint is a Decimal literal,
    # so the gate must admit Decimal alongside int/float or the decimal arm silently drops its bound.
    a, b = schema.get(incl), schema.get(excl)
    return (float(a), False) if isinstance(a, int | float | Decimal) else (float(b), True) if isinstance(b, int | float | Decimal) else (None, False)


def _construct(cls: type) -> Callable[[object], object]:
    return lambda payload: cls(**payload) if _is_schema(payload) else cls()


def _unwrap(schema: _Schema) -> _Schema:
    # function-after/before/wrap wrap an inner schema; validator bodies are opaque, so generate from the inner constraints.
    return _unwrap(_sub(schema, "schema")) if str(schema.get("type", "")).startswith("function-") else schema


def _pyd_node(schema: _Schema, defs: dict[str, _Schema]) -> st.SearchStrategy[object]:  # noqa: C901, PLR0911, PLR0912, PLR0914, PLR0915  # closed taxonomy: one polymorphic surface over the pydantic-core schema algebra
    """Map one ``pydantic-core`` schema node to a constraint-honoring strategy.

    Reads every machine-readable constraint the node carries (``ge``/``gt``/``le``/``lt``/``multiple_of``,
    ``pattern``/``min_length``/``max_length``, item/key/value sub-schemas, ``decimal_places``, discriminator
    choices) so generated instances pass pydantic validation. ``defs`` threads the ``definition-ref`` table so
    recursive models resolve via ``st.deferred`` without unbounded recursion.

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
                _multiples_int(lo_i if lo_i is not None else -(_CAP * 1000), hi_i if hi_i is not None else _CAP * 1000, mo)
                if isinstance(mo, int)
                else st.integers(min_value=lo_i, max_value=hi_i)
            )
        case "float":
            lo, excl_lo = _fbound(leaf, "ge", "gt")
            hi, excl_hi = _fbound(leaf, "le", "lt")
            mo_f = leaf.get("multiple_of")
            return (
                _multiples_float(lo if lo is not None else -(_CAP * 1000.0), hi if hi is not None else _CAP * 1000.0, mo_f)
                if isinstance(mo_f, int | float)
                else st.floats(min_value=lo, max_value=hi, exclude_min=excl_lo, exclude_max=excl_hi, allow_nan=False, allow_infinity=False)
            )
        case "decimal":
            lo_d, _ = _fbound(leaf, "ge", "gt")
            hi_d, _ = _fbound(leaf, "le", "lt")
            places = leaf.get("decimal_places")
            digit_max = _decimal_max(leaf.get("max_digits"), places)
            return st.decimals(
                min_value=lo_d,
                max_value=hi_d if hi_d is not None else digit_max,
                places=places if isinstance(places, int) else None,
                allow_nan=False,
                allow_infinity=False,
            )
        case "str":
            return _text(leaf.get("min_length"), leaf.get("max_length"), leaf.get("pattern"))
        case "bytes":
            mx_b = leaf.get("max_length")
            return st.binary(max_size=mx_b if isinstance(mx_b, int) else 256)
        case "bool":
            return st.booleans()
        case "none" | "any":
            return st.none()
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
            return st.none()  # url/json/call/custom leaves: no schema-derivable generator → encode-clean None


def _deferred_ref(ref: str, defs: dict[str, _Schema]) -> Callable[[], st.SearchStrategy[object]]:
    return lambda: _pyd_node(defs[ref], defs)


# --- [EXPORTS] ------------------------------------------------------------------------------


def resolve[T](subject: TypeForm[T]) -> st.SearchStrategy[T]:  # noqa: PLR0912  # one polymorphic entry point over the full type-form algebra (alias / pydantic / msgspec / native)
    """Idempotently register a constraint-bounded strategy for ``subject``, then return ``st.from_type(subject)``.

    Total over the type-form algebra (PEP 747 ``TypeForm``): PEP 695 ``type`` aliases unwrap through
    ``__value__`` and recurse; pydantic models walk ``__pydantic_core_schema__`` (honoring pattern /
    multiple_of / nested-item constraints that native ``from_type`` drops); msgspec Struct / dataclass /
    TypedDict / NamedTuple walk the ``msgspec.inspect`` node algebra; everything else (unions, ``Literal``,
    ``Annotated``, enums, leaves, containers) resolves natively through ``from_type``. The ``issubclass``
    probe is guarded behind ``isinstance(subject, type)`` so a non-``type`` form (alias / union / ``Literal``)
    never raises ``TypeError: issubclass() arg 1 must be a class``. Returning ``from_type`` preserves the
    generic ``SearchStrategy[T]`` typing with no ``cast``. The ``_REGISTERED`` guard makes the call idempotent
    and lets self-referential structs resolve through the registry.

    Returns:
        ``st.from_type(subject)`` — a ``SearchStrategy[T]`` producing valid, encode-clean instances.
    """
    if isinstance(subject, TypeAliasType):
        return resolve(subject.__value__)
    if isinstance(subject, type) and subject not in _REGISTERED:
        _REGISTERED.add(subject)
        # issubclass narrowing (not `match`) is required so `model.__pydantic_core_schema__` type-resolves; the
        # subsequent `match` over the msgspec node algebra is the actual variant dispatch.
        if issubclass(subject, pydantic.BaseModel):
            model = subject

            def _pyd_build() -> st.SearchStrategy[object]:
                cs = model.__pydantic_core_schema__
                return _pyd_node(cs, {}) if _is_schema(cs) else st.builds(model)

            st.register_type_strategy(subject, st.deferred(_pyd_build))
        else:
            match _mi.type_info(subject):
                case _mi.StructType(fields=fields) | _mi.DataclassType(fields=fields):
                    struct = subject

                    def _struct_build() -> st.SearchStrategy[object]:
                        return st.builds(struct, **{f.name: _node(f.type) for f in fields})

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


__all__ = ["resolve"]
