"""Descriptor-driven admission for asynchronous Connect message bodies."""

from collections.abc import AsyncGenerator, AsyncIterator, Awaitable, Callable
from enum import EnumCheck, StrEnum, verify
from typing import Final, final, Protocol, runtime_checkable

from connectrpc.code import Code
from connectrpc.errors import ConnectError
from connectrpc.request import RequestContext
from protobuf import Message
from protovalidate import CompilationError, EvaluationError, ValidationError, Validator, Violation


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
