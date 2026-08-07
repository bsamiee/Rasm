# [PY_RUNTIME_IDENTITY]

`ContentIdentity` is the single content-addressing owner the whole branch consumes, the module `rasm.runtime.identity`: one XxHash128 key over canonical bytes under a two-arm seed. The explicit `Some(0)` arm is the seed-zero cross-branch parity path every peer reproduces; the default arm derives its seed from the format and the consumer-folded policy bytes, so a re-tessellation at identical settings is a cache hit by reference and a settings change partitions the cache. Data, geometry, compute, and artifacts consume this one owner and mint no parallel content key.

One span-fold core serves both entries — `_derive_span` the sole `content.derive` bracket, `_minted` the sole fold-annotate-status body — so the railed `of` and the bare `key` differ only in the fallibility fence, never a re-opened span or a second status spelling. Its tracer mints from the `reliability/faults#FAULT` `SCOPES[Scope.IDENTITY]` row and the railed entry fences through `boundary(fmt, ...)` inside the live span. Corpus-parity binding lives in the sibling `evidence/reproduction` module, split out so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`).

## [01]-[INDEX]

- [02]-[IDENTITY]: the `ContentKey` value object with its `project` output axis, the `KEY_FMT` grammar both entries gate on, the `Tolerance`-folded seed, the closed `IdentitySource` modality ADT, the span-fold core, and the railed `of` beside the bare `key`.

## [02]-[IDENTITY]

- Owner: `IdentityPolicy.spec` IS the canonical-seed field contract — every field it renders enters the seed bytes, which `_framed` length-and-count frames exactly as it frames a `parts` key preimage, so one width and one byte order serve both — and the policy is a GENERIC carrier: a domain knob such as geometry's tessellation deflection/angle rides a consumer-owned policy folded into the canonical seed bytes, never a new `IdentityPolicy` field per domain. Key equality is bytes-law — `of(fmt, source)` under the default and under an explicit `CANONICAL_POLICY` mint the same key, the compute design-key resume cache the demanding proof. `IdentitySource` owns its own `lift` and `fold`, so dispatch is total and the digest algebra rides the union, never an external dispatcher or a second entrypoint.
- Entry: `KEY_FMT` admits the `fmt` before any derivation runs, because `fmt` enters the seed AND renders as the `hex` tail the C# peer joins on, so an unlawful spelling forks the key namespace at both ends and every later comparison answers "changed" for a cause the key cannot carry. One grammar, two seams keyed by each entry's own fallibility: `of` refuses on the rail before the span opens, while `key` — which returns a bare value and has no rail — carries the refined `KeyFmt` hint under `FAULT_CONF`, whose canonical violation the `CLASSIFY` `api` row folds at the enclosing fence. A per-call-site spelling check and a second pattern are the two deleted forms. `of` is the one polymorphic derivation over input shape and output projection — no per-render method and no parallel `of_canonical`; `key` is the bare synchronous accessor beside it, the one fallibility split, never a `rail: bool` knob. An empty or mixed tuple falls through to `stream`, whose seed-only fold is a deterministic degenerate key. A multi-FIELD preimage names its modality instead of inferring it — an already-lifted `IdentitySource(parts=...)` rides `of` verbatim, so the length-and-count framing the estate's preimage law demands runs at this owner and no producer spells a `to_bytes` width of its own; `key`'s signature admits `BareSource` while still excluding `Struct`: the split is encode-fallibility — a `Struct` must encode and an encode can raise, which the bare accessor has no rail to carry, while a lifted `parts`/`stream` source is already bytes and its length-and-count framing cannot fail, so the lifted case rides `key` verbatim and `of` remains the rail-carrying entry for fallible admissions. The refinement is what makes that split structural: a bare `IdentitySource` annotation admits its own `canonical` case, so the exclusion the prose states would be one the type walks straight past. `seed` is the `Option[U64]` override: `Nothing` the policy-folded settings seed, `Some(0)` the bare C# `XxHash128.HashToUInt128(span)` seed-zero path the `GeometryHash`/`NamingHashOps` boundary mints — geometry `mesh/daemon` keys GLB wire bytes under this seed-zero `RepresentationContentHash` parity contract — so the seed origin is one parameter, never a fake policy. Identity is recovered from the value shape, never a path, name suffix, or mode flag.
- Auto: the `merkle` child transcription reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold before `XxHash128.HashToUInt128`, so a parent key is order-sensitive over its parts. `lift`'s payload modalities are exported branch law — data keys operation bytes and derived-snapshot Merkle keys, compute keys buffer/stream payloads for its resume cache, geometry keys GLB bytes — so narrowing any modality is a cross-folder break. `project("hex")` renders `{value:032x}:{fmt}`, the digest-colon-tag spelling a C# artifact address carries, so a companion GLB result minted on the `Some(0)` arm keys byte-identically to the kernel seed-zero `RepresentationContentHash` — a default-arm key shares that render and never that value, since its seed preimage is this branch's own; `project("wire")` renders the bare 32-lowercase-hex form every wire digest and manifest key field carries — the python peer of `ContentAddress.ToValue()`, the ONE lowering site the key-spelling carve demands.
- Growth: a new evaluation parameter is one `Tolerance` field on `IdentityPolicy.spec`; a new output render one `KeyView` member with one `project` arm; a new input modality one `IdentitySource` case with one `fold` arm, plus one `lift` shape only where a bare value discriminates it; a distinct seed origin one `Some(value)` through the existing override; a new span attribute one line in the span-fold core reaching both entries; a widened `fmt` vocabulary is one `KEY_FMT` edit reaching the rail gate, the refined hint, and the corpus census at once.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law — and the C# `InterchangeIdentity` is the cross-boundary mechanics owner this seed reproduces. Consumers ride the unbroken `of`/`key`/`ContentKey`/`hex` surface. Its span scopes exactly the derivation: the downstream `execution/lanes#LANE` cache hit/miss the returned key drives is the lane owner's span, never folded into `content.derive`. This owner mints the branch's `CANONICAL_BYTE_IDENTITY` instance on the explicit `Some(0)` arm, under the `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` framing law and the `[CONTENT_KEY]` seed-zero law; parity across the three independent mints IS the conformance, and WHICH fields a producer offers stays the producer's — the framing of them never is, because a width or byte order chosen at a call site forks the key namespace with no surface able to report it. The default arm's derived seed governs the branch's own re-tessellation cache identity alone and states no parity: no conformance entry pins its seed preimage, so a peer reading a default-arm key reads a branch-local address.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Buffer, Callable, Iterable, Iterator
from contextlib import contextmanager
from typing import Annotated, Final, Literal, assert_never, overload

