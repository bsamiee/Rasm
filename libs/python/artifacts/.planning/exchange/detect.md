# [PY_ARTIFACTS_DETECT]

The media-type / file-info / format-identification owner at the ingest boundary. `Detect` is the configured detector — a `DetectProfile` plus a `DetectPolicy` — whose one polymorphic `of` admits a `Source` or an `Iterable[Source]` and sniffs each payload through `python-magic` libmagic content-pattern matching into a typed `DetectIdentity` carrying the MIME type, the human description, the charset encoding, the valid-extension tuple, the derived `MediaClass` routing discriminant, the orthogonal `Container` structural discriminant (the `ZIP`/`OLE`/`TAR`/compression kind a `.docx`/legacy-`.doc`/archive sits inside, which a consumer routes a second-pass unpacker on), the `keep_going` multi-match set, the ingress-declared `claimed` content-type, the `Trust` verdict folding the sniffed-vs-claimed evidence into `IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`, the input byte length, and the resolving `libmagic` version — never an extension-table guesser, never a per-output function family, never a per-source detector type, never a `detect_many` sibling beside the singular call. `Source` is the closed admission family (`Buffer` in-memory bytes, `File` an on-disk path the worker reads without the parent pickling megabytes across the process seam), `DetectProfile` the closed output vocabulary (`MIME`/`DESCRIBE`/`IDENTITY`) whose `_PROFILE_FACETS` primary row selects the `MagicFacet` set cooked, and `DetectPolicy` the one behavior-carrying value folding the `uncompress`/`keep_going`/`raw` `DetectFlag` set, the `MAGIC_PARAM_*` recursion/budget caps, and the custom `.mgc` database into one owner — so a multi-output identity is one worker crossing that constructs one flag-pinned `Magic` cookie per facet, because libmagic returns one cooked string per call, never per-call flags on the module-level `from_buffer` that exposes only `mime`. libmagic is a Forge-provisioned host dependency NOT on the cp315-core loader path, so every detection crosses the `anyio.to_process.run_sync` subprocess seam — bounded by the `execution/lanes#LANE`-owned shared `GATED_BAND` `CapacityLimiter`, never the default process limiter, with a transient `BrokenWorkerProcess` death recovered by a bounded `stamina.AsyncRetryingCaller` retry before the `async_boundary` rails the exhausted failure — onto the provisioned worker that imports `magic` at boundary scope; the same gated companion band `exchange/metadata#METADATA` and `graphic/raster/io#RASTER` cross. This is the ingest-boundary format-ID gate: every detection resolves a `DetectIdentity` the per-format reader dispatch reads through the typed `MediaClass` discriminant, minting no content key and contributing no `ArtifactReceipt` of its own — content-addressing is the `evidence/identity#IDENTITY` owner's concern and the descriptive-metadata fields inside the identified container are the `exchange/metadata#METADATA` owner's.

## [01]-[INDEX]

- [01]-[DETECT]: libmagic content-sniffing format-identification owner over `python-magic` — the `Source` admission family (`Buffer`/`File`), the closed `DetectProfile` vocabulary (`MIME`/`DESCRIBE`/`IDENTITY`) selecting a `MagicFacet` set through the `_PROFILE_FACETS` primary row and the `_FACET_FLAG` one-flag-per-cook derivation, the `DetectPolicy` value folding the `uncompress`/`keep_going`/`raw` flags, the `MAGIC_PARAM_*` tuning caps, and the custom `.mgc` database, the `MediaClass` routing discriminant and the orthogonal `Container` structural discriminant both folded from the sniffed MIME by one `_classified` exact-then-longest-prefix fold over `_MEDIA_CLASS`/`_CONTAINER`, the `Trust` declared-vs-sniffed verdict (`IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`) the `_trust` fold lands over the `claimed` content-type each `Source` carries, and the typed `DetectIdentity` every facet folds into — the one polymorphic `Detect.of(Source | Iterable[Source])` entry whose singular arm rails one crossing and whose plural arm fans the batch across the bounded `GATED_BAND` under one `anyio` task group, every crossing wrapped in the bounded `_WORKER_RETRY` `BrokenWorkerProcess` recovery, threaded by `faults.traversed` under the `Disposition` policy value (`ABORT`/`ACCUMULATE`/`PARTITION`). The standalone format-ID gate `graphic/raster/io#RASTER` composes for its raster ingress and the document/Office/scene owners read before per-format reader dispatch, all crossing the one `execution/lanes#LANE`-owned `to_process` companion band onto the provisioned worker.

## [02]-[DETECT]

