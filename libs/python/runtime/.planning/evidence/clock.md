# [PY_RUNTIME_CLOCK]

One logical-time owner serves the whole branch: the `Hlc` two-half cell, the sign-carrying `Ordering` verdict, the content-stable `ElementId`, the `Tenant` partition, the `SLOTS` slot/attribute vocabulary table, and the `CausalFrame` inbound frame. This owner stamps the branch's own causal frames under the `hlc-two-half` corpus layout — physical half first, logical half second — so it mints the layout in its own types rather than reading a peer's, decodes an inbound stamp on the `reliability/faults#FAULT` rail, and holds the compare/merge/successor algebra interior code reads; parity against `dotnet:Rasm/Domain/frame#RECEIPT_PORT` and the typescript peer is the conformance, never a mint ranking.

Carriage is per-branch and only the layout and the kernel-owned attribute slots are shared. `packed` COMPOSES the kernel `Hlc` layout — `physical_ticks << 64 | logical` as one UInt128, bit-identical — and the dotted `rasm.tenant` attribute COMPOSES the kernel `TenantContext.TenantSlot` spelling rather than re-minting it, so both halves of the stamp answer one estate law. The hyphenated `SLOTS` carrier keys are this branch's own transport dialect and bind no peer, exactly as the C# stamp rides its receipt envelope and the typescript stamp rides typed `-bin` metadata. `sealed` is that drift gate and it proves the SHARED half alone — the packed layout arithmetic and the composed attribute slots, whose shared-law column is `[CAUSAL_CARRIAGE]` at `dotnet:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` — reading no peer header spelling at all, because freezing one branch's transport dialect as estate law is what the co-equal minters the `hlc-two-half` roster names exist to foreclose.

`CausalFrame.decode` is the canonical inbound carrier reader `transport/serve#SERVE` `Admission.on_start` folds inside this owner's one `boundary(CLOCK_CARRIER, ...)` fence, answering `RuntimeRail[Option[Self]]` so ABSENCE and DRIFT stay two answers: a call carrying no causal headers admits as `Nothing` and a present stamp proves whole or refuses. `CausalFrame.attributes` is the canonical projection `execution/admission#CONTEXT` `RuntimeContext.attribute` and the serve enricher compose — consumers select a shape and re-spell nothing, so the two attribute layouts cannot drift. Admission's context threads the inbound carry as `Option[CausalFrame]` (`Nothing` locally minted, `Some(frame)` the host stamp), and the two-half pack/unpack layout rides the `evidence/reproduction#SEED_REPRODUCTION` `HLC_TWO_HALF` design pin, a value-level layout distinct from a byte serialization.

## [01]-[INDEX]

- [02]-[CLOCK]: `Hlc` cell, `Ordering` verdict, `ElementId`, `Tenant`, `SLOTS` table, the railed `CausalFrame` decode/attributes pair, and the `sealed` boot gate over the shared layout, on one owner.

## [02]-[CLOCK]