import xxhash
from beartype import beartype
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import EncodeError, Meta, Struct
from msgspec.msgpack import Encoder
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import FAULT_CONF, SCOPES, BoundaryFault, RuntimeRail, Scope, boundary, scoped

# --- [TYPES] ----------------------------------------------------------------------------

# msgspec rejects any integer bound past int64 at codec/convert build, so only the `ge=0` floor rides `Meta`; the ceilings are the
# digest algebra's — `xxh3_128_intdigest` yields <2**128 by construction, seeds are the 64-bit xxhash domain.
type U128 = Annotated[int, Meta(ge=0)]
type U64 = Annotated[int, Meta(ge=0)]
type Tolerance = Annotated[float, Meta(gt=0.0)]
type KeyView = Literal["value", "hex", "wire", "memory", "digest"]
type KeyRender = ContentKey | str | bytes | int
# an ALREADY-LIFTED source rides the same entry, which is what keeps the two multi-part modalities separable: a bare
# iterable is buffer chunks of one payload and lifts to `stream`, while N SEMANTIC parts are a value the caller
# constructs and hands in as `IdentitySource(parts=...)`. The discriminant stays recoverable from the value.
type Source = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | Struct | IdentitySource
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
    byte_length: int

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
                return f"{self.value:032x}:{self.fmt}"
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
        match source:
            case IdentitySource() as lifted:
                return lifted
            case tuple() as parts if parts and all(isinstance(part, ContentKey) for part in parts):
                return IdentitySource(merkle=parts)
            case Struct() as payload:
                return IdentitySource(canonical=payload)
            # `Buffer` (PEP 688) claims every buffer-protocol payload as `whole` BEFORE the `Iterable` arm can mis-key an iterable
            # buffer (`array`) as a chunk stream; `bytes(payload)` coerces once.
            case Buffer() as payload:
                return IdentitySource(whole=bytes(payload))
            case Iterable() as chunks if not isinstance(chunks, str):
                return IdentitySource(stream=tuple(chunks))
            case _ as unreachable:
                assert_never(unreachable)

    def fold(self, seed: U64) -> tuple[U128, int]:
        match self:
            case IdentitySource(tag="whole", whole=payload):
                return xxhash.xxh3_128_intdigest(payload, seed=seed), len(payload)
            case IdentitySource(tag="canonical", canonical=payload):
                wire = _ENCODER.encode(payload)
                return xxhash.xxh3_128_intdigest(wire, seed=seed), len(wire)
            case IdentitySource(tag="merkle", merkle=children):
                spine = b"".join(child.memory for child in children)
                return xxhash.xxh3_128_intdigest(spine, seed=seed), Block.of_seq(children).sum_by(lambda c: c.byte_length)
            case IdentitySource(tag="stream", stream=chunks):
                digest = xxhash.xxh3_128(seed=seed)
                for chunk in chunks:  # Exemption: the incremental digest is xxhash's own streaming seam
                    digest.update(chunk)
                return digest.intdigest(), Block.of_seq(chunks).sum_by(len)
            case IdentitySource(tag="parts", parts=fields):
                # `[PREIMAGE_FRAMING]` rides `_framed`, the module's ONE spelling of the width and byte order, so the
                # key preimage here and the settings-seed preimage `ContentIdentity.seed` folds cannot drift apart.
                return _framed(xxhash.xxh3_128(seed=seed), fields).intdigest(), Block.of_seq(fields).sum_by(len)
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


