# [PYTHON_BOUNDARIES]

Foreign material crosses once: a boundary owner projects native handles, sentinels, callbacks, thread-affine work, state cells, and protocol bytes into admitted values or typed rails, so everything the interior receives — values, receipts, policies, effects — is recoverable from declarations rather than from the foreign surface that produced it. The seam is the only site that names a provider type, catches a provider exception, or holds a native lifetime; the interior is total over admitted owners.

## [01]-[SEAM_CHOOSER]

This table selects the owner for a foreign signal; when a signal matches several rows, the most specific wins, and lifetime rows are read before transport rows.

| [INDEX] | [FOREIGN_SIGNAL]        | [SEAM_OWNER]        | [INTERIOR_FORM]                         | [REJECT]                   |
| :-----: | :---------------------- | :------------------ | :-------------------------------------- | :------------------------- |
|  [01]   | native handle or FFI    | capsule owner       | `Result[T, Fault]` value projection     | raw `ctypes` handle field  |
|  [02]   | borrowed live memory    | scoped view         | detached `bytes`/value copy             | escaping `memoryview`      |
|  [03]   | null or sentinel        | admission projector | `Option[T]` or closed family            | `None`-as-failure payload  |
|  [04]   | callback or event       | subscription value  | stream-fed admitted signal              | orphan handler             |
|  [05]   | foreign-thread re-entry | portal bridge       | `Result` over `BlockingPortal` re-entry | ambient thread check       |
|  [06]   | session or singleton    | token-gated cell    | closed lifecycle family                 | boolean lifecycle flag     |
|  [07]   | protocol payload        | wire converter      | admitted owner                          | codec-bearing domain owner |
|  [08]   | signed byte field       | byte contract       | canonical octets plus hash              | parse-then-reserialize     |

## [02]-[ADMISSION]

The absence axis arrives settled: a `None`-as-failure never crosses inward, computed non-failing absence is `Option`, and a cause-bearing foreign state is a `@tagged_union` case — the shape page owns that taxonomy and this page never re-decides it. The boundary law is narrower and unowned upstream — *where* a foreign sentinel is admitted, and that it is admitted exactly once.

[SENTINEL_PROJECTION]:
- Law: a foreign absence token — `None`, a missing key, a not-found code, a provider default — projects to the carrier at the single read site that first sees the foreign value, through `Option.of_optional(value)` for non-failing absence or `option.to_result(fault)` when the caller must know the cause; the projection is the last line that names a foreign sentinel, and every later interior reader holds the carrier the seam minted.
- Law: a `None` that *re-appears* after the seam — a transform that can return `None`, a nested field that erases late — is itself a second admission site and projects there too; the present branch never manufactures `Some(None)`, because a carrier wrapping a foreign sentinel re-leaks the sentinel one hop later.
- Reject: `None` checks threaded through interior flow; a nullable field riding past the seam; `dict.get(...)` returning `None` consumed directly; `Option.value` read without an `is_some` proof.

[ENV_ADMISSION]:
- Law: the environment is the one foreign surface whose read, validation, and rail lift fuse into a single admission step — a `pydantic-settings` `BaseSettings` runs the source chain (env, dotenv, secrets dir) at construction, so `os.environ` is never read raw and a missing or malformed variable surfaces as `SettingsError`/`ValidationError` captured once into the seam fault, never a scattered `os.getenv(...) or default` across the interior.
- Law: provision and runtime state are separate axes — a required capability is *always present* as a port whose value carries its own `unavailable`/`degraded` case, so a settings field is the static admission of presence and the port's tagged state is the dynamic admission of liveness; a `None` field standing in for "the service might be down" conflates the two.
- Reject: a raw `os.environ[...]` read in interior flow; a settings value re-validated past the `BaseSettings` step; a nullable settings field where the absence carries a cause the port should own.

[PROBE_SWEEP]:
- Law: a multi-probe admission fixes its sweep disposition once at the seam — abort on the first refused probe or accumulate every refusal — and the carrier the projector mints realizes it; the abort fold short-circuits on the first `Error`, the accumulating fold keeps the full casualty set, and the sequencing operator is the rail page's, composed here and never re-derived.
- Law: a per-probe boundary obligation — a release, a converted provider exception, a token check — attaches at the probe that incurs it, never inside the traverse that sweeps, because the traverse never guarantees the element function runs; a survivor/casualty partition is admitted only when callers need both the usable values and the rejected facts.
- Reject: a single rail for a sweep whose callers need every casualty; a later walk over the admitted set reinterpreting why each probe disappeared.