- Owner: `Hlc` — the two-half cell bit-identical to C# `Hlc`: `physical_ticks` the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count, `logical` the per-node `ulong` counter, `packed` mirroring the C# `Hlc.ToPacked` UInt128 layout so a stamp reconstructs without a field-order guess; the `order=True` synthesis IS the physical-dominant causal order, so `compare`/`merge`/`tick` share one synthesized comparison and `merge` is `max`, never a hand-branch. `Ordering` — the behavior-bearing verdict whose case payload pins the C# `Hlc.CompareTo` sign at the type level, `fold` the one dispatch so the causal symmetry `a.compare(b) == b.compare(a).reverse()` is one method and no consumer re-spells a match. `ElementId` — the `(origin, logical)` identity the CRDT RGA and OR-set address by (`origin` the C# `OpLog` origin guid bytes), never a positional index, the synthesized order replacing any hand sort of the tag set. `Tenant` — the one partition newtype the serve `CommandArguments.tenant` and the inbound slot both absorb into, with `ROOT_TENANT` the untagged whole a stamp carrying no tenant slot reads as. `SLOTS` — the one slot/attribute vocabulary table, so no consumer carries a scattered header or key literal. `sealed` — the boot gate over the shared layout and the composed attribute spellings.
- Entry: `CausalFrame.of` lifts an already-domain-valid pair — the generated `FaultDetail` causal fields `transport/serve#CAPABILITY_INVOKE` decodes feed it after that seam validates — and `decode` never routes through it, because a raw carrier gains its `Meta` domain check only at `convert`. `decode` keys presence on the PHYSICAL slot alone: that slot absent is a locally-minted call and answers `Ok(Nothing)`, where folding it onto a zero-filled frame published `Hlc(0, 0)` as a legitimate epoch stamp no consumer could tell from a peer that really stamped the epoch. A present stamp whose tenant slot is absent reads as `ROOT_TENANT` rather than as an unknown partition — the absent slot is a peer's REFUSAL to adopt a wire tenancy and every RLS predicate, receipt, and meter tag on that side already answers root for it.
- Entry: `attributes` selects by what the consumer stamps, one rule and no second map — a consumer folding ONE ordering slot takes `packed`, which renders the 128-bit cell as the `rasm.hlc` hex STRING an exporter admits, and a consumer stamping the two halves as their own native-int dimensions takes `halves`. Either way the render is causal-ordering evidence the `execution/admission#CONTEXT` projection folds beside the W3C ids, never a trace or span identity, which admission adopts from the propagator alone.
- Packages: `msgspec` — `gc=False` only on the leaf cells holding no container field; `CausalFrame` stays GC-tracked because it holds a struct reference, and `Ordering` carries neither `gc` nor `order=True` since a verdict is dispatched, never sorted; `U64`/`U128` are this owner's wire-domain aliases — the msgspec int64-constraint law caps every owner at the `ge=0` floor, each declaring its own ceiling check on its own domain — and `I63` is the clock-only tightening whose domain fits `le=2**63 - 1`. `opentelemetry-api` — the `Span.set_attributes` attribute-map shape only, API and never the SDK.
- Growth: a new clock dimension is one `Hlc` field the synthesized order folds and one `attributes` key; a new identity axis one `ElementId` field; a new frame dimension one `CausalFrame` column reachable through `decode`/`attributes`; a new slot or attribute key one `SLOTS` row the gate's own injectivity and grammar arms absorb; a new attribute layout one `AttrShape` arm, never a per-consumer map; a new layout assertion one `_LAYOUT_CELLS` row, never a second gate; a new comparison outcome is impossible — three is the closed `Ordering` set, and a new consumer behavior is one `fold` call site.
- Boundary: `sealed` proves the SHARED half and stops — layout arithmetic and the composed attribute grammar — so no arm reads a C# carrier header spelling and none can, three transport dialects riding one layout by ruling; a gate blocked on a peer minting this branch's four header keys is mis-aimed and never re-arms. `tick` mints the companion's derived presence beat strictly after every cause seen — purely logical, never the host physical mint. `transport/wire`'s codec reconstructs `Hlc`/`ElementId` from decoded op arms, admission carries the frame, and the serve enricher folds admission's projection, so the clock lives in one place. `merge`/`tick` are the join-semilattice and successor the op-log prefix replay converges through without double-counting a duplicate op, and the `transport/wire#CRDT_STATE` `converged` fold's `LwwRegister.absorbed` survivor decision reads `compare` through one `fold` call site, never a re-derived sign comparison at the adjudication seam.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from typing import Annotated, Final, Literal, NewType, Self, assert_never

from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Meta, Struct, ValidationError, convert

from rasm.runtime.faults import CLOCK_CARRIER, CLOCK_LAYOUT, CLOCK_SEALED, RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

Tenant = NewType("Tenant", str)
type Slot = Literal["physical", "logical", "tenant", "packed"]
type AttrShape = Literal["halves", "packed"]
_I63_MAX: Final[int] = 2**63 - 1
_U64_MAX: Final[int] = (1 << 64) - 1
type I63 = Annotated[int, Meta(ge=0, le=_I63_MAX)]
type U64 = Annotated[int, Meta(ge=0)]
type U128 = Annotated[int, Meta(ge=0)]


@tagged_union(frozen=True)
class Ordering:
    tag: Literal["before", "equal", "after"] = tag()
    before: Literal[-1] = case()
    equal: Literal[0] = case()
    after: Literal[1] = case()

    @classmethod
    def of_sign(cls, sign: int, /) -> Self:
        return cls(before=-1) if sign < 0 else cls(after=1) if sign > 0 else cls(equal=0)

    def fold[T](self, *, before: Callable[[], T], equal: Callable[[], T], after: Callable[[], T]) -> T:
        match self.tag:
            case "before":
                return before()
            case "equal":
                return equal()
            case "after":
                return after()
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def sign(self) -> Literal[-1, 0, 1]:
        return self.fold(before=lambda: -1, equal=lambda: 0, after=lambda: 1)

    def reverse(self) -> Ordering:
        return self.fold(before=lambda: Ordering(after=1), equal=lambda: self, after=lambda: Ordering(before=-1))


# --- [CONSTANTS] ------------------------------------------------------------------------

ROOT_TENANT: Final[Tenant] = Tenant("root")

_TENANT_ATTR: Final[str] = "rasm.tenant"
_HLC_ROOT: Final[str] = "rasm.hlc"

_LAYOUT_CELLS: Final[Block[tuple[tuple[int, int], int]]] = Block.of_seq([
    ((0, 0), 0),
    ((0, _U64_MAX), _U64_MAX),
    ((1, 0), 1 << 64),
    ((1 << 62, 1 << 32), (1 << 126) | (1 << 32)),
    ((_I63_MAX, _U64_MAX), (1 << 127) - 1),
])

_SHAPES: Final[Block[AttrShape]] = Block.of_seq(["halves", "packed"])

# --- [MODELS] ---------------------------------------------------------------------------


