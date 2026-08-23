# [PY_RUNTIME_IDENTITY]

`ContentIdentity` is the single content-addressing owner the whole branch consumes, the module `rasm.runtime.identity`: one XxHash128 key over canonical bytes under a two-arm seed. The explicit `Some(0)` arm is the seed-zero cross-branch parity path every peer reproduces; `CanonicalWriter` is its exact multi-field crossing codec, matching the kernel writer's fixed-width and framed-text byte grammar. The default arm derives its seed from the format and the consumer-folded policy bytes for branch-local identities. Data, geometry, compute, and artifacts consume this one owner and mint no parallel content key.

One span-fold core serves every entry — `_derive_span` is the sole `content.derive` bracket and `_closed` the sole key/status close. `_minted` folds an `IdentitySource` into that close, while `CanonicalWriter` streams its field digest into the same close. The tracer mints from the `reliability/faults#FAULT` `SCOPES[Scope.IDENTITY]` row and the railed entry fences through `boundary(IDENTITY_DERIVE, ...)` inside the live span. Corpus-parity binding lives in the sibling `evidence/reproduction` module, split out so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`).

## [01]-[INDEX]

- [02]-[IDENTITY]: the `ContentKey` value object with distinct canonical-memory and generated-wire byte projections, `CanonicalWriter` as the seed-zero cross-runtime field stream, the `KEY_FMT` grammar both entries gate on, the `Tolerance`-folded seed, the declared `BareValue` payload family over the closed `IdentitySource` modality ADT, the span-fold core, and the railed `of` beside the bare `key`.

## [02]-[IDENTITY]

- Owner: `IdentityPolicy.spec` IS the canonical-seed field contract — every field it renders enters the seed bytes, which `_framed` length-and-count frames exactly as it frames a `parts` key preimage, so one width and one byte order serve both — and the policy is a GENERIC carrier: a domain knob such as geometry's tessellation deflection/angle rides a consumer-owned policy folded into the canonical seed bytes, never a new `IdentityPolicy` field per domain. Key equality is bytes-law — `of(fmt, source)` under the default and under an explicit `CANONICAL_POLICY` mint the same key, the compute design-key resume cache the demanding proof. `IdentitySource` owns its own `lift` and `fold`, so dispatch is total and the digest algebra rides the union, never an external dispatcher or a second entrypoint.
- Entry: `KEY_FMT` admits the `fmt` before any derivation runs, because `fmt` enters the default seed AND renders as the `hex` tail the C# peer joins on, so an unlawful spelling forks the key namespace at both ends and every later comparison answers "changed" for a cause the key cannot carry. One grammar, two seams keyed by each entry's own fallibility: `of` refuses on the rail before the span opens, while `key` — which returns a bare value and has no rail — carries the refined `KeyFmt` hint under `FAULT_CONF`, whose canonical violation the `CLASSIFY` `api` row folds at the enclosing fence. A per-call-site spelling check and a second pattern are the two deleted forms. `of` is the one polymorphic derivation over input shape and output projection — no per-render method and no parallel `of_canonical`; `key` is the bare synchronous accessor beside it, the one fallibility split, never a `rail: bool` knob. An empty tuple lands on `stream`, whose seed-only fold is a deterministic degenerate key; a MIXED tuple lands on no arm at all and refuses at `lift`. A multi-FIELD preimage names its modality instead of inferring it — an already-lifted `IdentitySource(parts=...)` rides `of` verbatim, so the length-and-count framing the estate's preimage law demands runs at this owner and no producer spells a `to_bytes` width of its own; `key`'s signature is the `BareValue` roster `Source` widens by exactly the fallible pair, so the two entries state one admission set and its one delta rather than two hand-kept unions: the split is encode-fallibility — a `Struct` must encode and an encode can raise, which the bare accessor has no rail to carry, while a lifted `parts`/`stream` source is already bytes and its length-and-count framing cannot fail, so the lifted case rides `key` verbatim and `of` remains the rail-carrying entry for fallible admissions. The refinement is what makes that split structural: a bare `IdentitySource` annotation admits its own `canonical` case, so the exclusion the prose states would be one the type walks straight past. `seed` is the `Option[U64]` override: `Nothing` the policy-folded settings seed, `Some(0)` the bare C# `XxHash128.HashToUInt128(span)` seed-zero path the `GeometryHash`/`NamingHashOps` boundary mints — geometry `mesh/cad` keys GLB bytes under this seed-zero `RepresentationContentHash` parity contract, while `mesh/daemon` uses `CanonicalWriter` for the multi-field tessellation contract — so the seed origin is one parameter, never a fake policy. Identity is recovered from the value shape, never a path, name suffix, or mode flag.
- Law: the admitted payload family is DECLARED at `BareValue`, never inferred from arm order — one union arm per `IdentitySource` case and a `lift` total over exactly that roster, so a composing package reads its modality off the annotation and an unadmitted payload refuses at the lift rather than at the hash. `Buffer` keys as `whole`, a bytes iterable as `stream` chunks of ONE payload, a homogeneous `tuple[ContentKey, ...]` as the `merkle` spine, and a lifted `BareSource` as itself; `Source` adds the one fallible admission — a `Struct` as `canonical`. Nothing else keys. N SEMANTIC fields have no bare spelling BY DESIGN: the producer hands `IdentitySource(parts=...)`, so the modality is a value the caller states rather than a shape `lift` guesses and the `[PREIMAGE_FRAMING]` count-and-length frame runs at its one owner. No arm therefore admits a MIXED tuple of keys and buffers — a key is not bytes and a chunk is not a semantic field — so a producer holding both lowers each key through `ContentKey.memory`, the same little-endian u128 spelling the `merkle` spine reads, then hands ONE `parts` tuple; a `canonical` `Struct` preimage carrying a live `ContentKey` is refused a layer down, since msgspec raises `OverflowError` on any int past `2**64-1` before `enc_hook` ever fires and that raise is outside the `EncodeError` the rail catches.
- Auto: the `merkle` child transcription reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold before `XxHash128.HashToUInt128`, so a parent key is order-sensitive over its parts. `CanonicalWriter.u128` uses that same little-endian hash-input projection, while `ContentKey.wire_bytes` is the distinct sixteen-byte big-endian generated-`bytes` projection `ContentHash.Wire`/`Admit` owns; substituting either for the other byte-swaps every crossing key. `lift`'s payload modalities are exported branch law — data keys operation bytes and derived-snapshot Merkle keys, compute keys buffer/stream payloads for its resume cache, geometry keys GLB bytes — so narrowing any modality is a cross-folder break. `project("hex")` renders `{value:032x}:{fmt}`, the digest-colon-tag spelling a C# artifact address carries, so a companion GLB result minted on the `Some(0)` arm keys byte-identically to the kernel seed-zero `RepresentationContentHash` — a default-arm key shares that render and never that value, since its seed preimage is this branch's own; `project("wire")` renders the bare 32-lowercase-hex form every wire digest and manifest key field carries — the python peer of `ContentAddress.ToValue()`, the ONE lowering site the key-spelling carve demands.
- Law: `byte_length` is the MEASURED extent and bears absence: every mint states one because every fold measured its own preimage, and `ContentKey.decoded(value=, fmt=)` is the ONE arm answering `Nothing` — the refusal door for a key rebuilt from a wire render, which carries digest and tag alone. `Option` is totally ordered, so `order=True` stays total for the `Map` trees keyed on this value, and the `merkle` extent binds through the carrier so one unmeasured child leaves its whole parent unmeasured rather than lighter than its contents.
- Growth: a new evaluation parameter is one `Tolerance` field on `IdentityPolicy.spec`; a new output render one `KeyView` member with one `project` arm and one `of` overload, because a render with no overload matches none and type-checks nowhere while resolving fine at runtime; a new input modality one `IdentitySource` case with one `fold` arm, plus one `BareValue` arm and its `lift` shape only where a bare value discriminates it — a modality whose bare form no arm can tell apart grows as a `parts`-style lifted case with no union edit at all; a distinct seed origin one `Some(value)` through the existing override; a new span attribute one line in the span-fold core reaching both entries; a widened `fmt` vocabulary is one `KEY_FMT` edit reaching the rail gate, the refined hint, and the corpus census at once.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law — and the C# `ContentHash`/`CanonicalWriter` capsule is the cross-boundary mechanics owner this seed reproduces. Consumers ride the unbroken `of`/`key`/`ContentKey`/`ContentKey.decoded`/`IdentitySource`/`CanonicalWriter` surface. `IdentitySource(parts=...)` remains the branch-local semantic-field frame; a peer contract uses `CanonicalWriter` because its int32 little-endian ordinals, little-endian u128 hash inputs, exact doubles, framed UTF-8 strings, and raw terminal payload are the published cross-runtime grammar. Its `key` close pins seed zero and shares `_closed` with `ContentIdentity`, so key construction, measurement, and evidence have one implementation. This owner mints the branch's `CANONICAL_BYTE_IDENTITY` instance under the `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` and `[CONTENT_KEY]` laws; parity across the independent runtimes IS the conformance, and WHICH fields a producer offers stays the producer's while their widths and byte order never do. The default arm's derived seed governs branch-local identities alone and states no peer parity.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Buffer, Callable, Iterable, Iterator
from contextlib import contextmanager
from math import isnan
from struct import pack
from typing import Annotated, Final, Literal, Self, assert_never, overload

import xxhash
from beartype import beartype
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import EncodeError, Meta, Struct
from msgspec.msgpack import Encoder
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import FAULT_CONF, IDENTITY_DERIVE, IDENTITY_FMT, SCOPES, RuntimeRail, Scope, boundary, scoped

# --- [TYPES] ----------------------------------------------------------------------------

# msgspec rejects any integer bound past int64 at codec/convert build, so only the `ge=0` floor rides `Meta`; the ceilings are the
# digest algebra's — `xxh3_128_intdigest` yields <2**128 by construction, seeds are the 64-bit xxhash domain.
type U128 = Annotated[int, Meta(ge=0)]
type U64 = Annotated[int, Meta(ge=0)]
type Tolerance = Annotated[float, Meta(gt=0.0)]
type KeyView = Literal["value", "hex", "wire", "memory", "digest"]
type KeyRender = ContentKey | str | bytes | int
# THE ADMITTED PAYLOAD FAMILY, DECLARED — one union arm per `IdentitySource` case, so a composing package reads its
# modality off this annotation instead of recovering it from `lift`'s arm order, and `lift` is total over exactly
# this roster with nothing falling through:
#   `Buffer`                 -> `whole`     one contiguous payload, `bytes(...)`-coerced once at the arm
#   `Iterable[bytes]`        -> `stream`    BUFFER CHUNKS of one payload; the chunk boundary is an I/O artefact
#   `tuple[ContentKey, ...]` -> `merkle`    N child keys over the little-endian u128 C#-parity spine
#   `BareSource`             -> itself      the ALREADY-LIFTED source, and the ONE spelling of N SEMANTIC parts
# an ALREADY-LIFTED source rides the same entry, which is what keeps the two multi-part modalities separable: a bare
# iterable is buffer chunks of one payload and lifts to `stream`, while N SEMANTIC parts are a value the caller
# constructs and hands in as `IdentitySource(parts=...)`. The discriminant stays recoverable from the value, and a
# payload matching no arm — a MIXED tuple of keys and buffers above all — refuses at `lift` rather than keying.
# The two entries state ONE roster and its one delta: `BareValue` is every admission already in bytes, and `Source`
# widens it by exactly the fallible pair — a `Struct` that must encode, and the `canonical` case `BareSource` refines
# away — so a new modality lands on the primary and the railed roster re-derives with no second union to re-spell.
type BareValue = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | BareSource
type Source = BareValue | Struct | IdentitySource
# the same grammar the railed and the contracted entry both read, refined at the hint the bare accessor carries: `key`
# returns a `ContentKey` with no rail to refuse on, so its gate is the contract weave whose violation `FAULT_CONF`
# raises as the canonical `BeartypeCallHintViolation` an enclosing fence classifies `api`; `of` gates on the rail.
type KeyFmt = Annotated[str, Is[lambda text: KEY_FMT.fullmatch(text) is not None]]
# the lifted sources the BARE accessor admits, refined exactly as `KeyFmt` is: `whole`, `stream`, `parts`, and
# `merkle` all fold over bytes already in hand, so their derivation cannot raise, while `canonical` must ENCODE and
# an encode raises with no rail here to carry it. The bare `IdentitySource` annotation admits every case, so the
# encode-fallibility split would be a comment the type walks past — this refinement makes it structural, and the
# violation lands as the same `BeartypeCallHintViolation` the `CLASSIFY` `api` row folds at the enclosing fence.
type BareSource = Annotated[IdentitySource, Is[lambda lifted: lifted.tag != "canonical"]]

# --- [CONSTANTS] --------------------------------------------------------------------------

# `fmt` is load-bearing identity INPUT — it enters the seed beside the policy spec — and it is also the rendered tail
# of the `hex` view the C# `InterchangeIdentity.Key` peer joins on, so a misspelling forks the key namespace at both
# ends at once and every downstream comparison then answers "changed" for a reason no reader can recover from the
# key. One compiled grammar closes that: dot-separated lowercase segments over the same `[a-z0-9_-]` character class
# the hook registry's `HOOK_ID` admits for a point id, gated once per derivation before any digest exists rather than
# trusted per call site. The class is what makes the grammar TOTAL over the standing namespace — the estate's frozen
# tags spell hyphens (`geometry-topology` is the C#-parity fixture's own, `texture-plane`/`texture-set` the frozen
# plane and set namespaces) — while it still refuses every real hazard: an uppercase drift keying a second namespace,
# whitespace, an empty or dangling segment, and above all a `:`, which would fork the `hex` render's own separator
# and leave the C# join reading a truncated digest against a tail that is not the tag.
KEY_FMT: Final[re.Pattern[str]] = re.compile(r"^[a-z0-9_-]+(\.[a-z0-9_-]+)*$")

# --- [MODELS] ---------------------------------------------------------------------------


class ContentKey(Struct, frozen=True, order=True, gc=False):
    # `order=True` is load-bearing: `expression.Map` is an ordered tree, so every `Map[ContentKey, ...]`
    # (lane drain cache, plan tables, warm seeds) needs the field-order `<` this generates.
    value: U128
    fmt: str
    # the measured extent of the preimage, ABSENCE-BEARING and required: every mint states one and only `decoded`
    # answers `Nothing`, so no constructor can quietly default. `Option` is TOTALLY ordered here — `Nothing` sorts
    # below every `Some` — so `order=True` stays total and a `Map[ContentKey, ...]` tree keeps its comparison even
    # where a decoded key and its measured twin meet.
    byte_length: Option[int]

    @staticmethod
    def decoded(*, value: U128, fmt: str) -> "ContentKey":
        # the REFUSAL door for a key rebuilt from a WIRE render. The estate publishes a content key as digest and tag
        # alone, so a decode holds no extent and this arm is what stops one being invented: the retired
        # `byte_length=0` filled a required measurement slot with a number no producer took — `docs/laws/scars.md`
        # `[FORGED_ZERO]`, "converting a canon-only value mints a key or stamp no producer took" — and a consumer
        # summing extents across a mixed roster then read the crossing as WEIGHTLESS rather than as unmeasured,
        # which is a different fact and an unrecoverable one. Keyword-only, so a decode cannot be spelled positionally
        # beside a measured mint and read alike at the call site.
        return ContentKey(value=value, fmt=fmt, byte_length=Nothing)

    @overload
    def project(self, view: Literal["value"] = ..., /) -> "ContentKey": ...
    @overload
    def project(self, view: Literal["hex"], /) -> str: ...
    @overload
    def project(self, view: Literal["wire"], /) -> str: ...
    @overload
    def project(self, view: Literal["memory"], /) -> bytes: ...
    @overload
    def project(self, view: Literal["digest"], /) -> int: ...
    def project(self, view: KeyView = "value", /) -> KeyRender:
        match view:
            case "hex":
                # the wire lowering plus the tag tail — never a second `{value:032x}`: a claim that one arm is the
                # sole lowering site is void the moment a sibling arm re-spells the width, so `hex` composes `wire`.
                return f"{self.project('wire')}:{self.fmt}"
            case "wire":
                # the bare 32-lowercase-hex wire spelling — the appearance-vocabulary fragment's `wireLower`
                # pattern rejects the `hex` view's `:{fmt}` tail, and this arm is the ONE python lowering site,
                # so a manifest producer never hand-formats `{value:032x}` inline and forks the address
                return f"{self.value:032x}"
            case "memory":
                return self.memory
            case "digest":
                return self.value
            case "value":
                return self
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def hex(self) -> str:
        return self.project("hex")

    @property
    def memory(self) -> bytes:
        return self.value.to_bytes(16, "little")

    @property
    def wire_bytes(self) -> bytes:
        # Generated `bytes` columns use the kernel's ONE crossing projection, UInt128 big-endian. `memory` is the
        # little-endian canonical-input projection and cannot substitute here without byte-swapping the digest.
        return self.value.to_bytes(16, "big")


class CanonicalWriter:
    # Exact seed-zero crossing writer. The accumulator receives each field directly, so a terminal raw model body
    # is never duplicated merely to hash a semantic prefix beside it.
    __slots__ = ("_digest", "_byte_length")

    def __init__(self) -> None:
        self._digest = xxhash.xxh3_128(seed=0)
        self._byte_length = 0

    def _emit(self, value: bytes) -> Self:
        self._digest.update(value)
        self._byte_length += len(value)
        return self

    def ordinal(self, value: int) -> Self:
        return self._emit(pack("<i", value))

    def u128(self, value: U128) -> Self:
        return self._emit(value.to_bytes(16, "little"))

    def double(self, value: float) -> Self:
        # Match .NET `double.NaN`'s one quiet payload and collapse signed zero. Contract validation excludes every
        # non-finite tessellation budget, but the writer itself stays total over the kernel grammar.
        if isnan(value):
            return self._emit((0xFFF8000000000000).to_bytes(8, "little"))
        return self._emit(pack("<d", 0.0 if value == 0.0 else value))

    def string(self, value: str) -> Self:
        wire = value.encode("utf-8")
        return self.ordinal(len(wire)).raw(wire)

    def bytes(self, value: bytes) -> Self:
        # Variable-width octets frame exactly like UTF-8 text; `raw` remains reserved for fixed-width or already
        # delimited material, so a semantic bytes member cannot collide by shifting a field boundary.
        return self.ordinal(len(value)).raw(value)

    def rows[T](self, values: tuple[T, ...], field: Callable[[T, Self], None], /) -> Self:
        self.ordinal(len(values))
        for value in values:  # Exemption: the canonical writer is the incremental framing seam
            field(value, self)
        return self

    def raw(self, value: bytes) -> Self:
        return self._emit(value)

    @beartype(conf=FAULT_CONF)
    def key(self, fmt: KeyFmt) -> ContentKey:
        # One close, one digest owner: the writer and `ContentIdentity` share `_closed`, so measurement, span
        # evidence, and key projection cannot fork even though the writer streams instead of retaining its preimage.
        with _derive_span(fmt, "fields") as span:
            return _closed(span, fmt, self._digest.intdigest(), Some(self._byte_length))


class IdentityPolicy(Struct, frozen=True, gc=False):
    tolerance: Tolerance = 1e-6

    @property
    def spec(self) -> bytes:
        return f"{self.tolerance:.17g}".encode()


CANONICAL_POLICY: Final[IdentityPolicy] = IdentityPolicy()


@tagged_union(frozen=True)
class IdentitySource:
    # `stream` and `parts` are two multi-input modalities, never one: `stream` carries BUFFER CHUNKS of a single
    # payload, where the chunk boundary is an I/O artefact and framing it would key one file differently per read
    # size; `parts` carries N SEMANTIC fields whose boundary IS meaning, where concatenating them lets a byte moving
    # across a boundary hold the key still. So the framing rides the modality that means it and the estate's
    # `[PREIMAGE_FRAMING]` law lands at its one owner rather than as a lambda every producer re-spells.
    tag: Literal["whole", "stream", "parts", "merkle", "canonical"] = tag()
    whole: bytes = case()
    stream: tuple[bytes, ...] = case()
    parts: tuple[bytes, ...] = case()
    merkle: tuple[ContentKey, ...] = case()
    canonical: Struct = case()

    @staticmethod
    def lift(source: Source) -> "IdentitySource":
        # TOTAL over the declared family AND over the tuple element regime alike: every tuple decides at the two arms
        # below, so the `Iterable` arm can no longer swallow a heterogeneous one as a chunk stream — the mis-key that
        # handed `fold` a tuple of tuples and raised a bare `TypeError` inside `digest.update`, outside every rail.
        match source:
            case IdentitySource() as lifted:
                return lifted
            case tuple() as keys if keys and all(isinstance(key, ContentKey) for key in keys):
                return IdentitySource(merkle=keys)
            # `all` over no element holds, so the EMPTY tuple lands here and its seed-only fold is the deterministic
            # degenerate key; `bytes(...)` coerces a buffer chunk once, so the fold receives the case's own declared
            # `tuple[bytes, ...]` rather than whatever the producer's iterable happened to hold.
            case tuple() as chunks if all(isinstance(chunk, Buffer) for chunk in chunks):
                return IdentitySource(stream=tuple(bytes(chunk) for chunk in chunks))
            case Struct() as payload:
                return IdentitySource(canonical=payload)
            # `Buffer` (PEP 688) claims every buffer-protocol payload as `whole` BEFORE the `Iterable` arm can mis-key an iterable
            # buffer (`array`) as a chunk stream; `bytes(payload)` coerces once.
            case Buffer() as payload:
                return IdentitySource(whole=bytes(payload))
            # a ONE-SHOT iterable materializes once and re-enters at the tuple arms, so the element regime is read at
            # ONE site and no guard exhausts a generator the fold would then receive empty. `tuple` is excluded here
            # because a tuple already decided above — admitting it would recur forever on the un-admitted payload.
            case Iterable() as chunks if not isinstance(chunks, str | tuple):
                return IdentitySource.lift(tuple(chunks))
            case _ as unreachable:
                assert_never(unreachable)

    def fold(self, seed: U64) -> tuple[U128, Option[int]]:
        # every arm over BYTES measures its own extent, so four of the five answer `Some`. The `merkle` spine is the
        # one arm folding CHILD extents, and a parent whose children include a decoded key measured nothing itself:
        # the fold binds through `Option` so one unmeasured child leaves the whole parent unmeasured, where a
        # `sum_by` over a defaulted zero would publish the parent as lighter than its own contents.
        match self:
            case IdentitySource(tag="whole", whole=payload):
                return xxhash.xxh3_128_intdigest(payload, seed=seed), Some(len(payload))
            case IdentitySource(tag="canonical", canonical=payload):
                wire = _ENCODER.encode(payload)
                return xxhash.xxh3_128_intdigest(wire, seed=seed), Some(len(wire))
            case IdentitySource(tag="merkle", merkle=children):
                spine = b"".join(child.memory for child in children)
                extent = Block.of_seq(children).fold(
                    lambda held, child: held.bind(lambda total: child.byte_length.map(lambda size: total + size)), Some(0)
                )
                return xxhash.xxh3_128_intdigest(spine, seed=seed), extent
            case IdentitySource(tag="stream", stream=chunks):
                digest = xxhash.xxh3_128(seed=seed)
                for chunk in chunks:  # Exemption: the incremental digest is xxhash's own streaming seam
                    digest.update(chunk)
                return digest.intdigest(), Some(Block.of_seq(chunks).sum_by(len))
            case IdentitySource(tag="parts", parts=fields):
                # `[PREIMAGE_FRAMING]` rides `_framed`, the module's ONE spelling of the width and byte order, so the
                # key preimage here and the settings-seed preimage `ContentIdentity.seed` folds cannot drift apart.
                return _framed(xxhash.xxh3_128(seed=seed), fields).intdigest(), Some(Block.of_seq(fields).sum_by(len))
            case _ as unreachable:
                assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------

_ENCODER: Final[Encoder] = Encoder(order="deterministic")
_TRACER: Final[trace.Tracer] = scoped(trace.get_tracer, SCOPES[Scope.IDENTITY])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _framed[H: xxhash.xxh3_64 | xxhash.xxh3_128](digest: H, fields: tuple[bytes, ...]) -> H:
    # `[PREIMAGE_FRAMING]` at its ONE owner: the COUNT frames the collection and every variable-width field carries
    # its own little-endian u64 length ahead of its bytes, so no field boundary is spoofable by a value that happens
    # to contain a delimiter and no re-partition of the same total bytes collides. The width and byte order are this
    # routine's alone — a producer framing its own preimage forks both the instant one site spells
    # `to_bytes(4, "big")`, and every fork is a silent collision. Both XXH3 widths expose the same `update` seam, so
    # the 128-bit key preimage and the 64-bit settings-seed preimage frame through this one body rather than two.
    digest.update(len(fields).to_bytes(8, "little"))
    for field in fields:  # Exemption: the incremental digest is xxhash's own streaming seam
        digest.update(len(field).to_bytes(8, "little"))
        digest.update(field)
    return digest


@contextmanager
def _derive_span(fmt: str, modality: str) -> Iterator[Span]:
    # SOLE `content.derive` bracket both entries compose; attribute writes gate on `is_recording`.
    with _TRACER.start_as_current_span("content.derive") as span:
        if span.is_recording():
            span.set_attributes({"identity.fmt": fmt, "identity.modality": modality})
        yield span


def _closed(span: Span, fmt: str, value: U128, byte_length: Option[int]) -> ContentKey:
    # SOLE close-annotate-status body: the key mints FIRST so the span attribute reads the owner's own `wire` view — a
    # raw `U128` overflows the OTLP signed-int64 attribute bound, and an inline `{value:032x}` here would be the third
    # site formatting a width the `project` arm claims to own alone. Then the clean-path OK; a fault never reaches
    # here — the boundary wrapper records ERROR on the still-open span instead.
    key = ContentKey(value=value, fmt=fmt, byte_length=byte_length)  # every mint MEASURED its preimage; only `decoded` answers Nothing
    if span.is_recording():
        span.set_attribute("identity.key", key.project("wire"))
    span.set_status(Status(StatusCode.OK))
    return key


def _minted(span: Span, fmt: str, lifted: IdentitySource, seed: U64) -> ContentKey:
    value, byte_length = lifted.fold(seed)
    return _closed(span, fmt, value, byte_length)


def derived[T](fmt: str, source: IdentitySource, run: Callable[[Span], T]) -> RuntimeRail[T]:
    # railed composition, gated FIRST: an unlawful `fmt` refuses before the span opens and before the seed derives, so
    # a forked namespace costs no span, no digest, and no cache slot, and the refusal names the coordinate where a
    # minted key would have named nothing. Past the gate `boundary` fences INSIDE the live span so a canonical-encode
    # `EncodeError` records on it. Both refusals resolve a rostered `RAISES` anchor, so the caller's `fmt` rides as a
    # NAMED coordinate on the gate's row rather than standing in as the fence subject a leg never declared.
    match Ok(fmt) if KEY_FMT.fullmatch(fmt) is not None else Error(IDENTITY_FMT.raised(fmt, KEY_FMT.pattern)):
        case Result(tag="error") as refused:
            return refused
        case _:
            with _derive_span(fmt, source.tag) as span:
                return boundary(IDENTITY_DERIVE, lambda: run(span), catch=EncodeError)


class ContentIdentity:
    @staticmethod
    def seed(fmt: str, policy: IdentityPolicy) -> U64:
        # the settings seed frames its two fields under the SAME law the key preimage does. A separator join here
        # made `KEY_FMT`'s exclusion of that byte load-bearing on a gate this entry never runs — `seed` takes a bare
        # `fmt` and carries no rail — so a caller reaching it directly could slide the split and collide two settings
        # onto one seed. Framed, the collision is unreachable whatever `fmt` spells and the entry needs no gate.
        return _framed(xxhash.xxh3_64(), (fmt.encode(), policy.spec)).intdigest()

    @overload
    @classmethod
    def of(
        cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["value"] = ..., seed: Option[U64] = ...
    ) -> RuntimeRail[ContentKey]: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["hex"], seed: Option[U64] = ...) -> RuntimeRail[str]: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["wire"], seed: Option[U64] = ...) -> RuntimeRail[str]: ...
    @overload
    @classmethod
    def of(
        cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["memory"], seed: Option[U64] = ...
    ) -> RuntimeRail[bytes]: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["digest"], seed: Option[U64] = ...) -> RuntimeRail[int]: ...
    @classmethod
    def of(
        cls, fmt: str, source: Source, policy: IdentityPolicy = CANONICAL_POLICY, *, view: KeyView = "value", seed: Option[U64] = Nothing
    ) -> RuntimeRail[KeyRender]:
        lifted = IdentitySource.lift(source)
        resolved = seed.default_with(lambda: cls.seed(fmt, policy))
        return derived(fmt, lifted, lambda span: _minted(span, fmt, lifted, resolved).project(view))

    @classmethod
    @beartype(conf=FAULT_CONF)
    def key(cls, fmt: KeyFmt, source: BareValue, policy: IdentityPolicy = CANONICAL_POLICY, *, seed: Option[U64] = Nothing) -> ContentKey:
        # `key`'s signature excludes `Struct`, so `lift` cannot key `canonical` and the fold runs no fallible encode — same core, no rail.
        # It carries no rail to refuse on either, so the grammar rides the REFINED hint instead: the shared `KEY_FMT`
        # under `FAULT_CONF` raises the one canonical violation the `CLASSIFY` `api` row folds at whichever fence
        # encloses the caller. One grammar, two seams keyed by the entry's own fallibility — never a second pattern.
        lifted = IdentitySource.lift(source)
        with _derive_span(fmt, lifted.tag) as span:
            return _minted(span, fmt, lifted, seed.default_with(lambda: cls.seed(fmt, policy)))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