- Owner: `Detect` the one format-identification owner carrying a `DetectProfile`, a `DetectPolicy`, and a `Disposition` batch-combination policy (the runtime `faults.traversed` `by=` parameter — `ABORT`/`ACCUMULATE`/`PARTITION`); `Source` an `expression.tagged_union` closed admission family — `Buffer` the in-memory-bytes canonical row admission already holds, `File` the on-disk path the worker reads so the parent never pickles the payload across the process seam — each case carrying the ingress-declared content-type beside its locator and exposing its own `length` size projection and `claimed` content-type projection (the explicit declaration, or for a `File` the `mimetypes.guess_file_type` extension MIME) so the caller never branches on source kind; `DetectProfile` the closed `StrEnum` output vocabulary (`MIME`/`DESCRIBE`/`IDENTITY`) whose `_PROFILE_FACETS` primary row selects the `MagicFacet` tuple, collapsing the former per-output `DetectOp` case-and-factory family into one policy value; `MagicFacet` (`MIME`/`ENCODING`/`EXTENSION`/`DESCRIPTION`) the atomic cookable outputs each pinning exactly one `Magic` cookie flag through the `_FACET_FLAG` derivation, because libmagic returns one cooked string per call; `DetectPolicy` the one behavior value folding the `DetectFlag` set (`uncompress`/`keep_going`/`raw`), the `MagicParam` `MAGIC_PARAM_*` tuning caps, and the custom `.mgc` `magic_file` into one owner instead of scattering flags across cases or threading them as per-call arguments; `DetectIdentity` the one typed result every facet folds into, carrying the `claimed` declared type, the `Trust` verdict, and the `Container` structural discriminant beside the sniffed fields — a frozen `msgspec.Struct` that IS its own wire/egress projection the gated worker pickles back across the process seam and a consumer renders to span attributes through `msgspec.to_builtins`/`structs.asdict` directly, never a forwarding `facts()` hop that only re-states the field set a one-hop slower; `MediaClass` the derived routing discriminant and `Container` the orthogonal structural discriminant, both folded from the one sniffed MIME by the shared `_classified` exact-then-longest-prefix fold over `_MEDIA_CLASS`/`_CONTAINER` (a `.docx` resolving `WORD`+`ZIP`, a legacy `.doc` `OFFICE_LEGACY`+`OLE`, a bare archive `ARCHIVE`+its compression kind) so a consumer dispatches a reader on `media_class` and a second-pass unpacker on `container` without re-parsing the MIME string; `Trust` the closed declared-vs-sniffed verdict (`IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`) the `_trust` fold derives over the `media_class`, the `container`, the `keep_going` `matches`, the sniffed `extensions`, and the `Source.claimed` content-type, so a spoofed or polyglot payload is a verdict the gate states rather than evidence it silently discards. The `_FACET_FLAG` is the flag-policy collapse — a row maps a facet to the single `Magic` boolean it pins — so the worker constructs one flag-pinned cookie per facet, never a `tuple[bool, bool, bool]` triple and never a re-discriminating `match` inside an arm.
- Cases: `DetectProfile` rows — `MIME` (the single MIME-type gate, the `{MagicFacet.MIME}` facet, `from_buffer` under `mime=True`) · `DESCRIBE` (the human-description pass, the `{MagicFacet.DESCRIPTION}` facet cooked under no flag returning `PDF document, version 1.7`) · `IDENTITY` (the full pass, the `{MIME, DESCRIPTION, ENCODING, EXTENSION}` facets each holding its own flag-pinned cookie for MIME + charset + description + extension hints in one worker crossing) — the facet set is the `_PROFILE_FACETS` primary correspondence, never separate functions per output. `MagicFacet` rows map through `_FACET_FLAG` to the cookie boolean (`MIME`→`mime`, `ENCODING`→`mime_encoding`, `EXTENSION`→`extension`, `DESCRIPTION` cooks under no flag); the `DetectFlag` policy set composes onto every facet cookie (`uncompress` looks through gzip/bzip2/xz containers, `keep_going` returns all matches the worker splits into `matches`, `raw` keeps unprintable bytes); the `MagicParam` caps (`INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/name/regex/byte budgets and the `ELF_NOTES_MAX`/`ELF_PHNUM_MAX`/`ELF_SHNUM_MAX` ELF-table caps) apply through `setparam` to harden an untrusted ingest against unbounded libmagic recursion and ELF-table bombs. `Source` cases — `Buffer(payload, claimed=...)` dispatching `from_buffer`, `File(path, claimed=...)` dispatching `from_file`, each carrying the optional ingress-declared content-type beside its locator — matched by one total `match` in `_cooked`. `MediaClass` rows are the routing vocabulary the `_MEDIA_CLASS` exact-then-longest-prefix fold resolves (the longest registered prefix winning so the OOXML/ODF compound subtypes route through their `...wordprocessingml`/`...opendocument` row even when modern libmagic appends the `.document`/`.sheet`/`.presentation` suffix a bare exact key would miss), the `MODEL` member routing the IANA `model/*` 3D-artifact family (`model/gltf-binary`/`model/gltf+json`/`model/vnd.usdz+zip`/`model/stl`/`model/obj`/`model/3mf` plus the `model/` prefix) to the `scene/stage#STAGE`/`scene/export#EXPORT` consumers beside the document/image/media/font branches, the `EBOOK` member routing `application/epub+zip`/`application/x-mobipocket-ebook` to the `pymupdf` document reader beside the `PDF` branch, and the `DATA` member routing `application/json`/`application/xml`/`text/xml`/`application/yaml`/`application/toml`/`text/csv` to the `msgspec`/`lxml`/`ruamel-yaml`/`tomlkit`/`csvkit` structured-data readers a `text/` prefix would have floored to `TEXT` or libmagic left at `UNKNOWN`. The `Container` rows are the parallel structural axis the `_CONTAINER` table folds the same MIME into — `ZIP` for the OOXML/ODF/EPUB/USDZ/CBZ + bare-zip family, `OLE` for the CDFV2/`x-ole-storage` legacy-Office compound, and the per-compression `TAR`/`SEVENZIP`/`GZIP`/`BZIP2`/`XZ`/`ZSTD`/`LZ4` kinds — so `media_class` answers which reader and `container` answers which second-pass unpacker (`stream-unzip`/`olefile`/`py7zr`/…), orthogonal discriminants the consumer reads without a second sniff. `Trust` rows are the ingest verdict `_trust` folds in one pass — `UNKNOWN` for the `application/octet-stream` content floor, `AMBIGUOUS` for a `keep_going` multi-match polyglot, `MISMATCH` when the sniffed `MediaClass` disagrees with a known-class `claimed` declared type or when a same-class claim's `mimetypes.guess_all_extensions` set is disjoint from the sniffed `extensions`, and `IDENTIFIED` for a confident agreeing sniff or a generic-container claim (`application/zip` declared for a zip-based `.docx`) whose `Container` matches the sniffed container — the container generalization the cross-class check would otherwise false-flag, distinguished from a specific-format spoof (`application/epub+zip` for a `.docx`) by gating the exemption on the claim resolving the generic `ARCHIVE` class — so the `extensions`/`matches`/`container` evidence and the declared content-type resolve to a verdict rather than an unread tuple.
- Entry: `Detect.of` is `async` over the runtime `async_boundary` and is the one polymorphic modality entry over `Source | Iterable[Source]`, normalized once at the head by a total `match` — a lone `Source` rails one crossing as `RuntimeRail[DetectIdentity]`, an `Iterable[Source]` fans the batch as `RuntimeRail[Block[DetectIdentity]]` (the `ABORT`/`ACCUMULATE` disposition) or `RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]` (the `PARTITION` survivor/casualty split), the modality recovered from the value shape, never a `batch: bool` knob or a `detect_many` sibling. Both modality arms route through one `_railed` method wrapping `async_boundary(f"detect.{profile}", ...)` over the single `to_process` crossing, so the singular arm awaits `self._railed(one)` and the batch arm awaits `_many`, which opens one `anyio.create_task_group`, fans each source through that same per-source `_railed` child whose `TaskHandle.return_value` settles on the all-clean path, and threads the `Block[RuntimeRail[DetectIdentity]]` through `faults.traversed` under the owner's `Disposition` policy value — `ABORT` fail-fast (the default), `ACCUMULATE` combining every casualty fault, or `PARTITION` keeping the survivor/casualty split a folder import reads. EVERY crossing rides `anyio.to_process.run_sync(_gated_detect, source, profile, policy, limiter=GATED_BAND)`, because the `python-magic` ctypes binding loads a Forge-provisioned native `libmagic` (plus its compiled magic database) that is not on the cp315-core loader path — an `ImportError('failed to find libmagic')` at import time is the host-no-libmagic provisioning fault the runtime `reliability/faults#FAULT` `CLASSIFY` `import_` row lands, not a content fault. There is no in-process detect arm: `_gated_detect`/`_cooked` are module-level functions dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure), the worker imports `magic` at boundary scope, holds one flag-pinned `Magic` cookie per facet, and cooks the one identity string per flag; the shared `execution/lanes#LANE` `GATED_BAND` `CapacityLimiter` bounds the subprocess workers across the `detect`/`metadata`/`graphic/raster/io` companion band, never the per-loop default process limiter `concurrency.md` rejects.
- Auto: `_gated_detect` reads `magic.version()` once (the `EXTENSION` facet gated on `>= 524`, the version unavailable on an ancient libmagic falling to `0` so detection proceeds without the extension hint) and rides it onto `DetectIdentity.libmagic_version`; it folds each `_PROFILE_FACETS[profile]` facet through `_cooked` — the `functools.cache`-memoised `_cookie(magic_file, facet flag, flags, params)` flag-pinned `Magic` cookie (built once per config and reused across detections in the worker, never a per-facet-per-call magic-database reload) tuned by `setparam` over the `MagicParam` caps, dispatching `from_buffer`/`from_file` on the `Source` — into one `frozendict[MagicFacet, str]` of cooked strings in a single worker crossing rather than per-facet subprocess hops. The scalar fields take the strongest match (`raw.split("\n- ", 1)[0]` cutting the libmagic continue separator), the `extension` slash-list (`jpeg/jpg/jpe/jfif`) splits into the `extensions` tuple dropping the `???` unknown token, the `keep_going` multi-match splits into `matches`, and `MediaClass.of(mime)` folds the MIME through `_MEDIA_CLASS` exact-then-prefix (the exact `.get`, then the `/`-suffixed prefix rows alone so a longer exact key never false-matches as a prefix) into the routing discriminant, and `_trust` folds that `media_class`, the `matches` cardinality, the sniffed `extensions`, and `source.claimed` (the explicit ingress declaration, or for a `File` the `mimetypes.guess_file_type` extension MIME) into the `Trust` verdict — `MediaClass.UNKNOWN`/`application/octet-stream` to `UNKNOWN`, a multi-match to `AMBIGUOUS`, a known claimed class disagreeing with the sniffed class to `MISMATCH`, a same-class claim whose `mimetypes.guess_all_extensions` set is disjoint from the sniffed `extensions` to `MISMATCH` (the extension/label spoof a class-only check misses), otherwise `IDENTIFIED`. The flags are NOT per-call arguments — one flag-pinned `Magic` cookie per facet is the owner — and the libmagic `_handle509Bug` null-result quirk returns `application/octet-stream` (a valid unknown-content MIME classified `MediaClass.UNKNOWN`), never an escaping exception; a `MagicException` from a genuinely broken magic database is the worker raise the `async_boundary` `CLASSIFY` catch-all `boundary` case lands at the seam.
- Receipt: `Detect` is the ingest-boundary format-ID GATE, not an artifact producer — the ARCHITECTURE `[02]-[SEAMS]` `exchange/detect → python:artifacts/document` edge is the media-type gate at the ingest boundary, with no `exchange/detect → core/receipt` seam, so this owner contributes no `ArtifactReceipt` case and mints no content key. Each detection resolves a `DetectIdentity` — the resolved MIME, description, charset, extension tuple, `MediaClass`, `keep_going` match set, the `claimed` declared content-type, the `Trust` declared-vs-sniffed verdict, input byte length, and resolving libmagic version — the descriptive admission-gate evidence the document/PDF/image/scene owners read before per-format reader dispatch; it is the page's own format-ID identity, never the runtime `ContentKey` (the `evidence/identity#IDENTITY` content hash a producing owner mints over the bytes it emits, which `Detect` neither computes nor folds into). The `MediaClass` discriminant is the routing the consumers dispatch on — `PDF`→`pymupdf`/`pypdf`, `EBOOK`→`pymupdf`, `WORD`→`python-docx`, `SPREADSHEET`→`openpyxl`, `PRESENTATION`→`python-pptx`, `OFFICE_ODF`→`odfpy`, `VECTOR`→`svgelements`/`resvg-py`, `IMAGE`→`pillow`/`pyvips`, `ENCRYPTED`/`OFFICE_LEGACY`→`msoffcrypto-tool`, `AUDIO`/`VIDEO`→`av`, `MODEL`→`scene/stage#STAGE`/`scene/export#EXPORT`, `ARCHIVE`→`package/archive`, `FONT`→`typography/font#FONT` — so each consumer reads one closed vocabulary member resolving to exactly one reader, never re-parsing the MIME string and never one OOXML class fanning to three packages (the wordprocessingml/spreadsheetml/presentationml subtypes split so docx, xlsx, and pptx each route to their own owner); the consumers own the `MediaClass`→reader table, this owner owns only the classification, and the descriptive-metadata fields INSIDE the identified container are the `exchange/metadata#METADATA` owner's concern, never re-read here.
- Packages: `python-magic` (`Magic(mime=, magic_file=, mime_encoding=, keep_going=, uncompress=, raw=, extension=)` the flag-pinned cookie, `Magic.from_buffer`/`from_file` the per-source rows — `from_descriptor` is unusable here because a parent-process fd is invalid in the gated worker — `Magic.setparam`/`getparam` the `MAGIC_PARAM_*` tuning rows, the `MAGIC_PARAM_INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/budget plus `ELF_NOTES_MAX`/`ELF_PHNUM_MAX`/`ELF_SHNUM_MAX` ELF-table ordinal family, `version()` the libmagic version probe, `MagicException` carrying `.message`) the host-native provisioning-gated libmagic binding; `expression` (`tagged_union`/`tag`/`case`, `Block`/`Block.of_seq`/`Block.map` the batch carrier and fan); `msgspec` (`Struct(frozen=True, gc=False)` for the `DetectIdentity` egress value the consumers read and the gated worker pickles back across the process seam, `structs.asdict` deriving its `facts` egress from the field set, the `DetectPolicy` config and the `Detect` owner being frozen `@dataclass(slots=True)` value owners rather than wire structs); `anyio` (`to_process.run_sync(..., limiter=GATED_BAND)` the gated-band offload, `create_task_group`/`TaskHandle` the batch fan with `TaskHandle.return_value` the settled child read, `BrokenWorkerProcess` the worker-death raise the faults `CLASSIFY` `resource` row traps); `functools` (`cache` memoising the `_cookie` factory so the flag-pinned `Magic` cookie and its loaded magic database are built once per `(magic_file, facet flag, flags, params)` config per worker rather than reloaded per facet per detection); `mimetypes` (`guess_file_type` the extension-declared content-type a `File` `Source` claims when the ingress passes none, `guess_all_extensions` the claimed-type valid-extension set `_trust` compares against the sniffed `extensions` to land the same-class extension/label-spoof `MISMATCH`); runtime (`faults.RuntimeRail`/`async_boundary`/`traversed` the rail, the telemetry+fault weave, and the `Disposition`-parameterized batch reducer (`by=ABORT`/`ACCUMULATE`/`PARTITION`), `faults.BoundaryFault` the `PARTITION` casualty type; `execution/lanes#LANE` `GATED_BAND` the shared `to_process` companion-band `CapacityLimiter` every gated detection bounds against). No `content_identity`/`core/receipt` import: this owner produces only the descriptive `DetectIdentity` and contributes no receipt.
- Growth: a new detection facet is one `MagicFacet` member plus one `_FACET_FLAG` row; a new profile is one `DetectProfile` member plus one `_PROFILE_FACETS` row; a new detection-policy flag (`raw` look-through, a future libmagic boolean) is one `DetectFlag` member the cookie folds; a further libmagic tuning param is one `MagicParam` row applied through `setparam`; a custom `.mgc`/text database is the `magic_file` field on the existing policy; a new routing branch is one `MediaClass` member plus one `_MEDIA_CLASS` row (`EBOOK` the latest); a new source kind is one `Source` case; a new ingest verdict is one `Trust` member plus one arm in `_trust`; an ingress declaration rides the existing `Source` `claimed` field with zero new parameter; a new identity fact is one field on `DetectIdentity`; the singular/batch modality is the existing `of(Source | Iterable[Source])` with zero new entrypoint; zero new surface.
- Boundary: an extension-table guesser where libmagic content sniffing is admitted, a per-output `mime_of`/`encoding_of`/`describe` function family, a per-source `BufferDetector`/`FileDetector` class family, a `detect_many` sibling or a `for`/`async for` detection loop where one `of(Source | Iterable[Source])` fans the bounded band under one task group, a descriptor `Source` case where the gated `to_process` worker holds no parent-process fd (a `from_descriptor` row is meaningless across the process seam and is never admitted), a `tuple[bool, bool, bool]` flag triple where the `MagicFacet`→flag derivation is the owner, passing detection flags as per-call arguments where a flag-pinned `Magic` cookie is the owner, a fresh `Magic` cookie reconstructed (reloading the compiled magic database) per facet per detection where the `functools.cache`-memoised `_cookie` builds one per config per worker, a fixed abort-on-first batch that sinks a folder import on one unreadable `File` where the `Disposition` policy value (`ABORT`/`ACCUMULATE`/`PARTITION`) keeps the survivor/casualty partition past a per-source fault, an unbounded `to_process.run_sync` trusting the per-loop default process limiter (and the false `reliability/faults#FAULT`-owns-the-limiter attribution — the faults owner mints no `CapacityLimiter`) where the `execution/lanes#LANE` `GATED_BAND` bounds the shared companion band, a `MappingProxyType` table where `frozendict` is the table owner, the deprecation-wrapped `compat.*` shim (`detect_from_filename`/`detect_from_content`/`detect_from_fobj`/`open`) that collides with the upstream file-5.x binding, a bare-`str` fault where the runtime `BoundaryFault` vocabulary is the owner, a hand-built `facts` dict pre-stringifying or renaming the native scalars where `msgspec.structs.asdict` derives the egress from the field set with no drift, an `extensions`/`matches` evidence set the worker computes but draws no verdict from where the `_trust` fold classifies `IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`, a claimed-blind gate that routes a content-type-spoofed payload to the wrong per-format reader where the declared-vs-sniffed `MediaClass` comparison lands `Trust.MISMATCH`, a `mime.startswith` prefix scan over the exact `_MEDIA_CLASS` rows where the `/`-suffixed-prefix guard keeps a longer exact key from false-matching, and a content-key mint or `ContentIdentity` fold where this owner produces only the descriptive `DetectIdentity` are the deleted forms; this owner is content sniffing only and produces no artifact, holds no live UI viewer. `graphic/raster/io#RASTER` composes this owner for its raster `Detect` ingress gate rather than re-declaring a parallel magic surface — both owners' gated workers import `magic` at boundary scope on the same `execution/lanes#LANE`-owned `to_process` band under the one shared `GATED_BAND` limiter, so the raster ingress and the standalone format-ID gate share the one gated lane. The libmagic `ImportError` at import time is a host-provisioning fault (libmagic + its magic database are Forge-provisioned, NOT a wheel), landed by the faults `CLASSIFY` `import_` row, never a content fault; the Python ctypes surface is independent of provisioning, so the page fence is settled even where the host libmagic is not on the loader path until provisioned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import mimetypes
from collections.abc import Iterable
from dataclasses import dataclass, field
from enum import StrEnum
from functools import cache
from pathlib import Path
from typing import TYPE_CHECKING, Literal, assert_never, overload

import anyio
import stamina
from anyio import BrokenWorkerProcess, TaskHandle, to_process
from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, async_boundary, traversed
from rasm.runtime.lanes import GATED_BAND

if TYPE_CHECKING:
    import magic

# --- [TYPES] ----------------------------------------------------------------------------


class MagicFacet(StrEnum):
    MIME = "mime"
    ENCODING = "encoding"
    EXTENSION = "extension"
    DESCRIPTION = "description"


class DetectFlag(StrEnum):
    UNCOMPRESS = "uncompress"
    KEEP_GOING = "keep_going"
    RAW = "raw"


class MagicParam(StrEnum):
    INDIR_MAX = "MAGIC_PARAM_INDIR_MAX"
    NAME_MAX = "MAGIC_PARAM_NAME_MAX"
    REGEX_MAX = "MAGIC_PARAM_REGEX_MAX"
    BYTES_MAX = "MAGIC_PARAM_BYTES_MAX"
    ELF_NOTES_MAX = "MAGIC_PARAM_ELF_NOTES_MAX"
    ELF_PHNUM_MAX = "MAGIC_PARAM_ELF_PHNUM_MAX"
    ELF_SHNUM_MAX = "MAGIC_PARAM_ELF_SHNUM_MAX"


class DetectProfile(StrEnum):
    MIME = "mime"
    DESCRIBE = "describe"
    IDENTITY = "identity"


class MediaClass(StrEnum):
    PDF = "pdf"
    EBOOK = "ebook"
    WORD = "word"
    SPREADSHEET = "spreadsheet"
    PRESENTATION = "presentation"
    OFFICE_ODF = "office-odf"
    OFFICE_LEGACY = "office-legacy"
    ENCRYPTED = "encrypted"
    VECTOR = "vector"
    IMAGE = "image"
    AUDIO = "audio"
    VIDEO = "video"
    MODEL = "model"
    ARCHIVE = "archive"
    FONT = "font"
    DATA = "data"
    TEXT = "text"
    UNKNOWN = "unknown"

    @staticmethod
    def of(mime: str, /) -> "MediaClass":
        return _classified(mime, _MEDIA_CLASS, MediaClass.UNKNOWN)


class Container(StrEnum):  # the structural container/compression kind orthogonal to MediaClass — which unpacker a second-pass peek routes to
    NONE = "none"          # a leaf format with no wrapping structure
    ZIP = "zip"            # the OOXML/ODF/EPUB/USDZ/CBZ + bare-zip family a docx/xlsx/epub sits inside
    OLE = "ole"            # the CFB/CDFV2 compound the legacy Office + MSI family sits inside
    TAR = "tar"
    SEVENZIP = "sevenzip"
    GZIP = "gzip"
    BZIP2 = "bzip2"
    XZ = "xz"
    ZSTD = "zstd"
    LZ4 = "lz4"

    @staticmethod
    def of(mime: str, /) -> "Container":
        return _classified(mime, _CONTAINER, Container.NONE)


class Trust(StrEnum):  # the declared-vs-sniffed ingest verdict the gate folds over its own evidence
    IDENTIFIED = "identified"  # one confident content match, agreeing with the claim or unclaimed
    AMBIGUOUS = "ambiguous"    # keep_going found multiple strong matches — a polyglot
    MISMATCH = "mismatch"      # the sniffed class disagrees with the declared content-type — spoofing
    UNKNOWN = "unknown"        # fell to the application/octet-stream content floor


@tagged_union(frozen=True)
class Source:
    tag: Literal["buffer", "file"] = tag()
    buffer: tuple[bytes, str] = case()  # (payload, declared content-type — "" when the ingress claims none)
    file: tuple[Path, str] = case()     # (path, declared content-type — "" falls back to the extension MIME)

    @staticmethod
    def Buffer(payload: bytes, /, *, claimed: str = "") -> "Source":
        return Source(buffer=(payload, claimed))

    @staticmethod
    def File(path: Path, /, *, claimed: str = "") -> "Source":
        return Source(file=(path, claimed))

    @property
    def length(self) -> int:
        match self:
            case Source(tag="buffer", buffer=(payload, _)):
                return len(payload)
            case Source(tag="file", file=(path, _)):
                return path.stat().st_size
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def claimed(self) -> str:  # the explicit ingress claim, or for a File the mimetypes extension MIME
        match self:
            case Source(tag="buffer", buffer=(_, declared)):
                return declared
            case Source(tag="file", file=(path, declared)):
                return declared or (mimetypes.guess_file_type(path)[0] or "")
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------
_EXTENSION_MIN: int = 524  # libmagic floor for MAGIC_EXTENSION
_CONTINUE_SEP: str = "\n- "  # libmagic MAGIC_CONTINUE multi-match separator

# the transient `to_process` worker death recovered before the boundary rails it; the MagicParam
# recursion/ELF caps stay the magic-bomb defense, so retry recovers an OOM/signal death, never a deterministic crash
_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)

_FACET_FLAG: frozendict[MagicFacet, str] = frozendict({
    MagicFacet.MIME: "mime",
    MagicFacet.ENCODING: "mime_encoding",
    MagicFacet.EXTENSION: "extension",
})

_PROFILE_FACETS: frozendict[DetectProfile, tuple[MagicFacet, ...]] = frozendict({
    DetectProfile.MIME: (MagicFacet.MIME,),
    DetectProfile.DESCRIBE: (MagicFacet.DESCRIPTION,),
    DetectProfile.IDENTITY: (MagicFacet.MIME, MagicFacet.DESCRIPTION, MagicFacet.ENCODING, MagicFacet.EXTENSION),
})

_MEDIA_CLASS: frozendict[str, MediaClass] = frozendict({
    "application/pdf": MediaClass.PDF,
    "application/epub+zip": MediaClass.EBOOK,
    "application/x-mobipocket-ebook": MediaClass.EBOOK,
    "application/encrypted": MediaClass.ENCRYPTED,
    "application/x-ole-storage": MediaClass.OFFICE_LEGACY,
    "application/CDFV2": MediaClass.OFFICE_LEGACY,
    "application/msword": MediaClass.OFFICE_LEGACY,
    "application/vnd.ms-excel": MediaClass.OFFICE_LEGACY,
    "application/vnd.ms-powerpoint": MediaClass.OFFICE_LEGACY,
    "image/svg+xml": MediaClass.VECTOR,
    "application/zip": MediaClass.ARCHIVE,
    "application/x-7z-compressed": MediaClass.ARCHIVE,
    "application/gzip": MediaClass.ARCHIVE,
    "application/x-tar": MediaClass.ARCHIVE,
    "application/x-bzip2": MediaClass.ARCHIVE,
    "application/x-xz": MediaClass.ARCHIVE,
    "application/zstd": MediaClass.ARCHIVE,
    "application/x-lz4": MediaClass.ARCHIVE,
    "application/vnd.openxmlformats-officedocument.wordprocessingml": MediaClass.WORD,
    "application/vnd.openxmlformats-officedocument.spreadsheetml": MediaClass.SPREADSHEET,
    "application/vnd.openxmlformats-officedocument.presentationml": MediaClass.PRESENTATION,
    "application/vnd.oasis.opendocument": MediaClass.OFFICE_ODF,
    "application/vnd.ms-opentype": MediaClass.FONT,
    "application/font": MediaClass.FONT,
    "application/json": MediaClass.DATA,
    "application/xml": MediaClass.DATA,
    "text/xml": MediaClass.DATA,
    "application/yaml": MediaClass.DATA,
    "application/x-yaml": MediaClass.DATA,
    "text/yaml": MediaClass.DATA,
    "application/toml": MediaClass.DATA,
    "text/csv": MediaClass.DATA,
    "application/csv": MediaClass.DATA,
    "model/gltf-binary": MediaClass.MODEL,
    "model/gltf+json": MediaClass.MODEL,
    "model/vnd.usdz+zip": MediaClass.MODEL,
    "model/stl": MediaClass.MODEL,
    "model/obj": MediaClass.MODEL,
    "model/3mf": MediaClass.MODEL,
    "image/": MediaClass.IMAGE,
    "audio/": MediaClass.AUDIO,
    "video/": MediaClass.VIDEO,
    "model/": MediaClass.MODEL,
    "font/": MediaClass.FONT,
    "text/": MediaClass.TEXT,
})

# the structural container the sniffed MIME sits inside, orthogonal to MediaClass: a `.docx` is
# (WORD, ZIP), a legacy `.doc` is (OFFICE_LEGACY, OLE), a bare archive is (ARCHIVE, its compression
# kind) — the discriminant a consumer routes a second-pass unpacker (stream-unzip/olefile/py7zr/…) on.
_CONTAINER: frozendict[str, Container] = frozendict({
    "application/zip": Container.ZIP,
    "application/epub+zip": Container.ZIP,
    "application/vnd.comicbook+zip": Container.ZIP,
    "application/vnd.openxmlformats-officedocument.wordprocessingml": Container.ZIP,
    "application/vnd.openxmlformats-officedocument.spreadsheetml": Container.ZIP,
    "application/vnd.openxmlformats-officedocument.presentationml": Container.ZIP,
    "application/vnd.oasis.opendocument": Container.ZIP,
    "model/vnd.usdz+zip": Container.ZIP,
    "application/x-ole-storage": Container.OLE,
    "application/CDFV2": Container.OLE,
    "application/msword": Container.OLE,
    "application/vnd.ms-excel": Container.OLE,
    "application/vnd.ms-powerpoint": Container.OLE,
    "application/x-tar": Container.TAR,
    "application/x-7z-compressed": Container.SEVENZIP,
    "application/gzip": Container.GZIP,
    "application/x-bzip2": Container.BZIP2,
    "application/x-xz": Container.XZ,
    "application/zstd": Container.ZSTD,
    "application/x-lz4": Container.LZ4,
})


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class DetectPolicy:
    flags: frozenset[DetectFlag] = field(default_factory=frozenset)
    params: frozendict[MagicParam, int] = field(default_factory=frozendict)
    magic_file: Path | None = None


class DetectIdentity(Struct, frozen=True, gc=False):
    mime: str = ""
    description: str = ""
    encoding: str = ""
    extensions: tuple[str, ...] = ()
    media_class: MediaClass = MediaClass.UNKNOWN
    container: Container = Container.NONE
    matches: tuple[str, ...] = ()
    claimed: str = ""
    trust: Trust = Trust.UNKNOWN
    byte_length: int = 0
    libmagic_version: int = 0


@dataclass(frozen=True, slots=True, kw_only=True)
class Detect:
    profile: DetectProfile = DetectProfile.IDENTITY
    policy: DetectPolicy = field(default_factory=DetectPolicy)
    disposition: Disposition = Disposition.ABORT

    @overload
    async def of(self, source: Source, /) -> RuntimeRail[DetectIdentity]: ...
    @overload
    async def of(self, source: Iterable[Source], /) -> RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]: ...
    async def of(self, source: Source | Iterable[Source], /) -> RuntimeRail[DetectIdentity] | RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]:
        match source:
            case Source() as one:
                return await self._railed(one)
            case sources:
                return await self._many(Block.of_seq(sources))

    async def _railed(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        return await async_boundary(
            f"detect.{self.profile}",
            lambda: _WORKER_RETRY(to_process.run_sync, _gated_detect, source, self.profile, self.policy, limiter=GATED_BAND),
        )

    async def _many(self, sources: Block[Source], /) -> RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]:
        async with anyio.create_task_group() as group:
            handles: Block[TaskHandle[RuntimeRail[DetectIdentity]]] = sources.map(lambda one: group.start_soon(self._railed, one))
        return traversed(handles.map(lambda handle: handle.return_value), by=self.disposition)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _classified[E](mime: str, table: frozendict[str, E], default: E, /) -> E:
    # exact `.get` first, then the longest registered prefix the mime extends — a `/` top-level type
    # (`image/`) or a dotted compound subtype (`...wordprocessingml` catching the `.document`/`.sheet`
    # suffix modern libmagic appends) — so the most specific row wins and no shorter prefix shadows it.
    direct = table.get(mime)
    if direct is not None:
        return direct
    prefix = max((key for key in table if mime.startswith(key)), key=len, default="")
    return table[prefix] if prefix else default


@cache
def _cookie(magic_file: Path | None, facet_flag: str | None, flagged: frozendict[str, bool], params: frozendict[MagicParam, int], /) -> "magic.Magic":
    import magic

    cookie = magic.Magic(magic_file=str(magic_file) if magic_file is not None else None, **({facet_flag: True} if facet_flag else {}), **flagged)
    for param, value in params.items():
        cookie.setparam(getattr(magic, param.value), value)  # tuned once per cooked config; the cookie (and its loaded database) is cached across detections in the worker
    return cookie


def _cooked(source: Source, facet: MagicFacet, policy: DetectPolicy, flagged: frozendict[str, bool], /) -> str:
    cookie = _cookie(policy.magic_file, _FACET_FLAG.get(facet), flagged, policy.params)
    match source:
        case Source(tag="buffer", buffer=(payload, _)):
            return cookie.from_buffer(payload)
        case Source(tag="file", file=(path, _)):
            return cookie.from_file(path)
        case _ as unreachable:
            assert_never(unreachable)


def _trust(mime: str, media_class: MediaClass, container: Container, extensions: tuple[str, ...], matches: tuple[str, ...], claimed: str, /) -> Trust:
    declared = MediaClass.of(claimed) if claimed else MediaClass.UNKNOWN
    claimed_container = Container.of(claimed) if claimed else Container.NONE
    claimed_exts = frozenset(suffix.lstrip(".") for suffix in mimetypes.guess_all_extensions(claimed))  # the claim's valid extensions, dot-stripped to libmagic's form
    return (
        Trust.UNKNOWN if media_class is MediaClass.UNKNOWN or mime in ("", "application/octet-stream")
        else Trust.AMBIGUOUS if len(matches) > 1
        else Trust.IDENTIFIED if declared is MediaClass.ARCHIVE and claimed_container is container is not Container.NONE  # a generic-container claim naming the sniffed container is a generalization, not a spoof
        else Trust.MISMATCH if declared is not MediaClass.UNKNOWN and declared is not media_class  # cross-class spoof
        else Trust.MISMATCH if declared is media_class and extensions and claimed_exts and claimed_exts.isdisjoint(extensions)  # same-class extension/label spoof
        else Trust.IDENTIFIED
    )


def _gated_detect(source: Source, profile: DetectProfile, policy: DetectPolicy, /) -> DetectIdentity:
    import magic

    try:
        version = magic.version()
    except NotImplementedError:  # ancient libmagic lacks magic_version — detection proceeds without the extension hint
        version = 0
    flagged: frozendict[str, bool] = frozendict({flag.value: True for flag in policy.flags})
    facets = tuple(f for f in _PROFILE_FACETS[profile] if f is not MagicFacet.EXTENSION or version >= _EXTENSION_MIN)
    cooked: frozendict[MagicFacet, str] = frozendict({f: _cooked(source, f, policy, flagged) for f in facets})
    strongest = lambda raw: raw.split(_CONTINUE_SEP, 1)[0]
    mime = strongest(cooked.get(MagicFacet.MIME, ""))
    primary = cooked.get(MagicFacet.DESCRIPTION) or cooked.get(MagicFacet.MIME) or ""
    media_class = MediaClass.of(mime)
    container = Container.of(mime)
    matches = tuple(primary.split(_CONTINUE_SEP)) if DetectFlag.KEEP_GOING in policy.flags else ()
    extensions = tuple(e for e in strongest(cooked.get(MagicFacet.EXTENSION, "")).split("/") if e and e != "???")
    claimed = source.claimed
    return DetectIdentity(
        mime=mime,
        description=strongest(cooked.get(MagicFacet.DESCRIPTION, "")),
        encoding=strongest(cooked.get(MagicFacet.ENCODING, "")),
        extensions=extensions,
        media_class=media_class,
        container=container,
        matches=matches,
        claimed=claimed,
        trust=_trust(mime, media_class, container, extensions, matches, claimed),
        byte_length=source.length,
        libmagic_version=version,
    )
```

## [03]-[RESEARCH]

- [MEDIA_DETECT_GATE] [RESOLVED]: the `Magic(mime=False, magic_file=None, mime_encoding=False, keep_going=False, uncompress=False, raw=False, extension=False)` flag-pinned cookie constructor (`[03]-[ENTRYPOINTS]` configured-cookie row `[01]`), the `Magic.from_buffer`/`from_file` per-source rows `[02]`/`[03]` (the `from_descriptor` row `[04]` excluded — the gated `to_process` worker cannot resolve a parent-process fd), the `Magic.setparam`/`getparam` tuning rows `[05]`/`[06]`, the `mime`/`mime_encoding`/`extension`/`uncompress`/`keep_going`/`raw` flag-boolean vocabulary (`[03]` flag table rows `[01]`-`[06]` mapping to `MAGIC_MIME_TYPE`/`MAGIC_MIME_ENCODING`/`MAGIC_EXTENSION`/`MAGIC_COMPRESS`/`MAGIC_CONTINUE`/`MAGIC_RAW`), the `magic_file=` custom-database row `[07]`, the `MAGIC_PARAM_INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/budget plus `ELF_NOTES_MAX`/`ELF_PHNUM_MAX`/`ELF_SHNUM_MAX` ELF-table ordinal family (`[03]` flag table row `[08]`), the `MagicException` engine fault carrying `.message` (`[02]-[PUBLIC_TYPES]` row `[02]`), and `version()` (`[03]` stateless row `[04]`) all verify against the folder `python-magic` `.api` (`0.4.27`). The `_FACET_FLAG` one-flag-per-cook law is the `[04]-[IMPLEMENTATION_LAW]` output axis — a `Magic` cookie returns one cooked string per call, so the page pins exactly one boolean per facet (`mime` alone, then `mime_encoding` alone) rather than `Magic(mime=True, mime_encoding=True)` which returns the combined `text/plain; charset=utf-8` form, and a multi-output identity holds one cookie per facet in the one worker crossing, each cookie `functools.cache`-memoised by its `(magic_file, facet flag, flags, params)` config so the compiled magic database loads once per config per worker rather than once per facet per detection. The `extension` flag requires libmagic `>= 524`, so `version()` both gates the `EXTENSION` facet and rides onto `DetectIdentity.libmagic_version`; the `_handle509Bug` null-result quirk returns `application/octet-stream` (a valid unknown-content MIME classified `MediaClass.UNKNOWN`, not a fault), and a genuinely broken magic database's `MagicException` is the worker raise the faults `CLASSIFY` catch-all `boundary` case lands. The `MediaClass.of` exact-then-prefix fold over `_MEDIA_CLASS` is the routing discriminant the document/raster/Office/scene consumers dispatch on, never a re-parse of the raw MIME string.
- [MODEL_ROUTE] [RESOLVED]: `MediaClass.MODEL` is the IANA `model/*` top-level-type routing branch the artifacts scene plane (`scene/stage#STAGE` USD/USDZ authoring, `scene/export#EXPORT` glTF/VRML/OBJ export) consumes — modern libmagic emits `model/gltf-binary`/`model/gltf+json`/`model/stl`/`model/obj`/`model/3mf` and the registered `model/vnd.usdz+zip`, the exact-then-prefix `_MEDIA_CLASS` rows plus the `model/` prefix folding all of them to one discriminant. A `usdc`-binary or zip-packaged `usdz` libmagic cannot disambiguate falls to `application/octet-stream`/`application/zip` (the `UNKNOWN`/`ARCHIVE` floor) — a libmagic database limit, not a routing defect — and the scene consumer owns the `MODEL`→reader table exactly as the document/image consumers own theirs. The branch is justified by both the DOMAIN (IANA `model/*` is a registered top-level type the 3D-artifact ingest demands) and the CONSUMER (`scene/stage`/`scene/export` are realized artifacts-folder owners reading 3D model files), beside the existing image/audio/video/font/archive families.
- [EBOOK_ROUTE] [RESOLVED]: `MediaClass.EBOOK` routes the `application/epub+zip` and `application/x-mobipocket-ebook` document containers modern libmagic emits to the `pymupdf` document reader, which opens EPUB/MOBI/FB2/CBZ through the same `fitz.open` surface its `application/pdf` branch uses while `pypdf` stays PDF-only — so the gate stops classifying an ebook the document plane can read as `UNKNOWN`. The branch is justified by both the DOMAIN (EPUB/MOBI are registered ebook container formats the document ingest demands) and the CONSUMER (`pymupdf` is a realized artifacts reader opening them), beside the `PDF`/`WORD`/`SPREADSHEET`/`PRESENTATION` document families; an ebook libmagic cannot disambiguate from a bare zip falls to the `application/zip` `ARCHIVE` floor exactly as the OOXML/USDZ zip containers do — a libmagic database limit, not a routing defect, and the document consumer owns the `EBOOK`→reader edge exactly as the `PDF`/image consumers own theirs.
- [BATCH_MODALITY] [RESOLVED]: `Detect.of` is the one polymorphic modality entry over `Source | Iterable[Source]` normalized once by a total `match` (`MODAL_ARITY` doctrine), discriminating on the value shape rather than a `batch: bool` knob or a `detect_many` sibling — the ingest gate sees a stream of incoming files (a folder import, an archive expansion), the defining batch modality detect carries that the single-artifact `metadata`/`conformance` siblings do not. The singular arm rails one `async_boundary` crossing; the plural `_many` arm opens one `anyio.create_task_group`, fans each source through the same per-source `async_boundary`-railed `_railed` child the singular arm uses, whose `TaskHandle.return_value` settles on the all-clean path (the child rails its own fault so the group never raises through the `return_value` read), and threads the gathered `Block[RuntimeRail[DetectIdentity]]` through the runtime `faults.traversed` reducer under the owner's `Disposition` policy value — the boundary's correctness decision, not a fixed abort. A `Source.Buffer` carries the payload in memory so its only faults are systemic (a `BrokenWorkerProcess` worker death, a provisioning `ImportError`, a broken-database `MagicException`) and `ABORT` (the default) fails the batch fast; a `Source.File` the worker reads can raise a per-source `FileNotFoundError`/`OSError` (a missing, unreadable, or mid-import-deleted file) that is NOT systemic, so a folder import selects `ACCUMULATE` to fold every casualty into one `aggregate` `BoundaryFault` or `PARTITION` to keep the resolved `Block[DetectIdentity]` survivors beside the `Block[BoundaryFault]` casualties rather than losing every good identification to one unreadable file; an unknown-content payload resolves `MediaClass.UNKNOWN` rather than a fault on every disposition. The fan is bounded by the shared `GATED_BAND` `CapacityLimiter` on every `to_process.run_sync(limiter=...)` call, so N sources spawn N coroutines but at most `GATED_BAND` concurrent subprocess workers, the `concurrency.md` `gathered` shape (task group as the failure boundary, `CapacityLimiter` as the resource bound), never an unbounded subprocess fan-out.
- [DETECT_SEAM] [RESOLVED]: `_gated_detect` runs on the gated companion band through `anyio.to_process.run_sync(_gated_detect, source, profile, policy, limiter=GATED_BAND)` (branch `anyio` `.api` `[03]-[ENTRYPOINTS]` offload row `[06]` `to_process.run_sync(func, *args, cancellable=False, limiter=None)`), importing `magic` at boundary scope inside the worker, never on the cp315-core owner — `python-magic` is a pure-Python ctypes binding with no compiled extension, but at import time `loader.load_lib()` resolves the system `libmagic` shared object and raises `ImportError('failed to find libmagic')` when absent (`[01]-[PACKAGE_SURFACE]` abi line — the native `libmagic` + its magic database are a Forge-provisioned host dependency, NOT a wheel, so runtime detection is provisioning-gated until libmagic is on the loader path while the Python member surface is independent of provisioning). A subinterpreter shares the host loader path and cannot resolve the host-native libmagic any better than the core process, so detection crosses this genuine separate-process seam onto the provisioned gated-band worker exactly as `graphic/raster/io#RASTER` runs its raster-ingress gate on the same band — distinct from the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload which cannot host the host-native binding — and `_gated_detect`/`_cooked` are module-level functions dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure). A worker death surfaces as `anyio.BrokenWorkerProcess` (branch `anyio` `.api` `[02]-[PUBLIC_TYPES]` worker-error row `[04]`) the runtime `reliability/faults#FAULT` `CLASSIFY` `resource` row lands, the libmagic-missing `ImportError` the `import_` row, and a `MagicException` the catch-all `boundary` case — every worker raise pickles back through the seam into the one `async_boundary` conversion, no `magic` import landing on the core page.
- [GATED_BAND_OWNER] [OPEN]: the shared `to_process` companion-band `CapacityLimiter` `concurrency.md` mandates over `to_process.current_default_process_limiter()` has no realized runtime owner — `reliability/faults#FAULT` (read in full) mints `BoundaryFault`/`RuntimeRail`/`CLASSIFY`/`boundary`/`async_boundary`/`traversed`/`railed` and owns NO limiter, while `execution/lanes#LANE` owns the per-`LanePolicy`-identity `functools.cache`-memoised `to_interpreter` limiter, a distinct resource from the host-native `to_process` companion band. This page imports `from rasm.runtime.lanes import GATED_BAND` and passes `limiter=GATED_BAND` to bound the subprocess workers, the doctrine-correct explicit-limiter form; the cross-file resolution is for `execution/lanes#LANE` to export the shared `GATED_BAND` `CapacityLimiter` (sized by `os.process_cpu_count()`) for the host-native companion band, and for every gated artifacts page (`exchange/metadata`, `graphic/raster/io`/`measure`/`process`, `graphic/color/managed`, `document/emit`/`lens`/`report`, `graphic/marks/decode`, `media/audio`/`video`, `package/codec`, `scene/render`/`export`/`stage`, `typography/shape`, `visualization/chart/export`/`transform`, `visualization/diagram/layout`, `composition/compose`) to import it and pass `limiter=GATED_BAND`, replacing the corpus-wide false `reliability/faults#FAULT`-owns-the-limiter attribution and the unbounded bare `to_process.run_sync(...)` offload.
- [RASTER_COMPOSE] [RESOLVED]: `graphic/raster/io#RASTER` composes this standalone format-ID owner for its `RasterOp.Detect` raster-ingress gate rather than re-declaring a parallel magic surface — the raster owner resolves the `MIME` profile on the same gated `to_process` worker that imports `magic` at boundary scope (`[04]-[IMPLEMENTATION_LAW]` boundary naming `pillow`/`pyvips` as the image MIME-branch consumers), so the raster ingress reads this owner's `MediaClass.IMAGE` discriminant while this owner stays the full-identity format-ID gate the document/Office/scene owners read; the `compat` shim functions are deprecation-wrapped and NOT admitted (they collide with the upstream file-5.x binding), so both owners use the `Magic` cookie / `from_buffer` rows and bound on the one `execution/lanes#LANE`-owned `GATED_BAND` limiter, never a per-owner re-minted limiter and never the false faults-owned attribution. The `MediaClass` routing — `PDF`→`pymupdf`/`pypdf`, `EBOOK`→`pymupdf`, `WORD`→`python-docx`, `SPREADSHEET`→`openpyxl`, `PRESENTATION`→`python-pptx`, `OFFICE_ODF`→`odfpy`, `VECTOR`→`svgelements`/`resvg-py`, `IMAGE`→`pillow`/`pyvips`, `ENCRYPTED`/`OFFICE_LEGACY`→`msoffcrypto-tool`, `AUDIO`/`VIDEO`→`av`, `MODEL`→`scene/stage#STAGE`/`scene/export#EXPORT`, `ARCHIVE`→`package/archive`, `FONT`→`typography/font#FONT` — is the admission-gate dispatch the `DetectIdentity.media_class` discriminant feeds, never a parallel per-owner sniff; the consumers own the `MediaClass`→reader table, this owner owns only the classification.
- [TRUST_VERDICT] [RESOLVED]: the gate folds its own `extensions`/`matches`/`claimed` evidence into one `Trust` verdict rather than emitting an unread tuple — `_trust` lands `UNKNOWN` on the `application/octet-stream` content floor (the `_handle509Bug` null-result MIME and every `MediaClass.UNKNOWN` sniff), `AMBIGUOUS` when `keep_going` (`MAGIC_CONTINUE`) returns more than one strong match (the polyglot the splitter already separates into `matches`), `MISMATCH` when the sniffed `MediaClass` disagrees with a known-class `claimed` declared content-type (the MIME-confusion spoof a content-type-trusting reader dispatch would mis-route) or when a same-class claim's `mimetypes.guess_all_extensions` set is disjoint from the sniffed `extensions` (the extension/label spoof within one routing class a class-only check misses), and `IDENTIFIED` for a confident sniff agreeing with the claim or unclaimed. The `claimed` declaration rides each `Source` case beside its locator — the explicit ingress content-type (an HTTP `Content-Type`, an upload envelope), or for a `File` the `mimetypes.guess_file_type` (`system-apis.md` FILE_IO owner) extension MIME — and the trust derivation is pure, so the worker folds it onto `DetectIdentity.trust` in the one crossing with no second native call. The extension is justified on three axes: the DOMAIN (MIME-confusion and polyglot content are the registered ingest-boundary attack classes a content-sniffing gate exists to catch), the PACKAGE (`MAGIC_CONTINUE`'s multi-match plus the `MAGIC_EXTENSION` valid-extension hints the `_trust` cross-check folds against `mimetypes.guess_all_extensions(claimed)`, and the `mimetypes` declared-type evidence the page computes), and the CONSUMER (the document/raster/Office/scene owners dispatch on `media_class` to exactly one reader, so a spoofed payload routed past a claimed-blind gate reaches the wrong reader); it lands as one `Trust` member family, one `Source.claimed` field, one `DetectIdentity.trust` field, and one `_trust` fold inside the existing owners — never a parallel verdict surface, and orthogonal to the `exchange/conformance#CONFORMANCE` PDF-cryptographic `ConformanceVerdict` (a signature/timestamp axis, not a format-identification one). `mimetypes.guess_file_type`/`guess_all_extensions` and `msgspec.structs.asdict` verify against the stdlib and the folder `msgspec` `.api`; `TaskHandle.return_value` (not the absent `.result()`) is the settled-child read the `concurrency.md` `TASK_GROUP` `CHILD_CARRIER` law names.
