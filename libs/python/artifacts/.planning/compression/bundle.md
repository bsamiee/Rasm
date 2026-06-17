# [PY_ARTIFACTS_BUNDLE]

The artifact-bundling and compression owner. `Bundle` packs any emitted artifact into content-addressed compressed bytes; `CompressionAlgo` is ONE algorithm-row owner collapsing zstandard, lz4, brotli, and py7zr — never a per-algorithm class family. The packed bundle keys by the runtime content key over the compressed bytes, so a bundle of identical inputs at an identical algorithm is a cache hit by reference. This is bundling, not visualization; it owns no artifact production, only the compression close over already-emitted bytes.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[COMPRESSION]`, the algorithm-row compression and content-addressed bundle owner.

## [2]-[COMPRESSION]

- Owner: `Bundle` the one bundle owner; `CompressionAlgo` the closed `StrEnum` discriminating algorithm; the algorithm package is bound per row, never an `if zstd` branch.
- Cases: `CompressionAlgo` rows `ZSTD` (zstandard `ZstdCompressor`) · `LZ4` (lz4 `frame`) · `BROTLI` (brotli `compress`) · `SEVEN_Z` (py7zr `SevenZipFile`) — matched by `match`/`case`, each binding its algorithm package.
- Entry: `Bundle.pack` compresses the payload through the selected algorithm and returns a `RuntimeRail[ContentKey]` keyed by the content key over the compressed bytes; the SEVEN_Z arm writes through an in-memory `SevenZipFile` sink.
- Receipt: each pack contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Bundle` carrying the content key and the compressed byte count.
- Packages: `zstandard` (`ZstdCompressor`), `lz4` (`frame.compress`), `brotli` (`compress`/`MODE_GENERIC`), `py7zr` (`SevenZipFile`), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new algorithm is one `CompressionAlgo` row plus one acceptor arm; zero new surface.
- Boundary: a per-algorithm compression class family is the deleted form; this owner is content-addressed bundling over already-emitted artifact bytes and owns no artifact production; cp315-reflected backends.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

import brotli
import lz4.frame
import py7zr
import zstandard
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    SEVEN_Z = "7z"


class Bundle(Struct, frozen=True):
    payload: bytes
    algo: CompressionAlgo

    def pack(self) -> RuntimeRail[ContentKey]:
        return boundary(f"bundle.{self.algo}", self._emit)

    def _emit(self) -> ContentKey:
        data = _compress(self.payload, self.algo)
        return ContentIdentity.key(f"bundle-{self.algo}", data)


def _compress(payload: bytes, algo: CompressionAlgo) -> bytes:
    match algo:
        case CompressionAlgo.ZSTD:
            return zstandard.ZstdCompressor().compress(payload)
        case CompressionAlgo.LZ4:
            return lz4.frame.compress(payload, content_checksum=1)
        case CompressionAlgo.BROTLI:
            return brotli.compress(payload, mode=brotli.MODE_GENERIC, quality=11)
        case CompressionAlgo.SEVEN_Z:
            sink = BytesIO()
            with py7zr.SevenZipFile(sink, mode="w") as archive:
                archive.writef(BytesIO(payload), "payload")
            return sink.getvalue()
        case _:
            assert_never(algo)
```
