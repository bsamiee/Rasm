# [PY_RUNTIME_CONTENT_IDENTITY]

The single content-addressing owner the whole branch consumes. `ContentIdentity` derives one XxHash128 key over canonical bytes, reproducing the C# `System.IO.Hashing.XxHash128` seed with format, deflection, and tolerance folded into the key so a re-tessellation at identical settings is a cache hit by reference. This collapses the former parallel content owners — data `ExchangeBundle`, artifacts `ContentDigest`/`ArtifactBundle`, the companion GLB key — into one; data, geometry, and artifacts consume it and never re-mint.

## [1]-[INDEX]

- `[2]-[IDENTITY]` — the XxHash128 content key, the settings-folded seed, the value object, the one input-discriminated `of` entrypoint.
- `[3]-[SEED_REPRODUCTION]` — the Python-side reproduction binding asserting `ContentIdentity` reproduces the C# `XxHash128` seed bit-identically against the frozen `ONE_WIRE_FIXTURE_CORPUS`.

## [2]-[IDENTITY]

- Owner: `ContentIdentity` — the static surface deriving the content key; `ContentKey` the value object carrying the 128-bit identity, the format tag, and the byte length the receipt and cache contract read; `IdentityPolicy` the frozen evaluation policy folded into the seed.
- Entry: `ContentIdentity.of` is the one polymorphic content-key derivation discriminating on the source value — `bytes` keys a whole payload, an `Iterable[bytes]` streams chunks through the `xxh3_128` updater, and a `tuple[ContentKey, ...]` folds children into one Merkle parent under the same seed algebra; identity never derives from a path or filename, the modality is recovered from the value shape, never a name suffix or mode flag.
- Auto: `ContentIdentity.seed` folds the format key, deflection, tolerance, and angle tolerance into a 64-bit seed so a coarse and a fine tessellation of one input key distinctly and a re-import of identical bytes at identical settings keys identically; the children fold serializes each child's 128-bit value LITTLE-endian before re-hashing so the parent key is order-sensitive over its parts and reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `Rasm.Persistence/versioning#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold over before `XxHash128.HashToUInt128`.
- Auto: `ContentKey.hex` renders the `:x32` form the C# `ArtifactKey` contract expects (`{value:032x}:{fmt}`), so a companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key` output and a cache hit crosses by reference; `xxhash.xxh3_128_intdigest` over canonical bytes under the 64-bit seed is the digest the C# `System.IO.Hashing.XxHash128.HashToUInt128` seam reproduces, the child-key little-endian transcription matching the C# `WriteUInt128LittleEndian` writer and the digest-endianness parity fixed at the `[XXHASH_PARITY]` research seam.
- Packages: `xxhash` (`xxh3_128_intdigest`, `xxh3_64_intdigest`, streaming `xxh3_128`), `msgspec`.
- Growth: a new evaluation parameter that changes the artifact is one field folded into `seed`; zero new surface, no second hashing pass.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law the C# `System.IO.Hashing.XxHash128` and the whole-artifact identity rail already hold; a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit are the named defects; data/geometry/artifacts consume this owner, the C# `InterchangeIdentity` is the cross-boundary mechanics owner the seed reproduces.

```python signature
from collections.abc import Iterable
from typing import Final

import xxhash
from msgspec import Struct


class ContentKey(Struct, frozen=True):
    value: int
    fmt: str
    byte_length: int

    @property
    def hex(self) -> str:
        return f"{self.value:032x}:{self.fmt}"


class IdentityPolicy(Struct, frozen=True):
    deflection: float = 0.01
    tolerance: float = 1e-6
    angle_tolerance: float = 1e-4


CANONICAL_POLICY: Final[IdentityPolicy] = IdentityPolicy()


type Source = bytes | Iterable[bytes] | tuple[ContentKey, ...]


class ContentIdentity:
    @staticmethod
    def seed(fmt: str, policy: IdentityPolicy) -> int:
        spec = f"{fmt}|{policy.deflection:.17g}|{policy.tolerance:.17g}|{policy.angle_tolerance:.17g}"
        return xxhash.xxh3_64_intdigest(spec.encode())

    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = CANONICAL_POLICY) -> ContentKey:
        seed = cls.seed(fmt, policy)
        match source:
            case bytes() as payload:
                return ContentKey(value=xxhash.xxh3_128_intdigest(payload, seed=seed), fmt=fmt, byte_length=len(payload))
            case tuple() as children if all(isinstance(child, ContentKey) for child in children):
                spine = b"".join(child.value.to_bytes(16, "little") for child in children)
                digest = xxhash.xxh3_128_intdigest(spine, seed=seed)
                return ContentKey(value=digest, fmt=fmt, byte_length=sum(child.byte_length for child in children))
            case chunks:
                digest = xxhash.xxh3_128(seed=seed)
                length = sum((digest.update(chunk) or len(chunk)) for chunk in chunks)
                return ContentKey(value=digest.intdigest(), fmt=fmt, byte_length=length)
```

