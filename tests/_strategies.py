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
import enum
from typing import TYPE_CHECKING

from hypothesis import strategies as st
import msgspec.inspect as _mi
import pydantic


if TYPE_CHECKING:
    from collections.abc import Callable
    from typing import TypeIs


# --- [TYPES] --------------------------------------------------------------------------------

# `__pydantic_core_schema__` is runtime-materialized dict data; modelled as a string-keyed object map and
# narrowed per-access. This is the type-clean boundary shape — no `Any`, no `cast`.
type _Schema = Mapping[str, object]

# --- [CONSTANTS] ----------------------------------------------------------------------------

_REGISTERED: set[type] = set()
_EMPTY: _Schema = {}
_CAP = 64  # collection/string size cap keeps generated payloads encode-cheap

# --- [OPERATIONS] ---------------------------------------------------------------------------
# --- [MSGSPEC_NODE_ALGEBRA] -----------------------------------------------------------------


def _node(node: _mi.Type) -> st.SearchStrategy[object]:  # noqa: C901, PLR0911, PLR0912, PLR0914  # closed taxonomy: one polymorphic surface over the msgspec node algebra
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
            return st.integers(min_value=lo, max_value=hi)
        case _mi.FloatType(ge=ge, gt=gt, le=le, lt=lt):
            lo_f = ge if ge is not None else (gt if gt is not None else 0.0)
            hi_f = le if le is not None else (lt if lt is not None else 1_000_000.0)
            return st.floats(
                min_value=lo_f,
                max_value=hi_f,
                exclude_min=ge is None and gt is not None,
                exclude_max=le is None and lt is not None,
                allow_nan=False,
                allow_infinity=False,
            )
        case _mi.StrType(max_length=cap):
            return st.text(min_size=1, max_size=min(cap or _CAP, _CAP))
        case _mi.BoolType():
            return st.booleans()
        case _mi.BytesType():
            return st.binary(max_size=256)
        case _mi.EnumType(cls=cls):
            return st.sampled_from(list(cls))
        case _mi.LiteralType(values=values):
            return st.sampled_from(list(values))
        case _mi.DateTimeType():
            return st.datetimes(timezones=st.just(dt.UTC))  # msgspec JSON requires tz-aware (RFC 3339) datetimes
        case _mi.DateType():
            return st.dates()
        case _mi.NoneType():
            return st.none()
        case _mi.UnionType(types=types):
            return st.one_of(*(_node(t) for t in types))
        case _mi.VarTupleType(item_type=item):
            return st.lists(_node(item), max_size=3).map(tuple)
        case _mi.TupleType(item_types=items):
            return st.tuples(*(_node(t) for t in items))
        case _mi.ListType(item_type=item):
            return st.lists(_node(item), max_size=3)
        case _mi.SetType(item_type=item) | _mi.FrozenSetType(item_type=item):
            return st.frozensets(_node(item), max_size=3)
        case _mi.DictType(key_type=key, value_type=val):
            return st.dictionaries(_node(key), _node(val), max_size=3)
        case _mi.StructType(cls=cls) | _mi.DataclassType(cls=cls) | _mi.TypedDictType(cls=cls):
            return resolve(cls)
        case _mi.CustomType():
            return st.none()  # JSON-opaque leaves (Callable, Path, …): no stable projection → encode-clean None
        case _:
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
    a = schema.get(incl)
    b = schema.get(excl)
    return (float(a), False) if isinstance(a, int | float) else ((float(b), True) if isinstance(b, int | float) else (None, False))


def _multiple_of(step: int) -> Callable[[int], bool]:
    return lambda v: v % step == 0


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
            base: st.SearchStrategy[int] = st.integers(min_value=_ibound(leaf, "ge", "gt", 1), max_value=_ibound(leaf, "le", "lt", -1))
            mo = leaf.get("multiple_of")
            return base.filter(_multiple_of(mo)) if isinstance(mo, int) else base
        case "float":
            lo, excl_lo = _fbound(leaf, "ge", "gt")
            hi, excl_hi = _fbound(leaf, "le", "lt")
            return st.floats(min_value=lo, max_value=hi, exclude_min=excl_lo, exclude_max=excl_hi, allow_nan=False, allow_infinity=False)
        case "decimal":
            lo_d, _ = _fbound(leaf, "ge", "gt")
            hi_d, _ = _fbound(leaf, "le", "lt")
            places = leaf.get("decimal_places")
            return st.decimals(
                min_value=lo_d, max_value=hi_d, places=places if isinstance(places, int) else None, allow_nan=False, allow_infinity=False
            )
        case "str":
            pat = leaf.get("pattern")
            mn = leaf.get("min_length")
            mx = leaf.get("max_length")
            return (
                st.from_regex(pat, fullmatch=True)
                if isinstance(pat, str)
                else st.text(min_size=mn if isinstance(mn, int) else 1, max_size=mx if isinstance(mx, int) else _CAP)
            )
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


def resolve[T](subject: type[T]) -> st.SearchStrategy[T]:
    """Idempotently register a constraint-bounded strategy for ``subject``, then return ``st.from_type(subject)``.

    pydantic models walk ``__pydantic_core_schema__`` (honoring pattern / multiple_of / nested-item
    constraints that native ``from_type`` drops); msgspec Struct / dataclass / TypedDict walk the
    ``msgspec.inspect`` node algebra; everything else (enums, leaves, containers, unions, Literal) resolves
    natively through ``from_type``. Returning ``from_type`` preserves the generic ``SearchStrategy[T]`` typing
    with no ``cast``. The ``_REGISTERED`` guard (populated before building) makes the call idempotent and lets
    self-referential structs resolve through the registry.

    Returns:
        ``st.from_type(subject)`` — a ``SearchStrategy[T]`` producing valid, encode-clean instances.
    """
    if subject not in _REGISTERED:
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