```python conceptual
from typing import Annotated, Literal

from beartype.vale import Is
from expression import Error, Ok, Option, Result, case, tag, tagged_union
from pydantic import ValidationError
from pydantic_settings import BaseSettings, SettingsConfigDict, SettingsError

type Endpoint = Annotated[str, Is[lambda text: text.startswith("<scheme>")]]
type AdmitFault = Literal["<unset>", "<malformed>"]


class Env(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="<prefix>_", frozen=True, extra="forbid")
    endpoint: Endpoint
    fallback: str | None = None


@tagged_union(frozen=True)
class Link:
    tag: Literal["bound", "unavailable"] = tag()
    bound: Endpoint = case()
    unavailable: str = case()

    @staticmethod
    def admitted() -> Result[tuple["Link", Option[str]], AdmitFault]:
        try:
            env = Env()
        except SettingsError:
            return Error("<unset>")
        except ValidationError:
            return Error("<malformed>")
        return Ok((Link(bound=env.endpoint), Option.of_optional(env.fallback)))
```

## [03]-[LIFETIME]

[CAPSULE_OWNER]:
- Use: native handles, FFI pointers, pinned buffers, pooled values, external cursors, and any foreign object with an explicit release.
- Law: borrowed and owned lifetimes are cases of one closed `@tagged_union` capsule — the owned case registers release through `weakref.finalize` tied to the owner's identity and disposes on its terminal transition, the borrowed case projects a detached copy and never disposes; one `use` fold projects across both cases so a caller never branches on lifetime.
- Law: a native borrow spans the full operation that observes the pointer — the view materializes inside the borrow window, the projection copies to owned `bytes` while the window is open, and the window closes before the foreign owner can free; liveness is never tested apart from the consumption it guards, because a separate liveness probe races the free the borrow window already orders against.
- Law: callers receive values or rails, never live handles; the capsule converts its own `OSError`/`ctypes.ArgumentError`/`ValueError` into the domain fault, so no provider exception type escapes the seam.
- Exemption: the address-to-view materialization, the `view.release()` call, and the `weakref.finalize` registration inside the capsule kernel are the named platform-forced statement seam.
- Reject: a public handle field; scattered manual `close()`; parallel borrowed and owned wrapper classes; a `__del__` finalizer owning release where `weakref.finalize` states it.

```python conceptual
import ctypes
import weakref
from collections.abc import Callable
from typing import Literal

from expression import Error, Ok, Result, case, tag, tagged_union

type CapsuleFault = Literal["<provider>"]


@tagged_union(frozen=True)
class Capsule:
    tag: Literal["owned", "borrowed"] = tag()
    owned: tuple[int, int] = case()
    borrowed: tuple[int, int] = case()

    @staticmethod
    def acquired(address: int, size: int, release: Callable[[int], None], /) -> "Capsule":
        held = Capsule(owned=(address, size))
        weakref.finalize(held, release, address)
        return held

    @staticmethod
    def viewing(address: int, size: int, /) -> "Capsule":
        return Capsule(borrowed=(address, size))

    def use[T](self, copy: Callable[[memoryview], T], /) -> Result[T, CapsuleFault]:
        address, size = self.owned if self.tag == "owned" else self.borrowed
        try:
            view = ctypes.memoryview_at(address, size, readonly=True)
            try:
                return Ok(copy(view))
            finally:
                view.release()
        except (OSError, ValueError, ctypes.ArgumentError):
            return Error("<provider>")


def detached(capsule: Capsule, /) -> Result[bytes, CapsuleFault]:
    return capsule.use(bytes)
```

[REF_SAFE_PROJECTION]:
- Law: live foreign memory crosses as a scoped `memoryview`/`collections.abc.Buffer` whose result is a value copy; any value returned from the projector is detached from the foreign lifetime through `bytes(view)` or an owned array, never the view itself.
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
- Law: the seam this card owns is the inbound crossing — a foreign thread `anyio` never spawned re-entering the loop through a `BlockingPortal`. `from_thread.start_blocking_portal()` spins up the loop thread, `from_thread.BlockingPortalProvider` shares one loop across many foreign threads, and `portal.call(func, *args)` takes the coroutine function and its arguments — never a pre-built awaitable — raising `RunFinishedError` once the loop closes; the bare `from_thread.run(func, *args)` is valid only inside a worker thread `anyio` itself spawned, where the loop token is implicit. The outbound offload (`to_thread.run_sync` under a `CapacityLimiter`) and the cancellation re-raise (`get_cancelled_exc_class()`) are the rail page's effect runtime; the seam composes them and converts every other worker-thread exception into the closed seam fault, never letting a provider exception escape unconverted.
- Law: a portal-bridge fault carries which crossing failed — `RunFinishedError` after loop teardown, a converted worker exception, a refused readiness handshake — as distinct closed cases, never one stringified provider message, so a retry predicate refuses the cancellation arm first and routes the loop-closed arm to re-open rather than retry.
- Law: `ContextVar` propagation and thread affinity are separate decisions — the portal copies the caller's context across the bridge, and a callback reading no ambient state needs no propagation; an interior transform never reads ambient thread state to recover what the crossing already carried.
- Reject: a `threading.current_thread()` affinity test in interior flow; an ambient `ContextVar` read inside a reusable transform; a fire-and-forget thread launch outside a task group; `from_thread.run` from a thread `anyio` did not spawn; a stringified provider message standing in for the closed crossing fault.

