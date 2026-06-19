# [PYTHON_BOUNDARIES]

Foreign material crosses once: a boundary owner projects native handles, sentinels, callbacks, thread-affine work, state cells, and protocol bytes into admitted values or typed rails, so everything the interior receives — values, receipts, policies, effects — is recoverable from declarations rather than from the foreign surface that produced it. The seam is the only site that names a provider type, catches a provider exception, or holds a native lifetime; the interior is total over admitted owners.

## [01]-[SEAM_CHOOSER]

This table selects the owner for a foreign signal; when a signal matches several rows, the most specific wins, and lifetime rows are read before transport rows.

| [INDEX] | [FOREIGN_SIGNAL]     | [SEAM_OWNER]        | [INTERIOR_FORM]                         | [REJECT]                   |
| :-----: | :------------------- | :------------------ | :-------------------------------------- | :------------------------- |
|  [01]   | native handle or FFI | capsule owner       | `Result[T, Fault]` value projection     | raw `ctypes` handle field  |
|  [02]   | borrowed live memory | scoped view         | detached `bytes`/value copy             | escaping `memoryview`      |
|  [03]   | null or sentinel     | admission projector | `Option[T]` or closed family            | `None`-as-failure payload  |
|  [04]   | callback or event    | subscription value  | stream-fed admitted signal              | orphan handler             |
|  [05]   | thread-affine call   | marshal effect      | `Result` over `from_thread`/`to_thread` | ambient thread check       |
|  [06]   | session or singleton | token-gated cell    | closed lifecycle family                 | boolean lifecycle flag     |
|  [07]   | protocol payload     | wire converter      | admitted owner                          | codec-bearing domain owner |
|  [08]   | signed byte field    | byte contract       | canonical octets plus hash              | parse-then-reserialize     |

## [02]-[ADMISSION]

[SENTINEL_PROJECTION]:
- Use: any foreign `None`, missing key, default value, not-found code, or provider absence token.
- Law: project at the exact read site into `Option[T]`, `Result[T, Fault]`, or a closed absence family through `Option.of_optional(value)` or a `match`; the projector owns the absence vocabulary, and no `None`-as-failure survives into an interior or persisted shape.
- Law: the present branch never manufactures `Some(None)`; a value that can become `None` after a transformation returns absence explicitly instead of relying on the carrier to erase it.
- Reject: `None` checks scattered through interior flow; a nullable field riding past the seam; `dict.get` returning `None` consumed directly instead of projected.

[ABSENCE_TAXONOMY]:
- Law: cause-bearing foreign state — unavailable, degraded, pending, faulted — is a closed `@tagged_union` family, never `Option`; `Option[T]` is correct only when absence carries no action-changing cause.
- Law: provision and state are different axes: a required capability is always present as a port whose value carries its own unavailable state, and the environment read (pydantic-settings) is the one authorized sentinel-projection seam, fusing read, validation, and rail lift into one bound step.
- Reject: flattening nested absence before the layer carrying the cause is consumed; a `None` port standing in for runtime unavailability.

```python conceptual
from typing import Literal

from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union


@tagged_union(frozen=True)
class RawState:
    tag: Literal["missing", "detached", "ready"] = tag()
    missing: None = case()
    detached: str = case()
    ready: str = case()


type AdmitFault = Literal["<absent>", "<detached>"]


def admitted(state: RawState, /) -> Result[Option["Payload"], AdmitFault]:
    match state:
        case RawState(tag="missing"):
            return Ok(Nothing)
        case RawState(tag="ready"):
            return Payload.admit(state.ready).map(Some)
        case RawState(tag="detached"):
            return Error("<detached>")
        case _:
            return Error("<absent>")
```

## [03]-[LIFETIME]

[CAPSULE_OWNER]:
- Use: native handles, FFI pointers, pinned buffers, pooled values, external cursors, and any foreign object with an explicit release.
- Law: one capsule acquires, projects, and releases through an `async with`/`__exit__` owner or a `weakref.finalize` registration tied to the owner's identity; borrowed and owned lifetimes are cases of one closed family, where the owned case releases on exit and the borrowed case projects a detached copy without disposal.
- Law: a native borrow spans the full operation that observes the pointer — `ctypes.memoryview_at(addr, size)` materializes the view inside the borrow window, the projection copies to owned `bytes`, and the window closes before the foreign owner can free; liveness is never tested apart from the consumption it guards.
- Law: callers receive values or rails, never live handles; the capsule converts its own `OSError`/`ctypes.ArgumentError` into the domain fault.
- Exemption: the address-to-view materialization and the release call inside the capsule kernel are the named platform-forced statement seam.
- Reject: a public handle field; scattered manual `close()`; parallel borrowed and owned wrapper classes; a `__del__` finalizer owning release where `weakref.finalize` or a context manager states it.

