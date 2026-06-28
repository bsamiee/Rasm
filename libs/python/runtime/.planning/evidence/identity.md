# [PY_RUNTIME_IDENTITY]

`ContentIdentity` is the single content-addressing owner the whole branch consumes. It derives one XxHash128 key over canonical bytes, reproducing the C# `System.IO.Hashing.XxHash128` seed with format, deflection, and tolerance folded into the key, so a re-tessellation at identical settings is a cache hit by reference. This collapses the former parallel content owners — data `ExchangeBundle`, artifacts `ContentDigest`/`ArtifactBundle`, the companion GLB key — into one owner data, geometry, and artifacts consume without re-minting.

Five admitted surfaces weave as one rail. `expression` mints the `IdentitySource` `@tagged_union` whose `lift`/`fold` own the open-shape normalization, the canonical-`Struct` encode, and the Merkle/stream `Block` fold; `msgspec` supplies the `gc=False` `Meta`-bounded value objects and the cached deterministic `Encoder`; `xxhash` the seeded XXH3 digest; `opentelemetry-api` the `is_recording()`-gated `content.derive` span carrying a total `Status(StatusCode.OK | ERROR)` the reuse-fabric cache reads; runtime `reliability/faults#FAULT` `boundary(fmt, ...)` fences the `derived` core inside that span. One `RuntimeRail` lift over the fallible canonical-encode seam carries the `fmt` subject the fault names — the span-then-fence discipline `transport/wire#WIRE_RAIL` `Decode._traced` holds, so the `_convert` weave annotates the active span exactly once. One input-and-output-parameterized `of` discriminates every source shape and every render, collapsing the `of_canonical` parallel entrypoint and the raw `@trapped` call site into the single `derived`-fenced surface. The `[02]-[IDENTITY]` Boundary row enumerates the deleted forms.

## [01]-[INDEX]

- [01]-[IDENTITY]: the `gc=False` `Meta`-bounded XxHash128 content key with its `project`/`memory`/`hex` output axis, the `Tolerance`-bounded settings-folded seed, the closed four-case `IdentitySource` modality ADT (`whole`/`stream`/`merkle`/`canonical`) owning `lift`+`fold` including the `msgspec`-`Struct` canonical encode, and the one input-and-output-parameterized `of` rail aspected by `derived` under one `is_recording()`-gated `content.derive` span carrying a total `Status`.
- [02]-[SEED_REPRODUCTION]: the `CorpusFixture`-keyed corpus-parity binding whose `grade` folds every REAL fixture into `ParityReceipt` rows the `ReceiptContributor` port contributes (each DESIGN-PIN fixture riding a `planned`-phase obligation), asserting `ContentIdentity` reproduces the C# `XxHash128` seed bit-identically against the frozen `ONE_WIRE_FIXTURE_CORPUS` — including the float-bearing `MATERIAL_LAYER_GOLDEN` `IfcMaterialLayer` golden vector ([H7]) that exercises the IEEE-754-LE `Double`/`Measure` canon the integer-topology row cannot reach.

## [02]-[IDENTITY]

