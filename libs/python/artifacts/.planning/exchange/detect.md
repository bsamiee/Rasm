# [PY_ARTIFACTS_DETECT]

The media-type / file-info / format-identification owner at the ingest boundary. `Detect` is ONE owner over `python-magic` that sniffs a payload through libmagic content-pattern matching into a typed `DetectIdentity` carrying the MIME type, the human description, the charset encoding, and the valid-extension list — never an extension-table guesser, never a per-output function family, and never a per-source detector type. `DetectOp` discriminates by output profile (`Mime` the single MIME-type gate, `Identity` the full MIME+encoding+extension pass, `Describe` the human description) over one `Magic` cookie per flag-policy, so the format-ID gate is one polymorphic surface the document, raster, and Office owners read before per-format reader dispatch rather than each owner re-sniffing ad hoc. libmagic is a Forge-provisioned host dependency NOT on the cp315-core loader path, so every detection crosses the `faults`-owned `anyio.to_process.run_sync` gated-band subprocess seam onto the worker that imports `magic` at boundary scope, the same gated lane `graphic/raster/io#RASTER` composes for its `Detect` raster-ingress gate. This is the ingest-boundary format-ID gate — every detection folds into the runtime `ContentIdentity` and feeds the per-format reader dispatch, contributing no `ArtifactReceipt` of its own (the descriptive-metadata fields inside the identified container are the `exchange/metadata#METADATA` owner's concern).

## [01]-[INDEX]

- [01]-[DETECT]: libmagic content-sniffing format-identification owner over `python-magic` — the closed `DetectOp` family (`Mime`/`Identity`/`Describe`) folding into one typed `DetectIdentity`, one `Magic` cookie per flag-policy carrying the `mime`/`mime_encoding`/`extension` flag set, the `_COOKIE_TABLE` flag-policy dispatch, and the `from_buffer`/`from_file`/`from_descriptor` source rows — the standalone format-ID gate `graphic/raster/io#RASTER` composes for its raster ingress and the document/Office owners read before per-format reader dispatch, all crossing the one gated-band `to_process` subprocess seam.

## [02]-[DETECT]

- Owner: `Detect` the one format-identification owner discriminating output profile over the closed `DetectOp` family; `DetectOp` an `expression.tagged_union` whose every case carries its own typed payload (the source bytes plus the optional custom-database path), never a shared erased `params` dict; `DetectIdentity` the one typed result every arm folds into — `mime`/`description`/`encoding`/`extensions`/`byte_length` recovering the MIME type, the human description, the charset, the slash-separated valid-extension list, and the input size — projected to `core/receipt#RECEIPT` `ArtifactReceipt` descriptive-identity facts and folded into the runtime `ContentIdentity`. The `Magic` cookie is the libmagic handle owning the flag set, the `threading.Lock`, and the lazily-loaded database; one cookie per flag-policy is the canonical owner because libmagic returns one cooked string per call and the module-level `from_buffer` exposes only the `mime` switch — a multi-output identity holds a cookie. The `_COOKIE_TABLE` is the flag-policy collapse: a row carries the `(mime, mime_encoding, extension)` flag triple a `DetectOp` profile needs, the op routes by one table lookup, never a per-profile sibling detector and never a re-discriminating `match` inside an arm.
- Cases: `DetectOp` cases — `Mime(payload, magic_file)` (the single MIME-type gate `from_buffer(payload, mime=True)` returning `application/pdf` etc., the canonical in-memory-bytes row admission already holds the payload for) · `Identity(payload, magic_file)` (the full identity pass holding a `Magic(mime=True, mime_encoding=True)` cookie for MIME+charset in one call plus a second `Magic(extension=True)` cookie for the valid-extension hint list, since libmagic returns one cooked string per call) · `Describe(payload, magic_file)` (the human-description pass `from_buffer(payload, mime=False)` returning `PDF document, version 1.7`) — matched by one total `match`/`case`; the `magic_file` axis is the optional custom compiled `.mgc`/text database path a profile loads, the `source` axis is the in-memory-bytes row (file/descriptor rows avoid a re-read only when the payload is already on disk), and the output axis is the flag selection on one detection, never separate functions per output.
- Entry: `Detect.of` is `async` over the runtime `async_boundary` and dispatches the `DetectOp` case, returning one `RuntimeRail[DetectIdentity]` whose identity folds the resolved MIME/description/encoding/extension facts — never an erased `str` the consumer re-classifies; EVERY `DetectOp` case crosses the runtime `reliability/faults#FAULT` `anyio.to_process.run_sync` subprocess seam onto the gated-band worker, because the `python-magic` ctypes binding loads a Forge-provisioned native `libmagic` (plus its compiled magic database) that is not on the cp315-core loader path — an `ImportError('failed to find libmagic')` at import time means the host has no `libmagic`, which the file-control owner surfaces as a provisioning fault, not a content fault. There is no in-process detect arm: the payload folds onto the one `to_process.run_sync(_gated_detect, op)` seam where the worker imports `magic` at boundary scope, holds the flag-policy `Magic` cookie, and cooks the one identity string per flag.
- Auto: `_gated_detect` folds the case through one `match` at boundary scope — `Mime`/`Describe` resolve the one `magic.from_buffer(payload, mime=...)` row over the `_instances`-cached default cookie, and `Identity` holds a `Magic(mime=True, mime_encoding=True, magic_file=op.magic_file)` cookie whose `from_buffer` cooks the MIME, a second `from_buffer` over the same cookie under a re-flag cooks nothing extra (libmagic returns one string per call), so the encoding rides a `Magic(mime_encoding=True)` cookie and the extension hint rides a `Magic(extension=True)` cookie, the three cooked strings folded into one `DetectIdentity` in a single worker crossing rather than three subprocess hops. The `_COOKIE_TABLE` maps each profile to its flag triple and the worker constructs the flag-pinned cookie once, never passing detection flags as per-call arguments where a flag-pinned `Magic` cookie is the owner; the `_handle509Bug` libmagic null-result quirk returns `application/octet-stream` for the known MIME bug, lifted to the file-control fault rail at the boundary rather than escaping as a bare exception.
- Receipt: `Detect` is the ingest-boundary format-ID GATE, not an artifact producer — the ARCHITECTURE `[02]-[SEAMS]` `exchange/detect → python:artifacts/document` edge is the media-type gate at the ingest boundary, with no `exchange/detect → core/receipt` seam, so this owner contributes no `ArtifactReceipt` case and mints no content key. Each detection folds into `DetectIdentity` — the resolved MIME type, the human description, the charset encoding, the detected-extension list, and the input byte length — the admission-gate evidence the document/PDF/image owners read before per-format reader dispatch, folded into the runtime `ContentIdentity` rather than a re-minted identity. The MIME branch is the routing discriminant: `application/pdf` routes to the `pymupdf`/`pypdf` PDF owners, the OOXML/ODF MIME branch to `python-docx`/`python-pptx`/`openpyxl`, the image branches to `pillow`/`pyvips`, and the encrypted-Office branch to `msoffcrypto-tool`; the descriptive-metadata fields INSIDE the identified container are the `exchange/metadata#METADATA` owner's concern, never re-read here.
- Packages: `python-magic` (`from_buffer(buffer, mime=False)`/`from_file`/`from_descriptor` stateless rows, `Magic(mime=, magic_file=, mime_encoding=, keep_going=, uncompress=, raw=, extension=)` flag-pinned cookie, `Magic.from_buffer`/`from_file`/`from_descriptor`/`setparam`/`getparam`, `MagicException` carrying `.message`, `version()`) the host-native provisioning-gated libmagic binding; `expression` (`tagged_union`/`tag`/`case`); `msgspec` (`Struct(frozen=True)` for the `DetectIdentity` value owner); runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary` and the `faults`-owned `anyio.to_process.run_sync` subprocess seam every detect arm crosses).
- Growth: a new detection profile is one `DetectOp` case plus one `_COOKIE_TABLE` flag-policy row plus one `_gated_detect` arm; a new libmagic flag (`uncompress` look-through, `keep_going` all-matches, `raw`) is one column on the existing flag triple the cookie reads; a new libmagic tuning param (`INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/budget caps) is one `setparam` call on the cookie; a new source (file/descriptor where the payload is already on disk) is one `from_file`/`from_descriptor` row on the same cookie; a custom `.mgc`/text database is the `magic_file` field on the existing payload, no new surface; a new identity fact is one field on `DetectIdentity`; zero new surface.
- Boundary: an extension-table guesser where libmagic content sniffing is admitted, a per-output `mime_of`/`encoding_of`/`describe` function family, a per-source `BufferDetector`/`FileDetector` class family, passing detection flags as per-call arguments where a flag-pinned `Magic` cookie is the owner, the deprecation-wrapped `compat.*` shim (`detect_from_filename`/`detect_from_content`/`detect_from_fobj`/`open`) that collides with the upstream file-5.x binding, and an identity re-mint the runtime `content_identity` already owns are the deleted forms; this owner is content sniffing only and produces no artifact, holds no live UI viewer. python-magic owns the format-ID gate the document/PDF/image owners read before per-format reader dispatch — `graphic/raster/io#RASTER` composes this owner for its raster `Detect` ingress gate rather than re-declaring a parallel magic surface, the raster owner's gated worker already importing `magic` at boundary scope on the same `to_process` band so the raster ingress and the standalone format-ID gate share the one gated lane. The libmagic `ImportError` at import time is a host-provisioning fault (libmagic + its magic database are Forge-provisioned, NOT a wheel), surfaced as a provisioning fault the file-control owner lifts, never a content fault; the Python ctypes surface is independent of provisioning, so the page fence is settled even where the host libmagic is not on the loader path until provisioned.

