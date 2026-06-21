# [PY_RUNTIME_CLOCK]

The single logical-time owner the whole branch consumes. `Hlc` is the hybrid-logical-clock cell — a two-64-bit-half value packing the NodaTime physical instant into the high half and the per-node logical counter into the low half — reproducing the C# `Rasm.AppHost/Runtime/ports#PORT_RECORDS` host-minted stamp bit-identically; `ElementId` is the causal/content-stable peer-local identity the CRDT RGA and OR-set address by; `CausalFrame` composes the `Hlc` with the one `Tenant` wire-partition into the inbound frame every lane, receipt, and metric attributes to. The runtime mints no clock — the C# `AppHost/Runtime` is the sole mint (single-mint invariant); this owner decodes the inbound two-half stamp, holds the comparison and merge algebra interior code reads, and re-mints nothing. `Hlc`/`ElementId`/`CausalFrame`/`Tenant` formerly mis-lived in `transport/serve` and `execution/admission`; they collapse here so the wire codec, the admission context, and the metrics attributer all reference one clock owner rather than a stamp scattered across the serve and admission pages.

## [01]-[INDEX]

- [01]-[CLOCK]: the hybrid-logical-clock cell, the two-half NodaTime parity contract, the causal-merge algebra, the content-stable element id, the tenant partition, and the one inbound `CausalFrame` constructor.

## [02]-[CLOCK]

- Owner: `Hlc` — the frozen hybrid-logical-clock cell carrying the `physical_ticks` NodaTime instant and the `logical` per-node counter, with the comparison and `merge` join the causal order reads; `ElementId` the `(origin, logical)` causal/content-stable identity the CRDT element addressing keys by; `Tenant` the one wire-partition newtype; `CausalFrame` the `(Hlc, Tenant)` inbound frame the host mints and the companion decodes, with the one `of` constructor lifting the decoded two-half stamp.
- Cases: `Hlc.compare` is the total causal order over the two-half pair — physical half dominates and the logical half breaks the tie, so `(physical, logical)` lexicographic comparison is the NodaTime-parity order the C# `Hlc.CompareTo` holds; `CausalFrame.causal` is the carry the admission context threads as `Option[CausalFrame]` — `Nothing` for a locally-minted context and `Some(frame)` for a context admitting the host-minted inbound stamp.
- Entry: `CausalFrame.of(hlc_physical, hlc_logical, tenant)` is the one inbound constructor lifting the decoded `(physical_ticks, logical)` pair and the wire `tenant` string into the frozen frame — the companion's `transport/serve#SERVE` `ServerHost.inbound` is the sole caller, lifting the inbound carrier's `rasm-hlc-physical`/`rasm-hlc-logical`/`rasm-tenant` slot (the `transport/wire#PROTO_TRANSCODE` `FaultDetail` codec only decodes those values onto its `Struct` fields and never calls `of`), so the string-to-int parse rides the one `reliability/faults#FAULT` `boundary("wire", ...)` fence and a malformed slot lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))` before reaching `of`, never a partial frame; `Hlc.merge(left, right)` folds two cells to the causal-max over the `(physical_ticks, logical)` lexicographic order — the larger pair wins and an equal pair returns the left cell unchanged — so it is genuinely idempotent and a companion replaying the op-log prefix converges to the producer's monotonic stamp without double-counting a duplicate op; `Hlc.packed` renders the single 128-bit two-half value (`physical_ticks << 64 | logical`) the C# `Hlc.ToPacked` UInt128 layout holds, and `Hlc.of_packed` splits it back, so a packed stamp crosses the wire and reconstructs without a field-order guess.
- Auto: the two-half parity is the NodaTime contract — `physical_ticks` is the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count (the C# `Instant` the `AppHost/Runtime` stamps), occupying the high 64-bit half, and `logical` is the per-node counter occupying the low 64-bit half, so the packed `UInt128` orders physical-then-logical and the `(physical_ticks, logical)` Python pair reconstructs the C# `Hlc` value by value with no byte-swap — value-level reconstruction of the physical-high/logical-low two-half order the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] pins (DESIGN-PIN), an integer pack/unpack distinct from the byte-serialized `to_bytes(16, 'little')` xxhash child fold that page proves; `ElementId.origin` is the peer-local node identity (the C# `OpLog` origin guid bytes) and `ElementId.logical` the HLC-stable logical position the RGA and OR-set address by, never a positional index, so an element survives a re-order by identity; the merge is a join-semilattice over the physical-dominant order, so it is commutative, associative, and idempotent and a duplicate op is absorbed rather than double-counted; `Tenant` is the one partition newtype — the raw `transport/serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` and the inbound `rasm-tenant` slot both absorb into it, never a parallel tenant spelling.
- Packages: `msgspec` (`Struct` frozen records, `gc=False` on the leaf `Hlc`/`ElementId` cells that hold no container field so the collector never traces them).
- Growth: a new clock dimension (a wall-clock skew bound, a causal-stability watermark) is one field on `Hlc` folded into the existing `compare`/`merge`; a new identity axis is one field on `ElementId`; a new frame dimension is one column on `CausalFrame`, never a parallel stamp record; zero new surface.
- Boundary: the C# `AppHost/Runtime` is the sole clock mint — a Python-side re-minted stamp, a second `Hlc` spelling beside this owner, a wall-clock `time.time()` substitution for the host-minted physical half, a path-or-name-keyed `ElementId`, and a second `Tenant` newtype are the deleted forms; `CausalFrame.of` decodes the inbound two-half stamp and re-mints nothing, the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` being the single mint per the single-mint invariant; the wire codec at `transport/wire` reconstructs `Hlc`/`ElementId` from the decoded op arms and the admission context at `execution/admission` carries the `CausalFrame` — both consume this owner and re-mint nothing, so the clock lives in one place rather than scattered across the serve, wire, and admission pages.