- Owner: `ContentIdentity` derives the content key under one `content.derive` span through the `derived` fault aspect. `ContentKey` is the `gc=False` value object carrying the `Meta`-bounded 128-bit identity, the format tag, and the byte length, with `project` rendering every consumer view and `memory`/`hex` the named LE-span and `032x` properties. `IdentityPolicy` is the `gc=False` evaluation policy whose three `Tolerance` (`Meta(gt=0.0)`) fields fold into the seed through its `spec` projection. `IdentitySource` is the closed four-case `@tagged_union` modality ADT (`whole`/`stream`/`merkle`/`canonical`) that owns its own `lift` (open `Source` → closed case) and `fold` (case → `(U128, byte_length)`) so dispatch is total and the digest algebra — including the canonical `msgspec.Struct` encode through the cached `_ENCODER` — rides the union, never an external dispatcher or a second entrypoint. `KeyView` is the output-projection axis; `KeyRender` the union the projection returns.
- Entry: `ContentIdentity.of` is the one polymorphic derivation parameterized over both input shape and output projection, aspected by the `derived` `RuntimeRail` lift so a canonical-encode `EncodeError` rails exactly once. `fmt` is a free-form interpolated tag (`f"bundle-{algo}"`, `f"preview-{tag}"`). `source` admits the `Buffer`/`Iterable[bytes]`/`tuple[ContentKey, ...]`/`Struct` shapes `IdentitySource.lift` normalizes into the closed ADT, the `Buffer` arm the PEP 688 protocol the `xxhash` `_InputType = str | Buffer` constructor accepts: a non-empty all-`ContentKey` tuple keys `merkle`, a `msgspec.Struct` payload keys `canonical`, the `Buffer()` arm claims every buffer-protocol payload (`bytes`/`bytearray`/`memoryview`/`array`) into `whole` through one `bytes(...)` coerce, and any remaining non-`str` `Iterable` keys `stream`. The arm order is load-bearing: the `Buffer()` case precedes the `Iterable()` case so an iterable buffer (`array`) keys `whole` rather than mis-keying as a chunk stream, while the `tuple`/`Struct` cases precede `Buffer()` because neither satisfies the buffer protocol. An empty or non-`ContentKey` tuple falls through to `stream`, whose seed-only fold is a deterministic degenerate key; a stray `str` — the one non-`Buffer` value the `Iterable` arm's `not isinstance(chunks, str)` guard excludes — reaches `assert_never`, replacing the prior open `case chunks:` catch-all that silently mis-keyed it as a chunk stream. `view` selects the projection so one entrypoint returns the value object, its `hex`, its LE `memory`, or its raw `int` over every source — `bytes` and `Struct` alike — without a second method per render or a parallel `of_canonical`. `seed` is the `Option[U64]` override: `Nothing` defaults to the policy-folded settings seed of the production tessellation path, and `Some(0)` selects the bare C# `XxHash128.HashToUInt128(span)` seed-zero path the `GeometryHash`/`NamingHashOps` boundary mints, so the seed origin is one parameter rather than a fake policy. Identity is recovered from the value shape, never a path, name suffix, or mode flag.
- Auto: `IdentitySource.fold` owns the per-modality digest under one seed. `whole` runs `xxh3_128_intdigest` over the payload; `canonical` lowers the carried `Struct` through the cached deterministic `_ENCODER.encode` and `xxh3_128_intdigest`s the wire bytes (the `msgspec` content-identity admission law, the encode the `derived` aspect fault-rails); `merkle` joins each child's LE `memory` into one spine through `b"".join` (a single O(n) allocation, never an O(n²) `+`-fold) and `Block.sum_by`s the byte length before the parent `xxh3_128_intdigest`; `stream` feeds each chunk into the stateful `xxh3_128` updater and reads the length through `Block.sum_by(len)`. The child LE transcription reproduces the C# `BinaryPrimitives.WriteUInt128LittleEndian` canonical span the `Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.Of`/`MerkleRange.Of` and `#CRDT_WIRE` `CrdtWire.ContentKey` fold over before `XxHash128.HashToUInt128`, so the parent key is order-sensitive over its parts. `ContentIdentity.seed` folds `fmt` and the `IdentityPolicy.spec` (`.17g`-formatted deflection/tolerance/angle) into a `xxh3_64_intdigest` `U64` so a coarse and a fine tessellation key distinctly while identical bytes at identical settings key identically.
- Auto: `derived` is the one span-opening fault-fenced core every derivation rides. It opens the `content.derive` span, batches the `fmt`/modality attributes through `set_attributes` behind an `is_recording()` gate, and runs the pure render thunk inside the `reliability/faults#FAULT` `boundary(subject, ..., catch=EncodeError)` fence — `subject` is the caller's `fmt`, threaded so the fault names the derived key rather than a clobbering literal — so the fence catches a canonical-encode `EncodeError` while the span is still active. The `_convert` weave records that exception on the active span and sets ERROR; the success path sets `Status(StatusCode.OK)` itself, so the page never re-annotates a status the conversion owns, the same `_traced`-shaped discipline `transport/wire#WIRE_RAIL` `Decode._traced` holds against the faults-owner-egress trample. This collapses the prior split where a pure `of` ran untraced and an `of_canonical` rode an inline `@trapped` opening a second span. The span scopes exactly the derivation — `boundary` runs the `render` thunk eagerly inside the live `with`, so the fault records on the open span and the span ends as `derived` returns the resolved `RuntimeRail`; the downstream `execution/lanes#LANE` `Map[ContentKey, T]` cache hit/miss the returned `ContentKey` keys is a separate span the lane owner opens, never folded into `content.derive`. `ContentKey.project` is the one output render closed by `assert_never`: `"hex"` renders `{value:032x}:{fmt}` so a companion GLB result keys byte-identically to the C# `InterchangeIdentity.Key`, `"memory"` returns the LE `memory` the C# `UInt128` layout and the `merkle` child transcription share, `"digest"` the raw `int` the cache compares, and `"value"` the key itself; `hex`/`memory` delegate to `project` so the downstream `ContentKey.hex` contract is unbroken.
- Packages: `expression` (`tagged_union`/`tag`/`case` the `IdentitySource` ADT, `Block.of_seq`/`sum_by` the Merkle and stream length sums, `Option`/`Nothing`/`Some` the seed override), `msgspec` (`Struct(gc=False)` the leaf value objects, `Meta` the `U128`/`U64`/`Tolerance` constraints, `msgpack.Encoder(order="deterministic")` the cached canonical-bytes codec the `canonical` arm encodes through, `EncodeError` the `derived`-aspect-lifted class), `xxhash` (`xxh3_128_intdigest`, `xxh3_64_intdigest`, the streaming `xxh3_128` updater), `opentelemetry-api` (`trace.get_tracer`, `Tracer.start_as_current_span`, `Span.set_attributes` batching the `fmt`/modality pair in `derived` and `Span.set_attribute` the single `032x` key in `render`, `Span.is_recording`/`Span.set_status`, `trace.Status`/`StatusCode` the total derive-span status); runtime `reliability/faults#FAULT` (`boundary`/`RuntimeRail` the `derived` canonical-encode `fmt`-subject lift).
- Growth: a new evaluation parameter is one `Tolerance` field on `IdentityPolicy.spec` folded into `seed`; a new output render is one `KeyView` member plus one `project` arm reaching every call site; a new input modality is one `IdentitySource` case plus one `lift` shape plus one `fold` arm; a distinct seed origin is one `Some(value)` through the existing override; zero new entrypoint, no second hashing pass, no per-render method, no parallel canonical rail.
- Boundary: artifact identity is XxHash128 over canonical bytes — the suite hash law the C# `System.IO.Hashing.XxHash128` and the whole-artifact identity rail hold. A path-keyed identity, a second hashing owner per package, a cross-setting cache hit, an open `Source` catch-all, a parallel `of_canonical` entrypoint where the `canonical` `IdentitySource` case folds the `Struct` through the one `of`, a raw `@trapped` call site where the `derived` named aspect weaves the lift, a per-render projection method, an external `_lift`/`_key` dispatcher where the ADT owns its fold, an inline `boundary` lambda whose encode is untraced where the `derived` aspect rails it inside the one span, a hardcoded `boundary("identity", ...)` subject clobbering the `fmt` the fence names, a raw `U128` `identity.key` attribute overflowing the OTLP signed-int64 bound where the `032x` render holds, a two-pass merkle fold, a narrow `bytes`-only `Source` rejecting the `bytearray`/`memoryview`/`array` the `lift` `Buffer()` arm coerces where the PEP 688 `Buffer` admits the whole buffer protocol, an `Iterable()`-before-`Buffer()` arm order mis-keying an iterable buffer (`array`) as a chunk stream, an attribute computed without an `is_recording()` gate, and a `content.derive` span left `UNSET` on a fault arm are the deleted forms; data/geometry/artifacts consume this owner through the unbroken `of`/`ContentKey`/`hex` surface, the C# `InterchangeIdentity` the cross-boundary mechanics owner the seed reproduces.

