"""Contract admission laws across every asynchronous Connect body shape."""

# ruff: file-ignore[unused-async] -- Connect protocols require these async law doubles.


# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import AsyncIterator
from functools import partial
from typing import ClassVar, override

import anyio
from connectrpc.client import ConnectClient
from connectrpc.code import Code
from connectrpc.errors import ConnectError
from connectrpc.interceptor import BidiStreamInterceptor, ClientStreamInterceptor, ServerStreamInterceptor, UnaryInterceptor
from connectrpc.method import IdempotencyLevel, MethodInfo
from connectrpc.request import Headers, RequestContext
from connectrpc.server import ConnectASGIApplication, Endpoint
from hypercorn.asyncio import serve
from hypercorn.config import Config
from protovalidate import ValidationError
import pytest

from rasm.contracts.admission import AdmissionError, AdmissionPhase, AdmissionSide, AsyncClosable, BodyAdmission
from rasm.contracts.gen.rasm.contracts.clock.v1.hlc_pb import Hlc


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (AdmissionError, AdmissionPhase, AdmissionSide, AsyncClosable, BodyAdmission)


_METHOD = MethodInfo(name="Admit", service_name="rasm.contracts.test.Admission", input=Hlc, output=Hlc, idempotency_level=IdempotencyLevel.UNKNOWN)


# --- [MODELS] ---------------------------------------------------------------------------