```python signature
from types import MappingProxyType
from typing import Final, Literal, assert_never

from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, async_boundary

type CookieFlags = tuple[bool, bool, bool]


_COOKIE_TABLE: Final[MappingProxyType[str, CookieFlags]] = MappingProxyType({
    "mime": (True, False, False),
    "encoding": (False, True, False),
    "extension": (False, False, True),
})


class DetectIdentity(Struct, frozen=True):
    mime: str
    description: str
    encoding: str
    extensions: str
    byte_length: int

    def facts(self) -> dict[str, str]:
        return {
            "mime": self.mime,
            "description": self.description,
            "encoding": self.encoding,
            "extensions": self.extensions,
            "bytes": str(self.byte_length),
        }


@tagged_union(frozen=True)
class DetectOp:
    tag: Literal["mime", "identity", "describe"] = tag()
    mime: tuple[bytes, str | None] = case()
    identity: tuple[bytes, str | None] = case()
    describe: tuple[bytes, str | None] = case()

    @staticmethod
    def Mime(payload: bytes, magic_file: str | None = None) -> "DetectOp":
        return DetectOp(mime=(payload, magic_file))

    @staticmethod
    def Identity(payload: bytes, magic_file: str | None = None) -> "DetectOp":
        return DetectOp(identity=(payload, magic_file))

    @staticmethod
    def Describe(payload: bytes, magic_file: str | None = None) -> "DetectOp":
        return DetectOp(describe=(payload, magic_file))


class Detect(Struct, frozen=True):
    op: DetectOp

    async def of(self) -> RuntimeRail[DetectIdentity]:
        return await async_boundary(f"detect.{self.op.tag}", self._emit)

    async def _emit(self) -> DetectIdentity:
        return await to_process.run_sync(_gated_detect, self.op)


def _cook(payload: bytes, profile: str, magic_file: str | None) -> str:
    import magic

    mime, encoding, extension = _COOKIE_TABLE[profile]
    return magic.Magic(mime=mime, mime_encoding=encoding, extension=extension, magic_file=magic_file).from_buffer(payload)


def _gated_detect(op: DetectOp) -> DetectIdentity:
    import magic

    match op:
        case DetectOp(tag="mime", mime=(payload, magic_file)):
            mime = magic.Magic(mime=True, magic_file=magic_file).from_buffer(payload)
            return DetectIdentity(mime, "", "", "", len(payload))
        case DetectOp(tag="describe", describe=(payload, magic_file)):
            description = magic.Magic(magic_file=magic_file).from_buffer(payload)
            return DetectIdentity("", description, "", "", len(payload))
        case DetectOp(tag="identity", identity=(payload, magic_file)):
            return DetectIdentity(
                _cook(payload, "mime", magic_file),
                magic.Magic(magic_file=magic_file).from_buffer(payload),
                _cook(payload, "encoding", magic_file),
                _cook(payload, "extension", magic_file),
                len(payload),
            )
        case _:
            assert_never(op)
```