class Hlc(Struct, frozen=True, order=True, gc=False):
    physical_ticks: I63
    logical: U64

    @property
    def packed(self) -> U128:
        return (self.physical_ticks << 64) | self.logical

    @classmethod
    def of_packed(cls, packed: U128, /) -> Self:
        if packed < 0:
            raise ValidationError(f"packed value {packed} below the U128 wire domain")
        if (physical := packed >> 64) > _I63_MAX:
            raise ValidationError(f"packed physical half {physical} exceeds the I63 mint domain")
        return cls(physical_ticks=physical, logical=packed & _U64_MAX)

    def compare(self, other: Self, /) -> Ordering:
        return Ordering.of_sign(0 if self == other else -1 if self < other else 1)

    def tick(self, observed: Self, /) -> Self:
        ceiling = max(self, observed)
        if ceiling.logical < _U64_MAX:
            return type(self)(ceiling.physical_ticks, ceiling.logical + 1)
        if ceiling.physical_ticks >= _I63_MAX:
            raise ValidationError(f"hlc exhausted: physical {ceiling.physical_ticks} at the I63 ceiling with the logical half saturated")
        return type(self)(ceiling.physical_ticks + 1, 0)

    @staticmethod
    def merge(left: Hlc, right: Hlc, /) -> Hlc:
        return max(left, right)


class ElementId(Struct, frozen=True, order=True, gc=False):
    origin: Annotated[bytes, Meta(min_length=16, max_length=16)]
    logical: U64


class CausalFrame(Struct, frozen=True):
    hlc: Hlc
    tenant: Tenant

    @classmethod
    def of(cls, hlc_physical: I63, hlc_logical: U64, tenant: str) -> Self:
        return cls(hlc=Hlc(hlc_physical, hlc_logical), tenant=Tenant(tenant))

    @classmethod
    def decode(cls, carrier: Mapping[str, str]) -> RuntimeRail[Option[Self]]:
        def stamped(physical: str) -> Self:
            frame = convert(
                {
                    "hlc": {"physical_ticks": physical, "logical": carrier.get(SLOTS["logical"][0], "0")},
                    "tenant": carrier.get(SLOTS["tenant"][0], ROOT_TENANT),
                },
                cls,
                strict=False,
            )
            if frame.hlc.logical > _U64_MAX:
                raise ValidationError(f"logical {frame.hlc.logical} exceeds the u64 wire domain")
            return frame

        return boundary(
            CLOCK_CARRIER, lambda: Option.of_optional(carrier.get(SLOTS["physical"][0])).map(stamped), catch=(ValidationError, ValueError)
        )

    def attributes(self, shape: AttrShape = "packed") -> dict[str, str | int]:
        tenant = {SLOTS["tenant"][1]: self.tenant}
        match shape:
            case "halves":
                return tenant | {SLOTS["physical"][1]: self.hlc.physical_ticks, SLOTS["logical"][1]: self.hlc.logical}
            case "packed":
                return tenant | {SLOTS["packed"][1]: format(self.hlc.packed, "032x")}
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

SLOTS: Final[Map[Slot, tuple[str, str]]] = Map.of_seq([
    ("physical", ("rasm-hlc-physical", "rasm.hlc.physical")),
    ("logical", ("rasm-hlc-logical", "rasm.hlc.logical")),
    ("tenant", ("rasm-tenant", "rasm.tenant")),
    ("packed", ("rasm-hlc", "rasm.hlc")),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _layout(halves: tuple[int, int], packed: int) -> Block[str]:
    cell = Hlc(*halves)
    return (
        Block.singleton(f"{halves}:packed-{cell.packed:#x}-past-{packed:#x}")
        if cell.packed != packed
        else Block.empty()
        if (split := Hlc.of_packed(packed)) == cell
        else Block.singleton(f"{halves}:round-trip-{split.physical_ticks}-{split.logical}")
    )


def _composed(attribute: str) -> Option[str]:
    return (
        Nothing
        if attribute in (_TENANT_ATTR, _HLC_ROOT) or attribute.startswith(f"{_HLC_ROOT}.")
        else Some(f"{attribute}:attribute-outside-the-composed-slots")
    )


def sealed() -> RuntimeRail[int]:
    def proved() -> Block[str]:
        probe = CausalFrame(hlc=Hlc(_I63_MAX, _U64_MAX), tenant=ROOT_TENANT)
        rendered = _SHAPES.collect(lambda shape: Block.of_seq(sorted(probe.attributes(shape))))
        columns = Block.of_seq([
            ("carrier", tuple(carrier for carrier, _ in SLOTS.values())),
            ("attribute", tuple(attribute for _, attribute in SLOTS.values())),
        ])
        collided = columns.choose(
            lambda column: Nothing if len(frozenset(column[1])) == len(column[1]) else Some(f"slots.{column[0]}:keys-collided")
        )
        return _LAYOUT_CELLS.collect(lambda row: _layout(*row)).append(collided).append(rendered.choose(_composed))

    return boundary(CLOCK_SEALED, proved, catch=(KeyError, IndexError)).bind(
        lambda drift: Ok(len(_LAYOUT_CELLS) + len(SLOTS))
        if drift.is_empty()
        else Error(CLOCK_LAYOUT.raised(";".join(drift)))
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
