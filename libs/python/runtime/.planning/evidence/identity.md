# [PY_RUNTIME_IDENTITY]

`ContentIdentity` is the single content-addressing owner the whole branch consumes, the module `rasm.runtime.identity`. It derives one XxHash128 key over canonical bytes, reproducing the C# `System.IO.Hashing.XxHash128` seed with format, deflection, and tolerance folded into the key, so a re-tessellation at identical settings is a cache hit by reference. This collapses the former parallel content owners — data `ExchangeBundle`, artifacts `ContentDigest`/`ArtifactBundle`, the companion GLB key — into one owner data, geometry, compute, and artifacts consume without re-minting.

Five admitted surfaces weave as one rail. `expression` mints the `IdentitySource` `@tagged_union` whose `lift`/`fold` own the open-shape normalization, the canonical-`Struct` encode, and the Merkle/stream `Block` fold; `msgspec` supplies the `gc=False` `Meta`-bounded value objects and the cached deterministic `Encoder`; `xxhash` the seeded XXH3 digest; `opentelemetry-api` the `is_recording()`-gated `content.derive` span carrying a total `Status(StatusCode.OK | ERROR)` the reuse-fabric cache reads, its tracer minted from the `reliability/faults#FAULT` `SCOPES[Scope.IDENTITY]` row; runtime `reliability/faults#FAULT` `boundary(fmt, ...)` fences the railed entry inside that span. ONE span-fold core serves both entries: `_derive_span` is the sole `content.derive` bracket and `_minted` the sole fold-annotate-status body, so the railed `of` and the bare `key` differ only in the fallibility fence, never a re-opened span or a second attribute/status spelling. The `SeedReproduction`/`CorpusFixture`/`ParityReceipt` corpus-parity binding is the sibling `evidence/reproduction` module — split out so its `receipts` import stays DAG-legal (`identity < receipts < reproduction`).

## [01]-[INDEX]

- [01]-[IDENTITY]: the `gc=False` `Meta`-bounded XxHash128 content key with its `project`/`memory`/`hex` output axis, the `Tolerance`-bounded settings-folded seed, the closed four-case `IdentitySource` modality ADT (`whole`/`stream`/`merkle`/`canonical`) owning `lift`+`fold` including the `msgspec`-`Struct` canonical encode, the one `_derive_span`+`_minted` span-fold core, and the two entries composed over it — the railed input-and-output-parameterized `of` and the bare infallible `key`.

## [02]-[IDENTITY]