def _minted(span: Span, fmt: str, lifted: IdentitySource, seed: U64) -> ContentKey:
    # SOLE fold-annotate-status body: `032x` render (a raw `U128` overflows the OTLP signed-int64 attribute bound), then the
    # clean-path OK; a fault never reaches here — `_convert` records ERROR on the still-open span instead.
    value, byte_length = lifted.fold(seed)
    if span.is_recording():
        span.set_attribute("identity.key", f"{value:032x}")
    span.set_status(Status(StatusCode.OK))
    return ContentKey(value=value, fmt=fmt, byte_length=byte_length)


def derived[T](fmt: str, source: IdentitySource, run: Callable[[Span], T]) -> RuntimeRail[T]:
    # railed composition, gated FIRST: an unlawful `fmt` refuses before the span opens and before the seed derives, so
    # a forked namespace costs no span, no digest, and no cache slot, and the refusal names the coordinate where a
    # minted key would have named nothing. Past the gate `boundary` fences INSIDE the live span so a canonical-encode
    # `EncodeError` records on it; `subject` is the caller's `fmt`, never a clobbering literal.
    match Ok(fmt) if KEY_FMT.fullmatch(fmt) is not None else Error(BoundaryFault(config=("identity.fmt", f"{fmt!r} breaches {KEY_FMT.pattern}"))):
        case Result(tag="error") as refused:
            return refused
        case _:
            with _derive_span(fmt, source.tag) as span:
                return boundary(fmt, lambda: run(span), catch=EncodeError)


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
    def key(
        cls,
        fmt: KeyFmt,
        source: Buffer | Iterable[bytes] | tuple[ContentKey, ...] | BareSource,
        policy: IdentityPolicy = CANONICAL_POLICY,
        *,
        seed: Option[U64] = Nothing,
    ) -> ContentKey:
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