[HANDOFF_DRAIN]:
- Law: a high-frequency foreign callback submits intent and returns — an `anyio` memory object stream is the log for a consumer that must see every intermediate, and a single committed cell behind the agent is the latest-value register for a per-tick consumer; the consumer's need selects the carrier, and producer back-pressure is the stream's `max_buffer_size`, independent of consumer pacing.
- Law: a bounded stream's full behavior is the seam's declared policy stated where the producer sends — `max_buffer_size=0` rendezvous-blocks the producer, a positive bound buffers then back-pressures — and `send_nowait`'s two failure signals are distinct dispositions, never one collapsed arm: `WouldBlock` on a full positive bound is the back-pressure drop the policy already authorized, while `BrokenResourceError` means every receiver closed, so the callback retires its own subscription rather than dropping into a dead channel forever.
- Reject: blocking the foreign callback on interior work; mutating interior state from the callback thread; the callback running arbitrary downstream logic instead of sending one message; collapsing `WouldBlock` and `BrokenResourceError` into one silent drop.

```python conceptual
from collections.abc import Awaitable, Callable
from typing import Literal, assert_never

import anyio
from anyio.from_thread import BlockingPortalProvider
from anyio.streams.memory import MemoryObjectSendStream
from expression import Error, Ok, Result, case, tag, tagged_union


@tagged_union(frozen=True)
class CrossFault:
    tag: Literal["loop_closed", "worker"] = tag()
    loop_closed: None = case()
    worker: str = case()


def re_entered[T](provider: BlockingPortalProvider, cross: Callable[[], Awaitable[T]], /) -> Result[T, CrossFault]:
    with provider as portal:
        try:
            return Ok(portal.call(cross))
        except anyio.RunFinishedError:
            return Error(CrossFault(loop_closed=None))
        except anyio.get_cancelled_exc_class():
            raise
        except Exception as worker:  # noqa: BLE001 — every other provider raise becomes one closed crossing case
            return Error(CrossFault(worker=type(worker).__name__))


def retry_disposition(fault: CrossFault, /) -> Literal["<retry>", "<reopen>"]:
    match fault:
        case CrossFault(tag="worker"):
            return "<retry>"
        case CrossFault(tag="loop_closed"):
            return "<reopen>"
        case _ as unreachable:
            assert_never(unreachable)


def subscribed(emitter: "Emitter", sink: MemoryObjectSendStream["Signal"], /) -> Callable[[], None]:
    def detach() -> None:
        emitter.off_change(handler)

    def handler(raw: object, /) -> None:
        try:
            sink.send_nowait(Signal.of(raw))
        except anyio.WouldBlock:
            pass
        except anyio.BrokenResourceError:
            detach()

    emitter.on_change(handler)
    return detach
```

## [05]-[STATE_CELLS]