```python conceptual
import ctypes
from collections.abc import Callable
from contextlib import contextmanager
from collections.abc import Iterator

from expression import Error, Ok, Result


@contextmanager
def borrowed(address: int, size: int, /) -> Iterator[memoryview]:
    view = ctypes.memoryview_at(address, size, readonly=True)
    try:
        yield view
    finally:
        view.release()


def projected[T](address: int, size: int, copy: Callable[[memoryview], T], /) -> Result[T, str]:
    try:
        with borrowed(address, size) as view:
            return Ok(copy(view))
    except (OSError, ValueError) as fault:
        return Error(f"<capsule:{fault}>")
```

[REF_SAFE_PROJECTION]:
- Law: live foreign memory crosses as a scoped `memoryview`/`Buffer` whose result is a value copy; any value returned from the projector is detached from the foreign lifetime through `bytes(view)` or an owned array, never the view itself.
- Accept: a `memoryview` or `collections.abc.Buffer` consumed inside one synchronous projection that returns owned material.
- Reject: storing a `memoryview` on a frozen owner; returning a view; carrying a foreign buffer across an `await` where the foreign owner can free it mid-suspension.

## [04]-[EVENTS_AND_THREADS]

[SUBSCRIPTION_VALUE]:
- Use: events, callbacks, observers, file watches, notifications, and foreign lifecycle hooks.
- Law: a subscription is the detacher returned by attach, holding the exact callback identity attach used; the callback borrows every touched handle for the whole subscription window, and detach completes before any borrowed handle is released. The detacher is a frozen value with a `close()` method, consumed through `with` or an `AsyncExitStack`, never an inline lambda that cannot be removed.
- Law: the subscription set is the scope — reactivation constructs a fresh set, never appends to a retained one, and the set dies with the live state that owns it.
- Exemption: the attach/detach registration calls and the posted-callback body are the named platform-forced statement seam.
- Reject: an inline lambda passed to `attach` that detach cannot match; a finalizer-owned unsubscribe; split attach and detach owners; host deregistration assumed rather than confirmed.

[HOST_MARSHAL]:
- Law: thread-affine foreign work crosses through an explicit effect — `anyio.to_thread.run_sync(fn, limiter=...)` offloads a blocking call to a worker thread under a `CapacityLimiter`, and `anyio.from_thread.run(coro)` re-enters the event loop from a foreign thread through the token captured once at the composition root; the seam routes the call's failure through the rail and never lets a worker-thread exception escape unconverted.
- Law: cancellation normalizes once into the seam's fault vocabulary — the `anyio.get_cancelled_exc_class()` exception is re-raised after cleanup, never railed, and is the first arm every retry predicate refuses; a foreign timeout and a caller shutdown carry distinct evidence when behavior differs.
- Law: `ContextVar` propagation and thread affinity are separate decisions; `anyio.to_thread.run_sync` copies the context, and a callback reading no ambient state needs no propagation.
- Reject: a `threading.current_thread()` affinity test in interior flow; an ambient `ContextVar` read inside a reusable transform; a fire-and-forget thread launch outside a task group.

[HANDOFF_DRAIN]:
- Law: a high-frequency foreign callback submits intent and returns — an `anyio` memory object stream is the log for a consumer that must see every intermediate, and a single committed cell behind the agent is the latest-value register for a per-tick consumer; the consumer's need selects the carrier, and producer back-pressure is the stream's `max_buffer_size`, independent of consumer pacing.
- Law: a bounded stream's full behavior is the seam's declared policy — `max_buffer_size=0` rendezvous-blocks the producer, a positive bound buffers then back-pressures — stated where the producer sends.
- Reject: blocking the foreign callback on interior work; mutating interior state from the callback thread; the callback running arbitrary downstream logic instead of sending one message.

```python conceptual
from collections.abc import Callable

import anyio
from anyio.streams.memory import MemoryObjectSendStream
from expression import Error, Ok, Result


async def marshaled[T](blocking: Callable[[], T], /, *, limiter: anyio.CapacityLimiter) -> Result[T, str]:
    try:
        return Ok(await anyio.to_thread.run_sync(blocking, limiter=limiter))
    except anyio.get_cancelled_exc_class():
        raise
    except Exception as fault:  # noqa: BLE001 — seam converts every provider fault to the rail
        return Error(f"<worker:{fault}>")


def subscribed(emitter: "Emitter", sink: MemoryObjectSendStream["Signal"], /) -> Callable[[], None]:
    def handler(raw: object, /) -> None:
        sink.send_nowait(Signal.of(raw))

    emitter.on_change(handler)
    return lambda: emitter.off_change(handler)
```

## [05]-[STATE_CELLS]

[TOKEN_LIFECYCLE]:
- Use: session, singleton, wake, and cross-call boundary lifetime.
- Law: boundary lifecycle is a closed state family in one cell — pending, live, failed; never booleans — and a transition replaces the whole immutable cell reference under the serializing agent, so a torn multi-field read is structurally impossible. A stale teardown succeeds only while its token still owns the live state.
- Law: a faulted cell is escaped only by a fresh instance carrying typed evidence; re-opening replaces the whole cancellation scope, because a cancelled `anyio.CancelScope` never resets.
- Reject: a shutdown boolean; teardown that disposes a replacement session; a factory or external call inside the agent's apply step.