- Owner: `ContentIdentity` derives the content key under one `content.derive` span through the one span-fold core. `ContentKey` is the `gc=False` value object carrying the `Meta`-bounded 128-bit identity, the format tag, and the byte length, with `project` rendering every consumer view and `memory`/`hex` the named LE-span and `032x` properties. `IdentityPolicy` is the `gc=False` evaluation policy whose three `Tolerance` (`Meta(gt=0.0)`) fields fold into the seed through its `spec` projection — a GENERIC content-seed carrier: a domain knob (geometry's tessellation deflection/angle) rides a consumer-owned policy folded into the canonical seed bytes, never a new `IdentityPolicy` field per domain. `CANONICAL_POLICY` is the one exported default the `of`/`key` `policy` parameter binds, and key equality is bytes-law — `of(fmt, source)` under the default and `of(fmt, source, CANONICAL_POLICY)` mint the same key, the compute `experiments/study`/`experiments/history` design-key resume cache the demanding proof. `IdentitySource` is the closed four-case `@tagged_union` modality ADT (`whole`/`stream`/`merkle`/`canonical`) that owns its own `lift` (open `Source` → closed case) and `fold` (case → `(U128, byte_length)`) so dispatch is total and the digest algebra — including the canonical `msgspec.Struct` encode through the cached `_ENCODER` — rides the union, never an external dispatcher or a second entrypoint. `KeyView` is the output-projection axis; `KeyRender` the union the projection returns.
- Entry: `ContentIdentity.of` is the one polymorphic derivation parameterized over both input shape and output projection, riding the railed `derived` composition of the span-fold core so a canonical-encode `EncodeError` rails exactly once. `fmt` is a free-form interpolated tag (`f"bundle-{algo}"`, `f"preview-{tag}"`). `source` admits the `Buffer`/`Iterable[bytes]`/`tuple[ContentKey, ...]`/`Struct` shapes `IdentitySource.lift` normalizes into the closed ADT, the `Buffer` arm the PEP 688 protocol the `xxhash` `_InputType = str | Buffer` constructor accepts: a non-empty all-`ContentKey` tuple keys `merkle`, a `msgspec.Struct` payload keys `canonical`, the `Buffer()` arm claims every buffer-protocol payload (`bytes`/`bytearray`/`memoryview`/`array`) into `whole` through one `bytes(...)` coerce, and any remaining non-`str` `Iterable` keys `stream`. The arm order is load-bearing: the `Buffer()` case precedes the `Iterable()` case so an iterable buffer (`array`) keys `whole` rather than mis-keying as a chunk stream, while the `tuple`/`Struct` cases precede `Buffer()` because neither satisfies the buffer protocol. An empty or non-`ContentKey` tuple falls through to `stream`, whose seed-only fold is a deterministic degenerate key; a stray `str` — the one non-`Buffer` value the `Iterable` arm's `not isinstance(chunks, str)` guard excludes — reaches `assert_never`, replacing the prior open `case chunks:` catch-all that silently mis-keyed it as a chunk stream. `view` selects the projection so one entrypoint returns the value object, its `hex`, its LE `memory`, or its raw `int` over every source — `bytes` and `Struct` alike — without a second method per render or a parallel `of_canonical`. `ContentIdentity.key(fmt, source)` is the synchronous BARE-key accessor beside the railed `of`: its signature excludes `Struct`, so `lift` cannot key the `canonical` case and the fold runs no fallible encode — `key` composes the SAME `_derive_span` bracket and `_minted` body and returns a bare `ContentKey` a leaf producer keys `ArtifactReceipt.<Case>(key, ...)` by directly, while `of` stays the railed entry the fallible `canonical` `Struct` source and the reproduction parity fold consume — the one fallibility split, never a `rail: bool` knob and never a second span-fold spelling. `seed` is the `Option[U64]` override: `Nothing` defaults to the policy-folded settings seed of the production tessellation path, and `Some(0)` selects the bare C# `XxHash128.HashToUInt128(span)` seed-zero path the `GeometryHash`/`NamingHashOps` boundary mints — geometry `mesh/daemon` keys GLB wire bytes through the buffer modality under this seed-zero `RepresentationContentHash` parity contract — so the seed origin is one parameter rather than a fake policy. Identity is recovered from the value shape, never a path, name suffix, or mode flag.
- Auto: `IdentitySource.fold` owns the per-modality digest under one seed. `whole` runs `xxh3_128_intdigest` over the payload; `canonical` lowers the carried `Struct` through the cached deterministic `_ENCODER.encode` and `xxh3_128_intdigest`s the wire bytes (the `msgspec` content-identity admission law, the encode the railed `of` fault-fences); `merkle` joins each child's LE `memory` into one spine through `b"".join` (a single O(n) allocation, never an O(n²) `+`-fold) and `Block.sum_by`s the byte length before the parent `xxh3_128_intdigest`; `stream` feeds each chunk into the stateful `xxh3_128` updater and reads the length through `Block.sum_by(len)`. The child LE transcription reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold over before `XxHash128.HashToUInt128`, so the parent key is order-sensitive over its parts. The three `lift` payload modalities are exported branch law: data's egress keys operation bytes through the buffer/stream arms and its derived-snapshot Merkle key through the merkle arm, compute keys buffer and stream payloads for its design-key resume cache, and geometry keys GLB bytes through the buffer arm — narrowing any modality is a cross-folder break. `ContentIdentity.seed` folds `fmt` and the `IdentityPolicy.spec` (`.17g`-formatted deflection/tolerance/angle) into a `xxh3_64_intdigest` `U64` so a coarse and a fine tessellation key distinctly while identical bytes at identical settings key identically.
- Auto: the span-fold core is two private surfaces both entries compose exactly once. `_derive_span(fmt, modality)` is the sole `content.derive` bracket — it opens the span through the `SCOPES[Scope.IDENTITY]`-minted tracer and batches the `fmt`/modality attributes through `set_attributes` behind an `is_recording()` gate. `_minted(span, fmt, lifted, seed)` is the sole fold-annotate-status body — it runs `IdentitySource.fold`, writes the `032x`-rendered `identity.key` attribute (the raw `U128` overflows the OTLP signed-int64 attribute bound), sets `Status(StatusCode.OK)`, and mints the `ContentKey`. `derived(fmt, source, run)` composes the bracket with the `reliability/faults#FAULT` `boundary(fmt, ..., catch=EncodeError)` fence INSIDE the live span — `subject` is the caller's `fmt`, threaded so the fault names the derived key rather than a clobbering literal — so the `_convert` weave records a canonical-encode `EncodeError` on the open span and sets ERROR while the clean path's OK rides `_minted`, the status the conversion owns never re-annotated, the same `_traced`-shaped discipline `transport/wire#WIRE_RAIL` `Decode._traced` holds. The span scopes exactly the derivation; the downstream `execution/lanes#LANE` `Map[ContentKey, T]` cache hit/miss the returned `ContentKey` keys is a separate span the lane owner opens, never folded into `content.derive`. `ContentKey.project` is the one output render closed by `assert_never`: `"hex"` renders `{value:032x}:{fmt}` so a companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key`, `"memory"` returns the LE `memory` the C# `UInt128` layout and the `merkle` child transcription share, `"digest"` the raw `int` the cache compares, and `"value"` the key itself; `hex`/`memory` delegate to `project` so the downstream `ContentKey.hex` contract is unbroken.
- Packages: `expression` (`tagged_union`/`tag`/`case` the `IdentitySource` ADT, `Block.of_seq`/`sum_by` the Merkle and stream length sums, `Option`/`Nothing` the seed override with `Option.default_with` the lazy policy-seed fallback), `msgspec` (`Struct(gc=False)` the leaf value objects, `Meta` the `U128`/`U64`/`Tolerance` constraints, `msgpack.Encoder(order="deterministic")` the cached canonical-bytes codec the `canonical` arm encodes through, `EncodeError` the railed-entry-fenced class), `xxhash` (`xxh3_128_intdigest`, `xxh3_64_intdigest`, the streaming `xxh3_128` updater), `opentelemetry-api` (`trace.get_tracer`, `Tracer.start_as_current_span`, `Span.set_attributes`/`set_attribute`/`is_recording`/`set_status`, `trace.Status`/`StatusCode` the total derive-span status), stdlib `contextlib` (`contextmanager` the `_derive_span` bracket); runtime `reliability/faults#FAULT` (`boundary`/`RuntimeRail` the railed-entry `fmt`-subject lift, `Scope`/`SCOPES` the tracer-scope row this owner mints its handle from).
- Growth: a new evaluation parameter is one `Tolerance` field on `IdentityPolicy.spec` folded into `seed`; a new output render is one `KeyView` member plus one `project` arm reaching every call site; a new input modality is one `IdentitySource` case plus one `lift` shape plus one `fold` arm; a distinct seed origin is one `Some(value)` through the existing override; a new span attribute is one line in `_derive_span`/`_minted` reaching both entries at once; the leaf-producer bare-key surface is `key` (the infallible byte/stream/merkle sources) beside the railed projected `of` (all sources incl. the fallible `canonical` `Struct`) — the one fallibility split, no second hashing pass, no per-render method, no parallel canonical rail.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law the C# `System.IO.Hashing.XxHash128` and the whole-artifact identity rail hold. Data/geometry/compute/artifacts consume this owner through the unbroken `of`/`key`/`ContentKey`/`hex` surface (`key` the synchronous bare accessor for the infallible byte sources every leaf `_emit` mints its receipt off, `of` the railed projected entry the fallible `canonical` `Struct` and the `evidence/reproduction` parity fold keep), the C# `InterchangeIdentity` the cross-boundary mechanics owner the seed reproduces. The deleted forms are:
  - a path-keyed identity, a second hashing owner per package, or a cross-setting cache hit;
  - an open `Source` catch-all; a narrow `bytes`-only `Source` rejecting the `bytearray`/`memoryview`/`array` the `lift` `Buffer()` arm coerces where the PEP 688 `Buffer` admits the whole buffer protocol; an `Iterable()`-before-`Buffer()` arm order mis-keying an iterable buffer (`array`) as a chunk stream;
  - a parallel `of_canonical` entrypoint where the `canonical` `IdentitySource` case folds the `Struct` through the one `of`; a raw `@trapped` call site where the railed `of` weaves the lift; a per-render projection method; an external `_lift`/`_key` dispatcher where the ADT owns its fold; a two-pass merkle fold;
  - a second `content.derive` span open or an inline attribute/status spelling beside the `_derive_span`+`_minted` core; a hardcoded `boundary("identity", ...)` subject clobbering the `fmt` the fence names; a per-page tracer literal beside the `SCOPES[Scope.IDENTITY]` row; an attribute computed without an `is_recording()` gate; a `content.derive` span left `UNSET` on a fault arm; a raw `U128` `identity.key` attribute overflowing the OTLP signed-int64 bound where the `032x` render holds;
  - a second module spelling beside `rasm.runtime.identity`.

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

type U128 = Annotated[int, Meta(ge=0, lt=2**128)]
type U64 = Annotated[int, Meta(ge=0, lt=2**64)]
type Tolerance = Annotated[float, Meta(gt=0.0)]
type KeyView = Literal["value", "hex", "memory", "digest"]
type KeyRender = ContentKey | str | bytes | int
type Source = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | Struct

# --- [MODELS] ---------------------------------------------------------------------------


class ContentKey(Struct, frozen=True, gc=False):
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
    deflection: Tolerance = 0.01
    tolerance: Tolerance = 1e-6
    angle_tolerance: Tolerance = 1e-4

    @property
    def spec(self) -> bytes:
        return f"{self.deflection:.17g}|{self.tolerance:.17g}|{self.angle_tolerance:.17g}".encode()


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
            # `Buffer` (PEP 688, `@runtime_checkable`) claims every buffer-protocol payload —
            # `bytes`/`bytearray`/`memoryview`/`array` — as `whole` BEFORE the `Iterable` arm can
            # mis-key an iterable buffer (`array`) as a chunk stream; `bytes(source)` coerces once.
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
    # the SOLE `content.derive` bracket both entries compose; attribute writes gate on `is_recording`.
    with _TRACER.start_as_current_span("content.derive") as span:
        if span.is_recording():
            span.set_attributes({"identity.fmt": fmt, "identity.modality": modality})
        yield span


def _minted(span: Span, fmt: str, lifted: IdentitySource, seed: U64) -> ContentKey:
    # the SOLE fold-annotate-status body: `032x` render (a raw `U128` overflows the OTLP
    # signed-int64 attribute bound), then the clean-path OK; a fault never reaches here —
    # `_convert` records ERROR on the still-open span instead.
    value, byte_length = lifted.fold(seed)
    if span.is_recording():
        span.set_attribute("identity.key", f"{value:032x}")
    span.set_status(Status(StatusCode.OK))
    return ContentKey(value=value, fmt=fmt, byte_length=byte_length)


def derived[T](fmt: str, source: IdentitySource, run: Callable[[Span], T]) -> RuntimeRail[T]:
    # the railed composition: `boundary` fences INSIDE the live span so a canonical-encode
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
        # the bare infallible accessor: the signature excludes `Struct`, so `lift` cannot key
        # `canonical` and the fold runs no fallible encode — same `_derive_span`/`_minted` core,
        # no rail; `of` and `key` differ ONLY in the fallibility contract.
        lifted = IdentitySource.lift(source)
        with _derive_span(fmt, lifted.tag) as span:
            return _minted(span, fmt, lifted, seed.default_with(lambda: cls.seed(fmt, policy)))
```