```python signature
from collections.abc import Buffer, Callable, Iterable
from typing import Annotated, Final, Literal, assert_never, overload

import xxhash
from expression import Nothing, Option, case, tag, tagged_union
from expression.collections import Block
from msgspec import EncodeError, Meta, Struct
from msgspec.msgpack import Encoder
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import RuntimeRail, boundary

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
_TRACER: Final[trace.Tracer] = trace.get_tracer("rasm.runtime.content_identity")


# --- [OPERATIONS] -----------------------------------------------------------------------


def derived[T](subject: str, source: IdentitySource, run: Callable[[Span], T]) -> RuntimeRail[T]:
    # `boundary` fences INSIDE the live span so `_convert` records an `EncodeError` on it and sets
    # ERROR; the clean `run` sets OK — the status the conversion owns is never re-annotated here.
    with _TRACER.start_as_current_span("content.derive") as span:
        if span.is_recording():
            span.set_attributes({"identity.fmt": subject, "identity.modality": source.tag})
        return boundary(subject, lambda: run(span), catch=EncodeError)


class ContentIdentity:
    @staticmethod
    def seed(fmt: str, policy: IdentityPolicy) -> U64:
        return xxhash.xxh3_64_intdigest(fmt.encode() + b"|" + policy.spec)

    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["value"] = ..., seed: Option[U64] = ...) -> RuntimeRail[ContentKey]: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["hex"], seed: Option[U64] = ...) -> RuntimeRail[str]: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["memory"], seed: Option[U64] = ...) -> RuntimeRail[bytes]: ...
    @overload
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = ..., *, view: Literal["digest"], seed: Option[U64] = ...) -> RuntimeRail[int]: ...
    @classmethod
    def of(cls, fmt: str, source: Source, policy: IdentityPolicy = CANONICAL_POLICY, *, view: KeyView = "value", seed: Option[U64] = Nothing) -> RuntimeRail[KeyRender]:
        lifted = IdentitySource.lift(source)

        def render(span: Span) -> KeyRender:
            value, byte_length = lifted.fold(seed.default_with(lambda: cls.seed(fmt, policy)))
            if span.is_recording():
                # `032x` hex render: the raw `U128` overflows the OTLP signed-int64 attribute bound.
                span.set_attribute("identity.key", f"{value:032x}")
            span.set_status(Status(StatusCode.OK))
            return ContentKey(value=value, fmt=fmt, byte_length=byte_length).project(view)

        return derived(fmt, lifted, render)
```

