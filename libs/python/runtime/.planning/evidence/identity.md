# [PY_RUNTIME_IDENTITY]

`ContentIdentity` is the single content-addressing owner the whole branch consumes, the module `rasm.runtime.identity`: one XxHash128 key over canonical bytes, reproducing the C# `System.IO.Hashing.XxHash128` seed with format and consumer-folded policy bytes carried into the key, so a re-tessellation at identical settings is a cache hit by reference. Data, geometry, compute, and artifacts consume this one owner and mint no parallel content key.

One span-fold core serves both entries — `_derive_span` the sole `content.derive` bracket, `_minted` the sole fold-annotate-status body — so the railed `of` and the bare `key` differ only in the fallibility fence, never a re-opened span or a second status spelling. Its tracer mints from the `reliability/faults#FAULT` `SCOPES[Scope.IDENTITY]` row and the railed entry fences through `boundary(fmt, ...)` inside the live span. Corpus-parity binding lives in the sibling `evidence/reproduction` module, split out so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`).

## [01]-[INDEX]

- [01]-[IDENTITY]: the `ContentKey` value object with its `project` output axis, the `Tolerance`-folded seed, the closed `IdentitySource` modality ADT, the span-fold core, and the railed `of` beside the bare `key`.

## [02]-[IDENTITY]

- Owner: `IdentityPolicy.spec` IS the canonical-seed field contract — every field it renders enters the seed bytes — and the policy is a GENERIC carrier: a domain knob such as geometry's tessellation deflection/angle rides a consumer-owned policy folded into the canonical seed bytes, never a new `IdentityPolicy` field per domain. Key equality is bytes-law — `of(fmt, source)` under the default and under an explicit `CANONICAL_POLICY` mint the same key, the compute design-key resume cache the demanding proof. `IdentitySource` owns its own `lift` and `fold`, so dispatch is total and the digest algebra rides the union, never an external dispatcher or a second entrypoint.
- Entry: `of` is the one polymorphic derivation over input shape and output projection — no per-render method and no parallel `of_canonical`; `key` is the bare synchronous accessor beside it, the one fallibility split, never a `rail: bool` knob. An empty or mixed tuple falls through to `stream`, whose seed-only fold is a deterministic degenerate key. `seed` is the `Option[U64]` override: `Nothing` the policy-folded settings seed, `Some(0)` the bare C# `XxHash128.HashToUInt128(span)` seed-zero path the `GeometryHash`/`NamingHashOps` boundary mints — geometry `mesh/daemon` keys GLB wire bytes under this seed-zero `RepresentationContentHash` parity contract — so the seed origin is one parameter, never a fake policy. Identity is recovered from the value shape, never a path, name suffix, or mode flag.
- Auto: the `merkle` child transcription reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold before `XxHash128.HashToUInt128`, so a parent key is order-sensitive over its parts. `lift`'s payload modalities are exported branch law — data keys operation bytes and derived-snapshot Merkle keys, compute keys buffer/stream payloads for its resume cache, geometry keys GLB bytes — so narrowing any modality is a cross-folder break. `project("hex")` renders `{value:032x}:{fmt}` so a companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key`.
- Growth: a new evaluation parameter is one `Tolerance` field on `IdentityPolicy.spec`; a new output render one `KeyView` member plus one `project` arm; a new input modality one `IdentitySource` case plus one `lift` shape plus one `fold` arm; a distinct seed origin one `Some(value)` through the existing override; a new span attribute one line in the span-fold core reaching both entries.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law — and the C# `InterchangeIdentity` is the cross-boundary mechanics owner this seed reproduces. Consumers ride the unbroken `of`/`key`/`ContentKey`/`hex` surface. Its span scopes exactly the derivation: the downstream `execution/lanes#LANE` cache hit/miss the returned key drives is the lane owner's span, never folded into `content.derive`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Buffer, Callable, Iterable, Iterator
from contextlib import contextmanager
from typing import Annotated, Final, Literal, assert_never, overload

import xxhash
from expression import Nothing, Option, case, tag, tagged_union
from expression.collections import Block
from msgspec import EncodeError, Meta, Struct
from msgspec.msgpack import Encoder
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, boundary

# --- [TYPES] ----------------------------------------------------------------------------

# msgspec rejects any integer bound past int64 at codec/convert build, so only the `ge=0` floor rides `Meta`; the ceilings are the
# digest algebra's — `xxh3_128_intdigest` yields <2**128 by construction, seeds are the 64-bit xxhash domain.
type U128 = Annotated[int, Meta(ge=0)]
type U64 = Annotated[int, Meta(ge=0)]
type Tolerance = Annotated[float, Meta(gt=0.0)]
type KeyView = Literal["value", "hex", "memory", "digest"]
type KeyRender = ContentKey | str | bytes | int
type Source = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | Struct

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
    def project(self, view: Literal["memory"], /) -> bytes: ...
    @overload
    def project(self, view: Literal["digest"], /) -> int: ...
    def project(self, view: KeyView = "value", /) -> KeyRender:
        match view:
            case "hex":
                return f"{self.value:032x}:{self.fmt}"
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
    tag: Literal["whole", "stream", "merkle", "canonical"] = tag()
    whole: bytes = case()
    stream: tuple[bytes, ...] = case()
    merkle: tuple[ContentKey, ...] = case()
    canonical: Struct = case()

    @staticmethod
    def lift(source: Source) -> "IdentitySource":
        match source:
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
                for chunk in chunks:
                    digest.update(chunk)
                return digest.intdigest(), Block.of_seq(chunks).sum_by(len)
            case _ as unreachable:
                assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------

_ENCODER: Final[Encoder] = Encoder(order="deterministic")
_TRACER: Final[trace.Tracer] = trace.get_tracer(SCOPES[Scope.IDENTITY])


# --- [OPERATIONS] -----------------------------------------------------------------------


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
    # railed composition: `boundary` fences INSIDE the live span so a canonical-encode
    # `EncodeError` records on it; `subject` is the caller's `fmt`, never a clobbering literal.
    with _derive_span(fmt, source.tag) as span:
        return boundary(fmt, lambda: run(span), catch=EncodeError)


class ContentIdentity:
    @staticmethod
    def seed(fmt: str, policy: IdentityPolicy) -> U64:
        return xxhash.xxh3_64_intdigest(fmt.encode() + b"|" + policy.spec)

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
    def key(
        cls,
        fmt: str,
        source: Buffer | Iterable[bytes] | tuple[ContentKey, ...],
        policy: IdentityPolicy = CANONICAL_POLICY,
        *,
        seed: Option[U64] = Nothing,
    ) -> ContentKey:
        # `key`'s signature excludes `Struct`, so `lift` cannot key `canonical` and the fold runs no fallible encode — same core, no rail.
        lifted = IdentitySource.lift(source)
        with _derive_span(fmt, lifted.tag) as span:
            return _minted(span, fmt, lifted, seed.default_with(lambda: cls.seed(fmt, policy)))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