[DRAIN_COORDINATION]:
- Law: drain reads phase, in-flight count, and deadline as one message to the lifecycle agent; admission is fenced while mid-flight work reaches a typed terminal, and the post-drain continuation rides the agent's reply, never a second flag polled from outside.
- Law: a "the set I observed did not change while I decided" guarantee is the single agent's serialization — the one reader applies messages in order, so no read-modify-write tears; a `threading.Lock` around shared mutable state is the rejected form the agent replaces.
- Reject: closing from one flag while another cell still admits; acting on a change event fired from an aborted transition.

[MEMO_KEY]:
- Law: a memo key encodes every dimension that changes output — content, policy, capability, and foreign identity feed one hashable composite (a `frozendict` or `tuple`), and a `frozenset` of the policy axes joins it.
- Reject: a path-only, type-only, or option-partial cache key; a mutable `dict` as the memo store where the agent or a `frozendict` snapshot serializes.

## [06]-[WIRE_CONTRACTS]

[PROTOCOL_EDGE]:
- Use: payload structs, envelopes, serializer contracts, persisted packets, and remote frames.
- Law: wire shapes stay protocol-shaped at the edge — a `msgspec.Struct` with `rename=`/`forbid_unknown_fields=True` or a pydantic ingress model is the only site where protocol and interior schemas meet, and interior canonical owners carry no codec attributes, serializer options, or transport fields. The converter is the boundary; the domain owner is unaware of the wire.
- Law: inner envelopes reject drift — `forbid_unknown_fields=True` on `msgspec.Struct` and `extra="forbid"` on a pydantic model fail an unknown member before admission — while only a declared `extra_items` band tolerates extension material.
- Reject: a `schema_version` branch on a canonical owner that belongs to read-boundary migration; last-write-wins parse for an owned protocol shape; a domain owner decorated with serializer config.

[CONVERTER_OWNER]:
- Law: one converter owns a closed wire family — `msgspec.json.Decoder(type=Family)` over a tagged union resolves the discriminant once on read and `msgspec.json.Encoder` emits it on write, and `msgspec.convert(owner, Wire, from_attributes=True)` projects a pure field rename when the wire struct keeps canonical attribute names and renames only at the encoded edge; the converter consumes exactly the family it owns and raises `msgspec.ValidationError` for wire-shape rejection so the path and cause stay boundary facts.
- Law: a value-transforming projection (not a pure rename) rides an explicit constructor or a `frozendict` adapter table, never `msgspec.convert`; the convert path is reserved for name-only correspondence.
- Reject: a converter per case; a case-level codec bypassing the family owner; a sentinel returned from a decoder hook instead of a raised validation error; per-call `Decoder`/`Encoder` construction where one module-level instance serves.

```python conceptual
from typing import Literal

import msgspec
from expression import Error, Ok, Result

type WireFault = Literal["<invalid-frame>"]


class FrameSingle(msgspec.Struct, tag="single", frozen=True, forbid_unknown_fields=True):
    at: int


class FrameBlock(msgspec.Struct, tag="block", frozen=True, forbid_unknown_fields=True):
    start: int
    end: int


type Frame = FrameSingle | FrameBlock

_FRAME_DECODER = msgspec.json.Decoder(type=Frame)
_FRAME_ENCODER = msgspec.json.Encoder()


def decoded(raw: bytes, /) -> Result[Frame, WireFault]:
    try:
        return Ok(_FRAME_DECODER.decode(raw))
    except msgspec.ValidationError:
        return Error("<invalid-frame>")
```

[BYTE_IDENTITY]:
- Use: signatures, content hashes, idempotency keys, checksums, and byte-stable forwarding.
- Law: semantic equality and byte equality are different contracts; raw octets are captured before parse, the canonical form is emitted once, and one encoder per byte-identity domain is fixed at composition, never chosen per site. `hashlib.file_digest(file, "sha256")` streams a file hash without loading it, and `hashlib.sha256(octets).hexdigest()` hashes an in-memory payload.
- Law: a payload that must round-trip byte-identically is forwarded as its captured octets, never parsed and re-serialized — re-serialization re-spells floats, reorders members, and re-kinds non-finite values, breaking the signature.
- Boundary: a receipt carries the coordinate and the hash, never the payload bytes.
- Reject: parse-and-reserialize between verification, signing, or forwarding; a per-site encoder choice; a `GetHashCode`-style process-randomized hash persisted as stable identity (use `hashlib` or `xxhash` for persistent fingerprints).

## [07]-[RESEARCH]

- [HOST_TOKEN]: the exact spelling of the `anyio` blocking-portal token capture for `from_thread.run` from a non-anyio foreign thread is confirmed against the `runtime` package `anyio` catalogue at capture; the seam's portal-vs-token form follows the catalogued surface.