## [03]-[SEED_REPRODUCTION]

- Owner: `SeedReproduction` is the Python-side binding asserting `ContentIdentity` reproduces the one C#-owned `XxHash128` seed bit-identically across the cross-runtime fixture corpus. `CorpusFixture` is the typed corpus row — a fixture `name`, its `FixtureState` (`real`/`design_pin`), the `producer` it transcribes, the `Option[bytes]` producer-frozen seed-zero canonical `stream`, and the `Block[ParityRow]` it grades — so the corpus is a FOLD over fixtures, not a single hardcoded stream: the REAL `CANONICAL_BYTE_IDENTITY` (`csharp:Rasm/Geometry` row [1]) and the float-bearing DESIGN-PIN `MATERIAL_LAYER_GOLDEN` (`csharp:Rasm.Element/Projection/address` row [7], [H7]) are two rows of one table. `ParityAspect` is the closed `Literal` vocabulary (`value_identity`/`memory_layout`) each fixture's assertions collapse into. `ParityRow` pairs each `ParityAspect` with the `KeyView` projector and the typed `KeyRender` expected reference, owning the `grade(fixture, key)` that derives the observed render and the verdict. `ParityReceipt` is the typed evidence row `grade` yields — the fixture, aspect, typed expected/observed `KeyRender` renders, and the verdict — owning the `fact` projection to its `(fixture.aspect, status)` log pair. `_CORPUS` is the `Block[CorpusFixture]` data table. The C# seed is the single mint (seed zero, two-64-bit-half order); this binding re-mints no digest and authors no fixture byte — a REAL fixture transcribes the producer-frozen reference verbatim, a DESIGN-PIN fixture carries `Nothing` and no rows until the producer freezes its byte-deriving input, and the assertion folds through the typed row rather than a method per fixture.
- Reference: the FROZEN 52-byte int32-LE canonical-adjacency stream of the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00`, and its `XxHash128.HashToUInt128` digest under seed zero is `0x9462A71A5DD13DCFA3B1D6D225FCBE70`, persisted big-endian and read C#-side as the 16-byte LE memory `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`. No byte is re-authored here; the literals are the producer's frozen reference. The float-bearing `MATERIAL_LAYER_GOLDEN` golden vector ([H7]) is the second corpus fixture: an `IfcMaterialLayer`-shaped `MaterialComposition.LayerSet` node whose `csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS` `CanonicalWriter` bytes — case ordinal `1`, layer count, then per layer the material-id length-prefixed UTF-8, the `ThicknessMm` `Measure` (the IEEE-754 little-endian `Si` magnitude with `-0.0`→`0.0`/`NaN`→one quiet pattern, quantized to `Header.Tolerance`, plus the 7 SI `Dimension` exponent ordinals), and the layer-name UTF-8 — are hashed seed-zero, the ONLY corpus fixture that exercises the float canon the integer-topology row [1] cannot reach. It is DESIGN-PIN: the concrete layer node and its digest are frozen on that producer, so this binding carries `Nothing` and no fabricated byte set and grades the moment the producer pins them through the SAME `whole`/seed-zero path row [1] uses.
- Entry: `SeedReproduction.grade` folds `_CORPUS` once through `Block.fold`: each REAL fixture (its `stream` a `Some`) derives its `ContentKey` through the whole-payload modality of `ContentIdentity.of` under the explicit `seed=Some(0)` override — the bare C# `HashToUInt128(span)` path, not a fabricated policy — and grades its `ParityRow`s into the accumulated `RuntimeRail[Block[ParityReceipt]]` through `Result.bind`, while a DESIGN-PIN fixture (`Nothing` stream) is skipped in `grade` (`Option.default_value(Ok(graded))`) and rides the obligation stream `contribute` emits. For row [1], `value_identity` projects `"digest"` against `0x9462A71A5DD13DCFA3B1D6D225FCBE70` and `memory_layout` projects `"memory"` against the C# 16-byte LE memory, so the digest-value and LE-memory checks are two rows of one fold rather than two parallel boolean methods. A `SeedReproduction` instance satisfies the `observability/receipts#RECEIPT` `ReceiptContributor` Protocol through `contribute`: the graded fold maps the `Ok` rows through `ParityReceipt.fact` into one `dict` behind an `emitted`-phase `Receipt.of`, and the `Error` arm — total though unreachable for the `bytes` corpus — through `Receipt.of` over the rail's `BoundaryFault` into the receipts owner's `rejected` case, the two `Receipt`-typed arms collapsing through `Result.merge` to one receipt rather than a phantom `EncodeError` fabricated on the success path; each DESIGN-PIN fixture additionally mints one `planned`-phase obligation `Receipt.of` naming its producer, so the float-bearing golden vector is a VISIBLE pending parity obligation on the one receipt stream the metrics/lanes fold reads, never a bare assert, an unhandled rail, or a silent gap.
- Auto: `xxhash.xxh3_128_intdigest` returns the 128-bit digest as a Python `int` whose `ContentKey.memory` (`to_bytes(16, "little")`) is the C# `UInt128` in-memory layout — the C# `XxHash128.HashToUInt128` returns a `UInt128` and `BinaryPrimitives.WriteUInt128LittleEndian` writes the same LE memory, so value-equality holds with no byte-swap when both sides read seed zero. The corpus seed is zero because the C# `NamingHashOps.Encode`/`GeometryHash` path calls `XxHash128.HashToUInt128(span)` with no seed parameter, distinct from the settings-folded `ContentIdentity.seed` the tessellation rail derives, so the `CANONICAL_BYTE_IDENTITY` parity reads the bare seed-zero path through the `seed=Some(0)` seam and the format-policy seed governs only the re-tessellation cache identity. The parity reuses the production `ContentIdentity.of` and `ContentKey.project`/`memory` rather than a re-derived digest function, so a regression in the owner's derivation or projection surfaces as a failed `ParityReceipt`, never a pass against a parallel fixture path. When the producer freezes `MATERIAL_LAYER_GOLDEN`, the SAME `whole`/seed-zero path grades the layer node's float-bearing bytes, so the cross-runtime `Double`/`Measure` canon (`-0.0`→`0.0`, `NaN`→one quiet pattern, tolerance quantization, IEEE-754 little-endian) is proven by the one rail the integer fixture already uses, never a second float-specific harness.
- Packages: `xxhash` (`xxh3_128_intdigest`), `expression` (`Block.of_seq`/`fold`/`map`/`append`/`choose`/`dict(...)` the corpus fold over `_CORPUS`, `Result.bind`/`Result.map`/`Result.map_error`/`Result.merge` the railed grade thread collapsing both arms to one `Receipt`, `Option.map`/`Option.default_value`/`Option.is_none` the `stream`-gated REAL-vs-DESIGN-PIN dispatch, `Nothing`/`Some` the seed override and the DESIGN-PIN gap, `Ok` the fold seed), `msgspec` (`Struct` the `CorpusFixture`/`ParityRow`/`ParityReceipt` rows); runtime (`content_identity.ContentIdentity`/`ContentKey`/`KeyView`/`KeyRender`, `faults.RuntimeRail` the railed grade carrier, `receipts.Receipt`/`ReceiptContributor`).
- Growth: a new corpus parity aspect is one `ParityAspect` member plus one `ParityRow` on the owning `CorpusFixture`, reaching the fold and the contributed receipt through the existing `grade`/`fact`; a new cross-runtime fixture is one `CorpusFixture` row on `_CORPUS`; a DESIGN-PIN fixture GRADUATES to REAL when its producer freezes the byte-deriving input — one `Some(stream)` plus its `ParityRow`s flipping `state` to `real`, the `grade` fold and the `contribute` stream reaching it with zero new method and no parallel boolean.
- Boundary: the corpus is read-only and the C# seed is the single mint — a Python-side re-derived fixture, a per-runtime digest function, a parallel boolean method per assertion, and a fabricated byte set for an unpinned corpus row are the named drift defects; the harness DRIVER feeding the corpus stream through `ContentIdentity.of` and grading the `ParityReceipt` rows is a future `python:testing` consumer of the same corpus, never a second fixture store here; the corpus rows [3]-[7] (`FAULT_TRIPLES`, `CRDT_OP_SET`, `GLB_BY_KEY`, `HLC_TWO_HALF`, `MATERIAL_LAYER_GOLDEN`) stay DESIGN-PIN on their cross-folder producers and carry no fabricated bytes — a Python-fabricated golden byte set for an unpinned fixture is the named single-mint defect. `MATERIAL_LAYER_GOLDEN` ([H7]) is now a FIRST-CLASS `_CORPUS` row, the float-bearing `IfcMaterialLayer`-shaped `MaterialComposition.LayerSet` node pinned on the `csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS` producer — the regression anchor the C#/Python/TypeScript `ContentAddress` agree on byte-for-byte over the one seed-zero `XxHash128` rail so a `MeasureValue`/layer-thickness float-canon drift surfaces as a single corpus mismatch; until the producer freezes the concrete node it rides the `planned`-phase obligation, never a graded pass against fabricated bytes.

