# [PY_ARTIFACTS_DETECT]

The media-type / file-info / format-identification owner at the ingest boundary. `Detect` is the configured detector — a `DetectEngine`, a `DetectProfile`, a `DetectPolicy`, and a `deepscan` toggle — whose one polymorphic `of` admits a `Source` or an `Iterable[Source]` and sniffs each payload into a typed `DetectIdentity` carrying the MIME type, the human description, the charset encoding, the valid-extension tuple, the derived `MediaClass` routing discriminant, the orthogonal `Container` structural discriminant (the `ZIP`/`OLE`/`TAR`/compression kind a `.docx`/legacy-`.doc`/archive sits inside, which a consumer routes a second-pass unpacker on), the multi-match set, the ingress-declared `claimed` content-type, the `Trust` verdict folding the sniffed-vs-claimed evidence into `IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`, the sniff `confidence`, the resolving `engine`, the input byte length, and the resolving `libmagic` version — never an extension-table guesser, never a per-output function family, never a per-source detector type, never a `detect_many` sibling beside the singular call. `Detect` is the DUAL-ENGINE owner over ONE surface: `puremagic` is the in-process pure-Python DEFAULT — a confidence-ranked `PureMagicWithConfidence` roster whose `single_deep_scan` resolves the `PK\x03\x04` ZIP and `\xd0\xcf\x11\xe0` CFBF containers to the EXACT OOXML/ODF/EPUB/USDZ + legacy-Office subtype (closing the `application/zip`/`application/CDFV2` floor `python-magic` lands on) and whose float `confidence` ∈ `[0.0, 1.0]` is the native `Trust.AMBIGUOUS`/`UNKNOWN` signal `libmagic` can only approximate through `keep_going`; `python-magic`/libmagic is RETAINED as the broad-leaf-signature fallback ONLY where its compiled magic database recognizes a niche signature `magic_data.json` lacks — a strict-stronger-on-a-distinct-axis retention, never an overlap. The `DetectEngine` axis (`PUREMAGIC`/`LIBMAGIC`/`LAYERED`) composes the two under one owner: `puremagic` rides the runtime loader path IN-PROCESS on an `anyio.to_thread.run_sync` arm (the pure-Python parse + bounded head/foot read + ZIP/CFBF central-directory read is blocking-I/O-and-bounded-CPU the thread band owns, sharing the address space so a `Buffer` payload never pickles), while `libmagic` — a Forge-provisioned host dependency NOT on the loader path — crosses the `anyio.to_process.run_sync` subprocess seam bounded by the `execution/lanes#LANE`-owned shared `WORKER_BAND` `CapacityLimiter` with a transient `BrokenWorkerProcess` death recovered by a bounded `stamina.AsyncRetryingCaller` retry before `async_boundary` rails the exhausted failure. The `LAYERED` default runs `puremagic` and escalates ONLY a resolved-but-`UNKNOWN` verdict to the broad `libmagic` database, so a confident sniff never pays the process crossing and a libmagic provisioning fault keeps the honest `UNKNOWN`. This is the ingest-boundary format-ID gate: every detection resolves a `DetectIdentity` the per-format reader dispatch reads through the typed `MediaClass` discriminant, minting no content key and contributing no `ArtifactReceipt` of its own — content-addressing is the `evidence/identity#IDENTITY` owner's concern and the descriptive-metadata fields inside the identified container are the `exchange/metadata#METADATA` owner's.

## [01]-[INDEX]

- [01]-[DETECT]: the dual-engine ingest-boundary format-identification owner that IS the `Detect` frozen dataclass over a `DetectEngine`/`DetectProfile`/`DetectPolicy`/`deepscan` config and the one polymorphic `of(Source | Iterable[Source])` entry; `DetectEngine` the closed `PUREMAGIC`/`LIBMAGIC`/`LAYERED` provider-selection axis whose `_railed` `match` routes the pure-Python in-process default (`to_thread`, `_pure_detect` over the `puremagic` `magic_file`/`identify_all` confidence roster + `single_deep_scan` exact OOXML/CFBF subtype), the native worker-band fallback (`to_process`/`WORKER_BAND` + `stamina.AsyncRetryingCaller.on(BrokenWorkerProcess)` retry, `_gated_detect` over the `python-magic` flag-pinned `Magic` cookie), and the layered escalate-`UNKNOWN`-only composition; `Source` the closed `Buffer`/`File` admission family (each `@beartype(conf=FAULT_CONF)`-woven factory guarding the `bytes`/`Path` ingress, the `File` path read worker-side so the parent never pickles the payload) exposing `length`/`claimed` projections; `DetectProfile` the `MIME`/`DESCRIBE`/`IDENTITY` output vocabulary whose `_PROFILE_FACETS` selects the libmagic `MagicFacet` tuple; `DetectPolicy` the one behavior value folding the libmagic `DetectFlag` set, the `MagicParam` `MAGIC_PARAM_*` recursion/ELF caps, the `CheckClass` `MAGIC_NO_CHECK_*` test-class narrowing, and the custom `.mgc` `magic_db`; `DetectSettings` the `pydantic-settings` env-admission owner (`RASM_DETECT_` prefix) projecting the deployment `engine`/`deepscan`/`magic_db` into a configured `Detect`; `MediaClass`/`Container`/`Trust` the derived routing / structural / declared-vs-sniffed vocabularies the shared `_classified` exact-then-longest-prefix fold and the `_trust` confidence-gated fold resolve; `DetectIdentity` the one frozen `msgspec.Struct` result — its own wire/egress projection carrying the sniffed fields plus `confidence` and `engine` — every arm folds; `_pure_roster`/`_pure_detect` the puremagic in-process worker and `_cookie`/`_cooked`/`_gated_detect` the libmagic subprocess worker, both converging on the one `DetectIdentity` the consumers dispatch on and neither minting a `ContentKey` nor contributing an `ArtifactReceipt`.

## [02]-[DETECT]

