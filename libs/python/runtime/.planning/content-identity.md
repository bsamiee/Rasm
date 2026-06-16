# [PY_RUNTIME_CONTENT_IDENTITY]

The single content-addressing owner the whole branch consumes. `ContentIdentity` derives one XxHash128 key over canonical bytes, reproducing the C# `InterchangeIdentity` seed with deflection and tolerance folded into the key so a re-tessellation at identical settings is a cache hit by reference. This collapses the three former parallel content owners (data `ExchangeBundle`, artifacts `ContentDigest`/`ArtifactBundle`, the companion GLB key) into one; data, geometry, and artifacts consume it and never re-mint.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                            |
| :-----: | :-------- | :--------------------------------------------------------------- |
|   [1]   | IDENTITY  | the XxHash128 content key, the settings-folded seed, the value object |

## [2]-[IDENTITY]

- Owner: `ContentIdentity` — the static surface deriving the content key; `ContentKey` the value object carrying the 128-bit identity, the format tag, and the byte length the receipt and cache contract read.
- Entry: `ContentIdentity.key` is a pure value — identity derives from the bytes and the evaluation policy, never from a path or filename; `ContentIdentity.seed` folds the format key, deflection, tolerance, and angle tolerance into the seed so a coarse and a fine tessellation of the same input key distinctly and a re-import of identical bytes at identical settings key identically.
- Auto: `ContentKey.hex` renders the `:x32` form the C# `ArtifactKey` contract expects (`{key:x32}:{format}`), so the companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key` output and a cache hit crosses by reference.
- Packages: the admitted XxHash128 distribution computing the 128-bit digest with a 64-bit seed over canonical bytes, `msgspec`. The exact distribution name lands at admission per the suite TASKLOG XxHash128 row; the C# side uses `System.IO.Hashing.XxHash128`, and the Python owner reproduces that exact digest, so a byte-and-seed-compatible XxHash128 binding is the admission requirement.
- Growth: a new evaluation parameter that changes the artifact is one field folded into `seed`; zero new surface, no second hashing pass.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law the C# `InterchangeIdentity` and `remote-lane#ARTIFACT_FRAMES` whole-artifact identity already hold; a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit are the named defects; data/geometry/artifacts consume this owner, the C# `InterchangeIdentity` is the cross-boundary mechanics owner the seed reproduces.

```python signature
from typing import Final, Protocol

from msgspec import Struct


class Xxh128(Protocol):
    def intdigest(self) -> int: ...


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


class ContentIdentity:
    @staticmethod
    def seed(fmt: str, policy: IdentityPolicy) -> int:
        spec = f"{fmt}|{policy.deflection:.17g}|{policy.tolerance:.17g}|{policy.angle_tolerance:.17g}"
        return _digest64(spec.encode())

    @classmethod
    def key(cls, fmt: str, payload: bytes, policy: IdentityPolicy = CANONICAL_POLICY) -> ContentKey:
        return ContentKey(value=_digest128(payload, seed=cls.seed(fmt, policy)), fmt=fmt, byte_length=len(payload))
```

## [3]-[RESEARCH]

- [XXHASH_ADMISSION]: an XxHash128 distribution reproducing the C# `System.IO.Hashing.XxHash128.HashToUInt128(bytes, seed)` digest byte-identically is not yet pinned in the root manifest; `_digest128`/`_digest64` bind to that distribution at admission (suite TASKLOG XxHash128 row). The seed-derivation spelling (a 64-bit XxHash3 digest over the format/deflection/tolerance string) confirms against the C# `InterchangeIdentity.Seed` at the cross-boundary alignment, since a cross-setting hit must never collide.
