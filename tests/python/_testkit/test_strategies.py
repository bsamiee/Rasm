"""Resolver falsification laws: constraint honoring, omission lanes, and the type-form algebra."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from decimal import Decimal
from typing import Annotated, Literal, override

import annotated_types
from expression import case, tag, tagged_union
from hypothesis import given, settings as hyp_settings, strategies as st
import msgspec
import msgspec.msgpack
import pydantic
import pytest

from tests.python._testkit.strategies import resolve


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
class Axis:
    """Expression tagged union: the constructor accepts exactly one case keyword."""

    tag: str = tag()
    linear: Annotated[int, msgspec.Meta(ge=1, le=5)] = case()
    angular: float = case()


class _Opaque:
    """Schema-opaque leaf with no msgspec projection; resolution rides hypothesis's registry."""

    def __init__(self, token: int) -> None:
        self.token = token

    @override
    def __repr__(self) -> str:
        return f"_Opaque({self.token})"


class Carrier(msgspec.Struct, frozen=True):
    payload: _Opaque


class Framed(msgspec.Struct, frozen=True):
    ext: msgspec.msgpack.Ext


class Model(pydantic.BaseModel):
    count: int = pydantic.Field(ge=3, le=9)
    grade: Literal["low", "high"]
    blob: bytes = pydantic.Field(min_length=2, max_length=6)
    anything: object


class Priced(pydantic.BaseModel):
    # annotated-types spellings keep the Decimal multiple exact; Field's multiple_of stub admits only int | float.
    cost: Annotated[Decimal, annotated_types.Gt(Decimal(0)), annotated_types.Le(Decimal(1)), annotated_types.MultipleOf(Decimal("0.05"))]
    scale: Decimal = pydantic.Field(ge=Decimal("-2.5"), lt=Decimal("2.5"), decimal_places=2)
    budget: Decimal = pydantic.Field(max_digits=3)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MSGSPEC_ALGEBRA]


@_BUDGET
@given(resolve(Bounded))
def test_msgspec_constraints_hold_and_encode_clean(value: Bounded) -> None:
    """Every draw honors Meta bounds, multiples, exclusive edges, and lengths, and decodes through the C validator without repair."""
    assert 10 <= value.count <= 40 and value.count % 5 == 0, f"count constraint broke: {value.count}"
    assert 2 <= len(value.label) <= 4, f"label length broke: {value.label!r}"
    assert 0.0 <= value.ratio <= 1.0, f"ratio bound broke: {value.ratio}"
    # gt + multiple_of: the exclusive boundary is itself a multiple, so a naive k-window draws it.
    assert 0.0 < value.gain <= 2.0 and (value.gain / 0.25).is_integer(), f"gain exclusive-multiple broke: {value.gain}"
    assert msgspec.json.decode(msgspec.json.encode(value), type=Bounded) == value


@_BUDGET
@given(
    resolve(Patch).filter(lambda patch: patch.note is msgspec.UNSET),
    resolve(Patch).filter(lambda patch: not isinstance(patch.note, msgspec.UnsetType)),
)
def test_defaulted_fields_sample_presence_and_absence(absent: Patch, present: Patch) -> None:
    """The UNSET wire lane generates: omission and presence are both reachable, and only presence hits the wire.

    An unreachable lane exhausts its filter and fails the law as Unsatisfiable — the falsification arm.
    """
    assert msgspec.json.encode(absent) == b"{}", f"omitted field leaked onto the wire: {absent!r}"
    assert msgspec.json.encode(present) != b"{}", f"present field vanished from the wire: {present!r}"


@_BUDGET
@given(
    resolve(Unbounded).filter(lambda value: value.count < 0 and value.ratio < 0.0),
    resolve(Unbounded).filter(lambda value: value.count > 0 and value.ratio > 0.0),
)
def test_unconstrained_numerics_reach_both_signs(negative: Unbounded, positive: Unbounded) -> None:
    """Unconstrained int and float fields generate on both sides of zero — a one-sided lane exhausts its filter and fails as Unsatisfiable."""
    assert (negative.count < 0, positive.count > 0) == (True, True)


@_BUDGET
@given(resolve(Node))
def test_recursive_struct_resolves_boundedly(node: Node) -> None:
    """A self-nesting struct draws through the deferred registry without recursion failure."""
    assert isinstance(node.children, tuple)


# --- [TYPE_FORM_ALGEBRA]


@_BUDGET
@given(resolve(Annotated[int, msgspec.Meta(ge=10, le=12)]))
def test_annotated_form_carries_its_constraints(value: int) -> None:
    """A bare Annotated form generates inside its Meta bounds, never the unconstrained base."""
    assert 10 <= value <= 12, f"Annotated Meta constraint ignored: {value}"


