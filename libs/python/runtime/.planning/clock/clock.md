# [PY_RUNTIME_CLOCK]

One logical-time owner serves the whole branch: the `Hlc` two-half cell, the sign-carrying `Ordering` verdict, the content-stable `ElementId`, the `Tenant` partition, the `SLOTS` slot/attribute vocabulary table, and the `CausalFrame` inbound frame. No wall-clock stamp mints here — the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` port is the sole host mint — so this owner decodes the inbound stamp on the `reliability/faults#FAULT` rail, holds the compare/merge/successor algebra interior code reads, and re-mints nothing.

`CausalFrame.decode` is the canonical inbound carrier reader `transport/serve#SERVE` `ServerHost.inbound` folds inside this owner's one `boundary("wire", ...)` fence, and `CausalFrame.attributes` the canonical projection `execution/admission#CONTEXT` `RuntimeContext.attribute` and the serve enricher compose — consumers select a shape and re-spell nothing, so the two attribute layouts cannot drift. Admission's context threads the inbound carry as `Option[CausalFrame]` (`Nothing` locally minted, `Some(frame)` the host stamp), and the two-half pack/unpack layout rides the `evidence/reproduction#SEED_REPRODUCTION` `HLC_TWO_HALF` design pin, a value-level layout distinct from a byte serialization.

## [01]-[INDEX]

- [01]-[CLOCK]: the `Hlc` cell, `Ordering` verdict, `ElementId`, `Tenant`, the `SLOTS` table, and the railed `CausalFrame` decode/attributes pair on one owner.

## [02]-[CLOCK]

- Owner: `Hlc` — the two-half cell bit-identical to C# `Hlc`: `physical_ticks` the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count, `logical` the per-node `ulong` counter, `packed` mirroring the C# `Hlc.ToPacked` UInt128 layout so a stamp reconstructs without a field-order guess; the `order=True` synthesis IS the physical-dominant causal order, so `compare`/`merge`/`tick` share one synthesized comparison and `merge` is `max`, never a hand-branch. `Ordering` — the behavior-bearing verdict whose case payload pins the C# `Hlc.CompareTo` sign at the type level, `fold` the one dispatch so the causal symmetry `a.compare(b) == b.compare(a).reverse()` is one method and no consumer re-spells a match. `ElementId` — the `(origin, logical)` identity the CRDT RGA and OR-set address by (`origin` the C# `OpLog` origin guid bytes), never a positional index, the synthesized order replacing any hand sort of the tag set. `Tenant` — the one partition newtype the serve `CommandArguments.tenant` and the inbound slot both absorb into. `SLOTS` — the one slot/attribute vocabulary table, so no consumer carries a scattered header or key literal.
- Entry: `CausalFrame.of` lifts an already-domain-valid pair — the `transport/wire#PROTO_TRANSCODE`-decoded `FaultDetail` causal fields feed it after the transcode seam validates — and `decode` never routes through it, because a raw carrier gains its `Meta` domain check only at `convert`. `attributes("packed")` renders hex symmetric with `Correlation.trace_id.hex()`, so `Correlation.seed` un-hexes one spelling for the 16-byte parent id.
- Packages: `msgspec` — `gc=False` only on the leaf cells holding no container field; `CausalFrame` stays GC-tracked because it holds a struct reference, and `Ordering` carries neither `gc` nor `order=True` since a verdict is dispatched, never sorted; `U64`/`U128` share the `ge=0` floors the `evidence/identity#IDENTITY` aliases declare, `I63` the clock-only tightening whose domain fits `le=2**63 - 1`. `opentelemetry-api` — the `Span.set_attributes` attribute-map shape only, API and never the SDK.
- Growth: a new clock dimension is one `Hlc` field the synthesized order folds plus one `attributes` key; a new identity axis one `ElementId` field; a new frame dimension one `CausalFrame` column reachable through `decode`/`attributes`; a new slot or attribute key one `SLOTS` row; a new attribute layout one `AttrShape` arm, never a per-consumer map; a new comparison outcome is impossible — three is the closed `Ordering` set, and a new consumer behavior is one `fold` call site.
- Boundary: `tick` mints the companion's derived presence beat strictly after every cause seen — purely logical, never the host physical mint. `transport/wire`'s codec reconstructs `Hlc`/`ElementId` from decoded op arms, admission carries the frame, and the serve enricher folds admission's projection, so the clock lives in one place. `merge`/`tick` are the join-semilattice and successor the op-log prefix replay converges through without double-counting a duplicate op, and the `transport/wire#CRDT_DECODE` `LwwRegister` survivor decision reads `compare` through one `fold` call site, never a re-derived sign comparison at the adjudication seam.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Annotated, Final, Literal, NewType, Self, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Meta, Struct, ValidationError, convert

from rasm.runtime.faults import RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

