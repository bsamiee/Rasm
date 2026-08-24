# [PY_RUNTIME_BODY]

`BodyAdmission` owns descriptor-driven constraint admission of every asynchronous Connect body element — one unary request and response, every streamed element of a client, server, or bidi stream — on whichever trust boundary constructs it: the server projects a refusal onto the `ConnectError` status the peer reads, and the client raises `AdmissionError` retaining the typed `protovalidate` evidence before a socket opens. This page reads generated descriptors alone: it imports no runtime sibling and names no `rasm.contracts` family, so a new message family widens it with zero edits.

`AsyncClosable` is the one shape this page mints outward — `transport/artifact` imports it as `from rasm.runtime.transport.body import AsyncClosable` to release a caller-owned async source at every refusal. `transport/serve#SERVE` seats `BodyAdmission(AdmissionSide.SERVER)` on every generated application, and `transport/shapes#VOCABULARY` lifts `AdmissionError` through its `dialed` weave at every generated-client edge. Connect's interceptor primitives run on asyncio, so every host and proof runs under the asyncio backend alone.

## [01]-[INDEX]

- [02]-[POSTURE]: closed side and phase axes, the `protovalidate` cause union, the `AsyncClosable` release probe, and the `AdmissionError` carrier.
- [03]-[ADMISSION]: one module-scope `Validator`, the side-by-phase refusal projection, and `BodyAdmission` over the four interceptor protocols.

## [02]-[POSTURE]

- Owner: `AdmissionSide` and `AdmissionPhase` are the closed posture axes every refusal projection matches; `AdmissionError` is the client carrier.
- Cases: `AdmissionCause` closes over the three `protovalidate` raises — `ValidationError` carries violations, the other two are engine defects.
- Law: `AdmissionError.violations` reads `()` for an engine defect, so one field tells a contract refusal from a broken rule set.
- Law: `AsyncClosable` is `runtime_checkable`, so one `isinstance` probe decides release on any async source — generator, stream, or handle.
- Entry: `AdmissionError(phase, cause)` with `phase`, `cause`, and `violations` read-only.
- Packages: `protovalidate` (`ValidationError`, `CompilationError`, `EvaluationError`, `Violation`); stdlib `enum` and `typing`.
- Growth: a new posture value lands as one enum member and one arm in each closed match of `[03]-[ADMISSION]`; the type checker names the missing arm.
- Boundary: the posture pair and the carrier hold no message body; `transport/shapes#VOCABULARY` maps the carrier onto the runtime fault rail.

```python signature
"""Descriptor-driven admission for asynchronous Connect message bodies."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import EnumCheck, StrEnum, verify
from typing import final, Protocol, runtime_checkable

from protovalidate import CompilationError, EvaluationError, ValidationError, Violation


# --- [TYPES] ----------------------------------------------------------------------------


@verify(EnumCheck.UNIQUE)
class AdmissionSide(StrEnum):
    """The trust boundary on which body admission executes."""

    CLIENT = "client"
    SERVER = "server"


@verify(EnumCheck.UNIQUE)
class AdmissionPhase(StrEnum):
    """The message direction refused by contract admission."""

    REQUEST = "request"
    RESPONSE = "response"


type AdmissionCause = ValidationError | CompilationError | EvaluationError


@runtime_checkable
class AsyncClosable(Protocol):
    """An asynchronous iterator that releases its own source on early exit."""

    async def aclose(self) -> None:
        """Close an asynchronous iterator early."""


# --- [ERRORS] ---------------------------------------------------------------------------


@final
class AdmissionError(Exception):
    """A client-side contract refusal retaining its typed validation evidence."""

    __slots__ = ("_cause", "_phase")

    def __init__(self, phase: AdmissionPhase, cause: AdmissionCause) -> None:
        """Retain one client-side refusal."""
        super().__init__(f"contract {phase.value} admission failed")
        self._phase = phase
        self._cause = cause

    @property
    def phase(self) -> AdmissionPhase:
        """The refused message direction."""
        return self._phase

    @property
    def cause(self) -> AdmissionCause:
        """The original Protovalidate failure."""
        return self._cause

    @property
    def violations(self) -> tuple[Violation, ...]:
        """Structured constraint violations, or none for engine defects."""
        match self._cause:
            case ValidationError() as refused:
                return tuple(refused.violations)
            case CompilationError() | EvaluationError():
                return ()
```

