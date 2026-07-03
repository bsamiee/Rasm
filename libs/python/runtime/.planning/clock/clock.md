# [PY_RUNTIME_CLOCK]

The single logical-time owner the whole branch consumes. The runtime mints no wall-clock stamp: the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` is the sole host mint (single-mint invariant); this owner decodes the inbound two-half stamp on the `reliability/faults#FAULT` rail, holds the compare/merge/successor algebra interior code reads, and re-mints nothing.

Four shapes carry the concern:

| [SHAPE]       | [ROLE]                                                                                                                   |
| ------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `Hlc`         | the two-64-bit-half cell packing the NodaTime physical instant high and the per-node logical counter low, bit-identical to the C# `Hlc` stamp |
| `Ordering`    | the behavior-bearing three-case causal verdict carrying the C# `Hlc.CompareTo` sign as its case payload, with `fold[T]` so a consumer dispatches on the owner instead of re-`match`ing the tag |
| `ElementId`   | the causal/content-stable peer-local identity the CRDT RGA and OR-set address by                                         |
| `CausalFrame` | the `(Hlc, Tenant)` inbound frame owning the railed `decode` and the dual-shape `attributes` projection, both keyed off the one `SLOTS` table |

`SLOTS` pairs each slot's inbound gRPC-metadata header name with its outbound attribute key, so the slot spelling and the `(tenant, hlc)` attribute map derive from one table rather than scattered literals.

These four shapes collapse here from `transport/serve` and `execution/admission`, where `Hlc`/`ElementId`/`CausalFrame`/`Tenant` formerly mis-lived. `CausalFrame.decode` is the canonical inbound carrier reader `transport/serve#SERVE` `ServerHost.inbound` folds with `.map`; `CausalFrame.attributes` the canonical projection `execution/admission#CONTEXT` `RuntimeContext.attribute` and the serve enricher compose. Both consumers read this owner and re-spell nothing: `ServerHost.inbound` composes `CausalFrame.decode(carrier)` for the sole `SLOTS`-keyed `convert`-validated decode inside the one `boundary("wire", ...)` fence and imports no `boundary`, and admission folds `attributes("packed")` into its parent-id and span-attribute projections. No second `SLOTS`-reading decode, no re-spelled `rasm-hlc-physical`/`rasm-hlc-logical`/`rasm-tenant` literal, and no per-consumer attribute map exist beside this owner.

## [01]-[INDEX]

- [01]-[CLOCK]: the hybrid-logical-clock cell, the behavior-bearing three-case `Ordering` verdict with its sign payload and `fold`, the two-half NodaTime parity contract, the causal compare/merge/successor algebra, the content-stable element id, the tenant partition, the one `SLOTS` carrier-slot/attribute-key table, and the one railed inbound `CausalFrame.decode` plus its dual-shape outbound `attributes` projection.

## [02]-[CLOCK]

- Owner:
  - `Hlc` — the frozen `order=True` hybrid-logical-clock cell carrying `physical_ticks` (NodaTime instant) and `logical` (per-node counter), totally ordered physical-then-logical so the synthesized `<`/`==` is the NodaTime-parity causal order and `merge` reduces to `max`, never a hand-branch.
  - `Ordering` — the behavior-bearing three-case `@tagged_union` verdict (`before`/`equal`/`after`) `Hlc.compare` returns, carrying the C# `Hlc.CompareTo` sign as its case payload and exposing `of_sign`/`sign`/`reverse`/`fold[T]`, so the causal symmetry `a.compare(b) == b.compare(a).reverse()` is one method.
  - `ElementId` — the `order=True` `(origin, logical)` causal/content-stable identity the CRDT element addressing keys by, so the OR-set/RGA tag set sorts by identity.
  - `Tenant` — the one wire-partition newtype.
  - `SLOTS` — the one vocabulary table pairing each slot's inbound header name with its outbound attribute key.
  - `CausalFrame` — the `(Hlc, Tenant)` inbound frame the host mints and the companion decodes, owning the `of` lift, the railed `decode` carrier-slot reader, and the dual-shape `attributes` OTel projection.