Tenant = NewType("Tenant", str)  # NewType is its own constructor; never a PEP 695 `type` alias (a TypeAliasType is not callable)
type Slot = Literal["physical", "logical", "tenant", "packed"]
type AttrShape = Literal["halves", "packed"]
# msgspec's C core rejects any integer bound past int64 at constraint build, so I63 rides `le=_I63_MAX`, U64 carries only the `ge=0`
# floor (its <2**64 ceiling is the explicit `decode`/`of_packed` gate), and U128's ceiling is structural — `packed` shifts two gated halves.
_I63_MAX: Final[int] = 2**63 - 1  # the C# physical mint ceiling — non-negative NodaTime Int64 ticks, also the OTLP signed-int64 ceiling
_U64_MAX: Final[int] = (1 << 64) - 1  # the logical-half wire ceiling msgspec cannot express (int64-max constraint law)
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
        # one behavior-dispatch surface keyed on `tag`; `sign`/`reverse` are folds, never parallel matches.
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
        # declared return type is the inference context solving `T` to the literal union — an explicit `fold[...]` specialization
        # is a runtime `TypeError`, since a function object is not subscriptable.
        return self.fold(before=lambda: -1, equal=lambda: 0, after=lambda: 1)

    def reverse(self) -> Ordering:
        # `Ordering`, not `Self`: the `before`/`after` arms construct the sealed union directly.
        return self.fold(before=lambda: Ordering(after=1), equal=lambda: self, after=lambda: Ordering(before=-1))


# --- [MODELS] ---------------------------------------------------------------------------


class Hlc(Struct, frozen=True, order=True, gc=False):
    physical_ticks: I63
    logical: U64

    @property
    def packed(self) -> U128:
        return (self.physical_ticks << 64) | self.logical

    @classmethod
    def of_packed(cls, packed: U128, /) -> Self:
        # the shift alone truncates silently and `Meta` bounds run only at convert/decode, so this constructor gates
        # its own halves: the whole value against the non-negative wire floor FIRST (an arithmetic >> on a negative
        # int floors onto a negative physical half the ceiling check alone never sees, and the mask would still mint
        # a positive logical half beside it), then the physical half against the I63 mint domain; the logical half
        # is structurally < 2**64 by mask.
        if packed < 0:
            raise ValidationError(f"packed value {packed} below the U128 wire domain")
        if (physical := packed >> 64) > _I63_MAX:
            raise ValidationError(f"packed physical half {physical} exceeds the I63 mint domain")
        return cls(physical_ticks=physical, logical=packed & _U64_MAX)

    def compare(self, other: Self, /) -> Ordering:
        return Ordering.of_sign(0 if self == other else -1 if self < other else 1)

    def tick(self, observed: Self, /) -> Self:
        # receive-event successor: the join ceiling with the logical half advanced. A counter at the u64 wire ceiling
        # rolls onto the next physical tick under the C# reset-law (a physical advance zeroes the counter) — `+ 1`
        # past it would collide on `packed` with the next physical stamp — and the mint fails only at the I63 ceiling.
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
    origin: bytes
    logical: U64


class CausalFrame(Struct, frozen=True):
    hlc: Hlc
    tenant: Tenant

    @classmethod
    def of(cls, hlc_physical: I63, hlc_logical: U64, tenant: str) -> Self:
        return cls(hlc=Hlc(hlc_physical, hlc_logical), tenant=Tenant(tenant))

    @classmethod
    def decode(cls, carrier: dict[str, str]) -> RuntimeRail[Self]:
        # `convert` (not `Hlc(...)`) is load-bearing: `Meta` runs only at convert/decode, so the I63 domain and the U64 floor enforce
        # in the C core — a direct `__init__` admits a half the `packed` shift truncates. The <2**64 ceiling is the one bound msgspec
        # cannot express (int64-max constraint law), raised as the same fence's `ValidationError` — one railed surface, never a second
        # gate. An absent slot is the `.get(...)` floor; a present-but-malformed or out-of-domain half is a `ValidationError`.
        def lifted() -> Self:
            frame = convert(
                {
                    "hlc": {"physical_ticks": carrier.get(SLOTS["physical"][0], "0"), "logical": carrier.get(SLOTS["logical"][0], "0")},
                    "tenant": carrier.get(SLOTS["tenant"][0], "default"),
                },
                cls,
                strict=False,
            )
            if frame.hlc.logical > _U64_MAX:
                raise ValidationError(f"logical {frame.hlc.logical} exceeds the u64 wire domain")
            return frame

        return boundary("wire", lifted)

    def attributes(self, shape: AttrShape = "halves") -> dict[str, str | int]:
        # `halves` emits native ints — `physical_ticks` inside the OTLP signed-int64 bound BY TYPE (the I63 decode gate), `logical`
        # under the C# reset-law (a physical advance zeroes the counter); `packed` the `032x` hex STRING, since a raw 128-bit int
        # overflows that bound at export.
        tenant = {SLOTS["tenant"][1]: self.tenant}
        match shape:
            case "halves":
                return tenant | {SLOTS["physical"][1]: self.hlc.physical_ticks, SLOTS["logical"][1]: self.hlc.logical}
            case "packed":
                return tenant | {SLOTS["packed"][1]: format(self.hlc.packed, "032x")}
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# `packed`'s row IS data — never an `rsplit` derivation off the physical key's dotted shape.
SLOTS: Final[Map[Slot, tuple[str, str]]] = Map.of_seq([
    ("physical", ("rasm-hlc-physical", "rasm.hlc.physical")),
    ("logical", ("rasm-hlc-logical", "rasm.hlc.logical")),
    ("tenant", ("rasm-tenant", "rasm.tenant")),
    ("packed", ("rasm-hlc", "rasm.hlc")),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
