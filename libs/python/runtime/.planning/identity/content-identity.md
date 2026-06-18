# [PY_RUNTIME_CONTENT_IDENTITY]

The single content-addressing owner the whole branch consumes. `ContentIdentity` derives one XxHash128 key over canonical bytes, reproducing the C# `System.IO.Hashing.XxHash128` seed with format, deflection, and tolerance folded into the key so a re-tessellation at identical settings is a cache hit by reference. This collapses the former parallel content owners — data `ExchangeBundle`, artifacts `ContentDigest`/`ArtifactBundle`, the companion GLB key — into one; data, geometry, and artifacts consume it and never re-mint.

## [1]-[INDEX]

One cluster: `[2]-[IDENTITY]` — the XxHash128 content key, the settings-folded seed, the value object, the one input-discriminated `of` entrypoint.

## [2]-[IDENTITY]

- Owner: `ContentIdentity` — the static surface deriving the content key; `ContentKey` the value object carrying the 128-bit identity, the format tag, and the byte length the receipt and cache contract read; `IdentityPolicy` the frozen evaluation policy folded into the seed.
- Entry: `ContentIdentity.of` is the one polymorphic content-key derivation discriminating on the source value — `bytes` keys a whole payload, an `Iterable[bytes]` streams chunks through the `xxh3_128` updater, and a `tuple[ContentKey, ...]` folds children into one Merkle parent under the same seed algebra; identity never derives from a path or filename, the modality is recovered from the value shape, never a name suffix or mode flag.
- Auto: `ContentIdentity.seed` folds the format key, deflection, tolerance, and angle tolerance into a 64-bit seed so a coarse and a fine tessellation of one input key distinctly and a re-import of identical bytes at identical settings keys identically; the children fold serializes each child's 128-bit value big-endian before re-hashing so the parent key is order-sensitive over its parts.
- Auto: `ContentKey.hex` renders the `:x32` form the C# `ArtifactKey` contract expects (`{value:032x}:{fmt}`), so a companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key` output and a cache hit crosses by reference; `xxhash.xxh3_128_intdigest` over canonical bytes under the 64-bit seed is the digest the C# seam reproduces, the byte-order parity fixed at the `[XXHASH_PARITY]` research seam.
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
                spine = b"".join(child.value.to_bytes(16, "big") for child in children)
                digest = xxhash.xxh3_128_intdigest(spine, seed=seed)
                return ContentKey(value=digest, fmt=fmt, byte_length=sum(child.byte_length for child in children))
            case chunks:
                digest = xxhash.xxh3_128(seed=seed)
                length = sum((digest.update(chunk) or len(chunk)) for chunk in chunks)
                return ContentKey(value=digest.intdigest(), fmt=fmt, byte_length=length)
```

## [3]-[RESEARCH]

- [XXHASH_PARITY]: `xxhash` is manifest-declared (`xxhash>=3.7.0`, abi3 wheel) but carries no `.api/` catalogue and is uncaptured on this host; the streaming `xxh3_128(seed=...).update(...).intdigest()` surface, the `xxh3_128_intdigest`/`xxh3_64_intdigest` seed-keyword spellings, and the byte-order parity against the C# `System.IO.Hashing.XxHash128`/`XxHash3` digest confirm against the `xxhash` catalogue at capture; the `to_bytes(16, "big")` child serialization is fixed to the .NET digest endianness at the same seam.
