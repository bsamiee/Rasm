"""Property tests for constraint-aware Hypothesis strategy construction."""

# --- [IMPORTS] --------------------------------------------------------------------------

from decimal import Decimal
from typing import Annotated, Literal, override

import annotated_types
from expression import case, tag, tagged_union
from hypothesis import given, settings as hyp_settings, strategies as st
import msgspec
import msgspec.msgpack
import pydantic
import pytest

from tests.python.support.strategies import strategy_for

# --- [CONSTANTS] ------------------------------------------------------------------------

_BUDGET = hyp_settings(max_examples=25, deadline=None, database=None)

# --- [MODELS] ---------------------------------------------------------------------------


class Bounded(msgspec.Struct, frozen=True):
    count: Annotated[int, msgspec.Meta(ge=10, le=40, multiple_of=5)]
    label: Annotated[str, msgspec.Meta(min_length=2, max_length=4)]
    ratio: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
    gain: Annotated[float, msgspec.Meta(gt=0.0, le=2.0, multiple_of=0.25)]


class Unbounded(msgspec.Struct, frozen=True):
    count: int
    ratio: float


class Patch(msgspec.Struct, frozen=True, omit_defaults=True):
    note: str | msgspec.UnsetType = msgspec.UNSET


class Node(msgspec.Struct, frozen=True):
    children: tuple["Node", ...] = ()


class TaggedA(msgspec.Struct, tag="a", frozen=True):
    left: Annotated[int, msgspec.Meta(ge=1, le=3)]


class TaggedB(msgspec.Struct, tag="b", frozen=True):
    right: Annotated[int, msgspec.Meta(ge=7, le=9)]


type Either = TaggedA | TaggedB


@tagged_union
class Displacement:
    """Linear-or-angular displacement represented as an expression tagged union."""

    tag: str = tag()
    linear: Annotated[int, msgspec.Meta(ge=1, le=5)] = case()
    angular: float = case()


class _Opaque:
    """Schema-opaque leaf with no msgspec projection; Hypothesis resolves it through the registered strategy."""

    def __init__(self, token: int) -> None:
        self.token = token

    @override
    def __repr__(self) -> str:
        return f"_Opaque({self.token})"


class CustomPayload(msgspec.Struct, frozen=True):
    payload: _Opaque


class ExtensionRecord(msgspec.Struct, frozen=True):
    ext: msgspec.msgpack.Ext


class ValidatedRecord(pydantic.BaseModel):
    count: int = pydantic.Field(ge=3, le=9)
    grade: Literal["low", "high"]
    blob: bytes = pydantic.Field(min_length=2, max_length=6)
    anything: object


class DecimalRecord(pydantic.BaseModel):
    fraction: Annotated[Decimal, annotated_types.Gt(Decimal(0)), annotated_types.Le(Decimal(1)), annotated_types.MultipleOf(Decimal("0.05"))]
    offset: Decimal = pydantic.Field(ge=Decimal("-2.5"), lt=Decimal("2.5"), decimal_places=2)
    quantity: Decimal = pydantic.Field(max_digits=3)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MSGSPEC_STRATEGIES]


@_BUDGET
@given(strategy_for(Bounded))
def test_msgspec_constraints_hold_and_encode_successfully(value: Bounded) -> None:
    """Every draw satisfies Meta bounds, multiples, exclusive bounds, and lengths and passes the C validator unchanged."""
    assert 10 <= value.count <= 40 and value.count % 5 == 0, f"count constraint failed: {value.count}"
    assert 2 <= len(value.label) <= 4, f"label-length constraint failed: {value.label!r}"
    assert 0.0 <= value.ratio <= 1.0, f"ratio bound failed: {value.ratio}"
    assert 0.0 < value.gain <= 2.0 and (value.gain / 0.25).is_integer(), f"gain bound or multiple constraint failed: {value.gain}"
    assert msgspec.json.decode(msgspec.json.encode(value), type=Bounded) == value


@_BUDGET
@given(strategy_for(Patch).filter(lambda patch: patch.note is msgspec.UNSET), strategy_for(Patch).filter(lambda patch: not isinstance(patch.note, msgspec.UnsetType)))
def test_defaulted_fields_sample_presence_and_absence(absent: Patch, present: Patch) -> None:
    """Generated ``UNSET`` fields cover omission and presence, and only present values are encoded."""
    assert msgspec.json.encode(absent) == b"{}", f"omitted field was serialized: {absent!r}"
    assert msgspec.json.encode(present) != b"{}", f"present field was not serialized: {present!r}"


@_BUDGET
@given(strategy_for(Unbounded).filter(lambda value: value.count < 0 and value.ratio < 0.0), strategy_for(Unbounded).filter(lambda value: value.count > 0 and value.ratio > 0.0))
def test_unconstrained_numerics_reach_both_signs(negative: Unbounded, positive: Unbounded) -> None:
    """Unconstrained integers and floats generate negative and positive values."""
    assert (negative.count < 0, positive.count > 0) == (True, True)


@_BUDGET
@given(strategy_for(Node))
def test_recursive_struct_resolves_boundedly(node: Node) -> None:
    """A self-nesting struct draws through the deferred registry without recursion failure."""
    assert isinstance(node.children, tuple)