## [03]-[ADMISSION]

- Owner: `BodyAdmission` satisfies the four `connectrpc.interceptor` protocols structurally, so one instance seats server-side or client-side.
- Law: the refusal matrix is side by phase — a server request refusal crosses `INVALID_ARGUMENT` carrying the one `buf.validate.Violations` detail.
- Law: server response refusal and every engine defect refuse `INTERNAL` with zero details, so no rule set or handler output leaks to a peer.
- Law: the client raises `AdmissionError(phase, cause)` on both phases of every shape, the `ValidationError` violations retained per element.
- Law: `_admitted` refuses a non-`Message` element with `TypeError` — a Connect body is a generated class or nothing.
- Law: one `Validator` at module scope compiles each descriptor's rules once; `_VALIDATOR` is the `[SERVICES]` handle, never a per-call construction.
- Law: client-stream request refusal is lazy — Connect drains the request generator inside `call_next` and wraps the raise as a `ConnectError`.
- Law: `_StreamAdmission` retains that `AdmissionError`, so the caller gets its typed refusal chained from the transport error, late elements too.
- Law: a `ConnectError` with no retained refusal — a peer `UNAVAILABLE`, a `CANCELED` scope — passes through unchanged.
- Law: signal custody is per call — each client-stream and bidi call mints one `_StreamAdmission`, so concurrent calls never cross refusals.
- Law: `_stream` closes its source in `finally` through `AsyncClosable`, so cancellation and refusal alike release the caller-owned generator.
- Law: server-stream and bidi interceptors return lazy generators, so a response refusal raises at the element the consumer drains, never at the call.
- Entry: `intercept_unary`, `intercept_client_stream`, `intercept_server_stream`, `intercept_bidi_stream`, positional-only per the protocols.
- Packages: `connectrpc` (`Code`, `ConnectError`, `RequestContext`), `protobuf-py` (`Message`), `protovalidate` (`Validator`).
- Growth: a new Connect arity is one `intercept_*` method composing `_stream` and `_admitted`; a per-shape rule mirror has no seat.
- Boundary: this owner evaluates rules and projects refusals; deadline, metadata, spans, and the served roster are `transport/serve#SERVE`'s.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncGenerator, AsyncIterator, Awaitable, Callable
from typing import Final, final

from connectrpc.code import Code
from connectrpc.errors import ConnectError
from connectrpc.request import RequestContext
from protobuf import Message
from protovalidate import CompilationError, EvaluationError, ValidationError, Validator


# --- [SERVICES] -------------------------------------------------------------------------

_VALIDATOR: Final = Validator()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _status(phase: AdmissionPhase, cause: AdmissionCause, /) -> ConnectError:
    """Project one engine refusal onto the Connect status the serving side returns.

    Returns:
        The status a server raises for this phase and cause.
    """
    match phase, cause:
        case AdmissionPhase.REQUEST, ValidationError() as refused:
            return ConnectError(Code.INVALID_ARGUMENT, "contract request admission failed", (refused.to_proto(),))
        case _, ValidationError():
            return ConnectError(Code.INTERNAL, "contract response admission failed")
        case _, CompilationError() | EvaluationError():
            return ConnectError(Code.INTERNAL, "contract validation engine failed")


def _refusal(side: AdmissionSide, phase: AdmissionPhase, cause: AdmissionCause, /) -> Exception:
    """Project one engine refusal onto the carrier its own trust boundary raises.

    Returns:
        The exception this side raises for the refusal.
    """
    match side:
        case AdmissionSide.CLIENT:
            return AdmissionError(phase, cause)
        case AdmissionSide.SERVER:
            return _status(phase, cause)


def _admitted[M](message: M, side: AdmissionSide, phase: AdmissionPhase, /) -> M:
    """Evaluate one generated body element against its own descriptor rules.

    Returns:
        The admitted message.

    Raises:
        TypeError: The Connect body element is not a generated message.
        AdmissionError: The client boundary refused the element.
        ConnectError: The server boundary refused the element.
    """
    if not isinstance(message, Message):
        raise TypeError("Connect body admission requires a protobuf-py message")
    try:
        _VALIDATOR.validate(message)
    except (ValidationError, CompilationError, EvaluationError) as cause:
        refusal = _refusal(side, phase, cause)
        raise refusal from cause
    return message