@_BUDGET
@given(resolve(Either))
def test_alias_union_generates_constraint_true_members(value: TaggedA | TaggedB) -> None:
    """A PEP 695 alias over a tagged union draws both members with their field constraints intact."""
    match value:
        case TaggedA(left=left):
            assert 1 <= left <= 3, f"TaggedA constraint broke: {left}"
        case TaggedB(right=right):
            assert 7 <= right <= 9, f"TaggedB constraint broke: {right}"


@_BUDGET
@given(resolve(Either).filter(lambda v: isinstance(v, TaggedA)), resolve(Either).filter(lambda v: isinstance(v, TaggedB)))
def test_union_reaches_every_member(first: TaggedA, second: TaggedB) -> None:
    """Both union members are reachable draws, so the union lane is never a single-arm slice."""
    assert (isinstance(first, TaggedA), isinstance(second, TaggedB)) == (True, True)


@_BUDGET
@given(resolve(Literal["on", "off"]))
def test_literal_form_stays_inside_its_vocabulary(value: str) -> None:
    """A Literal form draws only its declared members."""
    assert value in {"on", "off"}, f"literal escaped its vocabulary: {value!r}"


# --- [EXPRESSION_AND_OPAQUE_ALGEBRA]


@_BUDGET
@given(resolve(Axis))
def test_expression_tagged_union_draws_exactly_one_constrained_case(value: Axis) -> None:
    """Every draw constructs through the one-case constructor with its case constraint intact; field-wise sampling refuses at build."""
    match value.tag:
        case "linear":
            assert 1 <= value.linear <= 5, f"case constraint broke: {value.linear}"
        case "angular":
            assert isinstance(value.angular, float)
        case unknown:
            pytest.fail(f"drawn union carries an unknown tag: {unknown!r}")


@_BUDGET
@given(resolve(Axis).filter(lambda v: v.tag == "linear"), resolve(Axis).filter(lambda v: v.tag == "angular"))
def test_expression_tagged_union_reaches_every_case(first: Axis, second: Axis) -> None:
    """Both cases are reachable draws — a single-arm slice exhausts its filter and fails as Unsatisfiable."""
    assert (first.tag, second.tag) == ("linear", "angular")


st.register_type_strategy(_Opaque, st.integers(min_value=1, max_value=9).map(_Opaque))


@_BUDGET
@given(resolve(Carrier))
def test_custom_leaf_resolves_through_the_hypothesis_registry(value: Carrier) -> None:
    """A schema-opaque field routes to ``st.from_type``, so the suite-registered strategy owns it."""
    assert isinstance(value.payload, _Opaque) and 1 <= value.payload.token <= 9, f"registered custom strategy ignored: {value.payload!r}"


@_BUDGET
@given(resolve(Framed))
def test_ext_leaf_generates_wire_valid_msgpack_extensions(value: Framed) -> None:
    """Ext fields draw real tagged extension payloads that survive the MessagePack codec."""
    assert 0 <= value.ext.code <= 127, f"ext code escaped the custom band: {value.ext.code}"
    assert msgspec.msgpack.decode(msgspec.msgpack.encode(value), type=Framed) == value


# --- [PYDANTIC_ALGEBRA]


@_BUDGET
@given(resolve(Model))
def test_pydantic_constraints_hold_on_every_draw(model: Model) -> None:
    """Field bounds, literal vocabularies, and bytes lengths all honor the core schema — an escaping draw fails here."""
    assert 3 <= model.count <= 9, f"count bound broke: {model.count}"
    assert model.grade in {"low", "high"}, f"grade escaped: {model.grade!r}"
    assert 2 <= len(model.blob) <= 6, f"blob length broke: {model.blob!r}"


@_BUDGET
@given(resolve(Model).filter(lambda model: model.anything is not None))
def test_pydantic_any_lane_reaches_beyond_none(model: Model) -> None:
    """The ``any`` schema lane generates real JSON values, never a None-only slice."""
    assert model.anything is not None


@_BUDGET
@given(resolve(Priced))
def test_pydantic_decimal_lane_honors_exclusivity_multiples_and_digit_budgets(model: Priced) -> None:
    """Every decimal draw constructs through live pydantic validation: exclusive edges never land, multiples and digit budgets hold."""
    assert Decimal(0) < model.cost <= Decimal(1) and model.cost % Decimal("0.05") == 0, f"cost exclusive-multiple broke: {model.cost}"
    assert Decimal("-2.5") <= model.scale < Decimal("2.5"), f"scale exclusive edge broke: {model.scale}"
    assert abs(model.budget) <= Decimal(999), f"budget digit ceiling broke: {model.budget}"