[SERIALIZING_AGENT]:
- Use: session, singleton, wake, drain, and cross-call boundary lifetime — every cell a foreign edge mutates from more than one task.
- Law: the serialization owner is one consumer task draining a `MemoryObjectReceiveStream[Command]` — the bundled `expression.MailboxProcessor` is forbidden here because it reaches `asyncio.get_event_loop()` against the `asyncio` ban, so the cell rides an `anyio` send/receive pair where producers `send_nowait` a command and the lone consumer applies it. The agent holds the cell as a plain local, replaces the whole immutable reference per applied command, and is the only reader, so a torn multi-field read and a read-modify-write race are both structurally impossible — the `anyio` single-consumer drain is what `threading.Lock` around shared mutable state is the rejected substitute for.
- Law: the lifecycle is a closed `@tagged_union` state family (pending, live, draining, failed; never booleans) and the agent's command set is a second closed `@tagged_union` (open, close, drain) folded under one total `match` — a new lifecycle verb is one state case plus one command case plus one transition arm, never a new lock, flag, or sibling mutator, so the agent surface absorbs the verb family the next requirement adds (the `draining` state is exactly that growth: one case carrying the deadline and in-flight count, one arm fencing admission).
- Law: each command carries its own reply channel — a per-command `anyio.Event` plus a single-slot result, set after the transition publishes — so a waiter wakes only on committed state, never an attempted or aborted one, and the post-command continuation rides that reply rather than a second flag polled from outside. The transition functions stay pure: `opened`/`closed`/`drained` take the state and return the successor and its outcome, and the agent is the only impure seam, holding no factory, disposer, deadline await, or external call inside the apply step.
- Law: a stale teardown succeeds only while its token still owns the live state — the transition matches the live token before it acts, so a `close` carrying a superseded token is a total no-op, never a disposed replacement session; a faulted cell is escaped only by a fresh agent carrying typed evidence, because a cancelled `anyio.CancelScope` never resets and re-opening replaces the whole scope.
- Law: drain is one command reading phase, in-flight count, and deadline together — admission is fenced the instant the agent applies it, and the post-drain continuation waits on the same reply channel as mid-flight work reaches a typed terminal, never a `threading.Lock` plus a polled second flag.
- Reject: a `MailboxProcessor` on the `anyio` substrate; a `threading.Lock` or shutdown boolean standing in for the agent; a second flag polled from outside the reply; teardown that disposes a replacement session; a factory, deadline await, or external call inside the apply step; acting on a change fired from an aborted transition.

[MEMO_KEY]:
- Law: a boundary memo key binds the foreign identity content alone cannot recover — a provider handle's `id()`, a session token, or a capability fingerprint joins the content and policy axes into one hashable `frozendict`/`tuple` composite, so two payloads identical in bytes but sourced from distinct foreign owners never collide.
- Reject: a content-only, path-only, or type-only key dropping the foreign axis; a mutable `dict` memo store where the agent's own state or a `frozendict` snapshot owns the table.

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass, field
from typing import Literal, assert_never

from anyio import Event
from anyio.streams.memory import MemoryObjectReceiveStream, MemoryObjectSendStream
from expression import Error, Ok, Result, case, tag, tagged_union


@tagged_union(frozen=True)
class Gate:
    tag: Literal["pending", "live", "draining", "failed"] = tag()
    pending: None = case()
    live: tuple[int, "Session"] = case()
    draining: tuple[int, "Session", float, int] = case()
    failed: str = case()


def opened(gate: Gate, token: int, session: "Session", /) -> tuple[Gate, Result["Session", str]]:
    match gate:
        case Gate(tag="pending"):
            return Gate(live=(token, session)), Ok(session)
        case Gate(tag="live"):
            return gate, Ok(gate.live[1])
        case Gate(tag="draining"):
            return gate, Error("<draining>")
        case Gate(tag="failed"):
            return gate, Error(gate.failed)
        case _ as unreachable:
            assert_never(unreachable)


def closed(gate: Gate, token: int, /) -> tuple[Gate, Result[None, str]]:
    match gate:
        case Gate(tag="live") if gate.live[0] == token:
            return Gate(pending=None), Ok(None)
        case Gate(tag="draining") if gate.draining[0] == token:
            return Gate(pending=None), Ok(None)
        case _:
            return gate, Ok(None)


def drained(gate: Gate, deadline: float, /) -> tuple[Gate, Result[int, str]]:
    match gate:
        case Gate(tag="live"):
            token, session = gate.live
            return Gate(draining=(token, session, deadline, 0)), Ok(0)
        case Gate(tag="draining"):
            return gate, Ok(gate.draining[3])
        case _:
            return gate, Error("<not-live>")


@dataclass(slots=True)
class Reply[T]:
    ready: Event = field(default_factory=Event)
    slot: list[Result[T, str]] = field(default_factory=list)

    def settle(self, outcome: Result[T, str], /) -> None:
        self.slot.append(outcome)
        self.ready.set()


@tagged_union(frozen=True)
class Command:
    tag: Literal["open", "close", "drain"] = tag()
    open: tuple[int, "Session", Reply["Session"]] = case()
    close: tuple[int, Reply[None]] = case()
    drain: tuple[float, Reply[int]] = case()


async def agent(inbox: MemoryObjectReceiveStream[Command], /) -> None:
    gate = Gate(pending=None)
    async with inbox:
        async for command in inbox:
            match command:
                case Command(tag="open"):
                    token, session, reply = command.open
                    gate, opened_outcome = opened(gate, token, session)
                    reply.settle(opened_outcome)
                case Command(tag="close"):
                    token, close_reply = command.close
                    gate, closed_outcome = closed(gate, token)
                    close_reply.settle(closed_outcome)
                case Command(tag="drain"):
                    deadline, drain_reply = command.drain
                    gate, drain_outcome = drained(gate, deadline)
                    drain_reply.settle(drain_outcome)
                case _ as unreachable:
                    assert_never(unreachable)