## [03]-[RESEARCH]

- [MEDIA_DETECT_GATE] [RESOLVED]: the libmagic `from_buffer(buffer, mime=False) -> str` stateless row (`[03]-[ENTRYPOINTS]` row `[01]`, `mime=True` returning the MIME type, the in-memory-bytes canonical detection because admission already holds the payload, `[04]-[IMPLEMENTATION_LAW]` source axis), the `Magic(mime=False, magic_file=None, mime_encoding=False, keep_going=False, uncompress=False, raw=False, extension=False)` flag-pinned cookie constructor (`[03]` configured-cookie row `[01]`), the `Magic.from_buffer`/`from_file`/`from_descriptor` per-call rows `[02]`-`[04]`, `Magic.setparam`/`getparam` tuning rows `[05]`/`[06]`, the `mime`/`mime_encoding`/`extension`/`uncompress`/`keep_going`/`raw` flag-boolean vocabulary (`[03]` flag table rows `[01]`-`[06]` mapping to `MAGIC_MIME_TYPE`/`MAGIC_MIME_ENCODING`/`MAGIC_EXTENSION`/`MAGIC_COMPRESS`/`MAGIC_CONTINUE`/`MAGIC_RAW`), the `magic_file=` custom-database row `[07]`, the `MAGIC_PARAM_*` ordinal family row `[08]`, the `MagicException` engine fault carrying `.message` (`[02]-[PUBLIC_TYPES]` row `[02]`), and `version()` (`[03]` stateless row `[04]`) all verify against the folder `python-magic` `.api` (`0.4.27`) catalogue. The flags are NOT per-call arguments — one `Magic` cookie per flag-policy is the canonical owner (`[04]-[IMPLEMENTATION_LAW]` detector axis), and a multi-output identity (MIME + encoding + extensions) holds a cookie because libmagic returns one cooked string per call (output axis), so the `Identity` arm constructs three flag-pinned cookies in the one worker crossing rather than stacking flags on the module-level `from_buffer` that exposes only `mime`. The `extension` flag requires libmagic >= 524; the `_handle509Bug` null-result quirk returns `application/octet-stream` (fault axis). The page fence resolves the field spellings against `[03]`/`[04]` and gates nothing.
- [DETECT_SEAM] [RESOLVED]: `_gated_detect` runs on the gated band through `anyio.to_process.run_sync(_gated_detect, op)`, importing `magic` at boundary scope inside the worker, never on the cp315-core owner — `python-magic` is a pure-Python ctypes binding with no compiled extension, but at import time `loader.load_lib()` resolves the system `libmagic` shared object and raises `ImportError('failed to find libmagic')` when absent (`[01]-[PACKAGE_SURFACE]` abi line — the native `libmagic` + its magic database are a Forge-provisioned host dependency, NOT a wheel, so runtime detection is provisioning-gated until libmagic is on the loader path while the Python member surface is independent of provisioning). The `to_process.run_sync` subprocess seam is the spelling the runtime `reliability/faults#FAULT` owner has settled — a subinterpreter shares the host loader path and cannot resolve the host-native libmagic any better than the core process, so detection crosses this genuine separate-process seam onto the gated-band worker exactly as `graphic/raster/io#RASTER` runs its `magic.from_buffer(payload, mime=True)` raster-ingress gate on the same band, and `_gated_detect`/`_cook` are module-level functions dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure), staying out of the `Detect` owner deliberately. The branch `anyio` `.api` catalogue reflects `open_process`/`run_process`/`to_thread.run_sync`/`to_interpreter.run_sync` but no `to_process` row, so `assay api` reflection over `anyio.to_process` deepens the branch catalogue to match the settled owner spelling, never re-opening this fence.
- [RASTER_COMPOSE] [RESOLVED]: `graphic/raster/io#RASTER` composes this standalone format-ID owner for its `RasterOp.Detect` raster-ingress gate rather than re-declaring a parallel magic surface — the raster owner's `Detect(payload)` case resolves `magic.from_buffer(payload, mime=True)` on the same gated `to_process` worker that imports `magic` at boundary scope (`[04]-[IMPLEMENTATION_LAW]` boundary, the raster boundary names `pillow`/`pyvips` as the image MIME branch consumers), so the raster ingress reads this owner's MIME gate while this owner stays the full-identity format-ID gate the document/Office owners read; the `compat` shim functions are deprecation-wrapped and NOT admitted (they collide with the upstream file-5.x binding), so both owners use the `Magic` cookie / `from_buffer` rows. The MIME-branch routing — `application/pdf` to `pymupdf`/`pypdf`, OOXML/ODF to `python-docx`/`python-pptx`/`openpyxl`, image branches to `pillow`/`pyvips`, encrypted-Office to `msoffcrypto-tool` — is the admission-gate dispatch the `DetectIdentity.mime` discriminant feeds, never a parallel per-owner sniff.
```