class _Service:
    @staticmethod
    async def unary(request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        return _invalid() if request.logical else request

    @staticmethod
    async def client_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        message = (await _drain(request))[0]
        if message.physical == 2:
            raise ConnectError(Code.UNAVAILABLE, "service unavailable")
        return _invalid() if message.logical else message

    @staticmethod
    async def server_stream(request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        yield _invalid() if request.logical else request

    @staticmethod
    async def bidi_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        async for message in request:
            yield _invalid() if message.logical else message


class _Application(ConnectASGIApplication[_Service]):
    _METHODS: ClassVar[dict[str, MethodInfo[Hlc, Hlc]]] = {
        name: MethodInfo(name=name, service_name="rasm.contracts.test.Admission", input=Hlc, output=Hlc, idempotency_level=IdempotencyLevel.UNKNOWN)
        for name in ("Unary", "ClientStream", "ServerStream", "BidiStream")
    }

    def __init__(self) -> None:
        super().__init__(
            service=_Service(),
            endpoints=lambda service: {
                f"{self.path}/Unary": Endpoint.unary(self._METHODS["Unary"], service.unary),
                f"{self.path}/ClientStream": Endpoint.client_stream(self._METHODS["ClientStream"], service.client_stream),
                f"{self.path}/ServerStream": Endpoint.server_stream(self._METHODS["ServerStream"], service.server_stream),
                f"{self.path}/BidiStream": Endpoint.bidi_stream(self._METHODS["BidiStream"], service.bidi_stream),
            },
        )

    @property
    @override
    def path(self) -> str:
        return "/rasm.contracts.test.Admission"


# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.fixture
def anyio_backend() -> str:
    return "asyncio"


def _context() -> RequestContext[Hlc, Hlc]:
    return RequestContext(method=_METHOD, http_method="POST", request_headers=Headers())


async def _messages(*messages: Hlc) -> AsyncIterator[Hlc]:
    for message in messages:
        yield message


async def _drain(messages: AsyncIterator[Hlc]) -> tuple[Hlc, ...]:
    return tuple([message async for message in messages])


def _valid() -> Hlc:
    return Hlc(physical=1)


def _invalid() -> Hlc:
    return Hlc(physical=-1)


def _assert_server_refusal(refused: ConnectError, code: Code, detail_count: int) -> None:
    assert refused.code is code
    assert len(refused.details) == detail_count
    if refused.details:
        assert refused.details[0].type_name == "buf.validate.Violations"


def _assert_client_refusal(refused: AdmissionError, phase: AdmissionPhase) -> None:
    assert refused.phase is phase
    assert isinstance(refused.cause, ValidationError)
    assert len(refused.violations) == 1


# --- [INTERCEPTOR_PROTOCOLS]


def test_body_admission_satisfies_every_async_connect_interceptor_protocol() -> None:
    admission = BodyAdmission(AdmissionSide.CLIENT)
    assert isinstance(admission, UnaryInterceptor)
    assert isinstance(admission, ClientStreamInterceptor)
    assert isinstance(admission, ServerStreamInterceptor)
    assert isinstance(admission, BidiStreamInterceptor)


# --- [SERVER_REFUSALS]


@pytest.mark.anyio
async def test_server_request_refusals_cover_every_shape_and_disclose_details_only_to_callers() -> None:
    admission = BodyAdmission(AdmissionSide.SERVER)

    async def unary(request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        return request

    async def client_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        return (await _drain(request))[0]

    async def server_stream(request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        yield request

    async def bidi_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        async for message in request:
            yield message

    with pytest.raises(ConnectError) as unary_refusal:
        await admission.intercept_unary(unary, _invalid(), _context())
    with pytest.raises(ConnectError) as client_refusal:
        await admission.intercept_client_stream(client_stream, _messages(_invalid()), _context())
    with pytest.raises(ConnectError) as server_refusal:
        await _drain(admission.intercept_server_stream(server_stream, _invalid(), _context()))
    with pytest.raises(ConnectError) as bidi_refusal:
        await _drain(admission.intercept_bidi_stream(bidi_stream, _messages(_invalid()), _context()))

    for refused in (unary_refusal.value, client_refusal.value, server_refusal.value, bidi_refusal.value):
        _assert_server_refusal(refused, Code.INVALID_ARGUMENT, 1)


@pytest.mark.anyio
async def test_server_response_refusals_cover_every_shape_without_internal_details() -> None:
    admission = BodyAdmission(AdmissionSide.SERVER)

    async def unary(_request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        return _invalid()

    async def client_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        await _drain(request)
        return _invalid()

    async def server_stream(_request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        yield _invalid()

    async def bidi_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        await _drain(request)
        yield _invalid()

    with pytest.raises(ConnectError) as unary_refusal:
        await admission.intercept_unary(unary, _valid(), _context())
    with pytest.raises(ConnectError) as client_refusal:
        await admission.intercept_client_stream(client_stream, _messages(_valid()), _context())
    with pytest.raises(ConnectError) as server_refusal:
        await _drain(admission.intercept_server_stream(server_stream, _valid(), _context()))
    with pytest.raises(ConnectError) as bidi_refusal:
        await _drain(admission.intercept_bidi_stream(bidi_stream, _messages(_valid()), _context()))

    for refused in (unary_refusal.value, client_refusal.value, server_refusal.value, bidi_refusal.value):
        _assert_server_refusal(refused, Code.INTERNAL, 0)


# --- [CLIENT_REFUSALS]


@pytest.mark.anyio
async def test_client_refusals_retain_typed_request_and_response_evidence_for_every_shape() -> None:
    admission = BodyAdmission(AdmissionSide.CLIENT)

    async def unary(request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        return request

    async def catching_client_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        try:
            return (await _drain(request))[0]
        except ConnectError:
            raise
        except Exception as cause:
            raise ConnectError(Code.UNAVAILABLE, "transport encode failed") from cause

    async def server_stream(request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        yield request

    async def catching_bidi_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        try:
            async for message in request:
                yield message
        except ConnectError:
            raise
        except Exception as cause:
            raise ConnectError(Code.UNAVAILABLE, "transport encode failed") from cause

    with pytest.raises(AdmissionError) as unary_request:
        await admission.intercept_unary(unary, _invalid(), _context())
    with pytest.raises(AdmissionError) as client_request:
        await admission.intercept_client_stream(catching_client_stream, _messages(_invalid()), _context())
    with pytest.raises(AdmissionError) as server_request:
        await _drain(admission.intercept_server_stream(server_stream, _invalid(), _context()))
    with pytest.raises(AdmissionError) as bidi_request:
        await _drain(admission.intercept_bidi_stream(catching_bidi_stream, _messages(_invalid()), _context()))

    async def invalid_unary(_request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        return _invalid()

    async def invalid_client_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> Hlc:
        await _drain(request)
        return _invalid()

    async def invalid_server_stream(_request: Hlc, _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        yield _invalid()

    async def invalid_bidi_stream(request: AsyncIterator[Hlc], _ctx: RequestContext[Hlc, Hlc]) -> AsyncIterator[Hlc]:
        await _drain(request)
        yield _invalid()

    with pytest.raises(AdmissionError) as unary_response:
        await admission.intercept_unary(invalid_unary, _valid(), _context())
    with pytest.raises(AdmissionError) as client_response:
        await admission.intercept_client_stream(invalid_client_stream, _messages(_valid()), _context())
    with pytest.raises(AdmissionError) as server_response:
        await _drain(admission.intercept_server_stream(invalid_server_stream, _valid(), _context()))
    with pytest.raises(AdmissionError) as bidi_response:
        await _drain(admission.intercept_bidi_stream(invalid_bidi_stream, _messages(_valid()), _context()))

    for refused in (unary_request.value, client_request.value, server_request.value, bidi_request.value):
        _assert_client_refusal(refused, AdmissionPhase.REQUEST)
    for refused in (unary_response.value, client_response.value, server_response.value, bidi_response.value):
        _assert_client_refusal(refused, AdmissionPhase.RESPONSE)


# --- [LIVE_TRANSPORT]


@pytest.mark.anyio
async def test_client_admission_survives_the_real_connect_transport_for_every_shape(socket_enabled: None, free_tcp_port: int) -> None:
    _ = socket_enabled
    config = Config()
    config.bind = [f"127.0.0.1:{free_tcp_port}"]
    config.accesslog = None
    config.errorlog = None
    stopped = anyio.Event()

    async with anyio.create_task_group() as tasks:
        # `hypercorn.typing.ASGIVersions` is `total=False` where the `asgiref.typing` twin that types
        # `ConnectASGIApplication.__call__` requires `spec_version`, so neither ASGI typing accepts the other's scope.
        served = tasks.start_soon(partial(serve, _Application(), config, shutdown_trigger=stopped.wait))  # ty: ignore[invalid-argument-type]
        with anyio.fail_after(2):
            while True:
                try:
                    stream = await anyio.connect_tcp("127.0.0.1", free_tcp_port)
                except OSError:
                    await anyio.sleep(0.01)
                else:
                    await stream.aclose()
                    break

        client = ConnectClient(f"http://127.0.0.1:{free_tcp_port}", interceptors=(BodyAdmission(AdmissionSide.CLIENT),))
        try:
            with pytest.raises(AdmissionError) as unary_request:
                await client.execute_unary(request=_invalid(), method=_Application._METHODS["Unary"])
            with pytest.raises(AdmissionError) as client_request:
                await client.execute_client_stream(request=_messages(_invalid()), method=_Application._METHODS["ClientStream"])
            with pytest.raises(AdmissionError) as server_request:
                await _drain(client.execute_server_stream(request=_invalid(), method=_Application._METHODS["ServerStream"]))
            with pytest.raises(AdmissionError) as bidi_request:
                await _drain(client.execute_bidi_stream(request=_messages(_invalid()), method=_Application._METHODS["BidiStream"]))

            response_trigger = Hlc(physical=1, logical=1)
            with pytest.raises(AdmissionError) as unary_response:
                await client.execute_unary(request=response_trigger, method=_Application._METHODS["Unary"])
            with pytest.raises(AdmissionError) as client_response:
                await client.execute_client_stream(request=_messages(response_trigger), method=_Application._METHODS["ClientStream"])
            with pytest.raises(AdmissionError) as server_response:
                await _drain(client.execute_server_stream(request=response_trigger, method=_Application._METHODS["ServerStream"]))
            with pytest.raises(AdmissionError) as bidi_response:
                await _drain(client.execute_bidi_stream(request=_messages(response_trigger), method=_Application._METHODS["BidiStream"]))

            with pytest.raises(AdmissionError) as late_request:
                await client.execute_client_stream(request=_messages(_valid(), _invalid()), method=_Application._METHODS["ClientStream"])
            _assert_client_refusal(late_request.value, AdmissionPhase.REQUEST)

            with pytest.raises(ConnectError) as unavailable:
                await client.execute_client_stream(request=_messages(Hlc(physical=2)), method=_Application._METHODS["ClientStream"])
            assert unavailable.value.code is Code.UNAVAILABLE

            async def drive(request: AsyncIterator[Hlc]) -> Hlc | AdmissionError:
                try:
                    return await client.execute_client_stream(request=request, method=_Application._METHODS["ClientStream"])
                except AdmissionError as refused:
                    return refused

            async with anyio.create_task_group() as calls:
                refused_call = calls.start_soon(drive, _messages(_valid(), _invalid()))
                admitted_call = calls.start_soon(drive, _messages(_valid()))
            concurrent = refused_call.return_value
            assert isinstance(concurrent, AdmissionError), concurrent
            _assert_client_refusal(concurrent, AdmissionPhase.REQUEST)
            assert admitted_call.return_value == _valid()

            source_closed = anyio.Event()

            async def stalled() -> AsyncIterator[Hlc]:
                try:
                    yield _valid()
                    await anyio.sleep_forever()
                finally:
                    source_closed.set()

            with anyio.move_on_after(0.05) as canceled, pytest.raises(ConnectError) as cancellation:
                await client.execute_client_stream(request=stalled(), method=_Application._METHODS["ClientStream"])
            assert canceled.cancel_called
            assert cancellation.value.code is Code.CANCELED
            with anyio.fail_after(1):
                await source_closed.wait()
            assert await client.execute_unary(request=_valid(), method=_Application._METHODS["Unary"]) == _valid()
            assert served.status is anyio.TaskHandle.Status.PENDING
        finally:
            await client.close()
            stopped.set()

    for refused in (unary_request.value, client_request.value, server_request.value, bidi_request.value):
        _assert_client_refusal(refused, AdmissionPhase.REQUEST)
    for refused in (unary_response.value, client_response.value, server_response.value, bidi_response.value):
        _assert_client_refusal(refused, AdmissionPhase.RESPONSE)