# --- [COMPOSITION] ----------------------------------------------------------------------


@final
class _StreamAdmission:
    """Retain a lazy request refusal across the HTTP body's error projection."""

    __slots__ = ("refusal",)

    def __init__(self) -> None:
        self.refusal: AdmissionError | None = None


@final
class BodyAdmission:
    """Validate every asynchronous Connect request and response body element."""

    __slots__ = ("_side",)

    def __init__(self, side: AdmissionSide) -> None:
        """Bind admission to a client or server trust boundary."""
        self._side = side

    def _admitted[M](self, message: M, phase: AdmissionPhase) -> M:
        return _admitted(message, self._side, phase)

    async def _stream[M](self, messages: AsyncIterator[M], phase: AdmissionPhase) -> AsyncGenerator[M]:
        try:
            async for message in messages:
                yield self._admitted(message, phase)
        finally:
            if isinstance(messages, AsyncClosable):
                await messages.aclose()

    async def _client_requests[M](self, messages: AsyncIterator[M], signal: _StreamAdmission) -> AsyncGenerator[M]:
        try:
            async for message in self._stream(messages, AdmissionPhase.REQUEST):
                yield message
        except AdmissionError as refused:
            signal.refusal = refused
            raise

    async def intercept_unary[REQ, RES](
        self, call_next: Callable[[REQ, RequestContext[REQ, RES]], Awaitable[RES]], request: REQ, ctx: RequestContext[REQ, RES], /
    ) -> RES:
        """Admit one request and one response.

        Returns:
            The admitted response.

        Raises:
            ConnectError: The transport refuses the call for a non-admission reason.
        """
        response = await call_next(self._admitted(request, AdmissionPhase.REQUEST), ctx)
        return self._admitted(response, AdmissionPhase.RESPONSE)

    async def intercept_client_stream[REQ, RES](
        self,
        call_next: Callable[[AsyncIterator[REQ], RequestContext[REQ, RES]], Awaitable[RES]],
        request: AsyncIterator[REQ],
        ctx: RequestContext[REQ, RES],
        /,
    ) -> RES:
        """Admit every request element and the unary response.

        Returns:
            The admitted response.

        Raises:
            ConnectError: The transport refuses the call for a non-admission reason.
        """
        signal = _StreamAdmission()
        requests = self._client_requests(request, signal) if self._side is AdmissionSide.CLIENT else self._stream(request, AdmissionPhase.REQUEST)
        try:
            response = await call_next(requests, ctx)
        except ConnectError as transport:
            if signal.refusal is not None:
                raise signal.refusal from transport
            raise
        return self._admitted(response, AdmissionPhase.RESPONSE)

    def intercept_server_stream[REQ, RES](
        self, call_next: Callable[[REQ, RequestContext[REQ, RES]], AsyncIterator[RES]], request: REQ, ctx: RequestContext[REQ, RES], /
    ) -> AsyncIterator[RES]:
        """Admit the unary request and every response element.

        Returns:
            The lazy admitted response stream.
        """
        return self._stream(call_next(self._admitted(request, AdmissionPhase.REQUEST), ctx), AdmissionPhase.RESPONSE)

    def intercept_bidi_stream[REQ, RES](
        self,
        call_next: Callable[[AsyncIterator[REQ], RequestContext[REQ, RES]], AsyncIterator[RES]],
        request: AsyncIterator[REQ],
        ctx: RequestContext[REQ, RES],
        /,
    ) -> AsyncIterator[RES]:
        """Admit every request and response element.

        Returns:
            The lazy admitted response stream.
        """

        async def admitted() -> AsyncIterator[RES]:
            signal = _StreamAdmission()
            requests = self._client_requests(request, signal) if self._side is AdmissionSide.CLIENT else self._stream(request, AdmissionPhase.REQUEST)
            try:
                async for response in self._stream(call_next(requests, ctx), AdmissionPhase.RESPONSE):
                    yield response
            except ConnectError as transport:
                if signal.refusal is not None:
                    raise signal.refusal from transport
                raise

        return admitted()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AdmissionError", "AdmissionPhase", "AdmissionSide", "AsyncClosable", "BodyAdmission"]
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
