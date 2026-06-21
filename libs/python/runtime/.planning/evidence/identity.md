# [PY_RUNTIME_IDENTITY]

The single content-addressing owner the whole branch consumes. `ContentIdentity` derives one XxHash128 key over canonical bytes, reproducing the C# `System.IO.Hashing.XxHash128` seed with format, deflection, and tolerance folded into the key so a re-tessellation at identical settings is a cache hit by reference. This collapses the former parallel content owners — data `ExchangeBundle`, artifacts `ContentDigest`/`ArtifactBundle`, the companion GLB key — into one; data, geometry, and artifacts consume it and never re-mint. The page weaves five admitted surfaces as ONE rail — `expression` (the `IdentitySource` `@tagged_union` modality ADT, the `Block` Merkle fold), `msgspec` (the `Meta`-width-bounded value object, the canonical-bytes `Encoder`), `xxhash` (the seeded XXH3 digest), `opentelemetry-api` (the derivation span the reuse-fabric cache reads), and the runtime `reliability/faults#FAULT` `boundary` (the one `RuntimeRail` lift over the fallible canonical-encode seam) — never a flat one-shot digest call, never an open `Source` catch-all, never a derivation the OTel signal stream cannot see, never an `EncodeError` escaping the fault rail.

## [01]-[INDEX]

- [01]-[IDENTITY]: the `Meta`-bounded XxHash128 content key, the settings-folded seed, the closed `IdentitySource` modality ADT, the `KeyView` output projection, the one input-and-output-parameterized `of` rail under a derivation span.
- [02]-[SEED_REPRODUCTION]: the data-driven `ParityAspect` corpus-parity table folded into typed `ParityReceipt` rows the `ReceiptContributor` port contributes, asserting `ContentIdentity` reproduces the C# `XxHash128` seed bit-identically against the frozen `ONE_WIRE_FIXTURE_CORPUS`.

## [02]-[IDENTITY]

