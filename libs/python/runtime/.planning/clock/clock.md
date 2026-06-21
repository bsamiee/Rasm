# [PY_RUNTIME_CLOCK]

The single logical-time owner the whole branch consumes. `Hlc` is the hybrid-logical-clock cell — a two-64-bit-half value packing the NodaTime physical instant into the high half and the per-node logical counter into the low half — reproducing the C# `Rasm.AppHost/Runtime/ports#PORT_RECORDS` host-minted stamp bit-identically; `Ordering` is the behavior-bearing three-case causal verdict every comparison and merge speaks instead of a raw `-1`/`0`/`1` sentinel, carrying the C# `Hlc.CompareTo` sign as its payload and a `fold[T]` so a consumer dispatches behavior on the owner rather than re-`match`ing the tag at every site; `ElementId` is the causal/content-stable peer-local identity the CRDT RGA and OR-set address by; `CausalFrame` composes the `Hlc` with the one `Tenant` wire-partition into the inbound frame the admission context carries and the serve span enricher reads, and owns both the inbound carrier-slot decode and the outbound OTel attribute projection over the one `SLOTS` vocabulary table that pairs each slot's inbound gRPC-metadata header name with its outbound attribute key — so the slot spelling and the `(tenant, hlc)` attribute map each derive from one table rather than scattered string literals. The runtime mints no wall-clock stamp — the C# `AppHost/Runtime` is the sole host mint (single-mint invariant); this owner decodes the inbound two-half stamp on the `reliability/faults#FAULT` rail, holds the comparison/merge/successor algebra interior code reads, and re-mints no host stamp. `Hlc`/`ElementId`/`CausalFrame`/`Tenant` formerly mis-lived in `transport/serve` and `execution/admission`, and the `rasm-hlc-physical`/`rasm-hlc-logical`/`rasm-tenant` slot literals plus the `(tenant, hlc)` attribute map were spelled inline across `transport/serve#SERVE` and `execution/admission#CONTEXT`; they collapse here so the wire codec, the admission context, the serve span enricher, and the metrics attributer compose `CausalFrame.decode`/`CausalFrame.attributes` rather than re-spelling a stamp, a slot literal, and an attribute map — the inline `int(carrier[...])` parse in `serve#SERVE` `ServerHost.inbound` and the inline `{"rasm.tenant": ..., "rasm.hlc": ...}` map in `admission#CONTEXT` `RuntimeContext.attribute` are the forms this owner replaces.

## [01]-[INDEX]

- [01]-[CLOCK]: the hybrid-logical-clock cell, the behavior-bearing three-case `Ordering` verdict with its sign payload and `fold`, the two-half NodaTime parity contract, the causal compare/merge/successor algebra, the content-stable element id, the tenant partition, the one `SLOTS` carrier-slot/attribute-key table, and the one railed inbound `CausalFrame.decode` plus its dual-shape outbound `attributes` projection.

## [02]-[CLOCK]