```python signature
from typing import NewType, Self

from msgspec import Struct


type Tenant = NewType("Tenant", str)


class Hlc(Struct, frozen=True, gc=False):
    physical_ticks: int
    logical: int

    @property
    def packed(self) -> int:
        return (self.physical_ticks << 64) | self.logical

    @classmethod
    def of_packed(cls, packed: int) -> Self:
        return cls(physical_ticks=packed >> 64, logical=packed & ((1 << 64) - 1))

    def compare(self, other: "Hlc") -> int:
        mine, theirs = (self.physical_ticks, self.logical), (other.physical_ticks, other.logical)
        return (mine > theirs) - (mine < theirs)

    @staticmethod
    def merge(left: "Hlc", right: "Hlc") -> "Hlc":
        match left.compare(right):
            case order if order >= 0:
                return left
            case _:
                return right


class ElementId(Struct, frozen=True, gc=False):
    origin: bytes
    logical: int


class CausalFrame(Struct, frozen=True):
    hlc: Hlc
    tenant: Tenant

    @classmethod
    def of(cls, hlc_physical: int, hlc_logical: int, tenant: str) -> Self:
        return cls(hlc=Hlc(hlc_physical, hlc_logical), tenant=Tenant(tenant))
```

## [03]-[RESEARCH]

- [NODATIME_PARITY]: [COMPLETE] (design) — the two-half contract is decoded against the C# `Rasm.AppHost/Runtime/ports#PORT_RECORDS` `Hlc` stamp. `physical_ticks` is the `NodaTime.Instant.ToUnixTimeTicks()` 100-ns count occupying the high 64-bit half and `logical` the per-node counter occupying the low 64-bit half, so the packed `UInt128` is `physical_ticks << 64 | logical` and the `(physical_ticks, logical)` Python pair reconstructs the C# value by value — a value-level integer pack (physical in the high 64-bit half, most-significant-half-first), distinct from any byte serialization, in the physical-high/logical-low two-half order the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] pins (DESIGN-PIN, never byte-fabricated in that binding). The comparison is physical-dominant lexicographic and the merge is the physical-dominant join-semilattice (commutative, associative, idempotent — the tie arm returns the left cell unchanged so `merge(x, x) == x`), matching the C# `Rasm.AppHost/Runtime/ports#PORT_RECORDS` `Hlc.CompareTo`/`Hlc.Merge` order and the `Rasm.Persistence/Version/commits#CRDT_WIRE` `Crdt.Merge` join-semilattice the op-log prefix converges; the design is settled and re-mints no stamp. `msgspec` is cp315-clean core-direct, so `Hlc`/`ElementId`/`CausalFrame` impose no gated band — distinct from the `xxhash`/`lz4` `python_version<'3.15'` companion legs the evidence and wire pages inherit.
- [GC_FALSE_LEAVES]: [COMPLETE] — `Hlc`/`ElementId` carry only `int`/`bytes` leaf fields and no container reference, so `gc=False` removes them from the cyclic collector's reachable set without a leak risk; `CausalFrame` holds the `Hlc` struct reference and the `Tenant` newtype so it stays GC-tracked, the `gc=False` applied only to the leaf cells the high-volume op-log decode allocates per arm.