- Owner: `ContentIdentity` — the static surface deriving the content key under a derivation span; `ContentKey` the value object carrying the `Meta`-width-bounded 128-bit identity, the format tag, and the byte length the receipt and cache contract read, with the polymorphic `project` rendering every consumer view; `IdentityPolicy` the frozen evaluation policy folded into the seed; `IdentitySource` the closed `@tagged_union` modality ADT (`whole`/`stream`/`merkle`) the open `Source` shapes lift into so the dispatch is total; `KeyView` the output-projection axis over `ContentKey`.
- Entry: `ContentIdentity.of` is the one polymorphic content-key derivation parameterized over BOTH input shape and output projection — `fmt` stays a free-form interpolated tag (`f"bundle-{algo}"`, `f"preview-{tag}"`) the callers build, `source` admits the raw `bytes`/`Iterable[bytes]`/`tuple[ContentKey, ...]` shapes the call sites pass and `_lift` folds into the closed `IdentitySource` ADT so the interior `match` is total over three named arms (`whole` keys a whole payload, `stream` folds chunks through the `xxh3_128` updater, `merkle` folds children into one parent), an `assert_never` closing the union so an off-shape value is a typed build failure rather than the prior open `case chunks:` catch-all that silently mis-keyed a stray `str`; `view` selects the output projection so one entrypoint returns the `ContentKey` value object, its `hex` render, its little-endian memory, or its raw `int` without a second method per render; `seed` is the `Option[U64]` override that defaults `Nothing` to the policy-folded settings seed (the production tessellation path) and reads `Some(0)` for the bare C# `XxHash128.HashToUInt128(span)` seed-zero path the `GeometryHash`/`NamingHashOps` boundary mints, so the seed source is one parameter rather than a fake policy. Identity never derives from a path or filename — the modality is recovered from the value shape, never a name suffix or mode flag.
- Auto: `ContentIdentity.seed` folds the format key, deflection, tolerance, and angle tolerance into a `Meta`-bounded 64-bit seed so a coarse and a fine tessellation of one input key distinctly and a re-import of identical bytes at identical settings keys identically; the `merkle` arm folds each child's 128-bit value LITTLE-endian through an `expression` `Block.fold` over the child spine before re-hashing so the parent key is order-sensitive over its parts and reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold over before `XxHash128.HashToUInt128`; the whole derivation runs inside the `opentelemetry-api` `content.derive` span carrying the `fmt` and modality `tag` attributes so the reuse-fabric cache hit/miss the `execution/lanes#LANE` `Map[ContentKey, T]` keys is one continuous trace from key mint through cache probe, never a silent untraced hash. `ContentIdentity.of_canonical` is the `Struct`-payload entrypoint realizing the `msgspec` content-identity admission law — the cached `_ENCODER` (one `msgspec.msgpack.Encoder(order="deterministic")`) lowers the payload to its canonical wire bytes and feeds the `whole` modality so a `Struct` keys off its deterministic wire form rather than in-memory identity, one `encode` into one seeded digest, the identity a deterministic function of the canonical bytes the wire codec also emits; the encode is the one fallible step, so `of_canonical` returns `RuntimeRail[ContentKey]` lifting a `msgspec.EncodeError` through the runtime `boundary("identity", ...)` exactly once rather than letting it escape, while the pure `of` over an already-`bytes` source stays a total non-railed derivation.
- Auto: `ContentKey.project` is the one polymorphic output render discriminating on the `KeyView` literal axis — `"value"` returns the key itself, `"hex"` renders the `:x32` form the C# `ArtifactKey` contract expects (`{value:032x}:{fmt}`) so a companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key` output and a cache hit crosses by reference, `"memory"` returns the `value.to_bytes(16, "little")` 16-byte LE span the C# `UInt128` in-memory layout and the `merkle` child transcription share, and `"digest"` the raw `int` — so the `hex` property the `artifacts/receipt#RECEIPT` fold reads, the LE memory the `[SEED_REPRODUCTION]` parity asserts, and the int the cache key compares are one projection axis closed by `assert_never`, never four ad-hoc renders scattered across call sites; `hex` survives as the thin property delegating to `project("hex")` so the downstream `ContentKey.hex` contract is unbroken. `xxhash.xxh3_128_intdigest` over canonical bytes under the 64-bit seed is the digest the C# `System.IO.Hashing.XxHash128.HashToUInt128` seam reproduces, the child-key little-endian transcription matching the C# `WriteUInt128LittleEndian` writer and the digest-endianness parity fixed at the `[XXHASH_PARITY]` research seam.
- Packages: `expression` (`tagged_union`/`tag`/`case` the `IdentitySource` modality ADT, `Block.of_seq`/`fold` the Merkle spine fold, `Option`/`Nothing` the seed override), `msgspec` (`Struct` the frozen value objects, `Meta` the digest/seed width constraints, `msgpack.Encoder` the cached canonical-bytes codec), `xxhash` (`xxh3_128_intdigest`, `xxh3_64_intdigest`, streaming `xxh3_128`), `opentelemetry-api` (`trace.get_tracer`, `Tracer.start_as_current_span`, `Span.set_attribute` the derivation span); runtime `reliability/faults#FAULT` (`boundary`/`RuntimeRail` the canonical-encode fault lift).
- Growth: a new evaluation parameter that changes the artifact is one field folded into `seed`; a new output render is one `KeyView` member plus one `project` arm reaching every call site through the existing projection; a new input modality is one `IdentitySource` arm plus one `_lift` shape plus one `match` arm; a distinct seed origin (a second peer boundary's bare-path seed) is one `Some(value)` at the call site through the existing `seed` override; zero new entrypoint, no second hashing pass, no per-render method.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law the C# `System.IO.Hashing.XxHash128` and the whole-artifact identity rail already hold; a path-keyed identity, a second hashing owner per package, a cross-setting cache hit, an open `Source` catch-all that mis-keys an off-shape value, a per-render projection method, and an untraced derivation the cache cannot correlate are the named defects; data/geometry/artifacts consume this owner through the unbroken `of`/`ContentKey`/`hex` surface, the C# `InterchangeIdentity` is the cross-boundary mechanics owner the seed reproduces.

```python signature
from collections.abc import Iterable
from typing import Annotated, Final, Literal, assert_never, overload

import xxhash
from expression import Nothing, Option, case, tag, tagged_union
from expression.collections import Block
from msgspec import Meta, Struct
from msgspec.msgpack import Encoder
from opentelemetry import trace

from rasm.runtime.faults import RuntimeRail, boundary


type U128 = Annotated[int, Meta(ge=0, lt=2**128)]
type U64 = Annotated[int, Meta(ge=0, lt=2**64)]
type KeyView = Literal["value", "hex", "memory", "digest"]
type KeyRender = "ContentKey | str | bytes | int"


class ContentKey(Struct, frozen=True):
    value: U128
    fmt: str
    byte_length: int

    @overload
    def project(self, view: Literal["value"] = ..., /) -> "ContentKey": ...
    @overload
    def project(self, view: Literal["hex"], /) -> str: ...
    @overload
    def project(self, view: Literal["memory"], /) -> bytes: ...
    @overload
    def project(self, view: Literal["digest"], /) -> int: ...
    def project(self, view: KeyView = "value", /) -> KeyRender:
        match view:
            case "hex":
                return f"{self.value:032x}:{self.fmt}"
            case "memory":
                return self.value.to_bytes(16, "little")
            case "digest":
                return self.value
            case "value":
                return self
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def hex(self) -> str:
        return self.project("hex")


class IdentityPolicy(Struct, frozen=True):
    deflection: float = 0.01
    tolerance: float = 1e-6
    angle_tolerance: float = 1e-4


@tagged_union(frozen=True)
class IdentitySource:
    tag: Literal["whole", "stream", "merkle"] = tag()
    whole: bytes = case()
    stream: tuple[bytes, ...] = case()
    merkle: tuple[ContentKey, ...] = case()


type Source = bytes | Iterable[bytes] | tuple[ContentKey, ...]


CANONICAL_POLICY: Final[IdentityPolicy] = IdentityPolicy()

_ENCODER: Final[Encoder] = Encoder(order="deterministic")
_TRACER: Final = trace.get_tracer("rasm.runtime.content_identity")


class ContentIdentity:
    @staticmethod
    def seed(fmt: str, policy: IdentityPolicy) -> U64:
        spec = f"{fmt}|{policy.deflection:.17g}|{policy.tolerance:.17g}|{policy.angle_tolerance:.17g}"
        return xxhash.xxh3_64_intdigest(spec.encode())

    @staticmethod
    def _lift(source: Source) -> IdentitySource:
        match source:
            case bytes() as payload:
                return IdentitySource(whole=payload)
            case tuple() as parts if all(isinstance(part, ContentKey) for part in parts):
                return IdentitySource(merkle=parts)
            case Iterable() as chunks if not isinstance(chunks, (str, bytes, bytearray)):
                return IdentitySource(stream=tuple(chunks))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _key(lifted: IdentitySource, fmt: str, seed: U64) -> ContentKey:
        match lifted:
            case IdentitySource(tag="whole", whole=payload):
                return ContentKey(value=xxhash.xxh3_128_intdigest(payload, seed=seed), fmt=fmt, byte_length=len(payload))
            case IdentitySource(tag="merkle", merkle=children):
                spine = Block.of_seq(children).fold(lambda acc, child: acc + child.value.to_bytes(16, "little"), b"")
                length = Block.of_seq(children).fold(lambda acc, child: acc + child.byte_length, 0)
                return ContentKey(value=xxhash.xxh3_128_intdigest(spine, seed=seed), fmt=fmt, byte_length=length)
            case IdentitySource(tag="stream", stream=chunks):
                digest = xxhash.xxh3_128(seed=seed)
                length = Block.of_seq(chunks).fold(lambda acc, chunk: (digest.update(chunk), acc + len(chunk))[1], 0)
                return ContentKey(value=digest.intdigest(), fmt=fmt, byte_length=length)
            case _ as unreachable:
                assert_never(unreachable)

    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["value"] = ..., seed: Option[U64] = ...) -> ContentKey: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["hex"], seed: Option[U64] = ...) -> str: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["memory"], seed: Option[U64] = ...) -> bytes: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["digest"], seed: Option[U64] = ...) -> int: ...
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = CANONICAL_POLICY, *, view: KeyView = "value", seed: Option[U64] = Nothing) -> KeyRender:
        lifted = cls._lift(source)
        with _TRACER.start_as_current_span("content.derive") as span:
            span.set_attribute("identity.fmt", fmt)
            span.set_attribute("identity.modality", lifted.tag)
            resolved = seed.default_with(lambda: cls.seed(fmt, policy))
            key = cls._key(lifted, fmt, resolved)
            span.set_attribute("identity.key", key.value)
            return key.project(view)

    @classmethod
    def of_canonical[S: Struct](cls, fmt: str, payload: S, policy: IdentityPolicy = CANONICAL_POLICY) -> RuntimeRail[ContentKey]:
        return boundary("identity", lambda: cls.of(fmt, _ENCODER.encode(payload), policy))
```

## [03]-[SEED_REPRODUCTION]

- Owner: `SeedReproduction` — the Python-side reproduction binding asserting `ContentIdentity` reproduces the one C#-owned `XxHash128` seed bit-identically, read against the FROZEN `csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [1] `CANONICAL_BYTE_IDENTITY`; `ParityAspect` the closed `Literal` parity-aspect vocabulary (`value_identity`/`memory_layout`) the corpus row's two assertions collapse into; `ParityReceipt` the typed evidence row a graded aspect yields, carrying the aspect, the expected/observed renders, and the verdict; `_PARITY_TABLE` the data table pairing each `ParityAspect` with the `KeyView` projector that derives its observed render and the frozen expected reference. The corpus is the single mint (seed zero, two-64-bit-half order); this binding re-mints no digest and authors no fixture byte — it transcribes the producer-frozen reference verbatim and folds the Python assertion the harness driver verifies through one table rather than a method per assertion.
- Reference: the FROZEN 52-byte int32-LE canonical-adjacency stream of the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00`, and its `XxHash128.HashToUInt128` digest under seed zero is `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, persisted big-endian and read C#-side as the 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`. No byte is re-authored here; the literals are the producer's frozen reference.
- Entry: `SeedReproduction.grade` is the one parity entrypoint folding the `_PARITY_TABLE` rows into a `Block[ParityReceipt]` — each row derives the corpus `ContentKey` once through the whole-payload modality of `ContentIdentity.of` under the explicit `seed=Some(0)` override (the bare C# `HashToUInt128(span)` path, not a fabricated policy), projects the observed render through the `KeyView` the row carries (`value_identity` reads `"digest"` against `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, `memory_layout` reads `"memory"` against the C# 16-byte LE memory), binds the projection once through a walrus, and grades equality into a `ParityReceipt`, so the digest-value and little-endian-memory checks are two rows of one fold rather than two parallel boolean methods; a `SeedReproduction` instance satisfies the `observability/receipts#RECEIPT` `ReceiptContributor` Protocol through its `contribute`, folding the graded rows into one `emitted`-phase `Receipt.of` fact map keyed by the corpus `fmt` so a parity run is structured evidence on the one receipt stream the metrics/lanes fold also reads, never a bare assert.
- Auto: `xxhash.xxh3_128_intdigest` returns the 128-bit digest as a Python `int` whose `to_bytes(16, "little")` (the `"memory"` projection the value object owns) is the C# `UInt128` in-memory layout — the C# `XxHash128.HashToUInt128` returns a `UInt128` and `BinaryPrimitives.WriteUInt128LittleEndian` writes the same LE memory, so the value-equality holds with no byte-swap when both sides read seed zero; the corpus seed is zero because the C# `NamingHashOps.Encode`/`GeometryHash` path calls `XxHash128.HashToUInt128(span)` with no seed parameter, distinct from the settings-folded `ContentIdentity.seed` the tessellation rail derives, so the `CANONICAL_BYTE_IDENTITY` parity reads the bare seed-zero path through the `of` `seed=Some(0)` seam and the format-policy seed governs only the re-tessellation cache identity; the parity reuses the production `ContentIdentity.of` and `ContentKey.project` rather than a re-derived digest function, so a regression in the owner's derivation or projection surfaces as a failed `ParityReceipt` rather than passing against a parallel fixture path.
- Packages: `xxhash` (`xxh3_128_intdigest`), `expression` (`Block.of_seq`/`map` the parity-table fold, `Some` the seed override), `msgspec` (`Struct` the `ParityReceipt` row); runtime (`content_identity.ContentIdentity`/`ContentKey`/`KeyView`, `receipts.Receipt`/`ReceiptContributor`).
- Growth: a new corpus parity aspect is one `ParityAspect` member plus one `_PARITY_TABLE` row pairing it with its `KeyView` projector and frozen reference, reaching the fold and the contributed receipt through the existing `grade`; a new corpus row promoted from DESIGN-PIN is one more `_PARITY_TABLE` entry; zero new method, no parallel boolean.
- Boundary: the corpus is read-only and the C# seed is the single mint — a Python-side re-derived fixture, a per-runtime digest function, a parallel boolean method per assertion, and a fabricated byte set for an unpinned corpus row are the named drift defects; the harness DRIVER feeding the corpus stream through `ContentIdentity.of` and grading the `ParityReceipt` rows is a future `python:testing` consumer of the same corpus, never a second fixture store here; the corpus rows [3]-[6] (`FAULT_TRIPLES`, `CRDT_OP_SET`, `GLB_BY_KEY`, `HLC_TWO_HALF`) stay DESIGN-PIN on their cross-folder producers and carry no fabricated bytes in this binding.

```python signature
from typing import Final, Literal

from expression import Some
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, KeyView
from rasm.runtime.receipts import Receipt


type ParityAspect = Literal["value_identity", "memory_layout"]


IDENTITY_FMT: Final[str] = "geometry-topology"
CANONICAL_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
CANONICAL_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
CANONICAL_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")


class ParityReceipt(Struct, frozen=True):
    aspect: ParityAspect
    expected: str
    observed: str
    verified: bool


_PARITY_TABLE: Final[Block[tuple[ParityAspect, KeyView, object]]] = Block.of_seq(
    (
        ("value_identity", "digest", CANONICAL_DIGEST),
        ("memory_layout", "memory", CANONICAL_LE_MEMORY),
    )
)


class SeedReproduction:
    @staticmethod
    def grade() -> Block[ParityReceipt]:
        key: ContentKey = ContentIdentity.of(IDENTITY_FMT, CANONICAL_STREAM, view="value", seed=Some(0))
        return _PARITY_TABLE.map(
            lambda row: ParityReceipt(
                aspect=row[0],
                expected=repr(row[2]),
                observed=repr(observed := key.project(row[1])),
                verified=observed == row[2],
            )
        )

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            row.aspect: ("ok" if row.verified else f"{row.observed}!={row.expected}") for row in self.grade()
        }
        return (Receipt.of("content_identity", ("emitted", IDENTITY_FMT, facts)),)
```

## [04]-[RESEARCH]

- [XXHASH_PARITY]: [UPSTREAM-BLOCKED] — `xxhash` is manifest-declared (`xxhash>=3.7.0`) with per-version `cp38-cp314` wheels and NO `abi3`/`cp315` wheel synced, so it does not import on the cp315 core on this host. Reflection-confirmed on the companion `python_version<'3.15'` band: the streaming `xxh3_128(seed=...).update(...).intdigest()` surface, the `xxh3_128_intdigest`/`xxh3_64_intdigest` seed-keyword spellings, and the digest-endianness parity against the C# `System.IO.Hashing.XxHash128.HashToUInt128` are the captured spellings; the `to_bytes(16, "little")` child serialization matches the C# `BinaryPrimitives.WriteUInt128LittleEndian` writer the `CommitGraph.Of`/`MerkleRange.Of`/`CrdtWire.ContentKey` fold uses, and the `[3]-[SEED_REPRODUCTION]` `CANONICAL_BYTE_IDENTITY` assertion confirms the seed-zero digest value `0x9462A71A5DD13DCFA3B1D6D225FCBE70` on the companion interpreter. The cp315 fence is the single install-gated link — the design (the little-endian child transcription, the seed-zero corpus parity, the streaming/whole/Merkle modalities) is fully resolved and the only absent dependency is the cp315/abi3 `xxhash` wheel, never fabricated as present.