- Cases: `Ordering` is the closed verdict `before`(`-1`) · `equal`(`0`) · `after`(`1`) every causal compare resolves to, the case payload pinning the C# `Hlc.CompareTo` sign at the type level so the verdict reproduces that contract as data, not a re-interpreted sentinel. `of_sign` is the one total constructor (`<0`→`before`, `>0`→`after`, else `equal`) `Hlc.compare` lowers its synthesized reads to, and the one entry any consumer reconstructing a verdict from a raw C# `CompareTo` sign reuses rather than a second sign-to-case ladder. `fold[T]` is the sole `tag`-keyed dispatch surface; `sign` and `reverse` are folds over it, never parallel matches — `sign` folds back the narrow `Literal[-1|0|1]` the C# `CompareTo` returns, `reverse` folds the symmetry `a.compare(b) == b.compare(a).reverse()`, so no consumer re-spells `match order: case Ordering(tag=...)` per site and the owner carries exactly one match. `Hlc.compare` lowers the struct's synthesized order to one verdict — `==` proves equal, else one `<` test signs it — physical half dominating, logical breaking the tie. The admission context threads the carry as `Option[CausalFrame]`: `Nothing` locally-minted, `Some(frame)` admitting the host inbound stamp.
- Entry:
  - `CausalFrame.decode(carrier)` returns `RuntimeRail[CausalFrame]`: it reads the `physical`/`logical`/`tenant` header names off `SLOTS` out of the `dict[str, str]` gRPC metadata and lifts the slot strings through `msgspec.convert(mapping, CausalFrame, strict=False)` inside the one `reliability/faults#FAULT` `boundary("wire", ...)` fence. `strict=False` coerces the numeric-string halves to `int` AND runs the `U64` `Meta(ge=0, lt=2**64)` bound in the C core, so the same constraint guards `decode` as guards the wire codec — a non-numeric, negative, or `≥2**64` half is a typed `msgspec.ValidationError` landing as `Error(BoundaryFault(boundary=("wire", "ValidationError")))`, never the `Hlc(...)` `__init__` path that runs no `Meta` check and would silently admit an out-of-domain half the `packed` shift truncates. A genuinely absent slot is the `.get(...)` `"0"`/`"default"` floor — the only branch the default owns, distinct from a present-but-malformed value the bound rejects. The decode and the domain check are one railed surface, not an inline `int()` plus a hand `try` per servicer.
  - `CausalFrame.of(hlc_physical, hlc_logical, tenant)` lifts an already-domain-valid `(physical_ticks, logical)` pair and the wire `tenant` string; the `transport/wire#PROTO_TRANSCODE`-decoded `FaultDetail` `hlc_physical`/`hlc_logical`/`tenant` fields feed it once read off the `Struct`, where the wire codec's own `convert` has already enforced the `U64` bound. It is the direct-construction lift for an upstream that has already validated the domain; `decode` never routes through it precisely because a raw carrier is unvalidated and must cross `convert` to gain the `Meta` check.
  - `CausalFrame.attributes(shape)` is the one OTel projection parameterized over output shape, every key read off `SLOTS` as data. `shape="halves"` emits `SLOTS["tenant"]` plus `SLOTS["physical"]`/`SLOTS["logical"]` (`rasm.hlc.physical`/`rasm.hlc.logical`) as native `int` (a NodaTime-tick count for any representable instant and a per-node counter both stay under `2**63`, inside the OTLP signed-int64 attribute bound the `Span.set_attributes` contract reads directly, even though the `U64` field type permits the full `2**64` domain; no dotted-string re-parse). `shape="packed"` emits `SLOTS["tenant"]` plus the `SLOTS["packed"]` key (`rasm.hlc`) carrying the single 128-bit value as the fixed-width `032x` hex STRING the correlation/parent-id path threads — NOT a raw 128-bit int, which overflows the OTLP signed-int64 domain at export; the hex is symmetric with `Correlation.trace_id.hex()`. The packed key is its OWN `SLOTS` row, the dotted parent the half-keys nest under, never an `rsplit` derivation off `SLOTS["physical"][1]` coupling the packed spelling to the physical key's dotted shape. `RuntimeContext.attribute` and the `transport/serve#SERVE` `ServerHost.enrich` (which folds it) select a shape rather than re-spelling a map, so the two layouts cannot drift across pages.
  - `Hlc.merge(left, right)` folds two cells to the causal-max — `max(left, right)` over the synthesized order returns the larger pair, an equal pair returns `left` unchanged — so it is idempotent and a companion replaying the op-log prefix converges to the producer's monotonic stamp without double-counting a duplicate op.
  - `Hlc.tick(observed)` is the receive-event successor — the larger of the local cell and an observed cell with the logical half advanced by one — the HLC algebra a companion materializing the op-log prefix uses to mint a derived presence beat strictly after every cause seen, distinct from the host physical mint it never performs.
  - `Hlc.packed` renders the single 128-bit value (`physical_ticks << 64 | logical`) the C# `Hlc.ToPacked` UInt128 layout holds and `Hlc.of_packed` splits it back, so a packed stamp crosses the wire and reconstructs without a field-order guess.