- Owner: `Hlc` — the frozen `order=True` hybrid-logical-clock cell carrying the `physical_ticks` NodaTime instant and the `logical` per-node counter, totally ordered physical-then-logical so the synthesized `<`/`==` is the NodaTime-parity causal order and `merge` reduces to `max` rather than a hand-branch; `Ordering` the behavior-bearing three-case `@tagged_union` verdict (`before`/`equal`/`after`) `Hlc.compare` returns, carrying the C# `Hlc.CompareTo` sign as its case payload and exposing `of_sign`/`sign`/`reverse`/`fold[T]` so a consumer dispatches behavior on the owner and the causal symmetry (`a.compare(b) == b.compare(a).reverse()`) is one method, not a re-test; `ElementId` the `(origin, logical)` causal/content-stable identity the CRDT element addressing keys by, `order=True` so the OR-set/RGA tag set sorts by identity; `Tenant` the one wire-partition newtype; `SLOTS` the one slot vocabulary table pairing each slot's inbound header name with its outbound attribute key; `CausalFrame` the `(Hlc, Tenant)` inbound frame the host mints and the companion decodes, owning the one `of` lift, the one railed `decode` carrier-slot reader, and the one dual-shape `attributes` OTel projection.
- Cases: `Ordering` is the bounded comparison vocabulary — `before`(`-1`) · `equal`(`0`) · `after`(`1`), the closed verdict set every causal compare resolves to, replacing the bare sentinel a caller would otherwise re-interpret; the case payload IS the C# `Hlc.CompareTo` sign, so `Ordering.of_sign` is the one total constructor (`<0`→`before`, `>0`→`after`, else `equal`), `sign` reads it back for a wire crossing, `reverse` flips the verdict for the symmetric compare, and `fold[T]` dispatches the three branches as callables on the owner so a consumer never re-spells `match order: case Ordering(tag=...)` per call site; `Hlc.compare` resolves the total causal order over the two-half pair to one `Ordering` case through a SINGLE synthesized comparison — `==` proves the equal case, else one `<` test signs the verdict — physical half dominating and the logical half breaking the tie, the NodaTime-parity comparison the C# `Hlc.CompareTo` holds; `CausalFrame.causal` is the carry the admission context threads as `Option[CausalFrame]` — `Nothing` for a locally-minted context and `Some(frame)` for a context admitting the host-minted inbound stamp.
- Entry: `CausalFrame.decode(carrier)` is the one inbound carrier reader returning `RuntimeRail[CausalFrame]` — it lifts the `physical`/`logical`/`tenant` header names off the `SLOTS` table (the sole owner of those literals) out of the `dict[str, str]` gRPC metadata map and folds them through `CausalFrame.of` INSIDE the one `reliability/faults#FAULT` `boundary("wire", ...)` fence, so a present-but-malformed half is a real `int()` raise that lands as `Error(BoundaryFault(boundary=("wire", "ValueError")))` rather than a silent `"0"`-default mask admitting a zero stamp, and a genuinely absent slot is the locally-minted floor — the parse and the lift are one railed surface a caller composes, not an inline `try` plus a hand `int()` per servicer; `CausalFrame.of(hlc_physical, hlc_logical, tenant)` is the value-level lift over the already-parsed `(physical_ticks, logical)` pair and the wire `tenant` string the `transport/wire#PROTO_TRANSCODE`-decoded `FaultDetail` fields also feed once their `hlc_physical`/`hlc_logical`/`tenant` values are read off the `Struct`; `CausalFrame.attributes(shape)` is the one polymorphic OTel projection parameterized over output shape — `shape="halves"` emits `rasm.hlc.physical`/`rasm.hlc.logical` as native ints (each a NodaTime-tick/counter value inside the OTLP signed-int64 attribute bound, the `Span.set_attributes` contract reads them directly, no dotted-string re-parse) plus `rasm.tenant`, and `shape="packed"` emits the single 128-bit `rasm.hlc` value as the fixed-width `032x` hex STRING the correlation/parent-id path threads — NOT a raw 128-bit int, which overflows the OTLP signed-int64 attribute domain a downstream exporter serializes to; the hex rendering is symmetric with `Correlation.trace_id.hex()` and is the canonical packed form the `execution/admission#CONTEXT` `RuntimeContext.attribute` composes; both shapes key off the one `SLOTS` table, so `RuntimeContext.attribute` and the `transport/serve#SERVE` `ServerHost.enrich` (which folds `RuntimeContext.attribute`) select a shape rather than re-spelling a map and the two attribute layouts cannot drift across pages; `Hlc.merge(left, right)` folds two cells to the causal-max — `max(left, right)` over the synthesized total order returns the larger pair and an equal pair returns `left` unchanged, so it is genuinely idempotent and a companion replaying the op-log prefix converges to the producer's monotonic stamp without double-counting a duplicate op; `Hlc.tick(observed)` is the receive-event successor — the larger of the local cell and an observed cell with the logical half advanced by one, the HLC algebra a companion materializing the op-log prefix uses to mint a derived presence beat strictly after every cause it has seen, distinct from the host physical mint it never performs; `Hlc.packed` renders the single 128-bit two-half value (`physical_ticks << 64 | logical`) the C# `Hlc.ToPacked` UInt128 layout holds, and `Hlc.of_packed` splits it back, so a packed stamp crosses the wire and reconstructs without a field-order guess.
- Auto: the two-half parity is the NodaTime contract — `physical_ticks` is the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count (the C# `Instant` the `AppHost/Runtime` stamps) occupying the high 64-bit half, and `logical` is the per-node counter occupying the low 64-bit half, so the packed `UInt128` orders physical-then-logical and the `(physical_ticks, logical)` Python pair reconstructs the C# `Hlc` value by value with no byte-swap — value-level reconstruction of the physical-high/logical-low two-half order the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] pins (DESIGN-PIN), an integer pack/unpack distinct from the byte-serialized `to_bytes(16, 'little')` xxhash child fold that page proves; the `order=True` struct synthesizes the tuple comparison over the declared field order (`physical_ticks` then `logical`), so `<`/`==`/`max` ARE the physical-dominant lexicographic order and `compare`/`merge`/`tick` share that one synthesized order rather than re-deriving a comparison — `merge` is therefore a join-semilattice (commutative, associative, idempotent) and `tick` a strictly-monotonic successor over the same order; `Ordering` carries the comparison sign as its payload rather than a meaningless per-case `bool`, so `compare` lowers ONE synthesized comparison to the verdict (the prior `(self > other) - (self < other)` two-comparison arithmetic-on-bools form is deleted), `of_sign` is the total constructor the C# sign feeds and the `transport/wire` decode reuses, `reverse` is the causal symmetry, and `fold[T]` is the case-fold-on-the-owner the surfaces-and-dispatch closed-family law mandates so the verdict owns its dispatch; `ElementId.origin` is the peer-local node identity (the C# `OpLog` origin guid bytes) and `ElementId.logical` the HLC-stable logical position the RGA and OR-set address by, never a positional index, the `order=True` synthesis ordering the `(origin, logical)` tag set deterministically so an element survives a re-order by identity and the OR-set tag comparison is the same synthesized order, never a hand sort; `Tenant` is the one partition newtype — the raw `transport/serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` and the inbound `rasm-tenant` slot both absorb into it, never a parallel tenant spelling; `attributes` keys off the one `SLOTS` table and resolves the value type by shape — the `halves` shape emits the two halves as native `int` (each inside the OTLP signed-int64 attribute bound, read as a number rather than a re-parsed `f"{p}.{l}"` dotted string), and the `packed` shape emits the one 128-bit value as the fixed-width `032x` hex STRING because a raw 128-bit int overflows the OTLP signed-int64 attribute domain at export — the hex rendering symmetric with the `trace_id.hex()` rendering and width-safe where a native packed int is not; the dual shape resolves the prior cross-page split where `admission#CONTEXT` emitted the single packed `rasm.hlc` hex while this owner had no projection, the `packed` arm being exactly the `format(packed, "032x")` rendering admission's `RuntimeContext.attribute` collapses into.
- Packages: `msgspec` (`Struct` frozen records, `order=True` on `Hlc`/`ElementId` for the synthesized total order, `gc=False` on the leaf `Hlc`/`ElementId` cells that hold no container field so the collector never traces them), `expression` (`tagged_union`/`case`/`tag` for the `Ordering` verdict, its `fold[T]` case dispatch, and the `Option[CausalFrame]` carry the admission context threads), `opentelemetry-api` (the `dict[str, str | int]` attribute-map shape `Span.set_attributes` PUBLIC_TYPES accepts — the projection target, API-only, the one attribute surface the branch enrichers feed; no SDK import), `reliability/faults#FAULT` (`boundary`/`RuntimeRail` — the one wire fence `CausalFrame.decode` folds the slot parse through so a malformed inbound stamp rides the rail rather than panicking).
- Growth: a new clock dimension (a wall-clock skew bound, a causal-stability watermark) is one field on `Hlc` the `order=True` synthesis folds into the existing `compare`/`merge`/`tick` and one key on `attributes`; a new identity axis is one field on `ElementId` the `order=True` synthesis sorts; a new frame dimension is one column on `CausalFrame` reachable through the existing `decode`/`attributes`; a new carrier slot or attribute key is one row on `SLOTS` both `decode` and `attributes` already read; a new attribute layout is one `AttrShape` arm on `attributes`, never a per-consumer map; a new comparison outcome is impossible — three is the closed `Ordering` set, and a new consumer behavior over the verdict is one `fold` call site, never a new compare method; zero new surface, no parallel stamp record.
- Boundary: the C# `AppHost/Runtime` is the sole host-clock mint — a Python-side re-minted physical stamp, a wall-clock `time.time()` substitution for the host-minted physical half, a second `Hlc` spelling beside this owner, a path-or-name-keyed `ElementId`, a second `Tenant` newtype, a raw `-1`/`0`/`1` comparison sentinel or a per-case `bool` payload beside the sign-carrying `Ordering` verdict, a re-`match` of the `Ordering` tag where `fold` dispatches, an inline `int(carrier[...])` slot parse or a `"0"`-default silent mask beside the railed `CausalFrame.decode`, a raw header/attribute string literal beside the one `SLOTS` table, and a per-consumer `{"rasm.tenant": ..., "rasm.hlc": ...}` attribute map beside `CausalFrame.attributes` are the deleted forms; `tick` is the companion's purely-logical receive-event successor and re-mints no host physical stamp, the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` being the single mint per the single-mint invariant; the wire codec at `transport/wire` reconstructs `Hlc`/`ElementId` from the decoded op arms, the admission context at `execution/admission` carries the `CausalFrame` and reads `attributes`, and the serve enricher folds the admission context's `attributes` projection — all consume this owner and re-mint no stamp, slot, or attribute set, so the clock lives in one place rather than scattered across the serve, wire, and admission pages.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final, Literal, NewType, Self

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

type Tenant = NewType("Tenant", str)
type Slot = Literal["physical", "logical", "tenant"]
type AttrShape = Literal["halves", "packed"]


@tagged_union(frozen=True)
class Ordering:
    # tag IS the verdict; the sign payload reproduces the C# `Hlc.CompareTo` return so a
    # consumer reads the wire sign directly and `fold` dispatches behavior on the owner.
    tag: Literal["before", "equal", "after"] = tag()
    before: int = case()
    equal: int = case()
    after: int = case()

    @classmethod
    def of_sign(cls, sign: int, /) -> Self:
        return cls(before=-1) if sign < 0 else cls(after=1) if sign > 0 else cls(equal=0)

    @property
    def sign(self) -> Literal[-1, 0, 1]:
        match self.tag:
            case "before":
                return -1
            case "after":
                return 1
            case _:
                return 0

    def reverse(self) -> Self:
        return Ordering.of_sign(-self.sign)

    def fold[T](self, *, before: Callable[[], T], equal: Callable[[], T], after: Callable[[], T]) -> T:
        match self.tag:
            case "before":
                return before()
            case "after":
                return after()
            case _:
                return equal()


# --- [MODELS] ---------------------------------------------------------------------------


class Hlc(Struct, frozen=True, order=True, gc=False):
    physical_ticks: int
    logical: int

    @property
    def packed(self) -> int:
        return (self.physical_ticks << 64) | self.logical

    @classmethod
    def of_packed(cls, packed: int, /) -> Self:
        return cls(physical_ticks=packed >> 64, logical=packed & ((1 << 64) - 1))

    def compare(self, other: "Hlc", /) -> Ordering:
        # one synthesized total-order read; `==` is exact equality, else the single `<` test signs it.
        return Ordering.of_sign(0 if self == other else -1 if self < other else 1)

    def tick(self, observed: "Hlc", /) -> "Hlc":
        ceiling = max(self, observed)
        return Hlc(ceiling.physical_ticks, ceiling.logical + 1)

    @staticmethod
    def merge(left: "Hlc", right: "Hlc") -> "Hlc":
        return max(left, right)


class ElementId(Struct, frozen=True, order=True, gc=False):
    origin: bytes
    logical: int


# --- [TABLES] ---------------------------------------------------------------------------

# the one carrier-slot vocabulary: the inbound gRPC-metadata header name and the outbound
# OTel attribute key for each slot live in this single table, so `decode` and `attributes`
# read one spelling and no consumer (serve#SERVE inbound decode, admission#CONTEXT attribute) re-spells it.
SLOTS: Final[dict[Slot, tuple[str, str]]] = {
    "physical": ("rasm-hlc-physical", "rasm.hlc.physical"),
    "logical": ("rasm-hlc-logical", "rasm.hlc.logical"),
    "tenant": ("rasm-tenant", "rasm.tenant"),
}


class CausalFrame(Struct, frozen=True):
    hlc: Hlc
    tenant: Tenant

    @classmethod
    def of(cls, hlc_physical: int, hlc_logical: int, tenant: str) -> Self:
        return cls(hlc=Hlc(hlc_physical, hlc_logical), tenant=Tenant(tenant))

    @classmethod
    def decode(cls, carrier: dict[str, str]) -> RuntimeRail[Self]:
        # total over the carrier: a present-but-malformed physical/logical slot is a real
        # `int()` raise the one wire fence converts, never a silent `"0"`-default mask that
        # admits a zero stamp; an absent slot is the locally-minted floor (physical 0 / logical 0).
        return boundary(
            "wire",
            lambda: cls.of(
                int(carrier.get(SLOTS["physical"][0], "0")),
                int(carrier.get(SLOTS["logical"][0], "0")),
                carrier.get(SLOTS["tenant"][0], "default"),
            ),
        )

    def attributes(self, shape: AttrShape = "halves") -> dict[str, str | int]:
        # one projection axis owns BOTH attribute shapes off the one `SLOTS` table: `halves`
        # emits the two native-int halves `Span.set_attributes` reads without re-parse (each
        # half a NodaTime-tick/counter int safely inside the OTLP signed-int64 attribute bound),
        # `packed` emits the single 128-bit value as the fixed-width `032x` hex STRING the
        # correlation/parent-id path threads — NOT a raw 128-bit `int`, which overflows the OTLP
        # signed-int64 attribute domain at export; the hex string is symmetric with `trace_id`'s
        # hex rendering and is the one form `admission#CONTEXT` composes rather than re-spelling.
        tenant = {SLOTS["tenant"][1]: str(self.tenant)}
        match shape:
            case "halves":
                return tenant | {SLOTS["physical"][1]: self.hlc.physical_ticks, SLOTS["logical"][1]: self.hlc.logical}
            case "packed":
                return tenant | {"rasm.hlc": format(self.hlc.packed, "032x")}
```

## [03]-[RESEARCH]

- [NODATIME_PARITY]: [COMPLETE] (design) — the two-half contract is decoded against the C# `Rasm.AppHost/Runtime/ports#PORT_RECORDS` `Hlc` stamp. `physical_ticks` is the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count occupying the high 64-bit half and `logical` the per-node counter occupying the low 64-bit half, so the packed `UInt128` is `physical_ticks << 64 | logical` and the `(physical_ticks, logical)` Python pair reconstructs the C# value by value — a value-level integer pack (physical in the high 64-bit half, most-significant-half-first), distinct from any byte serialization, in the physical-high/logical-low two-half order the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] pins (DESIGN-PIN, never byte-fabricated in that binding). The `msgspec` `order=True` struct synthesizes the comparison over the declared field tuple `(physical_ticks, logical)`, so `<`/`==`/`max` are the physical-dominant lexicographic order and `Hlc.compare` lowers that synthesized order to the three-case `Ordering` verdict matching the C# `Hlc.CompareTo` sign, `Hlc.merge` is the physical-dominant join-semilattice `max` (commutative, associative, idempotent — `max(x, x) == x`), and `Hlc.tick` is the receive-event successor (`max` ceiling with the logical half advanced) the C# `Hlc` receive path holds; all three read the one synthesized order, matching the C# `Rasm.AppHost/Runtime/ports#PORT_RECORDS` `Hlc.CompareTo`/`Hlc.Merge` order and the `Rasm.Persistence/Version/commits#CRDT_WIRE` `Crdt.Merge` join-semilattice the op-log prefix converges; the design is settled and re-mints no host stamp. `msgspec` and `expression` are cp315-clean (the latter pure-Python, no ABI tag), so `Hlc`/`Ordering`/`ElementId`/`CausalFrame` impose no gated band — distinct from the `xxhash`/`lz4` `python_version<'3.15'` companion legs the evidence and wire pages inherit.
- [ORDERING_VERDICT]: [COMPLETE] — the comparison result is the closed `expression` `@tagged_union` `Ordering` (`before`/`equal`/`after`) rather than a raw `-1`/`0`/`1` sentinel a caller re-interprets, so `Hlc.compare` returns a typed verdict a consumer resolves with `fold`/`match` and the bounded outcome set is the canonical-union owner the branch fault/credential/op unions also use. Each case payload is the comparison SIGN (`before=-1`/`equal=0`/`after=1`), not a placeholder `bool`, so the verdict reproduces the C# `Hlc.CompareTo` return for a wire crossing and `of_sign` is the one total constructor both `compare` and the `transport/wire` op-decode reuse; `reverse` is the causal symmetry (`a.compare(b) == b.compare(a).reverse()`) as one method, and `fold[T]` is the case-fold-on-the-owner the surfaces-and-dispatch closed-family law (FORM_CHOOSER row 03: a closed family owns its behavior through a case fold, not an external `match` repeated per call) mandates, so a consumer dispatches behavior on the verdict without re-spelling the tag match. The union carries no `order=True` because a verdict is dispatched, not sorted, and an alphabetical tag order (`after` before `before`) would contradict the causal disposition — the sortable order lives on `Hlc` itself. The `compare` body lowers ONE synthesized comparison (`==` then a single `<`) to `of_sign`, deleting the prior `(self > other) - (self < other)` two-comparison arithmetic-on-bools form that read the order twice; `merge`/`tick` bypass `compare` entirely and read `max` directly, `compare`/`fold` serving the consumer that needs the explicit three-way dispatch rather than the max fold.
- [GC_FALSE_LEAVES]: [COMPLETE] — `Hlc`/`ElementId` carry only `int`/`bytes` leaf fields and no container reference, so `gc=False` removes them from the cyclic collector's reachable set without a leak risk on the high-volume op-log decode that allocates a cell per arm; the `order=True` synthesis on BOTH (`Hlc` for the physical-then-logical causal order, `ElementId` for the `(origin, logical)` OR-set/RGA tag sort) adds the comparison dunders without adding a traced field, so each leaf stays GC-free. `CausalFrame` holds the `Hlc` struct reference and the `Tenant` newtype so it stays GC-tracked, the `gc=False` applied only to the leaf cells; `Ordering` is a frozen `@tagged_union` per-case verdict allocated at comparison sites carrying one `int` sign payload, not a per-arm op-log leaf, so it carries no `gc` directive.
- [ATTRIBUTE_PROJECTION]: [COMPLETE] — reflection-confirmed against `libs/python/.api/opentelemetry-api.md`: the OTel attribute contract admits `str | bool | int | float | Sequence[...]` values (IMPLEMENTATION_LAW attributes row) and `Span.set_attributes` (trace ENTRYPOINTS [12]) accepts a mapping in one call, so `CausalFrame.attributes(shape)` returns `dict[str, str | int]` keyed off the one `SLOTS` table. The value type is resolved BY SHAPE because the two layouts have different width domains, not a fixed int: `shape="halves"` emits `rasm.hlc.physical`/`rasm.hlc.logical` as native `int` (each half a NodaTime-tick/per-node counter value inside the OTLP signed-int64 attribute bound an exporter serializes the `int_value` field to, read as a number by a metric/exemplar with no re-parse), and `shape="packed"` emits the single 128-bit `rasm.hlc` as the fixed-width `032x` hex STRING the correlation/parent-id thread reads — a raw 128-bit `int` would OVERFLOW that signed-int64 attribute domain at export, so the packed value must cross as the width-safe hex string symmetric with the `Correlation.trace_id.hex()` rendering, never a native packed int and never a `f"{physical_ticks}.{logical}"` dotted decimal. The owner answers BOTH layouts from one method rather than forcing a downstream re-parse. This resolves a real cross-page split: `execution/admission#CONTEXT` `RuntimeContext.attribute` currently inlines `{"rasm.hlc": format(frame.hlc.packed, "032x")}` (the packed hex shape it already mints width-safely) while `transport/serve#SERVE` `ServerHost.enrich` folds `RuntimeContext.attribute()` — neither yet routes through this owner, so the slot literals and the `(tenant, hlc)` hex map are spelled across the admission and serve pages today. The clock owner is the canonical projector that inline map collapses INTO with byte-identical output: admission's `attribute` reads `frame.attributes("packed")` (yielding the same `032x` hex), serve's `enrich` composes `context.attribute()`, and a new attributed dimension is one `SLOTS` row plus, if a new layout is needed, one `AttrShape` arm — the inline `{"rasm.tenant": ..., "rasm.hlc": ...}` admission map is the deleted form. The actual edit replacing the inline map with `CausalFrame.attributes` lives on the admission page (cross-file), not realizable inside this owner; `observability/metrics#METRIC` is NOT a consumer — that owner keys its instruments by `rpc.method`/`DrainOutcome` only and reads no causal-frame attribute, so the clock projection's consumers are admission and the serve enricher that folds admission, not a metrics attributer.