```python signature
from collections.abc import Iterable
from typing import Final, Literal

from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, KeyRender, KeyView
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type ParityAspect = Literal["value_identity", "memory_layout"]
# REAL: the producer-frozen canonical bytes are present and the fixture grades NOW. DESIGN_PIN: the
# producer has not yet frozen the byte-deriving input, so the single-mint law forbids a Python-fabricated
# byte set — the fixture carries `Nothing` and rides the `planned`-phase obligation until the producer pins it.
type FixtureState = Literal["real", "design_pin"]

# --- [CONSTANTS] ------------------------------------------------------------------------

IDENTITY_FMT: Final[str] = "geometry-topology"
GOLDEN_FMT: Final[str] = "material-layer-golden"
# [1] CANONICAL_BYTE_IDENTITY — the sole host-derived REAL byte fixture, frozen on
# csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS[1]: the 52-byte int32-LE
# single-triangle adjacency stream and its seed-zero XxHash128 digest. No byte is re-authored here.
CANONICAL_STREAM: Final[bytes] = bytes.fromhex(
    "03000000030000000000000001000000000000000200000001000000020000000100000003000000000000000100000002000000"
)
CANONICAL_DIGEST: Final[int] = 0x9462A71A5DD13DCFA3B1D6D225FCBE70
CANONICAL_LE_MEMORY: Final[bytes] = bytes.fromhex("70befc25d2d6b1a3cf3dd15d1aa76294")

# --- [MODELS] ---------------------------------------------------------------------------


class ParityReceipt(Struct, frozen=True):
    fixture: str
    aspect: ParityAspect
    expected: KeyRender
    observed: KeyRender
    verified: bool

    @property
    def fact(self) -> tuple[str, str]:
        return f"{self.fixture}.{self.aspect}", "ok" if self.verified else f"{self.observed!r}!={self.expected!r}"


class ParityRow(Struct, frozen=True):
    aspect: ParityAspect
    view: KeyView
    expected: KeyRender

    def grade(self, fixture: str, key: ContentKey) -> ParityReceipt:
        observed = key.project(self.view)
        return ParityReceipt(fixture=fixture, aspect=self.aspect, expected=self.expected, observed=observed, verified=observed == self.expected)


class CorpusFixture(Struct, frozen=True):
    # one cross-runtime golden fixture the parity binding consumes. A REAL fixture carries the
    # producer-frozen seed-zero canonical byte `stream` (`Some`) plus its expected `ParityRow` renders and
    # grades NOW through the production `ContentIdentity.of` whole/seed-zero path; a DESIGN_PIN fixture
    # names its `producer` and carries `Nothing` + no rows — the single-mint law forbids a Python-fabricated
    # byte set, so it rides the `planned`-phase obligation until the producer freezes the input, then
    # graduates to REAL by one `Some(stream)` + rows with zero new method.
    name: str
    state: FixtureState
    producer: str
    stream: Option[bytes]
    rows: Block[ParityRow]

# --- [TABLES] ---------------------------------------------------------------------------

_CORPUS: Final[Block[CorpusFixture]] = Block.of_seq((
    CorpusFixture(
        name=IDENTITY_FMT,
        state="real",
        producer="csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS[1]",
        stream=Some(CANONICAL_STREAM),
        rows=Block.of_seq((
            ParityRow(aspect="value_identity", view="digest", expected=CANONICAL_DIGEST),
            ParityRow(aspect="memory_layout", view="memory", expected=CANONICAL_LE_MEMORY),
        )),
    ),
    # [7] MATERIAL_LAYER_GOLDEN — the float-bearing IfcMaterialLayer golden vector ([H7]). Its seed-zero
    # canonical bytes are the C# `CanonicalWriter` projection of a `MaterialComposition.LayerSet` node:
    # case ordinal `1`, layer count, then per layer the material-id `String`, the `ThicknessMm` `Measure`
    # (the IEEE-754 little-endian `Si` magnitude with `-0.0`->`0.0`/`NaN`->one quiet pattern, quantized to
    # `Header.Tolerance`, plus the 7 SI `Dimension` exponent ordinals), and the layer-name `String` — the
    # ONLY corpus fixture exercising the float canon the integer-topology row [1] cannot reach. DESIGN-PIN:
    # the concrete layer node and its digest are frozen on the `address#CONTENT_ADDRESS` producer, so this
    # binding carries NO fabricated bytes and grades the moment the producer pins them through the SAME
    # `whole`/seed-zero path row [1] uses — decode the producer reference, never re-mint a golden byte set.
    CorpusFixture(
        name=GOLDEN_FMT,
        state="design_pin",
        producer="csharp:Rasm.Element/Projection/address#CONTENT_ADDRESS",
        stream=Nothing,
        rows=Block.empty(),
    ),
))