# --- [TYPE_FORM_STRATEGIES]


@_BUDGET
@given(strategy_for(Annotated[int, msgspec.Meta(ge=10, le=12)]))
def test_annotated_form_carries_its_constraints(value: int) -> None:
    """A bare Annotated form generates inside its Meta bounds, never the unconstrained base."""
    assert 10 <= value <= 12, f"Annotated Meta constraint ignored: {value}"


@_BUDGET
@given(strategy_for(Either))
def test_alias_union_generates_members_satisfying_constraints(value: TaggedA | TaggedB) -> None:
    """A PEP 695 alias over a tagged union draws both members with their field constraints intact."""
    match value:
        case TaggedA(left=left):
            assert 1 <= left <= 3, f"TaggedA constraint failed: {left}"
        case TaggedB(right=right):
            assert 7 <= right <= 9, f"TaggedB constraint failed: {right}"


@_BUDGET
@given(strategy_for(Either).filter(lambda value: isinstance(value, TaggedA)), strategy_for(Either).filter(lambda value: isinstance(value, TaggedB)))
def test_union_reaches_every_member(first: TaggedA, second: TaggedB) -> None:
    """Generated values reach both union members."""
    assert (isinstance(first, TaggedA), isinstance(second, TaggedB)) == (True, True)


@_BUDGET
@given(strategy_for(Literal["on", "off"]))
def test_literal_form_generates_only_declared_values(value: str) -> None:
    """A Literal form draws only its declared members."""
    assert value in {"on", "off"}, f"literal was outside its declared values: {value!r}"


# --- [TAGGED_UNION_AND_CUSTOM_TYPE_STRATEGIES]


@_BUDGET
@given(strategy_for(Displacement))
def test_expression_tagged_union_draws_exactly_one_constrained_case(value: Displacement) -> None:
    """Every draw uses the one-case constructor and satisfies its case constraint; direct field sampling is invalid."""
    match value.tag:
        case "linear":
            assert 1 <= value.linear <= 5, f"case constraint failed: {value.linear}"
        case "angular":
            assert isinstance(value.angular, float)
        case unknown:
            pytest.fail(f"drawn union has an unknown tag: {unknown!r}")


@_BUDGET
@given(strategy_for(Displacement).filter(lambda value: value.tag == "linear"), strategy_for(Displacement).filter(lambda value: value.tag == "angular"))
def test_expression_tagged_union_reaches_every_case(first: Displacement, second: Displacement) -> None:
    """Generated values reach both tagged-union cases."""
    assert (first.tag, second.tag) == ("linear", "angular")


st.register_type_strategy(_Opaque, st.integers(min_value=1, max_value=9).map(_Opaque))


@_BUDGET
@given(strategy_for(CustomPayload))
def test_custom_type_uses_the_hypothesis_registry(value: CustomPayload) -> None:
    """A type without a msgspec schema uses its registered Hypothesis strategy."""
    assert isinstance(value.payload, _Opaque) and 1 <= value.payload.token <= 9, f"registered custom strategy ignored: {value.payload!r}"


@_BUDGET
@given(strategy_for(ExtensionRecord))
def test_ext_leaf_generates_valid_msgpack_extensions(value: ExtensionRecord) -> None:
    """Ext fields generate tagged extension payloads that round-trip through the MessagePack codec."""
    assert 0 <= value.ext.code <= 127, f"ext code was outside the custom range: {value.ext.code}"
    assert msgspec.msgpack.decode(msgspec.msgpack.encode(value), type=ExtensionRecord) == value


# --- [PYDANTIC_STRATEGIES]


@_BUDGET
@given(strategy_for(ValidatedRecord))
def test_pydantic_constraints_hold_on_every_draw(model: ValidatedRecord) -> None:
    """Generated values satisfy the core schema's field bounds, literal values, and byte-length constraints."""
    assert 3 <= model.count <= 9, f"count bound failed: {model.count}"
    assert model.grade in {"low", "high"}, f"grade was outside its declared values: {model.grade!r}"
    assert 2 <= len(model.blob) <= 6, f"blob-length constraint failed: {model.blob!r}"


@_BUDGET
@given(strategy_for(ValidatedRecord).filter(lambda model: model.anything is not None))
def test_pydantic_any_schema_generates_non_none_values(model: ValidatedRecord) -> None:
    """The ``any`` schema generates JSON-compatible values beyond ``None``."""
    assert model.anything is not None


@_BUDGET
@given(strategy_for(DecimalRecord))
def test_pydantic_decimal_strategy_satisfies_exclusivity_multiples_and_digit_limits(model: DecimalRecord) -> None:
    """Every decimal draw passes live pydantic validation: excluded bounds never appear, and multiples and digit limits hold."""
    assert Decimal(0) < model.fraction <= Decimal(1) and model.fraction % Decimal("0.05") == 0, f"fraction multiple constraint failed: {model.fraction}"
    assert Decimal("-2.5") <= model.offset < Decimal("2.5"), f"offset bound failed: {model.offset}"
    assert abs(model.quantity) <= Decimal(999), f"quantity digit limit failed: {model.quantity}"