## [3]-[SEED_REPRODUCTION]

- Owner: `SeedReproduction` — the Python-side reproduction binding asserting `ContentIdentity` reproduces the one C#-owned `XxHash128` seed bit-identically, read against the FROZEN `csharp:Rasm/Geometry/topology/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [1] `CANONICAL_BYTE_IDENTITY`. The corpus is the single mint (seed zero, two-64-bit-half order); this binding re-mints no digest and authors no fixture byte — it transcribes the producer-frozen reference verbatim and fixes the Python assertion the harness driver verifies.
- Reference: the FROZEN 52-byte int32-LE canonical-adjacency stream of the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00`, and its `XxHash128.HashToUInt128` digest under seed zero is `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, persisted big-endian and read C#-side as the 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`. No byte is re-authored here; the literals are the producer's frozen reference.
- Entry: `SeedReproduction.canonical_byte_identity` asserts `xxhash.xxh3_128_intdigest(CANONICAL_STREAM, seed=0) == 0x9462A71A5DD13DCFA3B1D6D225FCBE70` so the whole-payload modality of `ContentIdentity.of` over the corpus stream reproduces the C# digest by value; `SeedReproduction.little_endian_memory` asserts the digest serialized through `value.to_bytes(16, "little")` equals the C# 16-byte LE memory, fixing the endianness the `to_bytes(16, "little")` child-fold and the `CrdtWire.ContentKey`/`MerkleRange.Of` little-endian writer share.
- Auto: `xxhash.xxh3_128_intdigest` returns the 128-bit digest as a Python `int` whose `to_bytes(16, "little")` is the C# `UInt128` in-memory layout — the C# `XxHash128.HashToUInt128` returns a `UInt128` and `BinaryPrimitives.WriteUInt128LittleEndian` writes the same LE memory, so the value-equality holds with no byte-swap when both sides read seed zero; the corpus seed is zero because the C# `NamingHashOps.Encode`/`GeometryHash` path calls `XxHash128.HashToUInt128(span)` with no seed parameter, distinct from the settings-folded `ContentIdentity.seed` the tessellation rail derives, so the `CANONICAL_BYTE_IDENTITY` parity reads the bare seed-zero path and the format-policy seed governs only the re-tessellation cache identity.
- Packages: `xxhash` (`xxh3_128_intdigest`), `msgspec`.
- Boundary: the corpus is read-only and the C# seed is the single mint — a Python-side re-derived fixture, a per-runtime digest function, and a fabricated byte set for an unpinned corpus row are the named drift defects; the harness DRIVER feeding the corpus stream through `ContentIdentity.of` and grading the equality is a future `python:testing` consumer of the same corpus, never a second fixture store here; the corpus rows [3]-[6] (`FAULT_TRIPLES`, `CRDT_OP_SET`, `GLB_BY_KEY`, `HLC_TWO_HALF`) stay DESIGN-PIN on their cross-folder producers and carry no fabricated bytes in this binding.

```python signature
from typing import Final

import xxhash


CANONICAL_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
CANONICAL_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
CANONICAL_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")


class SeedReproduction:
    @staticmethod
    def canonical_byte_identity() -> bool:
        return xxhash.xxh3_128_intdigest(CANONICAL_STREAM, seed=0) == CANONICAL_DIGEST

    @staticmethod
    def little_endian_memory() -> bool:
        return CANONICAL_DIGEST.to_bytes(16, "little") == CANONICAL_LE_MEMORY
```

## [4]-[RESEARCH]

- [XXHASH_PARITY]: [UPSTREAM-BLOCKED] — `xxhash` is manifest-declared (`xxhash>=3.7.0`) with per-version `cp38-cp314` wheels and NO `abi3`/`cp315` wheel synced, so it does not import on the cp315 core on this host. Reflection-confirmed on the companion `python_version<'3.15'` band: the streaming `xxh3_128(seed=...).update(...).intdigest()` surface, the `xxh3_128_intdigest`/`xxh3_64_intdigest` seed-keyword spellings, and the digest-endianness parity against the C# `System.IO.Hashing.XxHash128.HashToUInt128` are the captured spellings; the `to_bytes(16, "little")` child serialization matches the C# `BinaryPrimitives.WriteUInt128LittleEndian` writer the `CommitGraph.Of`/`MerkleRange.Of`/`CrdtWire.ContentKey` fold uses, and the `[3]-[SEED_REPRODUCTION]` `CANONICAL_BYTE_IDENTITY` assertion confirms the seed-zero digest value `0x9462A71A5DD13DCFA3B1D6D225FCBE70` on the companion interpreter. The cp315 fence is the single install-gated link — the design (the little-endian child transcription, the seed-zero corpus parity, the streaming/whole/Merkle modalities) is fully resolved and the only absent dependency is the cp315/abi3 `xxhash` wheel, never fabricated as present.
