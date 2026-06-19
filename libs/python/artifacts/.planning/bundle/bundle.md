# [PY_ARTIFACTS_BUNDLE]

The artifact-bundling and compression owner. `Bundle` packs any emitted artifact into content-addressed compressed bytes; `CompressionAlgo` is ONE algorithm-row owner collapsing zstandard, lz4, brotli, and py7zr — never a per-algorithm class family. The packed bundle keys by the runtime content key over the compressed bytes, so a bundle of identical inputs at an identical algorithm is a cache hit by reference. This is bundling, not visualization; it owns no artifact production, only the compression close over already-emitted bytes.

## [1]-[INDEX]

- [1]-[COMPRESSION]: algorithm-row compression and content-addressed bundle owner.

## [2]-[COMPRESSION]

- Owner: `Bundle` the one bundle owner; `CompressionAlgo` the closed `StrEnum` discriminating algorithm; the algorithm package is bound per row, never an `if zstd` branch.
- Cases: `CompressionAlgo` rows `ZSTD` (zstandard `ZstdCompressor`) · `LZ4` (lz4 `frame`) · `BROTLI` (brotli `compress`) · `SEVEN_Z` (py7zr `SevenZipFile`) — matched by `match`/`case`, each binding its algorithm package.
- Entry: `Bundle.pack` is `async` over the runtime `async_boundary`, compresses the payload through the selected algorithm, and returns a `RuntimeRail[ContentKey]` keyed by the content key over the compressed bytes; the SEVEN_Z arm writes through an in-memory `SevenZipFile` sink.
- Receipt: each pack contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Bundle` carrying the content key and the compressed byte count.
- Packages: `zstandard` (`ZstdCompressor`), `py7zr` (`SevenZipFile`) on the cp315 core; `lz4` (`frame.compress`) and `brotli` (`compress`/`MODE_GENERIC`) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new algorithm is one `CompressionAlgo` row plus one acceptor arm; zero new surface.
- Boundary: a per-algorithm compression class family is the deleted form; this owner is content-addressed bundling over already-emitted artifact bytes and owns no artifact production. The cp315-core `ZSTD`/`SEVEN_Z` arms compress in-process; the `LZ4`/`BROTLI` arms ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so each dispatches its codec onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports the codec at module scope — neither a module-top nor a lazy gated import lands on the core page.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

import py7zr
import zstandard
from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    SEVEN_Z = "7z"


GATED: frozenset[CompressionAlgo] = frozenset({CompressionAlgo.LZ4, CompressionAlgo.BROTLI})


class Bundle(Struct, frozen=True):
    payload: bytes
    algo: CompressionAlgo

    async def pack(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"bundle.{self.algo}", self._emit)

    async def _emit(self) -> ContentKey:
        data = await to_process.run_sync(_gated_codec, self.algo.value, self.payload) if self.algo in GATED else _core_codec(self.payload, self.algo)
        return ContentIdentity.of(f"bundle-{self.algo}", data)


def _core_codec(payload: bytes, algo: CompressionAlgo) -> bytes:
    match algo:
        case CompressionAlgo.ZSTD:
            return zstandard.ZstdCompressor().compress(payload)
        case CompressionAlgo.SEVEN_Z:
            sink = BytesIO()
            with py7zr.SevenZipFile(sink, mode="w") as archive:
                archive.writef(BytesIO(payload), "payload")
            return sink.getvalue()
        case _:
            assert_never(algo)


def _gated_codec(algo: str, payload: bytes) -> bytes:
    match CompressionAlgo(algo):
        case CompressionAlgo.LZ4:
            import lz4.frame

            return lz4.frame.compress(payload, content_checksum=True)
        case CompressionAlgo.BROTLI:
            import brotli

            return brotli.compress(payload, mode=brotli.MODE_GENERIC, quality=11)
        case _:
            assert_never(algo)
```

## [3]-[RESEARCH]

No open items. `_gated_codec` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, importing `lz4.frame`/`brotli` at boundary scope inside the gated-band worker, never on the cp315-core owner; the `lz4.frame.compress` and `brotli.compress(mode=MODE_GENERIC, quality=11)` spellings verify against the folder `.api` catalogues for `lz4`/`brotli`. The cp315-core `zstandard.ZstdCompressor().compress` and `py7zr.SevenZipFile`/`writef` spellings resolve in-process.