- Auto:
  - The `order=True` struct synthesizes the tuple comparison over the declared field order (`physical_ticks` then `logical`), so `<`/`==`/`max` ARE the physical-dominant lexicographic order and `compare`/`merge`/`tick` share that one synthesized order rather than re-deriving a comparison — `merge` a join-semilattice (commutative, associative, idempotent), `tick` a strictly-monotonic successor. `physical_ticks` is the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count in the high 64-bit half, `logical` the per-node counter in the low half, both the `U64` wire domain; the value-level pack/unpack the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] pins (DESIGN-PIN), distinct from a byte serialization.
  - `Ordering` carries the comparison sign as its payload rather than a meaningless per-case `bool`, so `compare` lowers the struct's synthesized `==`/`<` reads to the verdict (the prior `(self > other) - (self < other)` arithmetic-on-bools form is deleted), `of_sign` is the one constructor `compare` and any sign-to-verdict reconstruction share, and `fold` the one case-fold every behavior consumer reuses.
  - `ElementId.origin` is the peer-local node identity (the C# `OpLog` origin guid bytes) and `ElementId.logical` the HLC-stable logical position the RGA and OR-set address by, never a positional index; the `order=True` synthesis orders the `(origin, logical)` tag set so an element survives a re-order by identity and the OR-set tag comparison is the same synthesized order, never a hand sort.
  - `Tenant` is the one partition newtype — the raw `transport/serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` and the inbound `rasm-tenant` slot both absorb into it, never a parallel spelling.
  - The `attributes` dual shape resolves the prior cross-page split where `execution/admission#CONTEXT` emitted the packed `rasm.hlc` hex while this owner had no projection; the `packed` arm is exactly the `format(packed, "032x")` rendering admission's `RuntimeContext.attribute` collapses into, keyed on the `SLOTS["packed"]` row both pages read.
- Packages: `msgspec` (`Struct` frozen records, `order=True` on `Hlc`/`ElementId` for the synthesized total order, `gc=False` on the leaf cells holding no container field so the collector never traces them, `Annotated[int, Meta(ge=0, lt=2**64)]`/`Meta(ge=0, lt=2**128)` the `U64`/`U128` constraint, and `convert(mapping, CausalFrame, strict=False)` the one decode that coerces the carrier strings to `int` AND enforces the `U64` bound in the C core — the same `Meta` bound the `evidence/identity#IDENTITY` page declares for its `U64`/`U128` aliases, one cross-page numeric domain, and the one msgspec member that closes the `Hlc(...)` `__init__`-bypasses-`Meta` gap, so an out-of-domain half is a typed `ValidationError` at every ingress), `expression` (`tagged_union`/`case`/`tag` for the `Ordering` verdict — `fold[T]` the one `tag`-keyed dispatch surface, `sign`/`reverse` folds over it that re-narrow to `Literal[-1|0|1]`/the reversed verdict rather than a second match, so the owner carries exactly one `match` — and the `Option[CausalFrame]` carry the admission context threads), `opentelemetry-api` (the `dict[str, str | int]` attribute-map shape `Span.set_attributes` PUBLIC_TYPES accepts — the projection target, API-only, the one attribute surface the branch enrichers feed; no SDK import), `reliability/faults#FAULT` (`boundary`/`RuntimeRail` — the one wire fence `CausalFrame.decode` folds the `convert` decode through so a malformed inbound stamp rides the rail as the `CLASSIFY` `msgspec`-row `boundary`-tagged fault rather than panicking).
- Growth: every dimension is one field, row, or arm on an existing owner — zero new surface, no parallel stamp record.
  - a new clock dimension (a wall-clock skew bound, a causal-stability watermark) is one field on `Hlc` the `order=True` synthesis folds into the existing `compare`/`merge`/`tick` plus one key on `attributes`;
  - a new identity axis is one field on `ElementId` the `order=True` synthesis sorts;
  - a new frame dimension is one column on `CausalFrame` reachable through the existing `decode`/`attributes`;
  - a new carrier slot or attribute key is one row on `SLOTS` both `decode` and `attributes` already read;
  - a new attribute layout is one `AttrShape` arm on `attributes`, never a per-consumer map;
  - a new comparison outcome is impossible — three is the closed `Ordering` set, and a new consumer behavior over the verdict is one `fold` call site, never a new compare method.
- Boundary: the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` is the sole host-clock mint (single-mint invariant), and `tick` is the companion's purely-logical receive-event successor that re-mints no host physical stamp. The deleted forms are:
  - a Python-side re-minted physical stamp, or a wall-clock `time.time()` substitution for the host half;
  - a second `Hlc` spelling, a path-or-name-keyed `ElementId`, or a second `Tenant` newtype beside this owner;
  - a raw `-1`/`0`/`1` sentinel or a per-case `bool` payload beside the sign-carrying `Ordering`, and a re-`match` of the `Ordering` tag where `fold` dispatches;
  - an inline `int(carrier[...])` slot parse plus a direct `Hlc(physical_ticks, logical)` construction beside the railed `convert` decode (the `__init__` runs no `Meta` check, so a present-but-negative or `≥2**64` half survives the pack-truncating construction the `convert` `U64` bound rejects); a `try`/`except ValueError` that masks a present-but-malformed half to the `"0"` floor (the floor is the absence default `carrier.get(slot, "0")` reads, never a malformed-value swallow);
  - a raw header/attribute literal beside `SLOTS`, and a per-consumer `{"rasm.tenant": ..., "rasm.hlc": ...}` map beside `CausalFrame.attributes`.

  The consumers re-mint nothing: the `transport/wire` codec reconstructs `Hlc`/`ElementId` from the decoded op arms, the `execution/admission` context carries the `CausalFrame` and reads `attributes`, and the serve enricher folds the admission context's projection — so the clock lives in one place rather than scattered across the serve, wire, and admission pages. `merge`/`tick` are the live join-semilattice and successor the op-log prefix replay converges through, and `compare`/`Ordering` is the typed three-way verdict the CRDT LWW adjudication (the `transport/wire#CRDT_DECODE` `set` arm's `LwwRegister` survivor decision) reads to pick the causally-later write — a `before`/`after`/`equal` dispatch that stays one `fold` call site, never a re-derived sign comparison at the adjudication seam.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Annotated, Final, Literal, NewType, Self, assert_never

from expression import case, tag, tagged_union
from msgspec import Meta, Struct, convert

from rasm.runtime.faults import RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

Tenant = NewType("Tenant", str)  # NewType is its own constructor; never a PEP 695 `type` alias (a TypeAliasType is not callable)
type Slot = Literal["physical", "logical", "tenant", "packed"]
type AttrShape = Literal["halves", "packed"]
type U64 = Annotated[int, Meta(ge=0, lt=2**64)]
type U128 = Annotated[int, Meta(ge=0, lt=2**128)]


@tagged_union(frozen=True)
class Ordering:
    # tag IS the verdict; the case payload pins the C# `Hlc.CompareTo` sign at the type level.
    tag: Literal["before", "equal", "after"] = tag()
    before: Literal[-1] = case()
    equal: Literal[0] = case()
    after: Literal[1] = case()

    @classmethod
    def of_sign(cls, sign: int, /) -> Self:
        return cls(before=-1) if sign < 0 else cls(after=1) if sign > 0 else cls(equal=0)

    def fold[T](self, *, before: Callable[[], T], equal: Callable[[], T], after: Callable[[], T]) -> T:
        # the one behavior-dispatch surface keyed on `tag`; `sign`/`reverse` are folds, not parallel matches.
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
        # the carried payload IS the C# `CompareTo` sign, read back through the one `fold` — never a
        # second `tag`-keyed match drifting from this owner's dispatch. The thunks return the narrow
        # `Literal`s so `fold[Literal[-1, 0, 1]]` infers the literal union, no widening to `int`, no cast.
        return self.fold[Literal[-1, 0, 1]](before=lambda: -1, equal=lambda: 0, after=lambda: 1)

    def reverse(self) -> Self:
        return self.fold(before=lambda: Ordering(after=1), equal=lambda: self, after=lambda: Ordering(before=-1))


# --- [MODELS] ---------------------------------------------------------------------------


class Hlc(Struct, frozen=True, order=True, gc=False):
    # both halves carry the UInt64 wire domain matching the packed `UInt128` layout, so a
    # negative or overflowing half is a typed `ValidationError` at `convert`-decode, never silent.
    physical_ticks: U64
    logical: U64

    @property
    def packed(self) -> U128:
        return (self.physical_ticks << 64) | self.logical

    @classmethod
    def of_packed(cls, packed: U128, /) -> Self:
        return cls(physical_ticks=packed >> 64, logical=packed & ((1 << 64) - 1))

    def compare(self, other: Self, /) -> Ordering:
        return Ordering.of_sign(0 if self == other else -1 if self < other else 1)

    def tick(self, observed: Self, /) -> Self:
        # receive-event successor: the join ceiling with the logical half advanced. Interior algebra
        # over two already-domain-valid cells — the `+ 1` rides the unchecked `__init__` because the HLC
        # invariant (a physical advance resets the counter) keeps `logical` far below the `U64` bound;
        # the bound is the decode gate, not this arithmetic's, and `tick` never re-decodes.
        ceiling = max(self, observed)
        return type(self)(ceiling.physical_ticks, ceiling.logical + 1)

    @staticmethod
    def merge(left: Hlc, right: Hlc, /) -> Hlc:
        # the join-semilattice over the synthesized order: commutative, associative, idempotent.
        return max(left, right)


class ElementId(Struct, frozen=True, order=True, gc=False):
    origin: bytes
    logical: U64


# --- [TABLES] ---------------------------------------------------------------------------

# the one carrier-slot vocabulary: the inbound gRPC-metadata header name and the outbound
# OTel attribute key for each slot live in this single table, so `decode` and `attributes`
# read one spelling and no consumer (serve#SERVE inbound decode, admission#CONTEXT attribute) re-spells it.
# the `packed` row is the dotted parent the two half-keys nest under; it owns the `rasm.hlc`
# attribute key (and the packed-hex carrier header) as a TABLE ROW, never a runtime `rsplit`
# derivation off the physical key's dotted shape — the packed key is data, not a string-coupling.
SLOTS: Final[dict[Slot, tuple[str, str]]] = {
    "physical": ("rasm-hlc-physical", "rasm.hlc.physical"),
    "logical": ("rasm-hlc-logical", "rasm.hlc.logical"),
    "tenant": ("rasm-tenant", "rasm.tenant"),
    "packed": ("rasm-hlc", "rasm.hlc"),
}


class CausalFrame(Struct, frozen=True):
    hlc: Hlc
    tenant: Tenant

    @classmethod
    def of(cls, hlc_physical: U64, hlc_logical: U64, tenant: str) -> Self:
        return cls(hlc=Hlc(hlc_physical, hlc_logical), tenant=Tenant(tenant))

    @classmethod
    def decode(cls, carrier: dict[str, str]) -> RuntimeRail[Self]:
        # `convert` (not `Hlc(...)`) is load-bearing: `Meta` runs only at convert/decode, so the
        # `U64` domain is enforced here; a direct `__init__` would admit a half the `packed` shift truncates.
        # An absent slot is the `.get(...)` floor; a present-but-malformed half is a `ValidationError`.
        return boundary(
            "wire",
            lambda: convert(
                {
                    "hlc": {"physical_ticks": carrier.get(SLOTS["physical"][0], "0"), "logical": carrier.get(SLOTS["logical"][0], "0")},
                    "tenant": carrier.get(SLOTS["tenant"][0], "default"),
                },
                cls,
                strict=False,
            ),
        )

    def attributes(self, shape: AttrShape = "halves") -> dict[str, str | int]:
        # one projection axis owns BOTH attribute shapes, every key read off the one `SLOTS`
        # table: `halves` emits the two native-int halves `Span.set_attributes` reads without
        # re-parse (each a NodaTime-tick/counter int inside the OTLP signed-int64 attribute bound);
        # `packed` emits the single 128-bit value as the fixed-width `032x` hex STRING the
        # correlation/parent-id path threads — NOT a raw 128-bit `int`, which overflows the OTLP
        # signed-int64 attribute domain at export. The packed key is the `SLOTS["packed"]` row, the
        # dotted parent the two half-keys nest under, read as data — never an `rsplit` off a half-key.
        tenant = {SLOTS["tenant"][1]: self.tenant}
        match shape:
            case "halves":
                return tenant | {SLOTS["physical"][1]: self.hlc.physical_ticks, SLOTS["logical"][1]: self.hlc.logical}
            case "packed":
                return tenant | {SLOTS["packed"][1]: format(self.hlc.packed, "032x")}
            case _ as unreachable:
                assert_never(unreachable)
```

## [03]-[RESEARCH]

- [ORDERING_VERDICT]: [COMPLETE] — the comparison result is the closed `expression` `@tagged_union` `Ordering` (`before`/`equal`/`after`) rather than a raw `-1`/`0`/`1` sentinel a caller re-interprets, so `Hlc.compare` returns a typed verdict resolved with `fold`/`match` and the bounded outcome set is the canonical-union owner the branch fault/credential/op unions also use. Each case payload pins the comparison SIGN at the type level (`before: Literal[-1]` · `equal: Literal[0]` · `after: Literal[1]`), not a placeholder `bool`, so the verdict reproduces the C# `Hlc.CompareTo` return and `of_sign` is the one total constructor `compare` lowers its synthesized reads to and any sign-to-verdict reconstruction reuses; `fold[T]` is the case-fold-on-the-owner the surfaces-and-dispatch closed-family law (FORM_CHOOSER row 03) mandates and the SOLE `tag`-keyed match on the union, closing on `assert_never` so a fourth tag is a typed build failure; `sign` (the narrow-`Literal` re-narrow) and `reverse` (the causal symmetry `a.compare(b) == b.compare(a).reverse()`) are folds over that one surface, never parallel matches, so the totality gate lives in one place rather than per derived method. The union carries no `order=True` because a verdict is dispatched, not sorted, and an alphabetical tag order would contradict the causal disposition — the sortable order lives on `Hlc`. The `compare` body lowers the struct's synthesized reads (`==` then at most one `<`) to `of_sign`, deleting the prior four-comparison `(self > other) - (self < other)` arithmetic-on-bools form; `merge`/`tick` bypass `compare` and read `max` directly, `compare`/`fold` serving the consumer that needs the explicit three-way dispatch.
- [GC_FALSE_LEAVES]: [COMPLETE] — `Hlc`/`ElementId` carry only `U64`/`bytes` leaf fields (the `Annotated[int, Meta(...)]` bound is decode-time validation metadata, not a container reference — the field is a plain `int` at runtime) and no container reference, so `gc=False` removes them from the cyclic collector's reachable set without a leak risk on the high-volume op-log decode that allocates a cell per arm; the `order=True` synthesis on BOTH (`Hlc` for the physical-then-logical causal order, `ElementId` for the `(origin, logical)` OR-set/RGA tag sort) adds the comparison dunders without adding a traced field, so each leaf stays GC-free. `CausalFrame` holds the `Hlc` struct reference and the `Tenant` newtype so it stays GC-tracked, the `gc=False` applied only to the leaf cells; `Ordering` is a frozen `@tagged_union` per-case verdict allocated at comparison sites carrying one `int` sign payload, not a per-arm op-log leaf, so it carries no `gc` directive.
- [ATTRIBUTE_PROJECTION]: [COMPLETE] — reflection-confirmed against `libs/python/.api/opentelemetry-api.md`: the OTel attribute contract admits `str | bool | int | float | Sequence[...]` values (IMPLEMENTATION_LAW attributes row) and `Span.set_attributes` (trace ENTRYPOINTS [12]) accepts a mapping in one call, so `CausalFrame.attributes(shape)` returns `dict[str, str | int]` with every key derived from the one `SLOTS` table. The value type resolves BY SHAPE because the two layouts have different width domains: `shape="halves"` emits `rasm.hlc.physical`/`rasm.hlc.logical` as native `int` (each inside the OTLP signed-int64 attribute bound an exporter serializes the `int_value` field to, read as a number with no re-parse); `shape="packed"` emits the single 128-bit value as the fixed-width `032x` hex STRING — a raw 128-bit `int` would OVERFLOW that signed-int64 domain at export, so the packed value crosses as the width-safe hex symmetric with `Correlation.trace_id.hex()`, never a native packed int and never a `f"{physical_ticks}.{logical}"` dotted decimal. The packed key is its OWN `SLOTS["packed"]` row (`rasm.hlc`) read as data, the dotted parent the two half-keys nest under — never a runtime `SLOTS["physical"][1].rsplit(".", 1)[0]` derivation coupling the packed spelling to the physical key's dotted shape, so the table carries the lineage and the packed key stays a row rather than a string-coupling off a sibling key. The match closes on `assert_never`, so a third `AttrShape` arm is a typed build failure. This is the canonical projector the `execution/admission#CONTEXT` `RuntimeContext.attribute` reads with byte-identical output: admission folds `causal.map(lambda frame: base | frame.attributes("packed")).default_value(base)` and `Correlation.seed` un-hexes `attributes("packed")["rasm.hlc"]` to the 16-byte parent id, the prior inline `format(frame.hlc.packed, "032x")` admission spelling deleted on that page. `observability/metrics#METRIC` is NOT a consumer — it keys instruments by `rpc.method`/`DrainOutcome` and reads no causal-frame attribute — so the consumers are admission and the serve enricher folding admission.