# --- [SERVICES] -------------------------------------------------------------------------


class SeedReproduction:
    @staticmethod
    def grade() -> RuntimeRail[Block[ParityReceipt]]:
        # one fold over the corpus: each REAL fixture (`Some` stream) derives its `ContentKey` once through
        # the production `ContentIdentity.of` seed-zero `whole` path and grades its rows into the accumulated
        # rail through `Result.bind`; a DESIGN-PIN fixture (`Nothing` stream) is skipped here and rides the
        # `planned`-phase obligation `contribute` emits, never a graded pass against a fabricated byte set.
        def step(acc: RuntimeRail[Block[ParityReceipt]], fixture: CorpusFixture) -> RuntimeRail[Block[ParityReceipt]]:
            return acc.bind(lambda graded: fixture.stream.map(
                lambda stream: ContentIdentity.of(fixture.name, stream, view="value", seed=Some(0)).map(
                    lambda key: graded.append(fixture.rows.map(lambda row: row.grade(fixture.name, key)))
                )
            ).default_value(Ok(graded)))

        return _CORPUS.fold(step, Ok(Block.empty()))

    def contribute(self) -> Iterable[Receipt]:
        # the graded REAL fixtures collapse to one `emitted` (or `rejected`) receipt; each DESIGN-PIN fixture
        # rides one `planned`-phase obligation naming its producer, so the float-bearing golden vector is a
        # VISIBLE pending parity obligation on the one receipt stream the metrics/lanes fold reads, never a
        # silent gap and never a graded pass against fabricated bytes.
        graded = (
            self.grade()
            .map(lambda rows: Receipt.of(IDENTITY_FMT, ("emitted", IDENTITY_FMT, dict(rows.map(lambda receipt: receipt.fact)))))
            .map_error(lambda fault: Receipt.of(IDENTITY_FMT, fault))
            .merge()
        )
        pending = _CORPUS.choose(
            lambda fixture: Some(Receipt.of(fixture.name, ("planned", fixture.name, {"design_pin": fixture.producer})))
            if fixture.stream.is_none()
            else Nothing
        )
        return (graded, *pending)
```

## [04]-[RESEARCH]