- Owner: `Detect` the one format-identification owner carrying a `DetectEngine` provider-selection axis, a `DetectProfile`, a `DetectPolicy`, a `deepscan` toggle, and a `Disposition` batch-combination policy (the runtime `faults.traversed` `by=` parameter — `ABORT`/`ACCUMULATE`/`PARTITION`); `DetectEngine` the closed `PUREMAGIC`/`LIBMAGIC`/`LAYERED` `StrEnum` whose `_railed` total `match` selects the arm — `PUREMAGIC` the pure-Python in-process default over `anyio.to_thread.run_sync` (no process crossing, no `BrokenWorkerProcess` retry, no payload pickle), `LIBMAGIC` the native worker-band arm over `anyio.to_process.run_sync(limiter=WORKER_BAND)` with the `_WORKER_RETRY` transient-death recovery, `LAYERED` the escalate-`UNKNOWN`-only composition (`puremagic` first, `libmagic` only when the resolved verdict is `MediaClass.UNKNOWN`, a libmagic fault kept subordinate to the puremagic `UNKNOWN`) — collapsing a would-be pair of parallel detector pages into one polymorphic owner exactly as the media capability-detection contract composes two arms under one filter node. `Source` an `expression.tagged_union` closed admission family — `Buffer` the in-memory-bytes canonical row admission already holds, `File` the on-disk path the worker reads so the parent never pickles the payload across the process seam — each `@beartype(conf=FAULT_CONF)`-woven factory guarding its `bytes`/`Path` ingress (so a malformed source lifts onto the runtime fault rail rather than a native `TypeError` deep in the sniff), carrying the ingress-declared content-type beside its locator and exposing its own `length` size projection and `claimed` content-type projection (the explicit declaration, or for a `File` the puremagic `ext_from_filename` compound-extension recovery + `from_extension` reverse-lookup MIME over the thinner stdlib `mimetypes.guess_file_type` fallback) so the caller never branches on source kind; `DetectProfile` the closed `StrEnum` output vocabulary (`MIME`/`DESCRIBE`/`IDENTITY`) whose `_PROFILE_FACETS` primary row selects the libmagic `MagicFacet` tuple (the `puremagic` arm resolves the full identity in one roster call, so the profile is the facet-cookie selector on the libmagic arm and the projection breadth marker on the pure arm); `MagicFacet` (`MIME`/`ENCODING`/`EXTENSION`/`DESCRIPTION`) the atomic libmagic cookable outputs each pinning exactly one `Magic` cookie flag through the `_FACET_FLAG` derivation, because libmagic returns one cooked string per call; `DetectPolicy` the one behavior value folding the libmagic `DetectFlag` set (`uncompress`/`keep_going`/`raw`), the `MagicParam` `MAGIC_PARAM_*` recursion/ELF tuning caps, the `CheckClass` `MAGIC_NO_CHECK_*` test-class narrowing, and the custom `.mgc` `magic_db` into one owner instead of scattering flags across cases or threading them as per-call arguments; `DetectSettings` the `pydantic-settings` `BaseSettings` env-admission owner admitted at the composition root (the `RASM_DETECT_` prefix over `engine`/`deepscan`/`magic_db`, the discovery-env→configured-path→bundled-fallback resolution of the libmagic `.mgc` database), whose `detector()` projects one configured `Detect` so the deployment env is admitted once and never re-read raw; `DetectIdentity` the one typed result every arm folds into, carrying the `claimed` declared type, the `Trust` verdict, the `Container` structural discriminant, the float `confidence`, the resolving `engine`, and the charset `encoding` BOTH arms now populate (the pure default arm through `text_scanner.decode_any`, symmetric with the libmagic `mime_encoding` cook that alone set it before) beside the sniffed fields — a frozen `msgspec.Struct` that IS its own wire/egress projection a consumer renders to span attributes through `msgspec.to_builtins`/`structs.asdict` directly, never a forwarding `facts()` hop; `MediaClass` the derived routing discriminant and `Container` the orthogonal structural discriminant, both folded from the one sniffed MIME by the shared `_classified` exact-then-longest-prefix fold over `_MEDIA_CLASS`/`_CONTAINER` (a `.docx` resolving `WORD`+`ZIP`, a legacy `.doc` `OFFICE_LEGACY`+`OLE`, a bare archive `ARCHIVE`+its compression kind) so a consumer dispatches a reader on `media_class` and a second-pass unpacker on `container` without re-parsing the MIME string; `Trust` the closed declared-vs-sniffed verdict (`IDENTIFIED`/`AMBIGUOUS`/`MISMATCH`/`UNKNOWN`) the `_trust` fold derives over the `media_class`, the `container`, the multi-match set, the sniffed `extensions`, the `Source.claimed` content-type, AND the sniff `confidence` (the `puremagic` native ambiguity signal below `_CONFIDENCE_FLOOR` collapsing to `UNKNOWN`, two distinct strong matches to `AMBIGUOUS`), so a spoofed or polyglot or low-confidence payload is a verdict the gate states rather than evidence it silently discards. The `_FACET_FLAG` is the libmagic flag-policy collapse — a row maps a facet to the single `Magic` boolean it pins — so the worker constructs one flag-pinned cookie per facet, never a `tuple[bool, bool, bool]` triple and never a re-discriminating `match` inside an arm.
- Cases: `DetectEngine` rows — `PUREMAGIC` (the pure-Python default, `_pure_detect` over the `puremagic` roster on the `to_thread` `_SNIFF_GATE` band, deep-scan folding the exact ZIP/CFBF subtype) · `LIBMAGIC` (the native fallback, `_gated_detect` over the `python-magic` `Magic` cookie on the `to_process` `WORKER_BAND` with the `_WORKER_RETRY` recovery) · `LAYERED` (the default composition: `_pure` first, `_libmagic` escalated ONLY on a `MediaClass.UNKNOWN` verdict, `or_else_with` keeping the puremagic `UNKNOWN` when the libmagic worker itself faults) — selected by one total `match` in `_railed`, never a `provider: str` knob the body re-pairs. `DetectProfile` rows — `MIME` (the single MIME-type gate, `{MagicFacet.MIME}` on the libmagic arm) · `DESCRIBE` (the human-description pass, `{MagicFacet.DESCRIPTION}`) · `IDENTITY` (the full pass, `{MIME, DESCRIPTION, ENCODING, EXTENSION}` each holding its own flag-pinned cookie in one libmagic crossing) — the facet set is the `_PROFILE_FACETS` primary correspondence, never separate functions per output. `MagicFacet` rows map through `_FACET_FLAG` to the cookie boolean (`MIME`→`mime`, `ENCODING`→`mime_encoding`, `EXTENSION`→`extension`, `DESCRIPTION` cooks under no flag); the `DetectFlag` policy set composes onto every libmagic facet cookie (`uncompress` looks through gzip/bzip2/xz containers, `keep_going` returns all matches, `raw` keeps unprintable bytes); the `MagicParam` caps (`INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/name/regex/byte budgets and the `ELF_NOTES_MAX`/`ELF_PHNUM_MAX`/`ELF_SHNUM_MAX` ELF-table caps) apply through `setparam` and the `CheckClass` narrowing (`COMPRESS`/`TAR`/`SOFT`/`APPTYPE`/`ELF`/`ASCII`/`TROFF`/`FORTRAN`/`TOKENS`) disables a libmagic test class through `magic_setflags` to harden an untrusted ingest against unbounded recursion, ELF-table bombs, and unneeded test classes. `Source` cases — `Buffer(payload, claimed=...)`, `File(path, claimed=...)` — matched by one total `match` in `_cooked` and `_pure_roster`. `MediaClass` rows are the routing vocabulary the `_MEDIA_CLASS` exact-then-longest-prefix fold resolves (the longest registered prefix winning so the OOXML/ODF compound subtypes route through their `...wordprocessingml`/`...opendocument` row even when the deep-scan or modern libmagic appends the `.document`/`.sheet`/`.presentation` suffix a bare exact key would miss), the `MODEL` member routing the IANA `model/*` 3D-artifact family to the `scene/stage#STAGE`/`scene/export#EXPORT` consumers, the `EBOOK` member routing `application/epub+zip`/`application/x-mobipocket-ebook` to the `pymupdf` document reader, and the `DATA` member routing `application/json`/`application/xml`/`application/yaml`/`application/toml`/`text/csv` to the structured-data readers a `text/` prefix would have floored to `TEXT`. The `Container` rows are the parallel structural axis the `_CONTAINER` table folds the same MIME into — `ZIP` for the OOXML/ODF/EPUB/USDZ/CBZ + bare-zip family, `OLE` for the CDFV2/`x-ole-storage` legacy-Office compound, and the per-compression `TAR`/`SEVENZIP`/`GZIP`/`BZIP2`/`XZ`/`ZSTD`/`LZ4` kinds — so `media_class` answers which reader and `container` answers which second-pass unpacker, orthogonal discriminants the consumer reads without a second sniff. `Trust` rows are the ingest verdict `_trust` folds in one pass — `UNKNOWN` for the `application/octet-stream` content floor, the `MediaClass.UNKNOWN` sniff, or a sub-`_CONFIDENCE_FLOOR` puremagic top match; `AMBIGUOUS` for two-or-more distinct strong matches (a polyglot); `MISMATCH` when the sniffed `MediaClass` disagrees with a known-class `claimed` declared type or when a same-class claim's `mimetypes.guess_all_extensions` set is disjoint from the sniffed `extensions`; and `IDENTIFIED` for a confident agreeing sniff or a generic-container claim (`application/zip` declared for a zip-based `.docx`) whose `Container` matches the sniffed container — the container-generalization the cross-class check would otherwise false-flag on the libmagic-floored path, gated on the claim resolving the generic `ARCHIVE` class, and needed LESS often now that the puremagic deep-scan resolves the exact subtype directly.
- Auto: `_railed` dispatches the `DetectEngine` arm through one total `match`; each arm wraps its worker in `async_boundary` (the span/`structlog` event + `RuntimeRail` typed-error envelope) so a `PureError`/`PureValueError`/`MagicException`/worker raise lands at the `CLASSIFY` boundary case. `_pure_detect` reads the `_pure_roster` — a `Buffer` deep-scanning by spilling to a bounded `NamedTemporaryFile` whose `puremagic.magic_file` roster folds the exact OOXML/CFBF subtype at `confidence == 1.0`, a `File` deep-scanning natively through `magic_file(path)`, and either dropping to the no-I/O `identify_all` table match over a `string_details`/`file_details` bounded head+foot when `deepscan` is off (the untrusted/latency-bounded pass) — traps `PureError`/`PureValueError` to an empty roster (an unmatched or empty payload is the gate's honest `UNKNOWN`, never a fatal), takes the highest-confidence head's `mime_type`/`name`/`confidence`, folds the distinct strong-match MIMEs into the `matches` polyglot set and the roster extensions into the dot-stripped `extensions` tuple, folds the bounded-head charset through `_charset(source, media_class)` (`text_scanner.decode_any` gated to the text-family class, `TypeError`-trapped) into the `encoding` the libmagic `mime_encoding` cook alone set before, and folds `MediaClass.of(mime)`/`Container.of(mime)` plus the `confidence` and the puremagic-resolved `source.claimed` (`ext_from_filename`+`from_extension` for a `File`) into the `_trust` verdict — `engine=DetectEngine.PUREMAGIC`, `libmagic_version=0`. `_gated_detect` reads `magic.version()` once (the `EXTENSION` facet gated on `>= 524`), folds each `_PROFILE_FACETS[profile]` facet through `_cooked` — the `functools.cache`-memoised `_cookie(magic_db, facet flag, flags, params, no_check)` flag-pinned `Magic` cookie (built once per config per worker, tuned by `setparam` over the `MagicParam` caps and `magic_setflags` over the `CheckClass` narrowing) dispatching `from_buffer`/`from_file` on the `Source` — into one `frozendict[MagicFacet, str]` of cooked strings in a single worker crossing, cuts the `MAGIC_CONTINUE` separator for the strongest match, splits the `extension` slash-list (dropping `???`) and the `keep_going` multi-match, and folds `MediaClass.of`/`Container.of`/`_trust` — `engine=DetectEngine.LIBMAGIC`, `confidence=1.0` (libmagic carries no confidence, so it passes the floor and trusts its single match). `LAYERED` runs `_pure` then, on a resolved `MediaClass.UNKNOWN`, `_libmagic`, returning whichever resolves and keeping the puremagic `UNKNOWN` when the libmagic worker faults (a provisioning gap never overrides a valid verdict). The libmagic `_handle509Bug` null-result quirk returns `application/octet-stream` (a valid unknown-content MIME classified `MediaClass.UNKNOWN`), never an escaping exception.
- Receipt: `Detect` is the ingest-boundary format-ID GATE, not an artifact producer — the ARCHITECTURE `[02]-[SEAMS]` `exchange/detect → python:artifacts/document` edge is the media-type gate at the ingest boundary, with no `exchange/detect → core/receipt` or `→ core/plan` seam, so this owner contributes no `ArtifactReceipt` case and mints no content key. Each detection resolves a `DetectIdentity` — the resolved MIME, description, charset, extension tuple, `MediaClass`, multi-match set, `claimed` declared content-type, `Trust` verdict, `confidence`, resolving `engine`, input byte length, and resolving libmagic version — the descriptive admission-gate evidence the document/PDF/image/scene owners read before per-format reader dispatch; it is the page's own format-ID identity, never the runtime `ContentKey` (the `evidence/identity#IDENTITY` content hash a producing owner mints over its emitted bytes, which `Detect` neither computes nor folds into). The `MediaClass` discriminant is the routing the consumers dispatch on — `PDF`→`pymupdf`/`pypdf`, `EBOOK`→`pymupdf`, `WORD`→`python-docx`, `SPREADSHEET`→`openpyxl`, `PRESENTATION`→`python-pptx`, `OFFICE_ODF`→`odfpy`, `VECTOR`→`svgelements`/`resvg-py`, `IMAGE`→`pillow`/`pyvips`, `ENCRYPTED`/`OFFICE_LEGACY`→`msoffcrypto-tool`, `AUDIO`/`VIDEO`→`av`, `MODEL`→`scene/stage#STAGE`/`scene/export#EXPORT`, `ARCHIVE`→`package/archive`, `FONT`→`typography/font#FONT`, `DATA`→`msgspec`/`lxml`/`ruamel-yaml`/`tomlkit` — so each consumer reads one closed vocabulary member resolving to exactly one reader, never re-parsing the MIME string and never one OOXML class fanning to three packages (the puremagic deep-scan splitting `.docx`/`.xlsx`/`.pptx` to their exact subtypes so docx, xlsx, and pptx each route to their own owner); the consumers own the `MediaClass`→reader table, this owner owns only the classification, and the descriptive-metadata fields INSIDE the identified container are the `exchange/metadata#METADATA` owner's concern, never re-read here.
- Growth: a new engine is one `DetectEngine` member plus one `_railed` arm; a new detection facet is one `MagicFacet` member plus one `_FACET_FLAG` row; a new profile is one `DetectProfile` member plus one `_PROFILE_FACETS` row; a new libmagic policy flag is one `DetectFlag` member the cookie folds; a further libmagic tuning param is one `MagicParam` row applied through `setparam`; a further test-class narrowing is one `CheckClass` member folded through `magic_setflags`; a custom `.mgc`/text database is the `magic_db` field on the existing policy or the `DetectSettings` env; a new routing branch is one `MediaClass` member plus one `_MEDIA_CLASS` row; a new source kind is one `Source` case; a new ingest verdict is one `Trust` member plus one arm in `_trust`; an ingress declaration rides the existing `Source` `claimed` field with zero new parameter; a new identity fact is one field on `DetectIdentity`; a new deployment knob is one `DetectSettings` field plus one `detector()` projection; the singular/batch modality is the existing `of(Source | Iterable[Source])` with zero new entrypoint; zero new surface.
- Boundary: `Detect` owns content sniffing only — it mints no `ContentKey` and contributes no `ArtifactReceipt` (identity and receipt are other owners' concerns), and reads no descriptive metadata INSIDE the identified container (the `exchange/metadata#METADATA` owner's concern). The `puremagic` default rides the runtime loader path IN-PROCESS through `anyio.to_thread.run_sync` (there is no native dependency to reify in a subprocess, so the `to_process`/`WORKER_BAND`/`BrokenWorkerProcess`/pickle seam the libmagic arm pays ALL drop; the thread band still keeps the bounded pure-Python parse and the blocking ZIP/CFBF central-directory read off the event loop, sharing the address space so a `Buffer` never pickles), while `python-magic`/libmagic — off the loader path — crosses the `to_process` `WORKER_BAND` seam a native provisioned dependency demands. The deleted forms are the SINGLE-ENGINE libmagic-only owner where `puremagic` is the categorical-best default, the `MappingProxyType` step table where `frozendict` is the owner, the per-source detector type where the `Source` cases discriminate on input, the per-output function family where `DetectProfile`/the one roster call resolve every output, the bare-string `sig_object_type`-style MIME comparisons where `MediaClass.of`/`Container.of` own the discriminant, the flag-as-per-call-argument where the flag-pinned `Magic` cookie is the owner, the `Magic(mime=True, mime_encoding=True)` combined-facet cook where one boolean per facet is the law, the unbounded `to_process.run_sync` trusting the per-loop default where `WORKER_BAND` bounds the process band, the confidence-blind `Trust` where the puremagic float `confidence` is the native ambiguity signal, the `application/zip`/`application/CDFV2` container floor where the deep-scan resolves the exact OOXML/legacy-Office subtype, the raw `os.environ` engine/DB/deepscan reads where the `DetectSettings` `BaseSettings` admits them once, the unguarded `str`/`bytes`/`Path` ingress where the `@beartype(conf=FAULT_CONF)` Source factories lift a malformed source onto the fault rail, the `magic.from_buffer` module row exposing only `mime` where the `Magic` cookie owns the facet flags, the `_pure_detect` default arm leaving `encoding` EMPTY where `text_scanner.decode_any` populates the charset in-process symmetric with the libmagic `mime_encoding` cook, the stdlib-only `mimetypes.guess_file_type` File claim where `ext_from_filename`+`from_extension` recover the compound extension over puremagic's richer ext<->MIME reverse-lookup table, the empty `[01]-[INDEX]` where the router names the owner, and the descriptive-metadata read that belongs to `exchange/metadata#METADATA`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import mimetypes
from collections.abc import Iterable
from dataclasses import dataclass, field
from enum import StrEnum
from functools import cache, reduce
from operator import or_
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Final, Literal, assert_never, overload

import anyio
import stamina
from anyio import BrokenWorkerProcess, CapacityLimiter, TaskHandle, to_process, to_thread
from beartype import beartype
from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import catch
from msgspec import Struct
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.faults import FAULT_CONF, BoundaryFault, Disposition, RuntimeRail, async_boundary, traversed
from rasm.runtime.lanes import WORKER_BAND

lazy from puremagic import (
    PureError,
    PureMagicWithConfidence,
    ext_from_filename,
    from_extension,
    magic_file,
)  # pure-Python default, on the loader path
lazy from puremagic.main import PureValueError, file_details, identify_all, string_details
lazy from puremagic.scanners import text_scanner  # in-process charset facet (decode_any), symmetric with the libmagic mime_encoding cook
lazy import magic  # libmagic native dep, off the runtime loader path; reified in the to_process worker


# --- [TYPES] ----------------------------------------------------------------------------
class DetectEngine(StrEnum):
    PUREMAGIC = "puremagic"  # pure-Python default, in-process to_thread, confidence roster + deep-scan exact subtypes
    LIBMAGIC = "libmagic"  # native libmagic worker-band fallback, broad leaf-signature database, to_process
    LAYERED = "layered"  # puremagic default → libmagic escalation on a MediaClass.UNKNOWN miss


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


# member values are the `magic.MAGIC_NO_CHECK_*` module ordinals; `getattr(magic, value)` resolves the raw
# bit the cookie disables through `magic_setflags` (no `Magic` constructor boolean exists for these).
class CheckClass(StrEnum):
    COMPRESS = "MAGIC_NO_CHECK_COMPRESS"
    TAR = "MAGIC_NO_CHECK_TAR"
    SOFT = "MAGIC_NO_CHECK_SOFT"
    APPTYPE = "MAGIC_NO_CHECK_APPTYPE"
    ELF = "MAGIC_NO_CHECK_ELF"
    ASCII = "MAGIC_NO_CHECK_ASCII"
    TROFF = "MAGIC_NO_CHECK_TROFF"
    FORTRAN = "MAGIC_NO_CHECK_FORTRAN"
    TOKENS = "MAGIC_NO_CHECK_TOKENS"


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
    NONE = "none"  # a leaf format with no wrapping structure
    ZIP = "zip"  # the OOXML/ODF/EPUB/USDZ/CBZ + bare-zip family a docx/xlsx/epub sits inside
    OLE = "ole"  # the CFB/CDFV2 compound the legacy Office + MSI family sits inside
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
    AMBIGUOUS = "ambiguous"  # two distinct strong matches — a polyglot (the puremagic confidence tail)
    MISMATCH = "mismatch"  # the sniffed class disagrees with the declared content-type — spoofing
    UNKNOWN = "unknown"  # the octet-stream floor, an UNKNOWN class, or a sub-floor confidence


@tagged_union(frozen=True)
class Source:
    tag: Literal["buffer", "file"] = tag()
    buffer: tuple[bytes, str] = case()  # (payload, declared content-type — "" when the ingress claims none)
    file: tuple[Path, str] = case()  # (path, declared content-type — "" falls back to the extension MIME)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def Buffer(payload: bytes, /, *, claimed: str = "") -> "Source":
        return Source(buffer=(payload, claimed))

    @staticmethod
    @beartype(conf=FAULT_CONF)
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
    def claimed(self) -> str:  # the explicit ingress claim, or for a File the puremagic ext<->MIME resolution
        match self:
            case Source(tag="buffer", buffer=(_, declared)):
                return declared
            case Source(tag="file", file=(path, declared)):
                return declared or _claim_mime(path)
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------
_EXTENSION_MIN: int = 524  # libmagic floor for MAGIC_EXTENSION
_CONTINUE_SEP: str = "\n- "  # libmagic MAGIC_CONTINUE multi-match separator
_CONFIDENCE_FLOOR: float = 0.3  # puremagic strong-signature-match floor: header/footer/deep-scan matches clear it, extension-only guesses fall below
_CHARSET_HEAD: int = (
    4096  # bounded leading-byte budget for the in-process charset sniff (text_scanner.decode_any), matching the libmagic mime_encoding read
)

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
class DetectPolicy:  # the libmagic-arm behavior value; the puremagic default reads only `deepscan` off the owner
    flags: frozenset[DetectFlag] = field(default_factory=frozenset)
    params: frozendict[MagicParam, int] = field(default_factory=frozendict)
    no_check: frozenset[CheckClass] = field(default_factory=frozenset)
    magic_db: Path | None = None


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
    confidence: float = 0.0
    engine: DetectEngine = DetectEngine.PUREMAGIC
    byte_length: int = 0
    libmagic_version: int = 0


class DetectSettings(BaseSettings):  # admitted once at the composition root; the deployment env → configured Detect
    model_config = SettingsConfigDict(env_prefix="RASM_DETECT_", frozen=True, extra="forbid")
    engine: DetectEngine = DetectEngine.LAYERED
    deepscan: bool = True
    magic_db: Path | None = None  # the discovery-env → configured-path → bundled-fallback libmagic .mgc database

    def detector(self) -> "Detect":
        return Detect(engine=self.engine, deepscan=self.deepscan, policy=DetectPolicy(magic_db=self.magic_db))


@dataclass(frozen=True, slots=True, kw_only=True)
class Detect:
    engine: DetectEngine = DetectEngine.LAYERED
    profile: DetectProfile = DetectProfile.IDENTITY
    policy: DetectPolicy = field(default_factory=DetectPolicy)
    disposition: Disposition = Disposition.ABORT
    deepscan: bool = True

    @overload
    async def of(self, source: Source, /) -> RuntimeRail[DetectIdentity]: ...
    @overload
    async def of(
        self, source: Iterable[Source], /
    ) -> RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]: ...
    async def of(
        self, source: Source | Iterable[Source], /
    ) -> RuntimeRail[DetectIdentity] | RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]:
        match source:
            case Source() as one:
                return await self._railed(one)
            case sources:
                return await self._many(Block.of_seq(sources))

    async def _railed(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        match self.engine:
            case DetectEngine.PUREMAGIC:
                return await self._pure(source)
            case DetectEngine.LIBMAGIC:
                return await self._libmagic(source)
            case DetectEngine.LAYERED:
                return await self._layered(source)
            case _ as unreachable:
                assert_never(unreachable)

    async def _pure(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        return await async_boundary(
            f"detect.pure.{self.profile}", lambda: to_thread.run_sync(_pure_detect, source, self.deepscan, limiter=_SNIFF_GATE)
        )

    async def _libmagic(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        return await async_boundary(
            f"detect.libmagic.{self.profile}",
            lambda: _WORKER_RETRY(to_process.run_sync, _gated_detect, source, self.profile, self.policy, limiter=WORKER_BAND),
        )

    async def _layered(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        # escalate ONLY a resolved-but-UNKNOWN puremagic verdict to the broad libmagic database; a confident
        # hit or a puremagic fault returns unescalated, and a libmagic provisioning fault keeps the puremagic UNKNOWN
        primary = await self._pure(source)
        resolved = primary.to_option()
        if resolved.is_none() or resolved.value.media_class is not MediaClass.UNKNOWN:
            return primary
        return (await self._libmagic(source)).or_else_with(lambda _fault: primary)

    async def _many(
        self, sources: Block[Source], /
    ) -> RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]:
        async with anyio.create_task_group() as group:
            handles: Block[TaskHandle[RuntimeRail[DetectIdentity]]] = sources.map(lambda one: group.start_soon(self._railed, one))
        return traversed(handles.map(lambda handle: handle.return_value), by=self.disposition)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _classified[E](mime: str, table: frozendict[str, E], default: E, /) -> E:
    # exact `.get` first, then the longest registered prefix the mime extends — a `/` top-level type
    # (`image/`) or a dotted compound subtype (`...wordprocessingml` catching the `.document`/`.sheet`
    # suffix the deep-scan or modern libmagic appends) — so the most specific row wins and no shorter prefix shadows it.
    direct = table.get(mime)
    if direct is not None:
        return direct
    prefix = max((key for key in table if mime.startswith(key)), key=len, default="")
    return table[prefix] if prefix else default


def _trust(
    mime: str,
    media_class: MediaClass,
    container: Container,
    extensions: tuple[str, ...],
    matches: tuple[str, ...],
    claimed: str,
    confidence: float,
    /,
) -> Trust:
    declared = MediaClass.of(claimed) if claimed else MediaClass.UNKNOWN
    claimed_container = Container.of(claimed) if claimed else Container.NONE
    claimed_exts = frozenset(
        suffix.lstrip(".") for suffix in mimetypes.guess_all_extensions(claimed)
    )  # the claim's valid extensions, dot-stripped to the sniffed form
    return (
        Trust.UNKNOWN
        if media_class is MediaClass.UNKNOWN or mime in ("", "application/octet-stream") or confidence < _CONFIDENCE_FLOOR
        else Trust.AMBIGUOUS
        if len(matches) > 1  # two distinct strong matches — a polyglot
        else Trust.IDENTIFIED
        if declared is MediaClass.ARCHIVE
        and claimed_container
        is container
        is not Container.NONE  # a generic-container claim naming the sniffed container is a generalization, not a spoof
        else Trust.MISMATCH
        if declared is not MediaClass.UNKNOWN and declared is not media_class  # cross-class spoof
        else Trust.MISMATCH
        if declared is media_class and extensions and claimed_exts and claimed_exts.isdisjoint(extensions)  # same-class extension/label spoof
        else Trust.IDENTIFIED
    )


def _claim_mime(path: Path, /) -> str:
    # the File claim MIME over puremagic's richer ext<->MIME table: `ext_from_filename` recovers the on-disk
    # extension (a registered compound extension the stdlib split drops), `from_extension` maps it through
    # puremagic's reverse-lookup rows (raising `PureError` on an unregistered extension, trapped to the stdlib
    # `guess_file_type` fallback), so the File `claimed` content-type — hence the `_trust` `declared` class and
    # the cross-class spoof — reads the deeper table rather than the thinner stdlib mimetypes parse alone.
    ext = ext_from_filename(path)
    stdlib = mimetypes.guess_file_type(path)[0] or ""
    return catch(exception=PureError)(from_extension)(ext).default_value(stdlib) if ext else stdlib


def _charset(source: Source, media_class: MediaClass, /) -> str:
    # the in-process charset facet symmetric with the libmagic `mime_encoding` cook the pure default arm
    # formerly left EMPTY: `text_scanner.decode_any` tries utf-8/cp1252/... over a bounded head and returns
    # `(text, encoding)`, non-raising through the `TypeError`-trap (`decode_any` raises on an undecodable binary
    # head, narrowed to ""), gated to the text-family class so a binary payload carries "" exactly as libmagic
    # returns `binary` — so both arms of the one owner converge on a populated `DetectIdentity.encoding`.
    if media_class not in (MediaClass.TEXT, MediaClass.DATA):
        return ""
    match source:
        case Source(tag="buffer", buffer=(payload, _)):
            head = payload[:_CHARSET_HEAD]
        case Source(tag="file", file=(path, _)):
            with path.open("rb") as handle:  # Exemption: bounded head read for the charset sniff, worker-side blocking I/O
                head = handle.read(_CHARSET_HEAD)
        case _ as unreachable:
            assert_never(unreachable)
    return catch(exception=TypeError)(text_scanner.decode_any)(head).map(lambda decoded: decoded[1]).default_value("") if head else ""


def _pure_roster(source: Source, deepscan: bool, /) -> Block[PureMagicWithConfidence]:
    # the confidence-ranked puremagic roster; a File deep-scans natively, a Buffer spills to a bounded temp
    # file so its ZIP/CFBF central directory resolves the exact OOXML/legacy-Office subtype at confidence 1.0;
    # deepscan off drops to the no-I/O `identify_all` table match over a bounded head+foot for untrusted ingest.
    # A PureError (no match) or PureValueError (empty input) is the gate's honest UNKNOWN, an empty roster.
    try:
        match source:
            case Source(tag="buffer", buffer=(payload, _)) if deepscan:
                with NamedTemporaryFile(
                    delete_on_close=False
                ) as spill:  # Exemption: puremagic deep-scan reads a real path for the ZIP/CFBF central directory
                    spill.write(payload)
                    spill.flush()
                    return Block.of_seq(magic_file(spill.name))
            case Source(tag="buffer", buffer=(payload, _)):
                head, foot = string_details(payload)
                return Block.of_seq(identify_all(head, foot))
            case Source(tag="file", file=(path, _)) if deepscan:
                return Block.of_seq(magic_file(path))
            case Source(tag="file", file=(path, _)):
                head, foot = file_details(path)
                return Block.of_seq(identify_all(head, foot))
            case _ as unreachable:
                assert_never(unreachable)
    except PureError, PureValueError:
        return Block.empty()


def _pure_detect(source: Source, deepscan: bool, /) -> DetectIdentity:
    roster = _pure_roster(source, deepscan)
    strong = roster.filter(lambda match: match.confidence >= _CONFIDENCE_FLOOR and bool(match.mime_type))
    top = roster.try_head()
    mime = top.map(lambda match: match.mime_type).default_value("")
    media_class, container = MediaClass.of(mime), Container.of(mime)
    matches = tuple(dict.fromkeys(match.mime_type for match in strong))  # distinct strong MIMEs — a polyglot when > 1
    extensions = tuple(dict.fromkeys(ext for match in roster if (ext := match.extension.lstrip("."))))
    confidence = top.map(lambda match: match.confidence).default_value(0.0)
    claimed = source.claimed
    return DetectIdentity(
        mime=mime,
        description=top.map(lambda match: match.name).default_value(""),
        encoding=_charset(source, media_class),
        extensions=extensions,
        media_class=media_class,
        container=container,
        matches=matches,
        claimed=claimed,
        trust=_trust(mime, media_class, container, extensions, matches, claimed, confidence),
        confidence=confidence,
        engine=DetectEngine.PUREMAGIC,
        byte_length=source.length,
    )


@cache
def _cookie(
    magic_db: Path | None,
    facet_flag: str | None,
    flagged: frozendict[str, bool],
    params: frozendict[MagicParam, int],
    no_check: frozenset[CheckClass],
    /,
) -> "magic.Magic":
    cookie = magic.Magic(magic_file=str(magic_db) if magic_db is not None else None, **({facet_flag: True} if facet_flag else {}), **flagged)
    for param, value in params.items():
        cookie.setparam(
            getattr(magic, param.value), value
        )  # tuned once per cooked config; the cookie (and its loaded database) is cached across detections in the worker
    if no_check:  # the MAGIC_NO_CHECK_* test-class narrowing is raw-bit-only, so the disabled classes OR into the applied flags through the C `magic_setflags` binding
        magic.magic_setflags(cookie.cookie, cookie.flags | reduce(or_, (getattr(magic, klass.value) for klass in no_check), 0))
    return cookie


def _cooked(source: Source, facet: MagicFacet, policy: DetectPolicy, flagged: frozendict[str, bool], /) -> str:
    cookie = _cookie(policy.magic_db, _FACET_FLAG.get(facet), flagged, policy.params, policy.no_check)
    match source:
        case Source(tag="buffer", buffer=(payload, _)):
            return cookie.from_buffer(payload)
        case Source(tag="file", file=(path, _)):
            return cookie.from_file(path)
        case _ as unreachable:
            assert_never(unreachable)


def _gated_detect(source: Source, profile: DetectProfile, policy: DetectPolicy, /) -> DetectIdentity:
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
    media_class, container = MediaClass.of(mime), Container.of(mime)
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
        trust=_trust(
            mime, media_class, container, extensions, matches, claimed, 1.0
        ),  # libmagic carries no confidence; it clears the floor and trusts its single match
        confidence=1.0,
        engine=DetectEngine.LIBMAGIC,
        byte_length=source.length,
        libmagic_version=version,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------
# the pure-Python puremagic default rides this local thread band (mirroring the exchange/conformance and
# exchange/credential sibling `_THREAD_GATE`): N concurrent in-process sniffs share a fixed thread band off
# the event loop, sharing the address space so a Buffer payload never pickles. The libmagic arm's process
# band is the execution/lanes#LANE-owned shared `WORKER_BAND`, never this thread band.
_SNIFF_GATE: Final[CapacityLimiter] = CapacityLimiter(8)
```

## [03]-[RESEARCH]

- [DUAL_ENGINE] [RESOLVED]: the `DetectEngine` `PUREMAGIC`/`LIBMAGIC`/`LAYERED` axis composes the two sniffers under ONE `Detect` owner rather than two parallel detector pages, mirroring the media capability-detection contract's one-filter-node-two-arms shape. `puremagic 2.2.0` (pure-Python `py3-none-any`, `Requires-Python >=3.12`, resolves on cp315 directly, no native extension) verifies as the DEFAULT: `magic_file(path)`/`magic_string(string, filename=)`/`magic_stream(stream, filename=)` return the confidence-ranked `list[PureMagicWithConfidence]` roster (top-first), `magic_file` runs `raise_on_none=False` (an unknown returns `[]`, never raises), and the `puremagic.main` head/foot primitives `file_details(path)`/`string_details(string)`/`identify_all(header, footer, ext=None)` back the no-I/O table pass. `PureMagicWithConfidence` carries `byte_match`/`offset`/`extension`/`mime_type`/`name`/`confidence` (`confidence` ∈ `[0.0, 1.0]`, `1.0` a deep-scan exact resolution); `PureError` (a `LookupError`, top-level `__all__`) and `PureValueError` (a `ValueError`, `puremagic.main` only) are the two faults `_pure_roster` traps to an empty roster. Verified against the folder `puremagic` `.api`: `single_deep_scan` (the `zip_scanner` reading `[Content_Types].xml` → exact OOXML `.docx`/`.xlsx`/`.pptx`, the `cfbf_scanner` reading the CFB stream+CLSID → exact legacy `.doc`/`.xls`/`.ppt`) is the strict-stronger axis over libmagic, which floors a `.docx` to `application/zip` and a `.doc` to `application/CDFV2`; the `magic_data.json` table (1485 header + 8 footer + 79 extension-only + 83 multi-part rows) needs no `.mgc` provisioning. `python-magic 0.4.27` is RETAINED behind it on the one distinct strict-stronger axis the brief admits — its broad compiled magic database recognizes the long tail of niche leaf signatures `magic_data.json` lacks — never the same sniff.
- [LOADER_PATH_OFFLOAD] [RESOLVED]: `puremagic` rides the runtime loader path IN-PROCESS, so `_pure` runs `_pure_detect` under `anyio.to_thread.run_sync(limiter=_SNIFF_GATE)` — NOT the `to_process` `WORKER_BAND` crossing the libmagic arm needs. The offload arm is `to_thread` (not the event loop, honoring `concurrency#OFFLOAD_LANE` that no synchronous body runs inline, and not `to_interpreter` which would pickle the `Buffer` payload in-process): puremagic's `magic_file`/`file_details` head+foot read is a bounded blocking syscall and the deep-scan opens the ZIP/CFBF central directory (more blocking I/O), the profile `to_thread` owns, and the shared address space passes the `Buffer` payload copy-free where a process/interpreter arm would pickle it. The bounded `get_max_lengths()`-style head+foot read keeps even a multi-gigabyte path off the heap. `_SNIFF_GATE` is a local `CapacityLimiter(8)` mirroring the `exchange/conformance`/`exchange/credential` sibling `_THREAD_GATE`, distinct from the shared `WORKER_BAND` process band the libmagic arm crosses. Because the puremagic path is on-loader-path, the `to_process` crossing, the `WORKER_BAND` limiter, the `BrokenWorkerProcess` retry, and the process-pickle seam ALL drop for the default; `async_boundary` still wraps it so the in-process sniff inherits the `structlog` event + span and the `RuntimeRail` envelope, and `PureError`/`PureValueError` lift to the `CLASSIFY` case.
- [LIBMAGIC_WORKER] [RESOLVED]: the retained libmagic arm verifies against `python-magic 0.4.27` and the folder `python-magic` `.api` — `Magic(mime=, magic_file=, mime_encoding=, keep_going=, uncompress=, raw=, extension=)`, `Magic.from_buffer`/`from_file`, `Magic.setparam`/`getparam`, `version()` (raising `NotImplementedError` on an ancient lib, so the `EXTENSION` facet gates on `>= 524`), `MagicException` (`.message`), and the `_handle509Bug` null-result quirk returning `application/octet-stream`. libmagic is a Forge-provisioned native `ctypes` binding OFF the loader path (`import magic` raises `ImportError('failed to find libmagic')` when `magic.loader.load_lib` finds no candidate), so `_libmagic` crosses `anyio.to_process.run_sync(limiter=WORKER_BAND)` with the `_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)` transient-death recovery, onto a worker that reifies `lazy import magic` at boundary scope. The `File` source is read worker-side so the parent never pickles the payload; `Magic.from_descriptor` is excluded (a parent-process fd does not cross the process seam). The `_FACET_FLAG` one-flag-per-cook law holds — `Magic(mime=True, mime_encoding=True)` returns the combined `text/plain; charset=utf-8` form, so the worker holds one `functools.cache`-memoised cookie per facet, the compiled database loading once per config per worker.
- [CHECK_SET_NARROWING] [RESOLVED]: the `CheckClass` vocabulary (`COMPRESS`/`TAR`/`SOFT`/`APPTYPE`/`ELF`/`ASCII`/`TROFF`/`FORTRAN`/`TOKENS`) folds the libmagic `MAGIC_NO_CHECK_*` test-class narrowing the `MagicParam` recursion/ELF caps' hardening peer — verified as module constants (`magic.MAGIC_NO_CHECK_COMPRESS = 0x001000` … `MAGIC_NO_CHECK_TOKENS = 0x100000`). These are raw-bit-only (no `Magic` constructor boolean), so `_cookie` ORs the disabled-class bits into the applied flags through `magic.magic_setflags(cookie.cookie, cookie.flags | bits)` (the module-level `libmagic.magic_setflags` ctypes binding, `restype=c_int`, `argtypes=[magic_t, c_int]`; `Magic.__init__` seeds its flags at construction via `magic_open(self.flags)`, so `magic_setflags` re-applies the widened set on the live `cookie.cookie` post-load) after `setparam` tunes the caps — so an untrusted or latency-bounded ingest disables a whole libmagic test class, `reduce(or_, ...)` folding the populated bits over the `no_check` frozenset that is part of the `_cookie` cache key.
- [PURE_TRUST_CONFIDENCE] [RESOLVED]: the puremagic float `confidence` is the native ambiguity signal `_trust` gates on — a top-match confidence below `_CONFIDENCE_FLOOR` (`0.3`, separating header/footer/deep-scan matches from extension-only guesses) collapses to `Trust.UNKNOWN`, and two distinct strong MIMEs (the `matches` set built from the `>= floor` roster tail) to `Trust.AMBIGUOUS` — the polyglot signal libmagic can only approximate through `keep_going`'s multi-match. The libmagic arm passes `confidence=1.0` (no confidence, trusts its single match). The `_trust` fold gains the `confidence` parameter beside the existing `media_class`/`container`/`extensions`/`matches`/`claimed` axes, so both engines converge on one verdict shape; the container-generalization `IDENTIFIED` branch is RETAINED (the libmagic-floored `application/zip`/`application/CDFV2` path still hits it) but fires less often now that the puremagic deep-scan resolves the exact subtype so a `.docx` sniffs `WORD`+`ZIP` directly rather than the generic `ARCHIVE` floor.
- [BEARTYPE_INGRESS] [RESOLVED]: the `Source.Buffer`/`Source.File` factories carry the shared `@beartype(conf=FAULT_CONF)` contract (verified against the folder `beartype` `.api` `violation_type`-redirect law, mirroring the `exchange/conformance`/`exchange/credential` signer projectors) so a malformed `bytes`/`Path` ingress lifts onto the runtime fault rail at admission rather than a native `TypeError` deep in the sniff or a `ctypes` fault across the worker seam — the `str`/`bytes`/`PathLike` ingress guard both the `puremagic` and `python-magic` catalogs name, which the single-engine page left unguarded.
- [DETECT_SETTINGS] [RESOLVED]: the `DetectSettings` `pydantic-settings` `BaseSettings` (verified against the shared `pydantic-settings` `.api` — `BaseSettings`, `SettingsConfigDict(env_prefix=, frozen=, extra=)`, `SettingsError`/`ValidationError` lifting at the composition root) owns the deployment env-admission the single-engine page hardcoded or omitted: the `RASM_DETECT_` prefix over `engine` (the default `DetectEngine`), `deepscan` (the `PUREMAGIC_DEEPSCAN`-equivalent toggle gating the container deep-scan for untrusted/latency-bounded ingest), and `magic_db` (the discovery-env → configured-path → bundled-fallback libmagic `.mgc` database path the brief `[03]` system-tool resolution names). Its `detector()` projects one configured `Detect` so the env is admitted once through the `BaseSettings` source chain and never a raw `os.environ` read scattered through the interior; the settings model is admitted at the composition root, never a module-level singleton reading env at import.
- [LAYERED_ESCALATION] [RESOLVED]: the `LAYERED` default runs `_pure` and escalates ONLY a resolved `MediaClass.UNKNOWN` verdict to `_libmagic` — a confident puremagic hit never pays the `to_process` crossing, and a puremagic FAULT (the rail `Error`, `to_option().is_none()`) returns unescalated rather than re-running the broad database on a genuine error. The libmagic escalation's own rail is recovered with `or_else_with(lambda _fault: primary)`, so a libmagic PROVISIONING fault (the worker `ImportError` async_boundary rails) keeps the honest puremagic `UNKNOWN` rather than surfacing the worker fault — the capability-detection philosophy of brief `[05]` applied to format-ID: probe the in-process default, fall to the broad native database only on a miss, never let its absence override a valid verdict. The `MediaClass`/`Container`/`Trust` closed vocabularies, the `_classified` exact-then-longest-prefix fold, `mimetypes.guess_file_type`/`guess_all_extensions`, and `TaskHandle.return_value` (the `concurrency.md` `TASK_GROUP` `CHILD_CARRIER` settled-child read) verify against the stdlib and the folder catalogs.
- [PURE_ENCODING] [RESOLVED]: `text_scanner.decode_any(unicode: bytes) -> tuple[str, str]` (verified at `puremagic.scanners.text_scanner`, returning `(decoded_text, encoding)` over a utf-8→cp1252→… charset ladder) closes the asymmetry where only the libmagic `_gated_detect` arm set `DetectIdentity.encoding` (via the `mime_encoding` facet cook) and the `puremagic` default arm left it EMPTY. `_pure_detect` folds `_charset(source, media_class)` over a bounded `_CHARSET_HEAD` (4096-byte) head — the `Buffer` payload slice or a worker-side bounded `File` read — gated to the `MediaClass.TEXT`/`DATA` text-family class so a binary payload carries "" exactly as libmagic returns `binary`. `decode_any` RAISES `TypeError("No encoding found")` on an undecodable head (verified against the installed `2.2.0`), so the fold rides `catch(exception=TypeError)(text_scanner.decode_any)(head).map(lambda decoded: decoded[1]).default_value("")` — non-raising, never faulting the sniff. `eml_check(head) -> Match | None` (the RFC-822 `.eml` recovery on the same module) stays deferred: the deep-scan `single_deep_scan` chain already routes EML, so a second call duplicates an owned resolution.
- [PURE_CLAIM_EXTENSION] [RESOLVED]: `ext_from_filename(filename) -> str` and `from_extension(extension, mime=True) -> str` verify against the top-level `puremagic` surface. The `Source.claimed` File arm and the `_trust` `declared`/spoof fold derived the File claim MIME through the stdlib `mimetypes.guess_file_type`, a thinner table than puremagic's own ext↔MIME rows the owner already loads. `_claim_mime(path)` now runs `ext_from_filename` (recovering a registered compound extension the stdlib split drops — `report.docx` → `.docx` → `from_extension` → the full `…wordprocessingml.document` MIME the `_classified` longest-prefix fold routes to `WORD`, verified) then `from_extension` (the reverse-lookup row, `PureError`-trapped to the stdlib fallback for an extension puremagic's table lacks — verified `from_extension('.zzz')` raises `PureError`), so the `File` claim — hence the `_trust` `declared` class and the cross-class spoof — reads the deeper table. `magic_extension(extension) -> list[PureMagicWithConfidence]` (the full ranked reverse-lookup roster) stays deferred behind the single-best `from_extension` for the claim MIME; the same-class `mimetypes.guess_all_extensions(claimed)` spoof set is retained (puremagic exposes no MIME→extensions reverse), now fed the puremagic-sharpened claim.