async def commanded[T](outbox: MemoryObjectSendStream[Command], build: Callable[[Reply[T]], Command], /) -> Result[T, str]:
    reply: Reply[T] = Reply()
    outbox.send_nowait(build(reply))
    await reply.ready.wait()
    return reply.slot[0]
```

## [06]-[WIRE_CONTRACTS]

[PROTOCOL_EDGE]:
- Use: payload structs, envelopes, serializer contracts, persisted packets, and remote frames.
- Law: wire shapes stay protocol-shaped at the edge — a `msgspec.Struct` with `rename=`/`forbid_unknown_fields=True` or a pydantic ingress model is the only site where protocol and interior schemas meet, and interior canonical owners carry no codec attributes, serializer options, or transport fields. The converter is the boundary; the domain owner is unaware of the wire.
- Law: inner envelopes reject drift — `forbid_unknown_fields=True` on `msgspec.Struct` and `extra="forbid"` on a pydantic model fail an unknown member before admission — while only a declared `extra_items` band tolerates extension material.
- Reject: a `schema_version` branch on a canonical owner that belongs to read-boundary migration; last-write-wins parse for an owned protocol shape; a domain owner decorated with serializer config.

[CONVERTER_OWNER]:
- Law: one converter owns a closed wire family — `msgspec.json.Decoder(type=Family)` over a tagged union resolves the discriminant once on read and `msgspec.json.Encoder` emits it on write, and `msgspec.convert(owner, Wire, from_attributes=True)` projects a pure field rename when the wire struct keeps canonical attribute names and renames only at the encoded edge; the converter consumes exactly the family it owns and converts both wire-rejection paths — `msgspec.DecodeError` for malformed bytes, `msgspec.ValidationError` for a constraint or discriminant miss, caught at their `msgspec.MsgspecError` base — so neither escapes and the path and cause stay boundary facts.
- Law: a value-transforming projection (not a pure rename) rides an explicit constructor or a `frozendict` adapter table, never `msgspec.convert`; the convert path is reserved for name-only correspondence.
- Reject: a converter per case; a case-level codec bypassing the family owner; a sentinel returned from a decoder hook instead of a raised validation error; per-call `Decoder`/`Encoder` construction where one module-level instance serves.

```python conceptual
from typing import Literal

import msgspec
from expression import Error, Ok, Result

type WireFault = Literal["<invalid-frame>"]


class FrameSingle(msgspec.Struct, tag="single", frozen=True, forbid_unknown_fields=True, rename={"at": "t"}):
    at: int


class FrameBlock(msgspec.Struct, tag="block", frozen=True, forbid_unknown_fields=True, rename={"start": "s", "end": "e"}):
    start: int
    end: int


type Frame = FrameSingle | FrameBlock

_FRAME_DECODER = msgspec.json.Decoder(type=Frame)
_FRAME_ENCODER = msgspec.json.Encoder()


def decoded(raw: bytes, /) -> Result[Frame, WireFault]:
    try:
        return Ok(_FRAME_DECODER.decode(raw))
    except msgspec.MsgspecError:
        return Error("<invalid-frame>")


def projected(owner: "Span", /) -> bytes:
    return _FRAME_ENCODER.encode(msgspec.convert(owner, FrameBlock, from_attributes=True))
```

[BYTE_IDENTITY]:
- Use: signatures, content hashes, idempotency keys, checksums, and byte-stable forwarding.
- Law: semantic equality and byte equality are different contracts; the seam captures raw octets before any parse, emits the canonical form exactly once, and binds one digest surface per byte-identity domain at composition rather than choosing per site — the digest-surface selection itself is the stdlib owner's, composed here, not taught here.
- Law: a payload that must round-trip byte-identically is forwarded as its captured octets, never parsed and re-serialized — re-serialization re-spells floats, reorders members, and re-kinds non-finite values, breaking the signature; capture-before-parse is therefore the seam's only correct position for the hash input.
- Boundary: a receipt carries the coordinate and the hash, never the payload bytes; the persisted fingerprint is a stable cross-process digest, never a process-randomized `hash()`.
- Reject: parse-and-reserialize between verification, signing, or forwarding; a per-site encoder choice where one composition-fixed encoder serves; a `hash()` value persisted as stable identity.
